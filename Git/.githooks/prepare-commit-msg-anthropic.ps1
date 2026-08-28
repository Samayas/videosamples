param(
    [Parameter(Mandatory = $true, Position = 0)]
    [string] $MessageFile,

    [Parameter(Position = 1)]
    [string] $Source,

    [Parameter(Position = 2)]
    [string] $CommitSha,

    [string] $ApiKey = $env:ANTHROPIC_API_KEY,
    [string] $Model = 'claude-sonnet-4-5',
    [string] $ApiBaseUrl = 'https://api.anthropic.com',
    [string] $AnthropicVersion = '2023-06-01',
    [int] $MaxDiffCharacters = 24000,
    [int] $MaxTokens = 1500,
    [int] $MinimumManualMessageLength = 10,
    [int] $TimeoutSeconds = 120,
    [switch] $Diagnostics
)

$ErrorActionPreference = 'Stop'

function Get-AnthropicErrorMessage {
    param($ErrorObject)

    if ($null -eq $ErrorObject) {
        return 'Anthropic returned an empty error object.'
    }

    if ($ErrorObject.error.message) {
        return [string] $ErrorObject.error.message
    }

    if ($ErrorObject.message) {
        return [string] $ErrorObject.message
    }

    return ($ErrorObject | ConvertTo-Json -Depth 20 -Compress)
}

try {
    $script:DebugLog = Join-Path $env:TEMP 'prepare-commit-msg-anthropic-debug.json'

    # Never replace Git-generated messages for special commit types.
    if ($Source -in @('template', 'merge', 'squash', 'commit')) {
        exit 0
    }

    # Visual Studio supplies its Git Changes text-box value through -m/-F,
    # causing Source to be 'message'. A value of 10 characters or fewer is
    # considered a placeholder and is discarded. Longer text is appended after
    # the generated commit message as a developer note.
    $manualMessage = ''

    if ($Source -eq 'message' -and (Test-Path -LiteralPath $MessageFile)) {
        $existingMessage = (Get-Content -LiteralPath $MessageFile -Raw -ErrorAction Stop).Trim()

        if ($existingMessage.Length -gt $MinimumManualMessageLength) {
            $manualMessage = $existingMessage
        }
    }

    if ([string]::IsNullOrWhiteSpace($ApiKey)) {
        throw 'Anthropic API key is missing. Set the ANTHROPIC_API_KEY environment variable or pass -ApiKey.'
    }

    if (-not (git rev-parse --is-inside-work-tree 2>$null)) {
        throw 'The hook must run from inside a Git working tree.'
    }

    $stat = git diff --cached --stat
    $diff = git diff --cached --no-ext-diff --unified=3

    if ([string]::IsNullOrWhiteSpace($diff)) {
        exit 0
    }

    if ($diff.Length -gt $MaxDiffCharacters) {
        $diff = $diff.Substring(0, $MaxDiffCharacters) +
            "`n`n[Diff truncated after $MaxDiffCharacters characters. Describe only visible changes.]"
    }

    $prompt = @"
Write a precise Git commit message based exclusively on the staged diff below.

Required format:
- Start with a clear, specific one-line summary. Do not impose an artificial character limit, but avoid repetition.
- Add one blank line.
- Add 2 to 6 concise bullet points that describe the important changes.
- Mention affected components, observable behavior changes, configuration or migration implications, and tests only when the diff provides evidence.
- Use a Conventional Commit prefix such as feat(scope): or fix(scope): only when it fits naturally.
- Do not invent facts, ticket numbers, APIs, tests, or behavior.
- Return only the final commit message.
- Do not include analysis, scratch work, reasoning, or <think> tags.

Changed-files summary:
$stat

Staged diff:
$diff
"@

    $requestBody = @{
        model = $Model
        max_tokens = $MaxTokens
        temperature = 0.1
        system = 'You generate accurate Git commit messages using only the supplied staged diff. Follow the requested output format exactly.'
        messages = @(
            @{
                role = 'user'
                content = $prompt
            }
        )
    }

    $requestJson = $requestBody | ConvertTo-Json -Depth 10
    $headers = @{
        'x-api-key' = $ApiKey
        'anthropic-version' = $AnthropicVersion
    }

    if ($Diagnostics) {
        $requestJson | Set-Content -LiteralPath ($script:DebugLog -replace '\.json$', '-request.json') -Encoding utf8NoBOM
    }

    $response = Invoke-RestMethod `
        -Method Post `
        -Uri "$($ApiBaseUrl.TrimEnd('/'))/v1/messages" `
        -Headers $headers `
        -ContentType 'application/json' `
        -Body $requestJson `
        -TimeoutSec $TimeoutSeconds

    if ($Diagnostics) {
        $response | ConvertTo-Json -Depth 30 | Set-Content -LiteralPath $script:DebugLog -Encoding utf8NoBOM
    }

    if ($null -eq $response) {
        throw 'Anthropic returned no response object.'
    }

    if ($response.error) {
        throw "Anthropic API error: $(Get-AnthropicErrorMessage $response)"
    }

    $textBlocks = @($response.content | Where-Object { $_.type -eq 'text' })

    if ($textBlocks.Count -eq 0) {
        $rawResponse = $response | ConvertTo-Json -Depth 30 -Compress
        throw "Anthropic returned no text content blocks. Response: $rawResponse"
    }

    $generated = (($textBlocks | ForEach-Object { [string] $_.text }) -join "`n").Trim()

    if ([string]::IsNullOrWhiteSpace($generated)) {
        $rawResponse = $response | ConvertTo-Json -Depth 30 -Compress
        throw "Anthropic returned empty text content. Response: $rawResponse"
    }

    if ($response.stop_reason -eq 'max_tokens') {
        Write-Warning "Anthropic reached the MaxTokens limit ($MaxTokens); the generated commit message may be incomplete."
    }

    if ([string]::IsNullOrWhiteSpace($manualMessage)) {
        $generated | Set-Content -LiteralPath $MessageFile -Encoding utf8NoBOM
    }
    else {
        @"
$generated

Developer note:
$manualMessage
"@ | Set-Content -LiteralPath $MessageFile -Encoding utf8NoBOM
    }
}
catch {
    $exceptionDetails = $_ | Format-List * -Force | Out-String

    if ($Diagnostics) {
        $exceptionDetails | Set-Content -LiteralPath ($script:DebugLog -replace '\.json$', '-error.txt') -Encoding utf8NoBOM
        Write-Warning "AI commit-message generation skipped: $($_.Exception.Message)"
        Write-Warning "Diagnostics written to: $script:DebugLog"
    }
    else {
        # Commit generation is optional. An unavailable Anthropic API must not prevent Git commits.
        Write-Warning "AI commit-message generation skipped: $($_.Exception.Message)"
    }

    exit 0
}

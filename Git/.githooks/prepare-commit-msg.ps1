param(
    [Parameter(Mandatory = $true, Position = 0)]
    [string] $MessageFile,

    [Parameter(Position = 1)]
    [string] $Source,

    [Parameter(Position = 2)]
    [string] $CommitSha,

    [string] $Model = 'qwen3.8-27b@q5_k_xl',
    [string] $ApiBaseUrl = 'http://127.0.0.1:11434/v1',
    [int] $MaxDiffCharacters = 24000,
    [int] $MaxTokens = 2000,
    [int] $MinimumManualMessageLength = 10,
    [int] $TimeoutSeconds = 900,
    [switch] $Diagnostics
)

$ErrorActionPreference = 'Stop'

function Get-LmStudioErrorMessage {
    param($ErrorObject)

    if ($null -eq $ErrorObject) {
        return 'LM Studio returned an empty error object.'
    }

    if ($ErrorObject.message) {
        return [string] $ErrorObject.message
    }

    return ($ErrorObject | ConvertTo-Json -Depth 20 -Compress)
}

try {
    $script:DebugLog = Join-Path $env:TEMP 'prepare-commit-msg-lmstudio-debug.json'

    # Never replace Git-generated messages for special commit types.
    if ($Source -in @('template', 'merge', 'squash', 'commit')) {
        exit 0
    }

    # Visual Studio supplies the Git Changes text-box value through -m/-F,
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
        messages = @(
            @{
                role = 'system'
                content = 'You generate accurate Git commit messages using only the supplied staged diff. Follow the requested output format exactly.'
            },
            @{
                role = 'user'
                content = $prompt
            }
        )
        temperature = 0.1
        max_tokens = $MaxTokens
        stream = $false
    }

    $requestJson = $requestBody | ConvertTo-Json -Depth 10

    if ($Diagnostics) {
        $requestJson | Set-Content -LiteralPath ($script:DebugLog -replace '\.json$', '-request.json') -Encoding utf8NoBOM
    }

    $response = Invoke-RestMethod `
        -Method Post `
        -Uri "$ApiBaseUrl/chat/completions" `
        -ContentType 'application/json' `
        -Body $requestJson `
        -TimeoutSec $TimeoutSeconds

    if ($Diagnostics) {
        $response | ConvertTo-Json -Depth 30 | Set-Content -LiteralPath $script:DebugLog -Encoding utf8NoBOM
    }

    if ($null -eq $response) {
        throw 'LM Studio returned no response object.'
    }

    if ($response.error) {
        throw "LM Studio API error: $(Get-LmStudioErrorMessage $response.error)"
    }

    $choice = @($response.choices) | Select-Object -First 1

    if ($null -eq $choice) {
        $rawResponse = $response | ConvertTo-Json -Depth 30 -Compress
        throw "LM Studio returned no completion choices. Response: $rawResponse"
    }

    $message = $choice.message

    if ($null -eq $message) {
        $rawResponse = $response | ConvertTo-Json -Depth 30 -Compress
        throw "LM Studio returned a completion choice with no message. Response: $rawResponse"
    }

    $generated = [string] $message.content

    if ([string]::IsNullOrWhiteSpace($generated)) {
        foreach ($propertyName in @('text', 'output_text', 'response')) {
            $candidate = [string] $message.$propertyName
            if (-not [string]::IsNullOrWhiteSpace($candidate)) {
                $generated = $candidate
                break
            }
        }
    }

    if ([string]::IsNullOrWhiteSpace($generated)) {
        $reasoning = [string] $message.reasoning_content

        if ([string]::IsNullOrWhiteSpace($reasoning)) {
            $reasoning = [string] $message.reasoning
        }

        $rawResponse = $response | ConvertTo-Json -Depth 30 -Compress

        if (-not [string]::IsNullOrWhiteSpace($reasoning)) {
            throw "LM Studio returned reasoning but no final message content. Response: $rawResponse"
        }

        throw "LM Studio returned an empty completion message. Response: $rawResponse"
    }

    $generated = $generated.Trim()

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
        # Commit generation is optional. An unavailable local LLM must not prevent Git commits.
        Write-Warning "AI commit-message generation skipped: $($_.Exception.Message)"
    }

    exit 0
}

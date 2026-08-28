param(
    [Parameter(Mandatory = $true, Position = 0)]
    [string] $MessageFile,

    [Parameter(Position = 1)]
    [string] $Source,

    [Parameter(Position = 2)]
    [string] $CommitSha,

    [string] $CopilotCommand = 'copilot',
    [string] $CopilotModel = 'auto',
    [int] $MaxDiffCharacters = 24000,
    [int] $MinimumManualMessageLength = 10,
    [switch] $Diagnostics
)

$ErrorActionPreference = 'Stop'

try {
    $script:DebugLog = Join-Path $env:TEMP 'prepare-commit-msg-copilot-debug.txt'

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

    if (-not (git rev-parse --is-inside-work-tree 2>$null)) {
        throw 'The hook must run from inside a Git working tree.'
    }

    $copilot = Get-Command $CopilotCommand -CommandType Application -ErrorAction SilentlyContinue

    if ($null -eq $copilot) {
        throw "GitHub Copilot CLI command '$CopilotCommand' was not found on PATH. Install it and run 'copilot login' once in an interactive terminal."
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
You are generating a Git commit message only.

Return only the final commit message. Do not include analysis, reasoning, explanations, Markdown fences, or <think> tags. Do not use tools, run commands, inspect files, edit files, stage files, commit, push, or perform any action.

Write a precise Git commit message based exclusively on the staged diff below.

Required format:
- Start with a clear, specific one-line summary. Do not impose an artificial character limit, but avoid repetition.
- Add one blank line.
- Add 2 to 6 concise bullet points that describe the important changes.
- Mention affected components, observable behavior changes, configuration or migration implications, and tests only when the diff provides evidence.
- Use a Conventional Commit prefix such as feat(scope): or fix(scope): only when it fits naturally.
- Do not invent facts, ticket numbers, APIs, tests, or behavior.

Changed-files summary:
$stat

Staged diff:
$diff
"@

    if ($Diagnostics) {
        @"
Timestamp: $(Get-Date -Format o)
Working directory: $(Get-Location)
Message file: $MessageFile
Source: $Source
Commit SHA: $CommitSha
Copilot executable: $($copilot.Source)
Copilot model: $CopilotModel

Prompt:
$prompt

Copilot output:
"@ | Set-Content -LiteralPath $script:DebugLog -Encoding utf8NoBOM
    }

    # -p executes non-interactively. -s returns only Copilot's final response.
    # Disable the built-in GitHub MCP server so this hook remains prompt-in/text-out.
    $copilotOutput = & $copilot.Source `
        --no-banner `
        --no-experimental `
        --no-ask-user `
        --disable-builtin-mcps `
        --model $CopilotModel `
        -s `
        -p $prompt 2>&1

    $copilotExitCode = $LASTEXITCODE
    $generated = ($copilotOutput | Out-String).Trim()

    if ($Diagnostics) {
        $generated | Add-Content -LiteralPath $script:DebugLog -Encoding utf8NoBOM
    }

    if ($copilotExitCode -ne 0) {
        throw "GitHub Copilot CLI failed with exit code $copilotExitCode. Output: $generated"
    }

    if ([string]::IsNullOrWhiteSpace($generated)) {
        throw 'GitHub Copilot CLI returned an empty response.'
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
        $exceptionDetails | Set-Content -LiteralPath ($script:DebugLog -replace '\.txt$', '-error.txt') -Encoding utf8NoBOM
        Write-Warning "AI commit-message generation skipped: $($_.Exception.Message)"
        Write-Warning "Diagnostics written to: $script:DebugLog"
    }
    else {
        # Commit generation is optional. An unavailable Copilot CLI must not prevent Git commits.
        Write-Warning "AI commit-message generation skipped: $($_.Exception.Message)"
    }

    exit 0
}

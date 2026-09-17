Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Get-ClaudeDirectory {
    [CmdletBinding()]
    param()

    [string]$UserDirectory = [Environment]::GetFolderPath([Environment+SpecialFolder]::UserProfile)

    if ([string]::IsNullOrWhiteSpace($UserDirectory)) {
        throw "Could not determine the current user's profile directory."
    }

    [string]$ClaudeDirectory = Join-Path -Path $UserDirectory -ChildPath ".claude"

    if (-not (Test-Path -LiteralPath $ClaudeDirectory -PathType Container)) {
        New-Item -ItemType Directory -Path $ClaudeDirectory -Force | Out-Null
    }

    return $ClaudeDirectory
}

function Replace-WithSymbolicLink {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [ValidateNotNullOrEmpty()]
        [string]$SourcePath,

        [Parameter(Mandatory)]
        [ValidateNotNullOrEmpty()]
        [string]$DestinationPath
    )

    if (-not (Test-Path -LiteralPath $SourcePath -PathType Leaf)) {
        throw "The source file does not exist: '$SourcePath'."
    }

    [string]$ResolvedSourcePath = (Resolve-Path -LiteralPath $SourcePath).Path
    [string]$DestinationDirectory = Split-Path -Path $DestinationPath -Parent

    if (-not (Test-Path -LiteralPath $DestinationDirectory -PathType Container)) {
        New-Item -ItemType Directory -Path $DestinationDirectory -Force | Out-Null
    }

    if (Test-Path -LiteralPath $DestinationPath) {
        [System.IO.FileAttributes]$Attributes = [System.IO.File]::GetAttributes(
            $DestinationPath
        )

        if (($Attributes -band [System.IO.FileAttributes]::Directory) -ne 0) {
            throw "Destination exists as a directory: '$DestinationPath'."
        }

        Remove-Item -LiteralPath $DestinationPath -Force
        Write-Host "Removed:                $DestinationPath" -ForegroundColor Yellow
    }

    New-Item -ItemType SymbolicLink -Path $DestinationPath -Target $ResolvedSourcePath  -Force | Out-Null

    Write-Host "Symbolic link created: $DestinationPath" -ForegroundColor Green
}

[string]$RepositoryRoot = $PSScriptRoot

if ([string]::IsNullOrWhiteSpace($RepositoryRoot)) {
    throw "Could not determine the script directory. Save the script as a .ps1 file and run it from disk."
}

[string]$RepositoryRoot = (Resolve-Path -LiteralPath $RepositoryRoot).Path
[string]$ClaudeDirectory = Get-ClaudeDirectory

Write-Host "Repository: $RepositoryRoot" -ForegroundColor Cyan
Write-Host "Claude dir:  $ClaudeDirectory" -ForegroundColor Cyan

# ============================================================================
# Explicit hard links only.
# Add one Replace-WithHardLink call per file you deliberately want to share.
# ============================================================================

Replace-WithSymbolicLink -SourcePath (Join-Path -Path $RepositoryRoot -ChildPath "CLAUDE.md") -DestinationPath (Join-Path -Path $ClaudeDirectory -ChildPath "CLAUDE.md")
Replace-WithSymbolicLink -SourcePath (Join-Path -Path $RepositoryRoot -ChildPath "settings.json") -DestinationPath (Join-Path -Path $ClaudeDirectory -ChildPath "settings.json")
Replace-WithSymbolicLink -SourcePath (Join-Path -Path $RepositoryRoot -ChildPath "rules\csharp-style.md") -DestinationPath (Join-Path -Path $ClaudeDirectory -ChildPath "rules\csharp-style.md")
Replace-WithSymbolicLink -SourcePath (Join-Path -Path $RepositoryRoot -ChildPath "rules\csharp-unit-tests.md") -DestinationPath (Join-Path -Path $ClaudeDirectory -ChildPath "rules\csharp-unit-tests.md")
Replace-WithSymbolicLink -SourcePath (Join-Path -Path $RepositoryRoot -ChildPath "rules\csproj-testproject.md") -DestinationPath (Join-Path -Path $ClaudeDirectory -ChildPath "rules\csproj-testproject.md")
Replace-WithSymbolicLink -SourcePath (Join-Path -Path $RepositoryRoot -ChildPath "rules\csproj-webproject.md") -DestinationPath (Join-Path -Path $ClaudeDirectory -ChildPath "rules\csproj-webproject.md")
Replace-WithSymbolicLink -SourcePath (Join-Path -Path $RepositoryRoot -ChildPath "rules\css-style.md") -DestinationPath (Join-Path -Path $ClaudeDirectory -ChildPath "rules\css-style.md")
Replace-WithSymbolicLink -SourcePath (Join-Path -Path $RepositoryRoot -ChildPath "rules\js-style.md") -DestinationPath (Join-Path -Path $ClaudeDirectory -ChildPath "rules\js-style.md")
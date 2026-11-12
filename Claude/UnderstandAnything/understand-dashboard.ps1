<#
.SYNOPSIS
    Starts the Understand Anything dashboard for this repository.

.DESCRIPTION
    Locates the understand-anything plugin in the current user's Claude Code
    plugin cache (any user, any installed version), then starts the dashboard
    against this repo's knowledge graph (.understand-anything\knowledge-graph.json
    when present, otherwise .ua\knowledge-graph.json).

    Tries the prebuilt release viewer first (no install, no build). If that
    release asset is unavailable, falls back to the local Vite dev server,
    installing and building only what is missing.

.EXAMPLE
    .\understand-dashboard.ps1
#>

[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

$ProjectDir = $PSScriptRoot

function Write-Step([string]$Message) {
    Write-Host "[understand-dashboard] $Message"
}

# --- Locate the knowledge graph --------------------------------------------
$UaDirName = if (Test-Path (Join-Path $ProjectDir '.understand-anything')) { '.understand-anything' } else { '.ua' }
$GraphPath = Join-Path (Join-Path $ProjectDir $UaDirName) 'knowledge-graph.json'
if (-not (Test-Path $GraphPath)) {
    Write-Error "No knowledge graph found at '$GraphPath'. Run /understand first to analyze this project."
    exit 1
}

# --- Locate the plugin root (user-agnostic, highest installed version) ------
$PluginRoot = $null
$CacheRoot = Join-Path $env:USERPROFILE '.claude\plugins\cache\understand-anything\understand-anything'
if (Test-Path $CacheRoot) {
    $PluginRoot = Get-ChildItem $CacheRoot -Directory |
        Where-Object { Test-Path (Join-Path $_.FullName 'packages\dashboard') } |
        Sort-Object { if ($_.Name -match '^\d+(\.\d+)+$') { [version]$_.Name } else { [version]'0.0' } } -Descending |
        Select-Object -First 1 -ExpandProperty FullName
}
if (-not $PluginRoot) {
    $Fallback = Join-Path $env:USERPROFILE '.understand-anything-plugin'
    if (Test-Path (Join-Path $Fallback 'packages\dashboard')) { $PluginRoot = $Fallback }
}
if (-not $PluginRoot) {
    Write-Error "Cannot find the understand-anything plugin under '$CacheRoot'. Install the plugin first (it is cached per-user under .claude\plugins\cache)."
    exit 1
}

if (-not (Get-Command npx -ErrorAction SilentlyContinue)) {
    Write-Error 'npx (Node.js) is required but was not found on PATH.'
    exit 1
}

$Version = (Get-Content (Join-Path $PluginRoot 'package.json') -Raw | ConvertFrom-Json).version
$DashboardDir = Join-Path $PluginRoot 'packages\dashboard'

Write-Step "Project: $ProjectDir"
Write-Step "Graph:   $GraphPath"
Write-Step "Plugin:  $PluginRoot (v$Version)"

# --- Fast path: prebuilt release viewer (no install, no build) --------------
# The viewer tarball is only published for some releases (e.g. v2.9.0, not
# v2.9.7), so probe the pinned URL before invoking npx to avoid npm 404 noise.
$ViewerUrl = "https://github.com/Egonex-AI/Understand-Anything/releases/download/v$Version/understand-anything-viewer.tgz"
$ViewerAvailable = $false
try {
    $ViewerProbe = Invoke-WebRequest -Uri $ViewerUrl -Method Head -UseBasicParsing -TimeoutSec 10
    $ViewerAvailable = $ViewerProbe.StatusCode -eq 200
}
catch { }
if ($ViewerAvailable) {
    $ViewerLog = Join-Path $env:TEMP "understand-dashboard-viewer-$PID.log"
    Write-Step 'Trying prebuilt release viewer...'
    npx --yes $ViewerUrl $ProjectDir 2>&1 | Tee-Object -FilePath $ViewerLog
    $ViewerStarted = Select-String -Path $ViewerLog -Pattern 'Dashboard URL' -Quiet
    Remove-Item $ViewerLog -ErrorAction SilentlyContinue
    if ($ViewerStarted) {
        exit $LASTEXITCODE
    }
    Write-Step 'Prebuilt viewer failed to start; falling back to the local dev server.'
}
else {
    Write-Step "No prebuilt viewer asset published for v$Version; using the local dev server."
}

# --- Fallback: install/build only what is missing ----------------------------
if (-not (Test-Path (Join-Path $DashboardDir 'node_modules'))) {
    Write-Step 'Installing dashboard dependencies (first run only)...'
    Push-Location $DashboardDir
    try {
        npx --yes pnpm install
        if ($LASTEXITCODE -ne 0) { throw "pnpm install failed with exit code $LASTEXITCODE." }
    }
    finally { Pop-Location }
}
if (-not (Test-Path (Join-Path $PluginRoot 'packages\core\dist'))) {
    Write-Step 'Building @understand-anything/core (first run only)...'
    Push-Location $PluginRoot
    try {
        npx --yes pnpm --filter '@understand-anything/core' build
        if ($LASTEXITCODE -ne 0) { throw "core build failed with exit code $LASTEXITCODE." }
    }
    finally { Pop-Location }
}

# --- Start the Vite dev server -----------------------------------------------
Write-Step 'Starting Vite dev server (Ctrl+C to stop)...'
$env:GRAPH_DIR = $ProjectDir
Push-Location $DashboardDir
try { npx vite --host 127.0.0.1 }
finally { Pop-Location }

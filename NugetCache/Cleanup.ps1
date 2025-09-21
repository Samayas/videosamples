param(
    [Parameter(Mandatory = $false)]
    [string]$PackagesRoot = ""
)

# If no argument is provided, print usage and exit.
if (-not $PackagesRoot -or $PackagesRoot -eq "") {
    Write-Host "Usage: powershell.exe -File CleanNugetVersions.ps1 -PackagesRoot 'C:\Path\To\.nuget\packages'"
    exit 1
}


$allPreviousPaths = @()
$exclusionList = @(".tools")

Write-Host "`nAll Nuget packagers and versions:"

Get-ChildItem -Path $packagesRoot -Directory |
    Where-Object { $exclusionList -notcontains $_.Name } |
    ForEach-Object {
        $packageDir = $_.FullName
        $packageName = $_.Name
        $versionFolders = Get-ChildItem -Path $packageDir -Directory | Where-Object {
            $_.Name -match '^\d+\.\d+\.\d+$'
        }
        if ($versionFolders.Count -gt 0) {
            $versions = $versionFolders | ForEach-Object {
                [PSCustomObject]@{
                    Folder = $_.FullName
                    Version = [Version]$_.Name
                    Name = $_.Name
                }
            } | Sort-Object Version
            $latest = $versions[-1]
            $previous = $versions[0..($versions.Count-2)]
            $prevNames = $previous | ForEach-Object { $_.Name }
            if ($prevNames.Count -gt 0) {
                $prevList = $prevNames -join ","
                Write-Host "Package [$packageName] [$($latest.Name)] -> Previous Versions [$prevList]"
            } else {
                Write-Host "Package [$packageName] [$($latest.Name)] -> No Previous Version"
            }

            $allPreviousPaths += ($previous | ForEach-Object { $_.Folder })
        }
    }

Write-Host "`nAll previous version folder paths:"
foreach ($path in $allPreviousPaths) {
    Write-Host "Deleting $path"
    Remove-Item -Path $path -Recurse -Force
}
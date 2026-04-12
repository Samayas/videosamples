Install-Module Microsoft.Graph -Scope CurrentUser -Force
Connect-MgGraph -Scopes "Sites.FullControl.All"
$site = Get-MgSite -SiteId "samayas.sharepoint.com:/sites/UploadFiles:"
Write-Host "Site ID : $($site.Id)"
Write-Host "Site Name: $($site.DisplayName)"

$appPermission = @{
    roles = @("write")
    grantedToIdentities = @(
        @{
            application = @{
                id          = "b5d7e24b-203c-4c5b-8946-7783830dfa37"
                displayName = "UploadFiles"
            }
        }
    )
}

$permission = New-MgSitePermission -SiteId $site.Id -BodyParameter $appPermission
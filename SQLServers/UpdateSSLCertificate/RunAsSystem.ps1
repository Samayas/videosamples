# Remove existing task if it exists from a previous failed run
$existingTask = Get-ScheduledTask -TaskName "CreateMachineCertificate" -ErrorAction SilentlyContinue
if ($existingTask) {
    Write-Host "Removing existing task from previous run..."
    Unregister-ScheduledTask -TaskName "CreateMachineCertificate" -Confirm:$false
}

Write-Host "Creating Scheduled Script for run..."
$action = New-ScheduledTaskAction -Execute 'powershell.exe' -Argument '-NoProfile -ExecutionPolicy Bypass -File "D:\Projects\Samayas\Infrastructure\SQLServers\UpdateSSLCertificate\CreateDomainCertificate.ps1"'
$trigger = New-ScheduledTaskTrigger -Once -At (Get-Date).AddMinutes(1)
$principal = New-ScheduledTaskPrincipal -UserId "NT AUTHORITY\SYSTEM" -LogonType ServiceAccount -RunLevel Highest
$settings = New-ScheduledTaskSettingsSet -AllowStartIfOnBatteries -DontStopIfGoingOnBatteries

Register-ScheduledTask -TaskName "CreateMachineCertificate" -Action $action -Trigger $trigger -Principal $principal -Settings $settings

# Wait for task to complete
Start-Sleep -Seconds 65

# Remove the task
Unregister-ScheduledTask -TaskName "CreateMachineCertificate" -Confirm:$false

Write-Host "Calling Grand Permission..."
& "D:\Projects\Samayas\Infrastructure\SQLServers\UpdateSSLCertificate\GrandReadAccessToDomainCertificate.ps1"

Write-Host "Calling Assign Certificate to SQL..."
& "D:\Projects\Samayas\Infrastructure\SQLServers\UpdateSSLCertificate\AssignDomainCertificateToSQLConfiguration.ps1"
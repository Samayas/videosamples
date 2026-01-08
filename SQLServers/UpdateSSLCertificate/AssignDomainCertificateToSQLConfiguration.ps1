Write-Host "Configuring SQL Server to use Machine certificate"

# SQL Server instance name (change if using different instance)
$instanceName = "SQLEXPRESS"
Write-Host "Target SQL Server Instance: $instanceName"

# Get the computer's FQDN
$hostname = [System.Net.Dns]::GetHostByName($env:COMPUTERNAME).HostName
Write-Host "Detected FQDN: $hostname"

function Get-CertificateTemplate {
    param([System.Security.Cryptography.X509Certificates.X509Certificate2]$Certificate)
    
    $temp = $Certificate.Extensions | Where-Object {$_.Oid.Value -eq "1.3.6.1.4.1.311.21.7"}
    if (!$temp) {
        $temp = $Certificate.Extensions | Where-Object {$_.Oid.Value -eq "1.3.6.1.4.1.311.20.2"}
    }
    
    if($temp) {
        $templateInfo = $temp.Format(0)
        if ($templateInfo -match "Template=([^(]+)") {
            return $matches[1].Trim()
        }
        return $templateInfo
    }
    else {
        return "Unknown"
    }
}

function Get-SqlServerInstanceId {
    param([string]$InstanceName)
    
    try {
        # Get the instance ID from registry
        $regPath = "HKLM:\SOFTWARE\Microsoft\Microsoft SQL Server\Instance Names\SQL"
        $instanceId = Get-ItemProperty -Path $regPath -Name $InstanceName -ErrorAction Stop
        return $instanceId.$InstanceName
    }
    catch {
        Write-Host "ERROR: Cannot find SQL Server instance '$InstanceName' in registry" -ForegroundColor Red
        Write-Host "Registry path: $regPath" -ForegroundColor Red
        return $null
    }
}

function Set-SqlServerCertificate {
    param(
        [string]$InstanceId,
        [string]$Thumbprint
    )
    
    try {
        # Registry path for SQL Server network configuration
        $regPath = "HKLM:\SOFTWARE\Microsoft\Microsoft SQL Server\$InstanceId\MSSQLServer\SuperSocketNetLib"
        
        Write-Host "Setting certificate in registry path: $regPath"
        
        # Clean thumbprint: LOWERCASE and no spaces (SQL Server Configuration Manager requires lowercase)
        $cleanThumbprint = $Thumbprint.ToLower().Replace(" ", "")
        
        Write-Host "Setting certificate thumbprint: $cleanThumbprint"
        
        # Set the certificate thumbprint
        Set-ItemProperty -Path $regPath -Name "Certificate" -Value $cleanThumbprint -ErrorAction Stop
        
        Write-Host "Successfully set certificate in SQL Server configuration" -ForegroundColor Green
        return $true
    }
    catch {
        Write-Host "ERROR: Failed to set certificate in registry - $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

function Restart-SqlServerService {
    param([string]$InstanceName)
    
    try {
        if ($InstanceName -eq "MSSQLSERVER") {
            $serviceName = "MSSQLSERVER"
        }
        else {
            $serviceName = "MSSQL`$$InstanceName"
        }
        
        Write-Host "Restarting SQL Server service: $serviceName"
        
        # Check if service exists
        $service = Get-Service -Name $serviceName -ErrorAction Stop
        
        if ($service.Status -eq "Running") {
            Write-Host "Stopping SQL Server service..."
            Stop-Service -Name $serviceName -Force -ErrorAction Stop
            
            # Wait for service to stop
            $service.WaitForStatus("Stopped", [TimeSpan]::FromSeconds(30))
            Write-Host "Service stopped successfully"
        }
        
        Write-Host "Starting SQL Server service..."
        Start-Service -Name $serviceName -ErrorAction Stop
        
        # Wait for service to start
        $service.WaitForStatus("Running", [TimeSpan]::FromSeconds(30))
        Write-Host "Service started successfully" -ForegroundColor Green
        
        return $true
    }
    catch {
        Write-Host "ERROR: Failed to restart SQL Server service - $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

# Find all Machine template certificates for this hostname
$certificates = @(Get-ChildItem cert:\LocalMachine\My | Where-Object { 
    $_.Subject -eq "CN=$hostname" -and (Get-CertificateTemplate $_) -eq "Machine"
})

if ($certificates.Count -eq 0) {
    Write-Host "ERROR: No Machine template certificate found for $hostname" -ForegroundColor Red
    Exit 1
}

# Always select the most recent certificate (sorted by NotBefore descending)
Write-Host "Found $($certificates.Count) Machine certificate(s) for $hostname"
$certificate = $certificates | Sort-Object NotBefore -Descending | Select-Object -First 1

Write-Host "Using most recent certificate:"
Write-Host "  Subject: $($certificate.Subject)"
Write-Host "  Thumbprint: $($certificate.Thumbprint)"
Write-Host "  Issued: $($certificate.NotBefore)"
Write-Host "  Expires: $($certificate.NotAfter)"
Write-Host ""

# Get SQL Server instance ID
$instanceId = Get-SqlServerInstanceId -InstanceName $instanceName
if ($null -eq $instanceId) {
    Exit 1
}

Write-Host "SQL Server Instance ID: $instanceId"
Write-Host ""

# Set the certificate in SQL Server configuration
$result = Set-SqlServerCertificate -InstanceId $instanceId -Thumbprint $certificate.Thumbprint
if (-not $result) {
    Exit 1
}

Write-Host ""

# Restart SQL Server service
$result = Restart-SqlServerService -InstanceName $instanceName
if (-not $result) {
    Exit 1
}

Write-Host ""
Write-Host "SQL Server has been configured to use the certificate!" -ForegroundColor Green
Write-Host "Certificate Thumbprint: $($certificate.Thumbprint.ToLower())" -ForegroundColor Green

Exit 0

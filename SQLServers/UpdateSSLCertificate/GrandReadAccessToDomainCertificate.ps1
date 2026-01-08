Write-Host "Granting certificate private key permissions to SQL Server service account"

# Get the computer's FQDN
$hostname = [System.Net.Dns]::GetHostByName($env:COMPUTERNAME).HostName
Write-Host "Detected FQDN: $hostname"

# SQL Server service account name
$serviceAccount = "NT SERVICE\MSSQL`$SQLEXPRESS"
Write-Host "Service Account: $serviceAccount"

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

function Grant-CertificatePrivateKeyPermission {
    param(
        [System.Security.Cryptography.X509Certificates.X509Certificate2]$Certificate,
        [string]$AccountName
    )
    
    Write-Host "Granting read permission on certificate private key"
    Write-Host "Certificate Subject: $($Certificate.Subject)"
    Write-Host "Certificate Thumbprint: $($Certificate.Thumbprint)"
    
    try {
        # Get the RSA private key
        $rsaCert = [System.Security.Cryptography.X509Certificates.RSACertificateExtensions]::GetRSAPrivateKey($Certificate)
        
        if ($null -eq $rsaCert) {
            Write-Host "ERROR: Certificate does not have a private key" -ForegroundColor Red
            return $false
        }
        
        # Get the unique key file name
        $keyFileName = $rsaCert.Key.UniqueName
        $keyPath = "$env:ALLUSERSPROFILE\Microsoft\Crypto\Keys\$keyFileName"
        
        Write-Host "Private key file: $keyPath"
        
        if (-not (Test-Path $keyPath)) {
            # Try RSA\MachineKeys for older certificates
            $keyPath = "$env:ALLUSERSPROFILE\Microsoft\Crypto\RSA\MachineKeys\$keyFileName"
            Write-Host "Trying alternate path: $keyPath"
        }
        
        if (-not (Test-Path $keyPath)) {
            Write-Host "ERROR: Private key file not found" -ForegroundColor Red
            return $false
        }
        
        # Get current ACL
        $acl = Get-Acl -Path $keyPath
        
        # Create access rule for the service account
        $accessRule = New-Object System.Security.AccessControl.FileSystemAccessRule(
            $AccountName,
            "Read",
            "Allow"
        )
        
        # Add the access rule
        $acl.AddAccessRule($accessRule)
        
        # Apply the new ACL
        Set-Acl -Path $keyPath -AclObject $acl
        
        Write-Host "Successfully granted Read permission to $AccountName" -ForegroundColor Green
        return $true
    }
    catch {
        Write-Host "ERROR: Failed to grant permissions - $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

# Find the Machine template certificate
$certificates = @(Get-ChildItem cert:\LocalMachine\My | Where-Object { 
    $_.Subject -eq "CN=$hostname" -and (Get-CertificateTemplate $_) -eq "Machine"
})

if ($certificates.Count -eq 0) {
    Write-Host "ERROR: No Machine template certificate found for $hostname" -ForegroundColor Red
    Exit 1
}

# If multiple certificates exist, use the newest one
if ($certificates.Count -gt 1) {
    Write-Host "Found $($certificates.Count) Machine certificates, using the newest one"
    $certificate = $certificates | Sort-Object NotBefore -Descending | Select-Object -First 1
}
else {
    $certificate = $certificates[0]
}

Write-Host "Found certificate:"
Write-Host "  Thumbprint: $($certificate.Thumbprint)"
Write-Host "  Issued: $($certificate.NotBefore)"
Write-Host "  Expires: $($certificate.NotAfter)"
Write-Host ""

# Grant permissions
$result = Grant-CertificatePrivateKeyPermission -Certificate $certificate -AccountName $serviceAccount

if ($result) {
    Write-Host "`nPermissions granted successfully!" -ForegroundColor Green
    Exit 0
}
else {
    Write-Host "`nFailed to grant permissions" -ForegroundColor Red
    Exit 1
}

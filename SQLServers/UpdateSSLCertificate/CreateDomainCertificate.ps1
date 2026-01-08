Write-Host "Creating Computer Certificate on $env:COMPUTERNAME"

# Get the computer's FQDN
$hostname = [System.Net.Dns]::GetHostByName($env:COMPUTERNAME).HostName
Write-Host "Detected FQDN: $hostname"

function New-ComputerCertificateRequest {
    param ( [string]$hostname )
    
    $CATemplate = "Machine"
    $CertificateINI = "cert.ini"
    $CertificateREQ = "cert.req"
    $CertificateRSP = "cert.rsp"
    $CertificateCER = "cert.cer"
    $Subject = 'Subject="CN=' + $hostname + '"'
    $SAN = '_continue_ = "dns=' + $hostname + '&"'

    ### INI file generation
    new-item -type file $CertificateINI -force
    add-content $CertificateINI '[Version]'
    add-content $CertificateINI 'Signature="$Windows NT$"'
    add-content $CertificateINI ''
    add-content $CertificateINI '[NewRequest]'
    add-content $CertificateINI $Subject
    add-content $CertificateINI 'Exportable=TRUE'
    add-content $CertificateINI 'KeyLength=2048'
    add-content $CertificateINI 'KeySpec=1'
    add-content $CertificateINI 'KeyUsage=0xA0'
    add-content $CertificateINI 'MachineKeySet=True'
    add-content $CertificateINI 'ProviderName="Microsoft RSA SChannel Cryptographic Provider"'
    add-content $CertificateINI 'ProviderType=12'
    add-content $CertificateINI 'SMIME=FALSE'
    add-content $CertificateINI 'RequestType=PKCS10'
    add-content $CertificateINI '[Strings]'
    add-content $CertificateINI 'szOID_SUBJECT_ALT_NAME2 = "2.5.29.17"'
    add-content $CertificateINI '[Extensions]'
    add-content $CertificateINI '2.5.29.17 = "{text}"'
    add-content $CertificateINI $SAN
  
    ### Certificate request generation
    if (test-path $CertificateREQ) {
        del $CertificateREQ
    }
    certreq -new $CertificateINI $CertificateREQ
  
    ### Online certificate request and import
    if ($OnlineCA) {
        if (test-path $CertificateCER) {
            del $CertificateCER
        }
        if (test-path $CertificateRSP) {
            del $CertificateRSP
        }
        certreq -submit -attrib "CertificateTemplate:$CATemplate" -config $OnlineCA $CertificateREQ $CertificateCER
        certreq -accept $CertificateCER
    }
    
    ### Delete certificate request files
    if (test-path $CertificateINI) {
        del $CertificateINI
    }
    if (test-path $CertificateREQ) {
        del $CertificateREQ
    }
    if (test-path $CertificateRSP) {
        del $CertificateRSP
    }
    if (test-path $CertificateCER) {
        del $CertificateCER
    }
}

function Remove-ExpiredCertificate {
    param ( 
        [string]$hostname,
        [System.Security.Cryptography.X509Certificates.X509Certificate2]$certificate
    )
    
    Write-Host "Removing expired certificate $hostname with thumbprint $($certificate.Thumbprint)"
    
    # Remove from certificate store
    $certificateStore = New-Object System.Security.Cryptography.X509Certificates.X509Store("My", "LocalMachine")
    $certificateStore.Open("ReadWrite")
    $certificateStore.Remove($certificate)
    $certificateStore.Close()
    
    Write-Host "Expired certificate $hostname removed successfully"
}

function Get-CertificateTemplate {
    param([System.Security.Cryptography.X509Certificates.X509Certificate2]$Certificate)
    
    $temp = $Certificate.Extensions | Where-Object {$_.Oid.Value -eq "1.3.6.1.4.1.311.21.7"}
    if (!$temp) {
        $temp = $Certificate.Extensions | Where-Object {$_.Oid.Value -eq "1.3.6.1.4.1.311.20.2"}
    }
    
    if($temp) {
        $templateInfo = $temp.Format(0)
        # Extract template name from format like "Template=Machine(1.3.6.1.4.1.311.21.8.xxxx)"
        if ($templateInfo -match "Template=([^(]+)") {
            return $matches[1].Trim()
        }
        return $templateInfo
    }
    else {
        return "Unknown"
    }
}

# Check if a CA exists in the domain
Write-Host "Querying Cert Util Settings"
if (@(certutil -dump | select-string "Config:")) {
    $OnlineCA = (certutil -dump | select-string "Config:").Line.replace("``",'"').replace("'",'"').split('"')[1]
    Write-Host "Online Certification Server $OnlineCA"
} else {
    Write-Host "Unable to determine certificate authority (CA) for this domain"
    Exit
}

# Check if a Machine template certificate already exists
$existingCertificates = @(Get-ChildItem cert:\LocalMachine\My | Where-Object { 
    $_.Subject -eq "CN=$hostname" -and (Get-CertificateTemplate $_) -eq "Machine"
})

if ($existingCertificates.Count -ne 0) {
    Write-Host "Found $($existingCertificates.Count) existing Machine template certificate(s) for $hostname"
    
    $needsNewCertificate = $false
    $currentDate = Get-Date
    
    foreach ($certificate in $existingCertificates) {
        $templateName = Get-CertificateTemplate $certificate
        $expiryDate = $certificate.NotAfter
        $daysUntilExpiry = ($expiryDate - $currentDate).Days
        
        Write-Host "Certificate Template: $templateName"
        Write-Host "Certificate Thumbprint: $($certificate.Thumbprint)"
        Write-Host "Certificate Issued: $($certificate.NotBefore)"
        Write-Host "Certificate Expires: $expiryDate"
        Write-Host "Days Until Expiry: $daysUntilExpiry"
        
        # Check if certificate is expired or will expire within 35 days
        if ($currentDate -gt $expiryDate) {
            Write-Host "Certificate is EXPIRED - will be replaced"
            $needsNewCertificate = $true
            Remove-ExpiredCertificate -hostname $hostname -certificate $certificate
        }
        elseif ($daysUntilExpiry -le 35) {
            Write-Host "Certificate expires within 35 days - will be replaced"
            $needsNewCertificate = $true
            Remove-ExpiredCertificate -hostname $hostname -certificate $certificate
        }
        else {
            Write-Host "Certificate is still valid for $daysUntilExpiry days - no action needed"
        }
    }
    
    if ($needsNewCertificate) {
        Write-Host "Creating new Machine certificate for $hostname"
        New-ComputerCertificateRequest -hostname $hostname > $null
        Write-Host "Created a new Machine certificate for $hostname"
    }
    else {
        Write-Host "Existing Machine certificate is valid - no new certificate created"
    }
}
else {
    Write-Host "No existing Machine template certificate found for $hostname - creating new certificate"
    New-ComputerCertificateRequest -hostname $hostname > $null
    Write-Host "Created a new Machine certificate for $hostname"
}

Exit

# Rename Computer
Rename-Computer -NewName "SRV-APP-01" -Restart -Force

# Rename Computer with Domain Credentials
Rename-Computer -NewName "SRV-APP-01" -DomainCredential (Get-Credential) -Restart -Force

# Add to Domain
Add-Computer -DomainName "yourdomain.com" -Credential (Get-Credential) -Restart

# Add to Domain and rename
Add-Computer -DomainName "yourdomain.com" -NewName "SRV-APP-01" -Credential (Get-Credential) -Restart -Force

# Set Network Details
$adapter = Get-NetAdapter | Where-Object {$_.Status -eq "Up"}; $adapter | Remove-NetIPAddress -AddressFamily IPv4 -Confirm:$false; $adapter | Remove-NetRoute -AddressFamily IPv4 -Confirm:$false; $adapter | New-NetIPAddress -IPAddress "192.168.1.100" -PrefixLength 24 -DefaultGateway "192.168.1.1"; $adapter | Set-DnsClientServerAddress -ServerAddresses ("8.8.8.8","8.8.4.4")

# Enable Mstsc
Set-ItemProperty -Path 'HKLM:\System\CurrentControlSet\Control\Terminal Server' -Name "fDenyTSConnections" -Value 0
Enable-NetFirewallRule -DisplayGroup "Remote Desktop"

# Change Regional Settings
Set-TimeZone -Id "Central European Standard Time"
Set-Culture -CultureInfo "fr-LU"
Set-WinHomeLocation -GeoId 147
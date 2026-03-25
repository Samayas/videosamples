Set-Service -Name "MSiSCSI" -StartupType Automatic -ErrorAction Stop
Start-Service -Name "MSiSCSI" -ErrorAction Stop
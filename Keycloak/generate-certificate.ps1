#elevate if required
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)

if (-not $isAdmin) {
    Write-Host "Not running as Administrator. Restarting with elevated privileges..." -ForegroundColor Yellow
    $scriptPath = $MyInvocation.MyCommand.Path
    Start-Process powershell.exe -ArgumentList "-NoProfile -ExecutionPolicy Bypass -File `"$scriptPath`"" -Verb RunAs
    exit
}

$hostname = "localhost"
$certPassword = "password"
$validityDays = 365
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $scriptPath
New-Item -ItemType Directory -Force -Path "certs" | Out-Null

Write-Host "Generating self-signed certificate for $hostname..." -ForegroundColor Cyan
$cert = New-SelfSignedCertificate `
    -DnsName $hostname, "localhost" `
    -CertStoreLocation "Cert:\LocalMachine\My" `
    -NotAfter (Get-Date).AddDays($validityDays) `
    -KeyAlgorithm RSA `
    -KeyLength 2048 `
    -HashAlgorithm SHA256 `
    -KeyUsage DigitalSignature, KeyEncipherment `
    -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.1") `
    -FriendlyName "Keycloak SSL Certificate"

Write-Host "Certificate created with thumbprint: $($cert.Thumbprint)" -ForegroundColor Green
$certThumbprint = $cert.Thumbprint

#export as pfx
$certPath = "certs\keycloak.pfx"
$securePassword = ConvertTo-SecureString -String $certPassword -Force -AsPlainText
Export-PfxCertificate -Cert $cert -FilePath $certPath -Password $securePassword | Out-Null
Write-Host "Exported certificate to: $certPath" -ForegroundColor Green

#export public key for trust purposes
$cerPath = "certs\keycloak.public.cer"
Export-Certificate -Cert $cert -FilePath $cerPath | Out-Null
Write-Host "Exported public certificate to: $cerPath" -ForegroundColor Green

Write-Host "`nInstalling certificate to Trusted Root Certification Authorities..." -ForegroundColor Cyan
try {
    $publicCert = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2($cerPath)
    $publicCert.FriendlyName = "Keycloak SSL Certificate"

    $store = New-Object System.Security.Cryptography.X509Certificates.X509Store("Root", "LocalMachine")
    $store.Open("ReadWrite")
    $store.Add($publicCert)
    $store.Close()
    
    Write-Host "Certificate successfully trusted!" -ForegroundColor Green
} catch {
    Write-Host "ERROR: Failed to trust certificate: $_" -ForegroundColor Red
}

#cleanup
Remove-Item -Path "Cert:\LocalMachine\My\$certThumbprint" -Force
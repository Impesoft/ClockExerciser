# Android Release Build Script
# This script helps you build a signed Android release without storing passwords in Git

Write-Host "===== Clock Exerciser - Android Release Build =====" -ForegroundColor Cyan
Write-Host ""

# Check if environment variables are set
$keyPass = $env:ANDROID_KEY_PASS
$storePass = $env:ANDROID_STORE_PASS

if (-not $keyPass -or -not $storePass) {
    Write-Host "??  Environment variables not found!" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "You can either:" -ForegroundColor White
    Write-Host "1. Set them permanently (recommended):" -ForegroundColor White
    Write-Host '   [System.Environment]::SetEnvironmentVariable("ANDROID_KEY_PASS", "your_password", "User")' -ForegroundColor Gray
    Write-Host '   [System.Environment]::SetEnvironmentVariable("ANDROID_STORE_PASS", "your_password", "User")' -ForegroundColor Gray
    Write-Host ""
    Write-Host "2. Or enter them now (session-only):" -ForegroundColor White
    
    $keyPass = Read-Host "Enter Key Password" -AsSecureString
    $storePass = Read-Host "Enter Store Password" -AsSecureString
    
    # Convert SecureString to plain text for MSBuild
    $BSTR_key = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($keyPass)
    $keyPassPlain = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR_key)
    
    $BSTR_store = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($storePass)
    $storePassPlain = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR_store)
    
    Write-Host ""
}
else {
    Write-Host "? Environment variables found!" -ForegroundColor Green
    $keyPassPlain = $keyPass
    $storePassPlain = $storePass
}

Write-Host ""
Write-Host "Select build type:" -ForegroundColor Cyan
Write-Host "1. APK (for direct installation)"
Write-Host "2. AAB (for Google Play Store)"
Write-Host ""

$choice = Read-Host "Enter choice (1 or 2)"

Write-Host ""
Write-Host "?? Building..." -ForegroundColor Yellow
Write-Host ""

if ($choice -eq "1") {
    # Build APK
    dotnet build "$PSScriptRoot\ClockExerciser\ClockExerciser.csproj" `
        -c Release `
        -f net10.0-android `
        -p:AndroidSigningKeyPass="$keyPassPlain" `
        -p:AndroidSigningStorePass="$storePassPlain"
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "? Build successful!" -ForegroundColor Green
        Write-Host "?? APK location:" -ForegroundColor Cyan
        Write-Host "   ClockExerciser\bin\Release\net10.0-android\com.companyname.clockexerciser-Signed.apk"
    }
}
elseif ($choice -eq "2") {
    # Build AAB
    dotnet publish "$PSScriptRoot\ClockExerciser\ClockExerciser.csproj" `
        -c Release `
        -f net10.0-android `
        -p:AndroidPackageFormat=aab `
        -p:AndroidSigningKeyPass="$keyPassPlain" `
        -p:AndroidSigningStorePass="$storePassPlain"
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "? Build successful!" -ForegroundColor Green
        Write-Host "?? AAB location:" -ForegroundColor Cyan
        Write-Host "   ClockExerciser\bin\Release\net10.0-android\publish\com.companyname.clockexerciser-Signed.aab"
    }
}
else {
    Write-Host "? Invalid choice. Exiting." -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Press any key to exit..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")

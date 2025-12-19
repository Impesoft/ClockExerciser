# Setup Android Signing Environment Variables
# Run this once to permanently store your signing passwords

Write-Host "===== Android Signing Setup =====" -ForegroundColor Cyan
Write-Host ""
Write-Host "This script will save your signing passwords as USER environment variables." -ForegroundColor Yellow
Write-Host "They will be available to Visual Studio and build scripts." -ForegroundColor Yellow
Write-Host "They will NOT be committed to Git." -ForegroundColor Green
Write-Host ""

# Prompt for passwords securely
$keyPass = Read-Host "Enter Key Password" -AsSecureString
$storePass = Read-Host "Enter Store Password (can be same as key password)" -AsSecureString

# Convert SecureString to plain text
$BSTR_key = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($keyPass)
$keyPassPlain = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR_key)

$BSTR_store = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($storePass)
$storePassPlain = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR_store)

Write-Host ""
Write-Host "Setting environment variables..." -ForegroundColor Yellow

# Set environment variables permanently for the user
[System.Environment]::SetEnvironmentVariable('ANDROID_KEY_PASS', $keyPassPlain, 'User')
[System.Environment]::SetEnvironmentVariable('ANDROID_STORE_PASS', $storePassPlain, 'User')

Write-Host ""
Write-Host "? Environment variables set successfully!" -ForegroundColor Green
Write-Host ""
Write-Host "??  IMPORTANT: Restart Visual Studio to use the new variables!" -ForegroundColor Yellow
Write-Host ""
Write-Host "You can now build release APKs/AABs without entering passwords." -ForegroundColor Cyan
Write-Host ""

# Verify they were set
$verify_key = [System.Environment]::GetEnvironmentVariable('ANDROID_KEY_PASS', 'User')
$verify_store = [System.Environment]::GetEnvironmentVariable('ANDROID_STORE_PASS', 'User')

if ($verify_key -and $verify_store) {
    Write-Host "Verification: Variables are stored ?" -ForegroundColor Green
} else {
    Write-Host "??  Warning: Could not verify variables were set" -ForegroundColor Red
}

Write-Host ""
Write-Host "Press any key to exit..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")

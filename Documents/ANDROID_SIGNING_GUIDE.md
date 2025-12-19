# Android Code Signing Guide

## Overview
This guide walks you through setting up code signing for the Clock Exerciser Android app. Code signing is required to:
- Publish to Google Play Store
- Install on devices outside of debug mode
- Ensure app authenticity and integrity

---

## Prerequisites
- Java JDK installed (for keytool command)
- Android SDK installed (comes with Visual Studio)
- Access to PowerShell or Command Prompt

---

## Step 1: Generate a Keystore

A keystore is a binary file that contains your private key used to sign the app.

### Open PowerShell/Terminal and run:

```powershell
# Navigate to a secure location (e.g., Documents)
cd $env:USERPROFILE\Documents

# Generate keystore
keytool -genkey -v -keystore clockexerciser.keystore -alias clockexerciser -keyalg RSA -keysize 2048 -validity 10000
```

### You'll be prompted for:
1. **Keystore password**: Choose a strong password (minimum 6 characters)
2. **Key password**: Can be the same as keystore password or different
3. **Your name**: Your name or company name
4. **Organizational unit**: Your department (e.g., "Development")
5. **Organization**: Your company name
6. **City/Locality**: Your city
7. **State/Province**: Your state
8. **Country code**: Two-letter country code (e.g., "US", "NL", "BE")

### Important Notes:
- **SAVE YOUR PASSWORDS!** Write them down in a password manager
- **BACKUP YOUR KEYSTORE!** Store it in a secure location (cloud backup, USB drive)
- **NEVER COMMIT TO GIT!** The keystore should never be in version control
- **VALIDITY**: 10000 days = ~27 years (adjust if needed)

### Example Output:
```
Generating 2,048 bit RSA key pair and self-signed certificate (SHA256withRSA)
Enter keystore password: ********
Re-enter new password: ********
What is your first and last name?
  [Unknown]:  Ward Beyens
What is the name of your organizational unit?
  [Unknown]:  Development
...
```

---

## Step 2: Create Signing Configuration File

Create a file called `signing.properties` in the project root (next to .csproj):

```properties
# Android Signing Configuration
# DO NOT COMMIT THIS FILE TO GIT!

keystore.path=C:\\Users\\ward\\Documents\\clockexerciser.keystore
keystore.password=YOUR_KEYSTORE_PASSWORD
key.alias=clockexerciser
key.password=YOUR_KEY_PASSWORD
```

**Replace**:
- `C:\\Users\\ward\\Documents\\` with your actual keystore path (use `\\` for Windows paths)
- `YOUR_KEYSTORE_PASSWORD` with your keystore password
- `YOUR_KEY_PASSWORD` with your key password (can be same as keystore password)

---

## Step 3: Update .gitignore

Add these lines to your `.gitignore` file:

```gitignore
# Android signing
*.keystore
*.jks
signing.properties
```

This prevents accidentally committing sensitive signing information.

---

## Step 4: Update ClockExerciser.csproj

The CSPROJ has been updated with conditional signing configuration:

```xml
<!-- Android Release Signing Configuration -->
<PropertyGroup Condition="'$(Configuration)' == 'Release' And '$(TargetFramework.Contains(`android`))' == 'true'">
  <AndroidKeyStore>True</AndroidKeyStore>
  <AndroidSigningKeyStore>$(MSBuildProjectDirectory)\signing.keystore</AndroidSigningKeyStore>
  <AndroidSigningKeyAlias>clockexerciser</AndroidSigningKeyAlias>
  <AndroidSigningKeyPass></AndroidSigningKeyPass>
  <AndroidSigningStorePass></AndroidSigningStorePass>
</PropertyGroup>
```

This configuration:
- Only applies to **Release** builds
- Only applies to **Android** target framework
- Looks for `signing.keystore` in the project directory
- Uses the alias `clockexerciser`

---

## Step 5: Copy Keystore to Project (Optional)

You can either:

**Option A: Copy keystore to project directory**
```powershell
Copy-Item "$env:USERPROFILE\Documents\clockexerciser.keystore" "ClockExerciser\signing.keystore"
```

**Option B: Use full path in CSPROJ**
Update the CSPROJ to point to your keystore's full path:
```xml
<AndroidSigningKeyStore>C:\Users\ward\Documents\clockexerciser.keystore</AndroidSigningKeyStore>
```

---

## Step 6: Build Release APK/AAB

### Quick Start: Use the Build Scripts (Easiest!)

**One-time setup:**
```powershell
.\setup-android-signing.ps1
```
This will securely save your passwords as environment variables.

**Then build releases:**
```powershell
.\build-android-release.ps1
```
This interactive script lets you choose APK or AAB and handles everything.

---

### Manual Method: Use Environment Variables (Recommended)

**Set environment variables permanently:**

```powershell
# Set passwords as environment variables (one-time setup)
[System.Environment]::SetEnvironmentVariable('ANDROID_KEY_PASS', 'your_key_password', 'User')
[System.Environment]::SetEnvironmentVariable('ANDROID_STORE_PASS', 'your_store_password', 'User')

# Restart Visual Studio to pick up the new variables
```

**Then build normally:**
```powershell
dotnet build ClockExerciser/ClockExerciser.csproj -c Release -f net10.0-android
```

or

```powershell
dotnet publish ClockExerciser/ClockExerciser.csproj -c Release -f net10.0-android -p:AndroidPackageFormat=aab
```

### Option B: Pass Passwords via Command Line

**Build Signed APK:**
```powershell
dotnet build ClockExerciser/ClockExerciser.csproj `
  -c Release `
  -f net10.0-android `
  -p:AndroidSigningKeyPass="your_key_password" `
  -p:AndroidSigningStorePass="your_store_password"
```

**Build Signed AAB (for Google Play):**
```powershell
dotnet publish ClockExerciser/ClockExerciser.csproj `
  -c Release `
  -f net10.0-android `
  -p:AndroidPackageFormat=aab `
  -p:AndroidSigningKeyPass="your_key_password" `
  -p:AndroidSigningStorePass="your_store_password"
```

### Option C: Visual Studio (using environment variables)

1. Set environment variables using Option A above
2. Restart Visual Studio
3. Right-click ClockExerciser project ? Publish ? Android ? Archive
4. Visual Studio will use the environment variables automatically

### Output Locations:
- **APK**: `ClockExerciser/bin/Release/net10.0-android/com.companyname.clockexerciser-Signed.apk`
- **AAB**: `ClockExerciser/bin/Release/net10.0-android/publish/com.companyname.clockexerciser-Signed.aab`

---

## Step 7: Verify the Signature

Verify your APK is properly signed:

```powershell
# Navigate to Android SDK build-tools directory (adjust path for your version)
cd "$env:LOCALAPPDATA\Android\Sdk\build-tools\34.0.0"

# Verify APK signature
.\apksigner.bat verify --verbose "C:\path\to\your\app-Signed.apk"
```

Expected output:
```
Verified using v1 scheme (JAR signing): true
Verified using v2 scheme (APK Signature Scheme v2): true
Verified using v3 scheme (APK Signature Scheme v3): true
```

---

## Troubleshooting

### "keytool: command not found"
- Java JDK not installed or not in PATH
- Install JDK: https://adoptium.net/
- Add to PATH: `C:\Program Files\Eclipse Adoptium\jdk-17.0.x\bin`

### "Keystore password incorrect"
- Double-check your password in environment variables
- Make sure there are no extra spaces
- Check that Visual Studio was restarted after setting environment variables

### "XA4314: $(AndroidSigningKeyPass) is empty"
- Environment variables not set or Visual Studio not restarted
- Set them permanently:
  ```powershell
  [System.Environment]::SetEnvironmentVariable('ANDROID_KEY_PASS', 'your_password', 'User')
  [System.Environment]::SetEnvironmentVariable('ANDROID_STORE_PASS', 'your_password', 'User')
  ```
- Or pass via command line: `-p:AndroidSigningKeyPass="password"`

### "Could not find key with alias 'clockexerciser'"
- Verify the alias in your keystore:
  ```powershell
  keytool -list -v -keystore clockexerciser.keystore
  ```
- Update `signing.properties` with the correct alias

### Build succeeds but APK not signed
- Check that you're building in **Release** configuration
- Verify `AndroidKeyStore` is set to `True` in CSPROJ
- Check that keystore path is correct

---

## Security Best Practices

1. **Never commit keystore or passwords to Git**
2. **Store keystore in multiple secure locations** (password manager, encrypted USB, cloud backup)
3. **Use strong, unique passwords**
4. **Limit access** to keystore and passwords
5. **For team projects**, use Azure DevOps / GitHub Actions secrets for CI/CD signing

---

## Google Play Store Preparation

When publishing to Google Play:

1. **Use AAB format** (Android App Bundle) - required for new apps
2. **Update Application ID** in CSPROJ:
   ```xml
   <ApplicationId>com.yourcompany.clockexerciser</ApplicationId>
   ```
3. **Set Version Codes**:
   ```xml
   <ApplicationVersion>1</ApplicationVersion>
   <ApplicationDisplayVersion>1.0.0</ApplicationDisplayVersion>
   ```
4. **Create Google Play Developer Account** ($25 one-time fee)
5. **Complete Play Console listing** (descriptions, screenshots, privacy policy)

---

## Quick Reference

| Task | Command |
|------|---------|
| Generate keystore | `keytool -genkey -v -keystore clockexerciser.keystore -alias clockexerciser -keyalg RSA -keysize 2048 -validity 10000` |
| List keystore aliases | `keytool -list -v -keystore clockexerciser.keystore` |
| Build signed APK | `dotnet build -c Release -f net10.0-android` |
| Build signed AAB | `dotnet publish -c Release -f net10.0-android -p:AndroidPackageFormat=aab` |
| Verify signature | `apksigner verify --verbose app-Signed.apk` |

---

**Last Updated**: December 2024
**For**: Clock Exerciser v1.0

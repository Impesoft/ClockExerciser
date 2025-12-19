# Quick Start: Building Android Release

## First Time Setup (Do Once)

1. **Generate keystore** (if you haven't already):
```powershell
cd $env:USERPROFILE\Documents
keytool -genkey -v -keystore clockexerciser.keystore -alias clockexerciser -keyalg RSA -keysize 2048 -validity 10000
```

2. **Set up environment variables**:
```powershell
.\setup-android-signing.ps1
```

3. **Restart Visual Studio**

## Building Releases (Anytime)

### Option 1: Use the Script (Easiest)
```powershell
.\build-android-release.ps1
```
Choose 1 for APK or 2 for AAB.

### Option 2: Command Line
```powershell
# APK
dotnet build ClockExerciser/ClockExerciser.csproj -c Release -f net10.0-android

# AAB (for Google Play)
dotnet publish ClockExerciser/ClockExerciser.csproj -c Release -f net10.0-android -p:AndroidPackageFormat=aab
```

### Option 3: Visual Studio
1. Right-click ClockExerciser project
2. Publish ? Android ? Archive
3. Sign and Distribute

## Output Locations

- **APK**: `ClockExerciser\bin\Release\net10.0-android\com.companyname.clockexerciser-Signed.apk`
- **AAB**: `ClockExerciser\bin\Release\net10.0-android\publish\com.companyname.clockexerciser-Signed.aab`

## Troubleshooting

**"XA0129: Fast Deployment Error"** ? FIXED
- Already configured: `<EmbedAssembliesIntoApk>true</EmbedAssembliesIntoApk>`
- This setting embeds assemblies into APK for reliable deployment

**"XA4314: $(AndroidSigningKeyPass) is empty"**
- Environment variables not set
- Run `.\setup-android-signing.ps1`
- Restart Visual Studio

**"Password incorrect"**
- Check your environment variables are correct
- Re-run setup script

**Device not detected**
- Enable USB Debugging on Android device
- Run `adb devices` to verify connection
- Try different USB cable/port

For more troubleshooting help, see `Documents/ANDROID_TROUBLESHOOTING.md`

## Security Notes

? Passwords stored in USER environment variables (not in Git)  
? Only you can see them (Windows user account)  
? Scripts provided for easy setup  
? Keystore excluded from Git via .gitignore  

For full documentation, see `Documents/ANDROID_SIGNING_GUIDE.md`

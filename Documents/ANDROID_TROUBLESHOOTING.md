# Android Deployment Troubleshooting

## Common Android Deployment Errors

### XA0129: Fast Deployment Error

**Error Message:**
```
Error XA0129: Error deploying 'files/.__override__/arm64-v8a/Xamarin.Google.Guava.ListenableFuture.pdb'.
Please set the 'EmbedAssembliesIntoApk' MSBuild property to 'true' to disable Fast Deployment
```

**Solution:** ? FIXED
Added `<EmbedAssembliesIntoApk>true</EmbedAssembliesIntoApk>` to the main PropertyGroup in ClockExerciser.csproj.

**What it does:**
- Disables Fast Deployment feature
- Embeds all assemblies directly into the APK
- Slightly slower deployment but more reliable
- Required for some dependency configurations

**Trade-offs:**
- ? More reliable deployment
- ? Fixes PDB deployment errors
- ?? Slightly slower initial deployment
- ?? Larger APK size during development

---

## Other Common Android Errors

### ADB Connection Issues

**Symptoms:**
- Device not detected
- "No devices found" error
- Deployment hangs

**Solutions:**
1. Enable USB Debugging on Android device
2. Accept USB debugging prompt on device
3. Restart ADB: `adb kill-server` then `adb start-server`
4. Try different USB cable/port
5. Install Google USB Driver (Windows)

### Build Errors After Package Updates

**Symptoms:**
- CS0234: Type or namespace does not exist
- Build fails after adding/updating NuGet packages

**Solutions:**
1. Clean solution: `dotnet clean`
2. Delete `bin/` and `obj/` folders
3. Restore packages: `dotnet restore --force`
4. Rebuild solution

### Android SDK Issues

**Symptoms:**
- "Android SDK not found"
- Build tools version errors

**Solutions:**
1. Open Visual Studio Android SDK Manager
2. Install required Android SDK platforms
3. Install Android Build Tools
4. Set ANDROID_HOME environment variable
5. Restart Visual Studio

### Signing Errors

**Symptoms:**
- XA4314: AndroidSigningKeyPass is empty
- Keystore not found

**Solutions:**
1. See `ANDROID_SIGNING_GUIDE.md` for full setup
2. Run `.\setup-android-signing.ps1`
3. Restart Visual Studio after setting environment variables
4. Verify keystore file exists at specified path

---

## Deployment Best Practices

### For Development (Debug)
```xml
<!-- Fast deployment enabled (default) -->
<EmbedAssembliesIntoApk>false</EmbedAssembliesIntoApk>
```

### For Testing/Release
```xml
<!-- All assemblies embedded (current setting) -->
<EmbedAssembliesIntoApk>true</EmbedAssembliesIntoApk>
```

### For Maximum Performance Testing
```xml
<EmbedAssembliesIntoApk>true</EmbedAssembliesIntoApk>
<AndroidLinkMode>Full</AndroidLinkMode>
<AndroidUseAapt2>true</AndroidUseAapt2>
```

---

## Checking Deployment Status

### Via Visual Studio
1. Open "Output" window
2. Select "Show output from: Build"
3. Look for "Deploy succeeded" message

### Via ADB
```powershell
# List installed packages
adb shell pm list packages | Select-String "clockexerciser"

# Get package info
adb shell dumpsys package com.clockexerciser.app

# Uninstall if needed
adb uninstall com.clockexerciser.app
```

### Via Device
1. Settings ? Apps
2. Find "ClockExerciser"
3. Check version number matches

---

## Performance Testing

### Check APK Size
```powershell
Get-Item "ClockExerciser\bin\Debug\net10.0-android\*.apk" | Select Name, Length
```

### Monitor Logcat During Runtime
```powershell
adb logcat | Select-String "ClockExerciser"
```

### Check Memory Usage
```powershell
adb shell dumpsys meminfo com.clockexerciser.app
```

---

## Quick Fixes Summary

| Issue | Quick Fix |
|-------|-----------|
| XA0129 Error | Set `EmbedAssembliesIntoApk=true` ? |
| Device not found | `adb devices` then enable USB debugging |
| Build fails | Clean ? Delete bin/obj ? Restore ? Rebuild |
| Signing error | Run `.\setup-android-signing.ps1` |
| Old APK installed | `adb uninstall com.clockexerciser.app` |

---

**Last Updated**: December 2024  
**Status**: XA0129 Error Resolved ?

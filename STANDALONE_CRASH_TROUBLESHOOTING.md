# Crash When Not Connected to Visual Studio - Troubleshooting Guide

## Symptom
App works fine when debugging in Visual Studio but crashes immediately when launched standalone (deployed APK or installed on device without debugger attached).

## Most Common Causes

### 1. Syncfusion License Issue (MOST LIKELY) ??

**Problem:**
Syncfusion Community License only works in DEBUG mode. When you deploy without Visual Studio, the app runs in RELEASE mode and the license check fails, causing a crash.

**Check if this is your issue:**
```csharp
// In MauiProgram.cs, line 15:
SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1JGaF5cXGpCf1FpR2dGfV5ycUVHYVZTRnxbQE0SNHVRdkdmWH1fd3VURGJZU011XUBWYEs=");
```

**Solutions:**

#### Option A: Get Valid Syncfusion License
1. Check https://www.syncfusion.com/sales/communitylicense
2. If eligible, get a Community License that works in Release mode
3. Replace the license key in MauiProgram.cs

#### Option B: Conditional License (Temporary Debug Aid)
```csharp
#if DEBUG
SyncfusionLicenseProvider.RegisterLicense("YOUR_COMMUNITY_LICENSE");
#else
SyncfusionLicenseProvider.RegisterLicense("YOUR_PRODUCTION_LICENSE");
#endif
```

#### Option C: Remove Syncfusion (Last Resort)
Replace Syncfusion.Maui.Gauges with a free alternative:
- Use MAUI Graphics to draw clock
- Use SkiaSharp
- Create custom clock control

**How to verify:**
1. Build in Release mode: `dotnet build -c Release -f net10.0-android`
2. Deploy APK without debugger
3. Check device logs: `adb logcat | Select-String "Syncfusion"`

---

### 2. Audio Files Not Embedded ??

**Problem:**
The audio files (success.mp3, error.mp3) might not be properly included in the release APK.

**Check if this is your issue:**
```bash
# Extract APK and check if audio files are present
Expand-Archive com.clockexerciser.app-Signed.apk temp_extract
Get-ChildItem temp_extract -Recurse -Filter "*.mp3"
```

**Solutions:**

#### Already Implemented:
? Added null checks in AudioService.cs
? Added try-catch blocks to prevent crashes

#### Verify Build Action:
Make sure audio files exist and are configured correctly:
```
ClockExerciser/Resources/Raw/
??? success.mp3  (must exist!)
??? error.mp3    (must exist!)
??? README.txt
```

#### Check CSPROJ:
```xml
<!-- Should be present in ClockExerciser.csproj -->
<MauiAsset Include="Resources\Raw\**" LogicalName="%(RecursiveDir)%(Filename)%(Extension)" />
```

**How to verify:**
1. Check if files exist: `Get-ChildItem ClockExerciser/Resources/Raw/*.mp3`
2. Build and check APK contents
3. Check logcat for "sound file not found" messages

---

### 3. AudioManager.Current is Null ??

**Problem:**
`AudioManager.Current` might be null on certain devices or in release builds.

**Already Implemented:**
? Added null check in MauiProgram.cs
? Will throw clear exception instead of mysterious crash

**How to verify:**
Check logcat for: "AudioManager.Current is null"

---

### 4. Unhandled Exceptions in Release Mode ??

**Problem:**
In DEBUG mode, Visual Studio catches exceptions. In Release mode without debugger, unhandled exceptions crash the app.

**Solution:**
Add global exception handling:

```csharp
// In App.xaml.cs
public App()
{
    InitializeComponent();
    
    // Global exception handler
    AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
    {
        var exception = args.ExceptionObject as Exception;
        System.Diagnostics.Debug.WriteLine($"Unhandled exception: {exception?.Message}");
        System.Diagnostics.Debug.WriteLine($"Stack trace: {exception?.StackTrace}");
    };
    
    MainPage = new AppShell();
}
```

---

## Debugging Steps

### Step 1: Check Logcat Output
```bash
# Connect device and get logs
adb logcat | Select-String "ClockExerciser"

# Look for:
# - "Syncfusion" errors
# - "sound file not found"
# - "AudioManager.Current is null"
# - Any exception messages
```

### Step 2: Build Release with Logging
Keep debug output even in Release mode:

```xml
<!-- In ClockExerciser.csproj -->
<PropertyGroup Condition="'$(Configuration)' == 'Release'">
    <DebugSymbols>true</DebugSymbols>
    <DebugType>portable</DebugType>
</PropertyGroup>
```

### Step 3: Test Audio Separately
Temporarily disable audio to isolate the issue:

```csharp
// In MauiProgram.cs, comment out audio registration:
// builder.Services.AddSingleton(AudioManager.Current);
// builder.Services.AddSingleton<IAudioService, AudioService>();

// Replace with dummy service:
builder.Services.AddSingleton<IAudioService, DummyAudioService>();

public class DummyAudioService : IAudioService
{
    public Task PlaySuccessSound() => Task.CompletedTask;
    public Task PlayErrorSound() => Task.CompletedTask;
}
```

### Step 4: Test Syncfusion License
Temporarily remove Syncfusion:

1. Comment out Syncfusion in GamePage.xaml
2. Use a Label instead of the gauge
3. Build and test
4. If it works, Syncfusion license is the issue

---

## Quick Fixes Applied

### ? AudioService.cs
- Added null checks for streams
- Added null checks for audio players
- Better error logging
- Won't crash if audio files missing

### ? MauiProgram.cs
- Added null check for AudioManager.Current
- Clear exception message if audio not available

### ?? Still Need to Check
- Syncfusion license validity in Release mode
- Audio files actually present in APK
- Device-specific issues

---

## Testing Checklist

### Build and Deploy Tests:
- [ ] Build in Debug mode ? Works? (baseline)
- [ ] Build in Release mode locally
- [ ] Deploy Release APK without debugger
- [ ] Check if app launches
- [ ] Check logcat for errors

### Isolation Tests:
- [ ] Disable audio ? Does it work?
- [ ] Remove Syncfusion ? Does it work?
- [ ] Test on different Android version
- [ ] Test on different device

### Verification:
- [ ] APK contains audio files (extract and check)
- [ ] Syncfusion license valid for Release
- [ ] All dependencies compatible with Release

---

## Most Likely Solution

**Based on the symptoms, 90% probability it's the Syncfusion license.**

### Immediate Action:
1. Check your Syncfusion license status
2. Verify it works in Release mode
3. If not, get a valid license or remove Syncfusion

### Short-term Workaround:
Build with DEBUG configuration even for deployment:
```bash
dotnet build -c Debug -f net10.0-android
```

### Long-term Fix:
Get a proper Syncfusion license or migrate to a free alternative.

---

## Logcat Commands Reference

```bash
# View all logs
adb logcat

# Filter for app
adb logcat | Select-String "ClockExerciser"

# Filter for crashes
adb logcat | Select-String "FATAL"

# Filter for exceptions
adb logcat | Select-String "Exception"

# Clear and watch
adb logcat -c
adb logcat | Select-String "ClockExerciser|FATAL|Exception"

# Save to file
adb logcat > crash_log.txt
```

---

## Files Modified

1. ? `ClockExerciser/Services/AudioService.cs`
   - Added null checks
   - Better error handling
   - Won't crash on missing files

2. ? `ClockExerciser/MauiProgram.cs`
   - Added AudioManager.Current null check
   - Clear error message

3. ?? Need to investigate:
   - Syncfusion license configuration
   - Audio file embedding

---

**Next Steps:**
1. Check logcat output when app crashes
2. Look for "Syncfusion" in the error message
3. If Syncfusion license error, get valid license or remove component
4. If audio error, verify files are in APK
5. Report back with logcat output for further diagnosis

**Status:** Preventive fixes applied, need logcat output to confirm root cause
**Priority:** High - prevents app from running outside Visual Studio
**Estimated Fix Time:** 
- If Syncfusion: Get license (immediate) or remove component (2-4 hours)
- If audio files: Fix build config (30 minutes)

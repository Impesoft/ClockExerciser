# Standalone Crash - Still Investigating

## Current Status
- ? **Fixed**: Slider validation bug (01:15 now matches correctly)
- ? **Still broken**: App won't start without debugger attached
- ? **Working**: App runs fine in debug mode with Visual Studio

## Bugs Fixed

### 1. Slider Validation Bug ?
**Problem:** Setting sliders to 01:15 was marked incorrect even though it matched target 01:15.

**Root Cause:** 
- `EvaluateClockAnswer()` was comparing `UserHourValue` (integer 1) with `GetTargetHourPointer()` (fractional 1.25)
- The target hour pointer includes fractional movement: `1 + 15/60 = 1.25`
- User slider can only be whole numbers

**Fix:**
Changed to compare integer hours and minutes directly:
```csharp
var userHour = (int)UserHourValue;  // 1
var targetHour = TargetTime.Hours % 12;  // 1
var userMinute = (int)UserMinuteValue;  // 15
var targetMinute = TargetTime.Minutes;  // 15
// Now compares 1 == 1 and 15 == 15 ?
```

## Still Investigating: Crash Without Debugger

### What We Know
1. **Works**: When launched from Visual Studio (debug mode)
2. **Works**: While running after debug session ends (app stays open)
3. **Crashes**: When launched fresh without debugger
4. **Crashes**: When launched standalone (not from VS)

This pattern strongly suggests **debugger-dependent initialization**.

### Potential Causes

#### 1. JIT Compilation vs AOT (Most Likely)
**Theory:** Debug builds use JIT compilation, Release/standalone uses AOT.

**Test:**
```xml
<!-- Add to ClockExerciser.csproj -->
<PropertyGroup Condition="'$(Configuration)' == 'Release'">
    <RunAOTCompilation>false</RunAOTCompilation>
    <AndroidEnableProfiledAot>false</AndroidEnableProfiledAot>
</PropertyGroup>
```

#### 2. Missing Linker Preservation
**Theory:** Linker strips necessary assemblies in standalone builds.

**Test:**
```xml
<PropertyGroup>
    <AndroidLinkMode>None</AndroidLinkMode>
    <PublishTrimmed>false</PublishTrimmed>
</PropertyGroup>
```

#### 3. Debugger-Specific Permissions
**Theory:** Visual Studio grants additional permissions that standalone doesn't have.

**Check:** AndroidManifest.xml permissions (already has INTERNET)

#### 4. Initialization Timing
**Theory:** Something depends on debugger's timing/threading behavior.

**Test:** Add delays in MauiProgram.cs

### Logging Added

#### App.xaml.cs
- ? Global exception handler
- ? Unobserved task exception handler
- ? Try-catch around InitializeComponent

#### MauiProgram.cs
- ? Step-by-step logging with emojis
- ? Try-catch around license registration
- ? Try-catch around AudioManager
- ? Try-catch around entire CreateMauiApp

#### MainActivity.cs (NEW)
- ? Try-catch around OnCreate
- ? Android-specific exception handler
- ? Logs to both Debug.WriteLine and Android.Util.Log

### How to Get Crash Logs

```bash
# Clear logs
adb logcat -c

# Launch app WITHOUT debugger
# (Install APK and tap icon, or use: adb shell am start -n com.clockexerciser.app/.MainActivity)

# Capture ALL logs
adb logcat > crash_full.txt

# Or filter for relevant info
adb logcat | Select-String "ClockExerciser|??|??|?|FATAL|AndroidRuntime|mono"
```

### What to Look For in Logs

#### Successful Start (Expected with debugger):
```
?? MainActivity.OnCreate starting...
?? Starting MauiApp creation...
?? Registering Syncfusion license...
? Syncfusion license registered successfully
?? Configuring MAUI app...
?? Registering audio manager...
? Audio manager registered
?? Registering services...
? All services registered
??? Building app...
?? Initializing ServiceHelper...
? MauiApp created successfully!
? MainActivity.OnCreate completed
```

#### Failed Start (What we need to see):
```
?? MainActivity.OnCreate starting...
?? Starting MauiApp creation...
?? Registering Syncfusion license...
[Last message before crash - tells us what's failing]
? FATAL ERROR: [the actual error]
```

### Debugging Steps

#### Step 1: Build and Deploy
```bash
# Build in Debug mode (to get detailed logging)
dotnet build -c Debug -f net10.0-android

# Install APK
adb install -r ClockExerciser\bin\Debug\net10.0-android\com.clockexerciser.app-Signed.apk

# Or deploy directly
dotnet build -c Debug -f net10.0-android -t:Run
```

#### Step 2: Launch WITHOUT Debugger
```bash
# Option A: Tap app icon on device
# Option B: Launch via adb
adb shell am start -n com.clockexerciser.app/.MainActivity

# Option C: Launch and immediately grab logs
adb shell am start -n com.clockexerciser.app/.MainActivity & adb logcat
```

#### Step 3: Capture Crash
```bash
# Watch logs in real-time
adb logcat | Select-String "ClockExerciser|FATAL|AndroidRuntime"

# Save to file
adb logcat > crash_$(Get-Date -Format "yyyyMMdd_HHmmss").txt
```

#### Step 4: Analyze Crash
Look for:
1. **Last successful log message** - what completed before crash?
2. **Exception type** - NullReferenceException? TypeLoadException?
3. **Stack trace** - where did it fail?
4. **Native crash** - check for "Fatal signal" or "backtrace"

### Quick Tests

#### Test 1: Disable Syncfusion Temporarily
Comment out in MauiProgram.cs:
```csharp
//.ConfigureSyncfusionCore()
```
And in GamePage.xaml, replace gauge with Label.

**If it works:** Syncfusion initialization is the problem.

#### Test 2: Disable Audio
Comment out in MauiProgram.cs:
```csharp
//builder.Services.AddSingleton(audioManager);
//builder.Services.AddSingleton<IAudioService, AudioService>();
```

**If it works:** Audio initialization is the problem.

#### Test 3: Minimal App
Create a simple test to isolate:
```csharp
// In App.xaml.cs CreateWindow
return new Window(new ContentPage { 
    Content = new Label { Text = "Hello!" } 
});
```

**If it works:** Something in AppShell or pages is the problem.

### Suspected Culprits (Ranked)

| Component | Probability | Why |
|-----------|-------------|-----|
| Syncfusion AOT | 40% | Complex library, might not support AOT |
| Plugin.Maui.Audio | 30% | Platform-specific, might have init issues |
| Linker trimming | 20% | Might remove needed assemblies |
| Resource loading | 10% | Fonts, images, XAML compilation |

### Next Steps

1. **Get logcat output** - This is critical!
   ```bash
   adb logcat -c
   # Launch app
   adb logcat > crash.txt
   # Share crash.txt
   ```

2. **Try linker settings** (if no logcat available):
   Add to CSPROJ:
   ```xml
   <PropertyGroup>
       <AndroidLinkMode>None</AndroidLinkMode>
   </PropertyGroup>
   ```

3. **Try AOT disable** (if linker doesn't help):
   ```xml
   <RunAOTCompilation>false</RunAOTCompilation>
   ```

## Summary

### ? What's Fixed
- Slider validation now works correctly (01:15 = 01:15)
- Comprehensive logging throughout startup
- Android-specific crash handlers

### ? What's Still Broken
- App crashes on standalone launch (without debugger)
- Need logcat output to diagnose

### ?? What We Need
- **Logcat output from crash** - Run `adb logcat` during failed launch
- Look for the last ? message before ? crash
- Share the exception type and stack trace

### ?? Most Likely Fix
Based on symptoms, probably need to add linker preservation or disable AOT:
```xml
<AndroidLinkMode>None</AndroidLinkMode>
<PublishTrimmed>false</PublishTrimmed>
```

But we need the crash logs to be sure!

---

**Status:** Slider bug fixed ?, Standalone crash still under investigation ?
**Next Action:** Get logcat output from standalone launch
**Priority:** High - app unusable without debugger

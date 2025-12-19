# TODO - Remaining Work

## High Priority (Before 1.0 Release)

### Audio Implementation ? COMPLETE
- [x] Source or create `success.mp3` sound file ?
- [x] Source or create `error.mp3` sound file ?
- [x] Add files to `ClockExerciser/Resources/Raw/` ?
- [x] Implement actual audio playback in `AudioService.cs` ?
  - ? Using Plugin.Maui.Audio (v3.0.1)
- [x] Register IAudioManager in DI ?
- [x] Fix Android deployment error (XA0129) ?
- [ ] Test audio on Android device/emulator
- [ ] Test audio in both success and error scenarios

### Testing & QA
- [x] Add debug information for incorrect answers ?
- [x] Fix language order to match default (English first) ?
- [x] Fix seconds validation bug (remove seconds from validation) ?
- [x] Add realistic ticking second hand (visual only) ?
- [x] Fix 12/24 hour time matching (use (hours % 12) + 12) ?
- [x] Add comprehensive startup logging and exception handling ?
- [x] Fix potential Syncfusion license validation crash ?
- [ ] Manual testing on Android device
  - [ ] **Test standalone launch (not from Visual Studio)** ??
  - [ ] **Check logcat output during launch** ??
  - [ ] **Verify app launches without debugger attached** ??
  - [ ] Test all three game modes
  - [ ] Test language switching (English ? Nederlands)
  - [ ] Test natural language input (English & Dutch)
  - [ ] Test navigation (menu ? game)
  - [ ] Test app icon and splash screen
  - [ ] Verify clock hands move correctly (hour and minute only)
  - [ ] **Verify second hand ticks every second** ??
  - [ ] Test answer validation in both modes (no seconds)
  - [ ] **Test 12/24 hour matching (17:50 = 5:50, 0:00 = 12:00)** ??
  - [ ] **Verify debug output shows 12-23 range (no 0 values)** ??
  - [ ] Use debug info to verify time parsing
  - [ ] Verify prompt text changes language correctly
  - [ ] Confirm second hand is red and visible
  - [ ] Verify only hour and minute sliders shown (no second slider)
- [ ] Fix any bugs found during testing

### Documentation
- [ ] Add screenshots to `Documents/Screenshots/`
  - Menu page
  - Clock to Time mode
  - Time to Clock mode
  - Language selection
- [ ] Update README with actual screenshots
- [ ] Create user guide (optional)

### Polish (Optional for 1.0)
- [ ] Add fade-in animation for menu page
- [ ] Add shake animation for incorrect answer
- [ ] Add checkmark animation for correct answer
- [ ] Improve button hover/press states

---

## Medium Priority (Version 1.1)

### Features
- [ ] Score tracking system
  - [ ] Track correct/incorrect answers
  - [ ] Display score on game page
  - [ ] Persist scores between sessions
- [ ] Settings page
  - [ ] Sound on/off toggle
  - [ ] Difficulty level selection
  - [ ] Theme selection (light/dark)
- [ ] Timer mode
  - [ ] Add countdown timer
  - [ ] Track time per answer
  - [ ] Display best times

### Testing
- [ ] Create unit test project
- [ ] Write tests for `DutchTimeParser`
- [ ] Write tests for `EnglishTimeParser`
- [ ] Write tests for `MatchesTime`
- [ ] Write tests for `GameViewModel` logic
- [ ] Set up CI/CD pipeline

---

## Low Priority (Version 1.2+)

### Platform Support
- [ ] iOS deployment
  - [ ] Apple Developer account
  - [ ] Code signing setup
  - [ ] Test on iOS simulator/device
  - [ ] App Store submission
- [ ] Windows deployment
  - [ ] Test on Windows 10/11
  - [ ] Microsoft Store preparation
- [ ] macOS (Catalyst) support

### Features
- [ ] Additional languages
  - [ ] French time parsing
  - [ ] German time parsing
  - [ ] Spanish time parsing
- [ ] Social features
  - [ ] Share scores
  - [ ] Leaderboards
  - [ ] Challenges between friends
- [ ] Analytics
  - [ ] Track most difficult times
  - [ ] Track learning progress
  - [ ] Generate progress reports

### Advanced
- [ ] Accessibility improvements
  - [ ] Screen reader support
  - [ ] High contrast mode
  - [ ] Font size options
- [ ] Tablet optimization
  - [ ] Larger layout for tablets
  - [ ] Split-screen support
- [ ] Wear OS support (watch face learning)

---

## Technical Debt

### Code Quality
- [ ] Add XML documentation comments to public APIs
- [ ] Add more comprehensive error handling
- [ ] Add logging framework
- [ ] Review and optimize performance

### Architecture
- [ ] Consider extracting time parsing to separate library
- [ ] Add more granular service interfaces
- [ ] Implement repository pattern for settings/scores
- [ ] Add caching where appropriate

### Build & Deployment
- [ ] Set up automated builds (GitHub Actions/Azure DevOps)
- [ ] Set up automated testing
- [ ] Create release pipeline
- [ ] Add version bumping automation

---

## Notes

### Priority Levels
- **High**: Required for 1.0 release
- **Medium**: Nice to have for 1.1 release
- **Low**: Future enhancements

### Estimation
- High priority work: ~8-16 hours
- Medium priority work: ~16-24 hours
- Low priority work: ~40+ hours

### Dependencies
Some tasks depend on others:
- Audio implementation ? Audio testing
- iOS deployment ? Apple Developer account
- Settings page ? Settings persistence
- Score tracking ? Settings persistence

---

**Last Updated**: December 2024
**Next Review**: After 1.0 release

# GitHub Copilot Instructions for Clock Exerciser

## Project Context
Clock Exerciser is a .NET 10 solution with:
- a **.NET MAUI app** for Android, iOS, Windows, and MacCatalyst
- a **hosted Blazor WebAssembly PWA** for browser/offline play
- shared UI and logic used across both app surfaces

The product helps users learn to read analog clocks and convert between analog and digital time formats in English and Dutch.

---

## Start With the Right Level of Context

Do **not** treat the `Documents/` folder as mandatory reading for every tiny fix.

### Review `Documents/*.md` first when:
- adding a new feature
- changing architecture or project structure
- changing navigation, platform behavior, deployment, or publish flow
- updating long-lived patterns, services, or shared components
- the task explicitly mentions planning, roadmap, or architecture

### Skip full doc review for small, local fixes when:
- the change is isolated to one or two files
- the task is a straightforward bug fix or cleanup
- the code clearly shows the current behavior already

### Important rule
- **Code is the source of truth.**
- Use docs as guidance, not as unquestionable truth.
- If docs and code differ, follow the code and update the docs only if the change is important enough to preserve.

### Documentation maintenance
Update `Documents/PROJECT_PLAN.md` and/or `Documents/ARCHITECTURE.md` only when the change affects:
- roadmap or task tracking
- architecture or runtime composition
- deployment or publish behavior
- reusable patterns other contributors should know

Do not update project docs for every minor bug fix.

---

## Current Solution Shape

Key projects:
- `Clock_Exerciser` - .NET MAUI app host
- `Clock_Exerciser.Web` - ASP.NET Core host for the Blazor WebAssembly client
- `Clock_Exerciser.Web.Client` - Blazor WebAssembly PWA client
- `Clock_Exerciser.Shared` - shared Razor components, pages, and static web assets
- `Clock_Exerciser.Core` - shared application logic and abstractions
- `Clock_Exerciser.Tests` - tests

### Current architecture guidance
- Prefer keeping business logic in `Clock_Exerciser.Core`
- Prefer keeping reusable UI in `Clock_Exerciser.Shared`
- Keep platform-specific host behavior in the MAUI or web host projects
- Avoid duplicating logic between MAUI and web unless platform constraints require it

---

## Technology Stack
- **.NET 10**
- **C# 14**
- **.NET MAUI** for native app hosting
- **Blazor WebAssembly + ASP.NET Core host** for web/PWA hosting
- **Syncfusion.Maui.Gauges** for the native analog clock UI

---

## Code Style & Conventions

### General C#
- Follow standard C# naming conventions
- Use `var` when the type is obvious
- Use nullable reference types correctly
- Keep methods focused and small
- Prefer existing abstractions before adding new ones

### MAUI/XAML
- Prefer binding over code-behind manipulation
- Use reusable styles/resources where appropriate
- Keep page-specific behavior out of code-behind unless platform/UI lifecycle requires it

### Blazor
- Prefer shared Razor components/pages in `Clock_Exerciser.Shared`
- Keep browser-only interop inside the web client or web host
- Use root-absolute static asset paths for hosted WebAssembly scenarios when relative paths may break on routed pages

### Dependency Injection
- Register services in the appropriate host:
  - `MauiProgram.cs` for MAUI
  - `Program.cs` for Blazor WebAssembly client
  - `Program.cs` for ASP.NET Core host if host-only behavior is needed

---

## Localization
- Do not hardcode user-facing strings
- Reuse the existing localization/text abstractions already present in the solution
- When adding new strings, update both English and Dutch resources
- Verify both languages when changing prompts, labels, or result messages

---

## Clock and Game Logic

### Clock hand scaling
- Hour hand uses direct 0-12 values
- Minute/second hands must use the existing dial-scaling logic
- Do not pass raw 0-59 values to a 0-12 dial without conversion

### Time validation
- Preserve wrap-around and 12/24-hour equivalence behavior
- Be careful with tolerance logic and circular comparisons

### Natural language parsing
- Support both English and Dutch patterns already established in the codebase
- Dutch `half vijf` means `4:30`, not `5:30`
- Return nullable parse results on failure where existing parser contracts expect that

---

## Audio Guidance
- Keep audio playback async
- Do not block UI interaction on audio
- Respect the current platform split:
  - MAUI uses the native/mobile audio implementation
  - Web/PWA uses browser audio via JS interop
- For hosted WebAssembly, ensure static asset paths are valid from routed pages and in published output

---

## PWA / Web Publish Guidance
- `Clock_Exerciser.Web` hosts the published WebAssembly client
- `Clock_Exerciser.Web.Client/wwwroot/index.html` controls browser boot behavior
- Service worker and static web asset behavior matter for publish/offline fixes
- Be careful with relative asset paths, placeholder-based script URLs, and service-worker cache behavior
- If changing publish/runtime behavior, validate both build output and browser behavior

---

## Testing Expectations

When adding or changing behavior:
- run relevant tests when they exist
- build the solution or the affected projects
- validate the touched runtime when practical:
  - MAUI for native-only changes
  - web host/client for browser or publish changes

For new business logic, prefer adding tests in `Clock_Exerciser.Tests`.

---

## Common Tasks

### Add a service
1. Put shared abstractions/logic in the appropriate shared project
2. Add platform-specific implementations only where needed
3. Register the implementation in the correct host project

### Add localized text
1. Add the resource key/value in English
2. Add the Dutch translation
3. Verify both languages in the affected UI

### Change web/PWA assets
1. Check whether the asset lives in `Clock_Exerciser.Web.Client`, `Clock_Exerciser.Web`, or `Clock_Exerciser.Shared`
2. Confirm the resulting path works in hosted WebAssembly
3. Consider service-worker/static-web-assets implications

---

## Communication Style
- Be concise and direct
- Explain why a change is needed when it is not obvious
- Offer alternatives only when there is a real tradeoff
- Do not invent architecture details not reflected in the code

---

## File Editing Guidelines
- When editing repository files, do not use the terminal as a fallback if file-editing tools are expected; use direct file editing and verify the result.

---

## Quick Reference

| Task | Guidance |
|------|----------|
| Small bug fix | Read the relevant code first; consult docs only if needed |
| Feature or architecture change | Review `Documents/PROJECT_PLAN.md` and `Documents/ARCHITECTURE.md` |
| Web/PWA asset fix | Check `index.html`, service worker, and static web assets |
| Add shared logic | Prefer `Clock_Exerciser.Core` |
| Add shared UI | Prefer `Clock_Exerciser.Shared` |
| Add host-specific registration | Update the corresponding `Program.cs` / `MauiProgram.cs` |

---

**Last Updated**: 2026-05-17

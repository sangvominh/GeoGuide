# Prompt: Workflow Console UI Agent

```powershell
cd C:\dev\personal\geoguide-ui-worktrees\workflow-console
```

Read this prompt and implement only your scope.

Goal: build a visible Reference Workflow Console for localization, audio/TTS, and offline maps. The APIs exist, but users currently see only JSON if they know the URL.

Own:

- `src/GeoGuide.Cms/Controllers/ReferenceConsoleController.cs`
- `src/GeoGuide.Cms/Views/ReferenceConsole/**`
- workflow console view models

Avoid:

- seller/admin review/AI pages
- mobile files

Requirements:

- `GET /ReferenceConsole`
  - show 3 panels: Localization, Audio/TTS, Maps Offline.
- Localization panel:
  - form for language code.
  - button/action to call warmup service logic or redirect/link to API.
  - show warmup status snapshot.
- Audio panel:
  - form with text + language.
  - POST should call internal service or HTTP endpoint to generate demo audio.
  - show generated audio URL/result metadata.
- Maps panel:
  - read/compose offline manifest and show packs/styles/fonts status.
  - show where PMTiles assets should be placed.
- Keep it simple MVC, no JavaScript framework.

Run:

```powershell
dotnet build src\GeoGuide.Cms\GeoGuide.Cms.csproj
```

Commit:

```text
feat(ui): add reference workflow console
```

Final response: changed files, build result, commit hash, known gaps.

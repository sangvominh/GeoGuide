# Prompt: Mobile Flow Agent

You are working in:

```text
C:\dev\personal\geoguide-agent-worktrees\mobile-flow
```

Branch:

```text
codex/ref-mobile-flow
```

Goal: connect the .NET MAUI mobile app to the reference-compatible startup, hotset, localization, offline, geofence, and narration flow.

Own these areas:

- `src/GeoGuide.Mobile/**`
- mobile-specific docs if needed

Avoid editing:

- CMS backend controllers unless a tiny DTO compatibility note is absolutely required
- dashboard docs

Required behavior:

- support startup POI load from `/api/v1/poi/load-all?lang=...`
- preserve existing sync fallback endpoints
- after location is available, call `/api/v1/poi/nearby` and `/api/v1/localizations/prepare-hotset`
- when narration needs missing target language content, call `/api/v1/localizations/on-demand`
- keep local database/offline fallback working
- keep device/local TTS fallback if backend audio is unavailable

Implementation guidance:

- Do not remove current MAUI services.
- Prefer extending `PoiApiService`, `StartupWarmupService`, `PoiRepository`, and narration-related services.
- Keep UI stable and demoable.

Run:

```powershell
dotnet build src/GeoGuide.Mobile/GeoGuide.Mobile.csproj -f net10.0-windows10.0.19041.0
```

If mobile target build is unavailable, run CMS build plus explain why mobile build could not run.

Commit with:

```text
feat(mobile): integrate reference startup and hotset flow
```

Final response must list changed files, tests run, and any known gaps.

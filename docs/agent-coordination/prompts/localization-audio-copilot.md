# Prompt: Localization + Audio Agent

You are working in:

```text
C:\dev\personal\geoguide-agent-worktrees\localization-audio
```

Branch:

```text
codex/ref-localization-audio
```

Goal: implement the teacher reference localization warmup/hotset/on-demand flow and audio/TTS API in the existing ASP.NET Core backend.

Own these areas:

- `src/GeoGuide.Cms/Controllers/Api/V1/*Localization*`
- `src/GeoGuide.Cms/Controllers/Api/V1/*Audio*`
- `src/GeoGuide.Cms/Services/*Audio*`
- optional audio/localization DTOs
- `src/GeoGuide.Cms/wwwroot/static/audio`

Avoid editing:

- POI route aliases owned by content agent
- maps, owner/RBAC, AI, mobile, dashboard docs

Required behavior:

- `POST /api/v1/localizations/prepare-hotset`
- `POST /api/v1/localizations/on-demand`
- `POST /api/v1/localizations/warmup`
- `GET /api/v1/localizations/warmup/{lang}/status`
- `GET /api/v1/audio/voices`
- `POST /api/v1/audio/tts`
- `GET /api/v1/audio/pack-manifest`
- task status endpoint or SSE-compatible placeholder
- deterministic local fallback when real Edge-TTS is unavailable

Implementation guidance:

- Use `PoiAudio` as the storage-compatible localization/audio record unless a new model is clearly necessary.
- Keep demo behavior honest: if audio is placeholder, name it as compatible/demo placeholder in API metadata.
- Do not require network or external API keys for build/demo.

Run:

```powershell
dotnet build src/GeoGuide.Cms/GeoGuide.Cms.csproj
```

Commit with:

```text
feat(audio): add localization warmup and TTS endpoints
```

Final response must list changed files, tests run, and any known gaps.

# Prompt: Maps Offline Agent

You are working in:

```text
C:\dev\personal\geoguide-agent-worktrees\maps-offline
```

Branch:

```text
codex/ref-maps-offline
```

Goal: implement PMTiles-style offline map pack endpoints from the teacher reference using safe ASP.NET Core static/range file serving.

Own these areas:

- `src/GeoGuide.Cms/Controllers/Api/V1/*Maps*`
- `src/GeoGuide.Cms/wwwroot/static/maps`
- map-specific README/docs if needed

Avoid editing:

- mobile map UI unless absolutely necessary
- POI/audio/localization/admin/AI controllers

Required behavior:

- `GET /api/v1/maps/offline-manifest`
- `GET /api/v1/maps/packs/{version}/{file}` with range request support
- `GET /api/v1/maps/styles/{path}`
- `GET /api/v1/maps/fonts/{fontstack}/{range}.pbf`
- path traversal guard for every file path
- useful `404` response explaining where assets should be placed

Implementation guidance:

- The project does not need real PMTiles committed.
- Add placeholder `.gitkeep` files if needed.
- Keep route behavior demoable even without map pack assets.

Run:

```powershell
dotnet build src/GeoGuide.Cms/GeoGuide.Cms.csproj
```

Commit with:

```text
feat(maps): add offline map pack endpoints
```

Final response must list changed files, tests run, and any known gaps.

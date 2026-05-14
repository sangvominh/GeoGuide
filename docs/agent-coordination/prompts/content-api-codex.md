# Prompt: Content API Agent

You are working in:

```text
C:\dev\personal\geoguide-agent-worktrees\content-api
```

Branch:

```text
codex/ref-content-api
```

Goal: implement the teacher reference POI/content API routes in the existing ASP.NET Core/EF Core project without breaking existing `/api/v1/pois` mobile routes.

Own these areas:

- `src/GeoGuide.Cms/Controllers/Api/V1/*Poi*`
- `src/GeoGuide.Cms/Models/Api/*Poi*`
- small DTO/helper classes needed for POI responses

Do not edit:

- MAUI mobile files
- audio/localization/maps/AI controllers
- dashboard docs except if you need a tiny API note

Required behavior:

- `GET /api/v1/poi/load-all?lang=en`
- `GET /api/v1/poi/nearby?lat=...&lng=...&radiusMeters=1500&lang=en`
- `GET /api/v1/poi/{id}?lang=en`
- authenticated `POST /api/v1/poi`
- authenticated `PUT /api/v1/poi/{id}`
- authenticated `DELETE /api/v1/poi/{id}` soft-deletes POI and cascades local content records
- response includes fallback chain target -> `en` -> `vi`
- existing `/api/v1/pois` still works

Run:

```powershell
dotnet build src/GeoGuide.Cms/GeoGuide.Cms.csproj
```

Commit with:

```text
feat(content): add reference POI API aliases
```

Final response must list changed files, tests run, and any known gaps.

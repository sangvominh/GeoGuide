# Merge Playbook

Recommended merge order:

1. `codex/ref-content-api`
2. `codex/ref-localization-audio`
3. `codex/ref-maps-offline`
4. `codex/ref-admin-owner-rbac`
5. `codex/ref-ai-advisor`
6. `codex/ref-mobile-flow`
7. `codex/ref-dashboard-docs`
8. `codex/ref-verification`

Use `codex/reference-agent-coordination` as the integration base unless the user chooses another target.

## Commands

```powershell
cd C:\dev\personal\geoguide-agent-worktrees\coordination
git merge --no-ff codex/ref-content-api
dotnet build src/GeoGuide.Cms/GeoGuide.Cms.csproj
```

Repeat branch by branch. Resolve conflicts before moving to the next branch.

## Commit Style

Feature branches should use clear commit messages:

- `feat(content): add reference POI route aliases`
- `feat(audio): add TTS and pack manifest endpoints`
- `feat(maps): add offline map manifest endpoints`
- `feat(owner): add owner registration API`
- `feat(ai): add description enhancement endpoint`
- `feat(mobile): call reference startup and hotset APIs`
- `docs(demo): align walkthrough with reference architecture`
- `test(verification): add endpoint smoke checks`

## Final Verification

Run from integration worktree:

```powershell
dotnet build src/GeoGuide.Cms/GeoGuide.Cms.csproj
dotnet build src/GeoGuide.Mobile/GeoGuide.Mobile.csproj -f net10.0-windows10.0.19041.0
```

If Docker is configured:

```powershell
docker compose up --build
```

Then smoke test:

- `/health`
- `/api/v1/pois`
- `/api/v1/poi/load-all?lang=en`
- `/api/v1/poi/nearby?lat=10.758&lng=106.704`
- `/api/v1/audio/voices`
- `/api/v1/audio/pack-manifest?lang=vi`
- `/api/v1/localizations/warmup/vi/status`
- `/api/v1/maps/offline-manifest`
- `/api/v1/admin/auth/me`
- `/api/v1/ai/enhance-description`

# Prompt: Verification Agent

You are working in:

```text
C:\dev\personal\geoguide-agent-worktrees\verification
```

Branch:

```text
codex/ref-verification
```

Goal: after feature branches are ready, verify integration and fix compile/test issues with minimal cross-cutting edits.

Own these areas:

- test or smoke-check scripts under `tools/`
- minimal compile fixes after branches are merged
- final verification notes under `docs/agent-coordination/verification-report.md`

Do not start before at least the backend feature branches are ready.

Required checks:

```powershell
dotnet build src/GeoGuide.Cms/GeoGuide.Cms.csproj
dotnet build src/GeoGuide.Mobile/GeoGuide.Mobile.csproj -f net10.0-windows10.0.19041.0
```

Smoke endpoints when the app runs:

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

Commit with:

```text
test(verification): add reference implementation checks
```

Final response must include passing/failing checks, fixes made, and residual risks.

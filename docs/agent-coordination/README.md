# GeoGuide Reference Implementation Coordination

This folder coordinates parallel AI-agent work for porting the teacher reference presentation into the current C#/.NET coursework project.

The reference HTML is:

- `C:\Users\vminh\Downloads\system-presentation-standalone.html`

The target stack remains:

- ASP.NET Core MVC/API for CMS and backend
- PostgreSQL + EF Core for persistence
- .NET MAUI for mobile
- Static/demo-compatible artifacts for features that require external services

Do not rewrite the project into FastAPI, MongoDB, React, or PWA. Implement the same product behavior and API contracts in the existing stack.

## Agent Rules

- Work only inside your assigned worktree and branch.
- Do not edit files outside your assigned ownership unless your prompt explicitly allows it.
- Do not commit build outputs, `bin`, `obj`, `.lscache`, local secrets, `.env`, or generated package/cache files.
- Prefer adding new controllers/services over changing shared startup code.
- If you must touch a shared file, write a short note in your final response and keep the diff tiny.
- Keep commits small and named by feature, for example `feat(audio): add reference TTS endpoints`.
- Before finalizing, run at least `dotnet build src/GeoGuide.Cms/GeoGuide.Cms.csproj`.
- If mobile code is touched, also run a compile check for the Windows MAUI target when available.

## Branches

All work branches are based on `codex/reference-agent-coordination`.

| Worktree | Branch | Owner Scope |
| --- | --- | --- |
| `content-api` | `codex/ref-content-api` | POI reference API aliases and DTO compatibility |
| `localization-audio` | `codex/ref-localization-audio` | localization fallback, warmup, TTS, audio tasks |
| `maps-offline` | `codex/ref-maps-offline` | PMTiles-style offline manifest and static map asset serving |
| `admin-owner-rbac` | `codex/ref-admin-owner-rbac` | owner portal, admin auth API, permissions/RBAC |
| `ai-advisor` | `codex/ref-ai-advisor` | AI enhancement endpoint and usage limiting |
| `mobile-flow` | `codex/ref-mobile-flow` | MAUI startup, hotset, geofence, offline/audio integration |
| `dashboard-docs` | `codex/ref-dashboard-docs` | CMS dashboard, demo links, documentation and demo script |
| `verification` | `codex/ref-verification` | integration test pass, compile fixes, merge assistance |

## Definition of Done

- The implemented route or UI behavior matches the reference section name and intent.
- Existing MVP routes continue to work.
- Build succeeds.
- The agent final message lists changed files, test commands, and known gaps.

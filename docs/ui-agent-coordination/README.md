# GeoGuide UI Expansion Coordination

This round exists because the previous round mostly implemented backend/reference API contracts. The goal now is to make the product visible and demoable through UI surfaces.

Base branch:

- `codex/reference-implementation-final`

Coordination branch:

- `codex/ui-agent-coordination`

Root:

```text
C:\dev\personal\geoguide-ui-worktrees
```

## What Must Change

The user should be able to open the CMS/mobile and immediately see:

- a new admin reference console, not just old Bootstrap cards
- a seller/owner portal UI, not only owner APIs
- admin pending owner submission review UI
- localization/audio/map/AI workflows as visible controls
- mobile app evidence for startup sync, hotset, on-demand localization, narration fallback, and offline cache

## Rules

- Work only in your assigned worktree.
- Do not modify another agent's owned files unless your prompt explicitly allows it.
- Prefer MVC controllers/views for CMS UI. Keep APIs intact.
- Keep styling practical and coursework-demo friendly. Do not create a marketing landing page.
- Do not remove existing admin CRUD pages.
- Build before committing.
- Commit on your current branch with the requested message.

## Definition Of Done

- A human can see the feature in UI, not only in API.
- Existing login/admin flows still work.
- `dotnet build src/GeoGuide.Cms/GeoGuide.Cms.csproj` passes for CMS UI work.
- Mobile UI work builds the Windows MAUI target where possible.
- Final response includes changed files, test command, commit hash, and known gaps.

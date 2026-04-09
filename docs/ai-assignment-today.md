# AI Assignment For Today

## Objective

Finish a demoable MVP in one day using four isolated git worktrees:

- Mobile stream
- CMS stream
- Analysis stream
- Integration stream

This is a one-day delivery plan. Every stream must optimize for a stable demo, not full feature completeness.

## Shared Rules

- Do not change branch in your assigned workspace
- Do not edit files outside your owned scope
- Do not rename shared contract fields unless `docs/mvp-parallel-plan.md` is updated first
- Commit small, focused changes only
- Push only to your assigned branch
- Do not merge into `main`

## Workspace Map

- `C:\dev\personal\GeoGuide-mobile` -> `codex/mobile-mvp`
- `C:\dev\personal\GeoGuide-cms` -> `codex/cms-mvp`
- `C:\dev\personal\GeoGuide-analysis` -> `codex/analysis-mvp`
- `C:\dev\personal\GeoGuide-integration` -> `codex/integration-mvp`

## Mobile Stream

### Workspace

- `C:\dev\personal\GeoGuide-mobile`
- Branch: `codex/mobile-mvp`

### Allowed Scope

- `MauiApp1/`
- mobile-side service abstractions
- mobile local cache
- mobile UI and navigation

### Forbidden Scope

- backend or CMS project files
- non-mobile infrastructure unless explicitly needed for integration

### Required Deliverables

- map screen loads POIs from backend API or local fallback
- mobile model matches shared POI contract
- location is retrieved and used for nearest-POI calculation
- manual narration trigger works
- automatic trigger by distance works if stable enough
- cooldown prevents repeated playback spam
- any required mobile env/config is documented

### Priority Order

1. Load POIs from API
2. Render POIs in map/list
3. Determine nearest POI by distance
4. Trigger narration manually
5. Add auto trigger with cooldown
6. Add offline fallback

### Commit Prefix

- `mobile: ...`

## CMS Stream

### Workspace

- `C:\dev\personal\GeoGuide-cms`
- Branch: `codex/cms-mvp`

### Allowed Scope

- backend API project
- CMS web admin project
- server-side database schema and seed data

### Forbidden Scope

- `MauiApp1/`
- mobile-only logic

### Required Deliverables

- PostgreSQL schema for POI and playback log
- seed data for demo
- API endpoint `GET /api/pois`
- API endpoint `GET /api/pois/{id}`
- API endpoint `POST /api/logs/playback`
- basic CMS UI to list/create/edit/delete POIs
- payloads match shared POI contract

### Priority Order

1. Database schema
2. Seed data
3. `GET /api/pois`
4. `GET /api/pois/{id}`
5. Basic CMS CRUD POI
6. `POST /api/logs/playback`

### Commit Prefix

- `cms: ...`

## Analysis Stream

### Workspace

- `C:\dev\personal\GeoGuide-analysis`
- Branch: `codex/analysis-mvp`

### Allowed Scope

- `docs/`
- architecture diagrams
- ERD
- workflow and use-case docs
- demo script

### Forbidden Scope

- feature code for mobile
- feature code for CMS

### Required Deliverables

- ERD for MVP scope
- architecture overview
- mobile workflow from app open to narration
- API contract summary
- acceptance criteria for the MVP
- demo script for final presentation
- integration checklist for final validation

### Priority Order

1. ERD
2. architecture overview
3. acceptance criteria
4. demo script
5. test/checklist docs

### Commit Prefix

- `analysis: ...`

## Integration Stream

### Workspace

- `C:\dev\personal\GeoGuide-integration`
- Branch: `codex/integration-mvp`

### Allowed Scope

- merge and conflict resolution
- integration-only fixes
- build/test validation
- shared configuration adjustments

### Forbidden Scope

- independent feature development that belongs in a stream branch

### Required Deliverables

- merge `codex/mobile-mvp`
- merge `codex/cms-mvp`
- merge `codex/analysis-mvp`
- resolve conflicts without breaking ownership boundaries
- run build/test checks
- document final integration status

### Priority Order

1. merge mobile
2. merge CMS
3. merge analysis
4. fix integration breaks
5. run final validation

### Commit Prefix

- `integration: ...`

## File Ownership Guardrail

To avoid overlap:

- `MauiApp1/` is owned by Mobile
- server/API/CMS folders are owned by CMS
- `docs/` is owned by Analysis
- shared contract docs are proposed by any stream but finalized through Integration if conflicts appear

## End-Of-Day Definition Of Done

- Mobile can show and use POI data from backend or seeded fallback
- CMS can manage demo POIs
- Shared POI contract is consistent across streams
- Playback log endpoint exists
- Architecture and demo documentation are ready
- Integration branch builds and is ready for final demo validation

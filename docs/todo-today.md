# CMS Todo Today

Branch: `codex/cms-mvp`
Workspace: `C:\dev\personal\GeoGuide-cms`

## Rules

- Do not change branch
- Do not edit `MauiApp1/`
- Follow `docs/mvp-parallel-plan.md`
- Commit with prefix `cms:`
- Push only to `codex/cms-mvp`

## Do In This Order

- Create backend/API project if not present
- Create PostgreSQL schema for POI and playback log
- Add seed data for demo
- Implement `GET /api/pois`
- Implement `GET /api/pois/{id}`
- Implement basic CMS page for list/create/edit/delete POIs
- Implement `POST /api/logs/playback`
- Document run steps and env vars

## Done When

- Backend can return POIs matching shared contract
- CMS can manage demo POIs
- Playback log endpoint accepts requests
- Branch runs or builds cleanly enough for integration

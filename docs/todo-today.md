# Mobile Todo Today

Branch: `codex/mobile-mvp`
Workspace: `C:\dev\personal\GeoGuide-mobile`

## Rules

- Do not change branch
- Only edit mobile code, mainly `MauiApp1/`
- Follow `docs/mvp-parallel-plan.md`
- Commit with prefix `mobile:`
- Push only to `codex/mobile-mvp`

## Do In This Order

- Align mobile POI model with `docs/mvp-parallel-plan.md`
- Add mobile service to load POIs from backend API
- Add local fallback cache for POIs if time allows
- Show POIs in map/list UI
- Get current location and calculate nearest POI
- Add manual narration trigger
- Add auto-trigger by distance with cooldown if stable
- Document any required config or env vars

## Done When

- App can show POIs from API or fallback
- App can determine nearest POI
- App can trigger narration at least manually
- Branch builds without breaking the project

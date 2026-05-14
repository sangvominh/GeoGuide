# Task Board

## 1. Content API

Goal: make the C# backend expose the POI lifecycle routes used by the teacher reference.

- Add `/api/v1/poi/load-all?lang=...`
- Add `/api/v1/poi/nearby?lat=...&lng=...&radiusMeters=...`
- Add `/api/v1/poi/{id}`
- Add authenticated create/update/delete aliases where practical
- Preserve existing `/api/v1/pois` behavior
- Include fallback localization data in responses

## 2. Localization + Audio

Goal: implement the reference hotset/on-demand/warmup and TTS contracts.

- Add `/api/v1/localizations/prepare-hotset`
- Add `/api/v1/localizations/on-demand`
- Add `/api/v1/localizations/warmup`
- Add `/api/v1/localizations/warmup/{lang}/status`
- Add `/api/v1/audio/voices`
- Add `/api/v1/audio/tts`
- Add `/api/v1/audio/pack-manifest`
- Add simple task status endpoint or SSE-compatible placeholder

## 3. Maps Offline

Goal: implement PMTiles-style map pack endpoints using safe static serving.

- Add `/api/v1/maps/offline-manifest`
- Add `/api/v1/maps/packs/{version}/{file}` with range support
- Add `/api/v1/maps/styles/{path}`
- Add `/api/v1/maps/fonts/{fontstack}/{range}.pbf`
- Add path traversal guard
- Add README notes for where to place PMTiles/style/font files

## 4. Admin + Owner + RBAC

Goal: match the reference owner portal and dynamic permission story in the existing Identity setup.

- Add `/api/v1/admin/auth/me`
- Add `/api/v1/admin/auth/change-password`
- Add `/api/v1/admin/auth/register-owner`
- Add owner POI/submission endpoint using pending approval
- Return permission domains in `me`
- Keep `SystemAdmin` and `PoiTenant` roles compatible with existing UI

## 5. AI Advisor

Goal: provide a demo-safe AI enhancement endpoint.

- Add `/api/v1/ai/enhance-description`
- Add daily usage limit of 10 per owner/user
- Use deterministic local enhancement if no Gemini key exists
- Optionally support Gemini through configuration
- Log usage enough for demo evidence

## 6. Mobile Flow

Goal: make MAUI client consume the reference-compatible behavior.

- Support `/api/v1/poi/load-all` in addition to existing sync endpoints
- Prepare hotset after startup location is available
- Request on-demand localization when a POI lacks target language content
- Keep local cache/offline fallback working
- Keep narration flow functional even when backend audio is unavailable

## 7. Dashboard + Docs

Goal: make the demo easy to control and explain.

- Add CMS dashboard cards for Content, Audio, Localization, Maps, Owner, AI
- Add links to reference presentation and key API endpoints
- Add `docs/architecture/reference-alignment.md`
- Update `docs/demo/demo-script.md`
- Add quick verification checklist

## 8. Verification

Goal: integrate after feature branches are ready.

- Build CMS
- Run mobile compile if feasible
- Smoke test key endpoints
- Resolve cross-branch compile conflicts
- Produce final merge order and residual risk list

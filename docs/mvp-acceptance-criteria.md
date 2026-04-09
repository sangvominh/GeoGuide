# MVP Acceptance Criteria

## Acceptance Boundary

The MVP is accepted when the team can demonstrate one end-to-end flow across CMS, backend API, mobile, and documentation using the shared contract in `docs/mvp-parallel-plan.md`.

## Contract Compliance

- All POI payloads exposed by the backend use the exact shared field names:
  - `id`
  - `name`
  - `description`
  - `latitude`
  - `longitude`
  - `triggerRadiusMeters`
  - `priority`
  - `categoryKey`
  - `categoryLabel`
  - `imageUrl`
  - `mapUrl`
  - `audioUrl`
  - `ttsScript`
  - `languageCode`
  - `isActive`
  - `updatedAt`
- Playback log submissions use the exact shared field names:
  - `poiId`
  - `playedAt`
  - `triggerType`
  - `durationSeconds`
  - `deviceId`
- No stream renames or removes shared fields without first updating `docs/mvp-parallel-plan.md`.

## CMS + Backend Acceptance Criteria

- CMS can create a POI with the minimum required fields for the shared contract.
- CMS can edit an existing POI and the updated data is returned by the API.
- CMS can mark a POI inactive.
- `GET /api/pois` returns at least one active demo POI.
- `GET /api/pois/{id}` returns the correct POI by UUID.
- `POST /api/logs/playback` accepts a valid playback log payload and persists it.

## Mobile Acceptance Criteria

- Mobile can load POIs from the backend API.
- Mobile can display nearby POIs on the map.
- Mobile can use cached POIs when the API is unavailable after one successful sync.
- Mobile can identify the nearest POI using coordinate distance.
- Mobile can start narration manually for a selected POI.
- If trigger-radius playback is enabled, mobile starts narration only when the user is within `triggerRadiusMeters`.
- Mobile can submit a playback log after narration completes.

## Demo Acceptance Criteria

- One POI is created or confirmed in CMS before the demo run.
- The same POI is visible in mobile after API fetch.
- A narration action is demonstrated for that POI.
- A playback log event is shown as accepted by the backend.
- The fallback path is ready:
  - manual playback if GPS is unstable
  - cached POIs if the API is temporarily unavailable

## Documentation Acceptance Criteria

- Architecture, ERD, workflow, acceptance criteria, and demo script exist in `docs/`.
- Documentation reflects the same POI and playback log contracts as `docs/mvp-parallel-plan.md`.
- Any schema example in `docs/` uses fields that can map cleanly to the shared contract.

## Explicit Non-Acceptance Conditions

- Backend returns differently named POI fields than the shared contract.
- Mobile depends on fields that do not exist in the shared contract.
- CMS demo data cannot be consumed by mobile without manual remapping.
- Playback logging is omitted entirely from the end-to-end demonstration.

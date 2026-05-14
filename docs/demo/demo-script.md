# MVP Demo Script

## Demo Goal

Show one integrated flow where CMS manages a POI, mobile consumes it, narration is triggered, and playback logging reaches the backend.

## Demo Setup

- Prepare at least one active POI in the database and CMS.
- Confirm the POI has valid latitude, longitude, `triggerRadiusMeters`, `ttsScript`, and `isActive = true`.
- Confirm backend API is reachable by the mobile app.
- Confirm the mobile app has location permission if GPS trigger will be demonstrated.

## Recommended Demo POI

- Use one centrally located POI with a short narration asset.
- Keep `triggerRadiusMeters` between `50` and `100` for stable demo behavior.
- Keep one backup POI available in case the first record is edited incorrectly.

## Demo Sequence

### 1. Show Reference Architecture

Say:

> Before diving into the live system, let's look at the target reference architecture and how it aligns with our .NET stack.

Do:

1. Open the CMS Dashboard.
2. Click the **"Reference Presentation"** link in the top banner to open `/system-presentation-standalone.html`.
3. Briefly walk through the key modules: Content, Audio, Localization, Maps, RBAC, AI, and Mobile Offline.
4. Explain that our implementation uses .NET/PostgreSQL/MAUI instead of FastAPI/MongoDB/React.

### 2. Show CMS Data

Say:

> The CMS is the source of truth for POI content. This record follows the same contract used by the API and mobile app.

Do:

1. Open the POI list in CMS.
2. Open one POI detail.
3. Highlight name, coordinates, trigger radius, category, narration field, and active status.

### 2. Show API Consumption

Say:

> The mobile app reads the same POI data from the backend API, without custom remapping for the demo.

Do:

1. Open or call `GET /api/pois`.
2. Show that the POI appears with the shared field names.
3. If available, open `GET /api/pois/{id}` for the same record.

### 3. Show Mobile Map

Say:

> The mobile app loads nearby POIs from the API and keeps a local cache for offline fallback.

Do:

1. Open the mobile map screen.
2. Show the target POI on the map.
3. Optionally note that the record was fetched from the backend and cached locally.

### 4. Show Playback

Say:

> Narration can be started manually, and if the GPS scenario is stable, it can also trigger by radius.

Do:

1. Preferred path: tap the POI and start playback manually.
2. Optional path: move into the trigger radius and show automatic playback.
3. Keep the TTS narration short and audible enough to confirm success.

### 5. Show Playback Logging

Say:

> After narration finishes, the mobile app posts a playback event back to the backend.

Do:

1. Show the request payload for `POST /api/logs/playback` or a backend confirmation.
2. Confirm `poiId`, `playedAt`, `triggerType`, `durationSeconds`, and `deviceId` are present.
3. If possible, show the saved record in the database or admin view.

## Fallback Script

### If GPS Is Unstable

Say:

> GPS accuracy is unstable in this environment, so we are using the supported manual playback path for the MVP.

Do:

1. Use manual POI selection.
2. Continue with playback log submission.

### If API Is Temporarily Unavailable

Say:

> The app has already cached POIs locally, so the user can still browse and play a previously synced record.

Do:

1. Show cached POI visibility in mobile.
2. If logging cannot be posted live, explain that the backend endpoint is part of the integrated target path.

## Demo Exit Criteria

- One POI is shown consistently across CMS, API, and mobile.
- One narration action succeeds.
- One playback log event is demonstrated or visibly accepted by the backend.

## Quick Verification Checklist

- [ ] `/system-presentation-standalone.html` loads from the dashboard.
- [ ] `GET /api/pois` returns a valid JSON array of POIs.
- [ ] `GET /api/pois/{id}` returns the expected POI detail view.
- [ ] `POST /api/logs/playback` accepts the correct payload.

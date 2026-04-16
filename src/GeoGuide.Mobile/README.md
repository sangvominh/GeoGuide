# Mobile MVP Config

The mobile app reads POIs from the backend API first, then falls back to local cache, then bundled seed data.

## Supported environment variables

- `POI_API_BASE_URL`
  - Backend base URL.
  - Example for Android emulator: `http://10.0.2.2:8080/`
  - Example for Windows local run: `http://localhost:8080/`
- `POI_API_TIMEOUT_SECONDS`
  - Optional HTTP timeout in seconds.
  - Default: `10`
- `POI_PLAYBACK_LOGS_ENABLED`
  - Optional flag for `POST /api/logs/playback`.
  - Set to `false` to skip playback log posting during local demos.

## Runtime behavior

- `GET /api/pois` is the primary POI source.
- Successful API reads are cached into app local storage.
- If the API is unavailable, the app loads cached POIs.
- If no cache exists yet, the app loads bundled fallback POIs from `Resources/Raw/poi-fallback.json`.
- Narration uses device TTS with `ttsScript` first, then falls back to `name + description`.

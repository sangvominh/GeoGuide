# MVP Parallel Delivery Plan

## Goal

Deliver a demoable coursework MVP in one day with three parallel workstreams:

- Mobile app
- CMS + Backend API
- Analysis + Documentation

The objective is not full feature completion. The objective is one integrated MVP that can be demonstrated end-to-end.

## MVP Scope

### In Scope

- Mobile .NET MAUI app shows map and nearby POIs
- Mobile reads POIs from backend API
- Mobile can cache POIs locally for basic offline fallback
- Mobile can determine nearest POI by distance
- Mobile can trigger narration manually and, if stable, automatically when inside trigger radius
- CMS can create, edit, delete, and list POIs
- Backend API can expose POI data for the mobile app
- Backend API can receive basic playback logs
- Analysis stream produces ERD, architecture, workflow, acceptance criteria, and demo script

### Out Of Scope For Today

- Full production-grade background tracking for both Android and iOS
- Full geofencing with native platform services
- Full analytics dashboard
- Complex auth/roles
- Full offline sync conflict resolution
- Final-quality QR flow unless the core mobile flow is already stable

## Shared Domain Contract

All streams must use the same POI contract.

```json
{
  "id": "uuid",
  "name": "string",
  "description": "string",
  "latitude": 10.77,
  "longitude": 106.69,
  "triggerRadiusMeters": 80,
  "priority": 1,
  "categoryKey": "attraction",
  "categoryLabel": "Tham quan",
  "imageUrl": "string",
  "mapUrl": "string",
  "audioUrl": "string",
  "ttsScript": "string",
  "languageCode": "vi-VN",
  "isActive": true,
  "updatedAt": "2026-04-09T10:00:00Z"
}
```

Playback log contract:

```json
{
  "poiId": "uuid",
  "playedAt": "2026-04-09T10:30:00Z",
  "triggerType": "gps|qr|manual",
  "durationSeconds": 25,
  "deviceId": "string"
}
```

## Stream Ownership

### Mobile Stream

Owns:

- `src/GeoGuide.Mobile/`
- local cache and mobile service abstractions
- map, location, geofence-by-distance, playback flow

Must not own:

- CMS UI
- backend admin models outside shared contract discussion

### CMS Stream

Owns:

- backend API project
- web admin project
- server database schema
- seed data and API payloads

Must not own:

- MAUI UI
- mobile-only caching and playback internals

### Analysis Stream

Owns:

- `docs/`
- architecture, ERD, workflow, report assets, demo script
- contract validation and acceptance criteria

Must not own:

- feature code in mobile or CMS unless asked to resolve integration documentation gaps

## Required API Endpoints

- `GET /api/pois`
- `GET /api/pois/{id}`
- `POST /api/logs/playback`

Optional only if time remains:

- `GET /api/tours`
- `GET /api/pois/nearby`

## Integration Rule

No stream may rename shared fields without updating this file first and notifying the other streams.

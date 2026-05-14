# GeoGuide CMS

ASP.NET Core MVC app cung cap:

- CMS CRUD cho POI
- API `GET /api/pois`
- API `GET /api/pois/{id}`
- API `POST /api/logs/playback`

## Env vars

- `ConnectionStrings__DefaultConnection`: chuoi ket noi PostgreSQL
- `POSTGRES_CONNECTION`: fallback neu khong dung key tren
- `ASPNETCORE_URLS`: tuy chon, vi du `http://localhost:5055`

## Run

1. Tao database PostgreSQL, vi du `geoguide_cms`
2. Cap nhat connection string
3. Chay app:

```powershell
dotnet run --project src/GeoGuide.Cms
```

App tu dong migrate schema va seed 4 POI demo khi khoi dong.

## Useful URLs

- `/`
- `/PoisAdmin`
- `/api/pois`

## What changed by role

- **User App**: Target backend API consumption is stable.
- **Seller/Owner**: Endpoints for POI submission and AI Advisor.
- **Admin**: Endpoints for review, approval, and rejection.
- **Backend/API**: Implemented reference workflow capabilities like AI Advisor and Localization via UI consoles.
- **Demo limitations**: AI Advisor and TTS APIs are fake implementations intended for UI demo purposes, utilizing hardcoded logic to avoid external dependencies.

## UI-Complete vs API/Placeholder

- **UI-Complete**: Admin Dashboard, Control Center, Workflow Console.
- **API/Placeholder**: AI text generation, text-to-speech audio rendering, offline map tiles generation. All these are mocked via deterministic logic or standard fake JSON responses.

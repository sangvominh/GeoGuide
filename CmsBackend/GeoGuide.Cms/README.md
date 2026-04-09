# GeoGuide CMS Backend

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
dotnet run --project CmsBackend/GeoGuide.Cms
```

App tu dong migrate schema va seed 4 POI demo khi khoi dong.

## Useful URLs

- `/`
- `/PoisAdmin`
- `/api/pois`

# GeoGuide

Repo nay duoc to chuc lai de de hieu va de van hanh hon.

## Thu muc chinh

- `src/GeoGuide.Mobile/`: ung dung mobile .NET MAUI
- `src/GeoGuide.Cms/`: CMS web admin va backend API
- `docs/product/`: PRD va acceptance criteria
- `docs/architecture/`: architecture, ERD, workflow
- `docs/database/`: script va tai lieu database
- `docs/demo/`: demo script
- `docs/internal/`: tai lieu phuc vu dot song song truoc day
- `tools/`: script ho tro local

## Bat dau nhanh

### Chay CMS

```powershell
dotnet run --project src/GeoGuide.Cms
```

### Chay mobile

Mo:

- `src/GeoGuide.Mobile/GeoGuide.Mobile.slnx`

hoac:

```powershell
dotnet build .\src\GeoGuide.Mobile\GeoGuide.Mobile.csproj -f net10.0-windows10.0.19041.0
```

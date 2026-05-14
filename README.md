# GeoGuide

Repo nay duoc to chuc lai de de hieu va de van hanh hon.

## Thu muc chinh

- `src/GeoGuide.Mobile/`: ung dung mobile .NET MAUI
- `src/GeoGuide.Cms/`: CMS web admin va backend API
- `docs/product/`: PRD va acceptance criteria
- `docs/architecture/`: architecture, ERD, workflow, va reference alignment
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

## What changed by role

- **User App**: Mobile UI with debug panel, map interface, sync status, offline cache fallback, and manual narration playback.
- **Seller/Owner**: Seller portal to register, submit POIs, check status of submissions, and use AI Advisor to enhance POI descriptions.
- **Admin**: Control center dashboard, review and approve/reject POI submissions, view system status.
- **Backend/API**: Authentication, role-based access control (RBAC), POI CRUD, fake AI Advisor endpoint (local demo fallback), and Reference Workflow Console for Localization/Audio/Map API simulation.
- **Demo limitations**: GPS trigger is simulated or manual. AI Advisor uses a deterministic local fallback if no Gemini key is provided. Reference Workflow Consoles simulate third-party service responses.

## UI-Complete vs API/Placeholder

- **UI-Complete**: Admin Control Center, Seller Portal, Admin Review Submission, Mobile Debug Panel, AI Advisor UI form, Reference Workflow Console UI.
- **API/Placeholder**: AI Advisor real processing (has local fallback), Audio/TTS generation (simulated in console), Offline Map tile packaging (simulated in console), Localization translation (simulated in console).

# Prompt: Admin Console UI Agent

```powershell
cd C:\dev\personal\geoguide-ui-worktrees\admin-console
```

Read this prompt and implement only your scope.

Goal: make the CMS dashboard visibly different and useful. The current dashboard still feels like the old app. Build a first-screen control center that proves the reference system exists.

Own:

- `src/GeoGuide.Cms/Views/Home/Index.cshtml`
- dashboard-specific CSS in `src/GeoGuide.Cms/wwwroot/css/site.css` if needed
- small dashboard view model/controller changes only if necessary

Avoid:

- seller portal pages
- admin review pages
- AI advisor pages
- workflow console pages
- mobile files

Requirements:

- Hero/control center with clear title: "GeoGuide Reference Control Center"
- Show role surfaces: User App, Seller/Owner, Admin, Backend/API
- Add launch buttons:
  - Seller Portal: `/OwnerPortal`
  - Register Seller: `/OwnerPortal/Register`
  - Admin Review: `/AdminReview`
  - Workflow Console: `/ReferenceConsole`
  - AI Advisor: `/AiAdvisor`
- Add visible module status cards:
  - Content API
  - Localization
  - Audio/TTS
  - Maps Offline
  - Owner Portal
  - AI Advisor
  - Mobile Offline
- Use real links to endpoints:
  - `/api/v1/poi/load-all?lang=en`
  - `/api/v1/audio/voices`
  - `/api/v1/maps/offline-manifest`
- Keep old management links to POIs, Tours, Analytics.

Run:

```powershell
dotnet build src\GeoGuide.Cms\GeoGuide.Cms.csproj
```

Commit:

```text
feat(ui): add reference admin control center
```

Final response: changed files, build result, commit hash, known gaps.

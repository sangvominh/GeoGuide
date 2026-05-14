# Prompt: Seller Portal UI Agent

```powershell
cd C:\dev\personal\geoguide-ui-worktrees\seller-portal
```

Read this prompt and implement only your scope.

Goal: build a visible Seller/Owner Portal UI. The owner APIs exist, but the user cannot see or use seller functionality from the browser. Create MVC pages that make it real.

Own:

- `src/GeoGuide.Cms/Controllers/OwnerPortalController.cs`
- `src/GeoGuide.Cms/Views/OwnerPortal/**`
- owner portal view models under `src/GeoGuide.Cms/Models/**`

Avoid:

- admin review controller/views
- dashboard except a tiny link if absolutely necessary
- mobile files
- API controllers unless a tiny helper is unavoidable

Requirements:

- `GET /OwnerPortal`
  - If not signed in, show seller landing/login/register links.
  - If signed in as `PoiTenant`, show seller dashboard with owned POIs and statuses.
- `GET /OwnerPortal/Register`
  - visible seller registration form.
  - It can post to an MVC action that uses Identity/UserManager directly.
- `POST /OwnerPortal/Register`
  - creates tenant + owner user with `PoiTenant` role.
  - handle duplicate tenant slug/email gracefully.
- `GET /OwnerPortal/Pois/Create`
  - seller POI submission form.
- `POST /OwnerPortal/Pois/Create`
  - creates POI assigned to seller tenant.
  - `ApprovalStatus = PendingApproval`, `IsActive = false`.
- `GET /OwnerPortal/Pois/Edit/{id}` and `POST`
  - only allow owner to edit their POIs.
- UI must show status badges: Pending, Approved, Rejected.

Run:

```powershell
dotnet build src\GeoGuide.Cms\GeoGuide.Cms.csproj
```

Commit:

```text
feat(ui): add seller owner portal
```

Final response: changed files, build result, commit hash, known gaps.

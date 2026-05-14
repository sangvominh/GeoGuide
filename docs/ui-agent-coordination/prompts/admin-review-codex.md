# Prompt: Admin Review UI Agent

```powershell
cd C:\dev\personal\geoguide-ui-worktrees\admin-review
```

Read this prompt and implement only your scope.

Goal: build visible admin UI for reviewing seller/owner POI submissions. The approve/reject APIs exist, but admin needs a page.

Own:

- `src/GeoGuide.Cms/Controllers/AdminReviewController.cs`
- `src/GeoGuide.Cms/Views/AdminReview/**`
- admin review view models

Avoid:

- seller portal pages
- dashboard except tiny nav link if absolutely necessary
- mobile files

Requirements:

- `GET /AdminReview`
  - only `SystemAdmin`
  - list POIs with `PendingApproval`, plus recently approved/rejected if easy
  - show owner/tenant, name, category, address/map, updated time
- `POST /AdminReview/Approve/{id}`
  - approve POI, set active true.
- `POST /AdminReview/Reject/{id}`
  - reject POI. Use `Rejected` enum if available; otherwise add it safely and ensure build/migrations are not required unless needed.
- Add approve/reject buttons with anti-forgery token.
- Link each POI to normal `PoisAdmin/Edit`.

Run:

```powershell
dotnet build src\GeoGuide.Cms\GeoGuide.Cms.csproj
```

Commit:

```text
feat(ui): add admin seller submission review
```

Final response: changed files, build result, commit hash, known gaps.

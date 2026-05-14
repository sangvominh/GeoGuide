# Prompt: Admin + Owner + RBAC Agent

You are working in:

```text
C:\dev\personal\geoguide-agent-worktrees\admin-owner-rbac
```

Branch:

```text
codex/ref-admin-owner-rbac
```

Goal: implement the teacher reference admin auth, owner portal, and permission story using the existing ASP.NET Identity setup.

Own these areas:

- `src/GeoGuide.Cms/Controllers/Api/V1/*Admin*`
- `src/GeoGuide.Cms/Controllers/Api/V1/*Owner*`
- `src/GeoGuide.Cms/Security/*`
- `src/GeoGuide.Cms/Services/CmsAccessService.cs`
- owner/RBAC-specific DTOs

Avoid editing:

- POI API aliases except where owner submission must create pending POIs
- audio/localization/maps/AI/mobile/dashboard

Required behavior:

- `GET /api/v1/admin/auth/me`
- `POST /api/v1/admin/auth/change-password`
- `POST /api/v1/admin/auth/register-owner`
- owner endpoint to create a pending POI/submission
- owner endpoint to update only owned POIs
- permission domains in API response: content, audio, admin, owner, ai_advisor, localization, maps, analytics, tours
- existing MVC login/logout and roles continue to work

Implementation guidance:

- Keep `SystemAdmin` and `PoiTenant` compatible.
- If adding dynamic permissions, prefer computed permissions over schema-heavy migrations unless necessary.
- Owner-created POIs should be pending approval and inactive until admin approval.

Run:

```powershell
dotnet build src/GeoGuide.Cms/GeoGuide.Cms.csproj
```

Commit with:

```text
feat(owner): add owner portal auth and permission APIs
```

Final response must list changed files, tests run, and any known gaps.

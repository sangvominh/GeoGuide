# Prompt: Demo Polish Docs Agent

```powershell
cd C:\dev\personal\geoguide-ui-worktrees\demo-polish-docs
```

Read this prompt and implement only your scope.

Goal: make it clear what changed by role and how to demo it. The user is confused because backend work is invisible.

Own:

- `README.md`
- `src/GeoGuide.Cms/README.md`
- `docs/demo/demo-script.md`
- `docs/architecture/reference-alignment.md`
- add new docs if helpful

Avoid:

- source code except tiny comment-free doc links if absolutely needed

Requirements:

- Add a "What changed by role" section:
  - User App
  - Seller/Owner
  - Admin
  - Backend/API
  - Demo limitations
- Add a demo path:
  1. login admin
  2. open dashboard/control center
  3. register seller
  4. seller submits POI
  5. admin approves
  6. mobile sync/offline narrative
  7. AI advisor/audio/localization/map console
- Clearly state which features are UI-complete and which are API/demo-placeholder.

Run:

```powershell
dotnet build src\GeoGuide.Cms\GeoGuide.Cms.csproj
```

Commit:

```text
docs(demo): explain visible role-based workflows
```

Final response: changed files, build result, commit hash, known gaps.

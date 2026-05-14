# Prompt: Dashboard + Docs Agent

You are working in:

```text
C:\dev\personal\geoguide-agent-worktrees\dashboard-docs
```

Branch:

```text
codex/ref-dashboard-docs
```

Goal: make the implemented reference architecture easy to demo and explain.

Own these areas:

- `src/GeoGuide.Cms/Views/Home/Index.cshtml`
- `src/GeoGuide.Cms/wwwroot/css/site.css`
- `src/GeoGuide.Cms/wwwroot/system-presentation-standalone.html`
- `docs/**`
- root `README.md`
- `src/GeoGuide.Cms/README.md`

Avoid editing:

- backend controllers/services
- mobile implementation

Required behavior:

- CMS dashboard links to teacher presentation artifact
- dashboard shows module cards for Content, Audio, Localization, Maps, Owner/Admin/RBAC, AI, Mobile Offline
- docs map every teacher reference section to implemented code
- demo script walks through the end-to-end flow
- quick verification checklist includes all reference endpoints

Implementation guidance:

- Copy the teacher HTML into `wwwroot` if it is not already there.
- Keep docs honest: mark demo-compatible placeholders clearly.
- Do not claim FastAPI/MongoDB/React if project uses .NET/PostgreSQL/MAUI.

Run:

```powershell
dotnet build src/GeoGuide.Cms/GeoGuide.Cms.csproj
```

Commit with:

```text
docs(demo): add reference implementation guide
```

Final response must list changed files, tests run, and any known gaps.

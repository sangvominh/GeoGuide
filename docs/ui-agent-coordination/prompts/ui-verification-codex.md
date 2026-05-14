# Prompt: UI Verification Agent

```powershell
cd C:\dev\personal\geoguide-ui-worktrees\ui-verification
```

Read this prompt and implement only after the UI feature branches have commits.

Goal: verify the UI integration after merge.

Required checks:

```powershell
dotnet build src\GeoGuide.Cms\GeoGuide.Cms.csproj
dotnet build src\GeoGuide.Mobile\GeoGuide.Mobile.csproj -f net10.0-windows10.0.19041.0
```

Runtime smoke pages:

- `/`
- `/OwnerPortal`
- `/OwnerPortal/Register`
- `/AdminReview`
- `/ReferenceConsole`
- `/AiAdvisor`
- `/api/v1/poi/load-all?lang=en`

Create or update:

- `docs/ui-agent-coordination/ui-verification-report.md`

Commit:

```text
test(ui): add visible workflow verification report
```

Final response: passing/failing checks, fixes made, residual risks.

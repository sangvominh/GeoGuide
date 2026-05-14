# UI Merge Playbook

Recommended merge order:

1. `codex/ui-admin-console`
2. `codex/ui-seller-portal`
3. `codex/ui-admin-review`
4. `codex/ui-workflow-console`
5. `codex/ui-ai-advisor`
6. `codex/ui-mobile-demo`
7. `codex/ui-demo-polish-docs`
8. `codex/ui-verification`

Use an integration worktree based on `codex/ui-agent-coordination`.

## Build Checks

```powershell
dotnet build src\GeoGuide.Cms\GeoGuide.Cms.csproj
dotnet build src\GeoGuide.Mobile\GeoGuide.Mobile.csproj -f net10.0-windows10.0.19041.0
```

## Runtime Pages To Check

- `/`
- `/OwnerPortal`
- `/OwnerPortal/Register`
- `/AdminReview`
- `/ReferenceConsole`
- `/AiAdvisor`
- `/system-presentation-standalone.html`

## Expected Outcome

The user should not need to inspect JSON endpoints to believe the system changed. The CMS should visibly expose admin, seller, AI, audio/localization/maps workflows.

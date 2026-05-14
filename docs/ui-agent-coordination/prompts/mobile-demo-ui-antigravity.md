# Prompt: Mobile Demo UI Agent

```powershell
cd C:\dev\personal\geoguide-ui-worktrees\mobile-demo-ui
```

Read this prompt and implement only your scope.

Goal: make the MAUI app visibly show the new reference behavior. Previous work added service calls, but the user cannot easily see what changed.

Own:

- `src/GeoGuide.Mobile/**`

Avoid:

- CMS backend files unless absolutely necessary

Requirements:

- Add a visible debug/demo status panel to the main mobile experience or a separate page.
- Show:
  - API base URL
  - selected language
  - last startup sync result
  - POI count in local cache
  - last nearby/hotset result
  - on-demand localization status
  - narration fallback mode
  - offline log queue count if available
- Add manual buttons where practical:
  - refresh POIs
  - prepare hotset
  - test narration/localization for selected POI
- Do not break existing map/main page flow.

Run:

```powershell
dotnet build src\GeoGuide.Mobile\GeoGuide.Mobile.csproj -f net10.0-windows10.0.19041.0
```

Commit:

```text
feat(mobile): add reference demo status UI
```

Final response: changed files, build result, commit hash, known gaps.

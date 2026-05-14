# Prompt: AI Advisor UI Agent

```powershell
cd C:\dev\personal\geoguide-ui-worktrees\ai-advisor-ui
```

Read this prompt and implement only your scope.

Goal: build a visible AI Advisor page. The API exists, but there is no UI for owner/admin to enhance POI descriptions.

Own:

- `src/GeoGuide.Cms/Controllers/AiAdvisorController.cs`
- `src/GeoGuide.Cms/Views/AiAdvisor/**`
- AI advisor UI view models

Avoid:

- API service changes unless required for reuse
- seller/admin review/mobile files

Requirements:

- `GET /AiAdvisor`
  - form fields: name, category, address, price range, description.
- `POST /AiAdvisor`
  - call `IAiAdvisorService` directly if available.
  - show enhanced result, provider, daily limit metadata if available.
- Include clear copyable output area.
- Add note if provider is local demo fallback.
- Keep page usable for both admin and owner users.

Run:

```powershell
dotnet build src\GeoGuide.Cms\GeoGuide.Cms.csproj
```

Commit:

```text
feat(ui): add AI advisor page
```

Final response: changed files, build result, commit hash, known gaps.

# Worktree Map

Root folder:

```text
C:\dev\personal\geoguide-agent-worktrees
```

Use one terminal per worktree.

```powershell
cd C:\dev\personal\geoguide-agent-worktrees\content-api
git status --short
```

## Worktrees

| Folder | Branch | Suggested Agent |
| --- | --- | --- |
| `C:\dev\personal\geoguide-agent-worktrees\content-api` | `codex/ref-content-api` | Codex |
| `C:\dev\personal\geoguide-agent-worktrees\localization-audio` | `codex/ref-localization-audio` | Copilot or Codex |
| `C:\dev\personal\geoguide-agent-worktrees\maps-offline` | `codex/ref-maps-offline` | Copilot |
| `C:\dev\personal\geoguide-agent-worktrees\admin-owner-rbac` | `codex/ref-admin-owner-rbac` | Antigravity or Codex |
| `C:\dev\personal\geoguide-agent-worktrees\ai-advisor` | `codex/ref-ai-advisor` | Copilot |
| `C:\dev\personal\geoguide-agent-worktrees\mobile-flow` | `codex/ref-mobile-flow` | Antigravity |
| `C:\dev\personal\geoguide-agent-worktrees\dashboard-docs` | `codex/ref-dashboard-docs` | Codex |
| `C:\dev\personal\geoguide-agent-worktrees\verification` | `codex/ref-verification` | Codex |

## Conflict Boundaries

| Area | Primary Branch | Avoid Touching From Other Branches |
| --- | --- | --- |
| `src/GeoGuide.Cms/Controllers/Api/V1/*Poi*` | `content-api` | All other branches |
| `src/GeoGuide.Cms/Controllers/Api/V1/*Localization*`, `*Audio*`, `Services/*Audio*` | `localization-audio` | All other branches |
| `src/GeoGuide.Cms/Controllers/Api/V1/*Maps*`, `wwwroot/static/maps` | `maps-offline` | All other branches |
| `src/GeoGuide.Cms/Security`, `Services/CmsAccessService.cs`, owner/admin auth controllers | `admin-owner-rbac` | All other branches |
| `src/GeoGuide.Cms/Controllers/Api/V1/*Ai*`, AI usage models/services | `ai-advisor` | All other branches |
| `src/GeoGuide.Mobile/**` | `mobile-flow` | All other branches |
| `Views/Home`, `wwwroot/css/site.css`, `docs/**`, root README | `dashboard-docs` | All other branches |
| Compile fixes across branches | `verification` | Only after feature branches are ready |

If two branches need the same shared file, prefer adding a new class and letting ASP.NET controller discovery pick it up. Only touch `Program.cs` when dependency injection is required.

# UI Worktree Map

| Folder | Branch | Goal |
| --- | --- | --- |
| `admin-console` | `codex/ui-admin-console` | Replace plain dashboard with visible reference console and module launchpad |
| `seller-portal` | `codex/ui-seller-portal` | Build seller/owner portal UI for registration, dashboard, POI submission/update |
| `admin-review` | `codex/ui-admin-review` | Build admin UI for pending seller POIs, approve/reject, owner audit |
| `workflow-console` | `codex/ui-workflow-console` | Build UI controls for localization warmup, audio TTS, maps manifest |
| `ai-advisor-ui` | `codex/ui-ai-advisor` | Build visible AI Advisor form and result surface |
| `mobile-demo-ui` | `codex/ui-mobile-demo` | Add MAUI visible demo/status UI for sync/hotset/offline/narration |
| `demo-polish-docs` | `codex/ui-demo-polish-docs` | Update demo script, screenshots checklist, README, visible navigation notes |
| `ui-verification` | `codex/ui-verification` | Merge/check UI branches, build, smoke test, screenshot report |

## Conflict Boundaries

| Area | Owner |
| --- | --- |
| `Views/Home/Index.cshtml`, dashboard-only CSS | `admin-console` |
| `Controllers/OwnerPortalController.cs`, `Views/OwnerPortal/**`, owner portal models | `seller-portal` |
| `Controllers/AdminReviewController.cs`, `Views/AdminReview/**`, approval UI models | `admin-review` |
| `Controllers/ReferenceConsoleController.cs`, `Views/ReferenceConsole/**` for localization/audio/maps only | `workflow-console` |
| `Controllers/AiAdvisorController.cs`, `Views/AiAdvisor/**` | `ai-advisor-ui` |
| `src/GeoGuide.Mobile/**` UI pages/status controls | `mobile-demo-ui` |
| `docs/**`, root README, CMS README | `demo-polish-docs` |
| integration fixes only after branches are ready | `ui-verification` |

If a shared navigation file must be touched, keep edits tiny and list them in final output.

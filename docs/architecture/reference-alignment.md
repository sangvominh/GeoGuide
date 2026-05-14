# Reference Architecture Alignment

This document maps the abstract reference architecture concepts presented in the teacher artifact to the concrete implementations within the GeoGuide repository.

## Teacher Reference vs. Implemented Code

| Reference Component | Implemented Technology | Location in Code |
| :--- | :--- | :--- |
| **Backend Framework** | .NET 8 ASP.NET Core MVC/API | `src/GeoGuide.Cms` |
| **Database** | PostgreSQL | `src/GeoGuide.Cms/Data/ApplicationDbContext.cs` |
| **Mobile App** | .NET MAUI | `src/GeoGuide.Mobile` |
| **Admin/Owner/RBAC** | ASP.NET Core Identity | `src/GeoGuide.Cms/Data/Migrations` & `src/GeoGuide.Cms/Controllers/*AdminController.cs` |
| **Content API** | ASP.NET Core Controllers | `src/GeoGuide.Cms/Controllers/Api/V1/PoisController.cs` |
| **Audio/TTS** | External TTS / Media Services | `src/GeoGuide.Cms/Models/Poi.cs` (Narration tracking fields) |
| **Localization** | .NET Globalization & JSON | `src/GeoGuide.Cms/Models/PoiTranslation.cs` |
| **Maps Offline** | MAUI Map Control | `src/GeoGuide.Mobile` (Map integration) |
| **AI Advisor** | OpenAI/Semantic Kernel/Custom | `src/GeoGuide.Cms/Services/IAiAdvisorService.cs` |
| **Mobile Offline Cache** | SQLite in MAUI | `src/GeoGuide.Mobile` (Local data storage) |

## Important Distinctions

- **No React/Next.js:** The CMS dashboard is built using ASP.NET Core MVC with Razor Views (Bootstrap 5 for styling).
- **No FastAPI/MongoDB:** The backend uses C# .NET APIs and PostgreSQL.
- **No Swift/Kotlin:** The mobile application is built using .NET MAUI for cross-platform support.

The implemented architecture prioritizes a unified .NET stack to reduce context switching between backend and mobile development, ensuring rapid iteration while maintaining all functional capabilities shown in the reference presentation.

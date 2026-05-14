# Prompt: AI Advisor Agent

You are working in:

```text
C:\dev\personal\geoguide-agent-worktrees\ai-advisor
```

Branch:

```text
codex/ref-ai-advisor
```

Goal: implement the teacher reference AI Advisor endpoint in a demo-safe way.

Own these areas:

- `src/GeoGuide.Cms/Controllers/Api/V1/*Ai*`
- AI-specific services/models
- optional EF model/migration only if necessary

Avoid editing:

- content/audio/maps/admin/mobile/dashboard files

Required behavior:

- `POST /api/v1/ai/enhance-description`
- accept POI name, description, category, address, price range
- return enhanced Vietnamese content suitable for food tourism
- enforce 10 requests/day/user or owner
- work without a Gemini API key using deterministic local enhancement
- optionally call Gemini if configured via appsettings/env var

Implementation guidance:

- Do not hardcode secrets.
- If no key exists, return a good local enhancement and metadata `provider = "local-demo"`.
- If adding persistence for usage logs is too invasive, use a scoped/simple durable approach that builds cleanly and document the limitation.

Run:

```powershell
dotnet build src/GeoGuide.Cms/GeoGuide.Cms.csproj
```

Commit with:

```text
feat(ai): add advisor description enhancement endpoint
```

Final response must list changed files, tests run, and any known gaps.

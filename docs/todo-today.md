# Integration Todo Today

Branch: `codex/integration-mvp`
Workspace: `C:\dev\personal\GeoGuide-integration`

## Rules

- Do not develop independent features here unless needed to resolve integration
- Use this branch to merge, validate, and stabilize
- Commit with prefix `integration:`
- Push only to `codex/integration-mvp`

## Do In This Order

- Wait for mobile, CMS, and analysis branches to receive their first pushes
- Merge `codex/mobile-mvp`
- Merge `codex/cms-mvp`
- Merge `codex/analysis-mvp`
- Resolve conflicts carefully without expanding scope
- Run build/test checks for the integrated result
- Document what still works and what is deferred

## Done When

- Integration branch contains all three streams
- Shared contract is still consistent
- Build and demo path are stable enough for final validation

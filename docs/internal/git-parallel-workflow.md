# Git Parallel Workflow

## Branch Model For Today

Base branch for integration:

- `codex/integration-mvp`

Parallel work branches:

- `codex/mobile-mvp`
- `codex/cms-mvp`
- `codex/analysis-mvp`

Setup branch:

- `codex/setup-mvp-workflow`

## Merge Direction

Only merge in this direction:

1. Feature branch -> `codex/integration-mvp`
2. `codex/integration-mvp` -> `main` after integration validation

Do not merge feature branches directly into `main`.

## Ownership Rules

- `codex/mobile-mvp` changes only mobile app and shared mobile-facing contract adjustments
- `codex/cms-mvp` changes only backend/CMS and shared server-facing contract adjustments
- `codex/analysis-mvp` changes only `docs/` and presentation/report assets

If a change touches more than one stream, land it first in `codex/integration-mvp`.

## Sync Rules

- Rebase or merge from `codex/integration-mvp` into each feature branch before opening a PR
- Keep PRs small and stream-specific
- Never force-push over someone else's branch
- Never rewrite `main`

## Commit Rules

- One concern per commit
- Reference the stream in the commit message prefix:
  - `mobile: ...`
  - `cms: ...`
  - `analysis: ...`
  - `integration: ...`

## Pull Request Rules

- Target branch must be `codex/integration-mvp` unless explicitly doing the final release merge
- PR must describe:
  - what changed
  - files/folders owned
  - contract changes
  - manual test evidence

## Manual Integration Checklist

Before merging into `codex/integration-mvp`:

- Branch is up to date with `codex/integration-mvp`
- Shared POI contract still matches `docs/mvp-parallel-plan.md`
- No unrelated file edits
- Build/test for the affected stream has been run
- Any new environment variables are documented

Before merging `codex/integration-mvp` into `main`:

- Mobile can load POIs from the live or seeded API
- CMS can create/edit POIs that match the shared contract
- Playback logging endpoint accepts requests
- Demo script is updated

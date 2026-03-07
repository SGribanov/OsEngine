# AGENTS.md instructions for c:\Repos\MyCloneOsEngine

## Mandatory session workflow
- At the start of every session, first synchronize repository state:
  - `git fetch --all --prune`
  - `git status --short --branch`
  - if worktree is clean: `git pull --rebase`
  - if worktree is dirty: report local changes and remote divergence before any pull/rebase.
- Determine active issue in this order:
  - explicit issue from user request (e.g. `#123`);
  - issue ID from current branch name / recent commit context;
  - latest relevant open issue in this repo.
- Before coding, read latest issue comments, linked PR state, and current GitHub Project item status.
- Publish a short session summary before implementation:
  - current state;
  - latest progress;
  - blockers;
  - last completed meaningful work.
- During execution, keep issue/project synchronized:
  - checkpoint after major milestones;
  - checkpoint before and after push/merge;
  - long-session checkpoint around `~4.5h`.
- Before ending session, always publish handoff to GitHub issue/PR comment:
  - completed work (facts + commits);
  - next steps (ordered);
  - blockers/risks;
  - verification status.

## GitHub project policy (one board per repository)
- Every repository must have its own dedicated GitHub Project board.
- Automation and scripts must not hardcode project IDs/URLs.
- Resolve board dynamically in this priority order:
  - repository variable `PROJECT_BOARD_NUMBER` (preferred explicit mapping);
  - repository variable `PROJECT_BOARD_TITLE`;
  - fallback: board title equals repository name.
- Board owner is derived from current repository owner (user or org), not hardcoded account names.
- If dedicated board is missing:
  - create it: `gh project create --owner <owner> --title "<repo-name>"`;
  - verify fields: `gh project field-list <project-number> --owner <owner>`;
  - ensure `Status` options include `Todo`, `In Progress`, `Done`.
- Every canonical issue must be linked to the repo-specific board and status-synced.

## Issue/task tracking policy
- One canonical GitHub issue per task/branch.
- New bug/feature/refactor -> create issue before substantial implementation.
- For multi-stream plans:
  - one parent issue for objective;
  - child issues only for major independent workstreams;
  - tiny steps as checklist items in parent issue.
- Do not close issue until acceptance criteria are met.
- If GitHub issue/project is unavailable, write local `session_handoff.md` and publish it on next session start.

## GitHub CLI execution policy
- For machine-readable operations, use `gh ... --json ... --jq ...`.
- Do not parse human-readable `gh` output with `grep` when `--json` is available.
- Prefer bulk operations when applicable (e.g., editing multiple issues in one command).
- Prefer explicit `gh` search filters for task slices (state/label/assignee/date).

## AI context policy for issue comments
- Keep one AI context block per active issue using markers:
  - `<!-- AI-CONTEXT:START -->`
  - `<!-- AI-CONTEXT:END -->`
- On resume, load latest AI context block from issue comments first.
- On pause/stop, update existing AI context comment (PATCH) instead of creating duplicates.
- Minimum context payload:
  - done;
  - next;
  - resume one-liner.

## Status source of truth
- Canonical progress status is GitHub Project field `Status` (`Todo`, `In Progress`, `Done`).
- Issue `open/closed` is used for completion state, not as a parallel progress ladder.
- Labels are optional metadata and must not duplicate workflow state machine.
- `AI-CONTEXT` should not introduce a separate status enum.

## Parent registry integration (`C:\Repos`)
- `C:\Repos\REPOSITORIES.md` is the parent inventory of child repositories and canonical links.
- At session start, verify current repository entry exists and matches actual remote.
- If a new top-level repo appears under `C:\Repos`, refresh parent registry in parent repo (`git registry-refresh`) and commit registry update separately.
- Product execution remains in child repo issues/projects; parent repo is for registry/governance artifacts.

## Mandatory .NET verification policy
- For any .NET code change, always run verification outside sandbox (`require_escalated`):
  - `dotnet restore`
  - `dotnet build`
  - `dotnet test`
- Prefer a single chained command for restore/build/test when possible.

## GitHub CLI auth in agent context
- Prefer `gh auth status`/keyring auth first.
- If keyring auth is unavailable, load `GH_TOKEN` from local file in a portable way (example for PowerShell):
  - `$tokenPath = Join-Path $HOME 'gh_pat.txt'`
  - `if (Test-Path $tokenPath) { $env:GH_TOKEN = (Get-Content $tokenPath -Raw).Trim() }`
- Run `gh` commands in the same command invocation/session where token is loaded.

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
- In this repository, prefer `powershell -NoProfile -ExecutionPolicy Bypass -File tools/verify-dotnet.ps1` as the canonical local verification entrypoint; it serializes restore/build/test and shuts down build servers before/after the run to reduce recurring WPF/generated-file lock noise.

## GitHub CLI auth in agent context
- Prefer `gh auth status`/keyring auth first.
- If keyring auth is unavailable, load `GH_TOKEN` from local file in a portable way (example for PowerShell):
  - `$tokenPath = Join-Path $HOME 'gh_pat.txt'`
  - `if (Test-Path $tokenPath) { $env:GH_TOKEN = (Get-Content $tokenPath -Raw).Trim() }`
- Run `gh` commands in the same command invocation/session where token is loaded.

## Reports language policy
- For any newly generated graphical reports in this repository (`reports/*.html`, chart dashboards, redirect pages, summary pages), all human-facing text must be in Russian.
- This includes titles, subtitles, badges, legends, axis labels, section headers, status labels, helper text, redirect-page text, and footer/meta copy.
- Keep repository identifiers, file names, commit hashes, issue/PR numbers, and code identifiers unchanged unless there is a separate explicit requirement to localize them.

## OsEngine robot skill policy
- For any robot task in OsEngine (create/edit/refactor robots in `Robots/*` or `Custom/Robots/*`), always load and follow local skill:
  - `$HOME/.codex-personal/skills/osengine-robot-authoring/SKILL.md`
- For script robots (`Custom/Robots/*`, `Custom/Robots/Scripts/*`, `project/OsEngine/bin/Debug/Custom/Robots/*`):
  - before `restore/build/test/perf`, temporarily include the robot file in `project/OsEngine/Robots/<TempCategory>/`;
  - after validation/testing, move it back to script location and remove the temporary project copy.
- If the skill is temporarily unavailable, report it explicitly and use best-effort fallback with the same constraints.

## .NET 10 / C# 14 baseline policy
- Use `.NET 10 (LTS)` as the default production baseline in this repository.
- Keep language/runtime compatibility aligned with TFM (`net10.0*` -> `C# 14` baseline by default).
- Do not force `LangVersion=latest/default` in ways that can bypass TFM compatibility constraints.
- Prefer `System.Threading.Lock` for new synchronization points; do not place `await` inside critical sections.
- For hot paths, prefer `Span<T>/ReadOnlySpan<T>` for synchronous boundaries and `Memory<T>/ReadOnlyMemory<T>` for async boundaries.
- Use `System.Text.Json` source generation for performance-critical or AOT-sensitive serialization paths.
- Use `TimeProvider`/`FakeTimeProvider` where time-dependent logic must be testable and deterministic.
- Keep .NET analyzers enabled and configure severities explicitly; avoid silently downgrading reliability/performance rules.
- For read-mostly lookup tables, consider `FrozenDictionary/FrozenSet`.
- Reference snapshot document for details and sources:
  - `dotnet_csharp14_best_practices_2026.md`

## General code quality and reuse policy
- Prefer `KISS` and `YAGNI`: do not introduce abstractions, layers, or patterns before they solve a real repeated problem.
- Apply `DRY` pragmatically: extract shared code only after stable repetition (`rule of three`), not after one-off similarity.
- Enforce `SRP` on classes/methods and keep contracts explicit: clear inputs/outputs, predictable side effects, fail-fast guards.
- Keep methods small and composable; avoid long \"god methods\" mixing domain logic, I/O, UI, and infrastructure concerns.
- For .NET APIs:
  - avoid `async void` (except event handlers);
  - propagate `CancellationToken` on I/O and long operations;
  - implement/dispose `IDisposable` resources deterministically;
  - treat nullable warnings as design feedback, not noise.
- `#region` policy:
  - do not use `#region` to hide complexity in production logic;
  - allowed only for generated/interoperability blocks or large test fixtures where navigation benefit is clear.
- Reuse policy:
  - prefer reuse of existing internal modules/utilities before creating new shared abstractions;
  - new shared components must have clear ownership, tests, and at least two concrete call sites.
- Reviewability-first:
  - optimize for readability and maintainability over clever syntax;
  - each non-trivial optimization/refactor should include rationale and verification evidence.

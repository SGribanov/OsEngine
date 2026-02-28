# AGENTS.md instructions for c:\Repos\MyCloneOsEngine

## Mandatory session workflow
- At the start of every session, first read latest progress and next steps from GitHub for the active issue/task.
- During the session, keep GitHub Project item status synchronized with real task state (`Todo`, `In Progress`, `Done`).
- Before ending every session, publish progress and next steps to GitHub (issue/PR comment), including blockers and verification status.

## Source of truth
- Prefer one canonical GitHub issue per task/branch.
- If issue/project is temporarily unavailable, write `session_handoff.md` locally and publish it to GitHub first in the next session.

## Mandatory .NET verification policy
- For any .NET code change, always run verification outside sandbox (`require_escalated`):
  - `dotnet restore`
  - `dotnet build`
  - `dotnet test`
- Prefer a single chained command for restore/build/test when possible.

## GitHub CLI auth in agent context
- In this environment, `gh` keyring auth may be unavailable for the agent user.
- For GitHub issue/project sync commands, load token from local file into session env:
  - `$env:GH_TOKEN = (Get-Content C:\Users\gsv777\gh_pat.txt -Raw).Trim()`
- Then run `gh` commands in the same command invocation.

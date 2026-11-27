Role: cleaner
Commit: see the commit that adds this file
Verdict: clean

## Formatted
_None._ The diff against `origin/main` holds no C# file (`.claude/agents/*.md`, `.claude/settings.json`, `sdd/work/**`); nothing to format.

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| _None._ | | |

## Unresolved
_None._

## Checks
`git diff --name-only origin/main...HEAD | grep -c '\.cs$'` -> 0. Build and focused tests not run: no compiled input changed.

## Follow-up candidates
_None._

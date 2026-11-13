Role: cleaner
Commit: see git log
Verdict: clean

## Formatted
_None._ The formatter changed no file of the 33 changed under `src/`.

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| Stale comment (auditor 13 finding 7) | RH5204 analyzer, `SetDirectiveIndentation` summary | Dropped the "directive path" sentence; the comment now states that analyzer and formatter leave identical input unchanged |

## Unresolved
_None._

## Checks
scripts/build.sh: 0 Warning(s), 0 Error(s). Comment-only change; no tests run.

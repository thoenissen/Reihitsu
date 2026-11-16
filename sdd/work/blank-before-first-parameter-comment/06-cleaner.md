Role: cleaner
Commit: (this commit; on top of b3647fb)
Verdict: clean

## Formatted
_None._ `scripts/format.sh` reported `Formatted 0 of 15 file(s).`

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| _None._ | | |

## Unresolved
_None._

## Checks
- `scripts/build.sh`: `Build succeeded. 0 Warning(s) 0 Error(s)`
- No code changed, so the implementer's test results stand; no focused tests re-run.

## Follow-up candidates
While building the CLI project, `scripts/format.sh` printed S1125 warnings at `src/Reihitsu.Cli/FormatCommandHandler.cs` lines 479, 490, 501. The run did not change that file, and the Release build is warning-free, so they are not part of this run.

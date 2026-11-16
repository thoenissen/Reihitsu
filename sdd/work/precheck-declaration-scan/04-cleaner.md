Role: cleaner
Commit: see the commit that adds this file
Verdict: clean

## Formatted
_None._ (`scripts/format.sh` on `PrecheckPhase.cs`: `Formatted 0 of 1 file(s).`)

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|

None needed.

## Unresolved
_None._

## Checks
- `scripts/build.sh`: `Build succeeded. 0 Warning(s) 0 Error(s)`
- `scripts/test.sh --project formatter --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Unit.Precheck"`: `Passed! - Failed: 0, Passed: 249`
- Text-only proof: not applicable (no comment fix).

## Follow-up candidates
The format run printed `src/Reihitsu.Cli/Diagnostics/PipelineTraceCommand.cs(483,29): warning S1125` (file not changed by the run; the Release build reports 0 warnings, so likely a Debug-only or format-script build message).

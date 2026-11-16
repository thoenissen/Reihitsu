Role: cleaner
Commit: 227795c
Verdict: clean

## Formatted
_None._ `scripts/format.sh` on both changed C# files: `Formatted 0 of 2 file(s).`

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| _None._ | | |

## Unresolved
_None._

## Checks
- `scripts/build.sh`: `0 Warning(s)`, `0 Error(s)`.
- `scripts/test.sh --configuration Debug --project cli --filter "FullyQualifiedName~Reihitsu.Cli.Test.Unit.PipelineTraceCommandTests"`: `Passed! - Failed: 0, Passed: 23, Skipped: 0, Total: 23`.
- Text-only proof: not applicable (no comment fixes).

## Follow-up candidates
_None._ The S1125 warning the format script's build printed for `src/Reihitsu.Cli/Diff/EditScriptBuilder.cs(91,125)` is not in a changed file and does not appear in the Release build.

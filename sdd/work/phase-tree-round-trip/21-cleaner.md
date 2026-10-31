Role: cleaner
Commit: 24b8d3e
Verdict: clean

## Formatted
_None._ (`scripts/format.sh` on the two changed C# files: "Formatted 0 of 2 file(s).")

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| _None._ | | |

## Unresolved
_None._

## Checks
- `scripts/format.sh src/Reihitsu.Formatter.Test/Unit/Pipeline/RawStringMovedLineStartTests.cs src/Reihitsu.Formatter/Pipeline/Indentation/Utilities/RawStringLineShifts.cs`: Formatted 0 of 2 file(s). (S1125 warnings printed from unchanged `Reihitsu.Cli/FormatCommandHandler.cs` during the script's own tool build are outside this run's files.)
- `scripts/build.sh`: Build succeeded. 0 Warning(s) 0 Error(s)
- `scripts/test.sh --project formatter --filter "FullyQualifiedName~RawStringMovedLineStartTests"`: Passed! Failed: 0, Passed: 29, Total: 29

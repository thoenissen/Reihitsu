Role: cleaner
Commit: 02db276
Verdict: clean

## Formatted
_None._ `scripts/format.sh` over the 22 changed C# files reported `Formatted 0 of 22 file(s).` The script's own CLI build printed S1125 warnings in `src/Reihitsu.Cli/FormatCommandHandler.cs`, a file this run did not change; they do not occur in the Release solution build below.

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| _None._ | | |

## Unresolved
_None._

## Checks
- `scripts/build.sh`: `Build succeeded.`, `0 Warning(s)`, `0 Error(s)`.
- `scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Documentation.RH80"`: `Passed!  - Failed: 0, Passed: 287, Skipped: 0, Total: 287`.
- Text-only proof: not applicable (no comment fix after an audit).

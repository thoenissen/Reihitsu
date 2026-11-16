Role: cleaner
Commit: 792d9df
Verdict: clean

## Formatted
_None._ (`scripts/format.sh` on the 15 changed C# files: "Formatted 0 of 15 file(s)".)

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| _None._ | | |

## Unresolved
_None._

## Checks
- `scripts/build.sh`: Build succeeded, 0 Warning(s), 0 Error(s).
- `scripts/test.sh --project formatter --filter FullyQualifiedName~FirstCommentBehindOpeningTokenTests`: Passed 21, Failed 0.

## Follow-up candidates
The formatter run printed S1125 warnings in `src/Reihitsu.Cli` (`EditScriptBuilder.cs`, `FormatCommandHandler.cs`), files this run did not change; the Release build is clean, so they come from the format tool's own build setup.

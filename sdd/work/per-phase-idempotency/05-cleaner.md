Role: cleaner
Commit: 32f6186
Verdict: clean

## Formatted
_None._ (`scripts/format.sh` reported "Formatted 0 of 5 file(s)"; the five existing changed C# files were already formatted.)

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| _None._ | | |

## Unresolved
_None._ The format script's build output lists S1125 warnings in `src/Reihitsu.Cli/FormatCommandHandler.cs`, a file this run did not change; the Release build of the solution reports none.

## Checks
- `scripts/build.sh`: Build succeeded. 0 Warning(s), 0 Error(s).
- `scripts/test.sh --project formatter --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Unit.Pipeline"`: Passed! Failed: 0, Passed: 135.

Role: cleaner
Commit: da8fd52
Verdict: clean

## Formatted
_None._ The formatter changed 0 of 60 changed C# files.

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| _None._ | | |

## Unresolved
_None._ The S1125 warnings the format script's own CLI build printed sit in `PipelineTraceCommand.cs` and `FormatCommandHandler.cs`, which this run did not change; the Release build of the solution reports none.

## Checks
- `scripts/format.sh --force <60 changed files>`: Formatted 0 of 60 file(s).
- `scripts/build.sh`: 0 Warning(s), 0 Error(s).
- `scripts/test.sh --project core`: see run (Passed, no failures).

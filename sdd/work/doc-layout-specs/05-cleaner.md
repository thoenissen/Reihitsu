Role: cleaner
Commit: see the commit carrying this file
Verdict: clean

## Formatted
_None._ `scripts/format.sh` reported "Formatted 0 of 12 file(s)" for the 12 changed C# test files.

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| _None._ | | |

## Unresolved
_None._ The formatter tool's own build printed S1125 warnings for `src/Reihitsu.Cli/FormatCommandHandler.cs`. That file is not part of this run's diff, and the Release build of the solution reports none.

## Checks
- `scripts/build.sh`: Build succeeded. 0 Warning(s) 0 Error(s)
- `scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH830|FullyQualifiedName~RH8401|FullyQualifiedName~RH8402|FullyQualifiedName~RH8501"`: Passed! Failed: 0, Passed: 255, Skipped: 0, Total: 255

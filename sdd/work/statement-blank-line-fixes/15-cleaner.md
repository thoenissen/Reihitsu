Role: cleaner
Commit: fc15581 (parent; this commit adds only this file)
Verdict: clean

## Formatted
_None._ `scripts/format.sh --force` over the 48 changed C# files of the run: Formatted 0 of 48.

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| _None._ | | |

## Unresolved
_None._ The format script's own build printed S1125 warnings for `src/Reihitsu.Cli/Diagnostics/PipelineTraceCommand.cs`, a file the run did not change; the Release build reports none.

## Checks
- `scripts/build.sh`: Build succeeded, 0 Warning(s), 0 Error(s).
- `scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH50|FullyQualifiedName~RH3208"`: Passed: 1018, Failed: 0.

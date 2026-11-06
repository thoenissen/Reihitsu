Role: cleaner
Commit: 3fb8b49
Verdict: clean

## Formatted
_None._ `scripts/format.sh --force` over the 27 changed C# files: "Formatted 0 of 27 file(s)".

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| _None._ | | |

## Unresolved
_None._ The Debug build inside `format.sh` printed S1125 warnings in `src/Reihitsu.Cli/Diagnostics/PipelineTraceCommand.cs`, a file the run did not change; the Release build has none.

## Checks
- `scripts/build.sh`: Build succeeded, 0 Warning(s), 0 Error(s).
- `scripts/test.sh --project analyzer --filter <RH7003|RH7004Using|RH7101DoNot|RH7105|RH7106|RH7109|RH7303|RH7304|RH7306|RH7309|RH7501>`: Passed! Failed: 0, Passed: 380, Total: 380.

## Whole run
- #1136: clean
- #1134: clean
- #1135: clean
- #1262: clean
- #1332: clean
- #1336: clean
- #1340: clean

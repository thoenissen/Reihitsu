Role: cleaner
Commit: 542f8ef (no code changes made on top of it; see the commit that adds this file)
Verdict: clean

## Formatted
_None._ `scripts/format.sh` over the 5 C# files changed since the last cleaner run: "Formatted 0 of 5 file(s)." (The run printed S1125 warnings from `src/Reihitsu.Cli/FormatCommandHandler.cs`, a file this run did not change.)

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| _None._ | | |

## Unresolved
_None._

## Checks
- `scripts/build.sh`: Build succeeded. 0 Warning(s) 0 Error(s)
- selfhosting, `AnalyzerViolationReportTests`: Passed! Failed: 0, Passed: 6, Total: 6
- tooling, `LaneClassifierTests|LaneCheckCommandTests`: Passed! Failed: 0, Passed: 75, Total: 75

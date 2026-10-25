Role: cleaner
Commit: 69038da (no code changes made on top of it; see the commit that adds this file)
Verdict: clean

## Formatted
_None._ `scripts/format.sh` over the 13 changed, non-deleted C# files: "Formatted 0 of 13 file(s)."

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| _None._ | | |

## Unresolved
_None._ (The format run printed S1125 warnings from `src/Reihitsu.Cli/FormatCommandHandler.cs`, a file this run did not change; the Release build is clean.)

## Checks
- `scripts/build.sh`: Build succeeded. 0 Warning(s) 0 Error(s)
- architecture, `TestScriptProjectSelectionTests`: Passed! Failed: 0, Passed: 7, Total: 7
- selfhosting, `AnalyzerViolationReportTests|SourceFileRowsCover`: Passed! Failed: 0, Passed: 6, Total: 6

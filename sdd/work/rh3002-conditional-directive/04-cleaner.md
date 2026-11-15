Role: cleaner
Commit: 8b1436c
Verdict: clean

## Formatted
_None._ The formatter reported "Formatted 0 of 1 file(s)" for the one changed C# file, `src/Reihitsu.Analyzer.Test/Clarity/RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzerTests.cs`.

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| _None._ | | |

## Unresolved
_None._ (The S1125 warnings printed during `scripts/format.sh` come from building the CLI tool in `FormatCommandHandler.cs`, which this run did not change; the Release build of the solution reports none.)

## Checks
- `scripts/build.sh`: Build succeeded. 0 Warning(s), 0 Error(s)
- `scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzerTests"`: Passed! - Failed: 0, Passed: 254, Skipped: 0, Total: 254

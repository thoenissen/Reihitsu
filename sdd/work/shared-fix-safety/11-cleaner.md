Role: cleaner
Commit: 2e654c5
Verdict: clean

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Formatted
_None._ `scripts/format.sh` over the one changed C# file of the repair (`RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzerTests.cs`): "Formatted 0 of 1 file(s)." The script's own CLI build printed S1125 warnings in unchanged `Reihitsu.Cli` files; they are outside this run.

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| _None._ | | |

## Unresolved
_None._

## Checks
- `scripts/build.sh`: Build succeeded. 0 Warning(s), 0 Error(s).
- `scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Ordering.RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzerTests|TestCategory=SHARED-FIX-003"`: Passed! Failed: 0, Passed: 48, Skipped: 0, Total: 48.
- Text-only proof: not applicable (no comment fixes after an audit).

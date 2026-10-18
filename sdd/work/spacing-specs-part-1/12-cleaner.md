Role: cleaner
Commit: 8f2682e
Verdict: clean

## Formatted
_None._ `scripts/format.sh` over the one changed C# file (`src/Reihitsu.Analyzer.Test/Formatting/RH5604CodeMustNotContainMixedLineEndingsAnalyzerTests.cs`): `Formatted 0 of 1 file(s).`

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| _None._ | | |

## Unresolved
_None._

## Checks
- `scripts/build.sh`: 0 Warning(s) 0 Error(s)
- `scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5604Code"`: Passed! - Failed: 0, Passed: 28, Skipped: 0, Total: 28
- Text-only proof: not applicable, no comments fixed.

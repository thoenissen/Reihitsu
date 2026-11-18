Role: cleaner
Commit: see git log (this commit)
Verdict: clean

## Formatted
_None._ `scripts/format.sh` over the 14 changed C# files formatted 0 of 14.

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| RH7103 (x2) | `LineCountAnalyzerBase.cs` `GetSpanBetweenBraces`, `TryGetBodySpan` | Static members moved before the instance members |
| RH7110 (x3) | `LineCountAnalyzerBase.cs` `CountLinesOfCode`, `IsTestCode`, `IsTrue`, abstract `TryGetCountedSpan` | Members reordered: internal static, private protected static, private protected abstract, private static, private instance |
| RH8201 (x2) | `RH2008...Analyzer.cs`, `RH2111...Analyzer.cs` `TryGetCountedSpan` | Own summary replaced by `<inheritdoc/>` (the base member's documentation applies; the per-rule detail is in the specs) |

## Unresolved
_None._

## Checks
- `scripts/build.sh`: 0 Warning(s), 0 Error(s).
- `scripts/test.sh --project analyzer --filter RH2008|RH2111|RH0001|RH8402`: Passed! Failed: 0, Passed: 324.
- `scripts/test.sh --project architecture`: Passed! Failed: 0, Passed: 198.

## Follow-up candidates
_None._

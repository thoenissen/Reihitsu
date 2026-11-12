Role: test implementer
Commit: (this commit; parent a35a7b6)
Verdict: tests written

Scope: decisions #17 and #19 of `00-lead.md`. No production code touched. My prompt carried no intended fix or hoped-for verdict beyond the two decisions.

## Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH5011…FormatterTests.VerifyFormatterKeepsBreakDirectlyAboveDoStatementFooterAndAnalyzerReportsNothing` (renamed from `…KeepsBreakDirectlyAboveElse…`) | RH5011-008 | `VerifyFormatter(source)` | passes |
| `RH5024…FormatterTests.VerifyFormatterKeepsBlankLineBelowRegionDirectiveAndAnalyzerReportsNothing` | (unchanged tags) | `VerifyFormatter(source)` | passes |
| `RH5025…FormatterTests.VerifyFormatterKeepsBlankLineBelowRegionDirectiveAndAnalyzerReportsNothing` | (unchanged tags) | `VerifyFormatter(source)` | passes |
| `RH5026…FormatterTests.VerifyFormatterKeepsBlankLineBelowRegionDirectiveAndAnalyzerReportsNothing` | (unchanged tags) | `VerifyFormatter(source)` | passes |
| `RH5027…FormatterTests.VerifyFormatterKeepsBlankLineBelowRegionDirectiveAndAnalyzerReportsNothing` | (unchanged tags) | `VerifyFormatter(source)` | passes |
| Deleted: the five `ContainsBlankLine*` tests in `src/Reihitsu.Core.Test/SyntaxTriviaUtilitiesTests.cs` | decision #19 | – | – |

Only the inputs changed; no assertion changed. Each new input is the formatter's own output on `main`-equivalent formatting (checked with `scripts/trace.sh`: stable after pass 1 except the final newline the raw string never carries), and keeps the line under test:

- **RH5024, RH5026, RH5027**: added the blank lines SHARED-BLANK-005 puts above `#region`/`#endregion` where the input lacked them. The blank line below `#region R` in front of `}` / `else` / `while` is unchanged.
- **RH5025**: the field initializer moved into a local `var d = new …Dictionary<string, int>` laid out as the formatter lays it (braces at the `new` column, region directives at the statement's indentation, blank lines above both directives, no trailing comma). The blank lines below `#region A` (above `{ "b", 2 }`) and below `#endregion // A` (above `{ "c", 3 }`) are still there. The original `= new()` field put the initializer at column 60, which would have made the test unreadable.
- **RH5011**: the `else` half was dropped and the test renamed. No formatter-stable input exists in which `break;` is directly followed by `else`: the formatter braces an `if`/`else` whose bodies are unbraced, so `break;` then precedes `}`. I tried a trailing comment (still braced), an empty `#if`/`#endif` between `break;` and `else` (still braced), and `break;` inside `#if DEBUG … #else … #endif` (stable, but the analyzer on 212a268 did not report that `break;` either, so it tests nothing about RH5011-008). The `else` case keeps its analyzer coverage: `VerifyBreakBeforeElseOnSameLineIsNotReported`, `VerifyBreakBeforeElseOnNextLineIsNotReported`, `VerifyBreakBeforeElseIsNotReportedWithCarriageReturnLineFeed` in `RH5011…AnalyzerTests.cs`. The do-footer half is unchanged.

Fails for the right reason: in a worktree at 212a268 (before the implementer's commit) with these five test files, all five fail only on the analyzer half (RH5011 at the do-footer `break;`; RH5024 at `}`; RH5025 at `{ "b", 2 }` and `{ "c", 3 }`; RH5026 at `else`; RH5027 at `while`). On the branch head they pass.

## Scenarios (bug fix)
_None new_; the reproductions are in 04-test-implementer.md.

## Not covered, and why
- Formatter-parity for `break;` directly above `else`: no formatter-stable input (above). Covered on the analyzer surface only.
- `ContainsBlankLine` itself is left for the implementer (decision #19); the Core test file still builds against it, and builds without it (the deleted tests were its only test callers; `rg -n "ContainsBlankLine\(" src -g '*.cs'` finds only the definition).

## Commands
```shell
scripts/test.sh --project analyzer --filter "Name=VerifyFormatterKeepsBreakDirectlyAboveDoStatementFooterAndAnalyzerReportsNothing|Name=VerifyFormatterKeepsBlankLineBelowRegionDirectiveAndAnalyzerReportsNothing"
scripts/test.sh --project core --filter "FullyQualifiedName~SyntaxTriviaUtilitiesTests"
```

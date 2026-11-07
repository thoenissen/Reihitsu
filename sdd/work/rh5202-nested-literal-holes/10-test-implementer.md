Role: test implementer
Commit: 607e2de
Verdict: tests written

Amendment to repair round 1, for lead decision 9 (00-lead.md) and the untested cells named in 09-implementer.md. The message carried no intended fix, suspected cause, or hoped-for verdict. Every test is green at 607e2de. The rightward nested tests were watched failing under a temporary mutation of the fix's `IsEmptyTextLine`: it was made to return `false` for a token whose parent's parent is an `InterpolationSyntax`, which brings back `main`'s spaces on the nested empty line. With that mutation both rightward tests failed on exactly that line and the other 65 RH5202 tests passed. The production file was restored byte for byte, and `git status` shows only my files.

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| VerifyFixLeavesEmptyContentLineOfRawStringNestedInHoleEmptyWhenShiftingRight | RH5202-004, SHARED-FIX-007 (decision 9: the formatter keeps the line empty, FMT-RAW-002); FC-027 | Analyzer `Verify` (2 diagnostics) + `ApplyCodeFixAsync`, one application, exact text + `Verify` (nested literal still reported) | green (red under the mutation above, and on `main`'s behavior) |
| …WhenShiftingRightWithCrLf | RH5202-004, SHARED-FIX-007; FC-033 | same + NormalizeToCarriageReturnLineFeed | green (red under the mutation) |
| …WhenShiftingLeft | RH5202-004, SHARED-FIX-007; FC-003 (other direction: nothing to remove, so the line stays empty whatever the guard decides) | same | green |
| …WhenShiftingLeftWithCrLf | RH5202-004, SHARED-FIX-007; FC-033 | same | green |
| RawStringLiteralUtilitiesTests.GetLeadingWhitespaceLengthCountsEveryLeadingWhitespaceCharacter (`src/Reihitsu.Core.Test/RawStringLiteralUtilitiesTests.cs`, 11 data rows) | RH5202-001 (the width of the whitespace in front of the closing quotes) | Core unit, `DataRow` | green |

Data rows of the Core test, each giving input and length:

| Input | Length |
|---|---|
| empty | 0 |
| `x` | 0 |
| four spaces + `"""` | 4 |
| two tabs + `"""` | 2 |
| space, tab, space + `"""` | 3 |
| U+00A0 + three spaces + `"""` | 4 |
| four spaces + U+00A0 + `"""` | 5 |
| U+000B, U+000C, U+2003 + `x` | 3 |
| `  x  ` (trailing whitespace not counted) | 2 |
| four spaces (all whitespace) | 4 |
| U+00A0, tab, space (all whitespace, mixed) | 3 |

The nested tests reuse the pattern of `VerifyFixShiftsLinesOfRawStringNestedInHoleAndNestedLiteralStaysReported`. In each one, a 3-line nested raw string with an empty line between `x` and `y` shifts with the outer literal (13 to the right, or 8 to the left), and its empty line stays empty.

Runs at 607e2de plus these tests:
- RH5202 analyzer class: 67 of 67 pass (63 earlier tests and these 4).
- Core `RawStringLiteralUtilitiesTests`: 11 of 11 pass.
- Architecture project: 115 of 115 pass.

## Not covered, and why

- **Parity with the formatter (`VerifyCodeFixMatchesFormatter`) for the nested case:** not applicable. The formatter also aligns the nested literal in the same pass (FMT-RAW-008), while the outer fix leaves it reported (RH5202-004), so the two outputs differ by design. The exact-text assertion covers the empty line.
- **An empty line inside an empty-content nested raw string, or deeper nesting:** `IsEmptyTextLine` decides from the token that spans the line, and that token is the same for any raw string token at any depth. Static proof: `rg -n "IsEmptyTextLine" src/Reihitsu.Analyzer.CodeFixes` finds one definition and one call site, and no condition on depth.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Formatting.RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzerTests"
scripts/test.sh --project core --filter "FullyQualifiedName~Reihitsu.Core.Test.RawStringLiteralUtilitiesTests"
scripts/test.sh --project architecture --filter "FullyQualifiedName~RequirementCoverageTests"
```

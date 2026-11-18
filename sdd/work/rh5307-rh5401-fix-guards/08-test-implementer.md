Role: test implementer
Commit: d2ca469
Verdict: tests written

My prompt carried the change to make (the RH5401 tests follow the amended RH5401-003/-004) and no intended fix, suspected cause, or hoped-for verdict.

All changes are in `src/Reihitsu.Analyzer.Test/Formatting/RH5401ExpressionStyleGetOnlyPropertiesShouldBeSingleLinedAnalyzerTests.cs`. Every test runs as `\n` and `\r\n` (DataRow). Expected values come from the RH5401-003 and RH5401-004 examples (`A`, `B`, `L`, `E`, `F` offered; `D`, `G`, `H`, `K`, `M` withheld) as amended in 398d780.

Run on d2ca469 (the implementer's write-back code): 14 fail, 64 pass. Every failure is the new condition: the fix is still offered (`VerifyNoCodeFixIsOffered` finds one action), or, in the Fix All test, `D` is fixed (`int D => 1;` with no blank line inserted) where the spec keeps it as written and reported. No failure is a compile error or unrelated layout.

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `VerifyFixPutsAPropertyWithAttributeListsOnOneLine` (changed: gains example `L`) | RH5401-003, RH5401-004 | `Verify` | pass |
| `VerifyFixSplitsAnAttributeListWithSeveralAttributesAboveTheProperty` (new) — `L` alone | RH5401-003, RH5401-004; FC-003 | `VerifyCodeFixWithEndOfLine` | pass |
| `VerifyFixKeepsABlankLineBehindAnAttributeListThatTheFormatterKeeps` (replaces `VerifyFixKeepsTheBlankLinesBehindAnAttributeList`, which held `E`, `G`, `H`, `K` fixed) — RH5401-003 blank-line example `E`, `F` | RH5401-003, RH5401-004; FC-008 | `Verify` | pass |
| `VerifyFixKeepsOneBlankLineInFrontOfACommentBehindAnAttributeList` (new) — `E` alone | RH5401-003, RH5401-004; FC-003 | `VerifyCodeFixWithEndOfLine` | pass |
| `VerifyFixKeepsOneBlankLineBetweenTwoAttributeLists` (new) — `F` alone | RH5401-003, RH5401-004; FC-003 | `VerifyCodeFixWithEndOfLine` | pass |
| `VerifyFixKeepsOneBlankLineBetweenAnAttributeListAndTheProperty` (split from `VerifyFixKeepsTheBlankLinesBetweenAnAttributeListAndTheProperty`, one-blank-line half) | RH5401-003, RH5401-004; FC-003 | `VerifyCodeFixWithEndOfLine` | pass |
| `VerifyFixIsWithheldWhenFormattingWouldChangeTheBlankLinesBehindAnAttributeList` (new) — RH5401-004 example `D`, `G`, `H`, `K`, `M` | RH5401-004 | `VerifyNoCodeFixIsOffered` | fail (fix offered) |
| `VerifyFixIsWithheldForACommentDirectlyBehindAnAttributeList` (replaces `VerifyFixInsertsNoBlankLineInFrontOfACommentBehindAnAttributeList`) — `D` | RH5401-004 | `VerifyNoCodeFixIsOffered` | fail (fix offered) |
| `VerifyFixIsWithheldForACommentDirectlyBetweenTwoAttributeLists` (new; `G` was inside the replaced combined test) | RH5401-004 | `VerifyNoCodeFixIsOffered` | fail (fix offered) |
| `VerifyFixIsWithheldForTwoBlankLinesInFrontOfACommentBehindAnAttributeList` (replaces `VerifyFixKeepsTwoBlankLinesInFrontOfACommentBehindAnAttributeList`) — `H` | RH5401-004 | `VerifyNoCodeFixIsOffered` | fail (fix offered) |
| `VerifyFixIsWithheldForTwoBlankLinesBetweenTwoAttributeLists` (replaces `VerifyFixKeepsTwoBlankLinesBetweenTwoAttributeLists`) — `K` | RH5401-004 | `VerifyNoCodeFixIsOffered` | fail (fix offered) |
| `VerifyFixIsWithheldForTwoBlankLinesBetweenAnAttributeListAndTheProperty` (split from `VerifyFixKeepsTheBlankLinesBetweenAnAttributeListAndTheProperty`, two-blank-line half) — `M` | RH5401-004 | `VerifyNoCodeFixIsOffered` | fail (fix offered) |
| `VerifyFixAllLeavesAPropertyWhoseBlankLinesTheFormatterWouldChange` (new) — `E`, `D`, and a plain attributed `A` in one document; iterative and Fix All fix `E` and `A` and leave `D` as written and reported | RH5401-003, RH5401-004; FC-008 | `Verify` (remaining diagnostic in fixed markup) | fail (`D` fixed) |

Unchanged and still offered, re-checked against the new condition: `VerifyFixKeepsAConditionalDirectiveBetweenTwoAttributeLists`, `VerifyFixKeepsAConditionalCommentBetweenAnAttributeListAndTheProperty`, `VerifyFixKeepsALineCommentBehindAnAttributeList`, `VerifyFixKeepsABlockCommentBehindAnAttributeListOnThePropertysLine`, `VerifyFixKeepsTheOneLineDocumentationCommentAboveTheAttributeLists`, `VerifyFixGivesAnAttributedPropertyBehindAMemberALineOfItsOwn`, `VerifyFixIsOfferedForACommentBehindTheSemicolon`, `VerifyFixIsOfferedForAnAttributeListOnSeveralLines`, `VerifyFixSplitsAnAttributeListWithSeveralAttributesOnThePropertysLine`. None of their inputs has a gap where the formatter changes the blank-line count. I checked that by formatting the inputs as a whole document with `scripts/format.sh` on a scratch copy: one blank line without a comment, the two `#if` inputs, the trailing line comment, and the block comment are kept; only `D` gains a blank line and `M` loses one. This shows the formatter's behavior, not the fix's. Expected values still come from the spec.

Other test files: `RH5401…FormatterTests` and `RH5408…AnalyzerTests` (`grep -rl RH5401 src/*Test*/`) assert formatter output or another rule, which this amendment does not change. They are left as they are.

## Not covered, and why

- Unchanged from 05: FC-018, FC-036, FC-010 to FC-012, FC-038 to FC-043, FC-029, FC-040.
- The condition counts blank lines per gap (07, *Internal choices*). There is no test where the formatter moves a blank line inside a gap without changing the count, because no such input has been observed (07).
- The new Fix All test puts a withheld diagnostic in the fixed markup. Its failure today is the expected difference (`D` is fixed). If it fails after the implementation only on the testing framework's iteration count, that is my defect, not a dispute with the spec. Tell me through the lead.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5401ExpressionStyleGetOnlyPropertiesShouldBeSingleLinedAnalyzerTests"
scripts/test.sh --project architecture
```

Architecture: 198 passed.

## Follow-up candidates

_None._

Role: test implementer
Commit: 870dcd7
Verdict: tests written

My prompt named the worklist: tests for auditor finding 1 as the amended RH5401-003/-004 state it. It carried no intended fix, suspected cause, or hoped-for verdict. Expected values come from the RH5401-003 and RH5401-004 examples as amended in babcc50 (13-spec-author.md), not from the auditor's suggestion or the code.

All changes are in `src/Reihitsu.Analyzer.Test/Formatting/RH5401ExpressionStyleGetOnlyPropertiesShouldBeSingleLinedAnalyzerTests.cs`. Every test runs as `\n` and `\r\n` (DataRow).

Run on 870dcd7 (the code at da8beca): 10 fail, 82 pass (92 in all; 78 before). The 10 failures are five tests on both line endings, each failing with `Assert.IsEmpty(actions)`, `expected count: 0, actual count: 1`. The fix is still offered for `N`, `O`, `P`, `R`, where the spec now withholds it. No failure is a compile error or unrelated layout. `S` passes today: the per-gap condition already withholds it, as 13 found. `Q` passes today, so the new condition must not take it away.

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `VerifyFixKeepsABlankLineBehindAnAttributeListThatTheFormatterKeeps` (changed: gains example `Q`, summary reworded) — RH5401-003 blank-line example `E`, `F`, `Q` | RH5401-003, RH5401-004; FC-008 | `Verify` | pass |
| `VerifyFixKeepsABlankLineInsideAnAttributeListTheFormatterDoesNotSplit` (new) — `Q` alone | RH5401-003, RH5401-004; FC-003 | `VerifyCodeFixWithEndOfLine` | pass |
| `VerifyFixIsWithheldWhenFormattingWouldDropABlankLineInsideAnAttributeListItSplits` (new) — RH5401-004 example `N`, `O`, `P`, `R`, `S` | RH5401-004 | `VerifyNoCodeFixIsOffered` | fail (fix offered) |
| `VerifyFixIsWithheldForABlankLineBehindTheCommaOfAnAttributeListTheFormatterSplits` (new) — `N` (auditor `p03`) | RH5401-004 | `VerifyNoCodeFixIsOffered` | fail (fix offered) |
| `VerifyFixIsWithheldForABlankLineInFrontOfTheCommaOfAnAttributeListTheFormatterSplits` (new) — `O` (auditor `s02`) | RH5401-004 | `VerifyNoCodeFixIsOffered` | fail (fix offered) |
| `VerifyFixIsWithheldForABlankLineBehindTheOpeningBracketOfAnAttributeListTheFormatterSplits` (new) — `P` | RH5401-004 | `VerifyNoCodeFixIsOffered` | fail (fix offered) |
| `VerifyFixIsWithheldForABlankLineInsideTheArgumentsOfAnAttributeInAListTheFormatterSplits` (new) — `R` | RH5401-004 | `VerifyNoCodeFixIsOffered` | fail (fix offered) |
| `VerifyFixIsWithheldWhenTheFormatterDropsABlankLineInsideASplitListAndInsertsOneInFrontOfAComment` (new) — `S`, the per-gap condition when the total is unchanged | RH5401-004; FC-003 | `VerifyNoCodeFixIsOffered` | pass |

Both sides of the new condition (FC-003): the total over the attribute region changes (`N`, `O`, `P`, `R`, withheld) versus it stays the same (`L` in the existing `VerifyFixSplitsAnAttributeListWithSeveralAttributesAboveTheProperty` and `VerifyFixPutsAPropertyWithAttributeListsOnOneLine`, `Q` new, offered). Both conditions together: `S` (total unchanged, one gap changed, withheld). The per-gap side is still covered by the 08 tests (`D`, `G`, `H`, `K`, `M` withheld; `E`, `F` offered).

Existing tests that assert a changed output: none. The amendment only withholds the fix for inputs with a blank line inside a split list. No existing RH5401 test has such an input (`L` tests have none), and `RH5401…FormatterTests` and `RH5408…AnalyzerTests` (`grep -rl RH5401 src/*Test*/ --include=*.cs`) assert formatter output or another rule. They are left as they are.

## Not covered, and why

- A blank line in front of the `]` of a list that is split: it keeps the count, so the spec offers the fix, but the formatter's output for it is a follow-up candidate (13) and no spec example states it. A test would pin a disputed output.
- Fix All over a mix of withheld split-list members and offered properties: the Fix All path runs the same `RegisterCodeFixesCoreAsync` gate per diagnostic as the single fix, and `VerifyFixAllLeavesAPropertyWhoseBlankLinesTheFormatterWouldChange` (08) already covers a withheld member next to offered ones in one document. A second Fix All test adds no new path.
- Unchanged from 05 and 08: FC-018, FC-036, FC-010 to FC-012, FC-038 to FC-043, FC-029, FC-040.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5401ExpressionStyleGetOnlyPropertiesShouldBeSingleLinedAnalyzerTests"
scripts/test.sh --project architecture
```

Architecture: 198 passed. `scripts/format.sh` changed nothing in the test file.

## Follow-up candidates

_None._

Role: test implementer
Commit: 2bc2fbe
Verdict: tests written

Repair round 1, for audit findings #1 and #2 (07-auditor.md) as decisions 6 and 7 in 00-lead.md settle them. The prompt carried no intended fix, suspected cause, or hoped-for verdict. Expected values follow RH5202-003 (shift by the difference between the opening column and the full width of the whitespace in front of the closing quotes) and decision 7 (an empty hole line outside nested text shifts as on `main`); none was taken from code output, except the two formatter characterization tests, which pin today's formatter output because decision 7 says it must not change.

New analyzer tests are in `src/Reihitsu.Analyzer.Test/Formatting/RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzerTests.cs`; the non-space whitespace is written as `¤` in the source and replaced with an escaped character (`' '`, `'\f'`, `'\v'`, `' '`) at the call, so no invisible character is in the file. `Verify` runs the single fix iteratively (one iteration per diagnostic) and Fix All, so a fix that needs a second application or makes no progress fails on the fixed text.

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| VerifyFixShiftsRightWhenClosingWhitespaceStartsWithNoBreakSpace | RH5202-003; FC-007, FC-009 (finding #1: NBSP first, rightward, plain) | Verify | red: lines shifted 16 instead of 12 |
| VerifyFixShiftsRightWhenClosingWhitespaceStartsWithNoBreakSpaceWithCrLf | RH5202-003; FC-033 | Verify + NormalizeToCarriageReturnLineFeed | red, same |
| VerifyFixShiftsRightWhenClosingWhitespaceStartsWithFormFeed | RH5202-003 (U+000C first, rightward, plain) | Verify | red: 16 instead of 12 |
| VerifyFixShiftsRightWhenClosingWhitespaceStartsWithVerticalTab | RH5202-003 (U+000B first, rightward, plain) | Verify | red: 16 instead of 12 |
| VerifyFixShiftsUtf8LiteralRightWhenClosingWhitespaceStartsWithEmSpace | RH5202-003 (U+2003 first, rightward, `u8`) | Verify | red: 16 instead of 12 |
| VerifyFixShiftsInterpolatedLiteralRightWhenClosingWhitespaceStartsWithNoBreakSpace | RH5202-003 (NBSP first, rightward, `$"""`) | Verify | red: 17 instead of 13 |
| VerifyFixShiftsInterpolatedLiteralRightWhenClosingWhitespaceStartsWithNoBreakSpaceWithCrLf | RH5202-003; FC-033 | Verify + CRLF | red, same |
| VerifyFixShiftsLeftWhenClosingWhitespaceEndsWithNoBreakSpace | RH5202-003 (NBSP behind 20 spaces, leftward, plain) | Verify | red: 4 removed instead of 5 |
| VerifyFixShiftsLeftWhenClosingWhitespaceEndsWithNoBreakSpaceWithCrLf | RH5202-003; FC-033 | Verify + CRLF | red, same |
| VerifyFixShiftsLeftWhenClosingWhitespaceStartsWithNoBreakSpace | RH5202-003 (NBSP first, leftward; the audit's pre-existing cell, red on `main` too) | Verify | red: at the head the fix inserts 16 spaces in front of the NBSP instead of removing NBSP + 4 spaces |
| VerifyFixShiftsEmptyLineInsideHole | RH5202-003, RH5202-005 (finding #2: empty hole line, rightward; RH5202-005's "not a line of one of its interpolation holes") | Verify + ExpandWhitespaceMarkers | red: empty line left empty, expected 13 spaces as on `main` |
| VerifyFixShiftsEmptyLineInsideHoleWithCrLf | RH5202-003, RH5202-005; FC-033 | Verify + markers + CRLF | red, same |
| VerifyFixLeavesEmptyLineInsideHoleEmptyWhenShiftingLeft | RH5202-003, RH5202-005 (other direction; nothing to remove); FC-003 | Verify | green |
| VerifyFixShiftsWhitespaceOnlyLineInsideHole | RH5202-003 (whitespace-only hole line, 2 → 15 spaces); FC-003 | Verify + ExpandWhitespaceMarkers | green |
| RawStringInterpolationHoleTests.EmptyLineInsideHoleOfMisalignedLiteralIsRemoved (`src/Reihitsu.Formatter.Test/Unit/RawStringAlignment/RawStringInterpolationHoleTests.cs`) | none — characterization, decision 7 ("formatter output must not change"); FC-010, FC-012 | AssertRuleResult (LF, CRLF, second pass, phase stability) | green |
| RawStringInterpolationHoleTests.EmptyLineBetweenStatementsOfLambdaInsideHoleOfMisalignedLiteralStaysEmpty | none — characterization, decision 7: an empty hole line that survives the blank-line phase reaches the indentation phase and must stay empty in the output | AssertRuleResult | green |

Run at 2bc2fbe plus these tests: RH5202 analyzer class 12 red, 51 green (all 49 earlier tests green); formatter `RawStringInterpolationHoleTests` 45/45 green; architecture project 115/115 green (requirement tags resolve).

## Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| Audit #1 counterexample (NBSP + 3 spaces, plain, rightward) | LF, CRLF | code fix | reproduced | 16 spaces inserted vs 12 |
| U+000C, U+000B first, plain, rightward | LF | code fix | reproduced | 16 vs 12 |
| U+2003 first, `u8`, rightward | LF | code fix | reproduced | 16 vs 12 |
| NBSP first, `$"""`, rightward | LF, CRLF | code fix | reproduced | 17 vs 13 |
| NBSP behind 20 spaces, leftward | LF, CRLF | code fix | reproduced | removes 4 vs 5 |
| NBSP first, leftward | LF | code fix | reproduced (pre-existing on `main` per audit) | inserts 16 spaces vs removing 5 characters |
| Audit #2 (empty hole line, rightward) | LF, CRLF | code fix | reproduced | empty vs 13 spaces |
| Empty hole line, leftward; whitespace-only hole line | LF | code fix | holds | — |
| Empty hole line | LF, CRLF | formatter, whole document | holds (characterization) | blank line removed, as the audit observed |

## Not covered, and why

- The holding cells of finding #1 (closing whitespace of spaces only, or spaces and tabs) are covered by the existing RH5202-001/003 tests; no new test.
- A NBSP-first leftward cell under CRLF and further whitespace kinds leftward: the leftward cells are covered by the two NBSP tests above (behind spaces, LF and CRLF; first, LF). U+000B, U+000C, U+2003 reach the same measurement as NBSP in both the analyzer (`TrimStart`) and the fix, so one leftward kind is tested; the audit's enumerated failing leftward cells are both covered.
- Interpolated with `$$` and closing whitespace other than spaces: the quote offset is shared (`RawStringLiteralUtilities.GetQuoteOffset`) and the closing-width measurement is the same statement as for `$`; not in the audit's enumeration.
- Code-fix/formatter parity for finding #1 and #2 (`VerifyCodeFixMatchesFormatter`): not applicable. For #1 the formatter's own output differs (follow-up candidate in 00-lead.md); for #2 whole-document formatting removes the blank line while decision 7 keeps the fix's `main` output, so SHARED-FIX-007 decides neither.
- The empty hole line is no longer covered by RH5202-005's empty-content-line tests; their behavior is unchanged and they stay green.
- FC-004/FC-005/FC-019/FC-020 (comment, directive in a gap): the fix only writes line starts; directives in a hole are parse errors (no diagnostic). FC-018, FC-023–FC-026, FC-029, FC-036: no rename, identifiers, member kinds or key/value layout.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Formatting.RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzerTests"
scripts/test.sh --project formatter --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Unit.RawStringAlignment.RawStringInterpolationHoleTests"
scripts/test.sh --project architecture --filter "FullyQualifiedName~RequirementCoverageTests"
```

Role: test implementer
Commit: 763b30b
Verdict: tests written

Repair round 2, for lead decisions 12 and 13 (auditor finding #2 in 18-auditor.md). The prompt named the cases to cover. It carried no intended fix, suspected cause, or hoped-for verdict.

Expected values come from the requirements, not from code output:
- FMT-INDENT-031 for an anonymous method: braces under `delegate`, statements one level deeper.
- FMT-INDENT-028 for a block lambda: braces at its `(`, the body keeping its relative indentation, one level deeper as the line-break phase writes it.
- FMT-INDENT-030 and FMT-RAW-007 (first bullet): the anchor's column is the one it has after the literal moved. The anchor stands on a line that FMT-RAW-009 leaves where it is, so that column is its written column.
- One test also relies on FMT-SPACE-013 (with FMT-SPACE-004): behind a comment that `(` carries, the space in front of the right token is removed. The spec example `M( /* four */a)` shows this.

Each expected value is the pass-2 fixed point that 18-auditor.md reports for its counterexample. That is also the only output that satisfies these requirements in one pass. Every spec decides its value; no value is a guess.

## Tests

All tests are new, in `src/Reihitsu.Formatter.Test/Regression/Indentation/HoleAnchorOnUnmovedLineAlignmentTests.cs`. All use `AssertRuleResult`, which checks the exact output under LF and CRLF, a second pass, the phase check, and the tree round-trip.

The opening quotes stand in column 17 in every test:
- "Moved" means the content starts in column 4, so the literal moves 13 columns right.
- "Aligned" means the content already starts in column 17.
- "Moved left" means the content starts in column 25, so the literal moves 8 columns left.

"Expected now" is on 763b30b.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| AnonymousMethodOnVerbatimContinuationLineOneColumnLeftOfShiftIsAlignedUnderDelegate (`$@`, anchor 12, moved) | FMT-INDENT-031, FMT-INDENT-030, FMT-RAW-007; FC-003, FC-010, FC-016 | `AssertRuleResult` | fail: `{`, `return 1;`, and `}` in column 13 instead of 12/16/12 |
| AnonymousMethodOnVerbatimContinuationLineInShiftColumnIsAlignedUnderDelegate (`$@`, anchor 13, moved) | FMT-INDENT-031, FMT-INDENT-030, FMT-RAW-007; FC-003 | `AssertRuleResult` | pass |
| AnonymousMethodOnVerbatimContinuationLineInAlignedLiteralIsAlignedUnderDelegate (`$@`, anchor 12, aligned) | FMT-INDENT-031, FMT-INDENT-030, FMT-RAW-007; FC-003 | `AssertRuleResult` | pass |
| AnonymousMethodBehindCommentEndLineOneColumnLeftOfShiftIsAlignedUnderDelegate (`/* */` end line, anchor 12, moved) | FMT-INDENT-031, FMT-INDENT-030, FMT-RAW-007; FC-003, FC-010, FC-021 | `AssertRuleResult` | fail: column 13 for all three lines |
| AnonymousMethodBehindCommentEndLineInShiftColumnIsAlignedUnderDelegate (comment, anchor 13, moved) | FMT-INDENT-031, FMT-INDENT-030, FMT-RAW-007; FC-003 | `AssertRuleResult` | pass |
| AnonymousMethodBehindCommentEndLineInAlignedLiteralIsAlignedUnderDelegate (comment, anchor 12, aligned) | FMT-INDENT-031, FMT-INDENT-030, FMT-RAW-007; FC-003 | `AssertRuleResult` | pass |
| BlockLambdaOnVerbatimContinuationLineOneColumnLeftOfShiftIsAlignedAtItsParenthesis (`$@`, anchor 12, moved) | FMT-INDENT-028, FMT-INDENT-030, FMT-RAW-007; FC-003, FC-010 | `AssertRuleResult` | fail: column 13 for all three lines (pre-existing on `main`) |
| BlockLambdaOnVerbatimContinuationLineInShiftColumnIsAlignedAtItsParenthesis (`$@`, anchor 13, moved) | FMT-INDENT-028, FMT-INDENT-030, FMT-RAW-007; FC-003 | `AssertRuleResult` | pass |
| BlockLambdaOnVerbatimContinuationLineInAlignedLiteralIsAlignedAtItsParenthesis (`$@`, anchor 12, aligned) | FMT-INDENT-028, FMT-INDENT-030, FMT-RAW-007; FC-003 | `AssertRuleResult` | pass |
| BlockLambdaBehindCommentEndLineOneColumnLeftOfShiftIsAlignedAtItsParenthesis (comment, anchor 12, moved) | FMT-INDENT-028, FMT-INDENT-030, FMT-RAW-007; FC-003, FC-010, FC-021 | `AssertRuleResult` | fail: column 13 for all three lines (pre-existing on `main`) |
| BlockLambdaBehindCommentEndLineInShiftColumnIsAlignedAtItsParenthesis (comment, anchor 13, moved) | FMT-INDENT-028, FMT-INDENT-030, FMT-RAW-007; FC-003 | `AssertRuleResult` | pass |
| BlockLambdaBehindCommentEndLineInAlignedLiteralIsAlignedAtItsParenthesis (comment, anchor 12, aligned) | FMT-INDENT-028, FMT-INDENT-030, FMT-RAW-007; FC-003 | `AssertRuleResult` | pass |
| AnonymousMethodOnInterpolatedVerbatimContinuationLineLeftOfShiftIsAlignedUnderDelegate (`@$`, anchor 4, moved) | FMT-INDENT-031, FMT-INDENT-030, FMT-RAW-007; FC-016 | `AssertRuleResult` | fail: column 13 for all three lines |
| AnonymousMethodBehindSpacedCommentEndLineLeftOfShiftIsAlignedUnderDelegate (the auditor's `two */ delegate`, anchor 6 once the space is removed, moved) | FMT-INDENT-031, FMT-INDENT-030, FMT-RAW-007, FMT-SPACE-013; FC-021 | `AssertRuleResult` | fail: column 13 for all three lines |
| AnonymousMethodOnVerbatimContinuationLineRightOfShiftIsAlignedUnderDelegate (`$@`, anchor 22, moved) | FMT-INDENT-031, FMT-INDENT-030, FMT-RAW-007 | `AssertRuleResult` | pass |
| AnonymousMethodOnVerbatimContinuationLineOfLiteralMovedLeftIsAlignedUnderDelegate (`$@`, anchor 4, moved left) | FMT-INDENT-031, FMT-INDENT-030, FMT-RAW-007 | `AssertRuleResult` | pass |

On 763b30b the class has 16 tests: 6 fail and 10 pass.

Each failure is the one finding #2 describes, and the observed output matches 18-auditor.md's pass 1:
- The block's three lines stand in column 13, the shift.
- The body is not one level deeper.
- Everything else in the output matches: the break behind the anchor, the unmoved anchor line, the moved content lines, and the closing quotes.

The six passing tests are the holding members of 18-auditor.md that guard the fix:
- the anchor in the shift's column (the boundary next to "one less");
- the anchor right of the shift;
- the literal already aligned;
- the literal moved left.

## Not covered, and why

- **Anchor on the closing line of a nested multi-line raw literal, and a non-interpolated verbatim or raw literal as an earlier argument.** 18-auditor.md lists both among the members that hold. Neither has an anchor on a line that stays where it is:
  - The nested raw literal's closing line moves (FMT-RAW-008).
  - In the other member, the line-break phase puts the anchor on a line of its own, which moves.

  Anchors on moving hole lines are already pinned throughout `AnonymousMethodHoleAlignmentTests`. Pinning these two outputs would also depend on line-break requirements outside this finding.
- **The block-lambda counterpart for `@$`, the anchor right of the shift, and the literal moved left.** The block-lambda and anonymous-method members differ only in which contributor names the anchor (FMT-INDENT-028 or FMT-INDENT-031). The finding is in the floor of the placed column (`LayoutModel.ShiftRange` and `IndentationRewriter`), and that floor applies after the anchor is chosen. The boundary, the aligned, and the moved cases are covered for both members.
- **CRLF and second-pass idempotency.** `AssertRuleResult` covers both in every test. The failing tests stop at LF, the first line ending the helper checks.
- **FC-020 (directive in a hole).** No such input exists.
- **FC-007, FC-008, FC-028.** No code fix and no node-level entry point changes.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~HoleAnchorOnUnmovedLineAlignmentTests"
```

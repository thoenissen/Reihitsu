Role: test implementer
Commit: (this commit; see `git log`)
Verdict: tests written

Repair round for lead decision 15. It covers implementer finding 1 in 28-implementer.md: no test covered the alignment-sweep convergence check (`LayoutModel.CaptureColumns`/`MatchesColumns`, which now capture the column contributors read). Fresh agent. The prompt named the change and the inputs to use. It carried no suspected cause or hoped-for verdict beyond decision 15 and finding 1.

Expected values come from the requirements, not from code output:
- **The block** (FMT-INDENT-031 for the anonymous method, FMT-INDENT-028 for `() =>`; anchor measured after the move, FMT-INDENT-030 / FMT-RAW-007 bullet 1):
  - The anchor stands in column 8 behind the `/* */` end line, a line FMT-RAW-009 leaves in place.
  - `{` and `}` go in column 8, `return 1;` in 12.
  - These are the same columns as in the family A tests of 27.
- **The chain** `M(…).A()` ⏎ `.B()` (FMT-INDENT-029 / SHARED-CHAIN-007): `M(…)` is the root, not a member-access link. So the anchor is `.A`, the first invoked link in front of the first link that starts a line.
- **Where `.A` lands:** it stands behind `})` on the block's closing line, so after the block is placed it is in column 8 + 2 = 10.
- **Why `.B()` goes under it:** the chain lies inside the hole and `.A` lies in the same hole. FMT-RAW-007 bullet 1 therefore keeps the alignment measured at `.A`'s column after the move, so `.B()` goes in column 10.
- **Expected output:** the same for every input column of the `})` line and for every literal state. The literal ends with its quotes and content in column 17.

## Tests

`AssertRuleResult` checks the exact output under LF and CRLF, a second pass, the phase check, and the tree round-trip. "Expected now" is on acb2ea0 plus these test files.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `HoleAnchorInStatementColumnAlignmentTests.{AnonymousMethod,ParenthesizedLambda}ClosingLineInInputColumn6WithOuterChainBehindCommentEndLineInStatementColumnInAlignedLiteralAlignsTheChainUnderItsAnchorLink` (2 tests) | FMT-INDENT-031 or FMT-INDENT-028, FMT-INDENT-029, FMT-INDENT-030, FMT-RAW-007; FC-010, FC-016 | `AssertRuleResult` | pass |
| `…ClosingLineInInputColumn14…InLiteralMovedLeft…` (2 tests; literal content in input column 25) | same | `AssertRuleResult` | pass |
| `…ClosingLineInInputColumn8…InAlignedLiteral…` (2 tests, control: the `})` line already in its block column) | same; FC-003 | `AssertRuleResult` | pass |
| `…ClosingLineInInputColumn16…InLiteralMovedLeft…` (2 tests, control) | same; FC-003 | `AssertRuleResult` | pass |
| `LayoutModelTests.CaptureColumnsCapturesRegisteredColumnOfUnplacedHoleLine` | none (mechanism of the convergence check); FC-016 | unit | pass |
| `LayoutModelTests.MatchesColumnsAfterZeroShiftPlacesHoleLineAwayFromItsRegisteredColumnReturnsFalse` | none; FC-016 | unit | pass |
| `LayoutModelTests.MatchesColumnsAfterSetPlacesHoleLineAtItsStandInColumnReturnsFalse` (the `Set` path that 28 names as the same mechanism) | none; FC-016 | unit | pass |
| `LayoutModelTests.MatchesColumnsAfterHoleLineIsPlacedInItsRegisteredColumnReturnsTrue` (control: the column read does not change) | none; FC-003 | unit | pass |
| `LayoutModelTests.MatchesColumnsAfterWriteFromOutsideTheHoleReturnsTrue` (control: the write is rejected, so the line stays unplaced) | none; FC-003 | unit | pass |

The unit tests carry no requirement tag. They verify the sweep's internal convergence snapshot, which no requirement states. The requirement behind them (idempotency, FMT-PIPE-004, through FMT-INDENT-030) is tagged on the regression tests.

**Results:**
- Both classes on head: Passed 108 (88 + 20).
- Full formatter project: Passed 4274, Failed 0.
- Architecture tests: Passed 92.

**Mutation.** In a scratch worktree at acb2ea0, only the `CaptureColumns`/`MatchesColumns` part of the commit was reverted (both `GetCapturedColumn(entry.Key, entry.Value)` calls back to `entry.Value.Column`). The `delta == 0` removals stayed. Result: Failed 7, Passed 101.
- **The four column-6 and column-14 regression tests fail for the expected reason.** On the first pass, under LF, `.B()` lands in column 8 instead of 10, and every other line matches. Column 8 is the block-indentation stand-in, which the chain read before the lambda placed the `}` line.
- **Three unit tests fail:**
  - `CaptureColumns…`: expected 6, actual 8.
  - `…ZeroShift…ReturnsFalse` and `…SetPlaces…ReturnsFalse`: the check returned true.
- **The four controls and the two `ReturnsTrue` unit tests pass under the mutation.** So the tests cover both sides of the condition.
- The worktree is removed (`git worktree list` shows only the main checkout).

## Not covered, and why

- **The async anonymous method, the async lambda and the simple lambda; the other unmoved kinds (`/** */` end line, verbatim continuation lines); the moved-right literal.**
  - 28 reports these as stable in its probe only for the two constructs it ran. No non-idempotent member was named outside the anonymous method / `() =>` × aligned / left-moving set.
  - The convergence check sits in one place, `LayoutModel.MatchesColumns`, whose one production caller is `LayoutComputer.RunAlignmentSweeps` (`rg -n "CaptureColumns|MatchesColumns" src --glob '!**/obj/**'`). The unit tests pin that check directly.
- **A sweep of the `})` input column beyond 6/8 and 14/16.**
  - 28's probe ran columns 0 to 16 and found only 6 (aligned) and 14 (moved left) failing without the snapshot change.
  - 8 and 16 are the holding neighbors used as controls.
- **CRLF and second pass.** `AssertRuleResult` runs both in every regression test. The failing tests under the mutation stop at LF.
- **FC-020** (directive in the gap): a directive cannot stand in a hole. **FC-007, FC-008, FC-028:** no code fix and no node-level entry point in scope.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~HoleAnchorInStatementColumnAlignmentTests|FullyQualifiedName~LayoutModelTests"
```

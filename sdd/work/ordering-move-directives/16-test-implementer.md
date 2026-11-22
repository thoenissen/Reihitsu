Role: test implementer
Commit: eab9a97
Verdict: tests written

Repair round 1 for finding #1 of `14-auditor.md` (#1668, SHARED-ORDER-008 RH7309 clause, answer 1668-2 A). The prompt carried no intended fix, suspected cause, or hoped-for verdict to ignore. No production code or spec was touched.

## Tests

Both in `tests/Reihitsu.Analyzer.Test/Formatting/RH7309RegionsShouldFollowCategoryOrderTriviaTests.cs`, behind `NoCodeFixWhenAGroupInFrontOfTheFirstBlockPrecedesAReorderOfInstanceFields`.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `NoCodeFixWhenAGroupBetweenTwoReorderedRunsPrecedesAReorderOfInstanceFieldsThatOpensAStruct` (the auditor's counterexample: blocks `Object`, `Methods`, then `#if X` / `}` / `public struct S` / `{` / `#endif`, then `IDisposable` with `_d` and `Base` with `_b`; reorder to `Methods`, `Object`, group, `Base`, `IDisposable`) | RH7309-005, SHARED-ORDER-008 | `VerifyNoCodeFixIsOffered` | **fails on eab9a97**: "No code fix may be offered for the diagnostic at [205..220) — expected count 0, actual 1" (the `#region Methods` diagnostic); both diagnostics reported as marked |
| `NoCodeFixWhenAGroupOfMembersBetweenTwoReorderedRunsPrecedesAReorderOfInstanceFields` (same layout, the group is an ordinary `#if DEBUG` / `private int _trace;` / `#endif`; 1668-2 A says any `#if` block in front of the moved fields) | RH7309-005, SHARED-ORDER-008 | `VerifyNoCodeFixIsOffered` | **fails on eab9a97** for the same reason |

Both fail on the assertion that no action is registered, not on diagnostics or compilation: the reported diagnostics match the markup.

## Members of the enumerated class the auditor lists as holding — coverage

| Member | Pinned by |
|---|---|
| Group in front of the first run | `NoCodeFixWhenAGroupInFrontOfTheFirstBlockPrecedesAReorderOfInstanceFields` (RH7309 Trivia) |
| Group inside a run (crossed code) | `ConditionalGroupInCodeThatAMovedBlockCrossesBetweenTwoReorderedPairsWithholdsTheFix`, `ConditionalPairInsideAKeptBlockThatMovedBlocksCrossWithholdsTheFix`, `VerifyNoCodeFixWhenAMovedRegionCrossesBalancedConditionalDirectivesBetweenRegions` |
| Group behind the last run | `ConditionalGroupBetweenBlocksThatNoMovedBlockCrossesDoesNotWithholdTheFix` |
| Group between runs, second run reorders only methods (no instance field) — the other side of the repaired condition (FC-003) | `ConditionalGroupBetweenTwoPairsReorderedOnTheirOwnSideDoesNotWithholdTheFix` (spec example 2, `Verify` with exact output); it must stay green after the repair |
| Second run reorders initialized members | `RH7309RegionsShouldFollowCategoryOrderInitializationOrderTests` (initializer-order refusal, independent of groups) |
| Type is a struct | `RH7309RegionsShouldFollowCategoryOrderInitializationOrderTests` struct-layout tests (struct already layout-sensitive) |
| Whole-type wrap exception | `GroupThatWrapsTheWholeTypeKeepsAReorderOfInstanceFields` |
| Member moves RH7102/RH7103/RH7109 (one contiguous run) | RH7109 TypeGroup layout tests (`04`); no new test: one run, so the anchor is unchanged |

## Not covered, and why

- CRLF: `VerifyNoCodeFixIsOffered` runs one line ending; the decision reads directive positions, not line breaks, and the auditor ran the counterexample under LF and CRLF with `scripts/apply-fix.sh` (fixed on HEAD under both). RH7309-007 owns line-ending behavior.
- Fix All: a no-fix test; nothing to batch.

## Commands

- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH7309RegionsShouldFollowCategoryOrderTriviaTests"`: Failed 2 (the two new tests), Passed 47, on eab9a97.
- `scripts/format.sh` on the test file: 0 of 1 changed.

## Follow-up candidates

_None._

## Addendum — spec author 15 (head da48669, decision 13)

Inputs: `15-spec-author.md` and the spec diff of db9eed8 (anchor at the last instance field of the spans; new RH7309-005 example and its prose opposite; the `A B C D` → `B A D C` example in SHARED-ORDER-008). The coordinator's message carried no intended fix or hoped-for verdict to ignore.

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `NoCodeFixWhenAGroupOfMembersBetweenTwoReorderedRunsPrecedesAReorderOfInstanceFields` — **changed** (my own test from the first part, not yet worked against): its group is now the spec's `#if DEBUG` / `public void Trace()` method group, so the test is the new RH7309-005 example verbatim | RH7309-005, SHARED-ORDER-008 | `VerifyNoCodeFixIsOffered` | fails on da48669: 1 action offered for the diagnostic at [205..220) |
| `FixReordersInstanceFieldsOfTheFirstRunInFrontOfAGroupBetweenTwoReorderedRuns` — the prose opposite: `_o` in `Object`, `_m` in `Methods`, `IDisposable`/`Base` hold only `Dispose`/`Reset`; output `Methods`, `Object`, group, `Base`, `IDisposable`, `_m` in front of `_o` | RH7309-005, SHARED-ORDER-008 | `Verify` (exact fixed and batch state) | pass (the other side of the repaired condition; must stay green) |
| `FixReordersTheBlocksWhenAGroupBetweenTwoReorderedRunsPrecedesAnInstanceFieldWhoseOrderStays` — `_o` in `Object`, `_d` in `IDisposable`, the same group: an instance field of the spans stands behind the group, but the reorder keeps `_o` in front of `_d` | SHARED-ORDER-008 (and SHARED-ORDER-007: the clause refuses only a move that reorders instance fields), RH7309-005 | `Verify` | pass; pins the "reorders instance fields" side of the condition (FC-003) so the new anchor does not over-refuse |

### Spec examples — coverage

- New RH7309-005 no-fix example: the changed test above; the auditor's struct-opening counterexample stays as `NoCodeFixWhenAGroupBetweenTwoReorderedRunsPrecedesAReorderOfInstanceFieldsThatOpensAStruct` (still red).
- Prose opposite: `FixReordersInstanceFieldsOfTheFirstRunInFrontOfAGroupBetweenTwoReorderedRuns`.
- SHARED-ORDER-008 `A B C D` → `B A D C`: a directive between `B` and `C` counts when `C` or `D` holds an instance field — the two red no-fix tests; does not when only `A` or `B` holds one — the opposite test; a directive between `A` and `B` refuses — `ConditionalGroupInCodeThatAMovedBlockCrossesBetweenTwoReorderedPairsWithholdsTheFix`; no instance field at all — `ConditionalGroupBetweenTwoPairsReorderedOnTheirOwnSideDoesNotWithholdTheFix`.
- SHARED-ORDER-008 *for a member move, this place is the body in front of the crossed span*: unchanged behavior, pinned by the RH7109 TypeGroup layout tests (`04`).

### Spec versus real output

`scripts/apply-fix.sh RH7309 <scratchpad fixtures>` on da48669, LF and CRLF:
- prose opposite: fixed, 1 action, 1 iteration; the diff is exactly the spec's description (`Methods`, `Object`, group, `Base`, `IDisposable`; `_m` in front of `_o`). The `Verify` test asserts that text and passes.
- one-field variant (`_o`, `_d`): fixed, same block order.
- new no-fix example: fixed on head (the defect under repair), as the red test shows.

**No mismatch between spec and real output** for the fix-offered cases.

### Not covered, and why

- CRLF for the no-fix tests: as above. The two `Verify` tests run one line ending; `apply-fix.sh` checked both.

### Commands

- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH7309RegionsShouldFollowCategoryOrderTriviaTests"` on da48669: Failed 2 (`NoCodeFixWhenAGroupBetweenTwoReorderedRunsPrecedesAReorderOfInstanceFieldsThatOpensAStruct`, `NoCodeFixWhenAGroupOfMembersBetweenTwoReorderedRunsPrecedesAReorderOfInstanceFields`), Passed 49.
- `scripts/format.sh` on the test file: 0 of 1 changed.

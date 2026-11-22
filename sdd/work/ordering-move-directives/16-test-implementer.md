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

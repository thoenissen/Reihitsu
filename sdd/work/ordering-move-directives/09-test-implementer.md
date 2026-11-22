Role: test implementer
Commit: 1700b09
Verdict: tests written

Scope: the two RH7309-005 / SHARED-ORDER-008 examples `07-spec-author.md` added (commit c0e555b), and the negative twin the sentence behind example 2 states. The prompt carried no intended fix, suspected cause, or hoped-for verdict to ignore. No production code or spec was touched.

## Tests

All in `tests/Reihitsu.Analyzer.Test/Formatting/RH7309RegionsShouldFollowCategoryOrderTriviaTests.cs`, behind `ConditionalGroupBetweenBlocksThatNoMovedBlockCrossesDoesNotWithholdTheFix`.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `ConditionalPairInsideAKeptBlockThatMovedBlocksCrossWithholdsTheFix` (example 1: `IDisposable`, `Object`, `Methods` → `Methods`, `Object`, `IDisposable`; `#if DEBUG` in `ToString` of the kept `Object` block; both `Object` and `Methods` reported) | RH7309-005, SHARED-ORDER-008 | `VerifyNoCodeFixIsOffered` | pass |
| `ConditionalGroupBetweenTwoPairsReorderedOnTheirOwnSideDoesNotWithholdTheFix` (example 2, input and output exactly as in the spec) | RH7309-005, SHARED-ORDER-008 | `Verify(testData, fixedData)` | pass |
| `ConditionalGroupInCodeThatAMovedBlockCrossesBetweenTwoReorderedPairsWithholdsTheFix` (example 2's twin: the same group between `Object` and `Methods`) | RH7309-005, SHARED-ORDER-008 | `VerifyNoCodeFixIsOffered` | pass |

Fail-first check: on f9b2eda (before the implementation commit), in a temporary worktree, `scripts/apply-fix.sh RH7309` offered and applied the fix for example 1 and for the twin under LF and CRLF. So both no-fix tests were red there for the right reason: a fix was offered. Example 2 was also fixed on f9b2eda, so its test is green both before and after the change. It pins the uncrossed-gap side of the condition against the twin (FC-003), not a change from the old behavior.

## Example 2 output confirmed

`scripts/apply-fix.sh RH7309 <fixtures>` on 1700b09, with the spec inputs written as fixtures: example 2 was fixed in one action and one iteration under both LF and CRLF. Its diff gives exactly the spec's after-output: blocks in the order `Methods`, `Object`, group, `Base`, `IDisposable`, with the group and its blank lines in place. Example 1 and the twin returned `no-fix-offered` under both line endings. The `Verify` test asserts the spec's after-text verbatim and passes. **No mismatch between the spec and the real output.**

## Not covered, and why

- Coverage first: the other side of example 1, a kept block with a conditional pair that no block crosses, is already pinned by `ConditionalPairInsideABlockThatKeepsItsPlaceDoesNotWithholdTheFix`. A group in a gap behind the last moved block, where the old reordered-span rule already exempted it, is pinned by `ConditionalGroupBetweenBlocksThatNoMovedBlockCrossesDoesNotWithholdTheFix`. A group crossed in a two-block type is pinned by `VerifyNoCodeFixWhenAMovedRegionCrossesBalancedConditionalDirectivesBetweenRegions`.
- The gap between `IDisposable` and `Base` in example 2's layout, which `Base` crosses: no new test. It reaches the same `GetCrossedPositions` decision as the twin, from the suffix-minimum side instead of the prefix-maximum side. Both sides are computed in the one method, so this is not the single-call-site proof that `README.md` asks for. If the lead wants it, it is one more no-fix test.
- Line endings: the RH7309 verifier runs one line ending per test. `apply-fix.sh` checked LF and CRLF for all three inputs. The conditional-crossing decision reads no line breaks, and RH7309-007 owns line-ending behavior.
- Fix All: example 2 has two diagnostics in one type, and `Verify` on the batch base applies them in one pass (RH7309-004 "one application").

## Commands

- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH7309RegionsShouldFollowCategoryOrderTriviaTests"`: Passed 47 of 47.
- `scripts/apply-fix.sh RH7309 <scratchpad fixtures>` on 1700b09 and on f9b2eda, as above.
- `scripts/format.sh` on the test file: 0 of 1 changed.

## Follow-up candidates

_None._

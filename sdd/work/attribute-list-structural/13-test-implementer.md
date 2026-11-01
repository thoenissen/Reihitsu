Role: test implementer
Commit: 9a97d4e (tests written against this head; production code as audited at 4404167)
Verdict: tests written

Repair round 2: the test side of audit findings 1–5 (`11-auditor.md`) as lead decisions 16–20 settle them and `12-spec-author.md` amends FMT-STRUCT-028, FMT-STRUCT-029 and FMT-BREAK-032. My prompt named those decisions as the change asked for and carried no intended fix, suspected cause, or hoped-for verdict beyond them.

Expected values come from the amended spec examples and requirements. Where a requirement leaves placement unspecified and the lead's decisions ask for `main`'s output, the expected value is `main`'s output, measured with `main`'s CLI (archive of `3a7bbea` in the session scratchpad) and named so below. Every final-output test whose expected value is `main`'s output was also run on that archive: all pass there, phase-level checks of `AssertRuleResult` included. The tests that fail on `main` there are exactly the visible change 1 rows (`main` needs two passes): `LineBreaksBetweenTwoAttributesAreDroppedWithTheComma`, `LineBreakBehindTheCommaSettlesInOnePassOnEveryOwner`, `AccessorSplitDropsTheLineBreaksBetweenTwoAttributes`, `AccessorSplitDropsTheLineBreakBehindTheOpeningBracket`, `LambdaAttributeListWithALineBreakBehindTheOpeningBracketSettlesInOnePass`.

## Tests

A = `src/Reihitsu.Formatter.Test/Unit/StructuralTransforms/AttributeListStructuralTransformTests.cs` (structural-phase output, LF and CRLF, no elastic trivia, FMT-PIPE-021 round trip); B = `src/Reihitsu.Formatter.Test/Regression/Structural/AttributeListStructuralFinalOutputTests.cs` (final output).

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| A `NothingBehindTheOpeningBracketStaysInTheFirstCreatedList` (`[⏎ A, B]`, `[⏎ A,⏎ B]`, `[  A, B]`) — finding 3 | FMT-STRUCT-028 | phase output | fails: first created list keeps `[⏎        A]` / `[  A]` |
| A `LineBreakInTheStretchIsKeptOnce`, new rows `BehindACommaOfTheFirstList`, `InFrontOfACommaOfTheFirstList`, `InFrontOfTheClosingBracketOfTheFirstList` — finding 4 | FMT-STRUCT-029 | phase output, line breaks between merged `]` and type = 1 | fails: 0 line breaks |
| A same test, new rows `InFrontOfTheClosingBracketOfARemovedList`, `BehindTheOpeningBracketOfARemovedList` (auditor's holding members) | FMT-STRUCT-029 | same | passes |
| A `LineBreakDirectlyBehindTheOpeningBracketStaysThere` (`void W([⏎ A] [B] int x, int y)`) | FMT-STRUCT-029 | phase output, line break between merged `[` and `A` | passes (holding member; other side of the "directly behind `[`" condition) |
| A `ParamAndTypevarListsOfAnotherOwnerAreMergedAndNotSplit` rows `MethodExample` (spec example: `[param: A,B]`, `[typevar: C, D]`, `[E]`, `[F]`), `Delegate`, `Class` (`typevar:`), `Property`, `EnumMember`, `LocalFunction` — finding 1 | FMT-STRUCT-029 | phase output, owner's list texts | fails: lists not merged |
| A same test, rows `ParamListWithSeveralAttributes`, `TypevarListWithSeveralAttributes` (not split; `[C]` beside it unaffected) | FMT-STRUCT-029 | same | passes (other side: never split) |
| B `LineBreaksBetweenTwoAttributesAreDroppedWithTheComma` — extended with the spec's new `void P()` row (`[⏎        A, B]`) | FMT-STRUCT-028 | `AssertRuleResult` | fails: `[⏎    A]` (P row) |
| B `AccessorSplitDropsTheLineBreakBehindTheOpeningBracket` (`[⏎ C, D] get;` in a multi-line accessor list) | FMT-BREAK-032 | `AssertRuleResult` | fails: `[⏎        C]` |
| B `LambdaAttributeListWithALineBreakBehindTheOpeningBracketSettlesInOnePass` (`main`'s fixed point, in one pass) | — characterization (change doc visible change 1, lambdas; no requirement) | first + second pass, LF/CRLF, no phase check (as `LambdaAttributeListKeepsItsOutput`) | fails: `[⏎        A]` |
| B `LineBreakBetweenMergedListsStillWrapsTheParameterList` — extended with the spec's new `void S([A,⏎ C] [B] int x, int y)` row | FMT-STRUCT-029 | `AssertRuleResult` | fails: S not wrapped |
| B `LineBreakInsideTheFirstMergedListStillWrapsTheParameterList` (record `[A,⏎ C] [B]`, method `[A⏎ , C] [B]`, local function `[A,⏎ C] [B]`; `main`'s output) | FMT-STRUCT-029 | `AssertRuleResult` | fails: parameter lists not wrapped |
| B `LineBreakInFrontOfTheClosingBracketOfTheFirstMergedListStillWrapsTheParameterList` (`[A⏎ ] [B]`) | FMT-STRUCT-029 | first and second pass, LF/CRLF: merged, `] int x,⏎           int y)` | fails: not wrapped |
| B `ParamListsOfASetterInAnAccessorListOverSeveralLinesAreMerged` (spec example) | FMT-BREAK-032 | `AssertRuleResult` | fails: `[param: A] [param: B] set` |
| B `ParamAndTypevarListsOfEveryAccessorInAnAccessorListOverSeveralLinesAreMerged` (`init`, indexer setter, event `remove`, `typevar:` setter, `[param: A, B] get` not split; `main`'s placement) | FMT-BREAK-032 | `AssertRuleResult` | fails: lists not merged |
| B `ParamListsOfASetterInAnAccessorListOnTheDeclarationsLineAreMerged` (`int R { get; [param: A] [param: B] set; }`) | FMT-BREAK-032 | `AssertRuleResult` | passes (other side of the layout condition) |
| B `ParamAndTypevarListsOnOtherOwnersKeepTheirOutput` (delegate, class, spec method example, property, `[param: A, B] void O()`, local function; `main`'s output) | — characterization (placement unspecified, FMT-BREAK-027; shape asserted by A) | `AssertRuleResult` | fails: lists not merged |
| B `ParamListsOfALambdaKeepTheirOutput` (`var f = [param: A] [param: B] (int x) => x;`; `main`'s output) | — characterization (lambdas unspecified; decision 16 "every owner") | first + second pass, no phase check | fails: not merged |
| B `DirectiveLeadingTheFirstListBlocksTheMerge` — finding 5: now asserts `void M(int x,⏎`, `#pragma warning disable CS0618⏎`, `[A] [B] int a)` once each, in order | FMT-STRUCT-029 | `AssertSecondPassIsStable` + ordered parts | passes |
| B `DocumentationCommentBetweenTheListsBlocksTheMerge` — finding 5: now also asserts `void M([A]`, `/// <summary>`, `/// x`, `/// </summary>`, `[B] int p)` once each, in order | FMT-STRUCT-029 | `AssertSecondPassIsStable` + ordered parts | passes |

Changed without changing assertions: `LambdaAttributeListKeepsItsOutput` now calls the new private helper `AssertFirstPassAndSecondPass`, which holds its former loop unchanged. The two spec-example tests in B were extended because the spec author extended the examples they verify; their earlier rows are untouched.

Every failing test was checked to fail on its asserted difference (list text, line-break count, merge, or wrap), not on a compile error or unrelated layout. `scripts/test.sh --project formatter --filter "FullyQualifiedName~AttributeListStructural"`: 22 failed, 76 passed. `scripts/test.sh --project architecture`: 92 passed.

## Not covered, and why

- **The documentation comment's own layout** in `DocumentationCommentBetweenTheListsBlocksTheMerge`: the documentation-comment phase rewrites `/// <summary>x</summary>` into three lines and the blank-line phase inserts a blank line in front of it (follow-up 5). The test asserts the comment's parts survive in order between the two lists, not their layout.
- **Whitespace written in front of a first list's `]`** (`[A⏎ ] [B]`): `main` and head both need a second pass to drop it (auditor hint, follow-up 3); the test asserts the merge and the wrap on both passes, not the first pass's full text.
- **`[⏎ A,⏎ B⏎ ]`** (auditor's further member of finding 3): its `[`-part is the `[⏎ A, B]` row; what stands in front of `]` is outside FMT-STRUCT-028, and `LineBreakInFrontOfTheClosingBracketKeepsItsOutput` already characterizes it.
- **Structural phase leaving accessor `param:` lists alone**: internal choice 1 puts the accessor merge in the line-break phase; a phase-level test would pin the placement of the merge rather than a requirement. The B accessor tests assert the final shape.
- **Placement of `param:`/`typevar:` lists on non-parameter owners**: unspecified (FMT-BREAK-027); asserted only as `main`'s output in untagged characterizations.
- **Code-fix side (RH5524 parity, finding 1)**: no code-fix behavior changes; `ParamAndTypevarListsOfEveryAccessor…` restores the output RH5524's fix produces (auditor measured it), so the existing RH55xx tests and these together cover the parity.
- **Failure classes walked**: FC-003 (both sides of: owner kind for `param:`/`typevar:`, accessor layout, line break directly behind `[` versus elsewhere, split versus merge — covered), FC-004/FC-017 (finding 5 — covered), FC-010 (LF/CRLF and second pass in every new test), FC-013 (formatter vs RH5524 — see above), FC-016 (split trivia behind `[` — covered on declaration, accessor, lambda), FC-001/FC-002 (line break inside the first list — covered on method, record, local function; type-parameter and indexer lists hold and are covered by `LineBreakBetweenMergedListsOfBracketedListsIsJoined`). Others as in `04-test-implementer.md`; none reaches the repair's code differently.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~AttributeListStructural"
scripts/test.sh --project architecture
```

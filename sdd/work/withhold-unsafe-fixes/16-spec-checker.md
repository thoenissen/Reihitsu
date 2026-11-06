Role: spec checker
Commit: 7d97760
Verdict: findings

Repair round 1 (`551f629..7d97760`). Inputs: `00-lead.md` (decisions 6-10), `09-spec-checker.md`, `10-auditor.md`, `11`-`15`, the specs and change docs, the repair's tests. The prompt carried no intended fix or hoped-for verdict.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| 1 | 1, 2 | `sdd/changes/2026-10/withhold-unsafe-fixes.md` (`Affected requirements`, the rule table under *What*, *Visible output changes*) against `sdd/specs/shared/ordering-move-safety.md` SHARED-ORDER-007 | The repair round gave SHARED-ORDER-007 new normative text: a member in `#if`/`#elif`/`#else` excluded code counts as a crossed initializer, so the RH7103 and RH7109 fixes are withheld where `main` and the approved plan offered them (decision 6; tests `NoCodeFixWhenTheMoveCrossesAnInitializedStaticFieldOfExcludedCode`, `NoCodeFixWhenTheMoveCrossesAnInitializedFieldOfExcludedCode`). The change doc names neither SHARED-ORDER-007 nor RH7103-007 among the affected requirements, its RH7109 row mentions only the layout guard, it has no RH7103 row, and *Visible output changes* has no before/after for the excluded-initializer case. The maintainer's approval (`00-lead.md`, answer 3) covers the doc as of c2e1f67, which did not contain this case. Decision 6 calls it demanded by the accepted *Build configurations* bullet, so whether it needs a new maintainer answer is the lead's call; the doc has to state it either way (check 1: every visible output change is listed with its answer in `Decisions`). | spec author |

## Checked
1. Change doc: finding 1. Both docs stay `Accepted`, no task list, `Visible output changes` present; the rest of the repair's output changes are `main`'s output restored (RH7004 a-e, the comment that does not end its line, RH7309 `field` before C# 14) or are listed (RH7109 and RH7309 layout, now by type symbol across partial parts; RH7309 documentation comment behind the last block; RH7501 `break` behind the opening brace).
2. Specs match the diff: finding 1. Otherwise ok. RH7501-004 (first token on its line, comment in front of the `break` moves with it, new `case 4` example) matches `CanMoveBreak` and the tests `VerifyCodeFixMovesBreakWithItsCommentsBehindBlock`, `NoCodeFixForTheSharedLineSpecificationExample`, `NoCodeFixWhenTheBreakSharesItsLineWithTheOpeningBrace`. SHARED-ORDER-007's two new examples mirror `NoCodeFixWhenTheMoveCrossesAnInitializedFieldOfExcludedCode` and `FixMovesExcludedCodeInFrontOfTheMovedFieldWithIt`; the second now has a block-bodied `Log` as decision 9 says and matches the fix output. The *Run time* bullet of SHARED-FIX-003 ("any other type declared with `[StructLayout(LayoutKind.Sequential)]`") and the pair wording ("a directive and the directive that ends its effect") are read by the new code (symbol-based layout over every part, named-colon arguments, pairing per warning with the end of the file no partner) without a contradiction, so no spec text had to change for them. Format of the changed requirements ok (normative sentence, `Why:`, markup in examples).
3. IDs: ok. No ID added, removed, or renumbered in the round; `requirement-ids.txt` unchanged.
4. Code owners: ok. SHARED-ORDER-007's header gained `SHARED-FIX-003` under `Uses` only; `RegionBlockLayout` stays internal and unnamed. `SpecConsistencyTests` passed within the architecture run.
5. User documentation: ok. No rule page changed by the round or needed to.
6. Open questions: ok. The round's handovers state none open; decisions 6-10 are in `00-lead.md`. The dropped `SplitsDirectivePair` candidate (decision 10) is not recorded as a follow-up, as decided.
7. Coverage: ok. `scripts/test.sh --project architecture`: "Passed!  - Failed: 0, Passed: 115, Skipped: 0, Total: 115". SHARED-ORDER-007 has tagged tests in RH7103 and RH7109 for both its negative and positive example.
8. Test tags: ok. The tests of `11-test-implementer.md` carry the tags its table lists (checked against the diff for RH7004, RH7103, RH7109, RH7309, RH7501, including `FixKeepsACommentBehindTheUsingKeyword` and `FixKeepsACommentInFrontOfTheSemicolon`, tagged SHARED-FIX-003). The spec author named no further example.
9. Nothing left behind: ok. `sdd/work/` holds only `withhold-unsafe-fixes/`.

## #1136
Finding 1 touches the run's shared condition only through SHARED-ORDER-007; SHARED-FIX-003 itself is consistent with the repaired pairing and layout code. No other findings.

## #1134
Consistent. RH7109-006 and SHARED-FIX-003 *Run time* match the symbol-based layout decision and its tests (partial class, record, Auto/Explicit on another part, named argument). The excluded-initializer withhold of RH7109 is finding 1.

## #1135
Finding 1 (RH7103, the adjacent fix). RH7309-006 bullets 1 and 2 match the shared owner and the tests, including `field` before C# 14.

## #1262
Consistent. RH7309-006 bullet 4 (documentation comment in the closing brace's leading trivia) matches `ChangesDocumentedDeclaration` and its three tests.

## #1332
Consistent. RH7501-004 and the method summary agree.

## #1336
Consistent. RH7004-004 (comment that does not end its line stays behind the new block; the full-output tests pin the placement) and the restored a-e cases match the tests.

## #1340
Consistent. No change in the round.

## Whole run
- Architecture: 115 of 115 passed (quoted above).
- Output questions: none from this check; finding 1 may need one for the lead's decision whether the excluded-initializer withhold of RH7103/RH7109 needs the maintainer's answer. Architecture findings: none.

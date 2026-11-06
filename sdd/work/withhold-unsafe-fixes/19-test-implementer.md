Role: test implementer
Commit: 01c0469
Verdict: tests written (structural change and repair round 2)

Inputs: `00-lead.md` (decisions 12 and 13), `11-test-implementer.md`, `18-auditor.md` (findings #9, #10, #11), and the specs as the spec author committed them in bf155df while this round ran: SHARED-ORDER-007 (rewritten), RH7103-007 (layout clause added), RH7109-006, SHARED-FIX-003 *Run time* and *Build configurations*, RH7004-004, and case 10 of `sdd/changes/2026-10/withhold-unsafe-fixes.md`. The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the worklist. I treated the audit's suggested changes as suggestions. Expected values come from the spec examples and the change doc's fixtures. Where a counterpart has a fix output, the expected value is the RH7103-005/RH7109-005 move (SHARED-ORDER-002: excluded code in the leading trivia travels with the declaration).

All tests are in `src/Reihitsu.Analyzer.Test`. "Fails" means it fails on 1817f3b's production code, which this round has not changed. The new tests that pass pin behavior the code already has (audit: "the code is right today") and that no earlier test pinned.

## thoenissen/Reihitsu#1135 — finding #9 (decision 12)

`RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzerTests`

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `NoCodeFixWhenExcludedCodeInFrontOfTheMovedStaticFieldWouldReorderTheInstanceFieldsOfAStruct` (fixture h, the SHARED-ORDER-007 / RH7103-007 layout example) | SHARED-FIX-003, SHARED-ORDER-007, RH7103-007 | `VerifyNoCodeFixIsOffered` | **fails**: `Assert.IsEmpty(actions)`, one action is registered. This is finding #9 itself: RH7103 passes `fieldOrderSetsLayout: false` |
| `FixMovesAStaticFieldOfAStructAcrossAnInstanceField` (the spec's counterpart: the same struct without `_y`) | RH7103-007 | `Verify` | passes |
| `FixCarriesAnInstanceFieldOfExcludedCodeInFrontOfTheMovedStaticFieldOfAClass` (same shape in a class, whose field order sets no layout) | SHARED-FIX-003, SHARED-ORDER-007 | `Verify` | passes |

The two counterparts each flip one condition of the withheld test: the excluded instance field, and the layout type. With both, a fix that withholds for every struct, or for every excluded code in the trivia, fails a test.

## thoenissen/Reihitsu#1134 — finding #10 (decision 13)

`RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsAnalyzerTests`

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `NoCodeFixWhenExcludedCodeInFrontOfTheMovedFieldWouldCarryAnInitializerAcrossAnInitializedField` (fixture a, the SHARED-ORDER-007 example verbatim) | SHARED-ORDER-007, RH7109-006 | `VerifyNoCodeFixIsOffered` | passes |
| `FixCarriesAnInitializedFieldOfExcludedCodeInFrontOfTheMovedFieldAcrossAFieldWithoutInitializer` (fixture a with `_a` uninitialized) | SHARED-ORDER-007 | `Verify` | passes |
| `NoCodeFixWhenExcludedCodeInFrontOfTheMovedStaticFieldWouldReorderTheInstanceFieldsOfAStruct` (fixture c, change doc case 10's `_r` input) | SHARED-ORDER-007, SHARED-FIX-003, RH7109-006 | `VerifyNoCodeFixIsOffered` | passes |
| `FixCarriesAnInstanceFieldOfExcludedCodeInFrontOfTheMovedStaticFieldOfAClass` (fixture c in a class) | SHARED-ORDER-007, SHARED-FIX-003 | `Verify` | passes |
| `FixMovesAStaticFieldOfAStructAcrossAnInstanceField` (fixture c without `_y`) | SHARED-FIX-003 | `Verify` | passes |

The initializer counterpart is the closest other side of fixture a: the moved `_b` has no initializer in either test, so only "a crossed member of its context has one" changes. The existing `FixMovesExcludedCodeInFrontOfTheMovedFieldWithIt` (the spec's positive example) keeps covering a moved declaration that has an initializer of its own.

## thoenissen/Reihitsu#1136 — finding #11

`RH7004UsingDeclarationsShouldNotBeUsedAnalyzerTests`. All five use the shape of the existing `NoCodeFixWhenTheNewBlockWouldSplitAPragmaPair`: the opening directive stands in front of the first following statement and moves into the block, and its partner stands in front of the enclosing `}` and stays behind the block.

| Test | Audit fixture | Requirement / failure class | Helper | Expected now |
|---|---|---|---|---|
| `NoCodeFixWhenTheNewBlockWouldSplitAPragmaPairThatNamesTheWarningOnceByNumberAndOnceWithItsPrefix` (`disable 219` … `restore CS0219`) | g1 | SHARED-FIX-003 | `VerifyNoCodeFixIsOffered` | passes |
| `NoCodeFixWhenTheNewBlockWouldSplitADisableOfOneWarningFromARestoreOfEveryWarning` (`disable CS0219` … `restore`) | g7 | SHARED-FIX-003 | same | passes |
| `NoCodeFixWhenTheNewBlockWouldSplitANullablePairOfTheWarningsSetting` (`disable warnings` … `restore warnings`) | g3 | SHARED-FIX-003 | same | passes |
| `NoCodeFixWhenTheNewBlockWouldSplitANullableDirectiveOfBothSettingsFromOneOfTheWarningsSetting` (`disable` … `restore warnings`) | g8 | SHARED-FIX-003 | same | passes |
| `FixIsOfferedForNullableDirectivesOfDifferentSettingsOnBothSidesOfTheNewBlock` (`disable annotations` inside … `enable warnings` behind) | g4 | RH7004-004, SHARED-FIX-003 | `Verify` | passes |

g4's expected output puts `#nullable disable annotations` inside the new block in front of `s.Flush();` and leaves `#nullable enable warnings` in front of the method's `}`, behind the new block. This is the same placement as the existing `FixIsOfferedInsideAConditionalPairThatEndsInFrontOfTheClosingBrace`, and RH7004-004 calls for it: the block holds exactly the following statements, and a directive in front of `}` belongs to no statement.

## Scenarios

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| #9 fixture h | LF | code fix | reproduced | one action registered vs no fix |
| #10 fixtures a, c | LF | code fix | holds | no fix (as specified) |
| #11 g1, g3, g4, g7, g8 | LF | code fix | holds | g1, g3, g7, g8 withheld; g4 offered with the specified output |

## Not covered, and why

- CRLF variants: none of the guards reads line structure. They read member kinds, initializers, the type symbol's layout, and directive kinds and their arguments. Every withheld test asserts that no action is registered, which no line ending changes. The g4 output has the same layout as the existing conditional-pair output, whose CRLF behavior other tests already cover.
- RH7309 with excluded code in a moved block's trivia: RH7309 permutes whole blocks through `RegionBlockLayout.ChangesMemberOrder`, the rank overload of the same owner. The audit found it holds, and decision 12 does not touch it.
- RH7102 (audit hint, fixtures j/k): outside the run, and listed as a follow-up candidate in `00-lead.md`.
- `#if false` members counted by the owner: an over-withhold the spec as written allows (audit hint); there is no requirement to test.
- Two `disable X` pairing: dropped by decision 10.
- FC walk: unchanged from `11-test-implementer.md`. This round adds both sides of the moved-trivia condition for initializers and for layout (FC-003, FC-016), and pins the pairing conditions the repair introduced (FC-017).

## Tests or specs found wrong

None. The spec author's bf155df examples match these tests: fixture a verbatim (no fix output, so the expression-bodied `Log` does not matter, unlike decision 9's case), fixture h verbatim, and fixture c with the change doc's names.

## Output questions

_None._ Every expected output is a withheld fix that decisions 12 and 13 put under the approved *Build configurations* bullet, or the RH7103-005/RH7109-005 move or `main`'s RH7004-004 output for an unchanged counterpart.

## Architecture findings

_None_ beyond decision 12. The failing RH7103 test is the test that decision 12's structural change must turn green. The RH7109 layout tests make sure the new owner, which derives the layout check from the symbol, keeps RH7109's verdicts.

## Build and tests

- `scripts/format.sh` over the three changed test files: the first run rewrapped one `<c>` element of a summary I wrote. I rewrote the summary, and the second run made no changes.
- `scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH7004Using|FullyQualifiedName~RH7103Static|FullyQualifiedName~RH7109"`: 156 tests, 155 passed, **1 failed**: the RH7103 `NoCodeFixWhenExcludedCodeInFrontOfTheMovedStaticFieldWouldReorderTheInstanceFieldsOfAStruct`, as expected.
- `scripts/test.sh --project architecture`: 115 passed.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH7004Using|FullyQualifiedName~RH7103Static|FullyQualifiedName~RH7109"
scripts/test.sh --project architecture
```

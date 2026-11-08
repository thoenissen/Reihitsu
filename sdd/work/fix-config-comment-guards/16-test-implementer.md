Role: test implementer
Commit: 8b2d63d (tests written on the production code that audit 2 saw at 3b3f696; nothing under `src/` except tests changed since)
Verdict: #1593: tests written, findings #3 and #4 reproduced, #5 pinned; #1595: no new tests; #1616: no new tests

This is the structural change of decision 21. The prompt carried no intended fix, suspected cause, or hoped-for verdict. It only named the work: tests for #3, #4a, #4b, #5 and the class matrix.

Inputs:

- `00-lead.md` (decision 21, worklist).
- `15-auditor.md` (findings #3, #4, #5, the architecture finding) and `09-auditor.md`.
- `05-test-implementer.md` and `10-test-implementer.md`.
- SHARED-ORDER-007 in `sdd/specs/shared/ordering-move-safety.md`.

How the rows were checked:

- Every fixture ran with `scripts/apply-fix.sh` under LF and CRLF, on the head and on a temporary worktree at `origin/main` 57acf0d. The worktree was removed afterwards.
- The expected output of each "keeps the fix" test is main's output for the same input. These test methods ran green against main's production code: 53 of 53 `Fix…` tests in the touched classes passed.
- Every test is a `[DataRow("\n")]`/`[DataRow("\r\n")]` pair, and both rows behave the same.
- `scripts/format.sh` was run on the three files. It only collapsed the doubled blank line at the insertion point.
- `scripts/test.sh --project architecture` passes (115).

## thoenissen/Reihitsu#1595

No new test. RH7102 reaches #3 and #4 only through the shared guard: `rg -n "ChangesMemberOrder\(" src -g '!*Test*'` shows `TypeMemberOrderingCodeFixProviderBase.IsMoveSafe` and `RegionBlockLayout.ChangesMemberOrder`, both calling `OrderingDeclarationUtilities.ChangesMemberOrder`. RH7102 has no move of its own that RH7109 and RH7103 lack.

## thoenissen/Reihitsu#1593

Files:

- `src/Reihitsu.Analyzer.Test/Ordering/RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsAnalyzerTests.cs`: 27 tests, and a private helper `VerifyCodeFixIsOffered`.
- `src/Reihitsu.Analyzer.Test/Ordering/RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzerTests.cs`: 1 test.
- `src/Reihitsu.Analyzer.Test/Formatting/RH7309RegionsShouldFollowCategoryOrderAnalyzerTests.cs`: 2 tests.

`VerifyCodeFixIsOffered` checks three things: the exact diagnostics, that an action is registered, and that its result compiles and is no longer reported. It is used only where the result's layout is the open follow-up of decision 15; see *Not covered*.

The class matrix runs along two axes:

- **Placement:** where the conditional group's branches fall relative to the piece boundaries. The cases are a group that opens in the previous member (inside its initializer or behind its declarator) and closes in front of the declaration's first token (**P→F**); a group that opens there and closes behind the declaration's modifiers (**P→I**); a complete group in front of the first token (**F**); a group from in front of the first token to inside the declaration (**F→I**); and a group inside the declaration (**I**).
- **Branches:** whether every branch writes the modifier (static in every build, so the fix is kept), or some branch does not (withheld).

Where SHARED-ORDER-007 decides a cell, both outcomes are tested. Unless stated otherwise, the crossed member is `_b` and the moved one is `_c`. "Form B" means a static initialized `_c` is moved across `_b`, so the move is withheld if any build makes `_b` static. "Form A" means an instance initialized `_c` is moved across `_b`, so the move is withheld if any build makes `_b` an instance field.

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH7109 `NoCodeFixWhenACrossedFieldIsStaticOnlyInAnExcludedBranchThatOpensInTheInitializerOfThePreviousField` (x9, #3, P→F, form B) | SHARED-ORDER-007, RH7109-006 | `VerifyNoCodeFixIsOffered` | red: one action registered |
| RH7109 `NoCodeFixWhenACrossedFieldIsStaticOnlyInAnExcludedBranchThatOpensBehindThePreviousDeclarator` (x4, #3, P→F) | SHARED-ORDER-007, RH7109-006 | `VerifyNoCodeFixIsOffered` | red: one action registered |
| RH7109 `FixMovesAFieldAcrossAFieldThatNoBranchOpeningBehindThePreviousDeclaratorMakesStatic` (x4 counterpart: the excluded branch writes `private` only) | SHARED-ORDER-007, RH7109-006 | `Verify(source, fixed)` | green |
| RH7109 `NoCodeFixWhenACrossedFieldIsStaticOnlyInAnExcludedElseBranchThatOpensBehindThePreviousDeclarator` (x8 in the `;` shape: `#if !DEBUG ; #else ; private static #endif`) | SHARED-ORDER-007, RH7109-006 | `VerifyNoCodeFixIsOffered` | green (main fixes it) |
| RH7109 `FixMovesAFieldAcrossAFieldThatNoElseBranchOpeningBehindThePreviousDeclaratorMakesStatic` (x8 counterpart) | SHARED-ORDER-007, RH7109-006 | `Verify(source, fixed)` | green |
| RH7109 `NoCodeFixWhenACrossedFieldIsStaticOnlyInTheSecondExcludedBranchOfAGroupThatOpensBehindThePreviousDeclarator` (P→F, `#if DEBUG ; private #elif TRACE ; private static #else ; #endif`) | SHARED-ORDER-007, RH7109-006 | `VerifyNoCodeFixIsOffered` | red: one action registered |
| RH7109 `NoCodeFixWhenACrossedFieldIsStaticOnlyInAnExcludedBranchThatSpansThePreviousDeclaratorAndItsModifiers` (P→I: `#if DEBUG ; private static #else ; private #endif int _b = …`) | SHARED-ORDER-007, RH7109-006 | `VerifyNoCodeFixIsOffered` | red: one action registered |
| RH7109 `NoCodeFixWhenACrossedFieldIsStaticOnlyInAnExcludedBranchThatSpansThePreviousInitializerAndItsModifiers` (P→I, the group opens inside `_a = …`) | SHARED-ORDER-007, RH7109-006 | `VerifyNoCodeFixIsOffered` | red: one action registered |
| RH7109 `FixMovesAFieldAcrossAFieldThatNoBranchSpanningThePreviousDeclaratorMakesStatic` (P→I counterpart: `internal` / `private`) | SHARED-ORDER-007, RH7109-006 | `Verify(source, fixed)` | green |
| RH7109 `FixMovesAnInstanceFieldAcrossAFieldThatEveryBranchSpanningThePreviousDeclaratorMakesStatic` (P→I, form A, `internal static` / `private static`) | SHARED-ORDER-007, RH7109-006 | `Verify(source, fixed)` | red: no fix (main fixes it) |
| RH7109 `NoCodeFixWhenACrossedFieldIsAnInstanceFieldOnlyInAnExcludedBranchThatSpansThePreviousDeclaratorAndItsModifiers` (P→I, form A, the excluded branch lacks `static`) | SHARED-ORDER-007, RH7109-006 | `VerifyNoCodeFixIsOffered` | green |
| RH7109 `FixMovesAnInstanceFieldAcrossAFieldThatIsStaticBehindAGroupThatOpensBehindThePreviousDeclarator` (P→F, form A: the excluded branch writes `private` only, with `static` behind `#endif` in every build) | SHARED-ORDER-007, RH7109-006 | `Verify(source, fixed)` | green |
| RH7109 `FixMovesAnInstanceFieldAcrossAStaticFieldWithAnExcludedGroupInFrontOfItsFirstToken` (F, form A: `#if DEBUG volatile #endif private static int _b`) | SHARED-ORDER-007, RH7109-006 | `Verify(source, fixed)` | green |
| RH7109 `FixMovesAnInstanceFieldAcrossAFieldThatIsStaticInEveryBranchWithAReadonlyModifierOnlyInOne` (x1, #4a, F→I) | SHARED-ORDER-007, RH7109-006 | `Verify(source, fixed)` | red: no fix |
| RH7109 `FixMovesAnInstanceFieldAcrossAFieldThatIsStaticInEveryBranchWithADifferentAccessibility` (x1b, #4a, F→I) | SHARED-ORDER-007, RH7109-006 | `Verify(source, fixed)` | red: no fix |
| RH7109 `NoCodeFixWhenACrossedFieldLacksStaticOnlyInAnExcludedBranchInFrontOfItsFirstToken` (F→I, form A, `internal` / `private static`) | SHARED-ORDER-007, RH7109-006 | `VerifyNoCodeFixIsOffered` | green (main fixes it) |
| RH7109 `FixMovesAnInstanceFieldAcrossADeclarationThatIsAConstantInEveryBranch` (x3, #4a, F→I, `internal const` / `private const`) | SHARED-ORDER-007, RH7109-006 | `Verify(source, fixed)` | red: no fix |
| RH7109 `NoCodeFixWhenACrossedDeclarationIsAnInstanceFieldInsteadOfAConstantOnlyInAnExcludedBranch` (F→I, `internal readonly` / `private const`) | SHARED-ORDER-007, RH7109-006 | `VerifyNoCodeFixIsOffered` | green (main fixes it) |
| RH7109 `FixMovesAnInstanceFieldAcrossAFieldThatIsStaticInEveryBranchBehindItsFirstModifier` (I, #4a: `private #if DEBUG static readonly #else static #endif`) | SHARED-ORDER-007, RH7109-006 | `Verify(source, fixed)` | red: no fix |
| RH7109 `NoCodeFixWhenACrossedFieldLacksStaticOnlyInAnExcludedBranchBehindItsFirstModifier` (I, `readonly` / `static`) | SHARED-ORDER-007, RH7109-006 | `VerifyNoCodeFixIsOffered` | green (main fixes it) |
| RH7109 `NoCodeFixWhenACrossedFieldIsStaticOnlyInAnExcludedInnerGroupBehindItsFirstModifier` (I, `#if !DEBUG #if TRACE static #endif #endif`, form B) | SHARED-ORDER-007, RH7109-006 | `VerifyNoCodeFixIsOffered` | green (main fixes it) |
| RH7109 `FixMovesAnInstanceFieldAcrossAFieldThatIsStaticInEveryBranchOfAnElifGroup` (F→I, `#if`/`#elif`/`#else` all `static`) | SHARED-ORDER-007, RH7109-006 | `Verify(source, fixed)` | red: no fix |
| RH7109 `NoCodeFixWhenACrossedFieldLacksStaticOnlyInAnExcludedElifBranch` (F→I, only `#elif` lacks `static`) | SHARED-ORDER-007, RH7109-006 | `VerifyNoCodeFixIsOffered` | green (main fixes it) |
| RH7109 `FixMovesAFieldThatIsStaticInEveryBranchAcrossAnInstanceInitializedField` (moved member, F→I, `internal static readonly` / `private static readonly`, across `private int _s = Log("s")`) | SHARED-ORDER-007, RH7109-006 | `Verify(source, fixed)` | red: no fix |
| RH7109 `NoCodeFixWhenTheMovedFieldLacksStaticOnlyInAnExcludedBranch` (moved member, F→I, `internal readonly` / `private static readonly`) | SHARED-ORDER-007, RH7109-006 | `VerifyNoCodeFixIsOffered` | green (main fixes it) |
| RH7109 `FixIsOfferedWhenAnExcludedInitializerOfACrossedFieldIsAStaticLambda` (h4, #4b) | SHARED-ORDER-007, RH7109-006 | `VerifyCodeFixIsOffered` | red: "A code fix must be offered for this input." |
| RH7109 `FixIsOfferedWhenAnExcludedArgumentOfACrossedFieldsInitializerIsAStaticLambda` (h5, #5 pin) | SHARED-ORDER-007, RH7109-006 | `VerifyCodeFixIsOffered` | green |
| RH7103 `NoCodeFixWhenACrossedFieldIsStaticOnlyInAnExcludedBranchThatOpensInTheInitializerOfThePreviousField` (x9 on RH7103: static `_c` moved in front of `_a` across the split `_b`) | RH7103-007, SHARED-ORDER-007 | `VerifyNoCodeFixIsOffered` | red: one action registered |
| RH7309 `CodeFixMovesABlockOfAStructAcrossAnInstanceFieldWhenEveryBranchDeclaresItsFieldStatic` (#4a layout-field member: struct `Fields` block with `#if DEBUG internal static #else private static #endif int _b;` moved across the `IDisposable` block's `private int _a`) | RH7309-006, SHARED-ORDER-007 | `Verify(source, fixed)` | red: no fix |
| RH7309 `NoCodeFixWhenAFieldOfTheMovedBlockOfAStructLacksStaticOnlyInAnExcludedBranch` (counterpart: `internal` / `private static`) | RH7309-006, SHARED-ORDER-007 | `VerifyNoCodeFixIsOffered` | green (main fixes it) |

Result now: 15 tests (30 rows) red as listed, for the stated reason. Every other test in the three classes passes (249 rows).

### Scenarios

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| x9, x4 (#3) | LF, CRLF | RH7109 single fix | reproduced | one action, as on main, vs none |
| x9 | LF, CRLF | RH7103 single fix | reproduced | one action vs none |
| P→F `#elif`, P→I (both shapes) | LF, CRLF | RH7109 | reproduced (members #3 did not name) | one action vs none |
| x1, x1b, x3, all-`static` in I / `#elif` / P→I, moved all-`static` (#4a) | LF, CRLF | RH7109 | reproduced | no action vs main's moved output |
| layout-field member (#4a) | LF, CRLF | RH7309 | reproduced | no action vs main's moved output |
| h4 (#4b) | LF, CRLF | RH7109 | reproduced | no action vs an action (main fixes it) |
| h5 (#5) | LF, CRLF | RH7109 | holds | fixed, on main and head |
| every "some branch lacks it" counterpart, and x8 | LF, CRLF | RH7109, RH7309 | holds | no action |

Two of the auditor's rows behave differently here:

- **The layout-field member of #4a is reachable through RH7309.** RH7109 groups only fields of the same context, so a struct whose only non-readonly fields are static gives no diagnostic there.
- **A moved member split across P→F or P→I is withheld on main as well,** for another reason. Its moved leading trivia holds an `#else`/`#endif` whose `#if` stays inside the previous member, so the moved range is unbalanced. It is not tested; see below.

### Not covered, and why

- **A moved member whose group opens in the previous member.** Every such move carries an unbalanced directive and is withheld on main and head alike (fixture `int _s #if DEBUG ; static #else ; #endif readonly int _c`, and the same with `volatile`). The member-order guard cannot decide it.
- **`#if DEBUG internal static readonly #else private const #endif int K = 1;` crossed by an instance initialized field.** SHARED-ORDER-007 does not decide this case clearly. "Counts in both contexts when one configuration declares it `static` and another does not" can be read to cover the `const` build, but a `const` has no initializer to reorder. This is left out rather than locked in, and the spec checker may want to state it.
- **h4 and h5 do not assert the exact text.** Main's output moves the crossed field's continuation line between `#else` and `#endif` (h4 from column 8 to 4, h5 to column 25). That is the open whole-type formatting follow-up of decision 15. `VerifyCodeFixIsOffered` asserts what these rows decide: the action is registered, and its result compiles and is not reported. Reverting the depth check for `static` makes h5 red. Counting a `static` behind `=` makes h4 red.
- **The x9 and P→I fix-kept counterparts are written in the `;` shape (x4n, x8sn, `pisn`, `pisall`) rather than the initializer shape.** Main's output for the initializer shape re-indents the active `2;` continuation line (decision 15).
- **RH7102 consumer.** It uses the same `OrderingDeclarationUtilities.ChangesMemberOrder` owner, per the `rg` under #1595. `RegionBlockLayout` (RH7309) ranks positions differently, so it got its own rows.
- **Formatter counterpart.** The formatter does not reorder members (P11).
- **Failure classes:**
  - FC-003: each condition is tested on both sides, per placement.
  - FC-033/035: LF and CRLF rows, and expected outputs taken from main.
  - FC-030: the continuation shapes are avoided or asserted without layout, as above.
  - FC-004/005/019/021: the guard handles no comments.
  - FC-007/008: no new action path; Fix All runs inside `Verify`.
  - FC-018/022–029: no rename, literal, or formatter phase is involved.

## thoenissen/Reihitsu#1616

No new test. Audit 2 has no finding for it.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsAnalyzerTests|FullyQualifiedName~RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzerTests|FullyQualifiedName~RH7309RegionsShouldFollowCategoryOrderAnalyzerTests"
scripts/test.sh --project architecture
```

Role: test implementer
Commit: 8e015be7 (base; this handover's commit adds only the test changes below)
Verdict: tests written

The prompt carried no intended fix, suspected cause, or hoped-for verdict. It named the work: decisions 38, 39, 40, 42 and audit 5's #14 counterexample and #15 fixtures M1–M3. Expectations come from those decisions, from decision 43, and from the spec author's commit 4d647f09 (SHARED-ORDER-007 backing-field paragraph, SHARED-ORDER-008 *No backing field from a branch* and the type-declaration paragraph, RH2001-009's conditional-group paragraph and example). That commit landed while I was writing; I re-read it and aligned every tag with it.

Every new test has an LF and a CRLF row, and both rows behave alike. `scripts/format.sh` changed nothing in the four files. The architecture tests pass (115 of 115).

Result on the head (8e015be7 production code): the RH7102, RH7103, RH7109, RH7309 and RH2001 classes give 20 failing rows out of 555. These are the 10 tests marked **red** below. Each one fails with "Expected collection to be empty … No code fix may be offered", which means the head registers the fix. None fails on a compile error or a diagnostic mismatch.

## thoenissen/Reihitsu#1595

No test added or changed. The type-declaration refusal (decision 39) runs through the shared guard and is covered under #1593. RH7102's fix reaches it through the same `IsMoveSafe` path as RH7109.

## thoenissen/Reihitsu#1593

### Tests: re-pointed by decision 38 (outcome unchanged, reason and tags changed)

Decision 38 moves the conditional bodiless-accessor case from SHARED-ORDER-007's count to a SHARED-ORDER-008 refusal ("like `field`"). Decision 43 (2A) narrows that refusal to a list that also holds an accessor with a body. The SHARED-ORDER-007 example these tests pinned is gone from the spec; 4d647f09 replaced it with the plain `get => 0; set;` example.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH7309…NoCodeFixWhenAPropertyOfAStructHasAnAutoAccessorOnlyInOneBranchOfItsAccessorList` (acc1; was RH7309-006/SHARED-ORDER-007) | RH7309-005, SHARED-ORDER-008 | `VerifyNoCodeFixIsOffered` | green |
| `RH7309…CodeFixMovesABlockOfAStructAcrossAPropertyWhoseAccessorListBranchesAreAllBodied` (acc1 counterpart: no branch holds a bodiless accessor) | RH7309-005, SHARED-ORDER-008 | `Verify` | green |
| `RH7309…NoCodeFixWhenAConditionalAccessorWithoutABodyGivesAPropertyOfAStructABackingField` (former SHARED-ORDER-007 example; same shape as acc1) | RH7309-005, SHARED-ORDER-008 | `VerifyNoCodeFixIsOffered` | green |
| `RH7309…CodeFixMovesABlockOfAStructAcrossAPropertyWhoseConditionalAccessorHasABody` (its `set { }` counterpart) | RH7309-005, SHARED-ORDER-008 | `Verify` | green |

### Tests: new

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH7309…NoCodeFixWhenAPropertyOfAStructHasAnAccessorWithoutABodyNextToAnAccessorWithABody` (#14 plain `get => 0; set;`, the new SHARED-ORDER-007 example) | RH7309-006, SHARED-ORDER-007 | `VerifyNoCodeFixIsOffered` | **red**: the head and `main` fix it, because `HasBackingField` requires every accessor to be bodiless |
| `RH7309…CodeFixMovesABlockOfAStructAcrossAPropertyWhoseAccessorsAllHaveABody` (plain `get => 0; set { }`, the example's counterpart) | RH7309-006, SHARED-ORDER-007 | `Verify`, `main`'s output | green |
| `RH7309…CodeFixMovesABlockOfAClassAcrossAPropertyWithAnAccessorWithoutABodyNextToAnAccessorWithABody` (plain mixed shape in a class: no layout, so the adjacent fix must not withhold) | RH7309-006, SHARED-ORDER-007 | `Verify`, `main`'s output | green |
| `RH7309…NoCodeFixWhenAnActiveConditionalAccessorWithoutABodyStandsNextToAnAccessorWithABody` (M4: `get => 0; #if !DEBUG set; #endif`) | RH7309-005, SHARED-ORDER-008 | `VerifyNoCodeFixIsOffered` | green. It must stay green once `HasConditionalAccessorWithoutBody` is deleted |
| `RH7309…NoCodeFixWhenAnExcludedConditionalAccessorWithoutABodyStandsNextToAnAccessorWithABody` (`get => 0; #if DEBUG set; #endif`) | RH7309-005, SHARED-ORDER-008 | `VerifyNoCodeFixIsOffered` | green, same reason |
| `RH7309…NoCodeFixWhenAGroupOfWholeAccessorsOfAStructPropertyUsesTheFieldKeyword` (RH7309 variant of M3) | RH7309-005, SHARED-ORDER-008 | `VerifyNoCodeFixIsOffered` | green (pins the accessor-list arm of the `field` check) |
| `RH7103…NoCodeFixWhenTheActiveBranchOfAGroupInACrossedAccessorBodyUsesTheFieldKeyword` (M2, `#if !TRACE return field;`) | RH7103-007, SHARED-ORDER-008 | `VerifyNoCodeFixIsOffered` | green (pins the active-branch `field` check) |
| `RH7103…NoCodeFixWhenAGroupOfWholeAccessorsOfACrossedPropertyUsesTheFieldKeyword` (M3) | RH7103-007, SHARED-ORDER-008 | `VerifyNoCodeFixIsOffered` | green |
| `RH7103…FixMovesAMemberAcrossAPropertyOfAStructWhoseGroupOfWholeAccessorsDoesNotUseTheFieldKeyword` (M3's `set { }` counterpart) | RH7103-007, SHARED-ORDER-008 | `Verify` | green |
| `RH7103…NoCodeFixWhenAGroupOfWholeAccessorsOfACrossedPropertyOfAClassHoldsAnAccessorWithoutABody` (`get => 0; #if DEBUG set; #endif` in a **class**, crossed by a static member) | RH7103-007, SHARED-ORDER-008 | `VerifyNoCodeFixIsOffered` | **red**: the head fixes it (a class has no layout, so the head's backing-field count does not matter there) |
| `RH7109…NoCodeFixWhenAGroupInTheCrossedSpanIsNestedInAGroupOutsideBothSpans` (M1) | SHARED-ORDER-008, RH7109-006 | `VerifyNoCodeFixIsOffered` | green (pins the enclosing-group count) |
| `RH7109…FixMovesAFieldInsideAGroupOutsideBothSpansWithNoGroupInTheSpans` (M1 without the inner group) | SHARED-ORDER-008, RH7109-006 | `Verify`, `main`'s output | green |
| `RH7109…NoCodeFixWhenAConditionalGroupWrapsTheLayoutAttributeOfTheType` (decision 39: `#if X [StructLayout(Sequential)] #endif class`) | SHARED-ORDER-008, RH7109-006 | `VerifyNoCodeFixIsOffered` | **red** |
| `RH7109…NoCodeFixWhenAConditionalGroupWrapsAnAttributeListOfTheType` (`#if X [System.Obsolete] #endif`: the refusal depends on the directive's place, not on the attribute) | SHARED-ORDER-008, RH7109-006 | `VerifyNoCodeFixIsOffered` | **red** |
| `RH7109…NoCodeFixWhenAConditionalGroupDecidesTheKindOfTheType` (`public #if X struct #else class #endif C`) | SHARED-ORDER-008, RH7109-006 | `VerifyNoCodeFixIsOffered` | **red** |
| `RH7109…NoCodeFixWhenAConditionalGroupStandsInTheBaseListOfTheType` (group in the opening brace's leading trivia) | SHARED-ORDER-008, RH7109-006 | `VerifyNoCodeFixIsOffered` | **red** |
| `RH7109…FixMovesAFieldOfATypeWithAnAttributeListOutsideAnyGroup` (unconditional `[System.Obsolete]`) | SHARED-ORDER-008, RH7109-006 | `Verify`, `main`'s output | green |
| `RH7109…FixMovesAFieldOfATypeBehindAGroupOfAWholeTypeInFrontOfIt` (decision 43 (1A): a whole-type group in the first token's leading trivia keeps the move) | SHARED-ORDER-008, RH7109-006 | `Verify`, `main`'s output | green |
| `RH7309…NoCodeFixWhenAConditionalDirectiveStandsInTheAttributeListsOfTheType` (decision 39 through `RegionBlockLayout`: conditional `[StructLayout(Sequential)]` on a class with an auto-property) | RH7309-005, SHARED-ORDER-008 | `VerifyNoCodeFixIsOffered` | **red** |

The M1–M3 fixtures are the auditor's inputs from audit 5, #15. The auditor showed each relaxation live; these tests are green now and turn red under the matching mutant. I did not run the mutants again.

### For the lead

The class test (`…OfAClassHoldsAnAccessorWithoutABody`) withholds a fix that `main` offers, in a type whose layout does not change. That follows the committed SHARED-ORDER-008 clause and the change doc ("refuses the move like `field`", with no type condition), and decision 43 (2A) narrows only by "an accessor of the list has a body". Decision 38's column "Withholds only where a build's layout changes" reads narrower. If the lead means the narrower reading, this test and the spec clause go together.

## thoenissen/Reihitsu#1616

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH2001…VerifyNoCodeFixIsOfferedForAConditionalExternBetweenTheModifiersAndTheType` (RH2001-009 example `First`) | RH2001-009 | `VerifyNoCodeFixIsOffered` | **red** |
| `RH2001…VerifyNoCodeFixIsOfferedForAConditionalReadonlyBetweenTheModifiersAndTheType` (example `Third`, struct) | RH2001-009 | `VerifyNoCodeFixIsOffered` | **red** |
| `RH2001…VerifyNoCodeFixIsOfferedForAConditionalUnsafeBetweenTheModifiersAndTheType` (example `Second`) | RH2001-009 | `VerifyNoCodeFixIsOffered` | **red** |
| `RH2001…VerifyFixIsOfferedForAConditionalNewBetweenTheModifiersAndTheType` (`new` is in the allowed set; `readonly` goes behind the group) | RH2001-009 | `Verify` (Fix All, iterations) | green |
| `RH2001…VerifyFixIsOfferedForAConditionalModifierBetweenTheModifiersAndTheType` (decision 36's `static` shape, unchanged) | RH2001-009, SHARED-FIX-009 | `Verify` | green |

## Scenarios (bug fix)

_Not applicable_: this round pins decisions and spec examples of a behavior change.

## Not covered, and why

- **Decision 39 through RH7102 and RH7103.** They reach the type-declaration check through the same `TypeMemberOrderingCodeFixProviderBase.IsMoveSafe` → `OrderingDeclarationUtilities.ChangesMemberOrder` call as RH7109 (`rg -n "ChangesMemberOrder\(" src -g '!*Test*'`). RH7309 is tested separately because it enters through `RegionBlockLayout.ChangesMemberOrder`.
- **Constraint clauses, a type parameter list, or a directive between the type's modifiers.** These are listed in the same bullet of the type-declaration paragraph. The keyword and base-list tests cover "a token after the first one, up to the opening brace"; the attribute tests cover the first-token bullet.
- **RH2001 groups holding an accessibility keyword.** These are in the allowed set. A group that holds one in front of the first modifier lies outside RH2001-009 (decision 44 dropped that candidate).
- **Formatter parity, FC-010 to FC-014.** No formatter code is involved.
- **FC-007/FC-008 (Fix All).** The new withholdings come from a property's or a type's shape and cannot co-occur with a fix in a way the existing Fix All scenarios do not already show. Every `Verify` test runs Fix All on its input.
- **FC-033.** Every new test has LF and CRLF rows.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH7109ReadonlyElements|FullyQualifiedName~RH7103StaticElements|FullyQualifiedName~RH7102Constants|FullyQualifiedName~RH7309Regions|FullyQualifiedName~RH2001PrivateAuto"
scripts/test.sh --project architecture
```

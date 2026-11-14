Role: test implementer (correction round, lead decisions 15, 16, 17)
Commit: e780d7d (tests written on top of it)
Verdict: tests written

The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the lead's decisions. Expected values come from the RH7102-005, RH7109-005, and RH7311-001 examples as `e780d7d` states them, and from unchanged SHARED-ORDER-004 / RH7102-006 / RH7109-006.

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH7102ConstantsMustAppearBeforeFieldsAnalyzerTests.CodeFixTargetsTheFirstFieldOfTheGroupInTheConstantsOwnRegion` (rewritten: `public int B;` first in region `B`) | RH7102-005 | `Verify(source, fixedSource)` | Pass |
| `RH7102ConstantsMustAppearBeforeFieldsAnalyzerTests.NoCodeFixWhenTheFirstFieldOfTheGroupInTheOwnRegionIsTheFirstMemberOfThatRegion` (new; the old example, `_b` first in region `B`) | RH7102-006, SHARED-ORDER-004 | `VerifyNoCodeFixIsOffered` | Pass |
| `RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsAnalyzerTests.CodeFixTargetsTheFirstFieldOfTheGroupInTheReadonlyFieldsOwnRegion` (rewritten the same way) | RH7109-005 | `Verify(source, fixedSource)` | Pass |
| `RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsAnalyzerTests.NoCodeFixWhenTheFirstFieldOfTheGroupInTheOwnRegionIsTheFirstMemberOfThatRegion` (new) | RH7109-006, SHARED-ORDER-004 | `VerifyNoCodeFixIsOffered` | Pass |
| `RH7311RegionDescriptionsMustBeUniqueWithinATypeAnalyzerTests.RegionsOfADeclarationThatAConditionalDirectiveEnclosesAreCompared` (new; the RH7311-001 spec example, `#if !NEVER` around `class C`) | RH7311-001 | `Verify(source)` | Pass |
| `RH7311RegionDescriptionsMustBeUniqueWithinATypeAnalyzerTests.RegionsInABranchThatStartsInsideTheDeclarationAreNotCompared` (new; active `#if !NEVER` inside the type holding both `#region Methods`) | RH7311-001 | `Verify(source)` | Pass |
| `RH7311RegionDescriptionsMustBeUniqueWithinATypeAnalyzerTests.RegionInABranchInsideADeclarationThatAConditionalDirectiveEnclosesIsNotCompared` (new; whole-type `#if !NEVER` plus an inner `#if !NEVER` holding the second region) | RH7311-001 | `Verify(source)` | Pass |
| `Reihitsu.Core.Test.RegionDirectiveUtilitiesTests.IsWithinRegionRecognizesAMemberInsideATopLevelRegion` (was `RegionContainmentHelpersFindContainingTopLevelRegion`; the `TryFindContainingRegion` call and its two assertions removed) | — | direct call | Pass |

The three RH7311 tests are the two sides of the boundary decision 16 draws: a conditional directive that encloses the whole declaration (regions compared) next to one whose `#if` starts inside the declaration (regions not compared), and both nested, where the walk must stop at the type's start and still count the inner `#if`. All three use `!NEVER`, so the branches are active and the outcome does not hinge on the test project's preprocessor symbols.

## Decision 17 — Core test

`RegionContainmentHelpersFindContainingTopLevelRegion` was the only Core test calling `TryFindContainingRegion`, but it also asserted `RegionDirectiveUtilities.IsWithinRegion`, which still has production callers (`TypesOrganizedWithRegionsAnalyzerBase` → RH7305, RH7305A) and no other direct Core test (`rg -n "IsWithinRegion" src --glob '*.cs'`). So the test was not deleted: it keeps its `IsWithinRegion` assertion under a new name, and only the `TryFindContainingRegion` part was removed. After this commit `rg -n "TryFindContainingRegion\b" src --glob '*.cs'` hits only the declaration in `src/Reihitsu.Core/RegionDirectiveUtilities.cs`, so the implementer can remove the helper.

## Scenarios (bug fix)

_None new in this round._ All new and rewritten tests pass on this head; the two #1339 tests that failed in `05-implementer.md` now pass with the rewritten examples.

## Not covered, and why

- CRLF variants: the changes are analyzer reporting and an unchanged move refusal; none of the new tests involves layout produced by the change beyond the existing fix path, which the classes already cover under both line endings.
- Fix All for the rewritten #1339 shape: `GetFixAllScenario` of both classes already covers several diagnostics in one document; the region-scoped target adds no co-occurrence the scenario does not exercise.
- RH7311 with an `#if` between the type header and its `{`: rare input (a type's braces split across a branch), and `06-spec-author.md` names it as the only difference between the two wordings; left untested.

## Commands

```shell
scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH7311|FullyQualifiedName~RH7102ConstantsMustAppearBeforeFieldsAnalyzerTests|FullyQualifiedName~RH7109ReadonlyElements"
scripts/test.sh --configuration Debug --project core --filter "FullyQualifiedName~RegionDirectiveUtilitiesTests"
```

Results: analyzer 419/419 passed; core 11/11 passed.

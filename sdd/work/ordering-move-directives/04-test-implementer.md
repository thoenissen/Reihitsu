Role: test implementer
Commit: 1b3f74b (tests; on top of e4d7458)
Verdict: #970: tests written; #1668: tests written

The prompt carried no intended fix, suspected cause, or hoped-for verdict to ignore.

Every new or changed test was run on e4d7458 (production code = `main`). A test that pins behavior the accepted change docs keep passes there. A test that pins the new behavior fails there for its own reason: a fix test fails with "Iterative code fix application … did not match" with the unchanged input as the actual result (no fix offered), and a no-fix test fails with "No code fix may be offered for the diagnostic at …" (fix offered). The full list of red tests is in *Whole run*.

## thoenissen/Reihitsu#970

### Tests

New class `tests/Reihitsu.Analyzer.Test/Ordering/RH7103StaticElementsMustAppearBeforeInstanceElementsWarningPairTests.cs` (abbreviated `WP` below; `BatchCodeFixTestsBase`, its Fix All scenario moves two static members across a pair in a crossed body, one with a pair in its own body).

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| WP `FixMovesAMemberAcrossAWarningPairInsideACrossedMethodBody` (LF, CRLF) | RH7103-007, SHARED-ORDER-011 (spec example) | `VerifyCodeFixWithEndOfLine` | red |
| WP `FixMovesAMemberWhoseBodyHoldsAWarningPair` | SHARED-ORDER-011 | `Verify` | red |
| WP `NoCodeFixWhenACrossedMethodBodyHoldsALoneWarningDisable` | RH7103-007, SHARED-ORDER-011 (spec example) | `VerifyNoCodeFixIsOffered` | green |
| WP `NoCodeFixWhenTheRestoreOfAPairInACrossedBodyNamesOtherIds` (other ID — spec's CS0219 —, other order, `168` against `CS0168`, fewer IDs, IDs only on the disable) | SHARED-ORDER-011 | `VerifyNoCodeFixIsOffered` | green |
| WP `FixMovesAMemberAcrossAWarningPairWithoutIds` | SHARED-ORDER-011 | `Verify` | red |
| WP `FixMovesAMemberAcrossAWarningPairWithSeveralIds` | SHARED-ORDER-011 | `Verify` | red |
| WP `FixMovesAMemberAcrossAWarningPairWithATrailingCommentOnBothDirectives` | SHARED-ORDER-011 | `Verify` | red |
| WP `NoCodeFixWhenACrossedBodyHoldsNestedWarningPairs` | SHARED-ORDER-011 | `VerifyNoCodeFixIsOffered` | green |
| WP `FixMovesAMemberAcrossTwoConsecutiveWarningPairsInACrossedBody` | SHARED-ORDER-011 | `Verify` | red |
| WP `FixMovesAMemberAcrossANullableDisableClosedByRestoreInACrossedBody` (no target, `warnings`, `annotations`) | SHARED-ORDER-011 | `Verify` | red |
| WP `FixMovesAMemberAcrossANullableEnableClosedByRestoreInACrossedBody` | SHARED-ORDER-011 | `Verify` | red |
| WP `NoCodeFixWhenANullableDirectiveInACrossedBodyIsNotClosedByItsRestore` (disable/enable, enable/disable, `disable warnings`/`restore`, `disable`/`restore annotations`) | SHARED-ORDER-011 | `VerifyNoCodeFixIsOffered` | green |
| WP `FixMovesAMemberAcrossAWarningPairThatEnclosesANullablePair` | SHARED-ORDER-011 | `Verify` | red |
| WP `NoCodeFixWhenTheDisableOfAPairStandsInFrontOfTheFirstTokenOfACrossedMethod` | SHARED-ORDER-005, SHARED-ORDER-011 (FC-003, refusing side) | `VerifyNoCodeFixIsOffered` | green |
| WP `FixMovesAMemberAcrossAPairWhoseDisableStandsBehindTheAttributeListOfACrossedMethod` | SHARED-ORDER-011 (FC-003, keeping side) | `Verify` | red |
| WP `FixMovesAMemberAcrossAPairBetweenTheMembersOfACrossedNestedType` | SHARED-ORDER-011 | `Verify` | red |
| WP `NoCodeFixWhenAnUnclosedWarningDirectiveStandsInFrontOfTheSpans` (CS1591, CS0168, in an excluded branch) | RH7103-007, SHARED-ORDER-011 (spec example) | `VerifyNoCodeFixIsOffered` | green |
| WP `UnclosedNullableDirectiveInFrontOfTheSpansWithholdsOnlyTheMoveAcrossANullablePair` | SHARED-ORDER-011 (FC-003 on "of the pair's kind") | `VerifyNoCodeFixIsOffered` + `Verify` | red (second half) |
| WP `FixMovesAMemberAcrossAWarningPairBehindAClosedPairInFrontOfTheSpans` | RH7103-007, SHARED-ORDER-011 (spec example, 970-3 B) | `Verify` | red |
| WP `FixMovesAMemberAcrossAWarningPairBehindAClosedPairAroundAnEarlierType` | SHARED-ORDER-011 | `Verify` | red |
| WP `FixMovesAMemberBehindAClosedWarningPairInFrontOfTheSpans` | SHARED-ORDER-011 | `Verify` | green |
| WP `NoCodeFixWhenAPairInFrontOfTheSpansIsSplitAcrossAConditionalBranch` | SHARED-ORDER-011 (*One branch* in front) | `VerifyNoCodeFixIsOffered` | green |
| WP `FixMovesAMemberBehindAPairInFrontOfTheSpansThatEnclosesAWholeConditionalGroup` | SHARED-ORDER-011 | `Verify` | red |
| WP `NoCodeFixWhenACrossedMethodBodyHoldsAnotherPositionSensitiveDirective` (`#line default`, `#warning`) | SHARED-ORDER-011 | `VerifyNoCodeFixIsOffered` | green |
| WP `NoCodeFixWhenACrossedMethodBodyHoldsAPragmaChecksum` | SHARED-ORDER-011 | `VerifyNoCodeFixIsOffered` | green |
| WP `FixAllCorrectsEveryDiagnostic` | RH7103-008, SHARED-ORDER-011 (FC-008) | Fix All scenario | red |
| RH7107 `WarningPairInTheCrossedAccessorBodyKeepsTheFix` (LF, CRLF) — replaces `BalancedPragmaPairInTheCrossedAccessorBodyWithholdsTheFix` | RH7107-005, SHARED-ORDER-011 | `VerifyCodeFixWithEndOfLine` | red |
| RH7107 `NoCodeFixWhenAWarningPairInTheCrossedAccessorBodyIsSplitAcrossAConditionalBranch` | RH7107-005, SHARED-ORDER-011 (spec example) | `VerifyNoCodeFixIsOffered` | green |
| RH7107 `WarningPairAroundAWholeConditionalGroupInTheCrossedAccessorBodyKeepsTheFix` | RH7107-005, SHARED-ORDER-011 (spec example) | `Verify` | red |
| RH7107 `WarningPairInTheCrossedAccessorBodyBehindAClosedPairInAMethodAboveKeepsTheFix` | SHARED-ORDER-011 | `Verify` | red |
| RH7107 `NoCodeFixWhenAWarningDirectiveInAMethodAboveThePropertyIsNotClosed` | SHARED-ORDER-011 | `VerifyNoCodeFixIsOffered` | green |
| RH7108 `WarningPairInTheCrossedRemoveAccessorBodyKeepsTheFix` | RH7108-005, SHARED-ORDER-011 | `Verify` | red |
| RH7108 `NoCodeFixWhenAWarningPairInTheCrossedRemoveAccessorBodyIsSplitAcrossAConditionalBranch` | RH7108-005, SHARED-ORDER-011 | `VerifyNoCodeFixIsOffered` | green |
| RH7309 Trivia `WarningPairInsideAMemberOfAMovedBlockDoesNotWithholdTheFix` | RH7309-005, SHARED-ORDER-011 (spec example) | `Verify` | red |
| RH7309 Trivia `WarningPairInsideAMemberOfABlockThatKeepsItsPlaceDoesNotWithholdTheFix` (LF, CRLF) | RH7309-005, SHARED-ORDER-011 (spec example, 970-4 B) | `VerifyCodeFixWithEndOfLine` | red |
| RH7309 Trivia `WarningPairInsideAMemberBetweenTheBlocksDoesNotWithholdTheFix` | RH7309-005, SHARED-ORDER-011 | `Verify` | red |
| RH7309 Trivia `NullablePairInsideAMemberOfAMovedBlockDoesNotWithholdTheFix` | SHARED-ORDER-011 | `Verify` | red |
| RH7309 Trivia `NoCodeFixWhenAMemberOfABlockHoldsALoneWarningDisable` | RH7309-005, SHARED-ORDER-011 | `VerifyNoCodeFixIsOffered` | green |
| RH7309 Trivia `NoCodeFixWhenAnUnclosedWarningDirectiveStandsInFrontOfTheFirstBlock` | RH7309-005, SHARED-ORDER-011 | `VerifyNoCodeFixIsOffered` | green |
| RH7309 Trivia `ClosedWarningPairInFrontOfTheFirstBlockDoesNotWithholdTheFix` | RH7309-005, SHARED-ORDER-011 | `Verify` | red |
| Core `OrderingDeclarationUtilitiesTests.MoveRangeContainsDirectivesReturnsFalseWhenCrossedMemberBodyContainsClosedWarningPair` — replaces `…ReturnsTrueWhenCrossedMemberBodyContainsDirective` | SHARED-ORDER-011 | unit | red |
| Core `OrderingDeclarationUtilitiesTests.MoveRangeContainsDirectivesReturnsTrueWhenCrossedMemberBodyContainsUnclosedWarningDirective` | SHARED-ORDER-011 | unit | green |
| Core `OrderingDeclarationUtilitiesTests.MoveRangeContainsDirectivesReturnsTrueWhenCrossedMemberBodyContainsLineDirective` | SHARED-ORDER-011 | unit | green |
| Core `AccessorOrderingUtilitiesTests.MoveRangeContainsDirectivesReturnsFalseWhenCrossedAccessorBodyContainsClosedWarningPair` — replaces `…ReturnsTrueWhenCrossedAccessorBodyContainsDirective` | SHARED-ORDER-011 | unit | red |
| Core `AccessorOrderingUtilitiesTests.MoveRangeContainsDirectivesReturnsTrueWhenCrossedAccessorBodyContainsNullableDisableClosedByEnable` | SHARED-ORDER-011 | unit | green |

Existing tests that pin an output this change alters, each changed: the three the skeptic named (two Core tests and RH7107 `BalancedPragmaPairInTheCrossedAccessorBodyWithholdsTheFix`), replaced as listed above. `rg` for `#pragma`/`#nullable` over the ordering and RH7309 test files found no other test that asserts a refusal for a pair SHARED-ORDER-011 now keeps. Left unchanged and green, because their directive is lone or in a declaration's leading trivia: RH7102 and RH7109 `PragmaDirectivesAroundACrossedFieldWithholdTheFix`, RH7103 `NoCodeFixWhenPragmaDirectivePrecedesMovedMember`, RH7107 `PragmaDirectiveInFrontOfTheGetAccessorWithholdsTheFix`, RH7108 `PragmaDirectiveInFrontOfTheAddAccessorWithholdsTheFix`, RH7309 Trivia `VerifyNoCodeFixWhenRegionContainsPragmaWarningDirective`, `VerifyNoCodeFixWhenRegionContainsNullableDirective`, `VerifyNoCodeFixWhenPragmaWarningDirectiveSitsBetweenRegions`, `NoCodeFixWhenAPragmaDirectiveWouldBeMovedAcross`, and `VerifyCodeFixReordersRegionsFollowingAPragmaWarningDirective` (a lone directive in front with none in the spans keeps the fix; green).

### Not covered, and why

- RH7102 and RH7109 get no SHARED-ORDER-011 test of their own. They reach the guard through the same call site as RH7103: `rg -n "MoveRangeContainsDirectives|ChangesMemberOrder" src/Reihitsu.Analyzer.CodeFixes` shows the one call in `TypeMemberOrderingCodeFixProviderBase.IsMoveSafe` (private static); the three providers (`rg -l TypeMemberOrderingCodeFixProviderBase src/Reihitsu.Analyzer.CodeFixes/Rules/Organization`) override only `TryGetTargetMember`. RH7107 and RH7108 call `AccessorOrderingUtilities.MoveRangeContainsDirectives` from their own `IsMoveSafe`, and RH7309 has its own call site; each gets tests.
- A `disable` without IDs closed by `restore CS0168` (skeptic candidate 8, second half) cannot be tested at fix level. The `disable` without IDs stays in effect for every other warning behind it, so RH7103 is suppressed on the moved member and nothing is reported. The `restore` without IDs in the other rows needs `TestBehaviors.SkipSuppressionCheck`, because it also undoes the `#pragma warning disable` the test harness puts in front to check suppression. That is stated in the test.
- `#line hidden` was replaced by `#line default`: `#line hidden` hides the RH7103 diagnostic of the method behind it.
- FC-033: LF and CRLF are run for the spec examples (WP crossed-body pair, RH7107 crossed-body pair, RH7309 kept-block pair). The moves are text moves (SHARED-ORDER-009), so no formatter surface is involved. FC-019/FC-020: comment and directive in the gap are the subject of every test here. FC-004: a pair in the moved member's body travels with it (`FixMovesAMemberWhoseBodyHoldsAWarningPair`). FC-044: no test models pragma state; the tests pin only the positional checks. FC-038–043 (unsupported shapes) do not apply: no shape is declared. FC-018/FC-036 (naming) do not apply.

### Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH7103StaticElementsMustAppearBeforeInstanceElementsWarningPairTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH7107|FullyQualifiedName~RH7108|FullyQualifiedName~RH7309RegionsShouldFollowCategoryOrderTriviaTests"
scripts/test.sh --project core --filter "FullyQualifiedName~OrderingDeclarationUtilitiesTests|FullyQualifiedName~AccessorOrderingUtilitiesTests"
```

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1668

### Spec finding — SHARED-ORDER-008, RH7103 example of "a group in the body in front of the spans"

The spec example's input contradicts the requirement's normative text, and its shown output is not `main`'s output:

```csharp
public class C
{
#if DEBUG
    private int _trace;
#endif
    public void Run()
    {
    }

    public static void {|RH7103:Create|}()
    {
    }
}
```

The group lies in the leading trivia of `Run`, the move's target. SHARED-ORDER-003 starts the crossed span "from the target's first trivia", so by SHARED-ORDER-008's first paragraph the group lies in the crossed span and refuses the move. The example shows the fix offered, with the group left in front of `Create`. On e4d7458 the fix is offered, but it moves the group together with `Run`: the output is `Create`, then the group, then `Run`, not the output the spec shows. The spec author's handover reports this example as checked against `main`. The existing RH7109 tests `NoCodeFixWhenAGroupInTheLeadingTriviaOfTheTargetClosesTheTypeAndOpensAStruct` (no fix) and `NoCodeFixWhenAGroupInFrontOfTheSpansClosesTheTypeAndOpensAStruct` (no fix, for the layout clause) read the boundary the same way. The RH7109 variant of the same example is consistent: there the target is `_a`, so the group stands in front of the spans.

What I wrote: `NoCodeFixWhenAGroupStandsInTheLeadingTriviaOfTheTarget` asserts the normative reading on this exact input, and `FixMovesAStaticMethodBehindAGroupInTheBodyInFrontOfTheSpans` covers the example's intent with `private int _count;` between the group and `Run`, so the group really stands in front of the spans. The lead decides whether the spec author fixes the example's input (as in my second test), or whether the requirement means otherwise and the first test flips.

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH7103 `NoCodeFixWhenACrossedMethodBodyHoldsAGroupOfWholeStatements` (LF, CRLF) — was `CodeFixIsOfferedWhenCrossedMemberBodyContainsBalancedConditional` | RH7103-007, SHARED-ORDER-008 (spec example) | `VerifyNoCodeFixIsOffered` | red |
| RH7103 `NoCodeFixWhenACrossedMethodBodyHoldsAnActiveGroupOfWholeStatements` (`DEBUG` defined) — was `…ActiveBalancedConditional` | RH7103-007, SHARED-ORDER-008 | `VerifyNoCodeFixIsOffered` | red |
| RH7103 `NoCodeFixWhenAGroupOfWholeMembersStandsInTheMovedMembersLeadingTrivia` (LF, CRLF) — was `FixMovesAMemberWithAGroupOfWholeMembersInItsLeadingTrivia` | RH7103-007, SHARED-ORDER-008 (spec example) | `VerifyNoCodeFixIsOffered` | red |
| RH7103 `NoCodeFixWhenAnIfFalseGroupOfWholeMembersStandsInTheMovedMembersLeadingTrivia` — was `CodeFixTravelsWithABalancedDisabledDirectiveBlockAboveTheMovedMember` | RH7103-007, SHARED-ORDER-008 (spec: `#if false`; FC-020) | `VerifyNoCodeFixIsOffered` | red |
| RH7103 `NoCodeFixWhenAGroupOfWholeAttributeListsStandsOnTheMovedMethod` (LF, CRLF) | RH7103-007, SHARED-ORDER-008 (spec: attribute lists) | `VerifyNoCodeFixIsOffered` | red |
| RH7103 `NoCodeFixWhenAGroupOfWholeAccessorsStandsInTheMovedProperty` (LF, CRLF) | RH7103-007, SHARED-ORDER-008 | `VerifyNoCodeFixIsOffered` | red |
| RH7103 `NoCodeFixWhenACrossedPropertyHoldsAGroupOfWholeAccessors` (LF, CRLF) | RH7103-007, SHARED-ORDER-008 (spec: accessors) | `VerifyNoCodeFixIsOffered` | red |
| RH7103 `NoCodeFixWhenASwitchSectionOfACrossedMethodHoldsAGroupOfWholeStatements` (LF, CRLF) | RH7103-007, SHARED-ORDER-008 | `VerifyNoCodeFixIsOffered` | red |
| RH7103 `NoCodeFixWhenACrossedNestedTypeHoldsAGroupOfWholeMembers` | RH7103-007, SHARED-ORDER-008 (spec: nested type) | `VerifyNoCodeFixIsOffered` | red |
| RH7103 `NoCodeFixWhenACrossedMethodBodyHoldsAGroupWithAnElifBranch` | SHARED-ORDER-008 | `VerifyNoCodeFixIsOffered` | red |
| RH7103 `FixMovesAMemberInFrontOfAGroupBehindTheSpans` (LF, CRLF) | RH7103-007, SHARED-ORDER-008 (spec example) | `VerifyCodeFixWithEndOfLine` | green |
| RH7103 `FixMovesAStaticMethodBehindAGroupInTheBodyInFrontOfTheSpans` | RH7103-007, SHARED-ORDER-008 | `Verify` | green |
| RH7103 `NoCodeFixWhenAGroupStandsInTheLeadingTriviaOfTheTarget` | RH7103-007, SHARED-ORDER-008 (FC-003; see the spec finding) | `VerifyNoCodeFixIsOffered` | red |
| RH7103 `FixAllMovesOnlyTheStaticMembersWhoseSpansHoldNoConditionalDirective` | RH7103-007, SHARED-ORDER-008 (FC-008) | `Verify` with markup in the fixed state | red |
| RH7103 `NoCodeFixWhenAGroupInFrontOfTheMovedStaticFieldOfAStructLiesInTheMovedSpan` — was `NoCodeFixWhenExcludedCodeInFrontOfTheMovedStaticFieldWouldReorderTheInstanceFieldsOfAStruct`, retagged | RH7103-007, SHARED-ORDER-008 (RH7103-007 example) | `VerifyNoCodeFixIsOffered` | green |
| RH7103 `NoCodeFixWhenTheMoveCrossesAnInitializedStaticFieldOfExcludedCode` — retagged SHARED-ORDER-007 → 008 | RH7103-007, SHARED-ORDER-008 | `VerifyNoCodeFixIsOffered` | green |
| RH7102 `NoCodeFixWhenAGroupOfWholeMembersStandsInFrontOfTheConstant` (LF, CRLF) — was `FixCarriesAnInitializedFieldOfExcludedCodeWithTheConstantAcrossAFieldWithoutInitializer` | RH7102-006, SHARED-ORDER-008 (RH7102-006 example) | `VerifyNoCodeFixIsOffered` | red |
| RH7102 `NoCodeFixWhenTheConstantCrossesAGroupOfWholeMembersInAStruct` (LF, CRLF) — was `FixCrossesAnInstanceFieldOfExcludedCodeInAStruct` | RH7102-006, SHARED-ORDER-008 | `VerifyNoCodeFixIsOffered` | red |
| RH7109 ExcludedCode `NoCodeFixWhenAGroupOfAWholeAttributeListStandsInFrontOfTheMovedField` (LF, CRLF) — was `FixMovesAFieldWithAnExcludedAttributeInItsLeadingTrivia` | RH7109-006, SHARED-ORDER-008 | `VerifyNoCodeFixIsOffered` | red |
| RH7109 ExcludedCode `NoCodeFixWhenTheSpansLieInOneBranchOfAGroupThatOpensInTheBodyInFrontOfThem` (LF, CRLF) — was `FixMovesAFieldInsideAGroupOutsideBothSpansWithNoGroupInTheSpans` | RH7109-006, SHARED-ORDER-008 (third place) | `VerifyNoCodeFixIsOffered` | red |
| RH7109 TypeGroup `NoCodeFixWhenTheEndifOfAGroupOfAWholeTypeInFrontStandsOnTheFirstTokenOfTheType` (LF, CRLF) — was `FixMovesAFieldOfATypeBehindAGroupOfAWholeTypeInFrontOfIt` | RH7109-006, SHARED-ORDER-008 (spec example; 1668-2 A) | `VerifyNoCodeFixIsOffered` | red |
| RH7109 TypeGroup `NoCodeFixWhenAGroupThatWrapsAnotherDeclarationOfThePartialClassEndsOnTheFirstTokenOfTheReorderedOne` (LF, CRLF) | RH7109-006, SHARED-ORDER-008 (FC-003 on the exception) | `VerifyNoCodeFixIsOffered` | red |
| RH7109 TypeGroup `NoCodeFixWhenTheSpansLieInTheActiveBranchOfAGroupWithAnElseBranch` (LF, CRLF) | RH7109-006, SHARED-ORDER-008 | `VerifyNoCodeFixIsOffered` | red |
| RH7109 TypeGroup `NoCodeFixWhenAGroupOfWholeMembersStandsInTheBodyInFrontOfTheSpans` (LF, CRLF) | RH7109-006, SHARED-ORDER-008 | `VerifyNoCodeFixIsOffered` | red |
| RH7109 TypeGroup `NoCodeFixWhenAMethodInFrontOfTheSpansHoldsAGroupOfWholeStatements` (LF, CRLF) | RH7109-006, SHARED-ORDER-008 | `VerifyNoCodeFixIsOffered` | red |
| RH7109 TypeGroup `NoCodeFixWhenAGroupOfWholeAttributeListsOfAMemberStandsInFrontOfTheSpans` (LF, CRLF) | RH7109-006, SHARED-ORDER-008 | `VerifyNoCodeFixIsOffered` | red |
| RH7109 TypeGroup `NoCodeFixWhenANestedTypeInFrontOfTheSpansHoldsAGroupOfWholeMembers` (LF, CRLF) | RH7109-006, SHARED-ORDER-008 | `VerifyNoCodeFixIsOffered` | red |
| RH7109 TypeGroup `NoCodeFixWhenAGroupInTheBodyInFrontOfTheSpansDeclaresAField` (`DEBUG` defined or not) | RH7109-006, SHARED-ORDER-008 (spec example) | `VerifyNoCodeFixIsOffered` | red |
| RH7109 TypeGroup `FixMovesAFieldOfATypeThatAGroupWrapsWhole` (LF, CRLF) | RH7109-006, SHARED-ORDER-008 (spec example; exception) | `VerifyCodeFixWithEndOfLine` | green |
| RH7109 TypeGroup `FixMovesAFieldOfATypeThatAGroupWrapsWholeAtTheEndOfTheDocument` | SHARED-ORDER-008 (exception; the token behind is end of file) | `Verify` | green |
| RH7109 TypeGroup `NoCodeFixWhenAGroupAroundTheWholeTypeHasAnotherBranch` (`#else` with a type, `#elif`, empty `#else`) | RH7109-006, SHARED-ORDER-008 (spec example) | `VerifyNoCodeFixIsOffered` | red |
| RH7109 TypeGroup `NoCodeFixWhenAGroupAroundTheTypeClosesBehindAnotherType` | SHARED-ORDER-008 (FC-003 on the `#endif` place) | `VerifyNoCodeFixIsOffered` | red |
| RH7309 Trivia `ConditionalPairInsideABlockWhosePlaceChangesWithholdsTheFix` — was `BalancedConditionalPairInsideABlockDoesNotWithholdTheFix` | RH7309-005, SHARED-ORDER-008 (spec example) | `VerifyNoCodeFixIsOffered` | red |
| RH7309 Trivia `VerifyNoCodeFixWhenAMovedRegionContainsBalancedConditionalDirectives` — was `VerifyCodeFixReordersRegionsContainingBalancedConditionalDirectives` | RH7309-005, SHARED-ORDER-008 | `VerifyNoCodeFixIsOffered` | red |
| RH7309 Trivia `VerifyNoCodeFixWhenAMovedRegionCrossesBalancedConditionalDirectivesBetweenRegions` — was `VerifyCodeFixReordersRegionsWithBalancedConditionalDirectivesBetweenThem` | RH7309-005, SHARED-ORDER-008 | `VerifyNoCodeFixIsOffered` | red |
| RH7309 Trivia `ConditionalPairInsideABlockThatKeepsItsPlaceDoesNotWithholdTheFix` | RH7309-005, SHARED-ORDER-008 (spec example) | `Verify` | green |
| RH7309 Trivia `ConditionalGroupBetweenBlocksThatNoMovedBlockCrossesDoesNotWithholdTheFix` | RH7309-005, SHARED-ORDER-008 (FC-003 on "cross") | `Verify` | green |
| RH7309 Trivia `NoCodeFixWhenAGroupInFrontOfTheFirstBlockPrecedesAReorderOfInstanceFields` | RH7309-005, SHARED-ORDER-008 (1668-2 A) | `VerifyNoCodeFixIsOffered` | red |
| RH7309 Trivia `GroupThatWrapsTheWholeTypeKeepsAReorderOfInstanceFields` | SHARED-ORDER-008 (exception) | `Verify` | green |

Existing tests that stay green, reviewed and kept with their assertion unchanged (summary updated where it named the retired whole-unit reading): RH7103 `FixMovesAStaticMethodOfAClassWithAConditionalAttributeList`, `FixMovesAStaticMethodOfAClassWithAConditionalEntryInItsBaseList`, `FixMovesAStaticFieldBehindAGroupInFrontOfTheSpansThatClosesTheTypeAndOpensAStruct`, `FixMovesAStaticFieldOfAPartialClassWhoseOtherDeclarationHoldsAConditionalLayoutAttribute`, and every RH7103 no-fix test tagged SHARED-ORDER-008. RH7102 `FixMovesAConstantBehindAGroupInFrontOfTheSpansThatClosesTheTypeAndOpensAStruct`, `NoCodeFixWhenTheMoveCrossesADeclarationThatIsConstOnlyInSomeConfigurations`, `NoCodeFixWhenACrossedFieldIsStaticOnlyInAGroupInFrontOfItsFirstToken`, and `FixAllSkipsTheConstantWhoseMoveWouldReorderTheInitializersOfAnotherConfiguration` (summary updated: K now refused for the group in its leading trivia). RH7109 TypeGroup: every attribute-list, base-list, kind, and `partial`-declaration no-fix test; the static-field and no-directive fix tests; `FixMovesAFieldOfAPartialClassWhoseOtherDeclarationHoldsAConditionalGroupInItsBody` and `FixMovesAFieldOfATypeWhoseEnclosingTypeHoldsAConditionalGroup` (places the spec says are not read); `FixMovesAFieldInFrontOfAGroupBehindTheSpansThatHoldsAModifier`; and the Fix All test. RH7309 Trivia `VerifyCodeFixReordersRegionsWrappedByTheSameConditionalDirective` stays a fix: the group surrounds the blocks and the reorder moves no instance field. RH7309 Analyzer tests are analyzer-only. RH7107, RH7108, `AccessorOrderingUtilitiesTests.MoveRangeContainsDirectivesReturnsFalseWhenCrossedAccessorBodyContainsBalancedConditional`, and RH2001-010's tests are unchanged.

Deleted, as the spec author's handover directs for tests that only told kept groups apart. Each one's input is now either refused because its group stands in the spans, which a test above pins per place, or it is a duplicate of one:

- RH7103: the `field`-keyword and bodiless-accessor tests (`NoCodeFixWhenAGroupInACrossedAccessorBodyUsesTheFieldKeyword`, `FixMovesAMemberAcrossAnAccessorWhoseGroupOfWholeStatementsDoesNotUseTheFieldKeyword`, `NoCodeFixWhenTheActiveBranchOfAGroupInACrossedAccessorBodyUsesTheFieldKeyword`, `NoCodeFixWhenAGroupOfWholeAccessorsOfACrossedPropertyUsesTheFieldKeyword`, `FixMovesAMemberAcrossAPropertyOfAStructWhoseGroupOfWholeAccessorsDoesNotUseTheFieldKeyword`, `NoCodeFixWhenAGroupOfWholeAccessorsOfACrossedPropertyOfAClassHoldsAnAccessorWithoutABody`, `NoCodeFixWhenAnActiveGroupOfWholeAccessorsOfACrossedPropertyOfAClassHoldsAnAccessorWithoutABody`). Also the excluded fields moved along (`FixMovesExcludedCodeInFrontOfTheMovedStaticFieldWithIt`, `FixCarriesAnInstanceFieldOfExcludedCodeInFrontOfTheMovedStaticFieldOfAClass`, `FixCarriesAnInstanceFieldOfExcludedCodeInFrontOfTheMovedStaticMethodOfAStructAcrossOnlyAMethod`), `FixMovesAStaticPropertyWithAnAssignmentInAnExcludedAccessor`, and `MethodIndentedAsItsSlotMovesWithTheDisabledTextItHolds`. For the last one: disabled text in a member move now always comes with a conditional directive in the span, so SHARED-ORDER-010's disabled-text clause is live only for the accessor moves. The RH7107 tests `ExcludedCodeAtTheEnd…` move accessors with disabled text, and `NoCodeFixWhenAShiftedMethodHoldsDisabledText` stays green.
- RH7102: all 17 SHARED-ORDER-007 excluded-code counting tests except the two converted above.
- RH7109 ExcludedCode: 66 tests of the excluded-branch parse and the excluded-member count. Kept: `FixMovesAStaticFieldOfAStructAcrossAnInstanceField`, the collection-initializer, expression-body, lambda, and SHARED-FIX-009 refusals (spans), its counterpart without a group, the two groups behind the spans (`FixMovesAFieldWhenAConditionalGroupOfTheBodyClosesBehindTheClosingBraceOfTheType` fix, `…AndAnInitializedFieldIsCrossed` no fix under SHARED-ORDER-007), and the two converted above.
- RH7109 TypeGroup: the excluded-partial-declaration parse tests (`FixMovesAFieldWhenAnExcludedPartialDeclarationOfAnotherNameWithAnAttributeListStandsInFrontOfIt`, `…OfTheNameWithAnotherArity…`, `…OfTheNameIsNestedInAnotherType`, `FixMovesAFieldOfATypeBehindANestedGroupOfAWholeTypeInFrontOfIt`, `NoCodeFixWhenAnExcludedBranchInFrontOfThePartialClassDeclaresAnotherDeclarationOfItWithAnAttributeListThatEndsInASemicolon`, `NoCodeFixWhenAnExcludedBranchInFrontOfAnotherTypeOfTheGroupAroundTheFirstTokenHoldsAnAttributeList`, `…ElifBranch…`), and `NoCodeFixWhenAGroupAroundTheSpansHasAnExcludedBranchThatIsNoWholeMember`.
- RH7309 InitializationOrder: the 8 `field`-keyword and bodiless-accessor group tests.

### Not covered, and why

- RH7102 and RH7109 share the spans refusal with RH7103 through `TypeMemberOrderingCodeFixProviderBase.IsMoveSafe` (same `rg` as for #970), so the per-place matrix is on RH7103 only. RH7102 and RH7109 keep one test each in the moved and the crossed span. The layout clause (1668-2) is tested on RH7109 and RH7309, the two fixes that reorder instance fields. `RegionBlockLayout.ChangesMemberOrder` → `OrderingDeclarationUtilities.ChangesMemberOrder(…, Func<int,int>)` is RH7309's separate entry.
- Core unit tests `OrderingDeclarationUtilitiesTests.MoveRangeContainsDirectivesReturnsFalseWhenCrossedMemberBodyContainsBalancedConditional` and `…MovedMemberBodyContainsBalancedConditional` are left asserting `false`. That follows the skeptic's recommendation to put the 1668 refusal in `ChangesMemberOrder`, not in `MoveRangeContainsDirectives`. If the implementer puts it in `MoveRangeContainsDirectives`, these two must flip through the lead (they pin an internal method, not a visible output).
- FC-033: LF and CRLF are run on every converted test that had them and on the spec examples. No formatter surface is involved (SHARED-ORDER-009). FC-020: `#if false`, `#elif`, `#else`, and an active group are covered. FC-008: the Fix All test above. FC-044 is the change itself: no test reads an excluded branch. FC-038–043 do not apply: no shape is declared.

### Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH7102|FullyQualifiedName~RH7103|FullyQualifiedName~RH7109|FullyQualifiedName~RH7309"
```

### Follow-up candidates

_None_ beyond RH2001-010, which `00-lead.md` already records.

## Whole run

- Baseline on e4d7458 before any change: `scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH7102|…RH7103|…RH7107|…RH7108|…RH7109|…RH7309"`: 744 passed.
- After the change on e4d7458: the same filter: 614 total, 534 passed, 80 red. Core filter above: 29 total, 27 passed, 2 red (the two Core tests replaced for #970). Every red test is one listed above as `red`, and fails for the reason given at the top. `scripts/test.sh --project architecture`: 198 passed (SHARED-ORDER-011's coverage check is now closed by the tags).
- Full analyzer project (`scripts/test.sh --project analyzer`): 11809 total, 11729 passed, 80 red — exactly the 80 of the focused filter; no other analyzer test changes outcome.
- Formatted with `scripts/format.sh` on every changed test file.

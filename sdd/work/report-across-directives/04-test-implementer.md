Role: test implementer
Commit: 7adfc5f (the tests are in the commit that adds this file)
Verdict: tests written

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Inputs: thoenissen/Reihitsu#1759, `00-lead.md` (batch 1 answered 1A–6A, decisions 1–6), `01-spec-author.md`, `02-skeptic.md`, `03-spec-author.md`, the accepted change doc `sdd/changes/2026-11/report-across-conditional-directives.md`, and `git diff 340608b HEAD -- sdd/specs documentation`. Expected values come from the spec examples and requirements; no production code was read for them.

All test files are under `src/Reihitsu.Analyzer.Test/`. Every test below that is "red now" fails on `7adfc5f` for one reason only: the analyzer does not report the diagnostic the markup expects (`Mismatch between number of diagnostics returned, expected "n" actual "n-k"`); 185 failures, all of that kind (trx classified). No failure is a compile error, a missing helper, or a fix-output mismatch.

## Tests

"Changed" = an existing test that asserted the old silence, rewritten to the new output (old name in brackets). "New" = added. Matrix tests carry one DataRow per directive shape: empty `#if X/#endif`, `#if NEVER` with excluded code, `#if !NEVER` with active code, `#if/#else`, `#if/#elif`, `#if false` (where the shape fits the construct).

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| **RH7105** `Ordering/RH7105DeclarationKeywordsMustFollowOrderAnalyzerTests` | | | |
| `MisorderedModifiersSplitByAConditionalDirectiveAreReportedWithoutFix` (changed, was `…AreNotReported`) — with and without `FEATURE` | RH7105-001, SHARED-DIAG-008 | `VerifyNoCodeFixIsOffered` | red |
| `ConditionalDirectiveInFrontOfALaterModifierKeepsTheReportAndWithholdsTheFix` (new, 6 shapes) | RH7105-001, SHARED-DIAG-008, SHARED-MODORDER-003 | `VerifyNoCodeFixIsOffered` | red |
| `ExcludedModifierBetweenTwoModifiersKeepsTheReportAndWithholdsTheFix` (new, RH7105 spec example, both builds) | RH7105-001, SHARED-DIAG-008 | `VerifyNoCodeFixIsOffered` | red |
| `FixAllCorrectsOnlyTheDeclarationWithoutAConditionalDirectiveBetweenItsModifiers` (new) | RH7105-001, SHARED-DIAG-008 | `Verify(source, fixed)` | red |
| `ConditionalDirectiveBehindTheLastModifierDoesNotKeepTheDeclarationFromBeingReported` (changed: `Value` now marked; spec example) | RH7105-001, SHARED-DIAG-008, SHARED-MODORDER-003 | `VerifyNoCodeFixIsOffered` | red |
| `ConditionalDirectiveThatStandsBetweenModifiersWhereTheSymbolIsDefinedKeepsTheReportAndWithholdsTheFix` (changed, was `…KeepsTheDeclarationFromBeingReported`; `DEBUG`) | RH7105-001, SHARED-DIAG-008 | `VerifyNoCodeFixIsOffered(…, "DEBUG")` | red |
| **RH7106** `Ordering/RH7106ProtectedMustComeBeforeInternalAnalyzerTests` | | | |
| `MisorderedCompoundAccessModifiersSplitByAConditionalDirectiveAreReportedWithoutFix` (changed, was `…AreNotReported`) | RH7106-001, SHARED-DIAG-008 | `VerifyNoCodeFixIsOffered` | red |
| `ConditionalDirectiveBetweenTheKeywordsKeepsTheReportAndWithholdsTheFix` (new, 6 shapes) | RH7106-001, SHARED-DIAG-008, SHARED-MODORDER-003 | `VerifyNoCodeFixIsOffered` | red |
| `FixAllCorrectsOnlyTheDeclarationWithoutAConditionalDirectiveBetweenItsModifiers` (new) | RH7106-001, SHARED-DIAG-008 | `Verify(source, fixed)` | red |
| `ConditionalDirectiveBehindTheLastModifierDoesNotKeepTheDeclarationFromBeingReported` (changed: `A` now marked; spec example) | RH7106-001, SHARED-DIAG-008, SHARED-MODORDER-003 | `VerifyNoCodeFixIsOffered` | red |
| `ConditionalDirectiveThatStandsBetweenModifiersWhereTheSymbolIsDefinedKeepsTheReportAndWithholdsTheFix` (changed; `DEBUG`) | RH7106-001, SHARED-DIAG-008 | `VerifyNoCodeFixIsOffered(…, "DEBUG")` | red |
| **RH7102** `Ordering/RH7102ConstantsMustAppearBeforeFieldsAnalyzerTests` | | | |
| `ConstantWhoseModifiersAnIfSplitsIntoAStaticFieldIsReportedWithoutFix` (changed, was `…IsNotReported`; LF+CRLF; `DEBUG` build: nothing reported) | RH7102-001, SHARED-DIAG-008 | `VerifyNoCodeFixIsOffered`, `VerifyWithPreprocessorSymbols` | red |
| `ConstantWhoseModifiersAnIfSplitsIntoAnInstanceFieldIsReportedWithoutFix` (changed; RH7102-001 spec example; LF+CRLF; both builds) | RH7102-001, SHARED-DIAG-008 | same | red |
| `ConditionalDirectiveInTheModifierListOfAConstantKeepsTheReportAndWithholdsTheFix` (new, 6 shapes) | RH7102-001, SHARED-DIAG-008 | `VerifyNoCodeFixIsOffered` | red |
| `FixAllMovesOnlyTheConstantWithoutAConditionalDirectiveInItsModifierList` (new; guarded and unguarded constants in separate types) | RH7102-001, SHARED-DIAG-008 | `Verify(source, fixed)` | red |
| **RH7103** `Ordering/RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzerTests` | | | |
| `StaticMemberWhoseStaticModifierAnIfSplitsIsReportedWithoutFix` (changed, was `…IsNotReported`; spec example; LF+CRLF; `DEBUG` build: nothing) | RH7103-001, SHARED-DIAG-008 | `VerifyNoCodeFixIsOffered`, `VerifyWithPreprocessorSymbols` | red |
| `ConditionalDirectiveInTheModifierListOfAStaticMemberKeepsTheReportAndWithholdsTheFix` (new, 6 shapes) | RH7103-001, SHARED-DIAG-008 | `VerifyNoCodeFixIsOffered` | red |
| `FixAllMovesOnlyTheStaticMembersWithoutAConditionalDirectiveInTheirModifierLists` (new) | RH7103-001, SHARED-DIAG-008 | `Verify(source, fixed)` | red |
| **RH7107** `Ordering/RH7107PropertyAccessorsMustFollowOrderAnalyzerTests` | | | |
| `ConditionalDirectiveAroundTheSetAccessorIsJudgedAsTheBuildCompilesIt` (changed, was `…KeepsTheGetAccessorFromBeingReported`): no `DEBUG` → nothing (only `get` compiled); `DEBUG` → reported, no fix | RH7107-001, RH7107-005, SHARED-DIAG-008 | `Verify`, `VerifyNoCodeFixIsOffered(…, "DEBUG")` | red |
| `ConditionalDirectiveBehindTheAttributeListOfTheGetAccessorKeepsTheReportAndWithholdsTheFix` (changed; `DEBUG`) | RH7107-001, RH7107-005, SHARED-DIAG-008 | `VerifyNoCodeFixIsOffered(…, "DEBUG")` | red |
| `ConditionalDirectiveBetweenTheAccessorsOrInsideABodyDoesNotKeepTheGetAccessorFromBeingReported` (changed; spec example: `A` reported without fix, `B` fixed) | RH7107-001, RH7107-005, SHARED-DIAG-008 | `Verify(source, fixed)` | red |
| `BalancedConditionalDirectivePairInFrontOfTheGetAccessorIsReportedAndMovesWithIt` (new, replaces `EmptyConditionalDirectivePairBetweenTheAccessorsKeepsTheGetAccessorFromBeingReported`; empty, excluded, `#if false`) | RH7107-001, RH7107-005, SHARED-DIAG-008, SHARED-ORDER-006 | `Verify(source, fixed)` | red |
| `ConditionalGroupAroundTheCrossedSetAccessorKeepsTheReportAndWithholdsTheFix` (new; active, if/else, elif) | RH7107-001, RH7107-005, SHARED-DIAG-008, SHARED-ORDER-003 | `VerifyNoCodeFixIsOffered` | red |
| `FixAllMovesOnlyTheGetAccessorsWhoseMoveKeepsTheirDirectivesPaired` (new) | RH7107-001, RH7107-005, SHARED-DIAG-008 | `Verify(source, fixed)` | red |
| **RH7108** `Ordering/RH7108EventAccessorsMustFollowOrderAnalyzerTests` | | | |
| `ConditionalDirectiveAroundTheRemoveAccessorKeepsTheReportAndWithholdsTheFix` (changed; `DEBUG`) | RH7108-001, RH7108-005, SHARED-DIAG-008 | `VerifyNoCodeFixIsOffered(…, "DEBUG")` | red |
| `ConditionalDirectiveBehindTheAttributeListOfTheAddAccessorKeepsTheReportAndWithholdsTheFix` (changed; `DEBUG`) | RH7108-001, RH7108-005, SHARED-DIAG-008 | `VerifyNoCodeFixIsOffered(…, "DEBUG")` | red |
| `ConditionalDirectiveBetweenTheAccessorsOrInsideABodyDoesNotKeepTheAddAccessorFromBeingReported` (changed; spec example) | RH7108-001, RH7108-005, SHARED-DIAG-008 | `Verify(source, fixed)` | red |
| `BalancedConditionalDirectivePairInFrontOfTheAddAccessorIsReportedAndMovesWithIt` (new, replaces `EmptyConditionalDirectivePair…KeepsTheAddAccessorFromBeingReported`) | RH7108-001, RH7108-005, SHARED-DIAG-008, SHARED-ORDER-006 | `Verify(source, fixed)` | red |
| `ConditionalGroupAroundTheCrossedRemoveAccessorKeepsTheReportAndWithholdsTheFix` (new) | RH7108-001, RH7108-005, SHARED-DIAG-008, SHARED-ORDER-003 | `VerifyNoCodeFixIsOffered` | red |
| `FixAllMovesOnlyTheAddAccessorsWhoseMoveKeepsTheirDirectivesPaired` (new) | RH7108-001, RH7108-005, SHARED-DIAG-008 | `Verify(source, fixed)` | red |
| **RH5102** `Formatting/RH5102ArgumentsShouldBeOnSingleOrSeparateLinesAnalyzerTests` | | | |
| `VerifyConditionalDirectiveBetweenArgumentsKeepsTheReportAndWithholdsTheFix` (changed, was `…KeepsTheListFromBeingReported`; both builds) | RH5102-004, SHARED-DIAG-008 | `VerifyNoCodeFixIsOffered` | red |
| `VerifyEveryShapeOfConditionalDirectiveBetweenArgumentsKeepsTheReportAndWithholdsTheFix` (new, 6 shapes) | RH5102-004, SHARED-DIAG-008 | `VerifyNoCodeFixIsOffered` | red |
| `VerifyArgumentsOnOneLineWithAConditionalArgumentInFrontOfTheClosingParenthesisAreNotReported` (new, stable side) | RH5102-001, SHARED-DIAG-008 | `Verify` | green |
| `VerifyFixAllLaysOutOnlyTheArgumentListsWithoutAConditionalDirective` (new) | RH5102-004, SHARED-DIAG-008 | `Verify(source, fixed)` | red |
| `VerifyConditionalDirectiveInAGapOrInsideAnArgumentWithholdsOnlyTheFix` (changed; spec example) | RH5102-004, SHARED-DIAG-008 | `VerifyNoCodeFixIsOffered` | red |
| `VerifyConditionalDirectiveWithAlternativesInFrontOfTheFirstArgumentKeepsTheReportAndWithholdsTheFix` (changed; both builds) | RH5102-004, SHARED-DIAG-008 | `VerifyNoCodeFixIsOffered` | red |
| `VerifyConditionalDirectiveExcludingAnArgumentKeepsTheReportAndWithholdsTheFix` (changed; LF+CRLF; both builds) | RH5102-004, SHARED-DIAG-008 | `VerifyNoCodeFixIsOffered` | red |
| **RH5109** `Formatting/RH5109ParametersMustBeOnSameLineOrSeparateLinesAnalyzerTests` | | | |
| `VerifyConditionalDirectiveBetweenParametersKeepsTheReportAndWithholdsTheFix` (changed; both builds) | RH5109-004, SHARED-DIAG-008 | `VerifyNoCodeFixIsOffered` | red |
| `VerifyEveryShapeOfConditionalDirectiveBetweenParametersKeepsTheReportAndWithholdsTheFix` (new, 6 shapes) | RH5109-004, SHARED-DIAG-008 | `VerifyNoCodeFixIsOffered` | red |
| `VerifyParametersOnOneLineWithAConditionalParameterInFrontOfTheClosingParenthesisAreNotReported` (new, stable side) | RH5109-001, SHARED-DIAG-008 | `Verify` | green |
| `VerifyFixAllLaysOutOnlyTheParameterListsWithoutAConditionalDirective` (new) | RH5109-004, SHARED-DIAG-008 | `Verify(source, fixed)` | red |
| `VerifyConditionalDirectiveInAGapOrInsideAParameterWithholdsOnlyTheFix` (changed; spec example) | RH5109-004, SHARED-DIAG-008 | `VerifyNoCodeFixIsOffered` | red |
| `VerifyConditionalDirectiveExcludingAParameterKeepsTheReportAndWithholdsTheFix` (changed; LF+CRLF; both builds) | RH5109-004, SHARED-DIAG-008 | `VerifyNoCodeFixIsOffered` | red |
| **RH5111** `Formatting/RH5111AssignmentsMustHaveProperLineBreaksAnalyzerTests` | | | |
| `VerifyDeclaratorWithConditionalDirectiveInFrontOfItsEqualsSignIsReportedWithoutFix` (changed) | RH5111-005, SHARED-DIAG-008 | `VerifyNoCodeFixIsOffered` | red |
| `VerifyEveryShapeOfConditionalDirectiveAroundTheEqualsSignKeepsTheReportAndWithholdsTheFix` (new, 6 shapes) | RH5111-005, SHARED-DIAG-008, SHARED-DIAG-010 | `VerifyNoCodeFixIsOffered` | red |
| `VerifyFixAllJoinsOnlyTheDeclaratorsWithoutAConditionalDirective` (new) | RH5111-005, SHARED-DIAG-008 | `Verify(source, fixed)` | red |
| `VerifyConditionalDirectiveInTheJudgedSpanOrInsideTheValueWithholdsOnlyTheFix` (changed; spec example) | RH5111-005, SHARED-DIAG-008 | `VerifyNoCodeFixIsOffered` | red |
| `VerifyDeclaratorWithConditionalDirectiveBehindItsEqualsSignIsReportedWithoutFix` (changed; both builds) | RH5111-005, SHARED-DIAG-008 | `VerifyNoCodeFixIsOffered` | red |
| **RH7101** `Formatting/RH7101DoNotCombineFieldsAnalyzerTests` | | | |
| `FieldDeclarationWithAConditionalDeclaratorBetweenItsDeclaratorsIsReportedOnTheSecondCompiledDeclarator` (changed; `_b` without `DEBUG`, `_debug` with) | RH7101-001, SHARED-DIAG-008 | `VerifyNoCodeFixIsOffered` | red |
| `SpecificationExampleConditionalDeclaratorIsReportedOnTheSecondCompiledDeclaratorWithoutFix` (new, RH7101-001 example) | RH7101-001, SHARED-DIAG-008 | `VerifyNoCodeFixIsOffered` | red |
| `FieldDeclarationWithAnEmptyConditionalDirectivePairBetweenItsDeclaratorsIsReportedWithoutFix` (changed) | RH7101-001, SHARED-DIAG-008 | `VerifyNoCodeFixIsOffered` | red |
| `FieldDeclarationWithAConditionalDirectiveInTheInitializerOfANonLastDeclaratorIsReportedWithoutFix` (changed) | RH7101-001, SHARED-DIAG-008 | `VerifyNoCodeFixIsOffered` | red |
| `EveryShapeOfConditionalDirectiveBetweenTheDeclaratorsKeepsTheReportAndWithholdsTheFix` (new, 6 shapes, diagnostic on the second compiled declarator) | RH7101-001, SHARED-DIAG-008 | `VerifyNoCodeFixIsOffered` | red |
| `FixAllSplitsOnlyTheDeclarationsWithoutAConditionalDirectiveBetweenTheirDeclarators` (new; guarded declaration in its own type, see *Not covered*) | RH7101-001, SHARED-DIAG-008 | `Verify(source, fixed)` | red |
| **Attribute shape** `Formatting/RH55xx…AttributeListsMustFollowShapeRulesAnalyzerTests` | | | |
| `VerifyDiagnosticWithoutCodeFixWhenConditionalDirectiveIsBetweenTheAttributes` in RH5502, RH5504, RH5506, RH5508, RH5510, RH5512, RH5514, RH5516, RH5518, RH5520, RH5522, RH5526, RH5528, RH5531 (changed, each was `VerifyNoDiagnosticWhen…`; LF+CRLF; with and without `DEBUG`) | SHARED-ATTR-019 (plus the rule's own tag where it had one), SHARED-DIAG-008 | `VerifyNoCodeFixIsOffered` | red |
| `VerifyDiagnosticWithoutCodeFixWhenConditionalDirectiveIsBetweenTheLists` in RH5524, RH5529 (changed; LF+CRLF; `[D]` reported) | SHARED-ATTR-020 (plus rule tag), SHARED-DIAG-008 | `VerifyNoCodeFixIsOffered` | red |
| RH5506 `VerifyDiagnosticWithoutCodeFixWhenDisabledTextHoldsAnAttributeOfTheList` (changed; `#if false/#else`) | RH5506-004, SHARED-ATTR-019, SHARED-DIAG-008 | `VerifyNoCodeFixIsOffered` | red |
| RH5506 `VerifyDiagnosticWithoutCodeFixForEveryShapeOfConditionalDirectiveBetweenTheAttributes` (new, 5 shapes) | RH5506-004, SHARED-ATTR-019, SHARED-DIAG-008 | `VerifyNoCodeFixIsOffered` | red |
| RH5506 `VerifyFixAllSplitsOnlyTheListsWithoutAConditionalDirective` (new) | RH5506-004, SHARED-ATTR-019, SHARED-DIAG-008 | `Verify(source, fixed)` | red |
| RH5524 `VerifyListInFrontOfConditionalDirectiveIsStillReported` (changed: `[D]` behind the directive now reported too) | RH5524-004, SHARED-ATTR-020, SHARED-DIAG-008 | `Verify` | red |
| RH5524 `VerifyDiagnosticWithoutCodeFixForEveryShapeOfConditionalDirectiveBetweenTheLists` (new, 6 shapes) | RH5524-004, SHARED-ATTR-020, SHARED-DIAG-008 | `VerifyNoCodeFixIsOffered` | red |
| RH5524 `VerifyFixAllMergesOnlyTheListsWithoutAConditionalDirective` (new) | RH5524-004, SHARED-ATTR-020, SHARED-DIAG-008 | `Verify(source, fixed)` | red |
| **Trailing commas** `Formatting/RH5409…`, `RH5410…`, `RH5411…AnalyzerTests` | | | |
| RH5409 `VerifyEnumWithConditionalBlockAfterFinalMemberIsReportedWithoutFix` (changed, was `…IsNotFlagged`; comma behind `First` without `SYMBOL`, behind `Second` with it) | SHARED-TRAIL-002, SHARED-DIAG-008 | `VerifyNoCodeFixIsOffered` | red |
| RH5409 `VerifySpecificationExampleReportsCommaInFrontOfRegionAndInFrontOfConditionalBlock` (changed; also the Fix All contrast: `F` fixed, `E` stays) | SHARED-TRAIL-002, SHARED-DIAG-008 | `Verify(source, fixed)` | red |
| RH5409 `VerifySpecificationExampleReportsCommaInFrontOfEndIfWhereTheSymbolIsDefined` (new; SHARED-TRAIL-002 `DEBUG` example) | SHARED-TRAIL-002, SHARED-DIAG-008 | `VerifyNoCodeFixIsOffered(…, "DEBUG")` | red |
| RH5409 `VerifySpecificationExampleCommaInsideTheBranchIsNotReportedInEitherBuild` (new, stable side) | SHARED-TRAIL-002, SHARED-DIAG-008 | `Verify`, `VerifyWithPreprocessorSymbols` | green |
| RH5409 `VerifyCommaInFrontOfEndIfOfAnActiveBlockIsReportedWithoutFix` (new, `#if !NEVER`) | SHARED-TRAIL-002, SHARED-DIAG-008 | `VerifyNoCodeFixIsOffered` | red |
| RH5409 `VerifyFixAllKeepsOnlyTheCommaInFrontOfAConditionalBlock` (new; plain and `#region` commas removed) | SHARED-TRAIL-002, SHARED-TRAIL-003, SHARED-DIAG-008 | `Verify(source, fixed)` | red |
| RH5409 `VerifyCommaInFrontOfEmptyConditionalBlockIsReportedWithoutFix`, `VerifyCommaInFrontOfElseAndElifBranchesIsReportedWithoutFix`, `VerifyCommaInFrontOfInactiveIfFalseBlockIsReportedWithoutFix` (changed, each was `…IsNotReported`) | SHARED-TRAIL-002, SHARED-DIAG-008 | `VerifyNoCodeFixIsOffered` | red |
| RH5410 `VerifyArrayWithConditionalBlockAfterFinalItemIsReportedWithoutFix`, RH5411 `VerifyCollectionWithConditionalBlockAfterFinalItemIsReportedWithoutFix` (changed, were `…IsNotFlagged`; both builds) | SHARED-TRAIL-002, SHARED-DIAG-008 | `VerifyNoCodeFixIsOffered` | red |
| RH5410 `VerifyArrayWithCommaInsideTheConditionalBlockIsNotReportedInEitherBuild`, RH5411 `VerifyCollectionWithCommaInsideTheConditionalBlockIsNotReportedInEitherBuild` (new, stable side) | SHARED-TRAIL-002, SHARED-DIAG-008 | `Verify`, `VerifyWithPreprocessorSymbols` | green |
| RH5410 / RH5411 `VerifyFixAllKeepsOnlyTheCommaInFrontOfAConditionalBlock` (new) | SHARED-TRAIL-002, SHARED-DIAG-008 | `Verify(source, fixed)` | red |
| `Formatter/Formatting/RH5409…FormatterTests.VerifyEnumWithConditionalBlockAfterFinalMemberIsReportedAndKeptByTheFormatter` (changed, was `…IsFormatterStable`, which asserted analyzer-clean) | SHARED-TRAIL-002, SHARED-DIAG-008, FMT-STRUCT-019 | `VerifyFormatterKeepsReportedShape` (new) | red |
| `Formatter/Formatting/RH5410…` / `RH5411…FormatterTests.Verify{Array,Collection}WithConditionalBlockAfterFinalItemIsReportedAndKeptByTheFormatter` (changed, same) | SHARED-TRAIL-002, SHARED-DIAG-008, FMT-STRUCT-020 | `VerifyFormatterKeepsReportedShape` | red |
| **RH7311** `Formatting/RH7311RegionDescriptionsMustBeUniqueWithinATypeAnalyzerTests` | | | |
| `RegionOfTheCompiledBranchIsComparedWithARegionOutsideEveryBranch` (changed, was `RegionsInBranchesAndARegionOutsideEveryBranchAreNotCompared`; spec example; both builds) | RH7311-001, SHARED-DIAG-008 | `Verify` | red |
| `RegionInAnExcludedBranchIsNotComparedWithARegionOutsideEveryBranch` (new; `#if NEVER`, `#if false`) | RH7311-001, SHARED-DIAG-008 | `Verify` | green |
| `RegionOfTheCompiledElifBranchIsComparedWithARegionOutsideEveryBranch` (new) | RH7311-001, SHARED-DIAG-008 | `Verify` | red |
| `RegionsOutsideEveryBranchAreComparedAcrossAConditionalDirective` (changed: `DEBUG` build added, branch region also reported) | RH7311-001, SHARED-DIAG-008 | `Verify` | red |
| `RegionsInACompiledBranchThatStartsInsideTheDeclarationAreCompared` (changed, was `…AreNotCompared`) | RH7311-001, SHARED-DIAG-008 | `Verify` | red |
| `RegionInACompiledBranchInsideADeclarationThatAConditionalDirectiveEnclosesIsCompared` (changed, was `…IsNotCompared`) | RH7311-001, SHARED-DIAG-008 | `Verify` | red |
| **RH8201** `Documentation/RH8201InheritdocShouldBeUsedAnalyzerTests` | | | |
| `VerifyDiagnosticWithoutCodeFix…` ×10 (changed, each was `VerifyNoDiagnostic…`): `ForImplicitImplementationWhenInterfaceListIsConditional` (+ `FEATURE` build: nothing), `…WhenAnotherInterfaceIsDisabled` (change-doc example), `…WhenOtherPartialDeclarationIsConditional`, `ForFieldLikeEventWhenInterfaceListIsConditional`, `WhenConditionalRegionEnclosesPartialDeclaration`, `WhenConditionalRegionSuppliesAlternativeHeaders`, `WhenInterfaceMemberIsConditional`, `WhenInterfaceBaseListIsConditional`, `WhenFileOfTypeContainsUnrelatedConditionalRegion`, `WhenInterfaceFileContainsConditionalRegion` (two documents) | RH8201-003, SHARED-DIAG-008 | `VerifyNoCodeFixIsOffered`; two-document case `Verify(source, source, …)` | red |
| `VerifySpecificationExampleConditionalInterfaceListWithholdsTheFixOnlyForImplicitImplementations` (changed, was `…SuppressesOnlyImplicitImplementations`; override fixed, `Dispose` stays reported — also the Fix All contrast) | RH8201-003, RH8201-004, SHARED-DIAG-008 | `Verify(source, fixed)` | red |
| **Unchanged outputs, spec examples** | | | |
| RH2005 `VerifyConditionalDirectiveAmongTheModifiersDoesNotKeepTheFieldFromBeingReported` (new, RH2005-001 example) | RH2005-001, RH2005-005, SHARED-DIAG-008 | `VerifyNoCodeFixIsOffered` | green |
| RH5412 `VerifyClassWhoseOnlyMemberLiesInAConditionalBlockIsReportedWithoutCodeFix` (new, SHARED-DIAG-008 / SHARED-EMPTY-001 example; `DEBUG` build: nothing) | SHARED-DIAG-008, SHARED-EMPTY-001, SHARED-EMPTY-004 | `VerifyNoCodeFixIsOffered`, `VerifyWithPreprocessorSymbols` | green |
| RH7003 `VerifyDirectiveContainingNamespaceIsReported` (tag added only) | RH7003-001, SHARED-DIAG-008 | `Verify` | green |

Test infrastructure added (test code only):

- `Base/AnalyzerTestsBase{TAnalyzer}.cs`: `VerifyWithPreprocessorSymbols(source, params symbols)` — analyzer verification with symbols defined, for the "other build reports nothing" side (`VerifyNoCodeFixIsOffered` asserts a diagnostic exists, so it cannot state that side).
- `Base/FormatterTestsBase{TAnalyzer}.cs`: `VerifyFormatterKeepsReportedShape(source)` — the analyzer reports the marked diagnostics, formatting under LF and CRLF changes nothing, and the phase stability check holds. `VerifyFormatter(source)` asserts the formatted code is analyzer-clean, which SHARED-TRAIL-002 now contradicts for a comma the formatter keeps.
- Removed the now-unused private `DefineDebug` from the RH7107 and RH7108 test classes.

Existing tests that assert a changed output, searched by tag (`SHARED-DIAG-008`, `SHARED-TRAIL-002`, `SHARED-ATTR-019/020`, `RH8201-003`, `RH7311-001`), by name (`NotReported`, `NotFlagged`, `KeepsThe…FromBeingReported`, `NoDiagnostic…Conditional…`), and by `#if` in every test class of the affected rules and their formatter parity classes: all listed above as changed. Left unchanged on purpose: RH7311 `SameDescriptionInIfAndElseBranchIsNotReported`, `…WhenTheIfBranchIsCompiled`, `SameDescriptionInIfAndElifBranchIsNotReported` (two branches are never compared, RH7311-001 unchanged in that part), `RegionsOfADeclarationThatAConditionalDirectiveEnclosesAreCompared`; RH5506 `VerifyDiagnosticWhenConditionalDirectiveIsInFrontOfTheList`; RH5524 `VerifyDiagnosticWithoutCodeFixWhenConditionalDirectiveFollowsBlankLineAfterTheGroup`; every RH7105/RH7106 test of a directive behind the last modifier or in front of the first; RH5416 formatter test `VerifyFormatterDoesNotRewriteDirectiveContainingRecordStruct` (formatter only, output unchanged).

The fix outputs of the unguarded halves of every Fix All and spec-example test (RH7105, RH7106, RH7102, RH7103, RH7107, RH7108, RH5102, RH5109, RH5111, RH7101, RH5409–RH5411, RH5506, RH5524, RH8201) were checked green on the unchanged code by temporarily removing the guarded diagnostic's markup and running them (67 + 5 passed), then restoring the files; the expected values are not taken from patched code (FC-035).

## Scenarios (bug fix)

_N/A_ (behavior change).

## Not covered, and why

- **Fix output of `BalancedConditionalDirectivePairInFront…IsReportedAndMovesWithIt` (RH7107/RH7108)** cannot be checked before the analyzer reports. The expected output moves the accessor together with its leading `#if … #endif` lines in front of the crossed accessor (SHARED-ORDER-006, change doc *Accessor order*). If the implementer's correct move lays these lines out differently, the layout is the formatter's and the lead decides whether the expected value or the code changes; the assertion that the fix is offered, and that the pair moves with the accessor, is the requirement.
- **RH7101 Fix All in one type:** a type that holds a declaration with a directive between its declarators refuses the fix for every other declaration of that type, because RH7101's fix formats the field declaration in its type and the precheck refuses that context (SHARED-FIX-010, FMT-UNSUP-002, observed on `7adfc5f`). The Fix All test therefore puts the guarded declaration in its own type. Unchanged behavior, not part of this run.
- **Matrix only on one representative rule per shared base for attribute shapes:** each of the 16 attribute-shape rules has its converted test; the six-shape matrix and the Fix All contrast run on RH5506 (split, `TargetAttributeListShapeAnalyzerBase` / `TargetAttributeListShapeCodeFixProviderBase`) and RH5524 (merge). This is not a one-call-site proof (the base has overrides); the per-rule tests cover the report and the withheld fix for every rule.
- **Empty-type rules RH5413–RH5416, RH5418, RH2001, RH3002, RH7003:** output unchanged; decision 3 in `00-lead.md`. Only the spec examples (RH5412, RH2005, RH7003 tag) were added.
- **CRLF** is run where the old test ran it and on the formatter parity tests; the analyzer-only report tests do not involve layout. The Fix All outputs that lay out code (RH5102, RH5109, RH5111, RH7101, RH5506, RH5524, RH7107, RH7108) run under LF only, as their neighbours do; the fixes delegate layout to the formatter, whose line-ending behavior the change does not touch.
- **Failure classes:** FC-003, FC-005, FC-008, FC-016, FC-019, FC-020 (all shapes incl. `#if false`), FC-013/FC-014 (formatter parity rewritten for trailing commas; RH5111's exception is the maintainer's answer 3), FC-017 (exact outputs, both builds where the build matters), FC-033 (both builds, LF/CRLF where the old test had it), FC-035 — covered above. FC-004/FC-007/FC-009/FC-030 are reached only through the fixes, which keep their guards; FC-018, FC-022–FC-029, FC-036, FC-038–FC-043 concern renames, formatter phases, or declared shapes, which this change does not touch. FC-010–FC-012: no formatter change.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~.RH7105|FullyQualifiedName~.RH7106|FullyQualifiedName~.RH7102|FullyQualifiedName~.RH7103|FullyQualifiedName~.RH7107|FullyQualifiedName~.RH7108"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~.RH5102|FullyQualifiedName~.RH5109|FullyQualifiedName~.RH5111|FullyQualifiedName~.RH7101"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~AttributeListsMustFollowShapeRules"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~.RH5409|FullyQualifiedName~.RH5410|FullyQualifiedName~.RH5411"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~.RH7311|FullyQualifiedName~.RH8201|FullyQualifiedName~.RH5412|FullyQualifiedName~.RH2005|FullyQualifiedName~.RH7003"
```

On `7adfc5f` with these tests: 1569 run in those classes, 185 fail, all with "expected n actual n-k diagnostics".

## Follow-up candidates

_None_ beyond those already in `00-lead.md`.

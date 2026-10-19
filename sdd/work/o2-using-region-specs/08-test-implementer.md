Role: test implementer
Commit: efcbbe9 (specs as of that commit; tests added in 70d303b, e0ac61a, bca2cbf, 59a944f)
Verdict: tests written

Lane: specs and tests for existing behavior. No production code was touched. Every new test was derived from a spec example (or from a requirement sentence where the example alone would assert less, see below) and passes on the unchanged code. No test failed, so there is no finding against a spec.

Before this run `RequirementCoverageTests` listed 88 uncovered requirements: every requirement of RH7201–RH7207, RH7301–RH7311, RH7305A, plus SHARED-REGDESC-001/002, SHARED-TYPEREGION-001…006, and SHARED-USING-001…009. After it: 0. `RequirementCoverageTests`, `SpecConsistencyTests`, and `SpecStructureTests` pass (39 tests).

The coordinator's prompt carried no intended fix or verdict to ignore. It asked me to confirm the skeptic's N5; that test is below.

## Tests

All tests are in `src/Reihitsu.Analyzer.Test/`. "New" means one test per spec example of the listed requirements; "tagged" means existing tests that now carry `[Requirement]`. All pass now. Tests of using rules whose examples import namespaces that do not exist (`Microsoft.CodeAnalysis`, `Reihitsu.Core`, …) run with `CompilerDiagnostics.None`. Analyzer-only examples use `Verify(source)`, fix examples use `Verify(source, fixedSource)`, and examples that withhold the fix use `VerifyNoCodeFixIsOffered`.

| File (class) | New tests | Requirements / failure class | Helper | Expected now |
|---|---|---|---|---|
| `Ordering/RH7201…AnalyzerTests` | `EverySystemDirectiveBelowAnotherRootIsReportedOnItsName`, `GlobalSectionIsCheckedOnItsOwn`, `DirectivesOfDifferentSectionsAreNotCompared`, `DirectivesOfDifferentBlocksAreNotCompared`, `BlockWithPreprocessorDirectiveIsNotReported`, `StaticAliasAndLookAlikeDirectivesAreNotReported`, `CodeFixOrganizesTheWholeBlock`, `CodeFixLeavesTheCodeAfterTheBlockUnchanged`; 6 existing tagged | RH7201-001…004, SHARED-USING-001, -002, -005, -009 | `Verify`, `GetCodeFixActionsAsync` | pass |
| `Ordering/RH7202…AnalyzerTests` | `EveryAliasAboveARegularDirectiveIsReportedOnItsAliasName`, `GlobalAliasIsNotComparedWithALocalDirective`, `BlockWithPreprocessorDirectiveIsNotReported`, `AliasFollowedOnlyByStaticDirectivesIsNotReported`, `CodeFixOrganizesTheWholeBlock`, `CodeFixLeavesTheCodeAfterTheBlockUnchanged`; 2 tagged | RH7202-001…004, SHARED-USING-002, -003, -009 | `Verify` | pass |
| `Ordering/RH7203…AnalyzerTests` | `EveryDirectiveSortedBeforeItsNeighborIsReportedOnItsName`, `CaseDoesNotDecideTheOrder`, `BlockWithPreprocessorDirectiveIsNotReported`, `EqualKeysOtherGroupsAndOtherKindsAreNotReported`, `CodeFixOrganizesTheWholeBlock`; 6 tagged (among them `CodeFixDoesNotFormatFollowingType`) | RH7203-001…004, SHARED-USING-004, -007, -009 | `Verify` | pass |
| `Ordering/RH7204…AnalyzerTests` | `EveryAliasSortedBeforeItsNeighborIsReportedOnItsAliasName`, `GlobalSectionIsCheckedOnItsOwn`, `BlockWithPreprocessorDirectiveIsNotReported`, `AliasesOfOtherGroupsAndOtherKindsAreNotReported`, `CodeFixOrganizesTheWholeBlock`, `CodeFixLeavesTheCodeAfterTheBlockUnchanged`; 4 tagged | RH7204-001…004, SHARED-USING-002, -004, -007, -009 | `Verify` | pass |
| `Ordering/RH7205…AnalyzerTests` | `StaticDirectiveAboveARegularDirectiveIsReportedOnItsTypeName`, `StaticDirectiveMisplacedOnBothSidesIsReportedOnce`, `GlobalStaticDirectiveIsNotComparedWithALocalDirective`, `BlockWithPreprocessorDirectiveIsNotReported`, `StaticDirectivesBetweenRegularAndAliasDirectivesAreNotReported`, `CodeFixOrganizesTheWholeBlock`, `CodeFixLeavesTheCodeAfterTheBlockUnchanged`; 2 tagged | RH7205-001…004, SHARED-USING-002, -003, -009 | `Verify` | pass |
| `Ordering/RH7206…AnalyzerTests` | `EveryStaticDirectiveSortedBeforeItsNeighborIsReportedOnItsTypeName`, `BlockWithPreprocessorDirectiveIsNotReported`, `StaticDirectivesOfOtherGroupsAndOtherKindsAreNotReported`, `CodeFixOrganizesTheWholeBlock`, `CodeFixLeavesTheCodeAfterTheBlockUnchanged`; 3 tagged | RH7206-001…004, SHARED-USING-004, -009 | `Verify` | pass |
| `Formatting/RH7207…AnalyzerTests` | 21 new: the RH7207 examples (`BlockOutOfCanonicalOrderIsReported`, `MissingBlankLineBetweenGroupsIsReported`, `GroupsSharingOneLineAreReported`, `BlankLineInsideAGroupIsReported`, `OnlyTheBlockThatIsNotOrganizedIsReported`, `BlockWithPreprocessorDirectiveIsNotReported`, `OrganizedBlockWithSeveralBlankLinesAtABoundaryIsNotReported`, `BlockWithASingleDirectiveIsNotReported`, `DiagnosticOfABlockStartingWithAnAliasIsOnTheAliasTarget`, `DiagnosticOfABlockStartingWithAnAliasOfAKeywordTypeCoversTheDirective`, `CodeFixOrganizesTheWholeBlock`) and every SHARED-USING-001…009 example through the RH7207 fix (`EveryUsingBlockIsOrganizedOnItsOwn`, `GlobalDirectivesComeFirst`, `RegularThenStaticThenAliasDirectives`, `GroupIsSectionKindAndRootNamespace`, `ExactSystemRootComesFirst`, `OtherRootsAreOrderedAlphabetically`, `AliasesAreSortedByAliasNameWithinTheirGroup`, `EqualKeysKeepTheirSourceOrder`, `BlankLineBetweenGroupsAndNoneWithinAGroup`, `BlockLedByANullableDirectiveIsNotReported`, `DirectiveAfterTheBlockDoesNotPreventOrganizing`); 22 tagged | RH7207-001…004, SHARED-USING-001…009 | `Verify`, `ApplyCodeFixAsync`, `GetCodeFixActionsAsync` | pass |
| `Formatter/Formatting/RH7207…FormatterTests` | 11 new, the formatter side of SHARED-USING-001, -003…-009 (`FormatterOrdersEveryUsingBlockOnItsOwn` … `FormatterOrganizesABlockFollowedByAConditionalDirective`). The spec example is the input and the expected output. `namespace X { internal class Placeholder; }` stubs are appended to both, so the examples compile. | SHARED-USING-001, -003…-009; FC-013, FC-010 | `FormatterTestsBase.VerifyFormatter` (checks analyzer before and after, plus a second pass, under LF and CRLF) | pass |
| `Formatting/RH7301…AnalyzerTests` | `EveryMismatchedEndRegionIsReported`, `NestedRegionsPairInnermostFirst`, `PairInAnExcludedBranchIsChecked`, `MatchingDescriptionsAreNotReported`, `WhitespaceAndLeadingDoubleSlashAreNotPartOfTheDescription`, `BlockCommentBehindTheEndDirectiveIsPartOfTheDescription`, `CodeFixRepeatsTheRegionDescription`, `CodeFixKeepsIndentationAndCarriageReturnLineFeed`, `NoCodeFixIsOfferedWhenTheRegionHasNoDescription`; 9 tagged | RH7301-001…005, SHARED-REGDESC-001 | `Verify`, `ApplyCodeFixAsync` (CRLF), `VerifyNoCodeFixIsOffered` | pass |
| `Formatting/RH7302…AnalyzerTests` | `EveryRegionStartingWithALowerCaseLetterIsReported`, `OtherFirstCharactersAreNotReported`, `CodeFixCapitalizesTheFirstLetterAndKeepsTheSpacing`, `CodeFixRenamesOnlyTheEndRegionThatRepeatsTheName`; 8 tagged | RH7302-001…005 | `Verify` | pass |
| `Formatting/RH7303…AnalyzerTests` | `RegionDirectivesInsideExecutableCodeAreReported`, `RegionDirectivesInAnAccessorListAndABlockAreReported`, `RegionDirectivesBetweenDeclarationsAreNotReported`, `CodeFixRemovesBothDirectiveLinesBetweenStatements`, `CodeFixRemovesBothDirectiveLinesWithCarriageReturnLineFeed`; 2 tagged | RH7303-001…004, SHARED-TYPEREGION-005 | `Verify`, `ApplyCodeFixAsync` (CRLF, exact output) | pass |
| `Formatting/RH7304…AnalyzerTests` | `DirectivesIndentedUnlikeTheFirstTokenOfTheRegionAreReported`, `OnlyTheMisalignedDirectiveOfAPairIsReported`, `RegionWithoutCodeIsNotReported`, `CodeFixIndentsBothDirectivesLikeTheFirstToken`; 7 tagged | RH7304-001…004, SHARED-TYPEREGION-005 | `Verify` | pass |
| `Formatting/RH7305…AnalyzerTests` | `TypeWithMembersOfOneKindOutsideRegionsIsReported`, `TypeWithOneMemberOutsideTheRegionsIsReported`, `TypeWithEveryMemberInARegionIsNotReported`, `NestedTypesAndEnumsAreNotCheckedButCountAsMembers`, `EachDeclarationOfAPartialTypeIsCheckedOnItsOwn`, `TypesWithoutCountedMembersAreNotReported`, `MemberInANestedRegionCounts`, `RegionsAroundTheTypeOrInsideABodyDoNotCount`, `OneDiagnosticOnTheTypeNameForSeveralMembers`; 13 tagged | RH7305-001, -002, SHARED-TYPEREGION-001…004 | `Verify` | pass |
| `Formatting/RH7305A…AnalyzerTests` | `TypeMixingFieldsAndMethodsOutsideRegionsIsReported`, `NestedTypeCountsAsAKindOfItsOwn`, `EventFieldAndEventWithAccessorsAreOneKind`, `OperatorsAndConversionOperatorsAreOneKind`, `EventAndOperatorAreTwoKinds`, `OrganizedTypeWithSeveralKindsIsNotReported`; 11 tagged | RH7305A-001…003, SHARED-TYPEREGION-001…004 | `Verify` | pass |
| `Formatting/RH7306…AnalyzerTests` | `DescriptionsEndingWithTheWordImplementationAreReported`, `WordInsideOrBeforeOtherTextIsNotReported`, `CodeFixRemovesTheTrailingWordFromBothDirectives`; 3 tagged | RH7306-001…003, SHARED-REGDESC-002 | `Verify` | pass |
| `Formatting/RH7307…AnalyzerTests` | `DescriptionThatIsOnlyMemberOrMembersIsReported`, `LongerDescriptionIsNotReported`, `EveryRegionDirectiveIsReportedOnItsOwn`, `DirectivesInAnElementBodyAndAnExcludedBranchAreChecked`; 3 tagged | RH7307-001, -002, SHARED-REGDESC-001, -002 | `Verify` | pass |
| `Formatting/RH7308…AnalyzerTests` | `MembersOfKindsTheLabelDoesNotNameAreReported`, `NestedTypesAndDelegatesAreNotReported`, `LabelsOfNounsQualifiersAndSeparatorsAreRecognized`, `LabelWithAnyOtherWordIsNotChecked`, `DiagnosticIsOnTheMemberName`; 17 tagged | RH7308-001…003, SHARED-TYPEREGION-006 | `Verify` | pass |
| `Formatting/RH7309…AnalyzerTests` | `RegionCategoriesComeFromTheDescription`, `DescriptionDifferingInCaseOrMissingIsCustom`, `CustomRegionAfterInterfaceRegionInStructIsReported`, `RegionsOfAnInterfaceAreNotChecked`, `CodeFixOrdersRegionBlocksAndKeepsCodeBetweenThemInPlace`, `OneApplicationCorrectsEveryDiagnosticOfTheType` (two diagnostics, `NumberOfIncrementalIterations = 1` and `NumberOfFixAllIterations = 1`), `NoCodeFixWhenAPragmaDirectiveWouldBeMovedAcross`, `BalancedConditionalPairInsideABlockDoesNotWithholdTheFix`; 19 tagged | RH7309-001…005, SHARED-REGDESC-001, SHARED-TYPEREGION-006; FC-007 | `Verify`, `VerifyNoCodeFixIsOffered` | pass |
| `Formatting/RH7310…AnalyzerTests` | `RegionsWithNothingInsideAreReported`, `RegionsWithContentOrInsideExecutableCodeAreNotReported`, `CodeFixRemovesTheRegionAndTheBlankLinesAfterIt`, `CodeFixRemovesTheRegionWithCarriageReturnLineFeed`; 13 tagged | RH7310-001…004, SHARED-TYPEREGION-005 | `Verify`, `ApplyCodeFixAsync` (CRLF, exact output) | pass |
| `Formatting/RH7311…AnalyzerTests` | `RepeatedDescriptionsAreReportedOnTheLaterRegions`, `RegionsOfAnotherPartialDeclarationAreNotCompared`, `NestedRegionsAndRegionsOfNestedTypesAreNotTopLevelRegions`; 11 tagged | RH7311-001…003, SHARED-REGDESC-001, SHARED-TYPEREGION-006 | `Verify` | pass |
| `Formatting/RH7311…AnalyzerTests` | `SameDescriptionInIfAndElseBranchIsCurrentlyReported` — characterization, **untagged** (N5 / RH7311 Q1) | none (outside RH7311-001, which covers only regions outside any `#if` branch) | `Verify` | pass |

Where a test asserts more than its example:
- **"MUST NOT change anything outside the block" (RH7201/02/04/05/06-004).** The spec examples have canonical code after the block, so the same fix on them cannot show this. The `CodeFixLeavesTheCodeAfterTheBlockUnchanged` tests put a non-canonical `void M( ) {   }` after the block and expect it unchanged. RH7203 and RH7207 already had `CodeFixDoesNotFormatFollowingType`, which is now tagged.
- **"One application" (RH7309-004).** `OneApplicationCorrectsEveryDiagnosticOfTheType` checks this on two diagnostics with exact iteration counts.
- **Line endings.** The code fixes that remove or rewrite lines (RH7301-004, RH7303-004, RH7310-004) each have a CRLF variant that compares the exact output text.

**N5 confirmed.** Input: `class C` / `{` / `#if DEBUG` / `    #region Fields` / `    int _a;` / `    #endregion` / `#else` / `    #region Fields` / `    int _b;` / `    #endregion` / `#endif` / `}`, with no preprocessor symbols defined, so the `#if` branch is the inactive one. RH7311 reports the `#region Fields` of the `#else` branch, and only that one. This matches the RH7311 Q1 text in `05-spec-author.md`. I did not run it with `DEBUG` defined.

## Scenarios (bug fix)

_N/A — specs and tests lane._

## Not covered, and why

- **Characterization tests for the other open questions** (RH7203/RH7204/RH7206 Q1, RH7302 Q1, RH7303 Q1–Q4, RH7304 Q1–Q2, RH7306 Q1, RH7308 Q1, RH7309 Q1–Q3, RH7310 Q1, RH7311 Q2, the shared-using non-name alias, and N1–N8). The spec authors asked for them. My role file does not ask for characterization tests in this lane, and the lead admitted them only where the role file does, so I wrote one only for N5, which the lead requested explicitly. Each of these cases is described with input and real output in `05-spec-author.md`, ready to file.
- **Existing tests left untagged on purpose.** Their inputs fall outside the narrowed requirements, so a tag would claim coverage of behavior the spec does not state:
  - `RH7303.VerifyRegionsWithinMethodBodiesAreDetectedAndFixed`: the region sits right after `{`, which is outside RH7303-004. It is tagged only -001 and -003.
  - `RH7310.VerifyEmptyRegionIsRemoved` and the other RH7310 fix tests: the region is next to the class's own braces, which is outside RH7310-004.
  - `RH7302.VerifyCodeFixUpdatesAllMatchingEndRegionOccurrences`: the `#endregion` text is not exactly the description.
  - `RH7304` `#if`-split tests.
  - `RH7308.VerifyNoDiagnosticsForUntrackedMemberKindInStandardRegion` and `VerifyNoDiagnosticsForMembersWithoutRegions`.
- **SHARED-USING-002 on the formatter side.** The spec example puts `global using` after `using System;`. That is compiler error CS8915, so `VerifyFormatter`, which requires a compiling input, cannot run it. SHARED-USING-002 is covered through the analyzers and the RH7207 fix (`GlobalDirectivesComeFirst`, plus the tagged `CodeFixPlacesGlobalSectionBeforeLocalExactSystemSection` and `GlobalUsingDirectivesAreOrganized`). The formatter project's `UsingDirectiveOrderingTests` covers the global section, but it is untagged: `Reihitsu.Formatter.Test` declares no `RequirementAttribute`, and adding one was out of scope.
- **Failure classes:**
  - FC-003: both sides of each spec condition are covered — reported versus not reported, the fix offered versus withheld (RH7301-005, RH7309-005), and the blank-line count at a group boundary.
  - FC-007: one-pass convergence, covered by the verifier's iteration checks and by `OneApplicationCorrectsEveryDiagnosticOfTheType`.
  - FC-008: Fix All, covered by the existing `GetFixAllScenario` overrides, unchanged.
  - FC-010 and FC-013: covered by the formatter parity tests.
  - FC-019/020 (directive in the gap): covered by the SHARED-USING-009 tests and RH7309-005.
  - Not applicable, because no fix rewrites names or literals and this lane changes no production code: FC-018/036 (renames), FC-022/023 (literals, identifiers), FC-025/026/027/029, FC-028, FC-030–032, FC-035.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH7201|FullyQualifiedName~RH7202|FullyQualifiedName~RH7203|FullyQualifiedName~RH7204|FullyQualifiedName~RH7205|FullyQualifiedName~RH7206|FullyQualifiedName~RH7207"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH7301|FullyQualifiedName~RH7302|FullyQualifiedName~RH7303|FullyQualifiedName~RH7304|FullyQualifiedName~RH7305|FullyQualifiedName~RH7306|FullyQualifiedName~RH7307"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH7308|FullyQualifiedName~RH7309|FullyQualifiedName~RH7310|FullyQualifiedName~RH7311"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH7311RegionDescriptionsMustBeUniqueWithinATypeAnalyzerTests.SameDescriptionInIfAndElseBranchIsCurrentlyReported"
scripts/test.sh --project architecture --filter "FullyQualifiedName~RequirementCoverageTests|FullyQualifiedName~SpecConsistencyTests|FullyQualifiedName~SpecStructureTests"
```

`scripts/format.sh` was run over all 20 changed test files and changed nothing.

Full runs on 59a944f: `scripts/test.sh --project analyzer` passed all 5232 tests, and `scripts/test.sh --project architecture` passed all 72.

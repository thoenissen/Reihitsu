Role: test implementer
Commit: 8dd7b90
Verdict: #1264: reproduced; #1333: reproduced; #1334: reproduced; #1339: reproduced; #1337: reproduced; #1412: reproduced; #1592: reproduced

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Every expected value comes from the clarified spec examples and requirements (`git diff origin/main -- sdd/specs`), the maintainer's answers in `00-lead.md`, the skeptic's breaking inputs, and the issue bodies; none from patched code. Results below are on head 8dd7b90 (production code identical to `main`). "Fails" means it fails now on the issue's own expected-versus-actual difference (checked one by one in the test output: a missing, extra, or misplaced diagnostic of the rule under test, or a fix result that differs); "passes" means it holds today and pins a boundary or a settled cell.

Existing characterization tests that pinned the reported behavior were the reproductions (role file, step 1): each was run green on this head, then rewritten to the specified output, renamed, and tagged. They are marked *(rewritten from …)*.

## thoenissen/Reihitsu#1264

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH7308…MemberInANestedRegionIsJudgedByTheInnermostRegion` | RH7308-001, SHARED-TYPEREGION-006 | `Verify` | fails (`Value` reported against `Fields`, `_other` not) |
| `RH7308…MemberInANestedRegionWithAnUnrecognizedLabelIsNotChecked` | RH7308-001, RH7308-002 | `Verify` | fails |
| `RH7308…MemberInANestedRegionWithoutADescriptionIsNotChecked` | RH7308-001 | `Verify` | fails |
| `RH7308…MemberOfTheKindOfTheOuterRegionIsReportedInANestedRegionOfAnotherKind` | RH7308-001 | `Verify` | fails |
| `RH7308…MemberInARegionNestedTwoLevelsDeepIsJudgedByTheInnermostRegion` | RH7308-001 | `Verify` | fails |
| `RH7308…MemberBehindANestedRegionIsJudgedByTheTopLevelRegion` | RH7308-001 | `Verify` | fails (the member inside the nested region is reported too) |
| `RH7311…SameDescriptionInIfAndElseBranchIsNotReported` *(rewritten from `SameDescriptionInIfAndElseBranchIsCurrentlyReported`)* | RH7311-001, SHARED-TYPEREGION-006 | `Verify` | fails |
| `RH7311…SameDescriptionInIfAndElseBranchIsNotReportedWhenTheIfBranchIsCompiled` | RH7311-001, SHARED-TYPEREGION-006 | `Verify` + `DEBUG` | fails |
| `RH7311…SameDescriptionInIfAndElifBranchIsNotReported` | RH7311-001, SHARED-TYPEREGION-006 | `Verify` | fails |
| `RH7311…RegionsInBranchesAndARegionOutsideEveryBranchAreNotCompared` | RH7311-001 | `Verify` | fails |
| `RH7311…RegionsOutsideEveryBranchAreComparedAcrossAConditionalDirective` | RH7311-001 | `Verify` | fails (branch region also reported) |
| `RH7311…RegionOfANestedEnumIsNotComparedWithTheRegionsOfTheType` | RH7311-002, SHARED-TYPEREGION-006 | `Verify` | fails |
| `RH7311…RegionOfANestedEnumInFrontOfTheRegionsOfTheTypeIsNotCompared` | RH7311-002, SHARED-TYPEREGION-006 | `Verify` | fails |
| `RH7311…TwoRegionsWithTheSameDescriptionInsideANestedEnumAreNotReported` | RH7311-002, SHARED-TYPEREGION-006 | `Verify` | fails |
| `RH7311…TopLevelRegionsAreComparedAcrossANestedEnumWithARegion` | RH7311-001, RH7311-002 | `Verify` | fails (enum region reported too) |
| `RH7309…RegionsInTheIfAndElseBranchOfOneConditionalDirectiveAreNotCompared` (Q1, `DEBUG` undefined) | RH7309-002, SHARED-TYPEREGION-006 | `Verify` | fails |
| `RH7309…RegionsInTheIfAndElseBranchOfOneConditionalDirectiveAreNotComparedWhereTheSymbolIsDefined` (Q1) | RH7309-002, SHARED-TYPEREGION-006 | `Verify` + `DEBUG` | fails |
| `RH7309…RegionsInTheIfAndElifBranchOfOneConditionalDirectiveAreNotCompared` | RH7309-002 | `Verify`, both configurations | fails |
| `RH7309…RegionInTheCompiledBranchAfterARegionOutsideEveryBranchIsReportedWithoutACodeFix` (Q1b, `DEBUG`) | RH7309-002, RH7309-005 | `VerifyNoCodeFixIsOffered(…, "DEBUG")` | passes |
| `RH7309…RegionInAnExcludedBranchAfterARegionOutsideEveryBranchIsNotReported` (Q1b, undefined) | RH7309-002 | `Verify` | fails |
| `RH7309…RegionInAnExcludedBranchDoesNotCountAsAnEarlierRegion` | RH7309-002 | `Verify` | fails |
| `RH7309…RegionInTheCompiledBranchCountsAsAnEarlierRegion` | RH7309-002 | `VerifyNoCodeFixIsOffered(…, "DEBUG")` | passes (other side of the boundary) |
| `RH7309…NoCodeFixWhenARegionInAnExcludedBranchStandsInFrontOfTheReportedRegions` | RH7309-004, RH7309-005 | `VerifyNoCodeFixIsOffered` | passes |
| `RH7309…VerifyCodeFixReordersRegionsWrappedByTheSameConditionalDirective` *(updated: now defines `DEBUG`)* | RH7309-002, RH7309-005 | `Verify(source, fixed, onConfigure)` | passes |
| `RH7309…RegionOfANestedEnumIsNotComparedWithTheRegionsOfTheType` | RH7309-002, SHARED-TYPEREGION-006 | `Verify` | fails |
| `RH7309…RegionsInsideANestedEnumAreNotReported` (skeptic's safety-rail row: fix swapped enum members) | RH7309-002, SHARED-TYPEREGION-006 | `Verify` | fails |
| `RH7309…CodeFixReordersTheBlocksOfTheTypeAroundANestedEnumWithARegion` (skeptic's safety-rail row: unparsable result) | RH7309-002, RH7309-004, SHARED-TYPEREGION-006 | `Verify(source, fixed)` | fails (enum region reported) |
| `Core…RegionDirectiveUtilitiesTests.GetTopLevelRegionsExcludesTheRegionsOfANestedEnum` | SHARED-TYPEREGION-006 | MSTest | fails (3 pairs instead of 1) |
| `RH7401…VerifyOverrideInNestedRegionIsJudgedByTheInnermostRegion` *(rewritten from `VerifyOverrideInNestedRegionIsJudgedByItsTopLevelRegion`)* | RH7401-001, SHARED-REGGROUP-001, SHARED-TYPEREGION-006 | `Verify` | fails (`N` reported instead of `M`) |
| `RH7401…VerifyOverrideInRegionWithoutDescriptionNestedInItsBaseTypeRegionIsReported` | RH7401-001, SHARED-REGGROUP-001 | `Verify` | fails |
| `RH7401…VerifyOverrideInRegionNestedTwoLevelsDeepIsJudgedByTheInnermostRegion` | RH7401-001, SHARED-REGGROUP-001 | `Verify` | fails |
| `RH7405…VerifyMethodInNestedRegionIsJudgedByTheInnermostRegion` *(rewritten from `VerifyMethodInNestedRegionIsJudgedByItsTopLevelRegion`)* | RH7405-001, RH7405-002, SHARED-REGGROUP-001, SHARED-TYPEREGION-006 | `Verify` | fails |
| `RH7405…VerifyMethodInRegionWithoutDescriptionNestedInOverrideRegionIsNotReported` | RH7405-002 | `Verify` | fails |
| `RH7405…VerifyNestedRegionIsOverrideRegionWhenAnOverrideOutsideItNamesIt` | RH7405-001, SHARED-REGGROUP-006 | `Verify` | fails |
| `RH7409…VerifyImplementationInNestedRegionIsJudgedByTheInnermostRegion` | RH7409-001, SHARED-REGGROUP-001, SHARED-TYPEREGION-006 | `Verify` | fails |

### Scenarios

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| RH7308-001 examples 3 and 4 | LF | analyzer | reproduced | `Value` reported against `Fields` vs `_other` only / nothing |
| RH7311 enum and branch examples | LF | analyzer | reproduced | enum / branch regions compared vs not compared |
| RH7309 Q1, Q1b (undefined), enum rows | LF | analyzer, fix | reproduced | reported vs not reported |
| RH7401/RH7405/RH7409 nested regions | LF | analyzer | reproduced | judged by top-level vs innermost region |

### Not covered, and why

- RH7402–RH7404, RH7406–RH7408, RH7410–RH7412 nested regions: the innermost-region decision sits in two shared call sites — `rg "TryFindContainingRegion" src/Reihitsu.Analyzer` hits only `MembersShouldBeGroupedByRegionsAnalyzerBase.HasMatchingRegion` (RH7401–RH7404, RH7409–RH7412) and `NonOverrideMembersShouldNotBePlacedInOverrideRegionsAnalyzerBase.TryGetContainingRegionName` (RH7405–RH7408), besides RH7308. Covered once per base via RH7401, RH7409 (interface name source) and RH7405.
- RH7305, RH7305A, RH7103, RH7110 (helper consumers): skeptic shows no verdict changes; the Core helper test pins the helper.
- LF/CRLF: the change is analyzer verdicts and a block reorder that moves whole lines; RH7309 already has CRLF tests (RH7309-007) on the same move code. No separate CRLF variant added.
- FC-019/020 (directives in gaps): the branch tests are the directive cases; comments do not decide region ownership.

## thoenissen/Reihitsu#1333

### Tests

Every issue input, also with the declarators in the opposite order (issue "Done when").

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH7403…VerifyEachDeclaratorOfFieldLikeOverrideEventIsJudgedOnItsOwn` *(rewritten from `…JudgedByItsFirstDeclarator`)* | RH7403-001, SHARED-REGGROUP-005 | `Verify` | fails |
| `RH7403…VerifyEachDeclaratorOfFieldLikeOverrideEventIsJudgedOnItsOwnInReversedOrder` | RH7403-001, SHARED-REGGROUP-005 | `Verify` | passes |
| `RH7407…VerifyEveryDeclaratorOfOverrideEventNamesOverrideRegion` *(rewritten from `VerifyOnlyFirstDeclaratorOfOverrideEventNamesOverrideRegion`)* | RH7407-001, SHARED-REGGROUP-006 | `Verify` | fails |
| `RH7407…VerifyEveryDeclaratorOfOverrideEventNamesOverrideRegionInReversedOrder` | RH7407-001, SHARED-REGGROUP-006 | `Verify` | fails |
| `RH7407…VerifyEventWithSeveralDeclaratorsInOverrideRegionIsReportedOncePerDeclarator` *(rewritten from `…ReportedOnceAtFirstDeclarator`)* | RH7407-001, SHARED-REGGROUP-005 | `Verify` | fails |
| `RH7407…VerifyEachDeclaratorOfPartialFieldLikeEventInOverrideRegionIsReportedOnItsDefiningPart` (skeptic) | RH7407-001, SHARED-REGGROUP-005, SHARED-DIAG-012 | `Verify` | fails |
| `RH7411…VerifyEachDeclaratorOfFieldLikeImplementationEventIsJudgedOnItsOwn` *(rewritten from `…JudgedByItsFirstDeclarator`)* | RH7411-001, SHARED-REGGROUP-005 | `Verify` | fails |
| `RH7411…VerifyEachDeclaratorOfFieldLikeImplementationEventIsJudgedOnItsOwnInReversedOrder` | RH7411-001, SHARED-REGGROUP-005 | `Verify` | passes |
| `RH7411…VerifySpecificationExampleOfDeclaratorsJudgedEachOnTheirOwn` | RH7411-001, SHARED-REGGROUP-005 | `Verify` | fails |
| `RH7411…VerifyFieldLikeImplementationEventWithSeveralDeclaratorsIsReportedOncePerDeclarator` *(rewritten from `…ReportedOnceAtFirstDeclarator`)* | RH7411-001, SHARED-REGGROUP-005 | `Verify` | fails |
| `RH7411…VerifyFieldLikeImplementationEventWithSeveralDeclaratorsIsReportedOncePerDeclaratorInReversedOrder` | RH7411-001, SHARED-REGGROUP-005 | `Verify` | fails |
| `RH7411…VerifyEachDeclaratorOfPartialFieldLikeEventIsReportedOnItsDefiningPart` (skeptic) | RH7411-001, SHARED-REGGROUP-005, SHARED-DIAG-012 | `Verify` | fails |

### Scenarios

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| issue inputs 1–5 | LF | analyzer | reproduced | first declarator only vs every declarator |

### Not covered, and why

- Three declarators (middle one differing): the per-declarator loop has no position-dependent branch beyond "first vs other"; first and last are covered in both orders. If the implementer adds an index-specific condition, a middle-declarator test is owed.
- No code fix exists; CRLF and fix classes do not apply.

## thoenissen/Reihitsu#1334

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH7405…VerifyOverrideOfOtherMemberKindMakesOverrideRegion` *(rewritten from `…DoesNotMakeMethodOverrideRegion`; Q1)* | RH7405-001, SHARED-REGGROUP-006 | `Verify` | fails |
| `RH7406…VerifyOverrideOfOtherMemberKindMakesPropertyOverrideRegion` *(rewritten from `…DoesNotMakePropertyOverrideRegion`)* | RH7406-001, SHARED-REGGROUP-006 | `Verify` | fails |
| `RH7407…VerifyOverrideOfOtherMemberKindMakesEventOverrideRegion` | RH7407-001, SHARED-REGGROUP-006 | `Verify` | fails |
| `RH7408…VerifyOverrideOfOtherMemberKindMakesIndexerOverrideRegion` | RH7408-001, SHARED-REGGROUP-006 | `Verify` | fails |
| `RH7405…VerifyOverrideInOtherPartialDeclarationMakesOverrideRegion` *(rewritten from `…DoesNotMakeOverrideRegion`; Q4)* | RH7405-001, SHARED-REGGROUP-006 | `Verify` | fails |
| `RH7405…VerifyOverrideInOtherPartialDeclarationInAnotherFileMakesOverrideRegion` (Q4, two files) | RH7405-001, SHARED-REGGROUP-006 | `Verify` + second source | fails |
| `RH7409…VerifyMethodImplementingTwoInterfacesInTheRegionOfEitherIsNotReportedInAnyBaseListOrder` *(rewritten from `…JudgedByTheFirstInBaseListOrder`; Q2, SHARED-DIAG-009 example)* | RH7409-001, SHARED-REGGROUP-004, SHARED-DIAG-009 | `Verify` | fails |
| `RH7409…VerifyMethodImplementingTwoInterfacesInTheRegionOfNeitherIsReportedInAnyBaseListOrder` | RH7409-001, SHARED-REGGROUP-004 | `Verify` | passes (other side) |
| `RH7409…VerifyPartialImplementationMethodWithOnlyOnePartInItsInterfaceRegionIsReportedOnTheOtherPart` *(rewritten from `VerifyPartialImplementationMethodIsJudgedAtItsDefiningPart`; RH7409-001 example, Q6)* | RH7409-001, SHARED-DIAG-004, SHARED-DIAG-012 | `Verify` | fails |
| `RH7409…VerifyPartialImplementationMethodWithOnlyItsDefiningPartInItsInterfaceRegionInAnotherFileIsReportedOnTheImplementingPart` | RH7409-001, SHARED-DIAG-012 | `Verify` + second source | fails |
| `RH7410…VerifyPartialImplementationPropertyWithOnlyOnePartInItsInterfaceRegionIsReportedOnTheOtherPart` *(rewritten from `VerifyPartialImplementationPropertyIsJudgedAtItsDefiningPart`)* | RH7410-001, SHARED-DIAG-004, SHARED-DIAG-012 | `Verify` | fails |
| `RH7401…VerifyPartialMethodWithOnlyItsDefiningPartInAMatchingRegionIsReportedOnItsImplementingPart` (RH7401-001 example) | RH7401-001, SHARED-DIAG-012 | `Verify` | passes |
| `RH7401…VerifyPartialMethodWithOnlyItsDefiningPartInAMatchingRegionInAnotherFileIsReportedOnItsImplementingPart` | RH7401-001, SHARED-DIAG-012 | `Verify` + second source | passes |
| `RH7401…VerifyPartialMethodWithOnlyItsImplementingPartInAMatchingRegionInAnotherFileIsReportedOnItsDefiningPart` | RH7401-001, SHARED-DIAG-012 | `Verify` + second source | passes |
| `RH7405…VerifyPartialMethodWithOnlyItsImplementingPartInOverrideRegionIsReportedOnThatPart` (RH7405-001 example) | RH7405-001, SHARED-DIAG-012 | `Verify` | passes |
| `RH7405…VerifyPartialMethodWithOnlyItsImplementingPartInOverrideRegionInAnotherFileIsReportedOnThatPart` (skeptic's two-file row) | RH7405-001, SHARED-DIAG-012 | `Verify` + second source | fails |
| `RH7405…VerifyPartialMethodWithOnlyItsDefiningPartInOverrideRegionInAnotherFileIsReportedOnThatPart` | RH7405-001, SHARED-DIAG-012 | `Verify` + second source | passes |

### Scenarios

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| Q1, Q2, Q4 (one and two files), Q6 | LF | analyzer | reproduced | as the skeptic observed: nothing / defining part vs the specified part |
| RH7401 partial, two files | LF | analyzer | not reproduced (holds) | kept as tests; the skeptic's "unverified" cell is now settled green |

### Not covered, and why

- RH7402–RH7404, RH7411/RH7412 partial-part choice: one shared selection (`PartialMemberUtilities` via the two bases); RH7401, RH7405, RH7409, RH7410 cover both bases and both interface/override sources.
- Derived-interface case of SHARED-REGGROUP-004: existing tagged tests already pin that a derived interface's name does not match; the "several interfaces" change must keep them green.

## thoenissen/Reihitsu#1339

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH7102…FieldOutsideEveryRegionAndConstantInARegionAreNotCompared` *(rewritten from `NoCodeFixWhenDirectivesAreInLeadingTrivia`, as the issue comment asks)* | RH7102-003 | `Verify` | fails |
| `RH7102…FieldAndConstantInDifferentRegionsAreNotCompared` (RH7102-003 example) | RH7102-003, SHARED-ORDER-004 | `Verify` | fails |
| `RH7102…ConstantOutsideEveryRegionIsNotComparedWithAFieldInARegion` | RH7102-003 | `Verify` | fails |
| `RH7102…CodeFixTargetsTheFirstFieldOfTheGroupInTheConstantsOwnRegion` (RH7102-005 example) | RH7102-005 | `Verify(source, fixed)` | fails (fix not offered: target crosses a region) |
| `RH7109…ReadonlyInterfaceFieldInARegionAfterAMutableFieldOutsideEveryRegionIsNotReported` *(rewritten from `MixedAccessibilityInterfaceViolationWithDirectivesHasNoCodeFix`)* | RH7109-003, SHARED-ORDER-004 | `Verify` | fails |
| `RH7109…FieldOutsideEveryRegionAndReadonlyFieldInARegionAreNotCompared` *(rewritten from `NoCodeFixWhenDirectivesAreInLeadingTrivia`)* | RH7109-003 | `Verify` | fails |
| `RH7109…FieldAndReadonlyFieldInDifferentRegionsAreNotCompared` (RH7109-003 example) | RH7109-003, SHARED-ORDER-004 | `Verify` | fails |
| `RH7109…ReadonlyFieldOutsideEveryRegionIsNotComparedWithAFieldInARegion` | RH7109-003 | `Verify` | fails |
| `RH7109…CodeFixTargetsTheFirstFieldOfTheGroupInTheReadonlyFieldsOwnRegion` (RH7109-005 example) | RH7109-005 | `Verify(source, fixed)` | fails |

The other side of the boundary (same top-level region, one of the two in a nested region: reported) is already pinned by `ConstantInANestedRegionOfTheSameTopLevelRegionIsReported` and `ReadonlyFieldInANestedRegionOfTheSameTopLevelRegionIsReported`, which pass and must stay green.

### Scenarios

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| issue input, #1664 input, RH7109 input | LF | analyzer | reproduced | reported vs not reported |
| skeptic's fix-target input | LF | code fix | reproduced | no fix vs `K`/`_c` moved in front of `_b` |

### Not covered, and why

- CRLF for the fix: the move code (SHARED-ORDER-001/002) is unchanged and has its own CRLF tests; only the target choice changes.

## thoenissen/Reihitsu#1337

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH7101…FieldDeclarationWithAConditionalDeclaratorBetweenItsDeclaratorsIsNotReported` (issue input) | RH7101-001, SHARED-DIAG-008 | `Verify`, both configurations | fails |
| `RH7101…FieldDeclarationWithAnEmptyConditionalDirectivePairBetweenItsDeclaratorsIsNotReported` | RH7101-001 | `Verify` | fails |
| `RH7101…FieldDeclarationWithAConditionalDirectiveInTheInitializerOfANonLastDeclaratorIsNotReported` (skeptic's matrix) | RH7101-001 | `Verify` | fails |
| `RH7101…FieldDeclarationWithAConditionalDirectiveInTheInitializerOfItsLastDeclaratorIsReported` | RH7101-001 | `Verify` | passes (other side) |
| `RH7105…MisorderedModifiersSplitByAConditionalDirectiveAreNotReported` *(rewritten from `MisorderedModifiersWithDirectiveAreReportedWithoutFix`)* | RH7105-001, SHARED-DIAG-008 | `Verify`, both configurations | fails |
| `RH7105…ConditionalDirectiveBehindTheLastModifierDoesNotKeepTheDeclarationFromBeingReported` (RH7105-001 example, Q3-B) | RH7105-001, SHARED-MODORDER-003 | `VerifyNoCodeFixIsOffered` | fails (`Value` reported too) |
| `RH7105…ConditionalDirectiveThatStandsBetweenModifiersWhereTheSymbolIsDefinedKeepsTheDeclarationFromBeingReported` (spec author's note) | RH7105-001 | `Verify` + `DEBUG` | fails |
| `RH7105…ConditionalDirectiveThatSelectsTheTypeBehindTheLastModifierDoesNotKeepTheDeclarationFromBeingReported` (skeptic's second Q3 input) | RH7105-001, SHARED-MODORDER-003 | `VerifyNoCodeFixIsOffered` | passes |
| `RH7106…MisorderedCompoundAccessModifiersSplitByAConditionalDirectiveAreNotReported` *(rewritten from `…WithDirectiveAreReportedWithoutFix`)* | RH7106-001, SHARED-DIAG-008 | `Verify`, both configurations | fails |
| `RH7106…ConditionalDirectiveBehindTheLastModifierDoesNotKeepTheDeclarationFromBeingReported` (RH7106-001 example) | RH7106-001, SHARED-MODORDER-003 | `VerifyNoCodeFixIsOffered` | fails |
| `RH7106…ConditionalDirectiveThatStandsBetweenModifiersWhereTheSymbolIsDefinedKeepsTheDeclarationFromBeingReported` | RH7106-001 | `Verify` + `DEBUG` | fails |
| `RH7107…ConditionalDirectiveAroundTheSetAccessorKeepsTheGetAccessorFromBeingReported` *(rewritten from `NoCodeFixWhenDirectivesAreInAccessorLeadingTrivia`)* | RH7107-001, SHARED-DIAG-008 | `Verify`, both configurations | fails |
| `RH7107…ConditionalDirectiveBehindTheAttributeListOfTheGetAccessorKeepsItFromBeingReported` *(rewritten from `NoCodeFixWhenDirectiveFollowsAccessorAttributeList`)* | RH7107-001, SHARED-DIAG-008 | `Verify` + `DEBUG` | fails |
| `RH7107…ConditionalDirectiveBetweenTheAccessorsButNotInsideABodyKeepsTheGetAccessorFromBeingReported` (RH7107-001 example) | RH7107-001 | `Verify` | fails (`A` reported too) |
| `RH7107…EmptyConditionalDirectivePairBetweenTheAccessorsKeepsTheGetAccessorFromBeingReported` (skeptic: today reported and fixed) | RH7107-001, SHARED-DIAG-008 | `Verify` | fails |
| `RH7108…ConditionalDirectiveAroundTheRemoveAccessorKeepsTheAddAccessorFromBeingReported` *(rewritten from `NoCodeFixWhenDirectivesAreInAccessorLeadingTrivia`)* | RH7108-001, SHARED-DIAG-008 | `Verify` + `DEBUG` | fails |
| `RH7108…ConditionalDirectiveBehindTheAttributeListOfTheAddAccessorKeepsItFromBeingReported` *(rewritten from `NoCodeFixWhenDirectiveFollowsAccessorAttributeList`)* | RH7108-001, SHARED-DIAG-008 | `Verify` + `DEBUG` | fails |
| `RH7108…ConditionalDirectiveBetweenTheAccessorsButNotInsideABodyKeepsTheAddAccessorFromBeingReported` (RH7108-001 example) | RH7108-001 | `Verify` | fails |
| `RH7108…EmptyConditionalDirectivePairBetweenTheAccessorsKeepsTheAddAccessorFromBeingReported` | RH7108-001, SHARED-DIAG-008 | `Verify` | fails |
| `RH5102…VerifyConditionalDirectiveBetweenArgumentsKeepsTheListFromBeingReported` *(rewritten from `VerifyDirectiveInsideArgumentListWithholdsTheCodeFix`)* | RH5102-004, SHARED-DIAG-008 | `Verify` | fails |
| `RH5102…VerifyConditionalDirectiveInAGapKeepsTheListFromBeingReportedButNotOneInsideAnArgument` (RH5102-004 example) | RH5102-004 | `VerifyNoCodeFixIsOffered` | fails (first call reported too) |
| `RH5102…VerifyConditionalDirectiveWithAlternativesInFrontOfTheFirstArgumentKeepsTheListFromBeingReported` | RH5102-004 | `Verify` | fails |
| `RH5109…VerifyConditionalDirectiveBetweenParametersKeepsTheListFromBeingReported` *(rewritten from `VerifyDirectiveInsideParameterListWithholdsTheCodeFix`)* | RH5109-004, SHARED-DIAG-008 | `Verify` | fails |
| `RH5109…VerifyConditionalDirectiveInAGapKeepsTheListFromBeingReportedButNotOneInsideAParameter` (RH5109-004 example) | RH5109-004 | `VerifyNoCodeFixIsOffered` | fails |
| `RH5111…VerifyDeclaratorWithConditionalDirectiveInFrontOfItsEqualsSignIsNotReported` *(rewritten from `VerifyDeclaratorWithDirectiveIsReportedWithoutCodeFix`)* | RH5111-005, SHARED-DIAG-008 | `Verify` | fails |
| `RH5111…VerifyConditionalDirectiveInTheJudgedSpanKeepsTheDeclaratorFromBeingReportedButNotOneInsideTheValue` (RH5111-005 example, Q4-A) | RH5111-005 | `VerifyNoCodeFixIsOffered` | fails (`value` reported too) |
| `RH5111…VerifyDeclaratorWithConditionalDirectiveBehindItsEqualsSignIsNotReported` | RH5111-005 | `Verify` | fails |

### Scenarios

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| issue inputs RH7101, RH7105/RH7106, RH7107/RH7108 | LF | analyzer | reproduced | reported (no fix) vs not reported |
| RH7107 issue input with `#if DEBUG` around `set` | LF | analyzer | reproduced only with `DEBUG` defined or `#if !NEVER` (skeptic's correction); both forms are in the tests | |

### Not covered, and why

- Non-conditional directives (`#pragma`, `#region`) in the judged spans: must stay "reported, no fix"; already pinned by existing tagged tests of each rule (e.g. RH7309/RH5102 pragma cases, RH7105/RH7106 directive-withholds-fix tests), plus the `#pragma` lines in the RH5102/RH5109 examples above.
- `#elif` arm per rule: the conditional-directive check is one predicate per rule over directive kinds; `#if`/`#else` and `#if`/`#elif` arms are covered for RH7309/RH7311, `#else` for RH5102 and RH7105. If the implementer filters kinds separately, an `#elif` test per rule is owed.
- The P11 formatter note (formatter splits the now-unreported argument list) is intended (skeptic); no test.
- RH7101 non-last-declarator initializer: derived from the skeptic's reading of RH7101-001's unchanged "between its declarators"; flagged for the spec checker in case the reading is disputed.

## thoenissen/Reihitsu#1412

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH7106…BothPartsOfAPartialConstructorAreReported` (issue input, with fix) | RH7106-001, SHARED-DIAG-004 | `Verify(source, fixed)` | fails (defining part not reported) |
| `RH7106…BothPartsOfAPartialConstructorOfARecordAreReported` | RH7106-001, SHARED-DIAG-004 | `Verify` | fails |
| `RH7106…DefiningPartOfAPartialConstructorIsReportedWhenTheImplementingPartIsInAnotherFile` | RH7106-001, SHARED-DIAG-004 | `Verify` + second source | fails |
| `RH7106…DefiningPartOfAPartialConstructorIsReportedWhenTheImplementingPartIsGenerated` (`C.g.cs`) | RH7106-001, SHARED-DIAG-004 | `Verify` + generated source | fails |
| `RH7105…BothPartsOfAPartialConstructorAreReported` (`unsafe public`) | RH7105-001, SHARED-DIAG-004 | `Verify` + unsafe | fails |
| `RH7105…DefiningPartOfAPartialConstructorIsReportedWhenTheImplementingPartIsInAnotherFile` | RH7105-001, SHARED-DIAG-004 | `Verify` + second source | fails |
| `RH7105…DefiningPartOfAPartialConstructorIsReportedWhenTheImplementingPartIsGenerated` | RH7105-001, SHARED-DIAG-004 | `Verify` + generated source | fails |

### Scenarios

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| issue input RH7106 | LF | analyzer + fix | reproduced | implementing part only vs both parts |
| RH7105 counterpart (`unsafe public partial C();`) | LF | analyzer | reproduced | |

### Not covered, and why

- The struct arm was replaced by a record: a `protected` constructor in a struct does not compile (CS0666). RH7105 has no access-pair input for a constructor, so it uses `unsafe public`.
- Other constructor rules (skeptic's follow-up): out of scope (lead decision 10).

## thoenissen/Reihitsu#1592

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH7309…CodeFixReordersTheTypeWhenTheReportedBlockStartsWithANestedClass` (issue shape 1) | RH7309-004, SHARED-TYPEREGION-006 | `Verify(source, fixed)` | fails (no fix applied) |
| `RH7309…CodeFixReordersTheTypeWhenTheReportedBlockStartsWithAnAttributedNestedRecord` | RH7309-004 | `Verify(source, fixed)` | fails |
| `RH7309…CodeFixReordersTheTypeWhenTheReportedBlockStartsWithANestedClassWhoseRegionsAreInOrder` (shape 2, offered but no progress today) | RH7309-004, SHARED-FIX-005, SHARED-TYPEREGION-006 | `Verify(source, fixed)` | fails |
| `RH7309…CodeFixForTheOuterTypeLeavesTheRegionsOfANestedClassAtTheStartOfTheReportedBlockInPlace` (nested regions out of order; fix applied at the outer diagnostic) | RH7309-004, SHARED-TYPEREGION-006 | `TryApplyCodeFixAtAsync` | fails (outer type unchanged) |

### Scenarios

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| issue shape 1 | LF | code fix, Fix All (via `Verify`) | reproduced | no fix vs blocks reordered |
| shape 2 via a nested type in order | LF | code fix | reproduced | no-op fix vs outer reordered |

### Not covered, and why

- Struct, interface, record struct, extension block as the first member: one call site — `rg "FirstAncestorOrSelf<TypeDeclarationSyntax>" src/Reihitsu.Analyzer.CodeFixes/Rules/Organization/RH7309*` (one hit in `RegisterCodeFixesCoreAsync`); class and attributed record cover the "trivia in front" arm.
- A separate identity-permutation input: the skeptic shows it has no input apart from shape 1's cause.

## Whole run

- Files: `src/Reihitsu.Analyzer.Test/Formatting/RH5102…`, `RH5109…`, `RH5111…`, `RH7101…`, `RH7308…`, `RH7309…`, `RH7311…`, `RH7401…`, `RH7403…`, `RH7405…`–`RH7411…` AnalyzerTests; `src/Reihitsu.Analyzer.Test/Ordering/RH7102…`, `RH7105…`–`RH7109…` AnalyzerTests; `src/Reihitsu.Core.Test/RegionDirectiveUtilitiesTests.cs`. A private `DefineDebug` solution transform was added to the test classes that need `DEBUG` (pattern from `RH7004…Tests`). Formatted with `scripts/format.sh`.
- Full analyzer project on this head: 96 failing, all of them new or rewritten tests listed above (no pre-existing test fails); core: 1 failing (the helper test); architecture: 118/118 green (requirement tags valid).
- Characterization tests whose doc said "pins today's behavior … change belongs to an existing follow-up" for these issues were all rewritten; none of them remains.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH7308|FullyQualifiedName~RH7309|FullyQualifiedName~RH7311"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH740|FullyQualifiedName~RH741"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH7102|FullyQualifiedName~RH7109"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH7101|FullyQualifiedName~RH7105|FullyQualifiedName~RH7106|FullyQualifiedName~RH7107|FullyQualifiedName~RH7108|FullyQualifiedName~RH5102|FullyQualifiedName~RH5109|FullyQualifiedName~RH5111"
scripts/test.sh --project core --filter "FullyQualifiedName~RegionDirectiveUtilitiesTests"
```

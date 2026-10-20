Role: test implementer
Commit: d5e15a18778bb9f63540d6683aeb1d6e1642e096
Verdict: tests written

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Lane: specs and tests for existing behavior. No production code changed. `OrderingDeclarationUtilitiesTests.cs` and `RegionDirectiveUtilitiesTests.cs` are untouched. Every test passes on the unchanged code. No spec statement turned out wrong against the code: every spec example, run as a test, produced exactly its markup and its after-block.

## Tests

All files are under `src/Reihitsu.Analyzer.Test/Formatting/`. "Tagged" means an existing test that got `[Requirement]` attributes and no other change. "Char." means a characterization test with no tag: it pins today's behavior for an input the specs leave out, and its summary says so. Helpers: `Verify(source)` is the markup analyzer verifier. `Verify(source, fixed)` is the code-fix verifier, which checks one-pass convergence, incremental iterations, and Fix All. `ApplyCodeFixAsync` returns the raw text after the first offered action, with no line-ending normalization. Every test is expected to pass now.

### RH7401–RH7404 (override members)

| Test | Requirement / failure class | Helper |
|---|---|---|
| RH7401 tagged: `VerifyDiagnosticForOverrideMethodImplementingInterfaceInInterfaceRegion`, `VerifyNoDiagnosticsForOverrideMethodInMatchingRegion`, `VerifyDiagnosticForOverrideMethodInGenericRegion`, `VerifyDiagnosticForRegionInsideOverrideMethodBody` | RH7401-001 | Verify |
| RH7401 tagged: `VerifyDiagnosticsForOverrideMethodsFromDifferentBaseTypes` | RH7401-001, SHARED-REGGROUP-003 | Verify |
| RH7401 tagged: `VerifyNoDiagnosticsForOverrideProperty` | RH7401-002 | Verify |
| `VerifyDiagnosticsForOverrideMethodsOutsideTheirBaseTypeRegion` (spec example) | RH7401-001 | Verify |
| `VerifyNoDiagnosticsForMethodThatOverridesNothingOrOverrideOfOtherKind` (spec example) | RH7401-002 | Verify |
| `VerifyNestedTypeAndEachPartialDeclarationAreCheckedAgainstTheirOwnRegions` (spec example) | SHARED-REGGROUP-001, FC-027, FC-024 | Verify |
| `VerifyRecordAndRecordStructDeclarationsAreChecked` | SHARED-REGGROUP-001 | Verify |
| `VerifyRegionNameMatchesOnlyTheSimpleNameOfAGenericBaseType` (spec example) | SHARED-REGGROUP-002 | Verify |
| `VerifyRegionNameMatchesTheValueOfAVerbatimTypeName` (spec example) | SHARED-REGGROUP-002, FC-023 | Verify |
| `VerifyOverrideComesFromTheTypeThatIntroducedTheMember` (spec example) | SHARED-REGGROUP-003 | Verify |
| `VerifySealedAndAbstractOverridesComeFromTheTypeThatIntroducedTheMember` | SHARED-REGGROUP-003 | Verify |
| `VerifyOverrideInNestedRegionIsJudgedByItsTopLevelRegion` | Char. P17 instance (nested region): `N` reported, `M` not | Verify |
| `VerifyRegionNamedAfterLowerCaseTypeMatchesOnlyTheExactName` | Char. Q5, FC-013 (analyzer side): `#region myBase` accepted, `#region MyBase` reported | Verify |
| `VerifyPartialOverrideMethodIsReportedOncePerPart` | Char. Q6 input 1, FC-024: two diagnostics, message `B` | Verify |
| RH7402 tagged: `…ImplementingInterfaceInInterfaceRegion`, `…InMatchingRegion`, `…OutsideRegion` | RH7402-001 | Verify |
| RH7402 tagged: `VerifyDiagnosticsForOverridePropertiesFromDifferentBaseTypes` | RH7402-001, SHARED-REGGROUP-003 | Verify |
| RH7402 tagged: `VerifyNoDiagnosticsForOverrideMethod` | RH7402-002 | Verify |
| `VerifyDiagnosticsForOverridePropertiesOutsideTheirBaseTypeRegion`, `VerifyNoDiagnosticsForPropertyThatOverridesNothingOrOverrideOfOtherKind` (spec examples) | RH7402-001, RH7402-002 | Verify |
| RH7403 tagged: `…ImplementingInterfaceInInterfaceRegion`, `VerifyNoDiagnosticsForExplicitOverrideEventInMatchingRegion` | RH7403-001 | Verify |
| RH7403 tagged: `VerifyDiagnosticForFieldLikeOverrideEventInGenericRegion` | RH7403-001, SHARED-REGGROUP-005 | Verify |
| RH7403 tagged: `VerifyDiagnosticsForOverrideEventsFromDifferentBaseTypes` | RH7403-001, SHARED-REGGROUP-003 | Verify |
| RH7403 tagged: `VerifyNoDiagnosticsForOverrideMethod` | RH7403-002 | Verify |
| `VerifyDiagnosticsForOverrideEventsOutsideTheirBaseTypeRegion`, `VerifyNoDiagnosticsForFieldLikeOverrideEventInMatchingRegion`, `VerifyNoDiagnosticsForEventThatOverridesNothingOrOverrideOfOtherKind` (spec examples) | RH7403-001, RH7403-001, RH7403-002 | Verify |
| `VerifyFieldLikeOverrideEventWithSeveralDeclaratorsIsJudgedByItsFirstDeclarator` | Char. Q3 input 1: not reported | Verify |
| RH7404 tagged: `…ImplementingInterfaceInInterfaceRegion`, `…InMatchingRegion` | RH7404-001 | Verify |
| RH7404 tagged: `VerifyDiagnosticForOverrideIndexerInGenericRegion` | RH7404-001, SHARED-REGGROUP-005 | Verify |
| RH7404 tagged: `VerifyDiagnosticsForOverrideIndexersFromDifferentBaseTypes` | RH7404-001, SHARED-REGGROUP-003 | Verify |
| RH7404 tagged: `VerifyNoDiagnosticsForOverrideProperty` | RH7404-002 | Verify |
| `VerifyDiagnosticsForOverrideIndexersOutsideTheirBaseTypeRegion`, `VerifyNoDiagnosticsForIndexerThatOverridesNothingOrOverrideOfOtherKind` (spec examples) | RH7404-001, RH7404-002 | Verify |

### RH7405–RH7408 (non-override members in override regions)

| Test | Requirement / failure class | Helper |
|---|---|---|
| RH7405–RH7408 tagged: `VerifyDiagnosticForNonOverride{Method,Property,Event,Indexer}InOverrideRegion` | RH74xx-001 (RH7407 and RH7408 also SHARED-REGGROUP-005) | Verify |
| RH7405–RH7408 tagged: `VerifyNoDiagnosticsForMixedOverrideKindsInSameRegion`, `VerifyNoDiagnosticsForNonOverride…InGenericRegion` | RH74xx-002 | Verify |
| RH7405–RH7408 tagged: `VerifyNoDiagnosticsForBaseTypeNamedRegionWithoutOverride…` | RH74xx-002, SHARED-REGGROUP-006 (negative sentence) | Verify |
| `VerifyDiagnosticsForStaticNewAndInterfaceMethodsInOverrideRegion`, `VerifyNoDiagnosticsForConstructorOperatorAndMethodInOrdinaryRegion` (spec examples) | RH7405-001, RH7405-002 | Verify |
| `VerifyRegionIsOverrideRegionWhenOverrideLiesOutsideIt` (spec example) | SHARED-REGGROUP-006, RH7405-001 | Verify |
| `VerifyOverrideOfOtherMemberKindDoesNotMakeMethodOverrideRegion` | Char. Q1: `N` not reported | Verify |
| `VerifyOverrideInOtherPartialDeclarationDoesNotMakeOverrideRegion` | Char. Q4, FC-024: `N` not reported | Verify |
| `VerifyMethodInNestedRegionIsJudgedByItsTopLevelRegion` | Char. P17 instance: helper in `B` > `Helpers` reported, helper in `Helpers2` > `B` not | Verify |
| `VerifyDiagnosticForPropertyOfOwnInOverrideRegion`, `VerifyNoDiagnosticsForOverridePropertyAndPropertyInOrdinaryRegion` (spec examples) | RH7406-001, RH7406-002 | Verify |
| `VerifyDiagnosticsForStaticNewAndInterfacePropertiesInOverrideRegion` | RH7406-001 | Verify |
| `VerifyOverrideOfOtherMemberKindDoesNotMakePropertyOverrideRegion` | Char. Q1 (RH7406 side): `Q` not reported | Verify |
| `VerifyDiagnosticsForFieldLikeAndAccessorEventsOfOwnInOverrideRegion`, `VerifyNoDiagnosticsForOverrideEventAndEventInOrdinaryRegion` (spec examples) | RH7407-001, RH7407-002 | Verify |
| `VerifyAccessorOverrideEventMakesOverrideRegion` | RH7407-001 (accessor override names an override region) | Verify |
| `VerifyOnlyFirstDeclaratorOfOverrideEventNamesOverrideRegion` | Char. Q3 input 3: `F` not reported | Verify |
| `VerifyEventWithSeveralDeclaratorsInOverrideRegionIsReportedOnceAtFirstDeclarator` | Char. Q3 input 4: one diagnostic at `F`, message `B` | Verify |
| `VerifyDiagnosticForIndexerOfOwnInOverrideRegion`, `VerifyNoDiagnosticsForOverrideIndexerAndIndexerInOrdinaryRegion` (spec examples) | RH7408-001, RH7408-002 | Verify |

### RH7409–RH7412 (interface implementations)

| Test | Requirement / failure class | Helper |
|---|---|---|
| RH7409–RH7412 tagged: `VerifyNoDiagnosticsForImplicitImplementationInMatchingRegion`, `VerifyDiagnosticForImplicitImplementationInGenericRegion` (RH7411 and RH7412 also SHARED-REGGROUP-005), `VerifyNoDiagnosticsForExplicitImplementationInMatchingRegion`, `VerifyDiagnosticForImplementationOutsideRegion`, `VerifyDiagnosticsForMembersFromDifferentInterfaces` | RH74xx-001 | Verify |
| RH7409–RH7412 tagged: `VerifyDiagnosticForExplicitImplementationInGenericRegion` | RH74xx-001, SHARED-REGGROUP-005 | Verify |
| RH7409–RH7412 tagged: `VerifyNoDiagnosticsForInheritedInterfaceMemberInDeclaringInterfaceRegion`, `VerifyDiagnosticForInheritedInterfaceMemberUsesDeclaringInterface` | RH74xx-001, SHARED-REGGROUP-004 | Verify |
| RH7409–RH7412 tagged: `VerifyNoDiagnosticsForNonInterfaceMember`, `VerifyNoDiagnosticsForMemberOfOtherKind` | RH74xx-002 | Verify |
| RH7409–RH7412 tagged: `VerifyNoDiagnosticsForOverride…ImplementingInterfaceInBaseTypeRegion` (RH7411 also the accessor variant) | RH74xx-002, SHARED-REGGROUP-004 | Verify |
| RH7409–RH7412: `…OutsideTheirInterfaceRegion`, `…InTheirInterfaceRegions`, `VerifyNoDiagnosticsFor…ImplementingNothingAnd…Implementation` (spec examples, three per rule) | RH74xx-001, RH74xx-001, RH74xx-002 | Verify |
| `VerifyNoDiagnosticsForOperatorImplementingStaticAbstractOperator` | RH7409-002, FC-024 | Verify |
| `VerifyRecordEqualsIsGroupedByEquatableInterface` | RH7409-001: typed `Equals` reported, message `IEquatable` | Verify |
| `VerifyInterfaceDeclarationIsNotChecked` (spec example) | SHARED-REGGROUP-001 | Verify |
| `VerifyRegionNameMatchesOnlyTheSimpleNameOfAGenericInterface` (spec example) | SHARED-REGGROUP-002 | Verify |
| `VerifyImplementationComesFromTheInterfaceThatDeclaresTheMember` (spec example) | SHARED-REGGROUP-004 | Verify |
| `VerifyOverrideImplementingInterfaceMethodDoesNotComeFromTheInterface` (spec example) | SHARED-REGGROUP-004, RH7409-002 | Verify |
| `VerifyMethodImplementingTwoInterfacesIsJudgedByTheFirstInBaseListOrder` | Char. Q2: `C : IA, IB` reported with message `IA`, `D : IB, IA` not | Verify |
| `VerifyPartialImplementationMethodIsJudgedAtItsDefiningPart` | Char. Q6 input 2, FC-024: one diagnostic at the defining part of `M`, none for `N`, whose defining part lies in `I` | Verify |
| `VerifyPartialImplementationPropertyIsJudgedAtItsDefiningPart` (RH7410) | Char. Q6 input 2, the same for a partial property | Verify |
| `VerifyDiagnosticForExplicitImplementationIsOnIdentifierAfterInterfaceName` (RH7410), `VerifyDiagnosticForFieldLikeEventIsOnIdentifierOfItsDeclarator` (RH7411), `VerifyDiagnosticForIndexerIsOnThisKeyword` (RH7412) | SHARED-REGGROUP-005. The spec example is split into one test per rule, because each verifier runs one analyzer. | Verify |
| `VerifyFieldLikeImplementationEventWithSeveralDeclaratorsIsJudgedByItsFirstDeclarator` | Char. Q3 input 2: not reported | Verify |
| `VerifyFieldLikeImplementationEventWithSeveralDeclaratorsIsReportedOnceAtFirstDeclarator` | Char. Q3 input 5: one diagnostic at `A`, message `IA` | Verify |

### RH7501

| Test | Requirement / failure class | Helper |
|---|---|---|
| tagged: `VerifyDiagnosticWhenBreakStatementIsInsideExplicitSwitchCaseBlock` | RH7501-001, -003, -004 | Verify(source, fixed) |
| tagged: `VerifyCodeFixPreservesCommentsWhenMovingBreakStatement`, `VerifyCodeFixDoesNotReformatNonCompliantSwitchSectionLayout` | RH7501-004 | Verify(source, fixed) |
| tagged: `VerifyNoDiagnosticWhenBreakStatementFollowsExplicitSwitchCaseBlock` | RH7501-002 | Verify |
| tagged: `VerifyNoDiagnosticWhenBreakStatementBelongsToNestedLoop`, `VerifyNoDiagnosticWhenBreakStatementIsNotLastStatementInExplicitSwitchCaseBlock`, `VerifyNoDiagnosticWhenBlockIsNotLastStatementInSwitchSection` | RH7501-001 | Verify |
| `VerifyDiagnosticsForBreakEndingSectionBlockWhateverStatementsPrecedeIt` (spec example) | RH7501-001, RH7501-003 | Verify |
| `VerifyNoDiagnosticsForBreakThatDoesNotEndTheSectionBlock` (spec example) | RH7501-001 (all four negative bullets) | Verify |
| `VerifyNoDiagnosticsForOtherJumpStatementsEndingSectionBlock` (spec example), `VerifyNoDiagnosticForYieldBreakEndingSectionBlock` (spec example) | RH7501-002 | Verify |
| `VerifyNoDiagnosticsForThrowContinueAndGotoDefaultEndingSectionBlock` | RH7501-002 (`throw`, `continue`, `goto default`, `goto` label, `return`) | Verify |
| `VerifyCodeFixMovesBreakWithItsCommentsBehindBlock` (spec example, two diagnostics) | RH7501-004 | Verify(source, fixed) |
| `VerifyCodeFixMovesCommentInFrontOfBreakOnItsLine` (`/* leave */ break;`) | RH7501-004, FC-021 | Verify(source, fixed) |
| `VerifyCodeFixKeepsCommentBehindClosingBrace` (`} // end of default`) | RH7501-004, FC-021 | Verify(source, fixed) |
| `VerifyCodeFixIndentsBreakLikeClosingBraceWhenOpeningBraceIsOnLabelLine` (`case 1: {`) | RH7501-004, FC-027 | Verify(source, fixed) |
| `VerifyCodeFixKeepsCommentBetweenBreakAndClosingBraceInsideBlock` | RH7501-004, FC-019 (comment in the gap) | Verify(source, fixed) |
| `VerifyCodeFixKeepsCarriageReturnLineFeedLineEndings` | RH7501-004, FC-017, FC-033 (CRLF in, CRLF out, raw text) | ApplyCodeFixAsync |
| `VerifyFixAllMovesBreaksOfNestedSwitchSections` | RH7501-004, FC-008, FC-007 | Verify(source, fixed) |
| `VerifyCodeFixMovesExcludedCodeInFrontOfBreakBehindBlock` | Char. Q7 input 1, FC-020 | ApplyCodeFixAsync |
| `VerifyCodeFixSplitsRegionAroundBreak` | Char. Q7 input 2, FC-020 | ApplyCodeFixAsync |
| `VerifyCodeFixLeavesConditionalCodeBehindBreakInsideBlock` | Char. Q7 input 3, FC-020 | ApplyCodeFixAsync |
| `VerifyCodeFixSeparatesPragmaPairAroundBreak` | Char. Q7 input 4, FC-020 | ApplyCodeFixAsync |
| `VerifyCodeFixOnSectionBlockOnOneLine` | Char. Q8 input 1, FC-026 | ApplyCodeFixAsync |
| `VerifyCodeFixWhenBreakSharesLineWithClosingBrace` | Char. Q8 input 2 | ApplyCodeFixAsync |
| `VerifyCodeFixWhenBreakSharesLineWithStatementInFrontOfIt` | Char. Q8 input 3 | ApplyCodeFixAsync |
| `VerifyCodeFixLeavesEmptyBlockWhenBreakIsOnlyStatement` | Char. Q9 | ApplyCodeFixAsync |
| `VerifyCodeFixMovesBlankLineInFrontOfBreakWithIt` | Char. Q10 | ApplyCodeFixAsync |

Fix All with two diagnostics in one document stays covered by the existing `GetFixAllScenario` (`FixAllCorrectsEveryDiagnostic`, SHARED-FIX-002 by the base class).

Expected values. Spec-example tests use the spec's blocks verbatim, with markup in place of `{|RHxxxx:…|}`. Characterization outputs and the four RH7501 fix edges (`/* leave */`, `} // end`, `case 1: {`, comment after `break`) were captured by running `ApplyCodeFixAsync` on the unchanged code. I captured them with a temporary dump test, which I deleted and did not commit. For the four in-spec edges, I checked each captured output by eye against RH7501-004's text: the `break` is behind the block on a line of its own, indented like the closing brace, comments in front of it move with it, and the rest is unchanged. They all agree. The characterization outputs match the real outputs the spec author recorded for Q7–Q10. One difference in detail: in Q7 input 1 the moved `#if DEBUG` line gets the closing brace's 16-column indentation, while `A(y);`, `#endif`, and `break;` keep their old columns.

## Spec statements against the code

No contradiction found. Each spec example block produced exactly its markup. The RH7501-004 example produced exactly its after-block under the code-fix verifier, with one-pass convergence and Fix All.

## Not covered, and why

- **FC-013, formatter side of Q5.** FMT-REGION-001 rewriting `#region myBase` to `MyBase` is formatter behavior with its own tests. The analyzer side is pinned by `VerifyRegionNamedAfterLowerCaseTypeMatchesOnlyTheExactName`. A `FormatterTestsBase` parity test would assert a disagreement that is an open question.
- **Q7 input 1 with DEBUG defined.** The test pins the fix with DEBUG undefined, as the question states it. The CS0103 in a DEBUG build follows from the pinned text.
- **Q6 input 1 for RH7402 (partial override property).** Partial override methods are pinned for RH7401. RH7402 shares `MembersShouldBeGroupedByRegionsAnalyzerBase.OnTypeDeclaration` with RH7401 but has its own `TryGetExpectedRegionName` override, so this is no static proof. It is left to the question's resolution.
- **SHARED-TYPEREGION-006 and SHARED-REGDESC-001.** These are already covered and tagged by the RH7301, RH7309, and RH7311 tests. They are not re-tagged here, because the RH74xx tests rely on them only incidentally.
- **FMT-SWITCH-002.** This is a formatter spec, and `Uses` does not enforce it. No RH7501 test tags it.
- **FC-001, FC-002, FC-003, FC-015, FC-016.** No guard changes in this lane. Both sides of every spec condition are tested: in and outside a matching region, override and non-override, the first and the second interface, the last and a non-last statement, a section block and a nested block.
- **FC-005, FC-022, FC-025, FC-028, FC-029, FC-036, FC-018.** These do not apply. The analyzers read no literals, the RH7501 fix renames nothing, and the RH7501 fix does not call the formatter.
- **FC-009, FC-030.** Every RH7501 action tested changes the document. The Q8 characterizations pin the cases where the fix rewrites more than the `break`.
- **FC-010, FC-011, FC-012, FC-014.** These do not apply: no formatter change.
- **FC-031, FC-032, FC-034, FC-037.** These do not apply: no production change, and no reproduction.

## Follow-up candidates

_None._

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH74|FullyQualifiedName~RH7501"
scripts/test.sh --project architecture --filter "FullyQualifiedName~RequirementCoverageTests|FullyQualifiedName~SpecConsistencyTests|FullyQualifiedName~SpecStructureTests"
scripts/test.sh --project architecture
```

Results on this tree: analyzer filter 175 passed, 0 failed. Architecture filter 39 passed, including `EveryRequirementOfAnEnforcedSpecIsCovered`. The full architecture project passed 72 of 72. `scripts/format.sh` on the 13 test files formatted 0 of 13.

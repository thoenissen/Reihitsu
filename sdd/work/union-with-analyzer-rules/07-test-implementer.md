Role: test implementer
Commit: d3e4e14
Verdict: tests written

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Expected values come from the spec examples and requirements of the spec diff `0866955..d3e4e14`, the change doc `sdd/changes/2026-11/union-analyzer-rules.md`, and, for fix outputs a union example does not spell out, from the struct or class siblings' existing tests of the same shape (a union follows the struct, Q1/Q2) — never from a patched copy of the code.

## thoenissen/Reihitsu#1767

### Tests

Every union test adds the `UnionSupport` reference (the analyzer tests compile against .NET 9 reference assemblies). "LF+CRLF" tests are `[DataRow]`s over both line endings with `ConvertEndOfLine`. A code-fix test's `Verify` also runs the framework's Fix All passes in document, project, and solution whenever the source holds several diagnostics (RH2004, RH5105, RH5106, RH5108, RH5404, RH5507, RH5508, RH7104, RH7105, RH5418), which covers Fix All over co-occurring union diagnostics; RH5418 has its own `GetFixAllScenario` (A and F of the spec example).

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH2004AccessModifierMustBeDeclaredAnalyzerTests.UnionWithoutAccessibilityIsReportedAndFixed` | RH2004-001, RH2004-006 | Verify + UnionSupport, DataRow LF/CRLF | fail (diagnostic or split missing) |
| `RH2004AccessModifierMustBeDeclaredAnalyzerTests.NestedUnionGetsTheDefaultAccessibilityOfItsContainer` | RH2004-001, RH2004-004, RH2004-006 | Verify + UnionSupport | fail (diagnostic or split missing) |
| `RH2004AccessModifierMustBeDeclaredAnalyzerTests.UnionWithOtherModifiersAttributesAndCommentGetsAccessibilityInFrontOfFirstToken` | RH2004-001, RH2004-006 | Verify + UnionSupport | fail (diagnostic or split missing) |
| `RH2004AccessModifierMustBeDeclaredAnalyzerTests.UnionWithAccessibilityFileModifierOrPartialIsNotReported` | RH2004-001, RH2004-002 | Verify + UnionSupport | pass |
| `RH2101NestedClassesShouldNotBeUsedAnalyzerTests.ClassDeclaredInUnionIsNested` | SHARED-NESTED-001 | Verify + UnionSupport | pass |
| `RH2104NestedStructsShouldNotBeUsedAnalyzerTests.NestedUnionIsNotReportedAsNestedStruct` | — (characterization: change doc Decisions, a union is not a struct for this rule) | Verify + UnionSupport | pass |
| `RH2110NestedUnionsShouldNotBeUsedAnalyzerTests.NestedUnionsAreReportedAsSpecified` | RH2110-001 | Verify + UnionSupport | compile error (rule type missing) |
| `RH2110NestedUnionsShouldNotBeUsedAnalyzerTests.UnionNestedInEveryKindOfTypeIsReported` | RH2110-001, SHARED-NESTED-001 | Verify + UnionSupport | compile error (rule type missing) |
| `RH2110NestedUnionsShouldNotBeUsedAnalyzerTests.TopLevelUnionsAndNestedStructsAreNotReported` | RH2110-001, SHARED-NESTED-001 | Verify + UnionSupport | compile error (rule type missing) |
| `RH2110NestedUnionsShouldNotBeUsedAnalyzerTests.UnionInFileScopedNamespaceIsNotReported` | RH2110-001, SHARED-NESTED-001 | Verify + UnionSupport | compile error (rule type missing) |
| `RH2110NestedUnionsShouldNotBeUsedAnalyzerTests.EachPartOfNestedPartialUnionIsReported` | RH2110-001, SHARED-NESTED-002 | Verify + UnionSupport | compile error (rule type missing) |
| `RH5418EmptyUnionsShouldUseSemicolonDeclarationsFormatterTests.VerifyFormatterFixesEmptyUnion` | FMT-STRUCT-017, RH5418-001 | VerifyFormatter (LF+CRLF, 2 passes) | compile error (rule type missing) |
| `RH5508StructAttributeListsMustFollowShapeRulesFormatterTests.VerifyFormatterSplitsUnionAttributeList` | FMT-STRUCT-028, RH5508-001 | VerifyFormatter (LF+CRLF, 2 passes) | fail (diagnostic or split missing) |
| `RH5035CommentsMustNotBePlacedInsideParameterListAnalyzerTests.CommentInsideUnionCaseTypeListIsReported` | RH5035-001 | Verify + UnionSupport | pass |
| `RH5038CommentsMustNotBePlacedInsideArgumentListAnalyzerTests.CommentInsideWithElementArgumentListIsReported` | RH5038-001 | Verify | pass |
| `RH5101FirstArgumentShouldBeOnSameLineAnalyzerTests.WithElementFirstArgumentBelowParenthesisIsReportedAndJoined` | RH5101-001 | Verify | pass |
| `RH5102ArgumentsShouldBeOnSingleOrSeparateLinesAnalyzerTests.WithElementArgumentListIsReported` | RH5102-001 | Verify | pass |
| `RH5105OpeningParenthesisMustBeOnDeclarationLineAnalyzerTests.UnionCaseTypeListOpeningParenthesisBelowNameIsReportedAndJoined` | RH5105-001, RH5105-002, RH5105-003 | Verify + UnionSupport, DataRow LF/CRLF | fail (diagnostic or split missing) |
| `RH5105OpeningParenthesisMustBeOnDeclarationLineAnalyzerTests.UnionCaseTypeListOpeningParenthesisBehindCommentOrDirectiveIsNotReported` | RH5105-001 | Verify + UnionSupport | pass |
| `RH5106ClosingParenthesisMustBeOnLineOfLastArgumentAnalyzerTests.UnionCaseTypeListClosingParenthesisBelowLastCaseTypeIsReportedAndJoined` | RH5106-001, RH5106-002, RH5106-003 | Verify + UnionSupport, DataRow LF/CRLF | fail (diagnostic or split missing) |
| `RH5106ClosingParenthesisMustBeOnLineOfLastArgumentAnalyzerTests.UnionCaseTypeListClosingParenthesisBehindCommentOrDirectiveIsNotReported` | RH5106-001 | Verify + UnionSupport | pass |
| `RH5107CommaMustBeOnSameLineAsPreviousParameterAnalyzerTests.UnionCaseTypeListCommaBelowCaseTypeIsReported` | RH5107-001 | Verify + UnionSupport | pass |
| `RH5108ParameterListMustFollowDeclarationAnalyzerTests.UnionFirstCaseTypeBelowOpeningParenthesisIsReportedAndJoined` | RH5108-001, RH5108-002, RH5108-003 | Verify + UnionSupport, DataRow LF/CRLF | fail (diagnostic or split missing) |
| `RH5108ParameterListMustFollowDeclarationAnalyzerTests.UnionFirstCaseTypeBehindCommentOrDirectiveIsNotReported` | RH5108-001 | Verify + UnionSupport | pass |
| `RH5109ParametersMustBeOnSameLineOrSeparateLinesAnalyzerTests.UnionCaseTypeListWithCaseTypesSharingALineIsReported` | RH5109-001 | Verify + UnionSupport | pass |
| `RH5110GenericTypeConstraintsShouldBeOnTheirOwnLineAnalyzerTests.UnionConstraintClauseOnDeclarationLineIsReportedAndMoved` | RH5110-001, RH5110-003 | Verify + UnionSupport, DataRow LF/CRLF | fail (diagnostic or split missing) |
| `RH5110GenericTypeConstraintsShouldBeOnTheirOwnLineAnalyzerTests.UnionWithSeveralConstraintClausesIsReportedOnceAndEachClauseMoved` | RH5110-002, RH5110-003 | Verify + UnionSupport | fail (diagnostic or split missing) |
| `RH5110GenericTypeConstraintsShouldBeOnTheirOwnLineAnalyzerTests.NestedUnionConstraintClauseIsReportedAndMoved` | RH5110-001, RH5110-003 | Verify + UnionSupport | fail (diagnostic or split missing) |
| `RH5110GenericTypeConstraintsShouldBeOnTheirOwnLineAnalyzerTests.UnionConstraintClauseColumnIsJudgedBehindComment` | RH5110-001 | Verify + UnionSupport | fail (diagnostic or split missing) |
| `RH5110GenericTypeConstraintsShouldBeOnTheirOwnLineAnalyzerTests.UnionConstraintClauseBehindDirectiveIsNotReported` | RH5110-001 | Verify + UnionSupport | pass |
| `RH5203MultiLineArgumentsShouldBeAlignedAnalyzerTests.WithElementMisalignedArgumentIsReported` | RH5203-001 | Verify | pass |
| `RH5305CollectionExpressionsShouldBeFormattedCorrectlyAnalyzerTests.WithElementCountsAsAnElement` | RH5305-002 | Verify | pass |
| `RH5404ElementMustNotBeOnSingleLineAnalyzerTests.SingleLineUnionIsReportedAndLaidOut` | RH5404-001, RH5404-001, RH5404-002, RH5404-003 | Verify + UnionSupport, DataRow LF/CRLF | fail (diagnostic or split missing) |
| `RH5404ElementMustNotBeOnSingleLineAnalyzerTests.UnionWithoutBodyOrWithBracesOnDifferentLinesIsNotReported` | RH5404-001 | Verify + UnionSupport | pass |
| `RH5404ElementMustNotBeOnSingleLineAnalyzerTests.NestedSingleLineUnionIsReportedAndLaidOut` | RH5404-001, RH5404-003 | Verify + UnionSupport | fail (diagnostic or split missing) |
| `RH5404ElementMustNotBeOnSingleLineAnalyzerTests.FixOfSingleLineUnionSplitsItsAttributeListAsTheFormatterDoes` | RH5404-003 | Verify + UnionSupport | fail (diagnostic or split missing) |
| `RH5413EmptyStructsShouldUseSemicolonDeclarationsAnalyzerTests.EmptyUnionIsNotReportedAsEmptyStruct` | — (characterization: change doc Decisions, a union is not a struct for this rule) | Verify + UnionSupport | pass |
| `RH5418EmptyUnionsShouldUseSemicolonDeclarationsAnalyzerTests.EmptyUnionsAreReportedAndFixedAsSpecified` | RH5418-001, SHARED-EMPTY-001, SHARED-EMPTY-002, SHARED-EMPTY-003, SHARED-EMPTY-004 | Verify + UnionSupport, DataRow LF/CRLF | compile error (rule type missing) |
| `RH5418EmptyUnionsShouldUseSemicolonDeclarationsAnalyzerTests.EmptyStructsAndRecordStructsAreNotReported` | RH5418-001 | Verify + UnionSupport | compile error (rule type missing) |
| `RH5418EmptyUnionsShouldUseSemicolonDeclarationsAnalyzerTests.EmptyUnionNestedInEveryKindOfTypeIsReportedAndFixed` | RH5418-001, SHARED-EMPTY-001, SHARED-EMPTY-003 | Verify + UnionSupport | compile error (rule type missing) |
| `RH5418EmptyUnionsShouldUseSemicolonDeclarationsAnalyzerTests.EmptyPartsOfPartialUnionAreReportedAndFixed` | RH5418-001, SHARED-EMPTY-001, SHARED-EMPTY-003 | Verify + UnionSupport | compile error (rule type missing) |
| `RH5418EmptyUnionsShouldUseSemicolonDeclarationsAnalyzerTests.FileLocalAndConstrainedUnionsAreReportedAndFixed` | RH5418-001, SHARED-EMPTY-003 | Verify + UnionSupport | compile error (rule type missing) |
| `RH5418EmptyUnionsShouldUseSemicolonDeclarationsAnalyzerTests.CommentBehindClosingBraceFollowsTheSemicolon` | SHARED-EMPTY-003 | Verify + UnionSupport | compile error (rule type missing) |
| `RH5418EmptyUnionsShouldUseSemicolonDeclarationsAnalyzerTests.UnionWithCommentThatKeepsTheBracesIsReportedWithoutFix` | SHARED-EMPTY-001, SHARED-EMPTY-004 | Verify + UnionSupport | compile error (rule type missing) |
| `RH5418EmptyUnionsShouldUseSemicolonDeclarationsAnalyzerTests.UnionWithRegionOrPragmaInBodyIsReportedWithoutFix` | SHARED-EMPTY-001, SHARED-EMPTY-004 | Verify + UnionSupport | compile error (rule type missing) |
| `RH5418EmptyUnionsShouldUseSemicolonDeclarationsAnalyzerTests.UnionWithUnsupportedCommentInCaseTypeListIsReportedWithoutFix` | SHARED-FIX-010 | Verify + UnionSupport | compile error (rule type missing) |
| `RH5418EmptyUnionsShouldUseSemicolonDeclarationsAnalyzerTests.UnionWithCommentInCaseTypeListIsFixedAndKeepsTheComment` | SHARED-EMPTY-003 | Verify + UnionSupport | compile error (rule type missing) |
| `RH5418EmptyUnionsShouldUseSemicolonDeclarationsAnalyzerTests.FixSplitsTheAttributeListOfTheUnionAsTheFormatterDoes` | SHARED-EMPTY-003 | Verify + UnionSupport | compile error (rule type missing) |
| `RH5418EmptyUnionsShouldUseSemicolonDeclarationsAnalyzerTests.UnionWithBracesAndSemicolonIsNotReported` | SHARED-EMPTY-001 | Verify + UnionSupport | compile error (rule type missing) |
| `RH5507StructAttributesMustFollowPlacementRulesAnalyzerTests.UnionAttributeListOnDeclarationLineIsReportedAndMoved` | RH5507-001, RH5507-002, RH5507-003 | Verify + UnionSupport, DataRow LF/CRLF | fail (diagnostic or split missing) |
| `RH5508StructAttributeListsMustFollowShapeRulesAnalyzerTests.UnionAttributeListWithSeveralAttributesIsReportedAndSplit` | RH5508-001, RH5508-002, RH5508-003 | Verify + UnionSupport, DataRow LF/CRLF | fail (diagnostic or split missing) |
| `RH5508StructAttributeListsMustFollowShapeRulesAnalyzerTests.UnionAttributeListWithCommentInsideIsReportedWithoutFix` | RH5508-001, RH5508-004 | Verify + UnionSupport | fail (diagnostic or split missing) |
| `RH7305ATypesShouldBeOrganizedWithRegionsForMultipleKindsAnalyzerTests.UnionMixingMemberKindsOutsideRegionsIsReported` | SHARED-TYPEREGION-001, RH7305A-001, RH7305A-002 | Verify + UnionSupport | fail (diagnostic or split missing) |
| `RH7305TypesShouldBeOrganizedWithRegionsAnalyzerTests.UnionWithMemberOutsideRegionsIsReported` | SHARED-TYPEREGION-001, RH7305-001 | Verify + UnionSupport | fail (diagnostic or split missing) |
| `RH7305TypesShouldBeOrganizedWithRegionsAnalyzerTests.UnionWithoutMembersOrOrganizedOrNestedIsNotReported` | SHARED-TYPEREGION-001, SHARED-TYPEREGION-002, RH7305-002 | Verify + UnionSupport | pass |
| `RH7305TypesShouldBeOrganizedWithRegionsAnalyzerTests.EachPartOfPartialUnionIsJudgedOnItsOwn` | SHARED-TYPEREGION-001, RH7305-001 | Verify + UnionSupport | fail (diagnostic or split missing) |
| `RH7308StandardRegionsShouldContainOnlyMatchingMemberKindAnalyzerTests.UnionMemberOfOtherKindInStandardRegionIsReported` | RH7308-001, SHARED-TYPEREGION-006 | Verify + UnionSupport | fail (diagnostic or split missing) |
| `RH7308StandardRegionsShouldContainOnlyMatchingMemberKindAnalyzerTests.NestedUnionMemberIsJudgedByTheUnionsRegion` | RH7308-001, SHARED-TYPEREGION-006 | Verify + UnionSupport | pass |
| `RH7309RegionsShouldFollowCategoryOrderAnalyzerTests.UnionCustomRegionAfterInterfaceRegionIsReportedAndReordered` | RH7309-002, RH7309-003, RH7309-004 | Verify + UnionSupport, DataRow LF/CRLF | fail (diagnostic or split missing) |
| `RH7311RegionDescriptionsMustBeUniqueWithinATypeAnalyzerTests.UnionRepeatedRegionDescriptionIsReported` | RH7311-001, RH7311-003 | Verify + UnionSupport | fail (diagnostic or split missing) |
| `RH7311RegionDescriptionsMustBeUniqueWithinATypeAnalyzerTests.NestedUnionRegionIsNotComparedWithRegionsOfTheClass` | RH7311-002, SHARED-TYPEREGION-006 | Verify + UnionSupport | pass |
| `RH7311RegionDescriptionsMustBeUniqueWithinATypeAnalyzerTests.RegionsOfTwoPartsOfPartialUnionAreNotCompared` | RH7311-002 | Verify + UnionSupport | pass |
| `RH7401OverrideMethodsShouldBeGroupedByBaseTypeRegionsAnalyzerTests.UnionOverrideOutsideObjectRegionIsReported` | SHARED-REGGROUP-001, RH7401-001 | Verify + UnionSupport | fail (diagnostic or split missing) |
| `RH7405NonOverrideMethodsShouldNotBePlacedInOverrideRegionsAnalyzerTests.UnionNonOverrideMethodInOverrideRegionIsReported` | SHARED-REGGROUP-001, RH7405-001 | Verify + UnionSupport | fail (diagnostic or split missing) |
| `RH7406NonOverridePropertiesShouldNotBePlacedInOverrideRegionsAnalyzerTests.UnionNonOverridePropertyInOverrideRegionIsReported` | SHARED-REGGROUP-001, RH7406-001 | Verify + UnionSupport | fail (diagnostic or split missing) |
| `RH7407NonOverrideEventsShouldNotBePlacedInOverrideRegionsAnalyzerTests.UnionNonOverrideEventInOverrideRegionIsReported` | SHARED-REGGROUP-001, RH7407-001 | Verify + UnionSupport | fail (diagnostic or split missing) |
| `RH7408NonOverrideIndexersShouldNotBePlacedInOverrideRegionsAnalyzerTests.UnionNonOverrideIndexerInOverrideRegionIsReported` | SHARED-REGGROUP-001, RH7408-001 | Verify + UnionSupport | fail (diagnostic or split missing) |
| `RH7409InterfaceMethodsShouldBeGroupedByInterfaceRegionsAnalyzerTests.UnionInterfaceMethodOutsideInterfaceRegionIsReported` | SHARED-REGGROUP-001, RH7409-001 | Verify + UnionSupport | fail (diagnostic or split missing) |
| `RH7410InterfacePropertiesShouldBeGroupedByInterfaceRegionsAnalyzerTests.UnionInterfacePropertyOutsideInterfaceRegionIsReported` | SHARED-REGGROUP-001, RH7410-001 | Verify + UnionSupport | fail (diagnostic or split missing) |
| `RH7411InterfaceEventsShouldBeGroupedByInterfaceRegionsAnalyzerTests.UnionInterfaceEventOutsideInterfaceRegionIsReported` | SHARED-REGGROUP-001, RH7411-001 | Verify + UnionSupport | fail (diagnostic or split missing) |
| `RH7412InterfaceIndexersShouldBeGroupedByInterfaceRegionsAnalyzerTests.UnionInterfaceIndexerOutsideInterfaceRegionIsReported` | SHARED-REGGROUP-001, RH7412-001 | Verify + UnionSupport | fail (diagnostic or split missing) |
| `RH7102ConstantsMustAppearBeforeFieldsAnalyzerTests.UnionConstantAfterFieldIsReportedAndMoved` | RH7102-001, RH7102-005 | Verify + UnionSupport | fail (diagnostic or split missing) |
| `RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzerTests.UnionStaticMemberAfterInstanceMemberIsReportedAndMoved` | RH7103-001, RH7103-006 | Verify + UnionSupport | fail (diagnostic or split missing) |
| `RH7104PartialElementsMustDeclareAccessModifierAnalyzerTests.PartialUnionWithoutAccessibilityIsReportedAndFixed` | RH7104-001, RH7104-003, RH7104-004 | Verify + UnionSupport, DataRow LF/CRLF | fail (diagnostic or split missing) |
| `RH7104PartialElementsMustDeclareAccessModifierAnalyzerTests.PartialUnionPartGetsAccessibilityOfOtherPart` | RH7104-001, RH7104-002, RH7104-004 | Verify + UnionSupport | fail (diagnostic or split missing) |
| `RH7104PartialElementsMustDeclareAccessModifierAnalyzerTests.NestedPartialUnionGetsPrivate` | RH7104-001, RH7104-004 | Verify + UnionSupport | fail (diagnostic or split missing) |
| `RH7105DeclarationKeywordsMustFollowOrderAnalyzerTests.UnionModifiersOutOfOrderAreReportedAndOrdered` | SHARED-MODORDER-001, RH7105-001, RH7105-004 | Verify + UnionSupport | fail (diagnostic or split missing) |
| `RH7106ProtectedMustComeBeforeInternalAnalyzerTests.UnionInternalProtectedIsReportedAndSwapped` | SHARED-MODORDER-001, RH7106-001, RH7106-004 | Verify + UnionSupport | fail (diagnostic or split missing) |
| `RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsAnalyzerTests.UnionReadonlyFieldAfterFieldIsReportedAndMoved` | RH7109-001, RH7109-005 | Verify + UnionSupport | fail (diagnostic or split missing) |
| `RH7110MembersMustBeOrderedByAccessibilityAnalyzerTests.UnionMemberAfterMemberLaterInTheOrderIsReported` | RH7110-001 | Verify + UnionSupport | fail (diagnostic or split missing) |
| `RH7110MembersMustBeOrderedByAccessibilityAnalyzerTests.MemberIsComparedWithEveryEarlierMemberOfItsGroup` | RH7110-001 | Verify | pass |
| `RH7110MembersMustBeOrderedByAccessibilityAnalyzerTests.UnionMembersAreOrderedByAccessibilityAsInAStruct` | RH7110-001 | Verify + UnionSupport | fail (diagnostic or split missing) |
| `UnionDeclarationStructuralTests.UnionAttributeListIsSplit` | FMT-STRUCT-028 | AssertRuleResult (LF+CRLF, 2 passes) | fail (diagnostic or split missing) |
| `UnionDeclarationStructuralTests.UnionAttributeListWithTypeSpecifierIsSplitKeepingTheSpecifier` | FMT-STRUCT-028 | AssertRuleResult (LF+CRLF, 2 passes) | fail (diagnostic or split missing) |
| `UnionDeclarationStructuralTests.NestedUnionAttributeListIsSplit` | FMT-STRUCT-028 | AssertRuleResult (LF+CRLF, 2 passes) | fail (diagnostic or split missing) |
| `UnionDeclarationStructuralTests.UnionAttributeListWithACommentInsideStaysOneList` | FMT-STRUCT-028 | AssertRuleResult (LF+CRLF, 2 passes) | pass |
| `RH5404ElementMustNotBeOnSingleLineAnalyzerTests.VerifyFixLaysOutAUnionInsideTheFixedType` (changed: the union `U` inside the one-line class is now marked as a second RH5404 diagnostic, count 2, tag RH5404-001 added; the fixed text is unchanged) | RH5404-001, RH5404-003 | Verify, `CompilerDiagnostics.None` as before | fail ("expected 2, actual 1") |
| `AttributeListStructuralTransformTests.ListOfEachDeclarationOwnerIsSplitWithNothingBetweenTheCreatedLists` — new `DataRow` "Union" (`[A, B] union U(int, string);` → `[A][B]`) | FMT-STRUCT-028 | phase-level, LF+CRLF | fail (list not split) |

Other changes to test code:

- `src/Reihitsu.Analyzer.Test/Base/FormatterTestsBase{TAnalyzer}.cs`: new overload `VerifyFormatter(string source, string fixedSource, Action<CSharpAnalyzerVerifierTest<TAnalyzer>> onConfigure, params DiagnosticResult[] expected)`, so a formatter-parity test can add the `UnionSupport` reference to every analyzer run; the existing overloads route through it with no configuration (behavior unchanged).
- `RH5404ElementMustNotBeOnSingleLineAnalyzerTests.VerifyBaseTypeDeclarationTypesAreFullyEnumerated`: the doc-comment paragraph saying unions are deliberately not registered is removed (FC-031); the assertion is unchanged and passes.

### Observed today (head d3e4e14, test code only)

- Every "fail" row above fails on a missing diagnostic (`Mismatch between number of diagnostics returned, expected "n" actual "0"`, or "actual 1" where a member or nested class in the union is already reported: RH2004 `Get`, RH7104 `N`, RH5404 `C`) or, in the formatter tests, on the unsplit list (`[A, B]` kept). None fails on a compile error or an unrelated layout difference.
- The four new-rule files (`RH2110…AnalyzerTests`, `RH5418…AnalyzerTests`, `RH5418…FormatterTests`) fail to compile with exactly CS0246 for `RH2110NestedUnionsShouldNotBeUsedAnalyzer`, `RH5418EmptyUnionsShouldUseSemicolonDeclarationsAnalyzer`, and `RH5418EmptyUnionsShouldUseSemicolonDeclarationsCodeFixProvider`; they also reference `AnalyzerResources.RH2110MessageFormat` and `AnalyzerResources.RH5418MessageFormat`. Until the implementer adds these, the whole analyzer test project does not build. The other results above were taken with those three files moved aside.
- The "pass" rows are clarifications that change no output (RH5035, RH5038, RH5101, RH5102, RH5107, RH5109, RH5203, RH5305, SHARED-NESTED-001, the RH7110-001 "every earlier member" example) and the not-reported side of each widened condition (FC-003); they must stay green after the change.

### Not covered, and why

- **RH7402, RH7403, RH7404** (override properties, events, indexers): no union input reaches them. A union cannot declare a base class, and `object`/`ValueType` declare no virtual property, event, or indexer, so a union has no such override. RH7401 and RH7405–RH7412 each have a union test; the shared bases (`MembersShouldBeGroupedByRegionsAnalyzerBase`, `NonOverrideMembersShouldNotBePlacedInOverrideRegionsAnalyzerBase`) were not taken as proof for one test, per the role file.
- **RH5418 below C# 15**: no test (lead decision 12; `union` declares no type below C# 15).
- **RH5418 with a conditional directive or disabled text in the body**: no test (lead decision 14; pre-existing SHARED-DIAG-008 defect of the empty-type base, follow-up). `#region` and `#pragma` in the body are covered.
- **RH5507 `[A][B] union`** (two lists on one line): not tested; whether SHARED-ATTR-004 reports the first list before a `[` is a struct question the union does not change.
- **RH5105-5108 comment between name and `(` for RH5035-002**: RH5035-002 is not changed by the run; only RH5035-001 got a union example.
- **FC-006 `partial` modifier order for RH7105** (`partial public union`): does not compile (CS0267); `readonly` order and nested `private readonly` are covered.
- **FC-018/FC-036** (renames): no naming fix in this run.
- **FC-038–FC-043** (unsupported shapes): the run declares no shape; FMT-UNSUP-009 is touched only as the existing refusal, covered by `UnionWithUnsupportedCommentInCaseTypeListIsReportedWithoutFix` (SHARED-FIX-010).
- **FC-010/FC-012/FC-013**: formatter changes are covered by `AssertRuleResult` (two passes, LF and CRLF, phase stability) and by the two analyzer formatter-parity tests (RH5508 union split, RH5418 empty union) under LF and CRLF with two passes. The skeptic's per-rule FC-013 checks for RH5105/RH5106/RH5108/RH5110 are covered by the fix tests, whose expected outputs equal the formatter layout the change doc names (change 9).
- **FC-029, FC-022, FC-023, FC-025**: not reachable by these rule widenings (no key/value layout, literal, identifier-shape, or expression-body decision changes).
- **Self-hosting and doc-example tests** (`PackageReadmeContainsNoExtraAnalyzerRows`, `RuleDocumentationExampleTests`) and the architecture Sdd checks fail until the implementer adds the rules; they already exist and were not duplicated.

### Follow-up candidates

_None_ beyond those in `00-lead.md`.

### Formatter observation

Observation for the implementer, not a diagnosis: a list with a comment inside, `[A /* c */, B] struct S;`, stays on the line of its declaration under the formatter today (no line break behind the list); the union twin is asserted to behave the same (`UnionAttributeListWithACommentInsideStaysOneList`), which differs from the "line break behind the list" the change doc states for a list without a comment.

## Commands

```shell
# all new and changed analyzer tests (needs the RH2110/RH5418 types to build)
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Union|FullyQualifiedName~WithElement|FullyQualifiedName~MemberIsComparedWithEveryEarlierMemberOfItsGroup|FullyQualifiedName~ClassDeclaredInUnionIsNested|FullyQualifiedName~RH2110|FullyQualifiedName~RH5418|FullyQualifiedName~VerifyBaseTypeDeclarationTypesAreFullyEnumerated"
scripts/test.sh --project formatter --filter "FullyQualifiedName~UnionDeclarationStructuralTests|FullyQualifiedName~ListOfEachDeclarationOwnerIsSplitWithNothingBetweenTheCreatedLists"
```

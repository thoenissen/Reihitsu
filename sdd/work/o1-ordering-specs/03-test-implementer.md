Role: test implementer
Commit: 42b0170
Verdict: tests written

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

Lane: specs and tests for existing behavior. No production code touched; `OrderingDeclarationUtilitiesTests.cs` and `RegionDirectiveUtilitiesTests.cs` untouched. Every test below passes on the unchanged code (42b0170). No requirement turned out false against the code: no "test or spec is wrong" finding.

Every spec example of RH7001–RH7004, RH7101, RH7102, RH7104–RH7110 and `shared/modifier-ordering.md` became one test, using the spec's own `{|RH####:…|}` markup. Analyzer-only blocks use `Verify(source)`. Fix blocks use `Verify(source, fixedSource)`, which also runs Fix All once over every diagnostic of the block. "Reported, and no code fix is offered" blocks use `VerifyNoCodeFixIsOffered`, which checks the markup diagnostics and then asserts that the provider registers no action for any of them. Every one of the 62 IDs now has at least one tagged test (`grep -rho 'Requirement("<ID>")'`: minimum 1 for RH7001-002, RH7004-005, RH7101-002, RH7102-002/-003, RH7104-001/-002/-003, RH7106-002, RH7107-002/-005, RH7108-002/-005, RH7109-003).

## Tests

All in `src/Reihitsu.Analyzer.Test`. "New" = added test; "tag" = existing test that received tags. Helper column: V = `Verify(source)`, VF = `Verify(source, fixedSource)`, NF = `VerifyNoCodeFixIsOffered`, GA = existing `GetCodeFixActionsAsync`.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH7001 `VerifyDiagnostic` (tag) | RH7001-001, -003 | V | pass |
| RH7001 `EveryTopLevelNamespaceAfterTheFirstIsReported` | RH7001-001 | V | pass |
| RH7001 `NestedNamespacesAreNotCounted` | RH7001-002 | V | pass |
| RH7001 `DiagnosticIsLocatedOnTheWholeQualifiedName` | RH7001-003 | V | pass |
| RH7002 `VerifyDiagnosticsForAdditionalTopLevelTypesInFileScopedNamespace` (tag) | RH7002-001, -003 | V | pass |
| RH7002 `VerifyDiagnosticsForAdditionalTopLevelTypesInNestedNamespaces` (tag) | RH7002-001 | V | pass |
| RH7002 `VerifyNestedTypesAreIgnored` (tag) | RH7002-002 | V | pass |
| RH7002 `EveryTopLevelTypeAfterTheFirstIsReportedOnItsName` | RH7002-001, -003 | V | pass |
| RH7002 `TypeAfterAPartialDeclarationIsReported` | RH7002-001, -003 (FC-024, single-part only) | V | pass |
| RH7002 `TypesOfSeveralNamespacesAreCountedTogether` | RH7002-001 | V | pass |
| RH7002 `NestedTypesAndDelegatesAreNotCounted` | RH7002-002 | V | pass |
| RH7003 `VerifySingleTopLevelBlockScopedNamespaceIsDetected` (tag) | RH7003-001, -004 | V | pass |
| RH7003 `VerifyMultipleNamespacesAreNotFlagged`, `VerifyNamespaceWithAdditionalTopLevelMemberIsNotFlagged` (tag) | RH7003-002 | V | pass |
| RH7003 `VerifyUnsupportedLanguageVersionIsNotFlagged` (tag) | RH7003-003 | V | pass |
| RH7003 `VerifyCommentedNamespaceIsReported` (tag) | RH7003-001 | V | pass |
| RH7003 `NamespaceBesideUsingDirectivesAndAssemblyAttributesIsReported` | RH7003-001, -004 | V | pass |
| RH7003 `NamespaceFollowedByATypeOutsideItIsNotReported` | RH7003-002 | V | pass |
| RH7003 `NamespaceBesideATopLevelStatementIsNotReported` | RH7003-002 (top-level statement arm; console output kind) | V | pass |
| RH7003 `SpecificationExampleIsNotReportedForLanguageVersionNine` | RH7003-003 | V + C# 9 transform | pass |
| RH7004 existing diagnostic tests (`VerifyDiagnosticWhen…`, 4) (tag) | RH7004-001, -003 | V | pass |
| RH7004 `VerifyNoDiagnosticWhen(Await)UsingStatementBlockIsUsed` (tag) | RH7004-002 | V | pass |
| RH7004 `VerifyCodeFixPreservesAwaitKeyword…`, `…ToUsingStatementBlock`, `…WithNoFollowingStatements` (tag) | RH7004-004 | VF | pass |
| RH7004 `VerifyCodeFixIsNotOfferedWhenFollowingStatementsContain{LocalFunction,LabeledStatement}` (tag) | RH7004-006 | GA | pass |
| RH7004 `EveryKindOfUsingDeclarationIsReportedOnItsUsingKeyword` | RH7004-001, -003 (lambda, FC-027) | V | pass |
| RH7004 `UsingDeclarationsInLocalFunctionsAndNestedBlocksAreReported` | RH7004-001 (FC-027) | V | pass |
| RH7004 `UsingStatementIsNotReported` | RH7004-002 | V | pass |
| RH7004 `FixWrapsTheFollowingStatementIntoAUsingStatement` | RH7004-004 | VF | pass |
| RH7004 `FixKeepsAwaitAndEveryDeclaratorAndWritesAnEmptyBlock` | RH7004-004 | VF | pass |
| RH7004 `TopLevelUsingDeclarationIsReportedWithoutFix` | RH7004-005 (console output kind, no CS8805) | NF | pass |
| RH7004 `LaterLocalFunctionOfTheBlockWithholdsTheFix` | RH7004-006 (two statements later, FC-002/FC-003) | NF | pass |
| RH7004 `LocalFunctionDirectlyAfterTheDeclarationWithholdsTheFix` | RH7004-006 (immediately after) | NF | pass |
| RH7004 `LaterLabelOfTheBlockWithholdsTheFix` | RH7004-006 (label arm) | NF | pass |
| RH7004 `LocalFunctionNestedInAFollowingStatementDoesNotWithholdTheFix` | RH7004-004, -006 (other side of the condition) | VF | pass |
| RH7101 `VerifyCombinedFieldDeclarationsAreDetectedAndFixed` (tag) | RH7101-001, -003, -004 | VF | pass |
| RH7101 `VerifyCommentsArePreservedWhenSplitting` (tag) | RH7101-004 | VF | pass |
| RH7101 `CombinedFieldDeclarationsOfEveryTypeKindAreReported` | RH7101-001, -003 (unsafe allowed) | V | pass |
| RH7101 `EventFieldsAndLocalsWithSeveralDeclaratorsAreNotReported` | RH7101-002 | V | pass |
| RH7101 `DeclarationIsReportedOnceOnItsSecondDeclarator` | RH7101-003 | V | pass |
| RH7101 `FixRepeatsAttributesModifiersAndTypeForEachField` | RH7101-004 | VF | pass |
| RH7101 `FixKeepsTheCommentsThatTrailedTheSeparators` | RH7101-004 | VF | pass |
| RH7101 `RegionDirectiveInTheDeclarationWithholdsTheFix` | RH7101-005 (directive in leading trivia) | NF | pass |
| RH7101 `PragmaDirectiveBetweenTheDeclaratorsWithholdsTheFix` | RH7101-005 (directive between declarators) | NF | pass |
| RH7102 `ConstFieldsAreReportedAndFixedWhenTheyAppearAfterMutableFields` (tag) | RH7102-001, -004, -005 | VF | pass |
| RH7102 `SeparatorSurvivesTheReorderWhenConstFieldWasAlreadySeparated` (tag) | RH7102-005 | VF | pass |
| RH7102 `ConstantsAfterFieldsOfTheirAccessibilityGroupAreReported` | RH7102-001, -004 | V | pass |
| RH7102 `ConstantAfterFieldsOfAnotherAccessibilityIsNotReported` | RH7102-002 | V | pass |
| RH7102 `ConstantInANestedRegionOfTheSameTopLevelRegionIsReported` | RH7102-003, -006 | NF | pass |
| RH7102 `DiagnosticIsOnTheFirstDeclarator` | RH7102-004 | V | pass |
| RH7102 `FixMovesTheConstantInFrontOfTheFirstFieldOfItsGroup` | RH7102-005, SHARED-ORDER-001 | VF | pass |
| RH7102 `FixMovesTheDocumentationCommentWithTheConstant` | RH7102-005, SHARED-ORDER-002 | VF | pass |
| RH7102 `PragmaDirectivesAroundACrossedFieldWithholdTheFix` | RH7102-006 | NF | pass |
| RH7102 `PrivateConstantAfterFieldWithoutAccessibilityModifierIsNotReported` | untagged characterization (#968 cause) | V | pass |
| RH7102 `FixAlsoLaysOutOtherMembersOfTheType` | untagged characterization (#969: `public class Inner { }` → `public class Inner;`) | VF | pass |
| RH7104 `PartialTypesWithoutAccessibilityModifierAreReportedOnTheirNames` | RH7104-001, -003 | V | pass |
| RH7104 `PartialTypesWithAccessibilityModifierAreNotReported` | RH7104-002 | V | pass |
| RH7104 `FixInsertsInternalAfterDocumentationCommentAndAttributes` | RH7104-004 (doc comment + attribute list) | VF | pass |
| RH7104 `FixKeepsTheDocumentationCommentInFrontOfTheInsertedKeyword` | RH7104-004 (doc comment on the first modifier, no attributes) | VF | pass |
| RH7104 `FixInsertsTheDefaultAccessibilityOfTheContainer` | RH7104-004 (interface, struct) | VF | pass |
| RH7104 `FixInsertsPrivateForATypeNestedInAClassOrRecord` | RH7104-004 (class, record; FC-027) | VF | pass |
| RH7105 existing `…IsNotReported` tests (7) (tag) | RH7105-002 | V | pass |
| RH7105 `MisorderedDeclarationKeywordsAreReportedAndFixed` (tag) | RH7105-001, -003, -004, SHARED-MODORDER-002 | VF | pass |
| RH7105 `MisorderedModifiersWithPartialAreReportedAndFixed`, `MisorderedModifiersOnRefPartialStructAreReportedAndFixed` (tag) | RH7105-001, -004 | VF | pass |
| RH7105 `MisorderedModifiersWithCommentAreReportedWithoutFix` (tag) | RH7105-004, SHARED-MODORDER-003 | V + GA | pass |
| RH7105 `ModifiersOutOfTheCanonicalOrderAreReported` | RH7105-001, -003 | V | pass |
| RH7105 `ModifiersInTheCanonicalOrderAreNotReported` | RH7105-002 (unsafe allowed) | V | pass |
| RH7105 `DiagnosticIsOnTheFirstMisplacedModifier` | RH7105-003 | V | pass |
| RH7105 `FixWritesTheCanonicalOrderAndKeepsTheAccessibilityOrder` | RH7105-004, SHARED-MODORDER-002 | VF | pass |
| RH7105 `EveryCheckedDeclarationKindIsReported` | SHARED-MODORDER-001, RH7105-001 | V | pass |
| RH7105 `FixKeepsTheLeadingTriviaAndWritesSingleSpaces` | SHARED-MODORDER-002, RH7105-004 | VF | pass |
| RH7105 `CommentAfterAModifierWithholdsTheFix` | SHARED-MODORDER-003, RH7105-004 | NF | pass |
| RH7105 `CommentAfterTheLastModifierWithholdsTheFix` | SHARED-MODORDER-003 (comment trailing the last modifier) | NF | pass |
| RH7105 `PragmaDirectiveInFrontOfALaterModifierWithholdsTheFix` | SHARED-MODORDER-003 | NF | pass |
| RH7106 `MisorderedCompoundAccessModifiersAreReportedAndFixed` (tag) | RH7106-001, -003, -004 | VF | pass |
| RH7106 `CompoundAccessibilitiesInTheOtherOrderAreReportedOnTheSecondKeyword` | RH7106-001, -003 | V | pass |
| RH7106 `CanonicalCompoundAccessibilitiesAreNotReported` | RH7106-002 | V | pass |
| RH7106 `FixSwapsTheTwoKeywordsInPlace` | RH7106-004, SHARED-MODORDER-002 | VF | pass |
| RH7106 `FixSwapsProtectedPrivate` | RH7106-004 (`private` arm) | VF | pass |
| RH7106 `EveryCheckedDeclarationKindIsReported` | SHARED-MODORDER-001, RH7106-001 | V | pass |
| RH7106 `PragmaDirectiveInFrontOfALaterModifierWithholdsTheFix` | SHARED-MODORDER-003, RH7106-004 | NF | pass |
| RH7106 `CommentInFrontOfTheFirstModifierDoesNotWithholdTheFix` | SHARED-MODORDER-002, -003 (other side of the condition) | VF | pass |
| RH7107 `PropertyAccessorsAreReportedAndFixedWhenGetAppearsAfterSet` (tag) | RH7107-001, -003, -004 | VF | pass |
| RH7107 `GetAccessorAfterSetOrInitIsReportedOnItsKeyword` | RH7107-001, -003 | V | pass |
| RH7107 `GetAccessorAfterSetInARecordIsReported` | RH7107-001 (record, struct) | V | pass |
| RH7107 `AccessorListsInOrderAreNotReported` | RH7107-002 | V | pass |
| RH7107 `FixMovesGetInFrontOfInit` | RH7107-004 | VF | pass |
| RH7107 `FixMovesTheCommentAboveTheGetAccessorWithIt` | RH7107-004, SHARED-ORDER-002 | VF | pass |
| RH7107 `PragmaDirectiveInFrontOfTheGetAccessorWithholdsTheFix` | RH7107-005 | NF | pass |
| RH7107 `BalancedPragmaPairInTheCrossedAccessorBodyWithholdsTheFix` | untagged characterization (#970) | NF | pass |
| RH7108 `EventAccessorsAreReportedAndFixedWhenAddAppearsAfterRemove` (tag) | RH7108-001, -003, -004 | VF | pass |
| RH7108 `AddAccessorAfterRemoveIsReportedOnItsKeyword` | RH7108-001, -003 | V | pass |
| RH7108 `AccessorListInOrderIsNotReported` | RH7108-002 | V | pass |
| RH7108 `FixMovesAddAndItsCommentInFrontOfRemove` | RH7108-004, SHARED-ORDER-002 | VF | pass |
| RH7108 `PragmaDirectiveInFrontOfTheAddAccessorWithholdsTheFix` | RH7108-005 | NF | pass |
| RH7109 existing tests (9) (tag) | RH7109-001/-002/-004/-005/-006 as listed in the file | V, VF, GA | pass |
| RH7109 `ReadonlyFieldsAfterFieldsOfTheirGroupAreReported` | RH7109-001, -004 | V | pass |
| RH7109 `ReadonlyFieldsAfterFieldsOfOtherGroupsAreNotReported` | RH7109-002 | V | pass |
| RH7109 `ReadonlyFieldInTheSameTopLevelRegionIsReported` | RH7109-003, -006 | NF | pass |
| RH7109 `FixMovesTheReadonlyFieldInFrontOfTheFirstFieldOfItsGroup` | RH7109-005, SHARED-ORDER-001 | VF | pass |
| RH7109 `FixMovesTheDocumentationCommentWithTheReadonlyField` | RH7109-005, SHARED-ORDER-002 | VF | pass |
| RH7109 `InitializersOfBothFieldsWithholdTheFix` | RH7109-006, SHARED-ORDER-007 | NF | pass |
| RH7109 `PragmaDirectivesAroundACrossedFieldWithholdTheFix` | RH7109-006 | NF | pass |
| RH7109 `FixReordersTheFieldsOfAStructWithDefaultLayout` | untagged characterization (#1134) | VF | pass |
| RH7110 existing diagnostic tests (6), `MembersOrderedFromMostToLeastAccessibleProduceNoDiagnostic`, `DifferentKindsAreNotComparedAgainstEachOther`, `OrderingIsEvaluatedWithinEachRegionIndependently` (tag) | RH7110-001/-002/-003/-004 as listed in the file | V | pass |
| RH7110 `MembersAfterAMemberLaterInTheAccessibilityOrderAreReported` | RH7110-001, -004 | V | pass |
| RH7110 `OnlyMembersOfTheSameKindAreCompared` | RH7110-002, -004 (constructor, delegate, indexer `this`) | V | pass |
| RH7110 `EventFieldIsReportedOnItsFirstDeclarator` | RH7110-004 | V | pass |
| RH7110 `OperatorsAreReportedOnTheirOperatorTokens` | RH7110-004 (operator token, `operator` keyword) | V, compiler diagnostics off | pass |
| RH7110 `MemberInANestedRegionIsComparedWithinItsTopLevelRegion` | RH7110-003 | V | pass |
| RH7110 `MemberInARegionIsNotComparedWithAMemberOutsideEveryRegion` | RH7110-003 | V | pass |

Notes:

- `OperatorsAreReportedOnTheirOperatorTokens`: operators must be `public static` (CS0558), so in compiling code RH7110 never reaches the operator and conversion-operator locations. The test turns compiler diagnostics off (`CompilerDiagnostics.None`) and checks only the location the requirement states. Whether that clause belongs in the spec is the spec checker's call; the test confirms it is true of the code.
- The untagged characterization tests pin known, tracked behavior that no requirement states (#968, #969, #970, #1134). A fix of those issues changes them on purpose.

## Scenarios (bug fix)

Not applicable (specs-and-tests lane).

## Not covered, and why

- Existing tests left untagged on purpose: tests with `#if`/`#endif` in the judged construct (P22: RH7003 `VerifyDirectiveContainingNamespaceIsReported`, RH7101 `VerifyFixIsNotOfferedWhenFieldCarriesDirective`, RH7105 `MisorderedModifiersWithDirectiveAreReportedWithoutFix`, RH7106 `MisorderedCompoundAccessModifiersWithDirectiveAreReportedWithoutFix`, RH7107/RH7108 `NoCodeFixWhenDirectivesAreInAccessorLeadingTrivia`, `NoCodeFixWhenDirectiveFollowsAccessorAttributeList`). Tests with fix output that carries formatting outside the diagnostic's code (FC-030: RH7004 `VerifyCodeFixTransformsUsingDeclarationWithMultipleFollowingStatements` (A4), RH7104 `PartialTypesWithoutAccessModifierAreReportedAndFixed` (D1), RH7107 `SingleLineExpressionBodiedAccessorsAreFixedOntoSeparateLines`, `MultiLineAutoIndexerAccessorsAreFixedOntoSingleLine`, `BlankLineAboveReorderedPropertyIsKeptWhenAccessorBodyIsBraced` (F1), RH7108 `SingleLineEventAccessorsAreFixedOntoSeparateLines`, RH7101 `VerifyFixDoesNotReformatUnrelatedMembers`). Multi-part partial types (D2: RH7104 `PartialTypesWithPublicAndCompoundAccessibilityAreFixed`). Members without accessibility modifier (#968: RH7109 `ImplicitAndPrivateClassFieldsDoNotProduceDiagnostics`, RH7110 `MembersWithoutExplicitAccessibilityAreIgnored`). Cross-region comparisons (N3, unspecified: RH7102 `NoCodeFixWhenDirectivesAreInLeadingTrivia`, RH7109 `NoCodeFixWhenDirectivesAreInLeadingTrivia`, `MixedAccessibilityInterfaceViolationWithDirectivesHasNoCodeFix`). Also RH7109 `OrderedReadonlyInterfaceFieldsDoNotProduceDiagnostics`, `ImplicitReadonlyBeforeExplicitPublicMutableInterfaceFieldDoesNotProduceDiagnostics`, `InterfaceConstantsDoNotProduceDiagnostics`, which no requirement states as such. The SHARED-FIX-003 struct-layout tests keep only their existing tag.
- Fix All: no requirement written or tagged. `NumberOfFixAllIterations = 2` in the RH7004, RH7102, RH7109 scenarios (#1143) is untouched and gets no tag (FC-017).
- N2 (a directive between the last modifier and the next token): not tested. SHARED-MODORDER-002/-003 exclude it, and the current output does not parse.
- Line endings: no test adds a CRLF variant. These requirements state no line-ending behavior beyond the shared specs (SHARED-FIX tests and SHARED-ORDER CRLF tests in the RH7103 class cover them). The layout of each fix belongs to the shared formatter path, which `shared/code-fixes.md` covers.
- Comment/directive in a token gap: RH7101 (region, pragma), SHARED-MODORDER-003 (comment, pragma), RH7102/RH7107/RH7108/RH7109 (pragma). No `#if` case is tagged (P22).
- Naming-fix Renamer: not applicable (no rename fix).
- Formatter parity: RH7101 parity stays in `RH7101DoNotCombineFieldsFormatterTests` (FMT-STRUCT-021..023 are the formatter's spec). Nothing added.
- Failure classes (FC-002/-003, -017, -020/-021, -024, -027, -030) are handled as above. Classes concerning formatter idempotency or phase interaction do not reach these specs: no formatter code is under test here.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH7001FileMay|FullyQualifiedName~RH7002FileMay|FullyQualifiedName~RH7003FileScoped|FullyQualifiedName~RH7004UsingDecl|FullyQualifiedName~RH7101DoNotCombineFieldsAnalyzerTests|FullyQualifiedName~RH7102Const|FullyQualifiedName~RH7104Partial|FullyQualifiedName~RH7105Decl|FullyQualifiedName~RH7106Prot|FullyQualifiedName~RH7107Prop|FullyQualifiedName~RH7108Event|FullyQualifiedName~RH7109Readonly|FullyQualifiedName~RH7110Members"
scripts/test.sh --project architecture --filter "FullyQualifiedName~SpecConsistencyTests|FullyQualifiedName~SpecStructureTests|FullyQualifiedName~RequirementCoverage"
```

Results on the working tree: analyzer filter 193 passed (before the last 6 additions) and then 84 of 84 in the re-run of the five classes that changed. Architecture filter: 39 passed; the whole architecture project: 72 passed. Analyzer `FullyQualifiedName~SelfHosting`: 17 passed (the self-hosting run takes about 16 minutes). `scripts/format.sh` over the 13 changed test files: 0 files changed.

Role: test implementer
Commit: df1f827
Verdict: tests written

Lane: specs and tests for existing behavior. No production code was touched; every test below passes on the unchanged code of `df1f827`. My prompt carried no intended fix, suspected cause, or hoped-for verdict, and neither did the handovers.

Inputs: thoenissen/Reihitsu#979, `00-lead.md`, `01-spec-author.md`, `02-skeptic.md`, `03-spec-author.md` (supersedes `01`), the 16 specs `sdd/specs/analyzer/RH4111.md`–`RH4118.md`, `RH4120.md`–`RH4127.md`, and `sdd/specs/shared/single-letter-names.md`. Expected values are the spec examples, copied verbatim (markup `{|RH####:…|}`), never derived from the code.

What was established:
- All 36 new IDs are covered: `scripts/test.sh --project architecture --configuration Debug` → 72 passed, including `RequirementCoverageTests.EveryRequirementOfAnEnforcedSpecIsCovered`, which failed with all 36 IDs before this commit (see `01-spec-author.md`).
- `scripts/test.sh --project analyzer --configuration Debug --filter "FullyQualifiedName~Naming.RH41"` → 327 passed, 0 failed.
- `scripts/test.sh --project analyzer --configuration Debug` (whole project) → 4329 passed, 0 failed.
- Every example block of every requirement of the 16 specs and of the shared spec has its own test (one test per example; one extra test where the requirement names a case its examples omit). Each "is not reported" block is a `Verify(source)` with no markup; each before/after pair is a `Verify(source, fixedSource)`, which also checks Fix All and re-analysis.
- Sanity check of the markup approach: removing one marker from `IterationVariablesOfEveryForeachFormAtAnyNestingDepthAreReported` made it fail with "expected 7 actual 8" diagnostics; restored.
- `scripts/format.sh` over the 16 files: formatted 0 of 16.

## Tests

New tests (all in `src/Reihitsu.Analyzer.Test/Naming/`):

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH4111…Tests.PrivatePropertiesOfEveryKindAreCheckedWhileOtherGroupsAreNot | RH4111-001 | Verify(source) | pass |
| RH4111…Tests.PartialPrivatePropertyIsReportedOnEachPart | RH4111-001 | Verify(source) | pass |
| RH4111…Tests.FixRenamesPrivatePropertiesAndTheirReferencesToPascalCase | RH4111-002 | Verify(source, fixedSource) | pass |
| RH4112…Tests.ProtectedPropertiesOfEveryKindAreCheckedWhileOtherGroupsAreNot | RH4112-001 | Verify(source) | pass |
| RH4112…Tests.AbstractAndVirtualProtectedPropertiesAreChecked | RH4112-001 | Verify(source) | pass |
| RH4112…Tests.PartialProtectedPropertyIsReportedOnEachPart | RH4112-001 | Verify(source) | pass |
| RH4112…Tests.FixRenamesProtectedPropertiesAndTheirReferencesToPascalCase | RH4112-002 | Verify(source, fixedSource) | pass |
| RH4112…Tests.FixRenamesOverriddenAndOverridingProtectedPropertiesInSource | RH4112-002 | Verify(source, fixedSource) | pass |
| RH4113…Tests.InternalPropertiesOfEveryKindAreCheckedWhileOtherGroupsAreNot | RH4113-001 | Verify(source) | pass |
| RH4113…Tests.AbstractAndVirtualInternalPropertiesAreChecked | RH4113-001 | Verify(source) | pass |
| RH4113…Tests.PartialInternalPropertyIsReportedOnEachPart | RH4113-001 | Verify(source) | pass |
| RH4113…Tests.FixRenamesInternalPropertiesAndTheirReferencesToPascalCase | RH4113-002 | Verify(source, fixedSource) | pass |
| RH4113…Tests.FixRenamesOverriddenAndOverridingInternalPropertiesInSource | RH4113-002 | Verify(source, fixedSource) | pass |
| RH4114…Tests.PublicPropertiesOfEveryKindAreCheckedWhileOtherGroupsAreNot | RH4114-001 | Verify(source) | pass |
| RH4114…Tests.AbstractVirtualOverridingAndImplementingPublicPropertiesAreChecked | RH4114-001 | Verify(source) | pass |
| RH4114…Tests.PartialPublicPropertyIsReportedOnEachPart | RH4114-001 | Verify(source) | pass |
| RH4114…Tests.FixRenamesPublicPropertiesTheirReferencesAndExplicitImplementationsToPascalCase | RH4114-002 | Verify(source, fixedSource) | pass |
| RH4114…Tests.FixRenamesImplicitImplementationsAndOverridesOfPublicPropertiesInSource | RH4114-002 | Verify(source, fixedSource) | pass |
| RH4115…Tests.LocalVariablesOfEveryDeclarationFormAreChecked | RH4115-001 | Verify(source) + AllowUnsafe | pass |
| RH4115…Tests.DesignationsInSwitchesInitializersAndSlicePatternsAreChecked | RH4115-001 | Verify(source) | pass |
| RH4115…Tests.VariablesOfVarDeconstructionsAreNotChecked | RH4115-002 | Verify(source) | pass |
| RH4115…Tests.FixRenamesLocalVariablesAndTheirReferencesToCamelCase | RH4115-003 | Verify(source, fixedSource) | pass |
| RH4116…Tests.TupleTypeElementNamesAreCheckedWhereverTheTupleTypeIsWritten | RH4116-001 | Verify(source) | pass |
| RH4116…Tests.TupleTypeElementNamedWithUnderscoreOrVerbatimIdentifierIsReported | RH4116-001 | Verify(source) | pass |
| RH4117…Tests.VariablesOfVarDeconstructionsAreCheckedInAssignmentsAndForeachStatements | RH4117-001 | Verify(source) | pass |
| RH4117…Tests.FixRenamesDeconstructionVariablesAndTheirReferencesToCamelCase | RH4117-002 | Verify(source, fixedSource) | pass |
| RH4118…Tests.WrittenTupleExpressionElementNamesAreChecked | RH4118-001 | Verify(source) | pass |
| RH4118…Tests.TupleExpressionElementNamedWithVerbatimIdentifierIsReportedIncludingThePrefix | RH4118-001 | Verify(source) | pass |
| RH4120…Tests.PrimaryConstructorParametersOfEveryRecordKindAreChecked | RH4120-001 | Verify(source) | pass |
| RH4120…Tests.DiscardsAndParametersOfOtherConstructorsAndMembersAreNotChecked | RH4120-002 | Verify(source) | pass |
| RH4120…Tests.FixRenamesParameterItsPropertyAndEveryReferenceToPascalCase | RH4120-003 | Verify(source, fixedSource) | pass |
| RH4121…Tests.TypeParametersOfEveryGenericDeclarationAreChecked | RH4121-001 | Verify(source) | pass |
| RH4121…Tests.PartialTypeTypeParameterIsReportedOnEachPart | RH4121-001 | Verify(source) | pass |
| RH4121…Tests.PartialMethodTypeParameterIsReportedOnEachPart | RH4121-001 | Verify(source) | pass |
| RH4121…Tests.TypeParameterNameIsTTAndPascalCaseOrTAndDigits | RH4121-002 | Verify(source) | pass |
| RH4121…Tests.FixAddsOrCorrectsTheTPrefixAndRenamesEveryReference | RH4121-003 | Verify(source, fixedSource) | pass |
| RH4121…Tests.FixIsWithheldWhenNoUsableNameCanBeDerived | RH4121-004 | Verify(source) + GetCodeFixActionsAsync | pass |
| RH4122…Tests.SingleLetterNameIsOneCharacterOtherThanUnderscore | SHARED-SINGLE-001, RH4122-001 | Verify(source) | pass |
| RH4122…Tests.SingleLetterNumberCountsAsSingleLetterName | SHARED-SINGLE-001 | Verify(source) | pass |
| RH4122…Tests.ParametersOfEveryMemberKindAndImplicitImplementationsAreChecked | RH4122-001 | Verify(source) | pass |
| RH4122…Tests.PartialMethodParameterIsReportedOnEachPart | RH4122-001 | Verify(source) | pass |
| RH4122…Tests.OverridesAndExplicitInterfaceImplementationsAreNotChecked | RH4122-002 | Verify(source) | pass |
| RH4122…Tests.RecordLocalFunctionLambdaAndAnonymousMethodParametersAreNotChecked | RH4122-003 | Verify(source) | pass |
| RH4123…Tests.SingleLetterParametersOfLocalFunctionsAreReported | RH4123-001 | Verify(source) | pass |
| RH4123…Tests.MethodLambdaAndAnonymousMethodParametersAreNotChecked | RH4123-002 | Verify(source) | pass |
| RH4124…Tests.SingleLetterLambdaAndAnonymousMethodParametersAreReported | RH4124-001 | Verify(source) | pass |
| RH4124…Tests.MethodAndLocalFunctionParametersAreNotChecked | RH4124-002 | Verify(source) | pass |
| RH4125…Tests.SingleLetterLocalsOfEveryDeclaratorAndDesignationAreReported | RH4125-001 | Verify(source) + AllowUnsafe | pass |
| RH4125…Tests.IterationVariablesCatchVariablesAndFieldsAreNotChecked | RH4125-002, SHARED-SINGLE-002 | Verify(source) | pass |
| RH4125…Tests.LocalsDeclaredInForeachCollectionOrBodyAreNotIterationVariables | SHARED-SINGLE-002, RH4125-001 | Verify(source) | pass |
| RH4126…Tests.SingleLetterIterationVariablesAreReported | RH4126-001 | Verify(source) | pass |
| RH4126…Tests.IterationVariablesOfEveryForeachFormAtAnyNestingDepthAreReported | SHARED-SINGLE-002, RH4126-001 | Verify(source) | pass |
| RH4126…Tests.LocalsOutsideTheIterationVariablesAreNotChecked | RH4126-002, SHARED-SINGLE-002 | Verify(source) | pass |
| RH4127…Tests.SingleLetterExceptionVariablesAreReported | RH4127-001 | Verify(source) | pass |

Existing tests tagged (no assertion changed):

| Test | Requirements added |
|---|---|
| RH4111…Tests.VerifyDiagnosticsForPrivatePropertyAndReferenceAreFixed | RH4111-001, RH4111-002 |
| RH4111…Tests.VerifyNoDiagnosticsForPascalCasePrivateProperty | RH4111-001 |
| RH4111…Tests.VerifyDiagnosticsForExpressionBodiedPrivatePropertyWrongCasing | RH4111-001, RH4111-002 |
| RH4111…Tests.VerifyNoDiagnosticsForPrivateProtectedProperty | RH4111-001 |
| RH4111…Tests.VerifyImplicitlyPrivateClassPropertyIsDetectedAndFixed | RH4111-001, RH4111-002 |
| RH4111…Tests.VerifyNoDiagnosticsForImplicitInterfaceProperty | RH4111-001 |
| RH4111…Tests.VerifyNoDiagnosticsForExplicitInterfacePropertyImplementation | RH4111-001 |
| RH4111…Tests.ExplicitInterfacePropertyImplementationBelongsToNoAccessibilityGroup | RH4111-001 |
| RH4112…Tests.VerifyDiagnosticsForProtectedPropertyAndReferenceAreFixed | RH4112-001, RH4112-002 |
| RH4112…Tests.VerifyNoDiagnosticsForPascalCaseProtectedProperty | RH4112-001 |
| RH4112…Tests.VerifyNoDiagnosticsForProtectedInternalProperty | RH4112-001 |
| RH4112…Tests.VerifyDiagnosticsForPrivateProtectedPropertyWrongCasing | RH4112-001, RH4112-002 |
| RH4112…Tests.VerifyNoDiagnosticsForImplicitProperties | RH4112-001 |
| RH4113…Tests.VerifyDiagnosticsForInternalPropertyAndReferenceAreFixed | RH4113-001, RH4113-002 |
| RH4113…Tests.VerifyNoDiagnosticsForPascalCaseInternalProperty | RH4113-001 |
| RH4113…Tests.VerifyDiagnosticsForExpressionBodiedInternalPropertyWrongCasing | RH4113-001, RH4113-002 |
| RH4113…Tests.VerifyDiagnosticsForProtectedInternalPropertyWrongCasing | RH4113-001, RH4113-002 |
| RH4113…Tests.VerifyNoDiagnosticsForImplicitProperties | RH4113-001 |
| RH4114…Tests.VerifyDiagnosticsForPublicPropertyAndReferenceAreFixed | RH4114-001, RH4114-002 |
| RH4114…Tests.VerifyNoDiagnosticsForPascalCasePublicProperty | RH4114-001 |
| RH4114…Tests.VerifyDiagnosticsForExpressionBodiedPublicPropertyWrongCasing | RH4114-001, RH4114-002 |
| RH4114…Tests.VerifyImplicitlyPublicInterfacePropertyIsDetectedAndFixed | RH4114-001, RH4114-002 |
| RH4114…Tests.VerifyNoDiagnosticsForImplicitClassProperty | RH4114-001 |
| RH4114…Tests.VerifyInterfacePropertyOwnsExplicitImplementationCasing | RH4114-001, RH4114-002 |
| RH4115…Tests.VerifyDiagnosticsForLocalVariableAndReferenceAreFixed | RH4115-001, RH4115-003 |
| RH4115…Tests.VerifyNoDiagnosticsForCamelCaseLocalVariable | RH4115-001 |
| RH4115…Tests.VerifyDiagnosticsForMultipleLocalVariables | RH4115-001 |
| RH4115…Tests.VerifyDiagnosticForForEachVariableAndReferenceIsFixed | RH4115-001, RH4115-003 |
| RH4115…Tests.VerifyDiagnosticForForInitializerVariableAndReferenceIsFixed | RH4115-001, RH4115-003 |
| RH4115…Tests.VerifyDiagnosticForUsingStatementVariableAndReferenceIsFixed | RH4115-001, RH4115-003 |
| RH4115…Tests.VerifyDiagnosticForFixedStatementVariableAndReferenceIsFixed | RH4115-001, RH4115-003 |
| RH4115…Tests.VerifyDiagnosticForOutVariableAndReferenceIsFixed | RH4115-001, RH4115-003 |
| RH4115…Tests.VerifyDiagnosticForPatternVariableAndReferenceIsFixed | RH4115-001, RH4115-003 |
| RH4115…Tests.VerifyDiagnosticForCatchVariableAndReferenceIsFixed | RH4115-001, RH4115-003 |
| RH4115…Tests.VerifyNoDiagnosticsForDeconstructionVariables | RH4115-002 |
| RH4116…Tests.VerifyDiagnosticsForTupleTypeElementWrongCasing | RH4116-001 |
| RH4116…Tests.VerifyDiagnosticsForMultipleTupleTypeElements | RH4116-001 |
| RH4116…Tests.VerifyNoDiagnosticsForPascalCaseTupleTypeElements | RH4116-001 |
| RH4116…Tests.VerifyNoDiagnosticsForUnnamedTupleTypeElements | RH4116-001 |
| RH4117…Tests.VerifyDiagnosticsForDeconstructionVariableAndReferenceAreFixed | RH4117-001, RH4117-002 |
| RH4117…Tests.VerifyDiagnosticsForMultipleDeconstructionVariables | RH4117-001 |
| RH4117…Tests.VerifyDiagnosticForNestedDeconstructionVariableAndReferenceIsFixed | RH4117-001, RH4117-002 |
| RH4117…Tests.VerifyNoDiagnosticsForCamelCaseDeconstructionVariables | RH4117-001 |
| RH4118…Tests.VerifyDiagnosticsForTupleExpressionElementWrongCasing | RH4118-001 |
| RH4118…Tests.VerifyDiagnosticsForMultipleTupleExpressionElements | RH4118-001 |
| RH4118…Tests.VerifyNoDiagnosticsForPascalCaseTupleExpressionElements | RH4118-001 |
| RH4118…Tests.VerifyNoDiagnosticsForUnnamedTupleExpressionElements | RH4118-001 |
| RH4120…Tests.VerifyDiagnosticsForRecordPrimaryConstructorParameterWrongCasing | RH4120-001, RH4120-003 |
| RH4120…Tests.VerifyCodeFixRenamesRecordPrimaryConstructorParameterReferences | RH4120-001, RH4120-003 |
| RH4120…Tests.VerifyDiagnosticsForRecordStructPrimaryConstructorParameterWrongCasing | RH4120-001, RH4120-003 |
| RH4120…Tests.VerifyNoDiagnosticsForPascalCaseRecordPrimaryConstructorParameter | RH4120-001 |
| RH4120…Tests.VerifyNoDiagnosticsForMethodParameter | RH4120-002 |
| RH4121…Tests.VerifyDiagnosticsForClassTypeParameterWithoutPrefix | RH4121-001, RH4121-002, RH4121-003 |
| RH4121…Tests.VerifyDiagnosticsForCamelCaseMethodTypeParameter | RH4121-001, RH4121-002, RH4121-003 |
| RH4121…Tests.VerifyDiagnosticsForDelegateTypeParameterWithoutPrefix | RH4121-001, RH4121-003 |
| RH4121…Tests.VerifyDiagnosticsForLocalFunctionTypeParameterWithoutPrefix | RH4121-001, RH4121-003 |
| RH4121…Tests.VerifyDiagnosticsForRecordTypeParameterWithoutPrefix | RH4121-001, RH4121-003 |
| RH4121…Tests.VerifyDiagnosticsForTypeParameterWithLowercasePrefix | RH4121-002, RH4121-003 |
| RH4121…Tests.VerifyDiagnosticsForTypeParameterWithLowercasePrefixAndDigitSuffix | RH4121-002, RH4121-003 |
| RH4121…Tests.VerifyDiagnosticsForTypeParameterNamedLikeWordStartingWithT | RH4121-002, RH4121-003 |
| RH4121…Tests.VerifyDiagnosticsForMultipleTypeParametersWithoutPrefix | RH4121-001, RH4121-003 |
| RH4121…Tests.VerifyNoDiagnosticsForSingleTTypeParameter | RH4121-002 |
| RH4121…Tests.VerifyNoDiagnosticsForPrefixedTypeParameters | RH4121-002 |
| RH4121…Tests.VerifyNoDiagnosticsForPrefixedMethodTypeParameter | RH4121-002 |
| RH4121…Tests.VerifyNoDiagnosticsForNumberedTypeParameters | RH4121-002 |
| RH4122…Tests.VerifyDiagnosticsForSupportedParameterKinds | RH4122-001 |
| RH4122…Tests.VerifyDiagnosticForClassPrimaryConstructorParameter | RH4122-001 |
| RH4122…Tests.VerifyDiagnosticsForAdditionalParameterKinds | RH4122-001 |
| RH4122…Tests.VerifyNoDiagnosticsForDescriptiveNamesAndDiscardIdentifier | SHARED-SINGLE-001, RH4122-001 |
| RH4122…Tests.VerifyNoDiagnosticsForOverrideParameters | RH4122-002 |
| RH4122…Tests.VerifyNoDiagnosticsForExplicitInterfaceImplementationParameters | RH4122-002 |
| RH4122…Tests.VerifyNoDiagnosticsForRecordPrimaryConstructorParameters | RH4122-003 |
| RH4122…Tests.VerifyNoDiagnosticsForOtherParameterKinds | RH4122-003 |
| RH4123…Tests.VerifyDiagnosticForLocalFunctionParameter | RH4123-001 |
| RH4123…Tests.VerifyNoDiagnosticsForDescriptiveNameAndDiscardIdentifier | RH4123-001 |
| RH4123…Tests.VerifyNoDiagnosticsForOtherParameterKinds | RH4123-002 |
| RH4124…Tests.VerifyDiagnosticForSimpleLambdaParameter | RH4124-001 |
| RH4124…Tests.VerifyDiagnosticsForParenthesizedLambdaAndAnonymousMethodParameters | RH4124-001 |
| RH4124…Tests.VerifyNoDiagnosticsForDescriptiveNamesAndDiscardIdentifier | RH4124-001 |
| RH4124…Tests.VerifyNoDiagnosticsForOtherParameterKinds | RH4124-002 |
| RH4125…Tests.VerifyDiagnosticsForLocalVariableDeclarators | RH4125-001 |
| RH4125…Tests.VerifyDiagnosticsForVariableDesignations | RH4125-001 |
| RH4125…Tests.VerifyNoDiagnosticsForDescriptiveLocalNames | RH4125-001 |
| RH4125…Tests.VerifyNoDiagnosticsForDiscards | SHARED-SINGLE-001, RH4125-001 |
| RH4125…Tests.VerifyNoDiagnosticsForFields | RH4125-002 |
| RH4125…Tests.VerifyNoDiagnosticsForDeconstructedForeachVariables | SHARED-SINGLE-002, RH4125-002 |
| RH4125…Tests.VerifyDiagnosticForDesignationInForeachBody | SHARED-SINGLE-002, RH4125-001 |
| RH4126…Tests.VerifyDiagnosticForForeachIterationVariable | SHARED-SINGLE-002, RH4126-001 |
| RH4126…Tests.VerifyDiagnosticsForDeconstructedForeachIterationVariables | SHARED-SINGLE-002, RH4126-001 |
| RH4126…Tests.VerifyNoDiagnosticsForDescriptiveNameAndDiscardIdentifier | SHARED-SINGLE-001, RH4126-001 |
| RH4126…Tests.VerifyNoDiagnosticsForRegularLocalVariables | RH4126-002 |
| RH4126…Tests.VerifyNoDiagnosticsForDesignationsInForeachBody | SHARED-SINGLE-002, RH4126-002 |
| RH4127…Tests.VerifyDiagnosticForCatchExceptionVariable | RH4127-001 |
| RH4127…Tests.VerifyNoDiagnosticsForAllowedCatchDeclarations | SHARED-SINGLE-001, RH4127-001 |

Failure classes and skeptic inputs these tests cover:
- FC-024 partial members: one diagnostic per part for RH4111–RH4114 (partial property), RH4121 (partial type and partial method), RH4122 (partial method). Static interface property (RH4114). Explicit implementations: no group (RH4111, RH4114), not checked (RH4122-002).
- FC-023: verbatim `@b` (RH4122), `@value` in a tuple type (RH4116) and tuple expression (RH4118), span includes `@`; `_` exempt in RH4120, RH4122–RH4127 and in lambda discards `(_, _)`; `_` reported as a tuple type element (RH4116, from SHARED-CASING-001); precomposed `é` (U+00E9) and `Ⅻ` (U+216B) count as single letters.
- FC-027 nesting: nested tuple types and expressions, `var (a, (b, c))`, `foreach ((int c, var (d, e)) in …)`, designations in a `foreach` collection or body count for RH4125 and not RH4126, recursive and list pattern designations (RH4115 `Text`, `Both`, `Items`), slice `.. var Rest`, `case long Big:`, switch-expression arm, `await foreach`, `out var` in field and constructor initializers.
- FC-003 both sides: RH4121 accepts `T`, `TKey`, `TKEY`, `T1`, `T23`, reports `Key`, `tKey`, `t1`, `Type`, `T1Key`; fix `tValue`→`TValue` versus `Type`→`TType`; single-letter versus two-letter names; `catch` with and without a variable; accessibility-group boundaries (`private protected`, `protected internal`, implicit accessibility).
- FC-009: no action registered for RH4121 `_` and `T1Key` (`GetCodeFixActionsAsync` returns empty).
- FC-006 / FC-036 (source chains only): RH4120 parameter, synthesized property and `r.firstName` renamed together; RH4112–RH4114 cascade to override/overridden properties and RH4114 interface/implicit implementation and explicit implementation.
- FC-025: expression-bodied and static properties (RH4111-001 to RH4114-001), abstract and virtual properties (RH4112–RH4114).

## Scenarios (bug fix)

_N/A._ No bug fix in this lane.

## Not covered, and why

- **Not pinned, by instruction (`02-skeptic.md`, `03-spec-author.md`):** Q1–Q11 of `03-spec-author.md` (including findings A, B, E, F), thoenissen/Reihitsu#971, and the questions of thoenissen/Reihitsu#1009 (FC-020 inactive code included). No new test asserts any of them, and none is tagged on an existing test that touches them.
- **Pre-existing tests that touch an open question, left untagged and unchanged:** `RH4115LocalVariableCasingAnalyzerTests.NoCodeFixForLetterlessLocalVariable` (`int _ = 42;`, Q1) asserts only that no action is registered for a constructed diagnostic; it does not assert that RH4115 reports `_`. Not tagged, because RH4115-001 covers names other than `_`.
- **Line endings / CRLF, comment or directive in a gap, formatter parity, idempotency:** not applicable. These rules rename identifiers through the Roslyn renamer or report only; no rule touches a token gap or layout, and no formatter phase renames identifiers (FC-013, FC-014 not reachable, per `02-skeptic.md` *Surfaces*). The shared rename/withhold/Fix All behavior (SHARED-CASING-007 to -010) is already covered by existing tagged tests and is not restated.
- **Fix All per rule:** each code-fix class keeps its existing `GetFixAllScenario`; every new before/after test with two or more diagnostics runs through the verifier's Fix All check as well. No rule-specific Fix All requirement exists (SHARED-CASING-010 covers it).
- **Referenced-assembly inputs (A, E):** the specs now exclude them; the test helpers build single-project solutions, and such inputs are open questions, so no test was added.
- **Shared-spec owner tests in `Reihitsu.Core.Test`:** `SingleLetterIdentifierUtilities` lives in the analyzer assembly; SHARED-SINGLE-001/-002 are covered through the analyzer tests of RH4122, RH4125 and RH4126, matching the spec's own examples.
- **`RH4127SingleLetterCatchExceptionVariableNamesAnalyzerTests.VerifyNoDiagnosticsForRegularLocalVariables`:** left untagged; RH4127 has no requirement stating that other locals are not reported, so it would verify RH4127-001 only incidentally.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Naming.RH411|FullyQualifiedName~Reihitsu.Analyzer.Test.Naming.RH412"
scripts/test.sh --project analyzer --filter "TestCategory=SHARED-SINGLE-001|TestCategory=SHARED-SINGLE-002"
scripts/test.sh --project architecture --filter "FullyQualifiedName~RequirementCoverageTests|FullyQualifiedName~SpecConsistencyTests|FullyQualifiedName~SpecStructureTests"
```

Per requirement: `scripts/test.sh --project analyzer --filter "TestCategory=<ID>"`.

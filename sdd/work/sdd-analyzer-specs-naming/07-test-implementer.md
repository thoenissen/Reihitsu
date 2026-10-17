Role: test implementer
Commit: a8971148922e1098dcc228cc1ffde380337c9beb
Verdict: tests written

My prompt named the issue, the slug, and the handovers to read. It carried no intended fix, suspected cause, or hoped-for verdict, so there was nothing to ignore.

This run writes specs for existing behavior. There is no reproduction, so the *Scenarios* section is empty. Every test below passes on the unchanged production code. No test failed against a spec. The first run had two red tests, and both came from the test harness, not from the specs or the code (see *How the examples are verified*).

## What was done

- **Tagged existing tests** in the 20 rule test classes, the RH4115 test class (the cross-document RH4002 tests and the sibling-scope test), and `CasingUtilitiesTests` (Core.Test).
- **Retagged RH4106** as the table in `06-spec-author.md` says:
  - The existing IDs RH4106-001 to -007 keep their tags.
  - `VerifyDiagnosticsForPrivateFieldAndReferenceAreFixed` now also tags SHARED-CASING-007.
  - `VerifyNoDiagnosticsForUnderlineCamelCasePrivateField` now also tags SHARED-CASING-003.
  - `VerifyNoDiagnosticsForPrivateProtectedField` and `VerifyImplicitlyPrivateFieldIsDetectedAndFixed` now also tag SHARED-CASING-004.
  - `NoCodeFixForLetterlessPrivateField` now also tags SHARED-CASING-008.
  - The `FixAllCorrectsEveryDiagnostic` override now also tags SHARED-CASING-010.
  - No existing RH4106 test uses a `_` separator or an all-uppercase word, so none of them gains SHARED-CASING-006. The new `FixDerivesUnderlineCamelCaseFromTheWordsOfTheOldName` covers it.
- **Added 79 focused tests and one override.**
  - Each new test runs one spec example verbatim, with the spec's `{|RH####:…|}` markup.
  - The override is `FixAllCorrectsEveryDiagnostic` in RH4105, tagged SHARED-CASING-010. Its existing scenario is the SHARED-CASING-010 example with different containing symbols. Overriding to tag a Fix All requirement that differs from the shared one is what `sdd/README.md` allows.
- **Every spec example** of `shared/casing.md`, RH4001–RH4011, RH4101–RH4110 and RH4106 is run by a test (see *Example coverage*).

No production code, spec, or rule page changed. `scripts/format.sh` over the 24 changed test files reported `Formatted 0 of 24 file(s)` both times I ran it.

## How the examples are verified

Each type of example is verified as follows:

- **Analyzer examples:** `Verify(testCode)`.
- **Fix examples:** `Verify(testCode, fixedCode)`. The verifier applies the single fixes one after another until the diagnostics are gone, re-analyzes the result, and runs Fix All on the same input with the same expected result. So each fix example also checks one-pass convergence (FC-007) and agreement between Fix All and the single fixes (SHARED-FIX-002).
- **Withheld-fix examples:** `Verify(testCode, testCode)`, with the diagnostic marked in both.

I made sure the withheld-fix shape can fail. I temporarily removed `_value` from `NoFixIsOfferedWhenTheNewNameIsTaken`. The test then failed with "Iterative code fix application … `private int Value;` → `private int _value;`". I restored the file.

The two red tests on the first run were `RH4107…FixAllSkipsFieldsThatWouldReceiveTheSameNameAndRenamesTheOthers` and `RH4109…FixRenamesPublicFieldAndWithholdsTheFixWhenTheNewNameIsTaken`:

- In both, the fixed state expects a diagnostic that keeps no fix.
- By default, the verifier ignores `{|RH####:…|}` markup of a fixable diagnostic ID in the fixed state.
- Setting `FixedState.MarkupHandling = MarkupMode.Allow`, and `BatchFixedState` too for RH4107, makes the remaining diagnostics count. Both tests then pass with the outputs that the spec examples state.
- Both set `NumberOfFixAllIterations = -2`, like the existing RH4106 and RH4109 scenarios, because the skipped or withheld diagnostic keeps the Fix All loop running for a second pass that changes nothing.

## Tests

New tests, all in `src/Reihitsu.Analyzer.Test/Naming/<rule>…AnalyzerTests.cs`. The *Expected now* column is "pass" for every row.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH4001 `NestedAndLaterTypesAreNotComparedWithTheFileName` | RH4001-001 | `Verify(source, onConfigure)`, file `Outer.cs` | pass |
| RH4001 `DelegateDeclaredBeforeTheMatchingTypeIsComparedWithTheFileName` | RH4001-001, -004 | `Verify(source)` | pass |
| RH4001 `FileWithoutTypeIsNotReported` | RH4001-001 | `Verify(source)` | pass |
| RH4001 `TypeNameIsComparedWithTheFileNameCaseSensitively` | RH4001-002 | file `testclass.cs` | pass |
| RH4001 `GenericTypeMatchesFileNameWithTypeParametersInBraces` | RH4001-003 | file `Cache{TKey,TValue}.cs` | pass |
| RH4001 `GenericTypeIsReportedInFileNamedWithoutTypeParameters` | RH4001-003 | file `Cache.cs` | pass |
| RH4001 `NoFixIsOfferedWhenDocumentWithTheNewFileNameExists` | RH4001-006 | `Verify(source, null, onConfigure)`, two documents, fixed state = test state | pass |
| RH4001 `NoFixIsOfferedWhenDocumentWithTheNewFileNameInOtherCasingExists` | RH4001-006, FC-003 (the case-insensitive side) | same, existing `testclass.cs` | pass |
| RH4002 `PascalCaseAcceptsLettersAndDigitsAndRejectsLowercaseStartsAndSeparators` | SHARED-CASING-001, RH4002-001 | `Verify(source)` | pass |
| RH4002 `ClassesAreCheckedWhateverTheirModifiersTypeParametersOrNesting` | RH4002-001 | `Verify(source)` | pass |
| RH4002 `FixRenamesClassAndEveryReference` | RH4002-002, FC-018 | `Verify(source, fixed)` | pass |
| RH4002 `NoFixIsOfferedWhenAnotherFileOfTheNamespaceDeclaresTheNewName` | SHARED-CASING-009 | two documents, `Verify(source, source, …)` | pass |
| RH4002 `FixIsOfferedWhenAnotherFileDeclaresFileLocalTypeOfTheNewName` | SHARED-CASING-009, FC-003 (file-local, other file) | two documents | pass |
| RH4002 `FixIsOfferedWhenTheTypeOfTheNewNameHasAnotherNumberOfTypeParameters` | SHARED-CASING-009, FC-003 (arity) | two documents | pass |
| RH4002 `NoFixIsOfferedWhenFileLocalTypeOfTheNewNameIsInTheSameFile` | SHARED-CASING-009, FC-003 (file-local, same file) | `Verify(source, source)` | pass |
| RH4003 `StructsAreCheckedIncludingReadonlyRefAndGenericStructs` | RH4003-001 | `Verify(source)` | pass |
| RH4003 `FixRenamesStructAndEveryReference` | RH4003-002 | `Verify(source, fixed)` | pass |
| RH4004 `EnumsAreCheckedWhateverTheirAccessibilityAttributesOrNesting` | RH4004-001 | `Verify(source)` | pass |
| RH4004 `FixRenamesEnumAndEveryReference` | RH4004-002 | `Verify(source, fixed)` | pass |
| RH4005 `InterfaceNamesAreAcceptedOnlyAsIFollowedByPascalCase` | RH4005-001 | `Verify(source)` | pass |
| RH4005 `FixKeepsExistingPrefixAddsMissingPrefixAndKeepsPascalCaseRest` | RH4005-002 (`URL` → `IURL`, `Document_STORE` → `IDocumentStore`) | `Verify(source, fixed)` | pass |
| RH4006 `DelegatesAreCheckedWhateverTheirReturnTypeTypeParametersOrAccessibility` | RH4006-001 | `Verify(source)` | pass |
| RH4006 `FixRenamesDelegateAndEveryReference` | RH4006-002 | `Verify(source, fixed)` | pass |
| RH4007 `EachFileScopedNamespaceSegmentThatIsNotPascalCaseIsReported` | RH4007-001, SHARED-CASING-005 | `Verify(source)` | pass |
| RH4007 `PascalCaseFileScopedNamespaceIsNotReported` | RH4007-001 | `Verify(source)` | pass |
| RH4008 `EachBlockScopedNamespaceSegmentThatIsNotPascalCaseIsReported` | RH4008-001 | `Verify(source)` | pass |
| RH4008 `NamespaceSegmentsAreReportedOnTheirOwnIdentifier` | SHARED-CASING-005, RH4008-001 | `Verify(source)` | pass |
| RH4009 `NamingSectionWithoutAllowListIsInactive` | RH4009-001 | `reihitsu.json` additional file | pass |
| RH4009 `AllowListWithoutStringEntryIsInactive` | RH4009-001 (`[ 1 ]`) | same | pass |
| RH4009 `EntriesThatAreNotStringsAreIgnoredAndStringEntriesStayInEffect` | RH4009-001 (`[ "A", 1 ]`), FC-003 | same | pass |
| RH4009 `NamespacesNotExactlyOnTheListAreReportedInEveryFile` | RH4009-002 | same, plus a second file with a file-scoped namespace | pass |
| RH4010 `RecordClassesAreCheckedAndRecordStructsAreNot` | RH4010-001 | `Verify(source)` | pass |
| RH4010 `FixRenamesRecordAndEveryReference` | RH4010-002 | `Verify(source, fixed)` | pass |
| RH4011 `RecordStructsAreCheckedAndRecordClassesAreNot` | RH4011-001 | `Verify(source)` | pass |
| RH4011 `FixRenamesRecordStructAndEveryReference` | RH4011-002 | `Verify(source, fixed)` | pass |
| RH4101 `EachEnumMemberThatIsNotPascalCaseIsReported` | RH4101-001 | `Verify(source)` | pass |
| RH4101 `FixRenamesEnumMembersAndEveryReference` | RH4101-002 | `Verify(source, fixed)` | pass |
| RH4102 `EveryKindOfEventIsCheckedWithOneDiagnosticPerEvent` | RH4102-001 | `Verify(source)` | pass |
| RH4102 `ExplicitInterfaceEventImplementationIsNotReportedAndIsRenamedWithTheInterfaceEvent` | RH4102-002, FC-024 | `Verify(source, fixed)` | pass |
| RH4102 `FixRenamesFieldLikeEventAndEverySubscriptionAndInvocation` | RH4102-003 | `Verify(source, fixed)` (field-like provider) | pass |
| RH4102 `FixRenamesEventWithAccessorsAndEverySubscription` | RH4102-003 | `VerifyCustomEvent` (accessor provider) | pass |
| RH4103 `EveryKindOfMethodWhoseOriginalDefinitionIsInSourceIsChecked` | RH4103-001 | `Verify(source)` | pass |
| RH4103 `ExplicitInterfaceMethodImplementationIsNotReportedAndIsRenamedWithTheInterfaceMethod` | RH4103-002, FC-024 | `Verify(source, fixed)` | pass |
| RH4103 `FixRenamesMethodAndEveryCallTreatingUnseparatedLowercaseNameAsOneWord` | RH4103-003 | `Verify(source, fixed)` | pass |
| RH4103 `FixRenamesOverridesAndImplementationsWithTheirBase` | RH4103-003 | `Verify(source, fixed)` | pass |
| RH4103 `FixRenamesTheDeclarationAndEveryReference` | SHARED-CASING-007, RH4103-003 | `Verify(source, fixed)` | pass |
| RH4104 `LocalFunctionsAreCheckedIncludingAsyncStaticGenericAndNestedOnes` | RH4104-001 | `Verify(source)` | pass |
| RH4104 `FixRenamesNestedLocalFunctionsAndEveryCall` | RH4104-002, FC-027 (nesting) | `Verify(source, fixed)` | pass |
| RH4105 `CamelCaseAcceptsLettersAndDigitsAndRejectsUppercaseStartsAndSeparators` | SHARED-CASING-002, RH4105-001 | `Verify(source)` | pass |
| RH4105 `ParametersOfEveryKindOfMemberAreChecked` | RH4105-001 | `Verify(source)` | pass |
| RH4105 `DiscardsAndRecordPrimaryConstructorParametersAreNotChecked` | RH4105-002, FC-023 (`_`) | `Verify(source)` | pass |
| RH4105 `FixRenamesConstructorAndLambdaParametersAndEveryReference` | RH4105-003 | `Verify(source, fixed)` | pass |
| RH4105 `FixDerivesCamelCaseFromTheWordsOfTheOldName` | SHARED-CASING-006, RH4105-003 | `Verify(source, fixed)` | pass |
| RH4105 `FixRenamesToContextualKeywordsWithoutVerbatimPrefix` | SHARED-CASING-007, RH4105-003, FC-023 (`Var` → `var`, `Nameof` → `nameof`, `Await` → `await` outside async) | `Verify(source, fixed)` | pass |
| RH4105 `NoFixIsOfferedWhenNoUsableCamelCaseNameCanBeDerived` | SHARED-CASING-008, FC-023 (`_1st`, `__`) | `Verify(source, source)` | pass |
| RH4105 `FixAllCorrectsEveryDiagnostic` (override) | SHARED-CASING-010 (different containing symbols) | `BatchCodeFixTestsBase` | pass |
| RH4106 `UnderlineCamelCaseAcceptsBareUnderscoreAndRejectsOtherPrefixesAndUppercaseOrDigitStarts` | SHARED-CASING-003, RH4106-002 | `Verify(source)` | pass |
| RH4106 `FieldsWithoutAccessibilityModifierBelongToThePrivateGroupOutsideInterfaces` | SHARED-CASING-004, RH4106-001, FC-002 | `Verify(source)` | pass |
| RH4106 `PrivateFieldsThatAreNotConstantsAreChecked` | RH4106-001 | `Verify(source)` | pass |
| RH4106 `CheckedFieldsThatAreNotUnderlineCamelCaseAreReported` | RH4106-002 | `Verify(source)` | pass |
| RH4106 `EachNonConformingDeclaratorIsReportedOnItsIdentifier` | RH4106-003 | `Verify(source)` | pass |
| RH4106 `FixRenamesImplicitlyPrivateAndStaticReadonlyFieldsToUnderlineCamelCase` | RH4106-004 | `Verify(source, fixed)` | pass |
| RH4106 `FixDerivesUnderlineCamelCaseFromTheWordsOfTheOldName` | SHARED-CASING-006, RH4106-004 (`iPhone` → `_iPhone`, `URL` → `_url`) | `Verify(source, fixed)` | pass |
| RH4106 `FixRenamesFieldReferencedInNameof` | SHARED-CASING-007, RH4106-005, FC-036 | `Verify(source, fixed)` | pass |
| RH4106 `NoFixIsOfferedForFieldWithoutLetterOrDigit` | RH4106-006 | `Verify(source, source)` | pass |
| RH4106 `NoFixIsOfferedWhenTheDerivedNameIsTheOldName` | SHARED-CASING-008, RH4106-006 | `Verify(source, source)` | pass |
| RH4106 `NoFixIsOfferedWhenTheNewNameIsTaken` | SHARED-CASING-009, RH4106-006 | `Verify(source, source)` | pass |
| RH4107 `FieldsOfTheProtectedGroupThatAreNotConstantsAreChecked` | RH4107-001 | `Verify(source)` | pass |
| RH4107 `FixRenamesEveryDeclaratorOfAProtectedFieldAndEveryReference` | RH4107-002, FC-008 (two declarators of one declaration) | `Verify(source, fixed)` | pass |
| RH4107 `FixAllSkipsFieldsThatWouldReceiveTheSameNameAndRenamesTheOthers` | SHARED-CASING-010, RH4107-002 | `Verify(source, fixed, …)` with `BatchFixedCode` | pass |
| RH4108 `AccessibilityGroupIsDecidedByTheModifiersOfTheField` | SHARED-CASING-004, RH4108-001 | `Verify(source)` | pass |
| RH4108 `FieldsOfTheInternalGroupThatAreNotConstantsAreChecked` | RH4108-001 | `Verify(source)` | pass |
| RH4108 `FixRenamesInternalFieldsAndEveryReference` | RH4108-002 | `Verify(source, fixed)` | pass |
| RH4109 `ContainingTypeDoesNotChangeTheAccessibilityGroup` | SHARED-CASING-004, RH4109-001 | `Verify(source)` | pass |
| RH4109 `FieldsOfThePublicGroupThatAreNotConstantsAreChecked` | RH4109-001 | `Verify(source)` | pass |
| RH4109 `FixRenamesPublicFieldAndWithholdsTheFixWhenTheNewNameIsTaken` | RH4109-002, SHARED-CASING-009 | `Verify(source, fixed, …)` | pass |
| RH4110 `ConstantsOfEveryAccessibilityAreChecked` | RH4110-001 | `Verify(source)` | pass |
| RH4110 `FixRenamesConstantAndEveryReferenceInAnotherInitializer` | RH4110-002 | `Verify(source, fixed)` | pass |
| RH4110 `FixDerivesPascalCaseFromTheWordsOfTheOldName` | SHARED-CASING-006, RH4110-002 | `Verify(source, fixed)` | pass |
| RH4111 `ExplicitInterfacePropertyImplementationBelongsToNoAccessibilityGroup` | SHARED-CASING-004 | `Verify(source)` | pass |

Existing tests that were tagged:

- **RH4001–RH4011, RH4101–RH4110:**
  - Report tests that also verify a fix tag the rule's report requirement and fix requirement (`-001` and `-002`, or `-003` for RH4102, RH4103 and RH4105).
  - No-diagnostic tests tag the report requirement.
  - The RH4001 file-name tests tag -001 to -006 by what they assert.
  - `CodeFixNotOfferedWhenTargetFileAlreadyExists` tags RH4001-006.
  - The RH4009 configuration tests tag -001; the namespace tests tag -002.
  - The RH4007 and RH4008 segment tests also tag SHARED-CASING-005.
  - `NoCodeFixForLetterlessMethodParameter` tags SHARED-CASING-008.
  - RH4102 `VerifyExplicitInterfaceEventImplementationIsExcluded` and RH4103 `VerifyNoDiagnosticsForExplicitInterfaceImplementation` tag -002.
  - The modifier-group tests of RH4106 to RH4109 also tag SHARED-CASING-004: implicitly private, `private protected`, and `protected internal`.
- **RH4115:**
  - The ten `TopLevelType*` and `NamedNamespaceType*` cross-document RH4002 tests tag SHARED-CASING-009.
  - `DuplicateNormalizedTargetsInSiblingScopesRemainFixableTogether` tags SHARED-CASING-010. It is the RH4115 sibling-scope example of SHARED-CASING-010.
- **Core.Test:** `ToPascalCaseTest`, `ToCamelCaseTest` and `ToUnderlineCamelCaseTest` tag SHARED-CASING-006. The `FName` row of `ToCamelCaseTest` (→ `fName`) is outside the coverage of SHARED-CASING-006: its first word starts with two uppercase letters, which is the domain of thoenissen/Reihitsu#971. It stays as a characterization row. Every other row is inside the coverage.

The `FixAllCorrectsEveryDiagnostic` base tests of the other rules are not overridden. Their scenarios carry only the shared Fix All requirements. Each rule's fix example has its own test instead.

## Example coverage

- Spec examples run by a new test: every example of the 20 rule specs, and every example of `shared/casing.md`, except the three listed below. RH4106: the examples of -001, -002, -003, -004 and -006, and the `Value`/`_value` sentence of -006.
- Examples run by an existing test with the same code inside a namespace wrapper or under another class name:
  - RH4001-001 `Mismatched.cs`: `LaterSiblingNamespaceTypeIsDetectedAndFileIsRenamed`.
  - RH4001-002 (`Test0.cs`, `.razor.cs`, `.xaml.cs`, `.Part1.cs`, the migration prefix, `A2345678901234_Something`, `SomeMigration`): the existing file-name tests.
  - RH4001-005 (`TestClass.cs`, `TestComponent.razor.cs`, `TestComponent.Part1.cs`, `TestClass{T}.cs`): the existing rename tests.
  - RH4106-005: `VerifyDiagnosticsForPrivateFieldAndReferenceAreFixed`.
  - RH4106-007: the RH4106 Fix All scenario.
  - The SHARED-CASING-010 RH4105 example: the RH4105 Fix All scenario.
  - The SHARED-CASING-010 RH4115 example: `DuplicateNormalizedTargetsInSiblingScopesRemainFixableTogether`.
- Test seeds from `06-spec-author.md` that are covered:
  - `Var`, `Nameof`, and `Await` outside an async member;
  - `nameof(Value)` → `nameof(_value)`;
  - `URL` → `IURL` and `Document_STORE` → `IDocumentStore`;
  - `{ "naming": { } }`, `[ 1 ]` and `[ "A", 1 ]`;
  - the anonymous-method parameter `Size`;
  - overrides and implicit implementations of source events and source methods.

## Not covered, and why

- **Inputs of the open questions in `06-spec-author.md`:** `06-spec-author.md` says not to write tests for them, and the specs carve them out. These are:
  - shared Q1, a namespace type renamed into CS0542 or CS0694;
  - shared Q2, keyword targets that become `@class`, `@object`, `@string` and `@await`;
  - shared Q3, combining marks and letter numbers;
  - shared Q4, a reference in inactive `#if` code (FC-020);
  - RH4005 Q1, the doubled `I` and `_`/`__`;
  - RH4001 Q1–Q3, verbatim names, the fix for a migration prefix, and top-level statements;
  - RH4009 Q1–Q2, nested namespaces, and trivia or `@` in the name;
  - RH4103 Q1–Q2, a referenced-assembly original definition and P/Invoke;
  - RH4102 Q1, a referenced-assembly original definition.
- **A mixed-kind RH4102 Fix All** (a field-like event and an event with accessors in one document): this is the bug candidate in `06-spec-author.md`, which the maintainer has not decided. Each kind is tested with its own provider only.
- **The leading uppercase run** (`_hTTPClient`, `XValue`): thoenissen/Reihitsu#971 owns it.
- **FC-019, FC-020 and FC-021 (comments and directives in an edited gap):** no fix in scope joins lines or edits a token gap. Every fix is a `Renamer.RenameSymbolAsync` call through `CasingCodeFixProviderBase<T>` or a document rename (RH4001). The inactive-`#if` case is shared Q4, and the shared limitation lives in `documentation/rules/general-notes.md`.
- **Line-ending variants (CRLF):** no fix in scope changes layout. The renamer replaces identifier tokens only, and RH4001 renames the document without changing its text (RH4001-005). So the role's rule "line endings are tested separately when layout is involved" does not apply.
- **FC-010 to FC-014 (formatter stability and parity):** no formatter phase renames identifiers, and no rule is new (`03-skeptic.md`, *Surfaces*).
- **FC-024, partial types and members with two diagnostics:** no requirement of these specs states the partial-type behavior. `03-skeptic.md` derived it from the code but did not run it, and no spec example shows it. Explicit interface implementations and `file` types are covered.
- **FC-006 (language-version gates, operators, target typing):** a rename changes no expression. A primary constructor, an operator parameter, `record struct` and `file class` occur in the examples and compile under `LanguageVersion.Latest`.
- **FC-035:** every expected value comes from a spec example, or from the 06 handover's seeds with the output stated there. None comes from running a patched copy of the code.
- **Existing RH4111 tests:** RH4111 has no spec yet, so its tests stay untagged. The SHARED-CASING-004 example is run by the new RH4111 test.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Naming"
scripts/test.sh --project core --filter "FullyQualifiedName~Reihitsu.Core.Test.CasingUtilitiesTests"
scripts/test.sh --project architecture --filter "FullyQualifiedName~Sdd"
scripts/test.sh --project analyzer --filter "TestCategory=SHARED-CASING-009"
```

Results on this head:

- Naming tests: 402 passed.
- `Sdd` architecture tests: 39 passed, including `EveryRequirementOfAnEnforcedSpecIsCovered`, `SpecConsistencyTests` and `SpecStructureTests`.
- Full analyzer project: 4272 passed, 0 failed.
- Core project: 527 passed, 0 failed.

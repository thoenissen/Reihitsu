Role: test implementer
Commit: 74aeaef
Verdict: tests written

Lane: specs and tests for existing behavior. No production code was touched. Every test below passes on the unchanged analyzers at `74aeaef`. My prompt carried no intended fix, suspected cause, or hoped-for verdict, so there was nothing to ignore. I worked from `00-lead.md`, `02-skeptic.md`, `03-spec-author.md`, `sdd/specs/shared/element-documentation.md`, `sdd/specs/analyzer/RH8001.md`–`RH8019.md`, and `sdd/specs/analyzer/RH8107.md`.

## Tests

All tests are in `src/Reihitsu.Analyzer.Test/Documentation/RH80nn…AnalyzerTests.cs`. They use the `AnalyzerTestsBase<TAnalyzer>.Verify` helper with markup locations, so every diagnostic test asserts the exact span. `Rnn` below means `RH80nn`.

### Tags on existing tests

| Existing tests | Tags |
|---|---|
| Every diagnostic test in R01–R19 | `Rnn-001`, `Rnn-002` |
| Every no-diagnostic test in R01–R19 | `Rnn-001` |
| `…WithRemarksButNoSummary`, `…WithoutDocumentation` of a non-private rule, `…WithSummary`, `…WithInheritdoc`, `VerifyNoDiagnosticForAllMembersDocumentedInInternalEnum`, and the single-part partial tests (`VerifyNoDiagnosticForPartialClassWithSummary`, `VerifyNoDiagnosticForPartialRecordWithSummary`) | also `SHARED-DOC-005` |
| Group tests: no modifier at namespace scope; `private protected`; `protected internal` and `protected` nested; explicit `private` (private rules); and the other group not reported (`…PurePrivateNested…`, `…InternalNested…`, `…UndocumentedPrivateDelegate`, `…UndocumentedInternalDelegate`) | also `SHARED-DOC-002` |
| Nested in a documented container, or nested in a private class (`…NestedInsidePrivateClass`) | also `SHARED-DOC-001` |
| Enum-member group tests in R13/R14 (non-private, protected, and implicit-internal enum; private and deeply nested private enum; the member of the other group not reported) | also `SHARED-DOC-003`; `VerifyDiagnosticForEnumMemberInNonPrivateEnumWithoutDocumentation` (undocumented enum) also `SHARED-DOC-001` |
| `VerifyNoDiagnosticsWhenDocumentationModeIsNone` in R01–R19 | also `SHARED-DOC-008` |
| `RH8107…Tests.VerifyNoDiagnosticsWhenDocumentationModeIsNone` (already tagged `RH8107-004`) | added `SHARED-DOC-008` (RH8107 lists it under `Uses`) |

### New tests

Each new test passes on the unchanged code ("Expected now": pass).

| Test | Requirement / failure class | Helper |
|---|---|---|
| R01 `VerifyDiagnosticForStaticAbstractAndSealedClassesWithoutDocumentation` | RH8001-001/002 | `Verify` |
| R01 `VerifyDiagnosticForClassWithVerbatimIdentifierCoversTheAtSign` | RH8001-002, FC-023 | `Verify` |
| R01 `VerifyDiagnosticForClassAtFileLevelWithoutNamespace` | RH8001-001, SHARED-DOC-001 | `Verify` |
| R01 `VerifyDiagnosticForFileLocalClassWithoutDocumentation` | SHARED-DOC-002, FC-024 (`file` type) | `Verify` |
| R01 `VerifyDiagnosticForUndocumentedClassNestedInUndocumentedClass` | SHARED-DOC-001 | `Verify` |
| R01 `VerifyDiagnosticForPublicClassNestedInPrivateClass` | SHARED-DOC-002 (container accessibility) | `Verify` |
| R01 `VerifyDiagnosticForClassWithoutModifierNestedInInterface` | SHARED-DOC-001/002 (interface arm) | `Verify` |
| R01 `VerifyDiagnosticForClassesNestedInStructRecordAndRecordStruct` | SHARED-DOC-001 | `Verify` |
| R01 `VerifyDiagnosticForClassDocumentedAfterAttributeList` / `VerifyNoDiagnosticForClassDocumentedBeforeAttributeList` | SHARED-DOC-004, FC-003 | `Verify` |
| R01 `VerifyNoDiagnosticForDocumentationFollowedBy{OrdinaryComment,BlockComment,ConditionalDirective,RegionDirective,PragmaDirective,DisabledText}` | SHARED-DOC-004, FC-005/019/020/021 (skeptic D1–D6) | `Verify` |
| R01 `VerifyDiagnosticForClassWith{SummaryNestedInRemarks,InheritdocNestedInRemarks,PlainTextDocumentation}` | SHARED-DOC-005 | `Verify` |
| R01 `VerifyNoDiagnosticForClassWithEmptySummaryElement`, `…WithBlockDocumentationComment` | SHARED-DOC-005 (and 004 for `/** */`) | `Verify` |
| R01 `VerifyNoDiagnosticForClassWithSummaryInLaterDocumentationComment` / `VerifyDiagnosticForClassWhenNoDocumentationCommentHasSummary` | SHARED-DOC-006, blank-line shape (D9) only | `Verify` |
| R01 `VerifyNoDiagnosticForClassWithSummaryInLaterDocumentationCommentInParseMode` | SHARED-DOC-006 + 008, FC-017 | `Verify` + Parse |
| R01 `VerifyNoDiagnosticForClassWith{SummaryFromIncludedFile,InheritdocFromIncludedFile}`; `VerifyDiagnosticForClassWith{IncludedFileWithoutSummary,IncludedSummaryNestedInRemarks,UnresolvedIncludedFile}` | SHARED-DOC-007, both sides | `Verify` + include |
| R01 `VerifyNoDiagnosticForClassWithSummaryFromIncludedFileInParseMode` | SHARED-DOC-007 + 008 | `Verify` + Parse + include |
| R01 `VerifyDiagnosticForUndocumentedClassInParseMode` | SHARED-DOC-008, Parse side | `Verify` + Parse |
| R01 `VerifyNoDiagnosticForRecordDeclarations` | RH8001-001 (record / record class excluded) | `Verify` |
| R02 `…PrivateStaticAbstractAndSealedClasses…`, `…GenericClassWithoutModifierInClass`, `…ClassWithoutModifierInStruct`, `…ClassesWithoutModifierInRecordAndRecordStruct` | RH8002-001/002, SHARED-DOC-002 (implicit-private arm, every container kind) | `Verify` |
| R02 `VerifyNoDiagnosticForPrivateProtectedNestedClass`, `VerifyNoDiagnosticForPrivateRecordDeclarations` | SHARED-DOC-002, RH8002-001 | `Verify` |
| R03 `VerifyDiagnosticForReadonlyAndRefStructsWithoutDocumentation`, `VerifyNoDiagnosticForRecordStructDeclaration` | RH8003-001/002 | `Verify` |
| R04 `VerifyDiagnosticForGenericReadonlyStructWithoutModifierInClass`, `VerifyNoDiagnosticForPrivateRecordStructDeclaration` | RH8004-001/002, SHARED-DOC-002 | `Verify` |
| R05 `VerifyDiagnosticForGenericInterfaceWithoutModifier`, `VerifyDiagnosticForInterfaceWithoutModifierNestedInInterface` | RH8005-001/002, SHARED-DOC-001/002 | `Verify` |
| R06 `VerifyDiagnosticForGenericInterfaceWithoutModifierInClass` | RH8006-001/002, SHARED-DOC-002 | `Verify` |
| R07 `VerifyDiagnosticForRecordClassWithoutDocumentation`, `VerifyNoDiagnosticForClassAndRecordStructDeclarations` | RH8007-001/002 | `Verify` |
| R08 `…PositionalPrivateRecord…`, `…GenericRecordClassWithoutModifierInClass`, `VerifyNoDiagnosticForPrivateClassAndRecordStructDeclarations` | RH8008-001/002 (parameter list excluded from span), SHARED-DOC-002 | `Verify` |
| R09 `VerifyDiagnosticForReadonlyRecordStructWithoutDocumentation`, `VerifyNoDiagnosticForStructAndRecordDeclarations` | RH8009-001/002 | `Verify` |
| R10 `…PositionalPrivateRecordStruct…`, `…GenericReadonlyRecordStructWithoutModifierInClass`, `VerifyNoDiagnosticForPrivateStructAndRecordDeclarations` | RH8010-001/002, SHARED-DOC-002 | `Verify` |
| R11 `VerifyDiagnosticForEnumWithUnderlyingTypeWithoutDocumentation` | RH8011-002 (underlying type excluded) | `Verify` |
| R12 `VerifyDiagnosticForEnumWithoutModifierAndWithUnderlyingTypeInClass` | RH8012-001/002, SHARED-DOC-002 | `Verify` |
| R13 `VerifyDiagnosticForEnumMemberWithValueWithoutDocumentation` | RH8013-002 (value excluded) | `Verify` |
| R13 `…EnumMemberWithSummaryNestedInRemarks`, `…EmptySummaryElement`, `…BlockDocumentationComment` | SHARED-DOC-005 on the enum-member path | `Verify` |
| R13 `…EnumMemberDocumentedAfterAttributeList` / `…BeforeAttributeList`, `…FollowedByConditionalDirective`, `…FollowedByOrdinaryComment` | SHARED-DOC-004 on the enum-member path (first token is the name) | `Verify` |
| R13 `…EnumMemberWithSummaryInLaterDocumentationComment` / `…WhenNoDocumentationCommentHasSummary` | SHARED-DOC-006 on the enum-member path | `Verify` |
| R13 `…EnumMemberWithSummaryFromIncludedFile` / `…WithIncludedFileWithoutSummary` | SHARED-DOC-007 on the enum-member path | `Verify` + include |
| R13 `VerifyDiagnosticForUndocumentedEnumMemberInParseMode` | SHARED-DOC-008, Parse side | `Verify` + Parse |
| R14 `VerifyDiagnosticForEnumMemberWithValueInPrivateEnum`, `VerifyDiagnosticForMemberOfEnumWithoutModifierInClass` | RH8014-001/002, SHARED-DOC-003 | `Verify` |
| R16 `VerifyDiagnosticForGenericDelegateWithoutModifierInClass` | RH8016-001/002, SHARED-DOC-002 | `Verify` |
| R17 `…PublicAndProtectedInternalConstructorsWithoutSummary`, `…PrivateProtectedConstructor…`, `…ConstructorsOfStructRecordAndRecordStruct` | RH8017-001/002, SHARED-DOC-002/005 | `Verify` |
| R17 `VerifyNoDiagnosticForDocumentedConstructor`, `…PrivateAndStaticConstructors` (class static ctor), `…PrimaryConstructor` | RH8017-001, SHARED-DOC-002/005 | `Verify` |
| R18 `…ConstructorWithoutModifierWithoutSummary`, `…StaticConstructorsOfClassStructRecordAndRecordStruct`, `…ConstructorWithoutModifierInStruct`, `VerifyNoDiagnosticForDocumentedPrivateConstructor`, `VerifyNoDiagnosticForPublicConstructor` | RH8018-001/002, SHARED-DOC-002/005, FC-024 (static ctors) | `Verify` |
| R19 `VerifyDiagnosticForDestructorWithoutSummaryInPrivateNestedClass`, `VerifyNoDiagnosticForDocumentedDestructor` | RH8019-001/002, SHARED-DOC-004 (first token `~`), SHARED-DOC-005 | `Verify` |

### Test infrastructure

- `src/Reihitsu.Analyzer.Test/Base/AnalyzerTestsBase{TAnalyzer}.cs` gains two methods:
  - `ApplyDocumentationModeParseToTestProject`;
  - `CreateIncludedDocumentationTransform(path, content)`, which sets the compilation's `XmlReferenceResolver`.
- `src/Reihitsu.Analyzer.Test/Base/InMemoryXmlReferenceResolver.cs` is new. It resolves only the files it is given; any other path resolves to `null`, as a missing file does.
- The malformed-XML tests keep their own local Parse helpers; I did not change them.

### Evidence that the tests can fail

- **Parse transform.** I temporarily swapped the Parse transform to `None`. `VerifyDiagnosticForUndocumentedClassInParseMode` and `VerifyDiagnosticForUndocumentedEnumMemberInParseMode` then failed, as they should. I restored the file afterwards (the diff shows only the additions).
- **Include resolver.** The tests that expect no diagnostic for an included summary (`…SummaryFromIncludedFile…`) can only pass if the resolver really supplies the file. Without the file, the analyzer would report.

## Not covered, and why

- **Open questions (thoenissen/Reihitsu#1037).** None of these cases is tagged or asserted:
  - documentation comments separated by a comment, directive, or disabled text (D8, L1–L6);
  - malformed XML: the existing tests `VerifyNoDiagnosticForClassWithMalformedAmpersandInDocumentation`, `VerifyNoDiagnosticForClassWithUnclosedTagInDocumentation` (RH8001) and `VerifyNoDiagnosticForEnumMemberWithMalformedAmpersandInDocumentation` (RH8013) stay untagged;
  - partial types and constructors in several parts;
  - the static constructor of an interface;
  - upper-case tag names: the Core test `XmlDocumentationExpanderTests.HasTagReturnsTrueForCaseInsensitiveDirectFullElement` stays untagged.
- **Single-part `partial` declarations.** The two existing tests on these are tagged. A partial type with one part is a single declaration, which the coverage sentences of SHARED-DOC-001 and SHARED-DOC-005 include.
- **Shared behavior tested on one representative rule.**
  - SHARED-DOC-004 to SHARED-DOC-007 qualifiers are tested on RH8001, plus RH8013 for the enum-member overload. They are not repeated for every type rule.
  - Static proof: `rg -n "HasRequiredDocumentation\(|GetDiagnosticLocation\(|MatchesAccessibilityGroup\(" src/Reihitsu.Analyzer/Base/SplitElementDocumentationAnalyzerBase.cs src/Reihitsu.Analyzer/Rules/Documentation/RH801[34]*.cs` shows two call sites.
    - The 17 rules RH8001–RH8012 and RH8015–RH8019 reach `DocumentationAnalysisUtilities.HasRequiredDocumentation(MemberDeclarationSyntax, …)` through the single `SplitElementDocumentationAnalyzerBase.OnDeclaration`.
    - RH8013/RH8014 reach the separate `HasRequiredDocumentation(EnumMemberDeclarationSyntax, …)` overload. That overload got its own tests.
  - Both overloads read the comment through the same `DirectDocumentationSyntaxChecker.GetDocumentationComment(SyntaxNode)`.
- **SHARED-DOC-008 `Parse` side.** It is tested on RH8001 and RH8013 only.
  - Static proof: every analyzer of this batch registers through `DocumentationModeAnalysisContextExtensions.RegisterSyntaxNodeActionWithDocumentationModeCheck`, which checks only for `DocumentationMode.None`.
  - Every rule keeps its own `None` test.
- **Verbatim identifier (FC-023).** It is tested once (RH8001).
  - Static proof: every rule's location comes from `DocumentationAnalysisUtilities.GetDiagnosticLocation`, which returns `Identifier.GetLocation()` for every kind of this batch.
- **Implicit-private arm.** It is tested for a class, struct, record, and record struct container (RH8002), and for a class container in the other private rules.
  - Static proof: `HasImplicitPrivateAccessibility` is one `TypeDeclarationSyntax` arm after the `InterfaceDeclarationSyntax` arm.
- **FC-004, 006–012, 018, 022, 025–030, 036.** Not applicable: these rules have no code-fix provider and no formatter counterpart, and the lane has no production change.
- **FC-013/014 and line endings.** The skeptic established that no formatter phase owns this concern. These rules only read trivia kinds, which do not depend on LF or CRLF, so no CRLF variant was added.
- **Records' finalizers and enum members of `file` enums.** Not added. They reach the same arms as the tested cases (`DestructorDeclaration`; `IsPurePrivateDeclaration` on the enum).

## Commands

```shell
scripts/test.sh --project analyzer --configuration Debug --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Documentation.RH80"
scripts/test.sh --project analyzer --configuration Debug --filter "FullyQualifiedName~RH8107"
scripts/test.sh --project analyzer --configuration Debug --filter "TestCategory=SHARED-DOC-004"
scripts/test.sh --project architecture --configuration Debug --filter "FullyQualifiedName~Sdd"
```

Results on this tree:

- Documentation RH80* filter: 287 passed.
- RH8107: 18 passed.
- Architecture `Sdd`: 39 passed, including `RequirementCoverageTests.EveryRequirementOfAnEnforcedSpecIsCovered`.
- Full architecture project: 72 passed.
- Full analyzer project, first run: 4456 passed and 1 failed. `SelfHostingTests.AnalyzersReportNoViolationsOnOwnSourceCode` reported RH7401 on the `Equals`/`GetHashCode` overrides of the new `InMemoryXmlReferenceResolver`. I moved both into an `Object` region. The re-run with `--filter "FullyQualifiedName~SelfHosting|FullyQualifiedName~IncludedFile"` then passed 24 of 24.

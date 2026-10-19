Role: test implementer
Commit: c979775
Verdict: tests written

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Lane: specs and tests for existing behavior. No production code was touched. Every test passes on the unchanged code, and no test produced a finding against a spec's stated behavior. One fact for the spec checker is under "Findings".

All changes are in `src/Reihitsu.Analyzer.Test/Formatting/RH54{09,10,…,17}*AnalyzerTests.cs`:

- Existing tests were tagged with `[Requirement]`.
- New tests were added inside `#region Tests`.
- RH5412–RH5416 gained a `#region Methods` with `ApplyLanguageVersion`. Existing tests were not changed beyond their tags.
- `scripts/format.sh` changed nothing (0 of 9 files).

## Tests

"Expected now" is pass for every row.

### Existing tests, tagged

| Test (class) | Requirement |
|---|---|
| RH5409 `VerifyTrailingCommaOnFinalEnumMemberIsDetectedAndFixed`, `VerifyTrailingCommaOnSingleEnumMemberIsDetectedAndFixed` | SHARED-TRAIL-001, SHARED-TRAIL-003, RH5409-001 |
| RH5409 `VerifyTrailingCommaWithSameLineCommentIsDetectedAndFixed`, `VerifyTrailingCommaOnOwnLineAfterCommentIsDetectedAndFixed`, `VerifyTrailingCommaIsRemovedSurgically` | SHARED-TRAIL-003 |
| RH5409 `VerifyMultipleEnumsWithTrailingCommasAreDetected` | SHARED-TRAIL-001, RH5409-001 |
| RH5409 `VerifyEnumWithoutTrailingCommaIsNotFlagged`, `VerifyEmptyEnumIsNotFlagged` | SHARED-TRAIL-001 |
| RH5409 `VerifyEnumWithConditionalBlockAfterFinalMemberIsNotFlagged` | SHARED-TRAIL-002 |
| RH5410 `VerifyTrailingCommaOnFinalExplicitArrayItemIsDetectedAndFixed`, `VerifyTrailingCommaOnImplicitArrayInitializerIsRemovedSurgically` | SHARED-TRAIL-001, SHARED-TRAIL-003, RH5410-001 |
| RH5410 `VerifyTrailingCommaWithSameLineCommentIsDetectedAndFixed` | SHARED-TRAIL-003, RH5410-001 |
| RH5410 `VerifyTrailingCommaOnOwnLineAfterCommentIsDetectedAndFixed` | SHARED-TRAIL-003 |
| RH5410 `VerifyArrayWithoutTrailingCommaIsNotFlagged`, `VerifyEmptyArrayInitializerIsNotFlagged` | SHARED-TRAIL-001 |
| RH5410 `VerifyArrayWithConditionalBlockAfterFinalItemIsNotFlagged` | SHARED-TRAIL-002 |
| RH5410 `VerifyArrayWithRegionDirectiveAfterFinalItemIsStillFlagged` | SHARED-TRAIL-002, SHARED-TRAIL-003 |
| RH5410 `VerifyTrailingCommaOnCollectionInitializerIsNotFlagged`, `VerifyTrailingCommaOnObjectInitializerIsNotFlagged` | RH5410-002 |
| RH5411 `VerifyTrailingCommaOnCollectionInitializerIsDetectedAndFixed` | SHARED-TRAIL-001, SHARED-TRAIL-003, RH5411-001 |
| RH5411 `VerifyTrailingCommaOnCollectionInitializerAssignmentIsRemovedSurgically` | SHARED-TRAIL-003, RH5411-001 |
| RH5411 `VerifyTrailingCommaWithSameLineCommentIsDetectedAndFixed` | SHARED-TRAIL-003 |
| RH5411 `VerifyCollectionInitializerWithoutTrailingCommaIsNotFlagged`, `VerifyEmptyCollectionInitializerIsNotFlagged` | SHARED-TRAIL-001 |
| RH5411 `VerifyCollectionWithConditionalBlockAfterFinalItemIsNotFlagged` | SHARED-TRAIL-002 |
| RH5411 `VerifyTrailingCommaOnArrayInitializerIsNotFlagged`, `VerifyTrailingCommaOnObjectInitializerIsNotFlagged` | RH5411-002 |
| RH5412–RH5416 `VerifyEmpty<Kind>IsDetectedAndFixed` | SHARED-EMPTY-001, SHARED-EMPTY-003, RH54nn-001 |
| RH5412–RH5416 `VerifySemicolon<Kind>IsNotFlagged` | SHARED-EMPTY-002, RH54nn-001 |
| RH5412–RH5414 `VerifyUnsupportedLanguageVersionIsNotFlagged` (C# 11) | RH54nn-001 |
| RH5412 `VerifyCommentedEmptyClassIsReportedWithoutCodeFix`, `VerifyTrailingHeaderCommentEmptyClassIsReportedWithoutCodeFix`; RH5412–RH5416 `VerifyLeadingBraceCommentEmpty<Kind>IsReportedWithoutCodeFix` | SHARED-EMPTY-001, SHARED-EMPTY-004 |
| RH5412 `VerifyBlankLineAboveFixedClassInFileScopedNamespaceIsKept` | SHARED-EMPTY-003 |
| RH5417 `VerifyEmptyMethodBodyIsDetectedAndFixed`, `VerifyPopulatedMethodBodyIsDetectedAndFixed`, `VerifyConstructorBodyIsDetectedAndFixed`, `VerifyDestructorBodyIsDetectedAndFixed`, `VerifyPropertyGetAccessorBodyIsDetectedAndFixed`, `VerifySingleLineBlockAccessorsAreFixedOntoSeparateLines` | RH5417-001, RH5417-003 |
| RH5417 `VerifyOperatorBodyIsDetected`, `VerifyConversionOperatorBodyIsDetected`, `VerifyIndexerAccessorBodyIsDetected`, `VerifySingleLineLocalFunctionProducesDiagnostic`, `VerifyLambdaAndAnonymousMethodBodiesProduceDiagnostics` | RH5417-001 |
| RH5417 `VerifyPropertySetAccessorBodyIsDetected`, `VerifyEventAccessorBodyIsDetected` | RH5417-001, RH5417-002 |
| RH5417 `VerifyNoDiagnosticsWhenMethodBodyIsAllman`, `VerifyExpressionBodied{Method,Property,Accessor}DoesNotProduceDiagnostics`, `VerifyAutoPropertyDoesNotProduceDiagnostics`, `VerifyCatchBodyDoesNotProduceDiagnostic`, `VerifySingleLineControlFlowBlockDoesNotProduceDiagnostics` | RH5417-002 |

### Existing tests, deliberately left untagged

These are characterization tests of open questions, as `03-spec-author.md` asks:

- `RH5412…VerifyBlankLineAboveCommentOfFixedClassInFileScopedNamespaceIsInserted` (question 2).
- `RH5416…VerifyDirectiveContainingRecordStructIsReportedWithoutCodeFix` (question 4).

### New tests

Helpers used:

- `Verify(source, fixed)`: the markup verifier, with incremental and Fix All iterations.
- `Verify(source)`: analyzer only.
- `VerifyNoCodeFixIsOffered`: diagnostics, plus no action registered for any of them.
- `ApplyCodeFixAsync` + `Assert.AreEqual`: raw text, so CRLF is compared exactly.

| Test | Requirement / failure class | Helper |
|---|---|---|
| RH5409 `VerifySpecificationExampleReportsOnlyTheCommaAfterTheLastMember` | SHARED-TRAIL-001, RH5409-001 (spec example) | Verify(source, fixed) |
| RH5409 `VerifySpecificationExampleReportsCommaInFrontOfRegionButNotInFrontOfConditionalBlock` | SHARED-TRAIL-002 (spec example) | Verify(source, fixed) |
| RH5409 `VerifyCommaInFrontOfPragmaDirectivesIsReportedAndFixed` | SHARED-TRAIL-002, SHARED-TRAIL-003; FC-003, FC-020 (`#pragma` side; first test of it) | Verify(source, fixed) |
| RH5409 `VerifyCommaInFrontOfEmptyConditionalBlockIsNotReported` | SHARED-TRAIL-002; FC-003 (directive with no excluded code) | Verify(source) |
| RH5409 `VerifyCommaInFrontOfElseAndElifBranchesIsNotReported` | SHARED-TRAIL-002; FC-020 (`#elif`/`#else`) | Verify(source) |
| RH5409 `VerifyCommaBehindConditionalBlockIsReportedAndFixed` | SHARED-TRAIL-002, SHARED-TRAIL-003; FC-001/FC-003 (directive before the comma, outside the gap; comma-only line removed) | Verify(source, fixed) |
| RH5409 `VerifySpecificationExampleOfCommaRemoval` | SHARED-TRAIL-003 (spec example) | Verify(source, fixed) |
| RH5409 `VerifyCommaOnlyLineIsRemovedWithCarriageReturnLineFeeds` | SHARED-TRAIL-003; FC-010 | ApplyCodeFixAsync, CRLF |
| RH5409 `VerifySpecificationExampleOfMembersWithValuesAttributesAndOneLineEnum` | RH5409-001 (spec example) | Verify(source, fixed) |
| RH5410 `VerifySpecificationExampleOfArrayInitializersIsReportedAndFixed` | SHARED-TRAIL-001, SHARED-TRAIL-003, RH5410-001 (spec example; all 5 dispatch kinds, 7 diagnostics) | Verify(source, fixed) |
| RH5410 `VerifyInnerAndOuterTrailingCommasOfMultiDimensionalArrayAreReportedAndFixed` | RH5410-001; FC-008 (nested diagnostics in one initializer) | Verify(source, fixed) |
| RH5410 `VerifySpecificationExampleOfOtherInitializersIsNotReported` | RH5410-002 (spec example) | Verify(source) |
| RH5411 `VerifySpecificationExampleOfCollectionInitializersIsReportedAndFixed` | SHARED-TRAIL-001, SHARED-TRAIL-003, RH5411-001 (spec example) | Verify(source, fixed) |
| RH5411 `VerifySpecificationExampleOfOtherInitializersIsNotReported` | RH5411-002 (spec example, index initializer) | Verify(source) |
| RH5412 `VerifySpecificationExampleOfEmptyBodiesIsReported` | SHARED-EMPTY-001, RH5412-001 (spec example) | Verify(source) |
| RH5412 `VerifyNestedEmptyClassIsReportedAndFixed` | SHARED-EMPTY-001, SHARED-EMPTY-003; FC-027 | Verify(source, fixed) |
| RH5412 `VerifySpecificationExampleOfClassWithMemberAndSemicolonDeclarationIsNotReported` | SHARED-EMPTY-002 (spec example) | Verify(source) |
| RH5412 `VerifySpecificationExampleOfSemicolonDeclarationFix` | SHARED-EMPTY-003 (spec example) | Verify(source, fixed) |
| RH5412 `VerifySemicolonDeclarationFixWithCarriageReturnLineFeeds` | SHARED-EMPTY-003; FC-010 | ApplyCodeFixAsync, CRLF |
| RH5412 `VerifyCommentBehindClosingBraceOfMultiLineClassFollowsSemicolon` | SHARED-EMPTY-003; FC-021 | Verify(source, fixed) |
| RH5412 `VerifyCommentInsideHeaderDoesNotWithholdFix` | SHARED-EMPTY-003, SHARED-EMPTY-004; FC-003 (fix side of the guard) | Verify(source, fixed) |
| RH5412 `VerifySpecificationExampleOfWithheldFix` | SHARED-EMPTY-001, SHARED-EMPTY-004 (spec example: after header, before `{`, in body) | VerifyNoCodeFixIsOffered |
| RH5412 `VerifyCommentBehindOpeningBraceIsReportedWithoutCodeFix` | SHARED-EMPTY-001, SHARED-EMPTY-004; FC-021 (fourth guarded position) | VerifyNoCodeFixIsOffered |
| RH5412 `VerifyEmptyPartialClassWithPrimaryConstructorAndBaseListIsFixed` | SHARED-EMPTY-001, SHARED-EMPTY-003; FC-024 | Verify(source, fixed) |
| RH5412 `VerifySpecificationExampleOfEmptyClassesIsReportedAndFixed` | RH5412-001 (spec example) | Verify(source, fixed) |
| RH5412 `VerifySpecificationExampleIsNotReportedUnderCSharp11` / `VerifyEmptyClassIsReportedUnderCSharp12` | RH5412-001; FC-003, FC-006 | Verify(source) |
| RH5413 `VerifySpecificationExampleOfEmptyStructsIsReportedAndFixed` | SHARED-EMPTY-003, RH5413-001 (spec example; record struct not reported) | Verify(source, fixed) |
| RH5413 `VerifySpecificationExampleIsNotReportedUnderCSharp11` / `VerifyEmptyStructIsReportedUnderCSharp12` | RH5413-001; FC-006 | Verify(source) |
| RH5414 `VerifySpecificationExampleOfEmptyInterfacesIsReportedAndFixed` | SHARED-EMPTY-003, RH5414-001 (spec example) | Verify(source, fixed) |
| RH5414 `VerifySpecificationExampleIsNotReportedUnderCSharp11` / `VerifyEmptyInterfaceIsReportedUnderCSharp12` | RH5414-001; FC-006 | Verify(source) |
| RH5415 `VerifySpecificationExampleOfEmptyRecordsIsReportedAndFixed` | SHARED-EMPTY-003, RH5415-001 (spec example; record struct not reported) | Verify(source, fixed) |
| RH5415 `VerifyEmptyRecordIsNotReportedUnderCSharp8` | RH5415-001; FC-006, FC-033 (see Findings) | Verify(source) with compiler errors |
| RH5415 `VerifyEmptyRecordIsReportedUnderCSharp9` | RH5415-001; FC-006 | Verify(source) |
| RH5416 `VerifySpecificationExampleOfEmptyRecordStructsIsReportedAndFixed` | SHARED-EMPTY-003, RH5416-001 (spec example; record class not reported) | Verify(source, fixed) |
| RH5416 `VerifyEmptyRecordStructIsNotReportedUnderCSharp9` | RH5416-001; FC-006, FC-033 (asserts the tree holds a `RecordStructDeclaration`; expects CS8773) | Verify(source) |
| RH5416 `VerifyEmptyRecordStructIsReportedUnderCSharp10` | RH5416-001; FC-006 | Verify(source) |
| RH5417 `VerifySpecificationExampleOfOneLineBodiesIsReported` | RH5417-001 (spec example, 14 diagnostics, every reported kind) | Verify(source) |
| RH5417 `VerifySpecificationExampleOfOtherBlocksIsNotReported` | RH5417-002 (spec example, comment after `{`) | Verify(source) |
| RH5417 `VerifySpecificationExampleOfBraceLayoutFix` | RH5417-003 (spec example) | Verify(source, fixed) |
| RH5417 `VerifyBraceLayoutFixWithCarriageReturnLineFeeds` | RH5417-003; FC-010 | ApplyCodeFixAsync, CRLF |
| RH5417 `VerifyOperatorBodyIsFixed`, `VerifyConversionOperatorBodyIsFixed` | RH5417-003; FC-025 (first fix tests for these kinds) | Verify(source, fixed) |
| RH5417 `VerifyIndexerAccessorBodyIsFixed`, `VerifyEventAccessorBodiesAreFixed`, `VerifyInitAccessorBodyIsFixedAndKeepsItsBlockBody` | RH5417-003; FC-027, FC-007/FC-008 (two event accessors in one member) | Verify(source, fixed) |
| RH5417 `VerifySpecificationExampleOfBodiesWithoutCodeFix` | RH5417-004 (spec example) | VerifyNoCodeFixIsOffered |
| RH5417 `VerifySimpleLambdaBodyIsReportedWithoutCodeFix` | RH5417-004 (simple-lambda form, not in the example) | VerifyNoCodeFixIsOffered |

Fix All: every class derives from `BatchCodeFixTestsBase`, and its inherited `FixAllCorrectsEveryDiagnostic` carries SHARED-FIX-001/002. `Verify(source, fixed)` with several diagnostics also runs the incremental and Fix All paths.

## Findings

- **RH5415, below C# 9 (FC-033).** At C# 8, Roslyn does not parse `record A { }` as a `RecordDeclarationSyntax`. It parses a property `A` of type `record`, with CS0246, CS0548 and CS9348.
  - The test asserts that no record declaration exists and expects those compiler errors.
  - So RH5415-001's clause "Below C# 9 it MUST NOT report a record" holds because of the parser. The analyzer's version check for records (`MinimumLanguageVersion => CSharp9`) cannot be reached from source.
  - This is not a defect against the spec. It is a fact for the spec checker: the spec's "Under C# 8, `A` is not reported" is true, but no source exercises the version check for this kind.
- **RH5416, below C# 10.** At C# 9 the source is parsed as a `RecordStructDeclaration`, with CS8773. The version check is what keeps it from being reported, and the test asserts both.
- **Spec examples.** Every spec example passed unchanged, as did the skeptic's breaking inputs listed above. The results and markup match the specs.

## Not covered, and why

- **Inputs tied to open questions 1–7 of `03-spec-author.md`.** No characterization tests were added for them; the existing two are kept untagged.
  - Q1: whitespace left on the comma's line.
  - Q2: reformatting outside the empty body.
  - Q3: `class C { };`.
  - Q4: bodies holding a directive.
  - Q5: an RH5417 member that shares its line.
  - Q6/Q7: `#region` renames.

  Pinning them now would fix a behavior the maintainer has not decided.
- **#1150 no-op fix under `<AUTO-GENERATED>` or a syntax error (FC-009).** Not pinned, because that would pin a known defect (skeptic, FC-009).
- **Per-kind repeats of the SHARED-EMPTY guard and CRLF tests on RH5413–RH5416.**
  - Proof: `rg -n "override" src/Reihitsu.Analyzer.CodeFixes/Rules/Layout/RH541[2-6]*CodeFixProvider.cs` shows that each provider overrides only `MinimumLanguageVersion`, `DeclarationKind`, `CodeFixTitle`, and `DiagnosticId`.
  - Registration and the rewrite are in `EmptyTypeDeclarationShouldUseSemicolonCodeFixProviderBase`. It calls `EmptyTypeDeclarationSemicolonAnalysisUtilities.CanConvertSafely` (line 103) and `ReihitsuFormatter.FormatNodeInDocumentAsync` (line 56).
  - So those conditions are tested once, on RH5412. Kind and version are tested per rule.
- **Repeats of the trailing-comma fix tests (comment, comma-only line, CRLF) on RH5410/RH5411.**
  - Proof: `rg -n "RemoveTrailingComma|class RH54(09|10|11)" src/Reihitsu.Analyzer.CodeFixes` shows that all three providers derive from `TrailingCommaRemovalCodeFixProviderBase`. Its only rewrite is `TrailingCommaCodeFixHelper.RemoveTrailingCommaAsync` (base line 62).
  - Existing RH5410/RH5411 tests still cover the comment and comma-only cases.
- **The analyzer-side SHARED-TRAIL-002 guard on RH5410/RH5411.** Each of them already has a conditional-block test and RH5410 has a `#region` test. `TrailingCommaAnalyzerBase<TNode>` holds the shared guard, and RH5409 now carries the remaining sides: `#pragma`, `#elif`/`#else`, an empty pair, and a directive before the comma.
- **FC-019/FC-020 for RH5417.** A directive cannot lie between braces that share one line, because a directive needs its own line. The comment case is covered: `/* c */` in the spec example, and `// c` after `{` in RH5417-002.
- **FC-018/FC-036 (naming), FC-022/FC-023 (literals, identifiers), FC-026, FC-029.** These cannot reach these rules: they do not rename, do not touch literal text, and do not re-resolve repeated tokens or lay out key/value pairs.
- **Formatter parity (FC-013).** The `Formatter/Formatting/RH54*FormatterTests` classes already exist and were left untagged; they belong to FMT-STRUCT/FMT-BREAK. The skeptic established FC-013 by running `format.sh` twice.
- **A top-level local function (RH5417-004).** Not added, because top-level statements in the test project's library output kind are a compiler error.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Formatting.RH54|FullyQualifiedName~Reihitsu.Analyzer.Test.Formatter.Formatting.RH54"
scripts/test.sh --project architecture --filter "FullyQualifiedName~SpecConsistencyTests|FullyQualifiedName~SpecStructureTests|FullyQualifiedName~RequirementCoverageTests"
```

Results on c979775: analyzer 317 passed, 0 failed. Architecture 39 passed, 0 failed, so `EveryRequirementOfAnEnforcedSpecIsCovered` now passes for all 21 new IDs.

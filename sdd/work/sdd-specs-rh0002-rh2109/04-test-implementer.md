Role: test implementer
Commit: a76d853
Verdict: tests written

Lane: Specs and tests for existing behavior. No production code was touched; every test below passes on the unchanged code. No test contradicted a spec, so there is no finding against a spec. My prompt carried no intended fix, suspected cause, or hoped-for verdict.

Inputs: `00-lead.md`, `02-skeptic.md`, `03-spec-author.md` (supersedes the question lists of `01-spec-author.md`), the 18 rule specs `sdd/specs/analyzer/RH0002.md` … `RH2109.md`, and `sdd/specs/shared/struct-equality.md`, `debug-message-text.md`, `nested-types.md`. Expected values come from the spec examples and requirements. Every characterization test pins output the spec author or skeptic observed and recorded in their handovers.

## Tests

All tests are in `src/Reihitsu.Analyzer.Test`. "Existing" means a test that was already there and only got its `[Requirement]` tags. "New" means I added it. "Char." means a characterization test: its doc comment starts with "Characterizes …" and says the case is an open question (or an existing follow-up), so the test pins today's behavior without stating it as intended. Characterization tests carry no `[Requirement]` tag.

| Test (class prefix · method) | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH0002 · DocumentationModeNone (existing) | RH0002-001, RH0002-002 | `Verify` + doc-mode transform | pass |
| RH0002 · DocumentationModeParse (existing) | RH0002-001 | same | pass |
| RH0002 · DiagnosticReportedForEverySyntaxTreeWithDocumentationModeNone (existing) | RH0002-001, RH0002-002 | same | pass |
| RH0002 · DocumentationModeDiagnose | RH0002-001 | same | pass |
| RH0002 · DocumentationModeNoneIsReportedOnFirstTokenAfterLeadingComments | RH0002-001, RH0002-002 (FC-002) | same | pass |
| RH0002 · DocumentationModeNoneCommentsOnlyFileIsReportedAtEndOfFile | RH0002-002 (FC-002) | same, empty-span markup | pass; mutation to start of file fails on line 1 vs 2 |
| RH0002 · DocumentationModeNoneEmptyFileIsReportedAtStart | RH0002-002 (FC-002) | same | pass |
| RH1001 · 13 existing tests | SHARED-EQUAL-001/002, RH1001-001…005 (see the tags in the file) | `Verify` | pass |
| RH1001 · VerifyStructTypesUsedAsKeysMustImplementEqualityMembers (existing) | left untagged: it also pins struct-equality Q1 (`IEquatable<T>` without `GetHashCode`) | `Verify` | pass |
| RH1001 · VerifyOnlyStructsWithoutTheirOwnEqualityMembersAreReported | SHARED-EQUAL-001, RH1001-001, RH1001-005 | `Verify` | pass |
| RH1001 · VerifyEveryHashBasedCollectionTypeIsReportedOnItsKey | RH1001-001, RH1001-005 | `Verify` | pass |
| RH1001 · VerifyEveryUseOfTheCollectionTypeIsReported | RH1001-002, RH1001-005 (FC-027: variable, cast with conditional, assignment, argument, arrow body) | `Verify` | pass |
| RH1001 · VerifyTargetTypedCreationInFieldPropertyAndWrappedCastIsCoveredByNamedType | RH1001-002, RH1001-005 (FC-027: field and property initializer, cast through `!` and `switch`) | `Verify` | pass |
| RH1001 · VerifyCreationsAndDeclarationsWithCustomComparerAreNotReported | RH1001-003, SHARED-EQUAL-002 (FC-002: several declarators, `Variables.All`) | `Verify` | pass |
| RH1001 · VerifyPropertyInitializedWithCustomComparerIsNotReported | RH1001-003 | `Verify` | pass |
| RH1001 · VerifyUsesOfAliasesAreReportedOnTheAliasName | RH1001-004, RH1001-005 | `Verify` | pass |
| RH1001 · VerifyStructImplementingIEquatableWithoutGetHashCodeIsNotReported | Char. struct-equality Q1 | `Verify` | pass |
| RH1001 · VerifyGenericStructsWithOwnEqualityMembersOverStructWithoutThemAreNotReported | Char. struct-equality Q2 | `Verify` | pass |
| RH1002 · 29 existing tests | SHARED-EQUAL-001/002, RH1002-001…005 (see the tags in the file) | `Verify` | pass |
| RH1002 · VerifyStructTypesUsedForEqualityComparisonMustImplementEqualityMembers, VerifyContainsAndSequenceEqualAreNotFlaggedForEqualityCapableElementTypes (existing) | left untagged: both also pin struct-equality Q1 | `Verify` | pass |
| RH1002 · VerifyEveryListedMethodIsReported | RH1002-001, RH1002-005 | `Verify` | pass |
| RH1002 · VerifyListedMethodsAreNotReportedForTypesWithEqualityMembers | RH1002-001, SHARED-EQUAL-001 | `Verify` | pass |
| RH1002 · VerifyComparedTypeIsTheSelectedOrPairKey | RH1002-002 | `Verify` | pass |
| RH1002 · VerifyMethodsOfOtherTypesAreNotReported | RH1002-003, RH1002-001 | `Verify` | pass |
| RH1002 · VerifyContainsOnDictionaryIsNotReported | RH1002-004 | `Verify` | pass |
| RH1002 · VerifyDiagnosticLocationOnNameTypeArgumentsAndNullConditionalCall | RH1002-005 | `Verify` | pass |
| RH1002 · VerifyOnlyArgumentsThatCanProduceAComparerCountAsCustom | SHARED-EQUAL-002, RH1002-001 (FC-002: `switch`, null-conditional on `null`, non-trailing named argument, both sides) | `Verify` | pass |
| RH1002 · VerifyConstantConditionCountsOnlyChosenBranch | SHARED-EQUAL-002 (FC-003: both branches of a constant condition) | `Verify` | pass |
| RH1002 · VerifyValueComparerCountsAsCustomComparerForKey | Char. struct-equality Q3 | `Verify` | pass |
| RH1002 · VerifyEqualityBasedMethodsOutsideTheListAreNotReported | Char. RH1002 Q1 (`CountBy`, `AggregateBy`, `ImmutableHashSet.Create`/`CreateRange`, `ImmutableDictionary.CreateRange`) | `Verify` | pass |
| RH2001 · 9 existing tests | RH2001-001…006 (see the tags in the file) | `Verify`, `GetCodeFixActionsAsync`, `ApplyCodeFixAsync` | pass |
| RH2001 · NoCodeFixForExplicitInterfaceImplementationAutoProperty (existing) | left untagged: it checks a fix for a diagnostic the analyzer never reports, and no requirement states that | `GetCodeFixActionsAsync` | pass |
| RH2001 · VerifyPrivateAutoPropertiesOfEveryShapeAreReported | RH2001-001, RH2001-003 | `Verify` | pass |
| RH2001 · VerifyOtherPropertiesAreNotReported | RH2001-002 | `Verify` | pass |
| RH2001 · VerifyFixReplacesPropertiesWithFieldsAndRenamesReferences | RH2001-004, RH2001-001 (FC-036: `nameof(Name)` → `nameof(_name)`) | `Verify(source, fixed)` | pass |
| RH2001 · VerifyFixReplacesPropertiesWithFieldsAndRenamesReferencesCarriageReturnLineFeed | RH2001-004 (CRLF) | `Verify` + `NormalizeToCarriageReturnLineFeed` | pass |
| RH2001 · VerifyFixReplacesStructPropertyAndRenamesReferencesInNestedType | RH2001-004 (FC-018: reference through another type) | `Verify(source, fixed)` | pass |
| RH2001 · VerifyFixDerivesFieldNameFromWordsOfPropertyName | RH2001-004, SHARED-CASING-006 | `Verify(source, fixed)` | pass |
| RH2001 · VerifyNoFixIsOfferedForInitAndModifiedAccessors | RH2001-005, RH2001-001 | `VerifyNoCodeFixIsOffered` | pass |
| RH2001 · VerifyNoFixIsOfferedWhenFieldOrMethodHasTheFieldName | RH2001-006 | `VerifyNoCodeFixIsOffered` | pass |
| RH2001 · VerifyPropertyUsingFieldKeywordWithLogicIsReportedWithoutFix | Char. RH2001 Q4 | `VerifyNoCodeFixIsOffered` | pass |
| RH2001 · VerifyPropertiesWithFieldTargetedOrAccessorAttributeAreReportedWithoutFix | Char. RH2001 Q5 | `VerifyNoCodeFixIsOffered` | pass |
| RH2001 · VerifyFixDropsCommentsInsidePropertyDeclaration | Char. RH2001 Q1 (P1; FC-004/FC-021) | `Verify(source, fixed)` | pass |
| RH2001 · VerifyFixLeavesReferenceInExcludedCodeUnchanged | Char. RH2001 Q2 (P1; FC-020) | `Verify(source, fixed)` | pass |
| RH2001 · VerifyFixReplacesPropertyOfMutableStructType | Char. RH2001 Q3 (P1; FC-006) | `Verify(source, fixed)` | pass |
| RH2001 · VerifyFixOnReadonlyPropertyWritesReadonlyTwice | Char. RH2001 Q6 (P1) | `ApplyCodeFixAsync` | pass |
| RH2001 · VerifyFixKeepsUppercaseRunOfLeadingAcronymInFieldName | Char. existing follow-up thoenissen/Reihitsu#971 | `Verify(source, fixed)` | pass |
| RH2002 · 6 existing tests | RH2002-001…005 | `Verify` | pass |
| RH2002 · VerifyStaticEventHandlerVirtualAndOverrideAsyncVoidMethodsAreReported | RH2002-001, RH2002-005 | `Verify` | pass |
| RH2002 · VerifyAsyncAnonymousFunctionsAreReportedOnlyForVoidDelegates | RH2002-003, RH2002-005 (inferred `var` lambda, `Task.Run`) | `Verify` | pass |
| RH2002 · VerifyAsyncValueTaskAndSynchronousVoidFunctionsAreNotReported | RH2002-004 (FC-025) | `Verify` | pass |
| RH2002 · VerifyAsyncVoidExplicitInterfaceImplementationIsNotReported | Char. RH2002 Q1 (FC-024) | `Verify` | pass |
| RH2002 · VerifyAsyncVoidPartialMethodIsReportedOnBothDeclarations | Char. RH2002 Q2 (FC-024) | `Verify` | pass |
| RH2003 · 5 existing tests | RH2003-001…004, SHARED-DIAG-002 | `Verify` | pass |
| RH2003 · VerifyQualifiedAndInitializedCreationsAreReportedOnCreatedType | RH2003-001, RH2003-004 | `Verify` | pass |
| RH2003 · VerifyTargetTypedCreationsAreReportedOnNewKeyword | RH2003-002, RH2003-004 | `Verify` | pass |
| RH2003 · VerifyDerivedAndSameNamedTypesAreNotReported | RH2003-003 | `Verify` | pass |
| RH2003 · VerifyCreationThroughUsingAliasIsNotReported | Char. RH2003 Q1 | `Verify` | pass |
| RH2004 · 13 existing tests | RH2004-001…006 | `Verify(source, fixed)`, `Verify` | pass |
| RH2004 · VerifyDeclarationsWithoutAccessibilityModifierAreReported | RH2004-001, RH2004-005 | `Verify` | pass |
| RH2004 · VerifyOperatorsWithoutAccessibilityModifierAreReportedOnTheirToken | RH2004-001, RH2004-005 | `Verify`, `CompilerDiagnostics.None` (an operator without `public` is CS0558) | pass |
| RH2004 · VerifyPartialTypeAndPartialMethodPartsAreNotReported | RH2004-002 | `Verify` | pass |
| RH2004 · VerifyStaticConstructorAndExplicitInterfaceImplementationsAreNotReported | RH2004-003 (FC-024: method, property, indexer, event) | `Verify` | pass |
| RH2004 · VerifyInterfaceMembersAreNotReportedAndNestedTypesAre | RH2004-004 (FC-024: static interface member) | `Verify` | pass |
| RH2004 · VerifyDiagnosticIsOnFirstDeclaratorAndIndexerKeyword | RH2004-005 | `Verify` | pass |
| RH2004 · VerifyFixAddsDefaultAccessibilityOfEachContext | RH2004-006, RH2004-001 (extension block member, C# 14) | `Verify(source, fixed)` | pass |
| RH2004 · VerifyFixAddsDefaultAccessibilityOfEachContextCarriageReturnLineFeed | RH2004-006 (CRLF) | `Verify` + CRLF | pass |
| RH2004 · VerifyFixAddsPrivateInStructAndRecordsAndInternalAtFileLevel | RH2004-006 | `Verify(source, fixed)` | pass |
| RH2004 · VerifyFixAddsPrivateToOperator | Char. RH2004 Q1 | `ApplyCodeFixAsync` | pass |
| RH2004 · VerifyExplicitInterfaceOperatorImplementationsAreReportedAndGetPrivate | Char. RH2004 Q2, Q3 (FC-024) | `Verify`, `ApplyCodeFixAsync` | pass |
| RH2004 · VerifyPartialMemberPartsAreReported | Char. RH2004 Q4 (reporting), Q5 (FC-024) | `Verify` | pass |
| RH2004 · VerifyOneFixApplicationChangesOnePartOfPartialProperty | Char. RH2004 Q4 (fix, P1) | `ApplyCodeFixAsync` | pass; it asserts that exactly one part gets the modifier, see the note below |
| RH2004 · VerifyFixOnAttributedMemberPlacesModifierBeforeIndentation | Char. existing follow-up thoenissen/Reihitsu#967 | `ApplyCodeFixAsync` | pass |
| RH2006 · 4 existing tests | RH2006-001, -003, -004, SHARED-DEBUG-001 | `Verify` | pass |
| RH2006 · VerifyEveryFormOfConstantWithoutTextIsReported | SHARED-DEBUG-001, RH2006-001 (literal, `default`, cast, `const` field and local, concatenation, interpolations, no-break space as `" "`) | `Verify` | pass |
| RH2006 · VerifyAssertionsWithoutMessageTextAreReported | RH2006-001, RH2006-004 | `Verify` | pass |
| RH2006 · VerifyOnlyMessageArgumentIsChecked | RH2006-002 | `Verify` | pass |
| RH2006 · VerifyAssertMethodsOfOtherTypesAreNotReported | RH2006-003 | `Verify` | pass |
| RH2006 · VerifyDiagnosticCoversWholeMultiLineInvocation | RH2006-004 | `Verify` | pass |
| RH2006 · VerifyStringEmptyAndConditionalsWithoutTextAreNotReported | Char. SHARED-DEBUG Q1 | `Verify` | pass |
| RH2007 · 2 existing tests | RH2007-001…004 | `Verify` | pass |
| RH2007 · VerifyEveryFormOfConstantWithoutTextIsReported | SHARED-DEBUG-001, RH2007-001 | `Verify` | pass |
| RH2007 · VerifyFailuresWithoutMessageTextAreReported | RH2007-001, RH2007-004 | `Verify` | pass |
| RH2007 · VerifyOnlyMessageArgumentIsChecked | RH2007-002 | `Verify` | pass |
| RH2007 · VerifyFailMethodsOfOtherTypesAreNotReported | RH2007-003 | `Verify` | pass |
| RH2007 · VerifyDiagnosticCoversWholeMultiLineInvocation | RH2007-004 | `Verify` | pass |
| RH2007 · VerifyStringEmptyAndConditionalsWithoutTextAreNotReported | Char. SHARED-DEBUG Q1 | `Verify` | pass |
| RH2101 · 4 existing tests | RH2101-001, -002, SHARED-NESTED-001, -002 | `Verify` | pass |
| RH2101 · VerifyClassesNestedInEveryKindOfTypeAtAnyDepthAreReported | SHARED-NESTED-001, RH2101-001 (class, interface, record, struct, record struct, static class, depth 2; namespace and file level not reported) | `Verify` | pass |
| RH2101 · VerifyEachPartOfANestedPartialClassIsReported | SHARED-NESTED-002, RH2101-001 | `Verify` | pass |
| RH2101 · VerifyNestedClassIsReportedAndNestedRecordIsNot | RH2101-001 | `Verify` | pass |
| RH2101 · VerifyNonStaticClassNestedInStaticClassIsReported | RH2101-002 | `Verify` | pass |
| RH2101 · VerifyNestedStaticPartialClassWithStaticOnEveryPartIsNotReported | RH2101-002 (FC-024, covered side) | `Verify` | pass |
| RH2101 · VerifyPartWithoutStaticOfNestedStaticPartialClassIsReported | Char. RH2101 Q1 (FC-024) | `Verify` | pass |
| RH2102 · 4 existing + 2 new | RH2102-001, -002 (nested, partial, generic; multi-line parameter list) | `Verify` | pass |
| RH2103 · 4 existing + 2 new | RH2103-001, -002 (readonly, partial; multi-line parameter list) | `Verify` | pass |
| RH2104 · 3 existing + 2 new | RH2104-001, SHARED-NESTED-001, -002 (struct in class, interface, depth 2; partial parts) | `Verify` | pass |
| RH2105 · 3 existing + 1 new | RH2105-001, SHARED-NESTED-001 (in class, interface, struct) | `Verify` | pass |
| RH2106 · 3 existing + 1 new | RH2106-001, SHARED-NESTED-001 (`record`, `record class`, `record struct`, in a record) | `Verify` | pass |
| RH2107 · 3 existing + 1 new | RH2107-001, SHARED-NESTED-001 (in class, struct, interface) | `Verify` | pass |
| RH2108 · 3 existing + 1 new | RH2108-001, SHARED-NESTED-001, -002 (in class, interface, record) | `Verify` | pass |
| RH2109 · 4 existing tests | RH2109-001, -002, -003 | `Verify` + additional file | pass |
| RH2109 · VerifyCodeBlocksWithBraceOnSameAndNextLineAreReportedOnDirective | RH2109-001, RH2109-003 | new `RazorCodeBlockSpan` helper (checks the 5-character span) | pass |
| RH2109 · VerifyCodeBlockWithoutWhiteSpaceBeforeBraceIsReported | RH2109-001 | same | pass |
| RH2109 · VerifyIndentedCodeBlockIsReportedOnDirective | RH2109-001, RH2109-003 | same | pass |
| RH2109 · VerifyUpperCaseRazorExtensionIsReported | RH2109-001 (case-insensitive extension) | same | pass |
| RH2109 · VerifyCodeBlockInCshtmlFileIsNotReported | RH2109-002 | `Verify` | pass |
| RH2109 · VerifyCodeBlockTextInCommentsAndStringsIsReported | Char. RH2109 Q1 (FC-022: HTML, Razor, C# block comment, C# string) | same | pass |
| RH2109 · VerifyEscapedAndEmailLikeCodeBlockTextIsReported | Char. RH2109 Q2 | same | pass |
| RH2109 · VerifyFunctionsBlockIsNotReported | Char. RH2109 Q3 | `Verify` | pass |

`FixAllCorrectsEveryDiagnostic` (FC-008) already exists for RH2001 and RH2004 through `BatchCodeFixTestsBase`, with two diagnostics in one document. I did not change it.

Note on `VerifyOneFixApplicationChangesOnePartOfPartialProperty`: `ApplyCodeFixAsync` fixes the first diagnostic the analyzer returns. For the two parts of a partial property, that order differed between runs: the first version, which asserted on the defining part, failed once. So the test asserts only that exactly one of the two parts gets `private`. The other `ApplyCodeFixAsync` tests have a single diagnostic each.

## Not covered, and why

- **FC-010 to FC-014 (formatter stability, analyzer/formatter parity).** No formatter phase owns these design and performance rules, and neither code fix changes a phase (the skeptic's handover). I wrote no `VerifyFormatter` tests.
- **FC-019 (a comment and a directive in a gap).** The comment and `#if` cases for RH2001 are filed questions (Q1, Q2). I pinned them only as characterization tests. RH2004 inserts a modifier token. Its comment and directive behavior is the shared limitation in `documentation/rules/general-notes.md` and SHARED-FIX-003. RH2004-006's coverage leaves declarations with attribute lists out. The only gap case observed is the existing follow-up thoenissen/Reihitsu#967, which is pinned as a characterization test.
- **FC-007 (one-pass convergence, another RH diagnostic).** The verifier applies the fixes and re-analyzes the fixed state. The shared code-fix tests own convergence. I added no RH7105 cross-rule test, because RH2004 inserting the accessibility first is what RH7105 expects (RH2004 non-goal), and RH2001's `readonly` ordering is exercised by `private static readonly string _cacheKey` in the RH2001-004 example.
- **`.NET 10` `LeftJoin`/`RightJoin` (RH1002 Q1).** The verifier uses `ReferenceAssemblies.Net.Net90`, which does not have them. The characterization covers the other methods of Q1.
- **RH2004 Q5 for itself.** Partial constructors are pinned inside `VerifyPartialMemberPartsAreReported`: only the implementing part is reported. A CS8799 state left behind after the constructor fix is not pinned.
- **Existing tests left untagged on purpose.** RH1001 `VerifyStructTypesUsedAsKeysMustImplementEqualityMembers`, RH1002 `VerifyStructTypesUsedForEqualityComparisonMustImplementEqualityMembers`, and RH1002 `VerifyContainsAndSequenceEqualAreNotFlaggedForEqualityCapableElementTypes` each include a struct that implements `IEquatable<T>` without overriding `GetHashCode()`. That is struct-equality Q1, which SHARED-EQUAL-001's coverage sentence leaves out. Tagging them would read as pinning Q1 as intended. Their other cases are covered by the new tagged tests. RH2001 `NoCodeFixForExplicitInterfaceImplementationAutoProperty` states no requirement.
- **FC-023 (verbatim identifiers).** SHARED-DIAG-002 tests already cover RH2003 (existing, now also tagged RH2003-001). I added no verbatim identifier test for RH2001 naming.
- **Out of scope.** The characterization tests double as a record for the lead: they pin the behavior of the filed questions and the two follow-up instances (#971, #967) listed in `03-spec-author.md`. I found no new question and no new follow-up candidate.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Analyzer.RH0002"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Performance.RH1001|FullyQualifiedName~Reihitsu.Analyzer.Test.Performance.RH1002"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Design.RH200"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Design.RH21"
scripts/test.sh --project architecture --filter "FullyQualifiedName~RequirementCoverageTests|FullyQualifiedName~SpecConsistencyTests|FullyQualifiedName~SpecStructureTests"
```

Results on this tree:
- The whole analyzer project, run once before the two CRLF tests were added: 5192 passed, 0 failed.
- The focused RH2001 and RH2004 run after the CRLF tests were added: 56 passed.
- The whole architecture project: 72 passed.
- The SDD filter: 39 passed. `RequirementCoverageTests` now covers all 60 IDs of the run.

`scripts/format.sh` on the 18 test files reported "Formatted 0 of 18 file(s)".

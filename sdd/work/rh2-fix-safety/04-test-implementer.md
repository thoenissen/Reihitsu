Role: test implementer
Commit: cd8f410
Verdict: #967: reproduced; #1286: reproduced; #1287: reproduced; #1288: reproduced

The prompt carried no intended fix, suspected cause, or hoped-for verdict, so there was nothing to ignore. Expected values come from the spec examples and requirements as amended up to `9494bcd` (diff against `162e7e6`), never from a patched copy of the code. All tests are in `src/Reihitsu.Analyzer.Test/`. No production code was touched.

On `cd8f410` plus these tests, the whole analyzer project gives 82 failed and 7021 passed. The 82 failures are exactly the tests marked "fails" below; nothing else fails. `scripts/test.sh --project architecture`: 115 passed. `RequirementCoverageTests` now finds a tagged test for RH2001-007, RH2001-008, RH2005-004, RH7002-004, and SHARED-DIAG-012. The test build has no warnings in these files. `scripts/format.sh` changed nothing.

Every failing test was checked for its reason. Fix-output tests fail on the diff of the issue's own expected-versus-actual difference. No-fix tests fail with "No code fix may be offered" or with an iterative-fix diff. Report tests fail on the diagnostic count. None fails on a compile error. The full log is in the session scratchpad.

Characterization tests that pinned today's behavior were replaced by tests of the specified behavior, as their own comments announced:

| Removed characterization test | Replaced by |
|---|---|
| `RH2004…Tests.VerifyExplicitInterfaceOperatorImplementationsAreReportedAndGetPrivate` | `VerifyExplicitInterfaceOperatorAndConversionImplementationsAreNotReported` |
| `RH2004…Tests.VerifyPartialMemberPartsAreReported` | the four `VerifyPartial…IsReportedOnceOnItsDefiningPartWithoutFix` tests |
| `RH2004…Tests.VerifyOneFixApplicationChangesOnePartOfPartialProperty` | the same four tests (no fix offered) |
| `RH2004…Tests.VerifyFixOnAttributedMemberPlacesModifierBeforeIndentation` | `VerifyFixPutsModifierOnTheMembersLineBehindAnAttributeListOnItsOwnLine` |
| `RH2001…Tests.VerifyFixDropsCommentsInsidePropertyDeclaration` | `VerifyNoFixIsOfferedForCommentsBehindTheNameAndInsideTheAccessorList` |
| `RH2001…Tests.VerifyFixLeavesReferenceInExcludedCodeUnchanged` | `VerifyNoFixIsOfferedWhenExcludedCodeNamesTheProperty` |
| `RH2001…Tests.VerifyFixReplacesPropertyOfMutableStructType` | `VerifyNoFixIsOfferedForAMutableStructPropertyChangedByACall` |
| `RH2001…Tests.VerifyFixOnReadonlyPropertyWritesReadonlyTwice` | `VerifyFixKeepsOneReadonlyModifierOfAReadonlyStructProperty` |
| `RH2002…Tests.VerifyAsyncVoidPartialMethodIsReportedOnBothDeclarations` | `VerifyAsyncVoidPartialMethodIsReportedOnceOnItsImplementingPart` |
| `RH2101…Tests.VerifyPartWithoutStaticOfNestedStaticPartialClassIsReported` | `VerifyNestedPartialClassWithStaticOnTheFirstPartIsNotReported` |
| `RH7401…Tests.VerifyPartialOverrideMethodIsReportedOncePerPart` | `RH7401…Tests.VerifyPartialMethodIsReportedOnceOnItsDefiningPart` |

`VerifyFixAddsPrivateToOperator` (ordinary operator, #1435) and the RH7409/RH7410 "judged at its defining part" characterizations (only-some-parts case, #1334) were left unchanged.

## thoenissen/Reihitsu#967

### Tests

File `Design/RH2005FieldsMustBePrivateAnalyzerTests.cs`, unless the RH2004 file is named.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `VerifyFixPutsPrivateInFrontOfStaticInTheFieldsColumn` | RH2005-003 | `Verify` | passes |
| `VerifyFixPutsPrivateOnTheFieldsLineBehindAnAttributeListOnItsOwnLine` | RH2005-003 | `Verify` | fails (reproduction) |
| `…OnItsOwnLineCarriageReturnLineFeed` | RH2005-003 | `Verify`, CRLF | fails |
| `VerifyFixKeepsCommentAndDirectiveLinesInFrontOfTheField` | RH2005-003 | `Verify` | fails |
| `…InFrontOfTheFieldCarriageReturnLineFeed` | RH2005-003 | `Verify`, CRLF | fails |
| `VerifyFixKeepsCommentLineInFrontOfTheField` | RH2005-003; FC-005 | `Verify` | fails |
| `VerifyFixKeepsDirectiveLineInFrontOfTheField` | RH2005-003; FC-005 | `Verify` | fails |
| `VerifyFixPutsPrivateBehindAnAttributeListOnTheFieldsLine` | RH2005-003; FC-003 | `Verify` | passes |
| `VerifyFixReplacesPublicThatFollowsAnAttributeListOnItsOwnLine` | RH2005-003; FC-003 | `Verify` | passes |
| `VerifyNoFixIsOfferedForAFieldReadFromAnotherType` | RH2005-004 | `VerifyNoCodeFixIsOffered` | fails (reproduction) |
| `VerifyNoFixIsOfferedForAFieldReadFromADerivedType` | RH2005-004 | `VerifyNoCodeFixIsOffered` | fails |
| `VerifyNoFixIsOfferedForAFieldReadFromATypeNestedInADerivedType` | RH2005-004 | `VerifyNoCodeFixIsOffered` | fails |
| `VerifyNoFixIsOfferedForAFieldOfANestedTypeReadByTheEnclosingType` | RH2005-004 | `VerifyNoCodeFixIsOffered` | fails |
| `VerifyNoFixIsOfferedForAFieldNamedByNameofInAnotherType` | RH2005-004 | `VerifyNoCodeFixIsOffered` | fails (reproduction) |
| `VerifyFixIsOfferedForAFieldReadFromAnotherPartAndANestedType` | RH2005-004; FC-003, FC-024 | `Verify` | passes |
| `VerifyFixIsOfferedForAFieldReadFromAnotherPartInAnotherDocument` | RH2005-004; FC-024 | `Verify`, two documents | passes |
| `VerifyNoFixIsOfferedForAFieldReadFromAnotherDocument` | RH2005-004 | `Verify(src, src)`, two documents | fails |
| `VerifyNoFixIsOfferedForAFieldReadFromAnotherProject` | RH2005-004 | `Verify(src, src)`, second project | fails |
| `VerifyNoFixIsOfferedWhenAnyFieldOfTheDeclarationIsReadOutsideTheType` | RH2005-004 | `VerifyNoCodeFixIsOffered` | fails |
| `VerifyNoFixIsOfferedForAFieldNamedInAnAttributeOfItsType` | RH2005-004 | `VerifyNoCodeFixIsOffered` | fails |
| `VerifyNoFixIsOfferedForAFieldNamedInACrefOfAnotherType` | RH2005-004 | `VerifyNoCodeFixIsOffered`, `DocumentationMode.Diagnose` | fails |
| `VerifyNoFixIsOfferedForAFieldReadOutsideItsTypeInExcludedCode` | SHARED-FIX-003; FC-020 | `VerifyNoCodeFixIsOffered` | fails |
| `VerifyFixIsOfferedForAFieldReadInsideItsTypeInExcludedCode` | SHARED-FIX-003; FC-003 | `Verify` | passes |
| `VerifyFixAllSkipsTheFieldReadOutsideItsType` | RH2005-004, SHARED-FIX-002; FC-008 | `Verify` (single and Fix All) | fails |
| RH2004 file: `VerifyFixPutsModifierOnTheMembersLineBehindAnAttributeListOnItsOwnLine` | RH2004-006 | `Verify` | fails (reproduction of the issue comment) |
| RH2004 file: `…OnItsOwnLineCarriageReturnLineFeed` | RH2004-006 | `Verify`, CRLF | fails |
| RH2004 file: `VerifyFixKeepsCommentAndDirectiveLinesInFrontOfTheDeclaration` | RH2004-006; FC-008 (nested D/Run under Fix All) | `Verify` | fails |
| RH2004 file: `…InFrontOfTheDeclarationCarriageReturnLineFeed` | RH2004-006 | `Verify`, CRLF | fails |
| RH2004 file: `VerifyFixKeepsCommentLineInFrontOfTheMember` | RH2004-006; FC-005 | `Verify` | fails |
| RH2004 file: `VerifyFixKeepsDirectiveLineInFrontOfTheMember` | RH2004-006; FC-005 | `Verify` | fails |
| RH2004 file: `VerifyFixPutsModifierInFrontOfStaticBehindAnAttributeListOnItsOwnLine` | RH2004-006 | `Verify` | fails |
| RH2004 file: `VerifyFixPutsModifierOnTheLinesOfAttributedFieldAndProperty` | RH2004-006 | `Verify` | fails |
| RH2004 file: `VerifyFixPutsModifierBehindAnAttributeListOnTheMembersLine` | RH2004-006; FC-003 | `Verify` | passes |

### Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| RH2005 `[Obsolete]⏎    static public int Counter;` | LF, CRLF | code fix | reproduced | `private     static int Counter;` at column 0 vs `    private static int Counter;` |
| RH2005 `[Obsolete]⏎#pragma …⏎    static public int Counter = 1;` | LF | code fix | reproduced | `private #pragma warning disable CS0414` then `    static int …` vs the directive line kept and `    private static int Counter = 1;` |
| RH2005 `static public int Counter;` without attributes (issue's case 1 as written) | LF | code fix | not reproduced; passes | `    private static int Counter;` as specified |
| RH2004 `[Obsolete]⏎    void M()` (issue comment) | LF, CRLF | code fix | reproduced | `private     void M()` vs `    private void M()` |
| RH2005 field read from `User` / derived type / `nameof` / other document / other project | LF | code fix | reproduced | fix offered, writes `private int Value;` vs no fix offered |

### Not covered, and why

- Formatter surface: not involved. Neither fix runs the formatter on these nodes, and RH2004 and RH2005 are not layout rules under SHARED-DIAG-010.
- The skeptic's balanced `#region` / `#if` line between attribute list and declaration: the `#pragma` tests cover a directive line in that gap. Both rules reach it through one helper (`DeclarationModifierUtilities.AddAccessibilityModifier(MemberDeclarationSyntax, …)`, whose only production callers are the two fix providers), and the defect is in where the inserted modifier takes its trivia from, whatever the directive kind is.
- RH2004 delegate, event field, constructor, and indexer behind an attribute list: the method, `static` method, field, property, and type tests cover the three first-token shapes of the skeptic's matrix (another modifier, a type keyword, the type's `class` keyword) through the same helper. This is the one-call-site argument above.

## thoenissen/Reihitsu#1286

### Tests

File `Design/RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzerTests.cs`.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `VerifyFixKeepsOneReadonlyModifierOfAReadonlyStructProperty` | RH2001-004 | `Verify` | fails (reproduction: `readonly readonly`) |
| `VerifyFixReplacesNullableIntProperty` | RH2001-004 | `Verify` (net9.0 reference assemblies) | passes |
| `VerifyFixReplacesPropertiesOfEnumAndReadonlyStructTypes` | RH2001-004; FC-003 | `Verify` | passes |
| `VerifyFixReplacesPropertyOfTypeParameterConstrainedToAClassType` | RH2001-004; FC-003 | `Verify` | passes |
| `VerifyNoFixIsOfferedWhenExcludedCodeNamesTheProperty` | SHARED-CASING-007, SHARED-FIX-003 | `VerifyNoCodeFixIsOffered` | fails (reproduction) |
| `VerifyNoFixIsOfferedWhenExcludedCodeDeclaresTheNewName` | SHARED-CASING-007, SHARED-FIX-003 | `VerifyNoCodeFixIsOffered` | fails |
| `VerifyNoFixIsOfferedWhenAnExcludedElifBranchNamesTheProperty` | SHARED-CASING-007, SHARED-FIX-003; FC-020 | `VerifyNoCodeFixIsOffered` | fails |
| `VerifyNoFixIsOfferedWhenAnExcludedElseBranchNamesTheProperty` | SHARED-CASING-007, SHARED-FIX-003; FC-020 | `VerifyNoCodeFixIsOffered` | fails |
| `VerifyNoFixIsOfferedWhenAnIfFalseBlockNamesTheProperty` | SHARED-CASING-007, SHARED-FIX-003; FC-020 | `VerifyNoCodeFixIsOffered` | fails |
| `VerifyFixRenamesReferenceInActiveConditionalBranch` | SHARED-CASING-007; FC-003 | `Verify` | passes |
| `VerifyNoFixIsOfferedForCommentsBehindTheNameAndInsideTheAccessorList` | RH2001-007 | `VerifyNoCodeFixIsOffered` | fails (reproduction) |
| `VerifyNoFixIsOfferedForACommentBehindTheName` | RH2001-007; FC-021 | `VerifyNoCodeFixIsOffered` | fails |
| `VerifyNoFixIsOfferedForACommentInsideAOneLineAccessorList` | RH2001-007; FC-021 | `VerifyNoCodeFixIsOffered` | fails |
| `VerifyNoFixIsOfferedForACommentBetweenTheAccessorListAndTheInitializer` | RH2001-007 | `VerifyNoCodeFixIsOffered` | fails |
| `VerifyNoFixIsOfferedForACommentLineInAMultiLineAccessorList` | RH2001-007 | `VerifyNoCodeFixIsOffered` | fails |
| `…MultiLineAccessorListCarriageReturnLineFeed` | RH2001-007 | `VerifyNoCodeFixIsOffered`, CRLF | fails |
| `VerifyNoFixIsOfferedForADirectiveInsideTheAccessorList` | RH2001-007; FC-005, FC-020 | `VerifyNoCodeFixIsOffered` | fails |
| `…InsideTheAccessorListCarriageReturnLineFeed` | RH2001-007 | `VerifyNoCodeFixIsOffered`, CRLF | fails |
| `VerifyNoFixIsOfferedForADirectiveBetweenTheNameAndTheAccessorList` | RH2001-007; FC-005 | `VerifyNoCodeFixIsOffered` | fails |
| `VerifyFixKeepsCommentsInFrontOfTheFirstTokenAndBehindTheLastToken` | RH2001-007, RH2001-004; FC-003 | `Verify` | passes |
| `VerifyNoFixIsOfferedForAMutableStructPropertyChangedByACall` | RH2001-008 | `VerifyNoCodeFixIsOffered` | fails (reproduction) |
| `VerifyNoFixIsOfferedForMutableStructPropertiesThatNoUseChanges` | RH2001-008 | `VerifyNoCodeFixIsOffered` | fails |
| `VerifyNoFixIsOfferedForIntPropertiesWhenTargetingNetStandard20` | RH2001-008 | `Verify(src, src)` with `ReferenceAssemblies.NetStandard.NetStandard20` | fails |
| `VerifyNoFixIsOfferedForAGetOnlyMutableStructPropertyChangedInAConstructor` | RH2001-008 | `VerifyNoCodeFixIsOffered` | fails |
| `VerifyNoFixIsOfferedForAGetOnlyMutableStructPropertyThatNoUseChanges` | RH2001-008 | `VerifyNoCodeFixIsOffered` | fails |
| `VerifyFixIsOfferedOnlyForTheTypeParameterConstrainedToAReferenceType` | RH2001-008, RH2001-004; FC-008 | `Verify` (single and Fix All) | fails |
| `VerifyNoFixIsOfferedForATypeParameterConstrainedToAValueType` | RH2001-008 | `VerifyNoCodeFixIsOffered` | fails |
| `VerifyFixAllSkipsThePropertiesWhoseFixIsWithheld` | RH2001-007, RH2001-008, SHARED-FIX-002; FC-008 | `Verify` (single and Fix All) | fails |

### Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| `Value /* name */ { get; /* inner */ set; } // trailing` | LF | code fix | reproduced | fix offered (drops both comments) vs no fix |
| `return Value;` under `#if NEVER` | LF | code fix | reproduced | fix offered vs no fix |
| `Counter Current { get; set; }` with `Current.Increment()` | LF | code fix | reproduced | fix offered vs no fix |
| `private readonly int Value { get; }` in a struct | LF | code fix | reproduced | `private readonly readonly int _value;` vs `private readonly int _value;` |
| `int` property, netstandard2.0 | LF | code fix | reproduced | fix offered (`_count`) vs no fix |

The spec author asked whether `Int32` is declared `readonly` in the .NET 9 reference assemblies, which the analyzer tests use. `VerifyFixReplacesNullableIntProperty` and the existing `int` fix tests can only stay green after the change if it is. `VerifyNoCodeFixIsOffered`'s fix check uses the runtime's own assemblies (net10.0), where the skeptic confirmed it.

### Not covered, and why

- Mutable-struct constructs (a non-`readonly` getter, an event `+=`, `foreach` with a mutating enumerator): RH2001-008 is a check on the property's type, independent of use. `VerifyNoFixIsOfferedForMutableStructPropertiesThatNoUseChanges` and `…GetOnlyMutableStructPropertyThatNoUseChanges` show the fix is withheld with no changing use at all, so there is no per-construct arm to test.
- Comments between modifiers, between type and name, or inside the initializer: RH2001-007 says these are not covered, so nothing is asserted for them.
- FC-036 (the rename reaching `nameof` or reflection): the produced name is the fix's own output under RH2001-004, and existing tests cover the rename. The run does not change it.

## thoenissen/Reihitsu#1287

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH2004: `VerifyPartialPropertyIsReportedOnceOnItsDefiningPartWithoutFix` | RH2004-001, SHARED-DIAG-004, SHARED-DIAG-012 | `VerifyNoCodeFixIsOffered` | fails (reproduction) |
| RH2004: `VerifyPartialIndexerIsReportedOnceOnItsDefiningPartWithoutFix` | RH2004-001, SHARED-DIAG-004, SHARED-DIAG-012 | `VerifyNoCodeFixIsOffered` | fails |
| RH2004: `VerifyPartialEventIsReportedOnceOnItsDefiningPartWithoutFix` | RH2004-001, SHARED-DIAG-004, SHARED-DIAG-012 | `VerifyNoCodeFixIsOffered` | fails |
| RH2004: `VerifyPartialConstructorIsReportedOnceOnItsDefiningPartWithoutFix` | RH2004-001, SHARED-DIAG-004, SHARED-DIAG-012 | `VerifyNoCodeFixIsOffered` | fails (reported on the implementing part today) |
| RH2004: `VerifyPartialPropertyInTwoDocumentsIsReportedOnTheDefiningPartInTheFirstDocument` | SHARED-DIAG-012 | `Verify`, two documents | fails |
| RH2004: `VerifyPartialPropertyInTwoDocumentsIsReportedOnTheDefiningPartInTheSecondDocument` | SHARED-DIAG-012 | `Verify`, two documents | fails |
| RH2004: `VerifyPartialPropertyWithGeneratedDefiningPartIsReportedOnTheImplementingPart` | SHARED-DIAG-012 | `Verify`, `.g.cs` | passes |
| RH2004: `VerifyPartialPropertyWithGeneratedImplementingPartIsReportedOnTheDefiningPart` | SHARED-DIAG-012 | `Verify`, `.g.cs` | passes |
| RH2004: `VerifyPartialPropertyWithBothPartsGeneratedIsNotReported` | SHARED-DIAG-012 | `Verify`, `.g.cs` | passes |
| RH2002: `VerifyAsyncVoidPartialMethodIsReportedOnceOnItsImplementingPart` | RH2002-001, SHARED-DIAG-004, SHARED-DIAG-012 | `Verify` | fails (reproduction) |
| RH2002: `VerifyAsyncVoidPartialMethodWithImplementingPartFirstIsReportedOnItsImplementingPart` | RH2002-001, SHARED-DIAG-012 | `Verify` | fails |
| RH2002: `VerifyAsyncVoidPartialMethodInTwoDocumentsIsReportedOnItsImplementingPart` | SHARED-DIAG-012 | `Verify`, two documents | fails |
| RH2002: `VerifyAsyncVoidPartialMethodWithGeneratedImplementingPartIsReportedOnItsDefiningPart` | RH2002-001, SHARED-DIAG-012 | `Verify`, `.g.cs` | passes |
| RH2002: `VerifyAsyncVoidPartialMethodWithGeneratedDefiningPartIsReportedOnItsImplementingPart` | SHARED-DIAG-012 | `Verify`, `.g.cs` | passes |
| RH2101: `VerifyNestedPartialClassWithStaticOnTheFirstPartIsNotReported` | RH2101-002, SHARED-DIAG-004 | `Verify` | fails (reproduction) |
| RH2101: `VerifyNestedPartialClassWithStaticOnTheSecondPartIsNotReported` | RH2101-002, SHARED-DIAG-004 | `Verify` | fails |
| RH2101: `VerifyNestedPartialClassInTwoPartsOfTheOuterClassWithStaticOnOnePartIsNotReported` | RH2101-002, SHARED-DIAG-004 | `Verify` | fails |
| RH2101: `VerifyNestedPartialClassWithStaticOnAPartInAnotherDocumentIsNotReported` | RH2101-002, SHARED-DIAG-004 | `Verify`, two documents | fails |
| RH7002: `FurtherPartOfAPartialTypeIsNotReported` | RH7002-004 | `Verify` | fails (reproduction) |
| RH7002: `PartialTypesOfOtherNamespaceOrArityAreReportedOnTheirFirstPart` | RH7002-004, RH7002-001 | `Verify` | fails |
| RH7002: `PartialDeclarationOfANewTypeAfterAnotherTypeIsReported` | RH7002-001; FC-003 | `Verify` | passes |
| RH7002: `FurtherPartAfterAnotherTypeIsNotReported` | RH7002-004, RH7002-001 | `Verify` | fails |
| RH7002: `FurtherPartInNestedAndQualifiedNamespaceOfTheSameNameIsNotReported` | RH7002-004 | `Verify` | fails |
| RH7002: `FurtherPartInAnotherDeclarationOfTheSameNamespaceIsNotReported` | RH7002-004 | `Verify` | fails |
| RH8306: `VerifyPartialPropertyWithTwoSentenceSummariesIsReportedOnceOnTheDefiningPart` | RH8306-001, SHARED-DIAG-012 | `Verify` | fails (reproduction) |
| RH8306: `VerifyPartialPropertyWithImplementingPartFirstIsReportedOnceOnTheDefiningPart` | RH8306-001, SHARED-DIAG-012 | `Verify` | fails |
| RH8306: `VerifyPartialPropertyWithSentenceSummaryOnTheImplementingPartIsReportedThere` | RH8306-001, SHARED-DIAG-012 | `Verify` | passes |
| RH8306: `VerifyPartialPropertyWithSentenceSummaryOnTheDefiningPartIsReportedThere` | RH8306-001, SHARED-DIAG-012 | `Verify` | passes |
| RH8306: `VerifyPartialPropertyWithNounPhraseSummariesIsNotReported` | RH8306-001; FC-003 | `Verify` | passes |
| RH8306: `VerifyPartialPropertyWithGeneratedDefiningPartIsReportedOnTheImplementingPart` | SHARED-DIAG-012 | `Verify`, `.g.cs` | passes |
| RH7401–RH7408: `VerifyPartial{Method,Property,Event,Indexer}IsReportedOnceOnItsDefiningPart` (one per rule, the spec example) | RH74nn-001, SHARED-DIAG-004, SHARED-DIAG-012 | `Verify` | fails (8 tests) |
| RH7409–RH7412: the same test per rule (spec example) | RH74nn-001, SHARED-DIAG-004, SHARED-DIAG-012 | `Verify` | passes (4 tests; these rules already report once on the defining part) |
| RH7401: `VerifyPartialOverrideMethodWithImplementingPartFirstIsReportedOnceOnItsDefiningPart` | SHARED-DIAG-012 | `Verify` | fails |
| RH7401: `VerifyPartialOverrideMethodWithGeneratedImplementingPartIsReportedOnItsDefiningPart` | SHARED-DIAG-012 | `Verify`, `.g.cs` | passes |
| RH7405: `VerifyPartialNonOverrideMethodWithImplementingPartFirstIsReportedOnceOnItsDefiningPart` | SHARED-DIAG-012 | `Verify` | fails |

### Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| RH2002 async partial method | LF | analyzer | reproduced | reported on both parts vs once on the implementing part |
| RH2004 partial property, indexer, event | LF | analyzer, code fix | reproduced | reported on both parts, fix offered on each vs once on the defining part, no fix |
| RH2004 partial constructor | LF | analyzer | reproduced | reported on the implementing part vs on the defining part, no fix |
| RH2101 `static partial` + `partial` nested class | LF | analyzer | reproduced | part without `static` reported vs nothing |
| RH7002 two parts of `partial class C` | LF | analyzer | reproduced | second part reported vs nothing |
| RH8306 partial property, both summaries sentences | LF | analyzer | reproduced | reported twice vs once on the defining part |
| RH7401–RH7408 partial member, both parts outside a matching region | LF | analyzer | reproduced | reported on both parts vs once on the defining part |
| RH7409–RH7412, same shape | LF | analyzer | not a defect today | already reported once on the defining part; tests pin it |

### Not covered, and why

- The skeptic's `partial class C<T>` plus `partial class C<U>` (RH7002): CS0264, the parts of a partial type must use the same type-parameter names. The input does not compile, so no test was written.
- A partial member where only some parts would be reported (RH7401–RH7412): RH74nn-001 says this is not covered (#1334). The existing RH7409/RH7410 characterizations stay.
- Generated parts for RH7402–RH7412: tested on RH2004, RH2002, RH8306, and RH7401. These cover three diagnostic positions: identifier, summary, and region-judged member. No proof was made that the other RH74xx rules share a call site, so this is a limitation, not a proof.
- Line endings: these are report-location tests with no layout, so they have no CRLF variants.

## thoenissen/Reihitsu#1288

### Tests

File `Design/RH2004AccessModifierMustBeDeclaredAnalyzerTests.cs`.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `VerifyExplicitInterfaceOperatorAndConversionImplementationsAreNotReported` | RH2004-003 | `Verify` | fails (reproduction; spec example) |
| `VerifyExplicitInterfaceUnaryAndCheckedOperatorImplementationsAreNotReported` | RH2004-003; FC-024, FC-025 | `Verify` | fails |
| `VerifyExplicitInterfaceImplicitConversionImplementationIsNotReported` | RH2004-003; FC-024, FC-025 | `Verify` | fails |

### Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| `static N IAdd<N>.operator +`, `static explicit IAdd<N>.operator int` | LF | analyzer | reproduced | 2 diagnostics vs none |
| unary `-`, `+`, `checked +` with block bodies | LF | analyzer | reproduced | 3 diagnostics vs none |
| `static implicit IConvert<N>.operator long` | LF | analyzer | reproduced | 1 diagnostic vs none |

### Not covered, and why

- Code-fix path: nothing is reported once the change is in, so no fix can be offered.

## Whole run

### Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH2005FieldsMustBePrivateAnalyzerTests|FullyQualifiedName~RH2004AccessModifierMustBeDeclaredAnalyzerTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzerTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH2002AsyncVoid|FullyQualifiedName~RH2101Nested|FullyQualifiedName~RH7002File|FullyQualifiedName~RH8306Property|FullyQualifiedName~Formatting.RH74"
scripts/test.sh --project architecture --filter "FullyQualifiedName~RequirementCoverage"
```

### Failure classes not applicable

- FC-010, FC-011, FC-012, FC-013, FC-014, FC-028, FC-029: formatter stability and parity. No formatter phase is involved.
- FC-018: no naming fix changes. The RH2001 rename is unchanged and goes through the Renamer.
- FC-022, FC-023, FC-026: no literal, identifier-form, or repeated-token edit is changed.
- FC-037: no comment-only change.

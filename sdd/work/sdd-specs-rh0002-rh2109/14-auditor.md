Role: auditor
Commit: 9f735c506fc359e65e8649560bf135118e53d56a
Verdict: findings

## Scope

- Head `9f735c5` is checked out, and the working tree was clean before this file. `origin/main` is `6ff1b0f14a45602f1dc76ed609f391b4659a0523`, which `git ls-remote origin refs/heads/main` also returns, and it is an ancestor of the head. So the merge is in, and `6ff1b0f...HEAD` equals `6ff1b0f..HEAD`.
- This is a full audit of the PR diff, 58 paths:
  - 18 rule specs, 3 shared specs, the index, and the ledger;
  - the rule pages `documentation/rules/RH1002.md` and `RH2004.md`;
  - 18 analyzer test classes;
  - the decisions entry `squad/decisions/2026-10-20-996-rh2001-002-second-spec-check.md`;
  - `sdd/work/`.
- No file under `src/Reihitsu.Analyzer`, `src/Reihitsu.Analyzer.CodeFixes`, `src/Reihitsu.Core`, or `src/Reihitsu.Formatter` changed. `git log af49c40..HEAD` on those paths is empty, so the cleaner's Release DLLs match the head.
- `scripts/lane-check.sh tooling` reports: "LANE CHECK: confirmed — claimed tooling, diff needs tooling". The diff matches what 00 to 13 describe.
- The last merge, `9f735c5`, has no handover. Against main it only adds index rows and ledger IDs. The ledger passes `LC_ALL=C sort -c` and has no duplicates, and every code owner the run claims appears in exactly one spec.
- My prompt carried no intended fix, suspected cause, or hoped-for verdict. It named the scope and the maintainer constraints of 00-lead.

How I checked:
- I traced every requirement statically against its analyzer, code-fix provider, and helper.
- I settled each suspicion by running one of these, all outside the repository and deleted afterwards:
  - a scratch `net10.0` / `LangVersion preview` project under the scratchpad, with the head's Release `Reihitsu.Analyzer.dll` and `Reihitsu.Core.dll` as `<Analyzer>` items, read with `dotnet build`;
  - `scripts/apply-fix.sh RH2001|RH2004 <scratch dir> --no-install`.

## Findings

| # | Severity | Location | Failure class | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|
| 1 | medium | `sdd/specs/analyzer/RH1002.md` RH1002-001 (and RH1002-005) | FC-027 (another form of the same call), FC-002 | same requirement | RH1002-001 says the analyzer MUST report "a call of one of the methods … `GroupBy`, `Join`, `GroupJoin` … that binds to a method declared on `System.Linq.Enumerable`". A query expression binds to exactly those methods, but the analyzer only registers `InvocationExpression`. Verified in the scratch build with `IEnumerable<S1> items` and a struct `S1` without equality members: `from x in items group x by x`, `from a in items join b in items on a equals b select a`, and `… join b in items on a equals b into g select g` get no RH1002, while `items.GroupBy(x => x)` in the same method does (`A.cs(20,28): warning RH1002`). The spec states a report that the code does not make, and no coverage sentence or non-goal leaves query syntax out. The case also looks wrong: the clauses hash `S1` through `EqualityComparer<S1>.Default`, exactly what the rule reports for the method form. It is in no question text of 09 (`rg` over `sdd/work/…` and the specs finds no mention). This breaks clarification 9, because done means the spec states current behavior and every looks-wrong case is filed. | Add a coverage sentence to RH1002-001 that limits it to calls written as invocations, so query clauses are left out. File the case as a new question in the `SDD open questions: sdd/specs/analyzer/RH1002.md` text, touching RH1002-001 and RH1002-005. Pin it with an untagged characterization test. |
| 2 | medium | `sdd/specs/analyzer/RH1001.md` RH1001-003; `sdd/specs/shared/struct-equality.md` SHARED-EQUAL-002 | FC-027 (complex sibling: `throw` branch or arm, collection expression), FC-002 | same requirement | RH1001-003 says the declared type MUST NOT be reported "when every creation that its initial value … can produce is a creation of that type with a custom comparer". Its Why says "free of that cost only when every collection it can hold was created that way". `HasExplicitEqualityComparerInitializer` treats every operand outside parentheses, `!`, non-user-defined casts, conditional branches, `switch` arms, and creations as "no comparer". Verified: `Dictionary<S1, int> d = _mode switch { 0 => new(_comparer), _ => throw new InvalidOperationException("x") };` and `… = _flag ? new(_comparer) : throw …;` are both reported on the declared type's `S1` (`A.cs(27,20)`, `A.cs(32,20)`). So is `HashSet<S1> set = [with(comparer), a];` under `LangVersion preview` (`A.cs(11,17)`). In each case every collection the declaration can hold uses the custom comparer. The spec's condition and Why say "not reported", or at best leave it undecided (if the `MAY` sentence is read as the exhaustive list of forms). In both readings, a report on a declaration whose every collection has a custom comparer looks wrong and is not filed. Related wording problem: `Dictionary<S1, int> d = _other ?? new(_comparer);` is reported (`A.cs(33,20)`). That matches the Why, but "every creation that its initial value can produce" does not say that a non-creation operand makes the type reported. The same gap exists in SHARED-EQUAL-002. Its first sentence decides by what an argument "can produce", and `items.Distinct(flag ? null : throw new InvalidOperationException("x"))` (and the `switch` form) can produce only `null`, yet it counts as custom and is not reported (scratch build, no RH1002 on lines 13 and 14, while `flag ? null : EqualityComparer<S1>.Default` on line 15 is reported). | Reword RH1001-003 so that its condition names what the analyzer recognizes: the initial value is such a creation, maybe wrapped, or picks only between such creations in branches or arms, and anything else leaves the declared type reported. Add coverage sentences that leave out a `throw` branch or arm and a collection expression with `with(…)` (RH1001-003), and a `throw` branch (SHARED-EQUAL-002). File the `throw` arm and `with(comparer)` reports as RH1001 questions (new issue `SDD open questions: sdd/specs/analyzer/RH1001.md`), and the SHARED-EQUAL-002 `throw` branch in the struct-equality issue. Pin them with untagged characterization tests. |
| 3 | medium | SHARED-EQUAL-002 example and its tagged tests (`RH1002…Tests.VerifyOnlyArgumentsThatCanProduceAComparerCountAsCustom`, `VerifyCompositeNullComparerDoesNotExempt`, `VerifyWrappedNullComparerArgumentsDoNotExempt`; `RH1001…Tests.VerifyCreationsAndDeclarationsWithCustomComparerAreNotReported`); RH1001-003 example | FC-003, FC-017 | same requirement | Several qualifiers have an example and a test on one side only (`sdd/README.md`: "each with an example on either side"; the skeptic's FC-002 row asked for both sides). Uncovered sides: a `??` with a custom operand (`comparer ?? null`), a null-conditional access with a receiver that is not `null` (`holder?.Comparer`), a cast with a user-defined conversion in SHARED-EQUAL-002 and in RH1001-003's initial value, and parentheses or `!` around a custom comparer. The scratch build confirms the current behavior on the custom side: `items.Distinct(comparer ?? null)`, `items.Distinct(holder?.Comparer)`, `items.Distinct((comparer))`, and `items.Distinct(comparer!)` are not reported. Grepping every `??` and `?.` in both test classes finds only null-like operands and `((ComparerHolder)null)?.Comparer`, and there is no `operator` in either class. A mutation that treats every `??` or every conditional access as non-custom, or unwraps user-defined casts, therefore stays green. `04-test-implementer.md` lists "null-conditional on `null` … both sides" for the first test above, which overstates its coverage. | Add the custom-side cases to the SHARED-EQUAL-002 and RH1001-003 examples and to tagged tests, expecting no diagnostic, and correct the claim in the test implementer's handover. |

## Guard scope

No predicate, guard, or exemption in code is added or changed. The table compares each requirement's stated scope (coverage sentence included) with the code predicate that makes the decision.

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| RH1001-003, declared or cast type exempt | the initial value, through parentheses, `!`, non-user-defined cast, conditional (constant condition picks a branch), `switch` arms, explicit or target-typed creation | `AreInitializerCreationsUsingExplicitEqualityComparers` / `HasExplicitEqualityComparerInitializer` (other operands: `default` → `false`) | disagree for a `throw` branch or arm, a collection expression with `with(…)`, and the wording for non-creation operands | finding 2 |
| RH1001-002, target-typed `new()` covered by the named type | ancestors of the `new()` through parentheses, `!`, conditional branch, `switch` arm, up to a cast or the equals-value of a declarator or property | `IsAnalyzedByTargetTypeReference` | agree, including field, property, and cast forms (`VerifyTargetTypedCreationInFieldPropertyAndWrappedCastIsCoveredByNamedType`) | ok |
| RH1002-001, which calls are checked | every call of a listed method bound to a listed type | `RegisterSyntaxNodeAction(…, InvocationExpression)` + `IsRelevantMethodName` + `IsRelevantContainingType` | disagree for query clauses | finding 1 |
| SHARED-EQUAL-002, non-custom argument | the argument expression; the listed forms | `IsDefinitelyNonCustomEqualityComparerExpression` and helpers (`UnwrapOperation` stops at user-defined conversions) | agree on the listed forms; disagree for a `throw` branch (finding 2); a variable holding `null` counts as custom (hint 1) | finding 2 |
| SHARED-EQUAL-001, lacks equality members | the compared type | `TypeKind.Structure` + `AreEqualityMembersImplemented` (`AllInterfaces` contains `IEquatable<self>`, or declared overrides of both) | agree; Q1/Q2 left out by the coverage sentence | ok |
| RH1002-002, compared type | key selector by parameter name `keySelector`/`outerKeySelector`, else first type argument | `GetEqualityComparisonType` / `FindKeySelectorParameter` | agree for every listed method, the `KeyValuePair` overloads, and the .NET 8 tuple `ToDictionary` | ok |
| RH1002-004, `Contains` on a dictionary | the static type of argument 0 after implicit conversions; 2-parameter overload | `IsDelegatedToDictionaryKeyLookup` / `IsReadOnlyDictionaryInterface` | agree | ok |
| RH2001-001/-002, reported property | private symbol, backing field exists, no property attribute, no EII | `OnPropertySymbol` + `IsAutoProperty` | agree with coverage. A partial property whose implementing part uses `field` is reported on that part, with no fix (scratch: `A.cs(13,28)`; `apply-fix.sh RH2001`: no-fix-offered), the same class as Q4 | ok |
| RH2001-004/-005/-006, fix offered | declaration syntax and accessors; members named like the field | `IsSupportedPropertyShape`, `HasFieldNameCollision` (re-checked in `ApplyCodeFixAsync`) | agree; Q1–Q3 and Q6 left out by the coverage sentence | ok |
| RH2004-001…-004, reported declaration | the declaration's own modifiers, parent kind, EII specifier | `ShouldSkip` + `DeclarationModifierUtilities.HasAccessibilityModifier` (counts `file`) | agree with coverage (EII operators, partial constructor) | ok |
| RH2004-006, inserted modifier | the parent node | `CreateUpdatedDeclaration` (compilation unit/namespace → `internal`, interface → `public`, otherwise `private`, including extension blocks) | agree. A leading `//` comment, `/* */` comment, `#if`, and `#pragma` stay in front (`apply-fix.sh RH2004`, LF and CRLF) | ok |
| SHARED-NESTED-001 | the direct parent | `NestedTypeAnalyzerHelper.IsNestedType` (`Parent is TypeDeclarationSyntax`) | agree for valid code (`ExtensionBlockDeclarationSyntax` cannot contain types) | ok |
| RH2101-002, static exemption | one declaration part | `IsStaticType` | agree with coverage (Q1 left out) | ok |
| SHARED-DEBUG-001 / RH2006-001 / RH2007-001 | the argument named `message`, else the one at its ordinal; its constant value | `ShouldReport` + `IsConstantMessageWithoutText` | agree, including the `AssertInterpolatedStringHandler` overloads (constant `$""` is reported) | ok |
| RH2109-001/-003 | the raw text of `.razor` additional files (case-insensitive) | `@code\s*\{` regex; span of 5 characters | agree with coverage (Q1–Q4 left out) | ok |
| RH0002-002 | the first token, including zero-width | `GetFirstToken(includeZeroWidth: true)` (the EOF token for an empty or comments-only file; the `TextSpan(0, 0)` fallback is unreachable) | agree | ok |
| RH2003-001 | the written simple name | `GetSimpleTypeName` pre-filter + display-string check | agree with coverage (alias left out) | ok |

## Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| SHARED-EQUAL (struct equality members, custom comparer) | `StructEqualityPerformanceAnalyzerBase`, `EqualityComparerArgumentUtilities`; users only `RH1001…Analyzer`, `RH1002…Analyzer`; no other `IEquatable\`1`/`IEqualityComparer\`1` logic under `src/` outside tests | single owner |
| SHARED-NESTED (nested declaration, location) | `NestedTypeAnalyzerHelper`; users RH2101, RH2104–RH2108 | single owner |
| SHARED-DEBUG (constant without text) | `DebugMessageTextAnalysisUtilities`; users RH2006, RH2007; other `IsNullOrWhiteSpace` hits are unrelated | single owner |
| Accessibility modifier detection and insertion | `DeclarationModifierUtilities` (code owner of `analyzer/RH2005.md`); RH2004 uses it and does not claim it (clarification 4) | ok |
| Modifier order of the RH2001 field | `ModifierOrderingUtilities.OrderModifiersForRh7105`; claimed by no spec (clarification 4) | ok |
| Field name of RH2001 | `CasingUtilities` (SHARED-CASING), referenced through `Uses` SHARED-CASING-006/-007 | ok |
| "Auto-implemented" (backing field exists) | `PropertySymbolExtensions.IsAutoProperty`; only user RH2001; unclaimed | ok (hint 2) |
| Spec ownership of claimed types | each claimed type appears in exactly one spec plus the index (`rg` on the backticked name) | ok |

## Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH0002 · `DocumentationModeDiagnose`, `…OnFirstTokenAfterLeadingComments`, `…CommentsOnlyFileIsReportedAtEndOfFile`, `…EmptyFileIsReportedAtStart` | mode `Diagnose` not reported; the span is the first token, or empty at the end | any diagnostic; a span at the comment, at the file start (line 1 vs 2), or of non-zero length | `Verify` with markup (exact set and span) | adequate |
| RH1001 · `VerifyOnlyStructsWithoutTheirOwnEqualityMembersAreReported`, `VerifyEveryHashBasedCollectionTypeIsReportedOnItsKey`, `VerifyEveryUseOfTheCollectionTypeIsReported`, `VerifyTargetTypedCreationInFieldPropertyAndWrappedCastIsCoveredByNamedType`, `VerifyPropertyInitializedWithCustomComparerIsNotReported`, `VerifyUsesOfAliasesAreReportedOnTheAliasName` | the spec examples, exact locations | a missing, extra, or moved diagnostic | `Verify` + `Diagnostics(…, n)` (exact) | adequate |
| RH1001 · `VerifyCreationsAndDeclarationsWithCustomComparerAreNotReported` | RH1001-003 forms | as above | exact | weaker than the requirement for the user-defined cast (finding 3) |
| RH1002 · `VerifyEveryListedMethodIsReported`, `…NotReportedForTypesWithEqualityMembers`, `VerifyComparedTypeIsTheSelectedOrPairKey`, `VerifyMethodsOfOtherTypesAreNotReported`, `VerifyContainsOnDictionaryIsNotReported`, `VerifyDiagnosticLocation…`, `VerifyConstantConditionCountsOnlyChosenBranch` | the spec examples | as above | exact | adequate |
| RH1002 · `VerifyOnlyArgumentsThatCanProduceAComparerCountAsCustom` | SHARED-EQUAL-002 on both sides | custom side of `??`, `?.`, user-defined cast, parentheses/`!` | exact | weaker than the requirement (finding 3) |
| RH2001 · `VerifyFixReplacesPropertiesWithFieldsAndRenamesReferences` (+ CRLF), `…StructPropertyAndRenamesReferencesInNestedType`, `VerifyFixDerivesFieldNameFromWordsOfPropertyName` | exact fixed output, references renamed (`nameof`, other type) | any byte of output differs, or the diagnostic remains after the fix | `Verify(source, fixed)` (fix applied and re-analyzed), CRLF variant | adequate |
| RH2001 · `VerifyPrivateAutoPropertiesOfEveryShapeAreReported`, `VerifyOtherPropertiesAreNotReported`, `VerifyNoFixIsOfferedForInitAndModifiedAccessors`, `VerifyNoFixIsOfferedWhenFieldOrMethodHasTheFieldName` | report set; no action on each diagnostic | an extra or missing diagnostic; any registered action | `Verify`; `VerifyNoCodeFixIsOffered` (asserts diagnostics, then zero actions per diagnostic) | adequate |
| RH2002 · `VerifyStaticEventHandlerVirtualAndOverride…`, `VerifyAsyncAnonymousFunctionsAreReportedOnlyForVoidDelegates`, `VerifyAsyncValueTaskAndSynchronousVoid…` | report set and location | as above | exact | adequate |
| RH2003 · `VerifyQualifiedAndInitializedCreations…`, `VerifyTargetTypedCreations…`, `VerifyDerivedAndSameNamedTypes…` | report set; span includes the qualification | as above | exact | adequate |
| RH2004 · `VerifyDeclarationsWithoutAccessibilityModifierAreReported`, `VerifyOperators…OnTheirToken`, `VerifyPartialTypeAndPartialMethodParts…`, `VerifyStaticConstructorAndExplicitInterface…`, `VerifyInterfaceMembers…`, `VerifyDiagnosticIsOnFirstDeclarator…` | report set and location | as above | exact (`CompilerDiagnostics.None` only where the input is CS0558) | adequate |
| RH2004 · `VerifyFixAddsDefaultAccessibilityOfEachContext` (+ CRLF), `VerifyFixAddsPrivateInStructAndRecordsAndInternalAtFileLevel` | exact fixed output per context, including the extension block | any byte differs | `Verify(source, fixed)` | adequate |
| RH2006/RH2007 · `VerifyEveryFormOfConstantWithoutTextIsReported` (real U+00A0 in source), `…WithoutMessageTextAreReported`, `VerifyOnlyMessageArgumentIsChecked`, `…OfOtherTypesAreNotReported`, `VerifyDiagnosticCoversWholeMultiLineInvocation` | report set; span is the whole invocation | as above | exact | adequate |
| RH2101–RH2108 new tests | nested set, depth, partial parts, record exclusions, identifier location | as above | exact | adequate |
| RH2109 · `VerifyCodeBlocksWithBraceOnSameAndNextLine…`, `…WithoutWhiteSpaceBeforeBrace…`, `VerifyIndentedCodeBlock…`, `VerifyUpperCaseRazorExtension…`, `VerifyCodeBlockInCshtmlFile…` | 5-character span at the directive | a wrong column, length, or count | `RazorCodeBlockSpan` (`WithSpan` start and end) | adequate |
| Characterization tests (all untagged): RH1001 Q1/Q2, RH1002 struct-equality Q3 / RH1002 Q1, RH2001 Q1–Q6 and #971, RH2002 Q1/Q2, RH2003 Q1, RH2004 Q1–Q5 and #967, RH2006/RH2007 SHARED-DEBUG Q1, RH2101 Q1, RH2109 Q1–Q4 | pin today's output of each filed input | output changes | `Verify`, `VerifyNoCodeFixIsOffered`, `ApplyCodeFixAsync` + `Assert.Contains`; the partial-property one asserts that exactly one part changed, because the order of diagnostics varies | adequate for pinning. Each uses the issue text's input; doc comments say "Characterizes …" |

## Failure classes dismissed

- FC-001, FC-005: no guard was narrowed or added. The run changes no production code.
- FC-004, FC-019, FC-020, FC-021:
  - RH2001's comment and `#if` cases are filed (Q1, Q2) and pinned.
  - For RH2004, I verified that a leading `//` comment, a `/* */` comment on the same line, `#if`, and `#pragma` stay in front of the inserted modifier (`apply-fix.sh`, LF and CRLF).
  - The attribute-line case is follow-up #967, pinned.
- FC-006, FC-024:
  - RH2001: a partial property is reported only on its implementing part, with no fix (verified), the same class as Q4.
  - RH2004: EII operators and partial members are filed as Q2–Q5.
  - RH2101: the static partial class is filed as Q1. RH2002: EII and partial methods are filed as Q1 and Q2. RH2102 and RH2103: partial is covered.
  - RH2001: `readonly` and mutable-struct properties are filed as Q6 and Q3.
- FC-007, FC-008: the verifier applies the RH2001 and RH2004 fixes and re-analyzes. `FixAllCorrectsEveryDiagnostic` already exists for both. `apply-fix.sh RH2004` converges on every fixture I ran.
- FC-009: RH2001 registers a fix only when `IsSupportedPropertyShape` holds and there is no collision, and `ApplyCodeFixAsync` repeats both checks. RH2004 always changes the document.
- FC-010 to FC-014, FC-028, FC-029: no formatter change. No formatter phase owns these design and performance concerns.
- FC-015, FC-016: see Policy ownership. No fix was changed, so no sibling shape stayed unfixed.
- FC-018: RH2001 renames through `Renamer.RenameSymbolAsync`. A reference in another type is tested.
- FC-022: RH2109's raw-string, string, and comment inputs are filed (Q1, decided by P5) and pinned. No other rule reads text.
- FC-023: SHARED-DIAG-002 tests cover RH2003's verbatim name. RH2001 derives the field name from `ValueText`.
- FC-025: RH2002 tests expression-bodied methods and lambdas, and the expression-bodied `field` property of RH2001 is filed in Q4.
- FC-026: no rewrite re-resolves tokens.
- FC-027: findings 1 and 2. The other requirements were checked against alternative forms: aliases, `using static`, `global::`, base lists, `typeof`, constraints, field and property initializers, casts, arrows, arguments, and multi-line spans. All agree.
- FC-030: RH2001 formats only the new field (`FormatNodeInDocumentAsync`). RH2004 formats nothing.
- FC-031: no production method changed. The doc comments of new tests describe their inputs.
- FC-032, FC-035: no compiled change, and expected values come from the unchanged code and the spec examples.
- FC-033: CRLF variants exist for both fixes. The RH2109 and RH0002 spans are computed from line tables.
- FC-034: every characterization test uses the exact input of its question text.
- FC-036: `nameof` is decided by the SHARED-FIX-003 run-time bound and pinned. See hint 4 for reflection.
- FC-037: no claim that a `.cs` change is text-only.

## Hints

1. SHARED-EQUAL-002 decides by what an argument "can produce". Its list starts with "when", not "exactly when". So `IEqualityComparer<S1> local = null; items.Distinct(local);` reads as not custom, but it is not reported (scratch build, line 17). A wording such as "as the argument's form shows" would keep the semantic reading out. This is uncertain, so it is not a finding.
2. `PropertySymbolExtensions.IsAutoProperty` is used only by RH2001 and implements the decision that RH2001-001 specifies. Under the ownership rule in `sdd/README.md`, it could be a code owner of `analyzer/RH2001.md`. As written, the type reads as a general-purpose extension.
3. RH2004's fix also adds `private` to other declarations that already fail to compile because they cannot be private: `abstract void M();` in an abstract class gives `private abstract void M();` (`apply-fix.sh RH2004`; CS0621 before and after). Overrides and `virtual` members without a modifier presumably behave the same; I ran only the `abstract` case. This is the same shape as RH2004 Q1, whose text names only operators.
4. RH2001's fix changes the member kind from property to field. Reflection (`GetProperty`) and expression trees see that change. The decision `2026-10-18-shared-fix-003-rename-not-runtime-change.md` exempts only the new name and accessibility. Whether the kind change the fix exists to produce counts is not decided.
5. No architecture-test run happened after merge `9f735c5`. The validator covers it.

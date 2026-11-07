Role: auditor
Commit: 42db22c37641c0f41e0d6ac2617869e0b61c6541
Verdict: findings

The prompt carried no intended fix, suspected cause, or hoped-for verdict, so there was nothing to ignore.

State check: the checkout is `42db22c`; `origin/main` is `b434355` and is an ancestor of HEAD; `scripts/lane-check.sh bug-fix` confirms the lane. The diff (`b434355..42db22c`, 67 files) is what handovers 01–09 describe: specs, tests, the 15 production files of `05-implementer.md`, and the harness change of `06-test-implementer.md`. I trusted the cleaner's build (`08-cleaner.md`) and the roles' focused test results.

Probes ran as throwaway test methods in two scratch `git worktree`s outside the repository (one at HEAD, one at `origin/main`), with `scripts/test.sh --configuration Debug --project analyzer --filter "Name~Probe…"`. Both worktrees are removed, and the repository tree is unchanged. Every "fails" below was observed, not inferred, unless it says "by trace".

## thoenissen/Reihitsu#967

### Scope
Full audit. Head `42db22c`, base `b434355`. Files: `src/Reihitsu.Core/DeclarationModifierUtilities.cs`, `RH2004AccessModifierMustBeDeclaredCodeFixProvider.cs` (partial guard, see #1287), `RH2005FieldsMustBePrivateCodeFixProvider.cs`, `ExcludedCodeUtilities.cs` (RH2005 consumer), and the RH2004/RH2005 tests.

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | high | `src/Reihitsu.Core/DeclarationModifierUtilities.cs:108-118` with `:137-157` | FC-004, FC-021, FC-016 | – | same requirement | When the replaced `public` is **not** the first modifier and starts its own line, its leading trivia is lost. The token-list overload gives the new `private` the old `public`'s leading trivia. The member overload then overwrites the first token's leading trivia with the trivia it captured from `static`. Input (RH2005): `    static⏎        // c⏎        public int Counter;` gives `    private static⏎int Counter;`. The comment line is dropped, and `int` moves to column 0 (the RH2005 fix does not format). This is the same on `main` (probe on `b434355`), so it is pre-existing. It is a member of #967's class that the matrix in `02-skeptic.md` did not span: the axis "trivia of that token" covered only the first token behind the attribute lists, not the trivia of the replaced accessibility keyword. A directive line in the same place is lost the same way (by trace). It violates SHARED-FIX-003 (in RH2005 `Uses`) and RH2005-003's "keeping the declaration's … attributes, documentation comment". Members that hold: `public` first, with any leading trivia (captured as the first token's trivia); `static public` on one line (`VerifyFixPutsPrivateInFrontOfStaticInTheFieldsColumn`); attributes on their own line (the new tests). RH2004 cannot reach it, because it reports only declarations without an accessibility keyword. | Carry the leading trivia of a removed accessibility keyword that is not the first modifier onto the token that follows it, or withhold the fix when that trivia holds a comment or directive. Add tests: the comment line, the directive line, and the indentation-only line (LF and CRLF). |

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Where the inserted accessibility keyword takes its leading trivia (`GetFirstTokenBehindAttributeLists`) | First modifier, else the token after the last attribute list, else the declaration's first token | Token-list overload `AddAccessibilityModifiers(SyntaxTokenList, …)`, which reuses the old accessibility token's leading trivia | Disagree when the old accessibility token is not first and carries leading trivia: the member overload overwrites what the list overload carried | finding #1 |
| RH2005-004 inside/outside (`IsInsideBody`) | `OpenBrace.End … CloseBrace.SpanStart` of every `TypeDeclarationSyntax` of the containing type | RH2005-004 text (Q3 = B: every reference outside the bodies counts; nested types and other parts are inside) | Agree: attributes of the type, base-list arguments, and `cref` in the type's own documentation comment are outside, which is what Q3 = B decided | ok |
| RH2005 excluded-code filter (`isCounted`) | `DisabledTextTrivia.SpanStart` against the same bodies | The same `IsInsideBody` for active references | Agree, except disabled text that starts inside a body and ends outside it, which is judged by its start only (see Hints) | ok |
| RH2004 fix withheld for `partial` | `memberDeclaration.Modifiers` has `partial` | Analyzer's partial path, `modifiers.Any(PartialKeyword)` | Same token test | ok |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Trivia of an inserted accessibility keyword | `rg -n "AddAccessibilityModifiers?\(" src --glob '!*Test*'`: only `DeclarationModifierUtilities` (production callers: RH2004 fix, RH2005 fix) | one owner |
| Excluded-`#if` occurrence search | `rg -n DisabledTextTrivia src/Reihitsu.Analyzer.CodeFixes`: only `ExcludedCodeUtilities.HasExcludedCodeOccurrenceAsync` performs the identifier search. The other hits (`ModifierOrderingCodeFixProviderBase`, RH2001 guard, RH3001, RH3103, RH7501) are trivia-preservation guards, a different decision | one owner |
| Reference search for RH2005-004 | `rg -n FindReferencesAsync src/Reihitsu.Analyzer.CodeFixes`: casing base (rename cascade) and RH2005 (inside/outside); different decisions | ok |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `RH2005…VerifyFixPutsPrivateOnTheFieldsLineBehindAnAttributeListOnItsOwnLine` (+CRLF) | `private` on the field's line, in its column | `private     static` at column 0 | `Verify` exact text, iterative and Fix All, LF and CRLF | adequate |
| `RH2005…VerifyFixKeepsCommentLineInFrontOfTheField` / `…DirectiveLine…` | comment and directive lines stay in front of `private` | the line joined behind `private` or dropped | `Verify` exact text; comment and directive separate (FC-005) | adequate |
| `RH2004…VerifyFixKeepsCommentAndDirectiveLinesInFrontOfTheDeclaration` (+CRLF) | the same for a type and a `static` member under Fix All | relocation | `Verify` exact text | adequate |
| `RH2005…VerifyNoFixIsOfferedForAFieldReadFromAnotherDocument` / `…AnotherProject` | fix withheld across documents and projects | a fix that changes either document | `Verify(src, src)`; fails if any fix changes the text | adequate |
| `RH2005…VerifyNoFixIsOfferedForAFieldNamedInACrefOfAnotherType` | `cref` alone withholds | fix registered | `VerifyNoCodeFixIsOffered`, `DocumentationMode.Diagnose` | adequate |
| `RH2005…VerifyFixAllSkipsTheFieldReadOutsideItsType` | Fix All changes only the fixable fields, and `Used` stays reported | `Used` made private, or a diagnostic lost | `Verify` with `MarkupMode.Allow` on both fixed states (06) | adequate |
| No test for a replaced `public` that is not first and starts its own line | – | – | – | missing, finding #1 |

## thoenissen/Reihitsu#1286

### Scope
Full audit. Files: `RH2001PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider.cs`, `ExcludedCodeUtilities.cs`, `CasingCodeFixProviderBase{T}.cs`, and the RH2001 tests.

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 2 | high | `RH2001PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider.cs:284-311` (guard) and `:199-205` (field built with a new identifier and a new `;`) | FC-001, FC-004, FC-016, FC-003 | – | same requirement | The guard inspects the identifier's trailing trivia, the accessor-list tokens, and the `=`. The field builder drops two more trivia spans that the guard does not inspect, so the fix is offered and the comment or directive is lost. **(a) Leading trivia of `;` behind an initializer.** RH2001-007 covers this ("between its name and the end of its declaration, other than inside its initializer"). `private int Value { get; set; } = 5⏎    // c⏎    ;` gives `private int _value = 5;` with the comment dropped. `#if DEBUG⏎#endif` in the same place is dropped too. **(b) Leading trivia of the name.** This is outside RH2001-007 ("in front of the name … not covered") and outside RH2001-004's coverage, so SHARED-FIX-003 (in `Uses`) decides it. `private int⏎    // c⏎    Value { get; set; }` gives `private int⏎_value;` with the comment dropped. Both reproduce identically on `main`, so they are pre-existing members of #1286's class ("comments dropped") that `02-skeptic.md`'s position matrix recorded as kept ("between type and name") or did not list (`;`). Members that hold: comments behind the name, in the accessor list, between `}` and `=` (the new tests); between modifiers, behind the type on one line, and inside the initializer (kept by the builder); in front of the first token and behind the last. | Withhold the fix for a comment or directive in the name's leading trivia and in the `;`'s leading trivia (or carry them onto the field's identifier and `;`). Better: derive the guard from what the builder drops instead of a second list of positions (see *Architecture findings*). Add tests for (a) comment, (a) directive, and (b), with LF and CRLF for the line-based shapes. |
| 3 | medium | `RH2001PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider.cs:305-310` | FC-015 | – | PR-introduced | The guard lists trivia kinds by hand: directive, disabled text, and four comment kinds. This copies the shared `SyntaxTriviaUtilities.IsCommentTrivia` plus `SyntaxTriviaUtilities.IsDirectiveOrDisabledTextTrivia` (and `SyntaxNodeUtilities.IsCommentOrDirective`) in `Reihitsu.Core`. The two are at parity today. | Use the shared predicates. |

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| RH2001-007 / SHARED-FIX-003: comment or directive the field cannot keep (`HasCommentOrDirectiveInsideDeclaration`) | identifier trailing; every accessor-list token's leading and trailing (the `}` trailing only with an initializer); `=` leading | `TryCreateFieldDeclaration`, which drops the identifier (leading and trailing), the accessor list, and `node.SemicolonToken`'s leading trivia | Disagree on the identifier's leading trivia and the `;`'s leading trivia | finding #2 |
| RH2001-008 value-safe type (`HasValueSafeFieldType`) | property type symbol: type parameter → `IsReferenceType`; `Nullable<T>` → `T` enum or `IsReadOnly` struct; struct → `IsReadOnly`; else true | RH2001-008 / RH2001-004 coverage, Q2 = C, Q4 = A | Agree, including `where T : IFoo` and `where T : notnull` (withheld) and `T? where T : struct` (withheld; `T` is no enum or `readonly` struct) | ok |
| RH2001 excluded code (`HasExcludedCodeOccurrenceAsync`) | old name and `_camelCase` name, in the referring documents of the private property (its containing type's documents) | SHARED-CASING-007 coverage as the casing fixes apply it through the same `ExcludedCodeUtilities` | Same helper and scope rule | ok |
| `readonly` added once (`IsGetOnlyProperty && !readonly`) | the property's modifiers | RH2001-004 "keeps that one modifier" | Agree | ok |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Excluded-code identifier search | `ExcludedCodeUtilities` only. The casing base no longer has a copy (`rg -n "GetReferenceScope|GetReferringDocuments" src`: only `ExcludedCodeUtilities`) | one owner |
| "struct declared `readonly`" | `rg -n "\.IsReadOnly\b\|IsReadOnly:" src --glob '!*Test*'`: RH2001 fix only (RH3005's hit is about fields) | one owner |
| "comment or directive trivia" kind test | RH2001 private list vs `SyntaxTriviaUtilities.IsCommentTrivia`/`IsDirectiveOrDisabledTextTrivia`, `SyntaxNodeUtilities.IsCommentOrDirective` | finding #3 |
| Casing-fix parity after the extraction | `CasingCodeFixProviderBase{T}.cs:752`: `GetOldNames(…).Add(identifier)` searched with the same Ordinal set semantics as the removed `oldNames.Contains \|\| string.Equals(…, Ordinal)`; `GetDependentProjectIds` moved unchanged | parity |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `VerifyFixKeepsOneReadonlyModifierOfAReadonlyStructProperty` | exactly one `readonly` | `readonly readonly` | `Verify` exact text | adequate |
| `VerifyNoFixIsOfferedWhenExcludedCodeNamesTheProperty` / `…DeclaresTheNewName` / `…Elif…` / `…Else…` / `…IfFalse…` | any excluded branch naming the old or new name withholds | fix registered | `VerifyNoCodeFixIsOffered` | adequate |
| `VerifyFixRenamesReferenceInActiveConditionalBranch` | the active branch is renamed | the fix withheld, or the reference left | `Verify` exact text | adequate (FC-003 boundary) |
| `VerifyNoFixIsOfferedFor…` (comment and directive positions, CRLF variants) | each guarded position withholds | fix registered | `VerifyNoCodeFixIsOffered` | adequate for the positions tested; positions (a) and (b) are untested (finding #2) |
| `VerifyNoFixIsOfferedForIntPropertiesWhenTargetingNetStandard20` | metadata decides `readonly` | fix offered for `int` on netstandard2.0 | `Verify(src, src)` with `ReferenceAssemblies.NetStandard.NetStandard20` | adequate |
| `VerifyFixIsOfferedOnlyForTheTypeParameterConstrainedToAReferenceType`, `VerifyFixAllSkipsThePropertiesWhoseFixIsWithheld` | the withheld properties stay reported under single and Fix All | the withheld property fixed, or its diagnostic lost | `Verify` with `MarkupMode.Allow` (06) | adequate |
| `VerifyFixReplacesNullableIntProperty` | `int?` keeps the fix (Q2 = C) | fix withheld | `Verify` exact text (net9.0 references) | adequate |

## thoenissen/Reihitsu#1287

### Scope
Full audit. Files: `PartialMemberUtilities.cs` (new), RH2002, RH2004, RH8306, RH2101 (`NestedTypeAnalyzerHelper`), RH7002, the two RH74 bases, and their tests.

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 4 | high | `src/Reihitsu.Analyzer/Core/PartialMemberUtilities.cs:89-91` (`analyzingPart`); `src/Reihitsu.Analyzer/Core/InterfaceImplementationUtilities.cs:50`; `MembersShouldBeGroupedByRegionsAnalyzerBase.cs:80-81` | FC-016, FC-002, FC-024 | – | same requirement | SHARED-DIAG-012 selects a part that no analysis ever reports from, so nothing is reported. **(a) RH2004, partial constructor whose implementing part is in generated code.** The driver does not analyze the defining part of a partial constructor (`05-implementer.md`), and `GeneratedCodeAnalysisFlags.None` skips the generated implementing part. So the analysis to which `GetReportedPart` delegates the constructor never runs. Input: `public partial class P { partial P(int x); }` plus `P.g.cs` with `partial P(int x) { }`. Expected per RH2004-001 and SHARED-DIAG-012: `partial {|RH2004:P|}(int x);`. Observed: no diagnostic. **(b) RH7409, partial method implementing an interface member, whose defining part is in generated code, both parts outside a matching region.** `InterfaceImplementationUtilities.GetImplementedInterfaceName` matches only the symbol that `FindImplementationForInterfaceMember` returns, which is the defining part. So `TryGetExpectedRegionName` fails for the implementing part, and its analysis `continue`s before the selection is consulted. The defining part is generated, so it is not analyzed. Input: `interface I { void M(); }`, `partial class C : I { #region Methods public partial void M() { } #endregion }` plus `C.g.cs` with `partial class C { #region Methods public partial void M(); #endregion }`. Expected: `M` in the user file reported, because every part judged alone lies outside region `I`, so RH7409-001 covers it and SHARED-DIAG-012 picks the implementing part. Observed: no diagnostic. RH7410, RH7411, and RH7412 fail the same way by trace (same utility). On `main`, both cells reported nothing either, but SHARED-DIAG-012 is new in this run. Cells that hold (observed): RH2004 property with the defining part generated, with the implementing part generated, and with both generated (tests); RH2004 constructor in two non-generated documents (probe); RH2002 with either part generated (tests); RH8306 with the defining part generated (test); RH7401 with the implementing part generated (test) and with the defining part generated (probe). By trace: RH2004 constructor with the defining part generated (the implementing part is analyzed and selects itself); RH7402–RH7408 with the defining part generated (`OverriddenX`/`IsRelevantNonOverrideMember` hold for the implementing symbol). | Give the selected part an analysis that reaches it. For example: decide partial members from an entry point that sees both parts (a symbol action, or the containing type's analysis) instead of predicting which part the driver visits. Or, for RH7409–RH7412, judge "comes from an interface" on the member (`PartialDefinitionPart ?? symbol`) rather than on the part symbol. Add tests for (a) and for (b) on each of RH7409–RH7412. |

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Which part's analysis reports (`GetReportedPart`: constructor → implementing part, else the selected part) | the two parts from `PartialDefinitionPart`/`PartialImplementationPart` | Analyzer-driver coverage (no action for a defining constructor part; none in generated trees) and each rule's per-part pre-filter (`ShouldSkip`, `TryGetExpectedRegionName`, `IsRelevantNonOverrideMember`, the summary check) | Disagree: the generated implementing constructor part is never analyzed, and RH7409–RH7412's pre-filter rejects every implementing part | finding #4 |
| `qualifies` for RH74 bases | `HasMatchingRegion(part, expectedRegionName, regions of the part's type declaration)`; RH7405–RH7408 `LiesInOverrideRegion`, with a part in another tree counting as qualifying (decision 13) | The rule's own loop conditions for the analyzed part | Agree for parts in the same tree; the other-tree shortcut is decision 13 and holds for every covered member (all parts reported) | ok |
| RH7002 "same type" (`GetTypeKey`) | namespace name parts by value (nested and qualified merged), identifier value text, type-parameter count | RH7002-004 "same name, same number of type parameters, same namespace" | Agree. Kind mismatch is CS0261, and differently named type parameters are CS0264; neither is a candidate | ok |
| RH2101 static (`IsStaticType`) | `INamedTypeSymbol.IsStatic`, fallback to modifiers | RH2101-002 "any part carries `static`" | Agree, also across documents (test) | ok |
| Generated part (`IsGeneratedCode`) | options → file-name patterns → `<auto-generated>`/`<autogenerated>` header | the driver's rule (SHARED-DIAG-001) | Same rule. A tree marked generated only by `[GeneratedCode]` on a symbol is not modeled (see Hints) | ok (decision 14) |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Part selection for a partial member | `rg -n "PartialDefinitionPart\|PartialImplementationPart" src --glob '!*Test*'`: `PartialMemberUtilities` only; callers RH2002, RH2004, RH8306, the two RH74 bases | one owner |
| What is generated code | `PartialMemberUtilities.IsGeneratedCode`, CLI `GeneratedFileUtilities`, formatter `IsAutoGeneratedSource` | three owners; recorded as decision 14 with follow-up candidate 2, not repeated here |
| "Same partial type" by syntax | `rg -n "TypeParameterList\??\.Parameters\.Count" src/Reihitsu.Analyzer src/Reihitsu.Core`: RH7002 only | one owner |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH2004 `VerifyPartial{Property,Indexer,Event,Constructor}IsReportedOnceOnItsDefiningPartWithoutFix` | one diagnostic on the defining part, no fix | two diagnostics, wrong part, a fix | `VerifyNoCodeFixIsOffered` (exact diagnostics plus registration check) | adequate |
| RH2004 `VerifyPartialPropertyInTwoDocumentsIsReportedOnTheDefiningPartIn{First,Second}Document` | order of documents irrelevant | diagnostic follows document order | `Verify`, two documents, both orders | adequate |
| RH2004/RH2002/RH8306/RH7401 generated-part tests | generated parts never chosen | diagnostic in generated code, or none | `Verify` with `*.g.cs` | adequate for the kinds tested; constructor with a generated implementing part and RH7409–RH7412 with a generated defining part are missing (finding #4) |
| RH7002 `FurtherPartInNestedAndQualifiedNamespaceOfTheSameNameIsNotReported` etc. | key equality by namespace value | further part reported | `Verify` | adequate |
| RH74nn `VerifyPartial…IsReportedOnceOnItsDefiningPart` (12 rules) | once, defining part | per-part report | `Verify` | adequate |

## thoenissen/Reihitsu#1288

### Scope
Full audit. File: `RH2004AccessModifierMustBeDeclaredAnalyzer.cs` (`ShouldSkip`) and three tests.

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|

_N/A — no finding._ The two new `switch` arms cover the remaining two of the six member kinds that can carry an `ExplicitInterfaceSpecifier`: operator (including unary and `checked`) and conversion (implicit and explicit). Tests cover each with block and expression bodies.

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| RH2004-003 exemption | `ExplicitInterfaceSpecifier` on method, property, indexer, event, operator, conversion | RH2004-003 text (six kinds) | Agree | ok |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Explicit-implementation exemption | `rg -n ExplicitInterfaceSpecifier src/Reihitsu.Analyzer/Rules/Design/RH2004*`: one `switch` | one owner |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `VerifyExplicitInterfaceOperatorAndConversionImplementationsAreNotReported`, `…UnaryAndChecked…`, `…ImplicitConversion…` | not reported | any RH2004 diagnostic | `Verify` (exact diagnostics) | adequate |

## Whole run

### Visible output is decided
Every added or changed expected value in a test of a visible surface is demanded by a requirement or covered by a maintainer answer:

- RH2005-003 and RH2004-006 (placement): the attribute, comment, and directive examples (decision 4, unchanged intent of the requirement).
- RH2005-004: the reference and `cref`/attribute examples (Q3 = B). Excluded code inside or outside the type follows SHARED-FIX-003.
- RH2001-004, RH2001-007, RH2001-008: the `int?`, enum, `readonly`-struct, and type-parameter cases (Q2 = C, Q4 = A).
- SHARED-DIAG-012 (Q1 = A): RH2002, RH2004, RH8306, and RH74nn locations. RH7002-004, RH2101-002, and RH2004-003 (round 7b A).

The removed characterization tests are each replaced by the specified behavior (`04-test-implementer.md` table). The moved tests `VerifyFixKeepsUppercaseRunOfLeadingAcronymInFieldName`, `…RenamesReferencesCarriageReturnLineFeed`, and `VerifyFixAddsDefaultAccessibilityOfEachContextCarriageReturnLineFeed` are unchanged in content. No rule page changed.

### Comments and conventions
- No `TODO`, no commented-out code, and no tracker number in any added `src/` line.
- Changed summaries describe the new code (FC-031). One exception is incomplete, not wrong: `PartialMemberUtilities.GetReportedPart`'s remark about the constructor does not say that a generated implementing part leaves the constructor unreported (finding #4).
- The BOM added to `MembersShouldBeGroupedByRegionsAnalyzerBase.cs` and to RH74 test files matches `.editorconfig` (`charset = utf-8-bom`).

### Scope
- The diff stays within the four issues and decisions 5 and 12. `scripts/lane-check.sh bug-fix`: confirmed.

### Failure classes dismissed
- FC-005: covered. The new tests separate comment and directive for RH2001, RH2004, and RH2005. The one gap is a position, not a kind (#1, #2).
- FC-006: the rewrites keep meaning. RH2004 fix withheld for partial; RH2005 withheld for outside references; RH2001 value-type cases withheld.
- FC-007 and FC-009: the fixes converge in one pass (iterative `Verify`). No registered action is a no-op: RH2004 and RH2001 re-check in the apply step, and RH2005 changes the modifier always.
- FC-008: Fix All tests exist for RH2001 and RH2005 with withheld members; `BatchFixer` per diagnostic.
- FC-010, FC-011, FC-012, FC-028, FC-029: no formatter phase changed. RH2001 still formats only its field through `FormatNodeInDocumentAsync`, and RH2004/RH2005 do not format.
- FC-013 and FC-014: no layout rule and no new rule. The RH74 region rules are order rules the formatter does not judge.
- FC-017: CRLF variants exist for every line-based fix shape tested.
- FC-018 and FC-036: the RH2001 rename path is unchanged (Renamer). `nameof` and reflection are as on `main`.
- FC-019, FC-020, FC-021: walked. Failing members are #1 and #2.
- FC-022: no literal edited.
- FC-023: RH7002 compares `ValueText` (`@class` equals `class`). RH2001's name derivation is unchanged.
- FC-025: RH2004-003's operator kinds tested with both body forms; RH2001 rejects expression-bodied properties as before.
- FC-026: no token re-resolution after edits. RH2001 tracks its node by annotation.
- FC-027: the multi-line accessor list is tested. The nested-type and partial-part bodies for RH2005 are tested.
- FC-030: the RH2001 fix formats only the field node.
- FC-032: no formatter change, so no stale project result.
- FC-033: reproductions use the code-fix path with LF and CRLF where layout matters, and two-document and two-project setups where scope matters.
- FC-034: the RH2005 reproduction uses the attribute-list shape the skeptic identified, not the simplified `static public` cousin.
- FC-035: expected values come from the spec examples (`04-test-implementer.md`), not from patched code.
- FC-037: no comment-only claim.

### Architecture findings
- **Part selection predicts the analyzer driver (finding #4).**
  - Components: `PartialMemberUtilities.GetReportedPart` and the analyzer driver (which declarations get a node action, and which trees are skipped as generated), plus each rule's per-part pre-filter that runs before the selection is consulted (RH7409–RH7412 `TryGetExpectedRegionName` through `InterfaceImplementationUtilities`).
  - Shared decision: "the analysis of which part emits the member's one diagnostic". `GetReportedPart` hard-codes a prediction of the driver (`declaration is ConstructorDeclarationSyntax ? implementingPart : reportedPart`) and assumes that every selected part's analysis reaches the selection.
  - Inputs that show the disagreement: the constructor with a generated implementing part, and RH7409 with a generated defining part (finding #4).
  - The defect is introduced by this run's new mechanism, so it reaches the run.
- **The RH2001 guard restates what the builder drops (finding #2).**
  - Components: `HasCommentOrDirectiveInsideDeclaration` and `TryCreateFieldDeclaration`.
  - The guard lists the positions whose trivia the field loses, separately from the builder that loses them. The two lists already differ on two spans (identifier leading, `;` leading).
  - The rule is stated in two places in one file, so this is local, but it is the structural cause of the missed members.

### Hints
- **RH2001 `readonly` insertion with a modifier reorder.** Pre-existing on `main` and outside the four issues. `static private int Value { get; }` becomes `private     static readonly int _value;` (five spaces). With `static /* a */ private`, the comment ends up behind `static` on the other side of `private`. The old first modifier's indentation travels to the middle of the list, and `FormatNodeInDocumentAsync` leaves the gap. Follow-up candidate: the `OrderModifiersForRh7105` path in `TryCreateFieldDeclaration` has no `ReorderingWouldDropCommentOrDirective`-style guard and no trivia normalization.
- **RH2004 partial constructor in two documents.** The diagnostic for the defining part's document is reported from the analysis of the implementing part's document. Batch and build analysis show it (probe passed). Whether the IDE's open-document analysis shows such a non-local diagnostic without full-solution analysis was not verified.
- **RH2005 excluded-code filter.** The filter judges a disabled-text block by its start position. An `#if` block that starts inside the type's body and ends outside it, holding another type that reads the field, is not counted. This is a contrived shape, not constructed.
- **`[GeneratedCode]`-only generated parts.** `PartialMemberUtilities.IsGeneratedCode` models the tree-level rule only. A part that the driver treats as generated because of a `[GeneratedCode]` attribute on its symbol is not excluded from the selection. Not constructed.

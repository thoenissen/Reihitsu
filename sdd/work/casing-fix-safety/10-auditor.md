Role: auditor
Commit: 5a1e7022b4dd963a5c668b85eb404d49a452b16c
Verdict: findings

The prompt carried no intended fix, suspected cause, or hoped-for verdict; it named the head, the base, `00-lead.md`, and handovers 01–09.

## Scope

Full audit of head `5a1e7022b4dd963a5c668b85eb404d49a452b16c` against base `origin/main` = `b673099172ca665ef61a85f760ffe436443bd72f`. State verified: the checkout is the head, `git ls-remote origin refs/heads/main` gives the base SHA, the base is an ancestor of the head, and the diff (`git diff origin/main...HEAD`, 48 files outside `sdd/work/`) is what handovers 04–08 describe. `scripts/lane-check.sh bug-fix`: confirmed. The cleaner's Release build and the roles' focused test results are trusted; no test was re-run. Constructed inputs were run with `scripts/apply-fix.sh RH4105` (LF and CRLF agreed) and, for run-time effects, as file-based programs (`dotnet run`) before and after the rename; fixtures lived in the scratchpad.

## thoenissen/Reihitsu#1597

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | high | `src/Reihitsu.Analyzer.CodeFixes/Base/CasingCodeFixProviderBase{T}.cs:822-874` (`IsNameofArgument`, `HasCallerArgumentExpressionNaming`); `sdd/specs/shared/casing.md` SHARED-CASING-012 (the `nameof` sentence and its *Why*) | FC-036, FC-001, FC-016 | – | same requirement | The `CallerArgumentExpression` guard still lets three members of #1597's case-3 class through; each rename changes which parameter the attribute names and so what the parameter receives at run time (SHARED-FIX-003 *Run time*, including the sentence this run added). Single fix and Fix All both use `RenameWithoutConflictAsync`. **(a) `nameof` of another symbol that has the old name**: `public static class Names { public const int Condition = 0; }` and `Check(bool Condition, [CallerArgumentExpression(nameof(Names.Condition))] string expression = null)`. The fix is offered and writes `Check(bool condition, …nameof(Names.Condition)…)`. `Guard.Check(1 > 0)` returns `1 > 0` before and `<null>` after. **(b) `nameof` of another symbol that has the new name**: `private const int condition = 0;` and `Check(bool Condition, [CallerArgumentExpression(nameof(condition))] …)`. The fix is offered, and the renamer rewrites the attribute to `nameof(Guard2.condition)`, which keeps the value `"condition"`; that value now names the renamed parameter. The call returns `<null>` before and `1 > 0` after. **(c) C# 14 extension block receiver**: `extension(bool Condition) { public string Check([CallerArgumentExpression("Condition")] string expression = null) => expression; }`. The fix is offered and renames the receiver to `condition`; `(1 > 0).Check()` returns `1 > 0` before and `<null>` after. The guard inspects only the parameter lists whose `ContainingSymbol` is a method or a property, and the receiver's containing symbol is the extension type. Members that hold: a literal or a `const` naming the old or the new name (method, constructor, class primary constructor, local function, delegate, indexer, partial method, record), and `nameof` of the cascade parameter itself. Cause: the exemption is keyed on the syntax `nameof(...)`, not on whether the `nameof` refers to a symbol of the cascade. The inspected lists also omit the member lists of an extension block whose receiver is in the cascade. The spec's *Why* ("a `nameof` argument is a reference, which the rename changes together with the parameter") holds only for a `nameof` of a cascade symbol. | Refuse this class. Exempt a `nameof` argument only when it is an unqualified identifier equal to an old name of the cascade, which binds to the parameter, and withhold every other `nameof` whose value is an old or the new name. Withhold a rename whose cascade holds an extension block receiver parameter when a member of that block carries a `CallerArgumentExpression` with such a value. Narrow the SHARED-CASING-012 `nameof` sentence and its *Why* to match. Add one test each for (a), (b), and (c). |

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Rename withheld for a compiler-looked-up method name (SHARED-CASING-011) | The declared symbol, when it is an `IMethodSymbol` and not a local function; the derived name against the 21-name list | SHARED-CASING-011's list, and the cascade (overrides and implementations share the name) | Agree. Non-method members cannot serve these lookups: a delegate field named `Add` gives CS0118 in a collection initializer (constructed). A method in a C# 14 extension block is an `IMethodSymbol` and is covered | holds |
| Rename withheld for a `CallerArgumentExpression` naming an old or the new name (SHARED-CASING-012) | The parameter lists of methods and properties that declare a cascade parameter; constant value; the argument syntax is `nameof(...)` | SHARED-FIX-003 *Run time*: whether the rename changes which parameter the attribute names, or whether it names one | Disagree: a syntactic `nameof` stands in for "is renamed with the parameter", and the extension-block member lists are missing | finding #1 |
| Excluded-code and compile-error checks | Unchanged | — | Not changed by the diff | n/a |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH4103 `NoFixIsOfferedWhenTheRenameWouldChangeTheMethodA{Foreach,QueryClause,Deconstruction,CollectionInitializer}Calls` and the 23-row `NoFixIsOfferedWhenTheDerivedNameIsANameTheCompilerLooksUp` | No action for a method whose derived name is in the list | Any registered action | `Verify(src, src)` | adequate |
| RH4103 `FixAllSkips…` (4), RH4105/RH4120 `FixAllSkipsTheParameterNamedByACallerArgumentExpression` | Fix All skips the withheld names and renames the other diagnostic in the document | Wrong text, or a missing or extra fixed-state diagnostic (`MarkupMode.Allow`, decision 8) | `Verify(src, fixed)`, one Fix All iteration | adequate |
| RH4103 `FixIsOfferedWhenTheDerivedNameIsNotANameTheCompilerLooksUp`, `FixIsOfferedForAMethodWhoseDerivedNameIsOutsideTheList`, RH4104 local-function test | The other side of the list boundary (FC-003) | No fix, or another text | `Verify(src, fixed)` | adequate |
| RH4105/RH4120 CAE tests (literal old, literal new, const, ctor, primary ctor, local function, delegate, partial; `nameof` and other-parameter controls) | Withheld for the listed forms; offered for `nameof` of the parameter | Any action, or no action for a control | `Verify` | adequate for the members built; members (a)–(c) of finding #1 are missing |
| RH4103 `FixRenamesAnExtensionMethodThatNoCallOfAnotherDocumentSees` (decision 9) | SHARED-CASING-009 rename with a name outside the list; the other document unchanged | Another text in either document | `Verify` + second document | adequate |
| RH4114 decision-11 pair | A cascade from a property to a positional parameter named in a CAE is withheld; offered without the attribute | Action offered, or not offered on the control | `Verify` | adequate |

## thoenissen/Reihitsu#1598

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 2 | low | `src/Reihitsu.Analyzer.Test/Naming/RH4109PublicFieldCasingAnalyzerTests.cs`, `ApplyFixInMultiTargetSolutionAsync`: `Assert.IsNotEmpty(initiatingDocument.GetLinkedDocumentIds().Concat([initiatingDocumentId]), …)` | FC-017, FC-033 | – | PR-introduced | The assertion can never fail, because it concatenates the initiating document's own ID. It does not check that the copies of `A.cs` are linked documents, which is the FC-033 precondition the skeptic required. The verdicts do not rest on it: the candidate 2 and 3 tests observed the rename in both copies of `A.cs`, which only linked documents produce. So this is a dead check, not a false result. | Assert `GetLinkedDocumentIds()` is non-empty where the shared file is in two projects, or drop the line. |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| The five RH4109 multi-target tests | Withheld, or renamed everywhere with no added compile error in any project | Old name left in any copy, or a compile error in any project | `ApplyFixInMultiTargetSolutionAsync`, `AssertWithheldOrRenamedEverywhere` | Adequate as characterization; the linkage precondition assertion is vacuous (finding #2) |

## thoenissen/Reihitsu#1016

### Findings
_None._

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| The fix is withheld when the rule still reports the derived name (SHARED-CASING-008) | `TryGetFixedIdentifier` on the derived name of the reported declaration, for single fix and in Fix All re-resolution | `CasingAnalyzerBase.CheckCasing`, now the same `IsValidName` | Same predicate, one owner. Every casing provider passes its own rule's analyzer (26 checked). A cascade crosses rules only where both styles are PascalCase (interface property to positional parameter, RH4114 to RH4120), so the declared rule's predicate decides for the whole cascade | holds |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH4106/RH4002/RH4105/RH4005 `NoFixIsOffered…StillReports…` rows | No fix for every start kind (digit, caseless, no mapping), leading and after `_` | Any action | `Verify(src, src)` | adequate |
| Controls `ß_x`, `_ŉx`, `_1` | Offered where the rule accepts the derived name (other side) | No fix, or another name | `Verify(src, fixed)` | adequate |
| `FixAllSkips…` (RH4106, RH4002, RH4105, RH4005) | Fix All skips and leaves the diagnostics | Text or fixed-state diagnostics differ | `Verify`, `MarkupMode.Allow` | adequate |

## thoenissen/Reihitsu#1015

### Findings
_None._

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Which nodes the RH4102 provider fixes | `CasingCodeFixProviderBase<SyntaxNode>`: `FindNode(span)` is any node; `GetIdentifier` gives a name for `VariableDeclaratorSyntax` and `EventDeclarationSyntax`, otherwise `""`, which `TryGetFixedIdentifier` rejects | RH4102 analyzer locations (declarator identifier, event identifier) and `GetDeclaredSymbol` arms | Agree; same pattern as RH4115. No other diagnostic ID has two providers | holds |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `FixAllRenames…Together` (2), `FixAllSkips…WouldCollide` | Fix All from every diagnostic, every provider, and all three scopes gives the exact text and compiles; it equals the sequential single fixes where SHARED-FIX-002 asks for it | Any text difference, a compile error, or no provider offering a fix | `VerifyFixAllFromEveryDiagnosticAsync` (exact text) | adequate |

## thoenissen/Reihitsu#1084

### Findings
_None._

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH4005 `_`, `__`, `___` rows; the spec example; Fix All; `_reader` control | No fix where no usable name remains; offered for `_reader` → `IReader` | Any action; no action on the control | `Verify` | adequate |

## Whole run

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Rule acceptance of a name | `CasingAnalyzerBase.IsValidName` (`_casingValidation`); consumers `CheckCasing` and `CasingCodeFixProviderBase<T>.TryGetFixedIdentifier` (`rg -n 'IsValidName\(\|_casingValidation' src`) | one owner |
| Compiler-looked-up method names | Only `IsCompilerLookupMethodRename` (`rg '"GetEnumerator"\|"Deconstruct"\|"GetAwaiter"' src -g '!*Test*'`) | one owner |
| `CallerArgumentExpression` handling | Only `HasCallerArgumentExpressionNaming` (`rg CallerArgumentExpression src -g '!*Test*'`) | one owner |
| `nameof` syntax detection | `IsNameofArgument` (code fix); `RH3103UseShorthandForNullableTypesAnalyzer` (analyzer-private, a different decision) | no shared policy; not a duplicate |
| RH4102 fix | `RH4102EventNameCasingCodeFixProvider` only; the custom provider is deleted, and the spec owner rows are updated | one owner |

### Checks not tied to one issue
- **Specs (1).** Every changed requirement holds in the code except SHARED-FIX-003 *Run time* for the members of finding #1. #1016's RH4107 (named in the issue) uses the same base check as RH4106; it has no test of its own, which is acceptable because the check is in one place.
- **Visible output (10).** Every changed expected value is either a withheld fix for a rare class (decisions 2, 3, 6, 9, 11; SHARED-FIX-001/003 unchanged) or `main`'s output, which unchanged requirements demand (`ß_x` → `_ßX`, `_ŉx` → `ŉx`, `_1` → `T1`, the RH4114 cascade rename, `first` → `First`, the RH4102 Fix All demanded by SHARED-FIX-002). No rule page changed. Nothing is undecided.
- **Cost of special cases (11).** Both new guards are cheap refusals (a fixed name list, and an attribute scan). The `IsValidName` check replaces the per-converter restatement. No declared unsupported shape is involved, because the fixes do not format.
- **Comments (8).** The summaries of `RenameWithoutConflictAsync`, `TryGetFixedIdentifier`, `PrepareRenameAsync`, the base constructor, and the RH4102 provider describe the new code. The remark of `HasCallerArgumentExpressionNaming` and SHARED-CASING-012's *Why* overstate the `nameof` case (finding #1). There is no `TODO` and no tracker number.
- **Scope (9).** Nothing is outside the run. The 26 one-line constructor changes are the mechanical part of decision 4.

### Failure classes dismissed
- FC-002, FC-003: no qualifier is lost. Both sides of each new boundary are tested: list in and out, local function, `nameof` of the parameter, another parameter, accepted and rejected derived names.
- FC-004, FC-005, FC-019, FC-020, FC-021: the fixes rename through the Renamer and move no trivia. The #1598 `#if` shape is covered by characterization tests.
- FC-006: overload and extension rebinding is the SHARED-CASING-011 list. Non-method members cannot serve the lookups (a delegate field `Add` gives CS0118, constructed).
- FC-007: closed by the `IsValidName` check. The derived name is accepted by the rule that reported it.
- FC-008: Fix All recomputes `TryGetFixedIdentifier` per candidate after earlier renames. The RH4102 merge removes the per-type filter split.
- FC-009: the RH4102 provider registers nothing when `GetIdentifier` gives `""`. No action is registered whose rename is withheld.
- FC-010 to FC-014, FC-022, FC-025 to FC-030, FC-038 to FC-043: no formatter phase, layout, literal rewrite, or unsupported shape is involved.
- FC-015: one owner per policy (table above).
- FC-018: the Renamer is unchanged.
- FC-023: `_`, digit, and Unicode names are tested. A verbatim `@select` takes `ValueText` `select` and reaches the same list check.
- FC-024: partial methods, explicit interface implementations, and records are tested.
- FC-031: covered under *Comments*.
- FC-032: no result predates a compiled change. The cleaner built Release after the last production edit.
- FC-034: the #1598 tests use the issue's own linked-document shape. #1597 rows 1, 2, 7, and 8 are the issue's cases.
- FC-035: the tests were written at `1c84020`, before the production change at `87ce0ad`.
- FC-037: no comment-only change is claimed.

### Architecture findings
_None._ Finding #1 is a guard keyed on syntax instead of on the cascade it protects, inside the one owner of the decision. It is not two components sharing the decision.

### Hints
- SHARED-CASING-011 withholds the fix for every lone method whose derived name is in the list, which decision 6 accepted as a rare class. In code written in Java style (camelCase methods throughout), `add`, `join`, `where`, `select`, `cast`, and `dispose` are ordinary method names, so Fix All on such a codebase leaves them all reported without a fix. If the lead doubts that this class is rare (AGENTS.md, *Code-fix coverage*, last bullet), it is a maintainer question. Otherwise no action is needed.
- In fixture (b) of finding #1, the renamer qualified `nameof(condition)` to `nameof(Guard2.condition)` to keep the binding. That is another instance of the call-site rewrite already recorded as a follow-up candidate in `00-lead.md`.
- Commands: `scripts/apply-fix.sh RH4105 <scratch>/f4105` (4 fixtures × LF/CRLF: (a), (b), and (c) fixed; the indexer variant had no fix offered); `dotnet run` of each before/after program: (a) `1 > 0` → `<null>`, (b) `<null>` → `1 > 0`, (c) `1 > 0` → `<null>`.

Role: auditor
Commit: 257eea788cdad88f7022155884a53f11aa76ddd8
Verdict: #1680: findings; #1742: pass

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

How this was established: static reading of the diff and its counterparts, plus a throwaway probe outside the repository (a console app in the session scratchpad referencing `src/Reihitsu.Analyzer`, Microsoft.CodeAnalysis 5.9.0, `LanguageVersion.Preview`, `DocumentationMode.Diagnose`) that runs every `[DiagnosticAnalyzer]` of `Reihitsu.Analyzer` with every ID forced to warning, or a chosen subset, and optionally logs every syntax-node callback on region, end-region, documentation-comment, and XML-element nodes with its `ContainingSymbol`. A duplicate is the same ID on the same span twice; every duplicate was compared with the same text in an ordinary class (control). No test was re-run; the roles' focused results and the cleaner's build are trusted.

## thoenissen/Reihitsu#1680

### Scope

Head `257eea788cdad88f7022155884a53f11aa76ddd8` is checked out. `origin/main` is `846aca462c94c0d85bc19a1d5ad9d78605590c29` (`git ls-remote` agrees) and is an ancestor of head. The diff matches 04-implementer.md (with its amendment) and 02-spec-author.md: the guard in `src/Reihitsu.Analyzer/Base/AnalysisContextExtensions.cs` (renamed from `DocumentationModeAnalysisContextExtensions.cs`), ten registrations switched over (RH8204, RH8304, RH8305, RH8307, RH8308, RH8309, RH7302, RH7310, `RegionDescriptionAnalyzerBase` for RH7306/RH7307), `IsPrimaryConstructorRun` deleted, code-owner renames in two specs, and tests only otherwise. `scripts/lane-check.sh bug-fix --base 846aca4`: `LANE CHECK: confirmed — claimed bug-fix, diff needs bug-fix`. Full audit.

### Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | medium | `src/Reihitsu.Analyzer/Base/AnalysisContextExtensions.cs` (`WrapWithRepeatedRunCheck` is applied only to registrations on structured-trivia kinds); registrations of `RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer` (`ParenthesizedExpression`), `RH3005UseReadableConditionsAnalyzer` (`EqualsExpression`, `NotEqualsExpression`), `RH3006ConditionalExpressionsMustDeclarePrecedenceAnalyzer` (`LogicalOrExpression`) | FC-016, FC-001 | – | same requirement | The run's mechanism is "every node inside structured trivia attached to the header or closing-brace tokens of a type with a parameter list is handed to syntax-node actions twice". The guard is keyed on the registered kind, so it covers only registrations on trivia kinds; the expression nodes of a `#if`/`#elif` condition are also inside structured trivia and are still handed twice. Counterexample (probe, control class beside it): `public class Service(int value) { public int Value => value; #if A \|\| B && C / #endif / #if true == A / #endif / #if ((A)) / #elif B \|\| C && A / #endif }` (each directive on its own line, in front of `}`) gives RH3006 x2 on `B && C` (both `#if` and `#elif`), RH3005 x2 on `==`, RH3002 x2 on the inner `(A)`; the same text in `public class Plain { ... }` gives each once. The same doubling holds for the conditions in front of the type's first token (RH3006 and RH3005 x2 there, probe `c1`). Members that hold (0 reports in both runs, so no duplicate): RH3001 (`!A`), RH5302 (`A&&B`) on directive conditions; RH1001, RH3103, RH6011, RH6012, RH6015 on crefs (`Nullable{Int32}`, `M(int ?)`, `List{ T }`, `Dictionary{Int32 ,String}`, an alias to a `Dictionary<Key,int>`). No other registered kind (rg over every `Register*SyntaxNode*Action` kind list) occurs inside structured trivia. Pre-existing: these three analyzers are untouched by the diff. No RH3002/RH3005/RH3006 requirement states "once" explicitly, and the issue names documentation analyzers, so whether this joins the run is the lead's call. | Either apply the same `IsRepeatedRun` check to these three registrations (the check is position-based and already correct for any node; it is a no-op outside a primary-constructor run, and a `IsPartOfStructuredTrivia()` precheck keeps it off the hot path), or file it as an `agent-filed` follow-up naming the three analyzers and the counterexample. |
| 2 | medium | Tests for `AnalysisContextExtensions.IsRepeatedRun` (no test class covers either edge below) | FC-003, FC-017 | – | PR-introduced | Two boundaries of the new guard have no test, so a regression in either keeps every test green: (a) the `PrimaryConstructorBaseTypeSyntax.ArgumentList` exemption — deleting the second `IsInSpan` term turns `public class C2(int value) : B` / `#region lower b` / `(value)` (directive in front of `(`) and `public class C4(int value) : B(value` / `#region lower d` / `)` from one RH7302 diagnostic into none; (b) `FullSpan` rather than `Span` of the parameter list — changing it drops `public class C1` / `#region lower a` / `(int value)` (directive in front of the list's `(`, visited only in the constructor run) from one to none. The probe shows all three reported once on head, so the code is right; only the tests are missing. The implementer named (a) as untested (04-implementer.md). | Add one RH7302 test per edge: a `#region` inside the base-type argument list, and one in front of the `(` of a primary constructor's parameter list, each reported once. |

### Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| A structured-trivia callback repeats one the type's own run already made (`IsRepeatedRun`) | `ContainingSymbol` is a constructor whose declaring syntax is a `TypeDeclarationSyntax` in the node's tree; node outside `ParameterList.FullSpan` and outside the primary-constructor base type's `ArgumentList.FullSpan` | The analyzer driver's visit set for a primary constructor (probed): header and closing-brace trivia visited by both runs; parameter list including the leading trivia of its `(`, and base argument list including the leading trivia of its `(`, by the constructor run only; trivia in front of the base type name, inside `[method: ...]` attribute lists and in front of them, by the type run only (dropping the constructor run never loses those) | Agrees on every probed position (C1–C6, R1–R4, `[method:]` lists, nested and partial records) | holds |
| Which registrations get the guard | Registrations on `RegionDirectiveTrivia`, `EndRegionDirectiveTrivia`, `SingleLineDocumentationCommentTrivia`, `XmlElement`, `XmlEmptyElement` | Every node the driver hands twice: every node inside structured trivia, including directive-condition expressions | Narrower: directive-condition expressions are handed twice and are not guarded | finding #1 |
| Old `IsPrimaryConstructorRun` (deleted) | Any constructor run of a type declaration | — | Replaced; its over-wide drop of constructor-only nodes is fixed (RH8305/RH8307 parameter-list tests) | closed |

### Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Skip the repeated primary-constructor run | `AnalysisContextExtensions.IsRepeatedRun` only (`rg -n "IsPrimaryConstructorRun\|MethodKind\.Constructor\|PrimaryConstructor" src/Reihitsu.Analyzer src/Reihitsu.Core`: the other hits are `DocumentationAnalysisUtilities.cs:375` base-constructor matching, `SingleLetterIdentifierUtilities.IsRecordPrimaryConstructorParameter`, `RewriteBindingUtilities`, RH2102/RH2103/RH4120 names — different decisions) | one owner |
| Structured-trivia registrations | `rg -U` over every `Register*Action` call with a trivia or `Xml*` kind: exactly the ten registrations listed in Scope, all through `RegisterStructuredTriviaAction[WithDocumentationModeCheck]` | one owner |
| Documentation-mode check | `AnalysisContextExtensions.WrapWithDocumentationModeCheck` (unchanged, renamed class) | one owner |

### Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH8204/RH8304/RH8308/RH8309 `…OfPositionalRecord…`, `…OfClassWithPrimaryConstructor…`, `…Nested…`, `…DelimitedComment…RecordStruct…` | one diagnostic on the element's span | a second diagnostic on the same span (count mismatch) | `Verify` with location markup and one expected diagnostic | adequate |
| RH7302/RH7306/RH7307/RH7310 region tests (closing brace, before type, after attribute list, nested) | one diagnostic per directive | count 2 | `Verify`, markup | adequate |
| RH8308, RH7302 `…IsRemoved`/`…IsFixed` | the fix applies in one pass on the primary-constructor shape | duplicate in test state, or fixed text differs | batch `Verify(source, fixed, …)` | adequate |
| RH8305/RH8307 `…InsideParameterListOfPositionalRecord…` and method siblings | the constructor-only node is reported once, like in a method's parameter list | 0 or 2 diagnostics | `Verify`, markup | adequate (were 0 on main) |
| RH8308/RH7302 `…InsideParameterList…`, RH8308 `…InFrontOfAttributeList…` | constructor-only / type-only nodes keep one report | 0 diagnostics | `Verify` | adequate; base-argument-list and `(`-leading-trivia edges missing (finding #2) |

### Visible output is decided

Every new expected value demands one diagnostic per element under unchanged requirements (RH8204-002, RH8304-003, RH8308-003, RH8309-003, RH7302-003, RH7310-003, SHARED-REGDESC-002). The one change from `main` that adds a diagnostic — RH8305/RH8307 on a documentation comment inside a positional record's parameter list, 0 on `main`, 1 now — is demanded by RH8305-003/RH8307-003 ("reported once, like any other") together with the unchanged behavior for the same comment in a method's parameter list (sibling tests); it is a CS1587 shape. No rule page changed. Nothing undecided.

### Failure classes dismissed

- FC-002 — the requirement's qualifier ("visited by both runs") is the explicit span condition; probed on both sides.
- FC-004, FC-019, FC-020, FC-021 — no token, gap, or trivia is edited; analyzers only stop a second report. Directives are the subject of the region tests.
- FC-005 — directives are covered by the same guard as comments (region rules); directive-condition expressions are finding #1.
- FC-006, FC-022, FC-023, FC-025, FC-026, FC-027, FC-029, FC-036 — no rewrite in the diff.
- FC-007, FC-008, FC-009, FC-018, FC-030 — code fixes unchanged; the skeptic's `apply-fix.sh` runs and the two batch-verifier tests cover the reproduced shape.
- FC-010–FC-014, FC-028, FC-038–FC-043 — formatter not involved; no layout rule's verdict changes.
- FC-024 — partial type with the parameter list on one part: the other part's trivia is visited once with the type symbol (probe R4), and the constructor's declaring reference names only the part with the list.
- FC-015, FC-016 — see Policy ownership and finding #1.
- FC-031 — the class summary, both registration summaries, and `IsRepeatedRun`'s summary describe the new code; RH8305/RH8307 inline comments no longer mention the guard (see Hints for one imprecision).
- FC-032 — no project result is reused across a compiled change.
- FC-033 — every test uses the analyzer verifier, not a build.
- FC-034 — the tests use the issue's shapes (positional record, primary-constructor class).
- FC-035 — expected values come from the requirements, not a patched copy.
- FC-037 — no text-only claim.

### Cost of special cases

The parameter-list and base-argument-list exemptions keep diagnostics `main` reports today (RH7302, RH8308 inside a parameter list); dropping them would remove a visible output, so they are not a rare-class special case to refuse. Both are cheap syntactic span checks. No unsupported shape is involved.

### Architecture findings

_None_ new. Finding #1 is the same structural fact the skeptic reported (01-skeptic.md, *Whole run*): the driver's double visit is a property of node position, while the guard is attached per registration kind, so every registration that can see a node in structured trivia needs to opt in.

### Hints

- `IsRepeatedRun`'s summary says the compiler runs actions for the type's structured trivia twice "except for the trivia inside the parameter list and inside the argument list of the primary constructor base type". The probe shows a third group the summary omits: trivia in front of the base type's name and inside or in front of `[method: …]` attribute lists is visited by the type run only. The guard is correct for it (only constructor-run callbacks are dropped); the summary slightly overstates. Low, comment only.

## thoenissen/Reihitsu#1742

### Scope

As above. No production change for this issue; SHARED-DIAG-013 added to `sdd/specs/shared/diagnostics.md` and the ID ledger, five characterization tests.

### Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| _None._ | | | | | | | |

Checked: SHARED-DIAG-013 matches maintainer answer 1 (B, with the once-on-the-implementing-part clarification, `00-lead.md`); its four examples are tested verbatim (RH4122 x2, RH7302, RH2004) plus the generated-implementing-part case; its exception paragraph (symbol/containing-type rules, RH7105/RH7106, whole-file rules, other partial members) agrees with RH2004's test and the existing RH7105/RH7106 tests. The new #1680 guard cannot touch a partial constructor (its declaring syntax is a `ConstructorDeclarationSyntax`, not a type declaration).

### Guard scope

_N/A — no predicate added or changed for this issue._

### Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Reaching the defining part of a partial constructor | `DeclarationModifierUtilities.GetDeclarationsJudgedWith` (RH7105/RH7106), `PartialMemberUtilities` (RH2004) — unchanged; SHARED-DIAG-013 records that other syntax-node rules do not reach it | unchanged by the diff; recorded as a decision (maintainer answer 1B) |

### Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH4122 `PartialConstructorParameterNamedAlikeOnBothParts…` | one diagnostic, on the implementing part | a diagnostic on the defining part, or none | `Verify`, inline markup | adequate |
| RH4122 `…NamedOnlyOnTheDefiningPartIsNotReported`, `…WithGeneratedImplementingPart…` | no diagnostic on the defining part | any RH4122 diagnostic | `Verify` (second source `/0/C.g.cs` for the generated case) | adequate |
| RH7302 `…InFrontOfDefiningPartOfPartialConstructorIsNotReported` | no diagnostic | any RH7302 diagnostic | `Verify` | adequate |
| RH2004 `VerifySpecificationExamplePartialConstructorIsReportedOnTheDefiningPart` | one diagnostic on the defining part | none, or on the implementing part | `Verify`, inline markup | adequate |

### Visible output is decided

Every expected value equals `main`'s output and is covered by maintainer answer 1 (B) under `Maintainer answers` in `00-lead.md`; RH2004's value is also demanded by SHARED-DIAG-012.

### Failure classes dismissed

- FC-024 — partial constructors are the subject; covered by the answer and the tests.
- FC-033 — the generated-implementing-part case uses two documents, one generated.
- Every other class — no production code changed for this issue.

### Architecture findings

_None._ The three ways of reaching the defining part remain, by the maintainer's decision.

### Hints

_None._ (The RH8102 follow-up candidate is already in `00-lead.md`.)

## Whole run

- Build and tests: trusted from 05-cleaner.md (Release build 0 warnings) and 04-implementer.md (analyzer 9379/9379, architecture 118/118 after the spec rename per 02-spec-author.md).
- Lane check: confirmed bug-fix (above).
- No `TODO`, commented-out code, or issue/PR number in changed source (`git diff … -- src | grep`).
- Probe and fixtures were in the session scratchpad; the working tree was clean before this handover.

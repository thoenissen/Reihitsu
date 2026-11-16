Role: auditor
Commit: 28450b6fa1b55e6938ef75de64c7599953e1face
Verdict: findings

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

Repair delta against `3ccbe182cd0d6d85f8d92891c0393a169d185d7f` (audit 1, `08-auditor.md`). Head `28450b6fa1b55e6938ef75de64c7599953e1face` (checked out, clean). `origin/main` `1d2d29a970cd9144d0527e1c505303f2ef164a66` (`git ls-remote`) is an ancestor of the head. The diff `3ccbe18..28450b6` is what handovers 09–13 describe: SHARED-DOC-019 and its ID, the `Uses` rows and -001 sentences of RH8035/RH8036/RH8108/RH8203, the change doc, `DocumentationAnalysisUtilities.IsUnionWithParameterElement` + private `HasParameterElement`, one early return each in `SplitElementDocumentationAnalyzerBase`, RH8108, and RH8203, and 31 tests. The repair stays inside the four rules and their shared helper; scope did not grow.

Probes ran in a throwaway worktree of the head (removed, nothing committed), Debug, test classes on the repository's `AnalyzerTestsBase` with `UnionSupport`, default documentation mode, and `CompilerDiagnostics.None`, as the counterexamples of audit 1.

## Previous findings

- #1 — **closed**. Every member its enumeration listed holds on the head: RH8203 with `param` and `paramref`, RH8035/RH8036 with `param`/`paramref` and no direct `<summary>`, RH8108 with `param` (tests of `10-test-implementer.md`, green per `11-implementer.md`; the lower-case probes below agree). Partial unions (decision 5) hold as well. A member the enumeration did not name still fails: #2.

## Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 2 | high | `src/Reihitsu.Analyzer/Core/DocumentationAnalysisUtilities.cs:661` (`HasParameterElement`: `obj is "param" or "paramref"`), reached from the same four callers as #1 | FC-016, FC-002 | #1 | PR-introduced | The guard matches the tag name ordinally in lower case, but the compiler recognizes a `param`/`paramref` name attribute regardless of the tag name's case: a union whose documentation holds `<Param name=…>`, `<PARAMREF name=…/>`, or `<pAram name=…>` passes the guard, reaches `XmlDocumentationExpander`, and the rule throws AD0001 with the same `NullReferenceException` as #1 (`BinderFactoryVisitor.GetParameterNameAttributeValueBinder` ← `VisitXmlNameAttribute` ← `DocumentationCommentWalker`). SHARED-DOC-019 leaves non-lower-case tag names "not covered", so the spec does not forbid the crash either. Counterexamples below. | Refuse the class with the same cheap check: compare the tag name case-insensitively (as `DirectDocumentationSyntaxChecker.GetTagsIncludingNested` already does), drop "in lower case" from SHARED-DOC-019's coverage sentence, and add a test with a non-lower-case tag name for the rules that reach the expander. |

Counterexamples for #2 (default mode, `UnionSupport`, `CompilerDiagnostics.None`; each observed on the head):

```csharp
// RH8203 — AD0001 (NullReferenceException)
/// <inheritdoc/>
/// <Param name="Circle">Circle</Param>
public union Shape(int, string);

// RH8203 — AD0001
/// <inheritdoc/>
/// <summary>S <PARAMREF name="x"/></summary>
public union Shape(int, string);

// RH8035 — AD0001 instead of RH8035 or no report
/// <Param name="Circle">Circle</Param>
public union Shape(int, string);

// RH8108 — AD0001
/// <summary>R</summary>
/// <typeparam name="T">T</typeparam>
/// <pAram name="x">X</pAram>
public union Result<T>(T, string);
```

Class enumeration for #2. Axes: rule (RH8035, RH8036, RH8108, RH8203) × spelling of the element name (lower case, other case, namespace prefix) × element (`param`, `paramref`) × part (single, partial part).

- Fail: RH8203 (`<Param>`, `<PARAMREF>`), RH8035 (`<Param>`), RH8108 (`<pAram>`) — probed. RH8036 on a non-partial private union with `<Param>` or `<ParamRef>` — traced: the same base `OnDeclaration`, guard, and `HasRequiredDocumentation` path as RH8035.
- Hold: every lower-case spelling (tests of the repair); a prefixed `<x:param name="x"/>` (RH8203, probed; the guard reads the local name and refuses it); an unterminated `<param name="x"` start tag (RH8203, RH8035, probed); a lower-case `<param>` on a union with an attribute list (RH8203, probed); a private partial union whose second part holds `<ParamRef name="x"/>` (RH8036, probed: no exception, both parts reported as RH8036-001 states for a union without `<summary>`); RH8030 and RH8109–RH8111 (no expander, unchanged from audit 1).
- Not a member: a documentation comment written after the attribute list (`[Obsolete]` then `/// <param …>`): it is not the union's documentation for the compiler either; RH8203 reports the leading `<inheritdoc/>` and nothing throws (probed). `<inheritdoc cref="Shape"/>` on a method or another union pointing at a refused union: no exception (probed for RH8203 and RH8035), because the expansion does not follow `inheritdoc`.

A caller reaches it: the analyzer package runs these rules on a user's compilation in the IDE and in builds; the input is the same as #1 with a capitalized tag name.

## Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Whether a union reaches the compiler's documentation expansion (`IsUnionWithParameterElement`) | every `DocumentationCommentTriviaSyntax` in the leading trivia of the union declaration (`GetDocumentationComments`), every descendant `XmlElementSyntax`/`XmlEmptyElementSyntax`, local name compared ordinally to `param`/`paramref`; for a `partial` union, the same for every `DeclaringSyntaxReferences` part | Roslyn: documentation comments in the declaration's leading trivia, all parts joined for a partial type; a `name` attribute of a `param`/`paramref` element bound by `GetParameterNameAttributeValueBinder`, the element name recognized case-insensitively | trivia span agrees (a superset: every doc comment in the leading trivia; doc comments after attributes are not the union's for either side, probed); depth agrees; partial parts agree; prefix: guard refuses more (harmless); case: guard covers less | #2 |
| Where the guard runs (RH8035/RH8036) | before `MatchesAccessibilityGroup` and `HasRequiredDocumentation` | the only expander call of `SplitElementDocumentationAnalyzerBase` is in `HasRequiredDocumentation` | agree | ok |
| Where the guard runs (RH8108, RH8203) | first condition after `NeedsDocumentation` (RH8108) / first condition (RH8203) | expander calls at RH8108:64, RH8203:92/104 | agree; RH8203:92 (`GetExpandedDocumentationOfPart`) excludes `TypeDeclarationSyntax`, so a union never reaches it | ok |
| Which rules need the guard | `rg` of every expander and `HasRequiredDocumentation` caller | kind lists: union is in `DocumentableDeclarationKinds` (RH8203, RH8030 via `SummaryDocumentationKinds`), `TypeParameterOwnerKinds` (RH8108–RH8111), and RH8035/RH8036 | the four rules that reach the expander with a union carry the guard; RH8030, RH8109–RH8111 never call the expander; RH8101–RH8105, RH8032/RH8033, RH8013/RH8014 receive no union | ok |
| Cost on non-union declarations | `IsKind(UnionDeclaration)` first; `GetDeclaredSymbol` only for a `partial` union | – | RH8203 runs it on every documentable declaration; the kind check returns at once | ok |

## Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Refusal of a union with a parameter element | `rg -n "IsUnionWithParameterElement" src` → the helper and its three callers (base, RH8108, RH8203) | one owner |
| Finding an element by tag name at any depth of a documentation comment | `DirectDocumentationSyntaxChecker.GetTagsIncludingNested` (case-insensitive, top-level-wins fallback) and the new `HasParameterElement` (ordinal); both use `XmlDocumentationElementOrderingUtilities.GetTagName` | two owners, difference justified in `11-implementer.md` (the fallback does not state "at any depth"); the ordinal comparison is the divergence behind #2 |
| Documentation comments of a declaration | `DirectDocumentationSyntaxChecker.GetDocumentationComments`, reused | one owner |

## Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH8035/RH8036/RH8108/RH8203 `…HoldsParam…`, `…Paramref…`, `…ParameterElement…` (default mode) | no report and no exception | AD0001 or a rule report (the testing library compares AD0001 as a diagnostic; 17 of them failed with AD0001 or a report before the repair, `10-test-implementer.md`) | `Verify`, `CompilerDiagnostics.None` (only to hide CS1569) | ok; none uses a non-lower-case tag name (#2) |
| `…InParseMode` variants | refusal is mode-independent | a rule report under `Parse` | `Verify` + `ApplyDocumentationModeParseToTestProject` | ok |
| `…LaterDocumentationComment…` (RH8035, RH8203) | the later comment of SHARED-DOC-006 is read | a guard reading only the first comment | `Verify` | ok |
| Partial-union tests (RH8035 ×3, RH8036, RH8108, RH8203 ×2) | every part refused when any part holds the element, also across files | a guard reading only the analyzed part (6 of 7 failed before `7d03636`) | `Verify`, second source via `TestState.Sources` | ok; `RH8035…PartialUnionWhoseOtherPartHoldsParamElement` passed before the widening (its first part is documented by `<inheritdoc/>`), the other six falsify it |
| Other side: `VerifyDiagnosticFor…HoldsNoParameterElement` (RH8035, RH8036), `…WithoutParameterElementAndInBodyOfRefusedUnion` (RH8203), nested-declaration tests (RH8035, RH8036, RH8108) | a union without the element is still judged; nested declarations judged on their own | a guard that refuses too much, or refuses the body | `Verify` with markup; the RH8035/RH8036 "no element" tests keep compiler diagnostics on | ok |
| RH8030, RH8109, RH8110, RH8111 with `<param>` | these rules still judge such a union | a guard added to them | `Verify` with markup | ok |

## Failure classes dismissed

- FC-001 — the guard's span is a superset of the compiler's on every axis but tag-name case, which is #2 (FC-002, FC-016).
- FC-003 — both sides of the condition are tested per rule (with and without the element; nested declarations).
- FC-004, FC-005, FC-019–FC-021 — the repair edits no token gap and offers no fix.
- FC-006, FC-022, FC-023, FC-025–FC-027, FC-036 — no rewrite; `partial` (FC-024) is covered by decision 5 and its tests.
- FC-007–FC-009, FC-018, FC-030 — the four rules have no code fix.
- FC-010–FC-014, FC-028, FC-029, FC-038–FC-043 — formatter, CLI, and precheck are not involved; FC-040's concern (documentation mode) is covered by the `Parse` tests and mode `None` (SHARED-DOC-008).
- FC-015 — see *Policy ownership*.
- FC-017 — see *Assertion adequacy*.
- FC-031 — the summaries of `IsUnionWithParameterElement` and `HasParameterElement` describe the new code; the three changed `OnDeclaration` summaries are generic. No `TODO`, no tracker number.
- FC-032 — no project result is reused across a compiled change; the probes ran on the head.
- FC-033 — every new test switches off only compiler diagnostics (CS1569), and AD0001 stays compared.
- FC-034, FC-035 — expected values come from SHARED-DOC-019's examples and the -001 requirements.
- FC-037 — no text-only claim.

Visible output: the new expected values are "not reported" for a refused rare class, which is also `main`'s output (no rule judges a union on `main`), and "reported" for unions without the element, covered by answers 1A–5A. No rule page changed. Scope: nothing outside the run.

## Architecture findings

#2 repeats the class of #1: the same decision — whether a union's documentation reaches the compiler's documentation expansion, which throws for a parameter name attribute on a union — failing between the same components (`IsUnionWithParameterElement` in front of RH8035, RH8036, RH8108, RH8203, and Roslyn's documentation-comment binder), differing only in the tag name's case. Structural cause: the guard predicts which elements the compiler binds as parameter names, and its prediction (ordinal, lower case) is narrower than the compiler's (case-insensitive). The places: `DocumentationAnalysisUtilities.cs:661` and SHARED-DOC-019's coverage sentence (`sdd/specs/shared/element-documentation.md:1519`).

## Hints

- The summary of `IsUnionWithParameterElement` and SHARED-DOC-019's *Why* say the compiler's expansion "fails"/"throws" without the mode; it throws under `Diagnose` only (audit 1, mode `Parse` probed). The refusal is mode-independent by decision (`09-spec-author.md`), so this is wording only.

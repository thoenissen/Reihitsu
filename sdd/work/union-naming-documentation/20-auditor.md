Role: auditor
Commit: cbac9ac925abdfde59572be2bb283462b7095607
Verdict: findings

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

Delta of the architecture-finding refusal for #2 (lead decision 6, decision 7) against `28450b6fa1b55e6938ef75de64c7599953e1face` (audit 2, `14-auditor.md`). Not a repair round. Head `cbac9ac925abdfde59572be2bb283462b7095607` (checked out, clean). `origin/main` `1d2d29a970cd9144d0527e1c505303f2ef164a66` (prompt, `git ls-remote`; `git fetch` agrees) is an ancestor of the head. The diff `28450b6..cbac9ac` is what handovers 15–19 describe: SHARED-DOC-019 widened (parameter element in any letter case and prefix ignored; paragraph (b) for a failed expansion; new example), the -001 sentences of RH8035/RH8036/RH8108/RH8203, the change doc; `IsUnionWithParameterElement` renamed `IsUnjudgedUnion`, `HasParameterElement` reusing `DirectDocumentationSyntaxChecker.GetTagsIncludingNested`, new `XmlDocumentationExpander.CanExpandDocumentation`, the three renamed call sites; 20 tests. No path outside the four rules, their shared helpers, specs, change doc, and tests; scope did not grow.

Probes ran in a throwaway worktree of the head (removed, nothing committed), Debug, test classes on the repository's `AnalyzerTestsBase` with `UnionSupport`, default mode `Diagnose` unless stated, as in audit 2.

## Previous findings

- #1 — **closed** (unchanged since audit 2; the lower-case tests are unchanged and the full analyzer run of `17-implementer.md` is green).
- #2 — **closed**. Every member its enumeration listed holds: `<Param>`/`<PARAMREF>` for RH8203, `<Param>`/`<PARAM>` for RH8035, `<pAram>` for RH8108, `<Param>`/`<ParamRef>` for RH8036, in Diagnose and Parse, and the partial-part variants (tests of `16-test-implementer.md`, 14 of them failing before `3a41188`, green after per `17-implementer.md`); the prefix and the `<Parameter>` other side still hold. A member on its *part* axis that the enumeration did not name still fails: #3.

## Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 3 | high | `src/Reihitsu.Analyzer/Core/DocumentationAnalysisUtilities.cs:536` (`IsUnjudgedUnion`: the other declarations are read only when the analyzed declaration carries `partial`) | FC-001, FC-024, FC-040 | #2 | PR-introduced | The compiler merges every declaration of a union of the same name into one symbol, also one without `partial` (CS0260, or CS0101 when none carries it), and joins their documentation; SHARED-DOC-019 refuses the union when "a documentation comment of one of its declarations" holds a parameter element, in every mode. The guard reads the other declarations only from a declaration that carries `partial`. Analyzing a declaration without `partial` whose sibling holds the element: under `Diagnose` it is refused only because the expansion probe catches the compiler's exception; under `Parse` the expansion does not throw and the rule judges the declaration — RH8035 reports it. Counterexample below. The input does not compile (CS0260/CS0101) and nothing crashes; the defect is a report the requirement forbids, and an outcome that depends on the documentation mode | Refuse the class: read the documentation of every declaration of the union symbol whenever it has more than one declaration, whatever modifier the analyzed declaration carries (drops the `partial` precondition, a prediction of the compiler's merging); test the declaration without `partial` in Parse and Diagnose |
| 4 | medium | `src/Reihitsu.Analyzer/Core/XmlDocumentationExpander.cs:58` (`CanExpandDocumentation`) and the SHARED-DOC-019 tests | FC-017 | – | new mechanism | SHARED-DOC-019 (b) has no test that fails when the probe is removed. `15-spec-author.md` and `16-test-implementer.md` state no falsifying input is known; one exists on the head: #3's input under `Diagnose`. With `CanExpandDocumentation` removed from `IsUnjudgedUnion` (probe), it and the CS0101 variant end in AD0001 (`NullReferenceException`) from RH8035; with it, both pass. The two include tests pass with or without the probe | Add the #3 input under `Diagnose` as (b)'s test. If #3 is resolved by reading every declaration, that input is refused syntactically and stops falsifying (b); then (b) again has no known falsifying input, the finding lapses, and (b) stays covered by review as 15/16 state |

Counterexample for #3 and #4 (RH8035, `CompilerDiagnostics.None`; `Test0.cs` analyzed):

```csharp
// Test0.cs
/// <remarks>Shape</remarks>
public union Shape;

// Test1.cs
/// <remarks>See <param name="x"/></remarks>
public partial union Shape(int, string);
```

- Head, `Parse`: RH8035 at `Test0.cs(2,14)`; SHARED-DOC-019 demands no report (Test1's declaration holds a parameter element).
- Head, `Diagnose`: no report, no exception — refused by the expansion probe only.
- Head with the probe removed, `Diagnose`: AD0001 from RH8035. The same with `public union Shape(int, string);` in Test1 (CS0101 instead of CS0260).
- With `CompilerDiagnostics.Errors` the compiler reports CS0260 on Test0, which confirms the declarations are merged.

Class enumeration for #3. Axes: rule (RH8035, RH8036, RH8108, RH8203) × modifier of the analyzed declaration (`partial`, none) × which declaration holds the element (analyzed, other) × mode (`Diagnose`, `Parse`) × kind of the other declaration (union, other type kind).

- Fail: analyzed declaration without `partial`, element in another declaration, `Parse` — RH8035 probed; RH8036, RH8108, RH8203 traced (the same `IsUnjudgedUnion` call before every expander call; under `Parse` nothing else stops them).
- Hold: element in the analyzed declaration (any modifier, any mode: syntactic check first); analyzed declaration with `partial` (siblings read); `Diagnose` with the element only in a sibling (probe; probed for RH8035 with CS0260 and CS0101).
- Not a member: a `partial class Shape<T>` merged with a `partial union Shape<T>` (CS0261), `<param>` on the class part, RH8108 on the class part — probed: no AD0001, no report beyond CS0261; the class part is no union, and its expansion does not throw.

A caller reaches it: the analyzer package runs these rules on a user's compilation in the IDE, including code with compile errors. It repeats #2's class: the same decision — whether a union is refused because its joined documentation holds a parameter element — between the same components (`IsUnjudgedUnion` and the compiler's documentation of the union symbol), differing only in the input shape: a member of #2's *part* axis that audit 2 did not name.

## Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Parameter element in a declaration's documentation (`HasParameterElement`) | every `DocumentationCommentTriviaSyntax` in the declaration's leading trivia; via `GetTagsIncludingNested`: top-level elements, otherwise every descendant `XmlElementSyntax`/`XmlEmptyElementSyntax` (non-empty exactly when any descendant matches); local name, `OrdinalIgnoreCase`, `param` or `paramref` | the compiler binds a `name` attribute on a `param`/`paramref` element by its local name, case-insensitively | case and prefix agree now (guard ⊇ on prefix, harmless); depth agrees; samples (`<code>`) included by the fallback, a superset | ok |
| Which declarations are read (`IsUnjudgedUnion`, sibling check) | the other `DeclaringSyntaxReferences` of the union symbol, only when the analyzed declaration has `partial` | the compiler merges every same-named declaration into one symbol and joins their documentation, with or without `partial` | guard covers less: a declaration without `partial` | #3 |
| Expansion probe (`CanExpandDocumentation`) | `GetDeclaredSymbol` of the expander, `GetDocumentationCommentXml(expandIncludes: true, cancellationToken)` | the rules' only expansion of a union: `GetExpandedDocumentation` → `ParseExpandedDocumentation`, same symbol and arguments (`XElement.Parse` failures already caught there); RH8203:92 (`GetExpandedDocumentationOfPart`) excludes type declarations | agree | ok |
| Exception filter | all exceptions but `OperationCanceledException` | `PipelineTraceCommand.cs:298` uses the same filter; cancellation propagates | agree; decision 7 accepts the catch-all | ok |
| Where the probe runs | after both syntactic checks, before every expander call of RH8035/RH8036 (base), RH8108, RH8203; RH8203 runs it for every union | `rg "XmlDocumentationExpander\.(GetExpanded|Parse)"`: expander callers unchanged since audit 2; the four rules are the only ones that reach it with a union | agree | ok |

## Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Tag-name comparison in documentation syntax | `DirectDocumentationSyntaxChecker.GetDirectTags`, `GetTagsIncludingNested`, `GetDocumentingTags` — all `OrdinalIgnoreCase` on `XmlDocumentationElementOrderingUtilities.GetTagName`; `HasParameterElement` holds no comparison of its own | one owner; audit 2's ordinal copy is gone |
| Compiler documentation expansion | `rg GetDocumentationCommentXml src` → `XmlDocumentationExpander.cs:64` (probe) and `:196` (`ParseExpandedDocumentation`), both in the one owner, same arguments | one owner |
| Refusal of an unjudged union | `rg IsUnjudgedUnion src` → helper and three callers; no `IsUnionWithParameterElement` left | one owner |
| Which declarations make up a union's documentation | `IsUnjudgedUnion` (`partial` precondition) vs the compiler's merge | divergence, #3 |

## Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH8035/RH8036/RH8108/RH8203 `…AnotherLetterCase…`, `…UpperCaseParam…`, `…MixedCaseParamref…` (Diagnose) | no report and no exception for a parameter element in another case | AD0001 or a report (observed before `3a41188`) | `Verify`, `CompilerDiagnostics.None` (CS1569 only; AD0001 stays compared) | ok |
| the same `…InParseMode` | refusal is mode-independent | a report under `Parse` (observed for RH8035, RH8036, RH8108, RH8203 before `3a41188`) | `Verify` + `ApplyDocumentationModeParseToTestProject` | ok; `RH8108…AnotherLetterCaseInParseMode` cannot fail (its union documents `T`), the `UndocumentedTypeParameter…InParseMode` test can, as 16 states |
| `…PartialUnionWhosePartInAnotherFile…` (RH8035, RH8203) | every part refused when another part holds the element | a report on either part (observed) | `Verify` + second source | ok; both parts carry `partial`, the member of #3 is untested |
| `…PrefixedParamElement` (RH8035) | prefix ignored | a report | `Verify` | ok, passed before too |
| `…ElementNamedParameter` (RH8035, RH8203) | `<Parameter>` is no parameter element; the union is judged | no report | `Verify` with markup, compiler diagnostics on | ok |
| `…IncludedDocumentationHoldsSummaryAndParamElement` (RH8035) | no exception, no report | AD0001 | `Verify` + include transform | holds with or without the probe; it falsifies neither (a) nor (b) — see #4 |
| `…IncludedDocumentationHoldsOnlyParamElement` (RH8035) | a union whose comment holds no parameter element and whose expansion succeeds is judged | no report | `Verify` + include transform, markup | ok (other side of (b)) |
| SHARED-DOC-019 (b) | no exception when the expansion fails | AD0001 with the probe removed | – | missing, #4 |

## Failure classes dismissed

- FC-001, FC-002, FC-016 — the tag-name axis of #2 is closed (one case-insensitive owner); the remaining span mismatch is #3.
- FC-003 — both sides tested per rule (parameter element in another case / `<Parameter>`; include with and without `<summary>`).
- FC-004, FC-005, FC-019–FC-021 — no token gap is edited, no fix.
- FC-006, FC-022, FC-023, FC-025–FC-027, FC-036 — no rewrite. FC-024 (`partial`): #3.
- FC-007–FC-009, FC-018, FC-030 — the four rules have no code fix.
- FC-010–FC-014, FC-028, FC-029, FC-038, FC-039, FC-041–FC-043 — formatter, CLI, and precheck are not involved. FC-040 (outcome depends on the documentation mode): (b) is mode-dependent by the spec's design for inputs with no parameter element; for #3's input it makes the outcome depend on the mode where (a) forbids it.
- FC-015 — see *Policy ownership*.
- FC-017 — see *Assertion adequacy*.
- FC-031 — the summaries of `IsUnjudgedUnion`, `HasParameterElement`, and `CanExpandDocumentation` describe the new code; the renamed call sites carry no comment. No `TODO`, no tracker number in the `src` diff (`rg`).
- FC-032 — the probes ran on the head; no earlier result reused across the compiled change.
- FC-033 — every new "not reported" test switches off compiler diagnostics only; AD0001 stays compared (observed as failures before the change).
- FC-034, FC-035 — expected values come from SHARED-DOC-019's examples and its two paragraphs, not from the patched code.
- FC-037 — no text-only claim.

Visible output: the new expected values are "not reported" for the refused rare class — `main`'s output, since no rule judges a union there — and "reported" for unions without a parameter element whose documentation expands, covered by answers 1A–5A. No rule page changed. Scope: nothing outside the run.

## Architecture findings

#3 repeats #2's class (see *Findings*). Structural cause: `IsUnjudgedUnion` still predicts the compiler — not which elements it binds any more, but which declarations it joins into the union's documentation — by the `partial` modifier, a narrower set than the compiler's merge. Place: `DocumentationAnalysisUtilities.cs:536–543`; the spec states the compiler's set ("one of its declarations", SHARED-DOC-019, `sdd/specs/shared/element-documentation.md:1517`). Decision 6 (b) contains the consequence under `Diagnose` (no crash); under `Parse` the prediction decides the output.

## Hints

- Decision 7 counts the expansion twice for a union that passes the syntactic checks; the compiler stores a symbol's documentation once it is computed, so the second call is likely cheap. Not measured; no finding.

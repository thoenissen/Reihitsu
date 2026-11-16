Role: auditor
Commit: 7784b474e6104cde39c08ea84c34055059e73e6a
Verdict: pass

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

Delta of the architecture-finding refusal for #3 and #4 (lead decision 8) against `cbac9ac925abdfde59572be2bb283462b7095607` (audit 3, `20-auditor.md`). Not a repair round. Head `7784b474e6104cde39c08ea84c34055059e73e6a` (checked out, clean). `origin/main` `1d2d29a970cd9144d0527e1c505303f2ef164a66` (prompt) is an ancestor of the head. The diff `cbac9ac..7784b47` is what handovers 21–24 describe: `IsUnjudgedUnion` without the `partial` precondition and its summary reworded (`src/Reihitsu.Analyzer/Core/DocumentationAnalysisUtilities.cs`, 7+/7−), 10 tests in the RH8035, RH8036, RH8108, RH8203 test classes, handovers and the lead note. No spec, change doc, or rule page changed; scope did not grow. `scripts/lane-check.sh behavior-change`: confirmed.

Static tracing only; no finding turned on a test run, so the implementer's and cleaner's focused and full results (`22-implementer.md`, `23-cleaner.md`) are taken as given.

## Previous findings

- #1 — **closed** (code and tests untouched since audit 3).
- #2 — **closed** (untouched since audit 3).
- #3 — **closed**. `IsUnjudgedUnion` now reads `HasParameterElement` over every `DeclaringSyntaxReferences` entry of the union's symbol, whatever modifier any declaration carries. That set is the one the compiler joins into the symbol's documentation, so the guard no longer predicts the merge: it reads the compiler's own set. Every member audit 3 enumerated as failing holds by trace and by test: analyzed declaration without `partial`, element only in another declaration, `Parse` — RH8035 (CS0260 and CS0101 variants), RH8036 (same file, `paramref`), RH8108, RH8203, each a `…InParseMode` test that failed before `a9ee349` and passes after (`21-test-implementer.md`, `22-implementer.md`). The members that held still hold: element in the analyzed declaration (first syntactic check, unchanged); analyzed declaration with `partial` (siblings were read before and still are); `Diagnose` (now refused syntactically before the probe instead of by it).
- #4 — **superseded** by the resolution of #3, as its own suggested change and decision 8 state: the only known falsifying input of SHARED-DOC-019 (b) — #3's input under `Diagnose` — is now refused by `HasParameterElement` on the sibling before `CanExpandDocumentation` runs, so it no longer reaches the probe. (b) again has no known falsifying input and stays covered by review, as `15-spec-author.md` and `16-test-implementer.md` state; the probe is unchanged.

Remaining axes of #3's class, checked:

- Kind of the other declaration (union / another type kind): whichever declarations the compiler merges into the union's symbol, the guard reads exactly `DeclaringSyntaxReferences` of that symbol, with `HasParameterElement` taking any `SyntaxNode`; the spans agree by construction. A declaration the compiler does not merge (another symbol) contributes nothing to the union's expanded documentation, so it needs no refusal. The class-part side was probed in audit 3 (no AD0001, no report).
- Same file / another file: both covered by the symbol's references; tested both ways (RH8036 same file, the others another file).
- Analyzed declaration with `partial`, the other without: held before (siblings read) and still does; the guard no longer inspects any modifier.

## Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|

_None._

## Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Which declarations make up a union's documentation (`IsUnjudgedUnion`, sibling check) | every `DeclaringSyntaxReferences` entry of `GetDeclaredSymbol(declaration)` other than the analyzed node, each through `HasParameterElement` (every documentation comment in its leading trivia) | the compiler's documentation of the symbol joins the documentation of the same `DeclaringSyntaxReferences` | equal by construction; no modifier or kind condition left | ok |
| Parameter element in a declaration (`HasParameterElement`) | unchanged since audit 3 | unchanged | agree | ok |
| Expansion probe (`CanExpandDocumentation`) | unchanged; runs only after both syntactic checks | the rules' expansion of the union | agree | ok |
| `obj != declaration` | reference comparison of the analyzed node with each reference's node | — | a miss only re-checks the analyzed declaration, which the first check already found clean; harmless | ok |

## Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Which declarations make up a union's documentation | `rg "PartialKeyword\|DeclaringSyntaxReferences"` over the four rules, their base, `XmlDocumentationExpander`, `DocumentationAnalysisUtilities`: `DocumentationAnalysisUtilities.cs:538` (the guard) and `RH8203…Analyzer.cs:53` (`IsPartOfPartialMember`, excludes `TypeDeclarationSyntax`, so never a union) | one owner; the `partial` copy is gone |
| Refusal of an unjudged union | `rg IsUnjudgedUnion src`: helper and its three callers (`SplitElementDocumentationAnalyzerBase.cs:60`, `RH8108…:50`, `RH8203…:65`), unchanged | one owner |
| Compiler documentation expansion | unchanged since audit 3 (`XmlDocumentationExpander`) | one owner |

## Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `RH8035…UnionWithoutPartialModifierWhosePartialDeclarationInAnotherFileHoldsParamElementInParseMode`, `RH8035…UnionDeclaredTwiceWithoutPartialModifier…InParseMode`, `RH8036…PrivateUnionWithoutPartialModifier…InParseMode`, `RH8108…UnionWithoutPartialModifier…InParseMode`, `RH8203…InheritdocOnUnionWithoutPartialModifier…InParseMode` | a declaration without `partial` is refused when another declaration of the union holds a parameter element, independent of the mode | the rule's report on the declaration without `partial` (observed for all five before `a9ee349`) | `Verify`, `CompilerDiagnostics.None` (CS0260/CS0101 only; AD0001 stays compared), Parse transform | ok |
| the five Diagnose counterparts | no report and no exception in the default mode | AD0001 or a report | same, default mode | ok; they pass with either the syntactic refusal or the probe, so they guard against losing both, not one — the Parse twins pin the syntactic one |

Each analyzed declaration's own comment holds no parameter element and would be reported if judged (RH8035/RH8036: no `<summary>`; RH8108: undocumented `T`; RH8203: `<inheritdoc/>` without a base), so a lost refusal is observable. All 10 carry `[Requirement("<rule>-001")]` and `[Requirement("SHARED-DOC-019")]`.

## Failure classes dismissed

- FC-001, FC-002, FC-024, FC-040 — #3's span mismatch is gone (see *Guard scope*); the `partial` axis no longer exists in the guard, and under the syntactic refusal the outcome for #3's input no longer depends on the mode.
- FC-003 — refused side tested per rule in both modes; the judged side (no parameter element anywhere) is not changed by this diff (sibling check false → probe → judged, as before) and stays covered by the tests of `16-test-implementer.md`.
- FC-004–FC-014, FC-018–FC-023, FC-025–FC-030, FC-036, FC-038, FC-039, FC-041–FC-043 — no rewrite, no code fix, no token gap; formatter, CLI, and precheck are not involved.
- FC-015 — see *Policy ownership*.
- FC-016 — no tag-name comparison changed.
- FC-017 — see *Assertion adequacy*; #4 superseded.
- FC-031 — the rewritten summary of `IsUnjudgedUnion` describes the new code ("declared more than once", "whatever modifiers the declarations carry"). No `TODO`, no tracker number in the `src` diff (`rg`).
- FC-032 — no earlier result reused across a compiled change; the only compiled change is traced on the head.
- FC-033 — every new "not reported" test switches off compiler diagnostics only; AD0001 stays compared.
- FC-034, FC-035 — expected values come from SHARED-DOC-019's "one of its declarations", not from the patched code; the unspecified judged side of a multiply-declared union is left untested on purpose (`21-test-implementer.md`).
- FC-037 — no text-only claim.

Visible output: the new expected values are "not reported" for the refused rare class (input does not compile: CS0260/CS0101, and CS1569 when documentation is written) — `main`'s output for unions, demanded by the unchanged SHARED-DOC-019. No rule page changed. Cost of special cases: the change removes a condition and adds none. Scope: nothing outside the run.

## Architecture findings

_None._ The structural cause audit 3 named — `IsUnjudgedUnion` predicting by the `partial` modifier which declarations the compiler joins — is removed: the guard reads the symbol's declaring references, the compiler's own set.

## Hints

- Text only, no finding: SHARED-DOC-019 ("For a partial union declared in several parts, this holds for every part", and "for a partial union, for none of its parts" in (b)) and the change doc (`sdd/changes/2026-11/union-naming-documentation.md:20` "on any part, for a partial union", `:203`) name the partial case only. The governing sentence ("a documentation comment of one of its declarations") already covers declarations joined without `partial`, as `24-spec-checker.md` states, so the code matches the spec; a later wording pass could say "declared more than once" to match the code's summary.

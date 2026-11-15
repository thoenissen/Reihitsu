Role: skeptic
Commit: 0f49d8e
Verdict: proceed — with one amendment to the sentence added to SHARED-DIAG-008 ("the one exception" is contradicted by RH2005-005)

Worked from thoenissen/Reihitsu#1756, its two maintainer comments, `00-lead.md`, `01-spec-author.md`, and `git diff 9f69e84 0f49d8e -- sdd/specs`. The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Premise

Real, and the spec diff states what the code does today.

- Caller: whole-document analysis of any document — the analyzer runs on every `ParenthesizedExpression` (`RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.Initialize`, `OnParenthesizedExpression`). `ShouldReport`/`IsUnnecessary` (lines ~1050–1115) look at the parent kind and the parser-reading guards only; nothing in the analyzer inspects trivia, directives, or disabled text. So a conditional directive between `(` and `)` never keeps a pair from being reported.
- Fix: `RH3002StatementMustNotUseUnnecessaryParenthesesCodeFixProvider` line 41 withholds itself on `SyntaxNodeUtilities.InteriorContainsCommentOrDirective(parenthesizedExpression)`, which walks `DescendantTrivia(node.Span, descendIntoTrivia: true)` — the `#if`/`#endif` in the leading trivia of `)` lies inside that span.
- Executed `scripts/apply-fix.sh RH3002 <tmp>` over nine fixtures (LF and CRLF each); every arm `no-fix-offered`, none `no-diagnostic`:
  - the issue's input (`return (a` / `#if DEBUG` / `+ 1` / `#endif` / `);`);
  - the same with `#if false`, and with `#if !NEVER` (the guarded code active);
  - `return (` / `#if X a #else b #endif` / `);` and the `#if X` / `#elif Y` / `#else` form;
  - the pair as an argument `N((a #if X + 1 #endif))` (outer pair; the runner fixes only the first diagnostic, so the inner pair of nested fixtures is not settled by this run);
  - **receiver**: `return (a` / `#if X` / `+ 1` / `#endif` / `).ToString();` and `return (` / `#if X a + b #else a #endif` / `).ToString();`.
- The receiver fixtures are the one shape where the verdict differs per build: without `X` the content is the name `a` (RH3002-002, reported); with `X` it is `a + b` (not reported). Today the non-`X` build reports it without a fix. This is exactly the case SHARED-DIAG-008's `Why:` was written for, but the maintainer's answer (judge the code as the current build's symbols are set, RH3002 keeps reporting) covers it, and RH3002-010 as written states it. Not an open output question; named so the lead can confirm the reading.
- No git history needed: no regression is claimed.

## Mechanism and candidates

_N/A_ — no defect; the issue's "contradiction" is between two spec texts, resolved by the maintainer's answer B.

One wording observation (internal): "or code that such a directive excludes, between `(` and `)`" in RH3002-010 adds no member — `(` and `)` are active tokens, so excluded code between them is always bounded by a directive between them. Harmless; it mirrors SHARED-DIAG-008's wording.

## Breaking inputs

Failure classes reaching the requirement: FC-020 (directive kind, including `#if false`), FC-005 (fix guard covers directives, not just comments — holds), FC-027 (position of the pair, nesting), FC-008 (Fix All next to a withheld pair).

Matrix for RH3002-010 — directive kind × position; every member below was run and is reported with no fix:

| | `return` (issue) | argument | receiver (verdict differs per build) |
|---|---|---|---|
| `#if` … `#endif`, guarded code excluded | reported, no fix | reported, no fix | reported, no fix |
| `#if false` | reported, no fix | — | — |
| `#if !NEVER` (guarded code active) | reported, no fix | — | — |
| `#if` / `#else` | reported, no fix | — | reported, no fix |
| `#if` / `#elif` / `#else` | reported, no fix | — | — |

Inputs the test implementer should cover for RH3002-010 (each: diagnostic `(`…`)`, no code fix offered):

1. The spec example — the issue's input. Its verdict does not depend on whether `DEBUG` is defined (a `return` value is reported either way), so the test does not depend on the test framework's preprocessor symbols.
2. The receiver input `return (a` / `#if X` / `+ 1` / `#endif` / `).ToString();` — the only shape that pins "judged as the current build compiles it" (with `X` it would not be reported). Requires that the test framework defines no `X`.
3. An `#if` / `#else` form, so `#else` (excluded code in front of an active branch) is covered, not only a lone `#if`.

Optional: a CRLF repeat as the file's other tests do; a Fix All scenario is already the class base's job and needs nothing new.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| RH3002 analyzer | must stay |
| RH3002 code fix / Fix All | must stay |
| `Reihitsu.Core` (`SyntaxNodeUtilities`) | not involved |
| Formatter | not involved — no phase removes parentheses |
| Rule page `RH3002.md` | must stay; `general-notes.md` ("Code excluded by `#if`", and the "reported, but no code fix" bullet) already states the behavior |
| Specs | change: RH3002-009, RH3002-010, SHARED-DIAG-008 (with the amendment below) |
| Tests | must change: tests tagged `RH3002-010` |

## Refuse option

Nothing is built, so refusing costs nothing and is already in place: the fix is withheld for every comment or directive inside the pair (RH3002-009). Declaring the shape unsupported (a conditional directive inside a parenthesized expression) is not an option worth naming: the formatter never removes parentheses, no formatter phase or RH3002 code carries a special case for it, and it would delete nothing — a shape that deletes nothing is not declared. No simpler option: the spec change is already the minimum.

## Output questions

_None._ The maintainer answered #1756 (B) and stated the principle that also decides the receiver shape above.

## Internal choices

1. **The added SHARED-DIAG-008 sentence says "RH3002 is the one exception". That is false against an existing spec.** RH2005-005 (`sdd/specs/analyzer/RH2005.md`, "This includes an `#if` block whose excluded code stands in one of these places. The diagnostic is still reported.") reports a `public` field whose modifier list — the span RH2005 judges, "a `public` field that can be reassigned" — holds an excluded `readonly`:

   ```csharp
   public class C
   {
       static public
   #if NEVER
           readonly
   #endif
           int {|RH2005:Total|};
   }
   ```

   In the `NEVER` build the field is `readonly` and not reported, so this is the same departure from SHARED-DIAG-008 as RH3002-010. Ran `scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH2005FieldsMustBePrivateAnalyzerTests.VerifyNoFixIsOfferedForAnExcludedModifier"`: 3 passed, so the code does report it. RH2001-007 ("A directive includes an `#if` block and the code it excludes. The diagnostic is still reported.", with a test of `#if DEBUG set; #endif` inside the accessor list of a reported property) is a weaker second candidate, depending on whether the accessor list counts as RH2001's judged span.
   - **A (recommended)** — drop "the one": e.g. "RH3002-010 departs from this requirement for RH3002: it reports a parenthesized expression with a conditional directive, or code such a directive excludes, between its parentheses, and withholds the fix." Still names no general permission, as the spec author intended, and states nothing about other rules.
   - **B** — name RH2005 (and RH2001) as further exceptions. Puts claims about rules this run does not touch into a shared spec, while their own specs neither cite SHARED-DIAG-008 nor call it an exception.
   - Neither changes visible output.
2. RH3002-010 as a separate requirement from RH3002-009 (spec author's choice): agree — the report and the fix are separate decisions with separate tests.
3. No change doc: agree — no output changes.

## Issue goals

- "Lane: behavior-change" — drop: nothing shipped changes; the run is tests lane. No visible output.
- "Required output" A (recommended by the issue) — overridden by the maintainer's answer B. No visible output change versus `main`.
- "Done when: answer recorded in the RH3002 spec (with a change doc if the visible output changes)" — met by RH3002-010 without a change doc.
- "Done when: the input above has a covering test" — keep; add the receiver input (Breaking inputs, 2), which the issue's input does not exercise.
- Severity: the issue described a spec contradiction, not a wrong output; nothing a user sees was wrong.

## Architecture findings

_None._

## Follow-up candidates

- Extend thoenissen/Reihitsu#1759 (no new issue): RH2005-005, and possibly RH2001-007, already report a construct with a conditional directive or excluded code in the span they judge, without citing SHARED-DIAG-008. Today that is an unrecorded departure from SHARED-DIAG-008 (or a defect against it); once #1759 reverses the reporting half it becomes conformant, and its "every affected rule" list should name them so they get their "reported, no fix" tests and SHARED-DIAG-008 references. No visible output in question.

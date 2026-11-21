Role: spec author
Commit: bf69a6a
Verdict: spec written

Applies Maintainer answers 23 and 24. The prompt carried no intended fix, suspected cause, or hoped-for verdict to ignore. The change doc stays `Status: Draft`.

**Answer 23 (04-spec-author Q1–Q4, all A).** The spec was already written on A; this round makes each answer explicit:
- `sdd/specs/analyzer/RH2009.md` — RH2009-003 recursion item: only an invocation counts; `new` of its own type in a constructor, its own property or indexer in an accessor, and its own operator in an operator score nothing. The *Nothing else* list names those uses and `&=`, `|=`, `^=` (also on `bool`). Two new examples at the end of RH2009-003: (1) limit 3, `Node`'s constructor (3, `new Node(…)` nothing) and `All` (3, `&=` nothing), not reported; (2) limit 2, a `?:` in a `when` clause nests in the switch, `M` scores 3 and is reported. `bool?` already had example `K`.
- `documentation/cognitive-complexity.md` — construct table: logical operators "on `bool` or `bool?`", new row `&=`, `|=`, `^=` = 0; switch section: a `?:` in a `when` clause adds its nesting points; recursion section: only `M(…)`, `this.M(…)`, `C.M(…)` count, not `new`, accessors, or operators.
- `documentation/rules/RH2009.md` — "on `bool` or `bool?`". Q2–Q4 are corner cases by the rule-page convention and stay on the guide.
- `sdd/changes/2026-11/method-complexity-rule.md` — *Decisions* gains "What scores, and how", recording each of the four answers with its reason; the rejected B readings are under *Alternatives considered*.

**Answer 24 (no reference to the external paper).** Removed from the guide (intro citation and link, the whole departures section; its one point not stated elsewhere — no totals per type, file, or project — moved to *What is scored*), the spec's introduction (now: RH2009-003 defines the score), and the change doc (*What* citation, the "stay close" decision, the departures framing of *Alternatives considered*, every "the paper" in reasons). The decision "Stay close to the published definition" is replaced by "Lambdas count; local functions stand alone" and "What scores, and how", which state the same decisions by their own reasons. The rule page and shared specs had none. `rg -i "sonar|white ?paper|paper|published definition|campbell"` over every changed file outside `sdd/work/` finds nothing. The local PDF copy in the session scratchpad was deleted.

Checks:
- All 53 C# blocks of the RH2009 spec, both shared specs, the change doc, the guide, and the rule page (markup removed; the rule page's method snippets wrapped in a class) compile with the .NET 11 SDK, `FEATURE` defined: `Build succeeded`.
- New example scores recomputed by hand: `Node` ctor `if` 1 + `if` 2 = 3; `All` `if` 1 + `foreach` 2 = 3; `M` `switch` 1 + `?:` 2 (nesting 1, in the `when` clause) = 3.
- No requirement ID, `Uses` row, or code owner changed; architecture tests not rerun (04's result stands).

## Changed requirements

- RH2009-003 — Cognitive complexity of a body — clarified: recursion only by invocation (not `new`, accessors, operators); compound assignments named in *Nothing else*; two new examples.
- Spec introduction — no external source; RH2009-003 defines the score.

## Examples for the tests

RH2009-001 to RH2009-006 (every example, including the two new ones at the end of RH2009-003), SHARED-LIMIT-001 and SHARED-LIMIT-002, RH0001-007, as in 04-spec-author.md.

## Output questions

_None._

## Internal choices

- The guide's former departures list is dropped, not rewritten: every point but one already had its own section; "no totals per type, file, or project" moved to *What is scored*.
- The change doc records the four answers without "answer A" labels, by their reasons, so it reads without the chat.

## Follow-up candidates

_None_ beyond 04-spec-author.md (indirect recursion) and the lead's list.

Role: spec author
Commit: e2e0749 (the tree the edits start from; the spec diff of 6990dee is unchanged apart from the lines below)
Verdict: spec written

My prompt carried no intended fix, suspected cause, or hoped-for verdict. It restated the lead's decisions 1–4, which I carried out from `00-lead.md`, not from the prompt.

The maintainer's batch-1 answers (all six as recommended) match the draft of 01-spec-author.md for questions 1–5, so no requirement's normative text changes. Question 6 (declaring a comment or directive in front of the arrow) is declared in a change of its own: it is not in this run's specs; the change doc's *Alternatives considered* now records it as the maintainer's decision for a separate change.

## Changed requirements

- FMT-UNSUP-012 — Comments and directives between the arrow of an expression body and its expression are unsupported — clarified (Why only, decision 4): "a comment behind it is no more common" replaced by the fact that RH3202, RH3203, RH3205–RH3208 report every expression-bodied method, constructor, operator, conversion operator, finalizer, and local function, so a comment behind the arrow is rare by definition, a local function's included.
- SHARED-BODY-001 — A member the formatter cannot convert is not reported — clarified (Why only, decision 3): the analyzer and the code fix share the predicate the formatter uses for FMT-STRUCT-007; for a directive or disabled text behind the arrow the formatter decides through FMT-UNSUP-012 before any conversion, and the analyzer keeps the predicate for that place so it does not report a member the formatter refuses.
- Change doc `sdd/changes/2026-11/expression-body-arrow-comment-unsupported.md`:
  - `Status: Accepted`.
  - *Decisions*: "pending confirmation" removed; the declaration is confirmed in this change; the answers to questions 2–5 recorded (accessors and local functions, directives and disabled text in every build, one line, the CLI reason text and why it differs from the issue's wording).
  - *Why*: the decision paragraph says it was asked again and confirmed; the rarity paragraph cites RH3202–RH3208 like FMT-UNSUP-012, and states (skeptic) that a method with a conditional group behind the arrow is reported by no rule, so directives are not rare by definition for every owner, with the reason they are in the shape anyway.
  - *What* (decision 2): the member's-column layout is unreachable only for a comment or directive behind the arrow; it remains for one in front of the arrow and for a `return` value on the next line.
  - *Decisions → RH5401-004 changed in place* (decision 1): the issue's goal of deleting the format-and-compare is stated as dropped, with the reason.
  - *Decisions → analyzers keep their decision for a directive*: points to SHARED-BODY-001's reworded reason.
  - *Alternatives considered*: the in-front shape is the maintainer's decision for a change of its own.

No rule page changes (unchanged from 01).

## Examples for the tests

Unchanged from 01-spec-author.md: FMT-UNSUP-012, FMT-STRUCT-006, FMT-STRUCT-024, RH5401-004, RH7101-007, SHARED-BODY-001, and change-doc changes 1–10. No example was added or changed in this round.

## Output questions

_None._

## Internal choices

- Directive rarity: the change doc now says plainly that a method with a conditional group behind the arrow is reported by no rule (skeptic, output question 3 addition), instead of "rarer still". Alternative: keep the old sentence — contradicted by the skeptic's `apply-fix.sh RH3202` result (`no-diagnostic`).
- The reason the CLI reason text differs from the issue's wording is recorded in *Decisions* (the issue is `agent-filed`; the maintainer chose A).

## Follow-up candidates

_None new._ The in-front shape (maintainer answer 6 A, in a change of its own) and the follow-ups listed in `00-lead.md` stay as the lead recorded them.

## Checks

- `scripts/test.sh --project architecture`: 196 passed, 1 failed — `EveryRequirementOfAnEnforcedSpecIsCovered` reports only `FMT-UNSUP-012: no test covers it`, as in 01; expected until the test implementer adds its tests. The spec-structure tests pass with the reworded Why sentences.

Role: spec author
Commit: 7fc6eba
Verdict: change doc written

Inputs: `00-lead.md` (*Maintainer answers*: batch 1, Q1 **A**, Q2 **A**), `03-spec-author.md`, the change doc, FMT-UNSUP-013, SHARED-BODY-001. My prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the maintainer's answer "1a 2a", which `00-lead.md` records.

## Changed requirements

None. Both answers select the option every spec already states:

- Q1 A (change 12, a conditional group around the signature, a constraint clause, or a property's type closing in front of a shared `=>`, is refused): FMT-UNSUP-013's *Why* already says such a group puts a directive into the gap and is refused, and SHARED-BODY-001 already states that RH3202–RH3208 report such a member with the fix withheld (FMT-UNSUP-013, SHARED-FIX-010). RH5401-004 already withholds RH5401's fix through the precheck.
- Q2 A (reason `a comment or directive between the arrow of an expression body and the code in front of it`): FMT-UNSUP-013 already gives this reason, and the change doc's *What* already names it.

Change doc `sdd/changes/2026-11/expression-body-arrow-leading-comment-unsupported.md`:

- `Status: Accepted` (every visible output change has the maintainer's answer: 1–11 under escalation 10, as lead decision 6 records; 12 and the reason in batch 1).
- *Decisions* → *Declare the shape, in this change*: records that changes 1–11 lie inside the span escalation 10 declared, and the two batch-1 answers (change 12 declared in this work, with RH3202's and every formatting fix withheld and RH3202 and RH5401 still reporting; the reason wording and why it beat the issue's).
- *Decisions* → *Directives and disabled text belong to the shape*: notes the maintainer accepted change 12's cost, and that the group around the whole member stays supported.
- *Alternatives considered*: two entries rejected by the maintainer — leaving change 12's group out of the shape, and the issue's reason wording.

No rule page, `Uses` row, code owner, or requirement-ID reference changed, so the architecture tests were not rerun for this edit.

## Examples for the tests

Unchanged from `03-spec-author.md` and `01-spec-author.md`. Change 12's refusal locations remain derived from FMT-UNSUP-013's location rule, not observed; the test implementer confirms them once the precheck exists.

## Output questions

_None._

## Internal choices

- The batch-1 answers are recorded inside the existing escalation-10 decision rather than as separate decision headings, as the sibling change doc (`expression-body-arrow-comment-unsupported.md`) records its run's answers. Alternative: one heading per answer.
- I set `Status: Accepted` as the lead's prompt asked; `sdd/README.md` says the lead sets it in a squad run, so the lead owns that edit.

## Follow-up candidates

_None_ new. The accessor-arrow candidate (lead decision 5) stands.

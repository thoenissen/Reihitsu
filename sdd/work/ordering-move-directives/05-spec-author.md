Role: spec author
Commit: 3932bd5
Verdict: spec written

Scope: decision 6 of `00-lead.md` only — the RH7103 example of SHARED-ORDER-008 "a group in the body in front of the spans" (`sdd/specs/shared/ordering-move-safety.md`).

## Changed requirements

- SHARED-ORDER-008 — A conditional directive in the moved or the crossed span refuses a member move — clarified (example only). The RH7103 example's input put the `#endif` in the leading trivia of `Run`, the move's target, so the group lay in the crossed span (SHARED-ORDER-003) and the normative text refuses the move. The input now has `private int _count;` between the group and `Run`, the input of `FixMovesAStaticMethodBehindAGroupInTheBodyInFrontOfTheSpans`; the shown output is that test's fixed source. One sentence behind the example states the other side of the boundary: without `_count`, the group lies in the crossed span and no code fix is offered (`NoCodeFixWhenAGroupStandsInTheLeadingTriviaOfTheTarget`). The RH7109 example in front of it is unchanged: its target is `_a`, so the group already stands in front of the spans.

The change doc `sdd/changes/2026-11/ordering-move-refuses-conditional-groups.md` repeats only the RH7109 variant (section 2), which is consistent; it is unchanged. No rule page changes.

Checked: `scripts/apply-fix.sh RH7103` on 3932bd5 gives the shown output for the corrected input under LF and CRLF (fixed, 1 action). For the old input, HEAD still offers the fix and moves `Create` in front of the group, which stays with `Run` — the refusal the implementer has to build, pinned by the red test above. No requirement-ID reference, `Uses` row, or code owner was added or removed, so the architecture tests were not run.

## Examples for the tests

SHARED-ORDER-008 (RH7103 example): already covered by `FixMovesAStaticMethodBehindAGroupInTheBodyInFrontOfTheSpans` and `NoCodeFixWhenAGroupStandsInTheLeadingTriviaOfTheTarget`; no new test.

## Output questions

_None._

## Internal choices

- Kept the RH7109 example's input unchanged and gave the RH7103 example its own input, rather than adding `_count` to both; the RH7109 input is already correct and repeated in the change doc.

## Follow-up candidates

_None._

Role: spec author
Commit: 131a7c3
Verdict: spec written

Merge of `origin/main` (fe1d122) into the branch. Main brought in FMT-UNSUP-011 (a comment, directive, or disabled text between a binary operator and its right operand is unsupported) with `sdd/changes/2026-11/binary-operator-comment-unsupported.md`.

## Changed requirements

- RH2001 header — `Uses` row: union of both sides (the branch's SHARED-DIAG-008, main's FMT-UNSUP-011). RH2001-007 merged without conflict and keeps both sentences: the branch's SHARED-DIAG-008 reference and main's FMT-UNSUP-011 paragraph.
- RH7101 header — `Uses` row: union of both sides (the branch's SHARED-DIAG-008, main's FMT-UNSUP-008, FMT-UNSUP-009, FMT-UNSUP-011). RH7101-007 is main's text unchanged.

`scripts/test.sh --project architecture`: 197 passed.

## Examples for the tests

_None._ No requirement's content changed in the merge.

## Output questions

_None._

## Internal choices

- The change doc `sdd/changes/2026-11/report-across-conditional-directives.md` is not changed. None of its visible output changes holds a binary operator with a directive behind it, and RH5302 — the rule main's change is about — is not among the rules this run reverses. FMT-UNSUP-011 only adds refusals, so the change doc's statement that every other fix reached already refuses the new inputs still holds; listing FMT-UNSUP-011 there would add a reason, not change the outcome. Alternative: name FMT-UNSUP-011 next to FMT-UNSUP-010 for RH7101; rejected as not required by the merge.

## Follow-up candidates

_None._

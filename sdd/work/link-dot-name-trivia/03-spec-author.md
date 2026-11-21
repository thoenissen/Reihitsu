Role: spec author
Commit: 273e79a9
Verdict: change doc written

Inputs read: `00-lead.md` (*Maintainer answers*: batch 1, 1A and 2A), `01-spec-author.md`, `02-skeptic.md`, `sdd/changes/2026-11/unsupported-link-dot-name-trivia.md`, FMT-UNSUP-016 in `sdd/specs/formatter/unsupported-syntax.md`. The prompt carried no intended fix or verdict.

Written:

- Change doc `sdd/changes/2026-11/unsupported-link-dot-name-trivia.md`: `Status: Accepted`. *Decisions*: "The shape is a place in the syntax tree, on any line" is marked as the maintainer's answer 2A and states that the one-line form (`t. /* c */ Length`, `s?. /* c1 */ Length`) is refused in this change; "The reason names the place" is marked as answer 1A and names the rejected alternative `a comment or directive between a member access's . and its name`. *Visible output changes* now calls the reason text the one the maintainer chose, not a proposal.
- Spec: no edit needed. FMT-UNSUP-016 already states both answers as written in a5328b71 — the reason `a comment or directive between a . and its member name`, and the condition **On any line** with `t. /* c */ Length` refused. Specs carry no decision record; the change doc does.

## Changed requirements

None in this round. FMT-UNSUP-016, FMT-UNSUP-015, FMT-BREAK-020, RH2001-007, RH7101-007 stand as listed in `01-spec-author.md`, now accepted.

## Examples for the tests

As in `01-spec-author.md`: FMT-UNSUP-016 (every example, the one-line form included), FMT-UNSUP-015 (the hand-off of the dot-to-name gap), FMT-BREAK-020, RH2001-007, RH7101-007, and the class matrix in `02-skeptic.md`.

## Output questions

_None._

## Internal choices

_None._

## Follow-up candidates

_None new._ The qualified-name candidate stays as recorded in `00-lead.md`.

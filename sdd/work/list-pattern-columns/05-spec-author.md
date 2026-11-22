Role: spec author
Commit: cec3017
Verdict: change doc written

Status follow-up. The maintainer answered batch 1 "as recommended" (1A, 3A, 6A, 7A, 7bA, 8A; `00-lead.md`, *Maintainer answers*). My prompt carried no intended fix or hoped-for verdict.

## Changed requirements

_None._ The specs were written on the recommendations in rounds 1 and 3. I checked the normative text of FMT-BREAK-017 (positional member, 7b), FMT-BREAK-042 (1), FMT-BREAK-043 (7), FMT-INDENT-006 (6, positional sentence), and FMT-INDENT-026 (8, `case` label). Each states option A and has no conditional wording.

Change docs under `sdd/changes/2026-11/` — each answer recorded in *Decisions*, `Status: Accepted`:
- `base-list-colon-wrap.md` — Q1 A (join onto the colon's line), Q3 A (comma moves behind its base type).
- `pattern-closing-delimiter-comment.md` — Q6 A, for the list pattern and the parenthesized pattern.
- `positional-pattern-layout.md` — Q7 A (layout, `(` joins its type, comma moves), Q7b A (FMT-BREAK-017). The conditional FMT-INDENT-006 entry ("holds only if the layout is taken") now records that the sentence stays.
- `pattern-anchor-behind-wrapper.md` — Q8 A, including the patterns a `case` label introduces.

In *Alternatives considered*, an option the maintainer was shown reads "rejected by the maintainer". An alternative the maintainer was not shown reads "not taken".

## Examples for the tests

Unchanged from `03-spec-author.md`.

## Output questions

_None._

## Internal choices

_None._

## Follow-up candidates

_None new._

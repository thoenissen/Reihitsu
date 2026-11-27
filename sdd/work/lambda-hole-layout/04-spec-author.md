Role: spec author
Commit: 1b9626c
Verdict: change doc written (answers recorded, all three accepted)

The maintainer's answer, relayed by the lead: "as recommended" — #1568 B, #1698 A, #1567 A.

## Changed requirements

None in this step. The specs were already written on the recommended options and match the answers:
- #1568 B — FMT-INDENT-031: the block aligns under the anchor (`async`, else `delegate`) wherever the anonymous method stands.
- #1698 A — FMT-INDENT-006: a comment in front of the closing brace of an interpolation of a regular, verbatim, or single-line raw string takes the brace's column, also when the hole's content stands in another column; the blank line above it stays (FMT-BLANK-011). Multi-line raw holes are unchanged.
- #1567 A — FMT-BREAK-045 joins a lambda's explicit return type and its `(`; FMT-BREAK-011 names it.

Change docs: `Status: Accepted`; each `Decisions` section opens with `### Maintainer answer (2026-11-27)`, which names the options asked and the one chosen; "rejected in the recommendation" under *Alternatives considered* now reads "rejected by the maintainer's answer".

## Examples for the tests

As in 01 and 03: FMT-INDENT-031, FMT-INDENT-006, FMT-BREAK-045.

## Output questions

_None._

## Internal choices

_None._

## Follow-up candidates

Of the conditional candidates in `00-lead.md`, the B-branch of #1568 (anonymous-method counterpart of RH5205 as a new rule; RH5111 initializer column) and the A-branch of #1567 (RH5105 and a lambda's `(` behind a return type) now apply; the A-branch of #1568 does not.

# block-wrap-comment-layout

| | |
|---|---|
| Date | 2026-11-01 |
| Run | issue #1490 |
| Pull request | #1535 |

## Questions to the maintainer

| # | Kind | Question in one line | Answer | As recommended |
|---|---|---|---|---|
| 1 | Output question | Braced switch section: keep the gap in front of a trailing comment as written? | A — keep it, no extra logic | Yes (after a clarifying round: the maintainer read the four spaces as indentation the formatter adds) |
| 2 | Output question | Braced switch section: keep the comment layout around the terminal `break`? | A — keep it | Yes |
| 3 | Output question (audit) | Braced switch section: keep a blank line between the last leading comment and its statement? | A — keep it | Yes |

## Cost

Two output-question stops before the test implementer (one of them a clarification of question 1), one at the audit. Two repair rounds: round 1 a spec-header code-owner gap found by the spec checker; round 2 the audit's undecided blank line (answer 3) plus an inaccurate code comment. The audit of round 2 found the same comment drifting the other way, resolved as a comment-only change proven text-only. Most expensive stages: the two audits and the first implementer run.

## Decisions taken

Eleven. Worth the maintainer's eye: dropping the issue's "directive guards shrink" goal (filed as #1537), and introducing `BlockWrapTriviaUtilities` as the single carrier for the `if`/`else` and switch wraps.

## Suggestions

- Output question 1 was misread because the example input used four spaces without saying they are the author's, not the formatter's. An output question whose input carries unusual whitespace could say so in one line under the input.
- The first audit's blank-line finding (answer 3) was a member of the same class the skeptic enumerated for answers 1 and 2 (layout the wrap carries); the skeptic's enumeration listed the blank line *between* comments but not the one *behind* the last comment. The failure-class catalog could carry a matrix for "trivia carried across an inserted brace": per place (label, first statement, last statement, terminal statement) × per layout (gap, same-line pair, blank line between, blank line behind, multi-line comment, documentation comment).
- Code comments that restate what a shared helper keeps drifted twice in one run; pointing to the helper instead could be a general convention.

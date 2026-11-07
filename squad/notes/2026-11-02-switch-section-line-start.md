# switch-section-line-start

| | |
|---|---|
| Date | 2026-11-02 |
| Run | issue #1495 |
| Pull request | #1558 |

## Questions to the maintainer

| # | Kind | Question in one line | Answer | As recommended |
|---|---|---|---|---|
| 1 | output question | Sections behind `return` / `throw` / `goto` / `continue` start their own line | A | yes |
| 2 | output question | A section on a `break`'s line gets its blank line in the first run | A | yes |
| 3 | output question | First statement moves behind its labels; a trailing `/* */` stays on the label line | A | yes |
| 4 | output question | Labels sharing a line are stacked | A | yes |
| 5 | output question | An empty first statement stays where it is | A | yes |
| 6 | output question | A `/** */` between label and first statement moves with the statement | B | yes |
| 7 | output question | RH5417 / RH5403 / RH3208 fix results follow the new layout | A | yes |
| 8 | output question (standing instruction) | A `/** */` between two labels moves with the following label | recommended | yes |
| 9 | output question (standing instruction) | A comment run starting with `/** */` moves with the token as written | recommended | yes |

With the first batch the maintainer gave a standing instruction: every later output question of the run takes the lead's recommendation, with a summary at the end. Questions 8 and 9 came from the first audit and were decided that way.

## Cost

- Stops: one output-question batch (7 questions).
- Repair rounds: two. Round 1 resolved four audit findings (an undecided label output, spec vs code on which comments move, missing braced-section comment tests, a garbled summary). Round 2 was a comment-only repair that became a round only because `verify-text-only.sh --strict-docs` treats a `public` member of an `internal` type as public API.
- Roles spawned more than once: spec author three times (draft, answers, round 1), test implementer three (tests, two disputed tests, round 1), cleaner three, spec checker three, auditor three; skeptic, implementer, validator, and issue manager once.
- Most expensive: the placement of `/** */` comments around labels and first statements, which needed a later output question and a spec restatement after the first audit.

## Decisions taken

15 rows. The ones the maintainer may want to see: row 2 (Done-when 2's first half dropped; the blank-line phase still writes both line breaks behind a `break` on the same line until the phase reorder), rows 5, 6, and 12 (a narrow next-token check in `SwitchCaseBraceRewriter.AddBraces`, including an adjacent fix for a blank line before the switch's `}` that only the first run wrote), and row 15 (a comment-only change run as a repair round because of `--strict-docs`).

## Suggestions

- `verify-text-only.sh --strict-docs` flags documentation on `public` members of `internal` types as public API documentation. That turned a comment fix into a repair round here, and the 2026-11-01 blank-line run hit the same case. The check could consider the containing type's accessibility.
- When the maintainer answers an output question about one token kind (here a first statement), the spec author tends to extend the answer to sibling kinds (labels) in the same change. Asking the sibling cases in the same batch would have avoided audit finding F1.

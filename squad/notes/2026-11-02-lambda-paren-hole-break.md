# lambda-paren-hole-break

| | |
|---|---|
| Date | 2026-11-02 |
| Run | issue #1494 |
| Pull request | #1565 |

## Questions to the maintainer

| # | Kind | Question in one line | Answer | As recommended |
|---|---|---|---|---|
| 1 | output question | Anonymous method in a multi-line raw-string hole: keep on one line (issue's proposal) or break and align like a block lambda | B (break and align) | yes |
| 2 | output question | Blank line in front of a parenthesized lambda that starts its line is kept | A | yes |
| 3 | output question | A lambda starting a hole line no alignment places keeps its column | A | yes |
| 4 | output question | `static` anonymous method aligns under `delegate` | B | yes |
| 5 | output question | Lambda elements in initializers / wrapped collection expressions laid out like any other element | A | yes |
| 6 | output question | `async` anonymous method aligns under `async` | B | yes |

With answer 1 the maintainer added a general rule: code in an interpolation hole is formatted as anywhere else.

## Cost

- Stops: three output-question batches (1–3; 4–5; 6). Batch 3 existed only because the lead's routing message extended answer 4 to `async` on its own; the lead then asked it as a question.
- Repair rounds: two, plus one structural change (not counted) and one text-only fix after the last passing audit.
- Audits: four. Three of them found members of one family: hole lines that do not move with the literal (comment end lines, verbatim continuation lines) and the anchors placed on them — first the hole that owns a line (round 1), then the column floor before the move (round 2), then a zero-delta alignment not recorded as placed (structural). Block lambdas had all three defects on `main` already.
- Roles spawned more than once: spec author 7, test implementer 6, implementer 4, cleaner 4, spec checker 5, auditor 4; skeptic, validator and issue manager once.
- Most expensive: the unmoved-hole-line family, enumerated one mechanism per audit.

## Decisions taken

19 rows. Worth the maintainer's eye: row 10 (an anonymous method in a string nested in a raw hole aligns, decided under answer 1 and the "same formatting" rule), rows 12–15 (block lambdas on unmoved hole lines change output too — `main` was not idempotent or not placed there; FMT-INDENT-028 already demanded the new output), row 8 (an unused test-helper cause kept because removing it breaks the build or needs a suppression).

## Suggestions

- The skeptic's premise check stopped at "which constructs no alignment places". For raw-string holes, a matrix of hole-line kinds (moved, comment end line, verbatim continuation, nested literal) × anchor column relative to the shift (left, equal, statement column) would have found the three later audit findings up front. `failure-classes.md` could carry that matrix.
- A lead's routing message is a place where it can silently decide an output (here `async`). The playbook could require routing messages to quote the maintainer's answer and nothing beyond it.

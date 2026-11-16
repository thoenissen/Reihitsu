# rh5102-wrap-behind-paren

| | |
|---|---|
| Date | 2026-11-15 |
| Run | issue #1755 |
| Pull request | #1778 |

## Questions to the maintainer

| # | Kind | Question in one line | Answer | As recommended |
|---|---|---|---|---|
| 1 | output question (answered on the issue before the run) | RH5102: is an argument list whose arguments all stand on one line, below `(`, reported? | A — not reported, as for RH5109 | yes |

## Cost

No stops: the only output question was answered on the issue before the claim. No repair round: the spec checker's one finding (change-doc status) and the auditor's one text finding were text-only, repaired by the continued spec author and proven with `verify-text-only.sh`. The spec author was continued four times for small text amendments (skeptic corrections, `Uses` rows, status, audit sentence). One structural follow-up after the implementer: the shared multi-line helper for RH5102/RH5109 (decision 8) plus its Core tests. The expensive parts were the skeptic and the auditor, both proving that the change only removes diagnostics.

## Usage

Session `364f4f9b-e76b-563a-a2e9-d96225908aa3`. Responses counted once per message id. Prices are not applied; compute a cost from the tokens and the model.

| Agent | Spawn | Agent ID | Description | Model | Effort | Prompts | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| lead (main session) | 1 | - | - | claude-opus-5-5 | medium | 15 | 49 | 123 | 18.748 | 2.701 | 7.580.286 | 128.194 |
| spec-author | 1 | a94d24d34244dd679 | Spec author for #1755 | claude-opus-5-5 | medium | 6 | 29 | 65 | 23.803 | 5.745 | 2.986.459 | 557.247 |
| skeptic | 1 | a55216e4b29a2d43b | Skeptic for #1755 | claude-opus-5-5 | high | 1 | 40 | 82 | 39.301 | 21.310 | 5.248.407 | 177.088 |
| test-implementer | 1 | a42a4574b83041dbf | Test implementer for #1755 | claude-opus-5-5 | medium | 2 | 40 | 82 | 38.346 | 7.387 | 5.273.741 | 289.461 |
| implementer | 1 | a60a5261b8f5faeea | Implementer for #1755 | claude-opus-5-5 | medium | 2 | 25 | 52 | 11.349 | 1.038 | 2.413.229 | 64.834 |
| cleaner | 1 | a8277c0166ba05359 | Cleaner for #1755 | claude-sonnet-5-5 | low | 1 | 6 | 12 | 1.373 | 39 | 392.741 | 81.097 |
| spec-checker | 1 | a5f0317190045b396 | Spec checker for #1755 | claude-sonnet-5-5 | medium | 1 | 7 | 14 | 3.638 | 689 | 543.661 | 103.880 |
| auditor | 1 | a1f70243291217628 | Auditor for #1755 | claude-opus-5-5 | high | 1 | 40 | 80 | 27.867 | 14.354 | 5.758.591 | 188.753 |
| validator | 1 | a9b96410de7c107c8 | Validator for #1755 | claude-haiku-5-5 | low | 2 | 7 | 14 | 3.253 | 404 | 392.576 | 161.479 |

| Model | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|
| claude-haiku-5-5 | 7 | 14 | 3.253 | 404 | 392.576 | 161.479 |
| claude-opus-5-5 | 223 | 484 | 159.414 | 52.535 | 29.260.713 | 1.405.577 |
| claude-sonnet-5-5 | 13 | 26 | 5.011 | 728 | 936.402 | 184.977 |
| **Total** | 243 | 524 | 167.678 | 53.667 | 30.589.691 | 1.752.033 |

## Decisions taken

13. Worth the maintainer's eye: decision 2 (change-doc entries 2–4 — `M(1, 2⏎)`, `M(1, 2 // c⏎)`, a comment or `#pragma` keeping the first argument off the `(` line — taken as covered by answer A rather than asked), decision 4 (not re-asking although the issue described option B's "before" wrongly; the fix joined `M(⏎1, 2)`, it did not split it), and decision 8 (RH5102 and RH5109 now share `SyntaxNodeUtilities.IsPartlyWrapped`).

## Suggestions

- The issue's "before" output was derived from reading code, and its "Done when" named a shape (`M(1,⏎2)`) as reported that is not. An issue that carries an output question could state its "before" from a run (`scripts/apply-fix.sh`) so the maintainer answers on true outputs; here the skeptic caught it, and the answer held.

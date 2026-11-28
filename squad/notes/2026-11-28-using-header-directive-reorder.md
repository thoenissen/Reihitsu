# using-header-directive-reorder

| | |
|---|---|
| Date | 2026-11-28 |
| Run | issue #1971 |
| Pull request | #1996 |

## Questions to the maintainer

| # | Kind | Question in one line | Answer | As recommended |
|---|---|---|---|---|
| 0 | output question | (in the issue, before the run) Reorder a block whose every directive stands in front of its first using? | A | yes |
| 1 | output question | RH7207 reports and fixes a missing group blank line in a header-led block | A | yes |
| 2 | output question | Comment header, blank line, `#nullable` in front: reorder, both stay on top | A | yes |
| 3 | output question | Comment directly below the header directive stays on top | A (after an ask-back) | yes |
| 4 | output question | Comment after a blank line below the header directive: moves with its using, or stays on top | B (stays on top) | no |
| 5 | output question | Conditional group in front of the block: reorder, or keep without a fix | B (keep, "don't make it complicated") | no |
| 6 | 10 | Declare a directive inside a using block unsupported | A (no) | yes |
| 7 | output question | Conditional group enclosing the block | B (no fix) | yes |

## Cost

Three stops for the maintainer: batch 1 (questions 1–6), an ask-back on 3/4 (the maintainer proposed inserting a blank line for 3 and asked about header-like comments), and question 7, which the spec author found while carrying 5B into the spec. The spec author ran three times (initial, amendment for batch 1, continued for answer 7). No repair rounds. The test implementer was the most expensive spawn (it ran the four test projects on a throwaway worktree to find flipping tests).

## Usage

Session `cc73180a-2609-5f7e-aa5c-21359c6cc7e1`. Responses counted once per message id. Models are named by tier (large, medium, small), as `.claude/agents/README.md` assigns them. Prices are not applied; compute a cost from the tokens and the tier.

| Agent | Spawn | Agent ID | Description | Tier | Effort | Prompts | Responses | Input | Output | of which thinking | Cache read | Cache write | Context growth |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| lead (main session) | 1 | - | - | large | medium | 14 | 53 | 131 | 22.196 | 4.536 | 6.092.749 | 112.553 | 82.323 |
| spec-author | 1 | a86743313ec3ecb5c | Spec author for #1971 | large | medium | 1 | 48 | 96 | 42.297 | 17.894 | 4.246.860 | 132.553 | 108.120 |
| skeptic | 1 | a10f8f5e1100ac546 | Skeptic for #1971 | large | high | 1 | 36 | 72 | 51.016 | 31.418 | 3.329.629 | 152.400 | 134.873 |
| spec-author | 2 | a3ba976129edf4f06 | Spec author amendment #1971 | large | medium | 2 | 39 | 80 | 35.499 | 9.602 | 2.854.767 | 101.284 | 82.884 |
| test-implementer | 1 | af935ce1941934f6c | Test implementer for #1971 | large | medium | 1 | 79 | 158 | 64.138 | 15.675 | 10.916.282 | 205.079 | 186.808 |
| implementer | 1 | aab9c8c6706765e36 | Implementer for #1971 | large | medium | 1 | 28 | 56 | 12.886 | 1.140 | 1.893.454 | 170.613 | 67.520 |
| cleaner | 1 | a83cdc98907b64184 | Cleaner for #1971 | medium | low | 1 | 6 | 12 | 1.422 | 0 | 181.979 | 40.614 | 16.794 |
| spec-checker | 1 | ac2c6f53c26f8f9dc | Spec checker for #1971 | medium | medium | 1 | 10 | 20 | 3.868 | 416 | 383.689 | 56.680 | 33.307 |
| auditor | 1 | a0719e44010b5405f | Auditor for #1971 | large | high | 1 | 37 | 74 | 35.970 | 21.079 | 3.962.667 | 164.069 | 140.090 |
| validator | 1 | a70d634869405c0c7 | Validator for #1971 | small | low | 1 | 12 | 24 | 5.170 | 1.055 | 417.518 | 86.676 | 24.949 |

| Tier | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|
| large | 320 | 667 | 264.002 | 101.344 | 33.296.408 | 1.038.551 |
| medium | 16 | 32 | 5.290 | 416 | 565.668 | 97.294 |
| small | 12 | 24 | 5.170 | 1.055 | 417.518 | 86.676 |
| **Total** | 348 | 723 | 274.462 | 102.815 | 34.279.594 | 1.222.521 |

## Decisions taken

Four: reading "after its usings" as "after the first using" (a directive behind the block keeps its fix), not asking case 3 (`#region` around the block), taking the skeptic's spec-wording suggestions, and limiting answer 4B to blocks with a header directive.

## Suggestions

Answer 5B was given for a conditional group in front of the block, but its option text ("disabled text counts as unsafe") and the maintainer's intent ("don't make it complicated") gave two different boundaries, which cost question 7. When an option draws a boundary, the question could show the neighbouring shape on each side of it (here: a group ending in front of the block, and one enclosing it) in the same batch.

# Fix the layout model's column for comment-led lines in the run that folds raw-string alignment into indentation

| | |
|---|---|
| Date | 2026-10-25 |
| Escalation | 1, 12, 5 |
| Run | issue #1463 |

## Question

The skeptic found that on a line led by a `/* */` or `/** */` comment, the layout model places the first token at the layout column while `IndentationRewriter` places the comment there (architecture finding D2), and that FMT-RAW-005 does not decide where the folded indentation phase moves a raw string's lines. Options: A — measure the opening quotes in the indented tree as the phase's last step and file D2 as a follow-up (no output changes, the shift stays computed twice); B — fix D2 in this run, structure first, and move raw-string lines by the model's single shift (six stable but wrong outputs change to what FMT-INDENT-012, -029 and -030 require; wider scope); C — do only the tab half and keep `RawStringAlignmentPhase` separate; D — stop the run. A second question about four follow-up candidates was put at the same time.

## Answer

B. The maintainer accepts the wider scope. The follow-up candidates are discussed once the pull request is ready. The maintainer also wants a follow-up issue that reviews `IndentationPhase` as a whole, with a possible rewrite in mind, to be drafted with the other follow-ups.

## Reason

The maintainer asked for an explanation of the defect first; after it, the structural fix in this run was preferred over keeping two computations of the raw-string shift.

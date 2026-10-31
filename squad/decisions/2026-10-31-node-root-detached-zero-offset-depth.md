# The depth of a node-in-document target whose first token stands behind a multi-line comment

| | |
|---|---|
| Date | 2026-10-31 |
| Escalation | 1 |
| Run | issue #1489 |

## Question

FMT-PIPE-017 gives an offset of zero when the target's first token stands behind the last line of a multi-line comment, so the depth reaches the output (15 outputs in the skeptic's corpus; an inner `if` behind `/* note⏎ */` as the body of `if (a)` moves from columns 8/12 to 12/16). (A use the new depth rule here too and list the outputs as changed; B keep Core's count for this case; C change FMT-PIPE-017's anchor)

## Answer

A — the new depth rule applies here too; the outputs are listed as changed.

## Reason

None given beyond accepting the recommendation.

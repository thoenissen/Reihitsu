# Third change-doc amendment, BOM column, and tab width in front of a node-level target

| | |
|---|---|
| Date | 2026-10-25 |
| Escalation | 2, 1 |
| Run | issue #1463 |

## Question

1. Approve the change doc as amended in `18-spec-author.md` (FMT-PIPE-017: node-level entry points lay out the node from its document column and move no line after the last phase; RH5303-006/RH5304-005 retired; FMT-RAW-007 and FMT-INDENT-006 for the hole comment line; the `AGENTS.md` sentence)? A approve, B approve with changes, C reject.
2. Audit finding 4, a raw string on a line led by U+FEFF: A — treat the BOM as zero-width everywhere and change RH5202; B — restore the earlier output by counting the BOM as one column in the raw-string move.
3. A tab in front of a node-level target that does not start its line: A — count it as four columns (FMT-TAB-001); B — count characters outside the target.

## Answer

1A, 2B, 3A — taken by the lead as its recommended options under the maintainer's standing instruction of 2026-10-25 (`2026-10-25-phase-prediction-removal-audit-1.md`); the maintainer reviews all such decisions at the end of the run. Checkpoint before this decision: b40c2fb.

## Reason

1: the amendment only states what decision 1B and 2A require. 2: the approved change doc allows only the output changes it lists, and B keeps RH5202 and the formatter in agreement without a new convention. 3: consistent with FMT-TAB-001, and the alternative would reintroduce two tab widths.

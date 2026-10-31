# Approve the change doc that makes every formatter phase return a round-tripping tree that keeps foreign annotations

| | |
|---|---|
| Date | 2026-10-31 |
| Escalation | 2 |
| Run | issue #1488 |

## Question

Approve What and Why of `sdd/changes/2026-11/phase-tree-round-trip.md`: FMT-PIPE-021 (a phase's text parses back to the same tree, with the owner of every comment, directive, and disabled-text span kept), FMT-PIPE-022 (phases keep annotations they do not own; the raw-string move edits tokens instead of reparsing), FMT-STRUCT-026 (structural transforms write only the separators the text needs, the `=>` spaces of the accessor and get-only-member transforms included, the field split as a recorded exception), FMT-BREAK-029 (residue whitespace before an inserted line break is left to FMT-CLEAN-001), and two final-output changes toward FMT-PIPE-013. (A approve as written; B approve with named changes)

## Answer

A — approved as written.

## Reason

None given beyond accepting the recommendation.

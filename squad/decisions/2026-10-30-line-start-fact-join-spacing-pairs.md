# The join keeps its tight-pair table for token pairs no spacing rule covers

| | |
|---|---|
| Date | 2026-10-30 |
| Escalation | 12 |
| Run | issue #1487 |

## Question

After the join stopped asking `SpacingPolicy`, it still decides the gap of token pairs no spacing rule covers with its own table (`ListElementInteriorJoinRewriter.IsTightBefore`/`IsTightAfter`), because the horizontal-spacing phase keeps such a gap as it finds it. (A keep the table in this run and draft a follow-up that gives `SpacingPolicy` rules for those pairs; B add those rules in this run, changing FMT-SPACE-002 behavior under a further change doc; C stop the run)

## Answer

A — applied by the lead under the maintainer's standing instruction for this run to take the lead's recommendation. B would change horizontal-spacing behavior that no issue of this run asks for.

## Reason

Delegated; the recommendation's reason: the remaining split changes no output, and closing it is a separate behavior change.

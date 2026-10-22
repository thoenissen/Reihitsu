# How does one squad run implement several issues?

| | |
|---|---|
| Date | 2026-10-22 |
| Escalation | 1 |
| Run | issue #1403 |

## Question

The issue leaves four design points of a multi-issue run (`/squad #a #b #c`) to the maintainer:

1. Role agents: one agent per role for the whole batch, with one section per issue in its handover; or one agent per role per issue.
2. One issue fails (the skeptic says no, it does not reproduce, a blocking question): only that issue drops out while the others continue, with escalations batched into one question; or the whole run stops.
3. Lane: the run claims the narrowest lane that admits all issues, widening along the existing orders, and escalates before claiming when no lane does; or every issue must have the same lane.
4. Size and change docs: a soft limit of eight issues (more needs the maintainer's consent), escalation 5 measured against the whole batch, and one change doc may cover several issues of the same cause, otherwise one per issue; or no limit and one change doc per issue.

## Answer

1. One agent per role for the whole batch; each handover carries one section per issue.
2. The whole run stops: an escalation for any one issue halts role work for every issue until the maintainer answers.
3. The narrowest lane that admits all issues, widening along the existing orders; when no lane admits them, the lead escalates before claiming.
4. A soft limit of eight issues; escalation 5 applies to the batch as a whole; one change doc may cover several issues that share a cause, otherwise one per issue.

## Reason

none given

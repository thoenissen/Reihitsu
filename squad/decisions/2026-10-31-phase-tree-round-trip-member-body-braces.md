# Member-body braces placed by two line-break rewriters

| | |
|---|---|
| Date | 2026-10-31 |
| Escalation | 12 |
| Run | issue #1488 |

## Question

`LineBreakBlockRewriter.VisitBlock` places the braces of every block, member bodies included, and `DeclarationBraceLineBreakRewriter` places the braces of member bodies and event accessor lists again. The issue asked for one path; the approved change doc left it undecided. Options: A fix narrowly and file a follow-up; B make one path place member-body braces in this run, with a change-doc amendment and no final-output change; C stop the run.

## Answer

B — one path places member-body braces in this run; the change doc is amended accordingly and the final output must not change.

## Reason

None given beyond accepting the recommendation.

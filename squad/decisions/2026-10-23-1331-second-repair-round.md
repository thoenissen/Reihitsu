# Second repair round for the auditor's findings (#1331)

| | |
|---|---|
| Date | 2026-10-23 |
| Escalation | 7 |
| Run | issue #1331 |

## Question

The run had used its repair round on the spec checker's findings. The auditor then reported seven findings, all introduced by the PR and none a new question about intended behavior: SHARED-DIAG-004's per-part paragraph lost the RH7104–RH7106 limit of P28 and now binds RH2004 on partial members; the SHARED-DOC-004 fix sentence also covers the RH7103 move fix that SHARED-ORDER-002 makes carry the comment along; plus a recaption, a title, a non-goal wording, three test summaries, and duplicated test helpers. Options: run a second repair round on all seven; repair only the high findings; stop and report.

## Answer

Run a second repair round on all seven findings; afterwards the cleaner, the spec checker and the auditor run again on the repair only, followed by the validator.

## Reason

none given.

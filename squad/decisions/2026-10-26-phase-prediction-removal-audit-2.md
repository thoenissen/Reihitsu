# Leading-trivia restore of the context entry point after the second audit

| | |
|---|---|
| Date | 2026-10-26 |
| Escalation | 12, 5 |
| Run | issue #1463 |

## Question

1. Audit finding #8 repeats the class of first-audit finding #1: `FormatNodeInDocumentWithContextAsync` restores the target's original leading trivia after the last phase even when the target starts a later line of its context, so the line moves back while the lines aligned to it stay (RH5111 on `int a = 1,` / `b =` / `Other(1,` / `2);` leaves `2` misaligned and RH5203 reports it), against FMT-PIPE-017. Options: A — limit the restore to the leading trivia FMT-PIPE-010 keeps (a target that does not start its line, or a direct child of the root), so no step after the last phase moves a line; B — write the exemption into FMT-PIPE-017 and record the RH5111 case as a known gap; C — stop the run.
2. Audit finding #10: the public `Reihitsu.Core.DocumentationCommentUtilities.ShiftContinuationExteriorMarkers` lost its only production caller in this run. Options: A — keep it and present its removal as a follow-up (a public API change in the shared Core assembly); B — remove it and its test in this run.

## Answer

1A, 2A — taken by the lead as its recommended options under the maintainer's standing instruction of 2026-10-25; the maintainer reviews them at the end of the run. Audit finding #9 (dead line-offset mapping and clone in the indentation phase) is repaired in the same round. This is repair round 3 of 3. Checkpoint before this decision: 504310f.

## Reason

1: A removes the last step after the pipeline that decides a line's column, which is what decision 1B asked for; B would keep the class open. 2: removing public API is scope (escalation 5) that this run does not need.

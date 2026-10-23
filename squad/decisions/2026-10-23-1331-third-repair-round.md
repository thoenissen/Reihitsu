# Third repair round for the auditor's repair findings (#1331)

| | |
|---|---|
| Date | 2026-10-23 |
| Escalation | 7 |
| Run | issue #1331 |

## Question

After the second repair round the auditor reported two new findings in the repaired spec text. (8) The SHARED-DOC-004 sentence that a fix moving the documented declaration carries the comment with it is wider than SHARED-ORDER-002: in documentation mode `None` the RH7102/RH7103/RH7109 fixes leave the comment behind the preceding code, and the RH7309 fix leaves a comment separated from its declaration by a region directive. (9) The new partial-member paragraph of SHARED-DIAG-004 says every part writes the same accessibility modifiers, while the compiler requires only the same accessibility, so it overlaps the per-part paragraph for RH7105/RH7106. Options: a third repair round; drop the new sentences; stop and report.

## Answer

Run a third repair round: limit the SHARED-DOC-004 MUST to what SHARED-ORDER-002 carries and mark the mode-`None` and directive-separated shapes as not covered, filed as a question on the open-questions issue of `sdd/specs/shared/element-documentation.md`; reword the partial-member paragraph to the same accessibility and limit it to which accessibility a part states. Then the cleaner, the spec checker, the auditor on the repair only, and the validator.

## Reason

none given.

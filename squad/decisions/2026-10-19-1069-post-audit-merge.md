# How to check the merge of main after a passing audit (#1069)

| | |
|---|---|
| Date | 2026-10-19 |
| Escalation | 9 |
| Run | issue #1069 |

## Question

After the audit passed, `main` gained four spec batches. `sdd/specs/requirement-ids.txt` merges cleanly; one analyzer test file (RH5202) conflicts, because both sides added tests to it. Options after the merge, its conflict resolution by the test implementer, and the cleaner: run the validator and rely on CI; re-audit the merge and run the validator; or rely on CI only.

## Answer

Rely on CI only: merge, resolve the conflict keeping both sides, run the cleaner, and publish without a second audit or a local validator run on the merged tree. The run report says which checks did not run locally.

## Reason

none given.

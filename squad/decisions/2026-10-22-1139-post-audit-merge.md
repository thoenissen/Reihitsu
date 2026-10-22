# Merge main after a passing audit (#1139)

| | |
|---|---|
| Date | 2026-10-22 |
| Escalation | none (lead playbook step 5: main moved after a passing audit) |
| Run | issue #1139 |

## Question

After the repair audit passed and the validator ran green, `main` gained the P15–P22 shared-spec change, whose only overlap with this PR is `sdd/specs/requirement-ids.txt`. Options: merge main and re-audit, or merge main and rely on CI.

## Answer

Merge main, keep both sides of the ledger, and rely on CI; no re-audit.

## Reason

none given.

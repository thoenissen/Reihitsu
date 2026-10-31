# The formatter spaces `fixed` like the other statement keywords RH6001 checks

| | |
|---|---|
| Date | 2026-10-31 |
| Escalation | 2 |
| Run | issue #1487 |

## Question

After RH6001-001 gained `fixed`, the audit found that the formatter keeps `fixed(int* p = items)` because FMT-SPACE-012 and `KeywordSpacingRule` do not list `fixed`, so formatted code raises RH6001 against RH6001-001's Why and SHARED-DIAG-010. (A add `fixed` to FMT-SPACE-012 and the formatter's keyword spacing, with a change doc; B drop `fixed` from RH6001; C keep both and record an exception to SHARED-DIAG-010)

## Answer

A — the formatter writes a space between `fixed` and `(`. The change doc for it is accepted in advance.

## Reason

None given beyond accepting the recommendation.

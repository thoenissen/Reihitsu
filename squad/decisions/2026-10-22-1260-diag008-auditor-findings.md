# Auditor findings on SHARED-DIAG-008 against further directive clauses (#1260)

| | |
|---|---|
| Date | 2026-10-22 |
| Escalation | 7 |
| Run | issue #1260 |

## Question

The auditor found further active specs contradicting SHARED-DIAG-008 (P22). (a) A conditional directive inside the judged list is reported with the fix withheld: RH5102-004, RH5109-004. Options: file as P22 instances like #1375/#1376, or list in the run report only. (b) A #region or #pragma in the judged span is not reported by RH5408-003, RH5307-001 and SHARED-JOIN-001 (formatter-delegating layout rules, the domain of P31), while DIAG-008 says those directives do not stop the report. Options: narrow the DIAG-008 wording, or file as P22 instances.

## Answer

(a) File them as P22 instances in the open-questions issues of RH5102 and RH5109. (b) Narrow the DIAG-008 wording: a region or pragma directive in the judged span does not by itself stop the report; a rule's own requirement (for example SHARED-JOIN-001) may still exclude the shape.

## Reason

none given.

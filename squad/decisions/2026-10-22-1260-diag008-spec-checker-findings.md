# Spec-checker findings on SHARED-DIAG-008 against existing directive clauses (#1260)

| | |
|---|---|
| Date | 2026-10-22 |
| Escalation | 4 |
| Run | issue #1260 |

## Question

The spec checker found active clauses that keep a report while a conditional directive lies in the judged span, contradicting SHARED-DIAG-008 (P22): RH5111-005 (declarator-to-equals gap) and SHARED-ATTR-011/-019/-020 with the RH55xx specs using them (attribute lists). It also asked whether the parenthesized operand of RH3001-006 is a judged span, and whether RH7003-001's no-conditional-directive clause contradicts the DIAG-008 RH7003 example. Options for the first: file as P22 instances or amend those specs here; for RH3001: not judged span or judged span; for RH7003: leave as is, amend RH7003-001, or drop the example.

## Answer

File RH5111-005 and the SHARED-ATTR clauses as P22 instances in the open-questions issues of their specs, leaving specs and tests unchanged in this PR. RH3001's parenthesized operand is not part of the judged span; nothing changes. RH7003-001 stays as is: its clause only narrows the requirement and does not contradict the DIAG-008 example.

## Reason

none given.

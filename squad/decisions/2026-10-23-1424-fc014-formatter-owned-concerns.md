# How far FC-014 narrows

| | |
|---|---|
| Date | 2026-10-23 |
| Escalation | 5 |
| Run | issues #1423, #1424 |

## Question

The run narrowed FC-014 and the FC-014 sentence of skeptic check 3 to layout analyzers, as the run's prompt asked. The audit showed that the formatter also owns concerns that are not layout — using order, expression bodies, trailing commas, braces of empty types, field splitting — so a new rule such as "no trailing comma in object initializers", which the formatter keeps, would leave permanent diagnostics and fall into no failure class. Options: 1A keep FC-014 to layout analyzers and only correct its stated reason; 1B define FC-014 by what the formatter judges, excluding only the order or content it does not judge (member order, comment text), as SHARED-DIAG-010's `Why:` does.

## Answer

1B. FC-014 covers a new rule whose concern is of a kind the formatter judges but no formatter phase owns; only order or content the formatter does not judge, such as member order and comment text, is excluded, citing SHARED-DIAG-010. Check 3 carries the matching sentence.

## Reason

none given

# Second repair round after the auditor's findings

| | |
|---|---|
| Date | 2026-10-17 |
| Escalation | 7 |
| Run | issue #978 |

## Question

The auditor blocked with five findings after the run's one repair round, which had gone to a spec-checker finding (neutral example names):

1. (high) For some names the shared casing spec covers, the fix produces a name the rule still reports, which breaks SHARED-FIX-001. Examples: `__1st` → `_1st`, `_注文` → `注文`, and `_2x` → `I2x` → `II2x`.
2. (high) RH4009-001 says invalid JSON disables the rule, but a file with trailing content after a valid root object still activates it.
3. (medium) RH4001-006 "in the same folder" and the new SHARED-CASING-009 nested-type clause are each tested on one side only.
4. (low) "Declared directly in a namespace" should state that it includes the global namespace.
5. (low) The RH4001 examples use test-project names.

Options: a second repair round for all five; a second round for findings 1 and 2 only; publish as is, with the findings listed as follow-up work.

A second question: file finding 1 as a bug against SHARED-FIX-001, or as open questions?

## Answer

"Second repair round (Recommended)" for all five findings, and finding 1 "File as a bug (Recommended)".

## Reason

none given

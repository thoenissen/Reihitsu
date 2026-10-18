# Does the name or accessibility a fix produces count as a run-time change under SHARED-FIX-003?

| | |
|---|---|
| Date | 2026-10-18 |
| Escalation | 1 |
| Run | issue #1068 |

## Question

SHARED-FIX-003 forbids a fix whose result changes what the program does at run time. A rename is observable through `nameof`, reflection, or serialization (the RH4106 fix turns `nameof(Value)` into `nameof(_value)`), and RH2005's public-to-private fix changes what reflection sees. Options: the new name or accessibility the fix exists to produce does not count; or every observable rename or accessibility change is withheld.

## Answer

The new name or accessibility that the fix exists to produce does not count as a run-time change; any other difference in behavior does.

## Reason

none given

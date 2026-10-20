# Who owns BlankLineCodeFixUtilities (RH5030 and RH3208)? (#1067, run #997)

| | |
|---|---|
| Date | 2026-10-19 |
| Escalation | 5 |
| Run | issue #997 |

## Question

The RH5030 and RH3208 fixes both insert the separating blank line through `BlankLineCodeFixUtilities.InsertBlankLineBefore` and both decide the need through SHARED-BLANK-003; RH3208-002 restated RH5030-003's placement in its own words. Options: a new shared spec owning the placement with the helper as code owner, retiring RH5030-003; RH5030 owns the helper and RH3208 keeps restating it; or leave the helper unowned and #1067 open.

## Answer

A new shared spec states the placement of the inserted blank line, with `BlankLineCodeFixUtilities` as its code owner. RH5030 and RH3208 list it under `Uses`, RH5030-003 is retired, and its covering tests carry the new ID. This run resolves #1067.

## Reason

none given.

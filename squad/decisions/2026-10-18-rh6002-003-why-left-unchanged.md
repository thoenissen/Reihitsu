# Correct RH6002-003's Why in the spacing-spec backfill?

| | |
|---|---|
| Date | 2026-10-18 |
| Escalation | 5 |
| Run | issue #986 |

## Question

The skeptic found that the Why of RH6002-003 says the bracket rules RH6007, RH6008, and RH6012 report the spaces next to the brackets of `int[ , ]` and `Dictionary< ,>`, but no rule reports the space after `<` in `Dictionary< ,>`. RH6002 is an existing spec outside the run's scope. Options: correct the Why in this PR, or leave RH6002 unchanged.

## Answer

Leave RH6002 unchanged in this run; the run's specs do not edit another spec's requirement text.

## Reason

None given; the lead's recommendation to keep the run's scope to its own 15 specs was accepted.

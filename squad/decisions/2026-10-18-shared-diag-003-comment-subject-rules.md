# Does SHARED-DIAG-003 restrict rules whose subject is comment text?

| | |
|---|---|
| Date | 2026-10-18 |
| Escalation | 1 |
| Run | issue #1069 |

## Question

Principle P5 says a rule neither reports nor changes the text of a comment. Taken literally, rules that check comment text on purpose (RH3201, RH7301, the RH8### documentation rules) would violate SHARED-DIAG-003. Options: (A) literal reading, so those rules become bugs; (B) the requirement restricts only rules that check code, and a rule whose subject is the comment itself may check its text.

## Answer

Reading B, worded generally: a rule whose subject is the text of a comment or of a directive, such as RH3201, RH7301, or a documentation rule, checks that text as its subject; SHARED-DIAG-003 restricts only rules that check code.

## Reason

It is the same reading as SHARED-FIX-003 (thoenissen/Reihitsu#1068), which is scoped to what the rule itself reports.

# Where to file the using-directive rules that compare names by spelling

| | |
|---|---|
| Date | 2026-10-18 |
| Escalation | 1 |
| Run | issue #1069 |

## Question

RH7201, RH7203 and RH7207 compare a using directive's name by its spelling (`System . IO` is treated differently from `System.IO`), which violates SHARED-DIAG-002. The open-questions issue for `shared/using-directive-ordering.md` is closed. Options: open a new open-questions issue, reopen the closed one, or extend the existing P3 bug issue.

## Answer

None of them: the case is already filed as bug thoenissen/Reihitsu#1105 against SHARED-DIAG-002. No open-questions issue is opened; the PR lists #1105 under follow-up work, and the case stays out of the spec.

## Reason

None given beyond the existing issue.

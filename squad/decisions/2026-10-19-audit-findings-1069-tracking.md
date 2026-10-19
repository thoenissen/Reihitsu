# How to track two pre-existing defects the #1069 audit found

| | |
|---|---|
| Date | 2026-10-19 |
| Escalation | 9 |
| Run | issue #1069 |

## Question

The auditor reproduced two defects on `main` that no issue tracked. (1) RH5023 does not report blank lines inside an interpolation hole's expression, because a shared helper exempts the whole interpolated string. (2) RH5601 reports a tab in the closing-line indentation of an interpolated multi-line raw string, and its fix produces code that no longer parses. Options for each: extend an existing bug issue, open a new bug issue, or leave it untracked.

## Answer

(1) Extend thoenissen/Reihitsu#1075 with the RH5023 instance, noting the shared helper and that RH5204 must not change before thoenissen/Reihitsu#951 is decided. (2) Open a new analyzer bug issue for RH5601, drafted by the lead and approved by the maintainer before it is created. Both are listed under follow-up work in the PR; neither is fixed in this run.

## Reason

none given (both answers were the lead's recommendation).

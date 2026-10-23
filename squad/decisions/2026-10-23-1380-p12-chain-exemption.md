# P12 against the RH5112 comment exemption (#1380)

| | |
|---|---|
| Date | 2026-10-23 |
| Escalation | 3 |
| Run | issue #1380 |

## Question

The amended SHARED-COMMENT-011 (P12 clarification: a line led by a misplaced comment is reported) and SHARED-CHAIN-004 (a comment between the root and the first link keeps RH5112 from reporting; RH5048 reports the comment) give opposite answers for `var x = s⏎/* c */ .Trim();`, which is not reported today. Options: (a) P12 governs and RH5112 reports the chain, with a follow-up; (b) the rule's own comment exemption stays and SHARED-COMMENT-011 says so.

## Answer

(b): a rule's own requirement that exempts a comment in that gap still applies; SHARED-COMMENT-011 states this, and RH5112 keeps not reporting the chain.

## Reason

none given.

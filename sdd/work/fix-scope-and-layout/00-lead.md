# Lead — fix-scope-and-layout

- Branch: `claude/squad-tickets-review-eiuqp5` (the session's required branch, instead of `claude/batch-1140-...`)
- Draft PR: thoenissen/Reihitsu#1686
- Lane: **bug fix**
- Base at claim: `d6a01ee`

## Issues (prompt order)

| Issue | agent-filed | Lane reason |
|---|---|---|
| thoenissen/Reihitsu#1140 | yes | Code fixes (RH5101/5102/5110/5111/5112+5201/5113/5204/5206, plus comment instances RH5412/RH5417 and round-7a instances) format more than the code their diagnostic is about; observed vs. expected against SHARED-FIX-006 (P9). Bug fix. |
| thoenissen/Reihitsu#1141 | yes | RH6023/RH6024 fixes format more than the two gaps next to the operator; against SHARED-FIX-006, SHARED-OPSPACE-004. Bug fix. |
| thoenissen/Reihitsu#1144 | yes | Fix output not laid out as the formatter lays it out (RH5101/5102 raw string, RH5205, RH5203, plus P14b instances RH5417/RH3202 and round-7a instances); against SHARED-FIX-007 (P10). Bug fix. |
| thoenissen/Reihitsu#1145 | yes | RH5103 fix withheld behind embedded/labeled statements (guard scope) and wrong indentation into one-line blocks; against RH5103-003/004, SHARED-FIX-007. Bug fix. |
| thoenissen/Reihitsu#1094 | yes | RH5513 fix: break before attribute list, indentation in continuation lambda, tab-indented code; against SHARED-ATTR-004/008/009, SHARED-FIX-007. Bug fix. |
| thoenissen/Reihitsu#1393 | yes | RH5104 fix inserts a blank line before moved comment (RH5417 half moved to #1443 by triage, not part of this run); likely resolved on main by #1457. Bug fix. |
| thoenissen/Reihitsu#1511 | yes | RH5204 fix makes no progress on a first type behind a leading doc comment/directive; against SHARED-FIX-001. Bug fix. |
| thoenissen/Reihitsu#1561 | yes | RH5204 fix makes no progress on a doc comment in front of a switch label; against SHARED-FIX-002. Bug fix. |

All goals are proposals (all `agent-filed`).

Maintainer clarifications recorded on the issues (summarized):
- #1140: triage says bug-fix lane, no change doc; RH5111-004 and RH5401-003 are aligned to SHARED-FIX-006 as a clarification in the same PR. Further P9/P14a instances in comments (round 7a list; RH5412/RH5417 region instances of 2026-11-07).
- #1144: further instances in comments (round 7a list; RH5417 `int _x; void M() { }`; RH3202 `=> _value; private int _x;`; RH5405 `else if` brace column). The "misindented target keeps its column" instance is now specified behavior (FMT-PIPE-017) — not a defect.
- #1393: triage moved the RH5417 half to #1443; this issue keeps only the RH5104 part; likely resolved on main by #1457 — run adds the regression test and closes, or `Refs` if a defect remains.

## Roles

Bug-fix lane: skeptic (premise) → spec author (only for clarifications) → test implementer (reproductions) → implementer → cleaner → spec checker → auditor → validator. Issue manager before publishing if follow-ups exist.

## Maintainer answers

_None yet._

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name | Use the session's required branch `claude/squad-tickets-review-eiuqp5` instead of `claude/batch-1140-<slug>` | The session's environment permits pushing only to that branch | Branch names are internal | 6976d68 |
| 2 | Start | Order of skeptic and spec author in the bug-fix lane | Skeptic first (premise + reachability + reproduction status on main), spec author afterwards only for the clarifications the confirmed issues need | Several issues may already be resolved on main (#1393, #1144 RH5101/5102); clarifications for cases that do not reproduce would be wasted | Order of roles is internal | — |

## Follow-up candidates

_None yet._

## Worklist / next step

- Next: spawn the skeptic over all eight issues.

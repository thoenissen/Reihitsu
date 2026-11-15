# Lead — fix-layout-scope

- Issues (prompt order): thoenissen/Reihitsu#1140 (agent-filed), thoenissen/Reihitsu#1687 (agent-filed), thoenissen/Reihitsu#1748 (agent-filed). All goals are proposals.
- Slug: `fix-layout-scope`
- Branch: `claude/magical-edison-h81uj2` (the session environment only permits pushing to this branch; used instead of `claude/issues-1140-1687-1748-<slug>`).
- Draft PR: thoenissen/Reihitsu#1760
- Lane: **bug-fix**
  - #1140: code fixes that format more than the code their diagnostic is about, against SHARED-FIX-006 and the rules' fix requirements — observed vs. expected; no new behavior.
  - #1687: fixes that format a member leave a sibling member on the member's line, against SHARED-FIX-007 (shared line) — observed vs. expected.
  - #1748: RH5403/RH5404 fixes insert a blank line above a comment that leads the target, against SHARED-FIX-006 — observed vs. expected.
- Roles: spec author (clarification: rule requirements that restate a wider scope than SHARED-FIX-006, per #1140) → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator. Issue manager before publishing if follow-ups exist.

## Maintainer clarifications (from the issues, summarized)

- #1140: principle P9 (maintainer, #912 round 5): a code fix changes only the code its diagnostic is about; layout around it stays. Round 7a (2026-10-20): also P14a — a fix inserts no blank line above the changed node. Triage: bug-fix lane, no change doc; RH5111-004 and RH5401-003 aligned to SHARED-FIX-006 as a clarification. The issue records that an entry point laying out only changed code would be >100 lines and a new type (design question, escalation 9 territory); until decided, instances are fixed narrowly with a cheap guard or stay as on main. Two output questions are still open on the issue (RH5112 chain in `if` header; RH5101 operator spacing inside an element) — not yet answered.
- #1748: maintainer answered **A** (2026-11-15): RH5403 and RH5404 fixes insert no blank line above a comment that leads their target; leading trivia, blank lines included, stays as written.
- #1687: no maintainer answer; the issue allows treating the class as rare (input already violates RH5417) with the gap recorded in the rule's spec — the reproduction decides.

## Maintainer answers

| # | Question | Answer |
|---|---|---|
| M1 | #1748 RH5403/RH5404 blank line above a leading comment | A — no blank line inserted; leading trivia kept as written (given on the issue, 2026-11-15) |

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| D1 | Claim | Branch name | Use `claude/magical-edison-h81uj2` | Session environment forbids pushing to other branches | Internal | ed5cd61 |
| D2 | Claim | Lane | bug-fix for all three | Each reports observed vs. expected against an unchanged shared requirement | Internal | ed5cd61 |

## Follow-up candidates

(none yet)

## Worklist / next step

- Next: spawn spec author.

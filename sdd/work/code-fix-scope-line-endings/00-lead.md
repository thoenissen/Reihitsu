# Lead — code-fix-scope-line-endings

- Issues: thoenissen/Reihitsu#1283, #1284, #1285 — all three carry `agent-filed` (goals are proposals).
- Slug: `code-fix-scope-line-endings`
- Branch: `claude/issues-1283-1284-1285-code-fix-scope-line-endings`
- Draft PR: thoenissen/Reihitsu#1699
- Lane: **bug fix**, for every issue.
  - #1283: RH3006 code fix writes an LF into a CRLF document; reported against SHARED-FIX-008 (exists on `main` since #1357). Observed-versus-expected difference in shipped code → bug fix.
  - #1284: expression-body fixes (RH3202, RH3203, RH3205–RH3208) format more than the replaced body; reported against SHARED-FIX-006/-007 (P14d). Bug fix; the issue and its triage comment ask that SHARED-BODY-003 and the RH3202 layout example of SHARED-FIX-003 be reworded as a clarification (spec author runs for that).
  - #1285: RH3001 fix withheld by accident for argument / list positions, and should be withheld for constant patterns (P23). Bug fix. A comment on #1285 adds the parentheses instance (#1245 Q3, round 7b option A: parentheses only where binding needs them) — it is part of the issue.
- Blocker #1138 is closed (merged via #1357): SHARED-FIX-006/-007/-008 exist.
- Roles: spec author (clarification only: SHARED-BODY-003, SHARED-FIX-003 example, RH3202 spec, any RH3001/RH3006 spec text the instances need) → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator. Issue manager before publishing if follow-up candidates exist.

## Maintainer answers

(none yet)

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch slug has five words (`code-fix-scope-line-endings`) instead of at most four | Keep it | Already pushed with the claim; renaming costs a new claim | Internal naming | 114ae37 |

## Follow-up candidates

(none yet)

## State

- Audited SHA: —
- Last step finished: claim, draft PR, prepare.
- Next step: spawn spec author.

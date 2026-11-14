# Lead — code-fix-scope-line-endings

- Issues: thoenissen/Reihitsu#1283, #1284, #1285 — all three carry `agent-filed` (goals are proposals).
- Slug: `code-fix-scope-line-endings`
- Branch: `claude/issues-1283-1284-1285-code-fix-scope-line-endings`
- Draft PR: thoenissen/Reihitsu#1699
- Lane: **behavior change** (widened from bug fix, decision 2), for every issue.
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
| 2 | Spec author | #1285 changes RH3001-004 and adds RH3001-008, so it needs a change doc | Widen the run's lane from bug fix to behavior change (lane check confirmed) | A change doc under `sdd/changes/` requires the behavior-change lane; the roles are the same | Lane is internal; visible changes are those of the change doc | cd2ddbc |
| 3 | Spec author | RH3001-008 keyed on the constant-pattern node, so `case !true:` keeps its fix | Take it | The maintainer saw `case !true:` fixed as the contrast case in #1245 Q2; withholding it would change visible output nobody asked to change | Keeps today's output | cd2ddbc |
| 4 | Spec author | #1284 triage comment's "coverage sentence" | SHARED-BODY-003 scope plus RH3202 intro, no new requirement per rule spec | Smallest reading that meets the comment | Spec wording only | cd2ddbc |
| 5 | Spec author | #1283's "CRLF check in the fix test base" goal | Left to the test implementer to propose; lead decides on its handover | Test infrastructure is internal | Tests only | — |

## Follow-up candidates

(none yet)

## State

- Audited SHA: —
- Last step finished: spec author (01-spec-author.md, change doc `sdd/changes/2026-11/rh3001-fix-scope.md` Draft).
- Next step: skeptic; then accept change doc if the skeptic confirms #1245 decisions cover its visible output changes, else output questions.

# Lead — casing-rename-call-sites

- Issue: thoenissen/Reihitsu#1676 — casing fixes rewrite call sites to keep their binding when renaming (labels: bug, analyzer, `agent-filed`; goals are proposals).
- Slug: `casing-rename-call-sites`
- Branch: `claude/adoring-hawking-l4994s` (the session's designated branch; see Decision 1)
- PR: thoenissen/Reihitsu#1751 (draft claim)
- Lane: **bug fix** — the issue reports an observed difference between the casing fixes and an unchanged requirement (SHARED-FIX-006). Option B of the issue would instead add a spec exception; if the maintainer picks it, the lane is re-evaluated.
- Roles: skeptic → (output question) → test implementer → implementer → cleaner → spec checker → auditor → validator. Spec author: only for a clarification (e.g. recording the withheld class in the casing spec).

## Maintainer clarifications

None yet. The issue itself carries an output question (withhold the casing fix when the rename would capture a call, or keep rewriting call sites); it is asked at the batch point before the test implementer.

## Maintainer answers

(none yet)

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name | Use the session's designated branch `claude/adoring-hawking-l4994s` instead of `claude/issue-1676-<slug>` | The session environment permits pushes only to that branch | Branch naming is internal | bb8fc78 |

## Follow-up candidates

(none yet)

## Worklist / next step

- Next: spawn the skeptic (premise, reachability, failure class of the renamer's call-site rewrites).

# Lead — using-ordering-by-value

## Run

- Issues (prompt order): thoenissen/Reihitsu#1105, thoenissen/Reihitsu#1266, thoenissen/Reihitsu#1267 — all three `agent-filed` (goals are proposals).
- Slug: `using-ordering-by-value`
- Branch: `claude/upbeat-newton-0kso6f` (session-mandated branch; see decision 1). Draft PR: thoenissen/Reihitsu#1735.
- Lane: **bug fix**, all three issues.
  - #1105: reported observed-vs-expected difference against SHARED-DIAG-002 (names compared by value): RH7201, RH7203, RH7204, RH7206, RH7207 and the formatter's using phase must order/group `System . IO`, `System.IO`, `@System.IO`, and a name with a comment inside identically.
  - #1266: observed-vs-expected difference against SHARED-USING-007 (compare with the previous directive of the same group, whatever lies between): RH7203, RH7204, RH7206. Maintainer triage comment: bug-fix lane, no change doc; RH7203-001, RH7204-001, RH7206-001 are aligned to SHARED-USING-007 as a clarification in the same PR. P18 clarified: the partner is every earlier element of the same group, not only the adjacent one.
  - #1267: observed-vs-expected difference against SHARED-USING-004 (an alias whose target is not a name forms its own group after every other group of its section): RH7204, RH7207, and the formatter (`UsingGrouping`); tuple targets too; formatter idempotent on the result. Issue calls for a spec clarification.
- Roles: spec author (clarifications only: rule specs RH7203/RH7204/RH7206/RH7207 aligned to the shared spec) → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator. Issue manager before publishing when follow-up candidates exist.
- Blockers named in the issues (#1069, #1260) are closed; SHARED-DIAG-002, SHARED-USING-004 and SHARED-USING-007 carry the decided wording on `main`.

## Maintainer answers

(none yet)

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name | `claude/upbeat-newton-0kso6f` instead of `claude/issues-1105-1266-1267-<slug>` | The session environment permits pushing only to this branch | Branch names are internal | 570f2ad |

## Follow-up candidates

(none yet)

## Worklist / next step

- Claimed (570f2ad, draft PR #1735), SDK prepared.
- Spec author done (8d25ac5, 01-spec-author.md): clarification only, no change doc, no output questions; lane check confirmed bug-fix. Open: its internal choice that by-value comparison binds the formatter (formatter output changes for `@System.IO` etc.) — skeptic asked for a verdict; lead decides after 02, or asks as an output question if nothing unchanged demands it.
- Next: skeptic (running), then output-question batch (if any), then test implementer.

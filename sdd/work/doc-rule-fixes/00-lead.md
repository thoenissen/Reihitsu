# Lead — doc-rule-fixes

- Branch: `claude/squad-tickets-review-y2yb3m` (session-designated branch; replaces the playbook's naming)
- Draft PR: thoenissen/Reihitsu#1700
- Base at claim: cf6d26b03fe1917f193d412c8540916f93a00dc4
- Lane: **bug fix**

## Issues (prompt order)

| Issue | agent-filed | Lane | Why |
|---|---|---|---|
| thoenissen/Reihitsu#1345 | yes | bug fix | Triage comment: behavior already required on main by SHARED-DOC-004; RH8303-001/-005 aligned as a clarification; the body's change-doc line no longer applies |
| thoenissen/Reihitsu#1346 | yes | bug fix | Triage comment: already required by SHARED-DOC-008; rule spec text aligned as clarification |
| thoenissen/Reihitsu#1347 | yes | bug fix | Triage comment: already required by SHARED-DIAG-010; rule spec text aligned as clarification |
| thoenissen/Reihitsu#1348 | yes | bug fix | Two fixes undo each other (SHARED-FIX-004); comments add the formatter side (FMT-BLANK-010 text should name the region-directive exception; no formatter output changes) |
| thoenissen/Reihitsu#1270 | yes | bug fix | P20 / SHARED-DIAG-006: every element of its kind (RH8106, RH8203; comment adds RH8305, RH8306 instances) |
| thoenissen/Reihitsu#1092 | yes | bug fix | Triage split: only Q3b stays (partial type group from accessibility across parts, SHARED-DIAG-004); Q4 moved to #1448, out of this run |
| thoenissen/Reihitsu#1153 | yes | bug fix | Triage split: only the bug stays — no RH8032 diagnostic without a source location (`extension(long)`, SHARED-DIAG-007); P8b/P8c moved to #1449, out of this run |
| thoenissen/Reihitsu#1617 | yes | bug fix | Unconfirmed, found by reading code; must be reproduced first (SHARED-DIAG-004) |

All issues carry `agent-filed`: goals are proposals.

## Roles

Bug-fix lane: spec author (clarifications only: RH8303, RH8401/RH8402, RH8307, RH8302/FMT-BLANK-010, RH8002 coverage, as the issues' triage notes name) → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator. Issue manager before publishing if follow-up candidates exist.

## Maintainer clarifications (summarized)

- Triage comments moved #1345, #1346, #1347 and #1092 to the bug-fix lane without a change doc; the rules' own spec text is aligned as a clarification in the same PR.
- #1092 keeps only Q3b; #1153 keeps only the missing-location bug; the split parts belong to #1448 and #1449.
- #1617: the run reproduces before building anything.

## Maintainer answers

(none yet)

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name | Use the session-designated branch `claude/squad-tickets-review-y2yb3m` instead of `claude/batch-1345-<slug>` | The session environment permits pushing only to that branch | Internal | 065c044 |

## Follow-up candidates

(none yet)

## Worklist / next step

- Next: spawn spec author (01).

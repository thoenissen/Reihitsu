# Lead — fix-config-comment-guards

- Branch: `claude/squad-issues-1595-1593-1616-2mn7be` (the session's designated branch; see Decisions #1)
- Draft PR: thoenissen/Reihitsu#1662
- Base: `origin/main` 57acf0d

## Issues

| Issue | agent-filed | Lane | Reason |
|---|---|---|---|
| thoenissen/Reihitsu#1595 | yes | bug fix | RH7102 fix reorders layout fields / initializers in another build configuration; observed vs. SHARED-FIX-003 |
| thoenissen/Reihitsu#1593 | yes | bug fix | RH7309 member-order guard miscounts a member split by `#if`; observed vs. SHARED-FIX-003 / RH7309-006 |
| thoenissen/Reihitsu#1616 | yes | bug fix | RH2001 fix leaves a modifier gap, relocates/duplicates comments when reordering modifiers; observed vs. SHARED-FIX-003 / SHARED-FIX-007 |

All three are `agent-filed`: goals, "Done when", target behavior are proposals.
Maintainer triage (issue comments, 2026-11-07) on #1595 and #1593: critical, meaning changes in another build configuration; milestone 1.2.1.

## Lane

Bug fix. Roles: spec author (only for a clarification — #1616 "Done when" asks the RH2001 spec to state the outcome), skeptic, test implementer, implementer, cleaner, spec checker, auditor, validator. Issue manager before publish if follow-up candidates exist.

## Maintainer answers

(none yet)

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name | Use the session's designated branch `claude/squad-issues-1595-1593-1616-2mn7be` instead of `claude/issues-1595-1593-1616-<slug>` | The session environment requires development on that branch | Branch name is internal | claim commit |
| 2 | Lane | Spec author before skeptic? | Run the skeptic first; spawn the spec author afterwards only for clarifications the skeptic or the issues need | Bug-fix lane uses the spec author only for a clarification, which is not known before the premise check | Internal ordering | — |

## Follow-up candidates

(none yet)

## Worklist / next step

- Next: skeptic (premise check of all three issues) → handover `01-skeptic.md`.

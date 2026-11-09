# Lead — conditional-group-fix-refusals

- Branch: `claude/bold-mccarthy-t1ajly` (the session's designated branch, used instead of `claude/issues-1666-1667-1665-...`; see Decisions #1)
- Draft PR: thoenissen/Reihitsu#1677
- Base at claim: origin/main 95aa3e69893acc33c8452843c352e5846fc3cd1f
- Slug: `conditional-group-fix-refusals`

## Issues (prompt order)

| Issue | agent-filed | Lane | Why |
|---|---|---|---|
| thoenissen/Reihitsu#1666 | yes (goals are proposals) | bug fix | RH2001 fix offered where a conditional group in front of the first modifier holds part of the declaration; reported output violates SHARED-FIX-003 |
| thoenissen/Reihitsu#1667 | yes | bug fix | Member-moving fixes ignore a conditional layout attribute on another part of a partial type; violates SHARED-FIX-003 / SHARED-ORDER-007. Issue also asks SHARED-ORDER-008 to say "a declaration of the type" (clarification) |
| thoenissen/Reihitsu#1665 | yes | bug fix | Member-moving fixes ignore a conditional group in front of the moved span that closes the type; violates SHARED-FIX-003 / SHARED-ORDER-007 |

Run lane: **bug fix**. Roles: spec author (clarification of SHARED-ORDER-008 and any requirement wording the refusals need), skeptic, test implementer, implementer, cleaner, spec checker, auditor, validator; issue manager before publishing if there are follow-up candidates.

## Maintainer clarifications

- Triage comment on all three issues (2026-11-09): critical — the fix changes program meaning or breaks the build in another build configuration; milestone 1.2.1; to be done in one run together (missing cheap refusals in member-moving fixes and the RH2001 fix).

## Maintainer answers

(none yet)

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name | Use the session's designated branch `claude/bold-mccarthy-t1ajly` | Session environment requires all pushes to that branch | Internal | claim commit |

## Follow-up candidates

(none yet)

## State

- Last step: claimed, draft PR opened, SDK prepared.
- Next: spec author (01), then skeptic.

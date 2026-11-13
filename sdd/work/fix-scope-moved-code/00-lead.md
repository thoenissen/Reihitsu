# Lead — fix-scope-moved-code

- Issues (prompt order): thoenissen/Reihitsu#969, #1335, #1263, #1151, #1604 — all labeled `agent-filed` (goals are proposals).
- Slug: `fix-scope-moved-code`
- Branch: `claude/dreamy-planck-yry5n0` (session-mandated branch, see Decisions #1)
- Draft PR: thoenissen/Reihitsu#1688
- Lane: **bug fix**
  - #969: observed vs. expected — member-ordering fixes (RH7102/RH7103/RH7109) reformat the whole type against FMT-PIPE-010/011.
  - #1335: observed vs. expected — RH7004, RH7104–RH7108 fixes change code beyond the node they rewrite or move (SHARED-FIX-006/007, P14a/P26).
  - #1263: observed vs. expected — RH7303/RH7310 fixes leave a blank line after removing a region (P14c, SHARED-FIX-007).
  - #1151: observed vs. expected — RH7302 fix renames `#endregion` comments that only contain the region name (SHARED-FIX-003).
  - #1604: observed vs. expected — moved code is reformatted against SHARED-FIX-007 (moved-code bullet); proposes a shared column-delta helper (RH3004, RH7004, member-ordering).
- Roles: spec author (clarifications only: #1151 asks the RH7302 spec to state the rule; any other clarification) → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator; issue manager before publishing when follow-up candidates exist.

## Maintainer clarifications (summarized)

- Withholding a code fix is an equally valid outcome. A full implementation can always be delivered later through a follow-up issue, when it is proportionate. (Given with the `/squad` prompt.)

## Maintainer answers

(none yet)

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name `claude/batch-969-<slug>` vs. the session's mandated branch | Use `claude/dreamy-planck-yry5n0` | The session environment allows pushing only to that branch | Branch names are internal | claim commit |

## Follow-up candidates

(none yet)

## Audited SHA

(none yet)

## Open worklist / next step

- Next: spawn spec author (clarifications), then skeptic.

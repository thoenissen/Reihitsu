# Lead — region-member-rules

- Branch: `claude/optimistic-hopper-gj3m1o` (the session's mandated branch; overrides the playbook's naming)
- Draft PR: thoenissen/Reihitsu#1734
- Slug: `region-member-rules`
- Lane: **bug fix**

## Issues (prompt order)

| Issue | agent-filed | Rules | Lane reason |
|---|---|---|---|
| thoenissen/Reihitsu#1264 | yes | RH7308, RH7309, RH7311 (+ RH7401–RH7412 bases, `RegionDirectiveUtilities.GetTopLevelRegions` for enums) | Triage comment: behavior already required on main by SHARED-TYPEREGION-006; bug fix, rule spec text aligned as clarification |
| thoenissen/Reihitsu#1333 | yes | RH7403, RH7407, RH7411 | Triage comment: bug against SHARED-DIAG-006; bug fix, coverage sentence widened as clarification |
| thoenissen/Reihitsu#1334 | yes | RH7401–RH7412 | Triage comment: required by SHARED-DIAG-009; SHARED-REGGROUP-006 aligned as clarification; "that is not partial" removed from RH7401-001…RH7412-001 as clarification; body's change-doc line no longer applies |
| thoenissen/Reihitsu#1339 | yes | RH7102, RH7109 | Triage comment: required by RH7102-003 and RH7109-003; bug fix. Comment adds the input "field outside every region vs constant in a region" and asks the test `NoCodeFixWhenDirectivesAreInLeadingTrivia` to assert the diagnostic |
| thoenissen/Reihitsu#1337 | yes | RH7101, RH7105–RH7108 | Triage comment: required by SHARED-DIAG-008 and RH7101-001; RH5102-004, RH5109-004, RH5111-005 aligned as clarification; body's change-doc line no longer applies |
| thoenissen/Reihitsu#1412 | yes | RH7105, RH7106 | Reported missing diagnostic (declaring part of a partial constructor) |
| thoenissen/Reihitsu#1592 | yes | RH7309 code fix | SHARED-FIX-005: fix offered where it changes nothing |

All issues are `agent-filed`: goals are proposals.

## Roles

Run: spec author (clarifications only, as the triage comments require), skeptic (premise + reachability), test implementer (reproductions), implementer, cleaner, spec checker, auditor, validator. Issue manager once before publishing if there are follow-up candidates.
Skipped: none of the bug-fix lane's roles.

## Maintainer clarifications

None beyond the triage comments above, which are the maintainer's (account) re-triage of lane: all seven run without a change doc; where a rule's spec text disagrees with a shared requirement, the PR aligns it as a clarification.

## Maintainer answers

(none yet)

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name | Use the session's mandated branch `claude/optimistic-hopper-gj3m1o` | The session environment allows pushes only there | Internal | 5f66bcd |
| 2 | Claim | Shared lane | Bug fix for all seven | Triage comments moved #1264, #1333, #1334, #1339, #1337 to bug fix without change doc; #1412, #1592 are bug fixes | Internal | 5f66bcd |

## Follow-up candidates

(none yet)

## Worklist / next step

- Done: claim (5f66bcd), draft PR #1734, prepare.sh green.
- Next: spawn spec author (clarifications), then skeptic.

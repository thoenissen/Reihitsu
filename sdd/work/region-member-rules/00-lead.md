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
| 3 | Spec author | RH5102-004, RH5109-004, RH5111-005 alignment (three more analyzers, three tagged tests flip) | In scope of #1337 | The #1337 triage comment names these three requirements; SHARED-DIAG-008 (unchanged) demands the outcome | Unchanged SHARED-DIAG-008 demands it | 187b246 |
| 4 | Spec author | Rule pages RH7308, RH7401–RH7412 still say "top-level region"/"override methods" | Correct them in this PR (spec author amendment, with the next spec round) | Rule pages must describe the analyzer; the change follows unchanged SHARED-TYPEREGION-006 / SHARED-REGGROUP-006 | Brings the page in line with requirements already in force | pending |
| 5 | Spec author | RH7308 innermost region with unrecognized label | Not checked; the outer region does not judge it (as written) | SHARED-TYPEREGION-006 + RH7308-002 | Decided by unchanged requirements | 187b246 |

## Follow-up candidates

(none yet)

## Open output questions (batch 1, before test implementer)

From 01-spec-author.md: Q1 (#1264 RH7309 regions in #if/#else branches), Q2 (#1334 partial member with only some parts in place; RH7405–RH7408 reading), Q3 (#1337 SHARED-MODORDER-003 directive between last modifier and type), Q4 (#1337 RH5111 directive inside the value).

## Worklist / next step

- Done: claim (5f66bcd), draft PR #1734, prepare.sh green, spec author 187b246 (lane check: confirmed).
- Next: skeptic (02), then batch-1 output questions, then spec amendment (rule pages + answers), then test implementer.

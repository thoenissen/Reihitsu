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

Run lane: **behavior change** (widened from bug fix, Decisions #2). Roles: spec author (clarification of SHARED-ORDER-008 and any requirement wording the refusals need), skeptic, test implementer, implementer, cleaner, spec checker, auditor, validator; issue manager before publishing if there are follow-up candidates.

## Maintainer clarifications

- Triage comment on all three issues (2026-11-09): critical — the fix changes program meaning or breaks the build in another build configuration; milestone 1.2.1; to be done in one run together (missing cheap refusals in member-moving fixes and the RH2001 fix).

## Maintainer answers

(none yet)

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name | Use the session's designated branch `claude/bold-mccarthy-t1ajly` | Session environment requires all pushes to that branch | Internal | claim commit |
| 2 | 01 spec author | Lane | Widen bug fix -> behavior change; all roles run | Spec author wrote change doc `sdd/changes/2026-11/conditional-group-fix-refusals.md` (three newly withheld fixes); lane check confirms behavior-change | Lane is internal; the visible changes go to the maintainer as output questions | 6a5f07c |
| 3 | 01 spec author | `DeclarationHoldsConditionalDirective` reads only the excluded text right behind a directive, not every excluded branch (input in 01-spec-author.md) | Take in as adjacent fix (needs a test) | Same function the #1667 fix changes; unchanged SHARED-ORDER-008 already means every branch; small | Brings code back to an unchanged requirement (withheld fix demanded by SHARED-FIX-003) | pending |
| 4 | 01 spec author | Internal choices (group in front counts as layout type; RH2001-010 reuses the whole-members parse; one change doc; declaring unsupported not an option) | Accept, pending skeptic | Recommendations checked against handover | Visible effects are the change doc's output changes, asked as output questions | 6a5f07c |

## Follow-up candidates

(none yet)

## State

- Last step: spec author 01 (change doc written, 6a5f07c). Output questions pending: 3 visible changes in the change doc (RH2001 `[Obsolete]` in excluded branch; RH7109 partial with conditional `[Serializable]`; RH7109 `#if DEBUG static #endif` in front of a member ahead of moved code).
- Next: skeptic (02), then output-question batch, then test implementer.

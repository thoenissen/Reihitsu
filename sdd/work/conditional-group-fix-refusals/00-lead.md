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
| 5 | 02 skeptic | A5: whole `partial` declaration with a layout attribute in an excluded branch in front of the reordered declaration (m06); SHARED-ORDER-008 still exempts it | Refuse; spec author amends the exemption sentence | Demanded by unchanged SHARED-FIX-003/SHARED-FIX-009; cheap check on an already-parsed branch | Withheld fix demanded by an unchanged requirement | pending |
| 6 | 02 skeptic | Architecture finding FC-015: "group wraps whole members" decided in `WrapsWholeUnits` and `DeclarationHoldsConditionalDirective`; RH2001-010 would add a third | Change structure in this run: one Core helper serving all three, no new type | The narrow fix would add a third copy; stays inside `OrderingDeclarationUtilities` (Core), which the run changes | Internal structure | pending |
| 7 | 02 skeptic | Issue goals | #1666: drop "refusal must not parse an excluded branch" (contradicts its own counterpart goal). #1665: narrow "a test per consumer asserts no fix" — RH7109/RH7309 no fix, RH7102/RH7103 keep fix (they reorder no instance field). #1667: keep, add a two-document test | Skeptic evidence; agent-filed goals are proposals | Kept fixes are today's output; no visible change beyond the change doc | 7ceac0b |
| 8 | 02 skeptic | A6: a `partial` declaration only excluded code writes elsewhere (behind the type, behind the spans, other document) | Not refused in this run; follow-up candidate (bug, safety rail on `main`), carries the question | No cheap check; the only refusal would withhold instance-field reorders on every partial type (common code); output unchanged by the run | Output identical to `main` | — |

## Follow-up candidates

- A6 (02-skeptic.md, #1667 *Breaking inputs*, *Follow-up candidates*): `partial` declaration with a layout attribute only in excluded code outside the A5 position (m07, m10, other document) — member-moving fixes still reorder fields of a type that has sequential layout in another build. File as `bug` (safety rail existing on `main`), name first in Review notes; carry the output question (keep fix vs refuse instance-field reorders on partial types).

## State

- Last step: spec author 01 (change doc written, 6a5f07c). Output questions pending: 3 visible changes in the change doc (RH2001 `[Obsolete]` in excluded branch; RH7109 partial with conditional `[Serializable]`; RH7109 `#if DEBUG static #endif` in front of a member ahead of moved code).
- Skeptic 02 done (7ceac0b): all three premises reproduce.
- Next: spec author amendment (03: A5 sentence, issue-goal narrowing in change doc), then output-question batch (3 questions), then test implementer.

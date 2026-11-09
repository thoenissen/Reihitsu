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

- Batch 1 (before test implementer): 1A, 2A, 3A — all three change-doc visible output changes as recommended: no fix offered (RH2001 attribute in excluded branch; RH7109 partial with conditional `[Serializable]`; RH7109 `#if DEBUG static #endif` in front of an uncrossed member). Matches the change doc; no amendment needed.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name | Use the session's designated branch `claude/bold-mccarthy-t1ajly` | Session environment requires all pushes to that branch | Internal | claim commit |
| 2 | 01 spec author | Lane | Widen bug fix -> behavior change; all roles run | Spec author wrote change doc `sdd/changes/2026-11/conditional-group-fix-refusals.md` (three newly withheld fixes); lane check confirms behavior-change | Lane is internal; the visible changes go to the maintainer as output questions | 6a5f07c |
| 3 | 01 spec author | `DeclarationHoldsConditionalDirective` reads only the excluded text right behind a directive, not every excluded branch (input in 01-spec-author.md) | Take in as adjacent fix (needs a test) | Same function the #1667 fix changes; unchanged SHARED-ORDER-008 already means every branch; small | Brings code back to an unchanged requirement (withheld fix demanded by SHARED-FIX-003) | 25cba35 |
| 4 | 01 spec author | Internal choices (group in front counts as layout type; RH2001-010 reuses the whole-members parse; one change doc; declaring unsupported not an option) | Accept, pending skeptic | Recommendations checked against handover | Visible effects are the change doc's output changes, asked as output questions | 6a5f07c |
| 5 | 02 skeptic | A5: whole `partial` declaration with a layout attribute in an excluded branch in front of the reordered declaration (m06); SHARED-ORDER-008 still exempts it | Refuse; spec author amends the exemption sentence | Demanded by unchanged SHARED-FIX-003/SHARED-FIX-009; cheap check on an already-parsed branch | Withheld fix demanded by an unchanged requirement | 7e019ae, 25cba35 |
| 6 | 02 skeptic | Architecture finding FC-015: "group wraps whole members" decided in `WrapsWholeUnits` and `DeclarationHoldsConditionalDirective`; RH2001-010 would add a third | Change structure in this run: one Core helper serving all three, no new type | The narrow fix would add a third copy; stays inside `OrderingDeclarationUtilities` (Core), which the run changes | Internal structure | 25cba35 |
| 7 | 02 skeptic | Issue goals | #1666: drop "refusal must not parse an excluded branch" (contradicts its own counterpart goal). #1665: narrow "a test per consumer asserts no fix" — RH7109/RH7309 no fix, RH7102/RH7103 keep fix (they reorder no instance field). #1667: keep, add a two-document test | Skeptic evidence; agent-filed goals are proposals | Kept fixes are today's output; no visible change beyond the change doc | 7ceac0b |
| 8 | 02 skeptic | A6: a `partial` declaration only excluded code writes elsewhere (behind the type, behind the spans, other document) | Not refused in this run; follow-up candidate (bug, safety rail on `main`), carries the question | No cheap check; the only refusal would withhold instance-field reorders on every partial type (common code); output unchanged by the run | Output identical to `main` | 7e019ae |
| 9 | 03 spec author | Scope of A5 refusal | Narrowed: only an excluded declaration of the same type (name, arity, `partial`) that carries an attribute list | One without attributes cannot set layout; keeps `main` output for every input not demanded | Kept fixes are today's output | 7e019ae |
| 10 | after batch 1 | Accept change doc | Accepted; Decisions section should record the answers when status changes | Skeptic ran, amendment written, maintainer answers match | Answers given | 7e019ae |
| 11 | 04 test implementer | Nested group of a whole type in front of the type's first token (`FixMovesAFieldOfATypeBehindANestedGroupOfAWholeTypeInFrontOfIt`): no-nesting clause of RH2001-010 vs none in SHARED-ORDER-008 at the type's first token | Keep the test (fix kept); the shared helper of Decision #6 applies the no-nesting clause only where a spec states it | Locks today's output, which no spec or answer changes; withholding it would be an unasked visible change | Output identical to `main` | 220f435 |
| 12 | 08 auditor (round 1) | Finding #1: RH2001-010's no-nesting half (group inside another group's branch) withholds safe fixes `main` offers (file wrapped in `#if NET`) | Drop that half for RH2001-010; keep `main`'s output there | Protects nothing for this fix; common multi-targeting shape | Keeps `main`'s output, so nothing in question | 7ef961e, c5fd203, 38bdb6d |
| 13 | 08 auditor (round 1) | Finding #2: excluded branch behind a property (`#if X = 5; #endif`) joins the declaration in another build | Take in as repair: refuse with the same shared reading on the token behind the property; spec author extends RH2001-010 | Same class as #1666 (excluded branch holds part of the declaration); cheap check with the existing helper | Withholding demanded by unchanged SHARED-FIX-003; rare class | 7ef961e, c5fd203, 38bdb6d |
| 14 | 08 auditor (round 1) | Finding #3: four rare-class withholdings not listed in the change doc | Spec author states them in the change doc as rare-class refusals outside *Visible output changes*; PR Review notes names the classes | Rare-class withholding puts nothing in question (README, What reaches the maintainer) | Rare class | 7ef961e, c5fd203, 38bdb6d |
| 15 | 08 auditor (round 1) | Finding #4: a partial declaration ending in `;` with a conditional layout attribute is not read | Repair: read such a declaration (class/struct/record forms), tests, SHARED-ORDER-008 wording | Same requirement as #1667 | Demanded by unchanged SHARED-FIX-003 | 7ef961e, c5fd203, 38bdb6d |
| 16 | 08 auditor (round 1) | Findings #5 (check order) and #6 (stale `<returns>`) | Implementer repairs both | Low, same file | Internal | 7ef961e, c5fd203, 38bdb6d |
| 17 | 09 spec author | (a) `#if !RELEASE` around a nested group and the property, `#endif` behind it, stays withheld; (b) behind-the-property check only for properties without initializer; (c) extend RH2001-010 instead of RH2001-011 | Accept all three | (a) a third nesting mode for a rare shape would grow the source (AGENTS, special cases); (b) one cheap check; (c) one test, existing tags | (a) rare class, listed in change doc *Rare-class withholdings* and PR Review notes; (b)(c) keep `main` output / internal | 7ef961e |

## Follow-up candidates

- A6 (02-skeptic.md, #1667 *Breaking inputs*, *Follow-up candidates*): `partial` declaration with a layout attribute only in excluded code outside the A5 position (m07, m10, other document) — member-moving fixes still reorder fields of a type that has sequential layout in another build. File as `bug` (safety rail existing on `main`), name first in Review notes; carry the output question (keep fix vs refuse instance-field reorders on partial types).

## State

- Last step: spec author 01 (change doc written, 6a5f07c). Output questions pending: 3 visible changes in the change doc (RH2001 `[Obsolete]` in excluded branch; RH7109 partial with conditional `[Serializable]`; RH7109 `#if DEBUG static #endif` in front of a member ahead of moved code).
- Skeptic 02 done (7ceac0b): all three premises reproduce.
- Spec author 03 done (7e019ae).
- Batch 1 answered (all A). Change doc accepted (Decisions #10).
- Test implementer 04 done (220f435): all three reproduced; 52 red tests.
- Implementer 05 done (25cba35): all green, ~51 net production lines.
- Cleaner 06 clean (5bdda45).
- Spec checker 07 consistent, no findings (fbb4c41).
- Audit 1 (08, ce0b3eb) on fce5e46: findings #1-#6. Repair round 1 started.
- Round 1: spec author 09 done (7ef961e); test implementer 10 done (c5fd203, 23 red); implementer 11 done (38bdb6d, all green). Worklist: spec author 09 (RH2001-010 for #1/#2, SHARED-ORDER-008 for #4, change doc #3) -> test implementer 10 (#1 kept, #2 refused, #4 refused, LF/CRLF) -> implementer 11 (#1, #2, #4, #5, #6) -> cleaner -> spec checker -> auditor (repair only) -> validator.

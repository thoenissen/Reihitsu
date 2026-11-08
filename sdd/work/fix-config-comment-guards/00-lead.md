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
| 3 | Skeptic | #1595 inputs use `public const` after `private` fields and do not reproduce | Use same-accessibility inputs (`private const`) and widen the tests to the skeptic's failing rows (layout: struct, record struct, Sequential class, auto-property; initializers: instance, static, property, event, `#if false`, `#elif`, crossed non-target) with its counterparts | agent-filed goal; as written the tests would pass vacuously | Only fixes SHARED-FIX-003 already requires are withheld | — |
| 4 | Skeptic | #1595 architecture finding: the member-order guard is opt-in per derived class of `TypeMemberOrderingCodeFixProviderBase`; RH7102 never opted in (FC-016) | Change the structure in this run: the member-moving base class applies the shared member-order guard for every derived fix, instead of RH7102 opting in as a third copy of the call | The class already repeated (a sibling went without the guard); the change stays within the components the run changes | RH7103/RH7109 already withhold on the same decision; RH7102 withholds only where SHARED-FIX-003 requires | — |
| 5 | Skeptic | #1595 spec clarification | Spec author: RH7102 spec lists SHARED-FIX-003 / SHARED-ORDER-007; SHARED-ORDER-007 states the guard for every member-moving fix (or names RH7102) | Spec should state what the code now does | Spec wording is internal | — |
| 6 | Skeptic | #1593 precision for a member split by `#if` | A: count what each configuration gives the member; the issue's own example keeps its fix and is a counterpart test. Defect lives in `OrderingDeclarationUtilities` (Core), not `RegionBlockLayout`; tests cover the skeptic's failing rows across RH7309, RH7109, RH7103 (and RH7102 after #1595) | B would withhold a correct fix, a visible change no requirement demands; agent-filed goal adjusted | A withholds only where SHARED-FIX-003 requires | — |
| 7 | Skeptic | #1616 architecture finding: modifier reorder has two owners (`ModifierOrderingCodeFixProviderBase` private trivia handling vs RH2001's own) | Change the structure in this run: RH2001's reorder goes through the one owner of SHARED-MODORDER-002/003 trivia handling | A narrow repair would add a second copy of SHARED-MODORDER-002 | Outputs follow SHARED-FIX-007, RH2001-004 and the maintainer's answers | — |
| 8 | Skeptic | #1616 cases c, d, e, n (comment/doc comment/`#pragma`/`#region` above the property) withheld today | Fix them: RH2001-004 requires a fix with the trivia once in front of the field | Bug against an unchanged requirement | Demanded by RH2001-004 | — |
| 9 | Skeptic | #1616 scope of withholding for a comment between modifiers (if Q1 = A) | Withhold only where the reorder moves a modifier across a comment; already-ordered `private /* a */ static` keeps today's fix | Wider guard would change a correct output no requirement demands | Keeps today's output for fixture o | — |

## Follow-up candidates

- RH7102 analyzer ignores regions: reports a constant in `#region Constants` after a field in `#region Fields`, against RH7102-001/-003; `NoCodeFixWhenDirectivesAreInLeadingTrivia` hides it (01-skeptic.md, #1595).

## Worklist / next step

- Done: skeptic `01-skeptic.md` (36f11ca).
- Now: output questions batch 1 (#1616 Q1 comment between reordered modifiers, Q2 blank line above) — waiting for the maintainer.
- Next: spec author (clarifications D5, D7 outcome, RH2001 spec after answers) → test implementer.

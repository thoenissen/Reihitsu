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

- Q1 (#1264, RH7309, regions in the `#if` and `#else` branches of one conditional): **A** — not reported.
- Q1b (#1264, RH7309, region in one branch after a region outside every branch): **A** — maintainer, summarized: with the symbol defined it is reported without a fix; with it undefined nothing is reported (the analyzer judges the active configuration).
- Q2 (#1334, RH7401–RH7412, partial member with only some parts in a matching region): **A** — reported once (implementing part in the example); for RH7405–RH7408 no part may lie in an override region.
- Q3 (#1337, RH7105/RH7106, conditional directive between the last modifier and the type, also when it selects the type): **B** — stays reported, no fix (unchanged; SHARED-MODORDER-003 keeps its clause). Maintainer's reasoning, summarized: the analyzer judges the code as the currently active defines leave it; no fix is offered.
- Q4 (#1337, RH5111, conditional directive inside the value): **A** — reported, no fix (unchanged).

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name | Use the session's mandated branch `claude/optimistic-hopper-gj3m1o` | The session environment allows pushes only there | Internal | 5f66bcd |
| 2 | Claim | Shared lane | Bug fix for all seven | Triage comments moved #1264, #1333, #1334, #1339, #1337 to bug fix without change doc; #1412, #1592 are bug fixes | Internal | 5f66bcd |
| 3 | Spec author | RH5102-004, RH5109-004, RH5111-005 alignment (three more analyzers, three tagged tests flip) | In scope of #1337 | The #1337 triage comment names these three requirements; SHARED-DIAG-008 (unchanged) demands the outcome | Unchanged SHARED-DIAG-008 demands it | 187b246 |
| 4 | Spec author | Rule pages RH7308, RH7401–RH7412 still say "top-level region"/"override methods" | Correct them in this PR (spec author amendment, with the next spec round) | Rule pages must describe the analyzer; the change follows unchanged SHARED-TYPEREGION-006 / SHARED-REGGROUP-006 | Brings the page in line with requirements already in force | cfa5e69 |
| 5 | Spec author | RH7308 innermost region with unrecognized label | Not checked; the outer region does not judge it (as written) | SHARED-TYPEREGION-006 + RH7308-002 | Decided by unchanged requirements | 187b246 |
| 6 | Skeptic | RH7309 fix on `main` breaks a safety rail for regions in a nested enum (unparsable result; enum members swapped) | Repair in this run via the enum ownership fix of #1264 (`GetTopLevelRegions`); tests pin both | Inside #1264's scope; repairable in scope, so no escalation 2 | Brings output in line with unchanged SHARED-TYPEREGION-006 | pending |
| 7 | Skeptic | Architecture finding #1592: RH7309 analyzer and code fix decide region ownership differently | Change the structure in this run: the fix uses the same ownership decision as the analyzer (one statement) | Not a rare class (`#region Nested types` is common); a narrow fix would keep two statements | Fix result follows SHARED-FIX-005 / RH7309-004 (unchanged) | pending |
| 8 | Skeptic | #1337 spec defects in 187b246: RH5102-004/RH5109-004 too wide (lambda bodies); RH7107-001/RH7108-001 must say "outside accessor bodies" | Spec author amends both as clarifications | SHARED-DIAG-008 decides; keeps today's output for those inputs | Unchanged requirement decides | cfa5e69 |
| 9 | Skeptic | #1339: RH7102-005/RH7109-005 should say "in the same top-level region" | Spec author clarifies | Follows RH7102-003/RH7109-003 | Unchanged requirement decides | cfa5e69 |
| 10 | Skeptic | #1412 cause is the Roslyn driver; other constructor rules miss the defining part too | Fix RH7105/RH7106 only; other constructor rules a follow-up | Scope | Narrower than the class; follow-up filed | pending |
| 11 | Skeptic | #1333: partial field-like event with two declarators | In scope; needs a test | Same requirement SHARED-DIAG-006 | Unchanged requirement decides | pending |
| 13 | Batch 1 | Q3-B leaves RH7105/RH7106's judged span different from RH7102/RH7103's (skeptic's conditional architecture finding) | Keep both statements, narrow fix: RH7105/RH7106 use the span up to the last modifier; no follow-up | The maintainer chose the visible output; the two rules judge different things (keyword order vs. modifier placement) | Output decided by the maintainer | cfa5e69 |
| 14 | Test implementer | RH7101: conditional directive in the initializer of a non-last declarator | Not reported (test as written) | RH7101-001's unchanged text "stands between its declarators" covers it literally; rare input | Decided by the unchanged requirement's literal text; reviewer can overrule | b47e6b2 |
| 15 | Implementer | #1339: new RH7102-005/RH7109-005 examples and tests `…CodeFixTargetsTheFirstFieldOfTheGroupIn…OwnRegion` expect a move across the `#region B` line, which unchanged SHARED-ORDER-004 forbids | Spec author rewrites both examples so the target is not the first member of its region; test implementer rewrites the two tests and pins that the SHARED-ORDER-004 shape stays without a fix | Unchanged SHARED-ORDER-004 decides; no shared move change | Decided by an unchanged requirement (today's output: reported, no fix) | e780d7d, 373d0ff |
| 16 | Implementer | RH7311: a type wholly inside `#if NET…` — implementer counts a branch only when its directive starts inside the type | Accept; spec author states it in RH7311-001, test implementer adds a test | Matches today's output and the maintainer's active-configuration principle; a literal reading would silence RH7311 for common whole-file `#if` | Output unchanged from `main` | e780d7d, 373d0ff |
| 17 | Implementer | `RegionDirectiveUtilities.TryFindContainingRegion` has no production caller left | Remove it and its Core test | Cleanup the run's change touches | Internal | pending |
| 18 | Implementer | 156 net production lines, no new type | Accept, no escalation 9 | Spread over seven issues and ~20 rules; per-issue changes are small; no new type | Internal | 69a1330 |
| 12 | Skeptic | Follow-up candidates: RH7311 branch-vs-outside; formatter `=` continuation after empty `#if` | Dropped | RH7311 decided by its unchanged spec (no defect); formatter case rare and stable | n/a | n/a |

## Follow-up candidates

- RH5101-004 wording equals the too-wide RH5102-004; ~60 rule specs say directives only withhold the fix without referring to SHARED-DIAG-008 (02-skeptic, #1337).
- Every syntax-node rule on constructors misses the defining part of a partial constructor (Roslyn driver), besides RH7105/RH7106 (02-skeptic, #1412).

## Open output questions (batch 1, before test implementer)

From 01-spec-author.md: Q1 (#1264 RH7309 regions in #if/#else branches), Q2 (#1334 partial member with only some parts in place; RH7405–RH7408 reading), Q3 (#1337 SHARED-MODORDER-003 directive between last modifier and type), Q4 (#1337 RH5111 directive inside the value).

## Batch 1

Answered in full (see Maintainer answers).

## Worklist / next step

- Done: claim (5f66bcd), draft PR #1734, prepare.sh green, spec author 187b246 (lane check: confirmed).
- Skeptic 790f9fc done.
- Spec author amendment cfa5e69 done (decisions 4, 8, 9, 13 + answers carried out).
- Test implementer b47e6b2 done: all seven reproduce (96 analyzer + 1 core failing tests, all new/rewritten).
- Implementer 69a1330 / 0edad3b: all pass except the two #1339 fix tests (decision 15).
- Not run locally by the implementer: formatter/CLI tests, self-hosting scan (CI runs it; RH7308/RH7311/RH74xx changes may show new findings there).
- Spec author e780d7d, test implementer 373d0ff: all corrected tests pass.
- Next: implementer (08: remove TryFindContainingRegion), then cleaner, spec checker, auditor, validator.

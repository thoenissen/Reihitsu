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
| 3 | Skeptic | #1604 goal: one shared "move lines by a column delta" helper used by RH3004, RH7004, member-ordering fixes | Dropped | RH3004's shifter covers none of RH7004's hard classes (02-skeptic); member-ordering fixes need no shifter under Q1 A; a shared extraction would exceed ~100 lines | Internal structure | — |
| 4 | Skeptic | RH7004 moved statements holding disabled text | Withhold the fix (rare class, local refusal, W1) | AGENTS.md Code-fix coverage; rare | Rare-class refusal puts nothing in question | — |
| 5 | Skeptic | RH7004 moved statements holding directives | Keep the fix; cheap per-line rule: non-region directives stay in column 0, region directives move with the code | Cheap check on the line's first trivia; `#pragma`/`#if` in bodies is not rare | Directive position decided by SHARED-FIX-003/007 | — |
| 6 | Skeptic | Fixes offered where an unsupported shape elsewhere in the type withholds them today (FC-043) | Accept; the guard follows the node actually formatted; PR names it | Required by SHARED-FIX-006/010 | Decided by unchanged requirements | — |
| 7 | Skeptic | #1604 comment item (d) did not reproduce | Test implementer records it as not reproduced; no further work | Three constructed shapes got no fix offered (directive guard) | Nothing changes | — |
| 8 | Skeptic | #1151: fix can rename the `#endregion` keyword (`#Endregion`), breaking compilation | In scope: RH7302-005 already excludes it; test required | Safety rail, inside the run's issue | Decided by RH7302-005 | — |
| 9 | Skeptic | RH7004 `/* */` comment continuation line changed (finding 2) | In scope; covered by Q5's answer | Pre-existing defect in the run's fix | Asked as Q5 | — |
| 2 | Spec author | #1604 goal: offer RH3004 fix for blocks with multi-line raw strings | Dropped from this run; follow-up candidate | Contradicts unchanged RH3004-007/008; would be a behavior change outside the bug-fix lane | The RH3004 output stays as on main | 57f49a2 |
| 1 | Claim | Branch name `claude/batch-969-<slug>` vs. the session's mandated branch | Use `claude/dreamy-planck-yry5n0` | The session environment allows pushing only to that branch | Branch names are internal | claim commit |

## Follow-up candidates

- RH3004 fix offered for blocks with multi-line raw string literals (behavior change; from #1604 goal) — 01-spec-author.md
- SHARED-FIX-003 ("indentation of its lines" is layout) vs FMT-INDENT-007 (a `/* */` comment's later lines are text) read differently — 02-skeptic.md

## Audited SHA

(none yet)

## Open worklist / next step

- Done: spec author 01 (57f49a2): clarifications RH7302-005, RH7303-004; output questions Q1–Q3; withholding options W1, W2.
- Done: skeptic 02 (66fa78d): all premises real; extra Q4, Q5a, Q5b.
- Asked batch 1 (Q1–Q5) to the maintainer; waiting.
- Next after answers: then batch Q1–Q3 (+ skeptic's) to the maintainer before the test implementer.

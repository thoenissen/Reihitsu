# Lead — fix-scope-moved-code

- Issues (prompt order): thoenissen/Reihitsu#969, #1335, #1263, #1151, #1604 — all labeled `agent-filed` (goals are proposals).
- Slug: `fix-scope-moved-code`
- Branch: `claude/dreamy-planck-yry5n0` (session-mandated branch, see Decisions #1)
- Draft PR: thoenissen/Reihitsu#1688
- Lane: **behavior change** (widened from bug fix, decision 10)
  - #969: observed vs. expected — member-ordering fixes (RH7102/RH7103/RH7109) reformat the whole type against FMT-PIPE-010/011.
  - #1335: observed vs. expected — RH7004, RH7104–RH7108 fixes change code beyond the node they rewrite or move (SHARED-FIX-006/007, P14a/P26).
  - #1263: observed vs. expected — RH7303/RH7310 fixes leave a blank line after removing a region (P14c, SHARED-FIX-007).
  - #1151: observed vs. expected — RH7302 fix renames `#endregion` comments that only contain the region name (SHARED-FIX-003).
  - #1604: observed vs. expected — moved code is reformatted against SHARED-FIX-007 (moved-code bullet); proposes a shared column-delta helper (RH3004, RH7004, member-ordering).
- Roles: spec author (clarifications only: #1151 asks the RH7302 spec to state the rule; any other clarification) → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator; issue manager before publishing when follow-up candidates exist.

## Maintainer clarifications (summarized)

- Withholding a code fix is an equally valid outcome. A full implementation can always be delivered later through a follow-up issue, when it is proportionate. (Given with the `/squad` prompt.)

## Maintainer answers

Batch 1 (asked as chat questions 1–6; skeptic/spec-author IDs in brackets):
- Q1 [Q1] member/accessor moves (RH7102/7103/7107/7108/7109): **A** — moved lines keep their written columns (shift = the member's first line's shift). Added: a move must always be a full-line move; any move that goes beyond moving whole lines (e.g. a member sharing a line with another member or token) is not offered — withhold the fix.
- Q2 [Q2] RH7004 continuation lines: **A** — a plain shift of every moved line by one level.
- Q3 [Q5a/W2] RH7004 with a string literal or `/* */` comment spanning lines in the moved statements: **C** — withhold the fix. A (shift raw-string lines) would also be acceptable only at no additional cost; it is not taken.
- Q4 [Q5b] RH7004 comment in column 0 among moved statements: **A** — stays in column 0.
- Q5 [Q4] RH7303 blank line at the edge of any braces (initializer, accessor list, switch): **A** — removed.
- Q7 RH7107/RH7108 accessor list on one line: **B** — accessors swap places within the line; SHARED-ORDER-009 gains "or the whole accessor list stands on one line"; RH7107-004 unchanged.
- Escalation 9 (size of the three Core helpers, +~470): **A** — keep everything, plus the cleanup (decisions 19, 20).
- Q6 [Q3] RH7302 trailing whitespace behind the `#region` description: **A** — the `#endregion // members` is still renamed.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 40 | Spec author 19 | Choices: comment in front of the first accessor also withholds the swap; RH3004 block-body path covered by the SHARED-FIX-008 refusal (decision 39 resolved in run); line-ending refusal stated once in SHARED-FIX-008 | Accept all three | Rare classes; one statement of the rule | Rare-class refusals | 0669c17 |
| 34 | Auditor 18 (repair round 2) | F1: interpolated strings spanning lines are shifted (value change); two owners of "a string spanning lines withholds" (line-based `StartsInsideContent` vs RH3004's node-based check) | Change structure in this run: one shared node-based check (string literal of any kind incl. interpolated, or comment, spanning lines) in Core, used by RH3004, RH7004 and the member/accessor moves; RH3004's private check deleted. Tests for every failing member | Safety rail (literal value); narrow fix would keep two owners | Refusal per Q3 C / SHARED-ORDER-010; nothing new visible | — |
| 35 | Auditor 18 | F2: SHARED-FIX-008 broken in mixed / all-`\r` documents | New line breaks are written with `LineEndingUtilities.DetectEndOfLine` (RH7004's private `GetEndOfLine` deleted); the fixes are withheld when the span they rewrite or move holds a line break other than the detected one (mixed endings: rare class). Tests: mixed and all-`\r` | Unchanged requirement; cheap refusal | Rare-class refusal; unchanged requirement | — |
| 36 | Auditor 18 | F3: one-line accessor swap moves comments to the other accessor | Withhold the one-line swap when a comment stands inside the accessor list (rare, cheap) | SHARED-ORDER-009; Code-fix coverage | Rare-class refusal | — |
| 37 | Auditor 18 hint | RH7004 on a tab-indented declaration copies `\t` onto the new brace lines | Withhold RH7004 when the declaration's or a moved line's indentation holds a tab (rare, RH5601; same as decision 26) | One rule for tabs | Rare-class refusal | — |
| 38 | Auditor 18 hint | RH3004 unreachable `return formattedDocument` fallback | Implementer removes it if truly unreachable | Dead code in a file the run changes | Internal | — |
| 39 | Auditor 18 hint | RH3004 mixed line endings (pre-existing on base) | Covered by decision 35 if RH3004 goes through the shared helper's refusal; otherwise follow-up candidate | — | — | — |
| 31 | Spec checker 15 (repair round 1) | #1263 (Q5 A) and #1151 (Q6 A) visible changes missing from the change doc | List both in the change doc (header, Visible output changes, Decisions); decision 14's "no change doc" is superseded | Both are maintainer-answered visible changes; one change doc for the run is simplest | Answered by Q5/Q6 | — |
| 32 | Spec checker 15 | `documentation/rules/RH7004.md` lists a withheld fix | Remove the paragraph (general-notes covers withheld fixes); adjacent docs fix | Rule-page convention; affected rule | Withheld fix itself unchanged; text only | — |
| 33 | Spec checker 15 | Change doc Status Draft | Lead accepts; spec author sets `Accepted` | All visible changes answered or rare-class refusals | — | — |
| 29 | Implementer 13 | Zero-shift member moves now also write whitespace-only lines empty | Accept | SHARED-FIX-007 states it for all moved code; main does the same (11) | Unchanged from main | eac59e0 |
| 30 | Implementer 13 | `UsesListsExactlyTheReferencedRequirements`: RH3004.md references FMT-PIPE-017 without listing it | Spec author 11 continued for the fix | Spec structure | Internal | — |
| 26 | Implementer 10 | RH3004 with tab indentation: shared rule counts a tab as 1 column, old private shifter as 4 → visible change (16 vs 25 spaces) | Neither A nor B: RH3004 withholds its block-body fix when the indentation of a line it would shift holds a tab (rare: RH5601 reports tabs). Cheap check | A visible change no requirement demands would be in question; refusing a rare class is not | Rare-class refusal | — |
| 27 | Implementer 10 | Whitespace-only blank lines in moved code: kept as written now, emptied on main (RH3004 old shifter and RH7004 via formatter) | The shared helper writes a whitespace-only line in the moved span empty (main's output for both fixes); SHARED-FIX-007 states it | Keeps main's visible output; one cheap rule | Unchanged from main | — |
| 28 | Implementer 10 | Release build error RH7305 in RH7109 test file | Cleaner | Formatting/warnings belong to the cleaner | Test code | — |
| 24 | Spec author 08 | A left shift that would cut a moving line at column 0 | Withhold (SHARED-ORDER-010); keeps main's no-fix result | Rare class (misindented member, RH5204) | Rare-class refusal | 840e68b |
| 25 | Spec author 08 | Change doc amendment: six added visible output changes | Accepted by the lead: all are rare-class refusals or covered by the Q1 answer (whole-line rule) / decision 22 | Code-fix coverage; maintainer answer Q1 | Refusals of rare classes / answered | 840e68b |
| 19 | Implementer 07 | Arch finding 1: moved-line rule in both `SyntaxIndentationUtilities.TryShiftMovedLines` and RH3004's private shifter; they disagree on `/* x */ Foo();` in column 0 | One owner: RH3004 uses the shared helper, its private copy is deleted; the helper's rule is RH3004's (only a line that holds nothing but a comment keeps column 0, per SHARED-FIX-007 "comment line"). | The run added the copy (README Architecture findings: change structure in this run); RH3004 is the counterpart #1604 names | RH3004 output unchanged; `/* x */ Foo();` is a code line, SHARED-FIX-007 decides | — |
| 20 | Implementer 07 | Arch finding 2 / dead code: old `MoveNodeBeforePreservingSeparators`, `MoveMemberBefore`, `MoveAccessorBefore` only called by Core tests | Delete helpers and their Core tests | Second statement of the separator rule; dead code | Internal | — |
| 21 | Implementer 07 | Unspecified rare refusals (RH7004: code before the declaration on its line; last statement on the `}` line; comment/directive before `;`. Moves: target leading trivia starting behind code) | Accept as rare-class refusals; spec author records them; test implementer fixes the stale summary/tag of `NoFixIsOfferedForACommentInFrontOfTheSemicolon` | Rare classes (other rules report them) | Rare-class refusal | 9a3dded |
| 22 | Implementer 07 | Layout choice: a `///` comment behind code that travels with the moved declaration is put on its own line | Withhold instead (decides a line break; Q1 whole-line rule) | Maintainer's Q1 addition | Covered by Q1 answer | — |
| 23 | Implementer 07 | Other open layout choices (crossed declarations sharing a line move as a block; blank-line join keeps the first blank lines; tab = one column) | Accept; spec author records where a spec leaves it open | Whole-line moves; rare inputs | Rare / covered by Q1, Q5 | 9a3dded |
| 15 | Test implementer 05 | SHARED-FIX-003 RH7004 example ("offered with and without DEBUG") vs RH7004-007 | RH7004-007 wins; spec author corrects the SHARED-FIX-003 sentence | Decision 4 | Rare-class refusal, decided | — |
| 16 | Test implementer 05 | RH7302-005 example span ends before trailing spaces vs RH7302-003 | RH7302-003 wins; spec author corrects the example markup | Unchanged requirement decides | Decided by RH7302-003 | — |
| 17 | Test implementer 05 | Column-0 comment in the moved declaration's own leading trivia with non-zero shift (SHARED-ORDER-002 vs SHARED-FIX-007) | SHARED-FIX-007's line rule applies (keeps column 0); spec author states it; no extra test (rare: non-zero shift needs a misindented member, RH5204) | One per-line rule, no special case | Rare class | — |
| 18 | Test implementer 05 | Visible changes demanded by unchanged requirements/decision 6 (3 blank lines above moved member kept; empty partial types keep braces after RH7104; tabs behind region keywords kept by RH7105; 7 fixes now offered next to refused comments) | Accept; name in PR review notes | Demanded by SHARED-FIX-006/007 and decision 6 | Unchanged requirements decide | 5bd3a7b |
| 10 | Spec author 03 | Change doc `sdd/changes/2026-11/moved-code-keeps-its-lines.md` required (#969, #1335, #1604) | Widen lane bug fix → behavior change; lane check confirms | Answers give output that is not today's behavior and add refusals | Visible changes all answered by the maintainer (batch 1) | 0a3211b |
| 11 | Spec author 03 | Second skeptic pass on the amended specs? | No; accept change doc once Q7 is answered and written; spec checker and auditor check it | The amendment carries out maintainer answers; skeptic already ran on the premise | Internal process | — |
| 12 | Spec author 03 | RH7004 withheld when a statement starts on the declaration's line | Accept (rare: RH5103 reports it; main writes `}     }`) | Rare-class refusal | Rare-class refusal | 0a3211b |
| 13 | Spec author 03 | SHARED-ORDER-010 (non-zero shift + spanning string/comment/disabled text withheld) | Accept as rare-class refusal (RH5204 reports misindented members) | Rare class | Rare-class refusal | 0a3211b |
| 14 | Spec author 03 | Column rule limited to member/accessor moves and RH7004; #1263/#1151 clarifications without change doc | Accept | Run's issues only; clarifications of unchanged requirements | Q5/Q6 answered | 0a3211b |
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
- RH7004 fix for moved statements holding multi-line strings / `/* */` comments (maintainer answer Q3: withheld now; implementation possible later if proportionate) — 00-lead.md
- (none from 07 beyond decisions 19/20, carried out in this run)

## Audited SHA

(none yet)

## Open worklist / next step

- Done: spec author 01 (57f49a2): clarifications RH7302-005, RH7303-004; output questions Q1–Q3; withholding options W1, W2.
- Done: skeptic 02 (66fa78d): all premises real; extra Q4, Q5a, Q5b.
- Asked batch 1 (Q1–Q5) to the maintainer; waiting.
- Answers received (see Maintainer answers).
- Done: spec author 03 (0a3211b): specs + change doc; open Q7 (one-line accessor lists). Q7 answered B.
- Done: spec author 04 (6de86b6): Q7 B in specs. Change doc accepted by the lead.
- Done: test implementer 05 (5bd3a7b): all reproduced; #1604 (d) not reproduced (characterization). 98 failing tests.
- Done: spec author 06 (c3c16cf).
- Done: implementer 07 (9a3dded, 10c718c): all tests pass; +700/-242 production, ~470 in three Core helpers.
- Escalation 9 answered A. Spec author 08 done (840e68b). Test implementer 09 done (ba8db8e, 8 failing rows). Implementer 10 done (1341a2c): all pass, net production +136. Spec author 11 done (bffd78a; change-doc tab refusal accepted by lead as rare class). Test implementer 12 done (a689780, 10 failing). Implementer 13 done (eac59e0; net production +156). Spec author 11 fix (7f06aeb), cleaner 14 (5b927c1) done. Spec checker 15: findings (repair round 1). Spec author 16 done (368cddb). Spec checker 17: consistent (c20db4a). main unmerged-needed: up to date (4bdcd13186c2882e63985526458f63c4b359236d). Release build green. Auditor 18: findings F1–F3 (92f7308, audited head 2ee125d, base 4bdcd13). Repair round 2: spec author 19 done (0669c17) → test implementer 20 → implementer 21 → cleaner 22 → spec checker 23 → auditor 24 (repair only) → validator. Structural round: repair-like structural round (decisions 19–23) — spec author, test implementer, implementer — then cleaner.
- (old) Next: fresh spec author writes the answers into the specs (and change doc if sdd/README requires one); then test implementer.
- (old) Next after answers: then batch Q1–Q3 (+ skeptic's) to the maintainer before the test implementer.

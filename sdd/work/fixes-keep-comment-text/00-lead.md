# Lead — fixes-keep-comment-text

- Issues: thoenissen/Reihitsu#1075 (`agent-filed`, goals are proposals), thoenissen/Reihitsu#1261 (`agent-filed`, goals are proposals)
- Branch: `claude/admiring-shannon-9h4j37` (the session's designated branch, see decision 1)
- PR: thoenissen/Reihitsu#1618 (draft claim)
- Slug: `fixes-keep-comment-text`
- Lane: **bug fix**
  - #1075: reported observed-versus-expected differences of blank-line analyzers/fixes (RH5025, RH5027, RH5011, RH5030, RH5023) against existing requirements SHARED-DIAG-003, SHARED-BLANK-001, SHARED-FIX-001, SHARED-FIX-003. Its blockers #1068 and #1069 are closed (merged as PRs #1100, #1099).
  - #1261: reported differences of code fixes RH5405–RH5407, RH5412–RH5417 against SHARED-FIX-003, SHARED-EMPTY-003, RH5417-003, SHARED-BRACE-002. Triage comment on the issue: bug-fix lane, no change doc; the fix follows SHARED-FIX-007, and SHARED-BRACE-002 is aligned to SHARED-FIX-007 as a clarification in the same PR.
- Roles: spec author (clarification only: SHARED-BRACE-002 alignment, and any clarification the analysis needs) → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator. Issue manager before publishing if there are follow-up candidates.

## Maintainer clarifications (from the issues)

- #1075: lines inside literal or comment text are content, never blank lines; a fix must not change that text. Instances: RH5025 interpolated raw string with an empty line before a hole brace (holes inside a literal not considered — decided); RH5027 checks only the gap directly before `while`; RH5011 inserts the blank line after the comment's end, or withholds; RH5030 `} /* a⏎b */ a++;` corrected in one application with correct indentation, or withheld. Comment: RH5023 does not report blank lines inside an interpolation hole's expression (SHARED-DIAG-003 treats holes as code); the shared helper `FormattingTextAnalysisUtilities.GetStringLineIndices` is also used by RH5204, RH5602, RH6004, RH8301–RH8303; RH5204's hole indentation is undecided (#951) and must not change.
- #1261: a fix touches only what its rule reports; comment and directive text stays as written. Instances: empty-type rules rewrite `#region legacy`/`#endregion` text; RH5417 the same in an accessor list; statement-brace fixes duplicate a trailing comment.

## Maintainer answers

- Output question 1 (RH5023, blank lines inside an interpolation hole's expression): **B** — not reported in this run, as today; the case becomes a follow-up issue carrying the question. RH5023-002 stays unchanged.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name | Use the session's designated branch `claude/admiring-shannon-9h4j37` instead of `claude/issues-1075-1261-<slug>` | The session's environment forbids pushing to any other branch | Branch names are internal | claim commit |
| 2 | Spec author | RH5011 and RH5030 fix for a comment containing a line break: correct it or withhold it | Withhold it (RH5011-007, RH5030-003) | The maintainer's decision in #1075 allows either; withholding needs no new behavior | Both outcomes were already chosen by the maintainer in #1075 | 461faa4 |
| 3 | Spec author | Change doc | None | Every change writes down what unchanged requirements (SHARED-FIX-003/006/007, SHARED-DIAG-003) already decide; triage on #1261 says no change doc | — | 461faa4 |
| 4 | Skeptic | RH5011: withhold for every comment with a line break, or insert below the comment | Insert the blank line below the comment's last line when the next code starts on its own line; withhold only when code follows on the comment's last line; cover switch labels too. Overrides decision 2 for RH5011; RH5030 stays withheld (RH5030-003) | AGENTS.md: a guard that refuses safe inputs is worth narrowing; RH5030 already handles that shape | The maintainer chose "insert after the comment's end, or withhold" in #1075 | pending |
| 5 | Skeptic | RH5027 extra inputs `/* c */ while` alone on its line and `} /* c */ while` | Take in: the unchanged RH5027-001 already decides them; same rule, same gap | Within #1075's instance | Unchanged requirement demands it | pending |
| 6 | Skeptic | RH5025-006 word "regular" | Drop it; a regular `$"` string cannot hold a line break | No real input | Spec wording | pending |
| 7 | Skeptic | #1261 region-text instances | Not reproduced on main (FMT-PIPE-028, PR 1578). Keep a characterization test only if one is cheap; the blank lines around region lines belong to #1140 → follow-up candidate | Already fixed | No output changes | — |
| 8 | Skeptic | SHARED-BRACE-002: full alignment to SHARED-FIX-007 or narrow | Narrow: revert SHARED-BRACE-002 to main's text plus a sentence that a comment is written once; the alignment and #1209 Q2–Q4 belong to #1140/#1144 → follow-up candidate. Spec author's Q2–Q6 are not asked | Maintainer already routed Q2–Q4 to #1140/#1144 (1.2.1); keeps today's outputs | Dropping the rewrite changes no output | pending |
| 9 | Skeptic | Architecture finding: two owners (ControlFlowBraceTransform and the node-level entry point FormatNodeInDocumentAsync) place the body's trailing trivia | Change the structure in this run: the duplication is removed so that exactly one of the existing owners writes that trivia; no third owner (no compensation in the code-fix base). Spec author clarifies FMT-PIPE-010 if the owner is the entry point | A narrow fix in the code fix would add a third owner (README, Architecture findings) | Comment written once is the maintainer's decision in #1261 | pending |
| 10 | Skeptic | `else if` duplication test | No dedicated test that would assert today's wrong brace column; the shared mechanism covers it via the other three positions | Avoid locking a wrong output (#1209 Q4 goes to #1144) | No output asserted | — |

## Follow-up candidates

- RH3208 fix on `int L() => 1; /* a⏎b */ L();` leaves `L();` at column 0 with trailing whitespace (02-skeptic). RH5030's change must not alter RH3208.
- Extend #1140: RH5412 and RH5417 fixes insert blank lines around region lines (02-skeptic).
- Extend #1140/#1144: align SHARED-BRACE-002 to SHARED-FIX-007 (#1209 Q2–Q4), dropped from this run by decision 8 (01-spec-author, 02-skeptic).
- RH5023 hole case: blank lines inside an interpolation hole's expression are not reported (SHARED-DIAG-003 vs RH5023-002); shared helper `GetStringLineIndices` also used by RH5204 (#951), RH5602, RH6004, RH8301–RH8303. Carry output question 1 (maintainer answered B: defer). Source: #1075 comment, 01-spec-author, 02-skeptic.

## Worklist / next step

- Done: spec author (01, 461faa4). Open output questions from it: Q1 RH5023 blank lines inside an interpolation hole; Q2-Q6 statement-brace outputs (Q2 is the issue's own decision).
- Done: skeptic (02, 1a16550).
- Maintainer answered Q1: B. Next: spec author amendment (decisions 4, 5, 6, 8, 9, Q1 answer) → test implementer.

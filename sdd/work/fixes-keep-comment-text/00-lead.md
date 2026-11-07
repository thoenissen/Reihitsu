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

(none yet)

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name | Use the session's designated branch `claude/admiring-shannon-9h4j37` instead of `claude/issues-1075-1261-<slug>` | The session's environment forbids pushing to any other branch | Branch names are internal | claim commit |
| 2 | Spec author | RH5011 and RH5030 fix for a comment containing a line break: correct it or withhold it | Withhold it (RH5011-007, RH5030-003) | The maintainer's decision in #1075 allows either; withholding needs no new behavior | Both outcomes were already chosen by the maintainer in #1075 | 461faa4 |
| 3 | Spec author | Change doc | None | Every change writes down what unchanged requirements (SHARED-FIX-003/006/007, SHARED-DIAG-003) already decide; triage on #1261 says no change doc | — | 461faa4 |

## Follow-up candidates

(none yet)

## Worklist / next step

- Done: spec author (01, 461faa4). Open output questions from it: Q1 RH5023 blank lines inside an interpolation hole; Q2-Q6 statement-brace outputs (Q2 is the issue's own decision).
- Next: skeptic running; then batch output questions before the test implementer.

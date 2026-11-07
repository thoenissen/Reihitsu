# Lead — rh5202-nested-literal-holes

- Issue: thoenissen/Reihitsu#1458 — "RH5202: the fix leaves nested verbatim strings and comments in a hole unchanged" — labels `bug`, `analyzer`, `agent-filed` (goals are proposals).
- Slug: `rh5202-nested-literal-holes`
- Branch: `claude/tender-shannon-bcg2tr` (session-designated branch, used instead of `claude/issue-1458-...`)
- PR: thoenissen/Reihitsu#1610 (draft claim)
- Lane: bug fix — the issue reports an observed-versus-expected difference in the RH5202 code fix against SHARED-FIX-003 / FMT-RAW-009.
- Roles: skeptic (premise + reachability) → test implementer (reproduction) → implementer → cleaner → spec checker → auditor → validator. Spec author only for a clarification.
- Maintainer clarification: the maintainer suspects the issue may already be fixed on `main` (later raw-string work landed, e.g. #1469, #1457, #1565).

## Maintainer answers

- Q1 (RH5202, a verbatim string or `/* */` comment spanning lines inside a hole): **A** — the fix writes the formatter's FMT-RAW-009 output; lines that start inside the nested literal's or comment's text stay as written; the diagnostic is cleared.
- Q2 (may the RH5202 fix withhold itself for an input where a rewrite would be unsafe?): **A** — yes; the fix is withheld and the diagnostic stays without a fix.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name | Use the session-designated branch `claude/tender-shannon-bcg2tr` | The session harness requires it | Branch name is internal | claim commit |
| 2 | Skeptic | Architecture finding: the RH5202 fix's `RealignContent` is a private copy of the line-shift decision `RawStringLineShifts` owns | Change the structure in this run: the code fix stops deciding line shifts on its own and takes the decision from the formatter's single owner (through the routes FMT-PIPE-009 allows); no second statement of FMT-RAW-009 in the code fix. The mechanism is the implementer's to derive; it must not run the full pipeline's unrelated edits into the literal (FC-030) | A narrow fix would restate FMT-RAW-009 inside the code fix, which is the drift that caused the bug | The output follows SHARED-FIX-007 / FMT-RAW-009 and the answer to output question 1 | — |
| 3 | Skeptic | Widen the issue goal from its two instances to the whole class (all nested non-raw literals and comments spanning lines in a hole, both directions, deeper nesting) | Widen | Same mechanism; every member follows output question 1 | Covered by output question 1 | — |
| 4 | Skeptic | Adjacent B5: the fix writes trailing spaces onto an empty content line of a plain multi-line raw string | Take into the run as an adjacent fix, with a test | Same component, same root cause, small; SHARED-FIX-007 already decides the output (the formatter's empty line) | An unchanged requirement demands the output | — |
| 5 | Spec author | Change doc required? | No; RH5202-003 clarified, RH5202-004/005 added as clarification; lane stays bug fix (lane check confirmed) | SHARED-FIX-003/007 and FMT-RAW-009 already decided both outputs | Outputs decided by unchanged shared requirements and Q1/Q2 | 060e546 |
| 6 | Audit 1 | Architecture finding: the width of the whitespace in front of the closing quotes is measured in three places (analyzer `TrimStart`, fix `GetLeadingWhitespaceCount`, formatter `GetLeadingSpaceCount`); the fix's copy drifted (finding #1) | Change the structure for the analyzer and its code fix: the fix takes the closing-line measurement from one statement shared with the analyzer (no private copy in the fix), which restores `main`'s convergence for every whitespace character. The formatter's measurement stays and becomes a follow-up candidate | A narrow fix would keep a second copy in the fix; the analyzer is the fix's counterpart, the formatter's divergence is pre-existing | Restores `main`'s output; nothing new visible | — |
| 7 | Audit 1 | Finding #2: empty lines inside a hole no longer shift, against RH5202-003/005 as written | Repair the code to the spec as written: an empty line inside a hole (not inside nested text) shifts as on `main`; RH5202-005 stays limited to content lines; formatter output must not change | Brings the code back to an unchanged requirement and to `main`'s output, so no output question | Same output as `main`, demanded by RH5202-003 | — |

## Follow-up candidates

- Formatter: whole-document formatting leaves a closing line that starts with a non-space whitespace character (e.g. U+00A0) that RH5202 still reports; `RawStringLineShifts.GetLeadingSpaceCount` measures only spaces while the analyzer measures every whitespace character (07-auditor.md, hint; decision 6).

## Worklist / next step

- Skeptic done (01-skeptic.md, e6b9fbb): reproduced on main for the code fix only; formatter correct; caller reaches it.
- Spec author done (02-spec-author.md, 060e546).
- Test implementer done (03-test-implementer.md, 5f2ff33): reproduced, 29 red.
- Implementer done (04-implementer.md, da441e9 production): shared `RawStringLineUtilities.MovesLine`; tests green.
- Cleaner clean (05, d328e75). Spec checker consistent, no findings (06, 744f053). Auditor (07, 3347630): findings #1, #2 + architecture finding.
- Repair round 1 (of 3): test implementer (tests for findings #1 and #2) → implementer (decisions 6, 7) → cleaner → spec checker → auditor (repair only) → validator. (was: spec author (RH5202-003 clarification per answer, Uses FMT-RAW-009) → test implementer.

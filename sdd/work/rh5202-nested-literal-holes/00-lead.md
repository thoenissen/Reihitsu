# Lead — rh5202-nested-literal-holes

- Issue: thoenissen/Reihitsu#1458 — "RH5202: the fix leaves nested verbatim strings and comments in a hole unchanged" — labels `bug`, `analyzer`, `agent-filed` (goals are proposals).
- Slug: `rh5202-nested-literal-holes`
- Branch: `claude/tender-shannon-bcg2tr` (session-designated branch, used instead of `claude/issue-1458-...`)
- PR: thoenissen/Reihitsu#1610 (draft claim)
- Lane: bug fix — the issue reports an observed-versus-expected difference in the RH5202 code fix against SHARED-FIX-003 / FMT-RAW-009.
- Roles: skeptic (premise + reachability) → test implementer (reproduction) → implementer → cleaner → spec checker → auditor → validator. Spec author only for a clarification.
- Maintainer clarification: the maintainer suspects the issue may already be fixed on `main` (later raw-string work landed, e.g. #1469, #1457, #1565).

## Maintainer answers

- Q1 (RH5202, verbatim string / comment spanning lines in a hole: A = formatter's output, B = withhold): asked, awaiting answer.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name | Use the session-designated branch `claude/tender-shannon-bcg2tr` | The session harness requires it | Branch name is internal | claim commit |
| 2 | Skeptic | Architecture finding: the RH5202 fix's `RealignContent` is a private copy of the line-shift decision `RawStringLineShifts` owns | Change the structure in this run: the code fix stops deciding line shifts on its own and takes the decision from the formatter's single owner (through the routes FMT-PIPE-009 allows); no second statement of FMT-RAW-009 in the code fix. The mechanism is the implementer's to derive; it must not run the full pipeline's unrelated edits into the literal (FC-030) | A narrow fix would restate FMT-RAW-009 inside the code fix, which is the drift that caused the bug | The output follows SHARED-FIX-007 / FMT-RAW-009 and the answer to output question 1 | — |
| 3 | Skeptic | Widen the issue goal from its two instances to the whole class (all nested non-raw literals and comments spanning lines in a hole, both directions, deeper nesting) | Widen | Same mechanism; every member follows output question 1 | Covered by output question 1 | — |
| 4 | Skeptic | Adjacent B5: the fix writes trailing spaces onto an empty content line of a plain multi-line raw string | Take into the run as an adjacent fix, with a test | Same component, same root cause, small; SHARED-FIX-007 already decides the output (the formatter's empty line) | An unchanged requirement demands the output | — |

## Follow-up candidates

None yet.

## Worklist / next step

- Skeptic done (01-skeptic.md, e6b9fbb): reproduced on main for the code fix only; formatter correct; caller reaches it.
- Waiting: maintainer answer to Q1. Then: spec author (RH5202-003 clarification per answer, Uses FMT-RAW-009) → test implementer.

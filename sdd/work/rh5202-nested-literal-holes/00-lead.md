# Lead — rh5202-nested-literal-holes

- Issue: thoenissen/Reihitsu#1458 — "RH5202: the fix leaves nested verbatim strings and comments in a hole unchanged" — labels `bug`, `analyzer`, `agent-filed` (goals are proposals).
- Slug: `rh5202-nested-literal-holes`
- Branch: `claude/tender-shannon-bcg2tr` (session-designated branch, used instead of `claude/issue-1458-...`)
- PR: thoenissen/Reihitsu#1610 (draft claim)
- Lane: bug fix — the issue reports an observed-versus-expected difference in the RH5202 code fix against SHARED-FIX-003 / FMT-RAW-009.
- Roles: skeptic (premise + reachability) → test implementer (reproduction) → implementer → cleaner (11, 371a1af) → spec checker (12, 4cf325b): 2 findings.
- Repair round 2 (of 3): spec author done (13, c603e8a); decision 10 reverses decision 7 → spec author amendment done (14, 3c67247) → test implementer (retag per 13 + expected outputs per decision 10) → implementer (decision 10) → cleaner → spec checker → auditor (repairs only) → validator. (orig plan: spec author (12 finding 1: RH5202-005 states empty literal-text lines incl. nested raw string stay empty, empty hole-code lines shift) → test implementer (12 finding 2: retag) → cleaner → spec checker → auditor (repairs only) → validator. (was: → spec checker → auditor → validator. Spec author only for a clarification.
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
| 8 | Repair 1 tests | NBSP-first closing line shifting left fails on `main` too (no progress); RH5202-003 demands the output | Adjacent fix: the implementer makes that test pass too | Same component and mechanism as decision 6, small, tested | Demanded by unchanged RH5202-003 | — |
| 9 | Repair 1 impl | Untested change: an empty content line of a raw string nested in a hole now stays empty (on `main` the fix wrote spaces) | Keep it (matches the formatter, SHARED-FIX-007, and RH5202-005's intent; the nested literal's value is unchanged either way) and have it tested; also a Core unit test for `RawStringLiteralUtilities.GetLeadingWhitespaceLength` | Untested behavior must not ship | An unchanged requirement (SHARED-FIX-007) demands the formatter's layout | — |
| 10 | Repair 2 | Revisit decision 7: should an empty line of hole code get spaces when the literal shifts right? | Reverse decision 7: every empty line after the opening line stays empty — literal text, nested raw-string text, and hole code alike; a whitespace-only line still shifts | SHARED-FIX-007 requires the changed code to be laid out so that formatting the result changes nothing in it; the formatter never leaves whitespace on an empty line (it keeps an empty hole line empty, e.g. between lambda-block statements, or removes it), so spaces written onto it are always reformatted. `main`'s spaces were the same defect as B5 (decision 4); the auditor's finding #2 was right that the code departed from the run's spec text, but the spec text was this run's own and is corrected here | Demanded by unchanged SHARED-FIX-007 | — |
| 11 | Repair 2 spec | Output question from 14-spec-author: a whitespace-only line of hole code (fix shifts it on `main` and head; the formatter removes it) | Not asked in this run: the case is outside the run's issue and the run leaves `main`'s behavior unchanged (RH5202-003 shifts it); filed as a follow-up candidate carrying the output question | Lead playbook: an open question about a case outside the run's issues becomes a follow-up | Output equals `main` | 3c67247 |
| 12 | Audit 2 | Finding #3: leftward shift changes the value of a raw string nested in a hole whose lines carry less leading whitespace than the shift (pre-existing; breaks RH5202-004 and SHARED-FIX-003) | Repair in this run: the fix is withheld for every input where shifting would change a nested literal's value (maintainer answer Q2 = A); RH5202-004 states the withholding; tests for the failing members and the boundary | A safety rail in an output the run changes cannot ship; Q2 = A already decides withholding | Covered by maintainer answer Q2 | — |
| 13 | Audit 2 | Finding #4: invisible U+00A0 / U+2003 in Core test data rows | Repair: write them as escapes | PR-introduced, readability | Test-only | — |

## Follow-up candidates

- Formatter: whole-document formatting leaves a closing line that starts with a non-space whitespace character (e.g. U+00A0) that RH5202 still reports; `RawStringLineShifts.GetLeadingSpaceCount` measures only spaces while the analyzer measures every whitespace character (07-auditor.md, hint; decision 6).
- RH5202 fix: a whitespace-only line of hole code is shifted (2 → 15 spaces) while whole-document formatting removes the line; output question A = write it empty (as an empty hole-code line), B = keep the shift (14-spec-author.md; decision 11).
- RH5202 analyzer/fix in documents with lone `\r` line breaks: the analyzer splits token text only on `\r\n`/`\n` and reports already-aligned literals; the fix (closing line from `SourceText`) then makes no progress — pre-existing on `main`; structural: the analyzer and the fix extract the closing line differently (19-auditor.md, hint).

## Worklist / next step

- Skeptic (01, e6b9fbb): reproduced on main for the code fix only; caller reaches it.
- Spec author (02, 060e546). Test implementer (03, 5f2ff33): 29 red. Implementer (04, da441e9). Cleaner (05). Spec checker (06): no findings.
- Audit 1 (07, 3347630; audited head 685ab42, base b4343557): findings #1, #2 + architecture finding → decisions 6–8.
- Repair round 1: tests (08, 2ec5c72), implementer (09, 06d1db8), tests amendment (10, 2f822ff; decision 9), cleaner (11, 371a1af), spec checker (12, 4cf325b): 2 findings.
- Repair round 2 (of 3): spec author (13, c603e8a), decision 10 (reverses 7), spec amendment (14, 3c67247), decision 11.
- Tests (15, 07eebea): 4 red.
- Implementer (16, 52e13fe), cleaner (17, 381cbe7), spec checker (18, 772c0b4): no findings.
- Audit 2 (19, fbe729e; audited 4a00ace): #1 closed, #2 superseded, arch closed; new #3 (high), #4 (low) → decisions 12, 13.
- Repair round 3 (of 3, last): spec author done (20, 0dd23fe; new RH5202-006) → tests (21, 43881b1; 12 red) → test implementer (#3 members + boundary, #4) → implementer → cleaner → spec checker → auditor (repair only) → validator.
- (old next: auditor (repairs since 685ab42) → validator → publish.

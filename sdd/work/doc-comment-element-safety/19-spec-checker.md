Role: spec checker
Commit: 03f78ab
Verdict: #1329: consistent; #1389: consistent; #1241: consistent; #1344: consistent; #1242: consistent

Repair round 3 re-check of F1 of `17-spec-checker.md`. Inputs: `18-spec-author.md` and `git diff 9431ee8..HEAD`, which changes only `sdd/specs/formatter/cleanup.md` and work docs. No code or test changed.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| — | — | — | None. | — |

## Closed finding
- **F1 (#1389): closed.** FMT-CLEAN-009 now states that only a `U+FEFF` at the start of the text is a byte order mark, and that a `U+FEFF` on a line holding nothing but white space elsewhere is white space of that line, removed with it (FMT-CLEAN-001) and with the line itself where the formatter removes the blank line (before a closing brace, FMT-BLANK-007; before `else`, FMT-BLANK-008). I checked FMT-BLANK-007 and FMT-BLANK-008 in `formatter/blank-lines.md`: they say exactly that. A coverage sentence leaves a `U+FEFF` that starts the white space in front of a later token out, the Why gains the zero-width no-break space reasoning, and a second prose example shows both outcomes. They match the tests tagged `FMT-CLEAN-009` for this claim: `ByteOrderMarkCharacterOnBlankLineBefore{Else,Finally,OpeningBrace,ClosingBrace,Statement}IsRemoved` (for example, the `else` input removes the line and its `U+FEFF`, the statement input leaves an empty line) and `CleanupPhaseTests.RemovesByteOrderMarkCharacterOnBlankLineInTheMiddleOfTheText`. The `Uses` header gains FMT-BLANK-008, which the spec now references; `SpecStructureTests` pass.

## Checked (full checklist on the changed file)
1. Change doc: a clarification of behavior that already exists on `main`; none required. ok.
2. Specs match the diff: format intact (normative sentence first, `Why:`, example); no contradiction with the code from `aa9d904`. ok.
3. IDs: none added, retired or moved; ledger untouched. ok.
4. Owners: header owners unchanged (`CleanupPhase`, `EndOfLineWhitespace`). ok.
5. User documentation: no rule page is affected. ok.
6. Open questions: the spec author lists one new question (a `U+FEFF` that starts an indented code line elsewhere, kept by the Indentation phase on `main`). It is not written into the spec: the coverage sentence names the case as not covered without a verdict, which is a non-goal-style exclusion of an undecided case. `sdd/README.md` says an undecided case is not written into a spec, neither as a requirement nor as a non-goal. The sentence "a `U+FEFF` that starts the white space in front of a token on a later line is not covered" is a coverage sentence of the kind `sdd/README.md` permits for open-ended input sets, and it states no behavior. The question must still reach the maintainer in the lead's message before the auditor (escalation 1, found-in-passing): **lead action**, not a finding against the spec author.
7. Coverage: `scripts/test.sh --project architecture` on 03f78ab: `Passed!  - Failed:     0, Passed:    84, Skipped:     0, Total:    84`. No requirement added. ok.
8. Tags: no test added or changed this round; the eight mid-text tests now tag a requirement that states their claim. ok.
9. Nothing left behind: `ls sdd/work` shows only `doc-comment-element-safety`; the worktree is clean. The lead deletes the run folder before publishing. ok.

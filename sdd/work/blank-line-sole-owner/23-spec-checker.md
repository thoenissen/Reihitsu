Role: spec checker
Commit: 8e6dc59 (checked tree; this handover is committed on top)
Verdict: consistent

The prompt carried no intended fix or hoped-for verdict; none was used. Repair round 2 after the audit: diff a8048bd..8e6dc59 (specs: `formatter/blank-lines.md`, `formatter/switch-case-braces.md`; change doc; tests of 16, 18, 21).

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| _None._ | | | | |

Observation, not a finding: the caption of the third FMT-BLANK-020 example (`blank-lines.md`, "a complex element initializer that stays on one line") still describes the example's input; the requirement text decides by syntax (answer 1A), so nothing contradicts.

## Checked
1. ok. Change doc stays required and `Accepted`; no task list; `Visible output changes` present. Entries 12 and 18 are answered (1A), entry 13 (2A for the blank line between comment and `{`; the guidance for the blank line above the comment, with row 24), entries 14-17 and 19 are listed as covered by the clarified guidance (entries 14, 15, 16, 17 concern comments sharing a line with code; 19 the glued terminal `break`; 13 a comment behind `=`). FMT-SWITCH-007 is added to *Affected requirements*; the `Decisions` section records the answers, the guidance and its clarification.
2. ok. FMT-BLANK-011, -018, -020, FMT-SWITCH-007 (and the Uses row of `switch-case-braces.md`) match the implementer's description (19) and the tests: one glued predicate, FMT-BLANK-020 by syntax and widened trigger, FMT-BLANK-008 limited to the do-statement `while`, the two FMT-BLANK-018 readings written down. Format holds (`Why:`, examples).
3. ok. No ID added, removed or renumbered; `requirement-ids.txt` untouched.
4. ok. No owner changed this round; `switch-case-braces.md` Uses now lists FMT-BLANK-018. `scripts/test.sh --project architecture`: `Passed!  - Failed:     0, Passed:    92, Skipped:     0, Total:    92`.
5. ok. No `documentation/` change; the RH5025 and RH5020 pages need none (change doc says so).
6. ok. Spec author 15/17/20 and test implementer 16/18/21 list no open output questions; auditor findings 1 and 3 are answered (1A, 2A), the glued-comment and `=` cases are covered by the recorded guidance (rows 21-26). Follow-up candidates are routed in `00-lead.md`.
7. ok. Architecture result above. Every new active requirement is covered: FMT-BLANK-018 by the `GluedCommentBlankLineTests`/`CommentInsideConstructBlankLineTests` tests, FMT-BLANK-020 by `CommentBeforeBraceBlankLineTests` and the RH5024 formatter-parity test, FMT-BREAK-030 and FMT-BLANK-011 by `InsertsBlankLineAboveCommentBetweenAssignmentAndOpeningBraceOfAssignedCollectionInitializer`.
8. ok. All tests added or renamed this round carry the tags of the tables in 16, 18 and 21 (checked in the files: e.g. `...ArrayInitializerBraceOrArgumentListParenthesis` and `...DocumentationCommentLeadingBraceLine` carry FMT-BLANK-011 and -018; the new `AuthorsBlankLineAboveCommentInFrontOfTheTerminalBreakIsKept` carries FMT-SWITCH-007). The spec author's examples (FMT-BLANK-020 third example, FMT-SWITCH-007 corrected example, entries 12-19) are tagged on at least one test; the optional multi-line-comment example of 20 is covered by the existing `ChainLinkBehindMultiLineCommentTests` and `TrailingCommentWithNoBlankLineGainsOneAtEndOfFile`.
9. ok. `sdd/work/` holds only `blank-line-sole-owner`.

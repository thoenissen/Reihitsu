Role: spec checker
Commit: eb1e9f3
Verdict: consistent

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| | | | none | |

## Checked
1. Change doc: ok. `sdd/changes/2026-11/positional-pattern-close-blank.md` is required (FMT-BLANK-019 changes behavior), Status `Accepted`, has `Visible output changes` and no task list; `Decisions` records the maintainer's option A and covers changes 1 to 3.
2. Specs match the diff: ok. FMT-BLANK-019 names the positional clause's `)`; FMT-BLANK-009, -018, -020 follow; the empty-clause sentence and examples are in. FMT-INDENT-006 already names the positional delimiter, which the shared predicate now serves in both phases. No requirement contradicts the diff.
3. IDs: ok. No ID added, retired, or renumbered; `requirement-ids.txt` untouched.
4. Code owners: ok. `BraceBlankLinePolicy` and `CommentIndentationContributor` are existing owners; no type renamed. `sdd/specs/README.md` needs no change. `Uses` gained FMT-INDENT-025, which exists.
5. User documentation: ok. No `documentation/` change; no rule reports this blank line. RH3202 fix output is covered by a test.
6. Open questions: ok. 01, 03 and 04 report none; the only output question is answered under `Maintainer answers`. Follow-up candidates: none.
7. Coverage: `scripts/test.sh --project architecture` gave `Passed! - Failed: 0, Passed: 198, Skipped: 0, Total: 198`. Every changed or added requirement example (019 positional, no-element, argument-list; 020, 018, 009 via the change doc) has a tagged test.
8. Tags: ok. All 18 test methods in `PositionalPatternClosingParenthesisBlankLineTests` carry `[Requirement]` (FMT-BLANK-019, -020, -018, -009) matching the handover table; the RH3202 DataRows carry FMT-BLANK-019.
9. Nothing left behind: ok. `sdd/work/` holds only this run's directory.

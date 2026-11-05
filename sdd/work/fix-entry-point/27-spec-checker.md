Role: spec checker
Commit: 7b27a2d2
Verdict: consistent

My prompt carried no intended fix, suspected cause, or hoped-for verdict. I checked `git diff origin/main` and, for rows 24 and 25, `git diff f1192956`.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| - | - | - | none | - |

## Checked
1. Change doc: ok. `sdd/changes/2026-11/rh7101-fix-through-context-entry-point.md` stays `Status: Accepted` with `Visible output changes`, no task list. The new case 7 inputs (spanning `/** */`, spanning `/* */` plus `/** */`) show the formatter's layout without the blank line (answer 1a-1f A, SHARED-FIX-006), so no new maintainer question. The holding-side sentence uses the same wording as FMT-PIPE-029 ("something in the gap starts a line").
2. Specs match the diff: ok. The FMT-PIPE-029 fifth bullet states the shared-line decision as the phases' result read from the laid-out context, asked only where no line break separates the tokens. This matches `ReihitsuFormatter.FormatSplitFieldInContext` (`AreSeparatedByEndOfLine` precondition, `StartsLineInLayout`, second layout with the document column when the first field keeps its line). The "line breaks that count" (end-of-line, `///`, directive; not a line break inside a spanning comment) equals `SyntaxTriviaUtilities.EndsLine` and SHARED-BLANK-008. Row 25 is resolved: the spec says "nothing in the gap starts a line", the code does the same, and the `_p;/    /** x */ private int _x, _y;` example is present. Requirement format kept (one heading, normative sentence, `Why:`, examples).
3. IDs: ok. No ID added, renumbered, or removed since f1192956; `requirement-ids.txt` is untouched in that range. `Uses` gained SHARED-BLANK-008, which exists in the ledger and in `sdd/specs/shared/blank-line-spacing.md`.
4. Code owners: ok. No owner renamed, added, or removed. `scripts/test.sh --project architecture`: Passed! Failed 0, Passed 115, Skipped 0, Total 115 (includes SpecConsistencyTests).
5. User documentation: ok. No rule page changed since f1192956; `RH7101.md` is untouched there and carries no corner-case note.
6. Open questions: ok. Rows 24 and 25 carry no output question; both handovers (21, 24) state none, and the lead's decisions are recorded in `00-lead.md`. Nothing is recorded only in the PR.
7. Coverage: ok. FMT-PIPE-029 is the only requirement the delta changes; the delta adds covering `[Requirement]` tests in `ReihitsuFormatterFieldSplitTests` (12 spanning-doc-comment shapes, c5, c6, the row-25 pair) and in `RH7101DoNotCombineFieldsAnalyzerTests`.
8. Tags: ok. Every new test method carries `[Requirement]`: FMT-PIPE-029 (all formatter tests, with FMT-PIPE-010/018 where the test implementer's table lists them) and RH7101-004 (all 16 new fix tests). The helper `AssertFormattingAddsOnlyBlankLines` is not a test. Every example the spec authors named under `Examples for the tests` (A, B, C, D, c5, c6, the row-25 shape) is tagged on at least one new test.
9. Nothing left behind: ok. `sdd/work/` holds only `fix-entry-point`, which the lead deletes before publishing.

Notes (not findings): `26-cleaner.md` carries a `Commit:` line; the `FormatCommandHandler.cs` S1125 warnings the cleaner mentions are in a file this run does not change.

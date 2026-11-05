Role: spec checker
Commit: aa45e87a
Verdict: consistent

My prompt carried no intended fix, suspected cause, or hoped-for verdict. Checked `git diff origin/main` and, for the repair, `git diff 79ae25f3`.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| - | - | - | none | - |

## Checked
1. Change doc: ok. `sdd/changes/2026-11/rh7101-fix-through-context-entry-point.md` is `Status: Accepted`, has `Visible output changes`, no task list. The repair adds case 7 inputs `p` and `m` whose "after" output is the formatter's layout (answer 1a-1f A; the later field at the member column is case 8 / FMT-PIPE-029 already), so no new maintainer question. The false sentence about the line-break phase is corrected in the summary and in "The field split keeps the layout FMT-STRUCT-022 decides".
2. Specs match the diff: ok. FMT-STRUCT-026 (split writes the line break behind a comment that spans lines; one-line comments none) matches `FieldDeclarationSplitTransform` and `TokenGapUtilities.HasLineBreak`. FMT-PIPE-029's new gap bullet (decided by FMT-BREAK-007) matches `ReihitsuFormatter.FormatSplitFieldInContext` using `HasLineBreakBetween`. Requirement format (one heading, normative sentence, `Why:`, examples) kept; `Uses` gained SHARED-COMMENT-012 and FMT-BREAK-007 as the text cites them.
3. IDs: ok. The repair adds none; FMT-PIPE-029/030/031 stay at their sorted position in `requirement-ids.txt`, none reused or removed.
4. Code owners: ok. No owner renamed, added or removed (`TokenGapUtilities` is not a spec owner). `scripts/test.sh --project architecture`: Passed! Failed 0, Passed 115, Total 115 (includes SpecConsistencyTests).
5. User documentation: ok. No rule page changed; `RH7101.md` Correction is unchanged for the plain case and carries no corner-case note.
6. Open questions: ok. The repair has no output question; the SHARED-COMMENT-012 / FMT-BREAK-007 member contradiction is routed to open issue #1572 (15-spec-author), not written into the spec. Follow-up candidates are listed in `00-lead.md`; decision 23 (`EndOfLineWhitespace` stays banned) is recorded.
7. Coverage: ok. Every active requirement the repair changes (FMT-STRUCT-026, FMT-PIPE-029) has new covering `[Requirement]` tests in the formatter tests and RH7101 analyzer tests.
8. Tags: ok. All 28 tests the repair adds carry `[Requirement]`: FMT-STRUCT-026 (12 tests, phase and document level), FMT-PIPE-029 with FMT-PIPE-010 where the table lists it (5), RH7101-004 (5, the fix side of the same examples). The test implementer's table names every one. Every spec-author example (three-declarator example, `a`/`b`/`c`/`h` and the two-comment shapes, `p`/`q`/`pq`/`m`) is tagged on at least one test.
9. Nothing left behind: ok. `sdd/work/` holds only `fix-entry-point`, which the lead deletes before publishing.

Note for the lead (not a finding): the new architecture ban set intentionally includes `EndOfLineWhitespace` (decision 23).

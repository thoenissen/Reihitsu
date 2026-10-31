Role: spec checker
Commit: c081723
Verdict: consistent

My prompt carried no intended fix, suspected cause, or hoped-for verdict to ignore.

Scope: `git diff 6285aee..HEAD`. The compiled change is `RawStringLineShifts` (a content line that starts with a hole's `{` gets its moved indentation as literal text); the tests are the new `RawStringMovedLineStartTests` (29 members). The diff touches no file under `sdd/specs/` and no change doc.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| _None._ | | | | |

## Checked
1. Change doc: ok. The round-2 change brings the code in line with the existing FMT-PIPE-021 (phase trees round-trip) and FMT-PIPE-022; no intended behavior changes and the final output is unchanged, so it is a bug fix and needs no new change doc. `sdd/changes/2026-10/phase-tree-round-trip.md` stays Accepted; its line that the move "edits the literal's tokens" is not contradicted (the fix still edits tokens and adds one text element, no reparse, as decision `2026-10-31-phase-tree-round-trip-raw-string-hole-line.md` chose).
2. Specs match the diff: ok. FMT-RAW-001 to FMT-RAW-004 (the move distances and the unchanged value) and FMT-PIPE-021/022 describe the behavior; no requirement says indentation in front of a hole is trivia. `raw-string-alignment.md` states no token-level mechanism.
3. IDs: ok. No ID added, changed, or removed; `requirement-ids.txt` lists FMT-RAW-001 to FMT-RAW-004.
4. Code owners: ok. `RawStringLineShifts` still exists and is listed in `raw-string-alignment.md` and `sdd/specs/README.md`; no type was added, renamed, or removed.
5. User documentation: ok. No rule page is affected.
6. Open questions: ok. The question went to the maintainer as escalation 12 and is answered in the decision file; the two follow-up candidates it names (`///` separator wording, direct Core tests for the would-merge helper) are left out by that decision.
7. Coverage: ok. No new requirement. `scripts/test.sh --project architecture`: `Passed!  - Failed: 0, Passed: 89, Skipped: 0, Total: 89`.
8. Test tags: ok. All 29 methods in `RawStringMovedLineStartTests` carry `[Requirement]` for exactly the IDs in `19-test-implementer.md` (FMT-PIPE-021, FMT-PIPE-022, and one of FMT-RAW-001/002/003/004 each, checked member by member). The table names every ID the tests carry.
9. Left behind: ok. `sdd/work/` holds only `phase-tree-round-trip/`; the lead deletes it before publishing.

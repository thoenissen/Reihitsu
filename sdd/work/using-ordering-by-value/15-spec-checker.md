Role: spec checker
Commit: 8a24111
Verdict: consistent

Scope: repair round 2, `git diff 25baa47 HEAD` excluding `sdd/work/` (3 specs, 3 test files; no production code, no ledger, no change doc, no rule page). The rest was checked in 08 and 10.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|

## Checked
1. Change doc: ok. Clarification of the sort key per Q2 (maintainer answered A, recorded in `00-lead.md`); the visible output change is decided by that answer, so no change doc is required in the bug-fix lane.
2. Specs match the diff: ok. SHARED-USING-004 and SHARED-USING-007 state that a leading `global::` is dropped from the root and the sort key; the RH7203-001/-002 and RH7206-001 examples and the SHARED-USING-007 formatter example match the added tests (spelling kept, `System; System.Collections; global::System.IO`). Markup `{|RH7203:...|}` / `{|RH7206:...|}` present. Not a contradiction: SHARED-USING-004 says `global :: System.IO` has key `System.IO`, consistent with Q2.
3. IDs: ok. No ID added, retired, or renumbered; `requirement-ids.txt` untouched.
4. Code owners: ok. No owner header changed; `scripts/test.sh --project architecture`: Passed 118, Failed 0, Total 118.
5. User documentation: ok. No rule page affected; the pages do not describe sort-key spelling.
6. Open questions: ok. Q2 answered under `Maintainer answers`; 12 reports no open output question; the Review-notes mention is the lead's task for the PR.
7. Coverage: ok. No new requirement ID; the architecture suite passes (118/118).
8. Tags: ok. All 7 added tests carry their requirement (RH7203-001, RH7203-001+004, RH7203-002, RH7206-001, RH7206-002, SHARED-USING-007 x2), matching the table in 13; every requirement named under "Examples for the tests" in 12 (SHARED-USING-007, RH7203-001/-002, RH7206-001) is tagged on an added test.
9. Nothing left behind: ok for this stage; only this run's `sdd/work/using-ordering-by-value/` exists, to be deleted by the lead before publishing.

Role: spec checker
Commit: (this commit)
Verdict: consistent

Scope: `efa7480..6990d08` (decisions 17 and 18). The prompt carried no intended fix or hoped-for verdict. Earlier checks in `15-spec-checker.md` stand where this change does not touch them.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| _None._ | | | | |

## Checked
1. Change doc: ok. Wording-only edits to the existing accepted doc; `Status` stays `Accepted`, no visible output change, no new decision needed. The FMT-PIPE-028 change is a clarification (behavior equals `main` and unchanged FMT-PIPE-019/FMT-TAB-002/FMT-EOL-002).
2. Specs match the diff: ok. FMT-PIPE-028 no longer lists the preparation phase's effects; the tab example still shows both halves; the region non-goal, change doc (What, code-fix section, decision section) and code comments all refer to FMT-PIPE-028 / FMT-PIPE-019 without a list. Grep over `sdd/specs`, `sdd/changes` (this run), `AGENTS.md`, `documentation`, `src` finds no remaining "only the tab", "one change left", or "still becomes four spaces" claim. Observation, not a finding: the change doc's "The region phase owns the line form" says "Only the tab replacement and the region phase reach that text" (the separator between directive tokens); that is about the separator only and is true, so it is not the exhaustive-list class.
3. IDs: ok. No ID added, changed, or removed; `requirement-ids.txt` untouched.
4. Code owners: ok. No spec header touched; `SpecConsistencyTests` passes within the architecture run.
5. User documentation: ok. No rule page affected.
6. Open questions: ok. The spec author and test implementer record none; decision 17 and 18 are in `00-lead.md`.
7. Coverage: ok. `scripts/test.sh --project architecture`: `Passed! - Failed: 0, Passed: 92, Skipped: 0, Total: 92`. No new requirement; FMT-PIPE-028 and FMT-EOL-002 are covered by the three new tests.
8. Tags: ok. The three added tests (`FormatNodeInDocumentAsyncRewritesForeignLineBreaksEndingRegionDirectives`, `FormatNodeInDocumentWithContextAsyncRewritesForeignLineBreaksEndingRegionDirectives`, `FormatNodeRewritesForeignLineBreaksEndingRegionDirectives`) each carry `[Requirement("FMT-PIPE-028")]` and `[Requirement("FMT-EOL-002")]`, matching the test implementer's table; the helper is not a test. The spec author's "Examples for the tests" (the tab example, and the line-ending case derived from FMT-PIPE-028 with FMT-PIPE-019/FMT-EOL-002) are tagged FMT-PIPE-028 on existing and new tests.
9. Nothing left behind: ok for now. `sdd/work/` holds only `region-directive-separator` (the lead deletes it before publishing).

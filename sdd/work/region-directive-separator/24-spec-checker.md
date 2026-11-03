Role: spec checker
Commit: (this commit)
Verdict: consistent

Scope: `3010b6a..00ebce2` (decision 19: audit findings 6a, 6b, 7). The prompt carried no intended fix or hoped-for verdict. Earlier checks in `15-spec-checker.md` and `20-spec-checker.md` stand where this repair does not touch them.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| _None._ | | | | |

## Checked
1. Change doc: ok. Wording-only edits (title, third *What* bullet, one section heading) to the existing doc; `Status` stays `Accepted`, no visible output change, no new decision. No requirement is added, changed, or retired.
2. Specs match the diff: ok. The third *What* bullet now says what FMT-PIPE-028 says (no region requirement at node level; the preparation phase still runs and does what it does to any directive, FMT-PIPE-019) and lists no effect of that phase, so it no longer contradicts the FMT-PIPE-028 tab example (6b). The test-class summary says "apply no region requirement (FMT-PIPE-028)" (6a). The `FormatNodeInDocumentWithContextAsync` remark again carries "because this entry point serves code fixes; for the same reason, no region requirement is applied (FMT-PIPE-028)", matching its two sibling remarks (7). Grep over the change doc, `pipeline.md`, `region-formatting.md`, `ReihitsuFormatter.cs`, the pipeline sources, the region test class, and `AGENTS.md` finds no remaining claim that node-level formatting keeps a region directive as written; the leftover "as written" hits speak about whole-document formatting or other rules. The renamed headings have no inbound links (spec author, 23).
3. IDs: ok. `requirement-ids.txt` untouched.
4. Code owners: ok. No spec header touched.
5. User documentation: ok. No rule page affected.
6. Open questions: ok. None in 22 and 23; decision 19 is in `00-lead.md`.
7. Coverage: ok. `scripts/test.sh --project architecture`: `Passed! - Failed: 0, Passed: 92, Skipped: 0, Total: 92`. No new requirement.
8. Tags: ok. No test added or retagged; the only test-file edit is the class summary.
9. Nothing left behind: ok for now. `sdd/work/` holds only `region-directive-separator`, which the lead deletes before publishing.

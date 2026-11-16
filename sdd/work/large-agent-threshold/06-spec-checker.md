Role: spec checker
Commit: 5c8cc56
Verdict: consistent

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| _None._ | | | | |

## Checked
1. Change doc: ok. The run touches no spec, so the strict rule does not require one; the doc `sdd/changes/2026-11/squad-large-agent-by-context-growth.md` follows the process-change precedent (`squad-resume-or-fresh-and-lead-compaction.md`, `Affected requirements: none — process change`) and lead decision 1. It has no task list or plan, has `Visible output changes` (none, stated), Status `Accepted`, and its `Depends on` link resolves. No visible output change is listed, so no maintainer answer is owed.
2. Specs match the diff: ok. No `sdd/specs/` change. The doc's *What* matches the diff: `squad/lead.md` section 4 (50k cold / 150k warm, judged at the follow-up, waiting follow-up and last response just under 5 min count as cold, retirement at the warm number, old 40-response and average signals and "roughly twice" gone), `scripts/usage.sh` (four `--json` fields per spawn, lead row included, `null` without a response; Markdown *Context growth* column repeated per tier row), and `scripts/README.md`.
3. IDs: ok, not applicable (no requirement added, `requirement-ids.txt` untouched).
4. Code owners: ok, not applicable (no spec header touched); no `src/` change.
5. User documentation: ok, not applicable (no rule page affected). `rg` finds no other statement of the old thresholds outside `squad/notes/`, so the size rule has one owner.
6. Open questions: ok. 03 lists none; 01 and 02 raised none that remain open; lead decisions 4 and 8 route the skeptic's option and the dropped follow-up. Nothing is recorded only in the PR.
7. Requirement coverage: ok, not applicable (no `sdd/specs/` diff, so the architecture project was not run).
8. Test tags: ok, not applicable (no test added; lead decision 7).
9. Nothing left behind: ok. `sdd/work/` holds only `large-agent-threshold`, which the lead deletes before publishing.

## Follow-up candidates
_None._

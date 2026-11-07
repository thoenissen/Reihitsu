Role: spec checker
Commit: faa36db
Verdict: consistent

My prompt carried no intended fix or hoped-for verdict. Repair round 1 check: finding 1 of 10 against decision 18. Since 6801683 the diff changes only `sdd/changes/2026-11/raw-string-tab-one-column.md` and handovers/`00-lead.md` under `sdd/work/`; no spec, ID, owner, test, or rule page.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|

None. Finding 1 of 10 is resolved:
- `Affected requirements` now lists FMT-RAW-002 (changed).
- *What* states the tab is the only extension into the non-goal and the only behavior change; a separate bullet states main counted only U+0020 and that counting/removing U+00A0, U+2003, vertical tab, form feed is the #1611 bug fix. This matches FMT-RAW-001 (measure for every whitespace character) and FMT-RAW-002 (leftward shift removes any leading whitespace, leftover characters stay) in the spec, and RH5202-003 ("removing that much leading whitespace") and SHARED-FIX-007 (formatter agrees with the fix), which are unchanged.
- *Why* and *Visible output changes* say the non-tab output is a defect fix, so no decision entry is owed; the two listed visible changes still map to Q2 and Q1 in `Decisions`.
- Nothing new introduced: status Accepted, no task list, no new requirement.

## Checked
1. Change doc: ok (finding 1 resolved; Accepted; `Visible output changes` present; Q1 and Q2 decisions match `00-lead.md`).
2. Specs match the diff: ok, unchanged since 10.
3. IDs: ok, unchanged.
4. Code owners: ok, unchanged.
5. User documentation: ok, no `documentation/` change.
6. Open questions: ok, none new.
7. Coverage: ok. `scripts/test.sh --project architecture`: "Passed! - Failed: 0, Passed: 115, Skipped: 0, Total: 115".
8. Tags: ok, unchanged since 10.
9. Nothing left behind: ok, `sdd/work/formatter-layout-fixes/` is the only `sdd/work/` (the lead deletes it before publishing).

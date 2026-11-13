Role: spec checker
Commit: b51ec90
Verdict: consistent

Repair round 1, run diff `c2866ea..HEAD`. The prompt carried no intended fix, suspected cause, or hoped-for verdict. Issues #1416, #1388, #1627, #1142 are all `agent-filed`. Finding 1 of 11 (change doc) is resolved by `sdd/changes/2026-11/brace-fix-layout-line-endings.md`.

`scripts/test.sh --project architecture`: `Passed!  - Failed: 0, Passed: 117, Skipped: 0, Total: 117`.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| - | - | - | none | - |

## Checked, per issue

### #1416 (brace fixes keep the statement's layout)
- Change doc: present, `Status: Accepted`, has `What`, `Why`, `Visible output changes`, `Decisions`, `Alternatives considered`; no task list. Header/blank-line and `else if` column outputs are listed (blocks 1, 7) and sit under SHARED-FIX-006/007 plus maintainer answers 1 to 3, 6 and #1220 Q3, all recorded in `Decisions`. Lead decision 16 accepted the two outputs no answer names (header/blank lines kept, multi-line header keeps `&&`), which the doc says are demanded by unchanged SHARED-FIX-006; that is the lead's call and is recorded in `00-lead.md`, not a mismatch.
- Specs: SHARED-BRACE-002/-003 match the doc's What section; Affected requirements lists every requirement the spec diff touches.
- IDs: SHARED-BRACE-003 added at its sorted place in `requirement-ids.txt`; nothing removed.
- Rule pages: none in the diff; the doc states they are unchanged; no withheld-fix note on a page.
- Open questions: answers 1 to 6 are in `00-lead.md` and in the doc's `Decisions`.
- Tests: covered and tagged (architecture `EveryRequirementOfAnEnforcedSpecIsCovered` passes; earlier check 11 found all added tests tagged).

### #1388 (fixes keep the document's line ending)
- Change doc: lists RH5011/RH8304 lone-`\r` and mixed documents, RH7309 moved block, and brace-fix line endings, with before/after; decisions 8, 9, 11 recorded in `Decisions`. Characterization-only instances (RH5405 CRLF comment, RH7004) are listed under `Unchanged`.
- Specs: SHARED-FIX-008 (changed), RH5011-004, RH8304-004 (clarified), RH7309-007 (added) match the doc; RH7309-007 inserted in the ledger in sorted position.
- Open questions: the FormatNode instance is dropped (decision 3); the write-back follow-up and RH8402 are under `Follow-up candidates`.

### #1627 (trailing whitespace behind the body)
- Change doc: block "whitespace at the end of the body's line" and the Decisions entry for answer 4; SHARED-BRACE-002 states the behavior. Consistent.

### #1142 (attribute-list split fixes change only the reported list)
- Change doc: blocks for shared-line shapes, Fix All, tab, CRLF, accessor-list `{`; Decisions record answer 7 (widened by lead decision 14 to SHARED-ATTR-022), #1109 Q1 for tab and Fix All. All 14 split-rule requirements the spec diff touches (-003/-004/-005/-006) and SHARED-ATTR-017/-022 are in `Affected requirements`.
- IDs: SHARED-ATTR-022 added at its sorted place; no ID reused or removed.
- Rule pages: unchanged, as stated.

### Whole run, by check
1. Change doc: ok (present, Accepted, Visible output changes, Decisions with maintainer answers 1 to 7).
2. Specs match the diff: ok.
3. IDs: ok.
4. Code owners: ok (`SpecConsistencyTests` pass; no owner or spec added).
5. User documentation: ok.
6. Open questions routed: ok.
7. Requirements covered: ok (architecture run above).
8. Tests carry their tag: ok (unchanged since 11; the diff since then is docs and work files only).
9. Nothing left behind: ok for now; `sdd/work/` holds only this run's directory, which the lead deletes before publishing.

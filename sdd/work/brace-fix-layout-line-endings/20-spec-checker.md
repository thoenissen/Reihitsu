Role: spec checker
Commit: c02e39b
Verdict: consistent

Repair round 2, run diff `166e3dd..HEAD`. The prompt carried no intended fix, suspected cause, or hoped-for verdict. Issues #1416, #1388, #1627, #1142 are all `agent-filed`. Round 2 added: SHARED-BRACE-002 end-of-file clarification (decision 17), SHARED-ATTR-022 widened (18), SHARED-ATTR-023 added (19), tests and code for them.

`scripts/test.sh --project architecture`: `Passed!  - Failed: 0, Passed: 117, Skipped: 0, Total: 117`. This includes `EveryRequirementOfAnEnforcedSpecIsCovered` (red for SHARED-ATTR-023 until test implementer 16) and `SpecConsistencyTests`.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| - | - | - | none | - |

## Checked, per issue

### #1416 (brace fixes keep the statement's layout)
- Change doc `sdd/changes/2026-11/brace-fix-layout-line-endings.md`: `Accepted`, has `Visible output changes` and `Decisions`, no task list. The What bullet states the end-of-file body; the "Unchanged" list shows its output equal on `main` and now; Decisions has the decision 17 entry. Specs: SHARED-BRACE-002 clarified (end-of-file paragraph and example), no contradiction with the diff.
- Tests: five `[Requirement("SHARED-BRACE-002")]` tests for the end-of-file case (16), corrected by 18 (decision 21: both DataRows hold a line break so FMT-PIPE-007 detects the ending). Tagged.

### #1388 (fixes keep the document's line ending)
- No change in round 2 beyond the unchanged RH7309-007 and SHARED-FIX-008. The F4 test (`FixIsOfferedForALineFeedInABlockThatKeepsItsPlaceOfACarriageReturnLineFeedDocument`) is tagged RH7309-004 and RH7309-007 and is listed in 16's table.

### #1627 (trailing whitespace behind the body)
- The end-of-file whitespace test `FixKeepsTrailingWhitespaceOfABodyThatEndsTheDocumentOnTheBodyLine` is tagged SHARED-BRACE-002; the change doc's whitespace block is unchanged and consistent.

### #1142 (attribute-list split fixes)
- SHARED-ATTR-022 widened (type, namespace, local-function block) and SHARED-ATTR-023 added; all 14 split-rule withhold requirements now cite both, and the change doc's Affected requirements lists all 14 plus SHARED-ATTR-023, with the new Visible output entries (type/namespace/block `{`, target-specifier line break, other line ending inside the list) and Decisions for decisions 18 and 19. Alternatives considered lists the two rejected options.
- IDs: SHARED-ATTR-023 inserted at its sorted place after SHARED-ATTR-022 in `requirement-ids.txt`; nothing reused, renumbered, or removed.
- Tests: SHARED-ATTR-023 carried by 7 methods in RH5514 and 5 in RH5518 tests (withheld and offered sides, lone-`\r`); SHARED-ATTR-022 tests tagged in RH5506, RH5514, RH5518, RH5526. Every requirement the spec author listed under `Examples for the tests` is tagged on at least one added test, and 16's table names the tags the added tests carry.

## Whole run, by check
1. Change doc: ok (Accepted; decisions 17 to 19 recorded; every visible output change is a maintainer answer, an unchanged requirement, or a lead-recorded rare-class withhold).
2. Specs match the diff: ok.
3. IDs: ok.
4. Code owners: ok (`SpecConsistencyTests` pass; no spec or owner added or removed in round 2).
5. User documentation: ok (no rule page in the diff, and the change doc states they are unchanged; no withheld-fix note on a page).
6. Open questions routed: ok (spec author 15 lists none and the follow-up candidates are in `00-lead.md`).
7. Requirements covered: ok (architecture run above).
8. Tests carry their tag: ok.
9. Nothing left behind: ok for now; `sdd/work/` holds only this run's directory, which the lead deletes before publishing.

Role: spec checker
Commit: eb50771
Verdict: consistent

Repair round 1 checked (`git diff f36127b HEAD`: SHARED-ATTR-021 text, six RH6023 test tags, the RH5513 tab test). Prompt carried no intended fix or verdict. Architecture project on this head: `Passed!  - Failed: 0, Passed: 117, Skipped: 0, Total: 117`.

- thoenissen/Reihitsu#1140: nothing to report.
- thoenissen/Reihitsu#1144: nothing to report.
- thoenissen/Reihitsu#1145: nothing to report.
- thoenissen/Reihitsu#1393: nothing to report.
- thoenissen/Reihitsu#1511: nothing to report.
- thoenissen/Reihitsu#1561: nothing to report.
- thoenissen/Reihitsu#1141: finding 1 of 09 resolved. No `FMT-PIPE-024` tag remains in the RH6023 test file. The six tests carry RH6023-002 plus SHARED-OPSPACE-004 (five newly added, one already had it), which matches SHARED-OPSPACE-004's sentence and the sibling tests.
- thoenissen/Reihitsu#1094: finding 2 of 09 resolved. The RH5513 tab test is tagged RH5513-004, SHARED-ATTR-008 and SHARED-ATTR-021. SHARED-ATTR-021 now states that a tab counts as four spaces and any other whitespace character as one. The code agrees (`TargetAttributePlacementCodeFixProviderBase`, line 208: `obj == '\t' ? IndentSize : 1` against `ComputeBaseIndentLevel(ancestor) * IndentSize`). The new tab example (namespace, class, method block, braces on two tabs = eight columns, fix offered; two spaces would withhold it) is the input of the tagged test, and the test's expected output is the SHARED-ATTR-008 result. The spec change is a clarification of existing behavior, so no change doc and no new ID (ledger unchanged, correct).

## Whole run

### Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| 1 | 1 (judgement, carried from 09 finding 3) | PR description | Still open for the lead: no change doc for RH5204-006, RH5203-003, RH5103-005, RH5205-004, SHARED-ATTR-021. Decisions 13, 7-9 and 24 record the reason; the PR's Review notes must state it. | lead |

### Checked
1. Change doc: unchanged from 09; ok under decisions 13 and 24 (judgement, finding 1). The repair adds none and needs none.
2. Specs match the diff: ok; the SHARED-ATTR-021 clarification matches the code and its tagged test (format kept: normative sentence first, Why, examples).
3. IDs: no ID added, reused, renumbered or deleted in the repair; ok.
4. Code owners: untouched; ok (SpecConsistencyTests in the 117).
5. User documentation: no rule page changed; ok.
6. Open questions: none new; the test implementer's tab observation is closed by decision 26; ok.
7. Coverage: architecture 117/117 passed; SHARED-ATTR-021 still has covering tests.
8. Tags: 09 findings 1 and 2 resolved; the test implementer's table names every requirement the changed tags carry; the spec author's tab example is tagged on a test.
9. Left behind: only this run's `sdd/work/fix-scope-and-layout/`; the lead deletes it before publishing.

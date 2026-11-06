Role: spec checker
Commit: 204bf60f
Verdict: consistent

Repair round 1. Checked the whole diff against `origin/main`, with emphasis on `af76b48e..204bf60f` (specs RH3004, RH3005, RH3101, RH3105, RH3106, SHARED-FIX-002; the five test classes; the implementer's and cleaner's files). My prompt carried no intended fix or verdict to set aside.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| _None blocking._ | | | | |

Observations (not findings; no requirement or example names them):
- RH3004-006, disabled-before-active example: the spec says the output is the same with and without `DEBUG`; `FixMeasuresTheShiftOnADisabledStatementInFrontOfTheFirstActiveStatement` runs only without `DEBUG`. The "with DEBUG" half is untested (the region example is tested both ways).
- RH3106-003 says a conditional, switch arm, or array element with a delegate-typed other branch is reported "in every position"; the tests pin the `var` initializer only. `object o = b ? delegate () { } : someAction;` (reported by the implementation, per 11-implementer) has no test.

## Checked
1. Change doc: ok. No `sdd/changes/` file; bug-fix lane, no visible output change listed. Rows 19-25 are clarifications of SHARED-FIX-002/-003/-007, FMT-INDENT-009 and maintainer answers 2, 4, 6, 7 (row 24 and 25 recorded in `00-lead.md` as decided on recommendation under the standing instruction).
2. Specs match the diff: ok. RH3004-006 (moved code keeps its layout, one delta measured on the first statement line incl. disabled text, column-0 directives stay, `#region`/`#endregion` and non-column-0 directives move, one-line block per answer 6 A) agrees with the examples' columns and with the implementer's `MoveStatementsAsync`/`IsColumnZeroDirectiveLine`/`IsDisabledStatementLine`. RH3106-002 (operators, Fix All) and RH3106-003 (natural-type positions, delegate-typed neighbor under `var`) agree with the analyzer and fix changes. SHARED-FIX-002 sentence and the RH3005/RH3101/RH3105 Fix All examples agree with `DocumentOrderFixAllProvider`. Requirement format (heading, normative sentence, `Why:`, markup examples) held; no open question written into a spec.
3. IDs: ok. No new ID since af76b48e; `requirement-ids.txt` untouched this round; nothing reused, renumbered, or deleted.
4. Code owners: ok. `scripts/test.sh --project architecture`: "Passed!  - Failed:     0, Passed:   115, Skipped:     0, Total:   115". Owner lines of the touched specs unchanged; `DocumentOrderFixAllProvider` is not a spec owner; `sdd/specs/README.md` needs no change.
5. User documentation: ok. No rule page changed since af76b48e; RH3004.md Correction (`(int value) => value + 1`) still equals the fix output, no formatter mention, footer present; RH3101/RH3103 pages as in handover 07.
6. Open questions: ok. 09/10/11 list no open output question; rows 24 and 25 are recorded as decided on recommendation; follow-up candidates (incl. the inactive column-0 directive check and the layout-only formatter mode) are under `Follow-up candidates` in `00-lead.md`.
7. Coverage: ok. No new requirement ID this round; every changed requirement (RH3004-006, RH3005-008, RH3101-005, RH3105-005, RH3106-002/-003, SHARED-FIX-002) has covering `[Requirement]` tests in the diff.
8. Tags: ok. Every test added since af76b48e carries the tags `10-test-implementer.md` lists (RH3004-003/-006, RH3005-008 + SHARED-FIX-002, RH3101-005 + SHARED-FIX-002, RH3105-005 + SHARED-FIX-002, RH3106-001/-002/-003 + SHARED-FIX-002); every requirement under `Examples for the tests` in 09-spec-author is tagged on at least one test (see the two observations above for untested sub-cases only).
9. Nothing left behind: ok for now. `sdd/work/` holds only `withhold-unsafe-fixes`; the lead deletes it before publishing. No tracker numbers or TODOs in changed non-test source.

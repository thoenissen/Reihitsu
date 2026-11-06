Role: spec checker
Commit: 21d4b48a
Verdict: consistent

Repair round 2. Checked the whole diff against `origin/main`, with emphasis on `2284decb..21d4b48a` (specs RH3004, RH3106, SHARED-FIX-002, `requirement-ids.txt`; the RH3004, RH3005, RH3106 test classes; `RewriteBindingUtilitiesTests`; the implementer's and cleaner's files). My prompt carried no intended fix or verdict to set aside.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| _None._ | | | | |

## Checked
1. Change doc: ok. No `sdd/changes/` file; bug-fix lane. Rows 26-35 are clarifications of SHARED-FIX-002/-003/-007, SHARED-DIAG-011 and maintainer answers 2 and 4, or decided on recommendation under the standing instruction (rows 28, 29, 31, 33 recorded as such in `00-lead.md`).
2. Specs match the diff: ok. RH3004-003/-006/-007 (raw-string withhold, shared-brace-line and column-0-crossing leftmost rule, column-0 comment/directive lines) agree with the tests' expected columns and the implementer's `MoveStatementsAsync`/`StaysInColumnZero`/`ContainsMultiLineRawStringLiteral`. RH3106-002/-003 (outcome-based) agree with the removal of the position lists and `RewriteBindingUtilities`. SHARED-FIX-002's nested-instance sentence agrees with `DocumentOrderFixAllProvider` on RH3004/RH3005/RH3101/RH3105. Format held (heading, normative sentence, `Why:`, markup examples); no open question written into a spec.
3. IDs: ok. Only `RH3004-007` added, at its sorted position (after `RH3004-006`); nothing reused, renumbered, or deleted.
4. Code owners: ok. `scripts/test.sh --project architecture`: "Passed!  - Failed:     0, Passed:   115, Skipped:     0, Total:   115". Spec `Uses` rows consistent (the test passes). `DocumentOrderFixAllProvider` and `RewriteBindingUtilities` are not spec owners; `sdd/specs/README.md` needs no change.
5. User documentation: ok. No rule page changed since 2284decb (branch-wide only RH3101.md and RH3103.md, as checked in handover 07/13). No page note about a withheld fix was added for RH3004-007 or RH3106-002.
6. Open questions: ok. 15/16/17 list no open output question; rows 28, 29, 31, 33 are recorded as decided on recommendation; follow-up candidates (raw-string shared helper, the comment-continuation shift measurement) are under `Follow-up candidates` in `00-lead.md`.
7. Coverage: ok. `RH3004-007` is covered by seven `[Requirement("RH3004-007")]` tests (architecture test passes). Every changed requirement has covering tests.
8. Tags: ok. Every test added since 2284decb carries the tags listed in `16-test-implementer.md` (RH3004-006/-007, RH3005 + SHARED-FIX-002, RH3106-001/-002/-003). The disabled-before-active example is now tested both with and without `DEBUG` (closes the observation in 13). `RewriteBindingUtilitiesTests` verifies a shared helper, not a requirement, so it carries no tag by design (row 35). Every requirement under `Examples for the tests` in 15-spec-author is tagged on at least one test; the `Run<T>(Func<T>)` case was dropped by row 34 (does not compile), with no spec example naming it.
9. Nothing left behind: ok for now. `sdd/work/` holds only `withhold-unsafe-fixes`; the lead deletes it before publishing. No tracker numbers or TODOs in the changed non-test source or specs.

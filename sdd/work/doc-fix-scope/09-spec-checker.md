Role: spec checker
Commit: c6b0a5c
Verdict: consistent

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| _None._ | | | | |

## Checked
1. Change doc: ok. No requirement changes intended behavior: the new requirements are bug-fix clarifications or rare-class withholding (decisions 13, 21, 22); no rule, phase, or surface is added or removed. No change doc is needed and none exists. The one output question (Q1) is answered A in `00-lead.md`.
2. Specs match the diff: ok. RH8305-005, RH8307-005/-006, RH8309-006/-007, RH8401-005, RH8402-006, FMT-PIPE-032 are each one per heading and present in the specs; the amended RH8201-005, RH8202-001, RH8204-003, RH8305/7-003/-004, RH8308, FMT-PIPE-009 and the unsupported-syntax text match the changed code and tests.
3. IDs: ok. The 8 new IDs follow the next free numbers, the ledger is sorted (`sort -c`), and nothing is removed or renumbered.
4. Code owners: ok. `SpecConsistencyTests` passed within the architecture run. The new helper types are members of existing owners, and no owner list needed a change.
5. User documentation: ok. `RH8401.md` lost its corner-case section, the rest is consistent, and no other rule page is affected. The footer is intact and the formatter is not mentioned. The RH8401 and RH83xx/84xx ranges fit the Documentation category.
6. Open questions: ok. The only output question (#1342, behind-code comments) is answered in `00-lead.md`. Every case outside the issues sits under *Follow-up candidates* there: the accessor RH8309 case, the double report on primary-constructor types, and the RH8307 `/** */` report. None is written into a spec.
7. Coverage: ok. `scripts/test.sh --project architecture`: "Passed!  - Failed: 0, Passed: 117, Skipped: 0, Total: 117". Every new requirement (all 8) has at least one `[Requirement]` test in the diff.
8. Tags: ok. Every requirement named in the test implementers' tables or under *Examples for the tests* is tagged on a test the diff adds (the 22 IDs tagged are RH8201-005 through FMT-UNSUP-002 as listed in 03 and 06), including RH8305-003 and RH8307-003 for the positional-record rows and FMT-PIPE-032 on the new formatter test class.
9. Nothing left behind: ok. `sdd/work/` holds only `doc-fix-scope/`, which the lead deletes before publishing.

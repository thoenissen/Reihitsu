Role: spec checker
Commit: dfea6e6
Verdict: consistent

The prompt carried no intended fix, suspected cause, or hoped-for verdict; nothing was ignored.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| _None._ | | | | |

## Checked
1. Change doc: ok. `node-root-detached.md` is Accepted, has `Visible output changes`, no task list; the RH5204 and zero-offset outputs carry the maintainer's answers (6, 4) in `Decisions`. `supported-inputs.md` (process, no affected requirements) is Accepted and matches the `AGENTS.md` and `squad/` text in the diff.
2. Specs match the diff: ok. FMT-PIPE-023 and FMT-PIPE-024 have the normative sentence, `Why:` and examples; FMT-CLEAN-008 and FMT-PIPE-010/012 are amended consistently with the code and tests.
3. IDs: ok. FMT-PIPE-023 and FMT-PIPE-024 are inserted in sorted position in `requirement-ids.txt`; none reused, renumbered, or deleted.
4. Code owners: ok. `PrecedingTokenFacts` and `RootListPosition` were added to the pipeline header and to `sdd/specs/README.md`; the architecture project passed, `SpecConsistencyTests` included.
5. User documentation: ok. No rule page is affected and none changed.
6. Open questions: ok. Maintainer answers 1-7 in `00-lead.md` cover the output questions; the spec author's handovers 06 and 07 record none open; decision 9 dropped the follow-up candidates, and #1521 and #1527 are filed.
7. Coverage: ok. `scripts/test.sh --project architecture`: Passed! - Failed: 0, Passed: 92, Skipped: 0, Total: 92. FMT-PIPE-023 (9 tests) and FMT-PIPE-024 (16 tests) are covered.
8. Tags: ok. The tag counts in the diff match the `Tests` table of `04-test-implementer.md` (FMT-PIPE-023: 9, FMT-PIPE-024: 16, FMT-PIPE-012: 12 with the changed test, FMT-CLEAN-008: 1). The four code-fix tests of `08-test-implementer.md` carry RH5204-006 and SHARED-FIX-001, or SHARED-FIX-003, as the table says. The spec author's examples for the tests are tagged.
9. Nothing left behind: ok. `sdd/work/` holds only `node-root-detached`; no `squad/decisions/` entries remain.

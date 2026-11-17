Role: spec checker
Commit: 2f9aa4e
Verdict: consistent

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| _None._ | | | | |

## Checked
1. Change doc: ok. The diff changes only test code and a test `.csproj` reference; no intended behavior, rule, phase, or surface changes, so none is required and none exists.
2. Specs match the diff: ok. The diff touches no file under `sdd/specs/`. FMT-PIPE-027 already states the behavior the tests now check (document and tree entry points, formatted outputs only, line ending detected in the output). The base's skip when the precheck refused the input matches its "Output of a run that formatted" bullet.
3. IDs: ok. No ID added, changed, or retired; `requirement-ids.txt` untouched.
4. Code owners: ok. No spec header touched.
5. User documentation: ok. No rule page, diagnostic, or code fix touched.
6. Open questions: ok. The skeptic and test implementer raise no output questions; both follow-up candidates are listed under `Follow-up candidates` in `00-lead.md`.
7. Coverage: not applicable. The diff touches no spec, so `scripts/test.sh --project architecture` is not required by this check (the test implementer reports it passes at 180 tests).
8. Requirement tags: ok. No test method is added; the table lists only existing tests with no tag claimed, and none is expected (the check is incidental to each rule's test).
9. Nothing left behind: ok. `sdd/work/` holds only `phase-check-shared`, which the lead deletes before publishing.

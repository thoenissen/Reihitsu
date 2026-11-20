Role: spec checker
Commit: 1c87153
Verdict: consistent

My prompt carried no intended fix, no suspected cause, and no verdict anyone hoped for.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| _None_ | | | | |

## Checked
1. Change doc: ok. Bug fix against unchanged CLI-016; no behavior added, changed, or retired; no change doc needed or present; no visible output change beyond the maintainer's triage answer.
2. Specs match the diff: ok. The diff touches no file under `sdd/specs/`; CLI-016 already states that a failing file does not stop the run, and the change makes the code meet it.
3. IDs: ok. No ID added, reused, or removed; `requirement-ids.txt` untouched.
4. Code owners: ok. No spec header touched, no type renamed, added, or removed.
5. User documentation: ok. No rule page affected.
6. Open questions: ok. The skeptic and test implementer raised no output questions; decisions 1-9 in `00-lead.md` record why nothing visible is in question.
7. Requirements covered: ok. No `sdd/specs/` change, so the architecture run is not required; the implementer reported architecture 198 passed. No new requirement; CLI-016 and CLI-022 are covered by the new tests.
8. Test tags: ok. Tagged tests in `CliDeepNestingEndToEndTests.cs` carry `[Requirement("CLI-016")]` (and `CLI-022` on the parallel test), matching the `Tests` table. `ModeratelyNestedFileKeepsFormatting` is an untagged characterization test, listed as "none" in the table; not a finding.
9. Nothing left behind: ok. The only `sdd/work/` on the branch is `sdd/work/cli-stack-overflow/`, which the lead deletes before publishing; none exists on origin/main.

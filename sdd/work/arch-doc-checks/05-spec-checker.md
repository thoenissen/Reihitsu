Role: spec checker
Commit: f5c9f8e
Verdict: #1507: consistent; #1428: consistent

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## thoenissen/Reihitsu#1507

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| - | - | - | none | - |

## Checked
1. Change doc: none required (no spec, requirement, rule, phase, or surface changes; diff touches no `sdd/specs/`, no `documentation/`): ok.
2. Specs match the diff: no spec covers the phase-ownership table; nothing to state: ok.
3. IDs: no new IDs; `requirement-ids.txt` untouched: ok.
4. Code owners: no spec header touched: ok.
5. User documentation: no rule page affected: ok.
6. Open questions: spec author and skeptic list none for this issue; lead decisions 1, 4, 5 record the test-only choices: ok.
7. Requirement coverage: diff does not touch `sdd/specs/`, but `sdd/README.md` and the coverage tests changed, so the architecture project was run (see Whole run).
8. Requirement tags: no requirement applies, none tagged, the `Tests` table claims none: ok.
9. Leftover `sdd/work/`: only this run's (`arch-doc-checks`): ok.

## thoenissen/Reihitsu#1428

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| - | - | - | none | - |

## Checked
1. Change doc: none required (rulebook text and a test-project check; lead decision 3): ok.
2. Specs match the diff: `sdd/README.md` *Tests and requirements* and *Checks* state both rules and match `RequirementCoverageTests` (`IsEnforced` without the `shared/` condition, `FindExemptionsWithoutSpecFile` against `SpecCatalog.EnumerateSpecFiles`); `ExemptedSpecs` stays empty; the "no spec is exempted" report now comes from the keys (decision 6). The remaining `shared/` mentions in the test file are the unchanged `shared/` fixtures and one historical summary sentence: ok.
3. IDs: no change: ok.
4. Code owners: no spec header touched: ok.
5. User documentation: no rule page affected: ok.
6. Open questions: none open; decisions 2, 3, 6, 7 are recorded in `00-lead.md`: ok.
7. See Whole run.
8. Requirement tags: none apply (`sdd/README.md` is not a spec); the `Tests` table claims none: ok.
9. Leftover `sdd/work/`: only this run's: ok.

## Whole run
- `scripts/test.sh --project architecture`: `Passed!  - Failed:     0, Passed:   153, Skipped:     0, Total:   153` at f5c9f8e.
- No `src/` production file, spec, ledger, or documentation page is in the diff; diff is `sdd/README.md`, `sdd/work/arch-doc-checks/`, and three files in `src/Reihitsu.ArchitectureTests/`, consistent with the tests lane.

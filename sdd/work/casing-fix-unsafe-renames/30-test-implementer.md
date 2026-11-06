Role: test implementer
Commit: 7da0730 (head before this handover; production code unchanged)
Verdict: tests written: finding 9 of `29-auditor.md` repaired, all 25 RH4105 tests pass

Repair round 2, lead decision 21: finding 9 (FC-017). My prompt carried no intended fix, suspected cause or hoped-for verdict. It asked me to re-derive the input myself and keep the expected output, and I did.

**Deriving the input.** For the cascade of `Arg`, the old names under SHARED-CASING-007 are `Arg` and the other part's `other`. The new name is `arg`. An excluded branch that pins only the new-name arm must name `arg` and neither old name. Its other tokens must be keywords or literals, not identifiers. The smallest such statement is `int arg = 0;`, the same shape the auditor suggested. The expected output stays the source in both documents: no fix, single or Fix All.

**Falsification check.** I can't remove the new-name arm from production code, so I checked the other side instead. A temporary control variant kept everything identical but excluded `int unrelated = 0;`. It failed at `Context: Iterative code fix application`, because the fix was offered and `Run(int Arg)` became `Run(int arg)`. With the repaired input, then, `arg` is the only token in the input that can withhold the fix: the test is green only because of the new-name check. I discarded the control and committed only the repaired input.

## Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH4105MethodParameterCasingAnalyzerTests.NoFixIsOfferedWhenAnExcludedBranchOfTheOtherPartOfAPartialMethodNamesTheNewName`: excluded branch changed from `int arg = other;` to `int arg = 0;` | SHARED-CASING-007, SHARED-FIX-003 | `Verify`, two documents, source as fixed state (single fix + three Fix All scopes) | passes; the control without `arg` fails with the fix offered |

The summary, name and tags stay as they are: they now describe what the input pins.

## Scenarios (bug fix)
| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| Other part's excluded branch names only the new name `arg` | LF | code fix, single + Fix All | holds | no fix, as specified |
| Control: other part's excluded branch names no name of the rename (`unrelated`) | LF | code fix, single | fix offered (not committed) | `Run(int Arg)` → `Run(int arg)`; shows the committed test depends on `arg` |

## Not covered, and why
- **CRLF, other surfaces, other failure classes.** The repair changes one identifier in one test's input and nothing else. The coverage reasoning of `25-test-implementer.md` is unchanged.

## Commands
```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Naming.RH4105"   # Passed 25, Failed 0
```

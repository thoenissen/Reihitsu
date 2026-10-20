Role: spec checker
Commit: 98d7339
Verdict: consistent

Re-check of finding 1 of `24-spec-checker.md` only, against `git diff 76d14dd HEAD -- src` (the two test files; no spec, rule page, or production file changed). My prompt carried no intended fix or hoped-for verdict.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|

## Checked

Finding 1 is closed. Each form of the old finding, on the side its list names, now has a tagged test whose markup and diagnostic count pin it exactly, with a contrast line that makes the test fail if the analyzer stopped distinguishing the sides. I read every new input and summary, and the inputs behave as I observed for the same forms in my own scratch runs for 24.

| Old item | New tagged test | Side pinned | Summary matches input |
|---|---|---|---|
| (a) not custom: `as` | RH1002 `VerifyConstantNullConvertedByAsIsNotCustomComparer` (SHARED-EQUAL-002): `NoComparer as I`, `null as I`, both marked, count 2 | reported (not custom) | yes |
| (b) custom: property, method call | RH1002 `VerifyPropertyOrMethodCallArgumentIsCustomComparer`: `Prop`, `Make()` unmarked; `EqualityComparer<S>.Default` marked | not reported (custom) | yes |
| (c) custom: `as` around a custom form | RH1002 `VerifyCustomFormConvertedByAsIsCustomComparer`: `_field as I`, `_boxed as I`, `new SComparer() as I` unmarked; `null as I` marked | not reported (custom) | yes |
| (d) custom: null-conditional receiver field, property, call | RH1002 `VerifyNullConditionalAccessOnFieldPropertyOrCallIsCustomComparer`: `_holder?.`, `HolderProp?.`, `MakeHolder()?.` unmarked; `((Holder)null)?.` marked | not reported (custom) | yes |
| (e) RH1001-003 reported: parentheses and `!` | RH1001 `VerifyWrappedVariableInitialValueLeavesDeclaredTypeReported` (RH1001-003): `(_other)`, `_other!` marked, count 3 with the field | reported | yes |
| (f) reported: constant condition choosing a reported branch | RH1001 `VerifyConstantConditionChoosingCreationWithoutComparerLeavesDeclaredTypeReported`: `true ? new() : new(_comparer)` and `false ? new(_comparer) : new()` marked; `false ? new() : new(_comparer)` exempt; count 2 | reported, with the exempt contrast | yes |
| (g) reported: `switch` with one reported arm | RH1001 `VerifySwitchWithArmWithoutCustomComparerLeavesDeclaredTypeReported`: new-without-comparer arm and variable arm each next to a custom-comparer arm marked; all-custom `switch` exempt; count 3 with the field | reported, with the exempt contrast | yes |

The implementer's mutation run (one mutation per test, 7 failures with the expected count mismatches, then restored) is consistent with these assertions; I did not repeat it. The `(_other)!` line the implementer dropped does not compile, so the two forms are tested separately, which is what the list item states.

No regression:
- `scripts/test.sh --project architecture`: "Passed!  - Failed:     0, Passed:    72, Skipped:     0, Total:    72, Duration: 48 s - Reihitsu.ArchitectureTests.dll (net10.0)".
- `scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Performance.RH1001|FullyQualifiedName~Reihitsu.Analyzer.Test.Performance.RH1002"`: "Passed!  - Failed:     0, Passed:    85, Skipped:     0, Total:    85, Duration: 5 s - Reihitsu.Analyzer.Test.dll (net10.0)".
- The diff outside the two test files is empty, the working tree was clean before this file, and `sdd/work/` holds only this run's directory.

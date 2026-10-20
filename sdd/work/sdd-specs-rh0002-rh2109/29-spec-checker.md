Role: spec checker
Commit: 29036e5
Verdict: consistent

Re-check of the delta `git diff 820eba6 HEAD`, outside `sdd/work/`: the decisions entry `squad/decisions/2026-10-20-996-test-only-audit-findings.md` and `RH1002…AnalyzerTests.cs`. No spec, rule page, or production file changed. My prompt carried no intended fix or hoped-for verdict.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|

## Checked

- **New tagged test** `VerifyNullConditionalAccessOnNullConstantReceiverIsNotCustomComparer` (tag `SHARED-EQUAL-002`): it verifies the null-constant receivers of the not-custom item "a null-conditional access whose receiver has a form of the first item". Four receivers are marked as reported (not custom): the `const` field `NullHolder`, the `const` local `isConst`, `default(Holder)` (reference type), and `default(VH?)` (nullable value type). The contrast `Holder notConst = null; items.Distinct(notConst?.Comparer);` is unmarked, so it must stay custom; the count of 4 diagnostics is exact. The contrast works: the non-`const` local is the custom-list form "a local variable that is not a `const` and is initialized with `null`", and the test fails if either side flips (the implementer's two mutations gave expected "3" actual "4" and expected "5" actual "4"). The summary states what the input does. Together with the existing `((Holder)null)?.Comparer` test, every receiver form the item names is now tagged on the reported side.
- **Rename and summary:** `VerifyOnlyArgumentsThatCanProduceAComparerCountAsCustom` is now `VerifyComparerArgumentCountsAsCustomByItsForm`, tags unchanged (`SHARED-EQUAL-002`, `RH1002-001`), input and assertion unchanged in the diff. The new name matches the form-based wording of the spec, and the old one contradicted the custom list (a non-`const` local holding `null`, a call returning `null`). The reworded phrase is accurate: in `Enumerable.Distinct(source: items, comparer)` the variable `comparer` is positional after a non-trailing named argument. Nothing outside `sdd/work/` references the old name (`grep` over `.cs` and `.md`).
- **Decisions entry:** faithful. It records the two test-only findings of `27-auditor.md` accurately (the missing `const` field, `const` local and `default` receivers, with only the cast-of-`null` receiver tagged; the misstating name and summary phrase), the three options, and the answer as carried out (one tagged test with a non-constant contrast, the rename and reworded summary, spec checker, auditor on the delta only, validator, open-question issues, publish). Reason "None given."; layout and escalation number match the other entries of this run and `squad/decisions/README.md`.
- **Runs:**
  - `scripts/test.sh --project architecture`: "Passed!  - Failed:     0, Passed:    72, Skipped:     0, Total:    72, Duration: 45 s - Reihitsu.ArchitectureTests.dll (net10.0)".
  - The two touched tests run on their own: "Passed!  - Failed:     0, Passed:     2, Skipped:     0, Total:     2, Duration: 3 s - Reihitsu.Analyzer.Test.dll (net10.0)".
- **Nothing else changed:** the diff holds the two files named above; the working tree was clean before this file; `sdd/work/` holds only this run's directory.

Role: test implementer
Commit: 1ab8ba0
Verdict: tests written

Repair round for findings 1 and 2 of `27-auditor.md`. The maintainer decided to fix both (`squad/decisions/2026-10-20-996-test-only-audit-findings.md`).
- Only `src/Reihitsu.Analyzer.Test/Performance/RH1002TypesUsedForEqualityComparisonMustImplementEqualityMembersAnalyzerTests.cs` changed.
- No production code, spec, or rule page was touched, and no existing assertion changed.
- The new test passes on the unchanged code and contradicts no spec.
- My prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the decided worklist.

## Tests

| Test (class prefix · method) | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH1002 · VerifyNullConditionalAccessOnNullConstantReceiverIsNotCustomComparer (new) | SHARED-EQUAL-002, not-custom item "a null-conditional access whose receiver has a form of the first item". It covers the receivers `NullHolder?.Comparer` (`const Holder NullHolder = null;` field), `isConst?.Comparer` (a `const` local), `default(Holder)?.Comparer`, and `default(VH?)?.Comparer` (`struct VH` with a `Comparer` property). Contrast on the custom side: `notConst?.Comparer`, where `notConst` is a non-`const` local holding `null` (FC-003, FC-017) | `Verify` + `Diagnostics(…, 4)` | pass: the four constant receivers are reported, the non-`const` local is not |
| RH1002 · VerifyComparerArgumentCountsAsCustomByItsForm (renamed from `VerifyOnlyArgumentsThatCanProduceAComparerCountAsCustom`) | SHARED-EQUAL-002, RH1002-001 (tags unchanged). FC-031: name and summary | `Verify` + `Diagnostics(…, 10)` (unchanged) | pass |

The inputs are those that finding 1 of `27-auditor.md` names, in a minimal frame: `using System.Collections.Generic;`, `using System.Linq;`, `struct S`, `class Holder`, `struct VH`, `class C`.

Finding 2: two changes, both test text only. The input and the assertion are byte-for-byte unchanged.
- **Rename.** The new name says that the argument's form decides, which matches the form-based wording of SHARED-EQUAL-002. The old name claimed that only arguments which can produce a comparer count as custom. That contradicts the custom list: a non-`const` local holding `null` is custom, and so is a call returning `null`.
  - I did not use the auditor's example name `VerifyFormOfComparerArgumentDecidesWhetherItIsCustom`. It is too close to the existing tagged test `VerifyFormOfArgumentDecidesWhetherComparerIsCustom`.
  - No file outside `sdd/work` references the old name (`grep -rn` over the repository).
- **Summary.** "a variable passed as a named or non-trailing named argument" became "a variable passed as a named argument or positionally after a non-trailing named argument". In `Enumerable.Distinct(source: items, comparer)` the variable `comparer` is positional. The rest of the summary was only re-wrapped.

The new test can fail. I ran two mutations, one at a time, and restored the file from a backup after each. The final diff contains no mutation (checked with `git diff` and `grep`).

| Mutation | Result of the test |
|---|---|
| no report expected on `NullHolder?.Comparer` (a `const` receiver dropped from the not-custom side) | failed: expected "3" actual "4" |
| a report expected on `notConst?.Comparer` (custom side) | failed: expected "5" actual "4" |

## Not covered, and why

- **The RH1001 counterparts `new(NullHolder?.Comparer)` and `new(default(Holder)?.Comparer)`.** The decision asks for one RH1002 test. Through RH1001-003's first item ("passes a custom comparer as SHARED-EQUAL-002 defines it"), they reach the same decision, `EqualityComparerArgumentUtilities.HasExplicitEqualityComparerArgument`. Per Policy ownership in `27-auditor.md`, that is the only owner, and the new RH1002 test pins it.
- **FC-010 to FC-014, FC-019, FC-007/FC-008, FC-033 (line endings).** No code fix and no formatter phase is involved, and the diagnostics are located by markup.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Performance.RH1001|FullyQualifiedName~Reihitsu.Analyzer.Test.Performance.RH1002"
scripts/test.sh --project architecture
```

Results on this tree:

| Command | Result |
|---|---|
| Focused RH1001/RH1002 run | "Passed! - Failed: 0, Passed: 86, Skipped: 0, Total: 86" |
| `scripts/test.sh --project architecture` | "Passed! - Failed: 0, Passed: 72, Skipped: 0, Total: 72" |
| `scripts/format.sh` on the changed file | "Formatted 0 of 1 file(s)." |
| `scripts/build.sh --no-incremental` | "Build succeeded. 0 Warning(s) 0 Error(s)" |

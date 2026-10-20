Role: test implementer
Commit: 76d14dd
Verdict: tests written

Repair round after `24-spec-checker.md`, finding 1. Several forms that the bounded lists of SHARED-EQUAL-002 and RH1001-003 name had no tagged test on the side the list names. I re-derived each form from the list text at `76d14dd`. Then I added focused tagged tests:
- one test per form group;
- each test pins the named side;
- most tests also have a contrast line on the other side.

No production code, spec, or rule page was touched. Every new test passes on the unchanged code, and no test contradicted a spec. My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Tests

All tests are new, in `src/Reihitsu.Analyzer.Test/Performance/`, at the end of their classes. They use numbered `{|#n:…|}` markup with the `Diagnostics(…, n)` helper, as the earlier tagged tests do.

| Test (class prefix · method) | Requirement / form (checker item) | Helper | Expected now |
|---|---|---|---|
| RH1002 · VerifyConstantNullConvertedByAsIsNotCustomComparer | SHARED-EQUAL-002, not custom: a constant `null` converted by `as`, as the `const` field `NoComparer as IEqualityComparer<S>` and as the literal `null as IEqualityComparer<S>` (a) | `Verify` + `Diagnostics(…, 2)` | pass: both calls reported |
| RH1002 · VerifyPropertyOrMethodCallArgumentIsCustomComparer | SHARED-EQUAL-002, custom: a property other than `Default` (`Prop`) and a method call (`Make()`, which returns `null`) (b). Contrast: `EqualityComparer<S>.Default` reported | `Verify` + `Diagnostics(…)` | pass: only the `Default` call reported |
| RH1002 · VerifyCustomFormConvertedByAsIsCustomComparer | SHARED-EQUAL-002, custom: a non-constant field (`IEqualityComparer<S> _field`, `object _boxed`) and an object creation, each converted by `as` (c). Contrast: `null as IEqualityComparer<S>` reported | `Verify` + `Diagnostics(…)` | pass: only the `null as` call reported |
| RH1002 · VerifyNullConditionalAccessOnFieldPropertyOrCallIsCustomComparer | SHARED-EQUAL-002, custom: a null-conditional access whose receiver is a field (`_holder?.Comparer`), a property (`HolderProp?.Comparer`), or a method call (`MakeHolder()?.Comparer`) (d). Contrast: `((Holder)null)?.Comparer` reported | `Verify` + `Diagnostics(…)` | pass: only the cast-of-`null` receiver reported |
| RH1001 · VerifyWrappedVariableInitialValueLeavesDeclaredTypeReported | RH1001-003, left reported: a variable inside parentheses (`(_other)`) and behind `!` (`_other!`) (e) | `Verify` + `Diagnostics(…, 3)` | pass: `a`, `b`, and the field `_other` reported |
| RH1001 · VerifyConstantConditionChoosingCreationWithoutComparerLeavesDeclaredTypeReported | RH1001-003, left reported: a constant condition whose chosen branch passes no comparer, once with `true` and once with `false` (f). Contrast: `false ? new() : new(_comparer)` exempt | `Verify` + `Diagnostics(…, 2)` | pass: `a` and `b` reported, `c` not |
| RH1001 · VerifySwitchWithArmWithoutCustomComparerLeavesDeclaredTypeReported | RH1001-003, left reported: a `switch` with an arm passing no comparer, and one with a variable arm, next to an arm that passes a custom comparer (g). Contrast: every arm passes a custom comparer, exempt | `Verify` + `Diagnostics(…, 3)` | pass: `a`, `b`, and the field `_other` reported, `c` not |

How the inputs relate to `24-spec-checker.md`:
- Every input line that the checker named is in a test unchanged: `NoComparer as IEqualityComparer<S>`, `Prop`, `Make()`, `_field as IEqualityComparer<S>`, `_holder?.Comparer`, `HolderProp?.Comparer`, `MakeHolder()?.Comparer`, `(_other)`, `_other!`, `true ? new() : new(_comparer)`, `_mode switch { 0 => new(_comparer), _ => new() }`.
- I added, within the same list items:
  - in (a), the literal `null as …`;
  - in (c), an `object` field and an object creation;
  - in (f), the `false` side;
  - in (g), the variable arm;
  - the contrast lines.
- I also tried `(_other)!`. It does not compile: C# parses `(_other)!…` as a cast to a type `_other` (CS0246, CS1525). That is my input's defect, not the checker's, and I dropped the line. The two forms the checker named, parentheses and `!`, are each tested on their own.
- The field `_other` in the RH1001 tests carries its own markup, because RH1001-002 reports its declared type, as in the existing RH1001-003 example (`_other`, `#2`).

Each new test can fail. I mutated each test once, in one batch, on the form the checker named. Then I restored both files from backups. The final diff contains no mutation (checked with `git diff --stat` and `grep`). Results of the focused run on the mutated tree: "Failed: 7, Passed: 78, Total: 85". Each test failed for the expected reason:

| Test | Mutation | Failure |
|---|---|---|
| VerifyConstantNullConvertedByAsIsNotCustomComparer | no report expected on `NoComparer as …` | expected "1" actual "2" |
| VerifyPropertyOrMethodCallArgumentIsCustomComparer | a report expected on `Distinct(Prop)` | expected "2" actual "1" |
| VerifyCustomFormConvertedByAsIsCustomComparer | a report expected on `Distinct(_field as …)` | expected "2" actual "1" |
| VerifyNullConditionalAccessOnFieldPropertyOrCallIsCustomComparer | a report expected on `Distinct(_holder?.Comparer)` | expected "2" actual "1" |
| VerifyWrappedVariableInitialValueLeavesDeclaredTypeReported | no report expected on `b = _other!` | expected "2" actual "3" |
| VerifyConstantConditionChoosingCreationWithoutComparerLeavesDeclaredTypeReported | no report expected on `a = true ? new() : new(_comparer)` | expected "1" actual "2" |
| VerifySwitchWithArmWithoutCustomComparerLeavesDeclaredTypeReported | no report expected on `a = _mode switch { 0 => new(_comparer), _ => new() }` | expected "2" actual "3" |

## Not covered, and why

- **The scratch-only RH1001 forms in `24-spec-checker.md` check 7**: `new(NoComparer as I)`, `new(NoComparer ?? EqualityComparer<S>.Default)`, `new(NoComparer ?? _comparer)`, and `new((_comparer as I))`. Finding 1 does not list them as missing. Through RH1001-003's first item they reach the same SHARED-EQUAL-002 decision the RH1002 tests pin.
- **FC-010 to FC-014, FC-019, FC-007/FC-008, FC-033 (line endings).** No code fix and no formatter phase is involved. RH1001 and RH1002 offer no fix, and the diagnostics are located by markup, not by layout.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Performance.RH1001|FullyQualifiedName~Reihitsu.Analyzer.Test.Performance.RH1002"
scripts/test.sh --project architecture
```

Results on this tree:

| Command | Result |
|---|---|
| Focused RH1001/RH1002 run | "Passed! - Failed: 0, Passed: 85, Skipped: 0, Total: 85" |
| `scripts/test.sh --project architecture` | "Passed! - Failed: 0, Passed: 72, Skipped: 0, Total: 72" |
| `scripts/format.sh` on the two test files | "Formatted 0 of 2 file(s)." |
| `scripts/build.sh` and `scripts/build.sh --no-incremental` | "Build succeeded. 0 Warning(s) 0 Error(s)" |

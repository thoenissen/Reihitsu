Role: test implementer
Commit: c5569d1
Verdict: tests written

Repair round after `14-auditor.md` (finding 3, and the characterization tests for findings 1 and 2), as allowed in `squad/decisions/2026-10-20-996-auditor-block-extra-repair.md`. I worked from the worklist in `15-spec-author.md` and the specs at `c5569d1`. No production code was touched. Every new test passes on the unchanged code, and no test contradicted a spec, so there is no finding against a spec. My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Tests

All tests are new, in `src/Reihitsu.Analyzer.Test/Performance/`. "Char." means a characterization test. It carries no `[Requirement]` tag, and its doc comment starts with "Characterizes …" and says that the case is an open question.

| Test (class prefix · method) | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH1002 · VerifyFormOfArgumentDecidesWhetherComparerIsCustom | SHARED-EQUAL-002 (FC-003, FC-017: the not-reported side of `(comparer)`, `comparer!`, `comparer ?? null`, `flag ? comparer : throw …`, `holder?.Comparer`, `(SComparer)(Token)null`, and a local `none` initialized with `null`; the reported side of `EqualityComparer<S>.Default ?? comparer`) | `Verify` + `Diagnostics(…, 11)` | pass |
| RH1001 · VerifyOnlyInitialValuesOfRecognizedCreationFormsExemptTheDeclaredType | RH1001-003 (FC-003, FC-017: not reported `p = (Dictionary<S, int>)new(_comparer)`; reported: `q` with a user-defined cast (declared type and cast type), `r` (`??`), `t` (a variable), `u` (`new()` next to `throw`), `w` (`default`), `v` (`[]`), the field `_other`, and the two `Wrapper` operator signatures) | `Verify` + `Diagnostics(…, 15)` | pass |
| RH1002 · VerifyArgumentThatCanOnlyProduceNullOrThrowCountsAsCustom | Char. struct-equality Q4 (RH1002 input) | `Verify` | pass: only the third call is reported |
| RH1002 · VerifyQueryExpressionClausesAreNotReported | Char. RH1002 Q2 | `Verify` | pass: only `GroupBy` is reported |
| RH1001 · VerifyCreationWithComparerArgumentThatCanOnlyProduceNullOrThrowIsNotReported | Char. struct-equality Q4 (RH1001 lines `map` and `other`) | `Verify` | pass: only the `S` of `other` is reported |
| RH1001 · VerifyDeclarationPickingBetweenCustomComparerCreationAndThrowIsReported | Char. RH1001 Q1 | `Verify` | pass: `a` and `b` are reported, `c` is not |
| RH1001 · VerifyCollectionExpressionPassingCustomComparerThroughWithIsReported | Char. RH1001 Q2 | `Verify` + a `SolutionTransforms` entry that sets `LanguageVersion.Preview` (local static function, the same pattern as the RH5412 tests) | pass: `set` is reported, `other` is not |
| RH1001 · VerifyCollectionExpressionArgumentIsNotReportedWhereTargetTypedCreationIs | Char. RH1001 Q3 | `Verify` | pass: the parameter type and the `new` of `Take(new())` are reported, `[_item]` is not |

How the inputs match their sources:
- The two tagged tests are the spec examples of SHARED-EQUAL-002 (`sdd/specs/shared/struct-equality.md`) and RH1001-003 (`sdd/specs/analyzer/RH1001.md`), taken verbatim. The only change is that each `{|RH100x:…|}` markup became a numbered `{|#n:…|}` markup, so the message is checked too.
- I checked this with a script. The script extracted the spec block and the test raw string, then replaced every markup key with the same placeholder. The two texts are identical, and so is the number of markups: 11 and 15.
- Each characterization test uses the input of its issue text in `15-spec-author.md` exactly as written. The struct-equality Q4 RH1001 issue text gives only the two declarations `map` and `other`. I put them in a minimal `struct S` / `class C` / `void M(bool flag)` frame with `using System;` and `using System.Collections.Generic;`.
- The RH1001 test file gains `using Microsoft.CodeAnalysis;` and `using Microsoft.CodeAnalysis.CSharp;`, which the preview transform needs.

Each new test can fail. I mutated each test's expectation temporarily, in one batch, and then restored the files from backups. The final diff contains no mutation (checked with `git diff` and `grep`). Results of `scripts/test.sh … RH1001|RH1002` on the mutated tree: 8 failed, 64 passed. Each test failed for the expected reason:

| Test | Mutation | Failure |
|---|---|---|
| VerifyFormOfArgumentDecidesWhetherComparerIsCustom | expects a report on `items.Distinct(holder?.Comparer)` | expected "12" actual "11" |
| VerifyOnlyInitialValuesOfRecognizedCreationFormsExemptTheDeclaredType | expects a report on the declared type of `p` | expected "16" actual "15" |
| VerifyArgumentThatCanOnlyProduceNullOrThrowCountsAsCustom | expects a report on the first call | expected "2" actual "1" |
| VerifyQueryExpressionClausesAreNotReported | expects a report on `group x by x` | expected "2" actual "1" |
| VerifyCreationWithComparerArgumentThatCanOnlyProduceNullOrThrowIsNotReported | expects a report on `map` | expected "2" actual "1" |
| VerifyDeclarationPickingBetweenCustomComparerCreationAndThrowIsReported | expects a report on `c` | expected "3" actual "2" |
| VerifyCollectionExpressionPassingCustomComparerThroughWithIsReported | moves the expected report from `set` to `other` | "Expected diagnostic to start on line "15" was actually on line "14"" |
| VerifyCollectionExpressionArgumentIsNotReportedWhereTargetTypedCreationIs | expects a report on `[_item]` | expected "3" actual "2" |

### Correction to `04-test-implementer.md`

- `04-test-implementer.md` says that `RH1002 · VerifyOnlyArgumentsThatCanProduceAComparerCountAsCustom` covers "null-conditional on `null` … both sides". That overstates the test.
  - Its only null-conditional access is `((Holder)null)?.Comparer`, on the reported side.
  - Before this round, no test covered a null-conditional access on a receiver that is not `null`. Nor did any test cover the custom side of `??`, parentheses or `!` around a custom comparer, or a user-defined conversion.
  - `VerifyFormOfArgumentDecidesWhetherComparerIsCustom` now covers those sides.
- Its tags stay unchanged. They are `SHARED-EQUAL-002` and `RH1002-001`, and the test verifies what they name:
  - `Distinct` is reported for a struct without equality members;
  - the forms it contains are decided as SHARED-EQUAL-002 states, on the sides it has.
- Its name and doc comment still use the old "can produce a comparer" framing. Under the new form-based wording of SHARED-EQUAL-002, a local holding `null` counts as custom. The doc comment's statements are still true for the inputs the test has, so I did not change the test.

## Not covered, and why

- **RH1002-001 and RH1001-002 coverage sentences.** These add no reported behavior. The covered sides already have tagged tests (`15-spec-author.md`). The excluded sides are pinned by the RH1002 Q2 and RH1001 Q3 characterization tests.
- **RH2001 code-owner change.** No requirement text changed, so there is no test.
- **FC-010 to FC-014, FC-019, FC-007/FC-008, FC-033 (line endings).** No code fix and no formatter phase is involved. RH1001 and RH1002 offer no fix (SHARED-EQUAL non-goal), and the diagnostics are located by markup, not by layout.
- **FC-027 (other forms).** The forms this round adds are the ones the auditor and the spec author listed. I added no further sibling forms beyond the spec examples and the issue texts.
- **Hint 1 (a local holding `null`).** Specified behavior now. It is on the not-reported side of the SHARED-EQUAL-002 test (`items.Distinct(none)`).

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Performance.RH1001|FullyQualifiedName~Reihitsu.Analyzer.Test.Performance.RH1002"
scripts/test.sh --project architecture
```

Results on this tree:

| Command | Result |
|---|---|
| Focused RH1001/RH1002 run | "Passed! - Failed: 0, Passed: 72, Skipped: 0, Total: 72" |
| `scripts/test.sh --project architecture` | "Passed! - Failed: 0, Passed: 72, Skipped: 0, Total: 72" (includes `RequirementCoverageTests`) |
| `scripts/format.sh` on the two test files | "Formatted 0 of 2 file(s)." |

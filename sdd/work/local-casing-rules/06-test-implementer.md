Role: test implementer
Commit: 30fcf474 (base; this handover's commit follows it)
Verdict: tests written

Repair round for decision 7 of `00-lead.md`: the two tests `05-implementer.md` disputes, corrected in their mechanics only. Inputs: `00-lead.md`, `04-test-implementer.md`, `05-implementer.md` (*Disputed tests or scope*), `sdd/specs/shared/casing.md` (SHARED-CASING-009, -010, -015), `sdd/specs/analyzer/RH4105.md`. The prompt carried no intended fix or hoped-for verdict. No input and no asserted fully fixed output changed; no production code touched.

## Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH4115LocalVariableCasingAnalyzerTests.FixKeepsTheLayoutAndCommentsOfAMultiLineAnonymousObjectMember` (LF, CRLF) | SHARED-CASING-015 | `Verify(source, fixed)` with both texts converted to the line ending; `TryApplyCodeFixAtAsync` once per diagnostic, located by the declarator identifier (new private helper `GetLocalIdentifierLocation`) | passes |
| `RH4105MethodParameterCasingAnalyzerTests.RangeVariablesWithTheSameNewNameInOneQueryAreNotFixed` | SHARED-CASING-009, SHARED-CASING-010 | `Verify` with `BatchFixedCode = testCode` and `NumberOfFixAllIterations = -1` | passes |

What changed and why:

1. **RH4115 multi-line anonymous object.** The old test used `VerifyCodeFixWithEndOfLine`, whose second assertion applies only the first action for the first diagnostic and compared it with the result of fixing both locals; with two diagnostics (`Total`, `Count`) that assertion could never hold. Now: `Verify` asserts the fully fixed text (unchanged: `var total`, `var count`, `Total = total, // trailing`, `Count = count /* last */`, own-line comment and layout kept) with the chosen line ending; then each single action is applied on the raw text, line endings included, and compared with the one-rename result SHARED-CASING-015 gives — renaming `Total` writes `Total = total, // trailing` and leaves `Count /* last */`; renaming `Count` writes `Count = count /* last */` and leaves `Total, // trailing`. Selecting the diagnostic by location removes the dependence on the order of the analyzer's diagnostics. Both single-action texts equal what `05-implementer.md` observed for `Total`.
2. **RH4105 colliding range variables.** Asserted outputs unchanged (single fix: `from value … from {|RH4105:VALUE|} … select value + VALUE`; Fix All: source unchanged, both diagnostics kept). Added `test.NumberOfFixAllIterations = -1`: the shared Fix All provider registers its action before it learns that SHARED-CASING-010 skips every candidate, and the framework treats `BatchFixedCode == source` as "no Fix All action expected". The bound allows at most one Fix All iteration; the batch result is still compared with the unchanged source, so the assertion is not weakened (same mechanism as `RH4106…FixAllSkipsAFieldWithALeadingAcronymThatCollidesWithAnotherField`, `-2`). Whether Fix All should offer no action at all is not specified, so no test asserts it.

## Scenarios (bug fix)
_N/A — behavior-change repair round._

## Not covered, and why
- No new matrix entries: this round only corrects two tests' mechanics; the matrix of `04-test-implementer.md` stands.
- Fix All on the RH4115 multi-line case: covered by the `Verify` call (the framework's Fix All result is the same fully fixed text).

## Commands
```shell
scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Naming"
```
Result on this head: `Failed: 0, Passed: 1002, Total: 1002`.

## Follow-up candidates
_None._

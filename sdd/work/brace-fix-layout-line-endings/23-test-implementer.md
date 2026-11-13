Role: test implementer
Commit: 04c09db5ee6d7a9f497f5e9a45fe13b45c045f7c
Verdict: tests written

Scope: decision 23 of `00-lead.md` (auditor 21 F8), SHARED-ATTR-022 as restated by spec author 22. The prompt carried no intended fix or hoped-for verdict.

## Tests
All in `src/Reihitsu.Analyzer.Test/Formatting/`.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH5514…Tests.SplitFixIsWithheldForALambdaInAFieldInitializerOnTheLineOfTheBraceOfTheClass` (spec `C2`) | SHARED-ATTR-022 | `VerifyNoCodeFixIsOffered` | red: fix offered |
| `RH5514…Tests.SplitFixIsWithheldBehindAClosedBodyWhileTheBraceOfTheClassIsStillOpenOnTheLineOfTheList` (spec `C3`) | SHARED-ATTR-022 | `VerifyNoCodeFixIsOffered` | green (pins that a closed `{` does not cancel an open one) |
| `RH5514…Tests.SplitFixIsWithheldForALambdaInAStatementOnTheLineOfTheBraceOfTheBlock` (spec `O`) | SHARED-ATTR-022 | `VerifyNoCodeFixIsOffered` | red: fix offered |
| `RH5514…Tests.SplitFixIsWithheldForALocalFunctionInASwitchSectionOnTheLineOfTheBraceOfTheSwitchStatement` (spec `P`) | SHARED-ATTR-022 | `VerifyNoCodeFixIsOffered` | red: fix offered |
| `RH5514…Tests.SplitFixIsOfferedForALambdaInAStatementBelowTheLineOfTheBraceOfTheBlock` (LF, CRLF) | SHARED-ATTR-022, SHARED-ATTR-017 | `VerifySplitFix` (verifier + raw text, line endings) | green |
| `RH5514…Tests.SplitFixIsOfferedBehindABraceInsideAStringLiteralOnTheLineOfTheList` (LF, CRLF) | SHARED-ATTR-022, SHARED-ATTR-017 | `VerifySplitFix` | green |
| `RH5514…Tests.SplitFixIsOfferedBehindABraceInsideACommentOnTheLineOfTheList` (LF, CRLF) | SHARED-ATTR-022, SHARED-ATTR-017 | `VerifySplitFix` | green |
| `RH5514…Tests.SplitFixIsOfferedBehindAClosedInterpolationOnTheLineOfTheList` (LF, CRLF) | SHARED-ATTR-022, SHARED-ATTR-017 | `VerifySplitFix` | green |
| `RH5528…Tests.VerifyDiagnosticWithoutCodeFixForReturnSpecifierListOfALambdaOnTheLineOfTheBraceOfTheBlock` | SHARED-ATTR-022 | `VerifyNoCodeFixIsOffered` | red: fix offered |

The four red tests fail on `Assert.IsEmpty(actions)` after the diagnostic is reported — the difference SHARED-ATTR-022 now states, not unrelated layout. RH5528 reports `[return: A, B]` on the lambda and offers the split today, so the change doc's RH5528 sentence (spec author 22, internal choice 3) holds and stays.

Existing SHARED-ATTR-022 tests: none changes. I checked every split-rule test with a list behind a `{` on its line (`rg '\{[^"]*\{\|(RH55[0-9]{2}|#[0-9]+):\[' src/Reihitsu.Analyzer.Test/Formatting/RH55*Tests.cs`): each withheld case (namespace, class, interface, enum, delegate in class, local-function block with and without a statement in front, accessor lists in RH5516/5518/5520/5528/5531) is still withheld under the restatement, and the only offered one behind a `{` on the line (`SplitFixIsOfferedBehindTheBraceOfAnotherBodyOnTheLineOfTheList`) is the closed-`{` case the spec keeps offered. The `{`-above examples (RH5518 `Value`/`Other`, RH5528 accessor list) stay offered and are covered by existing tests.

## Scenarios (bug fix)
_Not a bug fix round._

## Not covered, and why
- CRLF variants of the withheld cases: a withheld fix writes no layout, so line endings cannot change the outcome; the offered side runs LF and CRLF.
- One test per split rule for the new owner-free condition: every split rule goes through the one guard in `TargetAttributeListShapeCodeFixProviderBase` (`rg -n "SHARED-ATTR-022" src/Reihitsu.Analyzer.CodeFixes` → `Base/TargetAttributeListShapeCodeFixProviderBase.cs` only); RH5514 carries the full set, RH5528 one lambda case because its list target differs (`return:`), and the existing per-rule withheld tests stay.
- Extension block `extension(int i) { [A, B] void M() { } }`: needs a preview language version; it is the same open-`{` condition as the class case (spec author 22 calls the hint moot).
- Comment or directive in the gap (FC for token gaps): the comment side is the `/* { */` test; a directive cannot stand on the list's line in front of it.
- Idempotency / Fix All: unchanged by this round; the existing split tests and `GetFixAllScenario` cover them.

## Commands
```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5514MethodAttributeListsMustFollowShapeRulesAnalyzerTests|FullyQualifiedName~RH5528ReturnValueAttributeListsMustFollowShapeRulesAnalyzerTests"
```
Result on 04c09db plus these tests: 80 passed, 4 failed (the four red tests above).

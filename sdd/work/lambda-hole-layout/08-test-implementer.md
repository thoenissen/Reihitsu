Role: test implementer
Commit: see the commit that adds this handover (parent a3e2823)
Verdict: tests written

My prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the change asked (replace the helper of one test, keep its expected output).

## Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH5111AssignmentsMustHaveProperLineBreaksAnalyzerTests.VerifyFixAllAlignsTheBlockOfEveryAssignedAnonymousMethodUnderDelegate` (`\n`, `\r\n`) | FMT-INDENT-031 | `Verify(ConvertEndOfLine(testCode, endOfLine), ConvertEndOfLine(fixedCode, endOfLine))` | pass |

Only the call changed: `VerifyCodeFixWithEndOfLine` → `Verify(ConvertEndOfLine(...), ConvertEndOfLine(...))`. Source, expected output, and requirement tag are unchanged. `VerifyCodeFixWithEndOfLine`'s second step (`ApplyCodeFixAsync`) applies the fix of one diagnostic and compares it with the whole expected text, so it does not fit a two-diagnostic source. The Roslyn verifier behind `Verify` applies the fixes one at a time (iterative) and also runs Fix All, and compares each result with the expected text. This is the pattern the sibling tests use for an end-of-line row, for example `RH5115IndexAccessMustStayOnExpressionLineAnalyzerTests` lines 467, 720, 821, 1238, 1277.

Line-ending sensitivity checked: with the CRLF source compared against an LF expected text (a temporary edit, since reverted), the `\r\n` row fails with "Context: Iterative code fix application ... content of '/0/Test0.cs' did not match". So the verifier compares line endings exactly, and the `\r\n` row tests CRLF output.

## Scenarios (bug fix)
_Not applicable (behavior-change lane)._

## Not covered, and why
Nothing new. The class's `GetFixAllScenario` (one declaration with two declarators) is unchanged. It is not the place for this case because it takes one scenario without an end-of-line parameter.

## Commands
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Formatting.RH5111AssignmentsMustHaveProperLineBreaksAnalyzerTests"
Result on this head: 98 passed, 0 failed.

## Follow-up candidates
_None._

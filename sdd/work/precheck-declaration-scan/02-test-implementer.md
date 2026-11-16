Role: test implementer
Commit: b88bf2c (tests run on this head; production code untouched)
Verdict: tests written

The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the run's own handovers. The change is time-only, so there is no reproduction of visible behavior: every test below is a characterization test that passes on today's code and must still pass after the change. No test asserts timing.

## Tests

All in `src/Reihitsu.Formatter.Test/Unit/Precheck/UnsupportedVariableDeclarationTests.cs`, each run by the helper under LF and CRLF; `AssertSupported` also checks second-pass stability.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `CommentBehindTheLastTokenOfALaterValueOfSeveralTokensIsUnsupported` (`int a = x + y, b = p + q /* c */;`) | FMT-UNSUP-004 | `AssertUnsupported` | pass |
| `CommentOnItsOwnLineInFrontOfALaterValueOfSeveralTokensIsUnsupported` (`b =⏎// c⏎p + q`) | FMT-UNSUP-004 | `AssertUnsupported` | pass |
| `CommentInsideALaterValueBehindItsFirstTokenIsSupported` (`b = p /* c */ + q`) | FMT-UNSUP-004 | `AssertSupported` | pass |
| `CommentInsideALaterValueInFrontOfItsLastTokenIsSupported` (`b = p +⏎// c⏎q`, leading trivia of the last token) | FMT-UNSUP-004 | `AssertSupported` | pass |
| `CommentBetweenAValueOfSeveralTokensAndASeparatingCommaIsUnsupported` (`int a = x + y /* c */, b = p + q;`) | FMT-UNSUP-004 | `AssertUnsupported` | pass |
| `CommentInFrontOfTheFirstTokenOfALeftNestedValueIsUnsupported` (deep-left concatenation) | FMT-UNSUP-004 | `AssertUnsupported` | pass |
| `CommentBehindTheFirstTokenOfALeftNestedValueIsSupported` | FMT-UNSUP-004 | `AssertSupported` | pass |
| `CommentBehindTheLastTokenOfARightNestedValueIsUnsupported` (deep-right `?:` chain) | FMT-UNSUP-004 | `AssertUnsupported` | pass |
| `CommentInFrontOfTheLastTokenOfARightNestedValueIsSupported` (`... : 3 +⏎// c⏎4;`) | FMT-UNSUP-004 | `AssertSupported` | pass |
| `CommentOnItsOwnLineInsideAFluentChainValueIsSupported` | FMT-UNSUP-004 | `AssertSupported` | pass |
| `CommentBehindAFluentChainValueIsUnsupported` | FMT-UNSUP-004 | `AssertUnsupported` | pass |
| `CommentInFrontOfTheValueOfADeclarationInsideALaterValueIsUnsupported` | FMT-UNSUP-004 (FC-039) | `AssertUnsupported` | pass |
| `CommentBehindTheValueOfADeclarationInsideALaterValueIsUnsupported` | FMT-UNSUP-004 (FC-039) | `AssertUnsupported` | pass |
| `CommentInsideTheValueOfADeclarationInsideALaterValueIsSupported` | FMT-UNSUP-004 (FC-039) | `AssertSupported` | pass |

The first four are the skeptic's four inputs. Each condition of the value loop is tested on both sides (FC-003): leading trivia is checked only on a value's first token (unsupported in front, supported behind it), trailing trivia only on a value's last token (unsupported behind, supported in front of it), for a later value, for a deep-left first token, and for a deep-right last token.

Sensitivity check, made by temporarily editing `PrecheckPhase.cs` and reverting it (not committed; `git status` shows only the test file and this handover):

- pairing every in-value token with the declaration's first value: `CommentBehindTheLastTokenOfALaterValueOfSeveralTokensIsUnsupported` and `CommentOnItsOwnLineInFrontOfALaterValueOfSeveralTokensIsUnsupported` fail (no existing test failed);
- pairing every in-value token with the last value: `CommentBetweenAValueOfSeveralTokensAndASeparatingCommaIsUnsupported` fails, beside the existing `CommentBetweenAValueAndASeparatingCommaIsUnsupported` and `DeclaratorCommaKeepsTheReasonOfAVariableDeclarationForAComment`.

Existing tests asserting a changed output: none — no output changes in this run, so none was rewritten or left behind.

## Scenarios (bug fix)

_None._ Time-only change; no visible behavior to reproduce. The equivalence evidence is the skeptic's harness (1912 corpus files plus generated inputs, identical verdicts).

## Not covered, and why

- **Timing.** No test asserts time; flaky under CI load and not a requirement.
- **Declaration kinds (field, event field, `for`, `using`/`fixed` statement, input root).** One test per kind is not needed for this change: `GetSpanTokens` differs per parent only in the token list and terminator it returns, and every arm feeds the single value loop in `FindCommentInDeclaration` (`rg -n "GetSpanTokens\(|FindCommentInDeclaration\(" src/Reihitsu.Formatter/Pipeline/Precheck/PrecheckPhase.cs` shows one call each); the change touches only that loop. The existing `SpecExampleEveryKindOfVariableDeclarationIsChecked` and the entry-point tests cover the arms and the input root (FC-041).
- **FC-040** (documentation modes): trivia classification is untouched; existing documentation-mode tests stay.
- **FC-038, FC-042, FC-043**: no phase, guard, or code-fix target changes.
- **Many declarators (`values.Find`, term 3)**: the lead kept it out of scope (decision 2); behavior covered by the later-value tests.
- **Code-fix path**: `ReihitsuFormatter.FindUnsupportedSyntax` calls the same predicate (FMT-UNSUP-007, `FindUnsupportedSyntaxGivesTheAnswerOfThePrecheck`); no code fix changes.

## Commands

```shell
scripts/test.sh --no-install --project formatter --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Unit.Precheck"
scripts/test.sh --no-install --project formatter --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Unit.Precheck.UnsupportedVariableDeclarationTests"
```

Result on b88bf2c: `Passed! - Failed: 0, Passed: 249` (235 before plus 14 new).

## Follow-up candidates

_None._

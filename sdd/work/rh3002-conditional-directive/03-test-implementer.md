Role: test implementer
Commit: 7611501
Verdict: tests written

Lane: tests (specs for existing behavior). Worked from thoenissen/Reihitsu#1756 (`agent-filed`), its maintainer comments as recorded in `00-lead.md`, `01-spec-author.md`, `02-skeptic.md`, and `git diff 9f69e84 7611501 -- sdd/specs`. The prompt carried no intended fix, suspected cause, or hoped-for verdict.

All new tests are in `src/Reihitsu.Analyzer.Test/Clarity/RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzerTests.cs`, and all pass on the unchanged code, as a tests-lane run requires. No existing test covered RH3002-010. The nearest one, `VerifyDirectiveInsideThrowExpressionParenthesesWithholdsTheCodeFix`, has no excluded code, so it keeps only its RH3002-009 tag.

## Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `VerifyConditionalDirectiveInsideReturnedParenthesesIsReportedWithoutCodeFix` (the spec example, the issue's input) | RH3002-010 | `VerifyNoCodeFixIsOffered` | pass |
| `VerifyConditionalDirectiveInsideReturnedParenthesesIsReportedWithoutCodeFixWithCarriageReturnLineFeed` | RH3002-010 | `VerifyNoCodeFixIsOffered` + `ConvertEndOfLine` | pass |
| `VerifyConditionalDirectiveWithActiveCodeInsideReturnedParenthesesIsReportedWithoutCodeFix` (`#if !NEVER`, guarded code active) | RH3002-010; FC-020 | `VerifyNoCodeFixIsOffered` | pass |
| `VerifyIfFalseDirectiveInsideReturnedParenthesesIsReportedWithoutCodeFix` | RH3002-010; FC-020 | `VerifyNoCodeFixIsOffered` | pass |
| `VerifyElseDirectiveInsideReturnedParenthesesIsReportedWithoutCodeFix` | RH3002-010; FC-020 | `VerifyNoCodeFixIsOffered` | pass |
| `VerifyElifDirectiveInsideReturnedParenthesesIsReportedWithoutCodeFix` | RH3002-010; FC-020 | `VerifyNoCodeFixIsOffered` | pass |
| `VerifyConditionalDirectiveInsideArgumentParenthesesIsReportedWithoutCodeFix` | RH3002-010; FC-027 | `VerifyNoCodeFixIsOffered` | pass |
| `VerifyReceiverParenthesesAroundExcludedCodeAreReportedWithoutCodeFix` (receiver, `X` undefined: content is the name `a`) | RH3002-010 | `VerifyNoCodeFixIsOffered` | pass |
| `VerifyReceiverParenthesesAroundActiveBinaryCodeAreNotReported` (same input, `X` defined: content is `a + 1`, the other side of "judged as the current build compiles it") | RH3002-010 | analyzer `Verify(source, onConfigure)` with a preprocessor-symbol solution transform | pass (mutation check: symbol `Y` instead of `X` fails with one unexpected diagnostic) |
| `VerifyReceiverParenthesesAroundElseBranchNameAreReportedWithoutCodeFix` | RH3002-010; FC-020 | `VerifyNoCodeFixIsOffered` | pass |
| `VerifyFixAllLeavesParenthesesWithConditionalDirectiveReported` (a fixable pair next to a withheld one; Fix All and one-pass convergence) | RH3002-009, RH3002-010; FC-008 | `Verify(source, fixedSource, onConfigure)` with `MarkupMode.Allow` on the fixed and batch-fixed states | pass |

`VerifyNoCodeFixIsOffered` checks both halves of the requirement: the marked diagnostic `(`…`)` is reported, and the code fix registers no action for it (FC-005, the guard covers directives and not only comments). It also fails when nothing is reported, so the not-reported side uses the analyzer-only `Verify` instead.

## Scenarios (bug fix)
_N/A_ — tests lane, no defect claimed.

## Not covered, and why
- Formatter surface, idempotency, combined pipeline: no formatter phase removes parentheses (skeptic, *Surfaces*), so no formatter test applies.
- A CRLF variant for every shape: the analyzer and the fix guard do not depend on the line ending (they inspect parent kinds and `DescendantTrivia`), so one CRLF repeat of the spec example is enough. Line-ending layout is not involved, because no fix runs.
- Inner pair of nested parentheses with a directive: the skeptic left it unsettled, and RH3002-010 is about the pair whose parentheses hold the directive. Nesting is covered by position (argument, receiver, return), not depth.
- Token gap or join, naming or Renamer, unsupported shapes (FC-038 to FC-043): none apply. No join, rename, or declared shape is involved.
- `#region` or `#pragma` inside the parentheses: SHARED-DIAG-008 already does not let them suppress a report, and RH3002-010 covers conditional directives only.

## Commands
```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzerTests"
scripts/test.sh --project architecture --filter "FullyQualifiedName~RequirementCoverageTests"
```

Role: implementer
Commit: a4add0db
Verdict: #1279: tests pass; #1281: tests pass (no code change); #1257, #1282, #1417, #1422: nothing to do

Class-ending narrowing after audit 6 (lead rows 46–49, auditor findings 20–22 in `38-auditor.md`, spec 8f19631c, tests 82307f37 in `40-test-implementer.md`). My prompt described row 47's two guards and which helpers to delete; that matches the spec diff (RH3004-008/-009/-006), so I built from the spec and tests. It carried no suspected cause or hoped-for verdict. No output question raised (standing instruction, row 46).

## thoenissen/Reihitsu#1279

### Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Clarity/RH3004UseLambdaSyntaxCodeFixProvider.cs` | New guard `ContainsDirectiveOrDisabledText` (both paths): any directive in `anonymousMethodExpression.Span`, from `async`/`delegate` through `}`, via `SyntaxTriviaUtilities.ContainsDirectives`. Disabled text needs no own check: it ends at a directive that also lies inside the span, because a token of the anonymous method follows it (stated in the summary). Replaces `ContainsCrossingDirectiveChain` | RH3004-009 |
| same | New guard `ContainsStringOrCommentSpanningLines` (block path): any `IsString` node of the block, or `/* */`/`/** */` trivia inside `block.Span`, whose start and end lie on different lines (`SpansLines`, SourceText line numbers). Replaces `ContainsStringWithDirectiveLine` and `ContainsMultiLineRawStringLiteral` (a block-body multi-line raw string is a string that spans lines) | RH3004-008, RH3004-007 |
| same | Deleted: `ContainsCrossingDirectiveChain`, `ContainsStringWithDirectiveLine`, `IsOpenStringToken`, `ContainsLineStartingWithHash`, `ContainsMultiLineRawStringLiteral`, `IsMultiLineRawStringLiteralToken`, `GetOutermostString`, `AddLineElements`; the disabled-text lexing in `AddTriviaElements`; the string-as-one-element logic in `GetLineElements`; in `IsMovingLine`, the "line starts inside a string begun earlier", directive-line, and comment-continuation (recursive) decisions; in `GetOpenBraceTail`, the multi-line-comment condition (the brace's trailing trivia lies in `block.Span`, so the new guard covers it) | RH3004-006 |
| same | `IsMovingLine` now: a line holding code moves; a line without code stays iff its first comment starts in column 0; a later line of a `///` run (the only element that still spans lines) is decided on its own marker. Summaries of `MoveStatementsAsync`, `GetFirstStatementLinePosition`, `GetLineElements`, `AddTriviaElements`, `IsCode`, `IsMovingLine` rewritten to the new code (FC-031) | RH3004-006 |

Net: 66 insertions, 216 deletions in the provider (first commit bd581d46), then the Core reuse (a4add0db).

### Policy owners

- `rg -n "static bool (Contains|Span)\w*(Directive|DisabledText)\w*\(" src --glob '!*Test*'`: `SyntaxTriviaUtilities.ContainsDirectives` (Core) is the owner of "a span holds a directive"; reused. The others (`ContainsUnbalanced*`, `ContainsRegionDirectives`, `ContainsPositionSensitiveDirectives`, `SyntaxNodeUtilities.SpanContainsCommentOrDirective`, RH7309/RH7310/`AccessorExpressionBodyTransform` private checks, `UsingDirectiveOrderingSafety`) ask different questions (balance, region-only, comments too).
- `rg -n "SpansLines|IsMultiLine\w*\(|static bool \w*SpanningLines" src --glob '!*Test*'`: `ListElementInteriorJoinRewriter.SpansLines` (private, formatter, token/node via its own line test) and `LineBreakDetection.IsMultiLine` (formatter, a node). Neither is reachable from a code fix for this purpose without using formatter internals, and neither takes trivia, so the 1-line private `SpansLines` stays.

### Tests run

```shell
scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH3004UseLambdaSyntaxAnalyzerTests|FullyQualifiedName~RH3106UnnecessaryDelegateParenthesesShouldBeRemovedAnalyzerTests"
```
Before (82307f37): `Failed: 45, Passed: 165, Total: 210` — every failure "No code fix may be offered", as handed over. After: `Passed! - Failed: 0, Passed: 210, Total: 210`. After the Core reuse, RH3004 alone: `Passed: 118, Total: 118`.

## thoenissen/Reihitsu#1281

No code change (rows 48–49). The RH3106 tests pass in the focused run above.

## thoenissen/Reihitsu#1257, #1282, #1417, #1422

Nothing to do; untouched.

## Whole run

```shell
scripts/test.sh --configuration Debug --project analyzer      # Passed! - Failed: 0, Passed: 6992, Total: 6992
scripts/test.sh --configuration Debug --project core          # Passed! - Failed: 0, Passed: 649, Total: 649
scripts/test.sh --configuration Debug --project architecture  # Passed! - Failed: 0, Passed: 115, Total: 115
scripts/build.sh                                              # 0 Warning(s), 1 Error(s)
scripts/format.sh src/Reihitsu.Analyzer.CodeFixes/Rules/Clarity/RH3004UseLambdaSyntaxCodeFixProvider.cs  # Formatted 0 of 1 file(s)
```

The one Release error is not in production code: `src/Reihitsu.Analyzer.Test/Clarity/RH3004UseLambdaSyntaxAnalyzerTests.cs(3762,27): error RH5112: Wrapped fluent calls should keep the first call on the original line.` It is the `""".Replace("<form-feed>", "\f", System.StringComparison.Ordinal);` of `FixWithheldForAStringWhoseDirectiveLineStartsWithAFormFeed`, added in 82307f37. I did not touch the test file; it is the cleaner's to resolve (layout only, no assertion change). The production projects build with 0 warnings.

## Disputed tests or scope

_None._ Scope shrank: one file, net −150 lines.

## Architecture findings

_None._ The guards end the reparse hazard of auditor finding 21 by withholding; the structural cause (the fix reassembles text and reparses with no symbols) stays the follow-up already listed in `00-lead.md`.

## Follow-up candidates

_None new._

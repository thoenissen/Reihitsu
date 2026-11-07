Role: test implementer
Commit: 10433905
Verdict: #1279: tests written (RH3004-007/-008 amended, auditor finding 23 a–c); #1257, #1281, #1282, #1417, #1422: nothing to do

Lead row 50 (`00-lead.md`), auditor finding 23 (`44-auditor.md`, fixtures `audit7/rh3004/e2_expression_body.cs`, `e3_param_comment.cs`, `e3b_param_comment_expr.cs` in the session scratchpad, copied verbatim), spec 10433905 (`45-spec-author.md`). Expected values come from the RH3004-007/-008 examples and normative sentences, never from the code. No production code and no spec touched. Member (d) is row 51 (follow-up), not tested here. My prompt listed the work to do and carried no intended fix, suspected cause, or hoped-for verdict. No output questions raised (standing instruction, row 50).

"Fails" means the test fails on 10433905 at `Assert.IsEmpty(actions, "No code fix may be offered …")` (`AnalyzerTestsBase{TAnalyzer,TCodeFix}.cs:286`), after `VerifyNoCodeFixIsOffered`'s diagnostic check passed. All 11 failures are that assertion. All tests are in `src/Reihitsu.Analyzer.Test/Clarity/RH3004UseLambdaSyntaxAnalyzerTests.cs`.

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `FixWithheldForASingleReturnOfAStringThatHoldsALineStartingWithHash` (flipped and renamed from `FixRewritesASingleReturnOfAStringThatHoldsALineStartingWithHashAsAnExpressionBody`; input unchanged, tag RH3004-003 dropped since it no longer asserts the expression-body shape) | RH3004-008 | `VerifyNoCodeFixIsOffered` | fails |
| `FixWithheldForASingleReturnOfAStringThatHoldsALineStartingWithHashWithCarriageReturnLineFeed` | RH3004-008 | `VerifyNoCodeFixIsOffered` | fails |
| `FixWithheldForASingleReturnOfAStringWhoseHashLineStartsWithAFormFeed` (form feed inserted by ordinal `Replace`) | RH3004-008 | `VerifyNoCodeFixIsOffered` | fails |
| `FixWithheldForASingleReturnOfAStringWhoseHashLineStartsWithSpacesAndATab` (tab inserted by ordinal `Replace`) | RH3004-008 | `VerifyNoCodeFixIsOffered` | fails |
| `FixRewritesASingleReturnOfAMultiLineVerbatimStringWithoutAHashLineAsAnExpressionBody` (new offered spec example, `@"a⏎b"`) | RH3004-003, RH3004-008 | `Verify` | passes |
| `…WithoutAHashLineAsAnExpressionBodyWithCarriageReturnLineFeed` | RH3004-003, RH3004-008 | `Verify` | passes |
| `FixRewritesASingleReturnOfAStringWhoseLineHoldsHashBehindOtherTextAsAnExpressionBody` (`@"a⏎b #c"`, "Line start" side) | RH3004-003, RH3004-008 | `Verify` | passes |
| `FixWithheldForAReturnedStringThatHidesTheElseOfAChainAroundTheAnonymousMethod` (23 a) | RH3004-008 | `VerifyNoCodeFixIsOffered` | fails |
| `FixWithheldForAParameterListCommentThatHidesTheElseOfAChainOnTheBlockBodyPath` (23 b) | RH3004-008 | `VerifyNoCodeFixIsOffered` | fails |
| `FixWithheldForAParameterListCommentThatHidesTheElseOfAChainOnTheExpressionBodyPath` (23 c) | RH3004-008 | `VerifyNoCodeFixIsOffered` | fails |
| `FixWithheldForASingleReturnOfAMultiLineRawStringLiteralThatHoldsALineStartingWithHash` (raw string, `    #b`) | RH3004-007, RH3004-008 | `VerifyNoCodeFixIsOffered` | fails |
| `FixWithheldForADelimitedCommentInTheParameterListThatSpansLinesOnTheBlockBodyPath` (spec example `e`, no `#` line) | RH3004-008 | `VerifyNoCodeFixIsOffered` | fails |
| `…OnTheBlockBodyPathWithCarriageReturnLineFeed` | RH3004-008 | `VerifyNoCodeFixIsOffered` | fails |
| `FixWithheldForAParameterListCommentWithAHashLineOnTheExpressionBodyPath` (`/* first⏎#second */`, no directive in the document; "Place" side on the expression-body path) | RH3004-008 | `VerifyNoCodeFixIsOffered` | fails |

The spec puts example `e` into the existing four-of-a-kind block; I kept `FixWithheldForStringsAndCommentsThatSpanLinesWithoutAnyDirective` (still passing) and covered `e` in its own focused test instead of making a passing test fail.

Unchanged and still passing: `FixRewritesASingleReturnOfAMultiLineRawStringLiteralAsAnExpressionBody` (RH3004-007 offered side, no `#` line), `FixRewritesASingleReturnOfAnInterpolatedVerbatimStringAsAnExpressionBodyAndKeepsItsValue`, every earlier RH3004-008/-009 test.

## Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| 23 (a) returned string hides `#else` | LF | code fix | reproduced | fix offered vs none |
| 23 (b) parameter-list comment, block path | LF | code fix | reproduced | fix offered vs none |
| 23 (c) parameter-list comment, expression path | LF | code fix | reproduced | fix offered vs none |
| `return @"a⏎#b";`, no directive | LF, CRLF | code fix | fails | fix offered vs none |
| `#b` behind form feed / spaces+tab | LF | code fix | fails | fix offered vs none |
| raw string with `    #b` | LF | code fix | fails | fix offered vs none |
| param-list comment, no `#` line, block path | LF, CRLF | code fix | fails | fix offered vs none |

## Not covered, and why

- Finding 23 (d) (carrier before the anonymous method): row 51, a follow-up; RH3004-008 states it as out of scope.
- A spanning comment in the parameter list without a `#` line on the expression-body path: offered by the spec, but no spec example states its output (where the comment lands), so an expected value would have to come from the code (FC-035). Left to the spec author if wanted.
- A multi-line string in the parameter list: not reachable (CS7014 / CS1065, auditor).
- Fix All: every new withheld test asserts no action per diagnostic; the offered tests run through `Verify`, which includes the batch base's checks. No new co-occurrence shape.
- Formatter parity / idempotency: no formatter change; the offered outputs are laid out by the existing node-level entry point.
- Other failure classes (renames, member kinds, token gaps between list elements): no change touches them.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Clarity.RH3004UseLambdaSyntaxAnalyzerTests"
```
Result on 10433905: 131 total, 120 passed, 11 failed (the 11 rows marked "fails").

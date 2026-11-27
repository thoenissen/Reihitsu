Role: test implementer
Commit: 76adce2 (tests written against this head; the commit that adds them is the one carrying this handover)
Verdict: tests written

Lane: tests. Every new test passes on the unchanged code; no production file is touched. The prompt carried no intended fix, suspected cause, or hoped-for verdict. All tests are in `tests/Reihitsu.Analyzer.Test/Clarity/RH3004UseLambdaSyntaxMultiLineTokenAndDirectiveTests.cs` (class `RH3004UseLambdaSyntaxMultiLineTokenAndDirectiveTests`, `BatchCodeFixTestsBase`). The spec-example inputs and outputs were extracted verbatim from `sdd/specs/analyzer/RH3004.md` by script, not retyped; the other expected values follow RH3004-011's wording (later lines keep their columns, parameter list in the column of `delegate` or behind `async `, ` => ` and the body behind `)` on its line) and FMT-SPACE-014 for a03. A deliberately wrong column in the first test was confirmed to fail it, then reverted.

## Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `FixKeepsTheLaterLineOfAParameterListCommentThatSpansLinesInItsColumn` (RH3004-011 example 1; skeptic a12; two diagnostics in one document, Fix All) | RH3004-011; FC-008, FC-010/FC-013 (`AssertFormattingChangesNothing` on the result) | `Verify` + `AssertFormattingChangesNothing` | pass |
| `FixKeepsTheLaterLineOfAParameterListCommentLeftOfTheNewParameterListColumn` (issue input, a01) | RH3004-011 | `Verify` | pass |
| `FixKeepsTheLaterLineOfAParameterListCommentLeftOfTheNewParameterListColumnWithCarriageReturnLineFeed` | RH3004-011; FC-017 (CRLF) | `VerifyCodeFixWithEndOfLine(..., "\r\n")` | pass |
| `FixKeepsTheLaterLineOfAParameterListCommentOfAnAsyncAnonymousMethod` (a02) | RH3004-011 | `Verify` | pass |
| `FixPlacesTheExpressionBodyBehindAClosingParenthesisOnALineAfterTheParameterListComment` (RH3004-011 example 2; a14) | RH3004-011; FC-010/FC-013 | `Verify` + `AssertFormattingChangesNothing` | pass |
| `FixPlacesTheExpressionBodyBehindAClosingParenthesisOnALineAfterTheParameterListCommentWithCarriageReturnLineFeed` | RH3004-011; FC-017 | `VerifyCodeFixWithEndOfLine(..., "\r\n")` | pass |
| `FixKeepsAParameterListCommentBehindACommaAndIsWithheldForOneInFrontOfACommaOnALaterLine` (RH3004-011 example 3; a05 next to a04) | RH3004-011, SHARED-FIX-010; FC-003 | `Verify` with `FixedState`/`BatchFixedState.MarkupHandling = MarkupMode.Allow` (remaining diagnostic marked) | pass |
| `FixWithheldForAParameterListCommentBetweenAParameterAndACommaOnALaterLine` (a04 alone, asserts no action for the diagnostic) | RH3004-011, SHARED-FIX-010 | `VerifyNoCodeFixIsOffered` | pass |
| `FixKeepsTheLaterLineOfACommentInFrontOfTheFirstParameter` (a03) | RH3004-011; FC-021 (comment in front of a token) | `Verify` | pass |
| `FixKeepsTheLaterLineOfADelimitedDocumentationCommentInTheParameterList` (a06, `/** */`) | RH3004-011 | `Verify` | pass |
| `FixMovesTheParameterListOntoTheLineOfDelegateAndKeepsTheLaterCommentLine` (a08) | RH3004-011; FC-027 | `Verify` | pass |
| `FixKeepsTheLaterLineOfAParameterListCommentInColumnZero` (a10) | RH3004-011 | `Verify` | pass |
| `FixKeepsEveryLaterLineOfAParameterListCommentOverThreeLines` (a11, comment part) | RH3004-011 | `Verify` | pass |
| `FixKeepsTheLaterLineOfAParameterListCommentWhenTheBlockStandsOnLinesOfItsOwn` (a13) | RH3004-011 | `Verify` | pass |
| `FixWithheldForADelimitedCommentThatOpensBehindTheOpeningBraceAndEndsInFrontOfTheFirstStatement` (RH3004-008 new example; b01 + b02) | RH3004-008 | `VerifyNoCodeFixIsOffered` (checks every diagnostic) | pass |

`Verify` runs the incremental and the Fix All arms and re-analyzes the result, which covers one-pass convergence (FC-007).

## Scenarios (bug fix)
_N/A_ — tests lane, existing behavior.

## Not covered, and why
- Block-body path with a parameter-list comment over several lines: pinned by `FixWithheldForADelimitedCommentInTheParameterListThatSpansLinesOnTheBlockBodyPath` and its CRLF variant.
- Later line starting with `#` (a09): pinned by `FixWithheldForAParameterListCommentWithAHashLineOnTheExpressionBodyPath`.
- Directive in the gap (FC-019, FC-020) and issue item 3 (inactive nested directives): every directive or disabled text in the anonymous method withholds the fix by RH3004-009 (`ContainsDirectiveOrDisabledText`), pinned by the 30+ existing RH3004-009 tests, the parameter-list ones included (`FixWithheldForAChainInsideTheParameterListOnTheExpressionBodyPath`, `FixWithheldForAChainFromTheParameterListIntoTheBlock`). No new test, as the skeptic recommended.
- a07 (anonymous method as an argument): skeptic marked "none needed"; the wrap of `, 1` is outside the fixed node (SHARED-FIX-007).
- a11's multi-line returned expression: its layout is the formatter's (RH3004-007), not RH3004-011; the three-line comment part is covered.
- `//` comment in the parameter list: outside RH3004-011, left unspecified by decision (skeptic, internal choice 3).
- Separate CRLF variants for a02, a03, a06, a08, a10, a11, a13: the two CRLF tests cover both placements of `)`; the others share the same path (the fix's `CreateLambdaExpression` plus `FormatNodeInDocumentAsync`) and the skeptic's apply-fix CRLF arm matched LF for all of them.
- Existing tests asserting a changed output: none — the spec diff changes no output (`rg` over the three RH3004 test files and the code-fix tests found no test asserting a parameter-list comment over several lines on the expression-body path, nor a comment opened behind `{`).
- Failure classes not applicable: FC-018/FC-036 (no rename), FC-022/FC-023/FC-024/FC-025 (no literal, identifier, or member kind in the changed requirements), FC-038 to FC-043 (no shape declared unsupported in this run; FMT-UNSUP-009 is only referenced), FC-011/FC-012/FC-014 (no formatter phase changes), FC-031/FC-032/FC-035/FC-037 (no production change; expected values from the spec).

## Commands
```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Clarity.RH3004UseLambdaSyntax"
scripts/test.sh --project architecture
```
Both green at the commit of this handover (186 RH3004 tests, 198 architecture tests; `EveryRequirementOfAnEnforcedSpecIsCovered` now passes for RH3004-011).

## Follow-up candidates
_None._

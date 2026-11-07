Role: test implementer
Commit: 6870a6e
Verdict: #1075: reproduced; #1261: reproduced (comment duplication); region-text instance not reproduced (characterization test added)

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Expected values come from the spec examples and requirements at 6870a6e and from `00-lead.md` decisions 4–10 (decision 4: RH5011 inserts below the comment's last line, withholds when code follows on that line; decision 10: no `else if` test).

All new tests are appended to the existing test classes, inside their `#region Tests`. "Fails" below means: fails on 6870a6e for the issue's own expected-versus-actual difference (checked one by one in the test output); "passes" means a guard on the other side of a condition, green today and expected to stay green.

## thoenissen/Reihitsu#1075

### Tests

All in `src/Reihitsu.Analyzer.Test/Formatting/`.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH5011…Tests.VerifyFixInsertsBlankLineBelowCommentThatSpansTheBreakLine` (spec example 1: a1, a3, a5) | RH5011-007 | `Verify(source, fixed)` | fails: empty lines inserted inside the comments |
| `RH5011…Tests.VerifyFixInsertsBlankLineBelowCommentSpanningBreakLineBeforeStatementInOneApplication` (a1) | RH5011-007; FC-007 | `Verify` with `NumberOfIncrementalIterations = NumberOfFixAllIterations = 1` | fails: blank line inserted inside the comment |
| `RH5011…Tests.VerifyFixInsertsBlankLineBelowCommentSpanningBreakLineWithCarriageReturnLineFeed` (a1, CRLF) | RH5011-007, SHARED-FIX-008; FC-033 | `ApplyCodeFixAsync` exact text, then `Verify(actual)` | fails: `/* a\r\n\r\n b */` |
| `RH5011…Tests.VerifyFixInsertsBlankLineBelowCommentSpanningBreakLineBeforeSwitchLabel` (a3) | RH5011-007; FC-007 | `Verify`, one iteration | fails |
| `RH5011…Tests.VerifyFixInsertsBlankLineBelowLineCommentThatEndsTheSpanningCommentsLine` (a5) | RH5011-007; FC-021 | `Verify`, one iteration | fails: blank line inside `/* a … b */`, none above `// note` |
| `RH5011…Tests.VerifyFixInsertsBlankLineBelowDelimitedDocumentationCommentSpanningBreakLine` (a6) | RH5011-007 | `Verify` | fails: blank line inside `/** a … b */` |
| `RH5011…Tests.VerifyFixInsertsBlankLineBelowCommentSpanningBreakLineBeforeDirective` | RH5011-007; FC-019, FC-020 | `Verify` | fails: blank line inside the comment |
| `RH5011…Tests.VerifyNoFixIsOfferedWhenCodeStartsBehindCommentSpanningBreakLine` (spec example 2: a2, a4) | RH5011-007 | `VerifyNoCodeFixIsOffered` | fails: 1 action offered |
| `RH5011…Tests.VerifyNoFixIsOfferedWhenStatementStartsBehindCommentSpanningBreakLine` (a2) | RH5011-007 | `VerifyNoCodeFixIsOffered` | fails |
| `RH5011…Tests.VerifyNoFixIsOfferedWhenSwitchLabelStartsBehindCommentSpanningBreakLine` (a4, LF and CRLF) | RH5011-007 | `VerifyNoCodeFixIsOffered` | fails |
| `RH5011…Tests.VerifyFixAllHandlesSpanningCommentNextToPlainBreakAndWithheldBreak` | RH5011-004, RH5011-007; FC-008 | `Verify`, remaining diagnostic in fixed markup | fails: four empty lines inside the comment |
| `RH5011…Tests.VerifyFixInsertsBlankLineBelowBreakLineAboveMultiLineCommentOnNextLine` | RH5011-004; FC-003 | `Verify` | passes |
| `RH5025…Tests.VerifyEmptyLineAboveHoleBraceInRawInterpolatedStringIsNotReported` | RH5025-006; FC-022 | `Verify(source)` | fails: reported at (7,1) |
| `RH5025…Tests.VerifyEmptyLineAboveHoleBraceInRawInterpolatedStringIsNotReportedWithCarriageReturnLineFeed` | RH5025-006; FC-033 | `Verify(source)` | fails |
| `RH5025…Tests.VerifyEmptyLineAboveHoleBraceInMultipleDollarRawInterpolatedStringIsNotReported` | RH5025-006; FC-022 | `Verify(source, MarkupMode.None)` | fails |
| `RH5025…Tests.VerifyEmptyLineAboveHoleBraceInVerbatimInterpolatedStringIsNotReported` (`$@"` and `@$"`) | RH5025-006; FC-022 | `Verify(source)` | fails: 2 reported |
| `RH5025…Tests.VerifyBlankLineAboveLambdaBraceInsideInterpolationHoleIsReportedAndFixed` | RH5025-006; FC-003 | `Verify(source, fixed)` | passes |
| `RH5027…Tests.VerifyBlankLineAboveFooterLineStartingWithCodeIsNotReported` | RH5027-001; FC-002 | `Verify(source)` | fails: reported |
| `RH5027…Tests.VerifyBlankLineAboveFooterLineStartingWithCodeIsNotReportedWithCarriageReturnLineFeed` | RH5027-001; FC-033 | `Verify(source)` | fails |
| `RH5027…Tests.VerifyEmptyLineInsideVerbatimStringEndingInFrontOfFooterIsNotReported` | RH5027-001; FC-022 | `Verify(source)` | fails |
| `RH5027…Tests.VerifyEmptyLineInsideRawInterpolatedStringEndingInFrontOfFooterIsNotReported` | RH5027-001; FC-022 | `Verify(source)` | fails |
| `RH5027…Tests.VerifyEmptyLineInsideMultiLineCommentEndingInFrontOfFooterIsNotReported` | RH5027-001; FC-021 | `Verify(source)` | fails |
| `RH5027…Tests.VerifyBlankLineAboveCommentInFrontOfFooterIsNotReported` (`/* c */ while`) | RH5027-001; FC-002 | `Verify(source)` | fails |
| `RH5027…Tests.VerifyBlankLineAboveClosingBraceAndCommentInFrontOfFooterIsNotReported` (`} /* c */ while`) | RH5027-001; FC-002 | `Verify(source)` | fails |
| `RH5027…Tests.VerifyBlankLineAboveFooterBelowEmbeddedStatementIsReportedAndFixed` | RH5027-001, RH5027-004; FC-003 | `Verify(source, fixed)` | passes |
| `RH5030…Tests.VerifyNoFixIsOfferedWhenCommentHoldsEveryLineBreak` (spec example, b2) | RH5030-003 | `VerifyNoCodeFixIsOffered` | fails: 1 action offered |
| `RH5030…Tests.VerifyNoFixIsOfferedWhenCommentHoldsEveryLineBreakWithCarriageReturnLineFeed` | RH5030-003; FC-033 | `VerifyNoCodeFixIsOffered` | fails |
| `RH5030…Tests.VerifyNoFixIsOfferedWhenCommentHoldsEveryLineBreakInSwitchSection` (b3) | RH5030-003 | `VerifyNoCodeFixIsOffered` | fails |
| `RH5030…Tests.VerifyNoFixIsOfferedWhenDelimitedDocumentationCommentHoldsEveryLineBreak` (b6) | RH5030-003 | `VerifyNoCodeFixIsOffered` | fails |
| `RH5030…Tests.VerifyNoFixIsOfferedWhenTwoCommentsHoldEveryLineBreak` (b7) | RH5030-003 | `VerifyNoCodeFixIsOffered` | fails |
| `RH5030…Tests.VerifyFixAllLeavesWithheldBraceReported` | RH5030-003, SHARED-AFTERBRACE-001; FC-008 | `Verify`, remaining diagnostic in fixed markup | fails: `b */ ⏎a++;` at column 0 |
| `RH5030…Tests.VerifyFixInsertsBlankLineWhenLineBreakOutsideCommentEndsMultiLineCommentLine` (b1) | RH5030-003, SHARED-AFTERBRACE-001; FC-003 | `Verify`, one iteration | passes |
| `RH5030…Tests.VerifyFixInsertsBlankLineWhenLineCommentEndsMultiLineCommentLine` (b4) | RH5030-003, SHARED-AFTERBRACE-001; FC-003 | `Verify`, one iteration | passes |
| `RH5030…Tests.VerifyFixInsertsBlankLineBehindDirectiveFollowingMultiLineComment` | RH5030-003, SHARED-AFTERBRACE-001; FC-019, FC-020 | `Verify` | passes |

### Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| RH5025 `$"""⏎a⏎⏎{x}⏎"""` | LF, CRLF | analyzer | reproduced | reported at the hole's line vs not reported |
| RH5025 `$$"""…{{x}}` | LF | analyzer | reproduced | reported vs not reported |
| RH5025 `$@"a⏎⏎{x}"`, `@$"…"` | LF | analyzer | reproduced | 2 reported vs none |
| RH5027 `a++;⏎⏎do a++; while (…);` | LF, CRLF | analyzer | reproduced | reported vs not reported |
| RH5027 literal / comment / `/* c */ while` / `} /* c */ while` | LF | analyzer | reproduced | reported vs not reported |
| RH5011 `break; /* a⏎b */⏎a++;` | LF, CRLF | code fix, one pass | reproduced | empty line inside the comment vs below `b */` |
| RH5011 a3, a5, a6, directive below the comment | LF | code fix | reproduced | empty line inside the comment |
| RH5011 a2, a4 | LF, CRLF (a4) | code fix registration | reproduced | 1 action vs none |
| RH5011 Fix All (spanning + plain + withheld) | LF | Fix All | reproduced | 4 empty lines inside the comment |
| RH5030 `} /* a⏎b */ a++;` and b3, b6, b7 | LF, CRLF (b2) | code fix registration | reproduced | 1 action vs none |
| RH5030 Fix All (b1 + plain + b2) | LF | Fix All | reproduced | b2 rewritten to `b */ ⏎a++;` at column 0 vs left reported |

### Not covered, and why

- RH5023 hole case: maintainer answer B, not in this run (follow-up candidate). No test.
- RH3208 through `BlankLineCodeFixUtilities`: its wrong output is a follow-up candidate; a test of today's output would lock it. Existing RH3208 tests guard the other shapes.
- RH5011 row a6 under `DocumentationMode`: the verifier's harness reports `break; /** a⏎b */⏎a++;` (Roslyn lexes a `/**` in trailing trivia as an ordinary multi-line comment), so the test asserts the inserting class, as `03-spec-author.md` asks. Same for RH5030 b6.
- FC-006, FC-018, FC-023–FC-025, FC-029, FC-036: no rename, no expression-body or naming rewrite, no semantic change in these fixes.
- FC-010–FC-014: no formatter phase changes for #1075.
- FC-026: RH5011 and RH5030 locate their tokens by diagnostic span; covered by the Fix All tests with several instances of the same token in one document.
- RH5025/RH5027 code fixes must stay: their existing tests (`VerifyFixDeletes…`) cover them.

## thoenissen/Reihitsu#1261

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH5405…Tests.VerifyFixWritesTrailingCommentOfBodyOnce` (spec example, the issue's shape) | SHARED-BRACE-002 | `Verify(source, fixed)` | fails: `} // trail` |
| `RH5405…Tests.VerifyFixWritesTrailingCommentOfBodyOnceWithCarriageReturnLineFeed` | SHARED-BRACE-002; FC-033 | `Verify` CRLF | fails |
| `RH5405…Tests.VerifyFixWritesTrailingCommentOfBodyOnLineOfIfOnce` | SHARED-BRACE-002; FC-027 | `Verify` | fails |
| `RH5405…Tests.VerifyFixWritesMultiLineTrailingCommentOfBodyOnce` | SHARED-BRACE-002; FC-021 | `Verify` | fails: `} /* a⏎    b */` |
| `RH5405…Tests.VerifyFixWritesTrailingCommentOfNestedIfBodyOnce` | SHARED-BRACE-002; FC-026, FC-027 | `Verify` | fails |
| `RH5405…Tests.VerifyFixAllWritesTrailingCommentsOfBothBranchesOnce` | SHARED-BRACE-002; FC-008 | `Verify` (incremental and Fix All) | fails |
| `RH5405…Tests.VerifyFixKeepsTrailingCommentOfIfBodyFollowedByElseOnce` | SHARED-BRACE-002; FC-003 | `Verify` | passes |
| `RH5406…Tests.VerifyFixWritesTrailingCommentOfMultiLineBodyOnce` | SHARED-BRACE-002; FC-016 | `Verify` | fails |
| `RH5406…Tests.VerifyFixWritesTrailingCommentOfMultiLineBodyOnceWithCarriageReturnLineFeed` | SHARED-BRACE-002; FC-033 | `Verify` CRLF | fails |
| `RH5407…Tests.VerifyFixWritesTrailingCommentOfElseBodyOnce` | SHARED-BRACE-002; FC-016 | `Verify` | fails |
| `RH5407…Tests.VerifyFixWritesTrailingCommentOfElseBodyOnceWithCarriageReturnLineFeed` | SHARED-BRACE-002; FC-033 | `Verify` CRLF | fails |
| `RH5412…Tests.VerifyFixKeepsTextOfEnclosingRegionDirectives` (characterization, decision 7) | SHARED-FIX-003 | `Verify` | passes |
| `src/Reihitsu.Formatter.Test/Unit/ReihitsuFormatterEntryPointTests.FormatNodeInDocumentAsyncWritesTrailingCommentOfWrappedBodyOnce` (spec example) | FMT-PIPE-010; FC-028 | `FormatNodeInDocumentAsync`, LF and CRLF | fails: `} // trail` |
| `ReihitsuFormatterEntryPointTests.FormatNodeInDocumentAsyncWritesTrailingCommentOfBracedTargetOnce` | FMT-PIPE-010; FC-003 | `FormatNodeInDocumentAsync`, LF and CRLF | passes |

Every input avoids the outputs #1209 decided wrong (decision 8 and the skeptic's caution): the `if` is the first statement of its block, the other branch is braced or absent (or both are fixed), and spacing is canonical.

### Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| RH5405 `if (a)⏎    M(a, b); // trail⏎b++;` | LF, CRLF | code fix | reproduced | `} // trail` vs `}` |
| Same shape through `FormatNodeInDocumentAsync` | LF, CRLF | formatter node-level entry point | reproduced | `} // trail` vs `}` |
| RH5405 body on the `if` line; multi-line comment; nested `if`; both branches | LF | code fix, Fix All | reproduced | comment repeated behind `}` |
| RH5406 multi-line body, RH5407 `else` body | LF, CRLF | code fix | reproduced | comment repeated behind `}` |
| RH5412 `#region legacy⏎class A⏎{⏎}⏎#endregion` | LF | code fix | not reproduced | region text kept (FMT-PIPE-028) |

### Not covered, and why

- `else if` body: decision 10, no test (its brace column is #1144's).
- `FormatNodeInDocumentWithContextAsync` with a braced body: none of its callers (RH5111, RH7101, RH3208 fixes, the chain helper) passes a target whose body `ControlFlowBraceTransform` braces (02-skeptic), so no caller reaches it (AGENTS.md, *Supported inputs*).
- RH5413–RH5416, RH5417 region inputs: not reproduced; their outputs carry the blank lines that belong to #1140 (follow-up candidate), so a characterization test would lock a decided-wrong output.
- Comment behind the condition, comment line above the body: existing `VerifyFixKeepsTheTrailingCommentOfAnIfCondition…` tests in the three classes.
- FC-019/FC-020 directive in the gap: `ControlFlowBraceTransform` withholds wrapping when the trailing trivia holds a directive (03-spec-author); existing behavior, not changed by this run.
- FC-004/FC-030 (blank line above the `if`, reformatting of the other branch): #1140, out of the run.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5011|FullyQualifiedName~RH5025|FullyQualifiedName~RH5027|FullyQualifiedName~RH5030|FullyQualifiedName~RH5405|FullyQualifiedName~RH5406|FullyQualifiedName~RH5407|FullyQualifiedName~RH5412"
scripts/test.sh --project formatter --filter "FullyQualifiedName~ReihitsuFormatterEntryPointTests"
scripts/test.sh --project architecture
```

On 6870a6e plus these tests: analyzer 38 failed / 183 passed in that filter (all 38 are the new failing tests above); formatter 1 failed / 31 passed; architecture 115 passed (`RequirementCoverageTests` now finds RH5011-007, RH5025-006, RH5030-003).

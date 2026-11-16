Role: test implementer
Commit: 267010b
Verdict: tests written

My prompt carried no intended fix, suspected cause, or hoped-for verdict. Expected values come from the spec examples (FMT-BLANK-011, RH5020-002, RH8303-002) and the change doc's visible output changes. Where neither states a column, the test asserts only the blank-line state (`AssertBlankLinesAbove`). Every failing test below was run on 267010b and fails only because of the blank line above the first comment (formatter, code fixes), or because RH5020/RH8303 report exactly the first comments (analyzers). No test fails on a compile error.

## Tests

Formatter, `src/Reihitsu.Formatter.Test/Regression/BlankLines/FirstCommentBehindOpeningTokenTests.cs` (new). Every test runs under LF and CRLF, with a second pass and the per-phase stability check:

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `SpecExampleFirstCommentBehindOpeningParenthesisBracketOrLessThanGetsNoBlankLine` (record, union, method, argument list, collection expression, list pattern, type parameter list; C# 15) | FMT-BLANK-011 | `AssertRuleResult` | fail |
| `SpecExampleFirstDocumentationCommentBehindOpeningParenthesisGetsNoBlankLine` (`///` expanded, own-line `/** */`) | FMT-BLANK-011 | `AssertRuleResult` | fail |
| `SpecExampleFirstCommentBehindOpeningBraceOfAnonymousObjectGetsNoBlankLine` | FMT-BLANK-011 | `AssertRuleResult` | pass (brace clarification, today's behavior) |
| `FirstCommentBehindOtherOpeningTokensGetsNoBlankLine` (change doc entry 3: attribute argument list, indexer, type parameter list, tuple, array size, element access) | FMT-BLANK-011 | `AssertRuleResult` | fail |
| `DelimitedDocumentationCommentWrittenBehindOpeningParenthesisIsMovedWithoutBlankLine` (change doc entry 4: the move stays, the blank line goes; FC-012) | FMT-BLANK-011 | `AssertRuleResult` | fail |
| `CommentBehindCommaGetsBlankLineWhileFirstCommentBehindOpeningParenthesisGetsNone` (both sides in one list, `//` and `/* */`; FC-003) | FMT-BLANK-011 | `AssertRuleResult` | fail |
| `CommentBehindCommaInParenthesizedOrBracketedListGetsBlankLine` (comma side alone, parameter list and collection) | FMT-BLANK-011 | `AssertRuleResult` | pass |
| `AuthorsBlankLineBehindOpeningParenthesisBracketOrLessThanIsKept` (kept behind `(`, `[`, `<`; removed behind `{`) | FMT-BLANK-011, FMT-BLANK-007 | `AssertRuleResult` | pass |
| `FirstCommentBehindEveryOtherOpeningTokenGetsNoBlankLine` (attribute list `[`, type argument `<` in a return type and an invocation, `if (`, cast, parenthesized expression, positional and parenthesized pattern, lambda parameter list, nested collection `[`, function pointer `<`; FC-027) | FMT-BLANK-011 | `AssertBlankLinesAbove` | fail |
| `CommentBehindLessThanOperatorGetsBlankLine` (binary `<`, relational pattern `<`; FC-003) | FMT-BLANK-011 | `AssertBlankLinesAbove` | pass |
| `CommentInEmptyListBehindOpeningTokenGetsNoBlankLine` (the comment is owned by `)` and `]`) | FMT-BLANK-011 | `AssertBlankLinesAbove` | fail |
| `FirstCommentBelowTrailingCommentOfOpeningParenthesisGetsNoBlankLine` (`M( // note⏎// first⏎a)`) | FMT-BLANK-011 | `AssertBlankLinesAbove` | fail |
| `CommentBelowDirectiveBehindOpeningBracketGetsNoBlankLine` (`#pragma` between `[` and the comment; FC-019) | FMT-BLANK-011 | `AssertBlankLinesAbove` | pass |
| `TripleSlashCommentWithoutDocumentationParsingFollowsTheOpeningTokenRule` (DocumentationMode None: first `///` gets none, `///` behind a comma gets one; FC-040) | FMT-BLANK-011 | `AssertRuleResult` | fail |

RH5020, `src/Reihitsu.Analyzer.Test/Formatting/RH5020CommentsShouldBePrecededByABlankLineAnalyzerTests.cs`:

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `SpecExampleFirstCommentBehindOpeningParenthesisBracketOrLessThanIsNotReported` | RH5020-002 | `Verify` | fail (5 reports) |
| `SpecExampleFirstCommentBehindOpeningParenthesisBracketOrLessThanIsNotReportedWithCarriageReturnLineFeed` | RH5020-002 | `Verify` (CRLF) | fail (3 reports) |
| `SpecExampleCommentBehindCommaOfParameterListIsReportedAndFixed` | RH5020-001, RH5020-002 | `Verify` with fix | pass |
| `FirstCommentBehindOpeningTokenIsNotReportedWhileCommentBehindCommaIsReported` (parameter list `//`, collection `/* */`; FC-003) | RH5020-001, RH5020-002 | `Verify` with fix | fail (4 reports, 2 expected) |
| `FirstCommentBehindEveryOtherOpeningTokenIsNotReported` (15 openers: attribute list, attribute argument list, indexer, return-type `<`, `if (`, cast, parenthesized expression, tuple, array size, element access, positional, parenthesized and list pattern, lambda, type argument `<`) | RH5020-002 | `Verify` | fail (15 reports) |
| `FirstCommentInUnionCaseTypeListIsNotReported` | RH5020-002 | `Verify` + `UnionSupport` | fail |
| `FirstCommentInFunctionPointerParameterListIsNotReported` | RH5020-002 | `Verify` + AllowUnsafe | fail |
| `CommentBehindLessThanOperatorIsReportedAndFixed` (binary and relational `<`) | RH5020-001, RH5020-002 | `Verify` with fix | pass |
| `CommentInEmptyListBehindOpeningTokenIsNotReported` | RH5020-002 | `Verify` | fail |
| `FirstCommentBelowTrailingCommentOfOpeningParenthesisIsNotReported` | RH5020-002, RH5020-003 | `Verify` | fail |
| `TripleSlashCommentWithoutDocumentationParsingFollowsTheOpeningTokenRule` (DocumentationMode None; FC-040) | RH5020-001, RH5020-002 | `Verify` with fix | fail (first `///` reported) |

RH5020 formatter parity, `src/Reihitsu.Analyzer.Test/Formatter/Formatting/RH5020CommentsShouldBePrecededByABlankLineFormatterTests.cs` (FC-013):

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `VerifyFormatterLeavesFirstCommentBehindOpeningTokenWithoutBlankLine` (input is not reported; output equals the spec output, is not reported, and a second pass is stable) | RH5020-002, FMT-BLANK-011 | `VerifyFormatter(source, fixedSource)` | fail |
| `VerifyFormatterFixesCommentBehindCommaAndLeavesFirstCommentBehindOpeningParenthesis` | RH5020-002, FMT-BLANK-011 | `VerifyFormatter(source, fixedSource, diagnostics)` | fail |

RH8303, `src/Reihitsu.Analyzer.Test/Formatting/RH8303ElementDocumentationHeaderMustBePrecededByBlankLineAnalyzerTests.cs`:

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `SpecExampleFirstParameterAndFirstTypeParameterDocumentationIsNotReported` | RH8303-002 | `Verify` | fail |
| `SpecExampleFirstParameterAndFirstTypeParameterDocumentationIsNotReportedWithCarriageReturnLineFeed` | RH8303-002 | `Verify` (CRLF) | fail |
| `SpecExampleFirstDocumentationBehindTypeArgumentArgumentOrCollectionOpenerIsNotReported` | RH8303-002 | `Verify` | fail (3 reports) |
| `SpecExampleDocumentationBehindCommaOfParameterListIsReportedAndFixed` | RH8303-001, RH8303-002 | `Verify` with fix | pass |
| `FirstTypeParameterDocumentationIsNotReportedWhileDocumentationBehindCommaIsReported` (FC-003) | RH8303-001, RH8303-002 | `Verify` with fix | fail (2 reports, 1 expected) |
| `FirstParameterDocumentationBelowTrailingCommentOfOpeningParenthesisIsNotReported` | RH8303-002 | `Verify` | fail |
| `FirstParameterDelimitedDocumentationIsNotReported` (`/** */`) | RH8303-002 | `Verify` | fail |

RH8303 formatter parity, `src/Reihitsu.Analyzer.Test/Formatter/Formatting/RH8303ElementDocumentationHeaderMustBePrecededByBlankLineFormatterTests.cs` (FC-013):

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `VerifyFormatterKeepsFirstParameterDocumentationWithoutBlankLine` | RH8303-002, FMT-BLANK-011 | `VerifyFormatter(source)` | fail |

Code fixes (change doc entry 7; one pass, then re-analysis by `Verify`; FC-007):

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH3004UseLambdaSyntaxAnalyzerTests.FixLeavesNoBlankLineAboveTheFirstCommentOfTheParameterList` | RH3004-006 | `Verify` with fix | fail (blank line only) |
| `RH5305CollectionExpressionsShouldBeFormattedCorrectlyAnalyzerTests.VerifyFixLeavesNoBlankLineAboveTheFirstCommentOfTheCollectionExpression` | RH5305-004 | `Verify` with fix | fail (blank line only) |

Existing tests that assert a changed output: none. I searched the test projects, code-fix tests included, for each changed output:
- A blank line between an opener and a comment, in raw strings: `rg -U '[(\[<] *\n *\n *(//|/\*)' src -g '*.cs'`. No hit.
- The same in escaped strings: `rg '[(\[<](\\r)?\\n *(\\r)?\\n *//'`. One hit, `CollectionExpressionClosingBracketCommentTests.CommentBeforeClosingBracketOfEmptyCollectionExpressionIsIndented`. That blank line was written by the author, so it is kept and the test is left unchanged.
- An RH5020 or RH8303 report behind an opener: `rg -U '[(\[<] *(//[^\n]*)?\n *\{\|(RH5020|RH8303|#\d+):'`. No hit apart from the new operator tests.

`scripts/test.sh --project architecture`: 153 passed with the new tags.

## Not covered, and why

- FC-038 to FC-043 (unsupported shapes): the run declares no shape. The type-argument `<` cases are placed in a return type and an invocation, because inside a field or local type the comment is refused (FMT-UNSUP-004, 02-skeptic.md).
- FC-008 (Fix All): RH5020's and RH8303's code fixes are not touched. The existing `GetFixAllScenario` of both classes still covers their convergence. The only new co-occurrence, a first comment next to a comma comment in one list, is checked in single-fix tests, where the first comment must not be reported at all.
- FC-028 (detached vs document-scoped): no code fix changes. The RH3004 test (a local in a method) and the RH5305 test (a field initializer) both go through the document-scoped entry point that those fixes already use.
- FC-018, FC-036 (naming), FC-022 to FC-026 (literals, identifiers, partial, expression-bodied kinds): the change is a check of the token kind in front of a comment. No rename, literal, or member kind reaches it.
- FC-015/FC-016 (three copies of the predicate, lead decision 3): there is no shared owner yet to unit-test. Each of the three surfaces (formatter, RH5020, RH8303) gets its own copy of the whole opener matrix, so any drift between them fails a test.
- `,` of a type argument list and `>`: these are covered by the comma side. Every other token that opens nothing follows the same branch. No separate test.
- The column of the first element behind `if (`, a cast, `new int[`, `M<`, and a parenthesized pattern is unspecified (follow-up candidate in 00-lead.md). Those tests assert blank lines only.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~FirstCommentBehindOpeningTokenTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5020|FullyQualifiedName~RH8303|FullyQualifiedName~RH3004UseLambdaSyntaxAnalyzerTests.FixLeavesNoBlankLine|FullyQualifiedName~RH5305CollectionExpressionsShouldBeFormattedCorrectlyAnalyzerTests.VerifyFixLeavesNoBlankLine"
```

Results on 267010b:
- Formatter: 9 failed, 5 passed.
- Analyzer: 20 failed, 89 passed. All failures are new tests. Every pre-existing RH5020 and RH8303 test passes.

## Follow-up candidates

- Formatter layout of a parenthesized pattern whose first content is a comment. `_ = a is (⏎// pattern⏎> 0);` formats today as `_ = a is⏎      (⏎⏎// pattern⏎> 0⏎      );`: the `(` and `)` go onto their own lines, and the comment and pattern land in the statement's column. The blank-line change does not touch this. It is a rare shape (a comment inside a pattern), observed with `scripts/format.sh` on 267010b. It may belong with the existing indentation follow-up candidate.

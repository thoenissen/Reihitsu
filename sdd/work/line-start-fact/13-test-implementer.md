Role: test implementer
Commit: f9d02c6
Verdict: tests written

This is repair round 1, working from `12-auditor.md` and the decision `squad/decisions/2026-10-30-line-start-fact-audit-1-copies.md` (option B). The prompt carried no intended fix. I derived the expected values from FMT-BREAK-014, RH6021-001/-002/-004, and RH6005-001, not from the auditor's suggested changes.

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `Formatter.Test/Regression/Indentation/ZeroWidthTokenLineStartTests.WrappedListWithBlankLineBeforeClosingAngleBracketIsJoinedInOnePass` | FMT-BREAK-014 (FC-010, FC-003) | `AssertRuleResult` (one-pass expected output, second pass, LF+CRLF, phase check) | red |
| `…WrappedListWithBlankLineBeforeFirstSeparatorIsJoinedInOnePass` | FMT-BREAK-014 | same | red |
| `…WrappedListWithBlankLinesBeforeSeveralSeparatorsIsJoinedInOnePass` | FMT-BREAK-014 | same | red |
| `…WrappedListOfSingleOmittedTypeArgumentWithBlankLineIsJoinedInOnePass` | FMT-BREAK-014 | same | red |
| `…WrappedListWithTwoBlankLinesBeforeClosingAngleBracketIsJoinedInOnePass` | FMT-BREAK-014 | same | red |
| `…WrappedListWithWhitespaceOnlyLineBeforeClosingAngleBracketIsJoinedInOnePass` | FMT-BREAK-014 | same | red |
| `…WrappedListWithLineBreakAndThenBlankLineIsJoinedInOnePass` | FMT-BREAK-014 | same | red |
| `…WrappedTypeArgumentListWithTextAndBlankLineIsJoinedInOnePass` (control: type arguments with text) | — | same | green |
| `Analyzer.Test/Formatting/RH6021…AnalyzerTests.VerifyConstructorInitializerColonStartingItsLineAndFollowedByStrayDocumentationCommentIsNotReported` (the auditor's formatter output) | RH6021-002 | `Verify(source)` | red |
| `…RH6021….VerifyConstructorInitializerColonWithOneSpaceBeforeAndStrayDocumentationCommentBehindIsNotReported` | RH6021-002 | `Verify(source)` | red |
| `…RH6021….VerifyBaseListColonFollowedByStrayDocumentationCommentIsNotReported` | RH6021-002 | `Verify(source)` | red |
| `…RH6021….VerifyColonInColumnZeroBehindStrayDocumentationCommentIsNotReported` (leading side broken by a `///`, `:` with no whitespace in front) | RH6021-002 | `Verify(source)` | red |
| `…RH6021….VerifyOnlyLeadingSideIsFixedWhenStrayDocumentationCommentEndsTrailingSide` (`C():/// x`: leading side reported; fix gives `C() :/// x`) | RH6021-001, RH6021-004 | `Verify(source, fixed, …)` | red (the fix leaves the input unchanged) |
| the four `…SingleLineComment…` counterparts of the RH6021 rows above | — (controls) | `Verify(source)` | green |
| `Analyzer.Test/Formatter/Formatting/RH6021…FormatterTests.VerifyFormatterOutputWithStrayDocumentationCommentBehindColonIsNotReported` | RH6021-002 (FC-013) | `VerifyFormatter(source)` | red |
| `Analyzer.Test/Formatting/RH6005…AnalyzerTests.VerifyOperatorKeywordFollowedByStrayDocumentationCommentIsNotReported` (the auditor's formatter output) | RH6005-001 | `Verify(source)` | red |
| `…RH6005….VerifyConversionOperatorKeywordFollowedByStrayDocumentationCommentIsNotReported` | RH6005-001 | `Verify(source)` | red |
| the two `…SingleLineComment…` counterparts of the RH6005 rows above | — (controls) | `Verify(source)` | green |
| `Analyzer.Test/Formatter/Formatting/RH6005…FormatterTests.VerifyFormatterOutputWithStrayDocumentationCommentBehindOperatorKeywordIsNotReported` | RH6005-001 (FC-013) | `VerifyFormatter(source)` | red |
| `Core.Test/SyntaxTokenPositionUtilitiesTests.GetColumnReturnsZeroForTokenAtBeginningOfLine`, `…GetColumnReturnsColumnOfIndentedToken` (moved from `ReihitsuFormatterHelpersTests.ComputeTokenColumn…`) | — | direct call | green |

Every red test fails on its own difference:
- **Joins:** after the first pass the list is still wrapped at the blank-line gap.
- **RH6021 and RH6005:** one unexpected diagnostic.
- **The fix row:** no change, because the reported trailing side carries the comment and the fix withholds itself.

## Finding 1: the class of the blank-line join

Axes and members, each run under LF and CRLF by the helper:

- **Where the extra line break sits:**
  - in front of `>`;
  - in front of the first `,`;
  - in front of each of several `,`;
  - between `<` and `>` of a single omitted argument;
  - a plain line break in front of one `,` combined with a blank line in front of `>`.
- **What the empty line holds:** nothing (one or two blank lines), or only whitespace.
- **Control:** a list whose type arguments have text, with a blank line inside. It passes today.

Excluded:
- **A comment or directive together with a blank line.** The list stays wrapped. FMT-BREAK-014 does not say whether the blank line stays, so I wrote no exact-output test.
- **`int[,` / blank / `]`.** No requirement joins a rank specifier.

## Finding 2: the class for RH6021 and RH6005

**RH6021.** Axes: which colon, which side the `///` breaks, and what is on the other side.

| Colon | Side broken by `///` | Other side | Result |
|---|---|---|---|
| constructor initializer | trailing | starts its line | red |
| constructor initializer | trailing | one space | red |
| base list | trailing | one space | red |
| constructor initializer | leading (colon in column 0) | one space | red |
| constructor initializer | trailing | tight (`C():/// x`) | reported, red on the fix |

- The last row is the other side of the condition (FC-003): it is reported for its leading side only, and the fix must not touch the trailing side (RH6021-004).
- The leading-side member with indentation in front of `:` is not a member: whitespace stands there, so RH6021-001 does not apply.
- Every `//` counterpart is green.

**RH6005.** Members:
- operator declaration;
- conversion operator.

Both are red with `///`; the `//` counterparts are green.

Formatter output (FC-013):
- The formatter keeps `:/// x` and `operator/// x` (constructor initializer, operator `+`). `VerifyFormatter(source)` pins that output as unchanged and analyzer-clean.
- For the base-list and conversion-operator shapes, the documentation-comment phase moves the comment in front of `B` or `int`. I checked this with `scripts/format.sh` on cb59e98-era code. Those two shapes therefore have analyzer tests only.

## Finding 3: `ComputeTokenColumn`

- **Test references:** the only ones were `src/Reihitsu.Formatter.Test/Unit/ReihitsuFormatterHelpersTests.cs`, `ComputeTokenColumnReturnsZeroForTokenAtBeginningOfLine` and `…ReturnsCorrectColumnForIndentedToken`. I deleted both.
- **Coverage now:** the same assertions run against `SyntaxTokenPositionUtilities.GetColumn` in `src/Reihitsu.Core.Test/SyntaxTokenPositionUtilitiesTests.cs`. That method in `Reihitsu.Core` computes the same value (`token.GetLocation().GetLineSpan().StartLinePosition.Character`).
- **Remaining references:** no test names `ReihitsuFormatterHelpers.ComputeTokenColumn` any more. Its only remaining caller is `src/Reihitsu.Analyzer.CodeFixes/Base/TargetAttributePlacementCodeFixProviderBase.cs:111`.

## Not covered, and why

- **RH6001 (`nameof/// x`).** The documentation-comment phase moves that comment, so formatter output cannot reach it (auditor). The decision names only RH6021 and RH6005.
- **RH6016.** It decides on the line table and holds (auditor). No test is needed.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~ZeroWidthTokenLineStartTests|FullyQualifiedName~ReihitsuFormatterHelpersTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH6021|FullyQualifiedName~RH6005"
scripts/test.sh --project core --filter "FullyQualifiedName~SyntaxTokenPositionUtilitiesTests"
```

Results on f9d02c6:

| Run | Failed | Passed | Failures |
|---|---|---|---|
| formatter filter | 7 | 49 | the seven join rows |
| analyzer filter | 9 | 33 | the nine red RH6021 and RH6005 rows |
| core filter | 0 | 8 | — |

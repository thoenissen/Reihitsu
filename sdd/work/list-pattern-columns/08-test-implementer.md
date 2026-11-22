Role: test implementer
Commit: bea3bb6
Verdict: tests written

Follow-up round for lead decision 7: five existing tests asserted `main` output. Maintainer answers 6A (#1647) and 8A (#1929) replace that output. I took each expected value from the accepted spec text, not from the implementer's output. Then I ran the whole formatter project on the implementer's head. Result: 7174 passed, 0 failed.

## Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `Regression/Indentation/CollectionExpressionClosingBracketCommentTests.CommentBeforeClosingBracketOfListPatternIsIndented` (renamed from `...StaysInBracketColumn`) | FMT-INDENT-006 | `AssertRuleResult(input, expected)` | `// last` before a list pattern's `]` moves from the `]` column (17) to the elements' column (21). The blank line above it stays. |
| `Regression/BlankLines/MultiLineGluedCommentBlankLineTests.InsertsNoBlankLineAboveMultiLineCommentGluedToListPatternBracket` | FMT-BLANK-018, FMT-INDENT-006 | `AssertRuleResult(input, expected)` | Previously this asserted no change. The first line, `/* y`, now goes to the elements' column (24). The last line, `*/ ];`, stays at 20 under FMT-INDENT-007. This matches the collection-expression twin in the same file. |
| `Regression/BlankLines/MultiLineGluedCommentBlankLineTests.RemovesAuthorBlankLineAboveMultiLineCommentGluedToListPatternBracket` | FMT-BLANK-018, FMT-INDENT-006 | `AssertRuleResult(input, expected)` | The blank line is still removed. `/* y` goes to 24 and `*/ ];` stays at 20. |
| `Unit/ReihitsuFormatterNodeColumnTests.CaseLabelNotPatternTargetJoinsTheBraceOfItsPropertyPatternOntoNot` | FMT-PIPE-024, FMT-BREAK-041, FMT-INDENT-026 | `AssertFormatsTarget` + `AssertRuleResult` | For `case not {`, `A: 1` goes to column 21, one level right of `not` (17), and `}` goes to column 17, under `not`. Node level and document level give the same output. This matches the `case not {` member of the FMT-INDENT-026 example. |
| `Unit/ReihitsuFormatterNodeColumnTests.CaseLabelNotPatternTargetKeepsTheBraceWrittenBehindNot` | FMT-PIPE-024, FMT-BREAK-022, FMT-INDENT-026 | `AssertFormatsTarget` + `AssertRuleResult` | Same columns as the test above. |

Renamed: only the first test, because its old name stated the old rule ("StaysInBracketColumn"). The other four names state no column. I rewrote their summaries to state the new layout. Each test now carries the requirement tag that decides its new column.

## Not covered, and why
No new tests. The run's tests already cover the new behavior: `PatternClosingDelimiterCommentTests` and the FMT-INDENT-026 tests from 06-test-implementer.md. This round only corrects stale assertions.

## Commands
```shell
scripts/test.sh --project formatter
```

## Follow-up candidates
_None._

Role: implementer
Commit: 39fc4b0
Verdict: test disputed

My prompt carried no intended fix, suspected cause, or hoped-for verdict. The issue texts name `PatternAnchor`'s fallback for #1929. I took the fix from FMT-INDENT-026's text and the tests, not from the issue.

All 35 formatter tests and 2 analyzer tests that the test implementer handed over now pass. I changed no test. Five **existing** tests that the test implementer did not update now fail. Each asserts output that the accepted change docs replace. See *Disputed tests or scope*.

## thoenissen/Reihitsu#1633

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/LineBreakListRewriter.cs` | New `VisitBaseList`. It joins the first base type onto the colon's line through the existing `CollapseFirstElementToSameLine`. That helper calls `CollapseTokenToSameLine`, which refuses a gap holding a comment, a directive, or disabled text. Then it moves every comma that starts a line through the shared `CollapseSeparatorsToPreviousElementLine`. The class summary now names the base list. | FMT-BREAK-042, FMT-BREAK-038, FMT-BREAK-001 |

The indentation side needed no change. The later base types already align under the first (FMT-INDENT-016, `BaseTypeListContributor`). A comment-held first base type keeps the continuation indent (FMT-INDENT-034).

## thoenissen/Reihitsu#1634

No production change. The two characterization tests pass on this head, as the spec expects (decision 3).

## thoenissen/Reihitsu#1647

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/Indentation/Contributors/CommentIndentationContributor.cs` | A comment that starts its line in front of the `]` of a list pattern, or the `)` of a positional or parenthesized pattern, is indented one level deeper than the delimiter. A comment in front of a closing brace or a collection expression's `]` already was. The summary and the inline comment state the rule by its cause. | FMT-INDENT-006 |

The parenthesized pattern's content stands four columns right of `(`, and its `)` stands in the `(` column (FMT-INDENT-026). So "+ one level from the delimiter" gives the content column for all three constructs.

## thoenissen/Reihitsu#1685

No production change. The characterization tests pass. `main` already gives this output.

## thoenissen/Reihitsu#1542

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/LineBreakInitializerRewriter.cs` | New `LayOutPositionalClause`, called from `VisitRecursivePattern`. It uses the helpers the list pattern already uses: `CollapseTokenToSameLine` joins the `(` onto the type's last token, `CollapseSeparatorsToPreviousElementLine` moves line-starting commas, and for a multi-line clause `EnsureElementsOnSeparateLines`, `EnsureFirstContentOnNewLine`, `NormalizeGapBeforeOwnedToken` (`)` on its own line), and `CleanupTrailingWhitespaceBeforeToken` do the rest. Without a type, the `(` follows `is`, `not`, or a combinator through the existing FMT-BREAK-041 and FMT-BREAK-017 joins. | FMT-BREAK-043, FMT-BREAK-038, FMT-BREAK-022, FMT-BREAK-001 |
| `src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/BinaryOperatorLineBreakRewriter.cs` | `IsMultiLineDelimitedPattern` now counts a recursive pattern whose positional clause spans several lines, as well as one whose property clause does. | FMT-BREAK-017 |
| `src/Reihitsu.Formatter/Pipeline/Indentation/Contributors/RecursivePatternContributor.cs` | New `ContributePositionalClause`. When the clause spans several lines, the subpatterns go one level right of the `(`, and the `)` goes in the `(` column. This is the list pattern's rule (`ListPatternContributor`). | FMT-INDENT-025 |

The positional `)` comment is in the #1647 row (FMT-INDENT-006).

## thoenissen/Reihitsu#1929

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/Indentation/Contributors/PatternAnchor.cs` | In the fallback, the anchor is now the first token of the outermost pattern the `and`/`or`/`not` wrappers lead to (`current`), not the pattern's own first token. The summary states the new reading. | FMT-INDENT-026 |

This one line covers all the members in the tests: the switch-expression arm, the list-pattern element, the slice, the parenthesized pattern behind `not` (through `ParenthesizedPatternContributor`), the `case` labels, and the RH5206 fix. For a pattern with no wrapper, `current` is the pattern itself, so its output is unchanged.

## Policy owners
- Comma that starts a line: `rg -n "CollapseSeparatorsToPreviousElementLine" src` finds one owner, `LineBreakTriviaUtilities`. It is reused by the base list and the positional clause.
- First element onto the opening token's line: `rg -n "CollapseFirstElementToSameLine|CollapseTokenToSameLine" src` finds `LineBreakTriviaUtilities.CollapseTokenToSameLine` as the one owner. `LineBreakListRewriter.CollapseFirstElementToSameLine` wraps it and is reused.
- Multi-line delimited pattern next to a combinator: `rg -n "IsMultiLineDelimitedPattern|LabelContainsMultiLinePattern" src` finds `BinaryOperatorLineBreakRewriter.IsMultiLineDelimitedPattern`, which I extended. It also finds `SwitchCaseBraceRewriter.LabelContainsMultiLinePattern`, a different decision (FMT-SWITCH). It already counts every `RecursivePatternSyntax` by its whole span.
- Closing-delimiter comment column: `rg -n "CloseBracketToken\) && token.Parent is|CollectionExpressionSyntax or ListPatternSyntax" src` finds `CommentIndentationContributor` (FMT-INDENT-006, extended). It also finds `BraceBlankLinePolicy.IsClosingTokenOfPatternOrCollectionExpression`, which is FMT-BLANK-019's own list. The spec did not change that list, so I left it alone (see *Follow-up candidates*).
- Pattern anchor: `rg -n "PatternAnchor.GetColumn" src` finds the one owner, `PatternAnchor`, used by `RecursivePatternContributor` and `ParenthesizedPatternContributor`.
- Positional clause layout: `rg -n "PositionalPatternClause" src` finds only the new code and `PatternLineStartUtilities.StartsOwnLine`. That method excludes a property pattern with a positional clause from the brace-on-own-line rule, which is unchanged.

## Production diff size
`git diff --shortstat origin/main...HEAD -- src`: 6 files changed, 131 insertions(+), 17 deletions(-). No new type, no new public member, no new dependency.

## Deleted special cases
_N/A._ No shape is declared unsupported in this run.

## Tests run
- `scripts/test.sh --configuration Debug --project formatter --filter "FullyQualifiedName~PositionalPatternLayoutTests|FullyQualifiedName~PatternAnchorBehindWrapperTests|FullyQualifiedName~PatternClosingDelimiterCommentTests"`: Passed 29, Failed 0.
- `scripts/test.sh --configuration Debug --project formatter --filter "FullyQualifiedName~BaseListColonJoinTests"`: Passed 12, Failed 0.
- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH5412EmptyClassesShouldUseSemicolonDeclarationsAnalyzerTests|FullyQualifiedName~RH5206SwitchExpressionBracesShouldBeAnchoredAnalyzerTests"`: Passed 55, Failed 0.
- `scripts/test.sh --configuration Debug --project formatter`: Failed 5, Passed 7169. The 5 failures are the disputed tests below. Every handed-over test passes, including the fixture re-runs the test implementer named (`FirstCommentBehindOpeningTokenTests`, `CommentBeforeBraceBlankLineTests`, the other blank-line fixtures).
- `scripts/test.sh --configuration Debug --project analyzer`: Passed 11937, Failed 0.
- `scripts/test.sh --configuration Debug --project architecture`: Passed 198, Failed 0.
- `scripts/format.sh --check src tests`: 0 of 2051 files need formatting. The repository's own source is stable under the new layout.

## Disputed tests or scope
Five existing tests assert the output on `main` that the accepted change docs replace. The test implementer's searches did not catch them. All five are stale. The new output is exactly what the spec text gives. They go back to the test implementer:

1. `tests/Reihitsu.Formatter.Test/Regression/Indentation/CollectionExpressionClosingBracketCommentTests.cs`, `CommentBeforeClosingBracketOfListPatternStaysInBracketColumn`.
   - Asserts: `// last` in front of a list pattern's `]` stays in the `]` column. Its summary says "which FMT-INDENT-006 does not name".
   - Spec: the new FMT-INDENT-006 names the list pattern, and pattern-closing-delimiter-comment.md (6A) moves the comment to the elements' column.
   - Output now: `// last` at column 21 instead of 17.
   - Suggested repair: assert the elements' column, or delete the test as a duplicate of `PatternClosingDelimiterCommentTests`.
2. `tests/Reihitsu.Formatter.Test/Regression/BlankLines/MultiLineGluedCommentBlankLineTests.cs`, `InsertsNoBlankLineAboveMultiLineCommentGluedToListPatternBracket`.
3. Same file, `RemovesAuthorBlankLineAboveMultiLineCommentGluedToListPatternBracket`.
   - Both assert: the first line of `/* y⏎*/ ];` stays in the `]` column (20).
   - Spec: under FMT-INDENT-006 it goes to the elements' column (24). The output is now `/* y` at 24 and `*/ ];` at 20. That is exactly what the collection-expression twins in the same file assert (`...GluedToCollectionExpressionBracket`, both tagged FMT-INDENT-006).
   - Suggested repair: expect the twin's shape.
4. `tests/Reihitsu.Formatter.Test/Unit/ReihitsuFormatterNodeColumnTests.cs`, `CaseLabelNotPatternTargetJoinsTheBraceOfItsPropertyPatternOntoNot`.
5. Same file, `CaseLabelNotPatternTargetKeepsTheBraceWrittenBehindNot`.
   - Both assert: for `case not {`, `A: 1` at column 25 and `}` at column 21 (the brace's own column), at node level and document level.
   - Spec: under FMT-INDENT-026 as accepted with 8A, the content goes to column 21 and `}` to column 17, under the `not`. This is the same shape as the `case not {` member of the FMT-INDENT-026 spec example.
   - Output now: those columns, identical at node level and document level, so FMT-PIPE-024 parity holds.

No scope growth. No test assertion was changed.

## Architecture findings
_None._ No test needed a prediction or a copy of another component's behavior. Each change extends the rewriter or contributor that already owns the construct.

## Follow-up candidates
- **FMT-BLANK-019 does not name the positional pattern's `)`.** `BraceBlankLinePolicy.IsClosingTokenOfPatternOrCollectionExpression` covers the `]` of a collection expression or list pattern and the `)` of a parenthesized pattern. It does not cover the `)` of a positional pattern clause, which FMT-INDENT-025 and FMT-INDENT-006 now lay out like a list pattern's `]`. So the blank-line rule in front of a closing delimiter may now differ between a list pattern and a positional pattern. The spec did not change FMT-BLANK-019, and no test covers the case. This is a spec question for a later change, not this run's.

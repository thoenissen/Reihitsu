Role: test implementer
Commit: f651f95
Verdict: tests written

My prompt carried no intended fix, suspected cause, or hoped-for verdict. Expected values come from the spec examples (FMT-BLANK-019), the change doc's examples, and, for the contexts without a spec example, from the columns FMT-BREAK-022/FMT-INDENT-025 give (subpatterns four columns right of the `(`, `)` under the `(`), which this change does not touch. Every new failing test was checked to differ from today's output only by the blank line(s) in front of the `)` (or above the glued comment); several blank lines collapse to one today.

## Tests

Formatter: `tests/Reihitsu.Formatter.Test/Regression/BlankLines/PositionalPatternClosingParenthesisBlankLineTests.cs` (new). `AssertRuleResult` runs LF and CRLF, a second pass, and the phase stability check; `AssertBlankLinesAbove` runs LF and CRLF and the second-pass check.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `SpecExampleRemovesBlankLineBeforeClosingParenthesisOfPositionalPatternClause` (typed, untyped, switch arm) | FMT-BLANK-019 | `AssertRuleResult` | fail |
| `SpecExampleRemovesBlankLineInConstructWithNoElement` (`P(⏎⏎)` and `[⏎⏎]`) | FMT-BLANK-019 | `AssertRuleResult` | fail (positional part) |
| `SpecExampleKeepsBlankLineBeforeClosingParenthesisOfArgumentList` | FMT-BLANK-019 | `AssertRuleResult` (unchanged) | pass |
| `RemovesSeveralBlankLinesBeforeClosingParenthesisOfPositionalPatternClause` | FMT-BLANK-019 | `AssertRuleResult` | fail |
| `RemovesBlankLineBeforeClosingParenthesisOfPositionalPatternClauseInCaseLabel` | FMT-BLANK-019; FC-027 | `AssertRuleResult` | fail |
| `RemovesBlankLinesBeforeClosingParenthesesOfNestedPositionalPatternClauses` (inner and outer) | FMT-BLANK-019; FC-027 | `AssertRuleResult` | fail |
| `RemovesBlankLineBeforeClosingParenthesisOfPositionalPatternClauseBehindNot` | FMT-BLANK-019; FC-027 | `AssertRuleResult` | fail |
| `RemovesBlankLineBeforeClosingParenthesisOfPositionalPatternClauseFollowedByPropertyClause` | FMT-BLANK-019; FC-027 | `AssertRuleResult` | fail |
| `RemovesBlankLineBeforeClosingParenthesisOfPositionalPatternClauseFollowedByDesignation` | FMT-BLANK-019; FC-027 | `AssertRuleResult` | fail |
| `RemovesBlankLineBeforeClosingParenthesisOfPositionalPatternClauseWithOneSubpattern` | FMT-BLANK-019 | `AssertRuleResult` | fail |
| `RemovesBlankLineBeforeClosingParenthesisOfPositionalPatternClauseBehindEndOfLineComment` | FMT-BLANK-019; FC-021 | `AssertRuleResult` | fail |
| `KeepsBlankLineBehindOpeningParenthesisOfPositionalPatternClause` | FMT-BLANK-019; FC-003 | `AssertRuleResult` | pass |
| `RemovesBlankLineBetweenOwnLineCommentAndClosingParenthesisOfPositionalPatternClause` (change doc 2) | FMT-BLANK-020; FC-019 | `AssertRuleResult` | fail |
| `RemovesBlankLineAboveCommentGluedToClosingParenthesisOfPositionalPatternClause` (change doc 2) | FMT-BLANK-018; FC-021 | `AssertRuleResult` | fail |
| `RemovesBlankLineAboveMultiLineCommentGluedToClosingParenthesisOfPositionalPatternClause` | FMT-BLANK-018; FC-021 | `AssertBlankLinesAbove` | fail |
| `RemovesOnlyBlankLineBelowDirectiveBeforeClosingParenthesisOfPositionalPatternClause` (change doc 2) | FMT-BLANK-009; FC-019, FC-020 | `AssertRuleResult` | fail |
| `KeepsAndInsertsBlankLineBelowEndRegionBeforeClosingParenthesisOfPositionalPatternClause` | FMT-BLANK-019; FC-012, FC-020 | `AssertBlankLinesAbove` | pass |
| `KeepsBlankLineBeforeClosingParenthesisOfParenthesizedVariableDesignation` (`o is var (⏎x,⏎y⏎⏎)`) | FMT-BLANK-019; FC-003 | `AssertBlankLinesAbove` | pass |

Analyzer: `tests/Reihitsu.Analyzer.Test/Formatting/RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzerTests.cs`

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `VerifyFixRemovesBlankLineBeforeClosingParenthesisOfPositionalPatternClause` (LF, CRLF; change doc 3) | FMT-BLANK-019; FC-028 | `Verify` (`BatchCodeFixTestsBase`) | fail, only the blank line in front of `)` differs |

Run on bbe6546 + tests: formatter class 14 failed, 4 passed; RH3202 class 2 failed (the new DataRows), 104 passed.

## Scenarios (bug fix)

_N/A_ — behavior change.

## Not covered, and why

- Existing tests asserting a changed output: none. `rg -U -l "\n[ \t]*\n[ \t]*\)" tests` (all files, `.cs` and others) finds only `ExpressionBodiedAccessorTests.cs` (argument list), `CommentBeforeBraceBlankLineTests.cs` and `ClosingBracketBlankLineTests.cs` (parenthesized patterns), none with a positional clause; no test was changed.
- Individual columns in the `#region` and `var (x, y)` tests are not asserted: no requirement lays out a region directive inside a pattern or a parenthesized variable designation; the multi-line glued comment's continuation column likewise (same choice as the existing parenthesized-pattern tests).
- Separate CRLF / idempotency tests: covered by `AssertRuleResult`/`AssertBlankLinesAbove`, which run both endings, a second pass, and the phase stability check (FC-010, FC-017).
- Detached `FormatNode` (FC-028 other arm): the decision reads only the token's kind and its parent node (spec: "the kind of the token and the node that owns it decide"), with no entry-point input; the document-scoped node path is covered by the RH3202 test. Not a static proof of a single call site, so a lead may ask for one more test.
- Fix All across several diagnostics: the class's `GetFixAllScenario` already covers Fix All for RH3202; the formatter decision is per token and has no cross-instance state.
- FC-013: RH5020 reporting the glued `/* last */ )` is intended (FMT-BLANK-018 Why, skeptic 02); no test.
- FC-015/016 (shared predicate, decision 3): structural, not observable beyond the tests above; the own-line-comment test pins the indentation and blank-line phases agreeing on the positional `)`.
- Unsupported-shape classes (FC-038 to FC-043): no shape is declared.
- Naming fix, Renamer: not applicable.
- Analyzer parity (`FormatterTestsBase<TAnalyzer>`): no analyzer rule reports a blank line before a pattern's closing delimiter.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Regression.BlankLines.PositionalPatternClosingParenthesisBlankLineTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Formatting.RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzerTests.VerifyFixRemovesBlankLineBeforeClosingParenthesisOfPositionalPatternClause"
```

## Follow-up candidates

_None._

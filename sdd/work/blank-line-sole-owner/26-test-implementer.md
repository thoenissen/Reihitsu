Role: test implementer
Commit: see the commit that adds this file (subject "Test implementer: repair round 3 tests for the comment-position classification, entries 20-22 [skip ci]")
Verdict: tests written

Repair round 3, the last. The tests cover `00-lead.md` rows 29–35, the failing members of auditor findings 6–8 (`24-auditor.md`), and the *Examples for the tests* of `25-spec-author.md`. The prompt named the scope. It carried no intended fix, no suspected cause, and no hoped-for verdict. Expected values come from the FMT-BLANK-018 examples (the new last example included), the FMT-BLANK-020 sentence on `// c⏎⏎/* x⏎ */ }`, and change-doc entries 3, 4, 8, 9, and 20–22. Where the spec says the output is `main`'s, I also checked the expected value against `main`'s production code (see *Evidence against `main`*). No production code is touched. No existing test changed. No assertion is weakened.

All tests use `AssertRuleResult` or `AssertBlankLinesAbove`, so each one runs under LF and CRLF, checks that a second pass is stable, and checks that no phase alone changes the output. `AssertBlankLinesAbove` is used only where no requirement decides the column of the comment's first line. These are the complex element `{`, the assigned collection initializer `}`, and the parenthesized pattern `)`, where the indentation of a glued comment line may legitimately move.

## Tests

New files:
- `src/Reihitsu.Formatter.Test/Regression/BlankLines/MultiLineGluedCommentBlankLineTests.cs` (42 tests)
- `src/Reihitsu.Formatter.Test/Regression/BlankLines/BlankLineBehindDirectiveBeforeTokenTests.cs` (5 tests)

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `ClassifiesCommentsEndingOnTokenLineAsGluedAndOthersAsOwnLine` (FMT-BLANK-018 last example, verbatim) | FMT-BLANK-011, FMT-BLANK-018 | `AssertRuleResult` | fails: blank line inserted above `/* b`, `/* d`, `/* e */ /* f` |
| `InsertsNoBlankLineAboveMultiLineCommentGluedToMethodBodyBrace` (#6) | FMT-BLANK-018 | `AssertRuleResult` | fails: blank inserted above `/* x` |
| `RemovesAuthorBlankLineAboveMultiLineCommentGluedToMethodBodyBrace` (#6) | FMT-BLANK-018 | `AssertRuleResult` | fails: blank kept |
| `InsertsNoBlankLineAboveMultiLineCommentGluedToTypeBrace` (#6) | FMT-BLANK-018 | `AssertRuleResult` | fails: blank inserted |
| `RemovesAuthorBlankLineAboveMultiLineCommentGluedToTypeBrace` (#6) | FMT-BLANK-018 | `AssertRuleResult` | fails: blank kept |
| `InsertsNoBlankLineAboveMultiLineCommentGluedToClosingBrace` (#6) | FMT-BLANK-018 | `AssertRuleResult` | fails: blank inserted |
| `RemovesAuthorBlankLineAboveMultiLineCommentGluedToClosingBrace` (#6) | FMT-BLANK-018 | `AssertRuleResult` | fails: blank kept |
| `InsertsNoBlankLineAboveCommentInFrontOfMultiLineCommentGluedToClosingBrace` (#6, `/* a */ /* x⏎ */ }`) | FMT-BLANK-018 | `AssertRuleResult` | fails: blank inserted |
| `RemovesAuthorBlankLineAboveCommentInFrontOfMultiLineCommentGluedToClosingBrace` (#6) | FMT-BLANK-018 | `AssertRuleResult` | fails: blank kept |
| `InsertsNoBlankLineAboveMultiLineCommentsGluedToObjectInitializerBraces` (#6, `{` and `}`) | FMT-BLANK-018 | `AssertRuleResult` | fails: blanks inserted above both |
| `RemovesAuthorBlankLinesAboveMultiLineCommentsGluedToObjectInitializerBraces` (#6) | FMT-BLANK-018 | `AssertRuleResult` | fails: blanks kept |
| `InsertsNoBlankLineAboveMultiLineCommentGluedToAssignedCollectionInitializerBrace` (#6, `L =⏎/* x⏎ */ {`) | FMT-BLANK-018 | `AssertRuleResult` | fails: blank inserted |
| `RemovesAuthorBlankLineAboveMultiLineCommentGluedToAssignedCollectionInitializerBrace` (#6) | FMT-BLANK-018 | `AssertRuleResult` | fails: blank kept |
| `InsertsNoBlankLineAboveMultiLineCommentGluedToCollectionExpressionBracket` (#6) | FMT-BLANK-018 | `AssertRuleResult` | fails: blank inserted |
| `RemovesAuthorBlankLineAboveMultiLineCommentGluedToCollectionExpressionBracket` (#6) | FMT-BLANK-018 | `AssertRuleResult` | fails: blank kept |
| `InsertsNoBlankLineAboveMultiLineCommentGluedToListPatternBracket` (#6) | FMT-BLANK-018 | `AssertRuleResult` | fails: blank inserted |
| `RemovesAuthorBlankLineAboveMultiLineCommentGluedToListPatternBracket` (#6) | FMT-BLANK-018 | `AssertRuleResult` | fails: blank kept |
| `InsertsNoBlankLineAboveMultiLineCommentGluedToParenthesizedPatternParenthesis` (#6) | FMT-BLANK-018 | `AssertBlankLinesAbove` | fails: blank above `/* z` |
| `RemovesAuthorBlankLineAboveMultiLineCommentGluedToParenthesizedPatternParenthesis` (#6) | FMT-BLANK-018 | `AssertBlankLinesAbove` | fails: blank above `/* z` |
| `SeparatesOwnLineCommentInFrontOfMultiLineCommentGluedToClosingBrace` (#6 `// c⏎⏎/* x⏎ */ }`; entry 9) | FMT-BLANK-011, FMT-BLANK-018, FMT-BLANK-020 | `AssertRuleResult` | fails: blank between `// c` and `/* x` kept |
| `InsertsNoBlankLineAboveMultiLineCommentsGluedToArrayBraceMemberStatementOrElse` (entry 20 example, input unchanged) | FMT-BLANK-011, FMT-BLANK-018 | `AssertRuleResult` | fails: blanks inserted above `/* g`, `/* m`, `/* s`, `/* k` |
| `DecidesAuthorBlankLinesAboveMultiLineCommentsGluedToArrayBraceMemberStatementOrElse` (entry 20, author's blank lines) | FMT-BLANK-018 | `AssertRuleResult` | fails: blanks above `/* g` and `/* k` kept |
| `InsertsNoBlankLineAboveMultiLineCommentsGluedToCatchFinallyOrDoWhile` (entry 20) | FMT-BLANK-018 | `AssertRuleResult` | fails: blanks inserted |
| `RemovesAuthorBlankLinesAboveMultiLineCommentsGluedToCatchFinallyOrDoWhile` (entry 20) | FMT-BLANK-018 | `AssertRuleResult` | fails: blanks kept |
| `InsertsNoBlankLineAboveMultiLineCommentGluedToComplexElementInitializerBrace` (entry 20) | FMT-BLANK-018 | `AssertBlankLinesAbove` | fails: blank above `/* c` |
| `RemovesAuthorBlankLineAboveMultiLineCommentGluedToComplexElementInitializerBrace` (entry 20) | FMT-BLANK-018 | `AssertBlankLinesAbove` | fails: blank above `/* c` |
| `InsertsNoBlankLineAboveMultiLineCommentGluedToClosingBraceOfAssignedCollectionInitializer` (entry 20) | FMT-BLANK-018 | `AssertBlankLinesAbove` | fails: blank above `/* l` |
| `RemovesAuthorBlankLineAboveMultiLineCommentGluedToClosingBraceOfAssignedCollectionInitializer` (entry 20) | FMT-BLANK-018 | `AssertBlankLinesAbove` | fails: blank above `/* l` |
| `InsertsNoBlankLineAboveMultiLineCommentGluedThroughDocumentationComment` (entry 20, `/* x⏎ */ /** Doc */ {`) | FMT-BLANK-018 | `AssertRuleResult` | fails: blank inserted |
| `RemovesAuthorBlankLineAboveMultiLineCommentGluedThroughDocumentationComment` (entry 20) | FMT-BLANK-018 | `AssertRuleResult` | fails: blank kept |
| `InsertsNoBlankLineAboveMultiLineCommentsGluedToArgumentListParenthesisOrFirstChainLink` (entry 20) | FMT-BLANK-011, FMT-BLANK-018 | `AssertRuleResult` | fails: blanks inserted above `/* e` and `/* x` |
| `KeepsAuthorBlankLinesAboveMultiLineCommentsGluedToArgumentListParenthesisOrFirstChainLink` (entry 20) | FMT-BLANK-018 | `AssertRuleResult` | passes |
| `InsertsStatementBlankLineAboveMultiLineCommentGluedToReturnOrCaseLabel` (spec author example 4) | FMT-BLANK-018 | `AssertRuleResult` | passes |
| `InsertsBlankLineAfterClosingBraceAboveMultiLineCommentGluedToStatementOrMember` (example 4) | FMT-BLANK-018 | `AssertRuleResult` | passes |
| `DecidesBlankLinesAboveMultiLineCommentGluedToBlockStatementBrace` (entries 3 and 4: after `}` inserted, after `A();` none inserted, author's kept) | FMT-BLANK-017, FMT-BLANK-018 | `AssertRuleResult` | fails: blank inserted above `/* y` after `A();` |
| `InsertsBlankLineAfterBreakAboveMultiLineCommentGluedToBlockStatementBrace` (entry 8) | FMT-BLANK-017, FMT-BLANK-018 | `AssertRuleResult` | passes |
| `InsertsBlankLineAboveMultiLineDocumentationCommentStartingBraceLine` (not glued: `/** d⏎ */ {`) | FMT-BLANK-011, FMT-BLANK-018 | `AssertRuleResult` | passes |
| `KeepsBlankLineAboveMultiLineDocumentationCommentStartingBraceLine` (not glued) | FMT-BLANK-011, FMT-BLANK-018 | `AssertRuleResult` | passes |
| `SeparatesMultiLineCommentEndingInLineCommentAndRemovesBlankLineBeforeClosingBrace` (not glued: `/* x⏎ */ // c⏎⏎}`) | FMT-BLANK-011, FMT-BLANK-020 | `AssertRuleResult` | passes |
| `InsertsBlankLineAboveMultiLineCommentInFrontOfChainLinkMovedOffItsLine` (change-doc entry 20 chain example, `main`'s output) | FMT-BLANK-011, FMT-BLANK-018 | `AssertRuleResult` | passes |
| `KeepsBlankLineAboveMultiLineCommentInFrontOfChainLinkMovedOffItsLine` (same, with the author's blank line) | FMT-BLANK-011, FMT-BLANK-018 | `AssertRuleResult` | passes |
| `InsertsBlankLineAboveMultiLineCommentInFrontOfPrefixChainLinkMovedOffItsLine` (prefix link `.B`) | FMT-BLANK-011, FMT-BLANK-018 | `AssertRuleResult` | passes |
| `KeepsBlankLineBetweenDirectiveAndWhileStatement` (entry 21, #7) | FMT-BLANK-008, FMT-BLANK-009 | `AssertRuleResult` | passes |
| `RemovesBlankLineBetweenDirectiveAndDoStatementWhile` (entry 21 counterpart) | FMT-BLANK-008, FMT-BLANK-009 | `AssertRuleResult` | passes |
| `KeepsBlankLineAboveDirectiveInFrontOfAssignedCollectionInitializerBrace` (entry 22, #8) | FMT-BLANK-009, FMT-BREAK-030 | `AssertRuleResult` | passes |
| `RemovesBlankLineBetweenDirectiveAndAssignedCollectionInitializerBrace` (entry 22) | FMT-BLANK-009, FMT-BREAK-030 | `AssertRuleResult` | passes |
| `InsertsNoBlankLineNextToDirectiveInFrontOfAssignedCollectionInitializerBrace` (entry 22) | FMT-BLANK-009, FMT-BREAK-030 | `AssertRuleResult` | passes |

Each new test that fails here fails on the issue's own difference, which is a blank line above the multi-line comment, inserted or kept. I checked this with a line diff of expected against actual for all 32 failures. No failure involves a column, a moved token, or a phase-stability-only finding.

## Evidence against `main`

I copied both new test classes into a scratch worktree of `main` (741157c), together with the branch's `FormatterTestsBase` (for `AssertBlankLinesAbove`). `main`'s own phase-reversal list was kept. I ran them there.

- **18 tests for the members of #6 that should restore `main`'s output.** These are the method-body, type, and closing braces (plain and with `/* a */` in front), the object-initializer braces, the assigned-collection-initializer `{`, the collection-expression `]`, the list-pattern `]`, and the parenthesized-pattern `)`, each with and without the author's blank line. On `main`, every one of them produces exactly the expected full-pipeline output, under LF and CRLF, on both passes. They fail on `main` only in the phase-stability check: on `main`, the blank-line phase alone re-inserts the line, and the line-break phase removes it again. That is the second owner this run removes. So the expected values are `main`'s output.
- **Tests whose expected output on `main` is the branch's current output.** These are the entry-20 tests, the FMT-BLANK-018 last example, `// c⏎⏎/* x⏎ */ }` (entry 9), the block-statement tests (entries 3, 4, and 8), and the entry-21 and entry-22 tests. They fail on `main` with an output difference. That is intended: each is a change against `main` decided by the guidance, by an answer, or by an unchanged requirement.
- **The chain-link tests and the not-glued tests** pass on `main` and on the branch.

## Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| `void M(int a)⏎/* x⏎ */ {` (#6) | LF, CRLF | formatter, full pipeline, CLI built from HEAD 95cf517 | reproduced | observed: a blank line is inserted above `/* x`; expected (`main`, FMT-BLANK-018): none |
| `a++;⏎#pragma …⏎⏎while (a > 0)` (#7) | LF, CRLF | formatter | holds on HEAD | the blank line is kept, as entry 21 states |
| `L =⏎⏎#pragma …⏎{` (#8) | LF, CRLF | formatter | holds on HEAD | the blank line is kept, as entry 22 states |

## Not covered, and why

- **Analyzer parity (FC-013) for the multi-line glued comment.** RH5020 keeps reporting glued comments, as the change doc decides. Whether RH5024/RH5025 report the restored outputs did not change: the outputs equal `main`'s. I added no analyzer test. The full analyzer suite (6351 tests) passes unchanged.
- **Comment and directive in the gap (failure-class requirement for token gaps).** The directive side is entries 21 and 22 (new tests). The existing `KeepsBlankLinesOfGapWithDirectiveInFrontOfGluedComment` and the answer-5 and answer-6 tests cover a directive in front of a glued comment. The classification does not read directives.
- **Columns of glued comment lines** before a complex element `{`, an assigned collection initializer `}`, and a parenthesized pattern `)`. No requirement decides them. The tests assert only the blank lines, plus stability.
- **End-of-file comment.** `TrailingCommentWithNoBlankLineGainsOneAtEndOfFile` is unchanged and passes. The FMT-BLANK-018 example test also covers `// trailing`.
- **The existing `ChainLinkBehindMultiLineCommentTests`** stay untagged. Instead, three new chain-link tests carry FMT-BLANK-018 and FMT-BLANK-011, as `25-spec-author.md` example 6 allows.
- **Code-fix convergence, Fix All, and Renamer:** no code fix changes in this round.
- **Existing tests named by the spec author** (`CommentLedFirstTokenAnchorTests`, `CommentLedLineAlignmentTests`, `BracedSwitchSectionCommentLayoutTests`, RH5303/RH5304 formatter tests): they pass on HEAD. The implementer's change has to keep them green. Their shapes are first-in-construct, so neither reading inserts a line.

## Full suites on HEAD 95cf517 plus the new tests

- formatter: 3711 total, 32 failed. The failing set is exactly the 32 new tests listed above as failing (compared by name). No existing test fails.
- analyzer: 6351 passed, 0 failed.
- architecture: 92 passed, 0 failed (requirement tags and naming conventions).

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~MultiLineGluedCommentBlankLineTests|FullyQualifiedName~BlankLineBehindDirectiveBeforeTokenTests"
scripts/test.sh --project formatter
scripts/test.sh --project analyzer
scripts/test.sh --project architecture
```

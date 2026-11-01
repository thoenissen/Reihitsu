Role: test implementer
Commit: d633172
Verdict: tests written

Lane: behavior change, issue thoenissen/Reihitsu#1496. The prompt carried no intended fix, suspected cause, or hoped-for verdict to ignore. Expected values come from the spec examples and the change doc's visible output changes; for requirements the change doc calls a clarification of today's output (FMT-BLANK-019, FMT-BLANK-020, the unchanged rows of FMT-BLANK-018) the expected value is today's output, confirmed with `scripts/format.sh --force` at d633172 and checked against the requirement's text. Nothing was taken from a patched copy of the code.

## Tests

"Fails today" means on d633172; every failure was read and fails on the requirement's own difference (a missing or extra blank line, or the phase-level check catching a blank-line reversal), never on unrelated layout or a compile error. Formatter helpers run LF and CRLF, a second pass, and the phase-level stability check (every phase alone on the final output) unless noted.

### Formatter — `src/Reihitsu.Formatter.Test/Regression/BlankLines/BlockStatementBlankLineTests.cs` (new)

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| InsertsBlankLineAboveBlockStatementAfterClosingBrace | FMT-BLANK-017 | `AssertRuleResult` | fails: blank line missing (issue's first input) |
| InsertsBlankLineBetweenTwoBlockStatements | FMT-BLANK-017 | `AssertRuleResult` | fails: blank line missing |
| InsertsBlankLineAboveNestedBlockStatementAfterClosingBrace | FMT-BLANK-017 (FC-027) | `AssertRuleResult` | fails: blank line missing |
| KeepsAuthorBlankLineAboveBlockStatementAfterOtherStatement | FMT-BLANK-017 | `AssertRuleResult` | fails: author's blank line removed |
| InsertsNoBlankLineAboveBlockStatementAfterOtherStatement | FMT-BLANK-017 | `AssertRuleResult` | passes |
| CollapsesAuthorBlankLinesAboveBlockStatementAfterOtherStatementToOne | FMT-BLANK-017 | `AssertRuleResult` | fails: all blank lines removed |
| InsertsBlankLineAboveBlockStatementAfterBreak | FMT-BLANK-017 | `AssertRuleResult` | fails: blank line missing |
| PlacesBlankLineBelowDirectiveAndKeepsAuthorBlankLineBelowDirective | FMT-BLANK-017 (FC-020) | `AssertRuleResult` | fails: blank lines below `#if` and `#pragma` missing |
| SeparatesBlockStatementsThatFollowAStatementInsideSwitchSection | FMT-BLANK-017 | `AssertRuleResult` | fails: blank lines missing |
| RemovesBlankLineAboveFirstStatementOfSwitchSectionAndIfBody | FMT-BLANK-017 | `AssertRuleResult` | passes |
| RemovesBlankLineAboveBlockStatementThatIsFirstStatementOfBlock | FMT-BLANK-017 | `AssertRuleResult` | passes |
| RemovesBlankLineAboveBlockOfLabeledStatement | FMT-BLANK-017 | `AssertRuleResult` | passes |
| RemovesBlankLineAboveTopLevelBlock | FMT-BLANK-017 | `AssertRuleResult` | passes |
| KeepsAuthorBlankLineBetweenCommentAndBlockStatementAfterOtherStatement | FMT-BLANK-017, FMT-BLANK-020 | `AssertRuleResult` | fails: blank line below `// c` removed |
| KeepsAuthorBlankLineBetweenCommentAndBlockStatementAfterClosingBrace | FMT-BLANK-017 | `AssertRuleResult` | fails: blank line below `// c` removed |
| KeepsAuthorBlankLineBetweenCommentAndBlockStatementAfterBreak | FMT-BLANK-017 | `AssertRuleResult` | fails: blank line below `// c` removed |
| InsertsNoBlankLineBetweenCommentAndBlockStatementAfterClosingBrace | FMT-BLANK-017 | `AssertRuleResult` | passes |
| InsertsBlankLineAboveCommentGluedToBlockStatementAfterBreak | FMT-BLANK-017 | `AssertRuleResult` | fails: blank line missing |

### Formatter — `.../Regression/BlankLines/GluedCommentBlankLineTests.cs` (new)

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RemovesAuthorBlankLineAboveCommentGluedToInitializerBrace | FMT-BLANK-018 | `AssertRuleResult` | fails: output right, phase check reports `BlankLinePhase` alone inserting the line (reversal, FC-012) |
| InsertsNoBlankLineAboveCommentGluedToInitializerBrace | FMT-BLANK-018 | `AssertRuleResult` | fails: same reversal (issue's second input; Done-when "neither phase changes it alone") |
| KeepsCommentsGluedToBracesWithoutBlankLine | FMT-BLANK-018 | `AssertRuleResult` | fails: same reversal |
| SeparatesCommentGluedToArrayInitializerBraceAndArgumentListParenthesis | FMT-BLANK-018 | `AssertRuleResult` | passes |
| SeparatesCommentGluedToComplexElementInitializerBrace | FMT-BLANK-018 | `AssertRuleResult` | passes |
| SeparatesCommentsGluedToExtensionBlockBraces | FMT-BLANK-018 | `AssertRuleResult` | passes |
| SeparatesCommentGluedToInterpolationClosingBrace | FMT-BLANK-018 | `AssertBlankLinesAbove` | passes |
| RemovesAuthorBlankLineAboveCommentGluedToClosingBrace | FMT-BLANK-018 | `AssertRuleResult` | fails: reversal reported by the phase check |
| RemovesAuthorBlankLineAboveCommentGluedToClosingBracketOrSwitchExpressionBrace | FMT-BLANK-018 | `AssertRuleResult` | fails: reversal reported by the phase check |
| RemovesAuthorBlankLineAboveCommentGluedToParenthesizedPatternParenthesis | FMT-BLANK-018 | `AssertBlankLinesAbove` | fails: reversal reported by the phase check |
| KeepsBlankLinesOfGapWithDirectiveInFrontOfGluedComment | FMT-BLANK-018 (FC-020) | `AssertRuleResult` | fails: blank line inserted above `#pragma` |
| KeepsBlankLineBelowEndRegionInFrontOfCommentGluedToListPatternBracket | FMT-BLANK-018 | `AssertBlankLinesAbove` | passes |
| KeepsBlankLineBelowEndIfInFrontOfGluedCommentAndInsertsNoneAboveIf | FMT-BLANK-018 | `AssertRuleResult` | fails: blank line inserted above `#if` |
| KeepsAuthorBlankLinesOnBothSidesOfConditionalBlockInFrontOfGluedComment | FMT-BLANK-018 | `AssertRuleResult` | passes |
| InsertsNoBlankLineDirectlyBelowEndIfInFrontOfGluedComment | FMT-BLANK-018 | `AssertRuleResult` | passes |
| KeepsBlankLineOfInitializerGapWithDirectiveAndCommentsInFrontOfGluedComment | FMT-BLANK-018 | `AssertRuleResult` | fails: blank line inserted above `#if` |
| SeparatesCommentGluedToBlockStatementBraceAfterStatement | FMT-BLANK-017, FMT-BLANK-018 | `AssertRuleResult` | fails: blank line above `/* c */ {` missing after `}` |
| InsertsBlankLineAboveOwnLineCommentInFrontOfGluedComment | FMT-BLANK-011, FMT-BLANK-018 | `AssertRuleResult` | fails: no blank line above `// note` |
| KeepsOnlyBlankLineAboveOwnLineCommentInFrontOfGluedComment | FMT-BLANK-011, FMT-BLANK-018 | `AssertRuleResult` | fails: the line above `// note` removed, the one below kept |
| MovesBlankLineAboveOwnLineCommentInFrontOfCommentGluedToOpeningBrace | FMT-BLANK-011, FMT-BLANK-018 | `AssertRuleResult` | fails: same |

### Formatter — `.../Regression/BlankLines/ClosingBracketBlankLineTests.cs` (new)

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RemovesBlankLineBeforeClosingBracketOfCollectionExpressionAndListPattern | FMT-BLANK-019 | `AssertRuleResult` | passes |
| KeepsBlankLineBehindOpeningBracketOfCollectionExpression | FMT-BLANK-019 | `AssertRuleResult` | passes |
| RemovesBlankLineBeforeClosingParenthesisOfParenthesizedPattern | FMT-BLANK-019 | `AssertBlankLinesAbove` | passes |
| KeepsAndInsertsBlankLineBelowEndRegionBeforeClosingBracket | FMT-BLANK-019 (FC-020) | `AssertBlankLinesAbove` | passes |
| KeepsBlankLineBelowEndRegionBeforeClosingParenthesisOfParenthesizedPattern | FMT-BLANK-019 (FC-020) | `AssertBlankLinesAbove` | passes |
| RemovesOnlyBlankLineBelowDirectiveBeforeClosingBracket | FMT-BLANK-009 | `AssertRuleResult` | passes |
| RemovesBlankLineBelowDirectiveBeforeClosingParenthesisOfParenthesizedPattern | FMT-BLANK-009 | `AssertBlankLinesAbove` | passes |
| KeepsBlankLineBetweenDirectiveAndCommentBehindIt | FMT-BLANK-009 | `AssertRuleResult` | passes |

### Formatter — `.../Regression/BlankLines/CommentBeforeBraceBlankLineTests.cs` (new)

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RemovesBlankLineBetweenCommentAndBraceOrBracket | FMT-BLANK-020 | `AssertRuleResult` | passes |
| RemovesBlankLineBetweenCommentAndFirstBlockStatementOfBlock | FMT-BLANK-020 | `AssertRuleResult` | passes |
| RemovesBlankLineBehindTrailingCommentBeforeClosingBrace | FMT-BLANK-020 | `AssertRuleResult` | passes |
| KeepsBlankLineBetweenCommentAndBlockStatementThatFollowsStatement | FMT-BLANK-017, FMT-BLANK-020 | `AssertRuleResult` | fails: blank line below `// c` removed |
| RemovesBlankLineBetweenCommentAndEveryNamedBraceOrBracket | FMT-BLANK-020 (skeptic M1 rows) | `AssertRuleResult` | passes |
| RemovesBlankLineBetweenCommentBehindDirectiveAndBraceOrBracket | FMT-BLANK-020 (FC-020) | `AssertRuleResult` | passes |
| KeepsBlankLineBetweenCommentAndArrayInitializerBrace | FMT-BLANK-020 (FC-003, excluded token) | `AssertRuleResult` | passes |

### Formatter — `src/Reihitsu.Formatter.Test/Unit/LineBreaks/LineBreakPhaseBlankLineTests.cs` (new, line-break phase run alone, LF and CRLF, no second pass)

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| KeepsBlankLinesAboveBlockStatementAndGluedComment | FMT-BREAK-030 (spec example) | `AssertPhaseKeepsInput` | fails: blank line above the block statement removed |
| KeepsBlankLineAboveBlockStatementAfterOtherStatement | FMT-BREAK-030 | `AssertPhaseKeepsInput` | fails: blank line removed |
| KeepsBlankLineBetweenCommentAndClosingBrace | FMT-BREAK-030 | `AssertPhaseKeepsInput` | fails: blank line removed |
| KeepsBlankLineBetweenCommentAndOpeningBrace | FMT-BREAK-030 | `AssertPhaseKeepsInput` | fails: blank line removed |
| KeepsBlankLineBeforeClosingBracketOfCollectionExpression | FMT-BREAK-030 | `AssertPhaseKeepsInput` | fails: blank line removed |
| InsertsNoBlankLineInFrontOfDirectiveAboveGluedComment | FMT-BREAK-030 | `AssertPhaseKeepsInput` | fails: blank line inserted above `#pragma` |
| KeepsBlankLinesAroundDirectiveBeforeClosingBrace | FMT-BREAK-030 | `AssertPhaseKeepsInput` | fails: blank line below `#endif` removed |
| InsertsNoBlankLineWhereItMovesBraceOntoNewLine | FMT-BREAK-030 (FC-003, inserted line break) | phase alone | passes |

### Formatter — changed existing tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `ReihitsuFormatterNodeBlankLineTests.InsertsBlankLineAboveBlockStatementAfterClosingBrace` (was `DoesNotInsert…`, expectation changed) | FMT-PIPE-024, FMT-BLANK-017 | `AssertFormatsTarget` (node-level, LF/CRLF, second pass) | fails: blank line missing |
| `…InsertsBlankLineBelowDirectiveAboveBlockStatementAfterClosingBrace` (was `DoesNotInsert…`, expectation changed) | FMT-PIPE-024, FMT-BLANK-017 | `AssertFormatsTarget` | fails: blank line missing |
| `…InsertsBlankLineAboveBlockStatementAfterBreak` (was `DoesNotInsert…`, expectation changed) | FMT-PIPE-024, FMT-BLANK-017 | `AssertFormatsTarget` | fails: blank line missing |
| `…KeepsAuthorBlankLineAboveBlockStatementAfterOtherStatement` (new) | FMT-PIPE-024, FMT-BLANK-017 | `AssertFormatsTarget` | fails: blank line removed |
| `…InsertsNoBlankLineAboveBlockStatementAfterOtherStatement` (new) | FMT-PIPE-024, FMT-BLANK-017 | `AssertFormatsTarget` | passes |
| `…InsertsBlankLineAboveGluedCommentOfBlockStatementAfterClosingBrace` (new) | FMT-PIPE-024, FMT-BLANK-017 | `AssertFormatsTarget` | fails: blank line missing |
| `CommentLedFirstTokenAnchorTests.CreationInitializerBraceBehindCommentThatLeadsTheLineMovesNoOtherLine` (exception entry removed) | issue Done-when (FC-012) | `AssertRuleResult` | fails: phase check reports the `BlankLinePhase` reversal |
| `CommentLedFirstTokenAnchorTests.NestedInitializerAlignsToItsBraceBehindCommentThatLeadsTheLine` (exception entry removed) | issue Done-when (FC-012) | `AssertRuleResult` | fails: same |
| `StructuralTransformSeparatorFinalOutputTests.MembersWithACommentAtTheArrowReachAStableOutput` (exception entry removed) | issue Done-when (FC-012) | `AssertSecondPassIsStable` | fails: same |

### Analyzer — `src/Reihitsu.Analyzer.Test/Formatting/RH5025OpeningBraceMustNotBePrecededByBlankLineAnalyzerTests.cs`

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| VerifyBlankLineAboveBlockStatementThatFollowsStatementIsNotReported | RH5025-005 (spec example) | `Verify` | fails: 3 diagnostics, 1 expected |
| VerifyBlankLineAboveBlockStatementAfterClosingBraceIsNotReported | RH5025-005 | `Verify` | fails: reported |
| VerifyBlankLineAboveBlockStatementAfterOtherStatementIsNotReported | RH5025-005 | `Verify` | fails: reported |
| VerifyBlankLineBetweenDirectiveAndBlockStatementThatFollowsStatementIsNotReported | RH5025-005 (FC-020) | `Verify` | fails: reported |
| VerifyBlankLineAboveBlockStatementThatFollowsStatementInSwitchSectionIsNotReported | RH5025-005 | `Verify` | fails: reported |
| VerifyBlankLineAboveFirstBlockStatementOfBlockIsReported | RH5025-005 | `Verify` | passes |
| VerifyBlankLineAboveBlockOfLabeledStatementIsReported | RH5025-005 | `Verify` | passes |

### Analyzer ↔ formatter parity — `src/Reihitsu.Analyzer.Test/Formatter/Formatting/`

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH5030…FormatterTests.VerifyFormatterInsertsBlankLineAboveBlockStatementAfterClosingBrace` | FMT-BLANK-017 (FC-013) | `VerifyFormatter(source, fixed, diag)` | fails: formatter output has no blank line |
| `RH5011…FormatterTests.VerifyFormatterInsertsBlankLineAboveBlockStatementAfterBreak` | FMT-BLANK-017 (FC-013) | `VerifyFormatter(source, fixed, diag)` | fails: formatter output has no blank line |
| `RH5025…FormatterTests.VerifyFormatterKeepsBlankLineAboveBlockStatementAfterOtherStatement` | RH5025-005, FMT-BLANK-017 | `VerifyFormatter(source)` | fails: RH5025 reports the source |
| `RH5025…FormatterTests.VerifyFormatterKeepsBlankLineAboveBlockStatementAfterClosingBrace` | RH5025-005, FMT-BLANK-017 | `VerifyFormatter(source)` | fails: RH5025 reports the source (the output RH5030 asks for) |

### Test-helper changes

- `src/Reihitsu.Formatter.Test/Helpers/PhaseReversalExceptionList.cs`: the three `BlankLineNextToBrace` entries are removed (issue Done-when), with the now-unused `using` of the structural namespace.
- `src/Reihitsu.Formatter.Test/Helpers/PhaseReversalCauses.cs`: the cause `BlankLineNextToBrace` is removed; it has no entry left.
- `src/Reihitsu.Formatter.Test/Unit/Pipeline/PhaseStabilityCheckTests.cs`: the five recorder/helper tests that used `BlankLineNextToBrace` as a sample cause now use a local test cause `_blankLineCause` (same phase, assertions unchanged). The three tests that sampled real blank-line reversals as fixed points of the pipeline (`CheckCatchesBlankLinePhaseInsertingBlankLineBeforeBlockAfterClosingBrace`, `…AboveCommentGluedToOpeningBrace`, `…AboveCommentGluedToMemberBodyBrace`) are deleted: each asserts that its source is a final output of the formatter, which the spec makes false (FMT-BLANK-017, FMT-BLANK-018). The check's ability to catch a blank-line reversal stays evidenced by the GluedComment tests above, which fail today on exactly that report; the using-phase and line-break-phase samples remain.
- `src/Reihitsu.Formatter.Test/Unit/LineBreaks/TokenGapNormalizerTests.cs`: the four tests of `NormalizeLeadingGap(token, blankLineCount: …)` are deleted; they assert that the line-break phase's normalizer removes or keeps a requested number of blank lines, which FMT-BREAK-030 and the issue's Done-when ("`TokenGapNormalizer` takes no blank-line count") abolish. Phase-level coverage replaces them (`LineBreakPhaseBlankLineTests`).
- `src/Reihitsu.Formatter.Test/Helpers/FormatterTestsBase.cs`: new `AssertBlankLinesAbove(input, (lineStart, isBlankAbove)…)` — LF and CRLF, asserts only whether the line above each named output line is blank, then runs `AssertSecondPassIsStable` (second pass plus phase check). Used where no requirement decides the columns: parenthesized patterns, region directives inside collection expressions and patterns (spec author's instruction, skeptic's follow-up candidates), and an interpolation hole.

**For the implementer.** The one remaining `TokenGapNormalizerTests.NormalizeGapBeforeTokenInsertsMissingLineBreak` passes `blankLineCount: 0`; when the parameter goes, drop the argument — that is a call-site change, not an assertion change. No other test calls `TokenGapNormalizer`, `HasOwnLinePlacementOwner`, `GapCarriesDirectiveOrDisabledText`, or `RootListPosition.KeepsBlankLineAboveBlockStatement` (`rg` over `src/*Test*`).

## Scenarios (bug fix)

_N/A_ — behavior-change lane.

## Not covered, and why

- **"Six" node-level inputs of the issue.** Three `ReihitsuFormatterNodeBlankLineTests` expectations change (block statement after `}`, after `}` behind `#if`, after `break`); the other block-statement tests of that class (own-line comment after `}`, after `}` behind `#if`, after `break`, after `break` behind `#if`, documentation comments behind a statement) already expect the blank line above the comment or directive and keep their outputs under FMT-BLANK-017. The issue's count of six reversals is therefore three changed expectations plus three tests whose output does not change; I added three new node-level tests (after another statement with and without the author's line, glued comment after `}`).
- **Code-fix path through RH5403 (FC-028, FC-030).** The skeptic named `RH5403…CodeFixProvider` as a caller that formats an outer block statement. `scripts/apply-fix.sh RH5403` on a single-line block nested in a block statement after `}` reports no diagnostic (LF and CRLF), and the fix formats `block.Parent?.FirstAncestorOrSelf<StatementSyntax>()`, which for every reported block body (`if`, `else`, `catch`, switch section, loop) is a non-block statement. I found no input where the RH5403 fix targets a block statement, so the node-level tests (`FormatNodeInDocumentAsync` with a block-statement target) are the coverage of FMT-PIPE-024.
- **RH5025 on a top-level block.** Not written for the analyzer: the analyzer test harness compiles a library, where top-level statements do not compile. The formatter side is covered (`RemovesBlankLineAboveTopLevelBlock`); the RH5025 analyzer has no statement-list logic today, so the test of a labeled block (also not in a statement list) pins the same side of the condition.
- **Property-pattern `{` glued comment** (excluded token): today's output also moves the comment to a column no requirement decides (`/* p */ { Length: 2 }` lands at column 8). The array-initializer and argument-`)` tests and the complex-element test cover the excluded side of FMT-BLANK-018 instead.
- **Chained keywords behind a comment (`// f⏎⏎finally`)** and **array-initializer / extension-block / property-pattern `{` behind a comment**: outside FMT-BLANK-020's token set by decision; one test pins the array-initializer case for FC-003. The rest are the follow-up candidates in `02-skeptic.md` and `03-spec-author.md`.
- **Fix All / code-fix convergence (FC-007, FC-008, FC-009).** RH5025's code fix is unchanged; RH5025-005 only removes diagnostics, and the existing `GetFixAllScenario` stays valid.
- **FC-004/FC-021 (comments and directives dropped or moved).** Every comment and directive case above asserts the full output or the exact blank-line state, which would show a dropped or moved comment or directive.
- **FC-006, FC-022–FC-026, FC-029, FC-036, FC-018:** no rewrite of syntax, literals, identifiers, members, or names is involved — the change only decides blank lines.
- **FC-011 (loop without a decreasing measure):** no loop-until-stable is touched; second-pass stability is asserted by every formatter helper used.
- **FC-031 (stale comments):** for the implementer and the auditor; the test helpers' XML comments that described the removed cause and samples were removed with them.
- **Whole-repository stability:** `FormatterSelfHostingTests` in CI.

## Findings

- None against the spec. The analyzer-test files and the formatter-test files compile, and `scripts/test.sh --project architecture` passes (92/92), so `RequirementCoverageTests` sees a tagged test for every new requirement (FMT-BLANK-017 to FMT-BLANK-020, FMT-BREAK-030, RH5025-005) and FMT-PIPE-024.
- Full formatter project at d633172 + these tests: 40 failed, 3574 passed — exactly the tests listed as failing above. Analyzer RH5011/RH5025/RH5030 classes: 9 failed, 69 passed — exactly the tests listed as failing. I did not run the whole analyzer project; code-fix tests whose fix formats a block statement could change expectations once the formatter changes (FC-032), and the scan below found none.
- Scan for other tests pinning today's output of the changed cases: a script over every `*Test*` source (raw strings and `\n` literals) for `}` or `break;` followed by `{` at the same column, for a statement/comment/directive, a blank line, and `{`, and for a directive, blank line, and glued comment found no other expectation besides the node-level tests changed above.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~BlockStatementBlankLineTests|FullyQualifiedName~GluedCommentBlankLineTests|FullyQualifiedName~ClosingBracketBlankLineTests|FullyQualifiedName~CommentBeforeBraceBlankLineTests|FullyQualifiedName~LineBreakPhaseBlankLineTests|FullyQualifiedName~ReihitsuFormatterNodeBlankLineTests|FullyQualifiedName~PhaseStabilityCheckTests|FullyQualifiedName~TokenGapNormalizerTests|FullyQualifiedName~CommentLedFirstTokenAnchorTests|FullyQualifiedName~StructuralTransformSeparatorFinalOutputTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5025|FullyQualifiedName~RH5030|FullyQualifiedName~RH5011"
scripts/test.sh --project architecture
```

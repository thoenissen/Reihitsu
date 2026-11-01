Role: test implementer
Commit: see the commit that adds this file (subject "Test implementer: glued comment belongs to its token's line, FMT-BLANK-020 trigger and tokens [skip ci]")
Verdict: tests written

Repair round 2 after the audit, on `14-auditor.md` findings 1–3, `15-spec-author.md`, the spec on f97fbbf (FMT-BLANK-011, FMT-BLANK-018, FMT-BLANK-020) and the change doc's visible output changes 12–18. The prompt named the tests the round needs. It carried no intended fix, suspected cause, or hoped-for verdict. Expected values come from the requirements and spec examples, not from any patched code. Where a requirement decides only the blank lines and no requirement decides the columns (complex elements the line-break phase expands, a property pattern, an interpolation, a parenthesized pattern), the test uses `AssertBlankLinesAbove`, which still checks the second pass and the phase stability under LF and CRLF.

Run on f97fbbf plus these tests (production code unchanged): formatter focused set 63 failed / 172 passed, analyzer focused set 6 failed, architecture 92/92. Every failure was checked: in each one, the actual output differs from the expected output only in blank lines, or the blank-line state of the named line is wrong. The one exception is the phase-stability failure described under *Findings for the lead*, finding A.

## Findings for the lead

**A. FMT-BLANK-020's example and change-doc entry 13 contradict FMT-BLANK-011, and today's output for them comes from a phase reversal.** The spec example and entry 13 show `L =⏎// c⏎{` with no blank line above `// c`. FMT-BLANK-011 asks for exactly one blank line there: `// c` starts its own line, it is not the first content of a block, no directive precedes it, and it is not glued. `scripts/trace.sh` on f97fbbf confirms what happens. In every pass, the blank-line phase inserts that line, and then the line-break phase removes it (`L =⏎⏎// c` → `L =⏎// c`). That breaks FMT-BREAK-030 ("keeps the blank lines it finds") and leaves the line-break phase as a second owner of a blank line. The assigned *object* initializer behaves the way FMT-BLANK-011 says: the existing test `MovesBlankLineAboveOwnLineCommentInFrontOfCommentGluedToOpeningBrace` expects `Child =⏎⏎// note`. Two tests are written to the spec example. Both make no assumption about how the conflict is resolved, and both fail:
- `RemovesBlankLineBetweenCommentAndOpeningBraceOfAssignedCollectionInitializer`: the first pass equals the expected output. The phase-stability check then fails because BlankLinePhase, run alone, inserts the line above `// c`, and LineBreakPhase, run alone, changes the line too.
- `RemovesBlankLineBehindCommentLineBeforeAssignedCollectionInitializerAndOneLineComplexElement`: it will hit the same check once its other lines pass.

Either the example (and entry 13) changes to `L =⏎⏎// c⏎{`, or a requirement has to exempt a comment that follows an assignment's `=`. This is the "comment around `=`" class in the maintainer's guidance. The spec author decides which; it may be an output question.

**B. FMT-SWITCH-007's example contradicts the new FMT-BLANK-018.** `sdd/specs/formatter/switch-case-braces.md` (the example after "The gaps in front of trailing comments …", output line 701) shows a blank line inserted above `/* b1 */ /* b2 */ break; /* x */ // y` behind the new block. Under FMT-BLANK-018, that comment is glued to the terminal `break`. The statement rules decide the line, and SHARED-BLANK-003/004 ask for none in front of a terminal `break`. FMT-SWITCH-007's own text asks only for "the blank line FMT-BLANK-011 requires", which is now none. I left the three tests that pin that output unchanged, because one of them is the spec example: `BracedSwitchSectionCommentLayoutTests.TwoCommentsInFrontOfTheTerminalBreakStayOnTheBreaksLine`, `CommentInFrontOfTheTerminalBreakStaysOnTheBreaksLine`, and `SpecificationExampleKeepsEveryCommentLayoutOfTheBracedSections`. They pass on today's code. They will fail once FMT-BLANK-018 is implemented, unless the spec author keeps the line by a statement rule. The spec author decides; the change doc's entry 17 ("a comment glued to a statement") covers the output if the example changes.

## Tests

New and rewritten tests. "Fails now" means it fails on f97fbbf, on the blank-line difference stated.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `GluedCommentBlankLineTests.InsertsNoBlankLineAboveCommentGluedToArrayInitializerBraceOrArgumentListParenthesis` (was `SeparatesCommentGluedToArrayInitializerBraceAndArgumentListParenthesis`; input kept) | FMT-BLANK-011, FMT-BLANK-018 | `AssertRuleResult` | fails now: blank line inserted above `/* b */` and `/* e */` |
| `GluedCommentBlankLineTests.RemovesBlankLineAboveCommentGluedToComplexElementInitializerBrace` (was `SeparatesCommentGluedToComplexElementInitializerBrace`) | FMT-BLANK-018 | `AssertRuleResult` | fails now: blank above `/* c */` in D (inserted) and E (kept) |
| `GluedCommentBlankLineTests.RemovesBlankLinesAboveCommentsGluedToExtensionBlockBraces` (was `SeparatesCommentsGluedToExtensionBlockBraces`; adds the author's-blank side) | FMT-BLANK-018 | `AssertRuleResult` | fails now: blank lines above all four comments |
| `GluedCommentBlankLineTests.InsertsNoBlankLineAboveCommentGluedToInterpolationClosingBrace` (was `SeparatesCommentGluedToInterpolationClosingBrace`, now expects `false`) | FMT-BLANK-018 | `AssertBlankLinesAbove` | fails now: blank above `/* d */ }` |
| `GluedCommentBlankLineTests.RemovesAuthorBlankLineAboveCommentGluedToInterpolationClosingBrace` | FMT-BLANK-018 | `AssertBlankLinesAbove` | fails now: author's blank kept |
| `GluedCommentBlankLineTests.DecidesCommentGluedToComplexElementInitializerBraceIndependentOfLayout` (spec example; was `SeparatesCommentGluedToOneLineComplexElementInitializerOnly`) | FMT-BLANK-018 | `AssertRuleResult` | fails now: blank above `/* c2 */` kept |
| `GluedCommentBlankLineTests.InsertsNoBlankLineAboveCommentGluedToClosingBraceOfCollectionInitializer` (entry 14; was `SeparatesCommentGluedToClosingBraceOfAssignedCollectionInitializerOnly`) | FMT-BLANK-018 | `AssertRuleResult` | fails now: blank inserted above `/* l */` |
| `GluedCommentBlankLineTests.RemovesAuthorBlankLineAboveCommentGluedToClosingBraceOfCollectionInitializer` (spec example, entry 14; was `RemovesAuthorBlankLineAboveCommentGluedToClosingBraceOfUnassignedCollectionInitializerOnly`) | FMT-BLANK-018 | `AssertRuleResult` | fails now: blank above `/* l */` kept |
| `GluedCommentBlankLineTests.RemovesBlankLineAboveCommentGluedThroughDocumentationCommentAndSeparatesDocumentationCommentLeadingBraceLine` (spec example, entry 15; was `SeparatesCommentInFrontOfDocumentationCommentOnBraceLineOnly`) | FMT-BLANK-011, FMT-BLANK-018 | `AssertRuleResult` | fails now: blank above `/* x */ /** Doc */ {` kept |
| `GluedCommentBlankLineTests.InsertsNoBlankLineAboveCommentGluedThroughDocumentationComment` (entry 15) | FMT-BLANK-018 | `AssertRuleResult` | fails now: blank inserted |
| `GluedCommentBlankLineTests.DecidesBlankLinesAboveGluedCommentAsDirectlyBeforeItsToken` (spec example: array `{`, member, statement, argument-list `)`, `else`) | FMT-BLANK-011, FMT-BLANK-018 | `AssertRuleResult` | fails now: blank lines above `/* b */`, `/* m */`, `/* e */`, `/* k */` |
| `GluedCommentBlankLineTests.InsertsNoBlankLineAboveCommentGluedToChainedKeyword` (`else`, `catch`, `finally`, do `while`; entry 16) | FMT-BLANK-018 | `AssertRuleResult` | fails now: blank lines inserted |
| `GluedCommentBlankLineTests.RemovesAuthorBlankLineAboveCommentGluedToCatchFinallyOrDoWhile` (entry 16) | FMT-BLANK-018 | `AssertRuleResult` | fails now: author's blank lines kept |
| `GluedCommentBlankLineTests.InsertsNoBlankLineAboveCommentGluedToPropertyPatternBrace` (entry 16) | FMT-BLANK-018 | `AssertBlankLinesAbove` | fails now: blank above `/* p */` |
| `GluedCommentBlankLineTests.RemovesAuthorBlankLineAboveCommentGluedToPropertyPatternBrace` (entry 16) | FMT-BLANK-018 | `AssertBlankLinesAbove` | fails now: blank above `/* p */` |
| `GluedCommentBlankLineTests.InsertsNoBlankLineAboveCommentGluedToStatementMemberOrArgumentListParenthesis` (entry 17) | FMT-BLANK-011, FMT-BLANK-018 | `AssertRuleResult` | fails now: blank lines inserted |
| `GluedCommentBlankLineTests.KeepsAuthorBlankLineAboveCommentGluedToMemberOrArgumentListParenthesis` (entry 17, other side) | FMT-BLANK-018 | `AssertRuleResult` | passes now (holds) |
| `GluedCommentBlankLineTests.InsertsStatementBlankLineAboveCommentGluedToStatementAfterClosingBrace` (`}⏎/* s */ a++;`, entry 17) | FMT-BLANK-018 | `AssertRuleResult` | passes now (holds) |
| `GluedCommentBlankLineTests.InsertsNoBlankLineAboveCommentGluedToComplexElementWithLambdaBlockBody` (finding 1) | FMT-BLANK-018 | `AssertBlankLinesAbove` | fails now: blank above `/* c */` in pass 1 |
| `GluedCommentBlankLineTests.InsertsNoBlankLineAboveCommentGluedToComplexElementWithObjectInitializer` (finding 1) | FMT-BLANK-018 | `AssertBlankLinesAbove` | fails now: same |
| `GluedCommentBlankLineTests.InsertsNoBlankLineAboveCommentGluedToComplexElementWithCollectionInitializer` (finding 1) | FMT-BLANK-018 | `AssertBlankLinesAbove` | fails now: same |
| `GluedCommentBlankLineTests.InsertsNoBlankLineAboveCommentGluedToComplexElementWithAnonymousObject` (finding 1) | FMT-BLANK-018 | `AssertBlankLinesAbove` | fails now: same |
| `GluedCommentBlankLineTests.RemovesAuthorBlankLineAboveCommentGluedToComplexElementWithObjectInitializer` (finding 1, author's blank) | FMT-BLANK-018 | `AssertBlankLinesAbove` | fails now: blank kept in pass 1 |
| `GluedCommentBlankLineTests.RemovesAuthorBlankLineAboveCommentGluedToComplexElementWithLambdaBlockBody` (finding 1, author's blank) | FMT-BLANK-018 | `AssertBlankLinesAbove` | fails now: same |
| `CommentBeforeBraceBlankLineTests.RemovesBlankLineBehindCommentLineBeforeAssignedCollectionInitializerAndOneLineComplexElement` (spec example; entries 13, 18; finding 2 `/* a */ /* b */`) | FMT-BLANK-020 | `AssertRuleResult` | fails now: blank lines behind `// e` and `/* a */ /* b */` kept; see finding A |
| `CommentBeforeBraceBlankLineTests.RemovesBlankLineBetweenCommentAndOpeningBraceOfAssignedCollectionInitializer` (entry 13) | FMT-BLANK-020 | `AssertRuleResult` | fails now: phase-stability check; see finding A |
| `CommentBeforeBraceBlankLineTests.RemovesBlankLineBetweenCommentAndOneLineComplexElementInitializer` (entry 18) | FMT-BLANK-020 | `AssertRuleResult` | fails now: blank behind `// c` kept |
| `CommentBeforeBraceBlankLineTests.RemovesBlankLineBetweenCommentAndMultiLineComplexElementInitializer` (entry 18, "before and after") | FMT-BLANK-020 | `AssertRuleResult` | passes now (holds) |
| `CommentBeforeBraceBlankLineTests.RemovesBlankLineBetweenCommentAndComplexElementThatLineBreakPhaseExpands` (finding 1, `// c⏎⏎{ 2, new P { A = 2 } }`, one pass) | FMT-BLANK-020 | `AssertRuleResult` | fails now: blank behind `// c` kept in pass 1 |
| `CommentBeforeBraceBlankLineTests.KeepsBlankLineBetweenCommentAndClosingBraceOfAssignedCollectionInitializer` (change doc *Unchanged*) | FMT-BLANK-020 | `AssertRuleResult` | passes now (holds) |
| `CommentBeforeBraceBlankLineTests.RemovesBlankLineBehindLineOfTwoDelimitedComments` (finding 2; collection-expression `]`, method `{`, `if` `{`, object-initializer `}`, method `}`, type `}`) | FMT-BLANK-020 | `AssertRuleResult` via `AssertRemovesBlankLineBehindCommentLine` | fails now: all six blank lines kept |
| `CommentBeforeBraceBlankLineTests.RemovesBlankLineBehindLineOfDelimitedCommentAndLineComment` (finding 2, `/* a */ // b`) | FMT-BLANK-020 | same | fails now: same |
| `CommentBeforeBraceBlankLineTests.RemovesBlankLineBehindLineCommentThatFollowsEndOfMultiLineComment` (finding 2, `/* x⏎   y */ // b`) | FMT-BLANK-020 | same | fails now: same |
| `CommentBeforeBraceBlankLineTests.RemovesBlankLineBehindLineCommentThatFollowsDelimitedDocumentationComment` (finding 2, `/** d */ // c`) | FMT-BLANK-020 | same | fails now: same |
| `CommentBeforeBraceBlankLineTests.RemovesBlankLineBehind…BeforeParenthesizedPatternParenthesis` (four tests, one per shape above) | FMT-BLANK-020 | `AssertBlankLinesAbove` via `AssertRemovesBlankLineBehindCommentLineBeforeParenthesizedPatternParenthesis` | fails now: blank above `)` kept |
| `RH5024ClosingBraceMustNotBePrecededByBlankLineFormatterTests.VerifyFormatterRemovesBlankLineBehindLineOfTwoDelimitedComments` (finding 2, FC-013 parity) | FMT-BLANK-020 | `FormatterTestsBase<TAnalyzer>.VerifyFormatter(source, fixed)` | fails now: blank before `}` kept |
| `CommentInsideConstructBlankLineTests.NoBlankLineIsInsertedBeforeCommentGluedToConstraintClause` (was `BlankLineIsInsertedBeforeCommentLeadingConstraintClause`, untagged characterization) | FMT-BLANK-018 | `AssertRuleResult` | fails now: blank inserted |
| `CommentInsideConstructBlankLineTests.NoBlankLineIsInsertedBeforeBlockCommentGluedToArgument` (was `BlankLineIsInsertedBeforeBlockCommentLeadingArgument`) | FMT-BLANK-018 | `AssertRuleResult` | fails now: blank inserted |

The parenthesized-pattern `)` tests follow FMT-BLANK-020's text, which names that token and says that what stands in front of the comment does not matter. `14-auditor.md` lists `)` under "Hold" (both `main` and the branch keep the line). The spec decides otherwise, so these four tests ask for a change to `main`'s output. A single `// c` before the same `)` already loses its blank line on the branch (probe with `scripts/trace.sh`).

Adjusted existing tests: the blank line above a glued comment is removed from the expected output, and nothing else changes. That is FMT-BLANK-018's "above any other token … none is inserted", or "removed" for the array-initializer `{`, the type `{`, and the assigned collection initializer's `}`. All of these fail now, on that blank line only.
- `CommentLedFirstTokenAnchorTests`: 12 tests (`ArrayInitializer…`, `CollectionExpression…`, `ListPattern…`, `ObjectCreation…`, `CollectionCreation…`, `AnonymousObject…`, `SwitchExpression…`, `LambdaBlock…`, `BinaryOperator…`, `ConditionalBranches…`, `ChainLink…`, `PatternCombinator…AlignsTo…BehindCommentThatLeadsTheLine`). The comment is glued to `{`, `[`, `new`, an expression, or `o is`.
- `FieldDeclarationSplitTests`: 12 tests whose comment is glued to the first declarator (`CommentBeforeFirstDeclarator…`, `TwoCommentsBeforeFirstDeclaratorArePreserved`, `…AreBothPreserved`, `MisindentedCommentBeforeFirstDeclaratorIsNormalizedToMemberIndentation`).
- `ParameterContinuationLineAlignmentTests.RecordParameterTypeAfterAttributeListAndBlockCommentLineIsAligned`: glued to a parameter type. Its summary is updated.
- `HorizontalSpacingTests.BlockCommentBeforeGenericBracketOnContinuationLineKeepsGapUntouched`: glued to `<`.
- `DelimitedDocumentationBeforeBlockStatementTests.KeepsOwnLineCommentAndDelimitedDocumentationGluedToTypeOpeningBrace`: entry 15.
- `RH5201MethodChainsShouldBeAlignedAnalyzerTests.VerifyLaterLinkOnLineStartingWithBlockCommentIsDetectedAndFixed` and its `…WithCrLf` sibling: code-fix output, glued to `.Select`.
- `RH5201FluentChainLinkFormatterTests.VerifyBlockCommentInFrontOfWrappedFirstLinkAlignsTheLineStart` and `…LaterLinkAlignsTheLineStart`: glued to a chain link.
- `RH5304NestedCollectionInitializerAssignmentsShouldBeFormattedCorrectlyFormatterTests.VerifyFormatterKeepsCommentPrefixedOrdinaryElementsStable`: the clean source drops the blank line above `/* Keep close. */ }` (entry 14). On f97fbbf the formatter inserts it again.

The class summary of `CommentInsideConstructBlankLineTests` now says that its glued cases follow FMT-BLANK-018. Its line-comment test stays an untagged characterization test.

I found the adjusted tests with a scan over every raw-string fixture in the formatter, analyzer, and CLI test projects. The scan matched each line that starts with a delimited comment followed by code, and compared that line's blank-line state between the input and the expected output. Every expected output that has a blank line above a glued comment the input did not have is in the list above. The exceptions are the tests where FMT-BLANK-017 or a statement rule inserts the line, which are unchanged (`ReihitsuFormatterNodeBlankLineTests.InsertsBlankLineAboveGluedCommentOfBlockStatementAfterClosingBrace`, `BlockStatementBlankLineTests` after `break`), and the three switch tests of finding B. Glued comments that had an author's blank line before a member, a statement, an element, or a chain link keep it and are unchanged. The spec examples in `sdd/specs` were scanned the same way, which found finding B.

## Not covered, and why

- Columns of complex elements that the line-break phase expands behind a glued comment (the auditor's "odd column 31"): no requirement decides them, so those tests assert blank-line state only. `RemovesBlankLineBetweenCommentAndComplexElementThatLineBreakPhaseExpands` pins the full output, because there no comment shares the element's line.
- RH5020 reporting the formatter's output above a glued comment (FC-013): intended. The maintainer decided that RH5020 keeps reporting glued comments, and the guidance calls them unwanted. No RH5020 formatter-parity test contains a glued comment.
- RH5025 parity for finding 2's `{` members: one RH5024 parity test covers the trigger, and the formatter tests cover the `{` tokens. RH5025's report predicate is not changed by this round.
- Finding 4 (one glued predicate) and finding 5 (summaries): internal, with no visible behavior. The FMT-BLANK-018 tests above exercise both rewriters, which read the predicate: FMT-BLANK-011 skip and FMT-BLANK-018 removal.
- Directive in the gap, region directives, answers 3, 4, 5, and 7: the existing tests (`KeepsBlankLinesOfGapWithDirectiveInFrontOfGluedComment`, `…EndRegion…`, `…EndIf…`, `SeparatesCommentGluedToBlockStatementBraceAfterStatement`, `InsertsBlankLineAboveOwnLineCommentInFrontOfGluedComment`) are unchanged and pass.
- CRLF, second pass, phase stability: built into `AssertRuleResult`, `AssertBlankLinesAbove`, and `VerifyFormatter`.
- Code fix: the only code-fix outputs that change are the RH5201 ones above. They come through the node-level entry point, which reaches the same blank-line phase.

## Commands

```shell
scripts/test.sh --configuration Debug --project formatter --filter "FullyQualifiedName~GluedCommentBlankLineTests|FullyQualifiedName~CommentBeforeBraceBlankLineTests|FullyQualifiedName~CommentInsideConstructBlankLineTests|FullyQualifiedName~DelimitedDocumentationBeforeBlockStatementTests|FullyQualifiedName~CommentLedFirstTokenAnchorTests|FullyQualifiedName~ParameterContinuationLineAlignmentTests|FullyQualifiedName~HorizontalSpacingTests|FullyQualifiedName~FieldDeclarationSplitTests|FullyQualifiedName~BracedSwitchSectionCommentLayoutTests"
scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH5201MethodChainsShouldBeAlignedAnalyzerTests|FullyQualifiedName~RH5201FluentChainLinkFormatterTests|FullyQualifiedName~RH5304NestedCollectionInitializerAssignmentsShouldBeFormattedCorrectlyFormatterTests|FullyQualifiedName~RH5024ClosingBraceMustNotBePrecededByBlankLineFormatterTests"
scripts/test.sh --configuration Debug --project formatter
scripts/test.sh --configuration Debug --project architecture
```

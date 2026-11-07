Role: test implementer
Commit: 8432ef4
Verdict: #1243: reproduced; #1383: reproduced; #1149: reproduced; #1391: reproduced (base list, empty list); regression tests for the two instances resolved on main; #952: not reproduced (regression tests only, as decided); #1611: reproduced

The expected values come from the spec examples and requirements at `8432ef4` and from the maintainer's answers Q1 and Q2 (`00-lead.md`). Where a requirement fixes only one line and the rest of a fixture is laid out by requirements the run does not change, the rest is today's output for that line. Today's output came from `scripts/format.sh` on probe fixtures. Every new formatter test uses `AssertRuleResult` (LF and CRLF, second pass, per-phase stability), `AssertBlankLinesAbove`, or a column check plus `AssertSecondPassIsStable`, unless the row says otherwise. My prompt carried no intended fix or hoped-for verdict.

"Red" means the test fails today on the issue's own expected-versus-actual difference. I checked every red test's message: each shows the blank line, the kept wrap, the column, or the whitespace measure in question, and none fails on unrelated layout.

## thoenissen/Reihitsu#1243

### Tests
File: `src/Reihitsu.Formatter.Test/Regression/LineBreaks/LoneOperatorJoinBlankLineTests.cs`

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| LoneLogicalAndConditionalOperatorsAreJoinedWithoutBlankLine (the issue's input and expected output) | FMT-BREAK-030, FMT-BREAK-017, FMT-BREAK-018 | AssertRuleResult | red |
| LoneArithmeticOperatorIsJoinedWithoutBlankLine | FMT-BREAK-030 | AssertRuleResult | red |
| LoneCoalesceOperatorIsJoinedWithoutBlankLine | FMT-BREAK-030 | AssertRuleResult | red |
| LoneAsOperatorIsJoinedWithoutBlankLine | FMT-BREAK-030 | AssertRuleResult | red |
| LoneOperatorBehindEndOfLineCommentIsJoinedWithoutBlankLine | FMT-BREAK-030 (FC-019) | AssertRuleResult | red |
| LoneOperatorBelowCommentLineIsJoinedWithoutBlankLine | FMT-BREAK-030 (FC-021) | AssertRuleResult | red |
| SeveralLoneOperatorsAreJoinedWithoutBlankLines | FMT-BREAK-030 | AssertRuleResult | red |
| LoneConditionalColonIsJoinedWithoutBlankLine | FMT-BREAK-030, FMT-BREAK-018 | AssertRuleResult | red |
| LoneConditionalColonBehindSameLineQuestionIsJoinedWithoutBlankLine | FMT-BREAK-030, FMT-BREAK-018 | AssertRuleResult | red |
| LoneConditionalQuestionBehindEndOfLineCommentIsJoinedWithoutBlankLine | FMT-BREAK-030, FMT-BREAK-018 | AssertRuleResult | red |
| LoneConditionalColonBelowCommentLineIsJoinedWithoutBlankLine | FMT-BREAK-030, FMT-BREAK-018 | AssertRuleResult | red |
| OperatorAtEndOfOperandLineMovesWithoutBlankLine (the other side of the condition: no line break in the gap) | FMT-BREAK-017, FMT-BREAK-030 | AssertRuleResult | green |
| LoneOperatorFollowedByCommentGainsNoBlankLine (join withheld) | FMT-BREAK-030 | AssertBlankLinesAbove | green |
| AuthorBlankLineAboveOperatorStays | FMT-BREAK-030 | AssertRuleResult | green |

### Scenarios
| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| The issue's input (`a⏎&&⏎b`, `a⏎?⏎1⏎: 2`) | LF, CRLF | formatter | reproduced | blank line above `&& b` and above `? 1`; expected none |
| Each cell of the skeptic's table (`+`, `??`, `as`, `\|\|` chain, `:`, comment cells) | LF, CRLF | formatter | reproduced | blank line above the joined operator line, or above the comment line; expected none |

### Not covered, and why
- `is string` was not added. It is the same `BinaryExpressionSyntax` path as `as`, which is covered. That rests on the skeptic's statement that one rewriter method handles every binary operator kind (`BinaryOperatorLineBreakRewriter.NormalizeBinaryOperatorPosition`), not on a test of `is`.
- A directive in the gap (FC-020) has no test. A directive line between an operand and a lone operator keeps the gap's line break either way. The comment cells cover the "line already ends" condition.
- RH5302/RH5308 are not involved (skeptic).

## thoenissen/Reihitsu#1383

### Tests
Files: `src/Reihitsu.Formatter.Test/Regression/LineBreaks/IndexerParameterListLayoutTests.cs`, `CompoundAssignmentLayoutTests.cs`, and `src/Reihitsu.Formatter.Test/Regression/Indentation/CollectionExpressionClosingBracketCommentTests.cs`

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| IndexerParameterListHugsItsDeclaration (spec example) | FMT-BREAK-011, FMT-PIPE-014 | AssertRuleResult | red |
| LeadingCommaOfIndexerParameterMovesToPreviousLine (issue input) | FMT-BREAK-011 | AssertRuleResult | red |
| WrappedIndexerParameterListGetsOneParameterPerLine (spec example) | FMT-BREAK-012 | AssertRuleResult | red |
| FirstIndexerParameterJoinsOpeningBracket | FMT-BREAK-012 | AssertRuleResult | red |
| OneLineIndexerParameterListStays | FMT-BREAK-012 | AssertRuleResult | green |
| ExplicitInterfaceIndexerOpeningBracketJoinsThis | FMT-BREAK-011 (FC-024) | AssertRuleResult | red |
| IndexerWithAccessorBlockLeadingCommaMovesToPreviousLine | FMT-BREAK-011 (FC-025) | AssertRuleResult | red |
| MultiLineParameterSplitsIndexerParameterList (the method form beside it is today's output) | FMT-BREAK-013 | AssertRuleResult | red |
| WrapInsideIndexerParameterIsJoined | FMT-BREAK-015 | AssertRuleResult | green |
| WrappedIndexerParameterDefaultIsJoined | FMT-BREAK-015 | AssertRuleResult | green |
| CommentBehindIndexerParameterKeepsCommaOnItsLine | FMT-BREAK-011 (FC-019) | AssertRuleResult | green |
| CompoundAssignmentsAreJoinedWhileBinaryOperatorKeepsItsWrap (spec example) | FMT-BREAK-016, FMT-PIPE-014 (FC-002) | AssertRuleResult | red |
| SubtractionAssignmentWrappedBeforeOperatorIsJoined | FMT-BREAK-016 | AssertRuleResult | red |
| ShiftAssignmentWrappedAfterOperatorIsJoined | FMT-BREAK-016 | AssertRuleResult | red |
| LoneUnsignedShiftAssignmentOperatorIsJoined | FMT-BREAK-016 | AssertRuleResult | red |
| EventSubscriptionWrappedBeforeOperatorIsJoined | FMT-BREAK-016 | AssertRuleResult | red |
| CompoundAssignmentInForIncrementorIsJoined | FMT-BREAK-016 | AssertRuleResult | red |
| CompoundAssignmentAsLambdaBodyIsJoined | FMT-BREAK-016 | AssertRuleResult | red |
| NestedCoalesceAssignmentIsJoined | FMT-BREAK-016 | AssertRuleResult | red |
| CompoundAssignmentInsideArgumentIsJoined (single-argument call) | FMT-BREAK-016 | AssertRuleResult | red |
| CompoundAssignmentBehindEndOfLineCommentKeepsItsWrap | FMT-BREAK-016 (FC-019) | AssertBlankLinesAbove | green |
| CompoundAssignmentWrappedAroundDirectiveKeepsItsWrap | FMT-BREAK-016 (FC-020) | AssertBlankLinesAbove | green |
| OneLineCompoundAssignmentStays | FMT-BREAK-016 | AssertRuleResult | green |
| CommentBeforeClosingBracketIsIndentedLikeCommentBeforeClosingBrace (spec example) | FMT-INDENT-006, FMT-PIPE-014 | AssertRuleResult | red |
| CommentBeforeClosingBracketOfFieldInitializerIsIndented | FMT-INDENT-006 | AssertRuleResult | red |
| DelimitedCommentBeforeClosingBracketIsIndented | FMT-INDENT-006 | AssertRuleResult | red |
| CommentBeforeClosingBracketOfNestedCollectionExpressionIsIndented | FMT-INDENT-006 | AssertRuleResult | red |
| CommentBeforeClosingBracketOfEmptyCollectionExpressionIsIndented | FMT-INDENT-006 | AssertRuleResult | red |
| CommentBeforeClosingBracketOfArgumentIsIndented | FMT-INDENT-006 | AssertRuleResult | red |
| CommentInElementsColumnBeforeClosingBracketStays (formatted code must not change) | FMT-INDENT-006 | AssertRuleResult | red |
| CommentBeforeClosingBracketOfListPatternStaysInBracketColumn (FC-003 boundary; characterization) | — | AssertRuleResult | green |

### Scenarios
| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| `int this[int a⏎, int b] => a;` | LF, CRLF | formatter | reproduced | unchanged; expected `int this[int a,⏎int b]` |
| `x⏎+= 1;`, `s ??=⏎"x";` | LF, CRLF | formatter | reproduced | wraps kept, continuation at the statement column; expected joined |
| Comment in front of a collection expression's `]` | LF, CRLF | formatter | reproduced | comment in the `]` column; expected the elements' column |

### Not covered, and why
- Interface indexer declaration `int this⏎[int a] { get; }`. I wrote this test and dropped it: once its declaration is joined, no requirement of this run decides whether its accessor list `{ get; }` stays on one line, and today it is expanded. The bracketed list is `BracketedParameterListSyntax` whatever its parent; the class and explicit-interface indexers cover it.
- A compound assignment inside an argument of a multi-argument list (`M(x⏎+= 1, y)`). No requirement I found decides whether the list returns to one line once FMT-BREAK-016 has joined the wrap. FMT-BREAK-015's *Why* states it only for -015 joins, and FMT-BREAK-012 counts only wraps between elements. Today's output after the join can't be known yet, so the test uses a single-argument call. This is a candidate open question for the lead, not asserted.
- The attribute-target colon is resolved on main and covered by `src/Reihitsu.Formatter.Test/Regression/Spacing/AttributeTargetColonSpacingTests.cs` (skeptic).
- The `]` of an element access and of an attribute list: both are outside FMT-INDENT-006's collection-expression clause. The list-pattern test pins that boundary.
- In the empty collection-expression test, the input already carries a blank line above the comment. When it does not, whole-document formatting inserts one today (`[⏎⏎// c`). FMT-BREAK-030 keeps a blank line the author wrote, so the input fixes the blank-line state and the test asserts only the column.

## thoenissen/Reihitsu#1149

### Tests
Files: `src/Reihitsu.Formatter.Test/Regression/LineBreaks/EnumMemberValueLayoutTests.cs`, `src/Reihitsu.Analyzer.Test/Formatting/RH5111AssignmentsMustHaveProperLineBreaksAnalyzerTests.cs` (4 tests added), `src/Reihitsu.Analyzer.Test/Formatter/Formatting/RH5111AssignmentsMustHaveProperLineBreaksFormatterTests.cs` (1 test added)

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| SpecificationExampleIsLaidOutAsAssignments (FMT-BREAK-016 second example) | FMT-BREAK-016, FMT-BREAK-015, FMT-PIPE-014 | AssertRuleResult | red |
| EnumMemberEqualsBelowNameIsJoined (issue input; whole-document formatting drops the last comma, FMT-STRUCT-019) | FMT-BREAK-016 | AssertRuleResult | red |
| LoneEnumMemberEqualsIsJoined (issue's third input) | FMT-BREAK-016 | AssertRuleResult | red |
| LastEnumMemberEqualsIsJoined | FMT-BREAK-016 | AssertRuleResult | red |
| EnumMemberWithAttributeEqualsIsJoined | FMT-BREAK-016 | AssertRuleResult | red |
| DocumentedEnumMemberEqualsIsJoined | FMT-BREAK-016 | AssertRuleResult | red |
| EnumMemberBehindEndOfLineCommentKeepsItsWrap | FMT-BREAK-016 (FC-019) | AssertBlankLinesAbove | green |
| OneLineEnumMemberStays | FMT-BREAK-016 | AssertRuleResult | green |
| ParameterDefaultEqualsBelowNameIsJoined | FMT-BREAK-015 | AssertRuleResult | green |
| VerifyEnumMemberEqualsBelowNameIsReported (RH5111-001 example) | RH5111-001 | Verify (analyzer) | green |
| VerifyEnumMemberEqualsBelowNameIsFixedInOneApplication (`A = 1,`, comma kept; LF and CRLF; one incremental and one Fix All iteration) | RH5111-001, RH5111-004 (FC-007) | Verify (code fix) | red |
| VerifyLoneEnumMemberEqualsIsFixedInOneApplication | RH5111-004 | Verify (code fix) | red |
| VerifyFixAllJoinsSeveralEnumMembers (two members, one Fix All iteration) | RH5111-004 | Verify (code fix) | red |
| VerifyFormatterFixesEnumMemberEqualsBelowName (formatter clears RH5111, LF and CRLF, second pass) | FMT-BREAK-016 (FC-013) | VerifyFormatter | red |

### Scenarios
| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| `enum E { A⏎= 1, }` | LF, CRLF | formatter | reproduced | `A⏎    = 1`; expected `A = 1` |
| Same input, RH5111 fix | LF, CRLF | code fix | reproduced | `A⏎    = 1,` after the fix; expected `A = 1,` |
| Two members, Fix All | LF | code fix | reproduced | both members stay wrapped |

### Not covered, and why
- A directive between the name and the `=`: it keeps the wrap, as the comment cell does, and no requirement of the run changes that. Not added.

## thoenissen/Reihitsu#1391

### Tests
Files: `src/Reihitsu.Formatter.Test/Regression/Indentation/BaseListContinuationAlignmentTests.cs`, `EmptyWrappedListClosingDelimiterTests.cs`, `RawStringOtherLineTerminatorAlignmentTests.cs`, and one test added to `ZeroWidthTokenLineStartTests.cs`

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| LineInsideOnlyBaseTypeAndEnumUnderlyingTypeAlignsWithTheType (spec example, Q1; enum member without comma, see below) | FMT-INDENT-013 | AssertRuleResult | red |
| LineInsideLaterBaseTypeAlignsWithTheType (spec example, issue input) | FMT-INDENT-013 | AssertRuleResult | red |
| LineInsideLaterBaseTypeInNamespaceAlignsWithTheType (FC-027) | FMT-INDENT-013 | AssertRuleResult | red |
| LineInsideFirstOfTwoBaseTypesAlignsWithTheType | FMT-INDENT-013 | AssertRuleResult | red |
| LineInsideInterfaceBaseTypeAlignsWithTheType | FMT-INDENT-013 | AssertRuleResult | red |
| LineInsideStructBaseTypeAlignsWithTheType | FMT-INDENT-013 | AssertRuleResult | red |
| LineInsideRecordBaseTypeAlignsWithTheType | FMT-INDENT-013 | AssertRuleResult | red |
| PrimaryConstructorBaseArgumentStaysAlignedAfterParenthesis | FMT-INDENT-012 | AssertRuleResult | green |
| ClosingParenthesisOfEmptyArgumentListAlignsAfterOpeningParenthesis (issue input) | FMT-INDENT-012 | AssertRuleResult | red |
| ClosingParenthesisOfEmptyParameterListAlignsAfterOpeningParenthesis | FMT-INDENT-012 | AssertRuleResult | red |
| ClosingParenthesisOfEmptyObjectCreationAlignsAfterOpeningParenthesis | FMT-INDENT-012 | AssertRuleResult | red |
| ClosingParenthesisOfEmptyConstructorInitializerAlignsAfterOpeningParenthesis | FMT-INDENT-012 | AssertRuleResult | red |
| ClosingParenthesisOfEmptyPrimaryConstructorBaseAlignsAfterOpeningParenthesis | FMT-INDENT-012 | AssertRuleResult | red |
| ClosingParenthesisOfEmptyAttributeArgumentListAlignsAfterOpeningParenthesis | FMT-INDENT-012 | AssertRuleResult | red |
| ClosingParenthesisOfEmptyLocalFunctionParameterListAlignsAfterOpeningParenthesis | FMT-INDENT-012 | AssertRuleResult | red |
| ClosingParenthesisOfEmptyDelegateParameterListAlignsAfterOpeningParenthesis | FMT-INDENT-012 | AssertRuleResult | red |
| ClosingParenthesisOfEmptyLambdaParameterListAlignsAfterOpeningParenthesis (column 27 only; no requirement of the run decides the rest of the lambda's lines) | FMT-INDENT-012 | column check + AssertSecondPassIsStable | red |
| ClosingParenthesisOfEmptyAnonymousMethodParameterListAlignsAfterOpeningParenthesis (column 35 only, same reason) | FMT-INDENT-012 | column check + AssertSecondPassIsStable | red |
| ClosingParenthesisBehindDisabledBranchAlignsAfterOpeningParenthesis (empty list holding only an `#if` block) | FMT-INDENT-012 (FC-020) | AssertRuleResult | red |
| ClosingParenthesisOfNonEmptyListStaysAlignedAfterOpeningParenthesis | FMT-INDENT-012 | AssertRuleResult | green |
| RawStringWithOtherLineTerminatorIsAligned (lone `\r`, U+0085, U+2028, U+2029 inside the literal, in LF and CRLF files; byte-exact, second pass) | FMT-RAW-001 | FormatSyntaxTree, byte-exact | green (resolved on main) |
| ZeroWidthTokenLineStartTests.OmittedTypeArgumentBehindCommentTakesBlockColumn (issue input `Dictionary< // omitted⏎,>`) | FMT-INDENT-015 | AssertRuleResult | green (resolved on main) |

### Scenarios
| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| Base list `System. // c⏎ICloneable` | LF, CRLF | formatter | reproduced | `ICloneable` at column 0; expected column 10 |
| `M( // c⏎);` | LF, CRLF | formatter | reproduced | `)` at column 8; expected column 10 |
| Omitted type argument, issue input | LF, CRLF | formatter | not reproduced | `,>);` at column 8, as expected |
| Lone-CR raw string and Unicode terminators | LF, CRLF | formatter | not reproduced | aligned at column 16, as expected |

### Not covered, and why
- Bracketed argument and parameter lists, tuples, tuple types, and calling conventions cannot be empty without a syntax error, which the formatter skips (skeptic).
- A type argument list nested in a base type is follow-up 4 in `00-lead.md`.

## thoenissen/Reihitsu#952

### Tests
File: `src/Reihitsu.Formatter.Test/Regression/Indentation/DirectiveBehindDisabledTextTests.cs`

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| EndIfBehindDisabledBranchInMethodBodyMovesToColumnZero (the issue's exact input and expected output) | FMT-INDENT-009 (FC-034) | AssertRuleResult | green |
| DirectivesBehindDisabledBranchesAtMemberLevelMoveToColumnZero (`#elif`/`#else`/`#endif`) | FMT-INDENT-009 (FC-020) | AssertRuleResult | green |

### Scenarios
| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| The issue's input | LF, CRLF | formatter | not reproduced | `#endif` at column 0, as expected |
| Member level, sibling shape | LF, CRLF | formatter | not reproduced | all at column 0 |

### Not covered, and why
No analyzer reports directive columns in a way that would make a counterpart surface relevant (RH6004 is satisfied by column 0, skeptic). There is no code-fix path.

## thoenissen/Reihitsu#1611

### Tests
Files: `src/Reihitsu.Formatter.Test/Regression/FullPipeline/RawStringClosingWhitespaceMeasureTests.cs`, `src/Reihitsu.Analyzer.Test/Formatter/Formatting/RH5202RawStringLiteralsShouldBeFormattedCorrectlyFormatterTests.cs` (5 tests added). The fixtures write `·` for U+00A0 and `→` for a tab and replace them at run time, so the source files hold no invisible characters.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| NoBreakSpacesCountAsOneColumnEach (FMT-RAW-002 NBSP example) | FMT-RAW-001, FMT-RAW-002 | AssertRuleResult | red |
| TabsCountAsOneColumnEachAndStay (FMT-RAW-001 tab example, Q2) | FMT-RAW-001 | AssertRuleResult | red |
| LiteralBehindNoBreakSpacesMovesRight (issue input) | FMT-RAW-001, FMT-RAW-002 | AssertRuleResult | red |
| LiteralAlignedBySharedMeasureStays (12 spaces + 4 NBSP) | FMT-RAW-001 | AssertRuleResult | red |
| LiteralAlignedBySharedMeasureWithTabsStays (12 spaces + 4 tabs) | FMT-RAW-001 | AssertRuleResult | red |
| LeftShiftSmallerThanLeadingSpacesRemovesOnlySpaces | FMT-RAW-002 | AssertRuleResult | red |
| LeftShiftEqualToLeadingSpacesRemovesEverySpace | FMT-RAW-002 | AssertRuleResult | red |
| LeftShiftLargerThanLeadingSpacesRemovesNoBreakSpaces | FMT-RAW-002 | AssertRuleResult | red |
| LiteralBehindEmSpacesMovesRight | FMT-RAW-001 | AssertRuleResult | red |
| LiteralBehindVerticalTabsMovesRight | FMT-RAW-001 | AssertRuleResult | red |
| LiteralBehindFormFeedsMovesRight | FMT-RAW-001 | AssertRuleResult | red |
| SpacesInFrontOfNoBreakSpacesCountWithThem | FMT-RAW-001, FMT-RAW-002 | AssertRuleResult | red |
| SpacesBehindNoBreakSpacesCountWithThem | FMT-RAW-001, FMT-RAW-002 | AssertRuleResult | red |
| InterpolatedLiteralBehindNoBreakSpacesMovesRight | FMT-RAW-001, FMT-RAW-003 | AssertRuleResult | red |
| Utf8LiteralBehindNoBreakSpacesMovesRight | FMT-RAW-001 | AssertRuleResult | red |
| DeeperContentLineKeepsRelativeIndentationOnLeftShift | FMT-RAW-002 | AssertRuleResult | red |
| AnchoredLinesFollowTheMovedLiteral (chain on the closing line, wrapped hole) | FMT-RAW-001, FMT-INDENT-030 | AssertRuleResult | red |
| VerifyFormatterAlignsLiteralBehindNoBreakSpacesAndClearsTheDiagnostic | FMT-RAW-001, FMT-RAW-002 (FC-013) | VerifyFormatter | red |
| VerifyFormatterAlignsLiteralBehindTabsAndClearsTheDiagnostic | FMT-RAW-001 (FC-013) | VerifyFormatter | red |
| VerifyFormatterMovesLiteralBehindNoBreakSpacesLeftAndClearsTheDiagnostic | FMT-RAW-002 (FC-013) | VerifyFormatter | red |
| VerifyFormatterKeepsLiteralAlignedByTheAnalyzersMeasureBehindNoBreakSpaces (the RH5202 fix's output stays) | FMT-RAW-001 (SHARED-FIX-007 conflict) | VerifyFormatter (clean) | red |
| VerifyFormatterKeepsLiteralAlignedByTheAnalyzersMeasureBehindTabs | FMT-RAW-001 | VerifyFormatter (clean) | red |

### Scenarios
| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| Issue input, 4 NBSP | LF, CRLF | formatter | reproduced | 16 spaces + 4 NBSP; expected 12 + 4 |
| Formatted output under RH5202 | LF, CRLF | analyzer parity | reproduced | RH5202 still reports the formatter output |
| RH5202 fix's output (12 + 4 NBSP, 12 + 4 tabs) | LF, CRLF | formatter | reproduced | moved to 16 + 4; expected unchanged |

### Not covered, and why
- The RH5202 code fix itself is not re-tested: the run does not change it (skeptic: must stay). Its existing tests stay. The two "keeps" tests above pin the fix-versus-formatter agreement from the formatter side.

## Whole run

### Finding against the spec examples (for the lead)
The FMT-INDENT-013 example in `sdd/specs/formatter/indentation.md` and the "after" block of the change doc `sdd/changes/2026-11/raw-string-tab-one-column.md` (visible output change 2) show `enum E : System. // c⏎Int32⏎{⏎A,⏎}` with the trailing comma kept in the output. Whole-document formatting removes the comma after an enum's last member (FMT-STRUCT-019; observed: `A,` becomes `A`). Taken verbatim, the example cannot pass for a reason unrelated to FMT-INDENT-013. My test uses the example without the trailing comma. The example and the change doc's after block need `A` without a comma, or a second member.

### Commands
```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~LoneOperatorJoinBlankLineTests|FullyQualifiedName~IndexerParameterListLayoutTests|FullyQualifiedName~CompoundAssignmentLayoutTests|FullyQualifiedName~EnumMemberValueLayoutTests|FullyQualifiedName~CollectionExpressionClosingBracketCommentTests|FullyQualifiedName~BaseListContinuationAlignmentTests|FullyQualifiedName~EmptyWrappedListClosingDelimiterTests|FullyQualifiedName~RawStringOtherLineTerminatorAlignmentTests|FullyQualifiedName~DirectiveBehindDisabledTextTests|FullyQualifiedName~RawStringClosingWhitespaceMeasureTests|FullyQualifiedName~OmittedTypeArgumentBehindCommentTakesBlockColumn"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5111|FullyQualifiedName~RH5202"
```

At `8432ef4` plus these tests: formatter filter 75 failed, 23 passed (98). Analyzer filter 9 failed, 190 passed (every failure is a new test). `scripts/test.sh --project architecture`: 115 passed. All test files were formatted with `scripts/format.sh`.

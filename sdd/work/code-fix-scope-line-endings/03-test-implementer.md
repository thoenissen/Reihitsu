Role: test implementer
Commit: 8eb6056
Verdict: #1283: reproduced, tests written; #1284: reproduced, tests written; #1285: tests written

The prompt carried no intended fix, suspected cause, or hoped-for verdict. This handover states none.

Every expected value comes from a spec example, a requirement sentence, or the maintainer's answer recorded in `sdd/changes/2026-11/rh3001-fix-scope.md`. None was taken from a run of the code. Each new or rewritten test was run on 8eb6056 with the code unchanged. Every red test fails on the difference the requirement names; the diffs are quoted below. No production code was touched.

## thoenissen/Reihitsu#1283

### Tests

File: `src/Reihitsu.Analyzer.Test/Clarity/RH3006ConditionalExpressionsMustDeclarePrecedenceAnalyzerTests.cs`. Every test runs twice, with `DataRow("\n")` and `DataRow("\r\n")`. Most use `VerifyCodeFixWithEndOfLine`, which runs the framework verifier (an exact text compare, line endings included, with the iterative and Fix All paths) and then `ApplyCodeFixAsync` on the raw text.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `FixOfCommentedOperandOnTheNextLineKeepsTheLineBreakInFrontOfIt` (the issue's shape; replaces the characterization test `FixOfOperandOnTheNextLineInCarriageReturnLineFeedDocument`, which was green and asserted the stray LF) | SHARED-FIX-006, RH3006-003; FC-033, FC-034 | `VerifyCodeFixWithEndOfLine` | LF passes, CRLF fails |
| `FixOfOperandBehindALineCommentOnItsOwnLineKeepsTheLineBreaks` | SHARED-FIX-006, RH3006-003; FC-016 | `VerifyCodeFixWithEndOfLine` | LF passes, CRLF fails |
| `FixOfOperandBehindABlankLineAndACommentWritesTheDocumentsLineEnding` | SHARED-FIX-006, SHARED-FIX-008, RH3006-003; FC-016 | `VerifyCodeFixWithEndOfLine` | LF passes, CRLF fails |
| `FixOfCommentedOperandOnTheNextLineKeepsTheLineBreakInFrontOfTheSemicolonLine` | SHARED-FIX-006; FC-016 | `VerifyCodeFixWithEndOfLine` | LF passes, CRLF fails |
| `FixOfCommentedLeftOperandOnTheNextLineKeepsTheLineBreakInFrontOfIt` | SHARED-FIX-006, RH3006-003; FC-016 | `VerifyCodeFixWithEndOfLine` | LF passes, CRLF fails |
| `FixOfCommentedOperandOfAnArgumentOnTheNextLineKeepsTheLineBreakInFrontOfIt` | SHARED-FIX-006, RH3006-003; FC-016 | `VerifyCodeFixWithEndOfLine` | LF passes, CRLF fails |
| `FixOfCommentedPatternOperandOnTheNextLineKeepsTheLineBreakInFrontOfIt` (the `PatternSyntax` branch) | SHARED-FIX-006, RH3006-003; FC-016 | `VerifyCodeFixWithEndOfLine` | LF passes, CRLF fails |
| `FixAllOfTwoCommentedOperandsOnTheirOwnLinesKeepsTheLineBreaks` | SHARED-FIX-006, RH3006-003; FC-008 | `Verify` on CRLF/LF text (Fix All), then `ApplyCodeFixAsync` twice on the raw text | LF passes, CRLF fails |
| `FixOfOperandOnTheNextLineWithoutACommentKeepsTheLineBreakInFrontOfIt` (other side: no comment) | SHARED-FIX-006; FC-003 | `VerifyCodeFixWithEndOfLine` | passes |
| `FixOfOperandBehindALineCommentOnTheOperatorsLineKeepsTheLineBreak` (other side: the comment trails the operator) | SHARED-FIX-006; FC-003, FC-019 | `VerifyCodeFixWithEndOfLine` | passes |

### Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| `return a \|\|` ⏎ `/* x */ b && c;` (the issue's input) | CRLF | RH3006 fix, single and Fix All | reproduced | `return a \|\|<LF>` vs `return a \|\|<CR><LF>` |
| same | LF | RH3006 fix | holds | — |
| own-line `// x` in front of the operand | CRLF | RH3006 fix | reproduced | 2 LF |
| blank line, then `/* x */` | CRLF | RH3006 fix | reproduced | `a \|\|<LF><LF>` (the second LF lies inside the operand's leading trivia) |
| `;` on its own line | CRLF | RH3006 fix | reproduced | 1 LF |
| left operand behind `return` ⏎ | CRLF | RH3006 fix | reproduced | 1 LF |
| left operand of a whole argument behind `(` ⏎ | CRLF | RH3006 fix | reproduced | 1 LF |
| pattern operand behind `or` ⏎ | CRLF | RH3006 fix | reproduced | 1 LF |
| two own-line-commented operands | CRLF | Fix All and the single fix applied twice | reproduced | LF lines |
| no comment; `// x` trailing the operator | CRLF | RH3006 fix | holds | — |

The formatter is not a counterpart surface here: the RH3006 fix does not call it, so no formatter run was made.

### Not covered, and why

- RH3003 (the follow-up candidate in `00-lead.md`): it is outside the run's issues, so no test was written for it.
- A document whose line breaks are LF on a host whose line break is CRLF: these tests run on Linux, where the host line break is LF. The CRLF arm is the one that exposes a host line break written into a document, so it covers the direction CI can see.
- FC-006, FC-022–FC-024, FC-036: the fix only adds parentheses, so these do not apply. FC-010–FC-014: there is no formatter phase. FC-038–FC-043: no unsupported shape is involved.
- Decision 7 dropped the issue's goal of a CRLF check in the fix test base, so no new helper was written. `VerifyCodeFixWithEndOfLine` already asserts the exact text.

## thoenissen/Reihitsu#1284

### Tests

Files under `src/Reihitsu.Analyzer.Test/Formatting/`: `RH3202…AnalyzerTests.cs`, `RH3203…`, `RH3205…`, `RH3206…`, `RH3207…`, `RH3208…`.

Several existing tests asserted outputs that SHARED-FIX-006 or SHARED-BODY-003 now forbid: tests marked "Characterizes" and tests whose expected value formatted the member. Each was rewritten to the specified output, and the table says so. The input of each rewritten test is unchanged.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH3202 `FixKeepsTheSpacingOfTheSignature` (the SHARED-BODY-003 example) | SHARED-BODY-003, SHARED-FIX-006; FC-030, FC-017 | `VerifyCodeFixWithEndOfLine`, LF and CRLF | fails |
| RH3202 `FixKeepsTheDocumentationCommentAndTheAttributeOnTheMethodsLine` | SHARED-BODY-003, SHARED-FIX-006; FC-030 | `Verify` | fails |
| RH3202 `FixLeavesNoWhitespaceBehindTheSignatureWhenSeveralSpacesStoodInFrontOfTheArrow` | SHARED-BODY-003 | `Verify` (exact text, trailing whitespace included) | fails |
| RH3202 `FixKeepsABlockCommentBehindTheSignatureOnItsLine` (`( ) /* c */   =>`) | SHARED-BODY-003; FC-021 | `Verify` | fails |
| RH3202 `FixKeepsALineCommentBehindTheSignatureWhenTheArrowStartsTheNextLine` | SHARED-BODY-003; FC-021, FC-027 | `Verify` | fails |
| RH3202 `FixKeepsTheSpacingOfTheSignatureWhenTheArrowStartsTheNextLine` | SHARED-BODY-003; FC-027 | `Verify` | fails |
| RH3202 `FixKeepsAConstraintClauseAsWritten` | SHARED-BODY-003; FC-027 | `Verify` | fails |
| RH3202 `FixKeepsTheSpacingOfTheSignatureOfAnAsyncMethod` | SHARED-BODY-003; FC-025 | `Verify` | fails |
| RH3202 `FixAllKeepsTheSignaturesOfTwoMethods` (two blank lines between them) | SHARED-BODY-003, SHARED-FIX-006; FC-008 | `Verify`, LF and CRLF | fails |
| RH3202 `FixIsOfferedWhenAnotherMemberHoldsACommentTheFormatterRefuses` (other side of FMT-UNSUP-004: the shape is in a field, not in the converted method) | SHARED-FIX-010; FC-043, FC-003 | `Verify` | fails today only on the signature spacing; the fix is offered |
| RH3202 `NoFixIsOfferedForAMethodWhoseBodyHoldsACommentTheFormatterRefuses` | SHARED-FIX-010; FC-043 | `VerifyNoCodeFixIsOffered` | passes |
| RH3202 `VerifyFixKeepsTheDocumentationCommentAndChangesOnlyTheLayoutOfRawStringLiterals` (rewritten from `VerifyFixChangesOnlyTheLayoutOfDocumentationCommentsAndRawStringLiterals`: the `///` comment now stays as written) | SHARED-FIX-003, SHARED-FIX-006 | `Verify` | fails |
| RH3202 `VerifyFixKeepsTwoBlankLinesAboveTheMethod` (the SHARED-FIX-003 layout example; replaces the characterization test `VerifyFixCollapsesTwoBlankLinesAboveTheMethod`) | SHARED-FIX-003, SHARED-FIX-006, RH3202-002 | `Verify` | fails |
| RH3202 `VerifyFixKeepsTwoBlankLinesBelowTheMethod` | SHARED-FIX-006, RH3202-002; FC-003 | `Verify` | passes |
| RH3202 `VerifyFixKeepsTheTabsInFrontOfTheSignatureOfAMethodIndentedWithTabs` (rewritten from `VerifyFixLaysOutMethodIndentedWithTabsFromTheColumnTheTabsStandFor`; tags FMT-PIPE-017 and FMT-TAB-001 dropped, both are covered elsewhere) | SHARED-FIX-006, SHARED-BODY-003 | `Verify` | fails (see *Open point* below) |
| RH3202 `VerifyFixMovesRawStringOfMethodIndentedWithTabsUnderTheOpeningQuotes` (expected signature line now keeps `\t\t`) | FMT-PIPE-017, FMT-RAW-001 (unchanged tags) | `Verify` | fails |
| RH3202 `VerifyFixKeepsTwoDelimitedDocumentationCommentsBehindPreviousMember` (rewritten from `VerifyFixMovesFirstOfTwoDelimitedDocumentationCommentsBehindPreviousMember`: the comments stay behind `_a;` and no blank line is inserted) | SHARED-FIX-006 | `Verify`, LF and CRLF | fails |
| RH3203 `VerifyFixFormatsOnlyTheReplacedBody` (the SHARED-FIX-006 RH3203 example; replaces the characterization test `VerifyFixLaysOutTheWholeConstructor`) | SHARED-FIX-006, SHARED-BODY-003; FC-030 | `VerifyCodeFixWithEndOfLine`, LF and CRLF | fails |
| RH3203 `VerifyFixKeepsTheConstructorInitializerOnTheSignaturesLine` (replaces the characterization test `VerifyFixMovesTheConstructorInitializerOntoItsOwnLine`) | SHARED-BODY-003; FC-027 | `Verify` | fails |
| RH3203 `VerifyFixKeepsTheSpacingOfTheConstructorInitializer` (`:  base( value )`) | SHARED-BODY-003; FC-027 | `Verify` | fails |
| RH3203 `VerifyFixKeepsAConstructorInitializerOnTheLineBelowTheSignature` | SHARED-BODY-003; FC-027 | `Verify` | fails |
| RH3203 `VerifyFixKeepsTwoBlankLinesAboveTheConstructor` (replaces the characterization test `VerifyFixCollapsesTwoBlankLinesAboveTheConstructor`) | SHARED-FIX-006 | `Verify` | fails |
| RH3203 `VerifyFixAllKeepsTheSignaturesOfTwoConstructors` | SHARED-BODY-003; FC-008 | `Verify` | fails |
| RH3205 `VerifyFixKeepsTheSpacingOfTheSignature` | SHARED-BODY-003; FC-025 | `Verify` | fails |
| RH3206 `VerifyFixKeepsTheSpacingOfTheSignature` | SHARED-BODY-003; FC-025 | `Verify` | fails |
| RH3207 `VerifyFixKeepsTheSpacingOfTheSignature` (with a `///` comment) | SHARED-BODY-003; FC-025 | `Verify` | fails |
| RH3208 `VerifyFixKeepsTheSpacingOfTheSignature` (the containing method's `int  Run( int a )` and `int  x = a;` stay as well) | SHARED-BODY-003; FC-025 | `Verify` | fails |
| RH3208 `VerifyFixKeepsTheSpacingOfTheSignatureOfAnAsyncLocalFunction` | SHARED-BODY-003; FC-025 | `Verify` | fails |

Observed versus expected on 8eb6056, one line per class of failure:

- signature `int  GetValue( )` becomes `int GetValue()`, and likewise for every kind;
- `///<summary>x</summary>` becomes three lines;
- `[Obsolete] public  C( …)` is split onto two lines;
- `: this(…)` and `:  base( value )` move onto their own line;
- `where T : class` moves onto its own line;
- two blank lines collapse to one;
- `\t\tint Get()` becomes 8 spaces;
- `/** a */ /** b */` moves off `_a;`'s line behind a new blank line.

### Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| SHARED-BODY-003 RH3202 example | LF, CRLF | RH3202 fix | reproduced | `public int GetValue()` vs `public int  GetValue( )` |
| SHARED-FIX-006 RH3203 example | LF, CRLF | RH3203 fix | reproduced | doc comment and attribute reformatted |
| SHARED-FIX-003 layout example (two blank lines) | LF | RH3202 fix | reproduced | one blank line vs two |
| RH3205, RH3206, RH3207, RH3208 signatures | LF | each fix | reproduced | signature spacing normalized |
| Fix All, two members | LF, CRLF (RH3202) | Fix All | reproduced | both signatures normalized |

Counterpart surface: the formatter is meant to normalize these signatures when it formats a whole document. That is FMT-STRUCT-004/-005 and the layout phases, which are not under test here.

### Open point for the lead (not decided here)

`VerifyFixKeepsTheTabsInFrontOfTheSignatureOfAMethodIndentedWithTabs` asserts this output: the signature line keeps `\t\t` (SHARED-FIX-006 and SHARED-BODY-003: the rest of the member stays as written), and the new block is written with spaces in column 8, the column the formatter's layout gives it when tabs count four columns each. The same holds for the raw-string tab test.

SHARED-FIX-006 says "with the whitespace of the lines it edits". A second reading of that phrase would write the block's lines with the edited line's `\t\t`. That reading gives a different output for the same input.

The input is rare under *Code-fix coverage*, because RH5601 reports tab indentation. If the lead takes the second reading, or decides to withhold the fix for tab-indented members, these two expected values change.

### Not covered, and why

- RH3204 and the property, indexer, and accessor kinds of FC-025: they are not reported by these six rules and are outside SHARED-BODY.
- FC-040 (documentation mode): no unsupported shape is declared in this run. The FMT-UNSUP-004 refusal is tested in the default parse mode, as in the existing RH3208 test.
- The FMT-BREAK-018 tests and the shared-line tests in RH3202, and the labeled-function tests in RH3208, were left unchanged. Their signatures are already canonical, so their expected values follow SHARED-FIX-007 and RH3202-002 / RH3208-003, which this run does not change.
- FC-006 and FC-036: the conversion is not a rename, and a compile check is part of `Verify`.

## thoenissen/Reihitsu#1285

### Tests

File: `src/Reihitsu.Analyzer.Test/Clarity/RH3001NotOperatorShouldNotBeUsedAnalyzerTests.cs`.

Eight existing characterization tests asserted outputs that the accepted change doc changes. They were replaced, keeping their inputs:

- six tests that asserted no fix for a whole argument, tuple element, object creation argument, element access argument, attribute argument, or collection element (`…IsReportedWithoutACodeFix`), now `…IsFixed`;
- `FixOfNegationAfterUnsignedRightShiftAssignmentAddsParentheses`;
- `FixOfNegationAfterNullCoalescingAssignmentAddsParentheses`.

The two constant-pattern tests gained the tag RH3001-008.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `NegationsThatAreWholeArgumentsAttributeArgumentsOrElementsAreFixed` (RH3001-004 example 2) | RH3001-004; FC-001 | `Verify` | fails |
| `FixWritesNoParenthesesWhereTheComparisonBindsAsTheNegationDid` (example 3: `??=`, `orderby … descending`, `select`) | RH3001-004 | `Verify` | fails |
| `NegationAsAWholeArgumentIsFixed` | RH3001-004; FC-001 | `Verify` | fails |
| `NegationAsAWholeArgumentNextToAnotherArgumentIsFixed` (`N(!x, y)`) | RH3001-004 | `Verify` | fails |
| `NegationAsAWholeTupleElementIsFixed` | RH3001-004 | `Verify` | fails |
| `NegationAsAWholeObjectCreationArgumentIsFixed` | RH3001-004 | `Verify` | fails |
| `NegationAsAWholeImplicitObjectCreationArgumentIsFixed` (`new(!y)`) | RH3001-004 | `Verify` | fails |
| `NegationAsAWholeElementAccessArgumentIsFixed` | RH3001-004 | `Verify` | fails |
| `NegationAsAWholeConditionalElementAccessArgumentIsFixed` (`o?[!y]`) | RH3001-004 | `Verify` | fails |
| `NegationAsAWholeIndexInAnObjectInitializerIsFixed` (`new E { [!x] = true }`) | RH3001-004 | `Verify` | fails |
| `NegationAsAWholeConstructorInitializerArgumentIsFixed` | RH3001-004 | `Verify` | fails |
| `NegationAsAWholePrimaryConstructorBaseArgumentIsFixed` | RH3001-004 | `Verify` | fails |
| `NegationAsAWholeAttributeArgumentIsFixed` (the result is compiled by `Verify`) | RH3001-004; FC-006 | `Verify` | fails |
| `NegationAsAWholeCollectionExpressionElementIsFixed` | RH3001-004 | `Verify` | fails |
| `NegationAsAWholeOrderByKeyWithoutADirectionIsFixed` (the maintainer's answer, batch 1 Q1 A) | RH3001-004 | `Verify` | fails |
| `FixOfNegationAfterUnsignedRightShiftAssignmentAddsNoParentheses` | RH3001-004 | `Verify` | fails |
| `FixOfNegationAfterNullCoalescingAssignmentAddsNoParentheses` | RH3001-004 | `Verify` | fails |
| `FixWritesNoParenthesesInTheElementAndTheKeyOfAGroupClause` | RH3001-004 | `Verify` | fails |
| `FixWritesNoParenthesesInANamedAttributeArgument` (`P =` and `b:`) | RH3001-004 | `Verify` | fails |
| `FixWritesNoParenthesesBehindGotoCase` | RH3001-004 | `Verify` | fails |
| `NegationAsTheLeftOperandOfIsInAnArgumentIsParenthesized` (other side of the MUST NOT: binding differs, so parentheses stay) | RH3001-004; FC-003 | `Verify` | passes |
| `NegationAsAWholeNamedArgumentIsFixed` (`M(b: !x)`, regression) | RH3001-004; FC-003 | `Verify` | passes |
| `CommentAfterTheOperatorOfAWholeArgumentMovesInFrontOfTheOperand` | RH3001-004, RH3001-005; FC-019 | `Verify` | fails (no fix today) |
| `DirectiveAfterTheOperatorOfAWholeArgumentIsReportedWithoutACodeFix` | RH3001-006; FC-019, FC-020 | `VerifyNoCodeFixIsOffered` | passes |
| `FixOfACommentedWholeArgumentOnTheNextLineKeepsTheLineBreaks` | RH3001-004, SHARED-FIX-006; FC-017, FC-033 | `VerifyCodeFixWithEndOfLine`, LF and CRLF | fails (no fix today) |
| `FixAllFixesTwoWholeArgumentsOfOneInvocation` (`N(!x, !y)`) | RH3001-004; FC-008 | `Verify` | fails |
| `FixOfANestedNegationInAWholeArgumentFixesBoth` (`M(!(!x))` → `M((x == false) == false)`, two Fix All iterations) | RH3001-004; FC-008 | `Verify` | fails |
| `NegationAsTheValueOfACaseLabelWithoutAWhenClauseIsFixed` (`case true == false:`) | RH3001-008; FC-003 | `Verify` | passes |
| `NegationAsTheConstantPatternOfACaseLabelWithAWhenClauseIsReportedWithoutACodeFix` | RH3001-008; FC-003 | `VerifyNoCodeFixIsOffered` | passes |
| `NegationsThatAreConstantPatternsAreReportedWithoutACodeFix` (RH3001-008 example 1) | RH3001-008 | `VerifyNoCodeFixIsOffered` | passes |
| `NegationBehindTheArrowOfASwitchExpressionArmIsFixed` (RH3001-008 example 2) | RH3001-008; FC-003 | `Verify` | passes |
| `NegationAsTheConstantPatternOfAPropertySubpatternIsReportedWithoutACodeFix` | RH3001-008 | `VerifyNoCodeFixIsOffered` | passes |
| `NegationAsTheConstantPatternOfAPositionalSubpatternIsReportedWithoutACodeFix` | RH3001-008; FC-002 | `VerifyNoCodeFixIsOffered` | passes |
| `NegationAsTheConstantPatternOfAListPatternElementIsReportedWithoutACodeFix` | RH3001-008; FC-002 | `VerifyNoCodeFixIsOffered` | passes |
| `NegationAsTheConstantPatternOfALogicalPatternIsReportedWithoutACodeFix` (`not !true`, `!true or false`) | RH3001-008; FC-002 | `VerifyNoCodeFixIsOffered` | passes |
| `NegationAsAWholeIsPatternIsReportedWithoutACodeFix` (existing; RH3001-008 tag added) | SHARED-DIAG-011, SHARED-FIX-003, RH3001-008 | `VerifyNoCodeFixIsOffered` | passes |
| `NegationAsAWholeSwitchArmPatternIsReportedWithoutACodeFix` (existing; RH3001-008 tag added) | RH3001-008 | `VerifyNoCodeFixIsOffered` | passes |

The red tests fail on the difference the change doc names. Either no fix is offered today, so the source stays (for example `Base(!x)` vs `Base(x == false)`), or the comparison is put in parentheses (for example `goto case (true == false);` vs `goto case true == false;`).

The constant-pattern tests and the `case` tests pass on the unchanged code, as they must: RH3001-008 and the `case !true:` boundary keep today's output.

### Not covered, and why

- `B >>>= !x` in a whole-argument position, and `orderby !a ascending`: these reach the same parent-binding decision as the tested `>>>=` and `orderby … descending`. They differ only in the token, so they have no separate test. This is a judgment, not a static proof.
- The RH3001-007 interplay with whole arguments: the existing RH3001-007 tests (for example `M(G < A, B > !x switch …)`) keep their withheld fix. They were run green after the edit.
- The trailing-space case where `!` ends a line (decision 10): dropped by the lead, so no test.
- FC-010–FC-014 and FC-038–FC-043: the formatter is not involved.

## Whole run

- Lane check: not run. Only test files and this handover changed.
- `scripts/format.sh` over the eight changed test files: formatted 0 of 8.
- `scripts/test.sh --project architecture`: 117 passed. `RequirementCoverageTests` now finds RH3001-008 covered.
- Focused runs on 8eb6056 with the code unchanged:
  - RH3006 class: 35 tests, 8 failed (the CRLF arms listed above);
  - RH3001 class: 66 tests, 25 failed;
  - RH3202–RH3208 classes: 190 tests, 29 failed.

  Every failure is listed above, and every other test passes.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH3006ConditionalExpressionsMustDeclarePrecedenceAnalyzerTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH3001NotOperatorShouldNotBeUsedAnalyzerTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH3202ExpressionStyle|FullyQualifiedName~RH3203ExpressionStyle|FullyQualifiedName~RH3205ExpressionStyle|FullyQualifiedName~RH3206ExpressionStyle|FullyQualifiedName~RH3207ExpressionStyle|FullyQualifiedName~RH3208ExpressionStyle"
scripts/test.sh --project architecture
```

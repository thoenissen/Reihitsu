Role: test implementer
Commit: 3ab1917
Verdict: #1147: tests written; #1152: tests written; #1386: tests written; #1387: tests written; #1594: tests written; #1655: tests written (characterization only); #1736: tests written

My prompt carried no intended fix, suspected cause, or hoped-for verdict. Expected values come from the spec examples and requirements on this branch (4d1f11b). Where a requirement fixes no exact output — the layout a newly reported comment-led line gets from a fix — I wrote the formatter's output by hand from SHARED-COMMENT-011 (comment moved into the column, token keeps its distance) and confirmed with `scripts/format.sh` on unchanged `main` code that the formatter writes exactly that shape. No value comes from patched code (FC-035).

Every test is in `src/Reihitsu.Analyzer.Test`: `Formatting/<rule>…AnalyzerTests.cs` (analyzer and fix) and `Formatter/Formatting/<rule>…FormatterTests.cs` (`VerifyFormatter(source)`: not reported, unchanged by the formatter, analyzer-clean after a second pass, under LF and CRLF). "Expected now" is the result on this head with unchanged production code: **red** fails for the reason given, **green** is a characterization that must stay green.

A full analyzer run on this head: 119 failing cases (82 test names), every one of them a test listed here as red; no pre-existing test fails. Architecture tests: 118/118 pass.

## thoenissen/Reihitsu#1147

### Tests
Notation: `(F)` is the formatter test class of the rule; `bad` tests take `endOfLine` (`\n`, `\r\n`) and use `VerifyCodeFixIsWithheldOrProducesOneOf(…, mayBeWithheld: true, <formatter layout>)`: reported where the rule locates it, and the fix is either withheld or writes exactly the line with the comment in the right column, on which nothing is reported (FC-007, FC-009).

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH5110 (F) `VerifyConstraintBehindLeadingCommentInConstraintColumnIsNotReported` | RH5110-001, SHARED-COMMENT-011 | `VerifyFormatter(source)` | red: reported |
| RH5110 `VerifyConstraintBehindMisplacedLeadingCommentIsReported` | RH5110-001, SHARED-COMMENT-011 | withheld-or-one-of | red: not reported |
| RH5203 (F) `VerifyArgumentBehindLeadingCommentInAlignmentColumnIsNotReported` | RH5203-001, SHARED-COMMENT-011 | `VerifyFormatter` | red: reported |
| RH5203 `VerifyArgumentBehindMisplacedLeadingCommentIsReported` | RH5203-001, SHARED-COMMENT-011 | withheld-or-one-of | red: not reported |
| RH5205 (F) `VerifyBraceBehindLeadingCommentInLambdaColumnIsNotReported` | RH5205-001, SHARED-COMMENT-011 | `VerifyFormatter` | red: reported |
| RH5205 `VerifyBraceBehindMisplacedLeadingCommentIsReported` | RH5205-001, SHARED-COMMENT-011 | withheld-or-one-of | red: not reported |
| RH5206 (F) `VerifyArmBehindLeadingCommentInArmColumnIsNotReported` | RH5206-001, SHARED-COMMENT-011 | `VerifyFormatter` | red: reported |
| RH5206 (F) `VerifyOpeningBraceBehindLeadingCommentInAnchorColumnIsNotReported` | RH5206-001, SHARED-COMMENT-011 | `VerifyFormatter` | red: reported |
| RH5206 `VerifyArmBehindMisplacedLeadingCommentIsReported` | RH5206-001, SHARED-COMMENT-011 | withheld-or-one-of | red: not reported |
| RH5206 `VerifyOpeningBraceBehindMisplacedLeadingCommentIsReported` | RH5206-001, SHARED-COMMENT-011 | withheld-or-one-of | red: not reported |
| RH5302 (F) `VerifyOperatorBehindLeadingCommentInOperatorColumnIsNotReported` | RH5302-002, SHARED-COMMENT-011 | `VerifyFormatter` | red: reported |
| RH5302 `VerifyOperatorBehindMisplacedLeadingCommentIsReported` | RH5302-002, SHARED-COMMENT-011 | withheld-or-one-of | red: not reported |
| RH5308 (F) `VerifyOperatorBehindLeadingCommentInOperatorColumnIsNotReported` | RH5308-002, SHARED-COMMENT-011 | `VerifyFormatter` | red: reported |
| RH5308 `VerifyOperatorBehindMisplacedLeadingCommentIsReported` | RH5308-002, SHARED-COMMENT-011 | withheld-or-one-of | red: not reported |
| RH5305 (F) `VerifyClosingBracketBehindLeadingCommentInBracketColumnIsNotReported` | RH5305-001, SHARED-COMMENT-011 | `VerifyFormatter` | red: reported |
| RH5305 `VerifyClosingBracketBehindMisplacedLeadingCommentIsNotReported` | RH5305-001 | `Verify(source)` LF/CRLF | green (see *Not covered*) |
| RH5306 (F) `VerifyClosingBracketBehindLeadingCommentInBracketColumnIsNotReported` | RH5306-001, SHARED-COMMENT-011 | `VerifyFormatter` | green |
| RH5306 `VerifyClosingBracketBehindMisplacedLeadingCommentIsNotReported` | RH5306-001 | `Verify(source)` LF/CRLF | green (see *Not covered*) |
| RH5301 (F) `VerifyMemberBehindTwoLeadingCommentsInMemberColumnIsNotReported` (spec example) | SHARED-INIT-001, RH5301-002 | `VerifyFormatter` | red: reported |
| RH5301 (F) `VerifyClosingBraceBehindTwoLeadingCommentsInBraceColumnIsNotReported` | SHARED-INIT-001, RH5301-001 | `VerifyFormatter` | red: reported |
| RH5301 (F) `VerifyMemberBehindCommentTailAndFurtherCommentIsNotReported` (`/* a⏎ */ /* b */ P = 1`) | SHARED-INIT-001, RH5301-002 | `VerifyFormatter` | red: reported |
| RH5301 `VerifyMemberBehindTwoMisplacedLeadingCommentsIsReported` (spec example) | SHARED-INIT-001, RH5301-002 | withheld-or-one-of | red: the fix writes the accepted layout, which is still reported (FC-007) |
| RH5303 (F) `VerifyElementBehindTwoLeadingCommentsInElementColumnIsNotReported` | SHARED-INIT-001, RH5303-002 | `VerifyFormatter` | red: reported |
| RH5303 `VerifyElementBehindTwoMisplacedLeadingCommentsIsReported` | SHARED-INIT-001, RH5303-002 | withheld-or-one-of | red: fix result still reported |
| RH5304 (F) `VerifyElementBehindTwoLeadingCommentsInElementColumnIsNotReported` | SHARED-INIT-001, RH5304-002 | `VerifyFormatter` | red: reported |
| RH5304 `VerifyElementBehindTwoMisplacedLeadingCommentsIsReported` | SHARED-INIT-001, RH5304-002 | withheld-or-one-of | red: fix result still reported |

### Not covered, and why
- **RH5305 and RH5306, misplaced comment in front of the closing bracket: these tests assert "not reported", not "reported" as the skeptic's table and decision 5 expected.** RH5305-001 reports a collection expression "whose closing bracket has no comment in front of it on the bracket's line". RH5306-001 reports only "provided that no comment … sits inside the list pattern". SHARED-COMMENT-011 says "a requirement that keeps the rule from reporting because a comment stands in that place still applies". So both rules' own exemptions leave `/* end */ ];` unreported in any column. Both tests are green today. If the lead reads the specs as demanding a report, the specs have to change first: RH5305-001's wording was left alone on purpose (01-spec-author). RH5306's only failing member on the skeptic's list is this one. Under the spec as written, RH5306 has **no** failing member, and its correct-column formatter test is green too.
- RH5112: exempt under SHARED-CHAIN-004. RH5201 and RH5204 already hold (skeptic). Neither is touched.
- The fixes of the newly reported misplaced lines: no requirement fixes their output. The tests accept a withheld fix or exactly the formatter's layout, so a fix that does something else fails.
- A multi-line comment whose tail leads the token's line, for the column rules other than the initializers: SHARED-COMMENT-012 / P33 cases are unchanged by this run and not in the issue.
- FC-019/FC-020 (directive in the gap): the change is about which element starts a line, not about joining a gap. A directive never leads a code line in front of a token.

## thoenissen/Reihitsu#1152

### Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| 8 existing RH5204 tests whose markup asserted today's span (`///{|#0: Doc⏎|}`, `/**{|#0: Doc */|}`), rewritten to `{|#0:/// Doc|}` / `{|#0:/** Doc */|}` and newly tagged: `VerifyDelimitedDocumentationCommentOnOwnLineBelowOpeningBraceIsDetectedAndFixed`, `…WithCarriageReturnLineFeed`, `VerifyOverIndentedDelimitedDocumentationCommentIsDetectedAndFixed`, `VerifySingleLineDocumentationCommentOnOwnLineIsDetectedAndFixed`, `VerifyMultiLineSingleLineDocumentationCommentIsDetectedAndFixed`, `VerifyDelimitedDocumentationCommentBeforeStatementIsDetectedAndFixed`, `VerifyDocumentationCommentCodeFixDoesNotFormatIndependentMethod`, `VerifyMixedMisindentedCommentKindsAreDetectedAndFixed` | RH5204-005 | `Verify(source, fixed, …)` | red: diagnostic starts 3 columns late |
| `VerifyDocumentationCommentDiagnosticStartsAtExteriorAndExcludesLineBreak` (spec example) | RH5204-005 | `VerifyCodeFixWithEndOfLine` LF/CRLF | red: starts at column 10, not 7 |
| `VerifyDocumentationCommentDiagnosticSpanIsTheSameWithoutDocumentationParsing` | RH5204-005 | `Verify` in `DocumentationMode.None`, LF/CRLF | green (FC-040 guard) |
| `VerifyMultiLineDocumentationCommentDiagnosticEndsAtLastLineText` (`///` run with a differently indented middle line) | RH5204-005, RH5204-006 | `VerifyCodeFixWithEndOfLine` LF/CRLF | red: start column |
| `VerifyMultiLineDelimitedDocumentationCommentDiagnosticStartsAtOpeningDelimiter` | RH5204-005 | `Verify` in Parse and None mode, LF/CRLF | red: start column (Parse) |

### Not covered, and why
- The fix keeps working against the new start: the rewritten fix tests and the multi-line `///` test apply the fix through `FindTrivia(location.Start)`, under LF and CRLF.
- **Follow-up candidate (found while writing the tests, outside the run):** RH5204 does not check a member line whose first token is a predefined type keyword with no modifier, such as `  bool Value { get; }` misindented inside a class. It reports nothing there, and it reports nothing for a comment above such a line either. This holds on unchanged `main` (`scripts/apply-fix.sh RH5204` gives `no-diagnostic`; with `internal bool Value` the same fixtures are fixed). RH5204-001 lists "a keyword … that starts … a member", and RH5204-006's own spec example uses `bool Value { get; }`. My tests use `internal bool Value` so that they test the span and not this gap.

## thoenissen/Reihitsu#1386

### Tests
For each rule RH5033, RH5035–RH5049, a `///` test (`VerifyTripleSlash…IsReported`) and a `/** */` test (`VerifyDelimitedDocumentationStyle…IsReported`). Each one copies the rule's existing `//` own-line test in the same position and swaps only the comment. It asserts the same diagnostic in `DocumentationMode.Parse` and in `None`. All are tagged SHARED-COMMENT-001 and the rule's requirement of the copied test (RH5033-001, RH5035-001, RH5036-001, RH5037-001, RH5038-001, RH5039-001, RH5040-001, RH5041-001, RH5042-001, RH5043-001, RH5044-001, RH5045-001, RH5046-001, RH5047-002, RH5048-001, RH5049-001). All 32 are red: `expected 1 actual 0` in Parse mode.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| 32 per-rule tests above | SHARED-COMMENT-001, rule ID | `Verify(source, Parse/None transform, Diagnostics)` | red: not reported |
| RH5033 `VerifyTripleSlashSummaryBetweenAttributeAndMethodIsReported`, `VerifyDelimitedDocumentationStyleSummaryBetweenAttributeAndMethodIsReported` (issue inputs 1–2) | SHARED-COMMENT-001, RH5033-001 | same | red |
| RH5034 `VerifyNoDiagnosticForCommentBetweenDetachedAndLaterDocumentationComment` (spec example, issue input 4) | SHARED-COMMENT-001, RH5034-001 | `Verify` Parse and None | red: `// note` reported |
| RH5034 `VerifyNoDiagnosticForDelimitedCommentBetweenDetachedAndLaterDocumentationComment` | SHARED-COMMENT-001, RH5034-001 | same | red |
| RH5034 `VerifyNoDiagnosticForCommentBehindDirectiveBetweenDetachedAndLaterDocumentationComment` (`/// a⏎#pragma…⏎// note⏎/// b`) | SHARED-COMMENT-001, RH5034-001 | same | red |
| RH5034 `VerifyCommentBehindDocumentationCommentsSeparatedByBlankLineIsReported` (blank line does not detach) | SHARED-COMMENT-001, RH5034-001 | same | green |

### Not covered, and why
- Location: the markup asserts the comment's text from `///`/`/**` to its end without the line break, in both modes. This is my reading of "located on that comment" (SHARED-COMMENT-003), the span RH5204-005 now states for the same trivia. In None mode it is today's span. In Parse mode it is the span the implementer has to produce.
- The "same shapes with `//` are unchanged" goal: each per-rule test's `//` original stays in the suite, unchanged and green.

## thoenissen/Reihitsu#1387

### Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH5506 `VerifyDiagnosticWithoutCodeFixWhenCommentOrDirectiveIsInsideTheList` (existing, rewritten to the new spec example: comment, `#pragma` between attributes, `#if` inside an argument) | RH5506-004, SHARED-ATTR-019 | `VerifyNoCodeFixIsOffered` | green |
| RH5506 `VerifyNoDiagnosticWhenConditionalDirectiveIsBetweenTheAttributes` (spec example) | RH5506-004, SHARED-ATTR-019 | `Verify` LF/CRLF | red |
| RH5506 `VerifyNoDiagnosticWhenDisabledTextHoldsAnAttributeOfTheList` (`#if false … #else … #endif`) | RH5506-004, SHARED-ATTR-019 | `Verify` LF/CRLF | red |
| RH5506 `VerifyDiagnosticWithoutCodeFixWhenRegionDirectiveIsBetweenTheAttributes` | RH5506-004, SHARED-ATTR-019 | `VerifyNoCodeFixIsOffered` | green |
| RH5506 `VerifyDiagnosticWhenConditionalDirectiveIsInFrontOfTheList` (boundary: outside the span) | RH5506-004, SHARED-ATTR-019 | `Verify` LF/CRLF | green |
| RH5514 `VerifyNoDiagnosticWhenConditionalDirectiveIsBetweenTheAttributes` | SHARED-ATTR-019 | `Verify` LF/CRLF | red |
| RH5531 `VerifyNoDiagnosticWhenConditionalDirectiveIsBetweenTheAttributes` | SHARED-ATTR-019 | `Verify` LF/CRLF | red |
| RH5524 `VerifyNoDiagnosticWhenConditionalDirectiveIsBetweenTheLists` (spec example) | RH5524-004, SHARED-ATTR-020 | `Verify` LF/CRLF | red |
| RH5524 `VerifyListInFrontOfConditionalDirectiveIsStillReported` (`[A] [B]⏎#if⏎[C1]⏎#endif⏎[D]`: `[B]` reported, `[D]` not) | RH5524-004, SHARED-ATTR-020 | `Verify` LF/CRLF | red: `[D]` reported too |
| RH5529 `VerifyNoDiagnosticWhenConditionalDirectiveIsBetweenTheLists` | SHARED-ATTR-020 | `Verify` LF/CRLF | red |
| RH5102 `VerifyConditionalDirectiveExcludingAnArgumentKeepsTheListFromBeingReported` (issue input) | RH5102-004, SHARED-DIAG-008 | `Verify` LF/CRLF | green |
| RH5109 `VerifyConditionalDirectiveExcludingAParameterKeepsTheListFromBeingReported` (issue input) | RH5109-004, SHARED-DIAG-008 | `Verify` LF/CRLF | green |

### Not covered, and why
- The other ten one-attribute-per-list rules (RH5502, RH5504, RH5508, RH5510, RH5512, RH5516, RH5518, RH5520, RH5522, RH5526, RH5528) are not tested one by one. I covered both report branches of `TargetAttributeListShapeAnalyzerBase.OnAttributeList` (private, the only report site: `rg -n "ReportDiagnostic" src/Reihitsu.Analyzer/Base/TargetAttributeListShapeAnalyzerBase.cs`). The split branch is covered through RH5506, RH5514, and RH5531, the merge branch through RH5524 and RH5529. I also covered both overriding rules: `rg -n "override (TargetAttributeListShapeMode ResolveListShapeMode|bool IsAttributeListInScope)" src/Reihitsu.Analyzer/Rules` finds only RH5513, RH5514, RH5530, and RH5531, and RH5513/RH5530 are placement rules. The untested rules differ only in constructor arguments (target, mode). The role file does not accept a base class with overrides as proof of one call site. This is a deliberate gap the lead may close: one copy per rule of the RH5506 conditional test.
- The RH5111 conditional instance already has tests on `main` (`VerifyDeclaratorWithConditionalDirectiveInFrontOfItsEqualsSignIsNotReported` and two neighbors). RH5102's and RH5109's `#pragma` variants are covered by the existing `VerifyConditionalDirectiveInAGap…` tests.
- RH3002-009 and RH5408-003: audit only (01-spec-author), no test.

## thoenissen/Reihitsu#1594

### Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH7304 `VerifyDirectivesAreComparedWithTheCommentThatLeadsTheFirstLineOfCode` (spec example: Fields, Values, Names, Others, Lines) | RH7304-001, SHARED-COMMENT-011 | `Verify` LF/CRLF | red: 10 reported instead of 6 |
| RH7304 `VerifyDirectivesAreComparedWithTheFirstOfSeveralCommentsThatLeadTheFirstLineOfCode` | RH7304-001, SHARED-COMMENT-011 | `Verify` LF/CRLF | red: 4 instead of 2 |
| RH7304 `VerifyCommentOnItsOwnLineAboveTheFirstTokenDoesNotChangeTheComparedColumn` | RH7304-001 | `Verify` LF/CRLF | green |
| RH7304 `VerifyFixIndentsDirectivesToTheCommentThatLeadsTheFirstLineOfCode` (spec example of RH7304-004, two regions, Fix All) | RH7304-004 | `VerifyCodeFixWithEndOfLine` | red: fix withheld |
| RH7304 `VerifyFixIndentsDirectivesToAMisplacedCommentThatLeadsTheFirstLineOfCode` (Names → column 6) | RH7304-004 | `VerifyCodeFixWithEndOfLine` | red: fix withheld |
| RH7304 (F) `VerifyCommentLedRegionsAreNotReportedAndStayAsWritten` (one comment, comment tail, two comments) | RH7304-001, SHARED-COMMENT-011 | `VerifyFormatter` | red: 6 reported |

### Not covered, and why
- The issue's literal input (`/* c */ #region`) is invalid C# (CS1040), and no caller reaches it. No test.

## thoenissen/Reihitsu#1655

### Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH5111 `VerifyCommentOrPragmaInEveryWrappedGapKeepsTheReportWithoutCodeFix` (spec example: enum member, property, declarator, `/* c */` assignment, `#pragma` assignment, OQ3 declarator `x`) | RH5111-005, SHARED-DIAG-010 | `VerifyNoCodeFixIsOffered` | green |
| RH5111 `VerifyRegionDirectiveOrCommentInTheWrappedGapKeepsTheReportWithoutCodeFix` (`#region`/`#endregion` in front of `=`, own-line comment behind `=`, parameter default behind a comment) | RH5111-005, SHARED-DIAG-010 | `VerifyNoCodeFixIsOffered` | green |

### Not covered, and why
- Under the maintainer's answers OQ2 B and OQ3 B, RH5111's output on `main` is the specified output. Nothing is red. The issue's enum example stays reported without a fix, which the first test pins. The PR should link the issue with `Refs`, or state that the result is "behavior confirmed, spec aligned".

## thoenissen/Reihitsu#1736

### Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH5109 `VerifyListWrappedOnlyBehindOpeningParenthesisIsNotReported` (change-doc entry 1) | RH5109-001 | `Verify` LF/CRLF | red |
| RH5109 `VerifyListWrappedOnlyInFrontOfClosingParenthesisIsNotReported` (entry 2) | RH5109-001 | `Verify` LF/CRLF | red |
| RH5109 `VerifyListWhoseClosingParenthesisACommentHoldsOnALaterLineIsNotReported` (entry 3) | RH5109-001 | `Verify` LF/CRLF | red |
| RH5109 `VerifyListWhoseFirstParameterACommentKeepsOffTheOpeningParenthesisLineIsNotReported` (entry 4) | RH5109-001 | `Verify` LF/CRLF | red |
| RH5109 `VerifyListWhoseFirstParameterADirectiveKeepsOffTheOpeningParenthesisLineIsNotReported` (entry 4, `#pragma`) | RH5109-001 | `Verify` LF/CRLF | red |
| RH5109 `VerifyListsWrappedOnlyBehindOpeningParenthesisAreNotReportedForEveryOwner` (record primary constructor, delegate, constructor, operator, local function, parenthesized lambda, anonymous method) | RH5109-001 | `Verify` LF/CRLF | red: 7 reported |
| RH5109 `VerifyListWrappedBehindOpeningParenthesisWithASpanningParameterIsReported` (boundary: a spanning parameter makes it multi-line) | RH5109-001 | `Verify` LF/CRLF | green |
| RH5109 `VerifyListWithParametersOnTwoLinesBelowOpeningParenthesisIsFixed` (RH5109-003 example, entry 5; fix result stable under the formatter) | RH5109-001, RH5109-003 | `Verify(source, fixed)` with Fix All, LF/CRLF, plus `AssertFormattingChangesNothing` | green |

### Not covered, and why
- The existing RH5109 tests all use lists whose parameters start on different lines, so none of them has to change (`rg -n "{\|#0:\(\|}" …RH5109…AnalyzerTests.cs`, each checked).
- The single-line list and the single spanning parameter are already covered by `VerifySingleLineParameterListIsNotFlagged` and `VerifyMultiLineSingleParameterIsNotFlagged`.
- The rule page is checked by `AnalyzerPackageMetadataTests`, so no new test is needed.

## Whole run

### Failure classes walked
- FC-003: both sides of every changed condition (correct and wrong column, one comment and two, conditional and `#pragma`/`#region`, inside and outside the judged span, parameters on one line and on two).
- FC-005, FC-020: comment and directive variants side by side for the attribute rules, RH5109, and RH5111.
- FC-007, FC-009: newly reported fixable inputs either withhold the fix or converge in one pass.
- FC-013: the comment-led formatter output is tested not reported through `VerifyFormatter`.
- FC-017: exact spans and exact fix text, never balance checks.
- FC-021: comments on the header line and in front of a closing delimiter.
- FC-027: every RH5109 owner kind.
- FC-033: LF and CRLF for each layout test.
- FC-034: the issues' own inputs.
- FC-040: Parse and None mode for #1152 and #1386.

Not applicable:
- FC-004, FC-008 overlap: no fix in the run joins or moves trivia beyond what already exists.
- FC-010–012: no formatter change.
- FC-018, FC-036: no rename.
- FC-022–026, FC-029: no rewrite of the shapes they concern.
- FC-038–043: no shape is declared unsupported.

### Follow-up candidates
- RH5204 does not check a member line led by a predefined type keyword without a modifier (see #1152).

### Commands
```shell
scripts/test.sh --project analyzer --filter "Name~LeadingComment|Name~TwoLeadingComments|Name~TwoMisplacedLeadingComments|Name~CommentTailAndFurtherComment"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5204IndentationMustUseFourSpacesPerScopeLevelAnalyzerTests"
scripts/test.sh --project analyzer --filter "Name~TripleSlash|Name~DelimitedDocumentationStyle|Name~DetachedAndLaterDocumentationComment|Name~SeparatedByBlankLine"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5506|FullyQualifiedName~RH5514|FullyQualifiedName~RH5531|FullyQualifiedName~RH5524|FullyQualifiedName~RH5529|FullyQualifiedName~RH7304|FullyQualifiedName~RH5109|FullyQualifiedName~RH5102|FullyQualifiedName~RH5111"
scripts/test.sh --project architecture
```

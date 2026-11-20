Role: test implementer
Commit: 4575497
Verdict: tests written

Inputs: thoenissen/Reihitsu#851 (`agent-filed`), `00-lead.md`, `01-spec-author.md`, `02-skeptic.md`, `03-spec-author.md`, change doc `sdd/changes/2026-11/rh5419-indexer-arrow-single-line.md`, spec `sdd/specs/analyzer/RH5419.md` (commit 65f3e42). The prompt carried no intended fix or verdict.

Files:

- `src/Reihitsu.Analyzer.Test/Formatting/RH5419ExpressionStyleGetOnlyIndexersShouldBeSingleLinedAnalyzerTests.cs`: derives from `BatchCodeFixTestsBase<RH5419ExpressionStyleGetOnlyIndexersShouldBeSingleLinedAnalyzer, RH5419ExpressionStyleGetOnlyIndexersShouldBeSingleLinedCodeFixProvider>`. These are the spec's code owners, so the provider must return a Fix All provider.
- `src/Reihitsu.Analyzer.Test/Formatter/Formatting/RH5419ExpressionStyleGetOnlyIndexersShouldBeSingleLinedFormatterTests.cs`: `FormatterTestsBase<RH5419...Analyzer>`.

**Expected now:** the analyzer test project does not compile. The only errors are CS0246 for the two missing types. Neither the rule nor its resource strings exist yet.

**How the tests were checked:** I wrote a throwaway stub in the working tree and deleted it before committing; none of it is committed. It had the analyzer, the `.resx` strings, and a provider that joins the parameter list, formats, and copies RH5401's attribute-list guards. All 86 RH5419 test cases passed against it, and so did `scripts/test.sh --project architecture` (198/198, `EveryRequirementOfAnEnforcedSpecIsCovered` included). So every fixture compiles, every markup span is right, and the requirement tags cover RH5419-001 to 004.

Every expected value comes from the spec's examples and requirements, or from the formatter's output for the joined input (`scripts/format.sh`). None comes from the stub (FC-035).

I also mutated the stub's guards:
- Withholding the fix for any comment in the list, without checking whether the list spans lines, fails the one-line block-comment tests.
- Dropping the inner-trivia guard fails the withheld comment, directive, and disabled-text tests.

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| VerifyWrappedArrowIsReportedWithTheRuleMessageAndFixed | RH5419-001, RH5419-003 | `Verify(source, fixed, Diagnostics(...MessageFormat))` | compile error |
| VerifySpecificationExampleOfReportedAndUnreportedIndexers | RH5419-001 | `Verify(source)` | compile error |
| VerifyIndexerOnOneLineIsNotReported | RH5419-001 (FC-003, unreported side) | `Verify(source)` | compile error |
| VerifyArrowBelowTheClosingBracketIsReported | RH5419-001 (FC-003) | `Verify(source)` | compile error |
| VerifyExpressionBelowTheArrowIsReported | RH5419-001 (FC-003) | `Verify(source)` | compile error |
| VerifyParameterListOnSeveralLinesIsReportedWithTheArrowOnTheClosingBracketLine | RH5419-001 (FC-003) | `Verify(source)` | compile error |
| VerifyClosingBracketOnALineOfItsOwnIsReported | RH5419-001 | `Verify(source)` | compile error |
| VerifyExpressionContinuingOnLaterLinesIsNotReported | RH5419-001 (FC-003, FC-027: binary, call, switch) | `Verify(source)` | compile error |
| VerifyLineBreakBetweenThisAndTheOpeningBracketIsNotReported | RH5419-001 | `Verify(source)` | compile error |
| VerifyArrowBelowTheClosingBracketIsReportedBehindALineBreakBetweenThisAndTheOpeningBracket | RH5419-001 | `Verify(source)` | compile error |
| VerifyIndexerWithAnAccessorListIsNotReported | RH5419-001 (non-goal) | `Verify(source)` | compile error |
| VerifyPropertiesAndMethodsAreNotReported | RH5419-001 (non-goal, FC-025) | `Verify(source)` | compile error |
| VerifyReportedIndexersDoNotDependOnTheLineEnding (LF, CRLF) | RH5419-001 | `Verify(source)` | compile error |
| VerifyDiagnosticCoversAttributeListsAndModifiers | RH5419-002 | `Verify(source)` | compile error |
| VerifyIndexerWrappedInSeveralPlacesIsReportedOnce | RH5419-002 | `Verify(source, Diagnostics(...))` | compile error |
| VerifyFixPutsIndexersOnOneLine (LF, CRLF) | RH5419-003 (spec example) | `Verify` | compile error |
| VerifyFixKeepsTheDocumentationCommentAboveTheIndexer (LF, CRLF) | RH5419-003, SHARED-FIX-006 (spec example) | `VerifyCodeFixWithEndOfLine` | compile error |
| VerifyFixPutsAnIndexerWithAttributeListsOnOneLine (LF, CRLF) | RH5419-003 (spec example) | `Verify` | compile error |
| VerifyFixJoinsAPublicIndexerOverAField (LF, CRLF) | RH5419-003 (change doc item 1) | `Verify` | compile error |
| VerifyFixJoinsParameterListsOnSeveralLines (LF, CRLF) | RH5419-001, RH5419-003 (change doc item 2) | `Verify` | compile error |
| VerifyFixRemovesTheWhitespaceAroundLineBreaksInsideTheParameterList (LF, CRLF) | RH5419-003 (break before `,` and `]`, blank line in the list) | `Verify` | compile error |
| VerifyFixJoinsParametersWithAttributesModifiersAndDefaultValues (LF, CRLF) | RH5419-003 (FC-022: literal with a comma) | `VerifyCodeFixWithEndOfLine` | compile error |
| VerifyFixSpacesATightArrow (LF, CRLF) | RH5419-003 | `VerifyCodeFixWithEndOfLine` | compile error |
| VerifyFixPutsAnExplicitInterfaceImplementationOnOneLine (LF, CRLF) | RH5419-003 (FC-024) | `VerifyCodeFixWithEndOfLine` | compile error |
| VerifyFixPutsARefReturningIndexerOnOneLine (LF, CRLF) | RH5419-003 (FC-006: expression starts with `ref`) | `VerifyCodeFixWithEndOfLine` | compile error |
| VerifyFixPutsIndexersOfAStructAndAnInterfaceOnOneLine (LF, CRLF) | RH5419-003 (FC-024: default interface member) | `Verify` | compile error |
| VerifyFixPutsAnIndexerOfANestedTypeOnOneLine (LF, CRLF) | RH5419-003 (FC-028) | `VerifyCodeFixWithEndOfLine` | compile error |
| VerifyFixIsOfferedWhenTheFormatterJoinsTheExpression (LF, CRLF) | RH5419-003, RH5419-004 (counter-case of a withheld wrapped expression) | `VerifyCodeFixWithEndOfLine` | compile error |
| VerifyFixIsOfferedForALineCommentInFrontOfTheOpeningBracket (LF, CRLF) | RH5419-003, RH5419-004 (FC-003, FC-021) | `VerifyCodeFixWithEndOfLine` | compile error |
| VerifyFixIsOfferedForACommentInTheLeadingTriviaOfTheOpeningBracket (LF, CRLF) | RH5419-003, RH5419-004 (FC-003: leading trivia of `[` is outside the list) | `VerifyCodeFixWithEndOfLine` | compile error |
| VerifyFixGivesTheIndexerAndTheMemberBehindItLinesOfTheirOwn (LF, CRLF) | SHARED-FIX-007 (FC-043) | `VerifyCodeFixWithEndOfLine` | compile error |
| VerifyFixGivesAnAttributedIndexerBehindAMemberALineOfItsOwn (LF, CRLF) | SHARED-FIX-007 (FC-043) | `VerifyCodeFixWithEndOfLine` | compile error |
| VerifyFixIsOfferedForACommentBehindTheSemicolon (LF, CRLF) | RH5419-004 (FC-003: trivia behind the `;`) | `Verify` | compile error |
| VerifyFixKeepsOneBlankLineBetweenAnAttributeListAndTheIndexer (LF, CRLF) | RH5419-003, RH5419-004 (offered side of RH5401-004) | `VerifyCodeFixWithEndOfLine` | compile error |
| NoFixIsOfferedInAFileWithASyntaxError | SHARED-FIX-011 | `VerifyNoCodeFixIsOfferedOnceASyntaxErrorIsAdded` | compile error |
| VerifySpecificationExampleOfWithheldFixes (LF, CRLF) | RH5419-001, RH5419-004 (spec example) | `VerifyNoCodeFixIsOffered` | compile error |
| VerifySpecificationExampleOfBlockCommentsThatDoNotWithholdTheFix (LF, CRLF) | RH5419-003, RH5419-004 (spec example) | `Verify` | compile error |
| VerifyFixIsWithheldForABlockCommentInsideAParameterListOnSeveralLines (LF, CRLF) | RH5419-004 (FC-003, FC-021) | `VerifyNoCodeFixIsOffered` | compile error |
| VerifyFixIsWithheldForACommentAtEitherEndOfAParameterListOnSeveralLines (LF, CRLF) | RH5419-004 (FC-002, FC-021: trailing trivia of `[`, own line, leading trivia of `]`) | `VerifyNoCodeFixIsOffered` | compile error |
| VerifyMultiLineBlockCommentInsideTheParameterListIsReportedWithoutAFix (LF, CRLF) | RH5419-001, RH5419-004 | `VerifyNoCodeFixIsOffered` | compile error |
| VerifyFixIsWithheldForADirectiveOrDisabledTextInsideTheParameterList (LF, CRLF) | RH5419-001, RH5419-004 (FC-005, FC-020: `#pragma`, `#region`, `#if false`) | `VerifyNoCodeFixIsOffered` | compile error |
| VerifyFixIsWithheldWhenTheExpressionStaysOnSeveralLines (LF, CRLF) | RH5419-004 (FC-027, FC-022: switch, raw string, wrapped list plus switch) | `VerifyNoCodeFixIsOffered` | compile error |
| VerifyFixIsWithheldForACommentOrDirectiveInFrontOfTheArrow (LF, CRLF) | RH5419-004, SHARED-FIX-010 (FC-019, FC-020, FC-043) | `VerifyNoCodeFixIsOffered` | compile error |
| VerifyFixIsWithheldForACommentOrDirectiveBehindTheArrow (LF, CRLF) | RH5419-004, SHARED-FIX-010 (FC-019, FC-020, FC-043) | `VerifyNoCodeFixIsOffered` | compile error |
| VerifyFixIsWithheldWhenFormattingWouldChangeTheBlankLinesBehindAnAttributeList (LF, CRLF) | RH5419-004 | `VerifyNoCodeFixIsOffered` | compile error |
| VerifyFixIsWithheldForABlankLineADirectiveDisabledTextOrACommentOnItsOwnLineBetweenTheBracketsOfAnAttributeList (LF, CRLF) | RH5419-004 (FC-020) | `VerifyNoCodeFixIsOffered` | compile error |
| VerifyFixAllLeavesTheIndexersWhoseFixIsWithheld (LF, CRLF) | RH5419-003, RH5419-004 (FC-008; change doc items 5 and 6 combined) | `Verify` with `MarkupMode.Allow` | compile error |
| FixAllCorrectsEveryDiagnostic (inherited; `GetFixAllScenario`: two adjacent indexers, an arrow and a parameter list) | SHARED-FIX-001, SHARED-FIX-002 (base tags; FC-007, FC-008) | base | compile error |
| CodeFixProvidesFixAllProvider (inherited) | — | base | compile error |
| VerifyFormatterFixesAWrappedArrowOrExpression | RH5419-001 (FC-013, FC-014) | `VerifyFormatter(source, fixed, diags)` | compile error |
| VerifyFormatterFixesAnArrowBehindALineBreakBetweenThisAndTheOpeningBracket | RH5419-001 (FC-013) | `VerifyFormatter(source, fixed, diags)` | compile error |
| VerifyFormatterKeepsUnreportedIndexers | RH5419-001 (FC-013) | `VerifyFormatter(source)` | compile error |
| VerifyFormatterProducesAnUnreportedIndexerFromAGetOnlyAccessorList | — (FC-013: FMT-STRUCT-015's output is clean) | `VerifyFormatter(source, fixed)` | compile error |
| VerifyFormatterKeepsAReportedParameterListOnSeveralLines | RH5419-001, SHARED-DIAG-010 | `VerifyFormatterKeepsReportedShape` | compile error |

`Verify` with `ConvertEndOfLine` is used wherever a test has several diagnostics. `VerifyCodeFixWithEndOfLine`'s second assertion applies only the first fix, so it is used only for single-diagnostic tests.

## Deviations from the spec examples — finding for the spec author

The `RH5419-001` example in `sdd/specs/analyzer/RH5419.md` does not compile. Its line `int this[uint a, uint b] => a;` fails with CS0266 (`uint` to `int`). The analyzer test harness fails on compiler errors.

In the three tests that use this indexer (`VerifySpecificationExampleOfReportedAndUnreportedIndexers`, `VerifyReportedIndexersDoNotDependOnTheLineEnding`, `VerifyFormatterKeepsUnreportedIndexers`), I wrote `=> 1;` instead. The layout is unchanged.

**Suggested spec edit:** change the example's line to `int this[uint a, uint b] => 1;`, so the spec checker finds the test equal to the example. The other examples in the spec and change doc compile; I ran them all through the stubbed harness.

## Not covered, and why

- **FC-004 (comment or directive dropped by the join):** the fix joins only where no comment, directive, or disabled text stands inside the list, so nothing can be relocated. The withheld tests pin every position: behind `[`, on its own line, in front of `]`, between parameters, and as directive or disabled text. Comments outside the list are pinned by the offered tests: in front of `[` (trailing trivia of `this`, and leading trivia of `[`) and behind `;`.
- **FC-009 / FC-030:** every offered test asserts the full document. The member behind or in front of the indexer is in the SHARED-FIX-007 tests, and `int Foo(byte i) => i;`, which the formatter would turn into a block body, stays untouched in `VerifyFixIsOfferedWhenTheFormatterJoinsTheExpression`. So a fix that formats more than the indexer fails.
- **FC-010 / FC-011 / FC-012:** the formatter is unchanged in this run.
- **FC-038 to FC-043:** no shape is declared. FMT-UNSUP-012 and FMT-UNSUP-013 already exist and are covered through the withheld-around-the-arrow tests (FC-043 / SHARED-FIX-010).
- **FC-018 / FC-023 / FC-036:** no rename; identifiers are not touched.
- **FC-026:** the join edits trivia by token, not by text position. The several-identical-tokens case is in the tests anyway (`int a, int b`, `uint a, uint b`).
- **FC-029:** no key/value layout.
- **FC-040 (documentation mode):** no `///` comment can stand inside a parameter list. The documentation comment above the indexer is covered.
- **RH5401-004 attribute-list cases:** I mirrored the four classes, not every one of RH5401's ~30 attribute tests:
  - a blank line inserted in front of a comment;
  - blank lines collapsed;
  - a blank line, a comment, or a directive or disabled text between the brackets;
  - one blank line kept (offered).

  RH5419-004 refers to RH5401-004 for this guard, and the skeptic flagged FC-015: the guard must be shared, not copied. If the implementer shares RH5401's guard, RH5401's tests are its tests; if the auditor finds a private copy, that is the finding, not missing tests.
- **Skeptic's FC-028 nested-type candidate:** covered (`VerifyFixPutsAnIndexerOfANestedTypeOnOneLine`).
- **Formatter test for a refused indexer:** not written. `VerifyFormatterKeepsReportedShape` asserts that the formatter does not refuse its input, so it cannot express "refused and still reported". The refusal side is covered by the analyzer withheld tests tagged SHARED-FIX-010.
- **Existing tests whose asserted output changes:** none.
  - The change adds a rule and changes no formatter output.
  - Analyzer tests run one analyzer at a time, so no existing test sees RH5419.
  - `rg` over the non-test sources found no expression-bodied indexer with a wrapped arrow or parameter list, so `AnalyzerSelfHostingTests` gains no violation.
  - RH3210's tests keep their output: RH5419 reports some of RH3210's output by design (change doc item 2), but no RH3210 test runs RH5419.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5419"
scripts/test.sh --project architecture
```

## Follow-up candidates

_None._ The comment-between-`this`-and-`[` layout, where the formatter puts `[` or the comment in the member's column, was already dropped by the lead (decision 2). Two tests assert that output as the formatter writes it today: `VerifyFixIsOfferedForALineCommentInFrontOfTheOpeningBracket` and `VerifyFixIsOfferedForACommentInTheLeadingTriviaOfTheOpeningBracket`.

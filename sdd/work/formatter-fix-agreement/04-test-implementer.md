Role: test implementer
Commit: 95a1874
Verdict: #1529: tests written; #1543: tests written

My prompt carried no intended fix, suspected cause, or hoped-for verdict. Expected values come from the spec examples (RH5111-006, FMT-BLANK-020) and, for shapes the examples do not show, from the reference the requirement names: RH5111-006 says "as formatting the document lays it out", and whole-document formatting is unchanged by this run, so its output on `95a1874` (`scripts/format.sh`) is the expected value; for FMT-BLANK-020 the expected value is today's formatter output with only the blank line between the own-line comment and the named token removed (the change doc's before/after pairs differ in exactly that, which I confirmed on the spec example: `diff` of today's output against the spec's expected shows only those 13 blank lines). No expected value comes from patched code.

All new tests were run on `95a1874` with production code unchanged: every test of a changed output fails on the issue's own expected-versus-actual difference (the column of `.C`, the join behind an element binding, or the one blank line); every boundary test passes.

## thoenissen/Reihitsu#1529

### Tests

All in `src/Reihitsu.Analyzer.Test/Formatting/RH5111AssignmentsMustHaveProperLineBreaksAnalyzerTests.cs` unless named otherwise. The three former tests `VerifyFixKeepsTheWrappedLinkOfAnAssignedMember{BehindAMemberBinding,BehindAnElementBinding,InANestedConditionalAccess}` (expected output from the superseded `node-root-detached.md` answer) are replaced by the first, fifth, and sixth rows below. Every verify test runs under LF and CRLF (`VerifyUnderBothLineEndings` or two calls); the code-fix verifier also runs Fix All and checks re-analysis.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| VerifyFixAlignsAWrappedMemberBehindAMemberBindingUnderTheAnchor (`.C` col 10 → 9) | RH5111-006 | `Verify` | fail (col 10) |
| VerifyFixKeepsAWrappedMemberThatStandsInTheAnchorColumn (col 9 stays) | RH5111-006; FC-007 | `Verify` | fail (moves to 10) |
| VerifyFixMovesAWrappedMemberLeftOfTheAnchorIntoTheAnchorColumn (col 8 → 9) | RH5111-006; FC-003 | `Verify` | fail |
| VerifyFixMovesAWrappedMemberRightOfTheAssignmentStartIntoTheAnchorColumn (col 14 → 9) | RH5111-006; FC-003 | `Verify` | fail |
| VerifyFixJoinsAMemberBehindAnElementBinding (`a?[0].C = 5;`) | RH5111-006 | `Verify` | fail (wrap kept) |
| VerifyFixAlignsAWrappedMemberBehindANestedConditionalAccessUnderTheAnchor (col 12 → 9) | RH5111-006; FC-027 | `Verify` | fail (col 13) |
| VerifyFixMovesAWrappedMemberUnderTheLastBindingOfANestedConditionalAccessIntoTheAnchorColumn (col 13 → 9) | RH5111-006; FC-027 | `Verify` | fail |
| VerifyFixAlignsAWrappedMemberBehindAnInvokedBindingUnderTheAnchor (`a?.N()`) | RH5111-006 | `Verify` | fail |
| VerifyFixAlignsAWrappedMemberWhoseOperatorIsWrappedUnderTheAnchor (`=` wrapped) | RH5111-006 | `Verify` | fail |
| VerifyFixAlignsTwoWrappedLinksUnderTheAnchor | RH5111-006 | `Verify` | fail |
| VerifyFixAlignsAWrappedMemberBehindAReceiverWithAMemberAccessUnderTheAnchor (`a.B?.B`) | RH5111-006; FC-027 | `Verify` | fail (col 12) |
| VerifyFixAlignsAWrappedMemberBehindAnElementBindingOfANestedConditionalAccessUnderTheAnchor (`a?.B?[0]`) | RH5111-006; FC-027 | `Verify` | fail (col 13) |
| VerifyFixJoinsAMemberBehindTwoElementBindings (`a?[0]?[1].C = 5;`) | RH5111-006; FC-027 | `Verify` | fail |
| VerifyFixJoinsTheFirstLinkBehindAnElementBindingAndAlignsTheNextLinkUnderIt (`a?[0].B⏎ .C` in col 13) | RH5111-006; FC-027 | `Verify` | fail |
| VerifyFixJoinsAMemberAccessBehindAnElementBinding (`a?[0].B.C = 5;`) | RH5111-006 | `Verify` | fail |
| VerifyFixAlignsAWrappedMemberInALambdaBodyUnderTheAnchor (anchor col 33) | RH5111-006; FC-027 | `Verify` | fail (col 34) |
| VerifyFixKeepsTheCommentAboveTheConditionalAccess | RH5111-006; FC-019 | `Verify` | fail (col 10; comment kept) |
| VerifyFixAllLaysOutEveryAssignmentBehindAConditionalAccess (spec example 1, six diagnostics) | RH5111-006; FC-008 | `Verify` | fail |
| VerifyFixOfTheRequirementBoundariesLaysOutTheAssignmentsInPlace (spec example 2) | RH5111-006; FC-003 | `Verify` | pass |
| VerifyFixJoinsTheValueOfAnAssignmentBehindAConditionalAccessWhoseLinksShareALine (`a?.B.C =⏎ 5`) | RH5111-006; FC-003 | `Verify` | pass |
| VerifyFixJoinsTheValueOfAnAssignmentToABareBinding (`a?.B =⏎ a`) | RH5111-006; FC-003 | `Verify` | pass |
| VerifyFixOfAnAssignmentNotBehindAConditionalAccessAlignsItsWrappedMemberAsInPlace (`a.B⏎ .C`) | RH5111-006; FC-003 | `Verify` | pass |
| VerifyNoFixIsOfferedForAnAssignmentBehindAConditionalAccessWithACommentLineInFrontOfTheWrappedMember | RH5111-005; FC-019 | `VerifyNoCodeFixIsOffered` | pass |
| VerifyNoFixIsOfferedForAnAssignmentBehindAConditionalAccessWithACommentBehindTheBinding | RH5111-005; FC-019, FC-021 | `VerifyNoCodeFixIsOffered` | pass |
| VerifyNoFixIsOfferedForAnAssignmentBehindAConditionalAccessWithADirectiveInFrontOfTheWrappedMember | RH5111-005; FC-019, FC-020 | `VerifyNoCodeFixIsOffered` | pass |
| VerifyFixBehind{MemberBinding,ElementBinding,NestedConditionalAccess,ElementBindingWithTwoWrappedLinks,ReceiverWithAMemberAccess}MatchesFormattingTheDocument (5 tests) | RH5111-006; FC-028, FC-007 | `VerifyCodeFixMatchesFormatter`, LF and CRLF | fail |
| `RH5201MethodChainsShouldBeAlignedAnalyzerTests.VerifyNoDiagnosticForAssignmentsBehindAConditionalAccessLaidOutAsTheDocument` (spec example 1 output plus `a?[0].B⏎ .C`) | RH5111-006; FC-007 | `Verify(source)` LF and CRLF | pass |
| `RH5112WrappedFluentCallsShouldKeepFirstCallOnOriginalLineAnalyzerTests.VerifyNoDiagnosticForAssignmentsBehindAConditionalAccessLaidOutAsTheDocument` (same source) | RH5111-006; FC-007 | `Verify(source)` LF and CRLF | pass |

The fixtures' class `A` has a block-bodied `N()`: an expression-bodied method is rewritten by whole-document formatting (RH3202), which would make the parity tests fail for an unrelated reason.

### Not covered, and why

- RH6023: not involved (`00-lead.md` decision 2; existing tests pin its gap-only fix).
- Whole-document formatting tests for these inputs: no formatter requirement changes; the five parity tests compare the fix with `ReihitsuFormatter.FormatSyntaxTree` of the same document, which is the skeptic's requested check without pinning formatter columns twice.
- FC-006/FC-022–FC-025/FC-029/FC-036/FC-018: no rename, literal, member kind, or key/value layout involved.
- FC-038–FC-043: no shape declared unsupported (maintainer answer 1B rejects E).
- FC-030 (fix widens its target): the tests assert the whole document, so any change in front of the assignment fails them; `VerifyFixKeepsTheCommentAboveTheConditionalAccess` puts a comment in front of the conditional access. No input with an odd spacing around `?` was added: the requirement keeps "the code in front" unchanged, and no caller-reached shape needs it beyond what the whole-document assertions already catch.
- A comment or directive inside the reported assignment withholds the fix (RH5111-005, pinned above); a fix with a comment there is not a case of RH5111-006.

## thoenissen/Reihitsu#1543

### Tests

`src/Reihitsu.Formatter.Test/Regression/BlankLines/CommentBeforeBraceBlankLineTests.cs` (`AssertRuleResult`: LF and CRLF, second pass, per-phase stability):

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RemovesBlankLineBetweenCommentAndOpeningBraceOfPropertyArrayInitializer (was `KeepsBlankLineBetweenCommentAndArrayInitializerBrace`, now asserts removal) | FMT-BLANK-020 | `AssertRuleResult` | fail |
| RemovesBlankLineBetweenCommentAndClosingBraceOfAssignedCollectionInitializerInItsFormattedLayout (was `KeepsBlankLineBetweenCommentAndClosingBraceOfAssignedCollectionInitializer`, the change doc's old "unchanged" case) | FMT-BLANK-020 | `AssertRuleResult` | fail |
| RemovesBlankLineBetweenCommentAndBothBracesOfAssignedArrayInitializer (entry 1) | FMT-BLANK-020 | `AssertRuleResult` | fail |
| RemovesBlankLineBetweenCommentAndClosingBraceOfFieldArrayInitializerAndNestedElementBrace (entry 1, field form, nested `{`) | FMT-BLANK-020; FC-027 | `AssertRuleResult` | fail |
| RemovesBlankLineBetweenCommentAndClosingBraceOfImplicitArrayAndStackAllocInitializer (`new[]`, `stackalloc`) | FMT-BLANK-020; FC-027 | `AssertRuleResult` | fail |
| RemovesBlankLineBetweenCommentAndBothBracesOfExtensionBlock (entry 2) | FMT-BLANK-020 | `AssertRuleResult` | fail |
| RemovesBlankLineBetweenCommentAndOpeningBraceOfPropertyPattern (entry 3) | FMT-BLANK-020 | `AssertRuleResult` | fail |
| RemovesBlankLineBetweenCommentAndClosingBraceOfAssignedCollectionInitializer (entry 4) | FMT-BLANK-020 | `AssertRuleResult` | fail |
| RemovesBlankLineBetweenCommentAndClosingBraceOfRawStringInterpolation (entry 5) | FMT-BLANK-020; FC-022 | `AssertRuleResult` | fail |
| RemovesBlankLineBetweenCommentAndClosingBraceOfRegularStringInterpolation (blank lines only; the comment's column is not asserted, per the change doc) | FMT-BLANK-020; FC-022 | `AssertBlankLinesAbove` | fail |
| RemovesBlankLineBetweenCommentAndElseCatchFinallyAndDoWhile (entry 6) | FMT-BLANK-020 | `AssertRuleResult` | fail |
| RemovesBlankLineBehindDelimitedCommentAndBehindLastOfTwoCommentsBeforeArrayInitializerBraceAndElse (`/* c */`; two comments — the blank line between them stays) | FMT-BLANK-020; FC-021 | `AssertRuleResult` | fail |
| RemovesBlankLineBetweenCommentBehindDirectiveAndArrayInitializerBraceOrElse (`#if`/`#endif` then comment) | FMT-BLANK-020; FC-019, FC-020 | `AssertRuleResult` | fail |
| RemovesBlankLineBetweenCommentAndEveryBraceAndChainedKeywordOfSpecificationExample (the new spec example verbatim) | FMT-BLANK-020 | `AssertRuleResult` | fail |
| KeepsBlankLineBetweenCommentAndBlockStatementAfterStatementButRemovesItBehindCaseLabel (FMT-BLANK-017 side, and a block behind `case 1:` on the other) | FMT-BLANK-017, FMT-BLANK-020; FC-003 | `AssertRuleResult` | pass |

Analyzer round trips in `src/Reihitsu.Analyzer.Test/Formatter/Formatting/` (`FormatterTestsBase<TAnalyzer>.VerifyFormatter`: the rule reports the marked blank line before formatting and nothing after, second pass stable, LF and CRLF):

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH5024…FormatterTests.VerifyFormatterRemovesBlankLineBehindCommentBeforeClosingBraceOf{ArrayInitializer,ExtensionBlock,AssignedCollectionInitializer,RawStringInterpolation} (4 tests) | FMT-BLANK-020; FC-013 | `VerifyFormatter` | fail |
| RH5025…FormatterTests.VerifyFormatterRemovesBlankLineBehindCommentBeforeOpeningBraceOf{ArrayInitializer,NestedArrayElement,ExtensionBlock,PropertyPattern} (4 tests) | FMT-BLANK-020; FC-013 | `VerifyFormatter` | fail |
| RH5026…FormatterTests.VerifyFormatterRemovesBlankLineBehindCommentBeforeElseCatchAndFinally | FMT-BLANK-020; FC-013 | `VerifyFormatter` | fail |
| RH5027…FormatterTests.VerifyFormatterRemovesBlankLineBehindCommentBeforeDoStatementWhile | FMT-BLANK-020; FC-013 | `VerifyFormatter` | fail |

The RH5024 interpolation round trip uses a single-`$` raw string: the test-markup parser reads `$$` as a caret marker, so a `$$"""` source cannot carry markup.

### Not covered, and why

- Node-level formatting of a collection initializer assigned to a member (the `RootConstruct.AssignedCollectionInitializer` fact): no caller formats an `InitializerExpressionSyntax` as its target — `rg "InitializerExpression" src/Reihitsu.Analyzer.CodeFixes` finds only an attribute-placement brace lookup; RH5303/RH5304 format the enclosing statement, equals-value clause, or arrow clause (`TryGetFormatTarget`). Not reached (`AGENTS.md`, *Supported inputs*).
- The issue's own input (comment between a field's `=` and its initializer): refused by FMT-UNSUP-004 and unchanged (change doc *Decisions*); existing precheck tests cover the refusal.
- Code fixes of RH5024–RH5027: unchanged by this run; the round trips above check the analyzers against the formatter's new output.
- FC-012 (blank-line phase vs. FMT-BLANK-011): every test asserts the blank line above the comment stays, and `AssertRuleResult` checks the second pass and each phase alone.
- FC-038–FC-043, FC-018, FC-036: no unsupported shape, no rename.
- The column of a comment in front of a regular interpolated string hole's `}`: outside this run (follow-up in `00-lead.md`); only blank lines are asserted there.

## Whole run

Files: the eight test files named above and this handover. `scripts/format.sh` over them changes nothing. `scripts/test.sh --project architecture`: 117 of 117 pass (the requirement-coverage check now finds RH5111-006 covered). Focused runs on `95a1874`: analyzer 144 run, 33 fail (all new or rewritten tests of changed outputs), formatter `CommentBeforeBraceBlankLineTests` 35 run, 14 fail (likewise); the failing set is exactly the tests marked `fail` above.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5111AssignmentsMustHaveProperLineBreaksAnalyzerTests|FullyQualifiedName~VerifyNoDiagnosticForAssignmentsBehindAConditionalAccessLaidOutAsTheDocument|FullyQualifiedName~MustNotBePrecededByBlankLineFormatterTests"
scripts/test.sh --project formatter --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Regression.BlankLines.CommentBeforeBraceBlankLineTests"
scripts/test.sh --project architecture
```

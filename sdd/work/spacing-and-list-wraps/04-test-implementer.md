Role: test implementer
Commit: 144e6fe
Verdict: #1642: tests written (characterization, green); #1635: tests written (red for the specified reason)

My prompt carried no intended fix, suspected cause, or hoped-for verdict. Expected values come from FMT-BREAK-012's examples, the change doc `sdd/changes/2026-11/list-wrap-by-elements.md` (entries 1–11), and the skeptics' candidate inputs; none was taken from patched code.

## thoenissen/Reihitsu#1642

Decision 2: characterization only. Every test passes on 144e6fe under LF and CRLF, with a second pass and the per-phase stability check (`AssertRuleResult`). Each multi-line comment case sits next to its single-line twin in the same fixture, so the test shows both get the same gap.

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `MultiLineCommentGapSpacingTests.SpaceBehindMultiLineCommentAfterQualifiedNameDotIsRemovedAsBehindSingleLineComment` (issue's first shape, parameter type) | FMT-SPACE-001, FMT-SPACE-013; FC-033, FC-034 | `AssertRuleResult` | pass |
| `MultiLineCommentGapSpacingTests.SpaceBehindMultiLineCommentInConstraintClauseIsRemovedAsBehindSingleLineComment` | FMT-SPACE-001, FMT-SPACE-013 | `AssertRuleResult` | pass |
| `MultiLineCommentGapSpacingTests.SpaceBehindMultiLineCommentInFrontOfTypeArgumentIsRemovedAsBehindSingleLineComment` | FMT-SPACE-001, FMT-SPACE-013 | `AssertRuleResult` | pass |
| `MultiLineCommentGapSpacingTests.SpaceBehindMultiLineCommentInFrontOfBinaryOperandStaysAsBehindSingleLineComment` (issue's second shape, verbatim run of two spaces in front) | FMT-SPACE-001, FMT-SPACE-013 | `AssertRuleResult` | pass |
| `MultiLineCommentGapSpacingTests.SpaceDemandedBehindMultiLineCommentIsInsertedAsBehindSingleLineComment` | FMT-SPACE-001, FMT-SPACE-013 | `AssertRuleResult` | pass |

Observation from building the fixtures: with `1 +/* a⏎ b */2`, the gap in front of the comment stays empty (`1 +/* a`) while one space is inserted behind it (`*/ 2`), for the single-line twin too. This matches FMT-SPACE-013 (the whitespace in front of the comment is only collapsed).

### Not covered, and why

- The comma rule, the no-space rule after `(`, and the right-carried `/** */` case that 01-skeptic ran: these go through the same gap handling as the five cases above. I added no test for them, so this is a judgment, not a static proof.
- Code-fix path: no rule reports these gaps, so no fix is involved.

## thoenissen/Reihitsu#1635

All new `FMT-BREAK-012` tests except the ones marked "pass" fail on 144e6fe because of the specified output difference: on LF, before CRLF runs, the actual output is `main`'s output as the change doc records it under "before". I checked each failure message:
- the list stays partly wrapped;
- or the comma is split off onto its own line (`Foo(1⏎,⏎2,⏎3)`, `int b // keep⏎,⏎int c`);
- or the indexer is split behind `[`.

Two fixtures first failed for the wrong reason: an empty `class C : Attribute { }` became `class C : Attribute;` (FMT-STRUCT). I gave those classes a member, and they now fail on the comma layout only.

### Tests

New file `src/Reihitsu.Formatter.Test/Regression/LineBreaks/ListWrapByElementsTests.cs` (`AssertRuleResult`: LF, CRLF, second pass, per-phase stability):

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `ArgumentListWrappedInFrontOfCommaGetsOneArgumentPerLineWithTrailingCommas` (issue input, entry 1) | FMT-BREAK-012; FC-034 | `AssertRuleResult` | fail |
| `ArgumentListWrappedInFrontOfLastCommaGetsOneArgumentPerLine` | FMT-BREAK-012 | `AssertRuleResult` | fail |
| `LeadingCommaInListWithOneArgumentPerLineMovesBehindPreviousArgument` (entry 2) | FMT-BREAK-012 | `AssertRuleResult` | fail |
| `LeadingCommaOfTwoArgumentListMovesBehindFirstArgument` (entry 3) | FMT-BREAK-012 | `AssertRuleResult` | fail |
| `AttributeArgumentListWrappedInFrontOfCommaGetsOneArgumentPerLine` (entry 4) | FMT-BREAK-012; FC-016 | `AssertRuleResult` | fail |
| `NestedArgumentListWrappedInFrontOfCommaGetsTrailingCommas` (entry 5) | FMT-BREAK-012; FC-027 | `AssertRuleResult` | fail |
| `ObjectCreationArgumentListWrappedInFrontOfCommaGetsOneArgumentPerLine` | FMT-BREAK-012; FC-016 | `AssertRuleResult` | fail |
| `ConstructorInitializerArgumentListWrappedInFrontOfCommaGetsOneArgumentPerLine` | FMT-BREAK-012; FC-016 | `AssertRuleResult` | fail |
| `PrimaryConstructorBaseArgumentListWrappedInFrontOfCommaGetsOneArgumentPerLine` | FMT-BREAK-012; FC-016 | `AssertRuleResult` | fail |
| `SpecificationExampleLeadingCommasMoveBehindPreviousElement` (FMT-BREAK-012 second example, verbatim) | FMT-BREAK-012 | `AssertRuleResult` | fail |
| `SpecificationExampleHeldCommasStayAndCommentsMoveWithTheirComma` (third example, verbatim) | FMT-BREAK-012 | `AssertRuleResult` | fail |
| `SpecificationExampleListWrappedOnlyBehindOpeningParenthesisIsJoined` (first example's new `Foo(⏎a, b)` with its neighbors) | FMT-BREAK-012 | `AssertRuleResult` | pass (W1 join already on `main`) |
| `AttributeArgumentAndParameterListsWrappedOnlyBehindOpeningParenthesisAreJoined` | FMT-BREAK-012; FC-016 | `AssertRuleResult` | pass (W1 join already on `main`) |
| `SpecificationExampleIndexerWrappedOnlyBehindOpeningBracketIsJoined` (fourth example, verbatim; entry 6) | FMT-BREAK-012 | `AssertRuleResult` | fail |
| `CommentHeldParameterCommaStaysWhileFreeCommaMoves` (entry 7 `N`) | FMT-BREAK-012, FMT-BREAK-011 | `AssertRuleResult` | fail |
| `CommentHeldParameterCommaStaysWhileParametersSharingALineAreSplit` (entry 7 `O`) | FMT-BREAK-012, FMT-BREAK-011 | `AssertRuleResult` | fail |
| `CommentHeldArgumentCommaStaysWhileFreeCommaMoves` (entry 7, first argument list) | FMT-BREAK-012; FC-021 | `AssertRuleResult` | fail |
| `CommentHeldLastArgumentCommaStaysWhileArgumentsInFrontAreSplit` (entry 7, second) | FMT-BREAK-012; FC-021 | `AssertRuleResult` | fail |
| `CommentHeldArgumentCommaStaysWhileArgumentsOnItsLineAreSplit` (entry 7, third) | FMT-BREAK-012; FC-021 | `AssertRuleResult` | fail |
| `BlockCommentHeldArgumentCommaStaysWhileLaterArgumentsAreSplit` (`Foo(1 /* x */⏎, 2, 3)` → `Foo(1 /* x */⏎, 2,⏎3)`) | FMT-BREAK-012; FC-005, FC-021 | `AssertRuleResult` | fail |
| `DirectiveHeldArgumentCommaStaysWhileArgumentsInFrontAreSplit` (entry 8) | FMT-BREAK-012; FC-019, FC-020 | `AssertRuleResult` | fail |
| `CommentHeldAttributeArgumentCommaStaysWhileArgumentsInFrontAreSplit` | FMT-BREAK-012; FC-016 | `AssertRuleResult` | fail |
| `CommentHeldIndexerParameterCommaStaysWhileFreeCommaMoves` | FMT-BREAK-012, FMT-BREAK-011; FC-016 | `AssertRuleResult` | fail |
| `CommentHeldLambdaParameterCommaStaysWhileParametersInFrontAreSplit` | FMT-BREAK-012, FMT-BREAK-011; FC-016, FC-027 | `AssertRuleResult` | fail |
| `BlockCommentBehindLeadingArgumentCommaMovesWithTheComma` (entry 9, block) | FMT-BREAK-012; FC-004 | `AssertRuleResult` | fail |
| `LineCommentBehindLeadingArgumentCommaMovesWithTheComma` (entry 9, `//`) | FMT-BREAK-012; FC-004 | `AssertRuleResult` | fail |
| `BlockCommentInFrontOfCommaOnArgumentLineKeepsNothing` (`Foo(1 /* one */, 2,⏎3)`) | FMT-BREAK-012; FC-003 | `AssertRuleResult` | pass |
| `LineBreakInsideMultiLineCommentInFrontOfCommaKeepsNothing` (`Foo(1 /* a⏎ b */, 2,⏎3)`, SHARED-BLANK-008 counting) | FMT-BREAK-012; FC-003 | `AssertRuleResult` | pass |
| `LineBreakInsideMultiLineCommentBetweenArgumentsLeavesListSingleLine` (`Foo(1, /* x⏎ y */ 2, 3)` stays) | FMT-BREAK-012; FC-003 | `AssertRuleResult` | pass |
| `ArgumentListWithCommentHeldCommaInFinalLayoutStays` (entry 7 output as input; `main` splits the held comma off onto its own line) | FMT-BREAK-012; FC-010 | `AssertRuleResult` | fail |

Existing tests updated to the changed spec:

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `IndexerParameterListLayoutTests.IndexerParameterListWrappedOnlyBehindOpeningBracketIsJoined` (was `FirstIndexerParameterJoinsOpeningBracket`; expected flipped from the split to `int this[int a, int b] => a;`, entry 6) | FMT-BREAK-012 | `AssertRuleResult` | fail |
| `LineBreakRequirementTests.WrappedListsBecomeOneElementPerLine` (now FMT-BREAK-012's first example, verbatim, with the new `Foo(⏎a, b)` → `Foo(a, b)`) | FMT-BREAK-012 | `AssertRuleResult` | pass |

Code fixes (`src/Reihitsu.Analyzer.Test/Formatting/`):

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH5102…AnalyzerTests.FixMovesLeadingCommaBehindPreviousArgument` (entry 10, first list) | RH5102-003, FMT-BREAK-012; FC-009, FC-032 | `VerifyCodeFixWithEndOfLine` LF and CRLF (iterative, Fix All, one-pass `ApplyCodeFixAsync`) | fail (fix result is the input) |
| `RH5102…AnalyzerTests.FixMovesLeadingCommaOfListWrappedBehindOpeningParenthesis` (entry 10, second list) | RH5102-003, FMT-BREAK-012; FC-009, FC-032 | `VerifyCodeFixWithEndOfLine` LF and CRLF | fail |
| `RH5102…AnalyzerTests.FixMovesLeadingCommasOfSeveralArgumentListsInOneDocument` (entry 10, both lists in one document) | RH5102-003, FMT-BREAK-012; FC-008 | `Verify` (iterative and Fix All) | fail |
| `RH5102…AnalyzerTests.VerifyCommentHeldLeadingCommaWithOneArgumentPerLineIsNotReported` (entry 7 output: no RH5102 report) | RH5102-001; FC-013 | `Verify(source)` | pass |
| `RH5101…AnalyzerTests.FixMovesLeadingCommaOfListWrappedBehindOpeningParenthesis` (entry 11) | RH5101-003, FMT-BREAK-012; FC-032 | `VerifyCodeFixWithEndOfLine` LF and CRLF | fail (gives `Call(1⏎, 2, 3)`) |

### Not covered, and why

- **Per-owner arms of the argument list**: the invocation, object creation, constructor initializer, and primary-constructor base tests all reach `LineBreakListRewriter.VisitArgumentList` (`rg -n "VisitArgumentList|VisitAttributeArgumentList|VisitParameterList|VisitBracketedParameterList" src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/LineBreakListRewriter.cs`). I kept a test per owner anyway because indentation differs per owner. Implicit object creation (`new(1⏎, 2)`) has no test of its own; it reaches the same visitor.
- **A multi-line raw string as an element** (FC-022): 03-skeptic saw it split on `main`; the change leaves "an element spans lines" unchanged. Not re-tested here.
- **A held comma in front of the closing parenthesis** (`Foo(1, 2, 3⏎)`): out of scope (non-goal, #956).
- **FC-028** (detached vs document node): the RH5101/RH5102 fix tests go through the document-scoped entry point the fixes use; no caller formats these lists detached.
- **FC-038 to FC-043**: no shape is declared unsupported.
- **FC-006, FC-023 to FC-026, FC-029, FC-036, FC-018**: moving a comma between two tokens changes no meaning, and touches no identifier, partial type, expression body, key/value layout, or rename.
- **FC-031, FC-037**: these concern the implementer's and cleaner's changes, not tests.

### For the lead: existing tests the spec wording leaves open

Two existing, untagged tests in `src/Reihitsu.Formatter.Test/Regression/Indentation/ParameterContinuationLineAlignmentTests.cs` pin a held comma **alone on its line**, and expect it to stay:
- `CommaAloneOnItsLineStaysAligned`: `int first // keep⏎,⏎int second`
- `CommaAfterDirectiveHeldFirstParameterStaysAligned`: `#endif⏎,⏎int second`

FMT-BREAK-012 now says such a comma "MUST stay at the start of the line of the element behind it". The change doc calls the lone comma `main`'s defect (entry 7, before).

Read as a target layout, the requirement would join them to `, int second`. Read as "leave it where it is", they stay. Both pass on 144e6fe. I did not change them.

The implementer can break them either way. I recommend that the lead decide, and that the spec author add the case to FMT-BREAK-012's text if the target layout is meant.

### Pre-existing failure outside my lane

`scripts/test.sh --project architecture` fails `RepositoryFilesReferenceNoSquadNote`: `03-skeptic.md` line 24 names a file under the squad notes folder. This is not from this handover; the lead or the skeptic should reword that line before the PR.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~MultiLineCommentGapSpacingTests"
scripts/test.sh --project formatter --filter "FullyQualifiedName~ListWrapByElementsTests|FullyQualifiedName~IndexerParameterListLayoutTests|FullyQualifiedName~LineBreakRequirementTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5102ArgumentsShouldBeOnSingleOrSeparateLinesAnalyzerTests|FullyQualifiedName~RH5101FirstArgumentShouldBeOnSameLineAnalyzerTests"
```

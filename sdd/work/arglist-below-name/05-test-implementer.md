Role: test implementer
Commit: 112cd57
Verdict: tests written

Inputs: thoenissen/Reihitsu#1786 (`agent-filed`), `00-lead.md` (*Maintainer answers* 1A 2A 3A 4A), `01-spec-author.md` to `04-spec-author.md`, `02-skeptic.md`, `sdd/changes/2026-11/argument-list-below-name.md` (Accepted), `git diff origin/main -- sdd/specs`. My prompt carried no intended fix, suspected cause, or hoped-for verdict.

Expected values: the spec examples of FMT-BREAK-039 verbatim (extracted from `line-breaks.md`); the change doc's change 6 *after* blocks; everything else is `main`'s output (`scripts/format.sh`, `scripts/apply-fix.sh`) of the same input with only the line break in front of the argument list's `(` removed by hand — never from patched code (FC-035). "Unchanged" cases assert `main`'s output of the input as written. Every code-fix test was first run green with `main`'s *before* output to prove its markup, then switched to the *after* output.

Expected now: **29 formatter tests and 12 code-fix test cases (6 methods × LF/CRLF) fail**, each on the first-pass / iterative-fix output mismatch at the argument list's `(` (checked: no compile error, no markup or stability failure). All other new tests pass on `main` and must keep passing. `scripts/test.sh --project architecture`: 153 passed (FMT-BREAK-039 now covered).

## Tests

New file `src/Reihitsu.Formatter.Test/Regression/LineBreaks/ArgumentListLineStartTests.cs` (`FormatterTestsBase`; `AssertRuleResult` = LF and CRLF, second pass, per-phase stability):

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RequirementExampleJoinsTheArgumentListOfEveryOwner | FMT-BREAK-039 | AssertRuleResult | fail |
| RequirementExampleJoinsWithElementAndElementsWhileCollectionsKeepOneElementPerLine | FMT-BREAK-039 | AssertRuleResult | fail |
| RequirementExampleJudgesListAndConditionalAsTheLineTheyBecome | FMT-BREAK-039 | AssertRuleResult | fail |
| RequirementExampleKeepsTheLineBreakWhenTheGapHoldsCommentDirectiveOrDisabledText | FMT-BREAK-039 (FC-019, FC-020, FC-021) | AssertRuleResult (unchanged) | pass |
| GenericInvocationJoinsItsArgumentList | FMT-BREAK-039 | AssertRuleResult | fail |
| InvocationOfInvocationJoinsEveryArgumentList (`make()⏎(2)⏎(3)`) | FMT-BREAK-039 (FC-026) | AssertRuleResult | fail |
| InvocationOfSuppressedExpressionJoinsItsArgumentList (`f!⏎(1)`) | FMT-BREAK-039 | AssertRuleResult | fail |
| NameofInvocationJoinsItsArgumentList | FMT-BREAK-039 | AssertRuleResult | fail |
| ThisConstructorInitializerJoinsItsArgumentList | FMT-BREAK-039 | AssertRuleResult | fail |
| RecordPrimaryConstructorBaseTypeJoinsItsArgumentList | FMT-BREAK-039 | AssertRuleResult | fail |
| SeveralBlankLinesInTheGapGoWithTheJoin | FMT-BREAK-039 | AssertRuleResult | fail |
| ArgumentListInsideLambdaBlockJoins | FMT-BREAK-039 (FC-027) | AssertRuleResult | fail |
| NestedArgumentListsJoinBothNames (`Foo⏎(Bar⏎(1))`) | FMT-BREAK-039 (FC-027) | AssertRuleResult | fail |
| MultiLineArgumentListJoinsItsOpeningParenthesis (`Foo⏎(⏎1,⏎2)` → `Foo(1,⏎    2)`) | FMT-BREAK-039 (FC-027) | AssertRuleResult | fail |
| ArgumentListWithWrappedArgumentJoinsItsName | FMT-BREAK-039 | AssertRuleResult | fail |
| ObjectCreationWithInitializerJoinsItsArgumentList | FMT-BREAK-039 (FC-027) | AssertRuleResult | fail |
| ArgumentListInInterpolationHoleJoins | FMT-BREAK-039 (FC-022, FC-027) | AssertRuleResult | fail |
| ArgumentListInSwitchExpressionArmJoins | FMT-BREAK-039 (FC-027) | AssertRuleResult | fail |
| ArgumentListInLambdaExpressionBodyJoins | FMT-BREAK-039 (FC-027) | AssertRuleResult | fail |
| ArgumentListBehindBinaryOperatorJoins | FMT-BREAK-039 | AssertRuleResult | fail |
| ArgumentListInFieldInitializerAndArrowExpressionJoins | FMT-BREAK-039 (FC-025, FC-027) | AssertRuleResult | fail |
| ObjectCreationFollowedByMemberAccessJoinsItsArgumentList (`new C⏎().P`) | FMT-BREAK-039 | AssertRuleResult | fail |
| CommentBehindOpeningParenthesisDoesNotBlockTheJoin (`Foo⏎( // c⏎4)`) | FMT-BREAK-039 (FC-003, FC-021) | AssertRuleResult | fail |
| CommentsOutsideTheGapDoNotBlockTheJoin | FMT-BREAK-039 (FC-003, FC-021) | AssertRuleResult | fail |
| ArgumentListFormattedByFirstArgumentFixJoinsInTheDocument (`Foo⏎(⏎1, 2)`) | FMT-BREAK-039 | AssertRuleResult | fail |
| DelimitedCommentInFrontOfOpeningParenthesisKeepsTheLineBreak | FMT-BREAK-039 (FC-021) | AssertRuleResult (unchanged) | pass |
| CommentLineInTheGapKeepsTheLineBreak | FMT-BREAK-039 (FC-019) | AssertRuleResult | pass |
| PragmaDirectiveInTheGapKeepsTheLineBreak | FMT-BREAK-039 (FC-020) | AssertRuleResult (unchanged) | pass |
| RegionDirectiveInTheGapKeepsTheLineBreak | FMT-BREAK-039 (FC-020) | AssertRuleResult | pass |
| InactiveConditionalBlockInTheGapKeepsTheLineBreak (`#if false`) | FMT-BREAK-039 (FC-020) | AssertRuleResult (unchanged) | pass |
| ArgumentListsAlreadyOnTheirNamesLineStayUnchanged | FMT-BREAK-039 | AssertRuleResult (unchanged) | pass |
| TypeofOperandBelowTheKeywordKeepsItsLineBreak | — (FC-003, characterization: not an argument list) | AssertRuleResult (unchanged) | pass |
| StatementStartingWithParenthesizedExpressionIsNotJoined | — (FC-003, characterization) | AssertRuleResult (unchanged) | pass |
| DelimitedDocumentationCommentBehindTheNameKeepsTheLineBreak (Parse, Diagnose) | FMT-BREAK-039 (FC-040) | AssertRuleResult + parse options | pass |
| DelimitedDocumentationCommentBehindTheNameKeepsTheLineBreakWithoutDocumentationParsing (None) | FMT-BREAK-039 (FC-040) | AssertRuleResult (unchanged) | pass |
| DelimitedDocumentationCommentInFrontOfOpeningParenthesisKeepsTheLineBreak (Parse, Diagnose) | FMT-BREAK-039 (FC-040) | AssertRuleResult + parse options | pass |
| DelimitedDocumentationCommentInFrontOfOpeningParenthesisKeepsTheLineBreakWithoutDocumentationParsing (None) | FMT-BREAK-039 (FC-040) | AssertRuleResult (unchanged) | pass |
| SingleLineDocumentationCommentInTheGapKeepsTheLineBreak (None, Parse, Diagnose) | FMT-BREAK-039 (FC-040) | AssertRuleResult + parse options | pass |
| CommentInTheGapKeepsTheLineBreakForEveryOwner (`: base // c`, `B /* c */`, `new List<int> // c`, `[with // c`) | FMT-BREAK-039 (FC-019) | AssertBlankLinesAbove (line starts + stability only; those columns are undecided, F2) | pass |

Other new tests:

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `Unit/LineBreaks/JoinSeparatorPhaseTests.ArgumentListJoinsWriteNoWhitespace` (line-break phase alone: `: base(1)`, `class E() : B(2);`, `Foo(3);`, `new(4)`, same tokens, LF and CRLF) | FMT-BREAK-039, FMT-BREAK-031 | ExecuteLineBreakPhase | fail |
| `Unit/ReihitsuFormatterNodeColumnTests.StatementTargetJoinsArgumentListBelowItsName` | FMT-BREAK-039 (FC-028) | AssertFormatsTarget + AssertRuleResult | fail |
| `Unit/ReihitsuFormatterNodeColumnTests.CollectionExpressionTargetJoinsWithElementArgumentListBelowWith` (the issue's CLI/fix column split) | FMT-BREAK-039 (FC-028) | AssertFormatsTarget + AssertRuleResult | fail |
| `Unit/ReihitsuFormatterNodeColumnTests.ArgumentListTargetBelowItsNameKeepsItsOpeningParenthesisInPlace` | FMT-PIPE-017 | AssertFormatsTarget | pass |
| `Unit/ReihitsuFormatterDetachedRootTests.FormatNodeOnStatementJoinsArgumentListBelowItsName` | FMT-BREAK-039 (FC-028) | AssertFormatNode | fail |
| `RH5305…AnalyzerTests.VerifyFixJoinsWithElementArgumentListBelowWith` (LF, CRLF) | FMT-BREAK-039 (FC-007, FC-030) | VerifyCodeFixWithEndOfLine | fail |
| `RH5305…AnalyzerTests.VerifyFixJoinsWithElementArgumentListsOfTwoCollectionExpressions` (LF, CRLF) | FMT-BREAK-039 (FC-008) | Verify (iterative + Fix All) | fail |
| `RH5303…AnalyzerTests.VerifyFixJoinsElementArgumentListBelowItsName` (LF, CRLF) | FMT-BREAK-039 | VerifyCodeFixWithEndOfLine | fail |
| `RH5301…AnalyzerTests.VerifyFixJoinsAssignedValueArgumentListBelowItsName` (LF, CRLF) | FMT-BREAK-039 | VerifyCodeFixWithEndOfLine | fail |
| `RH5302LogicalExpressions…AnalyzerTests.VerifyFixJoinsOperandArgumentListBelowItsName` (LF, CRLF) | FMT-BREAK-039 | VerifyCodeFixWithEndOfLine | fail |
| `RH5308…AnalyzerTests.VerifyFixJoinsBranchArgumentListBelowItsName` (LF, CRLF) | FMT-BREAK-039 | VerifyCodeFixWithEndOfLine | fail |
| `RH5101…AnalyzerTests.VerifyFixKeepsOpeningParenthesisBelowTheInvokedName` (LF, CRLF) | FMT-PIPE-017 | VerifyCodeFixWithEndOfLine | pass |
| `RH5102…AnalyzerTests.VerifyFixKeepsOpeningParenthesisBelowTheInvokedName` (LF, CRLF) | FMT-PIPE-017 | VerifyCodeFixWithEndOfLine | pass |

Existing tests changed (`Regression/LineBreaks/ChainLinkLayoutTests.cs`):

| Test | Change | Expected now |
|---|---|---|
| ArgumentListWithoutMemberAccessKeepsItsBreak → **ArgumentListWithoutMemberAccessJoinsItsName** | asserted `var r = Foo⏎(1);` (the output the change doc changes) → `var r = Foo(1);`; tagged FMT-BREAK-039 | fail |
| ArgumentListJoinsItsElement | tag FMT-BREAK-020 → FMT-BREAK-039 (FMT-BREAK-020 no longer states the argument-list join); output unchanged | pass |
| ArgumentListOfRootJoinsInMemberChain | tag FMT-BREAK-020 → FMT-BREAK-039; output unchanged | pass |
| ArgumentListJoinsGenericMemberName | untagged → FMT-BREAK-039; output unchanged | pass |

Existing tests searched and left: a scan of every test `.cs` file for a line starting with `(` behind a line ending in a name, `>`, `)`, or `]`, plus a search for escaped `\n(` in string fixtures. Every other hit is not an argument list (`typeof`, parameter lists, lambda parameters, `delegate`, `union`, keyword `if/while/for…` in RH6001), already joined inside a list element or chain (`ListElementInteriorJoinTests.InvocationWrappedBeforeOpenParenthesisIsJoinedWithoutSpace`, `JoinSeparatorTests.JoinedPairWithoutRuleKeepsTheMinimalSeparator`, `JoinSeparatorPhaseTests.JoinsOfTokensThatStayApartWriteNoWhitespace`, `RH5201FluentChainLinkFormatterTests.VerifyWrappedArgumentListIsNotReported`) — outputs unchanged, tags left (FMT-BREAK-015/031 tests assert the separator or the list-element join, not the gap's owner) — or has a comment/directive in the gap (`ChainLinkLayoutTests.CommentBeforeArgumentListKeepsArgumentListAtAnchorColumn`, `RH7302…` `#endregion⏎(value);`), unchanged. No CLI, tooling, or self-hosting fixture holds the shape.

## Scenarios (bug fix)

_Not a bug fix._

## Not covered, and why

- Bracketed argument lists (`arr⏎[0]`) and attribute argument lists: FMT-BREAK-039's coverage sentence leaves them undecided; the existing `ChainLinkLayoutTests.ElementAccessWithoutMemberAccessKeepsItsBreak` characterizes the former (F1).
- Cast `(f)⏎(1)` / `(int)⏎(x + 1)`, `default⏎(int)`, `checked⏎(x)`, `sizeof⏎(int)`: not argument lists, outputs undecided (F4 dropped). One representative (`typeof`) plus the parenthesized-statement case pin the "not an argument list" side; more would lock undecided output.
- Columns of a line that keeps its `(` behind a comment/directive inside a collection or initializer: undecided (F2, thoenissen/Reihitsu#953); `CommentInTheGapKeepsTheLineBreakForEveryOwner` checks line starts only.
- FC-013 / FC-014: no rule reports the shape (RH5105 registers `ParameterList` only), and no rule is added; no `VerifyFormatter` parity test applies.
- FC-015 / FC-016 / FC-042 (one owner of the gap): structural, not observable in output beyond the existing FMT-BREAK-015/020 tests that keep passing next to the new ones; for the auditor.
- FC-018, FC-036 (renames), FC-029 (key/value), FC-024 (partial/explicit interface), FC-044: cannot reach a whitespace join.
- FC-022 raw strings: an interpolation hole is covered; a raw-string hole is the same gap inside a hole and not tested separately.
- FC-023 verbatim identifiers (`@Foo⏎(1)`): same identifier token kind in front of `(`; not tested separately.
- FC-038, FC-039, FC-041, FC-043: no shape is declared unsupported; the join removes only a gap with no comment, so it cannot carry a comment into an FMT-UNSUP span. FC-040's documentation modes are covered anyway (above).
- Separate idempotency fixtures in `Idempotency/IdempotencyTests.cs`: every `AssertRuleResult` already asserts the second pass under LF and CRLF (the issue's "Done when" idempotency item).

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~ArgumentListLineStartTests|FullyQualifiedName~ChainLinkLayoutTests|FullyQualifiedName~JoinSeparatorPhaseTests|FullyQualifiedName~ReihitsuFormatterNodeColumnTests|FullyQualifiedName~ReihitsuFormatterDetachedRootTests"
scripts/test.sh --project analyzer --filter "Name~VerifyFixJoins|Name~VerifyFixKeepsOpeningParenthesisBelowTheInvokedName"
scripts/test.sh --project architecture
```

## Follow-up candidates

_None_ beyond F1–F3 in `00-lead.md`.

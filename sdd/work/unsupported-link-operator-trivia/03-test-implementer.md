Role: test implementer
Commit: 277e0eb
Verdict: tests written

Inputs read: thoenissen/Reihitsu#894 (via `00-lead.md`; the maintainer's triage and batch-1 answers 1A, 2A), `00-lead.md`, `01-spec-author.md`, `02-skeptic.md`, the change doc `sdd/changes/2026-11/unsupported-link-operator-trivia.md`, the spec diff of 568f167 (FMT-UNSUP-015, FMT-BREAK-019, RH2001-007, RH7101-007). My prompt carried no intended fix, suspected cause, or hoped-for verdict.

Expected values come from FMT-UNSUP-015 (reason text, location = first comment/directive/disabled text, input unchanged) and the change doc's locations. Two expected outputs of supported inputs were confirmed against `main` output, whose behavior this run does not change: the FMT-BREAK-019 whitespace-join fixture (matches SHARED-CHAIN-006 and the skeptic's quoted output) and the RH2001 fix result for a comment in front of `?.` (`Source /* three */?.Length`, the same spacing as RH2001-007's `Offset(/* three */3)`).

Every new refusal test was run on this head and fails for the right reason: `AssertUnsupportedFor` fails on "The formatter must return a refused input unchanged" (the phases format the input today) or "must report the input as unsupported" (input already in formatted shape); node-level tests fail on `AssertReportsComment` "must be reported as unsupported"; code-fix tests fail on "No code fix may be offered" (a fix is offered today); CLI tests fail on exit code 0 instead of 2. No failure is a compile error or a diagnostic-markup mismatch. Every other-side, closure, and agreement test passes on this head.

## Tests

New file `src/Reihitsu.Formatter.Test/Unit/Precheck/UnsupportedLinkOperatorCommentTests.cs` (class `UnsupportedLinkOperatorCommentTests`, 205 cases):

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `SpecExampleEachLinkOperatorIsUnsupported` (location 7,20) | FMT-UNSUP-015 | `AssertUnsupportedFor` | red |
| `EachLinkOperatorOfTheSpecExampleIsUnsupported` (4 rows) | FMT-UNSUP-015 | `AssertUnsupportedFor` | red |
| `SpecExampleDirectiveInsideANullForgivingLinkOperatorIsUnsupportedInEveryBuild` (DEBUG on/off, location 8,1) | FMT-UNSUP-015 | `AssertUnsupportedFor` | red |
| `SpecExampleCommentsOutsideTheLinkOperatorsAreSupported` | FMT-UNSUP-015 | `AssertSupported` | green |
| `EveryCommentAndDirectiveInEveryGapOfEveryLinkOperatorIsUnsupported` (DynamicData: 4 gaps × 15 trivia = 60) — gaps `?`–`.` of `?.`, `!`–`.` of `!.`, `!`–`?` and `?`–`.` of `!?.`; trivia `/* */` one line (no line break, FC-003), without whitespace, `//` behind the earlier token, `/* */` at line end, comment line, comment line behind a blank line, `/* */` in front of the later token, multi-line `/* */`, `#pragma`, `#region`/`#endregion`, `#if false` with disabled text, comment in front of a directive, `#nullable`, `#line`, empty `#if`/`#elif`/`#endif` | FMT-UNSUP-015; FC-003, FC-005, FC-019, FC-020, FC-021 | `AssertUnsupportedFor` | red |
| `DocumentationCommentInEveryGapIsUnsupportedInEveryDocumentationMode` (4 gaps × 4 comments × None/Parse/Diagnose = 48) | FMT-UNSUP-015; FC-040 | `AssertUnsupportedFor` | red |
| `ConditionalGroupInEveryGapIsUnsupportedInEveryBuild` (4 gaps × X on/off) | FMT-UNSUP-015; FC-020 | `AssertUnsupportedFor` | red |
| `ConditionalGroupAroundTheWholeStatementIsSupported` (X on/off) | FMT-UNSUP-015 | `AssertSupported` | green |
| `EveryLinkOperatorIsCheckedWithItsOwnTokens` (13 rows: later link of wrapped chain, `!.` later link, invoked link, call-less chain, null-conditional assignment target, list element, interpolation hole, lambda, switch arm, another link's argument list, `?[ ]` index, local initializer, local function in a lambda) | FMT-UNSUP-015; FC-027, FC-039 | `AssertUnsupportedFor` | red |
| `LinkOperatorInAMemberIsUnsupported` (expression-bodied property, property initializer, field with one declarator) | FMT-UNSUP-015; FC-027 | `AssertUnsupportedFor` | red |
| `LinkOperatorInDisabledTextIsNotChecked` (X off: supported; X on: refused) | FMT-UNSUP-015 | `AssertSupported` / `AssertUnsupportedFor` | X off green, X on red |
| `CommentOutsideTheLinkOperatorIsSupported` (18 rows: front gap of `?.`, `!.`, `!?.` with `/* */`, `//`, comment line; front gap of `!.` behind another `!`; directive in front of a later link; `///` and `/** */` in front of `?.`; `.`-to-name gap of each operator; inside/behind the link; behind/above the statement; in front of the token in front) | FMT-UNSUP-015; FC-003 | `AssertSupported` | green |
| `CommentNextToATokenThatIsNoLinkOperatorIsSupported` (plain `.` both sides, logical `!`, `..` both sides, `->` both sides, `::`) | FMT-UNSUP-015 | `AssertSupported` | green |
| `CommentInTheGapOfAnotherShapeKeepsItsReason` (`n? /* c */ [0]`, `n! /* c */ ?[0]`, `n!? /* c */ [0]` → FMT-UNSUP-014 reason; `b ? /* c */ .5 : 1` → FMT-UNSUP-008 reason) | FMT-UNSUP-015 | `AssertUnsupportedFor` | green |
| `CommentInsideALinkOperatorInAFieldWithSeveralDeclaratorsGivesTheReasonOfTheLinkOperator` | FMT-UNSUP-004, FMT-UNSUP-015 | `AssertUnsupportedFor` | red |
| `DirectiveInsideALinkOperatorInAFieldWithSeveralDeclaratorsKeepsTheReasonOfTheFieldDeclaration` (X on/off; several declarators → FMT-UNSUP-010 reason, one declarator → FMT-UNSUP-015 reason) | FMT-UNSUP-010, FMT-UNSUP-015 | `AssertUnsupportedFor` | red (on its second assertion) |
| `FirstShapeInTheTextIsReported` (with FMT-UNSUP-011, -014, -008; and `u /* first */ !? /* second */ .Length` reports `/* second */`: the front gap of `!?.` is neither FMT-UNSUP-014 nor -015) | FMT-UNSUP-001, FMT-UNSUP-015 | `AssertUnsupportedFor` | red |
| `FormatDocumentAsyncRefusesTheShape` (LF, CRLF) | FMT-UNSUP-015 | `ReihitsuFormatter.FormatDocumentAsync` | red |
| `NodeLevelEntryPointsRefuseATargetThatHoldsTheShape` (`FormatNode` on method and statement, `FormatNodeInDocumentAsync` on statement and type, `FormatNodeInDocumentWithContextAsync`, method beside formatted) | FMT-UNSUP-015; FC-028 | entry points | red |
| `BranchOfAConditionalAccessAloneDoesNotHoldTheShape` (`WhenNotNull` with `// c` in the `.`'s leading trivia: not refused by `FindUnsupportedSyntax`, `FormatNode`, `FormatNodeInDocumentAsync`; conditional access and root refused) | FMT-UNSUP-015, FMT-UNSUP-003; FC-041 | entry points | red |
| `NullForgivingOperatorAloneDoesNotHoldTheShapeOfItsMemberAccess` (`t!` alone with `/* c */` in trailing trivia: not refused; member access refused) | FMT-UNSUP-015, FMT-UNSUP-003; FC-041 | entry points | red |
| `PartsOfANullForgivingConditionalAccessAloneDoNotHoldTheShape` (2 rows, `!`–`?` and `?`–`.` of `!?.`) | FMT-UNSUP-015, FMT-UNSUP-003; FC-041 | `FindUnsupportedSyntax`, `FormatNode` | red |
| `FindUnsupportedSyntaxAnswersForABuiltNode` (4 operators, built with `SyntaxFactory.ParseExpression`) | FMT-UNSUP-003, FMT-UNSUP-015; FC-043 | `FindUnsupportedSyntax` | red |
| `FindUnsupportedSyntaxGivesTheAnswerOfThePrecheck` (4 trees) | FMT-UNSUP-015 | `FindUnsupportedSyntax` vs `FormatSyntaxTree` | green (both null today; must stay equal) |
| `OutputOfFormattedLinkOperatorsIsNotUnsupported` (13 rows, whitespace-split operators joined beside comments in front, behind, behind the `.`, in the argument list, behind a later link, doc comments in modes Parse/None/Diagnose; two passes, LF and CRLF) | FMT-UNSUP-005, FMT-UNSUP-015; FC-038, FC-040, FC-017 | `FormatSyntaxTree` twice | green |

Other files:

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `ChainLinkLayoutTests.EveryTokenOfALinkOperatorSplitByWhitespaceJoinsTheLineOfItsFirstToken` (the skeptic's eight-statement fixture, exact output) | FMT-BREAK-019; FC-004 regression guard for the deletions | `AssertRuleResult` | green |
| `CliHiddenCodeEndToEndTests.CommentOrDirectiveInsideALinkOperatorIsReportedAsUnsupported` (the change doc's four inputs: (5,20), (8,1), (8,24), (7,20); exit 2, file unchanged, exact stderr line) | FMT-UNSUP-015 | `Program.Main` via `Run` | red |
| `RH5201MethodChainsShouldBeAlignedAnalyzerTests.NoFixIsOfferedForACommentInsideALinkOperator` (change doc input, LF/CRLF) | SHARED-FIX-010, FMT-UNSUP-015; FC-043 | `VerifyNoCodeFixIsOffered` | red |
| `RH5112…AnalyzerTests.VerifyNoFixIsOfferedForACommentInsideALinkOperator` (change doc input, LF/CRLF) | SHARED-FIX-010, FMT-UNSUP-015 | `VerifyNoCodeFixIsOffered` | red |
| `RH3202…AnalyzerTests.VerifyNoFixIsOfferedForACommentInsideALinkOperator` (change doc input, LF/CRLF) | SHARED-FIX-010, FMT-UNSUP-015 | `VerifyNoCodeFixIsOffered` | red |
| `RH3202…AnalyzerTests.VerifyNoFixIsOfferedForACommentInsideANullForgivingLinkOperator` (`!.`, `!`–`?` and `?`–`.` of `!?.`) | SHARED-FIX-010, FMT-UNSUP-015 | `VerifyNoCodeFixIsOffered` | red |
| `RH2001…TriviaTests.VerifyNoFixIsOfferedForACommentInsideALinkOperatorOfTheInitializerValue` (`= Source? /* three */ .Length;`, LF/CRLF) | RH2001-007 | `VerifyNoCodeFixIsOffered` | red |
| `RH2001…TriviaTests.VerifyNoFixIsOfferedForALineCommentInsideALinkOperatorOfTheInitializerValue` (change doc input, LF/CRLF) | RH2001-007 | `VerifyNoCodeFixIsOffered` | red |
| `RH2001…TriviaTests.VerifyFixIsOfferedForACommentInFrontOfALinkOperatorOfTheInitializerValue` (other side, LF/CRLF) | RH2001-007; FC-003 | `VerifyCodeFixWithEndOfLine` | green |
| `RH7101…CommentAndDirectiveTests.SpecExampleCommentInsideALinkOperatorWithholdsTheFix` (change doc input, LF/CRLF) | RH7101-007 | `VerifyNoCodeFixIsOffered` | red |
| `RH7101…CommentAndDirectiveTests.CommentInsideALinkOperatorElsewhereInTheTypeWithholdsTheFix` (`?.`, `!.`, `!?.` in a method of the type) | RH7101-007; FC-043 | `VerifyNoCodeFixIsOffered` | red |

Existing tests that asserted today's output for the shape, rewritten to the refusal (`AssertUnsupportedFor(LinkOperatorCommentReason, input, <first trivia>)`, input kept, tag now FMT-UNSUP-015; red now):

- `ConditionalAccessWrapBetweenOperatorTokensTests`: `LineCommentBetweenQuestionMarkAndDotKeepsCommentAndAlignsDotUnderQuestionMark` → `LineCommentBetweenQuestionMarkAndDotIsUnsupported`; `BlockCommentBetween…AlignsDotUnderQuestionMark` → `BlockCommentBetweenQuestionMarkAndDotIsUnsupported`; `PragmaBetween…AlignsDotUnderQuestionMark` → `PragmaBetweenQuestionMarkAndDotIsUnsupported`; `DisabledTextBetween…AlignsUnderQuestionMark` → `DisabledTextBetweenQuestionMarkAndDotIsUnsupported`; `CommentOnOneLinkOnlyLeavesOtherLinksJoined` → `CommentOnOneLinkOfAWrappedChainIsUnsupported`; `CommentOnSoleLinkMovesQuestionMarkOntoItsOwnLine` → `CommentOnSoleLinkBehindAnInvocationIsUnsupported`; `CommentAfterQuestionMarkOnIdentifierRootKeepsQuestionMarkOnRootLine` → `CommentAfterQuestionMarkOnIdentifierRootIsUnsupported`; `OwnLineCommentBetween…AlignsCommentAndDotUnderQuestionMark` → `OwnLineCommentBetweenQuestionMarkAndDotIsUnsupported`; `CommentBetweenQuestionMarkAndDotInShortChainAlignsDotUnderQuestionMark` → `CommentBetweenQuestionMarkAndDotInShortChainIsUnsupported`. The class summary's sentence about aligning the binding token under `?` now states the refusal (FC-031).
- `ChainGapJoinTests`: `SingleLineCommentInsideLinkOperatorKeepsItsLine` → `SingleLineCommentInsideLinkOperatorIsUnsupported`; `DirectiveInsideLinkOperatorKeepsItsLine` → `DirectiveInsideLinkOperatorIsUnsupported` (both were tagged FMT-BREAK-001; now FMT-UNSUP-015 only).
- `ChainLinkLayoutTests`: `CommentBetweenNullForgivingAndConditionalKeepsCommentBehindNullForgiving` → `CommentBetweenNullForgivingAndConditionalIsUnsupported`; `CommentInsideNullForgivingConditionalOperatorKeepsOperatorStartTogether` → `CommentBetweenNullForgivingConditionalAndDotIsUnsupported`; `CommentInsideNullForgivingMemberAccessPutsDotUnderNullForgiving` → `CommentInsideNullForgivingMemberAccessIsUnsupported`.

Helper: `FormatterTestsBase.LinkOperatorCommentReason = "a comment or directive inside a ?. or !. operator"` (batch 1, answer 1A).

How the list of existing tests was established: a scan of every `.cs` file under the test projects (escaped `\n` in `DataRow`s decoded) for a `?`, `!`, or `!?` directly behind an expression token or at a line start, followed by trivia that holds `//`, `/*`, or a directive line, and then `.` or `?.`. Hits outside the rewritten tests, left unchanged because they verify the analyzer only (no fix is applied, analyzers do not consult the precheck, RH5201-003 unchanged): `RH5048FluentChainLinkAnalyzerTests.VerifyCommentInsideConditionalOperatorIsNotReported`, `RH5201MethodChainsShouldBeAlignedAnalyzerTests.VerifyCommentHeldLineBreakInsideOperatorOfCallLessChainIsNotReported`, `…VerifyCommentHeldLineBreakInsideLaterLinkOperatorIsReportedForSharingALine` (analyzer-only `Verify(source, diagnostics)`). `ChainGapJoinTests.SingleLineCommentBetweenDotAndMemberNameKeepsItsLine` (the `.`-to-name gap) is outside the shape and unchanged. No unit test calls `AlignRestOfRun`, `CloseInnerGaps`, or `CloseGap` directly (`rg` over the test projects: no match).

Full formatter project on this head: 6309 tests, 156 failing — exactly the new and rewritten refusal tests; no other test changed state. Architecture tests: 198 of 198 pass (FMT-UNSUP-015 is now covered).

## Not covered, and why

- FMT-UNSUP-015's overlap with FMT-UNSUP-004 outside a value ("the two meet only outside a value"): I found no input in which a link operator stands inside a variable declaration outside its initializer values (attribute arguments must be constant; `nameof(a?.B)` does not compile). The skeptic's wording note stands for the spec checker; no test.
- FMT-UNSUP-006 (a phase's own refusal): the change declares no phase throw site, so there is no `SyntaxNotSupportedException` site to test.
- Fix All: withheld fixes register no action, so no Fix All path runs on the shape; the existing Fix All scenarios of the five rules are unaffected.
- FC-033 (reproduction normalized away): not a bug fix; every refusal test runs LF and CRLF, and the code-fix path and CLI are tested separately.
- FC-035: no expected value was taken from a patched copy; the two supported outputs confirmed on `main` are named above.
- FC-042 (a guard serving only the declared shape survives): a source-level audit concern; the tests that pinned the guards' outputs are rewritten to the refusal, and the whitespace-join fixture guards the deletion's other side.
- Every other catalog class (naming, renamer, casing, ordering, region, raw-string, CLI option parsing) touches no code this change reaches.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~UnsupportedLinkOperatorCommentTests|FullyQualifiedName~ConditionalAccessWrapBetweenOperatorTokensTests|FullyQualifiedName~ChainGapJoinTests|FullyQualifiedName~ChainLinkLayoutTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~LinkOperator"
scripts/test.sh --project cli --filter "FullyQualifiedName~CliHiddenCodeEndToEndTests.CommentOrDirectiveInsideALinkOperatorIsReportedAsUnsupported"
scripts/test.sh --project architecture
```

## Follow-up candidates

- Observed while formatting a draft of the test file, outside this run (no spec found for it): `scripts/format.sh` puts the continuation clauses of a query expression at the column of the enclosing member or statement. Input `return from a in new[] { 1 }⏎               from b in new[] { 2 }⏎               select a + b;` gives `return from a in new[] { 1 }⏎        from b in new[] { 2 }⏎        select a + b;` (same in an expression-bodied property). RH3007 reports every query expression, so such input already violates a Reihitsu rule; the test file avoids query syntax. Observation only.

Role: test implementer
Commit: 7457808
Verdict: #1737: reproduced; #1738: reproduced; whole run (FMT-UNSUP-009): tests written

The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond pointers to the handovers and specs. Expected values come from the spec examples (RH5107-004, FMT-BREAK-012, FMT-BREAK-038, FMT-UNSUP-009, FMT-STRUCT-027, FMT-INDENT-032, RH5110-003). For FMT-BREAK-038 cases the spec shows no example of (nested `Items = { … }`, complex element, `new[]`, `stackalloc`, collection initializer, list pattern, `case` property pattern, nesting, multi-line comment behind the comma), the expected value is what the requirement prescribes: the comma-last form of the same input, as `main` formats it (`scripts/format.sh` on a scratch file, run once; the comma-last inputs contain no comma that starts a line, so FMT-BREAK-038 does not touch them).

How the affected existing tests were found: a scratch Roslyn scanner (outside the repository) parsed every string literal of `src/**` as C# (no symbols, and again with every `#if` symbol of the literal defined) and listed each literal holding a comma token with a comment, directive, or disabled text between it and the token in front of it on an earlier text line — FMT-UNSUP-009's definition. 115 literals hit; each was then read. Literals built by interpolation or concatenation are not covered by that scan.

## thoenissen/Reihitsu#1737

### Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH5107…AnalyzerTests.VerifyNoCodeFixWhenBlockCommentStandsBetweenCommaAndParameter` (reproduction, rewritten from `VerifyCommentFollowingCommaIsPreservedAndAligned`) | RH5107-004 | `VerifyNoCodeFixIsOffered` | fail (fix offered) |
| `…VerifySpecificationExampleCommentsBehindTheCommaWithholdTheFix` (P, Q, R) | RH5107-004 | `VerifyNoCodeFixIsOffered` | fail |
| `…VerifyNoCodeFixForEachCommentBehindTheComma` (6 rows: `//`, block ending line, two blocks, block + `//`, multi-line `/* a⏎ b */`, no whitespace) | RH5107-004; FC-004/FC-021 | `VerifyNoCodeFixIsOffered` | fail |
| `…VerifyNoCodeFixForCommentBehindTheCommaInEveryParameterList` (primary ctor, ctor, local function, anonymous method, lambda) | RH5107-004; FC-016 | `VerifyNoCodeFixIsOffered` | fail |
| `…VerifyNoCodeFixForCommentBehindTheCommaWithCarriageReturnLineFeeds` | RH5107-004; FC-010 | `VerifyNoCodeFixIsOffered` | fail |
| `…VerifyFixAllMovesOnlyTheCommaWithoutACommentBehindIt` (Fix All, mixed list, commented diagnostic remains) | RH5107-004; FC-008 | `Verify` (batch) | fail (moves both) |
| `…VerifyCommentOnTheLineBehindTheCommaKeepsTheFix` (T) | RH5107-004, RH5107-003; FC-003 | `Verify` | pass |
| `…VerifyDirectiveOnTheLineBehindTheCommaKeepsTheFix` | RH5107-004, RH5107-003; FC-005/FC-020 | `Verify` | pass |

### Scenarios (bug fix)
| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| Issue input `void P(int a⏎, /* c */ int b)` | LF | RH5107 code fix | reproduced | Observed: fix offered, `int a,⏎/* c */ int b` (the existing characterization `VerifyCommentFollowingCommaIsPreservedAndAligned` was green on this head and asserted exactly that). Expected (RH5107-004): diagnostic, no fix. |
| Q `, // c`, R `, /* c */` | LF, CRLF | RH5107 code fix | reproduced | fix offered; expected none |
| V-shape under Fix All (one commented, one plain comma) | LF | Fix All | reproduced | both commas moved; expected only the plain one |

### Not covered, and why
- Formatter side of P/Q/R: unchanged behavior, already covered by FMT-BREAK-012's example test (`ListWrapByElementsTests.SpecificationExampleCommentsMoveWithTheirComma`, `BlockCommentBehindLeadingArgumentCommaMovesWithTheComma`, `LineCommentBehindLeadingArgumentCommaMovesWithTheComma`).
- Naming/Renamer (FC for naming fixes): not a naming fix.

## thoenissen/Reihitsu#1738

### Tests
New class `src/Reihitsu.Formatter.Test/Regression/LineBreaks/InitializerLeadingCommaTests.cs` (all `AssertRuleResult`: LF and CRLF, second pass, phase stability):

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `ReportedTrailingCommaOnItsOwnLineMovesBehindTheLastMember` (issue's input, reproduction) | FMT-BREAK-038 | `AssertRuleResult` | fail |
| `SpecificationExampleCommaThatStartsALineEndsTheElementsLine` | FMT-BREAK-038 | `AssertRuleResult` | fail |
| `SpecificationExampleArrayInitializerCommaMovesAndKeepsTheOtherLineBreaks` | FMT-BREAK-038 | `AssertRuleResult` | fail |
| `CommaAloneOnItsLineBetweenTwoMembersLeavesNoLine` | FMT-BREAK-038 | `AssertRuleResult` | fail |
| `MultiLineBlockCommentBehindTheCommaGoesWithIt` | FMT-BREAK-038; FC-021 | `AssertRuleResult` | fail |
| `CommaInNestedCollectionAndComplexElementInitializersEndsTheElementsLine` | FMT-BREAK-038; FC-016/FC-027 | `AssertRuleResult` | fail |
| `CommaInCollectionInitializerEndsTheElementsLine` | FMT-BREAK-038 | `AssertRuleResult` | fail |
| `CommaInImplicitArrayAndStackallocInitializersEndsTheElementsLine` | FMT-BREAK-038 | `AssertRuleResult` | fail |
| `CommaInListPatternAndCasePropertyPatternEndsTheSubpatternsLine` | FMT-BREAK-038 | `AssertRuleResult` | fail |
| `CommaInNestedInitializersEndsTheElementsLineAtEveryLevel` (argument, lambda body, initializer in initializer) | FMT-BREAK-038; FC-027 | `AssertRuleResult` | fail |
| `CommasThatDoNotStartALineAreNotChanged` | FMT-BREAK-038; FC-003 | `AssertRuleResult` | pass |
| `RH5301…AnalyzerTests.VerifyFixMovesACommaThatStartsALineBehindTheMember` (LF, CRLF) | RH5301-004, FMT-BREAK-038; FC-028/FC-032 | `ApplyCodeFixAsync` + `Format` | fail |
| `RH5301…VerifyFixMovesATrailingCommaThatStartsALineBehindTheLastMember` (LF, CRLF) | RH5301-004, FMT-BREAK-038 | `ApplyCodeFixAsync` + `Format` | fail |
| `RH5303…VerifyFixMovesACommaThatStartsALineBehindTheElement` (LF, CRLF) | RH5303-005, FMT-BREAK-038 | `ApplyCodeFixAsync` + `Format` | fail |
| `RH5304…VerifyFixMovesACommaThatStartsALineBehindTheElement` (LF, CRLF) | RH5304-004, FMT-BREAK-038 | `ApplyCodeFixAsync` + `Format` | fail |

Each fix-path test asserts both the exact expected text and that the fix result equals whole-document formatting (SHARED-FIX-007).

### Scenarios (bug fix)
| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| Issue input `new D(1, 2) { X = 3⏎, };` | LF, CRLF | formatter | reproduced | Observed: `X = 3` then `,` alone at column 8 (statement column). Expected: `X = 3,` |
| `new D { X = 3⏎, Y = 4 }` | LF, CRLF | RH5301 fix | reproduced | Observed: `,` alone at column 16; expected `X = 3,` ⏎ `Y = 4` |
| `new List<int> { 1⏎, 2 }` | LF, CRLF | RH5303 fix | reproduced | Observed: `,` alone at column 14 |
| nested `Items = { 1⏎, 2 }` | LF, CRLF | RH5304 fix | reproduced | Observed: `,` at the wrong column alone on its line |

### Not covered, and why
- Held commas in initializers (directive-held, `// k`-held): refused by FMT-UNSUP-009, covered under *Whole run*.
- RH5305/RH5306 fix paths: the skeptic found they report nothing on `, 2` (02-skeptic), so no fix path reaches the input.
- Switch arms, positional patterns, base lists: outside FMT-BREAK-038, follow-up candidates in `00-lead.md`.

## Whole run

FMT-UNSUP-009 declaration and the spec edits it caused.

### Tests
New class `src/Reihitsu.Formatter.Test/Unit/Precheck/UnsupportedListSeparatorCommentTests.cs`, modeled on the FMT-UNSUP-008 class; refusal asserts use `AssertUnsupportedFor` (LF and CRLF, output unchanged, reason, location in the caller's tree). New constant `ListSeparatorCommentReason` in `Formatter.Test/Helpers/FormatterTestsBase.cs`.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `SpecExampleCommentsInFrontOfCommasOnLaterLinesAreUnsupported` | FMT-UNSUP-009 | `AssertUnsupportedFor` | fail |
| `EachListOfTheSpecExampleIsUnsupported` (5 rows, incl. multi-line `/* a⏎ b */,`) | FMT-UNSUP-009 | `AssertUnsupportedFor` | fail |
| `ShapeIsUnsupportedInEveryListOfAStatement` (31 rows: arguments, element access, tuple, tuple type, every initializer kind, dictionary key, collection expression, list/property/positional pattern, switch arms, `for` list, lambda/anonymous/local-function parameters, type arguments, omitted type argument both ways, held trailing commas, `#pragma`) | FMT-UNSUP-009; FC-016 | `AssertUnsupportedFor` | fail |
| `ShapeIsUnsupportedInEveryListOfADeclaration` (11 rows: ctor, indexer, primary ctor, ctor initializer, type parameters, attribute arguments, attribute list, function pointer, base list, enum members, enum trailing comma) | FMT-UNSUP-009 | `AssertUnsupportedFor` | fail |
| `DocumentationCommentInFrontOfACommaIsUnsupportedInEveryDocumentationMode` (`///`, own-line `/** */`, trailing `/** */` × Parse/None/Diagnose) | FMT-UNSUP-009; FC-040 | `AssertUnsupportedFor` | fail |
| `SpecExampleDirectiveInFrontOfACommaIsUnsupportedInEveryBuild` (DEBUG defined / not) | FMT-UNSUP-009 | `AssertUnsupportedFor` | fail |
| `CommaInADefinedBranchIsUnsupportedAndInDisabledTextIsNot` | FMT-UNSUP-009 | both | fail (refusal half) |
| `DisabledTextInFrontOfACommaIsUnsupported`, `EndIfDirectiveInFrontOfACommaIsUnsupported` | FMT-UNSUP-009; FC-005/FC-020 | `AssertUnsupportedFor` | fail |
| `SpecExampleCommentsAndDirectivesOutsideTheShapeAreSupported` (X defined / not) | FMT-UNSUP-009 | `AssertSupported` | pass |
| `CommentsOutsideTheShapeAreSupported` (9 rows, each in both builds of X) | FMT-UNSUP-009; FC-003 | `AssertSupported` | pass |
| `CommasInsideDocumentationCommentsAndDirectivesAreSupported` | FMT-UNSUP-009 | `AssertSupported` | pass |
| `FirstOfTwoCommentsIsReported` | FMT-UNSUP-009 | `AssertUnsupportedFor` | fail |
| `DeclaratorCommaKeepsTheReasonOfAVariableDeclarationForAComment` | FMT-UNSUP-009, FMT-UNSUP-004 | `AssertUnsupportedFor` | fail (directive half; the comment halves pass today) |
| `FormatDocumentAsyncRefusesTheShape` | FMT-UNSUP-009, FMT-UNSUP-002 | direct | fail |
| `NodeLevelEntryPointsRefuseATargetThatHoldsTheShape` (FormatNode, FormatNodeInDocumentAsync on method and on the argument list, WithContext, beside method formatted) | FMT-UNSUP-009, FMT-UNSUP-002 | direct | fail |
| `ElementTargetWithoutTheCommaIsNotRefused` | FMT-UNSUP-009, FMT-UNSUP-003; FC-041 | direct | pass |
| `FindUnsupportedSyntaxAnswersForTheListAndNotForPartOfIt` | FMT-UNSUP-009, FMT-UNSUP-003, FMT-UNSUP-007 | direct | fail |
| `FindUnsupportedSyntaxGivesTheAnswerOfThePrecheck` (3 rows) | FMT-UNSUP-009, FMT-UNSUP-007 | direct | pass (agrees on null today; must stay in agreement) |
| `OutputOfFormattedCommasIsNotUnsupported` (6 rows: argument, parameter, constraint, initializer comma moves with comments behind, trailing-comma removals) | FMT-UNSUP-005, FMT-UNSUP-009; FC-038 | direct, two passes, LF/CRLF | pass |
| `CliHiddenCodeEndToEndTests.CommentInFrontOfACommaOnALaterLineIsReportedAsUnsupported` (change doc's example; `error: …(3,21): unsupported syntax: …`, exit 2, file unchanged) | FMT-UNSUP-009 | `Program.Main` | fail (exit 1) |
| `CliHiddenCodeEndToEndTests.CommentBehindACommaOnALaterLineIsFormatted` | FMT-UNSUP-009 | `Program.Main` | pass |
| `RH5301…VerifyNoCodeFixWhenACommentHoldsACommaAtTheStartOfItsLine` | SHARED-FIX-010, FMT-UNSUP-009; FC-043 | `TryApplyCodeFixAsync` | fail (fix offered) |
| `RH5303…VerifyNoCodeFixWhenACommentHoldsACommaAtTheStartOfItsLine` | SHARED-FIX-010, FMT-UNSUP-009 | `TryApplyCodeFixAsync` | fail |
| `RH5110…VerifyFixIsOfferedWhereCommentBlocksJoin` (rewritten from `VerifyFixIsOfferedWhereCommentsBlockJoins`, `N` removed) | RH5110-003 | `Verify` | pass |
| `RH5110…VerifyNoCodeFixWhereCommentStandsInFrontOfACommaOnALaterLine` (spec `N`) | RH5110-003 | `VerifyNoCodeFixIsOffered` | fail |
| `RH5110…VerifyNoCodeFixWhereDirectiveStandsInFrontOfACommaOnALaterLine` (`#if !X`, X undefined) | RH5110-003 | `VerifyNoCodeFixIsOffered` | fail |
| `ListWrapByElementsTests.SpecificationExampleCommentsMoveWithTheirComma` (rewritten from `SpecificationExampleHeldCommasStayAndCommentsMoveWithTheirComma` to FMT-BREAK-012's new example) | FMT-BREAK-012 | `AssertRuleResult` | pass |
| `ConstraintClauseAlignmentTests.LinesPushedDownByCommentsAndDirectivesTakeTheFirstConstraintColumn` (`O` removed) | FMT-INDENT-032 | `AssertRuleResult` | pass |
| `TrailingCommaCommentOwnerTests` (5 tests; inputs changed to FMT-STRUCT-027's new example `B⏎, // comma` and `2⏎, // comma`; `OwnLineCommentInFrontOfAnInitializerCommaLeadsTheClosingBrace` renamed `TrailingCommentOfAnInitializerCommaOnTheNextLineLeadsTheClosingBrace`) | FMT-STRUCT-027 | `AssertRuleResult`, phase tree | pass |
| `ArgumentContinuationLineAlignmentTests.TupleLeadingCommaStaysAligned` (renamed from `…AfterLineCommentStaysAligned`), `TupleLeadingCommaAtBlockColumnIsAlignedWithTheElements`, `NestedListInParameterAlignmentTests.TupleTypeLeadingCommaIsAlignedWithTheElements` (renamed): `// keep` removed, an unheld tuple comma still starts its line and is aligned (checked on `main`) | FMT-INDENT-012 where tagged before | `AssertRuleResult` | pass |

Deleted, because their input is now refused (each shape is a row of the precheck class above):
- `ListWrapByElementsTests`: `CommentHeldParameterCommaStaysWhileFreeCommaMoves`, `CommentHeldParameterCommaStaysWhileParametersSharingALineAreSplit`, `CommentHeldArgumentCommaStaysWhileFreeCommaMoves`, `CommentHeldLastArgumentCommaStaysWhileArgumentsInFrontAreSplit`, `CommentHeldArgumentCommaStaysWhileArgumentsOnItsLineAreSplit`, `BlockCommentHeldArgumentCommaStaysWhileLaterArgumentsAreSplit`, `DirectiveHeldArgumentCommaStaysWhileArgumentsInFrontAreSplit`, `CommentHeldAttributeArgumentCommaStaysWhileArgumentsInFrontAreSplit`, `CommentHeldIndexerParameterCommaStaysWhileFreeCommaMoves`, `CommentHeldLambdaParameterCommaStaysWhileParametersInFrontAreSplit`, `LineBreakInsideMultiLineCommentInFrontOfCommaKeepsNothing`, `ArgumentListWithCommentHeldCommaInFinalLayoutStays`.
- `ArgumentContinuationLineAlignmentTests`: the eight `Argument…`/`ElementAccess…`/`MemberAttribute…`/`ParameterAttribute…LeadingComma…` tests, `TypeArgumentLeadingCommaStaysAlignedWithTheFirstTypeArgument`, `DictionaryKeyLeadingCommaIsAlignedWithTheKeyBody` (unheld variants of argument, element-access, and dictionary-key commas are joined or moved on `main`, so no unheld rewrite keeps a comma at a line start).
- `ParameterContinuationLineAlignmentTests`: `LeadingCommaAfterLineCommentStaysAligned`, `LeadingCommaAtBaseIndentationIsAlignedWithTheParameters`, `LeadingCommaInActiveConditionalBlockStaysAligned`, `LeadingCommaAfterDisabledTextStaysAligned`, `CommaAloneOnItsLineStaysAligned`, `CommaAfterDirectiveHeldFirstParameterStaysAligned`, `IndexerLeadingCommaAfterSharedLineIsAlignedWithTheFirstParameter`.
- `ZeroWidthTokenLineStartTests`: `OwnLineCommentAndSeparatorBehindOmittedTypeArgumentTakeBlockColumn`, `OmittedTypeArgumentBehindCommentTakesBlockColumn`.
- `StrandedDocumentationLineStartTests.LeadingCommaBehindStrayDocumentationCommentIsAlignedAsBehindSingleLineComment`, `StrandedDocumentationTokenSpacingTests.CommaBelowStrayDocumentationCommentIsAlignedWithFirstArgument`.
- `WrappedAngleBracketListTests.RefusedJoinWithLeadingCommaAlignsSeparatorsToFirstArgumentColumn`.
- `ConstraintClauseLineBreakTests`: `CommentInFrontOfCommaKeepsCommaAtLineStart`, `DirectiveInFrontOfCommaKeepsCommaAtLineStart`, `CommaBehindMultiLineCommentClosingLineDoesNotWrapClause`, `CommaBehindMultiLineCommentClosingLineInWrappedClauseIsFollowedByLineBreak`.
- `RH5107…FormatterTests.VerifyLeadingCommaAfterLineCommentIsStable`, `RH5203…FormatterTests.VerifyLeadingCommaLayoutIsStableAndNotReported` (`VerifyFormatter` does not check the refusal, so these would pass vacuously on refused input).

Scanner hits checked and kept unchanged: the analyzer-only tests of RH5107 (SHARED-JOIN-001 not-reported cases), RH5102, RH5106, RH5203, RH6002, RH8201, RH7101, RH5409 (line-based fix), RH5505 (no fix); RH7103/RH7109 (the held comma is in disabled text in the test's build, or no fix is expected); `ColonKeptOnItsOwnLineByDisabledTextWrapsClause`, `GluedCommentBlankLineTests`, `CommentBeforeBraceBlankLineTests`, `ArgumentCommaInInactiveBranchLeavesSingleLineListUnchanged` (comma only in disabled text in the parsed build); `HorizontalSpacingRewriterTests.RemovesGapBetweenMultiLineCommentAndComma` (runs `HorizontalSpacingPhase.Execute` directly, no precheck).

### Not covered, and why
- Throw sites of `SyntaxNotSupportedException` (FC for phase refusals): FMT-UNSUP-009 names no phase, so no phase throw site exists for it.
- FC-039 (nested occurrence inside an excepted region): FMT-UNSUP-009 names no excepted region; nesting is covered by the nested-initializer and complex-element rows. FC-042 (surviving guard) is the implementer's deletion list in the change doc, not a test.
- `#region` as the directive in the gap: one representative per trivia kind (`//`, `/* */`, multi-line, `///`, `/** */`, `#if`, `#else`/`#endif`, `#pragma`, disabled text); the precheck tests trivia kinds by one predicate per the spec, not per directive keyword.
- Analyzer ↔ formatter parity (FC-013) for RH5301/RH5303 on held commas: the fix is withheld (SHARED-FIX-010); the diagnostic stays by design.
- Literals assembled by interpolation or concatenation were not scanned; a remaining test with a held comma would surface as a refusal once the precheck exists.

### Commands
```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5107|FullyQualifiedName~RH5110|FullyQualifiedName~RH5301|FullyQualifiedName~RH5303|FullyQualifiedName~RH5304"
scripts/test.sh --project formatter --filter "FullyQualifiedName~UnsupportedListSeparatorCommentTests|FullyQualifiedName~InitializerLeadingCommaTests|FullyQualifiedName~TrailingCommaCommentOwnerTests|FullyQualifiedName~ListWrapByElementsTests|FullyQualifiedName~ConstraintClause|FullyQualifiedName~ArgumentContinuationLineAlignmentTests|FullyQualifiedName~NestedListInParameterAlignmentTests"
scripts/test.sh --project cli --filter "FullyQualifiedName~CliHiddenCodeEndToEndTests"
scripts/test.sh --project architecture
```

Results on this head: analyzer 23 failed / 9505 passed (exactly the new failing tests above); formatter 77 failed / 5020 passed (exactly the new failing tests and rows); CLI 1 failed / 280 passed; architecture 118 passed (`EveryRequirementOfAnEnforcedSpecIsCovered` now passes for RH5107-004, FMT-BREAK-038, FMT-UNSUP-009). Every failure was read: the formatter changes a refused input (`must return a refused input unchanged`), reports nothing, offers a fix, or writes the comma alone on its line — none fails on a compile error or unrelated layout.

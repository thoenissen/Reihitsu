Role: test implementer
Commit: 73946cd
Verdict: tests written

Prompt note: the prompt carried no intended fix, suspected cause, or hoped-for verdict. Expected values come from the spec examples of `RH5115.md` and `RH5116.md`, from SHARED-WS-003 (the join deletes the gap and nothing else), and, for every chain layout and every formatter-parity fixture, from the formatter on this commit (`scripts/format.sh`), which this run does not change (FMT-BREAK-040, SHARED-CHAIN-009). Nothing came from a patched copy of the code.

## Tests

Files (all new, formatted with `scripts/format.sh`):

- `src/Reihitsu.Analyzer.Test/Formatting/RH5115IndexAccessMustStayOnExpressionLineAnalyzerTests.cs` (`BatchCodeFixTestsBase`)
- `src/Reihitsu.Analyzer.Test/Formatting/RH5116NullForgivingOperatorMustStayOnExpressionLineAnalyzerTests.cs` (`BatchCodeFixTestsBase`)
- `src/Reihitsu.Analyzer.Test/Formatter/Formatting/RH5115IndexAccessMustStayOnExpressionLineFormatterTests.cs` (`FormatterTestsBase`)
- `src/Reihitsu.Analyzer.Test/Formatter/Formatting/RH5116NullForgivingOperatorMustStayOnExpressionLineFormatterTests.cs` (`FormatterTestsBase`)

They reference the four type names of the specs' code owners (`RH5115IndexAccessMustStayOnExpressionLineAnalyzer`, `…CodeFixProvider`, `RH5116NullForgivingOperatorMustStayOnExpressionLineAnalyzer`, `…CodeFixProvider`, namespaces `Reihitsu.Analyzer.Rules.Layout` / `Reihitsu.Analyzer.CodeFixes.Rules.Layout`, each analyzer with `public const string DiagnosticId`). On this commit the test project therefore does not compile. "Expected now" below is what I observed with the tests compiled against stub types in a throw-away worktree (an analyzer that reports nothing, a fix provider with `WellKnownFixAllProviders.BatchFixer` that registers nothing, resource strings; deleted afterwards): 112 test cases, 100 failed, 12 passed. Every failure is `Mismatch between number of diagnostics returned, expected "N" actual "0"` (or the same mismatch reported as "Diagnostics of test state" by the code-fix verifier) — the missing analyzer, the right reason. The 12 passes are the negative tests, the syntax-error suppression tests, `CodeFixProvidesFixAllProvider`, and the formatted-is-stable tests, which must pass.

LF/CRLF rows are one test method with `[DataRow("\n")]`/`[DataRow("\r\n")]`. "fail: no diag" = fails on the missing diagnostic.

### RH5115 analyzer and code fix

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| SpecExampleIndexAccessesBelowTheirExpressionAreReported | RH5115-001, RH5115-003 | `Verify(source)` | fail: no diag |
| SpecExampleOtherSideOfEachConditionIsNotReported | RH5115-001 (FC-003) | `Verify(source)` | pass |
| BracketsThatStartNoIndexAccessAreNotReported (rank specifier, collection expression, list pattern, attribute) | RH5115-001 (FC-002) | `Verify(source)` | pass |
| SpecExampleCommentOrDirectiveInTheGapIsReportedWithoutFix | RH5115-002, RH5115-006 (FC-019, FC-020) | `VerifyNoCodeFixIsOffered` | fail: no diag |
| ConditionalDirectiveInTheGapIsJudgedInTheBuildThatDefinesTheSymbol (`X` defined) | RH5115-002, RH5115-006, SHARED-DIAG-008 | `VerifyNoCodeFixIsOffered(…, Parse, "X")` | fail: no diag |
| BlockCommentEndingInFrontOfTheAccessIsReportedWithoutFix | RH5115-002, RH5115-006 (FC-021) | `VerifyNoCodeFixIsOffered` | fail: no diag |
| CommentOnALineOfItsOwnInTheGapIsReportedWithoutFix | RH5115-002, RH5115-006 (FC-021) | `VerifyNoCodeFixIsOffered` | fail: no diag |
| CommentBetweenQuestionMarkAndBracketIsReportedOnTheQuestionMarkWithoutFix | RH5115-002, RH5115-003, RH5115-006 | `VerifyNoCodeFixIsOffered` | fail: no diag |
| CommentInFrontOfTheQuestionMarkIsReportedWithoutFix | RH5115-002, RH5115-006 | `VerifyNoCodeFixIsOffered` | fail: no diag |
| DisabledTextInTheGapIsReportedWithoutFix (`#if false`) | RH5115-002, RH5115-006 (FC-020) | `VerifyNoCodeFixIsOffered` | fail: no diag |
| PragmaDirectiveInTheGapIsReportedWithoutFix | RH5115-002, RH5115-006 (FC-005, FC-020) | `VerifyNoCodeFixIsOffered` | fail: no diag |
| RegionDirectivesInTheGapAreReportedWithoutFix | RH5115-002, RH5115-006 (FC-020) | `VerifyNoCodeFixIsOffered` | fail: no diag |
| DocumentationCommentInTheGapIsReportedWithoutFixInEveryDocumentationMode (None, Parse, Diagnose) | RH5115-002, RH5115-006 (FC-040) | `VerifyNoCodeFixIsOffered(…, mode)` | fail: no diag |
| ConditionalElementAccessWithCommentOnItsExpressionLineIsNotReported (`n? /* c */ [0]`, `n /* c */ ?[0]`) | RH5115-001 (FC-003) | `Verify(source)` | pass |
| SpecExampleEachAccessIsReportedOnItsFirstTokenAndJoined (LF, CRLF; `grid⏎[0]⏎[1]`, `n⏎?⏎[0]`) | RH5115-003, RH5115-004 (FC-026) | `Verify(source, fixed)` | fail: no diag |
| SpecExampleFixJoinsAnElementAccess (LF, CRLF) | RH5115-004 | `VerifyCodeFixWithEndOfLine` | fail: no diag |
| SpecExampleFixDeletesTheGapBetweenQuestionMarkAndBracket (LF, CRLF) | RH5115-004 | `VerifyCodeFixWithEndOfLine` | fail: no diag |
| FixJoinsAConditionalElementAccessWhoseQuestionMarkStandsBelowItsExpression (LF, CRLF) | RH5115-004 | `VerifyCodeFixWithEndOfLine` | fail: no diag |
| FixJoinsAnAccessBehindBlankLines (LF, CRLF) | RH5115-001, RH5115-004 | `VerifyCodeFixWithEndOfLine` | fail: no diag |
| SpecExampleFixKeepsTheColumnOfALaterLineAlignedBehindTheAccess (LF, CRLF; 004 `c`, `.Trim` keeps its column) | RH5115-004 (FC-007, answer 5A) | `VerifyCodeFixWithEndOfLine` | fail: no diag |
| SpecExampleFixJoinsEveryAccessOutsideAChainWithLinks (LF, CRLF; whole 004 example) | RH5115-004 | `Verify(source, fixed)` | fail: no diag |
| FixJoinsAnAccessBehindAMultiLineRawStringLiteral (LF, CRLF) | RH5115-001, RH5115-004 (FC-003 token in front spans lines, FC-022) | `VerifyCodeFixWithEndOfLine` | fail: no diag |
| FixJoinsAccessesOfThisAndBase (LF, CRLF) | RH5115-001, RH5115-004 (FC-027) | `Verify(source, fixed)` | fail: no diag |
| FixJoinsAnAccessBehindAnAccessWithAStringKey (LF, CRLF) | RH5115-001, RH5115-004 (FC-027) | `VerifyCodeFixWithEndOfLine` | fail: no diag |
| FixJoinsAnAccessInsideALambda (LF, CRLF) | RH5115-001, RH5115-004 (FC-027) | `VerifyCodeFixWithEndOfLine` | fail: no diag |
| FixJoinsAnAccessInsideAnInterpolationHole (LF, CRLF) | RH5115-001, RH5115-004 (FC-027) | `VerifyCodeFixWithEndOfLine` | fail: no diag |
| FixJoinsAConditionalElementAccessBehindANullForgivingOperator (LF, CRLF; `s!⏎?[0]`) | RH5115-001, RH5115-003, RH5115-004 (FC-002) | `VerifyCodeFixWithEndOfLine` | fail: no diag |
| OnlyTheQuestionMarkBehindAWrappedNullForgivingOperatorIsReportedAndJoined (LF, CRLF; `s⏎!⏎?[0]` → `s⏎!?[0]`) | RH5115-003, RH5115-004 (FC-002) | `VerifyCodeFixWithEndOfLine` | fail: no diag |
| IndexOnTheLineOfAWrappedNullForgivingOperatorIsNotReported (`n⏎![0]`, `n⏎!?[0]`) | RH5115-001 (FC-002) | `Verify(source)` | pass |
| SpecExampleFixLaysOutTheChainBehindACall (LF, CRLF; 005 `a`) | RH5115-005 | `VerifyCodeFixWithEndOfLine` | fail: no diag |
| SpecExampleFixLaysOutTheChainInFrontOfTheFirstLink (LF, CRLF; 005 `e`) | RH5115-005 | `VerifyCodeFixWithEndOfLine` | fail: no diag |
| FixJoinsAnAccessBehindTheLastLinkOfAnAlignedChain (LF, CRLF; 001 `f`) | RH5115-005 | `VerifyCodeFixWithEndOfLine` | fail: no diag |
| FixLaysOutAChainWhoseAccessStandsBehindAWrappedNullForgivingOperator (LF, CRLF) | RH5115-005 | `VerifyCodeFixWithEndOfLine` | fail: no diag |
| FixAllLaysOutOneChainWithTwoReportedAccesses (LF, CRLF; `x.B()⏎[0]⏎[1]⏎.C()`) | RH5115-005 (FC-008) | `Verify(source, fixed)` (one at a time and Fix All) | fail: no diag |
| FixAllCorrectsAJoinedAccessInsideAChainItLaysOut (LF, CRLF; `x.B(arr⏎[0])⏎[1]⏎.C()`) | RH5115-004, RH5115-005 (FC-008) | `Verify(source, fixed)` | fail: no diag |
| ChainFixIsWithheldWhenTheChainHoldsAnUnsupportedShape (`x.B(Foo /* c */ (1))⏎[0]⏎.C()`) | RH5115-006, SHARED-FIX-010 (FC-043) | `VerifyNoCodeFixIsOffered` | fail: no diag |
| JoinIsOfferedNextToAnUnsupportedShapeOutsideTheGap (LF, CRLF; `arr⏎[0] + Foo /* c */ (1)`) | RH5115-004 (FC-043, other side) | `VerifyCodeFixWithEndOfLine` | fail: no diag |
| JoinInsideACollectionExpressionLeavesNothingForRH5305 | SHARED-FIX-004 (FC-007) | `VerifyCodeFixWithEndOfLine` + `CSharpAnalyzerVerifierTest<RH5305…>` on the result | fail: no diag |
| UnrelatedSyntaxErrorSuppressesTheWholeFile | RH5115-001, SHARED-JOIN-002 | `Verify(source, CompilerDiagnostics.None)` | pass |
| SameAccessIsReportedWithoutTheUnrelatedSyntaxError | RH5115-001, SHARED-JOIN-002 | `Verify(source)` | fail: no diag |
| FixAllCorrectsEveryDiagnostic (base; scenario `arr⏎[0]` and `n⏎?[0]`) | SHARED-FIX-001, SHARED-FIX-002 (base tags) | `GetFixAllScenario` | fail: no diag |
| CodeFixProvidesFixAllProvider (base) | — | base | pass |

### RH5116 analyzer and code fix

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| SpecExampleNullForgivingOperatorsBelowTheirExpressionAreReported | RH5116-001, RH5116-003 | `Verify(source)` | fail: no diag |
| SpecExampleOtherSideOfEachConditionIsNotReported (`!.`, `!?.`, logical negation) | RH5116-001 (FC-002, FC-003) | `Verify(source)` | pass |
| FixJoinsAnOperatorInFrontOfAnArgumentList (LF, CRLF; `f⏎!(1)`) | RH5116-001, RH5116-004 (FC-002) | `VerifyCodeFixWithEndOfLine` | fail: no diag |
| OperatorOnItsExpressionLineInFrontOfAWrappedConditionalElementAccessIsNotReported (`s!⏎?[0]`) | RH5116-001 (FC-002) | `Verify(source)` | pass |
| SpecExampleCommentInTheGapIsReportedWithoutFix | RH5116-002, RH5116-006 (FC-019) | `VerifyNoCodeFixIsOffered` | fail: no diag |
| ConditionalDirectiveInTheGapIsReportedWithoutFix | RH5116-002, RH5116-006 (FC-020) | `VerifyNoCodeFixIsOffered` | fail: no diag |
| ConditionalDirectiveInTheGapIsJudgedInTheBuildThatDefinesTheSymbol | RH5116-002, RH5116-006, SHARED-DIAG-008 | `VerifyNoCodeFixIsOffered(…, Parse, "X")` | fail: no diag |
| BlockCommentEndingInFrontOfTheOperatorIsReportedWithoutFix | RH5116-002, RH5116-006 (FC-021) | `VerifyNoCodeFixIsOffered` | fail: no diag |
| CommentOnALineOfItsOwnInTheGapIsReportedWithoutFix | RH5116-002, RH5116-006 (FC-021) | `VerifyNoCodeFixIsOffered` | fail: no diag |
| DisabledTextInTheGapIsReportedWithoutFix | RH5116-002, RH5116-006 (FC-020) | `VerifyNoCodeFixIsOffered` | fail: no diag |
| PragmaDirectiveInTheGapIsReportedWithoutFix | RH5116-002, RH5116-006 (FC-005) | `VerifyNoCodeFixIsOffered` | fail: no diag |
| RegionDirectivesInTheGapAreReportedWithoutFix | RH5116-002, RH5116-006 (FC-020) | `VerifyNoCodeFixIsOffered` | fail: no diag |
| DocumentationCommentInTheGapIsReportedWithoutFixInEveryDocumentationMode (None, Parse, Diagnose) | RH5116-002, RH5116-006 (FC-040) | `VerifyNoCodeFixIsOffered(…, mode)` | fail: no diag |
| SpecExampleOnlyTheOperatorInFrontOfAWrappedIndexIsReportedAndJoined (LF, CRLF; `s⏎!⏎?[0]` → `s!⏎?[0]`) | RH5116-003, RH5116-004 | `VerifyCodeFixWithEndOfLine` | fail: no diag |
| SpecExampleFixJoinsTheOperator (LF, CRLF) | RH5116-004 | `VerifyCodeFixWithEndOfLine` | fail: no diag |
| SpecExampleFixKeepsTheColumnOfALaterLineAlignedBehindTheOperator (LF, CRLF; `2` keeps its column) | RH5116-004 (FC-007, answer 5A) | `VerifyCodeFixWithEndOfLine` | fail: no diag |
| SpecExampleFixJoinsEveryOperatorOutsideAChainWithLinks (LF, CRLF) | RH5116-004 | `Verify(source, fixed)` | fail: no diag |
| FixJoinsAnOperatorBehindBlankLines (LF, CRLF) | RH5116-001, RH5116-004 | `VerifyCodeFixWithEndOfLine` | fail: no diag |
| FixJoinsOperatorsInFrontOfAnIndexAndAConditionalElementAccess (LF, CRLF) | RH5116-001, RH5116-004 | `Verify(source, fixed)` | fail: no diag |
| FixJoinsOperatorsInsideAHoleAnArgumentAndALambda (LF, CRLF) | RH5116-001, RH5116-004 (FC-027) | `Verify(source, fixed)` | fail: no diag |
| SpecExampleFixLaysOutTheChain (LF, CRLF) | RH5116-005 | `VerifyCodeFixWithEndOfLine` | fail: no diag |
| FixLaysOutAChainWhoseOperatorStandsInFrontOfAWrappedIndex (LF, CRLF; `s⏎!⏎[0].ToString()⏎.Trim()`) | RH5116-005 | `VerifyCodeFixWithEndOfLine` | fail: no diag |
| ChainFixIsWithheldWhenTheChainHoldsAnUnsupportedShape | RH5116-006, SHARED-FIX-010 (FC-043) | `VerifyNoCodeFixIsOffered` | fail: no diag |
| UnrelatedSyntaxErrorSuppressesTheWholeFile | RH5116-001, SHARED-JOIN-002 | `Verify(source, CompilerDiagnostics.None)` | pass |
| SameOperatorIsReportedWithoutTheUnrelatedSyntaxError | RH5116-001, SHARED-JOIN-002 | `Verify(source)` | fail: no diag |
| FixAllCorrectsEveryDiagnostic (base; scenario `s⏎! + ""` and `n⏎![0]`) | SHARED-FIX-001, SHARED-FIX-002 (base tags) | `GetFixAllScenario` | fail: no diag |
| CodeFixProvidesFixAllProvider (base) | — | base | pass |

### Formatter parity (FC-013, FC-027)

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH5115 … VerifyFormatterJoinsReportedAccessesInMemberContexts (field and property initializer, expression-bodied property, constructor initializer, `this⏎[0]`) | RH5115-001, SHARED-DIAG-010 | `VerifyFormatter(source, fixed)` | fail: no diag |
| RH5115 … VerifyFormatterJoinsReportedAccessesInExpressionContexts (18 contexts: switch arm, query `in`, lambda, hole with format, raw interpolated hole, `ref`, cast, `[^1]`, `[1..]`, `d["k"]`, array creation, conditional, `foreach`, tuple target, array size, raw string, `if`, `return`) | RH5115-001, SHARED-DIAG-010 | `VerifyFormatter(source, fixed)` | fail: no diag |
| RH5115 … VerifyFormatterJoinsReportedAccessesInChainsWithLinks | RH5115-001, SHARED-DIAG-010 | `VerifyFormatter(source, fixed)` | fail: no diag |
| RH5115 … VerifyFormatterJoinsAReportedAccessInAFixedStatementHeader | RH5115-001, SHARED-DIAG-010 | `VerifyFormatter(…, AllowUnsafe)` | fail: no diag |
| RH5115 … VerifyFormattedContextsStayUnchangedAndUnreported | RH5115-001, SHARED-DIAG-010 | `VerifyFormatter(source)` | pass |
| RH5116 … VerifyFormatterJoinsReportedOperatorsInEnclosingContexts (field, expression body, `is` pattern, lambda, hole, argument list, index, `?[`, binary, `if`, `throw`) | RH5116-001, SHARED-DIAG-010 | `VerifyFormatter(source, fixed)` | fail: no diag |
| RH5116 … VerifyFormatterJoinsAReportedOperatorInAChainWithLinks | RH5116-001, SHARED-DIAG-010 | `VerifyFormatter(source, fixed)` | fail: no diag |
| RH5116 … VerifyFormattedContextsStayUnchangedAndUnreported | RH5116-001, SHARED-DIAG-010 | `VerifyFormatter(source)` | pass |

`VerifyFormatter` covers LF and CRLF, second-pass idempotency, and no diagnostic on the formatted output.

Coverage of `sdd/specs/requirement-ids.txt`: every ID RH5115-001 to RH5115-006 and RH5116-001 to RH5116-006 has at least one tagged test.

Existing tests whose asserted output this change alters: none found. Each analyzer test runs only its own analyzer, and the formatter does not change. I searched the test projects for `RH5114` (to find rule-ID lists) and found only RH5114's own tests. `AnalyzerPackageMetadataTests`, `RuleDocumentationExampleTests`, and the architecture tests (`AnalyzerSpecHeaderMatchesItsRule`, `EveryCodeOwnerNamesAnExistingType`, `EveryRequirementOfAnEnforcedSpecIsCovered`) pick up the new rules automatically. They pass once the types, resources, `Release.ruleset` entries, and the package README rows exist.

## Not covered, and why

- **FC-015 (predicate copy, decision #1).** No Core unit test was written for the moved attached-part predicate. Its member shape after the move is the implementer's choice. Its behavior is already covered through the formatter's FMT-BREAK-040 and FMT-UNSUP-014 tests, and through the RH5115/RH5116 tests above, which cover the qualifier cases where a copy would drift: `!.`/`!?.` vs `!?[`, `s!⏎?[0]`, `s⏎!⏎?[0]`, `f⏎!(1)`. Whether the analyzers use the one predicate is a structural property for the auditor, not a test.
- **Q5/FC-007 follow-on diagnostics (RH5201 on `.Trim`, RH5203 on `2`).** Not asserted. The spec states it informatively, and the skeptic observed it with `apply-fix.sh`. The tests lock in the 5A output: the later line keeps its column.
- **FC-004, FC-021 (fix drops a comment).** Covered by the no-fix tests. The fix is withheld for every trivia class in the gap, so it has no output to check for a dropped comment.
- **FC-006 (meaning or compile change).** Every `Verify(source, fixed)` compiles the fixed source. The join changes no token.
- **FC-009, FC-030.** Every offered fix is checked against an exact expected text. The join tests check that the fix changes nothing outside the gap (004 `c`, 004 `b`).
- **FC-010, FC-011, FC-012, FC-028, FC-038, FC-039, FC-041, FC-042.** These are formatter-phase and precheck classes. This run changes no formatter output and declares no shape, so they cannot be reached. The predicate move is guarded by the existing formatter suites.
- **FC-018, FC-023, FC-024, FC-025, FC-029, FC-036.** No rename, no declaration-kind dimension, and no key/value layout. FC-025 (expression bodies) is covered as a context: the expression-bodied property, indexer, and lambda in the parity fixtures.
- **FC-044.** The fix's guard for RH5115-006/RH5116-006 is a trivia check on the gap. No computed result is modelled.
- **Query clause layout (`from v in arr⏎[0..1]⏎select v`).** The query context keeps `select` on the `from` line, because the formatter's column for a wrapped `select` is the skeptic's separate follow-up candidate.
- **Interaction with RH5305.** Covered for the RH5115 join result only (the skeptic's input). No `!` inside a collection expression was tried. The mechanism is the same gap deletion.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5115|FullyQualifiedName~RH5116"
scripts/test.sh --project architecture
```

## Follow-up candidates

_None._

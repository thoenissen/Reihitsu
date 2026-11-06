Role: test implementer
Commit: bd9283a
Verdict: #1136: tests written; #1134: reproduced; #1135: reproduced; #1262: reproduced; #1332: reproduced; #1336: reproduced (B3 not reproduced); #1340: reproduced

Inputs: the seven issues (bodies; the comments of #1262), `00-lead.md`, `01`–`05` handovers, both accepted change docs, and the specs they name. The prompt carried no intended fix, suspected cause, or hoped-for verdict. All tests are in `src/Reihitsu.Analyzer.Test`. "Fails" below means it fails on bd9283a for the issue's own difference (a fix is offered where none may be, the analyzer reports where it may not, or the fix output differs as stated). "Passes" means it passes today and must keep passing. Every failing test was checked for its failure message; none fails on a compile error or on unrelated layout.

Test infrastructure changed (test code only): `AnalyzerTestsBase<TAnalyzer, TCodeFix>` gained `VerifyNoCodeFixIsOffered(source, DocumentationMode, params string[] preprocessorSymbols)` (analyzer and code-fix check both run with that mode and those symbols) and a `GetCodeFixActionsAsync` overload with a `DocumentationMode`. The existing overloads keep their behavior (`CreateTestDocument` still parses in mode `Parse` by default).

## thoenissen/Reihitsu#1136

### Tests

`RH7004UsingDeclarationsShouldNotBeUsedAnalyzerTests`:

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `NoCodeFixWhenTheNewBlockWouldEndInsideAConditionalPairWithTheSymbolDefined` | SHARED-FIX-003 | `VerifyNoCodeFixIsOffered(…, Diagnose, "DEBUG")` | fails |
| `NoCodeFixWhenTheNewBlockWouldEndInFrontOfAConditionalPairWithTheSymbolUndefined` | SHARED-FIX-003 | `VerifyNoCodeFixIsOffered` | fails |
| `FixMovesABalancedConditionalPairOfTheFollowingStatementsIntoTheBlockWithTheSymbolDefined` | SHARED-FIX-003 | `Verify` + DEBUG transform | passes |
| `FixMovesABalancedConditionalPairOfTheFollowingStatementsIntoTheBlockWithTheSymbolUndefined` (disabled text kept unchanged; its indentation is asserted as today, 8 spaces) | SHARED-FIX-003 | `Verify` | passes |
| `FixIsOfferedForAPragmaPairAroundTheDeclarationAndEveryFollowingStatement` (both braces between the pair) | SHARED-FIX-003 | `Verify` | passes |
| `NoCodeFixWhenTheNewBlockWouldSplitAPragmaPair` (one brace between) | SHARED-FIX-003 | `VerifyNoCodeFixIsOffered` | fails |
| `NoCodeFixWhenTheNewBlockWouldSplitANullablePair` | SHARED-FIX-003 | `VerifyNoCodeFixIsOffered` | fails |
| `NoCodeFixWhenTheNewBlockWouldSplitARegionPair` | SHARED-FIX-003 | `VerifyNoCodeFixIsOffered` | fails |

The RH7501 SHARED-FIX-003 examples (pragma pair, other build configuration) are under #1332; the RH7004 comment and the RH7309 comment examples under #1336 and #1262.

### Scenarios

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| RH7004 `#if DEBUG s.Flush(); #endif` after the declaration, DEBUG defined / undefined | LF | code fix | reproduced | fix offered vs withheld |
| RH7004 pragma / nullable / region pair, one directive in front of the following statement, partner in front of `}` | LF | code fix | reproduced | fix offered vs withheld |

### Not covered, and why

- Brace-split member "pair wholly outside both braces" (a pair in front of the declaration): the test I wrote (`#region Setup … #endregion` in front of the declaration) showed a pre-existing, unrelated defect — the fix inserts a blank line in front of `#endregion` and one behind it (SHARED-FIX-006 *Blank lines*). I removed that test rather than lock either output in; follow-up candidate below. The "both between" member is covered (pragma test above).
- `#if`/`#nullable` pairs in other fixes (RH3202–RH3208, RH5405–RH5407): outside the run (skeptic's audit follow-up).

## thoenissen/Reihitsu#1134

### Tests

`RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsAnalyzerTests`:

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `NoCodeFixForTheFieldsOfAStructWithDefaultLayout` (rewritten from the green characterization `FixReordersTheFieldsOfAStructWithDefaultLayout`, which asserted the swap) | SHARED-FIX-003, RH7109-006 | `VerifyNoCodeFixIsOffered` | fails |
| `NoCodeFixForTheSpecificationExampleStruct` | SHARED-FIX-003, RH7109-006 | same | fails |
| `NoCodeFixForTheFieldsOfARecordStruct` | SHARED-FIX-003, RH7109-006 | same | fails |
| `NoCodeFixForTheFieldsOfARefStruct` | SHARED-FIX-003, RH7109-006 | same | fails |
| `NoCodeFixForTheFieldsOfAClassWithSequentialLayout` | SHARED-FIX-003, RH7109-006 | same | fails |
| `NoCodeFixForTheFieldsOfARecordWithSequentialLayout` | SHARED-FIX-003, RH7109-006 | same | fails |
| `NoCodeFixForAClassWithAFullyQualifiedSequentialLayoutAttribute` (`System.Runtime.InteropServices.StructLayout(…, Pack = 1)`) | SHARED-FIX-003, RH7109-006 | same | fails |
| `NoCodeFixForTheFieldsOfAStructNestedInAClass` | SHARED-FIX-003, RH7109-006 | same | fails |
| `NoCodeFixForTheFieldsOfAPartOfAPartialStruct` | SHARED-FIX-003, RH7109-006 | same | fails |
| `FixReordersTheStaticFieldsOfAStruct` (spec example) | SHARED-FIX-003 | `Verify` | passes |

Existing counter-side tests that stay: `FixReordersTheFieldsOfAnExplicitLayoutStructAndKeepsEachOffset`, `FixReordersTheFieldsOfAnAutoLayoutStruct`, `FixReordersTheFieldsOfAClassWithoutLayoutAttribute` (all pass).

### Scenarios

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| issue input (default struct) and the eight sibling shapes above | LF | code fix | reproduced | fields swapped vs fix withheld |

### Not covered, and why

Line endings: the guard is a type-level predicate that reads no line structure; one LF case per shape. Fix All: a withheld fix has no Fix All to test; the batch scenario of the class is unchanged.

## thoenissen/Reihitsu#1135

### Tests

`RH7309RegionsShouldFollowCategoryOrderAnalyzerTests`:

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `NoCodeFixWhenAnInitializerReadsAFieldOfABlockItWouldCross` (issue input, spec example) | RH7309-006 | `VerifyNoCodeFixIsOffered` | fails |
| `NoCodeFixWhenAnInitializerWouldChangeOrderWithAMemberBetweenTheBlocks` | RH7309-006 | same | fails |
| `NoCodeFixWhenTwoInstanceInitializersWouldChangeOrder` | RH7309-006 | same | fails |
| `NoCodeFixWhenAnEventInitializerAndAPropertyInitializerWouldChangeOrder` | RH7309-006 | same | fails |
| `NoCodeFixWhenAnInitializerInDisabledTextWouldChangeOrder` (without DEBUG) | SHARED-FIX-003 | same | fails |
| `CodeFixReordersAStaticAndAnInstanceInitializer` | RH7309-006 | `Verify` | passes |
| `CodeFixReordersConstantFields` | RH7309-006 | `Verify` | passes |
| `CodeFixReordersInitializersOfNestedTypes` | RH7309-006 | `Verify` | passes |
| `NoCodeFixForTheInstanceFieldsOfAStruct` (spec example) | RH7309-006 | `VerifyNoCodeFixIsOffered` | fails |
| `NoCodeFixForAnAutoPropertyOfAStruct` (spec example) | RH7309-006, SHARED-FIX-003 | same | fails |
| `NoCodeFixForAnInstanceFieldLikeEventOfAStruct` | RH7309-006, SHARED-FIX-003 | same | fails |
| `NoCodeFixForAPropertyThatUsesTheFieldKeywordOfAStruct` | RH7309-006, SHARED-FIX-003 | same | fails |
| `NoCodeFixForTheInstanceFieldsOfAClassWithSequentialLayout` | RH7309-006 | same | fails |

### Scenarios

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| issue input | LF | code fix | reproduced | fix offered (moves `Fields` in front) vs withheld |

### Not covered, and why

Regions of a nested enum or inside an expression: out of the run (maintainer answer 1, 4C).

## thoenissen/Reihitsu#1262

### Tests

| Test | Class | Requirement / failure class | Helper | Expected now |
|---|---|---|---|---|
| `NoCodeFixForRegionsThatReachBetweenDeclarations` (spec example, both shapes) | RH7303 | RH7303-005 | `VerifyNoCodeFixIsOffered` | fails |
| `NoCodeFixForAnEndRegionInABodyWhoseRegionLiesBetweenDeclarations` (issue shape) | RH7303 | RH7303-005 | same | fails |
| `NoCodeFixForARegionInABodyWhoseEndRegionLiesBetweenDeclarations` (mirror) | RH7303 | RH7303-005 | same | fails |
| `RegionsWithADirectiveInsideExecutableCodeAreNotReported` (spec example) | RH7304 | RH7304-005 | `Verify` | fails (2 reported) |
| `RegionThatHoldsARegionOfAMethodBodyIsNotReported` (Outer/Inner) | RH7304 | RH7304-005 | `Verify` | fails (2 reported) |
| `RegionClosedInsideAMethodBodyIsNotReported` | RH7304 | RH7304-005 | `Verify` | passes |
| `VerifyExcludedElementBodyDirectiveCannotPairWithContainingTypeDirective` (changed, see below) | RH7304 | RH7304-005 | `Verify` | fails (2 reported) |
| `OfferedFixKeepsTheCodeParseableWhenACommentPrecedesTheFirstToken` (`/* c */ int _a;`) | RH7304 | SHARED-FIX-003 | every offered fix, result parsed | fails (`/* c */ #region Fields`) |
| `OfferedFixKeepsTheCodeParseableWhenACommentThatSpansLinesEndsInFrontOfTheFirstToken` | RH7304 | SHARED-FIX-003 | same | fails |
| `NoCodeFixWhenNothingRemainsOfTheDescription` (spec example, incl. `//Implementation`) | RH7306 | RH7306-004 | `VerifyNoCodeFixIsOffered` | fails |
| `NoCodeFixWhenNothingWouldRemainOfThePartnerDescription` (spec example) | RH7306 | RH7306-004 | same | fails |
| `NoCodeFixWhenNothingWouldRemainOfAPartnerDescriptionWithoutASpace` (`#endregion //implementation`) | RH7306 | RH7306-004 | same | fails |
| `CodeFixRemovesTheTrailingWordFromAPartnerWithoutASpaceBehindTheCommentMarker` | RH7306 | RH7306-003 | `Verify` | passes |
| `NoCodeFixWhenACommentStandsDirectlyAboveABlockThatWouldMove` (spec example) | RH7309 | RH7309-006 | `VerifyNoCodeFixIsOffered` | fails |
| `…ThatWouldMoveInCarriageReturnLineFeedDocument` | RH7309 | RH7309-006 | same, CRLF | fails |
| `NoCodeFixWhenACommentAboveABlockThatWouldMoveIsSeparatedByABlankLine` | RH7309 | RH7309-006 | same | fails |
| `CodeFixKeepsACommentAboveABlockThatKeepsItsPlace` (spec example) | RH7309 | RH7309-004, RH7309-006 | `Verify` | passes |
| `NoCodeFixWhenADocumentationCommentBehindTheLastCodeOfABlockDocumentsTheNextBlock` (shape 1) | RH7309 | RH7309-006 | `VerifyNoCodeFixIsOffered` | fails |
| `NoCodeFixWhenADocumentationCommentAboveTheRegionLineDocumentsTheBlock` (shape 2) | RH7309 | RH7309-006 | same | fails |
| `…DocumentsTheBlockInDocumentationModeDiagnose` | RH7309 | RH7309-006 | same, mode `Diagnose` | fails |
| `NoCodeFixWhenADocumentationCommentAboveTheEndRegionLineDocumentsTheNextBlock` (shape 3) | RH7309 | RH7309-006 | same | fails |
| `CodeFixMovesAnOrdinaryCommentBehindTheLastCodeOfABlockWithTheBlockInDocumentationModeNone` (spec example) | RH7309 | RH7309-006 | `Verify`, mode `None` | passes |

Changed existing test: `RH7304…Tests.VerifyExcludedElementBodyDirectiveCannotPairWithContainingTypeDirective` asserted two RH7304 reports (`#region Outer`, `#endregion // Body`) for an input in which `#region Body` lies inside a method body. Under RH7304-005 (accepted change doc `silence-uncorrectable-namespaces-and-regions.md`) neither pair is reported, so the test now asserts no report and carries the tag. The changed visible output is demanded by that accepted requirement.

### Scenarios

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| RH7303 straddling region, both directions | LF | code fix | reproduced | fix offered (removes the unreported partner) vs withheld |
| RH7304 `/* c */` in front of the first token | LF | code fix | reproduced | result `    /* c */ #region Fields` does not parse |
| RH7304 Outer/Inner | LF | analyzer | reproduced | 2 reports vs none |
| RH7304 type-level `#region` closed in a body, alone | LF | analyzer | not reproduced | no report today, as required |
| RH7306 `Implementation` alone, `//Implementation`, partner `// implementation` | LF | code fix | reproduced | fix offered vs withheld |
| RH7309 comment above a moved block (with and without blank line), three `///` shapes | LF, CRLF (one) | code fix | reproduced | fix offered vs withheld |

### Not covered, and why

- RH7304 with `// c` or `///` on the line above the first token, or an attribute first: offered and correct today (skeptic); the guard under test is "text in front of the first token on its line", and both sides are covered by the two parse tests and the existing fix tests.
- RH7309 regions of a nested enum or inside an expression: out of the run (4C).

## thoenissen/Reihitsu#1332

### Tests

`RH7501BreakStatementsShouldNotBeInsideExplicitSwitchCaseBlocksAnalyzerTests`. Seven green characterization tests on bd9283a asserted the issue's observed outputs; each was rewritten to the specified outcome (renames in brackets):

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `NoCodeFixWhenExcludedCodeLiesInFrontOfTheBreak` [was `VerifyCodeFixMovesExcludedCodeInFrontOfBreakBehindBlock`] | RH7501-004, SHARED-FIX-003 | `VerifyNoCodeFixIsOffered` | fails |
| `NoCodeFixWhenConditionalCodeLiesInFrontOfTheBreakWithTheSymbolDefined` | RH7501-004, SHARED-FIX-003 | same, `DEBUG` | fails |
| `NoCodeFixWhenARegionEnclosesTheBreak` [was `VerifyCodeFixSplitsRegionAroundBreak`] | RH7501-004, SHARED-FIX-003 | same | fails |
| `NoCodeFixWhenCodeOfAnotherBuildConfigurationFollowsTheBreak` [was `VerifyCodeFixLeavesConditionalCodeBehindBreakInsideBlock`] | RH7501-004, SHARED-FIX-003 | same | fails |
| `NoCodeFixWhenAPragmaPairEnclosesTheBreak` [was `VerifyCodeFixSeparatesPragmaPairAroundBreak`] | RH7501-004, SHARED-FIX-003 | same | fails |
| `NoCodeFixWhenTheSectionBlockSharesTheLineOfItsCaseLabel` [was `VerifyCodeFixOnSectionBlockOnOneLine`] | RH7501-004 | same | fails |
| `NoCodeFixWhenTheBreakSharesItsLineWithTheClosingBrace` [was `VerifyCodeFixWhenBreakSharesLineWithClosingBrace`] | RH7501-004 | same | fails |
| `NoCodeFixWhenTheBreakSharesItsLineWithTheStatementInFrontOfIt` [was `VerifyCodeFixWhenBreakSharesLineWithStatementInFrontOfIt`] | RH7501-004 | same | fails |
| `…InFrontOfItInCarriageReturnLineFeedDocument` | RH7501-004 | same, CRLF | fails |
| `NoCodeFixForTheSharedLineSpecificationExample` (incl. `A(); /* first */ break;`) | RH7501-004 | same | fails |
| `NoCodeFixForTheDirectiveSpecificationExample` | RH7501-004 | same | fails |
| `NoCodeFixWhenANullableDirectiveStandsInFrontOfTheBreak` | RH7501-004 | same | fails |
| `VerifyCodeFixLeavesEmptyBlockWhenBreakIsOnlyStatement` (assertion unchanged; now tagged, summary states the decided behavior) | RH7501-004 | `ApplyCodeFixAsync` | passes |

`VerifyCodeFixMovesBlankLineInFrontOfBreakWithIt` is left as it is (thoenissen/Reihitsu#1437, outside the run). `VerifyCodeFixKeepsCommentBetweenBreakAndClosingBraceInsideBlock` (comment on its own line between the `break` and `}` stays in the block) passes and is the counter-side of SHARED-FIX-003 condition 4 for this fix.

### Scenarios

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| Q7 inputs 1–4, Q8 inputs 1–3 | LF; CRLF for Q8 input 3 | code fix | reproduced | fix offered (broken or relocating output, as in the old characterizations) vs withheld |
| Q9 lone `break` | LF | code fix | holds | empty block kept, as required |

### Not covered, and why

The SHARED-FIX-003 example "code of another build configuration" with `DEBUG` defined: the `break` is then followed by `A();` and is not reported at all (RH7501-001), so there is no fix to withhold; only the undefined configuration is tested, as the spec example states.

## thoenissen/Reihitsu#1336

### Tests

| Test | Class | Requirement / failure class | Helper | Expected now |
|---|---|---|---|---|
| `FixKeepsACommentBehindTheDeclarationOnTheHeaderLine` (A2, spec example) | RH7004 | RH7004-004 | `Verify` | fails (`} // resource`) |
| `FixKeepsACommentBehindTheDeclarationOnTheHeaderLineUnderBothLineEndings` | RH7004 | RH7004-004 | `ApplyCodeFixAsync`, LF + CRLF | fails |
| `OfferedFixKeepsACommentBehindTheUsingKeyword` (`using /* c */ var …`) | RH7004 | SHARED-FIX-003 | every offered fix, comment present | fails (comment dropped) |
| `OfferedFixKeepsACommentInFrontOfTheSemicolon` | RH7004 | SHARED-FIX-003 | same | fails (comment dropped) |
| A1 = the #1136 RH7004 conditional-pair tests | RH7004 | SHARED-FIX-003 | | fails |
| `DocumentedDeclarationsAreReportedWithoutAFix` (spec example) | RH7101 | RH7101-006 | `VerifyNoCodeFixIsOffered` | fails |
| `DocumentedDeclarationsAreReportedWithoutAFixInDocumentationModeDiagnose` | RH7101 | RH7101-006 | same, mode `Diagnose` | fails |
| `DocumentationCommentInFrontOfTheAttributesOfTheDeclarationWithholdsTheFix` | RH7101 | RH7101-006 | same | fails |
| `DocumentationCommentInTheGapInFrontWithholdsTheFix` (maintainer answer 4 input) | RH7101 | RH7101-006 | same | fails |
| eight `DocumentationComment…InTheGapInFrontWithholdsTheFix[AlsoWithOneInTheGapBehind]` (multi-line and preceded-by-comment forms) | RH7101 | RH7101-006 | same | fail |
| `FixSplitsDeclarationsWithDocumentationStyleCommentsInDocumentationModeNone` (spec example) | RH7101 | RH7101-004, RH7101-006 | `Verify`, mode `None` | passes |
| `FixKeepsAnOrdinaryCommentInTheGapInFrontBehindTheMemberInFrontInDocumentationModeNone` | RH7101 | RH7101-004, RH7101-006 | `Verify` + LF/CRLF, mode `None` | passes |
| `OrdinaryCommentInFrontOfTheDeclarationDoesNotWithholdTheFix` (spec example) | RH7101 | RH7101-006 | `Verify` | passes |
| `FixLaysOutDocumentationCommentsInFrontOfLaterDeclaratorsAsTheFormatterLaysThemOut` (existing = later-declarator spec example; tag added) | RH7101 | RH7101-004, RH7101-006 | `Verify` | passes |
| `NoCodeFixForACombinedDeclarationInABlockScopedNamespace` (B3) | RH7101 | SHARED-FIX-005 | `GetCodeFixActionsAsync` | passes |
| `NoCodeFixForACombinedDeclarationInAFileScopedNamespace` (B3) | RH7101 | SHARED-FIX-005 | same | passes |
| `DirectiveBehindTheLastModifierWithholdsTheFix` (spec example, `#pragma`) | RH7105 | SHARED-MODORDER-003 | `VerifyNoCodeFixIsOffered` | fails |
| `CommentBetweenTheLastModifierAndTheTypeWithholdsTheFix` (spec example, both members) | RH7105 | SHARED-MODORDER-003 | same | fails |
| `NullableDirectiveBehindTheLastModifierWithholdsTheFix` | RH7105 | SHARED-MODORDER-003 | same | fails |
| `ConditionalDirectivesBehindTheLastModifierWithholdTheFix` (`#if`/`#else`/`#endif`) | RH7105 | SHARED-MODORDER-003 | same | fails |
| `RegionDirectiveBehindTheLastModifierWithholdsTheFix` | RH7105 | SHARED-MODORDER-003 | same | fails |
| `DirectiveBehindTheLastModifierOfATypeWithholdsTheFix` (`class D`) | RH7105 | SHARED-MODORDER-003 | same | fails |
| `DirectiveBehindTheLastModifierWithholdsTheFix` (`internal protected`) | RH7106 | SHARED-MODORDER-003 | same | fails |
| `CommentBetweenTheLastModifierAndTheTypeWithholdsTheFix` | RH7106 | SHARED-MODORDER-003 | same | fails |

Changed existing RH7101 tests, as maintainer answer 4 allows:

- Nine tests whose reported declaration has a `/** */` comment starting behind the code of the member in front, in the declaration's leading trivia (main's case 7 and its multi-line and preceded-by-comment variants), now assert that no fix is offered in modes `Parse`/`Diagnose`, are renamed to `DocumentationComment…WithholdsTheFix…`, and are tagged RH7101-006 instead of RH7101-004. Old names: `FixMovesADocumentationCommentInTheGapInFrontWithTheFirstField`, `FixStartsTheMembersBehindADocumentationCommentThatSpansLinesInTheGapsOnTheNextLine`, `FixStartsTheFirstFieldBehindADocumentationCommentThatSpansLinesInTheGapInFrontOnTheNextLine`, `FixStartsTheMembersBehindACommentThatSpansLinesAndADocumentationCommentInTheGapsOnTheNextLine`, `FixStartsTheFirstFieldBehindACommentThatSpansLinesAndADocumentationCommentInTheGapInFrontOnTheNextLine`, `FixStartsTheMembersBehindAOneLineCommentAndADocumentationCommentThatSpansLinesInTheGapsOnTheNextLine`, `FixStartsTheFirstFieldBehindAOneLineCommentAndADocumentationCommentThatSpansLinesInTheGapInFrontOnTheNextLine`, `FixStartsTheMembersBehindACommentThatSpansLinesAndADocumentationCommentThatSpansLinesInTheGapsOnTheNextLine`, `FixStartsTheFirstFieldBehindACommentThatSpansLinesAndADocumentationCommentThatSpansLinesInTheGapInFrontOnTheNextLine`. Their formatter assertions (`AssertFormattingAddsOnlyBlankLines`) went with them, because no fix result exists any more. The layout of a documentation comment in the gap *behind* the declaration keeps its own tests (`…InTheGapBehind…`), which are unaffected.
- Seven tests whose documented declaration splits with an unchanged result in mode `None` were moved to mode `None` with their assertions unchanged: `VerifyDocumentedCombinedFieldsAreFixedAtMemberIndentation`, `VerifyMisalignedDocumentationCommentFixUsesFieldLineIndentation`, `VerifyDocumentationCommentOnOpeningBraceLineFixUsesFieldLineIndentation`, `FixLeavesDocumentationCommentsThatSpanLinesInFrontOfMembersThatStartTheirLinesWhereTheyStand`, `FixLeavesSingleLineDocumentationCommentsInFrontOfMembersThatStartTheirLinesWhereTheyStand`, `FixLeavesDocumentationCommentsThatStartTheirLinesInFrontOfMembersOnTheSameLineWhereTheyStand`, `FixLeavesTheFirstFieldBehindADocumentationCommentThatStartsItsLineInTheGapInFrontOnTheTargetsLine`. The last four have a `///` or `/** */` behind `_p;` which Roslyn puts into the leading trivia of the next declaration in mode `Parse` (checked with a throw-away probe; `/// a` behind code is `SingleLineDocumentationCommentTrivia` of the following field), so they would be withheld in `Parse`.

The mode-`None` output for main's case-7 input, `private int _p; /** X. */` followed by the two fields, is RH7101-004's "a comment between them stays behind the code in front of it", observed today unchanged.

### Scenarios

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| A1 (RH7004 `#if DEBUG` pair) | LF | code fix | reproduced | see #1136 |
| A2 (RH7004 comment behind declaration) | LF, CRLF | code fix | reproduced | `} // resource` vs header line |
| B3 (`namespace N { int a, b; }`, `namespace N; int a, b;`) | LF, CRLF | code fix (`scripts/apply-fix.sh RH7101`, and tests) | **not reproduced** | `no-fix-offered` on bd9283a for both forms and both endings; the issue's `no-progress` no longer occurs after `main`'s RH7101 change (fix offered where the formatter splits, FMT-PIPE-031). Tests kept as characterization of SHARED-FIX-005 |
| N2 (RH7105/RH7106 directive or comment behind the last modifier; `#pragma`, `#nullable`, `#if`/`#else`, `#region`, `class D`, `internal protected`) | LF | code fix | reproduced | fix offered vs withheld |
| RH7101 documented declaration, `///`, `/** */`, with attribute, behind-code form | LF; CRLF for the mode-`None` behind-code form | code fix | reproduced | split vs withheld |

### Not covered, and why

- RH7004 `await /* c */ using …`: kept today (skeptic), the `await` keyword is reused; not a member of the failing mechanism.
- RH7105/RH7106 with the comment or directive tested on one declaration kind each beyond the issue's `class D`: the guard reads the trivia in front of the token after the last modifier, whatever that token is.

## thoenissen/Reihitsu#1340

### Tests

`RH7003FileScopedNamespacesShouldBeUsedAnalyzerTests`:

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `NamespaceThatContainsANamespaceIsNotReported` (spec example) | RH7003-005 | `Verify` | fails (1 reported) |
| `NamespaceThatContainsSeveralNamespacesIsNotReported` (spec example) | RH7003-005 | `Verify` | fails (1 reported) |
| `NamespaceThatContainsATypeAndANamespaceIsNotReported` | RH7003-005 | `Verify` | fails (1 reported) |

The counter-side (a namespace holding only types is reported) is covered by the existing `VerifySingleTopLevelBlockScopedNamespaceIsDetected`.

### Scenarios

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| both issue inputs | LF | analyzer | reproduced | reported vs not reported |

### Not covered, and why

No code fix exists. Line endings: the analyzer decides on syntax members only. The nested namespace in an excluded `#if` branch is a follow-up candidate (02-skeptic), not decided by the run.

## Whole run

### Failure classes

- Covered: FC-002 (layout qualifier, partner judged by description), FC-003 (each guard tested on both sides: layout vs Auto/Explicit/static/plain class; initializer contexts; moved vs kept block; doc mode `Parse`/`Diagnose` vs `None`; leading-trivia vs later-declarator vs ordinary comment; brace one-between vs both-between; lone `break` vs shared line; description alone vs with text), FC-004/FC-019/FC-020/FC-021 (comments and `#if`/`#pragma`/`#nullable`/`#region` in each gap a fix touches), FC-005 (MODORDER comment and directive tested separately), FC-009 (RH7306 `//Implementation`, RH7101 namespace), FC-016/FC-024/FC-027 (record struct, ref struct, partial struct, nested struct, multi-line comments, backing fields, member between blocks), FC-033 (CRLF variants for RH7004 comment, RH7101 mode-`None` layout, RH7309 comment guard, RH7501 shared line; DEBUG defined and undefined).
- FC-007 convergence: every `Verify` with a fixed source runs the verifier's iterations and Fix All. FC-008: no new fixed outputs with co-occurring diagnostics besides the RH7101 spec example in mode `None` (two diagnostics, Fix All covered by `Verify`).
- Not applicable: FC-018/FC-036 (no rename), FC-022/FC-023/FC-025/FC-026/FC-029 (no literal, identifier, expression-body, repeated-token, or key/value layout logic changes), FC-010–FC-014 and FC-028 (no formatter change), FC-030 (pre-existing, thoenissen/Reihitsu#969 and the skeptic's RH7105/RH7106 follow-up).

### Build and tests

- `scripts/format.sh` over every changed test file: clean after formatting.
- `scripts/test.sh` (all projects; stops after the analyzer project): analyzer project 6595 tests, 6514 passed, **81 failed — exactly the tests listed as "fails" above**, each for the issue's difference; no other test fails.
- `scripts/test.sh --project architecture`: 115 passed, including `EveryRequirementOfAnEnforcedSpecIsCovered` (the six new requirements are now tagged).
- Formatter, core, CLI and tooling projects not run: no file of theirs changed and none depends on the analyzer test project.

### Output questions

_None._ Every changed expectation of a visible output is demanded by an accepted requirement (RH7304-005; RH7101-006 with maintainer answer 4; RH7501-004; RH7109-006) or keeps today's output.

### Follow-up candidates

- RH7004 fix inserts a blank line in front of and behind an `#endregion` that precedes the using declaration (`#region Setup⏎var n = 1;⏎#endregion⏎using var s = …;` → blank lines around `#endregion`), against SHARED-FIX-006 *Blank lines* — pre-existing, outside the run's issues.
- RH7309 fix is offered and changes nothing when the reported block's first member is a nested type: the provider takes the type declaration from `root.FindTrivia(…).Token.Parent`, which is then the nested type (input: `#region Fields` whose first member is `private class Other { … }` behind an `IDisposable` block). FC-009 / SHARED-FIX-005, pre-existing, outside the run's issues; my test `CodeFixReordersInitializersOfNestedTypes` puts a method first to stay clear of it.

### Architecture findings

_None._

### Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH7003|FullyQualifiedName~RH7004Using|FullyQualifiedName~RH7101DoNot|FullyQualifiedName~RH7105|FullyQualifiedName~RH7106|FullyQualifiedName~RH7109|FullyQualifiedName~RH7303|FullyQualifiedName~RH7304|FullyQualifiedName~RH7306|FullyQualifiedName~RH7309|FullyQualifiedName~RH7501"
scripts/test.sh --project architecture
```

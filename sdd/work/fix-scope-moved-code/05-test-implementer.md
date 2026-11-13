Role: test implementer
Commit: 49c3a9c
Verdict: #969: reproduced, tests written; #1335: reproduced, tests written; #1263: reproduced, tests written; #1151: reproduced, tests written; #1604: reproduced for items (a)–(c), item (d) not reproduced (decision 7), tests written

How the statements below were established: issues and comments read through the GitHub MCP server; `00-lead.md`, 01–04 handovers, the change doc and the spec diff against `origin/main` read. Every test was run on 49c3a9c (no production change): `scripts/test.sh --project analyzer` over the RH7004, RH710x, RH7302, RH7303, RH7310 classes — 867 tests, 102 failing rows (98 test names), every one of them a test written or corrected in this round, failing on the issue's own expected-versus-observed difference (a layout diff, "no code fix offered" expected but one offered, or the reverse). `scripts/test.sh --project architecture`: 117 passed, `EveryRequirementOfAnEnforcedSpecIsCovered` included (RH7004-007, SHARED-ORDER-009, SHARED-ORDER-010 now tagged). Files formatted with `scripts/format.sh`.

Ignored input: none in my prompt beyond the issues' prescribed shared helper (#1604), which no test depends on.

Shared helper added: `AnalyzerTestsBase<TAnalyzer, TCodeFix>.VerifyCodeFixWithEndOfLine(source, fixedSource, endOfLine)` (`src/Reihitsu.Analyzer.Test/Base/AnalyzerTestsBase{TAnalyzer,TCodeFix}.cs`): runs `Verify` with both texts in the given line ending and asserts the raw `ApplyCodeFixAsync` result byte for byte. Every `(string endOfLine)` test below runs under `"\n"` and `"\r\n"`.

## Findings for the lead

1. **Spec conflict, SHARED-FIX-003 vs RH7004-007.** The SHARED-FIX-003 example "A pair that lies wholly in the statements the fix moves into the block moves with them" (`sdd/specs/shared/code-fixes.md`, around line 585) says the RH7004 fix "is offered with and without `DEBUG`". Without `DEBUG`, `s.Flush();` is disabled text among the moved statements, which RH7004-007 (decision 4) withholds. I followed RH7004-007: the existing test `FixMovesABalancedConditionalPairOfTheFollowingStatementsIntoTheBlockWithTheSymbolUndefined` is rewritten to `NoCodeFixForABalancedConditionalPairOfTheFollowingStatementsWithTheSymbolUndefined` (no fix). The SHARED-FIX-003 example's sentence needs the spec author; the `DEBUG`-defined half stays offered (existing test unchanged).
2. **Spec example markup, RH7302-005.** The trailing-whitespace example marks `{|RH7302:#region members|}` followed by two spaces. RH7302-003 locates the diagnostic "from its `#` to the end of its line", and the analyzer reports through the trailing spaces (measured: end column 22, not 20). The test marks the span to the end of the line, per RH7302-003; the example's markup is imprecise (spec author).
3. **Existing tests not named by the issues that contradict the specified output**, found by comparing each test's input and expected lines, and corrected (each listed below): RH7102 `FixAlsoLaysOutOtherMembersOfTheType`, `NoFixIsOfferedWhenAnotherMemberOfTheTypeHoldsACommentTheFormatterRefuses`; RH7103 `MultipleBlankLinesAboveTheMovedMemberCollapseToOneAfterReordering`; RH7104 `PartialTypesWithoutAccessModifierAreReportedAndFixed`, `PartialTypesWithPublicAndCompoundAccessibilityAreFixed`, `NoFixIsOfferedWhenTheTypeHoldsACommentTheFormatterRefuses`; RH7105 `FixOfTypeWithRegionPairReplacesTabsBehindRegionKeywords`, `NoFixIsOfferedWhenTheBodyOfTheReorderedMemberHoldsACommentTheFormatterRefuses`; RH7107/RH7108 `NoFixIsOfferedWhenTheOtherAccessorHoldsACommentTheFormatterRefuses`; RH7302 `VerifyCodeFixUpdatesAllMatchingEndRegionOccurrences`; RH7004 `...WithTheSymbolUndefined` (finding 1). The scan compares whole lines of `const string` pairs; tests that build their texts otherwise were read only where their names pointed at these fixes, so a further contradicting test may surface when the implementer runs the classes.
4. **Visible changes the tests lock in that the change doc does not list**, all demanded by unchanged requirements (SHARED-FIX-006/007, SHARED-ORDER-002) or decision 6: three blank lines above a moved member stay three (RH7103, was collapsed to one); empty partial types keep their braces after RH7104 (was `;`); tabs behind region keywords inside a type stay after RH7105 (was four spaces; the old test was tagged FMT-PIPE-028/FMT-TAB-002, now SHARED-FIX-006 — both IDs keep other covering tests); the RH7102/RH7103/RH7104/RH7105/RH7107/RH7108/RH7109 fixes offered where a comment the formatter refuses stands elsewhere in the type, member, or accessor list (decision 6). For the PR's review notes.

## thoenissen/Reihitsu#969

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH7103 `FixLeavesTheOtherMembersOfTheTypeAsWritten` (LF, CRLF) — the issue's example | SHARED-FIX-006, RH7103-006 | `VerifyCodeFixWithEndOfLine` | fails |
| RH7103 `CodeFixReordersStaticWithinRegionAndIgnoresInstanceMembersInOtherRegions` (corrected: `Counter` keeps its four lines) | RH7103-004, RH7103-006, SHARED-FIX-006 | `Verify` | fails |
| RH7103 `MultipleBlankLinesAboveTheMovedMemberStayAtTheirPosition` (renamed from `...CollapseToOneAfterReordering`; three blank lines stay) | RH7103-006, SHARED-FIX-006 | `Verify` | fails |
| RH7102 `FixLeavesTheOtherMembersOfTheTypeAsWritten` (renamed from `FixAlsoLaysOutOtherMembersOfTheType`; `public class Inner { }` stays) | SHARED-FIX-006 | `Verify` | fails |
| RH7102 `FixIsOfferedWhenAnotherMemberOfTheTypeHoldsACommentTheFormatterRefuses` (was `NoFixIsOffered...`) | SHARED-FIX-006, SHARED-FIX-010 | `Verify` | fails |
| RH7103 `FixIsOfferedWhenAnotherMemberOfTheTypeHoldsACommentTheFormatterRefuses` | SHARED-FIX-006, SHARED-FIX-010 | `Verify` | fails |
| RH7109 `FixIsOfferedWhenAnotherMemberOfTheTypeHoldsACommentTheFormatterRefuses` | SHARED-FIX-006, SHARED-FIX-010 | `Verify` | fails |
| RH7102 `ContinuationLineOfAMovedConstantKeepsItsColumn` (LF, CRLF) | SHARED-FIX-007 | `VerifyCodeFixWithEndOfLine` | fails |
| RH7109 `ContinuationLineOfAMovedFieldKeepsItsColumn` (LF, CRLF) — spec example | SHARED-FIX-007, RH7109-005 | `VerifyCodeFixWithEndOfLine` | fails |
| RH7109 `FieldIndentedDeeperThanItsSlotMovesAllItsLinesByTheSameShift` (LF, CRLF) — spec example | SHARED-FIX-007 | `VerifyCodeFixWithEndOfLine` | fails |
| RH7103 `RawStringLinesOfAMovedMethodKeepTheirColumns` (LF, CRLF) | SHARED-FIX-007 | `VerifyCodeFixWithEndOfLine` | fails |
| RH7103 `ShiftedMethodKeepsAColumnZeroCommentAndMovesItsRegionDirectives` (LF, CRLF) | SHARED-FIX-007 | `VerifyCodeFixWithEndOfLine` | fails |
| RH7103 `CommentOverTwoLinesInAMovedMethodKeepsItsLines` | SHARED-FIX-007 | `Verify` | passes (must keep holding) |
| RH7109 `NoCodeFixWhenTheMovedFieldSharesItsLineWithTheFieldInFrontOfIt` — spec example | SHARED-ORDER-009, RH7109-006 | `VerifyNoCodeFixIsOffered` | fails |
| RH7109 `NoCodeFixWhenTheTargetSharesItsLineWithTheOpeningBraceOfTheType` — spec example | SHARED-ORDER-009, RH7109-006 | `VerifyNoCodeFixIsOffered` | fails |
| RH7109 `NoCodeFixWhenTheMovedFieldSharesItsLineWithTheFieldBehindIt` | SHARED-ORDER-009 | `VerifyNoCodeFixIsOffered` | fails |
| RH7109 `NoCodeFixWhenTheMovedFieldSharesItsLineWithTheClosingBraceOfTheType` | SHARED-ORDER-009 | `VerifyNoCodeFixIsOffered` | fails |
| RH7109 `CommentBehindTheMovedFieldMovesWithIt` (LF, CRLF) — spec example | SHARED-ORDER-009 | `VerifyCodeFixWithEndOfLine` | passes (must keep holding) |
| RH7102 `NoCodeFixWhenTheMovedConstantSharesItsLineWithTheFieldInFrontOfIt` | SHARED-ORDER-009, RH7102-006 | `VerifyNoCodeFixIsOffered` | fails |
| RH7103 `NoCodeFixWhenTheMovedFieldSharesItsLineWithTheInstanceFieldInFrontOfIt` | SHARED-ORDER-009, RH7103-007 | `VerifyNoCodeFixIsOffered` | fails |
| RH7109 `NoCodeFixWhenAShiftedFieldHoldsAVerbatimStringOverTwoLines` — spec example | SHARED-ORDER-010, RH7109-006 | `VerifyNoCodeFixIsOffered` | fails |
| RH7109 `FieldIndentedAsItsSlotMovesWithAVerbatimStringOverTwoLines` (LF, CRLF) — spec example | SHARED-ORDER-010 | `VerifyCodeFixWithEndOfLine` | passes (must keep holding) |
| RH7109 `NoCodeFixWhenAShiftedFieldHoldsACommentOverTwoLinesInItsLeadingTrivia` | SHARED-ORDER-010 | `VerifyNoCodeFixIsOffered` | fails |
| RH7109 `FieldIndentedAsItsSlotMovesWithACommentOverTwoLinesInItsLeadingTrivia` | SHARED-ORDER-010 | `Verify` | passes (must keep holding) |
| RH7102 `NoCodeFixWhenAShiftedConstantHoldsAVerbatimStringOverTwoLines` | SHARED-ORDER-010, RH7102-006 | `VerifyNoCodeFixIsOffered` | fails |
| RH7103 `NoCodeFixWhenAShiftedMethodHoldsARawStringOverSeveralLines` | SHARED-ORDER-010, RH7103-007 | `VerifyNoCodeFixIsOffered` | fails |
| RH7103 `NoCodeFixWhenAShiftedMethodHoldsDisabledText` | SHARED-ORDER-010 | `VerifyNoCodeFixIsOffered` | fails |
| RH7103 `MethodIndentedAsItsSlotMovesWithTheDisabledTextItHolds` | SHARED-ORDER-010 | `Verify` | passes (must keep holding) |

Fix All: the existing `GetFixAllScenario` of RH7102, RH7103 and RH7109 (two moves in one type) stays and still passes on main; its inputs are formatter-clean.

### Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| Issue example, RH7103 on `Helpers` | LF, CRLF | code fix | reproduced | observed `public class Inner;` and a block-bodied operator; expected both as written |
| `Decoy` test | LF | code fix | reproduced | observed `public int Counter { get; set; }`; expected the four written lines (the `#endregion` instance the issue named no longer occurs on main) |
| Unsupported shape elsewhere (skeptic item 6), RH7102/RH7103/RH7109 | LF | code fix | reproduced | observed no fix offered; expected the move |
| Shared line (skeptic item 7) | LF | code fix | reproduced | observed the members split onto two lines; expected no fix (SHARED-ORDER-009) |

### Not covered, and why

- A second Fix All scenario with continuation lines: the batch path calls the same provider per diagnostic; the single-fix tests decide the layout, and `GetFixAllScenario` covers the merge.
- Moved-declaration comment in column 0 in the declaration's own leading trivia with a non-zero shift: SHARED-ORDER-002 (whitespace in front of the first comment belongs to the slot) and SHARED-FIX-007 (a column-0 comment line keeps its column) read two ways there; I tested the column-0 comment inside the moved body instead, where only SHARED-FIX-007 applies. Spec author may want to decide the leading-trivia case.

## thoenissen/Reihitsu#1335

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH7004 `VerifyCodeFixTransformsUsingDeclarationWithMultipleFollowingStatements` (corrected: no blank line in front of `using (`) | SHARED-FIX-006 | `Verify` | fails |
| RH7004 `MovedStatementsKeepTheirLayout` (LF, CRLF) — A3 | SHARED-FIX-007 | `VerifyCodeFixWithEndOfLine` | fails |
| RH7004 `NoBlankLineIsInsertedInFrontOfTheNewUsingStatement` (LF, CRLF) — A4 | SHARED-FIX-006 | `VerifyCodeFixWithEndOfLine` | fails |
| RH7004 `NoBlankLineIsInsertedAroundAnEndRegionInFrontOfTheUsingDeclaration` — comment of 2026-11-06 | SHARED-FIX-006 | `Verify` | fails |
| RH7004 `ContinuationLineOfAMovedStatementMovesOneLevelWithIt` (LF, CRLF) — spec example | SHARED-FIX-007 | `VerifyCodeFixWithEndOfLine` | fails |
| RH7004 `AlignedContinuationLineOfAMovedStatementStaysAligned` | SHARED-FIX-007 | `Verify` | passes (must keep holding) |
| RH7004 `ColumnZeroCommentAndDirectiveLinesAmongTheMovedStatements` (LF, CRLF) — spec example | SHARED-FIX-007 | `VerifyCodeFixWithEndOfLine` | fails |
| RH7004 `NoCodeFixForARawStringOverSeveralLinesAmongTheMovedStatements` — spec example | RH7004-007 | `VerifyNoCodeFixIsOffered` | fails |
| RH7004 `NoCodeFixForAVerbatimStringOverTwoLinesAmongTheMovedStatements` — spec example | RH7004-007 | `VerifyNoCodeFixIsOffered` | fails |
| RH7004 `NoCodeFixForAnInterpolatedVerbatimStringOverTwoLinesAmongTheMovedStatements` | RH7004-007 | `VerifyNoCodeFixIsOffered` | fails |
| RH7004 `NoCodeFixForACommentOverTwoLinesAmongTheMovedStatements` — spec example | RH7004-007 | `VerifyNoCodeFixIsOffered` | fails |
| RH7004 `NoCodeFixForACommentOverTwoLinesInsideAMovedStatement` | RH7004-007 | `VerifyNoCodeFixIsOffered` | fails |
| RH7004 `NoCodeFixForDisabledTextAmongTheMovedStatements` — spec example | RH7004-007 | `VerifyNoCodeFixIsOffered` | fails |
| RH7004 `NoCodeFixForABalancedConditionalPairOfTheFollowingStatementsWithTheSymbolUndefined` (rewritten, finding 1) | RH7004-007 | `VerifyNoCodeFixIsOffered` | fails |
| RH7004 `NoCodeFixForAStatementOnTheLineOfTheUsingDeclaration` — spec example | RH7004-007 | `VerifyNoCodeFixIsOffered` | fails |
| RH7004 `StringLiteralAndCommentOnOneLineKeepTheFix` (LF, CRLF) — spec example | RH7004-007 | `VerifyCodeFixWithEndOfLine` | fails (today a blank line is inserted before `return t;`) |
| RH7004 `RawStringOnOneLineKeepsTheFix` | RH7004-007 | `Verify` | fails (same blank line) |
| RH7104 `FixInsertsOnlyTheAccessModifierAndLeavesTheTypeAsWritten` (LF, CRLF) — D1 | SHARED-FIX-006, RH7104-004 | `VerifyCodeFixWithEndOfLine` | fails |
| RH7104 `PartialTypesWithoutAccessModifierAreReportedAndFixed` (corrected: empty bodies keep braces) | SHARED-FIX-006 | `Verify` | fails |
| RH7104 `PartialTypesWithPublicAndCompoundAccessibilityAreFixed` (corrected likewise) | SHARED-FIX-006 | `Verify` | fails |
| RH7104 `FixIsOfferedWhenTheTypeHoldsACommentTheFormatterRefuses` (was `NoFixIsOffered...`) | SHARED-FIX-006, SHARED-FIX-010 | `Verify` | fails |
| RH7105 `FixKeepsTheExpressionBodyOfTheReorderedMethod` (LF, CRLF) — E1 | SHARED-FIX-006, SHARED-MODORDER-002 | `VerifyCodeFixWithEndOfLine` | fails |
| RH7105 `FixKeepsTheExpressionBodyOfTheReorderedOperator` — E1 | SHARED-FIX-006 | `Verify` | fails |
| RH7105 `FixOfATypeLeavesItsMembersAsWritten` — E1 (`static public class D { static int M( ) => 1; }`) | SHARED-FIX-006 | `Verify` | fails |
| RH7105 `FixOfTypeWithRegionPairKeepsTabsBehindRegionKeywords` (renamed from `...ReplacesTabs...`; tabs stay) | SHARED-FIX-006 | `Verify` | fails |
| RH7105 `FixIsOfferedWhenTheBodyOfTheReorderedMemberHoldsACommentTheFormatterRefuses` (was `NoFixIsOffered...`) | SHARED-FIX-006, SHARED-FIX-010 | `Verify` | fails |
| RH7106 `FixKeepsTheSpacingAndTheExpressionBodyOfTheMethod` (LF, CRLF) — E1 `internal protected int M( ) => 1;` | SHARED-FIX-006 | `VerifyCodeFixWithEndOfLine` | fails |
| RH7107 `SingleLineExpressionBodiedAccessorsSwapPlacesWithinTheLine` (renamed from `...AreFixedOntoSeparateLines`) | RH7107-004, SHARED-ORDER-009 | `Verify` | fails |
| RH7107 `AccessorsWithBodiesOnOneLineSwapPlacesWithinTheLine` (LF, CRLF) — spec example (Q7 B) | RH7107-004, SHARED-ORDER-009 | `VerifyCodeFixWithEndOfLine` | fails |
| RH7107 `MultiLineAutoIndexerAccessorsKeepTheirLines` (renamed from `...AreFixedOntoSingleLine`) | SHARED-FIX-007, SHARED-ORDER-009 | `Verify` | fails |
| RH7107 `BlankLineAboveReorderedPropertyIsKeptWhenAccessorBodyIsBraced` (corrected: the `if` keeps no braces) | SHARED-FIX-006, SHARED-FIX-007 | `Verify` | fails |
| RH7107 `CrossedSetAccessorOverTwoLinesKeepsItsLayout` (LF, CRLF) — F1 | RH7107-004, SHARED-FIX-007 | `VerifyCodeFixWithEndOfLine` | fails |
| RH7107 `ContinuationLineOfAMovedAccessorKeepsItsColumn` | SHARED-FIX-007 | `Verify` | fails |
| RH7107 `NoCodeFixWhenTheMovedAccessorSharesItsLineInAListThatSpansLines` — spec example | SHARED-ORDER-009, RH7107-005 | `VerifyNoCodeFixIsOffered` | fails |
| RH7107 `NoCodeFixWhenTheTargetAccessorSharesItsLineWithTheOpeningBraceOfTheList` | SHARED-ORDER-009 | `VerifyNoCodeFixIsOffered` | fails |
| RH7107 `NoCodeFixWhenAShiftedAccessorHoldsAVerbatimStringOverTwoLines` | SHARED-ORDER-010, RH7107-005 | `VerifyNoCodeFixIsOffered` | fails |
| RH7107 `FixIsOfferedWhenTheOtherAccessorHoldsACommentTheFormatterRefuses` (was `NoFixIsOffered...`) | SHARED-FIX-006, SHARED-FIX-010 | `Verify` | fails |
| RH7108 `SingleLineEventAccessorsSwapPlacesWithinTheLine` (renamed from `...AreFixedOntoSeparateLines`) | RH7108-004, SHARED-ORDER-009 | `Verify` | fails |
| RH7108 `ExpressionBodiedEventAccessorsOnOneLineSwapPlacesWithinTheLine` (LF, CRLF) — spec example | RH7108-004, SHARED-ORDER-009 | `VerifyCodeFixWithEndOfLine` | fails |
| RH7108 `CrossedRemoveAccessorKeepsItsLayout` (LF, CRLF) — G1 | RH7108-004, SHARED-FIX-006 | `VerifyCodeFixWithEndOfLine` | fails |
| RH7108 `NoCodeFixWhenTheMovedAccessorSharesItsLineInAListThatSpansLines` | SHARED-ORDER-009, RH7108-005 | `VerifyNoCodeFixIsOffered` | fails |
| RH7108 `FixIsOfferedWhenTheOtherAccessorHoldsACommentTheFormatterRefuses` (was `NoFixIsOffered...`) | SHARED-FIX-006, SHARED-FIX-010 | `Verify` | fails |

`FixMovesGetInFrontOfInit` (`{ init; get; }`, RH7107-004) is unchanged and passes. N2 is covered by the existing SHARED-MODORDER-003 tests (`CommentAfterTheLastModifierWithholdsTheFix`, `RegionDirectiveBehindTheLastModifierWithholdsTheFix`), unchanged. B1 is resolved on main; no test.

### Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| A3 (RH7004) | LF, CRLF | code fix | reproduced | observed `s.Flush();`, braced `if`, blank lines inserted; expected statements as written, +4 columns |
| A4 (RH7004) | LF, CRLF | code fix | reproduced | observed blank lines in front of `using (` and `return x;`; expected none |
| `#endregion` in front of the declaration (RH7004) | LF | code fix | reproduced | observed blank lines around `#endregion`; expected none |
| D1 (RH7104) | LF, CRLF | code fix | reproduced | observed whole type formatted; expected only `internal ` inserted |
| E1 (RH7105, RH7106) | LF, CRLF | code fix | reproduced | observed block bodies, `M()`; expected only the modifiers changed |
| F1 (RH7107), G1 (RH7108) | LF, CRLF | code fix | reproduced | observed accessors expanded and `if` braced; expected whole-line swap |
| Comment of 2026-10-23 (three accessor-layout tests) | LF | code fix | reproduced | observed expanded/collapsed lists; expected one-line swap / kept lines |

### Not covered, and why

- RH7004 Fix All with two using declarations in one block: the existing `GetFixAllScenario` (nested edits) stays and passes on main; its input is formatter-clean.
- RH7004 with a tab in the moved indentation: rare (RH5601 reports it), and no requirement of the run decides it.
- RH7104/RH7105 in a node holding an unsupported shape in the changed code itself (FC-043, other side): the existing `DirectiveBehindTheLastModifierOfATypeWithholdsTheFix` and SHARED-MODORDER-003 tests keep the refusal for the changed code.

## thoenissen/Reihitsu#1263

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH7303 `CodeFixRemovesTheBlankLinesTheRemovalLeavesDoubledOrAtTheEdgesOfABody` (LF, CRLF) — spec example | RH7303-004, SHARED-FIX-007 | `Verify` | fails |
| RH7303 `CodeFixRemovesTheBlankLinesTheRemovalLeavesInsideTheBracesOfAnInitializer` — spec example (Q5 A) | RH7303-004, SHARED-FIX-007 | `Verify` | fails |
| RH7303 `CodeFixRemovesTheBlankLinesTheRemovalLeavesInsideAnAccessorList` | SHARED-FIX-007 | `Verify` | fails |
| RH7303 `CodeFixRemovesTheBlankLinesTheRemovalLeavesInsideTheBracesOfASwitchStatement` | SHARED-FIX-007 | `Verify` | fails |
| RH7303 `CodeFixKeepsASingleBlankLineBehindACaseLabel` | SHARED-FIX-007 | `Verify` | passes (must keep holding) |
| RH7303 `CodeFixRemovesOneOfTwoWhitespaceOnlyBlankLinesTheRemovalLeavesDoubled` (either an empty or a whitespace-only remaining line is accepted; the requirement does not decide which) | SHARED-FIX-007 | `VerifyCodeFixIsWithheldOrProducesOneOf` (not withheld) | fails |
| RH7303 `CodeFixInsertsNoBlankLineBehindAStatementThatEndsWithAClosingBrace` — spec example | RH7303-004 | `Verify` | passes (must keep holding) |
| RH7310 `CodeFixRemovesTheBlankLinesTheRemovalLeavesAtTheStartAndTheEndOfABody` (LF, CRLF) — SHARED-FIX-007 example | SHARED-FIX-007 | `Verify` | fails |
| RH7310 `CodeFixRemovesTheBlankLineTheRemovalLeavesInFrontOfTheClosingBrace` — the issue's first instance | SHARED-FIX-007 | `Verify` | fails |

The existing `CodeFixRemovesBothDirectiveLinesBetweenStatements`, `...WithCarriageReturnLineFeed` (RH7303) and the RH7310 between-members tests stay unchanged and pass.

### Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| `int _a;⏎⏎#region Methods⏎⏎#endregion // Methods⏎}` (RH7310) | LF | code fix | reproduced | observed `int _a;⏎⏎}`; expected `int _a;⏎}` |
| Leading region after `{⏎⏎` (RH7310) | LF, CRLF | code fix | reproduced | observed blank line after `{` kept; expected removed |
| `var x = 1;⏎⏎#region⏎⏎x++;` (RH7303) | LF, CRLF | code fix | reproduced | observed two blank lines; expected one |
| Accessor list, switch, initializer edges (RH7303) | LF | code fix | reproduced | observed blank lines kept at the braces; expected removed |
| Whitespace-only blank lines doubled (RH7303) | LF | code fix | reproduced | observed both kept; expected one |

### Not covered, and why

- Nested regions (only the reported pair goes): RH7303-004 removes the reported directive and its partner; the existing tests cover the pair, and a nested case adds no condition the change touches.
- A comment line directly above the removed `#region`: not a blank-line condition SHARED-FIX-007 states; the line stays under both the old and the new text.

## thoenissen/Reihitsu#1151

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH7302 `CodeFixLeavesAnEndRegionThatSaysMoreThanTheNameUnchanged` (LF, CRLF) — spec example | RH7302-005 | `Verify` | fails |
| RH7302 `CodeFixRenamesTheEndRegionWhenTheRegionLineEndsWithWhitespace` (LF, CRLF) — spec example (Q6 A); diagnostic span per RH7302-003 (finding 2) | RH7302-005 | `Verify` + `ApplyCodeFixAsync` | fails |
| RH7302 `CodeFixNeverRenamesTheEndRegionKeyword` (LF, CRLF) — spec example; today `#Endregion` / `#EndrEgion // E` | RH7302-005, SHARED-FIX-003 | `Verify` | fails |
| RH7302 `CodeFixLeavesTheEndRegionKeywordOfARegionNamedRegionUnchanged` | RH7302-005 | `Verify` | fails |
| RH7302 `VerifyCodeFixLeavesAnEndRegionTextThatRepeatsTheNameTwiceUnchanged` (renamed from `VerifyCodeFixUpdatesAllMatchingEndRegionOccurrences`; `// members members` stays) | RH7302-005 | `Verify` | fails |

The existing `CodeFixRenamesOnlyTheEndRegionThatRepeatsTheName`, `VerifyCodeFixUpdatesMatchingEndRegionComment` and `VerifyCodeFixRenamesOnlyTheEndRegionCommentThatRepeatsTheRegionName` stay and pass.

### Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| `#region a` / `#endregion // a cat` | LF, CRLF | code fix | reproduced | observed `// A cAt`; expected unchanged |
| `#region members` / `#endregion // end of members` | LF, CRLF | code fix | reproduced | observed `// end of Members`; expected unchanged |
| `#region end` / `#endregion`; `#region e` / `#endregion // e` | LF, CRLF | code fix | reproduced | observed `#Endregion`, `#EndrEgion // E` (does not compile); expected keyword unchanged, `// E` |
| `#region members␠␠` / `#endregion // members` | LF, CRLF | code fix | reproduced | observed `// members`; expected `// Members` |

### Not covered, and why

- A pair in an excluded `#if` branch: the rename path is the same text edit on the directive trivia; RH7302-001 reporting it is analyzer behavior the run does not change.

## thoenissen/Reihitsu#1604

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH7109 `UntouchedExpressionBodiedMethodKeepsItsBodyWhenAFieldMoves` — item (a) | SHARED-FIX-006 | `Verify` | fails |
| RH7109 `LineBreakInsideTheMovedFieldStays` (LF, CRLF) — item (b) | SHARED-FIX-007 | `VerifyCodeFixWithEndOfLine` | fails |
| RH7109 `ContinuationLineOfAMovedFieldKeepsItsColumn`, `FieldIndentedDeeperThanItsSlotMovesAllItsLinesByTheSameShift` — item (c) | (listed under #969) | | fails |
| RH7109 `NoCodeFixWhenTheMoveCrossesAPropertyWithAConditionalPairInItsBody` — item (d), characterization | — (no requirement; characterizes current behavior) | `VerifyNoCodeFixIsOffered` | passes |
| RH7004 continuation and directive tests | (listed under #1335) | | fails |

### Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| (a) unmoved `private static int Log(string name) => 0;` | LF | code fix | reproduced | observed block body; expected unchanged |
| (b) `private readonly int _b⏎    ;` | LF, CRLF | code fix | reproduced | observed joined `_b;`; expected kept |
| (c) continuation line of a moved field | LF, CRLF | code fix | reproduced | observed `+ 2` at the formatter's column; expected written column (Q1 A) |
| (d) `_c` moved across `P =>` / `#if DEBUG 1; #else 2; #endif` / `_e` | LF | code fix | not reproduced (decision 7) | no fix offered; the reported blank-line move cannot occur |

### Not covered, and why

- The dropped goals (shared helper, RH3004 raw strings; decisions 2 and 3): no test.

## Whole run

- Failure classes walked: FC-030/FC-016 (fix formats a larger node) — every fix of the run has a "leaves the rest as written" test; FC-043 (unsupported-syntax guard follows the formatted node) — one test per provider (RH7102, RH7103, RH7104, RH7105, RH7107, RH7108, RH7109); FC-003 (both sides of each new condition) — shared line vs whole lines, shifted vs unshifted for SHARED-ORDER-010, one-line vs spanning accessor lists, multi-line vs one-line strings and comments for RH7004-007, disabled text vs the defined symbol (existing test), brace edge vs `case` label; FC-017 (exact output, LF and CRLF) — `VerifyCodeFixWithEndOfLine`; FC-004/FC-020/FC-021 (comments and directives in moved spans) — column-0 comment, `#pragma`, `#region` lines; FC-022 (raw strings) — RH7103 and RH7004; FC-008 (Fix All) — the existing batch scenarios. Formatter idempotency, the formatter counterpart surface, and unsupported-shape declarations (FC-038–FC-043 beyond FC-043's guard) do not apply: no formatter phase changes and no shape is declared.
- `scripts/test.sh --project architecture`: 117 passed.
- Lane check not run by me (tests only).

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH7004|FullyQualifiedName~RH710|FullyQualifiedName~RH7302|FullyQualifiedName~RH7303|FullyQualifiedName~RH7310"
scripts/test.sh --project architecture --filter "FullyQualifiedName~Sdd"
```

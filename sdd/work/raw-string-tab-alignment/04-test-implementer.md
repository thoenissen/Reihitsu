Role: test implementer
Commit: 3b8c71c (tests written on this head; production code unchanged)
Verdict: tests written

Issue: thoenissen/Reihitsu#1432 (`agent-filed`), behavior change. The prompt carried no intended fix and no hoped-for verdict. Expected values come from the examples and conditions of FMT-RAW-010, FMT-RAW-001, FMT-RAW-006, RH5202-007, and RH5202-008 (`sdd/changes/2026-11/raw-string-tab-indentation.md`). Where the spec has no example (block lambda in a hole, raw string in a hole of a regular `$"…"`), the columns are the ones the unchanged code computes, and FMT-RAW-010 says that no column changes. I checked every red formatter test mechanically: the actual output on this head and the expected output have the same length. They differ only where the expected output holds a space and the actual output holds a tab, a no-break space, a vertical tab, or a form feed. So each test fails only on the FMT-RAW-010 rewrite.

In the tables, `→` stands for one tab.

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RawStringTabIndentationTests.RightShiftWritesTheStrippedTabsAsSpaces` | FMT-RAW-010 | `AssertRuleResult` (LF, CRLF, 2nd pass, phase stability) | red |
| `…TabBehindTheStrippedWidthStays` | FMT-RAW-010 | same | red |
| `…ClosingWhitespaceWithoutATabKeepsTheTabOfTheContentLine` | FMT-RAW-010 (other side of the tab condition) | same | green, must stay green |
| `…LeftShiftRemovesTabsAndWritesTheRestAsSpaces` (20 tabs) | FMT-RAW-010 | same | red |
| `…LeftShiftSmallerThanTheSpacesInFrontOfTheTabsWritesTheTabsAsSpaces` / `…EqualTo…` / `…LargerThan…` | FMT-RAW-010 (after the move; leftward axis with spaces in front of tabs) | same | red |
| `…WhitespaceOnlyLineShorterThanTheWidthBecomesOneSpacePerCharacter` (answer 4A) | FMT-RAW-010 | same | red |
| `…WhitespaceOnlyLineLongerThanTheWidthKeepsTheTabBehindIt` | FMT-RAW-010 (within width) | same | red |
| `…NoBreakSpaceNextToATabBecomesASpace`, `…VerticalTabAndFormFeedNextToATabBecomeSpaces` | FMT-RAW-010 | same | red |
| `…Utf8LiteralWritesTheStrippedTabsAsSpaces` | FMT-RAW-010 | same | red |
| `…InterpolatedLiteralWritesTheStrippedTabsInFrontOfHolesAsSpaces` (`→→{n}` behind the start token and behind a `}`) | FMT-RAW-010 | same | red |
| `…DoubleDollarLiteralWritesTheStrippedTabsAsSpaces` | FMT-RAW-010 | same | red |
| `…HoleLineAlignedToATokenOnATabIndentedContentLineKeepsItsColumn` (issue comment, change 6) | FMT-RAW-010; FC-034 | same | red |
| `…AlignedLiteralWithAnAnchoredHoleLineWritesTheTabsAsSpacesWithoutMovingALine` | FMT-RAW-006, FMT-RAW-010 | same | red |
| `…BlockLambdaInAHoleOnATabIndentedContentLineKeepsItsColumns` (skeptic candidate 9) | FMT-RAW-010; FC-027, FC-034 | same | red |
| `…NestedRawStringIsDecidedByItsOwnClosingMarker` (spec example) | FMT-RAW-010 | same | red |
| `…NestedRawStringAlignedInCharactersBehindTabsIsWrittenWithSpaces` (n01 shape, whole document) | FMT-RAW-010; FC-027 | same | red |
| `…OuterAndNestedRawStringsIndentedWithTabsAreBothWrittenWithSpaces` | FMT-RAW-010; FC-027 | same | red |
| `…NestedRawStringWithoutATabInFrontOfItsClosingMarkerKeepsItsTabsInsideATabIndentedLiteral` | FMT-RAW-010 (own text; other side of nested trigger); FC-027 | same | red (the outer literal's tabs) |
| `…RawStringInAHoleOfARegularInterpolatedStringWritesTheTabsAsSpaces` | FMT-RAW-010; FC-027 | same | red |
| `…ContinuationLineOfAVerbatimStringInAHoleKeepsItsTab` (spec example) | FMT-RAW-010 | same | red |
| `…ContinuationLineOfACommentInAHoleKeepsItsTab` | FMT-RAW-010; FC-019 | same | red |
| `RawStringClosingWhitespaceMeasureTests.TabsCountAsOneColumnEachAndBecomeSpaces` (was `…AndStay`; FMT-RAW-001 four-tab example) | FMT-RAW-001, FMT-RAW-010 | same | red |
| `RawStringClosingWhitespaceMeasureTests.LiteralAlignedBySharedMeasureWithTabsIsWrittenWithSpacesWithoutMoving` (was `…WithTabsStays`; FMT-RAW-010 aligned example, change 3) | FMT-RAW-006, FMT-RAW-010 | same | red |
| `ReihitsuFormatterNodeColumnTests.EqualsValueTargetHoldingTabIndentedRawStringWritesTheStrippedTabsAsSpaces` | FMT-RAW-010; FC-028 | `FormatNodeInDocumentAsync`, LF and CRLF, 2nd pass | red |
| `ReihitsuFormatterNodeColumnTests.FormatNodeWritesTheStrippedTabsOfARawStringAsSpaces` | FMT-RAW-010; FC-028 | `FormatNode`, LF and CRLF | red |
| `RH5202…FormatterTests.VerifyFormatterAlignsLiteralBehindTabsAndClearsTheDiagnostic` (expected updated) | FMT-RAW-001, FMT-RAW-010; FC-013 | `VerifyFormatter(source, fixed, diag)` | red |
| `RH5202…FormatterTests.VerifyFormatterWritesTabsOfLiteralAlignedByTheAnalyzersMeasureAsSpaces` (was `VerifyFormatterKeepsLiteralAlignedByTheAnalyzersMeasureBehindTabs`) | FMT-RAW-006, FMT-RAW-010; FC-013 | `VerifyFormatter(source, fixed)`, no diagnostic before or after | red |
| `RH5601UseTabsCorrectlyFormatterTests.VerifyFormatterWritesTheClosingLineTabOfAnInterpolatedRawStringAsASpaceAndItIsNotReported` (was `VerifyFormatterKeepsTheClosingLineTab…`) | RH5601-002, FMT-RAW-001, FMT-RAW-010 | `VerifyFormatter(source, fixed)` | red |
| `RH5202…TabIndentationTests.VerifyFixWritesTheStrippedTabsAsSpacesAndKeepsTheTabBehindTheWidth` (spec example) | RH5202-007 | `VerifyCodeFixWithEndOfLine` LF and CRLF | red |
| `…VerifyFixKeepsTheTabOfTheContentLineWhenTheClosingWhitespaceHoldsNoTab` | RH5202-007 (other side) | same | green, must stay green |
| `…VerifyLiteralAlignedInCharactersBehindTabsIsNotReported` | RH5202-007 (analyzer unchanged) | `Verify(source)` LF and CRLF | green, must stay green |
| `…VerifyFixShiftsLeftOverTabsAndWritesTheRestAsSpaces`, `…VerifyFixShiftsLeftOverSpacesAndTabsAndWritesTheRestAsSpaces` | RH5202-007 | `VerifyCodeFixWithEndOfLine` | red |
| `…VerifyFixWritesAWhitespaceOnlyLineShorterThanTheWidthAsOneSpacePerCharacter` | RH5202-007 | same | red |
| `…VerifyFixWritesANoBreakSpaceNextToATabAsASpace` | RH5202-007 | same | red |
| `…VerifyFixWritesTheStrippedTabsOfAnInterpolatedLiteralAsSpaces`, `…OfAUtf8LiteralAsSpaces` | RH5202-007 | same | red |
| `…VerifyFixMatchesFormatterForATabIndentedLiteral` | RH5202-007; FC-015 | `VerifyCodeFixMatchesFormatter` | green now (both keep tabs); must stay green once both write spaces |
| `…FixAllCorrectsEveryDiagnostic` (`GetFixAllScenario`: two tab literals, right and left shift) | FC-008 | `BatchCodeFixTestsBase` | red |
| `RH5202…HoleTests.VerifyFixIsWithheldWhenUnreportedRawStringNestedInHoleIsIndentedWithTabs` (spec example 1, n01) | RH5202-008 | `VerifyNoCodeFixIsOffered` LF and CRLF | red (fix offered today) |
| `…VerifyFixIsWithheldWhenReportedRawStringNestedInHoleIsIndentedWithTabsAndOfferedAfterTheNestedFix` (spec example 2: withheld, nested fix gives the intermediate, outer fix then gives the final) | RH5202-008 | outer/nested action check, `TryApplyCodeFixAtAsync`, `Verify`; LF and CRLF | red |
| `…VerifyEveryFixApplicationConvergesWhenRawStringNestedInHoleIsIndentedWithTabs` | RH5202-008; FC-007, FC-008 | `Verify` (incremental and Fix All) | red |
| `…VerifyFixIsWithheldWhenRawStringNestedTwoLevelsDeepIsIndentedWithTabs` | RH5202-008; FC-027 | `VerifyNoCodeFixIsOffered` | red |
| `…VerifyFixIsWithheldWhenInterpolatedRawStringNestedInHoleIsIndentedWithTabs`, `…Utf8RawStringNestedInHole…` | RH5202-008 | same | red |
| `…VerifyFixIsWithheldWhenShiftingLeftAndRawStringNestedInHoleIsIndentedWithTabs` | RH5202-008 (either direction) | same | red |
| `…VerifyFixIsOfferedAndKeepsTheTabOfRawStringNestedInHoleWithoutATabInFrontOfItsClosingQuotes` | RH5202-007, RH5202-008 (other side) | `VerifyCodeFixWithEndOfLine` LF and CRLF | green, must stay green |
| `RH3202…AnalyzerTests.VerifyFixWritesTheStrippedTabsOfARawStringAsSpaces` (change 8) | FMT-RAW-010 | `Verify` LF and CRLF | red |

Files: `src/Reihitsu.Formatter.Test/Regression/FullPipeline/RawStringTabIndentationTests.cs` (new), `src/Reihitsu.Analyzer.Test/Formatting/RH5202RawStringLiteralsShouldBeFormattedCorrectlyTabIndentationTests.cs` (new), and the existing `RawStringClosingWhitespaceMeasureTests.cs`, `ReihitsuFormatterNodeColumnTests.cs`, `RH5202RawStringLiteralsShouldBeFormattedCorrectlyHoleTests.cs`, `RH5202RawStringLiteralsShouldBeFormattedCorrectlyFormatterTests.cs`, `RH5601UseTabsCorrectlyFormatterTests.cs`, and `RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzerTests.cs`.

State on this head: formatter suite 26 red, all of them listed above. Analyzer suite 19 red, all of them listed above. Architecture suite is green, so `EveryRequirementOfAnEnforcedSpecIsCovered` now covers FMT-RAW-010, RH5202-007, and RH5202-008. No other test fails.

## Existing tests that assert a changed output

I searched the test projects for a tab, written as `→`, `\t`, or a literal tab, in front of a closing `"""` (`rg -P '(\\t|→|\t)(\\t|→|\t| |·|¤)*(\\"\\"\\"|""")'`). I also searched for `Literal("…")`-style builders and for tests tagged with FMT-RAW-*, FMT-TAB-003, FMT-INDENT-011, RH5601-002, or FMT-PIPE-019.
- Changed: the five tests marked "was …" in the table above.
- Left unchanged, and still green:
  - `RH5601UseTabsCorrectlyAnalyzerTests`, the tests that expect no diagnostic for raw-string tabs. They check the analyzer only.
  - `VerifyCodeFixLeavesTheTabsOfAnInterpolatedRawStringUntouched`. The RH5601 fix does not format the literal, and RH5601-002 is unchanged.
  - `RawStringInterpolationHoleTests` `…TabIndentedHoleLine…` and `PhaseColumnOwnershipTests`. Their tabs are in hole trivia, and FMT-TAB-001 replaces those.
  - `TabReplacementTests.TabsInsideOtherLiteralsAndDocumentationCommentsAreKept`. Its closing whitespace holds no tab, and the tab stands behind the width.
  - `TabReplacementPhaseTests` and `TabReplacementPhaseWholeFileTests`. They run the tab-replacement phase alone, which is unchanged.
  - `RawStringLiteralUtilitiesTests` (Core). The width measure is unchanged.
  - `NoBreakSpacesCountAsOneColumnEach` and the other no-break-space, em-space, vertical-tab, and form-feed tests. Their closing whitespace holds no tab, so their output stays as FMT-RAW-002 states.
- No repository source holds a tab-indented raw string (`rg -P '^[ \t]*\t[ \t]*\$*"""' src`), so the self-hosting stability check is not affected.

## Not covered, and why

- **FMT-RAW-002 no-break-space example** (closing whitespace without a tab stays): already pinned by `RawStringClosingWhitespaceMeasureTests.NoBreakSpacesCountAsOneColumnEach` and its siblings. No new test.
- **RH5202-008 other side using the existing nested examples of RH5202-004/005**: already pinned in `RH5202…HoleTests`. I added only the tab-behind-the-width case.
- **`$$` literal through the RH5202 fix**: not added. The markup parser reads `$$` as a position marker (see the `RH5601` test comment). The formatter test `DoubleDollarLiteralWritesTheStrippedTabsAsSpaces` covers the `$$` kind, and the fix shares the per-line edit with `$"""`. I can give no static proof yet, because the implementation does not exist. If the implementer gives `$$` its own branch, a fix test is owed.
- **Nested UTF-8 raw string through the formatter**: not added. The nested `u8` case is tested on the fix path, where RH5202-008 names the kind. On the formatter side, nested lines are decided per literal, and the plain nested tests cover that.
- **FC-019, directive in the gap**: the change rewrites literal text, not a token gap. The only trivia that can stand on a moved hole line is code trivia, which holds no tab after FMT-TAB-001. The comment case is covered (`ContinuationLineOfACommentInAHoleKeepsItsTab`).
- **FC-004/005/020/021** (joins, comments relocated by joins, directives between members): no join or reordering is involved.
- **FC-006/023/024/025/026/029/036/018**: not reachable. Each character becomes one character, and no identifiers, members, or renames are involved.
- **FC-011/012**: no loop-until-stable and no neighbour phase that inserts or collapses whitespace inside literal text. `AssertRuleResult`'s per-phase stability check covers every formatter test.
- **FC-038 to FC-043**: no shape is declared unsupported in this run.
- **FC-044**: no guard models a compiler result. The RH5202-008 check is syntactic.
- **FC-030**: the RH5202 fix formats only its literal, and that is unchanged.
- **FC-009**: no new code action.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~RawStringTabIndentationTests|FullyQualifiedName~RawStringClosingWhitespaceMeasureTests|FullyQualifiedName~ReihitsuFormatterNodeColumnTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5202RawStringLiteralsShouldBeFormattedCorrectlyTabIndentationTests|FullyQualifiedName~RH5202RawStringLiteralsShouldBeFormattedCorrectlyHoleTests|FullyQualifiedName~RH5202RawStringLiteralsShouldBeFormattedCorrectlyFormatterTests|FullyQualifiedName~RH5601UseTabsCorrectlyFormatterTests|FullyQualifiedName~RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzerTests.VerifyFixWritesTheStrippedTabs"
scripts/test.sh --project architecture
```

## Follow-up candidates

- A comment in `RH5202RawStringLiteralsShouldBeFormattedCorrectlyHoleTests.GetFixAllScenario` says that "one raw string literal can never nest inside another". An interpolated raw literal can hold one in a hole, so the claim is false; this run's RH5202-008 tests nest them. This is a test comment only, with no behavior affected. A cleaner can correct the wording.

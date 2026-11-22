Role: test implementer
Commit: 803b30b
Verdict: tests written

The prompt held no intended fix, suspected cause, or hoped-for verdict. Maintainer answers 1–7 in `00-lead.md` are binding and the tests follow them (1A marks vanish at the first edit, 2A parser-driven contextual keywords, 3A line-end mark, 4A doc comments and directives by trivia kind).

On the unchanged code (803b30b): 80 tests, 66 fail, 14 pass. Every failure is a missing behavior: the output is still a text area (its `TextContent` is empty), there is no coloured copy, no `hl-*` class, no `playground-mark`, no `role="alert"`, no error-line children, no stylesheet rules. `scripts/test.sh --project architecture` passes: PLAY-010 to PLAY-014 are now covered.

## Markup contract

The tests read the page through these points, and nothing else. They are fixed by the tests; how the code produces them is the implementer's call. The contract is written once, in the doc comment of `tests/Reihitsu.Playground.Test/PlaygroundMarkup.cs`.

- **Input:** the text area `#playground-input` (still a `TEXTAREA`, `spellcheck="false"`). The input's column (`.playground-grid > .playground-column`, first) holds exactly one top-level subtree with `aria-hidden="true"`: the coloured copy. Its `TextContent` is exactly the input text. There are no extra characters, and the trailing line break and tabs are kept.
- **Output:** `#playground-output` in the second column, not a `TEXTAREA` or `INPUT`, not `contenteditable`. Its `TextContent` is exactly the output text.
- **Categories:** a character is coloured by the nearest ancestor (up to the pane) with one of the classes `hl-keyword`, `hl-identifier`, `hl-string`, `hl-number`, `hl-comment`, `hl-directive`, `hl-disabled`. A character inside none of them has the normal colour. Whitespace is not asserted.
- **Marks:** elements with the class `playground-mark` inside the coloured copy. A mark is read as `<offset in the copy's text>:<its TextContent>`. A mark one character wide behind a line's or the text's last character holds **no text**, and its width comes from CSS. A mark never adds a character. Each error or shape gives one mark element. Two identical errors give two marks.
- **Message box:** `.playground-message` with `role="alert"`, directly after the button. Each line (the sentence, then each error) is **one child element**. A line is its `TextContent`, trimmed.
- **Stylesheet:** `PlaygroundStyles` loads `src/Reihitsu.Playground/wwwroot/css/app.css` into AngleSharp.Css (already a transitive bUnit dependency) and computes styles. The `prefers-color-scheme: dark` block is unwrapped for the dark scheme, and every other media rule is dropped. `var()` is resolved. CSS functions AngleSharp cannot evaluate (for example `light-dark()` or `color-mix()`) would make the computed colour tests fail, so use plain values or custom properties.

As a harness check, I ran the tests against a throwaway prototype of the page, kept only in the session scratchpad and never in the repository: 80 of 80 passed. I also ran five mutants against it, and each failed the tests named for it: marks kept after an edit, the line break marked, messages in the current culture, a sort that breaks parser order, warnings listed. The prototype showed one trap: a documentation comment's `/**` and `///` exterior lies outside `trivia.Span` and inside `FullSpan`, so `DelimitedDocumentationCommentIsACommentAsAWhole` catches colouring the comment by `Span`.

## Tests

All in `tests/Reihitsu.Playground.Test`. "Fails" means it fails on 803b30b for the missing behavior; "passes" means it already passes and guards the behavior.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `HomePageTests.FormattingRunsWithoutServicesOrJavaScriptInterop` (updated: reads output `TextContent`) | PLAY-001 | bUnit, strict JS interop | fails |
| `HomePageTests.PageShowsEditableInputAndReadOnlyOutputWithFormatButtonBelow` (rewritten: output is a non-editable code view; spell check asserted on the input only) | PLAY-002 | bUnit | fails |
| `HomePageTests.OutputTextIsSelectableAndExactlyTheOutputText` | PLAY-002 | bUnit, `PlaygroundStyles` | fails |
| `HomePageTests.PageOpensWithTheFormattedExample` (updated) | PLAY-003 | bUnit | fails |
| `HomePageTests.OutputChangesOnlyWhenFormatIsPressed` (updated) | PLAY-004 | bUnit | fails |
| `HomePageTests.InputWithSyntaxErrorIsShownUnchangedWithMessage` (updated: sentence + `line 1, column 28 — CS1002: ; expected`) | PLAY-006 | bUnit | fails |
| `HomePageTests.SyntaxErrorMessageDisappearsOnNextSuccessfulFormat` (updated) | PLAY-006 | bUnit | fails |
| `HomePageTests.SpecExampleInputWithUnsupportedSyntaxIsShownUnchangedWithMessage`, `UnsupportedSyntaxMessageDisappearsOnNextSuccessfulFormat` (updated) | PLAY-008 | bUnit | fails |
| `HomePageTests.SpecExampleInputWithHiddenCodeIsShownUnchangedWithMessage`, `HiddenCodeMessageDisappearsOnNextSuccessfulFormat`, `HiddenCodeBesideAnUnsupportedShapeShowsTheHiddenCodeMessage`, `HiddenCodeBesideASyntaxErrorShowsTheSyntaxErrorMessage` (updated) | PLAY-009 | bUnit | fails |
| `ErrorListTests.SpecExampleOneErrorIsListedBelowTheSentence` | PLAY-006 | bUnit | fails |
| `ErrorListTests.SpecExampleTwoErrorsAreListedByPosition` | PLAY-006 | bUnit | fails |
| `ErrorListTests.LinesAndColumnsAreCountedOverSeveralLines` | PLAY-006 | bUnit | fails |
| `ErrorListTests.TabCountsAsOneColumn` | PLAY-006 | bUnit | fails |
| `ErrorListTests.ErrorReportedTwiceAtOnePositionIsListedTwice` | PLAY-006 | bUnit | fails |
| `ErrorListTests.ErrorsAtOnePositionKeepTheParserOrder` (`class C { int @ x; int y }`: CS1519, CS1646, CS1519 all at column 15) | PLAY-006 | bUnit | fails |
| `ErrorListTests.ErrorOfAnErrorDirectiveIsListed` | PLAY-006 | bUnit | fails |
| `ErrorListTests.WarningIsNotListedBesideAnError` | PLAY-006 | bUnit | fails |
| `ErrorListTests.WarningWithoutAnErrorShowsNoMessage` | PLAY-006 | bUnit | passes |
| `ErrorListTests.MessagesAreEnglishUnderAnotherCulture` (de-DE current and UI culture; the German Roslyn satellite is in the test output, so the test can fail) | PLAY-006 | bUnit | fails |
| `ErrorListTests.ListIsReplacedOnTheNextFormat` | PLAY-006 | bUnit | fails |
| `HighlightingTests.SpecExampleInputIsHighlightedByTokenKind` | PLAY-010 | `PlaygroundMarkup.GetSegments` | fails |
| `HighlightingTests.SpecExampleOutputIsHighlightedByTokenKind` | PLAY-010 | same | fails |
| `HighlightingTests.SpecExampleDirectivesCommentsStringsNumbersAndDisabledTextAreHighlightedInTheInput` | PLAY-010 | same | fails |
| `HighlightingTests.SpecExampleDirectivesCommentsStringsNumbersAndDisabledTextAreHighlightedInTheOutput` | PLAY-010 | same | fails |
| `HighlightingTests.SpecExampleDocumentationCommentAndContextualKeywordsFollowTheParser` | PLAY-010 | same | fails |
| `HighlightingTests.DelimitedDocumentationCommentIsACommentAsAWhole` | PLAY-010, FC-040 | same | fails |
| `HighlightingTests.CommentLookingStringIsAStringAndVerbatimIdentifierIsAnIdentifier` | PLAY-010, FC-022, FC-023 | same | fails |
| `HighlightingTests.InterpolationFormatClauseIsAStringAndTheHoleIsNot` | PLAY-010 | same | fails |
| `HighlightingTests.RawStringLiteralsAreStringsExceptTheirHoles` | PLAY-010, FC-022 | same | fails |
| `HighlightingTests.NumericLiteralsAreNumbersAndCharacterLiteralsAreStrings` | PLAY-010 | same | fails |
| `HighlightingTests.CommentBehindADirectiveIsACommentAndADirectiveMessageIsADirective` (`#pragma … // why`, `#region X /* c */`) | PLAY-010 | same | fails |
| `HighlightingTests.ErrorDirectiveMessageThatLooksLikeACommentIsADirective` (`#error a // b`) | PLAY-010 | same | fails |
| `HighlightingTests.HighlightedTextIsExactlyTheTextItColours` (tabs, double spaces, blank line, trailing spaces, trailing line break, non-ASCII, in the copy and in the output) | PLAY-010 | bUnit | fails |
| `HighlightingTests.PageLoadsNoScriptOrStylesheetFromAnotherOrigin` | PLAY-010 | `index.html`, bUnit | passes |
| `HighlightingTests.EachCategoryHasItsOwnColourInEachColourScheme` (in both panes and both schemes: seven distinct colours, each different from the body colour, and every uncategorized character in the body colour) | PLAY-010 | `PlaygroundStyles` | fails |
| `InputHighlightingTests.SpecExampleTypingColoursTheInputAtOnceAndLeavesTheOutput` | PLAY-011 | bUnit | fails |
| `InputHighlightingTests.ColoursFollowEveryChangeOfTheInput` (`clas` → `class` → `classy` → empty) | PLAY-011, FC-003 | bUnit | fails |
| `InputHighlightingTests.ColouredCopyIsHiddenFromAssistiveTechnologyAndTheTextAreaIsNot` | PLAY-011 | bUnit | fails |
| `InputHighlightingTests.TextAreaAndColouredCopyShareTheirTextMetrics` (font-family, font-size, line-height, letter-spacing, tab-size) | PLAY-011 | `PlaygroundStyles` | fails |
| `LayoutTests.SpecExampleLongLinesStayOneLine` | PLAY-012 | bUnit | fails |
| `LayoutTests.PanesDoNotWrapLines` (text area: `wrap="off"` or `white-space: pre`; copy: `pre`; output: `pre`, or a `PRE` element with no override) | PLAY-012 | `PlaygroundStyles` | fails |
| `LayoutTests.PanesScrollHorizontally` | PLAY-012 | `PlaygroundStyles` | fails |
| `MessageBoxTests.SpecExampleSyntaxErrorsShowAnAlertBoxBelowTheButton` | PLAY-013 | bUnit | fails |
| `MessageBoxTests.SpecExampleNoMessageShowsNoBox` | PLAY-013 | bUnit | passes |
| `MessageBoxTests.BoxDisappearsWhenTheNextFormatShowsNoMessage` | PLAY-013 | bUnit | fails |
| `MessageBoxTests.UnsupportedSyntaxMessageIsShownInTheAlertBox` | PLAY-013 | bUnit | fails |
| `MessageBoxTests.HiddenCodeMessageIsShownInTheAlertBox` | PLAY-013 | bUnit | fails |
| `MessageBoxTests.BoxIsSetApartFromThePageText` (in both schemes, a border, or a background different from the page's) | PLAY-013 | `PlaygroundStyles` | fails |
| `ErrorMarkTests.SpecExampleCharacterAndEndOfTextAreMarked` (`58:}`, `59:`) | PLAY-014 | `PlaygroundMarkup.GetMarks` | fails |
| `ErrorMarkTests.SpecExampleEmptySpanAtALineBreakIsMarkedBehindTheLastCharacterOfTheLine` (`19:`) | PLAY-014 | same | fails |
| `ErrorMarkTests.EmptySpansAtTheLineBreaksOfTwoLinesAreMarkedOnTheirOwnLines` (`19:`, `29:`) | PLAY-014 | same | fails |
| `ErrorMarkTests.SpecExampleEmptySpansInFrontOfACharacterAndAtTheEndOfTheText` (`10:/`, `17:`) | PLAY-014 | same | fails |
| `ErrorMarkTests.EmptySpanInFrontOfAQuoteMarksTheQuote` (`21:"`, with a line break later on the line) | PLAY-014, FC-003 | same | fails |
| `ErrorMarkTests.SpanOfSeveralCharactersIsMarkedAsAWhole` (`29:baz`) | PLAY-014 | same | fails |
| `ErrorMarkTests.SpanBehindATabIsMarkedByItsPosition` (`31:}`, `32:`) | PLAY-014, FC-026 | same | fails |
| `ErrorMarkTests.ErrorReportedTwiceIsMarkedTwice` (`34:`, `34:` on a line with several `{`/`}`) | PLAY-014, FC-026 | same | fails |
| `ErrorMarkTests.SpecExampleUnsupportedShapeIsMarked` (`41:/* c */`) | PLAY-014 | same | fails |
| `ErrorMarkTests.UnsupportedShapeEndingInALineBreakIsMarkedWithItsSpan` (`31:#if X\n`, the reported span with its line break) | PLAY-014 | same | fails |
| `ErrorMarkTests.SpecExampleHiddenCodeIsNotMarked` | PLAY-014 | same | fails (no coloured copy yet) |
| `ErrorMarkTests.TypingInputWithSyntaxErrorsSetsNoMark` | PLAY-014, FC-003 | bUnit | fails (needs the copy) |
| `ErrorMarkTests.SpecExampleMarksDisappearAtTheFirstEditAndComeBackWithTheNextFormat` (after the edit, the box keeps its lines; after the next Format, `int y;` + input gives `64:}`, `65:`) | PLAY-014 | same | fails |
| `ErrorMarkTests.MarkOfAnUnsupportedShapeDisappearsAtTheFirstEdit` | PLAY-014 | bUnit | fails |
| `ErrorMarkTests.FormatAgainSetsTheSameMarksOnce` | PLAY-014 | same | fails |
| `ErrorMarkTests.FormatWithoutALocationLeavesNoMark` (after marks: a successful format, then the hidden-code format) | PLAY-014, FC-003 | bUnit | fails |
| `ErrorMarkTests.MarksAreDrawnAsAWavyUnderline` (a rule naming `.playground-mark` declares `wavy`) | PLAY-014 | `PlaygroundStyles.GetRulesNaming` | fails |

Each mark test also asserts that the copy's text is the input unchanged, that every mark on the page is in the copy, and that the output holds none.

## Scenarios (bug fix)

_N/A_ — behavior change.

## Not covered, and why

- **Expected values.** They come from the spec examples. The extra inputs come from the skeptic's probe and my own Roslyn 5.12.0 probe (`CSharpSyntaxTree.ParseText(...).GetDiagnostics()` in the scratchpad), never from Playground code (FC-035). The PLAY-010 example 2 output segments equal its input segments: the formatter changes only whitespace there, which the test relies on, not on the exact output text.
- **Error ordering where the parser's order differs from the position order.** I tried about 35 inputs and found none where `GetDiagnostics()` is not ordered by start position. `ErrorsAtOnePositionKeepTheParserOrder` pins the half of the rule that is reachable: same start, different lengths, and duplicates kept, which a sort by length or ID would break (mutant checked).
- **PLAY-011 alignment while scrolled, the one-character width of an empty mark, the red colour of the wavy line, and real wrapping.** These are layout properties bUnit does not render. They are covered only at declaration strength: shared text metrics, `white-space`, `overflow-x`, and `wavy` in a mark rule. The validator checks them in a browser, including typing into a few thousand lines (skeptic's performance note).
- **Colour of whitespace.** It is not asserted: `GetSegments` skips whitespace, and whitespace has no visible colour. This includes the whitespace inside a directive or comment, where PLAY-010 is silent on whitespace.
- **"Each category has its own colour" is read as distinct from every other category and from the normal text colour.** If the lead reads "own" as distinct among categories only, drop the `AreNotEqual(normal, …)` assert in `AssertPaneColours`.
- **CRLF input (FC-033).** The browser gives the page LF only (skeptic), and bUnit would pass a CR LF that no user produces. No test.
- **Skipped-token trivia (a stray `}`).** It is punctuation either way, and no probe produced a skipped keyword. Not specified, so no test.
- **Formatter failure classes** (FC-001 … FC-021, FC-024/025, FC-027 … FC-032, FC-034, FC-036 … FC-043). No formatter, analyzer, or code-fix code changes in this run. FC-015 (one owner for line/column) is an audit item, not testable from markup.
- **CLI, code fixes, Fix All.** Not involved.

## Commands

```shell
scripts/test.sh --project playground
scripts/test.sh --project playground --filter "TestCategory=PLAY-014"
scripts/test.sh --project architecture --filter "FullyQualifiedName~EveryRequirementOfAnEnforcedSpecIsCovered"
```

## Follow-up candidates

_None._

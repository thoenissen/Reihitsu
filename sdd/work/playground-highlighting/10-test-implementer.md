Role: test implementer
Commit: e8a2b1e
Verdict: tests written

Repair round 1. The prompt named the worklist and its sources. It held no intended fix, suspected cause, or verdict someone hopes for beyond what `09-auditor.md` already suggests, and I re-derived each item from the audit. The tests assert behavior and never a mechanism: no test names `SyntaxFacts`, `char.IsSurrogatePair`, `aria-labelledby` specifically, or a CSS value such as `40vh`.

On e8a2b1e: 87 tests, 7 fail, 80 pass. The 7 failures are the new tests for findings 1 and 4 and Decision 5. The strengthened tests for finding 2 pass, because the stylesheet already satisfies them, and six stylesheet mutants show they now catch what they used to let through (below). As a harness check, I applied the audit's suggested changes to a temporary copy of `GetMarkSpan`, `Home.razor`, and `app.css`, restored them afterwards, and never committed them: 87 of 87 passed. `scripts/test.sh --project architecture` passes (198).

## Tests

All in `tests/Reihitsu.Playground.Test`.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `ErrorMarkTests.EmptySpanInFrontOfASurrogatePairMarksTheWholeCharacter` (`class C { void M() { int x = 1😀 } }` → `30:😀`, `33:}`; observed `30:\uD83D`, `33:}`) | PLAY-014, FC-003 | `AssertMarks` / `PlaygroundMarkup.GetMarks` | fails |
| `ErrorMarkTests.EmptySpanAtALineSeparatorIsMarkedBehindTheLastCharacterOfTheLine` (LF example with U+2028 → `19:`; observed `19: `) | PLAY-014, FC-003 | same | fails |
| `ErrorMarkTests.EmptySpanAtAParagraphSeparatorIsMarkedBehindTheLastCharacterOfTheLine` (U+2029 → `19:`) | PLAY-014, FC-003 | same | fails |
| `ErrorMarkTests.EmptySpanAtANextLineCharacterIsMarkedBehindTheLastCharacterOfTheLine` (U+0085 → `19:`) | PLAY-014, FC-003 | same | fails |
| `ErrorMarkTests.MarksAreDrawnAsARedWavyUnderline` (replaces `MarksAreDrawnAsAWavyUnderline`: on a rendered mark around characters, computed `text-decoration-line` contains `underline`, `text-decoration-style` is `wavy`, and `text-decoration-color` is red, in both schemes) | PLAY-014, FC-017 | `PlaygroundStyles.CreateDocument`, new `PlaygroundStyles.IsRed` | passes |
| `InputHighlightingTests.TextAreaAndColouredCopyShareTheirTextMetrics` (adds `box-sizing`, `margin-top`, `margin-left`, `border-top-width`, `border-left-width`, `padding-top`, `padding-left`; each must be declared for the text area and equal on the copy) | PLAY-011, FC-017 | `PlaygroundStyles` | passes |
| `LayoutTests.PanesDoNotWrapLines` (the output's `PRE` fallback no longer accepts an explicit `white-space: normal`) | PLAY-012, FC-017 | `PlaygroundStyles` | passes |
| `HomePageTests.PanesAreNamedByTheirLabels` (input named "Input"; output named "Formatted output" by `aria-labelledby`, `aria-label`, or a `label for` on a labelable element; observed: the output has no name) | PLAY-002 | bUnit, private `GetAccessibleName` | fails |
| `LayoutTests.OutputIsReachableByKeyboard` (the box that scrolls the output horizontally has `tabindex` ≥ 0; observed: the `pre` has none) | PLAY-012 | `PlaygroundStyles` | fails |
| `LayoutTests.NarrowScreenStacksTheColumnsAndAppliesItsPaneHeight` (narrow screen: one column `1fr`; both panes share one height, which differs from the wide height; observed: `480px` in both) | — (styling, Decision 5) | `PlaygroundStyles.CreateDocument(…, narrow: true)` | fails |

Helper change: `PlaygroundStyles.CreateDocument` takes an optional `narrow` argument. On a narrow screen, every `max-width` media rule is unwrapped in its own place, so it keeps its position in the cascade, as a browser at that width applies it. Without the argument, the stylesheet is built exactly as before.

### Mutants of the strengthened tests

Each was run against the current `app.css` and then restored.

| Mutant | Test | Result |
|---|---|---|
| `.playground-input-copy { padding-left: 0; }` appended | `TextAreaAndColouredCopyShareTheirTextMetrics` | fails |
| `border: 0` moved from the shared copy/textarea rule to the copy only | same | fails on `border-top-width` |
| light `--color-code-mark` blue | `MarksAreDrawnAsARedWavyUnderline` | fails |
| dark `--color-code-mark` blue only | same | fails |
| `underline wavy` → `underline` | same | fails |
| `#playground-output { white-space: normal; }` appended | `PanesDoNotWrapLines` | fails |

## Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| CS1056 empty span in front of 😀 | LF | page (bUnit) | reproduced | marks `30:\uD83D` (one code unit) vs `30:😀` |
| CS1002 at U+2028 / U+2029 / U+0085, listed as `line 3, column 10` (Roslyn counts each as a line break) | the line break under test | page (bUnit) | reproduced | mark holds the line break (`19: `, …) vs an empty mark `19:` |
| Output label and focus | — | page (bUnit) | reproduced | no accessible name, no `tabindex` vs "Formatted output" and a focus stop |
| Narrow-screen pane height | — | stylesheet (AngleSharp, narrow) | reproduced | `480px` (the 60vh base rule) in both widths vs a narrow height that differs |

In bUnit, the surrogate split does not show as U+FFFD. `copy.TextContent == input` still holds, and the split shows only in the mark's text. The browser rendering of the replacement glyphs is for the validator.

## Not covered, and why

- **The holding members of the empty-span class** — an ordinary character (`EmptySpanInFrontOfAQuoteMarksTheQuote`, `SpecExampleEmptySpansInFrontOfACharacterAndAtTheEndOfTheText`), `\n` (`SpecExampleEmptySpanAtALineBreak…`, `EmptySpansAtTheLineBreaksOfTwoLines…`), and the end of text (`SpecExampleCharacterAndEndOfTextAreMarked`) — are already pinned, and these tests still pass. A BMP character next to the surrogate pair is covered by the same test's `33:}`.
- **`\r`, alone or as CR LF.** It is unreachable: the browser gives a text area LF only (skeptic, audit FC-033), so there is no test.
- **A second surrogate-pair input** (the non-BMP letter `𝑥` from the audit). It reaches the same check at `text[span.Start]`, because the widening is one branch of `GetMarkSpan` (`rg -n "GetMarkSpan" src/Reihitsu.Playground` → one definition, one call site in `Render`). One test covers it.
- **The red colour of an empty mark.** An empty mark is drawn by a `background` gradient, and AngleSharp does not evaluate gradients into colours. The validator checks it in a browser.
- **Actual alignment, focus behavior, and narrow layout in a browser.** These are declaration-strength checks, as in 04. Real rendering is the validator's.
- **Findings 3, 5, 6.** They are internal (one parse owner, a BOM, a doc comment) and cannot be observed from markup. They are the implementer's items, with no test.

## Commands

```shell
scripts/test.sh --project playground
scripts/test.sh --project playground --filter "FullyQualifiedName~EmptySpan|FullyQualifiedName~PanesAreNamedByTheirLabels|FullyQualifiedName~OutputIsReachableByKeyboard|FullyQualifiedName~NarrowScreen"
scripts/test.sh --project playground --filter "FullyQualifiedName~MarksAreDrawnAsARedWavyUnderline|FullyQualifiedName~TextAreaAndColouredCopyShareTheirTextMetrics|FullyQualifiedName~PanesDoNotWrapLines"
```

## Follow-up candidates

_None._

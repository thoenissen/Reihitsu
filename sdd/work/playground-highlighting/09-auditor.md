Role: auditor
Commit: fcb82346886ea93149e5229451aa87a43b7438fb
Verdict: findings

The prompt held no intended fix, suspected cause, or hoped-for verdict.

## Scope
Full audit. Head `fcb8234` (merge of `origin/main` into `claude/beautiful-volta-05rdua`); base `origin/main` = `1788f94d265c4e13052d5145404ae3ffd21e6acc`, confirmed with `git ls-remote origin refs/heads/main` and merged (`merge-base --is-ancestor`). The diff `1788f94...HEAD` is what handovers 01–08 describe: the Playground page, `PlaygroundHighlighter`, `PlaygroundFormatter`, `PlaygroundFormatResult`, `app.css`, the Playground tests, the PLAY spec, ledger, code-owner row, and the change doc. The merge touches no Playground file. `scripts/lane-check.sh behavior-change` → `confirmed`. Because the cleaner's test run predates the merge of formatter changes from `main` (FC-032), I re-ran `scripts/test.sh --project playground` on HEAD: 80 of 80 pass.

Evidence for findings #1 comes from a throwaway probe in the session scratchpad (a console project referencing the built Playground and bUnit assemblies, rendering `Home`, typing an input, and pressing Format); nothing of it is in the repository.

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | high | `src/Reihitsu.Playground/PlaygroundHighlighter.cs:97-107` (`GetMarkSpan`) | FC-002, FC-001 | – | PR-introduced | `GetMarkSpan` widens an empty span by one UTF-16 code unit and treats only `\r`/`\n` as a line break. Class (axis: what follows the empty span's position): ordinary BMP character (`§` in `class C { int x = 1 § }`, `"`, `/`) — holds; `\n` — holds; `\r` — holds (unreachable from a browser text area); end of text — holds; **a surrogate pair — fails**; **U+2028, U+2029, U+0085 — fail**. (a) `class C { void M() { int x = 1😀 } }`: Roslyn reports CS1056 with the empty span `[30..30)` in front of the emoji; the copy renders `…<span class="hl-number">1</span><span class="playground-mark">\uD83D</span>\uDE00 …` — the pair is split across two DOM text nodes. Blazor WebAssembly decodes render-batch strings as UTF-16 with replacement, so the browser shows two U+FFFD glyphs in place of the emoji: a changed character (PLAY-010 "no character is … changed", PLAY-014 "A mark MUST NOT … change a character"), and every later glyph on the line drifts off the text area's characters (PLAY-011). Same with a non-BMP letter (`int a = 1 𝑥 = 2;`, CS1056 `[31..31)`). bUnit keeps .NET strings, so `copy.TextContent == input` stays true there; the validator can confirm the glyphs in a browser. (b) `class C {     int x }`: Roslyn counts U+2028 as a line break (message `line 3, column 10 — CS1002`), but the mark element holds the U+2028 itself — the line break is marked and nothing is drawn behind `x`, which PLAY-014's second bullet forbids. The counterpart predicate is Roslyn's own line-break set (the one `FormatPosition` counts lines by) | Two cheap checks in `GetMarkSpan`: widen by two code units when `char.IsSurrogatePair(text, span.Start)`, and test the line break with Roslyn's `SyntaxFacts.IsNewLine(char)` instead of `'\r' or '\n'`; add a test per failing member (FC-003) |
| 2 | medium | `tests/Reihitsu.Playground.Test/InputHighlightingTests.cs` (`TextAreaAndColouredCopyShareTheirTextMetrics`), `ErrorMarkTests.cs` (`MarksAreDrawnAsAWavyUnderline`), `LayoutTests.cs` (`PanesDoNotWrapLines`) | FC-017 | – | same requirement | Declaration-strength tests that skip part of the declaration they stand in for. (a) PLAY-011 alignment depends on where the text starts in each box, but the metrics test compares only font-family, font-size, line-height, letter-spacing, and tab-size; a different `padding-left`/`padding-top` or border width on the copy or the text area misaligns every colour and passes. (b) PLAY-014 requires a *red* wavy underline; the test only finds the word `wavy` in some rule naming `.playground-mark`, so any colour passes. (c) The output fallback `output.TagName == "PRE" && white-space is "" or "normal"` also accepts an explicit `white-space: normal` override, which wraps. Members that hold: font metrics, `wavy`, the text area and copy `white-space` assertions | (a) add `padding-top`, `padding-left`, `border-top-width`, `border-left-width`, `box-sizing` to the compared properties; (b) assert a red mark colour (`text-decoration-color` or the custom property the rule uses) in both schemes; (c) drop `"normal"` from the fallback |
| 3 | low | `src/Reihitsu.Playground/PlaygroundHighlighter.cs:116`, `src/Reihitsu.Playground/PlaygroundFormatter.cs:26` | FC-015 | – | new mechanism | "Parse with the options of PLAY-005" has two owners, each a bare `CSharpSyntaxTree.ParseText(text)`. Parity holds today because both calls are identical, but a change to PLAY-005's options in one place (preprocessor symbols, language version) would colour the input by a different tree than the one the formatter and the error list use | One internal parse entry point (for example `PlaygroundFormatter.Parse(string)`) used by both |
| 4 | low | `src/Reihitsu.Playground/Pages/Home.razor:22-23` | – | – | PR-introduced | `<label for="playground-output">` now points at a `<pre>`, which is not a labelable element, so the output loses the accessible name it had as a text area; the `pre` is also not a focus stop, so in browsers that do not make scroll containers focusable (Safari) a keyboard user cannot scroll the output horizontally (PLAY-012) | Name the output by `aria-labelledby` on the `pre` (label with an `id`, no `for`), and give the `pre` `tabindex="0"` |
| 5 | low | `src/Reihitsu.Playground/Pages/Home.razor.cs:1` | – | – | PR-introduced | The file starts with two byte-order marks (`ef bb bf ef bb bf`); the second is a stray U+FEFF that Roslyn reads as whitespace. On `main` the file has one | Remove the second BOM |
| 6 | low | `src/Reihitsu.Playground/PlaygroundFormatResult.cs:16` | FC-031 | – | PR-introduced | The `Errors` parameter's doc says "empty when the input has none", but every result except the syntax-error one leaves it `null` (the default), which is why `Home.FormatInput` needs `result.Errors ?? []` | Say "`null` when the input has none", or default it to an empty list and drop the `?? []` |

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Which character an empty error span marks (`GetMarkSpan`) | `text[span.Start]`, one UTF-16 code unit, tested against `\r`/`\n` | Roslyn's line breaks (`\r`, `\n`, U+0085, U+2028, U+2029) that `GetLineSpan` counts for the message; a character as one Unicode scalar | disagree on surrogate pairs and on the three Unicode line breaks | finding #1 |
| When marks disappear (`OnInputChanged` via `@bind:after`) | every `oninput` change of `_input` | `FormatInput`, the only place that sets marks, always for the `_input` it formats | agree; marks can never refer to another text | ok |
| Which diagnostics are listed and marked (`GetSyntaxErrors`) | `Severity == Error` of `SyntaxTree.GetDiagnostics()` | PLAY-006 "not an error … MUST NOT be listed"; the same list feeds the message lines and the marks | agree | ok |
| Category by trivia kind (`AddTrivia`) | trivia kind; documentation comments by `FullSpan` (exterior included), other comments, disabled text, and message trivia by `Span`; structured trivia recursed with `inDirective` | PLAY-010 bullets and the "by kind, never by text" paragraph | agree; probed `#if A // c` (comment), `#elif`/`#else` with disabled text, conflict markers, `@$"…"`, nested `$"{$"{y}"}"` | ok |
| Mark nesting (`RenderRange`) | marks ordered by start, longest first; a mark starting inside another is nested and cut at its end | Decision 2 of `00-lead.md` | agree; identical spans (`CS1002` and `CS1525` on one `=`) nest, identical empty spans become siblings | ok |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| One-based line and column of a location | `rg -n "GetLineSpan\|StartLinePosition" src/Reihitsu.Playground` → `Home.FormatPosition` only, used by `CreateUnsupportedMessage` and `CreateErrorLine` | one owner |
| Which diagnostics are errors | `rg -n "DiagnosticSeverity" src/Reihitsu.Playground` → `PlaygroundFormatter.GetSyntaxErrors` | one owner |
| Parse with PLAY-005's options | `rg -n "ParseText\|ParseOptions" src/Reihitsu.Playground` → `PlaygroundFormatter.Format:26`, `PlaygroundHighlighter.Classify:116` | two owners, identical today — finding #3 |
| Category of a token or trivia; mark span | `PlaygroundHighlighter` only; no other highlighter or classifier in the repository | one owner |
| What a line break is, for marks | `rg -n "'\\\\n'" src/Reihitsu.Playground` → `PlaygroundHighlighter.GetMarkSpan:101` (private `\r`/`\n` test) vs. Roslyn's line breaks behind `GetLineSpan` | diverges — finding #1 |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `HighlightingTests.*` segment tests | category per non-whitespace character by nearest `hl-*` ancestor | a character in another or no category | `PlaygroundMarkup.GetSegments` (exact sequence) | adequate |
| `HighlightingTests.HighlightedTextIsExactlyTheTextItColours` | copy and output text equal the input | any added, dropped, or changed character | `TextContent` equality | adequate for bUnit; holds no non-BMP character at a mark (finding #1) |
| `HighlightingTests.EachCategoryHasItsOwnColourInEachColourScheme` | seven distinct colours ≠ body colour; uncategorized text in body colour, both schemes | equal colours or a coloured punctuation character | `PlaygroundStyles` computed style | adequate (declaration strength) |
| `InputHighlightingTests.TextAreaAndColouredCopyShareTheirTextMetrics` | copy stays under the text | different text origin | computed font properties | weaker than guarantee — finding #2a |
| `LayoutTests.PanesDoNotWrapLines` | no pane wraps | a pane with `white-space` that wraps | computed `white-space` | fallback admits `normal` — finding #2c |
| `LayoutTests.PanesScrollHorizontally` | an ancestor up to the column scrolls | `overflow-x` not auto/scroll | computed `overflow-x` | adequate (declaration strength) |
| `ErrorListTests.*` | exact message lines, order, duplicates, English, no warnings | any other line | `GetMessageLines` (exact sequence) | adequate; mutants named in 04 |
| `ErrorMarkTests.AssertMarks` cases | offset and text of each mark, copy unchanged, no mark outside the copy | a mark found by text, off by one, or in the output | `GetMarks` (exact sequence by offset) | adequate; FC-026 inputs with repeated `}` |
| `ErrorMarkTests.SpecExampleMarksDisappearAtTheFirstEdit…` | no mark after an edit; box unchanged; marks of the new input after Format | option B (offsets kept) | `GetMarks`, `GetMessageLines` | adequate |
| `ErrorMarkTests.MarksAreDrawnAsAWavyUnderline` | red wavy underline | a non-red or non-wavy underline | `GetRulesNaming` substring `wavy` | weaker than guarantee — finding #2b |
| `MessageBoxTests.*` | `role="alert"` box below the button, absent with no message, set apart in both schemes | missing role, wrong place, box without border and background | bUnit, computed style | adequate |
| `HomePageTests.PageShowsEditableInput…`, `OutputTextIsSelectable…` | text area input with spell check off; non-editable output whose text is exact and selectable | editable output, `user-select: none` | bUnit, computed style | adequate; does not check the output's label association (finding #4) |

## Failure classes dismissed
- FC-004, FC-005, FC-019, FC-020, FC-021 — no formatter, code-fix, or analyzer edit; the highlighter only reads trivia. Directive and comment trivia were still traced for PLAY-010 (Guard scope).
- FC-006, FC-024, FC-025, FC-027, FC-028, FC-029, FC-036 — no rewrite of user code.
- FC-007, FC-008, FC-009, FC-018, FC-030, FC-043 — no code fix changes.
- FC-010, FC-011, FC-012, FC-013, FC-014 — no formatter phase or analyzer changes; PLAY-005's idempotency is unchanged.
- FC-038, FC-039, FC-041, FC-042 — no unsupported shape is declared or changed; the page only reads `UnsupportedSyntax.Location`, which the formatter states in the caller's tree.
- FC-040 — both parses use default options (`DocumentationMode.Parse`), so `///` and `/** */` are structured trivia in the input and the output alike; `DelimitedDocumentationCommentIsACommentAsAWhole` covers the exterior.
- FC-022, FC-023, FC-026 — covered by tests and traced: comment-looking strings, raw and interpolated raw strings, `@class`, repeated `}` on a marked line.
- FC-003 — covered for ordinary character / line break / end of text; the missing members are finding #1.
- FC-016 — the one widened-span policy has no sibling copy; its failing members are listed in #1.
- FC-033 — the browser gives the page LF only; CRLF is unreachable (skeptic). The non-BMP member of #1 is a harness normalization of a different kind (bUnit keeps .NET strings), stated in #1.
- FC-034, FC-035, FC-037 — no regression reproduction; expected values come from spec examples and Roslyn probes; no comment-only proof claimed.
- FC-044 — no guard models a result another component computes; #1 is a private copy of Roslyn's line-break set, fixed by reading Roslyn's own predicate.
- FC-032 — handled by re-running the Playground tests on the merged HEAD (80/80).
- Tab-width matrix — formatter phases only; the page counts a tab as one column through Roslyn, tested by `TabCountsAsOneColumn` and `SpanBehindATabIsMarkedByItsPosition`.

Check 11 (cost of special cases): not applicable; no formatter or code-fix input class, and `RenderRange`'s cut for partially overlapping marks is two clamps that keep the markup well-formed (Decision 2). Check 10 (visible output decided): every expected value is demanded by PLAY-002, PLAY-006, PLAY-010 – PLAY-014 or Maintainer answers 3, 4, 5, 7 (1A–4A); colours themselves are styling, a spec non-goal. Check 9 (scope): no edit outside the Playground, its spec, ledger, code-owner row, and change doc.

## Architecture findings
_None._

## Hints
- Typing speed and scroll alignment are browser properties: the input is parsed and re-rendered on every keystroke, and a character typed at the end of the widest line briefly overflows the text area (`overflow: hidden` still scrolls to the caret) until the copy re-renders. The validator should type in a long input and at the right edge in a browser, and also confirm the U+FFFD rendering of #1 (a).
- Two errors with an identical non-empty span (`CS1002` and `CS1525` on `=` in `int a = 1 𝑥 = 2;`) render as two nested mark elements; one wavy line is drawn. Not a defect.

## Follow-up candidates
- Pre-existing on `main`: in `src/Reihitsu.Playground/wwwroot/css/app.css`, the `@media (max-width: 700px)` rule that sets the panes to `40vh` comes before the base rule that sets `60vh` with the same specificity, so the narrow-screen height never applies (on `main`: lines 118–124 vs. 140–141; now 139–146 vs. 162–164). Structural cause: a media override placed before the rule it overrides.

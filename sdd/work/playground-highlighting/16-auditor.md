Role: auditor
Commit: b38dac563ede47798e6adf8e3148d29d1a78980c
Verdict: findings

The prompt held no intended fix, suspected cause, or hoped-for verdict.

## Scope
Repair delta against `fcb82346886ea93149e5229451aa87a43b7438fb` (audit 1, `09-auditor.md`). Head `b38dac5`, which is the checkout. Base `origin/main` = `1788f94d265c4e13052d5145404ae3ffd21e6acc`, confirmed with `git ls-remote origin refs/heads/main` and merged (`merge-base --is-ancestor`). The repair diff `fcb8234...HEAD` matches handovers 10–14: the Playground marks, parse owner, output label and focus stop, narrow-screen media rule, BOM, `Errors` doc, the strengthened and new Playground tests, and one PLAY-012 sentence in the spec and the change doc. The scope did not grow: no file outside the Playground, its tests, its spec, and the change doc. `scripts/lane-check.sh behavior-change` → `confirmed`. Commits after the cleaner's 87/87 run (`b0fe894`) touch only Markdown, so I did not re-run the tests.

Evidence for the surrogate-pair class comes from a throwaway console probe in the session scratchpad, which references the built Release assemblies of `Reihitsu.Playground` and Roslyn. Nothing of it is in the repository.

## Previous findings
| # | Status | Evidence |
|---|---|---|
| 1 | closed | `GetMarkSpan` now tests `SyntaxFacts.IsNewLine` and widens by `char.IsSurrogatePair`. Probe over 24 inputs with non-BMP characters (bad character in front of `;`, `}`, whitespace, and the end of text; two pairs in a row; an emoji modifier sequence; a non-BMP identifier letter; inside a char literal, a string, an interpolation hole, a comment, disabled text, and `#error`; `@😀`; a lone high surrogate): no mark span from `GetMarkSpan`, no diagnostic span, no token span, and no trivia span starts or ends between a high and a low surrogate. U+2028, U+2029, and U+0085 each have a test. `\r` is in `IsNewLine` and unreachable from a text area. |
| 2 | closed | (a) `box-sizing`, margins, borders, and paddings are compared and must be declared; (b) computed `text-decoration-line`/`-style`/`-color` with `IsRed` in both schemes; (c) the `normal` fallback is gone. The mutants in `10-test-implementer.md` fail. |
| 3 | closed | `rg "ParseText\|ParseSyntaxTree\|ParseCompilationUnit\|SyntaxTree\.Create\|CSharpParseOptions" src/Reihitsu.Playground` finds only `PlaygroundFormatter.Parse`. |
| 4 | closed | The two members it listed hold: no `label for` points at the `pre` any more, and the `pre` has `tabindex="0"`. A member the finding did not list fails: see #7. |
| 5 | closed | `Home.razor.cs` starts with `ef bb bf 75 73 69`, one BOM as on `main`. |
| 6 | closed | `Errors` doc now says `null`, which matches `Home.FormatInput`'s `?? []`. |
| Follow-up (narrow-screen height) | closed | Taken in as Decision 5. The media rule now comes after the base rule, and `NarrowScreenStacksTheColumnsAndAppliesItsPaneHeight` pins it. |

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 7 | low | `src/Reihitsu.Playground/Pages/Home.razor:22-23`; `tests/Reihitsu.Playground.Test/HomePageTests.cs` (`GetAccessibleName`, `PanesAreNamedByTheirLabels`) | FC-017 | #4 | PR-introduced | The output's name comes from `aria-labelledby` on a `<pre>` that has no `role`. HTML-AAM maps `pre` to the ARIA role `generic`, and ARIA 1.2 prohibits `aria-label` and `aria-labelledby` on `generic` (name from: prohibited). It is an authoring error that axe reports (`aria-prohibited-attr`), and assistive technology may ignore the name, so the focus stop added for #4 can be announced without "Formatted output". The test stays green because `GetAccessibleName` takes `aria-labelledby` on any element and does not model the role that prohibits it. Members of the class (axis: how the output's name is given): `label for` on the `pre` fails (audit 1, now gone); `aria-labelledby` on a `pre` without a role fails (this finding); the input's `label for` on a `textarea` holds. | Give the `pre` a role that allows a name. A focusable scroll container commonly uses `role="region"` with its `aria-labelledby` and `tabindex="0"`. In `GetAccessibleName`, return no name from `aria-label`/`aria-labelledby` for an element without a `role` whose element maps to `generic` (`pre`, `div`, `span`), so the test fails on today's markup. |

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Which character an empty error span marks (`GetMarkSpan`) | `text[span.Start]` against `SyntaxFacts.IsNewLine`; the width is 2 when `char.IsSurrogatePair(text, span.Start)` | Roslyn's line-break set behind `GetLineSpan` (the message's line and column); Roslyn's span boundaries, which the probe shows never split a pair | agree | ok |
| Narrow-screen media rule (`app.css`) | `@media (max-width: 700px)` now after `.playground-editor, .playground-output { height: 60vh }` | the base rule with the same specificity; nothing after the media block sets `height` on the panes (`grep height` → lines 153 and 170 only) | agree | ok |
| Which media rules the test stylesheet unwraps (`PlaygroundStyles.CreateSchemeStyleSheet`) | `prefers-color-scheme:<scheme>`, and on `narrow` any `max-width:` condition | `app.css` holds exactly two media rules (lines 26 and 163) | agree | ok |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Parse with PLAY-005's options | `PlaygroundFormatter.Parse` (`PlaygroundFormatter.cs:56`); called by `Format:26` and `PlaygroundHighlighter.Classify:117` | one owner |
| What a line break is, for marks | `SyntaxFacts.IsNewLine` in `GetMarkSpan:102`; no private `\r`/`\n` test left in `src/Reihitsu.Playground` | one owner (Roslyn's) |
| Mark width of an empty span | `GetMarkSpan`, called from `Home.razor.cs:101` (unsupported) and `:112` (errors) | one owner |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `ErrorMarkTests.EmptySpanInFrontOfASurrogatePairMarksTheWholeCharacter` | the mark holds the whole non-BMP character | a mark of one code unit (`30:\uD83D`) | `AssertMarks` (exact offsets and texts, copy unchanged) | adequate |
| `ErrorMarkTests.EmptySpanAt{ALineSeparator,AParagraphSeparator,ANextLineCharacter}…` | an empty mark behind `x`, the line break not marked | a mark that holds the separator | `AssertMarks` | adequate |
| `ErrorMarkTests.MarksAreDrawnAsARedWavyUnderline` | a red wavy underline in both schemes | a non-red, non-wavy, or missing underline (mutants fail) | computed style, `PlaygroundStyles.IsRed` (an unknown colour format returns false or throws, so it never passes falsely) | adequate for marks around characters; an empty mark's gradient is for the validator (stated in 10) |
| `InputHighlightingTests.TextAreaAndColouredCopyShareTheirTextMetrics` | the copy's text starts where the text area's does | a different left or top margin, border, padding, or box sizing | computed style | adequate (declaration strength) |
| `LayoutTests.PanesDoNotWrapLines` | no pane wraps | `white-space: normal` on the output | computed style | adequate |
| `LayoutTests.OutputIsReachableByKeyboard` | the box that scrolls the output is a focus stop | no `tabindex`, or a negative one | computed `overflow-x`, attribute | adequate |
| `LayoutTests.NarrowScreenStacksTheColumnsAndAppliesItsPaneHeight` | on a narrow screen: one column, one pane height different from the wide one | the media rule overridden by the base rule (fails on `fcb8234`) | `CreateDocument(narrow: true)` | adequate (declaration strength) |
| `HomePageTests.PanesAreNamedByTheirLabels` | each pane has an accessible name equal to its label | no name | `GetAccessibleName` | weaker than the guarantee: it accepts a name that ARIA prohibits on the element's role (finding #7) |

## Failure classes dismissed
- FC-004, FC-005, FC-019, FC-020, FC-021, FC-006, FC-024, FC-025, FC-027, FC-028, FC-029, FC-036, FC-007, FC-008, FC-009, FC-018, FC-030, FC-010, FC-011, FC-012, FC-013, FC-014, FC-038, FC-039, FC-041, FC-042, FC-043: the repair changes no formatter phase, code fix, analyzer, or unsupported shape. As in audit 1, the Playground only reads trees and locations.
- FC-040: `Parse` keeps the default options, so the input and the output are parsed alike.
- FC-022, FC-023, FC-026: the surrogate probe covers strings, an interpolation hole, comments, disabled text, `@`, and non-BMP identifier letters.
- FC-001, FC-002, FC-003, FC-016: the widened-span predicate now reads Roslyn's own line-break set. Both sides of each boundary have tests: ordinary character, BMP character next to a pair, each Unicode line break, `\n`, and the end of text.
- FC-015: one owner per policy (table above).
- FC-031: the summaries of `GetMarkSpan`, `Classify`, `Parse`, `PlaygroundStyles`, and `CreateSchemeStyleSheet`, and the `Errors` doc, describe the new code. `Render`'s `marks` doc ("one character wide") still holds for a surrogate pair, which is one character.
- FC-032: the tests ran on the repaired production code (`b0fe894`); later commits change Markdown only.
- FC-033: bUnit keeps .NET strings, so the surrogate split could not show as U+FFFD. The new test catches it through the mark's text, not through the rendered glyph.
- FC-034, FC-035: the expected values come from PLAY-014's rule and Roslyn's reported spans, not from a patched copy; the patched harness check in 10 was only run to confirm the tests can pass.
- FC-037: no comment-only proof is claimed.
- FC-044: no guard models a computed result; the line-break test calls Roslyn's predicate.
- Tab-width matrix: formatter phases only.
- The BOM added to `ErrorMarkTests.cs` is not a finding: `.editorconfig` sets `charset = utf-8-bom`.

Check 10 (visible output is decided): the new mark expectations come from PLAY-014's empty-span rule and Maintainer answer 7 (3A). The red wavy underline comes from answer 3. The output's label "Formatted output" is named in PLAY-002. The keyboard focus stop keeps `main`'s focusable output text area (PLAY-012 sentence, Decision 7). The narrow-screen height is styling (Decision 5). Check 11: the repair adds no special case. Check 9: in scope.

## Architecture findings
_None._ Finding #7 repeats the class of #4 (the output pane's accessible name, between the page's markup and the accessibility tree). That is a markup choice, not a structural cause from `README.md`, *Architecture findings*. The lead decides how the repeat is handled.

## Hints
- The `PlaygroundFormatter.Parse` summary names "formatting, the error list, and the highlighting of the input". The output pane is highlighted through the same `Parse` (`Home.razor.cs:91` → `Render` → `Classify`). The summary is incomplete but not wrong.
- An emoji modifier sequence such as `1👍🏽` yields two CS1056 errors and so two marks, one per scalar. Each mark holds a whole pair and no character changes, but the cluster spans two elements. Whether a browser still draws one glyph across the boundary is for the validator to check.

## Follow-up candidates
_None._

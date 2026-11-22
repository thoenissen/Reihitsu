Role: auditor
Commit: 0fc8dcb
Verdict: pass

The prompt held no intended fix, suspected cause, or hoped-for verdict.

## Scope
Repair delta against `0605942` (audit 4, `28-auditor.md`, findings #8 and #9): `git diff 0605942...0fc8dcb`, the repair of Decisions 10 and 11 in `00-lead.md`. When the audit started, the checkout was `0fc8dcb`. `origin/main` = `1788f94d265c4e13052d5145404ae3ffd21e6acc` (`git ls-remote`) and is merged. HEAD then moved to `4813350`, which adds only `33-spec-checker.md`. The diff matches handovers 29–32: `PlaygroundHighlighter.cs` (lines that hold U+0000 are written as frames: `HoldsNullCharacter`, `CreateLine`, `OpenSpan`, `CloseSpan`, `AppendText`; the `'\0'` case is removed from `AppendEncoded` and its summary is corrected), `PlaygroundHighlightedLine.cs` (`Markup` string replaced by `Content` fragment), `app.css` (the trailing lone CR is removed), and the new tests `NullCharacterTests.cs` and `PlaygroundBrowserDocument.cs` (no `#pragma`, Decision 11). Scope did not grow: no spec, change doc, rule page, or other surface is touched, and the round adds 145/34 production lines in one file plus a property change.

The cleaner's Release build and 103/103 Playground run are on `b968e33`, which has the same `src/` and `tests/` as `0fc8dcb`. I did not re-run tests. I probed one suspicion in a real browser, because the implementer did not and the answer decides whether #8 is closed. The question was whether a text frame's U+0000 survives Blazor WebAssembly's render-batch string marshalling, which audit 4 had only probed as a raw DOM text node. Setup: `bin/Release` built after `42f86a4`, served on `localhost:5110`, Chrome via Playwright. Scripts are in `/tmp/claude-0/-home-user-Reihitsu/7ccecb21-b72f-5c00-b88a-569dde938365/scratchpad/audit34/`. The server was stopped afterwards.

## Previous findings
| # | Status | Evidence |
|---|---|---|
| 8 | closed | Every member the finding listed as failing now holds on the served build. Each check compares the copy's `textContent` with the text area's value, code unit by code unit; every U+0000 stays `0`, with no `fffd`. (1) Unmarked copy, pasted `// x␀y\r\nclass C { string s = "a␀b"; }\r\n`. (2) Output after Format: `… 22 61 0 62 22 …`. (3) Marked copy after a failing Format (`class C { string s = "a␀b"; `): the text is kept, colour spans are kept, and the mark element is present. (4) Incremental edits on a multi-line text: typing on another line, typing on the U+0000 line, deleting around it, and undo. In every state the copy equals the value. Deleting the U+0000 itself turns the line back into one markup frame (`<!--!-->` prefix), and undo turns it back into frames. A line written as frames is as wide as plain text of the same length (142.875 px against 142.828 px). The members that held in audit 4 (`&`, `<`, `>`, CR, U+0001, U+0008, U+000B, U+000C, U+007F, U+0085, U+2028, U+FEFF) still go through the unchanged `AppendEncoded` path, and `CarriageReturnAndOtherControlCharactersAreKeptInTheCopyAndTheOutput` pins them. The `AppendEncoded` summary now states what the code does. |
| 9 | closed | `app.css` ends with `…;\r\n}`, byte-equal to `origin/main`'s tail. |

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|

_None._

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| A line is written as frames instead of markup (`HoldsNullCharacter`) | `RenderLines` checks the line `[lineStart, lineEnd)`, its line feed included, and that is exactly the range `RenderText` writes for the line. `RenderMarkedText` checks the whole text, which is exactly what `RenderRange` writes as the one marked line | `AppendEncoded`, which now writes U+0000 raw and therefore must never see it. Every call of `AppendEncoded` goes through `AppendText` with `frames == null`, and the range it gets is a sub-range of the checked line | agree: whenever a written range holds U+0000, its line was checked over a superset of that range. An empty last line has length 0, so `IndexOf` over 0 characters returns -1 and the line is rendered as markup with no content, which is correct | ok |
| Empty line content omitted (`CreateLine`, `markup.Length == 0` → `null`; `CreateContent`, `Content != null`) | the markup written for the line | Blazor shows empty markup as a space, so an empty line must get no markup frame | agree. A line written as frames is never empty, because it holds U+0000. This matches the earlier `Markup.Length > 0` test | ok |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Markup escaping and line rendering | `rg -n "&amp;\|&lt;\|&#\|AddMarkupContent\|AddContent\|OpenElement\|playground-line" src/Reihitsu.Playground` finds them in `PlaygroundHighlighter` only (`AppendEncoded`, `OpenSpan`, `CloseSpan`, `AppendText`, `CreateLine`, `CreateContent`). The frame path and the markup path share `RenderRange`/`RenderText`, so categories and marks are decided once | one owner |
| Whether text may go through markup (U+0000) | `rg -n "'\\\\0'" src/Reihitsu.Playground` finds only `HoldsNullCharacter` | one owner |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `NullCharacterTypedIntoTheInputIsKeptInTheCopy`, `NullCharacterIsKeptInTheCopyOnEveryKindOfLine` | a typed U+0000 is shown unchanged in the copy: alone on a line, in a comment, in a string, in disabled text, twice in a row, at the start and at the end, on LF and CR LF lines | U+FFFD or a dropped character, named by its code unit | `PlaygroundBrowserDocument` (text frames become exact text nodes, markup frames go through the HTML parser in a `template`, empty markup becomes a space) | adequate. 29 watched the tests fail on the old code, each only on `0000`→`fffd`. The helper's model of text frames is confirmed in Chrome (Previous findings, #8) |
| `NullCharacterIsKeptInTheCopyAfterFormat`, `NullCharacterIsShownInTheOutput` | U+0000 survives a successful Format in both panes | same | same | adequate. They are separate tests, so a failure of one does not hide the other |
| `NullCharacterIsKeptInACopyWithErrorMarks` | the marked copy keeps U+0000 and the mark keeps its place (PLAY-014) | U+FFFD, or a mark missing or moved | same, `PlaygroundMarkup.GetMarks` | adequate. 31's scratch fix B (a plain text frame without spans) fails on the mark |
| `CarriageReturnAndOtherControlCharactersAreKeptInTheCopyAndTheOutput` | the characters audit 4 found to hold still hold | any of them changed | same | adequate. It is the counterpart of #8's class |

Colours on a line that holds U+0000 are not pinned by any test, and Decision 10 accepts losing them. The implementation keeps them, which the browser probe confirmed (`hl-keyword`, `hl-identifier`, and `hl-string` spans on the U+0000 line).

## Failure classes dismissed
- FC-001, FC-002, FC-003: see Guard scope. The check's span equals or contains every range written for the line. Both sides of the boundary are tested: the line with U+0000 and the lines without it on the same text (`…OnEveryKindOfLine`), and the transition back and forth by deleting and undoing (browser probe).
- FC-015, FC-016: one owner for each policy. #8's class was enumerated in audit 4, and every member is re-checked above.
- FC-017, FC-033: the harness that hid #8 (bUnit reading markup through AngleSharp) is bypassed by `PlaygroundBrowserDocument`, and the browser confirms what the helper models.
- FC-031: the summaries of `HoldsNullCharacter`, `CreateLine`, `CreateContent`, `RenderRange`, `RenderText`, `OpenSpan`, `CloseSpan`, `AppendText`, `AppendEncoded`, and `PlaygroundHighlightedLine.Content` and its constructor describe the new code. The inline comment about empty markup moved into the `CreateLine` summary, next to the code it explains.
- FC-032: the cleaner's 103/103 run and the Release build are on code identical to `0fc8dcb`. The browser probe used a Release build made after `42f86a4`, the last commit that touched `src/`.
- FC-034: the tests use the reported shape itself (U+0000 in a string literal, pasted or typed, in both panes).
- FC-035: the expected values are the input text or `PlaygroundFormatter.Format(input).Text`, independent of the rendering code.
- FC-044: the repair refuses the class by a cheap check on its cause (U+0000 present), not by modelling the HTML parser.
- FC-004 to FC-014, FC-018 to FC-030, FC-036 to FC-043, FC-037: the repair touches only the Playground display. No formatter phase, analyzer, code fix, unsupported shape, or comment-only change is involved.

Check 8: no `TODO`, no commented-out code, and no tracker number in the changed source or tests. There is no `#pragma` or `SuppressMessage` (Decision 11). Check 9: the edits stay within Decisions 10 and 11. Check 10: no expected value of a visible surface changes beyond PLAY-010 and PLAY-014 as written. Check 11: the frame path serves a rare input class, but it is the refusal Decision 10 orders. Keeping the mark needs elements around the text, and a cheaper variant (31's scratch fix B, one plain text frame) drops the mark that PLAY-014 requires. The colour spans come free through the shared `RenderText`, so no second renderer exists.

## Architecture findings
_None._

## Hints
- The markup `StringBuilder` is still allocated for a line that is written as frames, and it stays unused there. This is harmless.
- Each line's content is now a fragment inside a region frame, where before it was a bare markup frame. That adds one region frame and one delegate call per line on each render. It is not an obvious trap, but the validator's re-measurement of typing speed at 1000 lines (Decision 9's bar) is the check that settles it.

## Follow-up candidates
_None._

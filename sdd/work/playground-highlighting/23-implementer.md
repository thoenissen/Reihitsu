Role: implementer
Commit: 7791652
Verdict: tests pass

Repair round 2, Decision 9 (V1, V2), after `21-validator.md` and `22-test-implementer.md`. The prompt held no intended fix, suspected cause, or hoped-for verdict; it named handover 22's limitation (which fixes the width harness recognizes), and the fix chosen is one of the forms 22 verified against Chrome.

On d664c06 `PanesKeepTheirColumnWidthWhenTheirTextIsWider` failed as 22 says. On 7791652 all 88 Playground tests pass.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Playground/wwwroot/css/app.css` | V1: grid tracks `repeat(2, minmax(0, 1fr))`, narrow screen `minmax(0, 1fr)`, so a column no longer grows to its text and the pane scrolls inside itself. V2: `.playground-line { display: block }`; the "empty last line" `::after` moves from the copy to its last line | PLAY-012; PLAY-010 (layout of the copy unchanged) |
| `src/Reihitsu.Playground/PlaygroundHighlighter.cs` | `Render` replaced by `Highlight(text, marks, previous)`. Text is rendered as markup strings, one keyed `span.playground-line` per line (one line for the whole text while marks exist, so a mark across a line break stays one element). With the previous highlighting: the change is the common prefix/suffix difference, the tree is reparsed incrementally, a child that the parser reused (`IsIncrementallyIdenticalTo`) keeps its earlier colours shifted, only other tokens are classified again, and only lines touching that region are re-rendered; untouched lines keep their key object, so Blazor leaves their DOM alone. `RenderText` stops at the first span behind its range (it scanned every later span, quadratic once called per line). Characters are escaped for markup; CR and NUL as character references | PLAY-010, PLAY-014 (output unchanged) |
| `src/Reihitsu.Playground/PlaygroundHighlighting.cs` (new, public) | The result of `Highlight`: `Content`, plus the text, tree, coloured spans, and lines the next call reuses | PLAY-010 |
| `src/Reihitsu.Playground/PlaygroundHighlightedLine.cs` (new, internal) | One rendered line: length and markup; the instance is the element key | PLAY-010 |
| `src/Reihitsu.Playground/PlaygroundFormatter.cs` | `Reparse(previous, change)` next to `Parse`, so parsing stays in the one owner (Decision 4, F3) | PLAY-005 |
| `src/Reihitsu.Playground/Pages/Home.razor(.cs)` | `_marks` replaced by `_inputHighlighting`, updated in `FormatInput` and `OnInputChanged`; the copy renders `_inputHighlighting.Content` | PLAY-010, PLAY-014 |

## V2 — cause, measured in the browser

Setup: `scripts/build.sh` (Release), `dotnet run --no-build -c Release --launch-profile http` (port 5110), Playwright from `/opt/node22` on `/usr/bin/google-chrome`, viewport 1280x800, generated file of N lines (a class with commented one-line methods). Scripts in `/tmp/claude-0/-home-user-Reihitsu/7ccecb21-b72f-5c00-b88a-569dde938365/scratchpad/perf/` (`real.js` is the acceptance measurement; `perf.js`, `fill.js`, `dom.js`, `layout.js`, `narrow.js` the rest; `*.cpuprofile` are CDP CPU profiles).

Three causes, found in order with CDP CPU profiles and temporary stopwatch logging (removed):

1. **Blazor applying one DOM edit per coloured token.** On the tree before this round, a programmatic one-event paste of 1000 lines took 4.4 s, 3.6 s of it in `blazor.webassembly.js` (`insertText`, `insertElement`, and its logical-child bookkeeping, which is linear per insert, so quadratic overall). Rendering markup strings removed it (paste 0.5 s).
2. **Full reparse and reclassification per keystroke** in the interpreted runtime: parse ~180 ms, token walk ~100 ms at 1000 lines; browser layout of the whole copy ~40-58 ms (`dom.js`: replacing the whole copy 42 ms, one inline line 15-23 ms, one block line 1 ms). Incremental reparse ~15 ms, incremental classification ~17 ms, block lines keyed per line.
3. **The validator's 27 s / >120 s came mostly from the measuring method.** Playwright `fill` (and `keyboard.insertText`) of multi-line text dispatches one `input` event per line and per line break (300 lines: 601 events; 1000 lines: 2001 events), each a full re-highlight of a growing text. A real clipboard paste (`Ctrl+V`) dispatches **one** event (measured: `pasteInputEvents: 1`). `fill` is now 10 s for 300 lines (was 27 s) and 102 s for 1000 lines; it still measures thousands of keystrokes, not a paste.

### Measurements against the bar (Release, `real.js`, clipboard paste and real key presses; "frame" = from `keydown`/`paste` to the next animation frame after the copy changed)

| Lines (chars) | Paste, 1 event | Keystroke median / max (10) | Enter median / max (5) | Keystroke after Format, median / max (10) |
|---|---|---|---|---|
| 300 (15 900) | 221 ms | 32 / 40 ms | 33 / 55 ms | 30 / 77 ms |
| 1000 (54 400) | 705 ms | 90 / 118 ms | 81 / 93 ms | 90 / 194 ms |
| 3000 (165 891) | 2172 ms | 244 / 303 ms | 262 / 293 ms | 259 / 642 ms |

Before this round (same machine, programmatic one-event paste, `perf.js`): 1000 lines paste 4443 ms, keystroke ~420-630 ms.

Against Decision 9: pasting 1000 lines re-colours in ~0.7 s (bar: about 2 s or less) — **met**. A keystroke at 1000 lines: median 90 ms, single samples to 118 ms, the first keystroke after Format once 194 ms — **about at the bar, not clearly under it**. What remains is linear in the text and outside what this change can cut cheaply: Blazor marshals the whole textarea value with every `input` event, the coloured-span list (8 000 entries) is copied, and every render rebuilds and diffs the frames of every line of both panes (the output pane re-renders its unchanged lines on each keystroke, too). Copy and textarea text were equal after every run (`copyMatchesInput: true`).

Equivalence of the incremental path: a throwaway harness (not committed, `/tmp/claude-0/-home-user-Reihitsu/7ccecb21-b72f-5c00-b88a-569dde938365/scratchpad/ZzIncrementalFuzz.cs`) applied random edits (inserting `/*`, `*/`, quotes, `#if`/`#else`/`#endif`, doc-comment starts, raw-string quotes, `partial`, `async`, `where T : class`, CR LF, U+2028, NUL; deletions; replacements) to repository C# files and compared every incremental result (lines and coloured spans) with a fresh highlighting of the same text, also across marked/unmarked transitions: no difference in 60 000 edits over two runs on the committed code (300 files each from `src` and `tests/Reihitsu.Formatter.Test`), nor in 107 200 edits of earlier runs after the fixes below. The first harness runs found two real defects in my first design, both fixed before commit: `SyntaxTree.GetChangedSpans` reports text differences only, so a token whose syntax changes without a text change (text behind an inserted `#else` that leaves a doc comment) kept stale colours — replaced by the reuse check; and a token whose text shrank next to the changed span was missed.

V1 in Chrome (`layout.js`, `narrow.js`): 400-`x` line, 1280 px: input pane client 566 / scroll 3687, End scrolls the pane (`scrollLeft` 3121), page `scrollWidth` 1280; output client 566 / scroll 3667. 600 px: client 550, page `scrollWidth` 600, End scrolls the pane (`scrollLeft` 3137). Block lines: every line 21 px; empty input, trailing line feed, and blank lines show one line each, matching the text area.

## Policy owners
- Parsing: `rg -n "ParseText|WithChangedText" src/Reihitsu.Playground` → only `PlaygroundFormatter.Parse` and `PlaygroundFormatter.Reparse`.
- Colouring and mark rendering: `rg -n "hl-|playground-mark|playground-line" src/Reihitsu.Playground --glob '*.cs'` → only `PlaygroundHighlighter`.

## Production diff size
`git diff --shortstat origin/main...HEAD -- src`: 8 files changed, 1112 insertions(+), 49 deletions(-). Delta of this round: 7 files changed, 534 insertions(+), 59 deletions(-). New types: `PlaygroundHighlighting` (public), `PlaygroundHighlightedLine` (internal).

## Deleted special cases
_N/A._

## Tests run
- `scripts/test.sh --configuration Debug --project playground` → Passed! Failed: 0, Passed: 88, Skipped: 0, Total: 88.
- `scripts/test.sh --configuration Debug --project architecture` → Passed! Failed: 0, Passed: 198.
- `scripts/build.sh` (Release, analyzers on) → 0 warnings, 0 errors.
- `scripts/format.sh` over every changed `.cs` file → no further change.

## Disputed tests or scope
- **Scope grew** (`README.md`, *Scope*): this round adds 534 lines and two new types, close to doubling the production paths of the run, for V2. The lead decides. A smaller alternative that meets only the paste bar is cause 1 alone (one markup string per render, ~60 lines, no new type): paste 1000 lines ~0.5 s, keystroke ~450 ms.
- **Spec metadata**: `PlaygroundHighlighting` and `PlaygroundHighlightedLine` are missing from the Code owners of `sdd/specs/playground/playground.md` (as in Decision 3) — spec author.
- **Test candidate for the test implementer**: the incremental path's correctness is checkable without timing — after a sequence of edits through `#playground-input`, the copy's markup equals the copy of a freshly rendered page with the final text (the throwaway harness above does this on the highlighter directly).
- **The validator's method**: measure paste with a clipboard paste (`real.js`), not `locator.fill`, which dispatches one input event per line.

## Architecture findings
_None._

## Follow-up candidates
- Keystroke latency at 1000+ lines (still ~linear, ~90 ms at 1000 and ~250 ms at 3000 lines): coalescing input events that arrive while a highlight is pending (also makes key repeat and `fill` cheap), and keeping the output pane out of input-only renders. Both change the page's event handling; not done here.

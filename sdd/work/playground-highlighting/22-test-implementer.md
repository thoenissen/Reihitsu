Role: test implementer
Commit: 245ef82
Verdict: tests written

Repair round 2, Decision 9 (V1, V2) from `00-lead.md` and `21-validator.md`. The prompt held no intended fix, suspected cause, or hoped-for verdict. The new test asserts a behavior (no box from a pane up to the page takes its width from the pane's text) and accepts any fix CSS allows; it names no property the fix must set.

On this commit: 88 tests, 1 fails (`PanesKeepTheirColumnWidthWhenTheirTextIsWider`), 87 pass. `scripts/test.sh --project architecture` passes (198).

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `LayoutTests.PanesKeepTheirColumnWidthWhenTheirTextIsWider` (input pane `.playground-editor` and output pane `#playground-output`, wide and narrow screen; observed: `div.playground-column is a grid item in the track '1fr', whose minimum it fills with its content (its min-width is 'auto' and it does not scroll)`) | PLAY-012 | `PlaygroundStyles.CreateDocument` (wide and `narrow: true`), new `PlaygroundWidths.FindWidthFromText` | fails |
| `LayoutTests.NarrowScreenStacksTheColumnsAndAppliesItsPaneHeight` (changed: "stacked" now means one column track, read by `PlaygroundWidths.GetTracks`, instead of the literal value `1fr`) | — (styling, Decision 5) | `PlaygroundStyles` | passes |

**Level.** The repository runs no browser tests (`rg -il playwright` over the test projects, scripts, and workflows finds nothing), so per the prompt the test works on the stylesheet cascade (AngleSharp computed style). AngleSharp computes no layout, so the new helper `tests/Reihitsu.Playground.Test/PlaygroundWidths.cs` applies the CSS sizing rules of the layouts the page uses, on the computed style of each box from the pane up to `html`: a box's width follows its content when it has a `*-content` width, shrinks to fit (inline-level, table, float, absolute), is a flex item not stretched across a column, is an item of a flex row with a content-based basis or minimum, or is an auto-placed grid item whose track is sized from its items or has an automatic minimum (`auto`, so also `Nfr`) that the item fills with its content (`min-width: auto` and not a scroll container). Size containment (`contain: size | inline-size | strict`) or a fixed `width`/`max-width` on any box from that box down to the pane keeps the text out of the size. `grid-template-columns` is read with `repeat(n, …)` and `minmax(…)`.

**The change to the narrow-screen test** is needed so that a `minmax(0, 1fr)` track, a legitimate fix of V1, does not fail a test that only meant "stacked". The implementer had not yet worked against it in this round.

### Harness check: the model against Chrome

Each candidate was applied to a scratch copy of `app.css` (restored afterwards, nothing committed), run through the test, and rendered in Chrome (`/usr/bin/google-chrome`, Playwright from `/opt/node22`) on a static page with the page's structure and a 400-`x` line, at 1280 px and 600 px. Script: `/tmp/claude-0/-home-user-Reihitsu/7ccecb21-b72f-5c00-b88a-569dde938365/scratchpad/widths/check.js`.

| Stylesheet | Test | Chrome, pane client width (1280 / 600 px) |
|---|---|---|
| today's `app.css` | fails | 3662 / 3662 (pane grows, page scrolls) |
| `+ .playground-column { min-width: 0; }` | passes | 566 / 550, scroll width 3662 |
| grid `minmax(0, 1fr) minmax(0, 1fr)`, narrow `minmax(0, 1fr)` | passes | 566 / 550 |
| grid `repeat(2, minmax(0, 1fr))`, narrow `minmax(0, 1fr)` | passes | (same as above) |
| `+ .playground-column { overflow: hidden; }` | passes | 566 / 550 |
| `+ .playground-editor, .playground-output { min-width: 0; }` (does not fix) | fails | 3662 / 3662 |
| base grid `minmax(0, 1fr) …` only, narrow rule still `1fr` (fixes wide only) | fails (narrow) | 566 / 3662 |

The test and Chrome agree on every row.

## V2 — no test

No deterministic test can pin the fix. Observation, no diagnosis: in bUnit (JIT, no browser), entering a 1000-line input into `#playground-input` re-renders in 51 ms and one more character in 95 ms (300 lines: 116 ms and 34 ms; throwaway measurement, not committed). The slowness the validator measured (300-line fill 27 s) does not appear in the harness the tests run in, so no test there reproduces it. A deterministic proxy — a render count, an element count, a call count of the highlighter — would assert a mechanism the implementer has not chosen yet and that may not be the cause; a timing bound is excluded by the lead. The acceptance bar of Decision 9 stays the validator's browser measurement.

## Not covered, and why

- **`contain: inline-size` as a fix.** AngleSharp drops `contain: inline-size` from the computed style (probe: the declaration computes to empty; `contain: strict` is kept), so a fix that uses it would fail the test although Chrome honours it. The model accepts it in principle; if the implementer picks it, route back to me.
- **An explicitly placed grid item** (`grid-column`). The page auto-places both columns; the helper throws instead of guessing.
- **The input text area inside the editor.** The text area and its copy lie inside the scroll box `.playground-editor` (the `width: max-content` layer is the scrolled content); the walk starts at the scroll box, the pane PLAY-012 means. The existing `PanesScrollHorizontally` pins that the panes scroll.
- **The caret staying in view after End.** Follows from the pane scrolling inside itself; a browser check for the validator.
- **Both colour schemes.** The schemes change only colour tokens; the walk runs on the light scheme. `rg -n "prefers-color-scheme" -A30 src/Reihitsu.Playground/wwwroot/css/app.css` shows only `--color-*` declarations in it.

## Commands

```shell
scripts/test.sh --project playground --filter "FullyQualifiedName~PanesKeepTheirColumnWidthWhenTheirTextIsWider|FullyQualifiedName~NarrowScreenStacksTheColumns"
scripts/test.sh --project playground
```

## Follow-up candidates

_None._

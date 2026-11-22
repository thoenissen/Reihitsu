Role: validator
Commit: 154ba06e7a25d3a52406b7095c6218cdb6298c9e
Verdict: pass

The prompt held no intended fix, suspected cause, or hoped-for verdict. The run covered the final tree at 154ba06, which contains 7791652 (repair round 2) and the handovers after it. Only the handover file is added in this run.

## Steps
| Step | Result | Notes |
|---|---|---|
| scripts/prepare.sh | ok | SDK 11.0.100 already present; nothing installed |
| scripts/build.sh | ok | Release; 0 warnings, 0 errors |
| scripts/test.sh --no-build | ok | Analyzer 11885/11885; Formatter 7188/7188; Core 887/887; Cli 440 passed, 1 skipped (platform skip, as in 21); ArchitectureTests 198/198; Tooling 204/204; Playground 103/103 (was 88 in 21; the test set grew in this run). Total 0 failed |
| line-count rows | ok | `scripts/test.sh --no-build --project selfhosting --filter "Name~RH2008|Name~RH2111"`: 18/18 passed. Run because the diff adds lines to `tests/Reihitsu.Playground.Test/*.cs` |

Logs: `/tmp/claude-0/-home-user-Reihitsu/7ccecb21-b72f-5c00-b88a-569dde938365/scratchpad/test35.log`, `.../scratchpad/linecount35.log`, `.../scratchpad/build.log`.

## Failures
None.

## Browser checks (Release, served on localhost:5110, Chrome for Testing via `/usr/bin/google-chrome`)
These are observations, not test results. Scripts are under `/tmp/claude-0/-home-user-Reihitsu/7ccecb21-b72f-5c00-b88a-569dde938365/scratchpad/browser/` (`check.js`, `caret.js`, `more.js`) and `.../scratchpad/perf/` (`layout.js`, `real.js`).

| Check (from 21) | Result | Observed |
|---|---|---|
| Non-BMP character (`1😀`) not split or replaced | run | Copy text equals input; no U+FFFD; the mark holds `1f600` as one unit |
| U+2028 as a line break (09 finding 1(b)) | run | Alert reads `line 2, column 11 — CS1002: ; expected`; the mark holds `}` |
| Empty span at a line break is one character wide (PLAY-014) | run | Mark width 8.41 px, equal to one character width (8.41 px) |
| Wavy underline colour | run | `text-decoration` underline, style `wavy`, colour `rgb(209, 36, 47)` |
| Output pane named and focusable (PLAY-002, PLAY-012) | run | `role="region"`, `aria-labelledby="playground-output-label"`, `tabindex="0"` |
| No wrapping in either pane | run | `white-space: pre` on the copy and the text area |
| Horizontal scrolling of a pane whose text is wider than the pane (PLAY-012, V1) | run, **met** | Implementer's `layout.js` reproduces on this tree: input pane client 566 px, scroll width 3687 px; End scrolls the pane (`scrollLeft` 3121); `document` scroll width 1280 px; output pane client 566 px, scroll width 3667 px |
| Caret visible after End (V1) | run, **met** | Text area right edge at 631 px inside the 1280 px viewport after End; `taSelectionEnd` equals the text length |
| Served stylesheet equals the source at HEAD | checked | `minmax(0, 1fr)` at `app.css` line 136 and in the served `/css/app.css`; the narrow-screen rule matches |
| Console errors | run | One `404 Not Found` resource error on page load, not identified (21 reported the same). No page errors |

Correction to 21: its horizontal-overflow finding (panes grow to 3687 px) was measured on the tree before 7791652 (59c69cc). On 154ba06 the panes scroll inside themselves. In my own probes the element I read as "the box" was the editor's wrapper, whose width is the content width and which is not the pane; the pane measurement is the one from `layout.js`.

## Speed, real clipboard paste and real key presses (Decision 9)
Method: `real.js`. Paste is `Ctrl+V` from the clipboard, one input event (`pasteInputEvents: 1` at both sizes). Keystrokes are `keyboard.press` and `keyboard.type`, not `fill`. "frame" is from keydown or paste to the next animation frame after the copy changed. Values in milliseconds.

| Lines (chars) | Paste, 1 event | Keystroke median / max (10) | Enter median / max (5) | Keystroke after Format (10 keys, median / max) |
|---|---|---|---|---|
| 300 (15 900) | 263 frame (244 DOM) | 29 / 44 | 32 / 49 | 32 / 96 |
| 1000 (54 400) | 803 frame (721 DOM) | 93 / 126 | 106 / 114 | 94 / 251 |

Against Decision 9 (bar: paste of a 1000-line file re-colours in about 2 s or less; a keystroke at 1000 lines under about 100 ms):
- Paste 1000 lines: 803 ms to the frame, 816 ms after paint. Under the bar.
- Keystroke at 1000 lines: median 93 ms, max 126 ms; Enter median 106 ms. The median is under 100 ms; the max and Enter median are above it. The first keystroke after Format took 251 ms; the other nine were at most 127 ms.
- The bar is not met on every sample: the keystroke median is just under it and the maximum and Enter median are above it. The lead decides what that means under Decision 9.

Copy and text area equal after every run (`copyMatchesInput: true`).

## Not checked
- Wider text (3000 lines): not run in this handover.
- Narrow viewports (600 px) for horizontal scroll: observed by the implementer in 23, not repeated here.
- Emoji modifier clusters: not exercised.

## Log
`/tmp/claude-0/-home-user-Reihitsu/7ccecb21-b72f-5c00-b88a-569dde938365/scratchpad/test35.log` (default run), `.../scratchpad/linecount35.log` (line-count rows), `.../scratchpad/build.log`, `.../scratchpad/server35.log`.

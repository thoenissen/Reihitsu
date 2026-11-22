Role: validator
Commit: 59c69cc1b7fdfc40c16c2d31c9b55122edfee0b1
Verdict: pass

The prompt held no intended fix, suspected cause, or hoped-for verdict. The run covered the final tree: `59c69cc` differs from the audited `b63ec36` only in `sdd/work/playground-highlighting/00-lead.md` and `20-auditor.md` (Markdown, no compiled file).

## Steps
| Step | Result | Notes |
|---|---|---|
| scripts/prepare.sh | ok | SDK 11.0.100 already present at `/root/.dotnet`; nothing installed |
| scripts/build.sh | ok | Release; 0 warnings, 0 errors |
| scripts/test.sh --no-build | ok | Analyzer 11885/11885; Formatter 7188/7188; Core 887/887; Cli 440 passed, 1 skipped (`OnWindowsADirectoryDifferingOnlyInCaseIsTheSameDirectory`, a platform skip), 0 failed; ArchitectureTests 198/198; Tooling 204/204; Playground 87/87 (`Reihitsu.Playground.Test`, in the default set per `test.sh` `all)` branch). Total 0 failed. |
| line-count rows | ok | `scripts/test.sh --no-build --project selfhosting --filter "Name~RH2008|Name~RH2111"`: 18/18 passed. Run because the diff adds lines to `tests/Reihitsu.Playground.Test/*.cs`. |

Log of the default run: `/tmp/claude-0/-home-user-Reihitsu/7ccecb21-b72f-5c00-b88a-569dde938365/scratchpad/test.log`. Build log: `.../scratchpad/build.log`. Line-count rows: `.../scratchpad/linecount.log`.

## Failures
None. No test failed in any step.

## In-browser checks
Served `src/Reihitsu.Playground` in Release on `http://localhost:5110` (`dotnet run --no-build --launch-profile http`) and drove it with Playwright (`/opt/node22/lib/node_modules/playwright`) on `/usr/bin/google-chrome` (Chrome for Testing 153), viewport 1280 px wide. The server was stopped afterwards; the repository is unchanged by these checks. Scripts and screenshots are under `/tmp/claude-0/-home-user-Reihitsu/7ccecb21-b72f-5c00-b88a-569dde938365/scratchpad/browser/`. These are observations, not test results; I did not diagnose them.

| Check (from 04, 05, 09, 16, 20) | Result | Observed |
|---|---|---|
| Non-BMP character in input (`1😀`, U+1F600) is not split or replaced | run | Copy `textContent` equals the input; no U+FFFD in the rendered copy; the mark holds the whole emoji (`1f600`). Auditor 09 finding 1(a) is not reproduced in Chrome. |
| Emoji modifier cluster (16 hint) | not run | Not exercised. |
| U+2028 as a line break (09 finding 1(b)) | run | `class C {     int x }`: the alert reads `line 2, column 11 — CS1002: ; expected` and the one mark holds `}`. No mark is drawn on the line-break character. |
| Empty span at a line break is one character wide (PLAY-014, Decision 3A) | run | `int x = 1` followed by a newline: one mark, width 8.39 px, equal to the measured width of one character (8.41 px) in the same font. |
| Wavy underline colour | run | Mark: `text-decoration` underline, style `wavy`, colour `rgb(209, 36, 47)`. |
| Output pane is named and focusable (PLAY-002, PLAY-012, Decision 8) | run | `role="region"`, `aria-labelledby="playground-output-label"`, `tabindex="0"`. |
| Copy and text area aligned | run | Both at x = 0 in the measured layout. |
| No wrapping in either pane | run | `white-space: pre` on the copy and the text area. |
| Typing speed (skeptic performance note) | run, partly | Ten keystrokes at the end of a one-line input of about 230 characters: 105 ms. A single keystroke at 300 lines: 62 ms. A Playwright `fill` of a 300-line input took 27 s; a 1000-line `fill` did not finish within 120 s. The 3000-line typing check was not completed for this reason. |
| Caret stays visible past the right edge (05, 09) | run, **not met as observed** | See below. |
| Horizontal scrolling of a pane whose text is wider than the pane (PLAY-012) | run, **not met as observed** | See below. |

**Horizontal overflow (observed on the final tree).** Input `class C { void M() { int x = 1; ` followed by 400 `x`, and the same text formatted. The input pane `.playground-editor` grows to the width of its content: its client width and scroll width are both 3687 px, and its bounding box is 3689 px, in a 1280 px viewport. The output pane `#playground-output` likewise grows to 3680 px with its own scroll width equal to its client width. The page scrolls instead (`document.documentElement.scrollWidth` 4064 px for the input, 7450 px after formatting; `window.scrollX` 65 px after pressing End). After End in the input, the text area's right edge is at 3687 px, outside the 1280 px viewport, so the caret is off screen. PLAY-012 says a pane whose text is wider than the pane MUST scroll horizontally inside it, and the `overflow: auto` rule on these panes is not exercised because the panes are never narrower than their content. The validator does not say why; the lead routes it.

Not checked: the `:focus-visible` outline of the output pane (`app.css`), the stylesheet's narrow-screen height (adjacent fix of Decision 5), and the layout at narrow viewports.

Console: one `404 Not Found` on the first page load (`console` error event); the response listener in a second run recorded no response of status 400 or above. Not identified.

## Log
`/tmp/claude-0/-home-user-Reihitsu/7ccecb21-b72f-5c00-b88a-569dde938365/scratchpad/test.log` (default run, 50 lines of summary); `.../scratchpad/linecount.log`; `.../scratchpad/build.log`; `.../scratchpad/prepare.log`; browser scripts and `caret.png`, `shot.png` in `.../scratchpad/browser/`.

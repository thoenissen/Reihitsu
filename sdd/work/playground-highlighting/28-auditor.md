Role: auditor
Commit: 0605942afe86e739521e5bc52f95f54dd206f6a2
Verdict: findings

The prompt held no intended fix, suspected cause, or hoped-for verdict.

## Scope
Repair delta against `b63ec36` (audit 3, `20-auditor.md`, pass): `git diff b63ec36...0605942`, the repair of Decision 9 (V1 pane width, V2 typing speed) with maintainer answer 8. Checkout was `0605942` when the audit started; `origin/main` = `1788f94d265c4e13052d5145404ae3ffd21e6acc` (`git ls-remote`), merged. HEAD has since moved to `470f3ca`, which adds only `27-spec-checker.md`. The diff is what 22–26 describe: `app.css` (grid tracks `minmax(0, 1fr)`, block lines, `::after` on the last line), `PlaygroundHighlighter` (`Render` → incremental `Highlight`, markup strings), new `PlaygroundHighlighting` and `PlaygroundHighlightedLine`, `PlaygroundFormatter.Reparse`, `Home.razor(.cs)`, spec Code owners, tests `IncrementalHighlightingTests`, `LayoutTests`, `PlaygroundWidths`.

**Scope grew** (534 production lines, two new types, a new rendering mechanism: markup strings instead of text frames), approved by answer 8, so the repair surface got a full audit, not a delta. The cleaner's Release build and 97/97 Playground run are on `016fb28`; `0605942` adds Markdown only. Browser probes ran on the Release build of that code (`bin/Release` built after `7791652`), served on `localhost:5110`, Chrome via Playwright; scripts in `/tmp/claude-0/-home-user-Reihitsu/7ccecb21-b72f-5c00-b88a-569dde938365/scratchpad/audit28/`. Server stopped afterwards.

## Previous findings
Findings #1–#7 were closed in audits 2 and 3; nothing in this repair reopens them. `GetMarkSpan` (#1) is unchanged; the output `pre`'s `role="region"`, label, and `tabindex` (#4, #7) are unchanged in `Home.razor`.

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 8 | high | `src/Reihitsu.Playground/PlaygroundHighlighter.cs` `AppendEncoded` (the `'\0'` case writing `&#0;`) and its summary; reached from both panes through `CreateContent` → `AddMarkupContent` | FC-031, FC-017 | – | PR-introduced | PLAY-010: "The highlighted text MUST be exactly the text it colours: no character is added, dropped, or changed." The repair renders lines as markup, which Blazor inserts through the HTML parser, and writes U+0000 as `&#0;`. The HTML parser replaces a null character reference with U+FFFD (null-character-reference parse error), so the copy and the output show `�` where the text has U+0000. Counterexample on the served Release build: paste `class C { string s = "a<U+0000>b"; }` (Chrome's clipboard paste keeps U+0000 in the text area: value codes `… 61 0 62 …`); the input copy's `textContent` reads `… 61 fffd 62 …`, and after Format the output's `textContent` reads `… 61 fffd 62 …` too. On `b63ec36` both panes used text frames (`AddContent`), which keep U+0000 (Chrome: a text node keeps it, shown as a box glyph; the markup path shows the replacement-character glyph). The summary of `AppendEncoded` says the reference makes the page show "exactly these characters" and that it prevents the parser from dropping a null character, which is false. Members of the class (axis: a character the HTML parser changes in text content): `&`, `<` — escaped, hold; CR and CR LF — `&#13;`, holds (a numeric reference to U+000D is kept); U+0000 — **fails**, input copy and output, marked and unmarked paths alike (all go through `AppendEncoded`); U+0001, U+0008, U+000B, U+000C, U+007F, U+0085, U+2028, U+FEFF, U+FFFE, a lone U+D800 — probed raw through `template.innerHTML` in Chrome, all kept, hold. No test covers U+0000: the incremental tests compare against the same `Highlight` path, so they cannot see it (FC-033 by construction), and 25 dropped U+0000 from its sweep. | Keep U+0000 out of the markup path — for example render a line that holds it with text frames, or refuse highlighting for a text holding it and show it plain — or have the lead put an exemption for U+0000 to the maintainer; correct the `AppendEncoded` summary; add a page test that the copy and the output hold a pasted U+0000. Rare input class: the cheapest refusal is acceptable. |
| 9 | low | `src/Reihitsu.Playground/wwwroot/css/app.css`, last line | – | – | PR-introduced | The file now ends with `}\r` — a lone CR without LF; `main` and `b63ec36` end with `}`. Every other line ends with CR LF (`.editorconfig`: `end_of_line = crlf`). No effect on the stylesheet; a stray half line ending. | End the file with `}` (as on `main`) or `}\r\n`. |

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| A child of the reparsed tree keeps its earlier colours (`GetPreviousSpan` + `IsUnchanged`) | the child's full span, wholly in front of the change (end ≤ change start) or wholly behind it (start ≥ new change end), non-empty; same green node (`IsIncrementallyIdenticalTo`) at the shifted full span, for a node also every ancestor with that full span | the colouring (`AddToken`/`AddTrivia`/`GetTokenClass`), which depends only on the green node (kind, trivia, structured-trivia type), never on position or parent | agree: equal green node ⇒ equal categories, also for green nodes Roslyn caches and shares, so reuse need not mean "the parser reused it"; the boundary is inclusive on both sides and the sweeps of 25 insert and delete at every position | ok |
| Region of changed colours (`regionStart`/`regionEnd`) | the change plus the full span of every non-empty token classified again | `RenderChangedLines`' leading test (line ends, line break included, at or before `regionStart`) and trailing test (line starts strictly behind `oldRegionEnd`) | agree: text and categories outside the region are equal shifted; the trailing test is one character conservative | ok |
| Incremental lines or full render (`marks.Count == 0 && previous.IsMarked == false`) | the new marks and whether the previous lines are the one marked line | `RenderMarkedText` renders the whole text as one line | agree; the categories are incremental in both paths and complete | ok |
| Characters escaped for markup (`AppendEncoded`) | `&`, `<`, `>`, CR, U+0000 | the HTML tokenizer's data state and input preprocessing, which change `&`, `<`, CR, and U+0000 | disagree for U+0000: the reference it writes is itself replaced | finding #8 |
| Empty line markup omitted (`CreateContent`) | `line.Markup.Length > 0` | Blazor's `insertMarkup`, which shows empty markup as a space | agree; probed: empty input, `\n`, `\n\n`, blank lines all give 21 px lines equal to the text area | ok |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Parsing with PLAY-005's options | `rg -n "ParseText|WithChangedText" src/Reihitsu.Playground`: `PlaygroundFormatter.Parse` and `PlaygroundFormatter.Reparse` only | one owner |
| Colouring, line and mark rendering, markup escaping | `rg -n "&amp;|&lt;|&#|AddMarkupContent|SetKey|playground-line" src/Reihitsu.Playground`: `PlaygroundHighlighter` (`AppendEncoded`, `CreateContent`) and the two `.playground-line` rules in `app.css` | one owner |
| Pane width (PLAY-012) | `app.css` `.playground-grid` (base and narrow rule) | one owner |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `IncrementalHighlightingTests` (9 tests) | after any series of edits the copy (text, per-character categories, markup) equals the highlighting computed without an earlier version | a stale colour or line after an edit; watched fail on mutations M1, M2b, M2c (25) | bUnit page; `PlaygroundHighlighter.Highlight` with `previous` | adequate for equivalence. By construction it cannot see a defect the fresh path shares (finding #8); the fresh path's colours stay pinned by `HighlightingTests` |
| `LayoutTests.PanesKeepTheirColumnWidthWhenTheirTextIsWider` | no box from a pane up to the page takes its width from the pane's text, wide and narrow | a pane growing to its text; fails on `245ef82`, matched against Chrome on seven stylesheets (22) | `PlaygroundWidths.FindWidthFromText` (CSS sizing model on AngleSharp computed style) | adequate at the harness's level; confirmed in Chrome on the served build: 400-`x` line, editor client 566 / scroll 3687, page `scrollWidth` 1280 |
| `LayoutTests.NarrowScreenStacksTheColumnsAndAppliesItsPaneHeight` (changed) | one column track on a narrow screen | two tracks | `PlaygroundWidths.GetTracks` | adequate; the change admits `minmax(0, 1fr)` without weakening "stacked" |

## Failure classes dismissed
- FC-001, FC-002, FC-003: the reuse, region, and line-reuse guards agree with their counterparts (table above); both sides of each boundary are swept by 25.
- FC-015: one owner per policy (table above).
- FC-016: the escaping class is enumerated in finding #8; the reuse decision has no sibling copy.
- FC-017, FC-031, FC-033: finding #8. Other summaries of the repair (`Highlight`, `ClassifyChanged`, `GetPreviousSpan`, `IsUnchanged`, `RenderChangedLines`, `RenderLines`, `CreateContent`, `Reparse`, `PlaygroundHighlighting`, `PlaygroundHighlightedLine`, `Home._inputHighlighting`, `OnInputChanged`) describe the new code.
- FC-020, FC-022: directives, `#else`, doc comments, verbatim and raw multi-line strings, and CR LF are in the sweeps of 25 and its named shape tests.
- FC-032: the cleaner's 97/97 run is on the final code (`016fb28`); later commits are Markdown.
- FC-035: the incremental tests derive expected values from the non-incremental path by design (equivalence); the expected colours themselves come from `HighlightingTests`, written against PLAY-010's examples.
- FC-004 to FC-014, FC-018, FC-019, FC-021, FC-023 to FC-030, FC-034, FC-036 to FC-044: the repair touches only the Playground's display; no formatter phase, analyzer, code fix, rename, or unsupported shape changes, and the formatter output the Playground shows is unchanged.

Check 8: no `TODO`, no commented-out code, no tracker number in the changed source and tests. Check 9: in scope (Decision 9, answer 8). Check 10: no expected value of a visible surface changes; V1 restores PLAY-012 as written. Check 11: no special case for a rare input class added; finding #8's remedy may be one, and a cheap one is acceptable.

## Architecture findings
_None._ Finding #8 is one encoding function and its rendering path; no shared decision between components.

## Hints
- `PlaygroundFormatter.Reparse` states that its tree is the one `Parse` returns for the new text. That is Roslyn's incremental-parse invariant, not something the Playground checks; the fuzz of 23 (167 200 edits) and the sweeps of 25 found no difference. If Roslyn ever diverges, only colours differ, never the formatter's input (`Format` parses afresh).
- `AppendEncoded` also escapes `>`, which the HTML parser does not need; harmless.
- In the copy, `a<U+2028>b` renders as one line (21 px), as on `b63ec36`; unchanged by the block lines.

## Follow-up candidates
_None._

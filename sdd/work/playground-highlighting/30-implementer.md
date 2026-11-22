Role: implementer
Commit: 42f86a4
Verdict: tests pass

Repair round 3, Decision 10 (`00-lead.md`), findings 8 and 9 of `28-auditor.md`, tests of `29-test-implementer.md`. The prompt held no intended fix, suspected cause, or hoped-for verdict.

On de1fe1d the five `NullCharacterTests` failed as 29 says (U+0000 shown as U+FFFD). On 42f86a4 all 103 Playground tests pass. The run used the working tree, which held the test implementer's parallel, uncommitted edit of `tests/Reihitsu.Playground.Test/PlaygroundBrowserDocument.cs`; nothing under `tests/` is in my commit.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Playground/PlaygroundHighlighter.cs` | F8, the refusal Decision 10 names: a line that holds U+0000 (`HoldsNullCharacter`, one `IndexOf` over the line; for a marked text, over the whole text, which is one line) never goes through the markup path. It is written as frames: element frames for coloured parts and marks, text frames for the text, so the page keeps every character. The rendering functions `RenderRange`/`RenderText` take the frame list next to the markup builder and write spans and text through `OpenSpan`, `CloseSpan`, `AppendText`, so colours and marks stay one owner and also stay on such a line (nothing is lost). `CreateLine` builds the line's content (frames, markup, or none for an empty line); `CreateContent` adds it. `AppendEncoded` no longer writes `&#0;`, and its summary now states what it does: it escapes `&`, `<`, `>`, writes CR as `&#13;`, and holds no U+0000, which markup parsing replaces raw or as a reference | PLAY-010, PLAY-014 |
| `src/Reihitsu.Playground/PlaygroundHighlightedLine.cs` | `Markup` (string) replaced by `Content` (`RenderFragment`, `null` for an empty line), so a line can be markup or frames | PLAY-010 |
| `src/Reihitsu.Playground/wwwroot/css/app.css` | F9: the trailing lone CR removed; the file ends with `}` as on `main` (byte-checked against `origin/main`) | – |

Lines without U+0000 render exactly as before: one markup frame per line, the same markup string, now added through a fragment (`builder => builder.AddMarkupContent(0, markup)`) inside the line's element. The page markup bUnit reads is unchanged for them (all incremental and highlighting tests pass unchanged).

## Policy owners
- Markup escaping and line rendering: `rg -n "&amp;|&lt;|&#|AddMarkupContent|AddContent|OpenElement|playground-line" src/Reihitsu.Playground` → `PlaygroundHighlighter` (`AppendEncoded`, `OpenSpan`, `CloseSpan`, `AppendText`, `CreateLine`, `CreateContent`) and `Home.razor`'s own page elements; one owner.
- Parsing: unchanged (`PlaygroundFormatter.Parse`/`Reparse`).

## Production diff size
`git diff --shortstat origin/main...HEAD -- src`: 8 files changed, 1222 insertions(+), 48 deletions(-) after this round (1112/49 before). Delta of the round: 3 files changed, 145 insertions(+), 34 deletions(-), most of it XML summaries of the five new private helpers. No new type.

## Deleted special cases
_N/A._

## Tests run
- `scripts/test.sh --configuration Debug --project playground` → Passed! Failed: 0, Passed: 103, Skipped: 0, Total: 103.
- `scripts/test.sh --configuration Debug --project architecture` → Passed! Failed: 0, Passed: 198.
- `scripts/build.sh` (Release, analyzers on) → 0 Warning(s), 0 Error(s).
- `scripts/format.sh` over both changed `.cs` files → Formatted 0 of 2 file(s).

## Disputed tests or scope
- The frame path keeps colours and marks, more than Decision 10 required (it allowed losing colours). It costs no second renderer: the existing range functions write through three small helpers, which was cheaper than a frame renderer for marks alone, which `NullCharacterIsKeptInACopyWithErrorMarks` requires.
- Not verified in a real browser this round; the evidence is 29's browser-document helper, which models text frames as text nodes with exactly their text (the behaviour on `b63ec36`, which used text frames for both panes).

## Architecture findings
_None._

## Follow-up candidates
_None._

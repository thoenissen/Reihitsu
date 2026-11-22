Role: implementer
Commit: 9bf267048f2d64168c5464ac89d2c971ec775e09
Verdict: tests pass

Repair round 1. The prompt named the worklist and its sources and held no intended fix or hoped-for verdict beyond the audit's suggested changes, which I re-derived from `09-auditor.md` and the tests of `10-test-implementer.md`. On 61a94b7 the seven new tests failed as the test implementer's handover says; on 9bf2670 all 87 pass.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Playground/PlaygroundHighlighter.cs` | `GetMarkSpan`: an empty span is widened only when the character behind it is not a line break as Roslyn counts one (`SyntaxFacts.IsNewLine`, which covers `\r`, `\n`, U+0085, U+2028, U+2029 — the set `GetLineSpan` counts for the message line), and it is widened by both code units of a surrogate pair (`char.IsSurrogatePair`). XML summary updated. `Classify` parses through `PlaygroundFormatter.Parse`; its summary names that entry point | PLAY-014, PLAY-010 (audit 1); FC-015 (audit 3) |
| `src/Reihitsu.Playground/PlaygroundFormatter.cs` | New `internal static SyntaxTree Parse(string source)`, the one owner of "parse with PLAY-005's options"; `Format` uses it | audit 3 |
| `src/Reihitsu.Playground/Pages/Home.razor` | Output label gets `id="playground-output-label"` instead of `for`; the output `pre` gets `aria-labelledby="playground-output-label"` and `tabindex="0"` | PLAY-002, PLAY-012 (audit 4) |
| `src/Reihitsu.Playground/wwwroot/css/app.css` | The `@media (max-width: 700px)` block moved behind the base `.playground-editor, .playground-output` rule, so its `40vh` wins on a narrow screen; `.playground-output:focus-visible` joins the editor's `:focus-within` outline rule, so the new focus stop is visible. CRLF line endings kept | Decision 5; audit 4 |
| `src/Reihitsu.Playground/Pages/Home.razor.cs` | Second byte-order mark removed (file now starts with one, as on `main`) | audit 5 |
| `src/Reihitsu.Playground/PlaygroundFormatResult.cs` | `Errors` doc: "`null` when the input has none", which is what every non-syntax-error result carries and what `Home.FormatInput`'s `?? []` handles. I chose the doc over a default of `[]` because a record-struct parameter default must be a constant | audit 6 (FC-031) |

## Policy owners
- Parse with PLAY-005's options: `rg -n "ParseText|ParseOptions" src/Reihitsu.Playground --glob '*.cs'` → `PlaygroundFormatter.cs:56` (`Parse`) only.
- What a line break is, for marks: `rg -n "IsNewLine|'\\\\n'|IsSurrogatePair" src/Reihitsu.Playground --glob '*.cs'` → `PlaygroundHighlighter.cs:102` (`SyntaxFacts.IsNewLine`, Roslyn's own predicate) and `:104`; no private `\r`/`\n` test remains.
- One-based line and column: unchanged, `Home.FormatPosition` only.

## Production diff size
`git diff --shortstat origin/main...HEAD -- src`: 6 files changed, 610 insertions(+), 36 deletions(-). Delta of this round: 6 files changed, 37 insertions(+), 23 deletions(-) (12 of the lines are the moved media block). No new type; one new internal member, `PlaygroundFormatter.Parse`.

## Deleted special cases
_N/A._

## Tests run
- `scripts/test.sh --configuration Debug --project playground` → Passed! Failed: 0, Passed: 87, Total: 87.
- `scripts/test.sh --project playground` (Release) → Passed! Failed: 0, Passed: 87, Total: 87; no build warning.
- `scripts/test.sh --project architecture` → Passed! Failed: 0, Passed: 198, Total: 198.
- `scripts/format.sh` over the four changed `.cs` files → formatted 0 of 4.

## Disputed tests or scope
- The `:focus-visible` outline on the output is not demanded by a test; it is two selector lines that keep the focus stop audit 4 asked for visible, matching the editor's existing focus outline. The cleaner or lead may drop it.

## Architecture findings
_None._

## Follow-up candidates
_None._

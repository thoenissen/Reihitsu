Role: test implementer
Commit: 9cb5298
Verdict: tests written

Repair round 2, maintainer answer 8 (`00-lead.md`), the test candidate of `23-implementer.md`. The prompt held no intended fix, suspected cause, or hoped-for verdict. The tests are deterministic (no timing), touch only `tests/`, and assert one thing: after a series of edits the coloured copy is the copy a freshly rendered page shows for the final text. They name no mechanism; any update strategy that gives the fresh result passes.

On 9cb5298 (production code of 7791652, unchanged): all 97 Playground tests pass (9 new), `scripts/test.sh --project architecture` passes (198).

## Tests

All in `tests/Reihitsu.Playground.Test/IncrementalHighlightingTests.cs`. **The reference ("fresh page")** is `PlaygroundHighlighter.Highlight(text, [])` with no earlier highlighting, rendered by bUnit: the path the page takes for its first render (`Home.OnInitialized` → `FormatInput` with `_inputHighlighting == null`), whose colours the existing `HighlightingTests` pin against the PLAY-010 examples. Page-level tests compare the copy's text, its per-character categories (`PlaygroundMarkup.GetCharacterCategories`), and its whole inner markup with the reference; highlighter-level tests compare the rendered markup.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `TypingCharacterByCharacterMatchesAFreshPageAfterEveryKey` (doc comment, constraint, interpolation, delimited comment, `#if`/`#endif`; checked after every prefix) | PLAY-010, PLAY-011 | bUnit page, `Input` | passes |
| `DeletingCharacterByCharacterMatchesAFreshPageAfterEveryKey` (from the end to half, then from the start) | PLAY-010, PLAY-011 | bUnit page | passes |
| `InsertedElseRecoloursTheUnchangedTextBehindIt` — failure shape 1 of handover 23: an inserted `#else` makes the unchanged doc comment and class behind it active code, removing it makes them disabled text again | PLAY-010, PLAY-011 | bUnit page | passes |
| `TokenThatShrinksNextToTheEditRecoloursTheLinesItNoLongerCovers` — failure shape 2: a three-line delimited comment shortened by `*/` on its first line, a three-line raw string shortened by an inserted closing delimiter, and back | PLAY-010, PLAY-011 | bUnit page | passes |
| `OpeningAndClosingCommentsAndStringsRecolourTheUnchangedTextBehindThem` (`/*` open/close, `"` and `@"` opening unterminated strings, undo) | PLAY-010, PLAY-011 | bUnit page | passes |
| `CopyMatchesAFreshPageAcrossFormat` (format with errors: marks change no colour; first edit after marks; successful format of edited text; edit after it) | PLAY-010, PLAY-011 | bUnit page, `PlaygroundMarkup.Format` | passes |
| `InsertingAndRemovingAtEveryPositionOfADirectiveSourceMatchesAFreshHighlighting` (18 insertions × every position, each forward and back on the previous highlighting) | PLAY-010, PLAY-011 | `PlaygroundHighlighter.Highlight` with `previous` | passes |
| `InsertingAndRemovingAtEveryPositionOfALiteralSourceMatchesAFreshHighlighting` (verbatim and raw multi-line strings, CR LF, `async`/`await`) | PLAY-010, PLAY-011 | same | passes |
| `DeletingAndRestoringAtEveryPositionMatchesAFreshHighlighting` (1 and 2 characters, both sources) | PLAY-010, PLAY-011 | same | passes |

The insertions: `/*`, `*/`, `"`, `"""`, `'`, `@`, `$`, `{`, `}`, `x`, space, `///`, LF, CR LF, `#if A`, `#else`, `#endif`, `#region R` (directives with LF). The sweeps are exhaustive over positions, not random; the class runs in about 5 s.

### Watched fail: mutations of the incremental path

The production code already passes, so each test was checked against scratch mutations of `PlaygroundHighlighter.cs` (copied to `/tmp/claude-0/-home-user-Reihitsu/7ccecb21-b72f-5c00-b88a-569dde938365/scratchpad/mut25/orig.cs`, restored after each run, nothing committed; `git status` clean under `src/`):

| Mutation | Failing tests |
|---|---|
| M1: a child counts as unchanged when no span of `SyntaxTree.GetChangedSpans` touches it (the text-only design handover 23 names as its first defect) | 8 of 9, among them `InsertedElseRecolours…` (copy keeps `class C` behind the new `#else` disabled) and `TokenThatShrinks…` (`two\nthree */` stays a comment after `/* one*/`) |
| M2b: a reclassified token does not widen the region's end | 7 of 9, both named shape tests included |
| M2c: trailing lines kept although their line break touches the region | 5 of 9 (the sweeps and the typing/deleting tests) |
| M2a: token reuse without the `token.FullSpan == previousSpan` check | none — no input found that tells it apart; `IsIncrementallyIdenticalTo` on the same green token already implies the same width, so this is likely an equivalent mutant |

## Not covered, and why

- **Element identity (that unchanged lines keep their DOM).** That is the performance mechanism, not a requirement; asserting it would pin the implementer's design. The tests compare output only.
- **Marked copies compared by markup.** With marks the page renders the text as one line from incrementally computed categories; `CopyMatchesAFreshPageAcrossFormat` compares text and per-character categories there (marks add elements, not colours), and the existing `ErrorMarkTests` pin the marks themselves.
- **U+2028 and NUL insertions** (in handover 23's throwaway harness). Both go through the same `Highlight` path as the other insertions; left out to keep the sweep small. Can be added to `_insertions` in one line if the lead wants them.
- **The output pane.** It is highlighted without an earlier version (`Home.FormatInput`: `Highlight(result.Text, [])`), so no incremental path reaches it.
- **Timing.** Excluded by the lead; the validator's browser measurement stays the bar of Decision 9.

## Commands

```shell
scripts/test.sh --project playground --filter "FullyQualifiedName~IncrementalHighlightingTests"
scripts/test.sh --project playground
```

## Follow-up candidates

_None._

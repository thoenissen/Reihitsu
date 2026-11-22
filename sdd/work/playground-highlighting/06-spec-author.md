Role: spec author
Commit: 5c5d655
Verdict: spec written

The prompt held no intended fix, suspected cause, or hoped-for verdict; it named the change (code owners), which is Decision 3 of `00-lead.md`.

`PlaygroundHighlighter` (`src/Reihitsu.Playground/PlaygroundHighlighter.cs`, `public static class`) owns the PLAY-010 classification and the PLAY-014 mark spans. It is added to the spec's `Code owners` header in `sdd/specs/playground/playground.md` and to the Playground row of the code-owner table in `sdd/specs/README.md`, between `PlaygroundFormatResult` and `PlaygroundVersion`; both lists stay identical.

Other names checked: the spec and `sdd/changes/2026-11/playground-highlighting.md` name no other implementation type or member. The only other backticked type names are `ReihitsuFormatter.FormatSyntaxTree` and `Reihitsu.Formatter` (formatter API, unchanged) and Roslyn's `Classifier` (a rejected alternative). `HasSyntaxErrors`, replaced by `PlaygroundFormatter.GetSyntaxErrors`, appears nowhere under `sdd/specs` or `sdd/changes`. Nothing else needed the update.

`scripts/test.sh --configuration Debug --project architecture` → `Passed! - Failed: 0, Passed: 198, Skipped: 0, Total: 198`.

## Changed requirements
_None._ Spec header metadata only (code owners).

## Examples for the tests
_None._

## Output questions
_None._

## Internal choices
- Inserted `PlaygroundHighlighter` after `PlaygroundFormatResult`, keeping the existing order of the list rather than re-sorting it.

## Follow-up candidates
_None._

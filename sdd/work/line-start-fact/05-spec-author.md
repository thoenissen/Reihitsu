Role: spec author
Commit: 16f6f92
Verdict: spec written

Repair round for finding 1 of `04-test-implementer.md`. It also checks the maintainer's answer in `squad/decisions/2026-10-30-line-start-fact-end-of-file-region.md`. The change doc is not touched.

## Changed requirements

- **SHARED-BLANK-008** — A token starts its line after the same line breaks as trivia — changed. Two edits:
  - **End-of-file example.** Its expected output was wrong. I re-derived it by running `scripts/format.sh --force` over the spec's input: `}`, then a blank line, then `// end` in column 0. The full pipeline inserts that blank line, as `TrailingCommentGainsBlankLineAtEndOfFileTests` (unchanged on `main`) shows. The example now shows the blank line and attributes it to FMT-BLANK.
  - **`#endregion` without a final line break.** The text already covered this case: trivia in front of the end of the file starts its line as SHARED-BLANK-002 decides, and SHARED-BLANK-002 does not depend on a line break at the end of the file. To make the qualifier explicit (FC-002), the sentence now ends "whether or not the file ends with a line break". The maintainer decided the run fixes this case (option A), which matches that reading. `EndRegionBehindLastClosingBraceOfFileTakesColumnZero` covers the case without a final line break and `EndRegionBehindLastClosingBraceOfFileEndingWithLineBreakTakesColumnZero` covers the case with one. I added no spec example for it.

## Examples for the tests

- `ZeroWidthTokenLineStartTests.CommentBehindLastClosingBraceOfFileTakesColumnZero`: its expected output is the corrected SHARED-BLANK-008 example.

## Open questions for the maintainer

_None._

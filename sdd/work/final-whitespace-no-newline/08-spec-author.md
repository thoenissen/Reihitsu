Role: spec author
Commit: 066a8f4259fed490f00ae678115fcb4e57d002a1
Verdict: spec written

Repair round for auditor finding 1 (`07-auditor.md`, audited SHA 2a0087b). Spec wording only.

## Changed requirements

- FMT-CLEAN-005 — The file does not end with a line break or whitespace — clarified: the first sentence no longer says "every line break, whitespace-only line, and whitespace trivia after the last token ... so the file ends with the last token's text". It now says the formatter removes the line breaks and whitespace-only lines that end a file and the whitespace trivia that ends the file, so the file ends with the text of its last token or of the comment or directive that follows it (FMT-CLEAN-006, FMT-CLEAN-007), and adds that whitespace in front of a trailing comment, such as the space in `} /* c */`, does not end the file and stays. This also removes the old over-reach of "every line break" against FMT-CLEAN-006/007. The rest of the requirement and its examples are unchanged. No output changes; no test changes needed.

## Examples for the tests

Unchanged (FMT-CLEAN-005 examples as in `02-spec-author.md`).

## Output questions

_None._

## Internal choices

1. Also folded "or directive" into the sentence, so it agrees with FMT-CLEAN-007 (a final directive keeps its line break), instead of only the comment wording the auditor suggested.

## Follow-up candidates

_None._

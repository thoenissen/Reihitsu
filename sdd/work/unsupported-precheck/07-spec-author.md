Role: spec author
Commit: df103926
Verdict: spec written

## Changed requirements
- FMT-STRUCT-022 — Split fields keep their comments — clarified (example output only: `Offset(/* horizontal */ 1);` → `Offset(/* horizontal */1);`)
- FMT-STRUCT-030 — A documented field is not split — clarified (second example's output only: `Offset(/* c */ 1);` → `Offset(/* c */1);`)
- RH7101-004 — The fix splits the declaration — clarified (example output only: `Offset(/* horizontal */ 1);` → `Offset(/* horizontal */1);`)
- RH7101-007 — The fix is withheld for a comment inside the declaration — clarified (third example's output only: `Offset(/* c */ 1);` → `Offset(/* c */1);`)

Lead Decision 8 carried out. Verified against unchanged FMT-SPACE-013: the comment is in the trailing trivia of `(`, so the `(`/`1` rule (no space) applies to the whitespace between the comment and `1`; FMT-SPACE-013's own example gives `M( /* four */a)` for the same shape. The corrected outputs match the tests in `src/Reihitsu.Formatter.Test/Regression/Structural/FieldDeclarationSplitTests.cs` (lines 602, 692) and `src/Reihitsu.Analyzer.Test/Formatting/RH7101DoNotCombineFieldsAnalyzerTests.cs` (lines 2527, 2562). Inputs, normative text, and every other example are unchanged.

## Examples for the tests
FMT-STRUCT-022, FMT-STRUCT-030, RH7101-004, RH7101-007 — tests already assert these outputs; nothing new to derive.

## Output questions
_None._ The output is decided by unchanged FMT-SPACE-013 (Decision 8).

## Internal choices
- Corrected the output lines instead of switching the inputs to an unambiguous shape such as `1 + /* c */ 2`; alternative rejected because it would change the inputs the tests already mirror.

## Follow-up candidates
_None._

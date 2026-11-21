Role: implementer
Commit: see the commit that adds this file ("Implement tab-indented raw string alignment …")
Verdict: test disputed

Issue: thoenissen/Reihitsu#1432 (`agent-filed`), behavior change. The prompt carried no intended fix, suspected cause, or hoped-for verdict. Of the handed-over tests, all but one pass. That one cannot pass under RH5202-008 as specified; see *Disputed tests or scope*.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Core/RawStringLiteralUtilities.cs` | `GetLineShiftEdit` is replaced by `GetShiftedIndentation(indentation, shift, spacesWidth)`. This is the one per-line rule: shift right or left as before, then write the first `spacesWidth` characters (at most all of them) as one space each. New `GetStrippedIndentationSpacesWidth(closingLineText, closingWhitespaceLengthAfterShift)` gives the width after the shift when the closing whitespace holds a tab, and 0 otherwise. New `HasTabInLeadingWhitespace(lineText)` is the trigger test. | FMT-RAW-010, RH5202-007, RH5202-008; decision 2 (one owner) |
| `src/Reihitsu.Formatter/Pipeline/Indentation/Utilities/RawStringLineShifts.cs` | Each edit carries `SpacesWidth`. New `GetSpacesWidth` finds the literal whose own text the line starts in, from the token holding the line start. For a raw text token, an interpolated text token, or an end token, it computes the width once per literal from that literal's closing line and `getColumnAfterMove`. Hole lines (trivia) and hole-opening lines get 0. `CollectEdits` keeps a line that does not move when its width is > 0, so an aligned literal is rewritten too (FMT-RAW-006). `ReplaceIndentation` uses the shared `GetShiftedIndentation`. Updated the summaries of the class, `MoveLines`, `CollectEdits`, and `ReplaceIndentation`. | FMT-RAW-010, FMT-RAW-006 |
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Layout/RH5202RawStringLiteralsShouldBeFormattedCorrectlyCodeFixProvider.cs` | `ApplyCodeFixAsync` computes the width from the closing line and the shift. It writes each line's indentation via `GetShiftedIndentation`, with width 0 on lines that start in one of the literal's own holes (`StartsInOwnHole`). New `HasTabIndentedNestedRawString` withholds the fix for an interpolated literal when any multi-line raw string nested at any depth has a tab in front of its closing quotes. Updated summaries. | RH5202-007, RH5202-008 |

The analyzer is unchanged (RH5202-007: it reports the same literals).

## Policy owners
- `rg -n "GetShiftedIndentation|GetStrippedIndentationSpacesWidth|HasTabInLeadingWhitespace" src --glob '!*Test*'`: these are defined only in `RawStringLiteralUtilities`. They are consumed by `RawStringLineShifts` (formatter) and the RH5202 fix, and nothing else.
- `rg -n "IndexOf\('\\t'|Contains\('\\t'\)|== '\\t'" src --glob '!*Test*'`: no other test for a tab in raw-string closing whitespace. The other hits are token spacing, indentation measurement, and the tab-replacement phase, which are unrelated policies.
- The shift rule had two consumers through `GetLineShiftEdit` before this change. It still has the same two, through `GetShiftedIndentation`.

## Production diff size
`git diff --shortstat origin/main -- src ':(exclude,glob)src/*Test*/**'`: 3 files changed, 170 insertions(+), 42 deletions(-). No new type. One public Core member was removed (`GetLineShiftEdit`; no other caller and no test) and three were added in the same class.

## Deleted special cases
_N/A._ No shape is declared unsupported.

## Tests run
- `scripts/test.sh --configuration Debug --project formatter --filter "FullyQualifiedName~RawStringTabIndentationTests|FullyQualifiedName~RawStringClosingWhitespaceMeasureTests|FullyQualifiedName~ReihitsuFormatterNodeColumnTests"`: Passed 93/93.
- The focused analyzer filter from `04-test-implementer.md`: Failed 1, Passed 93 (the disputed test below).
- `--project formatter`: Passed 6854/6854.
- `--project analyzer`: Failed 1 (the disputed test), Passed 11898.
- `--project core`: Passed 812/812. `--project architecture`: Passed 198/198. `--project cli`: Passed 425/425.

## Disputed tests or scope
- **`RH5202RawStringLiteralsShouldBeFormattedCorrectlyHoleTests.VerifyEveryFixApplicationConvergesWhenRawStringNestedInHoleIsIndentedWithTabs`.** It fails with `Context: Fix all in document — Expected '1' iterations but found '2' iterations`. The test calls `Verify(source, fixed, diagnostics)` with the framework's default Fix All iteration count of 1. RH5202-008 withholds the outer fix while the nested literal has a tab in front of its closing quotes. The spec then says "the outer literal's fix is offered on the result (SHARED-FIX-002)", and the second example of RH5202-008 shows that two-step sequence. The batch fixer registers actions on the original document, so the first Fix All pass can only fix the nested literal, and the outer one follows in a second pass. Two iterations is therefore exactly what the spec prescribes. The final output matches `resultData`; only the iteration count differs. The incremental path passes. Proposed test change, for the test implementer: set `NumberOfFixAllIterations = 2` for this test. I see no implementation that keeps RH5202-008 and converges in one pass.
- **Scope:** the change keeps one public Core member's role, under a new signature (`GetLineShiftEdit` becomes `GetShiftedIndentation`), and adds two public helpers in the same class, as decision 2 asked. No new dependency and no new type.

## Architecture findings
_None._ The formatter and the fix share the per-line rewrite and the trigger through `RawStringLiteralUtilities`. Neither predicts the other's output.

## Follow-up candidates
_None._ Shape option: none arises. RH5202-008 is a local withholding for a rare class, as decided (decision 1).

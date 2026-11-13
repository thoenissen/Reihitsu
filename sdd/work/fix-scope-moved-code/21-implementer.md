Role: implementer
Commit: ecb86f4 (base); the production commit is the commit that adds this file
Verdict: tests pass

Repair round 2, lead decisions 34–38 (and 39/40 through the spec of 0669c17). I read `00-lead.md`, `18-auditor.md`, `19-spec-author.md`, `20-test-implementer.md`, and `13-implementer.md`. The 24 failing tests of 20 failed as stated before any change, each with "a code fix is offered". No test file was touched. My prompt listed the decisions and carried no suspected cause beyond them.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Core/SyntaxNodeUtilities.cs` | New `GetStringsAndCommentsSpanningLines(node, span)`. It returns the spans of string literal nodes (regular, verbatim, raw, UTF-8), interpolated string nodes of every `$` form, and delimited comments (`/* */`, `/** */`) that overlap the span and start and end on different lines. This is the one node-based owner (decision 34). | RH3004-008, RH7004-007, SHARED-ORDER-010 |
| `src/Reihitsu.Core/SyntaxIndentationUtilities.cs` | `TryShiftMovedLines` now asks whether a line starts strictly inside one of those spans (or in disabled text) instead of inside a token. So a line of an interpolated string that starts at `{` or at the closing quote is refused for a non-zero shift, and at shift 0 it is copied as-is. `StartsInsideContent` no longer has its token test. New public `HoldsTabInFrontOfMovingLine(root, text, span)` covers lines that start in the span, hold text, and do not keep their column (`KeepsItsColumn`). Summaries updated. | SHARED-ORDER-010, RH7004-007, RH3004-006 |
| `src/Reihitsu.Core/LineEndingUtilities.cs` | New `HoldsOtherLineBreak(text, span, endOfLine)`: a line break that starts in the span and differs from the given ending (mixed endings, lone `\r` against the detected `\n`). | SHARED-FIX-008 |
| `src/Reihitsu.Core/OrderingMoveSafety.cs` | `TryCreateWholeLineMove` refuses when the replaced span (target's full start through moved node's full end) holds a line break other than `DetectEndOfLine(root)`. Summary updated. The one-line swap is unchanged, so a lone-`\r` swap stays offered. | SHARED-FIX-008 |
| `src/Reihitsu.Core/AccessorOrderingUtilities.cs` | `TryCreateMove` refuses the one-line swap when a comment stands between the list's braces outside every accessor's `Span` (in front of the first, between two, behind the last). This is the private `HoldsCommentOutsideEveryAccessor`. A comment inside an accessor still swaps, and so does a comment behind `}`. | SHARED-ORDER-009, RH7107-005, RH7108-005 (decision 36) |
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Organization/RH7004…CodeFixProvider.cs` | Private `GetEndOfLine` deleted. Both paths write `LineEndingUtilities.DetectEndOfLine(root)`. The fix is refused when the declaration line's indentation holds a tab, when a moving line has a tab in front (`HoldsTabInFrontOfMovingLine`), and when the moved statements hold another line break. Summary updated. | RH7004-007 (decision 37), SHARED-FIX-008 (decision 35) |
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Clarity/RH3004UseLambdaSyntaxCodeFixProvider.cs` | Private `WithholdsFixForSpanningStringOrComment`, `IsString`, and `HoldsTabInFrontOfMovingLine` deleted. They are replaced by one `WithholdsFixForLines`, which uses the Core check with the same two arms as before: on the expression-body path a carrier withholds only with a `#` line, and on the block-body path every carrier withholds. The block-body path also refuses a tab in front of a moving line (Core helper, same lines as before) and a line break in the block other than the detected one. Fallback in `MoveStatementsAsync`: see *Disputed*. | RH3004-006, RH3004-008, SHARED-FIX-008 (decisions 34, 35, 38, 39) |

## Policy owners
- Strings and comments spanning lines: `rg -n "IsString\b|StringLiteralExpression\)|InterpolatedStringExpressionSyntax" src/Reihitsu.Core src/Reihitsu.Analyzer.CodeFixes`. The one moved-line owner is `SyntaxNodeUtilities.GetStringsAndCommentsSpanningLines`, used by `TryShiftMovedLines` (so RH7004 and every member and accessor move) and by RH3004. The other hits (`FormattingTextAnalysisUtilities`, `TokenSeparationUtilities`, `StringInterpolationUtilities`) answer other questions: token spacing, literal text, and interpolation markers.
- Line ending written: `rg -n "GetEndOfLine\(|HoldsOtherLineBreak|DetectEndOfLine"`. `LineEndingUtilities.DetectEndOfLine` is the only source in RH7004, RH3004, and `OrderingMoveSafety`, and the RH7004 private copy is gone. `HoldsOtherLineBreak` is the one refusal, used by those three.
- Tab in front of a moving line: `rg -n "IndexOf\('\\t'\)|HoldsTabInFrontOfMovingLine" src`. The owner is `SyntaxIndentationUtilities.HoldsTabInFrontOfMovingLine`, used by RH3004 and RH7004. RH7004 additionally checks its declaration line's indentation, because that line is not moved code.
- One-line accessor swap: `AccessorOrderingUtilities.TryCreateMove` is the only caller of `OrderingMoveSafety.TryCreateSwapWithinLine`.

## Production diff size
- Against `origin/main` (`git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'` after this commit): 17 files changed, 803 insertions(+), 579 deletions(-), net +224. Before this round it was 15 files, 701/545, net +156. `AccessorOrderingUtilities` and `LineEndingUtilities` are newly touched.
- Delta of this round: 7 files, +183 / −115 (net +68).
- No new type. New public members: `SyntaxNodeUtilities.GetStringsAndCommentsSpanningLines`, `SyntaxIndentationUtilities.HoldsTabInFrontOfMovingLine`, and `LineEndingUtilities.HoldsOtherLineBreak` (decision 34 asked for the first one; the other two replace private copies or serve as the shared refusal).

## Deleted special cases
_N/A._ No shape is declared unsupported. Private copies deleted: `RH3004.WithholdsFixForSpanningStringOrComment`, `RH3004.IsString`, `RH3004.HoldsTabInFrontOfMovingLine`, and `RH7004.GetEndOfLine`.

## Tests run
```text
scripts/test.sh --configuration Debug --project analyzer --filter "<20's focused filter>"   before: Failed: 24, Passed: 603 / after: Failed: 0, Passed: 627
scripts/test.sh --configuration Debug --project analyzer        Passed: 8516, Failed: 0
scripts/test.sh --configuration Debug --project core            Passed: 651, Failed: 0
scripts/test.sh --configuration Debug --project architecture    Passed: 117, Failed: 0
scripts/format.sh <the seven changed files>                     Formatted 0 of 7 file(s).
```

## Disputed tests or scope
1. **Decision 38 (RH3004 fallback).** It is unreachable today. Every refusal of `TryShiftMovedLines` is excluded by the registration guards: spanning strings and comments and the `#` arm by `WithholdsFixForLines`, and disabled text by `ContainsDirectiveOrDisabledText`. The deltas from `GetColumnDeltas` put no moving line left of column 0. Fix All re-registers through `RegisterCodeFixesAsync` (`DocumentOrderFixAllProvider`). Deleting the check outright, though, would mean ignoring the `bool`. On a future guard drift, `laterLinesText` would then be `null`, and the moved statements would be silently dropped. So I removed the `return formattedDocument` fallback and return `originalDocument` instead, as the auditor's hint suggested, with a comment that says why. The lead may want the check gone entirely. I would not recommend that, for the reason above.
2. **Scope.** Three new public Core members (listed above). Two of them have more than one consumer.
3. **RH7004 empty-block path.** The decision 37 tab refusal on the declaration line also withholds the empty-block path, where nothing moves. RH7004-007 names "a tab in front of the declaration's line" without restricting it to a path, so I applied it to both. No test pins the empty-block arm.

## Architecture findings
_None._ Decision 34's structural repair is done: the two owners (line-based `StartsInsideContent`, and node-based `RH3004.WithholdsFixForSpanningStringOrComment`/`IsString`) are now one, and the private line-ending choice is gone.

## Follow-up candidates
_None._

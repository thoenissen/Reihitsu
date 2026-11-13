Role: implementer
Commit: c6649f1 (base); production commit is the commit that adds this file
Verdict: tests pass

Read: `00-lead.md` (decisions 26–28), `10-implementer.md`, `11-spec-author.md`, `12-test-implementer.md`, and the spec diff of bffd78a. The 10 failing tests of 12 reproduced as stated before any change. No test file was touched. My prompt summarised the fix it wanted; I worked from decisions 26/27 and RH3004-006 / SHARED-FIX-007 rather than from that summary. They agree.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Clarity/RH3004UseLambdaSyntaxCodeFixProvider.cs` | New private `HoldsTabInFrontOfMovingLine(block, text)`. It checks every non-blank line after the opening brace's line, except a line that starts with the closing brace, for a tab in the leading spaces/tabs. `RegisterCodeFixesAsync` withholds the fix on the block-body path only (`GetExpressionBody == null`). | RH3004-006 (decision 26) |
| `src/Reihitsu.Core/SyntaxIndentationUtilities.cs` | `TryShiftMovedLines` now runs one per-line loop. A line that starts inside a token, a spanning comment, or disabled text refuses a non-zero shift and is copied as-is at shift 0. A whole whitespace-only line inside the span is written empty, and its line break is kept. The other lines behave as before. The shift-0 early return is gone, so blank lines are emptied at shift 0 too (as SHARED-FIX-007 says for all moved code). `HoldsLineThatCannotMove` was replaced by the per-position `StartsInsideContent`, which has the same conditions. Summary updated. | SHARED-FIX-007 (decision 27) |

## Policy owners
- `rg -n "TryShiftMovedLines|KeepsItsColumn" src --glob '!**/obj/**' --glob '!*Test*/**'`: one owner, `SyntaxIndentationUtilities`. Callers: `OrderingMoveSafety.TryCreateWholeLineMove`, RH7004, RH3004. Blank-line emptying lives only in `TryShiftMovedLines`.
- Tab refusal: only in RH3004 (`HoldsTabInFrontOfMovingLine`). The other movers count a tab as one column (SHARED-FIX-007).

## Production diff size
- `git diff --shortstat origin/main -- src ':(exclude,glob)src/*Test*/**'`: 15 files changed, 701 insertions(+), 545 deletions(-), net +156. After 10 it was net +136.
- Delta of this round: 2 files, +71 / -51 (net +20). No new type or public member.

## Deleted special cases
_N/A._

## Tests run
```text
scripts/test.sh --project analyzer --configuration Debug --filter "<12's focused filter>"   Passed: 16, Failed: 0
scripts/test.sh --project analyzer --configuration Debug       Passed: 8466, Failed: 0
scripts/test.sh --project core --configuration Debug           Passed: 651, Failed: 0
scripts/test.sh --project architecture --configuration Debug   Failed: 1, Passed: 116
scripts/format.sh <the two changed files>                      Formatted 0 of 2 file(s).
```
The one architecture failure exists before this change too: it fails the same way on c6649f1 with my change stashed. `SpecStructureTests.UsesListsExactlyTheReferencedRequirements`: "analyzer/RH3004.md: FMT-PIPE-017 is referenced but missing from Uses". bffd78a added that reference in RH3004-006's Why paragraph. This is for the spec author.

## Disputed tests or scope
1. **Blank lines at shift 0.** Member moves with no shift now also empty a whitespace-only line among the moved lines. SHARED-FIX-007 states this for all moved code, and 11-spec-author measured that main empties them for an RH7109 member move. No test covers the zero-shift case, which 12 lists under "Not covered".
2. The RH3004.md `Uses` row (see *Tests run*).

## Architecture findings
_None._

## Follow-up candidates
_None._

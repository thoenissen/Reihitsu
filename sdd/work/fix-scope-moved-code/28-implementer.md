Role: implementer
Commit: ee489e7 (base); the production commit is the commit that adds this file
Verdict: tests pass

Structural round for lead decisions 43 and 44 (auditor 26 findings #2, #4, #5 and the architecture finding). Issues #1335 and #1604 (both through RH7004). #969, #1263, #1151: nothing to do. My prompt named the decisions and carried no suspected cause beyond them. The 5 tests of 27 failed as stated before the change ("Expected collection to be empty"); no test file was touched.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Organization/RH7004UsingDeclarationsShouldNotBeUsedCodeFixProvider.cs` | `TryCreateChanges` runs `LineEndingUtilities.HoldsOtherLineBreak` over `FromBounds(usingDeclaration.SpanStart, movedSpan.End)` before the paths split, so the rewritten declaration, its trailing line break, and the moved statements are checked on both the statements-follow and the empty-block path. The narrower `movedSpan` check on the statements path is removed (subsumed). Summary updated. | SHARED-FIX-008, RH7004-007 (decision 43) |
| `src/Reihitsu.Core/SyntaxIndentationUtilities.cs` | `TryShiftMovedLines` summary: a blank line is one that holds only whitespace and does not start inside a string or a comment that spans lines or in disabled text (was "outside a token"). Text only; also rewrapped. | FC-031 (decision 44) |

## Policy owners
`rg -n "HoldsOtherLineBreak" src --glob '!*Test*'`: one predicate in `LineEndingUtilities`; callers `OrderingMoveSafety` (its `TextChange` span), RH3004 (`block.Span`), RH7004 (now declaration start through moved end, which covers every `TextChange` it emits: `(` at `Declaration.SpanStart`, the semicolon, and `movedSpan`).

## Production diff size
Against `origin/main`: 17 files changed, +812 / −579 after this commit (803/579 before). Delta of this round: 2 files, +22 / −13. No new type or member.

## Deleted special cases
_N/A._

## Tests run
```text
scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH7004UsingDeclarationsShouldNotBeUsedAnalyzerTests"   before: Failed: 5, Passed: 100
scripts/test.sh --configuration Debug --project analyzer        Passed: 8521, Failed: 0
scripts/test.sh --configuration Debug --project core            Passed: 651, Failed: 0
scripts/test.sh --configuration Debug --project architecture    Passed: 117, Failed: 0
scripts/format.sh <the two changed files>                       Formatted 0 of 2 file(s).
```

## Disputed tests or scope
_None._

## Architecture findings
_None._ Decision 43's class is refused: RH7004's checked span is now derived from the span its changes cover.

## Follow-up candidates
_None._

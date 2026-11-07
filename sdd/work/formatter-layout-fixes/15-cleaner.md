Role: cleaner
Commit: see git log (parent eeba52a)
Verdict: clean

## Formatted
_None._ Comment-only edits.

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| Auditor finding 2 | `RawStringLiteral.cs` `ClosingColumn` (param and property) | Now reads: the width of the whitespace in front of the closing marker, each whitespace character one column |
| Auditor finding 2 | `RawStringLineShifts.cs` `MoveLineInsideTrivia` `edit` | Now reads as `ReplaceIndentation`'s `edit`: characters of indentation it has, and the column after the move |
| Auditor finding 3 | `LineBreakListRewriter.cs` `EnsureSeparatorsHaveEndOfLine` `isWrappedAfterOpener` | Now: whether the list was wrapped behind its opening delimiter as written |

## Unresolved
_None._

## Checks
Text-only proof, `scripts/verify-text-only.sh --strict-docs --base eeba52a --head worktree`:
`TEXT-ONLY PROOF: FAIL — base=eeba52a head=worktree — 1 of 7 changed file(s) carry compiled behavior; xml-doc impact: 3 file(s), including public API documentation`
The one flagged file is `RawStringLiteral.cs`: "public API documentation changed and --strict-docs was requested". Tokens are identical; only the `public` members of the internal record struct have changed docs, which the strict flag counts. The type is internal (no public API), the other two files report `text-only ... changed trivia: documentation`. The lead decides whether this is acceptable.

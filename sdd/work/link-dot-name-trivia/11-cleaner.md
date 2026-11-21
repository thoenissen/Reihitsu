Role: cleaner
Commit: see git log (comment-repair commit)
Verdict: clean

## Formatted
_None._

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| Audit finding 1 (FC-031, stale comment) | `src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/ChainLineBreakRewriter.cs`, class summary `<para>` | Only the gap in front of a link operator is never closed; a gap between two tokens of a link operator or between a link's dot and its name never holds such trivia (FMT-UNSUP-015, FMT-UNSUP-016) |

## Unresolved
_None._

## Checks
TEXT-ONLY PROOF: PASS — base=d94d4f58 head=worktree — 3 changed file(s) (2 non-compiled, 1 C# comment/layout-only); xml-doc impact: 1 file(s)

## Follow-up candidates
_None._

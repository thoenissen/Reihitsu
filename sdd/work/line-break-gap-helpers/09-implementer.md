Role: implementer
Commit: see the commit that adds this file ("Implementer: repair round 1, route separator line break through EndLineInFrontOf [skip ci]")
Verdict: tests pass

The prompt carried no intended fix beyond the audit's suggestion, which I re-derived below rather than copied.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/LineBreaks/Utilities/LineBreakTriviaUtilities.cs` (`CollapseSeparatorsToPreviousElementLine`) | The private "end the line in front of the next token unless a line break already separates them" check and write (`HasTrailingEndOfLine(newSeparator) \|\| HasLeadingEndOfLine(nextToken)`, then `AppendEndOfLine(RemoveTrailingWhitespace(...))`) is replaced by `EndLineInFrontOf(newSeparator, nextToken, endOfLine)`. The `nextToken == default` branch is folded: a default token has an empty leading trivia list, so the helper then appends unless the comma already ends its line, as before. Summary rewritten to describe the new code (FC-031): the comma keeps its trailing trivia, whitespace at its end included, and the cleanup phase removes that whitespace | Audit finding #1 (FC-015, FC-016); #1651 decision 3 and 4; FMT-BREAK-011, FMT-BREAK-012, FMT-BREAK-038; FMT-BREAK-029 ¶2 (a rewrite of the trailing trivia may remove the whitespace, so keeping it is allowed); FMT-CLEAN-001 |

Re-derivation of the fact's parity:
- `HasLeadingEndOfLine(nextToken)` is `IsFirstOnLine(nextToken)` = `AreSeparatedByEndOfLine(nextToken.GetPreviousToken(), nextToken)` (its `None`-previous branch is unreachable: the comma is always in front). `nextToken` is read from the current `node`, so its previous token is the original comma, whose trailing trivia `newSeparator` keeps unchanged (only the leading trivia is rewritten).
- `HasTrailingEndOfLine(newSeparator)` is the first disjunct of `AreSeparatedByEndOfLine(newSeparator, nextToken)`.
- So the old guard equals `AreSeparatedByEndOfLine(newSeparator, nextToken)`, the helper's guard. The only difference is the write: the whitespace at the end of the comma's trailing trivia now stays in the line-break phase's output; the cleanup phase removes it in every entry point.
- `nextToken == default` is not reached by any caller: every list passed (argument, attribute argument, parameter, bracketed parameter, initializer, anonymous-object initializers, collection expression, list pattern, property pattern subpatterns) ends with a closing token inside the node, so `separator.GetNextToken()` is never default. The fold stays correct for it anyway.

## Policy owners
`rg -n "AppendEndOfLine\(" src/Reihitsu.Formatter`: `LineBreakTriviaUtilities.EndLineInFrontOf` (the owner; callers binary, ternary, chain, list, constraint, and now `CollapseSeparatorsToPreviousElementLine`) and `ChainLineBreakRewriter`'s comment-tail branch (leading side, a different decision kept by decision 3). One owner of the trailing-side append.

## Production diff size
`git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'` before this commit: 4 files changed, 73 insertions(+), 89 deletions(-). Delta of this round: 1 file changed, 6 insertions(+), 9 deletions(-). No new type or member.

## Deleted special cases
_N/A._

## Tests run
- `scripts/test.sh --configuration Debug --project formatter`: Passed! Failed: 0, Passed: 5781, Skipped: 0, Total: 5781.
- `scripts/test.sh --configuration Debug --project analyzer`: Passed! Failed: 0, Passed: 10639, Skipped: 0, Total: 10639.

No existing test's expectation would have to change; no assertion was touched.

## Disputed tests or scope
_None._

## Architecture findings
_None._

## Follow-up candidates
_None._

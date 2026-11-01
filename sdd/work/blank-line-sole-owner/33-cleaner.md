Role: cleaner
Commit: see the git log entry for this file
Verdict: clean

Intended fix, suspected cause, or hoped-for verdict in the prompt: none used.

## Formatted
_None._ (`scripts/format.sh`: Formatted 0 of 2 file(s).)

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| Finding 10 (parameter docs) | `BraceBlankLinePolicy.FindTokenLineStartIndex`, `FindGluedCommentIndex`, `ChainLineBreakRewriter.BreaksLineBehindMultiLineCommentTail` | The `token` param docs no longer say "still part of its tree". They state that a detached token has no parent and is classified without the chain-link case, which is exact for every token that is not a chain link operator. |

## Unresolved
_None._

## Checks
- Comment text only; no build or test run needed.
- `--strict-docs`: `TEXT-ONLY PROOF: FAIL — base=ddcdbf4 head=worktree — 1 of 3 changed file(s) carry compiled behavior; xml-doc impact: 2 file(s), including public API documentation`. The only cause is "public API documentation changed": `FindTokenLineStartIndex` and `FindGluedCommentIndex` are `public static` members, but their class is `internal static`, so no public API changes.
- Without `--strict-docs`: `TEXT-ONLY PROOF: PASS — base=ddcdbf4 head=worktree — 3 changed file(s) (1 non-compiled, 2 C# comment/layout-only); xml-doc impact: 2 file(s), including public API documentation`. `ChainLineBreakRewriter.cs` passes under strict mode too (tokens identical).

Role: cleaner
Commit: cef53e4 (plus this commit)
Verdict: clean

## Formatted
_None._ (`scripts/format.sh --force` over the 109 changed C# files: Formatted 0 of 109.)

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| Audit finding 3 (not a compiler warning) | `BinaryOperatorLineBreakRewriter.NormalizeBinaryOperatorPosition` summary | The `unless ...` clause is back in the first sentence; the type-test `is` exception is its own sentence |

## Unresolved
_None._

## Checks
```text
scripts/verify-text-only.sh --base cef53e4 --head worktree   TEXT-ONLY PROOF: PASS (1 file, documentation trivia only)
scripts/build.sh                                             0 Warning(s), 0 Error(s)
```
Focused tests not run: the change is proven text-only.

## Follow-up candidates
_None._

Role: cleaner
Commit: see git log (subject ends `[skip ci]`)
Verdict: clean

## Formatted
_None._ (`Formatted 0 of 1 file(s).`)

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| F4 (comment) | `TokenGapNormalizer.EnsureTokenStartsLine` summary, first sentence | Rewritten to "Puts a token on a line of its own when other content on its line stands in front of it (SHARED-BLANK-008)." |

## Unresolved
_None._

## Checks
- scripts/build.sh: 0 Warning(s) 0 Error(s)
- scripts/verify-text-only.sh --base d3be7a4 --head worktree: see report (strict-docs flags the public doc change by design)

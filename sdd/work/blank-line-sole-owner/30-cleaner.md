Role: cleaner
Commit: see git log (subject ends in [skip ci])
Verdict: clean

## Formatted
_None._ `scripts/format.sh` on the 9 C# files changed since b20875e: "Formatted 0 of 9 file(s)." (S1125 warnings printed from unchanged `src/Reihitsu.Cli` files only.)

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| _None._ | | |

## Unresolved
_None._

## Comment check
Checked `ChainLineBreakRewriter`, `TokenGapNormalizer`, `TriviaListUtilities`, `BraceBlankLinePolicy` and the blank-line rewriters against the new behavior (moved helpers, `CloseGap`, `GetWrappedLayoutLineBreaks`, context parameter). No stale comment or dangling cref found; no correction made.

## Checks
scripts/build.sh: Build succeeded. 0 Warning(s), 0 Error(s).
Focused formatter tests (BlankLine|Chain|LineBreak): Passed! Failed: 0, Passed: 1482, Skipped: 0, Total: 1482.
Text-only proof: not applicable (no comment fix).

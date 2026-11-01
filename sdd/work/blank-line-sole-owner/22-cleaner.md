Role: cleaner
Commit: see git log (subject ends in [skip ci])
Verdict: clean

## Formatted
_None._ `scripts/format.sh` on the 20 C# files changed since a8048bd: "Formatted 0 of 20 file(s)."

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| _None._ | | |

Note: format.sh printed S1125 warnings from `src/Reihitsu.Cli` files the run did not change; the Release build reports none.

## Unresolved
_None._

## Comment check
`RH6001KeywordsMustBeSpacedCorrectlyAnalyzerTests` (~line 906) says its input "is the formatter's output". Still true: formatting that input with `scripts/format.sh` leaves it unchanged, including the blank line before `/* c */(c)`. No correction made.

## Checks
scripts/build.sh: Build succeeded. 0 Warning(s), 0 Error(s).
scripts/test.sh --project formatter --filter "FullyQualifiedName~BlankLine": Passed! Failed: 0, Passed: 596, Skipped: 0, Total: 596.
Text-only proof: not applicable (no comment fix).

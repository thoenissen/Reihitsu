Role: cleaner
Commit: see git log (subject ends in [skip ci])
Verdict: clean

## Formatted
_None._ `scripts/format.sh` on `src/Reihitsu.Formatter.Test/Regression/BlankLines/DirectiveBeforeEndRegionBlankLineTests.cs` (the only C# file changed since a98b099): "Formatted 0 of 1 file(s)."

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| _None._ | | |

Note: the format.sh run printed S1125 warnings from `src/Reihitsu.Cli` files the run did not change; the Release build reports none.

## Unresolved
_None._

## Checks
scripts/build.sh: Build succeeded. 0 Warning(s), 0 Error(s).
scripts/test.sh --project formatter --filter "FullyQualifiedName~DirectiveBeforeEndRegionBlankLineTests": Passed! Failed: 0, Passed: 13, Skipped: 0, Total: 13.
Text-only proof: not applicable (no comment fix this round).

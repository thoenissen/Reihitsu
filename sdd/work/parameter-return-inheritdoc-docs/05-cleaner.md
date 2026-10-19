Role: cleaner
Commit: 59446c6
Verdict: clean

## Formatted
_None._

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| _None._ | | |

## Unresolved
_None._

## Checks
- `scripts/format.sh` over the 14 changed C# files: `Formatted 0 of 14 file(s).` (the formatter's own build printed S1125 warnings in `src/Reihitsu.Cli/FormatCommandHandler.cs`, a file this run did not change).
- `scripts/build.sh --no-install`: `Build succeeded. 0 Warning(s) 0 Error(s)`.
- `scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Documentation"`: `Passed! - Failed: 0, Passed: 747, Skipped: 0, Total: 747`.
- Text-only proof: not applicable (no comment fixes).

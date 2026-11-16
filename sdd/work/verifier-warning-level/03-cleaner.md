Role: cleaner
Commit: ae71eb4
Verdict: clean

## Formatted
_None._ (`scripts/format.sh`: Formatted 0 of 7 file(s).)

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| _None._ | | |

## Unresolved
_None._

## Checks
- `scripts/build.sh`: Build succeeded. 0 Warning(s) 0 Error(s)
- `scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Verifiers.CodeFixWarningWave"`: Passed! Failed: 0, Passed: 15, Total: 15
- Text-only proof: not applicable.

## Follow-up candidates
The format script's own build printed S1125 warnings in `src/Reihitsu.Cli/FormatCommandHandler.cs` (a file this run did not change); the Release build is clean.

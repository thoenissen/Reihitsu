Role: cleaner
Commit: 1a3d3c7
Verdict: clean

## Formatted
_None._ `scripts/format.sh` reported `Formatted 0 of 13 file(s).`

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| _None._ | | |

## Unresolved
_None._ (The format script's own build printed S1125 warnings in `src/Reihitsu.Cli/FormatCommandHandler.cs`, a file this run did not change; the Release solution build is clean.)

## Checks
- `scripts/build.sh`: `0 Warning(s)`, `0 Error(s)`.
- `scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Naming"`: `Passed! - Failed: 0, Passed: 741`.
- Text-only proof: not applicable (no comment-only fix).

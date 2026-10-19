Role: cleaner
Commit: f347ebf
Verdict: clean

## Formatted
_None._ `scripts/format.sh` on the two changed test files: "Formatted 0 of 2 file(s)." The spec file is not C#.

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| _None._ | | |

## Unresolved
_None._ (The formatter's own build printed S1125 warnings in `src/Reihitsu.Cli/FormatCommandHandler.cs`; that file is not changed by this run, and the Release build reports none.)

## Checks
- `scripts/build.sh`: Build succeeded. 0 Warning(s), 0 Error(s)
- `scripts/test.sh --project analyzer` (RH7103 and RH4106 test classes): Passed! - Failed: 0, Passed: 59, Skipped: 0, Total: 59

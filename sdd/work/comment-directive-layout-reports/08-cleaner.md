Role: cleaner
Commit: 52c9759
Verdict: clean

## Formatted
_None._ `scripts/format.sh --force` over the 80 changed C# files: "Formatted 0 of 80 file(s)."

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| _None._ | | |

## Unresolved
_None._ (The format script's own build printed S1125 warnings in `src/Reihitsu.Cli/FormatCommandHandler.cs`, a file this run did not change; the Release build reports none.)

## Checks
- `scripts/build.sh` -> 0 Warning(s), 0 Error(s).
- `scripts/test.sh --project analyzer --filter RH5110|RH520|RH530|RH7304|RH5109|RH503|RH504|RH55` -> Passed: 1794, Failed: 0.

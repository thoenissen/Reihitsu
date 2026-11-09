Role: cleaner
Commit: see the commit that adds this file
Verdict: clean

## Formatted
_None._ (`scripts/format.sh` over the 7 changed C# files: 0 of 7 formatted.)

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| _None._ | | |

## Unresolved
_None._ (The formatter run printed S1125 at `src/Reihitsu.Cli/FormatCommandHandler.cs:487`, a file this run did not change, from the tool's own build; the Release build of the solution reports none.)

## Checks
- `scripts/build.sh`: 0 Warning(s), 0 Error(s)
- `scripts/test.sh --project analyzer` (RH2001, RH7102, RH7103, RH7109, RH7309): Passed: 688, Failed: 0

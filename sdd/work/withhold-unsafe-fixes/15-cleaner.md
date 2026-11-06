Role: cleaner
Commit: 2941e2d
Verdict: clean

## Formatted
_None._ `scripts/format.sh --force` over the 15 C# files changed since 551f629: "Formatted 0 of 15 file(s)".

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| _None._ | | |

## Unresolved
_None._ The Debug build inside `format.sh` prints S1125 warnings in `src/Reihitsu.Cli/FormatCommandHandler.cs`, a file the run did not change; the Release build has none.

## Checks
- `scripts/build.sh`: Build succeeded, 0 Warning(s), 0 Error(s).
- `scripts/test.sh --project analyzer` (RH7003, RH7004, RH7101, RH7103, RH7105, RH7106, RH7109, RH7303, RH7304, RH7306, RH7309, RH7501): Passed! Failed: 0, Passed: 451, Total: 451.
- `scripts/test.sh --project core`: Passed! Failed: 0, Passed: 607, Total: 607.

## Whole run
All seven issues (#1136, #1134, #1135, #1262, #1332, #1336, #1340): clean

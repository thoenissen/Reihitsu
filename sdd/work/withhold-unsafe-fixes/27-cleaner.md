Role: cleaner
Commit: c55f130
Verdict: clean

## Formatted
_None._ `scripts/format.sh` over the 3 C# files changed since 69687d9: "Formatted 0 of 3 file(s)".

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| _None._ | | |

## Unresolved
_None._ The Debug build inside `format.sh` prints S1125 in `src/Reihitsu.Cli/FormatCommandHandler.cs`, a file the run did not change; the Release build has none.

## Checks
- `scripts/build.sh`: 0 Warning(s), 0 Error(s).
- `scripts/test.sh --project analyzer` (RH7103, RH7109): Passed! Failed: 0, Passed: 106, Total: 106.
- `scripts/test.sh --project core`: Passed! Failed: 0, Passed: 607, Total: 607.

## Whole run
All seven issues (#1136, #1134, #1135, #1262, #1332, #1336, #1340): clean

Role: cleaner
Commit: d7ca159
Verdict: clean

My prompt carried no intended fix or hoped-for verdict. Changed C# files since f3a47b4: the two analyzer test files (the other changes are specs and handovers).

## Formatted
_None._ `scripts/format.sh` on both files: `Formatted 0 of 2 file(s).`

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| _None._ | | |

## Unresolved
_None._ The format run printed `S1125` at `src/Reihitsu.Cli/DiffGenerator.cs(134,31)` while building the CLI. That file is not in this run's diff, and the Release build below reports 0 warnings, so I left it.

## Checks
- `scripts/build.sh`: `Build succeeded.  0 Warning(s)  0 Error(s)`
- `scripts/test.sh --project analyzer` (RH7105 and RH7106 test classes): `Passed!  - Failed: 0, Passed: 45, Skipped: 0, Total: 45`
- Text-only proof: not applicable (no comment fixes).

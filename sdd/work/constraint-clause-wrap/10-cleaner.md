Role: cleaner
Commit: see the commit that adds this file, on top of 2f352f2
Verdict: clean

## Formatted
_None._ `scripts/format.sh` over the 9 changed C# files: `Formatted 0 of 9 file(s).`

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| _None._ | | |

## Unresolved
_None._ The format script's own Debug build printed S1125 warnings in `src/Reihitsu.Cli/FormatCommandHandler.cs` (lines 430, 441, 452), a file this run did not change; the Release build is clean.

## Checks
- `scripts/build.sh`: `Build succeeded. 0 Warning(s) 0 Error(s)`
- formatter (ConstraintClause*, HorizontalSpacingRequirementTests): `Passed! Failed: 0, Passed: 108, Total: 108`
- analyzer (RH5110): `Passed! Failed: 0, Passed: 23, Total: 23`
- text-only proof: not applicable (no comment fixes).

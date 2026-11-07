Role: cleaner
Commit: see the commit that adds this file, on top of 53c3b4c
Verdict: clean

## Formatted
_None._ `scripts/format.sh` over the 11 changed C# files: `Formatted 0 of 11 file(s).`

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| _None._ | | |

## Unresolved
_None._ The format script's Debug build prints an S1125 warning in `src/Reihitsu.Cli/FormatCommandHandler.cs` (a file this run did not change); the Release build is clean.

## Checks
- `scripts/build.sh`: `0 Warning(s) 0 Error(s)`
- formatter (ConstraintClause*, ListElementInterior*): see lead run output below
- text-only proof: not applicable (no comment fixes).

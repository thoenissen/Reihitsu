Role: cleaner
Commit: a44e250
Verdict: clean

## Formatted
_None._ `scripts/format.sh` on the 8 changed C# files: "Formatted 0 of 8 file(s)."

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| _None._ | | |

## Unresolved
_None._ (S1125 warnings printed during format.sh come from building the CLI project in its own configuration, outside this change; the Release build of the solution is clean.)

## Checks
- `scripts/build.sh`: Build succeeded, 0 Warning(s), 0 Error(s).
- Formatter `RegionDirectiveLineAboveTests` + `DisabledRegionLayoutTests`: Passed 25, Failed 0.
- Analyzer `RH5031`: Passed 25, Failed 0.

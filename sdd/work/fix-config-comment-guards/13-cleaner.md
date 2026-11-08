Role: cleaner
Commit: c8519d6
Verdict: clean

## Formatted
_None._

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| _None._ | | |

## Unresolved
_None._ (format.sh's own build of Reihitsu.Cli printed S1125 at `src/Reihitsu.Cli/FormatCommandHandler.cs:506`; the file is not changed by the run and the Release solution build reports 0 warnings.)

## Checks
- `scripts/format.sh` over the 15 changed C# files: Formatted 0 of 15 file(s).
- `scripts/build.sh`: Build succeeded. 0 Warning(s) 0 Error(s).
- analyzer RH7102|RH7103|RH7109|RH7309: Passed 276, Failed 0.
- core: Passed 660, Failed 0.

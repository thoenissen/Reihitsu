Role: cleaner
Commit: see git log (subject ends in [skip ci])
Verdict: clean

## Formatted
_None._ `scripts/format.sh` over the 16 changed C# files: "Formatted 0 of 16 file(s)".

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| _None._ | | |

## Unresolved
_None._ (The format run's build printed S1125 at `src/Reihitsu.Cli/FormatCommandHandler.cs:452`, a file the run did not change; the Release build is clean.)

## Checks
- `scripts/build.sh`: 0 Warning(s), 0 Error(s).
- `scripts/test.sh --project analyzer --filter RH7109|RH7103|RH7309|RH7102|RH2001`: Passed 487, Failed 0.
- `scripts/test.sh --project core`: Passed 660, Failed 0.

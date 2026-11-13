Role: cleaner
Commit: see the commit that adds this file (no code changes)
Verdict: clean

## Formatted
_None._ `scripts/format.sh` over the 11 changed C# files: formatted 0 of 11 (Unicode-escape fixtures untouched).

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| _None._ | | |

## Unresolved
_None._ (The format script's own tool build printed S1125 warnings in `src/Reihitsu.Cli/FormatCommandHandler.cs`, a file this run did not change; they do not appear in `scripts/build.sh`.)

## Checks
- `scripts/build.sh`: 0 Warning(s), 0 Error(s).
- `scripts/test.sh --project analyzer` filtered to RH0001, RH2003, RH2109, RH4001, RH4009, RH8402: Passed 218/218.
- Text-only proof: N/A.

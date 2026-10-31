Role: cleaner
Commit: 30c4de2
Verdict: clean

## Formatted
_None._ (`scripts/format.sh --force` over the 18 C# files changed since 26545e6: "Formatted 0 of 18 file(s).")

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| _None._ | | |

## Unresolved
_None._ The format script's own build printed RH0002/S1125 warnings in unchanged Reihitsu.Cli files; the Release build is clean.

## Checks
- scripts/build.sh: 0 Warning(s), 0 Error(s)
- formatter project tests and analyzer RH3002 tests: results in the report

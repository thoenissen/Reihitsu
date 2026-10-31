Role: cleaner
Commit: eca45f0
Verdict: clean

## Formatted
_None._ (`scripts/format.sh --force` over the 29 changed C# files: "Formatted 0 of 29 file(s).")

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| _None._ | | |

## Unresolved
_None._ During the format script's own build, S1125 warnings appeared in unchanged Reihitsu.Cli files; the Release build is clean.

## Checks
- scripts/build.sh: Build succeeded. 0 Warning(s) 0 Error(s)
- scripts/test.sh --project formatter: Passed! - Failed: 0, Passed: 3332, Total: 3332

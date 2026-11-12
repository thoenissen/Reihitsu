Role: cleaner
Commit: b3cd1cd
Verdict: clean

## Formatted
_None._ (`scripts/format.sh` on the one changed C# file: "Formatted 0 of 1 file(s)".)

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| _None._ | | |

## Unresolved
_None._ (S1125 warnings printed during the formatter's own CLI build in `src/Reihitsu.Cli/FormatCommandHandler.cs` are in a file this run did not change and are not in the Release build of the solution.)

## Checks
- scripts/build.sh: Build succeeded. 0 Warning(s) 0 Error(s)
- RH5029 analyzer tests: Passed! Failed: 0, Passed: 52, Total: 52

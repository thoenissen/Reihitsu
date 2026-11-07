Role: cleaner
Commit: 1ce3899c
Verdict: clean

## Formatted
_None._

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|

## Unresolved
_None._ (The format run's build prints S1125 in `Reihitsu.Cli/FormatCommandHandler.cs`, outside this run's diff; the Release build is clean.)

## Checks
- scripts/format.sh over the 5 changed C# files: Formatted 0 of 5 file(s).
- scripts/build.sh: 0 Warning(s), 0 Error(s).
- core RewriteBindingUtilitiesTests: 28 passed.
- analyzer RH3004/3005/3101/3105/3106/3107: 283 passed.

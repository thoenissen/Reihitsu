Role: cleaner
Commit: see the commit carrying this file
Verdict: clean

## Formatted
_None._ `scripts/format.sh` over the 11 changed C# files: `Formatted 0 of 11 file(s).`

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| _None._ | | |

## Unresolved
_None._ (The format script's own build printed S1125 warnings in `src/Reihitsu.Cli/FormatCommandHandler.cs`, a file this run did not change; the Release build of the solution reports none.)

## Checks
- `scripts/build.sh` -> `0 Warning(s) 0 Error(s)`
- formatter `ReihitsuFormatterNodeTrailingLineEndTests` -> `Passed! - Failed: 0, Passed: 19`
- analyzer `RH8402|RH3003|RH3005` -> `Passed! - Failed: 0, Passed: 153`
- text-only proof: n/a

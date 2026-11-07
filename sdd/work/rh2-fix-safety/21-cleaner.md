Role: cleaner
Commit: afa7c6e
Verdict: clean

## Formatted
_None._ (`scripts/format.sh`: Formatted 0 of 4 file(s).)

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| _None._ | | |

## Unresolved
_None._ (The S1125 warnings printed while format.sh built the CLI are in `FormatCommandHandler.cs`, a file this run did not change; the Release build of the solution reports 0 warnings.)

## Checks
- `scripts/build.sh`: Build succeeded. 0 Warning(s), 0 Error(s)
- analyzer RH2005 + RH2001: Passed! - Failed: 0, Passed: 152
- core DeclarationModifierUtilities: Passed! - Failed: 0, Passed: 23

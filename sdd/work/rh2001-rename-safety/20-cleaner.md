Role: cleaner
Commit: ffeb7a6
Verdict: clean

## Formatted
_None._ (`scripts/format.sh`: Formatted 0 of 2 file(s).)

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| _None._ | | |

## Unresolved
_None._ (format.sh's own build printed S1125 warnings in `src/Reihitsu.Cli/FormatCommandHandler.cs`, a file this run did not change; the Release build reports none.)

## Checks
- `scripts/build.sh`: Build succeeded. 0 Warning(s), 0 Error(s).
- `scripts/test.sh --project analyzer --filter ...RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzerTests`: Passed! Failed: 0, Passed: 241, Total: 241.

Role: cleaner
Commit: 4bb16a7
Verdict: clean

## Formatted
_None._ `scripts/format.sh` reported "Formatted 0 of 2 file(s)".

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|

_None._ (The format script printed a pre-existing S1125 note for `src/Reihitsu.Cli/DiffGenerator.cs`, a file this run did not change; the Release build reports 0 warnings.)

## Unresolved
_None._

## Checks
- `scripts/build.sh`: Build succeeded. 0 Warning(s) 0 Error(s)
- `scripts/test.sh --project analyzer --filter ...RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzerTests`: Passed! Failed: 0, Passed: 231, Total: 231

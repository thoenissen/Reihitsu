Role: cleaner
Commit: d2db866
Verdict: clean

## Formatted
_None._ `scripts/format.sh` over the 13 changed test files: "Formatted 0 of 13 file(s)."

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| _None._ | | |

## Unresolved
_None._ During the format run the CLI's own build printed S1125 warnings in `src/Reihitsu.Cli/FormatCommandHandler.cs`, a file this run did not change and which the Release build does not report; not touched.

## Checks
- `scripts/build.sh`: Build succeeded. 0 Warning(s) 0 Error(s)
- `scripts/test.sh --project analyzer` (the 13 changed classes): Passed! Failed: 0, Passed: 199, Total: 199
- Text-only proof: not applicable (no comment fixes after an audit).

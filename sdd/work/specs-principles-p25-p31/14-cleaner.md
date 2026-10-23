Role: cleaner
Commit: see the commit carrying this file
Verdict: clean

## Formatted
_None._ `scripts/format.sh` reported "Formatted 0 of 7 file(s)" for the six changed C# files (all test files under `src/Reihitsu.Analyzer.Test/`).

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| _None._ | | |

## Unresolved
_None._ (The S1125 warnings printed during `scripts/format.sh` come from building `Reihitsu.Cli` for the formatter run, in files this run did not change; the Release build is warning-free.)

## Checks
- `scripts/build.sh`: Build succeeded. 0 Warning(s), 0 Error(s).
- `scripts/test.sh --project analyzer --filter` over RH8307, RH8402, RH7309, RH7103, RH7107, RH7108: Passed! Failed: 0, Passed: 143, Skipped: 0, Total: 143.
- Text-only proof: not applicable (no comment fixes after an audit).

Role: cleaner
Commit: ec646e3
Verdict: clean

## Formatted
_None._ The formatter changed 0 of 2 changed C# files (the RH2001 code fix provider and its test class).

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| _None._ | | |

## Unresolved
_None._ (The warnings that `scripts/format.sh` prints come from building the CLI in its own project, not from the Release build of the solution; no changed file is involved.)

## Checks
- `scripts/build.sh`: Build succeeded. 0 Warning(s), 0 Error(s).
- `scripts/test.sh --project analyzer --filter ...RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzerTests`: Passed! Failed: 0, Passed: 213, Total: 213.
- Text-only proof: not applicable (no comment fix after an audit).

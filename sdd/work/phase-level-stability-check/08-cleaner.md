Role: cleaner
Commit: 501dd1d
Verdict: clean

## Formatted
_None._ `scripts/format.sh --force` formatted 0 of 27 changed C# files. A first run without `--force` stopped at the confirmation prompt and changed nothing.

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| _None._ | | |

## Unresolved
_None._ The S1125 warnings that `scripts/format.sh` prints while building the CLI are in `src/Reihitsu.Cli/`, which is not part of this run's diff. The Release solution build shows 0 warnings.

## Checks
- `scripts/build.sh`: Build succeeded. 0 Warning(s), 0 Error(s)
- `scripts/test.sh --project formatter` (whole project, because the repair touched many test files): Passed! Failed: 0, Passed: 3040, Total: 3040
- Text-only proof: not applicable (no comment fix after an audit).

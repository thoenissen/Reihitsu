Role: cleaner
Commit: (this commit; parent fc5ff89)
Verdict: clean

## Formatted
_None._ Changed since 9906bb7: the owner, the expression-body base, and the RH3208, RH5002, RH5029 test files.

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| _None._ | | |

## Unresolved
_None._ (The format script's own CLI build printed S1125 at `src/Reihitsu.Cli/DiffGenerator.cs(134,31)`; the file is not changed by this run and the Release build of the solution reports 0 warnings.)

## Checks
- `scripts/format.sh` over the 5 files: Formatted 0 of 5 file(s).
- `scripts/build.sh`: 0 Warning(s), 0 Error(s).
- `scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH3208|FullyQualifiedName~RH5029|FullyQualifiedName~RH5002"`: Failed: 0, Passed: 153.

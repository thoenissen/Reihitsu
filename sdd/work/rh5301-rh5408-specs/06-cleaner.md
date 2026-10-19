Role: cleaner
Commit: 0c8b9c0
Verdict: clean

## Formatted
_None._ `scripts/format.sh` reported "Formatted 0 of 17 file(s)". It printed S1125 warnings from building `Reihitsu.Cli`, a project this run did not change. The Release build below is clean.

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| _None._ | | |

## Unresolved
_None._

## Checks
- `scripts/build.sh`: Build succeeded. 0 Warning(s), 0 Error(s)
- `scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH530|FullyQualifiedName~RH540"`: Passed! Failed: 0, Passed: 395, Skipped: 0, Total: 395
- Text-only proof: not applicable, because no comments were fixed.

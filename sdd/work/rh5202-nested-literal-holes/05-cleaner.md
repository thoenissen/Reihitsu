Role: cleaner
Commit: 76809c9
Verdict: clean

## Formatted
_None._ `scripts/format.sh` reported "Formatted 0 of 4 file(s)".

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| _None._ | | |

## Unresolved
_None._

## Checks
- `scripts/build.sh`: Build succeeded, 0 Warning(s), 0 Error(s).
- `scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5202"`: Passed! Failed: 0, Passed: 53, Total: 53.
- `scripts/test.sh --project formatter --filter "FullyQualifiedName~RawString"`: Passed! Failed: 0, Passed: 141, Total: 141.

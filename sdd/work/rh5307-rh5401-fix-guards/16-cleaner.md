Role: cleaner
Commit: d539fa5
Verdict: clean

## Formatted
_None._ The formatter changed no file.

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| _None._ | | |

## Unresolved
_None._

## Checks
- `scripts/format.sh` on all five changed C# files: no change.
- `scripts/build.sh`: Build succeeded, 0 Warning(s), 0 Error(s).
- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH5401"`: Passed! Failed: 0, Passed: 94, Total: 94.

## Follow-up candidates
_None._

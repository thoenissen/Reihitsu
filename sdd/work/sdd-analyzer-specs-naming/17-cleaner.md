Role: cleaner
Commit: 7f002ee
Verdict: clean

## Formatted
_None._ `scripts/format.sh` over the 3 changed C# files (RH4001, RH4002, RH4009 tests): `Formatted 0 of 3 file(s)`.

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| _None._ | | |

## Unresolved
_None._

## Checks
- `scripts/build.sh`: Build succeeded, 0 Warning(s), 0 Error(s).
- `scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH4001|FullyQualifiedName~RH4002|FullyQualifiedName~RH4009"`: Passed! Failed: 0, Passed: 60.

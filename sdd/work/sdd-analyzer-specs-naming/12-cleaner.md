Role: cleaner
Commit: 472d5a2
Verdict: clean

## Formatted
_None._ `scripts/format.sh` over the 2 changed C# files (RH4008 and RH4009 tests): `Formatted 0 of 2 file(s)`.

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| _None._ | | |

## Unresolved
_None._

## Checks
- `scripts/build.sh`: Build succeeded, 0 Warning(s), 0 Error(s).
- `scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH4008|FullyQualifiedName~RH4009"`: Passed! Failed: 0, Passed: 14.

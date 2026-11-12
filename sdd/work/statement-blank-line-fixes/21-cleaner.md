Role: cleaner
Commit: (this commit; parent c3e6579)
Verdict: clean

## Formatted
_None._

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| _None._ | | |

## Unresolved
_None._ (The S1125 lines printed by the first `format.sh` call come from its Debug build of `Reihitsu.Cli`, a file the run did not change; the Release build has none.)

## Checks
- `scripts/format.sh --force` over the 49 changed C# files: Formatted 0 of 49 file(s).
- `scripts/build.sh`: 0 Warning(s), 0 Error(s).
- `scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH3208|FullyQualifiedName~RH5030|FullyQualifiedName~PrecededByABlankLine"`: Passed! Failed: 0, Passed: 430.

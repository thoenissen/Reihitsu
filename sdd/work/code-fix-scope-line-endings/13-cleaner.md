Role: cleaner
Commit: 4573b2b
Verdict: clean

## Formatted
_None._ (`scripts/format.sh`: Formatted 0 of 4 file(s); the four .cs files changed since 888654727.)

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| _None._ | | |

## Unresolved
_None._ The format script's own build of `Reihitsu.Cli` printed S1125 notes for `FormatCommandHandler.cs`, a file this run did not change; the Release build reports none.

## Checks
- `scripts/build.sh`: Build succeeded, 0 Warning(s), 0 Error(s).
- `scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH3001"`: Passed! Failed: 0, Passed: 88, Total: 88.

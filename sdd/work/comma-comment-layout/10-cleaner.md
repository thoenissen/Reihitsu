Role: cleaner
Commit: (this commit; see `git log -1 -- sdd/work/comma-comment-layout/10-cleaner.md`)
Verdict: clean

## Formatted
_None._ `scripts/format.sh --force` over the 30 changed C# files: "Formatted 0 of 30 file(s)".

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| _None._ | | |

## Unresolved
_None._

## Checks
- `scripts/build.sh`: Build succeeded, 0 Warning(s), 0 Error(s).
- `scripts/test.sh --project formatter --filter "FullyQualifiedName~LineBreaks|FullyQualifiedName~Precheck"`: Passed 1185, Failed 0.
- `scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5107"`: Passed 39, Failed 0 (the disputed Fix All test passes).

Role: cleaner
Commit: c38575f
Verdict: clean

## Formatted
_None._ `scripts/format.sh` on `src/Reihitsu.ArchitectureTests/Sdd/RequirementCoverageTests.cs`, the only changed C# file: `Formatted 0 of 1 file(s).`

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| _None._ | | |

## Unresolved
_None._ (The S1125 warnings printed during the format script's own build are in `src/Reihitsu.Cli/FormatCommandHandler.cs`, a file this run did not change; the Release build reports none.)

## Checks
- `scripts/build.sh`: `Build succeeded. 0 Warning(s) 0 Error(s)`
- `scripts/test.sh --project architecture --filter "FullyQualifiedName~Reihitsu.ArchitectureTests.Sdd.RequirementCoverageTests"`: `Passed!  - Failed: 0, Passed: 24, Skipped: 0, Total: 24`

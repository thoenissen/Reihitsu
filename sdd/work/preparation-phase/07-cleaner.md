Role: cleaner
Commit: see the commit that adds this file
Verdict: clean

## Formatted
- src/Reihitsu.Formatter/Pipeline/Preparation/PreparationPhase.cs (collection expression indentation, RH5305)

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| RH5305 (error) | PreparationPhase.cs:38 | Reformatted by scripts/format.sh |

## Unresolved
_None._ The S1125 warnings in PipelineTraceCommand.cs seen once during the format run did not reproduce in the Release build.

## Checks
scripts/build.sh: 0 Warning(s), 0 Error(s) (also with --no-incremental).
scripts/test.sh --project formatter --filter FullyQualifiedName~Pipeline: Passed 310, Failed 0.

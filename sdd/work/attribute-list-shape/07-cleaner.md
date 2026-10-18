Role: cleaner
Commit: eb0a4a3
Verdict: clean

## Formatted
_None._ `scripts/format.sh` over the 16 changed test files: "Formatted 0 of 16 file(s)." While building the formatter tool it printed S1125 warnings in `src/Reihitsu.Cli` (`FormatCommandHandler.cs`, `PipelineTraceCommand.cs`). Those files are not part of this run and the Release solution build reports none, so they were left alone.

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| _None._ | | |

## Unresolved
_None._

## Checks
- `scripts/build.sh`: Build succeeded. 0 Warning(s), 0 Error(s).
- `scripts/test.sh --project analyzer --filter "FullyQualifiedName~ShapeRulesAnalyzerTests"`: Passed! Failed: 0, Passed: 200, Skipped: 0, Total: 200.
- Text-only proof: not applicable, no comments were fixed after an audit.

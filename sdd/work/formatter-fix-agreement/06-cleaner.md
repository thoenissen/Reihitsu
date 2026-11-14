Role: cleaner
Commit: 6745619
Verdict: clean

## thoenissen/Reihitsu#1529

### Formatted
_None._ `scripts/format.sh` over the 13 changed C# files: "Formatted 0 of 13 file(s)."

### Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| _None._ | | |

### Unresolved
_None._

### Checks
- `scripts/test.sh --project analyzer --filter "RH5111...AnalyzerTests|MustNotBePrecededByBlankLineFormatterTests"`: Passed 142, Failed 0.

## thoenissen/Reihitsu#1543

### Formatted
_None._

### Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| _None._ | | |

### Unresolved
_None._

### Checks
- `scripts/test.sh --project formatter --filter "CommentBeforeBraceBlankLineTests"`: Passed 35, Failed 0.

## Whole run

- `scripts/build.sh` (Release): Build succeeded, 0 Warning(s), 0 Error(s).
- The format script's own build printed S1125 warnings in untouched `Reihitsu.Cli` files (`PipelineTraceCommand.cs`, `EditScriptBuilder.cs`); they are not in the Release build and not in the run's diff, so left alone.
- Text-only proof: N/A (no comment fixes after an audit).

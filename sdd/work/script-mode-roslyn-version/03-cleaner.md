Role: cleaner
Commit: 3285de8
Verdict: clean

## Formatted
_None._ (`scripts/format.sh` reported 0 of 4 changed C# files formatted.)

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| _None._ | | |

## Unresolved
_None._

## Checks
- `scripts/build.sh`: `0 Warning(s)`, `0 Error(s)`
- `scripts/test.sh --project architecture --filter "ShellScriptExecutableModeTests|RoslynPackageVersionTests"`: `Passed! - Failed: 0, Passed: 8, Skipped: 0, Total: 8`
- Text-only proof: n/a (no comment fixes).

## Follow-up candidates
- Unchanged files: `scripts/format.sh` printed S1125 warnings for `src/Reihitsu.Cli/Diagnostics/PipelineTraceCommand.cs` and `FormatCommandHandler.cs` while compiling the CLI in Debug; the Release build is clean and the run did not change them.

Role: cleaner
Commit: see git log (cleaner commit on top of cb8000b)
Verdict: clean

## thoenissen/Reihitsu#1597

### Formatted
_None._ `scripts/format.sh` over `src/Reihitsu.Analyzer.CodeFixes/Base/CasingCodeFixProviderBase{T}.cs`: `Formatted 0 of 1 file(s).`

### Warnings resolved
_None._

### Unresolved
_None._

### Checks
- `scripts/build.sh` (Release): `0 Warning(s)`, `0 Error(s)`.
- Focused tests (RH4105, RH4120, RH4114): see Whole run.

## thoenissen/Reihitsu#1598
Nothing to do in this round.

## thoenissen/Reihitsu#1016
Nothing to do in this round.

## thoenissen/Reihitsu#1015
Nothing to do in this round.

## thoenissen/Reihitsu#1084
Nothing to do in this round.

## Whole run
- Changed C# file of the round: one; formatter changed nothing.
- Release build: 0 warnings, 0 errors. (The format script's own CLI build printed an S1125 note at `src/Reihitsu.Cli/Diagnostics/PipelineTraceCommand.cs(483,29)`, a file this run did not change; the Release solution build reports none.)

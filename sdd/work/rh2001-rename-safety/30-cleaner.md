Role: cleaner
Commit: see git log (parent e543db0)
Verdict: clean

## Formatted
_None._ `scripts/format.sh` on the one changed C# file (RH2001 analyzer tests): Formatted 0 of 1 file(s).

## Warnings resolved
_None._

## Unresolved
_None._ Note: `scripts/format.sh` printed S1125 at `src/Reihitsu.Cli/Diagnostics/PipelineTraceCommand.cs(527,64)` while building the CLI; this file is outside the run's diff and the Release build reports 0 warnings.

## Checks
- scripts/build.sh: 0 Warning(s), 0 Error(s)
- scripts/test.sh --project analyzer --filter "FullyQualifiedName~ATupleConvertedTo": Passed 3/3

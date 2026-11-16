Role: cleaner
Commit: 3f4eeb9
Verdict: clean

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

Covers the whole run; the three issues share one diff and the same checks.

## thoenissen/Reihitsu#1505

### Formatted
_None._ `scripts/format.sh` over the 14 changed C# files: `Formatted 0 of 14 file(s).`

### Warnings resolved
_None._

### Unresolved
_None._

### Checks
- `scripts/build.sh`: `Build succeeded. 0 Warning(s) 0 Error(s)`
- `scripts/test.sh --project formatter --filter <ObservedPipeline, PhaseStability, FormattingPipeline, LineEnding, DirectivePreservation, DocumentationCommentCollapseGuard, SingleLineTypeBraceWithLeadingUsing, RawStringOtherLineTerminatorAlignment>`: `Passed! - Failed: 0, Passed: 121, Total: 121`

## thoenissen/Reihitsu#1577

### Formatted
_None._ (same run as above)

### Warnings resolved
_None._

### Unresolved
_None._

### Checks
Same build and focused test run as #1505 (they cover the seven call sites).

## thoenissen/Reihitsu#1576

### Formatted
_None._ `FormatterSelfHostingTests.cs` was among the 14 files.

### Warnings resolved
_None._

### Unresolved
_None._

### Checks
Same clean Release build. The self-hosting scan was not re-run (implementer ran it green at 5652ed0; formatting changed nothing). No comment fix, so no text-only proof.

## Follow-up candidates
_None._ The `format.sh` run printed S1125 warnings in `src/Reihitsu.Cli/Diagnostics/PipelineTraceCommand.cs` (unchanged by this run); the Release build reports none, so they are an artifact of the format script's own Cli build.

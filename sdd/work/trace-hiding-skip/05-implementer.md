Role: implementer
Commit: 594c74e
Verdict: tests pass

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Cli/Diagnostics/PipelineTraceCommand.cs` | `GetInvalidTraceExitCode` gains a third check after the syntax-error and generated-source checks: `ReihitsuFormatter.HoldsHidingCarrier(syntaxTree)`, reported through the existing `WriteInvalidPassMessage` with skip reason `code hidden in another build configuration` (pass 1: stdout, exit `0`) and failure reason `source with code hidden in another build configuration` (later pass: stderr, exit `ExitCodes.Error`); `using Reihitsu.Formatter;` added; XML summary updated to name the three checks and their order | change doc *What*, *Order of the checks*, *A later pass* (FMT-PIPE-033 applied by the trace) |

## Policy owners
`rg -n "HoldsHidingCarrier" src --glob '!*Test*/**'`: the owner is `ReihitsuFormatterHelpers.HoldsHidingCarrier` (`src/Reihitsu.Formatter`), exposed as `ReihitsuFormatter.HoldsHidingCarrier`; callers are `ReihitsuFormatter.FormatDocumentAsync`/`FormatSyntaxTree`, `DefaultSourceFormatter` (for `FormatCommandHandler`), `PlaygroundFormatter`, and now `PipelineTraceCommand`. The trace calls the public query and adds no copy of the carrier rule (FC-015). The reason text `code hidden in another build configuration` is now written in `FormatCommandHandler` and the trace, as the skeptic anticipated for the existing `syntax errors`/`generated` reasons.

## Production diff size
`git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'`: 1 file changed, 11 insertions(+), 1 deletion(-). No new type or member.

## Deleted special cases
_N/A._

## Tests run
- `scripts/test.sh --configuration Debug --project cli --filter "FullyQualifiedName~Reihitsu.Cli.Test.Unit.PipelineTraceCommandTests"` → `Passed! - Failed: 0, Passed: 23, Skipped: 0, Total: 23`
- `scripts/test.sh --configuration Debug --project cli` → `Passed! - Failed: 0, Passed: 313, Skipped: 0, Total: 313`

`scripts/format.sh` on the changed file: `Formatted 0 of 1 file(s).` (The build prints a pre-existing S1125 warning in `FormatCommandHandler.cs:501`, not touched by this change.)

## Disputed tests or scope
_None._

## Architecture findings
_None_ beyond the skeptic's (five callers each write out the ordered whole-document skip checks), which the lead already decided to fix narrowly; this change follows that decision.

## Follow-up candidates
_None_ new.

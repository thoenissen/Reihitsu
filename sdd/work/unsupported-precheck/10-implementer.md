Role: implementer
Commit: (this handover's commit; base d1c5a01a)
Verdict: tests pass

Follow-up round for `00-lead.md` Decisions 11 and 12, on the tests of `08-test-implementer.md`. The prompt named only these decisions and carried no suspected cause or hoped-for verdict beyond them. No test assertion, input, or expected output was changed.

## Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Cli/Abstractions/ISourceFormatter.cs` | `FormatSyntaxTree` returns `FormattingResult<SyntaxTree>`; `FindUnsupportedSyntax` removed from the interface | CLI-019, CLI-020, FMT-UNSUP-001, -006 (Decision 11) |
| `src/Reihitsu.Cli/Abstractions/DefaultSourceFormatter.cs` | Returns `ReihitsuFormatter.FormatSyntaxTree`'s result as is; the `FindUnsupportedSyntax` implementation is gone | Decision 11 |
| `src/Reihitsu.Cli/FormatCommandHandler.cs` | `ProcessFileAsync` formats once and reports from `result.Unsupported` (one check site); the separate query before formatting is gone. A refusal raised by any phase now reaches the `error:`/`warning:` line, the summary count, and the exit code | CLI-019, CLI-020, FMT-UNSUP-006 |
| `src/Reihitsu.Formatter/SyntaxNotSupportedException.cs` | `internal` → `public` (S3871) | Decision 12 |

## Policy owners

- `rg -n "FindUnsupportedSyntax" src/Reihitsu.Cli src/Reihitsu.Playground`: no hits. The CLI and the Playground both read the refusal from the result object. `ReihitsuFormatter.FindUnsupportedSyntax` stays for the code fixes (maintainer answer 8A).
- `rg -n "catch \(SyntaxNotSupportedException" src`: one catch, `FormattingPipeline.ExecutePhases`.

## Production diff size

- Round: `git diff --shortstat` over `src`: 4 files changed, 11 insertions(+), 25 deletions(-).
- Whole run: `git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'` (before this commit): 64 files changed, 1239 insertions(+), 518 deletions(-). No new type, no new member; one interface member removed.

## Tests run

- `scripts/build.sh` (Release): Build succeeded, 0 Warning(s), no S3871.
- `scripts/test.sh --project cli`: Passed 270, Failed 0 (the 3 formerly disputed and the 4 new tests included).
- `scripts/test.sh --project architecture`: Passed 117, Failed 0.
- `scripts/test.sh --project formatter`: Passed 4829, Failed 0.
- `scripts/format.sh` over the four changed files: formatted 0 of 4.

## Disputed tests or scope

_None._

## Architecture findings

_None._ The two check sites in the CLI that Decision 11 named are now one.

## Follow-up candidates

_None._

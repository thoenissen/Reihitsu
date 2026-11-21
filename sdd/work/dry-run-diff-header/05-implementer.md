Role: implementer
Commit: 40b0b98 (production change made on this head; the commit that adds it and this handover follows it)
Verdict: tests pass

Inputs: thoenissen/Reihitsu#975, `00-lead.md`, `02-skeptic.md`, `03-spec-author.md`, `04-test-implementer.md`, CLI-006, CLI-013, CLI-024 in `sdd/specs/cli/reihitsu-format.md`, `sdd/changes/2026-11/dry-run-diff-header.md`. The prompt carried no intended fix, cause, or hoped-for verdict.

Before the change, the 13 non-Windows tests of `CliDryRunDiffHeaderEndToEndTests` failed on the header lines as `04-test-implementer.md` states; after it all 13 pass and the Windows-only test is skipped on Linux. No test, assertion, input, or expected output was changed.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Cli/Abstractions/DefaultDiffGenerator.cs` | `Generate` names the file by where its full path lies relative to `Directory.GetCurrentDirectory()`: inside (the directory, with a trailing separator, is a prefix of the full path under the CLI's path comparer) → `a/<relative>`/`b/<relative>` with the platform separator written as `/`; otherwise the full path unchanged, without prefixes. Paths compared as written, no link resolution. New private `TryGetPathInside` | CLI-024 |
| `src/Reihitsu.Cli/DiffGenerator.cs` | New `GenerateWithNames(originalName, formattedName, …)` writes the two header names as given; `Generate(filePath, …)` keeps its `a/`/`b/` header and delegates to it, so the trace (`PipelineTraceCommand`) and `DiffGeneratorTests` are unchanged | CLI-024 (trace non-goal kept) |
| `src/Reihitsu.Cli/PathComparison.cs` (new) | One owner of the CLI's path comparer (Windows case-insensitive, elsewhere case-sensitive) | CLI-006, CLI-024 |
| `src/Reihitsu.Cli/FormatCommandHandler.cs` | `CollectFiles` uses `PathComparison.Comparer` instead of its inline copy; comment updated | CLI-006 (no behavior change) |
| `src/Reihitsu.Cli/Abstractions/IDiffGenerator.cs` | `filePath` doc: the full path the header name is derived from | — |

The `IDiffGenerator.Generate(filePath, …)` contract is unchanged (it still receives the full path), so `FormatCommandHandlerTests.ExecuteAsyncDryRunCallsDiffGenerator` and `FormatCommandHandlerParallelTests` needed no update. The new overload is named `GenerateWithNames` rather than `Generate` because an overload made the `<see cref="DiffGenerator.Generate"/>` in `DiffGeneratorTests` ambiguous (CS0419).

## Policy owners
- Path case rule: `rg -n "IsWindows\(\)" src --glob '!*Test*/**'` → before: only `FormatCommandHandler.CollectFiles` in production (the other hits are architecture tests). After: `PathComparison.Comparer`, used by `CollectFiles` (and through it `IsInBuildOutputDirectory`) and `DefaultDiffGenerator.TryGetPathInside`. `rg -n "OrdinalIgnoreCase" src/Reihitsu.Cli` finds no other path comparison (only the `.cs` extension and the y/yes answer).
- Diff header text: `rg -n "\-\-\- |\+\+\+ " src/Reihitsu.Cli` → only `DiffGenerator.GenerateWithNames`.
- Relative-path computation: `rg -n "GetRelativePath|GetCurrentDirectory" src/Reihitsu.Cli` → `Program` (default path), `FormatCommandHandler.IsInBuildOutputDirectory` (relative to a selection root for `bin`/`obj`, a different decision), `DefaultDiffGenerator.Generate`. `Path.GetRelativePath` is deliberately not used for the header: it applies the BCL's own case rule and returns `..` paths, while CLI-024 fixes the rule to CLI-006's comparer and an ancestry condition.

## Production diff size
`git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'` after the commit: 5 files changed, 77 insertions(+), 9 deletions(-). New type: `PathComparison` (internal static, one property).

## Deleted special cases
_N/A._

## Tests run
- `scripts/test.sh --configuration Debug --project cli --filter "FullyQualifiedName~Reihitsu.Cli.Test.EndToEnd.CliDryRunDiffHeaderEndToEndTests"` → `Passed! - Failed: 0, Passed: 13, Skipped: 1, Total: 14`
- `scripts/test.sh --configuration Debug --project cli` → `Passed! - Failed: 0, Passed: 438, Skipped: 1, Total: 439`
- `scripts/test.sh --configuration Debug --project architecture` → `Passed! - Failed: 0, Passed: 198, Skipped: 0, Total: 198`
- `scripts/format.sh` over the changed files: clean on the last run. The Debug builds of the last runs printed no warning for the changed files.

## Disputed tests or scope
- New type `PathComparison` (internal): extracted so the header and CLI-006's dedupe share one case rule (FC-015), as the skeptic's internal choice 2 asks. The lead decides whether it stays.
- New public (internal-class) member `DiffGenerator.GenerateWithNames`.

## Architecture findings
_None._

## Follow-up candidates
_None_ beyond the `scripts/trace.sh` header already in `00-lead.md`.

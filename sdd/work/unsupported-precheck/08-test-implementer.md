Role: test implementer
Commit: 165d5802 (base; this handover's commit follows it)
Verdict: tests written

Follow-up round for `00-lead.md` Decisions 10, 11, and 12, and `06-implementer.md` disputes 1, 2, and 4. The prompt named these decisions and did not suggest a fix or verdict beyond them.

## Changes

- **Decision 10.** In `src/Reihitsu.Cli.Test/EndToEnd/CliUnsupportedSyntaxEndToEndTests.cs`, `FormattedSource` no longer ends with `\n`. It is now `"class B\n{\n    void M()\n    {\n    }\n}"`, which is formatter output, because the cleanup phase removes the final newline. No assertion changed.
- **Decision 11, mock setup.** `src/Reihitsu.Cli.Test/Unit/FormatCommandHandlerTests.cs` has three `FormatSyntaxTree(...).Returns(...)` setups: `SetupFormatter`, `CaptureParsedLanguageVersion`, and `ExecuteAsyncLargeRunConfirmedUsesContentReadAfterConfirmation`. Each one now wraps its tree in `new FormattingResult<SyntaxTree>(tree, null)`. The internal constructor is reachable through `InternalsVisibleTo("Reihitsu.Cli.Test")`. No test set up `FindUnsupportedSyntax`, so there was nothing to remove. No assertion changed.
- **Decision 11, new tests.** New helpers:
  - `CreateHandler` takes an optional `suppressUnsupported` parameter.
  - `SetupRefusingFormatter` makes `FormatSyntaxTree` return the tree it receives unchanged, with an `UnsupportedSyntax` located in that tree. This is how the formatter reports a refusal that a phase raises during the run (FMT-UNSUP-006).

  The new tests are in the region `Unsupported Syntax`, listed below. No existing CLI test covered this: before, a refusal reached the handler only through the separate query.
- **Decision 12.** In `src/Reihitsu.ArchitectureTests/Formatter/PublicApiSurfaceTests.cs`, the allow-list regex now includes `SyntaxNotSupportedException`. The test is renamed to `OnlyReihitsuFormatterItsResultTypesAndItsRefusalExceptionShouldBePublicInFormatterAssembly`. I added no test that requires the type to be public: the decision allows it, and S3871 already enforces it.

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `FormatCommandHandlerTests.ExecuteAsyncRefusalInFormattingResultIsReportedAsError` (new) | CLI-019 | NSubstitute `ISourceFormatter`, `SetupRefusingFormatter` | does not compile until `ISourceFormatter.FormatSyntaxTree` returns `FormattingResult<SyntaxTree>` |
| `FormatCommandHandlerTests.ExecuteAsyncRefusalInFormattingResultLeavesEncodingUnchanged` (new, `--utf8-bom`) | CLI-019 | same | same |
| `FormatCommandHandlerTests.ExecuteAsyncCheckModeRefusalInFormattingResultIsReportedOnlyAsError` (new, `--check --verbose`) | CLI-019 | same | same |
| `FormatCommandHandlerTests.ExecuteAsyncSuppressedRefusalInFormattingResultIsReportedAsWarning` (new) | CLI-020 | same | same |
| every other `FormatCommandHandlerTests` test (setup adapted only) | unchanged tags | `SetupFormatter` and the two inline setups | same |
| `CliUnsupportedSyntaxEndToEndTests.SpecExampleCheckReportsTheRefusedFileOnlyOnStandardError`, `SpecExampleDryRunReportsTheRefusedFileOnlyOnStandardError`, `SuppressUnsupportedDryRunWithOtherwiseFormattedFilesSucceeds` (input corrected) | unchanged tags | `Program.Main` | pass once the CLI project compiles |
| `PublicApiSurfaceTests.OnlyReihitsuFormatterItsResultTypesAndItsRefusalExceptionShouldBePublicInFormatterAssembly` (renamed, allow-list widened) | — (no tag, as before) | ArchUnitNET | passes |

The four new tests assert:
- the exact standard-error line, `error:` or `warning:` with `(line,column)` taken from the result's location;
- the exact standard-output summary, including `Skipped 1 file(s) with unsupported syntax.`;
- the exit code: 2, or 0 when suppressed;
- that no file was written, including when `--utf8-bom` would otherwise rewrite the encoding.

In check mode with `--verbose`, the test also asserts that no `Not formatted:` line and no `Unchanged:` line is printed.

## Verification on this head

On this head, the Release build stops at S3871 in `SyntaxNotSupportedException.cs`, a production error, and the CLI test project does not compile against `SyntaxTree FormatSyntaxTree`. Both are expected until the implementer applies Decisions 11 and 12.

To check the tests against a conforming implementation, I used a throwaway git worktree in the scratchpad. It held these test changes and a minimal production change that is not committed and is not a proposal:
- `public` on the exception;
- the interface returns `FormattingResult<SyntaxTree>`, and `FindUnsupportedSyntax` is removed from it;
- the handler reports from `result.Unsupported`.

Results in that worktree:
- `scripts/test.sh --project cli`: Passed 270, Failed 0. This includes the 3 previously disputed tests and the 4 new ones.
- `scripts/test.sh --project architecture`: Passed 117.
- I made the handler ignore `result.Unsupported`. All 4 new tests then failed on their assertions (exit code, warning line), which are the right reasons.

I removed the worktree afterwards.

## Not covered, and why

- **A real phase refusal end-to-end through the CLI.** No phase refuses an input yet ("No phase refuses an input in this way yet", FMT-UNSUP-006). The formatter side, where a phase throws and `ExecutePhases` returns the outcome, is already covered by `FormattingPipelineTests`, which carries two `[Requirement("FMT-UNSUP-006")]` tests with a `ThrowingPhase`. On the CLI side, the precheck refusal and a phase refusal reach the handler through the same `FormattingResult<SyntaxTree>.Unsupported`, so the mock-based tests cover both paths.
- **FMT-UNSUP-006 tag on the CLI tests.** I did not add it. These tests verify CLI-019 and CLI-020 for a refusal carried by the result. They do not check the formatter's conversion of the refusal into that result.
- **Dry-run variant of the result path.** It is the same handler branch as write and check mode, `ProcessFileAsync`, which returns before the mode-specific `HandleChangedFileAsync`. The end-to-end dry-run tests already cover it.
- **Line endings.** No layout is involved. The location arithmetic is covered end to end, including CRLF, by `CliUnsupportedSyntaxEndToEndTests`.

## Commands

```shell
scripts/test.sh --project cli --filter "FullyQualifiedName~Reihitsu.Cli.Test.Unit.FormatCommandHandlerTests"
scripts/test.sh --project cli --filter "FullyQualifiedName~Reihitsu.Cli.Test.EndToEnd.CliUnsupportedSyntaxEndToEndTests"
scripts/test.sh --project architecture --filter "FullyQualifiedName~Reihitsu.ArchitectureTests.Formatter.PublicApiSurfaceTests"
```

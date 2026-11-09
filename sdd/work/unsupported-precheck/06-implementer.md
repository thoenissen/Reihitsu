Role: implementer
Commit: f2b7638d
Verdict: test disputed

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Inputs: `00-lead.md`, the accepted change doc, the spec diff against `158e8fc`, `02-skeptic.md`, `04-spec-author.md`, and `05-test-implementer.md`.

Every handed-over test passes except three CLI end-to-end tests whose input constant is not formatted (see *Disputed tests or scope*). No assertion, input, or expected output was changed.

## Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/Precheck/PrecheckPhase.cs` (new) | The first phase. `FindUnsupportedSyntax(SyntaxNode)` is the one predicate: every `VariableDeclarationSyntax` in the node, its span from the owner's first token through the `;` or `)` (`for`: first `;`), skipping the first token's leading trivia, the terminator's trailing trivia, and the inside of each initializer value (from the value's first token's trailing trivia through its last token's leading trivia). It returns the first comment in the text, located by the trivia's `FullSpan` in the node's tree. `Execute` throws `SyntaxNotSupportedException` for it. A declaration that is the input itself gets only its own tokens (FMT-UNSUP-003) | FMT-UNSUP-001, -003, -004, -007 |
| `src/Reihitsu.Formatter/FormattingResult{T}.cs`, `UnsupportedSyntax.cs`, `SyntaxNotSupportedException.cs` (new) | Public `FormattingResult<T>` (`Output`, `Unsupported`, internal constructor), public `UnsupportedSyntax` (`Reason`, `Location`), internal exception carrying an `UnsupportedSyntax` | FMT-UNSUP-001, -006 |
| `src/Reihitsu.Formatter/Pipeline/FormattingPipeline.cs` | `PrecheckPhase` first in `CreatePhases`. `Execute`/`ExecutePhases` return `FormattingResult<SyntaxNode>`. `ExecutePhases` catches `SyntaxNotSupportedException` once and returns its input unchanged with the shape. The observer wrapping in `ExecutePhase` excludes the exception, so it is never wrapped in `InvalidOperationException`. A refusing phase is not reported to the observer | FMT-PIPE-001, FMT-UNSUP-001, -006 |
| `src/Reihitsu.Formatter/ReihitsuFormatter.cs` | The five formatting entry points return `FormattingResult<…>`. The node-level and context paths map the location from the detached copy the phases received back to the caller's tree by offset (`MapToCallerTree`). The split path propagates the context's refusal. New `FindUnsupportedSyntax`. `SplitsFieldDeclaration` summary no longer names comments | FMT-UNSUP-001, -002, -007, FMT-PIPE-031 |
| `src/Reihitsu.Formatter/Pipeline/StructuralTransforms/Rewriter/FieldDeclarationSplitTransform.cs` | Deleted `CarriesCommentInRepeatedPart`, `GetComments`, `BuildTrailingTrivia`, `BuildLeadingTrivia`, the `nextFieldHasLeadingComment` argument, `_context` and its constructor parameter. `CreateSplitField` (now static) drops a later declarator's leading trivia and a non-final field's trailing trivia, and keeps the terminator's leading trivia. Summaries rewritten | FMT-STRUCT-022, -026, -030, FMT-PIPE-031 |
| `src/Reihitsu.Formatter/Pipeline/Core/Utilities/TokenGapUtilities.cs` | Deleted `HasLineBreak(SyntaxTriviaList)`; its only caller was `BuildTrailingTrivia` | change doc *Deleted* |
| `StructuralTransformPhase.cs`, `PreparationPhase.cs` | New transform constructor; `.Output` of `ExecutePhases` | — |
| `src/Reihitsu.Cli/…` (`Program`, `ParseResult`, `FormatCommandHandler`, `FileProcessResult`, `ISourceFormatter`, `DefaultSourceFormatter`, `PipelineTraceCommand`) | `--suppress-unsupported` with its help line. The handler asks `FindUnsupportedSyntax` after the syntax-error and generated checks, writes the `error:`/`warning:` line to standard error, and counts the file under `Skipped <n> file(s) with unsupported syntax.`. A refused file forces exit code 2 unless suppressed. The trace prints a line for a refused pass | CLI-001, -014, -017, -019, -020 |
| `src/Reihitsu.Playground/…` | `PlaygroundFormatResult.Unsupported`; the page shows the PLAY-008 message | PLAY-008 |
| 41 files under `src/Reihitsu.Analyzer.CodeFixes` | `.Output` at every formatter call, and a `FindUnsupportedSyntax` check at registration on the node or context each fix formats (see below) | SHARED-FIX-010 |

### What each formatting fix asks at registration

- **The node it formats in the document**: RH3202, RH3203, RH5101, RH5102, RH5113, RH5205, RH5206, RH5301, RH5303–RH5307, RH5401, RH5404, RH5408, RH5417, the empty-type base, and the expression-body base. RH5110, RH8305, RH8307, the operator-spacing base, and the statement-braces base now find that node at registration through a helper they share with the action. RH5302 asks about the outermost logical expression. RH5308 asks about the enclosing statement or member. RH5403 asks about the statement that owns the block. RH5204 asks about the scope, but not for a directive line, which it indents without the formatter.
- **The context it formats in**: RH7101 asks about the declaring type. RH5111 asks about `GetFormattingContextNode`. RH3208 asks about the line-starting label, through a new `GetFormattingInput` override on the expression-body base. RH5112 and RH5201 ask about the chain's context, or the chain when it has none, through `FluentChainCodeFixHelper.IsFormattingRefused`.
- **The node it builds**: RH2001 asks about the field it builds. RH3004 asks about the lambda, now built by `CreateLambdaExpression`. RH7004 asks about the using statement, now built by `CreateUsingStatement`. RH7107 and RH7108 ask about the declaration with its accessors moved. RH8309 asks about the declaration with its documentation reordered. The modifier-ordering base asks about the declaration with its modifiers reordered. The type-member-ordering base asks about the type with the member moved.
- **RH7104 asks about the type as it stands, not the type it builds.** Building that type needs the semantic model, because the accessibility is resolved through the symbol. The fix only adds keywords in front of the type's modifiers, and no variable-declaration span contains those modifiers, so the predicate gives the same answer for both. A comment in the code says so.

## Policy owners

- Unsupported shapes: `rg -n "VariableDeclarationSyntax" src/Reihitsu.Formatter` finds the predicate in `PrecheckPhase` only. The one other hit, `DeclaredNameSpacingRule`, is a spacing rule. `ReihitsuFormatter.FindUnsupportedSyntax` delegates to `PrecheckPhase.FindUnsupportedSyntax`. No code fix, the CLI, or the Playground holds a copy: `rg -l "FindUnsupportedSyntax" src` lists the 41 fixes, the CLI's `ISourceFormatter`, `DefaultSourceFormatter`, and `FormatCommandHandler`, and the formatter itself.
- Refusal outcome: `rg -n "catch \(SyntaxNotSupportedException|SyntaxNotSupportedException\(" src` finds one catch, in `FormattingPipeline.ExecutePhases`, and one throw, in `PrecheckPhase.Execute`.

## Production diff size

`git diff --shortstat 158e8fc..f2b7638d -- src ':(exclude,glob)src/*Test*/**'`: 63 files changed, 1237 insertions(+), 517 deletions(-). The formatter alone accounts for 10 files, +549/−276. The split transform is +38/−150 and `TokenGapUtilities` is −11.

New types:

- `FormattingResult<T>` and `UnsupportedSyntax` (public; FMT-UNSUP-001)
- `SyntaxNotSupportedException` (internal; FMT-UNSUP-006)
- `PrecheckPhase`

New public members:

- `ReihitsuFormatter.FindUnsupportedSyntax`
- `ParseResult.SuppressUnsupported`
- `ISourceFormatter.FindUnsupportedSyntax` (internal)
- `PlaygroundFormatResult.Unsupported`

## Tests run

All runs were in Debug.

- `scripts/test.sh --project formatter --filter "<the test implementer's focused filter>"`: Passed 294, Failed 0.
- `scripts/test.sh --project formatter`: Passed 4829, Failed 0.
- `scripts/test.sh --project analyzer`: Passed 7731, Failed 0.
- `scripts/test.sh --project cli`: Passed 263, Failed 3. The three failures are disputed below.
- `scripts/test.sh --project playground`: Passed 19.
- `scripts/test.sh --project architecture`: Passed 117.
- `scripts/test.sh --project core`: Passed 660.
- `scripts/test.sh --project tooling`: Passed 126.

`scripts/format.sh --force` ran over every changed production file and refused none of them. A no-incremental build leaves one warning, described below.

## Disputed tests or scope

1. **Three CLI end-to-end tests use a "formatted" file that the formatter changes.** The tests are `CliUnsupportedSyntaxEndToEndTests.SpecExampleCheckReportsTheRefusedFileOnlyOnStandardError`, `SpecExampleDryRunReportsTheRefusedFileOnlyOnStandardError`, and `SuppressUnsupportedDryRunWithOtherwiseFormattedFilesSucceeds`.
   - **The cause.** The constant `FormattedSource = "class B\n{\n    void M()\n    {\n    }\n}\n"` ends with a line break. The cleanup phase removes the final newline, so `B.cs` is reported as `Not formatted:` or `Would format:` with a diff.
   - **What fails.** The tests assert `0 of 2 file(s) …`, and exit code 0 for the suppressed dry run.
   - **What the spec requires.** CLI-019 and CLI-020 call `B.cs` formatted.
   - **The fix.** Drop the trailing `\n` from the constant. The output the tests assert is then exactly what the code produces: the error line, the summary, and the exit code are correct in every other CLI test.
   - **Confirmation.** `Reihitsu.Cli.dll --dry-run` on that text reports `Would format` with `\ No newline at end of file`.
2. **The CLI asks the query; `ISourceFormatter.FormatSyntaxTree` still returns `SyntaxTree`.** `FormatCommandHandlerTests` mocks `FormatSyntaxTree(...).Returns(SyntaxTree)`, and that test is unchanged, so the interface keeps that return type. The handler therefore asks `ISourceFormatter.FindUnsupportedSyntax` first, the same pattern as the existing `IsAutoGeneratedSource` query. `DefaultSourceFormatter.FormatSyntaxTree` returns `.Output`.
   - **Limitation.** A refusal that a phase raises during the run (FMT-UNSUP-006) would leave the file unchanged but would not be reported by the CLI. No phase raises one today.
   - **Alternative.** Return `FormattingResult<SyntaxTree>` from the interface and change that test's mock setup. That change belongs to the test implementer.
3. **Scope.** The production diff is far larger than the formatter paths the handovers name, for three reasons:
   - the about 45 code-fix call sites, accepted in escalation 9;
   - the three public types and one public method that the spec requires;
   - the CLI and the Playground.

   Several fixes now build their node at registration through an extracted helper: RH3004, RH7004, RH7107, RH7108, RH8309, and the modifier-ordering base.
4. **Remaining warning: `S3871` (make `SyntaxNotSupportedException` public).** Making it public would fail `PublicApiSurfaceTests.OnlyReihitsuFormatterAndItsResultTypesShouldBePublicInFormatterAssembly`, and I add no suppression. This is for the cleaner.

## Architecture findings

_None._ One check site: the precheck runs on exactly the node `FormattingPipeline.Execute` receives, and the code fixes, the CLI, and the Playground ask the same predicate. The refusal is caught once, in `ExecutePhases`, and is never wrapped. The location is mapped back to the caller's tree in `ReihitsuFormatter`, which already owns detaching the node.

## Special cases

- **Deleted, as the change doc lists**:
  - `CarriesCommentInRepeatedPart`, together with its condition in `Splits`;
  - `GetComments`, `BuildTrailingTrivia`, `BuildLeadingTrivia`, and `nextFieldHasLeadingComment`;
  - `_context` and the transform's constructor parameter;
  - `TokenGapUtilities.HasLineBreak(SyntaxTriviaList)`.

  `CreateSplitField` lost its comment carry-over. The test implementer flagged its first-declarator and terminator handling: that code now carries only whitespace, as FMT-STRUCT-026 states, and was simplified to `WithoutLeadingTrivia` and empty trivia lists.
- **Further candidates found**: _None._ I searched the formatter for code that handles both declarations and comments: `rg -l "VariableDeclarat|FieldDeclaration|LocalDeclarationStatement|EqualsValueClause"` piped into `xargs grep -l Comment`. It finds only the split, the precheck, `ReihitsuFormatter`, and `SwitchCaseBraceRewriter`. `SwitchCaseBraceRewriter` uses a local declaration for scope, not for comments. `LineBreakAssignmentRewriter.NormalizeOperatorPlacement` is shared with properties, enum members, and assignments, so it stays as the change doc says.

## Follow-up candidates

- `ReihitsuFormatter.FormatNodeInDocumentAsync` and the context entry point detach the target and then run the precheck on it. A code fix that asked `FindUnsupportedSyntax` at registration therefore checks the same node twice per fix. This costs time only.
- `sdd/specs/README.md` lists `—` as the code owners of FMT-UNSUP. `PrecheckPhase`, `FormattingResult<T>`, `UnsupportedSyntax`, and `SyntaxNotSupportedException` could be named there. That is the spec author's call; I did not edit `sdd/specs`.

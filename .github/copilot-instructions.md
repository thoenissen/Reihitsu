# Reihitsu Copilot Instructions

## Build, test, and lint

```powershell
dotnet build Reihitsu.sln -c Release --verbosity minimal
```

```powershell
dotnet test Reihitsu.Analyzer.Test\Reihitsu.Analyzer.Test.csproj -c Release --verbosity minimal
dotnet test Reihitsu.Formatter.Test\Reihitsu.Formatter.Test.csproj -c Release --verbosity minimal
dotnet test Reihitsu.Core.Test\Reihitsu.Core.Test.csproj -c Release --verbosity minimal
dotnet test Reihitsu.Cli.Test\Reihitsu.Cli.Test.csproj -c Release --verbosity minimal
```

Single-test examples:

```powershell
dotnet test Reihitsu.Analyzer.Test\Reihitsu.Analyzer.Test.csproj -c Release --no-build --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Formatting.RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzerTests.VerifyExpressionBodiedMethodsAreDetectedAndFixed"
dotnet test Reihitsu.Formatter.Test\Reihitsu.Formatter.Test.csproj -c Release --no-build --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Unit.Indentation.LayoutComputerTests.ComputeReturnsNonEmptyModelForSimpleClass"
dotnet test Reihitsu.Core.Test\Reihitsu.Core.Test.csproj -c Release --no-build --filter "FullyQualifiedName~Reihitsu.Core.Test.CasingUtilitiesTests.ToCamelCaseTest"
dotnet test Reihitsu.Cli.Test\Reihitsu.Cli.Test.csproj -c Release --no-build --filter "FullyQualifiedName~Reihitsu.Cli.Test.Unit.ProgramTests.ParseArgumentsUnknownOptionReturnsUnknownOption"
```

## Workflow expectations

- Before running tests, run `Reihitsu.Cli` over the changed paths so formatting issues are corrected first. A repository-local invocation is:

```shell
dotnet run --project Reihitsu.Cli -- <changed-path-1> [<changed-path-2> ...]
```

- Do not consider a change complete until all relevant unit tests pass.
- For analyzer bug fixes, reproduce the bug in a unit test before changing production code.
- For formatter bug fixes, add a regression test first before implementing the fix.
- For analyzer tests, prefer many small, focused tests over one large test with many cases.

## Code-fix coverage

A code fix is never required to correct every case its diagnostic reports. Some inputs can only be corrected by hand, and reporting the diagnostic while offering no action is a complete, intended outcome for them — not a defect, and not by itself a reason to raise a report's severity.

- A reported diagnostic with no available fix is a supported end state. "The diagnostic has no fix here" is an observation that still has to be argued on its merits; it is never a defect on its own.
- For new analyzer rules, only implement a code fix when it can be delivered comprehensively. If only light or partial support is possible, omit the code fix.
- Prefer no fix over an unsafe one. A rewrite that would drop, reorder, or relocate user-authored comments, directives, disabled text, or literals must withhold itself for that input.
- None of this licenses an imprecise guard. A fix that refuses inputs it could rewrite safely is still worth narrowing, but the defect there is the guard's scope — how much safe ground it gives away — and not the bare fact that a diagnostic went unfixed.
- When a rule's fix deliberately leaves a class of inputs to manual correction, record that in `documentation/rules/RH####.md` so the gap reads as a decision rather than an oversight.

## Custom agents

The repository defines custom Copilot agents under `.github/agents`:

- `analyzer-rule-creator.agent.md`
- `formatter-extension.agent.md`
- `analyzer-rule-bugfix.agent.md`
- `formatter-bugfix.agent.md`

Use the agent that matches the task so the repository-specific workflow and checklist are applied from the start.

## High-level architecture

- `Reihitsu.Formatter` is the shared formatting engine. `ReihitsuFormatter` is the public entry point, and `Pipeline\FormattingPipeline` applies phases in this order: structural transforms, region formatting, documentation comments, using directives, blank lines, line breaks, switch-case braces, horizontal spacing, indentation/alignment, raw-string alignment, cleanup, line-ending normalization.
- `Reihitsu.Cli` packages the `reihitsu-format` .NET tool. `Program` parses arguments and hands execution to `FormatCommandHandler`, which walks files/directories, skips `bin\`, `obj\`, and generated files, then formats through the shared formatter. `--check` and `--dry-run` are first-class modes, not wrappers around shell diff tools.
- `Reihitsu.Core` contains shared Roslyn syntax and ordering utilities that are reused by analyzers, code fixes, and the formatter.
- `Reihitsu.Analyzer` contains Roslyn analyzers grouped by RH rule IDs and categories. `Reihitsu.Analyzer.CodeFixes` contains the matching code fixes and depends on both the analyzer project and the formatter project.
- `Reihitsu.Analyzer.Package` is the shipping NuGet package. It packs `Reihitsu.Analyzer.dll`, `Reihitsu.Analyzer.CodeFixes.dll`, and `Reihitsu.Formatter.dll` together under `analyzers/dotnet/cs`, so analyzer fixes can reuse formatter behavior inside the package.
- Test projects mirror the runtime surfaces: analyzer tests use Roslyn verifier helpers, formatter tests cover unit/regression/idempotency/full-pipeline behavior, Core tests cover the shared utility assembly directly, and CLI tests cover unit, integration, and end-to-end flows.

<!-- formatter-phase-ownership:start -->
### Formatter phase ownership

The trace tools show these top-level phase boundaries without modifying the input: `scripts/trace.sh <file> [--passes N] [--no-install]` or `.\scripts\trace.ps1 <file> [-Passes N] [-NoInstall]`. They default to three passes, print a unified diff for each phase that changes line content (or line-ending counts when only separators change), and stop early when a complete pass is stable.

Formatter transformations are non-destructive: they may rewrite syntax and trivia into canonical form, but they must preserve user-authored directives, comments, disabled text, literals, and executable information. Region formatting may normalize names, matching comments, layout, indentation, and line endings, but it never deletes `#region` or `#endregion`; removing a misplaced region remains an explicit analyzer code-fix action.

| Phase | Decisions it owns | Primary policy/guards |
|---|---|---|
| `StructuralTransformPhase` | Syntax-shape rewrites and trivia transfer when replacing tokens or nodes, including field splitting | Ordered `CreateRewriters`; per-rewriter guards such as `CanRewrite`, `RequiresExpressionBodyPreservation`, `CarriesDirective`, and the field-terminator trivia mapping |
| `RegionFormattingPhase` | Region-name capitalization and matching `#endregion` comments while preserving every directive | `RegionNamingRewriter.Rewrite` |
| `DocumentationCommentFormattingPhase` | Relocation of off-position `///` comments and XML/exterior normalization | `IsAfterSourceOnSameLine`; `HasOwningLeadingSourceOnSameLine`; `DocumentsFollowingCode`; `DocCommentElementNormalizer.RequiresNormalization` |
| `UsingDirectiveOrderingPhase` | Safe canonical ordering, group boundaries, and preservation of headers/comments attached to moved usings | `UsingDirectiveOrderingSafety.CanSafelyReorder`; `UsingGrouping.ComputeCanonicalOrder`; `AreInSameGroup`; `UsingLeadingTriviaBuilder`; `UsingTrailingTriviaBuilder` |
| `BlankLinePhase` | Blank-line boundaries around comments, directives, statements, braces and breaks, plus excessive-line collapse | `BlankLineEditor`/`TokenGapAnalysis`; `IsStrandedInlineDocumentation`; `BlankLineCollapser` |
| `LineBreakPhase` | Vertical layout for braces, lists, assignments, operators, chains, ternaries, constraints and attributes | Ordered `CreateRewriters`; `LineBreakDetection`; `TokenGapNormalizer`; `BracePlacer`; syntax-specific rewriter guards |
| `SwitchCaseBracePhase` | Whether switch sections are uniformly braced or unbraced and whether trivia makes rewriting unsafe | `IsMultiLineSection`; `IsFallThroughSection`; `HasCrossSectionGotoTarget`; `SectionCarriesDirectives`; `BraceTokensCarryCommentsOrDirectives` |
| `HorizontalSpacingPhase` | Same-line token spacing and the two-sided gaps split by comments | `SpacingPolicy.GetDesiredSpacesAfter`; `AreSeparatedByEndOfLine`; the next-token leading-comment guard; `TrimGapBeforeToken` |
| `IndentationPhase` | Desired token columns, alignment, and indentation of trivia that genuinely starts a line | `LayoutComputer.Compute`; `IndentationRewriter.Apply`; previous-token trailing-EOL detection; `RebuildLeadingTrivia` |
| `RawStringAlignmentPhase` | Alignment of multiline raw-string content and closing markers after indentation | Raw-string token-kind checks and `openingColumn != closingColumn` in the replacement methods |
| `CleanupPhase` | Trailing whitespace, whitespace tabs, directive-interior tabs, and final-newline removal | `CleanWhitespaceBeforeEndOfLine`; `StripTrailingWhitespaceAtEndOfLine`; `NormalizeDirectiveTriviaTabs`; `RemoveTrailingEndOfLineTrivia` |
| `LineEndingNormalizationPhase` | Final normalization of ordinary and XML-documentation newline tokens | `EndOfLineTrivia`/`XmlTextLiteralNewLineToken` whose text differs from `FormattingContext.EndOfLine` |
<!-- formatter-phase-ownership:end -->

## Key conventions

- Diagnostic ID ranges carry meaning across the repo: `RH0###` Analyzer, `RH1###` Performance, `RH2###` Design, `RH3###` Clarity, `RH4###` Naming, `RH5###` Layout, `RH6###` Spacing, `RH7###` Organization, `RH8###` Documentation.
- Diagnostic suffix letters such as `A` are reserved for alternative rules; new rules must use their own numeric IDs.
- Rule documentation is part of the implementation contract. `DiagnosticAnalyzerBase<TAnalyzer>` builds each diagnostic `helpLinkUri` as `documentation/rules/RH####.md`, so new or renamed rules should keep the matching rule doc in sync.
- Formatting-aware code fixes should delegate final layout to `ReihitsuFormatter.FormatNodeInDocumentAsync` (or `FormatNode` for detached nodes) instead of editing trivia manually.
- Analyzer tests follow the `AnalyzerTestsBase<TAnalyzer>` / `AnalyzerTestsBase<TAnalyzer, TCodeFix>` pattern and use Roslyn markup like `{|#0:...|}` for expected diagnostic locations.
- CLI end-to-end tests call `Program.Main()` directly, use the console/temp-directory helpers in `Reihitsu.Cli.Test\Helpers`, and are marked `[DoNotParallelize]`.
- The formatter and CLI intentionally leave syntax-invalid or generated code alone. Preserve that behavior when changing formatting flows: the formatter returns the original tree/document for syntax errors or auto-generated source, and the CLI skips generated files such as `.Designer.cs`, `.g.cs`, and `.g.i.cs`.

## Instructions

- The repository language is English. All communication, documentation, and code should be in English, even if user input is in another language.
- When drafting pull requests, keep the description concise, link related issues explicitly (for example `Closes #123`), and do not include a list of locally executed tests because CI already reports that information.

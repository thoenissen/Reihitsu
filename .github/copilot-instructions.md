# Reihitsu Copilot Instructions

## Build, test, and lint

```powershell
dotnet build Reihitsu.sln -c Release --verbosity minimal
```

```powershell
dotnet test Reihitsu.Analyzer.Test\Reihitsu.Analyzer.Test.csproj -c Release --verbosity minimal
dotnet test Reihitsu.Formatter.Test\Reihitsu.Formatter.Test.csproj -c Release --verbosity minimal
dotnet test Reihitsu.Cli.Test\Reihitsu.Cli.Test.csproj -c Release --verbosity minimal
```

Single-test examples:

```powershell
dotnet test Reihitsu.Analyzer.Test\Reihitsu.Analyzer.Test.csproj -c Release --no-build --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Formatting.RH0325ExpressionStyleMethodsShouldNotBeUsedAnalyzerTests.VerifyExpressionBodiedMethodsAreDetectedAndFixed"
dotnet test Reihitsu.Formatter.Test\Reihitsu.Formatter.Test.csproj -c Release --no-build --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Unit.Indentation.LayoutComputerTests.ComputeReturnsNonEmptyModelForSimpleClass"
dotnet test Reihitsu.Cli.Test\Reihitsu.Cli.Test.csproj -c Release --no-build --filter "FullyQualifiedName~Reihitsu.Cli.Test.Unit.ProgramTests.ParseArgumentsUnknownOptionReturnsUnknownOption"
```

There is no separate local lint command. `dotnet build` runs the repo's StyleCop-based linting because `Directory.Build.props` wires in `StyleCop.Debug.ruleset`, `StyleCop.Release.ruleset`, and `stylecop.json`.

## Workflow expectations

- Before running tests, run `Reihitsu.Cli` over the changed paths so formatting issues are corrected first. A repository-local invocation is:

```shell
dotnet run --project Reihitsu.Cli -- <changed-path-1> [<changed-path-2> ...]
```

- Do not consider a change complete until all relevant unit tests pass.
- For new analyzer rules, only implement a code fix when it can be delivered comprehensively. If only light or partial support is possible, omit the code fix.
- For analyzer bug fixes, reproduce the bug in a unit test before changing production code.
- For formatter bug fixes, add a regression test first before implementing the fix.
- For analyzer tests, prefer many small, focused tests over one large test with many cases.

## Custom agents

The repository defines custom Copilot agents under `.github/agents`:

- `analyzer-rule-creator.agent.md`
- `formatter-extension.agent.md`
- `analyzer-rule-bugfix.agent.md`
- `formatter-bugfix.agent.md`

Use the agent that matches the task so the repository-specific workflow and checklist are applied from the start.

## High-level architecture

- `Reihitsu.Formatter` is the shared formatting engine. `ReihitsuFormatter` is the public entry point, and `Pipeline\FormattingPipeline` applies phases in this order: structural transforms, region formatting, blank lines, line breaks, switch-case braces, horizontal spacing, indentation/alignment, raw-string alignment, cleanup.
- `Reihitsu.Cli` packages the `reihitsu-format` .NET tool. `Program` parses arguments and hands execution to `FormatCommandHandler`, which walks files/directories, skips `bin\`, `obj\`, and generated files, then formats through the shared formatter. `--check` and `--dry-run` are first-class modes, not wrappers around shell diff tools.
- `Reihitsu.Analyzer` contains Roslyn analyzers grouped by RH rule IDs and categories. `Reihitsu.Analyzer.CodeFixes` contains the matching code fixes and depends on both the analyzer project and the formatter project.
- `Reihitsu.Analyzer.Package` is the shipping NuGet package. It packs `Reihitsu.Analyzer.dll`, `Reihitsu.Analyzer.CodeFixes.dll`, and `Reihitsu.Formatter.dll` together under `analyzers/dotnet/cs`, so analyzer fixes can reuse formatter behavior inside the package.
- Test projects mirror the runtime surfaces: analyzer tests use Roslyn verifier helpers, formatter tests cover unit/regression/idempotency/full-pipeline behavior, and CLI tests cover unit, integration, and end-to-end flows.

## Key conventions

- Diagnostic ID ranges carry meaning across the repo: `RH00xx` Clarity, `RH01xx` Design, `RH02xx` Naming, `RH03xx` Formatting, `RH04xx` Documentation, `RH05xx` Performance.
- Rule documentation is part of the implementation contract. `DiagnosticAnalyzerBase<TAnalyzer>` builds each diagnostic `helpLinkUri` as `documentation/rules/RH####.md`, so new or renamed rules should keep the matching rule doc in sync.
- Formatting-aware code fixes should delegate final layout to `ReihitsuFormatter.FormatNodeInDocumentAsync` (or `FormatNode` for detached nodes) instead of editing trivia manually.
- Analyzer tests follow the `AnalyzerTestsBase<TAnalyzer>` / `AnalyzerTestsBase<TAnalyzer, TCodeFix>` pattern and use Roslyn markup like `{|#0:...|}` for expected diagnostic locations.
- CLI end-to-end tests call `Program.Main()` directly, use the console/temp-directory helpers in `Reihitsu.Cli.Test\Helpers`, and are marked `[DoNotParallelize]`.
- The formatter and CLI intentionally leave syntax-invalid or generated code alone. Preserve that behavior when changing formatting flows: the formatter returns the original tree/document for syntax errors or auto-generated source, and the CLI skips generated files such as `.Designer.cs`, `.g.cs`, and `.g.i.cs`.

## Instructions

- The repository language is English. All communication, documentation, and code should be in English, even if user input is in another language.
- When drafting pull requests, keep the description concise, link related issues explicitly (for example `Closes #123`), and do not include a list of locally executed tests because CI already reports that information.

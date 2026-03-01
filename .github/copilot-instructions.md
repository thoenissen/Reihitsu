# Copilot Instructions for Reihitsu

## Project Overview

Reihitsu is a Roslyn-based C# analyzer and code fix provider, distributed as a NuGet analyzer package. It enforces coding rules across categories: Clarity, Design, Naming, Formatting, Documentation, and Performance.

## Solution Structure

- `Reihitsu.Analyzer` — Analyzer library (targets `netstandard2.0`). Contains diagnostic analyzers organized under `Rules/<Category>/`.
- `Reihitsu.Analyzer.CodeFixes` — Code fix providers (targets `netstandard2.0`). Mirrors the `Rules/<Category>/` structure.
- `Reihitsu.Analyzer.Test` — MSTest test project (targets `net10.0`). Tests organized by category under `<Category>/`.
- `Reihitsu.Analyzer.Package` — NuGet packaging project.

## Target Frameworks & Language

- Analyzer and CodeFixes projects target **.NET Standard 2.0** (required for Roslyn analyzer compatibility).
- Test project targets **.NET 10**.
- `LangVersion` is set to `latest` via `Directory.Build.props`.
- Do **not** change target frameworks or `LangVersion` unless explicitly asked.

## Coding Conventions

### General Style

- Use **file-scoped namespaces** (`namespace X;`).
- Use **`#region` / `#endregion`** blocks to organize members (Fields, Constants, Constructor, Methods, overrides section named after the base type). Region descriptions must start with an uppercase letter, and `#region` and `#endregion` descriptions must match.
- Use `global using` directives in `Properties/GlobalUsings.cs` for common namespaces.
- XML doc comments are required on all public and protected members.
- Use `<inheritdoc/>` for overridden/implemented members.
- Prefer `== false` over the `!` operator for boolean negation (enforced by RH0001).
- Do not use expression-bodied methods or constructors (enforced by RH0325/RH0326).
- Statements (`if`, `try`, `return`, `foreach`, etc.) should be preceded by a blank line unless they are the first statement in a block.
- Private fields use camelCase with an `_` prefix (e.g., `_rule`).
- Method parameters and local variables use camelCase.
- All type names, public/protected/internal members use PascalCase.

### Analyzer Conventions

- Each diagnostic rule has a unique ID in format `RH####` (e.g., `RH0001`).
- Analyzer classes inherit from `DiagnosticAnalyzerBase<T>` or a category-specific base (e.g., `CasingAnalyzerBase<T>`, `StatementShouldBePrecededByABlankLineAnalyzerBase<T>`).
- Analyzer class naming: `RH####<Description>Analyzer` (e.g., `RH0001NotOperatorShouldNotBeUsedAnalyzer`).
- Code fix class naming: `RH####<Description>CodeFixProvider` (e.g., `RH0001NotOperatorShouldNotBeUsedCodeFixProvider`).
- Diagnostic IDs are exposed as `public const string DiagnosticId` on the analyzer.
- Title and message format strings are stored in `AnalyzerResources.resx` with keys like `RH####Title` and `RH####MessageFormat`.
- Code fix title strings are stored in `CodeFixResources.resx`.
- Analyzers must be decorated with `[DiagnosticAnalyzer(LanguageNames.CSharp)]`.
- Code fix providers must be decorated with `[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(...))]` and `[Shared]`.

### Diagnostic Categories

Categories are defined in `DiagnosticCategory` enum: `Clarity`, `Design`, `Naming`, `Formatting`, `Documentation`, `Performance`.

ID ranges by category:
- `RH00xx` — Clarity
- `RH01xx` — Design
- `RH02xx` — Naming
- `RH03xx` — Formatting
- `RH04xx` — Documentation
- `RH05xx` — Performance

## Testing

- Uses **MSTest** (`[TestClass]`, `[TestMethod]`).
- Test classes inherit from `AnalyzerTestsBase<TAnalyzer>` (analyzer-only) or `AnalyzerTestsBase<TAnalyzer, TCodeFix>` (analyzer + code fix).
- Test class naming: `RH####<Description>AnalyzerTests`.
- Diagnostic verification uses the `Verify` and `Diagnostics` helper methods from the base class.

### Test Data

- **Never pass source code as inline strings in tests.** All test data (source to analyze, expected fix results) must be stored as separate `.cs` files.
- Test data files are placed under `<Category>/Resources/` and follow the naming convention `RH####.TestData.cs` (input) and `RH####.ResultData.cs` (expected output after code fix).
- These `.cs` files are **not compiled** — they must be excluded from compilation via `<Compile Remove="..."/>` entries in the test project's `.csproj`.
- The files are referenced as `ResXFileRef` entries in the category's `TestData.resx`, which makes them available at runtime as strings through the auto-generated `TestData` class (e.g., `TestData.RH0001TestData`, `TestData.RH0001ResultData`).
- Do **not** pass source code as inline strings in tests.
- When adding a new rule's test data:
  1. Create the `.cs` data file(s) under `<Category>/Resources/`.
  2. Add a `<Compile Remove="<Category>\Resources\RH####.TestData.cs" />` (and `.ResultData.cs` if applicable) entry to the `.csproj`.
  3. Add a `<None Include="<Category>\Resources\RH####.TestData.cs" />` (and `.ResultData.cs` if applicable) entry to the `.csproj`.
  4. Add a `ResXFileRef` entry in the corresponding `TestData.resx` pointing to the new file.

## Build & Quality

- StyleCop is configured via `stylecop.json` and ruleset files.
- The Reihitsu analyzer itself is applied to the solution (self-referencing via `Directory.Build.props`).
- SonarCloud is used for quality gating.
- Do not edit auto-generated files (`*.Designer.cs`).

## README

- The `README.MD` contains a table of all diagnostic rules with their ID, description, and analyzer/code fix availability.
- When adding, removing, or modifying a rule, **always update the README.MD** rule table to keep it in sync.

## Dependencies

- `Microsoft.CodeAnalysis.CSharp` and `Microsoft.CodeAnalysis.Analyzers` — Roslyn SDK for analyzer development.
- `System.Text.Json` — used for configuration file parsing.
- `StyleCop.Analyzers` — additional code style enforcement.

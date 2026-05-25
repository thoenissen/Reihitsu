# Reihitsu Project Overview

Roslyn-based C# analyzer and code fix provider (NuGet analyzer package).

## Tech Stack
- C# / .NET Standard 2.0 (Analyzer, CodeFixes), .NET 10 (Tests)
- Roslyn SDK (`Microsoft.CodeAnalysis.CSharp`)
- MSTest for testing
- StyleCop for style enforcement

## Solution Structure
- `Reihitsu.Analyzer` — Analyzers (netstandard2.0)
- `Reihitsu.Analyzer.CodeFixes` — Code fix providers (netstandard2.0)
- `Reihitsu.Formatter` — Shared formatter engine
- `Reihitsu.Cli` — `reihitsu-format` .NET tool
- `Reihitsu.Core` — Shared Roslyn syntax and ordering utilities
- `Reihitsu.Analyzer.Test` — Analyzer MSTest project (net10.0)
- `Reihitsu.Formatter.Test` — Formatter MSTest project (net10.0)
- `Reihitsu.Core.Test` — Core MSTest project (net10.0)
- `Reihitsu.Cli.Test` — CLI MSTest project (net10.0)
- `Reihitsu.Analyzer.Package` — NuGet packaging
- `documentation/rules/` — Per-rule documentation (RH####.md)

## Key Base Classes
- `DiagnosticAnalyzerBase<TAnalyzer>` in `Base/`
- `CasingAnalyzerBase<T>`, `StatementShouldBePrecededByABlankLineAnalyzerBase`, etc.

## Diagnostic ID Ranges
- RH00xx: Clarity, RH01xx: Design, RH02xx: Naming, RH03xx: Formatting, RH04xx: Documentation, RH05xx: Performance

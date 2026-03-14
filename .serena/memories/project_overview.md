# Reihitsu Project Overview

Roslyn-based C# analyzer and code fix provider (NuGet analyzer package).

## Tech Stack
- C# / .NET Standard 2.0 (Analyzer, CodeFixes), .NET 10 (Tests)
- Roslyn SDK (`Microsoft.CodeAnalysis.CSharp`)
- MSTest for testing
- StyleCop for style enforcement

## Solution Structure
- `Reihitsu.Analyzer/Reihitsu.Analyzer` — Analyzers (netstandard2.0)
- `Reihitsu.Analyzer/Reihitsu.Analyzer.CodeFixes` — Code fix providers (netstandard2.0)
- `Reihitsu.Analyzer/Reihitsu.Analyzer.Test` — MSTest tests (net10.0)
- `Reihitsu.Analyzer/Reihitsu.Analyzer.Package` — NuGet packaging
- `documentation/rules/` — Per-rule documentation (RH####.md)

## Key Base Classes
- `DiagnosticAnalyzerBase<TAnalyzer>` in `Base/`
- `CasingAnalyzerBase<T>`, `StatementShouldBePrecededByABlankLineAnalyzerBase`, etc.

## Diagnostic ID Ranges
- RH00xx: Clarity, RH01xx: Design, RH02xx: Naming, RH03xx: Formatting, RH04xx: Documentation, RH05xx: Performance

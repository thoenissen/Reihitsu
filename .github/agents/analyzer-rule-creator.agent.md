---
name: analyzer-rule-creator
description: Create a new Reihitsu analyzer rule with the required tests, documentation, and packaging updates.
---

# Analyzer Rule Creator

You create new analyzer rules for Reihitsu.

## Core responsibility

Implement a new analyzer rule end to end, including tests and all required rule documentation wiring.

## Mandatory workflow

1. Understand the requested rule intent, scope, category, and expected developer experience.
2. Implement the analyzer with repository-consistent naming, resources, and diagnostics.
3. Implement a code fix **only** when it can be delivered comprehensively and confidently. If only light or partial support would be possible, do **not** add a code fix.
4. Before running tests, run `Reihitsu.Cli` over the changed paths so formatting issues are corrected first. Use a repository-local invocation such as:

   ```shell
   dotnet run --project Reihitsu.Cli -- <changed-path-1> [<changed-path-2> ...]
   ```

5. Do not consider the work complete until all relevant unit tests pass.

## Required checklist

- Positive tests for the new rule
- Negative tests for the new rule
- Code-fix tests when a code fix is implemented
- Rule markdown created under `documentation/rules/RH####.md`
- Rule markdown added to `Reihitsu.Analyzer.Package/README.MD`
- Rule markdown added to `Reihitsu.sln`

## Repository-specific guidance

- Follow the existing `AnalyzerTestsBase<TAnalyzer>` or `AnalyzerTestsBase<TAnalyzer, TCodeFix>` patterns.
- Prefer many small, focused analyzer tests over one large test with many cases.
- Use Roslyn markup such as `{|#0:...|}` for expected diagnostic locations.
- If the rule needs user-facing rule documentation, use the `analyzer-rule-md` skill.
- If the rule needs new localized strings, use the `resource-texts` skill.
- Formatting-aware code fixes should delegate final layout to `ReihitsuFormatter.FormatNodeInDocumentAsync` or `ReihitsuFormatter.FormatNode` instead of manually editing trivia.

## Validation

Run the relevant validation commands after formatting the changed files:

```shell
dotnet test Reihitsu.Analyzer.Test/Reihitsu.Analyzer.Test.csproj -c Release --verbosity minimal
dotnet build Reihitsu.sln -c Release --verbosity minimal
```

If the change touches formatter-coupled code-fix behavior, also run:

```shell
dotnet test Reihitsu.Formatter.Test/Reihitsu.Formatter.Test.csproj -c Release --verbosity minimal
```

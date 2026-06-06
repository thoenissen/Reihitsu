# Create Analyzer Rule

Create a new Reihitsu analyzer rule with the required tests, documentation, and packaging updates.

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
- Rule markdown created under `documentation/rules/RH####.md` (see rule doc format below)
- Rule markdown added to `Reihitsu.Analyzer.Package/README.MD`
- Rule markdown added to `Reihitsu.sln`
- New localized strings added to `AnalyzerResources.resx` and `AnalyzerResources.cs` (see resource text guidance below)
- Code-fix strings added to `CodeFixResources.resx` and `CodeFixResources.cs` when a code fix is implemented

## Repository-specific guidance

- Follow the existing `AnalyzerTestsBase<TAnalyzer>` or `AnalyzerTestsBase<TAnalyzer, TCodeFix>` patterns.
- Prefer many small, focused analyzer tests over one large test with many cases.
- Use Roslyn markup such as `{|#0:...|}` for expected diagnostic locations.
- Formatting-aware code fixes should delegate final layout to `ReihitsuFormatter.FormatNodeInDocumentAsync` or `ReihitsuFormatter.FormatNode` instead of manually editing trivia.

## Rule markdown format

Write user-facing documentation under `documentation/rules/RH####.md`. Goal: help a developer understand what the rule means, why it exists, how to fix a violation, and what correct code looks like.

```md
# RH#### — Rule title

| Property | Value |
|----------|-------|
| **ID** | RH#### |
| **Category** | Analyzer/Performance/Design/Clarity/Naming/Layout/Spacing/Organization/Documentation |
| **Severity** | Warning |
| **Code Fix** | ✓ or ❌ |

## Description

Short explanation of what the rule enforces.

## Why is this a problem?

Explain the readability, maintainability, correctness, or consistency problem from a user perspective.

## How to fix it

Give direct, practical advice.

## Examples

### Violation

\`\`\`cs
// violating example
\`\`\`

### Correction

\`\`\`cs
// corrected example
\`\`\`
```

Keep the tone concise and practical. Write for **users of the analyzer**, not for analyzer maintainers. Do not include Roslyn API choices, syntax kinds, semantic model discussion, or any implementation-internal content.

## Resource text guidance

When adding new strings:

1. Add `RH####Title` and `RH####MessageFormat` to `Reihitsu.Analyzer\AnalyzerResources.resx`.
2. Add matching properties to `Reihitsu.Analyzer\AnalyzerResources.cs`:

   ```cs
   internal static string RH####Title => GetString(nameof(RH####Title));
   internal static string RH####MessageFormat => GetString(nameof(RH####MessageFormat));
   ```

3. If a code fix is included, add `RH####Title` to `Reihitsu.Analyzer.CodeFixes\CodeFixResources.resx` and a matching property to `CodeFixResources.cs`.
4. Do **not** create or restore `.Designer.cs` files — wrapper classes are handwritten.

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

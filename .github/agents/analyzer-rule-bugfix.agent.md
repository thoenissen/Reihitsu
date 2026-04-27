---
name: analyzer-rule-bugfix
description: Fix a Reihitsu analyzer rule bug by reproducing it in tests first and then implementing the correction.
---

# Analyzer Rule Bug Fix

You fix bugs in existing Reihitsu analyzer rules or their code fixes.

## Core responsibility

Reproduce the reported analyzer problem first, then implement the fix without weakening unrelated analyzer behavior.

## Mandatory workflow

1. Reproduce the bug in a unit test before changing production code.
2. Once the failure is captured, implement the analyzer or code-fix correction.
3. Before running tests, run `Reihitsu.Cli` over the changed paths so formatting issues are corrected first. Use a repository-local invocation such as:

   ```shell
   dotnet run --project Reihitsu.Cli -- <changed-path-1> [<changed-path-2> ...]
   ```

4. Do not consider the work complete until all relevant unit tests pass.

## Required checklist

- A unit test that fails before the fix and passes after the fix
- The production fix for the analyzer or code fix
- Updated or expanded positive and negative coverage when needed to protect surrounding behavior

## Repository-specific guidance

- Keep the fix scoped to the reported analyzer behavior unless tightly coupled logic also requires adjustment.
- Prefer many small, focused analyzer tests over one large test with many cases.
- If the bug affects a code fix, prefer the formatter-backed code-fix pattern already used in the repository.
- If the user-facing rule documentation becomes inaccurate because of the fix, update it as part of the change.

## Validation

Run the relevant validation commands after formatting the changed files:

```shell
dotnet test Reihitsu.Analyzer.Test/Reihitsu.Analyzer.Test.csproj -c Release --verbosity minimal
dotnet build Reihitsu.sln -c Release --verbosity minimal
```

If the fix touches formatter-coupled code-fix behavior, also run:

```shell
dotnet test Reihitsu.Formatter.Test/Reihitsu.Formatter.Test.csproj -c Release --verbosity minimal
```

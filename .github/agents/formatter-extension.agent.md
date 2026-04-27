---
name: formatter-extension
description: Extend the Reihitsu formatter with tests and validation that match repository workflow expectations.
---

# Formatter Extension

You extend the Reihitsu formatter.

## Core responsibility

Implement a new formatter behavior or formatting rule while preserving the existing formatter pipeline and repository conventions.

## Mandatory workflow

1. Understand the requested formatting behavior and identify the relevant formatter pipeline phase or contributor.
2. Implement the formatter change using existing helpers and conventions where possible.
3. Before running tests, run `Reihitsu.Cli` over the changed paths so formatting issues are corrected first. Use a repository-local invocation such as:

   ```shell
   dotnet run --project Reihitsu.Cli -- <changed-path-1> [<changed-path-2> ...]
   ```

4. Do not consider the work complete until all relevant unit tests pass.

## Required checklist

- Positive tests for the new implementation
- Negative tests for the new implementation

## Repository-specific guidance

- Preserve existing behavior for syntax-invalid and generated code.
- Prefer extending the existing formatter pipeline instead of adding parallel formatting paths.
- Keep test coverage aligned with the current formatter test structure, including unit, regression, or idempotency coverage when relevant.

## Validation

Run the relevant validation commands after formatting the changed files:

```shell
dotnet test Reihitsu.Formatter.Test/Reihitsu.Formatter.Test.csproj -c Release --verbosity minimal
dotnet build Reihitsu.sln -c Release --verbosity minimal
```

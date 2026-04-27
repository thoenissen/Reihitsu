---
name: formatter-bugfix
description: Fix a Reihitsu formatter bug by adding a regression test first and then implementing the correction.
---

# Formatter Bug Fix

You fix bugs in the Reihitsu formatter.

## Core responsibility

Capture the regression first, then correct the formatter behavior without breaking existing formatting flows.

## Mandatory workflow

1. Add a regression test first so the formatter bug is reproducible before the fix.
2. Implement the formatter correction once the regression is captured.
3. After the correction, add further unit tests if they improve coverage around the fixed behavior.
4. Before running tests, run `Reihitsu.Cli` over the changed paths so formatting issues are corrected first. Use a repository-local invocation such as:

   ```shell
   dotnet run --project Reihitsu.Cli -- <changed-path-1> [<changed-path-2> ...]
   ```

5. Do not consider the work complete until all relevant unit tests pass.

## Required checklist

- A regression test that fails before the fix and passes after the fix
- The production fix for the formatter behavior
- Additional unit coverage when it meaningfully protects the corrected behavior

## Repository-specific guidance

- Preserve existing behavior for syntax-invalid and generated code.
- Keep the fix aligned with the current formatter pipeline instead of introducing separate formatting paths.
- Add regression coverage in the most appropriate formatter test area for the issue being fixed.

## Validation

Run the relevant validation commands after formatting the changed files:

```shell
dotnet test Reihitsu.Formatter.Test/Reihitsu.Formatter.Test.csproj -c Release --verbosity minimal
dotnet build Reihitsu.sln -c Release --verbosity minimal
```

# Reihitsu — Codex Instructions

## Build, test, and lint

```shell
dotnet build Reihitsu.sln -c Release --verbosity minimal
```

```shell
dotnet test Reihitsu.Analyzer.Test/Reihitsu.Analyzer.Test.csproj -c Release --verbosity minimal
dotnet test Reihitsu.Formatter.Test/Reihitsu.Formatter.Test.csproj -c Release --verbosity minimal
dotnet test Reihitsu.Core.Test/Reihitsu.Core.Test.csproj -c Release --verbosity minimal
dotnet test Reihitsu.Cli.Test/Reihitsu.Cli.Test.csproj -c Release --verbosity minimal
```

Single-test examples:

```shell
dotnet test Reihitsu.Analyzer.Test/Reihitsu.Analyzer.Test.csproj -c Release --no-build --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Formatting.RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzerTests.VerifyExpressionBodiedMethodsAreDetectedAndFixed"
dotnet test Reihitsu.Formatter.Test/Reihitsu.Formatter.Test.csproj -c Release --no-build --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Unit.Indentation.LayoutComputerTests.ComputeReturnsNonEmptyModelForSimpleClass"
dotnet test Reihitsu.Core.Test/Reihitsu.Core.Test.csproj -c Release --no-build --filter "FullyQualifiedName~Reihitsu.Core.Test.CasingUtilitiesTests.ToCamelCaseTest"
dotnet test Reihitsu.Cli.Test/Reihitsu.Cli.Test.csproj -c Release --no-build --filter "FullyQualifiedName~Reihitsu.Cli.Test.Unit.ProgramTests.ParseArgumentsUnknownOptionReturnsUnknownOption"
```

## Supported environments

- The instructions support Linux cloud and local Windows environments. Use the host-appropriate shell and forward-slash paths in repository documentation and `dotnet` commands.
- The required .NET SDK is preinstalled. Before builds or tests, run `dotnet --list-sdks`; never install an SDK, modify `PATH`, or otherwise change the environment.
- The authenticated `gh` CLI is available in both supported Codex environments. Use it for GitHub platform operations. Do not assume `pwsh` is installed in the Linux cloud environment.
- Do not run PowerShell scripts under `scripts/` in the Linux cloud environment.
- In local Windows sandbox environments, Git can report "detected dubious ownership" because the sandbox runs under a different Windows account. In that case, pass `-c safe.directory=<repository-root>` to the Git command; do not modify global Git configuration.
- If `gh` reports that its configuration cannot be read because of access restrictions, rerun the command with the required elevated permission. Do not reauthenticate or copy tokens into the repository.
- Before switching branches in a multi-worktree checkout, run `git worktree list`. A branch can be checked out in only one worktree at a time; use another branch or a detached checkout when the intended branch is already in use.

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

## Codex command playbooks

The repository defines Codex-oriented command playbooks under `.codex/commands`:

| Command | Purpose |
|---|---|
| `/create-analyzer-rule` | Create a new analyzer rule end to end |
| `/fix-analyzer-rule` | Fix a bug in an existing analyzer rule |
| `/extend-formatter` | Extend the formatter with new behavior |
| `/fix-formatter` | Fix a formatter bug |
| `/create-rule-doc` | Write or update rule markdown under `documentation/rules/` |
| `/draft-issue` | Create issue drafts in `plans/issues/` |
| `/add-resource-texts` | Add or update localized resource strings |
| `/gh-rubber-duck` | Derive the read-only Behavior Contract for an issue or pull request before implementing |
| `/gh-implement` | Implement a GitHub issue end to end, starting with the mandatory Behavior Contract gate |
| `/gh-preflight` | Run the read-only quality gate before external review |
| `/gh-review` | Review a GitHub pull request |
| `/gh-apply-review` | Apply review findings in the pull request author's task |
| `/gh-rereview` | Re-review a pull request after findings were addressed |

Use the command playbook that matches the task so the repository-specific workflow and checklist are applied from the start.

## GitHub workflow stages

Five distinct activities. Keep the names apart — calling all of them "review" is what causes analysis to be skipped and gates to be repeated:

- **Rubber Duck analysis** (`/gh-rubber-duck`) — read-only requirements and design pass that produces the **Behavior Contract**: user-visible examples, behavior rows, anchor and trivia rules, the counterpart map, an adversarial matrix, non-goals, and any decision the user must settle. It changes nothing. `gh-implement` runs it automatically in a dedicated read-only subagent before its first edit; `gh-apply-review` may use it when review feedback introduces a materially ambiguous behavior change.
- **Local self-review** — the implementing agent's own check of its change against every Behavior Contract row or review-worklist row, plus parity, defect-class closure, convergence, idempotency, directives, documentation, formatting, and focused tests. It is not official and needs no extra agent.
- **Official preflight** (`/gh-preflight`) — the fresh, independent, read-only quality gate on the final head. `gh-implement` and `gh-apply-review` get one attempt plus one **preflight retry** after a single consolidated repair cycle; a third attempt requires explicit user direction.
- **Full validation** — the solution build plus the four test projects, run once on the audited head.
- **CI** — triggered by the run's single non-`[skip ci]` push at the very end.

Sequencing rule: merge current `origin/main` into the working branch **before** the official preflight, so the audited head is the head that merges.

## GitHub issue ownership

For GitHub issue implementation, check the issue for an existing claim or open draft PR before editing. If unclaimed, create `codex/issue-<N>-<short-slug>` from the current `origin/main`, add an empty `Claim issue #<N>` commit, push it, and immediately open a draft PR whose initial body describes the planned work and includes `Closes #<N>`. The linked draft PR is the source of truth for ownership; do not post claim or PR-link comments on the issue. After the first focused implementation commit is pushed, update the PR body to describe the actual changes. Keep the PR draft through the required validation; the human reviewer marks it ready.

## High-level architecture

- `Reihitsu.Formatter` is the shared formatting engine. `ReihitsuFormatter` is the public entry point, and `Pipeline/FormattingPipeline` applies phases in this order: structural transforms, region formatting, blank lines, line breaks, switch-case braces, horizontal spacing, indentation/alignment, raw-string alignment, cleanup.
- `Reihitsu.Cli` packages the `reihitsu-format` .NET tool. `Program` parses arguments and hands execution to `FormatCommandHandler`, which walks files/directories, skips `bin/`, `obj/`, and generated files, then formats through the shared formatter. `--check` and `--dry-run` are first-class modes, not wrappers around shell diff tools.
- `Reihitsu.Core` contains shared Roslyn syntax and ordering utilities that are reused by analyzers, code fixes, and the formatter.
- `Reihitsu.Analyzer` contains Roslyn analyzers grouped by RH rule IDs and categories. `Reihitsu.Analyzer.CodeFixes` contains the matching code fixes and depends on both the analyzer project and the formatter project.
- `Reihitsu.Analyzer.Package` is the shipping NuGet package. It packs `Reihitsu.Analyzer.dll`, `Reihitsu.Analyzer.CodeFixes.dll`, and `Reihitsu.Formatter.dll` together under `analyzers/dotnet/cs`, so analyzer fixes can reuse formatter behavior inside the package.
- Test projects mirror the runtime surfaces: analyzer tests use Roslyn verifier helpers, formatter tests cover unit/regression/idempotency/full-pipeline behavior, Core tests cover the shared utility assembly directly, and CLI tests cover unit, integration, and end-to-end flows.

## Key conventions

- Diagnostic ID ranges carry meaning across the repo: `RH0###` Analyzer, `RH1###` Performance, `RH2###` Design, `RH3###` Clarity, `RH4###` Naming, `RH5###` Layout, `RH6###` Spacing, `RH7###` Organization, `RH8###` Documentation.
- Diagnostic suffix letters such as `A` are reserved for alternative rules; new rules must use their own numeric IDs.
- Rule documentation is part of the implementation contract. `DiagnosticAnalyzerBase<TAnalyzer>` builds each diagnostic `helpLinkUri` as `documentation/rules/RH####.md`, so new or renamed rules should keep the matching rule doc in sync.
- Formatting-aware code fixes should delegate final layout to `ReihitsuFormatter.FormatNodeInDocumentAsync` (or `FormatNode` for detached nodes) instead of editing trivia manually.
- Analyzer tests follow the `AnalyzerTestsBase<TAnalyzer>` / `AnalyzerTestsBase<TAnalyzer, TCodeFix>` pattern and use Roslyn markup like `{|#0:...|}` for expected diagnostic locations.
- CLI end-to-end tests call `Program.Main()` directly, use the console/temp-directory helpers in `Reihitsu.Cli.Test/Helpers`, and are marked `[DoNotParallelize]`.
- The formatter and CLI intentionally leave syntax-invalid or generated code alone. Preserve that behavior when changing formatting flows: the formatter returns the original tree/document for syntax errors or auto-generated source, and the CLI skips generated files such as `.Designer.cs`, `.g.cs`, and `.g.i.cs`.

## Instructions

- The repository language is English. All communication, documentation, and code should be in English, even if user input is in another language.
- When drafting pull requests, keep the description concise, link related issues explicitly (for example `Closes #123`), and do not include a list of locally executed tests because CI already reports that information.

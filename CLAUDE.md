# Reihitsu — Claude Instructions

## Build, test, and lint

```powershell
dotnet build Reihitsu.sln -c Release --verbosity minimal
```

```powershell
dotnet test Reihitsu.Analyzer.Test\Reihitsu.Analyzer.Test.csproj -c Release --verbosity minimal
dotnet test Reihitsu.Formatter.Test\Reihitsu.Formatter.Test.csproj -c Release --verbosity minimal
dotnet test Reihitsu.Core.Test\Reihitsu.Core.Test.csproj -c Release --verbosity minimal
dotnet test Reihitsu.Cli.Test\Reihitsu.Cli.Test.csproj -c Release --verbosity minimal
```

Single-test examples:

```powershell
dotnet test Reihitsu.Analyzer.Test\Reihitsu.Analyzer.Test.csproj -c Release --no-build --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Formatting.RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzerTests.VerifyExpressionBodiedMethodsAreDetectedAndFixed"
dotnet test Reihitsu.Formatter.Test\Reihitsu.Formatter.Test.csproj -c Release --no-build --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Unit.Indentation.LayoutComputerTests.ComputeReturnsNonEmptyModelForSimpleClass"
dotnet test Reihitsu.Core.Test\Reihitsu.Core.Test.csproj -c Release --no-build --filter "FullyQualifiedName~Reihitsu.Core.Test.CasingUtilitiesTests.ToCamelCaseTest"
dotnet test Reihitsu.Cli.Test\Reihitsu.Cli.Test.csproj -c Release --no-build --filter "FullyQualifiedName~Reihitsu.Cli.Test.Unit.ProgramTests.ParseArgumentsUnknownOptionReturnsUnknownOption"
```

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

## Custom slash commands

The repository defines custom Claude slash commands under `.claude/commands`:

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
| `/gh-implement` | Implement a GitHub issue with an early branch and draft-PR claim, starting with the Behavior Contract gate for behavioral work |
| `/gh-preflight` | Run the read-only quality gate before external review |

Use the command that matches the task so the repository-specific workflow and checklist are applied from the start.

## GitHub workflow stages

Five distinct activities, plus the triage that decides which of them a run needs. Keep the names apart — calling all of them "review" is what causes analysis to be skipped and gates to be repeated:

- **Rubber Duck analysis** (`/gh-rubber-duck`) — read-only requirements and design pass that produces the **Behavior Contract**: user-visible examples, behavior rows, anchor and trivia rules, the counterpart map, an adversarial matrix, non-goals, and any decision the user must settle. Every bug report also gets a decidable defect mechanism, a candidate enumeration derived from the dispatch code, and an executable sweep over every candidate before the gate can return `READY`. It changes no repository or GitHub state. `gh-implement` runs it automatically in a dedicated read-only subagent before its first edit; `gh-apply-review` may use it when review feedback introduces a materially ambiguous behavior change.
- **Scope triage** — the orchestrator's own up-front call on which gates a run needs. A run is *routine* only when it changes no analyzer, formatter, code-fix, or `Reihitsu.Core` behavior, admits exactly one reading, stays inside a small listable file set, adds no rule and changes no diagnostic, public API, or dependency, and is not an analyzer or formatter bug report. A mechanical veto comes first: any diff touching `Reihitsu.*` source, a test project, `*.csproj`, `Directory.Build.props`, a ruleset, or `.editorconfig` — or mixing those with documentation — is behavioral. A routine run may replace the Rubber Duck subagent with a short contract note, and skips both the official preflight and the full validation, because no compiled file changed and the human PR review is the gate for text-only work. Any trip-wire voids the classification and restores both gates. The run report always names the classification and the reason.
- **Local self-review** — the implementing agent's own check of its change against every Behavior Contract row or review-worklist row, plus parity, defect-class closure, convergence, idempotency, directives, documentation, formatting, and focused tests. It is not official and needs no extra agent.
- **Official preflight** (`/gh-preflight`) — the fresh, independent, read-only quality gate on the final tree. Its report explicitly audits guard scope against the counterpart predicate, enumerates every owner of a changed policy, and states the falsifiable invariant of every new or materially changed test. `gh-implement` and `gh-apply-review` get one attempt plus one **preflight retry** after a single consolidated repair cycle; a third attempt requires explicit user direction. Everything that compiles goes through at least one attempt.
- **Full validation** — the solution build plus the four test projects, run once on the audited tree. A run whose diff contains no compiled file skips it. Any change to a compiled file invalidates the build, the earlier project results, and the preflight alike.
- **CI** — triggered by the run's single non-`[skip ci]` push at the very end.

Sequencing rule: merge current `origin/main` into the working branch **before** the official preflight, so the audited tree is the tree that merges. The empty CI-trigger commit afterwards changes the SHA but not the tree; `git diff --exit-code <audited-sha> HEAD` is the proof.

Review-repair scope rule: `gh-apply-review` records the PR's decidable defect mechanism or accepted requirement boundary and freezes it after the complete worklist is classified. A confirmed item stays in the PR only when it is the same mechanism/requirement or PR-introduced, is a bug fix rather than new behavior, and changes no shipped diagnostic, public API, or dependency. Every other confirmed item becomes an English copy-ready follow-up draft in the final author-chat response plus an ignored local recovery cache. The user approves its exact content before the GitHub MCP server creates the issue; `/gh-*` workflows never invoke `scripts/upload-issues.ps1`.

## High-level architecture

- `Reihitsu.Formatter` is the shared formatting engine. `ReihitsuFormatter` is the public entry point, and `Pipeline\FormattingPipeline` applies phases in this order: structural transforms, region formatting, blank lines, line breaks, switch-case braces, horizontal spacing, indentation/alignment, raw-string alignment, cleanup.
- `Reihitsu.Cli` packages the `reihitsu-format` .NET tool. `Program` parses arguments and hands execution to `FormatCommandHandler`, which walks files/directories, skips `bin\`, `obj\`, and generated files, then formats through the shared formatter. `--check` and `--dry-run` are first-class modes, not wrappers around shell diff tools.
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
- CLI end-to-end tests call `Program.Main()` directly, use the console/temp-directory helpers in `Reihitsu.Cli.Test\Helpers`, and are marked `[DoNotParallelize]`.
- The formatter and CLI intentionally leave syntax-invalid or generated code alone. Preserve that behavior when changing formatting flows: the formatter returns the original tree/document for syntax errors or auto-generated source, and the CLI skips generated files such as `.Designer.cs`, `.g.cs`, and `.g.i.cs`.

## Instructions

- The repository language is English. All communication, documentation, and code should be in English, even if user input is in another language.
- When drafting pull requests, keep the description concise, link related issues explicitly (for example `Closes #123`), and do not include a list of locally executed tests because CI already reports that information.

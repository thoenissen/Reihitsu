# Reihitsu — Codex Instructions

## Build, test, and lint

The repository scripts under `scripts/` are the canonical entry points. Codex runs on Windows, so use the `.ps1` variants, and pass `-NoInstall` so a missing SDK fails instead of being installed — the required SDK is preinstalled and the scripts must not change it. See `scripts/README.md`.

```powershell
.\scripts\prepare.ps1 -NoInstall   # verify the preinstalled toolchain
.\scripts\build.ps1 -NoInstall     # dotnet build Reihitsu.sln -c Release
.\scripts\test.ps1 -NoInstall      # all four test projects
```

Focused runs:

```powershell
.\scripts\test.ps1 -NoInstall -Project analyzer -Filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Formatting.RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzerTests.VerifyExpressionBodiedMethodsAreDetectedAndFixed"
.\scripts\test.ps1 -NoInstall -Project formatter -Filter "FullyQualifiedName~Reihitsu.Formatter.Test.Unit.Indentation.LayoutComputerTests.ComputeReturnsNonEmptyModelForSimpleClass"
.\scripts\test.ps1 -NoInstall -Project core -Filter "FullyQualifiedName~Reihitsu.Core.Test.CasingUtilitiesTests.ToCamelCaseTest"
.\scripts\test.ps1 -NoInstall -Project cli -Filter "FullyQualifiedName~Reihitsu.Cli.Test.Unit.ProgramTests.ParseArgumentsUnknownOptionReturnsUnknownOption"
```

`scripts/verify-text-only.ps1` proves mechanically that a change carries no compiled behavior — a Roslyn comparison of the token stream, directive and disabled-text structure, literal contents, and changed trivia categories of every changed C# file. The `gh-*` workflows use its verdict to decide whether an expensive audit is required at all.

## Supported environments

- Codex runs on Windows, so use PowerShell and the `.ps1` variants of the repository scripts. The `.sh` variants exist for the Linux-based Claude workflows and are not used here. Keep forward-slash paths in repository documentation and `dotnet` commands.
- The required .NET SDK is preinstalled. Before builds or tests, run `scripts/prepare.ps1 -NoInstall`; it verifies the SDK and changes nothing. Never install an SDK, modify `PATH`, or otherwise change the environment.
- The authenticated `gh` CLI is available. Use it for GitHub platform operations.
- Git can report "detected dubious ownership" because the sandbox runs under a different Windows account. In that case, pass `-c safe.directory=<repository-root>` to the Git command; do not modify global Git configuration.
- If `gh` reports that its configuration cannot be read because of access restrictions, rerun the command with the required elevated permission. Do not reauthenticate or copy tokens into the repository.
- Before switching branches in a multi-worktree checkout, run `git worktree list`. A branch can be checked out in only one worktree at a time; use another branch or a detached checkout when the intended branch is already in use.

## Workflow expectations

- Before running tests, run `Reihitsu.Cli` over the changed paths so formatting issues are corrected first:

```powershell
.\scripts\format.ps1 -NoInstall <changed-path-1> [<changed-path-2> ...]
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
| `/gh-implement` | Implement a GitHub issue end to end, starting with the Behavior Contract gate for behavioral work |
| `/gh-preflight` | Run the read-only quality gate before external review |
| `/gh-review` | Review a GitHub pull request |
| `/gh-apply-review` | Apply review findings in the pull request author's task |
| `/gh-rereview` | Re-review a pull request after findings were addressed |

Use the command playbook that matches the task so the repository-specific workflow and checklist are applied from the start.

## GitHub workflow stages

Five distinct activities, plus the triage that decides which of them a run needs. Keep the names apart — calling all of them "review" is what causes analysis to be skipped and gates to be repeated:

- **Rubber Duck analysis** (`/gh-rubber-duck`) — read-only requirements and design pass that produces the **Behavior Contract**: user-visible examples, behavior rows, anchor and trivia rules, the counterpart map, an adversarial matrix, non-goals, and any decision the user must settle. Every bug report also gets a decidable defect mechanism, a candidate enumeration derived from the dispatch code, and an executable sweep over every candidate before the gate can return `READY`. Every change that moves a guard, predicate, or exemption additionally gets a **guard-delta** and **predicate-boundary** table: the sweep is a *before* analysis, and those tables are the *after* analysis that asks whether every region a decision depends on is still covered once the predicate has moved. Behavior rows cannot express that, which is why a fully green contract could otherwise ship a broken change. It changes no repository or GitHub state. `gh-implement` runs it automatically in a dedicated read-only subagent before its first edit; `gh-apply-review` may use it when review feedback introduces a materially ambiguous behavior change.
- **Scope triage** — the orchestrator's own up-front call on which gates a run needs. A run is *routine* only when it changes no analyzer, formatter, code-fix, or `Reihitsu.Core` behavior, admits exactly one reading, stays inside a small listable file set, adds no rule and changes no diagnostic, public API, or dependency, and is not an analyzer or formatter bug report. A mechanical veto comes first: any diff touching `Reihitsu.*` source, a test project, `*.csproj`, `Directory.Build.props`, a ruleset, `.editorconfig`, `scripts/**`, or a CI workflow — or mixing those with documentation — is behavioral. Touching a `.cs` file is not by itself a reason to audit when only its comments changed; `scripts/verify-text-only.ps1` proves that mechanically with a Roslyn comparison rather than a line-based `grep`, and a rename never qualifies. A routine run may replace the Rubber Duck subagent with a short contract note; it never by itself skips the preflight, which the trigger list below decides, nor the validation, which only a diff without any compiled file skips. Any trip-wire voids the classification and restores the gates. The run report always names the classification and the reason.
- **Local self-review and admission** — the implementing agent's own check of its change against every Behavior Contract row or review-worklist row, plus parity, defect-class closure, boundary closure, convergence, idempotency, directives, documentation, formatting, focused tests, and the XML summary and inline comments of every method whose body changed. It ends in a falsifiable **admission artifact** — qualifiers and their owners, each changed predicate with a test on both sides of its boundary, the exact `rg` result per changed policy owner, each contract or worklist row with its regression test, and each new test's invariant, falsifier, and helper. A missing row blocks admission to preflight, because it is cheap here and expensive in an audit. It is not official and needs no extra agent.
- **Official preflight** (`/gh-preflight`) — the fresh, independent, read-only quality gate on the final tree, and the owner of the trigger list, the gate results, the retry contract, and the evidence-bundle and restart policy. Its report explicitly audits guard scope against the counterpart predicate, enumerates every owner of a changed policy, and states the falsifiable invariant of every new or materially changed test. It is **risk-triggered, not mandatory**: required when the diff changes a predicate, guard, or report condition, which tokens or trivia a rewrite writes, a code-fix registration or applicability, a diagnostic ID, severity, or message, public API, a dependency, a repository script, build property, ruleset, or CI workflow, or adds a rule; not required for a comment, documentation, Markdown, skill, command, or template diff — including inside `.cs` — proven by `scripts/verify-text-only.ps1`, or for tests added to already-correct behavior. An uncertain case goes to the user rather than to a default run. It returns `PASS`, `PASS — non-blocking cleanup` (every remaining finding confined to comments or documentation, fixed by the parent without another attempt), `BLOCKED — findings`, or `BLOCKED — state mismatch`. `gh-implement` and `gh-apply-review` get one attempt plus one **preflight retry**, which is repair-delta aware and audits the repair, its moved guards, and its new boundary tests instead of re-auditing everything; a third attempt requires explicit user direction. Its `Required change` column is a suggestion the parent must re-derive, not a specification.
- **Full validation** — the solution build plus the four test projects, run once on the audited tree. Only a diff that contains no compiled file at all skips it; a skipped audit never skips validation, because tests cost wall-clock rather than tokens and the build is the last thing that catches a malformed comment. Any change to a compiled file invalidates the build and the earlier project results, so both are re-run on the repaired tree. It invalidates the preflight too — unless `scripts/verify-text-only.ps1` proves the change carried no compiled behavior, which keeps the audit valid but never excuses the validation.
- **CI** — triggered by the run's single non-`[skip ci]` push at the very end.

Sequencing rule: merge current `origin/main` into the working branch **before** the official preflight, so the audited tree is the tree that merges. The empty CI-trigger commit afterwards changes the SHA but not the tree; `git diff --exit-code <audited-sha> HEAD` is the proof.

Scope-ledger rule: both author-side workflows freeze scope. `gh-implement` records the accepted mechanism or requirement boundary, the behavior rows, the initial production owners, the intended production and test file set, and the shipped diagnostics, public APIs, and dependencies that must stay unchanged — at contract acceptance, before the first edit. A repair that touches a new behavior owner, a shared helper with other consumers, public API or dependencies, repository-wide canonical policy ownership, a materially enlarged compiled file set, or more than double the intended production paths triggers a mandatory scope checkpoint with three explicit choices: narrow or revert, approve an expanded contract, or create and link a follow-up issue.

Review-repair scope rule: `gh-apply-review` records the PR's decidable defect mechanism or accepted requirement boundary and freezes it after the complete worklist is classified. A confirmed item stays in the PR only when it is the same mechanism/requirement or PR-introduced, is a bug fix rather than new behavior, and changes no shipped diagnostic, public API, or dependency. Every other confirmed item becomes an English copy-ready follow-up draft in the final author-task response plus an ignored local recovery cache. The user approves its exact content before authenticated `gh` creates the issue; `/gh-*` workflows never invoke `scripts/upload-issues.ps1`.

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

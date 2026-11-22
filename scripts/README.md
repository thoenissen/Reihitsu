# Repository scripts

Bash entry points for building, testing, formatting, and proving changes, on Linux and macOS, so the repository instructions, the squad roles, and a developer shell all drive the same commands. The repository ships no PowerShell variants.

Each script resolves the .NET SDK itself through `lib/dotnet-env.sh`: it probes `dotnet --list-sdks` first and installs the required SDK into `$HOME/.dotnet` **only** when it is missing. On an image that already ships the SDK, that step is a no-op and nothing about the environment changes. Pass `--no-install` to turn a missing SDK into a failure instead of an installation — use it in any environment that must not change, such as a sandbox with a preinstalled SDK.

Paths are resolved against **your** working directory, not the repository root: the scripts address the solution, the test projects, and the CLI by absolute path instead of changing directory, so `scripts/format.sh Foo.cs` from a subdirectory means the `Foo.cs` next to you.

| Script | Purpose |
|---|---|
| `prepare.sh` | Verify the toolchain, installing the SDK only if needed |
| `build.sh` | `dotnet build Reihitsu.sln`, in Release unless `--configuration Debug` |
| `test.sh` | Run one test project or the default set ([The default test run](#the-default-test-run)), with an optional focused filter and configuration, under a [hang timeout](#hang-timeout) |
| `format.sh` | Format the given paths through `Reihitsu.Cli` |
| `trace.sh` | Trace source changes at each top-level formatter phase without modifying the input |
| `benchmark.sh` | Time released `reihitsu-format` versions from NuGet on a fixed corpus, in total and per file |
| `apply-fix.sh` | Run one diagnostic's code-fix surface over a fixture directory, under LF and CRLF |
| `verify-text-only.sh` | Prove that a change carries no compiled behavior |
| `lane-check.sh` | Confirm the squad lane a run claimed against the paths its diff changes |
| `usage.sh` | Report the token usage of a Claude Code session and of every subagent it spawned |

## Examples

```bash
scripts/prepare.sh
scripts/build.sh
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH3204"
scripts/test.sh --project selfhosting
scripts/test.sh --no-build --project selfhosting --filter "Name~RH2008|Name~RH2111"
scripts/format.sh src/Reihitsu.Formatter/Pipeline/LineBreaks/LineBreakDetection.cs
scripts/trace.sh samples/Example.cs --passes 3 --no-install
scripts/benchmark.sh --version 1.2.0 --version 1.3.0 --repetitions 5
scripts/apply-fix.sh RH7101 /tmp/sweep
scripts/verify-text-only.sh --base a1b2c3d --head worktree
scripts/lane-check.sh bug-fix --head worktree
scripts/usage.sh --json
```

## Formatter pipeline trace

`trace.sh <file> [--passes N] [--no-install]` runs one C# file through the twelve formatter phase boundaries: the preparation phase's two sub-phases, `PreparationPhase/TabReplacement` and `PreparationPhase/LineEndings`, and the ten phases after it. Paths are resolved relative to the caller, including when the wrapper is invoked outside the repository root. The input file is only read: each pass operates in memory, and the complete result is reparsed before the next pass to match repeated CLI invocations.

The trace defaults to at most three passes and requires a positive pass count. It prints a pass/phase header only when that phase changes serialized source. Line-content changes get a unified diff; when a change only normalizes line-ending separators, the shared diff generator has no line hunk, so the trace prints before/after CRLF, LF, and CR counts instead. It preserves the input's detected LF or CRLF convention and stops early after the first complete pass that makes no change. Syntax-invalid inputs, generated inputs, and inputs that hold code hidden in another build configuration (a multi-line string or delimited comment, started inside a conditional group, that holds a line another build reads as `#elif`, `#else`, or `#endif`; FMT-PIPE-033) are reported as legitimate skips, by the same checks and in the same order as whole-document formatting and `reihitsu-format`; the trace runs no phase for them.

Exit codes are `0` when formatting stabilizes or the input is legitimately skipped, `1` when the source is still changing after the requested pass count, and `2` for invalid arguments, missing files, unavailable tooling, or execution failures. Execution errors identify the active pass and, when available, the active phase.

The implementation lives in `trace/trace.cs` as a [.NET 11 file-based app](https://learn.microsoft.com/en-us/dotnet/core/sdk/file-based-apps) with a repository-only project reference to `Reihitsu.Cli`. Like the text-only proof app, it is outside `Reihitsu.sln`; its local `Directory.Build.props` isolates it from solution packaging and analyzer settings.

## The CLI benchmark

`benchmark.sh [--version <v>]... [--commit <rev>] [--path <repo-path>]... [--repetitions N] [--warmups N] [--top N] [--work <dir>] [--no-perturb] [--roll-forward] [--no-install]` times released `reihitsu-format` versions, installed from NuGet with `dotnet tool install --tool-path`, on one fixed corpus, so a per-file regression between releases can be told apart from a larger file count or slower I/O.

The corpus is the given paths at the given commit, extracted with `git archive`, C# files only; a shallow clone fetches the commit. The defaults are `src/Reihitsu.Formatter` and `src/Reihitsu.Analyzer` at the commit tagged `v1.2.0`, versions `1.2.0` and `1.3.0`, one warm-up and three timed repetitions. Unless `--no-perturb` is given, every `private ` in the corpus becomes `private  `, so most files need rewriting.

Every run formats a fresh copy of the corpus in write mode with `--force --verbose`, one file at a time: a version that offers `--max-parallelism` gets `--max-parallelism 1`, and released versions without it already work sequentially. The versions run interleaved, one repetition of each in turn, so drift of the machine does not land on one version. A file's time is the gap between the tool's verbose line for it and the line before; the first file's gap starts at process start and carries start-up and JIT, so per-file statistics leave it out and report it on its own.

The report gives each version's total (median, minimum, maximum), its start-up plus first file, the sum and median of its per-file medians, the per-file ratio of each version against the first, and the slowest files. The raw per-file timings of every repetition are written to `per-file.csv` in the work directory. Unless `--work` names one, the work directory is a new temporary directory with a random name, readable only by the current user, and it is kept after the run. The working tree is only read; a commit missing from the clone is fetched into it.

Each version runs on the runtime it targets, installed next to the SDK through `lib/dotnet-env.sh` when it is missing and `--no-install` is not given. `--roll-forward` runs every version on the newest installed runtime instead, which separates a change in the code from a change of runtime. Exit codes are `0` when the benchmark ran and `2` for invalid arguments or a tool, network, or setup failure.

The implementation lives in `benchmark/benchmark.cs`, a standalone file-based app outside `Reihitsu.sln` with no project reference, isolated by its local `Directory.Build.props` like the trace app.

## The code-fix fixture runner

`apply-fix.sh <diagnostic-id> <fixture-directory> [--max-iterations N]` exercises **the code-fix surface** — analyzer, then code fix provider, then re-analysis — over every `*.cs` file below a directory. It exists so a defect-class sweep by the skeptic or the test implementer is "write N fixture files and run one command" rather than "build a disposable harness, then write N fixture files"; `format.sh` already reaches the full pipeline, and this closes the surface that had no entry point.

The target is named by **diagnostic ID**, because `FixableDiagnosticIds` and `SupportedDiagnostics` are the only public, stable handles on these types: analyzer and code-fix type names carry no compatibility contract, and the formatter's structural transforms are `internal` and stay that way. An ID no analyzer reports, an ID no provider fixes, or an ID claimed by more than one provider fails before any fixture runs, naming every candidate — the runner never silently picks one.

Fixtures are only read, and paths resolve against the caller's working directory, so a read-only workflow keeps its fixture directory outside the repository and deletes it afterwards. The whole directory is one invocation: process startup is paid once, not once per candidate. Every fixture is run twice, normalized to LF and to CRLF, because the two differ in the trivia the code reads rather than only in the bytes.

Everything after a literal `--` is forwarded to the runner as-is by the wrapper, so a fixture directory that is itself named like an option (`--no-install`, for example) still reaches the runner instead of being consumed as the wrapper's own flag.

Each fixture is analyzed under its own on-disk relative path rather than a constant placeholder, so a rule that reads the document's file name (RH4001, for example) observes the fixture's real identity, and Roslyn's own generated-code heuristic — file name pattern and leading comment alike — is disabled so a fixture that happens to look generated is still analyzed like every other fixture. Each fixture is also the only document in its ad-hoc project, so a code fix that inspects sibling documents (RH4001's own rename-collision guard, for example) never observes another fixture in the sweep. A code fix that replaces the document's identity instead of only its text — RH4001's rename is the shipped example — is followed into the next iteration under its new name; the rename counts as progress on its own, so a rename-only fix still converges rather than being reported as `no-progress`. The arm header always names the fixture's on-disk path, never the renamed document, and an arm whose analyzed document was renamed prints one additional `Document renamed: <before> -> <after>` line.

Each arm prints a header, an optional line-ending note, an optional document-rename note, and the unified diff from the fixture's input to the final text, followed by one quotable `CODE-FIX RUN: …` summary line. The status token is the sweep's observation:

| Status | Meaning |
|---|---|
| `fixed (N action(s), M iteration(s))` | The fix was applied until the diagnostic stopped being reported. `N` above one means the provider offered several actions and the first was taken |
| `no-diagnostic` | The analyzer never reported — the fixture does not reproduce |
| `no-fix-offered` | Reported, but the provider registered no action. A **supported end state**, per the code-fix coverage policy in `AGENTS.md`, not a failure |
| `not-converged (M iteration(s))` | Still reported when the iteration cap was reached |
| `no-progress (M iteration(s))` | An action was applied but changed nothing, so the diagnostic can never clear. Reported apart from the cap because an ineffective fix is a defect in the rule and a cap is not |
| `analyzer-failure` | An analyzer threw. Roslyn reports this as `AD0001` rather than as a failure, so the runner surfaces it explicitly — filtering it away would make a crashed analyzer read as "does not reproduce" |
| `parse-error` | The fixture itself does not parse |
| `invalid-result (M iteration(s))` | The applied fix produced source that no longer parses |
| `[line-ending-drift]` | Suffix on any status whose result stopped using the arm's line ending exclusively — what a fix inserting `Environment.NewLine` instead of the document's own separator looks like |

Convergence is decided by **the diagnostic disappearing**, not by two consecutive passes producing the same text. A fixture carrying the same diagnostic in several places legitimately changes on every iteration while still converging, and a text-equality definition would misreport exactly that case.

Exit codes are `0` when every fixture reached a supported end state, `1` when a fixture never stopped reporting the diagnostic (`not-converged` or `no-progress`), and `2` when the command could not run — bad arguments, an unresolvable ID, a missing or fixture-less directory, an unparseable fixture, a fix producing unparseable source, or an analyzer that threw. `no-diagnostic` and `no-fix-offered` deliberately stay at `0`: every negative sweep row depends on "ran, found nothing" being distinguishable from "could not run".

The implementation lives in `Reihitsu.Tooling`, a non-packable project inside `Reihitsu.sln`, with `scripts/apply-fix/apply-fix.cs` as a thin file-based shim over it. Unlike the trace and proof apps it is a real project, because the runner adds classification behavior — fixture selection, target resolution, convergence, exit-code mapping — that needs test coverage; `Reihitsu.Tooling.Test` provides it through `scripts/test.sh --project tooling`. It needs no formatter internals and widens no public API.

## The default test run

`scripts/test.sh` without `--project`, or with `--project all`, is the default run: it runs every test project of the solution except `Reihitsu.SelfHosting.Test`, the self-hosting scans. The `all)` branch of `test.sh` defines the set, and `TestScriptProjectSelectionTests` in `Reihitsu.ArchitectureTests` keeps it equal to the solution's test projects minus the scans. This section is the one statement of what the default run covers; every other document that names the run links here instead of listing its projects.

`Reihitsu.SelfHosting.Test` runs the repository's own analyzers and formatter over the repository's own source: one test row per analyzer, and one row per source file for formatter idempotency, for formatter stability, and for the stability of the formatted output under each formatter phase run alone. It is a second safety net rather than a requirement check, and it takes long enough that the default run leaves it out. CI runs it on every pull request, and `scripts/test.sh --project selfhosting` runs it locally, narrowed with `--filter` to one analyzer or one file when needed. A violation fails the row of the analyzer or the file that found it and names the file, the line, and the rule.

### The line-count rows

The solution build passes the root `reihitsu.json` to the analyzers it loads from `binaries/` (`Directory.Build.props`), so a Release build — `scripts/build.sh`, and the default run, which builds in Release — fails on the repository's own line limits, RH2008 for a method and RH2111 for a type, with test code judged as test code, as the self-hosting scan does. The build runs the analyzers of the last version bump, the scan those built from source, so a change to either rule reaches the build only with the next rebuild of `binaries/`. The **line-count rows** are the self-hosting rows whose name contains `RH2008` or `RH2111`:

```bash
scripts/test.sh --no-build --project selfhosting --filter "Name~RH2008|Name~RH2111"
```

`--no-build` assumes a Release build of the same tree, as `scripts/build.sh` leaves it. The filter selects every self-hosting row whose name carries one of the two rule IDs: the two analyzer rows, the two ruleset rows of each rule (`ReleaseRulesetReportsRuleAsError`, `DebugRulesetHasNoEntryForLineCountRule`), and the formatter rows of the source files named after the two rules — 18 tests today, none of them a `LineCountSelfHostingTests` method, which name no rule ID. They took 41 seconds after the build in a cloud sandbox, most of it loading the solution, where the rows of every analyzer had not finished after nine minutes. The squad's validator runs them when a run adds lines to a test file ([`squad/roles/validator.md`](../squad/roles/validator.md)); this section is the one statement of which rows they are.

## Hang timeout

`test.sh` runs every test host under a hang timeout of **10 minutes**: when no test in a host starts or finishes for that long, the host is ended and its project fails with `The active Test Run was aborted`, naming the test that was running. A test that blocks forever therefore fails the run about 10 minutes after the last test in its host started or finished — its own start when the host runs tests one at a time, or a later test's when it runs them in parallel — whatever the blocked code checks; a test's own `[Timeout]` with cooperative cancellation stops only code that observes its token. No dump is written. The limit is the VSTest blame collector's (`--blame-hang-timeout 10min --blame-hang-dump-type none`), so it bounds a hang, not the length of a run, and it sits far above the longest single test, the self-hosting rows included.

CI applies the same limit: the build workflow runs `test.sh`, and the SonarCloud workflow, which calls `dotnet test` itself to collect coverage, passes the same two arguments to it. Each job also carries a `timeout-minutes` backstop well above its normal duration, for a hang outside a test host.

## Test-host memory

On Linux, `test.sh` passes every test host the glibc malloc setting `GLIBC_TUNABLES=glibc.malloc.mmap_threshold=131072:glibc.malloc.arena_max=2`, as a run setting (`RunConfiguration.EnvironmentVariables`), so it reaches the test hosts and the processes they start, but not the build or `dotnet test` itself. A caller that sets `GLIBC_TUNABLES` itself keeps its own value, and other platforms are not affected.

Without it, the analyzer test host grows to about 9 GiB over a whole-project run, and a machine with little more memory than that, such as a cloud sandbox, kills it. The growth is native memory the test host has already freed and glibc keeps in its arenas: calling `malloc_trim(0)` in the running host at 7.0 GiB brought it down to 0.7 GiB, while the managed heap stayed near 250 MiB. It does not depend on the worker count, since one worker grew as fast per test as four. With the setting, the same run peaks at about 4.5 GiB and takes as long as before.

## The text-only proof

`verify-text-only` answers one question mechanically: **can this change affect compiled behavior?** The squad uses it through `lane-check`, which runs it before classifying a diff ([The lane check](#the-lane-check) says which C# files it can make text), and to prove that a comment cleanup after a passing audit did not invalidate the audit.

It is syntax-aware rather than line-based, because a `grep` over the diff does not recognize every block-comment form and cannot tell a comment apart from a directive or from comment-looking text inside a string literal. For every changed C# file it parses both versions with Roslyn and proves that

- the non-trivia token stream is unchanged — which also covers string and character literal contents, since those are token text;
- no directive, disabled-text, or skipped-token structure changed;
- every changed syntax element belongs to an allowed comment, documentation, or layout trivia category;
- both versions parse without errors, so a malformed comment cannot slip through.

Generated XML-documentation impact is **reported separately** instead of folded into the verdict, because a documentation edit is exactly what a comment-only change is allowed to be. No assembly of this repository is a public API (`AGENTS.md`, *Supported inputs*), and the analyzer package ships no XML documentation, so a documentation change never fails the proof, whatever the member's accessibility.

A file that is neither C# nor a known non-compiled text path (Markdown, `.claude/**`, `documentation/**`, issue and PR templates) is reported as carrying behavior. Markdown is text wherever it lives, `scripts/**` included, because nothing in it executes, even where it documents what a script does. Every other file under `scripts/**` and `.github/workflows/**` is behavioral regardless of extension: nothing there compiles, but everything there executes. This decides only the proof's verdict; [the lane check](#the-lane-check) still classifies every path under `scripts/**`, Markdown included, as tooling. A renamed file, and an added or deleted C# file, is never text-only.

Two things about `--head worktree`. Both sides are compared with normalized line endings, because git rewrites line endings on checkout under `core.autocrlf` and a raw comparison would otherwise report every multi-line literal and disabled-text region as changed on Windows; a byte order mark is stripped from both sides for the same reason. And untracked files are ignored with a note rather than counted as additions — they are not part of the tree that will merge, so a stray scratch file must not decide the verdict.

`--exclude <path>`, repeatable, leaves one repository-relative path, written with forward slashes and matched exactly and case-insensitively, out of the proof, as if it had not changed; a path that names no changed file excludes nothing. The proof finds renames, so a renamed file is one changed file with an old and a new path, and it is left out only when both paths are excluded: a rename between a wording path and any other file stays in the proof. It exists for [the lane check](#the-lane-check), which gets the wording paths of the diff from the classifier and passes one `--exclude` per path, so a resource-text change does not turn a comment edit next to it into production. When it excluded at least one changed path, the `TEXT-ONLY PROOF: …` line ends with `; excluded: <n> path(s)`, so a quoted `PASS` never reads as covering them.

Exit codes: `0` proven text-only, `1` not proven, `2` tool or setup error. The single `TEXT-ONLY PROOF: …` line is the quotable evidence the workflows record.

The proof itself lives in `proof/verify-text-only.cs` as a standalone [file-based app](https://learn.microsoft.com/dotnet/core/sdk/file-based-programs). It is deliberately outside `Reihitsu.sln`: it ships with no package, is not built by CI, and its local `Directory.Build.props` keeps the solution's versioning, ruleset, and self-hosting analyzer references out of it. Its `#:package` directive pins Roslyn at the repository's one Roslyn version, the `RoslynVersion` property in the root `Directory.Build.props`, so the proof parses C# as the formatter and the analyzers do. It cannot read that property, so the version is written out; the Roslyn-version guard in `Reihitsu.ArchitectureTests` fails when a Roslyn `#:package` directive of a file-based program states any other version, so a change of `RoslynVersion` updates the pin with it.

One consequence is worth knowing: its leading `#:package` directive is not C# the Roslyn parser accepts, so `Reihitsu.Cli` reports the file as syntax-invalid and leaves it untouched, and neither self-hosting test sees it. Keep it to the repository's style by hand.

## The lane check

`lane-check.sh <lane> [--base <rev>] [--head <rev|worktree>] [--no-install]` confirms the lane a squad run claimed — `docs`, `wording`, `tests`, `bug-fix`, `behavior-change`, or `tooling` — against the paths its diff changes. The diff decides, not the claim. The defaults are the merge base with `origin/main` and `HEAD`; `--head worktree` includes uncommitted changes to tracked files and ignores untracked ones, so stage a new file before checking it.

It first runs the text-only proof over the same range, with one `--exclude` for each changed wording path (table below), and prints its line. The script gets those paths from `scripts/lane-check/lane-check.cs --wording-paths`, which reads the diff's `git diff --name-status --no-renames` lines, as below, and writes each path the classifier in `Reihitsu.Tooling` counts as wording, one per line; it takes no other argument. The script holds no path list of its own, so the table below stays the one statement of which paths are wording. The proof runs first because a C# file counts as text only when the proof covers the whole diff apart from the wording paths. Then every changed path (`git diff --name-status --no-renames`, so a rename counts as a deletion and an addition) is classified:

| Category | Paths |
|---|---|
| Change doc | `sdd/changes/**` |
| Text | the rest of `sdd/**`, `squad/notes/**`, `documentation/**`, `.github/ISSUE_TEMPLATE/**`, `.github/PULL_REQUEST_TEMPLATE.md`, every Markdown file at the repository root except `AGENTS.md` and `CLAUDE.md`, `LICENSE`, and, outside `src/Reihitsu.Tooling/`, Markdown under `src/` and `tests/` and C# under `src/` and `tests/` when the proof passed |
| Tooling | `.claude/**`, `squad/**`, `scripts/**`, `.github/**`, `binaries/**`, `src/Reihitsu.Tooling/**`, `AGENTS.md`, `CLAUDE.md`, the solution files, `.gitignore`, `.gitattributes`, the root `reihitsu.json`, and every `*.csproj`, `*.props`, `*.targets`, `*.ruleset`, `.editorconfig` |
| Wording | `src/Reihitsu.Analyzer/AnalyzerResources.resx` and `src/Reihitsu.Analyzer.CodeFixes/CodeFixResources.resx`, the resources that hold the diagnostic titles and messages and the code-fix titles; every other `*.resx` is production |
| Test | files of test projects (`tests/**`) |
| Production | every other file under `src/` |
| Unknown | anything else |

The narrowest lane that admits every category is the lane the diff needs. It is decided by the first of these rules that applies; text is admitted by every lane:

1. An unknown path: no lane.
2. Production: with tooling and without a change doc, no lane; otherwise `behavior-change` with a change doc and `bug-fix` without one. Wording and test paths next to production change nothing.
3. Tooling, without production: with wording, `behavior-change` when the diff has a change doc and no lane when it has none; without wording, `tooling`, with or without a change doc.
4. Wording, without production and tooling: with test paths, `behavior-change` when the diff has a change doc and `tests` when it has none; without test paths, `wording`, with or without a change doc.
5. Otherwise: a change doc needs `behavior-change`, test paths `tests`, and text alone `docs`.

The `wording` lane therefore admits the two resource files, text, and a change doc, and nothing else: a diff that also changes a test needs a wider lane, and these rules decide which. A change doc without a wording path still needs `behavior-change`.

The claim passes when it is that lane or a wider one along docs → wording → tests → bug fix → behavior change, or docs → tests → tooling → behavior change; `tooling` does not cover `wording`, and `wording` covers only `docs`. A `behavior-change` claim additionally needs a change doc in the diff; a `wording` claim needs nothing beyond its paths.

Exit codes are `0` when the claim covers the diff, `1` when it does not (the verdict names the lane the diff needs), `2` for a tool or setup error, and `3` when no lane admits the diff — an unknown path, or production or wording next to tooling without a change doc. The single `LANE CHECK: …` line is the quotable evidence the run report records.

This table is the one statement of the classification, and other documents link here instead of listing paths. A path belongs to the first row that names it, whatever its extension: every file under `squad/notes/` is text while the rest of `squad/**` is tooling, the issue and PR templates are text while the rest of `.github/**` is tooling, a test project's `*.csproj` is tooling rather than a test, and the two resource files are wording rather than production. The classification lives in `Reihitsu.Tooling` (`LaneClassifier`, `LaneCheckCommand`) with tests in `Reihitsu.Tooling.Test`; `scripts/lane-check/lane-check.cs` is a thin file-based shim over it, like the code-fix runner's.

## The usage report

`usage.sh [--session <id|path>] [--projects-dir <dir>] [--json]` reads the transcripts Claude Code writes for a session and prints its token usage: one row for the main session (the squad's lead) and one row per spawned subagent, split by model. It needs `jq` and no .NET SDK. Without `--session` it takes the most recently modified transcript of the current directory's project.

- **Spawns stay apart.** Every subagent has its own transcript, so a role spawned three times is three rows, numbered per agent type in start order. A subagent resumed through `SendMessage` writes to the same transcript and stays one row; its `Prompts` column counts the instructions it received, and `--json` breaks the row down per prompt.
- **Responses are counted once.** The transcript holds one line per content block of an API response, all with the same usage, so the report counts per message id.
- **No prices.** The transcript carries tokens, not costs. Every row names its model so a cost can be computed from the counts.
- **Context growth.** A response's context is its whole prompt: input, cache read, and cache write added together. Every spawn carries the context of its first and of its latest response, over all its prompts and tiers, and the growth between them; the Markdown table shows the growth in its *Context growth* column, repeated on each tier row of a spawn. The lead reads it as the agent's size when it chooses between continuing an agent and spawning a fresh one (`squad/lead.md`, section 4).
- **`--json` holds everything the transcripts record:** cache writes by lifetime, thinking tokens, web search and fetch requests, effort levels, and the first and last message of each model; and per spawn `first_context_tokens`, `last_context_tokens`, `context_growth_tokens`, and `last_response_at`, the time of the spawn's latest response.

Exit codes are `0` when the report is printed and `2` for bad arguments, a missing transcript, or a missing `jq`. The report covers the session up to the moment it runs; a pull request's CI repairs after that are not in it.

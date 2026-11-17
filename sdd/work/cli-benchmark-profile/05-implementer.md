Role: implementer
Commit: 9bd96da
Verdict: tests pass

Repair round 1 of `04-auditor.md`, on top of `dea40f4`. No test handover exists in this lane. The prompt carried the lead's decision on where the installer step lives (decision 4 in `00-lead.md`); that is a lead decision, not a hoped-for verdict, and it was followed. It carried no suspected cause.

## Changes

| File | What changed | Requirement |
|---|---|---|
| `scripts/lib/dotnet-env.sh` | New `reihitsu_run_dotnet_install <args>`: the one owner of downloading `dotnet-install.sh` over https only into a `mktemp` file and running it, with the existing error messages and exit status 2. `reihitsu_ensure_dotnet` calls it with the same arguments as before. New `reihitsu_install_dotnet_runtime <major>`: installs `--runtime dotnet --channel <major>.0` into `REIHITSU_DOTNET_ROOT` and checks afterwards that the runtime is visible, like the SDK path checks the SDK | Finding 2 (and the temporary file of finding 1) |
| `scripts/benchmark/benchmark.cs` | `InstallRuntimeAsync` no longer downloads anything: it sources the helper through `bash -c` and calls `reihitsu_install_dotnet_runtime`; the helper's progress goes to stderr and stdout carries only the install root, so the root's default also has one owner. `--no-install` is still checked in the app before the call, as before. Default work directory: `Directory.CreateTempSubdirectory("reihitsu-benchmark-")` (random name, mode 700, verified). `TrimStart('﻿')`. Header comment: warm-up count is `--warmups` (one by default), runtime install goes through the helper, the default work directory is private and kept. Comment in `ResolveCommitAsync` says any commit missing from the clone is fetched | Findings 1, 2, 4, 5 |
| `scripts/benchmark.sh` | Header: the working tree is only read, a commit missing from the clone is fetched into it; the work directory is a new private temporary directory unless `--work` names one | Finding 3 |
| `scripts/README.md` | *The CLI benchmark*: same correction; the default work directory is random, user-only, and kept; the runtime install goes through `lib/dotnet-env.sh` | Finding 3, auditor hint on the kept directory |

Per finding:

1. **Predictable temporary paths.** The default work directory is now created by `Directory.CreateTempSubdirectory`, so another user cannot pre-create it or plant a `tools/<version>/reihitsu-format` in it. The installer no longer has a path of its own in the app; it is the helper's `mktemp` file. A directory named with `--work` keeps reusing installed tools, as documented — that directory is the user's choice.
2. **Duplicated installer step.** One owner, `reihitsu_run_dotnet_install` in `scripts/lib/dotnet-env.sh`; `rg -n "dotnet-install" scripts` now finds the URL only there. The existing scripts' behavior is unchanged: same messages, same exit status, same installer arguments (verified below).
3. **"The repository is only read."** Rewritten in both places as derived from `ResolveCommitAsync`: the working tree is only read, a commit missing from the clone is fetched into it.
4. **Warm-up comment.** Now names `--warmups` with one as the default.
5. **Invisible U+FEFF.** Spelled `'﻿'`.

## Policy owners

- Downloading and running `dotnet-install.sh`: `rg -n "dotnet-install" scripts` → `scripts/lib/dotnet-env.sh` only (`reihitsu_run_dotnet_install`), plus mentions in comments of `benchmark.cs` that name the helper. One owner.
- The install root `REIHITSU_DOTNET_ROOT` and its default: `rg -n "REIHITSU_DOTNET_ROOT" scripts` → defined in `scripts/lib/dotnet-env.sh:15`; `benchmark.sh` exports it; the app reads it back from the helper's output. One owner.
- Runtime presence check (`Microsoft.NETCore.App <major>.`): `benchmark.cs` decides whether to install (via the `dotnet` on PATH), the helper checks the result in the root it installed into. Two different questions, not a copy.

## Production diff size

`git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'`: empty. No new type. Round delta (`git diff --shortstat dea40f4 9bd96da`): 4 files changed, 88 insertions(+), 44 deletions(-), all under `scripts/`.

## Deleted special cases

_N/A._

## Tests run

- Helper, runtime path: `REIHITSU_DOTNET_ROOT=<scratch> bash -c '. scripts/lib/dotnet-env.sh && reihitsu_install_dotnet_runtime 10 >&2 && printf %s "$REIHITSU_DOTNET_ROOT"'` → exit 0, stdout exactly the root, `shared/Microsoft.NETCore.App/10.0.15` present. With major `99` → exit 2, `dotnet: dotnet-install.sh failed.`
- Helper, SDK path (unchanged behavior) with no `dotnet` on PATH and a scratch `REIHITSU_DOTNET_ROOT`: `scripts/prepare.sh --no-install` → `dotnet: no .NET 11 SDK found and --no-install was requested.`, exit 2; `scripts/prepare.sh` → `installing channel 11.0 into <scratch>` … `dotnet: installed 11.0.100 in <scratch>`, exit 0; preinstalled SDK: `scripts/prepare.sh` → `dotnet: using /root/.dotnet/dotnet (11.0.100)`, exit 0.
- Benchmark runtime install end-to-end, on that scratch SDK (no .NET 10 runtime): `scripts/benchmark.sh --version 1.2.0 --path src/Reihitsu.Formatter/Pipeline/Cleanup --repetitions 1 --warmups 0 --work <scratch>` → `Installing the .NET 10 runtime next to the SDK` / `Installed the .NET 10 runtime into <scratch sdk>`, 1.2.0 ran on runtime 10.0.0, exit 0.
- Benchmark, defaults without `--work`: `scripts/benchmark.sh --repetitions 1 --warmups 0 --top 5` → exit 0, 542 file lines per version, `Formatted 325, Unchanged 217` for both; work directory `/tmp/reihitsu-benchmark-0ldozq`, mode `700`; working tree unchanged afterwards. Results are in the report below (run D).
- `scripts/test.sh --configuration Debug --project architecture` → `Passed! - Failed: 0, Passed: 198` (also exercises `test.sh` through the changed helper).
- `scripts/lane-check.sh tooling --head worktree` → `LANE CHECK: confirmed — claimed tooling, diff needs tooling`.
- `shellcheck` is not installed in the sandbox; `bash -n scripts/lib/dotnet-env.sh` passes.

## Disputed tests or scope

_None._ The helper gains two functions with one new consumer (the benchmark) and one existing consumer refactored without behavior change; that is the single owner decision 4 asked for.

## Architecture findings

_None._

## Follow-up candidates

1. The follow-up in the report below (whole-tree token and trivia replacement), with the class enumerated as the auditor asked.

## Report for #1820

This section is meant to be posted on the issue as it stands.

---

### Benchmark: Reihitsu.Cli 1.2.0 against 1.3.0

**Result: no meaningful per-file regression between 1.2.0 and 1.3.0.** The cost a user sees on a large tree is better explained by the number of files and by a per-file cost that grows faster than the file's length, which both versions share (profile below).

**Premise correction.** The issue says the CLI now formats files in parallel. That holds for `main` (`--max-parallelism`), not for either release: 1.2.0 and 1.3.0 both process files one after another (`FormatCommandHandler`, a plain `foreach` at both tags). The comparison is therefore sequential by construction.

**Corpus and setup**

- Corpus: `src/Reihitsu.Formatter` and `src/Reihitsu.Analyzer` at `e56624d` (the commit tagged `v1.2.0`), C# files only: 542 files. Every `private ` was turned into `private  ` (byte order mark kept), so 325 files are rewritten and 217 stay unchanged in every run, for both versions.
- Machine: cloud sandbox, 4 logical processors, Ubuntu 24.04.5 LTS. Tools from NuGet: 1.2.0 (targets .NET 10.0.0), 1.3.0 (targets .NET 11.0.0).
- Each run: write mode, `--force --verbose`, one file at a time, a fresh copy of the corpus per repetition, versions interleaved (1.2.0, 1.3.0, 1.2.0, …). A file's time is the gap between the tool's verbose line for it and the line before; the first file of a run carries start-up and is reported separately.
- Tool: `scripts/benchmark.sh` (added by this work). Runs A and B: `--repetitions 5` after one warm-up; run B adds `--roll-forward` (both versions on .NET 11.0.0). Runs C and D: `--repetitions 1 --warmups 0`, own runtimes, made later as checks.

**Totals (seconds, process start to exit)**

| Run | Runtimes | 1.2.0 | 1.3.0 | 1.3.0 vs 1.2.0, total | Per-file median ratio |
|---|---|---|---|---|---|
| A, 5 reps | own (10.0.0 / 11.0.0) | median 16.01 [15.72–16.93]; reps 16.01, 15.82, 16.93, 15.72, 16.49 | median 17.60 [16.47–18.02]; reps 17.91, 16.53, 17.60, 18.02, 16.47 | +9.9 % | 1.05 |
| B, 5 reps | both 11.0.0 | median 15.88 [15.78–16.09]; reps 15.87, 15.96, 15.78, 15.88, 16.09 | median 16.07 [15.90–16.96]; reps 16.07, 16.96, 15.99, 16.61, 15.90 | +1.2 % | 1.04 |
| C, 1 rep | own | 15.17 | 16.34 | +7.7 % | 1.05 |
| D, 1 rep | own | 14.94 | 15.26 | +2.1 % | 1.04 |

The per-file median ratio is taken over the 541 files after the first. Per-file medians were 9.5 → 10.1 ms (A), 9.0 → 9.7 ms (B), 8.6 → 9.1 ms (D): about 0.5 ms per small file.

Start-up plus first file: 1.69 s / 1.77 s (A), 1.70 s / 1.78 s (B), 1.53 s / 1.66 s (D).

**Slowest files** (per-file median, ms; run B first, run A in brackets)

| Lines | File | 1.2.0 | 1.3.0 |
|---|---|---|---|
| 3434 | `src/Reihitsu.Analyzer/AnalyzerResources.cs` | 3256.8 (3441.4) | 3201.3 (3559.9) |
| 1144 | `src/Reihitsu.Analyzer/Rules/Clarity/RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.cs` | 677.1 (597.8) | 595.0 (698.2) |
| 841 | `src/Reihitsu.Formatter/Pipeline/SwitchCaseBraces/Rewriter/SwitchCaseBraceRewriter.cs` | 407.6 (409.4) | 399.4 (503.2) |
| 629 | `src/Reihitsu.Analyzer/Core/DocumentationAnalysisUtilities.cs` | 321.7 (310.3) | 314.2 (364.1) |
| 760 | `src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/LineBreakListRewriter.cs` | 298.4 (288.9) | 292.5 (354.3) |
| 589 | `src/Reihitsu.Analyzer/Data/ConfigurationManager.cs` | 232.5 (234.4) | 232.2 (288.9) |
| 516 | `src/Reihitsu.Formatter/Pipeline/DocumentationComments/DocumentationCommentFormattingPhase.cs` | 212.9 (232.1) | 220.2 (241.7) |
| 655 | `src/Reihitsu.Formatter/Pipeline/BlankLines/Rewriter/BlankLineTokenCleanupRewriter.cs` | 211.9 (228.4) | 209.2 (266.4) |
| 509 | `src/Reihitsu.Analyzer/Rules/Performance/RH1001TypesUsedAsKeysMustImplementEqualityMembersAnalyzer.cs` | 176.4 (175.0) | 183.1 (200.2) |
| 483 | `src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/LineBreakInitializerRewriter.cs` | 183.1 (169.9) | 182.8 (214.8) |

`AnalyzerResources.cs` alone takes 3.2–3.6 s, about a fifth of every run, in both versions.

**What the numbers show, and what they do not**

- On the same runtime (run B), 1.3.0 is 1 % slower in total, inside its own run-to-run spread (15.90–16.96 s).
- On their own runtimes, 1.3.0 was slower in total in all three runs, by 9.9 %, 7.7 %, and 2.1 %. The size of that gap varies a lot between runs on this machine, and **its cause is not established**: these measurements do not separate the runtime change (.NET 10 → 11), the code, and the machine.
- A direct alternating test of the two largest files (`AnalyzerResources.cs` and the RH3002 analyzer, 4 rounds) showed no difference: 1.2.0 on .NET 10 5.00–5.10 s, 1.2.0 rolled forward to .NET 11 4.91–5.39 s, 1.3.0 on .NET 11 4.98–5.39 s.
- The per-file median ratio was 1.04–1.05 in every run, about 0.5 ms per small file; the slowest files are the same in both versions and within a few percent of each other.

### Per-phase profile of the per-file time

Measured on `main` (`074661f`; no formatter change since), in process, Release build, with a throwaway harness that runs the same steps as `ReihitsuFormatter.FormatSyntaxTree` and times each phase through the pipeline's phase observer, on the perturbed corpus above. Both released versions contain the two call sites named below unchanged.

The milliseconds are warm, in-process timings (median of 3–5 iterations after warm-up). They show how the time divides between phases; they are not what the CLI costs per file — the released CLI takes 3.2–3.6 s on `AnalyzerResources.cs` in one pass with cold JIT tiers, reading and writing included, against 2.3 s here. Read the shares, not the absolute values.

**Slowest files, by phase** (warm in-process ms; share of the pipeline)

| Phase | AnalyzerResources (3434 lines) | RH3002…Analyzer (1144) | SwitchCaseBraceRewriter (841) | DocumentationAnalysisUtilities (629) | LineBreakListRewriter (760) |
|---|---|---|---|---|---|
| Precheck | 1.6 | 1.2 | 1.0 | 0.7 | 1.1 |
| Preparation (tabs + line endings) | 6.5 | 3.2 | 3.0 | 1.9 | 2.0 |
| StructuralTransform | 14.1 | 7.1 | 5.5 | 3.7 | 4.7 |
| RegionFormatting | 1.6 | 0.9 | 0.7 | 0.6 | 0.7 |
| DocumentationComment | 6.7 | 2.1 | 1.8 | 1.3 | 1.5 |
| UsingDirectiveOrdering | 0.9 | 0.9 | 0.6 | 0.4 | 0.6 |
| LineBreak | 56.0 | 17.5 | 12.8 | 8.2 | 11.2 |
| SwitchCaseBrace | 0.9 | 0.7 | 0.4 | 0.2 | 0.3 |
| BlankLine | 70.9 | 12.5 | 10.0 | 6.7 | 8.7 |
| HorizontalSpacing | 25.6 | 4.0 | 3.1 | 2.2 | 2.9 |
| **Indentation** | **1090.8 (47 %)** | **316.2 (47 %)** | **204.6 (45 %)** | **101.6 (45 %)** | **155.7 (46 %)** |
| **Cleanup** | **994.9 (43 %)** | **290.9 (43 %)** | **197.6 (44 %)** | **96.3 (43 %)** | **144.4 (42 %)** |
| Pipeline total | 2315.4 | 674.2 | 453.4 | 223.5 | 340.8 |
| Parse (once) | 6.7 | 2.1 | 1.8 | 1.4 | 1.4 |
| Syntax-error, generated, hidden-code checks | 1.8 | 0.9 | 0.8 | 0.6 | 0.7 |

Indentation and Cleanup together are 88–90 % of the pipeline on each of these files.

**Whole corpus** (542 files, sum of per-file medians): Indentation about 41 % and Cleanup about 35 % of the pipeline, 76 % together; LineBreak, BlankLine, and StructuralTransform follow at about 8 %, 7 %, and 4 %, every other phase below 3 %. Parsing happens once per file; the CLI's own syntax-error, generated-code, and hidden-code checks, repeated by `FormatSyntaxTree`, are below 1 %. No phase re-parses the text.

**Where the time goes.** A CPU sample (`dotnet-trace`, 3 iterations of `AnalyzerResources.cs`) puts 75 % inclusive in Roslyn's `SyntaxNodeExtensions.ReplaceTokens` → `SyntaxReplacer` (`CalculateVisitationCriteria` 67 % inclusive, `HashSetExtensions.Any` 42 % exclusive), reached from `IndentationRewriter.Apply` (38.5 %) and `CleanupPhase.Execute` (36.8 %). `LayoutComputer.Compute` is 6.8 %.

Both call sites replace every token of the tree in one call, `root.ReplaceTokens(root.DescendantTokens(), ...)`, and that call's cost grows with the square of the token count. Synthetic class with N expression-bodied properties, each with a three-line documentation comment (warm in-process ms, best of 3):

| N | Lines | Tokens | Identity `ReplaceTokens` over all tokens | Identity `CSharpSyntaxRewriter.VisitToken` | Indentation phase | Cleanup phase | All other phases |
|---|---|---|---|---|---|---|---|
| 100 | 507 | 1321 | 70.9 | 2.0 | 60.5 | 31.2 | 91.2 |
| 200 | 1007 | 2621 | 81.9 | 1.1 | 106.7 | 83.7 | 45.6 |
| 400 | 2007 | 5221 | 307.7 | 2.2 | 351.1 | 321.2 | 76.2 |
| 800 | 4007 | 10421 | 1278.6 | 2.9 | 1325.1 | 1266.6 | 181.9 |
| 1600 | 8007 | 20821 | 5673.2 | 7.6 | 6029.3 | 6163.4 | 575.9 |

From N = 400 up, each doubling of the input multiplies the whole-tree replacement and both phases by about 4, while a token-visiting rewriter stays near-linear. Over the corpus, one identity whole-tree `ReplaceTokens` per file is about 31 % of the pipeline, so the two calls account for roughly 60 % of it.

The remaining phases also grow faster than linearly at the largest size (576 ms for N = 1600 against 182 ms for N = 800); that was not investigated and is an order of magnitude below the two calls.

### Follow-up candidate

**Formatter: whole-tree token and trivia replacement grows quadratically with file size.** No output changes; a performance follow-up of #1795.

- Mechanism: a Roslyn `ReplaceTokens`/`ReplaceTrivia` over the whole root whose replacement set grows with the file. Roslyn's replacer tests each visited node against the set of replaced spans, so the cost is quadratic in the set's size. A fix should address the mechanism, not two call sites.
- Members of the class (`rg -n "ReplaceTokens\(|ReplaceTrivia\(|ReplaceSyntax\(" src/Reihitsu.Formatter`, 31 sites):
  - `Pipeline/Indentation/Utilities/IndentationRewriter.cs:39` (`IndentationRewriter.Apply`) and `Pipeline/Cleanup/CleanupPhase.cs:33` (`CleanupPhase.Execute`) — every token of the file; the measured hot spot, 76 % of the pipeline over the corpus and 88–90 % on the slowest files.
  - `Pipeline/TabReplacement/TabReplacementPhase.cs:39` — every token with a tab in its whitespace, which in a tab-indented file is about every line. Measured with released 1.3.0 on `AnalyzerResources.cs` re-indented with tabs (2739 tab-indented lines): about +0.1 s (3 %) over the space-indented file, three alternating rounds. The benchmark corpus has no tabs, so the profile above could not see it.
  - `Pipeline/LineEndings/LineEndingNormalizationPhase.cs:45` (and the documentation-newline replacement at `:31`) — every line ending that differs from the detected one; grows with the file only for mixed line endings. Not measured.
  - `Pipeline/TabReplacement/TabReplacementPhase.cs:151` — one directive's own structure; small.
  - Every other site replaces a filtered or local set of a few tokens.
- Evidence: the shares and doubling behaviour above (cite those, not the warm in-process milliseconds, as the CLI's cost); the CPU sample with 75 % below `SyntaxReplacer`; an identity whole-tree `ReplaceTokens` takes 5673 ms on 20821 tokens where a token-visiting `CSharpSyntaxRewriter` takes 7.6 ms.
- Input that shows it: `src/Reihitsu.Analyzer/AnalyzerResources.cs` at `v1.2.0` (3434 lines; 3.2–3.6 s per file through the released CLI in both versions), or a generated class with N expression-bodied properties each preceded by a `/// <summary>` comment: formatting time roughly quadruples when N doubles.

No other repeatable hot spot was found.

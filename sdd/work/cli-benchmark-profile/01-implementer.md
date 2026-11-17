Role: implementer
Commit: f4b9b39
Verdict: tests pass

No test handover exists in this lane; the work is a benchmark tool plus a measurement. The prompt carried no intended fix or suspected cause.

## Changes

| File | What changed | Requirement |
|---|---|---|
| `scripts/benchmark.sh` | New wrapper: SDK resolution through `scripts/lib/dotnet-env.sh`, `--no-install`, exit codes `0`/`2`; mode 100755 | thoenissen/Reihitsu#1820 item 1 |
| `scripts/benchmark/benchmark.cs` | New file-based app (no project reference): extracts a pinned corpus with `git archive` (fetching the commit in a shallow clone), optionally perturbs it, installs each `Reihitsu.Cli` version with `dotnet tool install --tool-path`, installs a missing target runtime next to the SDK (or `--roll-forward` runs every version on the newest runtime), runs each version in write mode with `--force --verbose` on a fresh copy, interleaved after warm-ups, stamps each verbose file line, and reports totals, per-file medians, ratios, and slowest files, plus a raw `per-file.csv` | thoenissen/Reihitsu#1820 item 1 |
| `scripts/benchmark/Directory.Build.props` | Isolates the app from the solution's props, like `scripts/trace/` | — |
| `scripts/README.md` | Table row, example line, and section *The CLI benchmark* | — |

A version that offers `--max-parallelism` gets `--max-parallelism 1` (detected from its `--help`); neither 1.2.0 nor 1.3.0 has the option, and both process files one after another (`FormatCommandHandler` at `v1.2.0` and `v1.3.0`: a plain `foreach` over the files).

## Policy owners

_N/A_ — no production code changed. `rg "ReplaceTokens\(.*DescendantTokens\(\)" src` lists the whole-tree token replacements the profile names: `IndentationRewriter.cs:39`, `CleanupPhase.cs:33`, and `TabReplacementPhase.cs:151` (the last one on a directive's structure only, small).

## Production diff size

None: `git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'` is empty. No profiling code was committed; the throwaway harnesses lived in the scratch area.

## Deleted special cases

_N/A._

## Tests run

- `scripts/test.sh --configuration Debug --project architecture` — `Passed! - Failed: 0, Passed: 198` (covers the shell-script executable mode and the file-based-app Roslyn pin).
- `scripts/lane-check.sh tooling --head worktree` — `LANE CHECK: confirmed — claimed tooling, diff needs tooling`.
- `scripts/benchmark.sh --bogus` and `--repetitions 0` — exit `2` with a message each.

## Disputed tests or scope

_None._ Goal check on the `agent-filed` issue: both goals make sense and are met. One premise does not hold as written: the issue says "the CLI now formats files in parallel"; that is true of `main`, not of either release, so the release comparison is sequential by construction and needs no option.

## Benchmark

### Corpus and setup

- Corpus: `src/Reihitsu.Formatter` and `src/Reihitsu.Analyzer` at `e56624d45ea91431552c960665c181fe04545213` (the commit tagged `v1.2.0`), C# files only: **542 files**. Every `private ` was turned into `private  ` (byte order mark kept), so **325 files are rewritten and 217 stay unchanged** in every run, for both versions.
- Machine: cloud sandbox, 4 logical processors, Ubuntu 24.04.5 LTS. Tools from NuGet: `Reihitsu.Cli` 1.2.0 (targets .NET 10.0.0) and 1.3.0 (targets .NET 11.0.0).
- Each run: write mode, `--force --verbose`, one file at a time, fresh copy of the corpus; one untimed warm-up per version, then 5 repetitions interleaved 1.2.0, 1.3.0, 1.2.0, ...
- Commands: `scripts/benchmark.sh --repetitions 5 --top 15` (each version on its own runtime) and `scripts/benchmark.sh --repetitions 5 --top 15 --roll-forward` (both on .NET 11.0.0).

### Raw totals (seconds, process start to exit)

| Run | 1.2.0 | 1.3.0 |
|---|---|---|
| Own runtime (10.0.0 / 11.0.0), reps 1–5 | 16.01, 15.82, 16.93, 15.72, 16.49 | 17.91, 16.53, 17.60, 18.02, 16.47 |
| Both on 11.0.0, reps 1–5 | 15.87, 15.96, 15.78, 15.88, 16.09 | 16.07, 16.96, 15.99, 16.61, 15.90 |

### Summary

| Run | Version | Total median [min–max] | Start-up + first file | Sum of per-file medians | Per-file median |
|---|---|---|---|---|---|
| Own runtime | 1.2.0 | 16.01 s [15.72–16.93] | 1.69 s | 14.36 s | 9.5 ms |
| Own runtime | 1.3.0 | 17.60 s [16.47–18.02] | 1.77 s | 15.63 s | 10.1 ms |
| Both on 11.0.0 | 1.2.0 | 15.88 s [15.78–16.09] | 1.70 s | 13.87 s | 9.0 ms |
| Both on 11.0.0 | 1.3.0 | 16.07 s [15.90–16.96] | 1.78 s | 14.05 s | 9.7 ms |

Per file over the 541 files after the first, 1.3.0 against 1.2.0: median ratio **1.05** on their own runtimes, **1.04** on the same runtime.

### Slowest files (per-file median, ms; same-runtime run first, own-runtime run in brackets)

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

`AnalyzerResources.cs` alone is about 3.2–3.6 s, a fifth of every run, in both versions.

### Conclusion on a per-file regression

No meaningful per-file regression between 1.2.0 and 1.3.0. On the same runtime the totals differ by 1 % (15.88 s against 16.07 s), inside the run-to-run spread of 1.3.0 (15.90–16.96 s), and the per-file median ratio is 1.04. On their own runtimes 1.3.0 is about 10 % slower in total (median 17.60 s against 16.01 s) with overlapping ranges; the same 1.3.0 binary on the same .NET 11.0.0 measured 16.07 s in the other run, so most of that gap is run-to-run noise on this machine rather than code. The slowest files are the same in both versions and slow by the same amount. The cost the user saw is better explained by file count and by a per-file cost that grows faster than file length (below), which both versions share.

## Per-phase profile

Measured on the code at `f4b9b39` (`main` at `074661f` plus squad files), in process, Release build, with a throwaway file-based harness in the scratch area that calls the same steps as `ReihitsuFormatter.FormatSyntaxTree` and times each phase through the pipeline's phase observer; median of 3–5 warm iterations; input is the perturbed corpus above. Both released versions contain the same two call sites named below (`git show v1.2.0:` / `v1.3.0:` of `CleanupPhase.cs:33` and `IndentationRewriter.cs:39`).

### Slowest files, by phase (ms)

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
| **Indentation** | **1090.8** | **316.2** | **204.6** | **101.6** | **155.7** |
| **Cleanup** | **994.9** | **290.9** | **197.6** | **96.3** | **144.4** |
| Pipeline total | 2315.4 | 674.2 | 453.4 | 223.5 | 340.8 |
| Parse (once) | 6.7 | 2.1 | 1.8 | 1.4 | 1.4 |
| Syntax-error, generated, hidden-code checks | 1.8 | 0.9 | 0.8 | 0.6 | 0.7 |

### Whole corpus (542 files, sum of per-file medians)

Pipeline 10.5–11.4 s, of which Indentation 4.2–4.7 s (about 41 %) and Cleanup 3.5–3.9 s (about 35 %); LineBreak 0.87 s, BlankLine 0.71 s, StructuralTransform 0.38 s, every other phase below 0.25 s. Parsing is done once per file (0.13 s in total); the CLI's own syntax-error, generated-code, and hidden-code checks, which `FormatSyntaxTree` repeats, cost 0.05 s in total. No phase re-parses the text.

### Where the time goes

A CPU sample (`dotnet-trace collect`, default profile, 3 iterations of `AnalyzerResources.cs`): 75 % inclusive in Roslyn's `SyntaxNodeExtensions.ReplaceTokens` → `SyntaxReplacer` (`CalculateVisitationCriteria` 67 % inclusive, `HashSetExtensions.Any` 42 % exclusive, plus 34 % in memory zeroing and copying beneath it), reached from `IndentationRewriter.Apply` (38.5 %) and `CleanupPhase.Execute` (36.8 %). `LayoutComputer.Compute` is 6.8 %.

Both call sites replace every token of the tree in one call: `root.ReplaceTokens(root.DescendantTokens(), ...)` at `src/Reihitsu.Formatter/Pipeline/Indentation/Utilities/IndentationRewriter.cs:39` and `src/Reihitsu.Formatter/Pipeline/Cleanup/CleanupPhase.cs:33`. A whole-tree `ReplaceTokens` call costs time quadratic in the token count. Synthetic class with N expression-bodied properties, each with a three-line documentation comment (best of 3, ms):

| N | Lines | Tokens | Identity `ReplaceTokens` over all tokens | Identity `CSharpSyntaxRewriter.VisitToken` | Indentation phase | Cleanup phase | All other phases |
|---|---|---|---|---|---|---|---|
| 100 | 507 | 1321 | 70.9 | 2.0 | 60.5 | 31.2 | 91.2 |
| 200 | 1007 | 2621 | 81.9 | 1.1 | 106.7 | 83.7 | 45.6 |
| 400 | 2007 | 5221 | 307.7 | 2.2 | 351.1 | 321.2 | 76.2 |
| 800 | 4007 | 10421 | 1278.6 | 2.9 | 1325.1 | 1266.6 | 181.9 |
| 1600 | 8007 | 20821 | 5673.2 | 7.6 | 6029.3 | 6163.4 | 575.9 |

From 400 up, each doubling of the input multiplies the replacement and both phases by about 4. Across the corpus, one identity `ReplaceTokens` over all tokens of each file costs 3.22 s in total — 31 % of the 10.5 s pipeline — so the two calls account for roughly 6.4 s of it.

## Architecture findings

_None._

## Follow-up candidates

1. **Formatter: the indentation and cleanup phases rewrite every token with one whole-tree `ReplaceTokens` call, whose cost grows quadratically with the file's token count.**
   - Component: `src/Reihitsu.Formatter/Pipeline/Indentation/Utilities/IndentationRewriter.cs:39` (`IndentationRewriter.Apply`, `IndentationPhase`) and `src/Reihitsu.Formatter/Pipeline/Cleanup/CleanupPhase.cs:33` (`CleanupPhase.Execute`); present unchanged in 1.2.0, 1.3.0, and `main`.
   - Evidence: the two phases take 76 % of the pipeline over the 542-file corpus (Indentation 4.2–4.7 s, Cleanup 3.5–3.9 s of 10.5–11.4 s) and 90 % on `AnalyzerResources.cs` (1091 ms and 995 ms of 2315 ms); a CPU sample puts 75 % of that file's time in Roslyn's `SyntaxReplacer` below these two calls. An identity whole-tree `ReplaceTokens` takes 5673 ms on 20821 tokens where a token-visiting `CSharpSyntaxRewriter` takes 7.6 ms, and it quadruples per doubling (308 → 1279 → 5673 ms for 5221 → 10421 → 20821 tokens). Over the corpus, one such call per file costs 3.22 s, about 31 % of the pipeline.
   - Input that shows it: `src/Reihitsu.Analyzer/AnalyzerResources.cs` at `v1.2.0` (3434 lines, 680 properties; 3.2–3.6 s per file through the released CLI in both versions), or any generated class with N expression-bodied properties each preceded by a `/// <summary>` comment — formatting time roughly quadruples when N doubles (N = 800: Indentation 1325 ms, Cleanup 1267 ms; N = 1600: 6029 ms and 6163 ms).
   - Output: none changes; it is a performance follow-up of thoenissen/Reihitsu#1795.

No other repeatable hot spot was found: no phase re-parses the text, the repeated pre-checks cost 0.05 s per corpus, and the remaining phases together are about 2.5 s of 10.5 s (their share also grows faster than linearly at 8000 lines — 576 ms for N = 1600 against 182 ms for N = 800 — but was not investigated further, and it is an order of magnitude below the two calls above).

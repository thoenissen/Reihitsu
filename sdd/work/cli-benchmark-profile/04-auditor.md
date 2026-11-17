Role: auditor
Commit: 746358d8501716528220d62de1ef0de78cf495da
Verdict: findings

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

Full audit. Head `746358d` (checked out, clean tree); `origin/main` is `074661f9dccd0c1edf192d30e032b15856774a9b` (`git ls-remote`) and is an ancestor of the head. The diff against it is `scripts/benchmark.sh` (100755), `scripts/benchmark/benchmark.cs`, `scripts/benchmark/Directory.Build.props`, `scripts/README.md`, and `sdd/work/cli-benchmark-profile/`, as `01-implementer.md` describes; no `src/`, spec, or rule-page path. `scripts/lane-check.sh tooling`: confirmed.

Issue thoenissen/Reihitsu#1820 (`agent-filed`): item 1 (benchmark 1.2.0 against 1.3.0 at degree 1 on a fixed corpus) and item 2 (per-phase profile of the slowest files, one follow-up per hot spot) are both addressed in `01-implementer.md`; the tool is the run's only repository change.

## Measurement and profile claims

Checked against the released sources (`git show v1.2.0:` / `v1.3.0:`), `main`, and one run of the tool by the auditor (`scripts/benchmark.sh --repetitions 1 --warmups 0 --top 5 --work <scratch>`, exit 0, repository left unchanged).

| Claim in `01-implementer.md` | Evidence | Verdict |
|---|---|---|
| 1.2.0 and 1.3.0 format one file after another | `FormatCommandHandler.ProcessFilesAsync` is a plain `foreach` at `v1.2.0` (line 253) and `v1.3.0` (line 266); `--max-parallelism` exists only on `main` (`Program.cs:28`) | holds |
| A file's time is the gap to its verbose line | Both versions print the per-file line after the file is processed (`Unchanged:` after formatting, `Formatted:` after the write, `v1.3.0` lines 358, 435); every file prints one line under `--verbose`; a file error or an unsupported file makes the tool exit 2 (`v1.3.0` lines 183-187), which aborts the benchmark rather than folding that file's time into the next | holds |
| 542 files, 325 rewritten, 217 unchanged, for both versions | Auditor's run: `Formatted 325, Unchanged 217` for both, 542 file lines each | reproduced |
| Own-runtime run: 1.3.0 about 10 % slower in total, per-file median ratio 1.05 | Auditor's run: 15.17 s against 16.34 s (+7.7 %), per-file median ratio 1.05, slowest files 9-15 % slower in 1.3.0 | reproduced |
| The own-runtime gap is "mostly run-to-run noise", not code | An inference from comparing two separate runs; the gap reproduced in a second own-runtime run (above). A direct alternating test of the two largest files (`AnalyzerResources.cs` and the RH3002 analyzer, 4 rounds) shows parity: 1.2.0 on .NET 10 5.00-5.10 s, 1.2.0 rolled forward to .NET 11 4.91-5.39 s, 1.3.0 on .NET 11 4.98-5.39 s | conclusion "no meaningful per-file regression" holds; the attribution to noise is unproven (Hints) |
| Whole-tree `ReplaceTokens` at `IndentationRewriter.cs:39` and `CleanupPhase.cs:33`, unchanged in both releases and `main` | Present verbatim at both tags and on `main` | holds |
| That call is quadratic in the token count | The doubling table (308 → 1279 → 5673 ms) and the CPU sample (`HashSetExtensions.Any` 42 % exclusive below `SyntaxReplacer`) agree with Roslyn's replacer, which tests each visited node against the set of replaced spans | supported |
| No phase re-parses the text | `rg` over `src/Reihitsu.Formatter`: only trivia-sized `ParseLeadingTrivia`/`ParseTrailingTrivia` per comment (`DocumentationCommentFormattingPhase.cs:405,545`, `RawStringLineShifts.cs:387`, `TriviaFact.cs:72`); the one `WithChangedText` (`DocumentationCommentFormattingPhase.cs:66`) is in `LayOutElement`, reached only from `ReihitsuFormatter.cs:428` for the RH8305/RH8307 code fixes, not from the pipeline | holds |
| Profile measured on `main`, not on the releases | Stated in the handover; no `src/` change since `f4b9b39`, so the profile still describes the head | holds |

The report is supportable for posting once the attribution sentence is reworded (Hints, first item) and the follow-up names the extra member (Follow-up candidates).

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | medium | `scripts/benchmark/benchmark.cs:188`, `:301-306`, `:349` | – (security hygiene of a script that downloads and executes) | – | PR-introduced | Without `--work`, the work directory is the predictable `$TMPDIR/reihitsu-benchmark-<pid>`, created with `Directory.CreateDirectory` (succeeds on an existing directory another user owns), and an existing `tools/<version>/reihitsu-format` below it is executed without installing (`if (File.Exists(executable) == false)`). On a shared machine, a user who pre-creates `/tmp/reihitsu-benchmark-<pid>/tools/1.3.0/reihitsu-format` for a guessable PID gets that binary run as the benchmarking user. The runtime installer goes to the predictable `/tmp/dotnet-install-<pid>.sh` and is then run with `bash`; the repository's own installer uses `mktemp` (`scripts/lib/dotnet-env.sh:87`). | Create the default work directory with `Directory.CreateTempSubdirectory("reihitsu-benchmark-")` (random name, owner-only mode on Unix) and download the installer into the work directory, or into a `mktemp` file through the single owner of #2. |
| 2 | medium | `scripts/benchmark/benchmark.cs:346-364` against `scripts/lib/dotnet-env.sh:84-106` | FC-015 | – | new mechanism | Two owners of "fetch `dotnet-install.sh` over https only and install into `REIHITSU_DOTNET_ROOT`": the URL, the `--proto`/`--proto-redir` flags, and the target root are copied, and the copies already diverge (temporary-file creation, #1; error messages; `dotnet-env.sh` checks the result afterwards). The diff neither justifies the second owner nor proves parity. | One owner: a function in `scripts/lib/dotnet-env.sh` that installs a runtime of a given major into `REIHITSU_DOTNET_ROOT` (honoring `--no-install` as today), called by the app through `bash`; or the lead records why a C# copy is acceptable. |
| 3 | low | `scripts/README.md` (*The CLI benchmark*, "the repository is only read"), `scripts/benchmark.sh:12-13` | – | – | PR-introduced | `ResolveCommitAsync` (`benchmark.cs:248-254`) runs `git fetch --quiet --depth 1 origin <commit>` in the repository whenever the commit does not resolve locally — in a shallow clone, and also for any unknown `--commit` value — which writes objects, `FETCH_HEAD`, and in a shallow clone `.git/shallow`. "The repository is only read" is therefore not true as written. Text only: no compiled behavior changes. | Say that the working tree is only read and a commit missing from the clone is fetched into it. |
| 4 | low | `scripts/benchmark/benchmark.cs:18` | FC-031 | – | PR-introduced | The header comment says the versions run "after one untimed warm-up run each"; the count is `--warmups N` (default 1, may be 0). Comment only. | "after the untimed warm-up runs (`--warmups`, one by default)". |
| 5 | low | `scripts/benchmark/benchmark.cs:288` | – | – | PR-introduced | `perturbed.TrimStart('﻿')` holds a literal, invisible U+FEFF in the char literal; an editor or a copy can drop it silently, and the repository spells it `'﻿'` (`src/Reihitsu.Core/DocumentationCommentUtilities.cs:33`). The formatter skips this file (the `#:` directives parse as errors), so self-formatting does not catch it. | `TrimStart('﻿')`. |

Findings 3 and 4 are confined to comments and documentation; findings 1, 2, and 5 change code, so the verdict is not `pass, text cleanup`.

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Which output line is a per-file line (`FileLine` regex plus the `copy` path prefix, `benchmark.cs:76`, `:439`) | One whole stdout or stderr line | The CLI's per-file lines at `v1.2.0`/`v1.3.0`: `Formatted:`, `Unchanged:`, `Skipped (generated|syntax errors|code hidden in another build configuration|could not decode):`, each followed by the absolute path the CLI was given | Every per-file line of both versions matches; the non-matching ones (`error: <path>(l,c): unsupported syntax…`, `Error processing <path>: …`) both make the CLI exit 2, which the exit-code check turns into a benchmark failure | agrees |
| Accepted tool exit codes (`benchmark.cs:427`, 0 or 1) | Process exit code | `ExitCodes` at `v1.3.0`: 0 Success, 1 FormattingNeeded (check and dry-run only), 2 Error | agrees | agrees |
| Whether to pass `--max-parallelism 1` (`benchmark.cs:338`) | Whole `--help` output, substring | `Program.cs:350` on `main` prints `--max-parallelism <count>`; neither release mentions it | agrees | agrees |
| Whether the target runtime is installed (`benchmark.cs:315`) | `Microsoft.NETCore.App <major>.` prefix in `dotnet --list-runtimes` | The host's default roll-forward (`Minor`): any minor or patch of the target major | agrees | agrees |
| Whether a file carries a byte order mark (`benchmark.cs:287-290`) | First three bytes | `Encoding.UTF8.GetString` keeps U+FEFF as the first char, `TrimStart` removes it, `UTF8Encoding(true)` writes it back | agrees (auditor's run: 325/217 identical for both versions) | agrees |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Downloading and running `dotnet-install.sh` into `REIHITSU_DOTNET_ROOT` | `scripts/lib/dotnet-env.sh:89` (`reihitsu_ensure_dotnet`), `scripts/benchmark/benchmark.cs:355` (`InstallRuntimeAsync`); `rg -n "dotnet-install" scripts` | two owners — finding #2 |
| SDK resolution and `--no-install` for the wrapper | `scripts/benchmark.sh` sources `scripts/lib/dotnet-env.sh` like the other scripts | one owner |
| Isolation of a file-based app from the solution's props | `scripts/benchmark/Directory.Build.props`, mirroring `scripts/trace/Directory.Build.props` | the established per-app pattern |
| Per-file timing and summary statistics | `scripts/benchmark/benchmark.cs` only | one owner |

## Assertion adequacy
_N/A — the run adds no test; the tooling lane runs the test implementer only when the tool has tests, and `00-lead.md` records the skip (the tool is a script plus a file-based app outside any test project)._

## Failure classes dismissed
- FC-001, FC-002, FC-003 — the diff adds no predicate to production code; the benchmark's own predicates are in *Guard scope*.
- FC-004, FC-005, FC-019, FC-020, FC-021 — no edit of syntax, trivia, or directives in `src/`; the corpus perturbation writes only a scratch copy.
- FC-006, FC-022 to FC-030, FC-036 — no rewrite, code fix, or formatter change.
- FC-007, FC-008, FC-009, FC-018, FC-043 — no code fix touched.
- FC-010, FC-011, FC-012, FC-013, FC-014 — no formatter phase or analyzer touched.
- FC-016 — no fix in this run; the sibling enumeration of the profiled mechanism is under *Follow-up candidates*.
- FC-017, FC-033, FC-034, FC-035 — no test added. The profile harness is not committed; its absolute numbers are warm in-process timings, noted under *Hints*.
- FC-032 — the profile was gathered at `f4b9b39`; no compiled change followed it.
- FC-037 — no text-only proof claimed.
- FC-038 to FC-042, FC-044 — no unsupported shape, precheck, or guard modelling a computed result.
- Tab-width matrix — no phase touched.

## Architecture findings
_None._ The tool is standalone; the duplicated installer is a policy finding (#2) within scripts, not a structural defect of a runtime component.

## Hints
- **Attribution of the own-runtime gap.** `01-implementer.md` says the 10 % own-runtime total gap is "mostly run-to-run noise". The within-run comparison, which the interleaving protects, reproduced in the auditor's own-runtime run (+7.7 %, slowest files 9-15 % slower in 1.3.0), while the alternating two-file test above shows parity on both runtimes. The supported statement for the issue is: 1.3.0 was 8-10 % slower in total in two own-runtime runs and 1 % in the same-runtime run; a per-file alternating test of the two largest files showed no difference; the per-file median ratio was 1.04-1.05 in all three runs (about 0.5 ms per small file). The cause of the whole-run gap is not established. The conclusion that no meaningful per-file code regression exists still holds.
- **Absolute profile numbers.** The per-phase milliseconds are warm, in-process timings on `main` (`AnalyzerResources.cs`: 2315 ms pipeline), while the released CLI takes 3.2-3.6 s on the same file in one pass (cold tiers, read and write included). The follow-up should cite the shares and the doubling behavior rather than the absolute milliseconds as the CLI's cost.
- **Kept work directory.** Without `--work`, each run leaves about 90 MB below the temporary directory (83 MB of it installed tools). Keeping it is intended, since `per-file.csv` is printed for later use; the README could say the directory is kept.
- **Corpus with an unsupported shape.** A corpus that 1.3.0 or later refuses as unsupported makes the tool exit 2 and aborts the whole benchmark; the tool does not pass `--suppress-unsupported` to versions that offer it. Acceptable for the default corpus, which both versions process completely.

## Follow-up candidates
1. Confirmed as written in `01-implementer.md` and `00-lead.md` (whole-tree `ReplaceTokens` in `IndentationRewriter.Apply`, `IndentationRewriter.cs:39`, and `CleanupPhase.Execute`, `CleanupPhase.cs:33`), with its class enumerated so a fix covers the mechanism, not two call sites. The mechanism is a `ReplaceTokens`/`ReplaceTrivia` over the whole root whose replacement set grows with the file:
   - `IndentationRewriter.cs:39`, `CleanupPhase.cs:33` — every token; the measured hot spot.
   - `TabReplacementPhase.cs:39` — every token with a tab in its whitespace, which is about every line in a tab-indented file. Measured with released 1.3.0 on `AnalyzerResources.cs` re-indented with tabs (2739 tab-indented lines): about +0.1 s (3 %) over the space-indented file, three alternating rounds. A smaller member; the benchmark corpus has no tabs, so the profile could not see it.
   - `LineEndingNormalizationPhase.cs:45` — every end of line that differs from the detected one; grows with the file only for mixed line endings. Not measured.
   - `TabReplacementPhase.cs:151` — one directive's structure; small, as the implementer states.
   - Every other `ReplaceTokens`/`ReplaceTrivia`/`ReplaceSyntax` in `src/Reihitsu.Formatter` (`rg -n "ReplaceTokens\(|ReplaceTrivia\(|ReplaceSyntax\(" src/Reihitsu.Formatter`, 31 sites) replaces a filtered or local set of a few tokens.

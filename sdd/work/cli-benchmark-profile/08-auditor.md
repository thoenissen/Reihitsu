Role: auditor
Commit: 322b9d48a10310bd0a91bf92a76b2a82264b4371
Verdict: pass

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

Repair delta against `746358d` (audit 1, `04-auditor.md`). Head `322b9d4` checked out, clean tree; `origin/main` is `074661f9dccd0c1edf192d30e032b15856774a9b` (`git ls-remote`) and is an ancestor of the head. The repair diff `746358d..322b9d4` touches `scripts/lib/dotnet-env.sh`, `scripts/benchmark/benchmark.cs`, `scripts/benchmark.sh`, `scripts/README.md`, and this run's `sdd/work/` files, as `05-implementer.md` describes; no `src/`, spec, or rule-page path. `scripts/lane-check.sh tooling`: confirmed. The repair did not grow into an unrelated surface: the shared helper change is the single owner decision 4 in `00-lead.md` asked for, and its existing consumers were traced (below).

Previous findings:

| # | Status | Evidence |
|---|---|---|
| 1 | closed | Default work directory is `Directory.CreateTempSubdirectory("reihitsu-benchmark-")` (`benchmark.cs:192`; random name, mode 700 on Unix); the app no longer writes an installer of its own, the helper uses `mktemp` (`dotnet-env.sh:57`). A `--work` directory still reuses installed tools; that directory is the user's choice and the README says so. |
| 2 | closed | `rg -n "dotnet-install\|dot\.net/v1"` outside `sdd/work/` finds the download and run only in `reihitsu_run_dotnet_install` (`dotnet-env.sh:54-76`); `reihitsu_ensure_dotnet` and `reihitsu_install_dotnet_runtime` call it; the app reaches it through `bash -c`. The C# copy of the `REIHITSU_DOTNET_ROOT` default is gone too. |
| 3 | closed | `scripts/README.md` and `benchmark.sh:12-14`: "The working tree is only read; a commit missing from the clone is fetched into it", which matches `ResolveCommitAsync` (`benchmark.cs:246-258`) for a shallow clone and for any unknown `--commit`. |
| 4 | closed | Header comment names `--warmups`, one by default; `Options.Warmups` defaults to 1 and accepts 0. |
| 5 | closed | `TrimStart('﻿')` (`benchmark.cs:292`); no invisible format character remains in any changed script or the README (scan for Unicode category Cf). |

## Repair checks

- **`reihitsu_ensure_dotnet` unchanged for its seven existing callers** (`prepare.sh`, `build.sh`, `test.sh`, `format.sh`, `trace.sh`, `benchmark.sh` bare under `set -e`; `apply-fix.sh`, `lane-check.sh`, `verify-text-only.sh` in `if !`): same progress line, same installer arguments (`--channel`, `--install-dir`), same two error messages and exit status 2, same PATH/DOTNET_ROOT export and post-install SDK check. The one difference is that the helper body now runs inside `|| return 2`, so `errexit` is off for it; that only changes the outcome of a failing `mktemp`, which before aborted the bare callers with mktemp's status and now ends as "could not download", status 2. Not a finding.
- **Runtime install through `bash -c`** (`benchmark.cs:357-369`): the helper path and the major go in as `$1`/`$2`, quoted; sourcing `dotnet-env.sh` prints nothing; all helper and installer output goes to stderr (`>&2`), so stdout is exactly `printf '%s' "$REIHITSU_DOTNET_ROOT"`; `RunAsync` drains both streams and shows stderr only on failure, as the earlier in-app install did. `benchmark.sh` exports `REIHITSU_DOTNET_ROOT`; when the app runs without the wrapper the helper's `${REIHITSU_DOTNET_ROOT:-$HOME/.dotnet}` gives the same default the removed C# line had (and also handles an empty value, which the C# `??` did not).
- **Post-install check** in `reihitsu_install_dotnet_runtime` (`dotnet-env.sh:148`) asks the root's own `dotnet` host, which the runtime archive installs; it matches the app's decision to install (below).
- **`--no-install`**: still decided in the app before the helper is called (`benchmark.cs:329-336`); the helper's comment says the caller decides. Unchanged behavior.
- **The work directory's path** is printed with the `per-file.csv` line on success (`benchmark.cs:241`).

## Report for #1820 (`05-implementer.md`, *Report for #1820*)

Checked against `01-implementer.md`'s raw data, `04-auditor.md`, and `main`:

| Claim | Verdict |
|---|---|
| Totals and percentages, runs A-D (medians recomputed from the listed repetitions: 16.01/17.60, 15.88/16.07; +9.9 %, +1.2 %, +7.7 %, +2.1 %) | holds |
| Run C is the audit-1 run (15.17/16.34, ratio 1.05) | holds |
| Own-runtime gap: "its cause is not established"; same-runtime gap 1 %; alternating two-file test shows parity; per-file median ratio 1.04-1.05 | holds; the audit-1 attribution hint is resolved |
| Phase shares per file (47/43 %, 47/43 %, 45/44 %, 45/43 %, 46/42 %) and "88-90 % together" | holds (recomputed) |
| Whole-corpus shares (41 %, 35 %, 8 %, 7 %, 4 %, <3 %, <1 %) and "roughly 60 %" for the two calls | holds against `01-implementer.md`'s seconds (0.87, 0.71, 0.38, 0.25, 0.05 of 10.5-11.4 s; 6.4 of 10.5 s) |
| Absolute in-process ms marked as warm timings, not the CLI's cost | holds; the audit-1 hint is resolved |
| "Measured on `main` (`074661f`; no formatter change since)" | holds: `01-implementer.md` measured at `f4b9b39`, which is `074661f` plus squad files |
| Follow-up members and line numbers: `IndentationRewriter.cs:39`, `CleanupPhase.cs:33`, `TabReplacementPhase.cs:39` and `:151`, `LineEndingNormalizationPhase.cs:31` and `:45`; 31 sites | holds on the head |
| Invisible characters in the section to be posted | none |

The section is fit to post as it stands.

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| _None._ | | | | | | | |

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Whether to install a runtime (`benchmark.cs:319`) | `Microsoft.NETCore.App <major>.` line prefix in `dotnet --list-runtimes` of the `dotnet` on PATH | `reihitsu_install_dotnet_runtime`'s post-install check, `^Microsoft\.NETCore\.App <major>\.` in the install root's `dotnet --list-runtimes` (`dotnet-env.sh:148`) | Same prefix and boundary; different host by design (the app asks whether any visible host already has it, the helper whether the root it installed into has it) | agrees |
| Install failure (`reihitsu_run_dotnet_install`) | curl exit status, installer exit status | `RunAsync` exit-code check (`benchmark.cs` `RunAsync`), which turns status 2 into a `BenchmarkException` and benchmark exit 2 | agrees | agrees |
| Which output is the install root (`benchmark.cs:364`) | Whole stdout of `bash -c` | Every helper and installer write redirected to stderr; sourcing writes nothing | agrees | agrees |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Downloading and running `dotnet-install.sh` | `scripts/lib/dotnet-env.sh:54` (`reihitsu_run_dotnet_install`) only; `rg -n "dotnet-install\|dot\.net/v1"` outside `sdd/work/` | one owner |
| Install root and its default | `scripts/lib/dotnet-env.sh:15`; the app reads it back from the helper's stdout | one owner |
| `--no-install` for the runtime | `benchmark.cs:329-336` (`options.AllowInstall`); the helper leaves it to the caller, as its comment says. The SDK's `--no-install` stays in `reihitsu_ensure_dotnet` | one owner per decision |

## Assertion adequacy
_N/A — the repair adds no test; the tooling lane runs the test implementer only when the tool has tests, and `00-lead.md` records the skip. The implementer's manual checks of the helper (runtime path, a failing major, SDK path with and without `--no-install`, preinstalled SDK) are in `05-implementer.md`._

## Failure classes dismissed
- FC-001, FC-002 — the only new predicate is the helper's post-install runtime check; its span agrees with the app's (*Guard scope*).
- FC-003 to FC-014, FC-016, FC-018 to FC-030, FC-032, FC-036, FC-038 to FC-044, the class matrices — no `src/` change: no formatter phase, analyzer, code fix, syntax or trivia edit, or unsupported shape.
- FC-015 — finding 2 closed; one owner (*Policy ownership*).
- FC-017, FC-033, FC-034, FC-035 — no test added.
- FC-031 — every changed comment and summary describes the new code: `benchmark.cs` header and `InstallRuntimeAsync` summary, `ResolveCommitAsync` inline comment, both new helper headers, the README section, and the `benchmark.sh` header.
- FC-037 — no text-only proof claimed.

## Architecture findings
_None._

## Hints
- `05-implementer.md` lines 12 and 22 describe the change to finding 5 with a literal, invisible U+FEFF inside the backticks instead of the `'﻿'` the code now holds. The code is correct and the lines lie outside the report section; the file is deleted before publishing.
- When a run without `--work` fails, the kept work directory (random name, up to about 90 MB with installed tools) is not printed; only a successful run prints its path. Harmless; noted because the directory can no longer be guessed from the PID.
- Unchanged from audit 1: a corpus that 1.3.0 or later refuses as unsupported aborts the benchmark with exit 2; acceptable for the default corpus.

## Follow-up candidates
1. As stated in the report's *Follow-up candidate* and `00-lead.md` item 1 (whole-tree token and trivia replacement, quadratic in the replacement set's size), with the class enumerated as audit 1 asked. No new candidate.

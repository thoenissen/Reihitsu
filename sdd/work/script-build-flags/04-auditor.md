Role: auditor
Commit: e36870346ece8d988d66166894eff2257fb9fee2
Verdict: pass

## Scope
Full audit. Head e368703 (checked out, `git rev-parse HEAD`); base `origin/main` a971d8944cdc449278472f6ad85e403c9d48cd70, equal to `git ls-remote origin refs/heads/main` and an ancestor of HEAD. The diff (`git diff origin/main...HEAD`) is three one-line edits in `scripts/trace.sh:31`, `scripts/apply-fix.sh:51`, `scripts/lane-check.sh:107` plus the run's work docs, matching `01-implementer.md`. My prompt carried no intended fix or hoped-for verdict.

Issue #1828 (`agent-filed`, goals are proposals): "Done when" asks, per flag, for a clean-tree run with and without it, removal where the build stays warning-free and behavior unchanged, and a comment where a flag stays. The implementer's evidence covers all five flag occurrences independently (trace: `NoWarn`; apply-fix and lane-check: `Configuration=Debug` and `NoWarn`, each alone and both), from cleaned `src/*/bin`, `src/*/obj`, and the runfile cache, with an empty-`Configuration` control showing that `--verbosity quiet` does print `RH0002`/`S1125`, so a silent run is a real result. Every flag is removed; the keep-with-comment branch does not apply. Requirement addressed.

Static trace of why the removal is safe: each file-based app's own `scripts/<app>/Directory.Build.props` stops the search before the root file, so the app itself gets neither the root ruleset nor the root `binaries\` analyzers and cannot emit `RH0002`; the only `RH0002` source was a referenced repository project (`Reihitsu.Cli`, `Reihitsu.Tooling` and their references) compiled with an empty `Configuration`, which the root `Directory.Build.props` first property group now defaults to `Debug`, enabling `DocumentationFile` and `Debug.ruleset`. Without a global `Configuration`, the referenced projects evaluate that default themselves (the implementer observed `bin/Debug` with doc XML for every referenced project).

Spot check (finding-relevant, cheap): `scripts/lane-check.sh tooling --no-install` after deleting the lane-check runfile cache rebuilt the app without the flags, printed no warnings, and returned `LANE CHECK: confirmed — claimed tooling, diff needs tooling` (exit 0).

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| _None._ | | | | | | | |

## Guard scope
_N/A — the diff adds or changes no predicate, guard, or exemption; it removes command-line build properties._

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Default build configuration of repository projects when none is named | `Directory.Build.props:7-9` (`Configuration` defaults to `Debug` when empty); no script passes `Configuration=` any more (`rg -n "NoWarn\|Configuration=" scripts .github squad sdd/specs documentation AGENTS.md CLAUDE.md`). Scripts that name a configuration do so explicitly and on purpose: `scripts/build.sh:43` and `scripts/test.sh:134,136` (`-c "$configuration"`), CI workflows (`-c Release`) | one owner — ok |
| Suppression of `RH0002` in script builds | none left; the only mention is the history note `squad/notes/2026-11-17-s1125-format-build.md:43` | ok |
| Flags on the other `dotnet run` callers | `scripts/verify-text-only.sh:39` and `scripts/format.sh:52` never carried the flags; all five `dotnet run` call sites in `scripts/` now agree | ok — parity |

## Assertion adequacy
_N/A — the diff adds or changes no test; no test project covers wrapper-script build flags (`00-lead.md`, decision 1). The evidence that stands in for a test is the implementer's clean-tree matrix, which varies each flag independently, compares outputs by md5/`diff -q` against the `main` command, and includes a control that proves the observation channel (`--verbosity quiet`) shows warnings when they exist — strong enough for the guarantee claimed (no warnings, unchanged behavior)._

## Failure classes dismissed
- FC-001, FC-002, FC-003, FC-016, FC-044 — no predicate or guard in the diff.
- FC-015 — checked under Policy ownership: one owner of the default configuration.
- FC-004, FC-005, FC-019, FC-020, FC-021 — no trivia, gap, or directive handling touched.
- FC-006, FC-022–FC-027, FC-029, FC-036 — no rewrite or C# change.
- FC-007, FC-008, FC-009, FC-018, FC-030 — no code fix touched; `apply-fix.sh` drives fixes but its runner code is unchanged and its output is identical by md5.
- FC-010–FC-014, FC-028 — no formatter or analyzer change; `trace.sh` output identical to `main`.
- FC-038–FC-043 — no unsupported-shape or precheck change.
- FC-017, FC-033, FC-034, FC-035 — no test added; evidence reviewed under Assertion adequacy (the control run rules out FC-033's "harness normalized the scenario away": quiet verbosity does surface warnings).
- FC-031 — no method or script header comment describes the removed flags (headers of all three scripts read in full).
- FC-032 — evidence was gathered on the changed scripts themselves, from a clean tree, after the change.
- FC-037 — not a text-only claim; `verify-text-only.sh` reports the three scripts as compiled behavior and the lane check classifies them as tooling.

## Architecture findings
_None._

## Hints
- Without the global `Configuration=Debug`, an exported `Configuration` environment variable (MSBuild reads environment variables as properties) now decides the configuration of `apply-fix.sh` and `lane-check.sh` builds, as it already did for `trace.sh` and `format.sh`; e.g. `Configuration=Release` would build the referenced projects in Release with `TreatWarningsAsErrors`. Not a finding: no script or workflow sets that variable, the Release build is warning-free in CI, and the scripts are now consistent with each other.

## Follow-up candidates
_None._

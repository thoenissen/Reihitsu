Role: implementer
Commit: see the commit that adds this file (base 7e25bac)
Verdict: tests pass

No test project covers these wrapper-script flags (00-lead.md, decision 1), so the evidence below — clean-tree runs of each build with and without each flag — stands in for a test run. My prompt carried no intended fix or verdict beyond the issue's own "Done when".

## Changes
| File | What changed | Requirement |
|---|---|---|
| `scripts/trace.sh` | Drops `--property NoWarn=RH0002` from the `dotnet run` of `scripts/trace/trace.cs` | #1828 "Done when" (no spec covers the flags) |
| `scripts/apply-fix.sh` | Drops `--property Configuration=Debug --property NoWarn=RH0002` from the `dotnet run` of `scripts/apply-fix/apply-fix.cs` | #1828 "Done when" |
| `scripts/lane-check.sh` | Drops `--property Configuration=Debug --property NoWarn=RH0002` from the `dotnet run` of `scripts/lane-check/lane-check.cs` | #1828 "Done when" |

Every flag is removed. None is still needed, so the issue's alternative (keep a flag and add a comment saying what it suppresses) does not apply.

## Evidence

SDK 11.0.100 (`/root/.dotnet`). "Clean" means that before **every** run below, `src/*/bin`, `src/*/obj`, and the file-based apps' own build cache `/root/.local/share/dotnet/runfile` (where `dotnet run <app>.cs` writes, e.g. `runfile/trace-<hash>/bin/debug/Reihitsu.Trace.dll`) were deleted, so each run restored and built every project from scratch. Each run used the script's exact `dotnet run` line from the repository root, varying only the flags under test. Warnings were counted with `grep -E 'warning [A-Z]+[0-9]+'` over stdout and stderr.

Inputs (scratch, not committed): a trace input with expression-bodied and badly spaced members; an apply-fix fixture directory with one `internal int Get() => 42;` file, run as `RH3202 <dir>`; a lane-check stdin of `M\tscripts/trace.sh` and `M\tscripts/apply-fix.sh`, run as `--lane tooling --text-only fail`.

### Control: `quiet` prints warnings, and the old state reproduces

`--verbosity quiet` (what the scripts pass) still prints build warnings, so a quiet run with no warnings is a real result. Shown by forcing the state the issue describes — an empty global `Configuration`, which the root props default cannot override:

| Command | Warnings |
|---|---|
| `dotnet run scripts/trace/trace.cs --verbosity quiet --property Configuration= -- <input>` | RH0002 and S1125 (226 and 469 matching lines); build output lands in `bin/net11.0`, not `bin/Debug`, with no doc XML |
| same, `--verbosity minimal` | same RH0002 and S1125 |
| same, `--verbosity minimal --property NoWarn=RH0002` | S1125 only |

So `NoWarn=RH0002` only ever hid RH0002 from that empty-configuration state, and never S1125.

### trace.sh — `NoWarn=RH0002`

| Command (after `--verbosity <v>`) | Exit | Warnings | Referenced projects built into | Script output |
|---|---|---|---|---|
| `quiet --property NoWarn=RH0002 -- <input>` (as on `main`) | 0 | none | `src/{Core,Formatter,Cli}/bin/Debug`, each with its doc XML | baseline |
| `quiet -- <input>` | 0 | none | same | identical to baseline (md5) |
| `minimal --property NoWarn=RH0002 -- <input>` | 0 | none | same | trace part identical; restore lines differ only in timing and order |
| `minimal -- <input>` | 0 | none | same | same |

Unflagged `dotnet run` of a file-based app already gives the referenced projects no global `Configuration`, and the root `Directory.Build.props` default now makes it `Debug` for them; the app itself builds to `runfile/.../bin/debug`.

### apply-fix.sh and lane-check.sh — `Configuration=Debug` and `NoWarn=RH0002`, each independently

Every run at `--verbosity quiet`, as the scripts pass it.

| App | Flags | Exit | Warnings | Referenced projects built into | Output |
|---|---|---|---|---|---|
| apply-fix | both (as on `main`) | 0 | none | `src/{Core,Formatter,Analyzer,Analyzer.CodeFixes,Tooling}/bin/Debug`, each with its doc XML | baseline (`RH3202 ... fixed (1 action(s), 1 iteration(s))` under LF and CRLF) |
| apply-fix | `Configuration=Debug` only | 0 | none | same | identical (md5) |
| apply-fix | `NoWarn=RH0002` only | 0 | none | same | identical (md5) |
| apply-fix | neither | 0 | none | same | identical (md5) |
| lane-check | both (as on `main`) | 0 | none | same five projects, `bin/Debug`, with doc XML | baseline (`LANE CHECK: confirmed — claimed tooling, diff needs tooling`) |
| lane-check | `Configuration=Debug` only | 0 | none | same | identical (md5) |
| lane-check | `NoWarn=RH0002` only | 0 | none | same | identical (md5) |
| lane-check | neither | 0 | none | same | identical (md5) |

`Configuration=Debug` changed nothing observable: without it, the referenced projects already build as Debug through the root default, and the file-based apps' own `Directory.Build.props` does not need it (the app builds to `bin/debug` either way).

### The edited scripts, end to end

Each after a clean, with `--no-install`:

| Command | Exit | Warnings | Output |
|---|---|---|---|
| `scripts/trace.sh <input>` | 0 | none | identical to the `main` command's output (`diff -q`) |
| `scripts/apply-fix.sh RH3202 <fixtures>` | 0 | none | identical to the `main` command's output (`diff -q`) |
| `scripts/lane-check.sh tooling --head worktree` | 0 | none | `LANE CHECK: confirmed — claimed tooling, diff needs tooling` |

The lane-check run also built `scripts/proof/verify-text-only.cs` from clean, which never carried the flags, without warnings.

## Policy owners
`rg -n "NoWarn=RH0002|Configuration=Debug" scripts .github squad sdd documentation` — after the change no script passes either flag. The only remaining mention is history: `squad/notes/2026-11-17-s1125-format-build.md:43`, which records why this issue was filed. The owner of the default configuration is the root `Directory.Build.props` (its first property group); it was not changed.

## Production diff size
`git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'`: empty, no `src` change. Whole change: 3 files, 3 insertions, 3 deletions, all in `scripts/`. No new type.

## Deleted special cases
_N/A._

## Tests run
No test project covers the flags (see Evidence). The scripts' own runs above are the verification: every build exited 0 with no warnings, with and without each flag.

## Disputed tests or scope
_None._

## Architecture findings
_None._

## Follow-up candidates
- Out of these three scripts, observed only: a global `--property NoWarn=...` replaces each project's `NoWarn` instead of adding to it, so it would also drop the SDK's own defaults (such as `1701;1702`) in every referenced project. Moot now that no script passes it; worth knowing if a flag like it is ever reintroduced. No action proposed.

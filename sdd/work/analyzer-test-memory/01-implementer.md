Role: implementer
Commit: 9b401f7
Verdict: tests pass

## Measurements

Every measurement runs in a child memory cgroup (cgroup v1) capped below the sandbox's 13.35 GiB bash cgroup limit, so an exhaustion kills only the test host.

Wrapper: `bounded.sh <name> <GiB> <command>` (scratch area) puts the command into a fresh cgroup-v1 child of the bash cgroup with `memory.limit_in_bytes` = `<GiB>`, samples every second the RSS of every process in it, and prints the peaks. Build first: `scripts/build.sh` (Release), then each run uses `scripts/test.sh --project analyzer --no-build`.

| Run | Command | Result | Elapsed | Peak test-host RSS |
|---|---|---|---|---|
| w4 | `bounded.sh w4 10 scripts/test.sh --project analyzer --no-build` (default `Workers = 0` = 4 here) | Passed 10419 | 376 s | 9410 MiB, rising almost monotonically: 3.4 GiB at 14 s, 6.0 GiB at 60 s, 8.0 GiB at 186 s, 9.0 GiB at 283 s |
| w4_hard3g | `DOTNET_GCHeapHardLimit=0xC0000000 bounded.sh w4_hard3g 10 scripts/test.sh --project analyzer --no-build` | Passed 10419 | 327 s | 8223 MiB: a 3 GiB managed-heap cap neither fails a test nor bounds the peak |
| w4b | as w4, with `dotnet-counters collect -p <testhost> --counters System.Runtime --refresh-interval 10` attached (tool installed to the scratch area, run with `DOTNET_ROLL_FORWARD=Major`) | see below | | |

w4b counters (MiB): working set 4816 → 7175 between 22:20:04 and 22:23:14, while `gc.last_collection.memory.committed_size` stays 235–274, gen2 72 → 124, LOH < 10, `assembly.count` constant at 101, thread-pool threads 10, process threads 28. Gen2 collections run 3–4 per 10 s, so finalizable native memory is reclaimed regularly. The managed heap is about 3 % of the working set.

w4b `/proc/<testhost>/smaps` at 7.6 GiB RSS, grouped by mapping: 7194232 kB in 192 anonymous mappings of 8–64 MiB (the shape of glibc malloc arena heaps, 64 MiB each), 298720 kB in 7 anonymous mappings ≥ 64 MiB (GC regions), 190916 kB memfd (JIT double-mapping). So the growth is native heap (`malloc`), not the managed heap, not loaded assemblies, not threads.
| w4_arena2 | `MALLOC_ARENA_MAX=2 bounded.sh w4_arena2 10 scripts/test.sh --project analyzer --no-build` | Passed 10419 | 312 s | 4886 MiB (still rising: 3773 at 30 s, 4480 at 155 s, 4848 at 281 s) |
| w1 | `bounded.sh w1 10 scripts/test.sh --project analyzer --no-build -- MSTest.Parallelize.Workers=1` | **Test host OOM-killed by the 10 GiB cgroup** (`oom_kill 1`) after 3806 passed tests: "The active test run was aborted. Reason: Test host process crashed" | 245 s | 9491 MiB; 4577 at 20 s, 4853 at 81 s, 6659 at 101 s, 7865 at 122 s, 8914 at 224 s |

So the peak does **not** scale with the worker count: one worker reaches 9.5 GiB after 37 % of the tests, faster per test than four workers. It grows with the tests that have run, in jumps.
| w4c | as w4; at 7.0 GiB RSS, `gdb -q -batch -p <testhost> -x probe.gdb` calls `malloc_info(0, fopen(...))` and then `malloc_trim(0)` | see below | | |

w4c `malloc_info` at VmRSS 7374596 kB: `<total type="rest" count="3666" size="6753070994"/>` (free chunks held by glibc), `<system type="current" size="6781734912"/>`. Right after `malloc_trim(0)` returned 1: VmRSS **742616 kB**. About 6.3 GiB of the 7.0 GiB were memory the test host had already freed and glibc's arenas had not returned to the system.

**Cause.** The peak is glibc malloc retention in the analyzer test host: native allocations that the code frees, kept by the arenas. Not a leak (`malloc_trim` returns it), not the managed heap (≈ 250 MiB committed), not the worker count (one worker grows as fast per test as four).
| w4_mmap128k | `MALLOC_MMAP_THRESHOLD_=131072 bounded.sh w4_mmap128k 10 scripts/test.sh --project analyzer --no-build` | Passed 10419 | 343 s (Duration 5 m 39 s) | 5105 MiB |
| w4_mmap_arena2 | `GLIBC_TUNABLES=glibc.malloc.mmap_threshold=131072:glibc.malloc.arena_max=2 bounded.sh w4_mmap_arena2 10 scripts/test.sh --project analyzer --no-build` | Passed 10419 | 330 s (Duration 5 m 27 s) | 4452 MiB, one transient spike at 186 s; otherwise 1.9–3.0 GiB |
| envprobe | `bounded.sh envprobe 10 scripts/test.sh --project analyzer --no-build --filter "FullyQualifiedName~Formatting" -- RunConfiguration.EnvironmentVariables.GLIBC_TUNABLES=...` | Passed 5709; `/proc/<testhost>/environ` holds `GLIBC_TUNABLES=glibc.malloc.mmap_threshold=131072:glibc.malloc.arena_max=2` | 166 s | 3020 MiB |

Durations of the whole analyzer project without a setting: 6 m 11 s (w4), 5 m 30 s (w4b), 5 m 17 s (w4c); with the settings: 5 m 09 s (arena 2), 5 m 39 s (mmap), 5 m 27 s (both). The spread with and without overlaps.

Raw output of every run (`<name>.out`, `<name>.samples`, `<name>.summary`, `w4b.counters.csv`, `malloc_info.xml`, `probe.gdb`, `bounded.sh`) is in `/tmp/claude-0/-home-user-Reihitsu/f895a2f7-ddf6-5d3e-abc2-142f3d10e192/scratchpad/m/`. Machine: 4 cores, 15 GiB, no swap; the bash cgroup's own limit is 13.35 GiB (`memory.limit_in_bytes` 14338760704), which is where the earlier runs were killed.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `scripts/test.sh` | On Linux, when the caller has no `GLIBC_TUNABLES`, appends the run setting `RunConfiguration.EnvironmentVariables.GLIBC_TUNABLES=glibc.malloc.mmap_threshold=131072:glibc.malloc.arena_max=2` to every `dotnet test` call, adding the `--` separator only when the caller's arguments have none. Only the test host receives it; the build and `dotnet test` itself do not. Applies to every project of the run | Issue "Done when" 2 |
| `scripts/README.md` | New section *Test-host memory*: the setting, its scope, the measured cause, and the effect | Issue "Done when" 2, 4 |

`Workers = 0` in `src/Reihitsu.Analyzer.Test/Properties/AssemblyInfo.cs` is unchanged: the peak does not scale with the worker count (w1), so bounding workers would cost time without removing the cause.

## Policy owners
`rg -n "GLIBC_TUNABLES|MALLOC_" scripts src .github squad .claude` — only `scripts/test.sh` (the new setting) and `scripts/README.md` (its description). No other owner.

## Production diff size
`git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'`: empty. No production file, no new type.

## Deleted special cases
_N/A._

## Tests run
| Command | Result |
|---|---|
| `bounded.sh full_after 12 scripts/test.sh --no-build` (default run, with the change, 12 GiB cap) | rc 0, 454 s, peak test-host RSS 3841 MiB, `oom_kill 0`. Analyzer `Passed: 10419` (5 m 25 s), Formatter 5265, Core 738, Cli 303, ArchitectureTests 153, Tooling 133, Playground 23; no failure |
| `scripts/test.sh --project core --no-build -- MSTest.Parallelize.Workers=1` | `Passed: 738`: a caller's own `--` stays valid |
| `bash -x scripts/test.sh --project core --no-build --filter "FullyQualifiedName~CasingUtilitiesTests"` | `dotnet test ... --filter ... --no-build -- RunConfiguration.EnvironmentVariables.GLIBC_TUNABLES=...`, `Passed: 72` |
| `GLIBC_TUNABLES= bash -x scripts/test.sh --project core --no-build` | `dotnet test ... --no-build` without the setting, `Passed: 738`: a caller's value wins |
| `scripts/lane-check.sh tooling --no-install` | `LANE CHECK: confirmed — claimed tooling, diff needs tooling` |

**No slower.** Analyzer project durations overlap with and without the setting (above: 5 m 17 s – 6 m 11 s without, 5 m 09 s – 5 m 39 s with, 5 m 25 s in the full run). ArchitectureTests, the only project CI runs through `test.sh` (`.github/workflows/build.yml`): 21 s / 21 s / 21 s / 21 s alternating without (`GLIBC_TUNABLES=`) and with. CI runs the analyzer tests in `SonarCloud.yml` through plain `dotnet test`, which the change does not reach. macOS has no glibc and is skipped by the `uname` check; the repository ships no Windows scripts. The build is untouched because the setting reaches only the test host (`envprobe`).

## Disputed tests or scope
- The issue's goal "bound the worker count where the environment needs it" does not fit the measurement: one worker grows as fast per test as four and was OOM-killed at the 10 GiB cap after 37 % of the tests (w1). The change fixes what grows instead (glibc retention), which the issue also names as an option.
- Goal "No role file has to name a workaround": no role file names one today (`rg -n "chunk|one at a time|OOM|out of memory" squad/roles squad/*.md .claude AGENTS.md CLAUDE.md` finds nothing), so no role file changed. Historical run notes under `squad/notes/` mention the workaround; they are records and stay.
- Testable behavior: the change adds argument composition to `test.sh` (the run setting, the `--` separator, the caller's `GLIBC_TUNABLES` taking precedence). `TestScriptProjectSelectionTests` in `Reihitsu.ArchitectureTests` covers only project selection. A test that `test.sh` passes the setting, and does not add a second `--`, would need to run or parse the script; whether that is worth a test is the lead's decision.
- The origin of the freed native allocations was not identified. The counters show 178330 JIT-compiled methods and 36 s of JIT time in about 300 s, and `malloc_info` shows 3666 free chunks averaging about 1.8 MiB; which component makes those allocations is not established and not needed for the change.

## Architecture findings
_None._

## Follow-up candidates
- `.github/workflows/SonarCloud.yml` runs the analyzer tests through plain `dotnet test`, without this setting. GitHub's runners have 16 GiB, so the measured 9.4 GiB peak fits; if those runs ever die the same way, the same run setting applies there. Size: one argument.

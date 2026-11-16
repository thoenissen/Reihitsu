Role: implementer
Commit: cd0b4cd
Verdict: in progress

## Measurements

In progress. Every measurement runs in a child memory cgroup (cgroup v1) capped below the sandbox's 13.35 GiB bash cgroup limit, so an exhaustion kills only the test host.

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

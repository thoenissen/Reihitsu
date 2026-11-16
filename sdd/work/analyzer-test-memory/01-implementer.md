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

Role: implementer
Commit: cd0b4cd
Verdict: in progress

## Measurements

In progress. Every measurement runs in a child memory cgroup (cgroup v1) capped below the sandbox's 13.35 GiB bash cgroup limit, so an exhaustion kills only the test host.

Wrapper: `bounded.sh <name> <GiB> <command>` (scratch area) puts the command into a fresh cgroup-v1 child of the bash cgroup with `memory.limit_in_bytes` = `<GiB>`, samples every second the RSS of every process in it, and prints the peaks. Build first: `scripts/build.sh` (Release), then each run uses `scripts/test.sh --project analyzer --no-build`.

| Run | Command | Result | Elapsed | Peak test-host RSS |
|---|---|---|---|---|
| w4 | `bounded.sh w4 10 scripts/test.sh --project analyzer --no-build` (default `Workers = 0` = 4 here) | Passed 10419 | 376 s | 9410 MiB, rising almost monotonically: 3.4 GiB at 14 s, 6.0 GiB at 60 s, 8.0 GiB at 186 s, 9.0 GiB at 283 s |

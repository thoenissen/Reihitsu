Role: implementer
Commit: cd0b4cd
Verdict: in progress

## Measurements

In progress. Every measurement runs in a child memory cgroup (cgroup v1) capped below the sandbox's 13.35 GiB bash cgroup limit, so an exhaustion kills only the test host.

Role: validator
Commit: 55f9121
Verdict: #1505: pass; #1577: pass; #1576: pass

## thoenissen/Reihitsu#1505

No validator step is keyed to this issue alone; the whole-tree results are under `## Whole run`.

## thoenissen/Reihitsu#1577

No validator step is keyed to this issue alone; the whole-tree results are under `## Whole run`.

## thoenissen/Reihitsu#1576

No validator step is keyed to this issue alone; the whole-tree results are under `## Whole run`. The self-hosting filter run that covers this issue's new test is under `## Whole run`.

## Whole run

Steps:

| Step | Result | Notes |
|---|---|---|
| scripts/prepare.sh | ok | SDK 11.0.100 already present at /root/.dotnet; nothing installed |
| scripts/build.sh | ok | 0 warnings, 0 errors (Release) |
| scripts/test.sh --no-build | ok | Analyzer 10404/10404; Formatter 5251/5251; Core 738/738; Cli 303/303; ArchitectureTests 118/118; Tooling 126/126; Playground 23/23; 0 failed, 0 skipped in every project |
| scripts/test.sh --project selfhosting --filter "FullyQualifiedName~NoPhaseChangesTheFormattedOutputOfSourceFile" | ok | 1810/1810 passed, 0 failed, 0 skipped (2m 10s) |

Failures: none.

Log: `/tmp/claude-0/-home-user-Reihitsu/10d5a5b4-e12e-5eea-897d-d07c9ed4a993/scratchpad/test.log` (default run), `/tmp/claude-0/-home-user-Reihitsu/10d5a5b4-e12e-5eea-897d-d07c9ed4a993/scratchpad/selfhosting.log` (filtered self-hosting run), `/tmp/claude-0/-home-user-Reihitsu/10d5a5b4-e12e-5eea-897d-d07c9ed4a993/scratchpad/build.log`, `/tmp/claude-0/-home-user-Reihitsu/10d5a5b4-e12e-5eea-897d-d07c9ed4a993/scratchpad/prepare.log`.

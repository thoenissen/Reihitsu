# Lead — s1125-format-build

- Issue: thoenissen/Reihitsu#1800 — `agent-filed` (goals are proposals). Labels: maintenance, agent-filed. Milestone 1.4.0.
- Branch: `claude/inspiring-gauss-cv922n` (session-designated). Draft PR: #1823.
- Slug: `s1125-format-build`
- Lane: **tooling**. Reason: the report concerns the build configuration that `scripts/format.sh` uses (`Directory.Build.props`, `*.ruleset`, `scripts/**`), all tooling paths in the lane-check table; no shipped surface's behavior is in question.

## Issue summary

`scripts/format.sh` builds `src/Reihitsu.Cli` via `dotnet run` (Debug). Several sightings (issue body and comments, incl. folded-in #1801) report SonarAnalyzer `S1125` warnings for the repository's `== false` style in `FormatCommandHandler.cs`, `Diagnostics/PipelineTraceCommand.cs`, `EditScriptBuilder.cs`, while `scripts/build.sh` (Release) is clean. Both `Debug.ruleset` and `Release.ruleset` reportedly set S1125 to `None`. Cause not investigated. Done when (proposal): reproduce first; then the `format.sh` build reports no S1125 for the `== false` style, leaving that style unchanged; or the finding is documented as intended; if not reproduced, close with evidence.

## Roles

| Role | Runs | Why |
|---|---|---|
| Spec author | skip | tooling lane |
| Skeptic | skip | tooling lane |
| Test implementer | skip | no test project covers build configuration; the reproduction is a command run, done by the implementer before any change |
| Implementer | yes | reproduces, then fixes |
| Cleaner | yes | |
| Spec checker | yes | |
| Auditor | yes | |
| Validator | yes | |

## Maintainer answers

(none)

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Lane | Who reproduces in the tooling lane without a test implementer? | The implementer reproduces with `scripts/format.sh` on an unchanged tree first and returns `blocked`/not reproduced if no S1125 appears | Build configuration has no test project; reproduction is a command run | Build warnings of the repository's own scripts are internal | — |
| 2 | Implementer | Pull in the `--property NoWarn=RH0002` cleanup in `scripts/trace.sh` that the implementer suspects is now redundant? | No; follow-up candidate | Not established why the flag exists (the Debug-passing scripts carry it too); not a defect | Script flags are internal | — |

## Follow-up candidates

- `scripts/trace.sh` (and possibly `apply-fix.sh`, `lane-check.sh`) pass `--property NoWarn=RH0002`, which may have worked around the missing default configuration fixed here; check whether it is still needed. Source: `01-implementer.md`. Extended by `04-auditor.md`: the `--property Configuration=Debug` in `scripts/apply-fix.sh:51` and `scripts/lane-check.sh:107` may now repeat the default. One combined cleanup candidate.

## State

- Last step: implementer reproduced and fixed (7c8bd87, Directory.Build.props default Configuration). Lane check confirmed tooling.
- Cleaner clean, spec checker consistent, auditor pass. Audited SHA: 0b5b660 (base 1102aa5).
- Issue manager: follow-up #1828 created.
- Next: validator result, then note, delete sdd/work, trigger commit, publish PR #1823.

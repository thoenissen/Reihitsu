# Lead — cli-benchmark-profile

- Issue: thoenissen/Reihitsu#1820 — Reihitsu.Cli fixed-corpus benchmark 1.2.0 vs 1.3.0, and a per-phase profile of the per-file formatting time. Label `agent-filed`: goals are proposals.
- Branch: `claude/optimistic-albattani-opdxeu` (session-designated). Draft PR: #1859.
- Slug: `cli-benchmark-profile`
- Lane: **tooling**. Reason: the issue changes no output; the only repository change it can carry is a repeatable benchmark tool under `scripts/`. The measurement and the profile are results, not code.
- Roles run: implementer → cleaner → spec checker → auditor → validator; issue manager before publishing.
- Roles skipped: spec author, skeptic (tooling lane). Test implementer: skipped as long as the kept tool is a script outside any test project (the tooling lane runs it only when the tool has tests); if the tool's logic lands in `src/Reihitsu.Tooling`, the test implementer runs for it.
- Environment: NuGet is reachable (api.nuget.org lists Reihitsu.Cli 1.2.0 and 1.3.0). .NET 11.0.100 installed by `scripts/prepare.sh`.

## Maintainer answers

None yet.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Where does the report go, given "Done when" asks for it in the issue and only the issue manager writes to the tracker? | The implementer writes the timings and the per-phase profile into its handover; the issue manager posts them as a comment on #1820 and files one follow-up per hot spot. The PR summarizes the result. | Keeps the tracker write with its owner; `sdd/work/` is deleted before publishing. | Measurement only; no formatter, analyzer, or CLI output changes. | — |
| 2 | Claim | Keep a benchmark script? | Yes, a repeatable benchmark under `scripts/` is the run's tooling change, so the comparison can be repeated (it feeds #1803). | The issue names `scripts/` as its home; a one-off measurement cannot be re-run. | Tooling only. | — |

## Follow-up candidates

None yet.

## State

- Last step: claim, lane, `00-lead.md`.
- Next: spawn implementer.

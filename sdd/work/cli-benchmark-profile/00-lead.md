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
| 3 | Implementer | Issue premise says the CLI formats in parallel; 1.2.0 and 1.3.0 do not | Comparison runs at degree 1 without the option for these versions; goal otherwise kept | Implementer's finding | Measurement only | 8548a3a |
| 4 | Audit 1 | Five auditor findings (temp paths, duplicated installer step, README wording, warm-up comment, U+FEFF literal) and the report's "mostly noise" wording | Repair round 1, all routed to a fresh implementer; installer step gets one owner in `scripts/lib/dotnet-env.sh` (counterpart of the changed tool); the corrected report goes into `05-implementer.md` | All PR-introduced | Tooling and report text only | — |
| 2 | Claim | Keep a benchmark script? | Yes, a repeatable benchmark under `scripts/` is the run's tooling change, so the comparison can be repeated (it feeds #1803). | The issue names `scripts/` as its home; a one-off measurement cannot be re-run. | Tooling only. | — |

## Follow-up candidates

1. Indentation and cleanup phases each rewrite every token with one whole-tree `ReplaceTokens`, quadratic in token count (~75% of pipeline time; `IndentationRewriter.cs:39`, `CleanupPhase.cs:33`). Source: `01-implementer.md`. Follows up #1795; no output change. Audit 1 adds the same mechanism in `TabReplacementPhase.cs:39` (≈3% on a tab-indented large file) and `LineEndingNormalizationPhase.cs:45` (mixed line endings, unmeasured); cite phase shares and doubling behaviour, not warm in-process ms, as the CLI's cost (`04-auditor.md`).

## Report for #1820

The issue manager posts the measurements and profile of `01-implementer.md` as a comment on #1820 (decision 1).

## State

- Last step: implementer (8548a3a): no per-file regression 1.2.0 vs 1.3.0; one hot spot. Test implementer stays skipped: the tool is a script plus a file-based app outside any test project.
- Cleaner 106b50d clean; spec checker 746358d consistent; audit 1 of 746358d (main 074661f) → findings, a6fb087.
- Next: repair round 1 (implementer → cleaner → spec checker → auditor on repair).

# Lead — trace-hiding-skip

- Issue: thoenissen/Reihitsu#1724 — `agent-filed` (goals are proposals). Labels: maintenance, agent-filed. No comments.
- Slug: `trace-hiding-skip`
- Branch: `claude/admiring-shannon-b9qzyb` (session-designated)
- Draft PR: thoenissen/Reihitsu#1811
- Lane: **behavior-change**

## Lane reason

The issue says "Lane: tooling", but the trace command lives in `src/Reihitsu.Cli/Diagnostics/PipelineTraceCommand.cs` (production by the lane-check table) with tests in `src/Reihitsu.Cli.Test` (test), while its documentation, `scripts/README.md` ("Formatter pipeline trace", which lists the inputs reported as legitimate skips), is tooling. Production next to tooling admits no lane without a change doc, so the run claims behavior-change. The trace is repository tooling (`scripts/trace.sh`), not a surface of `reihitsu-format`, so its output is internal: no output question arises from it.

## Roles

Behavior-change lane: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator. None skipped.

## Maintainer answers

None yet.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Lane | behavior-change instead of the issue's "tooling" | Production code plus `scripts/README.md` needs a change doc; see Lane reason | The trace is repository tooling, not `reihitsu-format` | claim |
| 2 | Skeptic | Architecture finding: the ordered skip checks of whole-document formatting are written out in five callers (formatter's two whole-document entry points, CLI handler, Playground, trace) | Fix narrowly (trace calls the shared `HoldsHidingCarrier` predicate, adds no copy); the single shared "why is this file skipped" query becomes a follow-up candidate | The narrow fix adds no copy of the rule; the structural change touches components outside the run | Internal structure only | — |
| 3 | Skeptic | Issue goals: "tooling project" and "tooling spec" alternative | Drop the tooling-spec alternative; tests go to `Reihitsu.Cli.Test` | The trace lives in `Reihitsu.Cli` and has no spec (CLI spec lists it as a non-goal) | Trace is repository tooling | — |
| 5 | Auditor | Two non-blocking hints (caller lists in `HoldsHidingCarrier` summaries omit the trace; cancellation token not passed) | Leave as is | Summaries stay true; tree already parsed, token cosmetic; touching them would need a re-audit | Internal comments and tooling | — |
| 4 | Skeptic | Change doc wording corrections (refusal printed once, not every pass; reproduction commit label) | Route to a fresh spec author together with `Status: Accepted` | Change doc lists no visible output change, so it can be accepted now | Wording only | — |

## Follow-up candidates

- One shared query for why whole-document formatting skips a file, asked by all five callers instead of each writing out the ordered checks (02-skeptic.md, Architecture findings / Follow-up candidates).

## Audit

- Audited SHA: 147bd4f641fc902c9c420c861e4e17a1784dfdc5 — pass, no findings (08-auditor.md).

## State

- Last step: auditor (08) — pass.
- Next: validator (09), then issue manager, note, publish.

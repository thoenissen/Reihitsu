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

## Follow-up candidates

None yet.

## State

- Last step: spec author (01) — change doc written, no output questions, no follow-ups.
- Next: skeptic (02).

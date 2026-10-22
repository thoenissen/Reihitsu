Role: Lead
Commit: f52df45
Verdict: run started

## Issue

thoenissen/Reihitsu#1403 — let one squad run implement several issues (`/squad #a #b #c`) with one pull request. The issue names the files to adapt: `squad/lead.md`, `squad/README.md`, the role files under `squad/roles/` where they speak of "the issue", `.claude/commands/squad.md`, the `/squad` line in `CLAUDE.md`, and `AGENTS.md` if it describes the invocation. A single-issue prompt must keep working unchanged.

## Slug and branch

Slug: `squad-multiple-issues`. The session is bound to the branch `claude/cool-allen-rdrlfl` and may push nowhere else, so the run uses that branch instead of `claude/issue-1403-<slug>`. Claim: draft PR thoenissen/Reihitsu#1406.

## Lane

Tooling — the change is to agent instructions and the squad process (`squad/`, `.claude/`, `CLAUDE.md`, `AGENTS.md`), which the lane check classifies as tooling.

Roles run: test implementer (only if a tool with tests is affected — the implementer and test implementer decide from the diff; Markdown process files have no tests), implementer, cleaner, spec checker, auditor, validator. Skipped: spec author and skeptic, which the tooling lane does not run.

## Maintainer's decisions

Recorded in `squad/decisions/2026-10-22-1403-multi-issue-run-design.md`. In short:

1. One agent per role for the whole batch; every handover carries one section per issue.
2. If any one issue escalates (skeptic says no, not reproduced, blocking question), the whole run stops until the maintainer answers — issues do not drop out individually.
3. The run claims the narrowest lane that admits all issues, widening along the existing orders; no lane → escalate before claiming.
4. Soft limit of eight issues (more → ask the maintainer); escalation 5 is measured against the batch; one change doc may cover several issues of the same cause, otherwise one per issue.

The issue's other proposals (branch name, claim title, one `Closes #N` per issue, CI trigger commit naming, one `sdd/work/<slug>/`, rewording the "two separable concerns" rule so the lead never bundles issues on its own) are proposals, not decisions; the implementer weighs them.

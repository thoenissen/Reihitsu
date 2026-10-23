Role: Lead
Commit: f52df45
Verdict: run started

## Issue

thoenissen/Reihitsu#1403 — let one squad run implement several issues (`/squad #a #b #c`) with one pull request. The issue names the files to adapt: `squad/lead.md`, `squad/README.md`, the role files under `squad/roles/` where they speak of "the issue", `.claude/commands/squad.md`, the `/squad` line in `CLAUDE.md`, and `AGENTS.md` if it describes the invocation. A single-issue prompt must keep working unchanged.

## Slug and branch

Slug: `squad-multiple-issues`. The session is bound to the branch `claude/cool-allen-rdrlfl` and may push nowhere else, so the run uses that branch instead of `claude/issue-1403-<slug>`. Claim: draft PR thoenissen/Reihitsu#1406.

## Lane

Tooling — the change is to agent instructions and the squad process (`squad/`, `.claude/`, `CLAUDE.md`, `AGENTS.md`), which the lane check classifies as tooling.

Roles run: implementer, cleaner, spec checker, auditor, validator. Skipped: spec author and skeptic, which the tooling lane does not run; test implementer, because the change is to Markdown process files that no test covers (the lane classifier in `Reihitsu.Tooling` is not in scope).

## Maintainer's decisions

Recorded in `squad/decisions/2026-10-22-1403-multi-issue-run-design.md`. In short:

1. One agent per role for the whole batch; every handover carries one section per issue.
2. If any one issue escalates (skeptic says no, not reproduced, blocking question), the whole run stops until the maintainer answers — issues do not drop out individually.
3. The run claims the narrowest lane that admits all issues, widening along the existing orders; no lane → escalate before claiming.
4. Soft limit of eight issues (more → ask the maintainer); escalation 5 is measured against the batch; one change doc may cover several issues of the same cause, otherwise one per issue.

The issue's other proposals (branch name, claim title, one `Closes #N` per issue, CI trigger commit naming, one `sdd/work/<slug>/`, rewording the "two separable concerns" rule so the lead never bundles issues on its own) are proposals, not decisions; the implementer weighs them.

## Repair round 1

The auditor's verdict (`04-auditor.md`) has two findings, both routed to the implementer, who owns the changed process files:

1. Medium — which roles a run of several issues spawns. The maintainer decided (`squad/decisions/2026-10-22-1403-batch-roles-and-change-doc.md`): run the union of the claimed lane's roles and every issue's own-lane roles, and give each issue the duties of its own lane.
2. Low — the `/squad` command and `squad/lead.md` disagree on which arguments name issues.

The maintainer also decided to leave `sdd/changes/2026-10/establish-squad.md` unchanged.

## Repair round 2

The auditor blocked again (`08-auditor.md`, escalation 7). The maintainer chose a second repair round (`squad/decisions/2026-10-23-1403-second-repair-round.md`). Both findings go to the implementer:

1. Medium — an issue's own lane is never re-checked during the run; it widens like a single run's lane, with the roles the widening adds run for that issue and escalation 10 once the implementer has started; the README's two sentences on lane-keyed duties of the tree-checking roles must agree.
2. Low — only issue references select issues; other prompt text is context, and the lead asks when it is unclear whether an issue is meant.

## Repair round 3

The auditor blocked again (`12-auditor.md`, escalation 7). The maintainer chose a third repair round with a changed design (`squad/decisions/2026-10-23-1403-same-lane-batches.md`): every issue of a run shares one lane, the per-issue lanes and the union of roles are removed, the run widens as a whole like a single-issue run, and issues that need different lanes are split before the claim. Both findings go to the implementer:

1. Medium — the per-issue lane mechanism, replaced by the same-lane rule.
2. Low — a single issue reference inside other text is asked about too.

# GitHub PR Review

Review GitHub Pull Request **#$ARGUMENTS**.

Invoke the `gh-review` skill and follow it exactly. You run in a **Linux** cloud sandbox with no `gh` CLI — all GitHub access goes through the GitHub MCP server (`mcp__github__*`):

1. Fetch PR metadata, diff, changed files, and any linked issue via the GitHub MCP tools (`mcp__github__pull_request_read`, `mcp__github__issue_read`). The diff is not the review boundary — open counterpart files (analyzer ↔ formatter phase ↔ code fix) even when they are not in the diff.
2. Walk the 19-item review checklist. Items 2–7 are the Reihitsu invariants (Trivia preservation, Semantics & compilability, Fix convergence, Idempotency & termination, Analyzer/formatter/fix parity, Defect-class closure); the rest cover Correctness, SRP / concern leakage, Duplication, other SOLID, Coupling/cohesion, Security, Error handling, Tests, Performance, Repo conventions, Naming & docs, Scope discipline, and Issue coverage.
3. Trace the change against the skill's adversarial input corpus and check the test expectations for the change type (regression-first, idempotency double-run, convergence/FixAll, directive/comment gap cases).
4. Prefer static tracing. CI already builds and runs the full suite, so only reach for execution when a specific suspicion needs it — then install the .NET 10 SDK via `dotnet-install.sh` and run **only the targeted, filtered tests** (or a formatter double-run) that resolve it. A `high` finding should carry a concrete counterexample.
5. File follow-up issues for systemic suspicions that exceed the PR's scope via `mcp__github__issue_write` instead of dropping them.
6. Post **only high-confidence findings** as inline GitHub review comments through the MCP review tools (`mcp__github__pull_request_review_write` + `mcp__github__add_comment_to_pending_review`) — short, English, no praise, no duplicates.
7. Report back in chat using **only** the Checklist + Findings table + Verification + Hints block. Nothing else, no preamble, no closing summary.

If `$ARGUMENTS` is empty or not a positive integer, stop and ask for the PR number instead of guessing.

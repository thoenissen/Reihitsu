# GitHub PR Review

Review GitHub Pull Request **#$ARGUMENTS**.

Invoke the `gh-review` skill and follow it exactly. The Codex cloud environment is a **Linux** sandbox. Do not assume the `gh` CLI is installed; use GitHub MCP tools (`mcp__github__*`) for GitHub API operations when they are available:

1. Fetch PR metadata, diff, changed files, and any linked issue via the GitHub MCP tools (`mcp__github__pull_request_read`, `mcp__github__issue_read`). The diff is not the review boundary — open counterpart files (analyzer ↔ formatter phase ↔ code fix) even when they are not in the diff.
2. Walk the 19-item review checklist. Items 2–7 are the Reihitsu invariants (Trivia preservation, Semantics & compilability, Fix convergence, Idempotency & termination, Analyzer/formatter/fix parity, Defect-class closure); the rest cover Correctness, SRP / concern leakage, Duplication, other SOLID, Coupling/cohesion, Security, Error handling, Tests, Performance, Repo conventions, Naming & docs, Scope discipline, and Issue coverage.
3. Trace the change against the skill's adversarial input corpus and check the test expectations for the change type (regression-first, idempotency double-run, convergence/FixAll, directive/comment gap cases).
4. Prefer static tracing. CI already builds and runs the full suite, so only reach for execution when a specific suspicion needs it. Probe `dotnet --list-sdks`; if no `10.*` SDK is listed, install the .NET 10 SDK via `dotnet-install.sh`, then run **only the targeted, filtered tests** (or a formatter double-run) that resolve the suspicion. A `high` finding should carry a concrete counterexample.
5. File follow-up issues for systemic suspicions that exceed the PR's scope via `mcp__github__issue_write` instead of dropping them when GitHub MCP tools are available.
6. Post **only high-confidence findings** as inline GitHub review comments through the MCP review tools (`mcp__github__pull_request_review_write` + `mcp__github__add_comment_to_pending_review`) — short, English, no praise, no duplicates.
7. Report back in chat using **only** the Checklist + Findings table + Verification + Hints block. Nothing else, no preamble, no closing summary.

If `$ARGUMENTS` is empty or not a positive integer, stop and ask for the PR number instead of guessing.

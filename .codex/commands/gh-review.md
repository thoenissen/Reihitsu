# GitHub PR Review

Review GitHub Pull Request **#$ARGUMENTS**.

Invoke the `gh-review` skill and follow it exactly. This is a local Codex environment with the `gh` CLI available. Use it for all GitHub operations; do not use GitHub MCP tools:

1. Fetch PR metadata, diff, changed files, existing comments, and any linked issue via `gh pr view`, `gh pr diff`, `gh api`, and `gh issue view`. The diff is not the review boundary — open counterpart files (analyzer ↔ formatter phase ↔ code fix) even when they are not in the diff.
2. Walk the 19-item review checklist. Items 2–7 are the Reihitsu invariants (Trivia preservation, Semantics & compilability, Fix convergence, Idempotency & termination, Analyzer/formatter/fix parity, Defect-class closure); the rest cover Correctness, SRP / concern leakage, Duplication, other SOLID, Coupling/cohesion, Security, Error handling, Tests, Performance, Repo conventions, Naming & docs, Scope discipline, and Issue coverage.
3. Trace the change against the skill's adversarial input corpus and check the test expectations for the change type (regression-first, idempotency double-run, convergence/FixAll, directive/comment gap cases).
4. Prefer static tracing. CI already builds and runs the full suite, so only reach for execution when a specific suspicion needs it; then run **only the targeted, filtered tests** (or a formatter double-run) that resolve the suspicion. A `high` finding should carry a concrete counterexample.
5. File follow-up issues for systemic suspicions that exceed the PR's scope with `gh issue create` instead of dropping them.
6. Post **only high-confidence findings** as inline GitHub review comments through `gh api` review endpoints — short, English, no praise, no duplicates.
7. Report back in chat using **only** the Checklist + Findings table + Verification + Hints block. Nothing else, no preamble, no closing summary.

If `$ARGUMENTS` is empty or not a positive integer, stop and ask for the PR number instead of guessing.

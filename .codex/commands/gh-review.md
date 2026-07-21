# GitHub PR Review

Review GitHub Pull Request **#$ARGUMENTS**.

Invoke the `gh-review` skill and follow it exactly. It supports Linux cloud and local Windows environments. Use the authenticated `gh` CLI for all GitHub operations:

1. Confirm authentication with `gh auth status`, then fetch PR metadata, diff, changed files, existing comments, and any linked issue via `gh pr view`, `gh pr diff`, `gh api`, and `gh issue view`. The diff is not the review boundary — open counterpart files (analyzer ↔ formatter phase ↔ code fix) even when they are not in the diff.
2. Walk the 19-item review checklist. Items 2–7 are the Reihitsu invariants (Trivia preservation, Semantics & compilability, Fix convergence, Idempotency & termination, Analyzer/formatter/fix parity, Defect-class closure); the rest cover Correctness, SRP / concern leakage, Duplication, other SOLID, Coupling/cohesion, Security, Error handling, Tests, Performance, Repo conventions, Naming & docs, Scope discipline, and Issue coverage.
3. Trace the change against the skill's adversarial input corpus and check the test expectations for the change type (regression-first, idempotency double-run, convergence/FixAll, directive/comment gap cases).
4. Prefer static tracing. CI already builds and runs the full suite, so only reach for execution when a specific suspicion needs it — run `dotnet --list-sdks` to confirm the preinstalled .NET 10 SDK, then run **only the targeted, filtered tests** (or a formatter double-run) that resolve it. Do not install an SDK, modify `PATH`, or otherwise change the environment. A `high` finding should carry a concrete counterexample.
5. Keep the review self-contained: never search for or create GitHub issues. Every confirmed finding stays on the current PR, even when it is systemic, pre-existing, or broader than the diff. Do not demote a confirmed finding to a hint because of scope.
6. Submit **every high-confidence finding** in one GitHub review through `gh`: use inline comments when changed-line anchors exist and the same review's summary body otherwise. Never use a new issue or separate PR comment as the destination for a finding. Keep uncertain hints outside the Findings table.
7. Report back in chat using **only** the Checklist + Findings table + Verification + Hints block. Nothing else, no preamble, no closing summary.

If `$ARGUMENTS` is empty or not a positive integer, stop and ask for the PR number instead of guessing.

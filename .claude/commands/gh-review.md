# GitHub PR Review

Review GitHub Pull Request **#$ARGUMENTS**.

Invoke the `gh-review` skill and follow it exactly:

1. Fetch PR metadata, diff, and any linked issue via `gh` CLI.
2. Walk the 13-item review checklist (Correctness, SRP / concern leakage, Duplication, other SOLID, Coupling/cohesion, Security, Error handling, Tests, Performance, Repo conventions, Naming & docs, Scope discipline, Issue coverage).
3. Post **only high-confidence findings** as inline GitHub review comments — short, English, no praise, no duplicates.
4. Report back in chat using **only** the Checklist + Findings table + Hints block. Nothing else, no preamble, no closing summary.

If `$ARGUMENTS` is empty or not a positive integer, stop and ask for the PR number instead of guessing.

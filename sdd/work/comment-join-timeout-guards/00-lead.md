# Lead — comment-join-timeout-guards

- Issue: thoenissen/Reihitsu#1846 — `agent-filed` (goals are proposals)
- Slug: comment-join-timeout-guards
- Branch: claude/squad-1846-kehmy2
- Draft PR: thoenissen/Reihitsu#1849
- Lane: tests — the issue asks for test guards that fail on a reintroduced endless loop, for behavior that already exists (the loop is fixed on `main`). If the outcome needs a production change (the loop observing the token), the lane widens to bug fix and adds the implementer and the auditor.
- Roles run: skeptic (premise), test implementer, cleaner, spec checker, validator. Skipped: spec author (the run adds no spec), implementer and auditor (tests lane; added on widening).

## Maintainer answers

None yet.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Skeptic | The skeptic shows the attribute-placement loop already observes the token (`AttributeTargetFormattingRewriter.ApplyPlacement`, per pass) and that both guards fail after 30 s with the loop restored — the issue's "Done when" holds on `main`. Implement either remedy the issue proposes? | Drop both remedies (agent-filed goals). Instead make the coupling durable: comment-only edits stating that the two guards' cooperative timeout relies on the per-pass token check, at the two tests and at the token check. | The only real risk is someone removing the token check unaware that the guards depend on it; an outside guard would duplicate what a run-wide hang timeout (follow-up) covers more cheaply. | Tests and a source comment only; no formatter output, diagnostic, fix, rule page, or CLI behavior changes. | — |
| 2 | Skeptic | `Closes` or `Refs`? Escalation 4? | `Closes #1846`: the issue is a maintenance concern, not an observed bug, and its purpose — guards that fail within their limit on a reintroduced loop — is met and demonstrated. No escalation 4: no reported behavior to reproduce. | The issue's own "Done when" is met. | No visible output. | — |
| 3 | Skeptic | Roles after the narrowing | The diff becomes comment-only: spec author writes the comments (docs-lane text work), spec checker checks, `scripts/verify-text-only.sh` proves it; test implementer, implementer, cleaner, auditor skipped (nothing compiled changes); validator skipped unless the proof fails. The claimed tests lane stays (a claim may be wider than the diff). | Nothing compiled changes. | No visible output. | — |

## Follow-up candidates

- Tooling: CI sets no `timeout-minutes`, and neither `scripts/test.sh` nor the workflows pass a hang timeout (e.g. `dotnet test --blame-hang-timeout`); a test that hangs in code without a token check keeps the CI job running until GitHub's default job limit. Unverified whether the option works with the repository's MSTest runner setup. (01-skeptic.md)

## Worklist / next step

- Done: skeptic (`01-skeptic.md`, verdict: do not make the change).
- Next: spec author writes the comment-only edits (`02-spec-author.md`), then verify-text-only, spec checker.

# Lead — issue 1805

- Issue: thoenissen/Reihitsu#1805 — FMT-PIPE-027 per-phase stability check in the analyzer and CLI full-run idempotency tests. Label `agent-filed`: its goals are proposals.
- Slug: `phase-check-shared`
- Branch: `claude/intelligent-hopper-li5bmz` (session-designated); draft PR thoenissen/Reihitsu#1824.
- Lane: **tests** — the issue adds a check to tests of existing behavior (FMT-PIPE-027); no production behavior changes. The issue's own wiring needs a `.csproj` change (a reference or a shared project), which the lane check classifies as tooling; expect a widening to `tooling` once that path is in the diff.
- Roles (tests lane): skeptic (premise, reachability, and the reference-vs-shared-project structure), test implementer, cleaner, spec checker, validator. Spec author skipped: the run adds no spec unless the skeptic finds a spec gap. Implementer and auditor skipped in the tests lane; a widening to tooling adds them.
- Maintainer clarifications: none.

## Maintainer answers

None yet.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Skeptic | Keep the CLI half of the issue? | Drop it; the PR states the departure and still closes the issue | The CLI writes the formatter's output unchanged; the check there would only re-test the formatter on one fixture and can never find a CLI defect (01-skeptic) | Test-only change; no surface output changes | – |
| 2 | Skeptic | Reference vs. shared test-support project | `ProjectReference` from `Reihitsu.Analyzer.Test` to `Reihitsu.Formatter.Test` | Precedent of `Reihitsu.SelfHosting.Test`; a shared project needs a new `InternalsVisibleTo` in production code; failure text keeps its one owner | Test wiring only | – |
| 3 | Skeptic | How the base runs the check | Text overload with the line ending and parse options the base formatted with; check the output under both line endings; skip when the precheck refused the input; still check generated/hidden-code output | Skeptic's recommendations 1–3 | Test-only | – |
| 4 | Skeptic | Adjacent: RH5604 `VerifyFormatterEntryPointsAndIdempotency` | Take it in if it is the one-line call the skeptic describes | Same mechanism, same file set, gets the check | Test-only | – |

## Follow-up candidates

None yet.

## Worklist / next step

- Claimed (commit "Claim issue #1805"), prepare.sh green.
- Skeptic done (01-skeptic.md): proceed narrower.
- Next: test implementer; then lane check (expect widening to tooling for the .csproj).

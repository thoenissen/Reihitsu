# Lead — phase-check-helpers

- Branch: `claude/trusting-pasteur-mor1wo` (session-designated)
- PR: thoenissen/Reihitsu#1794 (draft claim)
- Slug: `phase-check-helpers`

## Issues

| Issue | agent-filed | Lane | Reason |
|---|---|---|---|
| thoenissen/Reihitsu#1505 | yes | tests | Formatter test helpers (`ObservedPipeline`, `FormatterPhaseTestsBase`) should parse through `SecondPassInput.Create`; test-project code only. A comment (2026-11-16) says `ObservedPipeline.Run` already uses the fixture's parse options; `FormatterPhaseTestsBase.ApplyPhase` still does not, and neither goes through `SecondPassInput.Create`. |
| thoenissen/Reihitsu#1577 | yes | tests | Seven idempotency test classes outside `FormatterTestsBase` should run `PhaseStabilityCheck` (FMT-PIPE-027) or state why not; test coverage only. |
| thoenissen/Reihitsu#1576 | yes | tests | `FormatterSelfHostingTests` should run `PhaseStabilityCheck` over its scanned sources, tagged `FMT-PIPE-027`; test coverage only. |

Shared mechanism: formatter test helpers around `PhaseStabilityCheck` / `SecondPassInput` in `src/Reihitsu.Formatter.Test/Helpers`.

## Roles

Lane `tests`: spec author (only if the run adds or changes spec text — e.g. a covering-test reference for FMT-PIPE-027; otherwise skipped), skeptic (spec against the code, premise and goals), test implementer, cleaner, spec checker, validator. Implementer and auditor are not part of the tests lane.

Note: the self-hosting scans are not run by `scripts/test.sh` by default (CI runs them, `--project selfhosting`); #1576 changes them, so the validator also runs `--project selfhosting` once.

## Maintainer answers

(none)

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Start | Batch of three issues | One run, lane tests | All three change formatter test helpers around the per-phase/second-pass check | Test code only | — |

## Follow-up candidates

(none)

## State

- Last step: claim, prepare.sh green.
- Next: skeptic (01).

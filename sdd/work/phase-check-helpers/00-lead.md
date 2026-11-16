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
| 2 | Skeptic | #1505 scope | Narrow: route only `ObservedPipeline.Run` through `SecondPassInput.Create` and update `SecondPassInput`'s summary of its users (FC-031); leave `FormatterPhaseTestsBase.ApplyPhase` as is. "Done when" read as every helper under `Helpers/` that builds a pass's root and context from fixture parse options | `ApplyPhase` takes no parse options, no subclass needs any, and `Create` would change the context's line ending its subclasses choose; the remaining defect is the duplicated rule in `ObservedPipeline` | Test code only | — |
| 3 | Skeptic | #1577 class list | Add the per-phase check to the five classes that assert idempotency of a full run (LineEndingIndependence, LineEnding, SingleLineTypeBraceWithLeadingUsing, DirectivePreservation, DocumentationCommentCollapseGuard) and to two classes the issue missed (`RawStringOtherLineTerminatorAlignmentTests`, `FormattingPipelineTests.ExecuteAlreadyFormattedCodeNoChanges`); leave `LineBreakRewriterTests` and `UsingDirectiveOrderingRewriterTests` unchanged, without a summary sentence | Those two run one phase or assert no idempotency, so the check does not apply; a sentence about an absent check is noise. The two additions are the same mechanism (adjacent, tested by construction) | Test code only | — |
| 4 | Skeptic | How the check is asserted outside `FormatterTestsBase` | One session-free assertion in `PhaseStabilityCheck` with the same failure message as the session check, using the output's detected line ending and the fixture's parse options | One statement of the failure text instead of seven copies | Test code only | — |
| 5 | Skeptic | Architecture finding: private copies of `FormatterTestsBase.ApplyRule` in three regression classes and `FormattingPipelineTests` | Fix narrowly (call the shared assertion); consolidation becomes a follow-up candidate | Test-only structure, outside the issues' goals | Test code only | — |
| 6 | Skeptic | #1576 needs `PhaseStabilityCheck` from `Reihitsu.SelfHosting.Test`, which does not reference `Reihitsu.Formatter.Test` | Widen the run's lane tests → tooling; `ProjectReference` SelfHosting.Test → Formatter.Test, call public `FindChangingPhases`; a new test method tagged FMT-PIPE-027; skip outputs the formatter did not format (unsupported, FMT-PIPE-033) | Only way to reach the helper without touching shipped source; tooling adds implementer and auditor | Test/project files only | — |

## Follow-up candidates

- Consolidate the private copies of `FormatterTestsBase.ApplyRule` in three regression test classes and `FormattingPipelineTests` into the base (01-skeptic, architecture finding).
- `scripts/trace.sh` is committed with mode 100644, so `scripts/trace.sh` as documented in `AGENTS.md` fails with "Permission denied"; `bash scripts/trace.sh` works (01-skeptic).

## State

- Lane widened to tooling (decision 6): roles now test implementer → implementer (project reference / anything tests need beyond test code) → cleaner → spec checker → auditor → validator (+ `--project selfhosting`).
- Last step: skeptic (01) done.
- Next: test implementer (02).

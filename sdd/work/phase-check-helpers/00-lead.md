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
| 7 | Implementer | `scripts/README.md` sentence naming the per-phase check in the self-hosting scan | Accept | Keeps the scripts documentation true for the scan it describes | Repository docs only | 5652ed0 |
| 8 | Implementer | `Reihitsu.Formatter.Test.dll` now in the self-hosting test output; SonarCloud collects coverage per project | Accept without change | Sonar classifies test projects as test code, so the assembly adds no coverage target; each project still runs its tests once | Tooling only | 5652ed0 |
| 9 | Audit 1 | Repair worklist (06-auditor #1–#5, all PR-introduced, test code) | Repair round 1, one fresh test implementer: #1 make the tree-form test fail when parse options are dropped; #2/#3 limit the summaries/message to what holds; #4 skip on `ReferenceEquals(result.Output, syntaxTree)` (covers all four unchanged-return reasons); #5 collect LF and CRLF failures and fail once, like the sibling methods | All are the run's own test code; #4 by the cause rather than by enumerated reasons | Test code only | — |
| 10 | Repair 1 | Test implementer added `FormatSyntaxTreeReturnsANewTreeForAnInputItFormatsWithoutChangingIt` to pin the new-tree promise the self-hosting skip (`ReferenceEquals`) relies on | Accept | Protects the skip in decision 9 from silently skipping formatted files | Test code only | 93fc307 |
| 11 | Audit 2 | Auditor hints: `PhaseStabilityCheck.Verify` repeats `FailOnChangingPhases`; `ObservedPipelineTests.cs` lacks a BOM | Leave as is | Not findings; a compiled change would re-run cleaner, auditor and validator for a three-line duplication in test code; the BOM is not enforced (256 files lack it) | Test code only | — |

## Follow-up candidates

- Consolidate the private copies of `FormatterTestsBase.ApplyRule` in three regression test classes and `FormattingPipelineTests` into the base (01-skeptic, architecture finding).
- `FormattingPipelineTests.ExecuteAlreadyFormattedCodeNoChanges` compares two passes after trimming trailing newlines, weaker than whole-text idempotency (FC-017); fold into the `ApplyRule` consolidation candidate (02-test-implementer).
- `Reihitsu.Analyzer.Test/Base/FormatterTestsBase{TAnalyzer}.cs` (159 classes) and `Reihitsu.Cli.Test` `FormatCommandHandlerIntegrationTests.ExecuteAsyncFormattingIsIdempotent` assert full-run idempotency without the per-phase check (FMT-PIPE-027); structural cause: the check lives in one test project's helpers while idempotency is asserted in three projects (06-auditor).
- The build inside `scripts/format.sh` prints S1125 warnings in `src/Reihitsu.Cli/Diagnostics/PipelineTraceCommand.cs` (untouched by the run; `scripts/build.sh` Release reports none) (04-cleaner).
- `scripts/trace.sh` is committed with mode 100644, so `scripts/trace.sh` as documented in `AGENTS.md` fails with "Permission denied"; `bash scripts/trace.sh` works (01-skeptic).

## State

- Lane widened to tooling (decision 6): roles now test implementer → implementer (project reference / anything tests need beyond test code) → cleaner → spec checker → auditor → validator (+ `--project selfhosting`).
- Implementer (03) at 5652ed0: selfhosting new test green on 1810 rows (+2m22s); only `WorkingDocumentsDirectoryContainsNoFiles` fails while `sdd/work/` exists (expected).
- Spec checker (05) consistent at 1b8a1bd.
- Audit 1 on 1b8a1bd (main 372daee): findings #1–#5 → repair round 1 (decision 9).
- Repair (07) at 93fc307; cleaner (08) clean; spec checker (09) consistent.
- Audit 2 (repair only) PASS on e6e041f; audited SHA bd4e5a7 (handover commit). main 372daee.
- Next: validator (11), then lane check, issue manager, note, delete sdd/work, trigger commit, PR.

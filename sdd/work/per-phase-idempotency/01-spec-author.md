Role: spec author
Commit: 237bde1
Verdict: change doc written

## Changed requirements

- FMT-PIPE-027 — No phase changes the formatter's output — added (`sdd/specs/formatter/pipeline.md`; ledger `sdd/specs/requirement-ids.txt`; `Uses` gains FMT-USING-010).
- FMT-PIPE-004 — Idempotency — clarified: one sentence pointing to FMT-PIPE-027; rule and `Why:` unchanged.
- Change doc: `sdd/changes/2026-11/per-phase-idempotency.md`, `Status: Draft`. Visible output changes: none.

No rule page or user documentation changes; no Markdown outside `sdd/` mentions the exception list or the phase-level check.

## Statements of the issue checked against `main` (`fe24dcc`)

- "The exception list holds fixtures that still fail" — moot: `PhaseReversalExceptionList.Entries` is already `[]`. What is left is the machinery that serves it: `PhaseReversalExceptionList`, `PhaseReversalCauses` (one cause, `LineStartIndentation`, referenced by no entry), `PhaseReversalCause`, `PhaseReversalEntry`, `PhaseReversalLineEndings`, the unreached-entry reporting of `PhaseStabilityRecorder`/`PhaseStabilitySession`/`PhaseStabilityCheck.EndTest`, and the `entries` parameter of `PhaseStabilityCheck.OverrideForCurrentTest`, plus the `PhaseStabilityCheckTests` and `DataRowKeyTests` that use them. Established with `rg PhaseReversal src`.
- "Today the using phase removes the blank line above the comment and the blank-line phase puts it back" — moot since thoenissen/Reihitsu#1497 (FMT-USING-010). `scripts/trace.sh` on the issue's example (formatted form, no final newline) reports `Stable after pass 1 (0 phase changes observed)`; with a final newline only `CleanupPhase` removes it in pass 1.
- "Tag the per-phase check with FMT-PIPE-004" — a tag sits on a `[TestMethod]`, not on a helper; the change doc names the tagged tests instead, under FMT-PIPE-027 (see Internal choices).

## Examples for the tests

- FMT-PIPE-027, example 1 (the issue's using block) and example 2 (`Call(1,` / `2);` aligned under the first argument): each is formatter output that every production phase, run alone, returns unchanged, under LF and CRLF. Both were confirmed stable in one pass with `scripts/trace.sh`.
- FMT-PIPE-027, first qualifier: `class C{void M(){int x=1;}}` is formatted and no phase changes its output; `class C{void M(){int x=1}}` is returned unchanged by FMT-PIPE-005 and is outside the requirement (no assertion about phases run alone on it).
- The change doc's *Tests* decision: tag the pipeline test over the examples and the `PhaseStabilityCheckTests` that show the check runs the production phases in order and finds a changing phase.

## Output questions

_None._

## Internal choices

- New requirement FMT-PIPE-027 instead of extending FMT-PIPE-004 as the issue proposes — the two have different reasons (one decision per heading, `sdd/README.md`); alternative: append the property to FMT-PIPE-004 and tag the check's tests with FMT-PIPE-004.
- Node-level entry points excluded, as FMT-PIPE-021 excludes them — their phases receive a detached root and FMT-PIPE-024 facts, which a phase run alone on their output text would not; alternative: cover them and add a node-level per-phase check.
- Comparison as text, matching the existing check; alternative: compare trees.
- Change doc kept although no production code and no visible output change: the requirement forbids reversals the previous state of the specs and tests tolerated (the exception list); alternative: treat it as a clarification without a change doc (the diff would then fall in the tests lane).

## Non-goals

- FMT-PIPE-004 keeps its rule and its tests (`FormatterSelfHostingTests`).
- No per-phase check in the self-hosting scans, the CLI, or the analyzer code-fix tests.
- No change to `scripts/trace.sh` or to any phase.

## Follow-up candidates

- Running the per-phase check in the self-hosting scans (`FormatterSelfHostingTests`) would apply FMT-PIPE-027 to the repository's own sources. Test coverage only; no output in question.

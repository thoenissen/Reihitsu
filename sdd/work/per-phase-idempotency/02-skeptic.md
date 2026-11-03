Role: skeptic
Commit: 1e7c44a
Verdict: proceed narrower

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Premise

The issue asks for a requirement, not a bug fix: no visible output changes, and none is in question. Its motivation holds: FMT-PIPE-004 compares whole passes, so two phases that undo each other pass it. No current requirement forbids two phases owning one decision when neither predicts the other. FMT-PIPE-001 and FMT-PIPE-025 forbid predicting or adjusting for a later phase. A plain insert in one phase and removal in another is neither, so FMT-PIPE-027 is the first requirement to rule it out.

The issue's statements about today's code, checked on `fe24dcc` (= `1e7c44a` for `src/`):

- **Exception list.** It is already empty: `PhaseReversalExceptionList.Entries = []`. The last entries had cause `LineStartIndentation` and came out in `781d8cf` (#1565). The cause object is still declared, and no entry uses it.
- **Using/blank-line reversal.** It is gone; `e3842c8` (#1547, FMT-USING-010) removed it.
- **Suite status.** The suite is green with no list: `scripts/test.sh --project formatter` gives 4411 passed, 0 failed. So every fixture that reaches `PhaseStabilityCheck.Verify` already meets FMT-PIPE-027 under LF and CRLF.
- **Beyond the fixtures.** I formatted a set of probes twice with `scripts/format.sh`, then ran `scripts/trace.sh --passes 1` on each. Every probe gave `Stable after pass 1 (0 phase changes observed)`. When a whole pass changes nothing, each phase ran alone on the output, so this result is FMT-PIPE-027. The probes were:
  - lambdas of every kind as arguments: simple, parenthesized, `async`, `static`, attributed, with explicit return type, anonymous methods, curried;
  - lambda bodies as block, switch expression, object creation, array, and collection expression;
  - lambdas as first argument, later argument, on a continuation line, in chains, in `new`/indexer/raw-string/non-raw interpolation holes, and in dictionary initializers;
  - the shapes of the five removed `LineStartIndentation` entries (minimal-API attributed lambda, switch expression in a chained lambda, wrapped lambda parameter list);
  - a trivia-heavy class with a using block with comment and `#if`, `#region`, `#pragma`, doc comments, switch sections, wrapped ternary/binary/argument lists, and expression bodies with comments;
  - each probe under LF and CRLF.
- **Repository sample.** I ran the same trace over the 75 largest C# files under `src/` (45 production, 30 test). All 75 report 0 phase changes. The fixtures were in the scratchpad and are deleted.

Reachability: the requirement covers `FormatDocumentAsync`/`FormatSyntaxTree`, which whole-document formatting and the CLI use. The node-level exclusion matches FMT-PIPE-021, and I agree with it (see Internal choices).

The spec's definition of the input matches what a second pass sees: the output parsed with the original parse options, plus the line ending FMT-PIPE-007 detects in it. `LineEndingUtilities.DetectEndOfLine` counts only end-of-line trivia and XML newlines, which the formatter has already normalized. Detection on the output therefore gives the line ending the second pass of FMT-PIPE-004 uses, and the two requirements check the same input. The cited requirements say what the examples claim: FMT-USING-010 and FMT-BLANK-011 for example 1, FMT-INDENT-012 for example 2.

## Mechanism and candidates

_N/A._ No defect. The candidate set is the production phase list: `FormattingPipeline.CreatePhases`, with `PreparationPhase` expanded into its `SubPhases`. `PhaseStabilityCheck.CreateProductionPhases` already reads it through reflection, and `CheckRunsTheProductionPhasesInPipelineOrder` checks it against the observer.

## Breaking inputs

No input breaks FMT-PIPE-027 today (see Premise). Classes that reach the change:

- **FC-010 / FC-012** (phase not a no-op on its own output, often only on CRLF; neighbouring phases undo each other). These are what the requirement forbids. The check already runs every phase, sub-phases included, under LF and CRLF. No failing member found.
- **FC-017** (assertion weaker than the guarantee). The new pipeline test over the examples should run both line endings. It should also first assert that the example is the formatter's output, as `AssertIsFinalOutput` does today, so the test cannot pass on text that is not formatter output.
- **FC-033** (the harness normalizes the scenario away). The test base formats through `FormattingPipeline.Execute`, not the public entry points. `ApplyRule` even formats inputs with syntax errors, which `FormatDocumentAsync` returns unchanged (FMT-PIPE-005). So the check also runs on outputs the requirement excludes. That is stricter than the spec, not weaker, and it is green. See Internal choices.
- **FC-031** (stale comments). Removing the list leaves doc comments that describe it. These places need updating or removal:
  - `FormatterTestsBase`: three `<see cref="PhaseReversalExceptionList"/>` mentions, plus the summaries of `BeginPhaseStabilityCheck` and `EndPhaseStabilityCheck`;
  - `PhaseStabilityCheck`: the summaries of `BeginTest`, `EndTest`, `Verify`, and `OverrideForCurrentTest`;
  - `PhaseStabilityRecorder` and `PhaseStabilitySession`;
  - `DataRowKey`'s failure message;
  - `SecondPassInput`'s summary, which still names the check (keep it if the check stays).

Inputs the test implementer must cover: FMT-PIPE-027 examples 1 and 2, each asserted as formatter output and then per phase, under LF and CRLF. The first qualifier's formatted case `class C{void M(){int x=1;}}` too, in its formatted form.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| Formatter phases and pipeline (`src/Reihitsu.Formatter`) | Must stay. No production change is needed or justified. |
| Formatter test helpers (`Reihitsu.Formatter.Test/Helpers`) | Must change. Remove `PhaseReversalExceptionList`, `PhaseReversalCauses`, `PhaseReversalCause`, `PhaseReversalEntry`, `PhaseReversalLineEndings`, and the entry and unreached-entry logic in `PhaseStabilityRecorder`/`PhaseStabilitySession`/`EndTest`. Also remove what serves only entry matching. The spec author's list omits this part: `DataRowKey`, the test key (`FormatTestName`/`FormatTestKey`), `ComputeFingerprint`, and the `[TestInitialize]`/`[TestCleanup]` identity plumbing, unless the override mechanism for the check's own tests still needs a session. |
| `PhaseStabilityCheckTests`, `DataRowKeyTests` | Must change. Tests of entries, staleness, data-row keying, and causes go. These tests stay: production order, no change in formatted source, finds the changing phase, catches a write-back, parse options, the helpers run the check. |
| Formatter spec `pipeline.md` | Must change (done in `1e7c44a`). |
| CLI, analyzers, code fixes, Fix All, `Reihitsu.Core` | Not involved. |
| Rule docs, `documentation/` | Not involved. No user documentation mentions the check. |
| Self-hosting scans | Not involved (follow-up candidate, below). |

## Simpler option

Treat FMT-PIPE-027 as a clarification without a change doc. `sdd/README.md`, *When a change doc is required*, lists as not required "a clarification — the spec gains a requirement that writes down behavior that already exists". Here the list is empty, the suite is green, and the probes and the 75-file sample all hold. No phase, no output, and no test outcome changes.

The spec author keeps the doc because the requirement "forbids reversals the previous state of the specs and tests tolerated". But the tolerance lived only in test infrastructure that was already empty, never in a spec. Dropping the doc would leave a diff of `sdd/specs` plus the test project only, and the lane check derives the tests lane from such a diff. Keeping the behavior-change claim stays admissible, since a claim may be wider than the diff. Either way is internal and changes no visible output; I recommend dropping the doc (see Internal choices).

## Output questions

_None._

## Internal choices

1. **Change doc or clarification.**
   - **A:** keep `sdd/changes/2026-11/per-phase-idempotency.md` and the behavior-change claim.
   - **B:** delete the doc, and move into the PR description the reasons the spec does not already carry. The node-level exclusion and the text-only comparison are already qualifiers in FMT-PIPE-027.

   **Recommendation: B.** `sdd/README.md` names this exact case as not requiring a doc, and the issue's demand for a doc is an `agent-filed` proposal. Neither option changes anything visible.

2. **Which tests carry `FMT-PIPE-027`.** The change doc tags "the tests of the phase-level stability check that show it runs the production phases in pipeline order and finds a phase that changes the text". `CheckFindsThePhaseThatChangesTheSource` and `CheckCatchesAPhaseWhoseChangeALaterPhaseWritesBack` test the test helper, partly with a fake `IndentationStrippingPhase`; they verify nothing the formatter does. `sdd/README.md`, *Tests and requirements*, tags a test with the requirement it covers.

   **Recommendation:** tag the new pipeline test over the examples, `CheckFindsNoChangeInFormattedSource`, and `CheckRunsTheProductionPhasesInPipelineOrder`. The last one establishes "every phase, each sub-phase included". Leave the helper-mechanics tests untagged. `CheckFindsNoChangeAboveCommentLedUsingDirectiveOfTheSameGroup` is example 1 without `class C;`; it may gain the tag next to FMT-USING-010 instead of getting a duplicate test.

3. **Entry point of the new example test.** The requirement names `FormatDocumentAsync`/`FormatSyntaxTree`, while the base and `AssertIsFinalOutput` use `FormattingPipeline.Execute`.

   **Recommendation:** for the examples, establish the "is formatter output" leg with `ReihitsuFormatter.FormatSyntaxTree`, so the tagged test checks what the requirement names. Its phases-alone leg can stay on `PhaseStabilityCheck.FindChangingPhases`. Either way the result is the same for these examples.

4. **The check on syntax-error inputs.** The base's `ApplyRule` formats such inputs and `Verify` checks them, although FMT-PIPE-027 says nothing about them.

   **Recommendation:** leave it. It is stricter than the spec, green today, and untagged. Mention it in the helper's summary so a future failure there is not read as an FMT-PIPE-027 violation.

5. **Separate requirement vs. extending FMT-PIPE-004.** I agree with the spec author: the two reasons differ (an endless diff for `--check` users vs. two owners of one decision).

6. **Node-level exclusion.** I agree: same reason as FMT-PIPE-021. Node-level output is not a parsed text, and the same phases are covered through the document entry points.

## Issue goals

- **"FMT-PIPE-004 also requires that every phase … leaves it unchanged."** Changed: FMT-PIPE-004 gets a pointer, and the property is FMT-PIPE-027. Nothing visible changes.
- **"The per-phase check carries the `FMT-PIPE-004` tag."** Changed: the covering tests carry `FMT-PIPE-027`. A tag sits on a `[TestMethod]`, not on a helper. Nothing visible changes.
- **"The exception list is deleted, so every formatter test that checks idempotency checks it per phase as well."** The deletion stays. The consequence is false: seven test classes check idempotency without deriving from `FormatterTestsBase`, so they never run the check, and deleting the list does not change that. They are `LineBreakRewriterTests`, `ReihitsuFormatterLineEndingIndependenceTests`, `ReihitsuFormatterLineEndingTests`, `SingleLineTypeBraceWithLeadingUsingTests`, `DirectivePreservationTests`, `DocumentationCommentCollapseGuardTests`, and `UsingDirectiveOrderingRewriterTests` (`FormatterPhaseTestsBase`). The requirement does not need them: the base already runs the check on its 4000+ fixture executions. Drop that clause. Nothing visible changes.
- **"A change doc … is required."** A proposal. See Simpler option and Internal choice 1.
- **"Today the using phase removes the blank line … in every pass."** Stale since #1547. The example's expected output is unchanged and correct.
- **"Pipeline test over the spec examples."** Keep.

## Architecture findings

_None._ The one shared decision the issue worried about (the blank line above a comment-led same-group using) has one owner since FMT-USING-010. No reversal exists today that would show two owners.

## Follow-up candidates

- **Per-phase check in the self-hosting scans.** Same as the spec author's candidate: apply FMT-PIPE-027 to the repository's own sources in `FormatterSelfHostingTests`. The 75-file sample above found no change, so it would land green. Test coverage only; no output in question.

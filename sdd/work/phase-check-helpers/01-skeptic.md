Role: skeptic
Commit: 759ce7e
Verdict: #1505: proceed narrower; #1577: proceed narrower (and wider in one place); #1576: proceed, but not in the tests lane

No intended fix, suspected cause, or hoped-for verdict was in my prompt beyond the lead's issue table in `00-lead.md`; I worked from the issues, their comment, the specs, and the code.

Fixture method used below: `scripts/trace.sh <file> --passes 3` (run as `bash scripts/trace.sh …`, see *Whole run*). The trace parses each pass with default parse options and the line ending `DetectEndOfLine` finds, as `FormatSyntaxTree` does. A pass that reports `0 phase changes` ran every phase on the text of the previous pass's output and no phase changed it, so each phase received that output and left it unchanged — the statement FMT-PIPE-027 makes. This is strong evidence for "the check lands green", not the check itself; the tests are the proof. All fixtures were written byte-exact with Python into a scratch directory outside the repository.

## thoenissen/Reihitsu#1505

### Premise

Partly real, and smaller than the issue says. The issue comment of 2026-11-16 is correct.

- `src/Reihitsu.Formatter.Test/Helpers/ObservedPipeline.cs:63-64` already parses with the caller's parse options and builds the context with `LanguageVersionResolver.Resolve(tree.Options)`. These two lines are a verbatim copy of the body of `SecondPassInput.Create` (`Helpers/SecondPassInput.cs:29-30`). No fixture is observed under other parse options than it is formatted with; the remaining defect is the duplicated statement only. Only one caller passes options: `Unit/Pipeline/PhaseTreeRoundTripTests.cs:219` (`GetPhaseOutput(…, parseOptions)`).
- `Helpers/FormatterPhaseTestsBase.cs:33` parses with default options, but `ApplyPhase` takes no parse options at all, and none of its five subclasses (`IndentationPhaseRawStringMoveTests`, `ExpressionBodiedTransformTests`, `ControlFlowBraceTransformTests`, `ExpressionBodiedLocalFunctionTransformTests`, `UsingDirectiveOrderingRewriterTests`) has a fixture that needs a language version or a symbol. The only directive fixture, `UsingDirectiveOrderingRewriterTests.ConditionalDirectiveSkipsReordering` (`#if DEBUG`), asserts that the phase skips the block, which holds with or without `DEBUG`. The base does not build the context: each subclass's `ExecutePhase` does, four with `new FormattingContext(Environment.NewLine)` and one (`IndentationPhaseRawStringMoveTests:459`) with the line ending detected from the text. `FormattingContext`'s default language version and `LanguageVersionResolver.Resolve` of default parse options both resolve to `CSharp15` (`Utilities/LanguageVersionResolver.cs`, `Data/FormattingContext.cs:60,72`), so routing this base through `SecondPassInput.Create(input, eol, null)` changes nothing any test sees.
- The issue's purpose sentence ("a fixture that needs a language version or preprocessor symbols is observed under different parse options") describes no input that exists today in either helper.

Reachability: test-helper code; no production caller is involved, and no visible output.

### Mechanism and candidates

_N/A_ — no behavioral defect; a duplicated statement of one rule (`ObservedPipeline.Run` restates `SecondPassInput.Create`).

Candidate parse sites in `Helpers/` (`rg "CSharpSyntaxTree.ParseText" Helpers`):

| Site | Builds a pass's root and context? | Status |
|---|---|---|
| `FormatterTestsBase.ApplyRule:338` | yes | already uses `SecondPassInput.Create` |
| `PhaseStabilityCheck.FindChangingPhases:167` | yes | already uses it |
| `ObservedPipeline.Run:63` | yes | copy of `Create`'s body — the issue's real target |
| `FormatterPhaseTestsBase.ApplyPhase:33` | root only; the subclass owns the context | default options, no option parameter |
| `FormatterTestsBase.AssertUnsupportedFor:254`, `AssertSupported:278` | no — they parse a `SyntaxTree` for the public `ReihitsuFormatter.FormatSyntaxTree`, which builds its own context | already pass the fixture's options |
| `PhaseTreeRoundTrip:60` | no — reparses a phase's output to compare trees | not formatter input |

Outside `Helpers/`, 117 files parse with `CSharpSyntaxTree.ParseText`, among them private per-class helpers that run one phase or the pipeline (`Integration/*PhaseIntegrationTests.ExecutePhase`, `Regression/LineBreaks/LeadingTriviaMultiLineDetectionTests.ExecutePhase`, the three regression classes of #1577). "Every formatter test helper" read literally covers them; the issue's *Files* list scopes it to `Helpers/`.

### Breaking inputs

None in the behavioral sense. Failure classes that reach the change:

- FC-031 — `SecondPassInput`'s summary enumerates its users ("The formatter passes of `FormatterTestsBase`, the phases the phase-level stability check runs alone, and the fixed-point assertion of the check's own tests"); a new caller makes that list stale. `ObservedPipeline.Run`'s summary does not mention how it parses, so it stays true.
- If `FormatterPhaseTestsBase` is routed too: the line ending. Four subclasses assert expectations built with `Environment.NewLine` against inputs written with `\n` (for example `UsingDirectiveOrderingRewriterTests:341-342`), so the context's line ending must stay `Environment.NewLine` for them, and the detected one for `IndentationPhaseRawStringMoveTests`. A base that detects the line ending for all five changes those expectations on Windows.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `Helpers/ObservedPipeline.cs` | must change (parse through `SecondPassInput.Create`) |
| `Helpers/SecondPassInput.cs` | summary must change if its list of users grows (FC-031) |
| `Helpers/FormatterPhaseTestsBase.cs` and its five subclasses | internal choice below |
| `FormatterTestsBase.AssertUnsupportedFor`/`AssertSupported` | must stay — they test the public tree entry point, which builds its own context |
| Formatter, analyzers, code fixes, CLI, specs, rule docs | not involved |

### Refuse option

No input class to refuse. The "no change" option: the issue's comment already records that the observed defect is gone, so closing with `Refs` and no diff is defensible. Cost of the narrow change instead: about two lines in `ObservedPipeline.Run` plus the `SecondPassInput` summary. Declaring a shape unsupported is not an option here (no shape).

### Output questions

_None._ Test code only.

### Internal choices

1. **Scope of `FormatterPhaseTestsBase`.**
   - A — leave it as it is. It runs one phase with a context its subclass owns, no subclass needs parse options, and routing it gains nothing a test can observe. The PR states it.
   - B — route it through `SecondPassInput.Create`: the base needs a line-ending decision per subclass (`Environment.NewLine` for four, detected for one) and `ExecutePhase` would receive the context, so six files change for no observable effect; without passing the context on, the call computes a context nobody uses, which reads as a second rule rather than one.
   - Recommendation: **A**. The issue's *Done when* ("every helper that parses formatter input") then reads as "every helper that builds a formatter pass's root and context", which `ObservedPipeline` was the last one not to do.
2. **The name `SecondPassInput`.** `ObservedPipeline` observes a first pass. Its summary already says the first passes of `FormatterTestsBase` use it. Recommendation: keep the name, update the summary's list (or replace the list with a statement that does not enumerate callers).

### Issue goals

- *Done when — "Every formatter test helper that parses formatter input does so through `SecondPassInput.Create`"*: change to "every helper under `Helpers/` that builds a formatter pass's root and context" (choice 1A). The literal reading would pull in `AssertUnsupportedFor`/`AssertSupported`, which parse a tree for the public entry point and must not build a context, and 100+ per-class helpers outside the issue's *Files*. No visible output either way.
- The *What and why* claim that `ObservedPipeline.Run` uses `ParseText(input)` without options is stale (fixed before this run, as the comment says).

### Architecture findings

One rule stated twice: `ObservedPipeline.Run:63-64` and `SecondPassInput.Create:29-30`. Test code; the issue itself is the fix.

### Follow-up candidates

_None_ beyond *Whole run*.

## thoenissen/Reihitsu#1577

### Premise

Real, but the issue's list is wrong in both directions.

Of the seven listed classes, five assert idempotency of a full formatter run and never run the per-phase check; two run a single phase and assert nothing about the formatter's output:

| Class | What it runs | Idempotency it asserts | Check applies? |
|---|---|---|---|
| `Unit/ReihitsuFormatterLineEndingIndependenceTests` | `FormatSyntaxTree` and `FormatDocumentAsync` (`AssertSameOutputAsCopy:228-237`) | full run, both entry points | yes |
| `Unit/ReihitsuFormatterLineEndingTests` | `FormatSyntaxTree` (`AssertFormatted:161-170`) | full run | yes |
| `Regression/LineBreaks/SingleLineTypeBraceWithLeadingUsingTests` | `FormattingPipeline.Execute` with its own context (`Format:64-71`) | `ExpandedTypeRemainsStableOnSecondPass`, LF | yes |
| `Regression/Structural/DirectivePreservationTests` | `FormattingPipeline.Execute`, parse options with `DEBUG` (`Format:47-56`) | `AssertFormatted:76-83`, LF | yes |
| `Regression/LineBreaks/DocumentationCommentCollapseGuardTests` | `FormattingPipeline.Execute` with its own context (`Format:123-130`) | four `…IsIdempotent` tests, LF | yes |
| `Unit/LineBreaks/LineBreakRewriterTests` | the line-break phase alone (`ExecuteLineBreakPhase`) | a second line-break-phase pass (`:196`, `:229`, `:327`) | no — the phase's output is not the formatter's output |
| `Unit/UsingDirectives/UsingDirectiveOrderingRewriterTests` | the using phase alone (`FormatterPhaseTestsBase.ApplyPhase`) | none: every assertion is `AreEqual(expected or input, ApplyPhase(input))` | no — and the issue's premise for it is wrong |

The two single-phase classes would fail the check by design, not by defect: a single phase's output is a later phase's input. For example `UsingDirectiveOrderingRewriterTests.CommentOnDirectiveMovedToLastPosition…` expects `"…System.Collections.Generic;  \nusing System.Linq; // keep  \n\nclass C;"`, whose trailing whitespace the cleanup phase removes. FMT-PIPE-027 covers only the output of `FormatDocumentAsync` and `FormatSyntaxTree`.

Two classes the issue missed assert full-run idempotency outside `FormatterTestsBase` (found by `rg -il "idempot|second pass|secondPass"` over classes that do not derive from it):

- `Regression/Indentation/RawStringOtherLineTerminatorAlignmentTests.RawStringWithOtherLineTerminatorIsAligned` — `FormatSyntaxTree`, a raw string whose lines end with `\r`, U+0085, U+2028, or U+2029, under LF and CRLF (`:61-70`). This is exactly an input `FormatterTestsBase` never builds (it only uses LF and CRLF), so the check says something new here.
- `Unit/Pipeline/FormattingPipelineTests.ExecuteAlreadyFormattedCodeNoChanges` — `FormattingPipeline.Execute` twice (`:144-156`). Its assertion compares `TrimEnd('\n')` of both passes, weaker than whole-text idempotency (FC-017); the check on its output would compare full text.

The other single-phase or single-rewriter classes that compare a second pass (`DocumentationCommentFormattingPhaseTests`, `ContainedBlockRewriterTests`, `BracePlacerTests`) do not assert the formatter's output; the check does not apply.

Does the check land green? Traced, every one green — pass 2 reports `0 phase changes`:

- `ReihitsuFormatterLineEndingTests.LineBreaksInsideLiteralsOfCarriageReturnLineFeedFileAreKept` input `"class C\r\n{\r\n    void M()\r\n    {\r\n            var s = @\"a\nb\";\r\n            var u = \"\"\"\n    d\n    \"\"\";\r\n    }\r\n}"` → pass 1 changes in `IndentationPhase`, `Stable after pass 2 (0 phase changes observed).`
- `…LineBreaksInsideLiteralsOfLineFeedFileAreKept` input `"class C\n{\n    string s = @\"a\r\nb\";\n    string t = $@\"c\r\n{1}\";\n    string u = \"\"\"\r\n               d\r\n               \"\"\";\n}"` → `Stable after pass 1 (0 phase changes observed).`
- `ReihitsuFormatterLineEndingIndependenceTests` rows, with `¶` → `\r`, `§` → `\r`, `¤` → the kept terminator: raw string with `¤` = U+2028; disabled text `#if false … a¤b§#endif` with `¤` = `\r` and with `¤` = U+2028; comment `/* a¤b */` with `¤` = U+2028; verbatim string `@"a¤b"` with `¤` = `\r`; the two-region class with every `¶` = U+0085 → each `Stable after pass 2 (0 phase changes observed).`
- `RawStringOtherLineTerminatorAlignmentTests`: all eight rows (`\r`, U+0085, U+2028, U+2029 × LF, CRLF) → pass 1 changes in `IndentationPhase` only, each `Stable after pass 2 (0 phase changes observed).`
- `DocumentationCommentCollapseGuardTests` constants `DocumentedAccessorTestData`, `DocumentedAttributeMergeTestData`, `DocumentedAttributeInteriorTestData`, `DirectiveWrappedAttributeTestData`, `DocumentedIndexerTestData`, and the local `testData`; `SingleLineTypeBraceWithLeadingUsingTests` `Input` and `Expected` (all LF) → each stable with `0 phase changes` at pass 1 or 2.
- Not settled: `DirectivePreservationTests`, because its fixtures need `DEBUG` and the trace parses with default options. The test is the proof there.

### Mechanism and candidates

_N/A_ — coverage gap, not a defect. Its structural cause is under *Architecture findings*.

Candidates: the seven listed classes plus the two found above, each classified in the tables under *Premise*.

### Breaking inputs

These decide whether the check, once called from these classes, checks the input FMT-PIPE-027 names or another one (FC-033, "the harness normalized the scenario away"):

- **Line ending.** FMT-PIPE-027 parses the output with "the line ending FMT-PIPE-007 detects in it". For the classes that go through `FormatSyntaxTree`/`FormatDocumentAsync` with `\r`, U+0085, U+2028, or U+2029 inputs, the check must use the line ending detected in the output (`ReihitsuFormatterHelpers.DetectEndOfLine`, reachable through `InternalsVisibleTo`), not a constant. Where a class fixes the line ending in its own `FormattingContext`, that is the one its output was formatted with.
- **Parse options.** `DirectivePreservationTests` formats with `preprocessorSymbols: ["DEBUG"]`; the check must get the same options, or it checks the other `#if` branch.
- **Both tree entry points.** `ReihitsuFormatterLineEndingIndependenceTests` asserts idempotency for `FormatSyntaxTree` and for `FormatDocumentAsync`; FMT-PIPE-027 covers both outputs. The test asserts the two outputs are equal, so one check per fixture covers both.
- **Session.** `PhaseStabilityCheck.Verify` needs the session `FormatterTestsBase` starts (`GetCurrentSession` fails otherwise, `PhaseStabilityCheck.cs:192`); `PhaseStabilityCheck.FindChangingPhases(text, endOfLine, parseOptions)` is public and needs none. The failure text naming the phase and FMT-PIPE-027 is built in `PhaseStabilitySession.Check:76`.
- FC-017: the regression classes assert idempotency under LF only; the issue does not ask for CRLF there, and this run need not add it.
- FC-031: `PhaseStabilityCheck`'s summary ("The helpers of `FormatterTestsBase` also run it …") and the `GetCurrentSession` message ("runs only inside a test of a class derived from FormatterTestsBase") describe who runs the check; check both once other classes call it.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| The five full-run classes from the issue's list | must change |
| `RawStringOtherLineTerminatorAlignmentTests`, `FormattingPipelineTests.ExecuteAlreadyFormattedCodeNoChanges` | should change (internal choice 2) |
| `LineBreakRewriterTests`, `UsingDirectiveOrderingRewriterTests` | must stay (internal choice 1) |
| `Helpers/PhaseStabilityCheck` (and session) | internal choice 3 |
| Formatter, analyzers, code fixes, CLI, specs, rule docs | not involved |

### Refuse option

No input class to refuse; no shape to declare. The "no change" option: FMT-PIPE-027 already has covering tests (`Unit/Pipeline/PhaseStabilityCheckTests`) and the check runs on every `FormatterTestsBase` fixture, so the issue is uniformity, as it says. The cost of making it uniform is small (one check call per idempotency assertion), and the line-terminator classes are the only place the check sees inputs other than LF and CRLF, so the change buys real coverage.

### Output questions

_None._ Test code only.

### Internal choices

1. **The two single-phase classes.** A — no change; the issue's *Done when* condition ("wherever it asserts idempotency of a full formatter run") does not hold for them, so it is met without an edit. B — add one sentence to each class summary saying why the check does not apply. Recommendation: **A**; a summary that explains the absence of a check from another requirement in a class that never runs the formatter adds a statement that can drift.
2. **The two missed classes.** A — include them (same reason as the five; `RawStringOtherLineTerminatorAlignmentTests` is the most valuable of all, other line terminators inside a raw string). B — leave them and file a follow-up. Recommendation: **A**; same component, same few lines each, a test each, nothing visible.
3. **How a class outside `FormatterTestsBase` runs the check.** A — each call site calls `FindChangingPhases` and builds its own failure text: a second (sixth, seventh …) statement of the message `PhaseStabilitySession.Check` builds. B — one session-free assertion in `PhaseStabilityCheck` that fails with the same text, shared with the session's check. C — move the three regression classes onto `FormatterTestsBase` and its `AssertRuleResult`/`AssertSecondPassIsStable`: deletes their private `Format` copies, but changes what they assert (adds CRLF, the FMT-PIPE-021 round trip) and needs the base to expose a first-pass result for the `Contains`/`DoesNotContain` tests. Recommendation: **B**; one statement of the failure, smallest diff, no assertion widened beyond the issue.

### Issue goals

- The list of seven classes: drop `UsingDirectiveOrderingRewriterTests` (no idempotency assertion at all) and `LineBreakRewriterTests` (single phase) from the work; add `RawStringOtherLineTerminatorAlignmentTests` and `FormattingPipelineTests.ExecuteAlreadyFormattedCodeNoChanges`. No visible output.
- *Done when*, the "or states in its summary why" branch: drop for the two single-phase classes (choice 1A). No visible output.
- "The check is expected to land green": confirmed by trace for every fixture except `DirectivePreservationTests` (see *Premise*).

### Architecture findings

A shared helper and private copies of it: `FormatterTestsBase.ApplyRule` (parse through `SecondPassInput.Create`, run the pipeline with the FMT-PIPE-021 round trip) is restated as a private `Format` in `SingleLineTypeBraceWithLeadingUsingTests:64`, `DirectivePreservationTests:47`, `DocumentationCommentCollapseGuardTests:123`, and inline in `FormattingPipelineTests:144-152`. The copies are why these classes never got the per-phase check (or the FMT-PIPE-021 round trip). Test code only; it does not reach a visible output. Narrow fix (choice 3B) and leave the copies; their consolidation is choice 3C or a follow-up, the lead's decision.

### Follow-up candidates

- Consolidate the private pipeline-formatting copies in the regression classes onto `FormatterTestsBase` (choice 3C), if not taken in this run. `agent-filed`, tests lane.

## thoenissen/Reihitsu#1576

### Premise

Real: `FormatterSelfHostingTests` checks FMT-PIPE-004 only (`FormatterIsIdempotentOnSourceFile`, `FormatterProducesNoChangesOnSourceFile`); a pair of phases that undo each other on a repository source gives an unchanged whole-pass output and goes unnoticed there. The caller is whole-document formatting through the CLI (the scan formats each source with `FormatSyntaxTree`, as the CLI does). Expected green: spot-checked by trace, each a fixed point with `Stable after pass 1 (0 phase changes observed).` under LF and CRLF copies (BOM stripped as `Encoding.UTF8` reading strips it): `src/Reihitsu.Formatter/ReihitsuFormatter.cs`, `src/Reihitsu.SelfHosting.Test/Formatting/FormatterSelfHostingTests.cs`, `src/Reihitsu.Formatter.Test/Unit/ReihitsuFormatterLineEndingIndependenceTests.cs`. `bash scripts/format.sh src --check` → `0 of 1912 file(s) need formatting.`, exit 0, and no source reported as unsupported.

The issue's location is stale, and that decides the lane:

- `FormatterSelfHostingTests` is in `src/Reihitsu.SelfHosting.Test/Formatting/`, not in `src/Reihitsu.Formatter.Test` as the issue's *Files* says.
- `Reihitsu.SelfHosting.Test.csproj` references `Reihitsu.Analyzer` and `Reihitsu.Formatter` only, not `Reihitsu.Formatter.Test`, where `PhaseStabilityCheck` lives.
- `Reihitsu.Formatter` grants `InternalsVisibleTo` to `Reihitsu.Formatter.Test` but not to `Reihitsu.SelfHosting.Test` (`src/Reihitsu.Formatter/Properties/AssemblyInfo.cs`), and the check needs internal types (`FormattingPipeline`, `IFormattingPhase`, `PreparationPhase`). `ReihitsuFormatter` exposes no phase observer publicly.

So the check cannot be called from `FormatterSelfHostingTests` with a change to test files alone. Every way to reach it adds a path outside the tests lane: a `.csproj` change is tooling (`LaneClassifier.cs:60-65`: "a test project's .csproj is tooling rather than a test"), an `InternalsVisibleTo` in `Reihitsu.Formatter` is shipped source.

### Mechanism and candidates

_N/A_ — coverage gap.

### Breaking inputs

What the scan's check must and must not run on, so that it checks FMT-PIPE-027's input (FC-033):

- **Line endings**: LF and CRLF, as the scan already normalizes them; detected and scan line ending are the same there.
- **Parse options**: default, as the scan parses; FMT-PIPE-027 requires the options of the tree the formatter was given.
- **Outputs the formatter did not format are out of scope** (FMT-PIPE-027, *Output of a run that formatted*: "covers the output of an input the formatter formats"). The scan already skips syntax-error files (`ParseUnderEachLineEnding:218`) and generated file names. Not yet skipped: a source `FormatSyntaxTree` refuses as unsupported (`result.Unsupported != null`), one that holds a hiding carrier (FMT-PIPE-033; `ReihitsuFormatter.cs:104` returns it unchanged without a report; public `ReihitsuFormatter.HoldsHidingCarrier`), and one with a generated-code header (no public check). None is refused as unsupported today (the CLI run above); whether any source holds a hiding carrier I did not settle. Phases run alone on such an unformatted file can change it, which is no violation of FMT-PIPE-027.
- **Which text**: the first pass's output, which is FMT-PIPE-027's subject; for today's sources it equals the source (`FormatterProducesNoChangesOnSourceFile`).
- **Failure names the phase**: `PhaseChange.Phase` carries the reported phase name.
- **Cost**: about 1,800 scanned files (`find` over `FormatterSourceCatalog.SourceDirectories`) × 2 line endings × one single-phase run per production phase — roughly one more formatting pass per file and line ending in the CI-only scan.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `src/Reihitsu.SelfHosting.Test/Formatting/FormatterSelfHostingTests.cs` | must change |
| `src/Reihitsu.SelfHosting.Test/Reihitsu.SelfHosting.Test.csproj` or `src/Reihitsu.Formatter/Properties/AssemblyInfo.cs` | one of them must change (internal choice 1) |
| `sdd/specs/formatter/pipeline.md` | not involved — specs list no covering tests; the tag goes on the test, and `RequirementReferenceCatalog` scans every `*.Test` project, `Reihitsu.SelfHosting.Test` included. The issue's *Files* entry for the spec is not needed |
| Formatter behavior, analyzers, code fixes, CLI, rule docs | not involved |

### Refuse option

No input class or shape. The "no change" option: link the issue `Refs #1576`, since FMT-PIPE-027 is covered by `PhaseStabilityCheckTests` and every `FormatterTestsBase` fixture. It gives up the only check of FMT-PIPE-027 on real, large sources, which the fixtures do not replace. Its cost against the change: the change needs a lane widening and one project or attribute edit.

### Output questions

_None._ Test code and project wiring only; nothing an end user sees.

### Internal choices

1. **How `Reihitsu.SelfHosting.Test` reaches the check.**
   - A — a `ProjectReference` from `Reihitsu.SelfHosting.Test` to `Reihitsu.Formatter.Test`; the scan calls the public `PhaseStabilityCheck.FindChangingPhases(text, endOfLine, parseOptions)` (its signature uses only public types). One `.csproj` line; the run widens to tooling (tests → tooling is an allowed widening; it adds the implementer and the auditor). I did not build it; whether one MSTest project referencing another builds and discovers only its own tests is for the implementer to confirm with `scripts/build.sh` and `scripts/test.sh --project selfhosting`.
   - B — an `InternalsVisibleTo("Reihitsu.SelfHosting.Test")` in `Reihitsu.Formatter` and a copy of the check in the self-hosting project: a second statement of the check (an architecture finding of its own), and shipped source next to the `.csproj`/linked files makes a diff no lane admits without a change doc.
   - C — move the scan, or a copy of it, into `Reihitsu.Formatter.Test`: the default `scripts/test.sh` run would then scan the whole repository, which the scripts keep out on purpose.
   - Recommendation: **A**, with the lane widened to tooling for the whole run.
2. **One test method or two.** A — a new `[DynamicData]` method tagged `FMT-PIPE-027`, next to the two FMT-PIPE-004 methods. B — fold the check into `FormatterProducesNoChangesOnSourceFile`. Recommendation: **A**; one requirement per test, as the existing methods do, and the issue asks for the tag on the covering test.
3. **Skip set.** Skip an output that is not formatted output (see *Breaking inputs*). Recommendation: skip on `result.Unsupported != null` and on `ReihitsuFormatter.HoldsHidingCarrier`; a generated header has no public check, and the scan's file catalog already excludes generated file names.

### Issue goals

- *Files*: the test is in `src/Reihitsu.SelfHosting.Test`, not `src/Reihitsu.Formatter.Test`; `sdd/specs/formatter/pipeline.md` needs no edit. No visible output.
- *Done when* stands as written (same sources, same line endings, `FMT-PIPE-027` tag, failure names the phase), plus the skip set above, which FMT-PIPE-027's own scope demands.
- "Lane: tests" in the issue body cannot hold (see *Premise*); the lead decides the widening.

### Architecture findings

_None._ (Choice 1B would create one.)

### Follow-up candidates

_None._

## Whole run

- **Lane.** #1505 and #1577 fit the tests lane. #1576 does not: it needs a `.csproj` change (tooling) or shipped source. Since an issue does not leave a batch once the run has started, the run widens to tooling as a whole if #1576 is done here (choice 1A of #1576). The lead decides; nothing visible is in question.
- **Out-of-scope find.** `scripts/trace.sh` is committed without the executable bit (`git ls-files -s scripts/*.sh`: `100644 scripts/trace.sh`, every other script `100755`), so `scripts/trace.sh <file>` as `AGENTS.md` documents it fails with `Permission denied` (exit 126); `bash scripts/trace.sh <file>` works. Follow-up candidate, tooling lane, or an adjacent fix if the run widens to tooling anyway.
- No output question, no escalation, no unsupported shape raised.

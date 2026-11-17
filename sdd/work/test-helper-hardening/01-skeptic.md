Role: skeptic
Commit: af13f5a
Verdict: #1804: proceed (wider than the issue names: 8 copy classes, not 4); #1838: proceed narrower (assertion yes, opt-out parameter no)

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

How facts below were established: a throw-away `git worktree` of `af13f5a` in the scratchpad (deleted afterwards; the repository tree was not touched). Each experiment patched only test helpers there and ran `scripts/test.sh --no-install` (Release where it builds, `--configuration Debug` where the experiment's own edits tripped S1172/RH7110). Experiments are named E1–E4 and referenced below.

- **E1** — `Analyzer.Test` `FormatterTestsBase{TAnalyzer}`: `Assert.IsFalse(isRefused)` after the first and second pass of `VerifyFormatter` and in `VerifyFormatterKeepsReportedShape`; `Formatter.Test` `PhaseTreeRoundTrip.ExecuteAndCompare` (the call inside `ApplyRule`) adds a failure when `result.Unsupported != null`. Runs: `--project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Formatter"` → `Passed: 485, Failed: 0` (485 = every `[TestMethod]` under `src/Reihitsu.Analyzer.Test/Formatter`, and no class outside it calls either helper); `--project formatter` (whole project) → `Passed: 5603, Failed: 0`.
- **E2** — `ApplyRule` made `internal`; the private `Format` of each of the 8 copy classes below replaced by a call to it (same line ending and parse options the copy used); `ExecuteAlreadyFormattedCodeNoChanges` compares whole texts, asserts `canonical == input`, and runs `ApplyRule` under LF and CRLF expecting the input back. Run: `--configuration Debug --project formatter --filter` on the 8 classes plus that test → `Passed: 85, Failed: 0`.
- **E3** — `ExecuteAppliesAllRules` and `ExecutePhaseOrderStructuralBeforeIndentation` additionally assert `ApplyRule` first pass == expected, second pass == expected, and `PhaseStabilityCheck.AssertNoPhaseChanges`, under LF and CRLF. Run: `--filter "FullyQualifiedName~FormattingPipelineTests"` → `Passed: 18, Failed: 0`.
- **E4** — `VerifyFormatterFixCore` (Analyzer.Test) additionally runs `PhaseTreeRoundTrip.ExecuteAndCompare` on its input and fails on any FMT-PIPE-021/FMT-PIPE-034 failure (needed a temporary `InternalsVisibleTo("Reihitsu.Analyzer.Test")` in Formatter.Test). Run: analyzer filter as E1 → `Passed: 485, Failed: 0`.

## thoenissen/Reihitsu#1804

### Premise

Real, as a test-infrastructure defect; no caller-reachability question arises (no production input involved, test code only).

`FormatterTestsBase.ApplyRule` (`src/Reihitsu.Formatter.Test/Helpers/FormatterTestsBase.cs:346`) is `private static`: it parses through `SecondPassInput.Create` (fixture's parse options, context with `LanguageVersionResolver.Resolve`) and runs `PhaseTreeRoundTrip.ExecuteAndCompare`, which adds the FMT-PIPE-021 round-trip and FMT-PIPE-034 elastic-trivia checks. The copies parse with `CSharpSyntaxTree.ParseText` + `new FormattingContext(endOfLine)` (no language version, except `ThrowOperandPrecedenceExpressionBodyTests`) + `FormattingPipeline.Execute`, so they lack both checks. Several also run LF only (`DirectivePreservationTests` throughout; most of `DocumentationCommentCollapseGuardTests`), and four of the classes do not derive from `FormatterTestsBase`, so they have no phase-stability session and call `PhaseStabilityCheck.AssertNoPhaseChanges` by hand in some tests.

`ExecuteAlreadyFormattedCodeNoChanges` (`src/Reihitsu.Formatter.Test/Unit/Pipeline/FormattingPipelineTests.cs:131`) compares `canonical.TrimEnd('\n')` with `actual.TrimEnd('\n')`, so the issue's claim is true. Two more things:

- The comment that justifies the trim ("trailing newline differences introduced by the cleanup phase") is stale. E2 shows no such difference: whole-text equality holds.
- The test's doc comment says the code "passes through the pipeline without any modifications", but the test never asserts first pass == input. E2 shows `canonical == input` holds under LF and CRLF. That is exactly what `AssertRuleResult(input)` asserts, and it implies whole-text idempotency.

No formatter defect hides behind the copies: E2 and E3 put every copy through the shared checks without a failure.

### Mechanism and candidates

Mechanism: a test class that runs the pipeline with its own parse-and-run code instead of the base helper misses every check the base helper adds.

Candidate set, from `rg "FormattingPipeline.Execute|SecondPassInput.Create"` over `src/Reihitsu.Formatter.Test` outside `Helpers/`, kept where a fixture is formatted by the full pipeline and its result compared:

| # | Class | Copy | Derives `FormatterTestsBase` | Named in issue |
|---|---|---|---|---|
| 1 | `Regression/LineBreaks/SingleLineTypeBraceWithLeadingUsingTests` | private `Format` | no | yes |
| 2 | `Regression/Structural/DirectivePreservationTests` | private `Format` (LF, `DEBUG`) | no | yes |
| 3 | `Regression/LineBreaks/DocumentationCommentCollapseGuardTests` | private `Format` | no | yes |
| 4 | `Unit/Pipeline/FormattingPipelineTests.ExecuteAlreadyFormattedCodeNoChanges` | inline | no | yes |
| 5 | `Regression/LineBreaks/AutoPropertyAccessorListCrlfTests` | private `Format` (CRLF only; asserts output == input) | no | **no** |
| 6 | `Regression/Structural/ElseKeywordClosingBraceAuditFanOutTests` | private `Format`, two passes | yes | **no** |
| 7 | `Regression/LineBreaks/ParameterAttributeTargetLayoutTests` | private `Format` (line 723), two passes at line 674 | yes | **no** |
| 8 | `Regression/Structural/ThrowOperandPrecedenceExpressionBodyTests` | private `Format` (line 1090), used to build `expected` | yes | **no** |
| 9 | `Unit/Pipeline/FormattingPipelineTests.ExecuteAppliesAllRules`, `ExecutePhaseOrderStructuralBeforeIndentation` | inline, exact expected | no | **no** |

Not members, so they stay:
- Helpers that call the public entry points `ReihitsuFormatter.FormatSyntaxTree`/`FormatDocumentAsync`/`FormatNode*`, such as `ExpressionBodyArrowCommentTests.Format`, `SwitchSectionSharedLocalTests.Format`, `AttributeListStructuralFinalOutputTests.FormatOnce`, `EndOfFileWhitespaceTests.FormatTree`, `CommentLineTerminatorIndentationTests.Format`, and the `ReihitsuFormatter*Tests` classes. They test a different entry point.
- Helpers that run a single phase or rewriter (`DocumentationCommentFormattingPhaseTests`, `BlankLine*Tests`, `IndentationRewriterTests`, and others).
- The `FormattingPipelineTests` tests of the pipeline API itself: cancellation, observers, `ExecutePhases`, exceptions, empty unit.
- `DirectivePreservationTests.FieldDeclarationWithDirectivesInDeclarationIsRefused` (line 214). It asserts a refusal and is no copy of `ApplyRule`; it overlaps the base's `AssertUnsupportedFor`, which uses `FormatSyntaxTree`, not the internal pipeline.

Row 9 is a judgment call. These are unit tests of `FormattingPipeline.Execute`, so calling it directly is defensible, but they compare a fixture's full output exactly as `AssertRuleResult` does, and the "Done when" wording covers them. E3: they pass every shared check.

### Breaking inputs

- FC-015 (a private copy next to the shared helper drifts from it) is the issue itself. The drift that exists today: no FMT-PIPE-021/034 checks, no language version in the context, and LF-only runs.
- FC-016 (sibling copies keep the hazard): the issue names 4 of 8 classes (rows 5–8 missing) plus the two row-9 tests.
- FC-010 (not a no-op only on CRLF): the LF-only copies (rows 2 and 3) have never been run under CRLF. E2 kept each copy's own line ending, so **the CRLF run of rows 2 and 3 is unestablished**. The test implementer finds out when they move to a helper that runs both endings.

The test implementer must cover:
- Every row's fixtures, through the shared helper.
- `ExecuteAlreadyFormattedCodeNoChanges` with whole-text comparison. Its input has no trailing line break, so "the passes differ only in trailing line breaks" is detected only without the trim.

Constraint to keep: rows 3 and 6 assert partial properties on purpose (`Assert.Contains`/`DoesNotContain`; row 6's doc says each fixture asserts "only that one reported expected-versus-actual difference, so an unrelated layout guess cannot produce a false failure"). The shared way has to hand them the formatted text. Turning them into exact-output assertions would lock in layout no requirement of these tests decides (see *Internal choices*).

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `src/Reihitsu.Formatter.Test/Helpers/FormatterTestsBase.cs` | must change: one shared pipeline-format entry the copies can call |
| The 8 classes / 3 tests above | must change |
| `src/Reihitsu.Analyzer.Test/Base/FormatterTestsBase{TAnalyzer}.cs` `VerifyFormatterFixCore` | cross-project member of the same class; see *Architecture findings* |
| Formatter production code, phases, specs | must stay (E1–E3: no failure, so no behavior change is needed) |
| Analyzer, code fixes, CLI, rule docs | not involved |

### Refuse option

Not applicable: no user input class is refused or withheld; this is test code. Declaring a shape unsupported is not an option here. Simpler option: limit the run to the four named classes and the trim. It is cheaper, but it leaves rows 5–9 as copies, so the "Done when" fails. Not recommended.

### Output questions

_None._ No test's asserted visible output changes. E2 and E3 show every existing expected value holds under the shared checks.

### Internal choices

1. **Scope of the class**: rows 1–8 only, or also row 9. Recommend 1–8 plus row 9's two tests. They restate the same parse-and-run-and-compare, E3 shows they pass, and the "Done when" wording covers them. Leave the API-level `FormattingPipelineTests` tests alone.
2. **`ExecuteAlreadyFormattedCodeNoChanges`**: whole-text two-pass comparison only (the issue's wording), or first pass == input (`AssertRuleResult(input)` semantics, which implies the former). Recommend the latter. It is what the test's own doc comment claims, and E2 shows it holds under LF and CRLF.
3. **Partial-assertion tests (rows 3, 6)**: keep their `Contains`-style or per-difference assertions on the shared helper's output. Do not convert them to exact expected text: that would add asserted formatter output no requirement in these tests decides, which the auditor's *Visible output is decided* check would have to clear.

### Issue goals

- "Four test classes": change to the 8 classes plus the two row-9 tests. The issue's list is incomplete (FC-016). No visible output.
- "fails when the two passes differ in their trailing line breaks": keep, but it follows from whole-text comparison; it cannot be shown by a failing run today. No visible output.
- Everything else stands.

### Architecture findings

The issue is itself one: a shared helper (`ApplyRule`) and private copies own the same decision, how a fixture is parsed and run. The class has 8 members in `Formatter.Test`, plus one in `Analyzer.Test`:

`FormatterTestsBase{TAnalyzer}.VerifyFormatterFixCore` (`src/Reihitsu.Analyzer.Test/Base/FormatterTestsBase{TAnalyzer}.cs:211`) restates `SecondPassInput.Create` + `FormattingPipeline.Execute` (identical parse and context) without the FMT-PIPE-021/034 round-trip checks.

- E4: all 485 parity tests pass with those checks added, so unifying it would turn up no failures.
- `SecondPassInput` and `PhaseTreeRoundTrip` are `internal` to `Reihitsu.Formatter.Test`, which grants no `InternalsVisibleTo`. Reusing them from `Analyzer.Test` needs a visibility decision.
- This is the file #1838 changes, so it fits *Scope* as an adjacent cleanup of the same file. The lead decides.

### Follow-up candidates

_None_ beyond the scope decision above.

## thoenissen/Reihitsu#1838

### Premise

Real by the code, and preventive in severity: no test is affected today.

- `VerifyFormatterFixCore` asserts `formatted == fixed` and returns `IsRefused`, and both callers use `IsRefused` only to skip `PhaseStabilityCheck.AssertNoPhaseChanges` (`FormatterTestsBase{TAnalyzer}.cs:138`, `:158`). A refused document comes back unchanged.
- Only the calls whose expected text equals the source are exposed: `VerifyFormatterKeepsReportedShape` (3 callers: RH5409, RH5410, RH5411 parity tests), and `VerifyFormatter` with `fixedSource` null or equal to the source and no expected diagnostics (about 100 single-argument `VerifyFormatter(x)` calls).
- A `VerifyFormatter` call with expected diagnostics is already protected: refusal gives formatted == source, which either differs from `fixedSource` or still carries the diagnostics that `Verify(formatted, onConfigure)` expects none of.
- `git log -S isRefused`: the skip came in with #1824 (`6cf7fdd`, per-phase stability) and `17fb08c` (#1832, `VerifyFormatterKeepsReportedShape`). Neither was prompted by a test that is refused.
- E1: with refusal asserted in both helpers, all 485 parity tests pass. **No current call uses a refused input.**

### Mechanism and candidates

Mechanism: a helper that checks "output equals an expected text equal to the input" cannot tell "the formatter kept it" from "the formatter refused the file", because a refusal returns the input unchanged.

Candidates (every test helper of that shape), from the helper definitions:

| Helper | Exposed when | Today (E1) |
|---|---|---|
| `Analyzer.Test` `VerifyFormatterKeepsReportedShape` | always | 0 refused |
| `Analyzer.Test` `VerifyFormatter` (both public overloads) | expected == source, no diagnostics | 0 refused |
| `Formatter.Test` `AssertRuleResult(input)` / `(input, expected == input)` | always / when equal | 0 refused (5603 pass) |
| `Formatter.Test` `AssertSecondPassIsStable`, `AssertBlankLinesAbove` | a refused input is trivially stable | 0 refused |

The `Formatter.Test` rows are outside #1838's text; they are the same class in #1804's helper (see *Follow-up candidates*). `AssertSupported` already asserts non-refusal and is no member.

### Breaking inputs

FC-016 (sibling helpers keep the hazard): the `Formatter.Test` rows above.

Interaction with #1732 (claimed, draft #1839, unmerged; it declares a comment or directive between an expression-bodied member's `=>` and its expression):
- `rg` over `src/Reihitsu.Analyzer.Test/Formatter` finds only a lambda `=>` followed by a comment (`RH5205StatementLambdaOpeningBraceShouldBeAlignedFormatterTests.cs:75-76`). That is not an arrow expression clause, so #1732 is not expected to make any parity fixture refused, whatever the merge order.
- `src/Reihitsu.Formatter.Test` has many `AssertRuleResult` fixtures with a comment behind `=>` (for example `ExpressionBodyDirectiveGuardTests`, `ExpressionBodiedMethodTests`, `GetOnlyMemberExpressionBodyTests`). If a refusal check goes into `ApplyRule`, #1732's run will see those that become refused fail instead of pass silently. That is the intended effect of the check, but it adds work to that run.

The test implementer must cover a refused input reaching each changed helper, so the helper is shown to fail. One proven refused input: `HorizontalSpacingRequirementTests.CommentBetweenFixedAndTheTypeOfAFixedSizeBufferIsUnsupported` (`src/Reihitsu.Formatter.Test/Regression/Spacing/HorizontalSpacingRequirementTests.cs:927`, FMT-UNSUP-004) asserts the refusal of `unsafe struct S { public fixed /* d */  int _d[4]; }` written on four lines. A parity-test fixture also has to compile for the analyzer verifier, so the test implementer must confirm that such an input passes `Verify` before relying on it.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `src/Reihitsu.Analyzer.Test/Base/FormatterTestsBase{TAnalyzer}.cs` | must change |
| Parity test classes under `src/Reihitsu.Analyzer.Test/Formatter` | must stay (E1: none needs an opt-out) |
| Formatter, analyzers, code fixes, CLI, specs, rule docs | not involved |

### Refuse option

Not applicable (test code only; no input class is refused or withheld, and declaring a shape is not an option here).

Simpler option: assert non-refusal unconditionally, with no opt-out parameter. No current call needs one (E1).

### Output questions

_None._

### Internal choices

1. **Opt-out parameter**: add it now (as the issue and the triage comment propose) or not. Recommend not: it would have zero callers (E1). A test that wants to show a refusal belongs with `Formatter.Test`'s `AssertUnsupportedFor`, which also checks reason and location. The first change that needs an opt-out adds it.
2. **Which pass to check in `VerifyFormatter`**: first, second, or both. A refused first pass makes the second refused too, since its input is unchanged. Checking the first pass states the claim, and checking the second also pins FMT-UNSUP-005 (no phase produces an unsupported shape). Recommend both, or the first at least. With the assertion in place, the `isRefused == false` guards around `PhaseStabilityCheck` become always true and can go.

### Issue goals

- Drop "opt out explicitly, for example by a parameter": no caller (E1); it would be speculative API. No visible output.
- Drop "the test classes that call … with an input the formatter refuses": the set is empty today (E1). No visible output.
- The triage comment's expectation that the assertion checks the parity tests against #1732's declaration still holds once #1732 merges, but per the grep above no parity fixture holds #1732's shape.
- "Both helpers fail when the formatter refuses an input … and the analyzer test project is green": keep.

### Architecture findings

_None_ beyond #1804's cross-project member (`VerifyFormatterFixCore`), reported there.

### Follow-up candidates

- **The same refusal gap in `Formatter.Test`'s `ApplyRule`** (`AssertRuleResult` with no expected or expected == input, `AssertSecondPassIsStable`, `AssertBlankLinesAbove`).
  - E1: no fixture of the 5603 is refused today, so a refusal check there is free now.
  - It is the helper #1804 consolidates, so it fits *Scope* as an adjacent fix (small, same component, no visible output).
  - Coupling: it makes #1732's run convert every fixture its declaration refuses (see *Breaking inputs*).
  - If the lead does not pull it in, it is a follow-up.

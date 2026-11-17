Role: test implementer
Commit: 13b99aa
Verdict: #1804: tests written; #1838: tests written

The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the lead's recorded decisions (rows 1–6 of `00-lead.md`), which set the scope and were followed.

Lane: tests. The run's change is test code only; this role wrote the helper and test-class changes the issues call for (no implementer in this lane). No production file, spec, or rule doc is touched. No existing expected value of any test changed.

## thoenissen/Reihitsu#1804

### Change

`src/Reihitsu.Formatter.Test/Helpers/FormatterTestsBase.cs` gains one shared entry for tests that assert only a property of the output:

```csharp
protected static string FormatWithPipeline(string input, string endOfLine, CSharpParseOptions parseOptions = null)
    => ApplyRule(NormalizeLineEndings(input, endOfLine), endOfLine, parseOptions);   // body in the file is a block
```

`ApplyRule` stays private; every copy now goes through `AssertRuleResult`, `AssertSecondPassIsStable`, or `FormatWithPipeline`, so each one parses through `SecondPassInput.Create` (fixture parse options, language version) and gets the FMT-PIPE-021/034 round-trip checks of `PhaseTreeRoundTrip.ExecuteAndCompare`. The four classes that did not derive from `FormatterTestsBase` (rows 1, 2, 3, 5 below) and `FormattingPipelineTests` now derive from it and drop their own `TestContext`, so they run inside the phase-stability session.

| # | Class | Before | After |
|---|---|---|---|
| 1 | `Regression/LineBreaks/SingleLineTypeBraceWithLeadingUsingTests` | private `Format`, separate LF and CRLF tests, LF-only second pass | `OpeningBraceMovesToOwnLineWithLeadingUsingLf` + `…Crlf` merged into `OpeningBraceMovesToOwnLineWithLeadingUsing` = `AssertRuleResult(Input, Expected)`; `ExpandedTypeRemainsStableOnSecondPass` = `AssertRuleResult(Expected)` |
| 2 | `Regression/Structural/DirectivePreservationTests` | private `Format`/`AssertFormatted`, LF only | `AssertRuleResult(input, expected, _parseOptions)` / `AssertRuleResult(input, parseOptions: _parseOptions)` (now LF and CRLF). `FieldDeclarationWithDirectivesInDeclarationIsRefused` kept as is (not a copy; skeptic) |
| 3 | `Regression/LineBreaks/DocumentationCommentCollapseGuardTests` | private `Format`, mostly LF | partial `Contains`/`DoesNotContain` assertions kept unchanged on `FormatWithPipeline(…, eol)` (decision 2); the four `…FormattingIsIdempotent` tests = `AssertSecondPassIsStable(…)` (now LF and CRLF) |
| 4 | `Unit/Pipeline/FormattingPipelineTests.ExecuteAlreadyFormattedCodeNoChanges` | inline two passes, `TrimEnd('\n')` comparison, stale comment | `AssertRuleResult(input)`: whole text of the first pass == input under LF and CRLF (so the second pass equals it too), phase stability; stale comment gone (decision 3) |
| 5 | `Regression/LineBreaks/AutoPropertyAccessorListCrlfTests` | private `Format`, CRLF only, output == input | `AssertRuleResult(TestData)` (CRLF and LF); test name kept |
| 6 | `Regression/Structural/ElseKeywordClosingBraceAuditFanOutTests` | private `Format`, two passes | same per-fixture glue check and second-pass equality, on `FormatWithPipeline` (decision 2) |
| 7 | `Regression/LineBreaks/ParameterAttributeTargetLayoutTests.SameTargetListsFollowedByCommentLineAreNotMerged` | private `Format`, manual two-pass loop | per-ending `Contains` assertions kept on `FormatWithPipeline`; idempotency = `AssertSecondPassIsStable(input)`; private `Format` removed |
| 8 | `Regression/Structural/ThrowOperandPrecedenceExpressionBodyTests` | private `Format(source)` building `expected` | `FormatWithPipeline(authored, "\n")` |
| 9 | `Unit/Pipeline/FormattingPipelineTests.ExecuteAppliesAllRules`, `ExecutePhaseOrderStructuralBeforeIndentation` | inline, one ending, exact expected | `AssertRuleResult(input, expected)` (decision 1) |

`rg "FormattingPipeline.Execute|SecondPassInput.Create" src/Reihitsu.Formatter.Test` outside `Helpers/` now finds only the `FormattingPipelineTests` tests of the pipeline API itself (cancellation, empty unit, observers, `ExecutePhases`, exceptions) and `DirectivePreservationTests.FieldDeclarationWithDirectivesInDeclarationIsRefused`, all kept by decision 1 / the skeptic's *Not members* list.

Established now (was unestablished per the skeptic): rows 2 and 3 pass under CRLF through the shared helpers.

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `SingleLineTypeBraceWithLeadingUsingTests.OpeningBraceMovesToOwnLineWithLeadingUsing` | FC-015, FC-010 | `AssertRuleResult(input, expected)` | pass |
| `SingleLineTypeBraceWithLeadingUsingTests.ExpandedTypeRemainsStableOnSecondPass` | FC-015 | `AssertRuleResult(input)` | pass |
| `DirectivePreservationTests.SwitchCaseBraceInsertionKeepsDirectivesInSection` | FC-015, FC-010 | `AssertRuleResult` | pass |
| `DirectivePreservationTests.ControlFlowBraceInsertionKeepsDirectivesAroundBody` | FMT-STRUCT-003 (existing tag, unchanged) | `AssertRuleResult` | pass |
| `DirectivePreservationTests.ControlFlowBraceInsertionKeepsDirectiveBeforeElse` | FC-015, FC-010 | `AssertRuleResult` | pass |
| `DocumentationCommentCollapseGuardTests` (16 tests; 4 idempotency tests rewritten, 12 rerouted) | FC-015, FC-010 | `FormatWithPipeline`, `AssertSecondPassIsStable` | pass |
| `AutoPropertyAccessorListCrlfTests.AutoPropertyAccessorListStaysOnPropertyLineWithCrlf` | FC-015, FC-010 | `AssertRuleResult(input)` | pass |
| `ElseKeywordClosingBraceAuditFanOutTests.AuditFanOutNeverGluesElseOntoClosingBrace` | FC-015 | `FormatWithPipeline` | pass |
| `ParameterAttributeTargetLayoutTests.SameTargetListsFollowedByCommentLineAreNotMerged` | FMT-BREAK-001, FMT-BREAK-027 (existing tags, unchanged) | `FormatWithPipeline`, `AssertSecondPassIsStable` | pass |
| `ThrowOperandPrecedenceExpressionBodyTests` (2 tests that built `expected` with the copy) | existing tags unchanged | `FormatWithPipeline`, `AssertRuleResult` | pass |
| `FormattingPipelineTests.ExecuteAlreadyFormattedCodeNoChanges` | FC-015, FC-010 | `AssertRuleResult(input)` | pass |
| `FormattingPipelineTests.ExecuteAppliesAllRules` | FC-015 | `AssertRuleResult(input, expected)` | pass |
| `FormattingPipelineTests.ExecutePhaseOrderStructuralBeforeIndentation` | FC-015 | `AssertRuleResult(input, expected)` | pass |

No requirement tag was added or removed; these are test-infrastructure changes that verify no new requirement.

### Not covered, and why

- A failing run of "the two passes differ only in a trailing line break": not producible without a formatter defect. The new assertion compares whole texts (`Assert.AreEqual(normalizedInput, actual)` in `AssertRuleResult`), so the trim that hid such a difference is gone; that is the guarantee, shown statically.
- Exact-output assertions for rows 3 and 6: deliberately not added (decision 2).
- A refusal check in `ApplyRule`: out of scope (decision 6).
- Formatter failure-class walk: the run changes no formatter, analyzer, or code-fix behavior, so FC classes about phases, fixes, comments/directives in gaps, renames, and unsupported shapes cannot reach it; only FC-015/FC-016 (drifting copies, which this removes) and FC-010 (line endings, now run on both) apply.

### Follow-up candidates

_None_ beyond the lead's recorded ones (cross-project `VerifyFormatterFixCore` copy, decision 4).

## thoenissen/Reihitsu#1838

### Change

`src/Reihitsu.Analyzer.Test/Base/FormatterTestsBase{TAnalyzer}.cs`: `VerifyFormatterFixCore`, which both `VerifyFormatter` (every overload, both passes) and `VerifyFormatterKeepsReportedShape` go through, now asserts before the output comparison

```csharp
Assert.IsNull(result.Unsupported, $"The formatter must not refuse the input as unsupported: {result.Unsupported?.Reason}");
```

and no longer returns `IsRefused`; the `isRefused == false` guards around `PhaseStabilityCheck.AssertNoPhaseChanges` are removed (always true now). No opt-out parameter (decision 5). Doc comments updated.

Static proof that one test per public helper suffices: `rg -n "VerifyFormatterFixCore\(" "src/Reihitsu.Analyzer.Test/Base/FormatterTestsBase{TAnalyzer}.cs"` shows the three call sites (two in the private `VerifyFormatter(source, fixedSource, endOfLine, onConfigure, expected)` that all three public `VerifyFormatter` overloads reach, one in the private `VerifyFormatterKeepsReportedShape(source, endOfLine)`); the assertion is unconditional inside the shared `VerifyFormatterFixCore`.

### Tests

New class `src/Reihitsu.Analyzer.Test/Base/FormatterTestsBaseTests.cs` (derives `FormatterTestsBase<RH5409FinalEnumMemberMustNotHaveTrailingCommaAnalyzer>`; fixture `private int /* comment */ _value;` in a class, refused under FMT-UNSUP-004, compiles, RH5409 reports nothing):

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `VerifyFormatterFailsWhenTheFormatterRefusesTheSource` | FC-016 (helper hazard) | `VerifyFormatter(source)` | pass: throws `AssertFailedException` whose message holds the reason `a comment between a variable declaration's own tokens` |
| `VerifyFormatterKeepsReportedShapeFailsWhenTheFormatterRefusesTheSource` | FC-016 | `VerifyFormatterKeepsReportedShape(source)` | pass: same |
| `VerifyFormatterPassesWhenTheFormatterKeepsTheSource` | other side of the condition (FC-003) | `VerifyFormatter(source)` | pass |

No requirement tag: they test a test helper.

Watched fail for the right reason: with the `Assert.IsNull(result.Unsupported, …)` line removed (temporarily, restored before commit), the two refusal tests fail with `Assert.ThrowsExactlyAsync<AssertFailedException>(…)` (no exception thrown: the helper passes a refused input), and the third passes. With the line in place all three pass, and all 485 existing parity tests under `Reihitsu.Analyzer.Test.Formatter` still pass (no current parity fixture is refused, matching the skeptic's E1).

### Not covered, and why

- `VerifyFormatter(source, fixedSource, expected)` with diagnostics and a refused input: already failed before this change (refusal gives formatted == source ≠ `fixedSource`, or diagnostics remain); same shared call site as above, so no separate test.
- An opt-out parameter test: no parameter (decision 5).

### Follow-up candidates

_None_ beyond the lead's recorded one (refusal check for Formatter.Test `ApplyRule`, decision 6).

## Whole run

### Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~SingleLineTypeBraceWithLeadingUsingTests|FullyQualifiedName~DirectivePreservationTests|FullyQualifiedName~DocumentationCommentCollapseGuardTests|FullyQualifiedName~AutoPropertyAccessorListCrlfTests|FullyQualifiedName~ElseKeywordClosingBraceAuditFanOutTests|FullyQualifiedName~ParameterAttributeTargetLayoutTests|FullyQualifiedName~ThrowOperandPrecedenceExpressionBodyTests|FullyQualifiedName~Unit.Pipeline.FormattingPipelineTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Formatter|FullyQualifiedName~Reihitsu.Analyzer.Test.Base.FormatterTestsBaseTests"
```

Results on 13b99aa (Release, as `scripts/test.sh` builds): formatter `Passed: 101, Failed: 0`; analyzer `Passed: 488, Failed: 0` (485 parity + 3 new). Build log carried no warnings. `scripts/format.sh` was run over all 11 changed C# files (it only added final line breaks to 3 files).

Changed paths (all test code): the 11 files under `src/Reihitsu.Formatter.Test` and `src/Reihitsu.Analyzer.Test` named above. Full-project runs and the lane check are the validator's / lead's.

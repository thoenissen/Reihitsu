Role: test implementer
Commit: 8ac4176
Verdict: reproduced

The prompt carried no intended fix, no suspected cause, and no hoped-for verdict. Expected values come from SHARED-BLANK-005 / FMT-BLANK-013 as the maintainer's answers to output questions 1 A and 2 A and lead decision 7 read them (`00-lead.md`); unrelated layout in each fixture is already canonical, so every failing formatter test differs from today's output by exactly the one missing blank line (checked in each failure message). The `while` goal (decision 3) and the restructuring goal (decision 4) are dropped by the lead; no test was written for them.

## Tests

New file `src/Reihitsu.Formatter.Test/Regression/BlankLines/RegionDirectiveLineAboveTests.cs` (class `RegionDirectiveLineAboveTests`), plus additions to three existing files.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RegionDirectiveLineAboveTests.RegionBelowIfDirectiveAfterTypeBraceGetsBlankLine` (f3, reproduction) | FMT-BLANK-013, SHARED-BLANK-005 | `AssertRuleResult(input, expected)` | fail |
| `RegionDirectiveLineAboveTests.BlankLineBetweenIfDirectiveAfterTypeBraceAndRegionIsKept` (f7) | FMT-BLANK-013, SHARED-BLANK-005 | `AssertRuleResult(input)` | pass |
| `RegionDirectiveLineAboveTests.RegionBelowElseDirectiveAfterTypeBraceGetsBlankLine` (f5) | FMT-BLANK-013, SHARED-BLANK-005 | `AssertRuleResult` | fail |
| `RegionDirectiveLineAboveTests.RegionBelowElifDirectiveAfterTypeBraceGetsBlankLine` (g5) | FMT-BLANK-013, SHARED-BLANK-005 | `AssertRuleResult` | fail |
| `RegionDirectiveLineAboveTests.RegionBelowEndIfDirectiveAfterTypeBraceGetsBlankLine` (g4) | FMT-BLANK-013, SHARED-BLANK-005 | `AssertRuleResult` | fail |
| `RegionDirectiveLineAboveTests.RegionBelowIfDirectiveAfterMethodBodyBraceGetsBlankLine` (f6) | FMT-BLANK-013, SHARED-BLANK-005 | `AssertRuleResult` | fail |
| `RegionDirectiveLineAboveTests.RegionBelowIfDirectiveAfterNamespaceBraceGetsBlankLine` (g6) | FMT-BLANK-013, SHARED-BLANK-005 | `AssertRuleResult` | fail |
| `RegionDirectiveLineAboveTests.RegionBelowIfDirectiveAfterSwitchBlockBraceGetsBlankLine` (g7) | FMT-BLANK-013, SHARED-BLANK-005 | `AssertBlankLinesAbove` (`#if` no blank above, `#region A` blank above) | fail |
| `RegionDirectiveLineAboveTests.RegionBelowIfDirectiveAfterAccessorListBraceGetsBlankLine` (g8) | FMT-BLANK-013, SHARED-BLANK-005 | `AssertBlankLinesAbove` | fail |
| `RegionDirectiveLineAboveTests.RegionBelowHeaderCommentAtFileStartGetsBlankLine` (g1) | FMT-BLANK-013, SHARED-BLANK-005 | `AssertRuleResult` | fail |
| `RegionDirectiveLineAboveTests.RegionBelowIfDirectiveAtFileStartGetsBlankLine` (g2) | FMT-BLANK-013, SHARED-BLANK-005 | `AssertRuleResult` | fail |
| `RegionDirectiveLineAboveTests.RegionBelowNullableDirectiveAtFileStartGetsBlankLine` (k4) | FMT-BLANK-013, SHARED-BLANK-005 | `AssertRuleResult` | fail |
| `RegionDirectiveLineAboveTests.RegionOnFirstLineOfFileIsUnchanged` (first-line exemption, other side of g1/g2/k4) | FMT-BLANK-013, SHARED-BLANK-005 | `AssertRuleResult(input)` | pass |
| `RegionDirectiveLineAboveTests.SecondRegionBelowCommentInSameGapGetsBlankLine` (h1) | FMT-BLANK-013, SHARED-BLANK-005 | `AssertRuleResult` | fail |
| `RegionDirectiveLineAboveTests.SecondRegionBelowPragmaInSameGapGetsBlankLine` (h2) | FMT-BLANK-013, SHARED-BLANK-005 | `AssertRuleResult` | fail |
| `RegionDirectiveLineAboveTests.SecondRegionBelowIfDirectiveInSameGapGetsBlankLine` (h3) | FMT-BLANK-013, SHARED-BLANK-005 | `AssertRuleResult` | fail |
| `RegionDirectiveLineAboveTests.FirstRegionBelowIfDirectiveBehindMemberGetsBlankLine` (h4, first-of-kind side of h3) | FMT-BLANK-013, SHARED-BLANK-005 | `AssertRuleResult` | pass |
| `RegionDirectiveLineAboveTests.SecondEndRegionBelowCommentInSameGapGetsBlankLine` (k1) | FMT-BLANK-013, SHARED-BLANK-005 | `AssertRuleResult` | fail |
| `RegionDirectiveLineAboveTests.SecondEndRegionBelowPragmaInSameGapGetsBlankLine` (k2) | FMT-BLANK-013, SHARED-BLANK-005 | `AssertRuleResult` | fail |
| `DisabledRegionLayoutTests.RegionInsideActiveConditionalIsStillFormatted` — expected value changed per Q1 A (blank line between `#if DEBUG` and `#region Test`); tags added | FMT-BLANK-013, SHARED-BLANK-005, SHARED-BLANK-007 | `AssertRuleResult(input, expected, _withDebug)` | fail |
| `RH5031RegionDirectivesShouldBePrecededByABlankLineFormatterTests.VerifyFormatterSeparatesActiveConditionalAfterBraceFromRegion` (RH5031 characterization on an active branch, decision 5) | FMT-BLANK-013, SHARED-BLANK-005 | `VerifyFormatter(source, fixed, diagnostics)` | fail |
| `RH5031RegionDirectivesShouldBePrecededByABlankLineFormatterTests.VerifyFormatterLeavesRegionInInactiveConditionalAfterBraceUnchanged` (the issue's literal `#if DEBUG` input, inactive guard) | FMT-BLANK-013, SHARED-BLANK-007 | `VerifyFormatter(source)` | pass |
| `RH5031RegionDirectivesShouldBePrecededByABlankLineFormatterTests.VerifyFormatterSeparatesHeaderCommentAtFileStartFromRegion` | FMT-BLANK-013, SHARED-BLANK-005 | `VerifyFormatter(source, fixed, diagnostics)` | fail |
| `RH5031RegionDirectivesShouldBePrecededByABlankLineFormatterTests.VerifyFormatterSeparatesCommentFromSecondRegionInGap` | FMT-BLANK-013, SHARED-BLANK-005 | `VerifyFormatter(source, fixed, diagnostics)` | fail |
| `RH5031RegionDirectivesShouldBePrecededByABlankLineAnalyzerTests.VerifyRegionBelowActiveIfDirectiveAfterBraceIsDetectedAndFixed` (guard: RH5031 must keep its behavior, decision 6) | RH5031-001, SHARED-BLANK-005 | `Verify(source, fixed, diagnostics)` | pass |
| `RH5031RegionDirectivesShouldBePrecededByABlankLineAnalyzerTests.VerifyRegionBelowHeaderCommentAtFileStartIsDetectedAndFixed` | RH5031-001, SHARED-BLANK-005 | `Verify(source, fixed, diagnostics)` | pass |
| `RH5031RegionDirectivesShouldBePrecededByABlankLineAnalyzerTests.VerifyRegionOnFirstLineOfFileIsNotReported` | RH5031-001, SHARED-BLANK-005 | `Verify(source)` | pass |
| `RH5031RegionDirectivesShouldBePrecededByABlankLineAnalyzerTests.VerifySecondRegionBelowCommentInSameGapIsDetectedAndFixed` | RH5031-001, SHARED-BLANK-005 | `Verify(source, fixed, diagnostics)` | pass |
| `RH5031RegionDirectivesShouldBePrecededByABlankLineAnalyzerTests.VerifySecondEndRegionBelowPragmaInSameGapIsDetectedAndFixed` | RH5031-001, SHARED-BLANK-005 | `Verify(source, fixed, diagnostics)` | pass |

Observed while writing the last test: a bare `#pragma warning disable` above the directive suppresses RH5031 itself (0 diagnostics), so the analyzer test uses `#pragma warning disable CS0168`. The formatter tests h2/k2 keep the bare pragma (the formatter does not read pragmas). In the formatter's output a `#pragma` stands at column 0, so the fixtures place it there.

## Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| f3 `#if !DEBUG` below type `{`, `#region` | LF (CRLF runs in the same helper after LF passes) | formatter, full pipeline | reproduced | observed `#if !DEBUG\n    #region A`; expected a blank line between them |
| Same shape, `DEBUG` set in parse options (`DisabledRegionLayoutTests`) | LF | formatter | reproduced | same one-line difference |
| f3 shape through `FormatterTestsBase<RH5031…>` | LF, then CRLF | formatter output re-analyzed by RH5031 | reproduced | formatter output differs from the RH5031-clean expected value by the missing blank line |
| Issue's literal `#if DEBUG` input (DEBUG undefined) | LF and CRLF | formatter + RH5031 | holds | unchanged, no diagnostic (SHARED-BLANK-007) |
| f5, g5, g4, f6, g6, g7, g8, g1, g2, k4, h1, h2, h3, k1, k2 | LF | formatter | reproduced | each: the blank line above the region directive is missing; nothing else differs |
| Same shapes, analyzer + code fix | default | RH5031 | holds | reported and fixed as expected (RH5031 already applies SHARED-BLANK-005) |

## Not covered, and why

- `while` (FMT-BLANK-008) and the one ordered rule list: dropped by lead decisions 3 and 4; `BlankLineBehindDirectiveBeforeTokenTests.KeepsBlankLineBetweenDirectiveAndWhileStatement` already pins the issue's input.
- f4 (`#define DEBUG` + `#if DEBUG`): parses to the same `IfDirectiveTrivia` above the region as f3 and as the `_withDebug` variant in `DisabledRegionLayoutTests`; the conditional's active state comes from the parser in all three. Not added separately.
- g9 (`#pragma` first after `{`) and k5 (comment first behind `;`): already covered by `RegionDirectlyAfterOpeningBraceTests.PragmaFirstAfterBraceIsSeparatedFromEmptyRegionBelow` and `BlankLineRegionTests.BlankLineInsertedBetweenCommentAndRegion`; both pass today.
- Brace exemption without anything in between: `RegionDirectlyAfterOpeningBraceTests` (type, method, namespace, enum, statement block, switch, accessor) and `ConditionalDirectivesDirectlyInsideBracesAreUnchanged` guard the other side of the `{` condition; trailing-comment brace: `EmptyRegionBelowBraceWithTrailingCommentGetsBlankLine`.
- FMT-BLANK-009 interplay (region directive ending a gap before `}` with a directive in front): `DirectiveBeforeEndRegionBlankLineTests` already covers it.
- g3 (`#endregion` behind `{` + `#if`): parse error, the formatter leaves syntax-invalid input alone.
- `#endregion` in trailing trivia and detached-node formatting (FC-028): the skeptic found no caller that produces either; not tested.
- Documentation comment directly above a region directive (FC-012, d1): follow-up candidate 1, outside this run.
- FC-004/FC-005/FC-021: the rule only inserts a line break between existing lines and moves no trivia. FC-006, FC-022–FC-027, FC-029, FC-036, FC-018: no rewrite of code, literals, identifiers, or names. FC-007/FC-008/FC-009/FC-030: RH5031's code fix is unchanged in behavior and text-based (`RegionDirectiveBlankLineCodeFixProviderBase`, no formatter call); its existing `GetFixAllScenario` covers Fix All, and the new analyzer tests use `Verify`, which re-analyzes the fixed source. FC-010/FC-017/FC-033: every formatter test uses `AssertRuleResult`/`AssertBlankLinesAbove`/`VerifyFormatter`, which run LF and CRLF, a second pass, and `PhaseStabilityCheck`. FC-019/FC-020: the matrix carries comments, `#pragma`, `#nullable`, and all four conditional directives in the gap. FC-015/FC-016: there is no Core API yet to test directly; the RH5031 analyzer guards pin Core's current behavior for the same shapes, so a shared predicate that changes it fails them.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Regression.BlankLines.RegionDirectiveLineAboveTests|FullyQualifiedName~Reihitsu.Formatter.Test.Regression.Regions.DisabledRegionLayoutTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5031RegionDirectivesShouldBePrecededByABlankLine"
```

On 8ac4176: formatter 17 failed / 8 passed (the 17 are the "fail" rows above); analyzer 3 failed / 22 passed (the three `VerifyFormatterSeparates…` rows).

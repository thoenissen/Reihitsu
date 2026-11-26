Role: test implementer
Commit: 217d49f
Verdict: tests written

My prompt carried no intended fix or verdict beyond maintainer answer C (`00-lead.md`). I did not use the spec author's notes for the implementer (01, 03 *Internal choices*).

## Tests

Expected now = on 217d49f, before the implementer. Every "red" test fails on the reason the change addresses: "Iterative code fix application … did not match", where the diagnostics match and the only diff is the comma the fix still keeps, because the fix is withheld today.

RH5409 (`tests/Reihitsu.Analyzer.Test/Formatting/RH5409FinalEnumMemberMustNotHaveTrailingCommaAnalyzerTests.cs`, on `Verify`, which also runs Fix All and iterates to convergence):

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `VerifyEnumWithConditionalBlockAfterFinalMemberIsReportedWithoutFix` (changed: only its `withoutSymbol` half remains) | SHARED-TRAIL-002, SHARED-DIAG-008 | `VerifyNoCodeFixIsOffered` | green |
| `VerifyCommaOfFinalMemberInsideConditionalBlockIsFixedWhereTheSymbolIsDefined` (new: the former `withSymbol` half, now fixed) | SHARED-TRAIL-004, SHARED-DIAG-008 | `Verify`, `SYMBOL` defined | red |
| `VerifySpecificationExampleRemovesCommaInFrontOfEndIfWhereTheSymbolIsDefined` (renamed and flipped from `…ReportsCommaInFrontOfEndIfWhereTheSymbolIsDefined`), LF and CRLF | SHARED-TRAIL-004, SHARED-DIAG-008 | `Verify`, `DEBUG` defined | red |
| `VerifyCommaInFrontOfEndIfOfAnActiveBlockIsReportedAndFixed` (renamed and flipped from `…IsReportedWithoutFix`), LF and CRLF | SHARED-TRAIL-004, SHARED-DIAG-008 | `Verify` | red |
| `VerifySpecificationExampleRemovesCommaAtTheEndOfAnActiveElseBranch`, LF and CRLF | SHARED-TRAIL-004 | `Verify` | red |
| `VerifySpecificationExampleRemovesCommaInFrontOfElif` (disabled text in the gap), LF and CRLF | SHARED-TRAIL-004 | `Verify` | red |
| `VerifyCommaInFrontOfElifAndElseBranchesWithoutIfIsReportedAndFixed` | SHARED-TRAIL-004 | `Verify` | red |
| `VerifyFixAllRemovesCommasAtTheEndOfActiveBranches` (change doc's `E`/`F` Fix All), LF and CRLF | SHARED-TRAIL-004; FC-008 | `Verify` | red |
| `VerifyFixAllRemovesCommaInFrontOfEndIfAndKeepsCommaInFrontOfIf` (a fixable comma next to a withheld one) | SHARED-TRAIL-002, SHARED-TRAIL-004; FC-008 | `Verify`, markup kept in the fixed state | red |
| `VerifySpecificationExampleCommaInFrontOfEndIfAndFurtherIfIsReportedWithoutFix` | SHARED-TRAIL-002 | `VerifyNoCodeFixIsOffered` | green |
| `VerifySpecificationExampleIfInExcludedCodeWithholdsTheFix` (`#if Y` behind `#else`) | SHARED-TRAIL-002 | `VerifyNoCodeFixIsOffered` | green |
| `VerifyIfInCodeAnElifExcludesWithholdsTheFix` (`#if Z` behind `#elif`) | SHARED-TRAIL-002 | `VerifyNoCodeFixIsOffered` | green |
| `VerifyCommaInFrontOfTwoNestedEndIfDirectivesIsReportedAndFixed` | SHARED-TRAIL-004; FC-027 | `Verify` | red |
| `VerifyCommaInFrontOfEndIfAndRegionPairIsReportedAndFixed` | SHARED-TRAIL-004; FC-020 | `Verify` | red |
| `VerifyCommaOnOwnLineInFrontOfEndIfIsReportedAndFixed` (the `#if` in front of the comma is outside the gap) | SHARED-TRAIL-004; FC-001 | `Verify` | red |
| `VerifyCommaWithCommentInFrontOfEndIfIsReportedAndFixed` | SHARED-TRAIL-004; FC-019, FC-004 | `Verify` | red |

RH5410 (`…/Formatting/RH5410FinalArrayInitializerItemsMustNotHaveTrailingCommasAnalyzerTests.cs`):

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `VerifyArrayWithConditionalBlockAfterFinalItemIsReportedWithoutFix` (changed: only its `withoutSymbol` half remains) | SHARED-TRAIL-002, SHARED-DIAG-008 | `VerifyNoCodeFixIsOffered` | green |
| `VerifyCommaOfFinalItemInsideConditionalBlockIsFixedWhereTheSymbolIsDefined` (new: the former `withSymbol` half) | SHARED-TRAIL-004, SHARED-DIAG-008 | `Verify`, `SYMBOL` defined | red |
| `VerifyCommaInFrontOfEndIfOfAnActiveBlockIsReportedAndFixed` (change doc RH5410 example), LF and CRLF | SHARED-TRAIL-004 | `Verify` | red |
| `VerifyInnerCommaInFrontOfEndIfAndOuterCommaAreReportedAndFixed` | SHARED-TRAIL-004; FC-027, FC-008 | `Verify` | red (outer comma already removed; inner kept) |

RH5411 (`…/Formatting/RH5411FinalCollectionInitializerItemsMustNotHaveTrailingCommasAnalyzerTests.cs`):

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `VerifyCollectionWithConditionalBlockAfterFinalItemIsReportedWithoutFix` (changed: only its `withoutSymbol` half remains) | SHARED-TRAIL-002, SHARED-DIAG-008 | `VerifyNoCodeFixIsOffered` | green |
| `VerifyCommaOfFinalItemInsideConditionalBlockIsFixedWhereTheSymbolIsDefined` (new: the former `withSymbol` half) | SHARED-TRAIL-004, SHARED-DIAG-008 | `Verify`, `SYMBOL` defined | red |
| `VerifyCommaAtTheEndOfAnActiveElseBranchIsReportedAndFixed` (change doc RH5411 example), LF and CRLF | SHARED-TRAIL-004 | `Verify` | red |

Formatter stays as on `main` (answer C):

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH5409…FormatterTests.VerifyCommaAtTheEndOfAnActiveElseBranchIsReportedAndKeptByTheFormatter` (`Level`) | FMT-STRUCT-019, SHARED-TRAIL-004; FC-013 | `VerifyFormatterKeepsReportedShape` (LF, CRLF) | green |
| `RH5409…FormatterTests.VerifyCommaInFrontOfElifIsReportedAndKeptByTheFormatter` | FMT-STRUCT-019, SHARED-TRAIL-004 | `VerifyFormatterKeepsReportedShape` | green |
| `RH5410…FormatterTests.VerifyCommaInFrontOfEndIfOfAnActiveBlockIsReportedAndKeptByTheFormatter` (change doc input) | FMT-STRUCT-020, SHARED-TRAIL-004 | `VerifyFormatterKeepsReportedShape` | green |
| `RH5411…FormatterTests.VerifyCommaAtTheEndOfAnActiveElseBranchIsReportedAndKeptByTheFormatter` (change doc input) | FMT-STRUCT-020, SHARED-TRAIL-004 | `VerifyFormatterKeepsReportedShape` | green |
| `RH5303CollectionInitializerShouldBeFormattedCorrectlyAnalyzerTests.VerifyCodeFixKeepsTrailingCommaInFrontOfEndIfBehindActiveElseBranch` (change doc RH5303 example) | FMT-STRUCT-020, SHARED-TRAIL-004; FC-032 | `Verify` | green |
| `Formatter.Test/Regression/Structural/StructuralTransformRequirementTests.SpecificationExampleOfEnumTrailingCommasKeepsCommasInFrontOfConditionalDirectives` (the whole FMT-STRUCT-019 example, `Mode` and `Level` included) | FMT-STRUCT-019; FC-010 | `AssertRuleResult` (LF, CRLF, second pass, phase stability) | green |

`scripts/test.sh --project architecture` is green again: SHARED-TRAIL-004 is now tagged (`EveryRequirementOfAnEnforcedSpecIsCovered`).

## Scenarios (bug fix)

_N/A_ — behavior change.

## Existing tests checked against the changed outputs

- Flipped (changed above): RH5409 `VerifySpecificationExampleReportsCommaInFrontOfEndIfWhereTheSymbolIsDefined`, RH5409 `VerifyCommaInFrontOfEndIfOfAnActiveBlockIsReportedWithoutFix`, and the `withSymbol` halves of `VerifyEnumWithConditionalBlockAfterFinalMemberIsReportedWithoutFix` / `VerifyArrayWithConditionalBlockAfterFinalItemIsReportedWithoutFix` / `VerifyCollectionWithConditionalBlockAfterFinalItemIsReportedWithoutFix`. `rg -A1 '\{\|(RH54(09|10|11)|#\d):,\|\}' tests | rg '#(endif|else|elif)'` finds no other comma marked in front of one of these directives.
- Kept, still green (their gap holds an `#if`): RH5409 `VerifyCommaInFrontOfElseAndElifBranchesIsReportedWithoutFix` (`#if SYMBOL` directly behind the comma), `VerifyCommaInFrontOfEmptyConditionalBlockIsReportedWithoutFix`, `VerifyCommaInFrontOfInactiveIfFalseBlockIsReportedWithoutFix`, `VerifySpecificationExampleReportsCommaInFrontOfRegionAndInFrontOfConditionalBlock`, and `VerifyFixAllKeepsOnlyTheCommaInFrontOfAConditionalBlock` in all three rules.
- Kept, must stay green under C (formatter unchanged): `StructuralTransformPhaseIntegrationTests.PreservesTrailingCommaWhenElseBranchFollowsFinalElement`, `PreservesTrailingCommaWhenActiveIfTrueBranchFollowsFinalElement`, `PreservesTrailingCommaInEnumWhenActiveIfTrueBranchFollowsFinalMember`, `PreservesTrailingCommaWhenElementFreeConditionalRegionFollowsFinalElement`; `Core.Test/SyntaxTriviaUtilitiesTests.ContainsConditionalCompilationBoundary*` (the formatter transforms keep calling that predicate).

## Not covered, and why

- **Directive-kind matrix only on RH5409.** RH5410 and RH5411 get the change doc examples, the former `withSymbol` case, and (RH5410) the nested case, not the full matrix. Static proof: `rg -n "TrailingCommaRemovalCodeFixProviderBase" src` shows all three providers derive from it, and the guard sits in its `sealed override RegisterCodeFixesCoreAsync`, which calls `TrailingCommaCodeFixHelper.CommaPrecedesConditionalCompilationBoundary`. No override exists, and the method does not switch on the rule.
- **`#pragma` combined with `#endif`.** The region pair covers the non-conditional side of the condition. `VerifyCommaInFrontOfPragmaDirectivesIsReportedAndFixed` pins `#pragma` alone. Both reach the same kind check.
- **CLI end-to-end.** Formatter output does not change. The whole-pipeline spec example test and the `VerifyFormatterKeepsReportedShape` tests pin it.
- **FC-038–043 (unsupported shapes).** Nothing is declared.
- **FC-018, FC-036 (rename), FC-022–026, FC-029, FC-006.** Not reachable: the fix deletes one comma token's text.
- **FC-028, FC-040.** The formatter is unchanged, and the check reads directive kinds, which the documentation mode does not affect.
- **FC-031 (stale comments) is the implementer's.** The inline comment in `TrailingCommaRemovalCodeFixProviderBase.RegisterCodeFixesCoreAsync` and the summary of `TrailingCommaCodeFixHelper.CommaPrecedesConditionalCompilationBoundary` describe today's withholding rule. The test summaries I changed are updated.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5409|FullyQualifiedName~RH5410|FullyQualifiedName~RH5411|FullyQualifiedName~RH5303"
scripts/test.sh --project formatter --filter "FullyQualifiedName~StructuralTransformRequirementTests|FullyQualifiedName~StructuralTransformPhaseIntegrationTests"
scripts/test.sh --project core --filter "FullyQualifiedName~SyntaxTriviaUtilitiesTests"
scripts/test.sh --project architecture
```

On 217d49f the analyzer run gives 17 failed and 169 passed. The 17 failures are the "red" rows above. The formatter run (41) and the architecture run (198) are green.

## Follow-up candidates

_None._

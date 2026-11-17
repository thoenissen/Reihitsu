Role: test implementer
Commit: the commit that adds this file (on top of 4430f29)
Verdict: tests written

Repair round for decisions 7 and 8 of `00-lead.md`. Inputs: `00-lead.md`, `04-test-implementer.md`, `05-implementer.md` (*Disputed tests or scope*), `06-spec-author.md`. The prompt named the change and carried no suspected cause or hoped-for verdict beyond the two decisions. No assertion changed meaning; no production code was touched.

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH7102 `FixAllMovesOnlyTheConstantWithoutAConditionalDirectiveInItsModifierList` | RH7102-001, SHARED-DIAG-008 (unchanged tags) | `Verify(source, fixed, onConfigure)` | green |
| RH7103 `FixAllMovesOnlyTheStaticMembersWithoutAConditionalDirectiveInTheirModifierLists` | RH7103-001, SHARED-DIAG-008 | same | green |
| RH7105 `FixAllCorrectsOnlyTheDeclarationWithoutAConditionalDirectiveBetweenItsModifiers` | RH7105-001, SHARED-DIAG-008 | same | green |
| RH7106 `FixAllCorrectsOnlyTheDeclarationWithoutAConditionalDirectiveBetweenItsModifiers` | RH7106-001, SHARED-DIAG-008 | same | green |
| RH7107 `ConditionalDirectiveBetweenTheAccessorsOrInsideABodyDoesNotKeepTheGetAccessorFromBeingReported` | RH7107-001, RH7107-005, SHARED-DIAG-008 | same | green |
| RH7107 `FixAllMovesOnlyTheGetAccessorsWhoseMoveKeepsTheirDirectivesPaired` | RH7107-001, RH7107-005, SHARED-DIAG-008 | same | green |
| RH7108 `ConditionalDirectiveBetweenTheAccessorsOrInsideABodyDoesNotKeepTheAddAccessorFromBeingReported` | RH7108-001, RH7108-005, SHARED-DIAG-008 | same | green |
| RH7108 `FixAllMovesOnlyTheAddAccessorsWhoseMoveKeepsTheirDirectivesPaired` | RH7108-001, RH7108-005, SHARED-DIAG-008 | same | green |
| RH5102 `VerifyFixAllLaysOutOnlyTheArgumentListsWithoutAConditionalDirective` | RH5102-004, SHARED-DIAG-008 | same | green |
| RH5109 `VerifyFixAllLaysOutOnlyTheParameterListsWithoutAConditionalDirective` | RH5109-004, SHARED-DIAG-008 | same | green |
| RH5111 `VerifyFixAllJoinsOnlyTheDeclaratorsWithoutAConditionalDirective` | RH5111-005, SHARED-DIAG-008 | same | green |
| RH5409 `VerifySpecificationExampleReportsCommaInFrontOfRegionAndInFrontOfConditionalBlock` | unchanged tags | same (plus the existing extra diagnostic) | green |
| RH5409, RH5410, RH5411 `VerifyFixAllKeepsOnlyTheCommaInFrontOfAConditionalBlock` | unchanged tags | same | green |
| RH5506 `VerifyFixAllSplitsOnlyTheListsWithoutAConditionalDirective` | unchanged tags | same | green |
| RH5524 `VerifyFixAllMergesOnlyTheListsWithoutAConditionalDirective` | unchanged tags | same | green |
| RH7101 `FixAllSplitsOnlyTheDeclarationsWithoutAConditionalDirectiveBetweenTheirDeclarators` | RH7101-001, SHARED-DIAG-008 | same | green |
| RH8201 `VerifySpecificationExampleConditionalInterfaceListWithholdsTheFixOnlyForImplicitImplementations` | unchanged tags | same (plus the existing extra diagnostic) | green |
| RH7102 `ConstantWhoseModifiersAnIfSplitsIntoAnInstanceFieldIsReportedWithoutFix` (LF, CRLF) | RH7102-001, SHARED-DIAG-008 | `VerifyNoCodeFixIsOffered`, `VerifyWithPreprocessorSymbols(…, "DEBUG")` | green |

Decision 7: each of the 19 tests now passes the configuration the repository already uses where a fixed source keeps a diagnostic (`RH7101DoNotCombineFieldsAnalyzerTests.cs`, `RH5030…Tests.cs`): `test.FixedState.MarkupHandling = MarkupMode.Allow; test.BatchFixedState.MarkupHandling = MarkupMode.Allow;` through the `onConfigure` overload of `Verify`, with the same one-line comment. The marked span in the fixed source is now an expected diagnostic of the fixed state and of the Fix All state, so the remaining diagnostic is checked by ID and location. `using Microsoft.CodeAnalysis.Testing;` was added where the file lacked it. Sources, fixed sources, and markup are unchanged.

Decision 8: `ConstantWhoseModifiersAnIfSplitsIntoAnInstanceFieldIsReportedWithoutFix` follows the repaired RH7102-001 example: `public struct S` → `public class S`; the summary now says the `DEBUG` build compiles `K` as a readonly instance field of the class.

Failing for the right reason before the change (stash of these edits, same filter): 21 failed, each the 19 with `Diagnostics of fixed state — expected "0" actual "1"` as `05-implementer.md` describes, and the two RH7102 rows on `CS8983`. After: all pass.

## Scenarios (bug fix)

_N/A._ Repair round.

## Not covered, and why

- Other tests of this run with the decision-8 defect: none. The test harness compiles every build a test runs with the default `CompilerDiagnostics.Errors` (`rg -n CompilerDiagnostics src/Reihitsu.Analyzer.Test/Base src/Reihitsu.Analyzer.Test/Verifiers` finds only `CodeFixWarningWaveCheckTests.cs:62`, outside this run), so a source that does not compile in a build a test runs fails that test. On `233dfda` the only such failures in the analyzer suite were the two RH7102 rows (`05-implementer.md`: 21 failures in the full analyzer run, 19 of them decision 7). The RH8201 test derived from the change doc's example already carries the `#if FEATURE` `Clone` member that `06-spec-author.md` added to the change doc.
- `BalancedConditionalDirectivePairIn…MovesWithIt` (RH7107, RH7108): their fixed sources keep no diagnostic, so they need no markup handling; unchanged.

## Commands

```shell
scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~.RH7102ConstantsMustAppearBeforeFieldsAnalyzerTests.|FullyQualifiedName~.RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzerTests.|FullyQualifiedName~.RH7105DeclarationKeywordsMustFollowOrderAnalyzerTests.|FullyQualifiedName~.RH7106ProtectedMustComeBeforeInternalAnalyzerTests.|FullyQualifiedName~.RH7107PropertyAccessorsMustFollowOrderAnalyzerTests.|FullyQualifiedName~.RH7108EventAccessorsMustFollowOrderAnalyzerTests.|FullyQualifiedName~.RH5102ArgumentsShouldBeOnSingleOrSeparateLinesAnalyzerTests.|FullyQualifiedName~.RH5109ParametersMustBeOnSameLineOrSeparateLinesAnalyzerTests.|FullyQualifiedName~.RH5111AssignmentsMustHaveProperLineBreaksAnalyzerTests.|FullyQualifiedName~.RH5409FinalEnumMemberMustNotHaveTrailingCommaAnalyzerTests.|FullyQualifiedName~.RH5410FinalArrayInitializerItemsMustNotHaveTrailingCommasAnalyzerTests.|FullyQualifiedName~.RH5411FinalCollectionInitializerItemsMustNotHaveTrailingCommasAnalyzerTests.|FullyQualifiedName~.RH5506ClassAttributeListsMustFollowShapeRulesAnalyzerTests.|FullyQualifiedName~.RH5524ParameterAttributeListsMustFollowShapeRulesAnalyzerTests.|FullyQualifiedName~.RH7101DoNotCombineFieldsAnalyzerTests.|FullyQualifiedName~.RH8201InheritdocShouldBeUsedAnalyzerTests."
```

Result: `Passed: 1086, Failed: 0`.

## Follow-up candidates

_None._

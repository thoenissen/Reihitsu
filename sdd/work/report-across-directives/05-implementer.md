Role: implementer
Commit: 233dfda
Verdict: test disputed

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Inputs: `00-lead.md`, `01-spec-author.md` through `04-test-implementer.md`, the change doc `sdd/changes/2026-11/report-across-conditional-directives.md`.

On `233dfda`, 164 of the 185 red tests pass. The other 21 fail for two test-side reasons only (*Disputed tests or scope*); with the first one bypassed locally they all pass, so the production change is complete for them.

## Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer/Rules/Organization/RH7105DeclarationKeywordsMustFollowOrderAnalyzer.cs`, `RH7106ProtectedMustComeBeforeInternalAnalyzer.cs` | Dropped the `HasConditionalDirectiveBetweenModifiers` guard. The fix already refuses (`ModifierOrderingCodeFixProviderBase.ReorderingWouldDropCommentOrDirective`) | RH7105-001, RH7106-001, SHARED-DIAG-008, SHARED-MODORDER-003 |
| `src/Reihitsu.Analyzer/Rules/Organization/RH7102ConstantsMustAppearBeforeFieldsAnalyzer.cs`, `RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzer.cs` | Dropped the `HasModifiersSplitByConditionalDirective` guard. The ordering fix already refuses (ordering requirements) | RH7102-001, RH7103-001, SHARED-DIAG-008 |
| `src/Reihitsu.Analyzer/Rules/Organization/RH7107PropertyAccessorsMustFollowOrderAnalyzer.cs`, `RH7108EventAccessorsMustFollowOrderAnalyzer.cs` | Dropped the `ContainsConditionalCompilationBoundaryOutsideBodies` guard. The move is decided by the existing ordering-move safety (SHARED-ORDER-003/006) | RH7107-001/005, RH7108-001/005, SHARED-DIAG-008 |
| `src/Reihitsu.Analyzer/Rules/Layout/RH5102…Analyzer.cs`, `RH5109…Analyzer.cs`, `RH5111…Analyzer.cs` | Dropped the conditional-directive guards on list gaps and on the gaps around `=`. The fixes already refuse any directive in those gaps | RH5102-004, RH5109-004, RH5111-005, SHARED-DIAG-008, SHARED-DIAG-010 |
| `src/Reihitsu.Analyzer/Rules/Organization/RH7101DoNotCombineFieldsAnalyzer.cs` | Dropped the guard between the declarators. The fix is withheld by SHARED-FIX-010 (FMT-UNSUP-010) | RH7101-001, SHARED-DIAG-008 |
| `src/Reihitsu.Analyzer/Base/TargetAttributeListShapeAnalyzerBase.cs` | Dropped both guards: in the list's shape (split mode) and between the first list of the target and this one (merge mode) | SHARED-ATTR-019, SHARED-ATTR-020, SHARED-DIAG-008 |
| `src/Reihitsu.Analyzer/Base/TrailingCommaAnalyzerBase{TNode}.cs`, `RH5409/RH5410/RH5411…Analyzer.cs` | Dropped the guard on the gap between the last comma and the brace. Removed the abstract `GetCloseBraceToken` and its three overrides, which nothing used any more | SHARED-TRAIL-002, SHARED-DIAG-008 |
| `src/Reihitsu.Analyzer.CodeFixes/Base/TrailingCommaRemovalCodeFixProviderBase.cs`, `Core/TrailingCommaCodeFixHelper.cs` | The fix is no longer registered (single fix and Fix All) when a conditional directive or excluded code lies between the comma and the token behind it: new `TrailingCommaCodeFixHelper.CommaPrecedesConditionalCompilationBoundary` | SHARED-TRAIL-002, decision 4 |
| `src/Reihitsu.Analyzer/Rules/Organization/RH7311RegionDescriptionsMustBeUniqueWithinATypeAnalyzer.cs` | Compares only *active* region directives (`IsActive: true`) in place of skipping every region in a branch. Roslyn parses `#region` in an excluded branch as an inactive directive, so the excluded branch is not compared, and two branches are never compared with each other | RH7311-001, SHARED-DIAG-008 |
| `src/Reihitsu.Analyzer/Rules/Documentation/RH8201InheritdocShouldBeUsedAnalyzer.cs` | `InheritsDocumentation` no longer excludes implicit implementations in files with `#if`. `HasConditionalInterfaces` moved to the fix provider | RH8201-003, SHARED-DIAG-008 |
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Documentation/RH8201InheritdocShouldBeUsedCodeFixProvider.cs` | New `IsImplicitImplementationWithConditionalInterfaces`: the fix is not registered for a member that is neither `override` nor an explicit implementation when a file declaring its type or one of its interfaces holds `#if`. Uses the document's semantic model | RH8201-003, decision 4 |
| `src/Reihitsu.Core/OrderingDeclarationUtilities.cs` | Deleted `HasModifiersSplitByConditionalDirective` and `HasConditionalDirectiveBetweenModifiers` (no remaining caller) | — |
| `src/Reihitsu.Core/AccessorOrderingUtilities.cs` | Deleted `ContainsConditionalCompilationBoundaryOutsideBodies` and its private `GetBodySpan` (no remaining caller) | — |
| `src/Reihitsu.Core/RegionDirectiveUtilities.cs` | Deleted `IsInConditionalBranch` (no remaining caller) | — |
| `src/Reihitsu.Core/SyntaxTriviaUtilities.cs` | Deleted `LeadingTriviaContainsConditionalCompilationBoundary` and `ContainsConditionalCompilationBoundaryInElementGaps` (no remaining caller) | — |

Every changed method's summary and inline comments were re-read and now describe the new code (FC-031). Unused `using` directives left by the guards were removed.

## Policy owners

- Conditional directive or excluded code in a span: `rg -n "ContainsConditionalCompilationBoundary" src --glob '!**/bin/**' --glob '!**/obj/**' --glob '!*Test*/**'`. The one owner is `SyntaxTriviaUtilities.ContainsConditionalCompilationBoundary`. It is called by `EnumTrailingCommaRemovalTransform` and `InitializerTrailingCommaRemovalTransform` (the formatter keeps the comma, FMT-STRUCT-019/020) and now by `TrailingCommaCodeFixHelper.CommaPrecedesConditionalCompilationBoundary`. All three build the same gap (comma end to the next token's start) at their own call site. The formatter sites take the closing brace and the fix takes `GetNextToken()`, which is the same token because directives and disabled text are trivia.
- `#if` in a file of the type or its interfaces (RH8201): `rg -n "HasConditionalInterfaces" src`. It is now owned only by `RH8201InheritdocShouldBeUsedCodeFixProvider`, moved there from the analyzer and not copied.
- Conditional-directive checks for modifiers, accessors, and list gaps: `rg -n "HasModifiersSplitByConditionalDirective|HasConditionalDirectiveBetweenModifiers|ContainsConditionalCompilationBoundaryOutsideBodies|ContainsConditionalCompilationBoundaryInElementGaps|LeadingTriviaContainsConditionalCompilationBoundary|IsInConditionalBranch" src`. No owner is left; all were deleted. `RH2001PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider` keeps its own private `HasConditionalDirectiveBetweenModifiers`, a fix guard this run does not touch.

## Production diff size

`git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'` reports 24 files changed, 141 insertions(+), 329 deletions(-). There is no new type. New members: `TrailingCommaCodeFixHelper.CommaPrecedesConditionalCompilationBoundary` (internal) and `RH8201InheritdocShouldBeUsedCodeFixProvider.IsImplicitImplementationWithConditionalInterfaces` (private). The public `Reihitsu.Core` helpers listed above were deleted.

## Deleted special cases

_N/A._ The run declares no shape unsupported. The deleted helpers are listed under *Changes*.

## Tests run

```shell
scripts/test.sh --configuration Debug --project analyzer --filter "<the five filters of 04-test-implementer.md, joined>"
```
`Failed: 21, Passed: 1548, Total: 1569`. The 21 failures are exactly the ones under *Disputed tests or scope*.

```shell
scripts/test.sh --configuration Debug --project analyzer
```
`Failed: 21, Passed: 10563, Total: 10584`. The same 21 failures; there are no other regressions.

```shell
scripts/test.sh --configuration Debug --project core          # Passed: 738
scripts/test.sh --configuration Debug --project architecture  # Passed: 185
```

Experiment (reverted, not committed): with `FixedState.MarkupHandling` and `BatchFixedState.MarkupHandling` set to `MarkupMode.Allow` in `AnalyzerTestsBase<TAnalyzer, TCodeFix>.Verify(source, fixedSource, …)`, the run `--filter "FullyQualifiedName~FixAll|FullyQualifiedName~ConditionalDirectiveBetweenTheAccessorsOrInsideABody|FullyQualifiedName~VerifySpecificationExampleReportsCommaInFrontOfRegionAndInFrontOfConditionalBlock|FullyQualifiedName~VerifySpecificationExampleConditionalInterfaceListWithholdsTheFixOnlyForImplicitImplementations"` gave `Passed: 613, Failed: 0`. This includes all 19 tests in item 1 below, `BalancedConditionalDirectivePair…MovesWithIt` included.

Self-hosting was not run. No production source under `src/` holds `#if` outside test string literals, so the newly reporting rules have nothing to find there.

## Disputed tests or scope

1. **Remaining diagnostics in the fixed source are ignored (19 tests).** These tests write the diagnostic that stays after the fix as `{|RHxxxx:…|}` markup in the *fixed* source. The testing library ignores markup for fixable diagnostic IDs in `FixedState`/`BatchFixedState` unless `MarkupHandling = MarkupMode.Allow` is set. The repository sets it wherever a fixed source keeps a diagnostic, for example `RH7101DoNotCombineFieldsAnalyzerTests.cs:2621`, `RH5030…Tests.cs:1343`, and `RH8309…Tests.cs:1288`. These tests do not set it, so each fails with `Context: Diagnostics of fixed state — expected "0" actual "1"`, and the one actual diagnostic is exactly the one the markup marks. The tests:
   - RH7102 `FixAllMovesOnlyTheConstantWithoutAConditionalDirectiveInItsModifierList`
   - RH7103 `FixAllMovesOnlyTheStaticMembersWithoutAConditionalDirectiveInTheirModifierLists`
   - RH7105 and RH7106 `FixAllCorrectsOnlyTheDeclarationWithoutAConditionalDirectiveBetweenItsModifiers`
   - RH7107 `ConditionalDirectiveBetweenTheAccessorsOrInsideABodyDoesNotKeepTheGetAccessorFromBeingReported` and `FixAllMovesOnlyTheGetAccessorsWhoseMoveKeepsTheirDirectivesPaired`
   - RH7108 `…DoesNotKeepTheAddAccessorFromBeingReported` and `FixAllMovesOnlyTheAddAccessorsWhoseMoveKeepsTheirDirectivesPaired`
   - RH5102 `VerifyFixAllLaysOutOnlyTheArgumentListsWithoutAConditionalDirective`
   - RH5109 `VerifyFixAllLaysOutOnlyTheParameterListsWithoutAConditionalDirective`
   - RH5111 `VerifyFixAllJoinsOnlyTheDeclaratorsWithoutAConditionalDirective`
   - RH5409 `VerifySpecificationExampleReportsCommaInFrontOfRegionAndInFrontOfConditionalBlock` and `VerifyFixAllKeepsOnlyTheCommaInFrontOfAConditionalBlock`
   - RH5410 and RH5411 `VerifyFixAllKeepsOnlyTheCommaInFrontOfAConditionalBlock`
   - RH5506 `VerifyFixAllSplitsOnlyTheListsWithoutAConditionalDirective`
   - RH5524 `VerifyFixAllMergesOnlyTheListsWithoutAConditionalDirective`
   - RH7101 `FixAllSplitsOnlyTheDeclarationsWithoutAConditionalDirectiveBetweenTheirDeclarators`
   - RH8201 `VerifySpecificationExampleConditionalInterfaceListWithholdsTheFixOnlyForImplicitImplementations`

   The assertions are right; the test setup is missing. Route it to the test implementer: pass the `MarkupMode.Allow` configuration as the repository's other tests do. With it, all 19 pass on `233dfda` (experiment above).
2. **RH7102 `ConstantWhoseModifiersAnIfSplitsIntoAnInstanceFieldIsReportedWithoutFix` (LF and CRLF rows) does not compile in the `DEBUG` build.** The input is the change doc's and RH7102-001's example: `public struct S { private int _x; private #if DEBUG readonly #else const #endif int K = 1; }`. With `DEBUG` defined, `K` is `private readonly int K = 1;`, a field initializer in a struct without an explicit constructor, so the compiler reports `CS8983` at `(1,15)`. `VerifyWithPreprocessorSymbols(…, "DEBUG")` expects no diagnostics, so the test fails on the compiler error, not on RH7102. The non-`DEBUG` half (`VerifyNoCodeFixIsOffered`) passes. The spec example has the same defect. The spec author should change `struct` to `class`, or add a constructor, in the change doc and the RH7102-001 example; the test implementer should then follow.

Scope: the change deletes five public `Reihitsu.Core` helpers that no caller uses any more, and the abstract `TrailingCommaAnalyzerBase<TNode>.GetCloseBraceToken`. The RH8201 fix now requests the semantic model when it registers.

## Architecture findings

_None._

## Follow-up candidates

_None_ beyond those in `00-lead.md`.

Role: test implementer
Commit: fdd89ba
Verdict: tests written

Repair round, items R1 and R2 of `00-lead.md`. These are findings 1 and 3 of `07-auditor.md`. I re-derived both against the spec text and the code before changing anything. Only `<summary>` text changed: no assertion, test name, attribute, or tag. The changes name no tracker number. `scripts/verify-text-only.sh --base HEAD --head worktree` passes for all five files ("changed trivia: documentation"). My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Tests

### R1 — seven contradicting characterization tests (decision `2026-10-21-1260-contradicting-characterization-tests.md`)

Each test below pins the opposite of a requirement in this run. I checked each one against the requirement's normative sentence, using the same wording as `04-test-implementer.md`: "This test pins today's behavior; the decided behavior is stated by <ID> …, and its change belongs to an existing follow-up".

| Test | Requirement | Why it contradicts (re-derived) | Expected now |
|---|---|---|---|
| `RH1001…Tests.VerifyCreationWithComparerArgumentThatCanOnlyProduceNullOrThrowIsNotReported` | SHARED-DIAG-005 | `flag ? null : throw …` is not reported. Under DIAG-005 the `throw` branch yields no value, so `null` decides and the creation would be reported | pass |
| `RH1001…Tests.VerifyDeclarationPickingBetweenCustomComparerCreationAndThrowIsReported` | SHARED-DIAG-005 | `a` (switch with a `throw` arm) and `b` (conditional with a `throw` branch) are reported. Under DIAG-005 only `new(_comparer)` decides, which is custom, so neither would be reported | pass |
| `RH1001…Tests.VerifyCreationWithWrappedAsExpressionComparerArgumentIsNotReported` | SHARED-DIAG-005 | `(null as IEqualityComparer<S>)` is not reported, while the unwrapped form is. DIAG-005 judges parentheses as the expression inside them | pass |
| `RH7407…Tests.VerifyOnlyFirstDeclaratorOfOverrideEventNamesOverrideRegion` | SHARED-DIAG-006 | Only the first declarator `A` of `override event A, X` is checked, so region `B2` (named by `X`) is not treated as an override region. DIAG-006 requires every declarator to be checked | pass |
| `RH7407…Tests.VerifyEventWithSeveralDeclaratorsInOverrideRegionIsReportedOnceAtFirstDeclarator` | SHARED-DIAG-006 | `F, G` is reported once, at `F`. One reading of DIAG-006's "declaration as a whole" clause would allow this. The auditor cites the round-8 note on thoenissen/Reihitsu#1260 and thoenissen/Reihitsu#1333, which name this input as one diagnostic at `F` and one at `G`. The RH7411 sibling was reworded the same way in `04` | pass |
| `RH7410…Tests.VerifyPartialImplementationPropertyIsJudgedAtItsDefiningPart` | SHARED-DIAG-004 | The property is judged only at its defining part, not by its symbol. This is the sibling of the RH7409 test reworded in `04` | pass |
| `RH7105…Tests.MisorderedModifiersWithDirectiveAreReportedWithoutFix` | SHARED-DIAG-008 | `static` / `#if FEATURE` / `#endif` / `public` is reported. DIAG-008 says a conditional directive in the judged modifier list means not reported. The test is now labelled a characterization; its old summary stated the behavior as intended. It is the sibling of the RH7106 test reworded in `04` | pass |

### R2 — RH7305 nested-enum summary

- **Test:** `RH7305TypesShouldBeOrganizedWithRegionsAnalyzerTests.NestedEnumInARegionIsOneMemberWhateverRegionsItHasAmongItsValues` (SHARED-TYPEREGION-002; its tag is kept).
- **Re-derived:** `RegionDirectiveUtilities.GetTopLevelRegions` excludes only the spans of nested `TypeDeclarationSyntax` nodes. An `EnumDeclarationSyntax` is not one of them, so `#region Values` counts toward `C`. It is not a top-level region only because it is nested in `Types`. The old summary claimed a mechanism ("the region among its values belongs to the enum") that the test cannot observe.
- **New summary:** it states only what the test observes. The input is the specification example: a type whose only member is a nested enum that lies in a region and has a region among its values. The observation is that the type is not reported.
- **Expected now:** pass. The test still cannot fail, as `03` and `04` already state.

## Scenarios (bug fix)

_N/A._

## Not covered, and why

- **Remaining "open question" summaries in sibling classes.** `rg -c "open question"` over RH1001, RH1002, RH74xx, RH710x, RH541x, and RH73xx finds ten more. I read each one; none pins the opposite of a requirement in this run, so none was changed:
  - RH1001 generic structs, `with(…)`, and `default(TV)`; RH1002 `ToImmutableDictionary` value comparer, query clauses, and `default(TV)`: none of these is a parentheses, `!`, or `throw` form.
  - RH7401: a lower-case base-type name.
  - RH7405 and RH7406: a region named only by another member kind.
  - RH7405: an override in another partial declaration. These are per-part regions, which SHARED-DIAG-004's coverage sentence leaves out.
  - RH7409: base-list order, which is a P25 instance (thoenissen/Reihitsu#1331, per the auditor).
- **RH5111-005 and RH3001-006** stay unchanged, as decided in `04`.

## Commands

All passed. The architecture run includes the spec author's R3 commit `fdd89ba`.

```shell
scripts/format.sh --force <the 5 changed test files>        # Formatted 0 of 5
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH1001TypesUsedAsKeysMustImplementEqualityMembersAnalyzerTests|FullyQualifiedName~RH7407NonOverrideEventsShouldNotBePlacedInOverrideRegionsAnalyzerTests|FullyQualifiedName~RH7410InterfacePropertiesShouldBeGroupedByInterfaceRegionsAnalyzerTests|FullyQualifiedName~RH7105DeclarationKeywordsMustFollowOrderAnalyzerTests|FullyQualifiedName~RH7305TypesShouldBeOrganizedWithRegionsAnalyzerTests"   # 111 passed
scripts/test.sh --project architecture --filter "FullyQualifiedName~RequirementCoverageTests|FullyQualifiedName~SpecConsistencyTests|FullyQualifiedName~SpecStructureTests"   # 39 passed
scripts/verify-text-only.sh --base HEAD --head worktree        # PASS, 5 C# comment-only files
```

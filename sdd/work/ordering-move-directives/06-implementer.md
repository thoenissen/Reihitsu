Role: implementer
Commit: 44bd79c
Verdict: #970: tests pass; #1668: tests pass

The prompt carried no intended fix, suspected cause, or hoped-for verdict to ignore.

Before the change the handed-over tests failed as `04-test-implementer.md` states (80 red analyzer tests, 2 red Core tests, on f9b2eda). After 44bd79c every one passes; no test was changed.

## thoenissen/Reihitsu#970

### Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Core/OrderingMoveSafety.cs` | New public `ContainsPositionSensitiveDirectivesThatBlockRelocation(root, span, declarations, spansStart)`, the one implementation of SHARED-ORDER-011: a `#pragma warning` or `#nullable` directive in a relocated span is carried only when it stands behind the first token of a declaration lying wholly in the span, the directives of its kind in that declaration form consecutive closed pairs (`FormClosedPairs`: `Closes` compares IDs as written and in order, or the `#nullable` target; `SplitsConditionalGroup` checks *One branch*), and the directives of its kind in the whole document in front of `spansStart` form closed pairs too. Every other non-conditional, non-region directive still refuses. `RelocationChangesDirectiveScope` now calls it with the list nodes and the crossed span's start | SHARED-ORDER-005, -006, -011 |
| `src/Reihitsu.Core/SyntaxTriviaUtilities.cs` | `GetPositionSensitiveDirectives` extracted from `ContainsPositionSensitiveDirectives`, so which directives count as position sensitive stays stated once | SHARED-ORDER-011 |
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Organization/RH7309RegionsShouldFollowCategoryOrderCodeFixProvider.cs` | `ContainsUnsafeDirectives` uses the shared check, with the type's members as declarations and the first region block's start as `spansStart` (decision 1; answer 970-4 B) | RH7309-005, SHARED-ORDER-011 |
| `src/Reihitsu.Analyzer.CodeFixes/Base/TypeMemberOrderingCodeFixProviderBase.cs` | Summary of `IsMoveSafe` only (body unchanged), now naming SHARED-ORDER-011 and the new SHARED-ORDER-007/008 | FC-031 |

### Policy owners

- `rg -n "PragmaWarningDirectiveTrivia|NullableDirectiveTrivia" src --glob '*.cs'`: besides the new code, only `DirectivePairSplitScan` (the brace-insertion split check of `SyntaxTriviaUtilities.SplitsDirectivePair`). It answers a different question (do two inserted braces split a pair), pairs by setting, and normalizes `219` to `CS0219`, which SHARED-ORDER-011 deliberately does not ("comparing IDs as written"). Not reused; a different decision.
- `rg -n "ContainsPositionSensitiveDirectives|GetPositionSensitiveDirectives" src --glob '*.cs'`: `SyntaxTriviaUtilities` (owner) and `OrderingMoveSafety` (only caller of `GetPositionSensitiveDirectives`). `ContainsPositionSensitiveDirectives` now has no production caller; it stays because `SyntaxTriviaUtilitiesTests` pins it.
- The pair rule has exactly two callers, member/accessor moves (`RelocationChangesDirectiveScope`) and RH7309 (`ContainsUnsafeDirectives`), which closes the skeptic's architecture finding as decision 1 routed it.

## thoenissen/Reihitsu#1668

### Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Core/OrderingDeclarationUtilities.cs` | `ChangesMemberOrder(…, Func<int,int>)`: any `#if`/`#elif`/`#else`/`#endif` at a position of the body that the reorder moves or moves code across refuses (decision 7: in `ChangesMemberOrder`, so RH7102, RH7103, RH7109, and RH7309 share it). `GetReorderedSpan` is replaced by `GetCrossedPositions` (prefix maximum / suffix minimum of the ranks); the layout clause reads any conditional directive in the body in front of the first crossed position. `DeclarationHoldsConditionalDirective`: a conditional directive on the first token counts unless `WrapsWholeDeclaration` (an `#if`/`#endif` pair only, `#if` on the first token, `#endif` on the token behind the last token, end of file included). Members are counted from the active tree only | SHARED-ORDER-007, -008 (1668-1 B, 1668-2 A, decision 4) |
| `src/Reihitsu.Core/ConditionalGroupUnitUtilities.cs` | Shrunk to what RH2001-010 still needs: `WrapsWholeUnitsOf` → `WrapsWholeMembers(group, typeDeclaration)` (nesting check always on), `ReadsAsWholeUnits` → `ReadsAsWholeMembers` (type-body arm only); unused usings dropped | RH2001-010 (unchanged behavior) |

Deleted for the member-moving fixes, as the skeptic's *Refuse option* lists: `ConditionalGroupUnitUtilities.WrapsWholeUnits`, `GetBoundaryLists`, `IsUnitListOfTypeBody`, `ForbidsFieldKeyword`, the non-type-body arms of `ReadsAsWholeUnits`; `OrderingDeclarationUtilities.GetMembersOfDisabledText`, `DeclaresTypeWithAttributes`, `GetReorderedSpan`, and the group branches of `ChangesMemberOrder` and `DeclarationHoldsConditionalDirective`. The excluded-text parse remains only behind `ConditionalGroupsWrapWholeMembers` (RH2001-010), as `00-lead.md` records.

### Policy owners

- `rg -n "WrapsWholeMembers|ReadsAsWholeMembers|ParseInTypeBody|GetExcludedText" src --glob '*.cs'`: only `ConditionalGroupUnitUtilities` and its one caller `OrderingDeclarationUtilities.ConditionalGroupsWrapWholeMembers` (RH2001).
- `rg -n "GetRelatedDirectives" src/Reihitsu.Core --glob '*.cs'`: `ConditionalGroupsWrapWholeMembers`, `WrapsWholeDeclaration`, and `OrderingMoveSafety.SplitsConditionalGroup`; each reads the group for its own requirement.
- The crossed-position rule has one owner, `GetCrossedPositions`, used for the spans refusal and the layout clause.

### Interpretation note

SHARED-ORDER-008 says for RH7309 "the region blocks whose place the fix changes and the code between blocks that they cross". `GetCrossedPositions` also counts a block that keeps its place but is crossed by a moved block (blocks `A B C` → `C B A`: `B`). That is the safe reading (members of `A` and `C` change order across `B`'s group), and no test distinguishes it; the spec author may want a sentence. A gap between blocks that no moved block crosses (`A B C D` → `B A D C`, the gap between `B` and `C`) is not counted, as the spec says; the old `GetReorderedSpan` would have counted it.

## Whole run

### Production diff size

`git diff --shortstat origin/main...HEAD -- src`: 6 files changed, 281 insertions(+), 370 deletions(-). No new type. New public member: `OrderingMoveSafety.ContainsPositionSensitiveDirectivesThatBlockRelocation` (needed by RH7309 in the code-fix assembly, decision 1) and `SyntaxTriviaUtilities.GetPositionSensitiveDirectives`.

### Deleted special cases

_N/A._ No shape is declared unsupported (decision 2).

### Tests run

- `scripts/test.sh --configuration Debug --project core --filter "FullyQualifiedName~OrderingDeclarationUtilitiesTests|FullyQualifiedName~AccessorOrderingUtilitiesTests|FullyQualifiedName~SyntaxTriviaUtilitiesTests"`: Passed 118 of 118.
- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH7102|FullyQualifiedName~RH7103|FullyQualifiedName~RH7107|FullyQualifiedName~RH7108|FullyQualifiedName~RH7109|FullyQualifiedName~RH7309"`: Passed 614 of 614.
- `scripts/test.sh --configuration Debug --project core`: Passed 815 of 815.
- `scripts/test.sh --configuration Debug --project architecture`: Passed 198 of 198.
- `scripts/test.sh --configuration Debug --project analyzer`: Passed 11809 of 11809 (RH2001 included).
- `scripts/build.sh --configuration Debug`: 0 warnings, 0 errors. `scripts/format.sh` on the five changed Core/CodeFixes files: 0 of 5 changed.
- `scripts/lane-check.sh behavior-change`: confirmed.

### Disputed tests or scope

_None._ Scope stays inside the files the handovers name; the two new public members are listed above.

### Architecture findings

_None._ The skeptic's finding (pair rule stated twice) is resolved as decision 1 routed it.

### Follow-up candidates

- `SyntaxTriviaUtilities.ContainsPositionSensitiveDirectives` has no production caller after this run; only `SyntaxTriviaUtilitiesTests` uses it. Candidate for removal together with its tests (a test change, so not done here).
- RH2001-010 remains the only excluded-branch parse, as `00-lead.md` already records.

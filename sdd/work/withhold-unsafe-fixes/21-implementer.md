Role: implementer
Commit: cd3fa06
Verdict: tests pass

Structural change of decision 12 (audit finding #9 and its architecture finding). Inputs: `00-lead.md` (decisions 12–14), `13-implementer.md`, `18-auditor.md`, `19-test-implementer.md`, `20-spec-author.md`, SHARED-ORDER-007 and RH7103-007 as committed in bf155df. The prompt carried no intended fix beyond the decision; I took the audit's suggested change ("RH7103 resolves the symbol … or the owner takes the symbol") as a suggestion and chose the second variant, because the first would still leave the flag in callers' hands.

## thoenissen/Reihitsu#1135

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Core/OrderingDeclarationUtilities.cs` | The two public `ChangesMemberOrder` overloads (single move, rank function) take the type's `INamedTypeSymbol` instead of a `bool fieldOrderSetsLayout`, and decide the layout check themselves through `FieldOrderSetsLayout(typeSymbol)`. `FieldOrderSetsLayout` is now `private`, and the flag-taking overloads are `private` implementations. `ChangesInitializerExecutionOrder` (initializers only, no layout) is now `internal`, so no code fix can reach it; it stays for the Core unit tests that call it. Members re-sorted public → internal → private; summaries and `<param>` text rewritten for the new signatures (FC-031) | SHARED-FIX-003 *Run time*, *Build configurations*; SHARED-ORDER-007; RH7103-007 |
| `src/Reihitsu.Core/Reihitsu.Core.csproj` | `<InternalsVisibleTo Include="Reihitsu.Core.Test" />`, so the four existing `ChangesInitializerExecutionOrder` tests compile unchanged | – |
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Organization/RH7103StaticElementsMustAppearBeforeInstanceElementsCodeFixProvider.cs` | `IsMoveSafe` resolves the type symbol and asks `ChangesMemberOrder(typeDeclaration, typeSymbol, …)`, as RH7109 does; without a symbol the fix is withheld, as in RH7109 | RH7103-007 |

### Policy owners
- `rg -n "ChangesInitializerExecutionOrder|ChangesMemberOrder|FieldOrderSetsLayout|fieldOrderSetsLayout" src` (excluding `bin`/`obj`): production callers are only RH7103, RH7109 (single-move overload with the symbol) and `RegionBlockLayout.ChangesMemberOrder` → RH7309 (rank overload with the symbol). `FieldOrderSetsLayout` and the flag appear only inside `OrderingDeclarationUtilities`. `ChangesInitializerExecutionOrder` is called only by `Reihitsu.Core.Test`.
- `rg "StructLayout|LayoutKind" src` outside tests: only `OrderingDeclarationUtilities` (unchanged).

### Tests run
- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH7004Using|FullyQualifiedName~RH7103Static|FullyQualifiedName~RH7109|FullyQualifiedName~RH7309"`: Passed 215, Failed 0 (includes `NoCodeFixWhenExcludedCodeInFrontOfTheMovedStaticFieldWouldReorderTheInstanceFieldsOfAStruct`, which failed before as `19-test-implementer.md` states, and its two counterparts).
- `scripts/test.sh --configuration Debug --project core`: Passed 607, Failed 0.

## thoenissen/Reihitsu#1134

### Changes
| File | What changed | Requirement |
|---|---|---|
| `RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsCodeFixProvider.cs` | Passes the symbol instead of `FieldOrderSetsLayout(typeSymbol)`; same verdicts | RH7109-006 |

Tests: the RH7109 run above, all green; the five new repair-2 tests pass.

## thoenissen/Reihitsu#1262

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer.CodeFixes/Core/RegionBlockLayout.cs` | `ChangesMemberOrder(TypeDeclarationSyntax, INamedTypeSymbol)` forwards the symbol; summary and `<param>` updated | RH7309-006 |
| `RH7309RegionsShouldFollowCategoryOrderCodeFixProvider.cs` | Passes the symbol it already resolved | RH7309-006 |

Tests: the RH7309 run above, all green.

## thoenissen/Reihitsu#1136

Nothing to do in this round: decision 12 does not touch RH7004; the repair-2 RH7004 tests pass.

## thoenissen/Reihitsu#1332

Nothing to do in this round.

## thoenissen/Reihitsu#1336

Nothing to do in this round.

## thoenissen/Reihitsu#1340

Nothing to do in this round.

## Whole run

### Tests run
- `scripts/build.sh` (Release): 0 Warning(s), 0 Error(s).
- `scripts/test.sh` (Release, all projects except self-hosting): Analyzer 6633/6633, Formatter 4517/4517, Core 607/607, Cli 251/251, Architecture 115/115, Tooling 125/125, Playground 17/17 — all passed.
- `scripts/format.sh` over the five changed `.cs` files: 0 of 5 formatted.
- `scripts/lane-check.sh behavior-change`: "LANE CHECK: confirmed".

No test file changed.

### Disputed tests or scope
- Scope note (small): to remove the opt-out completely, `ChangesInitializerExecutionOrder` became `internal` and `Reihitsu.Core` gained `InternalsVisibleTo` for `Reihitsu.Core.Test` (one csproj item). The alternative — keeping it `public` — would leave a layout-free path that a code fix could call again, which is what decision 12 closes; deleting it would require changing four Core tests. The lead may prefer deleting it together with a test-implementer change to those tests.
- Public surface: `FieldOrderSetsLayout` went from `public` to `private`; no other consumer existed.

### Architecture findings
_None._ This change is decision 12's structural change; after it no caller passes the layout flag, and `FieldOrderSetsLayout` has one caller, the owner.

### Follow-up candidates
_None new._ RH7102 (no member-order guard, `18-auditor.md` *Hints*) stays with the lead. With the symbol-taking owner, adding RH7102 would be one `IsMoveSafe` override.

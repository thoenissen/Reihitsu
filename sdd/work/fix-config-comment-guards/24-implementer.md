Role: implementer
Commit: 6b91450
Verdict: tests pass

This is the structural change of decision 24, with decision 25's scoped cap and decision 26's per-configuration judgment. The prompt carried no intended fix, suspected cause, or hoped-for verdict. It named the decisions, the audit, and the handovers to read.

Inputs: `00-lead.md` (decisions 24–26), `22-auditor.md` (#6–#8, *Architecture findings*), `23-test-implementer.md`, `17-implementer.md`, and SHARED-ORDER-007 as the spec author committed it in 9bc4acf.

Before the change, the RH7109 class had 8 tests (16 rows) red, as `23-test-implementer.md` states. No test, assertion, or spec file was changed.

## thoenissen/Reihitsu#1595

No change of its own. RH7102 reaches the guard through `TypeMemberOrderingCodeFixProviderBase.IsMoveSafe` → `OrderingDeclarationUtilities.ChangesMemberOrder`, which is restructured under #1593. All RH7102 tests pass (part of the 382 focused rows below).

## thoenissen/Reihitsu#1593

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Core/OrderingDeclarationUtilities.cs` | **No group is read as text by a prediction from the active tree (#6).** `IsReadAsText`, `WritesOnlyInsideBrackets`, and `GetEnclosingBody` are removed. `IsBalanced` replaces them. It decides from the group's own text, in every branch: either the group is unbalanced (a piece opens or closes a bracket it does not close or open, or the body does not both open and close the group), or it is balanced. A balanced group can also be square-only (its pieces hold only square brackets at depth 0, such as attribute lists). Only square-only groups stay text everywhere: they write no modifier, initializer, or member in any context. An unbalanced group is always read per configuration. A balanced group starts as text. In each configuration's parse, its `#if` is checked: if the parse compiles it outside every bracket of a member (or inside an accessor list), the group is read per configuration as well, and the region's configurations are read again. The parse is the only reader of the context, so the block, expression-body, and nested-type rules disappear. A group that crosses a block, an expression body, or a nested type in one branch is unbalanced (blk1, arr1, nt1). A group that another group's branch moves out of a bracket is found in that configuration's parse (b2, b1s) | SHARED-ORDER-007, SHARED-FIX-003 |
| same | **No join by name (#7, decision 26).** `GetMemberName` and the per-name union are removed. `TryGetOrderedMembersOfRegion` returns the ordered members of each configuration separately (`GetOrderedMembersOfConfiguration`). `ChangesOrder` checks two things. Pairs within one region count only inside one configuration. Pairs of different units count in any combination of configurations: the active members, which every configuration compiles, and different regions, whose groups are read as independent, as before. This is a linear pass that keeps the highest rank and the highest rank of another unit | SHARED-ORDER-007 (per-configuration paragraph), SHARED-FIX-003 |
| same | **Scoped cap (#8, decision 25).** A region over `MaximumConfigurationCount` no longer fails the whole type. It refuses the reorder only when `IsTouched` holds: the rank is not the same over the whole region, or a member outside it, in any configuration, lands on the other side. Otherwise it is left out (far9) | Decision 25 (maintainer's standing answer); SHARED-ORDER-007 limit paragraph |
| same | The public `ChangesMemberOrder(…, Func<int,int>)` summary and the `MaximumConfigurationCount` summary now describe per-configuration judgment and the scoped cap (FC-031) | — |
| `src/Reihitsu.Core/ConditionalGroup.cs` | `IsComplete` (the body opens and closes the group) and `GetSelfAndNestedGroups()` | same |
| `src/Reihitsu.Core/OrderedMember.cs` | Summaries: a member's flags hold for one configuration (or all, for an active member), no longer "in some configuration" | — |

What the structure removes, by finding:

- **#6:**
  - b2: the `TRACE` group is balanced. The `DEBUG` parse compiles it at member level, so it is read per configuration, and `DEBUG`+`TRACE` declares `_z`.
  - b1s: the same, with the side-by-side groups of one member in one region.
  - blk1, arr1, nt1: these groups are unbalanced, so each is read per configuration.
  - The counterparts keep their fix: blk1c, arr1c, and nt1c are balanced inside brackets in every configuration. b2c still offers no fix.
- **#7:**
  - j1s and j2s: each build is judged with what it compiles, so they are fixed with main's output.
  - j1w and j2w: no fix is offered, because one build crosses an initializer of the moved member's context.
- **#8:** far9 is fixed. The nine-group test, whose move crosses the region, offers no fix.

Probes with `scripts/apply-fix.sh RH7109`, LF and CRLF:

| Probe | Result |
|---|---|
| `public int P => 1` / `#if DEBUG ; static int _z = Log("z"); int Q => 2 #endif ;` (bracket-balanced, but the excluded branch ends the expression body) crossed by a static initialized `_c` | no fix offered (the branch is at member level in the parse) |
| A crossed expression body whose `#if` chooses `1;` or `2;`, with nothing initialized of `_c`'s context crossed | fixed |
| Nine `#if` groups of statements in a crossed method body | fixed |
| A balanced argument group in `_a` beside a member-level `TRACE` group declaring a static initialized field | no fix offered |
| A `get` block with an `#if` choosing the return statement | fixed |

### Policy owners
- `rg -n "ChangesMemberOrder\(" src -g '!*Test*'` finds `TypeMemberOrderingCodeFixProviderBase.IsMoveSafe:79` and `RegionBlockLayout.ChangesMemberOrder:71`. Both call `OrderingDeclarationUtilities.ChangesMemberOrder`, so there is one owner, unchanged.
- `rg -n "ParseCompilationUnit\(|ParseTokens\(" src/Reihitsu.Core src/Reihitsu.Analyzer.CodeFixes -g '!*Test*'` finds the following. What a member contributes per configuration has one owner.
  - `OrderingDeclarationUtilities:706`: `IsBalanced`, which is the former bracket rule's token scan.
  - `OrderingDeclarationUtilities:1124`: the configuration parse.
  - `TokenSeparationUtilities:76` and `ExcludedCodeUtilities:119`: these decide something else.
- Bracket depth is owned by `GetNestingDelta`, `GetNestingDepths`, and `ContainsTopLevelToken`, which are reused. `LiesInsideBrackets` now runs on the parsed configuration and no longer on the active tree.

### Tests run
```
scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsAnalyzerTests"
  before: 8 tests (16 rows) red, as 23-test-implementer.md lists
scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH7109|FullyQualifiedName~RH7103|FullyQualifiedName~RH7309|FullyQualifiedName~RH7102"
  Passed! - Failed: 0, Passed: 382, Total: 382
scripts/test.sh --configuration Debug --project analyzer      Passed! - Failed: 0, Passed: 7608
scripts/test.sh --configuration Debug --project core          Passed! - Failed: 0, Passed: 660
scripts/test.sh --configuration Debug --project architecture  Passed! - Failed: 0, Passed: 115
```
The focused run was repeated after the last edit, which changed only summaries. `scripts/format.sh` was run over the three files. The Debug build has no warnings.

### Disputed tests or scope
- **No disputed test.** Every test passed without a change. No test's withholding rested only on the cross-configuration union that decision 26 removes.
- **New withholdings where main offers a fix.** Main offers a fix for the expression-body probe above (`=> 1 #if DEBUG ; static int _z … #endif ;`), and the head withholds it. The cause is a bracket-balanced branch that ends the member. SHARED-FIX-003 demands the withholding, because the `DEBUG` build would reorder initializers. No test pins it. The lead may want one: it is the balanced-but-member-level side of the context check, and b2 and b1s cover that side only through another group.
- **Cost.** A balanced group inside a member, such as an `#if` in a method body, now puts that member into a region. Without the check from the active tree, its `#if` must be located in a parse. The region has one configuration unless the check expands the group, so the cost is one parse per such member when a move is evaluated. The full analyzer run took 2 min 31 s.

### Architecture findings
_None._ This change carries out decision 24. The per-configuration parse is now the only reader of a group's context. No member is joined across configurations; each configuration is judged with what it compiles.

### Follow-up candidates
- **`field` keyword decided in a block or bracket.** This is unchanged from `17-implementer.md`. A balanced group inside an accessor body or an argument list that chooses whether `field` is used stays text, as on main.

## thoenissen/Reihitsu#1616

No change. Audit 3 has nothing for it.

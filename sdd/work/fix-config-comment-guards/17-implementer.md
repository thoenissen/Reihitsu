Role: implementer
Commit: 234e32a
Verdict: tests pass

This is the structural change of decision 21, with the per-configuration reading that decision 22 states. The prompt carried no intended fix, suspected cause, or hoped-for verdict. It named the decisions, the audits, and the handovers to read. Before the change, the three touched test classes had 30 rows red (15 tests), as `16-test-implementer.md` states. No test, assertion, or spec file was changed.

## thoenissen/Reihitsu#1595

No change of its own. RH7102 reaches the guard through `TypeMemberOrderingCodeFixProviderBase.IsMoveSafe` → `OrderingDeclarationUtilities.ChangesMemberOrder`, which is restructured under #1593. All RH7102 tests pass (`FullyQualifiedName~RH7102`, part of the 336 focused rows below).

## thoenissen/Reihitsu#1593

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Core/ConditionalGroup.cs` (new, internal) | One conditional group of a type's body: its boundaries (the directive spans, or an empty span at the start or end of the body for an `#if` in front of the body or an `#endif` behind it), its directives in the body, its nested groups, its branch spans, and whether it has an `#else` | SHARED-ORDER-007, SHARED-FIX-003 |
| `src/Reihitsu.Core/OrderingDeclarationUtilities.cs` | The piecewise readers are replaced: `GetOrderedMembers`, `GetOrderedMembersOfDeclaration`, `IsStaticInExcludedCodeInFront`, `GetNestingDepth`, `GetConditionalTriviaOfDeclaration`, `GetConditionalSpans`, `GetOrderedMembersOfDisabledText`, and `GetOrderedMembersOfText` are removed. New structure: `TryGetOrderedMembers` → `GetConditionalGroups` (reads every group of the body once, with all its branches and nested groups) → `GetConditionalRegions` (each outermost group extended over the members whose full span it overlaps; regions that share a member merge) → `GetConfigurations` (one branch per group, nested groups included, plus the empty branch of a group without `#else`) → `TryAddOrderedMembersOfRegion` (for each configuration, blanks the directives and the other branches of the region's text, which keeps every position, parses it as a type body, and reads each member with the ordinary syntax predicates). A member that the configurations declare under one name counts with what any of them contributes, at every position where one declares it. A `const` configuration contributes no initializer, context, or layout field. Members that no group touches are read from the active code, as before | SHARED-ORDER-007 (split-member bullets, decision 22), SHARED-FIX-003 |
| same | `IsReadAsText`: a group is not read per configuration when every directive lies in one block, expression body, or nested type body (unchanged scope), or when every configuration only writes text inside brackets (`WritesOnlyInsideBrackets`, `LiesInsideBrackets`): every piece between two boundaries is bracket-balanced, and the group either lies inside an active bracket other than an accessor list, or holds nothing outside square brackets (attribute lists, array ranks) | Keeps main's fixes; see *Disputed tests or scope* |
| same | `MaximumConfigurationCount` (256): a region with more configurations counts as reordered, so the fix is withheld | SHARED-FIX-003 (no fix over an unsafe one) |

What the structure removes, by finding:

- **#3 (x9, x4, P→F, P→I):** the group is read as a whole, so an excluded branch that lies in the previous member's span is parsed together with the declaration it continues. In `DEBUG`, `_b` is `private static int _b = Log("b")`.
- **#4a (x1, x1b, x3, the I and `#elif` cases, the moved all-`static` member, RH7309 struct):** each configuration declares its own modifiers, so a `static` or `const` that every branch writes no longer counts as "missing in some configuration".
- **#4b (h4):** `static` is read only as a modifier of a parsed declaration, so a static lambda in an initializer is not a modifier.
- **#5 (h5):** the same holds for a nested static lambda.
- **Decision 22:** a `const` configuration contributes nothing. Probed with `scripts/apply-fix.sh RH7109`, under LF and CRLF:
  - `#if DEBUG internal static readonly #else private const #endif int K = 1;`, crossed by an instance initialized field: **fixed**.
  - The same with `internal readonly` in `DEBUG`: **no fix offered**, because that build declares an instance initializer.

### Policy owners
- `rg -n "ChangesMemberOrder\(" src -g '!*Test*'` finds `TypeMemberOrderingCodeFixProviderBase.IsMoveSafe` and `RegionBlockLayout.ChangesMemberOrder`. Both call `OrderingDeclarationUtilities.ChangesMemberOrder`; there is one owner, unchanged.
- `rg -n "ElseDirectiveTrivia|EndIfDirectiveTrivia" src -g '!*Test*' -l` finds `SyntaxTriviaUtilities` and `RH7004…CodeFixProvider`. These decide whether a span or a gap holds a conditional boundary, which is a different decision. What a member contributes per configuration has one owner: `OrderingDeclarationUtilities` with `ConditionalGroup`.
- `rg -n "ParseCompilationUnit\(" src/Reihitsu.Core src/Reihitsu.Analyzer.CodeFixes -g '!*Test*'` finds only the new region parse; the old `GetOrderedMembersOfText` parse is removed.
- Bracket depth: `GetNestingDelta`, `GetNestingDepths`, and `ContainsTopLevelToken` are the existing owners, reused. `GetNestingDepth`, which walked the active tokens, is removed.

### Tests run
```
scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsAnalyzerTests|FullyQualifiedName~RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzerTests|FullyQualifiedName~RH7309RegionsShouldFollowCategoryOrderAnalyzerTests"
  before: Failed: 30, Passed: 249, Total: 279
scripts/test.sh --configuration Debug --project analyzer --filter "...the three above...|FullyQualifiedName~RH7102"
  Passed! - Failed: 0, Passed: 336, Total: 336
scripts/test.sh --configuration Debug --project analyzer      Passed! - Failed: 0, Passed: 7562
scripts/test.sh --configuration Debug --project core          Passed! - Failed: 0, Passed: 660
scripts/test.sh --configuration Debug --project architecture  Passed! - Failed: 0, Passed: 115
```
The last run was taken after the last compiled edit (234e32a).

Probes with `scripts/apply-fix.sh RH7109` (LF and CRLF), compared with a temporary worktree at `origin/main` (removed afterwards). Every one has the same result on main and on the head:

| Probe | Result |
|---|---|
| Nine `#if` groups in a method body | fixed |
| Nine groups of conditional attribute lists in front of a crossed field | fixed |
| A static collection initializer with nine conditional entries | fixed |
| A type whose `#if` chooses between two headers, each with its `{` | no fix offered (`MoveRangeContainsDirectives`) |
| A body whose `#if` is closed behind the type's `}` | fixed when nothing initialized is crossed, no fix offered when something is |

### Disputed tests or scope
- **New internal type** `ConditionalGroup` in Core (one type per file, as `OrderedMember`). It has no other consumer, and no public member was added.
- **Behavior that no handed-over test pins:**
  - `WritesOnlyInsideBrackets` and `MaximumConfigurationCount`. Without the bracket rule, the attribute and collection-initializer probes above exceeded the cap. A cap fallback that withholds the fix also withheld fixes main offers.
  - The handling of a group that the body does not open or close. A first draft that withheld every fix for such a body did the same: it withheld main's fixes, for example for the two-header `#if` pattern of multi-target libraries.
  - The lead may want tests for these: the attribute-list and collection-initializer probes (fixed), and a region over the cap (no fix offered).
- **Per-name union.** SHARED-ORDER-007 says a split member "counts with what any configuration contributes", attribute by attribute. The code joins configurations by the member's name (its first declarator or its identifier) within a region. Two different members that share a name in mutually exclusive branches are therefore joined, which can only withhold more.

### Architecture findings
_None._ This change is the structural fix of decision 21. The decision of what a split declaration contributes per configuration is now read once per conditional group, with all its branches, rather than in pieces by trivia list and member span.

### Follow-up candidates
- **A conditional group inside an accessor or method block that decides the `field` keyword.** For example, `get { #if DEBUG return field; #else return 0; #endif }` gives the property a backing field only in `DEBUG`. Groups inside blocks are read as text, as on main, so that build's layout field is not counted. This is unchanged from main and reachable only for a struct or a sequential class.
- **Cleaner items left in the build output:** S3267 at `OrderingDeclarationUtilities.cs:463`, `:565`, and `:1094`, and S1168 at `:764` and `:798` (`GetConfigurations` returns `null` for "over the cap").

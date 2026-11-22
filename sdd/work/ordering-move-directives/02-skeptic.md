Role: skeptic
Commit: 4be5928
Verdict: #970: proceed narrower, with two new output questions (970-3, 970-4) next to 970-1 and 970-2; #1668: output question (1668-1, 1668-2 stand as drafted)

The prompt carried no intended fix, suspected cause, or hoped-for verdict to ignore.

Fixtures below were run with `scripts/apply-fix.sh <ID> <dir>` on 4be5928, whose `src/` equals `main`; every one was run under LF and CRLF with the same status. Fixture directories lived under the session scratchpad and are deleted.

## thoenissen/Reihitsu#970

### Premise

Real and reachable, through the RH7102, RH7103, RH7107, RH7108, RH7109 code fixes (`TypeMemberOrderingCodeFixProviderBase.IsMoveSafe` → `OrderingDeclarationUtilities.MoveRangeContainsDirectives` → `OrderingMoveSafety.RelocationChangesDirectiveScope` → `SyntaxTriviaUtilities.ContainsPositionSensitiveDirectives`; accessors through `AccessorOrderingUtilities.MoveRangeContainsDirectives`), and through the RH7309 fix (`RH7309RegionsShouldFollowCategoryOrderCodeFixProvider.ContainsUnsafeDirectives` → the same predicate).

`scripts/apply-fix.sh RH7103` on the issue's example (`Issue970.cs`, verbatim from the issue): `no-fix-offered`.

The issue's reasoning is wrong in general, as the spec author showed: `restore` returns to the project's setting, so a pair does not end its own effect behind an earlier directive of its kind. Its expected behavior ("balanced inside a crossed body does not block") is unsafe as stated, and its `#nullable` wording ("every `#nullable` change undone within the same body") is unsafe too: only `#nullable restore` returns to a known setting. The narrowing in SHARED-ORDER-011 is required, not optional.

The spec author's claim that a `#pragma` line in an excluded branch is directive trivia is confirmed by the fix's own behavior. Both fixtures for RH7103, with `#if NEVER_DEFINED` inside `Run`'s body in front of `public static void Create()`:

```csharp
public class C
{
    public void Run()
    {
#if NEVER_DEFINED
#pragma warning disable CS0168
#endif
    }

    public static void Create()
    {
    }
}
```

`ExcludedPragma.cs`: `no-fix-offered`. The same file with `// nothing` in place of the `#pragma` line (`ExcludedComment.cs`): `fixed`. The only difference is the `#pragma` line, so `ContainsPositionSensitiveDirectives` sees it as a directive.

Severity: the issue calls it a bug; it is a guard-scope narrowing (`AGENTS.md`, *Code-fix coverage*): today's output is a supported end state. Worth doing because local suppressions in member bodies are common code and the check is cheap.

### Mechanism and candidates

Not a bug, so no defect mechanism. The guard being narrowed: `ContainsPositionSensitiveDirectives` refuses every directive that is neither conditional nor region anywhere in either span, whatever its pairing or position.

Dispatch: every fix that reaches the predicate — RH7102, RH7103, RH7109 (members, `TypeMemberOrderingCodeFixProviderBase`), RH7107, RH7108 (accessors, `AccessorOrderingUtilities`), RH7309 (`ContainsUnsafeDirectives`). No other caller of `OrderingMoveSafety.MoveRangeContainsDirectives` or of `ContainsPositionSensitiveDirectives` in a code fix (`rg` over `src/`). Directive kinds (`DirectiveTriviaSyntax` subtypes other than conditional and region): `PragmaWarningDirectiveTriviaSyntax`, `NullableDirectiveTriviaSyntax` (the two SHARED-ORDER-011 admits), `LineDirectiveTriviaSyntax`, `LineSpanDirectiveTriviaSyntax`, `PragmaChecksumDirectiveTriviaSyntax`, `ErrorDirectiveTriviaSyntax`, `WarningDirectiveTriviaSyntax`, `DefineDirectiveTriviaSyntax`/`UndefDirectiveTriviaSyntax` (only legal before the first token, so not in a span), `ReferenceDirectiveTriviaSyntax`/`LoadDirectiveTriviaSyntax`/`ShebangDirectiveTriviaSyntax` (scripts or file start), `BadDirectiveTriviaSyntax`. All but the first two keep refusing.

### Breaking inputs

Failure classes that reach the change: FC-003 (both sides of the first-token boundary), FC-008 (Fix All), FC-020 (`#if` and `#pragma` in the gaps the move crosses, `#if false` blocks), FC-033 (CRLF), FC-044 (a guard must not model the pragma state; the "nothing in front" check is the cheap refusal of that class).

Existing tests that pin today's refusal of exactly the inputs SHARED-ORDER-011 now keeps — they are updated, not duplicated (`README.md`, *Coverage first*). The spec author named only the first:

- `tests/Reihitsu.Core.Test/OrderingDeclarationUtilitiesTests.cs` `MoveRangeContainsDirectivesReturnsTrueWhenCrossedMemberBodyContainsDirective` (CS0219 pair in a crossed body);
- `tests/Reihitsu.Core.Test/AccessorOrderingUtilitiesTests.cs` `MoveRangeContainsDirectivesReturnsTrueWhenCrossedAccessorBodyContainsDirective` (CS0219 pair in the crossed `set` body);
- `tests/Reihitsu.Analyzer.Test/Ordering/RH7107PropertyAccessorsMustFollowOrderAnalyzerTests.cs` `BalancedPragmaPairInTheCrossedAccessorBodyWithholdsTheFix` (CS0168 pair in the crossed `set` body; becomes a fix).

Existing tests that stay as they are and already pin a refusing neighbour: RH7102 and RH7109 `PragmaDirectivesAroundACrossedFieldWithholdTheFix` (pair in the leading trivia of two members, SHARED-ORDER-005); RH7103 `NoCodeFixWhenPragmaDirectivePrecedesMovedMember`; RH7107 `PragmaDirectiveInFrontOfTheGetAccessorWithholdsTheFix`; RH7108 `PragmaDirectiveInFrontOfTheAddAccessorWithholdsTheFix`; RH7309 (`tests/Reihitsu.Analyzer.Test/Formatting/RH7309RegionsShouldFollowCategoryOrderTriviaTests.cs`) `VerifyNoCodeFixWhenRegionContainsPragmaWarningDirective`, `VerifyNoCodeFixWhenRegionContainsNullableDirective`, `NoCodeFixWhenAPragmaDirectiveWouldBeMovedAcross`, and `VerifyCodeFixReordersRegionsFollowingAPragmaWarningDirective` (a lone directive in front with none in the spans keeps the fix; it must stay green).

Candidates the test implementer covers, none found pinned unless named:

1. Pair in a crossed body (RH7103, the issue's example) — fix; pair in the moved member's own body — fix.
2. Pair in an event accessor body (RH7108) — fix. None found.
3. `#nullable disable` + `#nullable restore` — fix; `#nullable disable` + `#nullable enable` — no fix; `#nullable disable warnings` + `#nullable restore` (target differs) — no fix.
4. Lone `disable`; `restore` with other IDs; IDs in another order; `168` against `CS0168` — no fix.
5. FC-003, the first-token boundary: a `disable` in the leading trivia of the declaration's first token, `restore` in its body — no fix (SHARED-ORDER-005); a `disable` behind the attribute list, on the modifier, `restore` in the body — fix:

   ```csharp
   public class C
   {
       public void Run()
       {
       }

       [System.Obsolete]
   #pragma warning disable CS0168
       public void Reset()
       {
           int unused;
   #pragma warning restore CS0168
       }

       public static void {|RH7103:Create|}()
       {
       }
   }
   ```

6. A pair in the members of a crossed nested type, its `disable` on the first token of the nested type's first member and its `restore` on the first token of its second — fix (a nested member is part of the nested type's declaration).
7. The Visual Studio *Suppress in Source* form, a trailing comment on both directives (`#pragma warning disable CS0168 // Variable is declared but never used`, the same on `restore`) — fix: IDs are compared as tokens, not with their trivia.
8. `disable` without IDs + `restore` without IDs — fix; `disable` without IDs + `restore CS0168` — no fix. Nested pairs (`disable A`, `disable B`, `restore B`, `restore A`) — no fix.
9. A directive of the kind in front, active and in an excluded branch (`#if NEVER_DEFINED` / `#pragma warning disable CS1591` / `#endif` at file start) — no fix.
10. Pair split across `#if` — no fix. Under 1668-1 B the RH7103 variant in SHARED-ORDER-011 is refused by SHARED-ORDER-008 as well, so it no longer tells the two requirements apart; pin it for RH7107 (see *Internal choices*).
11. `#line`, `#error`, `#warning` in a crossed body — no fix. Only `ContainsPositionSensitiveDirectives` unit tests exist (`SyntaxTriviaUtilitiesTests`); one fix-level test for `#line` suffices.
12. RH7309: pair inside a member of a moved block (the spec's example); pair inside a member of a block that keeps its place (970-4); pair inside a member in the code between blocks.
13. FC-008: a Fix All scenario of RH7103 with two reported static members and a pair in a crossed body. Every move's guard is read on the original document; with every directive of the kind in pairs inside declarations and none in front, any permutation keeps each declaration at the project's setting, so the merged result is safe, but nothing pins it.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `Reihitsu.Core` (`OrderingMoveSafety`, `SyntaxTriviaUtilities`) | Must change |
| RH7309 code fix (`ContainsUnsafeDirectives`) | Must change if 970-2 is A; see *Architecture findings* |
| RH7102, RH7103, RH7107, RH7108, RH7109 code-fix providers | Must stay; they reach the change through Core |
| Fix All | Must stay (batch fixer); one scenario to cover (candidate 13) |
| Analyzers | Not involved: every diagnostic stays |
| Formatter | Not involved: the moves are text moves (SHARED-ORDER-009) |
| Rule pages | Not involved: no RH71xx or RH7309 page mentions `#pragma`; `general-notes.md` covers withheld fixes |
| Tests | Must change: the three flipped tests above; new tests per candidates |

### Refuse option

Refusing is `main`'s behavior: no production lines. The requested change narrows a guard for common code (members that suppress a warning locally) by four cheap syntactic checks (position, pairing, branch, nothing in front), without a parse and without a state model, so it is worth doing (`AGENTS.md`, *Code-fix coverage*, last bullet before *Special cases*).

Declaring a shape unsupported is not an option: the ordering fixes move text and do not format (SHARED-ORDER-009), so no formatter special case would be deleted, and the shape — a `#pragma warning` or `#nullable` directive — is code people commonly write. A shape that deletes nothing is not declared.

Simpler alternative inside the change: none that keeps the issue's gain; 970-3 and 970-4 below are cheap ways to keep more of it.

### Output questions

970-1 and 970-2: labels kept, content checked. I agree with both recommendations. One clarification for 970-1: option B changes only `#pragma warning`; for `#nullable` every directive in front refuses under both options, because `#nullable` names no IDs. 970-1 and 970-3 are independent and combine.

**970-3 · RH7103 · Static elements must appear before instance elements — a pair in a crossed body, and another pair in a member in front of the move**

Same for RH7102, RH7107, RH7108, RH7109, RH7309. For RH7107 and RH7108 it is the common case: every pair in any method written above the property in the file counts as in front.

Input:
```csharp
public class C
{
    public static void Log()
    {
#pragma warning disable CS0168
        int unused;
#pragma warning restore CS0168
    }

    public void Run()
    {
    }

    public void Reset()
    {
#pragma warning disable CS0168
        int unused;
#pragma warning restore CS0168
    }

    public static void Create()
    {
    }
}
```

Before (`main`): no fix offered (`PairInFrontAndInSpan.cs`).

After, A: no fix offered. Any `#pragma warning` in front of the moved code withholds the fix, also a closed pair (the spec as written).

After, B (recommended):
```csharp
public class C
{
    public static void Log()
    {
#pragma warning disable CS0168
        int unused;
#pragma warning restore CS0168
    }

    public static void Create()
    {
    }

    public void Run()
    {
    }

    public void Reset()
    {
#pragma warning disable CS0168
        int unused;
#pragma warning restore CS0168
    }
}
```

Why B: a closed pair in front leaves the project's setting behind it, so it is as safe as no directive; the check is the same pairing check over the text in front, and without it a file loses the fix as soon as it suppresses a warning in two places.

The B output is `main`'s output for the same input with the pair in `Reset` removed (`PairInFrontOnly.cs`: `fixed`, `Create` lands between `Log` and `Run`), plus the pair lines. Under B, a directive in front that is not part of such a pair — a file-level `#pragma warning disable CS1591` without `restore`, a `#nullable enable` at the top — still withholds the fix (970-1 decides only the `#pragma warning` variant with other IDs).

**970-4 · RH7309 · Regions should follow a fixed category order — a pair inside a member of a block that keeps its place**

Input:
```csharp
using System;

public class C : IDisposable, IComparable
{
    #region IDisposable

    public void Dispose()
    {
    }

    #endregion // IDisposable

    #region Fields

    private int _a;

    #endregion // Fields

    #region IComparable

    public int CompareTo(object obj)
    {
#pragma warning disable CS0168
        int unused;
#pragma warning restore CS0168
        return 0;
    }

    #endregion // IComparable
}
```

Before (`main`): no fix offered (`PairInKeptBlock.cs`).

After, A: no fix offered. SHARED-ORDER-011 as written keeps a pair only in a block whose place changes, or in code between blocks that the reorder crosses; `IComparable` keeps its place.

After, B (recommended):
```csharp
using System;

public class C : IDisposable, IComparable
{
    #region Fields

    private int _a;

    #endregion // Fields

    #region IDisposable

    public void Dispose()
    {
    }

    #endregion // IDisposable

    #region IComparable

    public int CompareTo(object obj)
    {
#pragma warning disable CS0168
        int unused;
#pragma warning restore CS0168
        return 0;
    }

    #endregion // IComparable
}
```

Why B: RH7309-005 already inspects every block and the code between every two blocks, so applying SHARED-ORDER-011 to the same spans, with "in front" meaning in front of the first block, needs no new notion of which blocks move, and it is safe for the same reason as a moved block.

The B output is `main`'s output for the input without the two pragma lines (`NoPairKeptBlock.cs`: `fixed`, the blocks become `Fields`, `IDisposable`, `IComparable`), plus the pair lines. Under A, a pair in the first block is refused as well when that block keeps its place, because it stands in front of the first block that moves.

### Internal choices

- **SHARED-ORDER-011's RH7309 wording.** "In front of the first region block or code between blocks whose place the reorder changes": code between blocks never changes place (RH7309-004 keeps it in place). Under 970-4 B it becomes "in front of the first region block"; under A it needs rewording anyway. The spec's intro paragraph still says only SHARED-ORDER-007 and SHARED-ORDER-008 apply to RH7309; SHARED-ORDER-011 now does too. Spec author's.
- **Split-pair example.** SHARED-ORDER-011's negative example for a pair split across `#if` uses RH7103. If 1668-1 is B, SHARED-ORDER-008 refuses that input for its own reason, so a test on it cannot tell whether SHARED-ORDER-011's *One branch* clause works. Recommend: state the split-pair example for RH7107 (SHARED-ORDER-008 never applies to accessors), keep the RH7103 one only if 1668-1 is A. Under 1668-1 B, *One branch* is live only for RH7107 and RH7108.
- **One branch against "no conditional directive between the pair's directives".** The simpler clause refuses a pair around an `#if` group, which `main` refuses too, so nothing visible is in question relative to `main`; it removes one example (the RH7107 `#if DEBUG` inside a pair). The difference in code is a few lines of `GetRelatedDirectives`. Recommend keeping the spec as written; the lead may take the simpler clause without asking.
- **Where the prefix check reads.** `MoveRangeContainsDirectives` is called with the type declaration or the accessor list as its root; "nothing in front" reads the whole document (the syntax tree's root). The Core unit tests call it with a single-type tree, so they see the whole document either way.

### Issue goals

- *Expected Behavior* (balanced pairs, `#nullable` "undone") — narrowed to SHARED-ORDER-011. Changes visible output relative to the issue's request; covered by the change doc's *Visible output changes* and 970-1/970-3.
- *Diagnostic ID* list omits RH7309 — 970-2.
- "The Core test is replaced by tests for the balanced and the unbalanced case" — keep, and extend to the two other tests that pin the same refusal (*Breaking inputs*). Not visible.
- "Spec: the open-question non-goal is removed" — stale: `main`'s `ordering-move-safety.md` has no such non-goal. Drop. Not visible.
- "Squad lane: bug fix" — superseded by the triage comment; the run is behavior-change. Not visible.

### Architecture findings

The decision "which non-conditional, non-region directive may lie in a span that a reorder relocates" is stated twice: `OrderingMoveSafety.RelocationChangesDirectiveScope` (member and accessor moves) and `RH7309RegionsShouldFollowCategoryOrderCodeFixProvider.ContainsUnsafeDirectives`, each composing `ContainsPositionSensitiveDirectives` with its own conditional check. SHARED-ORDER-011 changes that decision for both (970-2 A), with a different "in front" anchor for RH7309. Applying the narrowing in each place would state the pairing rule twice. Shown by: the two call sites (`src/Reihitsu.Core/OrderingMoveSafety.cs:280-285`, `src/Reihitsu.Analyzer.CodeFixes/Rules/Organization/RH7309RegionsShouldFollowCategoryOrderCodeFixProvider.cs:263-267`). Within the components the run changes; the shape of the shared predicate is the implementer's.

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1668

### Premise

Reachable: the member-moving fixes offer moves today across and with conditional groups of whole units. `scripts/apply-fix.sh RH7103` on 1668-1's input (`Q1668_1.cs`, verbatim): `fixed`, `Create` moved in front of `Run`. `scripts/apply-fix.sh RH7109` on 1668-2's input (`Q1668_2.cs`) and on its exception (`Q1668_2_Exception.cs`): both `fixed`, `_b` in front of `_a`.

The issue is a decision, not a defect. The maintainer's triage comment sets the criterion; the spec author's finding that the check is not trivial holds against the code:

- `ConditionalGroupUnitUtilities.WrapsWholeUnits` intersects the unit lists of every directive of a group over four list kinds (`GetBoundaryLists`, a switch over seven syntax shapes plus an attribute-list case), parses every excluded branch with a per-list prefix and suffix (`ReadsAsWholeUnits`), and adds a `field`-keyword clause and a bodiless-accessor clause;
- `OrderingDeclarationUtilities.GetMembersOfDisabledText` parses excluded text a second time to count members for SHARED-ORDER-007;
- `DeclarationHoldsConditionalDirective` and `DeclaresTypeWithAttributes` read excluded branches in front of the type for partial declarations with attributes.

This is the FC-044 shape exactly: the guard models which code an `#if` group excludes; "refusing the class by a cheap check on its cause ends it". That supports the recommendation B.

Spec conflict to state (role question 2): SHARED-ORDER-006's *Why* argues against B for member moves ("Refusing it would give away safe ground — for example every member whose body contains an `#if DEBUG` block"). That safe ground exists only through SHARED-ORDER-008's parse: a body group's excluded branch can close the member and declare fields (SHARED-ORDER-008's own *Why*), so a cheaper version of the same ground does not exist. The spec author's change doc already retires the example; the `Why` sentence must go with it if B is chosen.

Severity: B withholds fixes for code people commonly write (`#if DEBUG` in a method body), so it is escalation 9 in output-question form, which is how the spec author drafted it. The maintainer's triage comment pre-decides it conditionally; the question confirms the condition is met.

### Mechanism and candidates

_N/A_ as a bug. Candidates for the refusal under B, from the dispatch (`ConditionalGroupUnitUtilities.GetBoundaryLists` and `ChangesMemberOrder`):

- directive kinds: `#if`, `#elif`, `#else`, `#endif`; active branch, excluded branch, fully inactive `#if false` (FC-020);
- unit lists: type-body members, a member's attribute lists, accessor lists, statements of a block or switch section (with lambdas and local functions), members of a nested type;
- place: moved span, crossed span, body in front of the spans (layout clause only), declaration header, leading trivia of the declaration's first token (whole-type wrap with or without `#else`, or a group wholly in front whose `#endif` lands there), another partial declaration, behind the spans;
- fixes: RH7102, RH7103, RH7109, RH7309; RH7107 and RH7108 not affected.

### Breaking inputs

Classes: FC-020, FC-044, FC-003 (a move that reorders no instance field next to one that does, under 1668-2), FC-008 (Fix All with a group in one move's span and not in the other's).

Existing coverage that B rewrites: about 149 tests tagged SHARED-ORDER-008 — `RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsExcludedCodeTests` (59), `RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsTypeDeclarationGroupTests` (46), `RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzerTests` (21), `RH7309RegionsShouldFollowCategoryOrderInitializationOrderTests` (20), `RH7102ConstantsMustAppearBeforeFieldsAnalyzerTests` (3), and 48 tagged SHARED-ORDER-007 in the same five files (some carry both tags). Every test that asserts a fix for a group in the spans flips to no fix; every no-fix test stays; tests that only tell kept groups apart (the `field` keyword, bodiless accessors, excluded-member initializers, excluded partial declarations with attributes) are deleted per the issue's *Done when*. Tests that must stay green: `AccessorOrderingUtilitiesTests.MoveRangeContainsDirectivesReturnsFalseWhenCrossedAccessorBodyContainsBalancedConditional` and the RH7107/RH7108 conditional tests (accessor moves unchanged); RH2001-010's tests.

Candidates after the answer, under B:

1. One group per unit list in the crossed span and in the moved span (members, attribute lists on the moved member, accessors, statements, nested-type members) — no fix, RH7103.
2. Group behind the spans — fix kept.
3. RH7309: group in a block whose place changes — no fix; group in a block that keeps its place outside the reordered span — fix kept.
4. 1668-2 A: group in the body in front — no fix for an instance-field reorder (RH7109), fix kept for a move that reorders no instance field (RH7103 moving a static method across the same class); whole-class `#if` without `#else` — fix; with `#else` — no fix; a group wholly in front of the class whose `#endif` stands on its first token (`#if DEBUG` / `class A { }` / `#endif` / `public class C`) — no fix.
5. `#if false` around whole members in the crossed span — no fix.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `Reihitsu.Core` `OrderingDeclarationUtilities`, `ConditionalGroupUnitUtilities` | Must change (shrink) under B; stay under A |
| RH7309 code fix | Must change under B (its conditional-pair clause, RH7309-005) |
| `TypeMemberOrderingCodeFixProviderBase` | Must stay unless the refusal moves into `MoveRangeContainsDirectives` (internal) |
| RH2001 code fix (`ConditionalGroupsWrapWholeMembers`, RH2001-010) | Must stay; out of the issue's scope (follow-up) |
| RH7107, RH7108 | Must stay; SHARED-ORDER-008 never applied |
| Analyzers, formatter, rule pages | Not involved |
| Tests | Must change under B: the tests tagged SHARED-ORDER-007 or SHARED-ORDER-008 above are reviewed |

### Refuse option

B is the refuse option: every conditional group in the spans withholds the fix. It deletes code; its cost is fixes for common code, which is why it is the maintainer's to confirm. A (keep) costs the parse machinery above, which the criterion rules out. C keeps the four-list place test and the build dependence (the spec author's alternatives say so) and gains only groups whose code is all active.

Under B, the parse is not deleted from the repository: RH2001-010 keeps `ConditionalGroupsWrapWholeMembers`, which keeps `WrapsWholeUnitsOf`, `GetExcludedText`, `ParseInTypeBody`, and the type-body arm of `ReadsAsWholeUnits`. What B deletes is `WrapsWholeUnits`, `GetBoundaryLists`, `IsUnitListOfTypeBody`, `ForbidsFieldKeyword`, the other arms of `ReadsAsWholeUnits`, `GetMembersOfDisabledText`, `DeclaresTypeWithAttributes`, and the group branches of `ChangesMemberOrder` and `DeclarationHoldsConditionalDirective`. The issue's *Done when* item is met only for the member-moving fixes.

Declaring a shape unsupported is not an option: the shape that needs the parse (a group whose excluded branch is not whole units) is not a cheap check, and its cheap superset (any conditional group in a type body) refuses formatting for common code while deleting no formatter special case — the ordering fixes do not format.

### Output questions

1668-1 and 1668-2: labels kept, content checked against the code and the fixtures; I agree with both recommendations. One neighbour for 1668-2 A worth showing in its text, because the rule "in front of the class declaration" reaches it without the reader expecting it: a group around a preceding type in the same file, whose `#endif` stands on the class's first token:

```csharp
#if DEBUG
public class Trace
{
}
#endif
public class C
{
    private int _a;
    private readonly int _b;
}
```

`main` offers RH7109's fix here (`GroupBeforeClass.cs`, `scripts/apply-fix.sh RH7109`: `fixed`, `_b` in front of `_a`; the branch holds a whole type of another name); under 1668-2 A it is withheld, as the change doc's last paragraph of section 2 says. It does not need its own label.

### Internal choices

- Where B's refusal lives — in `MoveRangeContainsDirectives` (then `OrderingDeclarationUtilitiesTests.MoveRangeContainsDirectivesReturnsFalseWhen...BalancedConditional` flip) or in `ChangesMemberOrder`, as SHARED-ORDER-008 is today (they stay). No visible difference; recommend `ChangesMemberOrder`, where RH7309 already reaches it through `RegionBlockLayout`, so one change serves all four fixes.
- SHARED-ORDER-006's *Why* sentence about `#if DEBUG` bodies goes with its example under B.

### Issue goals

- *Done when*: "the parse … removed" — only from the member-moving fixes; RH2001-010 keeps its part (*Refuse option*). Not visible.
- *Files or areas* names `sdd/specs/shared/code-fixes.md` for SHARED-ORDER-007/008 — stale; they are in `sdd/specs/shared/ordering-move-safety.md`. Not visible.
- "Lane: bug-fix" — the run is behavior-change. Not visible.
- The body's "the maintainer chose to keep the parse for now" is superseded by the triage comment. Not visible.

### Architecture findings

Under B, the decision "which conditional group around members is harmless" stays stated in RH2001-010 (`ConditionalGroupsWrapWholeMembers`) and is dropped from SHARED-ORDER-008, so the two would decide differently for the same group of whole members. Not a defect of the run; it is the spec author's follow-up candidate below.

### Follow-up candidates

- The spec author's RH2001-010 candidate stands, conditional on 1668-1 B; it is also what would let the excluded-text parse leave the repository.

## Whole run

- No build or test run; facts were settled with `scripts/apply-fix.sh` (RH7103, RH7107, RH7109, RH7309) on 4be5928. One more fixture, `PairInFront.cs` for RH7107 (a CS0168 pair in a method above the property, no directive in the accessor list): `fixed` on `main`; SHARED-ORDER-011 does not touch it, since no directive lies in the spans.
- Output questions open for the batch: 970-1, 970-2 (spec author), 970-3, 970-4 (new), 1668-1, 1668-2 (spec author). 970-3 and 970-4 change SHARED-ORDER-011 only; neither blocks the test implementer from the spec's existing examples, but the RH7309 and "in front" examples depend on them.

Role: spec author
Commit: e12c463
Verdict: open question

The run has two issues. #970 has its spec written on the recommendations below; its two output questions do not block the test implementer, who can start from SHARED-ORDER-011's examples, but an answer other than the recommendation changes one example each. #1668 cannot be specified before the maintainer answers: its change doc is a Draft with the visible changes, and the spec diff waits.

The prompt carried no intended fix, cause, or verdict to ignore.

# #970 — balanced `#pragma` pair inside a crossed member's body

Verdict for this issue: spec written; change doc written (`sdd/changes/2026-11/ordering-move-warning-nullable-pairs.md`, Status Draft).

## Finding on the premise

The issue repeats decision Q1 of #927: a pair restored within the same body "changes nothing the moved member is covered by". That is false where a directive of the pair's kind is in effect in front of it. `#pragma warning restore` and `#nullable restore` return to the project's setting, not to the setting in front of the pair. Checked with Roslyn 5.12.0 (the repository's version):

```csharp
#pragma warning disable CS0168
class C
{
    void Before() { int a; }   // no CS0168
    void Reset()
    {
#pragma warning disable CS0168
        int b;
#pragma warning restore CS0168
    }
    void After() { int c; }    // CS0168 (9,22): the restore ended the first directive's effect
}
```

A member moved from behind `Reset` to in front of it falls under the first directive again. That is the change of governed code that SHARED-FIX-003 (*Comments and directives in place*) forbids. So the spec keeps the move only when no directive of the pair's kind stands in front of the spans (SHARED-ORDER-011, *Nothing in front*). Offering the move without that condition is not an option, because it violates SHARED-FIX-003. How wide the condition reaches is question 970-1.

A `#pragma` or `#nullable` line in an excluded `#if` branch is directive trivia in Roslyn, not disabled text (checked). The "in front" and "pairs" checks therefore cover other builds by reading the tree, without evaluating a condition.

The fix moves text only (SHARED-ORDER-009) and formats nothing, so no formatter output and no other fix changes. No rule page changes: the change narrows a withheld fix, which `general-notes.md` already covers. The case is reachable through the code fix: RH7103 reports `Create` in the issue's example, and `scripts/apply-fix.sh` shows `no-fix-offered` on `main`.

## Changed requirements

- SHARED-ORDER-011 — A warning or nullable pair that ends its own effect inside one declaration travels intact — added
- SHARED-ORDER-006 — Balanced directive pairs travel intact — changed: a non-conditional, non-region directive in either span refuses unless SHARED-ORDER-011 keeps it
- RH7102-006, RH7103-007, RH7107-005, RH7108-005, RH7109-006 — clarified: SHARED-ORDER-011 named next to SHARED-ORDER-006; `Uses` rows extended
- RH7309-005 — changed: SHARED-ORDER-011 named, a pair example added; `Uses` row extended
- `ordering-move-safety.md` `Uses` gains RH7102-006, which SHARED-ORDER-011 points to

## Examples for the tests

- SHARED-ORDER-011: all five examples. The pair in a crossed body gets the fix (RH7103). A lone directive, a directive of the kind in front (also in an excluded branch), and a pair split across `#if` get no fix (RH7103). A pair around a whole `#if` group gets the fix (RH7107). The text's variants: `#nullable disable`/`#nullable restore` keeps the move, `#nullable disable`/`#nullable enable` refuses it, a `restore` with other IDs refuses it.
- RH7309-005: the new `#pragma warning` pair example (fix offered).
- RH7102-006's existing example, whose pair stands in the leading trivia of two members, stays refused. It is a regression guard for SHARED-ORDER-005.
- The Core test `MoveRangeContainsDirectivesReturnsTrueWhenCrossedMemberBodyContainsDirective` (`tests/Reihitsu.Core.Test/OrderingDeclarationUtilitiesTests.cs`) pins today's refusal of exactly this input. It is replaced as the issue says.
- `scripts/test.sh --project architecture` fails only `EveryRequirementOfAnEnforcedSpecIsCovered` for SHARED-ORDER-011, until the test implementer tags its tests.

Every example input was run through `scripts/apply-fix.sh` on `main`. Each one reported and compiled. The "after" blocks of the positive examples match `main`'s output for the same input without the pragma lines.

## Output questions

**970-1 · RH7103 · Static elements must appear before instance elements — a pair in a crossed body, with a file-level `#pragma warning` for another warning**

Same for RH7102, RH7107, RH7108, RH7109, RH7309.

Input:
```csharp
#pragma warning disable CS1591
public class C
{
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

Before (`main`): no fix offered.

After, A (recommended): no fix offered. Any `#pragma warning` in front of the moved code withholds the fix, whatever it names.

After, B:
```csharp
#pragma warning disable CS1591
public class C
{
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

With `#pragma warning disable CS0168` as the first line instead, both A and B offer no fix. The move would put `Create` under that line, which it is not under today.

Why A: B has to compare warning IDs written as `168` or `CS0168`, and a directive with no IDs covers every warning. That is a normalization the one-kind check avoids, and it gains the fix only in files that also carry an unrelated file-level `#pragma warning`.

**970-2 · RH7309 · Regions should follow category order — a `#pragma warning` pair inside a member of a block**

Input:
```csharp
using System;

public class C : IDisposable
{
    #region IDisposable

    public void Dispose()
    {
#pragma warning disable CS0168
        int unused;
#pragma warning restore CS0168
    }

    #endregion // IDisposable

    #region Fields

    private int _a;

    #endregion // Fields
}
```

Before (`main`): no fix offered.

After, A (recommended):
```csharp
using System;

public class C : IDisposable
{
    #region Fields

    private int _a;

    #endregion // Fields

    #region IDisposable

    public void Dispose()
    {
#pragma warning disable CS0168
        int unused;
#pragma warning restore CS0168
    }

    #endregion // IDisposable
}
```

After, B: no fix offered (RH7309 stays as on `main`; only RH7102, RH7103, RH7107, RH7108, RH7109 change, as #970 and #927 Q1 list them).

Why A: RH7309 uses the same directive check as the member moves, and a block can hold the same pairs. Leaving it out would make RH7309 refuse a block that RH7103 moves a member across.

## Internal choices

- A new requirement, SHARED-ORDER-011, next to SHARED-ORDER-006, rather than widening SHARED-ORDER-006 itself. The issue asks for this, and the conditions are one decision with their own Why. Alternative: fold the conditions into SHARED-ORDER-006.
- The boundary is stated by its cause: a pair counts when it lies in the leading trivia of tokens of one declaration of the reordered list, behind its first token. A nested type's members belong to the nested type's declaration. Alternative: "inside the body", which misses accessor bodies, initializers, and nested types.
- The pair is consecutive among the declaration's directives of its kind, so nested pairs (`disable A`, `disable B`, `restore B`, `restore A`) refuse. Alternative: a stack-based match, which is more code for a rare shape.
- IDs are compared as written and in the same order. Alternative: a normalized set comparison (see 970-1).
- `#nullable` is closed only by `#nullable restore` with the same target. This is required, not chosen: `#nullable enable` closed by `#nullable disable` ends at `disable`, not at the project's setting.
- *One branch* and counting directives in excluded branches are required by SHARED-FIX-003 (*Build configurations*), not chosen. A `restore` that only some builds compile leaves `disable` in effect in the others.

## Follow-up candidates

_None._

# #1668 — refuse any conditional group in the moved span, and drop the excluded-text parse

Verdict for this issue: open question. The change doc is written as a Draft (`sdd/changes/2026-11/ordering-move-refuses-conditional-groups.md`). The spec diff to SHARED-ORDER-006/007/008 and RH7102-006, RH7103-007, RH7109-006, and RH7309-005 is written after the answer.

## Finding against the maintainer's criterion

The triage comment says: if checking precisely which conditional blocks matter is a non-trivial check, the fixes refuse any conditional block in the moved span instead. On `main` the check is non-trivial. For each group in the spans, `OrderingDeclarationUtilities` and `ConditionalGroupUnitUtilities` decide five things:

1. Which of four kinds of unit list each directive stands at the boundary of: type members, attribute lists, accessors, or statements in a body. Lambdas and local functions are included; field initializers and expression bodies are excluded.
2. Whether another group nests inside or around it.
3. Whether each excluded branch, parsed on its own as that list's content, reads as whole units with no syntax error.
4. For accessors, whether a branch uses `field` or adds an accessor without a body.
5. Which members each excluded branch in the type body declares, from a second parse (`GetMembersOfDisabledText`), so SHARED-ORDER-007 can count initializers and layout fields.

The type-layout clause reads excluded branches in front of the spans and around the type the same way. This is the parser-like reading of excluded text that *Special cases do not grow the source* names as machinery. By the criterion, the simpler rule follows. It withholds fixes that `main` offers for common code (`#if DEBUG` inside a method body), so the outputs below are the maintainer's to confirm.

Everything was checked with `scripts/apply-fix.sh` on `main`. RH7103 fixes all four inputs below, and RH7109 fixes both layout inputs.

Out of scope and unchanged: RH7107 and RH7108, which SHARED-ORDER-008 never governed, and RH2001's reading of excluded members (RH2001-010). If B is chosen, RH2001-010 keeps the only remaining parse (see Follow-up candidates).

## Changed requirements

None yet. Planned under the recommendation, as listed in the change doc:

- SHARED-ORDER-008: changed. Every conditional directive in either span refuses the move, and the whole-unit test is retired.
- SHARED-ORDER-007: changed. Members of excluded code are no longer counted.
- SHARED-ORDER-006: its `#if DEBUG` body example no longer applies to member moves.
- RH7102-006, RH7103-007, RH7109-006, RH7309-005: changed.

## Examples for the tests

After the answer.

## Output questions

**1668-1 · RH7103 · Static elements must appear before instance elements — an `#if` block inside the code the move crosses**

Same for RH7102, RH7109, RH7309.

Input:
```csharp
public class C
{
    public void Run()
    {
#if DEBUG
        System.Console.WriteLine();
#endif
    }

    public static void Create()
    {
    }
}
```

Before (`main`):
```csharp
public class C
{
    public static void Create()
    {
    }

    public void Run()
    {
#if DEBUG
        System.Console.WriteLine();
#endif
    }
}
```

After, A: as `main`. The fix keeps reading excluded `#if` code to decide whether it holds whole members, attribute lists, accessors, or statements.

After, B (recommended): no fix offered. The same holds wherever an `#if` block lies in the code the fix moves or crosses, where `main` offers the fix:
- `#if DEBUG` around a whole method in front of `Create`;
- `#if DEBUG` around `[System.Obsolete]` on `Create`;
- `#if DEBUG` around an accessor, or around a member of a nested class that the move crosses.

After, C: no fix offered when `DEBUG` is not defined; the `main` result when `DEBUG` is defined. An `#if` block whose code is all active keeps the fix. One with excluded code, or with an `#else`, withholds it.

Why B: deciding which blocks are harmless needs excluded code parsed as members, attributes, accessors, or statements. By the maintainer's criterion, that check is not trivial.

**1668-2 · RH7109 · Readonly elements must appear before non-readonly elements — an `#if` block in front of the moved fields** (only if 1668-1 is B)

Same for RH7309.

Input:
```csharp
public class C
{
#if DEBUG
    private int _trace;
#endif
    public void Run()
    {
    }

    private int _a;
    private readonly int _b;
}
```

Before (`main`):
```csharp
public class C
{
#if DEBUG
    private int _trace;
#endif
    public void Run()
    {
    }

    private readonly int _b;
    private int _a;
}
```

After, A (recommended): no fix offered. Any `#if` block in the class in front of the moved fields, or in front of the class declaration, withholds a fix that reorders the class's instance fields. One exception keeps `main`'s result: an `#if` with no `#else` that wraps the whole class, as in
```csharp
namespace N
{
#if !DEBUG
    public class C
    {
        private int _a;
        private readonly int _b;
    }
#endif
}
```
which the fix still turns into `_b` in front of `_a`. Moves that reorder no instance field (methods, static members, constants) are not affected.

After, B: as `main`. The fix keeps reading the excluded code in front of the moved fields.

Why A: B keeps the excluded-code parse alive for this one place. The exception is a check of where the two directives stand, and it keeps the common shape of a whole class compiled for some targets only.

## Internal choices

- One change doc for #1668, separate from #970's. The two share the guard type but not a cause: #970 narrows a refusal, #1668 widens one.
- The exception in 1668-2 A is part of the recommended option, not a separate question. It only keeps `main`'s output for its shape.

## Follow-up candidates

- If 1668-1 is answered B: RH2001's fix (RH2001-010, `OrderingDeclarationUtilities.ConditionalGroupsWrapWholeMembers`) still parses excluded branches as whole members around a property it rewrites. The same criterion would refuse any conditional group there. That is an output question outside this run's issues and would be filed `agent-filed`. Input: a private auto-property with `#if DEBUG` around a whole member directly in front of it. Before: RH2001's fix offered. After, under the criterion: no fix offered.

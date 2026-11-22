# A warning or nullable pair inside one declaration no longer blocks an ordering move

| | |
|---|---|
| Status | Draft |
| Date | 2026-11 |
| Affected requirements | SHARED-ORDER-011 (added), SHARED-ORDER-006 (changed: a directive that is neither a conditional nor a region directive refuses the move unless SHARED-ORDER-011 keeps it), RH7102-006, RH7103-007, RH7107-005, RH7108-005, RH7109-006, RH7309-005 (changed: SHARED-ORDER-011 named) |

## What

The ordering fixes that move a declaration in front of an earlier sibling (RH7102, RH7103, RH7107, RH7108, RH7109) and the RH7309 fix that reorders region blocks no longer refuse a move for a `#pragma warning` or `#nullable` pair that opens and closes inside one declaration of the moved or the crossed span (SHARED-ORDER-011). A pair is a `#pragma warning disable` followed by a `#pragma warning restore` for the same IDs, or a `#nullable enable` or `#nullable disable` followed by a `#nullable restore` for the same target. It stays allowed only when no conditional branch splits it and no directive of its kind stands anywhere in the document in front of the moved and the crossed span.

Every other directive that is neither a conditional nor a region directive still refuses the move: one in a declaration's leading trivia (SHARED-ORDER-005, unchanged), a lone or unpaired one, and every `#line`, `#error`, `#warning`, or other kind.

This change covers thoenissen/Reihitsu#970 and carries out the maintainer's decision Q1 on thoenissen/Reihitsu#927, narrowed where that decision's premise does not hold (*Decisions*).

## Why

SHARED-ORDER-006 refuses a move when either span holds any directive other than a conditional or a region directive, also one that sits inside a crossed member's body. A balanced `#if` pair in the same place is allowed, because it moves or stays with the code it governs. A `#pragma warning disable` restored inside the same body governs only that body's code as well, as long as its `restore` returns to the setting in effect in front of the pair. Refusing it withholds the fix from every member whose body locally suppresses a warning, which is common code.

`restore` does not return to the setting in front of the pair, though: `#pragma warning restore` and `#nullable restore` return to the project's setting. Checked with the compiler:

```csharp
#pragma warning disable CS0168
class C
{
    void Before() { int a; }   // no CS0168: the first directive governs it

    void Reset()
    {
#pragma warning disable CS0168
        int b;
#pragma warning restore CS0168
    }

    void After() { int c; }    // CS0168: the restore ended the first directive's effect
}
```

A member moved from behind `Reset` to in front of it would fall under the first directive again, so the move changes which code a directive governs (SHARED-FIX-003, *Comments and directives in place*). A pair therefore keeps a move only where no directive of its kind is in effect in front of the spans. The cheap way to know that is to find no directive of the kind in front of them at all.

## Visible output changes

### The fix is offered across a pair in a member's body

RH7102, RH7103, RH7107, RH7108, RH7109, and RH7309 alike. Input, for RH7103:

```csharp
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

Before: no code fix is offered.

After:

```csharp
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

The same holds for a pair in the moved declaration's own body, for a `#nullable disable` or `#nullable enable` closed by `#nullable restore`, and, for RH7107 and RH7108, for a pair in an accessor's body.

### The fix stays withheld where a pair does not end its own effect

Unchanged from `main`, listed because the issue asks for the fix in some of these cases:

- a directive of the pair's kind anywhere in the document in front of the spans, also one in an excluded branch and also one with other IDs, such as a file-level `#pragma warning disable CS1591` (the second example of SHARED-ORDER-011);
- a pair whose `restore` stands in a conditional branch that the `disable` is not in (the fourth example of SHARED-ORDER-011);
- a `#nullable disable` closed by `#nullable enable`, or the reverse;
- a pair whose two directives name the same IDs written differently, such as `168` and `CS0168`, or in another order.

### Code fixes that format the affected node

None besides the fixes named above. The move is a text move of whole lines (SHARED-ORDER-009) and does not format the moved node; no formatter output changes.

## Decisions

### A pair counts only with no directive of its kind in front of the spans

The decision on thoenissen/Reihitsu#927 says a pair restored within the same body "changes nothing the moved member is covered by". That holds only when nothing of the pair's kind is in effect in front of it, because `restore` returns to the project's setting (*Why*). Offering the move without that condition would violate SHARED-FIX-003, so it is not an option; how wide the condition reaches is visible output and is put to the maintainer: any directive of the kind refuses (recommended, written into the spec), or only one that names an ID of the pair.

### `#nullable` is closed only by `#nullable restore`

A `#nullable enable` closed by `#nullable disable` ends at `disable`, not at the project's setting, so it is not a pair that ends its own effect. Only `restore`, with the same target, closes one.

### The pair lies behind the declaration's first token, read from the syntax tree

The boundary is the cause: a directive in the leading trivia of a declaration's first token stands in front of the declaration and is SHARED-ORDER-005's; one in the leading trivia of any later token of the declaration lies inside it and moves or stays with it. That covers a method body, an accessor body, an initializer, and the members of a nested type alike, without a list of positions.

### A directive in an excluded branch counts

The compiler reads a `#pragma` or `#nullable` line in an excluded branch as a directive, so the syntax tree holds it like an active one. Counting it in the pairs and in front of the spans covers the builds that compile it without evaluating a condition (SHARED-FIX-009).

### RH7309 is included

The RH7309 fix uses the same directive predicate as the member moves, and its blocks hold members whose bodies can hold the same pairs. Leaving it out would need a second predicate and would make RH7309 refuse a block that RH7103 moves across. Whether RH7309 is included is visible output and is put to the maintainer; the spec is written including it.

## Alternatives considered

- **Allow every balanced pair, as the issue states it** — rejected: a pair behind an earlier directive of its kind ends that directive's effect for the code behind it, so the move changes which code a directive governs (SHARED-FIX-003).
- **Compare the IDs of the directives in front of the spans with the pair's** — kept as an option for the maintainer, not recommended: IDs can be spelled as numbers or with a prefix, and a directive without IDs covers every warning, so the comparison needs a normalization the one-kind check does not; it gains the fix only in files that also hold an unrelated file-level `#pragma warning`.
- **Track the setting in effect at the spans from every directive in front of them** — rejected: it models the compiler's pragma state, which *Special cases do not grow the source* rules out for a guard.
- **Keep refusing every such directive** — rejected: it withholds the fix from members that suppress a warning locally, which is common code, and the maintainer's decision on thoenissen/Reihitsu#927 asks for the narrower guard.

# The argument list of a conditional element access is joined and reported like an element access's

| | |
|---|---|
| Status | Accepted |
| Date | 2026-11 |
| Affected requirements | FMT-BREAK-014 (changed: names the bracketed argument list of a conditional element access's element binding among the lists it joins, and divides the gaps with FMT-BREAK-040), RH5307-001 (changed: the list of a conditional element access is reported; the exclusion and its example are removed), RH5307-003 (changed: where the diagnostic of that list is), RH5307-004 (changed: the fix joins that list; its explanation of the `c?[0][…]` example no longer relies on the exclusion) |
| Depends on | [An element access or a null-forgiving operator written below its element joins the element's line](attached-part-below-element.md) |

## What

A bracketed argument list (`BracketedArgumentListSyntax`) has one of three parents that hold an index: an element access (`m[1, 2]`), the element binding of a conditional element access (the `[1, 2]` of `c?[1, 2]`), and an implicit element access in an initializer (`[1, 2] = 3`). The formatter joins a wrapped list of the first and the third onto one line (FMT-BREAK-014), and RH5307 reports and fixes those two; the list of an element binding is left as written by both, and RH5307-001 states that exclusion.

With this change the list of an element binding is one of the lists FMT-BREAK-014 joins, under the same conditions: the gaps from its `[` to its `]` are joined, unless an argument spans several lines or a comment, a directive, or disabled text sits in a gap the join would remove (FMT-BREAK-001). The gap in front of the `?` and the gap between the `?` and the `[` stay FMT-BREAK-040's. RH5307 reports such a list exactly as it reports the list of an element access — same conditions, one diagnostic per list — and its fix joins it as the formatter does, changing only the list (SHARED-FIX-006). The sentence of the RH5307 rule page that excludes `target?[...]` is removed.

A null-forgiving operator in front of the brackets (`d![1,⏎2]`) is an element access whose receiver is the null-forgiving operator; the formatter and RH5307 already treat its list as any element access's. This change writes it into the examples so that `[`, `![`, and `?[` are stated alike.

The list of a fixed-size buffer's declarator (`fixed int b[3];`) is a bracketed argument list too, but it holds a size, not an index, and is not part of this change.

## Why

The maintainer's direction, given on thoenissen/Reihitsu#1869 and repeated in the triage of thoenissen/Reihitsu#1877: whether `?` or `!` stands in front of the index brackets must not change how the list is laid out, reported, or fixed. On `main` it does, because the syntax tree hangs the list of `c?[…]` under an element binding instead of an element access, and both the formatter's join and RH5307 test for the element access parent.

The RH5307 fix formats the list through the formatter, so the fix for `c?[…]` produces the joined list only once the formatter joins it; the two issues are one change (thoenissen/Reihitsu#1877 for the formatter, thoenissen/Reihitsu#1878 for RH5307). With the formatter joining the list and RH5307 silent on it, the rule would have its own gap on its own construct ([P11](../../README.md#the-formatter-may-do-more-than-the-analyzers-p11), *The rule's own gap*); with the formatter keeping it, SHARED-DIAG-010 would forbid the report. Joining in the formatter and reporting in RH5307 keeps both surfaces aligned.

## Visible output changes

Every *before* is the output of `main`, produced with `scripts/format.sh` or `scripts/apply-fix.sh`. Every *after* is the output the changed requirements give; it is not built yet.

**1. Whole-document formatting joins the wrapped list of a conditional element access.**

```csharp
// input
#nullable enable

class C
{
    void M(int[,]? c, int[,] d, int[,][,]? e)
    {
        var a = c?[1,
                   2];
        var g = c?[
            1, 2];
        var i = e?[0,
                   1]?[1,
                       2];
        var h = c?[1, // note
                   2];
        var b = d![1,
                   2];
    }
}
```

```csharp
// before
#nullable enable

class C
{
    void M(int[,]? c, int[,] d, int[,][,]? e)
    {
        var a = c?[1,
                   2];
        var g = c?[
                   1, 2];
        var i = e?[0,
                   1]?[1,
                       2];
        var h = c?[1, // note
                   2];
        var b = d![1, 2];
    }
}
```

```csharp
// after
#nullable enable

class C
{
    void M(int[,]? c, int[,] d, int[,][,]? e)
    {
        var a = c?[1, 2];
        var g = c?[1, 2];
        var i = e?[0, 1]?[1, 2];
        var h = c?[1, // note
                   2];
        var b = d![1, 2];
    }
}
```

The list with a comment between its brackets stays as written (FMT-BREAK-001); `d![…]` is joined on `main` already.

**2. Code fixes that format a node holding such a list join it as well.** Every code fix that formats a node through the formatter now joins a wrapped list of a conditional element access inside that node, as it already joins the list of an element access there. Observed with `scripts/apply-fix.sh` for RH5101, RH5111, RH5302, and RH5308; for example RH5111:

```csharp
// input
int a;
a
    = c?[1,
         2] ?? 0;
```

```csharp
// before
int a;
a = c?[1,
       2] ?? 0;
```

```csharp
// after
int a;
a = c?[1, 2] ?? 0;
```

RH5101 (`N(⏎c?[1,⏎2] ?? 0, …)`), RH5302 (`x &&⏎(c?[1,⏎2] ?? false) …`), and RH5308 (`x ?⏎(c?[1,⏎2] ?? 0) : …`) change the same way: the list inside the formatted node becomes `c?[1, 2]`. A fix that changes only its own gap, such as RH5115's outside a chain with links (RH5115-004), is not affected.

**3. RH5307 reports and fixes the list of a conditional element access.**

```csharp
// input
class C
{
    void M(int[,] c, int[][,] e)
    {
        var a = c?[1,
                   2];
        var i = e?[0][1,
                      2];
        var j = c?[1,
                   2].ToString();
    }
}
```

Before: `c?[1,⏎2]` and the list behind `c?` in `j` are not reported; `e?[0][1,⏎2]` is reported on `[0][1,⏎2]` and fixed to `e?[0][1, 2]`.

After (reported on the element binding, from `[` through `]`, as decided under *Report the list on its brackets*):

```csharp
class C
{
    void M(int[,] c, int[][,] e)
    {
        var a = c?{|RH5307:[1,
                   2]|};
        var i = e?{|RH5307:[0][1,
                      2]|};
        var j = c?{|RH5307:[1,
                   2]|}.ToString();
    }
}
```

Fixed:

```csharp
class C
{
    void M(int[,] c, int[][,] e)
    {
        var a = c?[1, 2];
        var i = e?[0][1, 2];
        var j = c?[1, 2].ToString();
    }
}
```

**4. A receiver list of a conditional element access is now reported as well.**

```csharp
// input
class C
{
    void M(int[,][,] c)
    {
        var n = c?[0,
                   1][1,
                      2];
    }
}
```

Before: one diagnostic, on the element access `[0,⏎1][1,⏎2]`; its fix gives `c?[0,⏎1][1, 2]`, and the receiver's list stays wrapped.

After: two diagnostics, on the element binding `[0,⏎1]` and on the element access `[0,⏎1][1,⏎2]`; each fix joins its own list, and fixing both gives `c?[0, 1][1, 2]`.

**5. The RH5307 rule page.** The sentence "A null-conditional index access (`target?[...]`) is not checked." is removed from `documentation/rules/RH5307.md`, and the description names `target?[...]` beside `target[...]`.

## Decisions

### One change for the formatter and the rule

The formatter's join and RH5307's report and fix change together, in one change doc, because the fix formats the list through the formatter and SHARED-DIAG-010 ties the report to the formatter's decision.

### The gaps are divided by token

FMT-BREAK-014 owns the gaps from the `[` of the element binding's list to its `]`; FMT-BREAK-040 keeps the gap in front of the `?` and the gap between the `?` and the `[`. RH5307 likewise judges only the list, and RH5115 the gaps in front of it, so a comment in front of the `?` or between the `?` and the `[` does not keep RH5307 from reporting the list, as a comment in front of an element access's `[` does not today.

### Join the list in the formatter (maintainer, question 1A)

The maintainer answered question 1 with A ("As recommended"): the formatter joins the wrapped argument list of a conditional element access (`c?[1,⏎2]` becomes `c?[1, 2]`), and every code fix that formats a node holding such a list joins it too (visible output changes 1 and 2). The existing regression expectation `a?[b[1, 2],⏎3]` becomes `a?[b[1, 2], 3]`.

### Report the list on its brackets (maintainer, question 2A)

The maintainer answered question 2 with A ("As recommended"): RH5307 reports the list of a conditional element access on its brackets, from the `[` through the `]` (RH5307-003), and the fix joins it (`c?[1,⏎2]` becomes `c?[1, 2]`); `e?[0,⏎1][1,⏎2]` gets two diagnostics and becomes `e?[0, 1][1, 2]` (visible output changes 3 and 4). The rule page drops the sentence that excludes `target?[...]` (visible output change 5).

### Which lists hold an index is stated once

RH5307-001 refers to FMT-BREAK-014's definition of a bracketed argument list that holds an index (its three parent kinds) instead of restating it, so the specs state the rule in one place.

## Alternatives considered

- **Narrow FMT-BREAK-014 to exclude the element binding's list, and keep RH5307 silent on it** — rejected by the maintainer (questions 1 and 2): contradicts the maintainer's direction that `?` and `!` in front of the brackets must not change the layout, and keeps the RH5307 rule page's exclusion.
- **Report `c?[…]` in RH5307 without the formatter's join** — rejected: the fix would produce nothing the formatter keeps, and SHARED-DIAG-010 forbids reporting a shape the formatter keeps.
- **Locate the diagnostic on the whole conditional access expression** — rejected by the maintainer (question 2): the conditional access also holds everything behind the brackets (`c?[1,⏎2].ToString()`), and the element accesses inside a conditional access are already located from the token behind the `?` (`c?{|RH5307:[0][1,⏎2]|}`).
- **Join the size list of a fixed-size buffer too** — not part of this change: it is no index, no issue asks for it, and no rule reports it.

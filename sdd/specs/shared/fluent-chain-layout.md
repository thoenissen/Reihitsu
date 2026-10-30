# Shared — Fluent chain layout

| | |
|---|---|
| ID prefix | SHARED-CHAIN |
| Status | Active |
| Code owners | `FluentChain`, `FluentChainLink`, `FluentChainAttachedPart`, `FluentChainUtilities`, `FluentChainAnalyzerBase`, `FluentChainCodeFixHelper` |
| Uses | FMT-PIPE-002, SHARED-BLANK-008, SHARED-COMMENT-012 |

This spec covers the layout of member-access chains (`items.Where(…).Select(…)`) that the analyzer and the formatter must agree on: what a chain is, when it counts as wrapped, which links must start a line, and the column wrapped links align to. The analyzer rules RH5112, RH5201, and RH5048 report against this policy, and the code fixes of RH5112 and RH5201 lay a chain out by it; the formatter's line-break phase (FMT-BREAK) and indentation phase (FMT-INDENT) produce it.

## Non-goals

- Deciding when a chain is too long. Neither surface wraps a chain the author wrote on one line.
- Comments between later links of a wrapped chain — only a comment in front of the first link has a rule.
- How the formatter rejoins a member name split from its dot or an attached part split from its element — that is formatter-only layout, specified in FMT-BREAK.

## Requirements

### SHARED-CHAIN-001 — A chain is a root, links, and attached parts

Analyzer and formatter MUST model a chain as one root followed by links and attached parts. A link is a member access or member binding together with its operator `.`, `?.`, `!.`, or `!?.`. An attached part — an argument list, an element access, a conditional element access, or a null-forgiving operator not followed by `.` or `?.` — belongs to the element in front of it and is never a link. The links in front of the first invoked link form the prefix; every link from the first invoked link on forms the chain part. A chain without an invoked link consists of its prefix only. Only the outermost node of a chain is analyzed or laid out as a chain.

Why: Every layout rule below counts links. Treating an argument list or an index as a link would force `[0]` or `(x)` onto a line of its own, and treating a property prefix (`settings.Items`) like a call would force every property read of a long expression onto its own line. One model in one place keeps both surfaces counting the same links.

Example — the element access `[0]` is an attached part, so it belongs to the `.Select(…)` element and does not start a line:

```csharp
class C
{
    void M()
    {
        var d = items.Where(x => x.A)
                     .Select(x => x.B)
                     [0];
    }
}
```

```csharp
class C
{
    void M()
    {
        var d = items.Where(x => x.A)
                     .Select(x => x.B)[0];
    }
}
```

### SHARED-CHAIN-002 — Only the author wraps a chain

A chain MUST count as wrapped only when one of its link operators starts a line (SHARED-BLANK-008) or holds a line break. Analyzer and formatter MUST NOT require or introduce a wrap in a chain the author wrote on one line, however long it is.

Why: Whether a chain is long enough to wrap depends on the reader, the screen, and the surrounding code; the policy only governs how a chain is wrapped once its author decided to wrap it.

Example:

```csharp
class C
{
    void M()
    {
        var a = items.Where(x => x.A).Select(x => x.B).OrderBy(x => x).ThenBy(x => x).ToList().AsReadOnly();
    }
}
```

is left unchanged.

### SHARED-CHAIN-003 — The first link stays on the root's line

The chain's first link MUST stay on the line the root ends on, whether it is a call or a property read. RH5112 reports a first link that starts a line; the formatter joins it to the root's line.

Why: The root says what the chain operates on. Starting the first link on a new line separates the receiver from the first thing done to it and leaves the root dangling on a line of its own.

Example:

```csharp
class C
{
    void M()
    {
        var c = items
                .Where(x => x.A)
                .Select(x => x.B);
        var a = order
                .Customer
                .Address;
    }
}
```

```csharp
class C
{
    void M()
    {
        var c = items.Where(x => x.A)
                     .Select(x => x.B);
        var a = order.Customer
                     .Address;
    }
}
```

### SHARED-CHAIN-004 — A comment or directive before the first link keeps it in place

When a comment, a preprocessor directive, or disabled text sits between the root and the chain's first link, SHARED-CHAIN-003 MUST NOT apply to that chain: RH5112 does not report it and the formatter does not join the link. RH5048 reports such a comment instead, so the author can move it.

Why: Joining the link would either move the comment behind code or swallow a directive into a line, both of which change user-authored content (FMT-PIPE-002). Reporting the comment rather than the link points the author at the actual obstacle.

Example:

```csharp
class C
{
    void M()
    {
        var a = items // c
                .Where(x => x.A)
                .Select(x => x.B);
        var b = items
#if X
                .Where(x => x.A)
#endif
                .Select(x => x.B);
    }
}
```

is left unchanged.

### SHARED-CHAIN-005 — In a wrapped chain, each later call starts its own line

In a wrapped chain, every link of the chain part after the first invoked link MUST start its own line. The prefix links and the links of a chain without an invoked link MUST keep the line the author put them on. RH5201 reports a later link that does not start a line; the formatter breaks the line in front of its operator.

The one exception is a link whose operator stands behind the closing `*/` of a delimited comment that spans several lines, which does not start its line (SHARED-COMMENT-012). In a wrapped chain such a link MUST start its own line, also when it is a prefix link, a link of a chain without an invoked link, or the first invoked link: the formatter breaks the line in front of its operator, and RH5201 reports the link as SHARED-COMMENT-012 specifies for a rule that checks in which column a line starts. This exception does not decide the chain's first link, which SHARED-CHAIN-003 and SHARED-CHAIN-004 govern, nor whether a chain whose only line breaks lie inside such a comment counts as wrapped.

Why: Once a chain is wrapped, one step per line is what makes it scannable; a wrapped chain that still carries several calls on one line hides steps at the end of a line. A property prefix (`settings.Items`) is not a step of the pipeline, so it may stay together on one line, and a chain of property reads has no steps at all. A link behind the tail of a comment that spans several lines has no line of its own to keep: the author wrote it on the line the comment started on, so it gets a line of its own like every other step of a wrapped chain.

Example:

```csharp
class C
{
    void M()
    {
        var a = items.Where(x => x.A)
                     .Select(x => x.B).ToList();
        var b = order.Customer
                     .Address.Street;
        var c = settings.Items
                        .Where(x => x.A).Select(x => x.B);
    }
}
```

```csharp
class C
{
    void M()
    {
        var a = items.Where(x => x.A)
                     .Select(x => x.B)
                     .ToList();
        var b = order.Customer
                     .Address.Street;
        var c = settings.Items
                        .Where(x => x.A)
                        .Select(x => x.B);
    }
}
```

Example — the prefix link `.B` and the first invoked link `.C()` stand behind the comment's last line, so each starts its own line and aligns with the anchor:

```csharp
class C
{
    void M(dynamic a)
    {
        var r = a.B

                 /* x
        */ .B
                 .C()
                 .D();
        var s = a.B

                 /* x
        */ .C()
                 .D();
    }
}
```

```csharp
class C
{
    void M(dynamic a)
    {
        var r = a.B

                 /* x
        */
                 .B
                 .C()
                 .D();
        var s = a.B

                 /* x
        */
                 .C()
                 .D();
    }
}
```

### SHARED-CHAIN-006 — A link operator is never split

A line break MUST NOT sit inside a link operator (between the `?` and the `.` of `?.`, or between the `!` and the `.` of `!.`). RH5201 reports such a link; the formatter moves the line break in front of the operator's first token.

Why: `?.` and `!.` read as one operator. Splitting them leaves a `?` or `!` dangling at the end of a line, where it reads as a conditional or a negation rather than as part of the next link.

Example:

```csharp
class C
{
    void M()
    {
        var a = items.Where(x => x.A)?
                     .Select(x => x.B);
    }
}
```

```csharp
class C
{
    void M()
    {
        var a = items.Where(x => x.A)
                     ?.Select(x => x.B);
    }
}
```

### SHARED-CHAIN-007 — Wrapped links align to the anchor link

Every link that starts a line MUST start in the column of the chain's anchor: the operator of the first invoked link in front of the first link that starts a line, or the chain's first link when no invoked link comes before it. When SHARED-CHAIN-004 keeps the first link on its own line, the anchor is the start of the root. RH5201 reports a misaligned link; the formatter's indentation phase aligns it.

Why: Aligning the dots makes the chain read as a column of steps under the first one. The fallback to the root keeps the chain aligned when a comment prevents the first link from joining the root's line.

Example:

```csharp
class C
{
    void M()
    {
        var a = items.Where(x => x.A)
            .Select(x => x.B);
        var b = this.Settings.Items
            .Where(x => x.A);
        var c = items // c
            .Where(x => x.A);
    }
}
```

```csharp
class C
{
    void M()
    {
        var a = items.Where(x => x.A)
                     .Select(x => x.B);
        var b = this.Settings.Items
                    .Where(x => x.A);
        var c = items // c
                .Where(x => x.A);
    }
}
```

### SHARED-CHAIN-008 — A chain fix gives the chain its formatted layout

A code fix that lays out a chain MUST give the whole chain — every link, not only the reported one — the line breaks and columns that formatting the document gives it, and MUST NOT split a link operator.

Why: One diagnostic of a chain is usually one of several, and fixing one link at a time could leave the chain in a layout the formatter would change again. Taking the layout the formatter gives the whole chain, instead of a layout of its own, clears every diagnostic the chain had in one application.

Example — the RH5201 fix, applied to the first of two misplaced links:

```csharp
using System.Linq;

class C
{
    void M(int[] items)
    {
        var a = items.Where(x => x > 0)
            {|RH5201:.|}Select(x => x){|RH5201:.|}ToList();
    }
}
```

```csharp
using System.Linq;

class C
{
    void M(int[] items)
    {
        var a = items.Where(x => x > 0)
                     .Select(x => x)
                     .ToList();
    }
}
```

### SHARED-CHAIN-009 — A chain fix formats more than the chain only when the chain's layout depends on it

A code fix that lays out a chain MUST change only the chain when the chain alone, formatted in the unchanged code around it, gets the same text and starts in the same column as it does when the innermost statement or member declaration that contains the chain and starts a line is formatted. When it does not — for example because formatting changes the spacing of a declaration in front of the chain — the fix MUST write back that formatted statement or member declaration instead.

Why: The columns of a wrapped chain depend on where the chain starts, and the formatter computes them for the code in front of the chain as it will be after formatting. Writing back only the chain keeps the rest of the statement as written; the formatted statement is needed only when the chain alone would get other columns than formatting gives it.

Example — the RH5201 fix leaves the code behind the chain as written:

```csharp
class C
{
    int M(dynamic a, int d)
    {
        var r = a.B()
            {|RH5201:.|}C() + d+1;

        return r;
    }
}
```

```csharp
class C
{
    int M(dynamic a, int d)
    {
        var r = a.B()
                 .C() + d+1;

        return r;
    }
}
```

The RH5112 fix formats the declaration in front of the chain as well:

```csharp
class C
{
    object M(dynamic a)
    {
        var r=a
            {|RH5112:.|}B()
            .C();

        return r;
    }
}
```

```csharp
class C
{
    object M(dynamic a)
    {
        var r = a.B()
                 .C();

        return r;
    }
}
```

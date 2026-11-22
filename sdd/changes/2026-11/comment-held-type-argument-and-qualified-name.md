# A comment-held first element of an angle-bracket list, and a line inside a qualified name, get a column of their own

| | |
|---|---|
| Status | Draft |
| Date | 2026-11 |
| Affected requirements | FMT-INDENT-037 (new), FMT-INDENT-038 (new) |
| Depends on | [continuation-indent.md](continuation-indent.md) |

## What

When a comment, a directive, or disabled text keeps the first element of an attribute list, a type argument list, a type parameter list, or a function-pointer parameter list below its opening bracket, that element is indented to the column directly after the bracket (FMT-INDENT-037); the later elements and the closing bracket already align under it (FMT-INDENT-015).

A line that starts inside a qualified name — the part behind a `.`, or the `.` itself — is indented four columns right of the name's first token (FMT-INDENT-038), where no requirement but the continuation indent places it. Inside an element of an argument, parameter, attribute, angle-bracket, or base list, FMT-INDENT-013 keeps aligning such a line with the element's first token.

This change covers thoenissen/Reihitsu#1634.

## Why

No requirement places these lines, so on `main` they get the continuation indent of FMT-INDENT-034, four columns right of the line on which the construct they continue starts. For a type argument list that puts the first element left of the column FMT-INDENT-015 gives every later element, and for a qualified name that starts in the middle of its line — the type of a constraint behind `where T :` — it puts the name's right part left of its own first token. [continuation-indent.md](continuation-indent.md) left both shapes to a requirement that aligns them, which then takes precedence.

The two shapes are decided differently because they stand differently. An angle-bracket list's elements already align with each other behind the bracket (FMT-INDENT-015), and an argument list whose first argument a comment pushes down aligns behind its parenthesis (FMT-INDENT-012). A qualified name often starts its line, as a member's type does; aligning its right part with its first token would put that part in the member's column, where it reads as a member of its own, so it is set one level deeper than the name instead.

## Visible output changes

Every example is whole-document formatting; "Before" is the output on `main`, observed with `scripts/format.sh`, and stable on a second pass. A comment inside a field's type is refused (FMT-UNSUP-004), so the examples use a property.

### 1. A first type argument or type parameter that a comment keeps below the bracket

Input:

```csharp
using System;
using System.Collections.Generic;

class C
{
    List< // c
        int> P => null;

    void M<T>()
        where T : class, IComparable< // c
            T>, new()
    {
    }

    void N(List< // c
        int> a)
    {
        var b = new List< // c
            int>();
    }
}

class D<
    // c
    T>
{
}
```

Before:

```csharp
using System;
using System.Collections.Generic;

class C
{
    List< // c
        int> P => null;

    void M<T>()
        where T : class,
                  IComparable< // c
                      T>,
                  new()
    {
    }

    void N(List< // c
           int> a)
    {
        var b = new List< // c
            int>();
    }
}

class D<
    // c
    T>
{
}
```

After:

```csharp
using System;
using System.Collections.Generic;

class C
{
    List< // c
         int> P => null;

    void M<T>()
        where T : class,
                  IComparable< // c
                              T>,
                  new()
    {
    }

    void N(List< // c
                int> a)
    {
        var b = new List< // c
                         int>();
    }
}

class D<
        // c
        T>
{
}
```

An attribute list's first attribute behaves the same way: `[ // c⏎Obsolete]` puts `Obsolete]` in column 4 on `main` and in column 1, behind the `[`, after this change.

### 2. A line inside a qualified name

Input:

```csharp
using System;

class C
{
    System. // c
        IDisposable P => null;

    void N<T>()
        where T : System. // c
            IDisposable
    {
    }
}
```

Before: the input; `IDisposable` stands in column 8 in the property and in column 12 in the constraint, left of `System` in column 18.

After:

```csharp
using System;

class C
{
    System. // c
        IDisposable P => null;

    void N<T>()
        where T : System. // c
                      IDisposable
    {
    }
}
```

The property's line keeps its column: the name starts its line, so four columns right of the name is four columns right of the line.

### Code fixes

RH5110's fix formats the constraint clause it puts on its own line, so a comment-held first type argument inside a constraint moves with it: on `void M<T>() where T : class, IComparable< // c⏎T>, new()`, `T>,` goes to column 22 on `main` (observed with `scripts/apply-fix.sh`) and to column 30 after this change. Every other fix that formats a member, a type, or a statement through the formatter places these lines as whole-document formatting does.

No rule page changes: no rule page shows such a line.

## Decisions

### A first element kept below its bracket aligns behind the bracket

Pending the maintainer's answer. The recommendation aligns it, as the issue's reading A and FMT-INDENT-012 do.

### A line inside a qualified name is one level deeper than the name

Pending the maintainer's answer. The recommendation differs from the issue's reading A, which aligned the right part with the name's first token: for a member's type that starts its line, that column is the member's.

### The lists are the lists of FMT-INDENT-015

The issue names angle-bracket lists. FMT-INDENT-015 aligns the later elements of attribute lists and angle-bracket lists alike, so the first element follows the same rule for all of them rather than for a list of bracket kinds.

## Alternatives considered

- **Keep the continuation indent of FMT-INDENT-034 for both shapes** (today's output) — not recommended: a first type argument then stands left of the column of the list's later elements, and the right part of a name in a constraint stands left of the name.
- **Align the right part of a qualified name with the name's first token** (the issue's reading A for names) — not recommended: in a member's type the right part then stands in the member's column. Inside a list element, FMT-INDENT-013 already aligns it so and stays unchanged.
- **Indent a first type argument one level deeper than the line of the list's start** (the issue's reading B) — not recommended: it leaves the first element apart from the column every later element takes.

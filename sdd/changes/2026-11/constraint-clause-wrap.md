# Wrapped constraint clauses get one constraint per line, aligned to the first constraint

| | |
|---|---|
| Status | Draft |
| Date | 2026-11 |
| Affected requirements | FMT-BREAK-037 (added), FMT-INDENT-032 (added) |

## What

- **One line or one constraint per line** (FMT-BREAK-037). The constraints of a `where` clause either all stay on the line of the clause's colon, or each start a line of their own. A clause is wrapped when a line break stands behind its colon, or when its constraints span more than one line — through a line break around a comma between two constraints, or inside one constraint. A wrapped clause becomes one constraint per line: the first constraint moves onto the colon's line, every later constraint starts a line no other constraint shares, and each comma stands at the end of the line of the constraint in front of it. A join never crosses a comment, a directive, or disabled text (FMT-BREAK-001). A clause written on one line stays on one line.
- **Aligned to the first constraint** (FMT-INDENT-032). Every constraint and every comma of a clause that starts a line is indented to the column of the clause's first constraint, the one directly behind the colon. When a comment behind the colon keeps the first constraint on the next line, the column is the one the first constraint would have behind the colon.

Until now no requirement owned these lines. FMT-BREAK-023 starts each `where` clause on its own line and FMT-INDENT-017 indents that line; a constraint that started a line fell back to the block column of the enclosing declaration's scope, one level left of its `where` keyword, and the formatter kept partly wrapped clauses as written.

## Why

thoenissen/Reihitsu#1631 observed by running the formatter that the wrapped constraints of a clause land one level left of their `where` keyword, and that a clause can keep some constraints on the `where` line while others are wrapped. Every other wrapped list the formatter handles is either on one line or one element per line (FMT-BREAK-012), and aligned to its first element or its opening delimiter (FMT-INDENT-012, FMT-INDENT-015, FMT-INDENT-016); constraint clauses had neither rule. The rules here follow the parameter-list model: the first element on the opening line, commas at the end of the line, the rest aligned under the first element.

## Visible output changes

Each entry shows the formatter's output on `main` (`scripts/format.sh` at b1d4a94) and the output after this change. Inputs use `using System;` and `using System.Collections.Generic;` where a constraint names an interface.

### 1. A partly wrapped clause becomes one constraint per line, aligned

```csharp
class C
{
    void M<T>()
        where T : class, IComparable<T>,
            IEquatable<T>, new()
    {
    }
}
```

Before:

```csharp
class C
{
    void M<T>()
        where T : class, IComparable<T>,
    IEquatable<T>, new()
    {
    }
}
```

After:

```csharp
class C
{
    void M<T>()
        where T : class,
                  IComparable<T>,
                  IEquatable<T>,
                  new()
    {
    }
}
```

### 2. Constraints already one per line are aligned

```csharp
class A<TKey, TValue>
    where TKey : class,
    new()
    where TValue : struct
{
    void M()
    {
    }
}
```

Before:

```csharp
class A<TKey, TValue>
    where TKey : class,
new()
    where TValue : struct
{
    void M()
    {
    }
}
```

After:

```csharp
class A<TKey, TValue>
    where TKey : class,
                 new()
    where TValue : struct
{
    void M()
    {
    }
}
```

### 3. A wrap behind the colon is joined

```csharp
class C
{
    void M<T>()
        where T :
            IDisposable
    {
    }

    void N<T>()
        where T :
            class,
            new()
    {
    }
}
```

Before:

```csharp
class C
{
    void M<T>()
        where T :
    IDisposable
    {
    }

    void N<T>()
        where T :
    class,
    new()
    {
    }
}
```

After:

```csharp
class C
{
    void M<T>()
        where T : IDisposable
    {
    }

    void N<T>()
        where T : class,
                  new()
    {
    }
}
```

### 4. A comma at the start of a line moves to the end of the previous line

```csharp
class C
{
    void M<T>()
        where T : class
            , new()
    {
    }
}
```

Before:

```csharp
class C
{
    void M<T>()
        where T : class
    , new()
    {
    }
}
```

After:

```csharp
class C
{
    void M<T>()
        where T : class,
                  new()
    {
    }
}
```

### 5. Comments keep their place; the constraints around them are aligned

```csharp
class C
{
    void M<T>()
        where T : class, // c
            new()
    {
    }

    void N<T>()
        where T : class, /* c */ new(),
            IDisposable
    {
    }

    void O<T>()
        where T : class // c
            , new()
    {
    }

    void P<T>()
        where T : // c
            class,
            new()
    {
    }
}
```

Before:

```csharp
class C
{
    void M<T>()
        where T : class, // c
    new()
    {
    }

    void N<T>()
        where T : class, /* c */ new(),
    IDisposable
    {
    }

    void O<T>()
        where T : class // c
    , new()
    {
    }

    void P<T>()
        where T : // c
    class,
    new()
    {
    }
}
```

After:

```csharp
class C
{
    void M<T>()
        where T : class, // c
                  new()
    {
    }

    void N<T>()
        where T : class, /* c */
                  new(),
                  IDisposable
    {
    }

    void O<T>()
        where T : class // c
                  , new()
    {
    }

    void P<T>()
        where T : // c
                  class,
                  new()
    {
    }
}
```

### 6. A constraint that spans several lines splits its clause

```csharp
class C
{
    void M<T>()
        where T : class, IDictionary<int, // c
            string>, new()
    {
    }
}
```

Before:

```csharp
class C
{
    void M<T>()
        where T : class, IDictionary<int, // c
                                     string>, new()
    {
    }
}
```

After:

```csharp
class C
{
    void M<T>()
        where T : class,
                  IDictionary<int, // c
                              string>,
                  new()
    {
    }
}
```

### 7. Members without a body, local functions, and delegates follow the same layout

```csharp
interface I
{
    void M<T>()
        where T : class,
        new();
}

class C
{
    void M()
    {
        void L<T>()
            where T : class,
            new()
        {
        }
    }
}

delegate void D<T>()
    where T : class,
    new();
```

Before:

```csharp
interface I
{
    void M<T>()
        where T : class,
    new();
}

class C
{
    void M()
    {
        void L<T>()
            where T : class,
        new()
        {
        }
    }
}

delegate void D<T>()
    where T : class,
new();
```

After:

```csharp
interface I
{
    void M<T>()
        where T : class,
                  new();
}

class C
{
    void M()
    {
        void L<T>()
            where T : class,
                      new()
        {
        }
    }
}

delegate void D<T>()
    where T : class,
              new();
```

### 8. RH5110's code fix lays out the constraints of the declaration it fixes the same way

RH5110's fix formats the declaration as the formatter does (RH5110-003), so its result changes with the formatter's.

```csharp
class C
{
    void M<T>() {|RH5110:where|} T : class,
        new()
    {
    }

    void N<T>() {|RH5110:where|} T :
        IDisposable
    {
    }
}
```

Before (fix result on `main`, `scripts/apply-fix.sh RH5110`):

```csharp
class C
{
    void M<T>()
        where T : class,
    new()
    {
    }

    void N<T>()
        where T :
    IDisposable
    {
    }
}
```

After:

```csharp
class C
{
    void M<T>()
        where T : class,
                  new()
    {
    }

    void N<T>()
        where T : IDisposable
    {
    }
}
```

### Unchanged

- A clause written on one line, such as `where T : class, new()`, stays on one line.
- RH5110's diagnostics, and every rule page, are unchanged: RH5110 still reports only the line and column of the `where` keyword.

## Decisions

### One line or one constraint per line

A wrapped clause is laid out like a parameter list (FMT-BREAK-012, FMT-BREAK-011): one constraint per line, the first on the opening line, commas at the end of the line. Keeping the author's line breaks and only aligning them, as base lists do (FMT-INDENT-016), would keep partly wrapped clauses that hide constraints at the end of a line, which is the shape the issue reports.

### The first constraint joins the colon's line

A wrap behind the colon is joined, as a wrap behind the opening parenthesis of an argument or parameter list is (FMT-BREAK-012). The issue left this case open and sketched one level under `where` as one possible result; joining leaves one rule for the column of every wrapped constraint and turns a single wrapped constraint into the one-line clause it is.

### A multi-line constraint splits its clause

A constraint that spans several lines — possible only where a comment or a directive keeps a type argument list wrapped (FMT-BREAK-014) — counts as a wrap of its clause, as a multi-line element splits an argument or parameter list (FMT-BREAK-013).

### The column behind the colon when a comment pushes the first constraint down

The wrapped constraints take the column the first constraint has when it stands behind the colon, as an argument list whose first argument a comment pushes onto the next line aligns to the column after its opening parenthesis (FMT-INDENT-012). The column of a constraint then does not depend on whether a comment stands behind the colon.

### No analyzer change

RH5110 names the position of the `where` keyword and excludes line breaks inside a clause by its non-goal. The formatter may lay out more than the rules report ([P11](../../README.md#the-formatter-may-do-more-than-the-analyzers-p11)), so no rule is widened and no rule page changes.

## Alternatives considered

- **Align only, keep the author's line breaks** — rejected: leaves partly wrapped clauses (entry 1) as they are; the issue's first rule asks for all or none.
- **Keep a wrap behind the colon and indent the constraints one level deeper than `where`** — the issue's sketch for that case; rejected: it gives wrapped constraints two different columns depending on where the first constraint stands, and differs from the argument- and parameter-list model.
- **Keep a comma that starts a line in front of its constraint** — rejected: FMT-BREAK-011 puts the commas of a parameter list at the end of the line for the same reason.
- **Indent every wrapped constraint one level deeper than `where` instead of aligning** — rejected: every other wrapped list in a declaration header aligns to its first element (FMT-INDENT-012, FMT-INDENT-015, FMT-INDENT-016).

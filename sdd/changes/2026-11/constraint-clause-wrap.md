# Wrapped constraint clauses get one constraint per line, aligned to the first constraint

| | |
|---|---|
| Status | Accepted |
| Date | 2026-11 |
| Affected requirements | FMT-BREAK-037 (added), FMT-INDENT-032 (added), FMT-SPACE-009 (changed), RH5110-003 (changed) |

## What

These rules hold for every `where` clause, whichever declaration owns it: a type, a method, a local function, a delegate, or an extension block.

- **Wraps that structure nothing are joined first** (FMT-BREAK-037). The colon joins the line of the clause's type parameter, and a line break inside one constraint is joined, for example in a qualified name, a type argument list, `new()`, `class?`, or `allows ref struct`. The clause is judged after these joins.
- **One line or one constraint per line** (FMT-BREAK-037). A clause is wrapped in two cases: a line break stands behind its colon, or its constraints span more than one line. A wrapped clause becomes one constraint per line. The first constraint moves onto the colon's line, every later constraint starts a line no other constraint shares, and each comma stands at the end of the line of the constraint in front of it. A clause written on one line stays on one line.
- **Comments and directives block joins, never splits** (FMT-BREAK-001, FMT-BREAK-037). A comment, a directive, or disabled text in a gap keeps the line break there. A colon or a first constraint kept that way starts its own line, and a colon kept that way wraps its clause. A comma kept that way stays first on its line. A constraint kept over two lines that way wraps its clause.
- **Aligned to the first constraint** (FMT-INDENT-032). Every constraint and every comma of a clause that starts a line is indented to the column of the clause's first constraint, the one directly behind the colon. Where a comment or a directive keeps the first constraint below the colon, the column is the one the first constraint would have behind the colon. Where one keeps the colon below the type parameter, the colon is indented one level deeper than `where`, and the constraints align under the first constraint behind it.
- **One space on each side of the colon** (FMT-SPACE-009). The colon of a constraint clause is spaced like the colon of a base list, wherever it shares a line with a neighbor: a clause written `where T:class` becomes `where T : class`, and a colon a join writes gets its spaces too.
- **RH5110's fix lays out the constraints too** (RH5110-003). The fix already formats the declaration as the formatter does. Its result now includes the constraint layout. Where a comment, a directive, or disabled text blocks a join, the fix is still offered, and its result is the fallback layout above.

Until now no requirement owned these lines. FMT-BREAK-023 starts each `where` clause on its own line, and FMT-INDENT-017 indents that line. A colon or a constraint that started a line fell back to the block column of the enclosing scope, one level left of its `where` keyword. The formatter also kept partly wrapped clauses as written.

## Why

thoenissen/Reihitsu#1631 observed by running the formatter that the wrapped constraints of a clause land one level left of their `where` keyword, and that a clause can keep some constraints on the `where` line while others are wrapped. Every other wrapped list the formatter handles is either on one line or one element per line (FMT-BREAK-012), and aligned to its first element or its opening delimiter (FMT-INDENT-012, FMT-INDENT-015, FMT-INDENT-016); constraint clauses had neither rule. The rules here follow the parameter-list model: the first element on the opening line, commas at the end of the line, the rest aligned under the first element.

## Visible output changes

Each entry shows the formatter's output on `main` (`scripts/format.sh` at b1d4a94, entries 9–13 at c317972, entries 14 and 15 at e3924e2, with no production change in between) and the output after this change. Inputs use `using System;` and `using System.Collections.Generic;` where a constraint names an interface.

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

### 6. A constraint that a comment keeps over several lines splits its clause

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

### 9. A wrap between the type parameter and the colon is joined; a comment keeps the colon on its own line

```csharp
class C
{
    void M<T>()
        where T
            : class,
            new()
    {
    }

    void N<T>()
        where T // c
            : class,
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
        where T
    : class,
    new()
    {
    }

    void N<T>()
        where T // c
    : class,
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
        where T : class,
                  new()
    {
    }

    void N<T>()
        where T // c
            : class,
              new()
    {
    }
}
```

### 10. A wrap inside a constraint is joined before the clause is judged

```csharp
class C
{
    void M<T>()
        where T : class, System.
            IDisposable, new()
    {
    }
}
```

Before:

```csharp
class C
{
    void M<T>()
        where T : class, System.
    IDisposable, new()
    {
    }
}
```

After:

```csharp
class C
{
    void M<T>()
        where T : class, System.IDisposable, new()
    {
    }
}
```

A wrapped type argument list in a constraint was already joined (FMT-BREAK-014). It stays joined and no longer wraps the clause: `where T : class, IDictionary<int,⏎string>, new()` gives `where T : class, IDictionary<int, string>, new()`, as before.

### 11. Directives keep their lines; the constraints around them are aligned

```csharp
class C
{
    void M<T>()
        where T : IDisposable
#if !X
            , allows ref struct
#endif
    {
    }

    void N<T>()
        where T :
#if !X
            class,
#endif
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
        where T : IDisposable
#if !X
    , allows ref struct
#endif
    {
    }

    void N<T>()
        where T :
#if !X
    class,
#endif
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
#if !X
                  , allows ref struct
#endif
    {
    }

    void N<T>()
        where T :
#if !X
                  class,
#endif
                  new()
    {
    }
}
```

### 12. Extension blocks follow the same layout

```csharp
static class E
{
    extension<T>(T value)
        where T : class,
        new()
    {
        public void X()
        {
        }
    }
}
```

Before:

```csharp
static class E
{
    extension<T>(T value)
        where T : class,
    new()
    {
        public void X()
        {
        }
    }
}
```

After:

```csharp
static class E
{
    extension<T>(T value)
        where T : class,
                  new()
    {
        public void X()
        {
        }
    }
}
```

Where the `where` keyword of an extension block starts its line is unchanged: FMT-BREAK-023 does not apply to extension blocks today, and this change does not extend it.

### 13. RH5110's fix is offered where a comment blocks a join, with the fallback layout

```csharp
class C
{
    void M<T>() {|RH5110:where|} T : // c
        class, new()
    {
    }

    void N<T>() {|RH5110:where|} T : class // c
        , new()
    {
    }
}
```

Before (fix result on `main`):

```csharp
class C
{
    void M<T>()
        where T : // c
    class, new()
    {
    }

    void N<T>()
        where T : class // c
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
        where T : // c
                  class,
                  new()
    {
    }

    void N<T>()
        where T : class // c
                  , new()
    {
    }
}
```

### 14. The colon of a constraint clause gets one space on each side

```csharp
class C
{
    void M<T>()
        where T:class
    {
    }

    void N<T>()
        where T :new()
    {
    }

    void O<T>()
        where T: struct
    {
    }
}

class D<T>
    where T:class
{
    void P()
    {
    }
}
```

Before: unchanged.

After:

```csharp
class C
{
    void M<T>()
        where T : class
    {
    }

    void N<T>()
        where T : new()
    {
    }

    void O<T>()
        where T : struct
    {
    }
}

class D<T>
    where T : class
{
    void P()
    {
    }
}
```

The colons that entries 3, 8, and 9 join get the same spaces.

### 15. A colon that a comment keeps on its own line wraps its clause

The formatter:

```csharp
class C
{
    void M<T>()
        where T // c
            : class, new()
    {
    }
}
```

Before:

```csharp
class C
{
    void M<T>()
        where T // c
    : class, new()
    {
    }
}
```

After:

```csharp
class C
{
    void M<T>()
        where T // c
            : class,
              new()
    {
    }
}
```

RH5110's fix gives the same layout:

```csharp
class C
{
    void M<T>() {|RH5110:where|} T // c
        : class, new()
    {
    }
}
```

Before (fix result on `main`, `scripts/apply-fix.sh RH5110`):

```csharp
class C
{
    void M<T>()
        where T // c
    : class, new()
    {
    }
}
```

After:

```csharp
class C
{
    void M<T>()
        where T // c
            : class,
              new()
    {
    }
}
```

### Unchanged

- A clause written on one line, such as `where T : class, new()`, stays on one line; only the spaces around its colon change (entry 14).
- RH5110's diagnostics and every rule page are unchanged. RH5110 still reports only the line and column of the `where` keyword, and its fix is offered for every input it was offered for before.

## Decisions

The maintainer answered the run's output questions in several batches and chose the recommended option, A, for each of them. Each answer is recorded below with the requirement that carries it.

### Q1: one constraint per line, aligned under the first constraint

A wrapped clause puts one constraint per line, aligned under its first constraint, the column behind `where T : ` (entries 1, 2, 7, 12; FMT-BREAK-037, FMT-INDENT-032). This follows the parameter-list model (FMT-BREAK-012, FMT-INDENT-012). The rejected option kept the author's line breaks and only aligned the wrapped lines, as base lists do (FMT-INDENT-016).

### Q2: a wrap behind the colon is joined

A first constraint wrapped below the colon is joined onto the colon's line, and the clause is then laid out by Q1 (entry 3; FMT-BREAK-037). An argument or parameter list joins a wrap behind its opening parenthesis the same way (FMT-BREAK-012). The rejected options kept the line, either one level deeper than `where` (the issue's sketch) or aligned behind the colon.

### Q3: the column behind the colon when a comment or directive blocks that join

Where a comment, a directive, or disabled text keeps the first constraint below the colon, the constraints take the column the first constraint would have behind the colon (entries 5 and 11; FMT-INDENT-032). An argument list whose first argument a comment pushes down aligns the same way (FMT-INDENT-012). The rejected option used one level deeper than `where`.

### Q4: commas at the end of the line

A comma at the start of a line moves to the end of the previous line (entry 4; FMT-BREAK-037), as in parameter lists (FMT-BREAK-011). Where a comment or a directive blocks that move, the comma stays first on its line and is aligned to the constraint column (entries 5 and 11; FMT-INDENT-032). The rejected option kept every comma first and aligned it.

### Q5: a constraint held over two lines wraps its clause

A constraint that a comment or a directive keeps over two lines wraps the whole clause, one constraint per line, as a multi-line element splits a list (FMT-BREAK-013). The constraint's inner line stays aligned inside the constraint, measured from where the constraint now stands (entry 6; FMT-BREAK-037, FMT-INDENT-032). Such a constraint is not limited to type argument lists. A comment can hold a wrap in a qualified name, in `new()`, in `class?`, or in `allows ref struct` as well. Only an inner line that an existing requirement places, such as a later type argument (FMT-INDENT-015), is aligned inside the constraint. Other inner lines are not decided by this change.

### Q6: wraps inside a constraint are joined first

A wrap inside a constraint that no comment or directive holds is joined, and the clause is judged after the join (entry 10; FMT-BREAK-037), as wraps inside a list element are joined before the list is laid out (FMT-BREAK-015). The rejected options let such a wrap stay and split the clause, either for every kind of wrap or for every kind except type argument lists.

### Q7: the colon joins the type parameter's line

In `where T⏎ : …` the colon joins the type parameter's line. Where a comment or a directive blocks the join, the colon keeps its own line, indented one level deeper than `where`, and the constraints align under the first constraint behind it (entry 9; FMT-BREAK-037, FMT-INDENT-032). The rejected option kept every such colon on its own line.

### Q8: RH5110's fix is withheld only where a comment, directive, or disabled text would have to move

RH5110's fix is withheld for inputs where its layout could be reached only by moving a comment, a directive, or a literal. Q9 narrowed this answer. The fix formats through the formatter, and FMT-BREAK-037 never joins across a comment, a directive, or disabled text (FMT-BREAK-001). A constraint clause cannot hold a literal. No input therefore reaches the layout by moving one, and no withholding specific to RH5110 remains. SHARED-FIX-003 alone governs, as it does for every fix. No RH5110 requirement records a withheld fix.

### Q9: the fix stays offered where a join is blocked

Where a comment, a directive, or disabled text blocks a join that FMT-BREAK-037 would make, RH5110's fix is still offered (entry 13; RH5110-003). Its result is the formatter's fallback layout. A comment-held first constraint takes the column behind the colon (Q3). A comment-held comma stays first on its line and is aligned (Q4). A comment-held colon goes one level deeper than `where` (Q7). The rejected option withheld the fix for these inputs. That option would have given away inputs the fix rewrites safely, because the fallback layout moves nothing the user wrote.

### Q10: one space on each side of the constraint colon

The colon of a constraint clause always gets one space on each side, like the colon of a base list (entry 14; FMT-SPACE-009). This also changes a clause written on one line: `where T:class` becomes `where T : class`. The question arose because FMT-BREAK-037's joins write no space (FMT-BREAK-031), and no spacing rule covered this colon, so a joined colon would have stayed tight (`where T :IDisposable`). The rejected option let only the join write the space, as an exception to FMT-BREAK-031, and left a colon the author wrote tight on one line as it is.

### Q11: a colon kept on its own line wraps its clause

Where a comment, a directive, or disabled text keeps the colon on its own line, the clause is wrapped: its constraints go one per line, aligned under the first constraint behind the colon (entry 15; FMT-BREAK-037, FMT-INDENT-032). RH5110's fix gives the same result (RH5110-003). The rejected option judged such a clause only by its constraints, so `: class, new()` would have stayed on one line below `where T // c`.

### Scope: extension blocks

FMT-BREAK-037 and FMT-INDENT-032 apply to the clauses of every owner, extension blocks included (entry 12). Whether an extension block's `where` keyword starts its own line is decided by FMT-BREAK-023 and RH5110-001. Neither covers extension blocks today, and changing that is outside this change.

### No analyzer change

RH5110 names the position of the `where` keyword, and its non-goal leaves out line breaks inside a clause. RH6021 checks the spacing of only the base-list and constructor-initializer colons, and its non-goal now states that the constraint colon the formatter spaces (Q10) is a different construct, which a rule of its own would report. The formatter may lay out more than the rules report ([P11](../../README.md#the-formatter-may-do-more-than-the-analyzers-p11)). No rule is widened, and no rule page changes.

## Alternatives considered

- **Align only, keep the author's line breaks** — rejected: leaves partly wrapped clauses (entry 1) as they are; the issue's first rule asks for all or none.
- **Keep a wrap behind the colon and indent the constraints one level deeper than `where`** — the issue's sketch for that case; rejected (Q2, Q3).
- **Keep a wrap between the type parameter and the colon** — rejected (Q7).
- **Let a wrap inside a constraint stay and split the clause** — rejected (Q6).
- **Withhold RH5110's fix where a comment or directive blocks a join** — rejected (Q9).
- **Let only a join write the space behind the constraint colon (an exception to FMT-BREAK-031)** — rejected (Q10).
- **Judge a clause whose colon a comment keeps on its own line by its constraints alone** — rejected (Q11).
- **Keep a comma that starts a line in front of its constraint** — rejected: FMT-BREAK-011 puts the commas of a parameter list at the end of the line for the same reason.
- **Indent every wrapped constraint one level deeper than `where` instead of aligning** — rejected: every other wrapped list in a declaration header aligns to its first element (FMT-INDENT-012, FMT-INDENT-015, FMT-INDENT-016).

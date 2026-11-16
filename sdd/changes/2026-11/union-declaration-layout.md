# Union declarations are laid out like the other type declarations

| | |
|---|---|
| Status | Accepted |
| Date | 2026-11 |
| Affected requirements | FMT-BREAK-002, FMT-BREAK-007, FMT-BREAK-011, FMT-BREAK-023, FMT-STRUCT-017, FMT-STRUCT-021, FMT-PIPE-029, FMT-PIPE-031, RH7101-002 |
| Depends on | [Roslyn 5.12 and C# 15 as the newest supported language version](roslyn-512-csharp-15.md) |

## What

A C# 15 union declaration (`union U(int, string) { … }`) gets the layout and the structural rewrites the other type declarations get:

- its braces start their own lines, and its first member starts the line after the `{` (FMT-BREAK-002);
- each of its members starts its own line (FMT-BREAK-007);
- its case-type list is laid out as a primary constructor's parameter list: the `(` joins the union's name, the commas end their lines, the `)` joins the last case type, and a wrapped list keeps one case type per line, aligned under the first (FMT-BREAK-011, with FMT-BREAK-012 and FMT-INDENT-012, which already apply);
- each of its `where` clauses starts its own line, also on a union declared with a semicolon (FMT-BREAK-023);
- an empty brace body becomes a semicolon, from C# 15 on (FMT-STRUCT-017), unless a comment or a directive in the body keeps the braces (FMT-STRUCT-018, unchanged);
- a field declaration with several declarators in a union is split into one field per declarator (FMT-STRUCT-021), and `SplitsFieldDeclaration` answers true for it (FMT-PIPE-031), so the RH7101 code fix is offered for such a field and passes the union declaration as context (FMT-PIPE-029); RH7101-002 names the union among the types whose fields the formatter splits.

The formatter's blank-line, spacing, and indentation phases already treat a union as a type declaration; once its braces and members are on their own lines, its members are indented one level inside its braces, as a class's are.

## Why

The formatter handles type declarations through visitors and kind lists that name class, struct, interface, and record, so a union declaration, which Roslyn 5.12 parses under C# 15, misses them: its braces and members stay on one line, a member that gets a block body leaves its braces on that line, its constraint keeps a missing space (`)where`), its empty body stays braced, and its combined fields stay combined. The change that made C# 15 the CLI's default accepted that interim layout until this change specifies union layout ([roslyn-512-csharp-15.md](roslyn-512-csharp-15.md), *The interim union layout is accepted until #1762*). A union is a type declaration in every respect these rules decide on, so it is laid out as one.

## Visible output changes

All outputs are the CLI's, which parses as C# 15 by default.

**1. Braces and members of a union start their own lines.**

```csharp
// input
public union U1(int, string) { public int X => 1; public int Y => 2; }
public union U4(int, string) : System.IComparable { public int CompareTo(object o) => 0; }
```

```csharp
// before
public union U1(int, string) { public int X => 1; public int Y => 2; }
public union U4(int, string) : System.IComparable { public int CompareTo(object o)
    {
        return 0;
    } }
```

```csharp
// after
public union U1(int, string)
{
    public int X => 1;
    public int Y => 2;
}
public union U4(int, string) : System.IComparable
{
    public int CompareTo(object o)
    {
        return 0;
    }
}
```

**2. The `(` of the case-type list joins the union's name.**

```csharp
// input
public union U2
    (int, string);
```

```csharp
// before
public union U2
(int, string);
```

```csharp
// after
public union U2(int, string);
```

**3. A wrapped case-type list keeps the primary-constructor layout** — no change; listed because the issue asks it as an output question.

```csharp
// input
internal union Nested(int,
    string,
    long);
```

```csharp
// before and after
internal union Nested(int,
                      string,
                      long);
```

**4. Each `where` clause of a union starts its own line.**

```csharp
// input
public union U6<T>(T, string)where T:class;
public union U8<T, S>(T, S) where T : class where S : struct { public bool IsOk => true; }
```

```csharp
// before
public union U6<T>(T, string)where T : class;
public union U8<T, S>(T, S) where T : class where S : struct { public bool IsOk => true; }
```

```csharp
// after
public union U6<T>(T, string)
    where T : class;
public union U8<T, S>(T, S)
    where T : class
    where S : struct
{
    public bool IsOk => true;
}
```

**5. An empty union body becomes a semicolon.**

```csharp
// input
public union U2(int, string) { }
public union U3(int, string)
{
}
```

```csharp
// before
public union U2(int, string) { }
public union U3(int, string)
{
}
```

```csharp
// after
public union U2(int, string);
public union U3(int, string);
```

A body that holds a comment keeps its braces (FMT-STRUCT-018); the braces then start their own lines (change 1):

```csharp
// input
public union U9(int, string) { /* reserved */ }
```

```csharp
// before
public union U9(int, string) { /* reserved */ }
```

```csharp
// after
public union U9(int, string)
{ /* reserved */
}
```

This is the output `public class C9(int a) { /* reserved */ }` gets today (FMT-BREAK-002, FMT-BREAK-003).

**6. A combined field in a union is split, by the formatter and by the RH7101 code fix.**

```csharp
// input
public union U7(int, string)
{
    public static int A = 1, B = 2;
}
```

```csharp
// before
public union U7(int, string)
{
    public static int A = 1, B = 2;
}
```

```csharp
// after
public union U7(int, string)
{
    public static int A = 1;
    public static int B = 2;
}
```

RH7101 reports `B` in this input before and after the change. Before, no code fix was offered; after, the fix produces the output above.

**7. The RH5404 code fix lays out a union inside the type it formats.**

The fix formats the element it reports through the formatter, so a union nested in that element gets the layout of change 1. This is a consequence of change 1, not a decision of its own.

```csharp
// input
class C { union U(int, string) { public int X => 1; } }
```

```csharp
// before
class C
{
    union U(int, string) { public int X => 1; }
}
```

```csharp
// after
class C
{
    union U(int, string)
    {
        public int X => 1;
    }
}
```

## Decisions

### A union is a type declaration for every layout and shape rule that names types

The requirements that name type declarations in general — braces (FMT-BREAK-002), members (FMT-BREAK-007), constraints (FMT-BREAK-023) — now name the union explicitly, and the requirements that list kinds — the empty body (FMT-STRUCT-017), the field split (FMT-STRUCT-021, FMT-PIPE-031), and the parameter lists (FMT-BREAK-011) — add it to their lists. No new requirement states the union's layout on its own, so a later rule about type declarations is read for unions too.

### The empty-body semicolon is gated by C# 15

A union exists only from C# 15, and its semicolon form exists with it. The gate keeps FMT-STRUCT-017's shape of one version per kind; it never refuses a union the formatter sees, because an earlier version parses a union as a syntax error and the formatter leaves such a file alone (FMT-PIPE-005).

### A wrapped case-type list keeps the primary-constructor layout

Maintainer answer (change 3): the wrapped list keeps one case type per line, aligned under the first, as a primary constructor's parameter list does (FMT-BREAK-011, FMT-BREAK-012, FMT-INDENT-012). It is not joined onto one line like a type-argument list.

### An empty union body becomes a semicolon

Maintainer answer (change 5): `{ }` and `{⏎}` become `;` (FMT-STRUCT-017); a body that holds a comment or a directive keeps its braces (FMT-STRUCT-018).

### A union gets the layout and rewrites of a class, and the code fixes follow

Maintainer answer (changes 1, 2, 4, and 6): braces and members on their own lines, the `(` of the case-type list joined to the name, each `where` clause on its own line, and the split of a combined static field, with the RH7101 code fix offered for it. The maintainer accepted with this answer that the code fixes which format through the formatter produce the same layout for a union they format, such as the RH5404 fix for a union nested in the type it lays out (change 7).

## Alternatives considered

- **Join a wrapped case-type list onto one line, as a type-argument list is joined (FMT-BREAK-014)** — not recommended: a union can list many case types, and its list already behaves as a parameter list in every other respect (FMT-BREAK-011, FMT-BREAK-012); the primary-constructor layout is the one the formatter produces today.
- **Keep the braces of an empty union body** — not recommended: every other type declaration with a semicolon form loses its empty braces, and a union with no members is the common case, `union Pet(Cat, Dog);`.
- **Leave union layout to the analyzer work of the C# 15 issues** — rejected: the formatter is the surface the CLI runs, and the analyzer rules for unions (thoenissen/Reihitsu#1766, thoenissen/Reihitsu#1767) do not decide its output.

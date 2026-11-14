# A list is multi-line when its elements are, and its commas end their lines

| | |
|---|---|
| Status | Draft |
| Date | 2026-11 |
| Affected requirements | FMT-BREAK-012 (changed), FMT-BREAK-011 (changed) |

## What

These rules hold for argument lists, parameter lists (the bracketed parameter list of an indexer included), and attribute argument lists.

- **The elements decide whether a list is multi-line** (FMT-BREAK-012). A list is multi-line when two of its elements start on different lines, or when an element spans several lines (FMT-BREAK-013). A line break in front of a comma separates the elements on either side of it, so it makes the list multi-line. A list whose elements all start on one line is single-line, also when a line break stands behind its opening parenthesis or bracket; its first element joins the opening line and the list stays on one line. Until now the requirement also counted a line break behind the opening parenthesis or bracket as a wrap.
- **Commas end their lines in argument and attribute argument lists** (FMT-BREAK-012). In a multi-line argument list and attribute argument list, a comma written at the start of an element's line moves behind the element in front of it, and every element gets its own line, as in a parameter list (FMT-BREAK-011). Until now the comma's position in an argument list was a non-goal, and a list wrapped only in front of commas was kept partly wrapped.
- **A comment or directive in front of a comma on a later line keeps the list as written** (FMT-BREAK-012, FMT-BREAK-011). When a comment, a directive, or disabled text stands between an element and a comma on a later line, the phase adds no line break between the list's elements and moves none of its commas. A comment in front of a comma on the element's own line does not keep a list as written.

The non-goal of `line-breaks.md` on argument lists keeps only the closing parenthesis; it stays tracked in thoenissen/Reihitsu#956.

## Why

thoenissen/Reihitsu#1635 observed that `Foo(1⏎, 2, 3);` stays partly wrapped although FMT-BREAK-012 requires one argument per line, and that the RH5102 code fix, which delegates to the formatter, is offered and changes nothing. The spec did not say where the commas of such a list go. The maintainer decided:

- `Foo(1⏎, 2, 3)` and `Foo(1⏎, 2,⏎3)` become one argument per line with the commas at the end of the lines, in argument lists and attribute argument lists alike.
- A list is multi-line when its elements are on different lines and single-line when all elements are on the same line. A list wrapped only behind its opening parenthesis, such as `M(⏎a, b)`, is therefore single-line, and today's join to `M(a, b)` is the intended output.

## Visible output changes

Each entry shows the formatter's output on `main` (`scripts/format.sh` at 62e50b5, which has no production change against f8239ce) and the output after this change. Entries 1 to 5 are the maintainer's answers for #1635; entries 6 and 7 follow from them for the other list kinds.

**1. An argument list wrapped in front of a comma**

```csharp
// input
Foo(1
    , 2, 3);
```

```csharp
// before: unchanged
Foo(1
    , 2, 3);
```

```csharp
// after
Foo(1,
    2,
    3);
```

**2. A leading comma in a list that already has one argument per line**

```csharp
// input
Foo(1
    , 2,
    3);
```

```csharp
// before: the comma is split off onto its own line
Foo(1
    ,
    2,
    3);
```

```csharp
// after
Foo(1,
    2,
    3);
```

**3. A two-argument list with a leading comma**

```csharp
// input
Foo(a
    , b);
```

```csharp
// before: unchanged
Foo(a
    , b);
```

```csharp
// after
Foo(a,
    b);
```

**4. An attribute argument list wrapped in front of a comma**

```csharp
// input
[AttributeUsage(AttributeTargets.All
    , AllowMultiple = true, Inherited = false)]
```

```csharp
// before: realigned only
[AttributeUsage(AttributeTargets.All
                , AllowMultiple = true, Inherited = false)]
```

```csharp
// after
[AttributeUsage(AttributeTargets.All,
                AllowMultiple = true,
                Inherited = false)]
```

**5. A nested argument list wrapped in front of a comma**

```csharp
// input
Foo(Bar(1
        , 2), 3);
```

```csharp
// before
Foo(Bar(1
        , 2),
    3);
```

```csharp
// after
Foo(Bar(1,
        2),
    3);
```

**6. An indexer's parameter list wrapped only behind its opening bracket**

```csharp
// input
class C
{
    int this[
        int a, int b] => a;
}
```

```csharp
// before
class C
{
    int this[int a,
             int b] => a;
}
```

```csharp
// after
class C
{
    int this[int a, int b] => a;
}
```

Argument, attribute argument, and parenthesized parameter lists wrapped only behind their opening parenthesis are joined onto one line on `main` already (`Foo(⏎a, b)` → `Foo(a, b)`), and keep that output.

**7. A parameter list with a comma held at the start of a line by a comment**

```csharp
// input
class C
{
    void N(int a
           , int b // keep
           , int c)
    {
    }
}
```

```csharp
// before: the first comma moves, the second is split off onto its own line
class C
{
    void N(int a,
           int b // keep
           ,
           int c)
    {
    }
}
```

```csharp
// after: unchanged
class C
{
    void N(int a
           , int b // keep
           , int c)
    {
    }
}
```

The same list kinds with a comma held only by a comment or a directive and no other leading comma, such as `Foo(1⏎, 2 // keep⏎, 3)` or `void O(int a, int b // keep⏎, int c)`, are unchanged on `main` and stay unchanged.

**8. The RH5102 code fix (RH5102 · Arguments should either all be on one line or each on its own line) on entry 1**

```csharp
// input
Foo{|RH5102:(1
    , 2, 3)|};
```

```csharp
// before: the fix is offered and changes nothing
Foo(1
    , 2, 3);
```

```csharp
// after
Foo(1,
    2,
    3);
```

The fix delegates to the formatter (RH5102-003), so its result follows entry 1. No diagnostic and no rule page changes: RH5101, RH5102, RH5107, RH5109, and RH6002 report the same inputs as before.

## Decisions

### Elements decide, not line breaks at the delimiters

The maintainer's definition: a list is multi-line when its elements are on different lines. A line break only behind the opening delimiter puts no element on a line of its own, so the list is joined as a one-line list. This replaces the earlier wording "wrapped anywhere — after its opening parenthesis or bracket or between two elements", which the argument, attribute argument, and parenthesized parameter lists never implemented and the indexer list did (entry 6). One definition now holds for every list kind FMT-BREAK-012 covers.

### Trailing commas in argument and attribute argument lists

The maintainer chose commas at the end of the lines (option C of the run's question) over keeping the author's leading commas. This applies the parameter-list layout of FMT-BREAK-011 to the commas of multi-line argument lists now; the closing parenthesis of an argument list stays a non-goal, tracked in thoenissen/Reihitsu#956.

### A comma held by a comment or directive keeps the list as written

Moving such a comma would cross the comment or directive, which FMT-BREAK-001 forbids. Laying out the rest of the list around a comma that cannot move leaves a mix of leading and trailing commas, or a comma alone on its line (entry 7, before). The class is rare — RH5035 and RH5038 report a comment inside a parameter or argument list — so the list is left as written, decided by a cheap check of the trivia in front of each comma (`AGENTS.md`, *Special cases do not grow the source*).

## Alternatives considered

- **Keep the author's leading commas and put each element on its own line** (`Foo(1⏎, 2⏎, 3)`) — rejected by the maintainer in favor of trailing commas.
- **Split only behind the existing commas** (`Foo(1⏎, 2,⏎3)`) — rejected by the maintainer; it mixes comma styles in one list.
- **Keep counting a line break behind the opening delimiter as a wrap** (`M(⏎a, b)` → `M(a,⏎b)`) — rejected by the maintainer's definition of a multi-line list.
- **Move every comma that can move and leave only the held one in place** (entry 7 → `int a,⏎int b // keep⏎, int c`) — rejected: it needs the layout to work around a comma it cannot move for a class that is rare by definition, and still mixes comma styles.
- **Declare a line break in front of a comma in an argument list unsupported** — rejected: it would delete no special case, and the maintainer kept the shape a bug to fix.

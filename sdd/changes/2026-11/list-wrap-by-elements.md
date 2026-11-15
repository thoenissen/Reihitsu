# A list is multi-line when its elements are, and its commas end their lines

| | |
|---|---|
| Status | Accepted |
| Date | 2026-11 |
| Affected requirements | FMT-BREAK-012 (changed), FMT-BREAK-011 (changed) |

## What

These rules hold for argument lists, parameter lists (the bracketed parameter list of an indexer included), and attribute argument lists.

- **The elements decide whether a list is multi-line** (FMT-BREAK-012). A list is multi-line when two of its elements start on different lines, or when an element spans several lines (FMT-BREAK-013). A line break in front of a comma separates the elements on either side of it, so it makes the list multi-line. A list whose elements all start on one line is single-line, also when a line break stands behind its opening parenthesis or bracket; its first element joins the opening line and the list stays on one line. A first element that a comment, a directive, or disabled text keeps off the opening line cannot join it, so it starts its own line and the list is multi-line, one element per line (`Foo( // c⏎1, 2)` → `Foo( // c⏎1,⏎2)`), as on `main`. Until now the requirement also counted a line break behind the opening parenthesis or bracket as a wrap.
- **Commas end their lines in argument and attribute argument lists** (FMT-BREAK-012). In a multi-line argument list and attribute argument list, a comma written at the start of an element's line moves behind the element in front of it, and every element gets its own line, as in a parameter list (FMT-BREAK-011). Until now the comma's position in an argument list was a non-goal, and a list wrapped only in front of commas was kept partly wrapped.
- **A comment behind a moved comma goes with it** (FMT-BREAK-012). A comment on the line of a leading comma, behind it, stays behind the comma when the comma moves, as it does in a parameter list.
- **A comma held by a comment or directive stays, and only that gap is left as written** (FMT-BREAK-012, FMT-BREAK-011). A comma that a comment, a directive, or disabled text keeps off the line of the element in front of it stays at the start of the line of the element behind it; when it stands alone on its line, the line break behind it stays too, so it is not joined to the element behind it (`int a // keep⏎,⏎int b` is unchanged). Every other comma and element of the list is laid out as above, as FMT-BREAK-037 does for the commas of a constraint clause. Line breaks are counted as SHARED-BLANK-008 counts them, so a line break inside the text of a multi-line comment keeps no comma off its line.

The non-goal of `line-breaks.md` on argument lists keeps only the closing parenthesis; it stays tracked in thoenissen/Reihitsu#956.

## Why

thoenissen/Reihitsu#1635 observed that `Foo(1⏎, 2, 3);` stays partly wrapped although FMT-BREAK-012 requires one argument per line, and that the RH5102 code fix, which delegates to the formatter, is offered and changes nothing. The spec did not say where the commas of such a list go. The maintainer decided:

- `Foo(1⏎, 2, 3)` and `Foo(1⏎, 2,⏎3)` become one argument per line with the commas at the end of the lines, in argument lists and attribute argument lists alike.
- A list is multi-line when its elements are on different lines and single-line when all elements are on the same line. A list wrapped only behind its opening parenthesis, such as `M(⏎a, b)`, is therefore single-line, and today's join to `M(a, b)` is the intended output.

## Visible output changes

Each entry shows the formatter's output on `main` (`scripts/format.sh` at 62e50b5, which has no production change against f8239ce) and the output after this change. Entries 1 to 5 are the maintainer's answers for #1635; entries 6 to 11 follow from them for the other list kinds, for commas held or accompanied by a comment or directive, and for the code fixes that delegate to the formatter. The "after" outputs are the requirements' outputs and what the formatter produces with this change.

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

**7. A comma held at the start of a line by a comment, next to commas that can move**

Only the held comma stays; every other comma and element is laid out as in entries 1 to 3.

```csharp
// input
class C
{
    void N(int a
           , int b // keep
           , int c)
    {
    }

    void O(int a, int b // keep
           , int c)
    {
    }

    void M()
    {
        Foo(1
            , 2 // keep
            , 3);
        Foo(1, 2
            , 3 // keep
            , 4);
        Foo(1, 2
            , 3, 4 // keep
            , 5);
    }
}
```

```csharp
// before: N moves its first comma and splits the held one off onto its own line; O and the argument lists are unchanged
class C
{
    void N(int a,
           int b // keep
           ,
           int c)
    {
    }

    void O(int a, int b // keep
           , int c)
    {
    }

    void M()
    {
        Foo(1
            , 2 // keep
            , 3);
        Foo(1, 2
            , 3 // keep
            , 4);
        Foo(1, 2
            , 3, 4 // keep
            , 5);
    }
}
```

```csharp
// after
class C
{
    void N(int a,
           int b // keep
           , int c)
    {
    }

    void O(int a,
           int b // keep
           , int c)
    {
    }

    void M()
    {
        Foo(1,
            2 // keep
            , 3);
        Foo(1,
            2,
            3 // keep
            , 4);
        Foo(1,
            2,
            3,
            4 // keep
            , 5);
    }
}
```

**8. A comma held at the start of a line by a directive**

Shown in a build that defines `X`, where `, 3` is code and its comma is a separator of the list. In a build that does not define `X`, `, 3` is disabled text, the list holds only `1` and `2` on one line, and the input stays unchanged, before and after.

```csharp
// input
Foo(1, 2
#if X
    , 3
#endif
    );
```

```csharp
// before: unchanged
Foo(1, 2
#if X
    , 3
#endif
    );
```

```csharp
// after
Foo(1,
    2
#if X
    , 3
#endif
    );
```

**9. A comment behind a leading comma that moves**

The comment goes with its comma, as it already does in a parameter list (`void P(int a⏎, /* c */ int b)` gives `void P(int a, /* c */⏎int b)` on `main`).

```csharp
// input
Foo(1
    , /* c */ 2, 3);
Foo(1
    , // c
    2, 3);
```

```csharp
// before: the first list is unchanged; the second gets a line break behind 2 only
Foo(1
    , /* c */ 2, 3);
Foo(1
    , // c
    2,
    3);
```

```csharp
// after
Foo(1, /* c */
    2,
    3);
Foo(1, // c
    2,
    3);
```

**10. The RH5102 code fix (RH5102 · Arguments should either all be on one line or each on its own line) on entry 1, and on a list also wrapped behind its opening parenthesis**

```csharp
// input: RH5102 reports both lists
Foo(1
    , 2, 3);
Foo(
    1
    , 2, 3);
```

```csharp
// before: the fix is offered and changes nothing on either list
Foo(1
    , 2, 3);
Foo(
    1
    , 2, 3);
```

```csharp
// after
Foo(1,
    2,
    3);
Foo(1,
    2,
    3);
```

**11. The RH5101 code fix (RH5101 · The first argument should be on the same line as the opening parenthesis) on a list wrapped behind its opening parenthesis and in front of a comma**

```csharp
// input: RH5101 reports the first argument
Foo(
    1
    , 2, 3);
```

```csharp
// before
Foo(1
    , 2, 3);
```

```csharp
// after
Foo(1,
    2,
    3);
```

Both fixes delegate to the formatter (RH5101-003, RH5102-003), so their results follow entries 1 to 9. No diagnostic and no rule page changes: RH5101, RH5102, RH5107, RH5109, and RH6002 report the same inputs as before. A held comma left at the start of its line leaves no RH5102 report: in entries 7 and 8 no two arguments share a line after the change.

## Decisions

### Elements decide, not line breaks at the delimiters

The maintainer's definition: a list is multi-line when its elements are on different lines. A line break only behind the opening delimiter puts no element on a line of its own, so the list is joined as a one-line list. This replaces the earlier wording "wrapped anywhere — after its opening parenthesis or bracket or between two elements", which the argument, attribute argument, and parenthesized parameter lists never implemented and the indexer list did (entry 6). One definition now holds for every list kind FMT-BREAK-012 covers. Lines are counted as SHARED-BLANK-008 counts them, both for whether a list is multi-line and for whether a comma is kept off its element's line, so a line break inside the text of a multi-line comment counts for neither.

A first element held off the opening line by a comment, a directive, or disabled text is the one exception to "single-line when all elements start on one line": the join behind the opening delimiter would cross what holds it (FMT-BREAK-001), so the element starts its own line and the list is laid out as a multi-line list, one element per line. This is the output on `main`, so it is not a visible output change. Keeping the other elements on the held element's line instead would leave a list that RH5102 reports and whose fix it withholds (SHARED-DIAG-010), while the one-element-per-line output is not reported. The class is rare: RH5035 and RH5038 report the comment.

### Trailing commas in argument and attribute argument lists

The maintainer chose commas at the end of the lines (option C of the run's question) over keeping the author's leading commas. This applies the parameter-list layout of FMT-BREAK-011 to the commas of multi-line argument lists now; the closing parenthesis of an argument list stays a non-goal, tracked in thoenissen/Reihitsu#956.

### A comment behind a moved comma goes with it

A comment on the comma's line behind a leading comma stays behind the comma when the comma moves (entry 9). Parameter lists already do this, and FMT-BREAK-037 keeps a comment behind a comma of a constraint clause; one layout for the family follows FMT-PIPE-014.

### Only the held gap is left as written

A comma that a comment, a directive, or disabled text keeps off the line of the element in front of it cannot move without crossing it (FMT-BREAK-001), so it stays at the start of the line of the element behind it — or alone on its line, with the line break behind it as written, when it was written alone there; joining it to the element behind it is a layout around an immovable comma that the rare class does not need — and the rest of the list is laid out as any multi-line list — the per-gap refusal FMT-BREAK-037 already makes for the commas of a constraint clause. The class is rare: RH5035 and RH5038 report a comment inside a parameter or argument list. The check is the trivia in front of each comma, a cheap syntactic check (`AGENTS.md`, *Special cases do not grow the source*).

## Alternatives considered

- **Keep the author's leading commas and put each element on its own line** (`Foo(1⏎, 2⏎, 3)`) — rejected by the maintainer in favor of trailing commas.
- **Split only behind the existing commas** (`Foo(1⏎, 2,⏎3)`) — rejected by the maintainer; it mixes comma styles in one list.
- **Keep the elements behind a held first element on its line** (`Foo( // c⏎1, 2)` unchanged) — rejected: RH5102 reports that list and withholds its fix (SHARED-DIAG-010), and it would change `main`'s output for a rare class.
- **Keep counting a line break behind the opening delimiter as a wrap** (`M(⏎a, b)` → `M(a,⏎b)`) — rejected by the maintainer's definition of a multi-line list.
- **Leave the whole list as written when one comma is held** (entry 7 → unchanged) — rejected: the formatter would then keep a list that RH5102 reports and whose fix it withholds, against SHARED-DIAG-010; in a parameter list it would give up the comma `main` already moves, leaving a report of RH5107 in the output; and it would refuse differently from the commas of a constraint clause (FMT-BREAK-037).
- **Leave a comment behind a moved comma in front of the element behind it** (`Foo(1,⏎/* c */ 2,⏎3)`) — rejected: it would lay out argument lists differently from parameter lists and constraint clauses.
- **Declare a line break in front of a comma in an argument list, or a comment between an element and the comma behind it, unsupported** — rejected: neither would delete a special case, since FMT-BREAK-001's guard serves every join, and the maintainer kept the shape a bug to fix.

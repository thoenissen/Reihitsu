# An argument list written below its name joins the name's line

| | |
|---|---|
| Status | Accepted |
| Date | 2026-11 |
| Affected requirements | FMT-BREAK-039 (new), FMT-BREAK-031, FMT-BREAK-020, FMT-BREAK-015 |

## What

The line-break phase joins the opening parenthesis of an argument list onto the line of the token in front of it when a line break separates the two (FMT-BREAK-039): the argument list of an invocation, of an object creation and an implicit object creation, of a constructor initializer, of a primary-constructor base type, and of the `with(…)` element of a collection expression, wherever the argument list stands. A comment, a directive, or disabled text in the gap blocks the join (FMT-BREAK-001), and blank lines in the gap go with it (FMT-BREAK-030). A collection expression, a complex element initializer, or another construct of FMT-BREAK-022 that was written over several lines only because of such a line break still counts as spanning several lines and keeps one element per line, as it does today when FMT-BREAK-020 rejoins a split inside one of its elements. A list (FMT-BREAK-012) and a conditional expression (FMT-BREAK-018) are judged as the line they become, as a list is today after the join of FMT-BREAK-015 and a conditional expression after the joins of FMT-BREAK-015 and FMT-BREAK-020. A list is judged so also where the argument list stands inside an interpolation hole, a query clause after the first, or an array or `stackalloc` initializer of one of its elements, which FMT-BREAK-015 does not join into (change 7).

FMT-BREAK-039 is the one requirement that owns the gap in front of an argument list's `(`: FMT-BREAK-020, which joined it in a chain, and FMT-BREAK-015, which joined it inside a list element, now point to FMT-BREAK-039 instead of stating the join again. FMT-BREAK-015's output does not change. FMT-BREAK-020's output changes in one case: today it joins an argument list in a chain only after the list around it is laid out, so a list whose only line break was such a gap inside an interpolation hole, a query clause, or an array or `stackalloc` initializer of an element gets one argument per line; it now becomes one line (change 7). FMT-BREAK-031 lists the new join among the joins that write the minimal separator; it changes no output of its own.

## Why

Today the phase joins such a parenthesis only in a chain with at least one link (FMT-BREAK-020) and inside an element of an argument, parameter, attribute argument, tuple, or type-argument list (FMT-BREAK-015). Everywhere else the line break stays, and the indentation phase gives the line that starts with `(` the column of an unowned continuation line: in whole-document formatting the enclosing statement's or declaration's column, in a code fix the column the fix's formatted node lays it out from. The argument list then reads as a parenthesized expression on a line of its own, and for one input the CLI and a code fix put it in different columns:

```csharp
// input
List<int> a = [with
(4), 1, 2];
```

```csharp
// CLI (main)
List<int> a = [
                  with
(4),
                  1,
                  2
              ];
```

```csharp
// RH5305 code fix (main)
List<int> a = [
                  with
              (4),
                  1,
                  2
              ];
```

(The input is shown at the statement's column; the CLI puts `(4)` at the column of `List`, the fix at the column of `[`.) Joining the parenthesis removes the line, so neither column is needed.

## Visible output changes

Every *before* below is the output of `main`; every *after* was produced by formatting the input with the line break in front of `(` removed by hand, so the parts this change leaves alone are today's output.

**1. The argument list of an invocation and of an object creation joins the line in front of it.**

```csharp
// input
void M(Func<int, int> f, Func<int, int> g)
{
    Foo
    (4);

    var l = new List<int>
    (4);
    List<int> k = new
    (4);
    var r = (f ?? g)
    (1);
    if (Has
        (1))
    {
    }
}
```

```csharp
// before
void M(Func<int, int> f, Func<int, int> g)
{
    Foo
    (4);

    var l = new List<int>
    (4);
    List<int> k = new
    (4);
    var r = (f ?? g)
    (1);

    if (Has
    (1))
    {
    }
}
```

```csharp
// after
void M(Func<int, int> f, Func<int, int> g)
{
    Foo(4);

    var l = new List<int>(4);
    List<int> k = new(4);
    var r = (f ?? g)(1);

    if (Has(1))
    {
    }
}
```

**2. The argument list of a `with(…)` element and of an element of a collection expression or a complex element initializer joins its name; the collection expression and the initializer keep one element per line.**

```csharp
// input
HashSet<int> h = [with
(4), 1, 2];
int[] a = [Bar
(4)];
var e = new Dictionary<int, int> { { Bar
    (1), 2 } };
```

```csharp
// before
HashSet<int> h = [
                     with
(4),
                     1,
                     2
                 ];
int[] a = [
              Bar
(4)
          ];
var e = new Dictionary<int, int>
        {
            {
                Bar
(1),
                2
            }
        };
```

```csharp
// after
HashSet<int> h = [
                     with(4),
                     1,
                     2
                 ];
int[] a = [
              Bar(4)
          ];
var e = new Dictionary<int, int>
        {
            {
                Bar(1),
                2
            }
        };
```

**3. A conditional expression that spanned several lines only because of such a line break becomes one line.**

```csharp
// input
var x = a ? Get
(1) : 2;
```

```csharp
// before
var x = a
            ? Get
(1)
            : 2;
```

```csharp
// after
var x = a ? Get(1) : 2;
```

A list that holds such a line break directly in an element (`Baz(Get⏎(1), 2)`) already becomes one line on `main` (FMT-BREAK-015); that output does not change. A list that holds it inside a construct nested in an element is change 7.

**4. The argument list of a constructor initializer and of a primary-constructor base type joins its keyword or base type.**

```csharp
// input
class D : B
{
    public D()
        : base
        (1)
    {
    }
}

class E() : B
    (1);
```

```csharp
// before
class D : B
{
    public D()
        : base
    (1)
    {
    }
}

class E() : B
            (1);
```

```csharp
// after
class D : B
{
    public D()
        : base(1)
    {
    }
}

class E() : B(1);
```

**5. Blank lines between the name and the argument list go with the join.**

```csharp
// input
Get

(4);
```

```csharp
// before
Get

(4);
```

```csharp
// after
Get(4);
```

**6. Code fixes that format a node holding such an argument list write the join too.**

The code fixes format their node through the formatter, so the join reaches every fix whose formatted node holds such an argument list other than as the node's first token: among them the fixes of RH3004, RH3208, RH5111, RH5205, RH5206, RH5301, RH5302, RH5303, RH5304, RH5305, RH5308, RH5401, RH5403, and RH7101, whose formatted nodes are expressions, statements, initializers, or declarations that can hold an invocation or an object creation. The fixes of RH5101 and RH5102 format the argument list itself, whose first token is the `(`; that token keeps its line and column (FMT-PIPE-017), so their results do not change. This is a consequence of changes 1 to 5, not a decision of its own. Checked with `scripts/apply-fix.sh` on `main`:

```csharp
// input
List<int> a = [with
(4), 1, 2];                                  // RH5305
var l = new List<int> { Get
(4), 2 };                                    // RH5303
var p = new P { X = Get
(4) };                                       // RH5301
return a &&
    Has
    (1);                                     // RH5302
return a ? Get
(1)
    : 2;                                     // RH5308
```

```csharp
// before
List<int> a = [
                  with
              (4),
                  1,
                  2
              ];
var l = new List<int>
        {
            Get
        (4),
            2
        };
var p = new P
        {
            X = Get
        (4)
        };
return a
       && Has
       (1);
return a
           ? Get
       (1)
           : 2;
```

```csharp
// after
List<int> a = [
                  with(4),
                  1,
                  2
              ];
var l = new List<int>
        {
            Get(4),
            2
        };
var p = new P
        {
            X = Get(4)
        };
return a
       && Has(1);
return a
           ? Get(1)
           : 2;
```

**7. A list whose only line break was such a gap inside an interpolation hole, a query clause, or an array initializer of an element becomes one line, in a chain or not.**

FMT-BREAK-015 does not join lines inside these constructs. On `main` the gap was joined there only in a chain, by FMT-BREAK-020, after the list was laid out, and outside a chain not at all; either way the list got one argument per line. The same holds for a `stackalloc` initializer.

```csharp
// input
void M(object x, List<int> list)
{
    Baz($"{x.ToString
    ()}", 2);
    Baz($"{Bar
    (1)}", 2);
    Baz(from v in list select Bar
    (v), 2);
    Baz(new[] { x.GetHashCode
    () }, 2);
}
```

```csharp
// before
void M(object x, List<int> list)
{
    Baz($"{x.ToString()}",
        2);
    Baz($"{Bar
    (1)}",
        2);
    Baz(from v in list select Bar
        (v),
        2);
    Baz(new[] { x.GetHashCode() },
        2);
}
```

```csharp
// after
void M(object x, List<int> list)
{
    Baz($"{x.ToString()}", 2);
    Baz($"{Bar(1)}", 2);
    Baz(from v in list select Bar(v), 2);
    Baz(new[] { x.GetHashCode() }, 2);
}
```

As in change 6, every code fix whose formatted node holds such a list writes the new layout. Checked with `scripts/apply-fix.sh` on `main` and on the branch:

```csharp
// input
return a &&
    Has($"{x.ToString
    ()}", 2);                                // RH5302
```

```csharp
// before
return a
       && Has($"{x.ToString()}",
              2);
```

```csharp
// after
return a
       && Has($"{x.ToString()}", 2);
```

No rule page changes: no analyzer rule reports an argument list below its name, and what RH5305 and the other rules report does not change.

## Decisions

### The maintainer chose the join, with collection-like constructs judged as written

The maintainer answered the output questions as follows. Change 6 follows from them.

- **Argument list below its name (changes 1, 2, and 5): join.** `Foo⏎(4)`, `new List<int>⏎(4)`, `(f ?? g)⏎(1)`, and `with⏎(4)` join the line in front of them; a collection expression written over several lines keeps one element per line (`[⏎with(4),⏎1,⏎2⏎]`), rather than becoming one line or keeping today's output.
- **Constructor initializer and primary-constructor base type (change 4): join.** `: base⏎(1)` becomes `: base(1)` and `class E() : B⏎(1);` becomes `class E() : B(1);`, rather than keeping today's output.
- **Conditional expression (change 3): judged after the join.** `a ? Get⏎(1) : 2` becomes `a ? Get(1) : 2`, rather than one branch per line.
- **Dictionary element initializer (change 2): one element per line.** `{ { Get⏎(1), 2 } }` keeps its inner braces over several lines with `Get(1)` and `2` on lines of their own, rather than becoming `{ Get(1), 2 }`.
- **List around an argument list inside an interpolation hole, a query clause, or an array or `stackalloc` initializer (change 7): one line.** `Baz($"{x.ToString⏎()}", 2)` and `Baz($"{Bar⏎(1)}", 2)` become `Baz($"{x.ToString()}", 2)` and `Baz($"{Bar(1)}", 2)`, in a chain or not, rather than one argument per line as on `main`.

### The join is stated by its cause, the gap in front of an argument list's `(`

The requirement names the gap between an argument list's opening parenthesis and the token in front of it, whichever node owns the argument list, instead of listing invocations of names. An invocation of a parenthesized expression or of another invocation (`(f ?? g)⏎(1)`, `Make(1)⏎(2)`) is the same gap and gets the same join. (`(f)⏎(1)` is not one: C# reads it as a cast of `(1)` to the type `f`.)

### The constructs of FMT-BREAK-022 are judged as written, lists and conditional expressions as joined

The join counts for a surrounding construct as the phase's other joins inside it count today. FMT-BREAK-022 decides from the author's line breaks whether a collection expression or a complex element initializer spans several lines, also where a join inside an element removes one of them: `[v.⏎Length, 2]`, `[Get⏎(1).GetHashCode(), 2]`, and `{ { v.Equals⏎(1).GetHashCode(), 2 } }` get one element per line on `main`. So `[with⏎(4), 1, 2]` keeps one element per line rather than becoming `[with(4), 1, 2]`. A list is judged after the join of FMT-BREAK-015 (`Foo(1, Get⏎(4))` → `Foo(1, Get(4))` on `main`) and a conditional expression after the joins of FMT-BREAK-015 and FMT-BREAK-020 (`a ? v.Equals⏎(1).GetHashCode() : 2` → one line on `main`), so `a ? Get⏎(1) : 2` becomes one line too. A list is judged after the join also where the argument list stands inside a construct nested in an element (change 7), which is the maintainer's answer, not a precedent: on `main` a chain's argument list there was joined only after the list was laid out. The precedent differs by construct, and the requirement follows it rather than unifying it.

### One requirement owns the gap in front of an argument list's `(`

FMT-BREAK-039 states the join for every argument list, so FMT-BREAK-020 (in a chain) and FMT-BREAK-015 (inside a list element) no longer state it themselves and point to FMT-BREAK-039. One statement of the join keeps its guards and exceptions from drifting apart, and the implementation has one owner of the gap. The output differs from the three statements it replaces in one case: FMT-BREAK-020 joined an argument list in a chain after the list around it was laid out, so inside an interpolation hole, a query clause, or an array or `stackalloc` initializer of a list element the list kept one argument per line. The one owner joins every such gap before the list is laid out, and the maintainer chose the resulting one-line list (change 7).

## Alternatives considered

- **Keep the line break (today's output)** — not recommended: the argument list stays on a line of its own at a column that shows nothing about where it belongs, and the CLI and the code fixes keep putting it in different columns (Why). Making those columns agree without the join is the indentation of unowned continuation lines, a separate decided change (thoenissen/Reihitsu#953).
- **Join, and let a collection expression that spanned several lines only because of the join become one line** (`[with(4), 1, 2]`, as the issue sketched) — not recommended: a collection expression would then lay out differently after this join than after every other join inside an element, and making those agree would change today's output for chain splits inside elements as well.
- **Judge a conditional expression as written too** (`a ? Get⏎(1) : 2` → one branch per line, `? Get(1)` and `: 2` on lines of their own) — not recommended: a conditional whose only line break was a split inside a branch already becomes one line on `main` after the joins of FMT-BREAK-015 and FMT-BREAK-020, and the new join would lay it out differently from them.
- **Join only invocations, object creations, and `with(…)` elements, and keep the line break in front of a constructor initializer's and a primary-constructor base type's argument list** — not recommended: the gap is the same, and the line `(1)` gets a column today (the constructor's, or the column after `class E() : `) that shows as little as an invocation's does.
- **Keep one argument per line for a list whose only line break was the gap inside an interpolation hole, a query clause, or an array or `stackalloc` initializer** (change 7 as on `main` for a chain's argument list) — not chosen by the maintainer: the list would then depend on whether its element's argument list was written below its name, which the join removes, and outside a chain `main` gave such a list one argument per line only because the gap was never joined.
- **Report the shape with a new analyzer rule** — out of scope: the formatter may do more than the analyzers (P11); a rule for it is a candidate of its own.

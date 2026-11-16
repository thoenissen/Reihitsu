# An argument list written below its name joins the name's line

| | |
|---|---|
| Status | Draft |
| Date | 2026-11 |
| Affected requirements | FMT-BREAK-039 (new), FMT-BREAK-031 |

## What

The line-break phase joins the opening parenthesis of an argument list onto the line of the token in front of it when a line break separates the two (FMT-BREAK-039): the argument list of an invocation, of an object creation and an implicit object creation, of a constructor initializer, of a primary-constructor base type, and of the `with(…)` element of a collection expression. A comment, a directive, or disabled text in the gap blocks the join (FMT-BREAK-001), and blank lines in the gap go with it (FMT-BREAK-030). A collection expression that was written over several lines only because of such a line break still counts as spanning several lines and keeps one element per line (FMT-BREAK-022), as it does today when FMT-BREAK-020 rejoins a split inside one of its elements.

FMT-BREAK-031 lists the new join among the joins that write the minimal separator; it changes no output of its own.

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
void M(Func<int, int> f)
{
    Foo
    (4);

    var l = new List<int>
    (4);
    List<int> k = new
    (4);
    var r = (f)
    (1);
    if (Has
        (1))
    {
    }
}
```

```csharp
// before
void M(Func<int, int> f)
{
    Foo
    (4);

    var l = new List<int>
    (4);
    List<int> k = new
    (4);
    var r = (f)
    (1);

    if (Has
    (1))
    {
    }
}
```

```csharp
// after
void M(Func<int, int> f)
{
    Foo(4);

    var l = new List<int>(4);
    List<int> k = new(4);
    var r = (f)(1);

    if (Has(1))
    {
    }
}
```

**2. The argument list of a `with(…)` element and of an element of a collection expression joins its name; the collection expression keeps one element per line.**

```csharp
// input
HashSet<int> h = [with
(4), 1, 2];
int[] a = [Bar
(4)];
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
```

**3. The argument list of a constructor initializer and of a primary-constructor base type joins its keyword or base type.**

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

**4. Blank lines between the name and the argument list go with the join.**

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

**5. Code fixes that format a node holding such an argument list write the join too.**

The code fixes format their node through the formatter, so the join reaches every fix whose formatted node holds such an argument list other than as the node's first token: among them the fixes of RH3004, RH3208, RH5111, RH5205, RH5206, RH5301, RH5302, RH5303, RH5304, RH5305, RH5308, RH5401, RH5403, and RH7101, whose formatted nodes are expressions, statements, initializers, or declarations that can hold an invocation or an object creation. The fixes of RH5101 and RH5102 format the argument list itself, whose first token is the `(`; that token keeps its line and column (FMT-PIPE-017), so their results do not change. This is a consequence of changes 1 to 4, not a decision of its own. Checked with `scripts/apply-fix.sh` on `main`:

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

No rule page changes: no analyzer rule reports an argument list below its name, and what RH5305 and the other rules report does not change.

## Decisions

### The maintainer's answer is pending

Changes 1 to 5 are open output questions; the answer will be recorded here.

### The join is stated by its cause, the gap in front of an argument list's `(`

The requirement names the gap between an argument list's opening parenthesis and the token in front of it, whichever node owns the argument list, instead of listing invocations of names. An invocation of a parenthesized expression or of another invocation (`(f)⏎(1)`, `Make(1)⏎(2)`) is the same gap and gets the same join.

### A collection expression is judged as written

FMT-BREAK-022 decides from the author's line breaks whether a collection expression spans several lines, also where a join inside an element removes one of them: `[v.⏎Length, 2]` and `[Get⏎(1).GetHashCode(), 2]` get one element per line on `main`. The new join follows that, so `[with⏎(4), 1, 2]` keeps one element per line rather than becoming `[with(4), 1, 2]`.

## Alternatives considered

- **Keep the line break (today's output)** — not recommended: the argument list stays on a line of its own at a column that shows nothing about where it belongs, and the CLI and the code fixes keep putting it in different columns (Why). Making those columns agree without the join is the indentation of unowned continuation lines, a separate decided change (thoenissen/Reihitsu#953).
- **Join, and let a collection expression that spanned several lines only because of the join become one line** (`[with(4), 1, 2]`, as the issue sketched) — not recommended: a collection expression would then lay out differently after this join than after every other join inside an element, and making those agree would change today's output for chain splits inside elements as well.
- **Join only invocations, object creations, and `with(…)` elements, and keep the line break in front of a constructor initializer's and a primary-constructor base type's argument list** — not recommended: the gap is the same, and the line `(1)` gets a column today (the constructor's, or the column after `class E() : `) that shows as little as an invocation's does.
- **Report the shape with a new analyzer rule** — out of scope: the formatter may do more than the analyzers (P11); a rule for it is a candidate of its own.

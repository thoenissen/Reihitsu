# Continuations behind a keyword, `is`, a combinator, a modifier, an arm, a header, a cast, or a lambda join their line, and comments there are unsupported

| | |
|---|---|
| Status | Accepted |
| Date | 2026-11 |
| Affected requirements | FMT-BREAK-041 (new), FMT-UNSUP-017 (new), FMT-BREAK-015, FMT-BREAK-016, FMT-BREAK-017, FMT-BREAK-022, FMT-BREAK-031, FMT-INDENT-034, FMT-INDENT-035, FMT-SPACE-010, FMT-UNSUP-010, FMT-UNSUP-011, FMT-UNSUP-012, FMT-UNSUP-013, FMT-STRUCT-006, FMT-STRUCT-021, RH2001-004, RH2001-007, RH2001-009, RH5401-003, RH5401-004, RH5419-003, RH5419-004, RH5111-004, RH7101-007 |
| Depends on | [continuation-indent.md](continuation-indent.md) |

## What

The line-break phase joins a set of gaps onto one line, removing their line breaks and blank lines (FMT-BREAK-041): between `return`, `yield return`, `throw`, `await`, or `new` and what follows it, also between `await` and the `foreach` or `using` of an asynchronous loop or `using`, and between `new` and the `[` of `new[]`; between `is` and the pattern or the type behind it; between `not` and its pattern; between a switch-expression arm's pattern and its `when` and `=>`; between a type and its base list's `:`; behind every modifier of a declaration — in front of the next modifier, the type, or the keyword or name that starts the declaration, in members, types, delegates, locals, local functions, accessors, lambdas, and anonymous methods; in front of the `when` of a catch clause; behind each `;` of a `for` header; behind the `in` of a `foreach` statement; behind a cast; behind the `=>` of a lambda with an expression body; and in front of `with`. FMT-BREAK-016 joins the `=` of an anonymous-object member and of a `let` clause with the name in front of it and the value behind it, as it does for every other `=`. The pattern behind `is` keeps its own line where the opening delimiter of a multi-line property or parenthesized pattern starts one (FMT-BREAK-022).

A pattern chain is laid out like a chain of binary operators, and more strictly: once a line break stands next to one of its combinators (`and`, `or`), every combinator of the chain starts its own line, its right pattern joins it, and the combinators align under the `is`, or under the first pattern of a `case` label or a switch arm, as FMT-INDENT-019 already states (FMT-BREAK-017).

A comment, a directive, or disabled text in one of these gaps, or between a pattern combinator and its right pattern, is a shape the formatter refuses as a whole (FMT-UNSUP-017): the CLI leaves such a file unchanged and reports it, and every code fix whose formatted node holds the shape is withheld (SHARED-FIX-010). The gap behind the `is` of a type test is refused already (FMT-UNSUP-011), and the gap in front of the `(` of `new()` as well (FMT-UNSUP-014).

This change covers the instances of thoenissen/Reihitsu#953 that the maintainer decided to join rather than indent, and the `return` value of thoenissen/Reihitsu#1852. Lines that cannot be joined keep the continuation indent of [continuation-indent.md](continuation-indent.md).

## Why

The first draft of this run indented every such line one level deeper than the line of its construct (FMT-INDENT-034). The maintainer decided that the lines behind these gaps belong on the line in front of them: a `return` and its value, a pattern and its `is`, an arm and its arrow, a type and its base list, a modifier and its type, a catch clause and its filter, a `for` header, a lambda and its body, an expression and the `with` that copies it, a member name and its `=` read as one construct, and a line break there carries no meaning (batch 1b, answer A). The maintainer extended the joins to the neighbouring gaps the run found — two modifiers, a modifier and `event`, `class`, a constructor's name, `implicit`, or `delegate`, the `in` of a `foreach`, `await foreach` and `await using`, a cast, `new[]`, a switch arm's `when`, both sides of the `=` of an anonymous-object member and of a `let` clause, `not`, `yield return`, and the `is` of a type test (batch 2, answers 1 B, 2 B, and 4 B). A pattern chain wrapped behind its combinators is rewritten so that each combinator starts its line, aligned under the `is` (batch 1c, answer A), and a chain wrapped at one combinator is wrapped at all of them (batch 2, answer 5 B). A comment or a directive in a joined gap would keep the join from happening and leave a line that needs a column of its own; the maintainer decided to refuse those files instead (batch 1c, answer 1d A; batch 2, answers 1 B, 3 A, and 6 B), with one reason per gap (batch 2, answer 9 A).

## Visible output changes

Every *before* below is the output of `main`, produced with `scripts/format.sh` for the formatter and with `scripts/apply-fix.sh` for the code fixes. The CLI's line has the form it prints today for the other unsupported shapes.

### 1. The gaps are joined

Input (also the output of `main`):

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

class B;

public
class D
: B
{
    private static
    readonly int _f = 1;

    public
    event Action E;

    public
    D()
    {
    }

    async Task<int> M(int x, object o, Task<int> t, int[] a, IAsyncEnumerable<int> s)
    {
        var p = o is
        not
        null;
        var r = o switch
                {
                    int i
        when i > 0
        => 2,
                    _ => 0,
                };

        try
        {
        }
        catch (Exception e)
        when (e != null)
        {
        }

        for (var i = 0;
        i < 3;
        i++)
        {
        }

        foreach (var y in
        a)
        {
        }

        await
        foreach (var z in s)
        {
        }

        Func<int> u = () =>
        a.Length;
        var n = new
        List<int>();
        int[] m = new
        [] { 1 };
        var c = (long)
        x;
        var q = await
        t;

        if (x > 0)
        {
            throw
            new Exception();
        }

        return
        x;
    }

    IEnumerable<int> Y(int x)
    {
        yield return
        x;
    }
}

record R
{
    void M(R r)
    {
        var w = r
        with
        {
        };
    }
}
```

After:

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

class B;

public class D : B
{
    private static readonly int _f = 1;

    public event Action E;

    public D()
    {
    }

    async Task<int> M(int x, object o, Task<int> t, int[] a, IAsyncEnumerable<int> s)
    {
        var p = o is not null;
        var r = o switch
                {
                    int i when i > 0 => 2,
                    _ => 0,
                };

        try
        {
        }
        catch (Exception e) when (e != null)
        {
        }

        for (var i = 0; i < 3; i++)
        {
        }

        foreach (var y in a)
        {
        }

        await foreach (var z in s)
        {
        }

        Func<int> u = () => a.Length;
        var n = new List<int>();
        int[] m = new[] { 1 };
        var c = (long)x;
        var q = await t;

        if (x > 0)
        {
            throw new Exception();
        }

        return x;
    }

    IEnumerable<int> Y(int x)
    {
        yield return x;
    }
}

record R
{
    void M(R r)
    {
        var w = r with
                  {
                  };
    }
}
```

The `=` of an anonymous-object member and of a `let` clause joins both sides (FMT-BREAK-016). Input (`main` keeps every line break and moves `= 1`, `2`, `= x`, `y`, and the query's clauses to the column of the statement):

```csharp
using System.Linq;

class C
{
    object M(int[] arr)
    {
        var w = new
                {
                    A
                        = 1,
                    B =
                        2
                };
        var q = from x in arr
                let y
                    = x
                let z =
                    y
                select z;

        return q;
    }
}
```

After:

```csharp
using System.Linq;

class C
{
    object M(int[] arr)
    {
        var w = new
                {
                    A = 1,
                    B = 2
                };
        var q = from x in arr
                let y = x
                let z = y
                select z;

        return q;
    }
}
```

A blank line in a joined gap goes with the join: `return⏎⏎x;` gives `return x;`.

The join removes only the line breaks of the gap; the construct around it then takes the shape it has on `main` when written on one line. Input:

```csharp
using System;

record R;

static class C
{
    public
    class N
    {
    }

    static void M(R r)
    {
        Func<int> g = static
            delegate { return 1; };
        var w = r
            with { };
    }
}
```

Before (`main`): the gaps keep their line breaks; the same rules already shape the type, the block, and the initializer:

```csharp
using System;

record R;

static class C
{
    public
    class N;

    static void M(R r)
    {
        Func<int> g = static
        delegate
        {
            return 1;
        };
        var w = r
        with
        {
        };
    }
}
```

After: the empty type becomes a semicolon declaration (FMT-STRUCT-017), the anonymous method's block spans several lines (FMT-BREAK-002, FMT-BREAK-006), and the initializer behind `with` starts its own line (FMT-BREAK-021):

```csharp
using System;

record R;

static class C
{
    public class N;

    static void M(R r)
    {
        Func<int> g = static delegate
        {
            return 1;
        };
        var w = r with
                  {
                  };
    }
}
```

The opening parenthesis of a multi-line parenthesized pattern keeps its own line below the `is`, as on `main`:

```csharp
        return o is (
            int or long
        );
```

gives, as on `main`, `(` and `);` in the column of `is`; only the content moves (FMT-INDENT-026, [continuation-indent.md](continuation-indent.md)).

### 2. A pattern chain starts each combinator on its line

Input (also the output of `main`):

```csharp
class C
{
    int M(int x)
    {
        var k = x is
        1 or
        2 or
        3;

        switch (x)
        {
            case 1 or
            2:
                break;
        }

        return x switch
               {
                   1 or
                   3 => 4,
                   _ => 0,
               };
    }
}
```

After:

```csharp
class C
{
    int M(int x)
    {
        var k = x is 1
                  or 2
                  or 3;

        switch (x)
        {
            case 1
                 or 2:
                break;
        }

        return x switch
               {
                   1
                   or 3 => 4,
                   _ => 0,
               };
    }
}
```

A chain wrapped at one combinator is wrapped at all of them, and the type behind the `is` of a type test joins the `is`. Input:

```csharp
class C
{
    bool M(int x, object o)
    {
        var k = x is 1 or 2 or
        3;
        var t = o is
        string;

        return k && t;
    }
}
```

Before (`main`):

```csharp
class C
{
    bool M(int x, object o)
    {
        var k = x is 1 or 2 or
        3;
        var t = o
                is string;

        return k && t;
    }
}
```

After:

```csharp
class C
{
    bool M(int x, object o)
    {
        var k = x is 1
                  or 2
                  or 3;
        var t = o is string;

        return k && t;
    }
}
```

A chain with no line break next to a combinator keeps its layout, and so does a combinator next to a multi-line property, list, or parenthesized pattern, which already starts its own line on `main`.

### 3. A comment, a directive, or disabled text in a joined gap refuses the file

Input:

```csharp
class C
{
    int M(int x, object o)
    {
        var p = o is // not empty
            not null;

        return // the count
            x;
    }
}
```

Before (`main`): `not null;` and `x;` move to the column of their statement.

```csharp
class C
{
    int M(int x, object o)
    {
        var p = o is // not empty
        not null;

        return // the count
        x;
    }
}
```

After: the file is unchanged, and the CLI reports

```
error: <file>(5,22): unsupported syntax: a comment or directive between is and its pattern. The file was left unchanged.
```

Input:

```csharp
class B;

class D // derived
    : B
{
}
```

Before (`main`):

```csharp
class B;

class D // derived
: B;
```

After: the file is unchanged, and the CLI reports `error: <file>(3,9): unsupported syntax: a comment or directive in front of the : of a base list. The file was left unchanged.`

Each gap has its reason (FMT-UNSUP-017): `a comment or directive between return, throw, await, or new and what follows it`, `a comment or directive between is and its pattern`, `a comment or directive between and, or, or not and its pattern`, `a comment or directive in front of the when of a switch arm`, `a comment or directive in front of the => of a switch arm`, `a comment or directive in front of the : of a base list`, `a comment or directive behind a modifier`, `a comment or directive in front of the when of a catch clause`, `a comment or directive behind a ; of a for header`, `a comment or directive behind the in of a foreach statement`, `a comment or directive between a cast and its operand`, `a comment or directive behind the => of a lambda`, `a comment or directive in front of with`, `a comment or directive in front of or behind the = of an anonymous-object member`, and `a comment or directive in front of or behind the = of a let clause`. A comment behind a field's or a local's modifier, or behind the modifier of a function-pointer parameter inside a field's or a local's type, keeps the reason it gives on `main` (FMT-UNSUP-004), and so does a directive in a field declaration with several declarators (FMT-UNSUP-010).

Code that held such a comment is formatted on `main` and refused now; the same comment written above the statement or behind its line is formatted. A switch arm with a comment in front of its `=>` (`1 /* c */ => "one"`) and a lambda with a comment behind its `=>` (`() => /* c */ 1`), which the other sides of FMT-UNSUP-013 and FMT-UNSUP-012 showed as formatted, are refused now, and so are `x is 1 or // c⏎2`, `o is not /* c */ null`, `private /* c */ static int M()`, `this /* c */ int x`, `new { A = /* c */ 1 }`, and a comment behind the modifier of a function-pointer parameter in the type of a property or a parameter:

```csharp
unsafe class C
{
    delegate*<ref // keep
              int, void> P => null;
}
```

`main` formats this input unchanged; now the file is unchanged, and the CLI reports `error: <file>(3,19): unsupported syntax: a comment or directive behind a modifier. The file was left unchanged.`

A conditional group of modifiers, an idiom of code built for several targets, refuses the file as well (batch 2, answer 3 A), and so does a conditional group behind `return`. Input, which `main` formats (it turns `=> f;` into a block):

```csharp
using System;

public partial class Foo
#if NET
    : IFormattable
#endif
{
#if NET
    public
#else
    internal
#endif
    static int Count(int x)
    {
        return
#if NET
            x + 1;
#else
            x;
#endif
    }

    public string ToString(string f, IFormatProvider p) => f;
}
```

After: the file is unchanged, and the CLI, which parses without `NET`, reports

```
error: <file>(12,1): unsupported syntax: a comment or directive behind a modifier. The file was left unchanged.
```

The group behind `return` refuses the file in every build as well. The conditional base list refuses it only in a build that defines `NET`, such as a code fix in an IDE that defines it. A conditional group around whole declarations (`#if NET⏎public static int Count(int x) { … }⏎#else⏎internal static int Count(int x) { … }⏎#endif`) is formatted as on `main`.

### 4. Code fixes

Every code fix that lays out a node through the formatter joins these gaps in that node, and is withheld where the node holds a comment, a directive, or disabled text in one of them (SHARED-FIX-010). The ones the specs and the run's probes show:

- **RH3202, RH3205, RH3206, RH3208** (an expression body converted to a block with a `return`): the created `return` joins its value, also across a blank line; [expression-body-join-across-blank-line.md](expression-body-join-across-blank-line.md) shows the result. A body whose expression holds a refused gap, such as `int M() => x is /* c */ 1;`, gets no fix.
- **RH5302** (a chain of `&&` and `||` operators), whose fix formats the chain:

  ```csharp
          var r = o is
              not null &&
              x > 0;
  ```

  Before:

  ```csharp
          var r = o is
                  not null
                  && x > 0;
  ```

  After (whole-document formatting gives the same):

  ```csharp
          var r = o is not null
                  && x > 0;
  ```

- **RH5305** (a collection expression laid out one element per line): a lambda element whose body stood on the next line gets it behind its `=>`.

  ```csharp
          Func<int>[] f = [() =>
              1, () => 2];
  ```

  Before:

  ```csharp
          Func<int>[] f = [
                              () =>
                          1,
                              () => 2
                          ];
  ```

  After:

  ```csharp
          Func<int>[] f = [
                              () => 1,
                              () => 2
                          ];
  ```

- **RH7101** (a field split into one field per declaration): a line break between the last modifier and the type, which the split repeats in every field (FMT-STRUCT-021), is joined.

  ```csharp
      private
          int _a, _b;
  ```

  Before:

  ```csharp
      private
      int _a;
      private
      int _b;
  ```

  After:

  ```csharp
      private int _a;
      private int _b;
  ```

- **RH5419** (an expression-bodied indexer put on one line): a line break behind a modifier, which withheld the fix on `main`, is joined, so the fix is offered; a comment behind a modifier, which the fix kept on `main`, now withholds it, also on one line.

  ```csharp
      public
      virtual int this[int a,
                       int b] => a;

      public /* c */ int this[short a,
                              short b] => a;
  ```

  Before: no fix for the first indexer; the second becomes

  ```csharp
      public /* c */ int this[short a, short b] => a;
  ```

  After: the first becomes

  ```csharp
      public virtual int this[int a, int b] => a;
  ```

  and no fix is offered for the second. The diagnostic is reported for both, before and after.

- **RH5401** (an expression-bodied property put on one line), in the same way:

  ```csharp
      public
      int Q
          => 2;

      public /* c */ int P
          => 1;
  ```

  Before: no fix for `Q`; `P` becomes `public /* c */ int P => 1;`. After: `Q` becomes `public int Q => 2;`, and no fix is offered for `P`. The diagnostic is reported for both, before and after.

- **RH5111** (an assignment, an initializer, or a default value laid out with its `=`), whose fix formats the whole declaration (RH5111-004): a line break behind a modifier of a property is joined, and the property's accessor list then takes its one-line form (FMT-BREAK-024), as whole-document formatting gives it.

  ```csharp
          public
          string Property
          {
              get;
              set;
          }
          = "test";
  ```

  Before:

  ```csharp
          public
          string Property
          {
              get;
              set;
          } = "test";
  ```

  After:

  ```csharp
          public string Property { get; set; } = "test";
  ```

- **RH2001, RH7101** (a private auto-property turned into a field; a field split into one field per declaration), whose fixes ask the formatter before they are offered: a comment, a directive, or disabled text in one of the gaps inside the field the RH2001 fix builds, such as `private Counter Value { get; } = new /* three */ Counter();`, or anywhere in the type the RH7101 fix formats, withholds the fix, where `main` offers it. A list of omitted type arguments with a comment does the same for RH7101 ([omitted-type-argument-comment-unsupported.md](omitted-type-argument-comment-unsupported.md)). For RH2001, this includes a directive between two modifiers that keep their order, and one between the last modifier and the type, which `main` leaves in its place in the field (RH2001-009):

  ```csharp
  class C
  {
      private
  #pragma warning disable CS0169
      static int Value { get; }
  }
  ```

  Before (`main`):

  ```csharp
  class C
  {
      private
  #pragma warning disable CS0169
      static readonly int _value;
  }
  ```

  After: reported, and no code fix is offered.

  RH2001-004 follows: its paragraph on the modifier list now hands every comment or directive between two modifiers or between the last modifier and the type to RH2001-009, where it named only a comment for the gap in front of the type.

No rule page changes: no rule page shows a gap this change joins or refuses, and rule pages never describe the formatter.

### 5. One space around the `is` of a pattern, a pattern combinator, and the `=` of a `let` clause

The joins put these tokens next to their operands with the minimal separator, and no spacing rule covered them on `main`, so `x is 1 or⏎(2)` would have become `x is 1 or(2)`. FMT-SPACE-010 now gives them one space on both sides, like a binary operator. This also changes code written on one line, wherever such a token touches the token next to it — a parenthesis, a bracket, a literal, or a parenthesized expression; a gap that holds whitespace already became one space on `main`, and the `not` of a negated pattern, the `{` of a property pattern, and the `is` of a type test were spaced on `main` already.

Input (also the output of `main`):

```csharp
using System.Linq;

class C
{
    void M(object o, int[] arr)
    {
        var a = o is(1);
        var b = arr is[1 or(2), _];
        var c = (o)is"s";
        var d = o is(1)and(2);
        var e = from x in arr let y=x select y;
        var f = o switch
                {
                    (1)or(2) => 1,
                    _ => 0,
                };
    }
}
```

After:

```csharp
using System.Linq;

class C
{
    void M(object o, int[] arr)
    {
        var a = o is (1);
        var b = arr is [1 or (2), _];
        var c = (o) is "s";
        var d = o is (1) and (2);
        var e = from x in arr let y = x select y;
        var f = o switch
                {
                    (1) or (2) => 1,
                    _ => 0,
                };
    }
}
```

The combinator of a `case` label gets its spaces the same way (`case(1)or(2):` gives `case(1) or (2):`); the gap behind `case` keeps what it has, as on `main`. Every code fix that formats a node holding such a token writes the same spaces. No analyzer reports these gaps, so no diagnostic changes, and no rule page changes.

## Special cases

### Deleted

- **The decision by the expression's leading trivia in `ExpressionBodyToBlockConverter.CreateReturnStatement`** — the conversion dropped the expression's leading trivia when it held only whitespace and kept it when it held a line break, so that `=>⏎⏎2` became `return⏎2`. That condition is gone. It could not go without a replacement: for `=>⏎    42` the line break belongs to the arrow's trailing trivia, which the conversion discards, so without a case no line break would be left for the join, and the expression's indentation would stand behind `return` (`return         42`). The conversion now builds the `return` with the arrow's trailing trivia minus its whitespace, and the expression keeps its leading trivia unchanged; this is one filter applied to every conversion, not a case for an input class. The line-break phase then joins the `return` and its value across every line break and blank line (FMT-BREAK-041), and FMT-UNSUP-012 and FMT-UNSUP-017 leave no comment, directive, or disabled text in that gap. The check that keeps `return` and an expression from reading as one token (`return42`) remains, now also conditioned on the arrow leaving no line break.

This is the one existing case the declaration deletes, reshaped as stated. The joins themselves are new, and none of them carries a refusal of its own: FMT-BREAK-001's check is the one every join of the phase shares, and with the declaration no gap these joins close ever holds what it refuses, so no join, the combinator join of FMT-BREAK-017 included, needs it, and no line behind such a gap is left for FMT-INDENT-034 to place. Without the declaration, each new join would run that check and leave a continuation line behind every comment it met; that is the alternative the maintainer rejected, not code the change removes.

### Checked and kept

- **`GetOnlyMemberExpressionBodyRewriteUtilities.HasLayoutOnlySeams`** — it checks the trailing trivia of a `return` and the leading trivia of the returned expression before the get-only conversion, exactly the gap behind `return` that FMT-UNSUP-017 refuses. Through the formatter's accessor and get-only transforms (FMT-STRUCT-012) that check is no longer reached with a comment there, because the input is refused first. The RH3209 and RH3210 code fixes call `TryConvertToExpressionBody` on unformatted input, before the formatter sees it, so the check still serves them, and it is kept. FMT-STRUCT-012's clause for a comment behind `return` stays for the same reason.
- **The trivia the get-only conversion carries from a `throw` statement's operand** — `GetOnlyMemberExpressionBodyRewriteUtilities` builds the throw expression with the operand's trivia, which puts a comment behind `throw` in front of an added opening parenthesis. Through the formatter that comment is now refused; the RH3209 and RH3210 fixes still reach the conversion with it, and the transfer also carries whitespace, so it is kept.
- **FMT-BREAK-001's refusal in FMT-BREAK-016's join behind an assignment operator** — a comment behind an assignment operator is not part of this declaration, so that join keeps the shared refusal; the value behind such a comment is a continuation line (FMT-INDENT-034), or, inside a switch arm or an anonymous-object member, refused ([assignment-comment-in-arm-unsupported.md](assignment-comment-in-arm-unsupported.md)).
- **`ListElementInteriorJoinRewriter.IsOperatorWrap`'s cases for `IsPatternExpressionSyntax` and `BinaryPatternSyntax`** — they hand the `is` and the combinators inside a list element to the rewriters that lay them out, which now join and move them; the list-element join still must not decide those gaps. The list-element join keeps joining a parameter's modifiers, which FMT-UNSUP-017 now refuses a comment behind.
- **`ParenthesizedPatternContributor` and `PatternAnchor`** — they place the delimiters of a multi-line parenthesized pattern, which keeps its own line below the `is` (batch 2, answer 10 A); no join reaches them.
- **`BinaryExpressionContributor.ProcessIsPattern`** — it aligns an `is` that starts its line under the tested expression (FMT-INDENT-019); the join is between the `is` and what follows it, not in front of the `is`.
- **The precheck's exclusion of lambdas and switch arms in FMT-UNSUP-012 and FMT-UNSUP-013** — those requirements still decide only expression bodies of members; the lambda and switch-arm gaps are FMT-UNSUP-017's, which checks them with its own gap, so the gap behind a switch arm's `=>` and the gap in front of a lambda's `=>` stay formatted.

## Decisions

### Join the gaps the maintainer listed (maintainer, batch 1b, answer A)

Every listed gap is joined where no comment or directive sits in it: a `return` value, a pattern behind `is`, a switch arm's `=>`, a base list's `:`, a type behind a modifier, a catch filter's `when`, a `for` header behind its `;`, a lambda's expression body, `with`, and `throw`, `await`, and `new` with what follows them. The anonymous-object member's `=` is joined as well, because the maintainer's list of declared gaps names it (batch 1c, answer 1d A), and a declared gap is one the line-break phase joins.

### Join the neighbouring gaps the run found (maintainer, batch 2, answers 1 B, 2 B, and 4 B)

The maintainer joined every further gap the run listed: two modifiers, a modifier and `event`, the `in` of a `foreach` statement, `await foreach` and `await using`, a cast and its operand, `new[]`, a parameter's modifiers, a switch arm's `when`, both sides of the `=` of an anonymous-object member and of a `let` clause, `not` and its pattern, and `yield return` and its value (answer 1 B); a modifier and whatever starts the declaration behind it when that is no type — `class`, a constructor's name, `implicit` or `explicit`, `delegate` (answer 2 B); and the `is` of a type test and its type (answer 4 B). The modifier gap is stated by its cause, every modifier list of the syntax tree, so it also covers the modifiers of an accessor, a lambda, and an anonymous method, which no question showed; the lead decides that on the scheme of the maintainer's answers. The `new` of `new()` needs nothing new: its `(` is joined by FMT-BREAK-039, and a comment in front of it is refused by FMT-UNSUP-014. The modifiers of a function-pointer parameter (`delegate*<ref⏎int, void>`) are a parameter's modifiers: FMT-BREAK-015 joins them as it does on `main`, and a comment or directive behind them is refused like one behind any other parameter's modifier; the lead decided that under the maintainer's standing instruction.

### RH5419's and RH5401's fixes follow the modifier join (lead, under the maintainer's standing instruction)

RH5419 withheld its fix where the formatter kept a line break in front of `this`, a line break behind a modifier included, and RH5401 withheld its fix for a line break behind a property's modifier in the same way. The formatter now joins that line break, so each fix puts such an indexer or property on one line, and a comment behind a modifier, which the formatter now refuses, withholds it (SHARED-FIX-010). Each fix keeps asking the formatter rather than measuring the gap itself, as every formatting-aware fix does.

### The `=` of an anonymous-object member and of a `let` clause is FMT-BREAK-016's

They name a value with `=` like a declarator, a property, an enum member, and a parameter, so FMT-BREAK-016 lays out both sides of all of them as one syntax family; FMT-BREAK-041 holds only gaps that FMT-BREAK-016 does not cover.

### Each combinator starts its line, aligned under `is` (maintainer, batch 1c, answer A; batch 2, answer 5 B)

`var k = x is⏎1 or⏎2 or⏎3;` becomes `var k = x is 1` / `or 2` / `or 3;` with each `or` in the column of `is`; `and` alike; in a `case` label and a switch arm, the `or` stands under the first pattern. FMT-INDENT-019 already gives those columns; the change is the line breaks, which FMT-BREAK-017 now puts in front of every combinator of a chain once one combinator of it has a line break next to it. A chain is the binary patterns nested directly in each other; a parenthesized pattern starts a chain of its own.

### The type behind a type test's `is` joins the `is` (maintainer, batch 2, answer 4 B)

The parser reads `o is string` as a binary expression and `o is string s` as a pattern; a reader cannot see the difference, so both keep the type on the line of the `is`. FMT-BREAK-017 no longer moves a type-test `is` from the end of a line to the start of the next; an `is` that starts its line keeps its place. A comment behind it is already refused (FMT-UNSUP-011).

### Comments and directives in the joined gaps are unsupported, in this run (maintainer, batch 1c, answer 1d A; batch 2, answers 1 B, 3 A, and 6 B)

The maintainer listed the gaps: between `return`, `throw`, `await`, or `new` and what follows; between `is` and its pattern; between a switch arm's pattern and its `=>`; between a type and its base list's `:`; between a modifier and the type; in front of `catch … when`; inside a `for` header behind a `;`; behind a lambda's `=>`; in front of `with`; between an anonymous-object member name and its `=`. Batch 2 added the gaps of answers 1 B and 2 B, and the gap between a pattern combinator and its right pattern (answer 6 B), and confirmed that directives are refused as well, including a conditional group of modifiers and one behind `return` (answer 3 A). A declarator behind `#endif` is not among them and keeps the continuation indent.

### One requirement for the gaps, with a reason per gap (maintainer, batch 2, answer 9 A)

The gaps are one decision with one cause — the gaps the line-break phase joins — so they are one requirement, as the gaps of FMT-UNSUP-014 are. The reason names the gap, so that the CLI's line tells the author where the comment stands, as the reasons of the other shapes do.

### The multi-line pattern's opening delimiter keeps its line (maintainer, batch 2, answer 10 A)

FMT-BREAK-022 starts the opening brace of a multi-line property pattern on its own line, and the opening parenthesis of a multi-line parenthesized pattern as well. The `is` join, and the join of a right pattern onto its combinator, do not undo them: only the content moves (FMT-INDENT-026).

## Alternatives considered

- **Indent the lines behind these gaps as continuation lines (FMT-INDENT-034)** — rejected by the maintainer: the lines belong on the line in front of them.
- **Keep the join refusal and leave comment-kept lines as continuation lines** — rejected by the maintainer (answers 1d A and 6 B): each join would keep a refusal, and the lines it left would need a column.
- **Comments only, directives left out of the declaration** — rejected by the maintainer (batch 2, answer 3 A): every join would have to keep the refusal for directives.
- **Join only the gaps of the first list, and indent the neighbouring ones** — rejected by the maintainer (batch 2, answers 1 B and 2 B).
- **Move only the combinator at the end of a line, as for binary operators** — rejected by the maintainer (batch 2, answer 5 B): a chain wrapped anywhere is wrapped at every combinator.
- **Keep moving a type test's `is` to the next line** — rejected by the maintainer (batch 2, answer 4 B).
- **Ten requirements, one per gap, or one reason for all** — rejected: one cause, one requirement; the table keeps each gap's conditions and reason apart (batch 2, answer 9 A).

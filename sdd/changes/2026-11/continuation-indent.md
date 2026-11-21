# Continuation lines no other requirement places are indented one level deeper

| | |
|---|---|
| Status | Draft |
| Date | 2026-11 |
| Affected requirements | FMT-INDENT-034 (new), FMT-INDENT-035 (new), FMT-INDENT-002, FMT-INDENT-013, FMT-INDENT-015, FMT-DOC-003, FMT-STRUCT-007, FMT-STRUCT-021, FMT-BLANK-020, FMT-SPACE-001, FMT-PIPE-017, SHARED-BLANK-008, SHARED-BODY-001, SHARED-ATTR-022, SHARED-USING-009, RH5111-004 |

## What

A line that continues a statement, a member declaration, or a using directive, and that no requirement of the indentation spec other than block indentation places, is indented four columns right of the line on which the construct it continues starts (FMT-INDENT-034). Until now such a line took the block column of its statement or member, which the indentation spec kept as a non-goal. The construct a line continues is the nearest node that contains the line's first token and starts on an earlier line, so a continuation inside an arm of a switch expression, a member of an anonymous object, a parenthesized pattern, or a lambda inside a collection expression is indented from that construct's line, not from the statement's.

The lines that block indentation places by their scope are not continuations: a nested statement or member, an accessor, a switch section or label, the braces of a scope, `else`, `catch`, `finally`, the `while` of a `do` statement, and a declaration's attribute lists with the token behind them.

The clauses of a query expression that start a line align with the query's first token everywhere, as they already did inside a list element (FMT-INDENT-035).

FMT-INDENT-015 no longer keeps the lines of a wrapped list of omitted type arguments at the block column; they are continuation lines. The examples and texts of other specs that showed the old column are updated: FMT-DOC-003, FMT-STRUCT-007, FMT-STRUCT-021, FMT-BLANK-020, FMT-SPACE-001, FMT-PIPE-017, SHARED-BLANK-008, SHARED-BODY-001, SHARED-ATTR-022, SHARED-USING-009, and RH5111-004.

This change covers thoenissen/Reihitsu#953, the continuation indent of a `return` value from thoenissen/Reihitsu#1852, thoenissen/Reihitsu#1896, and thoenissen/Reihitsu#1818. The join of an expression body across a blank line, the second question of thoenissen/Reihitsu#1852, has its own change doc, [expression-body-join-across-blank-line.md](expression-body-join-across-blank-line.md).

## Why

A continuation line at the column of its statement or member reads like a new statement, and inside an expression laid out to the right it lands left of the construct it belongs to, outside its own braces or parentheses. The maintainer decided one general rule instead of one alignment contributor per construct (thoenissen/Reihitsu#921, Q1; thoenissen/Reihitsu#918, Q5; thoenissen/Reihitsu#913, Q2): a continuation line that no alignment requirement owns is indented one level deeper than the line its statement or member starts on, and the alignment requirements keep precedence. The instances collected on thoenissen/Reihitsu#953 and the three issues of this run are applications of that rule.

Several instances listed on thoenissen/Reihitsu#953 are no longer reachable: the formatter now refuses the shapes that held them (a comment behind `?` or `:`, FMT-UNSUP-008; a comment inside a variable declaration, FMT-UNSUP-004; a comment, directive, or disabled text behind `=>` or in front of it, FMT-UNSUP-012 and FMT-UNSUP-013, which also covers the region between a signature and its arrow; a directive in a field with several declarators, FMT-UNSUP-010), or the line-break phase now joins them (a primary constructor's base argument list, FMT-BREAK-039), or another requirement already places them (an array initializer inside a wrapped argument list). They need no requirement here, and the clarification of FMT-BLANK-013 the issue asked for has no input left to decide.

## Visible output changes

Every example below is whole-document formatting. "Before" is the output on `main` (observed with `scripts/format.sh`); the input is the same text.

### A value, an operand, a pattern, and a `return` value (thoenissen/Reihitsu#953, thoenissen/Reihitsu#1852)

```csharp
class C
{
    int M(int x, object o)
    {
        x += // c
        3;

        switch (x)
        {
            case 1 or
            2:
                break;
        }

        var p = o is
        not null;

        return
        x;
    }
}
```

After:

```csharp
class C
{
    int M(int x, object o)
    {
        x += // c
            3;

        switch (x)
        {
            case 1 or
                2:
                break;
        }

        var p = o is
            not null;

        return
            x;
    }
}
```

A comment behind `return` gives the same column: `return // c⏎_v;` puts `_v;` one level deeper than `return`, where `main` puts it in the column of `return`.

### A continuation inside an expression laid out to the right (thoenissen/Reihitsu#953)

Before:

```csharp
class C
{
    int M(int x, object o)
    {
        var w = new
                {
                    A // c
        = 1
                };
        var d = o is
                  (
        int or long
                  );

        return x switch
               {
                   1
        => 2,
                   _ => 0,
               };
    }
}
```

After:

```csharp
class C
{
    int M(int x, object o)
    {
        var w = new
                {
                    A // c
                        = 1
                };
        var d = o is
                  (
                      int or long
                  );

        return x switch
               {
                   1
                       => 2,
                   _ => 0,
               };
    }
}
```

### Later declarators, also behind `#endif` (thoenissen/Reihitsu#1818, thoenissen/Reihitsu#953)

Before:

```csharp
using System;

class C
{
    /// <summary>
    /// Q
    /// </summary>
    public int Q,
    R;

    public event Action A,
#if X
        B,
#endif
    D;

    void M()
    {
        int a = 1,
        b = 2;
    }
}
```

After:

```csharp
using System;

class C
{
    /// <summary>
    /// Q
    /// </summary>
    public int Q,
        R;

    public event Action A,
#if X
        B,
#endif
        D;

    void M()
    {
        int a = 1,
            b = 2;
    }
}
```

### Declaration headers and clauses (thoenissen/Reihitsu#953)

Before:

```csharp
class B;

class D
: B
{
    private
    int _f;

    void M()
    {
        try
        {
        }
        catch (System.Exception e)
        when (e != null)
        {
        }
    }
}
```

After:

```csharp
class B;

class D
    : B
{
    private
        int _f;

    void M()
    {
        try
        {
        }
        catch (System.Exception e)
            when (e != null)
        {
        }
    }
}
```

### Lambda bodies and a `with` expression (thoenissen/Reihitsu#953)

Before:

```csharp
using System;

class C
{
    void M(int[] a)
    {
        Func<int> u = () =>
        a.Length;
        Func<int>[] f = [
                            () =>
        1,
                            () => 2
                        ];
        var w = new R()
        with
        {
        };
    }
}

record R;
```

After:

```csharp
using System;

class C
{
    void M(int[] a)
    {
        Func<int> u = () =>
            a.Length;
        Func<int>[] f = [
                            () =>
                                1,
                            () => 2
                        ];
        var w = new R()
            with
            {
            };
    }
}

record R;
```

### The hole of a multi-line verbatim interpolated string (thoenissen/Reihitsu#953)

Before:

```csharp
class C
{
    string M(int x)
    {
        var s = $@"line one
      {
        x
        }";

        return s;
    }
}
```

After: `x` and `}";` move to column 12, one level deeper than the line of `var`; the text of the string is unchanged.

### Query clauses outside a list element (thoenissen/Reihitsu#1896)

Before:

```csharp
using System.Linq;

class C
{
    void M(int[] arr)
    {
        var q = from x in arr
        let y = x
        where y > 0
        select y;
        var r = from x in arr
        let y
        = x
        select y into z
        select z;
    }
}
```

After:

```csharp
using System.Linq;

class C
{
    void M(int[] arr)
    {
        var q = from x in arr
                let y = x
                where y > 0
                select y;
        var r = from x in arr
                let y
                    = x
                select y into z
                select z;
    }
}
```

### A wrapped list of omitted type arguments (FMT-INDENT-015)

Before:

```csharp
class C
{
    void M()
    {
        var type = typeof(System.Collections.Generic.Dictionary<, // c
        >);
    }
}
```

After: `>);` moves to column 12.

### A `;` or another token behind a stranded documentation comment or a conditional group (FMT-DOC-003, FMT-SPACE-001, FMT-STRUCT-007)

Before:

```csharp
class C
{
    int A => 1 /// stray
    ;

    public int M()
    {
        return 1    /// stray
        ;
    }

    int Get()
    {
        return 42
#if DEBUG
        + 1
#endif
        ;
    }
}
```

After: each `;` moves one level deeper than the line its declaration or statement starts on, to columns 8, 12, and 12. The disabled `+ 1` keeps its column.

### Split fields and other declarations (FMT-STRUCT-021, FMT-BLANK-020, SHARED-USING-009)

The fields that FMT-STRUCT-021 splits keep a line break behind the type or between two modifiers; the line behind it moves one level deeper than the field's first line:

```csharp
class C
{
    private static
    readonly int _g = 1;
}
```

After: `readonly int _g = 1;` moves to column 8. In the same way, a property pattern's `{` behind `if (o is C` and a comment on its own line (FMT-BLANK-020) moves to column 12 with the comment, and `.Text;` behind a directive inside a using directive (SHARED-USING-009) moves to column 4.

### Code fixes

Every code fix that lays out a node through the formatter gives a continuation line in that node the new column. The ones the specs show:

- **RH3202, RH3203, RH3205, RH3206, RH3207, RH3208** (an expression body converted to a block, SHARED-BODY-001): a `return` value or a `;` that stays on its own line inside the created block moves one level deeper than the created statement. Example, RH3203:

  ```csharp
  class C
  {
      private int _value;

      public C(int value) => _value = value
  #if DEBUG
          + 1
  #endif
          ;
  }
  ```

  Before:

  ```csharp
  class C
  {
      private int _value;

      public C(int value)
      {
          _value = value
  #if DEBUG
          + 1
  #endif
          ;
      }
  }
  ```

  After: the `;` moves to column 12.

- **RH5514** (an attribute list with several attributes split into lists, SHARED-ATTR-022): on a lambda, the list moved to the next line continues the lambda.

  ```csharp
  Func<int> f = [A, B] () => 1;
  ```

  Before:

  ```csharp
          Func<int> f = [A]
          [B] () => 1;
  ```

  After:

  ```csharp
          Func<int> f = [A]
              [B] () => 1;
  ```

- **RH7101** (a field split into one field per declaration, FMT-STRUCT-021): the line behind a line break in a field's head moves as for the formatter above.
- **RH5305** (a collection expression laid out one element per line): the body of a lambda element on its own line moves from column 24, the column of `[`, to one level deeper than the line of `() =>`, the column the CLI now gives it too.

RH5111 keeps its result: its target keeps its document column (FMT-PIPE-017), and only the column the context's layout would give a later declarator changes, from 8 to 12, which the fix does not write.

No rule page changes: no rule page shows a continuation line that this change moves.

## Decisions

### The construct a line continues is the nearest one that starts on an earlier line

The maintainer's decision names the line on which the statement or member starts. For a continuation inside a construct laid out to the right — a switch-expression arm, an anonymous-object member, a parenthesized pattern, a lambda inside a collection expression — that line puts the continuation left of its own construct. The rule measures from the nearest node that contains the line's first token and starts on an earlier line; where that node starts on the statement's line, which holds for every example of the decision, both readings agree. Open for the maintainer (output question).

### Lines that block indentation places by their scope are not continuations

Which lines count is stated by cause: a line whose first token begins a part that block indentation places by its scope keeps that column. The list of those parts is closed by the syntax of C#: a statement, a member, an accessor, a switch section or label, a scope's brace, the keyword of an `else`, `catch`, or `finally` clause, the `while` of a `do` statement, and a declaration's attribute lists with the token behind them.

### Query clauses align with `from` everywhere

FMT-INDENT-013 already aligned the clauses of a query inside a list element with the query's first token. The same alignment applies outside a list element (FMT-INDENT-035), so a query looks the same wherever it stands. Open for the maintainer (thoenissen/Reihitsu#1896).

### Omitted type arguments are continuation lines

FMT-INDENT-015 kept a wrapped list of omitted type arguments at the block column because such a list has no first element to align to. That reason is the general rule's case, so its lines become continuation lines. Open for the maintainer (output question).

## Alternatives considered

- **One alignment contributor per construct** — rejected by the maintainer's decision: it repeats today's approach and keeps leaving gaps.
- **Measure every continuation from the statement's or member's line** — rejected as the recommendation: it puts the continuation of an arm, an anonymous-object member, or a parenthesized pattern left of that construct.
- **Indent query clauses one level deeper than the statement** — rejected as the recommendation: the clauses of one query would not line up under `from`, which they already do inside a list element.
- **Keep the block column for the shapes other open issues ask to align differently** (a base list wrapped behind its colon, thoenissen/Reihitsu#1633; a line inside an angle-bracket list's first element or a qualified name, thoenissen/Reihitsu#1634 and thoenissen/Reihitsu#1905; the content of a parenthesized or positional pattern, thoenissen/Reihitsu#1542) — rejected: excluding them would be a special case in the rule, and the maintainer decided the rule for every construct. A requirement that aligns one of them later takes precedence over this rule.

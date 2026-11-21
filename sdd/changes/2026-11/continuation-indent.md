# Continuation lines no other requirement places are indented one level deeper

| | |
|---|---|
| Status | Accepted |
| Date | 2026-11 |
| Affected requirements | FMT-INDENT-034 (new), FMT-INDENT-035 (new), FMT-INDENT-002, FMT-INDENT-013, FMT-INDENT-026, FMT-DOC-003, FMT-STRUCT-007, FMT-STRUCT-021, FMT-BLANK-020, FMT-SPACE-001, FMT-PIPE-017, FMT-PIPE-024, SHARED-BLANK-008, SHARED-BODY-001, SHARED-USING-009, RH5111-004 |

## What

A line that continues a statement, a member declaration, or a using directive, and that no requirement of the indentation spec other than block indentation places, is indented four columns right of the line on which the construct it continues starts (FMT-INDENT-034). Until now such a line took the block column of its statement or member, which the indentation spec kept as a non-goal. The construct a line continues is the nearest node that contains the line's first token and starts on an earlier line, so a continuation inside an arm of a switch expression or a member of an anonymous object is indented from that construct's line, not from the statement's.

Most lines of this kind no longer exist: the maintainer decided to join the gaps they stood behind — a `return` value, a pattern behind `is`, a switch arm's `=>`, a base list, a type behind a modifier, a catch filter, a `for` header, a lambda's body, `with`, `throw`, `await`, `new`, and an anonymous-object member's `=` — and to refuse a comment or a directive in those gaps; [continuation-join.md](continuation-join.md) records that change. FMT-INDENT-034 places what no join removes: a line kept behind a comment or a directive that no requirement refuses, a later declarator, a `;` behind a conditional group or a stray documentation comment, a line inside the hole of a verbatim interpolated string, and a line behind any other line break no requirement joins.

The lines that block indentation places by their scope are not continuations: a nested statement or member, an accessor, a switch section or label, the braces of a scope, `else`, `catch`, `finally`, the `while` of a `do` statement, and a declaration's attribute lists with the token behind them.

The pattern inside a parenthesized pattern that spans several lines is indented four columns right of its opening parenthesis, whatever kind of pattern it is (FMT-INDENT-026); until now only a property pattern there was.

The clauses of a query expression that start a line align with the query's first token everywhere, as they already did inside a list element (FMT-INDENT-035).

The examples and texts of other specs that showed the old column are updated: FMT-DOC-003, FMT-STRUCT-007, FMT-STRUCT-021, FMT-BLANK-020, FMT-SPACE-001, FMT-PIPE-017, SHARED-BLANK-008, SHARED-BODY-001, SHARED-USING-009, and RH5111-004.

A code fix that formats a node starting in the middle of its line measures a continuation line whose construct starts on that line from where the document begins the line. The node-level entry points pass that column as one more fact, the line-start column (FMT-PIPE-024), so the fix gives the line the column whole-document formatting gives it.

This change covers thoenissen/Reihitsu#953, thoenissen/Reihitsu#1896, and thoenissen/Reihitsu#1818. The `return` value of thoenissen/Reihitsu#1852 is joined ([continuation-join.md](continuation-join.md)), and the join of an expression body across a blank line has its own change doc, [expression-body-join-across-blank-line.md](expression-body-join-across-blank-line.md). The list of omitted type arguments FMT-INDENT-015 kept at the block column is refused instead ([omitted-type-argument-comment-unsupported.md](omitted-type-argument-comment-unsupported.md)).

## Why

A continuation line at the column of its statement or member reads like a new statement, and inside an expression laid out to the right it lands left of the construct it belongs to, outside its own braces or parentheses. The maintainer decided one general rule instead of one alignment contributor per construct (thoenissen/Reihitsu#921, Q1; thoenissen/Reihitsu#918, Q5; thoenissen/Reihitsu#913, Q2): a continuation line that no alignment requirement owns is indented one level deeper than the line its statement or member starts on, and the alignment requirements keep precedence. The instances collected on thoenissen/Reihitsu#953 and the three issues of this run are applications of that rule, except where the maintainer decided to join the line instead (batch 1b, answer A).

Several instances listed on thoenissen/Reihitsu#953 are no longer reachable: the formatter now refuses the shapes that held them (a comment behind `?` or `:`, FMT-UNSUP-008; a comment inside a variable declaration, FMT-UNSUP-004; a comment, directive, or disabled text behind `=>` or in front of it, FMT-UNSUP-012 and FMT-UNSUP-013, which also covers the region between a signature and its arrow; a directive in a field with several declarators, FMT-UNSUP-010), or the line-break phase now joins them (a primary constructor's base argument list, FMT-BREAK-039), or another requirement already places them (an array initializer inside a wrapped argument list). They need no requirement here, and the clarification of FMT-BLANK-013 the issue asked for has no input left to decide.

## Visible output changes

Every example below is whole-document formatting. "Before" is the output on `main` (observed with `scripts/format.sh`); the input is the same text.

### A value kept on its own line by a comment (thoenissen/Reihitsu#953)

Before:

```csharp
class C
{
    int M(int x, int y)
    {
        x += // c
        3;
        y += // c
        3 + 4;

        return x + y;
    }
}
```

After:

```csharp
class C
{
    int M(int x, int y)
    {
        x += // c
            3;
        y += // c
            3 + 4;

        return x + y;
    }
}
```

### A continuation inside an expression laid out to the right (thoenissen/Reihitsu#953)

Before:

```csharp
class C
{
    int M(int x, int y)
    {
        var w = new
                {
                    A = x += // c
        3
                };

        return x switch
               {
                   1 => y += // c
        2,
                   _ => 0,
               };
    }
}
```

After: the value is indented from the line of `A` (column 20) and from the line of `1` (column 19).

```csharp
class C
{
    int M(int x, int y)
    {
        var w = new
                {
                    A = x += // c
                        3
                };

        return x switch
               {
                   1 => y += // c
                       2,
                   _ => 0,
               };
    }
}
```

### The content of a parenthesized pattern (thoenissen/Reihitsu#953)

Before:

```csharp
class C
{
    bool M(object o)
    {
        var d = o is
                  (
        int
        or long
                  );

        return d;
    }
}
```

After: the content is indented four columns right of `(`, and `or` aligns with `int` (FMT-INDENT-019).

```csharp
class C
{
    bool M(object o)
    {
        var d = o is
                  (
                      int
                      or long
                  );

        return d;
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

### Other lines no join removes (thoenissen/Reihitsu#953)

A line break that no requirement joins leaves a continuation line. On `main` these lines stand in the column of their statement; after the change they stand one level deeper than the line of their construct. Whether these lines are joined instead is open (the maintainer's answers do not name them); the spec places them as continuation lines until then.

Before:

```csharp
using System.Collections.Generic;

class C
{
    IEnumerable<int> M(int x, object o, int[] a)
    {
        foreach (var y in
        a)
        {
        }

        var c = (int)
        x;
        var r = o switch
                {
                    int i
        when i > 0 => 1,
                    _ => 0,
                };
        var w = new
                {
                    A =
        1
                };

        yield return
        x;
    }
}
```

After:

```csharp
using System.Collections.Generic;

class C
{
    IEnumerable<int> M(int x, object o, int[] a)
    {
        foreach (var y in
            a)
        {
        }

        var c = (int)
            x;
        var r = o switch
                {
                    int i
                        when i > 0 => 1,
                    _ => 0,
                };
        var w = new
                {
                    A =
                        1
                };

        yield return
            x;
    }
}
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

### The closing bracket behind an omitted array size (SHARED-BLANK-008)

Before:

```csharp
class C
{
    void M()
    {
        var type = typeof(int[, // c
        ]);
    }
}
```

After: `]);` moves to column 12.

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

- **RH3202, RH3203, RH3205, RH3206, RH3207, RH3208** (an expression body converted to a block, SHARED-BODY-001): a `;` that stays on its own line inside the created block, behind a conditional group or a stray documentation comment, moves one level deeper than the created statement. A `return` value is joined instead ([continuation-join.md](continuation-join.md)). Example, RH3203:

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

- **RH7101** (a field split into one field per declaration, FMT-STRUCT-021): the line behind a line break between two modifiers or between the type and the name moves as for the formatter above; a line break between the last modifier and the type is joined ([continuation-join.md](continuation-join.md)).
- **RH5302, RH5308** (a chain of `&&` and `||` operators, a conditional expression), whose fixes format a node that starts in the middle of its line: a continuation line whose construct starts on that line moves from four columns right of the node's first token to four columns right of where the document begins the line, the column whole-document formatting now gives it.

  ```csharp
          var r = (x += // c
          3) > 0 &&
          y;
  ```

  Before, RH5302 (whole-document formatting puts `3) > 0` in column 8):

  ```csharp
          var r = (x += // c
                  3) > 0
                  && y;
  ```

  After, RH5302 and whole-document formatting:

  ```csharp
          var r = (x += // c
              3) > 0
                  && y;
  ```

  RH5308 on `var s = (x += // c⏎3) > 0 ? 1 :⏎2;`: `3) > 0` moves from column 16 (whole-document formatting: 8) to column 12; `? 1` and `: 2;` stay in column 20.

RH5111 keeps its result: its target keeps its document column (FMT-PIPE-017), and only the column the context's layout would give a later declarator changes, from 8 to 12, which the fix does not write.

No rule page changes: no rule page shows a continuation line that this change moves.

## Decisions

### Join where the maintainer listed a gap, indent what remains (maintainer, batch 1b, answers A and Q7)

The maintainer replaced most applications of the continuation indent by joins ([continuation-join.md](continuation-join.md)). Lines that cannot be joined — a `;` behind `#endif` or a stray `///`, the hole of a verbatim interpolated string, a value kept behind a comment no requirement refuses — keep the continuation indent of FMT-INDENT-034. A declarator behind `#endif` gets it as well (maintainer, answer 6A).

### The construct a line continues is the nearest one that starts on an earlier line

The maintainer's earlier decision (thoenissen/Reihitsu#921, Q1) names the line on which the statement or member starts. For a continuation inside a construct laid out to the right — a switch-expression arm or an anonymous-object member — that line puts the continuation left of its own construct. The rule measures from the nearest node that contains the line's first token and starts on an earlier line; where that node starts on the statement's line, which holds for every example of the decision, both readings agree. The maintainer's answers replaced the question with joins and named no column for the lines that remain inside such a construct; the spec is written on this reading, and the run asks it again for the remaining lines.

### The content of a parenthesized pattern is one level inside its parentheses (maintainer, batch 1b, answer 1e A)

The pattern inside a parenthesized pattern that spans several lines is indented four columns right of the opening parenthesis, whatever kind of pattern it is. FMT-INDENT-026 already did so for a property pattern there; the anchor now applies to every pattern inside the parentheses.

### A node formatted on its own measures from where the document begins its line

A code fix formats a node as a root of its own, and a phase reads nothing of the document outside it. When the node starts in the middle of its line and a continuation's construct starts on that line, the line's beginning is outside the root. The node-level entry points pass the column of the first token or comment on that line, as it stands in the document, as a fact (FMT-PIPE-024); the indentation phase measures from it. The fix keeps everything in front of the node as written, so this is a fact about the document, not a column a phase would compute. RH5302 and RH5308 already require their fixes to format as the formatter formats in place, so the output is decided by those requirements. Measuring from the node's first token instead was rejected: it gives column 20 where whole-document formatting gives 12.

### Lines that block indentation places by their scope are not continuations

Which lines count is stated by cause: a line whose first token begins a part that block indentation places by its scope keeps that column. The list of those parts is closed by the syntax of C#: a statement, a member, an accessor, a switch section or label, a scope's brace, the keyword of an `else`, `catch`, or `finally` clause, the `while` of a `do` statement, and a declaration's attribute lists with the token behind them.

### Query clauses align with `from` everywhere (maintainer, answer 3A)

FMT-INDENT-013 already aligned the clauses of a query inside a list element with the query's first token. The same alignment applies outside a list element (FMT-INDENT-035), so a query looks the same wherever it stands.

## Alternatives considered

- **One alignment contributor per construct** — rejected by the maintainer's decision: it repeats today's approach and keeps leaving gaps.
- **Indent every line behind a gap the maintainer listed as a continuation line** — rejected by the maintainer: those lines are joined.
- **Measure every continuation from the statement's or member's line** — not chosen: it puts the continuation inside an arm or an anonymous-object member left of that construct.
- **Indent query clauses one level deeper than the statement** — rejected by the maintainer (answer 3A).
- **Keep the block column for the shapes other open issues ask to align differently** (a line inside an angle-bracket list's first element or a qualified name, thoenissen/Reihitsu#1634 and thoenissen/Reihitsu#1905; the content of a positional pattern, thoenissen/Reihitsu#1542) — rejected: excluding them would be a special case in the rule, and the maintainer decided the rule for every construct. A requirement that aligns one of them later takes precedence over this rule. A base list whose `:` starts its line (thoenissen/Reihitsu#1633) no longer exists: the `:` joins the type's line.

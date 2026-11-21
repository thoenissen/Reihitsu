# Continuations behind a keyword, a pattern's `is`, an arm, a header, or a lambda join their line, and comments there are unsupported

| | |
|---|---|
| Status | Accepted |
| Date | 2026-11 |
| Affected requirements | FMT-BREAK-041 (new), FMT-UNSUP-017 (new), FMT-BREAK-017, FMT-BREAK-022, FMT-UNSUP-010, FMT-UNSUP-011, FMT-UNSUP-012, FMT-UNSUP-013, FMT-STRUCT-006, RH2001-007, RH7101-007 |
| Depends on | [continuation-indent.md](continuation-indent.md) |

## What

The line-break phase joins ten gaps onto one line, removing their line breaks and blank lines (FMT-BREAK-041): between `return`, `throw`, `await`, or `new` and what follows it; between the `is` of a pattern expression and its pattern; between a switch-expression arm's pattern and its `=>`; between a type and its base list's `:`; between the last modifier and the type; in front of the `when` of a catch clause; behind each `;` of a `for` header; behind the `=>` of a lambda with an expression body; in front of `with`; and between an anonymous-object member's name and its `=`. The pattern behind `is` keeps its own line where the opening delimiter of a multi-line property or parenthesized pattern starts one (FMT-BREAK-022, which now states the parenthesized pattern's case as well).

A pattern chain is laid out like a chain of binary operators: a combinator (`and`, `or`) at the end of a line moves to the start of the next line, and its right pattern joins it (FMT-BREAK-017). The combinators then align under the `is`, or under the first pattern of a `case` label or a switch arm, as FMT-INDENT-019 already states.

A comment, a directive, or disabled text in one of the ten gaps is a shape the formatter refuses as a whole (FMT-UNSUP-017): the CLI leaves such a file unchanged and reports it, and every code fix whose formatted node holds the shape is withheld (SHARED-FIX-010).

This change covers the instances of thoenissen/Reihitsu#953 that the maintainer decided to join rather than indent, and the `return` value of thoenissen/Reihitsu#1852. Lines that cannot be joined keep the continuation indent of [continuation-indent.md](continuation-indent.md).

## Why

The first draft of this run indented every such line one level deeper than the line of its construct (FMT-INDENT-034). The maintainer decided that the lines behind these gaps belong on the line in front of them: a `return` and its value, a pattern and its `is`, an arm and its arrow, a type and its base list, a modifier and its type, a catch clause and its filter, a `for` header, a lambda and its body, an expression and the `with` that copies it, a member name and its `=` read as one construct, and a line break there carries no meaning (batch 1b, answer A). A pattern chain wrapped behind its combinators is rewritten so that each combinator starts its line, aligned under the `is` (batch 1c, answer A). A comment or a directive in a joined gap would keep the join from happening and leave a line that needs a column of its own; the maintainer decided to refuse those files instead (batch 1c, answer 1d A).

## Visible output changes

Every *before* below is the output of `main`, produced with `scripts/format.sh` for the formatter and with `scripts/apply-fix.sh` for the code fixes. The CLI's line has the form it prints today for the other unsupported shapes.

### 1. The ten gaps are joined

Input (also the output of `main`):

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

class B;

class D
: B
{
    private
    int _f;

    async Task<int> M(int x, object o, Task<int> t, int[] a)
    {
        var p = o is
        not null;
        var r = x switch
                {
                    1
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

        Func<int> u = () =>
        a.Length;
        var n = new
        List<int>();
        var q = await
        t;
        var w = new
                {
                    A
        = 1
                };

        if (x > 0)
        {
            throw
            new Exception();
        }

        return
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

class D : B
{
    private int _f;

    async Task<int> M(int x, object o, Task<int> t, int[] a)
    {
        var p = o is not null;
        var r = x switch
                {
                    1 => 2,
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

        Func<int> u = () => a.Length;
        var n = new List<int>();
        var q = await t;
        var w = new
                {
                    A = 1
                };

        if (x > 0)
        {
            throw new Exception();
        }

        return x;
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

A blank line in a joined gap goes with the join: `return⏎⏎x;` gives `return x;`.

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

A chain written on one line, or the part of it that stands on one line, keeps its layout: `x is 1 or 2 or⏎3` gives `x is 1 or 2` and `or 3` below it, under the `is`.

### 3. A comment, a directive, or disabled text in one of the ten gaps refuses the file

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

Each gap has its reason (FMT-UNSUP-017): `a comment or directive between return, throw, await, or new and what follows it`, `a comment or directive between is and its pattern`, `a comment or directive in front of the => of a switch arm`, `a comment or directive in front of the : of a base list`, `a comment or directive between a modifier and the type`, `a comment or directive in front of the when of a catch clause`, `a comment or directive behind a ; of a for header`, `a comment or directive behind the => of a lambda`, `a comment or directive in front of with`, and `a comment or directive between an anonymous-object member's name and its =`. A comment between a field's or a local's modifier and its type keeps the reason it gives on `main` (FMT-UNSUP-004), and so does a directive in a field declaration with several declarators (FMT-UNSUP-010).

Code that held such a comment is formatted on `main` and refused now; the same comment written above the statement or behind its line is formatted. A switch arm with a comment in front of its `=>` (`1 /* c */ => "one"`) and a lambda with a comment behind its `=>` (`() => /* c */ 1`), which the other sides of FMT-UNSUP-013 and FMT-UNSUP-012 showed as formatted, are refused now.

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

- **RH2001, RH7101** (a private auto-property turned into a field; a field split into one field per declaration), whose fixes ask the formatter before they are offered: a comment, a directive, or disabled text in one of the gaps inside the field the RH2001 fix builds, such as `private Counter Value { get; } = new /* three */ Counter();`, or anywhere in the type the RH7101 fix formats, withholds the fix, where `main` offers it. A list of omitted type arguments with a comment does the same for RH7101 ([omitted-type-argument-comment-unsupported.md](omitted-type-argument-comment-unsupported.md)).

No rule page changes: no rule page shows a gap this change joins or refuses, and rule pages never describe the formatter.

## Special cases

### Deleted

- **The kept line break in `ExpressionBodyToBlockConverter.CreateReturnStatement`** — the conversion drops the expression's leading trivia when it holds only whitespace and keeps it when it holds a line break, so that `=>⏎⏎2` became `return⏎2`. The line-break phase now joins every `return` and its value across line breaks and blank lines (FMT-BREAK-041), and FMT-UNSUP-012 and FMT-UNSUP-017 leave no comment, directive, or disabled text in that gap, so the conversion needs no case of its own for the expression's leading trivia: whatever whitespace and line breaks it carries, the join removes them. Only the check that keeps `return` and an expression from reading as one token (`return42`) remains.
- **The join refusal in each of the ten joins** — with FMT-UNSUP-017, no gap the joins close holds a comment, a directive, or disabled text, so none of the ten joins needs the refusal of FMT-BREAK-001, and no line behind such a gap is left for FMT-INDENT-034 to place. Without the declaration, every join would carry that refusal, and the lines it left would be continuation lines kept by a comment.

### Checked and kept

- **FMT-BREAK-001's refusal for the combinator join of FMT-BREAK-017** — a comment behind `and` or `or` is not part of the declaration (the maintainer's list names no combinator gap), so the combinator join keeps the shared refusal, and the right pattern then starts a continuation line (FMT-INDENT-034). It is the same check every other join of the phase uses, not a case of its own.
- **`ListElementInteriorJoinRewriter.IsOperatorWrap`'s cases for `IsPatternExpressionSyntax` and `BinaryPatternSyntax`** — they hand the `is` and the combinators inside a list element to the rewriters that lay them out, which now join and move them; the list-element join still must not decide those gaps.
- **`ParenthesizedPatternContributor` and `PatternAnchor`** — they place the delimiters of a multi-line parenthesized pattern, which keeps its own line below the `is`; no join reaches them.
- **`BinaryExpressionContributor.ProcessIsPattern`** — it aligns an `is` that starts its line under the tested expression (FMT-INDENT-019); the join is between the `is` and its pattern, not in front of the `is`.
- **The precheck's exclusion of lambdas and switch arms in FMT-UNSUP-012 and FMT-UNSUP-013** — those requirements still decide only expression bodies of members; the lambda and switch-arm gaps are FMT-UNSUP-017's, which checks them with its own gap, so the gap behind a switch arm's `=>` and the gap in front of a lambda's `=>` stay formatted.

## Decisions

### Join the gaps the maintainer listed (maintainer, batch 1b, answer A)

Every listed gap is joined where no comment or directive sits in it: a `return` value, a pattern behind `is`, a switch arm's `=>`, a base list's `:`, a type behind a modifier, a catch filter's `when`, a `for` header behind its `;`, a lambda's expression body, `with`, and `throw`, `await`, and `new` with what follows them. The anonymous-object member's `=` is joined as well, because the maintainer's list of declared gaps names it (batch 1c, answer 1d A), and a declared gap is one the line-break phase joins.

### Each combinator starts its line, aligned under `is` (maintainer, batch 1c, answer A)

`var k = x is⏎1 or⏎2 or⏎3;` becomes `var k = x is 1` / `or 2` / `or 3;` with each `or` in the column of `is`; `and` alike; in a `case` label and a switch arm, the `or` stands under the first pattern. FMT-INDENT-019 already gives those columns; the change is the line break, which FMT-BREAK-017 now moves in front of the combinator as it does for a binary operator.

### Comments and directives in the joined gaps are unsupported, in this run (maintainer, batch 1c, answer 1d A)

The maintainer listed the gaps: between `return`, `throw`, `await`, or `new` and what follows; between `is` and its pattern; between a switch arm's pattern and its `=>`; between a type and its base list's `:`; between a modifier and the type; in front of `catch … when`; inside a `for` header behind a `;`; behind a lambda's `=>`; in front of `with`; between an anonymous-object member name and its `=`. A declarator behind `#endif` is not among them and keeps the continuation indent.

### One requirement for the ten gaps, with a reason per gap

The ten gaps are one decision with one cause — the gaps FMT-BREAK-041 joins — so they are one requirement, as the gaps of FMT-UNSUP-014 are. The reason names the gap, so that the CLI's line tells the author where the comment stands, as the reasons of the other shapes do.

### The multi-line pattern's opening delimiter keeps its line

FMT-BREAK-022 starts the opening brace of a multi-line property pattern on its own line, and the line-break phase does the same for the opening parenthesis of a multi-line parenthesized pattern; FMT-BREAK-022 now states that case. The `is` join does not undo them: the maintainer's answer on a parenthesized pattern's content (batch 1b, answer 1e A) was given for that layout.

## Alternatives considered

- **Indent the lines behind these gaps as continuation lines (FMT-INDENT-034)** — rejected by the maintainer: the lines belong on the line in front of them.
- **Keep the join refusal and leave comment-kept lines as continuation lines** — rejected by the maintainer (answer 1d A): each join would keep a refusal, and the lines it left would need a column.
- **Ten requirements, one per gap** — rejected: one cause, one requirement; the table keeps each gap's conditions and reason apart.
- **A combinator at the start of every line of a chain that is wrapped anywhere** — rejected: FMT-BREAK-017 never wraps what the author wrote on one line, for binary operators as for combinators.

# A wrapped positional pattern is laid out like a list pattern

| | |
|---|---|
| Status | Accepted |
| Date | 2026-11 |
| Affected requirements | FMT-BREAK-017, FMT-BREAK-022, FMT-BREAK-038, FMT-BREAK-043 (new), FMT-INDENT-025, FMT-INDENT-006, the indentation spec's non-goals |
| Depends on | [continuation-indent.md](continuation-indent.md), [pattern-closing-delimiter-comment.md](pattern-closing-delimiter-comment.md) |

## What

The positional pattern clause of a recursive pattern — `(1, 2)` in `o is (1, 2)` and in `o is P(1, 2)` — joins the list of constructs FMT-BREAK-022 lays out: written on one line, it stays on one line; once it spans several lines, every subpattern starts its own line, the first one on the line after the `(`, and the `)` starts its own line. Its `(` stays on the line of the token in front of it: behind `is` and `not` FMT-BREAK-041 already joins it, and behind the pattern's type FMT-BREAK-043 now joins it. FMT-INDENT-025 indents the subpatterns one level deeper than the `(` and puts the `)` in the column of the `(`, as for a list pattern. A comma that starts a line in the clause moves behind the subpattern in front of it (FMT-BREAK-038), and a comment in front of the `)` is indented into the subpatterns' column (FMT-INDENT-006). A positional pattern that spans several lines next to a combinator wraps the chain, so the combinator starts its own line, as next to a multi-line list pattern (FMT-BREAK-017). The indentation spec drops the positional pattern from its non-goal on comma columns.

This change covers the positional half of thoenissen/Reihitsu#1542 and the comma-first shape recorded on it. The parenthesized half is already decided: the maintainer kept the `(` of a multi-line parenthesized pattern on its own line below the `is` (batch 2, answer 10 A, [continuation-join.md](continuation-join.md)) and indented its content four columns right of the `(` (answer 1e A, [continuation-indent.md](continuation-indent.md)); `main` produces that layout (FMT-BREAK-022, FMT-INDENT-026), so the issue's reading A for parenthesized patterns, which kept the `(` on the `is` line, is not taken up. The further members recorded on the issue are decided on `main` too: `o is not⏎(1 or 2)` and the pattern behind `is` are joined (FMT-BREAK-041), `{ X:⏎1 }` gets the continuation indent inside a property pattern laid out by FMT-BREAK-022, and the parenthesized `(1⏎or 2)`, `(int⏎or long)`, and the `case` label form follow FMT-BREAK-022 and FMT-INDENT-026.

## Why

No requirement lays out a positional pattern. On `main` its lines keep the author's breaks and get the continuation indent of FMT-INDENT-034, so its subpatterns and its `)` stand four columns right of the statement, left of the `(` they belong to. Every other delimited pattern — a list pattern, a property pattern, a parenthesized pattern — has a decided layout (FMT-BREAK-022, FMT-INDENT-025, FMT-INDENT-026). A positional pattern is a list of subpatterns behind a bracket that stays on the line of its `is` or its type, which is the shape of a list pattern, so it takes the list pattern's layout.

## Visible output changes

Whole-document formatting; "Before" is the output on `main`, observed with `scripts/format.sh`, and stable on a second pass.

Input:

```csharp
class P
{
    public void Deconstruct(out int a, out int b)
    {
        a = 0;
        b = 0;
    }
}

class C
{
    bool M(object o, (int, int) t)
    {
        var a = o is P(1,
            2);
        var b = t is (
            1,
            2
            );
        var c = o is P
            (1, 2);
        var d = t is (1
            , 2);
        var e = o is P(1,
            2

            // last
            );
        var f = o is (1, 1) and (2,
            3);

        return a && b && c && d && e && f;
    }

    int K((int, int) t)
    {
        return t switch
        {
            (1,
                2) => 1,
            _ => 0
        };
    }

    void N(object o)
    {
        switch (o)
        {
            case (1,
                2):
                break;
        }
    }
}
```

Before: the input, unchanged except for the switch expression, whose braces and arms align to `t` (FMT-INDENT-027); the subpatterns and the `)` are continuation lines in column 12, in the arm in column 23, and in the `case` label in column 16:

```csharp
    int K((int, int) t)
    {
        return t switch
               {
                   (1,
                       2) => 1,
                   _ => 0
               };
    }
```

After:

```csharp
class P
{
    public void Deconstruct(out int a, out int b)
    {
        a = 0;
        b = 0;
    }
}

class C
{
    bool M(object o, (int, int) t)
    {
        var a = o is P(
                          1,
                          2
                      );
        var b = t is (
                         1,
                         2
                     );
        var c = o is P(1, 2);
        var d = t is (
                         1,
                         2
                     );
        var e = o is P(
                          1,
                          2

                          // last
                      );
        var f = o is (1, 1)
                  and (
                          2,
                          3
                      );

        return a && b && c && d && e && f;
    }

    int K((int, int) t)
    {
        return t switch
               {
                   (
                       1,
                       2
                   ) => 1,
                   _ => 0
               };
    }

    void N(object o)
    {
        switch (o)
        {
            case (
                     1,
                     2
                 ):
                break;
        }
    }
}
```

### Code fixes

RH5306's fix formats the list pattern it reports, and a positional subpattern inside it that spans several lines gets the new layout. No other fix was found that formats a pattern on its own; a fix that formats a statement or a member holding a positional pattern, such as RH5302's, gives it the layout whole-document formatting gives.

No rule page changes. RH5306 reports list patterns and is unchanged; the formatter may lay out positional patterns without a rule that reports them ([P11](../../README.md#the-formatter-may-do-more-than-the-analyzers-p11)).

## Decisions

### A wrapped positional pattern is laid out like a list pattern

The maintainer chose A for question 7, the issue's reading A — content one level inside the parentheses, `)` under the `(`, `(` on the line that opens it — which is the list pattern's layout.

### The `(` joins the pattern's type

Decided with question 7 (A). The clause belongs to its type as an argument list belongs to its name (FMT-BREAK-039).

### A comma that starts a line moves behind its subpattern

Decided with question 7 (A). Every list FMT-BREAK-022 lays out moves such a comma (FMT-BREAK-038).

### A multi-line positional pattern next to a combinator wraps the chain

The maintainer chose A for question 7b: FMT-BREAK-017 already wraps a chain next to a multi-line property, list, or parenthesized pattern, and the positional pattern joins that list; otherwise `o is (1, 1) and (2,⏎3)` would keep `and (` on the line of `(1, 1)` and open the subpatterns behind it.

### A comment in front of the `)` goes into the subpatterns' column

Follows from question 7 (A): the positional sentence of FMT-INDENT-006 — a comment in front of the `)` goes into the subpatterns' column — rests on the subpatterns standing one level deeper than the `)`, which the chosen layout gives, so it stays in FMT-INDENT-006's text and example.

## Alternatives considered

- **Lay a positional pattern out like an argument list** (FMT-BREAK-012, FMT-INDENT-012): the first subpattern on the `(` line, every later one aligned behind the `(`, the `)` behind the last subpattern. Shown as reading B in the question; rejected by the maintainer: the pattern family would hold two layouts for delimited lists of subpatterns.
- **Lay it out like a parenthesized pattern**, with the `(` on a line of its own below the `is` or the type — not taken: a positional pattern's `(` belongs to its type, and below a type it would read as a separate pattern.
- **Keep today's continuation lines** (the issue's reading C) — rejected by the maintainer: the subpatterns stand left of their own parentheses.
- **Keep a combinator on its line next to a multi-line positional pattern** — rejected by the maintainer: the chain would be wrapped next to a multi-line list pattern and not next to a positional one.
- **Keep the `(` of a parenthesized pattern on the `is` line** (the issue's reading A for parenthesized patterns) — not taken up: the maintainer decided the opposite.

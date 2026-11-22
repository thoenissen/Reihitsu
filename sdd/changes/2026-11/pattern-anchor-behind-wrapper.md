# The braces of a property pattern behind `and`, `or`, or `not` are anchored to the outermost pattern

| | |
|---|---|
| Status | Accepted |
| Date | 2026-11 |
| Affected requirements | FMT-INDENT-026, examples of FMT-BREAK-017, FMT-BREAK-022, and FMT-BREAK-041 |

## What

FMT-INDENT-026 anchors the delimiters of a multi-line property or parenthesized pattern, skipping `and`, `or`, and `not` wrappers. Where no `is`, subpattern, or parenthesized pattern introduces the pattern — in a switch-expression arm, in an element of a list pattern, and in a `case` label —, the anchor is now stated as the first token of the outermost pattern those wrappers lead to: the `P` of `P and { … }`, the `not` of `not { … }`. Before, the text read "the pattern's own first token", which can be read as the brace itself.

In a switch-expression arm and in an element of a list pattern, the brace of such a pattern starts its own line (FMT-BREAK-022); taken as its own anchor, it has no column, and on `main` it lands left of the arm or the list (thoenissen/Reihitsu#1929, a bug against either reading). In a `case` label the brace stays behind the `and` or `not` (FMT-BREAK-022), and on `main` the content and the `}` take the brace's own column; with the stated anchor they move under the `P` or the `not`, as they already stand for `case P { … }`.

## Why

A brace that starts its line cannot be its own anchor. The outermost pattern is what the arm or the label introduces, and the pattern with a type, `P { … }`, already puts its braces under its first token; `P and { … }` and `not { … }` then read the same way. One reading for arms and labels keeps one layout for one tree.

## Visible output changes

Whole-document formatting; "Before" is the output on `main`, observed with `scripts/format.sh`, and stable on a second pass.

Input:

```csharp
class P
{
    public int A { get; set; }

    public int B { get; set; }
}

class C
{
    int M(object o)
    {
        return o switch
        {
            P and { A: 1,
                B: 2 } => 1,
            not { A: 3,
                B: 4 } => 2,
            _ => 0
        };
    }

    bool L(object[] a)
    {
        return a is [P and { A: 1,
            B: 2 }];
    }

    void N(object o)
    {
        switch (o)
        {
            case P and {
            A: 1
            }:
                break;

            case not {
            A: 2
            }:
                break;

            case P and not {
            A: 3
            }:
                break;
        }
    }
}
```

Before:

```csharp
class P
{
    public int A { get; set; }

    public int B { get; set; }
}

class C
{
    int M(object o)
    {
        return o switch
               {
                   P
                   and
                {
                    A: 1,
                    B: 2
                } => 1,
                   not
                {
                    A: 3,
                    B: 4
                } => 2,
                   _ => 0
               };
    }

    bool L(object[] a)
    {
        return a is [
                        P
                        and
                {
                    A: 1,
                    B: 2
                }
                    ];
    }

    void N(object o)
    {
        switch (o)
        {
            case P
                 and {
                         A: 1
                     }:
                break;

            case not {
                         A: 2
                     }:
                break;

            case P
                 and not {
                             A: 3
                         }:
                break;
        }
    }
}
```

After:

```csharp
class P
{
    public int A { get; set; }

    public int B { get; set; }
}

class C
{
    int M(object o)
    {
        return o switch
               {
                   P
                   and
                   {
                       A: 1,
                       B: 2
                   } => 1,
                   not
                   {
                       A: 3,
                       B: 4
                   } => 2,
                   _ => 0
               };
    }

    bool L(object[] a)
    {
        return a is [
                        P
                        and
                        {
                            A: 1,
                            B: 2
                        }
                    ];
    }

    void N(object o)
    {
        switch (o)
        {
            case P
                 and {
                     A: 1
                 }:
                break;

            case not {
                     A: 2
                 }:
                break;

            case P
                 and not {
                     A: 3
                 }:
                break;
        }
    }
}
```

The switch-expression arms are the bug thoenissen/Reihitsu#1929 reports, and the element of the list pattern is a member of the same bug that the issue does not name: in both, the brace starts its own line at column 16, left of the arm's first token (column 19) and of the list's `[` (column 21). The `case` labels are the visible change this clarification adds: there the label introduces the pattern, the brace stays behind `and` or `not` (FMT-BREAK-022), and the content and the `}` move under the `P` or the `not`. `case P and not {`, with two wrappers, moves furthest (content from column 29 to 21). `case 1⏎or {` and `case 1⏎or not {` move the same way, under the `1` (the examples of FMT-BREAK-017 and FMT-BREAK-041). A property pattern inside a list pattern in a `case` label is not introduced by the label (FMT-BREAK-022), so it is laid out as in the `is` expression above, whatever the `case`-label reading.

Two more members fail on `main` the same way and follow from the same text (observed with `scripts/format.sh`): the slice of a list pattern (`[1, .. not { A: 1,⏎A: 2 }]`, brace at column 16, left of the `[` at 21; afterwards under the `not`), and a parenthesized pattern behind `not` in an arm (`not (1 or⏎2) => 1`, `(` at column 16, left of the arm at 19; afterwards under the `not`, its content four columns right of the `(`).

### Code fixes

RH5206's fix formats the switch expression it anchors, so the arms move with it: on `return o switch⏎    {⏎        P and { A: 1,⏎            B: 2 } => 1, …`, the `{` and `}` go to column 16 on `main` (observed with `scripts/apply-fix.sh`) and to column 19, under the `P`, after this change.

No rule page changes.

## Decisions

### The anchor is the first token of the outermost pattern

The maintainer chose A for question 8, which also covers the patterns a `case` label introduces: their content and `}` take the outermost pattern's column, not the brace's own column as on `main`. For the switch-expression arm, the brace's own column is no column, so the outermost pattern's first token is the only reading that gives one.

## Alternatives considered

- **Anchor to the outermost pattern only where the brace starts its line, and to the brace elsewhere** (today's `case` label output) — rejected by the maintainer: the column of the content would depend on where the author put the brace's line break, `case P and {` would differ from `case P {`, and the rule would need a condition on the layout of another phase.

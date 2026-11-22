# The delimiters of a property or parenthesized pattern behind `and`, `or`, or `not` are anchored to the outermost pattern

| | |
|---|---|
| Status | Accepted |
| Date | 2026-11 |
| Affected requirements | FMT-INDENT-026, examples of FMT-BREAK-017, FMT-BREAK-022, and FMT-BREAK-041 |

## What

FMT-INDENT-026 anchors the delimiters of a multi-line property or parenthesized pattern, skipping `and`, `or`, and `not` wrappers. Where no `is`, subpattern, or parenthesized pattern introduces the pattern — in a switch-expression arm, in an element of a list pattern, and in a `case` label —, the anchor is now stated as the first token of the outermost pattern those wrappers lead to: the `P` of `P and { … }`, the `not` of `not { … }`. Before, the text read "the pattern's own first token", which can be read as the brace itself.

In a switch-expression arm and in an element of a list pattern, the brace of such a pattern starts its own line (FMT-BREAK-022); taken as its own anchor, it has no column, and on `main` it lands left of the arm or the list (thoenissen/Reihitsu#1929, a bug against either reading). In a `case` label the brace stays behind the `and` or `not` (FMT-BREAK-022), and on `main` the content and the `}` take the brace's own column; with the stated anchor they move under the `P` or the `not`, as they already stand for `case P { … }`.

The same anchor holds for every pattern behind a wrapper, not only for braces without a type: the braces of a typed property pattern, `P { … } or Q { … }` and `not Q { … }`, stand under the first token of the outermost pattern, the `P` or the `not`, and not under the `Q`; and the parentheses of a parenthesized pattern, `0 or (…)` and `not (…)`, stand there too, in an arm, a list-pattern element, a slice, and a `case` label (maintainer answer 9A).

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
    int M(P o)
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

    void N(P o)
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
    int M(P o)
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

    void N(P o)
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
    int M(P o)
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

    void N(P o)
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

### Typed property patterns and parenthesized patterns behind a wrapper

The anchor also moves the members below (maintainer answer 9A). For a typed property pattern behind a wrapper, and for a parenthesized pattern behind a wrapper in a `case` label, `main` gives a real column — the column of the type behind the wrapper, or the `case` keyword's column plus four for the `(` — and the change moves them to the first token of the outermost pattern. For a parenthesized pattern behind a wrapper in an arm, a list-pattern element, or a slice, `main` puts the `(` left of the arm or the list, the same bug as the braces above. Whole-document formatting, observed with the CLI built from `main` and from this change, each stable on a second pass.

Input:

```csharp
class P
{
    public int A { get; set; }

    public int B { get; set; }
}

class Q
{
    public int A { get; set; }

    public int B { get; set; }
}

class S
{
    public int Length => 0;

    public object this[int index] => null;

    public object Slice(int start, int length)
    {
        return null;
    }
}

class C
{
    int M(object o)
    {
        return o switch
        {
            P { A: 1,
                B: 2 } or Q { A: 1,
                B: 2 } => 1,
            null or P { A: 5,
                B: 6 } => 2,
            not Q { A: 3,
                B: 4 } => 3,
            _ => 0
        };
    }

    int K(int i)
    {
        return i switch
        {
            0
            or
            (1 or
            2) => 1,
            _ => 0
        };
    }

    bool L(S a, S b)
    {
        return a is [1, not Q { A: 1,
            B: 2 }]
            && a is [1, .. not Q { A: 1,
            B: 2 }]
            && b is [1, not (1 or
            2)]
            && b is [1, .. not (1 or
            2)];
    }

    void N(object o, int i)
    {
        switch (o)
        {
            case null or P { A: 1,
            B: 2 }:
                break;

            case not Q { A: 1,
            B: 2 }:
                break;
        }

        switch (i)
        {
            case > 5 and (< 10 or
            > 20):
                break;

            case 3 or (4 or
            5):
                break;

            case not (1 or
            2):
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

class Q
{
    public int A { get; set; }

    public int B { get; set; }
}

class S
{
    public int Length => 0;

    public object this[int index] => null;

    public object Slice(int start, int length)
    {
        return null;
    }
}

class C
{
    int M(object o)
    {
        return o switch
               {
                   P
                   {
                       A: 1,
                       B: 2
                   }
                   or Q
                      {
                          A: 1,
                          B: 2
                      } => 1,
                   null
                   or P
                      {
                          A: 5,
                          B: 6
                      } => 2,
                   not Q
                       {
                           A: 3,
                           B: 4
                       } => 3,
                   _ => 0
               };
    }

    int K(int i)
    {
        return i switch
               {
                   0
                   or
                (
                    1
                    or 2
                ) => 1,
                   _ => 0
               };
    }

    bool L(S a, S b)
    {
        return a is [
                        1,
                        not Q
                            {
                                A: 1,
                                B: 2
                            }
                    ]
               && a is [
                           1,
                           .. not Q
                                  {
                                      A: 1,
                                      B: 2
                                  }
                       ]
               && b is [
                           1,
                           not
                    (
                        1
                        or 2
                    )
                       ]
               && b is [
                           1,
                           .. not
                    (
                        1
                        or 2
                    )
                       ];
    }

    void N(object o, int i)
    {
        switch (o)
        {
            case null
                 or P {
                        A: 1,
                        B: 2
                    }:
                break;

            case not Q {
                         A: 1,
                         B: 2
                     }:
                break;
        }

        switch (i)
        {
            case > 5
                 and
                (
                    < 10
                    or > 20
                ):
                break;

            case 3
                 or
                (
                    4
                    or 5
                ):
                break;

            case not
                (
                    1
                    or 2
                ):
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

class Q
{
    public int A { get; set; }

    public int B { get; set; }
}

class S
{
    public int Length => 0;

    public object this[int index] => null;

    public object Slice(int start, int length)
    {
        return null;
    }
}

class C
{
    int M(object o)
    {
        return o switch
               {
                   P
                   {
                       A: 1,
                       B: 2
                   }
                   or Q
                   {
                       A: 1,
                       B: 2
                   } => 1,
                   null
                   or P
                   {
                       A: 5,
                       B: 6
                   } => 2,
                   not Q
                   {
                       A: 3,
                       B: 4
                   } => 3,
                   _ => 0
               };
    }

    int K(int i)
    {
        return i switch
               {
                   0
                   or
                   (
                       1
                       or 2
                   ) => 1,
                   _ => 0
               };
    }

    bool L(S a, S b)
    {
        return a is [
                        1,
                        not Q
                        {
                            A: 1,
                            B: 2
                        }
                    ]
               && a is [
                           1,
                           .. not Q
                              {
                                  A: 1,
                                  B: 2
                              }
                       ]
               && b is [
                           1,
                           not
                           (
                               1
                               or 2
                           )
                       ]
               && b is [
                           1,
                           .. not
                              (
                                  1
                                  or 2
                              )
                       ];
    }

    void N(object o, int i)
    {
        switch (o)
        {
            case null
                 or P {
                     A: 1,
                     B: 2
                 }:
                break;

            case not Q {
                     A: 1,
                     B: 2
                 }:
                break;
        }

        switch (i)
        {
            case > 5
                 and
                 (
                     < 10
                     or > 20
                 ):
                break;

            case 3
                 or
                 (
                     4
                     or 5
                 ):
                break;

            case not
                 (
                     1
                     or 2
                 ):
                break;
        }
    }
}
```

The members, with the column of the opening delimiter (0-based) before and after:

- typed property pattern behind `or` in an arm, `P { … } or Q { … }`: the braces of `Q { … }` from under `Q` (22) to under `P` (19);
- typed property pattern behind `not` in an arm, `not Q { … }`: from under `Q` (23) to under `not` (19);
- typed property pattern behind `or` with a non-property left operand in an arm, `null or P { … }`: from under `P` (22) to under `null` (19);
- parenthesized pattern behind `or` in an arm, `0⏎or⏎(…)`: `(` from 16, left of the arm, to under `0` (19), its content four columns right of it;
- typed property pattern behind `not` in a list-pattern element, `[1, not Q { … }]`: from under `Q` (28) to under `not` (24);
- typed property pattern behind `not` in a slice, `[1, .. not Q { … }]`: from under `Q` (34) to under `not` (30);
- parenthesized pattern behind `not` in a list-pattern element, `[1, not (…)]`: `(` from 20, left of the element, to under `not` (27);
- parenthesized pattern behind `not` in a slice, `[1, .. not (…)]`: `(` from 20 to under `not` (30);
- typed property pattern behind `not` in a `case` label, `case not Q {`: the brace stays behind `Q`; the content from 25 to 21 and the `}` from 21 to 17, under `not`;
- typed property pattern behind `or` in a `case` label, `case null⏎or P {`: the content from 24 to 21 and the `}` from 20 to 17, under `null`;
- parenthesized pattern behind `not`, `and`, or `or` in a `case` label, `case not (`, `case > 5⏎and (`, `case 3⏎or (`: `(` and `)` from 16 (`case` plus four) to 17, under the first token of the label's pattern, the content from 20 to 21.

Patterns whose first token is unchanged keep their columns: the left operand of `and`/`or`, a pattern with no wrapper (`P { … }`, `case P {`), a pattern introduced by `is`, a subpattern, and a pattern inside a parenthesized pattern.

### Code fixes

RH5206's fix formats the switch expression it anchors, so the arms move with it: on `return o switch⏎    {⏎        P and { A: 1,⏎            B: 2 } => 1, …`, the `{` and `}` go to column 16 on `main` (observed with `scripts/apply-fix.sh`) and to column 19, under the `P`, after this change.

The typed members move with it: on `return o switch⏎    {⏎        P { A: 1,⏎            B: 2 } or Q { A: 1 } => 1,⏎        not Q { A: 3,⏎            B: 4 } => 2, …`, the braces of `not Q` go to column 19, under the `not`, after this change (observed with `scripts/apply-fix.sh`), where they stood under the `Q` (23) on `main`. No other code fix formats a pattern behind a wrapper on its own: RH5306 formats a list pattern, whose elements anchor as in the whole document.

No rule page changes.

## Decisions

### The anchor is the first token of the outermost pattern

The maintainer chose A for question 8, which also covers the patterns a `case` label introduces: their content and `}` take the outermost pattern's column, not the brace's own column as on `main`. For the switch-expression arm, the brace's own column is no column, so the outermost pattern's first token is the only reading that gives one.

### Typed property patterns and parenthesized patterns behind a wrapper follow the same anchor

The maintainer chose A for question 9: a typed property pattern behind `and`, `or`, or `not`, and a parenthesized pattern behind a wrapper in a `case` label, are anchored like the untyped braces of answer 8A — to the first token of the arm, the list element, the slice's pattern, or the pattern the `case` label introduces. On `main` these members had a real column (under the type, or `case` plus four), but one anchor for every pattern behind a wrapper keeps one rule: the delimiters of `Q { … }` and `( … )` stand where those of `{ … }` stand behind the same wrapper. The parenthesized patterns behind a wrapper in an arm, a list-pattern element, or a slice had no column on `main` and follow 8A.

## Alternatives considered

- **Keep `main`'s column where it had one** — read the outermost pattern only where `main` gave the delimiter no column, and keep the type's column for `not Q { … }` and `case`-plus-four for `case not (` — rejected by the maintainer (question 9, option B): the anchor would depend on whether the pattern behind the wrapper has a type, so `not { … }` and `not Q { … }` in the same arm would place their braces differently.
- **Anchor to the outermost pattern only where the brace starts its line, and to the brace elsewhere** (today's `case` label output) — rejected by the maintainer: the column of the content would depend on where the author put the brace's line break, `case P and {` would differ from `case P {`, and the rule would need a condition on the layout of another phase.

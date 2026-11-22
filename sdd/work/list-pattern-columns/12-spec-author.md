Role: spec author
Commit: d6e83c81d1b835bee83bf21fc296aa29281f4614
Verdict: spec written

Repair round 1, thoenissen/Reihitsu#1929, audit finding #1 (`11-auditor.md`). The maintainer answered question 9 "as recommended" (9A, `00-lead.md`, *Maintainer answers*). My prompt carried no intended fix, suspected cause, or hoped-for verdict beyond naming that answer.

No production change is needed: the head already gives 9A for every member (the CLI built from `d6e83c8`, each output stable on a second pass). The "Before" outputs were taken with the CLI built from `75cc39f` (`main`).

## Changed requirements

- FMT-INDENT-026 — Property patterns and parenthesized patterns align to their introducer — clarified. The normative text now names the members explicitly: a typed property pattern behind a wrapper (`P { … } or Q { … }`, `not Q { … }`: the anchor is the `P` or the `not`, not the `Q`) and a parenthesized pattern behind a wrapper (`0 or ( … )`, `not ( … )`, `case > 5 and ( … )`: the anchor is the `0`, the `not`, or the `>`, not the parenthesis or the `case` keyword), in an arm, a list-pattern element or slice, and a `case` label. A new example covers them.

Change doc `sdd/changes/2026-11/pattern-anchor-behind-wrapper.md` (stays `Accepted`): title widened to "delimiters of a property or parenthesized pattern"; *What* states the 9A members; *Visible output changes* has a new section listing each member with before/after columns; *Code fixes* adds RH5206's result for a typed arm (`not Q {`: braces from 23 to 19, observed with `scripts/apply-fix.sh`); *Decisions* records answer 9A; *Alternatives considered* records option B as rejected by the maintainer.

## Examples for the tests

FMT-INDENT-026, the last example ("behind `and`, `or`, and `not`, the braces of a typed property pattern and the parentheses of a parenthesized pattern …"). The same input/output pair is in the change doc. Each member below is a separate small test. "main" is the 0-based column of the opening delimiter on `main`, "head" the column required.

| Member | Context | main | required |
|---|---|---|---|
| `P { … }⏎or Q { … }` — braces of `Q { … }` | arm | 22 (under `Q`) | 19 (under `P`), content 23 |
| `not Q { … }` | arm | 23 (under `Q`) | 19 (under `not`) |
| `null⏎or P { … }` | arm | 22 (under `P`) | 19 (under `null`) |
| `0⏎or⏎( 1⏎or 2 )` (8A follower) | arm | 16 (left of the arm) | 19 (under `0`), content 23 |
| `[1, not Q { … }]` | list element | 28 (under `Q`) | 24 (under `not`) |
| `[1, .. not Q { … }]` | slice | 34 (under `Q`) | 30 (under `not`) |
| `[1, not ( 1⏎or 2 )]` (8A follower) | list element | 20 (left of the element) | 27 (under `not`), content 31 |
| `[1, .. not ( 1⏎or 2 )]` (8A follower) | slice | 20 | 30 (under `not`) |
| `case not Q {` — content / `}` | `case` label | 25 / 21 | 21 / 17 (under `not`) |
| `case null⏎or P {` — content / `}` | `case` label | 24 / 20 | 21 / 17 (under `null`) |
| `case not (` — `(` and `)` / content | `case` label | 16 / 20 | 17 / 21 (under `not`) |
| `case > 5⏎and (` | `case` label | 16 / 20 | 17 / 21 (under `>`) |
| `case 3⏎or (` | `case` label | 16 / 20 | 17 / 21 (under `3`) |

The input and the required output, the same pair as the spec example:

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

class C
{
    int M(object o)
    {
        return o switch
        {
            P { A: 1,
                B: 2 } or Q { A: 1,
                B: 2 } => 1,
            not Q { A: 3,
                B: 4 } => 2,
            null or P { A: 5,
                B: 6 } => 3,
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

    bool L(object[] a, int[] b)
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
            case not Q { A: 1,
            B: 2 }:
                break;

            case null or P { A: 1,
            B: 2 }:
                break;
        }

        switch (i)
        {
            case not (1 or
            2):
                break;

            case > 5 and (< 10 or
            > 20):
                break;

            case 3 or (4 or
            5):
                break;
        }
    }
}
```

Required output:

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
                   not Q
                   {
                       A: 3,
                       B: 4
                   } => 2,
                   null
                   or P
                   {
                       A: 5,
                       B: 6
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

    bool L(object[] a, int[] b)
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
            case not Q {
                     A: 1,
                     B: 2
                 }:
                break;

            case null
                 or P {
                     A: 1,
                     B: 2
                 }:
                break;
        }

        switch (i)
        {
            case not
                 (
                     1
                     or 2
                 ):
                break;

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
        }
    }
}
```

Code fix (RH5206, `Verify`): input

```csharp
class P
{
    public int A { get; set; }

    public int B { get; set; }
}

class Q
{
    public int A { get; set; }
}

class C
{
    int M(object o)
    {
        return o switch
            {
                P { A: 1,
                    B: 2 } or Q { A: 1 } => 1,
                not Q { A: 3,
                    A: 4 } => 2,
                _ => 0
            };
    }
}
```

fixes to (the braces of `not Q` under `not`, column 19):

```csharp
class P
{
    public int A { get; set; }

    public int B { get; set; }
}

class Q
{
    public int A { get; set; }
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
                   or Q { A: 1 } => 1,
                   not Q
                   {
                       A: 3,
                       A: 4
                   } => 2,
                   _ => 0
               };
    }
}
```

Members that keep their columns, already covered (`PropertyPatternsWithoutAWrapperKeepTheirAnchors` and others): the left operand of `and`/`or`, a pattern with no wrapper, a pattern introduced by `is`, a subpattern, a pattern inside a parenthesized pattern. The first `P { … }` of the arm in the example above is such a left operand.

## Output questions

_None._ Question 9 is answered (9A).

## Internal choices

- I used one combined example in FMT-INDENT-026 for all 9A members rather than one example per member; the alternative was several small examples. The table above splits it for the tests.

## Follow-up candidates

_None._

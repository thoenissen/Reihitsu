Role: spec author
Commit: 72974f9
Verdict: change doc written

Follow-up round on `00-lead.md` decisions 3–6 and the skeptic's amendments (`02-skeptic.md`). My prompt carried no intended fix or hoped-for verdict. The inputs still hold one: thoenissen/Reihitsu#1929 suggests where `PatternAnchor` should read its fallback. I ignored it, as in round 1.

Every "Before" below is the output of `scripts/format.sh` on this branch, whose `src/` equals `origin/main`. Each was stable on a second pass. Fixtures are under `/tmp/claude-0/-home-user-Reihitsu/86292ddd-4907-5234-9281-ccf5f9f4a5ca/scratchpad/q/`. Every example block I added or changed in the specs and change docs was formatted as well. Every block parses. Inputs that the run leaves unchanged match today's output. The "After" blocks of changes not yet built differ from today's output only in the columns and joins under question.

## Changed requirements

- FMT-INDENT-036 — A first base type kept below the colon takes the column behind the colon — retired (in the change that added it; decision 3)
- FMT-INDENT-037 — A first element kept below its opening bracket aligns behind the bracket — retired (same)
- FMT-INDENT-038 — A line inside a qualified name is indented one level deeper than the name — retired (same; it would also have moved comment-free qualified names in casts, `typeof`, `default`, using aliases, and namespace names)
- FMT-INDENT-034 — clarified: a first base type below the `:`, a first element below the opening bracket of an attribute, type argument, type parameter, or function-pointer parameter list, and the right part of a qualified name below its `.` are continuation lines when a comment keeps them there. FMT-INDENT-034 places them, and the `Why:` records why they get no column of their own (the rare class: RH5043, RH5044, RH5046, RH5104). New example. This closes #1634 (decision 5).
- FMT-BREAK-042 — changed: a comment-held first base type is a continuation line (FMT-INDENT-034), and the later types align under it (FMT-INDENT-016). The reference to FMT-INDENT-036 is gone. In the comment example's "After", the type is in column 4. The enum example has no trailing comma any more (skeptic).
- FMT-BREAK-017 — changed: a recursive pattern whose positional clause spans several lines wraps the chain like a list pattern (7b, on recommendation A). New example.
- FMT-INDENT-026 — example extended: an element of a list pattern (`a is [P and { A: 1,⏎B: 2 }]`) and `case P and not {`. The requirement text names the list-pattern element.
- `line-breaks.md` `Uses`: FMT-INDENT-036 removed.

Change docs:
- `sdd/changes/2026-11/comment-held-type-argument-and-qualified-name.md` — deleted (decision 5: no visible output change remains).
- `sdd/changes/2026-11/base-list-colon-wrap.md` — the comment-held column is dropped. A *Decisions* entry records that comment-held lines keep the continuation indent, and that FMT-INDENT-036, -037, and -038 were retired before they shipped. The enum example is fixed. The visible changes are now 1 (join) and 2 (comma first).
- `sdd/changes/2026-11/positional-pattern-layout.md` — adds the switch-expression arm, the FMT-BREAK-017 member (7b), and the dependency of FMT-INDENT-006's positional sentence on question 7.
- `sdd/changes/2026-11/pattern-anchor-behind-wrapper.md` — adds the list-pattern element and `case P and not {`. Two more failing members are named, both observed on `main`: the slice (`[1, .. not { A: 1,⏎A: 2 }]`, brace at column 16) and a parenthesized pattern behind `not` in an arm (`(` at column 16). The *Decisions* entry states that the other reading changes only patterns a `case` label introduces.

## Examples for the tests

FMT-INDENT-034 (the new comment-held example: a characterization test, unchanged output), FMT-BREAK-042 (examples as amended), FMT-BREAK-017 (positional example), FMT-INDENT-026 (arm, list-pattern element, `case` labels including `and not`), and the slice and the parenthesized-behind-`not` members from the anchor change doc. The tests of round 1's list that named FMT-INDENT-036, -037, or -038 are dropped. A retired requirement is never tagged.

## Output questions

Final form for the batch. Question numbers and option labels are kept from round 1 (questions 2, 4, and 5 are dropped by decision 3).

**1 · Formatter — a base list wrapped behind its colon**

Input:
```csharp
interface I1;

interface I2;

class C :
    I1, I2
{
    int _x;
}

class D :
    I1,
    I2
{
    int _x;
}

class E : // c
    I1,
    I2
{
    int _x;
}

record R(int X) :
    Q(X);

record Q(int X);
```

Before (`main`): unchanged; every base type stays in column 4.

After, A (recommended):
```csharp
interface I1;

interface I2;

class C : I1, I2
{
    int _x;
}

class D : I1,
          I2
{
    int _x;
}

class E : // c
    I1,
    I2
{
    int _x;
}

record R(int X) : Q(X);

record Q(int X);
```

After, B: unchanged, as on `main`.

Why A: a wrapped `where` clause already joins its first constraint onto the colon's line, and the later base types then align under the first; a comment behind the colon keeps today's layout under both options.

**3 · Formatter — a comma that starts a line in a base list**

Input:
```csharp
interface I1;

interface I2;

class C : I1
    , I2
{
    int _x;
}
```

Before (`main`): unchanged; `, I2` stays in column 4.

After, A (recommended):
```csharp
interface I1;

interface I2;

class C : I1,
          I2
{
    int _x;
}
```

After, B:
```csharp
interface I1;

interface I2;

class C : I1
          , I2
{
    int _x;
}
```

After, C: unchanged, as on `main`.

Why A: argument lists, parameter lists, initializers, collection expressions, and list and property patterns all move such a comma behind the element in front of it.

**6 · Formatter — a comment in front of the closing `]` of a list pattern or the closing `)` of a parenthesized pattern**

Input:
```csharp
class C
{
    bool M(int[] a, object o)
    {
        var b = a is [
                          1,

                      // last
                      ];
        var c = o is (1 or
            2

            // last
            );

        return b && c;
    }
}
```

Before (`main`):
```csharp
class C
{
    bool M(int[] a, object o)
    {
        var b = a is [
                         1,

                     // last
                     ];
        var c = o is
                  (
                      1
                      or 2

                  // last
                  );

        return b && c;
    }
}
```

After, A (recommended):
```csharp
class C
{
    bool M(int[] a, object o)
    {
        var b = a is [
                         1,

                         // last
                     ];
        var c = o is
                  (
                      1
                      or 2

                      // last
                  );

        return b && c;
    }
}
```

After, B:
```csharp
class C
{
    bool M(int[] a, object o)
    {
        var b = a is [
                         1,

                         // last
                     ];
        var c = o is
                  (
                      1
                      or 2

                  // last
                  );

        return b && c;
    }
}
```

After, C: unchanged, as on `main`.

Why A: a comment in front of a collection expression's `]` or a closing brace already goes into the content it closes; at the delimiter's column it reads as describing the delimiter.

**7 · Formatter — a wrapped positional pattern**

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

        return a && b && c && d;
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

Before (`main`):
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

        return a && b && c && d;
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

After, A (recommended): laid out like a list pattern.
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

        return a && b && c && d;
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

After, B: laid out like an argument list.
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
        var b = t is (1,
                      2
                      );
        var c = o is P(1, 2);
        var d = t is (1,
                      2);

        return a && b && c && d;
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

After, C: unchanged, as on `main`.

Why A: every other delimited pattern (list, property, parenthesized) puts one subpattern per line inside its delimiters, and a positional pattern's `(` stays on its line, as a list pattern's `[` does.

**7b · Formatter — a combinator next to a wrapped positional pattern** (asked only if 7 = A)

Input:
```csharp
class C
{
    bool M(object o)
    {
        return o is (1, 1) and (2,
            3);
    }
}
```

Before (`main`): unchanged.

After, A (recommended):
```csharp
class C
{
    bool M(object o)
    {
        return o is (1, 1)
                 and (
                         2,
                         3
                     );
    }
}
```

After, B:
```csharp
class C
{
    bool M(object o)
    {
        return o is (1, 1) and (
                                   2,
                                   3
                               );
    }
}
```

Why A: next to a wrapped list, property, or parenthesized pattern, the combinator already starts its own line.

**8 · Formatter — a multi-line property pattern behind `and` or `not` in a switch arm, a list pattern, or a `case` label**

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

Before (`main`):
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

After, A (recommended):
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

After, B: the arm and the list-pattern element change as in A; the patterns a `case` label introduces stay as on `main`.
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

Why A: `case P { A: 1 }` written over several lines already puts its `}` under `P`; A gives `P and { … }` and `not { … }` the same layout in a label as in an arm.

## Internal choices

- The retirements of FMT-INDENT-037 and FMT-INDENT-038 are recorded in their `Status: Retired` lines and in `base-list-colon-wrap.md`'s *Decisions*, which covers all three IDs as one decision. No change doc of their own: relative to `main`, no requirement is retired and no output changes, and FMT-INDENT-034 gains a clarification (`sdd/README.md`, *When a change doc is required*). Alternative: keep a small #1634 change doc that records only the retirements.
- The clarification is written in FMT-INDENT-034's own text, among "the lines this requirement places", and the reason is in its `Why:`. The alternatives were the indentation spec's non-goals, or a sentence in FMT-INDENT-015/016. FMT-INDENT-034 is the requirement that owns these lines.
- Decision 4: round 1's internal choice is withdrawn. It had weighed refusing a comment behind a base list's `:` through FMT-UNSUP-017. No spec or change doc carries it; `base-list-colon-wrap.md` names it only as a rejected alternative, because the shape deletes nothing.
- 7b is written into FMT-BREAK-017 on recommendation A. If 7 is not A, that sentence and its example are removed, together with the positional sentence of FMT-INDENT-006 (recorded in `positional-pattern-layout.md`).
- Q3 gains option C (unchanged). The labels A and B are kept.
- FMT-INDENT-026's requirement sentence names the list-pattern element but not the slice. The slice follows from the text by cause, with the `not` as its anchor, and is listed in the change doc. Naming it in the requirement would add a member that has no example in the spec.

## Follow-up candidates

_None new._ Round 1's and the skeptic's candidates stand as listed in `00-lead.md`.

## Whole run

- `scripts/test.sh --project architecture`: 197 passed, 1 failed. The failure is `EveryRequirementOfAnEnforcedSpecIsCovered`, for FMT-BREAK-042 and FMT-BREAK-043 only, which need their tests from the test implementer. The retired IDs and the ledger pass.
- Rule pages: none changed.
- Code owners: unchanged.

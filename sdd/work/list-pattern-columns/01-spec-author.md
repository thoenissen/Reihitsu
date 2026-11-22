Role: spec author
Commit: 77747a7
Verdict: #1633: change doc written; #1634: change doc written; #1647: change doc written; #1685: spec written; #1542: change doc written; #1929: change doc written

Every "Before" below is today's output of `scripts/format.sh` on the branch base (identical to `main` for the formatter), stable on a second pass; code-fix results were observed with `scripts/apply-fix.sh`. Every example block of the new and changed requirements and of the five change docs was run through `scripts/format.sh --dry-run`: all parse (no syntax-error or unsupported skip); the unchanged parts of each "After" match today's output.

Nothing in my prompt carried an intended fix. Two inputs did: thoenissen/Reihitsu#1929 suggests where `PatternAnchor` should read its fallback, and `00-lead.md` records the coordinator's expectation that #1929 needs no output question. I ignored both; #1929 turned out to need one (question 8).

## thoenissen/Reihitsu#1633

### Changed requirements

- FMT-BREAK-042 — A first base type wrapped behind the colon joins the colon's line — added
- FMT-INDENT-036 — A first base type kept below the colon takes the column behind the colon — added
- FMT-BREAK-038 — A comma that starts a line in an initializer, a collection expression, a pattern, or a base list ends the element's line — changed (base-list commas; title)
- FMT-BREAK-041 — clarified: the gap behind a base list's `:` is named as joined by FMT-BREAK-042
- `indentation.md` non-goals — the base list leaves the comma-column non-goal

Change doc: `sdd/changes/2026-11/base-list-colon-wrap.md`.

The issue's own "Before" is stale in part: the `:` that starts a line now joins the type's line (FMT-BREAK-041), but a first base type behind a colon that ends its line still exists. FMT-BREAK-041 names that gap as one it does not join, so today the base types get the continuation indent (FMT-INDENT-034), column 4 — the issue's reading B. The question is restated against that output.

### Examples for the tests

FMT-BREAK-042 (no comment: class with two types, with later types wrapped, enum, record with an argument list; comment behind the colon blocks), FMT-INDENT-036 (one and two types behind a comment; first type behind the colon, FMT-INDENT-016 only), FMT-BREAK-038 (the base-list half of its new example). The code-fix result of RH5412 named in the change doc (`class C :⏎    I1⏎{⏎}` gives `class C : I1;` after the change).

### Output questions

Questions 1, 2, 3 below (end of file).

### Internal choices

- A new requirement FMT-BREAK-042 rather than another gap in FMT-BREAK-041: every gap of FMT-BREAK-041 is refused when a comment stands in it (FMT-UNSUP-017); this one is not refused, a comment blocks it (FMT-BREAK-001), as for the constraint clause (FMT-BREAK-037). Alternative: add the gap to FMT-BREAK-041 and FMT-UNSUP-017, which would refuse every file with `class C : // c`.
- FMT-BREAK-038 extended rather than a new requirement: same decision, same reason.
- Only the first base type is joined; later line breaks stay (no issue or requirement asks for more).

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1634

### Changed requirements

- FMT-INDENT-037 — A first element kept below its opening bracket aligns behind the bracket — added
- FMT-INDENT-038 — A line inside a qualified name is indented one level deeper than the name — added

Change doc: `sdd/changes/2026-11/comment-held-type-argument-and-qualified-name.md`.

The issue's field example is refused today (FMT-UNSUP-004: a comment inside a variable declaration), so the examples use a property. "Before" is FMT-INDENT-034's continuation indent, as the issue's comment says.

### Examples for the tests

FMT-INDENT-037 (property type, constraint, parameter type, object creation, type parameter list with a comment on its own line; the attribute-list case in the change doc's prose), FMT-INDENT-038 (property type, constraint). The RH5110 code-fix result in the change doc (`T>,` column 22 → 30).

### Output questions

Questions 4 and 5 below.

### Internal choices

- FMT-INDENT-037 covers every list of FMT-INDENT-015 (attribute lists, type argument, type parameter, function-pointer parameter lists), by cause, not only angle-bracket lists as the issue's title says. Alternative: angle-bracket lists only. The attribute-list output is shown in question 4.
- FMT-INDENT-037 also places a first element inside a parameter (`void N(List< // c⏎int> a)`, today column 11 under the parameter, by FMT-INDENT-013): FMT-INDENT-013 leaves a nested list to itself. Shown in question 4.
- FMT-INDENT-038 yields to FMT-INDENT-013 inside list elements (the base-list case `class B : System. // c⏎IDisposable` keeps aligning with `System`), so it changes only lines that today get the continuation indent. The consequence: a name's right part is aligned inside a list element and one level deeper elsewhere. Alternative: one rule everywhere, which changes FMT-INDENT-013's base-list example.

### Follow-up candidates

**Formatter — a qualified name wrapped behind its dot in a member's type is not joined**

Input:
```csharp
using System;

class C
{
    System.
        IDisposable P => null;
}
```

Before (`main`): unchanged. The same wrap inside a constraint (`where T : System.⏎IDisposable`) is joined to `where T : System.IDisposable`.

After, A (recommended):
```csharp
using System;

class C
{
    System.IDisposable P => null;
}
```

After, B: unchanged; FMT-INDENT-038 places the line four columns right of `System`.

Why A: nothing stands in the gap, and the same wrap is joined in a constraint. Outside the run: #1634 is about lines a comment holds.

## thoenissen/Reihitsu#1647

### Changed requirements

- FMT-INDENT-006 — A comment on its own line takes the column of the code that follows it — changed: a comment in front of the closing delimiter of a list pattern or a positional pattern goes one level deeper than the delimiter, and in front of a parenthesized pattern's `)` into the pattern's column (four right of `(`); stated by cause (the delimiter closes content indented deeper than it), with the argument-list `)` named as not reached.

Change doc: `sdd/changes/2026-11/pattern-closing-delimiter-comment.md`.

### Examples for the tests

FMT-INDENT-006 (the new list-pattern / positional / parenthesized example).

### Output questions

Question 6 below.

### Internal choices

- The parenthesized pattern's `)` is included: same shape, same cause, today the comment stays in the `)` column (observed). Alternative: list pattern only, as the issue names. Shown in question 6.
- No rule page change: RH5306 does not report the comment alone.

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1685

### Changed requirements

- FMT-INDENT-034 — an example added (a `case` label's `when` condition wrapped behind `when`); no requirement text changes.

No change doc: today's output already is the issue's reading A (`Check(value):` one level deeper than `case`), produced by FMT-INDENT-034, which was merged after the issue was filed. Observed with `scripts/format.sh`; stable. Also observed: `case 2 when Check(value) &&⏎Check(value + 1):` aligns `&&` with its left operand (FMT-INDENT-018), and `case 3 // c⏎when …` keeps FMT-INDENT-021.

### Examples for the tests

FMT-INDENT-034 (the new `case 1 when⏎Check(value):` example) — a characterization test; nothing visible changes.

### Output questions

_None._ The issue's purpose is met on `main`.

### Internal choices

- No clarifying sentence in FMT-INDENT-021: FMT-INDENT-034 already places the line; an example there is enough. Alternative: a sentence in FMT-INDENT-021 pointing at FMT-INDENT-034.

### Follow-up candidates

- Spec wording only, no output: FMT-INDENT-021's `Why:` says the guard must not reach "the column of the section's statements, one level deeper", while its own example puts `when` one level deeper than `case`, the statements' column. The `Why:` reads against the example. Not changed here.

## thoenissen/Reihitsu#1542

### Changed requirements

- FMT-BREAK-022 — Wrapped collection expressions and patterns get one element per line — changed: the positional pattern clause is added; its `(` stays on the line in front of it; new example. The `case`-label example's columns are also changed (see #1929).
- FMT-BREAK-043 — The positional pattern clause starts on the line of its type — added
- FMT-BREAK-038 — changed (positional-pattern commas)
- FMT-INDENT-025 — Collection expressions, list patterns, and positional patterns indent from their bracket — changed (positional pattern added, title, new example)
- FMT-INDENT-006 — changed (positional `)`, with #1647)
- `indentation.md` non-goals — the positional pattern leaves the comma-column non-goal

Change doc: `sdd/changes/2026-11/positional-pattern-layout.md`.

The parenthesized half of the issue is decided on `main` by maintainer answers (the `(` keeps its own line below `is`, batch 2 answer 10 A; content four columns right of `(`, answer 1e A), and the formatter produces it (observed for `o is (⏎1 or 2⏎)`, `(1⏎or 2)`, `(int⏎or long)`, and `case (int⏎or long):`). The issue's reading A for parenthesized patterns — keep `(` on the `is` line — contradicts those answers; I dropped it (agent-filed goal; output stays as an unchanged requirement demands). `o is not⏎(1 or 2)` is joined (FMT-BREAK-041); `{ X:⏎1 }` gets the continuation indent inside FMT-BREAK-022's layout. Only the positional pattern is open.

### Examples for the tests

FMT-BREAK-022 (positional example), FMT-BREAK-043 (`o is P⏎(1, 2)`), FMT-BREAK-038 (positional half of its new example), FMT-INDENT-025 (positional with type, `case` label, behind `&& t is`), FMT-INDENT-006 (positional `)` comment).

### Output questions

Question 7 below.

### Internal choices

- The parenthesized goal is dropped as described above.
- The positional pattern is not added to FMT-BREAK-017's list of operands that wrap a chain (a multi-line property, list, or parenthesized pattern next to a combinator). Alternative: add it; see follow-up candidates.

### Follow-up candidates

**Formatter — a multi-line positional pattern next to a combinator does not wrap the chain** (only once question 7 is answered A)

Input:
```csharp
class C
{
    bool M(object o)
    {
        return o is [1] and [2,
            3];
    }
}
```

The list pattern makes `and` start its own line today (FMT-BREAK-017). With question 7 answered A, `o is (1, 1) and (2,⏎3)` would keep `and` on the line and give `and (⏎2,⏎3⏎)`, because FMT-BREAK-017 lists only property, list, and parenthesized patterns. Readings: A (recommended) add the positional pattern to that list, so `and` starts its own line as for a list pattern; B keep the list. The lead may instead fold this into question 7.

## thoenissen/Reihitsu#1929

### Changed requirements

- FMT-INDENT-026 — Property patterns and parenthesized patterns align to their introducer — clarified: where no `is`, subpattern, or parenthesized pattern introduces it, the anchor is the first token of the outermost pattern the `and`/`or`/`not` wrappers lead to (`P` of `P and { … }`, `not` of `not { … }`); `Why:` extended; new example (switch-expression arms and `case` labels).
- Examples of FMT-BREAK-017, FMT-BREAK-022, and FMT-BREAK-041 — the `case P⏎and {`, `case 1⏎or {`, `case not {`, and `case 1⏎or not {` outputs change columns under that reading (content one level deeper than `P`/`1`/`not`, `}` under it).

Change doc: `sdd/changes/2026-11/pattern-anchor-behind-wrapper.md`.

The arms are a bug under every reading: today the brace lands in column 16, left of the arm's `P` in column 19 (observed). But the spec's "the pattern's own first token" has two readings with different output in a `case` label, where the brace stays behind `and`/`not`: today the content and `}` take the brace's own column (and the specs' examples show that), while the outermost-pattern reading puts them under `P`/`not`, as `case P { … }` already does. So #1929 is not a bug fix alone: the `case`-label part is question 8, and I wrote the spec on the recommendation.

### Examples for the tests

FMT-INDENT-026 (new arm and `case` example), FMT-BREAK-017 and FMT-BREAK-041 `case` examples (changed columns), FMT-BREAK-022 `case` example (changed columns). The RH5206 result in the change doc (brace column 16 → 19). Both line endings, per the issue.

### Output questions

Question 8 below.

### Internal choices

- Where the delimited pattern is itself outermost and its opening delimiter starts its line (`{` at the start of an arm, `(` below `case`), the line's own requirement places the delimiter and the content follows from it; stated in the `Why:`. Today's output for those already does so.

### Follow-up candidates

_None._

## Output questions

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

record R(int X) :
    Q(X);

record Q(int X);
```

Before (`main`): unchanged; the base types stay in column 4.

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

record R(int X) : Q(X);

record Q(int X);
```

After, B: unchanged, as on `main`.

Why A: a wrapped `where` clause already joins its first constraint onto the colon's line, and the later base types then align under the first.

**2 · Formatter — a first base type that a comment keeps below the colon**

Input:
```csharp
interface I1;

interface I2;

class D : // c
    I1, // d
    I2
{
    int _x;
}
```

Before (`main`): unchanged; `I1, // d` and `I2` in column 4.

After, A (recommended):
```csharp
interface I1;

interface I2;

class D : // c
          I1, // d
          I2
{
    int _x;
}
```

After, B: unchanged, as on `main`.

Why A: the base types keep the column they have without the comment, as constraints and argument lists do.

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

Before (`main`): unchanged; `, I2` in column 4.

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

Why A: every other list — arguments, parameters, initializers, collection expressions, list and property patterns — moves such a comma behind the element in front of it.

**4 · Formatter — a first type argument, type parameter, or attribute that a comment keeps below the bracket**

Input:
```csharp
using System;
using System.Collections.Generic;

class C
{
    List< // c
        int> P => null;

    void M<T>()
        where T : class, IComparable< // c
            T>, new()
    {
    }

    void N(List< // c
        int> a)
    {
    }
}

[ // c
    Obsolete]
class D<
    // c
    T>
{
}
```

Before (`main`):
```csharp
using System;
using System.Collections.Generic;

class C
{
    List< // c
        int> P => null;

    void M<T>()
        where T : class,
                  IComparable< // c
                      T>,
                  new()
    {
    }

    void N(List< // c
           int> a)
    {
    }
}

[ // c
    Obsolete]
class D<
    // c
    T>
{
}
```

After, A (recommended):
```csharp
using System;
using System.Collections.Generic;

class C
{
    List< // c
         int> P => null;

    void M<T>()
        where T : class,
                  IComparable< // c
                              T>,
                  new()
    {
    }

    void N(List< // c
                int> a)
    {
    }
}

[ // c
 Obsolete]
class D<
        // c
        T>
{
}
```

After, B: unchanged, as on `main`.

Why A: the first element takes the column every later element of the list already takes, as a comment-held first argument aligns behind its parenthesis.

**5 · Formatter — a line inside a qualified name**

Input:
```csharp
using System;

class C
{
    System. // c
        IDisposable P => null;

    void N<T>()
        where T : System. // c
            IDisposable
    {
    }
}
```

Before (`main`): unchanged; `IDisposable` in column 8 in the property and in column 12 in the constraint, left of `System` (column 18).

After, A (recommended): one level deeper than the name's first token.
```csharp
using System;

class C
{
    System. // c
        IDisposable P => null;

    void N<T>()
        where T : System. // c
                      IDisposable
    {
    }
}
```

After, B: aligned with the name's first token (the issue's suggestion).
```csharp
using System;

class C
{
    System. // c
    IDisposable P => null;

    void N<T>()
        where T : System. // c
                  IDisposable
    {
    }
}
```

After, C: unchanged, as on `main`.

Why A: under B a part of a member's type stands in the member's column and reads as a member of its own; under C it stands left of the name in a constraint.

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

After, A (recommended): both comments move into the content's column.
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

After, B: only the list pattern's comment moves (column 25); the parenthesized pattern's stays in column 18.

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

Before (`main`): unchanged; every line below a `(` stands in column 12 (column 16 in the `case` label), left of its own parentheses.

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

Why A: every other delimited pattern (list, property, parenthesized) gets one subpattern per line inside its delimiters, and a positional pattern's `(` stays on its line as a list pattern's `[` does.

**8 · Formatter — a multi-line property pattern behind `and` or `not` in a switch arm or a `case` label**

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
            _ => 0
        };
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
                   _ => 0
               };
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
        }
    }
}
```

After, A (recommended): the braces stand under the arm's or the label's first token in both places.
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
                   _ => 0
               };
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
        }
    }
}
```

After, B: the arm as in A; the `case` labels unchanged, as on `main`.

Why A: `case P { A: 1 }` written over several lines already puts its `}` under `P`; A gives `P and { … }` and `not { … }` the same layout in a label as in an arm.

## Whole run

- `scripts/test.sh --project architecture`: 197 passed, 1 failed — `EveryRequirementOfAnEnforcedSpecIsCovered`, for exactly the five new requirements (FMT-INDENT-036, FMT-INDENT-037, FMT-INDENT-038, FMT-BREAK-042, FMT-BREAK-043), which need their tests from the test implementer. The `Uses` rows (indentation.md: + FMT-BREAK-001, FMT-BREAK-022, FMT-BREAK-042, FMT-BREAK-043; line-breaks.md: + FMT-INDENT-016, FMT-INDENT-036) and the ledger pass.
- Rule pages: none changed. No rule page shows any of these shapes; RH5043 (comments in a base list) and RH5306 (list patterns) report what they reported before.
- Code owners: unchanged. New behavior in the line-break phase (FMT-BREAK-042, FMT-BREAK-043) may land in a type not yet listed in `line-breaks.md`; the implementer adds it there and to `sdd/specs/README.md`.
- Every change doc is `Status: Draft`; its `Decisions` sections say "Pending the maintainer's answer" for each listed visible change.

Role: spec author
Commit: 65ea14d
Verdict: change doc written

Amendment round for decisions 5 and 6 of `00-lead.md`. The prompt carried no intended fix or hoped-for verdict. The skeptic's proposed wording was taken as a suggestion; the wording below is re-derived.

## What changed

Spec (`sdd/specs/formatter/pipeline.md`):

- FMT-PIPE-017 — the context target's other lines are now required to end where the indentation phase puts them "when it lays the target out as if that token stood in that column", and a new sentence states that where the root has the target's first token, and what the phases write in front of it there, is not written back and decides no column. No claim that any phase puts the token back in its document column within the root.
- FMT-PIPE-017 — list of ways: the first way now includes the join (first parameter on the line after `(`) and names field, `for`-initializer, and first-declarator-after-its-type targets; the second way names the parameter and the enum member behind `{` (line break) and the declarator, parameter, and enum member behind more than one space (spacing); the third way names the parameter behind a moved raw-string closing line. The "other side" paragraph names a target on a later line whose column the layout keeps, a construct one space behind its sibling, the first parameter directly behind `(`, and a local function directly or one space behind its label.
- FMT-PIPE-017 — three new examples: the join (`void M(` / `int a = 1` / `+ 2` under `1` in column 16), the enum member behind `{` (`+` under `1` in column 13), and the declarator behind its type and four spaces (`2` under `1` in column 25).
- FMT-PIPE-017 Why — "a result the alignment rules report" narrowed: the next formatting moves it, and an alignment rule reports it only where one covers the line (RH5203, RH5301), per the skeptic's measurements.
- FMT-PIPE-018 — the sentence "the indentation phase gives that token its column in the document …, which changes at most the whitespace directly in front of it" removed. Now: the phases lay out the target's other lines from the fact's column, not from the token's column in the root; the write-back changes no line and no column the phases decided — for a line-start target the written-back whitespace puts the token in the fact's column, and for a mid-line target what stands in front is written back as in the document, whatever the horizontal-spacing phase or a raw-string move does to it in the root.
- FMT-PIPE-024 — the indentation bullet now lays out the marked target's lines "as if that target's first token stood in that fact's column", and states that no phase other than the horizontal-spacing phase decides the spaces in front of that token and that its column in the root decides no column of the result. The line-break bullet ("neither breaking … nor joining") is unchanged (decision 6).
- `AGENTS.md` architecture sentence: "keep … the target's first token where the document has them" replaced by "lay out the target from the column its first token has in the document", so it no longer states the mechanism.
- SHARED-FIX-007 and RH5111-004 already stated results only; unchanged in this round. Rule pages unchanged.

Change doc (`sdd/changes/2026-11/context-target-first-line.md`):

- *What*: the indentation bullet restated as a result; new paragraph that the root's place of the target's first token decides nothing and the horizontal-spacing phase stays the only owner of the gap; the RH5111 caller bullet lists the join, the break behind `{`, and the spacing cases.
- *Why*: the claim that "the alignment rules report" every case narrowed to RH5203 (change 1) and RH5301 (change 2); no rule reports the binary-operator and local-function cases.
- *Visible output changes*: changes 7–18 added (p2, f1, j3, t1, j1, p3, s1, s3, s2, s5, s6, s7 of `02-skeptic.md`), each with input, before, and after. Every "before" re-measured by me with `scripts/apply-fix.sh RH5111` at `65ea14d` (code equal to `main` `21c2ff4`), LF and CRLF identical; every one matches the skeptic's columns except s7, where my fixture gives `"y"` in column 24 (the skeptic reported 28 for a different fixture); the change doc uses my fixture.
- *Unchanged* line corrected: "the first parameter behind `(`" became "directly behind `(`"; added one-space siblings, a later-line parameter whose column the layout keeps, the first parameter on the line after `(` where the join keeps its column (p1), and a declarator behind a multi-line comment's last line (s4).
- *Alternatives*: added "have the indentation phase put the token back in its document column within the root" — rejected (same-line gap owned by the horizontal-spacing phase; impossible behind a moved raw-string closing line).

Checks: `scripts/test.sh --project architecture --filter "FullyQualifiedName~Reihitsu.ArchitectureTests.Sdd"` — 51 passed. No new requirement IDs.

## Changed requirements

- FMT-PIPE-017 — Node-level targets take their columns from the indentation phase — changed (this round: result wording, ways list, three examples, Why)
- FMT-PIPE-018 — The context entry point writes back its target's leading trivia as in the document — changed (this round: mechanism sentence replaced by result)
- FMT-PIPE-024 — Node-level input facts are plain values — changed (this round: indentation bullet)
- SHARED-FIX-007, RH5111-004 — changed in round 1; unchanged in this round

## Examples for the tests

- FMT-PIPE-017: every example, including the three new ones (join; enum member behind `{`; declarator behind its type and four spaces).
- The change doc's changes 1–18 are all reached by the RH5111 or RH3208 fix and are the code-fix-level cases; FMT-PIPE-018's two examples keep their output.
- Skeptic's trivia cases (FC-004/019/020/021) and the "must stay" members (p1, p4, p5, f2, j4, j5, p6, p7, s4, t2) are not examples in the spec; the test implementer takes them from `02-skeptic.md`.

## Output questions

All within thoenissen/Reihitsu#1501, and all reached by a code fix (the RH5111 fix or the RH3208 fix; the target and context of each are named in the change doc). This is the complete, current list: every visible output change in the change doc. Questions 1 and 4 have a second reading besides keeping today's output; the others have only one output consistent with FMT-PIPE-010 besides today's, because what stands in front of the target is outside it.

**1 · RH5111 · Assignments must have proper line breaks — a declarator on a later line of its declaration, with a wrapped value**

Input:
```csharp
class C
{
    static int Other(int x, int y)
    {
        return x + y;
    }

    void M()
    {
        int a = 1,
              b =
                  Other(1,
                        2);
    }
}
```

Before (`main`):
```csharp
        int a = 1,
              b = Other(1,
                  2);
```

After, A (recommended):
```csharp
        int a = 1,
              b = Other(1,
                        2);
```

After, B (the declarator's first line in the column formatting the document gives it):
```csharp
        int a = 1,
        b = Other(1,
                  2);
```

After, C: today's output (Before) stays.

Why A: the fix keeps the construct's first token where the user wrote it, as every other formatting code fix does, and the wrapped argument follows it; B rewrites the whitespace in front of the declarator, which the fix keeps for every construct (FMT-PIPE-018), and C leaves `2` off `1`, which RH5203 reports.

**2 · RH5111 · Assignments must have proper line breaks — a declarator on a later line, whose value is an object creation or a lambda**

Input:
```csharp
        C a = null,
              b =
                  new C
                  {
                      P = 1
                  };
        Action c = null,
              d =
                  () =>
                  {
                      M();
                  };
```

Before (`main`):
```csharp
        C a = null,
              b = new C
            {
                P = 1
            };
        Action c = null,
              d = () =>
            {
                M();
            };
```

After, A (recommended):
```csharp
        C a = null,
              b = new C
                  {
                      P = 1
                  };
        Action c = null,
              d = () =>
                  {
                      M();
                  };
```

After, B: today's output (Before) stays.

Why A: the braces stay under `new` and `(` as formatting gives them; B leaves them off, which RH5301 reports for the object creation.

**3 · RH5111 · Assignments must have proper line breaks — an enum member and a parameter on a later line, with a wrapped value**

Input:
```csharp
enum E
{
    A = 1,
      B =
      1 +
      2
}

class C
{
    void M(int a,
                string c
               = "x" +
                 "y")
    {
    }
}
```

Before (`main`):
```csharp
enum E
{
    A = 1,
      B = 1
        + 2
}

class C
{
    void M(int a,
                string c = "x"
                      + "y")
    {
    }
}
```

After, A (recommended):
```csharp
enum E
{
    A = 1,
      B = 1
          + 2
}

class C
{
    void M(int a,
                string c = "x"
                           + "y")
    {
    }
}
```

After, B: today's output (Before) stays.

Why A: the wrapped part stays under the anchor on the construct's first line, which keeps its place; B leaves it off that anchor, and the next formatting moves it.

**4 · RH5111 · Assignments must have proper line breaks — a parameter behind another parameter on its line, with a wrapped value**

Input:
```csharp
class C
{
    void M(int a,
           int b, string c
               = "x" +
                 "y")
    {
    }
}
```

Before (`main`):
```csharp
    void M(int a,
           int b, string c = "x"
                      + "y")
```

After, A (recommended):
```csharp
    void M(int a,
           int b, string c = "x"
                             + "y")
```

After, B (a line break in front of the parameter, as formatting the document gives):
```csharp
    void M(int a,
           int b,
           string c = "x"
                      + "y")
```

After, C: today's output (Before) stays.

Why A: the fix changes only the reported parameter's own lines and keeps it on the line it shares with `int b`; B also rewrites the gap behind `int b,`, outside what the fix formats, and C leaves `+` off `"x"`.

**5 · RH5111 · Assignments must have proper line breaks — a declarator behind the closing line of a raw string literal that formatting moves**

Input:
```csharp
class C
{
    void M()
    {
        string a = """
            x
            """, b =
            "y" +
            "z";
    }
}
```

Before (`main`):
```csharp
        string a = """
            x
            """, b = "y"
                            + "z";
```

After, A (recommended):
```csharp
        string a = """
            x
            """, b = "y"
                     + "z";
```

After, B: today's output (Before) stays.

Why A: `+` stays under `"y"`; B leaves it under the place `"y"` would have after the literal moved, which the fix does not write.

**6 · RH3208 · Expression style local functions should not be used — a local function behind a label and several spaces, with wrapped parameters**

Input:
```csharp
class C
{
    void M()
    {
        L:    int F(int a,
                    int b) => a + b;
    }
}
```

Before (`main`):
```csharp
        L:    int F(int a,
                 int b)
        {
            return a + b;
        }
```

After, A (recommended):
```csharp
        L:    int F(int a,
                    int b)
        {
            return a + b;
        }
```

After, B: today's output (Before) stays.

Why A: `int b` stays under `int a`; B leaves it three columns off.

**7 · RH5111 · Assignments must have proper line breaks — the first declarator on the line after its type, with a wrapped value**

Input:
```csharp
class C
{
    static int Other(int x, int y)
    {
        return x + y;
    }

    void M()
    {
        int
            a =
                Other(1,
                      2);
    }
}
```

Before (`main`):
```csharp
        int
            a = Other(1,
                  2);
```

After, A (recommended):
```csharp
        int
            a = Other(1,
                      2);
```

After, B: today's output (Before) stays.

Why A: the wrapped part stays under the anchor on the construct's first line, which keeps its place; B leaves it off that anchor, and the next formatting moves it.

**8 · RH5111 · Assignments must have proper line breaks — a field declarator on a later line of its declaration, with a wrapped value**

Input:
```csharp
class C
{
    static int Other(int x, int y)
    {
        return x + y;
    }

    private static int _a = 1,
                       _b =
                           Other(1,
                                 2);
}
```

Before (`main`):
```csharp
    private static int _a = 1,
                       _b = Other(1,
               2);
```

After, A (recommended):
```csharp
    private static int _a = 1,
                       _b = Other(1,
                                  2);
```

After, B: today's output (Before) stays.

Why A: the wrapped part stays under the anchor on the construct's first line, which keeps its place; B leaves it off that anchor, and the next formatting moves it.

**9 · RH5111 · Assignments must have proper line breaks — a declarator on a later line of a `for` initializer, with a wrapped value**

Input:
```csharp
class C
{
    static int Other(int x, int y)
    {
        return x + y;
    }

    void M()
    {
        for (int i = 0,
                 j =
                     Other(1,
                           2); i < j; i++)
        {
        }
    }
}
```

Before (`main`):
```csharp
        for (int i = 0,
                 j = Other(1,
                  2); i < j; i++)
```

After, A (recommended):
```csharp
        for (int i = 0,
                 j = Other(1,
                           2); i < j; i++)
```

After, B: today's output (Before) stays.

Why A: the wrapped part stays under the anchor on the construct's first line, which keeps its place; B leaves it off that anchor, and the next formatting moves it.

**10 · RH5111 · Assignments must have proper line breaks — a declarator on a later line of a tab-indented declaration, with a wrapped value**

Shown with `→` for a tab; every other indentation is spaces.

Input:
```csharp
class C
{
→static int Other(int x, int y)
→{
→→return x + y;
→}

→void M()
→{
→→int a = 1,
→→    b =
→→        Other(1,
→→              2);
→}
}
```

Before (`main`):
```csharp
→→int a = 1,
→→    b = Other(1,
            2);
```

After, A (recommended):
```csharp
→→int a = 1,
→→    b = Other(1,
                2);
```

After, B: today's output (Before) stays.

Why A: `2` stays under `1` with each tab counted as one column, as every node-level fix counts a tab it keeps; B leaves it four columns off. Formatting the document later replaces the tabs and moves both lines together.

**11 · RH5111 · Assignments must have proper line breaks — the first parameter on the line after `(`, with a wrapped value**

Input:
```csharp
class C
{
    void M(
        int a
            = 1 +
              2)
    {
    }
}
```

Before (`main`):
```csharp
    void M(
        int a = 1
                   + 2)
```

After, A (recommended):
```csharp
    void M(
        int a = 1
                + 2)
```

After, B: today's output (Before) stays.

Why A: the wrapped part stays under the anchor on the construct's first line, which keeps its place; B leaves it off that anchor, and the next formatting moves it.

**12 · RH5111 · Assignments must have proper line breaks — an enum member behind `{` on its line, with a wrapped value**

Input:
```csharp
enum E { A =
           1 +
           2 }
```

Before (`main`):
```csharp
enum E { A = 1
        + 2 }
```

After, A (recommended):
```csharp
enum E { A = 1
             + 2 }
```

After, B: today's output (Before) stays.

Why A: the wrapped part stays under the anchor on the construct's first line, which keeps its place; B leaves it off that anchor, and the next formatting moves it.

**13 · RH5111 · Assignments must have proper line breaks — the first declarator behind its type and several spaces, with a wrapped value**

Input:
```csharp
class C
{
    static int Other(int x, int y)
    {
        return x + y;
    }

    void M()
    {
        int    a =
            Other(1,
                  2);
    }
}
```

Before (`main`):
```csharp
        int    a = Other(1,
                      2);
```

After, A (recommended):
```csharp
        int    a = Other(1,
                         2);
```

After, B: today's output (Before) stays.

Why A: the wrapped part stays under the anchor on the construct's first line, which keeps its place; B leaves it off that anchor, and the next formatting moves it.

**14 · RH5111 · Assignments must have proper line breaks — the first parameter behind `(` and several spaces, with a wrapped value**

Input:
```csharp
class C
{
    void M(   int a
                  = 1 +
                    2)
    {
    }
}
```

Before (`main`):
```csharp
    void M(   int a = 1
                   + 2)
```

After, A (recommended):
```csharp
    void M(   int a = 1
                      + 2)
```

After, B: today's output (Before) stays.

Why A: the wrapped part stays under the anchor on the construct's first line, which keeps its place; B leaves it off that anchor, and the next formatting moves it.

**15 · RH5111 · Assignments must have proper line breaks — a parameter behind another parameter and several spaces, with a wrapped value**

Input:
```csharp
class C
{
    void M(int a,    string c
                         = "x" +
                           "y")
    {
    }
}
```

Before (`main`):
```csharp
    void M(int a,    string c = "x"
                      + "y")
```

After, A (recommended):
```csharp
    void M(int a,    string c = "x"
                                + "y")
```

After, B: today's output (Before) stays.

Why A: the wrapped part stays under the anchor on the construct's first line, which keeps its place; B leaves it off that anchor, and the next formatting moves it.

**16 · RH5111 · Assignments must have proper line breaks — a declarator behind another declarator and several spaces, with a wrapped value**

Input:
```csharp
class C
{
    static int Other(int x, int y)
    {
        return x + y;
    }

    void M()
    {
        int a = 1,    b =
            Other(1,
                  2);
    }
}
```

Before (`main`):
```csharp
        int a = 1,    b = Other(1,
                             2);
```

After, A (recommended):
```csharp
        int a = 1,    b = Other(1,
                                2);
```

After, B: today's output (Before) stays.

Why A: the wrapped part stays under the anchor on the construct's first line, which keeps its place; B leaves it off that anchor, and the next formatting moves it.

**17 · RH5111 · Assignments must have proper line breaks — an enum member behind another enum member and several spaces, with a wrapped value**

Input:
```csharp
enum E
{
    A = 1,    B =
        1 +
        2
}
```

Before (`main`):
```csharp
    A = 1,    B = 1
               + 2
```

After, A (recommended):
```csharp
    A = 1,    B = 1
                  + 2
```

After, B: today's output (Before) stays.

Why A: the wrapped part stays under the anchor on the construct's first line, which keeps its place; B leaves it off that anchor, and the next formatting moves it.

**18 · RH5111 · Assignments must have proper line breaks — a parameter behind the closing line of a raw string literal that formatting moves, with a wrapped value**

Input:
```csharp
class C
{
    void M(string a = """
        x
        """, string c =
        "y" +
        "z")
    {
    }
}
```

Before (`main`):
```csharp
    void M(string a = """
        x
        """, string c = "y"
                      + "z")
```

After, A (recommended):
```csharp
    void M(string a = """
        x
        """, string c = "y"
                        + "z")
```

After, B: today's output (Before) stays.

Why A: the wrapped part stays under the anchor on the construct's first line, which keeps its place; B leaves it off that anchor, and the next formatting moves it.

## Internal choices

- FMT-PIPE-018/024 are stated as results; the mechanism (whether the indentation phase moves the token in the root, or the horizontal-spacing phase keeps the gap) is left to the implementer. Alternative (skeptic's): have the horizontal-spacing phase keep the gap in front of the target — rejected, it fails for the raw-string move and adds a fact the result does not need.
- The line-break bullet of FMT-PIPE-024 ("neither breaking nor joining") is kept as a mechanism: the line-break phase owns breaks and joins, and the bullet keeps the target's leading trivia intact for FMT-PIPE-018. Alternative: drop it and rely on the result wording — rejected, a break or join in front of a line-start target edits its leading trivia, which FMT-PIPE-018 forbids the phases.
- Three of the twelve new cases became spec examples (join, break behind `{`, spacing); the rest stay in the change doc only, to keep FMT-PIPE-017 readable. Alternative: an example per case.
- The questions' option B/C "today's output stays" is listed so the maintainer can reject a change individually; the recommendation is A throughout.

## Follow-up candidates

_None._

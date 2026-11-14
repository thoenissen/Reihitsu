Role: spec author
Commit: 5f577b0
Verdict: #1529: open question (change doc written, Draft); #1543: open question (change doc written, Draft)

No spec requirement is written yet: for both issues the competing readings change different requirements (and different surfaces), and an undecided case is not written into a spec (`sdd/README.md`, *Open questions*). Each change doc is written on the recommended reading with every visible output change of it; the spec diff follows the maintainer's answer. My prompt carried no intended fix or hoped-for verdict.

Reproductions: CLI (`scripts/format.sh`) and `scripts/apply-fix.sh` on scratch fixtures, LF and CRLF, at 5f577b0.

## thoenissen/Reihitsu#1529

### Changed requirements

None yet. Planned on reading B: one RH5111 requirement added (next free ID `RH5111-006`), with the three examples below. Change doc: `sdd/changes/2026-11/conditional-access-assignment-fix-layout.md` (Draft).

### Examples for the tests

None until the answer. On B, the three inputs below give the RH5111 code-fix tests; whole-document formatting is unchanged and already gives the target output.

### Established facts

- `a?.B.C = 5` parses as a conditional access whose branch is the assignment `.B.C = 5`; the RH5111 fix formats that assignment, so its first token is the `.` of `.B`.
- RH5111 fix today: `a?.B⏎  .C =⏎    5;` → `a?.B⏎  .C = 5;` (`.C` col 10); `a?[0]⏎  .C =⏎    5;` → `a?[0]⏎  .C = 5;`; `a?.B?.B⏎    .C =⏎    5;` → `.C` moved from col 12 to col 13.
- Whole-document formatting of the same inputs: `.C` in col 9 (under `?`) for the simple and the nested case; `a?[0].C = 5;` joined. It treats a called link the same way: `a?.B⏎  .N();` → `.N()` in col 9; `a?[0]⏎  .N();` → `a?[0].N();`. The issue's claim that the fix's column "is the one the chain alignment uses elsewhere" does not hold.
- The RH5111 fix's output is reported by layout rules: RH5201 on the simple and nested results (its fix moves `.C` to col 9), RH5112 on the element-binding result (its fix joins to `a?[0].C = 5;`). Whole-document formatting's output is silent under RH5111, RH5112, RH5201, and RH6023.
- RH6023 is out of the question: its fix now changes only the gaps next to the operator (SHARED-OPSPACE-004). `a?.B⏎  .C=5;` → `a?.B⏎  .C = 5;`, nested keeps col 12, element binding keeps the wrap, also with the value wrapped (`.C=⏎    5;` → `.C =⏎    5;`). Every reading leaves RH6023's output as it is; the issue anticipated this.
- Today's fix output was chosen by the maintainer in `sdd/changes/2026-10/node-root-detached.md` (Decisions, repair round 2), on the stated premise that it matches whole-document formatting. That premise is false for the column and the element binding, so changing it reverses a maintainer answer: an output question, not a bug fix.

### Output questions

**1 · RH5111 · Assignments must have proper line breaks — member wrapped behind a conditional access**

Input:
```csharp
class A
{
    public A B;
    public A this[int i] => null;
    public int C;
}

class D
{
    void M(A a)
    {
        a?.B
          .C =
            5;
        a?[0]
          .C =
            5;
        a?.B?.B
            .C =
            5;
    }
}
```

Before (`main`), RH5111 fix:
```csharp
        a?.B
          .C = 5;
        a?[0]
          .C = 5;
        a?.B?.B
             .C = 5;
```
Whole-document formatting on `main` gives B's output below.

After, A — whole-document formatting adopts the fix's layout (formatter changes; RH5201 and RH5112 must stop reporting it):
```csharp
        a?.B
          .C = 5;
        a?[0]
          .C = 5;
        a?.B?.B
             .C = 5;
```

After, B (recommended) — the fix adopts whole-document formatting's layout (fix changes; formatter, RH5201, RH5112 unchanged):
```csharp
        a?.B
         .C = 5;
        a?[0].C = 5;
        a?.B?.B
         .C = 5;
```

After, C — both stay as they are; the RH5111 fix's result keeps being reported by RH5201 (cases 1 and 3) and RH5112 (case 2).

Why B: whole-document formatting already lays out this chain like every other chain (`a?.B⏎ .N();`), and the layout rules agree with it; only the fix differs, and its result is reported again by RH5201 or RH5112.

Requirement IDs touched: RH5111-004, SHARED-FIX-007; on A also the chain alignment and line-break requirements of `shared/fluent-chain-layout.md` and RH5201/RH5112.

### Internal choices

- One change doc for #1529, separate from #1543's: the two issues share no cause.
- On B, the requirement goes into RH5111's spec rather than SHARED-FIX-007 or `fluent-chain-layout.md`: only the RH5111 fix formats an assignment that is a conditional-access branch with a member wrapped (RH6023 no longer formats). Alternative: an example under SHARED-FIX-007.

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1543

### Changed requirements

None yet. Planned on reading A: FMT-BLANK-020 changed — its token list becomes the set FMT-BLANK-007, FMT-BLANK-008, and FMT-BLANK-019 name (every brace other than the opening brace of a block statement after a statement, FMT-BLANK-017 unchanged; `else`, `catch`, `finally`, do-statement `while`; the closing bracket and parenthesis it names today), and the sentence "whether it should remove it there too is not decided" goes. On B: RH5024, RH5025, RH5026, RH5027 each gain an exemption requirement, and FMT-BLANK-020 states the kept blank line as decided. Change doc: `sdd/changes/2026-11/own-line-comment-before-every-brace.md` (Draft), with six visible output changes.

### Examples for the tests

None until the answer. On A, the six entries of the change doc give the formatter regression tests (each brace kind, the chained keywords).

### Established facts

- The issue's own input (`int[] G =⏎// c⏎⏎{ … }` as a field) is now refused by the precheck as unsupported (comment inside a variable declaration outside its initializer values, FMT-UNSUP-004); the CLI reports it and leaves the file unchanged. The premise holds on other inputs: an assignment `g = new int[]⏎// c⏎⏎{`, and `// d⏎⏎}` inside a field's array initializer.
- Today's output keeps the blank line behind an own-line comment in front of: both array-initializer braces, both extension-block braces, a property pattern's `{`, an assigned collection initializer's `}`, an interpolation's `}`, and `else`, `catch`, `finally`, do-`while`. Confirmed with the CLI.
- The issue lists "the opening brace of a complex element initializer" as kept; that no longer holds — FMT-BLANK-020 names it and the CLI removes the blank line (`{ 1, 2 },⏎// c⏎⏎{ 3, 4 }` → no blank line behind `// c`). Anonymous-object and switch-expression braces are removed today too.
- Rules reporting the formatter's output: RH5024 (array `}`, extension `}`, assigned collection initializer `}`, interpolation `}`), RH5025 (array `{`, extension `{`, property pattern `{`), RH5026 (`else`, `catch`, `finally`), RH5027 (do-`while`; the issue comment names RH5026 for it, which is wrong). Each fix removes the blank line.
- An interpolation's `{` cannot have a comment in front of it (string text precedes it), so only its `}` is in question.
- A comment between `}` and `else` is also reported by RH5037, a comment between a field's `=` and its initializer by RH5042 and refused by the precheck — rare shapes; the braces of extension blocks, property patterns, and initializers with a trailing comment inside are ordinary code.

### Output questions

**2 · Formatter · Blank line between an own-line comment and the brace or keyword behind it**

Input:
```csharp
class C
{
    void M(object o, int a)
    {
        int[] g;
        g = new int[]
            // c

            {
                1
                // d

            };

        if (o is C
            // e

            { })
        {
        }
        // f

        else
        {
        }
    }
}
```

Before (`main`):
```csharp
        g = new int[]

            // c

            {
                1

                // d

            };

        if (o is C

        // e

        { })
        {
        }

        // f

        else
        {
        }
```
RH5025 reports the blank lines above `{` (twice), RH5024 the one above `}`, RH5026 the one above `else`.

After, A (recommended) — the formatter removes the blank line, as it does before every other brace:
```csharp
        g = new int[]

            // c
            {
                1

                // d
            };

        if (o is C

        // e
        { })
        {
        }

        // f
        else
        {
        }
```

After, B — the formatter's output stays as on `main`; RH5024, RH5025, RH5026, and RH5027 no longer report a blank line behind an own-line comment in front of these braces and keywords.

The same holds for both braces of an extension block, an assigned collection initializer's `}`, an interpolation's `}`, `catch`, `finally`, and a do-statement's `while` (change doc entries 2, 4, 5, 6).

Why A: one rule for every brace and chained keyword, and four rules need no exemption for braces a user cannot tell apart from the others.

Requirement IDs touched: FMT-BLANK-020 (A); RH5024-001, RH5025-001, RH5026-001, RH5027-001 (B).

### Internal choices

- Chained keywords are included in the same question and change doc, as the issue's comment extends it; RH5027 (not RH5026) covers do-`while`.
- No rule page changes under either reading: A changes no rule; B's exemptions are corner cases (`squad/roles/spec-author.md`, step 4).

### Follow-up candidates

**Formatter · comment in front of an interpolation's closing brace keeps its column** (outside the run's issue: the blank lines are this issue's, the column is not). Reached through the CLI.

Input:
```csharp
class C
{
    void M(int a)
    {
        var t = $"{
            // e
            a
            // f
            }";
    }
}
```

Real output:
```csharp
        var t = $"{
        // e
        a

            // f
        }";
```

Why it looks wrong: every other line of the hole, `// e` and `}` included, moves to column 8; `// f` alone stays in column 12. Readings: A — `// f` moves to column 8 with the other lines of the hole; B — the formatter keeps the hole's lines as written. Recommendation A, lower confidence: not checked which indentation requirement owns hole lines.

## Whole run

Files: `sdd/changes/2026-11/conditional-access-assignment-fix-layout.md`, `sdd/changes/2026-11/own-line-comment-before-every-brace.md`, this handover. No spec, ledger, or rule page changed.

Role: spec author
Commit: 2365510
Verdict: change doc written (amendment of 01 for #1568 and #1698; #1567 unchanged)

My prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the amendments the skeptic's handover requests. Every "Before" below is real output of `scripts/format.sh` (run twice, stable) or `scripts/apply-fix.sh` (LF and CRLF agree) at 2365510, whose production code equals fe7b8f8. Every "After" is hand-built; for #1568 it is the real output of the same input with each anonymous method replaced by its block-lambda counterpart, the lambda's text swapped back (the block's columns are the lambda's, since `delegate` starts where the lambda's `(` starts). Fixtures: scratchpad `a3/`, not in the repository.

## Changed requirements

- FMT-INDENT-031 — An anonymous method's block aligns like a block lambda — changed further: the position list names a field's value, a compound assignment, an object initializer, a conditional branch, a switch-expression arm, a collection expression, and a chain of calls; a new example shows those positions and a nested anonymous method (input and output above the first raw-string example).
- FMT-INDENT-006 — A comment on its own line takes the column of the code that follows it — clarified within this run's change: the brace's column holds "also when the hole's content stands in another column"; the sentence that said the hole's lines and its brace stand "in one column" (wrong for an aligned operand) is reworded; `Why:` reworded; the interpolation example gains `u` (`$"{a +⏎b⏎// f⏎}"`, `// f` in the brace's column 12, not the operand's 19). Run through today's formatter: the example differs from today's output only in the five `// f` lines the change moves.
- FMT-BREAK-045, FMT-BREAK-011 — unchanged; #1567 was already narrowed to the explicit return type in 01 and the skeptic confirmed that scope.

Change docs:
- `sdd/changes/2026-11/anonymous-method-aligns-everywhere.md` — entry 5 (field, initializer, conditional branch, switch arm, collection element, `+=`, chain, nested; real before, hand-built after), RH5111 initializer result (column 25 under `F` → 29 under `delegate`), one sentence in *Why*.
- `sdd/changes/2026-11/interpolation-close-brace-comment-column.md` — new entry 3 (content column ≠ brace column; old entry 3 is now 4), the alternative "column of the hole's content line" (A′) under *Alternatives considered*, *Why* reworded to the same effect.
- `sdd/changes/2026-11/lambda-return-type-join.md` — unchanged.

`scripts/test.sh --project architecture`: 197 passed, 1 failed — the same expected `FMT-BREAK-045: no test covers it` as in 01.

## Examples for the tests

FMT-INDENT-031 (the new positions example in addition to those listed in 01); FMT-INDENT-006 (the interpolation example, now with `u`); FMT-BREAK-045 as in 01.

## Output questions

The three questions below are ready to send in one message. Option labels are kept from 01 and 02: #1568 A/B, #1698 A/A′/B/C, #1567 A/B.

---

**1 · Formatter · the block of an anonymous method (`delegate { … }`) outside a raw-string hole**

Input:
```csharp
using System;

class D
{
    public Func<int> F { get; set; }
}

class C
{
    int M(Func<int> f, int k = 0)
    {
        var x = M(delegate { return 1; }, 2);
        var d1 = new D { F = delegate { return 1; } };
        Func<int> c1 = k > 0 ? delegate { return 1; } : null;

        return M(delegate { return 1; });
    }
}
```

Before (`main`):
```csharp
        var x = M(delegate
        {
            return 1;
        },
                  2);
        var d1 = new D
                 {
                     F = delegate
        {
            return 1;
        }
                 };
        Func<int> c1 = k > 0
                           ? delegate
        {
            return 1;
        }
                           : null;

        return M(delegate
        {
            return 1;
        });
```

After, A: unchanged (as Before).

After, B (recommended):
```csharp
        var x = M(delegate
                  {
                      return 1;
                  },
                  2);
        var d1 = new D
                 {
                     F = delegate
                         {
                             return 1;
                         }
                 };
        Func<int> c1 = k > 0
                           ? delegate
                             {
                                 return 1;
                             }
                           : null;

        return M(delegate
                 {
                     return 1;
                 });
```

Under B the same holds for every other position — a field's value, `static` and `async` anonymous methods (under `delegate` and `async`), `+=`, a switch-expression arm, a collection element, a chain of calls, an anonymous method nested in another or in a lambda's block — and for the code fixes that format such code: today the fix of RH5101 (first argument on the line of the opening parenthesis) puts the block in column 16 and the fix of RH5111 (assignment line breaks) in column 8, or 25 in an initializer; under B both put it under `delegate`.

Why B: one layout for the construct everywhere, as a block lambda in the same position already gets, and it removes the blocks that today stand left of the initializer or branch that holds them.

---

**2 · Formatter · comment on its own line in front of the closing `}` of a hole in a regular interpolated string**

Input:
```csharp
class C
{
    string M(int a, int b)
    {
        var t = $"{
            // e
            a
            // f
            }";
        var u = $"{a +
            b
            // f
            }";

        return t + u;
    }
}
```

Before (`main`; the same whichever column `// f` is written in):
```csharp
        var t = $"{
            // e
            a

                // f
            }";
        var u = $"{a
                   + b

                // f
            }";
```

After, A (recommended) — the column of the `}`:
```csharp
        var t = $"{
            // e
            a

            // f
            }";
        var u = $"{a
                   + b

            // f
            }";
```

After, A′ — the column of the hole's content line above:
```csharp
        var t = $"{
            // e
            a

            // f
            }";
        var u = $"{a
                   + b

                   // f
            }";
```

After, B: unchanged (as Before).

After, C — as A, without the blank line in front of the comment:
```csharp
        var t = $"{
            // e
            a
            // f
            }";
        var u = $"{a
                   + b
            // f
            }";
```

The same applies to `$@"…"` and `$"""{…}"""` strings, and to a regular string nested in a multi-line raw string's hole; a multi-line raw string's own hole (`{{ … }}`) keeps the comment one level deeper in every option.

Why A: a comment on its own line takes the column of the token it precedes, as in front of every other closing delimiter, and keeps the blank line every comment behind code gets.

---

**3 · Formatter · lambda whose explicit return type stands on the line above its `(`**

Input:
```csharp
using System;

class C
{
    void M()
    {
        Func<int, int> i = int
            (int x) => x;
        Func<int, int> m = int
            (int x) =>
            {
                return x;
            };
        N(int
            (int x) => x);
    }

    void N(Func<int, int> f)
    {
    }
}
```

Before (`main`):
```csharp
        Func<int, int> i = int
            (int x) => x;
        Func<int, int> m = int
            (int x) =>
            {
                return x;
            };

        N(int (int x) => x);
```

After, A (recommended):
```csharp
        Func<int, int> i = int (int x) => x;
        Func<int, int> m = int (int x) =>
                               {
                                   return x;
                               };

        N(int (int x) => x);
```

After, B: unchanged (as Before).

A comment between the return type and `(` keeps the line break in both options. The issue's `static⏎(int x) => x` and `async` cases are already joined on `main`.

Why A: the same lambda is already joined as an argument; as a value it keeps a line break that structures nothing.

---

## Internal choices

- #1568 question shows four positions (argument + second argument, initializer, conditional branch, return) and names the rest in one line, rather than the full entry-5 input; the change doc holds the full list. Alternative: show all positions in the question.
- #1698 question merges the two inputs into one question with four options (A, A′, B, C) rather than two questions, because A′ equals A for the first input and C differs from A only in the blank line.
- #1567 question adds the argument form `N(int⏎(int x) => x)` to show the inconsistency the skeptic called decisive.
- FMT-INDENT-006 rewording for the aligned-content case is part of option A, not a separate requirement.

## Follow-up candidates

Unchanged from 01 and 02 (lead's list in `00-lead.md`).

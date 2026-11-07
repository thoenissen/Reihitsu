Role: spec author
Commit: b1d4a94
Verdict: change doc written

The prompt carried no intended fix or suspected cause. The issue's "Probable owners" note names `GenericConstraintContributor` and `GenericConstraintLineBreakRewriter`; that is a suspected cause and was not used. The issue's expected output and its "one level under `where`" sketch are `agent-filed` proposals; where this spec departs from them is stated below.

## Premise

Reproduced on b1d4a94 with `scripts/format.sh` on the issue's input: the output matches the issue's "Actual" block exactly (wrapped constraints one level left of `where`; `IEquatable<TFirst>, new()` kept on one line). Reachable through the CLI and whole-document formatting on any file with a wrapped constraint clause, and through RH5110's code fix (`scripts/apply-fix.sh RH5110`, entry 8 of the change doc). Every "before" output in the change doc was taken by running `scripts/format.sh` or `scripts/apply-fix.sh` on b1d4a94; inputs in scratch, not in the repository.

## Changed requirements

- FMT-BREAK-037 — The constraints of a clause are on one line or one constraint per line — added (`sdd/specs/formatter/line-breaks.md`)
- FMT-INDENT-032 — Wrapped constraints align to the first constraint — added (`sdd/specs/formatter/indentation.md`)
- `Uses` rows: line-breaks gains FMT-INDENT-032, indentation gains FMT-BREAK-037. Ledger updated. `SpecStructureTests` and `SpecConsistencyTests` pass (26/26); `RequirementCoverageTests` will fail until the two requirements have tests.
- Change doc: `sdd/changes/2026-11/constraint-clause-wrap.md`, `Status: Draft`, eight visible output changes.
- No code owners change; `GenericConstraintLineBreakRewriter` and `GenericConstraintContributor` already belong to these two specs. Rule pages untouched: RH5110's report and fix description do not change for the user in the normal case (RH5110's non-goal excludes line breaks inside a clause; P11).

## Examples for the tests

- FMT-BREAK-037: three example pairs — (1) wrapped between constraints / behind the colon (one and several constraints) / comma at line start / one-line clause unchanged; (2) multi-line constraint wraps its clause vs. one-line `IDictionary<int, string>` not wrapped; (3) comments: behind the colon, in front of a comma, behind a comma.
- FMT-INDENT-032: two example pairs — type-level and method-level clause aligned (later `where` keeps FMT-INDENT-017); constraint and comma pushed down by a comment.
- Qualifiers with an example on each side: "wrapped" (FMT-BREAK-037 examples 1 and 2), "first constraint starts a line" (FMT-INDENT-032 example 2 vs. 1).
- Declaration kinds beyond class/method (interface member without body, local function, delegate) are change-doc entry 7; RH5110 fix result is entry 8 (RH5110-003 already demands the fix follow the formatter).

## Output questions

All within thoenissen/Reihitsu#1631. The specs are written on the recommendations; the run can continue to the skeptic, but tests lock these outputs, so they must be answered before the test implementer. Inputs below are wrapped in `using System; class C { … }` where needed. Columns: `where` at 8, so the first constraint behind `where T : ` is at column 18.

### 1 · Formatter — layout of a wrapped constraint clause (FMT-BREAK-037, FMT-INDENT-032)

Input:
```csharp
class C
{
    void M<T>()
        where T : class, IComparable<T>,
            IEquatable<T>, new()
    {
    }
}
```

Before (`main`):
```csharp
class C
{
    void M<T>()
        where T : class, IComparable<T>,
    IEquatable<T>, new()
    {
    }
}
```

After, A (recommended) — one constraint per line, aligned under the first:
```csharp
class C
{
    void M<T>()
        where T : class,
                  IComparable<T>,
                  IEquatable<T>,
                  new()
    {
    }
}
```

After, B — author's line breaks kept, wrapped lines aligned under the first constraint:
```csharp
class C
{
    void M<T>()
        where T : class, IComparable<T>,
                  IEquatable<T>, new()
    {
    }
}
```

Why A: the issue's all-or-none rule, and the same shape as wrapped parameter lists (FMT-BREAK-012, FMT-INDENT-012).

### 2 · Formatter — first constraint wrapped onto the line below the colon

Input:
```csharp
class C
{
    void M<T>()
        where T :
            IDisposable
    {
    }

    void N<T>()
        where T :
            class,
            new()
    {
    }
}
```

Before (`main`):
```csharp
class C
{
    void M<T>()
        where T :
    IDisposable
    {
    }

    void N<T>()
        where T :
    class,
    new()
    {
    }
}
```

After, A (recommended) — joined onto the colon's line:
```csharp
class C
{
    void M<T>()
        where T : IDisposable
    {
    }

    void N<T>()
        where T : class,
                  new()
    {
    }
}
```

After, B — kept on its line, one level deeper than `where` (the issue's sketch):
```csharp
class C
{
    void M<T>()
        where T :
            IDisposable
    {
    }

    void N<T>()
        where T :
            class,
            new()
    {
    }
}
```

After, C — kept on its line, aligned to where the first constraint would stand behind the colon:
```csharp
class C
{
    void M<T>()
        where T :
                  IDisposable
    {
    }

    void N<T>()
        where T :
                  class,
                  new()
    {
    }
}
```

Why A: an argument or parameter list joins a wrap behind its opening parenthesis the same way (FMT-BREAK-012), and it leaves one column for every wrapped constraint.

### 3 · Formatter — a comment behind the colon keeps the first constraint on the next line

Only reachable with a comment (a join never crosses one, FMT-BREAK-001). If question 2 is answered B, B here follows from it.

Input:
```csharp
class C
{
    void M<T>()
        where T : // c
            class,
            new()
    {
    }
}
```

Before (`main`):
```csharp
class C
{
    void M<T>()
        where T : // c
    class,
    new()
    {
    }
}
```

After, A (recommended) — the column the first constraint has behind the colon:
```csharp
class C
{
    void M<T>()
        where T : // c
                  class,
                  new()
    {
    }
}
```

After, B — one level deeper than `where`:
```csharp
class C
{
    void M<T>()
        where T : // c
            class,
            new()
    {
    }
}
```

Why A: the constraints keep the same column with or without the comment, as an argument list whose first argument a comment pushes down aligns to the column after `(` (FMT-INDENT-012).

### 4 · Formatter — comma at the start of a line

Input:
```csharp
class C
{
    void M<T>()
        where T : class
            , new()
    {
    }

    void N<T>()
        where T : class // c
            , new()
    {
    }
}
```

Before (`main`):
```csharp
class C
{
    void M<T>()
        where T : class
    , new()
    {
    }

    void N<T>()
        where T : class // c
    , new()
    {
    }
}
```

After, A (recommended) — the comma moves to the end of the previous line; where a comment blocks that, the comma stays first and is aligned:
```csharp
class C
{
    void M<T>()
        where T : class,
                  new()
    {
    }

    void N<T>()
        where T : class // c
                  , new()
    {
    }
}
```

After, B — the comma stays at the start of its line, aligned under the first constraint:
```csharp
class C
{
    void M<T>()
        where T : class
                  , new()
    {
    }

    void N<T>()
        where T : class // c
                  , new()
    {
    }
}
```

Why A: parameter lists put the comma at the end of the line (FMT-BREAK-011), and the issue's expected output does too.

### 5 · Formatter — a constraint that a comment keeps over two lines

Input:
```csharp
using System.Collections.Generic;

class C
{
    void M<T>()
        where T : class, IDictionary<int, // c
            string>, new()
    {
    }
}
```

Before (`main`):
```csharp
class C
{
    void M<T>()
        where T : class, IDictionary<int, // c
                                     string>, new()
    {
    }
}
```

After, A (recommended) — the multi-line constraint splits the clause:
```csharp
class C
{
    void M<T>()
        where T : class,
                  IDictionary<int, // c
                              string>,
                  new()
    {
    }
}
```

After, B — unchanged (only line breaks around commas or behind the colon wrap a clause).

Why A: a multi-line element splits an argument or parameter list the same way (FMT-BREAK-013).

### Consequential change, not a separate question

RH5110's code fix formats the declaration as the formatter does (RH5110-003), so its result follows answers 1–4 (change doc entry 8): `void M<T>() where T : class,⏎ new()` fixes to `where T : class,` / `new()` aligned at column 18 instead of column 4. Diagnostics and rule pages do not change.

## Internal choices

- Two requirements (line breaks in FMT-BREAK, columns in FMT-INDENT), following the phase split, instead of one requirement in either spec. Alternative: one combined requirement in line-breaks.md.
- FMT-INDENT-017 left unchanged (it states only the `where` line's column); the new requirement is separate. Alternative: widen FMT-INDENT-017.
- The gap between the type parameter and the colon (`where T⏎ : class`) is excluded from FMT-BREAK-037 by its coverage sentence, not decided: the issue asks about constraints, not that gap. It is a follow-up candidate below. Alternative: decide it in this run as an extra output question.
- FMT-INDENT-032 states explicitly that it places only lines starting with a constraint or a comma; lines inside a constraint stay with their own construct (FMT-INDENT-015).
- No non-goal added: everything the issue does not ask for that looks wrong went to follow-up candidates, as non-goals may hold only deliberate exclusions.

## Follow-up candidates

All observed with `scripts/format.sh` on b1d4a94; reachable through the CLI and whole-document formatting.

(Former candidate 1, the wrap between the type parameter and the colon, joined the run as output question S2/Q7; see `03-spec-author.md`.)

2. **Base list wrapped behind its colon.** `class B :⏎    IDisposable, ICloneable` and `class B : // c⏎    IDisposable` → today the base types land at column 0 (the declaration's block column). FMT-INDENT-016 aligns only later base types to the first. Readings: A join the first base type onto the colon's line (comment-blocked: align to the column behind the colon); B indent one level deeper than the declaration. Recommendation A, by analogy with this run's constraint rules.
3. **Angle-bracket list whose first element a comment pushes onto the next line.** `where T : class, IComparable< // c⏎            T>, new()` → today `T>` lands at column 4. FMT-INDENT-015 decides only later elements (and omitted type arguments). Readings: A the column after `<`; B one level deeper than the line holding `<`. Recommendation A (FMT-INDENT-012 model). Not specific to constraints; check a field type `List< // c⏎ int> _x;` first.
4. **Argument list wrapped in front of a comma is not laid out one element per line.** `Foo(1⏎            , 2, 3);` stays exactly as written; FMT-BREAK-012 says a list wrapped "between two elements" becomes one element per line. Looks like a bug against FMT-BREAK-012 (comma position itself is a non-goal of line-breaks.md for argument lists, one-per-line is not). Outside this run's issue.

The issue's side observation (`where T : class⏎ , new()` on an interface with `{ }` ending as `, new();`) is not a separate problem: the empty body becoming `;` is FMT-STRUCT's empty-type rule, and the misplaced `, new()` is change-doc entry 4 (question 4) of this run.

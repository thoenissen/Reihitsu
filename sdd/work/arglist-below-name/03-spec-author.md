Role: spec author
Commit: b6bfe9b
Verdict: change doc written

Amendment carrying out `00-lead.md` decisions 1–3. Inputs: `00-lead.md`, `01-spec-author.md`, `02-skeptic.md`; my prompt carried no intended fix. `Status: Draft` kept. Every *before* below is real output of `main`'s code (the branch changes only specs and docs) from `scripts/format.sh` (CLI) and `scripts/apply-fix.sh` on scratch copies outside the tree; every *after* is the CLI's output of the same input with the line break in front of `(` removed by hand (and, for the multi-line options, the container still written over two lines), so the parts the change leaves alone are today's output.

Files:
- `sdd/specs/formatter/line-breaks.md` — FMT-BREAK-039, FMT-BREAK-020, FMT-BREAK-015.
- `sdd/changes/2026-11/argument-list-below-name.md` — What, changes 1–2, new change 3 (conditional), old 3–5 renumbered 4–6, Decisions, Alternatives.

`scripts/test.sh --project architecture`: 152 passed, 1 failed — `EveryRequirementOfAnEnforcedSpecIsCovered` for FMT-BREAK-039, the covering test still to be written; as before.

## Changed requirements

- FMT-BREAK-039 — An argument list starts on the line of the code in front of it — added (amended): `(f)⏎(1)` replaced by `(f ?? g)⏎(1)` in text and example (decision 1); it states that it owns the gap wherever the argument list stands, in a chain and inside a list element too (decision 3); its second paragraph now covers every construct FMT-BREAK-022 decides (judged as written) and states that a list of FMT-BREAK-012 and a conditional expression are judged as the line they become (decision 2; the conditional on the recommended answer of question 3 below); its Why says the precedent differs by construct instead of "as for every other join inside an element".
- FMT-BREAK-020 — Split member names and attached parts are rejoined — clarified: joins element accesses and conditional element accesses in a chain; an attached argument list is FMT-BREAK-039's. Its example is labeled accordingly. No output change.
- FMT-BREAK-015 — Wraps inside a list element are joined — clarified: the opening parenthesis of an argument list joins its list of gaps another requirement owns (FMT-BREAK-039). No output change.

## Examples for the tests

FMT-BREAK-039: four examples — (1) invocation (incl. `(f ?? g)`), object creation, implicit object creation with a blank line, constructor initializer, primary-constructor base type; (2) `with(…)`, collection element, and complex element initializer, each keeping one element per line; (3) new: an argument list `Baz(Bar⏎(1), 2)` and a conditional `a ? Bar⏎(1) : 2` judged as the line they become; (4) comment, directive, disabled text in the gap: unchanged. Examples 2 and 3: before and after checked with `scripts/format.sh` (the before of 3's `Baz` is already one line on `main`; the conditional's is not). FMT-BREAK-020 and FMT-BREAK-015 tests are unchanged; the test implementer should tag the existing argument-list cases of their tests with FMT-BREAK-039 as well where they now demonstrate it.

## Output questions

The complete batch, as it will be asked. Option labels as drafted.

**1 · Formatter · Argument list written on the line below its name**

Input:
```csharp
        Foo
        (4);
        var l = new List<int>
        (4);
        var r = (f ?? g)
        (1);
        HashSet<int> h = [with
        (4), 1, 2];
        int[] a = [Bar
        (4), 1, 2];
```

Before (`main`, CLI):
```csharp
        Foo
        (4);

        var l = new List<int>
        (4);
        var r = (f ?? g)
        (1);
        HashSet<int> h = [
                             with
        (4),
                             1,
                             2
                         ];
        int[] a = [
                      Bar
        (4),
                      1,
                      2
                  ];
```
The RH5305 fix gives the same, but with `(4)` under the `[`.

After, A (recommended) — joined; a collection written over several lines keeps one element per line:
```csharp
        Foo(4);

        var l = new List<int>(4);
        var r = (f ?? g)(1);
        HashSet<int> h = [
                             with(4),
                             1,
                             2
                         ];
        int[] a = [
                      Bar(4),
                      1,
                      2
                  ];
```

After, B — joined; a collection that spanned lines only because of this break becomes one line:
```csharp
        Foo(4);

        var l = new List<int>(4);
        var r = (f ?? g)(1);
        HashSet<int> h = [with(4), 1, 2];
        int[] a = [Bar(4), 1, 2];
```

After, C: unchanged (the output of `main` above).

Why A: the argument list belongs to its name, and a collection already keeps one element per line today when another split inside an element is rejoined.

**2 · Formatter · Constructor initializer and primary-constructor base type written below their keyword or type** (only if 1 is A or B)

Input:
```csharp
class D : B
{
    public D()
        : base
        (1)
    {
    }
}

class E() : B
    (1);
```

Before (`main`):
```csharp
class D : B
{
    public D()
        : base
    (1)
    {
    }
}

class E() : B
            (1);
```

After, A (recommended):
```csharp
class D : B
{
    public D()
        : base(1)
    {
    }
}

class E() : B(1);
```

After, B: unchanged (the output of `main` above).

Why A: it is the same gap as an invocation's, and today's column for `(1)` shows as little.

**3 · Formatter · Argument list written below its name inside a conditional expression** (only if 1 is A or B)

Input:
```csharp
        var x = a ? Get
        (1) : 2;
```

Before (`main`, CLI):
```csharp
        var x = a
                    ? Get
        (1)
                    : 2;
```
The RH5308 fix gives the same, but with `(1)` at column 16.

After, A (recommended) — judged after the join:
```csharp
        var x = a ? Get(1) : 2;
```

After, B — judged as written:
```csharp
        var x = a
                    ? Get(1)
                    : 2;
```

Why A: a conditional whose only line break was another split inside a branch already becomes one line today.

**4 · Formatter · Argument list written below its name inside a dictionary's element initializer** (only if 1 is A or B)

Input:
```csharp
        var e = new Dictionary<int, int> { { Get
            (1), 2 } };
```

Before (`main`):
```csharp
        var e = new Dictionary<int, int>
                {
                    {
                        Get
        (1),
                        2
                    }
                };
```

After, A (recommended with 1A):
```csharp
        var e = new Dictionary<int, int>
                {
                    {
                        Get(1),
                        2
                    }
                };
```

After, B (with 1B):
```csharp
        var e = new Dictionary<int, int>
                {
                    { Get(1), 2 }
                };
```

Why A: the inner braces follow the collection expression, as they do today after another split inside an element is rejoined.

## Internal choices

- Decision 3 written as: FMT-BREAK-039 owns the gap and says so; FMT-BREAK-020 keeps element accesses and points to FMT-BREAK-039 for argument lists; FMT-BREAK-015 lists the argument list's `(` among gaps another requirement owns. Alternative: FMT-BREAK-039 covers only the gaps the other two leave — rejected by the lead's decision.
- FMT-BREAK-039 states by its cause that it applies "wherever the argument list stands", including inside a lambda's block inside an argument, where FMT-BREAK-015 did not join; that is new output already inside question 1's scope (same join), not a separate question.
- The conditional sentence of FMT-BREAK-039 and change 3 are written on question 3's recommendation A; B changes that bullet and drops change 3.
- No rule page or RH5305/RH5308 spec change, as in `01-spec-author.md`.

## Follow-up candidates

Unchanged from `00-lead.md` (F1–F3); F4 dropped by the lead.

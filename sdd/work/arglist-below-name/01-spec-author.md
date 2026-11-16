Role: spec author
Commit: 0fc7890
Verdict: change doc written

Inputs: thoenissen/Reihitsu#1786 (no comments), `00-lead.md` (no maintainer answers). My prompt carried no intended fix. Every *before* below is real output of `main` at 0fc7890, from `scripts/format.sh` (CLI) and `scripts/apply-fix.sh` (code fixes) on scratch copies outside the tree. Every *after* is today's output of the same input with the line break in front of `(` removed by hand (for the collection cases: with the collection still written over two lines), so the parts the change leaves alone are today's output.

Premise check: confirmed. On `main`, `Foo⏎(4);` keeps the line break and the CLI puts `(4)` at the enclosing statement's column; in a collection expression the CLI puts `(4)` at the statement's column and the RH5305 fix at the `[`'s column (the issue's "different columns" claim holds). Reachable through the CLI, whole-document formatting, and every code fix whose formatted node holds such an argument list. Already joined on `main`, so not in question: an argument list in a chain with a link (`this.Foo⏎(4)`, `Get⏎(4).ToString()`, `g?.Invoke⏎(1)`; FMT-BREAK-020), inside an argument-list element (`Foo(Get⏎(4))`; FMT-BREAK-015), and an attribute's argument list (`[AttributeUsage⏎(…)]`). Not joined on `main`: invocations of any expression outside those, object creations (`new List<int>⏎(4)`, `new⏎(4)`, `new C⏎().P`), constructor initializers (`: base⏎(1)` → `(1)` at the constructor's column), primary-constructor base types (`class E() : B⏎(1)` → `(1)` at column 12), `with(…)` elements. Formatting the issue-shaped fixture twice is idempotent on `main`.

Files:
- `/home/user/Reihitsu/sdd/changes/2026-11/argument-list-below-name.md` — change doc, `Status: Draft`, five visible output changes.
- `/home/user/Reihitsu/sdd/specs/formatter/line-breaks.md` — FMT-BREAK-039 added at the end; FMT-BREAK-031's list of joins names it.
- `/home/user/Reihitsu/sdd/specs/requirement-ids.txt` — FMT-BREAK-039 inserted.
- No rule page, no RH5305 spec change: no rule reports the shape, and what RH5305 reports does not change.

`scripts/test.sh --project architecture`: 152 passed, 1 failed — `EveryRequirementOfAnEnforcedSpecIsCovered`: "FMT-BREAK-039: no test covers it". That is the covering test the test implementer adds; nothing else fails.

The spec is written on the recommended options (Q1 A, Q2 A below). Q1 B changes FMT-BREAK-039's second paragraph and the change doc's change 2; Q1 C retracts the requirement and the change doc; Q2 B narrows the first paragraph and drops change 3.

## Changed requirements

- FMT-BREAK-039 — An argument list starts on the line of the code in front of it — added
- FMT-BREAK-031 — A join writes the minimal separator, and the phase writes no other space — clarified (its list of joins names FMT-BREAK-039; no output of its own)

## Examples for the tests

FMT-BREAK-039: three examples — (1) invocation, object creation, implicit object creation, invocation of a parenthesized expression, constructor initializer, primary-constructor base type, and a blank line in the gap; (2) `with(…)` element and collection element, collection keeps one element per line; (3) `//` comment, `/* */` comment, and `#if` in the gap: unchanged. All three verified against `main`: the outputs of (1) and (2) are stable under today's formatter, and (3) is unchanged today. The change doc's change 5 gives five code-fix cases (RH5305, RH5303, RH5301, RH5302, RH5308) with before from `apply-fix.sh` and after from `apply-fix.sh` on the joined input. Idempotency tests per the issue's "Done when".

## Output questions

**1 · Formatter · Argument list written on the line below its name**

Input:
```csharp
using System;
using System.Collections.Generic;

class C
{
    void M(Func<int, int> f)
    {
        Foo
        (4);

        var l = new List<int>
        (4);
        var r = (f)
        (1);
        HashSet<int> h = [with
        (4), 1, 2];
        int[] a = [Bar
        (4), 1, 2];
    }
}
```

Before (`main`):
```csharp
        Foo
        (4);

        var l = new List<int>
        (4);
        var r = (f)
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
RH5305 fix on `main`: the same collection layout, but `(4)` at the column of `[`.

After, A (recommended) — joined; a collection written over several lines keeps one element per line (CLI and every code fix that formats the node):
```csharp
        Foo(4);

        var l = new List<int>(4);
        var r = (f)(1);
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

After, B — joined; a collection that spanned lines only because of this break becomes one line (the issue's sketch):
```csharp
        Foo(4);

        var l = new List<int>(4);
        var r = (f)(1);
        HashSet<int> h = [with(4), 1, 2];
        int[] a = [Bar(4), 1, 2];
```

After, C — unchanged: the output of `main` above; the CLI and the RH5305 fix keep putting `(4)` in different columns.

Why A: the argument list belongs to its name, and a collection expression already keeps one element per line on `main` when another split inside an element is rejoined (`[v.⏎Length, 2]`, `[Get⏎(1).GetHashCode(), 2]`), so B would make collections lay out differently by which split the author wrote.

Requirement IDs: FMT-BREAK-039 (new), FMT-BREAK-022 (unchanged; A follows it as `main` applies it today). Code-fix consequences: change doc, change 5.

**2 · Formatter · Constructor initializer and primary-constructor base type written below their keyword or type** (only when 1 is A or B)

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

Why A: it is the same gap as an invocation's, and the column `main` gives `(1)` (the constructor's, or 12) shows as little.

## Internal choices

- FMT-BREAK-039 is stated by its cause — the gap in front of any argument list's `(` — so an invocation of a parenthesized expression or of an invocation (`(f)⏎(1)`, `Make(1)⏎(2)`) joins too; alternative: name only invocations of names, which misses those members.
- FMT-BREAK-020 left unchanged: for an argument list in a chain, it and FMT-BREAK-039 demand the same join; alternative: narrow FMT-BREAK-020 to element accesses, which rewrites a requirement whose output does not change.
- Bracketed argument lists and attribute argument lists are left out by a coverage sentence, not a non-goal (sdd/README: non-goals are deliberate exclusions, and `a⏎[0]` is undecided — see follow-up 1).
- FMT-BREAK-039's second paragraph pins the collection-expression interaction (Q1 A) explicitly, because the outcome depends on the order of the phase's rewriters; other constructs whose layout depends on spanning several lines (conditional, binary) are not decided by the requirement — on `main` a conditional is judged after a FMT-BREAK-020 join (`a ? v.Clone⏎().GetHashCode() : 2` → one line), and the change doc's examples avoid that case.
- No rule page and no RH5305 spec change: RH5305's non-goal "the layout of an element after its first token" already leaves the element's interior to the formatter.

## Follow-up candidates

**F1 · Formatter · Element access written below its expression, outside a chain**

Input:
```csharp
var y = arr
[0];
```
Before (`main`): unchanged; `[0]` at the statement's column. FMT-BREAK-020 joins `[0]` only in a chain with a link.
After, A: `var y = arr[0];` After, B: unchanged. Recommendation: A, by the same reasoning as Q1. Not in the issue.

**F2 · Formatter and code fixes · An unowned continuation line inside a node a code fix formats gets a different column than whole-document formatting gives it**

Input:
```csharp
Func<int>[] a = [() =>
1, () => 2];
```
CLI (`main`):
```csharp
        Func<int>[] a = [
                            () =>
        1,
                            () => 2
                        ];
```
RH5305 fix (`main`): the same, but `1,` at column 24 (the `[`'s column). Same mechanism as the issue's `with⏎(4)` columns, for lines FMT-INDENT does not own (its non-goal, thoenissen/Reihitsu#953). FMT-BREAK-039 removes the argument-list member of this class only.

**F3 · Analyzer · New-rule candidate (P11)**: no rule reports an argument list's `(` on the line below its name; RH5105 covers parameter lists and names argument lists as a non-goal. A candidate for a rule of its own, not a widening of RH5105.

# A comma that starts a line in an initializer

| | |
|---|---|
| Status | Accepted |
| Date | 2026-11 |
| Affected requirements | FMT-BREAK-038 (added), FMT-INDENT-033 (added and retired in this change), FMT-BREAK-021 (changed: array initializers move a comma that starts a line), FMT-BREAK-022 (changed: points to FMT-BREAK-038). FMT-INDENT-022 to FMT-INDENT-026 are referenced, not changed. |
| Depends on | [A comment or directive in front of a comma on a later line is unsupported](comma-behind-comment-unsupported.md) |

## What

No requirement decided where a comma goes that starts a line inside an object, collection, anonymous-object, `with`, or array initializer (FMT-BREAK-021), or inside a collection expression, list pattern, property pattern, or complex element initializer (FMT-BREAK-022): FMT-INDENT-022 to FMT-INDENT-026 place the braces and the elements that start a line, not a separator. The change decides it for every construct of that family: a comma that starts a line moves to the end of the line of the element in front of it, and the line break moves behind it, so the element behind it starts its own line; a comment behind the comma goes with it (FMT-BREAK-038). This also holds in an array initializer, whose other line breaks stay as written. No line in these constructs then starts with a comma, so no column has to be decided for one.

A comma that a comment, a directive, or disabled text keeps off the element's line is not laid out: the formatter refuses an input that holds one (FMT-UNSUP-009), which [its own change doc](comma-behind-comment-unsupported.md) declares. FMT-INDENT-033, which this run first wrote to give such a comma's line the elements' column, is retired before it shipped.

## Why

On `main` the line-break phase puts every element on its own line and leaves the comma alone on a line between them, and the indentation phase puts that line at the column of the enclosing statement, outside the initializer's braces; in an array initializer the comma keeps its element on its line, but that line also goes to the statement's column. Through a code fix that formats the initializer as a node (RH5301, RH5303, RH5304), the same line lands in yet another column, which whole-document formatting then moves, so the fixes' results differ from the formatter's output. Argument, parameter, and attribute argument lists already move such a comma behind the element in front of it (FMT-BREAK-011, FMT-BREAK-012). Comma-first layout is a style people write, and no rule reports it in most of these constructs, so it gets a decided layout rather than a refusal.

## Visible output changes

Input:

```csharp
class C
{
    void M(R p)
    {
        var a = new D(1, 2) { X = 3
            , };
        var b = new D { X = 3
            , /* c */ Y = 4 };
        int[] d = [1
            , 2];
        var e = new int[]
        {
            1
            , 2
        };
    }
}
```

Before (`main`):

```csharp
class C
{
    void M(R p)
    {
        var a = new D(1, 2)
                {
                    X = 3
        ,
                };
        var b = new D
                {
                    X = 3
        , /* c */
                    Y = 4
                };
        int[] d = [
                      1
        ,
                      2
                  ];
        var e = new int[]
                {
                    1
        , 2
                };
    }
}
```

After:

```csharp
class C
{
    void M(R p)
    {
        var a = new D(1, 2)
                {
                    X = 3,
                };
        var b = new D
                {
                    X = 3, /* c */
                    Y = 4
                };
        int[] d = [
                      1,
                      2
                  ];
        var e = new int[]
                {
                    1,
                    2
                };
    }
}
```

The same holds for an anonymous object, a `with` initializer, a nested collection initializer (`Items = { … }`), a complex element initializer, a list pattern, and a property pattern (FMT-BREAK-038 examples). The results of RH5301's, RH5303's, and RH5304's code fixes on such an initializer change with the formatter: the comma ends the element's line, as in whole-document formatting. A comma that a comment holds, such as `X = 3 // k⏎, Y = 4`, is refused as unsupported; its visible change is in the change doc of the declaration.

## Decisions

### Placement of the comma (maintainer, batch 1, question 1: A, with arrays moving the comma)

The comma ends the line of the element in front of it, a comment behind it goes with it, and the element behind it starts its own line, in every construct of FMT-BREAK-021 and FMT-BREAK-022. The maintainer answered A with one change: in an array initializer the comma also moves behind the element in front of it (`1⏎, 2` becomes `1,⏎2`), as in the other constructs, instead of keeping the array's line breaks and only fixing the column. FMT-BREAK-021 keeps the array's other line breaks; its `Why:` now states that the move keeps the array's line count.

The answer's second part — a comma that a `//` comment keeps off the element's line (case `c`) stays alone on its line at the elements' column — and the batch-1b answer for arrays (`1 // k⏎, 2` keeps `, 2` on its line, only the column changes) are superseded by escalation 10 below: such a comma is refused, so FMT-BREAK-038 states no held comma and FMT-INDENT-033 is retired.

### Comma-first code stays supported (maintainer, batch 1, question 2: not declared)

"A comma separated from the token in front of it by a line break" is not declared unsupported: the formatter keeps converting comma-first code, in parameter and argument lists as today and in the constructs of this change.

### A comment or directive in front of the comma is unsupported (maintainer, escalation 10: question 4 A, question 5 A)

The maintainer then declared the narrower shape — a comment, a directive, or disabled text between a comma and the token in front of it, with the comma on a later line — unsupported, in this run (FMT-UNSUP-009). The held comma of this issue (case `c`) is that shape, so this change writes no line rule and no column for it: FMT-BREAK-038's paragraph on a held comma and its third example are dropped, and FMT-INDENT-033, which only placed the lines such a comma leaves, is retired. The declaration, its visible change, and the special cases it deletes are recorded in [its own change doc](comma-behind-comment-unsupported.md), because it reaches every list, not only this issue's constructs.

### Scope

The decision covers the whole construct family of FMT-BREAK-021 and FMT-BREAK-022, every member of which failed the same way on `main`, not only the object initializer of thoenissen/Reihitsu#1738. A comma that starts a line between switch-expression arms, in a positional pattern, or in a base list also falls to the enclosing column on `main`; those are outside this change (indentation non-goal) and are follow-up candidates.

### The issue's goals

The issue's goal — decide and specify the layout and cover it with a test — is kept and widened to the family and to the code-fix paths that format these constructs. Its alternative "leave as written, the shape is rare" is dropped: the lone comma line is produced by the line-break phase itself, so "as written" is not something the formatter can keep by skipping the construct, and comma-first code is not rare outside object and collection initializers.

## Alternatives considered

- **The comma keeps its own line, at the elements' column** — rejected by the maintainer's answer: the comma then belongs to neither element and sits where no other list puts it.
- **A held comma stays alone on its line at the elements' column** (batch 1, question 1, as first answered) — superseded by the declaration (escalation 10): it needed a held-comma line rule and FMT-INDENT-033 for a shape the formatter now refuses.
- **A held comma keeps the element behind it on its line, as in an argument list** — rejected: in an object or collection initializer that element would start off the elements' column, which RH5301 and RH5303 report and whose fixes would write the same line back, a permanent diagnostic.
- **Array initializers keep their line breaks and only the comma's column changes** — rejected by the maintainer's answer.
- **Leave the comma as written, the alternative the issue names** — rejected: see *The issue's goals*.
- **Decide only the object initializer of the issue's example** — rejected: the same comma is left at the statement's column in every construct of FMT-BREAK-021 and FMT-BREAK-022, so a narrower decision leaves the rest of the class with today's output.

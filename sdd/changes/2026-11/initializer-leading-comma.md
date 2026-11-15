# A comma that starts a line in an initializer

| | |
|---|---|
| Status | Draft |
| Date | 2026-11 |
| Affected requirements | To be added once the maintainer answers: a line-break requirement in `line-breaks.md` for the commas of the constructs of FMT-BREAK-021 and FMT-BREAK-022, and an indentation requirement in `indentation.md` for a comma that starts a line in an initializer or a collection expression. FMT-BREAK-021, FMT-BREAK-022, FMT-INDENT-022, FMT-INDENT-023, and FMT-INDENT-025 are referenced, not changed. |

## What

No requirement decides where a comma goes that starts a line inside an object, collection, anonymous-object, or `with` initializer (FMT-BREAK-021), an array initializer, or a collection expression, list pattern, property pattern, or complex element initializer (FMT-BREAK-022), nor which column such a comma's line starts in: FMT-INDENT-022, FMT-INDENT-023, and FMT-INDENT-025 place the braces and the elements that start a line, not a separator. The change decides both.

## Why

Today the line-break phase puts every element on its own line and leaves the comma alone on a line between them, and the indentation phase puts that line at the column of the enclosing statement, outside the initializer's braces. The result is stable, but it separates the comma from both elements and from the initializer. Argument, parameter, and attribute argument lists already move such a comma behind the element in front of it (FMT-BREAK-011, FMT-BREAK-012) and align a separator that starts a line with the elements (FMT-INDENT-012). Comma-first layout is a style people write, so the shape is not rare and gets a decided layout rather than a refusal.

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
        var c = new D
        {
            X = 3 // k
            , Y = 4
        };
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
        var c = new D
                {
                    X = 3 // k
        ,
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

After, option A — the comma ends the line of the element in front of it, a comment behind it goes with it, as in an argument list; a comma that a comment keeps off that line stays at the start of the next element's line, at the elements' column; an array initializer keeps its line breaks, and its comma line takes the elements' column:

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
        var c = new D
                {
                    X = 3 // k
                    , Y = 4
                };
        int[] d = [
                      1,
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

After, option B — the comma keeps its own line, at the elements' column:

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
        var c = new D
                {
                    X = 3 // k
                    ,
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

## Decisions

### Placement and column of the comma

Pending: the maintainer's answer to the output questions of this run. Recommended: A, for every construct named under *What*.

## Alternatives considered

- **Leave the comma as written, the alternative the issue names** — rejected: the phase already rewrites the lines around the comma (FMT-BREAK-021 puts every element on its own line), so "as written" no longer exists for the comma's line, and comma-first code is not a rare input class that `AGENTS.md` (*Code-fix coverage*) lets the formatter skip.
- **Decide only the object initializer of the issue's example** — rejected: the same comma is left at the statement's column in every construct of FMT-BREAK-021 and FMT-BREAK-022 (traced on `main`), so a narrower decision leaves the other members of the class with today's output.

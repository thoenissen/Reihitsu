# No blank line between an own-line comment and any brace or chained keyword behind it

| | |
|---|---|
| Status | Accepted |
| Date | 2026-11 |
| Affected requirements | FMT-BLANK-020 (changed: title, token list, and why) |
| Depends on | [The blank-line phase alone decides blank lines next to braces and comments](blank-line-sole-owner.md) |

## What

FMT-BLANK-020 removes the blank lines between an ordinary comment on its own line and the token behind it for a fixed list of tokens. The list leaves out the braces of an array initializer, of an extension block, and of an interpolation, the opening brace of a property pattern, the closing brace of a collection initializer assigned to a member, and the `else`, `catch`, `finally`, and do-statement `while` keywords. Behind a comment on its own line the formatter keeps the author's blank line in front of them, while it removes the same blank line in front of every other brace and directly in front of these tokens (FMT-BLANK-007, FMT-BLANK-008).

With this change, FMT-BLANK-020 names every token FMT-BLANK-007, FMT-BLANK-008, and FMT-BLANK-019 name: every opening and closing brace other than the opening brace of a block statement that follows another statement (FMT-BLANK-017, unchanged), the chained keywords, the closing bracket of a collection expression or a list pattern, and the closing parenthesis of a parenthesized pattern. The sentence that leaves the excluded braces undecided goes.

RH5024, RH5025, RH5026, and RH5027 are unchanged.

## Why

RH5024, RH5025, RH5026, and RH5027 report a blank line directly above a brace, a chained keyword, or a do-statement `while`, also behind an own-line comment, and their fixes remove it. Behind the tokens FMT-BLANK-020 leaves out, the formatter's output keeps that blank line, so a formatted file is reported by those rules for as long as nobody applies their fixes. The two do not fight each other — once a fix has removed the blank line, formatting keeps the removal — but a user who formats and does not run the fixes keeps diagnostics on formatted code. The exclusions were kept in [The blank-line phase alone decides blank lines next to braces and comments](blank-line-sole-owner.md) only to keep the formatter's earlier output, and FMT-BLANK-020 states that it does not decide them. The same change already removes the blank lines above a comment glued to every one of these tokens (its visible output change 16), so one rule then holds for every brace.

## Visible output changes

In each, FMT-BLANK-011 puts a blank line above the comment before and after, unchanged.

### 1. Array initializer, both braces

```csharp
// input
class C
{
    void M()
    {
        int[] g;
        g = new int[]
            // c

            {
                1,
                2
                // d

            };
    }
}
```

```csharp
// before
        g = new int[]

            // c

            {
                1,
                2

                // d

            };
```

```csharp
// after
        g = new int[]

            // c
            {
                1,
                2

                // d
            };
```

The same holds in a field's array initializer, in a nested element of a multi-dimensional one, in `new[] { … }`, and in `stackalloc int[] { … }`:

```csharp
// input
class C
{
    int[] G =
    {
        1
        // d

    };

    int[,] H =
    {
        { 1 },
        // e

        { 2 }
    };
}
```

```csharp
// before
    int[] G = {
                  1

                  // d

              };

    int[,] H = {
                   { 1 },

                   // e

                   { 2 }
               };
```

```csharp
// after
    int[] G = {
                  1

                  // d
              };

    int[,] H = {
                   { 1 },

                   // e
                   { 2 }
               };
```

### 2. Extension block, both braces

```csharp
// input
static class E
{
    extension(int i)
    // g

    {
        public int D => i;
        // h

    }
}
```

```csharp
// before
    extension(int i)

    // g

    {
        public int D => i;

        // h

    }
```

```csharp
// after
    extension(int i)

    // g
    {
        public int D => i;

        // h
    }
```

### 3. Opening brace of a property pattern

```csharp
// input
        if (o is C
            // c

            { })
        {
        }
```

```csharp
// before
        if (o is C

        // c

        { })
```

```csharp
// after
        if (o is C

        // c
        { })
```

### 4. Closing brace of a collection initializer assigned to a member

```csharp
// input
        var p = new P
                {
                    L =
                    {
                        1
                        // d

                    }
                };
```

```csharp
// before
                    L = {
                            1

                            // d

                        }
```

```csharp
// after
                    L = {
                            1

                            // d
                        }
```

### 5. Closing brace of an interpolation

```csharp
// input
        var s = $$"""
            {{
                a
                // f

            }}
            """;
```

```csharp
// before
        var s = $$"""
                  {{
                      a

                      // f

                  }}
                  """;
```

```csharp
// after
        var s = $$"""
                  {{
                      a

                      // f
                  }}
                  """;
```

The same holds in a regular interpolated string; the column the formatter gives a comment in front of a hole's closing brace there is not part of this change. The opening brace of an interpolation follows the string's text directly, so no comment can stand in front of it.

### 6. Chained keywords

```csharp
// input
class C
{
    void M(int a)
    {
        if (a > 0)
        {
            a++;
        }
        // c

        else
        {
            a--;
        }

        do
        {
            a++;
        }
        // f

        while (a > 0);
    }
}
```

```csharp
// before
        }

        // c

        else
        ...
        }

        // f

        while (a > 0);
```

```csharp
// after
        }

        // c
        else
        ...
        }

        // f
        while (a > 0);
```

The same holds for `catch` and `finally`.

## Decisions

### Which side yields: the formatter or the four rules

Decided by the maintainer (output question 2 of the run for thoenissen/Reihitsu#1543, answer A): the formatter removes the blank line between an own-line comment and every brace behind it, other than the opening brace of a block statement that follows another statement (FMT-BLANK-017, unchanged), and between such a comment and `else`, `catch`, `finally`, and a do-statement's `while`. RH5024, RH5025, RH5026, and RH5027 are unchanged.

### One token set for the blank line in front of a token and behind a comment

FMT-BLANK-020 names the tokens FMT-BLANK-007, FMT-BLANK-008, and FMT-BLANK-019 name, instead of a list of its own with exclusions. The exclusions were kept only to keep the formatter's earlier output; with them gone, the brace's construct no longer decides the blank line, only whether the brace opens a block statement that follows another statement. The exclusion of a collection initializer's closing brace by whether it is assigned to a member goes with them, so the blank-line phase no longer asks the line-break phase's question for it.

### The issue's own input

The issue's input — a comment between a field's `=` and its array initializer — is refused by the precheck (FMT-UNSUP-004) and stays unchanged under this change, as every file holding that shape does. The visible output changes show the same blank lines in an assignment and inside an initializer, which the precheck does not refuse.

## Alternatives considered

- **RH5024, RH5025, RH5026, and RH5027 stop reporting a blank line behind an own-line comment in front of these tokens** (B) — rejected by the maintainer: four rules gain an exemption for the braces of particular constructs, which a user cannot tell from other braces, and the formatter keeps two rules for one shape.
- **Declare an own-line comment directly in front of `else`, `catch`, `finally`, or do-`while` unsupported** — not proposed: RH5037 already reports the comment, and the declaration would delete no special case that this change does not delete anyway.

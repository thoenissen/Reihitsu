# Parenthesized lambdas keep their line, and an anonymous method in a raw-string hole keeps its lines

| | |
|---|---|
| Status | Draft |
| Date | 2026-11 |
| Affected requirements | FMT-BREAK-011 (changed), FMT-BREAK-002 (changed), FMT-PIPE-025 (added) |

## What

- **The opening parenthesis of a parenthesized lambda has no declaration-line rule** (FMT-BREAK-011). FMT-BREAK-011 keeps its comma and closing-parenthesis clauses for every parameter list, lambdas included, and its opening-parenthesis clause for every parameter list except a parenthesized lambda's. The gap in front of the lambda belongs to the requirement that owns it — FMT-BREAK-015 inside a list element, FMT-BREAK-012 for the first element of a list, FMT-BREAK-016 for an assignment's value — and where none joins it, the line-break phase leaves its line break and blank lines as written (FMT-BREAK-030). Until now the line-break phase stripped the indentation and the blank lines in front of a lambda argument that starts its own line, and the indentation phase wrote the indentation back.
- **No line break goes into an anonymous method in a raw-string hole** (FMT-PIPE-025). Inside an interpolation hole of a multi-line interpolated raw string literal, no phase inserts a line break between an anonymous method's `delegate` keyword and its closing brace, unless the anonymous method lies inside the block body of a lambda in the same hole. This covers the line-break phase (braces, statements, list elements) and the blank-line phase (the line break it writes together with a blank line). Line breaks the author wrote there are kept and move with the literal (FMT-RAW-007). FMT-BREAK-002 names the exception; the line-break and blank-line specs point to it in their non-goals.

## Why

**Parenthesized lambdas.** FMT-BREAK-011's opening-parenthesis clause listed parenthesized lambdas, but a lambda has no name in front of its parenthesis. For a lambda argument on its own line, the line break sits behind the `,` in front, outside the parameter list, so the collapse could not join; it stripped the parenthesis's leading trivia instead, and the indentation phase wrote the indentation back. The output looked unchanged, but the two phases undid each other on every pass, and the strip took the blank lines in front of the lambda with it, against FMT-BREAK-030, and the indentation of a lambda that starts a hole line no alignment places, against FMT-RAW-007. RH5105 already leaves a lambda's opening parenthesis unreported, and `ListElementInteriorJoinRewriter` already treats its gap as unowned by the list rules.

**Raw-string holes.** The line-break and blank-line phases broke an anonymous method in a hole onto new lines. No alignment places those lines: FMT-RAW-007 forbids the scope-depth column inside a hole, and no alignment measures an anonymous method's lines from a token inside the literal. The lines kept the column the breaking phase left (column 0) and moved only with the literal, so the same tokens ended in column 13 or in column 0 depending on how far the literal stood off in the input. A block lambda is aligned to the lambda (FMT-INDENT-028), which lies inside the literal, so its body and the anonymous methods in it are placed and are broken as before.

## Visible output changes

Each entry shows the formatter's output on `main` (from `scripts/format.sh` at e094895) and the intended output after this change.

### 1. An anonymous method in a hole of a literal the formatter moves stays on one line

```csharp
class C
{
    int M(System.Func<int> f)
    {
        var a = $"""
    Hello {M(delegate { return 1; })}
    """;
    }
}
```

Before:

```csharp
class C
{
    int M(System.Func<int> f)
    {
        var a = $"""
                 Hello {M(delegate
             {
             return 1;
             })}
                 """;
    }
}
```

After:

```csharp
class C
{
    int M(System.Func<int> f)
    {
        var a = $"""
                 Hello {M(delegate { return 1; })}
                 """;
    }
}
```

### 2. An anonymous method in a hole of an aligned literal stays on one line

```csharp
class C
{
    int M(System.Func<int> f)
    {
        var a = $"""
                 Hello {M(delegate { return 1; })}
                 """;
    }
}
```

Before:

```csharp
class C
{
    int M(System.Func<int> f)
    {
        var a = $"""
                 Hello {M(delegate
{
return 1;
})}
                 """;
    }
}
```

After: unchanged input.

### 3. Statements of such an anonymous method get no line break from the blank-line rules

```csharp
class C
{
    int M(System.Func<int> f)
    {
        var a = $"""
                 Hello {M(delegate { int x = 1; return x; })}
                 """;
    }
}
```

Before:

```csharp
class C
{
    int M(System.Func<int> f)
    {
        var a = $"""
                 Hello {M(delegate
{
int x = 1;

return x;
})}
                 """;
    }
}
```

After: unchanged input.

### 4. A block lambda inside such an anonymous method stays on its line too

```csharp
class C
{
    int M(System.Func<int> f)
    {
        var a = $"""
                 Hello {M(delegate { return M(() => { return 1; }); })}
                 """;
    }
}
```

Before:

```csharp
class C
{
    int M(System.Func<int> f)
    {
        var a = $"""
                 Hello {M(delegate
{
return M(() =>
         {
             return 1;
         });
})}
                 """;
    }
}
```

After: unchanged input.

### 5. The blank line in front of a lambda argument is kept

```csharp
using System.Linq;

class C
{
    void M(int[] items)
    {
        var total = items.Aggregate(0,

                                    (sum, item) => sum + item);
    }
}
```

Before:

```csharp
using System.Linq;

class C
{
    void M(int[] items)
    {
        var total = items.Aggregate(0,
                                    (sum, item) => sum + item);
    }
}
```

After: unchanged input, as for any other argument (`Aggregate(0,⏎⏎5)` keeps its blank line on `main`). FMT-BREAK-030 already demands this output.

### 6. A lambda that starts a hole line no alignment places keeps its column

```csharp
using System;

class C
{
    int M(Func<int, int> f, int v)
    {
        var a = $"""
                 A {M(v switch
                      {
                          _ =>
                              (x) => x
                      }, 1)}
                 """;

        return 0;
    }
}
```

Before:

```csharp
using System;

class C
{
    int M(Func<int, int> f, int v)
    {
        var a = $"""
                 A {M(v switch
                      {
                          _ =>
(x) => x
                      },
                      1)}
                 """;

        return 0;
    }
}
```

After:

```csharp
using System;

class C
{
    int M(Func<int, int> f, int v)
    {
        var a = $"""
                 A {M(v switch
                      {
                          _ =>
                              (x) => x
                      },
                      1)}
                 """;

        return 0;
    }
}
```

FMT-RAW-007 already demands this output: the literal is aligned, so the line stays where it is.

### Unchanged

- A lambda argument on its own line without a blank line above it (`Aggregate(0,⏎(sum, item) => sum + item)`) keeps its output; only the strip-and-repaint between the two phases goes.
- A block lambda in a hole is broken and aligned as before (`BlockLambdaBrokenInsideHoleOfAlignedLiteralKeepsItsAlignment`), and so is an anonymous method inside its block body.
- An anonymous method the author wrote over several lines in a hole keeps those lines.
- An anonymous method in a hole of a single-line raw string literal is broken as before.

## Decisions

### FMT-BREAK-011 keeps the comma and closing-parenthesis clauses for lambdas

RH5106 and RH5107 report a lambda's misplaced closing parenthesis and comma, and both are inside the parameter list, where the collapse can and does join. Only the opening clause, whose gap lies outside the list, is dropped for lambdas.

### "No alignment places the lines" is decided by the construct

The requirement names the construct — an anonymous method in the hole that no block lambda of the same hole contains — rather than asking whether the indentation phase would place the resulting lines. The phases that insert line breaks run before the indentation phase, and deciding from the layout it will compute would be a phase predicting another phase's result. Every construct that the line-break and blank-line phases break inside a hole was checked on `main`: initializers, anonymous objects, switch expressions, nested conditionals, wrapped chains, operators and lists are placed by their alignment requirements; block lambdas by FMT-INDENT-028; the anonymous method is the one whose lines nothing places.

### One statement for all phases

The rule binds two phases — the line-break phase and the blank-line phase both write line breaks between statements — so it is stated once, in the pipeline spec, and the two phase specs point to it.

### The test expectation changes

`AnonymousMethodBrokenInsideHoleIsStableOnTheSecondPass` checked only second-pass stability of the broken output; it changes to the one-line hole of visible output change 1.

## Alternatives considered

- **The collapse owns parenthesized lambdas fully and joins a lambda argument onto the previous line** (`items.Aggregate(0, (sum, item) => sum + item)`) — rejected: it conflicts with FMT-BREAK-012, which keeps a wrapped list at one element per line.
- **Lines created inside a hole get block indentation from the hole's first line** — rejected: it needs a placement rule for constructs inside holes that FMT-RAW-007 deliberately leaves to the literal's shift.
- **Align a broken anonymous method in a hole under its `delegate` keyword, like a block lambda under its anchor** — rejected: outside holes an anonymous method's braces take the scope-depth column, not an anchor, so the hole would get a layout of its own; and it adds a new alignment requirement where keeping the author's lines needs none.
- **Only the line-break phase refrains** — rejected: the blank-line phase writes line breaks between statements on one line too (visible output change 3), and those lines would be unplaced in the same way.
- **Extend FMT-RAW-007 instead of adding a pipeline requirement** — rejected: FMT-RAW-007 is owned by the indentation phase's raw-string types and describes how lines move, not which line breaks earlier phases may insert.

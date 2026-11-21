# The stripped indentation of a tab-indented raw string is written as spaces

| | |
|---|---|
| Status | Draft |
| Date | 2026-11 |
| Affected requirements | FMT-RAW-010 (new), FMT-RAW-001, FMT-RAW-002, FMT-RAW-006, FMT-TAB-003, FMT-INDENT-011, RH5202-007 (new), RH5202-003 |

## What

When the whitespace in front of the closing marker of a multi-line raw string literal holds a tab, the formatter writes the indentation the compiler strips as spaces (FMT-RAW-010): after the literal's lines move as today (FMT-RAW-001, FMT-RAW-002), every whitespace character at the start of the closing line, and every whitespace character at the start of a content line that lies within the width of the closing line's whitespace, becomes one space. The closing marker and the content then start with spaces only, in the column of the opening quotes; whitespace behind that width, a tab included, is part of the value and stays.

The rule applies to an aligned literal too, so FMT-RAW-006 leaves alone only a literal whose closing whitespace holds no tab. A literal whose closing whitespace holds no tab — spaces only, or other whitespace characters such as a no-break space — keeps today's output (FMT-RAW-002).

The RH5202 code fix writes the same spaces on the lines it shifts (RH5202-007), so its result stays the formatter's layout of the literal (SHARED-FIX-007). Every code fix that lays out its result through the formatter, such as the RH3202 fix converting an expression body that holds such a literal, gets the formatter's new layout.

No column changes: each character becomes one character, so every token on a moved line keeps the column, counted in characters, that it has today, and every alignment measured after the move (FMT-INDENT-030) lands where it does today. The RH5202 analyzer measures the closing whitespace as today and reports the same literals.

## Why

The compiler strips from every content line exactly the whitespace in front of the closing marker, so that whitespace is no part of the literal's value. Today the formatter counts each tab in it as one column and keeps it, so a tab-indented literal comes out as spaces followed by tabs: the indentation is mixed, and in an editor that renders a tab wider than one column, the closing marker and the content do not stand under the opening quotes. A line of an interpolation hole that is aligned to a token on a content line is measured in characters and stands visually beside that token. Writing the stripped indentation as spaces makes the character column the visual column, keeps the value, and needs no tab width.

## Visible output changes

`→` stands for one tab. Every *before* is the output of `main` from `scripts/format.sh` or `scripts/apply-fix.sh`.

**1. A misaligned literal indented with tabs.**

```csharp
// input
class C
{
    void M()
    {
        var a = """
→→Hello
→→""";
    }
}
```

```csharp
// before: 14 spaces, then the two tabs
class C
{
    void M()
    {
        var a = """
              →→Hello
              →→""";
    }
}
```

```csharp
// after
class C
{
    void M()
    {
        var a = """
                Hello
                """;
    }
}
```

**2. A content line indented further than the closing marker keeps the extra tab.**

```csharp
// input
class C
{
    void M()
    {
        var a = """
→→→Hello
→→""";
    }
}
```

```csharp
// before
class C
{
    void M()
    {
        var a = """
              →→→Hello
              →→""";
    }
}
```

```csharp
// after
class C
{
    void M()
    {
        var a = """
                →Hello
                """;
    }
}
```

**3. A literal aligned by its character count is rewritten, without moving.** The closing marker stands behind 12 spaces and four tabs, in column 16 counted in characters; this is today's output for the four-tab example of FMT-RAW-001, and FMT-RAW-006 leaves it unchanged today.

```csharp
// input = before
class C
{
    void M()
    {
        var a = """
            →→→→x
            →→→→""";
    }
}
```

```csharp
// after
class C
{
    void M()
    {
        var a = """
                x
                """;
    }
}
```

**4. A literal that moves to the left.** The closing marker stands behind twenty tabs; the move removes four of them, as today, and the sixteen left become spaces.

```csharp
// input = before
class C
{
    void M()
    {
        var a = """
→→→→→→→→→→→→→→→→→→→→x
→→→→→→→→→→→→→→→→→→→→""";
    }
}
```

`main` writes sixteen tabs in front of `x` and of the closing marker.

```csharp
// after
class C
{
    void M()
    {
        var a = """
                x
                """;
    }
}
```

**5. A content line holding only part of the closing whitespace.** The compiler accepts a content line that holds only whitespace and is shorter than the closing line's whitespace, when it is a start of it; here it holds one tab. It contributes an empty line to the value either way. The line moves as today, 14 columns, and its tab becomes a space: 15 spaces, shown as `·`.

```csharp
// input
class C
{
    void M()
    {
        var a = """
→→Hello
→
→→""";
    }
}
```

```csharp
// before: 14 spaces and one tab on the middle line
class C
{
    void M()
    {
        var a = """
              →→Hello
              →
              →→""";
    }
}
```

```csharp
// after
class C
{
    void M()
    {
        var a = """
                Hello
···············
                """;
    }
}
```

**6. A hole line aligned to a token on a tab-indented content line.** The wrapped argument stays in column 38, counted in characters, as today; with the tab in front of `Hello` written as a space, column 38 is also where `"a"` starts on screen.

```csharp
// input
class C
{
    void M()
    {
        var a = $"""
→Hello {string.Concat("a",
    "b")}
→""";
    }
}
```

```csharp
// before
class C
{
    void M()
    {
        var a = $"""
                →Hello {string.Concat("a",
                                      "b")}
                →""";
    }
}
```

```csharp
// after
class C
{
    void M()
    {
        var a = $"""
                 Hello {string.Concat("a",
                                      "b")}
                 """;
    }
}
```

**7. The RH5202 code fix.** For the input of change 1, the fix today writes the formatter's *before* of change 1; it writes the formatter's *after* of change 1. For a literal aligned by its character count (change 3), RH5202 reports nothing, as today, and no fix is offered.

**8. Code fixes that lay out their result through the formatter.** Their result follows changes 1 to 6 wherever the node they format holds such a literal. For example, the RH3202 fix:

```csharp
// input
class C
{
    string M() => """
→→Hello
→→""";
}
```

```csharp
// before
class C
{
    string M()
    {
        return """
             →→Hello
             →→""";
    }
}
```

```csharp
// after
class C
{
    string M()
    {
        return """
               Hello
               """;
    }
}
```

No diagnostic, rule page, or CLI behavior changes.

## Decisions

### Only a tab in the closing whitespace triggers the rewrite

The rewrite applies when the whitespace in front of the closing marker holds at least one tab, and then turns every whitespace character within that width into a space, a no-break space among them. A literal whose closing whitespace holds no tab keeps today's output, so the no-break-space example of FMT-RAW-002 does not change. The issue asks for tabs; other whitespace characters render one column wide in common editors, so they do not misalign the literal.

### One space per character, after the move

Each whitespace character of the stripped indentation becomes one space, after the move of FMT-RAW-001 and FMT-RAW-002. The move stays the one measure RH5202 and the formatter share (every whitespace character one column), so no column changes, no anchor is measured differently, and the analyzer needs no change. Replacing the closing whitespace by the spaces of the target column, as the issue phrases it, gives the same output on every line that holds all of it; on a whitespace-only line holding only a start of it (change 5), one space per character is the rule that needs no second case.

### Whitespace behind the stripped width stays

Whitespace in front of a content line beyond the width of the closing whitespace is part of the literal's value (FMT-PIPE-002) and stays as written, a tab included.

## Alternatives considered

- **Keep tab-indented raw strings a non-goal** — rejected: it keeps the mixed indentation and the visual misalignment the issue reports.
- **Count a tab as the width of a tab stop** — rejected: the width depends on the reader's editor, every other column of the formatter counts a tab after FMT-TAB-001 replaced it, and RH5202 would need a second measure.
- **Rewrite every whitespace character that is not a space, without a tab** — rejected for this change: it changes the output of FMT-RAW-002 for a no-break space, which nobody asked for.
- **Leave an aligned literal alone (FMT-RAW-006 unchanged)** — rejected: the output would depend on whether the input happened to be aligned in characters, and the mixed indentation of change 3 is exactly today's output for a tab-indented input.
- **Write a whitespace-only line shorter than the closing whitespace as the full width, or empty** — rejected in favour of one space per character; each keeps the value, but needs a rule of its own for that line.

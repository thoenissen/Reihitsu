# Formatter — Raw-string alignment

| | |
|---|---|
| ID prefix | FMT-RAW |
| Status | Active |
| Code owners | `RawStringAlignmentPhase`, `RawStringLiteralUtilities` |
| Uses | FMT-PIPE-001, FMT-PIPE-002, FMT-PIPE-004 |

This spec covers the raw-string phase: after indentation has placed every token, the content lines and the closing marker of each multi-line raw string literal are moved so the closing marker starts in the column of the opening quotes. The literal's value never changes.

## Non-goals

- The position of the opening quotes — they move with the statement, decided by FMT-BREAK, FMT-SPACE, and FMT-INDENT.
- Single-line raw strings, regular strings, and verbatim (`@"…"`) strings, whose line starts are part of the value.
- The layout of interpolation holes that span several lines, and of raw strings nested inside an interpolation hole. Their intended layout is decided as a bug, tracked in thoenissen/Reihitsu#951.
- Raw strings whose indentation uses tab characters. Their intended alignment is decided as a bug, tracked in thoenissen/Reihitsu#951.

## Requirements

### FMT-RAW-001 — The closing marker starts under the opening quotes

For every multi-line raw string literal — plain, UTF-8 (`u8`), or interpolated — the formatter MUST move the closing marker so it starts in the column of the first `"` of the opening marker, and MUST move every content line by the same number of columns.

Why: The column of the closing marker defines how much indentation the compiler strips from every content line. Lining it up under the opening quotes makes the literal read as one block attached to the code that owns it, wherever indentation moved that code.

Example:

```csharp
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

```csharp
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

### FMT-RAW-002 — Relative indentation inside the literal is kept

The formatter MUST shift all content lines of a literal by one uniform amount — to the right or to the left — so the indentation of each content line relative to the closing marker stays as written. Lines that are empty MUST stay empty.

Why: The relative indentation is the literal's value (FMT-PIPE-002). A uniform shift together with the closing marker changes only where the block sits in the file, not one character of the string it produces.

Example:

```csharp
class C
{
    void M()
    {
        var a = """
                        Hello
                            World
                        """;
    }
}
```

```csharp
class C
{
    void M()
    {
        var a = """
                Hello
                    World
                """;
    }
}
```

and a literal whose content is indented further than its closing marker keeps that extra indentation:

```csharp
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

```csharp
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

### FMT-RAW-003 — Interpolated raw strings align under the quotes, not the `$`

For an interpolated raw string, the formatter MUST take the column of the first `"` after the `$` characters as the target column, so the content and the closing marker start under the quotes regardless of how many `$` precede them.

Why: The `$` prefix is not part of the quote block. Aligning under the `$` would put the closing marker one or more columns to the left of the opening quotes, which reads as a misaligned block.

Example:

```csharp
class C
{
    void M()
    {
        var name = "x";
        var a = $$"""
    {"a": {{name}}}
    """;
    }
}
```

```csharp
class C
{
    void M()
    {
        var name = "x";
        var a = $$"""
                  {"a": {{name}}}
                  """;
    }
}
```

### FMT-RAW-004 — UTF-8 raw strings are aligned like plain ones

The formatter MUST apply FMT-RAW-001 and FMT-RAW-002 to multi-line UTF-8 raw string literals (`"""…"""u8`).

Why: The `u8` suffix changes the literal's type, not how its indentation is stripped.

Example:

```csharp
class C
{
    void M()
    {
        var a = """
    Hello

      Indented
    """u8;
    }
}
```

```csharp
class C
{
    void M()
    {
        var a = """
                Hello

                  Indented
                """u8;
    }
}
```

### FMT-RAW-005 — Alignment follows the final position of the opening quotes

The formatter MUST compute the target column from the position the opening quotes have after the earlier phases (FMT-PIPE-001), so a raw string whose statement was re-indented, joined onto the previous line, or passed as an argument is aligned to where its opening quotes ended up.

Why: Indentation and line-break phases move only the token that starts a line; the content lines of a raw string are part of the token text and do not move with it. This phase runs after them to re-attach the content to its opening quotes.

Example — the statement is re-indented and the literal follows it:

```csharp
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

```csharp
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

and a literal joined onto the line of its assignment is aligned to its new column:

```csharp
class C
{
    void M()
    {
        var a =
            """
    Hello
    """;
    }
}
```

```csharp
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

### FMT-RAW-006 — Aligned and single-line literals are left alone

The formatter MUST NOT change a raw string literal that is on one line or whose closing marker already starts in the target column of FMT-RAW-001.

Why: A single-line raw string has no content lines to align, and an aligned literal is already canonical; rewriting it would only risk churn (FMT-PIPE-004).

Example:

```csharp
class C
{
    void M()
    {
        var a = """
                Hello
                """;
        var b = """Hello""";
    }
}
```

is left unchanged.

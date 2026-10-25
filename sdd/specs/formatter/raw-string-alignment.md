# Formatter — Raw-string alignment

| | |
|---|---|
| ID prefix | FMT-RAW |
| Status | Active |
| Code owners | `RawStringLineShifts`, `RawStringLiteral`, `RawStringLiteralUtilities` |
| Uses | FMT-PIPE-001, FMT-PIPE-002, FMT-PIPE-004, FMT-INDENT-002, FMT-INDENT-004, FMT-INDENT-006, FMT-INDENT-007, FMT-INDENT-011, FMT-INDENT-012, FMT-INDENT-029, FMT-INDENT-030, FMT-TAB-001 |

This spec covers how the indentation phase moves the lines of a multi-line raw string literal: once the phase has placed the line of the opening quotes, it moves the literal's content lines, the lines of its interpolation holes, and its closing marker so the closing marker starts in the column of the opening quotes. The move belongs to the indentation phase (FMT-PIPE-001), so every alignment of that phase is measured after it (FMT-INDENT-030). The literal's value never changes.

## Non-goals

- The position of the opening quotes — they move with the statement, decided by FMT-BREAK, FMT-SPACE, and FMT-INDENT.
- Single-line raw strings, regular strings, and verbatim (`@"…"`) strings, whose line starts are part of the value.
- Raw strings whose indentation uses tab characters. Their alignment is a planned extension, tracked in thoenissen/Reihitsu#1432.

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

The formatter MUST compute the target column from the column the opening quotes have in the output of the indentation phase, the same phase that moves the literal's lines, so a raw string whose statement was re-indented, joined onto the previous line, or passed as an argument is aligned to where its opening quotes end up. That column counts everything in front of the opening quotes on their line, including a comment that leads the line (FMT-INDENT-030).

Why: Placing a line moves only the token that starts it; the content lines of a raw string are part of the literal's token text and do not move with it. The indentation phase decides where the opening quotes go, so it is the phase that re-attaches the content to them; a later phase would have to wait for that decision, and every line the indentation phase aligns to a token on the literal's lines would then have to predict the move (FMT-INDENT-030).

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

and on a line that a comment leads, the literal is aligned to the opening quotes behind the comment, in column 24, not to the column the line starts in:

```csharp
class C
{
    void M()
    {
    /* c */ var s = """
    a
    """;
    }
}
```

```csharp
class C
{
    void M()
    {
        /* c */ var s = """
                        a
                        """;
    }
}
```

### FMT-RAW-006 — Aligned and single-line literals are left alone

For a raw string literal that is on one line, or whose closing marker already starts in the target column of FMT-RAW-001, the formatter MUST NOT move its content lines, its closing marker, or the lines of its interpolation holes that FMT-RAW-007 moves only with the literal.

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

### FMT-RAW-007 — Hole lines move only with the literal unless aligned within it

For a multi-line interpolated raw string, a line that starts inside one of its interpolation holes with a token or a comment is placed by one of three conditions:

- When an alignment requirement (FMT-INDENT-012 to FMT-INDENT-029) places the line relative to a token that lies inside the literal — in the same hole or on one of the literal's content lines — and the construct it aligns lies inside the hole, the formatter MUST keep that alignment, measured at the token's column after the literal moved.
- When a comment starts the line, the formatter MUST give it the column FMT-INDENT-006 gives it — the column of the token it precedes, or one level deeper than a closing brace it precedes — measured at that token's column after this requirement placed it. When another comment leads the line of that token, the token's line starts with that comment, and the comment above takes the column that line starts in, as on any other line (FMT-INDENT-006); the column of the token behind the leading comment does not decide it.
- Any other line that a token starts, the formatter MUST NOT re-indent, whatever column the indentation requirements would give it: a column from the scope depth (FMT-INDENT-002 to FMT-INDENT-004), or a column that a construct outside the hole sets, such as the body of a lambda around the literal. It MUST move the line by the same number of columns as the closing marker (FMT-RAW-001, FMT-RAW-002), and, when the closing marker already starts in the target column, MUST leave the line where it is. Such lines are, for example, started by a token of the hole's expression that no alignment inside the hole places, by the `,` of an alignment clause, by the `:` of a format clause, or by the hole's closing `}`.

The whitespace in front of such a line holds no tab: FMT-TAB-001 replaced each with four spaces before any layout was computed, so the line's column is the number of characters in front of its first token. The lines of a raw string literal nested in a hole are aligned as FMT-RAW-008 states, and the lines that start inside the text of another nested literal or inside a nested comment as FMT-RAW-009 states.

Why: A column from the scope depth, or from a construct outside the hole, does not depend on where the literal sits, while the literal's shift moves the same line again. The two moves add up in one pass, and the next pass, with the literal aligned and no shift left, puts the line back at that column: the output changes on every pass (FMT-PIPE-004). A column measured from a token inside the literal moves with that token, so an aligned line already lands in the same place on every pass, and it stays the column the alignment rules check. A comment on its own line describes the code below it and sits in that code's column, wherever that code ends up.

This requirement does not specify the column that an earlier phase of the same pass gives a line it breaks inside a hole, nor the outcome for a hole line that is indented less than the number of columns the literal moves to the left.

Example — the literal moves 13 columns to the right, and the lines at the block column move only with it:

```csharp
class C
{
    void M()
    {
        var name = "x";
        var a = $"""
    Hello {
        name
    }
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
        var a = $"""
                 Hello {
                     name
                 }
                 """;
    }
}
```

and in an aligned literal those lines keep their columns:

```csharp
class C
{
    void M()
    {
        var name = "x";
        var a = $"""
                 Hello {
                         name
                 }
                 """;
    }
}
```

is left unchanged. A wrapped argument keeps its alignment to the opening parenthesis (FMT-INDENT-012), measured after the shift:

```csharp
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

```csharp
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

A literal inside a lambda body: the hole lines take neither the block column inside the body nor a column the lambda sets, and move only with the literal:

```csharp
class C
{
    void M(System.Action f)
    {
        var name = "x";

        M(() =>
          {
              var s = $"""
          Hello {
              name
          }
          """;
          });
    }
}
```

```csharp
class C
{
    void M(System.Action f)
    {
        var name = "x";

        M(() =>
          {
              var s = $"""
                       Hello {
                           name
                       }
                       """;
          });
    }
}
```

A comment that starts a hole line takes the column of the token below it, after that token moved with the literal:

```csharp
class C
{
    void M()
    {
        var name = "x";
        var a = $"""
    Hello {
    // c
        name
    }
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
        var a = $"""
                 Hello {
                     // c
                     name
                 }
                 """;
    }
}
```

A comment line above a hole line that another comment leads takes the column of that line's start, where the leading comment stands, not the column of `x` behind it:

```csharp
class C
{
    void M()
    {
        var x = "x";
        var a = $"""
    a {
    // above
    /* c */ x
    } b
    """;
    }
}
```

```csharp
class C
{
    void M()
    {
        var x = "x";
        var a = $"""
                 a {
                 // above
                 /* c */ x
                 } b
                 """;
    }
}
```

### FMT-RAW-008 — A raw string nested in a hole is aligned in the same pass

When a multi-line raw string literal — plain, UTF-8, or interpolated — stands inside an interpolation hole of a multi-line interpolated raw string, the formatter MUST align both literals in the same pass: the outer literal as FMT-RAW-001 and FMT-RAW-007 state, and the inner literal under its opening quotes at the column they have after the outer literal moved (FMT-RAW-005). This holds at every depth of nesting.

Why: The inner literal's opening quotes move with the outer literal's content lines, so its target column is known only after the outer shift. Aligning the inner literal only on the next pass leaves output that formatting changes again (FMT-PIPE-004).

Example — the outer literal moves 13 columns to the right, and the inner literal, carried to column 21 by that shift, moves on to column 24 under its opening quotes:

```csharp
class C
{
    void M()
    {
        var a = $"""
    Outer {"""
        inner
        """}
    """;
    }
}
```

```csharp
class C
{
    void M()
    {
        var a = $"""
                 Outer {"""
                        inner
                        """}
                 """;
    }
}
```

and when the outer literal is already aligned, only the inner literal moves:

```csharp
class C
{
    void M()
    {
        var a = $"""
                 Outer {"""
                     inner
                     """}
                 """;
    }
}
```

```csharp
class C
{
    void M()
    {
        var a = $"""
                 Outer {"""
                        inner
                        """}
                 """;
    }
}
```

### FMT-RAW-009 — Text nested in a hole keeps its lines

For a multi-line interpolated raw string, the formatter MUST leave a line as written when the line starts inside the text of a literal other than a raw string, or inside a comment, that began on an earlier line within one of the literal's interpolation holes — the continuation line of a verbatim string (`@"…"` or `$@"…"`), or a following line of a `/* */` comment. This holds also when the literal around the hole moves.

Why: The start of such a line is part of the nested literal's text or of the comment's text (FMT-PIPE-002, FMT-INDENT-007, FMT-INDENT-011). Shifting it with the outer literal would change the value of the nested string or the text of the comment.

Example — the literal moves 13 columns to the right; the continuation line of the verbatim string stays in column 0:

```csharp
class C
{
    void M()
    {
        var a = $"""
    Hello {@"a
b"}
    """;
    }
}
```

```csharp
class C
{
    void M()
    {
        var a = $"""
                 Hello {@"a
b"}
                 """;
    }
}
```

and the second line of a comment in a hole stays in column 2:

```csharp
class C
{
    void M()
    {
        var name = "x";
        var a = $"""
    Hello {name /* a
  b */
    }
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
        var a = $"""
                 Hello {name /* a
  b */
                 }
                 """;
    }
}
```

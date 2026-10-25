# Formatter — Tab replacement

| | |
|---|---|
| ID prefix | FMT-TAB |
| Status | Active |
| Code owners | `TabReplacementPhase` |
| Uses | FMT-PIPE-001, FMT-PIPE-002, FMT-PIPE-016, FMT-RAW-007 |

This spec covers `TabReplacementPhase`, the first phase of the pipeline (FMT-PIPE-001): the replacement of every tab in whitespace and between the tokens of a directive with four spaces, before any other phase runs, and the text in which tabs are kept.

## Non-goals

- The indentation of a line and the gaps between tokens on a line. FMT-INDENT and FMT-SPACE decide them later; this phase only replaces the tabs they would otherwise receive.
- Whitespace at the end of a line and the final line break — FMT-CLEAN.
- Tabs inside the text of a multi-line raw string literal, which are part of its value. Aligning raw strings whose indentation uses tab characters is a planned extension, tracked in thoenissen/Reihitsu#1432.

## Requirements

### FMT-TAB-001 — Tabs in whitespace become four spaces each

The formatter MUST replace every tab character in whitespace trivia with four spaces. The replacement MUST NOT depend on the column the tab starts in. This holds for whitespace anywhere in the code: in front of a line's first token, between two tokens, in front of a comment, at the end of a line, and in the code of an interpolation hole.

Why: The formatter's indentation unit is four spaces, and a tab renders at a width that depends on the reader's editor. All layout is computed in spaces, so the replacement runs before any phase measures a column; no phase has to count a tab's width (FMT-PIPE-016). A fixed width keeps the result independent of tab-stop settings.

Example — not shown as an input block because the input contains tabs: in a class whose fields read `int z;`, a tab, `// e` and `int w;`, a space, a tab, a space, `// f`, the gaps become four and six spaces:

```csharp
class C
{
    int z;    // e
    int w;      // f
}
```

### FMT-TAB-002 — Tabs inside directives become four spaces each

The formatter MUST replace every tab in the whitespace between the tokens of a preprocessor directive with four spaces, by the same rule as FMT-TAB-001. Free text that a directive carries — a region description, the message of `#error` or `#warning` — is directive content and MUST NOT be changed.

Why: The tokens of a directive live in structured trivia, which the token-level replacement does not reach, so the directive interior gets its own pass. A region description is user-authored text (FMT-PIPE-002); a tab inside it is part of the name and is repeated in the `#endregion` comment.

Example — not shown as an input block because the input contains tabs: `#define`, a tab, `DEBUG` and `#pragma warning`, a tab, `disable CS1591` become

```csharp
#define    DEBUG
class C
{
#pragma warning    disable CS1591
    int x;
}
```

while a tab inside the description of `#region Name` is left as it is. A tab between `#region` and its description is whitespace between the directive's tokens and becomes four spaces; a tab after the first word of the description is part of the description and stays.

### FMT-TAB-003 — Tabs in literals, comments, and disabled text are kept

The formatter MUST NOT replace a tab inside a comment, a documentation comment, a string or character literal (regular, verbatim, interpolated, or raw), or disabled text. Only whitespace trivia and the whitespace between directive tokens are changed. In an interpolated string, a tab in the literal's text is kept, while a tab in the whitespace of the code inside one of its holes is whitespace trivia and becomes four spaces (FMT-TAB-001).

Why: These are user-authored content (FMT-PIPE-002). A tab inside a literal changes the program's value, and inside a comment or disabled text it may be deliberate alignment. The code inside an interpolation hole is code like any other, so its whitespace follows FMT-TAB-001.

Example — not shown as blocks because the content is tabs: in

```csharp
class C
{
    // a b
    /* c d */
    string s = "e f";
    string t = @"g h
line";
#if NEVER
    disabled text
#endif
}
```

when each space inside the comments, inside the text of the literals, and inside the disabled text is a tab, every one of those tabs is kept.

Example — a hole of a multi-line interpolated raw string; the line `name` is indented with a tab followed by four spaces (not shown faithfully in Markdown), the content lines with four spaces:

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

The tab is whitespace in the hole's code and becomes four spaces, so the line starts in column 8 like the lines around it, and it moves with the literal (FMT-RAW-007):

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

A tab written between `Hello` and `{` instead is part of the literal's text and is kept.

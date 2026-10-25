# Formatter — Tab replacement

| | |
|---|---|
| ID prefix | FMT-TAB |
| Status | Active |
| Code owners | `TabReplacementPhase` |
| Uses | FMT-PIPE-001, FMT-PIPE-002, FMT-PIPE-016, FMT-RAW-007, FMT-REGION-001, FMT-INDENT-008 |

This spec covers `TabReplacementPhase`, the first phase of the pipeline (FMT-PIPE-001): the replacement of every tab in whitespace and between the tokens of a directive with four spaces, before any other phase runs, and the text in which tabs are kept.

## Non-goals

- The indentation of a line and the gaps between tokens on a line. FMT-INDENT and FMT-SPACE decide them later; this phase only replaces the tabs they would otherwise receive.
- Whitespace at the end of a line and the final line break — FMT-CLEAN.
- Tabs inside the text of a multi-line raw string literal, which are part of its value. Aligning raw strings whose indentation uses tab characters is a planned extension, tracked in thoenissen/Reihitsu#1432.

## Requirements

### FMT-TAB-001 — Tabs in whitespace become four spaces each

The formatter MUST replace every tab character in the whitespace trivia of the code with four spaces. The replacement MUST NOT depend on the column the tab starts in. The whitespace of the code is the whitespace trivia that a token outside every documentation comment and directive carries in front of or behind it, wherever it stands: in front of a line's first token, between two tokens, in front of a comment, at the end of a line, and in the code of an interpolation hole. The whitespace inside a documentation comment is not whitespace of the code and keeps its tabs (FMT-TAB-003); the whitespace between the tokens of a directive follows FMT-TAB-002.

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

A tab between `#region` and its description is whitespace between the directive's tokens and becomes four spaces in this phase; a tab after the first word of the description is part of the description and stays. Whether the four spaces reach the output depends on the region phase that runs later: when it rewrites the `#region` line, it writes one space in front of the description (FMT-REGION-001). So, through the whole pipeline, a region written as `#region`, a tab, `Bar` and closed by a bare `#endregion` keeps the four spaces, while `#region`, a tab, `foo` is rewritten to start with an upper-case letter and is written with one space:

```csharp
class C
{
    #region    Bar

    int y;

    #endregion // Bar

    #region Foo

    int x;

    #endregion // Foo
}
```

A region written as `#region`, a tab, `Baz`, a tab, `qux` keeps the tab between `Baz` and `qux`, in the `#region` line and in the description the `#endregion` comment repeats.

### FMT-TAB-003 — Tabs in literals, comments, and disabled text are kept

The formatter MUST NOT replace a tab inside a comment, a documentation comment, a string or character literal (regular, verbatim, interpolated, or raw), or disabled text. Only the whitespace of the code (FMT-TAB-001) and the whitespace between directive tokens (FMT-TAB-002) are changed. In an interpolated string, a tab in the literal's text is kept, while a tab in the whitespace of the code inside one of its holes is whitespace trivia and becomes four spaces (FMT-TAB-001). In a documentation comment, every tab is kept, also one in the whitespace between the parts of an XML element, such as between `param` and `name` in `<param name="x">`. The whitespace in front of the `///` marker of a continuation line is not replaced either; the indentation phase rewrites it as a whole when it indents the marker (FMT-INDENT-008), and then writes spaces.

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

Example — a documentation comment; not shown as an input block because the input contains tabs: in

```csharp
class C
{
    /// <param name="x">b</param>
    void M(int x)
    {
    }
}
```

when the space between `param` and `name` is a tab, that tab is kept, while a tab written in front of `void` instead is whitespace of the code and becomes four spaces (FMT-TAB-001), which the indentation phase then replaces with the member's indentation.

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

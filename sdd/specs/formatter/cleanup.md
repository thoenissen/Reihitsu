# Formatter — Cleanup

| | |
|---|---|
| ID prefix | FMT-CLEAN |
| Status | Active |
| Code owners | `CleanupPhase`, `EndOfLineWhitespace` |
| Uses | FMT-PIPE-002, FMT-PIPE-010, FMT-PIPE-012 |

This spec covers `CleanupPhase`: the removal of whitespace that ends a line, the replacement of tabs that earlier phases left in whitespace and inside directives, and the removal of the line breaks at the end of a file — including the cases where a file's final directive keeps its line break.

## Non-goals

- Collapsing runs of blank lines (FMT-BLANK-014) and deciding where blank lines go — the phase only removes whitespace on a blank line, never the line itself, except at the end of the file.
- Indentation and the gaps between tokens on a line — FMT-INDENT and FMT-SPACE replace leading tabs and normalize token gaps before this phase runs.
- Whitespace after `///` on a documentation line (FMT-DOC-005, FMT-DOC-006).
- Which character sequence a line break uses (FMT-PIPE-007, FMT-EOL).
- Whitespace at the end of a single-line comment, a documentation comment line, or a directive line, and whitespace behind the last token of a file that has no final line break — the phase currently leaves all of these in place. Removing that whitespace is decided: at the end of a comment or a documentation comment line as a behavior change, tracked in thoenissen/Reihitsu#959, and at the end of a directive line without message text and behind the last token as a bug, tracked in thoenissen/Reihitsu#960.

## Requirements

### FMT-CLEAN-001 — No whitespace before a line break

The formatter MUST remove the whitespace trivia between a token and the line break that ends its line, and the whitespace on a line that holds nothing else. This MUST hold whether the line break is part of the token's own trailing trivia or the first trivia of the following token.

Why: Trailing whitespace is invisible, carries no meaning, and shows up as noise in every diff that touches the line. A line break an earlier phase moved into the next token's leading trivia must not let the whitespace in front of it escape.

Example — not shown as blocks because the difference is trailing whitespace: in

```csharp
class C
{
    void M()
    {
        int x = 1;

        int y = 2;
    }
}
```

when `int x = 1;` is followed by three spaces and the blank line consists of eight spaces, both runs of spaces are removed and the blank line stays as an empty line.

### FMT-CLEAN-002 — Tabs in whitespace become four spaces each

The formatter MUST replace every tab character in whitespace trivia with four spaces. The replacement MUST NOT depend on the column the tab starts in.

Why: The formatter's indentation unit is four spaces, and a tab renders at a width that depends on the reader's editor. Earlier phases rebuild indentation and most token gaps; this rule catches the whitespace they keep as written, such as the gap before a trailing comment. A fixed width keeps the result independent of tab-stop settings.

Example — not shown as an input block because the input contains tabs: in a class whose fields read `int z;`, a tab, `// e` and `int w;`, a space, a tab, a space, `// f`, the gaps become four and six spaces:

```csharp
class C
{
    int z;    // e
    int w;      // f
}
```

### FMT-CLEAN-003 — Tabs inside directives become four spaces each

The formatter MUST replace every tab in the whitespace between the tokens of a preprocessor directive with four spaces, by the same rule as FMT-CLEAN-002. Free text that a directive carries — a region description, the message of `#error` or `#warning` — is directive content and MUST NOT be changed.

Why: The tokens of a directive live in structured trivia, which the token-level part of the phase does not reach, so the directive interior gets its own pass. A region description is user-authored text (FMT-PIPE-002); a tab inside it is part of the name and is repeated in the `#endregion` comment.

Example — not shown as an input block because the input contains tabs: `#define`, a tab, `DEBUG` and `#pragma warning`, a tab, `disable CS1591` become

```csharp
#define    DEBUG
class C
{
#pragma warning    disable CS1591
    int x;
}
```

while a tab inside the description of `#region Name` is left as it is.

### FMT-CLEAN-004 — Literals, comments, and disabled text are not cleaned

The formatter MUST NOT replace tabs or remove whitespace inside a comment, a documentation comment, a string or character literal (regular, verbatim, interpolated, or raw), or disabled text. Only whitespace trivia and the whitespace between directive tokens are cleaned.

Why: These are user-authored content (FMT-PIPE-002). A tab or a run of spaces inside a literal changes the program's value, and inside a comment or disabled text it may be deliberate alignment.

Example — not shown as blocks because the content is tabs and trailing whitespace: in

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

when each space inside the comments, the literals, and the disabled text is a tab, and the line `    disabled text` and the line `string t = @"g h` end in spaces, every tab and every one of those spaces is kept.

### FMT-CLEAN-005 — The file does not end with a line break

The formatter MUST remove every line break and whitespace-only line after the last token of a file, so the file ends with the last token's text.

Why: The formatter applies one fixed end-of-file shape, so a file never differs from its formatted form only in its trailing newlines.

Example:

```csharp
class C
{
    int x;
}


```

```csharp
class C
{
    int x;
}
```

### FMT-CLEAN-006 — A trailing comment keeps its own line

When comments follow the last token of a file, the formatter MUST keep the line breaks between that token and the comments and MUST remove only the line breaks after the last comment.

Why: Removing the last token's line break would join the comment onto the code line, which turns a block comment into a trailing comment and, for a single-line comment, changes what the reader sees as belonging to the last line of code.

Example:

```csharp
class C
{
    int x;
}

// end

```

```csharp
class C
{
    int x;
}

// end
```

### FMT-CLEAN-007 — A file ending in a directive keeps its final line break

When a preprocessor directive or disabled text follows the last token of a file, the formatter MUST keep the line break between that token and the directive and the line break that ends the last directive, and MUST remove only the blank lines after it. A file that contains no token besides directives MUST be left unchanged.

Why: A directive is a line of its own; its line break is part of the directive, not a separator after it. Joining a trailing `#pragma` onto the closing brace would produce a line the compiler rejects, and keeping the directive's line break is what keeps it on its own line.

Example — the input ends with two blank lines after the directive; the output ends with the directive's line break:

```csharp
class C
{
    int x;
}

#pragma warning restore CS1591


```

```csharp
class C
{
    int x;
}

#pragma warning restore CS1591
```

The same holds for disabled text at the end of a file: the following input is left unchanged, including the line break after `#endif`.

```csharp
class C
{
    int x;
}
#if NEVER
int ignored;
#endif
```

### FMT-CLEAN-008 — A detached node does not end with a line break

When `FormatNode` formats a node that is not part of a syntax tree (FMT-PIPE-012), the formatter MUST remove the line breaks at the end of the node's last token.

Why: A detached node has no following token, so it is treated like the end of a file. The caller inserts the node into its target and decides what follows it. The node-in-document entry points restore the target's original trailing trivia instead (FMT-PIPE-010).

### FMT-CLEAN-009 — The byte order mark is kept

The formatter MUST keep a byte order mark at the start of the text.

Why: Removing or adding a byte order mark changes the file's encoding signature, which tools and compilers may depend on; cleanup only removes whitespace and line breaks.

# Formatter — Documentation comments

| | |
|---|---|
| ID prefix | FMT-DOC |
| Status | Active |
| Code owners | `DocumentationCommentFormattingPhase`, `DocCommentElementNormalizer`, `DocumentationCommentUtilities` |
| Uses | FMT-CLEAN-009, FMT-PIPE-002, FMT-PIPE-004, FMT-PIPE-010, SHARED-DOC-017 |

This spec covers what the documentation-comment phase decides about single-line documentation comments (`///`): moving a comment written behind code onto its own line, the separator after each `///` exterior, and the line layout of the XML elements inside the comment. It is the normalization of documentation-comment text that FMT-PIPE-002 allows.

## Non-goals

- Delimited documentation comments (`/** … */`). This phase leaves their text and position alone (FMT-DOC-012); separating them from preceding code belongs to FMT-BLANK.
- Blank lines around a documentation comment, including the blank line that appears above a relocated comment — FMT-BLANK.
- The column of the `///` exterior and of the documented code — FMT-INDENT.
- The line-break sequence of the lines this phase writes — FMT-PIPE-007 and FMT-EOL.
- XML validity and content: the formatter does not add, remove, reorder, or rename elements or attributes, does not check `cref` targets, and does not reflow prose.
- Ordinary `//` comments, even when they look like documentation.

## Requirements

### FMT-DOC-001 — Documentation written behind code moves onto its own line

When a single-line documentation comment starts on a line after other source text, and the token that follows the comment opens the syntax node it belongs to, the formatter MUST move the comment onto a line of its own directly before that token. The whitespace between the preceding code and the comment MUST be removed, and the preceding code MUST stay on its line. This requirement covers comments before type and member declarations; it does not yet decide comments before a statement or inside an expression, which document nothing even when the following token opens a node.

Why: The compiler attaches a documentation comment to the declaration that follows it, so a comment behind `int A;` documents `B`, not `A`. Putting it on its own line above `B` makes the layout say what the compiler already does. The blank line above the moved comment is FMT-BLANK's doing; the node-level exception for this whitespace is FMT-PIPE-010.

Example:

```csharp
class C
{
    int A; /// <inheritdoc/>
    int B;
}
```

```csharp
class C
{
    int A;

    /// <inheritdoc/>
    int B;
}
```

The same holds behind an opening brace:

```csharp
class C
{ /// <inheritdoc/>
    void M()
    {
    }
}
```

```csharp
class C
{
    /// <inheritdoc/>
    void M()
    {
    }
}
```

### FMT-DOC-002 — Comments before the documentation stay in front of it

When the source text before a documentation comment on its line belongs to the leading trivia of the same token — for example a block comment at the start of a file — the formatter MUST insert the line break between that text and the documentation comment, keeping the earlier comment on its original line.

Why: The earlier comment is not code the documentation could belong to, but it still must not end up between the documentation and the code it documents, and its text must survive (FMT-PIPE-002).

Example:

```csharp
/* banner */ /// <inheritdoc/>
class C
{
    int A;
}
```

```csharp
/* banner */
/// <inheritdoc/>
class C
{
    int A;
}
```

### FMT-DOC-003 — Stranded documentation stays as it was written

The formatter MUST NOT move a documentation comment written behind code when the token that follows it does not open the syntax node it belongs to — such as a `;`, `)`, `]`, or `}`, a binary operator such as `+`, or the `.` of a member access — and MUST leave the text of every line of that comment as written: FMT-DOC-005 to FMT-DOC-011 do not apply to it.

Why: Such a comment documents nothing; the compiler reports CS1587 wherever it stands and does not read it as documentation, so it is an ordinary comment whose text the formatter keeps (FMT-PIPE-002). Moving it would change the author's layout without giving the comment a meaning, and laying out its elements would rewrite text that documents nothing, so the formatter keeps it as it is and leaves the decision to the author.

Example:

```csharp
class C
{
    void M()
    {
        var x = 1; /// stray
    }
}
```

is left unchanged, and so is a comment before a `;`:

```csharp
class C
{
    int A /// stray
        ;
}
```

Its elements are not laid out, and its separators are not normalized:

```csharp
class C
{
    int A; /// <summary>end</summary>
}
```

```csharp
class C
{
    int A; ///<remarks>a
    /// b</remarks>
}
```

are left unchanged.

### FMT-DOC-004 — Relocated documentation is normalized in the same run

A documentation comment that FMT-DOC-001 or FMT-DOC-002 moves MUST also receive the element and exterior normalization of FMT-DOC-005 to FMT-DOC-011 in the same run, unless FMT-DOC-013 leaves its text as written.

Why: A single run must produce the final shape (FMT-PIPE-004); a moved comment that is normalized only on the next run would make the formatter non-idempotent.

Example:

```csharp
class C
{
    int A; /// <summary>Gets B.</summary>
    int B;
}
```

```csharp
class C
{
    int A;

    /// <summary>
    /// Gets B.
    /// </summary>
    int B;
}
```

### FMT-DOC-005 — One space separates `///` from its content

On every line of a single-line documentation comment whose `///` exterior is followed by content, the formatter MUST make the first character after the exterior a single space: a missing separator MUST be inserted, and a separator that is another whitespace character, such as a tab, MUST be replaced by a space. Whitespace after that first character belongs to the content and MUST be kept.

This requirement does not apply to a comment that FMT-DOC-003 or FMT-DOC-013 leaves as written.

Why: One space is the canonical separator. Anything beyond it is indentation the author chose inside the content — for example in a `<code>` block or a continued list item — and must not be flattened.

Example:

```csharp
class C
{
    ///Gets the value.
    int A;
}
```

```csharp
class C
{
    /// Gets the value.
    int A;
}
```

A `///` followed by a tab and `Gets the value.` gives the same output, while this input, whose last line carries four spaces of content indentation, is left unchanged:

```csharp
class C
{
    /// <summary>
    /// Gets the value.
    /// </summary>
    ///    indented content
    int A;
}
```

### FMT-DOC-006 — Exterior-only lines

On a documentation line that carries only whitespace after `///`, the formatter MUST remove that whitespace, except that a single space MUST be kept.

This requirement does not apply to a comment that FMT-DOC-003 or FMT-DOC-013 leaves as written.

Why: A line made of `///` and whitespace has no content to separate, so the separator rule of FMT-DOC-005 has nothing to apply to; the formatter drops what cannot be a separator.

Example — not shown as blocks because the difference is trailing whitespace: in a comment with the lines `///` followed by three spaces and `///` followed by one space, the first becomes `///` and the second is left unchanged.

### FMT-DOC-007 — Later slashes on a line are content

The formatter MUST treat only the first `///` of a line, after its leading whitespace, as the exterior. Any further `///` on the same line is content and MUST be left unchanged.

Why: Content such as a URL or a code sample can contain three slashes; treating them as another exterior would rewrite the author's text.

Example:

```csharp
class C
{
    /// <summary>
    /// See http://example.com/// for details.
    /// </summary>
    int A;
}
```

is left unchanged.

### FMT-DOC-008 — A summary spans at least three lines

The formatter MUST rewrite a `<summary>` element (the name is matched case-insensitively) whose start and end tags are less than two lines apart so that the start tag, the content, and the end tag each start a `///` line of their own. A summary is never collapsed to one line.

This requirement does not apply to a comment that FMT-DOC-003 or FMT-DOC-013 leaves as written.

Why: The repository style puts summary text on its own lines, so a summary reads the same however short it is and grows by adding lines rather than by rewrapping the tags.

Example:

```csharp
class C
{
    /// <summary>Gets the value.</summary>
    int A;
}
```

```csharp
class C
{
    /// <summary>
    /// Gets the value.
    /// </summary>
    int A;
}
```

The same output results when the end tag shares a line with the content in a two-line summary:

```csharp
class C
{
    /// <summary>
    /// Gets the value.</summary>
    int A;
}
```

### FMT-DOC-009 — Content that starts on the start-tag line and continues is moved off it

When an XML element's first content starts on the line of its start tag and the element's end tag is on a later line, the formatter MUST rebuild the element: if the content has more than one line, the start tag, every content line, and the end tag each start a `///` line of their own; if the content has exactly one line and the element is not a `<summary>`, the element MUST be collapsed onto the start-tag line.

Why: Content that begins beside the start tag but ends below it is the half-wrapped shape that appears when a one-line element grows. The formatter picks the nearest canonical form: a single line when everything fits on one, otherwise the expanded block form.

This requirement does not yet decide inline formatting elements such as `<b>` or `<i>` whose content wraps across lines, and it does not apply to a comment that FMT-DOC-003 or FMT-DOC-013 leaves as written.

Example — more than one content line:

```csharp
class C
{
    /// <summary>
    /// Gets the value.
    /// </summary>
    /// <remarks>First line.
    /// Second line.</remarks>
    int A;
}
```

```csharp
class C
{
    /// <summary>
    /// Gets the value.
    /// </summary>
    /// <remarks>
    /// First line.
    /// Second line.
    /// </remarks>
    int A;
}
```

One content line:

```csharp
class C
{
    /// <summary>
    /// Gets the value.
    /// </summary>
    /// <remarks>Only line.
    /// </remarks>
    int A;
}
```

```csharp
class C
{
    /// <summary>
    /// Gets the value.
    /// </summary>
    /// <remarks>Only line.</remarks>
    int A;
}
```

### FMT-DOC-010 — Elements already in a canonical shape are left alone

The formatter MUST NOT rebuild an element that fits on one line, nor a non-summary element whose content starts on a line after its start tag, nor a summary that already spans at least three lines and whose content starts on a line after its start tag.

Why: Both the one-line and the block form are canonical; rewriting them would only churn the author's choice.

Example:

```csharp
class C
{
    /// <summary>
    /// Gets the value.
    /// </summary>
    /// <remarks>
    /// Already aligned.
    /// </remarks>
    /// <param name="x">One line.</param>
    void M(int x)
    {
    }
}
```

is left unchanged.

### FMT-DOC-011 — Rebuilding keeps the content

When the formatter rebuilds an element under FMT-DOC-008 or FMT-DOC-009, it MUST keep the content's text, its inline XML, and the indentation of every content line relative to the `///` separator; it MUST drop only leading and trailing content lines that are empty, and MUST write an empty interior content line as a bare `///`. Nested elements that need rebuilding MUST be rebuilt in the same run. A line the rebuild adds MUST start with the spaces and tabs in front of the comment's first `///` and nothing else: code, a delimited comment, or a byte order mark in front of that `///` is not repeated.

Why: The rebuild changes only where the tags sit. A byte order mark is the encoding signature of the text, not indentation, so it stays once at the start of the text (FMT-CLEAN-009). Significant indentation inside `<code>` or a continued `<param>` description, inline elements such as `<see>` and `<c>`, and paragraph breaks are part of what the author wrote (FMT-PIPE-002). Rebuilding nested elements in one run keeps the formatter idempotent (FMT-PIPE-004).

Example — nested elements and code indentation:

```csharp
class C
{
    /// <summary>
    /// Gets the value.
    /// </summary>
    /// <remarks>Use it like this:
    /// <code>if (x)
    ///     M();</code>
    /// </remarks>
    int A;
}
```

```csharp
class C
{
    /// <summary>
    /// Gets the value.
    /// </summary>
    /// <remarks>
    /// Use it like this:
    /// <code>
    /// if (x)
    ///     M();
    /// </code>
    /// </remarks>
    int A;
}
```

A comment at the start of a text that begins with a byte order mark: `U+FEFF/// <summary>S</summary>`, `class C`, `{`, `}` becomes `U+FEFF/// <summary>`, `/// S`, `/// </summary>`, `class C`, `{`, `}` — only the first line starts with the byte order mark.

Inline XML and a paragraph break:

```csharp
class C
{
    /// <summary>Uses <see cref="C"/> and <c>A</c>.
    ///
    /// Second paragraph.</summary>
    int A;
}
```

```csharp
class C
{
    /// <summary>
    /// Uses <see cref="C"/> and <c>A</c>.
    ///
    /// Second paragraph.
    /// </summary>
    int A;
}
```

### FMT-DOC-012 — Delimited documentation comments are not rewritten

The formatter MUST NOT rewrite the text of a delimited documentation comment (`/** … */`): FMT-DOC-001 to FMT-DOC-011 apply to single-line documentation comments only.

Why: Rebuilding a delimited comment would require inventing a `*` continuation style the repository has not defined. Only the `///` form has a canonical layout here. Whether a delimited comment written behind code is separated from it is FMT-BLANK's decision.

Example:

```csharp
class C
{
    /** <summary>Gets the value.</summary> */
    int A;
}
```

is left unchanged.

### FMT-DOC-013 — Documentation with an element without an end tag of its own is left as written

When an XML element of a single-line documentation comment has no end tag of its own, as SHARED-DOC-017 defines it — no end tag at all, or one with another name — at the top level or nested inside another element, the formatter MUST leave the text of every line of that comment as written: FMT-DOC-005 to FMT-DOC-011 do not apply to it, not even to its complete elements. The code that follows the comment MUST stay code. When FMT-DOC-001 moves such a comment from behind code onto a line of its own, it MUST move the comment and leave its text as written.

End-tag names are compared as the compiler reads XML: exactly, prefix and local name, so `</returns>` is no end tag of its own for `<Returns>`. The text is what follows the indentation of each line; the column of each line is FMT-INDENT's, as for an ordinary comment.

This requirement covers a documentation comment that starts its own line and one that FMT-DOC-001 moves; a comment that FMT-DOC-003 keeps behind code is left as written by FMT-DOC-003.

Why: The parser lets an element without an end tag run to the end of the comment, and an end tag with another name is not the element's end, so the comment is not well-formed and where the author meant the element to end is unknown. Laying it out would either guess that end or take the code that follows into the comment, and laying out only the rest of the comment would leave half-normalized documentation. Incomplete documentation is corrected by hand. Moving the comment changes none of its text and keeps it documenting the declaration below it.

Example:

```csharp
class C
{
    /// <summary>S
    int A;

    /// <summary>Gets the value.</summary>
    /// <remarks>Text
    /// more
    int B;

    /// <summary>S</returns>
    int D;

    /// <Returns>Nothing</returns>
    void E()
    {
    }
}
```

is left unchanged, including the one-line `<summary>` of `B`.

A comment behind code that FMT-DOC-001 moves:

```csharp
class C
{
    int A; /// <summary>S
    int B;
}
```

```csharp
class C
{
    int A;

    /// <summary>S
    int B;
}
```

# Open questions — documentation-comments.md (FMT-DOC)

## Q1 — An empty summary is expanded with a trailing space on its content line

Input:

```csharp
class C
{
    /// <summary></summary>
    int A;
}
```

Output (the middle line is `///` followed by one space; `$` marks the line end in `cat -A`: `    /// $`):

```csharp
class C
{
    /// <summary>
    /// 
    /// </summary>
    int A;
}
```

Phases: `DocumentationCommentFormatting, Cleanup`. Idempotent.

Why it looks wrong: `BuildExpandedElement` writes an empty content line as `documentationPrefix` + `""` (= `/// `) when the content has a single (empty) line, while an empty interior line of multi-line content is written as a bare `///` (FMT-DOC-011). The Cleanup phase does not strip trailing whitespace inside documentation trivia, and FMT-DOC-006 keeps a single space after an exterior, so the trailing space is permanent. Related: trailing whitespace after documentation content (`/// Gets the value.` followed by spaces) also survives the whole pipeline — is that intended, or should Cleanup or this phase own it?

Touches: FMT-DOC-006, FMT-DOC-008, FMT-DOC-011 (left the empty-summary case out of the spec).

## Q2 — An end tag sharing a line with the last content line is only fixed for short summaries

Inputs, each left unchanged (phases: `Cleanup` only):

```csharp
class C
{
    /// <summary>
    /// First line.
    /// Second line.</summary>
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
    /// Only line.</remarks>
    int A;
}
```

whereas the two-line summary

```csharp
class C
{
    /// <summary>
    /// Gets the value.</summary>
    int A;
}
```

is rebuilt to the three-line form (FMT-DOC-008).

Why it looks wrong: `NeedsLineAlignmentNormalization` only checks whether content starts on the start-tag line. Content that ends on the end-tag line is the mirror-image half-wrapped shape, but it is normalized only when the summary happens to be shorter than three lines. Should the end-tag side be symmetric?

Touches: FMT-DOC-009, FMT-DOC-010 (the spec makes no claim about these inputs).

## Q3 — Inline formatting elements that wrap are expanded into block form

Input:

```csharp
class C
{
    /// <summary>
    /// Gets the value.
    /// </summary>
    /// <remarks>Text <b>bold
    /// still bold</b>
    /// </remarks>
    int A;
}
```

Output (phases: `DocumentationCommentFormatting, Cleanup`, idempotent):

```csharp
class C
{
    /// <summary>
    /// Gets the value.
    /// </summary>
    /// <remarks>
    /// Text <b>
    /// bold
    /// still bold
    /// </b>
    /// </remarks>
    int A;
}
```

Why it looks wrong: `RequiresNormalization` applies to every `XmlElementSyntax`, so an inline `<b>`, `<i>`, `<c>`, or `<see>…</see>` whose content wraps is rebuilt like a block element. The result puts the start tag at the end of a prose line and the end tag on its own line, which reads oddly and differs from how block elements look. Should the rebuild be limited to block-level elements (`summary`, `remarks`, `param`, `returns`, `example`, `code`, `para`, …), or is this intended?

Touches: FMT-DOC-009 (the spec explicitly leaves inline elements undecided).

## Q4 — Documentation behind a statement or inside an expression is relocated although it documents nothing

Inputs:

```csharp
class C
{
    void M()
    {
        var x = 1; /// stray
        var y = 2;
    }
}
```

```csharp
class C
{
    void M()
    {
        var x = 1 + /// stray
            2;
    }
}
```

Outputs (phases: `DocumentationCommentFormatting, BlankLine, Indentation, Cleanup`, idempotent):

```csharp
class C
{
    void M()
    {
        var x = 1;

        /// stray
        var y = 2;
    }
}
```

```csharp
class C
{
    void M()
    {
        var x = 1 +

        /// stray
        2;
    }
}
```

Why it looks wrong: the code comment in `RegisterRelocation` says relocation is skipped when it "changes the author's layout without making the comment mean anything" because the compiler reports CS1587 either way. `ReihitsuFormatterHelpers.DocumentsFollowingCode` only asks whether the following token is the first token of its parent node, so a local declaration or a literal operand qualifies even though neither can carry documentation and CS1587 still fires. The second output also splits a binary expression with a blank line and a documentation comment. Should the predicate require the following node to be documentable (a type, member, enum member, or similar declaration), or is the syntactic rule intended? Note the predicate is shared with `BlankLineTriviaBoundaryRewriter`, so FMT-BLANK is affected too.

Touches: FMT-DOC-001, FMT-DOC-003 (the spec limits FMT-DOC-001 to type and member declarations).

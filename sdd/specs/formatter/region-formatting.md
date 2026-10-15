# Formatter — Region formatting

| | |
|---|---|
| ID prefix | FMT-REGION |
| Status | Active |
| Code owners | `RegionFormattingPhase`, `RegionNamingRewriter`, `RegionDirectiveUtilities` |
| Uses | FMT-PIPE-002, FMT-PIPE-005 |

This spec covers the region phase: the capitalization of `#region` descriptions and the `// Description` comment on the matching `#endregion`. The phase changes only the text of region directives; it never adds, removes, or moves one.

## Non-goals

- Indentation of region directives — FMT-INDENT.
- Blank lines around region directives — FMT-BLANK.
- Whether a region is placed sensibly, for example straddling a member body. The formatter keeps a misplaced region; removing it is an explicit analyzer code-fix action.
- Unbalanced regions. A `#region` without `#endregion` (or the reverse) is a syntax error, so the whole file is left unchanged under FMT-PIPE-005.
- Any directive other than `#region` and `#endregion`.

## Requirements

### FMT-REGION-001 — Region descriptions start with an upper-case letter

The formatter MUST change the first character of a `#region` description to its invariant upper-case form when it is a lower-case letter, and MUST leave every other character of the description as written. A description that starts with an upper-case letter or with a character that has no upper-case form (a digit, `_`) MUST keep its first character. When the formatter rewrites a `#region` line, it MUST write it as `#region`, one space, and the trimmed description.

Why: Region names are headings; a consistent capital letter makes them read as such. Only the first character is touched because the rest is the author's wording — the formatter cannot know whether `fields and properties` should become title case.

Example:

```csharp
class C
{
    #region   äpfel und birnen

    int _value;

    #endregion // Äpfel und birnen
}
```

```csharp
class C
{
    #region Äpfel und birnen

    int _value;

    #endregion // Äpfel und birnen
}
```

and a description that starts with a digit is kept:

```csharp
class C
{
    #region 1st fields

    int _value;

    #endregion // 1st fields
}
```

is left unchanged.

### FMT-REGION-002 — `#endregion` repeats the region description

The formatter MUST write every `#endregion` whose `#region` has a description as `#endregion // <description>`, using the description of the matching `#region` after FMT-REGION-001. Whatever followed the `#endregion` keyword before — nothing, a different name, a different casing, `//` without a space, a block comment, or a bare word — MUST be replaced. This replacement is the normalization FMT-PIPE-002 allows for region names.

Why: A closing directive that names its region lets a reader see which block ends without scrolling up. The text after `#endregion` is a preprocessing message, not a comment the compiler keeps; it carries no information beyond the region name, so the formatter derives it from the `#region` instead of trusting a stale copy.

Example:

```csharp
class C
{
    #region fields

    int _value;

    #endregion
}
```

```csharp
class C
{
    #region Fields

    int _value;

    #endregion // Fields
}
```

and a mismatched name is corrected:

```csharp
class C
{
    #region Fields

    int _value;

    #endregion /* Fields */
}
```

```csharp
class C
{
    #region Fields

    int _value;

    #endregion // Fields
}
```

### FMT-REGION-003 — Nested regions pair innermost first

The formatter MUST pair each `#endregion` with the nearest preceding unclosed `#region` in source order, so nested regions each receive their own description.

Why: This is how the compiler matches region directives. Pairing any other way would write the outer region's name onto the inner `#endregion`.

Example:

```csharp
class C
{
    #region outer

    #region inner

    int _value;

    #endregion

    #endregion
}
```

```csharp
class C
{
    #region Outer

    #region Inner

    int _value;

    #endregion // Inner

    #endregion // Outer
}
```

### FMT-REGION-004 — Regions without a description are left alone

The formatter MUST NOT change a `#region` without a description or its matching `#endregion`, including any text after that `#endregion`.

Why: There is no name to capitalize or to repeat. Removing the text after the `#endregion` would delete the only label the author wrote for the block.

Example:

```csharp
class C
{
    #region

    int _value;

    #endregion // Stale
}
```

is left unchanged.

### FMT-REGION-005 — Region directives are never added, removed, or moved

The formatter MUST keep every `#region` and `#endregion` directive at its position in the token stream, including a region whose directives sit in different element bodies, and MUST still name such a pair under FMT-REGION-001 and FMT-REGION-002.

Why: Removing or relocating a region changes the structure the author chose and, for a region that straddles a member body, can orphan one of its directives. Correcting a misplaced region is a decision for the user, offered by the analyzer's code fix, not a layout normalization.

Example:

```csharp
class C
{
    #region members

    void M()
    {
        var x = 1;

        #endregion
    }
}
```

```csharp
class C
{
    #region Members

    void M()
    {
        var x = 1;

        #endregion // Members
    }
}
```

### FMT-REGION-006 — Regions in disabled code are named too

The formatter MUST apply FMT-REGION-001 and FMT-REGION-002 to region directives inside a conditional-compilation branch the current build does not compile, and MUST NOT change the disabled text around them.

Why: The analyzer reports region names in inactive branches as well. If the formatter skipped them, it would leave a diagnostic behind that it could fix with the same text-only rewrite it applies to active code.

Example:

```csharp
class C
{
#if false
    #region test

    void M()
    {
    }

    #endregion
#endif
}
```

```csharp
class C
{
#if false
    #region Test

    void M()
    {
    }

    #endregion // Test
#endif
}
```

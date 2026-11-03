# Formatter — Region formatting

| | |
|---|---|
| ID prefix | FMT-REGION |
| Status | Active |
| Code owners | `RegionFormattingPhase`, `RegionNamingRewriter`, `RegionDirectiveUtilities` |
| Uses | FMT-PIPE-002, FMT-PIPE-005, FMT-TAB-002 |

This spec covers the region phase: the capitalization of `#region` descriptions, the form of a `#region` line with a description, the `// Description` comment on the matching `#endregion`, and the space in front of the text of an `#endregion` whose `#region` has no description. The phase changes only the text of region directives; it never adds, removes, or moves one. Its requirements apply when a whole document or syntax tree is formatted.

## Non-goals

- Indentation of region directives — FMT-INDENT.
- Blank lines around region directives — FMT-BLANK.
- Whether a region is placed sensibly, for example straddling a member body. The formatter keeps a misplaced region; removing it is an explicit analyzer code-fix action.
- Unbalanced regions. A `#region` without `#endregion` (or the reverse) is a syntax error, so the whole file is left unchanged under FMT-PIPE-005.
- Any directive other than `#region` and `#endregion`.
- Region directives in the code a node-level entry point formats, which keep their text — FMT-PIPE-028.

## Requirements

### FMT-REGION-001 — Region descriptions start with an upper-case letter

The formatter MUST change the first character of a `#region` description to its invariant upper-case form when it is a lower-case letter, and MUST leave every other character of the description as written. A description that starts with an upper-case letter or with a character that has no upper-case form (a digit, `_`) MUST keep its first character. The line that holds the description is written as FMT-REGION-007 states.

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

### FMT-REGION-004 — Regions without a description keep their text

The formatter MUST NOT change a `#region` without a description, and MUST NOT change the text after its matching `#endregion`. When that `#endregion` has text after it, the formatter MUST write exactly one space between the `endregion` keyword and the first character of that text, whatever whitespace stood there before, also when none stood there, and MUST remove whitespace behind that text. A bare `#endregion` MUST stay bare.

Why: There is no name to capitalize or to repeat. Removing or rewriting the text after the `#endregion` would change the only label the author wrote for the block. The whitespace in front of that label and behind it is layout, not part of it: the label gets the one space a `#region` line gets in front of its description, and the line ends at the label as a `#region` line ends at its description (FMT-REGION-007).

Example:

```csharp
class C
{
    #region

    int _value;

    #endregion // Stale
}
```

is left unchanged, and the whitespace around the label is normalized — `···` stands for three spaces at the end of the line:

```csharp
class C
{
    #region

    int _value;

    #endregion    // Stale···
}
```

```csharp
class C
{
    #region

    int _value;

    #endregion // Stale
}
```

`#endregion//Stale` becomes `#endregion //Stale`: the text after the keyword is kept as written.

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

### FMT-REGION-007 — A `#region` line is `#region`, one space, and the description

The formatter MUST write every `#region` that has a description as `#region`, exactly one space, and the trimmed description: whatever whitespace stood between the `region` keyword and the description — several spaces, or the four spaces a tab became under FMT-TAB-002 — becomes one space, and whitespace behind the description is removed. This holds whether or not FMT-REGION-001 changes the description's first character, and also for a `#region` inside a conditional-compilation branch the current build does not compile. It applies to the `#region` directives the phase pairs with an `#endregion` (FMT-REGION-003); a file with a region directive that has no partner is left unchanged as a whole (FMT-PIPE-005). A `#region` without a description is left as FMT-REGION-004 states.

Why: The separator and the whitespace behind the description are layout, not part of the name: the description that FMT-REGION-001 and FMT-REGION-002 work with is trimmed. Writing the line form only when the name is rewritten made the output depend on whether the name happened to start with a lower-case letter — `#region    foo` became `#region Foo`, while `#region    Bar` kept its four spaces. One form for every `#region` line with a description matches the one form FMT-REGION-002 gives every `#endregion` behind it, and the line reads the same whether or not its branch is compiled.

Example — `···` stands for three spaces at the end of the line:

```csharp
class C
{
    #region    Bar
    int a;
    #endregion    Bar

    #region Baz···
    int b;
    #endregion // Baz
}
```

```csharp
class C
{
    #region Bar

    int a;

    #endregion // Bar

    #region Baz

    int b;

    #endregion // Baz
}
```

and a `#region` line that already has this form is left unchanged:

```csharp
class C
{
    #region Bar

    int a;

    #endregion // Bar
}
```

A region in a branch the build does not compile is written the same way:

```csharp
class C
{
#if false
    #region    Bar
    int a;
    #endregion // Bar
#endif
}
```

```csharp
class C
{
#if false
    #region Bar
    int a;
    #endregion // Bar
#endif
}
```

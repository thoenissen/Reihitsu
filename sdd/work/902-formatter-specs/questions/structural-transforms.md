# Open questions — structural-transforms.md (FMT-STRUCT)

## Q1 — Comment after an expression-body arrow is glued to the opening brace

Input:

```csharp
class C
{
    int Get() => // why
        42;
}
```

Output (`StructuralTransform, LineBreak, HorizontalSpacing, Indentation, Cleanup`, idempotent):

```csharp
class C
{
    int Get()
    {// why
        return 42;
    }
}
```

Why it looks wrong: `ExpressionBodyToBlockConverter.CreateBlock` documents the relocation as "mirroring a hand-written `{ // comment` block opener", but the output has no space between `{` and the comment. No later phase adds one (a hand-written `{// why` is also left as it is). The comment survives, so FMT-PIPE-002 holds; only the spacing is in doubt. Touches FMT-STRUCT-006 (stated without an example of this case) and possibly FMT-SPACE.

## Q2 — The line after a directive in a guarded construct is dedented to the member column

Input:

```csharp
class C
{
    int Get() =>
#pragma warning disable CS0000
        42;
#pragma warning restore CS0000
}
```

Output (`Indentation, Cleanup`, idempotent):

```csharp
class C
{
    int Get() =>
#pragma warning disable CS0000
    42;
#pragma warning restore CS0000
}
```

The structural guard (FMT-STRUCT-007) correctly keeps the expression body, but the indentation phase moves the continuation `42;` back to the member's column. The same happens for an expression-bodied property (`int P =>` + `#pragma` + `42;`, no structural phase involved), for `int Get() =>` + `#if DEBUG 1; #else 2; #endif` (the active `2;` is dedented), and for a field kept intact by FMT-STRUCT-023:

```csharp
class C
{
    private int _x,
#if DEBUG
                _debug,
#endif
                _y;
}
```

```csharp
class C
{
    private int _x,
#if DEBUG
                _debug,
#endif
    _y;
}
```

Why it looks wrong: a continuation line should be indented as a continuation, not at the member's own column. This belongs to FMT-INDENT, but it is why FMT-STRUCT-007 shows no refusal example and FMT-STRUCT-023 describes its example in prose.

## Q3 — A comment between the type and the first declarator is duplicated onto every split field

Input:

```csharp
class C
{
    private int /* c */ _first, _second;
}
```

Output (`StructuralTransform, Cleanup`, idempotent):

```csharp
class C
{
    private int /* c */ _first;
    private int /* c */ _second;
}
```

The regression test `CommentOnTheTypeLineIsStillDuplicatedOntoEveryField` pins this deliberately. Duplicating a comment does not delete it, but FMT-PIPE-002 speaks of preserving the comment's text, not of copying it. Is the duplication intended behavior to specify under FMT-STRUCT-021/022, or a known limitation? FMT-STRUCT-022 currently does not mention this slot.

## Q4 — A region between a method signature and its arrow is spread over blank lines

Input:

```csharp
class C
{
    int Other()
#region Body
        => 42;
#endregion
}
```

Output (`RegionFormatting, BlankLine, Indentation, Cleanup`, idempotent):

```csharp
class C
{
    int Other()

    #region Body

    => 42;

    #endregion // Body
}
```

The structural guard (FMT-STRUCT-007) keeps the expression body as required. The blank lines around a region that sits inside a member declaration, and the `=>` at the member's column, come from the region, blank-line, and indentation phases. It looks wrong for a region inside a declaration, but the input is unusual (RH7303 reports regions within elements). Touches FMT-REGION / FMT-BLANK / FMT-INDENT, not FMT-STRUCT.

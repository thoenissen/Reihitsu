# Open questions — region-formatting.md

## Q1 — Extra spaces after `#region` are only normalized when the description is capitalized

Input:

```csharp
class C
{
    #region   Fields

    int _value;

    #endregion // Fields
}
```

Output (unchanged apart from the final newline):

```csharp
class C
{
    #region   Fields

    int _value;

    #endregion // Fields
}
```

Phases: `Cleanup`.

With a lower-case description (`#region   fields`) the same line becomes `#region Fields` (phases `RegionFormatting, Cleanup`), so the single space is a side effect of the capitalization rewrite rather than a rule of its own. Trailing whitespace after an already capitalized description is likewise kept (`TrailingOpeningWhitespaceWithCanonicalCloseIsStable`). Is the spacing inside a `#region` line meant to be normalized independently of capitalization? FMT-REGION-001 only specifies the line written when the formatter rewrites it.

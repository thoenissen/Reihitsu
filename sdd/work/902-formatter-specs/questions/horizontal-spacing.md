# Open questions — horizontal-spacing.md (FMT-SPACE)

## Q1 — `fixed(` gets no space, although RH6001 requires one

Input:

```csharp
class C
{
    unsafe void M(int[] arr)
    {
        fixed(int* p = arr)
        {
        }
    }
}
```

Output (`Cleanup`, idempotent): unchanged apart from the final newline.

Why it looks wrong: `documentation/rules/RH6001.md` lists `fixed` among the keywords that need a space before their parenthesis, but `KeywordSpacingRule.IsSpacedKeyword` does not contain `FixedKeyword`, so the formatter emits code RH6001 reports. Touches FMT-SPACE-012, whose keyword list currently follows the code and leaves `fixed` out.

## Q2 — Token pairs without a rule keep the author's spacing

Input:

```csharp
using System;
using System.Collections.Generic;

class C
{
    void M(int a, bool b, int[] arr)
    {
        Func<int, int> f = x=>x;
        var r = b?1:2;
        var e = arr [0];
        var t = typeof (C);
        var l = new List<int> ();
        var s = global :: System.String.Empty;
        M (a, b:b, arr :arr);
    }
}
```

Output (`BlankLine, Cleanup`, idempotent): only a blank line is inserted before the last statement; every gap above is kept as written.

Why it might be wrong: the lambda arrow, the conditional operator `?:`, the space before an element-access `[` or an invocation `(` (including after `typeof` and after a closing generic `>`), the `::` alias qualifier, and named-argument colons are covered by no spacing rule, so the phase only collapses their whitespace (FMT-SPACE-002). Several of them (`x=>x`, `b?1:2`) look unformatted. No analyzer rule reports most of them either (RH6021 explicitly exempts conditional and named-argument colons), so this may be intended scope. The spec lists them as a non-goal; confirm, or decide which ones should get a rule.

## Q3 — Initializer braces are spaced on one side only

Input:

```csharp
class C
{
    void M()
    {
        var n = new int[] {1, 2};
    }
}
```

Output (`HorizontalSpacing, Cleanup`, idempotent):

```csharp
class C
{
    void M()
    {
        var n = new int[] {1, 2 };
    }
}
```

Why it looks wrong: the single-space rule only looks at the token in front of a brace, so `}` gets a space before it while `{` gets none after it. This matches RH6009/RH6010, which also check only the side in front of a brace, but the result `{1, 2 }` is asymmetric. FMT-SPACE-009 specifies only the "before a brace" part and its example avoids this shape.

## Q4 — A double space in front of a delimited documentation comment is not collapsed

Input:

```csharp
class C
{
    int _a  /** x */;
}
```

Output (`Cleanup`, idempotent): unchanged apart from the final newline.

Why it might be wrong: when the right token carries a comment in its leading trivia, the rewriter returns before any rule or collapse runs (the exemption FMT-SPACE-014 describes), so the two spaces stay. Every other gap the phase sees with a run of whitespace is collapsed to one space (FMT-SPACE-002, FMT-SPACE-013). FMT-SPACE-014 only states that the whitespace is not moved, and its example uses a single space.

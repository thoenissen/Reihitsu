# Open questions — line-breaks.md (FMT-BREAK) and fluent-chain-layout.md (SHARED-CHAIN)

## Q1 — A constructor-initializer colon at the end of the line leaves `this(…)` at the declaration's column

Input:

```csharp
class C
{
    C(string s) :
        this(1)
    {
    }
}
```

Output:

```csharp
class C
{
    C(string s)
        :
    this(1)
    {
    }
}
```

Phases: `LineBreak, Indentation, Cleanup` (idempotent).

Why it looks wrong: the phase moves the `:` onto its own line (FMT-BREAK-010) but leaves the line break behind it, so the initializer call is split from its colon and ends up at the member's indentation. The expected shape is `: this(1)` on one line below the parameter list, as for `C() : this(1)`. FMT-BREAK-010 is written only for an initializer that shares the parameter list's line.

Touches: FMT-BREAK-010.

## Q2 — A wrap inside a generic constraint clause is kept and the continuation drops to column 0

Input:

```csharp
class D<T> where T :
    class
{
    int _a;
}
```

Output:

```csharp
class D<T>
    where T :
class
{
    int _a;
}
```

Phases: `LineBreak, Indentation, Cleanup` (idempotent).

Why it looks wrong: the `where` clause is moved onto its own line (FMT-BREAK-023), but the wrap after `:` survives and the indentation phase places `class` at column 0. Either the wrap inside a constraint clause should be joined (as FMT-BREAK-015 does inside list elements) or the continuation should at least be indented. FMT-BREAK-023 does not cover this case.

Touches: FMT-BREAK-023 (and possibly FMT-INDENT).

## Q3 — An empty region directly after an opening brace gains a blank line after the brace (routed from blank-lines Q1)

Agreed: this is a FMT-BREAK issue. Re-checked with the harness and `scripts/trace.sh --passes 2`.

Input:

```csharp
class C
{
    #region A
    #endregion // A
}
```

Output:

```csharp
class C
{

    #region A

    #endregion // A
}
```

Phases: `BlankLine, LineBreak, Cleanup`. The trace shows `BlankLinePhase` adding the blank line between `#region A` and `#endregion` (SHARED-BLANK-006) and then, in every pass, `LineBreakPhase` inserting a blank line after `{` that `BlankLinePhase` removes again in the next pass (FMT-BLANK-007). The harness reports the result as idempotent only because both phases repeat the same pair of edits.

More detail:

- The same happens inside a method body, when the input already has a blank line between the two directives, and when a comment precedes the region (`{`, `// c`, `#region A`, `#endregion // A`, `}` — the blank line appears above `// c`). It does not happen for `#if X` / `#endif` directly inside braces, nor when a member sits inside the region.
- In all failing cases the whole content between the braces is leading trivia of the closing brace. The likely path is the closing-brace normalization (`BracePlacer.EnsureBraceOnOwnLine` → `TokenGapNormalizer.NormalizeGapBeforeToken` with zero blank lines): the gap counts the blank line inside the region pair as excess, rewrites the closing brace's leading trivia, and because a directive or comment owns its own line (`HasOwnLinePlacementOwner`) keeps the opening brace's trailing line break while the rebuilt leading trivia starts with another one. This is a hypothesis from reading the code, not confirmed in a debugger.

Expected: no blank line between `{` and `#region A` (SHARED-BLANK-005 exemption, FMT-BLANK-007). Not written into FMT-BREAK as intended behavior.

Touches: FMT-BREAK-002, SHARED-BLANK-005, FMT-BLANK-007.

## Q4 — Switch expression arms are not put on separate lines

Input:

```csharp
class C
{
    int M(int a)
    {
        return a switch { 1 => 2, _ => 3 };
    }
}
```

Output:

```csharp
class C
{
    int M(int a)
    {
        return a switch
               {
                   1 => 2, _ => 3
               };
    }
}
```

Phases: `LineBreak, Indentation, Cleanup` (idempotent).

Why it looks odd: the braces get their own lines (FMT-BREAK-008), but both arms stay on one line inside them. Every comparable construct the phase opens up — object initializers, wrapped collection expressions, property patterns — gets one element per line. FMT-BREAK-008 states only the brace placement and lists the arms' layout as a non-goal; is one arm per line intended?

Touches: FMT-BREAK-008.

## Q5 — A continuation line after a trailing comment loses its continuation indentation (FMT-INDENT)

Input:

```csharp
class C
{
    void M()
    {
        var u = a && // c
                b;
        var w = a ? // c
                b : c;
    }
}
```

Output:

```csharp
class C
{
    void M()
    {
        var u = a && // c
        b;
        var w = a ? // c
        b
                    : c;
    }
}
```

Phases: `LineBreak, Indentation, Cleanup` (idempotent).

Why it looks wrong: the line-break phase correctly refuses to move the operator across the comment (FMT-BREAK-001), but the line it keeps is then indented to the statement's column instead of a continuation column. The same happens for `var x = // c` followed by the value, for `int P => // c` followed by the expression, and for the `#else` branch of `var y =` / `#if X` / `1;` / `#else` / `2;` / `#endif`. This belongs to FMT-INDENT; FMT-BREAK only specifies that these line breaks are kept and uses no such example.

Touches: FMT-BREAK-001, FMT-INDENT.

## Q6 — Argument lists keep leading commas and a closing parenthesis on its own line

Input:

```csharp
class C
{
    void M()
    {
        Foo(a
            , b);
        Foo(a, b
            );
    }
}
```

is left unchanged by the line-break phase, while the same shapes in a parameter list are corrected (FMT-BREAK-011, RH5106, RH5107).

Why it is a question: RH5106 and RH5107 cover parameter lists only, so this may be intended, but the asymmetry is surprising — the phase does put arguments one per line (FMT-BREAK-012) without moving the comma. Recorded as a non-goal in FMT-BREAK until decided.

Touches: FMT-BREAK-011, FMT-BREAK-012.

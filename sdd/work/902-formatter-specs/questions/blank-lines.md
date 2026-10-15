# Open questions — blank lines (FMT-BLANK, SHARED-BLANK)

## Q1 — An empty region directly after an opening brace gains a blank line after the brace

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

Phases: `BlankLine, LineBreak, Cleanup`. The harness reports the result idempotent, but `scripts/trace.sh --passes 2` shows the blank line after `{` oscillating: the blank-line phase removes it in every pass (FMT-BLANK-007) and `LineBreakPhase` inserts it again. With a member inside the region (`#region Fields` / `private int _x;` / `#endregion // Fields`) no blank line appears after the brace (SHARED-BLANK-005 example).

Why it looks wrong: SHARED-BLANK-005 exempts a region directive directly below an opening brace, and FMT-BLANK-007 / RH5022 forbid a blank line after an opening brace, so the expected output has no blank line between `{` and `#region A`. The inserted line comes from `LineBreakPhase`, apparently when `#region` and `#endregion` both sit in the leading trivia of the closing brace. Likely a FMT-BREAK bug rather than a FMT-BLANK one.

Touches: SHARED-BLANK-005, FMT-BLANK-007 (both written as the expected behavior; the empty-region case is not used as an example).

## Q2 — Blank line before a comment that leads a same-group using directive

Same case as `using-directives.md` Q1, seen from this phase: the blank-line phase applies the comment boundary (FMT-BLANK-011) to a comment between two using directives of one group, which inserts the blank line SHARED-USING-008 forbids.

Input:

```csharp
using System;
// I/O helpers
using System.IO;

class C
{
    int x;
}
```

Output:

```csharp
using System;

// I/O helpers
using System.IO;

class C
{
    int x;
}
```

Phases: `BlankLine, Cleanup`.

Why it looks wrong: RH5020 (comments preceded by a blank line) and RH7207 (no blank line within a using group) contradict each other here, and the formatter follows RH5020. One of the two must give way. FMT-BLANK-011 explicitly leaves this case undecided until the maintainer chooses.

Touches: FMT-BLANK-011, SHARED-USING-008.

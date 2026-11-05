# Code fixes are withheld where they would separate comments and directives from their code or change the program

| | |
|---|---|
| Status | Accepted |
| Date | 2026-10 |
| Affected requirements | SHARED-FIX-003, SHARED-MODORDER-003, RH7004-004, RH7101-004, RH7109-005, RH7109-006, RH7306-003, RH7309-004, RH7501-004, RH7101-006, RH7303-005, RH7306-004 and RH7309-006 (added by this change), FMT-PIPE-031 (reasoning only) |

## What

SHARED-FIX-003 lists what a code fix must never do. It gains three conditions, and the organization fixes that violated them are withheld for the inputs concerned. The diagnostic is still reported in every case.

- **Comments and directives stay with their code.** A fix must not move a comment away from the code it stands with, and must not reorder two comments that stand with the same code. A comment behind code at the end of its line stands with that code. A comment that starts its line stands with the code or directive line below it, when no blank line lies between them. A fix also must not change which code a directive governs. A conditional or region directive governs the code between it and its partner. Any other directive governs the code from its line to the directive that ends its effect. A directive pair counts as split across a brace when the fix moves one directive across a brace while its partner stays, or adds a pair of braces with one brace between the two directives and the other outside them. A pair of braces added wholly between the two directives, or wholly outside them, does not split the pair.
- **Disabled text is content.** The text of code that an `#if` branch excludes must not change, and removing a comment, a directive, or disabled text counts as changing it. A fix whose diagnostic reports a whole region on its `#region`, as RH7310 does, still removes that region's `#endregion` with it.
- **Every build configuration counts.** A compile error or a run-time change that only a build with or without a conditional compilation symbol would show withholds the fix too.

The run-time condition is clarified as well: the instance fields of a layout type include the compiler's backing fields of auto-properties, of properties that use `field`, and of instance field-like events.

The rule specs state what this means for each fix:

| Rule | Withheld or changed | Requirement |
|---|---|---|
| RH7004 | A comment behind the using declaration stays behind the header of the `using` statement instead of moving behind the new block. A directive pair that the new block would split is a SHARED-FIX-003 case. | RH7004-004 |
| RH7101 | Withheld for a declaration with a `///` or `/** */` documentation comment in its leading trivia in documentation mode `Parse` or `Diagnose`, although the formatter splits it. In mode `None`, and for such a comment in front of a later declarator, the fix still splits. | RH7101-004, RH7101-006 |
| RH7105, RH7106 | Withheld when a comment or a directive stands between the last modifier and the token after it. | SHARED-MODORDER-003 |
| RH7109 | Withheld when the move reorders the instance fields of a struct without `LayoutKind.Auto` or `LayoutKind.Explicit`, or of a type with `[StructLayout(LayoutKind.Sequential)]`. | RH7109-005, RH7109-006 |
| RH7303 | Withheld for a reported directive whose partner lies between declarations. | RH7303-005 |
| RH7306 | The partner directive is judged by its description, as the analyzer judges it. Withheld when the description of the reported directive, or of the partner it would edit, is the word `implementation` alone. | RH7306-003, RH7306-004 |
| RH7309 | Withheld when the reorder would swap two runtime initializers of one context, swap two instance fields of a layout type, move a block away from a comment above its `#region` line (also across blank lines), or hand a documentation comment to another declaration in mode `Parse` or `Diagnose`. A comment above a block that keeps its place does not withhold it. | RH7309-004, RH7309-006 |
| RH7501 | Withheld when the `break` shares its line with other code, when the block's closing brace shares its line with the `break`, or when a directive or excluded code lies between the token in front of the `break` and the closing brace. A `break` that is the block's only statement is still moved, and the block it leaves empty stays. | RH7501-004 |

## Why

`AGENTS.md` (*Code-fix coverage*) already calls a fix unsafe when it drops, reorders, or relocates comments, directives, disabled text, or literals. It points to SHARED-FIX-003 for what that means, but SHARED-FIX-003 covered only changed text, not placement, and said nothing about disabled text. Without that wording, several organization fixes produced code that did not compile, compiled differently in another build configuration, changed initializer order or a struct's memory layout, took documentation from a declaration, or left a comment or a `#pragma` acting on other code. Their rule specs often still required the rewrite. A diagnostic without a fix is a supported end state; a fix that hands the user such a defect is not.

## Visible output changes

The "before" outputs were measured with `scripts/apply-fix.sh <ID>` on the code of `main` at `a59a003`, in the documentation mode `Parse`, under LF and CRLF alike, except where a case names another source. The "after" outputs follow from the requirements in the table above. In every case the diagnostic is still reported, before and after.

Unchanged: a `break` that is the only statement of its block is moved behind the closing brace, and the block stays (RH7501-004), as before; a declaration whose `///` or `/** */` comments stand in front of later declarators is split as before (RH7101-006); in the documentation mode `None`, the RH7101 and RH7309 fixes are offered as before.

### 1. RH7004 — a comment behind the using declaration

```csharp
// input
using System.IO;

class C
{
    void M()
    {
        using var s = new MemoryStream(); // resource
        s.Flush();
    }
}
```

```csharp
// before
using System.IO;

class C
{
    void M()
    {
        using (var s = new MemoryStream())
        {
            s.Flush();
        } // resource
    }
}
```

```csharp
// after
using System.IO;

class C
{
    void M()
    {
        using (var s = new MemoryStream()) // resource
        {
            s.Flush();
        }
    }
}
```

### 2. RH7004 — a directive pair the new block would split

```csharp
// input, with DEBUG defined
using System.IO;

class C
{
    void M()
    {
        using var s = new MemoryStream();
#if DEBUG
        s.Flush();
#endif
    }
}
```

```csharp
// before
using System.IO;

class C
{
    void M()
    {
        using (var s = new MemoryStream())
        {
#if DEBUG
            s.Flush();
        }
#endif
    }
}
```

After: no code fix is offered. A build without `DEBUG` would lose the closing brace of the block. Without `DEBUG` defined, the fix is withheld as well (SHARED-FIX-003).

### 3. RH7101 — a documented declaration

```csharp
// input
class C
{
    /// <summary>The coordinates</summary>
    public int X, Y;
}
```

```csharp
// before
class C
{
    /// <summary>The coordinates</summary>
    public int X;
    public int Y;
}
```

After, in the documentation modes `Parse` and `Diagnose`: no code fix is offered. The same holds for a `/** */` comment in front of the declaration, and for a documentation comment that starts behind the code of another member on the declaration's line:

```csharp
// input
class C
{
    private int _p; /** X. */ private int _x, _y;
}
```

```csharp
// before: _y loses its documentation
class C
{
    private int _p;
    /** X. */ private int _x;
    private int _y;
}
```

After: no code fix is offered. This input keeps the output that `rh7101-fix-through-context-entry-point.md` (case 7) gives it only in the documentation mode `None`, where the comment is an ordinary comment.

### 4. RH7105 and RH7106 — a directive or a comment behind the last modifier

```csharp
// input
class C
{
    static public
#pragma warning disable CS0169
    int _a;
}
```

```csharp
// before: does not compile
class C
{
    public static #pragma warning disable CS0169
    int _a;
}
```

```csharp
// input
class C
{
    static public
    // the counter
    int _a;
}
```

```csharp
// before
class C
{
    public static

    // the counter
    int _a;
}
```

After, for both inputs: no code fix is offered.

### 5. RH7109 — the instance fields of a struct

```csharp
// input
public struct S
{
    public int _a;
    public readonly int _b;
}
```

```csharp
// before
public struct S
{
    public readonly int _b;
    public int _a;
}
```

After: no code fix is offered. The same holds for a `record struct`, a `ref struct`, and a class declared with `[StructLayout(LayoutKind.Sequential)]`.

### 6. RH7303 — a region that reaches between declarations

```csharp
// input
class C
{
    #region Members

    void M()
    {
        #endregion
    }
}
```

```csharp
// before
class C
{

    void M()
    {
    }
}
```

After: no code fix is offered. The same holds for the mirror shape, a `#region` in a body whose `#endregion` lies between declarations.

### 7. RH7306 — nothing would remain of a description

```csharp
// input
class C
{
    #region Implementation

    int _a;

    #endregion // Implementation
}
```

```csharp
// before
class C
{
    #region

    int _a;

    #endregion //
}
```

```csharp
// input
class C
{
    #region IDisposable implementation

    int _a;

    #endregion // implementation
}
```

```csharp
// before
class C
{
    #region IDisposable

    int _a;

    #endregion //
}
```

After, for both inputs: no code fix is offered for either directive. For `#endregion //Implementation` behind `#region IDisposable`, the fix was offered and changed nothing (observed by the skeptic of this change on `735d0db`); after, it is not offered.

### 8. RH7309 — a reorder that would change the program or separate a comment

```csharp
// input
using System;

public class C : IComparable
{
    #region IComparable

    private static readonly int Y = X + 1;

    public int CompareTo(object obj) => Y;

    #endregion // IComparable

    #region Fields

    private static readonly int X = 5;

    #endregion // Fields
}
```

```csharp
// before: Y is 6 instead of 1
using System;

public class C : IComparable
{
    #region Fields

    private static readonly int X = 5;

    #endregion // Fields

    #region IComparable

    private static readonly int Y = X + 1;

    public int CompareTo(object obj) => Y;

    #endregion // IComparable
}
```

```csharp
// input
using System;

public class C : IDisposable
{
    // The disposal logic
    #region IDisposable

    public void Dispose()
    {
    }

    #endregion // IDisposable

    #region Fields

    private int _a;

    #endregion // Fields
}
```

```csharp
// before: the comment heads the Fields block
using System;

public class C : IDisposable
{
    // The disposal logic
    #region Fields

    private int _a;

    #endregion // Fields

    #region IDisposable

    public void Dispose()
    {
    }

    #endregion // IDisposable
}
```

```csharp
// input
using System;

public class C : IDisposable
{
    #region IDisposable

    public void Dispose()
    {
    }

    #endregion // IDisposable

    /// <summary>M</summary>
    #region Methods

    public void M()
    {
    }

    #endregion // Methods
}
```

```csharp
// before: the documentation comment documents Dispose
using System;

public class C : IDisposable
{
    #region Methods

    public void M()
    {
    }

    #endregion // Methods

    /// <summary>M</summary>
    #region IDisposable

    public void Dispose()
    {
    }

    #endregion // IDisposable
}
```

After, for all three inputs: no code fix is offered. The same holds for two instance fields of a struct in two blocks, an auto-property counting as a field, a comment separated from the `#region` line by blank lines, and the other two documentation shapes of RH7309-006. A comment above a block that keeps its place keeps the fix (RH7309-004).

### 9. RH7501 — a `break` that cannot move without rearranging its line or a directive

```csharp
// input
class C
{
    void M(int x)
    {
        switch (x)
        {
            case 1:
                {
                    A(); /* first */ break;
                }
        }
    }

    void A()
    {
    }
}
```

```csharp
// before
class C
{
    void M(int x)
    {
        switch (x)
        {
            case 1:
                {
                    A(); /* first */                 }
                break;
        }
    }

    void A()
    {
    }
}
```

```csharp
// input
class C
{
    void M(int x)
    {
        switch (x)
        {
            case 1:
                {
                    A();
#pragma warning disable CS0162
                    break;
#pragma warning restore CS0162
                }
        }
    }

    void A()
    {
    }
}
```

```csharp
// before: the warning stays disabled to the end of the file
class C
{
    void M(int x)
    {
        switch (x)
        {
            case 1:
                {
                    A();
#pragma warning restore CS0162
                }
                #pragma warning disable CS0162
                    break;
        }
    }

    void A()
    {
    }
}
```

After, for both inputs: no code fix is offered. The same holds for a block whose closing brace shares the `break`'s line, for a `#region` or an `#if` branch between the token in front of the `break` and the closing brace, and for code of another build configuration behind the `break`.

## Decisions

### One shared condition, with each rule's instances in its own spec

The placement, disabled-text, and configuration rules are stated once in SHARED-FIX-003. A rule spec names its own instances as "withheld" requirements with examples, and an "unless withheld" in the requirement that states the rewrite, so no rule spec reads as requiring a rewrite that SHARED-FIX-003 forbids. The layout guard of RH7109 is not stated again in `shared/ordering-move-safety.md`. RH7309-006 takes its runtime-initializer definition from SHARED-ORDER-007.

### The brace split follows the maintainer's clarification

A pair counts as split when one directive moves across a brace or one new brace lands between the two directives. Braces added wholly inside a governed span are not a split. Counting them would withhold every brace or expression-body fix inside a `#region`, although the code each directive governs stays the same.

### RH7101 withholds its fix where the formatter splits

The RH7101 fix asks the formatter whether it splits a declaration (FMT-PIPE-031) and is not offered where it does not. RH7101-006 withholds it also for a documented declaration that the formatter splits: formatting a file may keep the documentation on the first field, but a fix that the user applies to one diagnostic must not take documentation from a declaration (SHARED-FIX-003). The condition is the fix's own and states no rule of the split, so the reasoning of FMT-PIPE-031 says "not offered where formatting does not split" instead of "exactly where".

### Comment placement is decided only for the shapes it names

The condition decides a comment at the end of a line of code and a comment that directly heads the next line. A comment that a blank line separates from the code below it, and a comment between two tokens of one line, are left to each rule. RH7309 decides its own case: a comment above a `#region` heads that block, also across blank lines.

### The maintainer's answers

The maintainer approved the visible output changes of this change, listed above, after `main` was merged into the run on 2026-11-05. The narrower questions answered before:

- Backing fields count as fields of a layout type.
- The RH7303 mirror shape (`#region` in a body, `#endregion` between declarations) is withheld like the reported shape.
- A comment above an RH7309 block that keeps its place does not withhold the fix.
- RH7306 is withheld when the partner directive would be left with an empty description.
- A comment behind the last modifier withholds the RH7105 and RH7106 fixes like a directive.
- RH7501 keeps the empty block that a lone `break` leaves.
- RH7101: a documentation comment that starts behind the code of another member on the declaration's line, in the declaration's leading trivia, documents the declaration, so in the documentation modes `Parse` and `Diagnose` the fix is withheld for it as for any documented declaration (RH7101-006). This answer (2026-11-05) supersedes, for those modes, the split shown for this input in case 7 of `rh7101-fix-through-context-entry-point.md`.

### Out of this change

- The fate of a blank line in front of the `break` of RH7501 is decided by a separate change.
- RH7309 regions that the region helper counts although they belong to a nested enum or lie inside an expression are left to a follow-up.
- Fixes outside these rules are not audited against the new conditions here. A fix that violates them is a bug against SHARED-FIX-003.

## Alternatives considered

- **Keep the fixes and move the comment or directive somewhere plausible** (the RH7309 comment above the moved block, the RH7101 documentation copied to every field, the RH7501 `break` laid out on a new line) — rejected: where a comment or documentation belongs is the author's decision, and "prefer no fix" applies.
- **Treat every brace added between two directives as a split** — rejected: it withholds common, safe fixes inside regions and `#pragma` spans.
- **Withhold RH7309 whenever any block has a comment above it** — rejected: it gives away safe ground, because nothing is relocated when the block keeps its place.
- **Count only declared fields for the layout guard** — rejected: a backing field's offset changes exactly like a declared field's.
- **Offer RH7303 for the mirror shape and RH7306 with an empty `#endregion //`** — rejected: one drops an unreported directive, the other leaves a nameless directive.
- **Fix the RH7309 region selection in the shared helper in this change** — rejected for now: it is a change to a helper with many consumers and a gap in SHARED-TYPEREGION-005, both outside the fixes this change is about.

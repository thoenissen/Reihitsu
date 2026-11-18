# Get-only properties and indexers are reported for an expression body

| | |
|---|---|
| Status | Accepted |
| Date | 2026-11 |
| Affected requirements | new specs: RH3209 (RH3209-001), RH3210 (RH3210-001), shared/get-only-expression-bodies.md (SHARED-GETONLY-001 to SHARED-GETONLY-008) |

## What

Two new Clarity rules report a get-only member whose accessor list the formatter replaces with an expression body (FMT-STRUCT-014, FMT-STRUCT-015):

- **RH3209 — Get-only properties should use an expression body** (Clarity, Warning, enabled, Fix All) reports a property.
- **RH3210 — Get-only indexers should use an expression body** (Clarity, Warning, enabled, Fix All) reports an indexer.

Both report the members the formatter converts: an accessor list whose only accessor is a `get` without attributes and modifiers, expression-bodied or with a block of one `return` with a value or one `throw` with an operand that the accessor conversion accepts, on a member without an initializer, from C# 6 on, and from C# 7 on for a getter that throws. The diagnostic is on the accessor list. The fix replaces the accessor list with the arrow clause and semicolon the formatter writes and keeps the rest of the member as written. A member that the formatter keeps for a comment, a directive, or disabled text, or refuses as unsupported, is reported, and its fix is withheld. The decision whether a member is converted, and the conversion, move into one type in `Reihitsu.Core`, which the formatter, both analyzers, and both fixes use. The behavior both rules share is one shared spec; each rule's spec names its kind of member.

## Why

The formatter converts a get-only property or indexer to an expression-bodied member (FMT-STRUCT-014), and no rule reports the accessor list it replaces: a user who runs only the analyzers never learns that the accessor list is not the house style, and a user who formats sees a rewrite no diagnostic announced. The construct — a get-only accessor list — is one no existing rule names, so it gets rules of its own ([P11](../../README.md#the-formatter-may-do-more-than-the-analyzers-p11), a new-rule candidate). Properties and indexers get separate rules so that each kind can be configured on its own, as the expression-bodied member rules RH3202 to RH3208 split their kinds (thoenissen/Reihitsu#849, thoenissen/Reihitsu#850).

## Visible output changes

"Before" is today's `main`, where neither rule exists. Each diagnostic named under "after" is new.

**1. RH3209 — a get-only property is reported, and the fix writes the expression-bodied property.**

```csharp
// input
using System;

class C
{
    private int _value;

    public int A { get => _value; }

    public int B
    {
        get
        {
            return _value;
        }
    }

    public int D { get { throw new InvalidOperationException(); } }

    public ref int E { get { return ref _value; } }
}
```

```csharp
// before: not reported
```

```csharp
// after: RH3209 on the accessor list of A, B, D, and E, from its `{` through its `}`; the fix gives
using System;

class C
{
    private int _value;

    public int A => _value;

    public int B => _value;

    public int D => throw new InvalidOperationException();

    public ref int E => ref _value;
}
```

**2. RH3210 — a get-only indexer is reported, and the fix writes the expression-bodied indexer.**

```csharp
// input
class C
{
    private readonly int[] _values = new int[3];

    public int this[int index] { get => _values[index]; }

    public int this[long index]
    {
        get
        {
            return _values[index];
        }
    }
}
```

```csharp
// before: not reported
```

```csharp
// after: RH3210 on the accessor list of both indexers; the fix gives
class C
{
    private readonly int[] _values = new int[3];

    public int this[int index] => _values[index];

    public int this[long index] => _values[index];
}
```

**3. Members whose shape the formatter keeps are not reported** (RH3209 shown; RH3210 alike).

```csharp
// input, compiled with C# 14
using System;

class C
{
    private int _value;

    public int A
    {
        get => _value;
        set => _value = value;
    }

    public int B { get; }

    public int D
    {
        [Obsolete]
        get => _value;
    }

    public int E
    {
        get
        {
            var value = _value;

            return value;
        }
    }

    public int F { get => field; } = 5;
}
```

```csharp
// before: not reported
```

```csharp
// after: not reported
```

Under C# 6, a getter that throws is not reported; under C# 5, no member is.

**4. A comment the formatter keeps the accessor list for — the member is reported without a fix.**

```csharp
// input
class C
{
    private int _value;

    public int A // note
    {
        get => _value;
    }

    public int B
    {
        // the value
        get => _value;
    }

    public int D
    {
        get => _value; // a
    } // b

    public int E
    {
        get
        {
            // cached
            return _value;
        }
    }
}
```

```csharp
// before: not reported
```

```csharp
// after: RH3209 on the accessor list of A, B, D, and E; no code fix is offered
```

A comment behind only the getter's semicolon or only the closing brace does not withhold the fix, and the fix carries it behind the new semicolon:

```csharp
// input
class C
{
    private int _value;

    public int A { get => _value; } // a
}
```

```csharp
// after: RH3209 on the accessor list; the fix gives
class C
{
    private int _value;

    public int A => _value; // a
}
```

**5. A getter the formatter refuses as unsupported is reported without a fix** — a comment, a directive, or disabled text between `get` and `=>`, or between `=>` and the expression.

```csharp
// input
class C
{
    private int _value;

    public int A { get /* c */ => _value; }

    public int B
    {
        get
#if DEBUG
            => 1;
#else
            => 2;
#endif
    }
}
```

```csharp
// before: not reported; the formatter refuses the file as unsupported
```

```csharp
// after: RH3209 on the accessor list of A and B; no code fix is offered
```

**6. A directive the formatter keeps the accessor list or the getter block for — the member is reported without a fix.** Compiled without `WRITABLE`; compiled with it, `A` has a setter and is not reported.

```csharp
// input
class C
{
    private int _value;

    public int A
    {
        get => _value;
#if WRITABLE
        set => _value = value;
#endif
    }

    public int B
    {
#if DEBUG
        get => 1;
#else
        get => 2;
#endif
    }
}
```

```csharp
// before: not reported
```

```csharp
// after: RH3209 on the accessor list of A and B; no code fix is offered
```

A conditional group inside the getter's expression keeps neither the report nor the fix: the fix converts the member as the formatter does and keeps the group in place.

**7. The fix keeps the signature as written.** It replaces only the accessor list; the spacing in front of it stays.

```csharp
// input
class C
{
    private int _value;

    public  int  A { get { return _value; } }
}
```

```csharp
// before: not reported
```

```csharp
// after: RH3209 on the accessor list; the fix gives
class C
{
    private int _value;

    public  int  A => _value;
}
```

Formatting the document instead writes `public int A => _value;`.

**8. Rule pages.** `documentation/rules/RH3209.md` and `documentation/rules/RH3210.md` are new.

## Decisions

### The rules report the get-only shape, and the fix converts what the formatter converts

The analyzer reports the members whose accessor list FMT-STRUCT-014 replaces, and also those the formatter keeps only for a comment (SHARED-GETONLY-003) or for a directive or disabled text (SHARED-GETONLY-007), or refuses as unsupported (SHARED-GETONLY-006): the comment, the directive, or the refused shape does not make the accessor list the better form, and the author can move it above the member, after which the formatter and the fix convert it. For these members the fix is withheld, because the formatter keeps or refuses them and the fix writes what the formatter writes (SHARED-FIX-003, SHARED-FIX-010); a reported diagnostic without a fix is a supported end state. A refused shape is not one the formatter keeps (SHARED-DIAG-010 treats it alike). The maintainer chose this for comments and for directives over leaving those members unreported.

### A directive the formatter keeps the member for is reported without a fix

A rule analyzes the code the build compiles, and in that build the member is get-only, so the expression-bodied form can be written there (SHARED-DIAG-011); how the other builds are written is the author's to decide, for example by writing the whole member in each branch of the conditional group. A member whose `set` accessor stands in the active branch is not get-only in that build and is not reported (SHARED-GETONLY-002). The formatter keeps the accessor list or the getter block for the directive (FMT-STRUCT-013, FMT-STRUCT-015), because a rewrite would drop or move code of another build, so the fix is withheld (SHARED-FIX-003, SHARED-GETONLY-007). A conditional group wholly inside the getter's expression is converted by the formatter, so its member is reported with a fix. The expression-bodied member rules leave a member with a directive in the span their fix rebuilds unreported (SHARED-BODY-001), because no fix could convert it; these rules are about the accessor list itself, which the author can rewrite by hand. The maintainer chose to report these members over leaving them unreported.

### The fix asks the formatter about the getter as written

The conversion deletes `get` and both gaps around the getter's `=>`, so the member the fix builds no longer holds a comment or directive that stood there, and SHARED-FIX-010's question about the built member alone would offer a fix that drops it. The fix therefore also asks `ReihitsuFormatter.FindUnsupportedSyntax` about the getter as written, and withholds itself when the getter holds a refused shape (SHARED-FIX-003). The question is about the getter, not the whole member: a refused shape elsewhere in the member is also in the member the fix builds.

### The diagnostic is on the accessor list

The accessor list is what the fix replaces, as the expression-bodied member rules put their diagnostic on the expression body their fix replaces (SHARED-BODY-002).

### The fix keeps the signature as written

The fix replaces only the accessor list, and the whitespace between the signature and the list goes with it, as the block-body fixes replace only the expression body (SHARED-BODY-003, SHARED-FIX-006). The gap in front of the new `=>` is the one the formatter writes for the converted member, not one the fix writes on its own, so the fix does not predict the formatter's spacing.

### One owner for the conversion

The decision whether a get-only member is converted, and the conversion itself, move from the formatter's transforms into one type in `Reihitsu.Core`, `GetOnlyMemberExpressionBodyRewriteUtilities`, which the formatter's get-only transform, both analyzers, and both fixes call. The analyzer references only `Reihitsu.Core`, and a code fix may not call a member of a formatter transform (FMT-PIPE-009), so without one owner the conditions of FMT-STRUCT-009 to FMT-STRUCT-015 would be written three times and drift apart. `ExpressionBodyRewriteUtilities` (SHARED-BODY) and `EmptyTypeDeclarationSemicolonAnalysisUtilities` (SHARED-EMPTY) are the precedents. The formatter's output does not change.

### The shape is not declared unsupported

A comment or directive inside a get-only accessor list is not declared unsupported: the formatter has a clear output for it, it keeps the accessor list, and a cheap precheck could not tell a convertible getter from one with several statements, so it would refuse common code.

### One shared spec

The two rules differ only in the kind of member, so their behavior is specified once under `shared/`, as SHARED-BODY does for RH3202 to RH3208 and SHARED-EMPTY for RH5412 to RH5418.

## Alternatives considered

- **One rule for properties and indexers** — rejected: separate rules allow separate configuration, as for the other member kinds.
- **Do not report a member the formatter keeps for a comment** — rejected by the maintainer: the rule would then hide a get-only accessor list whenever a comment sits in it, although the author can move the comment and convert the member.
- **Do not report a member the formatter keeps for a directive** — rejected by the maintainer: the member is get-only in the build that is analyzed, and the author can rewrite the accessor list by hand; the expression-bodied member rules' precedent (SHARED-BODY-001) rests on a fix that cannot exist, not on the shape.
- **Declare a comment or directive inside a get-only accessor list unsupported** — rejected by the maintainer: the formatter has a clear output for such code.
- **Restate the formatter's conditions in the analyzer and the fix** — rejected: one rule in three places drifts; one `Reihitsu.Core` owner instead.
- **The fix writes the space in front of `=>` itself** — rejected: it would predict the formatter's spacing; the gap comes from the formatter.
- **Put the diagnostic on the member's name or on `get`** — rejected: the accessor list is the code the fix replaces.
- **No analyzer, formatter only** — rejected: an analyzer-only user never learns the form the formatter writes.

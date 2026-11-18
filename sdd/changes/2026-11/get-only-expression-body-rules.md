# Get-only properties and indexers are reported for an expression body

| | |
|---|---|
| Status | Draft |
| Date | 2026-11 |
| Affected requirements | new specs: RH3209 (RH3209-001), RH3210 (RH3210-001), shared/get-only-expression-bodies.md (SHARED-GETONLY-001 to SHARED-GETONLY-006) |

## What

Two new Clarity rules report a get-only member whose accessor list the formatter replaces with an expression body (FMT-STRUCT-014, FMT-STRUCT-015):

- **RH3209 — Get-only properties should use an expression body** (Clarity, Warning, enabled, Fix All) reports a property.
- **RH3210 — Get-only indexers should use an expression body** (Clarity, Warning, enabled, Fix All) reports an indexer.

Both report exactly the members the formatter converts: an accessor list whose only accessor is a `get` without attributes and modifiers, expression-bodied or with a block of one `return` with a value or one `throw` with an operand that the accessor conversion accepts, on a member without an initializer, from C# 6 on, and from C# 7 on for a getter that throws. The diagnostic is on the accessor list. The fix replaces the accessor list with the arrow clause and semicolon the formatter writes and keeps the rest of the member as written. A member the formatter refuses as unsupported is reported, and its fix is withheld. The behavior both rules share is one shared spec; each rule's spec names its kind of member.

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

**4. A comment the formatter keeps the accessor list for — the member is not reported.**

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
}
```

```csharp
// before: not reported
```

```csharp
// after: not reported
```

A comment behind only the getter's semicolon or only the closing brace does not keep the member from being reported, and the fix carries it behind the new semicolon:

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

**5. A getter the formatter refuses as unsupported is reported without a fix.**

```csharp
// input
class C
{
    private int _value;

    public int A { get /* c */ => _value; }
}
```

```csharp
// before: not reported
```

```csharp
// after: RH3209 on the accessor list; no code fix is offered
```

**6. Rule pages.** `documentation/rules/RH3209.md` and `documentation/rules/RH3210.md` are new.

## Decisions

### The rules report what the formatter converts

The analyzer reports exactly the members whose accessor list FMT-STRUCT-014 replaces and FMT-STRUCT-015 does not keep, also for the comment and directive guards (SHARED-GETONLY-003): formatting a document then clears every diagnostic of the two rules, and the fix has a result wherever a member is reported. A member the formatter refuses as unsupported is not one it keeps, so it is reported, as SHARED-DIAG-010 and SHARED-BODY-001 treat refused shapes, and its fix is withheld (SHARED-FIX-010).

### The diagnostic is on the accessor list

The accessor list is what the fix replaces, as the expression-bodied member rules put their diagnostic on the expression body their fix replaces (SHARED-BODY-002).

### The fix keeps the signature as written

The fix replaces only the accessor list, and the whitespace between the signature and the list goes with it, as the block-body fixes replace only the expression body (SHARED-BODY-003, SHARED-FIX-006).

### One shared spec

The two rules differ only in the kind of member, so their behavior is specified once under `shared/`, as SHARED-BODY does for RH3202 to RH3208 and SHARED-EMPTY for RH5412 to RH5418.

## Alternatives considered

- **One rule for properties and indexers** — rejected: separate rules allow separate configuration, as for the other member kinds.
- **Report a member the formatter keeps for a comment, without a fix** — as SHARED-EMPTY-001 reports an empty type with a comment. Rejected in this draft: the diagnostic would stay after formatting the document, and no fix could clear it. It is the alternative named in visible output change 4.
- **Put the diagnostic on the member's name or on `get`** — rejected: the accessor list is the code the fix replaces.
- **No analyzer, formatter only** — rejected: an analyzer-only user never learns the form the formatter writes.

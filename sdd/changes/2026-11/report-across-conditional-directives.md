# Rules judge the code the current build compiles

| | |
|---|---|
| Status | Draft |
| Date | 2026-11 |
| Affected requirements | SHARED-DIAG-008, SHARED-DIAG-010, SHARED-ATTR-019, SHARED-ATTR-020, SHARED-EMPTY-001, SHARED-MODORDER-003, SHARED-TRAIL-002, RH2001-007, RH2005-001, RH3002-010, RH5102-004, RH5109-004, RH5111-005, RH7003-001, RH7101-001, RH7102-001, RH7102-006, RH7103-001, RH7105-001, RH7106-001, RH7107-001, RH7107-005, RH7108-001, RH7108-005, RH7311-001, RH8201-003 |

## What

SHARED-DIAG-008 used to keep a construct unreported when a conditional directive (`#if`, `#elif`, `#else`, `#endif`), or code such a directive excludes, lay inside the span the rule judges. Its reporting half is reversed: a rule judges a construct as the analyzed build compiles it, with the conditional compilation symbols that build defines. Excluded code counts as absent, and a conditional directive in the judged span no longer keeps the construct from being reported.

The fix half stays. A code fix that rewrites or moves the part of the judged span that holds a directive or excluded code is not offered, and a fix that leaves them in place is offered only where it keeps every build intact (SHARED-FIX-003, SHARED-FIX-009). The diagnostic is reported without a fix wherever a fix would have to change code that another build compiles differently.

The rules whose own requirements carried the silence, citing SHARED-DIAG-008 or with a guard of their own for the same reason, report the construct now:

- modifier order — RH7105, RH7106;
- member order — RH7102 (a constant's modifier list), RH7103 (a static member's modifier list);
- accessor order — RH7107, RH7108;
- line breaks — RH5102 (argument lists), RH5109 (parameter lists), RH5111 (the gaps around `=`);
- RH7101 — the declarators of a field declaration;
- attribute list shape — the rules of SHARED-ATTR-019 (RH5502, RH5504, RH5506, RH5508, RH5510, RH5512, RH5514, RH5516, RH5518, RH5520, RH5522, RH5526, RH5528, RH5531) and of SHARED-ATTR-020 (RH5524, RH5529);
- trailing commas — RH5409, RH5410, RH5411 (SHARED-TRAIL-002);
- RH7311 — a region in a branch the build compiles is compared with the other regions of the type;
- RH8201 — an implicit interface implementation in a file that holds `#if`; the fix stays withheld there.

Several rules already behaved this way, and only their specs change: RH3002 (RH3002-010 no longer reads as a departure), RH7003 (RH7003-001 said a conditional directive in the namespace prevents the report, which the analyzer and its test never did), RH2005 and RH2001 (they report across a conditional directive among the modifiers or in the accessor list and gain the SHARED-DIAG-008 reference), and the empty-type rules RH5412, RH5413, RH5414, RH5415, RH5416, and RH5418 (SHARED-EMPTY-001 now states that directives and excluded code in the body do not keep the type from being reported; the fix stays withheld under SHARED-EMPTY-004).

SHARED-DIAG-010 names RH5111 as the rule that reports a gap around `=` the formatter keeps; the exception now covers a directive of any kind there, not only a comment, `#region`, `#endregion`, or `#pragma`.

## Why

The maintainer decided on 2026-11-15, under the RH3002 question, that analyzers judge the code as the preprocessor symbols of the current build are set, because that is the common analyzer case. The old reading of SHARED-DIAG-008 did the opposite for every rule but RH3002: a construct was checked in no build at all as soon as another build could compile another version of its judged span. A class whose only member lies in `#if DEBUG` was the specification's own example of a construct no rule reports.

A code fix is not an analyzer verdict. It writes code that every build compiles, so it still has to keep every build intact, and it never works out what another build compiles (SHARED-FIX-009). Where the rule's verdict is right for the analyzed build but the fix could break another one, the diagnostic stays and the author corrects the code by hand.

## Visible output changes

Every input below is analyzed in a build that defines none of the symbols it tests. "Before" is today's output on `main`.

### Modifier order — RH7105, RH7106

```csharp
class C
{
    static
#if FEATURE
#endif
    public int Value { get; set; }

    internal
#if FEATURE
#endif
    protected int A;
}
```

Before: not reported.

After: reported, and no code fix is offered.

```csharp
class C
{
    {|RH7105:static|}
#if FEATURE
#endif
    public int Value { get; set; }

    internal
#if FEATURE
#endif
    {|RH7106:protected|} int A;
}
```

### Member order — RH7102, RH7103

```csharp
public struct S
{
    private int _x;
    private
#if DEBUG
        readonly
#else
        const
#endif
        int K = 1;
}

public struct T
{
    private int _a;
    private
#if !DEBUG
        static
#endif
        int _b;
}
```

Before: not reported.

After: reported, and no code fix is offered.

```csharp
public struct S
{
    private int _x;
    private
#if DEBUG
        readonly
#else
        const
#endif
        int {|RH7102:K|} = 1;
}

public struct T
{
    private int _a;
    private
#if !DEBUG
        static
#endif
        int {|RH7103:_b|};
}
```

### Accessor order — RH7107, RH7108

```csharp
using System;

class C
{
    private int _a;
    private Action _handlers;

    public int A
    {
#if !NEVER
        set { _a = value; }
#endif
        get { return _a; }
    }

    public event Action E
    {
#if !NEVER
        remove { _handlers -= value; }
#endif
        add { _handlers += value; }
    }
}
```

Before: not reported.

After: reported, and no code fix is offered, because moving the accessor would take the `#endif` in front of it away from its `#if` (SHARED-ORDER-003). Where the ordering requirements allow the move, such as for a balanced `#if` … `#endif` pair in the moved accessor's own leading trivia, the fix is offered as for any other accessor list.

```csharp
using System;

class C
{
    private int _a;
    private Action _handlers;

    public int A
    {
#if !NEVER
        set { _a = value; }
#endif
        {|RH7107:get|} { return _a; }
    }

    public event Action E
    {
#if !NEVER
        remove { _handlers -= value; }
#endif
        {|RH7108:add|} { _handlers += value; }
    }
}
```

### Argument and parameter lists — RH5102, RH5109

```csharp
class C
{
    void N()
    {
        M(1, 2,
#if DEBUG
#endif
          3);
    }

    void P(int a, int b,
#if DEBUG
#endif
           int c)
    {
    }

    int M(params int[] values) => 0;
}
```

Before: not reported.

After: reported, and no code fix is offered — as for a `#pragma` in the same gap today.

```csharp
class C
{
    void N()
    {
        M{|RH5102:(1, 2,
#if DEBUG
#endif
          3)|};
    }

    void P{|RH5109:(|}int a, int b,
#if DEBUG
#endif
           int c)
    {
    }

    int M(params int[] values) => 0;
}
```

### Assignment line breaks — RH5111

```csharp
class C
{
    void M()
    {
        var value
#if DEBUG
#endif
            = 1;
    }
}
```

Before: not reported.

After: reported, and no code fix is offered — as for a `#pragma` in the same gap today.

```csharp
class C
{
    void M()
    {
        var {|RH5111:value
#if DEBUG
#endif
            = 1|};
    }
}
```

### Combined fields — RH7101

```csharp
class C
{
    private int _a,
#if DEBUG
                _b,
#endif
                _c;
}
```

Before: not reported.

After: reported on the second declarator the build compiles, and no code fix is offered.

```csharp
class C
{
    private int _a,
#if DEBUG
                _b,
#endif
                {|RH7101:_c|};
}
```

### Attribute list shape — SHARED-ATTR-019 and SHARED-ATTR-020 rules

```csharp
[A,
#if DEBUG
 B,
#endif
 C]
class D
{
}

class E
{
    void M([A]
#if DEBUG
           [B]
#endif
           [C] int a)
    {
    }
}
```

Before: not reported.

After: reported, and no code fix is offered — as for a `#pragma` in the same place today.

```csharp
{|RH5506:[A,
#if DEBUG
 B,
#endif
 C]|}
class D
{
}

class E
{
    void M([A]
#if DEBUG
           [B]
#endif
           {|RH5524:[C]|} int a)
    {
    }
}
```

### Trailing commas — RH5409, RH5410, RH5411

```csharp
enum E
{
    A,
#if DEBUG
    B,
#endif
}
```

Before: not reported.

After: reported, and no code fix is offered: removing the comma would break a build that defines `DEBUG`. The author can write the comma inside the branch instead, in front of `B`.

```csharp
enum E
{
    A{|RH5409:,|}
#if DEBUG
    B,
#endif
}
```

The rule pages RH5409, RH5410, and RH5411 lose their *Exceptions* section, which stated the old silence.

### Repeated region descriptions — RH7311

```csharp
class C
{
#if DEBUG
    #region Fields

    int _a;

    #endregion
#else
    #region Fields

    int _b;

    #endregion
#endif

    #region Fields

    int _c;

    #endregion
}
```

Before: not reported — a region inside a branch was compared with no other region.

After: the region of the branch the build compiles is compared with the regions outside it; the regions of the two branches are still never compared with each other.

```csharp
class C
{
#if DEBUG
    #region Fields

    int _a;

    #endregion
#else
    #region Fields

    int _b;

    #endregion
#endif

    {|RH7311:#region Fields|}

    int _c;

    #endregion
}
```

### Inherited documentation — RH8201

```csharp
using System;

/// <summary>
/// Service
/// </summary>
public class C : IDisposable
#if FEATURE
    , ICloneable
#endif
{
    /// <summary>Disposes</summary>
    public void Dispose()
    {
    }
}
```

Before: not reported.

After: reported, and no code fix is offered.

```csharp
using System;

/// <summary>
/// Service
/// </summary>
public class C : IDisposable
#if FEATURE
    , ICloneable
#endif
{
    ///{|RH8201: <summary>Disposes</summary>
|}    public void Dispose()
    {
    }
}
```

The rule page RH8201 loses its *Exceptions* section, which stated the old silence.

### General notes

`documentation/rules/general-notes.md`, section *Code excluded by `#if`*, gains a paragraph: a rule judges the code as the current build compiles it, also when an `#if` block sits inside the code it checks, and a code fix is not offered where it would have to change code another build compiles differently.

### Unchanged output

RH3002, RH7003, RH2005, RH2001, and the empty-type rules RH5412–RH5416 and RH5418 already report across a conditional directive without a fix; their output does not change. The formatter does not change.

## Decisions

### The fix half keeps every build intact, by the part it rewrites

A fix that rewrites or moves the part of the judged span that holds a directive or excluded code is withheld, as before for `#region`, `#endregion`, and `#pragma`. A fix that leaves them in place stays offered only where it keeps every build intact; removing the comma in front of a conditional directive does not, because another build's element can need it, so SHARED-TRAIL-002 withholds it. Stating the boundary by the part the fix rewrites keeps RH2005's fix, which replaces `public` and leaves an excluded modifier in front of the first modifier where it is, and lets the ordering requirements decide RH7107's and RH7108's moves as they decide every other move.

### Rules that stay silent for a reason of their own

A rule's own requirement may still leave a shape that holds a directive unreported when the reason holds in every build. These are checked and kept:

- SHARED-JOIN-001 (RH5101, RH5105, RH5106, RH5107, RH5108, RH5113), RH5307-002, and RH5408-003 do not report a gap or a list that a comment or any directive keeps from being joined; the formatter keeps the same gap (SHARED-DIAG-010).
- RH7310 does not report a region that holds a directive or excluded code; such a region always holds a directive line, which is content in every build.
- SHARED-USING-009 keeps RH7201–RH7207 silent on a using block that holds any preprocessor directive; its reason is that neither the fix nor the formatter may reorder the block.
- RH7309 already judges the regions the analyzed build compiles.

### One requirement per shared statement

The empty-type rules share SHARED-EMPTY-001, so the clarification is stated there once, not in six rule specs. SHARED-TRAIL-002, SHARED-ATTR-019, and SHARED-ATTR-020 change in place, so every rule that uses them follows without a change to its own spec.

## Alternatives considered

- **Withhold every fix whenever a conditional directive lies in the judged span** — rejected: it would withdraw RH2005's fix for an excluded modifier in front of the first modifier, which is offered today and keeps every build intact, and it would add a guard the ordering requirements make unnecessary for RH7107 and RH7108.
- **Keep the silence for trailing commas, where the comma usually exists for another build's element** — rejected: it is the same reason the decision overruled for every other rule; the diagnostic without a fix tells the author that the analyzed build has a trailing comma.
- **Reverse only the rules that cite SHARED-DIAG-008** — rejected: RH7311 and RH8201 stay silent for the same reason without citing it, and leaving them would keep two readings of one rule.

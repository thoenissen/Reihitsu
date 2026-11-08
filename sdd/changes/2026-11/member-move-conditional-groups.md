# Member-moving fixes read only conditional groups of whole members or whole statements

| | |
|---|---|
| Status | Draft |
| Date | 2026-11 |
| Affected requirements | SHARED-FIX-009 (added), SHARED-ORDER-008 (added), SHARED-ORDER-006 (changed), SHARED-ORDER-007 (changed), RH7102-006 (changed), RH7103-006 (changed), RH7109-006 (changed), RH7309-005 (changed) |

## What

- **A shared principle for every code fix** (SHARED-FIX-009). A fix does not work out what another build configuration compiles: it evaluates no `#if`/`#elif` condition, combines no branches of several groups, and assembles no declaration, statement, or expression from parts that several branches write. Where only such a reading could show that a fix keeps every build intact, the fix is withheld and the diagnostic stays. A fix may still read excluded code whole — whole members, whole statements, names — counting it as present in every build, so that the reading can only withhold.
- **The member reordering fixes apply it** (SHARED-ORDER-008). The fixes of RH7102, RH7103, RH7109, and RH7309 keep a move only when every conditional directive in the moved and the crossed span belongs to a *group of whole members* (directives between members, branches that hold whole member declarations, no nesting) or to a *group of whole statements* (directives inside the body block of a method, constructor, destructor, operator, conversion operator, or accessor, branches that hold whole statements, no nesting, no `field` keyword in an accessor's branch). Every other conditional directive in the spans withholds the fix: one between two tokens of a member declaration outside such a body (modifiers, initializer, expression body, accessor list, …), a nested group, and a group whose branch begins or ends a member, a type body, or a block.
- **SHARED-ORDER-007 returns to main's reading** for the groups that stay: each member of an excluded branch counts as present at its position, in every build; the move is refused when a moved and a crossed member of one context both have a runtime initializer, or when it reorders the instance fields of a layout type. What this run keeps from its earlier stages: the guard applies to every member reordering fix, RH7102 included (thoenissen/Reihitsu#1595).

## Why

The maintainer's answer to the run's third batch (2026-11-08): the fix is withheld whenever the moved or crossed span holds a conditional directive that is not a group of whole members; groups of whole members keep main's reading. The maintainer does not want fixes to parse or infer excluded configurations: `#if` is a special case that needs no code fix, and this is recorded as a shared principle.

The run had first tried to judge each build configuration on its own (per-branch reading, then per-assignment parsing of the preprocessor symbols with a configuration limit). Four audits each found another input where the reading missed a build or withheld a fix main offers. Withholding the fix is exact enough: the diagnostic stays, and a diagnostic without a fix is a supported end state (`AGENTS.md`, *Code-fix coverage*).

Withholdings that SHARED-FIX-003 already demands — main offers a fix that changes another build, such as a branch that closes the type's body around the moved member, or a member split into a static initialized field in some build — are bug fixes against an unchanged requirement and are not listed below.

## Visible output changes

Code-fix output on `main` measured with `scripts/apply-fix.sh` at `57acf0d`, LF and CRLF alike. Each case is a fix that main offers, which changes no build, and which is now withheld. The diagnostic stays in every case.

A member whose initializer an `#if` completes, RH7109 (SHARED-ORDER-008, SHARED-FIX-009):

```csharp
public class C
{
    private int _a;
    private int _b
#if DEBUG
        = 1
#endif
        ;
    private readonly int _c;
}
```

Before (`main`):

```csharp
public class C
{
    private readonly int _c;
    private int _a;
    private int _b
#if DEBUG
        = 1
#endif
    ;
}
```

After: no fix offered.

A member whose modifiers an `#if` splits, RH7109:

```csharp
public class C
{
    private int _a;
#if DEBUG
    internal static readonly
#else
    private const
#endif
        int K = 1;
    private readonly int _b = Log("b");

    private static int Log(string name)
    {
        return 0;
    }
}
```

Before (`main`):

```csharp
public class C
{
    private readonly int _b = Log("b");
    private int _a;
#if DEBUG
    internal static readonly
#else
    private const
#endif
    int K = 1;

    private static int Log(string name)
    {
        return 0;
    }
}
```

After: no fix offered.

A member split by `#if` inside a region block, RH7309 — the input of thoenissen/Reihitsu#1593:

```csharp
using System;

public class C : IDisposable
{
    #region IDisposable

    public void Dispose()
    {
    }

    #endregion // IDisposable

    #region Fields

#if DEBUG
    private static readonly int X = Log("x");
    private static readonly int Y =
#else
    private static readonly int Y =
#endif
        Log("y");

    #endregion // Fields

    private static int Log(string name) => 0;
}
```

Before (`main`): the fix moves the `Fields` block in front of the `IDisposable` block.

After: no fix offered.

A nested conditional group, RH7109:

```csharp
public class C
{
    private int _a;
#if FEATURE
#if DEBUG
    private int _d;
#endif
#endif
    private readonly int _b;
}
```

Before (`main`):

```csharp
public class C
{
#if FEATURE
#if DEBUG
    private int _d;
#endif
#endif
    private readonly int _b;
    private int _a;
}
```

After: no fix offered.

An excluded branch that does not hold whole members, RH7109:

```csharp
public class C
{
    private int _a;
#if false
    Notes: _b used to be computed here.
#endif
    private readonly int _b;
}
```

Before (`main`):

```csharp
public class C
{
#if false
    Notes: _b used to be computed here.
#endif
    private readonly int _b;
    private int _a;
}
```

After: no fix offered.

An accessor body whose `#if` branch uses the `field` keyword, RH7103:

```csharp
public struct S
{
    private int _x;

    public int Value
    {
        get
        {
#if TRACE
            return field;
#else
            return 0;
#endif
        }
    }

    public static int Shared => 1;
}
```

Before (`main`): the fix moves `Shared` in front of `Value`.

After: no fix offered.

Unchanged against `main`: a group of whole members (the SHARED-ORDER-007 examples, and `#if DEBUG` around a whole method in front of the moved member), and a group of whole statements in a method or accessor body that does not use `field` (the first SHARED-ORDER-006 example, the second RH7309-005 example) keep main's fix and output.

## Decisions

### Batch 3, answer A — withhold for every conditional directive that is not a group of whole members

The maintainer's answer (2026-11-08), quoted from the run's record: the fix is withheld whenever the moved or crossed span holds a conditional directive that is not a group of whole members — an `#if` inside a member, splitting a member's modifiers, nested `#if`, or crossing a member or type boundary; groups of whole members keep main's reading (initializer and layout check). Fixes do not parse or infer excluded configurations; `#if` is a special case that needs no code fix; the principle is recorded in the shared specs. This decides every case listed under *Visible output changes* except the accessor `field` case, which follows from the next decision.

### A balanced group inside a member's body keeps the fix (pending the lead's confirmation)

The answer's words "an `#if` inside a member" would also withhold the fix for every member whose body holds an `#if DEBUG` block — the first example of SHARED-ORDER-006 and the second of RH7309-005, which main specifies and fixes. The spec author reads the answer as aimed at directives that change what a member is (modifiers, initializer, braces) and keeps a balanced group of whole statements inside a body, which changes no initializer, context, or layout field, except in an accessor whose branch uses `field`. This was put to the lead as an output question with that recommendation; under the maintainer's standing answer the recommended option applies unless the lead asks the maintainer.

### The parts of the run's earlier reading that this replaces

The per-configuration reading of SHARED-ORDER-007 (each build judged on its own, assignments of the preprocessor symbols, the 256-configuration limit, the membership condition for branches that close or open a type) is removed. Its outcomes for the inputs it was written for are kept or replaced by withholding: every input whose conditional directives are not groups of whole members or whole statements now withholds the fix.

## Alternatives considered

- **Judge each build configuration on its own** — rejected by the maintainer's answer: it requires evaluating conditions and combining branches, and four audits found inputs it read wrong.
- **Withhold for every conditional directive inside a member, the body included** — the literal reading of the answer; see the second decision.
- **Main's reading unchanged** — rejected: main offers fixes that change another build when a member is split by `#if` (SHARED-FIX-003), which thoenissen/Reihitsu#1593 reports.

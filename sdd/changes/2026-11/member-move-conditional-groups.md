# Member-moving fixes keep only conditional groups that wrap whole units

| | |
|---|---|
| Status | Accepted |
| Date | 2026-11 |
| Affected requirements | SHARED-FIX-009 (added), SHARED-ORDER-008 (added), SHARED-ORDER-006 (changed), SHARED-ORDER-007 (changed), RH7102-006 (changed), RH7103-007 (changed), RH7109-006 (changed), RH7309-005 (changed) |

## What

- **A shared principle for every code fix** (SHARED-FIX-009). A fix does not work out what another build configuration compiles: it evaluates no `#if`/`#elif` condition, combines no branches of several groups, and assembles no declaration, statement, or expression from parts that several branches write. Where only such a reading could show that a fix keeps every build intact, the fix is withheld and the diagnostic stays. A fix that reads excluded code reads it whole — whole members, statements, attribute lists, accessors, names — counting it as present in every build, so that the reading can only withhold, and it decides which directives withhold it from where they stand in the syntax tree, never from their conditions. A fix that reads no excluded code and leaves every directive in place is governed by SHARED-FIX-003 and SHARED-DIAG-008 alone.
- **The member reordering fixes apply it** (SHARED-ORDER-008). The fixes of RH7102, RH7103, RH7109, and RH7309 keep a move only when every conditional directive in the moved and the crossed span belongs to a group that wraps whole units, with no nesting: whole members of a type body (the reordered type's or a nested type's in the spans), whole attribute lists of one member, whole accessors of one accessor list, or whole statements of one block or switch section in a member's body. No branch of an accessor group, or of a statement group in an accessor's body, may contain `field`, and no branch of an accessor group may hold an accessor without a body while an accessor of the list has one. A conditional directive in the reordered type's declaration in front of its body — among its attribute lists or in its header — withholds every move of the type, unless it belongs to a group in front of the declaration whose excluded branches hold whole members, such as a whole type. Every other conditional directive in the spans withholds the fix. The guard decides this by a small structural test — which token's leading trivia holds each directive, whether another group lies between them, and one parse of each excluded branch, the parse main already uses to find excluded members — and refuses whatever that test does not classify.
- **SHARED-ORDER-007 returns to main's reading** for the groups that stay: each member of an excluded branch of a group of whole members counts as present at its position, in every build; the move is refused when a moved and a crossed member of one context both have a runtime initializer, or when it reorders the instance fields of a layout type. A property declares a backing field when any accessor of its list has neither a body nor an expression body, as `get => 0; set;` does, not only when all of them lack one; the rule is stated once. What this run keeps from its earlier stages: the guard applies to every member reordering fix, RH7102 included (thoenissen/Reihitsu#1595).

## Why

The maintainer's answer to the run's third batch (2026-11-08): the fix is withheld whenever the moved or crossed span holds a conditional directive that is not a group of whole members; groups of whole members keep main's reading. The maintainer does not want fixes to parse or infer excluded configurations: `#if` is a special case that needs no code fix, and this is recorded as a shared principle.

The run had first tried to judge each build configuration on its own (per-branch reading, then per-assignment parsing of the preprocessor symbols with a configuration limit). Four audits each found another input where the reading missed a build or withheld a fix main offers. Withholding the fix is exact enough: the diagnostic stays, and a diagnostic without a fix is a supported end state (`AGENTS.md`, *Code-fix coverage*).

Withholdings that SHARED-FIX-003 already demands — main offers a fix that changes another build, such as a branch that closes the type's body around the moved member, a member split into a static initialized field in some build, a region reorder that puts an instance field in front of a backing field that `field` or a conditional accessor without a body declares in some build — are bug fixes against an unchanged requirement and are not listed below, except those that the backing-field rule and the check of the type's declaration add, which the end of the next section lists.

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

An excluded branch in front of the moved member that holds a modifier, RH7109:

```csharp
public class C
{
    private int _a;
#if DEBUG
    static
#endif
    private readonly int _c;
}
```

Before (`main`):

```csharp
public class C
{
#if DEBUG
    static
#endif
    private readonly int _c;
    private int _a;
}
```

After: no fix offered.

A conditional argument inside a statement of a crossed method, RH7103:

```csharp
public class C
{
    public void Run()
    {
        System.Console.WriteLine(
#if DEBUG
            "debug"
#else
            "release"
#endif
            );
    }

    public static void Create()
    {
    }
}
```

Before (`main`): the fix moves `Create` in front of `Run` (and re-indents the `#else` branch and the closing parenthesis of the call).

After: no fix offered.

Whole statements in a lambda inside a crossed field initializer, RH7109:

```csharp
using System;

public class C
{
    private Action _a = () =>
    {
#if DEBUG
        Console.WriteLine();
#endif
    };

    private readonly int _c;
}
```

Before (`main`): the fix moves `_c` in front of `_a` (and re-indents the lambda's braces).

After: no fix offered.

A group of whole accessors that gives a crossed property a backing field in some builds only, RH7103 (measured at `3578f98e`):

```csharp
public class C
{
    public int Value
    {
        get => 0;
#if DEBUG
        set;
#endif
    }

    public static int Shared => 1;
}
```

Before (`main`): the fix moves `Shared` in front of `Value`.

After: no fix offered. With `get;` in place of `get => 0;`, or `set { }` in place of `set;`, the fix stays as on `main`.

A conditional attribute list on the reordered type, RH7103 (measured at `3578f98e`):

```csharp
#if DEBUG
[System.Serializable]
#endif
public class C
{
    public void Run()
    {
    }

    public static void Create()
    {
    }
}
```

Before (`main`): the fix moves `Create` in front of `Run`.

After: no fix offered. A whole type in an `#if` block in front of `C` keeps `main`'s fix.

These withholdings, measured at `3578f98e` as well, are bug fixes against SHARED-FIX-003, because `main`'s fix changes a layout in some build:

- `get => 0;` followed by `set;` in a property of a struct, RH7309 (the SHARED-ORDER-007 example): `main` moves the `Fields` block in front of `Value`'s backing field in every build; now no fix is offered.
- The same with `set;` inside `#if DEBUG` … `#endif`, RH7309: `main` moves the `Fields` block; now no fix is offered.
- `#if INTEROP` / `[StructLayout(LayoutKind.Sequential)]` / `#endif` on a class, RH7109 (the SHARED-ORDER-008 example): `main` moves `_c` in front of `_a`, which reorders the layout in a build with `INTEROP`; now no fix is offered.

The same holds for every other conditional directive that stands between two tokens of a member declaration and at no boundary of whole attribute lists, accessors, or statements of a member's body — in an expression body, a parameter list, a constructor initializer, or a base list — and for a group of whole accessors whose branch contains `field` in a type where no build would change.

Unchanged against `main`: a group of whole members in front of the moved member or in the crossed span (the SHARED-ORDER-007 examples, and `#if DEBUG` around a whole method in front of the moved member) or in the body of a crossed nested type; a group of whole attribute lists on the moved or a crossed member; a group of whole accessors without `field` whose branches hold no accessor without a body, or whose list holds no accessor with one; a group of whole statements in a method's or an accessor's body or in a switch section that does not use `field` (the first SHARED-ORDER-006 example, the second RH7309-005 example, the SHARED-ORDER-008 examples); and a branch that holds only comments or `#region` lines. These keep main's fix and output.

## Decisions

### Batch 3, answer A — withhold for every conditional directive that is not a group of whole members

The maintainer's answer (2026-11-08), quoted from the run's record: the fix is withheld whenever the moved or crossed span holds a conditional directive that is not a group of whole members — an `#if` inside a member, splitting a member's modifiers, nested `#if`, or crossing a member or type boundary; groups of whole members keep main's reading (initializer and layout check). The maintainer does not want fixes to parse or infer excluded configurations: `#if` is a special case that needs no code fix, and the principle is recorded in the shared specs (SHARED-FIX-009). This decides the initializer split, the split modifiers, the input of thoenissen/Reihitsu#1593, the nested group, the excluded branch that is not code, the excluded modifier, and the conditional argument listed under *Visible output changes*.

### Batch 4, answer A — whole statements inside a member's body keep the fix

The maintainer's answer (2026-11-08): a group of whole statements inside a member body that the fix moves or crosses keeps main's fix, except an accessor branch that uses `field`. The maintainer accepts that some `#if` shapes are allowed exceptions. The statements of a switch section count like those of a block. A group in a lambda inside a field initializer is not in a member's body and is withheld (listed above). The `field` exception decides the accessor case listed above.

### Batch 5 — more whole units keep the fix; no condition is evaluated

The maintainer's answers (2026-11-08):

1. **A** — a group of whole members in the body of a nested type that the move crosses keeps main's fix.
2. **A** — a group of whole attribute lists on the moved or a crossed member keeps main's fix.
3. **A** — a group of whole accessors keeps main's fix, except accessors that use `field`.

The maintainer is concerned that reading inside conditions grows complex again: no condition is evaluated, and the analyzer and fix must stay maintainable. The guard is therefore a small structural test of where the directives stand — the token whose leading trivia holds each directive and the list of units that token begins or ends — plus main's existing parse of excluded text, checked for whole units; it evaluates no condition, combines no branches, parses no configuration, and has no configuration limit. Anything that test cannot classify is refused (SHARED-ORDER-008).

### Batch 6, answer A — main's parse of excluded members stays

The maintainer's answer (2026-11-08): main's parse of excluded text in the type body, which finds the members SHARED-ORDER-007 counts, stays as it is for now. Refusing every `#if` block in the spans and dropping that parse is a possible later simplification that the maintainer will revisit, not this change. The guard checks that parse's result for whole members or whole attribute lists; for a branch inside a member, it places the text into an empty list of the unit's kind and parses it the same way.

### A conditional accessor without a body refuses the move; one statement of the backing-field rule

The lead's decision 38, on a finding of the run's fifth audit. Keeping whole accessors (batch 5, answer 3A) admits a group that gives a property a backing field in one build only, such as `#if DEBUG get; #else get => 0; #endif` in a struct, whose reorder SHARED-FIX-003 forbids. An earlier stage counted such a property as declaring a backing field, with a second reading of the excluded accessors that stated the backing-field rule a second time and disagreed with the first on `get => 0; set;`. Following `AGENTS.md`, *Special cases do not grow the source*, the rare class is refused instead: a group of whole accessors with a branch that holds an accessor without a body, while an accessor of the list has one, refuses the move like `field` (SHARED-ORDER-008). A group in which every accessor lacks a body, such as `get; #if DEBUG set; #endif`, gives a backing field in every build and keeps the fix that batch 5 keeps. The backing-field rule is stated once (SHARED-ORDER-007): any accessor without a body is enough, which also closes the plain `get => 0; set;` shape that `main` fixes against SHARED-FIX-003.

### A conditional directive in the reordered type's declaration refuses every move

The lead's decision 39, on a hint of the run's fifth audit. Whether a type's field order sets its layout is read from the analyzed build's symbol, so a conditional `[StructLayout(LayoutKind.Sequential)]` or a conditional `struct` in place of `class` gives another build a layout the fix does not see. A conditional directive in the declaration in front of the body refuses every move of the type: a check of where the directive stands. A group in front of the declaration whose excluded branches read as whole members, such as a whole type, changes nothing of the type and keeps the moves, so a type declared after an `#if` block that holds another type keeps `main`'s fix.

### Branches that hold only comments or region lines

A branch that holds nothing but whitespace, comments, or `#region` and `#endregion` lines is a sequence of no units, so `#else // nothing in release` and a region inside an excluded branch keep main's fix and output.

### The parts of the run's earlier reading that this replaces

The per-configuration reading of SHARED-ORDER-007 (each build judged on its own, assignments of the preprocessor symbols, the 256-configuration limit, the membership condition for branches that close or open a type) is removed. Its outcomes for the inputs it was written for are kept or replaced by withholding: every input whose conditional directives do not wrap whole units now withholds the fix.

## Alternatives considered

- **Judge each build configuration on its own** — rejected by the maintainer's batch 3 answer: it requires evaluating conditions and combining branches, and four audits found inputs it read wrong.
- **Withhold for every conditional directive inside a member, the body included** — the literal reading of the batch 3 answer; rejected by the batch 4 answer for statements and the batch 5 answers for attribute lists and accessors.
- **Withhold for whole members in a crossed nested type, whole attribute lists, or whole accessors** — rejected by the batch 5 answers.
- **Count a conditional accessor without a body as a backing field instead of refusing** — rejected by decision 38: it needs a second reading of excluded accessors and a second statement of the backing-field rule, for a rare class.
- **Refuse every group of whole accessors with an accessor without a body** — rejected: it would withdraw the fix for `get; #if DEBUG set; #endif`, which batch 5 (answer 3A) keeps, although that group gives the property a backing field in every build.
- **Refuse every move in a type with any conditional directive in front of its first token** — rejected: it would withhold fixes for a type declared after an `#if` block that holds another whole type, which people commonly write.
- **Main's reading unchanged** — rejected: main offers fixes that change another build when a member is split by `#if` (SHARED-FIX-003), which thoenissen/Reihitsu#1593 reports.

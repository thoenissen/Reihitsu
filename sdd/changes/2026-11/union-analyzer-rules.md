# Union declarations in the type-kind analyzer rules

| | |
|---|---|
| Status | Accepted |
| Date | 2026-11 |
| Affected requirements | RH2004-001, RH2004-006, RH5105-001, RH5106-001, RH5108-001, RH5110-001, RH5404-001, RH5404-003, RH5507-001, RH5508-001, RH7102-001, RH7103-001, RH7104-001, RH7109-001, RH7110-001, RH7308-001, RH7309-002, RH7311-001, RH7311-002, SHARED-ATTR-001, SHARED-MODORDER-001, SHARED-REGGROUP-001, SHARED-TYPEREGION-001, SHARED-TYPEREGION-002, SHARED-TYPEREGION-006; new specs: RH2110 (RH2110-001), RH5418 (RH5418-001); clarified: SHARED-NESTED-001, RH7104-004, RH5035-001, RH5038-001, RH5101-001, RH5102-001, RH5107-001, RH5109-001, RH5203-001, RH5305-002, FMT-STRUCT-017, FMT-STRUCT-028 |
| Depends on | [Union declarations are laid out like the other type declarations](union-declaration-layout.md), [No space between `with` and its argument list in a collection expression](with-element-spacing.md) |

## What

A C# 15 union declaration (`union Pet(Cat, Dog) { … }`) becomes a type declaration for the design, organization, and layout rules that name the kinds of type they check, as the naming and documentation rules already treat it ([union-naming-documentation.md](union-naming-documentation.md)):

- **RH2004** reports a union without an accessibility modifier; its fix adds `internal` at the top level and `private` in a type. A member declared in a union body is already reported and fixed with `private`; RH2004-006 now says so.
- **RH5404** reports a union whose braces share one line; its fix lays the union out through the formatter (FMT-BREAK-002, FMT-BREAK-007, FMT-STRUCT-017).
- **RH7102, RH7103, RH7109, RH7110** order the members of a union body; **RH7104** reports a `partial` union without accessibility, and its fix inserts `internal` for a top-level union; **RH7105, RH7106** check a union's modifiers (SHARED-MODORDER-001).
- **RH7305, RH7305A** check a top-level union with members for regions (SHARED-TYPEREGION-001); **RH7308, RH7309, RH7311** check a union's regions; **RH7401 to RH7412** group a union's overrides and interface implementations by region (SHARED-REGGROUP-001). A region inside a union belongs to the union (SHARED-TYPEREGION-006).
- **RH5105, RH5106, RH5108** check the parentheses and the first case type of a union's case-type list, which the formatter lays out as a primary constructor's parameter list (FMT-BREAK-011); **RH5110** checks a union's constraint clauses (FMT-BREAK-023).
- A union's attribute lists resolve to the **struct target** (SHARED-ATTR-001), so **RH5507** and **RH5508** check them, and the formatter's attribute split (FMT-STRUCT-028), which keys on the same target resolution, splits them.

Two new rules give the union what every other type kind has a rule of its own for:

- **RH5418 — Empty unions should use semicolon declarations** (Layout, Warning, enabled, Fix All) reports a union with an empty brace body from C# 15 on, as RH5412 to RH5416 report the other type kinds (SHARED-EMPTY-001 to SHARED-EMPTY-004); its fix writes the semicolon declaration the formatter already writes (FMT-STRUCT-017).
- **RH2110 — Nested unions should not be used** (Design, Warning, enabled, no fix) reports a union declared inside another type, as RH2101 and RH2104 to RH2108 report the other kinds (SHARED-NESTED-001, SHARED-NESTED-002).

Clarifications that change no output:

- RH5035, RH5107, and RH5109 already check the case-type list of a union, and the rules that judge a type body by nesting already treat a union body as one (SHARED-NESTED-001): a class nested in a union is reported by RH2101.
- RH5101, RH5102, RH5203, and RH5038 already check the argument list of a `with(…)` element; RH5305 already counts a `with(…)` element as an element (FMT-BREAK-022). RH6001 does not check the `with` of such an element, which takes its argument list without a space (FMT-SPACE-028); this is now a non-goal.
- FMT-STRUCT-028 already names every type; its example now shows a union.
- RH7104 already reports a `partial` type nested in a union and fixes it with `private`; RH7104-004 now says so.
- RH7110-001 now states what the rule already does: a member is compared with every earlier member of its group (SHARED-USING-007), not only the previous one.
- RH2103 does not report the case-type list of a union, which is not a primary constructor, and RH2005 does not report the fields of a union, which is a struct; both are now non-goals.
- RH5404 loses its non-goal "Union declarations", which RH5404-001 contradicted once it named the union.
- FMT-STRUCT-017's `Why:` no longer says that no rule reports a braced union: RH5418 does. The formatter's output is unchanged.
- The non-goals of RH2101, RH2104 to RH2108, RH7002, RH5404, RH5412 to RH5416, and the shared empty-type spec point at the new rules.

## Why

These rules register explicit syntax kinds — class, struct, interface, record, record struct — or a target resolution that lists them, so a union is skipped: a union with members outside regions reports no RH7305 while a class does, `union U(int, string);` without a modifier reports no RH2004, and `[Serializable, Obsolete] union U(int, string);` keeps both attributes in one list while a struct gets one per list (thoenissen/Reihitsu#1767). Each of these rules names a construct a union has in the same form as a struct — a modifier list, a member list, regions in a type body, a parameter-like list after the name, constraint clauses, attribute lists — so skipping the union is the rule's own gap ([P11](../../README.md#the-formatter-may-do-more-than-the-analyzers-p11)), not a new construct. The formatter already lays these parts of a union out like a class's ([union-declaration-layout.md](union-declaration-layout.md)), so the rules and their fixes now agree with it.

The compiler emits a union as a struct and accepts on it exactly the attributes valid on a struct (an attribute limited to `AttributeTargets.Class` fails with CS0592 on a union), so its attribute lists follow the struct rules, as a `record struct`'s do.

## Visible output changes

All inputs parse as C# 15. "Before" is today's `main`; every diagnostic named under "after" is new unless stated.

**1. RH2004 — a union without an accessibility modifier is reported and fixed.**

```csharp
// input
union Choice(int, string)
{
    int Get() => 1;

    union Inner(long, string);
}
```

```csharp
// before: RH2004 on `Get` only; the fix writes `private int Get() => 1;`
```

```csharp
// after: RH2004 on `Choice`, `Get`, and `Inner`; the fix gives
internal union Choice(int, string)
{
    private int Get() => 1;

    private union Inner(long, string);
}
```

**2. RH5404 — a union body on one line is reported, and the fix lays it out.**

```csharp
// input
union U(int, string) { public int X => 1; }

union V(int, string) { }
```

```csharp
// before: not reported
```

```csharp
// after: RH5404 on `U` and `V`; the fix gives
union U(int, string)
{
    public int X => 1;
}

union V(int, string);
```

The rule page drops its sentence that union declarations are not reported.

**3. RH7102, RH7103, RH7109, RH7110 — members of a union body are ordered.**

```csharp
// input
public union U(int, string)
{
    private void A()
    {
    }

    public void B()
    {
    }

    public static int S;
    public static readonly int RO = 1;
    public const int K = 2;
    public static void Z()
    {
    }
}
```

```csharp
// before: not reported
```

```csharp
// after: as for the same members in a struct — RH7110 on `B` and `Z`, RH7109 on `RO`, RH7102 on `K`, RH7103 on `Z`;
// each fix moves the member as it moves it in a struct
```

**4. RH7104 — a `partial` union without accessibility is reported and fixed.**

```csharp
// input
partial union U(int, string);

public union V(int, string)
{
    partial class N
    {
    }
}
```

```csharp
// before: RH7104 on `N` only; the fix writes `private partial class N`
```

```csharp
// after: RH7104 on `U` and `N`; the fixes give
internal partial union U(int, string);

public union V(int, string)
{
    private partial class N
    {
    }
}
```

Only `U` is new: RH7104 already reports a `partial` class nested in a union and fixes it with the union's default, `private`.

**5. RH7105, RH7106 — a union's modifiers are ordered.**

```csharp
// input
readonly public union U(int, string);

public class C
{
    internal protected union V(int, string);
}
```

```csharp
// before: not reported
```

```csharp
// after: RH7105 on `readonly`, RH7106 on `protected`; the fixes give
public readonly union U(int, string);

public class C
{
    protected internal union V(int, string);
}
```

**6. RH7305, RH7305A — a top-level union with members is checked for regions.**

```csharp
// input
public union U(int, string)
{
    public int Get() => 1;
}
```

```csharp
// before: not reported
```

```csharp
// after: RH7305 on `U` (and RH7305A when the union has members of several kinds); no code fix
```

A union without a body, `union U(int, string);`, declares no members and stays unreported.

**7. RH7308, RH7309, RH7311 — the regions of a union are checked.**

```csharp
// input
using System;

public union U(int, string) : IComparable
{
    #region IComparable

    public int CompareTo(object o)
    {
        return 0;
    }

    #endregion // IComparable

    #region Methods

    public static int Count;

    #endregion // Methods

    #region Methods

    public void M()
    {
    }

    #endregion // Methods
}
```

```csharp
// before: not reported
```

```csharp
// after: as for the same regions in a struct — RH7308 on `Count`, RH7309 on both `#region Methods`, RH7311 on the second `#region Methods`;
// RH7309's fix reorders the regions as it does in a struct
```

**8. RH7401 to RH7412 — a union's overrides and interface implementations are grouped by region.**

```csharp
// input
using System;

public union U(int, string) : IComparable
{
    #region Methods

    public int CompareTo(object o)
    {
        return 0;
    }

    public override string ToString()
    {
        return "";
    }

    #endregion // Methods
}
```

```csharp
// before: not reported
```

```csharp
// after: RH7409 on `CompareTo` (region `IComparable` expected), RH7401 on `ToString` (region `Object` expected), as in a struct; no code fix
```

**9. RH5105, RH5106, RH5108, RH5110 — the case-type list and the constraints of a union.**

```csharp
// input
union L1
    (int, string);

union L2(
    int, string);

union L3(int, string
    );

union L4<T>(T, string) where T : class;
```

```csharp
// before: not reported
```

```csharp
// after: RH5105 on the `(` of `L1`, RH5108 on `int` of `L2`, RH5106 on the `)` of `L3`, RH5110 on `where`; the fixes give
union L1(int, string);

union L2(int, string);

union L3(int, string);

union L4<T>(T, string)
    where T : class;
```

This is the layout the formatter gives these inputs today. RH5108 does not check the primary constructor of a class or a struct; the union's case-type list is checked as a record's parameter list is.

**10. RH5507, RH5508, and the formatter — a union's attribute lists follow the struct rules.**

```csharp
// input
[Serializable, Obsolete] public union U(int, string);
```

```csharp
// before: not reported; the formatter leaves the line as written apart from the line break behind the list:
[Serializable, Obsolete]
public union U(int, string);
```

```csharp
// after: RH5507 and RH5508 on `[Serializable, Obsolete]`; the formatter and the RH5508 fix give
[Serializable]
[Obsolete]
public union U(int, string);
```

The rule pages of RH5507 and RH5508 name unions next to record structs.

**11. RH5418 — an empty union body is reported, and the fix writes the semicolon declaration.**

```csharp
// input
public union U(int, string)
{
}

union W(int, string)
{
    // reserved
}
```

```csharp
// before: not reported (the formatter already writes `public union U(int, string);` and keeps the braces of `W`)
```

```csharp
// after: RH5418 on `U` and `W`; the fix is offered for `U` only and gives
public union U(int, string);

union W(int, string)
{
    // reserved
}
```

A union whose empty body shares one line with its braces, `union V(int, string) { }`, is reported by RH5418 and by RH5404 (change 2), as `struct S { }` is reported by RH5413 and RH5404; both fixes give `union V(int, string);`.

**12. RH2110 — a nested union is reported.**

```csharp
// input
public class C
{
    public union Result(int, string);
}

public union Top(int, string)
{
    union Inner(long, string);
}
```

```csharp
// before: not reported (RH2104 reports nested structs only, and the union is not one)
```

```csharp
// after: RH2110 on `Result` and `Inner`; no code fix
```

**13. Rule pages.** RH5404 (see change 2), RH5105 and RH5108 (they name the case-type list of a union), RH5507 and RH5508 (change 10). New pages RH5418 and RH2110, and their rows in the package's rule list.

## Decisions

### A union is a type declaration for every rule that names type kinds

The rules widen the lists they already have; no requirement states a union rule of its own, so a later rule about type declarations is read for unions too, as the formatter's layout change decided ([union-declaration-layout.md](union-declaration-layout.md), *A union is a type declaration for every layout and shape rule that names types*). Where a rule checks the struct and the union the same way (RH7309, SHARED-REGGROUP-001), the union's overrides of `object` members come from `Object`, as a struct's do (SHARED-REGGROUP-003).

### A union's attribute lists resolve to the struct target

The attribute rules are keyed by the target the compiler applies an attribute to (SHARED-ATTR-001), and the compiler applies a union's attributes to a struct. The union therefore joins RH5507 and RH5508 instead of getting rules of its own. The formatter's split (FMT-STRUCT-028) already names every type; it skipped the union only because the shared target resolution did not know it, so once it does, the split follows without a formatter requirement change.

### The `with(…)` element needs no rule change

RH5101, RH5102, RH5203, and RH5038 check the argument list of a `with(…)` element today, and their fixes lay it out as the formatter does (FMT-BREAK-012); RH5305 counts the element as an element, as FMT-BREAK-022 does. RH6001 reports only a missing space after a statement keyword, and the formatter writes no space after this `with` (FMT-SPACE-028), so it stays silent. The requirements now name the element so that tests can carry it.

### The maintainer's answers to the output questions

The run asked four output questions; the maintainer answered each with option A.

- **Q1 — unions join the type-kind rules.** RH2004, RH5404, RH5105, RH5106, RH5108, RH5110, RH7102 to RH7106, RH7109, RH7110, RH7305, RH7305A, RH7308, RH7309, RH7311, and RH7401 to RH7412 check a union as they check a struct (changes 1 to 9). Unions are treated like the other type declarations, such as a class or a struct.
- **Q2 — a union's attribute lists follow the struct rules.** RH5507 and RH5508 report a union's attribute lists, and the formatter splits them under the unchanged FMT-STRUCT-028 (change 10).
- **Q3 — a new rule for the empty union.** RH5418 reports an empty union body from C# 15 on and fixes it, like RH5412 to RH5416 (change 11).
- **Q4 — a new rule for the nested union.** RH2110 reports a nested union without a fix, like RH2101 and RH2104 to RH2108 (change 12).

### A union gets rules of its own where every other type kind has one

The empty-type rules and the nested-type rules are one rule per kind of declaration, each named after its kind, and the naming and documentation rules gave unions their own IDs ([union-naming-documentation.md](union-naming-documentation.md)). RH5418 and RH2110 continue that pattern instead of widening RH5413 or RH2104, whose titles and messages name structs: a user who reads "Nested structs should not be used" on a `union` declaration would not recognize the construct, and a project can now configure the union rules on their own. A struct or record struct is never reported by the union rules, and a union never by RH5413, RH5416, RH2104, or RH2106.

## Alternatives considered

- **Keep unions out of these rules** — not recommended: a union with members, regions, or attributes is then left unchecked in exactly the respects a struct is checked, while the formatter already lays it out like one.
- **Rules of their own for union attributes** — not recommended: the attribute rules are named after the compiler's targets, which have no union target; a union rule pair would duplicate RH5507 and RH5508 for the same target.
- **No rule for the empty union body (Q3 B)** — rejected by the maintainer: the formatter writes the semicolon declaration either way (P11), but every other type kind with that form has a rule, and a union without the formatter would be the only one left unchecked.
- **Report empty or nested unions under RH5413 or RH2104** — rejected: those rules name structs in their titles and messages, and every other kind has its own rule.
- **No rule for the nested union (Q4 B)** — rejected by the maintainer: a nested union would be the only nested type kind no rule reports.
- **Leave the union's case-type list out of RH5108, as the primary constructors of classes and structs are** — not recommended: the issue names RH5108, and a record's parameter list, the closest form, is checked; the class and struct primary constructors are a gap of their own.

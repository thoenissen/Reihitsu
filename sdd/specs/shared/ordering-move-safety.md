# Shared — Ordering move safety

| | |
|---|---|
| ID prefix | SHARED-ORDER |
| Status | Active |
| Code owners | `OrderingMoveSafety`, `OrderingDeclarationUtilities`, `AccessorOrderingUtilities`, `TypeMemberOrderingCodeFixProviderBase` |
| Uses | SHARED-FIX-003, SHARED-TYPEREGION-006 |

This spec covers how the code fixes of the ordering rules move one declaration in front of an earlier sibling, and when such a move is refused. It applies to the member reordering fixes (RH7102 constants before fields, RH7103 static before instance, RH7109 readonly before non-readonly) and to the accessor reordering fixes (RH7107 property accessors, RH7108 event accessors). It also states the region scope that every rule ordering the members of a type shares (SHARED-ORDER-004), RH7110 included, which offers no fix. Apart from that scope, which declaration a rule reports and where it should go is each rule's own spec; this spec owns the move and its guards.

## Non-goals

- Which members a rule reports, apart from the region scope of SHARED-ORDER-004, and which earlier sibling a fix chooses as the target — each rule's own spec.
- Layout of the moved code after the move — SHARED-FIX-007. This spec only fixes which trivia travels and which stays.
- Reordering of using directives (SHARED-USING) and of whole regions (RH7309).
- Rules that report an ordering problem without offering a fix, such as RH7110, apart from the region scope of SHARED-ORDER-004.

## Requirements

### SHARED-ORDER-001 — A move stays in one list and goes forward

A fix MUST move a declaration only within the list it belongs to — the members of one type declaration, or the accessors of one accessor list — and only in front of a sibling that precedes it. Members of another part of a `partial` type, of a nested type, or of another accessor list MUST NOT be involved.

Why: Each list is one scope, and the move changes nothing but the position inside it. Moving forward only keeps every move describable as "the moved node, then the crossed nodes", which is what the guards in SHARED-ORDER-003 to SHARED-ORDER-006 inspect.

Example:

```csharp
public class C
{
    private int _value;

    public int Value
    {
        set
        {
            _value = value;
        }

        get
        {
            return _value;
        }
    }
}
```

```csharp
public class C
{
    private int _value;

    public int Value
    {
        get
        {
            return _value;
        }

        set
        {
            _value = value;
        }
    }
}
```

### SHARED-ORDER-002 — Separators stay, attached trivia travels

A move MUST keep each blank-line separator at the position it occupied: the whitespace and line breaks in front of a declaration's first comment, documentation comment, or directive belong to the slot, not to the declaration. Comments, documentation comments, directives, and disabled text in the moved declaration's leading trivia MUST travel with it, together with the indentation in front of its first token.

Why: A blank line separates two positions, and a comment or documentation comment describes the declaration below it. A plain remove-and-insert would carry the separator away with whichever declaration happens to own it in the trivia and leave the moved member glued to the type's opening brace.

Example:

```csharp
public class TestClass
{
    public void Run()
    {
    }

    /// <summary>
    /// Creates a new instance
    /// </summary>
    public static void Create()
    {
    }
}
```

```csharp
public class TestClass
{
    /// <summary>
    /// Creates a new instance
    /// </summary>
    public static void Create()
    {
    }

    public void Run()
    {
    }
}
```

### SHARED-ORDER-003 — A conditional directive is not torn from its partner

A fix MUST NOT offer a move when the moved declaration's full span, or the full span of the crossed declarations (from the target's first trivia to the moved declaration's first trivia), contains an `#if` or `#endif` whose partner lies outside that span, or an `#elif` or `#else` outside any `#if` of that span. The whole span is inspected, including directives after an attribute list or between modifiers.

Why: Moving such a directive separates it from its partner, so the moved or the crossed code ends up compiled under different conditions or the file no longer compiles (CS1027, CS1028). A directive after an attribute list attaches to a later token, so checking only the first token's leading trivia would miss it.

Example:

```csharp
public class TestClass
{
    public void Run()
    {
    }

    [System.Obsolete]
#if DEBUG
    public static void Create()
    {
    }
#endif
}
```

is reported by RH7103 when `DEBUG` is defined, and no code fix is offered.

### SHARED-ORDER-004 — A region boundary is not crossed

A rule that checks the order of the members of a type declaration MUST check each top-level region of the declaration (SHARED-TYPEREGION-006) on its own: it MUST compare a member only with members that lie in the same top-level region, or, for a member that lies outside every top-level region, only with members that lie outside every top-level region too. A member that lies in a region nested in a top-level region belongs to that top-level region and MUST be compared with the other members of it. A fix MUST NOT offer a move when the moved span or the crossed span (SHARED-ORDER-003) contains a `#region` or `#endregion` whose partner lies outside that span.

Why: A type organized in regions has chosen its outline by region, and each region is read on its own. Comparing a member with one in another region would ask it to leave the region its author chose. A nested region is a sub-heading of the region around it, so its members are read in order with the rest of that region. Moving the directive with the declaration, or moving the declaration across it, puts declarations in a region the author did not choose. Regions are excluded from SHARED-ORDER-005, so this requirement is their only guard.

Example — the members are compared only within their own top-level region:

```csharp
public class C
{
    #region Instance

    private int _a;

    public void Run()
    {
    }

    #endregion

    #region Shared

    public int B;

    public static void Create()
    {
    }

    #endregion
}
```

is not reported: RH7103 does not compare `Create` with `Run`, and RH7110 does not compare `B` with `_a`, because each pair lies in two top-level regions.

Example — RH7102, where the field and the constant lie in two top-level regions:

```csharp
class C
{
    #region Fields

    private int _count;

    #endregion

    #region Constants

    private const int Max = 1;

    #endregion
}
```

is not reported.

Example — a region boundary in the crossed span:

```csharp
public class TestClass
{
    #region Fields
    private int _instance;
    private static int _static = 1;
    #endregion
}
```

is reported by RH7103, and no code fix is offered: the `#region` line belongs to the leading trivia of `_instance`, so it lies in the crossed span without its partner.

### SHARED-ORDER-005 — A position-sensitive directive blocks the move

A fix MUST NOT offer a move when the leading trivia of the moved declaration or of a crossed declaration contains a directive that is neither a conditional directive nor a region directive — `#pragma`, `#nullable`, `#line`, `#error`, `#warning`, or any other kind.

Why: These directives apply from their position onwards, so moving code across them, or moving them with code, changes which code they cover. Every kind counts, so a directive kind the guard does not know about refuses the move instead of slipping through.

Example:

```csharp
public class C
{
    public void Run()
    {
    }

#pragma warning disable CS0168
    public static void Create()
    {
    }
}
```

is reported by RH7103, and no code fix is offered.

### SHARED-ORDER-006 — Balanced directive pairs travel intact

A move MUST stay available when every conditional and region directive in the moved span and in the crossed span has its partner in the same span, and neither span contains a directive of any other kind.

Why: A complete pair inside one half moves or stays as one unit, so the code it governs is the same before and after the move. Refusing it would give away safe ground — for example every member whose body contains an `#if DEBUG` block.

Example:

```csharp
public class TestClass
{
    public void Run()
    {
#if DEBUG
        System.Console.WriteLine();
#endif
    }

    public static void Create()
    {
    }
}
```

```csharp
public class TestClass
{
    public static void Create()
    {
    }

    public void Run()
    {
#if DEBUG
        System.Console.WriteLine();
#endif
    }
}
```

A balanced `#if false` … `#endif` block in the moved declaration's own leading trivia travels with it under SHARED-ORDER-002:

```csharp
public class TestClass
{
    public void Run()
    {
    }

#if false
    public static void Old()
    {
    }
#endif
    public static void Create()
    {
    }
}
```

```csharp
public class TestClass
{
#if false
    public static void Old()
    {
    }
#endif
    public static void Create()
    {
    }

    public void Run()
    {
    }
}
```

### SHARED-ORDER-007 — Initializer execution order is kept

A member reordering fix — RH7102, RH7103, or RH7109 — MUST NOT offer a move when a member that the move moves has a runtime initializer and a member of the same context that the move crosses — both static or both instance — also has one. A `const` field has no runtime initializer. A member that code excluded by an `#if`, `#elif`, or `#else` branch declares counts like any other member, because a build that includes it runs its initializer (SHARED-FIX-003, *Build configurations*):

- a member of excluded code in the crossed span (SHARED-ORDER-003) is crossed;
- a member of excluded code in the moved declaration's own leading trivia moves, because that trivia travels with the declaration (SHARED-ORDER-002), and it crosses every member the declaration crosses. The move is refused when such a member has a runtime initializer and a crossed member of its context has one, also when the moved declaration itself has none.

The moved members are the moved declaration and the members of excluded code in its leading trivia or inside it; no other member moves.

A member reordering fix MUST NOT offer a move either when it would reorder the instance fields of a type whose field order sets its layout (SHARED-FIX-003, *Run time*), and it counts the instance fields the same way: an instance field of excluded code in the moved declaration's leading trivia moves across every instance field the move crosses, and an instance field of excluded code in the crossed span is crossed. A move of a static member therefore reorders instance fields when excluded code in its leading trivia declares one and the move crosses an instance field.

A member whose declaration an `#if`, `#elif`, or `#else` branch splits, so that the configurations compile different parts of it, is read per configuration: each configuration contributes what it declares the member as, and the member counts with what any configuration contributes:

- it has a runtime initializer when one configuration that does not declare it `const` gives it an initializer, such as an `= Log("b")` that only a build with `DEBUG` compiles, also behind the accessor list of a property, and also when its initializer is active in every configuration and only one configuration declares it without `const`;
- it is a static member when one configuration declares it `static` and not `const`, and an instance member when one configuration declares it neither `static` nor `const`; it counts in both contexts when one configuration makes it a static member and another an instance member;
- it is an instance field when one configuration declares it as one;
- a configuration that declares it `const` contributes nothing: neither a runtime initializer, nor a context, nor an instance field, because a constant has nothing to reorder. A member that is `static readonly` in one configuration and `const` in every other therefore counts only as a static member, with a runtime initializer when the `static readonly` configuration gives it one;
- a further member that excluded code inside its declaration declares counts like any member of excluded code, at that position.

A split member whose parts give it no runtime initializer, context, or instance field that conflicts with a member it is moved across keeps the move.

Every member reordering fix applies both guards. RH7102 moves only constants. A constant that no `#if`, `#elif`, or `#else` splits has no runtime initializer and takes no place in the layout, but the excluded code in its leading trivia moves with it; a declaration that is `const` only in some configurations counts as a split member. RH7309 counts initializers and instance fields the same way.

Why: Initializers run in declaration order within their context, the static ones in the static constructor and the instance ones in each instance constructor. Swapping two of them can change observable values silently. Static and instance initializers run at different times, so crossing a member of the other context is safe. Excluded code is compiled by another build, and the code in front of the moved declaration goes where the declaration goes, so in that build its members are moved like the declaration itself; refusing only for the declaration's own initializer would let the fix reorder that build's initializers or fields. A build sees only the parts of a split member that it compiles, so an initializer or a `static` that one build adds to the member is that build's, and the member is moved or crossed with it; a build that compiles the member as a `const` sees a constant, which runs no initializer and takes no place in the layout, so that build adds nothing to the member.

Example:

```csharp
public class TestClass
{
    private int _instance;
    private static int _a = Compute();
    private static int _b = _a;

    private static int Compute() => 1;
}
```

Both `_a` and `_b` are reported by RH7103. For `_b` alone, no code fix is offered, because it would cross `_a`. For `_a`, the fix is offered; once it is applied, `_b` no longer crosses an initialized static member and is fixed too.

A member in excluded code that the move crosses refuses it like an active one. Without `DEBUG` defined,

```csharp
public class C
{
    private int _a;
#if DEBUG
    private int _c = Log("c");
#endif
    private int _d;
    private readonly int {|RH7109:_b|} = Log("b");

    private static int Log(string name) => 0;
}
```

is reported by RH7109, and no code fix is offered: moving `_b` in front of `_a` would run `Log("b")` before `Log("c")` in a build with `DEBUG`.

Excluded code in front of the moved member moves with it, and its members cross what the moved member crosses. Without `DEBUG` defined,

```csharp
public class C
{
    private int _a = Log("a");
#if DEBUG
    private int _c = Log("c");
#endif
    private readonly int {|RH7109:_b|};

    private static int Log(string name) => 0;
}
```

is reported by RH7109, and no code fix is offered: `_b` has no initializer, but `_c` moves with it in front of `_a`, so a build with `DEBUG` would run `Log("c")` before `Log("a")`. When the crossed members have no initializer, the fix is offered, and the excluded code keeps its place in front of the moved member:

```csharp
public class C
{
    private int _a;
    private int _d;
#if DEBUG
    private int _c = Log("c");
#endif
    private readonly int {|RH7109:_b|} = Log("b");

    private static int Log(string name)
    {
        return 0;
    }
}
```

```csharp
public class C
{
#if DEBUG
    private int _c = Log("c");
#endif
    private readonly int _b = Log("b");
    private int _a;
    private int _d;

    private static int Log(string name)
    {
        return 0;
    }
}
```

The instance fields of a layout type are counted the same way. Without `DEBUG` defined,

```csharp
public struct S
{
    private int _x;
#if DEBUG
    private int _y;
#endif
    private static int {|RH7103:_s|};
}
```

is reported by RH7103, and no code fix is offered: `_y` moves with `_s` in front of `_x`, so a build with `DEBUG` would lay out `_y` before `_x`.

A member that an `#if` splits counts with what the excluded part gives it. Without `DEBUG` defined,

```csharp
public class C
{
    private int _a = Log("a");
    private readonly int {|RH7109:_b|}
#if DEBUG
        = Log("b")
#endif
        ;

    private static int Log(string name)
    {
        return 0;
    }
}
```

is reported by RH7109, and no code fix is offered: in a build with `DEBUG`, `_b` has an initializer, and moving it in front of `_a` would run `Log("b")` before `Log("a")`.

A `static` that only one configuration compiles counts in both contexts. Without `DEBUG` defined,

```csharp
using System;

public struct S : IDisposable
{
    #region IDisposable

    private int _a;

    public void Dispose()
    {
    }

    #endregion // IDisposable

    {|RH7309:#region Fields|}

    private
#if !DEBUG
        static
#endif
        int _b;

    #endregion // Fields
}
```

is reported by RH7309, and no code fix is offered: in a build with `DEBUG`, `_b` is an instance field, and moving `Fields` in front of `IDisposable` would lay out `_b` before `_a`.

A configuration that declares the split member `const` adds no context. Without `DEBUG` defined,

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
    private readonly int {|RH7109:_b|} = Log("b");

    private static int Log(string name)
    {
        return 0;
    }
}
```

is reported by RH7109, and its code fix is offered and moves `_b` in front of `_a`: in a build with `DEBUG`, `K` is a static member with an initializer, which `_b` may cross, and in a build without `DEBUG`, `K` is a constant. Neither build gives `K` an instance initializer that `_b` would cross. With `private readonly` in place of `private const`, the build without `DEBUG` gives `K` an instance initializer, and no code fix is offered.

A member split across `#if DEBUG` and `#else` keeps the move when no configuration gives a moved member and a crossed member of one context an initializer each. Without `DEBUG` defined, RH7309 reports the `Fields` block, and its fix moves the block in front of `IDisposable`, which holds no initialized member:

```csharp
using System;

public class C : IDisposable
{
    #region IDisposable

    public void Dispose()
    {
    }

    #endregion // IDisposable

    {|RH7309:#region Fields|}

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

```csharp
using System;

public class C : IDisposable
{
    #region Fields

#if DEBUG
    private static readonly int X = Log("x");
    private static readonly int Y =
#else
    private static readonly int Y =
#endif
        Log("y");

    #endregion // Fields

    #region IDisposable

    public void Dispose()
    {
    }

    #endregion // IDisposable

    private static int Log(string name) => 0;
}
```

Crossing an instance initializer is allowed:

```csharp
public class C
{
    private int _instance = 1;

    // The shared counter
    private static int _count = 0;
}
```

```csharp
public class C
{
    // The shared counter
    private static int _count = 0;

    private int _instance = 1;
}
```

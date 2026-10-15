# Shared — Ordering move safety

| | |
|---|---|
| ID prefix | SHARED-ORDER |
| Status | Active |
| Code owners | `OrderingMoveSafety`, `OrderingDeclarationUtilities`, `AccessorOrderingUtilities`, `TypeMemberOrderingCodeFixProviderBase`, `SyntaxTriviaUtilities` |
| Uses | — |

This spec covers how the code fixes of the ordering rules move one declaration in front of an earlier sibling, and when such a move is refused. It applies to the member reordering fixes (RH7102 constants before fields, RH7103 static before instance, RH7109 readonly before non-readonly) and to the accessor reordering fixes (RH7107 property accessors, RH7108 event accessors). Which declaration a rule reports and where it should go is each rule's own spec; this spec owns the move and its guards.

## Non-goals

- Which members a rule reports, and which earlier sibling a fix chooses as the target — each rule's own spec.
- Layout of the moved code after the move. Every fix lays the result out with the formatter (FMT-PIPE); this spec only fixes which trivia travels and which stays.
- Reordering of using directives (SHARED-USING) and of whole regions (RH7309).
- Rules that report an ordering problem without offering a fix, such as RH7110.

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

A fix MUST NOT offer a move when the moved span or the crossed span (SHARED-ORDER-003) contains a `#region` or `#endregion` whose partner lies outside that span.

Why: Moving the directive with the declaration, or moving the declaration across it, puts declarations in a region the author did not choose. Regions are excluded from SHARED-ORDER-005, so this requirement is their only guard.

Example:

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

A member reordering fix that can move fields, event fields, or properties with a runtime initializer MUST NOT offer a move when the moved member has a runtime initializer and a crossed member of the same context — both static or both instance — also has one. A `const` field has no runtime initializer. RH7103 and RH7109 apply this guard.

Why: Initializers run in declaration order within their context, the static ones in the static constructor and the instance ones in each instance constructor. Swapping two of them can change observable values silently. Static and instance initializers run at different times, so crossing a member of the other context is safe.

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

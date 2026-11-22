# Shared — Ordering move safety

| | |
|---|---|
| ID prefix | SHARED-ORDER |
| Status | Active |
| Code owners | `OrderingMoveSafety`, `OrderingDeclarationUtilities`, `AccessorOrderingUtilities`, `TypeMemberOrderingCodeFixProviderBase` |
| Uses | RH7102-006, RH7309-005, SHARED-DOC-004, SHARED-FIX-003, SHARED-FIX-006, SHARED-FIX-007, SHARED-FIX-009, SHARED-TYPEREGION-006 |

This spec covers how the code fixes of the ordering rules move one declaration in front of an earlier sibling, and when such a move is refused. It applies to the member reordering fixes (RH7102 constants before fields, RH7103 static before instance, RH7109 readonly before non-readonly) and to the accessor reordering fixes (RH7107 property accessors, RH7108 event accessors). SHARED-ORDER-007, SHARED-ORDER-008, and SHARED-ORDER-011 also apply to the RH7309 fix, which reorders region blocks. It also states the region scope that every rule ordering the members of a type shares (SHARED-ORDER-004), RH7110 included, which offers no fix. Apart from that scope, which declaration a rule reports and where it should go is each rule's own spec; this spec owns the move and its guards.

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

A move MUST keep each blank-line separator at the position it occupied: the whitespace and line breaks in front of a declaration's first comment, documentation comment, or directive belong to the slot, not to the declaration. Comments, documentation comments, directives, and disabled text in the moved declaration's leading trivia MUST travel with it, together with the indentation in front of its first token. The column each of those lines takes at the new position is SHARED-FIX-007's (*Moved code*): a line that holds nothing but comments, the first of them in column 0, keeps column 0.

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

A move MUST stay available when every conditional and region directive in the moved span and in the crossed span has its partner in the same span, every directive of any other kind in either span is one that SHARED-ORDER-011 keeps, and, for a member reordering fix, SHARED-ORDER-007 and SHARED-ORDER-008 do not refuse it. Any other directive that is neither a conditional nor a region directive refuses the move wherever it stands in either span: in a declaration's leading trivia under SHARED-ORDER-005, and anywhere else in the spans under this requirement. For a member reordering fix, SHARED-ORDER-008 refuses every conditional directive in either span, so the balanced pairs that keep a member move are region pairs and the pairs of SHARED-ORDER-011; for an accessor reordering fix, a balanced conditional pair keeps the move as well.

Why: A complete pair inside one half moves or stays as one unit, so the code it governs is the same before and after the move. Refusing it would give away safe ground — for example every accessor whose body contains an `#if DEBUG` block. A member move gives that ground up for conditional pairs, because telling a harmless conditional group around members from one that changes a member in another build needs the excluded code read (SHARED-ORDER-008); an accessor carries no initializer and no field, so a conditional pair inside one does not need that reading.

Example — a balanced conditional pair in a crossed accessor's body, for RH7107:

```csharp
class C
{
    public int A
    {
        set
        {
#if DEBUG
            System.Console.WriteLine(value);
#endif
        }

        {|RH7107:get|}
        {
            return 0;
        }
    }
}
```

```csharp
class C
{
    public int A
    {
        get
        {
            return 0;
        }

        set
        {
#if DEBUG
            System.Console.WriteLine(value);
#endif
        }
    }
}
```

The same group in a crossed method's body refuses a member move (SHARED-ORDER-008, first example).

### SHARED-ORDER-011 — A warning or nullable pair that ends its own effect inside one declaration travels intact

A `#pragma warning` or `#nullable` directive in the moved span or in the crossed span MUST NOT refuse a move when all of these hold:

- **Inside one declaration.** The directive stands in the leading trivia of a token of one declaration of the list the move reorders — a member, or an accessor (SHARED-ORDER-001) — that lies wholly in the moved or the crossed span, and that token is not the declaration's first token. For RH7309, the declarations are the members of the type that lie wholly in one of its region blocks or in the code between two consecutive blocks — every span that RH7309-005 inspects, whether or not the reorder changes its place. A directive in the leading trivia of a declaration's first token is SHARED-ORDER-005's and still refuses the move. A member of a nested type is part of the nested type's declaration.
- **Pairs.** The directives of the declaration of the directive's kind — `#pragma warning` or `#nullable` — read in the order of the text, active ones and those in an excluded branch alike, form consecutive pairs, each an opening directive followed directly, among the declaration's directives of that kind, by its closing directive:
  - for `#pragma warning`, a `#pragma warning disable` followed by a `#pragma warning restore` that names the same diagnostic IDs in the same order, each written as in the `disable`, or that names none where the `disable` names none;
  - for `#nullable`, a `#nullable enable` or `#nullable disable` followed by a `#nullable restore` with the same target — `warnings`, `annotations`, or none.
- **One branch.** Every `#if`, `#elif`, `#else`, and `#endif` that lies between the two directives of a pair belongs to a conditional group that lies wholly between them.
- **Nothing open in front.** The directives of the pair's kind that stand in the document in front of the moved and the crossed span — in front of the target's first trivia, for RH7309 in front of the type's first region block — active ones and those in an excluded branch alike, form consecutive pairs as *Pairs* states, each of which keeps *One branch*. They need not stand inside one declaration: a closed pair in front, such as one inside an earlier member's body or around an earlier type, leaves the project's setting behind it. A directive of the kind in front that is not part of such a pair — a lone one, such as a file-level `#pragma warning disable CS1591`, whatever IDs it names, or one of a pair that a conditional branch splits — refuses the move.

Every other `#pragma warning` or `#nullable` directive in either span still refuses the move (SHARED-ORDER-006): one in a declaration's leading trivia, a lone one, one whose closing directive names other IDs or another target, a `#nullable enable` or `#nullable disable` closed by the opposite setting instead of `#nullable restore`, two opening directives in a row, a pair split across a conditional branch, and a pair with a directive of its kind in front of the spans that is not part of a closed pair there. Every directive that is neither a conditional, a region, a `#pragma warning`, nor a `#nullable` directive, such as `#line`, `#error`, `#warning`, or `#pragma checksum`, refuses the move wherever it stands in either span.

Why: `#pragma warning restore` and `#nullable restore` do not return to the setting in effect in front of the pair; they return to the project's setting. A pair therefore ends its own effect only where nothing of its kind is in effect in front of it, and moving code across a pair that does not changes which code the earlier directive governs (SHARED-FIX-003, *Comments and directives in place*): a member moved from behind the pair to in front of it falls under an earlier `#pragma warning disable` that the pair's `restore` ended for it. With every directive of the kind in front of the spans closed by its pair, the project's setting is in effect where the spans begin, so every declaration of the spans starts and ends at that setting, so a pair that opens and closes inside one declaration governs only that declaration's code and moves or stays with it, as a balanced conditional pair does (SHARED-ORDER-006). A pair split across a conditional branch, or an opening directive that only some builds close, leaves its effect behind in a build that compiles one directive without the other (SHARED-FIX-003, *Build configurations*); a directive in an excluded branch counts like an active one, which needs no evaluation of its condition (SHARED-FIX-009). Each condition is a check of where a directive stands and what it names, read from the syntax tree; comparing IDs as written refuses a pair spelled two ways, such as `168` and `CS0168`, which can only withhold the fix.

Example — a pair inside a crossed member's body, for RH7103:

```csharp
public class C
{
    public void Run()
    {
    }

    public void Reset()
    {
#pragma warning disable CS0168
        int unused;
#pragma warning restore CS0168
    }

    public static void {|RH7103:Create|}()
    {
    }
}
```

```csharp
public class C
{
    public static void Create()
    {
    }

    public void Run()
    {
    }

    public void Reset()
    {
#pragma warning disable CS0168
        int unused;
#pragma warning restore CS0168
    }
}
```

A `#nullable disable` closed by `#nullable restore` in the same place keeps the move as well; closed by `#nullable enable`, it refuses it.

Example — a lone directive inside a crossed member's body, for RH7103:

```csharp
public class C
{
    public void Reset()
    {
#pragma warning disable CS0168
        int unused;
    }

    public static void {|RH7103:Create|}()
    {
    }
}
```

is reported, and no code fix is offered: the `disable` governs `Create` as long as `Create` stands behind it. The same holds when the closing directive is `#pragma warning restore CS0219`.

Example — a directive of the pair's kind in front of the spans, for RH7103:

```csharp
#pragma warning disable CS1591
public class C
{
    public void Run()
    {
    }

    public void Reset()
    {
#pragma warning disable CS0168
        int unused;
#pragma warning restore CS0168
    }

    public static void {|RH7103:Create|}()
    {
    }
}
```

is reported, and no code fix is offered: a `#pragma warning` directive in front of the spans that no `restore` closes refuses the move, whichever IDs it names. With `#pragma warning disable CS0168` in that place, the move would be wrong: `Create` stands behind the `restore` and is not governed by that `disable`, and moved in front of `Run`, it would be. The same holds when the directive in front stands in an excluded branch.

Example — a closed pair in front of the spans, for RH7103:

```csharp
public class C
{
    public static void Log()
    {
#pragma warning disable CS0168
        int unused;
#pragma warning restore CS0168
    }

    public void Run()
    {
    }

    public void Reset()
    {
#pragma warning disable CS0168
        int unused;
#pragma warning restore CS0168
    }

    public static void {|RH7103:Create|}()
    {
    }
}
```

```csharp
public class C
{
    public static void Log()
    {
#pragma warning disable CS0168
        int unused;
#pragma warning restore CS0168
    }

    public static void Create()
    {
    }

    public void Run()
    {
    }

    public void Reset()
    {
#pragma warning disable CS0168
        int unused;
#pragma warning restore CS0168
    }
}
```

Example — a pair split across a conditional branch, for RH7107. Without `DEBUG` defined,

```csharp
class C
{
    public int A
    {
        set
        {
#pragma warning disable CS0168
            int unused;
#if DEBUG
#pragma warning restore CS0168
#endif
        }

        {|RH7107:get|}
        {
            return 0;
        }
    }
}
```

is reported, and no code fix is offered: a build without `DEBUG` leaves `CS0168` disabled up to `get`, and the move would take `get` out of it. The member reordering fixes refuse the same pair in a member's body already for its conditional directives (SHARED-ORDER-008), so for them *One branch* decides only through a pair in front of the spans. A conditional group that lies wholly between the two directives keeps the move, for RH7107:

```csharp
class C
{
    public int A
    {
        set
        {
#pragma warning disable CS0168
#if DEBUG
            int unused;
#endif
#pragma warning restore CS0168
        }

        {|RH7107:get|}
        {
            return 0;
        }
    }
}
```

```csharp
class C
{
    public int A
    {
        get
        {
            return 0;
        }

        set
        {
#pragma warning disable CS0168
#if DEBUG
            int unused;
#endif
#pragma warning restore CS0168
        }
    }
}
```

A pair whose directives stand in the leading trivia of two members, as in RH7102-006's example, is SHARED-ORDER-005's and refuses the move.

### SHARED-ORDER-007 — Initializer execution order is kept

A member reordering fix — RH7102, RH7103, or RH7109 — MUST NOT offer a move when the moved declaration has a runtime initializer and a member of the same context that the move crosses — both static or both instance — also has one. A `const` field has no runtime initializer.

A member reordering fix MUST NOT offer a move either when it would reorder the instance fields of a type whose field order sets its layout (SHARED-FIX-003, *Run time*), or of a type that SHARED-ORDER-008 counts as one whose field order sets its layout because of a conditional directive in a declaration of the type, in front of one, or in the reordered body in front of the moved and the crossed span. A move of a method, of a static member, or of a constant reorders no instance field.

The fix counts the members as the analyzed build compiles them. A conditional directive in the moved or the crossed span refuses the move (SHARED-ORDER-008), so no member of excluded code moves or is crossed, and the fix reads no excluded code to count initializers or fields.

An instance property counts as having a backing field, which counts as an instance field of the layout (SHARED-FIX-003, *Run time*), unless it is abstract or extern, or every accessor of its accessor list has a body or an expression body — or it has an expression body in place of an accessor list — and none of its tokens is the `field` keyword. One accessor without a body is enough: `get => 0; set;` counts just as `get; set;` does. The count reads the accessors of the declaration and nothing else, also for a partial property: the defining declaration of a partial property, whose accessors have no body, counts, and so does an implementing declaration with an accessor without a body or with `field`, although the compiler declares at most one backing field for both. An implementing declaration whose accessors all have a body or an expression body that does not use `field`, such as `get => 0; set { }`, does not count, and neither does an expression-bodied property that does not use `field`.

Every member reordering fix applies both guards. RH7102 moves only constants, which have no runtime initializer and take no place in the layout, so neither guard refuses its move. RH7309 counts initializers and instance fields the same way over the region blocks whose order its fix changes.

Why: Initializers run in declaration order within their context, the static ones in the static constructor and the instance ones in each instance constructor. Swapping two of them can change observable values silently. Static and instance initializers run at different times, so crossing a member of the other context is safe. Excluded code is compiled by another build, and counting its members would need it read as members; with every conditional directive in the spans refused, no build moves or crosses a member that the analyzed build does not see, so the active code is all there is to count. Whether the compiler declares a backing field for a property depends on how its declarations, accessors, and the `field` keyword combine; one condition on the shape of the accessors that counts too many backing fields rather than too few can only refuse a move that reorders layout fields, and it keeps the guard to one condition.

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

An accessor without a body gives the property a backing field also when another accessor of the list has a body, for RH7309:

```csharp
public struct S : System.IComparable
{
    #region IComparable

    public int Value
    {
        get => 0;
        set;
    }

    public int CompareTo(object obj)
    {
        return 0;
    }

    #endregion // IComparable

    {|RH7309:#region Fields|}

    private int _x;

    #endregion // Fields
}
```

is reported, and no code fix is offered: `set;` gives `Value` a backing field, and the reorder would put `_x` in front of it. When both accessors have a body or an expression body that does not use `field`, such as `get => 0;` and `set { }`, `Value` declares no backing field, and the fix moves the `Fields` block in front of the `IComparable` block.

A declaration of a partial property counts by its own accessors, for RH7309:

```csharp
public partial struct S
{
    public partial int Value { get => 0; set { } }
}

public partial struct S : System.IComparable
{
    #region IComparable

    public partial int Value { get; set; }

    public int CompareTo(object obj)
    {
        return 0;
    }

    #endregion // IComparable

    {|RH7309:#region Fields|}

    private int _x;

    #endregion // Fields
}
```

is reported, and no code fix is offered: the defining declaration's accessors have no body, so `Value` counts as having a backing field in front of `_x`, although its implementing declaration declares none. The same holds when the region block holds an implementing declaration with an accessor without a body, such as `public partial int Value { get => 0; set; }`, and the other part the defining declaration. When the region block holds the implementing declaration `public partial int Value { get => 0; set { } }` and the other part the defining declaration, the block's declaration does not count, and the fix moves the `Fields` block in front of the `IComparable` block.

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

### SHARED-ORDER-008 — A conditional directive in the moved or the crossed span refuses a member move

A member reordering fix — RH7102, RH7103, RH7109, or RH7309 — MUST NOT offer a move when the moved span or the crossed span (SHARED-ORDER-003) contains an `#if`, `#elif`, `#else`, or `#endif` directive, whatever its group holds and wherever in the span it stands: in a declaration's leading trivia, among its attribute lists or modifiers, in an accessor list, in an initializer, or in a body, also in the body of a nested type. For RH7309, the moved span and the crossed span are the region blocks whose place the fix changes and the code between blocks that they cross. A conditional directive behind both spans does not refuse the move; one in front of them refuses only a move that reorders instance fields, as the next paragraph states.

A member reordering fix MUST treat a type as one whose field order sets its layout (SHARED-ORDER-007), and so MUST NOT offer a move that would reorder the type's instance fields, when a conditional directive stands in one of these places:

- **In a declaration of the type, in front of its body.** In the leading trivia of a token of a declaration of the type that follows its first token, up to and including the opening brace, or, for a declaration that ends in `;` instead of a body, up to and including that `;` — among or behind its attribute lists, between its modifiers, or at its keyword, name, type parameter list, base list, or constraint clauses.
- **In front of a declaration of the type.** In the leading trivia of the first token of a declaration of the type, unless the directive belongs to a group that wraps the whole declaration: a group of an `#if` and its `#endif` with no `#elif` and no `#else`, whose `#if` stands in the leading trivia of the declaration's first token and whose `#endif` stands in the leading trivia of the token that follows the declaration's last token.
- **In the reordered body, in front of the spans.** In the body of the declaration whose members the fix reorders, between its opening brace and the start of the moved and the crossed span — for a member move, the start of the crossed span, at the target's first trivia (SHARED-ORDER-003); for RH7309, the start of the first region block whose place the fix changes or of the code between blocks that the reorder crosses. A group whose directives all stand there and a group with a directive there and its other directives behind the spans, so that the spans lie in one of its branches, count alike.

A declaration of the type is the declaration whose members the fix reorders, and, for a `partial` type, every other declaration of it that the analyzed build compiles, in the same document or in another, also one that ends in `;` instead of a body, such as `public partial class C;`; each is read on its own by the first two items. A move that reorders no instance field of such a type, such as a move of a method or of a static member, stays available as it is for any other type. A struct's field order sets its layout already (SHARED-FIX-003), so only moves that reorder the instance fields of a class or a record gain a refusal from this paragraph.

The fix decides all of this from where each conditional directive stands — the token whose leading trivia holds it — and, for the exception of the second item, which directives its group has; it reads no excluded branch, evaluates no condition, and combines no branches (SHARED-FIX-009). A conditional directive that none of these places holds is not read: one behind both spans, one in the body of another declaration of a `partial` type, one in a declaration of a type that encloses the reordered one, and one elsewhere in the document, such as a group around another type that does not reach the leading trivia of a declaration of the reordered type. A declaration of a `partial` type that only excluded code writes is therefore not read, and the move stays available, although a build that compiles that declaration can give the type a layout (SHARED-FIX-003, *Build configurations*). This is a decided gap, not an oversight: the analyzers analyze the build whose conditional compilation symbols the project defines as they stand, which is the common case, and this gap leaves such a move to be judged in that build; finding such a declaration would mean reading every excluded branch of every document, and the one refusal that needs no such reading, counting every `partial` type as one whose field order sets its layout, would withhold the move of instance fields on every partial type with fields, which is common code.

Why: A conditional directive in the spans can make a moved or a crossed member, or the type around them, another one in another build: a `static` or an initializer from one branch, a brace that one branch opens or closes, a backing field that one branch's accessor declares, or members that one branch adds between the moved and the crossed code. Telling a harmless group from such a one means reading each excluded branch as members, attribute lists, accessors, or statements — a parser-like reading of excluded text that a guard does not grow for, and that SHARED-FIX-009 allows only where it can only withhold. Refusing every conditional directive in the spans is one check of where a directive stands, and it can only withhold a fix; the cost is that a member whose body holds an `#if DEBUG` block, or that crosses one, is moved by hand, and a reported diagnostic without a fix is a supported end state. The declaration of the type in front of its body decides whether its field order sets its layout (SHARED-FIX-003, *Run time*), and the fix reads that from the analyzed build's symbol: a conditional `[StructLayout(LayoutKind.Sequential)]`, or a conditional `struct` in place of `class`, gives another build a layout that the analyzed build does not show. A group in front of the declaration can hold such an attribute list or modifier, or another declaration of a `partial` type with one, and a group in the body in front of the spans can close the type's body and open a struct whose fields the members of the spans become, or give a member in front of them a modifier; whether a branch does needs the branch read, so the type counts as one whose field order sets its layout. A group whose only branch is the whole declaration needs no reading: a build compiles the declaration as written or not at all. The members of the spans and their initializers keep their order relative to each other in every build, so a move that reorders no instance field stays available. A group behind the spans can change neither which type the members of the spans belong to nor their order. The attributes of every declaration of a `partial` type count for its layout, so a conditional directive in or in front of any of them counts as one in the reordered declaration does.

Example — a group of whole statements in a crossed method's body, for RH7103:

```csharp
public class C
{
    public void Run()
    {
#if DEBUG
        System.Console.WriteLine();
#endif
    }

    public static void {|RH7103:Create|}()
    {
    }
}
```

is reported, and no code fix is offered. The same holds for a group of whole members in front of `Create` and behind `Run`, which lies in the moved declaration's leading trivia:

```csharp
public class C
{
    public void Run()
    {
    }

#if DEBUG
    public void Trace()
    {
    }
#endif
    public static void {|RH7103:Create|}()
    {
    }
}
```

and for `#if false` in place of `#if DEBUG`, for a group of whole attribute lists in front of `Create`, such as `#if DEBUG` / `[System.Obsolete]` / `#endif`, for a group of whole accessors in a crossed property, such as `set { }` inside `#if DEBUG` … `#endif` behind `get;`, and for a group of whole members in the body of a crossed nested type.

Example — a group behind the spans keeps the move, for RH7103:

```csharp
public class C
{
    public void Run()
    {
    }

    public static void {|RH7103:Create|}()
    {
    }

#if DEBUG
    public void Trace()
    {
    }
#endif
}
```

```csharp
public class C
{
    public static void Create()
    {
    }

    public void Run()
    {
    }

#if DEBUG
    public void Trace()
    {
    }
#endif
}
```

Example — a member that an `#if` splits, for RH7109: the example of SHARED-FIX-009, where an `#if` inside the initializer of the crossed `_b` refuses the move.

Example — a conditional attribute list on the reordered type, for RH7109. Without `INTEROP` defined,

```csharp
using System.Runtime.InteropServices;

#if INTEROP
[StructLayout(LayoutKind.Sequential)]
#endif
public class C
{
    private int _a;
    private readonly int {|RH7109:_c|};
}
```

is reported, and no code fix is offered: in a build with `INTEROP`, the field order of `C` sets its layout, and moving `_c` in front of `_a` would change it. The same holds for an attribute that sets no layout, for RH7109. Without `DEBUG` defined,

```csharp
#if DEBUG
[System.Serializable]
#endif
public class C
{
    private int _a;
    private readonly int {|RH7109:_c|};
}
```

is reported, and no code fix is offered: the fix does not work out whether the attribute in the excluded branch sets a layout.

Example — a conditional attribute list on the reordered type keeps a move that reorders no instance field, for RH7103. Without `DEBUG` defined,

```csharp
#if DEBUG
[System.Serializable]
#endif
public class C
{
    public void Run()
    {
    }

    public static void {|RH7103:Create|}()
    {
    }
}
```

```csharp
#if DEBUG
[System.Serializable]
#endif
public class C
{
    public static void Create()
    {
    }

    public void Run()
    {
    }
}
```

The same holds for a conditional entry of the base list, such as `, System.IAsyncDisposable` inside `#if NET` … `#endif` behind `public sealed class C : System.IDisposable`: the fix moves a static method in front of an instance method.

Example — a whole type in an excluded branch in front of the reordered type, for RH7109. Without `DEBUG` defined,

```csharp
#if DEBUG
public class Trace
{
}
#endif
public class C
{
    private int _a;
    private readonly int {|RH7109:_b|};
}
```

is reported, and no code fix is offered: the `#endif` stands in the leading trivia of `public`, the first token of `C`'s declaration, and the fix does not read whether the branch holds a type of another name, an attribute list of `C`, or another declaration of `C`. The same holds for a group whose excluded branch holds another declaration of a `partial` type `C` with an attribute list, such as `[StructLayout(LayoutKind.Sequential)]` / `public partial class C { }`.

Example — a group that wraps the whole reordered declaration keeps a move that reorders instance fields, for RH7109. Without `DEBUG` defined,

```csharp
namespace N
{
#if !DEBUG
    public class C
    {
        private int _a;
        private readonly int {|RH7109:_b|};
    }
#endif
}
```

```csharp
namespace N
{
#if !DEBUG
    public class C
    {
        private readonly int _b;
        private int _a;
    }
#endif
}
```

With an `#else` branch, such as `#else` / `public class C { }` in front of the `#endif`, the same input is reported, and no code fix is offered.

Example — a conditional attribute list on another declaration of a `partial` type, for RH7109. Without `NET` defined,

```csharp
using System.Runtime.InteropServices;

#if NET
[StructLayout(LayoutKind.Sequential)]
#endif
public partial class C
{
}

public partial class C
{
    private int _a;
    private readonly int {|RH7109:_c|};

    public C(int c)
    {
        _c = c;
    }
}
```

is reported, and no code fix is offered: in a build with `NET`, the field order of `C` sets its layout, although the declaration whose members the fix reorders holds no directive. The same holds when `NET` is defined, where the `#endif` stands in the leading trivia of `public`, behind the declaration's first token, and for a struct whose other declaration carries `[StructLayout(LayoutKind.Auto)]` inside `#if !X` … `#endif`: the analyzed build sees automatic layout, and a build with `X` lays the struct out in field order. Without a conditional directive in any declaration of `C`, the fix moves `_c` in front of `_a`:

```csharp
public partial class C
{
}

public partial class C
{
    private int _a;
    private readonly int {|RH7109:_c|};
}
```

```csharp
public partial class C
{
}

public partial class C
{
    private readonly int _c;
    private int _a;
}
```

Example — a conditional attribute list on another declaration of a `partial` type that ends in `;` instead of a body, for RH7109. Without `NET` defined,

```csharp
using System.Runtime.InteropServices;

#if NET
[StructLayout(LayoutKind.Sequential)]
#endif
public partial class C;

public partial class C
{
    private int _a;
    private readonly int {|RH7109:_c|};
}
```

is reported, and no code fix is offered: the declaration `public partial class C;` is read like one with a body, and in a build with `NET` the field order of `C` sets its layout. The same holds for a `partial` struct, record, or record struct declared with `;`, and for a conditional directive behind the declaration's first token, such as in its base list or in front of its `;`. Without a conditional directive in any declaration of `C`, the fix moves `_c` in front of `_a`, as the example above shows for a declaration with a body.

Example — a group in the body in front of the spans, for RH7109:

```csharp
public class C
{
#if DEBUG
    private int _trace;
#endif
    public void Run()
    {
    }

    private int _a;
    private readonly int {|RH7109:_b|};
}
```

is reported, and no code fix is offered, whether or not `DEBUG` is defined. The same group keeps a move that reorders no instance field, for RH7103:

```csharp
public class C
{
#if DEBUG
    private int _trace;
#endif
    public void Run()
    {
    }

    public static void {|RH7103:Create|}()
    {
    }
}
```

```csharp
public class C
{
#if DEBUG
    private int _trace;
#endif
    public static void Create()
    {
    }

    public void Run()
    {
    }
}
```

### SHARED-ORDER-009 — A move moves whole lines

A fix MUST NOT offer a move unless it moves whole lines: no token outside the moved declaration stands on a line that holds a token of it, and no token stands in front of the target declaration on the line of its first token. A declaration that shares a line with another declaration, or with a brace of the type or the accessor list, is such a token. The leading trivia of the moved declaration and of the target MUST start their line as well: no token stands in front of the first comment, documentation comment, or directive of that leading trivia on its line. A `///` comment written behind code, which documents the declaration below it when the documentation mode is `Parse` or `Diagnose` (SHARED-DOC-004), is leading trivia that does not start its line; a move of the declaration it documents, or a move in front of that declaration, is not offered. Declarations that the move crosses may share lines with each other: the declarations on one line keep that line, as written, and take the position of the first of them as one block behind the moved declaration. For an accessor move, this holds for an accessor list whose opening and closing brace stand on different lines. When the whole accessor list stands on one line — its opening and its closing brace on the same line — this requirement does not refuse the move, and the fix MUST perform it as a swap within that line: the moved accessor takes the place of the target and the target the place behind it, and the whitespace between the braces and the accessors stays at its position (SHARED-ORDER-002); no line breaks, joins, or moves, so the column shift of SHARED-FIX-007 does not apply. The swap within the line MUST NOT be offered when a comment stands in the accessor list outside every accessor: in front of the first accessor, between two accessors, or behind the last accessor in front of the closing brace. A comment inside an accessor — in its body, its expression body, or its attribute list — does not withhold the swap; it travels with its accessor. Where the move is refused, the diagnostic is still reported. A comment behind the moved declaration on its last line, and the comments and attribute lists in its leading trivia, are part of the moved declaration and travel with it (SHARED-ORDER-002).

Why: A move of whole lines keeps every line as the author wrote it and needs no decision about where a line breaks; the moved declaration only changes its position (SHARED-FIX-007). A `///` comment behind code belongs to the declaration below it, so a move carries it with that declaration (SHARED-DOC-004); at its new position it would need a line of its own, a line break the author did not write, and leaving it behind would hand the documentation to another declaration (SHARED-FIX-003). Such a comment is reported by RH8303. The crossed declarations do not move relative to each other, so a line they share decides no line break. Taking a declaration out of a line it shares, or putting it into a line in front of another token, would decide a line break the author did not write, which is layout the diagnostic does not name (SHARED-FIX-006). Declarations that share a line are reported by the layout rules on their own. An accessor list on one line is the common shape of a short property, and swapping its accessors within the line decides no line break, so it keeps its fix. On one line, a comment between or behind the accessors belongs to no accessor in the syntax tree: it is trailing trivia of the token in front of it, so a swap of the accessors would leave it behind and hand it to the other accessor, and deciding which accessor it describes would need a rule the line does not state. Such a list is rare, and the check is cheap, so the swap is withheld for it.

Example — the moved field shares its line with the field in front of it, for RH7109:

```csharp
class C
{
    private int _a; private readonly int {|RH7109:_b|};
}
```

is reported, and no code fix is offered.

Example — the target shares its line with the type's opening brace, for RH7109:

```csharp
class C
{   private int _a;
    private readonly int {|RH7109:_b|};
}
```

is reported, and no code fix is offered.

Example — the comment behind the constant documents the target `_a`, for RH7109. With the documentation mode `Parse` or `Diagnose`,

```csharp
class C
{
    private const int K = 1; /// <summary>A</summary>
    private int _a;
    private readonly int {|RH7109:_b|};
}
```

is reported, and no code fix is offered. The same holds for RH7103 where the comment behind code documents the moved declaration (SHARED-DOC-004).

Example — the crossed fields `_a` and `_c` share a line; they keep it and move behind the moved field as one block, for RH7109:

```csharp
class C
{
    private int _a; private int _c;
    private readonly int {|RH7109:_b|};
}
```

```csharp
class C
{
    private readonly int _b;
    private int _a; private int _c;
}
```

Example — the moved accessor shares its line with another accessor in a list that spans lines, for RH7107:

```csharp
class C
{
    public int A
    {
        set; {|RH7107:get|};
    }
}
```

is reported, and no code fix is offered.

Example — the whole accessor list stands on one line, for RH7107; the accessors swap places within the line:

```csharp
class C
{
    private int _b;

    public int B { set => _b = value; {|RH7107:get|} => _b; }
}
```

```csharp
class C
{
    private int _b;

    public int B { get => _b; set => _b = value; }
}
```

Example — a comment stands between the accessors of a list on one line, for RH7107:

```csharp
class C
{
    public int A { set { } /* s */ {|RH7107:get|} => 1; }
}
```

is reported, and no code fix is offered. The same holds for a comment behind the last accessor, as in `{ set { } get => 1; /* g */ }`, and for one in front of the first accessor, as in `{ /* c */ set { } get => 1; }`.

Example — a comment inside an accessor travels with it, for RH7107:

```csharp
class C
{
    public int A { set { /* s */ } {|RH7107:get|} => 1; }
}
```

```csharp
class C
{
    public int A { get => 1; set { /* s */ } }
}
```

Example — whole lines move; the comment behind the moved field moves with it, for RH7109:

```csharp
class C
{
    private int _a;
    private readonly int {|RH7109:_b|}; // cached
}
```

```csharp
class C
{
    private readonly int _b; // cached
    private int _a;
}
```

### SHARED-ORDER-010 — A shifted declaration holds no line that cannot move

When a move changes the column of the moved declaration's first token (SHARED-FIX-007, *Moved code*), the fix MUST NOT be offered if the moved declaration, with its leading trivia, holds a string literal of any kind — regular, verbatim, interpolated, or raw — that spans lines, a comment that spans lines, or disabled text. An interpolated string counts in every `$` form, from its first character through its closing quote, its holes included: it spans lines when any part of it does, also where only a hole spans lines, and also where every later line starts at a hole's `{` or at the closing quote rather than inside the string's text. It MUST NOT be offered either when the move shifts the declaration to the left and one of its lines that moves (SHARED-FIX-007, *Moved code*) holds fewer whitespace characters in front of its first character than the shift removes, a tab counted as one. The diagnostic is still reported. A move that keeps the column of the first token moves no line, and is offered for such a declaration.

Why: Every other line of a shifted declaration moves with its first token, but the whitespace at the start of a line inside a verbatim or interpolated string is part of its value, the lines of a raw string literal are bound to its closing quotes, the later lines of a delimited comment are its text, and disabled text is code the compiler does not read: none of them can move as a plain line without changing content (SHARED-FIX-003) or without a rule of its own. An interpolated string is several tokens, so a later line of it can start at a hole or at the closing quote and still be part of the string's value; deciding on the string as a whole, not on where its lines start, keeps every such line out of the shift. A line written further left than the shift reaches cannot move by it without being cut off at column 0, which would change its column relative to the other lines. A declaration indented differently from the slot it moves into is reported by the indentation rule, so this class is rare, and a reported diagnostic without a fix is a supported end state.

Example — a field indented deeper than its slot holds a verbatim string over two lines, for RH7109:

```csharp
class C
{
    private int _a;
        private readonly string {|RH7109:_b|} = @"x
y";
}
```

is reported, and no code fix is offered. Indented as its slot, the same field moves:

```csharp
class C
{
    private int _a;
    private readonly string {|RH7109:_b|} = @"x
y";
}
```

```csharp
class C
{
    private readonly string _b = @"x
y";
    private int _a;
}
```

Example — a field indented deeper than its slot holds a comment line further left than the shift reaches, for RH7109:

```csharp
class C
{
    private int _a;
        private readonly int {|RH7109:_b|} =
  // two columns
            1;
}
```

is reported, and no code fix is offered: the shift of four columns to the left would cut the comment line off at column 0.

Example — a field indented less deeply than its slot holds an interpolated verbatim string whose second line starts at a hole, for RH7109:

```csharp
class C
{
    private int _a;
  private readonly string {|RH7109:_b|} = $@"a
{nameof(C)}";
}
```

is reported, and no code fix is offered: the shift of two columns to the right would write two spaces in front of `{nameof(C)}` into the value of `_b`. The same holds where the second line starts at the closing quote, and for an interpolated string whose hole spans lines.

# Shared — Ordering move safety

| | |
|---|---|
| ID prefix | SHARED-ORDER |
| Status | Active |
| Code owners | `OrderingMoveSafety`, `OrderingDeclarationUtilities`, `AccessorOrderingUtilities`, `TypeMemberOrderingCodeFixProviderBase` |
| Uses | RH7309-005, SHARED-FIX-003, SHARED-FIX-009, SHARED-TYPEREGION-006 |

This spec covers how the code fixes of the ordering rules move one declaration in front of an earlier sibling, and when such a move is refused. It applies to the member reordering fixes (RH7102 constants before fields, RH7103 static before instance, RH7109 readonly before non-readonly) and to the accessor reordering fixes (RH7107 property accessors, RH7108 event accessors). SHARED-ORDER-007 and SHARED-ORDER-008 also apply to the RH7309 fix, which reorders region blocks. It also states the region scope that every rule ordering the members of a type shares (SHARED-ORDER-004), RH7110 included, which offers no fix. Apart from that scope, which declaration a rule reports and where it should go is each rule's own spec; this spec owns the move and its guards.

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

A move MUST stay available when every conditional and region directive in the moved span and in the crossed span has its partner in the same span, neither span contains a directive of any other kind, and, for a member reordering fix, SHARED-ORDER-007 and SHARED-ORDER-008 do not refuse it.

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

A member reordering fix — RH7102, RH7103, or RH7109 — MUST NOT offer a move when a member that the move moves has a runtime initializer and a member of the same context that the move crosses — both static or both instance — also has one. A `const` field has no runtime initializer. A member that an excluded branch of a group that wraps whole members of the type (SHARED-ORDER-008) declares counts like any other member, because a build that includes it runs its initializer (SHARED-FIX-003, *Build configurations*):

- a member of excluded code in the crossed span (SHARED-ORDER-003) is crossed;
- a member of excluded code in the moved declaration's own leading trivia moves, because that trivia travels with the declaration (SHARED-ORDER-002), and it crosses every member the declaration crosses. The move is refused when such a member has a runtime initializer and a crossed member of its context has one, also when the moved declaration itself has none.

The moved members are the moved declaration and the members of excluded code in its leading trivia; no other member moves.

A member reordering fix MUST NOT offer a move either when it would reorder the instance fields of a type whose field order sets its layout (SHARED-FIX-003, *Run time*), or of a type that SHARED-ORDER-008 counts as one whose field order sets its layout because of a conditional directive in a declaration of the type or in front of the moved and the crossed span, and it counts the instance fields the same way: an instance field of excluded code in the moved declaration's leading trivia moves across every instance field the move crosses, and an instance field of excluded code in the crossed span is crossed. A move of a static member therefore reorders instance fields when excluded code in its leading trivia declares one and the move crosses an instance field.

Each member of excluded code counts as present at its position, in every build, whether or not a build compiles it together with the other members; the fix evaluates no condition and combines no branches (SHARED-FIX-009). Two members in branches that exclude each other, such as `#if DEBUG` and `#else`, both count. A group of whole members in the body of a nested type declares members of the nested type, which take no part in the move (SHARED-ORDER-001), and is not counted. A group of whole attribute lists, of whole accessors, or of whole statements declares no member and is not counted; its member counts as the active code declares it. A group of whole accessors that could decide whether the property has a backing field refuses the move instead (SHARED-ORDER-008, *No backing field from a branch*). Every other conditional directive in the moved span or the crossed span refuses the move under SHARED-ORDER-008, so a member that a conditional directive splits is never counted in parts.

An instance property counts as having a backing field, which counts as an instance field of the layout (SHARED-FIX-003, *Run time*), unless it is abstract or extern, or every accessor of its accessor list has a body or an expression body — or it has an expression body in place of an accessor list — and none of its tokens is the `field` keyword. One accessor without a body is enough: `get => 0; set;` counts just as `get; set;` does. The count reads the accessors of the declaration and nothing else, also for a partial property: the defining declaration of a partial property, whose accessors have no body, counts, and so does an implementing declaration with an accessor without a body or with `field`, although the compiler declares at most one backing field for both. An implementing declaration whose accessors all have a body or an expression body that does not use `field`, such as `get => 0; set { }`, does not count, and neither does an expression-bodied property that does not use `field`.

Every member reordering fix applies both guards. RH7102 moves only constants: a constant has no runtime initializer and takes no place in the layout, but the excluded code in its leading trivia moves with it. RH7309 counts initializers and instance fields the same way over the region blocks whose order its fix changes.

Why: Initializers run in declaration order within their context, the static ones in the static constructor and the instance ones in each instance constructor. Swapping two of them can change observable values silently. Static and instance initializers run at different times, so crossing a member of the other context is safe. Excluded code is compiled by another build, and the code in front of the moved declaration goes where the declaration goes, so in that build its members are moved like the declaration itself; refusing only for the declaration's own initializer would let the fix reorder that build's initializers or fields. Counting every whole member as present can only refuse a move, so the fix needs no knowledge of which builds compile which branch. Whether the compiler declares a backing field for a property depends on how its declarations, accessors, and the `field` keyword combine; one condition on the shape of the accessors that counts too many backing fields rather than too few can only refuse a move that reorders layout fields, and it keeps the guard to one condition.

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

Members in branches that exclude each other both count. Without `DEBUG` defined,

```csharp
public class C
{
    private int _a;
#if DEBUG
    private int _c = Log("c");
#else
    private int _e;
#endif
    private readonly int {|RH7109:_b|} = Log("b");

    private static int Log(string name)
    {
        return 0;
    }
}
```

is reported by RH7109, and no code fix is offered: `_c` is crossed, although the build without `DEBUG` compiles `_e` in its place.

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

### SHARED-ORDER-008 — Only conditional groups of whole units keep a member move

A member reordering fix — RH7102, RH7103, RH7109, or RH7309 — MUST NOT offer a move when the moved span or the crossed span (SHARED-ORDER-003) contains an `#if`, `#elif`, `#else`, or `#endif` directive of a conditional group that does not wrap whole units. For RH7309, the moved span and the crossed span are the region blocks whose place the fix changes and the code between blocks that they cross. A conditional group is an `#if` together with its `#elif` and `#else` directives and its `#endif`, and its branches are the code or disabled text between two consecutive directives of the group.

A conditional group wraps whole units when all of these hold:

- **Place.** Every directive of the group stands at a boundary of one unit list: in the leading trivia of the first token of a unit of that list, or of the token that follows the list's last unit. The unit lists are:
  - the members of one type body — of the type the fix reorders, or of a nested type in the moved or the crossed span or in front of them; the token that follows the last member is the body's closing brace;
  - the attribute lists of one member of such a type body; the token that follows the last attribute list is the member's first token behind its attributes;
  - the accessors of one accessor list of such a member; the token that follows the last accessor is the accessor list's closing brace;
  - the statements of one block or one switch section inside the body block of a method, constructor, destructor, operator, conversion operator, or accessor of such a member, the blocks nested in it included, also those of a lambda or a local function; the token that follows the last statement is the block's closing brace, or, for a switch section, the first token of the next section or the switch statement's closing brace.
- **No nesting.** No other conditional group lies inside one of its branches, and it lies inside no branch of another conditional group.
- **Whole units in every branch.** The active branch, if there is one, holds only whole units of that list. The text of each excluded branch, parsed on its own as the content of such a list, reads as a sequence of whole units of that list without a syntax error: each unit it begins also ends in it, and it closes no brace that it did not open. A branch that holds nothing but whitespace, comments, or `#region` and `#endregion` lines is a sequence of no units.
- **No `field`.** When the list is the accessors of a property, or the statements of a block inside an accessor's body, no branch, active or excluded, contains `field` as a token.
- **No backing field from a branch.** When the list is the accessors of a property, no branch, active or excluded, holds an accessor without a body and without an expression body, unless no accessor of the list — in any branch of the group or outside it — has a body or an expression body. A group whose accessors and the accessors around it all lack a body, such as `set;` inside `#if DEBUG` … `#endif` behind `get;`, gives the property a backing field in every build, which SHARED-ORDER-007 counts from the active code. A group whose branches hold no accessor without a body, such as `set { }` inside `#if DEBUG` … `#endif` behind `get;`, is not affected by this clause.

The leading trivia of a member's first token is a boundary of two lists: of the type body's members, and of the member's attribute lists, which that token begins or, for a member without attributes, follows. A group whose directives stand only there wraps whole units when it does so for one of the two lists: every excluded branch reads as whole members, or every excluded branch reads as whole attribute lists of the member behind the group.

Every other conditional directive in either span refuses the move, among them:

- a directive between two tokens of a member declaration that is no boundary of one of the lists above: between two modifiers, in front of the member's type or name, in its initializer, its expression body, its parameter list, its constructor initializer, or its base list, or inside a statement, such as between two arguments of a call;
- a group whose excluded branch holds something other than whole units of the list at whose boundary it stands — such as `static` in front of a member, which makes the member static in a build that compiles it, or text that is not code;
- a group inside a branch of another group, also when both groups would wrap whole units on their own;
- a group with a branch that begins or ends a member declaration, a type body, or a block — that ends the type's body and opens another type, or opens a nested type that a later group closes;
- a group of statements in a lambda or an anonymous method inside a field initializer, a property initializer, or an expression body, which is no body block of a member;
- a group of whole accessors, or of whole statements inside an accessor's body, with a branch that contains `field`;
- a group of whole accessors with a branch that holds an accessor without a body and without an expression body, while an accessor of the list has a body or an expression body — such as `set;` inside `#if DEBUG` … `#endif` behind `get => 0;`, which gives the property a backing field only in a build with `DEBUG`.

The fix decides each group by a small structural test of where its directives stand, and by nothing else (SHARED-FIX-009): for each directive, the token whose leading trivia holds it and the unit list at whose boundary that token stands; whether a directive of another group lies between the group's directives; and, for each excluded branch, one parse of its text on its own as the content of that list, checked for whole units, for a syntax error, and, where the list is an accessor list or the statements of an accessor's body, for a `field` token and for an accessor without a body. For a branch in a type body outside every member's own tokens, that parse is the one with which the guard already finds the members that SHARED-ORDER-007 counts, unchanged; for a branch inside a member, it is the same parse with the text placed into an empty list of the unit's kind. The fix evaluates no condition, combines no branches of one or of several groups, parses no build configuration, and has no limit on the number of configurations. A group that this test does not classify as wrapping whole units refuses the move. A conditional directive outside both spans does not refuse the move. A conditional directive behind both spans, or outside every declaration of the reordered type, is not read; one in a declaration of the type in front of its body, or in the body of the reordered declaration in front of the moved and the crossed span, is read as the next paragraph states. Balanced pairs that this requirement keeps travel as SHARED-ORDER-006 states.

A member reordering fix MUST treat a type as one whose field order sets its layout (SHARED-ORDER-007) when a declaration of the type holds a conditional directive in front of its body's opening brace, or when the body of the declaration whose members the fix reorders holds one in front of the moved and the crossed span, as stated below: it MUST NOT offer a move that would reorder the type's instance fields, counted as SHARED-ORDER-007 counts them. A declaration of the type is the declaration whose members the fix reorders, and, for a `partial` type, every other declaration of it, in the same document or in another; each is read on its own, by the same test. The directive counts when it stands

- in the leading trivia of a token of the declaration that follows its first token, up to and including the opening brace — among or behind its attribute lists, between its modifiers, or at its keyword, name, type parameter list, base list, or constraint clauses;
- in the leading trivia of the declaration's first token, when the directive belongs to a group with an excluded branch that does not read as whole members (*Whole units in every branch*), such as a branch that holds attribute lists, a modifier, or the keyword of the type;
- in the body of the declaration whose members the fix reorders, between its opening brace and the start of the moved and the crossed span — for a member move, the start of the crossed span, at the target's first trivia (SHARED-ORDER-003); for RH7309, the start of the first region block whose place the fix changes or of the code between blocks that the reorder crosses — when the directive belongs to a group that does not wrap whole units under the test above, such as a group whose branch closes the type's body and opens another type, or holds a modifier of a member in front of the spans. A group of whole members, of whole attribute lists, of whole accessors, or of whole statements in front of the spans does not count.

This holds for a group whose directives all stand in front of the spans, and for a group with a directive in front of the spans and its other directives behind them, so that the spans lie in one of its branches. A group with a directive in the moved or the crossed span is decided by the paragraphs above.

A move that reorders no instance field of such a type, such as a move of a method or of a static member, stays available as it is for any other type. A group in the leading trivia of the declaration's first token whose excluded branches all read as whole members, such as a whole type declared in front of the reordered one, does not make the type count as one whose field order sets its layout. The test is the same structural test as above: where the directive stands, and the one parse of each excluded branch. A conditional directive in a declaration of a type that encloses the reordered one does not count, and neither does one in the body of another declaration of a `partial` type.

Why: A group of whole members adds or removes members and changes nothing else, so counting every member it declares as present (SHARED-ORDER-007) covers every build without working one out; the members of a nested type take no part in the move at all. A group of whole attribute lists or of whole accessors changes the member it belongs to in every build alike: the attributes and accessors travel with their member, and none of them runs an initializer or orders a field, except an accessor without a body or the `field` keyword, either of which declares a backing field; a group that would declare one in some builds only is refused, because which builds have the field is a question of conditions. A group of whole statements inside a body changes what the body does, which no move of the member changes, and no initializer, context, or layout field of the type depends on it, again except for `field` in an accessor's body. Every other directive makes a member, a type, or a brace depend on the build: a `static`, an initializer, or a closing brace from one branch makes the member or the type around the moved code another one in each build. Telling those builds apart means evaluating conditions and combining branches, which no fix does (SHARED-FIX-009); the user decides by hand. Deciding from where the directives stand keeps the test small, and a group it cannot classify is refused, which can only withhold a fix. The declaration of the type in front of its body decides whether its field order sets its layout (SHARED-FIX-003, *Run time*), and the fix reads that from the analyzed build's symbol: a conditional `[StructLayout(LayoutKind.Sequential)]`, or a conditional `struct` in place of `class`, gives another build a layout that the analyzed build does not show. Counting such a type as one whose field order sets its layout is one check of where the directive stands, and it refuses only the moves that a layout could make unsafe; the order of initializers, which SHARED-ORDER-007 checks for every type, does not depend on the declaration, and a move that reorders no instance field changes no layout in any build. A whole type in an excluded branch in front of the declaration changes nothing of it. The attributes of every declaration of a `partial` type count for its layout, so a conditional directive in any of them counts as one in the reordered declaration does. A group in the body in front of the moved and the crossed span whose branch does not hold whole units can make the members of the spans belong to another type in another build — a branch that closes the type's body and opens a struct makes them that struct's fields — or make that type another one; which type and which layout would have to be worked out from the branches, so the type counts as one whose field order sets its layout. The members of the spans and their initializers keep their order relative to each other in every build, so the move of a member that reorders no instance field stays available, as for a conditional directive in the type's declaration. A group behind the spans can change neither which type the members of the spans belong to nor their order.

Example — a group of whole members in the moved member's leading trivia keeps the move, for RH7103:

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

```csharp
public class C
{
#if DEBUG
    public void Trace()
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

The group is the leading trivia of `Create` and moves with it (SHARED-ORDER-002).

Example — a group of whole members in the body of a crossed nested type keeps the move, for RH7109:

```csharp
public class C
{
    private int _a;

    private class N
    {
#if DEBUG
        private int _d;
#endif
    }

    private readonly int {|RH7109:_c|};
}
```

is reported, and the fix moves `_c` in front of `_a`, as it would without the group: `_d` is a member of `N`, which takes no part in the move.

Example — a group of whole attribute lists on the moved member keeps the move, for RH7109. Without `DEBUG` defined,

```csharp
public class C
{
    private int _a;
#if DEBUG
    [System.Obsolete]
#else
    [System.CLSCompliant(false)]
#endif
    private readonly int {|RH7109:_c|};
}
```

```csharp
public class C
{
#if DEBUG
    [System.Obsolete]
#else
    [System.CLSCompliant(false)]
#endif
    private readonly int _c;
    private int _a;
}
```

The same holds for a group of whole attribute lists in front of a crossed member.

Example — a group of whole accessors in a crossed property keeps the move, for RH7103:

```csharp
public class C
{
    public int Value
    {
        get;
#if DEBUG
        set;
#endif
    }

    public static int {|RH7103:Shared|} => 1;
}
```

```csharp
public class C
{
    public static int Shared => 1;

    public int Value
    {
        get;
#if DEBUG
        set;
#endif
    }
}
```

Example — a group of whole statements in a crossed method keeps the move: the first example of SHARED-ORDER-006 for RH7103, and the second example of RH7309-005 for RH7309. The statements of a switch section count like those of a block, for RH7103:

```csharp
public class C
{
    public int Run(int value)
    {
        switch (value)
        {
            case 1:
#if DEBUG
                System.Console.WriteLine();
#endif
                return 1;
            default:
                return 0;
        }
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

    public int Run(int value)
    {
        switch (value)
        {
            case 1:
#if DEBUG
                System.Console.WriteLine();
#endif
                return 1;
            default:
                return 0;
        }
    }
}
```

Example — an `#if` that splits a member's initializer, for RH7109: the example of SHARED-FIX-009, where `_b` is crossed and no code fix is offered.

Example — an `#if` that splits a member's modifiers, for RH7109. Without `DEBUG` defined,

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

is reported, and no code fix is offered: `K` is a static member with an initializer in one build and a constant in the other, and the fix does not work out which.

Example — an excluded branch in front of the moved member that holds a modifier, for RH7109. Without `DEBUG` defined,

```csharp
public class C
{
    private int _a;
#if DEBUG
    static
#endif
    private readonly int {|RH7109:_c|};
}
```

is reported, and no code fix is offered: the excluded text `static` is neither whole members nor whole attribute lists, and a build with `DEBUG` compiles `_c` as a static field.

Example — a member that an `#if` splits inside a region block, for RH7309. Without `DEBUG` defined,

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

is reported, and no code fix is offered: the `#endif` stands inside the declaration of `Y`, and the excluded branch begins a declaration that it does not end.

Example — a nested group, for RH7109. Without `FEATURE` defined,

```csharp
public class C
{
    private int _a;
#if FEATURE
#if DEBUG
    private int _d;
#endif
#endif
    private readonly int {|RH7109:_b|};
}
```

is reported, and no code fix is offered: the `#if DEBUG` group lies inside the `#if FEATURE` group.

Example — a branch that ends the type's body, for RH7109. Without `DEBUG` defined,

```csharp
public class C
{
    private static int _t;
#if DEBUG
}

public class D
{
    private static int _z = Log("z");
#endif
    private int _d;
    private static readonly int {|RH7109:_c|} = Log("c");

    private static int Log(string name)
    {
        return 0;
    }
}
```

is reported, and no code fix is offered: the excluded branch closes a brace it did not open. The same holds for a group that opens a nested type around the moved member, which a later group closes:

```csharp
public class C
{
    private static int _t;
#if DEBUG
    private class N
    {
#endif
        private int _d;
        private static readonly int {|RH7109:_c|} = Log("c");

        private static int Log(string name)
        {
            return 0;
        }
#if DEBUG
    }
#endif
}
```

is reported by RH7109, and no code fix is offered.

Example — a conditional argument inside a statement of a crossed method, for RH7103. Without `DEBUG` defined,

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

    public static void {|RH7103:Create|}()
    {
    }
}
```

is reported, and no code fix is offered: the directives stand between two tokens of one statement.

Example — whole statements in a lambda inside a crossed field initializer, for RH7109. Without `DEBUG` defined,

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

    private readonly int {|RH7109:_c|};
}
```

is reported, and no code fix is offered: the lambda lies in an initializer, not in the body block of a member.

Example — a group in an accessor's body that uses the `field` keyword, for RH7309. Without `TRACE` defined,

```csharp
public struct S : System.IComparable
{
    #region IComparable

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

is reported, and no code fix is offered: in a build with `TRACE`, `Value` has a backing field, and the reorder would put `_x` in front of it.

The `field` clause withholds the fix also where no build would change, for RH7103. Without `TRACE` defined,

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

    public static int {|RH7103:Shared|} => 1;
}
```

is reported, and no code fix is offered, although moving the static `Shared` reorders no instance field in any build: the fix does not work out what the branch that uses `field` declares. With `return 1;` in place of `return field;`, the group wraps whole statements, and the fix moves `Shared` in front of `Value`.

Example — a group of whole accessors that gives the property a backing field in some builds only, for RH7309. Without `DEBUG` defined,

```csharp
public struct S : System.IComparable
{
    #region IComparable

    public int Value
    {
#if DEBUG
        get;
#else
        get => 0;
#endif
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

is reported, and no code fix is offered: in a build with `DEBUG`, `Value` is an auto-property whose backing field the reorder would put behind `_x`. The same group withholds the fix where no build would change, for RH7103. Without `DEBUG` defined,

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

    public static int {|RH7103:Shared|} => 1;
}
```

is reported, and no code fix is offered, although moving the static `Shared` reorders no instance field in any build: `set;` gives `Value` a backing field only in a build with `DEBUG`. With `set { }` in place of `set;`, no accessor lacks a body, and the fix moves `Shared` in front of `Value`. With `get;` in place of `get => 0;`, every accessor lacks a body, and the fix moves `Shared` as in the example of a group of whole accessors above.

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

is reported, and no code fix is offered: the excluded branch holds an attribute list of `C`, not whole members, and the fix does not work out whether that attribute sets a layout.

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

Example — a whole type in an excluded branch in front of the reordered type keeps a move that reorders instance fields, for RH7109. Without `DEBUG` defined,

```csharp
#if DEBUG
public class D
{
}
#endif

public class C
{
    private int _a;
    private readonly int {|RH7109:_c|};
}
```

```csharp
#if DEBUG
public class D
{
}
#endif

public class C
{
    private readonly int _c;
    private int _a;
}
```

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

Example — a group in front of the spans whose branch closes the type's body, for RH7109. Without `X` defined,

```csharp
public class C
{
#if X
}

public struct D
{
#endif
    private readonly int _y;
    private int _a;
    private readonly int {|RH7109:_c|};
}
```

is reported, and no code fix is offered: the group stands in the leading trivia of `_y`, in front of the crossed span, which begins at `_a`, and its excluded branch closes a brace it did not open. In a build with `X`, the three fields are the fields of the struct `D`, and moving `_c` in front of `_a` would change its layout. The same group keeps a move that reorders no instance field, for RH7103. Without `X` defined,

```csharp
public class C
{
#if X
}

public struct D
{
#endif
    private static int _y;
    private int _a;
    private static int {|RH7103:_c|};
}
```

```csharp
public class C
{
#if X
}

public struct D
{
#endif
    private static int _y;
    private static int _c;
    private int _a;
}
```

Example — a group in front of the spans that holds a modifier of a member in front of them, for RH7109. Without `DEBUG` defined,

```csharp
public class C
{
#if DEBUG
    static
#endif
    private readonly int _q;
    private int _a;
    private readonly int {|RH7109:_c|};
}
```

is reported, and no code fix is offered: the excluded branch `static` is neither whole members nor whole attribute lists, and the fix does not work out what it makes of the type in a build with `DEBUG`.

Example — a group of whole members in front of the spans keeps a move that reorders instance fields, for RH7109. Without `X` defined,

```csharp
public class C
{
#if X
    private int _t;
#endif
    private readonly int _y;
    private int _a;
    private readonly int {|RH7109:_c|};
}
```

```csharp
public class C
{
#if X
    private int _t;
#endif
    private readonly int _y;
    private readonly int _c;
    private int _a;
}
```

The same holds for a group of whole statements in the body of a method in front of the spans.

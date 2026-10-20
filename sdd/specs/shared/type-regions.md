# Shared — Regions of a type

| | |
|---|---|
| ID prefix | SHARED-TYPEREGION |
| Status | Active |
| Code owners | `TypesOrganizedWithRegionsAnalyzerBase` |
| Uses | FMT-REGION-003 |

This spec covers what the region rules share about where a region sits: when a region directive is inside executable code or between declarations (RH7303, RH7304, RH7310), what the top-level regions of a type declaration are (RH7305, RH7305A, RH7308, RH7309, RH7311), and what RH7305 (every type with members) and RH7305A (only types with several kinds of member) share beyond that: which type declarations they check, which members they count, when a type counts as organized with regions, and where the diagnostic is. Which of these regions or types a rule reports is each rule's own spec.

## Non-goals

- Which members belong in which region — RH7308 — and the order of the regions — RH7309.
- The names of the regions — RH7302, RH7306, RH7307, RH7311.
- A code fix for RH7305 or RH7305A. Which regions a type should have is the user's decision, so neither rule offers one.
- How `#region` and `#endregion` directives pair — FMT-REGION-003.

## Requirements

### SHARED-TYPEREGION-001 — Top-level type declarations are checked

A rule MUST check every class, struct, interface, record, and record struct declaration that is not declared inside another type, and each declaration of a `partial` type on its own. It MUST NOT report a type declared inside another type, an enum declaration, or a delegate declaration.

Why: A nested type is a member of the type around it, so the enclosing type's regions already group it; asking for regions inside it as well would demand a second outline for one entry of the first. An enum's members are values, not a mix of declarations to navigate. Each declaration of a partial type is what the user reads in one file, so it is checked with the members it declares itself.

Example — RH7305:

```csharp
class {|RH7305:C|}
{
    #region Methods

    void M()
    {
    }

    #endregion

    class D
    {
        int _x;

        void N()
        {
        }
    }
}

enum E
{
    A,
    B,
}
```

`C` is reported because its nested type `D` lies outside every region; `D` and `E` are not reported. For a partial type, only the declaration with a member outside every region is reported:

```csharp
partial class P
{
    #region Methods

    void A()
    {
    }

    #endregion
}

partial class {|RH7305:P|}
{
    void B()
    {
    }
}
```

### SHARED-TYPEREGION-002 — The members a type declares itself are counted

A rule MUST count the members the type declaration declares directly: nested types (enums and extension blocks included), delegates, fields, constructors, finalizers, properties, indexers, events in both forms, methods, operators, and conversion operators. It MUST NOT report a type that declares none of them.

Why: These are the declarations a reader navigates by. A type without any — an empty class or a positional record whose members are all generated from its parameter list — has nothing to organize.

Example — RH7305:

```csharp
class C
{
}

record R(int X);
```

is not reported.

### SHARED-TYPEREGION-003 — A type is organized when every counted member is in a top-level region

A type declaration MUST count as organized with regions exactly when every member it counts (SHARED-TYPEREGION-002) lies in one of its top-level regions (SHARED-TYPEREGION-006).

Why: The top-level regions of a type's body are its outline. A region around the type groups types, not members, and a region inside a method body hides statements, not members, so neither tells the reader where a member is. A nested region still lies inside the outer one, so a member in it is accounted for.

Example — RH7305, a member inside a nested region counts:

```csharp
class C
{
    #region Members

    #region Fields

    int _a;

    #endregion

    void M()
    {
    }

    #endregion
}
```

is not reported, while a region around the type or inside a body does not count:

```csharp
#region Types
class {|RH7305:C|}
{
    void M()
    {
        #region Body
        var a = 1;
        #endregion
    }
}
#endregion
```

### SHARED-TYPEREGION-004 — The diagnostic is on the type's name

A rule MUST report at most one diagnostic per type declaration, located on the type's identifier.

Why: The finding is about the type's shape as a whole, not about one member; one diagnostic on the name keeps a type with many members outside regions from flooding the list.

Example:

```csharp
interface {|RH7305:I|}
{
    void M();

    int P { get; }
}
```

### SHARED-TYPEREGION-005 — Inside executable code and between declarations

In a file without top-level statements, a rule MUST treat a region directive as inside executable code when it lies in a block, an accessor list, a lambda or anonymous method, a local function, or a statement. It MUST treat a region directive as between declarations when it lies between the members of a type or of an enum, between the declarations of a namespace, or at the top level of a file.

Why: Inside executable code a region hides statements; between declarations it groups members, types, or namespaces, which is what regions are for. The rules that report misplaced regions, align region directives, or remove empty regions need the same line between the two, or one rule would align a directive that another asks to remove.

Example — RH7303, which reports region directives inside executable code:

```csharp
class C
{
    int P
    {
        {|RH7303:#region Accessors|}
        get { return 1; }
        {|RH7303:#endregion|}
    }

    void M()
    {
        {|RH7303:#region Work|}
        M();
        {|RH7303:#endregion|}
    }
}
```

while directives between declarations are not reported:

```csharp
#region Types
namespace N
{
    #region Classes
    class C
    {
        #region Fields
        int _a;
        #endregion
    }
    #endregion

    enum E
    {
        #region Values
        A,
        #endregion
    }
}
#endregion
```

### SHARED-TYPEREGION-006 — The top-level regions of a type declaration

When every region directive inside a class, struct, interface, record, or record struct declaration lies inside executable code or between declarations (SHARED-TYPEREGION-005), none lies inside a nested enum or a nested extension block, and no `#region`–`#endregion` pair (FMT-REGION-003) crosses the start or the end of one of the declaration's members, the declaration's top-level regions MUST be the pairs whose two directives lie between its members (SHARED-TYPEREGION-005), inside its braces and outside its nested types, and that are not nested in another such pair. A member MUST count as lying in a top-level region when it lies wholly between the region's `#region` and `#endregion`, also when it lies in a region nested in it. Each declaration of a `partial` type has its own top-level regions.

Why: The regions directly in a type's body form the type's outline, and a nested region is a sub-heading of the region around it, so a member belongs to the outline entry of its outer region. A region around the type belongs to the enclosing scope, a region of a nested type to that type's outline, and a region in a method body to the method; none of them is an entry of this type's outline.

Example — RH7311, which compares the descriptions of top-level regions:

```csharp
class C
{
    #region Methods

    #region Methods

    void M()
    {
    }

    #endregion

    #endregion

    class D
    {
        #region Methods

        void N()
        {
        }

        #endregion
    }

    {|RH7311:#region Methods|}

    void O()
    {
    }

    #endregion
}
```

The nested `Methods` region and the region of `D` are not top-level regions of `C`, so only the second top-level `Methods` region of `C` is reported.


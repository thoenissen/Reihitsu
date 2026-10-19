# Shared — Types organized with regions

| | |
|---|---|
| ID prefix | SHARED-TYPEREGION |
| Status | Active |
| Code owners | `TypesOrganizedWithRegionsAnalyzerBase` |
| Uses | — |

This spec covers what RH7305 (every type with members) and RH7305A (only types with several kinds of member) share: which type declarations they check, which members they count, when a type counts as organized with regions, and where the diagnostic is. Which of the checked types a rule reports is each rule's own spec.

## Non-goals

- Which members belong in which region — RH7308 — and the order of the regions — RH7309.
- The names of the regions — RH7302, RH7306, RH7307, RH7311.
- A code fix. Which regions a type should have is the user's decision, so neither rule offers one.

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

A rule MUST count the members the type declaration declares directly: nested types (also enums), delegates, fields, constructors, finalizers, properties, indexers, events in both forms, methods, operators, and conversion operators. It MUST NOT report a type that declares none of them.

Why: These are the declarations a reader navigates by. A type without any — an empty class or a positional record whose members are all generated from its parameter list — has nothing to organize.

Example — RH7305:

```csharp
class C
{
}

record R(int X);
```

is not reported.

### SHARED-TYPEREGION-003 — A type is organized when every counted member is inside a region

A type declaration MUST count as organized with regions exactly when every member it counts (SHARED-TYPEREGION-002) lies wholly between a `#region` of the type's own body and the `#endregion` that closes it. A region nested in another region of the body counts through the outer region. A region around the whole type declaration and a region inside an element body MUST NOT count.

Why: The regions of a type's body are its outline. A region around the type groups types, not members, and a region inside a method body hides statements, not members, so neither tells the reader where a member is. A nested region still lies inside the outer one, so a member in it is accounted for.

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

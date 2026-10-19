# Shared — Nested type declarations

| | |
|---|---|
| ID prefix | SHARED-NESTED |
| Status | Active |
| Code owners | `NestedTypeAnalyzerHelper` |
| Uses | — |

This spec covers the rules that report a type of one kind declared inside another type. It defines which declarations are nested and where their diagnostic is located. Each rule that uses this spec states which kind of declaration it reports and any exceptions of its own.

## Non-goals

- Which kind of declaration a rule reports and its own exceptions — each rule's spec.
- How the containing type is declared: its kind, its accessibility, and whether it is `static` do not change whether a declaration in it is nested.
- A code fix: no rule of this kind offers one, because moving a type changes the name every user of it writes and may need a new file.

## Requirements

### SHARED-NESTED-001 — A nested declaration is declared directly in a type

A rule MUST treat a type or delegate declaration as nested exactly when it is declared directly in the body of a class, struct, interface, record, or record struct declaration, at any depth of nesting. A declaration directly in a namespace, in a file-scoped namespace, or at file level MUST NOT be treated as nested.

Why: A nested type is reached through its containing type, so it is harder to find, to reuse, and to test on its own, whatever kind of type contains it. A type declared in a namespace stands on its own.

Example — RH2101:

```csharp
namespace N
{
    class TopLevel
    {
        class {|RH2101:Inner|}
        {
            class {|RH2101:Deeper|}
            {
            }
        }
    }

    interface I
    {
        class {|RH2101:InInterface|}
        {
        }
    }

    record R
    {
        class {|RH2101:InRecord|}
        {
        }
    }

    struct S
    {
        class {|RH2101:InStruct|}
        {
        }
    }

    static class Holder
    {
        class {|RH2101:InStaticClass|}
        {
        }
    }
}

class AtFileLevel
{
}
```

### SHARED-NESTED-002 — One diagnostic per declaration, on its name

A rule MUST report one diagnostic per nested declaration, located on its identifier. Each part of a nested partial type MUST be reported as its own declaration.

Why: The name is what the user moves to a top-level declaration. Each part of a partial type is a separate declaration that has to move.

Example — RH2101:

```csharp
class Outer
{
    partial class {|RH2101:Part|}
    {
    }

    partial class {|RH2101:Part|}
    {
    }
}
```

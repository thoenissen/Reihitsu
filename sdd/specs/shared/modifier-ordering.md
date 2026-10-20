# Shared — Modifier ordering

| | |
|---|---|
| ID prefix | SHARED-MODORDER |
| Status | Active |
| Code owners | `ModifierOrderingUtilities`, `ModifierOrderingCodeFixProviderBase` |
| Uses | — |

This spec covers what the two modifier ordering rules share: which declarations they check, how their code fixes rewrite the modifier list, and when the fixes are withheld. It applies to RH7105 (declaration keyword order) and RH7106 (`protected` before `internal`, `private` before `protected`). Which order each rule enforces, which modifier it reports, and where the fix puts each keyword is that rule's own spec.

## Non-goals

- The order each rule enforces — RH7105 and RH7106.
- The layout of the rest of the declaration after the fix. The fix lays the declaration out with the formatter (FMT-PIPE); this spec only fixes what happens to the modifiers and the trivia between them.
- Which accessibility a declaration should have — RH2004, RH2005, RH7104.

## Requirements

### SHARED-MODORDER-001 — The checked declarations

A modifier ordering rule MUST check the modifier list of every class, struct, interface, record, record struct, delegate, field, constructor, property, event, event field, method, indexer, operator, and conversion operator declaration.

Why: These are the declarations whose modifier lists carry accessibility together with other keywords, so a reader compares their modifiers across a type. Checking them all with one list keeps the two rules consistent: a declaration that one rule checks is checked by the other too.

Example — RH7105 checks a class, a field, an event field, a property, a method, an operator, and a conversion operator:

```csharp
using System;

{|RH7105:static|} public class S
{
}

public class C
{
    {|RH7105:static|} public int F;

    {|RH7105:static|} public event Action E;

    {|RH7105:static|} public int P { get; set; }

    {|RH7105:static|} public void M()
    {
    }

    {|RH7105:static|} public C operator +(C a, C b)
    {
        return a;
    }

    {|RH7105:static|} public implicit operator int(C c)
    {
        return 0;
    }
}
```

Example — RH7106 checks a struct, an interface, a record, a record struct, a delegate, a constructor, an event, and an indexer:

```csharp
using System;

public class C
{
    internal {|RH7106:protected|} struct S
    {
    }

    internal {|RH7106:protected|} interface I
    {
    }

    internal {|RH7106:protected|} record R;

    internal {|RH7106:protected|} record struct RS;

    internal {|RH7106:protected|} delegate void D();

    internal {|RH7106:protected|} C()
    {
    }

    internal {|RH7106:protected|} event Action E
    {
        add
        {
        }

        remove
        {
        }
    }

    internal {|RH7106:protected|} int this[int index] => index;
}
```

### SHARED-MODORDER-002 — The fix rewrites only the modifier keywords

The code fix MUST write the declaration's modifier keywords in the order its rule produces, keeping every keyword the declaration had. The trivia in front of the first modifier MUST stay in front of the new first modifier, and each modifier MUST be followed by a single space.

Why: The diagnostic is about the order of the keywords, so the fix changes the order and nothing else in the list. The trivia in front of the first modifier — the indentation, a comment on the same line, a documentation comment of a declaration without attributes — belongs to the declaration, not to the keyword that happened to come first.

Example — RH7105:

```csharp
class C
{
    /* lead */ {|RH7105:static|}   public int A;
}
```

```csharp
class C
{
    /* lead */ public static int A;
}
```

### SHARED-MODORDER-003 — A comment or directive between the modifiers withholds the fix

The code fix MUST NOT be offered when a comment or a directive follows any modifier before the next token of the declaration, or precedes any modifier but the first. The diagnostic is still reported. A comment or directive in front of the first modifier does not withhold the fix (SHARED-MODORDER-002).

Why: The fix writes each modifier with a single space after it, so trivia between the modifiers would be dropped, and a comment written next to one keyword cannot be placed next to the same keyword in the new order. A reported diagnostic without a fix is a supported end state.

Example — a comment after a modifier:

```csharp
class C
{
    {|RH7105:static|} /* keep */ public int Value { get; set; }
}
```

is reported, and no code fix is offered.

Example — a directive in front of a later modifier:

```csharp
class C
{
    internal
#pragma warning disable CS0169
    {|RH7106:protected|} int _value;
}
```

is reported, and no code fix is offered.

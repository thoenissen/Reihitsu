# Shared — Single-letter names

| | |
|---|---|
| ID prefix | SHARED-SINGLE |
| Status | Active |
| Code owners | `SingleLetterIdentifierUtilities` |
| Uses | — |

This spec covers the rules that report a name of a single letter for one kind of declaration. It defines which names count as single-letter names and which variables are the iteration variables of a `foreach` statement. Each rule that uses this spec states which declarations it checks, where its diagnostic is located, and any exceptions of its own.

## Non-goals

- The casing of a name — the shared casing spec and the rules that use it.
- Which declarations a rule checks and its own exceptions — each rule's spec.
- A code fix: no rule of this kind offers one, because a meaningful name cannot be derived from a single letter.

## Requirements

### SHARED-SINGLE-001 — A single-letter name is one character other than `_`

A name MUST count as a single-letter name exactly when it consists of one character and that character is not `_`. The name is the identifier without its verbatim prefix, so `@b` is the single-letter name `b`. An uppercase letter and a letter of any script count like a lowercase ASCII letter.

Why: A single letter says nothing about the value it names, so a reader has to trace the declaration and its uses to learn what the value is. `_` is the conventional name of a value that is deliberately not used; there is nothing for a longer name to say about it.

Example — RH4122:

```csharp
class C
{
    void M(int {|RH4122:a|}, int {|RH4122:X|}, int {|RH4122:@b|}, int {|RH4122:é|}, int _, int ab)
    {
    }
}
```

### SHARED-SINGLE-002 — The iteration variables of a `foreach` statement

A variable MUST count as an iteration variable of a `foreach` statement exactly when it is the single variable that the statement declares, as in `foreach (var x in …)`, or when it is declared inside the variable part of a deconstructing `foreach` statement, as in `foreach (var (k, v) in …)` or `foreach ((var k, var v) in …)`, at any nesting depth. A variable declared in the collection expression or in the body of the statement MUST NOT count as an iteration variable.

Why: The iteration variables are the ones the loop assigns on every pass and names the elements by. A variable declared in the collection expression or in the body is an ordinary local of the surrounding code or of the loop body.

Example — RH4126, which reports single-letter iteration variables:

```csharp
using System.Collections.Generic;

class C
{
    void M(List<(int, int)> pairs, List<(int, (int, int))> nested, int[] values)
    {
        foreach (var {|RH4126:x|} in values)
        {
        }

        foreach (var ({|RH4126:k|}, {|RH4126:v|}) in pairs)
        {
        }

        foreach ((var {|RH4126:a|}, var {|RH4126:b|}) in pairs)
        {
        }

        foreach ((int {|RH4126:c|}, var ({|RH4126:d|}, {|RH4126:e|})) in nested)
        {
        }
    }
}
```

Example — RH4125, which reports the other single-letter locals:

```csharp
class C
{
    void M()
    {
        foreach (var item in Get(out var {|RH4125:z|}))
        {
            var {|RH4125:y|} = item;
        }
    }

    int[] Get(out int count)
    {
        count = 0;

        return [];
    }
}
```

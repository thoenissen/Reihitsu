# Local constants get their own casing rule, and every deconstruction variable goes to RH4117

| | |
|---|---|
| Status | Draft |
| Date | 2026-11 |
| Affected requirements | RH4115-001, RH4115-002, RH4115-004 (added), RH4115-005 (added), RH4117-001, RH4117-003 (added); new spec: RH4128 (RH4128-001 to RH4128-003) |

## What

Three rules split the casing of local names between them, and each local name is checked by exactly one of them (thoenissen/Reihitsu#1087, thoenissen/Reihitsu#1088):

- **RH4115 — Local variable names should be in camelCase** no longer reports a local named `_`, no longer checks local constants, and no longer checks any variable that a deconstruction declares.
- **RH4117 — Deconstruction variable names should be in camelCase** checks every variable that a deconstruction declares: the variables of `var (…)`, the variables declared inside a tuple on the left of a deconstruction or in front of `in` of a `foreach` (`(var a, var b) = p`, `foreach ((var k, var v) in list)`), and the variables of a deconstruction pattern (`p is var (a, b)`, `case var (l, r)`, and the subpatterns of a positional pattern, `p is (var a, int b)`).
- A new rule, **RH4128 — Local constant names should be in camelCase** (Naming, Warning, enabled, Fix All), reports a local constant whose name is not in camelCase, and its code fix renames the constant and every reference to it. A local constant named `_` is not reported.

A simple pattern variable (`is int x`, `is var x`), the variable that names a whole property or positional pattern (`p is (1, 2) both`), and every other local stay with RH4115.

## Why

The maintainer decided the open questions of thoenissen/Reihitsu#1019 in question round 3 (2026-10-18), recorded in thoenissen/Reihitsu#1087, and the casing of local constants on 2026-11-21, recorded in thoenissen/Reihitsu#1088:

- `_` is the conventional name of a value that is not used. Every neighbouring rule exempts it — a parameter `_` (RH4105, RH4120), a field `_` (SHARED-CASING-003), and every single-letter rule (SHARED-SINGLE-001) — and RH4115 offered no fix for it, so the user could only resolve the report by hand.
- Which rule reported a deconstruction variable depended on how the deconstruction was spelled: `var (a, b) = p` went to RH4117, `(var a, var b) = p` to RH4115, and `p is var (a, b)` to no rule at all. A user who turned RH4117 off still got RH4115 for one spelling, and the RH4115 rule page already said deconstruction variables were RH4117's.
- Local constants get a rule of their own so they can be configured apart from variables. The style stays camelCase, like every other local: the repository's own code already writes local constants that way, so the rule needs no repository-wide rename.

## Visible output changes

Each block below is the input; the lines after it give the diagnostics and the fix result before and after the change.

### 1. RH4115 · Local variable names should be in camelCase — a local named `_`

```csharp
using System;

class C
{
    void M(int[] values)
    {
        int _ = 42;

        foreach (var _ in values)
        {
        }

        try
        {
        }
        catch (Exception _)
        {
        }
    }
}
```

Before: RH4115 is reported on each of the three `_`; no code fix is offered.

After: not reported.

### 2. RH4115 / RH4128 — a local constant

```csharp
class C
{
    int M()
    {
        const int MaxValue = 3;

        return MaxValue;
    }
}
```

Before: RH4115 (*Local variable names should be in camelCase*) is reported on `MaxValue`.

After: RH4128 (*Local constant names should be in camelCase*) is reported on `MaxValue`; RH4115 is not.

The fix result is the same before (RH4115) and after (RH4128):

```csharp
class C
{
    int M()
    {
        const int maxValue = 3;

        return maxValue;
    }
}
```

`const int maxValue = 3;` is reported by neither rule, before or after. A single-letter local constant such as `const int K = 2;` is reported by RH4115 and RH4125 before, and by RH4128 and RH4125 after.

### 3. RH4128 · Local constant names should be in camelCase — a local constant named `_`

```csharp
class C
{
    void M()
    {
        const int _ = 1;
    }
}
```

Before: RH4115 is reported on `_`; no code fix is offered.

After: not reported.

### 4. RH4115 / RH4117 — variables declared inside a tuple on the left of a deconstruction

```csharp
using System.Collections.Generic;

class C
{
    int M((int, int) pair, List<(int, int)> pairs)
    {
        (var First, var Second) = pair;
        (int Third, var (Fourth, Fifth)) = (1, pair);

        foreach ((var Key, var Value) in pairs)
        {
        }

        return First + Second + Third + Fourth + Fifth;
    }
}
```

Before: RH4115 is reported on `First`, `Second`, `Third`, `Key`, and `Value`; RH4117 on `Fourth` and `Fifth`.

After: RH4117 is reported on all seven; RH4115 on none.

The fix result is the same before and after:

```csharp
using System.Collections.Generic;

class C
{
    int M((int, int) pair, List<(int, int)> pairs)
    {
        (var first, var second) = pair;
        (int third, var (fourth, fifth)) = (1, pair);

        foreach ((var key, var value) in pairs)
        {
        }

        return first + second + third + fourth + fifth;
    }
}
```

### 5. RH4117 · Deconstruction variable names should be in camelCase — a `var (…)` pattern

```csharp
class C
{
    int M((int, int) pair)
    {
        if (pair is var (First, Second))
        {
            return First + Second;
        }

        switch (pair)
        {
            case var (Left, Right):
                return Left + Right;
        }

        return 0;
    }
}
```

Before: not reported.

After: RH4117 is reported on `First`, `Second`, `Left`, and `Right`, and the fix gives:

```csharp
class C
{
    int M((int, int) pair)
    {
        if (pair is var (first, second))
        {
            return first + second;
        }

        switch (pair)
        {
            case var (left, right):
                return left + right;
        }

        return 0;
    }
}
```

### 6. RH4115 / RH4117 — the subpatterns of a positional pattern

```csharp
class C
{
    int M((object, object) pair)
    {
        if (pair is (var Third, int Fourth))
        {
            return (int)Third + Fourth;
        }

        return 0;
    }
}
```

Before: RH4115 is reported on `Third` and `Fourth`.

After: RH4117 is reported on `Third` and `Fourth`; RH4115 is not. The fix result is the same before and after:

```csharp
class C
{
    int M((object, object) pair)
    {
        if (pair is (var third, int fourth))
        {
            return (int)third + fourth;
        }

        return 0;
    }
}
```

This case is the spec author's reading of the maintainer's decision ("RH4117 owns every variable a deconstruction declares, in assignments and in patterns alike"), which named `(var a, var b) = p` but not its pattern counterpart `p is (var a, var b)`; it is asked as an output question before it is built.

Unchanged: the variable that names a whole pattern, such as `Both` in `pair is (1, 2) Both`, a variable of a property pattern (`is string { Length: var Size }`), and a simple pattern variable (`is int Number`, `is var Value`) stay with RH4115.

### 7. Rule pages

- `documentation/rules/RH4115.md` says that local constants are checked by RH4128.
- `documentation/rules/RH4117.md` says that the rule covers every way of declaring variables in a deconstruction, in patterns as well.
- `documentation/rules/RH4128.md` is new.

## Decisions

### `_` is exempt from RH4115

Maintainer, 2026-10-18 (thoenissen/Reihitsu#1087, Q1 of thoenissen/Reihitsu#1019). The new rule exempts a local constant named `_` as well, for the same reason (RH4128-002).

### RH4115 does not check local constants; RH4128 checks them for camelCase

Maintainer, 2026-10-18 (Q2) and 2026-11-21 (thoenissen/Reihitsu#1088), which replaced the PascalCase starting point of the issue with camelCase. Both changes ship together, so a local constant is checked by exactly one rule. The rule's ID is RH4128: RH4119, the gap in the RH41xx range, was shipped in `v1.0.0-beta5` and `v1.0.0-beta6` for single-letter identifiers and retired when RH4122 to RH4127 replaced it, so it is not free; a user's suppression of the old RH4119 would otherwise silence the new rule.

### RH4128 has a code fix

The fix is the rename that RH4115 already applied to local constants: the shared casing rename (SHARED-CASING-006 to SHARED-CASING-010, SHARED-CASING-013), which renames the constant and every reference to it and is withheld where the rename is not safe. It is delivered comprehensively, so the rule offers it with Fix All (`AGENTS.md`, *Code-fix coverage*).

### RH4117 owns every variable that a deconstruction declares

Maintainer, 2026-10-18 (Q3 and Q4): in assignments, `foreach`, and patterns alike; a simple pattern variable stays with RH4115. The boundary is stated by where the variable's designation stands in the syntax tree: inside a parenthesized variable designation, in a declaration expression that is an element of a tuple expression, or as the pattern of a subpattern of a positional pattern. The last one is the open reading in case 6.

## Alternatives considered

- **PascalCase for local constants**, like constant fields (RH4110) and the .NET convention — rejected by the maintainer on 2026-11-21: the repository writes local constants in camelCase, and a local constant is a local.
- **Keep the split by spelling and correct the RH4115 rule page** (Q3, reading B) — rejected: it keeps one deconstruction reported by two rules depending on its spelling.
- **RH4115 reports the variables of a `var (…)` pattern** (Q4, reading A) — rejected: they are deconstruction variables and belong with the other forms.
- **Reuse RH4119** for the new rule — rejected: a retired, shipped ID is not free.

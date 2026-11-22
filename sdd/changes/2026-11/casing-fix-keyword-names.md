# Casing fixes are withheld when the new name needs `@`

| | |
|---|---|
| Status | Accepted |
| Date | 2026-11 |
| Affected requirements | SHARED-CASING-007, SHARED-CASING-013 (added by this change), RH4105-003, RH4115-003, RH4117-002 |

## What

A casing fix is no longer offered when the new name could only be written with the verbatim prefix `@`, at the declaration or at a reference the rename changes (SHARED-CASING-013): when the derived name is a reserved C# keyword, such as `class`, and when it is a contextual keyword that is a keyword where the declaration or a reference stands, such as `await` inside an async method. The diagnostic is still reported. A contextual keyword that can be written without `@` everywhere, such as `var`, `value`, or `nameof`, keeps its fix (thoenissen/Reihitsu#1072).

Only a camelCase target can produce such a name, because every keyword is lowercase: the fixes of RH4105, RH4115, and RH4117. `_camelCase` targets start with `_`, and PascalCase and type-parameter targets with an uppercase letter.

## Why

Principle P3 (thoenissen/Reihitsu#912) decided that a casing fix whose derived name is a reserved keyword is withheld instead of writing `@keyword` (thoenissen/Reihitsu#1009 Q2). The `@` is spelling the author did not write, and the name stays a keyword to every reader. SHARED-CASING-007 so far left these renames outside its coverage without deciding them.

## Visible output changes

The "before" outputs were measured with `scripts/apply-fix.sh RH4105` on `a8fa3fa`, under LF and CRLF alike. No rule page changes: the withheld fix is a gap `documentation/rules/general-notes.md` already covers ("A diagnostic without a code fix").

### 1. RH4105 — a derived name that is a reserved keyword, or `await` in an async method

```csharp
// input
using System.Threading.Tasks;

class C
{
    void M(int Class, object Object)
    {
    }

    int N(string String)
    {
        return String.Length;
    }

    async Task<int> O(int Await)
    {
        await Task.Yield();
        return Await;
    }
}
```

```csharp
// before
using System.Threading.Tasks;

class C
{
    void M(int @class, object @object)
    {
    }

    int N(string @string)
    {
        return @string.Length;
    }

    async Task<int> O(int @await)
    {
        await Task.Yield();
        return @await;
    }
}
```

After: all four parameters are still reported, and no code fix is offered for any of them. The same holds for a local variable (RH4115) and a deconstruction variable (RH4117).

### 2. Unchanged — a contextual keyword that needs no `@`

```csharp
// input
class C
{
    int M(int Var, int Nameof, int Await)
    {
        return Var + Nameof + Await;
    }
}
```

```csharp
// before and after
class C
{
    int M(int var, int nameof, int await)
    {
        return var + nameof + await;
    }
}
```

## Decisions

### Withheld by the cause, not by a keyword list

The requirement names the cause — the rename would write `@` — and gives reserved keywords and `await` in an async member as the cases it covers. A list of contextual keywords would have to track the language; the cause decides every one of them the same way.

### A new requirement instead of a condition of SHARED-CASING-008

SHARED-CASING-008 withholds a fix when no usable name can be derived from the old one. A keyword is a usable name with `@`; withholding it has its own reason (P3), so it is its own requirement.

### Maintainer answer to output batch 1

- **Question 6 — RH4105, RH4115, RH4117, a derived name that needs `@`: A.** No fix is offered for `Class`, `Object`, and `Await` in an async method; the diagnostics stay. `Var`, `Nameof`, and `Await` in a method that is not async keep their fix (`var`, `nameof`, `await`).

## Alternatives considered

- **Keep writing `@keyword`** — rejected by P3.
- **Derive another name, such as `classValue`** — rejected: the fix would invent a word the author did not write.

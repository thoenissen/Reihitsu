# Naming and documentation rules for union declarations

| | |
|---|---|
| Status | Draft |
| Date | 2026-11 |
| Affected requirements | RH8030-001, RH8108-001, RH8109-001, RH8110-001, RH8111-001, RH8203-001, SHARED-DOC-001, SHARED-DOC-002; the specs `analyzer/RH4012.md`, `analyzer/RH8035.md`, and `analyzer/RH8036.md` |
| Depends on | [Roslyn 5.12 and C# 15 as the newest supported language version](roslyn-512-csharp-15.md) |

## What

A C# 15 union declaration (`union Pet(Cat, Dog);`) becomes a type that the naming and documentation rules check, as they check classes, structs, interfaces, records, record structs, enums, and delegates:

- **RH4012 Union name casing** (new) reports a union whose name is not in PascalCase and offers the rename fix every type-casing rule offers, with Fix All.
- **RH8035 Non-private unions must be documented** and **RH8036 Private unions must be documented** (new) report a union without `<summary>` or `<inheritdoc>`, split by the private and non-private groups of SHARED-DOC-002, as every other type kind is split.
- **RH8030** checks the `<summary>` of a union for text.
- **RH8108 to RH8111** check the type parameters of a generic union and its `<typeparam>` elements.
- **RH8203** reports `<inheritdoc/>` on a union that inherits nothing: a union without a base list.
- The case types of a union have no names, so **RH8101 to RH8104** do not check a union at all — no `<param>` is required for a case type, and a `<param>` element on a union is not judged.

SHARED-DOC-001 and SHARED-DOC-002 name a union where they list the declarations that contain a nested declaration. That writes down what the code already does: a member declared in a union body is checked, and one without an access modifier is in the private group.

## Why

The naming and documentation rules register one rule per type kind by syntax kind, so a union is never analyzed: `public union pet(Cat, Dog);` gets neither a casing nor a documentation diagnostic, while `public class shape { }` gets both (thoenissen/Reihitsu#1766). A union is a type that other code names and uses like any other, so it needs a PascalCase name and a summary for the same reasons. Following the one-rule-per-kind convention, a union gets its own rule IDs rather than widening the struct or class rules, so a project can configure unions on their own, and a rule's ID keeps naming exactly one construct ([P11](../../README.md#the-formatter-may-do-more-than-the-analyzers-p11)).

A union's case types are written as a parameter list, but they are types without names: no `<param name="…">` can refer to one. The parameter rules therefore have nothing to check on a union.

## Visible output changes

**1. RH4012 — a union name that is not in PascalCase is reported and renamed.**

```csharp
// input
public union pet(int, string);
```

```csharp
// before: not reported
```

```csharp
// after: RH4012 on `pet`; the code fix renames the union and every reference to it
public union Pet(int, string);
```

**2. RH8035 / RH8036 — an undocumented union is reported.**

```csharp
// input
public union Shape(int, string);

/// <summary>
/// Outer
/// </summary>
public class Outer
{
    private union Inner(int, string);
}
```

```csharp
// before: not reported
```

```csharp
// after: RH8035 on `Shape`, RH8036 on `Inner`; no code fix
public union {|RH8035:Shape|}(int, string);

/// <summary>
/// Outer
/// </summary>
public class Outer
{
    private union {|RH8036:Inner|}(int, string);
}
```

**3. RH8030 — a union summary without text is reported.**

```csharp
// input
/// <summary></summary>
public union Shape(int, string);
```

```csharp
// before: not reported
```

```csharp
// after: RH8030 on the summary element
/// {|RH8030:<summary></summary>|}
public union Shape(int, string);
```

**4. RH8108 to RH8111 — the type parameters of a union are checked.**

```csharp
// input
/// <summary>
/// Result
/// </summary>
/// <typeparam name="TValue">Value</typeparam>
/// <typeparam name="TOther"></typeparam>
public union Result<TValue, TError>(TValue, TError);
```

```csharp
// before: not reported
```

```csharp
// after: RH8108 on `TError`, RH8109 and RH8111 on the second <typeparam> element
/// <summary>
/// Result
/// </summary>
/// <typeparam name="TValue">Value</typeparam>
/// {|RH8109:{|RH8111:<typeparam name="TOther"></typeparam>|}|}
public union Result<TValue, {|RH8108:TError|}>(TValue, TError);
```

**5. RH8203 — `<inheritdoc/>` on a union without a base list is reported.**

```csharp
// input, with a documented interface IShape
/// <inheritdoc/>
public union Shape(int, string);

/// <inheritdoc/>
public union Named(int, string) : IShape;
```

```csharp
// before: not reported
```

```csharp
// after: RH8203 on the first <inheritdoc/>; the union with a base list is not reported
/// {|RH8203:<inheritdoc/>|}
public union Shape(int, string);

/// <inheritdoc/>
public union Named(int, string) : IShape;
```

**6. RH8101 to RH8104 — unchanged: a union's case types never cause a parameter-documentation diagnostic.**

```csharp
// input
/// <summary>
/// Shape
/// </summary>
/// <param name="Circle"></param>
public union Shape(int, string);
```

```csharp
// before: not reported
```

```csharp
// after: not reported
```

**7. Rule pages.** New pages `documentation/rules/RH4012.md`, `RH8035.md`, and `RH8036.md`. The page of RH8030 names RH8035 and RH8036 among the rules that require a declaration to be documented at all.

## Decisions

### Unions get their own rule IDs

Pending the maintainer's answer. Recommended: one naming rule (RH4012) and two documentation rules split by group (RH8035, RH8036), as every other type kind has. The issue names only a non-private documentation rule; the private one follows from the split that every documentation rule of a type kind has (SHARED-DOC-002), so that a nested `private union` is reported under the private rule and a project that requires documentation only for what other code uses can turn it off.

### RH4012 offers the rename fix

Pending the maintainer's answer. Recommended: the fix every type-casing rule offers (SHARED-CASING-006 to SHARED-CASING-010), with Fix All; a union is a named type, and renaming it and its references is the same rename as for a struct.

### A union's case types are not documented

Pending the maintainer's answer. Recommended: RH8101 to RH8104 do not check a union, so nothing changes for them. A case type has no name, so no `<param>` can document it; checking a union would only add a report for a stray `<param>` on a union, which is rare, and would need the pairing of SHARED-DOC-015 to learn parameters without names.

### Summary text, type parameters, and `<inheritdoc>` on a union are checked like any other type

Pending the maintainer's answer. Recommended: RH8030, RH8108 to RH8111, and RH8203 check a union as they check a struct. A union inherits documentation when it has a base list, as a struct does; the interface the compiler adds to every union is not written in its base list, just as a struct's `ValueType` is not, so a union without a base list inherits nothing.

### SHARED-DOC-001 and SHARED-DOC-002 name the union body

A member declared in a union body is already checked, and one without an access modifier is in the private group, because the code treats every type body other than an interface's alike. The two requirements name a union so that their lists say so; this is a clarification and changes no output.

## Alternatives considered

- **Widen RH4003, RH8003, and RH8004 (structs) to unions**, since the compiler emits a union as a struct — rejected: a union is a different declaration with its own keyword, and a rule is not widened to a different construct because it roughly fits (P11). A user who configures structs would silently configure unions too.
- **Widen RH4002 and RH8001 (classes)** — rejected for the same reason, and a union is not a class.
- **One documentation rule for every union, without the private split** — rejected: every other type kind is split by SHARED-DOC-002, and a project that documents only what other code uses could not exempt a nested private union.
- **Check unions in RH8101 to RH8104 and skip the nameless case types** — rejected, see *A union's case types are not documented*.
- **Treat the compiler-added union interface as a base type for RH8203** — rejected: `<inheritdoc/>` on such a union would inherit nothing a reader wrote, and a struct's implicit base type is not treated as one either.

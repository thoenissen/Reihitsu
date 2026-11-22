# RH4120 does not report a positional record parameter whose property another compilation dictates

| | |
|---|---|
| Status | Draft |
| Date | 2026-11 |
| Affected requirements | SHARED-CASING-014, RH4120-001 |
| Depends on | [Casing rules do not report names that another compilation or a native entry point dictates](casing-names-dictated-elsewhere.md) |

## What

RH4120 no longer reports a positional parameter of a record when the property the compiler declares for that parameter overrides or implicitly implements a member declared outside the compilation — in a referenced assembly, or in another project of the solution —, directly or through a member it overrides at any level (thoenissen/Reihitsu#1939). SHARED-CASING-014 gains this case as one more name a casing rule does not report; RH4120-001 references it.

Every other positional parameter keeps today's behavior and is reported when its name is not in PascalCase:

- a parameter whose property overrides or implements only members declared in source;
- a parameter whose property implements an interface member only for a derived record that lists the interface, while the parameter's own record does not implement it (SHARED-CASING-014 already states this boundary for members);
- a parameter for which the compiler declares no property, because the record declares or inherits a member of the parameter's name — whatever that member overrides or implements.

## Why

The compiler names the property it declares for a positional parameter after the parameter, so the parameter's name is a public property's name. When that property implements or overrides a member another compilation declares, the compiler requires it to repeat that member's name: renaming the parameter to PascalCase gives the record a property that no longer implements the interface member or overrides the abstract one, and the code stops compiling. This is the cause SHARED-CASING-014 states for a property declared in the record's body, which RH4112 to RH4114 already do not report; the diagnostic on the parameter has no actionable outcome either.

A parameter for which the compiler declares no property dictates nothing: its name is a constructor parameter's name only, and renaming it does not touch the inherited member.

## Visible output changes

The fixture runner cannot add a referenced assembly, so the "before" is read from the analyzer, which reports every positional parameter of a record whose name is not in PascalCase by its syntax alone. The input compiles with the referenced assembly below (checked with a two-project build). No code-fix result changes beyond the diagnostic: where nothing is reported, no fix is offered. No rule page changes: the page shows the normal case, and this is an exception.

### 1. RH4120 — a positional parameter whose property implements or overrides a referenced-assembly member

With a referenced assembly that declares:

```csharp
public interface INamed
{
    string name { get; }
}

public abstract record MetaRecord
{
    public abstract string title { get; init; }
}
```

```csharp
// input
public record Person(string name) : INamed;

public record struct Point(string name) : INamed;

public record Item(string title) : MetaRecord;
```

```csharp
// before
public record Person(string {|RH4120:name|}) : INamed;

public record struct Point(string {|RH4120:name|}) : INamed;

public record Item(string {|RH4120:title|}) : MetaRecord;
```

```csharp
// after — not reported
public record Person(string name) : INamed;

public record struct Point(string name) : INamed;

public record Item(string title) : MetaRecord;
```

Unchanged, with the same referenced assembly — still reported:

```csharp
public record Holder(string {|RH4120:name|});

public record Sub(string {|RH4120:other|}) : Holder(other), INamed;

public record Derived(string {|RH4120:name|}) : Person(name);
```

## Decisions

### A dictated property's parameter is not reported

The maintainer chose option A on thoenissen/Reihitsu#1939 (2026-11-22): RH4120 does not report a positional record parameter whose generated property implements a property of another assembly; that name is dictated, as SHARED-CASING-014 states for properties. The override of an abstract property that another compilation declares, which the compiler also generates for a positional parameter, has the same cause and follows the same answer.

### The case is stated in SHARED-CASING-014

The case is a bullet of SHARED-CASING-014, which defines when a name is dictated, and RH4120-001 references it, so the definition of a dictated member — the compilation boundary, the chain through every level, and the rule that only the member's own type decides an implementation — applies to the parameter's property without being restated.

### Only the property the compiler declares decides

The parameter is exempted through the property the compiler declares for it, not through a member of the same name it inherits. A parameter that only passes its value to a base record's dictated property declares nothing that the other compilation names, and its rename compiles.

## Alternatives considered

- **Report the parameter and withhold the fix** (option B on the issue) — rejected by the maintainer: the diagnostic would have no actionable outcome, the cause SHARED-CASING-014 excludes.
- **A requirement of RH4120's own** instead of a bullet of SHARED-CASING-014 — rejected: it would restate the definition of a dictated member, which the shared spec owns.
- **Also exempt a parameter whose record inherits a dictated member of the parameter's name** — rejected: the compiler declares no property for that parameter, so its name dictates nothing and a rename compiles; the issue does not ask for it.

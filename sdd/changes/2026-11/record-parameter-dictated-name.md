# RH4120 does not report a positional record parameter whose member another compilation dictates

| | |
|---|---|
| Status | Accepted |
| Date | 2026-11 |
| Affected requirements | SHARED-CASING-014, RH4120-001 |
| Depends on | [Casing rules do not report names that another compilation or a native entry point dictates](casing-names-dictated-elsewhere.md) |

## What

RH4120 no longer reports a positional parameter of a record when the member the parameter stands for is dictated (thoenissen/Reihitsu#1939): when that member overrides or implicitly implements a member declared outside the compilation — in a referenced assembly, or in another project of the solution —, directly or through a member it overrides at any level. The member the parameter stands for is the property the compiler declares for it, or, when the compiler declares none because the record declares or inherits a member of the parameter's name, that member. SHARED-CASING-014 gains this case as one more name a casing rule does not report; RH4120-001 references it, and its coverage statement now names every positional parameter, as the analyzer already checks them.

Every other positional parameter keeps today's behavior and is reported when its name is not in PascalCase:

- a parameter whose member overrides or implements only members declared in source;
- a parameter whose member implements an interface member only for a derived record that lists the interface, while the member's own type does not implement it (SHARED-CASING-014 already states this boundary for members);
- a parameter that matches a member it inherits from another compilation, when that member overrides and implements nothing there — it is declared elsewhere, but not dictated.

## Why

The compiler names the property it declares for a positional parameter after the parameter, so the parameter's name is a public property's name. When that property implements or overrides a member another compilation declares, the compiler requires it to repeat that member's name: renaming the parameter to PascalCase gives the record a property that no longer implements the interface member or overrides the abstract one, and the code stops compiling. This is the cause SHARED-CASING-014 states for a property declared in the record's body, which RH4112 to RH4114 already do not report; the diagnostic on the parameter has no actionable outcome either.

When the record declares or inherits a member of the parameter's name, the compiler declares no property and matches the parameter to that member. Renaming the parameter then compiles, but gives the record a second public property next to the dictated one, which shows in equality, `ToString`, `with`, and serialization — the outcome RH4120's code fix already refuses as unsafe. When that member is dictated, its own name cannot change either, so the diagnostic has no acceptable outcome. A member that is merely declared in another compilation, without overriding or implementing anything there, is not dictated, and the parameter stays reported.

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
```

### 2. RH4120 — a positional parameter that matches a dictated member the record inherits or declares

With the referenced assembly of case 1, which also declares:

```csharp
public record MetaPerson(string name);
```

```csharp
// input
public record Person(string name) : INamed;

public record Derived(string name) : Person(name);

public record Annotated(string name) : INamed
{
    public string name { get; init; } = name;
}

public record Inherited(string name) : MetaPerson(name);
```

```csharp
// before
public record Person(string {|RH4120:name|}) : INamed;

public record Derived(string {|RH4120:name|}) : Person(name);

public record Annotated(string {|RH4120:name|}) : INamed
{
    public string name { get; init; } = name;
}

public record Inherited(string {|RH4120:name|}) : MetaPerson(name);
```

No code fix is offered today on `Derived`, `Annotated`, or `Inherited`.

```csharp
// after
public record Person(string name) : INamed;

public record Derived(string name) : Person(name);

public record Annotated(string name) : INamed
{
    public string name { get; init; } = name;
}

public record Inherited(string {|RH4120:name|}) : MetaPerson(name);
```

`Inherited.name` stays reported, still without a fix: `MetaPerson.name` is declared in the referenced assembly but overrides and implements nothing, so it is not dictated.

## Decisions

### A dictated property's parameter is not reported

The maintainer chose option A on thoenissen/Reihitsu#1939 (2026-11-22): RH4120 does not report a positional record parameter whose generated property implements a property of another assembly; that name is dictated, as SHARED-CASING-014 states for properties.

### An override counts like an implementation

The maintainer chose 1A (2026-11-22): `public record Item(string title) : MetaRecord;`, where `MetaRecord.title` is an abstract property of a referenced assembly, is not reported. The compiler declares an overriding property for the parameter, and renaming it fails to compile (CS0534), the same cause as the interface case; RH4116 already exempts the tuple element names of this parameter for the override.

### The case is stated in SHARED-CASING-014

The case is a bullet of SHARED-CASING-014, which defines when a name is dictated, and RH4120-001 references it, so the definition of a dictated member — the compilation boundary, the chain through every level, and the rule that only the member's own type decides an implementation — applies to the parameter's property without being restated.

### A parameter that matches a dictated member is not reported

The maintainer chose 2B (2026-11-22): a positional parameter for which the compiler declares no property, because the record inherits or declares a member of its name, is not reported when that member is dictated — `public record Derived(string name) : Person(name);`, where `Person.name` implements the referenced `INamed.name`, and `public record Annotated(string name) : INamed { public string name { get; init; } = name; }`. The only way to resolve the diagnostic by hand is a rename that adds a second public property, the outcome the code fix already refuses. A parameter whose matching member is merely declared in another compilation, without being dictated (`public record Inherited(string name) : MetaPerson(name);`), stays reported (option C was not chosen): the exemption covers names a contract fixes, not every name another compilation chose.

### RH4120-001 covers every positional parameter

RH4120-001's coverage statement named only parameters the compiler turns into a property, while the analyzer checks every positional parameter, and existing tests pin the diagnostic on a parameter that matches a declared or inherited member. The statement now names every positional parameter, minus the exemptions of SHARED-CASING-014 and RH4120-002; this describes the existing behavior and changes no output beyond case 2.

## Alternatives considered

- **Report the parameter and withhold the fix** (option B on the issue) — rejected by the maintainer: the diagnostic would have no actionable outcome, the cause SHARED-CASING-014 excludes.
- **A requirement of RH4120's own** instead of a bullet of SHARED-CASING-014 — rejected: it would restate the definition of a dictated member, which the shared spec owns.
- **Report a parameter whose property overrides a referenced-assembly member** (1B) — rejected by the maintainer: the rename fails to compile as in the interface case, and RH4116 already exempts the same parameter.
- **Exempt only the property the compiler declares, and keep reporting a parameter that matches a dictated member** (2A) — rejected by the maintainer: the only manual resolution adds a duplicate public property.
- **Also exempt a parameter whose matching member is merely declared in another compilation** (2C) — rejected by the maintainer: such a member is not dictated, and the exemption would cover names no contract fixes.

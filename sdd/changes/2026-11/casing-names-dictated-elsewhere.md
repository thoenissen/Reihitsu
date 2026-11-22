# Casing rules do not report names that another compilation or a native entry point dictates

| | |
|---|---|
| Status | Accepted |
| Date | 2026-11 |
| Affected requirements | SHARED-CASING-007, SHARED-CASING-009, SHARED-CASING-014 (added by this change), SHARED-FIX-003, RH4102-001, RH4103-001, RH4104-001, RH4104-002, RH4112-001, RH4113-001, RH4114-001, RH4116-001 |

## What

The casing rules no longer report a name the declaration cannot choose (SHARED-CASING-014, thoenissen/Reihitsu#1434):

- the name of a method, property, or event that overrides or implicitly implements a member declared outside the compilation that declares it — in a referenced assembly, or in another project of the solution —, directly or through a member it overrides at any level;
- an element name of a tuple type in the signature of a member that overrides or implements, implicitly or explicitly, such a member;
- the name of a method or local function declared with `[DllImport]`, and of a method declared with `[LibraryImport]`, whose attribute has no `EntryPoint` argument.

A member implements an interface member only when its own type implements that interface: a base-class method that implements a referenced-assembly interface member only for a derived type that lists the interface stays reported. The tuple element names of a dictated member's signature are not reported even where a type argument written in this compilation supplies them; that type argument, where they are written, stays reported.

Overrides and implementations whose chain stays inside the compilation keep today's behavior: they are reported, and their fix renames the whole chain. A method whose attribute names an `EntryPoint` is reported as before.

The affected rules are RH4102 (events), RH4103 (methods), RH4104 (local functions, for `[DllImport]`), RH4112, RH4113, and RH4114 (properties), and RH4116 (tuple type element names). RH4111 is not affected: a private property can neither override nor implicitly implement a member.

## Why

Principle P2 (thoenissen/Reihitsu#912) decided that a name dictated from outside is not reported. The compiler requires an override and an implicit implementation to repeat the name of the member it overrides or implements, and an override or implementation to repeat the tuple element names of its signature; the runtime looks a native function up by the method's name when no `EntryPoint` is given. The author cannot rename such a name without breaking the build or the native binding, so the diagnostic has no actionable outcome.

The boundary is the compilation, not the solution. In the IDE, another project of the solution is a compilation reference whose members are declared in source; in a command-line build, the same project is a referenced assembly. Deciding by the compilation that declares the member gives the IDE and the build the same diagnostics.

## Visible output changes

The "before" of example 3 was measured with `scripts/apply-fix.sh RH4103` and `RH4104` on `a8fa3fa`. The fixture runner cannot add a referenced assembly or a second project, so the reports of examples 1 and 2 are read from the analyzers, which check these declarations by their syntax alone and report each of these names, and the cross-project fix of example 2 from `RH4103MethodNameCasingAnalyzerTests`; that no fix is offered in example 1 is not measured. No rule page changes: each page shows the normal case, and these are exceptions.

### 1. RH4102, RH4103, RH4112–RH4114, RH4116 — members that override or implement a member of a referenced assembly

With a referenced assembly that declares:

```csharp
public abstract class MetaBase
{
    public abstract void process();

    public abstract event System.EventHandler changed;

    public abstract int count { get; }

    public abstract (int first, int second) pair();
}

public interface IMeta
{
    void run();
}
```

```csharp
// input
using System;

public class Worker : MetaBase, IMeta
{
    public override void process()
    {
    }

    public override event EventHandler changed;

    public override int count => 0;

    public override (int first, int second) pair() => default;

    public void run()
    {
    }
}

public class Leaf : Worker
{
    public override void process()
    {
    }
}
```

Before: `process` (both), `run`, and `pair` are reported by RH4103, `changed` by RH4102, `count` by RH4114, and `first` and `second` by RH4116. After: none is reported.

### 2. RH4103 — an override of a method declared in another project of the solution

Project `BaseProject`:

```csharp
public class Base
{
    public virtual void process()
    {
    }
}
```

Project that references `BaseProject`:

```csharp
// input
public class Leaf : Base
{
    public override void process()
    {
    }
}
```

Before: `Leaf.process` is reported, and its fix renames `Leaf.process` and `Base.process` to `Process`. After: `Leaf.process` is not reported. `Base.process` is reported in `BaseProject` as before, and its fix renames both methods.

### 3. RH4103 and RH4104 — a native entry point taken from the method's name

```csharp
// input
using System.Runtime.InteropServices;

public static class Native
{
    [DllImport("libc")]
    public static extern int getpid();

    [DllImport("libc", EntryPoint = "getppid")]
    public static extern int parentId();

    public static int M()
    {
        return localPid();

        [DllImport("libc", EntryPoint = "getpid")]
        static extern int localPid();
    }

    public static int N()
    {
        return getuid();

        [DllImport("libc")]
        static extern int getuid();
    }
}
```

Before: `getpid` and `parentId` are reported by RH4103, `localPid` and `getuid` by RH4104; the fix is withheld for `getpid` and `getuid` (SHARED-FIX-003). After: `getpid` (RH4103) and `getuid` (RH4104) are not reported; `parentId` and `localPid` are reported as before. The same holds for a `partial` method declared with `[LibraryImport("libc")]`.

### 4. RH4116 — tuple element names that a type argument written in this compilation supplies

```csharp
// input
using System.Collections;
using System.Collections.Generic;

public class Pairs : IEnumerable<(string key, int value)>
{
    public IEnumerator<(string key, int value)> GetEnumerator()
    {
        yield break;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
```

Before (read from the analyzer, which checks tuple types by their syntax alone; not measured, RH4116 has no code fix): `key` and `value` are reported in the base list and in the signature of `GetEnumerator`. After: they are reported in the base list only; `GetEnumerator` implements `IEnumerable<T>.GetEnumerator`, which is declared outside the compilation.

### 5. Unchanged — RH4103, a base-class method that implements a referenced-assembly interface member only for a derived type

With a referenced assembly that declares `public interface IMeta { void process(); }`:

```csharp
// input
public class A
{
    public void process()
    {
    }
}

public class B : A, IMeta
{
}
```

Before and after: `A.process` is reported, and no fix is offered (SHARED-CASING-009).

## Decisions

### A member whose override chain reaches another compilation counts, at any level

`Leaf.process` in example 1 overrides `Worker.process`, which is declared in source, but `Worker.process` overrides `MetaBase.process`. Renaming `Leaf.process` alone breaks the override, and renaming the chain would have to rename `MetaBase.process`, which the fix cannot reach; so `Leaf.process` is dictated as much as `Worker.process`. This is the boundary the rule specs already drew as their coverage ("following the members it overrides, through every level").

### An explicit interface implementation keeps its tuple element names exempt only for an interface member outside the compilation

An explicit implementation is not reported by its name (RH4102-002, RH4103-002), but RH4116 reports the tuple element names in its signature. Those names are dictated by the interface member; when that member is declared in source, RH4116 reports it there too, and the implementation keeps today's behavior, as the issue asks for members declared in source.

### `[DllImport]` local functions are included

A local function declared with `[DllImport]` binds to its native entry point by its name exactly like a method, so the cause P2 names applies to it. RH4104 is not among the rules thoenissen/Reihitsu#1434 lists; the requirement states the boundary by its cause.

### Parameters and type parameters stay reported

The compiler does not require an override to repeat the names of its parameters or type parameters, so RH4105 and RH4121 keep reporting them; this is a non-goal of SHARED-CASING-014.

### The requirement takes the next free ID

thoenissen/Reihitsu#1434 proposes SHARED-CASING-011, which is taken; the requirement is SHARED-CASING-014.

### A member implements only for its own type

A member that becomes an implementation only because a derived type lists the interface is not dictated (output batch 1, question 11, answer A). `A` itself implements nothing, and the analyzer of `A` would have to search every derived type, which it cannot see in another project, so the IDE and the build could differ. The member stays reported, and SHARED-CASING-009 keeps withholding its fix.

### Tuple names are exempt in a dictated signature, whatever supplies them

The element names in the signature of a dictated member are not reported, also where a type argument written in this compilation supplies them (output batch 1, question 12, answer A). Those names can only change together with the type argument, which is reported where it is written, so each choice is reported once; tracing which name the other compilation writes and which a type argument supplies would need more machinery.

### Maintainer answers to output batch 1

The maintainer chose the recommended option for every question of this change:

- **Question 7 — names dictated by a referenced assembly: A.** None of the members of example 1 is reported, `Leaf.process` included, whose chain reaches `MetaBase.process` through `Worker.process`.
- **Question 8 — an override of a method declared in another project of the solution: A.** `Leaf.process` is not reported anywhere; `Base.process` is reported in its own project, and its fix renames both.
- **Question 9 — a `[DllImport]` local function without `EntryPoint`: A.** Not reported, like the same method under RH4103.
- **Question 11 — a base-class method that implements a referenced-assembly interface member only for a derived type: A.** Reported, with no fix, as today (example 5).
- **Question 12 — tuple element names that a type argument written in this compilation supplies: A.** Reported in the base list only (example 4).

## Alternatives considered

- **Keep reporting and only withhold the fix** — rejected: it leaves a diagnostic the author cannot resolve (P2).
- **Decide by whether the member is declared in the solution's source** — rejected: the analyzer sees only its compilation, and another project is source in the IDE but a referenced assembly in a command-line build, so the two would report different diagnostics.
- **Exempt only the members that override a referenced-assembly member directly** — rejected: an override of such an override has the same dictated name.
- **Exempt a member that implements an interface member on behalf of a derived type** — rejected by the maintainer (question 11): the member's own type implements nothing, and a derived type in another project is out of the analyzer's sight.
- **Exempt only the tuple names that the declaration in the other compilation writes** — rejected by the maintainer (question 12): the names supplied by a type argument would be reported twice, and telling them apart needs more machinery.

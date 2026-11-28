# RH4001 keeps a migration timestamp prefix; RH4009 compares nested namespaces by their full name

| | |
|---|---|
| Status | Accepted |
| Date | 2026-11 |
| Affected requirements | RH4001-006, RH4001-008 (new), RH4009-003 (new) |

## What

Two independent changes to two naming rules.

**RH4001.** When the file name up to its first dot starts with the 14-digit timestamp prefix of RH4001-002 — the prefix Entity Framework Core gives its migration files — the code fix keeps that prefix and replaces only the name behind it (RH4001-008). Today the fix drops the prefix, because RH4001-005 decides only file names without it and the prefixed case was left unspecified. RH4001-006 names the file name of RH4001-008 as the one checked for a collision, which is the name the fix produces.

**RH4009.** A block-scoped namespace declaration inside another block-scoped namespace declaration is compared with the allow-list by its full name: the names of every enclosing namespace declaration, outermost first, and its own name, joined by `.` (RH4009-003). Today it is compared by its own name only, because RH4009-002 decides only namespaces declared at file level. The diagnostic stays on the nested declaration's own name, and every enclosing declaration is still checked on its own.

## Why

RH4001 ignores the prefix when it compares, because Entity Framework Core names a migration's files after the migration's ID, the timestamp prefix followed by the migration's name. The prefix sorts the migration files in the order they were created and keeps each next to its `.Designer.cs` file, which carries the same prefix; a fix that drops it moves the file away from the other migrations and from its `.Designer.cs` file. The fix has to produce a name that RH4001 accepts and that keeps the file's role, as it already keeps the part behind the first dot. Entity Framework Core itself does not read file names — it orders migrations by the ID in their `[Migration]` attribute — so neither today's fix nor the new one changes what it does at runtime; the change is about the files in the project.

RH4009 pins the namespaces a project declares, and a nested declaration declares its full namespace: `namespace A { namespace B { } }` declares `A.B`, exactly as `namespace A.B { }` does. Comparing only `B` let `["A", "B"]` allow `A.B` and made `["A", "A.B"]` report it, which contradicts the rule page's statement that the full declared namespace is compared.

## Visible output changes

**1. RH4001 · The name of the type should match the filename — the code fix keeps the timestamp prefix.**

```csharp
// input, in a file 20210501083748_InitialCreate.cs
public class {|RH4001:SomeMigration|}
{
}
```

```csharp
// before: the fix renames the file to SomeMigration.cs
```

```csharp
// after: the fix renames the file to 20210501083748_SomeMigration.cs
```

The part behind the first dot is kept as before: `20210501083748_InitialCreate.Part1.cs` that declares `partial class SomeMigration` is renamed to `20210501083748_SomeMigration.Part1.cs` instead of `SomeMigration.Part1.cs`. The content of the file does not change. A file name without the prefix is renamed as before (RH4001-005).

The collision check of RH4001-006 follows the new name: a document `20210501083748_SomeMigration.cs` in the same folder withholds the fix, and a document `SomeMigration.cs` there, which withholds it today, no longer does.

**2. RH4009 · The given namespace is not allowed — a nested namespace is compared by its full name.**

```csharp
// input, with { "naming": { "allowedNamespaceDeclarations": [ "A", "A.B" ] } }
namespace A
{
    namespace B
    {
    }
}
```

```csharp
// before
namespace A
{
    namespace {|RH4009:B|}
    {
    }
}
```

```csharp
// after: not reported
```

```csharp
// input, with { "naming": { "allowedNamespaceDeclarations": [ "A", "B" ] } }
namespace A
{
    namespace B
    {
    }
}
```

```csharp
// before: not reported
```

```csharp
// after
namespace A
{
    namespace {|RH4009:B|}
    {
    }
}
```

**3. Rule pages.** `documentation/rules/RH4001.md` states that the code fix keeps the timestamp prefix. `documentation/rules/RH4009.md` states that a namespace declared inside another one is compared by its full name.

## Decisions

The maintainer decided the visible output changes 1 and 2 above in question round 3 (2026-10-18), recorded in issue #1085; change 3 restates those two decisions on the rule pages.

### The fix keeps the prefix

Maintainer, question round 3 (2026-10-18), recorded in issue #1085: for `20210501083748_InitialCreate.cs` holding `class SomeMigration`, the fix produces `20210501083748_SomeMigration.cs`. The prefix is kept exactly as written, and the rest of the new name is derived as RH4001-005 derives it, so a generic type or a part behind the first dot is handled the same way with and without the prefix.

### A nested namespace is compared by its full name

Maintainer, same round: `namespace A { namespace B { } }` is compared as `A.B`; with `["A", "A.B"]` nothing is reported, and `["A", "B"]` no longer allows it.

### Where the diagnostic stays, and what the enclosing declarations do

The diagnostic of a nested declaration stays on its own name, as today: that name is the part the nested declaration adds, and the enclosing names are reported, when at all, on their own declarations. Each enclosing declaration is still compared on its own — the outermost by RH4009-002, an intermediate one by RH4009-003 — so with `["A.B"]`, `namespace A { namespace B { } }` reports `A` and not `B`, as `A` is a namespace the file declares. Neither is a new output: the location is today's, and the outermost declaration's report is RH4009-002, unchanged.

## Alternatives considered

- **Withhold the RH4001 fix for a prefixed file name** — rejected: the maintainer decided the fix keeps the prefix, and the renamed file is a name RH4001 accepts.
- **Report a nested RH4009 diagnostic on the whole qualified span, from the outermost name** — rejected: that span crosses the enclosing declarations' bodies and other members, and the enclosing names have their own declarations to be reported on.
- **Allow an enclosing declaration implicitly when a nested full name is on the list** — rejected: a declaration of `A` declares `A`, and each allowed namespace is listed on its own (RH4009-002).

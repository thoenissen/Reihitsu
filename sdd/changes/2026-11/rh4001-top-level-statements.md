# RH4001 does not report a file with top-level statements

| | |
|---|---|
| Status | Draft |
| Date | 2026-11 |
| Affected requirements | RH4001-001, RH4001-007 |

## What

RH4001 no longer checks a file that contains top-level statements: a file whose compilation unit holds at least one global statement is not reported, whatever types it declares next to them, and so no rename is offered for it. A file without top-level statements is checked as before.

## Why

A file with top-level statements is the program's entry point, not the home of the types declared next to it. Today `Program.cs` with top-level statements and a helper class is reported, and the fix renames the entry point after the helper, to `Helper.cs` — a name that tells the reader nothing about the file's role. The maintainer decided this in question round 3 of #1011 (Q3, option a), recorded in #1086.

## Visible output changes

A diagnostic and its code-fix result. In a file `Program.cs`:

```csharp
using System;

Console.WriteLine(Helper.Greeting);

internal class Helper
{
    public const string Greeting = "Hello";
}
```

Before — reported, and the fix renames the file to `Helper.cs`:

```csharp
using System;

Console.WriteLine(Helper.Greeting);

internal class {|RH4001:Helper|}
{
    public const string Greeting = "Hello";
}
```

After — not reported; no fix is offered, and the file keeps its name:

```csharp
using System;

Console.WriteLine(Helper.Greeting);

internal class Helper
{
    public const string Greeting = "Hello";
}
```

The rule page does not change: it describes the normal case, a file that declares types, and a file of top-level statements is an exception the page does not list.

## Decisions

### The whole file is exempt, not only the types next to the statements

A file is exempt as soon as its compilation unit holds one global statement, also when its first type is declared inside a namespace after the statements. Every type in such a file sits next to the entry point, so none of them names the file.

### The exemption is decided from the syntax of the file alone

The analyzer looks for a global statement among the members of the file's compilation unit. A local function declared at the top level is a global statement as well. Whether the project is an executable, or another file of the project holds top-level statements, plays no part: a file without top-level statements is checked as before, also in a project whose `Program.cs` uses them.

## Alternatives considered

- **Keep reporting the first type** — rejected: the maintainer decided against it (#1011 Q3); renaming the entry point after a helper type hides its role.
- **Expect the name `Program` for a file with top-level statements** — rejected: nothing requires the entry-point file to be named `Program.cs`, and the decision was not to report such a file at all.

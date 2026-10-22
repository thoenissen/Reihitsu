# Shared — Using directive ordering

| | |
|---|---|
| ID prefix | SHARED-USING |
| Status | Active |
| Code owners | `UsingDirectiveOrderingUtilities`, `UsingDirectiveOrderingSafety`, `UsingDirectiveOrderingGroup` |
| Uses | — |

This spec covers the using-directive policy that the analyzer and the formatter must agree on: which using blocks may be reordered at all, how directives are grouped, the order of the groups, and the order within a group. The analyzer rules RH7201 (System directives first), RH7202 (alias directives last), RH7203 (alphabetical by namespace), RH7204 (aliases alphabetical by alias name), RH7205 (static directives between regular and alias directives), RH7206 (static directives alphabetical), and RH7207 (organized into groups) report against this policy; their code fixes and the formatter's FMT-USING phase produce it.

## Non-goals

- How the formatter rebuilds the trivia of a reordered block — header comments, attached comments, trailing comments, and one directive per line. That is formatter-only and specified in FMT-USING.
- Which directive a rule reports and where — each rule's own spec.
- Placement of using directives inside or outside a namespace, and removal of unused or duplicate directives.

## Requirements

### SHARED-USING-001 — Every using block is ordered on its own

Analyzer and formatter MUST treat the using directives of the compilation unit, of each block-scoped namespace, and of each file-scoped namespace as separate blocks, and MUST NOT move a directive from one block into another. A block with fewer than two directives has no order to check.

Why: A using directive's meaning depends on the scope it is declared in; moving it between scopes can change name lookup. Every rule and the formatter therefore look at one block at a time.

Example:

```csharp
using System.Linq;
using System;

namespace N
{
    using Reihitsu.Core;
    using System.Text;

    class C
    {
        int x;
    }
}
```

```csharp
using System;
using System.Linq;

namespace N
{
    using System.Text;

    using Reihitsu.Core;

    class C
    {
        int x;
    }
}
```

### SHARED-USING-002 — Global directives come first

Within a block, analyzer and formatter MUST place every `global using` directive before every non-global directive, regardless of kind or namespace. Global and non-global directives MUST NOT share a group (SHARED-USING-004).

Why: Global usings apply to the whole compilation and are a different kind of declaration from the file's own imports; keeping them in a leading section makes them easy to find and keeps a local `System` directive from being sorted between them.

Example:

```csharp
using System;
global using Microsoft.CodeAnalysis;
global using System.Linq;

class C
{
    int x;
}
```

```csharp
global using System.Linq;

global using Microsoft.CodeAnalysis;

using System;

class C
{
    int x;
}
```

### SHARED-USING-003 — Regular, then static, then alias directives

Within the global and within the non-global section, analyzer and formatter MUST order the directive kinds as regular namespace directives first, then `using static` directives, then alias directives (`using A = …;`), as classified by `UsingDirectiveOrderingGroup`.

Why: This is the order most C# style guides and IDE "sort usings" commands use, so code organized by Reihitsu reads the way users expect. RH7202, RH7205, and RH7207 report against it.

Example:

```csharp
using static System.Math;
using A = System.Text;
using System;

class C
{
    int x;
}
```

```csharp
using System;

using static System.Math;

using A = System.Text;

class C
{
    int x;
}
```

### SHARED-USING-004 — A group is section, kind, and root namespace

Two directives MUST be in the same group exactly when they agree in all three of: global or not (SHARED-USING-002), kind (SHARED-USING-003), and root namespace. The root namespace is the first segment of the directive's name after a leading `global::` qualifier is removed, compared ordinally (case-sensitive). For an alias directive, the name is the alias target, not the alias; for a `using static` directive, it is the type name. An alias whose target is not a name — a predefined type such as `int`, a tuple, an array, a pointer, or a nullable type — has no root namespace: it MUST form a group of its own, and that group MUST come after every other group of its section.

This requirement covers a section with at most one alias whose target is not a name.

Why: Grouping by root namespace keeps everything from one library together (`Microsoft.*`, `Reihitsu.*`) and is what RH7207 checks blank lines against. Grouping aliases by their target makes an alias sit next to the library it points into rather than in one alphabetical list of unrelated names. An alias of a type that is not a name points into no library, so it belongs to none of the groups and is placed after all of them, where it cannot be mistaken for the `System` group.

Example:

```csharp
using Zeta = Microsoft.CodeAnalysis.SyntaxNode;
using static System.Math;
using Reihitsu.Core;
using Alpha = System.Text.StringBuilder;
using Microsoft.CodeAnalysis;
using System.Linq;
using static Reihitsu.Core.CasingUtilities;
using System;

class C
{
    int x;
}
```

```csharp
using System;
using System.Linq;

using Microsoft.CodeAnalysis;

using Reihitsu.Core;

using static System.Math;

using static Reihitsu.Core.CasingUtilities;

using Alpha = System.Text.StringBuilder;

using Zeta = Microsoft.CodeAnalysis.SyntaxNode;

class C
{
    int x;
}
```

Example — an alias of `int` forms its own group after the alias group of `System`:

```csharp
using System;

using B = int;

using A = System.Text;

class C
{
    int x;
}
```

```csharp
using System;

using A = System.Text;

using B = int;

class C
{
    int x;
}
```

### SHARED-USING-005 — The `System` root comes first

Within each section and kind, analyzer and formatter MUST place the group whose root namespace is exactly `System` (ordinal, after removing `global::`) before every other group. A root that differs from `System` only in case (`SYSTEM`, `system`) or that merely starts with it (`Systematic`) MUST be treated as an ordinary root.

Why: The base class library is imported by almost every file, and listing it first is the long-standing .NET convention RH7201 enforces. The comparison is exact because C# namespaces are case-sensitive: `SYSTEM.Text` is a different namespace, and treating it as `System` would merge unrelated imports into one group.

Example:

```csharp
using System.Linq;
using SYSTEM.Text;
using System;

class C
{
    int x;
}
```

```csharp
using System;
using System.Linq;

using SYSTEM.Text;

class C
{
    int x;
}
```

### SHARED-USING-006 — Other roots are ordered alphabetically

After the `System` group, analyzer and formatter MUST order the remaining groups of a section and kind by root namespace, compared case-insensitively (ordinal), and MUST break a tie between roots that differ only in case ordinally, so that two such roots stay separate, contiguous groups.

Why: Case-insensitive comparison gives the alphabetical order a reader expects; the ordinal tie-break makes the order deterministic and keeps `Foo` and `foo` — different namespaces — from interleaving.

Example:

```csharp
using Systematic;
using system.Text;
using foo.Bar;
using Foo.Baz;
using System;

class C
{
    int x;
}
```

```csharp
using System;

using Foo.Baz;

using foo.Bar;

using system.Text;

using Systematic;

class C
{
    int x;
}
```

### SHARED-USING-007 — Alphabetical order within a group, stable on ties

Within a group, analyzer and formatter MUST order directives by their sort key — the alias name for an alias directive, otherwise the imported namespace or type name — compared case-insensitively (ordinal). Directives whose keys compare equal MUST keep their source order.

A rule that reports a directive out of this order MUST compare the directive with every earlier directive of its group in the block, whatever directives of other groups or kinds lie between them, and MUST report it when its sort key comes before the key of any of them. Every other rule that checks the order of a sequence of elements MUST compare an element in the same way: with every earlier element of its group, whatever lies between them.

Why: RH7203, RH7204, and RH7206 report directives out of this order. A stable order on ties means two directives that differ only in case are never swapped back and forth, so the analyzer and the formatter both converge. Whether an element is in order is a question about its group alone: an element of another group between two elements of one group does not put them in order, and an element that sorts before an earlier element of its group is out of place even when the element right before it sorts lower.

Example — aliases are grouped by target root (SHARED-USING-004) and sorted by alias name within each group:

```csharp
using Zeta = System.Text.StringBuilder;
using Beta = Microsoft.CodeAnalysis.SyntaxNode;
using Alpha = Microsoft.CodeAnalysis.SyntaxToken;
using Gamma = System.Console;

class C
{
    int x;
}
```

```csharp
using Gamma = System.Console;
using Zeta = System.Text.StringBuilder;

using Alpha = Microsoft.CodeAnalysis.SyntaxToken;
using Beta = Microsoft.CodeAnalysis.SyntaxNode;

class C
{
    int x;
}
```

Example — `System.collections` and `System.Collections` compare equal and keep their source order:

```csharp
using System.Xml;
using System.collections;
using System.Collections;

class C
{
    int x;
}
```

```csharp
using System.collections;
using System.Collections;
using System.Xml;

class C
{
    int x;
}
```

Example — `Microsoft.A` is compared with `Microsoft.B`, although a directive of the `System` group lies between them:

```csharp
using Microsoft.B;
using System;
using {|RH7203:Microsoft.A|};

class C
{
    int x;
}
```

Example — `Microsoft.B` comes after `Microsoft.A`, but before the earlier `Microsoft.C`:

```csharp
using Microsoft.C;
using {|RH7203:Microsoft.A|};
using {|RH7203:Microsoft.B|};

class C
{
    int x;
}
```

Example — RH7110 compares `D` with every earlier field, so it is reported after the `private` field `_a`, although the `public` field right before it comes earlier in the accessibility order:

```csharp
class C
{
    private int _a;
    public int {|RH7110:B|};
    internal int {|RH7110:D|};
}
```

### SHARED-USING-008 — A blank line between groups, none within a group

In an organized block, a directive that starts a new group (SHARED-USING-004) MUST be preceded by a blank line, and a directive that continues the group of its predecessor MUST NOT be. Only the line breaks before the first comment that leads the directive count; a comment attached to the directive stays directly above it.

Why: The blank line makes the group structure visible, and RH7207 checks exactly this. Counting only the space before an attached comment lets a group-starting directive keep its comment while still being separated from the group above.

Example:

```csharp
using System;

using System.Linq;
using Microsoft.CodeAnalysis;

class C
{
    int x;
}
```

```csharp
using System;
using System.Linq;

using Microsoft.CodeAnalysis;

class C
{
    int x;
}
```

### SHARED-USING-009 — Blocks with preprocessor directives are not reordered

Analyzer and formatter MUST NOT reorder a using block — and the RH7201–RH7207 analyzers MUST NOT report on it, nor their code fixes be offered — when the block contains a preprocessor directive (`#if`, `#nullable`, `#pragma`, `#region`, …) anywhere in the leading or trailing trivia of its directives, including the leading trivia of its first directive. A preprocessor directive that follows the last using directive belongs to the next declaration and does not make the block unsafe.

Why: Moving a using across a `#if` changes which directives are compiled, and moving it across `#nullable` or `#pragma` changes the context it is compiled in. Reporting an order the fix and the formatter are not allowed to produce would leave the user with a diagnostic they cannot resolve automatically, so the analyzers stay silent on exactly the blocks the formatter leaves alone.

Example:

```csharp
using System.Linq;
#if DEBUG
using System.Diagnostics;
#endif
using System;

class C
{
    int x;
}
```

is left unchanged, and so is:

```csharp
#nullable enable
using System.Linq;
using System;

class C
{
    int x;
}
```

while a directive after the block does not count:

```csharp
using System.Linq;
using System;
#if DEBUG
class D
{
    int y;
}
#endif

class C
{
    int x;
}
```

```csharp
using System;
using System.Linq;
#if DEBUG
class D
{
    int y;
}
#endif

class C
{
    int x;
}
```

### SHARED-USING-010 — A split-off directive takes the scope's indentation

When a directive that shared a line with its predecessor is moved onto a line of its own — by the RH5114 code fix or by the formatter (FMT-USING) — it MUST be placed at the indentation of the line on which the block's first directive starts. When that line begins inside a comment that started on an earlier line, the indentation of the line on which the comment starts MUST be used instead.

Why: The whitespace in front of a directive that did not start its line is the gap after the previous token, not an indentation, so it cannot be kept. The first directive's line is the best evidence of how the user indents the block. In the formatter's final output the indentation phase (FMT-INDENT) re-indents every line anyway, so this rule is visible only in the RH5114 code fix and in the output of the using-directive phase on its own.

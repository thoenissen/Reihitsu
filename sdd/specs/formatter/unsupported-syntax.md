# Formatter — Unsupported syntax

| | |
|---|---|
| ID prefix | FMT-UNSUP |
| Status | Active |
| Code owners | `PrecheckPhase`, `FormattingResult<T>`, `UnsupportedSyntax`, `SyntaxNotSupportedException` |
| Uses | FMT-PIPE-001, FMT-PIPE-004, FMT-PIPE-005, FMT-PIPE-006, FMT-PIPE-009, FMT-PIPE-017, FMT-PIPE-030, FMT-PIPE-031, FMT-PIPE-032, FMT-STRUCT-021, SHARED-FIX-010 |

This spec covers the formatter's *Unsupported* outcome: the input shapes the formatter refuses as a whole, the precheck phase that finds them before any other phase runs (FMT-PIPE-001), a phase's refusal of a shape the precheck does not find, what every entry point returns for a refused input, and the query that tells a code fix whether the formatter refuses a node. Each unsupported shape is a requirement of this spec; a shape is declared here so that no phase has to handle it.

## Non-goals

- What a host does with the outcome — the CLI (CLI specs), the code fixes (SHARED-FIX), and the Playground (PLAY).
- Whether an analyzer rule reports an unsupported shape. The list of unsupported shapes belongs to the formatter and depends on no analyzer rule.
- Formatting part of a refused input. A refused input is never formatted in part.
- Code with syntax errors and generated code, which FMT-PIPE-005 and FMT-PIPE-006 leave untouched before this spec applies.

## Requirements

### FMT-UNSUP-001 — An unsupported input is refused as a whole

When the input of the formatting phases contains a shape that a requirement of this spec declares unsupported, the formatter MUST NOT format any part of it: the entry point MUST return its input unchanged, and MUST report the input as unsupported, with the reason — the unsupported shape — and the location of the shape. The phases after the precheck MUST NOT run on a refused input.

Every formatting entry point of `ReihitsuFormatter` — `FormatDocumentAsync`, `FormatSyntaxTree`, `FormatNode`, `FormatNodeInDocumentAsync`, and `FormatNodeInDocumentWithContextAsync` — MUST return a result object, `FormattingResult<T>`, instead of the bare document, tree, or node:

- `Output` is what the entry point returned before this outcome existed: the formatted document, tree, or node, or the input unchanged when the formatter refuses it, and also when FMT-PIPE-005 or FMT-PIPE-006 leaves it untouched.
- `Unsupported` is null when the input is not refused, also for an input FMT-PIPE-005 or FMT-PIPE-006 leaves untouched. For a refused input it is an `UnsupportedSyntax` that carries the reason — the description the requirement of the shape gives, such as `a comment between a variable declaration's own tokens` for FMT-UNSUP-004 — and the location: the span of the shape in the caller's syntax tree, from which a host derives the line and the column of its first character.

When the input holds several unsupported shapes, the reason and the location MUST be those of the shape that starts first in the text.

`OrganizeUsingDirectivesInDocumentAsync` and `FormatDocumentationElementInDocumentAsync` refuse no input (FMT-UNSUP-002) and return the document (FMT-PIPE-030, FMT-PIPE-032); `SplitsFieldDeclaration` answers a question and keeps its return type (FMT-PIPE-031).

Why: Rare comment and directive shapes have cost analysis, spec text, and special cases in the phases, and many of them ended in leaving the construct as written anyway, silently and decided again for every phase. Refusing the whole input removes the shape from every phase at once: a phase that never sees it needs no guard for it. Formatting the rest of the input would put the phases back next to the shape, and they would need their guards again. Reporting the refusal tells the user why the input was left as written, which the silent outcome did not. A result object makes the outcome part of every call: no caller can format and overlook a refusal, and the CLI, the Playground, and the code fixes all have to report it or withhold on it. One location per input keeps the report one line; the first shape in the text is the one a user meets first, and after rewriting it the next run names the next one.

Example — `FormatSyntaxTree` with

```csharp
class C{
    void M(){
        int /* c */ a=1;
    }
}
```

returns a result whose `Output` is the tree unchanged, also the brace and the spacing the formatter would correct elsewhere, and whose `Unsupported` names the reason `a comment between a variable declaration's own tokens` (FMT-UNSUP-004) and the span of `/* c */`, which starts in line 3, column 13.

Example — in

```csharp
class C
{
    void M()
    {
        int b = /* second */ 1;
        int /* first */ a = 1;
    }
}
```

the location is that of `/* second */`, the shape that starts first in the text.

Example — `FormatSyntaxTree` with `class C{ int x }`, which has a syntax error, returns a result whose `Output` is the tree unchanged and whose `Unsupported` is null (FMT-PIPE-005).

### FMT-UNSUP-002 — The precheck runs on exactly what the phases receive

The formatter MUST decide whether an input is unsupported on exactly the node its formatting phases receive, before the first of them runs (FMT-PIPE-001), so that every entry point gives the same answer for the same node:

- `FormatDocumentAsync` and `FormatSyntaxTree` — the root of the syntax tree;
- `FormatNode` — the node it is given;
- `FormatNodeInDocumentAsync` — its target;
- `FormatNodeInDocumentWithContextAsync` — its context, with the target in it.

`OrganizeUsingDirectivesInDocumentAsync` runs no formatting phase but the using phase (FMT-PIPE-030) and MUST NOT refuse an input under this spec. `FormatDocumentationElementInDocumentAsync` lays out one documentation element and runs no formatting phase on the document (FMT-PIPE-032), and MUST NOT refuse an input under this spec either: no shape this spec declares can stand inside a documentation element. `SplitsFieldDeclaration` answers a question and formats nothing (FMT-PIPE-031). An input that FMT-PIPE-005 or FMT-PIPE-006 returns unchanged is not reported as unsupported: those requirements apply first.

Why: One check site gives one answer. A second check in an entry point would restate the list of unsupported shapes, and the two statements could drift apart. Checking the context of `FormatNodeInDocumentWithContextAsync`, not only its target, follows from what its phases receive: they format the whole context, so an unsupported shape anywhere in it reaches them.

Example — in

```csharp
class C
{
    void M()
    {
        int /* c */ a = 1;
    }

    void N(){ }
}
```

`FormatNodeInDocumentAsync` with the method `N` as target formats `N`, while with the method `M` as target it returns the document unchanged and reports it as unsupported. `FormatNodeInDocumentWithContextAsync` with `N` as target and the class `C` as context returns the document unchanged and reports it as unsupported, because the context holds the declaration in `M`.

### FMT-UNSUP-003 — The decision is local to the input

The formatter MUST decide whether an input is unsupported from the tokens and the trivia inside that input alone. Text outside the input, and the node the input stands in, MUST NOT decide it: a shape that a requirement of this spec names by its syntax counts only where that syntax is part of the input.

So whole-document formatting and node-level formatting agree for the same node, but not between a document and a node below the shape: a target that holds part of a refused declaration, but not the declaration, is formatted, while the document that holds it is refused.

Why: A decision that read the code around a node-level input would give a node one answer as part of the document and another as a target. A node-level input is formatted as a root of its own (FMT-PIPE-017), and its phases see nothing else either.

Example — `FormatNodeInDocumentAsync` with the equals-value clause `= /* a */ new List<int> { 1 }` of

```csharp
using System.Collections.Generic;

class C
{
    void M()
    {
        var d = /* a */ new List<int> { 1 };
    }
}
```

as target formats the clause: the clause holds the `=` and the comment, but not the variable declaration that FMT-UNSUP-004 names. With the local declaration statement as target, the input holds the declaration, and the document is returned unchanged and reported as unsupported; `FormatDocumentAsync` refuses the document as well.

### FMT-UNSUP-004 — Comments inside a variable declaration are unsupported

An input MUST be refused when a comment — `//`, `/* */`, `///`, or `/** */`, in every documentation mode — stands in the trivia of a token in the span of a variable declaration, other than in the places excepted below.

The span of a variable declaration runs from its first token through its terminator, and every token in between belongs to it — attribute lists with their arguments, modifiers, every token of the type, each declarator's name, the brackets and the size of a fixed-size buffer, each `=`, and each `,` between declarators:

- a field or an event field: from the member's first token — its first attribute list, its first modifier, or its type — through the `;` that ends it;
- a local declaration statement, a `using` declaration included: from the statement's first token — such as `using`, `await`, `const`, or the type — through its `;`;
- the initializer of a `for` statement: from the type's first token through the `for` statement's first `;`;
- a `using` statement or a `fixed` statement: from the type's first token through the `)` behind the last declarator. The statement's keyword and its `(` are not in the span.

These places are excepted:

- the leading trivia of the span's first token: a comment above the declaration, or in front of it on its line;
- the trailing trivia of the `;` or `)` that ends the span: a comment behind the declaration;
- inside an initializer value: the trivia between the value's first token and its last token — from the trailing trivia of its first token through the leading trivia of its last token. Neither the leading trivia of the value's first token nor the trailing trivia of its last token is excepted: a comment in front of the value, also on its own line, and a comment between the value and the `,`, `;`, or `)` behind it refuse the input. A value of one token has no inside.

A variable declaration inside an initializer value, such as a local declared in a lambda body, is a declaration of its own: this requirement applies to it with its own span, whatever the declaration around it holds.

The reason the formatter gives for this shape is `a comment between a variable declaration's own tokens`, and its location is the span of the comment.

Why: A comment inside a declaration makes every phase that rewrites or lays out the declaration — the field split, which repeats the attributes, modifiers, and type on every field, the line break at `=`, the spacing around `=` and `,` — carry the comment along and decide where it goes, and each of them needed special cases for it. The shape is rare, and refusing it lets those special cases go. Refusing the whole declaration, not only the trivia of some of its tokens, makes the boundary one check from the first token through the terminator, with no carve-out for the attributes, their arguments, the modifiers, or the type, so no phase keeps a guard for a comment anywhere among them. The value's inside ends where its last token ends: a comment between the value and the `,` or `;` behind it is refused like one in front of the `;`, so it makes no difference whether the parser attaches it to the value or to the `;` — `int a = 1 /** c */;` is refused whether documentation comments are parsed or not. The `)` of a `using` or `fixed` statement ends its declaration as the `;` ends a local's, and a comment in front of it is refused like one in front of the `;`. A comment above or in front of the declaration, behind its `;` or `)`, or inside a value stands where the phases already handle comments for every other statement, member, and expression.

Example — each declaration is refused, and the input that holds it is returned unchanged:

```csharp
class C
{
    void M()
    {
        int /* c1 */ a = 1;
        int b /* c2 */ = 1;
        int c = // c3
            1;
        int d = 1, /* c4 */ e = 2;
        int f, // c5
            g;
    }
}
```

Example — a comment among the attributes or in an attribute's arguments, among the modifiers, or in the type, on its own line in front of the value, or on its own line in front of the `;`; each declaration is refused:

```csharp
using System;

class C
{
    System.Collections.Generic.Dictionary<int, // first
        string> _map;

    private
        // c
        int _b;

    [Obsolete /* why */]
    private int _c;

    [Obsolete("old" /* why */)]
    private int _e;

    private /* c */ static int _d;

    void M()
    {
        int c =
            // c
            1;
        int d
            // c
            ;
    }
}
```

Example — a comment between a value and the `,`, `;`, or `)` behind it, and a comment on its own line in front of the `)` of a `using` statement; each declaration is refused:

```csharp
class C
{
    void M()
    {
        var timeout = 30 /* seconds */;
        int b = 1 /* c */, c = 2;

        using (var s = Open() /* c */)
        {
        }

        using (var t = Open()
            // c
            )
        {
        }
    }

    System.IDisposable Open() => null;
}
```

Example — the other side: none of these comments refuses the input, and the input is formatted:

```csharp
using System;

class C
{
    /// <summary>The count</summary>
    private int _count;

    void M()
    {
        // c6
        int a = 1; // c7

        var o = new Options
        {
            // c8
            Retries = 3,
        };
        Func<int> f = () =>
        {
            // c9
            return 1;
        };
        int b = Compute(1, /* c10 */ 2);

        using (var s = Open()) // c11
        {
        }
    }

    IDisposable Open() => null;
}
```

Example — a declaration inside a value is checked on its own: the lambda's local refuses the input, although the comment is inside the value of `f`:

```csharp
using System;

class C
{
    void M()
    {
        Func<int> f = () =>
        {
            int /* c */ x = 1;
            return x;
        };
    }
}
```

Example — the requirement holds for every variable declaration; each of these is refused:

```csharp
using System;

unsafe struct S
{
    private int _a, /* c */ _b;
    public event Action /* c */ Changed;
    public fixed int _buffer[4] /* c */;

    void M(int[] items)
    {
        for (int i = 0, /* c */ j = 0; i < j; i++)
        {
        }

        using var /* c */ stream = Open();

        fixed (int* /* c */ p = items)
        {
        }
    }
}
```

### FMT-UNSUP-005 — Formatted output is never unsupported

The formatter MUST NOT produce a shape that a requirement of this spec declares unsupported: the output of an input it formats MUST NOT be refused when it is formatted again.

Why: A phase that wrote an unsupported shape would hand the later phases the shape they no longer guard against, and a second run would refuse what the first one wrote, which breaks idempotency (FMT-PIPE-004).

Example — the field split (FMT-STRUCT-021) keeps a comment inside an initializer value inside that value, where no requirement of this spec refuses it:

```csharp
class C
{
    private int _a = 1 + /* c */ 2, _b;
}
```

```csharp
class C
{
    private int _a = 1 + /* c */ 2;
    private int _b;
}
```

### FMT-UNSUP-006 — A phase's refusal gives the same outcome

When a phase refuses an input during the run, for a shape that the precheck does not find, every public entry point of `ReihitsuFormatter` MUST return its input unchanged and report it as unsupported, as FMT-UNSUP-001 states for the precheck's refusal; the refusal MUST NOT reach the caller as an exception. Every shape a phase refuses this way MUST be stated by a requirement of this spec, which names the phase.

The formatter MUST turn a phase's refusal into the outcome in one place, where the pipeline run that every entry point uses produces the precheck's outcome, also when a pipeline observer or the trace watches the run: the refusal MUST NOT reach the observer's caller as an exception either, wrapped in another exception or not.

No phase refuses an input in this way yet.

Why: Some shapes are cheaper to recognize where a phase meets them than ahead of all phases; the outcome must not depend on where the shape was found. An exception that escaped would abort the CLI's run and surface as `AD0001` in a code fix. Producing both refusals in one place gives one outcome site, and it lets a test run the pipeline with a phase that refuses. Roslyn's syntax trees are immutable, so stopping in the middle of the phases cannot leave a partly formatted result behind. A refusal stated in no requirement would be a special case nobody can find.

### FMT-UNSUP-007 — The formatter answers whether it refuses a node

`ReihitsuFormatter.FindUnsupportedSyntax` MUST return, for the node it is given, the `UnsupportedSyntax` that the precheck gives for that node as the input of the phases — the same reason and location (FMT-UNSUP-001) — or null when the precheck does not refuse it. It MUST decide by the same predicate the precheck runs, not by a copy of it, and MUST NOT format the node. It answers for the node alone (FMT-UNSUP-003): text outside the node does not decide it.

This requirement decides the inputs the formatting code fixes pass (`AGENTS.md`, *Supported inputs*): the node, or the context, a fix formats, which SHARED-FIX-010 states.

Why: A code fix has to know when it registers whether the formatter will refuse its formatting, because an action that changes nothing or leaves its edit unformatted is no fix (SHARED-FIX-010). Code fixes reach the formatter only through `ReihitsuFormatter` (FMT-PIPE-009), so the answer is one of its entry points. One predicate for the precheck and the query means one statement of every unsupported shape; a copy in the code fixes could drift apart from it. Formatting the node at registration to read the outcome would format a whole type for each RH7101 diagnostic shown in the light bulb.

Example — for

```csharp
class C
{
    private int _a, _b;

    void M()
    {
        int /* c */ x = 1;
    }
}
```

`FindUnsupportedSyntax` returns the reason `a comment between a variable declaration's own tokens` and the span of `/* c */` for the class `C` and for the method `M`, and null for the field declaration `_a, _b`.

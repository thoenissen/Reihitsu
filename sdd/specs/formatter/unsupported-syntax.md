# Formatter — Unsupported syntax

| | |
|---|---|
| ID prefix | FMT-UNSUP |
| Status | Active |
| Code owners | — |
| Uses | FMT-PIPE-001, FMT-PIPE-004, FMT-PIPE-005, FMT-PIPE-006, FMT-PIPE-017, FMT-PIPE-030, FMT-PIPE-031, FMT-STRUCT-021 |

This spec covers the formatter's *Unsupported* outcome: the input shapes the formatter refuses as a whole, the precheck phase that finds them before any other phase runs (FMT-PIPE-001), a phase's refusal of a shape the precheck does not find, and what every entry point returns for a refused input. Each unsupported shape is a requirement of this spec; a shape is declared here so that no phase has to handle it.

## Non-goals

- What a host does with the outcome — the CLI (CLI specs), the code fixes (SHARED-FIX), and the Playground (PLAY).
- Whether an analyzer rule reports an unsupported shape. The list of unsupported shapes belongs to the formatter and depends on no analyzer rule.
- Formatting part of a refused input. A refused input is never formatted in part.
- Code with syntax errors and generated code, which FMT-PIPE-005 and FMT-PIPE-006 leave untouched before this spec applies.

## Requirements

### FMT-UNSUP-001 — An unsupported input is refused as a whole

When the input of the formatting phases contains a shape that a requirement of this spec declares unsupported, the formatter MUST NOT format any part of it: the entry point MUST return its input unchanged, and MUST report the input as unsupported, with the reason — the unsupported shape — and the location of the shape. The phases after the precheck MUST NOT run on a refused input.

This requirement does not decide how an entry point represents the outcome to its caller.

Why: Rare comment and directive shapes have cost analysis, spec text, and special cases in the phases, and many of them ended in leaving the construct as written anyway, silently and decided again for every phase. Refusing the whole input removes the shape from every phase at once: a phase that never sees it needs no guard for it. Formatting the rest of the input would put the phases back next to the shape, and they would need their guards again. Reporting the refusal tells the user why the input was left as written, which the silent outcome did not.

Example — `FormatSyntaxTree` with

```csharp
class C{
    void M(){
        int /* c */ a=1;
    }
}
```

returns the tree unchanged, also the brace and the spacing the formatter would correct elsewhere, and reports it as unsupported, at the comment `/* c */`, for a comment between a variable declaration's own tokens (FMT-UNSUP-004).

### FMT-UNSUP-002 — The precheck runs on exactly what the phases receive

The formatter MUST decide whether an input is unsupported on exactly the node its formatting phases receive, before the first of them runs (FMT-PIPE-001), so that every entry point gives the same answer for the same node:

- `FormatDocumentAsync` and `FormatSyntaxTree` — the root of the syntax tree;
- `FormatNode` — the node it is given;
- `FormatNodeInDocumentAsync` — its target;
- `FormatNodeInDocumentWithContextAsync` — its context, with the target in it.

`OrganizeUsingDirectivesInDocumentAsync` runs no formatting phase but the using phase (FMT-PIPE-030) and MUST NOT refuse an input under this spec. `SplitsFieldDeclaration` answers a question and formats nothing (FMT-PIPE-031). An input that FMT-PIPE-005 or FMT-PIPE-006 returns unchanged is not reported as unsupported: those requirements apply first.

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

Why: A decision that read the code around a node-level input would give a node one answer as part of the document and another as a target, and whole-document and node-level formatting would disagree about what they refuse. A node-level input is formatted as a root of its own (FMT-PIPE-017), and its phases see nothing else either.

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

as target formats the clause: the clause holds the `=` and the comment, but not the variable declaration that FMT-UNSUP-004 names. With the local declaration statement as target, the input holds the declaration, and the document is returned unchanged and reported as unsupported.

### FMT-UNSUP-004 — Comments between a variable declaration's own tokens are unsupported

An input MUST be refused when a comment — `//`, `/* */`, `///`, or `/** */` — stands in the trivia of one of these tokens of a variable declaration:

- between the type and the first declarator's name: behind the type's last token, or in front of the first declarator's name;
- in the trivia of a declarator's name, in front of it or behind it, also when no `=` follows;
- in the trivia of the `=` of a declarator's initializer, in front of it or behind it;
- in the trivia of a `,` between two declarators, in front of it or behind it.

This holds for every variable declaration: of a local, a field, an event field, a `using` statement or declaration, a `fixed` statement, and the initializer of a `for` statement.

These comments MUST NOT refuse an input:

- a comment in the leading trivia of the declaration's first token, above the declaration or in front of it on its line;
- a comment behind the `;` that ends the declaration;
- a comment inside an initializer value, such as one inside an object initializer or a lambda body, or one behind the value's last token.

This requirement specifies the outcome only for the trivia it names and leaves every other comment of a variable declaration unspecified.

Why: A comment between the tokens of one declaration makes every phase that rewrites or lays out the declaration — the field split, the line break at `=`, the spacing around `=` and `,` — carry the comment along and decide where it goes, and each of them needed special cases for it. The shape is rare, and refusing it lets those special cases go. A comment above or in front of the declaration, behind its `;`, or inside a value stands where the phases already handle comments for every other statement, member, and expression.

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

Example — the other side: none of these comments refuses the input, and the input is formatted:

```csharp
using System;

class C
{
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
        int b = 1 /* c10 */, c = 2;
    }
}
```

Example — the requirement holds for every variable declaration; each of these is refused:

```csharp
using System;

class C
{
    private int _a, /* c */ _b;
    public event Action /* c */ Changed;

    void M()
    {
        for (int i = 0, /* c */ j = 0; i < j; i++)
        {
        }
    }
}
```

### FMT-UNSUP-005 — Formatted output is never unsupported

The formatter MUST NOT produce a shape that a requirement of this spec declares unsupported: the output of an input it formats MUST NOT be refused when it is formatted again.

Why: A phase that wrote an unsupported shape would hand the later phases the shape they no longer guard against, and a second run would refuse what the first one wrote, which breaks idempotency (FMT-PIPE-004).

Example — the field split (FMT-STRUCT-021) keeps a comment behind an initializer value inside that value, where no requirement of this spec refuses it:

```csharp
class C
{
    private int _a = 1 /* c */, _b;
}
```

```csharp
class C
{
    private int _a = 1 /* c */;
    private int _b;
}
```

### FMT-UNSUP-006 — A phase's refusal gives the same outcome

When a phase refuses an input during the run, for a shape that the precheck does not find, every public entry point of `ReihitsuFormatter` MUST return its input unchanged and report it as unsupported, as FMT-UNSUP-001 states for the precheck's refusal; the refusal MUST NOT reach the caller as an exception. Every shape a phase refuses this way MUST be stated by a requirement of this spec, which names the phase.

No phase refuses an input in this way yet.

Why: Some shapes are cheaper to recognize where a phase meets them than ahead of all phases; the outcome must not depend on where the shape was found. An exception that escaped would abort the CLI's run and surface as `AD0001` in a code fix. Roslyn's syntax trees are immutable, so stopping in the middle of the phases cannot leave a partly formatted result behind. A refusal stated in no requirement would be a special case nobody can find.

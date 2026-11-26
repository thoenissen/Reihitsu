# Formatter — Unsupported syntax

| | |
|---|---|
| ID prefix | FMT-UNSUP |
| Status | Active |
| Code owners | `PrecheckPhase`, `FormattingResult<T>`, `UnsupportedSyntax`, `SyntaxNotSupportedException` |
| Uses | FMT-BREAK-001, FMT-BREAK-014, FMT-BREAK-015, FMT-BREAK-016, FMT-BREAK-017, FMT-BREAK-019, FMT-BREAK-020, FMT-BREAK-021, FMT-BREAK-026, FMT-BREAK-039, FMT-BREAK-040, FMT-BREAK-041, FMT-INDENT-013, FMT-INDENT-015, FMT-INDENT-034, FMT-PIPE-001, FMT-PIPE-004, FMT-PIPE-005, FMT-PIPE-006, FMT-PIPE-009, FMT-PIPE-017, FMT-PIPE-030, FMT-PIPE-031, FMT-PIPE-032, FMT-PIPE-033, FMT-SPACE-028, FMT-STRUCT-004, FMT-STRUCT-005, FMT-STRUCT-007, FMT-STRUCT-008, FMT-STRUCT-009, FMT-STRUCT-012, FMT-STRUCT-014, FMT-STRUCT-015, FMT-STRUCT-019, FMT-STRUCT-021, FMT-STRUCT-022, RH5040-001, RH5040-002, RH5111-005, SHARED-BLANK-008, SHARED-CHAIN-001, SHARED-CHAIN-004, SHARED-CHAIN-006, SHARED-COMMENT-007, SHARED-FIX-010 |

This spec covers the formatter's *Unsupported* outcome: the input shapes the formatter refuses as a whole, the precheck phase that finds them before any other phase runs (FMT-PIPE-001), a phase's refusal of a shape the precheck does not find, what every entry point returns for a refused input, and the query that tells a code fix whether the formatter refuses a node. Each unsupported shape is a requirement of this spec; a shape is declared here so that no phase has to handle it.

## Non-goals

- What a host does with the outcome — the CLI (CLI specs), the code fixes (SHARED-FIX), and the Playground (PLAY).
- Whether an analyzer rule reports an unsupported shape. The list of unsupported shapes belongs to the formatter and depends on no analyzer rule.
- Formatting part of a refused input. A refused input is never formatted in part.
- Code with syntax errors, generated code, and a document whose text another build compiles as code, which FMT-PIPE-005, FMT-PIPE-006, and FMT-PIPE-033 leave untouched before this spec applies.

## Requirements

### FMT-UNSUP-001 — An unsupported input is refused as a whole

When the input of the formatting phases contains a shape that a requirement of this spec declares unsupported, the formatter MUST NOT format any part of it: the entry point MUST return its input unchanged, and MUST report the input as unsupported, with the reason — the unsupported shape — and the location of the shape. The phases after the precheck MUST NOT run on a refused input.

Every formatting entry point of `ReihitsuFormatter` — `FormatDocumentAsync`, `FormatSyntaxTree`, `FormatNode`, `FormatNodeInDocumentAsync`, and `FormatNodeInDocumentWithContextAsync` — MUST return a result object, `FormattingResult<T>`, instead of the bare document, tree, or node:

- `Output` is what the entry point returned before this outcome existed: the formatted document, tree, or node, or the input unchanged when the formatter refuses it, and also when FMT-PIPE-005, FMT-PIPE-006, or FMT-PIPE-033 leaves it untouched.
- `Unsupported` is null when the input is not refused, also for an input FMT-PIPE-005, FMT-PIPE-006, or FMT-PIPE-033 leaves untouched. For a refused input it is an `UnsupportedSyntax` that carries the reason — the description the requirement of the shape gives, such as `a comment between a variable declaration's own tokens` for FMT-UNSUP-004 — and the location: the span of the shape in the caller's syntax tree, from which a host derives the line and the column of its first character.

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

`OrganizeUsingDirectivesInDocumentAsync` runs no formatting phase but the using phase (FMT-PIPE-030) and MUST NOT refuse an input under this spec. `FormatDocumentationElementInDocumentAsync` lays out one documentation element and runs no formatting phase on the document (FMT-PIPE-032), and MUST NOT refuse an input under this spec either: no shape this spec declares can stand inside a documentation element. `SplitsFieldDeclaration` answers a question and formats nothing (FMT-PIPE-031). An input that FMT-PIPE-005, FMT-PIPE-006, or FMT-PIPE-033 returns unchanged is not reported as unsupported: those requirements apply first.

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

Example — `FormatNodeInDocumentAsync` with the object creation `/* a */ new List<int> { 1 }` of

```csharp
using System.Collections.Generic;

class C
{
    void M()
    {
        var d =
            /* a */ new List<int> { 1 };
    }
}
```

as target formats the object creation: it holds the comment, in the leading trivia of its first token, but neither the variable declaration that FMT-UNSUP-004 names nor the `=` of the equals-value clause that FMT-UNSUP-020 names. With the equals-value clause as target, the input holds the `=`, and with the local declaration statement as target, it holds the declaration; each is returned unchanged and reported as unsupported, the clause with FMT-UNSUP-020's reason and the statement with FMT-UNSUP-004's. `FormatDocumentAsync` refuses the document as well.

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

### FMT-UNSUP-008 — Comments and directives between a conditional operator and its branch are unsupported

An input MUST be refused when a comment — `//`, `/* */`, `///`, or `/** */`, in every documentation mode — a preprocessor directive, or disabled text stands between an operator of a conditional expression and the branch behind it:

- between the `?` and the first token of the `true` branch, or
- between the `:` and the first token of the `false` branch,

that is, in the trailing trivia of the `?` or the `:`, or in the leading trivia of the branch's first token. The requirement holds for every conditional expression, also for one nested in a branch of another, and for one inside a lambda or another expression in a branch, each with its own operators.

These places are not refused:

- in front of the `?` or the `:`: in the trailing trivia of the condition's or the `true` branch's last token, or in the leading trivia of the `?` or the `:` — a comment on the line above the operator's line, or behind the code in front of the operator;
- inside a branch, behind its first token;
- the null-conditional operators `?.` and `?[]`, and the null-coalescing operator `??`, which are not conditional expressions.

The reason the formatter gives for this shape is `a comment or directive between a conditional operator and its branch`, and its location is the span of the first comment, directive, or disabled text in that place.

Why: A comment or a directive behind `?` or `:` keeps the branch off the operator's line, and the line-break phase had to refuse its move of the operator and leave the conditional half wrapped, which RH5308 then kept reporting and its fix could not correct. A comment in front of the operator stands on its own line or behind the code before it, where the phases already handle comments for every other wrapped operator, so a comment that explains a branch is written above the operator's line. The shape is rare, and refusing it lets the line-break phase join every branch to its operator without a guard.

Example — each conditional is refused, and the input that holds it is returned unchanged:

```csharp
class C
{
    int M(bool a)
    {
        var x = a ? // c1
            1
            : 2;
        var y = a
                    ? 1 : // c2
                    2;
        var z = a ? /* c3 */ 1
                : 2;

        return x;
    }
}
```

Example — a directive and disabled text behind the `?` refuse the input as well:

```csharp
class C
{
    int M(bool a)
    {
        return a ?
#if DEBUG
            1
#else
            2
#endif
            : 3;
    }
}
```

Example — the other side: none of these comments refuses the input, and the input is formatted:

```csharp
class C
{
    int M(bool a)
    {
        var x = a
                    // c4
                    ? 1
                    : 2;
        var y = a /* c5 */
                    ? 1 // c6
                    : N(/* c7 */ 2); // c8

        return x;
    }

    int N(int v) => v;
}
```

### FMT-UNSUP-009 — Comments and directives between a list element and the comma behind it are unsupported

An input MUST be refused when, for a comma token of the input, a comment — `//`, `/* */`, `///`, or `/** */`, in every documentation mode — a preprocessor directive, or disabled text stands between the comma and the token in front of it — in the trailing trivia of that token or in the leading trivia of the comma — and the comma stands on a later line of the text than the line on which that token ends.

The conditions, each with both sides:

- **Between the comma and the token in front of it.** Only the gap in front of the comma counts. A comment behind the comma, on its line or on a later one, does not refuse the input: `Foo(1⏎, /* c */ 2)`, `Foo(1⏎, // c⏎2)`, and `Foo(1, // c⏎2)` are formatted. When the token in front of the comma is an omitted element without text, such as the omitted type argument of `Dictionary<,>`, the gap starts behind the token in front of that element: `typeof(Dictionary<⏎// c⏎,>)` is refused, `typeof(Dictionary<, // c⏎>)` is not refused under this requirement (FMT-UNSUP-018 refuses it).
- **On a later line of the text.** Every line break in the text between the end of the token in front of the comma and the comma counts, also one inside the text of a multi-line comment: `Foo(1 // c⏎, 2)` and `Foo(1 /* a⏎ b */, 2)` are refused. A comment in front of a comma on the line the token ends on does not refuse the input: `Foo(true /* force */, false)` and `{ 0x01 /* SOH */, 0x02 /* STX */ }` are formatted.
- **A comma token of the input.** Every comma of the code counts, whatever list it separates or ends — argument, parameter, attribute argument, type parameter, and type argument lists, base lists, constraint clauses, initializers, collection expressions, patterns, tuples, enum members, switch-expression arms, and `for` expression lists — a trailing comma behind the last element included. A comma in disabled text is no token of the input: in a build that does not define `NEVER`, `Foo(1⏎#if NEVER⏎, 2⏎#endif⏎)` is formatted, while a build that defines it refuses the same text, because there `, 2` is code and `#if NEVER` stands in front of its comma. A comma inside a directive or inside a documentation comment, such as the one in `<see cref="M(int, int)"/>`, is not a token of the code and is not checked.

A comma inside the span of a variable declaration (FMT-UNSUP-004) is a comma of the input too, and a comment in front of it can refuse the input under both requirements, starting at the same comment, where FMT-UNSUP-001's first shape in the text does not decide. Which reason and location the formatter gives depends on the comma:

- **A comma between two declarators of the declaration**, such as the one in front of `, b = 2` in `int a = 1 // c⏎, b = 2;`: the reason and the location are FMT-UNSUP-004's. A directive or disabled text in front of such a comma, which FMT-UNSUP-004 does not cover, is refused under this requirement with its reason; in a field declaration with several declarators, FMT-UNSUP-010 refuses it as well, and the reason and the location stay this requirement's — also where the declaration is a local declaration inside a value of that field, such as one in the body of a lambda.
- **Every other comma in the span** — one in the declaration's type, such as the comma of a tuple type, a function pointer type, or a type argument list, and one in an attribute argument list of a field: `(int A // c⏎, int B) t = default;` and `delegate*<int // c⏎, void> _f;` give this requirement's reason and location. A directive or disabled text in front of such a comma in a field declaration with several declarators is refused under FMT-UNSUP-010 as well, starting at the same directive, and gives FMT-UNSUP-010's reason.

A comma inside an initializer value raises no such question: FMT-UNSUP-004 excepts the inside of the value, and a comma is never a value's first or last token, so only this requirement refuses a comment in front of it.

The reason the formatter gives for this shape is `a comment or directive between a list element and the comma behind it`, and its location is the span of the first comment, directive, or disabled text in that place.

Why: A comma that a comment or a directive keeps off the line of the element in front of it cannot move behind that element without crossing the comment or the directive (FMT-BREAK-001). Every phase that places commas — the comma moves of argument, parameter, and attribute argument lists, of constraint clauses, and of initializers, collection expressions, and patterns, and the alignment of a comma that starts a line — had to find such a comma and leave it as written, each with its own guard, and the initializers needed a column of their own for it. Comma-first code with a comment or a directive in that gap is rare: in parameter, argument, attribute, base, and type lists a rule already reports the comment, and elsewhere it takes comma-first code with a comment written above the comma. Refusing it lets those guards go. The comment and the directive belong behind the element or behind the comma, where the phases already carry them. A comma at the end of its element's line, also one in front of a directive (`2,⏎#if X⏎3,⏎#endif`), is not the shape. Nor is a comment in front of a comma on the element's line, also where the parser puts it into the comma's leading trivia: in the documentation modes `Parse` and `Diagnose`, a documentation comment never trails a token, so the `/** doc */` of `B /** doc */,` is the comma's leading trivia with no line break in front of it, and the removal of a trailing comma still keeps it behind the last element (FMT-STRUCT-019). Counting the lines of the text, and not the line breaks SHARED-BLANK-008 counts, makes a comma behind a multi-line comment part of the shape, so the phase that moves commas needs no guard for it either. The comma and the token in front of it always belong to one list node, so a node-level input that holds the comma holds the whole gap (FMT-UNSUP-003), and no phase writes a comment, a directive, or disabled text into that gap when it moves a comma (FMT-UNSUP-005). Where FMT-UNSUP-004 refuses the same comment in front of a comma between two declarators, its reason stays the one the formatter gives for that declaration today; every other comma is a list comma like any other, so the reason names the comma, whichever list it stands in.

Example — each list is refused, and the input that holds it is returned unchanged:

```csharp
class C
{
    void M<T>(int a // c1
              , int b)
        where T : class /* c2 */
                  , new()
    {
        Foo(1
            // c3
            , 2);
        var e = new D { X = 3 // c4
            , Y = 4 };
        Foo(1 /* a
            b */, 2);
    }
}
```

Example — a directive between a list element and the comma behind it refuses the input in every build: where `DEBUG` is defined, `#if DEBUG` stands in front of the comma of `, c`; where it is not, `#if DEBUG`, the disabled text `, c`, and `#else` stand in front of the comma of `, d`:

```csharp
class C
{
    void M(int a, int b, int c, int d)
    {
        M(a, b
#if DEBUG
            , c
#else
            , d
#endif
            );
    }
}
```

Example — the other side: none of these comments and directives refuses the input, and the input is formatted. The `#if NEVER` list is shown as parsed in a build that does not define `NEVER`; the enum is not refused in either build of `X`:

```csharp
enum E
{
    A,
#if X
    B,
#endif
    C,
}

class C
{
    void M()
    {
        Foo(true /* force */, false);
        Foo(1
            , /* c */ 2
            , // c
            3);
        Foo(1, // c
            2);
        int[] d = [1
            , 2];
        Foo(1
#if NEVER
            , 2
#endif
            );
    }
}
```

Example — a comment in front of a comma between two declarators is reported with FMT-UNSUP-004's reason, `a comment between a variable declaration's own tokens`, at the span of `// c`:

```csharp
class C
{
    void M()
    {
        int a = 1 // c
            , b = 2;
    }
}
```

Example — the other side: a comment in front of the comma of a tuple type in a variable declaration also stands in FMT-UNSUP-004's span, but the comma is not between declarators, so the input is reported with this requirement's reason, `a comment or directive between a list element and the comma behind it`, at the span of `// c`:

```csharp
class C
{
    void M()
    {
        (int A // c
            , int B) t = default;
    }
}
```

### FMT-UNSUP-010 — Directives inside a field declaration with several declarators are unsupported

An input MUST be refused when a preprocessor directive or disabled text stands in the trivia of a token of a field declaration that has more than one declarator, other than in the leading trivia of the declaration's first token.

The declaration's tokens run from its first token — its first attribute list, its first modifier, or its type — through the `;` that ends it, as FMT-UNSUP-004 states for a field, and include every token inside its initializer values. The conditions, each with both sides:

- **A field declaration.** The requirement names the field declaration by its syntax, whatever declares it, and decides from the declaration's own tokens and trivia (FMT-UNSUP-003). An event field declaration and a local declaration statement are no field declaration: `public event Action A,⏎#if X⏎B,⏎#endif⏎C;` and `int a,⏎#if X⏎b,⏎#endif⏎c;` in a method are not refused under this requirement.
- **More than one declarator.** The declarators are those of the build being formatted. A field with one declarator is not refused, also with a directive among its modifiers (`[Obsolete]⏎#if X⏎public⏎#endif⏎int _a;`). In a build that does not define `X`, `private int _a⏎#if X⏎, _b⏎#endif⏎;` has one declarator, and `, _b` is disabled text in front of the `;`: not refused under this requirement. In a build that defines `X` it has two, and it is refused.
- **Other than in the leading trivia of the declaration's first token.** A directive and disabled text above the declaration, or in front of it on its line, are not refused: a `#region`, the `#if` or `#else` of a group that holds the whole declaration, and the `#endif` and the disabled text of a group in front of it. Whether the declaration is then split is FMT-STRUCT-022's: it is split when that trivia holds no disabled text, and kept whole when it does. A directive behind the declaration's `;` belongs to the token behind it, outside the declaration — a directive and disabled text are always leading trivia — such as the `#endif` of a group that holds the declaration. A directive between the attribute lists and the modifiers, or between the modifiers and the type, stands in the leading trivia of a later token and refuses the input.
- **Inside an initializer value as well.** Unlike FMT-UNSUP-004, this requirement excepts no initializer value: a directive inside the lambda, the collection, or any other value of one of the declarators refuses the input.

A node-level input that holds only part of the declaration — a declarator, its initializer value, or the variable declaration without the field — does not hold the shape (FMT-UNSUP-003).

A directive or disabled text between a declarator and a comma on a later line of the text is refused under this requirement and under FMT-UNSUP-009, starting at the same directive: the reason and the location are FMT-UNSUP-009's. The declarator and the comma can belong to any variable declaration inside the field declaration: to the field's own, such as the `#if X` of `private int _a⏎#if X⏎, _b⏎#endif⏎;` in a build that defines `X`, and to a local declaration inside a value, such as the `#pragma` of `private Func<int> _f = () =>⏎{⏎int x = 1⏎#pragma warning disable CS0168⏎, y = 2;⏎return x + y;⏎}, _g;`, which gives FMT-UNSUP-009's reason as well. Every other directive in the declaration — one in front of a comma that separates no declarators, such as a comma of an argument list or a tuple type, one between a conditional operator and its branch (FMT-UNSUP-008), and every directive that no other requirement refuses — gives this requirement's reason.

The reason the formatter gives for this shape is `a directive inside a field declaration with several declarators`, and its location is the span of the first directive or disabled text in that place.

Why: The field split repeats the attributes, modifiers, and type on every field it creates and carries over only the declaration's leading trivia, the declarators with their values, and the trivia behind its `;` (FMT-STRUCT-021, FMT-STRUCT-022). A directive anywhere else would be dropped or would end up guarding a different field, and a conditional group that starts in one value and ends in another value or in front of the `;` would put the repeated modifiers and type into the text of another build. The split kept such a declaration whole through a guard of its own, which the RH7101 fix asked about. Telling a group that stays inside one value from one that crosses a comma would need the directives matched up, which is no cheap check; a declaration with several declarators and a directive inside is rare, and refusing it removes the guard. The leading trivia of the first token is a different place: it holds the `#region` of the members and the directives of a group that wraps the whole declaration or ends in front of it, which is common code. The split keeps that trivia on the first field unchanged, and a group around the declaration then holds all of its fields, so a directive there is neither dropped nor moved, and the declaration is split there. Disabled text in that trivia can hold an attribute list or a modifier that another build compiles as part of this declaration, which a split would give to the first field alone; that class is rare, and the split leaves it whole rather than refuse the file (FMT-STRUCT-022).

Example — each declaration is refused, and the input that holds it is returned unchanged; in a build that does not define `X`, `_b,` is disabled text, and in one that defines it, `#if X` stands in front of `_b`:

```csharp
using System;

class C
{
    int a,
#if X
        b,
#endif
        c;

    private int _a,
#pragma warning disable CS0169
        _b;
#pragma warning restore CS0169

    [Obsolete]
#if X
    public
#endif
    int _c, _d;

    private Func<int> _f = () =>
    {
#if DEBUG
        return 1;
#else
        return 2;
#endif
    }, _g;
}
```

The location is the span of the first directive, `#if X` in front of `b,`.

Example — the other side: none of these directives refuses the input. The combined fields are split (FMT-STRUCT-021), and the directives stay where they are; the event field and the local of the first condition above are not refused either. A combined field behind disabled text is not refused either, and it stays whole (FMT-STRUCT-022). In a build that defines `X`, the `#endif` in front of `int _single` stands between the modifier `public` and the type, and FMT-UNSUP-017 refuses the input; in a build that does not, `_single` has no modifier, and the input is formatted:

```csharp
using System;

class C
{
    #region Fields

    private int _x, _y;

    #endregion // Fields

#if !RELEASE
    private int _p, _q = 1;
#endif

    [Obsolete]
#if X
    public
#endif
    int _single;
}
```

```csharp
using System;

class C
{
    #region Fields

    private int _x;
    private int _y;

    #endregion // Fields

#if !RELEASE
    private int _p;
    private int _q = 1;
#endif

    [Obsolete]
#if X
    public
#endif
    int _single;
}
```

### FMT-UNSUP-011 — Comments and directives between a binary operator and its right operand are unsupported

An input MUST be refused when a comment — `//`, `/* */`, `///`, or `/** */`, in every documentation mode — a preprocessor directive, or disabled text stands between the operator of a binary expression and its right operand: in the trailing trivia of the operator, or in the leading trivia of the right operand's first token.

The conditions, each with both sides:

- **The operator of a binary expression.** The operator is a binary operator as SHARED-COMMENT-007 defines it: an arithmetic, shift, relational, equality, bitwise, logical, or null-coalescing operator, the `is` of a type test, and the `as` of a conversion. `a + /* c */ b`, `o is /* c */ string`, and `o as /* c */ string` are refused. Whether an `is` is the operator of a type test is decided by the syntax tree alone: an `is` that the parser reads as the operator of a binary expression counts, whatever its right operand denotes, so `o is /* c */ Color.Red`, where `Color.Red` is an enum member, is refused like `o is /* c */ string`, although SHARED-COMMENT-007 leaves such an `is` open; an `is` the parser reads as a pattern match, such as the `is` of `o is /* c */ 5` or `o is /* c */ not null`, does not count under this requirement (FMT-UNSUP-017 refuses the gap behind it). A token that is no such operator is not: the `is` of a pattern match and the operators inside a pattern, such as `and`, `or`, and `>` (`n is > /* c */ 1`; FMT-UNSUP-017 refuses the gap behind `and`, `or`, and `not`), an assignment or compound assignment operator (`n += /* c */ 1`, which FMT-UNSUP-020 refuses), the `..` of a range, and a conditional expression's `?` and `:`, which FMT-UNSUP-008 decides.
- **Behind the operator.** Only the gap between the operator and the right operand's first token counts. A comment, a directive, or disabled text in front of the operator — in the trailing trivia of the left operand's last token or in the leading trivia of the operator, such as a comment behind the left operand on its line (`a /* c */ && b`) or on a line above the operator's line — does not refuse the input. Nor does one inside the right operand, behind its first token (`a && (/* c */ b)`, `n + M(/* c */ 1)`), or behind the right operand.
- **On any line.** Whether the gap holds a line break does not matter: `n * /* scale */ 2` on one line is refused like `a && // c⏎b`.
- **Every binary expression.** Each binary expression is checked with its own operator and right operand — one in a chain, such as the second `&&` of `a && b && /* c */ c`, one nested in an operand of another, and one inside a lambda, an argument, or any other expression. A token in disabled text is no token of the input: in a build that does not define `X`, the `|` of the disabled text `| B` in `A⏎#if X⏎| B⏎#endif⏎| C` is not checked, and the directives stand in front of the last `|`, so the input is not refused in either build.

A node-level input that holds the right operand but not the binary expression, such as the right operand itself, does not hold the shape (FMT-UNSUP-003).

Where FMT-UNSUP-004 or FMT-UNSUP-010 refuses the same comment, directive, or disabled text, both shapes start at it, and FMT-UNSUP-001's first shape in the text does not decide; the reason and the location are then FMT-UNSUP-004's or FMT-UNSUP-010's:

- **FMT-UNSUP-004** excepts the inside of an initializer value, and the gap behind an operator of a value is always inside it, so the two meet only outside a value, such as in an attribute argument of a declaration: `[Obsolete("old" + /* why */ " API")] private int _e;` gives FMT-UNSUP-004's reason, while `int c = 1 + /* c */ 2;` gives this requirement's.
- **FMT-UNSUP-010** excepts no initializer value: a directive or disabled text behind an operator anywhere in a field declaration with several declarators, also inside a value, such as the `#if X` of `private int _a = 1 +⏎#if X⏎2⏎#else⏎3⏎#endif⏎, _b;`, gives FMT-UNSUP-010's reason. In a field with one declarator, the same directive gives this requirement's.

The reason the formatter gives for this shape is `a comment or directive between a binary operator and its right operand`, and its location is the span of the first comment, directive, or disabled text in that place.

Why: The line-break phase moves a binary operator at the end of a line to the start of the next line and joins the right operand behind it, and it joins the type behind the `is` of a type test onto the `is` (FMT-BREAK-017, FMT-BREAK-041). A comment or a directive behind the operator kept the right operand off the operator's line, so the phase refused the join (FMT-BREAK-001), left the operator at the end of the line, and the right operand started a line that no requirement places, in the column of the enclosing statement; RH5302's fix carried a copy of that refusal. RH5049 already reports a comment behind a binary operator, so the comment is rare by the repository's definition. No rule reports a directive or disabled text there; they are rare in practice, because a conditional operand is written behind a trailing operator only where the operators are not written at the line starts, as the formatter writes them (FMT-BREAK-017), and written with leading operators, the same code is formatted. A comment that explains an operand belongs in front of the operator — on the line above the operator's line or behind the left operand — where the phases carry it as they do for every wrapped operator. Directives and disabled text are part of the shape because the join refuses them alike: with comments alone, the refusal would have to stay. The shape is the same on one line, as it is for a conditional's operators (FMT-UNSUP-008), so the boundary is a place in the syntax tree, not a question of line breaks. The operator and its right operand belong to one binary expression, so a node-level input that holds the operator holds the whole gap, and no phase writes a comment, a directive, or disabled text into that gap: the join carries only whitespace into it (FMT-UNSUP-005). The precheck reads syntax only, so whether an `is` is a type test is the parser's reading, the reading the line-break phase joins by; whether the name behind it denotes a type or a constant needs a semantic model the precheck does not have. The gap behind the `is` of a pattern match and the gap behind a pattern combinator are FMT-UNSUP-017's, which declares them with the other gaps the line-break phase joins, with reasons that name them. The assignment operators are laid out by FMT-BREAK-016, and a comment, a directive, or disabled text behind one is refused by FMT-UNSUP-020. A comment in an attribute argument of a declaration, and a directive in a field declaration with several declarators, were refused under FMT-UNSUP-004 and FMT-UNSUP-010 before this requirement existed, and keep the reason they gave.

Example — each binary expression is refused, and the input that holds it is returned unchanged:

```csharp
class C
{
    bool M(bool a, bool b, int n, object o)
    {
        var x = a && // c1
                b;
        var y = n
                + // c2
                1;
        var z = n * /* c3 */ 2;
        var w = a
                && b
                ||
                // c4
                o is string;
        var v = o as /* c5 */ string ?? /* c6 */ "";

        return x;
    }
}
```

The location is the span of `// c1`, which starts in line 5, column 22.

Example — an `is` followed by an enum member is read as the operator of a binary expression, and the input is refused:

```csharp
enum Color
{
    Red
}

class C
{
    bool M(object o)
    {
        return o is /* c */ Color.Red;
    }
}
```

The location is the span of `/* c */`, which starts in line 10, column 21.

Example — directives and disabled text behind an operator refuse the input in every build: where `DEBUG` is defined, `#if DEBUG` stands behind the first `|`; where it is not, `#if DEBUG`, the disabled text, and `#endif` stand behind it:

```csharp
using System.Text.RegularExpressions;

class C
{
    RegexOptions M()
    {
        return RegexOptions.Compiled |
#if DEBUG
               RegexOptions.IgnoreCase |
#endif
               RegexOptions.Multiline;
    }
}
```

Example — the other side: none of these comments and directives refuses the input, and the input is formatted:

```csharp
using System.Text.RegularExpressions;

class C
{
    bool M(bool a, bool b, int n)
    {
        var x = a
                // c1
                && b;
        var y = a /* c2 */ && b;
        var z = n + M2(1) // c3
                + 2;
        var w = n is > /* c4 */ 1 or 3;
        var u = M3() /* c7 */ is 5;

        return x && y && z > 0 && w && u;
    }

    object M3()
    {
        return 5;
    }

    int M2(int v)
    {
        return v;
    }

    RegexOptions Options()
    {
        return RegexOptions.Compiled
#if DEBUG
               | RegexOptions.IgnoreCase
#endif
               | RegexOptions.Multiline;
    }
}
```

### FMT-UNSUP-012 — Comments and directives between the arrow of an expression body and its expression are unsupported

An input MUST be refused when a comment — `//`, `/* */`, `///`, or `/** */`, in every documentation mode — a preprocessor directive, or disabled text stands between the `=>` of an expression body and its expression: in the trailing trivia of the `=>`, or in the leading trivia of the expression's first token.

The conditions, each with both sides:

- **The `=>` of an expression body.** An expression body is the arrow clause of a member, an accessor, or a local function: of a method, a constructor, a finalizer, an operator, a conversion operator, a property, an indexer, a `get`, `set`, `init`, `add`, or `remove` accessor, or a local function. `int A => /* c */ 1;`, `get => /* c */ _v;`, and `int Local() => /* c */ 1;` are refused. The `=>` of a lambda and of a switch-expression arm belongs to no expression body: `() => /* c */ 1` and `1 => /* c */ "one"` do not refuse the input under this requirement; FMT-UNSUP-017 refuses the first.
- **Behind the `=>`.** Only the gap between the `=>` and the expression's first token counts. A comment, a directive, or disabled text in front of the `=>` — behind the token in front of it on its line (`int A /* c */ => 1;`), on a line of its own above the `=>`, or in front of the `=>` on the `=>`'s line — is not this shape: FMT-UNSUP-013 refuses it. A comment, a directive, or disabled text inside the expression, behind its first token (`=> (/* c */ 1)`, `=> M(/* c */ 1)`), or behind the expression, in front of the `;` or behind it (`=> 1 /* c */;`, `=> 1; // c`), does not refuse the input.
- **On any line.** Whether the gap holds a line break does not matter: `int Get() => /* c */ 42;` on one line is refused like `int A => // c⏎1;` and like a comment on a line of its own between the `=>` and the expression.
- **Every expression body.** Each expression body is checked with its own `=>` and expression, also one nested in another construct, such as a local function inside a lambda's body or an accessor of a property in a nested type. A token in disabled text is no token of the input: an expression body inside disabled text is not checked.

A node-level input that holds the expression but not the arrow clause, such as the expression itself, does not hold the shape (FMT-UNSUP-003).

Where FMT-UNSUP-010 refuses the same directive or disabled text — behind the `=>` of an expression body inside an initializer value of a field declaration with several declarators, such as a local function in a lambda's body — both shapes start at it, and FMT-UNSUP-001's first shape in the text does not decide; the reason and the location are FMT-UNSUP-010's. In a field with one declarator, the same directive gives this requirement's reason.

The reason the formatter gives for this shape is `a comment or directive between the arrow of an expression body and its expression`, and its location is the span of the first comment, directive, or disabled text in that place.

Why: The conversion of an expression body into a block (FMT-STRUCT-004, FMT-STRUCT-005) removes the arrow, and a comment behind it had no owner: the conversion carried it onto the new opening brace, in front of the created statement, or behind the created `return`, each a case of its own, and it kept the body for a directive there (FMT-STRUCT-007), because the created `return` cannot share a directive's line. A property, an indexer, and an accessor keep their expression body (FMT-STRUCT-008); there the line-break phase refused the join of FMT-BREAK-026 behind a comment or directive (FMT-BREAK-001), the expression started a line that no requirement places, in the member's own column, and the lift of a get-only accessor list to the member kept the list (FMT-STRUCT-015). RH5040 already reports a comment around the arrow of a member or an accessor, and RH3202, RH3203, RH3205, RH3206, RH3207, and RH3208 report every expression-bodied method, constructor, operator, conversion operator, finalizer, and local function as a whole, so a comment behind the arrow is rare by the repository's definition, also behind a local function's arrow, which RH5040 leaves to other rules (RH5040-002). A local function's conversion runs through the same code as a method's, so leaving it out would keep every case for it alone. Directives and disabled text are part of the shape because the join refusal and the seam check of the get-only lift test them alike: with comments alone, both would have to stay. A comment that describes the body belongs above the member, where the phases carry it as they do for every member. The shape is the same on one line, as for the operators of FMT-UNSUP-008 and FMT-UNSUP-011: the boundary is a place in the syntax tree, not a question of line breaks. The `=>` and its expression belong to one arrow clause, so a node-level input that holds the `=>` holds the whole gap, and no phase writes a comment, a directive, or disabled text into that gap: the conversions that create an arrow (FMT-STRUCT-009, FMT-STRUCT-014) create it with nothing behind it, and the join of FMT-BREAK-026 carries only whitespace (FMT-UNSUP-005). The `=>` of a lambda and of a switch-expression arm belongs to an expression, which no transform converts and FMT-BREAK-026 does not join, so declaring it here would delete nothing; the gap behind a lambda's `=>` is FMT-UNSUP-017's, because FMT-BREAK-041 joins it, and the gap behind a switch arm's `=>` is FMT-UNSUP-021's, declared on the maintainer's answer. A directive in a field declaration with several declarators was refused under FMT-UNSUP-010 before this requirement existed, and keeps the reason it gave.

Example — each expression body is refused, and the input that holds it is returned unchanged:

```csharp
class C
{
    private int _v;

    int A => // c1
        1;

    int B =>
        // c2
        2;

    int this[int i] => /* c3 */ i;

    int Get() => /* c4 */ 42;

    int V
    {
        get => // c5
            _v;
    }

    void M()
    {
        int Local() =>
            /* c6 */ 1;

        Local();
    }
}
```

The location is the span of `// c1`, which starts in line 5, column 14.

Example — a directive and disabled text behind the `=>` refuse the input in every build: where `DEBUG` is defined, `#if DEBUG` stands in front of `1`; where it is not, `#if DEBUG`, the disabled text, and `#else` stand in front of `2`:

```csharp
class C
{
    int A =>
#if DEBUG
        1;
#else
        2;
#endif
}
```

The location is the span of `#if DEBUG`, which starts in line 4, column 1.

Example — the other side: none of these comments refuses the input, and the input is formatted:

```csharp
using System;

class C
{
    // c1
    int A => 1;

    int C2 => (/* c2 */ 1);

    int D => 1 /* c3 */;

    int E => 1; // c4

    void M(int n)
    {
        Func<int> f = () => (/* c5 */ 1);
    }
}
```

### FMT-UNSUP-013 — Comments and directives between the arrow of an expression body and the code in front of it are unsupported

An input MUST be refused when a comment — `//`, `/* */`, `///`, or `/** */`, in every documentation mode — a preprocessor directive, or disabled text stands between the `=>` of an expression body and the token in front of the `=>`: in the trailing trivia of the token in front of the `=>`, or in the leading trivia of the `=>`.

The conditions, each with both sides:

- **The `=>` of an expression body.** An expression body is the arrow clause FMT-UNSUP-012 names: of a method, a constructor, a finalizer, an operator, a conversion operator, a property, an indexer, a `get`, `set`, `init`, `add`, or `remove` accessor, or a local function. `int A // c⏎=> 1;`, `get /* c */ => _v;`, and `int Local() // c⏎=> 1;` are refused. The `=>` of a lambda and of a switch-expression arm belongs to no expression body: `() /* c */ => 1` and `1 /* c */ => "one"` do not refuse the input under this requirement; FMT-UNSUP-017 refuses the second.
- **In front of the `=>`.** Only the gap between the `=>` and the token in front of it counts. That token is the last token of what the arrow clause follows: the name of a property, the `]` of an indexer's parameter list, the `)` of a parameter list or of a constructor initializer, the last token of a constraint clause, or an accessor's keyword. A comment, a directive, or disabled text in the leading trivia of the declaration's first token — above the member, its documentation comment included — does not refuse the input, and neither does one in front of the token in front of the `=>`, such as inside the parameter list (`int F(/* c */ int a) => a;`). One behind the `=>`, in front of the expression, is FMT-UNSUP-012's shape.
- **On any line.** Whether the gap holds a line break does not matter: `int A /* c */ => 1;` on one line is refused like `int A // c⏎=> 1;` and like a comment on a line of its own between the token in front of the `=>` and the `=>`.
- **Every expression body.** Each expression body is checked with its own `=>` and the token in front of it, also one nested in another construct, such as a local function inside a lambda's body or an accessor of a property in a nested type. A token in disabled text is no token of the input: an expression body inside disabled text is not checked.

The gap lies between two tokens, so a node-level input holds the shape only when it holds both: an input that holds the `=>` but not the token in front of it, such as the arrow clause itself, does not hold the shape (FMT-UNSUP-003).

Where FMT-UNSUP-010 refuses the same directive or disabled text — in front of the `=>` of an expression body inside an initializer value of a field declaration with several declarators, such as a local function in a lambda's body — both shapes start at it, and FMT-UNSUP-001's first shape in the text does not decide; the reason and the location are FMT-UNSUP-010's. In a field with one declarator, the same directive gives this requirement's reason.

The reason the formatter gives for this shape is `a comment or directive between the arrow of an expression body and the code in front of it`, and its location is the span of the first comment, directive, or disabled text in that place.

Why: A property, an indexer, and an accessor keep their expression body (FMT-STRUCT-008). For a property and an indexer, the line-break phase refused the join of FMT-BREAK-026 across a comment or directive in front of the arrow (FMT-BREAK-001), and the `=>` then started a line that no requirement places, in the member's own column. FMT-BREAK-026 does not join an accessor's arrow; for an accessor, the lift of a get-only accessor list to the member had to check the trivia behind `get` and in front of the getter's arrow (FMT-STRUCT-015). The conversion of a method, a constructor, a finalizer, an operator, a conversion operator, or a local function into a block (FMT-STRUCT-004, FMT-STRUCT-005) removes the arrow, and a comment in front of it had no owner: the conversion moved comments on lines of their own in front of the created statement, kept them in front of the new opening brace when a comment stood on the arrow's line, which wrote the comment directly in front of the brace, and kept the expression body for a region directive there (FMT-STRUCT-007). RH5040 already reports every comment directly in front of the `=>` of a member or an accessor (RH5040-001), and RH3208 reports every expression-bodied local function as a whole, so a comment in front of the arrow is rare by the repository's definition. No rule reports a directive or disabled text there as such, although RH3202, RH3203, RH3205, RH3206, RH3207, RH3208, and RH5401 report most members that hold one as a whole. A conditional group that closes directly in front of the `=>` — around the signature or around a constraint clause — or that chooses the `=>` puts a directive into the gap and is refused; the same choice written around the whole member, from in front of its first token through behind its `;`, leaves the gap free of directives and is formatted. Directives and disabled text are part of the shape because the join refusal and the seam checks of the get-only lift test them alike: with comments alone, both would have to stay. A comment that describes the member belongs above it, in the leading trivia of its first token, where the phases carry it as they do for every member. Together with FMT-UNSUP-012, both gaps around the `=>` of an expression body are free of comments, directives, and disabled text in every input the phases receive. The shape is the same on one line, as for FMT-UNSUP-012: the boundary is a place in the syntax tree, not a question of line breaks. No phase writes a comment, a directive, or disabled text into the gap (FMT-UNSUP-005): the join of FMT-BREAK-026 carries only whitespace, and the conversions that create an arrow (FMT-STRUCT-009, FMT-STRUCT-014) keep the accessor's block or the accessor list when a comment, directive, or disabled text stands in the trivia they would put in front of the created arrow — behind the accessor keyword (FMT-STRUCT-012), or behind the signature and around the braces of the accessor list (FMT-STRUCT-015). The `=>` of a lambda and of a switch-expression arm belongs to an expression, which no transform converts and FMT-BREAK-026 does not join, so declaring it here would delete nothing; the gap in front of a switch arm's `=>` is FMT-UNSUP-017's, because FMT-BREAK-041 joins it. A directive in a field declaration with several declarators was refused under FMT-UNSUP-010 before this requirement existed, and keeps the reason it gave.

Example — each expression body is refused, and the input that holds it is returned unchanged:

```csharp
class C
{
    private int _v;

    int A // c1
        => 1;

    int B
        // c2
        => 2;

    int this[int i] /* c3 */ => i;

    int Get() /* c4 */ => 42;

    int V
    {
        get // c5
            => _v;
    }

    void M()
    {
        int Local()
            /* c6 */ => 1;

        Local();
    }
}
```

The location is the span of `// c1`, which starts in line 5, column 11.

Example — a directive and disabled text in front of the `=>` refuse the input in every build: where `DEBUG` is defined, `#if DEBUG` stands in front of the first `=>`; where it is not, `#if DEBUG`, the disabled text, and `#else` stand in front of the second:

```csharp
class C
{
    int A
#if DEBUG
        => 1;
#else
        => 2;
#endif
}
```

The location is the span of `#if DEBUG`, which starts in line 4, column 1.

Example — the other side: none of these comments refuses the input under this requirement or FMT-UNSUP-012, and the input is formatted:

```csharp
using System;

class C
{
    // c1
    int A => 1;

    int B(/* c2 */ int a) => a;

    int D => 1 /* c3 */;

    int E => 1; // c4

    void M(int n)
    {
        Func<int> f = () /* c5 */ => 1;
        var s = n switch
        {
            1 => "one", // c6
            _ => "other",
        };
    }
}
```

### FMT-UNSUP-014 — Comments and directives in front of an argument list, an element access, or a null-forgiving operator are unsupported

An input MUST be refused when a comment — `//`, `/* */`, `///`, or `/** */`, in every documentation mode — a preprocessor directive, or disabled text stands in one of the gaps of an argument list, an element access, a conditional element access, or a null-forgiving operator of the kinds whose gap FMT-BREAK-039 or FMT-BREAK-040 joins:

- the gap in front of the part's first token: in the trailing trivia of the token in front of it, or in the leading trivia of that first token;
- the gap between the `?` and the `[` of a conditional element access: in the trailing trivia of the `?`, or in the leading trivia of the `[`.

The part's first token is decided by the syntax tree alone:

- the `(` of an `ArgumentListSyntax` whose parent is an invocation, an object creation, an implicit object creation, a constructor initializer, a primary-constructor base type, or the `with(…)` element of a collection expression — the argument lists FMT-BREAK-039 names: `Foo /* c */ (1)`, `new List<int> // c⏎(4)`, `: base /* c */ (1)`;
- the `[` of the bracketed argument list of an `ElementAccessExpressionSyntax`: `arr // c⏎[0]`, `this /* c */ [0]`;
- the `?` of a `ConditionalAccessExpressionSyntax` whose next token is the `[` of an `ElementBindingExpressionSyntax`'s bracketed argument list: `n // c⏎?[0]`, also behind a null-forgiving operator (`x! /* c */ ?[0]`);
- the `!` of a null-forgiving operator (`SuppressNullableWarningExpression`) that is not followed by `.` or by the `?.` of a conditional member access: `Get() /* c */ !`, `s // c⏎! + ""`, `x /* c */ !?[0]`.

The conditions, each with both sides:

- **The parts FMT-BREAK-039 and FMT-BREAK-040 join.** Every other `(`, `[`, `?`, and `!` is not this shape: the `(` of a parameter list (`void M /* c */ (int x)`), of an attribute's argument list (`[Obsolete /* c */ ("x")]`), of a parenthesized expression, a cast, or a tuple; the `[` of an implicit element access (`["a"] = 1`), an array rank specifier, `new[]`, `stackalloc []`, a collection expression, a list pattern, an attribute list, or an indexer's parameter list; the `?` of a conditional member access (`n /* c */ ?.Length`) and of a conditional expression; and a `!` followed by `.` or `?.`, which is the first token of a link operator (SHARED-CHAIN-001) whose gap the chain layout decides (`s // c⏎!.Length`). The `!` of a logical negation is a prefix operator, not a null-forgiving operator.
- **In front of the part, and between `?` and `[`.** Only the gap between the part's first token and the token in front of it counts, and, for a conditional element access, the gap between its `?` and its `[` (`n? /* c */ [0]`, `n? // c⏎[0]`, also behind a null-forgiving operator: `x!? /* c */ [0]`). These are all the gaps between the tokens a part starts with: an argument list, an element access, and a null-forgiving operator start with one token, a conditional element access with its `?` and `[`. A comment, a directive, or disabled text in front of the token in front of the part — above the statement, or on the line above it — does not refuse the input, and neither does one inside the part's brackets or parentheses (`Foo(/* c */ 1)`, `arr[/* c */ 0]`, `n?[/* c */ 0]`) or behind the part (`arr[0] /* c */`).
- **On any line.** Whether the gap holds a line break does not matter: `Foo /* c */ (1)` on one line is refused like `Foo // c⏎(1)` and like a comment on a line of its own between `Foo` and `(1)`, and so is `[with /* size */ (4)]` (FMT-SPACE-028) and `n? /* c */ [0]`.
- **Every part.** Each part is checked with its own gap, whatever expression stands in front of it — in a chain with links or in an expression without any, inside a list element, an interpolation hole, a lambda, or any other construct. A token in disabled text is no token of the input: a part inside disabled text is not checked.

The gap lies between two tokens, so a node-level input holds the shape only when it holds both: an input that holds the part but not the token in front of it, such as an argument list itself, does not hold the shape, and neither does one that holds the token in front but not the part, such as the invoked expression alone (FMT-UNSUP-003).

Where FMT-UNSUP-004 or FMT-UNSUP-010 refuses the same comment, directive, or disabled text, both shapes start at it, and FMT-UNSUP-001's first shape in the text does not decide; the reason and the location are then FMT-UNSUP-004's or FMT-UNSUP-010's:

- **FMT-UNSUP-004** excepts the inside of an initializer value, and the gap in front of a part inside a value is always inside it, so the two meet only outside a value, such as in an attribute argument of a declaration: `[Obsolete(nameof /* c */ (C))] private int _e;` gives FMT-UNSUP-004's reason, while `int c = Foo /* c */ (1);` gives this requirement's.
- **FMT-UNSUP-010** excepts no initializer value: a directive or disabled text in front of a part anywhere in a field declaration with several declarators, also inside a value, gives FMT-UNSUP-010's reason. In a field with one declarator, the same directive gives this requirement's.

The reason the formatter gives for this shape is `a comment or directive in front of an argument list, an element access, or a null-forgiving operator`, and its location is the span of the first comment, directive, or disabled text in that place.

Why: The line-break phase joins an argument list, an element access, a conditional element access, and a null-forgiving operator onto the line of the token in front of it (FMT-BREAK-039, FMT-BREAK-040). A comment, a directive, or disabled text in that gap kept the part off that line, so the phase refused the join (FMT-BREAK-001), and the part started a line that no requirement places: outside a chain with links it took the column of the enclosing statement, in a chain with links the indentation phase gave it a column of its own, and the branches of a conditional group around it could get different columns. A comment between an expression and its argument list or index is rare, and a comment that explains the part belongs behind the line or above the statement, where the phases carry it as they do for every statement. A conditional group that chooses between two argument lists is rare as well; the same choice written around the whole statement is formatted. Directives and disabled text are part of the shape because the join refusal and the column of a part kept below test them alike: with comments alone, both would have to stay. The shape is the same on one line, as for the operators of FMT-UNSUP-008 and FMT-UNSUP-011: the boundary is a place in the syntax tree, not a question of line breaks. The gap between a conditional element access's `?` and `[` is in the shape as well: FMT-BREAK-040 removes a line break there, and a comment there kept the `[` on a line of its own that the chain layout aligned under the `?`; with the gap in the shape, no gap between the tokens a part starts with ever keeps a line break, and neither the join nor the indentation phase has to handle one. The inside of a part's brackets or parentheses is not in the shape: a comment there is handled as before. No phase writes a comment, a directive, or disabled text into either gap (FMT-UNSUP-005): the joins of FMT-BREAK-039 and FMT-BREAK-040 carry only whitespace into it, and no transform creates such a part or moves trivia in front of one or between its `?` and `[`. A comment in an attribute argument of a declaration and a directive in a field declaration with several declarators were refused under FMT-UNSUP-004 and FMT-UNSUP-010 before this requirement existed, and keep the reason they gave.

Example — each part is refused, and the input that holds it is returned unchanged:

```csharp
#nullable enable

class C
{
    int M(int[] arr, int[]? n, string? s)
    {
        var a = arr // c1
            [0];
        var b = Foo /* c2 */ (1);
        var c = n
            // c3
            ?[0];
        var d = s /* c4 */ !;
        var e = n? // c5
            [0];

        return a + b + (c ?? 0) + d.Length + (e ?? 0);
    }

    int Foo(int x)
    {
        return x;
    }
}
```

The location is the span of `// c1`, which starts in line 7, column 21.

Example — a directive and disabled text in front of an argument list refuse the input in every build: where `DEBUG` is defined, `#if DEBUG` stands in front of `(1)`; where it is not, `#if DEBUG`, the disabled text, and `#else` stand in front of `(2)`:

```csharp
class C
{
    int M()
    {
        return Foo
#if DEBUG
            (1);
#else
            (2);
#endif
    }

    int Foo(int x)
    {
        return x;
    }
}
```

The location is the span of `#if DEBUG`, which starts in line 6, column 1.

Example — the other side: none of these comments refuses the input under this requirement, and the input is formatted:

```csharp
#nullable enable

using System;

class C
{
    [Obsolete /* c1 */ ("x")]
    int M(int[] arr, int[]? n, string? s)
    {
        // c2
        var a = arr[/* c3 */ 0];
        var b = Foo(/* c4 */ 1); // c5
        var c = n?[/* c6 */ 0];
        var d = s // c7
                !.Length;
        var e = (/* c8 */ 1);
        int[] f = [/* c9 */ 1];

        return a + b + (c ?? 0) + d + e + f[0];
    }

    int Foo /* c10 */ (int x)
    {
        return x;
    }
}
```

### FMT-UNSUP-015 — Comments and directives inside a link operator are unsupported

An input MUST be refused when a comment — `//`, `/* */`, `///`, or `/** */`, in every documentation mode — a preprocessor directive, or disabled text stands between two tokens of a link operator `?.`, `!.`, or `!?.` (SHARED-CHAIN-001): in the trailing trivia of one of the operator's tokens other than its `.`, or in the leading trivia of one of its tokens other than its first.

The operator's tokens are decided by the syntax tree alone:

- `?.` — the `?` of a `ConditionalAccessExpressionSyntax` whose next token is a `.`, and that `.`, which is the operator token of the `MemberBindingExpressionSyntax` its branch starts with: `s? /* c */ .Length`, `a? // c⏎.B()`;
- `!.` — the `!` of a null-forgiving operator (`SuppressNullableWarningExpression`) that is the expression of a simple member access, and that member access's `.`: `t! /* c */ .Length`, `Get()!⏎#if DEBUG⏎.Trim()`;
- `!?.` — the `!` of a null-forgiving operator that is the expression of a `ConditionalAccessExpressionSyntax` whose `?` is followed by a `.`, that `?`, and that `.`: `u! /* c */ ?.Length`, `u!? /* c */ .Length`.

The conditions, each with both sides:

- **Between the operator's own tokens.** Only the gaps between the tokens of one operator count. A comment, a directive, or disabled text in front of the operator's first token — the gap in front of a link, which SHARED-CHAIN-004 decides for the first link and the chain layout for every other (`s /* c */ ?.Length`, `items // c⏎.Where(…)`, `s // c⏎!.Length`) — does not refuse the input, and neither does one between the operator's `.` and the member name, which FMT-UNSUP-016 decides (`s?. /* c */ Length`), one inside the link's argument list (`a?.B(/* c */ 1)`), or one behind the link (`a?.B() // c`).
- **Link operators only.** A token pair that is not two tokens of a link operator is not this shape: a plain `.` is one token and has no gap of its own; the `?` and the `[` of a conditional element access, and the `!` and the `?` of `!?[`, are the tokens a part starts with, which FMT-UNSUP-014 decides (`n? /* c */ [0]`, `x! /* c */ ?[0]`); a conditional expression's `?` is no operator of a link (`c ? /* c */ .5 : 1`, FMT-UNSUP-008); and neither are the `!` of a logical negation, the `..` of a range, the `->` of a pointer member access, or the `::` of an alias-qualified name.
- **On any line.** Whether the gap holds a line break does not matter: `s? /* c */ .Length` on one line is refused like `s? // c⏎.Length` and like a comment on a line of its own between `?` and `.`.
- **Every link.** Each link operator is checked with its own tokens, whatever stands around it — the only link of an expression or one link of a wrapped chain, an invoked link or a member read, inside a list element, an interpolation hole, a lambda, or any other construct. A token in disabled text is no token of the input: a link operator inside disabled text is not checked.

The tokens of one link operator all belong to one node — the conditional access of `?.` and `!?.`, the member access of `!.` — so a node-level input holds the shape only when it holds that node: an input that holds the `.` but not the `?` or the `!` in front of it, such as the branch of a conditional access, does not hold the shape, and neither does one that holds the `!` but not the `.` behind it, such as the null-forgiving operator alone (FMT-UNSUP-003).

Where FMT-UNSUP-004 or FMT-UNSUP-010 refuses the same comment, directive, or disabled text, both shapes start at it, and FMT-UNSUP-001's first shape in the text does not decide; the reason and the location are then FMT-UNSUP-004's or FMT-UNSUP-010's. FMT-UNSUP-004 excepts the inside of an initializer value, and a link operator of a value is always inside it, so the two meet only outside a value. FMT-UNSUP-010 excepts no initializer value: a directive or disabled text inside a link operator anywhere in a field declaration with several declarators, also inside a value, gives FMT-UNSUP-010's reason. In a field with one declarator, the same directive gives this requirement's.

The reason the formatter gives for this shape is `a comment or directive inside a ?. or !. operator`, and its location is the span of the first comment, directive, or disabled text in that place.

Why: A link operator reads as one operator, and the line-break phase moves a line break inside it in front of its first token (SHARED-CHAIN-006, FMT-BREAK-019). A comment, a directive, or disabled text between its tokens kept them apart, so the phase refused the join (FMT-BREAK-001), and the indentation phase put the rest of the operator under its first token, a column no requirement states; the operator stayed split for good, the `?` or `!` at the end of a line read as a conditional or a negation, the branches of a conditional group between the tokens got different columns, and an own-line comment between them got a blank line inserted in front of it inside the statement. A comment between `?` and `.` is rare, and a comment that explains a link belongs behind the line or above the statement, where the phases carry it as they do for every statement; a conditional group that chooses between two links is rare as well, and the same choice written around the whole statement is formatted. Directives and disabled text are part of the shape because the join refusal and the column of the operator's rest test them alike: with comments alone, both would have to stay. The shape is the same on one line, as for the operators of FMT-UNSUP-008 and FMT-UNSUP-011: the boundary is a place in the syntax tree, not a question of line breaks. With the shape refused, no gap inside a link operator ever keeps a line break, so neither the join nor the indentation phase has to handle one. The gap in front of a link stays supported: SHARED-CHAIN-004 and the chain layout decide it, and RH5048 reports a comment in front of the first link. No phase writes a comment, a directive, or disabled text into a link operator (FMT-UNSUP-005): the join carries only whitespace into it, and no transform creates a link operator or moves trivia into one. The `?[` of a conditional element access is no link operator (SHARED-CHAIN-001); FMT-UNSUP-014 already refuses a comment, a directive, or disabled text between its `?` and `[`, and between a `!` and the `?` of `!?[`.

Example — each link operator is refused, and the input that holds it is returned unchanged:

```csharp
#nullable enable

class C
{
    int? M(string? s, string? t, string? u, string? w)
    {
        var p = s? // c1
            .Length;
        var q = t! /* c2 */ .Length;
        var r = u!
            // c3
            ?.Length;
        var v = w!? /* c4 */ .Length;

        return p + q + r + v;
    }
}
```

The location is the span of `// c1`, which starts in line 7, column 20.

Example — a directive and disabled text inside a `!.` refuse the input in every build: where `DEBUG` is defined, `#if DEBUG` stands between `!` and the `.` of `.Trim()`; where it is not, `#if DEBUG`, the disabled text, and `#else` stand between `!` and the `.` of `.ToUpper()`:

```csharp
#nullable enable

class C
{
    string M()
    {
        return Get()!
#if DEBUG
            .Trim();
#else
            .ToUpper();
#endif
    }

    string? Get()
    {
        return null;
    }
}
```

The location is the span of `#if DEBUG`, which starts in line 8, column 1.

Example — the other side: none of these comments refuses the input under this requirement, and the input is formatted:

```csharp
#nullable enable

class C
{
    int? M(string? s, string? t, C? c, int[]? n)
    {
        // c1
        var p = s // c2
                ?.Length;
        var q = t /* c3 */ !.Length;
        var r = c?.N(/* c4 */ 1) // c5
                 ?.N(2);
        var x = n?[/* c6 */ 0];

        return p + q + (r == null ? 0 : 1) + x; // c7
    }

    C? N(int value)
    {
        return value > 0 ? this : null;
    }
}
```

### FMT-UNSUP-016 — Comments and directives between a link's dot and its member name are unsupported

An input MUST be refused when a comment — `//`, `/* */`, `///`, or `/** */`, in every documentation mode — a preprocessor directive, or disabled text stands between the `.` of a link (SHARED-CHAIN-001) and its member name: in the trailing trivia of the `.`, or in the leading trivia of the name's first token.

The `.` and the name are decided by the syntax tree alone:

- a simple member access (`SimpleMemberAccessExpression`) — its operator token and the first token of its name: `a.B(). // c⏎C()`, `t. /* c */ Length`, `this. /* c */ _x = 1`, also behind a null-forgiving operator (`t!. /* c */ Length`);
- a member binding (`MemberBindingExpressionSyntax`) — its operator token and the first token of its name: `s?. /* c */ Length`, `a?. // c⏎B()`, also behind a null-forgiving operator (`u!?. /* c */ Length`).

The conditions, each with both sides:

- **Between the dot and the name.** Only the gap between the `.` and the first token of the member name counts. A comment, a directive, or disabled text in front of the link operator — the gap in front of a link, which SHARED-CHAIN-004 decides for the first link and the chain layout for every other (`items // c⏎.Where(…)`) — does not refuse the input under this requirement, and neither does one between the tokens of a link operator, which FMT-UNSUP-015 decides (`s? /* c */ .Length`), one inside the name (`a.B</* c */ int>()`), one in front of the attached part behind the name, which FMT-UNSUP-014 decides (`a.B /* c */ (1)`), one inside the link's argument list (`a.B(/* c */ 1)`), or one behind the link (`a.B() // c`).
- **Member accesses and member bindings only.** A `.` that is not the operator token of a simple member access or a member binding is not this shape: the `.` of a qualified name (`class B : System. // c⏎IDisposable`, whose layout FMT-INDENT-013 states; `using System. /* c */ Text;`), the `::` of an alias-qualified name, the `->` of a pointer member access, a `.` in a documentation comment's `cref`, and the `..` of a range.
- **On any line.** Whether the gap holds a line break does not matter: `t. /* c */ Length` on one line is refused like `t. // c⏎Length` and like a comment on a line of its own between the `.` and `Length`.
- **Every link.** Each simple member access and member binding is checked with its own gap, whatever stands around it — the only link of an expression or one link of a wrapped chain, an invoked member or a member read, an assignment's target, inside a list element, an interpolation hole, a lambda, or any other construct. A token in disabled text is no token of the input: a member access inside disabled text is not checked.

The `.` and the name belong to one node — the member access or the member binding — so a node-level input holds the shape only when it holds that node: an input that holds the name but not the `.` in front of it, such as the name itself, does not hold the shape (FMT-UNSUP-003).

Where FMT-UNSUP-004 or FMT-UNSUP-010 refuses the same comment, directive, or disabled text, both shapes start at it, and FMT-UNSUP-001's first shape in the text does not decide; the reason and the location are then FMT-UNSUP-004's or FMT-UNSUP-010's. FMT-UNSUP-004 excepts the inside of an initializer value, and a member access of a value is always inside it, so the two meet only outside a value, such as in an attribute argument of a declaration: `[Obsolete(nameof(C. /* c */ M))] private int _e;` gives FMT-UNSUP-004's reason, while `int c = t. /* c */ Length;` gives this requirement's. FMT-UNSUP-010 excepts no initializer value: a directive or disabled text between a `.` and its member name anywhere in a field declaration with several declarators, also inside a value, gives FMT-UNSUP-010's reason. In a field with one declarator, the same directive gives this requirement's.

The reason the formatter gives for this shape is `a comment or directive between a . and its member name`, and its location is the span of the first comment, directive, or disabled text in that place.

Why: The line-break phase joins a member name split from its dot back onto the dot (FMT-BREAK-020). A comment, a directive, or disabled text in the gap kept the name off the dot's line, so the phase refused the join (FMT-BREAK-001), and the name started a line that no requirement places: at statement level it took the column of the enclosing statement, where it read as a new statement; in a wrapped chain the `.` was left on a line of its own and a blank line was inserted inside the statement in front of an own-line comment; and the branches of a conditional group between the dot and the name got different columns. Placing the name below its dot would need a column rule of its own in the indentation phase and would keep the join refusal. A comment between a `.` and its member name is rare, and a comment that explains a link belongs behind the line or above the statement, where the phases carry it as they do for every statement; a conditional group that chooses between two member names is rare as well, and the same choice written around the whole statement is formatted. Directives and disabled text are part of the shape because the join refusal tests them alike: with comments alone, it would have to stay. The shape is the same on one line, as for the operators of FMT-UNSUP-008, FMT-UNSUP-011, and FMT-UNSUP-015: the boundary is a place in the syntax tree, not a question of line breaks. With the shape refused, the gap between a link's dot and its name never keeps a line break, so the join needs no refusal there. No phase writes a comment, a directive, or disabled text into the gap (FMT-UNSUP-005): the join carries only whitespace into it, and no transform creates a member access or a member binding or moves trivia between a dot and a name. The `.` of a qualified name is no link (SHARED-CHAIN-001): the chain layout never joins its name onto it, and FMT-INDENT-013 states where its continuation goes in a base list.

Example — the gap behind each link's `.` is refused, and the input that holds it is returned unchanged:

```csharp
#nullable enable

class C
{
    object M(dynamic a, string? s, string? t, string? u)
    {
        var p = a.B(). // c1
            C();
        var q = s?. /* c2 */ Length;
        var r = t!.
            // c3
            Length;
        var v = u?.Trim(). /* c4 */ Length;

        return (p, q, r, v);
    }
}
```

The location is the span of `// c1`, which starts in line 7, column 24.

Example — a directive and disabled text between a `.` and its member name refuse the input in every build: where `DEBUG` is defined, `#if DEBUG` stands between the `.` and `Trim`; where it is not, `#if DEBUG`, the disabled text, and `#else` stand between the `.` and `ToUpper`:

```csharp
#nullable enable

class C
{
    string M(string s)
    {
        return s.
#if DEBUG
            Trim();
#else
            ToUpper();
#endif
    }
}
```

The location is the span of `#if DEBUG`, which starts in line 8, column 1.

Example — the other side: none of these comments refuses the input under this requirement, and the input is formatted:

```csharp
#nullable enable

class C : System. // c1
          IDisposable
{
    public void Dispose()
    {
    }

    int? M(string? s, string t)
    {
        // c2
        var p = s // c3
                ?.Length;
        var q = t.Substring(/* c4 */1) // c5
                 .Length;
        var r = t.Trim(); // c6

        return p + q + r.Length; // c7
    }
}
```

### FMT-UNSUP-017 — Comments and directives in a gap the line-break phase joins are unsupported

An input MUST be refused when a comment — `//`, `/* */`, `///`, or `/** */`, in every documentation mode — a preprocessor directive, or disabled text stands in one of the gaps below: in the trailing trivia of the token in front of the gap, or in the leading trivia of the token behind it. These are the gaps FMT-BREAK-041 joins, the gaps between a pattern combinator and its right pattern that FMT-BREAK-017 joins, and the gaps around the `=` of an anonymous-object member and of a `let` clause that FMT-BREAK-016 joins. Each gap is decided by the syntax tree alone:

| Gap | Token in front of it | Token behind it | Refused, for example | Reason |
|---|---|---|---|---|
| A keyword and its operand | the `return` of a return statement with an expression or of a `yield return` statement; the `throw` of a throw statement with an expression or of a throw expression; the `await` of an await expression, of an `await foreach` statement, or of an `await using` statement or declaration; the `new` of an object creation, an array creation, or an implicitly typed array creation | the first token of the expression or the operand; the `foreach` or `using`; the first token of the created type, or the `[` of `new[]` | `return // c⏎x;`, `yield return /* c */ x;`, `o ?? throw /* c */ new E()`, `await /* c */ t`, `await // c⏎foreach (…)`, `new // c⏎List<int>()`, `new /* c */ [] { 1 }` | `a comment or directive between return, throw, await, or new and what follows it` |
| A pattern and its `is` | the `is` of an `is` pattern expression (`IsPatternExpressionSyntax`) | the first token of its pattern | `o is /* c */ not null`, `o is // c⏎(int or long)` | `a comment or directive between is and its pattern` |
| A pattern combinator | the `and` or `or` of a binary pattern; the `not` of a negated pattern | the first token of the right pattern or of the negated pattern | `x is 1 or // c⏎2`, `o is not /* c */ null` | `a comment or directive between and, or, or not and its pattern` |
| A switch arm's `when` | the last token of the arm's pattern | the `when` of the arm's `when` clause | `int i /* c */ when i > 0 => 1` | `a comment or directive in front of the when of a switch arm` |
| A switch arm's `=>` | the last token of the arm's pattern, or of its `when` clause | the `=>` of the arm | `1 /* c */ => 2`, `int i when i > 0 // c⏎=> 1` | `a comment or directive in front of the => of a switch arm` |
| A base list | the token in front of a base list's `:` | the `:` | `class D // c⏎: B`, `enum E /* c */ : byte` | `a comment or directive in front of the : of a base list` |
| A modifier | each modifier of a member, type, or delegate declaration, a local declaration, a local function, a parameter, a function-pointer parameter, an accessor, a lambda, or an anonymous method | the token behind it: the next modifier, or the first token of what the modifiers precede | `private /* c */ int M()`, `public // c⏎static void M()`, `public /* c */ class N`, `static // c⏎int Local()`, `this /* c */ int x`, `delegate*<ref // c⏎int, void>`, `private /* c */ set;`, `async /* c */ x => x` | `a comment or directive behind a modifier` |
| A catch filter | the token in front of the `when` of a catch filter clause | the `when` | `catch (Exception e) /* c */ when (e != null)` | `a comment or directive in front of the when of a catch clause` |
| A `for` header | each of the two `;` of a `for` statement's header | the token behind that `;` | `for (var i = 0; // c⏎i < 3; i++)` | `a comment or directive behind a ; of a for header` |
| A `foreach` expression | the `in` of a `foreach` statement | the first token of its expression | `foreach (var y in /* c */ a)` | `a comment or directive behind the in of a foreach statement` |
| A cast | the `)` of a cast expression | the first token of its operand | `(int) /* c */ x`, `(int) // c⏎x` | `a comment or directive between a cast and its operand` |
| A lambda's expression body | the `=>` of a lambda with an expression body | the first token of that expression | `() => /* c */ 1`, `x => // c⏎x + 1` | `a comment or directive behind the => of a lambda` |
| A `with` expression | the last token of the expression a `with` expression copies | the `with` | `r /* c */ with { }`, `new R() // c⏎with { }` | `a comment or directive in front of with` |
| An anonymous-object member's `=` | the name of an anonymous-object member declarator; its `=` | the `=`; the first token of the value | `new { A /* c */ = 1 }`, `new { A = // c⏎1 }` | `a comment or directive in front of or behind the = of an anonymous-object member` |
| A `let` clause's `=` | the identifier of a `let` clause; its `=` | the `=`; the first token of the value | `let y /* c */ = x`, `let y = // c⏎x` | `a comment or directive in front of or behind the = of a let clause` |

The conditions, each with both sides:

- **These gaps only.** A comment, a directive, or disabled text next to the same tokens but outside the gap does not refuse the input under this requirement: in front of `return` (`// c⏎return x;`), behind the operand or the expression (`return x; // c`, `await t /* c */;`), inside the expression or the pattern behind its first token (`return M(/* c */ 1);`, `o is { Length: /* c */ 2 }`), in front of a combinator (`x is 1 /* c */ or 2`), behind the `=>` of a switch arm (`1 => /* c */ 2`), which FMT-UNSUP-021 refuses, behind a base list's `:` (`class D : /* c */ B`), in front of the first modifier (`/* c */ public int M()`), behind a catch filter's `when` (`when /* c */ (e != null)`), behind the `(` of a `for` header or in front of its `)` (`for (/* c */ ;;)`), in front of the `in` of a `foreach` statement (`foreach (var y /* c */ in a)`), inside a cast's parentheses (`(int /* c */) x`), in front of a lambda's `=>` (`() /* c */ => 1`), behind the `=>` of a lambda with a block body (`() => // c⏎{ }`), behind `with` (`r with /* c */ { }`), and between `return` or `throw` and the `;` of a statement without an expression (`return /* c */;`, `throw /* c */;`). Neither does the `new` of an anonymous object (`new /* c */ { A = 1 }`), whose opening brace starts its own line (FMT-BREAK-021); the `new` of an implicit object creation, whose argument list FMT-UNSUP-014 decides (`new /* c */ ()`); the `is` of a type test, which FMT-UNSUP-011 decides (`o is /* c */ string`); or the `=>` of an expression body of a member, which FMT-UNSUP-012 and FMT-UNSUP-013 decide.
- **On any line.** Whether the gap holds a line break does not matter: `return /* c */ x;` on one line is refused like `return // c⏎x;` and like a comment on a line of its own between `return` and `x`.
- **Every such gap.** Each gap is checked wherever it stands — at statement level, inside a list element, an interpolation hole, a lambda, a switch expression, or any other construct. A token in disabled text is no token of the input: a gap inside disabled text is not checked. A modifier that a conditional group makes conditional is a modifier of the build that compiles it: in `#if X⏎public⏎#else⏎internal⏎#endif⏎static int M()`, the `#else` and the disabled text behind `public` stand behind a modifier where `X` is defined, and the `#endif` stands behind `internal` where it is not, so the input is refused in every build; in `#if X⏎public⏎#endif⏎int M()`, `#endif` stands behind `public` where `X` is defined and in front of the first token where it is not, so the input is refused only where `X` is defined. The same holds for a type declaration (`#if X⏎public⏎#else⏎internal⏎#endif⏎class Foo` is refused in every build).

The gap lies between two tokens, so a node-level input holds the shape only when it holds both: an input that holds the token behind the gap but not the token in front of it, such as the expression behind `return`, the pattern behind `is`, or a base list without its type declaration, does not hold the shape (FMT-UNSUP-003).

Where FMT-UNSUP-004 or FMT-UNSUP-010 refuses the same comment, directive, or disabled text, both shapes start at it, and FMT-UNSUP-001's first shape in the text does not decide; the reason and the location are then FMT-UNSUP-004's or FMT-UNSUP-010's. They meet in the declarations those requirements cover: a comment behind a field's modifier (`private /* c */ int _f;`, `private /* c */ static int _g;`), a comment between a local's `const` and its type, and a comment behind a function-pointer parameter's modifier inside the type of a field or a local (`delegate*<ref /* c */ int, void> _p;`) give FMT-UNSUP-004's reason, and a directive or disabled text in any of the gaps inside a field declaration with several declarators, also inside a value, gives FMT-UNSUP-010's reason. FMT-UNSUP-004 excepts the inside of an initializer value and the trailing trivia of the `;` that ends a `for` statement's initializer, so a gap inside a value (`int x = await /* c */ t;`) and the gap behind the first `;` of a `for` header give this requirement's reason.

The reason the formatter gives for this shape is the one the table names for the gap, and its location is the span of the first comment, directive, or disabled text in that place.

Why: The line-break phase joins each of these gaps (FMT-BREAK-016, FMT-BREAK-017, FMT-BREAK-041). A comment or a directive in the gap would keep the token behind it off the line of the token in front of it, so the join would have to refuse it (FMT-BREAK-001), and the token behind it would start a line that only the general continuation rule places (FMT-INDENT-034) — the shape the maintainer asked to refuse instead, for every gap of the table. A comment there is rare: a comment that explains a value, a pattern, an arm, a base list, a modifier, a filter, a loop condition, a cast, or a lambda's body belongs above the statement or the declaration or behind its line, where the phases carry it as they do for every statement; a conditional group in such a gap is rare as well, and the same choice written around the whole statement or member is formatted. That includes a conditional group of modifiers, such as an accessibility that differs between builds, which the maintainer decided to refuse as well: written as two whole declarations in the group, it is formatted. Directives and disabled text are part of the shape because the join would refuse them alike: with comments alone, the refusal would have to stay. The shape is the same on one line, as for FMT-UNSUP-008, FMT-UNSUP-011, and FMT-UNSUP-015: the boundary is a place in the syntax tree, not a question of line breaks. Each gap lies between two tokens of one node — a statement, an expression, a declaration, a clause, or a declarator — and no phase writes a comment, a directive, or disabled text into it (FMT-UNSUP-005): the joins carry only whitespace into it; the conversion of an expression body into a block creates the `return` with the expression's leading trivia, which holds no comment, directive, or disabled text (FMT-UNSUP-012); and the field split (FMT-STRUCT-021) repeats a field's modifiers and type only where FMT-UNSUP-004 and FMT-UNSUP-010 leave no comment and no directive among them.

Example — each gap is refused, and the input that holds it is returned unchanged:

```csharp
using System;

class B;

class D // c1
    : B
{
    int M(int x, object o, int[] a)
    {
        var p = o is /* c2 */ not null;
        var r = x switch
                {
                    1 // c3
                        => 2,
                    _ => 0,
                };
        Func<int> u = () => /* c4 */ a.Length;
        var w = new { A /* c5 */ = 1 };

        for (var i = 0; // c6
            i < 3; i++)
        {
        }

        return // c7
            x;
    }
}
```

The location is the span of `// c1`, which starts in line 5, column 9.

Example — the gaps behind a combinator, a modifier, a cast, the `in` of a `foreach` statement, the name of a `let` clause, and `yield return` are refused alike; the conditional group of modifiers refuses the input in every build:

```csharp
using System;
using System.Collections.Generic;
using System.Linq;

#if NET
public
#else
internal
#endif
class N
{
    IEnumerable<int> M(int x, object o, int[] a)
    {
        var k = x is 1 or // c1
            2;
        var c = (long) /* c2 */ x;

        foreach (var y in /* c3 */ a)
        {
        }

        var q = from v in a
                let u /* c4 */ = v
                select u;

        yield return /* c5 */ x;
    }
}
```

Where `NET` is defined, the location is the span of `#else`, which starts in line 7, column 1; where it is not, the span of `#endif`, which starts in line 9, column 1.

Example — a directive and disabled text between `return` and its value refuse the input in every build: where `DEBUG` is defined, `#if DEBUG` stands in front of `1`; where it is not, `#if DEBUG`, the disabled text, and `#else` stand in front of `2`:

```csharp
class C
{
    int M()
    {
        return
#if DEBUG
            1;
#else
            2;
#endif
    }
}
```

The location is the span of `#if DEBUG`, which starts in line 6, column 1.

Example — the other side: none of these comments refuses the input under this requirement, and the input is formatted:

```csharp
using System;

class B;

// c1
class D : /* c2 */ B
{
    int M(int x, object o, int[] a)
    {
        // c3
        var p = o is string { Length: /* c4 */ 2 };
        Func<int> u = () /* c6 */ => a.Length;
        var w = new
                {
                    A = 1 /* c7 */
                };

        return x; // c8
    }
}
```

Example — the other side: a combinator, a modifier, a cast, and a `let` clause with a comment outside their gaps are formatted:

```csharp
using System.Collections.Generic;
using System.Linq;

/* c1 */ internal class N
{
    IEnumerable<int> M(int x, int[] a)
    {
        var k = x is 1 /* c2 */ or 2;
        var c = (long)x; // c3
        var q = from v in a
                let u = v // c4
                select u;

        yield return x; // c5
    }
}
```

### FMT-UNSUP-018 — Comments and directives inside a list of omitted type arguments are unsupported

An input MUST be refused when a comment — `//`, `/* */`, `///`, or `/** */`, in every documentation mode — a preprocessor directive, or disabled text stands inside a type argument list whose arguments are all omitted (`Dictionary<,>`, `List<>`): in the trailing trivia of its `<` or of one of its commas, or in the leading trivia of one of its commas or of its `>`.

The conditions, each with both sides:

- **A list of omitted type arguments.** Only a type argument list whose every argument is an omitted type argument counts: `typeof(Dictionary<, // c⏎>)` and `typeof(Dictionary</* c */,>)` are refused. A type argument list with a type in it is not this shape: `List<int /* c */>` and `Dictionary<int, // c⏎string>` are formatted, and FMT-UNSUP-009 decides a comment in front of one of their commas on a later line.
- **Inside the list.** Only the trivia between the `<` and the `>` counts. A comment in front of the `<` (`Dictionary /* c */ <,>`) or behind the `>` (`typeof(Dictionary<,> /* c */)`) does not refuse the input under this requirement.
- **On any line.** Whether a line break stands in the list does not matter: `typeof(Dictionary</* c */,>)` on one line is refused like `typeof(Dictionary<, // c⏎>)`.
- **Every such list.** Each list is checked wherever it stands, in a `typeof`, a `nameof`, or a nested type argument list. A token in disabled text is no token of the input: a list inside disabled text is not checked.

The list is one node, so a node-level input that holds one of its commas or its `>` holds the whole list (FMT-UNSUP-003).

Where FMT-UNSUP-009 refuses the same comment, directive, or disabled text — in front of a comma that stands on a later line of the text than the `<` (`typeof(Dictionary<⏎// c⏎,>)`) — both shapes start at it, and FMT-UNSUP-001's first shape in the text does not decide; the reason and the location are FMT-UNSUP-009's. Where FMT-UNSUP-004 or FMT-UNSUP-010 refuses it — in the type of a variable declaration, such as an attribute argument of a field — their reason and location are given.

The reason the formatter gives for this shape is `a comment or directive inside a list of omitted type arguments`, and its location is the span of the first comment, directive, or disabled text in that place.

Why: FMT-BREAK-014 joins a wrapped type argument list onto one line. A comment or a directive inside a list of omitted type arguments kept the list wrapped (FMT-BREAK-001), and the indentation phase had to place its later lines without a first element to align to: an omitted argument has no text and no column (SHARED-BLANK-008), so the alignment of FMT-INDENT-015 needed a case of its own that left the list to block indentation. Such a list is rare — it only names an unbound generic type in a `typeof` or a `nameof` — and a comment inside it, or a conditional group between its commas, is rarer still; the comment belongs behind the expression, where the phases carry it. Directives and disabled text are part of the shape because they keep the list wrapped alike: with comments alone, the case of its own would have to stay. With the shape refused, no list of omitted type arguments reaches the indentation phase wrapped. No phase writes a comment, a directive, or disabled text into the list (FMT-UNSUP-005): the join carries only whitespace into it, and no transform creates a type argument list.

Example — each list is refused, and the input that holds it is returned unchanged:

```csharp
class C
{
    void M()
    {
        var a = typeof(System.Collections.Generic.Dictionary<, // c1
            >);
        var b = typeof(System.Collections.Generic.List</* c2 */>);
    }
}
```

The location is the span of `// c1`, which starts in line 5, column 64.

Example — the other side: none of these comments refuses the input under this requirement, and the input is formatted:

```csharp
class C
{
    void M()
    {
        var a = typeof(System.Collections.Generic.Dictionary<,>); // c1
        var b = typeof(System.Collections.Generic.Dictionary<int, /* c2 */ string>);
    }
}
```

### FMT-UNSUP-019 — Comments and directives behind an assignment operator inside a switch arm or an anonymous-object member are unsupported

Status: Retired — the maintainer decided that a comment, a directive, or disabled text behind every assignment operator is refused, wherever the assignment stands; FMT-UNSUP-020 states the shape, which holds every input this requirement refused, with its own reason.

### FMT-UNSUP-020 — Comments and directives behind an assignment operator or the `=` of a declaration are unsupported

An input MUST be refused when a comment — `//`, `/* */`, `///`, or `/** */`, in every documentation mode — a preprocessor directive, or disabled text stands in one of these gaps:

- between the operator of an assignment expression and its right operand;
- between the `=` of an equals-value clause and its value: the initializer of a property or of a variable declarator, the value of an enum member, and the default value of a parameter.

The gap is the trailing trivia of the operator or the `=`, and the leading trivia of the first token of the right operand or the value.

The conditions, each with both sides:

- **Every assignment operator.** The operator is `=` or a compound assignment operator — `+=`, `-=`, `*=`, `/=`, `%=`, `&=`, `|=`, `^=`, `<<=`, `>>=`, `>>>=`, or `??=` — of an assignment expression, wherever the assignment stands: at statement level, in an argument, in a lambda, in a `for` incrementor, in an initializer value, in a switch-expression arm, and in an anonymous-object member. `x = // c⏎y + 1;`, `x += // c⏎3;`, `y ??= /* c */ 0;`, `M(x = /* c */ 1)`, `1 => x -= /* c */ 2`, `new { A = x += // c⏎3 }`, and the ref assignment `r = /* c */ ref y;` are refused. The members of an object initializer, of the initializer of a `with` expression, and of an index initializer are assignment expressions too, whose left operand is the member or the index: `new R { A = /* c */ 3 }`, `r with { A = /* c */ 3 }`, and `new Dictionary<int, int> { [0] = /* c */ 1 }` are refused. So is a deconstruction, which the syntax tree writes as an assignment expression whether its left side declares the variables or not: `(a, b) = /* c */ t;`, `var (a, b) = // c⏎t;`, and `(var a, b) = /* c */ t;` are refused.
- **Every equals-value clause.** The `=` of a property's initializer (`int[] G { get; } = // c⏎{ 1, 2 };`), of an enum member (`A = // c⏎1`), of a parameter's default value (`int y = /* c */ 1`), and of a variable declarator — of a field, an event field, a local, a `using` or `fixed` statement, or a `for` initializer — is part of the shape. The gap belongs to the shape for every owner of the clause, with a comment, a directive, or disabled text alike: `int x =⏎#if DEBUG⏎1;⏎#else⏎2;⏎#endif` is refused, and so is an equals-value clause of a declarator formatted without its declaration, with a comment behind its `=`. Where the input also holds the variable declaration, the order rule below gives FMT-UNSUP-004's reason for a comment, and FMT-UNSUP-010's for a directive or disabled text in a field declaration with several declarators.
- **No other `=`.** The `=` of a name-equals is not part of this shape: the `=` of an attribute argument (`[N(Name = /* c */ 1)]`) and of a using alias (`using Text = // c⏎System.Text;`) are FMT-UNSUP-022's, and the `=` of an anonymous-object member declarator and of a `let` clause are FMT-UNSUP-017's (`new { A = /* c */ 1 }`). A comparison such as `==`, `<=`, `>=`, or `!=` is a binary operator, which FMT-UNSUP-011 decides, and the `=>` of a switch arm, an expression body, or a lambda is FMT-UNSUP-021's, FMT-UNSUP-012's, or FMT-UNSUP-017's.
- **Behind the operator.** Only the gap between the operator or the `=` and the first token of the right operand or the value counts. A comment in front of the operator (`x /* c */ = 1;`, `int P { get; } /* c */ = 1;`, `int y /* c */ = 1`) or inside the right operand or the value behind its first token (`x = y /* c */ + 1;`, `x = ref /* c */ y;`) does not refuse the input under this requirement.
- **On any line.** Whether the gap holds a line break does not matter: `x = /* c */ 1;` on one line is refused like `x = // c⏎1;`.
- **Every such operator.** Each assignment and each equals-value clause is checked with its own operator or `=`, wherever it stands. A token in disabled text is no token of the input.

The operator belongs to the assignment expression, and the `=` to the equals-value clause; each of the two nodes holds the whole gap, so a node-level input that holds the operator or the `=` holds the shape, whatever it stands in — an equals-value clause formatted without its property, declarator, enum member, or parameter is refused like the whole declaration. A node-level input that holds the right operand or the value but neither the operator nor the `=` does not hold the shape (FMT-UNSUP-003): the object creation of `o = // c⏎new R { A = 1 };` or of `o =⏎// c⏎new R { A = 1 };`, which the RH5301 fix formats, is formatted, although the comment of the second stands in the leading trivia of its first token.

Where another shape of this spec refuses the same comment, directive, or disabled text, its reason and location are given: this shape is listed after FMT-UNSUP-004 through FMT-UNSUP-018. So FMT-UNSUP-004 keeps its reason for a comment behind a variable declarator's `=` (`int c = // c⏎1;`), and FMT-UNSUP-010 for a directive behind the `=` of a declarator in a field declaration with several declarators.

The reason the formatter gives for this shape is `a comment or directive behind an assignment operator or the = of a declaration`, and its location is the span of the first comment, directive, or disabled text in that place.

Why: The maintainer decided to declare this shape unsupported, in a change of its own (escalation 10), for every assignment operator and every equals-value clause. A comment or a directive behind the operator or the `=` keeps the value off the operator's line, because the join of FMT-BREAK-016 does not cross it (FMT-BREAK-001), and the value starts a continuation line (FMT-INDENT-034). RH5111 already reports a value kept below its `=` by a comment or a directive (RH5111-005). A comment that explains the value belongs above the statement, the member, or the parameter, where the phases carry it as they do for every statement and member. Directives and disabled text are part of the shape because the join refuses them alike: with comments alone, the refusal would have to stay. The shape is the same on one line, as for the other shapes of this spec, so the boundary is a place in the syntax tree. The members of an initializer and a deconstruction are assignment expressions in the syntax tree, so the same line is not supported in one syntax and refused in the other. Taking every operator and every equals-value clause makes the check one walk over the assignments and the clauses, with no condition on where they stand, and it deletes FMT-BREAK-001's refusal from every join of FMT-BREAK-016 behind an operator or an `=`: every input that reaches such a join holds the node that owns the gap, so the gap holds no comment, directive, or disabled text. It also takes the place of FMT-UNSUP-019, whose walk from an assignment up to the nearest switch arm or anonymous-object member is no longer needed. The joins in front of the operator or the `=` keep their refusal, because the gap there is not part of the shape. No phase writes a comment, a directive, or disabled text into the gap (FMT-UNSUP-005): the joins of FMT-BREAK-016 carry only whitespace into it.

Example — each `=` is refused, and the input that holds it is returned unchanged:

```csharp
class C
{
    int[] G { get; } = // values
        { 1, 2 };

    int M(int x, int y)
    {
        x = // reset
            y + 1;

        return x;
    }
}
```

The location is the span of `// values`, which starts in line 3, column 24.

Example — the members of an initializer are assignments, and a comment on one line is refused as well; the input is returned unchanged:

```csharp
using System.Collections.Generic;

record R
{
    public int A { get; set; }
}

class C
{
    object M(int x, R r)
    {
        x = /* one */ 1;

        var o = new R
                {
                    A = /* c1 */ 3
                };
        var p = r with
                  {
                      A = /* c2 */ 4
                  };
        var d = new Dictionary<int, int>
                {
                    [0] = /* c3 */ 1
                };

        return x + o.A + p.A + d.Count;
    }
}
```

The location is the span of `/* one */`, which starts in line 12, column 13.

Example — a compound assignment operator and a deconstruction, with and without declared variables, are refused as well; the input is returned unchanged:

```csharp
class C
{
    int M(int x, int? y, (int, int) t)
    {
        x += // add
            3;
        y ??= /* fallback */ 0;
        var (a, b) = // pair
            t;
        (x, a) = /* swap */ (a, x);

        return x + a + b;
    }
}
```

The location is the span of `// add`, which starts in line 5, column 14.

Example — an assignment inside a switch arm or an anonymous-object member is refused like any other, with this requirement's reason; the input is returned unchanged:

```csharp
class C
{
    object M(int x, int y)
    {
        var w = new
                {
                    A = x += // c1
                        3
                };

        return y switch
               {
                   1 => x -= /* c2 */ 2,
                   _ => w,
               };
    }
}
```

The location is the span of `// c1`, which starts in line 7, column 30.

Example — the `=` of an enum member, of a property, of a parameter's default value, and of a variable declarator; each is refused, and the input is returned unchanged:

```csharp
enum E
{
    A = // one
        1
}

class C
{
    int P { get; } =
#if DEBUG
        1;
#else
        2;
#endif

    int M(int y = /* default */ 1)
    {
        int x =
#if DEBUG
            1;
#else
            2;
#endif

        return x + y;
    }
}
```

The location is the span of `// one`, which starts in line 3, column 9.

Example — a directive behind the `=` of an assignment is refused, and the input is returned unchanged; the location is the span of `#if DEBUG`, which starts in line 6, column 1:

```csharp
class C
{
    int M(int x)
    {
        x =
#if DEBUG
            1;
#else
            2;
#endif

        return x;
    }
}
```

Example — the other side: none of these comments refuses the input under this requirement, and the input is formatted:

```csharp
class C
{
    int P { get; } /* c1 */ = 1;

    int M(int x, int y /* c2 */ = 1)
    {
        x /* c3 */ = 1;
        x = y /* c4 */ + 1;

        return x;
    }
}
```

### FMT-UNSUP-021 — Comments and directives behind the arrow of a switch arm are unsupported

An input MUST be refused when a comment — `//`, `/* */`, `///`, or `/** */`, in every documentation mode — a preprocessor directive, or disabled text stands between the `=>` of a switch-expression arm and the first token of the arm's expression: in the trailing trivia of the `=>`, or in the leading trivia of the expression's first token.

The conditions, each with both sides:

- **A switch-expression arm.** Only the `=>` of an arm of a switch expression counts: `1 => // c⏎2` is refused. The `=>` of a lambda is not part of this shape — behind a lambda's `=>`, FMT-UNSUP-017 refuses a comment in front of an expression body, and a comment in front of a block body (`() => // c⏎{ }`) is formatted —, and neither is the `=>` of a member's or an accessor's expression body, which FMT-UNSUP-012 decides. A `case` label of a switch statement has no `=>`: `case 1: // c` is formatted.
- **Behind the `=>`.** Only the gap between the `=>` and the first token of the arm's expression counts. A comment in front of the `=>` is FMT-UNSUP-017's (`1 /* c */ => 2`). A comment inside the expression behind its first token (`1 => M(/* c */ 2)`) or behind the expression (`1 => 2, // c`) does not refuse the input under this requirement.
- **On any line.** Whether the gap holds a line break does not matter: `1 => /* c */ 2` on one line is refused like `1 => // c⏎2`.
- **Every arm.** Each arm is checked with its own `=>`, also in a nested switch expression. A token in disabled text is no token of the input.

A line break alone behind the `=>` is formatted: no requirement joins it (FMT-BREAK-041), and the expression's line is a continuation line (FMT-INDENT-034).

The `=>` belongs to the arm, so a node-level input that holds the `=>` holds the whole gap.

Where another shape of this spec refuses the same comment, directive, or disabled text, its reason and location are given: this shape is listed after FMT-UNSUP-004 through FMT-UNSUP-020.

The reason the formatter gives for this shape is `a comment or directive behind the => of a switch arm`, and its location is the span of the first comment, directive, or disabled text in that place.

Why: The maintainer decided to declare this shape unsupported, in a change of its own (escalation 10). A comment behind the `=>` stands on the line the arm's expression would start on; a comment that explains the arm belongs above the arm or behind its comma, where the phases carry it as they do for every arm. Directives and disabled text are part of the shape alike. The shape is the same on one line, so the boundary is a place in the syntax tree. No requirement joins the gap behind the `=>`, so the declaration deletes no special case: the line behind the `=>` is a continuation line with or without a comment, and the shape is declared on the maintainer's explicit answer. No phase writes a comment, a directive, or disabled text into the gap (FMT-UNSUP-005): no phase moves trivia across an arm's `=>`.

Example — each arm is refused, and the input that holds it is returned unchanged:

```csharp
class C
{
    int M(int v)
    {
        return v switch
               {
                   1 => // one
                       2,
                   2 => /* two */ 3,
                   _ => 0
               };
    }
}
```

The location is the span of `// one`, which starts in line 7, column 25.

Example — the other side: none of these comments refuses the input under this requirement, and the line break alone behind `1 =>` is formatted:

```csharp
using System;

class C
{
    int M(int v)
    {
        var r = v switch
                {
                    1 =>
                        2, // c1
                    2 => 3 /* c2 */,
                    _ => 0
                };
        Action a = () => // c3
                   {
                   };

        switch (v)
        {
            case 1: // c4
                break;
        }

        return r;
    }
}
```

### FMT-UNSUP-022 — Comments and directives behind the `=` of an attribute argument or a using alias are unsupported

An input MUST be refused when a comment — `//`, `/* */`, `///`, or `/** */`, in every documentation mode — a preprocessor directive, or disabled text stands between the `=` of the name-equals of an attribute argument or of a using alias and the first token behind it: in the trailing trivia of the `=`, or in the leading trivia of the first token of the attribute argument's expression or of the alias's target.

The conditions, each with both sides:

- **An attribute argument or a using alias.** Only a name-equals whose parent is an attribute argument (`[N(Name = /* c */ 1)]`) or a using directive (`using Text = // c⏎System.Text;`, also with `global` and inside a namespace) counts. The `=` of an anonymous-object member declarator, also a name-equals, is FMT-UNSUP-017's (`new { A = /* c */ 1 }`). A named attribute argument written with a colon (`[Obsolete(message: /* c */ "old")]`) has no `=` and is not this shape. The `=` of an assignment or of an equals-value clause is FMT-UNSUP-020's.
- **Behind the `=`.** Only the gap between the `=` and the first token behind it counts. A comment in front of the `=` (`[N(Name /* c */ = 1)]`, `using Text /* c */ = System.Text;`) or behind the value's first token (`[N(Name = 1 /* c */)]`) does not refuse the input under this requirement.
- **On any line.** Whether the gap holds a line break does not matter: `[N(Name = /* c */ 1)]` on one line is refused like `[N(Name = // c⏎1)]`.
- **Every such `=`.** Each attribute argument and each using alias is checked with its own `=`, wherever it stands. A token in disabled text is no token of the input.

A node-level input holds the shape when it holds both the `=` and the comment, directive, or disabled text in the gap: the attribute argument, its attribute, and every node around them do, and so does the using directive. A node-level input that holds the value but not the `=` does not hold the shape, also with the comment in the leading trivia of its first token (FMT-UNSUP-003).

Where another shape of this spec refuses the same comment, directive, or disabled text, its reason and location are given: this shape is listed after every other. So FMT-UNSUP-004 keeps its reason for a comment behind the `=` of an attribute argument of a field declaration (`[N(Name = /* c */ 1)] private int _e;`), and FMT-UNSUP-010 for a directive there in a field declaration with several declarators.

The reason the formatter gives for this shape is `a comment or directive behind the = of an attribute argument or a using alias`, and its location is the span of the first comment, directive, or disabled text in that place.

Why: The maintainer decided to declare this shape unsupported in this work, next to FMT-UNSUP-020, with a reason of its own: to a user, the `=` of an attribute argument or of a using alias names an argument or a type, and is neither an assignment operator nor the `=` of a declaration. A comment behind the `=` keeps the value off the `=`'s line: the join of FMT-BREAK-015 does not cross it inside an attribute argument (FMT-BREAK-001), and behind a using alias's `=` no requirement joins the gap, so the target starts a continuation line (FMT-INDENT-034). A comment that explains the argument or the alias belongs above the attribute or the directive, or behind the value. Directives and disabled text are part of the shape alike. The shape is the same on one line, so the boundary is a place in the syntax tree. The declaration deletes no special case: the join of FMT-BREAK-015 refuses a comment behind an attribute argument's `=` by the refusal every wrap inside a list element goes through, which the other gaps inside an element keep needing, and no requirement joins the gap behind a using alias's `=`; the shape is declared on the maintainer's explicit answer. No phase writes a comment, a directive, or disabled text into the gap (FMT-UNSUP-005): the join of FMT-BREAK-015 carries only whitespace into it, and the phases that move or merge attributes and using directives move them with their trivia and change none inside them.

Example — each `=` is refused, and the input that holds it is returned unchanged:

```csharp
using System;
using Text = // alias
System.Text;

class NAttribute : Attribute
{
    public int Name { get; set; }
}

class C
{
    [N(Name = /* name */ 1)]
    void M()
    {
    }
}
```

The location is the span of `// alias`, which starts in line 2, column 14.

Example — an attribute argument alone is refused as well; the input is returned unchanged:

```csharp
using System;

class NAttribute : Attribute
{
    public int Name { get; set; }
}

class C
{
    [N(Name = // name
    1)]
    void M()
    {
    }
}
```

The location is the span of `// name`, which starts in line 10, column 15.

Example — the other side: none of these comments refuses the input under this requirement, and the input is formatted:

```csharp
using System;

using Text /* c1 */ = System.Text;

class NAttribute : Attribute
{
    public int Name { get; set; }
}

class C
{
    [Obsolete(message: /* c2 */ "old")]
    [N(Name /* c3 */ = 1)]
    [N(Name = 1 /* c4 */)]
    void M()
    {
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
    private int _a = 1 /* c */ + 2, _b;
}
```

```csharp
class C
{
    private int _a = 1 /* c */ + 2;
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

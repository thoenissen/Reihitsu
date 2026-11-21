# Formatter — Line breaks

| | |
|---|---|
| ID prefix | FMT-BREAK |
| Status | Active |
| Code owners | `LineBreakPhase`, `LineBreakBlockRewriter`, `LineBreakInitializerRewriter`, `LineBreakContainedBlockRewriter`, `LineBreakAssignmentRewriter`, `DeclarationSemicolonLineBreakRewriter`, `ListElementInteriorJoinRewriter`, `ListElementInteriorUtilities`, `AttachedPartLineStartRewriter`, `AttachedPartGapUtilities`, `LineBreakListRewriter`, `PropertyLayoutLineBreakRewriter`, `GenericConstraintLineBreakRewriter`, `DeclarationBraceLineBreakRewriter`, `MemberLineStartRewriter`, `AttributeTargetFormattingRewriter`, `AttributeTargetFormattingShared`, `BinaryOperatorLineBreakRewriter`, `ChainLineBreakRewriter`, `TernaryLineBreakRewriter`, `SwitchCaseWhenLineBreakRewriter`, `BracePlacer`, `TokenGapNormalizer`, `AccessorListLayout`, `LineBreakDetection`, `LineBreakTriviaUtilities` |
| Uses | FMT-BLANK-001, FMT-BLANK-002, FMT-BLANK-003, FMT-BLANK-004, FMT-BLANK-005, FMT-BLANK-012, FMT-BLANK-018, FMT-CLEAN-001, FMT-DOC-001, FMT-INDENT-003, FMT-INDENT-012, FMT-INDENT-032, FMT-PIPE-001, FMT-PIPE-002, FMT-PIPE-003, FMT-PIPE-004, FMT-PIPE-010, FMT-PIPE-014, FMT-PIPE-021, FMT-PIPE-025, FMT-PIPE-026, FMT-PIPE-034, FMT-SPACE-001, FMT-SPACE-002, FMT-SPACE-003, FMT-SPACE-008, FMT-SPACE-017, FMT-SPACE-022, FMT-STRUCT-009, FMT-STRUCT-014, FMT-STRUCT-026, FMT-STRUCT-028, FMT-STRUCT-029, FMT-SWITCH-003, FMT-UNSUP-008, FMT-UNSUP-009, FMT-UNSUP-011, FMT-UNSUP-014, FMT-UNSUP-015, FMT-UNSUP-016, SHARED-ATTR-014, SHARED-ATTR-018, SHARED-BLANK-008, SHARED-CHAIN-001, SHARED-CHAIN-002, SHARED-CHAIN-003, SHARED-CHAIN-004, SHARED-CHAIN-005, SHARED-CHAIN-006, SHARED-COMMENT-012, SHARED-STMT-001 |

This spec covers `LineBreakPhase`, the phase of the pipeline that decides where lines break (its position in the phase order is FMT-PIPE-001): brace placement, one statement and one member per line, the layout of argument, parameter, attribute, and type-argument lists, initializers and patterns, assignments, operators, conditionals, chains, generic constraints, attribute lists, accessor lists, the lines that switch sections, their labels, and their first statements start, and `case … when` guards. The phase adds and removes line breaks and keeps the blank lines it finds (FMT-BREAK-030), and it changes tokens only where it shapes an accessor's attribute lists (FMT-BREAK-032). The columns the resulting lines start in are decided later by FMT-INDENT, and the spaces between tokens on a line by FMT-SPACE.

## Non-goals

- Indentation and alignment of the lines this phase produces — FMT-INDENT. Examples here show the final output, so they include its alignment.
- How many blank lines separate statements, members, comments, and directives — FMT-BLANK. This phase keeps the blank lines it finds (FMT-BREAK-030).
- Whether an `if`/`else` body gets braces, whether a member is expression-bodied, and whether an empty type becomes a semicolon declaration — FMT-STRUCT. Braces of switch sections — FMT-SWITCH.
- Starting a wrap because a line is long. The phase has no line-length limit: it only normalizes wraps the author made, apart from the shapes it always lays out over several lines (braces, object initializers, nested conditionals).
- Unbraced embedded statements (`while (a) Foo();`), enum member lists, tuple expressions, and the arms of a switch expression keep the line breaks the author wrote. One switch-expression arm per line is a decided behavior change, tracked in thoenissen/Reihitsu#956.
- The position of the closing parenthesis in an argument list (as opposed to a parameter list, FMT-BREAK-011). Laying it out as in a parameter list is a decided behavior change, tracked in thoenissen/Reihitsu#956. The commas of a multi-line argument list are FMT-BREAK-012's.

## Requirements

### FMT-BREAK-001 — Joins never cross comments, directives, or disabled text

Wherever this phase joins two tokens onto one line, it MUST refuse the join when a comment, a preprocessor directive, or disabled text sits in the gap between them, and leave that gap's line breaks as written. Where it breaks a line, it MUST insert the line break in front of the moved token, so a comment that trailed the previous token stays at the end of the previous line.

Why: A `//` comment ends its line, so joining across it would comment out code; a directive must start its own line; and moving a comment from behind one token to behind another changes what the comment is about (FMT-PIPE-002). Every requirement below that joins lines is subject to this guard and does not repeat it.

Example — neither the comment nor the directive lets the second argument join the first:

```csharp
class C
{
    void M()
    {
        Foo(out /* why */
            var value);
        Foo(a, // first
            b);
        Foo(a,
#if X
            b);
#else
            c);
#endif
    }
}
```

is left unchanged.

### FMT-BREAK-002 — Braces start their own line

The opening and the closing brace of every namespace, type — a union declaration included —, member body, accessor body, statement block, lambda or anonymous-method block, local function, and switch statement MUST each start their own line, and the first token after an opening brace MUST start a new line. An empty body MUST therefore span two lines. A first statement behind the closing `*/` of a delimited comment that spans several lines is placed as FMT-BREAK-006 states, and the first section of a switch statement there as FMT-BREAK-033 states.

This holds for the node a node-level entry point formats as it holds for a document: when the target is a member body, a block, or an event's accessor list, its braces are placed, whether the declaration that owns it lies inside the formatted node or not.

Why: The repository uses Allman style throughout: a brace on its own line makes the extent of every scope visible at the left margin. An empty `{ }` is expanded too, so a body looks the same before and after its first statement is added. A node-level entry point formats its target as part of a document (FMT-PIPE-010), so the braces of a body it is given are its own to place, as they are when the whole file is formatted.

Example:

```csharp
namespace N {
    class C {
        void M() {
            Action a = () => { Foo(); };
        }

        void N() { }
    }
}
```

```csharp
namespace N
{
    class C
    {
        void M()
        {
            Action a = () =>
                       {
                           Foo();
                       };
        }

        void N()
        {
        }
    }
}
```

Example — `FormatNodeInDocumentAsync` with the body block of `M` as target:

```csharp
class C
{
    void M()
    { Foo(); }
}
```

```csharp
class C
{
    void M()
    {
        Foo();
    }
}
```

The same holds for the body of a constructor and of a local function, and for the accessor list of an event, each as the target.

Example — a union declaration's braces are placed as a class's are, and its members start their own lines (FMT-BREAK-007):

```csharp
union U(int, string) { public int X => 1; }
```

```csharp
union U(int, string)
{
    public int X => 1;
}
```

### FMT-BREAK-003 — A comment behind an opening brace stays there

When a comment follows an opening brace on the same line, the formatter MUST keep it on the brace's line and break the line behind it. A comment between a declaration and its opening brace MUST stay where it is and the brace MUST start the next line.

Why: Moving the brace onto its own line is a layout decision; moving the comment would be a content decision the formatter cannot make (FMT-BREAK-001).

Example:

```csharp
class C
{
    void M() /* c */ {
        Foo();
    }

    void O() { /* c */ }
}
```

```csharp
class C
{
    void M() /* c */
    {
        Foo();
    }

    void O()
    { /* c */
    }
}
```

### FMT-BREAK-004 — Chained keywords start their own line

`else`, `catch`, `finally`, and the `while` of a `do` statement that follows a block's closing brace MUST start their own line.

Why: With Allman braces, `} else {` would put a keyword between two braces on one line; each part of a compound statement starts at the left margin instead, like its braces.

Example:

```csharp
class C
{
    void M(bool a)
    {
        if (a) {
            Foo();
        } else {
            Bar();
        }

        try {
            Foo();
        } catch (Exception) {
            Bar();
        } finally {
            Baz();
        }

        do {
            Foo();
        } while (a);
    }
}
```

```csharp
class C
{
    void M(bool a)
    {
        if (a)
        {
            Foo();
        }
        else
        {
            Bar();
        }

        try
        {
            Foo();
        }
        catch (Exception)
        {
            Bar();
        }
        finally
        {
            Baz();
        }

        do
        {
            Foo();
        }
        while (a);
    }
}
```

### FMT-BREAK-005 — A comment behind an `if` condition moves above the `if`

Status: Retired — the maintainer decided that a comment behind the condition of an `if` stays where the author wrote it, as it does behind the head of every other block statement; FMT-BREAK-036 states the rule.

### FMT-BREAK-006 — One statement per line

Every statement in a block or a switch section that follows another statement of the same list MUST start its own line. A comment that trailed the previous statement MUST stay on that statement's line. An empty statement (`;`) and the statement next to it MUST be left as written, unless a requirement of FMT-BLANK asks for a blank line between the two (FMT-BLANK-001, FMT-BLANK-002, FMT-BLANK-003, FMT-BLANK-004, FMT-BLANK-005): then the second of them MUST start its own line too, and the blank-line phase, which runs after this phase, adds only the blank line. The first statement of a block starts its line behind the opening brace (FMT-BREAK-002), and the first statement of a switch section behind the section's labels (FMT-BREAK-035).

A statement that stands behind the closing `*/` of a delimited comment (`/* */` or `/** */`) that spans several lines, on that comment's last line, does not start its line (SHARED-COMMENT-012). The formatter MUST leave such a statement behind the `*/`, with two exceptions, in which it MUST start its own line, directly below the comment's last line:

- the statement follows another statement, a requirement of FMT-BLANK asks for a blank line between the two (FMT-BLANK-001 to FMT-BLANK-005), and the comment starts on the previous statement's line, behind its code;
- the statement is the first statement of a block or a switch section, and the comment starts on the line of the opening brace or of the section's last label, behind it.

The condition on the comment is where it starts: a comment that starts on a line of its own leaves the statement behind its `*/` in every case, also where a blank-line requirement applies — the blank line then goes above the comment (FMT-BLANK-018). A `/** */` comment written behind code is such a comment: behind the previous statement or an opening brace, FMT-DOC-001 puts it on a line of its own before this phase runs, and behind a section's last label it moves with the first statement as FMT-BREAK-035 states, so the statement stays behind its `*/` and neither exception applies. The condition on the pair is the blank-line requirement: behind a comment that starts on the previous statement's line, a statement no requirement of FMT-BLANK separates from that statement stays behind the `*/`.

Why: One statement per line keeps every statement visible at the left margin and gives each its own line in diffs and stack traces. Empty statements are left alone because splitting them off only moves a stray `;` around; they are rare and usually deliberate. Where a statement rule asks for a blank line next to one, the pair is separated anyway — the analyzer rules that use SHARED-STMT-001 report a statement on its predecessor's line too, and a blank line can stand only between two lines — and the line break is this phase's, so the blank-line phase writes no line break that ends a statement's line and the switch-brace phase measures the section on the lines it keeps (FMT-SWITCH-003, FMT-PIPE-025). Which pairs a statement rule separates is read from the one statement of that rule that the blank-line phase applies, not from a copy of it. A statement behind a multi-line comment's closing line stays where the author put it, as the formatter leaves code behind a comment on its line in many places; the maintainer accepted this placement (change doc *Line breaks are decided before switch braces and blank lines*, maintainer answer 2). It moves only where a line must end anyway: where a blank line is required and the comment's first line belongs to the previous statement, a blank line can stand only below that line, and a first statement behind a comment that starts behind the brace or the label would continue the line that FMT-BREAK-002 and FMT-BREAK-035 keep free of statements.

Example:

```csharp
class C
{
    void M()
    {
        Foo(); Qux();
        Bar(); /* d */ Baz();
        ; Foo();
    }
}
```

```csharp
class C
{
    void M()
    {
        Foo();
        Qux();
        Bar(); /* d */
        Baz();
        ; Foo();
    }
}
```

Where a statement rule asks for a blank line between an empty statement and its neighbour, the two are split, in a block and in a switch section alike, and the blank-line phase adds the blank line in the same run:

```csharp
class C
{
    int M(int a)
    {
        a++;
        ; return a;
    }

    void N(bool a)
    {
        if (a) { } ;
    }

    int O(int x)
    {
        switch (x)
        {
            case 1:
                ; return 1;
            default:
                return 0;
        }
    }
}
```

```csharp
class C
{
    int M(int a)
    {
        a++;
        ;

        return a;
    }

    void N(bool a)
    {
        if (a)
        {
        }

        ;
    }

    int O(int x)
    {
        switch (x)
        {
            case 1:
                {
                    ;

                    return 1;
                }
            default:
                {
                    return 0;
                }
        }
    }
}
```

Behind the closing line of a comment that spans several lines, a statement stays behind the `*/` unless a blank-line requirement separates it from a statement whose line the comment starts on (`return a;` in `M`), or it is the first statement of a section whose label line the comment starts on (`A();` in `O`). `B();` in `M` has no blank-line requirement, and the comments in front of `return a;` in `N` and `B();` in `O` start on lines of their own; the blank line in `N` goes above the comment:

```csharp
class C
{
    int M(int a)
    {
        A(); /* x
        */ B();
        a++; /* y
        */ return a;
    }

    int N(int a)
    {
        a++;
        /* z
        */ return a;
    }

    void O(int a)
    {
        switch (a)
        {
            case 1: /* p
            */ A();
                break;
            case 2:
                /* q
                */ B();
                break;
        }
    }
}
```

```csharp
class C
{
    int M(int a)
    {
        A(); /* x
        */ B();
        a++; /* y
        */

        return a;
    }

    int N(int a)
    {
        a++;

        /* z
        */ return a;
    }

    void O(int a)
    {
        switch (a)
        {
            case 1: /* p
            */
                A();
                break;

            case 2:
                /* q
                */ B();
                break;
        }
    }
}
```

### FMT-BREAK-007 — One member per line

Every member of a compilation unit, a namespace, a file-scoped namespace, or a type body — the body of a union declaration included — MUST start its own line. A comment that trailed the previous member MUST stay on that member's line.

Why: As with statements, a member that shares a line with another is easily overlooked when reading or reviewing a type.

Example:

```csharp
class C
{
    int _a; /* c */ int _b;
    void M()
    {
    } void N()
    {
    }
}
```

```csharp
class C
{
    int _a; /* c */
    int _b;
    void M()
    {
    }
    void N()
    {
    }
}
```

Example — the members of a union declaration:

```csharp
union U(int, string)
{
    public int X => 1; public int Y => 2;
}
```

```csharp
union U(int, string)
{
    public int X => 1;
    public int Y => 2;
}
```

### FMT-BREAK-008 — Switch braces start their own line

The braces of a switch statement and of a switch expression MUST each start their own line, and a block that forms a switch section's body MUST have its braces on their own lines. The arms of a switch expression keep the line breaks the author wrote.

Why: The same Allman rule as FMT-BREAK-002, applied to the two switch forms; a switch expression laid out over several lines reads like the statement it replaces.

Example:

```csharp
class C
{
    int M(int a)
    {
        switch (a) {
            case 1: { Foo(); break; }
        }

        return a switch { _ => 3 };
    }
}
```

```csharp
class C
{
    int M(int a)
    {
        switch (a)
        {
            case 1:
                {
                    Foo();

                    break;
                }
        }

        return a switch
               {
                   _ => 3
               };
    }
}
```

### FMT-BREAK-009 — Declaration semicolons stay on the declaration line

The terminating semicolon of a field, an event field, a delegate, and a property with an initializer MUST stay on the line that ends the declaration.

Why: A semicolon on a line of its own looks like an empty statement and splits a single declaration over lines for no reason. RH5113 reports the same shape.

Example:

```csharp
class C
{
    int _a
        ;
    event Action E
        ;
    int P { get; set; }
        = 5
        ;
}
delegate void D()
    ;
```

```csharp
class C
{
    int _a;
    event Action E;
    int P { get; set; } = 5;
}
delegate void D();
```

### FMT-BREAK-010 — The constructor initializer starts its own line

A constructor initializer (`: base(…)` or `: this(…)`) that shares the line of the parameter list MUST start its own line with its colon.

Why: The initializer is a call that runs before the body; on its own line it is visibly separate from the parameters it receives.

Example:

```csharp
class C
{
    C(int a)
        : base(a)
    {
    }

    C() : this(1)
    {
    }
}
```

```csharp
class C
{
    C(int a)
        : base(a)
    {
    }

    C()
        : this(1)
    {
    }
}
```

### FMT-BREAK-011 — Parameter lists hug their declaration

A comma between parameters MUST stay at the end of the previous parameter's line, and the closing parenthesis MUST stay on the line of the last parameter. This holds for every parameter list — methods, constructors, operators, delegates, local functions, records, primary constructors, the case-type list of a union declaration (`union U(int, string)`), anonymous methods, parenthesized lambdas, and indexers, whose bracketed parameter list (`this[int a, int b]`) has its brackets in place of the parentheses (FMT-PIPE-014). The opening parenthesis MUST stay on the declaration line for every one of these parameter lists except a parenthesized lambda's; the opening bracket of an indexer's parameter list stays on the line of `this`.

The opening parenthesis of a parenthesized lambda is the lambda's first token unless a modifier or a return type stands in front of it, and this requirement does not move it: the requirement that owns the gap in front of the lambda decides it — inside a list element FMT-BREAK-015 joins it, as the first element of a list FMT-BREAK-012 does, and as an assignment's value FMT-BREAK-016 does — and where no requirement joins that gap, the phase leaves its line break and its blank lines as written (FMT-BREAK-030). A line break that another requirement inserts in front of the lambda — in front of an element of an initializer (FMT-BREAK-021) or of a collection expression (FMT-BREAK-022), or of a list element (FMT-BREAK-012) — stays, as in front of any other expression.

Why: A parenthesis or comma at the start of a line separates the declaration from its parameters or a parameter from its separator, which is easy to misread. RH5105, RH5106, and RH5107 report the same shapes. A parenthesized lambda has no name in front of its parameter list: its opening parenthesis starts an expression and is placed by the code around it, as RH5105 leaves it unreported. A rule that moved it would compete with the owner of the gap in front of the lambda; as an element of a wrapped list the lambda keeps its own line (FMT-BREAK-012).

Example:

```csharp
class C
{
    void M
        (int a
         , int b)
    {
    }

    void N(int a, int b
        )
    {
    }
}
```

```csharp
class C
{
    void M(int a,
           int b)
    {
    }

    void N(int a, int b)
    {
    }
}
```

Example — an indexer's parameter list is laid out as a method's is: its opening bracket joins `this`, its commas end their lines, and its closing bracket joins the last parameter:

```csharp
class C
{
    int this
        [int a] => a;

    int this[int a
             , int b] => a;

    int this[string a, string b
        ] => 0;
}
```

```csharp
class C
{
    int this[int a] => a;

    int this[int a,
             int b] => a;

    int this[string a, string b] => 0;
}
```

Example — a union's case-type list is laid out as a primary constructor's parameter list: its opening parenthesis joins the union's name, its commas end their lines, its closing parenthesis joins the last case type, and a wrapped list keeps one case type per line (FMT-BREAK-012), aligned under the first (FMT-INDENT-012):

```csharp
union U1
    (int, string);

union U2(int
         , string
         , long
    );
```

```csharp
union U1(int, string);

union U2(int,
         string,
         long);
```

Example — the opening parenthesis of an anonymous method moves onto the `delegate` line; a parenthesized lambda's comma and closing parenthesis are placed like any other, while a lambda argument on its own line keeps that line and the blank line above it:

```csharp
using System;
using System.Linq;

class C
{
    void M(int[] items)
    {
        Func<int, int, int> f = delegate
            (int a, int b) { return a; };
        var total = items.Aggregate(0,
                                    (sum
                                     , item
                                    ) => sum + item);
        var other = items.Aggregate(0,

                                    (sum, item) => sum + item);
    }
}
```

```csharp
using System;
using System.Linq;

class C
{
    void M(int[] items)
    {
        Func<int, int, int> f = delegate(int a, int b)
        {
            return a;
        };
        var total = items.Aggregate(0,
                                    (sum,
                                     item) => sum + item);
        var other = items.Aggregate(0,

                                    (sum, item) => sum + item);
    }
}
```

Example — a lambda element of a collection initializer and of a wrapped collection expression starts its own line like any other element:

```csharp
using System;
using System.Collections.Generic;

class C
{
    void M()
    {
        var list = new List<Func<int, int>> { (x) => x, (x) => x };
        Func<int, int>[] col = [(x) => x,
                                (x) => x];
    }
}
```

```csharp
using System;
using System.Collections.Generic;

class C
{
    void M()
    {
        var list = new List<Func<int, int>>
                   {
                       (x) => x,
                       (x) => x
                   };
        Func<int, int>[] col = [
                                   (x) => x,
                                   (x) => x
                               ];
    }
}
```

### FMT-BREAK-012 — A list is on one line or one element per line

An argument list, a parameter list — the bracketed parameter list of an indexer included (FMT-BREAK-011) — and an attribute argument list MUST either have all its elements on one line or put every element on its own line. Which of the two a list gets is decided by its elements alone, with lines counted as SHARED-BLANK-008 counts them:

- A list is **multi-line** when two of its elements start on different lines — a line break in front of a comma separates the elements on either side of it — or when an element spans several lines (FMT-BREAK-013). A multi-line list MUST become one element per line, with the first element on the line of the opening parenthesis or bracket, unless a comment, a directive, or disabled text holds it off that line (third bullet).
- A list whose elements all start on one line, none of them spanning lines, is **single-line**, also when a line break stands behind its opening parenthesis or bracket. Its elements MUST stay on that line, and its first element MUST join the line of the opening parenthesis or bracket. A line break in front of the closing parenthesis of an argument list stays (Non-goals); the closing parenthesis or bracket of a parameter list is FMT-BREAK-011's.
- A first element that a comment, a preprocessor directive, or disabled text keeps off the line of the opening parenthesis or bracket — one of them stands between the opening delimiter and the element's first token, and a line break separates the two — cannot join that line (FMT-BREAK-001). It MUST stay at the start of its own line, and the list is then **multi-line**, also when all its elements start on that one line: every element MUST start its own line, and the first element stays where it is. Whether a line break separates them is decided as SHARED-BLANK-008 counts line breaks: in `Foo( /* a⏎ b */ 1, 2)` the line break is inside the text of the comment, the first element is on the opening line, and the list stays single-line; in `Foo( // c⏎1, 2)` and `Foo(⏎/* c */⏎1, 2)` the first element is held, and the list becomes one element per line. A line break behind the opening delimiter with no comment, directive, or disabled text in the gap (`Foo(⏎1, 2)`) holds nothing, and the list is joined.

In a multi-line argument list and a multi-line attribute argument list, every comma MUST end the line of the element in front of it, as in a parameter list (FMT-BREAK-011): a comma that starts a line moves behind the element in front of it, and the element behind it starts its own line. A comment behind such a comma, on the comma's line, MUST move with the comma and stay behind it, as in a parameter list: `Foo(1⏎, /* c */ 2, 3)` becomes `Foo(1, /* c */⏎2,⏎3)`, and a `//` comment behind the comma keeps ending the line. This covers every comment the parser lets trail the comma: each `//` and `/* */` comment, and every comment in the documentation mode `None`, where `Foo(1⏎, /** c */ 2)` becomes `Foo(1, /** c */⏎2)`. A documentation comment (`/** */`, `///`) behind the comma in the documentation modes `Parse` and `Diagnose` is not covered: it is a documentation comment written behind code, which the documentation-comment phase places before the comma moves (FMT-PIPE-001; FMT-DOC-001 states that phase's relocation, and does not yet decide this position). It goes onto a line of its own in front of the element behind the comma, as it does for the same code written comma-last (`Foo(1, /** c */ 2,⏎3)`): `Foo(1⏎, /** c */ 2)` becomes `Foo(1,⏎⏎/** c */ 2)`, with the blank line in front of the comment that FMT-BLANK-012 gives a relocated documentation comment, and `Foo(1⏎, /// c⏎2)` becomes `Foo(1,⏎⏎/// c⏎2)`.

No comment, directive, or disabled text stands between an element and a comma on a later line: the formatter refuses an input that holds one there (FMT-UNSUP-009), so every comma that starts a line can move behind the element in front of it. A comment in front of a comma on the element's own line, such as `/* one */` in `Foo(1 /* one */, 2,⏎3)`, stays where it is.

Why: A partly wrapped list hides elements at the end of a line. Whether a list is wrapped is a question about its elements: a line break only behind the opening parenthesis puts every element on one line, so the list reads as a one-line list and is joined like one. A first element that a comment or a directive holds off the opening line already stands on a line of its own; joining it would cross the comment or the directive (FMT-BREAK-001), so the list is already wrapped at its first element and is laid out as a multi-line list around the held element. RH5102 and RH5109 do not report such a list when its elements all start on one line; the formatter may do more than the rules ([P11](../../README.md#the-formatter-may-do-more-than-the-analyzers-p11)). Such a list is rare: RH5035 and RH5038 report a comment inside a parameter or argument list. Keeping the first element on the opening line lets the remaining elements align under it (FMT-INDENT). A comma at the end of a line shows that the list continues, as it does in a parameter list (FMT-BREAK-011). A comment behind a comma stays behind it, as in a parameter list and in a constraint clause (FMT-BREAK-037), so it goes where the comma goes. A comma that a comment or a directive kept off the element's line could not move without crossing it (FMT-BREAK-001); the formatter refuses that shape instead of laying the list out around it (FMT-UNSUP-009). RH5101, RH5102, and RH5109 report partly wrapped lists.

Example:

```csharp
[Bar(
    1,
    2)]
class C
{
    void M(int a, int b,
           int c)
    {
        Foo(a, b,
            c);
        Foo(
            a,
            b);
        Foo(
            a, b);
        Foo(a, b, c);
    }
}
```

```csharp
[Bar(1,
     2)]
class C
{
    void M(int a,
           int b,
           int c)
    {
        Foo(a,
            b,
            c);
        Foo(a,
            b);
        Foo(a, b);
        Foo(a, b, c);
    }
}
```

Example — a comma at the start of a line makes the list multi-line and moves behind the element in front of it, in an argument list and in an attribute argument list:

```csharp
using System;

[AttributeUsage(AttributeTargets.All
    , AllowMultiple = true, Inherited = false)]
class C
{
    void M()
    {
        Foo(1
            , 2, 3);
        Foo(1
            , 2,
            3);
        Foo(a
            , b);
        Foo(Bar(1
                , 2), 3);
    }
}
```

```csharp
using System;

[AttributeUsage(AttributeTargets.All,
                AllowMultiple = true,
                Inherited = false)]
class C
{
    void M()
    {
        Foo(1,
            2,
            3);
        Foo(1,
            2,
            3);
        Foo(a,
            b);
        Foo(Bar(1,
                2),
            3);
    }
}
```

Example — a comment behind a comma that moves goes with the comma, and a block comment in front of a comma on the element's own line keeps nothing:

```csharp
class C
{
    void M()
    {
        Foo(1
            , /* c */ 2, 3);
        Foo(1
            , // c
            2, 3);
        Foo(1 /* one */, 2,
            3);
    }
}
```

```csharp
class C
{
    void M()
    {
        Foo(1, /* c */
            2,
            3);
        Foo(1, // c
            2,
            3);
        Foo(1 /* one */,
            2,
            3);
    }
}
```

Example — a first element that a comment or a directive keeps off the line of the opening parenthesis stays on its own line, and the list becomes one element per line:

```csharp
class C
{
    void N( // c
        int a, int b)
    {
    }

    void M()
    {
        Foo( // c
            1, 2);
        Foo(
#pragma warning disable CS0168
            1, 2);
#pragma warning restore CS0168
    }
}
```

```csharp
class C
{
    void N( // c
           int a,
           int b)
    {
    }

    void M()
    {
        Foo( // c
            1,
            2);
        Foo(
#pragma warning disable CS0168
            1,
            2);
#pragma warning restore CS0168
    }
}
```

Example — an indexer's parameter list, wrapped between two of its parameters, gets one parameter per line, and one wrapped only behind its opening bracket is joined:

```csharp
class C
{
    int this[int a, int b,
             int c] => a;

    int this[
        string a, string b] => 0;
}
```

```csharp
class C
{
    int this[int a,
             int b,
             int c] => a;

    int this[string a, string b] => 0;
}
```

### FMT-BREAK-013 — A multi-line element splits its list

When an element of an argument, parameter, or attribute argument list spans several lines — a nested wrapped list, a lambda with a block body, an initializer — the list MUST put every element on its own line, as if it were wrapped. This holds for the bracketed parameter list of an indexer as for any other parameter list (FMT-BREAK-011).

Why: An element that spans lines already breaks the list's one-line shape; continuing the list on the element's last line would hide the following elements behind it.

Example:

```csharp
class C
{
    void M()
    {
        Foo(a, Bar(b,
                   c));
        Foo(x => { Bar(); }, b);
    }
}
```

```csharp
class C
{
    void M()
    {
        Foo(a,
            Bar(b,
                c));
        Foo(x =>
            {
                Bar();
            },
            b);
    }
}
```

### FMT-BREAK-014 — Indexer arguments and angle-bracket lists are joined

A wrapped bracketed argument list that holds an index, type argument list, type parameter list, and function-pointer parameter list MUST be joined onto one line, unless an element itself spans several lines. The join only rewrites the gaps between the delimiters and the elements, so a comment behind the closing delimiter does not block it.

A bracketed argument list holds an index when its parent in the syntax tree is an `ElementAccessExpressionSyntax` (`m[1, 2]`, also behind a null-forgiving operator, `d![1, 2]`), the `ElementBindingExpressionSyntax` of a conditional element access (the `[1, 2]` of `c?[1, 2]`), or an `ImplicitElementAccessSyntax` (`[1, 2] = 3` in an object or dictionary initializer). Which of the three it is MUST NOT change whether the list is joined: `m[1,⏎2]`, `d![1,⏎2]`, and `c?[1,⏎2]` all become one line. The bracketed argument list of a fixed-size buffer's declarator (`fixed int b[3];`, parent `VariableDeclaratorSyntax`) holds a size, not an index, and this requirement does not decide it. For a conditional element access, this requirement owns the gaps from the list's `[` to its `]`; the gap in front of the `?` and the gap between the `?` and the `[` are FMT-BREAK-040's.

Why: These lists are short by nature and read as part of a name or an index; wrapping them breaks up a type or an access expression without making it clearer. The `?` of a conditional element access and the `!` of a null-forgiving operator in front of the brackets change what the access does when the receiver is null, not what its index is, so the index reads the same behind them. RH5307 reports a wrapped index list with the same guard.

Example:

```csharp
class C<T,
        U>
{
    void M()
    {
        var x = m[1,
                  2];
        var l = new List<int,
                         string>();
    }
}
```

```csharp
class C<T, U>
{
    void M()
    {
        var x = m[1, 2];
        var l = new List<int, string>();
    }
}
```

Example — the list of an element access behind a null-forgiving operator and the list of a conditional element access, with an opening bracket on its own and a list behind another conditional element access, are joined alike; a comment between the brackets keeps the list as written (FMT-BREAK-001):

```csharp
#nullable enable

class C
{
    void M(int[,]? c, int[,] d, int[,][,]? e)
    {
        var a = c?[1,
                   2];
        var g = c?[
            1, 2];
        var i = e?[0,
                   1]?[1,
                       2];
        var h = c?[1, // note
                   2];
        var b = d![1,
                   2];
    }
}
```

```csharp
#nullable enable

class C
{
    void M(int[,]? c, int[,] d, int[,][,]? e)
    {
        var a = c?[1, 2];
        var g = c?[1, 2];
        var i = e?[0, 1]?[1, 2];
        var h = c?[1, // note
                   2];
        var b = d![1, 2];
    }
}
```

### FMT-BREAK-015 — Wraps inside a list element are joined

A line inside an element of an argument, parameter, attribute argument, tuple, or type-argument list that starts at a token other than the element's first token MUST be joined onto the previous line. A gap whose layout another requirement owns MUST NOT be joined by this rule: list boundaries, the opening parenthesis of a parameter list other than a lambda's, the opening parenthesis of an argument list (FMT-BREAK-039), the gap in front of an element access, a conditional element access, or a null-forgiving operator and the gap inside a conditional element access (FMT-BREAK-040), operator wraps of binary, conditional, member-access, postfix, `is`-pattern, and assignment expressions (FMT-BREAK-016 to FMT-BREAK-020), multi-line literals, attribute placement, and every line inside a nested braced, bracketed, or query construct. The `=` of a parameter's default value is not such an operator wrap: the gap in front of it is a wrap inside the element, and this rule joins it, in a bracketed parameter list as in a parenthesized one (FMT-BREAK-016 defers to it there). The gap behind it is not this rule's: FMT-BREAK-016 places the value, as for every equals-value clause.

Why: A wrap such as `out⏎var value` or `message:⏎"x"` splits one element over two lines without structuring anything. Joining runs before the list layout, so a list that was multi-line only because of such a wrap is laid out as the one-line list it becomes. No analyzer rule reports these wraps; the join is a formatter-only decision.

Example:

```csharp
class C
{
    void M()
    {
        Foo(out
            var value, message:
            "x");
        Bar(x =>
            x + 1);
        Foo(a
            + b, c);
    }
}
```

```csharp
class C
{
    void M()
    {
        Foo(out var value, message: "x");
        Bar(x => x + 1);
        Foo(a
            + b,
            c);
    }
}
```

### FMT-BREAK-016 — Assignments keep target, operator, and value together

The operator of an assignment MUST stay on the line of its target, and the value MUST start on the line of the operator. This holds for every assignment operator — `=` and every compound assignment operator, such as `+=`, `-=`, `|=`, `<<=`, and `??=` — and for the `=` of a variable declarator, a property initializer, an enum member's value, and a parameter's default value (FMT-PIPE-014). A value that spans several lines starts on the operator line and keeps its own layout; a collection initializer or collection expression assigned to a target keeps its opening delimiter on the operator line. Inside a parameter list, this requirement does not decide the gap in front of a parameter's default `=`: FMT-BREAK-015 owns it and joins it as a wrap inside a list element, which gives the layout stated here. The value behind the `=` is placed by this requirement, for a parameter's default as for every equals-value clause, so a multi-line literal default starts on the `=` line (`string s = """`) although FMT-BREAK-015 does not join a multi-line literal.

A binary operator is not an assignment operator: `a⏎+ b` is laid out by FMT-BREAK-017, while `a⏎+= b` is joined by this requirement.

Why: Splitting the target from its value makes the reader search for what is assigned to what, whichever operator assigns it and whichever construct the `=` belongs to. RH5111 reports the same shapes for the `=` of an assignment, a declarator, a property, an enum member, and a parameter; the formatter lays out the compound assignment operators alike as well (FMT-PIPE-014).

Example:

```csharp
class C
{
    void M()
    {
        var x =
            Foo();
        x
            = Bar();

        var w =
            """
            a
            """;
    }
}
```

```csharp
class C
{
    void M()
    {
        var x = Foo();
        x = Bar();

        var w = """
                a
                """;
    }
}
```

Example — a compound assignment, an enum member's value, and a parameter's default value are laid out as an assignment with `=` is, while a wrapped binary operator keeps its wrap; the parameter list, wrapped only inside its elements, becomes the one-line list it is once they are joined (FMT-BREAK-015 in front of each `=`, this requirement behind it):

```csharp
enum E
{
    A
        = 1,
    B =
        2
}

class C
{
    void M(int a
               = 1, string s =
                   null)
    {
        a
            += 1;
        s ??=
            "x";
        a = a
            + 1;
    }
}
```

```csharp
enum E
{
    A = 1,
    B = 2
}

class C
{
    void M(int a = 1, string s = null)
    {
        a += 1;
        s ??= "x";
        a = a
            + 1;
    }
}
```

### FMT-BREAK-017 — Binary operators start the continuation line

When a binary expression is wrapped at its operator, the operator MUST start the continuation line instead of ending the previous one. A pattern combinator (`and`, `or`) MUST start its own line when an operand next to it is a property, list, or parenthesized pattern that spans several lines; otherwise it keeps the author's layout. The formatter never wraps a binary expression the author wrote on one line.

Why: An operator at the start of a line shows at a glance how the line continues the expression, and lines up the operands of a long condition. RH5302 reports the same shape for logical expressions. The join of the right operand onto its operator's line never meets a comment, a directive, or disabled text: the formatter refuses an input that holds one there (FMT-UNSUP-011), so FMT-BREAK-001 never has to refuse this join.

Example:

```csharp
class C
{
    void M()
    {
        var x = a &&
                b;
        var z = a + b
                + c * d;
    }
}
```

```csharp
class C
{
    void M()
    {
        var x = a
                && b;
        var z = a + b
                + c * d;
    }
}
```

### FMT-BREAK-018 — Conditional expressions are on one line or one branch per line

A conditional expression written on one line MUST stay on one line. A conditional expression that spans several lines, and every conditional expression that contains a nested conditional in a branch, MUST put `?` and `:` at the start of their own lines, each followed by its branch; nested conditionals are laid out the same way.

Why: A partly wrapped conditional mixes the two shapes and hides a branch at the end of a line. A nested conditional on one line is hard to parse, so it is always spread out. RH5308 reports the same shapes. The join of a branch onto its operator's line never meets a comment, a directive, or disabled text: the formatter refuses an input that holds one there (FMT-UNSUP-008), so FMT-BREAK-001 never has to refuse this join.

Example:

```csharp
class C
{
    int M(bool a, bool b)
    {
        var x = a ? 1 : 2;
        var y = a ?
                1 :
                2;

        return a ? 1 : b ? 2 : 3;
    }
}
```

```csharp
class C
{
    int M(bool a, bool b)
    {
        var x = a ? 1 : 2;
        var y = a
                    ? 1
                    : 2;

        return a
                   ? 1
                   : b
                       ? 2
                       : 3;
    }
}
```

### FMT-BREAK-019 — Chains follow the shared chain layout

The formatter MUST lay out member-access chains as the shared chain policy specifies: it never starts a wrap (SHARED-CHAIN-002), joins the first link to the root's line (SHARED-CHAIN-003) unless a comment, directive, or disabled text is in the way (SHARED-CHAIN-004), starts every later call of a wrapped chain on its own line, and the first call when the line the root ends on does not reach it (SHARED-CHAIN-005), and moves a line break out of a link operator (SHARED-CHAIN-006).

A link whose operator stands behind the closing `*/` of a comment that spans several lines does not start its line (SHARED-COMMENT-012). In a wrapped chain, the formatter MUST break the line in front of its operator as SHARED-CHAIN-005 asks, whatever column the comment's last line starts in — for a later link, and, by the exception SHARED-CHAIN-005 states, for a prefix link and for the first invoked link too.

The line break the formatter moves out of a link operator (SHARED-CHAIN-006) never has a comment, a directive, or disabled text beside it: the formatter refuses an input that holds one between the tokens of a link operator (FMT-UNSUP-015), so FMT-BREAK-001 never has to refuse that join, and every token of a link operator stands on the line of the operator's first token.

Why: The analyzer rules RH5112, RH5201, and RH5048 report against the same policy; a formatter that laid out chains differently would produce code those rules report. The other examples are in the shared spec.

Example — `.D` stands behind the comment's last line, so the line breaks in front of it and `.D` aligns with the other calls:

```csharp
class C
{
    object M(dynamic a)
    {
        var r = a.B()
                 .C()

                 /* x
        */ .D();

        return r;
    }
}
```

```csharp
class C
{
    object M(dynamic a)
    {
        var r = a.B()
                 .C()

                 /* x
        */
                 .D();

        return r;
    }
}
```

### FMT-BREAK-020 — Split member names are rejoined, and split attached parts are no wrap

A member name separated from its dot by a line break MUST be joined back to the dot, and such a split MUST NOT count as a wrap of the chain. The attached parts of a chain (SHARED-CHAIN-001) are joined onto the line of the element in front of them by FMT-BREAK-039 (an argument list) and FMT-BREAK-040 (an element access, a conditional element access, a null-forgiving operator), in a chain as everywhere else, and a line break in front of an attached part MUST NOT count as a wrap of the chain either.

The join of a member name onto its dot never meets a comment, a directive, or disabled text: the formatter refuses an input that holds one between the `.` of a simple member access or a member binding and its name (FMT-UNSUP-016), so FMT-BREAK-001 never has to refuse this join, and no member name of a link starts a line.

Why: `items.⏎Where` and `Where⏎(x)` split a single step of the chain rather than separating two steps. Treating the trailing dot as a wrap would turn a stray line break into a fully wrapped chain. The join of an attached part is stated once, for chains and every other expression, by the requirements that own its gap.

Example — the argument list behind `Where` in `b` is joined by FMT-BREAK-039:

```csharp
class C
{
    void M()
    {
        var a = items.
            Where(x => x.A);
        var b = items.Where
                    (x => x.A)
                     .Select(x => x.B);
    }
}
```

```csharp
class C
{
    void M()
    {
        var a = items.Where(x => x.A);
        var b = items.Where(x => x.A)
                     .Select(x => x.B);
    }
}
```

### FMT-BREAK-021 — Object initializers are always laid out over several lines

The initializer of an object creation (`new Foo { … }`, `new() { … }`), a collection initializer, an anonymous object creation, and the initializer of a `with` expression MUST put the opening brace, every element, and the closing brace on their own lines, also when the author wrote it on one line. A collection initializer assigned to a member inside an object initializer (`Items = { … }`) keeps its opening brace on the `=` line. Array initializers (`new int[] { 1, 2 }`, `int[] f = { 1, 2 }`) keep the author's layout, apart from a comma that starts a line, which FMT-BREAK-038 moves. The commas of every initializer this requirement names are FMT-BREAK-038's.

Why: An initializer sets several independent values; one per line makes each visible and gives each its own diff line. A `with` initializer sets values the same way. Array initializers are often long lists of literal data, for which one element per line would explode the file. Moving a comma that starts a line keeps the number of lines: only the side of the comma the line break stands on changes, so the array keeps its author's line count (FMT-BREAK-038). RH5301 and RH5303 report the same shapes, apart from the `with` initializer, which no rule reports.

Example:

```csharp
class C
{
    void M()
    {
        var a = new List<int> { 1, 2 };
        var b = new Foo { A = 1 };
        var d = new { A = 1 };
        var c = new int[] { 1, 2, 3 };
    }
}
```

```csharp
class C
{
    void M()
    {
        var a = new List<int>
                {
                    1,
                    2
                };
        var b = new Foo
                {
                    A = 1
                };
        var d = new
                {
                    A = 1
                };
        var c = new int[] { 1, 2, 3 };
    }
}
```

A `with` initializer, also in an expression-bodied property:

```csharp
class C
{
    R P => _r with { A = 1, B = 2 };

    void M()
    {
        var q = p with { X = 1, Y = 2 };
    }
}
```

```csharp
class C
{
    R P => _r with
              {
                  A = 1,
                  B = 2
              };

    void M()
    {
        var q = p with
                  {
                      X = 1,
                      Y = 2
                  };
    }
}
```

### FMT-BREAK-022 — Wrapped collection expressions and patterns get one element per line

A collection expression, a list pattern, a property pattern clause, a parenthesized pattern, and a complex element initializer (`{ 1, 2 }` inside a dictionary initializer) written on one line MUST stay on one line. When one of them spans several lines, every element MUST start its own line, the first element MUST start the line after the opening delimiter, and the closing delimiter MUST start its own line. The opening brace of a wrapped property pattern MUST start its own line, except in a pattern introduced directly by a `case` label, where it stays on the label's line. A comma that starts a line in one of them is FMT-BREAK-038's.

A `with(…)` element of a collection expression (`[with(comparer), "a"]`) is an element like any other: in a wrapped collection expression it starts its own line, the first line after the `[`.

Why: These constructs are expressions inside an expression; on one line they read as a value. Once wrapped, the same one-element-per-line rule as for initializers applies. The `case` exception keeps the pattern from being mistaken for the section body. The `with(…)` element passes arguments to the collection's construction rather than adding an item, but it is written in the element list and separated by a comma like an item, so it is laid out like one. RH5305 and RH5306 report the same shapes.

Example:

```csharp
class C
{
    bool M(object o)
    {
        int[] e = [1, 2, 3];
        int[] f = [1,
                   2];

        return o is Point { X: 1,
                            Y: 2 } or null;
    }
}
```

```csharp
class C
{
    bool M(object o)
    {
        int[] e = [1, 2, 3];
        int[] f = [
                      1,
                      2
                  ];

        return o is Point
                 {
                     X: 1,
                     Y: 2
                 }
                 or null;
    }
}
```

Example — a wrapped collection expression that starts with a `with(…)` element:

```csharp
using System;
using System.Collections.Generic;

class C
{
    void M()
    {
        HashSet<string> s = [with(StringComparer.Ordinal),
                             "a", "b"];
    }
}
```

```csharp
using System;
using System.Collections.Generic;

class C
{
    void M()
    {
        HashSet<string> s = [
                                with(StringComparer.Ordinal),
                                "a",
                                "b"
                            ];
    }
}
```

### FMT-BREAK-023 — Each generic constraint starts its own line

Every `where` clause of a type — a union declaration included —, method, local function, or delegate MUST start its own line, also on a member without a body and on a type declared with a semicolon.

Why: Constraints are part of the signature but not of its name or parameters; on their own lines they stay readable when there are several. RH5110 reports the same shape.

Example:

```csharp
class C<T> where T : class, new()
{
    void M<U, V>() where U : class where V : struct
    {
    }
}

interface I
{
    void M<T>() where T : class;
}
```

```csharp
class C<T>
    where T : class, new()
{
    void M<U, V>()
        where U : class
        where V : struct
    {
    }
}

interface I
{
    void M<T>()
        where T : class;
}
```

Example — the clauses of a union declaration:

```csharp
union U<T>(T, string)where T : class;

union V<T, S>(T, S) where T : class where S : struct
{
    public int X => 1;
}
```

```csharp
union U<T>(T, string)
    where T : class;

union V<T, S>(T, S)
    where T : class
    where S : struct
{
    public int X => 1;
}
```

### FMT-BREAK-024 — Simple auto-property accessor lists are on one line

The accessor list of a property or indexer whose accessors are all auto accessors without bodies, comments, or directives MUST be on one line together with the declaration (`int P { get; set; }`), also when an initializer follows. An accessor list with an attribute on an accessor MUST stay on its line when the declaration is on one line and MUST otherwise be laid out over several lines; the spaces inside a list on one line are decided by FMT-SPACE-022.

Why: An auto-property is a single declaration; spreading `get;` and `set;` over four lines adds height without information. RH5408 reports the same shape.

Example:

```csharp
class C
{
    int P
    {
        get; set;
    } = 5;

    int Q { get;
        set; }
}
```

```csharp
class C
{
    int P { get; set; } = 5;

    int Q { get; set; }
}
```

### FMT-BREAK-025 — Other accessor lists put one accessor per line

Every other accessor list — of properties, indexers, and events with accessor bodies or expression bodies, or with a comment or directive inside — MUST have its braces on their own lines and each accessor starting its own line. A comment behind an accessor MUST stay on that accessor's line.

Why: Once an accessor has a body, the list is a block of members, and the Allman rule of FMT-BREAK-002 applies to it like to a type body.

Example:

```csharp
class C
{
    int R { get { return _r; } set { _r = value; } }

    event Action E { add { } remove { } }

    int Q
    {
        get; // c
        set;
    }
}
```

```csharp
class C
{
    int R
    {
        get => _r;
        set => _r = value;
    }

    event Action E
    {
        add
        {
        }
        remove
        {
        }
    }

    int Q
    {
        get; // c
        set;
    }
}
```

The accessor bodies of `R` become expression bodies by FMT-STRUCT-009.

### FMT-BREAK-026 — Expression-bodied properties and indexers are on one line with their arrow

The `=>` of an expression-bodied property or indexer and the start of its expression MUST be on the line of the declaration. The phase joins them with the separator of FMT-BREAK-031 and writes no other whitespace around the arrow, also where it joins nothing; the arrow is spaced by FMT-SPACE-017.

Why: An expression-bodied property is a one-line declaration by intent (FMT-STRUCT-014 creates them from get-only properties); a wrapped arrow makes it look like a statement. RH5401 reports the same shape for properties, RH5419 for indexers. The arrow of a property is spaced like every other arrow, so its spacing is not this phase's.

Example:

```csharp
class C
{
    int S
        => 5;

    int T =>
        5;

    int this[int i]
        => i;
}
```

```csharp
class C
{
    int S => 5;

    int T => 5;

    int this[int i] => i;
}
```

### FMT-BREAK-027 — Attribute lists of declarations start their own line

Each attribute list of a type, member, accessor, or other declaration MUST start its own line, and the declaration MUST start its own line behind its last list. Attribute lists of parameters and type parameters MUST stay on the parameter's line. The attribute lists of an accessor whose accessor list stays on the declaration's line (FMT-BREAK-024) MUST stay on that line. How many attributes a list holds is decided before this phase (FMT-STRUCT-028, FMT-STRUCT-029), and for the lists of an accessor by FMT-BREAK-032. This requirement leaves the placement of a list with a comment or directive between its brackets unspecified, and the placement of a `param:` or `typevar:` list on an owner other than a parameter or type parameter.

Why: A list on its own line keeps each attribute visible and diffable on declarations, while parameter attributes stay inline so that a parameter list keeps one parameter per line. The RH55xx attribute placement rules report the same shapes.

Example:

```csharp
class C
{
    [A, B] void M([C] int a, [D, E] int b) { }

    [return: F] [G] int N() { return 0; }
}
```

```csharp
class C
{
    [A]
    [B]
    void M([C] int a, [D, E] int b)
    {
    }

    [return: F]
    [G]
    int N()
    {
        return 0;
    }
}
```

### FMT-BREAK-028 — A `when` guard follows a one-line `case` pattern

The `when` clause of a `case` label MUST stay on the label's line when the pattern before it is on one line, and MUST start its own line when the pattern spans several lines.

Why: A short guard reads as part of its label; behind a multi-line pattern it would be lost at the end of the pattern's last line.

Example:

```csharp
class C
{
    void M(object o)
    {
        switch (o)
        {
            case int i
                when i > 0:
                break;
        }
    }
}
```

```csharp
class C
{
    void M(object o)
    {
        switch (o)
        {
            case int i when i > 0:
                break;
        }
    }
}
```

### FMT-BREAK-029 — Whitespace in front of an inserted line break is left to the cleanup phase

When the phase moves a token onto a new line by writing a line break into that token's leading trivia and leaves the trailing trivia of the token before it in place, it MUST NOT remove the whitespace that ends that trivia; the cleanup phase removes it (FMT-CLEAN-001), so the final output holds none. This holds wherever the token before the line break stands, inside the node the phase lays out or outside it.

This requirement reaches only that case. Where the phase rewrites the trailing trivia of the token before — it moves a line break out of it, as when it rewraps a list, or it normalizes the gap in front of a token it places, such as a `}` behind `M();` or a brace of an initializer — it may remove the whitespace together with that rewrite; the final output is the same, because FMT-CLEAN-001 removes whatever whitespace is left.

Why: Whitespace at the end of a line has one owner, the cleanup phase, which removes it in front of every line break. A second removal in this phase, which reaches only the tokens of the node it rewrites, makes the phase's output depend on where a node starts. Whitespace that ends a line decides nothing in the phases between: the spacing phase leaves a gap with a line break as it is (FMT-SPACE-001), and the indentation phase places lines by their first token.

Example — in

```csharp
class C
{
    void M() {
        if (true) {
            M();
        }
    }
}
```

the phase moves both `{` onto lines of their own. Its output keeps the space behind `void M()` and the space behind `if (true)`; the final output holds neither:

```csharp
class C
{
    void M()
    {
        if (true)
        {
            M();
        }
    }
}
```

### FMT-BREAK-030 — The phase keeps the blank lines of every gap it does not join

In every gap between two tokens that this phase leaves on separate lines, whether it inserts the line break or keeps one, it MUST leave the number of blank lines as it found it: it MUST NOT insert a blank line and MUST NOT remove one. Where it joins two tokens onto one line, the gap holds no line break afterwards, so its blank lines go with the join; whether it may join is decided by the requirements above and FMT-BREAK-001.

Why: Blank lines are decided by FMT-BLANK alone, in the blank-line phase, which runs after this phase (FMT-PIPE-001). A phase that also decided them would make one decision in two places (FMT-PIPE-025): the blank-line phase would have to undo a blank line this phase inserted, or restore one it removed. Keeping the count hands every blank line to FMT-BLANK as the author wrote it.

Example — run alone, this phase leaves the input below unchanged, with the blank line above the block statement and the blank line above the comment glued to the initializer's brace:

```csharp
class C
{
    void M(bool a)
    {
        if (a)
        {
        }

        {
        }

        var p = new P
                {
                    Child =

                    /* c */ {
                                X = 2
                            }
                };
    }
}
```

In the whole pipeline, FMT-BLANK keeps the first of these blank lines and removes the second after this phase runs.

### FMT-BREAK-031 — A join writes the minimal separator, and the phase writes no other space

Where the phase joins two tokens onto one line, it MUST leave the gap between them without whitespace, unless the two tokens written without whitespace would not read back as the same two tokens — they would read as one token, as another pair of tokens, or as the start of a comment; there it MUST write exactly one space. The phase MUST NOT write, remove, or change whitespace in any other gap between two tokens that are on the same line before and after the phase. The spaces of every gap on one line, a joined gap included, are decided by FMT-SPACE (FMT-SPACE-002).

This holds for every join of the phase: a list element's interior wrap (FMT-BREAK-015), a list, its delimiters, and its separators (FMT-BREAK-011 to FMT-BREAK-014), an assignment's operator and value (FMT-BREAK-016), the branch behind a `?` or `:` that moves to the next line (FMT-BREAK-018), a chain's member names (FMT-BREAK-019, FMT-BREAK-020), an argument list's opening parenthesis and the token in front of it (FMT-BREAK-039), an element access, a conditional element access, or a null-forgiving operator and the token in front of it (FMT-BREAK-040), a binary operator that moves to the start of the next line and the operand behind it (FMT-BREAK-017), an accessor list and its accessors (FMT-BREAK-024), the arrow and expression of a property or indexer (FMT-BREAK-026), a `when` guard (FMT-BREAK-028), a declaration's semicolon (FMT-BREAK-009), and the token behind an attribute list (FMT-BREAK-027).

Why: A space chosen by the kind of the joined tokens is a spacing decision, which FMT-SPACE owns; when this phase made it, the same pair was spaced one way when joined and another way when written on one line, and the phase spaced constructs such as a property's arrow even where it joined nothing. Two tokens that would read as one are the exception, because the phase's text has to be its tree (FMT-PIPE-021) and the program must not change (FMT-PIPE-003): `protected` and `internal` joined without a space read as the identifier `protectedinternal`, `-` and `-b` as `--b`, and `/` and `*p` as the start of a comment. The structural transforms follow the same rule for the tokens they create (FMT-STRUCT-026).

Example — run alone, this phase turns

```csharp
class C
{
    public int P
        => 1;

    public int Q
    {
        get;
        protected
        internal set;
    }

    unsafe int M(bool b, int a, int* p)
    {
        N(name:
          a);

        var x = a -
                -a;
        var y = a /
                *p;

        return b ?
               1 :
               2;
    }
}
```

into a tree whose text holds `public int P=> 1;`, `public int Q{get;protected internal set;}`, `N(name:a);`, `var x = a` followed by `- -a;` on the next line, `var y = a` followed by `/ *p;`, and `return b` followed by `?1` and `:2` on lines of their own. The final output is spaced by FMT-SPACE:

```csharp
class C
{
    public int P => 1;

    public int Q { get; protected internal set; }

    unsafe int M(bool b, int a, int* p)
    {
        N(name: a);

        var x = a
                - -a;
        var y = a
                / *p;

        return b
                   ? 1
                   : 2;
    }
}
```

A gap the phase does not join keeps its whitespace: `public int R =>1;` and `public int S {get;set;}` written on one line leave the phase as they are.

One join is a recorded exception: where the phase merges the attribute lists of an accessor (FMT-BREAK-032), the shared merge that the attribute-list code fixes also use writes one space behind the merged list's `]` and behind a token it leaves touching a joined token. The spacing phase decides the same space for both pairs (FMT-SPACE-003, FMT-SPACE-008), so the final output does not depend on it. The exception is recorded as long as the phase merges the attribute lists of accessors. It covers only those two spaces: inside the merged list the merge writes no whitespace, and the gap behind each comma between two attributes, also behind a comma the first list held with a space in the input, is left without whitespace for the spacing phase (FMT-BREAK-032).

### FMT-BREAK-032 — The attribute lists of an accessor follow its accessor list's layout

In an accessor list that this phase lays out over several lines (FMT-BREAK-024, FMT-BREAK-025), an attribute list of an accessor that holds several attributes MUST be split into one list per attribute, as FMT-STRUCT-028 states for a declaration, with its conditions and with what it states a created list holds between its brackets. A list whose specifier is `param:` or `typevar:` is the exception there, as in FMT-STRUCT-028: it MUST NOT be split, and the accessor's `param:` lists and its `typevar:` lists MUST each be merged into the first of them, as FMT-STRUCT-029 states and with its conditions. In an accessor list that stays on the declaration's line (FMT-BREAK-024), the attribute lists of an accessor MUST NOT be split, and its lists of one target MUST be merged into the first of them, as FMT-STRUCT-029 states for a parameter and with its targets and conditions. In either layout, a merge of an accessor's lists keeps no line break of the stretch it rewrites, and the space it writes is FMT-BREAK-031's recorded exception. The merge MUST write every comma between two attributes of the merged list — a comma it creates between the attributes of two merged lists and a comma the first list already held — with no trivia in front of it or behind it, no whitespace and no elastic trivia (FMT-PIPE-034); the space behind each such comma is the horizontal-spacing phase's (FMT-SPACE-008), and the recorded exception does not cover it. A comma the first list already held lies in the stretch the merge rewrites, so it loses a space it had behind it in the input, and the spacing phase writes that space again; the final output is the same. The merge of the attribute-list code fixes is no part of this phase and keeps its output (SHARED-ATTR-018). Which of the two layouts an accessor list has is the layout this phase gives it, not the one it was written in: an accessor list written on one line that the phase lays out over several lines is split, and one written over several lines that the phase joins onto the declaration's line is merged. This holds for the accessors of properties, indexers, and events.

Why: The shape follows from where the lists stand. On lines of their own, each attribute gets a line, as on any other declaration; on the declaration's line, a second list only repeats the brackets, and the list-shape rules do not report it there (SHARED-ATTR-014). A `param:` or `typevar:` list keeps the parameter shape in either layout, as on every owner (FMT-STRUCT-029). Whether an accessor list stays on the declaration's line is decided by this phase, after it joins an initializer's value (FMT-BREAK-016) and lays out the accessor list, so the shape of its attribute lists is decided here, behind that layout; a structural transform would have to predict it.

Example:

```csharp
class C
{
    int P { [A] [B] get; [C, D] set; }

    int Q
    {
        [A, B] get;
        [C] [D] set;
    }

    int R { [A, B] get => 1; }

    int S { [A] [B] get; set; } =
        1;
}
```

```csharp
class C
{
    int P { [A, B] get; [C, D] set; }

    int Q
    {
        [A]
        [B]
        get;
        [C]
        [D]
        set;
    }

    int R
    {
        [A]
        [B]
        get => 1;
    }

    int S { [A, B] get; set; } = 1;
}
```

Example — in an accessor list over several lines, the `param:` lists of a setter are merged, not split:

```csharp
class C
{
    int _v;

    int T
    {
        get => _v;
        [param: A] [param: B] set => _v = value;
    }
}
```

```csharp
class C
{
    int _v;

    int T
    {
        get => _v;
        [param: A, B] set => _v = value;
    }
}
```

### FMT-BREAK-033 — Every switch section starts its own line

The first token of every switch section that follows another section of the same switch statement MUST start its own line, whatever statement ends the section before it. Whether the token starts its line is decided by SHARED-BLANK-008: a section behind a `//` comment or a directive starts its line and is kept; a section on the line of the previous section's last token, or behind the last line of a multi-line comment there, does not, and the phase MUST insert a line break in front of it, so a comment that trailed the previous section stays on that section's last line (FMT-BREAK-001). Comments in front of a token that moves move with it when they are the token's own leading trivia and stand on the token's line; a delimited comment stands on the line it starts on. On the line of the previous token, the C# parser ends the previous token's trailing trivia in front of the first documentation comment (`/** */`), so these are the comments from that first `/** */` comment up to the token, of any kind and however many: they MUST move with the token as written, in their order. An ordinary comment in front of that first `/** */` comment trails the previous token and MUST stay on its line (FMT-BREAK-001). The token starts its line when the first of the comments that move with it starts its line; where the phase inserts the line break, it MUST insert it in front of that comment, so the comments and the token start the new line together. FMT-BREAK-034 and FMT-BREAK-035 apply this rule to a label and a first statement. The first section of a switch statement starts its line behind the opening brace (FMT-BREAK-002). The phase inserts no blank line in front of a section (FMT-BREAK-030); the blank line behind a section that ends in `break` is FMT-BLANK-004's.

A section whose first token stands behind the closing `*/` of a delimited comment (`/* */` or `/** */`) that spans several lines, on that comment's last line, does not start its line (SHARED-COMMENT-012). Where the comment starts decides whether it moves:

- when the comment starts on the line of the token in front of it, behind that token — the last token of the previous section, or, for the first section, the switch statement's opening brace — the phase MUST insert the line break in front of the section, directly below the comment's last line, whatever statement ends the previous section;
- when the comment starts on a line of its own, the phase MUST leave the section behind the `*/`.

A `/** */` comment behind the previous token is among the comments that move with the token, as stated above, so it starts a line of its own and the section stays behind its `*/`. FMT-BREAK-034 applies the same placement to a label. The blank line that FMT-BLANK-004 asks for in front of a section behind a `break` statement goes below the comment's last line where the line breaks, and above the comment where the section stays behind it (FMT-BLANK-018).

This requirement, FMT-BREAK-034, and FMT-BREAK-035 decide the lines inside the node that a node-level entry point formats; the gap in front of the target itself is kept as FMT-PIPE-010 states.

Why: A switch is read by its labels: each section is one path, and its first label at the label column (FMT-INDENT-003) is where a reader finds it. A section that shares a line with the section before it hides its label in the middle of that line. Until now only a section behind a `break` statement got a line of its own, as a side effect of its blank line; behind `return`, `throw`, `goto`, or `continue` the sections stayed on one line. A `/** */` comment in front of a token is part of that token's leading trivia, and the formatter already keeps it in front of that token when it moves documentation off code (FMT-DOC-001); separating the two would leave the comment on a line above, away from what it documents. The comments behind it on the same line stand between the documentation and its token, so they move along and the comments keep their order; an ordinary comment in front of the documentation comment trails the previous token, as any trailing comment does. A section or a label behind a multi-line comment's closing line stays where the author put it, as a statement does (FMT-BREAK-006): the formatter leaves code behind a comment on its line in many places, and the maintainer accepted this placement for sections and labels as for statements (change doc *Line breaks are decided before switch braces and blank lines*, maintainer answer 3). It moves only where the comment starts behind the previous token: there the comment's first line belongs to that token's line, and a section or a label behind it would continue the line of the previous section, label, or brace.

Example:

```csharp
class C
{
    int M(int x)
    {
        switch (x)
        {
            case 1:
                return 1; case 2:
                goto case 1; /* c */ case 3:
                throw null;
            default:
                return 0;
        }
    }
}
```

```csharp
class C
{
    int M(int x)
    {
        switch (x)
        {
            case 1:
                return 1;
            case 2:
                goto case 1; /* c */
            case 3:
                throw null;
            default:
                return 0;
        }
    }
}
```

Example — behind the last line of a multi-line comment, a section does not start its line and is moved; behind a `//` comment, it starts its line and is kept:

```csharp
class C
{
    int M(int x)
    {
        switch (x)
        {
            case 1:
                return 1; /* a
                b */ case 2:
                return 2; // c
            case 3:
                return 3;
        }
    }
}
```

```csharp
class C
{
    int M(int x)
    {
        switch (x)
        {
            case 1:
                return 1; /* a
                b */
            case 2:
                return 2; // c
            case 3:
                return 3;
        }
    }
}
```

Example — behind the closing line of a multi-line comment, a section or a label moves when the comment starts behind the token in front of it (`case 1:` behind the opening brace, `case 3:` behind `return 2;`, `case 4:` behind `case 3:`), and stays behind the `*/` when the comment starts on a line of its own (`case 2:`, `case 5:`):

```csharp
class C
{
    int M(int x)
    {
        switch (x)
        { /* a
          */ case 1:
                return 1;
            /* b
            */ case 2:
                return 2; /* c
                */ case 3: /* d
            */ case 4:
            /* e
            */ case 5:
                return 5;
        }
    }
}
```

```csharp
class C
{
    int M(int x)
    {
        switch (x)
        { /* a
          */
            case 1:
                return 1;
            /* b
            */ case 2:
                return 2; /* c
                */
            case 3: /* d
            */
            case 4:
            /* e
            */ case 5:
                return 5;
        }
    }
}
```

Example — a `/** */` comment in front of a section keeps its place in front of the label. Behind a statement, FMT-DOC-001 already moves it onto a line of its own before this phase runs, so the section starts its line behind the comment and is kept; the blank line of FMT-BLANK-012 in front of the comment is added after this phase:

```csharp
class C
{
    int M(int x)
    {
        switch (x)
        {
            case 1:
                return 1; /** d */ case 2:
                return 2;
        }
    }
}
```

```csharp
class C
{
    int M(int x)
    {
        switch (x)
        {
            case 1:
                return 1;

            /** d */ case 2:
                return 2;
        }
    }
}
```

### FMT-BREAK-034 — Each label of a switch section starts its own line

Every label of a switch section that follows another label of the same section MUST start its own line (SHARED-BLANK-008); when it does not, the phase MUST insert a line break in front of it, so a comment that trailed the label before it stays on that label's line (FMT-BREAK-001). The comments in front of the label move with it as FMT-BREAK-033 states: from the first `/** */` comment on the previous label's line up to the label, the line break goes in front of them, and an ordinary comment in front of them stays on the previous label's line. This holds for `case` labels, pattern labels with or without a `when` guard, and `default` labels alike. A label behind the closing `*/` of a delimited comment that spans several lines is placed as FMT-BREAK-033 places a section there: the line break goes in front of it when the comment starts on the previous label's line, behind that label, and the label stays behind the `*/` when the comment starts on a line of its own.

Why: Stacked labels select one shared body, and each of them names a value the section handles. One label per line puts every value at the label column, where FMT-BREAK-033 puts the first label of every section, and gives each label its own line in diffs.

Example:

```csharp
class C
{
    void M(int x)
    {
        switch (x)
        {
            case 1: case 2:
                A();
                break;
            case 3: /* c */ default:
                B();
                break;
            case 4: /** f */ case 5:
                B();
                break;
        }
    }

    void A()
    {
    }

    void B()
    {
    }
}
```

```csharp
class C
{
    void M(int x)
    {
        switch (x)
        {
            case 1:
            case 2:
                A();
                break;

            case 3: /* c */
            default:
                B();
                break;

            case 4:
            /** f */ case 5:
                B();
                break;
        }
    }

    void A()
    {
    }

    void B()
    {
    }
}
```

Example — two `/** */` comments in front of a label move with it together; an ordinary comment in front of the first `/** */` comment stays on the previous label's line:

```csharp
class C
{
    void M(int x)
    {
        switch (x)
        {
            case 1: /* c */ /** d */ case 2:
                A();
                break;

            case 3: /** a */ /** b */ case 4:
                A();
                break;
        }
    }

    void A()
    {
    }
}
```

```csharp
class C
{
    void M(int x)
    {
        switch (x)
        {
            case 1: /* c */
            /** d */ case 2:
                A();
                break;

            case 3:
            /** a */ /** b */ case 4:
                A();
                break;
        }
    }

    void A()
    {
    }
}
```

### FMT-BREAK-035 — The first statement of a switch section starts its own line

The first statement of a switch section MUST start its own line (SHARED-BLANK-008); when it does not, the phase MUST insert a line break in front of it, so a comment that trailed the section's last label stays on the label's line (FMT-BREAK-001). The comments in front of the statement move with it as FMT-BREAK-033 states: from the first `/** */` comment on the label's line up to the statement, the line break goes in front of them and the first of them starts the statement's line, and an ordinary comment in front of them stays on the label's line. This holds for every kind of statement, a `break` statement included; a block that forms the section's body has its braces placed by FMT-BREAK-008. An empty statement (`;`) that is the first statement of a section MUST be left as written, as FMT-BREAK-006 leaves it between statements, and a first statement behind the closing `*/` of a delimited comment that spans several lines is placed as FMT-BREAK-006 states.

Why: The label introduces the section's statements and is not one of them. A statement on the label's line stands at the label column instead of the statement column (FMT-INDENT-003), so the section's statements do not line up, and the first one is easily overlooked when another is added below it. Every later statement of a section already starts its own line (FMT-BREAK-006); the first one now does too. An empty statement is left alone for FMT-BREAK-006's reason: moving it only moves a stray `;` around.

Example:

```csharp
class C
{
    void M(int x, object o)
    {
        switch (x)
        {
            case 1: A(); break;
            case 2: /** d */ A();
                break;
            default: break;
        }

        switch (o)
        {
            case int i when i > 0: /* c */ A();
                break;
        }
    }

    void A()
    {
    }
}
```

```csharp
class C
{
    void M(int x, object o)
    {
        switch (x)
        {
            case 1:
                A();
                break;

            case 2:
                /** d */ A();
                break;

            default:
                break;
        }

        switch (o)
        {
            case int i when i > 0: /* c */
                A();
                break;
        }
    }

    void A()
    {
    }
}
```

Example — which comments move with the first statement: everything from the first `/** */` comment on, as written and in order, including the whole of a `/** */` comment that spans lines; an ordinary comment in front of the first `/** */` comment stays on the label's line:

```csharp
class C
{
    void M(int x)
    {
        switch (x)
        {
            case 1: /* c */ /** d */ A();
                break;

            case 2: /** d */ /* c */ A();
                break;

            case 3: /** a
                b */ A();
                break;
        }
    }

    void A()
    {
    }
}
```

```csharp
class C
{
    void M(int x)
    {
        switch (x)
        {
            case 1: /* c */
                /** d */ A();
                break;

            case 2:
                /** d */ /* c */ A();
                break;

            case 3:
                /** a
                b */ A();
                break;
        }
    }

    void A()
    {
    }
}
```

Example — run alone, this phase leaves the input below unchanged: the empty first statement stays on the label's line, and `break` stays next to it (FMT-BREAK-006):

```csharp
class C
{
    void M(int x)
    {
        switch (x)
        {
            case 1: ; break;
        }
    }
}
```

### FMT-BREAK-036 — A comment behind the head of a block statement stays there

When the phase puts the opening brace of the body of an `if`, `else if`, `else`, `while`, `for`, `foreach`, `using`, or `lock` statement on its own line, a comment that trails the token in front of that brace — the closing parenthesis of the head, or the `else` keyword — MUST stay behind that token, on its line. This holds whether the brace stood on the next line or on the line of the comment.

This requirement specifies the outcome only for the statements it names and leaves every other statement unspecified.

Why: The author put the comment on the line of the head, so it describes the head: the condition, the loop, or the resource. The brace starts its own line below it (FMT-BREAK-002), so the comment stands neither between head and brace nor glued to the brace. Moving it onto a line of its own above the statement would make it the only comment the formatter takes off the line it was written on, and the blank-line phase decides the blank line above a comment on the line the comment ends up on (FMT-PIPE-026).

Example:

```csharp
class C
{
    void M(bool a, bool b, int[] xs)
    {
        while (b)
            if (a) // why
            {
                Foo();
            }

        if (a) /* m */ { Foo(); }
        else if (b) // c
        {
            Bar();
        }

        foreach (var x in xs) /* f */ { Foo(); }
    }
}
```

```csharp
class C
{
    void M(bool a, bool b, int[] xs)
    {
        while (b)
            if (a) // why
            {
                Foo();
            }

        if (a) /* m */
        {
            Foo();
        }
        else if (b) // c
        {
            Bar();
        }

        foreach (var x in xs) /* f */
        {
            Foo();
        }
    }
}
```

### FMT-BREAK-037 — The constraints of a clause are on one line or one constraint per line

This requirement applies to every `where` clause, whichever declaration owns it — a type, a method, a local function, a delegate, or an extension block.

In this requirement, a line break is one that SHARED-BLANK-008 counts: an end-of-line trivia, or the line break that ends a single-line comment, a `///` comment, or a directive. A line break inside the text of a multi-line comment (`/* … */`) is not one: two tokens that only such a comment puts on different lines of the text are not separated by a line break, for the joins, for whether the clause is wrapped, and for whether a constraint or the colon starts a line. A comma behind such a comment on a later line of the text is refused (FMT-UNSUP-009).

The phase MUST first join the line breaks inside a clause that structure nothing: a line break between the clause's type parameter and its colon MUST be joined, so that the colon stands on the type parameter's line, and a line break between two tokens of one constraint MUST be joined — in a qualified or alias-qualified name, in front of the `<` of a type argument list, in `new()`, `class?`, or `allows ref struct`. The gaps of a type argument list, from its `<` to its `>`, are not this requirement's: FMT-BREAK-014 and FMT-BREAK-015 join them. A line break that those requirements, and the requirements of a type argument's own construct, keep — for example one inside a tuple type argument wrapped over two lines (`IEquatable<(int,⏎ string)>`) — stays, and the constraint then spans two lines.

The clause MUST then be judged as these joins leave it. It is wrapped when a line break separates its colon from its first constraint, when a line break separates its colon from its type parameter because a comment, a directive, or disabled text keeps the join from happening, or when a line break separates two tokens of its constraints, from the first token of the first constraint to the last token of the last one — in front of or behind a comma between two constraints, or inside one constraint where the joins leave it. A wrapped clause MUST become one constraint per line: its first constraint MUST move onto the line of the colon, every later constraint MUST start a line — a line break MUST separate it from the token in front of it — that no other constraint of the clause shares, and every comma between two constraints MUST stand at the end of the line of the constraint in front of it. Where a multi-line comment stands behind a comma, the later constraint behind the comment's closing line does not start a line, and the line break goes behind the comment, which stays behind the comma (FMT-BREAK-001). In a clause that is not wrapped, this requirement changes nothing beyond the joins of the first step, also where multi-line comments put its colon or its constraints on different lines of the text.

A join MUST NOT cross a comment, a directive, or disabled text (FMT-BREAK-001). Where one stands in a gap, the line break stays: a colon kept off the type parameter's line starts its own line and wraps the clause, a first constraint kept off the colon's line starts its own line, and a line break inside a constraint that one keeps wraps the clause. A comment, a directive, or disabled text between a constraint and a comma on a later line is a shape the formatter refuses (FMT-UNSUP-009), so no comma is kept off the previous line. The phase keeps the blank lines of every gap it does not join (FMT-BREAK-030).

Why: A partly wrapped clause hides constraints at the end of a line, as a partly wrapped list hides elements (FMT-BREAK-012). With the first constraint on the colon's line and the commas at the end of the line, as in a parameter list (FMT-BREAK-011), the later constraints have a constraint to align under (FMT-INDENT-032). A wrap between the type parameter and the colon, or inside one constraint, splits one part of the clause over two lines without structuring anything, as FMT-BREAK-015 reasons for wraps inside a list element, and the clause is judged after those joins as a list is. A constraint that a comment or a directive keeps over several lines breaks the clause's one-line shape as a multi-line element breaks a list's (FMT-BREAK-013). A colon that one keeps on its own line already takes the clause off the line of its `where` keyword, so its constraints follow the one-per-line layout of every other wrapped clause instead of a shape of their own. Whether a clause is wrapped, and whether a part of it starts a line, is decided on the same line-start facts as every other list (SHARED-BLANK-008): a line break inside a multi-line comment is part of the comment's text and does not wrap a parameter list either (FMT-BREAK-012; `void N(int a, /* x⏎ y */ int b, int c)` stays on one line), and in a wrapped list a parameter behind such a comment's closing line gets a line of its own behind the comment, as a constraint does here. RH5110 reports only the position of the `where` keyword, so this layout is a formatter-only decision.

Example — a clause wrapped between constraints, behind its colon, or in front of a comma becomes one constraint per line; a clause on one line stays:

```csharp
using System;

class C
{
    void M<T>()
        where T : class, IComparable<T>,
            IEquatable<T>, new()
    {
    }

    void N<T>()
        where T :
            IDisposable
    {
    }

    void O<T>()
        where T :
            class,
            new()
    {
    }

    void P<T>()
        where T : class
            , new()
    {
    }

    void Q<T>()
        where T : class, new()
    {
    }
}
```

```csharp
using System;

class C
{
    void M<T>()
        where T : class,
                  IComparable<T>,
                  IEquatable<T>,
                  new()
    {
    }

    void N<T>()
        where T : IDisposable
    {
    }

    void O<T>()
        where T : class,
                  new()
    {
    }

    void P<T>()
        where T : class,
                  new()
    {
    }

    void Q<T>()
        where T : class, new()
    {
    }
}
```

Example — the colon joins the type parameter's line, and a wrap inside a constraint that nothing holds is joined before the clause is judged, so these clauses end on one line:

```csharp
using System.Collections.Generic;

class C
{
    void M<T>()
        where T
            : class, new()
    {
    }

    void N<T>()
        where T : class, IDictionary<int,
            string>, new()
    {
    }

    void O<T>()
        where T : class, System.
            IDisposable, new()
    {
    }
}
```

```csharp
using System.Collections.Generic;

class C
{
    void M<T>()
        where T : class, new()
    {
    }

    void N<T>()
        where T : class, IDictionary<int, string>, new()
    {
    }

    void O<T>()
        where T : class, System.IDisposable, new()
    {
    }
}
```

Example — a constraint that a comment keeps over two lines wraps its clause; the constraint's inner line follows the constraint:

```csharp
using System.Collections.Generic;

class C
{
    void M<T>()
        where T : class, IDictionary<int, // c
            string>, new()
    {
    }
}
```

```csharp
using System.Collections.Generic;

class C
{
    void M<T>()
        where T : class,
                  IDictionary<int, // c
                              string>,
                  new()
    {
    }
}
```

Example — a colon that a comment keeps on its own line wraps its clause, also when its constraints are written on one line; a clause whose colon is joined is judged as usual:

```csharp
class C
{
    void M<T>()
        where T // c
            : class, new()
    {
    }

    void N<T>()
        where T
            : class, new()
    {
    }
}
```

```csharp
class C
{
    void M<T>()
        where T // c
            : class,
              new()
    {
    }

    void N<T>()
        where T : class, new()
    {
    }
}
```

Example — comments and directives block a join but not a split: a comment behind the type parameter keeps the colon on the next line, a comment or a directive behind the colon keeps the first constraint on the next line, and a comment behind a comma stays behind it:

```csharp
using System;

class C
{
    void M<T>()
        where T // c
            : class,
            new()
    {
    }

    void N<T>()
        where T : // c
            class,
            new()
    {
    }

    void O<T>()
        where T :
#if !X
            class,
#endif
            new()
    {
    }

    void R<T>()
        where T : class, /* c */ new(),
            IDisposable
    {
    }
}
```

```csharp
using System;

class C
{
    void M<T>()
        where T // c
            : class,
              new()
    {
    }

    void N<T>()
        where T : // c
                  class,
                  new()
    {
    }

    void O<T>()
        where T :
#if !X
                  class,
#endif
                  new()
    {
    }

    void R<T>()
        where T : class, /* c */
                  new(),
                  IDisposable
    {
    }
}
```

Example — a line break inside a multi-line comment wraps nothing: the comment's closing line does not put the colon (`M`), the first constraint (`N`), or a later constraint (`P`) on a line of its own, so these clauses stay as written. In `S`, the line break in front of `IDisposable` wraps the clause; `new()` behind the comment's closing line does not start a line, so a line break goes behind the comment, which stays behind its comma:

```csharp
using System;

class C
{
    void M<T>()
        where T /* a
            b */ : class, new()
    {
    }

    void N<T>()
        where T : /* a
            b */ class, new()
    {
    }

    void P<T>()
        where T : class, /* a
            b */ new(), IDisposable
    {
    }

    void S<T>()
        where T : class, /* a
            b */ new(),
            IDisposable
    {
    }
}
```

```csharp
using System;

class C
{
    void M<T>()
        where T /* a
            b */ : class, new()
    {
    }

    void N<T>()
        where T : /* a
            b */ class, new()
    {
    }

    void P<T>()
        where T : class, /* a
            b */ new(), IDisposable
    {
    }

    void S<T>()
        where T : class, /* a
            b */
                  new(),
                  IDisposable
    {
    }
}
```

### FMT-BREAK-038 — A comma that starts a line in an initializer, a collection expression, or a pattern ends the element's line

This requirement covers the commas of an object, collection, anonymous-object, or `with` initializer and of a collection initializer assigned to a member (FMT-BREAK-021), of an array initializer (`new int[] { … }`, `new[] { … }`, `int[] a = { … }`, `stackalloc int[] { … }`), and of a collection expression, a list pattern, a property pattern clause, and a complex element initializer (FMT-BREAK-022) — the commas between two elements and a trailing comma behind the last element. A comma **starts a line** when it stands on a later line of the text than the line on which the token in front of it ends: every line break in the text between the two counts, as FMT-UNSUP-009 counts them. `X = 3⏎, Y = 4` starts a line; `X = 3, Y = 4` and `X = 3,⏎Y = 4` do not. No comment, directive, or disabled text stands in that gap: the formatter refuses an input that holds one there (FMT-UNSUP-009), so no line break inside the text of a comment is ever counted, and the count is the one SHARED-BLANK-008 gives as well.

A comma that starts a line MUST move to the end of the line of the token in front of it, and the line break that stood in front of it then stands behind it: the element behind the comma starts its own line, also in an array initializer, whose other line breaks stay as written (FMT-BREAK-021). A comma alone on its line leaves no line behind. A comment behind such a comma, on the comma's line, MUST move with the comma and stay behind it, as in an argument list (FMT-BREAK-012): a block comment followed by an element puts the line break behind the comment, and a `//` comment keeps ending the line. As in an argument list, this covers every comment the parser lets trail the comma — each `//` and `/* */` comment, and every comment in the documentation mode `None` (`X = 3⏎, /** c */ Y = 4` becomes `X = 3, /** c */⏎Y = 4`) — and not a documentation comment in the documentation modes `Parse` and `Diagnose`, which the documentation-comment phase places first (FMT-PIPE-001, FMT-DOC-001), as for the same code written comma-last: there `X = 3⏎, /** c */ Y = 4` becomes `X = 3,⏎⏎/** c */ Y = 4`, the comment in front of the element on a line of its own. A comma that does not start a line is not changed by this requirement.

Why: A comma at the end of a line shows that the list continues, as it does in a parameter, argument, or attribute argument list (FMT-BREAK-011, FMT-BREAK-012), and an element behind it then starts its line like every other element of the construct. A comma left alone between two element lines belongs to neither. In an array initializer the move keeps the number of lines and only changes which side of the comma the line break stands on, so the array keeps its author's line count (FMT-BREAK-021). A comment behind a comma goes where the comma goes, as in every other list. A documentation comment behind a comma is the exception in the modes that parse documentation: the documentation-comment phase runs first and moves it onto a line of its own, and keeping it behind the comma would only last until the next run, which moves it again (FMT-PIPE-004); so the comma-first form gives what the comma-last form gives. A comma that a comment or a directive kept off the element's line could not move without crossing it (FMT-BREAK-001); the formatter refuses that shape (FMT-UNSUP-009) rather than giving such a comma a line and a column of its own. Comma-first code is a style people write and no rule reports it in most of these constructs, so it gets a decided layout; only a comma that a comment or a directive holds is refused.

Example — a comma that starts a line ends the line of the element in front of it, a comment behind it goes with it, and a comma alone on its line leaves no line behind:

```csharp
class C
{
    void M(object o, P p)
    {
        var a = new D(1, 2) { X = 3
            , };
        var b = new D { X = 3
            , /* c */ Y = 4
            , // d
            Z = 5 };
        int[] d = [1
            , 2];
        var h = new { A = 1
            , B = 2 };
        var q = p with { X = 1
            , Y = 2 };
        var r = o is P { X: 1
            , Y: 2 };
    }
}
```

```csharp
class C
{
    void M(object o, P p)
    {
        var a = new D(1, 2)
                {
                    X = 3,
                };
        var b = new D
                {
                    X = 3, /* c */
                    Y = 4, // d
                    Z = 5
                };
        int[] d = [
                      1,
                      2
                  ];
        var h = new
                {
                    A = 1,
                    B = 2
                };
        var q = p with
                  {
                      X = 1,
                      Y = 2
                  };
        var r = o is P
                  {
                      X: 1,
                      Y: 2
                  };
    }
}
```

Example — in an array initializer the comma moves and the element behind it starts the next line; the array's other line breaks stay as written:

```csharp
class C
{
    void M()
    {
        var e = new int[]
        {
            1
            , 2
        };
        var f = new int[]
        {
            1, 2
            , 3, 4
        };
        int[] y = { 1
            , 2 };
    }
}
```

```csharp
class C
{
    void M()
    {
        var e = new int[]
                {
                    1,
                    2
                };
        var f = new int[]
                {
                    1, 2,
                    3, 4
                };
        int[] y = { 1,
                      2 };
    }
}
```

### FMT-BREAK-039 — An argument list starts on the line of the code in front of it

When a line break separates the opening parenthesis of an argument list from the token in front of it, the parenthesis MUST be joined onto that token's line. This holds for the argument list of an invocation, whatever expression is invoked (`Foo⏎(4)`, `(f ?? g)⏎(1)`, `Make(1)⏎(2)`), of an object creation (`new List<int>⏎(4)`) and an implicit object creation (`new⏎(4)`), of a constructor initializer (`: base⏎(1)`, `: this⏎()`), of a primary-constructor base type (`class E() : B⏎(1)`), and of the `with(…)` element of a collection expression (`[with⏎(4)]`), wherever the argument list stands: in a chain, inside a list element, inside a lambda's block, and everywhere else. This requirement is the one owner of that gap; FMT-BREAK-020 and FMT-BREAK-015 leave it to it. Blank lines in the gap go with the join (FMT-BREAK-030).

Where a construct around the argument list is laid out by whether it spans several lines, the line break the join removes counts as follows:

- For every construct FMT-BREAK-022 decides — a collection expression, a list pattern, a property pattern clause, a parenthesized pattern, a complex element initializer — it still counts: one that spans several lines only because of it gets one element per line, as it does when FMT-BREAK-020 rejoins a split inside one of its elements.
- For a list FMT-BREAK-012 lays out and for a conditional expression (FMT-BREAK-018) it does not count: the list or the conditional is judged as the line it becomes. For a list this holds wherever the argument list stands inside an element: directly in the element, and also inside a construct nested in the element whose lines FMT-BREAK-015 does not join, such as an interpolation hole, a query clause after the first, or an array or `stackalloc` initializer (`Baz($"{Bar⏎(1)}", 2)` becomes `Baz($"{Bar(1)}", 2)`), in a chain or not. The constructs of the first bullet are the other side: a collection expression nested in an element is judged as written.

This requirement specifies the outcome only for the argument lists it names. It does not decide the gap in front of the `[` of a bracketed argument list, nor in front of the `(` of an attribute argument list.

Why: An argument list belongs to the expression or keyword in front of it; on a line of its own it reads as a parenthesized expression that starts a new line, and no column the indentation phase can give it shows that it belongs to the code above. The phase already joined such an argument list in a chain (FMT-BREAK-020) and inside a list element (FMT-BREAK-015); this requirement makes the join everywhere and states it once. A surrounding construct counts the removed line break as it counts the phase's other joins inside it: the constructs of FMT-BREAK-022 keep the one-element-per-line layout their author's line breaks give them, as after FMT-BREAK-020 rejoins a split inside an element, and lists and conditional expressions are judged as the line they become, as after a join of FMT-BREAK-015 inside a list and the joins of FMT-BREAK-015 and FMT-BREAK-020 inside a conditional expression. A list is judged so wherever the argument list stands in an element, also inside an interpolation hole, a query clause after the first, or an array or `stackalloc` initializer of the element, so the line break the join removes never counts for the list itself; it reaches the list only through a construct of the first bullet nested in the element, such as a collection expression, which is judged as written. No analyzer rule reports such an argument list ([P11](../../README.md#the-formatter-may-do-more-than-the-analyzers-p11)); RH5105 reports the opening parenthesis of a parameter list only. The join never meets a comment, a directive, or disabled text: the formatter refuses an input that holds one in the gap (FMT-UNSUP-014), so FMT-BREAK-001 never has to refuse this join.

Example:

```csharp
using System;
using System.Collections.Generic;

class B
{
    public B(int x)
    {
    }
}

class D : B
{
    public D()
        : base
        (1)
    {
    }
}

class E() : B
    (1);

class C
{
    void M(Func<int, int> f, Func<int, int> g)
    {
        Foo
        (4);

        var l = new List<int>
        (4);
        List<int> k = new

        (4);
        var r = (f ?? g)
        (1);
    }

    void Foo(int x)
    {
    }
}
```

```csharp
using System;
using System.Collections.Generic;

class B
{
    public B(int x)
    {
    }
}

class D : B
{
    public D()
        : base(1)
    {
    }
}

class E() : B(1);

class C
{
    void M(Func<int, int> f, Func<int, int> g)
    {
        Foo(4);

        var l = new List<int>(4);
        List<int> k = new(4);
        var r = (f ?? g)(1);
    }

    void Foo(int x)
    {
    }
}
```

Example — the argument list of a `with(…)` element and of an element joins its name; the collection expressions and the complex element initializer were written over several lines and keep one element per line:

```csharp
using System.Collections.Generic;

class C
{
    void M()
    {
        HashSet<int> h = [with
        (4), 1, 2];
        int[] a = [Bar
        (4)];
        var e = new Dictionary<int, int> { { Bar
            (1), 2 } };
    }

    int Bar(int x)
    {
        return x;
    }
}
```

```csharp
using System.Collections.Generic;

class C
{
    void M()
    {
        HashSet<int> h = [
                             with(4),
                             1,
                             2
                         ];
        int[] a = [
                      Bar(4)
                  ];
        var e = new Dictionary<int, int>
                {
                    {
                        Bar(1),
                        2
                    }
                };
    }

    int Bar(int x)
    {
        return x;
    }
}
```

Example — an argument list and a conditional expression that spanned several lines only because of the join are judged as the line they become:

```csharp
class C
{
    int M(bool a)
    {
        Baz(Bar
            (1), 2);

        var x = a ? Bar
        (1) : 2;

        return x;
    }

    void Baz(int x, int y)
    {
    }

    int Bar(int x)
    {
        return x;
    }
}
```

```csharp
class C
{
    int M(bool a)
    {
        Baz(Bar(1), 2);

        var x = a ? Bar(1) : 2;

        return x;
    }

    void Baz(int x, int y)
    {
    }

    int Bar(int x)
    {
        return x;
    }
}
```

Example — an argument list inside an interpolation hole, a query clause after the first, or an array initializer of a list element joins its name, in a chain or not, and the list is judged as the line it becomes:

```csharp
using System.Collections.Generic;
using System.Linq;

class C
{
    void M(object x, List<int> list)
    {
        Baz($"{x.ToString
        ()}", 2);
        Baz($"{Bar
        (1)}", 2);
        Baz(from v in list select Bar
        (v), 2);
        Baz(new[] { x.GetHashCode
        () }, 2);
    }

    void Baz(object a, int b)
    {
    }

    int Bar(int x)
    {
        return x;
    }
}
```

```csharp
using System.Collections.Generic;
using System.Linq;

class C
{
    void M(object x, List<int> list)
    {
        Baz($"{x.ToString()}", 2);
        Baz($"{Bar(1)}", 2);
        Baz(from v in list select Bar(v), 2);
        Baz(new[] { x.GetHashCode() }, 2);
    }

    void Baz(object a, int b)
    {
    }

    int Bar(int x)
    {
        return x;
    }
}
```

Example — a comment, a directive, or disabled text in the gap refuses the input as a whole (FMT-UNSUP-014):

```csharp
class C
{
    void M()
    {
        Foo // four
        (4);
        Foo /* four */
        (4);
        Foo
#if X
        (4);
#else
        (5);
#endif
    }

    void Foo(int x)
    {
    }
}
```

is returned unchanged and reported as unsupported.

### FMT-BREAK-040 — An element access or a null-forgiving operator starts on the line of the code in front of it

When a line break separates an element access, a conditional element access, or a null-forgiving operator — the attached parts of SHARED-CHAIN-001 other than an argument list — from the token in front of it, the part MUST be joined onto that token's line. The part's first token is the one joined: the `[` of an element access's bracketed argument list (`arr⏎[0]`, `this⏎[0]`, `d["k"]⏎[0]`, `(arr)⏎[0]`), the `?` of a conditional element access (`n⏎?[0]`), and the `!` of a null-forgiving operator (`Get()⏎!`, `s⏎! + ""`). A line break between the `?` and the `[` of a conditional element access MUST be removed as well (`n?⏎[0]` becomes `n?[0]`). This holds wherever the part stands: in a chain with links, in an expression without any link, inside a list element, inside an interpolation hole, inside a lambda, as an assignment's target, and everywhere else. This requirement is the one owner of these gaps; FMT-BREAK-020 and FMT-BREAK-015 leave them to it. Blank lines in the gap go with the join (FMT-BREAK-030).

A null-forgiving operator is an attached part only when no `.` or `?.` follows it (SHARED-CHAIN-001). In `s⏎!;` and `s⏎! + ""` the `!` is one and joins `s`; in `s⏎!.Length` the `!` is the first token of the link operator `!.`, whose gap is laid out as the gap in front of a link (FMT-BREAK-019), not by this requirement. The `[` this requirement joins is the opening bracket of the bracketed argument list of an `ElementAccessExpressionSyntax`, or of the `ElementBindingExpressionSyntax` that a conditional element access holds behind its `?`. Every other `[` is not the first token of an attached part, and this requirement does not decide the gap in front of it. For example: the bracketed argument list of an implicit element access (`ImplicitElementAccessSyntax`, the `["a"]` of `["a"] = 1` in an object or dictionary initializer), which has no element in front of it, only the initializer's `{` or a `,`; the `[` of an implicit array creation (`new[] { 1 }`) and of an implicit `stackalloc` array creation (`stackalloc [] { 1 }`), which belongs to the `new` or `stackalloc` expression itself; an array rank specifier (`new int[3]`, `stackalloc int[3]`, `int[]`); a collection expression; a list pattern; an attribute list; an indexer's parameter list; and a function pointer's calling-convention list.

Where a construct around the part is laid out by whether it spans several lines, the line break the join removes counts as FMT-BREAK-039 states for the line break in front of an argument list: a construct FMT-BREAK-022 decides still counts it, and a list FMT-BREAK-012 lays out and a conditional expression (FMT-BREAK-018) are judged as the line they become, also where the part stands inside an interpolation hole, a query clause after the first, or an array or `stackalloc` initializer of a list element.

Why: An element access and a null-forgiving operator belong to the expression in front of them, as an argument list does (FMT-BREAK-039): `[0]` on a line of its own reads as a collection expression or an attribute list, and `!` as a negation of what follows, and no column the indentation phase can give the line shows that it belongs to the code above. The phase already joined these parts in a chain with at least one link, and a conditional element access everywhere; this requirement makes the join everywhere and states it once. The removed line break counts for a surrounding construct as FMT-BREAK-039's does, so an attached part split from its element lays out alike whichever part it is. RH5115 reports such an element access or conditional element access, and RH5116 such a null-forgiving operator. Neither the join in front of the part's first token nor the removal of a line break between a conditional element access's `?` and `[` ever meets a comment, a directive, or disabled text: the formatter refuses an input that holds one in either gap (FMT-UNSUP-014), so FMT-BREAK-001 never has to refuse it there.

Example:

```csharp
#nullable enable

class C
{
    void M(int[] arr, int[]? n, string? s)
    {
        var a = arr
        [0];
        var b = n
        ?[0];
        var c = Get()
        !;
        var d = s

        ! + "";
        arr
        [0] = 1;
    }

    object? Get()
    {
        return null;
    }
}
```

```csharp
#nullable enable

class C
{
    void M(int[] arr, int[]? n, string? s)
    {
        var a = arr[0];
        var b = n?[0];
        var c = Get()!;
        var d = s! + "";
        arr[0] = 1;
    }

    object? Get()
    {
        return null;
    }
}
```

Example — a list and a conditional expression that spanned several lines only because of the join are judged as the line they become; a collection expression written over several lines keeps one element per line:

```csharp
#nullable enable

class C
{
    int M(bool a, int[] arr, int[]? n, string? s)
    {
        Baz(s
            !, 2);
        Baz(n
            ?[0], 2);
        Baz($"{arr
        [0]}", 2);

        var x = a ? arr
        [0] : 2;
        int[] e = [arr
        [0], 2];

        return x;
    }

    void Baz(object? x, int y)
    {
    }
}
```

```csharp
#nullable enable

class C
{
    int M(bool a, int[] arr, int[]? n, string? s)
    {
        Baz(s!, 2);
        Baz(n?[0], 2);
        Baz($"{arr[0]}", 2);

        var x = a ? arr[0] : 2;
        int[] e = [
                      arr[0],
                      2
                  ];

        return x;
    }

    void Baz(object? x, int y)
    {
    }
}
```

Example — a `[` that is not the first token of an attached part is not joined: the implicit element accesses of a dictionary initializer stay on their own lines, and the `[` of `new[]`, `new int[3]`, and `stackalloc []` stays where it is written. The input below is kept as written:

```csharp
using System;
using System.Collections.Generic;

class C
{
    void M(int[] arr)
    {
        var d = new Dictionary<string, int>
                {
                    ["a"] = 1,
                    ["b"] = arr[0]
                };
        var e = new[] { 1, 2 };
        var f = new int[3];
        Span<int> g = stackalloc [] { 1, 2 };
        Span<int> h = stackalloc int[3];
    }
}
```

# Formatter — Line breaks

| | |
|---|---|
| ID prefix | FMT-BREAK |
| Status | Active |
| Code owners | `LineBreakPhase`, `LineBreakBlockRewriter`, `LineBreakInitializerRewriter`, `LineBreakContainedBlockRewriter`, `LineBreakAssignmentRewriter`, `DeclarationSemicolonLineBreakRewriter`, `ListElementInteriorJoinRewriter`, `ListElementInteriorUtilities`, `LineBreakListRewriter`, `PropertyLayoutLineBreakRewriter`, `GenericConstraintLineBreakRewriter`, `DeclarationBraceLineBreakRewriter`, `MemberLineStartRewriter`, `AttributeTargetFormattingRewriter`, `AttributeTargetFormattingShared`, `BinaryOperatorLineBreakRewriter`, `ChainLineBreakRewriter`, `TernaryLineBreakRewriter`, `SwitchCaseWhenLineBreakRewriter`, `BracePlacer`, `TokenGapNormalizer`, `AccessorListLayout`, `LineBreakDetection`, `LineBreakTriviaUtilities` |
| Uses | FMT-PIPE-002, FMT-STRUCT-009, FMT-STRUCT-014, SHARED-CHAIN-001, SHARED-CHAIN-002, SHARED-CHAIN-003, SHARED-CHAIN-004, SHARED-CHAIN-005, SHARED-CHAIN-006, SHARED-COMMENT-012 |

This spec covers the sixth phase of the pipeline, which decides where lines break: brace placement, one statement and one member per line, the layout of argument, parameter, attribute, and type-argument lists, initializers and patterns, assignments, operators, conditionals, chains, generic constraints, attribute lists, accessor lists, and `case … when` guards. The phase only adds and removes line breaks; the columns the resulting lines start in are decided later by FMT-INDENT, and the spaces between tokens on a line by FMT-SPACE.

## Non-goals

- Indentation and alignment of the lines this phase produces — FMT-INDENT. Examples here show the final output, so they include its alignment.
- How many blank lines separate statements, members, comments, and directives — FMT-BLANK. Where this phase moves a token onto a new line it inserts no blank line.
- Whether an `if`/`else` body gets braces, whether a member is expression-bodied, and whether an empty type becomes a semicolon declaration — FMT-STRUCT. Braces of switch sections — FMT-SWITCH.
- Starting a wrap because a line is long. The phase has no line-length limit: it only normalizes wraps the author made, apart from the shapes it always lays out over several lines (braces, object initializers, nested conditionals).
- Unbraced embedded statements (`while (a) Foo();`), enum member lists, tuple expressions, and the arms of a switch expression keep the line breaks the author wrote. One switch-expression arm per line is a decided behavior change, tracked in thoenissen/Reihitsu#956.
- The position of a comma or of the closing parenthesis in an argument list (as opposed to a parameter list, FMT-BREAK-011). Laying out argument lists like parameter lists is a decided behavior change, tracked in thoenissen/Reihitsu#956.

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

The opening and the closing brace of every namespace, type, member body, accessor body, statement block, lambda or anonymous-method block, local function, and switch statement MUST each start their own line, and the first token after an opening brace MUST start a new line. An empty body MUST therefore span two lines.

Why: The repository uses Allman style throughout: a brace on its own line makes the extent of every scope visible at the left margin. An empty `{ }` is expanded too, so a body looks the same before and after its first statement is added.

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

A comment that trails the closing parenthesis of an `if` condition, with nothing but whitespace and line breaks behind it, MUST move into the leading trivia of the `if` keyword, onto the line above the statement. The comment of an `else if` condition MUST stay where it is.

Why: With the opening brace on the next line, a comment behind the condition would sit on a line of its own between condition and brace or be glued to the brace. Above the `if` it describes the whole statement. For `else if`, the leading trivia of `if` lies between `else` and `if`, so moving the comment there would split the two keywords.

Example:

```csharp
class C
{
    void M(bool a, bool b)
    {
        if (a) // why
        {
            Foo();
        }
        else if (b) // c
        {
            Bar();
        }
    }
}
```

```csharp
class C
{
    void M(bool a, bool b)
    {
        // why
        if (a)
        {
            Foo();
        }
        else if (b) // c
        {
            Bar();
        }
    }
}
```

### FMT-BREAK-006 — One statement per line

Every statement in a block or a switch section MUST start its own line. A comment that trailed the previous statement MUST stay on that statement's line. An empty statement (`;`) and the statement next to it MUST be left as written.

Why: One statement per line keeps every statement visible at the left margin and gives each its own line in diffs and stack traces. Empty statements are left alone because splitting them off only moves a stray `;` around; they are rare and usually deliberate.

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

### FMT-BREAK-007 — One member per line

Every member of a compilation unit, a namespace, a file-scoped namespace, or a type body MUST start its own line. A comment that trailed the previous member MUST stay on that member's line.

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

The opening parenthesis of a parameter list MUST stay on the declaration line, a comma between parameters MUST stay at the end of the previous parameter's line, and the closing parenthesis MUST stay on the line of the last parameter. This holds for every parameter list — methods, constructors, operators, delegates, local functions, records, primary constructors, anonymous methods, and parenthesized lambdas.

Why: A parenthesis or comma at the start of a line separates the declaration from its parameters or a parameter from its separator, which is easy to misread. RH5105, RH5106, and RH5107 report the same shapes.

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

### FMT-BREAK-012 — A list is on one line or one element per line

An argument list, a parameter list, and an attribute argument list MUST either stay on one line or put every element on its own line. A list that is wrapped anywhere — after its opening parenthesis or between two elements — MUST become one element per line, with the first element on the line of the opening parenthesis. A list written on one line MUST be left on one line.

Why: A partly wrapped list hides elements at the end of a line. Keeping the first element on the opening line lets the remaining elements align under it (FMT-INDENT). RH5101, RH5102, and RH5109 report the same shapes.

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
        Foo(a, b, c);
    }
}
```

### FMT-BREAK-013 — A multi-line element splits its list

When an element of an argument, parameter, or attribute argument list spans several lines — a nested wrapped list, a lambda with a block body, an initializer — the list MUST put every element on its own line, as if it were wrapped.

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

A wrapped bracketed argument list (`m[1, 2]`), type argument list, type parameter list, and function-pointer parameter list MUST be joined onto one line, unless an element itself spans several lines. The join only rewrites the gaps between the delimiters and the elements, so a comment behind the closing delimiter does not block it.

Why: These lists are short by nature and read as part of a name or an index; wrapping them breaks up a type or an access expression without making it clearer. RH5307 reports a wrapped indexer argument list with the same guard.

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

### FMT-BREAK-015 — Wraps inside a list element are joined

A line inside an element of an argument, parameter, attribute argument, tuple, or type-argument list that starts at a token other than the element's first token MUST be joined onto the previous line. A gap whose layout another requirement owns MUST NOT be joined by this rule: list boundaries, the opening parenthesis of a parameter list other than a lambda's, operator wraps of binary, conditional, member-access, postfix, `is`-pattern, and assignment expressions (FMT-BREAK-016 to FMT-BREAK-020), multi-line literals, attribute placement, and every line inside a nested braced, bracketed, or query construct.

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

The `=` of an assignment, a variable declarator, or a property initializer MUST stay on the line of its target, and the value MUST start on the line of the `=`. A value that spans several lines starts on the `=` line and keeps its own layout; a collection initializer or collection expression assigned to a target keeps its opening delimiter on the `=` line.

Why: Splitting the target from its value makes the reader search for what is assigned to what. RH5111 reports the same shapes.

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

### FMT-BREAK-017 — Binary operators start the continuation line

When a binary expression is wrapped at its operator, the operator MUST start the continuation line instead of ending the previous one. A pattern combinator (`and`, `or`) MUST start its own line when an operand next to it is a property, list, or parenthesized pattern that spans several lines; otherwise it keeps the author's layout. The formatter never wraps a binary expression the author wrote on one line.

Why: An operator at the start of a line shows at a glance how the line continues the expression, and lines up the operands of a long condition. RH5302 reports the same shape for logical expressions.

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

Why: A partly wrapped conditional mixes the two shapes and hides a branch at the end of a line. A nested conditional on one line is hard to parse, so it is always spread out. RH5308 reports the same shapes.

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

The formatter MUST lay out member-access chains as the shared chain policy specifies: it never starts a wrap (SHARED-CHAIN-002), joins the first link to the root's line (SHARED-CHAIN-003) unless a comment, directive, or disabled text is in the way (SHARED-CHAIN-004), starts every later call of a wrapped chain on its own line (SHARED-CHAIN-005), and moves a line break out of a link operator (SHARED-CHAIN-006).

A link whose operator stands behind the closing `*/` of a comment that spans several lines does not start its line (SHARED-COMMENT-012). In a wrapped chain, the formatter MUST break the line in front of its operator as SHARED-CHAIN-005 asks, whatever column the comment's last line starts in — for a later link, and, by the exception SHARED-CHAIN-005 states, for a prefix link and for the first invoked link too.

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

### FMT-BREAK-020 — Split member names and attached parts are rejoined

A member name separated from its dot by a line break MUST be joined back to the dot, and such a split MUST NOT count as a wrap of the chain. In a chain with at least one link, an attached part (SHARED-CHAIN-001) — an argument list, an element access, a conditional element access — MUST be joined onto the line of the element in front of it.

Why: `items.⏎Where` and `Where⏎(x)` split a single step of the chain rather than separating two steps. Treating the trailing dot as a wrap would turn a stray line break into a fully wrapped chain.

Example:

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

The initializer of an object creation (`new Foo { … }`, `new() { … }`), a collection initializer, an anonymous object creation, and the initializer of a `with` expression MUST put the opening brace, every element, and the closing brace on their own lines, also when the author wrote it on one line. A collection initializer assigned to a member inside an object initializer (`Items = { … }`) keeps its opening brace on the `=` line. Array initializers (`new int[] { 1, 2 }`, `int[] f = { 1, 2 }`) keep the author's layout.

Why: An initializer sets several independent values; one per line makes each visible and gives each its own diff line. A `with` initializer sets values the same way. Array initializers are often long lists of literal data, for which one element per line would explode the file. RH5301 and RH5303 report the same shapes, apart from the `with` initializer, which no rule reports.

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

A collection expression, a list pattern, a property pattern clause, a parenthesized pattern, and a complex element initializer (`{ 1, 2 }` inside a dictionary initializer) written on one line MUST stay on one line. When one of them spans several lines, every element MUST start its own line, the first element MUST start the line after the opening delimiter, and the closing delimiter MUST start its own line. The opening brace of a wrapped property pattern MUST start its own line, except in a pattern introduced directly by a `case` label, where it stays on the label's line.

Why: These constructs are expressions inside an expression; on one line they read as a value. Once wrapped, the same one-element-per-line rule as for initializers applies. The `case` exception keeps the pattern from being mistaken for the section body. RH5305 and RH5306 report the same shapes.

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

### FMT-BREAK-023 — Each generic constraint starts its own line

Every `where` clause of a type, method, local function, or delegate MUST start its own line, also on a member without a body.

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

### FMT-BREAK-024 — Simple auto-property accessor lists are on one line

The accessor list of a property or indexer whose accessors are all auto accessors without bodies, comments, or directives MUST be on one line together with the declaration (`int P { get; set; }`), also when an initializer follows. An accessor list with an attribute on an accessor MUST stay as written when the declaration is on one line and MUST otherwise be laid out over several lines.

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

The `=>` of an expression-bodied property or indexer and the start of its expression MUST be on the line of the declaration.

Why: An expression-bodied property is a one-line declaration by intent (FMT-STRUCT-014 creates them from get-only properties); a wrapped arrow makes it look like a statement. RH5401 reports the same shape.

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

Each attribute list of a type, member, accessor, or other declaration MUST start its own line, and a list with several attributes MUST be split into one list per attribute. Attribute lists of parameters and type parameters MUST stay on the parameter's line and keep their attributes in one list. Lists are only split or merged when no comment or directive is in the way, and lists of different targets are never merged.

Why: One attribute per line keeps each attribute visible and diffable on declarations, while parameter attributes stay inline so that a parameter list keeps one parameter per line. The RH55xx attribute placement and list-shape rules report the same shapes.

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

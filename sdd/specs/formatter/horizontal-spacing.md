# Formatter — Horizontal spacing

| | |
|---|---|
| ID prefix | FMT-SPACE |
| Status | Active |
| Code owners | `HorizontalSpacingPhase`, `HorizontalSpacingRewriter`, `SpacingPolicy`, `ISpacingRule`, `AttributeListCloseBracketSpacingRule`, `NoSpaceSpacingRule`, `CommaSpacingRule`, `SingleSpaceSpacingRule`, `OperatorSpacingRule`, `ForLoopSemicolonSpacingRule`, `KeywordSpacingRule`, `TrailingWhitespaceWriter`, `CommaSpacingUtilities`, `UnaryOperatorSpacingUtilities` |
| Uses | FMT-BREAK-015, FMT-BREAK-018, FMT-BREAK-021, FMT-BREAK-024, FMT-BREAK-031, FMT-PIPE-001, FMT-PIPE-002, FMT-PIPE-003, FMT-PIPE-004, FMT-PIPE-014, SHARED-BLANK-008 |

This spec covers `HorizontalSpacingPhase`: the whitespace between two adjacent tokens on the same line, as decided by the ordered spacing rules of `SpacingPolicy`, how that decision is applied when a comment sits in the gap, and the gaps the phase leaves alone.

## Non-goals

- Whitespace in a gap that contains a line break: indentation and continuation columns belong to [indentation.md](indentation.md) (FMT-INDENT), and whether a line break exists at all to [line-breaks.md](line-breaks.md) (FMT-BREAK).
- Whitespace at the end of a line and the final newline — [cleanup.md](cleanup.md) (FMT-CLEAN); tabs in whitespace, which are replaced before this phase runs — [tab-replacement.md](tab-replacement.md) (FMT-TAB).
- The text of literals, interpolated-string text, comments, and disabled text, which the phase never edits (FMT-PIPE-002).
- Token pairs no rule covers, such as the `case` colon, the space between a method name and its argument list, the `[` of an element access, the `::` alias qualifier, the parenthesis behind `typeof` or `default`, the space after `when`, or the inside of a one-line initializer's braces. The phase only collapses their whitespace (FMT-SPACE-002). Spacing rules for the space before an argument list or an element access, `::`, and initializer braces are a decided behavior change, tracked in thoenissen/Reihitsu#1440.

## Requirements

### FMT-SPACE-001 — Only gaps on one line are changed

The formatter MUST change only the whitespace between two tokens that are on the same line. When the left token's trailing trivia or the right token's leading trivia contains a line break — an end-of-line trivia, or the line break that ends a single-line documentation comment (`///`) in front of the right token, which then starts its line (SHARED-BLANK-008) — the phase MUST leave the whole gap unchanged, including the whitespace between a token and a comment that ends the line. A line break inside the text of a multi-line comment is not a line break of the gap.

Why: A gap across a line break is indentation, which a later phase computes (FMT-PIPE-001). The space in front of an end-of-line comment is the author's choice of comment position; rewriting it would fight hand-aligned trailing comments. A `///` comment ends with its own line break, so the whitespace behind it is the indentation of the next line, even when no space is allowed in front of the token there.

Example:

```csharp
class C
{
    void M(int a)
    {
        M(a);    // trailing
        M(a);// tight
    }
}
```

is left unchanged, and so is the gap broken by a `///` comment written behind code:

```csharp
class C
{
    public int M()
    {
        return 1    /// stray
        ;
    }
}
```

### FMT-SPACE-002 — Whitespace no rule covers is only collapsed

When no spacing rule applies to a token pair, the formatter MUST replace each run of whitespace between the two tokens by a single space and MUST NOT insert or remove a gap. A run of tabs, or of tabs and spaces, counts as a run and becomes one space. This holds for a gap the line-break phase joined as for one the author wrote on one line: the phase decides a gap by its two tokens, never by whether the gap held a line break before, so a joined pair that no rule covers keeps the separator the join wrote (FMT-BREAK-031). When several rules apply to one pair, the first rule in this order MUST decide: attribute close bracket (FMT-SPACE-003), no-space (FMT-SPACE-004 to FMT-SPACE-007), comma (FMT-SPACE-008), single-space (FMT-SPACE-009), operator (FMT-SPACE-010), for-loop semicolon (FMT-SPACE-011), attribute-target colon (FMT-SPACE-016), keyword (FMT-SPACE-012), arrow (FMT-SPACE-017), conditional operator (FMT-SPACE-018), name colon (FMT-SPACE-019), `when` (FMT-SPACE-020), declared name (FMT-SPACE-021), accessor list (FMT-SPACE-022), relational pattern (FMT-SPACE-023), modifier and prefix keyword (FMT-SPACE-024), `with` (FMT-SPACE-025), lambda return type (FMT-SPACE-026), function-pointer calling convention (FMT-SPACE-027).

Why: The phase only enforces a spacing it has a rule for. Where it has none — the space after a cast, for instance — both forms are acceptable and the author's choice is kept; only the clearly accidental double space or tab goes. A joined pair is decided like the same pair written on one line, so the output does not depend on where the author broke the line; a rule that applied to joined gaps only would keep that difference. A tab is collapsed even when it is alone, because a tab the phase kept would later be expanded to spaces and collapsed on the next run, and the output would never become stable (FMT-PIPE-004).

Example:

```csharp
class C
{
    void M(object o)
    {
        var s = (string)o;
        var t = (string) o;
        var u = (string)  o;
    }
}
```

```csharp
class C
{
    void M(object o)
    {
        var s = (string)o;
        var t = (string) o;
        var u = (string) o;
    }
}
```

### FMT-SPACE-003 — One space after an attribute list

The formatter MUST put exactly one space after the closing bracket of an attribute list when the next token is on the same line, whatever that token is.

Why: An attribute written on the line of its target must stay visibly separate from it; `[In]int` reads as one word.

Example:

```csharp
using System.Runtime.InteropServices;

class C
{
    void M([In]int a, [In]  int b)
    {
    }
}
```

```csharp
using System.Runtime.InteropServices;

class C
{
    void M([In] int a, [In] int b)
    {
    }
}
```

### FMT-SPACE-004 — No space inside parentheses and brackets, before commas and semicolons

The formatter MUST remove the whitespace after `(` and `[`, and before `)`, `]`, `,`, and `;`.

Why: These delimiters belong to the token next to them. This rule takes precedence over the comma, operator, for-loop, and keyword rules, so `for (;;)` keeps its semicolons together and `return;` and `x++;` stay closed.

Example:

```csharp
class C
{
    int M(int[] values, int a)
    {
        M( values , a );

        return values[ a ] ;
    }
}
```

```csharp
class C
{
    int M(int[] values, int a)
    {
        M(values, a);

        return values[a];
    }
}
```

### FMT-SPACE-005 — Generic angle brackets and nullable question marks are tight

The formatter MUST remove the whitespace on both sides of the `<` and before the `>` of a type argument or type parameter list, and before the `?` of a nullable type. The whitespace after the closing `>` is not decided by this rule.

Why: `List<int>` and `int?` are type names; a space inside them makes them read as comparison or conditional operators.

Example:

```csharp
using System.Collections.Generic;

class C
{
    void M(int? a)
    {
        List < int > list = null;
        int ? b = a;
    }
}
```

```csharp
using System.Collections.Generic;

class C
{
    void M(int? a)
    {
        List<int> list = null;
        int? b = a;
    }
}
```

### FMT-SPACE-006 — Member-access symbols are tight

The formatter MUST remove the whitespace on both sides of the dot of a member access, member binding, or qualified name, of the `?` that opens a conditional access, and of the `->` of a pointer member access.

Why: A member access is one name path. Tightening only one side of `?.` or `->` would leave `x ?.Foo` or `p ->V`, which no analyzer rule accepts. A `?` of a conditional expression or of a nullable type is not a member-access symbol.

Example:

```csharp
class C
{
    unsafe void M(C c, S* p)
    {
        var a = c . ToString();
        var b = c ?. ToString();
        var e = p -> V;
    }
}

struct S
{
    public int V;
}
```

```csharp
class C
{
    unsafe void M(C c, S* p)
    {
        var a = c.ToString();
        var b = c?.ToString();
        var e = p->V;
    }
}

struct S
{
    public int V;
}
```

### FMT-SPACE-007 — Unary operators are tight unless that would change the operator

The formatter MUST remove the whitespace between a prefix unary operator and its operand and between an operand and a postfix unary operator (including the null-forgiving `!`). It MUST NOT remove the whitespace after a prefix `-`, `+`, or `&` when the operand starts with the same symbol or with `--`, `++`, or `&&`; that whitespace is only collapsed (FMT-SPACE-002).

Why: A unary operator binds to its operand. Removing the space in `- -a` or `+ +a` would make the lexer read `--a` or `++a`, which changes the program (FMT-PIPE-003); `& &x` would become `&&x` and no longer parse.

Example:

```csharp
class C
{
    void M(int a, bool b, object o)
    {
        var x = - a;
        var y = ! b;
        var z = o !;
        var w = -  -a;
        var v = + +a;
    }
}
```

```csharp
class C
{
    void M(int a, bool b, object o)
    {
        var x = -a;
        var y = !b;
        var z = o!;
        var w = - -a;
        var v = + +a;
    }
}
```

### FMT-SPACE-008 — One space after a comma, except in compact comma forms

The formatter MUST put exactly one space after a comma. It MUST put no space after a comma of a rank-only array specifier (`int[,]`), of an unbound generic type (`Dictionary<,>`), or of an interpolation alignment clause (`{value,-10}`). FMT-SPACE-004 removes the space before every comma.

Why: A comma separates list elements and the space makes the boundary visible. In the compact forms the comma is part of a single type name or of a format specification, and a space there looks like a list where there is none. Analyzer and formatter share this decision (`CommaSpacingUtilities`), so the formatter never produces a comma the comma-spacing rule reports.

Example:

```csharp
using System.Collections.Generic;

class C
{
    void M(int a)
    {
        var t = typeof(Dictionary< , >);
        int[ , ] r = new int[2,3];
        var s = $"{a , -10}|{a,  5}|{a,-3:N2}";
    }
}
```

```csharp
using System.Collections.Generic;

class C
{
    void M(int a)
    {
        var t = typeof(Dictionary<,>);
        int[,] r = new int[2, 3];
        var s = $"{a,-10}|{a,5}|{a,-3:N2}";
    }
}
```

### FMT-SPACE-009 — One space before braces and around declaration colons

The formatter MUST put exactly one space before an opening or closing brace that does not belong to an interpolation, and on both sides of the colon of a base list or of a constructor initializer. Braces of an interpolation MUST NOT get this space.

Why: A brace or a declaration colon separates two parts of a construct; gluing them hides the boundary. Interpolation braces are part of the string's text layout and keep the author's spacing (FMT-SPACE-002). The space after an opening brace is not decided by this rule.

Example:

```csharp
class B
{
    public B(int a)
    {
    }
}

class C:B
{
    public C(int a)
        :base(a)
    {
    }

    string M(int a)
    {
        var values = new int[]{ a };

        return $"{a}";
    }
}
```

```csharp
class B
{
    public B(int a)
    {
    }
}

class C : B
{
    public C(int a)
        : base(a)
    {
    }

    string M(int a)
    {
        var values = new int[] { a };

        return $"{a}";
    }
}
```

### FMT-SPACE-010 — One space around binary and assignment operators

The formatter MUST put exactly one space on both sides of a binary operator, an assignment or compound assignment operator, the `=` of a variable, parameter, or property initializer, and the `=` of a name-equals clause (using alias, attribute argument, anonymous-object member).

Why: Operands run together with their operator (`a+b`) are hard to read, and extra spaces used for alignment break on the next edit. The rule decides the gap on both sides because either token of the pair may be the operator.

Example:

```csharp
using X=System.String;

class A : System.Attribute
{
    public int N { get; set; }
}

class C
{
    [A(N=1)]
    void M(int a)
    {
        var x=a+1;
        x+=a;

        var b=a==1&&a<2;
        var q=a??1;
    }
}
```

```csharp
using X = System.String;

class A : System.Attribute
{
    public int N { get; set; }
}

class C
{
    [A(N = 1)]
    void M(int a)
    {
        var x = a + 1;
        x += a;

        var b = a == 1 && a < 2;
        var q = a ?? 1;
    }
}
```

### FMT-SPACE-011 — One space after a for-loop semicolon

The formatter MUST put exactly one space after each semicolon of a `for` header, unless the next token is `)` or another semicolon (FMT-SPACE-004).

Why: The three clauses of a `for` header are separate statements and need the same separation as a comma-separated list. An empty clause stays tight, so `for (;;)` is the canonical endless loop.

Example:

```csharp
class C
{
    void M(int a)
    {
        for (int i = 0;i < a;i++)
        {
        }

        for (; ;)
        {
        }
    }
}
```

```csharp
class C
{
    void M(int a)
    {
        for (int i = 0; i < a; i++)
        {
        }

        for (;;)
        {
        }
    }
}
```

### FMT-SPACE-012 — One space after statement keywords, `new`, and `operator`

The formatter MUST put exactly one space after `if`, `for`, `foreach`, `while`, `switch`, `catch`, `fixed`, `using`, `lock`, `return`, `throw`, `new`, and `operator`, with these exceptions, where it MUST put no space: `return` or `throw` directly before `;`; `new` before the `(` of a target-typed `new()` or of a `new()` constraint; and `new` before the `[` of an implicitly typed array `new[]`. A keyword used as an attribute target, such as `return` in `[return: A]`, is spaced by FMT-SPACE-016, which decides before this rule.

Why: Without the space, `if(x)` and `return(x)` look like calls to methods named `if` and `return`, and `operator+` like an identifier. The exceptions are the forms where the keyword and the next token make up a single construct.

Example:

```csharp
using System;

class C
{
    int M(int a, object o)
    {
        if(a > 0)
        {
            return(a);
        }

        lock(o)
        {
            throw new  Exception();
        }
    }

    public static C operator+(C a, C b)
    {
        return a;
    }
}
```

```csharp
using System;

class C
{
    int M(int a, object o)
    {
        if (a > 0)
        {
            return (a);
        }

        lock (o)
        {
            throw new Exception();
        }
    }

    public static C operator +(C a, C b)
    {
        return a;
    }
}
```

The same holds for the `fixed` statement:

```csharp
class C
{
    unsafe void M(int[] items)
    {
        fixed(int* p = items)
        {
        }
    }
}
```

```csharp
class C
{
    unsafe void M(int[] items)
    {
        fixed (int* p = items)
        {
        }
    }
}
```

and for a fixed-size buffer, where the space follows a comment glued to the type (FMT-SPACE-013):

```csharp
unsafe struct S
{
    public fixed/* c */int _b[4];
}
```

```csharp
unsafe struct S
{
    public fixed/* c */ int _b[4];
}
```

The exceptions:

```csharp
class C<T>
    where T : new ( )
{
    [return : System.Obsolete]
    C<T> M()
    {
        C<T> c = new  ();
        var arr = new  [] { 1 };

        return c ;
    }

    void N()
    {
        try
        {
        }
        catch
        {
            throw ;
        }
    }
}
```

```csharp
class C<T>
    where T : new()
{
    [return: System.Obsolete]
    C<T> M()
    {
        C<T> c = new();
        var arr = new[] { 1 };

        return c;
    }

    void N()
    {
        try
        {
        }
        catch
        {
            throw;
        }
    }
}
```

### FMT-SPACE-013 — A comment the left token carries keeps its place

When the gap contains a comment in the left token's trailing trivia, the formatter MUST apply the rule's decision to the whitespace between the last such comment and the right token. Whitespace in front of the comment MUST only be collapsed to a single space; the phase MUST NOT insert or remove it.

Why: The rule answers for the distance to the next token, and the comment must survive (FMT-PIPE-002). Removing the space in front of a comment would glue it to code the author separated it from; inserting one would change a placement the rule says nothing about.

Example:

```csharp
class C
{
    void M(int a)
    {
        var x = a  /* one */  + 1;
        var y = a/* two */  + 1;

        M(a /* three */ );
        M( /* four */ a);
    }
}
```

```csharp
class C
{
    void M(int a)
    {
        var x = a /* one */ + 1;
        var y = a/* two */ + 1;

        M(a /* three */);
        M( /* four */a);
    }
}
```

### FMT-SPACE-014 — A comment the right token carries is not moved

When the right token carries a comment in its own leading trivia on the same line (a delimited documentation comment `/** */` written between two tokens), the formatter MUST NOT change the whitespace in front of that comment. When no space is allowed before the right token, the formatter MUST remove the whitespace between that comment and the token.

Why: The rules see only the token pair, not the comment. Applying a "no space before `;`" decision to the whitespace in front of the comment would glue the comment to the left token (`_a/** x */;`). The whitespace after the comment is the part the rule actually answers for; leaving it would keep a space the analyzer rules for semicolons, commas, and attribute brackets report.

Example:

```csharp
using System;

class C
{
    int _a /** x */;
    int _b /** y */ ;

    [Obsolete /** why */ ]
    void M()
    {
    }
}
```

```csharp
using System;

class C
{
    int _a /** x */;
    int _b /** y */;

    [Obsolete /** why */]
    void M()
    {
    }
}
```

### FMT-SPACE-015 — Indentation after a single-line documentation comment is kept

Status: Retired — the line break that ends a `///` comment is a line break of the gap (SHARED-BLANK-008), so FMT-SPACE-001 leaves the whitespace behind the comment unchanged, and the line behind it is placed by the indentation phase.

### FMT-SPACE-016 — Attribute-target colons: no space before, one space after

The formatter MUST put no space in front of the colon that ends an attribute target and exactly one space behind it, whatever the target: a keyword such as `return`, `event`, or `assembly`, or an identifier such as `field`, `param`, or `type`. Every other colon in an attribute is decided by its own rule, such as the colon of a named attribute argument (FMT-SPACE-019).

Why: The target names what the attribute applies to and reads as one label with its colon, like a named argument; the space behind it separates the label from the attribute. FMT-PIPE-014 asks for every attribute-target colon to be spaced alike, and a rule keyed by the keyword list of FMT-SPACE-012 reached `return` only.

Example:

```csharp
using System;

[assembly:CLSCompliant(true)]

class C
{
    [field :NonSerialized]
    int _f;

    [return:Obsolete]
    int M()
    {
        return 1;
    }

    [method  :  Obsolete]
    void N([param :Obsolete] int a, [Obsolete(message:"x")] int b)
    {
    }
}
```

```csharp
using System;

[assembly: CLSCompliant(true)]

class C
{
    [field: NonSerialized]
    int _f;

    [return: Obsolete]
    int M()
    {
        return 1;
    }

    [method: Obsolete]
    void N([param: Obsolete] int a, [Obsolete(message: "x")] int b)
    {
    }
}
```

A comment in the gap follows FMT-SPACE-013 and FMT-SPACE-014: `[return /** x */ :Obsolete]` gives `[return /** x */: Obsolete]`, and `[return:/* x */Obsolete]` gives `[return:/* x */ Obsolete]`.

### FMT-SPACE-017 — One space on each side of every arrow

The formatter MUST put exactly one space on each side of every `=>`: the arrow of a lambda, of an expression body of a member, accessor, or local function, and of a switch-expression arm.

Why: The arrow separates a head from a body, and `x=>x` hides that boundary. FMT-PIPE-014 asks for every arrow to be spaced alike, whatever construct it belongs to.

Example:

```csharp
using System;

class C
{
    int _p;

    int A
    {
        get =>_p;
        set=>_p = value;
    }

    int P =>1;

    void M(int x)
    {
        Func<int, int> f = y=>y;
        Func<int, int> g = (int y)  =>y;
        var s = x switch
                {
                    1=>2,
                    _ =>3
                };
    }
}
```

```csharp
using System;

class C
{
    int _p;

    int A
    {
        get => _p;
        set => _p = value;
    }

    int P => 1;

    void M(int x)
    {
        Func<int, int> f = y => y;
        Func<int, int> g = (int y) => y;
        var s = x switch
                {
                    1 => 2,
                    _ => 3
                };
    }
}
```

### FMT-SPACE-018 — One space on each side of the conditional operator

The formatter MUST put exactly one space on each side of the `?` and the `:` of a conditional expression. A `?` that belongs to a nullable type (FMT-SPACE-005) or opens a conditional access (FMT-SPACE-006), and the `??` operator (FMT-SPACE-010), are not a conditional operator.

Why: `b?1:2` runs condition and branches together; the spaces show where each branch starts. A conditional laid out over several lines puts `?` and `:` at the start of their lines (FMT-BREAK-018), and the branch behind each operator needs the same space as on one line.

Example:

```csharp
class C
{
    int M(bool b, int? n, C c)
    {
        var r = b?1:2;
        var s = c?.M(b, n) ?? 0;

        return b ? -1:n.Value;
    }
}
```

```csharp
class C
{
    int M(bool b, int? n, C c)
    {
        var r = b ? 1 : 2;
        var s = c?.M(b, n) ?? 0;

        return b ? -1 : n.Value;
    }
}
```

### FMT-SPACE-019 — Name colons: no space before, one space after

The formatter MUST put no space in front of the colon of a name colon or an expression colon and exactly one space behind it: a named argument, a named attribute argument, a named tuple element, and the name of a property or positional subpattern. The colon of a `case` or `default` label, of a conditional expression (FMT-SPACE-018), of a base list or constructor initializer (FMT-SPACE-009), of an attribute target (FMT-SPACE-016), and of an interpolation's format clause is not a name colon.

Why: The name labels the value behind it and belongs to it, so the colon stays with the name and the space separates the label from the value — the form `name: value` the line-break phase's join examples already show (FMT-BREAK-015).

Example:

```csharp
class C
{
    void M(bool b, int c, object o)
    {
        M(b:b, c :1, o: o);

        var t = (A:1, B :2);
        var p = o is string { Length:1 };
        var q = $"{c:N2}";
    }
}
```

```csharp
class C
{
    void M(bool b, int c, object o)
    {
        M(b: b, c: 1, o: o);

        var t = (A: 1, B: 2);
        var p = o is string { Length: 1 };
        var q = $"{c:N2}";
    }
}
```

A comment in the gap follows FMT-SPACE-013 and FMT-SPACE-014: `M(b /** x */ :b)` gives `M(b /** x */: b)`, and `M(b:/* x */b)` gives `M(b:/* x */ b)`.

### FMT-SPACE-020 — One space in front of `when`

The formatter MUST put exactly one space in front of the `when` of a `case` guard, of a switch-expression arm, and of a `catch` filter. The space behind `when` is not decided by this rule.

Why: Without the space, `case (1, 2)when b:` glues the guard to the pattern. FMT-PIPE-014 asks for every `when` guard to be spaced alike, the filter of a `catch` clause included. A pattern that ends in an identifier or keyword already needs the space to stay two tokens; one that ends in `)`, `}`, `]`, or a literal does not, and would keep a gap a join leaves empty (FMT-BREAK-031).

Example:

```csharp
class C
{
    int M(object o, bool b)
    {
        switch (o)
        {
            case (1, 2)when b:
                break;

            case int i  when(i > 0):
                break;
        }

        return o switch
               {
                   "x"when b => 1,
                   _ => 0
               };
    }

    void N()
    {
        try
        {
        }
        catch (System.Exception e)when (e != null)
        {
        }
    }
}
```

```csharp
class C
{
    int M(object o, bool b)
    {
        switch (o)
        {
            case (1, 2) when b:
                break;

            case int i when(i > 0):
                break;
        }

        return o switch
               {
                   "x" when b => 1,
                   _ => 0
               };
    }

    void N()
    {
        try
        {
        }
        catch (System.Exception e) when (e != null)
        {
        }
    }
}
```

### FMT-SPACE-021 — One space between a type or pattern and the name it declares

The formatter MUST put exactly one space between the type of a parameter, a tuple element, a variable declaration, a declaration expression, a declaration pattern, a `foreach` variable, a `catch` declaration, or the range variable of a `from` or `join` clause and the name or designation behind it, and between a recursive or list pattern and the designation behind it. The gaps inside the type, such as the one in front of `?` (FMT-SPACE-005) or in front of an array rank, are not decided by this rule.

Why: A type that ends in `?`, `]`, `>`, `*`, or `)` reads as two tokens with its name also without a space, so a join leaves that gap empty (FMT-BREAK-031) and `int?a` would stay. A type that ends in an identifier or keyword already needs the space, so for it the rule only collapses the gap. FMT-PIPE-014 asks for every declared name to be spaced alike, so the rule covers every declaration that names a variable behind a type or pattern, not only the ones a join reaches.

This requirement specifies the outcome only for the declarations it names and leaves every other type in front of a name unspecified, such as the return type of a method or the type of a property.

Example:

```csharp
class C
{
    void M(int?a, int[]b, (int, int)c, object o)
    {
        int?z = null;

        N(out int?w);

        var p = o is int[]d;
        var q = o is { }e && o is [1]f;

        foreach (int?g in new int?[0])
        {
        }

        try
        {
        }
        catch (System.Exception?h)
        {
        }

        var r = from int?i in new int?[0] select i;
    }
}
```

```csharp
class C
{
    void M(int? a, int[] b, (int, int) c, object o)
    {
        int? z = null;

        N(out int? w);

        var p = o is int[] d;
        var q = o is { } e && o is [1] f;

        foreach (int? g in new int?[0])
        {
        }

        try
        {
        }
        catch (System.Exception? h)
        {
        }

        var r = from int? i in new int?[0] select i;
    }
}
```

### FMT-SPACE-022 — One space inside a one-line accessor list

The formatter MUST put exactly one space behind the `{` of a property, indexer, or event accessor list and between two accessors of such a list when the next token is on the same line. FMT-SPACE-009 decides the space in front of the list's `}`. The braces of an initializer, an anonymous object, or a property pattern are not an accessor list.

Why: `{ get; set; }` is the one-line form of an auto-property (FMT-BREAK-024); `{get;set;}` hides where an accessor starts. The braces of an initializer are spaced by their own rules, which are a decided behavior change (thoenissen/Reihitsu#1440).

Example:

```csharp
using System;

class C
{
    int P {get;set;}

    int Q {[Obsolete] get;private set;}

    int[] R = new[] {1, 2};
}
```

```csharp
using System;

class C
{
    int P { get; set; }

    int Q { [Obsolete] get; private set; }

    int[] R = new[] {1, 2 };
}
```

### FMT-SPACE-023 — One space behind a relational pattern's operator

The formatter MUST put exactly one space between the operator of a relational pattern (`<`, `<=`, `>`, `>=`) and its operand. The space in front of the operator, and the operators of a binary expression (FMT-SPACE-010), are not decided by this rule.

Why: `x is >5` reads the operator and the bound as one token; the space makes the bound stand out as in the binary comparison `x > 5`.

Example:

```csharp
class C
{
    bool M(int x)
    {
        return x is >5 and <=  10;
    }
}
```

```csharp
class C
{
    bool M(int x)
    {
        return x is > 5 and <= 10;
    }
}
```

### FMT-SPACE-024 — One space behind a modifier or prefix keyword in front of its operand

The formatter MUST put exactly one space between each of these keywords and the token behind it:

- a lambda modifier: `static`, `async`;
- a modifier of an argument, a parameter, a type, or an expression: `ref`, `out`, `in`, `readonly`, `scoped`, `this`, `params`;
- `await` and `stackalloc`;
- `var` in front of a parenthesized designation (`var (x, y)`);
- the `not` of a pattern;
- the `in` of a `foreach` statement and of a `from` or `join` clause.

A keyword whose next token is its own parenthesis, such as `typeof`, `sizeof`, `default`, `checked`, `unchecked`, `nameof`, or `__arglist`, is not one of these keywords, and `new` is decided by FMT-SPACE-012. A modifier of a member or local declaration is not decided by this rule either: in `static (int, int) F() => default;` and `public(int, int) G;` the gap behind `static` and `public` is kept as written, while in `N(static(int v) => v)` and `static void E(this(int, int) t)` the lambda modifier and the parameter modifier get the space.

Each of these positions is decided as follows, wherever the token behind the keyword is a `(`, a `[`, or a `*`:

- A lambda modifier gets the space also in front of the lambda's return type: `static (int, int) () => (1, 2)`.
- A parameter modifier is also one of a function pointer's parameter: `delegate*<ref (int, int), void>`.
- The `ref` and the `readonly` of a lambda's return type get the space: `ref (int, int) () => ref t`.
- The `ref` of a ref expression gets the space wherever the expression stands, in an argument as in a statement: `ref (b ? ref x : ref y)`, `return ref (x);`, `return ref *p;`.
- `var` in front of the designation of a deconstruction gets the space as in a pattern: `var (u, w) = t;`.
- The modifiers of a local function are modifiers of a declaration, like a member's: `static(int, int) K()` keeps its gap, and so does a member's `readonly` in `public readonly(int, int) M()`.
- The `this` and `base` of a constructor initializer and the `this` of an indexer declaration are not modifiers and are not decided by this rule: `: this(a)`, `: base(a)`, and `int this[int i]` keep their gap.

Why: Each of these keywords modifies or introduces the operand behind it and is not called with it: `await(t)` and `async(x) => x` read like calls to methods named `await` and `async`, and `not"x"` or `out*p` glue the keyword to an operand of another kind. A keyword followed by an identifier or another keyword needs the space anyway; one followed by `(`, `[`, `*`, `@`, a literal, or an operator does not, so a join leaves that gap empty (FMT-BREAK-031) and only this rule keeps the space a joined pair had before.

Example:

```csharp
using System.Linq;
using System.Threading.Tasks;

static class C
{
    static void E(this(int, int) t, params(int, int)[] a)
    {
    }

    static async Task M(int a, int[] xs, object o, Task t)
    {
        N(async(x) => x, static(int v) => v);
        N(ref(a), in(a));
        await(t);

        var b = o is not"x" && o is not> 5;
        var c = o is var(u, w);

        foreach (var e in(xs))
        {
        }

        var q = from z in(xs) select z;
    }

    static unsafe void F(int* p)
    {
        N(out*p);

        var s = stackalloc[] { 1 };
    }
}
```

```csharp
using System.Linq;
using System.Threading.Tasks;

static class C
{
    static void E(this (int, int) t, params (int, int)[] a)
    {
    }

    static async Task M(int a, int[] xs, object o, Task t)
    {
        N(async (x) => x, static (int v) => v);
        N(ref (a), in (a));
        await (t);

        var b = o is not "x" && o is not > 5;
        var c = o is var (u, w);

        foreach (var e in (xs))
        {
        }

        var q = from z in (xs) select z;
    }

    static unsafe void F(int* p)
    {
        N(out *p);

        var s = stackalloc [] { 1 };
    }
}
```

`typeof(C)`, `default(int)`, and `nameof(a)` keep their gap.

Example — a lambda's return type, a function pointer's parameter, ref expressions, and a deconstruction get the space; a local function's modifier keeps its gap:

```csharp
unsafe class C
{
    static (int, int) s_t;

    static ref int M(bool b, ref int x, ref int y)
    {
        var(u, w) = s_t;
        N(ref(int, int) () => ref s_t, static(int, int) () => (1, 2));

        delegate*<ref(int, int), void> f = null;
        ref var z = ref(b ? ref x : ref y);

        return ref(x);
    }

    static void L()
    {
        static(int, int) K()
        {
            return default;
        }
    }
}
```

```csharp
unsafe class C
{
    static (int, int) s_t;

    static ref int M(bool b, ref int x, ref int y)
    {
        var (u, w) = s_t;
        N(ref (int, int) () => ref s_t, static (int, int) () => (1, 2));

        delegate*<ref (int, int), void> f = null;
        ref var z = ref (b ? ref x : ref y);

        return ref (x);
    }

    static void L()
    {
        static(int, int) K()
        {
            return default;
        }
    }
}
```

### FMT-SPACE-025 — One space on each side of `with`

The formatter MUST put exactly one space on each side of the `with` of a `with` expression.

Why: `with` is a binary operator written as a word; `p.Q()with { X = 1 }` glues it to its left operand, which an identifier in front of it would not allow. The initializer behind it starts its own line (FMT-BREAK-021); where it shares the line, the space in front of its `{` is decided by FMT-SPACE-009.

Example:

```csharp
class C
{
    void M(P p, P[] ps)
    {
        var a = p.Q()with { X = 1 };
        var b = ps[0]with { X = 2 };
        var c = p!   with { X = 3 };
    }
}
```

```csharp
class C
{
    void M(P p, P[] ps)
    {
        var a = p.Q() with
                      {
                          X = 1
                      };
        var b = ps[0] with
                      {
                          X = 2
                      };
        var c = p! with
                   {
                       X = 3
                   };
    }
}
```

### FMT-SPACE-026 — One space between an explicit lambda return type and its parameter list

The formatter MUST put exactly one space between the explicit return type of a lambda and the `(` of its parameter list. A lambda without a return type is decided by its modifier (FMT-SPACE-024) or starts its line.

Why: `int(x) => x` and `List<int>(x) => x` read like a conversion and a constructor call; the space separates the return type from the parameter list, as the space between a method's return type and its name does.

Example:

```csharp
using System.Collections.Generic;

class C
{
    void M()
    {
        N(int(x) => x, int[](x) => x, List<int>(x) => x, int?(x) => x, (int, int)(x) => x);
    }
}
```

```csharp
using System.Collections.Generic;

class C
{
    void M()
    {
        N(int (x) => x, int[] (x) => x, List<int> (x) => x, int? (x) => x, (int, int) (x) => x);
    }
}
```

### FMT-SPACE-027 — One space behind a function pointer's `*` in front of its calling convention

The formatter MUST put exactly one space between the `*` of a function-pointer type and the `managed` or `unmanaged` behind it. The gap in front of the `*` and the gap in front of the calling-convention list's `[` are not decided by this rule.

Why: `delegate*unmanaged[Cdecl]` runs the pointer and its calling convention together; `delegate* unmanaged[Cdecl]<int, void>` is the form the C# documentation shows.

Example:

```csharp
unsafe class C
{
    void M(delegate*unmanaged[Cdecl]<int, void> f, delegate*managed<int> g)
    {
    }
}
```

```csharp
unsafe class C
{
    void M(delegate* unmanaged[Cdecl]<int, void> f, delegate* managed<int> g)
    {
    }
}
```

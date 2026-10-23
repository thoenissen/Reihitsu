# Formatter — Horizontal spacing

| | |
|---|---|
| ID prefix | FMT-SPACE |
| Status | Active |
| Code owners | `HorizontalSpacingPhase`, `HorizontalSpacingRewriter`, `SpacingPolicy`, `ISpacingRule`, `AttributeListCloseBracketSpacingRule`, `NoSpaceSpacingRule`, `CommaSpacingRule`, `SingleSpaceSpacingRule`, `OperatorSpacingRule`, `ForLoopSemicolonSpacingRule`, `KeywordSpacingRule`, `TrailingWhitespaceWriter`, `CommaSpacingUtilities`, `UnaryOperatorSpacingUtilities` |
| Uses | FMT-PIPE-001, FMT-PIPE-002, FMT-PIPE-003, FMT-PIPE-004 |

This spec covers `HorizontalSpacingPhase`: the whitespace between two adjacent tokens on the same line, as decided by the ordered spacing rules of `SpacingPolicy`, how that decision is applied when a comment sits in the gap, and the gaps the phase leaves alone.

## Non-goals

- Whitespace in a gap that contains a line break: indentation and continuation columns belong to [indentation.md](indentation.md) (FMT-INDENT), and whether a line break exists at all to [line-breaks.md](line-breaks.md) (FMT-BREAK).
- Whitespace at the end of a line, tabs used as indentation, and the final newline — [cleanup.md](cleanup.md) (FMT-CLEAN).
- The text of literals, interpolated-string text, comments, and disabled text, which the phase never edits (FMT-PIPE-002).
- Token pairs no rule covers, such as the lambda arrow, the conditional operator, `case` and named-argument colons, or the space between a method name and its argument list. The phase only collapses their whitespace (FMT-SPACE-002). Spacing rules for the lambda arrow, the conditional operator, named-argument colons, and the space before an argument list are a decided behavior change, tracked in thoenissen/Reihitsu#958.

## Requirements

### FMT-SPACE-001 — Only gaps on one line are changed

The formatter MUST change only the whitespace between two tokens that are on the same line. When the left token's trailing trivia or the right token's leading trivia contains a line break, the phase MUST leave the whole gap unchanged, including the whitespace between a token and a comment that ends the line.

Why: A gap across a line break is indentation, which a later phase computes (FMT-PIPE-001). The space in front of an end-of-line comment is the author's choice of comment position; rewriting it would fight hand-aligned trailing comments.

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

is left unchanged.

### FMT-SPACE-002 — Whitespace no rule covers is only collapsed

When no spacing rule applies to a token pair, the formatter MUST replace each run of whitespace between the two tokens by a single space and MUST NOT insert or remove a gap. A run of tabs, or of tabs and spaces, counts as a run and becomes one space. When several rules apply to one pair, the first rule in this order MUST decide: attribute close bracket (FMT-SPACE-003), no-space (FMT-SPACE-004 to FMT-SPACE-007), comma (FMT-SPACE-008), single-space (FMT-SPACE-009), operator (FMT-SPACE-010), for-loop semicolon (FMT-SPACE-011), keyword (FMT-SPACE-012).

Why: The phase only enforces a spacing it has a rule for. Where it has none — the space after a cast, for instance — both forms are acceptable and the author's choice is kept; only the clearly accidental double space or tab goes. A tab is collapsed even when it is alone, because a tab the phase kept would later be expanded to spaces and collapsed on the next run, and the output would never become stable (FMT-PIPE-004).

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

The formatter MUST put exactly one space after `if`, `for`, `foreach`, `while`, `switch`, `catch`, `using`, `lock`, `return`, `throw`, `new`, and `operator`, with these exceptions, where it MUST put no space: `return` or `throw` directly before `;`; a keyword used as an attribute target before its `:` (`[return: A]`); `new` before the `(` of a target-typed `new()` or of a `new()` constraint; and `new` before the `[` of an implicitly typed array `new[]`.

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

When the right token's leading trivia ends with a single-line documentation comment (`///`) followed by whitespace, the formatter MUST NOT remove that whitespace, even when no space is allowed before the token.

Why: A `///` comment ends with its own line break, so the whitespace after it starts a new line and is the token's indentation, not a gap. No end-of-line trivia separates the two tokens in that case, so the phase cannot see the line break the usual way (FMT-SPACE-001), and no later phase would restore the indentation once it was removed.

Example:

```csharp
class C
{
    public int M()
    {
        return 1 /// stray
        ;
    }
}
```

is left unchanged.

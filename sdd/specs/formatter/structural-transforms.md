# Formatter — Structural transforms

| | |
|---|---|
| ID prefix | FMT-STRUCT |
| Status | Active |
| Code owners | `StructuralTransformPhase`, `ConfigurableStructuralTransforms`, `ControlFlowBraceTransform`, `ExpressionBodiedMethodTransform`, `ExpressionBodiedConstructorTransform`, `ExpressionBodiedOperatorTransform`, `ExpressionBodiedConversionTransform`, `ExpressionBodiedFinalizerTransform`, `ExpressionBodiedLocalFunctionTransform`, `ExpressionBodiedTransformUtilities`, `ExpressionBodyToBlockConverter`, `AccessorExpressionBodyTransform`, `GetOnlyMemberExpressionBodyTransform`, `EmptyTypeDeclarationSemicolonTransform`, `EnumTrailingCommaRemovalTransform`, `InitializerTrailingCommaRemovalTransform`, `TrailingCommaRemovalUtilities`, `FieldDeclarationSplitTransform` |
| Uses | FMT-PIPE-001, FMT-PIPE-002, FMT-PIPE-003, FMT-PIPE-008, FMT-PIPE-011, FMT-PIPE-013, FMT-PIPE-021 |

This spec covers the phase that rewrites syntax shape before any layout is decided, right after the tabs in whitespace are replaced (FMT-PIPE-001): braces for `if`/`else` bodies, block bodies for expression-bodied functions, expression bodies for simple accessors and get-only members, semicolon declarations for empty types, removal of trailing commas, and one field per declaration. Every rewrite produces equivalent code (FMT-PIPE-003) and is refused for a construct whose comments, directives, or disabled text it could not carry over (FMT-PIPE-002).

## Non-goals

- Line breaks, spacing, and indentation of the rewritten code, apart from the separators FMT-STRUCT-026 names and the layout it leaves to the field split. The phase only builds the new shape; later phases lay it out (FMT-PIPE-001).
- Braces for `while`, `for`, `foreach`, `using`, `lock`, `fixed`, and `do` bodies. The phase braces `if` and `else` bodies only; other unbraced bodies are left as they are.
- Braces for switch sections — FMT-SWITCH owns them.
- Expression-bodied properties, indexers, accessors, and lambdas. They are never turned into block bodies.
- Trailing commas in object initializers, anonymous object creations, collection expressions, switch expressions, and list patterns.
- Splitting event field declarations or local variable declarations with several declarators.
- Which language version applies — FMT-PIPE-008 decides it; this spec only names the version each transform needs.

## Requirements

### FMT-STRUCT-001 — `if` and `else` bodies are braced

The formatter MUST wrap an unbraced body of an `if` statement and an unbraced `else` body in a block. An `else` whose body is another `if` statement MUST NOT be wrapped, so an `else if` chain stays a chain and each `if` in it is braced on its own.

Why: An unbraced body invites the classic error of adding a second statement that only looks guarded. The `else if` exception keeps the chain flat instead of nesting every further branch one level deeper. The analyzer rule RH5405 reports the same shape.

Example:

```csharp
class C
{
    void M(bool a, bool b)
    {
        if (a)
            Foo();
        else if (b)
            Bar();
        else
            Baz();
    }
}
```

```csharp
class C
{
    void M(bool a, bool b)
    {
        if (a)
        {
            Foo();
        }
        else if (b)
        {
            Bar();
        }
        else
        {
            Baz();
        }
    }
}
```

### FMT-STRUCT-002 — Comments of a braced body stay with it

When the formatter braces a body under FMT-STRUCT-001, a comment on its own line above the body MUST move inside the new block in front of the statement, and a comment that trailed the statement MUST stay on the statement's line. The layout of these comments is kept as FMT-PIPE-013 states.

Why: Both comments describe the statement, so they belong inside the block with it. Dropping them would violate FMT-PIPE-002; leaving the leading one outside would make it describe the brace.

Example:

```csharp
class C
{
    void M(bool a)
    {
        if (a)
            // leading
            Foo(); // trailing
        else
            Bar(); /* block */
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
            // leading
            Foo(); // trailing
        }
        else
        {
            Bar(); /* block */
        }
    }
}
```

### FMT-STRUCT-003 — A body with directives is not braced

The formatter MUST NOT brace an `if` or `else` body whose leading or trailing trivia contains a preprocessor directive or disabled text.

Why: Bracing rebuilds the body's surrounding trivia from its comments only. A directive there would be dropped or end up on the wrong side of a brace, which changes what compiles under which configuration.

Example:

```csharp
class C
{
    void M(bool a)
    {
        if (a)
#if DEBUG
            Foo();
#else
            Bar();
#endif
    }
}
```

is left unchanged.

### FMT-STRUCT-004 — Expression-bodied methods and local functions get a block body

The formatter MUST convert an expression-bodied method or local function to a block body with a single statement:

- an expression statement when the return type is `void`, or when the function is `async` and returns the non-generic `Task` or `ValueTask`, with or without a nullable annotation (`Task?`);
- a `throw` statement when the expression is a throw expression;
- a `return` statement otherwise, including `async` functions that return `Task<T>` or `ValueTask<T>`, with or without a nullable annotation, and non-`async` functions that return `Task`.

An `async` function whose return type is neither `void` nor written as `Task`, `ValueTask`, `Task<…>`, or `ValueTask<…>` keeps its expression body instead (FMT-STRUCT-025). This applies under every language version.

Why: The house style uses expression bodies only for properties, indexers, and accessors; the analyzer rules RH3202 and RH3208 report expression-bodied methods and local functions. The statement form follows what the expression means in the function: an `async Task` body awaits and returns nothing, while a non-`async` `Task` method returns the task it computes. `return throw …;` does not compile, so a throw expression always becomes a `throw` statement.

Example:

```csharp
using System;
using System.Threading.Tasks;

class C
{
    void Run() => Console.WriteLine();

    int Get() => 42;

    async Task RunAsync() => await Task.Delay(1);

    async Task<int> GetAsync() => await Task.FromResult(1);

    Task Start() => Task.CompletedTask;

    int Fail() => throw new InvalidOperationException();
}
```

```csharp
using System;
using System.Threading.Tasks;

class C
{
    void Run()
    {
        Console.WriteLine();
    }

    int Get()
    {
        return 42;
    }

    async Task RunAsync()
    {
        await Task.Delay(1);
    }

    async Task<int> GetAsync()
    {
        return await Task.FromResult(1);
    }

    Task Start()
    {
        return Task.CompletedTask;
    }

    int Fail()
    {
        throw new InvalidOperationException();
    }
}
```

### FMT-STRUCT-005 — Expression-bodied constructors, finalizers, and operators get a block body

The formatter MUST convert expression-bodied constructors, finalizers, operators, and conversion operators to a block body with a single statement. Constructors and finalizers MUST use an expression statement, conversion operators a `return` statement, and operators the statement form FMT-STRUCT-004 gives their return type — so a `void` compound-assignment operator uses an expression statement. A throw expression MUST become a `throw` statement in every case.

Why: The same style decision as FMT-STRUCT-004, reported by the analyzer rules RH3203, RH3205, RH3206, and RH3207. Constructors and finalizers produce no value; a conversion always does.

Example:

```csharp
class C
{
    private int _value;

    C(int value) => _value = value;

    ~C() => Release();

    public static explicit operator int(C c) => c._value;

    public void operator +=(int value) => _value += value;
}
```

```csharp
class C
{
    private int _value;

    C(int value)
    {
        _value = value;
    }

    ~C()
    {
        Release();
    }

    public static explicit operator int(C c)
    {
        return c._value;
    }

    public void operator +=(int value)
    {
        _value += value;
    }
}
```

### FMT-STRUCT-006 — The converted body keeps the expression's trivia

When the formatter converts an expression body under FMT-STRUCT-004 or FMT-STRUCT-005, an expression written on the line after the arrow MUST join the created `return` keyword, a comment between the expression and the semicolon MUST stay in front of the statement's semicolon, and a comment or directive in the trivia of the removed arrow or semicolon MUST be carried into the block rather than dropped. The layout of the comments the converted body carries is kept as FMT-PIPE-013 states.

Unless a comment stands directly at the arrow, a comment that stands on a line of its own — nothing but whitespace in front of it on its line, and a line break behind it — between the token in front of the arrow and the arrow, or between the arrow and the expression, MUST be carried into the block as a comment on a line of its own in front of the created statement, in the order the comments were written. An expression behind such a comment MUST still join the created `return` keyword, so that the statement stays on one line as it would without the comment. A comment that starts the line after the arrow and is followed on that line by the expression MUST stay directly in front of the expression, which joins the created `return` keyword together with it (`return /* c */ 42;`). A comment stands directly at the arrow when it follows the arrow on the arrow's line, before the next token (`=> // b`, `=> /* c */ 42`), or precedes the arrow on a line on which no token stands in front of the arrow, behind the last line break in front of it (a line `/* c */ => 42;`). A comment behind the token in front of the arrow on the same line (`int M() /* c */ =>`) belongs to that token, does not stand at the arrow, and MUST NOT stop the move. When a comment stands directly at the arrow, the conversion MUST NOT move the comments on lines of their own in front of the created statement, and all comments of the member MUST keep the order in which they were written. A comment further along the arrow's line, behind the expression or behind the semicolon (`=> 42 /* t */;`, `=> 42; // t`), does not stand at the arrow and MUST NOT stop the move: the own-line comments still lead the created statement. This paragraph specifies the placement only for the comments it names; it leaves a documentation comment (`///`), the placement of a comment directly at the arrow and of the other comments of such a member, and a comment in front of an expression that becomes an expression statement or a `throw` statement, unspecified.

Why: The arrow and the semicolon disappear, so whatever trivia they carried needs a new owner. The line break after the arrow is layout only and has no meaning inside a `return` statement; a comment does, and FMT-PIPE-002 forbids losing it. A comment written on its own line next to the arrow introduces the body; in front of the created statement it keeps introducing it, while a place between the signature and the brace, or on a line of its own between `return` and its value, would put it next to other code (FMT-STRUCT-024). A comment written in front of the expression on the expression's line belongs to the expression and moves with it. Where a comment stands directly at the arrow, it lands on or in front of the new opening brace, so moving the own-line comments into the block would put them behind it, in reverse of the order the author wrote them; they stay where they are, and where that comment belongs is a separate decision. A comment behind the expression or the semicolon ends up behind the created statement, so moving the own-line comments keeps the source order.

Example:

```csharp
class C
{
    int Get() => 42 /* why */;

    int Other() =>
        Compute(1, 2);
}
```

```csharp
class C
{
    int Get()
    {
        return 42 /* why */;
    }

    int Other()
    {
        return Compute(1, 2);
    }
}
```

Comments on lines of their own around the arrow lead the created statement; a comment in front of the expression on its line stays in front of it:

```csharp
class C
{
    int Other()
    // before arrow
    => 1;

    int Get() =>
        // after arrow
        42;

    void Run() =>
        // run
        Foo();

    int Value() =>
        /* c */ 42;
}
```

```csharp
class C
{
    int Other()
    {
        // before arrow
        return 1;
    }

    int Get()
    {
        // after arrow
        return 42;
    }

    void Run()
    {
        // run
        Foo();
    }

    int Value()
    {
        return /* c */ 42;
    }
}
```

### FMT-STRUCT-007 — Directives that cannot survive keep the expression body

The formatter MUST keep the expression body of a member covered by FMT-STRUCT-004 or FMT-STRUCT-005 when any directive lies between the arrow and the start of the expression, when a conditional directive in the span from the arrow to the semicolon has its partner outside that span, or when a `#region` or `#endregion` lies in that span or in the arrow's leading trivia. A balanced conditional group, or another directive between the expression and the semicolon, MUST NOT block the conversion; it travels inside the created statement.

Why: A directive between the arrow and the expression would land directly after the created `return` keyword, where a directive cannot stand, so the result would not parse. Splitting a conditional group orphans the half that stays behind, and a region cannot keep both endpoints in place while the span is rebuilt. The decision is `ExpressionBodyRewriteUtilities.BlocksRewrite` in `Reihitsu.Core`, which the analyzer rules of FMT-STRUCT-004 and FMT-STRUCT-005 consult too, so an analyzer never reports a member whose fix would be refused.

Example — a balanced group between the expression and the semicolon does not block the conversion:

```csharp
class C
{
    int Get() => 42
#if DEBUG
        + 1
#endif
        ;
}
```

```csharp
class C
{
    int Get()
    {
        return 42
#if DEBUG
        + 1
#endif
        ;
    }
}
```

### FMT-STRUCT-008 — Other expression bodies are kept

The formatter MUST NOT convert expression-bodied properties, indexers, accessors, or lambdas to block bodies.

Why: For these members the expression body is the canonical form — FMT-STRUCT-009 and FMT-STRUCT-012 produce it. A lambda body is part of an expression, not a member body, and is out of the phase's reach.

Example:

```csharp
using System;

class C
{
    int P => 1;

    int this[int i] => i;

    Func<int> F = () => 1;
}
```

is left unchanged.

### FMT-STRUCT-009 — Single-statement accessors get an expression body

From C# 7 on, the formatter MUST convert a block-bodied `get`, `set`, or `init` accessor whose block holds exactly one statement to an expression-bodied accessor when the statement has an expression form: for a getter, a `return` with a value or a `throw` with an operand; for a setter or initializer, an expression statement or a `throw` with an operand. A `throw` statement MUST become a throw expression. Below C# 7 the accessor MUST keep its block.

Why: A one-line accessor reads best as one line. Expression-bodied accessors and throw expressions only exist from C# 7 on, so older projects keep the block (FMT-PIPE-008).

Example:

```csharp
using System;

class C
{
    private int _value;

    int Value
    {
        get
        {
            return _value;
        }
        set
        {
            _value = value;
        }
    }

    int Init
    {
        get
        {
            return _value;
        }
        init
        {
            throw new NotSupportedException();
        }
    }
}
```

```csharp
using System;

class C
{
    private int _value;

    int Value
    {
        get => _value;
        set => _value = value;
    }

    int Init
    {
        get => _value;
        init => throw new NotSupportedException();
    }
}
```

The property `Value` above is left unchanged under C# 6.

### FMT-STRUCT-010 — Accessors without an expression form keep their block

The formatter MUST keep the block of an accessor that holds more than one statement, no statement, a statement with attributes, or a statement without an expression form for that accessor kind — such as a rethrow without an operand, a `return` in a setter, `yield return`, a declaration, or a compound statement. Event `add` and `remove` accessors MUST keep their blocks.

Why: An expression body can only hold what an expression can express; any other statement would change meaning or not compile. Event accessors are outside the style rule.

Example:

```csharp
using System;

class C
{
    int Value
    {
        get
        {
            throw;
        }
        set
        {
            return;
        }
    }

    event EventHandler Changed
    {
        add
        {
            _handler += value;
        }
        remove
        {
            _handler -= value;
        }
    }
}
```

is left unchanged.

### FMT-STRUCT-011 — A throw operand keeps its meaning

When a `throw` statement becomes a throw expression under FMT-STRUCT-009 or FMT-STRUCT-012, the formatter MUST wrap its operand in parentheses when the operand's top-level node is a conditional, assignment, lambda, or query expression, and MUST leave every other operand bare.

Why: A throw expression parses its operand only down to the null-coalescing level. A bare conditional or assignment would bind the throw expression inside it and not compile, a bare lambda would not parse, and a bare query draws a precedence warning. Tighter operands, including `??`, re-parse unchanged and need no pair.

Example:

```csharp
class C
{
    int Value
    {
        get
        {
            throw _flag ? new A() : new B();
        }
        set
        {
            throw _a ?? _b;
        }
    }
}
```

```csharp
class C
{
    int Value
    {
        get => throw (_flag ? new A() : new B());
        set => throw _a ?? _b;
    }
}
```

### FMT-STRUCT-012 — Comments decide whether an accessor block can go

The formatter MUST keep an accessor's block when a comment, directive, or disabled text lies in trivia the conversion would delete or join onto the accessor's line: after the accessor keyword, on either side of the opening brace, before the statement, after `return`, before the returned expression, before the statement's semicolon, or before the closing brace. A comment that trailed the statement's semicolon or the closing brace MUST follow the new semicolon instead; when both carry a comment, the block MUST be kept. A single-line comment that trails the returned expression before the semicolon MUST also keep the block.

Why: The deleted tokens leave no place for a comment between them; moving it would change what it describes, and a single-line comment in front of the new semicolon would comment the semicolon out. A trailing comment describes the whole accessor and can follow it, but two of them cannot share one line end without reordering the author's lines.

Example:

```csharp
class C
{
    private int _value;

    int Value
    {
        get
        {
            // cached
            return _value;
        }
        set
        {
            _value = value; // store
        }
    }
}
```

```csharp
class C
{
    private int _value;

    int Value
    {
        get
        {
            // cached
            return _value;
        }
        set => _value = value; // store
    }
}
```

With a comment behind both the statement and the closing brace, the block is kept:

```csharp
class C
{
    private int _value;

    int Value
    {
        get
        {
            return _value; // a
        } // b
        set
        {
            _value = value;
        }
    }
}
```

```csharp
class C
{
    private int _value;

    int Value
    {
        get
        {
            return _value; // a
        } // b
        set => _value = value;
    }
}
```

### FMT-STRUCT-013 — Directives that cannot survive keep the accessor block

The formatter MUST keep an accessor's block when a conditional directive inside the block has its partner outside it, or when a `#region` or `#endregion` lies anywhere in the block. A balanced conditional group wholly inside the expression MUST NOT block the conversion.

Why: These are the hazards of FMT-STRUCT-007 in the opposite direction. Removing the braces would orphan half of a conditional group, and a region cannot keep both endpoints in place while the accessor is rebuilt.

Example:

```csharp
class C
{
    private int _value;

    int Value
    {
        get
        {
            return _value;
#if DEBUG
        }
#else
        }
#endif
        set
        {
            _value = value;
        }
    }
}
```

```csharp
class C
{
    private int _value;

    int Value
    {
        get
        {
            return _value;
#if DEBUG
        }
#else
        }
#endif
        set => _value = value;
    }
}
```

### FMT-STRUCT-014 — Get-only properties and indexers become expression-bodied members

From C# 6 on, the formatter MUST convert a property or indexer whose accessor list holds a single getter — block-bodied under the rules of FMT-STRUCT-009 to FMT-STRUCT-013, or already expression-bodied — to an expression-bodied member. A getter that throws MUST only be converted from C# 7 on. Below C# 6 the accessor list MUST be kept.

Why: `int P => x;` is the shortest equivalent of a get-only property. Expression-bodied properties and indexers need C# 6 and throw expressions need C# 7 (FMT-PIPE-008). The block-bodied getter is converted with exactly the rules of the accessor-level transform, so the member-level transform does not depend on it and still applies under C# 6, where FMT-STRUCT-009 does not.

Example:

```csharp
using System;

class C
{
    private int _value;

    int Value
    {
        get
        {
            return _value;
        }
    }

    int this[int index]
    {
        get
        {
            return index;
        }
    }

    int Fail
    {
        get
        {
            throw new NotSupportedException();
        }
    }
}
```

```csharp
using System;

class C
{
    private int _value;

    int Value => _value;

    int this[int index] => index;

    int Fail => throw new NotSupportedException();
}
```

Under C# 6, `Value` and the indexer are converted the same way and `Fail` keeps its accessor list unchanged.

### FMT-STRUCT-015 — Get-only members that keep their accessor list

The formatter MUST keep the accessor list of a property or indexer when the getter is an auto-accessor, carries an attribute or a modifier, cannot be converted under FMT-STRUCT-009 to FMT-STRUCT-013, or when the property has an initializer. It MUST also keep it when a comment, directive, or disabled text lies in trivia the conversion would delete or join onto the signature's line — after the signature, around the braces, around `get`, around the getter's arrow, before the expression, or before the getter's semicolon — or when a directive in the accessor list would be split as in FMT-STRUCT-013. A comment trailing the getter's semicolon or the closing brace MUST follow the member's new semicolon; when both carry a comment, the accessor list MUST be kept.

Why: An attribute on the getter would change its target on the member, and a modifier such as `readonly` would be lost. An auto-accessor has no expression to lift. The comment rules are those of FMT-STRUCT-012, applied to the accessor list.

Example:

```csharp
class C
{
    private int _value;

    int Auto { get; }

    int Attributed
    {
        [Obsolete]
        get => _value;
    }

    int Modified
    {
        readonly get => _value;
    }

    int Commented // note
    {
        get => _value;
    }
}
```

is left unchanged, while a trailing comment follows the member's semicolon unless the closing brace carries one too:

```csharp
class C
{
    private int _value;

    int Value
    {
        get => _value; // a
    }

    int Other
    {
        get => _value; // a
    } // b
}
```

```csharp
class C
{
    private int _value;

    int Value => _value; // a

    int Other
    {
        get => _value; // a
    } // b
}
```

### FMT-STRUCT-016 — Node-level formatting keeps accessor shapes

The formatter MUST apply FMT-STRUCT-009 to FMT-STRUCT-015 only through the document- and tree-level entry points, as FMT-PIPE-011 requires. Every other requirement of this spec MUST apply through every entry point.

Why: Accessor and get-only member shapes are a separate decision a code fix did not ask for (FMT-PIPE-011). The other transforms only canonicalize the construct the fix is already rewriting.

### FMT-STRUCT-017 — Empty types use a semicolon declaration

The formatter MUST replace the empty brace body of a class, struct, or interface with a semicolon from C# 12 on, of a record class from C# 9 on, and of a record struct from C# 10 on. An empty enum MUST keep its braces.

Why: `class C;` says "no members" without two lines of braces; the analyzer rules RH5412 to RH5416 report the braced form. Each kind is gated by the version that allows its semicolon form (FMT-PIPE-008); an enum has no such form.

Example:

```csharp
class Empty
{
}

struct EmptyStruct
{
}

interface IEmpty
{
}

record EmptyRecord(int Value)
{
}

record struct EmptyRecordStruct
{
}

enum E
{
}
```

```csharp
class Empty;

struct EmptyStruct;

interface IEmpty;

record EmptyRecord(int Value);

record struct EmptyRecordStruct;

enum E
{
}
```

Under C# 11, only the two records are converted.

### FMT-STRUCT-018 — An empty body with trivia keeps its braces

The formatter MUST keep the braces of an empty type declaration when anything other than whitespace and line breaks lies after the declaration header, before or after the opening brace, or before the closing brace.

Why: The braces are the only place such a comment or directive can stay; removing them would delete it. The decision is `EmptyTypeDeclarationSemicolonAnalysisUtilities.CanConvertSafely` in `Reihitsu.Core`, which the analyzer code fixes use too.

Example:

```csharp
class Commented // note
{
}

class Inside
{
    // reserved
}

class Directive
{
#if DEBUG
#endif
}
```

is left unchanged.

### FMT-STRUCT-019 — Enums have no trailing comma

The formatter MUST remove the comma after the last member of an enum and keep the comma's trivia, such as a trailing comment, on the last member. It MUST keep the comma when a conditional directive lies between it and the closing brace.

Why: The analyzer rule RH5409 reports the trailing comma; without it, the last member reads as the end of the list. A conditional directive in that gap means a different member may be last in another configuration, where the comma is still needed.

Example:

```csharp
enum Color
{
    Red,
    Green,
}

enum Size
{
    Small,
    Large, // largest
}

enum Mode
{
    A,
#if DEBUG
    B,
#endif
}
```

```csharp
enum Color
{
    Red,
    Green
}

enum Size
{
    Small,
    Large // largest
}

enum Mode
{
    A,
#if DEBUG
    B,
#endif
}
```

### FMT-STRUCT-020 — Array and collection initializers have no trailing comma

The formatter MUST remove the comma after the last element of an array initializer or a collection initializer under the same trivia and directive rules as FMT-STRUCT-019. Object initializers, anonymous object creations, and collection expressions MUST keep their trailing comma.

Why: The analyzer rules RH5410 and RH5411 report these two initializer kinds only; the other comma-separated lists are not part of the style rule.

Example:

```csharp
using System.Collections.Generic;

class C
{
    int[] _numbers = new[] { 1, 2, 3, };

    List<int> _list = new List<int>
                      {
                          1,
                          2,
                      };

    Point _point = new Point
                   {
                       X = 1,
                       Y = 2,
                   };

    int[] _expression = [1, 2, 3,];
}
```

```csharp
using System.Collections.Generic;

class C
{
    int[] _numbers = new[] { 1, 2, 3 };

    List<int> _list = new List<int>
                      {
                          1,
                          2
                      };

    Point _point = new Point
                   {
                       X = 1,
                       Y = 2,
                   };

    int[] _expression = [1, 2, 3,];
}
```

### FMT-STRUCT-021 — One field per declaration

The formatter MUST split a field declaration with several declarators in a class, struct, record, or interface into one field declaration per declarator, in source order, each repeating the original attributes, modifiers, and type, and each keeping its own initializer. Event field declarations and local variable declarations MUST NOT be split.

Why: One field per line keeps each field's attributes, documentation, and diff history separate; the analyzer rule RH7101 reports combined fields. Repeating the attributes and modifiers keeps every field exactly what it was.

Example:

```csharp
using System;

class C
{
    private int _x, _y = 1, _z;

    [NonSerialized]
    private static readonly string _a = "a", _b = "b";
}
```

```csharp
using System;

class C
{
    private int _x;
    private int _y = 1;
    private int _z;

    [NonSerialized]
    private static readonly string _a = "a";
    [NonSerialized]
    private static readonly string _b = "b";
}
```

### FMT-STRUCT-022 — Split fields keep their comments

When the formatter splits a field under FMT-STRUCT-021, the declaration's leading trivia, including its documentation comment, MUST stay on the first generated field; a comment that trailed a separating comma MUST trail the field that comma now ends; and a comment in front of a later declarator MUST move onto its own line in front of that declarator's field.

Why: Each comment keeps describing the declarator it was written next to. A documentation comment documents the declaration it precedes, which is the first field.

Example:

```csharp
class C
{
    /// <summary>
    /// The coordinates
    /// </summary>
    private int _x, // horizontal

                /* vertical */ _y;
}
```

```csharp
class C
{
    /// <summary>
    /// The coordinates
    /// </summary>
    private int _x; // horizontal

    /* vertical */
    private int _y;
}
```

### FMT-STRUCT-023 — A field with directives is not split

The formatter MUST NOT split a field declaration that contains a preprocessor directive or disabled text anywhere in its trivia.

Why: The split rebuilds each generated field's trivia from comments only, so a directive between the declarators would be dropped or end up guarding a different field.

For example, a declaration `private int _x, _debug, _y;` whose `_debug,` line is wrapped in `#if DEBUG` … `#endif` keeps its three declarators in one declaration.

### FMT-STRUCT-024 — A rewrite that cannot be completed safely is not applied

The formatter MUST NOT apply a rewrite of this phase when its result would not compile, would leave a declaration without documentation that applied to it in the input, or would put a comment next to other code than the code it was written next to. It MUST leave the construct as it was written instead. This holds for every transform of the phase, whether or not a requirement of its own names the case; FMT-STRUCT-007 and the comment clauses of FMT-STRUCT-012 are instances of it. It adds to FMT-PIPE-002 and FMT-PIPE-003 and does not replace them: a rewrite that would drop a comment, drop or misplace a directive or disabled text, or change what the code compiles to, is refused by those requirements, as FMT-STRUCT-003, FMT-STRUCT-018, and FMT-STRUCT-023 refuse it. A rewrite that keeps each comment next to the code it was written next to, such as one that moves a comment onto its own line in front of the same code, is not refused by this requirement.

Why: A structural rewrite only changes the shape of code the author wrote, so it is worth applying only when nothing else changes. A result that does not compile, a declaration that lost its documentation, or a comment that now describes other code is a regression the author has to find and repair by hand, which costs more than the canonical shape gains. A code fix is withheld in the same situations.

Example — a directive between the arrow and the expression would land directly behind the created `return`, where it does not parse (FMT-STRUCT-007):

```csharp
class C
{
    int M() =>
#if DEBUG
        1;
#else
    0;
#endif
}
```

is left unchanged.

### FMT-STRUCT-025 — An `async` function of an unknown task type keeps its expression body

The formatter MUST keep the expression body of an `async` method or local function whose return type is neither `void` nor written as `Task`, `ValueTask`, `Task<…>`, or `ValueTask<…>`, either as a simple name or as the last part of a qualified name, with or without a nullable annotation (`Task?`, `Task<int>?`). This covers an alias of a task type and a custom task-like type alike. An `async` function whose return type is written in one of these forms MUST be converted as FMT-STRUCT-004 states.

Why: Under `async`, the statement form depends on whether the task type carries a result, which a nullable annotation does not change: an expression statement for a task without one, a `return` statement for a task with one, and the wrong choice does not compile (CS1997 for a `return` with a value in an `async` function whose task has no result). The formatter sees only syntax, so behind an alias or a custom task-like type it cannot tell which form compiles, and a rewrite it cannot prove equivalent is not applied (FMT-PIPE-003, FMT-STRUCT-024).

Example:

```csharp
using T = System.Threading.Tasks.Task;
using V = System.Threading.Tasks.ValueTask<int>;

class C
{
    async T RunAsync() => await T.Delay(1);

    async V GetAsync() => await Compute();

    async MyTask StartAsync() => await Prepare();
}
```

is left unchanged. A task type written by its qualified name is converted:

```csharp
class C
{
    async System.Threading.Tasks.Task RunAsync() => await Prepare();

    async System.Threading.Tasks.Task<int> GetAsync() => await Compute();
}
```

```csharp
class C
{
    async System.Threading.Tasks.Task RunAsync()
    {
        await Prepare();
    }

    async System.Threading.Tasks.Task<int> GetAsync()
    {
        return await Compute();
    }
}
```

A nullable annotation does not hide the task type:

```csharp
using System.Threading.Tasks;

class C
{
    async Task? RunAsync() => await Task.Delay(1);

    async Task<int>? GetAsync() => await Task.FromResult(1);
}
```

```csharp
using System.Threading.Tasks;

class C
{
    async Task? RunAsync()
    {
        await Task.Delay(1);
    }

    async Task<int>? GetAsync()
    {
        return await Task.FromResult(1);
    }
}
```

### FMT-STRUCT-026 — The phase writes only the separators its text needs

A structural transform MUST NOT write whitespace, a line break, or indentation next to a token it creates, nor into the trivia it carries over, except a separator without which its output would not parse back to its tree (FMT-PIPE-021). These separators are decided:

- one space between a token the transform creates and the token after it when the two would otherwise read as one token, such as `return` before `42`; no space where they read as two tokens without it, such as `{` before `return`, `)` before `{`, or `return` before a comment;
- a line break between a single-line comment and a token the transform creates behind it, such as `// trailing` before an inserted `}`;
- a line break between a token the transform creates and a comment behind it that leads the next token, such as an inserted `{` before the comment `// leading` above the braced statement, or an inserted `}` before a comment `// about Bar` that leads the next statement;
- a line break between a token the transform creates and a preprocessor directive behind it, such as an inserted `}` before `#if`.

No other line break follows a token the transform creates: none behind an inserted `{` that the braced statement follows without a leading comment, and none behind an inserted `}` that `else`, a statement without a leading comment, or the enclosing block's `}` follows.

The trivia a transform carries over keeps what the author wrote:

- The whitespace in front of and between the comments the transform carries over — the comments that lead or trail a braced statement, and a comment that FMT-STRUCT-006 moves behind `return` or in front of the converted statement — stays as the author wrote it; the transform writes none there and drops none.
- The trailing trivia of a braced statement stays on the statement, its line break included, so a line break ends the statement's line in front of the inserted `}` exactly when the author wrote one.
- The trailing trivia of the `;` that a converted expression body ends with moves to the created `}`, its line break included.

The transform MAY drop the line breaks in front of the first comment that leads a braced statement, and the whitespace and line breaks of a token it removes when no carried comment stands next to them, such as the line break behind a removed `=>` or the whitespace in front of an expression that has no comment in front of it.

The field split (FMT-STRUCT-021) is outside this requirement: it writes the line break and indentation in front of each field it creates, a space in front of each comment it carries over, and the line break behind each comment it moves onto its own line, because the RH7101 code fix runs it without the phases after it. That exception is recorded until the fix formats through an entry point the pipeline spec names (thoenissen/Reihitsu#1502).

Why: The phases after this one, the trace, and the documentation phase read the text of this phase's tree, so the text has to be that tree (FMT-PIPE-021). Every other piece of layout is decided again by the phase that owns it — line breaks by FMT-BREAK, spaces by FMT-SPACE, columns by FMT-INDENT — so a transform that writes it makes one decision in two places (FMT-PIPE-001), and the two can disagree. A wrap is not a decision about the comments it carries (FMT-PIPE-013): the gaps the author left around them are what the phases see in code that needs no wrap, and the spacing phase keeps a gap in front of a comment as it finds it.

Example — the transform's output for the class of FMT-PIPE-021's first example holds `int Get(){return 42;}` followed by the line break the author wrote behind `;`, the author's line break between `// trailing` and the inserted `}` behind it, and no line break behind either inserted `}` of `M`. The final output is unchanged by this requirement:

```csharp
class C
{
    int Get()
    {
        return 42;
    }

    void M(bool a)
    {
        if (a)
        {
            Foo(); // trailing
        }
        else
        {
            Bar();
        }
    }
}
```

Example — in

```csharp
class C
{
    void M(bool a)
    {
        if (a)
            // leading
            Foo();
        if (a)
            /* x */ /* y */ Foo();
        if (a)
            Foo();
#if DEBUG
        Bar();
#endif
        if (a)
            Foo();
        // about Bar
        Bar();
    }
}
```

the transform's output has a line break behind the first inserted `{`, in front of `// leading`, and keeps the line break the author wrote behind that comment; it has a line break behind the second inserted `{`, in front of `/* x */`, and keeps `/* x */ /* y */ Foo();` on one line with the spaces the author wrote; it has no line break behind the third inserted `{`, and a line break between the third inserted `}` and `#if DEBUG`; and it has a line break between the fourth inserted `}` and `// about Bar`, so the comment keeps leading `Bar();`. The final output keeps the comments where the author put them (FMT-PIPE-013):

```csharp
class C
{
    void M(bool a)
    {
        if (a)
        {
            // leading
            Foo();
        }

        if (a)
        {
            /* x */ /* y */ Foo();
        }

        if (a)
        {
            Foo();
        }
#if DEBUG
        Bar();
#endif

        if (a)
        {
            Foo();
        }

        // about Bar
        Bar();
    }
}
```

Example — the gaps in front of carried trailing comments stay as written:

```csharp
class C
{
    void M(bool a)
    {
        if (a)
            Foo();    // x
        if (a)
            Foo();// y
        if (a)
            Foo(); /* a */ // b
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
            Foo();    // x
        }

        if (a)
        {
            Foo();// y
        }

        if (a)
        {
            Foo(); /* a */ // b
        }
    }
}
```

Example — a comment that FMT-STRUCT-006 moves behind `return` keeps the whitespace in front of it: in `int Get() =>`, followed by a line holding `/* c */ 42;` indented by eight spaces, the transform's output holds `return` followed by the eight spaces and `/* c */ 42;`, which the spacing phase turns into `return /* c */ 42;`; when that line is not indented, the output holds `return/* c */ 42;`, and the final output keeps `return/* c */` as written.

### FMT-STRUCT-027 — A removed comma's comment on a line of its own stays there

When the formatter removes a trailing comma under FMT-STRUCT-019 or FMT-STRUCT-020, a comment of that comma that stands on a line below the line the last element ends on — because the comma stood on a later line, or the comment led the comma on a line of its own — MUST stay on a line of its own and lead the closing brace. A comment of the comma on the line the last element ends on stays behind the last element, as FMT-STRUCT-019 states.

Why: A line break already ends the last element's line, so a comment behind it starts a new line, and in the text that line belongs to the closing brace (FMT-PIPE-021). Joining it to the last element's line would move a comment between lines, and behind a `//` comment it is not possible at all. The closing brace's leading comment is then laid out like any other comment in front of a closing brace, in one pass, which is what a second pass gives today.

Example:

```csharp
enum E { A,
B // about B
    , // comma
}

class C
{
    int[] _a = new[] { 1,
2 /* two */
// before comma
, };
}
```

```csharp
enum E
{
    A,
    B // about B

    // comma
}

class C
{
    int[] _a = new[] { 1,
                   2 /* two */

                   // before comma
               };
}
```

# A comment or directive in front of the arrow of an expression body is unsupported

| | |
|---|---|
| Status | Accepted |
| Date | 2026-11 |
| Affected requirements | FMT-UNSUP-013 (added), FMT-UNSUP-012 (changed: the gap in front of the `=>` is FMT-UNSUP-013's, and its example of the supported side loses the comments in front of the `=>`), FMT-STRUCT-006 (changed: no comment in front of the arrow to place), FMT-STRUCT-007 (changed: a region directive in the arrow's leading trivia is refused instead), FMT-STRUCT-015 (changed: the seams behind `get` and in front of the getter's arrow are refused instead), FMT-STRUCT-026 (changed: wording), SHARED-BODY-001 (changed: names the region directive in the leading trivia of `=>` and the comment in front of `=>`), SHARED-BODY-003 (changed: a comment, directive, or disabled text in front of `=>` withholds the fix), RH5401-004 (changed: a comment or directive in front of the `=>` withholds the fix through the precheck), RH7101-007 (changed: FMT-UNSUP-013 withholds the fix as well) |
| Depends on | [The formatter's Unsupported outcome](unsupported-precheck.md), [Declaring a shape unsupported is a refusal](unsupported-shape-as-refusal.md), [A comment or directive behind the arrow of an expression body is unsupported](expression-body-arrow-comment-unsupported.md) |

## What

The formatter declares a new unsupported shape (FMT-UNSUP-013): a comment, a preprocessor directive, or disabled text between the `=>` of an expression body and the token in front of it — in the trailing trivia of that token, or in the leading trivia of the `=>`. The expression bodies are those of FMT-UNSUP-012: the arrow clause of a method, a constructor, a finalizer, an operator, a conversion operator, a property, an indexer, an accessor, or a local function; the `=>` of a lambda and of a switch-expression arm is not one. The token in front of the `=>` is the last token of what the arrow clause follows: a property's name, an indexer's `]`, the `)` of a parameter list or a constructor initializer, the last token of a constraint clause, or an accessor's keyword. The shape holds whether the gap holds a line break or not. Every entry point refuses an input that holds the shape and reports it with the reason `a comment or directive between the arrow of an expression body and the code in front of it`; the CLI leaves the file unchanged, reports it as an error, and exits with code 2; and every code fix whose formatted node holds it is withheld (SHARED-FIX-010).

Not refused: a comment or directive above the member, in the leading trivia of its first token, its documentation comment included; a comment inside the signature in front of its last token, such as inside the parameter list; and every comment the formatter accepts behind the arrow today — inside the expression behind its first token, and behind the expression or the `;`.

With FMT-UNSUP-012, both gaps around the `=>` of an expression body are free of comments, directives, and disabled text in every input the phases receive. The line-break phase loses its last refusal to join a property's or indexer's arrow; the conversion of an expression body into a block loses its move of comments in front of the arrow; the lift of a get-only accessor list to the member loses its checks of the trivia behind `get` and in front of the getter's arrow (*Special cases*).

## Why

A comment in front of the arrow has no good place once the formatter acts on the member:

- For a property or an indexer, which keep their expression body (FMT-STRUCT-008), the line-break phase refused the join of FMT-BREAK-026 (FMT-BREAK-001), and the `=>` then started a line in the member's own column, which no requirement places. RH5401 reports such a property and could offer no fix. In a conditional group that chooses the arrow, or one around the signature that closes in front of it, the active `=>` was moved to the member's column and the inactive one left as written.
- For an accessor, which keeps its expression body as well and whose arrow FMT-BREAK-026 does not join, the lift of a get-only accessor list to the member had to check the trivia behind `get` and in front of the getter's arrow (FMT-STRUCT-015).
- For a method, a constructor, a finalizer, an operator, a conversion operator, or a local function, the conversion removes the arrow. FMT-STRUCT-006 moved comments on lines of their own in front of the created statement, but only when no comment stood on the arrow's line; such a comment was written directly in front of the new opening brace (`/* c */ {`), a placement FMT-STRUCT-006 left unspecified. A region directive in the arrow's leading trivia kept the whole body (FMT-STRUCT-007).

RH5040 already reports every comment directly in front of the `=>` of a member or an accessor (RH5040-001) — in the trailing trivia of the token in front of it and in the leading trivia of the `=>` alike — and RH3208 reports every expression-bodied local function as a whole, so a comment in front of the arrow is rare by the repository's definition. No rule reports a directive or disabled text there as such, although RH3202–RH3208 and RH5401 report most members that hold one as a whole. A conditional group that closes directly in front of the `=>` — around the signature or around a constraint clause, as multi-targeting code writes it — or that chooses the `=>` puts a directive into the gap and is refused (changes 9 and 12); the same choice written around the whole member, from in front of its first token through behind its `;`, leaves the gap free of directives and is formatted, before and after. Directives and disabled text are part of the shape because the join refusal and the seam checks of the get-only lift test them alike; with comments alone, both would stay. A comment that describes the member belongs above it, where the phases carry it as they carry every member's comment.

The maintainer decided before this run (escalation 10) to declare this shape unsupported, in a change of its own; thoenissen/Reihitsu#1851 records the answer. It is the twin of FMT-UNSUP-012, which the preceding change declared for the gap behind the arrow.

## Visible output changes

Every *After* below that says "refused" means: the file is unchanged, the CLI reports `error: <file>(<line>,<column>): unsupported syntax: a comment or directive between the arrow of an expression body and the code in front of it. The file was left unchanged.` with the position of the first comment, directive, or disabled text in the gap, and exits with code 2.

**1. A property with a `//` comment behind its name.** Input:

```csharp
class C
{
    int P // c
        => 1;
}
```

Before (`main`, `scripts/format.sh`): the arrow line lands in the member's column.

```csharp
class C
{
    int P // c
    => 1;
}
```

After: refused at `(3,11)`.

**2. An indexer with a `//` comment, a property with a block comment directly at the arrow, and a property with a block comment on one line.** Input:

```csharp
class C
{
    int this[int i] // c
        => i;

    int Q
        /* c */ => 2;

    int A /* c */ => 1;
}
```

Before (`main`): the arrow lines of the indexer and of `Q` land in the member's column; `A` is kept.

```csharp
class C
{
    int this[int i] // c
    => i;

    int Q
    /* c */ => 2;

    int A /* c */ => 1;
}
```

After: refused at `(3,21)`, the indexer's comment, which starts first. `A` alone, which the formatter kept, is refused as well.

**3. An accessor with a comment behind its keyword.** Input:

```csharp
class C
{
    private int _v;

    int V
    {
        get // c
            => _v;
    }
}
```

Before (`main`): the arrow line lands in the accessor's column.

```csharp
class C
{
    private int _v;

    int V
    {
        get // c
        => _v;
    }
}
```

After: refused at `(7,13)`.

**4. A method with a block comment directly at the arrow, and one with a block comment on one line.** Input:

```csharp
class C
{
    int B()
        /* c */ => 1;

    int M() /* c */ => 1;
}
```

Before (`main`): both are converted; the comment of `B` lands directly in front of the new opening brace.

```csharp
class C
{
    int B()
    /* c */ {
        return 1;
    }

    int M() /* c */
    {
        return 1;
    }
}
```

After: refused at `(4,9)`.

**5. A method with a comment on a line of its own in front of the arrow, and one with a comment behind its signature.** Input:

```csharp
class C
{
    int Other()
        // before arrow
        => 1;

    void Run() // run
        => Foo();

    void Foo()
    {
    }
}
```

Before (`main`): both are converted; the own-line comment leads the created statement (FMT-STRUCT-006), and `// run` stays behind the signature.

```csharp
class C
{
    int Other()
    {
        // before arrow
        return 1;
    }

    void Run() // run
    {
        Foo();
    }

    void Foo()
    {
    }
}
```

After: refused at `(4,9)`. This is the case the declaration costs most: the formatter converted these members well before. Written above the member, the same comment is formatted, before and after:

```csharp
class C
{
    // before arrow
    int Other() => 1;
}
```

```csharp
class C
{
    // before arrow
    int Other()
    {
        return 1;
    }
}
```

**6. A block comment in front of the arrow on one line refuses the whole file**, also the code elsewhere in it that the formatter would correct. Input:

```csharp
class C{
    int A /* c */ => 1;
}
```

Before (`main`):

```csharp
class C
{
    int A /* c */ => 1;
}
```

After: refused at `(2,11)`; `class C{` stays as written.

**7. A constructor with an initializer and a method with a constraint clause, each with a comment in front of the arrow.** Input:

```csharp
class B
{
    public B(int v)
    {
    }
}

class C : B
{
    private int _v;

    public C() : base(1) // c
        => _v = 1;

    T Make<T>()
        where T : new() // c
        => new T();
}
```

Before (`main`):

```csharp
class B
{
    public B(int v)
    {
    }
}

class C : B
{
    private int _v;

    public C()
        : base(1) // c
    {
        _v = 1;
    }

    T Make<T>()
        where T : new() // c
    {
        return new T();
    }
}
```

After: refused at `(12,26)`.

**8. A local function with a comment behind its signature.** Input:

```csharp
class C
{
    void M()
    {
        int Local() // c
            => 1;

        Local();
    }
}
```

Before (`main`):

```csharp
class C
{
    void M()
    {
        int Local() // c
        {
            return 1;
        }

        Local();
    }
}
```

After: refused at `(5,21)`.

**9. A directive in front of the arrow.** Input, parsed without `DEBUG`:

```csharp
class C
{
    int A
#if DEBUG
        => 1;
#else
        => 2;
#endif

    int M()
#if DEBUG
        => 1;
#else
        => 0;
#endif
}
```

Before (`main`): the active `=>` of the property lands in the member's column; the active branch of the method is converted inside the conditional group.

```csharp
class C
{
    int A
#if DEBUG
        => 1;
#else
    => 2;
#endif

    int M()
#if DEBUG
        => 1;
#else
    {
        return 0;
    }
#endif
}
```

After: refused at `(4,1)`, in every build.

A region directive in the arrow's leading trivia. Input:

```csharp
class C
{
    int M()
        #region R
        => 1;
        #endregion
}
```

Before (`main`): the method keeps its expression body (FMT-STRUCT-007), and blank lines are written around the directives.

```csharp
class C
{
    int M()

    #region R

    => 1;

    #endregion // R
}
```

After: refused at `(4,9)`.

**10. The code fixes that convert an expression body to a block.** RH3202 (`Expression-style methods should not be used`), RH3203 (`Expression-style constructors should not be used`), RH3205 (`Expression-style operators should not be used`), RH3206 (`Expression-style conversion operators should not be used`), RH3207 (`Expression-style finalizers should not be used`), and RH3208 (`Expression-style local functions should not be used`) format the member they convert, so their fix is withheld for a member that holds the shape (SHARED-FIX-010); the diagnostics stay. RH3202, input:

```csharp
class C
{
    int Other()
    // before arrow
    => 1;

    void Run() // run
        // first
        => Foo();

    void Foo()
    {
    }
}
```

Before (`main`, `scripts/apply-fix.sh`): the fix is offered for both and gives

```csharp
class C
{
    int Other()
    {
        // before arrow
        return 1;
    }

    void Run() // run
    {
        // first
        Foo();
    }

    void Foo()
    {
    }
}
```

After: no fix offered for either; both stay reported. RH3203 on `public C() : base(1) // c` followed by `=> _v = 1;` gave `public C() : base(1) // c` followed by a block holding `_v = 1;`, and is withheld in the same way; RH3208 on `int Local() // c` followed by `=> 1;` gave a block holding `return 1;` behind `int Local() // c`, and is withheld.

**11. Other code fixes that format a node holding the shape** are withheld (SHARED-FIX-010); their diagnostics stay. RH5110 (`Generic type constraints should be on their own line`), which formats the declaration. Input:

```csharp
class C
{
    T Make<T>() where T : new() // c
        => new T();
}
```

Before (`main`): the fix is offered and gives

```csharp
class C
{
    T Make<T>()
        where T : new() // c
    {
        return new T();
    }
}
```

After: no fix offered; the diagnostic stays. RH7101 (`Do not combine fields`), which formats the whole type (RH7101-004), on

```csharp
class C
{
    private int _a, _b;

    int A // c
        => _a;
}
```

gave `private int _a;` and `private int _b;` on two lines with the rest unchanged; after, no fix is offered and the diagnostic stays. The same holds for every fix that formats through the formatter wherever the node it formats holds such an expression body.

**12. A conditional group around the signature or a constraint clause, closing in front of a shared arrow** (new). The group's closing directive, and without the symbol also the group's opening directive and its disabled text, stand in the leading trivia of the `=>`. Input, parsed without `NET8_0` and `NET7_0_OR_GREATER`:

```csharp
using System.Numerics;

class C
{
#if NET8_0
    public int M(int a)
#else
    public int M(long a)
#endif
        => 1;

    T Make<T>()
#if NET7_0_OR_GREATER
        where T : INumber<T>
#endif
        => default;

#if NET8_0
    public int P
#else
    public long P
#endif
        => 1;
}
```

Before (`main`, `scripts/format.sh`): both methods are converted with every other line unchanged; the active `=>` of the property lands in the member's column.

```csharp
using System.Numerics;

class C
{
#if NET8_0
    public int M(int a)
#else
    public int M(long a)
#endif
    {
        return 1;
    }

    T Make<T>()
#if NET7_0_OR_GREATER
        where T : INumber<T>
#endif
    {
        return default;
    }

#if NET8_0
    public int P
#else
    public long P
#endif
    => 1;
}
```

After: refused at `(9,1)`, the `#endif` in front of the first `=>`. Alone in a file, `Make` is refused at its `#if NET7_0_OR_GREATER` and `P` at its `#endif`.

RH3202 (`Expression-style methods should not be used`) reports both methods before and after (SHARED-BODY-001). Before (`main`, `scripts/apply-fix.sh RH3202`), its fix is offered on each and gives the same block as the formatter, LF and CRLF alike; after, no fix is offered on either. RH5401 reports the property and offers no fix, before and after.

The same group written around the whole member is formatted, and RH3202's fix offered, before and after:

```csharp
class C
{
#if NET8_0
    public int M(int a) => 1;
#else
    public int M(long a) => 1;
#endif
}
```

gives

```csharp
class C
{
#if NET8_0
    public int M(int a) => 1;
#else
    public int M(long a)
    {
        return 1;
    }
#endif
}
```

Unchanged:

- RH5401 (`Expression-style get-only properties should be single-lined`) reports the property of change 1, and `int D⏎/* note */ => 1;`, and offers no fix, before and after; only the reason it is withheld changes (RH5401-004). It keeps reporting them: a shape the formatter refuses as unsupported is not one it keeps (SHARED-DIAG-010).
- RH5040 still reports the comments, and RH3202–RH3208 still report the members of change 10; analyzers do not consult the precheck. RH3202–RH3208 still do not report a member with a `#region` or `#endregion` in the leading trivia of `=>` (SHARED-BODY-001).
- A conditional group around the whole member is formatted as before (change 12's alternative).
- A comment above the member is formatted as before (change 5's alternative), and so is a comment inside the signature in front of its last token, such as `int F(/* c */ int a) => a;`.
- A comment behind the arrow is refused as before, with FMT-UNSUP-012's reason.
- The `=>` of a lambda and of a switch-expression arm: `Func<int> f = () /* c */ => 1;` and `1 /* c */ => "one"` are formatted as before.

No rule page changes: the fixes above lose their action only for the refused shape, which `documentation/rules/general-notes.md` covers ("A diagnostic without a code fix").

## Decisions

### Declare the shape, in this change (maintainer, escalation 10)

The maintainer decided before this run to declare a comment, directive, or disabled text between the `=>` of an expression body and the code in front of it unsupported, in a change of its own, with the span the issue gives: the trailing trivia of the token in front of the `=>` and the leading trivia of the `=>`. This change is that declaration. Changes 1–11 lie inside that span — comments, directives, and disabled text in either trivia list, for every owner the issue lists — so the run did not ask them again.

The run asked the two places the issue did not settle, and the maintainer chose each as recommended:

- A conditional group around the signature, a constraint clause, or a property's type that closes directly in front of a shared `=>` (change 12) is part of the shape: the formatter refuses it, RH3202's fix and every other fix that formats such a member are withheld, and RH3202 and RH5401 keep reporting the member. It is declared in this change, not in one of its own.
- The reason the CLI reports is `a comment or directive between the arrow of an expression body and the code in front of it`, not the issue's wording `a comment or directive between an expression body's arrow and the code in front of it`, because it pairs with FMT-UNSUP-012's reason, `a comment or directive between the arrow of an expression body and its expression`.

### The same expression bodies as FMT-UNSUP-012

The shape names the arrow clause, whatever owns it: a member, an accessor, or a local function, as FMT-UNSUP-012 does. The conversion code is shared by every converted member, the local function included, and the accessor shares the property's join and column. The `=>` of a lambda and of a switch-expression arm belongs to an expression; no transform converts it and FMT-BREAK-026 does not join it, so declaring it would delete nothing.

### The boundary is the token in front of the `=>`

The gap ends at the last token of what the arrow clause follows. A comment above the member is in the leading trivia of the member's first token and is the place where a comment that describes the member belongs; a comment inside the signature belongs to the signature's own constructs. Neither costs a special case of the expression body.

### Directives and disabled text belong to the shape

The join refusal of the line-break phase and the seam checks of the get-only lift test the gap for comments, directives, and disabled text alike, so a shape of comments alone would delete neither. The cost is changes 9 and 12: a property or method whose arrow a conditional group chooses, a region directive in front of the arrow, and a conditional group around the signature or a constraint clause that closes in front of a shared arrow, which the formatter and RH3202's fix converted well before. The maintainer accepted that cost for change 12 (*Declare the shape, in this change*); the same choice written around the whole member stays supported.

### Also on one line

A block comment in front of the arrow on the arrow's line (`int A /* c */ => 1;`) is refused as well, as FMT-UNSUP-008, FMT-UNSUP-011, and FMT-UNSUP-012 refuse their places on one line. The lift of a get-only accessor list deletes `get`, so its check of the trivia behind `get` goes only if the shape covers `get /* c */ => _v;` on one line too.

### A node-level input holds the shape only with both tokens

The gap lies between the `=>` and the token in front of it, so an input that holds the `=>` but not that token — the arrow clause itself — does not hold the shape (FMT-UNSUP-003). No caller formats an arrow clause alone; the property and indexer layout, the conversion, and the get-only lift each see the whole declaration.

### A requirement of its own

The shape is a requirement of its own, FMT-UNSUP-013, not a widening of FMT-UNSUP-012: it is one decision with its own reason text, and FMT-UNSUP-012's reason, which the CLI reports today, stays as it is.

### FMT-UNSUP-010 keeps its reason

A directive in front of the arrow of a local function inside an initializer value of a field declaration with several declarators is refused under FMT-UNSUP-010 already; both shapes start at the same directive, and the reason stays FMT-UNSUP-010's, as FMT-UNSUP-011 and FMT-UNSUP-012 keep it.

### The analyzers keep their decisions

`ExpressionBodyRewriteUtilities.BlocksRewrite` refuses a member with a `#region` or `#endregion` from the start of the arrow's full span, the arrow's leading trivia included. The formatter no longer meets such a member, but the analyzers of RH3202–RH3208 still decide by it not to report the member, so the check stays and SHARED-BODY-001 names the case itself instead of reaching it through FMT-STRUCT-007.

### RH5401-004 changed in place

RH5401's fix still formats the property and compares, because that decides the wrapped expression the formatter keeps on several lines. For a comment or directive in front of the arrow, the fix is now withheld because the formatter refuses the property, which the fix already asks (SHARED-FIX-010). The requirement names both reasons; the visible result does not change.

## Special cases

### Deleted

- `PropertyLayoutLineBreakRewriter.CollapseExpressionBody`: the refusal `LineBreakTriviaUtilities.WouldJoinAcrossUnjoinableTrivia(expressionBody.ArrowToken.GetPreviousToken(), expressionBody.ArrowToken)`. It tests exactly the trailing trivia of the token in front of the arrow and the arrow's leading trivia for comments, directives, and disabled text — FMT-UNSUP-013's place — and the rewriter visits only property and indexer declarations, so both tokens are inside every input it sees (FMT-UNSUP-003). With FMT-UNSUP-012's refusal already gone, the method refuses no join any more.
- `ExpressionBodyToBlockConverter.CreateBlock` and its helpers: the move of comments on lines of their own out of the arrow's leading trivia in front of the created statement — the condition in `CreateBlock`, `ArrowLineHoldsComment`, `TrySplitOwnLineComments`, and the line break inserted in front of the moved comment lines. The arrow's leading trivia then holds whitespace and line breaks at most. The converter runs only on whole members, whose input holds both tokens.
- `GetOnlyMemberExpressionBodyTransform.HasLayoutOnlySeams`: the checks of the getter keyword's trailing trivia and of the getter arrow's leading trivia. A getter written with an expression body brings that gap into the input, where the precheck refuses it; a getter converted from a block keeps its block when its keyword's trailing trivia holds a comment, directive, or disabled text (FMT-STRUCT-012), and its created arrow gets no trivia (`AccessorExpressionBodyTransform.ConvertToExpressionBody`).
- Spec text: FMT-STRUCT-006's placement of comments on lines of their own in front of the arrow, its definition of a comment directly at the arrow, and its example; FMT-STRUCT-007's clause for a region directive in the arrow's leading trivia (the analyzers' case moves to SHARED-BODY-001); FMT-STRUCT-015's seams behind `get` and in front of the getter's arrow; SHARED-BODY-003's pointer to FMT-STRUCT-006 for a comment in front of `=>`; RH5401-004's statement that a comment or directive in front of the `=>` keeps the property on several lines.

### Checked and kept

- `ExpressionBodyRewriteUtilities.BlocksRewrite`, its region check from the arrow's full span start included: the analyzers of RH3202–RH3208 decide by it not to report such a member (SHARED-BODY-001).
- `LineBreakTriviaUtilities.WouldJoinAcrossUnjoinableTrivia`: other joins of the line-break phase use it.
- `ExpressionBodyToBlockConverter`'s handling of the semicolon's trivia (`ExtractSignificantTrivia`) and of the expression's leading whitespace in `CreateReturnStatement`: they serve trivia outside the gap.
- `GetOnlyMemberExpressionBodyTransform.HasLayoutOnlySeams`'s other seams — the trailing trivia of the token in front of the accessor list, both sides of the opening brace, the leading trivia of `get`, the getter semicolon's leading trivia, and the closing brace's leading trivia: the lift deletes those tokens or joins their trivia onto the signature's line, and the trivia behind the signature and around the opening brace would otherwise land in front of the created arrow, where the formatter would produce the shape it refuses (FMT-UNSUP-005).
- `AccessorExpressionBodyTransform`'s seam checks of the accessor keyword's trailing trivia and of the opening brace: that trivia would land in front of the arrow the conversion creates (FMT-UNSUP-005).
- `ExpressionBodyCodeFixUtilities.ConvertBodyAsync`: it copies the formatted trailing trivia of the last token in front of the body and holds no case for a comment.
- `RH5401ExpressionStyleGetOnlyPropertiesShouldBeSingleLinedCodeFixProvider.CanCollapse`: it decides the wrapped expression.
- `ArrowSpacingRule`: it spaces every arrow alike and holds no case for a comment.
- RH5040's analyzer, RH5401's analyzer, and the analyzers of RH3202–RH3208: analyzers do not consult the precheck.

## Alternatives considered

- **Do not declare the shape; repair the layout in front of the arrow** — rejected by the maintainer: it keeps the join refusal and the conversion's comment move, and adds a continuation column for the `=>` behind a comment and a placement for a comment directly at a method's arrow.
- **Widen FMT-UNSUP-012 to both gaps** — rejected: it changes the reason the CLI reports today for the gap behind the arrow, and puts two places with two costs under one requirement.
- **Comments only, without directives and disabled text** — rejected: the join refusal and the seam checks of the get-only lift test directives alike, so they would stay.
- **Only a gap with a line break** — rejected: the get-only lift would keep its check of the trivia behind `get` for `get /* c */ => _v;`, and the boundary would differ from FMT-UNSUP-008, FMT-UNSUP-011, and FMT-UNSUP-012.
- **Members and accessors only, as RH5040 reports, not local functions** — rejected: the conversion of a local function runs through the same code, which would keep the comment move for it alone.
- **Also the arrow of a lambda and of a switch-expression arm** — rejected: nothing would be deleted for them.
- **Leave a conditional group that closes in front of a shared `=>` out of the shape** (change 12) — rejected by the maintainer: the join refusal and the get-only lift's seam checks test directives as they test comments, so they would stay.
- **The issue's reason wording, `a comment or directive between an expression body's arrow and the code in front of it`** — rejected by the maintainer: FMT-UNSUP-012's reason says "the arrow of an expression body", and the two reasons read as a pair.
- **Stop RH5401 reporting a property with a comment or directive in front of the arrow** — not chosen: SHARED-DIAG-010 asks a layout rule to report a shape the formatter refuses as unsupported, so the report stays and only its fix is withheld, as before.

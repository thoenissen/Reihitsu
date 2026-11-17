# A comment or directive behind the arrow of an expression body is unsupported

| | |
|---|---|
| Status | Draft |
| Date | 2026-11 |
| Affected requirements | FMT-UNSUP-012 (added), FMT-STRUCT-006 (changed: no comment between the arrow and the expression to place), FMT-STRUCT-007 (changed: a directive behind the arrow is refused instead), FMT-STRUCT-015 (changed: a comment behind the getter's arrow is refused instead), FMT-STRUCT-024 (changed: example), FMT-STRUCT-026 (changed: the examples of comments behind the arrow are removed), SHARED-BODY-001 (changed: names the directive behind the arrow itself), RH5401-004 (changed: a comment behind the arrow withholds the fix through the precheck), RH7101-007 (changed: FMT-UNSUP-012 withholds the fix as well) |
| Depends on | [The formatter's Unsupported outcome](unsupported-precheck.md), [Declaring a shape unsupported is a refusal](unsupported-shape-as-refusal.md) |

## What

The formatter declares a new unsupported shape (FMT-UNSUP-012): a comment, a preprocessor directive, or disabled text between the `=>` of an expression body and its expression — in the trailing trivia of the `=>`, or in the leading trivia of the expression's first token. An expression body is the arrow clause of a method, a constructor, a finalizer, an operator, a conversion operator, a property, an indexer, an accessor, or a local function; the `=>` of a lambda and of a switch-expression arm is not one. The shape holds whether the gap holds a line break or not. Every entry point refuses an input that holds the shape and reports it with the reason `a comment or directive between the arrow of an expression body and its expression`; the CLI leaves the file unchanged, reports it as an error, and exits with code 2; and every code fix whose formatted node holds it is withheld (SHARED-FIX-010).

Not refused: a comment in front of the `=>` — behind the signature on its line, on a line of its own above the `=>`, or in front of the `=>` on its line — a comment inside the expression behind its first token, and a comment behind the expression or the `;`.

The conversion of an expression body into a block (FMT-STRUCT-004, FMT-STRUCT-005) loses every case it had for a comment behind the arrow (FMT-STRUCT-006); the line-break phase loses its refusal to join a property's or indexer's expression onto the arrow's line; the lift of a get-only accessor list to the member loses its check of the getter's arrow gap (*Special cases*). The layout of an expression that a comment behind the arrow kept on the next line — in the member's own column — is no longer reachable and needs no repair.

## Why

A comment behind the arrow has no good place once the formatter acts on the member:

- For a method, constructor, finalizer, operator, conversion operator, or local function, the conversion removes the arrow. FMT-STRUCT-006 then carried a comment on the arrow's line onto the new opening brace (`{// c`), carried own-line comments in front of the created statement, moved a block comment behind the created `return`, or kept it leading the expression — four cases, each with its own code and examples, and a fifth that FMT-STRUCT-006 leaves unspecified. A directive there kept the whole body (FMT-STRUCT-007).
- For a property or an indexer, which keeps its expression body (FMT-STRUCT-008), the line-break phase refused the join that FMT-BREAK-026 asks for (FMT-BREAK-001), and the expression then started a line that no requirement places, in the member's own column; with a comment on a line of its own, a blank line was written in front of it as well. RH5401 reports such a property and could offer no fix.
- For an accessor, the expression was likewise left in the accessor's column, and the lift of a get-only accessor list to the member kept the list.

RH5040 already reports a comment directly before or after the arrow of a member or accessor, so the comment is rare by the repository's definition. A local function's arrow is outside RH5040 (RH5040-002), but a comment behind it is no more common, and its conversion runs through the same code as a method's; leaving it out would keep every case of FMT-STRUCT-006 alive for it alone. A directive or disabled text behind the arrow is rarer still, the conversion already refused it, and with comments alone the refusal of the line-break join would have to stay. A comment that describes the body belongs above the member, where the phases carry it as they carry every member's comment.

The maintainer decided on 2026-11-14 to declare this shape unsupported, in a change of its own, with the same boundary as for the conditional operator (FMT-UNSUP-008) and the binary operator (FMT-UNSUP-011): comments above the member stay supported, only the position behind the `=>` is refused. The decision was recorded in the issue text only, so this change asks it again with the visible changes below.

## Visible output changes

Every *After* below that says "refused" means: the file is unchanged, the CLI reports `error: <file>(<line>,<column>): unsupported syntax: a comment or directive between the arrow of an expression body and its expression. The file was left unchanged.` with the position of the first comment, directive, or disabled text in the gap, and exits with code 2.

**1. A property with a `//` comment behind the arrow.** Input:

```csharp
class C
{
    int A => // c
        1;
}
```

Before (`main`, `scripts/format.sh`): the expression lands in the member's column.

```csharp
class C
{
    int A => // c
    1;
}
```

After: refused at `(3,14)`.

**2. A property with a comment on a line of its own behind the arrow, and an indexer with a block comment on one line.** Input:

```csharp
class C
{
    int this[int i] => /* c */ i;

    int B =>
        // c
        1;
}
```

Before (`main`): the indexer is kept; a blank line is written in front of the comment of `B`, and the comment and the expression land in the member's column.

```csharp
class C
{
    int this[int i] => /* c */ i;

    int B =>

    // c
    1;
}
```

After: refused at `(3,24)`, the indexer's comment, which starts first.

**3. A method with a `//` comment behind the arrow.** Input:

```csharp
class C
{
    int Get() => // c
        42;
}
```

Before (`main`): the comment lands on the new opening brace.

```csharp
class C
{
    int Get()
    {// c
        return 42;
    }
}
```

After: refused at `(3,18)`.

**4. A method with a comment on a line of its own behind the arrow, or in front of the expression on its line.** Input:

```csharp
class C
{
    int Other() =>
        // after arrow
        42;

    int Value() =>
        /* c */ 42;
}
```

Before (`main`): both are converted, the own-line comment leads the created statement, and the block comment stays in front of the expression (FMT-STRUCT-006).

```csharp
class C
{
    int Other()
    {
        // after arrow
        return 42;
    }

    int Value()
    {
        return /* c */ 42;
    }
}
```

After: refused at `(4,9)`. This is the case the declaration costs most: the formatter converted these members well before. Written above the member, the same comment is formatted:

```csharp
class C
{
    // after arrow
    int Other() => 42;
}
```

```csharp
class C
{
    // after arrow
    int Other()
    {
        return 42;
    }
}
```

**5. A block comment behind the arrow on one line.** The whole file is refused, also the code elsewhere in it that the formatter would correct. Input:

```csharp
class C{
    int Get() => /* c */ 42;
}
```

Before (`main`):

```csharp
class C
{
    int Get()
    {/* c */
        return 42;
    }
}
```

After: refused at `(2,18)`; `class C{` stays as written.

**6. An accessor with a comment behind the arrow.** Input:

```csharp
class C
{
    private int _v;

    int A
    {
        get => // c
            _v;
        set => /* c */ _v = value;
    }
}
```

Before (`main`): the getter's expression lands in the accessor's column.

```csharp
class C
{
    private int _v;

    int A
    {
        get => // c
        _v;
        set => /* c */ _v = value;
    }
}
```

After: refused at `(7,16)`.

**7. A local function with a comment behind the arrow.** Input:

```csharp
class C
{
    void M()
    {
        int Local() => /* c */ 1;

        int Other() =>
            // c
            1;

        Local();
        Other();
    }
}
```

Before (`main`):

```csharp
class C
{
    void M()
    {
        int Local()
        {/* c */
            return 1;
        }

        int Other()
        {
            // c
            return 1;
        }

        Local();
        Other();
    }
}
```

After: refused at `(5,24)`.

**8. A directive behind the arrow.** Input, parsed without `DEBUG`:

```csharp
class C
{
    int A =>
#if DEBUG
        1;
#else
        2;
#endif

    int M() =>
#if DEBUG
        1;
#else
        0;
#endif
}
```

Before (`main`): the method keeps its expression body (FMT-STRUCT-007), and the active line of each member lands in the member's column.

```csharp
class C
{
    int A =>
#if DEBUG
        1;
#else
    2;
#endif

    int M() =>
#if DEBUG
        1;
#else
    0;
#endif
}
```

After: refused at `(4,1)`, in every build.

**9. The code fixes that convert an expression body to a block.** RH3202 (`Expression-style methods should not be used`), RH3203 (`Expression-style constructors should not be used`), RH3205 (`Expression-style operators should not be used`), RH3206 (`Expression-style conversion operators should not be used`), RH3207 (`Expression-style finalizers should not be used`), and RH3208 (`Expression-style local functions should not be used`) format the member they convert, so their fix is withheld for a member that holds the shape (SHARED-FIX-010); the diagnostics stay. RH3202, input:

```csharp
class C
{
    int Get() => // c
        42;

    int Other() =>
        // after arrow
        42;
}
```

Before (`main`, `scripts/apply-fix.sh`): the fix is offered for both and gives

```csharp
class C
{
    int Get()
    {// c
        return 42;
    }

    int Other()
    {
        // after arrow
        return 42;
    }
}
```

After: no fix offered for either; both stay reported. RH3203 on `public C(int v) => // c` followed by `_v = v;` gave `public C(int v)` with `{// c` and is withheld in the same way; RH3208 on `int Local() => /* c */ 1;` gave `{/* c */` and is withheld.

**10. Other code fixes that format a node holding the shape** are withheld (SHARED-FIX-010); their diagnostics stay. For example, RH7101 (`Do not combine fields`), which formats the whole type (RH7101-004). Input:

```csharp
class C
{
    private int _a, _b;

    int A => // c
        _a;
}
```

Before (`main`): the fix is offered and gives

```csharp
class C
{
    private int _a;
    private int _b;

    int A => // c
        _a;
}
```

After: no fix offered; the diagnostic stays. The same holds for every fix that formats through the formatter wherever the node it formats holds such an expression body.

Unchanged:

- RH5401 (`Expression-style get-only properties should be single-lined`) reports the property of change 1 and offers no fix, before and after; only the reason it is withheld changes (RH5401-004). It keeps reporting it: a shape the formatter refuses as unsupported is not one it keeps (SHARED-DIAG-010).
- RH5040 still reports the comments, and RH3202–RH3208 still report the members of change 9; analyzers do not consult the precheck.
- RH3202–RH3208 still do not report a member with a directive between `=>` and the expression (SHARED-BODY-001).
- A comment in front of the arrow is formatted as before, such as `int A /* c */ => 1;` and an own-line comment above the `=>`, which the conversion of a method still moves in front of the created statement (FMT-STRUCT-006).
- The `=>` of a lambda and of a switch-expression arm: `Func<int> f = () => /* c */ 1;` is formatted as before.

No rule page changes: the fixes above lose their action only for the refused shape, which `documentation/rules/general-notes.md` covers ("A diagnostic without a code fix").

## Decisions

### Declare the shape, in this change (maintainer, escalation 10; pending confirmation)

The maintainer decided before this run to declare a comment or directive between the `=>` of an expression-bodied member and its expression unsupported, in a change of its own. The issue carrying that answer is `agent-filed`, so the run asks it again with the visible changes above.

### Every expression body, also an accessor's and a local function's

The shape names the arrow clause, whatever owns it: a member, an accessor, or a local function. The conversion code that a comment behind the arrow forces into special cases is shared by every converted member, the local function included, so leaving one kind out keeps every case alive for it. The accessor and the property share the join refusal of the line-break phase and the column-of-the-member layout.

### Not the arrow of a lambda or a switch-expression arm

A lambda and a switch-expression arm belong to an expression. No transform converts them (FMT-STRUCT-008), the line-break phase does not join their bodies under FMT-BREAK-026, and RH5040 leaves them alone (RH5040-002); declaring them would delete nothing.

### Directives and disabled text belong to the shape

The join refusal of the line-break phase and the seam check of the get-only lift test the gap for comments, directives, and disabled text alike, so a shape of comments alone would delete neither. The conversion already refused a directive there (FMT-STRUCT-007); the cost is change 8, a property or accessor whose expression is chosen by a conditional group behind the arrow, which the formatter laid out in the member's column before.

### Also on one line

A block comment behind the arrow with the expression on the same line (`int Get() => /* c */ 42;`) is refused as well, as FMT-UNSUP-008 and FMT-UNSUP-011 refuse the same place on one line. The conversion carries such a comment onto the new opening brace with code of its own, which goes only if the shape covers it.

### FMT-UNSUP-010 keeps its reason

A directive behind the arrow of a local function inside an initializer value of a field declaration with several declarators is refused under FMT-UNSUP-010 already; both shapes start at the same directive, and the reason stays FMT-UNSUP-010's, as FMT-UNSUP-009 and FMT-UNSUP-011 keep it.

### The analyzers of the expression-body rules keep their decision for a directive

`ExpressionBodyRewriteUtilities.BlocksRewrite` is the predicate by which both the formatter's conversions and the analyzers of RH3202–RH3208 decide (SHARED-BODY-001). Its check of a directive between the arrow and the expression is no longer needed by the formatter, which refuses such an input before any conversion, but the analyzers still decide by it not to report the member. It is kept so that their diagnostics do not change; SHARED-BODY-001 now names the case itself instead of reaching it through FMT-STRUCT-007.

### RH5401-004 changed in place

RH5401's fix still formats the property and compares (`CanCollapse`), because that decides the cases the formatter keeps on several lines: a wrapped expression and a comment in front of the arrow. For a comment behind the arrow, the fix is now withheld because the formatter refuses the property, which the fix already asks (SHARED-FIX-010). The requirement names both reasons; the visible result does not change.

## Special cases

### Deleted

- `PropertyLayoutLineBreakRewriter.CollapseExpressionBody`: the refusal `LineBreakTriviaUtilities.WouldJoinAcrossUnjoinableTrivia(expressionBody.ArrowToken, expressionBody.Expression.GetFirstToken())`. It tests exactly the arrow's trailing trivia and the expression's first token's leading trivia for comments, directives, and disabled text — FMT-UNSUP-012's place — and the rewriter visits only property and indexer declarations, so every arrow clause it sees is whole inside its input (FMT-UNSUP-003).
- `ExpressionBodyToBlockConverter.CreateBlock` and its helpers: every case for trivia behind the arrow — the check of the arrow's trailing trivia in `ArrowLineHoldsComment`, the move of own-line comments out of the expression's leading trivia, the transfer of the arrow's trailing comments and directives onto the opening brace (`ExtractSignificantTrivia(arrowToken.TrailingTrivia)`), and in `CreateReturnStatement` the move of a comment in front of the expression behind the created `return`, the line break written behind `return` in front of a comment that leads the expression, `LeadsWithCommentOnItsLine`, and the `moveComments` and `lineBreak` parameters that only these cases use. The converter runs only on whole members, whose arrow clause the input holds.
- `GetOnlyMemberExpressionBodyTransform.HasLayoutOnlySeams`: the checks of the getter arrow's trailing trivia and of the leading trivia of the getter expression's first token. A getter written with an expression body brings that gap into the input; a getter converted from a block gets an arrow with no trivia and an expression without leading trivia (`AccessorExpressionBodyTransform.ConvertToExpressionBody`, `TryRehostExpression`).
- Spec text: FMT-STRUCT-006's placement rules for a comment between the arrow and the expression and for a comment directly behind the arrow, with their examples; FMT-STRUCT-007's clause for a directive between the arrow and the expression; FMT-STRUCT-015's seams behind the getter's arrow; the two examples of FMT-STRUCT-026 that start from a comment behind the arrow; RH5401-004's statement that a comment behind the arrow keeps the property on several lines.

### Checked and kept

- `ExpressionBodyRewriteUtilities.BlocksRewrite`, its check of a directive between the arrow and the expression included: the analyzers of RH3202–RH3208 decide by it not to report such a member (SHARED-BODY-001). Without it they would report the member, which changes their diagnostics.
- `PropertyLayoutLineBreakRewriter.CollapseExpressionBody`'s refusal of a join across a comment or directive in front of the arrow: that comment stays supported.
- `ExpressionBodyToBlockConverter`'s handling of the arrow's leading trivia — own-line comments in front of the arrow that lead the created statement, and the part of `ArrowLineHoldsComment` that finds a comment in front of the arrow on its line — and of the semicolon's trivia and the expression's trailing trivia: all of it serves comments outside the gap. The handling of leading trivia of the expression that holds a line break, a blank line between the arrow and the expression, is layout and stays.
- `GetOnlyMemberExpressionBodyTransform.HasLayoutOnlySeams`'s other seams, the arrow's leading trivia included, and `AccessorExpressionBodyTransform`: it creates an arrow with nothing behind it and checks the seams of the block it removes, not an existing arrow's gap.
- `RH5401ExpressionStyleGetOnlyPropertiesShouldBeSingleLinedCodeFixProvider.CanCollapse`: it decides the wrapped expression and the comment in front of the arrow.
- `ExpressionBodyCodeFixUtilities.ConvertBodyAsync`: it takes the formatter's result and holds no case for the gap.
- `ArrowSpacingRule`: it spaces every arrow alike and holds no case for a comment.
- The indentation phase: the column of an expression behind a comment at the arrow was no special case of its own, so there is nothing to delete and nothing to repair.
- RH5040's analyzer, RH5401's analyzer, and the analyzers of RH3202–RH3208: analyzers do not consult the precheck.

## Alternatives considered

- **Do not declare the shape; repair the layout behind the comment and stop RH5401 reporting the property** — rejected by the maintainer: it keeps every case of the conversion and adds a layout for the expression behind the comment, where declaring deletes them.
- **Comments only, without directives and disabled text** — rejected: the join refusal and the seam check of the get-only lift test directives alike, so they would stay.
- **Only a gap with a line break** — rejected: the conversion's transfer of a same-line comment onto the opening brace would stay, and the boundary would differ from FMT-UNSUP-008 and FMT-UNSUP-011.
- **Members and accessors only, as RH5040 reports, not local functions** — rejected: the conversion of a local function runs through the same code, which would keep every case of FMT-STRUCT-006 for it.
- **Also the arrow of a lambda and of a switch-expression arm** — rejected: nothing would be deleted for them.
- **Also a comment in front of the arrow** — not offered: the maintainer's decision names only the position behind the arrow, and a comment in front of it is placed by FMT-STRUCT-006 and the join refusal that stays.
- **Delete `BlocksRewrite`'s directive check and let RH3202–RH3208 report such a member, with no fix** — not chosen: it changes diagnostics the issue does not ask about; it remains possible as a change of its own.

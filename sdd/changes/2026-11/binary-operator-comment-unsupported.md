# A comment or directive behind a binary operator is unsupported

| | |
|---|---|
| Status | Draft |
| Date | 2026-11 |
| Affected requirements | FMT-UNSUP-011 (added), FMT-UNSUP-005 (changed: example), FMT-BREAK-017 (changed: the join never meets the shape), RH5302-005 (changed: the fix is withheld through the precheck, for every operator of the chain) |
| Depends on | [The formatter's Unsupported outcome](unsupported-precheck.md), [Declaring a shape unsupported is a refusal](unsupported-shape-as-refusal.md) |

## What

The formatter declares a new unsupported shape (FMT-UNSUP-011): a comment, a preprocessor directive, or disabled text between the operator of a binary expression and its right operand — in the operator's trailing trivia or in the leading trivia of the right operand's first token. It holds for every binary operator as SHARED-COMMENT-007 defines it (arithmetic, shift, relational, equality, bitwise, logical, null-coalescing, the `is` of a type test, and `as`), whether the gap holds a line break or not. Every entry point refuses an input that holds the shape and reports it with the reason `a comment or directive between a binary operator and its right operand`; the CLI leaves the file unchanged, reports it as an error, and exits with code 2; and every code fix whose formatted node holds it is withheld (SHARED-FIX-010), RH5302's among them (RH5302-005).

Not refused: a comment in front of the operator, behind the left operand or on the line above the operator's line; a comment inside the right operand, behind its first token; and the operators that are not binary operators — pattern combinators and relational patterns, the `is` of a pattern match, assignment and compound assignment operators, the `..` of a range, and a conditional's `?` and `:` (FMT-UNSUP-008).

The line-break phase loses its refusal to join a right operand onto the line of the operator that moves in front of it, and RH5302's fix loses its copy of that refusal (*Special cases*).

## Why

A comment or directive behind an operator kept the right operand off the operator's line. The line-break phase refused the join that FMT-BREAK-017 asks for (FMT-BREAK-001), left the operator at the end of its line, and the right operand started a line that no requirement places: it landed in the column of the enclosing statement. RH5302's fix carried a copy of the refusal so it would not offer a fix that changes nothing. RH5049 already reports a comment behind a binary operator, so the shape is rare by the repository's definition; a comment that explains an operand belongs in front of the operator, where the phases handle it as they do for every wrapped operator. The maintainer chose to declare the shape, the same decision as for the conditional operator (FMT-UNSUP-008).

## Visible output changes

**1. A comment behind a logical operator at the end of a line.** Input:

```csharp
class C
{
    bool M(bool a, bool b)
    {
        var x = a && // note
                b;

        return x;
    }
}
```

Before (`main`, `scripts/format.sh`):

```csharp
class C
{
    bool M(bool a, bool b)
    {
        var x = a && // note
        b;

        return x;
    }
}
```

After: the file is unchanged, and the CLI reports

```
error: <file>(5,22): unsupported syntax: a comment or directive between a binary operator and its right operand. The file was left unchanged.
```

**2. A comment behind an arithmetic operator.** Input:

```csharp
class C
{
    int M(int b)
    {
        var y = b + // c
            1;

        return y;
    }
}
```

Before (`main`):

```csharp
class C
{
    int M(int b)
    {
        var y = b + // c
        1;

        return y;
    }
}
```

After: the file is unchanged, and the CLI reports `error: <file>(5,21): unsupported syntax: a comment or directive between a binary operator and its right operand. The file was left unchanged.`

**3. A block comment behind an operator on one line.** The whole file is refused, also the code elsewhere in it that the formatter would correct. Input:

```csharp
class C{
    string M(int x, object o)
    {
        int z = x * /* scale */ 2;

        return o as /* text */ string ?? z.ToString();
    }
}
```

Before (`main`): only the brace of `class C{` moves to its own line; both comments stay.

```csharp
class C
{
    string M(int x, object o)
    {
        int z = x * /* scale */ 2;

        return o as /* text */ string ?? z.ToString();
    }
}
```

After: the file is unchanged, `class C{` included, and the CLI reports `error: <file>(4,21): unsupported syntax: a comment or directive between a binary operator and its right operand. The file was left unchanged.`

**4. A conditional operand behind a trailing operator.** Input:

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

Before (`main`):

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

After: the file is unchanged, in every build, and the CLI reports `error: <file>(8,1): unsupported syntax: a comment or directive between a binary operator and its right operand. The file was left unchanged.` Written with the operators in front of the operands, the same code is formatted:

```csharp
        return RegexOptions.Compiled
#if DEBUG
               | RegexOptions.IgnoreCase
#endif
               | RegexOptions.Multiline;
```

**5. RH5302's fix** (`RH5302 · Logical expressions should be formatted correctly`). The fix formats the outermost chain of `&&` and `||` (RH5302-004); it is now withheld whenever that chain holds the shape anywhere, not only behind a reported operator at the end of a line. The diagnostics stay.

Input — an operator at the start of a line, misaligned, with a comment behind it:

```csharp
        var x = a
            && /* c */ b;
```

Before (`main`): the fix is offered and gives

```csharp
        var x = a
                && /* c */ b;
```

After: no fix offered.

Input — another operator of the chain has a comment behind it:

```csharp
        if (a &&
            b && // c
            c)
```

Before (`main`): the fix is offered for the first `&&` and gives

```csharp
        if (a
            && b && // c
            c)
```

and no fix is offered for the second `&&`. After: no fix offered for either.

Input — an arithmetic operator inside an operand of the chain:

```csharp
        if (x + /* c */ 1 > 2 &&
            b)
```

Before (`main`): the fix is offered and gives

```csharp
        if (x + /* c */ 1 > 2
            && b)
```

After: no fix offered.

Unchanged: the fix for an operator at the end of a line with a comment directly behind it (`a && // note⏎b`) was withheld before and still is; a comment in front of the operator (`a /* note */ &&⏎b`) does not withhold the fix, and a comment behind an operator outside the chain, such as in another argument, does not either.

**6. Other code fixes that format a node holding the shape** are withheld (SHARED-FIX-010); their diagnostics stay. For example, RH3202 (`Expression-style methods should not be used`), input:

```csharp
class C
{
    int M(int a, int b) => a + // c
                           b;
}
```

Before (`main`): the fix is offered and gives

```csharp
class C
{
    int M(int a, int b)
    {
        return a + // c
        b;
    }
}
```

After: no fix offered. The same holds for every fix that formats through the formatter — among them RH5111, RH5101, RH5102, RH5308, RH5403, and RH7101 — wherever the node it formats holds such an operator.

The analyzers do not change: RH5302 still reports the operators above (SHARED-DIAG-010: a shape the formatter refuses as unsupported is not one it keeps), RH5049 still reports the comments, and analyzers do not consult the precheck.

No rule page changes: RH5302 and the other rules lose their fix only for the refused shape, which `documentation/rules/general-notes.md` covers ("A diagnostic without a code fix").

## Decisions

### Declare the shape, in a change of its own (maintainer, escalation 10)

The maintainer decided before this run to declare a comment or directive between a binary operator and its right operand unsupported, in a change of its own, with the same boundary as for the conditional operator: comments above the operator's line stay supported, only the position behind the operator is refused.

### Directives and disabled text belong to the shape

The join refusal in the line-break phase and its copy in RH5302's fix test the gap for comments, directives, and disabled text alike. A shape of comments alone would delete neither, so it would not be declared (`AGENTS.md`, *Special cases do not grow the source*). The cost is visible output change 4: a conditional operand written behind a trailing operator, which the formatter now refuses in every build; written with leading operators, as the formatter writes binary operators anyway (FMT-BREAK-017), the same code is formatted.

### Every binary operator, also on one line

The shape names every operator of a binary expression, so it is one rule with no list of operators, as the line-break phase joins every binary expression's right operand with one guard. A comment behind the operator with the operand on the same line (`n * /* scale */ 2`) is refused as well, as FMT-UNSUP-008 refuses `a ? /* c */ 1 : 2`: the boundary is a place in the syntax tree, and the maintainer's answer names the place, not a line break.

### Not the pattern operators or the assignments

The `is` of a pattern match, the pattern combinators `and` and `or`, the relational patterns, and the assignment operators are not binary operators (SHARED-COMMENT-007). The line-break phase lays them out under other requirements — the combinator moves to a new line and joins nothing (FMT-BREAK-017), the assignment joins under FMT-BREAK-016 — and no special case for a comment behind them would be deleted, so they are not declared.

### FMT-UNSUP-004 and FMT-UNSUP-010 keep their reason

A comment behind an operator in an attribute argument of a declaration is refused under FMT-UNSUP-004 already, and a directive behind an operator in a field declaration with several declarators under FMT-UNSUP-010; both start at the same comment or directive as the new shape, where FMT-UNSUP-001's first shape in the text does not decide. The reason stays the one these declarations report today, as FMT-UNSUP-009 kept FMT-UNSUP-004's reason for a comma between declarators. Inside an initializer value, FMT-UNSUP-004 excepts the comment, so the new reason applies there.

### RH5302-005 changed in place, not retired

The issue proposed deleting RH5302-005. The rule's visible behavior — reported, no fix offered — remains for the case it states and widens to the whole chain, so the requirement is rewritten as RH5308-005 was for FMT-UNSUP-008: the fix is withheld because the formatter refuses the chain, and it asks through FMT-UNSUP-007, with no check of its own.

## Special cases

### Deleted

- `BinaryOperatorLineBreakRewriter.NormalizeBinaryOperatorPosition`: the `LineBreakTriviaUtilities.WouldJoinAcrossUnjoinableTrivia(operatorToken, node.Right.GetFirstToken())` refusal. It tests exactly the operator's trailing trivia and the right operand's first token's leading trivia for comments, directives, and disabled text — FMT-UNSUP-011's place — and the rewriter visits only binary expressions inside its input, so every expression it sees holds its operator and its right operand, and a refused input never reaches it (FMT-UNSUP-003).
- `RH5302LogicalExpressionsShouldBeFormattedCorrectlyCodeFixProvider.RegisterCodeFixesCoreAsync`: the `IsTrailingOperator(binaryExpression) && SyntaxTriviaUtilities.WouldJoinAcrossUnjoinableTrivia(operatorToken, binaryExpression.Right.GetFirstToken())` guard, and `IsTrailingOperator`, which only that guard uses. The fix formats the outermost logical chain, which holds the reported binary expression, and it already asks `ReihitsuFormatter.FindUnsupportedSyntax` about that chain (SHARED-FIX-010), which now refuses every input the guard refused.
- Spec text: RH5302-005's statement that the fix itself refuses the join behind a reported operator at the end of a line, and its reference to FMT-BREAK-001.

### Checked and kept

- `LineBreakTriviaUtilities.WouldJoinAcrossUnjoinableTrivia` and `SyntaxTriviaUtilities.WouldJoinAcrossUnjoinableTrivia`: many other callers — parentheses, chains, assignments, accessors, lists, conditional operators' other gaps.
- `BinaryOperatorLineBreakRewriter.NormalizePatternOperatorPosition`: it moves a pattern combinator to a new line and joins nothing behind it, so it has no refusal to delete.
- The handling of a comment in front of the operator — `LineBreakTriviaUtilities.EndLineInFrontOf` in `NormalizeBinaryOperatorPosition`, which ends the left operand's line behind its trailing comment: that comment stays supported.
- `ListElementInteriorJoinRewriter.IsOperatorWrap`'s binary-operator case: it leaves the gaps around a binary operator to `BinaryOperatorLineBreakRewriter` whatever trivia they hold, a split of ownership, not a comment guard.
- `BinaryExpressionContributor`: it places operators that start a line and holds no case for a right operand that starts one.
- RH5049's analyzer and RH5302's analyzer: analyzers do not consult the precheck, and RH5302 keeps reporting the shape (SHARED-DIAG-010).
- The other code fixes: none has a guard of its own for a comment behind a binary operator; the formatter-delegating fixes are withheld for the shape by SHARED-FIX-010 without a change. RH3006 and RH6024 do not format through the formatter and are not affected.

## Alternatives considered

- **Do not declare the shape; stop RH5302 reporting it** (SHARED-DIAG-010) — rejected by the maintainer: declaring gives one rule for a comment behind an operator, and the formatter's layout of the right operand at the statement's column would stay.
- **Declare comments only** — not offered: directives reach the same guard, so the declaration would delete nothing.
- **Only a gap with a line break** — rejected: the maintainer's answer names the place behind the operator, as for the conditional operator; a same-line comment there is reported by RH5049 like any other.
- **Include the pattern combinators and the assignment operators** — rejected: nothing would be deleted for them, and their comment positions are governed by other requirements.

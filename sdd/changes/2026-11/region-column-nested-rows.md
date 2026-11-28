# A region directive inside a member is unsupported; a multi-line array initializer puts its braces on lines of their own

| | |
|---|---|
| Status | Draft |
| Date | 2026-11 |
| Affected requirements | FMT-UNSUP-023 (added), FMT-BREAK-046 (added), FMT-BREAK-021 (changed), FMT-INDENT-010 (changed), FMT-INDENT-023 (changed), FMT-UNSUP-022 (clarified), FMT-BLANK-009 (clarified), FMT-BLANK-013 (clarified), FMT-BLANK-018 (clarified), FMT-REGION-005 (clarified) |

## What

**Region directives inside a member.** The formatter refuses, as unsupported, every input that holds an active `#region` or `#endregion` directive inside a member: in its body, an accessor, a lambda, a switch section, a field or property initializer, a parameter list, the arguments of the member's attributes, between its attribute lists or modifiers, and in the header of a type (FMT-UNSUP-023). Supported are the positions between members: in front of the first token of a member (a nested type, a namespace, and each top-level statement included), of a using directive, an extern alias, or an `[assembly: …]` list; in front of the closing brace of a type, an enum, or a namespace; at the end of the file; and in front of the first token of a node-level input. Every entry point returns a refused input unchanged and reports it; the CLI leaves the file unchanged and reports it; every code fix whose formatted node holds such a directive is withheld (SHARED-FIX-010). FMT-INDENT-010 now states one column, the members' column.

**Array initializers.** Every array initializer — of `new T[…]`, `new[]`, `stackalloc`, of a declaration, and every row of a multi-dimensional array — is judged on its own: written on one line, it stays as written; spanning several lines, its opening brace and its closing brace stand on lines of their own (FMT-BREAK-046). The opening brace of a declaration's initializer stays on the line of the `=`; with `new` or `stackalloc` and for a row it starts its own line. The line breaks between the elements stay as written. A row is laid out like a complex element initializer: its elements one level deeper than its brace, and its closing brace under its opening brace (FMT-INDENT-023).

## Why

The issue (thoenissen/Reihitsu#1399) asked, after round 9 of the SDD open questions (thoenissen/Reihitsu#912, items 20 and 21), for a region inside an initializer, a collection expression, a switch expression, or an argument list to take the elements' column, and for the rows of a multi-dimensional array to be laid out like complex element initializers.

The maintainer answered this run's output questions differently from the issue (batches 1, 1b, 1c in the run's lead note):

- A region inside a member is declared unsupported in this run, instead of a column being decided for each construct. A region groups members. Inside a member it groups statements, elements, or arms that stand at aligned columns, where the right column for the directive is unclear. RH7303 reports a region inside executable code, so such input is rare. A nested type is not a member in this sense, so the regions between its members stay supported.
- Every array initializer is either single-line or multi-line, judged for each initializer when they are nested. A multi-line one puts its `{` and `}` on lines of their own. Behind the `=` of a declaration the `{` stays on the `=` line; with `new[]` or `new T[…]` it goes on its own line under `new`. Element line breaks inside are not decided by this rule.

The maintainer confirmed eight concrete outputs (lead note, *Confirmed examples*); the examples below and in FMT-BREAK-046 are those outputs.

The issue's description of today's rows is out of date. On the base, a row's elements already stand one level deeper than the row's line. What differs is the row's closing brace and the shapes `{ {`, `}, {`, and `} };`.

## Visible output changes

Each "before" is the output of `scripts/format.sh` (whole document, CLI) or `scripts/apply-fix.sh` on this branch's base, 4a91817, whose production code is that of 13b0ed2. A second run left every "before" unchanged. Each "after" is specified by this change and not built yet. Its unchanged lines match today's output.

### 1. A region in a method body

```csharp
class C
{
    void M(int x)
    {
        #region Setup
        var a = x;
        #endregion
    }
}
```

Before:

```csharp
class C
{
    void M(int x)
    {
        #region Setup

        var a = x;

        #endregion // Setup
    }
}
```

After: the input unchanged.

```text
error: <file>(5,9): unsupported syntax: a region directive inside a member. The file was left unchanged.
```

### 2. A region in a field's initializer

```csharp
class C
{
    int[] F = [
                  #region R
                  1,
                  #endregion
                  2
              ];
}
```

Before:

```csharp
class C
{
    int[] F = [

    #region R

                  1,

    #endregion // R

                  2
              ];
}
```

After: the input unchanged.

```text
error: <file>(4,19): unsupported syntax: a region directive inside a member. The file was left unchanged.
```

### 3. A region in a parameter list

```csharp
class C
{
    void M(int a,
           #region R
           int b)
           #endregion
    {
    }
}
```

Before:

```csharp
class C
{
    void M(int a,

    #region R

           int b)

    #endregion // R

    {
    }
}
```

After: the input unchanged.

```text
error: <file>(4,12): unsupported syntax: a region directive inside a member. The file was left unchanged.
```

### 4. A region in a member's attribute arguments

```csharp
class A : System.Attribute
{
    public A(int a, int b)
    {
    }
}

class C
{
    [A(1,
       #region At
       2)]
       #endregion
    void N()
    {
    }
}
```

Before:

```csharp
class C
{
    [A(1,

    #region At

       2)]

    #endregion // At

    void N()
    {
    }
}
```

After: the input unchanged.

```text
error: <file>(11,8): unsupported syntax: a region directive inside a member. The file was left unchanged.
```

### 5. Regions between members, also of a nested type: unchanged

```csharp
class C
{
    #region Types

    class D
    {
        #region Fields

        private int _a;

        #endregion // Fields
    }

    #endregion // Types
}
```

Before and after: unchanged, formatted as written.

### 6. Code fixes on a member that holds a region

A code fix whose formatted node holds a region directive inside a member is no longer offered; the diagnostic stays. Run through `scripts/apply-fix.sh` on the base:

- RH5305, on `int[] a = [⏎    #region R⏎    1,⏎    #endregion⏎    2⏎    ];` in a method body. Before, the fix was offered and wrote the directives at 18 (the column of `[`) and the elements at 22. After: no fix offered.
- RH5206, on `return x switch⏎{⏎#region R ... };` with the braces at 8. Before, the fix was offered and wrote the directives at 15 (the column of the braces) and the arms at 19. After: no fix offered.

Every other fix that formats such a node through the formatter is withheld in the same way: RH5101, RH5304, RH5306, RH5307, and the fixes that format a whole member or type. Those were not run. Fixes that edit region trivia without the formatter (RH7301, RH7302, RH7303, RH7304, RH7306, RH7310, RH5031, RH5032) are not affected.

### 7. A multi-line array initializer behind `=`, with a multi-line and a single-line row

```csharp
class C
{
    void M()
    {
        int[,] b = { {
        1, 2
        }, { 3, 4 } };
    }
}
```

Before:

```csharp
        int[,] b = { {
            1, 2
            }, { 3, 4 } };
```

After:

```csharp
        int[,] b = {
                       {
                           1, 2
                       },
                       { 3, 4 }
                   };
```

### 8. `new[]` spanning several lines

```csharp
        var e = new[] {
        1, 2
        };
```

Before:

```csharp
        var e = new[] {
                    1, 2
                };
```

After:

```csharp
        var e = new[]
                {
                    1, 2
                };
```

### 9. `new int[,]` with a single-line and a multi-line row

```csharp
        var f = new int[,] { { 1, 2 }, {
        3, 4
        } };
```

Before:

```csharp
        var f = new int[,] { { 1, 2 }, {
            3, 4
            } };
```

After:

```csharp
        var f = new int[,]
                {
                    { 1, 2 },
                    {
                        3, 4
                    }
                };
```

### 10. A row whose element list wraps

```csharp
        int[,] g = { { 1,
        2 }, { 3, 4 } };
```

Before:

```csharp
        int[,] g = { { 1,
            2 }, { 3, 4 } };
```

After: the element line break stays as written.

```csharp
        int[,] g = {
                       {
                           1,
                           2
                       },
                       { 3, 4 }
                   };
```

### 11. A field's array initializer

```csharp
class C
{
    int[] F = { 1,
    2 };
}
```

Before:

```csharp
    int[] F = { 1,
                  2 };
```

After:

```csharp
    int[] F = {
                  1,
                  2
              };
```

### 12. Rows already on lines of their own

```csharp
        int[,] a = {
                       {
                       1, 2
                       },
                       {
                       3, 4
                       }
                   };
```

Before:

```csharp
        int[,] a = {
                       {
                           1, 2
                           },
                       {
                           3, 4
                           }
                   };
```

After: the closing brace of each row under its opening brace.

```csharp
        int[,] a = {
                       {
                           1, 2
                       },
                       {
                           3, 4
                       }
                   };
```

### 13. Single-line array initializers: unchanged

```csharp
        int[,] a = { { 1, 2 }, { 3, 4 } };
        var h = new[] { 1, 2 };
```

Before and after: unchanged.

### Code-fix results for array initializers

The code fixes that format a node holding a multi-line array initializer give the layout of entries 7–12, measured from where the node stands: RH5101 and the fixes that format a whole member or type. Those were not run. RH5410 (trailing comma in an array initializer) edits the comma without the formatter. On `new int[] { 1,⏎2, }` it removes the comma and leaves the braces as written, before and after the change.

## Special cases

### Deleted

- **The column of a region directive inside a construct.** The run's earlier draft of FMT-INDENT-010 needed this. Inside an initializer, an anonymous object, a collection expression, a pattern, a switch expression, or a wrapped list, the directive would take the elements' column: a list of constructs in the indentation phase, beside the comment rule of FMT-INDENT-006, and a column of its own in front of each closing delimiter. The declaration removes it from the spec and keeps it out of the code.
- **Pairing a switch section's `#endregion` with its `#region`.** An `#endregion` after a section's last statement stands four columns left of a `#region` among the statements. Giving it the statement column needs the matching `#region`'s position, which the indentation phase does not have. The declaration removes the case.
- **FMT-INDENT-010's statement column and label column.** These are no longer stated for whole documents. They remain only for the region directive in the leading trivia of a node-level input's first token.
- **The in-member clauses of the blank-line and region specs.** These are FMT-BLANK-009's sentence on a region directive ending the gap in front of a closing bracket or parenthesis, FMT-BLANK-013's switch-label example, and FMT-REGION-005's region straddling a member body. These positions are refused now. The examples are moved between members where the requirement's point survives there.
- **The RH7304 ping-pong at member level.** For a region in a field's collection expression or object initializer, the CLI wrote the member column and RH7304's fix wrote the column of the region's first token. The CLI now refuses such a file and leaves it unchanged.

### Checked and kept

- `LayoutComputer.SetDirectiveIndentation` and `SyntaxIndentationUtilities.GetTriviaIndentLevel` are kept. They place the directives between members at every depth, and the one in front of a node-level input's first token. The switch-section interval of `GetIndentingScopeRange` serves statements as well.
- `IndentationRewriter`'s split between region and non-region directives is kept: non-region directives still go to column 0 (FMT-INDENT-009).
- `BlankLineRegionDirectiveRewriter`, `BlankLineEditor.EnsureBlankLineBeforeRegionDirectives`/`IsMissingRequiredBlankLineBeforeRegionDirective`, `BlankLineTriviaBoundaryRewriter`'s `#endregion` check, and `BlankLineTokenCleanupRewriter.RemoveBlankLinesAfterTrailingEndRegion` are kept. They serve the regions between members and in front of a type's closing brace, through one path that serves the in-member positions as well. None of them holds a branch for a block's brace, a bracket, or a parenthesis that could go.
- The region clause of `ExpressionBodyRewriteUtilities.BlocksRewrite` is kept. So is the region clause of `GetOnlyMemberExpressionBodyRewriteUtilities.ContainsBlockingDirective`. Both are unreachable from the formatter after the declaration, but both predicates are shared with the analyzers RH3202, RH3203, the expression-body analyzer base, and the get-only-member analyzer and code fix. The analyzers decide their report with them independently of the precheck. A formatter-only copy without the clause would be a second statement of the predicate.
- `SwitchCaseBraceRewriter.SectionCarriesDirectives` is kept: it refuses every directive, and conditional directives still reach it.
- `SyntaxTriviaUtilities.ContainsUnbalancedRegionDirectives` in `OrderingMoveSafety` is kept: it guards member moves across regions between members, which stay supported.
- `RegionNamingRewriter` is kept: it names regions in disabled text inside members, which this shape does not refuse.

## Decisions

### The shape is the positions that are not between members

The requirement lists what is supported — the first token of a member, a using, an extern alias, an `[assembly: …]` list, a type's or namespace's closing brace, the end of the file, the input's first token — and refuses every other region directive. Each exception is read from the token and the node it begins or closes. That node lies in the input whenever the token is not the input's first one, so the check is local (FMT-UNSUP-003). A node-level input below a member, such as a collection expression a code fix formats, refuses a region directive inside it, so the fix is withheld.

### The input's first token is excepted

A region directive above a statement or a member that a code fix formats stands in the leading trivia of the fix's first token. Refusing it would withhold fixes on common code (`#region Methods` above an attributed method). It keeps the column it gets today.

### The header of a type is not between members

A region directive in a base list, a primary-constructor parameter list, or in front of a type's `{` groups no members. It is refused on the same terms as one inside a member (handover output question 1).

### Each top-level statement is a member

A region between top-level statements stands at column 0, like a region between types, and is supported. One inside a top-level statement's block is refused (handover output question 2).

### An array initializer is judged by every line break in its text

An initializer spans several lines when any line break stands between its braces: in trivia, a `//` comment's end included, or inside a token such as a verbatim string. This needs no other definition (handover output question 4).

## Alternatives considered

- **The elements' column for a region inside a construct (the issue's request, round 9 option b).** The maintainer replaced it with the declaration: it needs a list of constructs and a column for each closing delimiter.
- **The statement column, as today (round 9 option a).** Rejected by round 9: the directive stands far left of what it groups.
- **Declaring only the constructs that are not block scopes.** This would refuse regions in initializers, lists, and patterns, and keep regions in blocks and switch sections. The maintainer chose the whole member (batch 1b, 1A).
- **Rows only.** The maintainer chose every array initializer for the brace rule (batch 1b, 3A).
- **The opening brace on its own line also behind `=`.** Rejected in batch 1c: behind the `=` of a declaration the brace stays on the `=` line, and RH5111 reports a value that starts on a later line than the `=`.

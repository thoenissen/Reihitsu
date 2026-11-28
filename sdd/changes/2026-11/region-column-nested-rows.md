# A region directive inside a member is unsupported; a multi-line array initializer puts its braces on lines of their own

| | |
|---|---|
| Status | Accepted |
| Date | 2026-11 |
| Affected requirements | FMT-UNSUP-023 (added), FMT-BREAK-046 (added), FMT-BREAK-021 (changed), FMT-INDENT-010 (changed), FMT-INDENT-023 (changed), FMT-UNSUP-022 (clarified), FMT-BLANK-009 (clarified), FMT-BLANK-013 (clarified), FMT-BLANK-018 (clarified), FMT-REGION-005 (clarified) |

## What

**Region directives inside a member.** The formatter refuses, as unsupported, every input that holds an active `#region` or `#endregion` directive inside a member: in its body, an accessor, a lambda, a switch section, a field or property initializer, a parameter list, the arguments of the member's attributes, between its attribute lists or modifiers, and in the header of a type or of an extension block (FMT-UNSUP-023). Supported are the positions between members: in front of the first token of a member (a nested type, an extension block, and a namespace included), of a using directive, an extern alias, or an `[assembly: …]` list; in front of the closing brace of a type, an extension block, an enum, or a namespace; at the end of the file; and in front of the first token of a node-level input. A file with top-level statements is refused for every region directive in it, also one between the statements. Every entry point returns a refused input unchanged and reports it; the CLI leaves the file unchanged and reports it; every code fix whose formatted node holds such a directive is withheld (SHARED-FIX-010). FMT-INDENT-010 now states one column, the members' column.

**Array initializers.** Every array initializer — of `new T[…]`, `new[]`, `stackalloc T[]`, `stackalloc[]`, of a declaration, and every row of a multi-dimensional array — is judged on its own, on the lines the formatter's other line-break rules leave inside it: on one line, it stays as it is; spanning several lines, its opening brace and its closing brace stand on lines of their own (FMT-BREAK-046). A line break inside a string literal counts. The opening brace of a declaration's initializer stays on the line of the `=`; with `new` or `stackalloc` and for a row it starts its own line, also behind a comment, which keeps its line while the brace moves below it. The line breaks between the elements stay as written. A row is laid out like a complex element initializer: its elements one level deeper than its brace, and its closing brace under its opening brace (FMT-INDENT-023).

## Why

The issue (thoenissen/Reihitsu#1399) asked, after round 9 of the SDD open questions (thoenissen/Reihitsu#912, items 20 and 21), for a region inside an initializer, a collection expression, a switch expression, or an argument list to take the elements' column, and for the rows of a multi-dimensional array to be laid out like complex element initializers.

The maintainer answered this run's output questions differently from the issue (batches 1, 1b, 1c in the run's lead note):

- A region inside a member is declared unsupported in this run, instead of a column being decided for each construct. A region groups members. Inside a member it groups statements, elements, or arms that stand at aligned columns, where the right column for the directive is unclear. RH7303 reports a region inside executable code, so such input is rare. A nested type is not a member in this sense, so the regions between its members stay supported.
- Every array initializer is either single-line or multi-line, judged for each initializer when they are nested. A multi-line one puts its `{` and `}` on lines of their own. Behind the `=` of a declaration the `{` stays on the `=` line; with `new[]` or `new T[…]` it goes on its own line under `new`. Element line breaks inside are not decided by this rule.
- Batch 2 settled the cases the first answers left open: a region in the header of a type is refused; in a file with top-level statements every region is refused, because all top-level statements count as one member; a comment in front of a multi-line row's `{` keeps its line and the `{` moves below it; a line break inside a string literal makes an initializer multi-line; `stackalloc T[] { … }` and `stackalloc[] { … }` follow the `new` rule; an initializer is judged on the lines the other line-break rules leave; and an extension block is treated like a nested type.

The maintainer confirmed eight concrete outputs (lead note, *Confirmed examples*); the examples below and in FMT-BREAK-046 are those outputs.

The issue's description of today's rows is out of date. On the base, a row's elements already stand one level deeper than the row's line. What differs is the row's closing brace and the shapes `{ {`, `}, {`, and `} };`.

## Visible output changes

Each "before" is the output of `scripts/format.sh` (whole document, CLI) or `scripts/apply-fix.sh` on this branch's base, 4a91817, whose production code is that of 13b0ed2; entries 14–20 were run on c64be37, whose production code is the same. A second run left every "before" unchanged. Each "after" is specified by this change and not built yet. Its unchanged lines match today's output.

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

### 14. A region in the header of a type

```csharp
interface I
{
}

class B
{
}

class C : B,
          #region Interfaces
          I
          #endregion
{
}

record R(int A,
         #region More
         int B);
         #endregion
```

Before:

```csharp
interface I;

class B;

class C : B,

#region Interfaces

          I

#endregion // Interfaces

{
}

record R(int A,

#region More

         int B);

#endregion // More
```

After: the input unchanged.

```text
error: <file>(10,11): unsupported syntax: a region directive inside a member. The file was left unchanged.
```

### 15. Regions in a file with top-level statements

```csharp
using System;

#region Setup
var a = 1;
#endregion

Console.WriteLine(a);

class D
{
    #region Fields
    private int _b;
    #endregion
}
```

Before:

```csharp
using System;

#region Setup

var a = 1;

#endregion // Setup

Console.WriteLine(a);

class D
{
    #region Fields

    private int _b;

    #endregion // Fields
}
```

After: the input unchanged.

```text
error: <file>(3,1): unsupported syntax: a region directive inside a member. The file was left unchanged.
```

Without the regions around `var a = 1;`, the file is refused at `#region Fields` all the same. A code fix that formats the class `D` alone still formats it, with its regions (FMT-UNSUP-003).

### 16. Regions between the members of an extension block: unchanged

```csharp
static class Ext
{
    extension(int x)
    {
        #region Members
        public int Twice() => x * 2;
        #endregion
    }
}
```

Before and after:

```csharp
static class Ext
{
    extension(int x)
    {
        #region Members

        public int Twice()
        {
            return x * 2;
        }

        #endregion // Members
    }
}
```

### 17. A comment in front of a multi-line row's opening brace

```csharp
class C
{
    int[,] F = {
                   /* c */ { 3,
                   4 }
               };
}
```

Before:

```csharp
    int[,] F = {
                   /* c */ { 3,
                       4 }
               };
```

After: the comment keeps its line, and the brace moves below it.

```csharp
    int[,] F = {
                   /* c */
                   {
                       3,
                       4
                   }
               };
```

### 18. An array initializer whose only line break is inside a string literal

```csharp
class C
{
    int[] F = { @"a
b".Length, 2 };
}
```

Before: unchanged.

After:

```csharp
class C
{
    int[] F = {
                  @"a
b".Length, 2
              };
}
```

### 19. `stackalloc` spanning several lines

```csharp
class C
{
    void M()
    {
        System.Span<int> s = stackalloc int[] { 1,
        2 };
        System.Span<int> t = stackalloc[] { 1,
        2 };
    }
}
```

Before:

```csharp
        System.Span<int> s = stackalloc int[] { 1,
                                 2 };
        System.Span<int> t = stackalloc [] { 1,
                                 2 };
```

After:

```csharp
        System.Span<int> s = stackalloc int[]
                             {
                                 1,
                                 2
                             };
        System.Span<int> t = stackalloc []
                             {
                                 1,
                                 2
                             };
```

### 20. An array initializer the other line-break rules make multi-line or single-line

```csharp
class X
{
    public int A { get; set; }
}

class C
{
    void M(int y)
    {
        var x = new[] { new X { A = 1 } };
        var c = new[] { (int)
            y };
    }
}
```

Before:

```csharp
        var x = new[] { new X
                        {
                            A = 1
                        } };
        var c = new[] { (int)y };
```

After: the object initializer is laid out over several lines, so the array initializer is multi-line; the cast is joined, so the other one is on one line.

```csharp
        var x = new[]
                {
                    new X
                    {
                        A = 1
                    }
                };
        var c = new[] { (int)y };
```

### Code-fix results for array initializers

The code fixes that format a node holding a multi-line array initializer give the layout of entries 7–12 and 17–20, measured from where the node stands: RH5101 and the fixes that format a whole member or type. Those were not run. RH5410 (trailing comma in an array initializer) edits the comma without the formatter. On `new int[] { 1,⏎2, }` it removes the comma and leaves the braces as written, before and after the change.

## Special cases

### Deleted

- **The column of a region directive inside a construct.** The run's earlier draft of FMT-INDENT-010 needed this. Inside an initializer, an anonymous object, a collection expression, a pattern, a switch expression, or a wrapped list, the directive would take the elements' column: a list of constructs in the indentation phase, beside the comment rule of FMT-INDENT-006, and a column of its own in front of each closing delimiter. The declaration removes it from the spec and keeps it out of the code.
- **Pairing a switch section's `#endregion` with its `#region`.** An `#endregion` after a section's last statement stands four columns left of a `#region` among the statements. Giving it the statement column needs the matching `#region`'s position, which the indentation phase does not have. The declaration removes the case.
- **FMT-INDENT-010's statement column and label column.** These are no longer stated. The one region directive inside a member that the phases still receive, in the leading trivia of a node-level input's first token, is the target's leading trivia, which the node-in-document entry points write back as it stands (FMT-PIPE-018); FMT-INDENT-010 gives it no column.
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

The requirement lists what is supported — the first token of a member, a using, an extern alias, an `[assembly: …]` list, a type's, an extension block's, an enum's, or a namespace's closing brace, the end of the file, the input's first token — and refuses every other region directive, and every region directive of a whole file with top-level statements. Each exception is read from the token and the node it begins or closes. That node lies in the input whenever the token is not the input's first one, so the check is local (FMT-UNSUP-003). A node-level input below a member, such as a collection expression a code fix formats, refuses a region directive inside it, so the fix is withheld.

### The input's first token is excepted

A region directive above a statement or a member that a code fix formats stands in the leading trivia of the fix's first token. Refusing it would withhold fixes on common code (`#region Methods` above an attributed method). The node-in-document entry points write it back as it stands in the document (FMT-PIPE-018).

### A region in the header of a type or an extension block is refused

Batch 2, answer 1A, and answer 7A for an extension block's header. A region directive in a base list, a primary-constructor parameter list, `extension(…)`, or in front of the opening brace groups no members.

### Every region of a file with top-level statements is refused

Batch 2, answer 2B, confirmed by a follow-up answer: all top-level statements of a file count as one member, the body of the entry point, so every region directive in such a file is refused, also one between the statements, around the usings, or between the members of a type the file declares. The check reads only whether the whole file holds a top-level statement. A node-level input is never a whole file: a type declared in such a file, formatted by a code fix, is decided by its own members (FMT-UNSUP-003), so the fix stays offered while the CLI refuses the file.

### An extension block is treated like a nested type

Batch 2, answer 7A. The regions between an extension block's members, and in front of its closing brace, stay supported at every depth.

### A comment in front of a row's opening brace keeps its line

Batch 2, answer 3B. The brace moves onto the line below the comment, and the row is laid out from that line. FMT-BREAK-046 states this for every opening brace that starts its line, with `new` and `stackalloc` as for a row, and for a comment behind the opening brace or behind the last element: a comment never changes its line, and the brace or the element on its other side moves (handover 05, output question 1).

### An array initializer is judged on the lines the other line-break rules leave

Batch 2, answer 6A. An object initializer, a block lambda, or a switch expression that the formatter lays out over several lines makes the array initializer around it multi-line; a join such as a cast's makes a wrapped one single-line. Judging the lines as written would move the braces again on the second pass. Batch 2, answer 4A: a line break counts wherever it stands between the braces, also inside a verbatim or raw string.

### `stackalloc` follows the `new` rule

Batch 2, answer 5A: `stackalloc T[] { … }` and `stackalloc[] { … }` put a multi-line initializer's opening brace on its own line under `stackalloc`.

## Alternatives considered

- **The elements' column for a region inside a construct (the issue's request, round 9 option b).** The maintainer replaced it with the declaration: it needs a list of constructs and a column for each closing delimiter.
- **The statement column, as today (round 9 option a).** Rejected by round 9: the directive stands far left of what it groups.
- **Declaring only the constructs that are not block scopes.** This would refuse regions in initializers, lists, and patterns, and keep regions in blocks and switch sections. The maintainer chose the whole member (batch 1b, 1A).
- **Rows only.** The maintainer chose every array initializer for the brace rule (batch 1b, 3A).
- **Only the regions inside a top-level statement refused (batch 2, 2A).** Rejected: the maintainer counts all top-level statements as one member.
- **A comment in front of a row's opening brace keeping the brace behind it (batch 2, 3A).** Rejected: every brace of a multi-line initializer starts its line.
- **Judging an initializer on the lines as written, keeping it on one line by suppressing the other line-break rules inside it (batch 2, 6B).** Rejected: an object initializer inside an array would no longer be laid out over several lines, and a wrapped cast would get braces on lines of their own.
- **The regions of an extension block refused (batch 2, 7B).** Rejected: an extension block holds members as a nested type does.
- **The opening brace on its own line also behind `=`.** Rejected in batch 1c: behind the `=` of a declaration the brace stays on the `=` line, and RH5111 reports a value that starts on a later line than the `=`.

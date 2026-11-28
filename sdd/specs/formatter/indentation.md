# Formatter — Indentation

| | |
|---|---|
| ID prefix | FMT-INDENT |
| Status | Active |
| Code owners | `IndentationPhase`, `LayoutComputer`, `LayoutModel`, `TokenLayout`, `IndentationRewriter`, `ListElementAligner`, `SyntaxIndentationUtilities`, `ArgumentAlignmentContributor`, `AngleBracketListAlignmentContributor`, `AnonymousObjectContributor`, `BaseTypeListContributor`, `BinaryExpressionContributor`, `CaseWhenClauseContributor`, `CollectionExpressionContributor`, `CommentIndentationContributor`, `ConditionalExpressionContributor`, `ConstructorInitializerContributor`, `ContinuationLineContributor`, `GenericConstraintContributor`, `LambdaAlignmentContributor`, `ListPatternContributor`, `MethodChainAlignmentContributor`, `ObjectInitializerContributor`, `ParenthesizedPatternContributor`, `PatternAnchor`, `QueryClauseContributor`, `RecursivePatternContributor`, `SwitchExpressionContributor` |
| Uses | FMT-BLANK-011, FMT-BREAK-001, FMT-BREAK-002, FMT-BREAK-006, FMT-BREAK-014, FMT-BREAK-016, FMT-BREAK-017, FMT-BREAK-022, FMT-BREAK-037, FMT-BREAK-038, FMT-BREAK-041, FMT-BREAK-042, FMT-BREAK-043, FMT-BREAK-044, FMT-DOC-003, FMT-DOC-012, FMT-PIPE-001, FMT-PIPE-002, FMT-PIPE-004, FMT-PIPE-014, FMT-PIPE-015, FMT-PIPE-023, FMT-PIPE-024, FMT-RAW-001, FMT-RAW-005, FMT-RAW-007, FMT-RAW-010, FMT-SWITCH-001, FMT-UNSUP-004, FMT-UNSUP-009, FMT-UNSUP-011, FMT-UNSUP-017, FMT-UNSUP-018, FMT-UNSUP-020, FMT-UNSUP-021, SHARED-BLANK-002, SHARED-BLANK-008, SHARED-CHAIN-007 |

This spec covers the indentation phase: the column at which every line that starts with a token begins — block indentation by scope depth, the alignment of wrapped constructs to an anchor token, and the re-indentation of comments and directives that start a line. Line breaks are already decided when this phase runs (FMT-PIPE-001); it only changes the whitespace in front of the first token or trivia of a line. The same phase moves the lines of multi-line raw string literals as FMT-RAW specifies, so the column it gives a line is final (FMT-PIPE-015).

## Non-goals

- Where lines break — FMT-BREAK. This phase never adds or removes a line break.
- Spacing between tokens on the same line — FMT-SPACE. The rest of a line keeps its spacing and moves with its first token.
- Blank lines — FMT-BLANK.
- How the content lines and the closing marker of a multi-line raw string literal, and the lines inside its interpolation holes that no alignment inside the hole places, are moved — FMT-RAW, which specifies that part of this phase.
- The text of documentation comments other than the column of their `///` markers — FMT-DOC.
- The column of a comma that starts a line between the arms of a switch expression. In initializers, collection expressions, list patterns, property patterns, positional patterns, and base lists no line starts with a comma: FMT-BREAK-038 moves every such comma behind the element in front of it.
- Configuration. The indentation unit is fixed; no option or `.editorconfig` setting is read.

## Requirements

### FMT-INDENT-001 — Four spaces per level, spaces only

The formatter MUST write every indentation it sets as space characters, one indentation unit being four spaces, and MUST replace tabs or any other whitespace in front of a line's first token by that indentation.

Why: One fixed unit makes every file look the same regardless of the editor that wrote it. Columns are counted in characters, so a tab would make every alignment below depend on the reader's tab width.

Example — the input indents each level with one tab character instead of four spaces (not shown faithfully in Markdown); the output is:

```csharp
class C
{
    void M()
    {
        var x = 1;
    }
}
```

### FMT-INDENT-002 — Block indentation follows the scope depth

The formatter MUST indent the first token of every line by one level per enclosing scope: the braces of a block-scoped namespace, a type, a block, an accessor list, and a switch statement each add one level for what lies between them, and the braces themselves stay at their owner's level. A file-scoped namespace and top-level statements MUST NOT add a level. A labeled statement MUST be indented like any other statement. The first token of a line is the token that starts it as SHARED-BLANK-008 decides, also when the line break in front of it is the one that ends a single-line documentation comment (`///`) written behind code on the line above; the lines of that comment are its text and MUST keep their columns (FMT-DOC-003). A continuation line takes the column FMT-INDENT-034 gives it instead, unless another requirement of this spec places it.

Why: Depth by scope is the convention every C# reader expects, and computing it from the syntax tree rather than from the existing whitespace makes the result independent of how badly the input was indented. A label is a statement in the syntax tree and gets no separate outdent rule. A line behind a `///` comment written behind code is a line like any other; it gets the column it would get behind a `//` comment, rather than whatever column the author left it in.

Example:

```csharp
namespace N
{
class C
{
      int _value;

  void M()
  {
          {
    M();
          }
  }
}
}
```

```csharp
namespace N
{
    class C
    {
        int _value;

        void M()
        {
            {
                M();
            }
        }
    }
}
```

A file-scoped namespace adds no level:

```csharp
namespace N;

    class C
    {
        void M()
        {
        }
    }
```

```csharp
namespace N;

class C
{
    void M()
    {
    }
}
```

and a label sits at the statement column:

```csharp
class C
{
    void M(int x)
    {
    retry:
        x--;

        if (x > 0)
        {
            goto retry;
        }
    }
}
```

```csharp
class C
{
    void M(int x)
    {
        retry:
        x--;

        if (x > 0)
        {
            goto retry;
        }
    }
}
```

A closing brace behind a `///` comment written behind code starts its line and is indented:

```csharp
class C
{
    int A; /// <summary>end</summary>
          }
```

```csharp
class C
{
    int A; /// <summary>end</summary>
}
```

while the continuation line of such a comment is part of the comment's text and keeps its column:

```csharp
class C
{
    int A; ///<remarks>a
        /// b</remarks>
}
```

is left unchanged.

### FMT-INDENT-003 — Switch sections

The formatter MUST indent the labels of a switch statement one level deeper than the `switch` keyword and the statements of each section one level deeper than its labels. A block that forms a section's body (FMT-SWITCH-001) is a statement of the section and MUST be indented like one.

Why: Labels and statements are visually distinct levels; aligning statements with their labels would hide where one section ends and the next begins.

Example:

```csharp
class C
{
    void M(int x)
    {
        switch (x)
        {
        case 1:
        M(2);
        break;
        }
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
                M(2);
                break;
        }
    }
}
```

### FMT-INDENT-004 — Unbraced embedded statements get one level

The formatter MUST indent the unbraced embedded statement of `if`, `else`, `while`, `do`, `for`, `foreach`, `using`, `lock`, and `fixed` one level deeper than its owner. An `else if` MUST stay at the level of its `if`.

Why: Without braces the indentation is the only visual sign of what the statement controls. FMT-STRUCT braces `if` and `else` bodies, so in the formatter's output this mostly concerns the loop and resource statements.

Example:

```csharp
class C
{
    void M(int x)
    {
        while (x < 10)
        x++;
    }
}
```

```csharp
class C
{
    void M(int x)
    {
        while (x < 10)
            x++;
    }
}
```

### FMT-INDENT-005 — Only the start of a line moves

The formatter MUST change only the whitespace in front of the first token or trivia of a line. The rest of the line, including a trailing comment, MUST keep its spacing and move with the line's first token.

Why: The phase owns columns of line starts; spacing within a line is FMT-SPACE's decision. Moving a line as a whole keeps a trailing comment attached to the code it describes.

Example:

```csharp
class C
{
    void M()
    {
            var x = 1; // one
    }
}
```

```csharp
class C
{
    void M()
    {
        var x = 1; // one
    }
}
```

### FMT-INDENT-006 — A comment on its own line takes the column of the code that follows it

The formatter MUST indent a comment that starts its line — as decided by SHARED-BLANK-002 — to the column of the token it precedes. When that token stands behind another comment that leads the token's line, the line starts with that comment: the comment on its own line above takes the column the token's line starts in, which is the column of the leading comment, not the column of the token behind it (FMT-INDENT-030): in a method body, `  // above` above `            /* c */ M();` gives `        // above` above `        /* c */ M();`. A comment that precedes a closing brace MUST be indented one level deeper than the brace, inside the scope it closes — except the closing brace of an interpolation (`InterpolationSyntax`) of a regular, verbatim, or single-line raw interpolated string, which closes no content one level deeper: the lines of its hole and the brace itself are continuation lines of the statement (FMT-INDENT-034), so a comment that precedes that brace MUST be indented to the column of the brace, also when the hole's content stands in another column, such as an operand aligned under the one above it. Which string the interpolation belongs to decides, not where that string stands: the closing brace of an interpolation of a regular string nested in a hole of a multi-line raw string literal takes the comment to its own column as well, while the closing brace of an interpolation of a multi-line raw string literal keeps the comment one level deeper; a comment that precedes the closing delimiter of a collection expression, a list pattern, or a positional pattern MUST be indented one level deeper than the delimiter, in the column of the construct's elements (FMT-INDENT-025), and a comment that precedes the closing parenthesis of a parenthesized pattern MUST be indented to the column of the pattern inside it, four columns right of the opening parenthesis (FMT-INDENT-026). Each of these closing delimiters closes content that this spec indents one level deeper than the delimiter, as the closing brace of an initializer does (FMT-PIPE-014). A comment that precedes the closing delimiter of any other construct, such as the closing parenthesis of an argument list, is not decided by this sentence and takes the column of that delimiter. A comment that does not start its line, such as a delimited documentation comment written behind an opening brace, MUST be left where it is and MUST NOT receive indentation in the middle of its line.

Why: A comment on its own line describes the code below it and reads as part of that code's block. A comment at the end of a scope describes the scope's contents, not the brace. The hole of an interpolation that is not part of a multi-line raw string literal holds no content one level deeper than its closing brace, so a comment there one level deeper would stand apart from the lines around it; the comment takes the column of the brace it precedes, as in front of the closing delimiter of any construct whose content is not one level deeper. Indenting a comment that shares a line with code would insert whitespace into the middle of that line.

Example:

```csharp
class C
{
    void M()
    {
            // before statement
        var x = 1;

  // before brace
    }
}
```

```csharp
class C
{
    void M()
    {
        // before statement
        var x = 1;

        // before brace
    }
}
```

and a comment in front of the closing bracket of a collection expression is indented as one in front of the closing brace of a collection initializer is:

```csharp
using System.Collections.Generic;

class C
{
    void M()
    {
        int[] a = [
                      1,

                  // last
                  ];
        var l = new List<int>
                {
                    1

                // last
                };
    }
}
```

```csharp
using System.Collections.Generic;

class C
{
    void M()
    {
        int[] a = [
                      1,

                      // last
                  ];
        var l = new List<int>
                {
                    1

                    // last
                };
    }
}
```

and a comment in front of the closing delimiter of a list pattern, a positional pattern, or a parenthesized pattern is indented into the column of the content it closes:

```csharp
class P
{
    public void Deconstruct(out int a, out int b)
    {
        a = 0;
        b = 0;
    }
}

class C
{
    bool M(int[] a, object o)
    {
        var b = a is [
                         1,

                     // last
                     ];
        var c = o is P(
                          1,
                          2

                      // last
                      );
        var d = o is
                  (
                      1
                      or 2

                  // last
                  );

        return b && c && d;
    }
}
```

```csharp
class P
{
    public void Deconstruct(out int a, out int b)
    {
        a = 0;
        b = 0;
    }
}

class C
{
    bool M(int[] a, object o)
    {
        var b = a is [
                         1,

                         // last
                     ];
        var c = o is P(
                          1,
                          2

                          // last
                      );
        var d = o is
                  (
                      1
                      or 2

                      // last
                  );

        return b && c && d;
    }
}
```

and a comment in front of the closing brace of an interpolation of a regular, a verbatim, or a single-line raw interpolated string, also one nested in a hole of a multi-line raw string literal, takes the column of the brace, also when the hole's content stands in another column, while one in front of the closing brace of a multi-line raw string literal's own interpolation stays one level deeper than the brace. FMT-BLANK-011 puts a blank line above each of these comments:

```csharp
class C
{
    string M(int a, int b)
    {
        var t = $"{
            // e
            a
            // f
            }";
        var u = $"{a +
            b
            // f
            }";
        var v = $@"{
            a
        // f
            }";
        var s = $"""{
            a
            // f
            }""";
        var n = $"""
            {$"{
                a
                // f
                }"}
            """;
        var r = $$"""
                  {{
                      a
                  // f
                  }}
                  """;

        return t + u + v + s + n + r;
    }
}
```

```csharp
class C
{
    string M(int a, int b)
    {
        var t = $"{
            // e
            a

            // f
            }";
        var u = $"{a
                   + b

            // f
            }";
        var v = $@"{
            a

            // f
            }";
        var s = $"""{
            a

            // f
            }""";
        var n = $"""
                 {$"{
                     a

                     // f
                     }"}
                 """;
        var r = $$"""
                  {{
                      a

                      // f
                  }}
                  """;

        return t + u + v + s + n + r;
    }
}
```

and a documentation comment behind the opening brace stays there while the member below it is re-indented:

```csharp
public class TestClass
{ /** Doc */
        public string Description { get; set; }
}
```

```csharp
public class TestClass
{ /** Doc */
    public string Description { get; set; }
}
```

### FMT-INDENT-007 — Multi-line comments move only their first line

The formatter MUST re-indent only the first line of a multi-line delimited comment (`/* … */`, `/** … */`). The following lines of the comment are part of its text and MUST be kept as written (FMT-PIPE-002, FMT-DOC-012).

Why: The lines inside a delimited comment are not separate trivia; their whitespace is content the author laid out, for example an aligned list or ASCII art.

Example:

```csharp
class C
{
    void M()
    {
            /* first
               second */
        var x = 1;
    }
}
```

```csharp
class C
{
    void M()
    {
        /* first
               second */
        var x = 1;
    }
}
```

### FMT-INDENT-008 — Every line of a `///` comment is indented with the documented code

The formatter MUST indent every `///` marker of a single-line documentation comment that starts its line to the column of the token the comment documents, including the markers of its continuation lines.

Why: A documentation comment is one block attached to its member. A comment whose first line follows the member but whose later lines stay behind breaks that block apart.

Example:

```csharp
class C
{
        /// <summary>
  /// Gets the value.
            /// </summary>
    int _value;
}
```

```csharp
class C
{
    /// <summary>
    /// Gets the value.
    /// </summary>
    int _value;
}
```

### FMT-INDENT-009 — Preprocessor directives other than regions start at column 0

The formatter MUST remove the whitespace in front of every active preprocessor directive that starts its line, except `#region` and `#endregion`, so the directive starts in column 0.

Why: Conditional-compilation and `#pragma` directives cut across the code's structure — they can open in one scope and close in another — so no scope depth describes them. Column 0 makes them stand out from the code they control.

Example:

```csharp
class C
{
    void M()
    {
        var x = 0;
        #if !NEVER
        x = 1;
        #endif
            #pragma warning disable CS0168
        int y;
    }
}
```

```csharp
class C
{
    void M()
    {
        var x = 0;
#if !NEVER
        x = 1;
#endif
#pragma warning disable CS0168
        int y;
    }
}
```

### FMT-INDENT-010 — Region directives are indented like the code around them

The formatter MUST indent an active `#region` or `#endregion` directive to the block indentation of the position it is in: the member column inside a type, the statement column inside a block and between the statements of a switch section, and the label column between the sections of a switch statement. A region directive in disabled text MUST NOT be touched.

When the token whose leading trivia holds the directive belongs to one of the following constructs below its opening delimiter — it is a token of one of the construct's elements, or the construct's closing delimiter — the formatter MUST instead indent the directive to the column of the construct's elements, which is the column FMT-INDENT-006 gives a comment that starts its line in the same position: the column of the element the directive stands in front of, and, in front of the closing delimiter, the column of the elements that delimiter closes. The constructs are an object, collection, array, implicit array, target-typed `new`, `with`, or `stackalloc` initializer, a nested initializer assigned to a member, and a complex element initializer (FMT-INDENT-022, FMT-INDENT-023); an anonymous object creation (FMT-INDENT-024); a collection expression, a list pattern, and the positional pattern clause of a recursive pattern (FMT-INDENT-025); a property pattern and a parenthesized pattern (FMT-INDENT-026); a switch expression (FMT-INDENT-027); and every list that FMT-INDENT-012 aligns. When several of these constructs and blocks hold the token, the innermost one decides: a directive in a block inside such a construct, such as the block body of a lambda passed as an argument, takes the statement column of that block, and a directive in front of the statement that holds such a construct takes the statement column of the statement's block.

This requirement decides the column of a region directive only in the positions it names; inside any other construct that is not a block scope, such as a wrapped method chain, a wrapped binary expression, or an attribute list, it leaves the column unspecified.

Why: Regions group members, statements, or the elements of a construct and belong to the structure they group, so they take that structure's column; a misindented region would otherwise look as if it belonged to an outer or inner scope. Inside an initializer, a collection expression, a pattern, a switch expression, or a wrapped list the grouped elements stand at an aligned column rather than at the block indentation, so the directive follows the elements, as a comment on its own line does in the same position. Disabled text is never rewritten (FMT-INDENT-011).

Example:

```csharp
class C
{
        #region Fields

    private int _x;

  #endregion // Fields

    void M(int x)
    {
        switch (x)
        {
                #region Cases

            case 1:
                break;

        #endregion // Cases
        }
    }
}
```

```csharp
class C
{
    #region Fields

    private int _x;

    #endregion // Fields

    void M(int x)
    {
        switch (x)
        {
            #region Cases

            case 1:
                break;

            #endregion // Cases
        }
    }
}
```

Example — directives in front of the elements of an object initializer, an argument list, and a switch expression take the elements' column; the region in front of the statement keeps the statement column:

```csharp
class P
{
    public int X { get; set; }

    public int Y { get; set; }
}

class C
{
    void N(int a, int b, int c)
    {
    }

    int M(int x)
    {
        #region Creation

        var p = new P
                {
        #region X

                    X = 1,

        #endregion // X

                    Y = 2
                };

        #endregion // Creation

        N(1,

        #region Second

          2,

        #endregion // Second

          3);

        return x switch
               {
        #region One

                   1 => 1,

        #endregion // One

                   _ => 0
               };
    }
}
```

```csharp
class P
{
    public int X { get; set; }

    public int Y { get; set; }
}

class C
{
    void N(int a, int b, int c)
    {
    }

    int M(int x)
    {
        #region Creation

        var p = new P
                {
                    #region X

                    X = 1,

                    #endregion // X

                    Y = 2
                };

        #endregion // Creation

        N(1,

          #region Second

          2,

          #endregion // Second

          3);

        return x switch
               {
                   #region One

                   1 => 1,

                   #endregion // One

                   _ => 0
               };
    }
}
```

Example — an `#endregion` in front of the closing delimiter of a collection expression or an object initializer takes the column of the elements, while the directives in the block body of a lambda passed as an argument take that block's statement column:

```csharp
class P
{
    public int X { get; set; }
}

class C
{
    void Run(int a, System.Action b)
    {
    }

    void M()
    {
        int[] b = [
                      1,

        #region Last

                      2

        #endregion // Last

                  ];
        var p = new P
                {
        #region X

                    X = 1

        #endregion // X
                };

        Run(1,
            () =>
            {
        #region Body

                M();

        #endregion // Body
            });
    }
}
```

```csharp
class P
{
    public int X { get; set; }
}

class C
{
    void Run(int a, System.Action b)
    {
    }

    void M()
    {
        int[] b = [
                      1,

                      #region Last

                      2

                      #endregion // Last

                  ];
        var p = new P
                {
                    #region X

                    X = 1

                    #endregion // X
                };

        Run(1,
            () =>
            {
                #region Body

                M();

                #endregion // Body
            });
    }
}
```

### FMT-INDENT-011 — Disabled text and literal content are never re-indented

The formatter MUST NOT change the whitespace inside disabled text or inside the text of a string or character literal, including the continuation lines of a verbatim string, and MUST NOT give a content line or the closing line of a multi-line raw string literal, nor a line inside one of its interpolation holes that FMT-RAW-007 moves only with the literal, a column from the requirements of this spec: such a line moves only as FMT-RAW-001 and FMT-RAW-007 move it, and the only whitespace of a raw string's text the formatter rewrites is the indentation the compiler strips, which FMT-RAW-010 writes as spaces. Only the other lines that start with a token, a comment, or a directive are re-indented; a line inside a hole that an alignment inside the hole places keeps that alignment.

Why: Disabled text is not parsed, so its structure is unknown; re-indenting it would half-format code nobody compiles. The whitespace inside a literal is part of its value (FMT-PIPE-002). Multi-line raw strings are aligned as FMT-RAW-001 states, which moves the lines of their holes with them; giving a hole line a column from the scope depth or from a construct outside the hole as well would add a second move that does not depend on where the literal sits.

Example:

```csharp
class C
{
    void M()
    {
#if NEVER
            var  y = 2;
  if (y) {
#endif
        var s = @"line one
      line two";
    }
}
```

is left unchanged.

### FMT-INDENT-012 — Wrapped argument and parameter lists align to the opening delimiter

When a parenthesized or bracketed list spans several lines — arguments, bracketed arguments, parameters, bracketed parameters, attribute arguments, tuple elements, tuple type elements, and unmanaged calling conventions — the formatter MUST indent every element, every separator, and the closing delimiter that starts a line to the column directly after the opening delimiter.

A bracketed argument list is aligned this way whatever its parent in the syntax tree is: an `ElementAccessExpressionSyntax` (`d[0,⏎1]`), the `ElementBindingExpressionSyntax` of a conditional element access (`c?[1,⏎2]`), or the `VariableDeclaratorSyntax` of a fixed-size buffer (`fixed int b[⏎3];`). The one exception is the key of an indexer initializer, whose parent is an `ImplicitElementAccessSyntax` (`[key] = value` in an object initializer): FMT-INDENT-014 indents it as a block.

Why: Aligning with the first element shows at a glance that the lines belong to one list and which call or declaration owns them, independent of the scope depth.

Example:

```csharp
class C
{
    void M()
    {
        Call(1,
           2,
                3);
    }

    void Call(int a,
          int b,
        int c)
    {
    }
}
```

```csharp
class C
{
    void M()
    {
        Call(1,
             2,
             3);
    }

    void Call(int a,
              int b,
              int c)
    {
    }
}
```

Example — a bracketed argument list aligns behind its opening bracket alike for an element access, a conditional element access, and a fixed-size buffer; FMT-BREAK-014 keeps the two index lists wrapped here, one because an element spans two lines and one because a comment sits in a gap, and it does not decide the buffer's size list:

```csharp
unsafe struct S
{
    fixed int b[
        3];

    void M(int[,] c, int[,] d)
    {
        var a = c?[1,
            2 +
            3];
        var e = d[0,
            /* c */ 1];
    }
}
```

```csharp
unsafe struct S
{
    fixed int b[
                3];

    void M(int[,] c, int[,] d)
    {
        var a = c?[1,
                   2
                   + 3];
        var e = d[0,
                  /* c */ 1];
    }
}
```

### FMT-INDENT-013 — A line inside a list element aligns with the element

When a line starts inside an element of a list from FMT-INDENT-012, FMT-INDENT-015, or FMT-INDENT-016 at a token other than the element's first token, the formatter MUST indent it to the column of the element's first token. A line inside a construct that lays out its own lines — such as a nested list, an initializer, a switch expression, a lambda body, or an interpolation hole — MUST be left to that construct; the clauses of a query expression are placed by FMT-INDENT-035.

Why: Such a line exists only because a comment, a directive, or disabled text kept FMT-BREAK from joining it. Without this rule it would be laid out as a continuation of the whole statement (FMT-INDENT-034), away from the element it belongs to.

Example:

```csharp
class C
{
    void M()
    {
        N(out // keep
        var value);
    }

    void N(out int v)
    {
        v = 0;
    }
}
```

```csharp
class C
{
    void M()
    {
        N(out // keep
          var value);
    }

    void N(out int v)
    {
        v = 0;
    }
}
```

A base list counts as a list from FMT-INDENT-016 for this requirement whatever the number of its types: a line inside the only type of a base list — of a class, struct, record, or interface, and the underlying type of an enum — aligns with that type's first token exactly as a line inside a type of a base list with two or more types does. FMT-INDENT-016's own condition, two or more types, applies only to the later base types it aligns.

Example — a base list with one type and an enum's underlying type:

```csharp
class B : System. // c
    IDisposable
{
    public void Dispose()
    {
    }
}

enum E : System. // c
    Int32
{
    A,
}
```

```csharp
class B : System. // c
          IDisposable
{
    public void Dispose()
    {
    }
}

enum E : System. // c
         Int32
{
    A
}
```

and a line inside a later type of a base list with two types aligns the same way, with that type's first token:

```csharp
class C : System.IDisposable,
          System. // c
    ICloneable
{
    public void Dispose()
    {
    }

    public object Clone()
    {
        return null;
    }
}
```

```csharp
class C : System.IDisposable,
          System. // c
          ICloneable
{
    public void Dispose()
    {
    }

    public object Clone()
    {
        return null;
    }
}
```

### FMT-INDENT-014 — A wrapped dictionary key is indented as a block

When the bracketed key of an indexer initializer inside an object initializer (`[key] = value`) spans several lines, the formatter MUST indent its arguments and separators that start a line one level deeper than the opening bracket and MUST align a closing bracket that starts a line with the opening bracket.

Why: The key sits at the start of an initializer member, so aligning to the column after `[` would put its continuation one character right of the member column — hard to tell apart from the member itself. A block indent makes the key read like a small nested scope.

Example:

```csharp
class Grid
{
    public int this[int a, int b]
    {
        set
        {
        }
    }
}

class C
{
    void M()
    {
        var grid = new Grid
                   {
                       [1 // keep
        , 2] = 3,
                   };
    }
}
```

```csharp
class Grid
{
    public int this[int a, int b]
    {
        set
        {
        }
    }
}

class C
{
    void M()
    {
        var grid = new Grid
                   {
                       [1 // keep
                           , 2] = 3,
                   };
    }
}
```

### FMT-INDENT-015 — Attribute lists and angle-bracket lists align to their first element

When an attribute list, a type argument list, a type parameter list, or a function-pointer parameter list spans several lines, the formatter MUST indent every later element, every separator, and the closing bracket that starts a line to the column of the list's first element. A list of omitted type arguments (`Dictionary<,>`) never reaches this requirement wrapped: FMT-BREAK-014 joins it, and the formatter refuses a comment, a directive, or disabled text inside it (FMT-UNSUP-018).

Why: These lists open with a bracket that sits directly in front of the first element, so the first element's column is the natural anchor. An omitted type argument has no width and therefore no column to align to; such a list is refused where it would stay wrapped, so the rule needs no case for it.

Example:

```csharp
class C
{
    [System.Obsolete("a"), // first
        System.CLSCompliant(true)]
    void M()
    {
    }

    System.Collections.Generic.Dictionary<int, // first
        string> Map => null;
}
```

```csharp
class C
{
    [System.Obsolete("a"), // first
     System.CLSCompliant(true)]
    void M()
    {
    }

    System.Collections.Generic.Dictionary<int, // first
                                          string> Map => null;
}
```

### FMT-INDENT-016 — Base types align to the first base type

When a base list with two or more types spans several lines, the formatter MUST indent every later base type that starts a line to the column of the first base type.

Why: The base list is a list like any other; aligning its entries shows that they all belong to the same declaration.

Example:

```csharp
class C : System.IDisposable, // first
    System.ICloneable
{
    public void Dispose()
    {
    }

    public object Clone()
    {
        return null;
    }
}
```

```csharp
class C : System.IDisposable, // first
          System.ICloneable
{
    public void Dispose()
    {
    }

    public object Clone()
    {
        return null;
    }
}
```

### FMT-INDENT-017 — Constructor initializers and constraint clauses are indented one level

The formatter MUST indent a constructor initializer (`: base(…)`, `: this(…)`) and every `where` constraint clause that starts a line one level deeper than the first line of the declaration that owns it.

Why: Both are part of the declaration's header, not of its body. One level of indentation separates them from the declaration name and from the body's braces, which stay at the declaration's own column.

Example:

```csharp
class B
{
    public B(int x)
    {
    }
}

class C<T> : B
    where T : class
{
    public C()
    : base(1)
    {
    }

    void M<U>()
            where U : struct
    {
    }
}
```

```csharp
class B
{
    public B(int x)
    {
    }
}

class C<T> : B
    where T : class
{
    public C()
        : base(1)
    {
    }

    void M<U>()
        where U : struct
    {
    }
}
```

### FMT-INDENT-018 — Binary operators align to the left operand

When an operator of a binary expression starts a line, the formatter MUST indent it to the column of the first token of the outermost left operand of the chain of operators of the same kind. A `??` operator MUST be indented one level deeper than that column.

Why: Operators at a shared column make the structure of a long condition or concatenation readable from the left edge. `??` is a fallback rather than a peer of its left operand, so it is set off by one level.

Example:

```csharp
class C
{
    bool M(bool a, bool b, bool c, string s)
    {
        var x = a
            && b
                && c;
        var y = s
            ?? "d";

        return x;
    }
}
```

```csharp
class C
{
    bool M(bool a, bool b, bool c, string s)
    {
        var x = a
                && b
                && c;
        var y = s
                    ?? "d";

        return x;
    }
}
```

### FMT-INDENT-019 — `is` aligns to its expression; pattern combinators align to `is`

When the `is` keyword of a pattern expression starts a line, the formatter MUST indent it to the column of the tested expression's first token. When an `and` or `or` combinator starts a line, the formatter MUST indent it to the column of the `is` keyword when the pattern belongs to an `is` expression, and otherwise to the column of the combined pattern's first token.

Why: `is` continues its expression like a binary operator. The combinators continue the pattern, whose visible start is the `is` keyword, so they line up under it.

Example:

```csharp
class C
{
    bool M(object o)
    {
        return o is int
            or long;
    }
}
```

```csharp
class C
{
    bool M(object o)
    {
        return o is int
                 or long;
    }
}
```

### FMT-INDENT-020 — Conditional operators are indented one level per nesting

When the `?` or `:` of a conditional expression starts a line, the formatter MUST indent it one level deeper than the first token of the outermost condition. Each conditional nested in the `true` or `false` branch MUST indent its own `?` and `:` one further level.

Why: The stair makes the nesting visible: every branch of a nested conditional sits under the operator that leads to it.

Example:

```csharp
class C
{
    int M(bool a, bool b)
    {
        return a
        ? 1
        : b
        ? 2
        : 3;
    }
}
```

```csharp
class C
{
    int M(bool a, bool b)
    {
        return a
                   ? 1
                   : b
                       ? 2
                       : 3;
    }
}
```

### FMT-INDENT-021 — A wrapped `when` clause of a case label is indented one level

When the `when` keyword of a pattern case label starts a line, the formatter MUST indent it one level deeper than the `case` keyword.

Why: The guard belongs to the label, so it must not reach the column of the section's statements, one level deeper, nor sit at the label's own column, where it would look like another label.

Example:

```csharp
class C
{
    void M(object o)
    {
        switch (o)
        {
            case int i // positive
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
            case int i // positive
                when i > 0:
                break;
        }
    }
}
```

### FMT-INDENT-022 — Creation initializers align to their keyword

For an object, array, implicit array, target-typed `new`, `with`, or `stackalloc` initializer, the formatter MUST indent an opening or closing brace that starts a line to the column of the `new`, `with`, or `stackalloc` keyword, and every member that starts a line one level deeper.

Why: The initializer belongs to the expression that creates the object, which can start anywhere on its line. Anchoring to the keyword keeps the braces with the expression they complete, the way block braces stay with their statement.

Example:

```csharp
class P
{
    public int X { get; set; }

    public P Child { get; set; }
}

class C
{
    void M()
    {
        var p = new P
        {
        X = 1,
                Child = new P
                {
                X = 2
                }
        };
    }
}
```

```csharp
class P
{
    public int X { get; set; }

    public P Child { get; set; }
}

class C
{
    void M()
    {
        var p = new P
                {
                    X = 1,
                    Child = new P
                            {
                                X = 2
                            }
                };
    }
}
```

and the same for `with` and `stackalloc`:

```csharp
class P
{
    public int X { get; set; }
}

class C
{
    void M(P p)
    {
        var q = p with
        {
        X = 3
        };
        System.Span<int> s = stackalloc int[]
        {
        1, 2
        };
    }
}
```

```csharp
class P
{
    public int X { get; set; }
}

class C
{
    void M(P p)
    {
        var q = p with
                  {
                      X = 3
                  };
        System.Span<int> s = stackalloc int[]
                             {
                                 1, 2
                             };
    }
}
```

### FMT-INDENT-023 — Initializers without a keyword align to their target or their brace

For a nested initializer assigned to a member (`Child = { … }`), the formatter MUST indent an opening brace that starts a line to the column of the assigned member, and the closing brace to the same column as the opening brace. For an array initializer of a declaration (`int[] a = { … }`), for an array initializer whose parent is another array initializer (a row `{ 1, 2 }` of a multi-dimensional array, as in `int[,] a = { { 1, 2 } }` and `new int[,] { { 1, 2 } }`), and for a complex element initializer (`{ 1, 2 }` inside a collection initializer), the closing brace MUST be indented to the column of the opening brace. An array initializer that is the initializer of an array creation inside another array initializer, such as `new[] { 5 }` in `int[][] j = { new[] { 5 } }`, is not such a row: its braces align to its `new` (FMT-INDENT-022). In every case the members that start a line MUST be indented one level deeper than the opening brace's column.

When a comment leads the line of the opening brace of a complex element initializer or of a row of a multi-dimensional array, the comment takes the column that line starts in, and the column of the opening brace is the column it stands in behind the comment (FMT-INDENT-030): the closing brace and the members follow that column, not the comment's. When the comment stands on a line above the brace, the brace starts its line, and its column is the column of that line. A comment in front of the opening brace of a nested initializer assigned to a member stands behind the `=` of the assignment, and one in front of the opening brace of a declaration's array initializer stands inside the declaration; the formatter refuses both (FMT-UNSUP-020, FMT-UNSUP-004).

Why: Without a `new` keyword the target of the assignment is the closest token that owns the initializer; when the brace stays on the line of `=`, the brace itself is the anchor. A row of a multi-dimensional array has neither a keyword nor a target, like a complex element initializer, so it is laid out like one: its own opening brace is the anchor of its elements and its closing brace.

Example:

```csharp
class P
{
    public int X { get; set; }

    public P Child { get; set; } = new P();
}

class C
{
    void M()
    {
        var p = new P
                {
                    Child =
                    {
                    X = 2
                    }
                };
        int[] b = {
        1,
        2
        };
    }
}
```

```csharp
class P
{
    public int X { get; set; }

    public P Child { get; set; } = new P();
}

class C
{
    void M()
    {
        var p = new P
                {
                    Child =
                    {
                        X = 2
                    }
                };
        int[] b = {
                      1,
                      2
                  };
    }
}
```

Example — a comment leads the line of a complex element initializer's opening brace; the comment takes the element's column, 33, the brace stands in column 41, and the elements and the closing brace follow the brace:

```csharp
using System.Collections.Generic;

class C
{
    Dictionary<int, int> D = new()
                             {
                                 /* c */ {
                                 3,
                                 4
                                 }
                             };
}
```

```csharp
using System.Collections.Generic;

class C
{
    Dictionary<int, int> D = new()
                             {
                                 /* c */ {
                                             3,
                                             4
                                         }
                             };
}
```

Example — the rows of a multi-dimensional array are laid out like complex element initializers, while the braces of an array creation inside an array initializer align to its `new`:

```csharp
class C
{
    void M()
    {
        int[,] a = {
                       {
                       1, 2
                       },
                       {
                       3, 4
                       }
                   };
        int[][] j = {
                        new[]
                        {
                        5
                        }
                    };
    }
}
```

```csharp
class C
{
    void M()
    {
        int[,] a = {
                       {
                           1, 2
                       },
                       {
                           3, 4
                       }
                   };
        int[][] j = {
                        new[]
                        {
                            5
                        }
                    };
    }
}
```

### FMT-INDENT-024 — Anonymous objects align to `new`

The formatter MUST indent the braces of an anonymous object creation that start a line to the column of its `new` keyword, and every member that starts a line one level deeper.

Why: Same as FMT-INDENT-022: the braces belong to the creation expression, not to the enclosing statement.

Example:

```csharp
class C
{
    void M()
    {
        var a = new
        {
        X = 1,
                Y = 2
        };
    }
}
```

```csharp
class C
{
    void M()
    {
        var a = new
                {
                    X = 1,
                    Y = 2
                };
    }
}
```

### FMT-INDENT-025 — Collection expressions, list patterns, and positional patterns indent from their bracket

When a collection expression, a list pattern, or the positional pattern clause of a recursive pattern (`(1, 2)` in `o is (1, 2)` and in `o is P(1, 2)`) spans several lines, the formatter MUST indent every element that starts a line one level deeper than the opening bracket or parenthesis and a closing bracket or parenthesis that starts a line to the column of the opening one.

Why: The opening bracket usually stays at the end of the line that introduces it, so it is the anchor of the block it opens, as with FMT-INDENT-023. A positional pattern is a list of subpatterns like a list pattern, and its opening parenthesis stays on the line of its type or its `is` as the bracket of a list pattern does (FMT-BREAK-022, FMT-BREAK-043), so it is laid out the same way.

Example:

```csharp
class C
{
    int[] M()
    {
        int[] a = [
        1,
                2
            ];

        return a;
    }
}
```

```csharp
class C
{
    int[] M()
    {
        int[] a = [
                      1,
                      2
                  ];

        return a;
    }
}
```

and a list pattern:

```csharp
class C
{
    bool M(int[] a)
    {
        return a is [
        1,
                2
            ];
    }
}
```

```csharp
class C
{
    bool M(int[] a)
    {
        return a is [
                        1,
                        2
                    ];
    }
}
```

and a positional pattern, with and without a type, also in a `case` label:

```csharp
class P
{
    public void Deconstruct(out int a, out int b)
    {
        a = 0;
        b = 0;
    }
}

class C
{
    bool M(object o, (int, int) t)
    {
        var a = o is P(
        1,
                2
            );

        switch (o)
        {
            case (
            1,
            2
            ):
                break;
        }

        return a && t is (
        1,
        2
        );
    }
}
```

```csharp
class P
{
    public void Deconstruct(out int a, out int b)
    {
        a = 0;
        b = 0;
    }
}

class C
{
    bool M(object o, (int, int) t)
    {
        var a = o is P(
                          1,
                          2
                      );

        switch (o)
        {
            case (
                     1,
                     2
                 ):
                break;
        }

        return a && t is (
                             1,
                             2
                         );
    }
}
```

### FMT-INDENT-026 — Property patterns and parenthesized patterns align to their introducer

When the braces of a property pattern or the parentheses of a parenthesized pattern span several lines, the formatter MUST indent the delimiters that start a line to the pattern's anchor column, the subpatterns of a property pattern one level deeper, and the pattern inside a parenthesized pattern four columns right of the opening parenthesis — whatever kind of pattern it is. The anchor column MUST be, skipping `and`, `or`, and `not` wrappers: the `is` keyword for a pattern of an `is` expression, the first token of the subpattern for a nested pattern, one level deeper than the opening parenthesis for a pattern inside a parenthesized pattern, and otherwise the first token of the pattern those wrappers lead to — the outermost pattern that holds the property or parenthesized pattern only through `and`, `or`, and `not` patterns. For `P and { … }` and `not { … }` in a switch-expression arm, in an element or a slice of a list pattern, or in a `case` label, that is the `P` or the `not`, not the brace. The same holds whether or not the pattern behind the wrapper has a type and whether it is a property or a parenthesized pattern: for `P { … } or Q { … }` and `not Q { … }` it is the `P` or the `not`, not the `Q`, and for `0 or ( … )`, `not ( … )`, and `case > 5 and ( … )` it is the `0`, the `not`, or the `>`, not the parenthesis or the `case` keyword; for a pattern with no wrapper around it, such as `P { … }` or `{ … }`, it is the pattern's own first token. A combinator that starts a line inside the parentheses aligns with the first token of the pattern it combines (FMT-INDENT-019).

Why: A multi-line pattern reads as a block that belongs to the construct introducing it; anchoring to `is` or to the owning subpattern keeps that block next to its introducer however far right the pattern starts. The content of a parenthesized pattern is one level inside its parentheses, as the maintainer decided, for a constant, type, relational, or combined pattern as for a property pattern. Skipping the wrappers also where nothing introduces the pattern keeps the braces of `P and { … }` and `not { … }` under the `P` or the `not`, where the braces of `P { … }` stand under the `P`; a brace behind a wrapper is not its own anchor, since a brace that starts its line below the wrapper would then have no column to take. Where the property or parenthesized pattern is itself the outermost pattern and its opening delimiter starts the line, as `{` at the start of a switch-expression arm or `(` below a `case` keyword, the requirement that places that line gives the delimiter its column, and the content follows from it.

Example:

```csharp
class C
{
    bool M(string value)
    {
        return value is
            (
            {
            Length: > 0
            }
            );
    }
}
```

```csharp
class C
{
    bool M(string value)
    {
        return value is
                     (
                         {
                             Length: > 0
                         }
                     );
    }
}
```

Example — the content of a parenthesized pattern that is no property pattern is indented four columns right of the opening parenthesis, and the combinator inside it aligns with the first pattern it combines:

```csharp
class C
{
    bool M(object o)
    {
        var d = o is
                  (
        int
        or long
                  );

        return d;
    }
}
```

```csharp
class C
{
    bool M(object o)
    {
        var d = o is
                  (
                      int
                      or long
                  );

        return d;
    }
}
```

Example — behind `and` and `not` in a switch-expression arm, in an element of a list pattern, and in a `case` label, the braces of a property pattern stand under the first token of the arm's, the element's, or the label's pattern, the `P` or the `not`, as they do for `P { … }`:

```csharp
class P
{
    public int A { get; set; }

    public int B { get; set; }
}

class C
{
    int M(P o)
    {
        return o switch
        {
            P and { A: 1,
                B: 2 } => 1,
            not { A: 3,
                B: 4 } => 2,
            _ => 0
        };
    }

    bool L(object[] a)
    {
        return a is [P and { A: 1,
            B: 2 }];
    }

    void N(P o)
    {
        switch (o)
        {
            case P and {
            A: 1
            }:
                break;

            case not {
            A: 2
            }:
                break;

            case P and not {
            A: 3
            }:
                break;
        }
    }
}
```

```csharp
class P
{
    public int A { get; set; }

    public int B { get; set; }
}

class C
{
    int M(P o)
    {
        return o switch
               {
                   P
                   and
                   {
                       A: 1,
                       B: 2
                   } => 1,
                   not
                   {
                       A: 3,
                       B: 4
                   } => 2,
                   _ => 0
               };
    }

    bool L(object[] a)
    {
        return a is [
                        P
                        and
                        {
                            A: 1,
                            B: 2
                        }
                    ];
    }

    void N(P o)
    {
        switch (o)
        {
            case P
                 and {
                     A: 1
                 }:
                break;

            case not {
                     A: 2
                 }:
                break;

            case P
                 and not {
                     A: 3
                 }:
                break;
        }
    }
}
```

Example — behind `and`, `or`, and `not`, the braces of a typed property pattern and the parentheses of a parenthesized pattern stand under the first token of the arm's, the element's, the slice's, or the label's pattern, not under the type or one level right of `case`; a pattern with no wrapper, such as the first `P { … }`, keeps its own first token:

```csharp
class P
{
    public int A { get; set; }

    public int B { get; set; }
}

class Q
{
    public int A { get; set; }

    public int B { get; set; }
}

class S
{
    public int Length => 0;

    public object this[int index] => null;

    public object Slice(int start, int length)
    {
        return null;
    }
}

class C
{
    int M(object o)
    {
        return o switch
        {
            P { A: 1,
                B: 2 } or Q { A: 1,
                B: 2 } => 1,
            null or P { A: 5,
                B: 6 } => 2,
            not Q { A: 3,
                B: 4 } => 3,
            _ => 0
        };
    }

    int K(int i)
    {
        return i switch
        {
            0
            or
            (1 or
            2) => 1,
            _ => 0
        };
    }

    bool L(S a, S b)
    {
        return a is [1, not Q { A: 1,
            B: 2 }]
            && a is [1, .. not Q { A: 1,
            B: 2 }]
            && b is [1, not (1 or
            2)]
            && b is [1, .. not (1 or
            2)];
    }

    void N(object o, int i)
    {
        switch (o)
        {
            case null or P { A: 1,
            B: 2 }:
                break;

            case not Q { A: 1,
            B: 2 }:
                break;
        }

        switch (i)
        {
            case > 5 and (< 10 or
            > 20):
                break;

            case 3 or (4 or
            5):
                break;

            case not (1 or
            2):
                break;
        }
    }
}
```

```csharp
class P
{
    public int A { get; set; }

    public int B { get; set; }
}

class Q
{
    public int A { get; set; }

    public int B { get; set; }
}

class S
{
    public int Length => 0;

    public object this[int index] => null;

    public object Slice(int start, int length)
    {
        return null;
    }
}

class C
{
    int M(object o)
    {
        return o switch
               {
                   P
                   {
                       A: 1,
                       B: 2
                   }
                   or Q
                   {
                       A: 1,
                       B: 2
                   } => 1,
                   null
                   or P
                   {
                       A: 5,
                       B: 6
                   } => 2,
                   not Q
                   {
                       A: 3,
                       B: 4
                   } => 3,
                   _ => 0
               };
    }

    int K(int i)
    {
        return i switch
               {
                   0
                   or
                   (
                       1
                       or 2
                   ) => 1,
                   _ => 0
               };
    }

    bool L(S a, S b)
    {
        return a is [
                        1,
                        not Q
                        {
                            A: 1,
                            B: 2
                        }
                    ]
               && a is [
                           1,
                           .. not Q
                              {
                                  A: 1,
                                  B: 2
                              }
                       ]
               && b is [
                           1,
                           not
                           (
                               1
                               or 2
                           )
                       ]
               && b is [
                           1,
                           .. not
                              (
                                  1
                                  or 2
                              )
                       ];
    }

    void N(object o, int i)
    {
        switch (o)
        {
            case null
                 or P {
                     A: 1,
                     B: 2
                 }:
                break;

            case not Q {
                     A: 1,
                     B: 2
                 }:
                break;
        }

        switch (i)
        {
            case > 5
                 and
                 (
                     < 10
                     or > 20
                 ):
                break;

            case 3
                 or
                 (
                     4
                     or 5
                 ):
                break;

            case not
                 (
                     1
                     or 2
                 ):
                break;
        }
    }
}
```

### FMT-INDENT-027 — Switch expressions align to their governing expression

The formatter MUST indent the braces of a switch expression that start a line to the column of the first token of its governing expression, and every arm that starts a line one level deeper.

Why: The governing expression is where the switch expression begins; aligning the braces with it keeps the arms visibly inside the expression, even when the expression is an argument or a return value far to the right.

Example:

```csharp
class C
{
    int M(int x)
    {
        return x switch
        {
        1 => 10,
                _ => 0
        };
    }
}
```

```csharp
class C
{
    int M(int x)
    {
        return x switch
               {
                   1 => 10,
                   _ => 0
               };
    }
}
```

### FMT-INDENT-028 — Lambda block bodies align to the lambda

The formatter MUST shift the block body of a lambda — its braces and every line between them — so the opening brace starts in the column of the lambda's anchor: the `async` keyword of an async lambda, otherwise the opening parenthesis of a parenthesized lambda or the parameter of a simple lambda. The lines inside the body MUST keep their indentation relative to the opening brace.

Why: A lambda can start anywhere on its line, typically after `=` or inside an argument list. Aligning its body with the lambda's own start keeps the body visually attached to the lambda rather than to the statement around it.

Example:

```csharp
using System;

class C
{
    void M()
    {
        Action<int> a = x =>
        {
        Console.WriteLine(x);
        };

        Func<int, System.Threading.Tasks.Task> b = async x =>
        {
            await System.Threading.Tasks.Task.Yield();
        };
    }
}
```

```csharp
using System;

class C
{
    void M()
    {
        Action<int> a = x =>
                        {
                            Console.WriteLine(x);
                        };

        Func<int, System.Threading.Tasks.Task> b = async x =>
                                                   {
                                                       await System.Threading.Tasks.Task.Yield();
                                                   };
    }
}
```

### FMT-INDENT-029 — Method chains align to their anchor link

The formatter MUST indent every link operator of a member-access chain (`.`, `?.`, `!.`) that starts a line to the column of the chain's anchor as SHARED-CHAIN-007 defines it, measured at the anchor's final column (FMT-INDENT-030).

Why: The anchor rule is shared with RH5201, so analyzer and formatter cannot disagree about where a wrapped link belongs; this phase only applies it.

Example:

```csharp
using System.Linq;

class C
{
    int[] Items = new int[0];

    void M()
    {
        var r = this.Items.Where(x => x > 0)
            .Select(x => x)
            .ToList();
    }
}
```

```csharp
using System.Linq;

class C
{
    int[] Items = new int[0];

    void M()
    {
        var r = this.Items.Where(x => x > 0)
                          .Select(x => x)
                          .ToList();
    }
}
```

and a chain whose first link is kept on its own line by a directive aligns to its root:

```csharp
class C
{
    object M()
    {
        return new Builder()
#if !NEVER
            .UseLogging()
#endif
            .Build();
    }
}
```

```csharp
class C
{
    object M()
    {
        return new Builder()
#if !NEVER
               .UseLogging()
#endif
               .Build();
    }
}
```

and a chain whose first wrapped link continues a prefix of property reads aligns to the last of them, with or without a later call:

```csharp
class C
{
    int[] Items = new int[0];

    object M(dynamic order)
    {
        var s = this.Items.Length
            .ToString();

        return order.Customer.Address
            .City;
    }
}
```

```csharp
class C
{
    int[] Items = new int[0];

    object M(dynamic order)
    {
        var s = this.Items.Length
                          .ToString();

        return order.Customer.Address
                             .City;
    }
}
```

### FMT-INDENT-030 — Anchors are measured at their final column

Every alignment above MUST be computed from the column its anchor token has after this phase, not from the column it had in the input: when the line of an anchor moves, everything aligned to it MUST move with it, through any number of nested constructs. This holds also when the anchor's line is a line that a multi-line raw string literal moves — its closing line, or a line inside one of its interpolation holes: the anchor is measured after the literal moved (FMT-RAW-001, FMT-RAW-007), which happens in this phase.

Why: Alignment against the input's columns would leave a wrapped list or initializer misaligned as soon as its owner is re-indented, and a second run would then move it again (FMT-PIPE-004). Because the raw string's lines move in this phase too, the anchor's final column is this phase's own result; no column has to be predicted from a phase that runs later.

Example:

```csharp
class C
{
    void M()
    {
            Call(1,
        2);
    }

    void Call(int a, int b)
    {
    }
}
```

```csharp
class C
{
    void M()
    {
        Call(1,
             2);
    }

    void Call(int a, int b)
    {
    }
}
```

Example — the anchor link `.Trim` stands behind the closing marker of a raw string literal that moves 12 columns to the right; `.ToUpper` aligns to the anchor where the move puts it:

```csharp
class C
{
    void M()
    {
        var s = """
    a
    """.Trim()
    .ToUpper();
    }
}
```

```csharp
class C
{
    void M()
    {
        var s = """
                a
                """.Trim()
                   .ToUpper();
    }
}
```

When the literal is not moved, because its closing marker already starts under its opening quotes, the link aligns to the same column, and the output is left unchanged.

The column of an anchor is the column it has in this phase's output, counting everything that stands in front of it on its line. When a comment stands in front of a line's first token, on that token's line, the comment takes the column FMT-INDENT-006 gives it and the token keeps its gap behind the comment (FMT-INDENT-005): an anchor on that line is measured behind the comment, and so are the opening quotes of a raw string literal on it (FMT-RAW-005). The same holds for a token behind the last line of a multi-line comment, which this phase does not move (FMT-INDENT-007): the token is measured where that line puts it. A comment that ends with its line — a `//` comment, or a delimited comment followed by a line break — is not in front of the token on the token's line; the token then starts its line and is measured at the line's column.

This holds also when the anchor is itself the first token of its line, such as the opening brace of a complex element initializer (FMT-INDENT-023), the opening bracket of a collection expression (FMT-INDENT-025), the `new` keyword of a creation expression (FMT-INDENT-022), the first token of a governing expression (FMT-INDENT-027), or the left operand of a wrapped binary expression (FMT-INDENT-018): the comment takes the column that the requirement placing the line gives its start, and the anchor is measured behind the comment, where it stands. A token that starts its line behind such a comment but anchors nothing — such as the opening brace of a creation initializer, whose members align to `new` (FMT-INDENT-022) — moves no other line.

Example — a comment leads the line of the call; `1` stands behind it, in column 21, and `2` aligns there:

```csharp
class C
{
    void M()
    {
        /* c */ Call(1,
2);
    }
}
```

```csharp
class C
{
    void M()
    {
        /* c */ Call(1,
                     2);
    }
}
```

while a comment on the line above the call leaves `Call` at the start of its line, and `2` aligns to column 13:

```csharp
class C
{
    void M()
    {
        // c
        Call(1,
             2);
    }
}
```

Example — a comment leads the line of a raw string literal whose closing line carries the anchor link `.Trim`; the opening quotes stand behind the comment, in column 24, the literal moves under them, and `.ToUpper` aligns to `.Trim` where that move puts it:

```csharp
class C
{
    void M()
    {
        /* c */ var s = """
    a
    """.Trim()
    .ToUpper();
    }
}
```

```csharp
class C
{
    void M()
    {
        /* c */ var s = """
                        a
                        """.Trim()
                           .ToUpper();
    }
}
```

The same holds for the alignment of every other requirement above, such as a chain link: behind `/* c */ a.B()`, a wrapped `.C()` aligns to `.B`, in column 17 when the line starts in column 8.

Example — a comment leads the line of a complex element initializer's opening brace, which is the first token of its line; the comment takes the element's column, 20, the brace stands behind it in column 28, and the element's expressions and closing brace follow the brace (FMT-INDENT-023), while the second element, whose comment stands on the line above, keeps its brace at the line's column:

```csharp
using System.Collections.Generic;

class C
{
    void M()
    {
        var d = new Dictionary<int, int>
                {
                    /* e */ {
                        1,
                        2
                    },

                    // f
                    {
                        3,
                        4
                    }
                };
    }
}
```

```csharp
using System.Collections.Generic;

class C
{
    void M()
    {
        var d = new Dictionary<int, int>
                {
                    /* e */ {
                                1,
                                2
                            },

                    // f
                    {
                        3,
                        4
                    }
                };
    }
}
```

### FMT-INDENT-031 — An anonymous method's block aligns like a block lambda

When the opening brace of an anonymous method's block starts a line, the formatter MUST shift the block — its braces and every line between them — so the opening brace starts in the column of the anonymous method's anchor: the `async` keyword of an async anonymous method, otherwise the `delegate` keyword, also behind a `static` modifier. The anchor is the one FMT-INDENT-028 takes for a block lambda in the same position: an `async` lambda aligns at `async`, a `static` one past `static`. The lines inside the block MUST keep their indentation relative to the opening brace, one level deeper for the block's statements as in every block (FMT-INDENT-002). The column of the anchor is measured where this phase puts it (FMT-INDENT-030), also after a multi-line raw string literal around it moved (FMT-RAW-007).

This holds wherever the anonymous method stands — as an argument, as the value of a field, a declaration, an assignment, a compound assignment, or a return statement, in an object initializer, a conditional branch, a switch-expression arm, or a collection expression, in a chain of calls, inside the block body of a lambda or of another anonymous method, and in an interpolation hole of any interpolated string — exactly as FMT-INDENT-028 aligns a block lambda in every position. A block whose opening brace does not start its line, such as one that stays on the line of `delegate` in an interpolation that cannot hold a line break (FMT-BREAK-044), is not shifted.

Why: An anonymous method can start anywhere on its line, typically after `=` or inside an argument list, as a lambda can. Aligning its block with its own first token keeps the block visually attached to it rather than to the statement around it, the layout FMT-INDENT-028 gives the block lambda that is the newer spelling of the same construct. The maintainer decided that an interpolation hole has no layout rules of its own, so the anonymous method has one layout in every position: the scope-depth column, which it took outside the holes of multi-line raw string literals before, is not available inside them (FMT-RAW-007), while the anchor is available everywhere. The rest of the anonymous method is formatted as everywhere else: its braces and statements start their own lines (FMT-BREAK-002, FMT-BREAK-006), and its body is one level deeper than its braces.

Example — as an argument, followed by another argument, behind a `static` modifier (under `delegate`), behind an `async` modifier (under `async`), with a parameter list, and as the value of a declaration, an assignment, and a return statement:

```csharp
using System;
using System.Threading.Tasks;

class C
{
    object M(Func<int> f, int y = 0)
    {
        var x = M(delegate { return 1; }, 2);
        Func<int> s = static delegate { return 1; };
        Func<Task<int>> t = async delegate { return 1; };
        Func<int, int> d = delegate (int a) { return a; };
        Func<int> g;
        g = delegate { return 1; };

        return M(delegate { return 1; });
    }
}
```

```csharp
using System;
using System.Threading.Tasks;

class C
{
    object M(Func<int> f, int y = 0)
    {
        var x = M(delegate
                  {
                      return 1;
                  },
                  2);
        Func<int> s = static delegate
                             {
                                 return 1;
                             };
        Func<Task<int>> t = async delegate
                            {
                                return 1;
                            };
        Func<int, int> d = delegate (int a)
                           {
                               return a;
                           };
        Func<int> g;
        g = delegate
            {
                return 1;
            };

        return M(delegate
                 {
                     return 1;
                 });
    }
}
```

Example — as a field's value, in an object initializer, in a conditional branch, in a switch-expression arm, as a collection element, behind `+=`, in a chain of calls (each block under its own `delegate`, the second measured after the first moved), and nested in another anonymous method's block, as a block lambda aligns in each of these positions (FMT-INDENT-028):

```csharp
using System;
using System.Collections.Generic;
using System.Linq;

class D
{
    public Func<int> F { get; set; }
}

class C
{
    private Func<int> _f = delegate { return 1; };

    event EventHandler E;

    int M(Func<int> f, int k = 0)
    {
        var d1 = new D { F = delegate { return 1; } };
        Func<int> c1 = k > 0 ? delegate { return 1; } : null;
        var a = k switch
        {
            0 => M(delegate { return 1; }),
            _ => 2,
        };
        Func<int>[] arr = [delegate { return 1; }, null];
        E += delegate { Console.WriteLine(); };
        var q = new List<int>().Select(delegate (int b) { return b; }).Where(delegate (int b) { return b > 0; });

        return M(delegate { return M(delegate { return 2; }); });
    }
}
```

```csharp
using System;
using System.Collections.Generic;
using System.Linq;

class D
{
    public Func<int> F { get; set; }
}

class C
{
    private Func<int> _f = delegate
                           {
                               return 1;
                           };

    event EventHandler E;

    int M(Func<int> f, int k = 0)
    {
        var d1 = new D
                 {
                     F = delegate
                         {
                             return 1;
                         }
                 };
        Func<int> c1 = k > 0
                           ? delegate
                             {
                                 return 1;
                             }
                           : null;
        var a = k switch
                {
                    0 => M(delegate
                           {
                               return 1;
                           }),
                    _ => 2,
                };
        Func<int>[] arr = [
                              delegate
                              {
                                  return 1;
                              },
                              null
                          ];
        E += delegate
             {
                 Console.WriteLine();
             };

        var q = new List<int>().Select(delegate (int b)
                                       {
                                           return b;
                                       }).Where(delegate (int b)
                                                {
                                                    return b > 0;
                                                });

        return M(delegate
                 {
                     return M(delegate
                              {
                                  return 2;
                              });
                 });
    }
}
```

Example — the literal moves 13 columns to the right, and the anonymous method's block aligns under `delegate`:

```csharp
class C
{
    int M(System.Func<int> f)
    {
        var a = $"""
    Hello {M(delegate { return 1; })}
    """;
    }
}
```

```csharp
class C
{
    int M(System.Func<int> f)
    {
        var a = $"""
                 Hello {M(delegate
                          {
                              return 1;
                          })}
                 """;
    }
}
```

Example — in an aligned literal, with statements separated as anywhere else, behind a `static` modifier (under `delegate`), behind an `async` modifier (under `async`), and inside a lambda's expression body:

```csharp
class C
{
    int M(System.Func<int> f)
    {
        var a = $"""
                 Hello {M(delegate { int x = 1; return x; })}
                 S {M(static delegate { return 1; })}
                 T {M(async delegate { return 1; })}
                 A {M(() => M(delegate { return 1; }))}
                 """;

        return 0;
    }
}
```

```csharp
class C
{
    int M(System.Func<int> f)
    {
        var a = $"""
                 Hello {M(delegate
                          {
                              int x = 1;

                              return x;
                          })}
                 S {M(static delegate
                             {
                                 return 1;
                             })}
                 T {M(async delegate
                      {
                          return 1;
                      })}
                 A {M(() => M(delegate
                              {
                                  return 1;
                              }))}
                 """;

        return 0;
    }
}
```

Example — a block lambda inside the anonymous method is aligned to the lambda (FMT-INDENT-028), measured after the anonymous method's block moved:

```csharp
class C
{
    int M(System.Func<int> f)
    {
        var a = $"""
                 Hello {M(delegate { return M(() => { return 1; }); })}
                 """;
    }
}
```

```csharp
class C
{
    int M(System.Func<int> f)
    {
        var a = $"""
                 Hello {M(delegate
                          {
                              return M(() =>
                                       {
                                           return 1;
                                       });
                          })}
                 """;
    }
}
```

Example — the literal moves 13 columns to the right, and an anonymous method in a hole of a regular, a verbatim, and a single-line raw interpolated string nested in the hole aligns under `delegate`, as `() =>` aligns at its `(` in the same position:

```csharp
class C
{
    int M(System.Func<int> f)
    {
        var a = $"""
    R {$"in {M(delegate { return 1; })}"}
    V {$@"in {M(delegate { return 1; })}"}
    S {$"""in {M(delegate { return 1; })}"""}
    L {$"in {M(() => { return 1; })}"}
    """;

        return 0;
    }
}
```

```csharp
class C
{
    int M(System.Func<int> f)
    {
        var a = $"""
                 R {$"in {M(delegate
                            {
                                return 1;
                            })}"}
                 V {$@"in {M(delegate
                             {
                                 return 1;
                             })}"}
                 S {$"""in {M(delegate
                              {
                                  return 1;
                              })}"""}
                 L {$"in {M(() =>
                            {
                                return 1;
                            })}"}
                 """;

        return 0;
    }
}
```

The same input with the literal already aligned gives the same output. A single-line raw string that no multi-line raw hole encloses aligns the block under `delegate` as well:

```csharp
class C
{
    int M(System.Func<int> f)
    {
        var c = $"""in {M(delegate
        {
            return 1;
        })}""";

        return 0;
    }
}
```

```csharp
class C
{
    int M(System.Func<int> f)
    {
        var c = $"""in {M(delegate
                          {
                              return 1;
                          })}""";

        return 0;
    }
}
```

Example — inside the block body of a lambda, in a hole as outside one, the anonymous method's block aligns under `delegate`, measured after the lambda's body moved:

```csharp
class C
{
    int M(System.Func<int> f)
    {
        var a = $"""
                 G {M(() =>
                      {
                          return M(delegate
                          {
                              return 1;
                          });
                      })}
                 """;

        return M(() =>
        {
            return M(delegate
            {
                return 1;
            });
        });
    }
}
```

```csharp
class C
{
    int M(System.Func<int> f)
    {
        var a = $"""
                 G {M(() =>
                      {
                          return M(delegate
                                   {
                                       return 1;
                                   });
                      })}
                 """;

        return M(() =>
                 {
                     return M(delegate
                              {
                                  return 1;
                              });
                 });
    }
}
```

Example — the same holds when a regular interpolated string lies between the lambda's body and the anonymous method:

```csharp
class C
{
    int M(System.Func<int> f)
    {
        var a = $"""
                 G {M(() =>
                      {
                          return M($"x{M(delegate
                          {
                              return 1;
                          })}");
                      })}
                 """;

        return 0;
    }
}
```

```csharp
class C
{
    int M(System.Func<int> f)
    {
        var a = $"""
                 G {M(() =>
                      {
                          return M($"x{M(delegate
                                         {
                                             return 1;
                                         })}");
                      })}
                 """;

        return 0;
    }
}
```

Example — with language version C# 10, an anonymous method in an interpolation of a regular interpolated string stays on one line (FMT-BREAK-044); its opening brace does not start a line, and nothing is shifted:

```csharp
class C
{
    string M(System.Func<int> f)
    {
        return $"{M(delegate { return 1; })}";
    }
}
```

is left unchanged.

### FMT-INDENT-032 — Wrapped constraints align to the first constraint

This requirement applies to every `where` clause, whichever declaration owns it — a type, a method, a local function, a delegate, or an extension block.

When the constraints of a `where` clause span several lines, the formatter MUST indent every constraint that starts a line to the column of the clause's first constraint, the constraint directly behind the colon. When the first constraint itself starts a line, because a comment, a directive, or disabled text behind the colon keeps FMT-BREAK-037 from joining it onto the colon's line, the formatter MUST indent the constraints that start a line to the column behind the colon and one space — the column the first constraint has when it stands behind the colon. When the colon starts a line, because a comment, a directive, or disabled text behind the type parameter keeps FMT-BREAK-037 from joining it, the formatter MUST indent the colon one level deeper than the clause's `where` keyword, and the constraints follow from the colon's column as stated above.

This requirement places only the lines that start with the colon or with a constraint. No line of a wrapped clause starts with a comma: FMT-BREAK-037 ends the line of the constraint in front of every comma, and the formatter refuses a comma that a comment, a directive, or disabled text keeps off that line (FMT-UNSUP-009). A line inside a constraint that starts at a token other than the constraint's first token is placed by the requirement of the construct it belongs to, measured from where the constraint stands after this requirement (FMT-INDENT-030); FMT-INDENT-015 aligns a later type argument.

Why: Aligning with the first constraint shows that the constraints belong to one clause, as base types align to the first base type (FMT-INDENT-016); the block column of the declaration's scope would put them left of their own `where` keyword. Taking the first constraint's column also when a comment or a directive pushes it onto the next line keeps the column independent of it, as an argument list whose first argument a comment pushes down aligns to the column after its opening parenthesis (FMT-INDENT-012). A colon that has to start a line belongs to the clause's header, which FMT-INDENT-017 indents one level from its owner; one level deeper than `where` keeps it apart from the next `where` clause.

Example — the constraints of a type's, a method's, and an extension block's clauses that start a line align under the first constraint; a later `where` clause keeps the column of FMT-INDENT-017:

```csharp
class A<TKey, TValue>
    where TKey : class,
    new()
    where TValue : struct
{
    void M<T>()
        where T : class,
        new()
    {
    }
}

static class E
{
    extension<T>(T value)
        where T : class,
        new()
    {
        public void X()
        {
        }
    }
}
```

```csharp
class A<TKey, TValue>
    where TKey : class,
                 new()
    where TValue : struct
{
    void M<T>()
        where T : class,
                  new()
    {
    }
}

static class E
{
    extension<T>(T value)
        where T : class,
                  new()
    {
        public void X()
        {
        }
    }
}
```

Example — a constraint that a comment or a directive pushes onto the next line takes the first constraint's column, and a colon that a comment pushes onto the next line is indented one level deeper than `where`:

```csharp
class C
{
    void M<T>()
        where T : // c
        class,
        new()
    {
    }

    void N<T>()
        where T :
#if !X
        class,
#endif
        new()
    {
    }

    void P<T>()
        where T // c
        : class,
        new()
    {
    }
}
```

```csharp
class C
{
    void M<T>()
        where T : // c
                  class,
                  new()
    {
    }

    void N<T>()
        where T :
#if !X
                  class,
#endif
                  new()
    {
    }

    void P<T>()
        where T // c
            : class,
              new()
    {
    }
}
```

### FMT-INDENT-033 — A comma that starts a line in an initializer, a collection expression, or a pattern takes its elements' column

Status: Retired in the change that added it — a line that starts with a comma in these constructs is left only where a comment, a directive, or disabled text keeps the comma off the line of the element in front of it, and the formatter refuses that shape (FMT-UNSUP-009); every other such comma moves behind its element (FMT-BREAK-038), so no line is left for a column.

### FMT-INDENT-034 — A continuation line no other requirement places is indented one level deeper than the line its construct starts on

The formatter MUST indent a continuation line that no other requirement of this spec and no FMT-RAW requirement places — FMT-INDENT-002 and FMT-INDENT-004 excepted — four columns right of the column at which the line of the construct it continues begins:

- **Continuation line.** A line is a continuation line when the token that starts it (SHARED-BLANK-008) stands inside a statement, a member declaration, or a using directive that starts on an earlier line, and does not begin a part of it that block indentation places by its scope (FMT-INDENT-002 to FMT-INDENT-004). Those parts are a nested statement — a block, an embedded statement, the statement of a labeled statement, and a local function included —, a nested member declaration — an enum member included —, an accessor, a switch section or label, a brace of a scope that FMT-INDENT-002 names, the `else`, `catch`, or `finally` keyword of its clause, the `while` of a `do` statement, and an attribute list of a member, an accessor, or a local function together with the first token behind the last such list. A line that starts with any other token of the statement, member, or directive is a continuation line: a value, an operand, a pattern, a declarator, a clause, a modifier behind the first one, a type, a base list, or the closing token of a construct. A node that a node-level entry point formats as a root of its own (FMT-PIPE-023) counts as such a statement when no statement, member declaration, or using directive inside it holds the token — for example the `&&` chain the RH5302 fix formats: the root stands in for the statement or member that holds it in the document, so a line that starts inside it below its first line is a continuation line, as it is in the whole document.
- **Construct it continues.** The construct a continuation line continues is the nearest node that contains the line's first token and whose first token stands on an earlier line. A node that starts on the line itself does not count, however far it reaches: the `3` of the switch arm `1 =>⏎3 + 4,` starts the binary expression `3 + 4`, so the line continues the arm, which starts on the line of `1`.
- **Its line.** The line of that construct is the line on which its first token stands. When that line begins inside text that started on a still earlier line — the text of a multi-line string literal or of a multi-line comment —, the line of the construct is the line on which that text starts.
- **The column.** The column at which that line begins is the column the indentation phase gives the first token or comment on it, measured where the phase puts it (FMT-INDENT-030), so a continuation line of a continuation line is indented one more level.
- **A root formatted on its own.** When a node-level entry point formats a node as a root of its own (FMT-PIPE-023) and the line of the construct is the line on which the root's first token stands, the column at which that line begins is the line-start column (FMT-PIPE-024): the column of the first token or comment on that line in the document, which may stand in front of the root. The entry point keeps everything in front of the root as written, so that line begins where the document has it, and the continuation line gets the column whole-document formatting gives it when the document's line begins where whole-document formatting puts it. `FormatNode`, which has no such fact, measures from the root's first token.

The gaps FMT-BREAK-041 joins, the gaps behind an assignment operator and behind an `=` that FMT-BREAK-016 joins, and the combinators FMT-BREAK-017 moves to the start of a line leave no continuation line behind them; a comment or a directive in one of those gaps is refused (FMT-UNSUP-011, FMT-UNSUP-017, FMT-UNSUP-020). The lines this requirement places are those no join removes: a line kept behind a comment or a directive that no requirement refuses, such as the operand behind `( // c` of a parenthesized expression, a later declarator, a `;` behind a conditional group or a documentation comment written behind code, a line inside the hole of a verbatim interpolated string, and a line behind any other line break no requirement joins, such as the expression behind a switch arm's `=>` (a comment there is refused, FMT-UNSUP-021). Among them are the lines a comment keeps inside a base list, inside an attribute, type argument, type parameter, or function-pointer parameter list, and inside a qualified name: a first base type below the base list's `:`, which FMT-BREAK-042 does not join across the comment (FMT-BREAK-001); a first element below the opening bracket of one of those lists, which FMT-BREAK-014 does not join across it; and the right part of a qualified name below its `.`. No requirement aligns them with the colon, the bracket, or the name: this requirement places them, the later base types and list elements align under the first (FMT-INDENT-015, FMT-INDENT-016), and a line inside an element of a list that FMT-INDENT-013 places keeps that column.

Why: A continuation line that took the column of its statement or member read like a new statement, or, inside an expression laid out to the right, landed left of the construct it belongs to. One rule for every construct replaces one contributor per construct, which kept leaving gaps; the requirements that align a construct to an anchor keep their own columns, so this rule only places what none of them owns. Taking the nearest construct that starts on an earlier line keeps a continuation inside an element of a collection expression or an initializer to the right of that construct, and makes the column independent of the entry point: a code fix that formats only a node measures from the same line as whole-document formatting, and where that line begins in front of the node, the line-start column carries where the document begins it, so the fix gives the line the same column as whole-document formatting. A line that block indentation places by its scope is not a continuation, so a statement, an `else`, a label, or an attribute list on its own line keeps its column. The lines a comment keeps inside a base list, a bracket list, or a qualified name get no column of their own: a comment there is reported by another rule — RH5043 in a base list, RH5044 in an attribute list, RH5046 in a type parameter or type argument list, RH5104 behind code on its line — or stands where code hardly ever holds one, so the class is rare, and a column behind the colon, behind the bracket, or measured from the name would be a placement rule for that class alone.

Example — an operand kept on its own line by a comment behind a parenthesis continues the statement, and an arm's expression kept on its own line behind the `=>` continues the arm, also where it starts an expression of its own (`3 + 4`):

```csharp
class C
{
    int M(int x, int y)
    {
        x = ( // c
        y + 3);

        var s = y switch
                {
                    1 =>
                    3 + 4,
                    _ => 0
                };

        return x + s;
    }
}
```

```csharp
class C
{
    int M(int x, int y)
    {
        x = ( // c
            y + 3);

        var s = y switch
                {
                    1 =>
                        3 + 4,
                    _ => 0
                };

        return x + s;
    }
}
```

Example — the line continues a construct inside an expression, so it is indented from the line of that construct: the operand inside an element of a collection expression from the line of `(` in column 22:

```csharp
class C
{
    int M(int y)
    {
        int[] z = [
                      1,
                      ( // c
        y + 2)
                  ];

        return z.Length;
    }
}
```

```csharp
class C
{
    int M(int y)
    {
        int[] z = [
                      1,
                      ( // c
                          y + 2)
                  ];

        return z.Length;
    }
}
```

Example — the condition of a `case` label's `when` clause, wrapped behind `when`, continues the label, which starts on the line of `case`, so it is indented one level deeper than `case`:

```csharp
class C
{
    void M(int value)
    {
        switch (value)
        {
            case 1 when
            Check(value):
                break;
        }
    }

    bool Check(int v)
    {
        return v > 0;
    }
}
```

```csharp
class C
{
    void M(int value)
    {
        switch (value)
        {
            case 1 when
                Check(value):
                break;
        }
    }

    bool Check(int v)
    {
        return v > 0;
    }
}
```

Example — a first base type below the colon, a first type argument below `<`, and the right part of a qualified name below its `.`, each kept there by a comment, are continuation lines; the later base type aligns under the first:

```csharp
using System;
using System.Collections.Generic;

interface I1;

interface I2;

class C : // c
I1,
I2
{
    List< // c
    int> P => null;

    void N<T>()
        where T : System. // c
        IDisposable
    {
    }
}
```

```csharp
using System;
using System.Collections.Generic;

interface I1;

interface I2;

class C : // c
    I1,
    I2
{
    List< // c
        int> P => null;

    void N<T>()
        where T : System. // c
            IDisposable
    {
    }
}
```

Example — a later declarator continues its declaration, also behind an `#endif`, in a field that stays whole, an event field, and a local declaration; the declarator in the disabled text keeps its column (FMT-INDENT-011):

```csharp
using System;

class C
{
    /// <summary>
    /// Q
    /// </summary>
    public int Q,
    R;

    public event Action A,
#if X
        B,
#endif
    D;

    void M()
    {
        int a = 1,
        b = 2;
    }
}
```

```csharp
using System;

class C
{
    /// <summary>
    /// Q
    /// </summary>
    public int Q,
        R;

    public event Action A,
#if X
        B,
#endif
        D;

    void M()
    {
        int a = 1,
            b = 2;
    }
}
```

Example — the line of the interpolation in front of `x` begins inside the text of the verbatim string, which starts on the line of `var`, so `x` and the closing brace of the hole are indented from the line of `var`; the text of the string keeps its columns (FMT-INDENT-011):

```csharp
class C
{
    string M(int x)
    {
        var s = $@"line one
      {
        x
        }";

        return s;
    }
}
```

```csharp
class C
{
    string M(int x)
    {
        var s = $@"line one
      {
            x
            }";

        return s;
    }
}
```

Example — the RH5302 fix formats the `&&` expression as a root of its own, and that root starts behind `var r = ` in column 16. The parenthesized expression that `x + 3)` continues starts on the root's first line, which begins with `var` in column 8 in the document, so `x + 3) > 0` is indented to column 12 by the fix, as by whole-document formatting, and not to column 20, four columns right of the root's first token. The `&&` aligns with its left operand (FMT-INDENT-018):

```csharp
class C
{
    bool M(int x, bool y)
    {
        var r = ( // c
        x + 3) > 0 &&
        y;

        return r;
    }
}
```

```csharp
class C
{
    bool M(int x, bool y)
    {
        var r = ( // c
            x + 3) > 0
                && y;

        return r;
    }
}
```

The RH5308 fix of `var s = ( // c⏎x + 3) > 0 ? 1 :⏎2;` gives `x + 3) > 0` column 12 in the same way, with `? 1` and `: 2;` in column 20 (FMT-INDENT-020).

Example — lines that block indentation places by their scope are not continuations and keep their columns: the attribute lists of a member and the token behind them, a label and its statement, `else`, and the `while` of a `do` statement:

```csharp
class C
{
    [System.Obsolete]
    [System.CLSCompliant(false)]
    public void M(int x)
    {
        L:
        x++;

        if (x > 0)
        {
        }
        else
        {
        }

        do
        {
        }
        while (x < 0);
    }
}
```

is left unchanged.

### FMT-INDENT-035 — Query clauses align with the query's first token

When a clause of a query expression starts a line — a `from`, `let`, `where`, `join`, `orderby`, `select`, or `group` clause, or the `into` of a query continuation —, the formatter MUST indent it to the column of the first token of the query expression the clause belongs to, inside a list element as anywhere else. The clauses behind an `into` belong to the query expression that holds the continuation. A line inside a clause that starts at another token of the clause is a continuation line of that clause (FMT-INDENT-034).

Why: The clauses of one query read as a column under `from`, which is how a query is read top to bottom; the statement's column would put them left of the query they belong to, and one level deeper than the statement would put them in no relation to `from`. Inside a list element the clauses already aligned with the query's first token, and a query outside a list element has no reason to look different.

Example:

```csharp
using System.Linq;

class C
{
    void M(int[] arr, int[] brr)
    {
        var q = from x in arr
        let y = x
        where y > 0
        select y;
        var r = from x in arr
        join b in brr on x
        equals b
        select x into z
        select z;
    }
}
```

```csharp
using System.Linq;

class C
{
    void M(int[] arr, int[] brr)
    {
        var q = from x in arr
                let y = x
                where y > 0
                select y;
        var r = from x in arr
                join b in brr on x
                    equals b
                select x into z
                select z;
    }
}
```

### FMT-INDENT-036 — A first base type kept below the colon takes the column behind the colon

Status: Retired in the change that added it — a first base type that a comment keeps below the base list's colon stays a continuation line (FMT-INDENT-034), and the later base types align under it (FMT-INDENT-016). RH5043 reports every comment inside a base list, so the class is rare, and it keeps the continuation indent rather than a column of its own.

### FMT-INDENT-037 — A first element kept below its opening bracket aligns behind the bracket

Status: Retired in the change that added it — a first element that a comment keeps below the opening bracket of an attribute list, a type argument list, a type parameter list, or a function-pointer parameter list stays a continuation line (FMT-INDENT-034), or, inside an element of a list that FMT-INDENT-013 places, keeps that column; the later elements align under it (FMT-INDENT-015). RH5044 and RH5046 report every comment inside these lists, so the class is rare, and it keeps the continuation indent rather than a column of its own.

### FMT-INDENT-038 — A line inside a qualified name is indented one level deeper than the name

Status: Retired in the change that added it — a line that a comment keeps inside a qualified name stays a continuation line (FMT-INDENT-034), or, inside an element of a list that FMT-INDENT-013 places, keeps that column. The comment is rare, and a column measured from the name would also have moved qualified names wrapped without any comment — in a cast, a `typeof`, a `default`, a using alias, a namespace name — which no requirement or issue asks to change.

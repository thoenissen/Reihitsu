# Formatter — Indentation

| | |
|---|---|
| ID prefix | FMT-INDENT |
| Status | Active |
| Code owners | `IndentationPhase`, `LayoutComputer`, `LayoutModel`, `TokenLayout`, `IndentationRewriter`, `ListElementAligner`, `SyntaxIndentationUtilities`, `ArgumentAlignmentContributor`, `AngleBracketListAlignmentContributor`, `AnonymousObjectContributor`, `BaseTypeListContributor`, `BinaryExpressionContributor`, `CaseWhenClauseContributor`, `CollectionExpressionContributor`, `CommentIndentationContributor`, `ConditionalExpressionContributor`, `ConstructorInitializerContributor`, `GenericConstraintContributor`, `LambdaAlignmentContributor`, `ListPatternContributor`, `MethodChainAlignmentContributor`, `ObjectInitializerContributor`, `ParenthesizedPatternContributor`, `PatternAnchor`, `RecursivePatternContributor`, `SwitchExpressionContributor` |
| Uses | FMT-BREAK-014, FMT-PIPE-001, FMT-PIPE-002, FMT-PIPE-004, FMT-PIPE-015, SHARED-BLANK-002, SHARED-BLANK-008, SHARED-CHAIN-007, FMT-DOC-003, FMT-DOC-012, FMT-RAW-001, FMT-RAW-005, FMT-RAW-007, FMT-SWITCH-001 |

This spec covers the indentation phase: the column at which every line that starts with a token begins — block indentation by scope depth, the alignment of wrapped constructs to an anchor token, and the re-indentation of comments and directives that start a line. Line breaks are already decided when this phase runs (FMT-PIPE-001); it only changes the whitespace in front of the first token or trivia of a line. The same phase moves the lines of multi-line raw string literals as FMT-RAW specifies, so the column it gives a line is final (FMT-PIPE-015).

## Non-goals

- Where lines break — FMT-BREAK. This phase never adds or removes a line break.
- Spacing between tokens on the same line — FMT-SPACE. The rest of a line keeps its spacing and moves with its first token.
- Blank lines — FMT-BLANK.
- How the content lines and the closing marker of a multi-line raw string literal, and the lines inside its interpolation holes that no alignment inside the hole places, are moved — FMT-RAW, which specifies that part of this phase.
- The text of documentation comments other than the column of their `///` markers — FMT-DOC.
- The column of a continuation line that no requirement below owns — for example the right operand of a wrapped binary expression, the expression after `=>` or `=` that stays on its own line, or the content of a parenthesized pattern that is not a property pattern. Such lines currently take the block column of the enclosing statement or declaration; indenting them one level deeper is a decided behavior change, tracked in thoenissen/Reihitsu#953.
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

The formatter MUST indent the first token of every line by one level per enclosing scope: the braces of a block-scoped namespace, a type, a block, an accessor list, and a switch statement each add one level for what lies between them, and the braces themselves stay at their owner's level. A file-scoped namespace and top-level statements MUST NOT add a level. A labeled statement MUST be indented like any other statement. The first token of a line is the token that starts it as SHARED-BLANK-008 decides, also when the line break in front of it is the one that ends a single-line documentation comment (`///`) written behind code on the line above; the lines of that comment are its text and MUST keep their columns (FMT-DOC-003).

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

The formatter MUST indent a comment that starts its line — as decided by SHARED-BLANK-002 — to the column of the token it precedes. When that token stands behind another comment that leads the token's line, the line starts with that comment: the comment on its own line above takes the column the token's line starts in, which is the column of the leading comment, not the column of the token behind it (FMT-INDENT-030): in a method body, `  // above` above `            /* c */ M();` gives `        // above` above `        /* c */ M();`. A comment that precedes a closing brace MUST be indented one level deeper than the brace, inside the scope it closes. A comment that does not start its line, such as a delimited documentation comment written behind an opening brace, MUST be left where it is and MUST NOT receive indentation in the middle of its line.

Why: A comment on its own line describes the code below it and reads as part of that code's block. A comment at the end of a scope describes the scope's contents, not the brace. Indenting a comment that shares a line with code would insert whitespace into the middle of that line.

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

The formatter MUST indent an active `#region` or `#endregion` directive to the block indentation of the position it is in: the member column inside a type, the statement column inside a block, and the label column between the sections of a switch statement. A region directive in disabled text MUST NOT be touched.

Why: Regions group members or statements and belong to the structure they group, so they take that structure's column; a misindented region would otherwise look as if it belonged to an outer or inner scope. Disabled text is never rewritten (FMT-INDENT-011).

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

### FMT-INDENT-011 — Disabled text and literal content are never re-indented

The formatter MUST NOT change the whitespace inside disabled text or inside the text of a string or character literal, including the continuation lines of a verbatim string, and MUST NOT give a content line or the closing line of a multi-line raw string literal, nor a line inside one of its interpolation holes that FMT-RAW-007 moves only with the literal, a column from the requirements of this spec: such a line moves only as FMT-RAW-001 and FMT-RAW-007 move it. Only the other lines that start with a token, a comment, or a directive are re-indented; a line inside a hole that an alignment inside the hole places keeps that alignment.

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

When a parenthesized or bracketed list spans several lines — arguments, element-access arguments, parameters, bracketed parameters, attribute arguments, tuple elements, tuple type elements, and unmanaged calling conventions — the formatter MUST indent every element, every separator, and the closing delimiter that starts a line to the column directly after the opening delimiter.

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

### FMT-INDENT-013 — A line inside a list element aligns with the element

When a line starts inside an element of a list from FMT-INDENT-012, FMT-INDENT-015, or FMT-INDENT-016 at a token other than the element's first token, the formatter MUST indent it to the column of the element's first token. A line inside a construct that lays out its own lines — such as a nested list, an initializer, a switch expression, a lambda body, or an interpolation hole — MUST be left to that construct; the clauses of a query expression MUST align with the query's first token.

Why: Such a line exists only because a comment, a directive, or disabled text kept FMT-BREAK from joining it. Without this rule it would fall back to the statement's column and look like a new statement.

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

When an attribute list, a type argument list, a type parameter list, or a function-pointer parameter list spans several lines, the formatter MUST indent every later element, every separator, and the closing bracket that starts a line to the column of the list's first element. A list whose first element is an omitted type argument (`Dictionary<,>`) and that stays wrapped, because a comment or a directive in it keeps FMT-BREAK-014 from joining it, MUST keep block indentation.

Why: These lists open with a bracket that sits directly in front of the first element, so the first element's column is the natural anchor. An omitted type argument has no width and therefore no column to align to.

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
        string> _map;
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
                                          string> _map;
}
```

A list of omitted type arguments that a comment keeps wrapped keeps block indentation:

```csharp
class C
{
    void M()
    {
        var type = typeof(System.Collections.Generic.Dictionary<, // c
                                                                >);
    }
}
```

```csharp
class C
{
    void M()
    {
        var type = typeof(System.Collections.Generic.Dictionary<, // c
        >);
    }
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

For a nested initializer assigned to a member (`Child = { … }`), the formatter MUST indent an opening brace that starts a line to the column of the assigned member, and the closing brace to the same column as the opening brace. For an array initializer of a declaration (`int[] a = { … }`) and for a complex element initializer (`{ 1, 2 }` inside a collection initializer), the closing brace MUST be indented to the column of the opening brace. In every case the members that start a line MUST be indented one level deeper than the opening brace's column.

When a comment leads the line of the opening brace, the comment takes the column that line starts in — for a nested initializer assigned to a member, the column of the assigned member (FMT-INDENT-006) — and the column of the opening brace is the column it stands in behind the comment (FMT-INDENT-030): the closing brace and the members follow that column, not the comment's. When the comment stands on a line above the brace, the brace starts its line, and its column is the column of that line.

Why: Without a `new` keyword the target of the assignment is the closest token that owns the initializer; when the brace stays on the line of `=`, the brace itself is the anchor.

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

Example — a comment leads the line of a nested initializer's opening brace; the comment takes the column of `Child`, 20, the brace stands in column 28, and the member and the closing brace follow the brace:

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
                    /* c */ {
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

    public P Child { get; set; } = new P();
}

class C
{
    void M()
    {
        var p = new P
                {
                    Child =
                    /* c */ {
                                X = 2
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

### FMT-INDENT-025 — Collection expressions and list patterns indent from their bracket

When a collection expression or a list pattern spans several lines, the formatter MUST indent every element that starts a line one level deeper than the opening bracket and a closing bracket that starts a line to the column of the opening bracket.

Why: The opening bracket usually stays at the end of the line that introduces it, so it is the anchor of the block it opens, as with FMT-INDENT-023.

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

### FMT-INDENT-026 — Property patterns and parenthesized patterns align to their introducer

When the braces of a property pattern or the parentheses of a parenthesized pattern span several lines, the formatter MUST indent the delimiters that start a line to the pattern's anchor column, and the subpatterns of a property pattern one level deeper. The anchor column MUST be, skipping `and`, `or`, and `not` wrappers: the `is` keyword for a pattern of an `is` expression, the first token of the subpattern for a nested pattern, one level deeper than the opening parenthesis for a pattern inside a parenthesized pattern, and otherwise the pattern's own first token.

Why: A multi-line pattern reads as a block that belongs to the construct introducing it; anchoring to `is` or to the owning subpattern keeps that block next to its introducer however far right the pattern starts.

Example:

```csharp
class C
{
    bool M(object value)
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
    bool M(object value)
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

This holds also when the anchor is itself the first token of its line, such as the opening brace of a complex element initializer or of a nested initializer (FMT-INDENT-023), the opening bracket of a collection expression (FMT-INDENT-025), the `new` keyword of a creation expression (FMT-INDENT-022), the first token of a governing expression (FMT-INDENT-027), or the left operand of a wrapped binary expression (FMT-INDENT-018): the comment takes the column that the requirement placing the line gives its start, and the anchor is measured behind the comment, where it stands. A token that starts its line behind such a comment but anchors nothing — such as the opening brace of a creation initializer, whose members align to `new` (FMT-INDENT-022) — moves no other line.

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

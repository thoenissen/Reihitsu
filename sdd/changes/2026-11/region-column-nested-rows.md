# A region inside an initializer, a pattern, or a wrapped list takes the elements' column; the rows of a multi-dimensional array are laid out like complex element initializers

| | |
|---|---|
| Status | Draft |
| Date | 2026-11 |
| Affected requirements | FMT-INDENT-010 (changed), FMT-INDENT-023 (changed) |

## What

**Region directives.** An active `#region` or `#endregion` directive whose line stands inside an initializer, an anonymous object, a collection expression, a list pattern, a positional pattern, a property pattern, a parenthesized pattern, a switch expression, or a list that FMT-INDENT-012 aligns (arguments, bracketed arguments, parameters, attribute arguments, tuple elements) is indented to the column of that construct's elements — the column a comment on its own line takes in the same position (FMT-INDENT-006) — instead of the block indentation of the enclosing statement or member. In front of the construct's closing delimiter the directive takes the column of the elements that delimiter closes. The innermost construct or block that holds the directive decides, so a directive in the block body of a lambda passed as an argument keeps that block's statement column. Positions in types, blocks, and switch statements keep their columns. FMT-INDENT-010 also names the positions it decides and leaves the column inside other constructs that are not block scopes, such as a wrapped method chain, unspecified.

**Rows of a multi-dimensional array.** An array initializer whose parent is another array initializer — a row `{ 1, 2 }` of `int[,] a = { … }` or `new int[,] { … }` — is laid out as FMT-INDENT-023 lays out a complex element initializer: its closing brace in the column of its opening brace, its elements one level deeper than that brace, and, when a comment leads the line of the opening brace, the brace's own column behind the comment as the anchor. Until now the elements stood one level deeper than the line the row starts on and the closing brace stood with them.

## Why

The maintainer decided both cases in round 9 of the SDD open questions (thoenissen/Reihitsu#912, items 20 and 21, both option b; sources thoenissen/Reihitsu#1370 Q-I1 and Q-I2), tracked in thoenissen/Reihitsu#1399.

FMT-INDENT-010's own reason is that a region belongs to the structure it groups and must not look as if it belonged to an outer scope. Inside an initializer, a pattern, a switch expression, or a wrapped list, the grouped elements stand at an aligned column, not at the block indentation, so the block indentation puts the directive far to the left of what it groups: 8 columns in an object initializer whose members stand at 20. Comments on their own line in those positions already follow the elements (FMT-INDENT-006); the directives now do the same.

A row of a multi-dimensional array matched none of the anchors FMT-INDENT-022 and FMT-INDENT-023 name, so its closing brace stood with its elements, one level right of its opening brace. A row is a brace-delimited group of elements with neither a keyword nor a target, which is exactly the shape of a complex element initializer.

The issue reported, for the rows, that the elements and the closing brace fell to the statement column. On this branch's base the elements already stand one level deeper than the row's line (the continuation indentation of FMT-INDENT-034), and only the closing brace and the comment-led shape still differ from a complex element initializer.

## Visible output changes

Each "before" is the output of `scripts/format.sh` on this branch's base (4a91817, whose production code is that of 13b0ed2), and every "before" is stable under a second run. Each "after" is the layout this change specifies; it is not built yet. The blank lines around the directives and the `// Name` behind `#endregion` come from the blank-line and region phases and do not change. Only the lines that move are shown in full; the surrounding declarations are those of the input.

### 1. Object initializer

```csharp
class P
{
    public int X { get; set; }

    public int Y { get; set; }
}

class C
{
    void M()
    {
        var p = new P
                {
                    #region X
                    X = 1,
                    #endregion
                    Y = 2
                };
    }
}
```

Before:

```csharp
        var p = new P
                {
        #region X

                    X = 1,

        #endregion // X

                    Y = 2
                };
```

After:

```csharp
        var p = new P
                {
                    #region X

                    X = 1,

                    #endregion // X

                    Y = 2
                };
```

The same holds in a collection initializer (`new List<int> { #region R 1, #endregion 2 }`: before at 8, after at 20, the elements' column) and in an anonymous object (`new { #region R X = 1, #endregion Y = 2 }`: before at 8, after at 20).

### 2. Collection expression

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
    int[] F = [

    #region R

                  1,

    #endregion // R

                  2
              ];
```

After:

```csharp
    int[] F = [

                  #region R

                  1,

                  #endregion // R

                  2
              ];
```

### 3. Switch expression

```csharp
class C
{
    int M(int x)
    {
        return x switch
               {
                   #region R
                   1 => 1,
                   #endregion
                   _ => 0
               };
    }
}
```

Before:

```csharp
        return x switch
               {
        #region R

                   1 => 1,

        #endregion // R

                   _ => 0
               };
```

After:

```csharp
        return x switch
               {
                   #region R

                   1 => 1,

                   #endregion // R

                   _ => 0
               };
```

### 4. Argument list

```csharp
class C
{
    void N(int a, int b, int c)
    {
    }

    void M()
    {
        N(1,
          #region R
          2,
          #endregion
          3);
    }
}
```

Before:

```csharp
        N(1,

        #region R

          2,

        #endregion // R

          3);
```

After:

```csharp
        N(1,

          #region R

          2,

          #endregion // R

          3);
```

### 5. List pattern, property pattern, parenthesized pattern

Three inputs, each the only statement of its method; `P` is the class of entry 1:

```csharp
    bool M(int[] a)
    {
        return a is [
                        #region R
                        1,
                        #endregion
                        2
                    ];
    }
```

```csharp
    bool M(P p)
    {
        return p is {
                        #region R
                        X: 1,
                        #endregion
                        Y: 2
                    };
    }
```

```csharp
    bool M(int x)
    {
        return x is (
                        #region R
                        > 1
                        #endregion
                        and < 5
                    );
    }
```

Before: every directive at column 8.

```csharp
        return a is [

        #region R

                        1,

        #endregion // R

                        2
                    ];
```

```csharp
        return p is
                 {
        #region R

                     X: 1,

        #endregion // R

                     Y: 2
                 };
```

```csharp
        return x is
                 (

        #region R

                     > 1

        #endregion // R

                     and < 5
                 );
```

After: the directives at the column of the elements, subpatterns, and the pattern inside the parentheses — 24, 21, and 21.

```csharp
        return a is [

                        #region R

                        1,

                        #endregion // R

                        2
                    ];
```

```csharp
        return p is
                 {
                     #region R

                     X: 1,

                     #endregion // R

                     Y: 2
                 };
```

```csharp
        return x is
                 (

                     #region R

                     > 1

                     #endregion // R

                     and < 5
                 );
```

### 6. `#endregion` in front of a closing delimiter

```csharp
class P
{
    public int X { get; set; }
}

class C
{
    void N(int a)
    {
    }

    void M()
    {
        var p = new P
                {
                    #region X
                    X = 1
                    #endregion
                };
        int[] a = [
                      #region R
                      1
                      #endregion
                  ];
        N(
          #region S
          1
          #endregion
          );
    }
}
```

Before: every directive at column 8.

```csharp
        var p = new P
                {
        #region X

                    X = 1

        #endregion // X
                };
        int[] a = [

        #region R

                      1

        #endregion // R

                  ];
        N(

        #region S

          1

        #endregion // S

          );
```

After: each `#endregion` in the column of the elements its delimiter closes, like its `#region`.

```csharp
        var p = new P
                {
                    #region X

                    X = 1

                    #endregion // X
                };
        int[] a = [

                      #region R

                      1

                      #endregion // R

                  ];
        N(

          #region S

          1

          #endregion // S

          );
```

### 7. The other lists FMT-INDENT-012 aligns: parameters, tuple elements, bracketed arguments, attribute arguments

The issue and the decision name argument lists; the same column rule is applied to every list FMT-INDENT-012 aligns, since they share the alignment.

```csharp
class A : System.Attribute
{
    public A(int a, int b)
    {
    }
}

class C
{
    void M(int a,
           #region R
           int b)
           #endregion
    {
        var t = (1,
                 #region T
                 2,
                 #endregion
                 3);
        var d = new int[3];
        var e = d[0,
                  #region E
                  1];
                  #endregion
    }

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
    void M(int a,

    #region R

           int b)

    #endregion // R

    {
        var t = (1,

        #region T

                 2,

        #endregion // T

                 3);
        var d = new int[3];
        var e = d[0,

        #region E

                  1];

        #endregion // E
    }

    [A(1,

    #region At

       2)]

    #endregion // At

    void N()
```

After: each `#region` at the column of the element behind it (11, 17, 18, 7); each `#endregion` that stands behind the list's closing delimiter is outside the list and keeps its column.

```csharp
    void M(int a,

           #region R

           int b)

    #endregion // R

    {
        var t = (1,

                 #region T

                 2,

                 #endregion // T

                 3);
        var d = new int[3];
        var e = d[0,

                  #region E

                  1];

        #endregion // E
    }

    [A(1,

       #region At

       2)]

    #endregion // At

    void N()
```

### 8. Rows of a multi-dimensional array

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
    }
}
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

After:

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

The same holds for the rows of `new int[,] { … }` (before: `}` at 24 under the elements, after: `}` at 20 under the row's `{`) and for each level of a three-dimensional array:

```csharp
        int[,,] c = {
                        {
                        {
                        1
                        }
                        }
                    };
```

Before:

```csharp
        int[,,] c = {
                        {
                            {
                                1
                                }
                            }
                    };
```

After:

```csharp
        int[,,] c = {
                        {
                            {
                                1
                            }
                        }
                    };
```

### 9. A row that opens behind another token

```csharp
class C
{
    void M()
    {
        int[,] b = { {
        1, 2
        }, {
        3, 4
        } };
    }
}
```

Before:

```csharp
        int[,] b = { {
            1, 2
            }, {
                3, 4
                } };
```

After: each row's elements one level deeper than the column its opening brace stands in (21, then 24), and its closing brace in that column.

```csharp
        int[,] b = { {
                         1, 2
                     }, {
                            3, 4
                        } };
```

### 10. A row whose opening brace is led by a comment

```csharp
class C
{
    int[,] F = {
                   /* c */ {
                   3,
                   4
                   }
               };
}
```

Before:

```csharp
    int[,] F = {
                   /* c */ {
                       3,
                       4
                       }
               };
```

After: the elements and the closing brace follow the brace's column, 27, not the comment's (as FMT-INDENT-023 already states for a complex element initializer).

```csharp
    int[,] F = {
                   /* c */ {
                               3,
                               4
                           }
               };
```

### Code-fix results

The code fixes that format a node holding such a construct give the same columns, measured from where the node stands. Run through `scripts/apply-fix.sh` on the base:

- RH5305 (collection expressions), on `int[] a = [⏎    #region R⏎    1,⏎    #endregion⏎    2⏎    ];` in a method body: before, the directives at 18 (the column of `[`) and the elements at 22; after, the directives at 22.
- RH5206 (switch-expression braces), on `return x switch⏎{⏎#region R ... };` with the braces at 8: before, the directives at 15 (the column of the braces) and the arms at 19; after, the directives at 19.
- RH5102 offers no fix when a directive stands in the argument list, on the base and after the change.

Every other code fix whose formatted node holds a region directive in one of these constructs, or a row of a multi-dimensional array, gives the new columns wherever it offers a fix for such a node; the fixes that format such constructs are RH5101, RH5304, RH5306, and RH5307 besides the two above, and the fixes that format a whole member or type. Those were not run.

## Decisions

### The directive follows the comment rule of its position

A region directive inside these constructs takes the column FMT-INDENT-006 gives a comment on its own line in the same position. That states the column once, for the element in front of which the directive stands and for the closing delimiter, and it already agrees with FMT-INDENT-010 in types, blocks, and switch statements, where the comment rule and the block indentation give the same column.

### The constructs are named by the requirements that align them

The decision names initializers, collection expressions, switch expressions, and argument lists; the issue's later comments add anonymous objects, list patterns, and parenthesized patterns. FMT-INDENT-010 names the constructs by the alignment requirements that place their elements (FMT-INDENT-012 and FMT-INDENT-022 to FMT-INDENT-027), so property patterns, positional patterns, and every list of FMT-INDENT-012 follow too, and leaves every other construct that is not a block scope unspecified rather than claiming the statement column for it.

### A row is an array initializer whose parent is an array initializer

The row is recognized by its parent in the syntax tree. An array creation inside an array initializer (`new[] { 5 }` in a jagged array) keeps its `new` anchor (FMT-INDENT-022).

## Alternatives considered

- **Keep the statement column (round 9, option a)** — rejected by the maintainer's decision: the directive would keep standing far left of what it groups.
- **Leave the directive's column as written inside these constructs** — rejected: the input's column would survive formatting, so two files with the same structure would format differently, and the decided column is a cheap read of the column the comment rule already computes.
- **The column of the construct's opening delimiter** — rejected: it puts the directive one level left of the elements it groups, which is what node-level formatting writes today (*Code-fix results*).
- **Give a row the continuation indentation and only align its closing brace with the elements' line start** — rejected: the decision lays out a row like a complex element initializer, whose brace is its anchor.

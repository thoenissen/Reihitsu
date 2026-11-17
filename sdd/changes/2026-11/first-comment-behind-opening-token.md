# No blank line above the first comment behind an opening parenthesis, bracket, or `<`

| | |
|---|---|
| Status | Accepted |
| Date | 2026-11 |
| Affected requirements | FMT-BLANK-011 (changed: the exemption names every opening token; clarified: every opening brace), RH5020-002 (changed: title, token list, and why), RH8303-002 (changed: title, token list — the same opening tokens as FMT-BLANK-011 —, and why) |

## What

FMT-BLANK-011 puts one blank line above a comment on its own line, except above the first content of a block, a type body, or a switch section. With this change the exemption covers the first content behind every opening token: an opening brace of any construct, as the code already does today, and in addition an opening parenthesis `(` or opening bracket `[` of any construct and the `<` that opens a type parameter list, a type argument list, or a function pointer parameter list. Above such a comment the formatter inserts no blank line. It removes none either: an author's blank line behind `(`, `[`, or `<` stays, as FMT-BLANK-019 already keeps it behind the opening bracket of a collection expression.

RH5020 and RH8303, which the formatter mirrors, exempt the same comments: RH5020-002 and RH8303-002 name the same opening tokens, so neither rule reports a comment the formatter leaves without a blank line, and their code fixes no longer insert one there.

A comment behind a comma, an operator, or any other token that opens nothing keeps its blank line, inside a list as anywhere else.

This implements thoenissen/Reihitsu#1781, which asked for the exemption behind the opening parenthesis of a parameter list and of a union's case-type list. The maintainer chose to widen it to every opening parenthesis, bracket, and generic `<` (*Decisions*).

## Why

Behind an opening token nothing of the construct stands above the comment, so the token already provides the boundary a blank line would provide, as the `{` of a block does. Today the formatter writes:

```csharp
public record R(

                // first
                int A,
                int B);
```

The blank line separates the comment from nothing and detaches it from the parameter it describes.

The formatter already exempts the first content behind every opening brace — of an initializer, an anonymous object, a property pattern, a switch expression, as of a block — and RH5020 exempts the same comments (RH5020-002 names "an opening brace `{`"). FMT-BLANK-011 named only the block, the type body, and the switch section; this change states the brace case as the code implements it, and extends the same rule to the other opening tokens.

The issue's own construct is a comment inside a parameter list, which RH5035 reports for every parameter list, the case-type list of a union and an indexer's bracketed list included, and which the user is asked to remove; RH5035 keeps reporting it. The case people commonly write is the first element of a collection expression or a list pattern, where no rule reports a comment and the formatter writes the same blank line:

```csharp
int[] F = [

              // first
              1,
              2
          ];
```

The rule rests on consistency, not on cost: it is the rule the formatter and both analyzers already apply behind every opening brace, extended to the other opening tokens, and it reaches the collection expression and the list pattern, which a rule limited to parameter lists would leave as they are. It is a check of the kind of the token in front of the comment's token, plus the parent kind for `<`, which is an opening token only in a type parameter list, a type argument list, or a function pointer parameter list.

## Visible output changes

Every *before* below is the output of `main` at 869c867; every *after* is the same output without the blank line above the comment. Indentation and line breaks do not change.

### 1. Formatter — the first comment in a parameter list, a union's case-type list, and an argument list

```csharp
// input
public record R(
    // first
    int A,
    int B);

public union U(
    // first
    int,
    string);

class C
{
    int M(
        // first
        int a,
        int b)
    {
        return M(
            // first
            a,
            b);
    }
}
```

```csharp
// before
public record R(

                // first
                int A,
                int B);

public union U(

               // first
               int,
               string);

class C
{
    int M(

          // first
          int a,
          int b)
    {
        return M(

                 // first
                 a,
                 b);
    }
}
```

```csharp
// after
public record R(
                // first
                int A,
                int B);

public union U(
               // first
               int,
               string);

class C
{
    int M(
          // first
          int a,
          int b)
    {
        return M(
                 // first
                 a,
                 b);
    }
}
```

### 2. Formatter — the first element of a collection expression and of a list pattern

```csharp
// input
class C
{
    int[] F = [
        // first
        1,
        2
    ];

    bool N(int[] a)
    {
        return a is [
            // first
            1,
            ..
        ];
    }
}
```

```csharp
// before
class C
{
    int[] F = [

                  // first
                  1,
                  2
              ];

    bool N(int[] a)
    {
        return a is [

                        // first
                        1,
                        ..
                    ];
    }
}
```

```csharp
// after
class C
{
    int[] F = [
                  // first
                  1,
                  2
              ];

    bool N(int[] a)
    {
        return a is [
                        // first
                        1,
                        ..
                    ];
    }
}
```

### 3. Formatter — the other opening tokens: an attribute argument list, an indexer's parameter list, a type parameter list, a tuple, an array size, and an element access

```csharp
// input
using System;

class C
{
    [Obsolete(
        // first
        "x")]
    int this[
        // first
        int i] => i;

    void M<
        // first
        T>()
    {
        var t = (
            // first
            1,
            2);
        var a = new int[
            // first
            3];
        _ = a[
            // first
            0];
        _ = t;
    }
}
```

```csharp
// before
using System;

class C
{
    [Obsolete(

              // first
              "x")]
    int this[

             // first
             int i] => i;

    void M<

    // first
    T>()
    {
        var t = (

                 // first
                 1,
                 2);
        var a = new int[

        // first
        3];
        _ = a[

              // first
              0];
        _ = t;
    }
}
```

```csharp
// after
using System;

class C
{
    [Obsolete(
              // first
              "x")]
    int this[
             // first
             int i] => i;

    void M<
    // first
    T>()
    {
        var t = (
                 // first
                 1,
                 2);
        var a = new int[
        // first
        3];
        _ = a[
              // first
              0];
        _ = t;
    }
}
```

The same holds behind every other opening parenthesis — a statement header such as `if (`, a cast, a parenthesized expression, a positional or parenthesized pattern — and behind the opening bracket of an attribute list.

### 4. Formatter — a documentation comment that is the first content behind an opening token

A `///` comment on a line of its own, and a `/** */` comment written behind the opening parenthesis, which the formatter moves onto a line of its own as before:

```csharp
// input
public record R(
    /// <summary>first</summary>
    int A,
    int B);

class C
{
    void M(/** first */ int a, int b)
    {
    }
}
```

```csharp
// before
public record R(

                /// <summary>
                /// first
                /// </summary>
                int A,
                int B);

class C
{
    void M(

           /** first */ int a,
           int b)
    {
    }
}
```

```csharp
// after
public record R(
                /// <summary>
                /// first
                /// </summary>
                int A,
                int B);

class C
{
    void M(
           /** first */ int a,
           int b)
    {
    }
}
```

### 5. RH5020 · Comments should be preceded by a blank line

Input: the input of entries 1 to 3.

Before: RH5020 reports each `// first`, and its code fix inserts the blank line above it.

After: not reported.

A comment behind a comma is still reported:

```csharp
class C
{
    void M(int a,
           {|RH5020:// second|}
           int b)
    {
    }
}
```

### 6. RH8303 · Element documentation header must be preceded by blank line

```csharp
public record R(
    /// <summary>first</summary>
    int A,
    int B);

class C
{
    void M<
        /// <summary>first</summary>
        T>()
    {
    }
}
```

Before: RH8303 reports both `///`, and its code fix inserts the blank line above each.

After: not reported. Documentation behind a comma is still reported (`record R(int A,⏎{|RH8303:///|} <summary>second</summary>⏎int B);`).

### 7. Code fixes that format a node holding such a comment

The RH3004 code fix (RH3004 · Use lambda syntax) on an anonymous method whose parameter list starts with a comment:

```csharp
// input
using System;

class C
{
    void M()
    {
        Func<int, int> f = delegate(
            // the value
            int a)
        {
            return a;
        };
        _ = f;
    }
}
```

```csharp
// before
        Func<int, int> f = (

                            // the value
                            int a) => a;
```

```csharp
// after
        Func<int, int> f = (
                            // the value
                            int a) => a;
```

The RH5305 code fix (RH5305 · Collection expressions should be formatted correctly):

```csharp
// input
class C
{
    int[] _a = [
        // first
        1, 2];
}
```

```csharp
// before
    int[] _a = [

                   // first
                   1,
                   2
               ];
```

```csharp
// after
    int[] _a = [
                   // first
                   1,
                   2
               ];
```

### 8. Rule page

`documentation/rules/RH5020.md`, *Exceptions*: the bullet "the first thing inside a block" gains "the first thing behind an opening parenthesis or bracket, such as the first element of a parameter list, an argument list, or a collection expression".

`documentation/rules/RH8303.md`, *Exemptions*: the first of the three positions, "Documentation that opens a block, directly after the opening brace …", becomes "Documentation that is the first thing inside a block, directly after the opening brace of the type, namespace, or member list, or the first thing behind an opening parenthesis or bracket, such as the first element of a parameter list, even when a comment follows that brace, parenthesis, or bracket on its line". The page keeps three positions.

### Unchanged

- A comment behind a comma or an operator keeps its blank line, and RH5020 and RH8303 keep reporting it without one (entries 5 and 6).
- A blank line the author wrote behind `(`, `[`, or `<` stays; behind `{` FMT-BLANK-007 removes it, as before.
- A comment on the line of the opening token (`M( // c`) trails that token and gets no blank line, as before.
- RH5035, RH5038, RH5044, and RH5046 keep reporting a comment inside a parameter list, an argument list, an attribute list, a type parameter or type argument list, and a tuple.
- The first content behind an opening brace — of a block, a type body, an initializer, an anonymous object — gets no blank line, as before; FMT-BLANK-011 now says so for every brace.

## Decisions

### The maintainer's answer to the run's output question

On 2026-11-16 the maintainer chose option C of output question 1 (a comment that is the first content behind an opening parenthesis, bracket, or `<`): no blank line between any opening `(`, `[`, or generic `<` and a comment that is its first content, for every construct — parameter, argument, attribute argument, type parameter, and type argument lists, tuples, collection expressions, list patterns, element access, array sizes, statement headers, casts, and parenthesized expressions and patterns. A comment behind a comma keeps its blank line, an author's blank line behind the opening token stays, and documentation comments (`///`, `/** */`) follow the same answer (visible output changes 1 to 4 and 7). The rejected options were the issue's A (parameter lists only) and B (keep today's output); see *Alternatives considered*.

The spec is written on this answer.

### Every opening token, not only a parameter list's

The issue's reason — the opening parenthesis already provides the boundary, as a block's brace does — holds for every opening parenthesis, bracket, and generic `<`, and the code already applies it to every opening brace. Limiting the exemption to parameter lists would change output only for comments RH5035 asks the user to remove, and leave the collection expression, where such a comment is commonly written and allowed, with the blank line. The generic `<` is included although it is reached only by code RH5046 reports, so that `M<⏎// c⏎T>` and `M(⏎// c⏎a` follow one rule.

### RH5020 and RH8303 change with the formatter

The analyzer change follows from the answer above and was not asked separately: the question's option C states it, and an unchanged requirement decides it. The formatter mirrors RH5020 and RH8303 (FMT-BLANK-011), and SHARED-DIAG-010 forbids a layout rule to report a shape the formatter keeps. RH5020-002 and RH8303-002 therefore name exactly the opening tokens FMT-BLANK-011 names: neither rule reports a comment the formatter leaves without a blank line, their code fixes insert none there, and both keep reporting a comment behind a comma, an operator, or any other token that opens nothing (visible output changes 5, 6, and 8).

### The author's blank line behind `(`, `[`, or `<` is kept

No requirement removes it today, and FMT-BLANK-019 keeps it behind the opening bracket of a collection expression by decision. Removing it would be a further output change that thoenissen/Reihitsu#1781 does not ask for.

## Alternatives considered

- **Exempt only a parameter list and a union's case-type list (the issue's option A)** — the same change limited to the `(` of a parameter list, the `[` of an indexer's parameter list, and a union's case-type list; entries 2, 3 (but the indexer), and most of 7 stay as *before*. Rejected by the maintainer (option A): it changes output only for code RH5035 reports, and it leaves the commonly written collection-expression case unequal to an initializer's brace.
- **Keep today's output (the issue's option B)** — rejected by the maintainer (option B): the blank line detaches the comment from the element it describes and separates it from nothing.
- **One shared requirement for the formatter and both analyzers** — rejected: SHARED-BLANK leaves the comment boundary to RH5020 and FMT-BLANK by its non-goals, and each states it separately today.

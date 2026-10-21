# Shared — Comment placement

| | |
|---|---|
| ID prefix | SHARED-COMMENT |
| Status | Active |
| Code owners | `CommentPositionAnalyzerBase`, `CommentRegionAnalyzerBase`, `CommentPositionUtilities`, `FluentChainAnalysisHelper` |
| Uses | — |

This spec covers the rules that report a comment placed in a position inside a declaration or a statement: which comments they check, where the diagnostic is, and, for the rules that report every comment inside a delimited region such as an argument list or a statement header, which gaps of the region count and which comment positions inside it stay valid. Each rule that uses this spec states the positions it reports, or the regions it checks, and any exception of its own.

It also states two things every rule shares: which comments are ordinary comments, every kind alike (SHARED-COMMENT-001), and how a rule that checks where a line starts treats a line that starts with a comment (SHARED-COMMENT-011).

## Non-goals

- Which positions or regions a rule checks, and its own exceptions — each rule's spec.
- A comment before the first call of a wrapped fluent chain — RH5048 and SHARED-CHAIN.
- Comments next to a token the parser inserted for a syntax error.
- Moving a comment. These rules have no code fix; only the author can decide what a comment refers to.

## Requirements

### SHARED-COMMENT-001 — Only ordinary comments are checked, every kind alike

A rule MUST check only ordinary comments, and MUST treat every kind of them alike: a `//` comment, a `/* */` comment, and a `///` or `/** */` comment that documents nothing. It MUST NOT report a documentation comment that documents a declaration. A `///` or `/** */` comment documents a declaration exactly when the compiler reads it as that declaration's documentation and does not report it as misplaced (CS1587). The compiler reads documentation only in front of a type, a delegate, a member, or an enum member, ahead of the declaration's attribute lists and modifiers; a `///` or `/** */` comment anywhere else documents nothing — behind an attribute list, inside an argument list or a statement, in front of a namespace, an accessor, a parameter, a local function, or a statement, or behind the last member of a type.

The verdict MUST NOT depend on whether the compiler parses documentation comments. When it does not, a comment counts as a `///` or `/** */` comment by its text — a `//` comment that starts with exactly three slashes, and a `/* */` comment that starts with `/**` and is not the empty comment `/**/` — and it documents a declaration exactly where it would if documentation comments were parsed.

Wherever a rule distinguishes a line that holds only a comment from a line that holds code, a line holding only a `/* */` comment MUST count like a line holding only a `//` comment.

Why: A documentation comment belongs to the declaration it documents; it is not a remark on the code around it. A `///` or `/** */` comment that documents nothing belongs to no declaration — the compiler discards it — so it is a remark like any other comment, however its delimiters are written. Whether the compiler parses documentation comments is a project setting; the same source must not change its diagnostics with it. A line that holds only a comment reads as a comment line whichever delimiters the comment uses.

Example — RH5033, where every comment behind an attribute list documents nothing:

```csharp
using System;

class C
{
    [Obsolete]
    {|RH5033:/// <summary>M</summary>|}
    void M()
    {
    }

    [Obsolete]
    {|RH5033:/** <summary>N</summary> */|}
    void N()
    {
    }

    [Obsolete]
    {|RH5033:/**/|}
    void O()
    {
    }
}
```

With documentation comments not parsed (`DocumentationMode.None`), RH5033 reports exactly the same comments, and it reports `//// note` in the same position, because it starts with four slashes.

Example — RH5038, where a `///` comment inside an argument list documents nothing:

```csharp
class C
{
    int M(params object[] values) => 0;

    void N()
    {
        M(
            {|RH5038:/// first|}
            1);
    }
}
```

Example — RH5034, where both documentation comments document `M`:

```csharp
class C
{
    /// <summary>M</summary>
    /** <remarks>M</remarks> */
    void M()
    {
    }
}
```

is not reported, whether or not documentation comments are parsed.

Example — RH5104, whose fix inserts no blank line between a comment line and the comment it moves below it:

```csharp
class C
{
    void N()
    {
        M();
        /* note */
        M(); {|RH5104:// next|}
    }

    void M()
    {
    }
}
```

```csharp
class C
{
    void N()
    {
        M();
        /* note */
        // next
        M();
    }

    void M()
    {
    }
}
```

### SHARED-COMMENT-002 — A comment inside a preprocessor directive is not checked

A rule MUST NOT report a comment that is part of a preprocessor directive line, such as the comment behind `#if DEBUG`.

Why: The comment belongs to the directive, not to the code around it, and a directive cannot be moved out of the position it controls.

Example — RH5033:

```csharp
using System;

class C
{
    [Obsolete]
#if DEBUG // note
#endif
    void M()
    {
    }
}
```

is not reported.

### SHARED-COMMENT-003 — Each comment is reported on its own

A rule MUST report one diagnostic for each comment in a position it reports, located on that comment, whether the comment stands on a line of its own or shares a line with code. Two comments in the same gap MUST be reported separately. A comment in a position that several rules report MUST be reported by each of them, each rule deciding on its own.

Why: Each comment has to be moved on its own, and the author may want to move one and delete the other. A comment behind code on the same line is just as misplaced as one on its own line. A comment inside, say, an argument list that is itself inside a statement header splits both constructs.

Example — RH5038:

```csharp
class C
{
    int M(params object[] values) => 0;

    void N()
    {
        M(1, {|RH5038:// first|}
          {|RH5038:/* second */|} 2);
    }
}
```

Example — RH5038 and RH5039, which both report the comment:

```csharp
class C
{
    int M(params object[] values) => 0;

    void N()
    {
        if (M(
            {|RH5038:// value|}
            1) > 0)
        {
        }
    }
}
```

RH5039 reports the same comment, because the argument list is part of the `if` header.

### SHARED-COMMENT-004 — A comment inside a region is reported

A rule that checks a region MUST report a comment when the token before the comment is the region's first token or a later one, and the token after the comment is the region's last token or an earlier one, except where SHARED-COMMENT-006, SHARED-COMMENT-008, or SHARED-COMMENT-009 keeps the comment valid. A comment before the region's first token or after its last token MUST NOT be reported, unless the rule's own spec reports it.

Why: A comment inside a single construct leaves it unclear which part of the construct it describes, and it splits the construct across lines, so its layout can no longer be kept consistent. A comment in front of or behind the construct describes the construct as a whole.

Example — RH5038, whose region is an argument list from its opening to its closing parenthesis:

```csharp
class C
{
    int M(params object[] values) => 0;

    void N()
    {
        M(
            {|RH5038:// first|}
            1,
            2
            {|RH5038:// last|}
            );
        M
            // before the list
            (1)
            // after the list
            ;
    }
}
```

### SHARED-COMMENT-005 — A gap at a region boundary or before an element belongs to the region

The gap directly before one of the region's own tokens — such as its delimiters, its keywords, and the separators between its elements — and the gap directly before the first token of one of its elements MUST belong to the region, even when the element starts with a construct that lays out its own lines (SHARED-COMMENT-006).

Why: A comment in front of an element or a separator stands between the elements of the region itself, not inside a nested construct, so it splits the region like any other comment there.

Example — RH5038:

```csharp
class C
{
    int M(params object[] values) => 0;

    void N(object o)
    {
        M(
            {|RH5038:// values|}
            new { First = 1 });
        M(1,
            {|RH5038:// classify|}
            o switch
            {
                _ => 1
            });
        M(
            {|RH5038:// items|}
            [1]);
        M(new { First = 1 }
            {|RH5038:// separator|}
            , 2);
    }
}
```

### SHARED-COMMENT-006 — A gap that a nested construct lays out belongs to that construct

A comment in any other gap inside a region MUST NOT be reported when the token after the comment is a token of a construct, nested in the region, that lays out the lines of its own content. These constructs, and the tokens of each that count, are:

- a block or another brace-delimited body, an accessor list, an object, collection, or array initializer, a list pattern, a parenthesized pattern, and an interpolation — every token, from the opening delimiter on;
- a collection expression — every token after its own opening bracket; the opening bracket of a collection expression nested in it is one of its tokens;
- the clauses of a query after its first `from` clause — every token;
- an anonymous object — every token from its `{` on;
- a switch expression — every token from its `switch` keyword on;
- a property or positional pattern — every token from its `{` or `(` on.

The region that decides is the innermost region of the rule whose gap contains the comment, so a comment inside a region nested in such a construct is reported for that nested region.

This requirement and SHARED-COMMENT-004 do not cover a comment directly before the `new` of an anonymous object, before a token of the expression a switch expression switches on, or before a token of the type in front of a property or positional pattern, unless that token starts an element of the region (SHARED-COMMENT-005).

Why: A nested construct with its own lines, such as a lambda block, is laid out on its own terms; a comment above a statement in it describes that statement, not the region around it. A lambda's expression body, a type test, or an operand of an operator lays out no lines of its own, so a comment there still splits the element.

Example — RH5038:

```csharp
using System;
using System.Linq;

class C
{
    int M(params object[] values) => 0;

    void N(object o, int[] items, int[][] jagged)
    {
        M((Func<int>)(() =>
        {
            // explanation
            return M(
                {|RH5038:// inner|}
                1);
        }));
        M(new
            // values
            {
                // first
                First = 1
            });
        M(o
            // classify
            switch
            {
                // text
                string => 1,
                _ => 2
            });
        M(from item in items
            // only positive
            where item > 0
            select item);
        M(from item
            {|RH5038:// source|}
            in items
            select item);
        M(items ?? [
            // first
            1]);
        M(items ??
            {|RH5038:// fallback|}
            [1]);
        M(jagged ?? [
            // inner
            [1]]);
        M((Func<int, int>)(value =>
            {|RH5038:// projection|}
            value));
    }
}
```

### SHARED-COMMENT-007 — What counts as a binary operator

A token MUST count as a binary operator exactly when it is the operator of a binary expression: an arithmetic, shift, relational, equality, bitwise, logical, or null-coalescing operator, and the `is` of a type test and the `as` of a conversion. The `is` of a pattern match, the operators inside a pattern, such as `and`, `or`, `not`, and `>`, and the `..` of a range MUST NOT count.

This requirement does not cover an `is` followed by a name that denotes a constant rather than a type, such as an enum member.

Why: A binary expression joins two operands, and a wrapped one breaks before its operator, so the operator starts the line of the operand it introduces. A pattern is not an operand, so its operators are not wrapped this way.

Example — RH5038, where a comment directly before a binary operator is not reported (SHARED-COMMENT-008):

```csharp
class C
{
    int M(params object[] values) => 0;

    void N(object o, int x)
    {
        M(x
            // shifted
            << 2);
        M(o
            // fallback
            ?? x);
        M(o
            // text check
            is string);
        M(o
            // conversion
            as string);
        M(o
            {|RH5038:// null check|}
            is null);
        M(o is int
            {|RH5038:// or long|}
            or long);
        M(x
            {|RH5038:// end|}
            ..2);
    }
}
```

### SHARED-COMMENT-008 — Inside a region, a comment before a binary operator stays valid

A rule that checks a region MUST NOT report a comment directly before a binary operator, as SHARED-COMMENT-007 defines it. A comment directly after a binary operator is not kept valid by this requirement; SHARED-COMMENT-004 and SHARED-COMMENT-006 decide it like any other comment of the region.

Why: A wrapped binary expression starts each continuation line with its operator, so a comment above that line describes the operand that follows it, the same way a comment above a statement describes the statement.

Example — RH5039, whose region is a statement header:

```csharp
class C
{
    void M(bool a, bool b)
    {
        if (a
            // why b
            && b)
        {
        }

        if (a &&
            {|RH5039:// why b|}
            b)
        {
        }
    }
}
```

### SHARED-COMMENT-009 — Inside a region, a comment before a later call of a chain stays valid

A rule that checks a region MUST NOT report a comment directly before a later call of a member-access chain: a member access that is invoked, written with `.`, `?.`, or `!.`, and preceded in its chain by another invoked member access. The comment is directly before the first character of the operator: the `.`, the `?` of `?.`, or the `!` of `!.`. A comment between the `!` and the `.` of `!.`, a comment before the chain's first call, a comment before a later member access that is not invoked, and a comment before an element access are not kept valid by this requirement; SHARED-COMMENT-004 and SHARED-COMMENT-006 decide them like any other comment of the region.

This requirement and SHARED-COMMENT-004 cover a comment inside a member-access chain only when every conditional access of the chain in front of the comment, and a conditional access directly after it, is followed by a call, as in `?.Where(…)`, and the comment is not directly before the `!` of a `!?.` link. They do not cover a comment directly before or directly after a postfix `++` or `--` that a member access follows, nor a comment before a member access written with `->`.

Why: A wrapped fluent chain starts each later call on its own line, so a comment above that line describes the call that follows it. The first call stays on the line of the receiver, and a property read or an element access is part of the element before it, so a comment in front of them still splits the chain.

Example — RH5038:

```csharp
using System.Linq;

class C
{
    int M(params object[] values) => 0;

    void N(int[] items, object o)
    {
        M(items.Where(value => value > 0)
                // materialize
                .ToList());
        M(items?.Where(value => value > 0)
                 // materialize
                 ?.ToList());
        M(o.ToString()
           // text
           !.Trim());
        M(o.ToString()!
            {|RH5038:// text|}
            .Trim());
        M(items
            {|RH5038:// only positive|}
            .Where(value => value > 0)
            .ToList());
        M(items.Where(value => value > 0).ToList()
            {|RH5038:// size|}
            .Count);
        M(items.Where(value => value > 0).ToArray()
            {|RH5038:// first|}
            [0]);
    }
}
```

### SHARED-COMMENT-010 — What a statement header is

The header of an `if`, `while`, `for`, `foreach`, `using`, `lock`, or `fixed` statement MUST run from the statement keyword — or from the `await` keyword of an `await foreach` or `await using` statement — to the closing parenthesis, and the statement's embedded statement MUST count as its body. A `foreach` statement that deconstructs its element counts too. Other statements and clauses MUST NOT count as having a header, such as a `switch` statement, a `catch` clause, the `while` of a `do` statement, `else`, `try`, `finally`, `checked`, and `unchecked`.

Why: These statements put a condition, a loop declaration, or a resource between a keyword and an embedded statement, and that pair reads as one line of thought. The other statements either have no header, or follow it with something other than an embedded statement, such as the sections of a `switch` or the block of a `catch`.

Example — RH5039, which checks the header, and RH5036, which checks the gap between header and body:

```csharp
using System.Collections.Generic;
using System.Threading.Tasks;

class C
{
    async Task M(bool a, IAsyncEnumerable<int> values, (int, int)[] pairs)
    {
        await {|RH5039:/* each */|} foreach (var value in values) {|RH5036:// body|}
        {
        }

        foreach (var (first, second) in pairs) {|RH5036:// body|}
        {
        }

        switch (/* value */ a) // body
        {
            default:
                break;
        }
    }
}
```

The comments of the `switch` statement are not reported.

### SHARED-COMMENT-011 — A line that starts with a comment is not judged by the token behind it

A rule that checks where a line starts — whether a token starts its line, or in which column a line starts — MUST take a comment that starts the line as the line's first element, whatever kind of comment it is. A line whose first element is a comment MUST NOT be reported for the position of the token behind that comment: neither for the token not starting its line nor for the token's column. A comment that does not start the line — one behind a token on the line, or one on a line of its own above it — MUST NOT change how the line is judged. This requirement does not cover a line whose leading comment is itself not where the rule asks the line to start.

Why: The first element of a line is what the reader sees at its start, so the layout of the line is measured there. The token behind a leading comment cannot be moved onto another line or into another column without moving the comment as well; a diagnostic on it would have no fix that clears it. Whether the comment may stand there at all is decided by the rules that report comments.

Example — RH5110, RH5201, RH5203, RH5204, and RH5206:

```csharp
using System.Linq;

class C
{
    /* a */   private int _a;

    void M<T>()
        /* c */ where T : class
    {
    }

    string N(int status, int[] items)
    {
        P(1,
          /* two */ 2);
        var list = items.Where(x => x > 0)
                        /* all */   .ToList();
        var a = status switch
                {
                    /* idle */ 0 => "Idle",
                    _ => "Other"
                };

        return a;
    }

    void P(params int[] values)
    {
    }
}
```

is not reported. A comment that does not start the line leaves the line to its first token:

```csharp
class C
{
    void N()
    {
        P(1, /* one */
            {|RH5203:2|});
        P(1,
          // two
            {|RH5203:2|});
    }

    void P(params int[] values)
    {
    }
}
```

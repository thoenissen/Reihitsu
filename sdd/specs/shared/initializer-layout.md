# Shared — Initializer layout

| | |
|---|---|
| ID prefix | SHARED-INIT |
| Status | Active |
| Code owners | `InitializerLayoutAnalysisUtilities` |
| Uses | — |

This spec covers what the rules that check the layout of object and collection initializers share: which column a line holding a comment in front of a brace or an element counts as, and when a complex element initializer (`{ 1, 2 }` inside a collection initializer) is laid out correctly. Each rule that uses this spec states which initializers it checks, the columns it expects, and where it reports.

## Non-goals

- Which initializers a rule checks, the column it expects for a brace or an element, and where its diagnostic is — each rule's spec, such as RH5301, RH5303, and RH5304.
- Array initializers, anonymous object creations, collection expressions, and list patterns.

## Requirements

### SHARED-INIT-001 — A comment in front of a token anchors its line

A rule that uses this requirement and checks the column at which a brace or an element of an initializer starts MUST check a token in front of which at most one comment starts on the token's own line as follows, with every comment in the token's leading trivia taken into account:

- A comment that starts on an earlier line than the token MUST start at the expected column; in front of a closing brace, one level (four columns) deeper. This holds whether the comment ends above the token's line or on it.
- A comment that starts on the token's line in front of it MUST start at the expected column.
- Every further line of a `///` comment MUST start its `///` at the same column as the comment's first line.
- When a comment ends on the token's line, the token itself MUST NOT be checked: it may follow the comment anywhere on that line. Otherwise the token MUST start at the expected column.

Why: The formatter indents the first thing on a line, and a comment that starts the line is that first thing; the code after it on the same line keeps its distance to the comment. A comment that starts above a closing brace belongs to the last element of the block, so it is indented like the elements, also when it ends on the brace's line.

Example — RH5301, whose braces belong at the column of `new` and whose members one level deeper:

```csharp
class C
{
    int P { get; set; }
    int Q { get; set; }

    void M()
    {
        var c = new C
                /* open */ {
                    // first
                    P = 1,
                    /* second */ Q = 2
                    // end
                };
        var d = new C
                {
                    P = 1
                    /* last
                       one */ };
    }
}
```

is not reported, while a comment that starts its line in the wrong column is reported on the member behind it, and a comment that starts above a closing brace in the column of the brace instead of one level deeper is reported on the object creation:

```csharp
class C
{
    int P { get; set; }

    void M()
    {
        var c = new C
                {
            /* shifted */ {|RH5301:P = 1|}
                };
        var d = {|RH5301:new C
                {
                    P = 1
                /* last
                   one */ }|};
    }
}
```

### SHARED-INIT-002 — A wrapped complex element is laid out as a block

A rule that uses this requirement MUST accept a complex element initializer (`{ 1, 2 }` inside a collection initializer) that is on one line. A complex element initializer that spans several lines MUST be treated as misaligned when:

- its closing brace does not start at the column of its opening brace;
- one of its expressions starts on the line of the opening brace, on the line of the closing brace, or on a line on which an earlier expression starts; or
- one of its expressions does not start one level (four columns) deeper than the opening brace.

The columns follow SHARED-INIT-001.

Why: A complex element is a small initializer of its own. On one line it reads as one value; once it is wrapped, its braces and its expressions follow the same block layout as the initializer around it, so the nesting stays visible.

Example — RH5303, which reports a misaligned complex element on its first misplaced part:

```csharp
using System.Collections.Generic;

class C
{
    void M()
    {
        var d = new Dictionary<int, int>
                {
                    { 1, 2 },
                    {
                        3,
                        4
                    },
                    {
                        5,
                      {|RH5303:6|}
                    },
                    {
                        7, {|RH5303:8|}
                    },
                    {|RH5303:{
                        9,
                        10
                  }|}
                };
    }
}
```

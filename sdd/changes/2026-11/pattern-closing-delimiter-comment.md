# A comment in front of a pattern's closing bracket or parenthesis is indented into its content

| | |
|---|---|
| Status | Draft |
| Date | 2026-11 |
| Affected requirements | FMT-INDENT-006 |

## What

A comment on its own line in front of the closing `]` of a list pattern is indented one level deeper than the bracket, into the column of the pattern's elements, as one in front of the `]` of a collection expression already is. A comment in front of the closing `)` of a parenthesized pattern is indented into the column of the pattern inside the parentheses, four columns right of the opening parenthesis (FMT-INDENT-026). The positional pattern gets the same rule in [positional-pattern-layout.md](positional-pattern-layout.md).

This change covers thoenissen/Reihitsu#1647, and the parenthesized pattern of the same class.

## Why

FMT-INDENT-006 indents a comment in front of a closing brace, and in front of the closing bracket of a collection expression, into the content it closes: at the column of the delimiter it reads as describing the delimiter. It names only the collection expression among the bracketed constructs, so the same comment in front of a list pattern's `]`, which FMT-INDENT-025 lays out like a collection expression otherwise, stays in the `]` column. The closing `)` of a parenthesized pattern closes content that FMT-INDENT-026 indents four columns right of the opening parenthesis, the same shape for the same reason, and its comment stays in the `)` column on `main` too. The requirement now states the rule by its cause — the delimiter closes content indented one level deeper than it — instead of naming one construct.

A comment in front of a closing delimiter whose content is not indented deeper than it, such as the closing parenthesis of an argument list, keeps its column; this change does not reach it.

## Visible output changes

Whole-document formatting; "Before" is the output on `main`, observed with `scripts/format.sh`, and stable on a second pass.

Input:

```csharp
class C
{
    bool M(int[] a, object o)
    {
        var b = a is [
                          1,

                      // last
                      ];
        var c = o is (1 or
            2

            // last
            );

        return b && c;
    }
}
```

Before:

```csharp
class C
{
    bool M(int[] a, object o)
    {
        var b = a is [
                         1,

                     // last
                     ];
        var c = o is
                  (
                      1
                      or 2

                  // last
                  );

        return b && c;
    }
}
```

After:

```csharp
class C
{
    bool M(int[] a, object o)
    {
        var b = a is [
                         1,

                         // last
                     ];
        var c = o is
                  (
                      1
                      or 2

                      // last
                  );

        return b && c;
    }
}
```

### Code fixes

RH5306's fix formats the list pattern it reports, so a comment in front of the `]` of a reported list pattern moves with it. RH5306 does not report a list pattern for such a comment alone (observed with `scripts/apply-fix.sh` on `a is [1,⏎2,⏎// last⏎]`: no diagnostic), so the fix moves the comment only where it lays out a list pattern for another reason.

No rule page changes.

## Decisions

### The list pattern gets the collection expression's rule

Pending the maintainer's answer. The recommendation indents the comment into the elements' column, as the issue's reading A does.

### The parenthesized pattern is decided together

Pending the maintainer's answer. The issue names only the list pattern; the parenthesized pattern has the same shape and the same cause, so the requirement states the cause and the question shows both.

## Alternatives considered

- **Keep the comment in the delimiter's column** (today's output) — not recommended: a collection expression and a list pattern, laid out alike everywhere else, would differ here.
- **Change only the list pattern** — not recommended: the parenthesized pattern would keep a rule stated per construct, and the next construct of the class would need its own change.

# RH5107's fix and the formatter place a comment behind a leading comma alike

| | |
|---|---|
| Status | Draft |
| Date | 2026-11 |
| Affected requirements | Option A: RH5107-003 (changed). Option B: FMT-BREAK-011 (changed), FMT-BREAK-012 (changed). Decided by the maintainer's answer below. |

## What

A comma that starts a line in a parameter list, with a comment behind it on its line, is moved behind the parameter in front of it both by RH5107's code fix and by the formatter, but the two put the comment in different places:

- the formatter moves the comment with the comma and keeps it behind the comma (FMT-BREAK-011, and FMT-BREAK-012, which states the placement for argument and attribute argument lists "as in a parameter list");
- RH5107's fix leaves the comment where it was and only replaces the indentation, the comma, and the whitespace behind it with spaces up to the first parameter's column (RH5107-003).

Each result is stable under the other component: formatting the fix's result changes nothing, so SHARED-FIX-007 holds for the fix today. The change makes the fix and the formatter produce the same result from the same input, by changing one of them.

## Why

A user who applies RH5107's fix gets a different file than a user who runs the formatter on the same code, and neither result is ever converted into the other, so a code base formatted both ways keeps both placements. A comment behind a comma is placed behind the comma in every other list the formatter lays out — argument lists, attribute argument lists, and constraint clauses (FMT-BREAK-012, FMT-BREAK-037).

## Visible output changes

Input (each case is one parameter list; the fix is offered on every one of them today, because no comment stands between the parameter in front of the comma and the comma):

```csharp
class C
{
    void P(int a
           , /* c */ int b)
    {
    }

    void Q(int a
           , // c
           int b)
    {
    }

    void R(int a
           , /* c */
           int b)
    {
    }
}
```

Before — formatter (`main`):

```csharp
class C
{
    void P(int a, /* c */
           int b)
    {
    }

    void Q(int a, // c
           int b)
    {
    }

    void R(int a, /* c */
           int b)
    {
    }
}
```

Before — RH5107's fix (`main`):

```csharp
class C
{
    void P(int a,
           /* c */ int b)
    {
    }

    void Q(int a,
           // c
           int b)
    {
    }

    void R(int a,
           /* c */
           int b)
    {
    }
}
```

After, option A — the fix changes to the formatter's output above; the formatter is unchanged.

After, option B — the formatter changes to the fix's output above, for parameter lists; RH5107's fix is unchanged. Argument and attribute argument lists keep FMT-BREAK-012's placement unless the answer extends B to them.

## Decisions

### Which component changes

Pending: the maintainer's answer to the output question of this run. Recommended: A.

## Alternatives considered

- **Withhold RH5107's fix where a comment stands behind the comma** — rejected: the formatter already moves such a comment safely, the shape is reachable through the fix today, and a withheld fix would still leave the formatter's and a hand edit's placements to differ; it is no answer to which placement is the rule's.
- **Leave both as they are** — rejected: two stable results for one input is the disagreement the change removes.

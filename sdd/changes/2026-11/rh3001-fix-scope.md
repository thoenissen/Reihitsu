# RH3001 fix in whole-argument positions, no fix for constant patterns, parentheses only where binding needs them

| | |
|---|---|
| Status | Accepted |
| Date | 2026-11 |
| Affected requirements | RH3001-004, RH3001-008 |

## What

- RH3001-004 no longer limits the fix to a negation that covers less text than the syntax node around it. A negation that is a whole argument of any argument list (such as that of an invocation, an object creation with or without its type, an element access or a conditional element access, an index in an object initializer, a constructor initializer, a primary constructor's base type, or a tuple), a whole attribute argument, or a whole collection-expression element gets the fix like any other.
- RH3001-004 states that the fix puts the comparison in parentheses only where it binds differently from the negation, and in no other position.
- RH3001-008 is added: a negation that is the whole expression of a constant pattern is reported without a fix.

## Why

The limit in RH3001-004 described an accident of how the fix finds its node, not a decision: a negation that fills its argument was not found, while `M(x: !y)` was fixed. Withholding a fix there gives away inputs the rewrite handles safely (`AGENTS.md`, *Code-fix coverage*). The parentheses the fix wrote behind `>>>=` and `??=`, in named attribute arguments, behind `goto case`, and in `select`, `group`, and `orderby` group nothing. A constant pattern cannot take the comparison the fix writes without turning into a constant expression the reader has to evaluate, so it keeps its report and gets no fix; once the fix finds whole-node negations, that case has to be stated, because it is a whole-node negation too.

## Visible output changes

The fix is offered where it was not:

```csharp
class C
{
    void M(bool b)
    {
    }

    void Run(bool x, bool y)
    {
        M(!x);
        bool[] a = [!x, y];
    }
}
```

Before: reported, no code fix offered.

After:

```csharp
class C
{
    void M(bool b)
    {
    }

    void Run(bool x, bool y)
    {
        M(x == false);
        bool[] a = [x == false, y];
    }
}
```

The same for `N(!x, y)`, `(!x, y)`, `new C(!y)`, `new(!y)`, `this[!y]`, `o?[!y]`, `new E { [!x] = true }`, `class D(bool x) : Base(!x)`, and `[A(!true)]`: each becomes `… x == false …` (or `y == false`, `true == false`) in place of the negation, without parentheses.

The fix writes no parentheses where none are needed:

```csharp
class C
{
    bool? _b;

    void Run(bool x)
    {
        _b ??= !x;
    }
}
```

Before:

```csharp
class C
{
    bool? _b;

    void Run(bool x)
    {
        _b ??= (x == false);
    }
}
```

After:

```csharp
class C
{
    bool? _b;

    void Run(bool x)
    {
        _b ??= x == false;
    }
}
```

The same behind `>>>=`, in a named attribute argument (`[A(P = !true)]` becomes `[A(P = true == false)]`, also in the `:` form), behind `goto case` (`goto case true == false;`), and in `select`, `group`, and `orderby` (`orderby w == false descending`).

The fix is offered for a negation that is the whole key of an `orderby` clause without a direction:

```csharp
using System.Linq;

class C
{
    void Run(bool[] v)
    {
        var q = from a in v
                orderby !a
                select a;
    }
}
```

Before: reported, no code fix offered. After: `orderby a == false`.

Constant patterns (`x is !true`, a switch-expression arm `!true => 1`, `case !true when x:`) stay reported without a fix: no visible change.

## Decisions

### Whole-node negations get the fix, constant patterns do not

Decided by the maintainer in question round 7a of thoenissen/Reihitsu#912 (2026-10-20), for thoenissen/Reihitsu#1245 Q2: the guard's scope is a defect, so a plain argument gets the fix as a named one does, and so does `[!x, y]`; a constant pattern (`x is !true`, `!true => 1`) is reported without a fix (P23).

### Parentheses only where binding needs them

Decided by the maintainer in question round 7b of thoenissen/Reihitsu#912 (2026-10-20), for thoenissen/Reihitsu#1245 Q3, option A.

### An `orderby` key without a direction gets the fix

Decided by the maintainer in this run (2026-11), option A: a negation that is the whole key of an `orderby` clause without a direction gets the fix, `orderby !a` becomes `orderby a == false`, as `orderby !a descending` becomes `orderby a == false descending`.

### `case !true:` keeps its fix

A `case` label of a switch statement is not a constant pattern in the syntax tree, and its negation was fixed before this change (`case true == false:`), as the maintainer was shown in thoenissen/Reihitsu#1245 Q2. RH3001-008 is keyed on the constant pattern and leaves it fixed.

## Alternatives considered

- **Keep RH3001-004's limit and document the gap** — rejected by the maintainer's answer to Q2.
- **Accept the extra parentheses and state them in RH3001-004** — rejected by the maintainer's answer to Q3.
- **Not report a negation that is a constant pattern** — rejected by the maintainer's answer to Q2 (P23: report, no fix).

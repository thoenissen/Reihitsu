# RH3001 fix in whole-argument positions, no fix for constant patterns, parentheses only where binding needs them

| | |
|---|---|
| Status | Accepted |
| Date | 2026-11 |
| Affected requirements | RH3001-004, RH3001-008, RH3001-009 |

## What

- RH3001-004 no longer limits the fix to a negation that covers less text than the syntax node around it. A negation that is a whole argument of any argument list (such as that of an invocation, an object creation with or without its type, an element access or a conditional element access, an index in an object initializer, a constructor initializer, a primary constructor's base type, or a tuple), a whole attribute argument, or a whole collection-expression element gets the fix like any other.
- RH3001-004 states that the fix puts the comparison in parentheses only where it binds differently from the negation, and in no other position.
- RH3001-008 is added: a negation that is the whole expression of a constant pattern is reported without a fix.
- RH3001-009 is added: a negation that touches a keyword or an identifier in front of `!` or behind its operand, with nothing between them (`return!a`, `where !(w)select w`), is reported without a fix. In front of `!`, this holds whatever the operand starts with, a parenthesis or a kept comment included (`return!(a)`, `return!/* c */a`).

## Why

The limit in RH3001-004 described an accident of how the fix finds its node, not a decision: a negation that fills its argument was not found, while `M(x: !y)` was fixed. Withholding a fix there gives away inputs the rewrite handles safely (`AGENTS.md`, *Code-fix coverage*). The parentheses the fix wrote behind `>>>=` and `??=`, in named attribute arguments, behind `goto case`, and in `select`, `group`, and `orderby` group nothing. Once parentheses are written only where binding needs them, a negation written against a word in front of or behind it cannot take the comparison without parentheses, which group nothing there, or a space the fix would insert, wherever the operand starts with a word or `false` meets the word behind it. Where the operand starts with `(` or a kept comment, nothing would merge, but telling those operands apart is a second check for the same rare shape, so the front side is decided on the gap in front of `!` alone; that rare class keeps its report and gets no fix. A constant pattern cannot take the comparison the fix writes without turning into a constant expression the reader has to evaluate, so it keeps its report and gets no fix; once the fix finds whole-node negations, that case has to be stated, because it is a whole-node negation too.

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

The fix is no longer offered for a negation written against a word in front of or behind it:

```csharp
class C
{
    bool M(bool a, bool b)
    {
        if (b) return!a;
        return a;
    }
}
```

Before:

```csharp
class C
{
    bool M(bool a, bool b)
    {
        if (b) return a == false;
        return a;
    }
}
```

After: reported, no code fix offered. The same for `yield return!a` (before: `yield return a == false`), `case!true:` (before: `case true == false:`), `_ when!y =>` (before: `_ when y == false =>`), `select!w` (before: `select (w == false)`), and `where !(w)select w` (before: `where (w) == false` with `select w` moved to the next line). `orderby!w` stays without a fix, as before.

The same holds where the operand starts with a parenthesis or a comment stays between `!` and the operand, although nothing would merge there:

```csharp
class C
{
    bool M(bool a, bool b)
    {
        if (b) return!(a);
        return a;
    }
}
```

Before:

```csharp
class C
{
    bool M(bool a, bool b)
    {
        if (b) return(a) == false;
        return a;
    }
}
```

After: reported, no code fix offered. The same for `yield return!(a)` (before: `yield return(a) == false`), `case!(true):` (before: `case(true) == false:`), `_ when!(y) =>` (before: `_ when(y) == false =>`), `where!(w) select w` (before: `where(w) == false select w`), and `return!/* c */a` (before: `return/* c */a == false`).

With a space or a comment between the negation and the word, the fix is offered as RH3001-004 states: `return !a` becomes `return a == false`.

## Decisions

### Whole-node negations get the fix, constant patterns do not

Decided by the maintainer in question round 7a of thoenissen/Reihitsu#912 (2026-10-20), for thoenissen/Reihitsu#1245 Q2: the guard's scope is a defect, so a plain argument gets the fix as a named one does, and so does `[!x, y]`; a constant pattern (`x is !true`, `!true => 1`) is reported without a fix (P23).

### Parentheses only where binding needs them

Decided by the maintainer in question round 7b of thoenissen/Reihitsu#912 (2026-10-20), for thoenissen/Reihitsu#1245 Q3, option A.

### An `orderby` key without a direction gets the fix

Decided by the maintainer in this run (2026-11), option A: a negation that is the whole key of an `orderby` clause without a direction gets the fix, `orderby !a` becomes `orderby a == false`, as `orderby !a descending` becomes `orderby a == false descending`.

### `case !true:` keeps its fix

A `case` label of a switch statement is not a constant pattern in the syntax tree, and its negation was fixed before this change (`case true == false:`), as the maintainer was shown in thoenissen/Reihitsu#1245 Q2. RH3001-008 is keyed on the constant pattern and leaves it fixed.

### A negation written against a word gets no fix

Decided by the lead in this run (2026-11) under `AGENTS.md`, *Code-fix coverage*: a word written against the negation is a rare input class, and where the operand starts with a word, or `false` meets the word behind the operand, the comparison fits there only in parentheses that group nothing, which RH3001-004 forbids, or with a space the fix would insert. The lead also accepted that the front side is decided on the gap in front of `!` alone, so the fix is withheld where the operand starts with `(` or a kept comment and nothing would merge (`return!(a)`, `return!/* c */a`): the class is refused as a whole rather than split by the operand's first token. Withholding a fix for a rare class is the rule's own decision and puts no output question to the maintainer.

## Alternatives considered

- **Keep RH3001-004's limit and document the gap** — rejected by the maintainer's answer to Q2.
- **Accept the extra parentheses and state them in RH3001-004** — rejected by the maintainer's answer to Q3.
- **Not report a negation that is a constant pattern** — rejected by the maintainer's answer to Q2 (P23: report, no fix).
- **Write the comparison in parentheses where the negation touches a word** (`return(a == false)`) — rejected: the parentheses group nothing, which RH3001-004 forbids.
- **Insert a space between the word and the comparison** (`return a == false`, as `main` wrote it) — rejected, although RH3002's fix and the formatter answer a merge of two tokens in the same way, with one space: this fix would need its own second rule for which gaps get the space, for a rare class, where withholding the fix needs none. The repository answers the merge question two ways here, by a space in RH3002 and the formatter and by a refusal in RH3001, and that difference is a decision, not an oversight.
- **Withhold only where a token would merge, and fix `return!(a)` as `return(a) == false`** — rejected: it adds a check of the operand's first token and of the comments kept in front of it for the same rare shape, and `return(a)` is what RH6001 reports.

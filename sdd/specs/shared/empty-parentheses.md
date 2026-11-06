# Shared — Empty parentheses

| | |
|---|---|
| ID prefix | SHARED-PARENS |
| Status | Active |
| Code owners | `EmptyParenthesesAnalyzerBase<TNode>` |
| Uses | — |

This spec covers what the rules share that report an empty pair of parentheses the language lets the author leave out — RH3106 (the parameter list of an anonymous method) and RH3107 (the argument list of an attribute): where the diagnostic is and what the code fix produces. Each rule's spec states which list it reports.

## Non-goals

- Which list a rule reports — the rule's own spec.
- Empty parentheses the language requires, such as the argument list of an invocation or an object creation.

## Requirements

### SHARED-PARENS-001 — The diagnostic covers the empty list

The rule MUST report one diagnostic per empty list, from its `(` to its `)`.

Why: The parentheses are what the fix removes; the construct they belong to stays.

Example — RH3107:

```csharp
using System;

[Obsolete{|RH3107:()|}]
class C
{
}
```

### SHARED-PARENS-002 — The fix removes the empty list

For a reported list that holds nothing but whitespace between `(` and `)`, the code fix MUST remove the list from `(` to `)`. A comment in front of `(` MUST stay. The whitespace that the removal leaves next to each other is laid out as the formatter lays it out (SHARED-FIX-007): one space between `delegate` and the body of an anonymous method, and no space in front of the `]` or `,` behind an attribute.

Why: Without the parentheses the construct means the same; a comment in front of the list belongs to the code before it.

Example — RH3106:

```csharp
using System;

class C
{
    void M()
    {
        Action a = delegate{|RH3106:()|} { };
    }
}
```

```csharp
using System;

class C
{
    void M()
    {
        Action a = delegate { };
    }
}
```

Example — RH3107:

```csharp
using System;

[Obsolete{|RH3107:()|}, Serializable /* a */{|RH3107:()|}]
class C
{
}
```

```csharp
using System;

[Obsolete, Serializable /* a */]
class C
{
}
```

Example — the whitespace around the removed list:

```csharp
using System;

class C
{
    [Obsolete /* a */ {|RH3107:()|}]
    void M()
    {
        Action a = delegate {|RH3106:()|} { };
    }
}
```

```csharp
using System;

class C
{
    [Obsolete /* a */]
    void M()
    {
        Action a = delegate { };
    }
}
```

### SHARED-PARENS-003 — A comment inside the list withholds the fix

The code fix MUST NOT be offered when a comment or a preprocessor directive stands between `(` and `)` of the reported list. The diagnostic is still reported. A comment in front of `(` MUST NOT withhold the fix and stays as SHARED-PARENS-002 states.

Why: The fix removes the list from `(` to `)`; a comment inside it would be deleted with the parentheses (SHARED-FIX-003).

Example — RH3106:

```csharp
using System;

class C
{
    void M()
    {
        Action a = delegate {|RH3106:( /* none */ )|} { };
    }
}
```

is reported, and no code fix is offered.

Example — RH3107:

```csharp
using System;

[Obsolete{|RH3107:(/* none */)|}]
class C
{
}
```

is reported, and no code fix is offered.

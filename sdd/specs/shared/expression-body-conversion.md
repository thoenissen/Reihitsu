# Shared — Expression bodies reported for a block body

| | |
|---|---|
| ID prefix | SHARED-BODY |
| Status | Active |
| Code owners | `ExpressionBodyAnalyzerBase<TNode>`, `ExpressionBodyToBlockCodeFixProviderBase<TNode>`, `ExpressionBodyRewriteUtilities` |
| Uses | FMT-STRUCT-004, FMT-STRUCT-005, FMT-STRUCT-006, FMT-STRUCT-007, FMT-PIPE-010 |

This spec covers what the rules share that report an expression-bodied member so that it gets a block body — RH3203 (constructors), RH3205 (operators), RH3206 (conversion operators), RH3207 (finalizers), and RH3208 (local functions): which members are left alone because the formatter cannot convert them, where the diagnostic is, and what the code fix produces. Each rule's spec states which kind of member it reports. "The member" below is the declaration the rule reports, a local function included.

## Non-goals

- Which kind of member a rule reports — the rule's own spec.
- Expression-bodied methods, which RH3202 specifies in its own requirements.
- Expression-bodied properties, indexers, accessors, and lambdas, which stay expression-bodied (FMT-STRUCT-008).
- The statement the block body contains and how the trivia of `=>` and `;` is carried over — the formatter's FMT-STRUCT requirements own that; this spec only references them.

## Requirements

### SHARED-BODY-001 — A member the formatter cannot convert is not reported

The rule MUST NOT report a member whose expression body the formatter keeps under FMT-STRUCT-007: a directive between `=>` and the start of the expression, a conditional directive between `=>` and the semicolon whose partner lies outside that span, or a `#region` or `#endregion` in that span or in the leading trivia of `=>`. A balanced conditional group, or another directive, between the expression and the semicolon MUST NOT keep the member from being reported.

Why: A diagnostic whose fix the formatter then refuses would never converge: the fix would run and leave the code as it was. The analyzer, the code fix, and the formatter take the decision from one predicate, so they stay in step.

Example — RH3203, a directive between `=>` and the expression:

```csharp
class C
{
    private int _value;

    public C(int value) =>
#if DEBUG
        _value = value;
#else
        _value = 0;
#endif
}
```

is not reported. Neither is a region that the semicolon splits:

```csharp
class C
{
    private int _value;

    public C(int value) => _value = value
    #region R
        ;
    #endregion
}
```

A balanced group between the expression and the semicolon does not block the conversion:

```csharp
class C
{
    private int _value;

    public C(int value) {|RH3203:=> _value = value
#if DEBUG
        + 1
#endif|}
        ;
}
```

```csharp
class C
{
    private int _value;

    public C(int value)
    {
        _value = value
#if DEBUG
        + 1
#endif
        ;
    }
}
```

### SHARED-BODY-002 — The diagnostic covers the expression body

The rule MUST report one diagnostic per member, on its expression body: from the `=>` token to the end of the expression, excluding the semicolon.

Why: The arrow and the expression are what the fix replaces; the signature of the member stays.

Example — RH3205:

```csharp
class C
{
    public static C operator +(C a, C b) {|RH3205:=>
        a|};
}
```

### SHARED-BODY-003 — The fix converts the member as the formatter does

The code fix MUST replace the member with the result of formatting it through the formatter's node-level entry point (FMT-PIPE-010), so that the block body is exactly the one FMT-STRUCT-004 or FMT-STRUCT-005 produces, with the trivia FMT-STRUCT-006 carries over. The rest of the member is laid out by the formatter in the same step.

Why: One owner for the conversion keeps the fixes and the formatter from drifting apart. A fix that built the block itself would have to repeat the statement choice for `void`, `async`, conversion, and throw expressions and the transfer of the trivia of `=>` and `;`.

Example — RH3203:

```csharp
using System;

class B
{
    public B(int v)
    {
    }
}

class C : B
{
    private int _value;
    private static int s_count;

    static C() {|RH3203:=> s_count = 1|};

    public C(int value) : base(value) {|RH3203:=> _value = value|};

    public C(string text) {|RH3203:=> throw new ArgumentException(text)|};
}
```

```csharp
using System;

class B
{
    public B(int v)
    {
    }
}

class C : B
{
    private int _value;
    private static int s_count;

    static C()
    {
        s_count = 1;
    }

    public C(int value)
        : base(value)
    {
        _value = value;
    }

    public C(string text)
    {
        throw new ArgumentException(text);
    }
}
```

Example — RH3207, an expression on the line after `=>`, and a comment between the expression and the semicolon:

```csharp
class C
{
    ~C() {|RH3207:=>
        Release()|} /* why */;

    private void Release()
    {
    }
}
```

```csharp
class C
{
    ~C()
    {
        Release() /* why */;
    }

    private void Release()
    {
    }
}
```

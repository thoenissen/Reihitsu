# Shared — Operator spacing

| | |
|---|---|
| ID prefix | SHARED-OPSPACE |
| Status | Active |
| Code owners | `OperatorSpacingCodeFixProviderBase` |
| Uses | FMT-SPACE-010 |

This spec covers the rules that require exactly one space on both sides of an operator token: which gap next to the rule's operator is reported, where the diagnostic is, and what the fix produces. Each rule that uses this spec states which operators it checks; it names the requirements of this spec it follows.

In the examples, `→` marks a tab.

## Non-goals

- Which operators a rule checks — each rule's spec, such as RH6023 and RH6024.
- Operators that require no space, such as unary operators and member-access symbols — RH6016 to RH6020.
- Whether an operator that wraps belongs at the end of the line or at the start of the next one — the line-break rules and the formatter (FMT-BREAK).

## Requirements

### SHARED-OPSPACE-001 — A gap that is not exactly one space is reported

An analyzer that uses this requirement MUST report the rule's operator when the gap between the operator and the token before it, or between the operator and the token after it, holds anything other than exactly one space: no whitespace at all, more than one space, or a tab. A gap that holds a line break MUST NOT be reported, and neither MUST a gap that holds a comment; each side is decided on its own, so a reported side is reported also when the other side holds a comment.

Why: Operands run together with their operator (`a+b`) are hard to read, and padding used to line operators up breaks on the next edit. A side that wraps to the next line is laid out by the indentation, not by a gap width. A comment in the gap is the author's placement, and the rule does not decide where it goes.

Example — RH6024:

```csharp
class C
{
    int M(int a, int b)
    {
        var x = a{|RH6024:+|}b;
        var y = a {|RH6024:+|}b;
        var z = a  {|RH6024:+|}  b;
        var w = a→{|RH6024:*|} b;
        var v = a /* note */{|RH6024:+|}b;

        return a +
            b;
    }
}
```

### SHARED-OPSPACE-002 — Gaps with a line break or a comment are not reported

An analyzer that uses this requirement MUST NOT report the rule's operator when each gap next to it is exactly one space, holds a line break, or holds a comment.

Why: These are the gaps SHARED-OPSPACE-001 leaves alone; an operator whose gaps are all of that kind has nothing the rule asks to change.

Example — RH6024:

```csharp
class C
{
    int M(int a, int b)
    {
        var x = a + b;
        var y = a /* note */+ b;
        var z = a + /* note */b;
        var w = a
            + b;

        return a +
            b;
    }
}
```

is not reported.

### SHARED-OPSPACE-003 — One diagnostic per operator, on the operator

An analyzer that uses this requirement MUST report one diagnostic per operator, located on the operator token, whether one side or both sides are reported.

Why: The operator is the token whose surroundings are wrong, and one diagnostic per operator keeps one fix per operator.

Example: see SHARED-OPSPACE-001, where both sides of the operator in `a  +  b` are wrong and it is reported once.

### SHARED-OPSPACE-004 — The fix puts one space on both sides

A code fix that uses this requirement MUST leave exactly one space in each gap next to the reported operator that holds no line break and no comment, as FMT-SPACE-010 specifies for the formatter.

Why: The fix answers exactly the question the diagnostic asks. Using the formatter's operator-spacing decision keeps the fixed code identical to formatted code, so neither raises the diagnostic again.

Example — RH6024:

```csharp
class C
{
    int M(int a, int b)
    {
        var x = a{|RH6024:+|}b;
        var z = a  {|RH6024:+|}  b;
        var w = a→{|RH6024:*|} b;

        return a{|RH6024:-|} -b;
    }
}
```

```csharp
class C
{
    int M(int a, int b)
    {
        var x = a + b;
        var z = a + b;
        var w = a * b;

        return a - -b;
    }
}
```

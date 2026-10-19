# Shared — Debug message text

| | |
|---|---|
| ID prefix | SHARED-DEBUG |
| Status | Active |
| Code owners | `DebugMessageTextAnalysisUtilities` |
| Uses | — |

This spec covers the rules that require a call of a `System.Diagnostics.Debug` method to pass message text. It defines when the message argument of such a call has no text. Each rule that uses this spec states which method it checks and when a call without a message argument is reported.

## Non-goals

- Which method a rule checks, and how it finds the message argument of a call — each rule's spec.
- Whether the text of a message is helpful. Any text counts.
- A code fix: no rule of this kind offers one, because only the author knows what the message has to say.

## Requirements

### SHARED-DEBUG-001 — A constant without text is no message

A rule MUST treat a message argument as having no text exactly when it is a compile-time constant whose value is `null`, the empty string, or a string of white-space characters only, as `string.IsNullOrWhiteSpace` decides. The constant MAY be written in any form the compiler folds: a literal, `default`, a cast of `null`, a `const` field or local, a concatenation of constants, or an interpolated string without holes or with constant holes only. A message whose value is not known at compile time MUST be treated as having text.

This requirement covers a message that is not a compile-time constant only when it is a parameter, a local variable, a method call, or an interpolated string with a hole whose value is not constant.

Why: A message without text gives whoever hits the failure nothing more than a stack trace. The analyzer can judge only a value the compiler knows; a message computed at run time may well have text, and reporting it would be a guess.

Example — RH2006; the no-break space ` ` is white space for `string.IsNullOrWhiteSpace`:

```csharp
using System.Diagnostics;

class C
{
    const string Empty = "";

    string Describe() => "described";

    void M(bool condition, string message, int value)
    {
        {|RH2006:Debug.Assert(condition, null)|};
        {|RH2006:Debug.Assert(condition, default)|};
        {|RH2006:Debug.Assert(condition, (string)null)|};
        {|RH2006:Debug.Assert(condition, "")|};
        {|RH2006:Debug.Assert(condition, " \t\n")|};
        {|RH2006:Debug.Assert(condition, " ")|};
        {|RH2006:Debug.Assert(condition, Empty)|};
        {|RH2006:Debug.Assert(condition, "" + " ")|};
        {|RH2006:Debug.Assert(condition, $"")|};
        {|RH2006:Debug.Assert(condition, $"{Empty}")|};
        Debug.Assert(condition, "Condition must hold.");
        Debug.Assert(condition, message);
        Debug.Assert(condition, Describe());
        Debug.Assert(condition, $"Value {value}");
    }
}
```

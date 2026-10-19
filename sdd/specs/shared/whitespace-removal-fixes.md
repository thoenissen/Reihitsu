# Shared — Whitespace removal fixes

| | |
|---|---|
| ID prefix | SHARED-WS |
| Status | Active |
| Code owners | `RemoveWhitespaceRunCodeFixProviderBase`, `WhitespaceSpanRemovalCodeFixProviderBase`, `BlankLineSpanRemovalCodeFixProviderBase`, `CollapseTokenGapCodeFixProviderBase` |
| Uses | SHARED-JOIN-001 |

This spec covers the code fixes of rules that report whitespace that must not be there — a run of spaces and tabs, a span of blank lines, or the gap in front of a token that belongs on the line of the token before it — and correct it by deleting that whitespace: what the deletion changes, and when a comment or a directive near a run withholds it. Each rule that uses this spec states which whitespace it reports and where its diagnostic is; it names the requirements of this spec its fix follows.

## Non-goals

- Which whitespace a rule reports and where its diagnostic is — each rule's spec.
- Fixes that insert whitespace or set a gap to a fixed width, such as RH6002 and RH6009.
- Code with syntax errors, such as a reported run at the end of a truncated file.

## Requirements

### SHARED-WS-001 — The fix deletes exactly the reported whitespace

A code fix that uses this requirement MUST delete the whitespace of the reported span and MUST NOT change any other text: no comment, directive, or token is added, moved, or removed, and no whitespace outside the span is changed.

Why: The diagnostic names one span of whitespace as wrong, and deleting that span is the whole correction. Whether the span holds a line break is decided by the rule that reports it: a rule that reports a run within one line keeps the line structure of the file as it was.

Example — RH6003, which reports the whitespace before a semicolon:

```csharp
class C
{
    void M(int a)
    {
        M(a){|RH6003: |};
        M(a){|RH6003:  |}; // note
    }
}
```

```csharp
class C
{
    void M(int a)
    {
        M(a);
        M(a); // note
    }
}
```

### SHARED-WS-002 — A comment or directive in the token gap withholds the fix

A code fix that uses this requirement MUST NOT be offered when the gap between the two tokens around the reported whitespace contains a comment or a preprocessor directive, wherever in that gap it is — next to the reported run or on another line of the gap. A comment or directive outside that gap, such as a trailing comment after the second token, MUST NOT withhold the fix. The diagnostic is still reported.

Why: Deleting whitespace next to a comment glues the comment to a token or changes what it appears to annotate, and the author decides where a comment goes (`documentation/rules/general-notes.md`). The fix checks the whole gap rather than only the run's neighborhood: it may refuse a run it could delete safely, but it never has to decide whether a comment on another line is affected.

Example — RH6003 and RH6006, which reports the whitespace after an opening parenthesis:

```csharp
class C
{
    void M(int a)
    {
        M(a) /* note */{|RH6003: |};
        M({|RH6006:  |}
            // first argument
            a);
        M({|RH6006:  |}
#if FEATURE
#endif
            a);
    }
}
```

is reported, and no code fix is offered. In

```csharp
class C
{
    void M(int a)
    {
        M(a){|RH6003: |}; // note
    }
}
```

the comment follows the semicolon, outside the gap, and the fix is offered.

### SHARED-WS-003 — The fix joins a reported token to the token before it

A code fix that uses this requirement MUST delete the whole gap between the reported token and the token before it, line breaks and indentation included, so that the reported token directly follows the token before it on that token's line, and MUST NOT change any other text.

Why: The rules that use this requirement report a token that belongs directly behind the token before it, such as the opening parenthesis of a parameter list behind the declaration's name; deleting the gap is the whole correction. These rules do not report a gap that holds a comment, a directive, or disabled text (SHARED-JOIN-001), so the deletion only ever removes whitespace.

Example — RH5105, which reports an opening parenthesis of a parameter list that starts a line:

```csharp
class C
{
    void M
        {|RH5105:(|}int a)
    {
    }
}
```

```csharp
class C
{
    void M(int a)
    {
    }
}
```

# Shared — Whitespace removal fixes

| | |
|---|---|
| ID prefix | SHARED-WS |
| Status | Active |
| Code owners | `RemoveWhitespaceRunCodeFixProviderBase`, `WhitespaceSpanRemovalCodeFixProviderBase` |
| Uses | — |

This spec covers the code fixes of rules that report a run of whitespace that must not be there and correct it by deleting that run: what the deletion changes, and when a comment or a directive near the run withholds it. Each rule that uses this spec states which whitespace it reports and where its diagnostic is; it names the requirements of this spec its fix follows.

## Non-goals

- Which whitespace a rule reports and where its diagnostic is — each rule's spec.
- Fixes that insert whitespace or set a gap to a fixed width, such as RH6002 and RH6009.
- Code with syntax errors, such as a reported run at the end of a truncated file.

## Requirements

### SHARED-WS-001 — The fix deletes exactly the reported whitespace

A code fix that uses this spec MUST delete the whitespace of the reported span and MUST NOT change any other text: no line break, comment, directive, or token is added, moved, or removed.

Why: The diagnostic names one run of whitespace as wrong, and deleting that run is the whole correction. A reported run never contains a line break, so the line structure of the file stays as it was.

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

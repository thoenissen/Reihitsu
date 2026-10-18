# Shared — Token-gap fixes

| | |
|---|---|
| ID prefix | SHARED-GAP |
| Status | Active |
| Code owners | `CommentSafeSpanReplacementCodeFixProviderBase` |
| Uses | — |

This spec covers the code fixes of rules that check the whitespace on both sides of one token and correct it by rewriting the gap before the token, the gap after it, or both: when a comment or a directive in such a gap withholds the fix. Each rule that uses this spec states which sides it reports and what it writes into a rewritten gap.

## Non-goals

- Which sides a rule reports and what the fix writes — each rule's spec, such as RH6016 and RH6021.
- Fixes that delete one reported whitespace run — SHARED-WS-002 decides when a comment or directive withholds them.
- Code with syntax errors.

## Requirements

### SHARED-GAP-001 — A comment in a rewritten gap withholds the fix

A code fix that uses this requirement MUST NOT be offered when a gap it rewrites — the whole gap between the reported token and the token next to it on that side — contains a comment or a preprocessor directive. A comment or directive in a gap the fix leaves unchanged, because the rule did not report that side, MUST NOT withhold the fix. The diagnostic is still reported.

Why: Rewriting the whitespace next to a comment glues the comment to a token or changes what it appears to annotate, and the author decides where a comment goes (`documentation/rules/general-notes.md`). Each side is decided on its own, so a comment on a side the fix does not touch is no reason to refuse the side it does.

Example — RH6016, which reports whitespace next to a member-access symbol, and RH6021, which reports a base-list colon without a space on each side:

```csharp
class B
{
}

class C /* note */{|RH6021::|} B
{
    int M(string s)
    {
        return s /* note */ {|RH6016:.|}Length;
    }
}
```

is reported, and no code fix is offered. In

```csharp
class B
{
}

class C // note
    {|RH6021::|}B
{
    int M(string s)
    {
        return s /* note */{|RH6016:.|} Length;
    }
}
```

the comments are in the gaps before the colon and before the dot, which the fixes do not rewrite, and the fixes are offered:

```csharp
class B
{
}

class C // note
    : B
{
    int M(string s)
    {
        return s /* note */.Length;
    }
}
```

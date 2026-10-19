# Shared — Line-join rules

| | |
|---|---|
| ID prefix | SHARED-JOIN |
| Status | Active |
| Code owners | `SyntaxTreeErrorVerdictCache` |
| Uses | FMT-BREAK-001 |

This spec covers the rules that report a token or an element that belongs on the line of the token in front of it, such as an opening parenthesis below a declaration's name, a first argument below its opening parenthesis, or a semicolon below its declaration: when the gap in front of it keeps such a rule from reporting, and when a whole file is not analyzed. Each rule that uses this spec states which tokens it reports and names the requirements of this spec it follows.

## Non-goals

- Which tokens a rule reports, where its diagnostic is, and what its fix produces — each rule's spec.
- The gap in front of the first link of a member-access chain — SHARED-CHAIN-004.

## Requirements

### SHARED-JOIN-001 — A gap the formatter does not join is not reported

A rule that uses this requirement MUST NOT report a token or an element when a comment, a preprocessor directive, or disabled text sits in the gap between it and the token in front of it, in the trailing trivia of the token in front of it or in its own leading trivia.

Why: The token cannot move onto the line of the token in front of it without moving the comment or the directive, and the formatter refuses that join as well (FMT-BREAK-001). A diagnostic that no layout can clear would stay for good; the author decides where the comment or the directive goes.

Example — RH5105 and RH5113:

```csharp
class C
{
    int _a // note
        ;

    void M /* why */
        (int a)
    {
    }

    void N
#if DEBUG
#endif
        (int a)
    {
    }
}
```

is not reported, while the same gaps holding only whitespace are reported:

```csharp
class C
{
    int _a
        {|RH5113:;|}

    void M
        {|RH5105:(|}int a)
    {
    }
}
```

### SHARED-JOIN-002 — A file with a syntax error is not analyzed

A rule that uses this requirement MUST NOT report anything in a file that contains a syntax error, wherever the error is.

Why: A syntax error can make the parser attach a token to the wrong construct or end a list at the wrong token, so a diagnostic there could point at code the author never wrote that way; the formatter leaves such a file untouched as well.

Example — RH5105:

```csharp
class C
{
    void M
        (int a)
    {
    }

    int _x = ;
}
```

is not reported.

# Shared — Region directive blank-line diagnostics and fixes

| | |
|---|---|
| ID prefix | SHARED-REGION |
| Status | Active |
| Code owners | `RegionDirectiveBlankLineAnalyzerBase`, `RegionDirectiveBlankLineCodeFixProviderBase` |
| Uses | SHARED-BLANK-005, SHARED-BLANK-006, SHARED-FIX-008 |

This spec covers what the two rules that ask for a blank line next to a `#region` or `#endregion` directive share beyond the blank-line policy itself: where the diagnostic is, and what the code fix inserts. Which directive misses its blank line is SHARED-BLANK-005 (the line above, RH5031) and SHARED-BLANK-006 (the line below, RH5032); each rule that uses this spec names the side it checks.

## Non-goals

- Which directives need a blank line, and the exemptions next to braces, at the start and end of the file, and in disabled code — SHARED-BLANK-005 to SHARED-BLANK-007.
- The blank line before or after a comment next to a directive — RH5020 and the formatter.
- The name of a region and the comment behind its `#endregion` — the formatter's region formatting.

## Requirements

### SHARED-REGION-001 — The diagnostic covers the directive's text

A rule that uses this spec MUST report one diagnostic per directive that misses its blank line, located on the directive's text from the `#` to its last character that is not whitespace, so a region name or a comment behind `#endregion` is part of the span and the indentation in front of the directive is not.

Why: The directive, not the gap next to it, is what the reader has to find; a span on the missing line would be empty. Covering the whole directive text makes the diagnostic point at the region boundary the blank line belongs to.

Example — RH5031:

```csharp
class C
{
    #region Fields
    private int _x;
    {|RH5031:#endregion // Fields|}
    int _a;
}
```

and RH5032:

```csharp
class C
{
    {|RH5032:#region Fields|}
    private int _x;

    {|RH5032:#endregion // Fields|}
    int _a;
}
```

### SHARED-REGION-002 — The fix inserts one empty line on the reported side

The code fix MUST insert one empty line — a line break in the line ending SHARED-FIX-008 specifies, with no indentation — directly above the directive's line for the rule that checks the line above, or directly below it for the rule that checks the line below. It MUST NOT change anything else: a comment directly above or below the directive stays where it is, so the empty line goes between that comment and the directive. The fix is offered for every reported directive.

Why: The missing blank line is the whole correction. A comment next to the directive is its own line of content whose separation is decided by RH5020, not by the region rules; moving it would change what the comment appears to annotate.

Example — RH5031, with a comment directly above the directive:

```csharp
class C
{
    #region Fields
    private int _x;
    {|RH5031:#endregion // Fields|}
    int _a;
    // header
    {|RH5031:#region R|}

    int _b;

    #endregion // R
}
```

```csharp
class C
{
    #region Fields
    private int _x;

    #endregion // Fields
    int _a;
    // header

    #region R

    int _b;

    #endregion // R
}
```

RH5032:

```csharp
class C
{
    {|RH5032:#region Fields|}
    private int _x;

    {|RH5032:#endregion // Fields|}
    int _a;
}
```

```csharp
class C
{
    #region Fields

    private int _x;

    #endregion // Fields

    int _a;
}
```

# RH7304 fixes a region whose first line of code starts with a comment

| | |
|---|---|
| Status | Accepted |
| Date | 2026-11 |
| Affected requirements | RH7304-001 (clarified), RH7304-004 (changed) |

## What

When the first line of a region's code starts with a comment — a comment on that line in front of the first token, or a comment that starts on an earlier line and ends in front of the token — RH7304 compares the region's directives with the column where that comment starts (RH7304-001), and its code fix indents misaligned directives to that column (RH7304-004). Until now the analyzer took the line's text up to the token as the expected indentation, so it reported both directives even where they stood in the comment's column, and the fix was never offered for such a region.

## Why

thoenissen/Reihitsu#1594 found that RH7304 reports the layout the formatter writes for a region whose first line of code starts with a comment. A comment that starts a line is that line's first element (SHARED-COMMENT-011), so the directives belong in its column. Comparing with that column is a clarification of RH7304-001 under SHARED-COMMENT-011: directives in the comment's column, which is the formatter's output, are no longer reported (SHARED-DIAG-010). Offering the fix where it was withheld changes a code-fix result, which the maintainer decided.

## Visible output changes

**A comment leads the region's first line of code, on one line or across two**

```csharp
// input
class C
{
  #region Fields
    /* c */ int _a;
  #endregion // Fields

  #region Lines
    /* g
     */ int _e;
  #endregion // Lines
}
```

```csharp
// before: RH7304 reports all four directives; no fix is offered
```

```csharp
// after: RH7304 reports all four directives; the fix gives
class C
{
    #region Fields
    /* c */ int _a;
    #endregion // Fields

    #region Lines
    /* g
     */ int _e;
    #endregion // Lines
}
```

The rule page `documentation/rules/RH7304.md` does not change.

## Decisions

### The fix indents the directives to the comment's column

The maintainer chose A (2026-11-15): for a region whose first line of code starts with a comment, on one line or across several, misaligned directives are still reported, and the fix is offered and indents them to the column where the comment starts.

### A comment that spans lines counts from where it starts

A comment that starts on an earlier line and ends in front of the region's first token leads the region's code; the directives are compared with the column where it starts, as SHARED-INIT-001 takes such a comment for an initializer.

## Alternatives considered

- **Keep withholding the fix for a comment-led region line** — rejected by the maintainer: only the directives' whitespace changes, so the fix can rewrite the input safely, and withholding it would refuse safe ground.
- **Never report a region whose first token does not start its line, as thoenissen/Reihitsu#1594 proposed** — rejected: misaligned directives stay misaligned whatever stands in front of the first token, and comparing with the comment's column already leaves the formatter's output unreported.

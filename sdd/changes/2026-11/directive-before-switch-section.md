# A directive directly in front of a switch section suppresses its blank line

| | |
|---|---|
| Status | Accepted |
| Date | 2026-11 |
| Affected requirements | FMT-BLANK-004, FMT-BLANK-006, FMT-BLANK-016 |

## What

FMT-BLANK-006 now covers switch sections as well as statements: the formatter does not insert the blank line FMT-BLANK-004 asks for in front of a switch section behind a section that ends in `break` when a preprocessor directive directly precedes that section. A whole conditional group, with the disabled text between its directives, counts as directly preceding. When a comment stands between the last directive and the section, the section still gets the blank line, after the last directive and in front of the comment (FMT-BLANK-016).

## Why

Before this change, FMT-BLANK-006 named statements only, so the spec asked for a blank line in a section gap that holds a directive, placed after the directive by FMT-BLANK-016, while the same shape in a block (`break;`, a directive, a statement) gets none. The formatter's actual output, a blank line in front of the directive, matched neither. A blank line after the directive lands inside the conditional region the directive opens, and the analyzer already treats both gaps alike: RH5011 does not report a `break` whose next trivia is a directive (RH5011-002), in front of a statement or a section. One exemption for statements and sections keeps the formatter consistent with itself and with RH5011.

## Visible output changes

1 · A directive directly in front of a section behind a section that ends in `break`:

```csharp
switch (value)
{
    case 1:
        value++;
        break;
#pragma warning disable CS0168
    case 2:
        value--;
        break;
}
```

Before:

```csharp
switch (value)
{
    case 1:
        value++;
        break;

#pragma warning disable CS0168
    case 2:
        value--;
        break;
}
```

After (unchanged from the input):

```csharp
switch (value)
{
    case 1:
        value++;
        break;
#pragma warning disable CS0168
    case 2:
        value--;
        break;
}
```

2 · A conditional group whose branch the build does not compile, directly in front of a section:

```csharp
switch (value)
{
    case 1:
        value++;
        break;
#if NEVER
    case 3:
        break;
#endif
    case 2:
        value--;
        break;
}
```

Before:

```csharp
switch (value)
{
    case 1:
        value++;
        break;

#if NEVER
    case 3:
        break;
#endif
    case 2:
        value--;
        break;
}
```

After (unchanged from the input):

```csharp
switch (value)
{
    case 1:
        value++;
        break;
#if NEVER
    case 3:
        break;
#endif
    case 2:
        value--;
        break;
}
```

A section with a comment between the directive and the section is not exempt; it gets the blank line after the directive as FMT-BLANK-016 already required, and its fix is a bug fix outside this change.

## Decisions

### No blank line in front of a section that a directive directly precedes

Maintainer answer 1A in the `directive-switch-region` run (issue #1385): no blank line is inserted; the directive suppresses the section blank line as FMT-BLANK-006 does for statements. This covers the disabled conditional group directly in front of a section (input 2). With a comment between the directive and the section, the blank line goes after the directive.

## Alternatives considered

- **Blank line after the directive (`#pragma …`, blank line, `case 2:`)** — rejected by the maintainer (option B): it is the literal reading of the former FMT-BLANK-006, but it puts a blank line inside the region the directive opens, and the block form of the same gap gets none.
- **Keep the blank line in front of the directive (the former output)** — rejected: FMT-BLANK-016 forbids inserting a blank line directly in front of a directive.

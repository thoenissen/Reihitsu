# RH5107's fix is withheld for a comment behind a leading comma

| | |
|---|---|
| Status | Accepted |
| Date | 2026-11 |
| Affected requirements | RH5107-003 (changed: points to RH5107-004), RH5107-004 (added) |

## What

A comma that starts a line in a parameter list, with a comment behind it on its line, is moved behind the parameter in front of it both by RH5107's code fix and by the formatter, but the two put the comment in different places:

- the formatter moves the comment with the comma and keeps it behind the comma (FMT-BREAK-011, and FMT-BREAK-012, which states the placement for argument and attribute argument lists "as in a parameter list");
- RH5107's fix moves only the comma and leaves the comment where it was (RH5107-003).

The change withholds RH5107's fix for that input: when a comment stands behind the reported comma on the comma's line, the diagnostic is still reported and no fix is offered (RH5107-004). A comment that starts the line behind the comma's line keeps the fix, whose result the formatter already writes for the same input. The formatter does not change.

## Why

For a comment that ends the comma's line (`, // c`, `, /* c */`), the fix's result moves the comment away from the code it stands with: the comment stands behind the comma, and the fix moves the comma and leaves the comment on a line of its own. That violates SHARED-FIX-003. Formatting that result does not give the formatter's output either: the formatter inserts a blank line above the comment that is now on its own line (FMT-BLANK-011), so fix-then-format and format-only end in two different files. For a block comment between the comma and the next parameter (`, /* c */ int b`), SHARED-FIX-003 decides nothing, but the fix and the formatter still keep two placements of one comment.

The class is rare: every such input holds a comment inside a parameter list, which RH5035 reports (RH5035-001), and `AGENTS.md` (*Code-fix coverage*) treats an input that violates another Reihitsu rule as rare by definition. For a rare class, withholding the fix is preferred over writing the formatter's comment placement a second time into the fix's own text edit (*Special cases do not grow the source*). A diagnostic without a fix is a supported end state.

## Visible output changes

Input:

```csharp
class C
{
    void P(int a
           , /* c */ int b)
    {
    }

    void Q(int a
           , // c
           int b)
    {
    }

    void R(int a
           , /* c */
           int b)
    {
    }
}
```

Before — RH5107's fix (`main`):

```csharp
class C
{
    void P(int a,
           /* c */ int b)
    {
    }

    void Q(int a,
           // c
           int b)
    {
    }

    void R(int a,
           /* c */
           int b)
    {
    }
}
```

After: the three commas are still reported, and no code fix is offered.

The formatter's output does not change; on `main` and after the change it is:

```csharp
class C
{
    void P(int a, /* c */
           int b)
    {
    }

    void Q(int a, // c
           int b)
    {
    }

    void R(int a, /* c */
           int b)
    {
    }
}
```

Unchanged — a comment that starts the line behind the comma's line keeps the fix, and its result equals the formatter's output:

```csharp
class C
{
    void T(int a
           ,
           /* c */ int b)
    {
    }
}
```

```csharp
class C
{
    void T(int a,
           /* c */ int b)
    {
    }
}
```

The rule page does not change: `documentation/rules/general-notes.md` already covers a diagnostic without a code fix.

## Decisions

### Withhold the fix instead of changing either component

The fix is withheld for the class; neither the fix's placement nor the formatter's changes. Withholding a fix for a rare class is the rule's own decision and puts no output in question (`squad/README.md`, *What reaches the maintainer*), so the output question drafted for this issue — fix follows the formatter, or formatter follows the fix — was not asked.

### The class is "a comment behind the comma on its line"

The fix is withheld when any comment stands behind the reported comma on the comma's line, also a block comment followed by the next parameter, for which SHARED-FIX-003 decides nothing. One class keeps the guard to one check of the trivia behind the comma on its line, and no fix result then differs from the formatter's placement. That trivia is the comma's trailing trivia and, for a documentation comment (`/** c */`, `/// c`) in the documentation modes `Parse` and `Diagnose`, the leading trivia of the parameter behind the comma, because the parser puts no documentation comment into trailing trivia; the comment is written behind the comma either way, so both withhold the fix (added in repair round 1, after the audit found the fix applied to `int a⏎, /// c⏎int b`). A comment that starts a later line keeps the fix, because its result is the formatter's output.

### The issue's goal

thoenissen/Reihitsu#1737 asked that the fix's result and the formatter's output agree on the input. That goal is met by withholding: no fix result exists that disagrees with the formatter. The issue's "Required output" B (the formatter follows the fix) is not available, because it would make the formatter keep a placement that violates SHARED-FIX-003 when a fix writes it.

### Not affected by the declaration of FMT-UNSUP-009

The maintainer's escalation-10 answer (question 4 A, question 5 A) declares a comment, a directive, or disabled text in front of a comma on a later line unsupported, in this run ([its change doc](comma-behind-comment-unsupported.md)). That shape is the gap in front of the comma; this change concerns a comment behind it, which stays supported, so RH5107-004 and the formatter's placement it is measured against (FMT-BREAK-011, FMT-BREAK-012) are unchanged. RH5107's analyzer still does not report a comma that a comment keeps off its parameter's line (SHARED-JOIN-001): analyzers do not consult the precheck, and the fix is a text edit outside the formatter, which SHARED-FIX-010 does not withhold.

## Alternatives considered

- **The fix follows the formatter: it moves the comment with the comma** — rejected: the fix is a hand-written text edit, not a formatter call, so it would need a second copy of FMT-BREAK-012's comment placement, distinguishing a `//` comment, a block comment ending the line, and a block comment followed by the next parameter, for an input class that is rare by definition. Making the fix delegate its layout to the formatter is a separate, structural change and a follow-up candidate.
- **The formatter follows the fix** — rejected: the fix's result for a comment that ends the comma's line violates SHARED-FIX-003, and FMT-BREAK-012 derives the argument-list placement from the parameter list's, so parameter lists alone would become the exception.
- **Leave both as they are** — rejected: the fix keeps a result that violates SHARED-FIX-003, and formatting that result gives a third output.

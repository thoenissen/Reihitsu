# RH2109 reads a line inside a multi-line C# comment or literal like markup

| | |
|---|---|
| Status | Draft |
| Date | 2026-11 |
| Affected requirements | RH2109-001, RH2109-004, SHARED-DIAG-003 |

## What

- The new requirement RH2109-004 states that RH2109 reads a `.razor` file as Razor markup and does not lex its C#. A line inside a multi-line C# comment or literal — `/* */`, `/** */`, verbatim, raw, interpolated verbatim, or interpolated raw string — is read like a line of markup in two directions:
  - a `@code` followed by `{` that starts such a line is reported like a directive;
  - an `@*` or `<!--` that starts such a line opens a Razor or HTML comment for the rule, so a `@code` up to the `*@` that closes an `@*`, or the `-->` that closes an `<!--`, or up to the end of the file when that closer does not follow, is not reported, also when it is a real directive; a closer of the other kind does not end the comment.
- RH2109-001 previously required, through SHARED-DIAG-003, that such a `@code` is not reported. For this class it now points to RH2109-004 instead; every other case SHARED-DIAG-003 makes text stays unreported.
- SHARED-DIAG-003 gains one sentence stating that RH2109 is an exception to it for this class, as RH2109-004 states.

## Why

The analyzer receives a `.razor` file as plain text, not as a syntax tree. Deciding whether a line lies inside a C# comment or literal needs where Razor's C# starts and ends — across template markup, `<text>`, `@:` lines, and builder methods or lambdas that hold markup — and a lexer for C# comments and every literal form, raw and interpolated raw strings with holes included. That is a parser of text, which `AGENTS.md` (*Special cases do not grow the source*) refuses for a rare input; an earlier attempt to build it was refused after two audit rounds found further missed real directives. The input is rare — commented-out or quoted Razor source — and when the host is a `@code` block, the file already carries this rule's diagnostic.

## Visible output changes

None — no output an end user sees changes. The shipped analyzer already reports `@code {` on such a line and already skips from a line-starting `@*` or `<!--` inside C#; only the requirement changes to match it.

```razor
@code {
    string s = """
@code {
""";
}
```

Before: reported on line 1 and on line 3.

After: reported on line 1 and on line 3.

## Decisions

### The class stays reported

The maintainer chose to keep the diagnostic on a `@code {` that starts a line inside a multi-line C# comment or literal (option A of the run's output question), and to record the converse miss — a line-starting `@*` or `<!--` inside C# that hides a later real `@code {` — in the same requirement, so both read as a decision rather than an oversight.

## Alternatives considered

- **Not reported (option B)** — rejected: it needs a scanner of Razor's C# extents and a C# lexer over text, machinery the repository does not grow for a rare input.
- **Recording only the reported direction** — rejected: the converse miss comes from the same decision not to lex C#, and leaving it out would make it read as a defect.
- **Changing SHARED-DIAG-003 to carve out the exception there** — rejected: the exception belongs to one rule; the shared requirement only names it and points to RH2109-004.
- **Recording the class inside RH2109-001** — rejected: its reason (the cost of lexing C# in Razor text) differs from RH2109-001's, so it is a requirement of its own.

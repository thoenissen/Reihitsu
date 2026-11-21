# A comment or directive between a link's `.` and its member name is unsupported

| | |
|---|---|
| Status | Accepted |
| Date | 2026-11 |
| Affected requirements | FMT-UNSUP-016 (new), FMT-UNSUP-015 (changed: the gap between a link's `.` and its member name is FMT-UNSUP-016's), FMT-BREAK-020 (changed: the join of a member name onto its dot never meets the shape), RH2001-007 (changed: a comment between a `.` and its member name in the initializer's value withholds the fix), RH7101-007 (changed: names FMT-UNSUP-016 among the shapes that withhold the fix) |
| Depends on | [A comment or directive inside a `?.` or `!.` operator is unsupported](unsupported-link-operator-trivia.md), [The formatter's Unsupported outcome](unsupported-precheck.md), [Declaring a shape unsupported is a refusal](unsupported-shape-as-refusal.md) |

## What

The formatter declares a new unsupported shape (FMT-UNSUP-016): a comment, a preprocessor directive, or disabled text between the `.` of a link — a simple member access or a member binding (SHARED-CHAIN-001) — and the member name behind it, whether the gap holds a line break or not. Every formatting entry point refuses an input that holds the shape, returns it unchanged, and reports it with the reason `a comment or directive between a . and its member name` and the location of the first comment, directive, or disabled text in that place; the CLI leaves the file unchanged and reports it; and every code fix whose formatted node holds the shape is withheld (SHARED-FIX-010), its diagnostic staying.

So the line-break phase's join of a member name onto its dot (FMT-BREAK-020) never meets a comment, a directive, or disabled text, and no member name of a link ever starts a line. FMT-BREAK-020 states this; FMT-UNSUP-015 hands the gap behind a link operator's `.` to the new requirement; and RH2001-007 and RH7101-007 name the new shape among those that withhold their fixes.

Not refused: the gap in front of a link operator (SHARED-CHAIN-004 and the chain layout), the gaps between the operator's own tokens (FMT-UNSUP-015), the gap behind the member name and the inside of the link's argument or type-argument list, and the `.` of anything that is not a member access or a member binding — a qualified name (`System. // c⏎IDisposable` in a base list, FMT-INDENT-013), an alias-qualified name's `::`, a pointer member access's `->`, and a cref in a documentation comment.

No analyzer rule is added or changed, and no rule page changes.

## Why

On `main` the line-break phase joins a member name split from its dot back onto the dot (FMT-BREAK-020), but refuses to when a comment, a directive, or disabled text stands in the gap (FMT-BREAK-001). The name then starts a line that no requirement places: at statement level it takes the column of the enclosing statement, where `C();` reads as a new statement; in a wrapped chain the `.` is left on a line of its own and a blank line is inserted inside the statement in front of an own-line comment; the branches of a conditional group between the dot and the name get different columns; and on one line, the space behind a block comment in the gap is removed. Inside a list element the name line happens to align with the element (FMT-INDENT-013), but only because the element's rule covers every line inside it.

A comment between a `.` and its member name is rare; a comment that explains a link belongs behind the line or above the statement. Placing the name below its dot would need a column rule of its own in the indentation phase and would keep the join refusal. Refusing the shape removes the join refusal for this gap from the chain layout (*Special cases*), and the refusal is reported, so the user learns why the file was left as written.

The maintainer decided, while the run for FMT-UNSUP-015 was open, to declare this shape unsupported in a change of its own (*Decisions*); this is that change.

## Visible output changes

Every *before* below is the output of `main`, produced with `scripts/format.sh` for the formatter and with `scripts/apply-fix.sh` for the code fixes. The CLI's line has the form it prints today for the other unsupported shapes; the reason text is the one the maintainer chose (*Decisions*).

**1. A comment, a directive, or disabled text between a link's `.` and its member name refuses the file.**

Input — a `//` comment behind the dot (the issue's example):

```csharp
class C
{
    object M(dynamic a)
    {
        var r = a.B(). // c
            C();

        return r;
    }
}
```

Before (`main`): `C();` moves to the column of the statement.

```csharp
class C
{
    object M(dynamic a)
    {
        var r = a.B(). // c
        C();

        return r;
    }
}
```

After: the file is unchanged, and the CLI reports

```
error: <file>(5,24): unsupported syntax: a comment or directive between a . and its member name. The file was left unchanged.
```

Input — a conditional group between the dot and the name:

```csharp
#nullable enable

class C
{
    string M(string s)
    {
        return s.
#if DEBUG
            Trim();
#else
            ToUpper();
#endif
    }
}
```

Before (`main`), in a build without `DEBUG`: `ToUpper();` moves to the column of the statement, while the disabled `Trim();` keeps its column, so the two branches get different columns.

```csharp
#nullable enable

class C
{
    string M(string s)
    {
        return s.
#if DEBUG
            Trim();
#else
        ToUpper();
#endif
    }
}
```

After: the file is unchanged, in every build, and the CLI reports `error: <file>(8,1): unsupported syntax: a comment or directive between a . and its member name. The file was left unchanged.` Written around the whole statement, the same choice is formatted.

Input — an own-line comment between the dot and the name in a wrapped chain:

```csharp
class C
{
    object M(dynamic a)
    {
        var r = a.B()
                 .C().
                 // c
                 D()
                 .E();

        return r;
    }
}
```

Before (`main`): the `.` is left on a line of its own, a blank line is inserted inside the statement, and the comment and `D()` move to the column of the statement.

```csharp
class C
{
    object M(dynamic a)
    {
        var r = a.B()
                 .C()
                 .

        // c
        D()
                 .E();

        return r;
    }
}
```

After: the file is unchanged, and the CLI reports `error: <file>(7,18): unsupported syntax: a comment or directive between a . and its member name. The file was left unchanged.`

Input — block comments between the dot and the name on one line, behind a plain `.` and behind a `?.`:

```csharp
#nullable enable

class C
{
    int? M(string? s, string t)
    {
        var a = s?. /* c1 */ Length;
        var b = t. /* c2 */ Length;

        return a + b;
    }
}
```

Before (`main`): the space behind each comment is removed.

```csharp
        var a = s?. /* c1 */Length;
        var b = t. /* c2 */Length;
```

After: the file is unchanged, and the CLI reports `error: <file>(7,21): unsupported syntax: a comment or directive between a . and its member name. The file was left unchanged.`

The analyzers do not change: no rule is added, and analyzers do not consult the precheck. No rule page changes.

**2. Code fixes that format a node holding the shape are withheld** (SHARED-FIX-010); their diagnostics stay.

RH5201 (`Method chains should be aligned`), input:

```csharp
class C
{
    object M(dynamic a)
    {
        var r = a.B()
                 .C(). // note
                 D()
            .E();

        return r;
    }
}
```

Before (`main`): the fix is offered and gives

```csharp
        var r = a.B()
                 .C()
                 . // note
        D()
                 .E();
```

After: no fix offered.

RH5112 (`Wrapped chains should keep the first member access on the original line`), input:

```csharp
class C
{
    object M(dynamic a)
    {
        var r = a
            .B(). // note
            C();

        return r;
    }
}
```

Before (`main`): the fix is offered and gives

```csharp
        var r = a.B()
                 . // note
        C();
```

After: no fix offered.

RH3202 (`Expression style methods should not be used`), input:

```csharp
class C
{
    int M(string s) => s. // note
                       Length;
}
```

Before (`main`): the fix is offered and gives

```csharp
class C
{
    int M(string s)
    {
        return s. // note
        Length;
    }
}
```

After: no fix offered.

RH2001 (`Private auto-implemented properties should not be used`), input:

```csharp
class C
{
    private static string Source = "";

    private int Value { get; set; } = Source. // three
                                      Length;
}
```

Before (`main`): the fix is offered and gives

```csharp
    private int _value = Source. // three
    Length;
```

After: no fix offered (RH2001-007).

RH7101 (`Do not combine fields`), input:

```csharp
class C
{
    private static string Source = "";

    private int _a = Source. // one
                     Length, _b;
}
```

Before (`main`): the fix is offered and gives

```csharp
    private int _a = Source. // one
    Length;
    private int _b;
```

After: no fix offered (RH7101-007). The fix formats the whole type, so the shape anywhere in the type withholds it.

The same holds for every other fix that formats through the formatter, wherever the node it formats holds the shape. No rule page changes: the rules lose their fix only for the refused shape, which `documentation/rules/general-notes.md` covers ("A diagnostic without a code fix").

## Special cases

### Deleted

- **The refusal to close the gap between a link's dot and its member name** — `ChainLineBreakRewriter.NormalizeChain` selects that gap through `CloseGap`, whose `LineBreakTriviaUtilities.WouldJoinAcrossUnjoinableTrivia` check skips a gap that holds a comment, a directive, or disabled text. No such gap reaches the phase any more, so selecting it needs only the check for a line break, as the gaps inside a link operator already do (`CloseInnerGaps`). The dot and the name belong to one node — the member access or the member binding — so a node-level input either holds both, and the precheck sees the shape, or holds no such gap at all (FMT-UNSUP-003).

### Checked and kept

- **`CloseGap`'s check itself** — it also serves the gap in front of the chain's first link (SHARED-CHAIN-004), which `ChainLineBreakRewriter.ApplyWrappedLayout` closes and which is not in the shape.
- **The check of the root line in `ChainLineBreakRewriter.GetWrappedLayoutLineBreaks`** — it reads the gap in front of the first link (SHARED-CHAIN-004), not the gap behind a dot.
- **`FluentChainLink.HasInnerLineBreak`** — it reads only the gaps between a link operator's tokens; a line break between a dot and its name is no wrap (FMT-BREAK-020), and no wrap decision reads that gap.
- **`ListElementInteriorJoinRewriter.CanJoin`'s check, and `IsOperatorWrap`** — the check guards every gap inside a list element; the gap behind a member access's or member binding's `.` is an operator wrap the chain layout owns (`IsOwnedGap`), so the list-element join never decided the shape.
- **The alignment of a line inside a list element (FMT-INDENT-013)** — it places every line inside an element, a qualified name's continuation in a base list among them, and has no case of its own for a member name; nothing in the indentation phase places a member name below its dot.
- **`HorizontalSpacingRewriter.NormalizeGapBehindLeadingComment`** — it serves every gap with a comment in the next token's leading trivia, on any token pair; it has no case for the gap behind a dot.
- **The same-line condition in `MethodChainAlignmentContributor.GetChainAnchorColumn`** — it rebases the chain's anchor onto the initializer's `new` keyword only when the anchor sits on the line of the first dot behind the initializer's closing brace. A comment between that dot and its name was one way to put the anchor on a later line; it is refused now, but a link in front of the anchor that spans lines still does it, with no comment and no refused shape: an element access whose index holds a verbatim, raw, or interpolated string over several lines, or an argument list wrapped inside the index (`}.Map[@"a` ⏎ `b"].Trim()` ⏎ `.ToString();`). Without the condition, that input mixes the columns of two lines: the first pass puts `.ToString()` at the start of its line and the second moves it under `.Trim()`, so the output is not idempotent.

## Decisions

### Declare the shape, in a change of its own (maintainer, batch 1 of the FMT-UNSUP-015 run, answer 2A)

While the run for FMT-UNSUP-015 was open, the maintainer was asked whether the gap between a link's `.` and its member name should be declared unsupported, and answered: declare it, in a change of its own, carried by a follow-up issue (recorded in [unsupported-link-operator-trivia.md](unsupported-link-operator-trivia.md), *Decisions*). This change is that declaration; it deletes the special case the shape makes unnecessary and lists it above.

### The shape is a place in the syntax tree, on any line (maintainer, batch 1, answer 2A)

The shape is the gap between the `.` and the member name, whether or not it holds a line break, as for FMT-UNSUP-008, FMT-UNSUP-011, FMT-UNSUP-014, and FMT-UNSUP-015. A block comment there on one line (`t. /* c */ Length`) is refused too; on `main` the formatter removes the space behind it (change 1, last input). Refusing only a gap that holds a line break would make the boundary depend on layout. The maintainer confirmed that the one-line form is refused too, in this change (batch 1, answer 2A).

### Directives and disabled text belong to the shape

The join refusal tests comments, directives, and disabled text alike; with comments alone, it would have to stay.

### Member accesses and member bindings only

The `.` of a qualified name is not a link (SHARED-CHAIN-001), the chain layout never joins its name onto it, and FMT-INDENT-013 specifies where a qualified name's continuation goes in a base list. The issue asks for the link's gap only; a qualified name stays supported.

### The reason names the place (maintainer, batch 1, answer 1A)

The reason `a comment or directive between a . and its member name` follows the form of the other reasons, which name the place of the comment, and stays apart from FMT-UNSUP-015's `inside a ?. or !. operator`. The maintainer chose this reason text over `a comment or directive between a member access's . and its name` (batch 1, answer 1A).

## Alternatives considered

- **Place the name below its dot** (the issue's rejected alternative) — rejected: it needs a column rule of its own in the indentation phase and keeps the join refusal.
- **Refuse comments only, and keep directives and disabled text** — rejected: the join refusal would stay for directives and disabled text.
- **Also declare the gap between a qualified name's `.` and its name** — not part of this change: a qualified name is no link, the chain layout has no special case for it, and FMT-INDENT-013 specifies its layout in a base list.
- **Fold the gap into FMT-UNSUP-015** — rejected: FMT-UNSUP-015 is about the tokens of a link operator, and its reason names the operators; a requirement of its own keeps one reason per place.

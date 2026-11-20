# A comment or directive inside a `?.` or `!.` operator is unsupported

| | |
|---|---|
| Status | Draft |
| Date | 2026-11 |
| Affected requirements | FMT-UNSUP-015 (new), FMT-BREAK-019 (changed: the join inside a link operator never meets the shape), RH2001-007 (changed: a comment between the tokens of a link operator in the initializer's value withholds the fix), RH7101-007 (changed: names FMT-UNSUP-015 among the shapes that withhold the fix) |
| Depends on | [The formatter's Unsupported outcome](unsupported-precheck.md), [Declaring a shape unsupported is a refusal](unsupported-shape-as-refusal.md), [An element access or a null-forgiving operator written below its element joins the element's line](attached-part-below-element.md) |

## What

The formatter declares a new unsupported shape (FMT-UNSUP-015): a comment, a preprocessor directive, or disabled text between two tokens of a member-access link operator — between the `?` and the `.` of `?.`, between the `!` and the `.` of `!.`, and between any two tokens of `!?.` — whether the gap holds a line break or not. Every formatting entry point refuses an input that holds the shape, returns it unchanged, and reports it with the reason `a comment or directive inside a ?. or !. operator` and the location of the first comment, directive, or disabled text in that place; the CLI leaves the file unchanged and reports it; and every code fix whose formatted node holds the shape is withheld (SHARED-FIX-010), its diagnostic staying.

So the line-break phase's join inside a link operator (SHARED-CHAIN-006, FMT-BREAK-019) never meets a comment, a directive, or disabled text, and no token of a link operator after the first ever starts a line. FMT-BREAK-019 states this, and RH2001-007 and RH7101-007 name the new shape among those that withhold their fixes.

Not refused: the gap in front of a link operator's first token (SHARED-CHAIN-004 and the chain layout decide it, and RH5048 reports a comment in front of the first link), the gap between a link's `.` and its member name, the inside of a link's argument list, and the `?[` of a conditional element access, whose gaps FMT-UNSUP-014 already refuses.

No analyzer rule is added. thoenissen/Reihitsu#894 proposed a Layout rule that reports these comments without a code fix; the maintainer decided to declare the shape unsupported instead (*Decisions*).

## Why

On `main` the line-break phase moves a line break inside a link operator in front of its first token, but refuses to when a comment, a directive, or disabled text stands between the operator's tokens (FMT-BREAK-001). The operator then stays split, and the indentation phase puts the rest of it under its first token — a column no requirement states. A `?` or `!` left at the end of a line reads as a conditional or a negation, the branches of a conditional group inside the operator get different columns, and an own-line comment between the tokens gets a blank line inserted in front of it, inside the statement. No analyzer rule reports a directive or disabled text there, so the operator stays split without any hint.

A comment between `?` and `.` is rare; a comment that explains a link belongs behind the line or above the statement. Refusing the shape removes the join refusal and the alignment of the operator's rest from the phases (*Special cases*), and the refusal is reported, so the user learns why the file was left as written.

## Visible output changes

Every *before* below is the output of `main`, produced with `scripts/format.sh` for the formatter and with `scripts/apply-fix.sh` for the code fixes. The CLI's line has the form it prints today for the other unsupported shapes.

**1. A comment, a directive, or disabled text inside a `?.`, `!.`, or `!?.` operator refuses the file.**

Input — a `//` comment between `?` and `.`:

```csharp
class C
{
    object M(dynamic a)
    {
        var r = a? // note
            .B();

        return r;
    }
}
```

Before (`main`): `.B()` moves under the `?`.

```csharp
class C
{
    object M(dynamic a)
    {
        var r = a? // note
                 .B();

        return r;
    }
}
```

After: the file is unchanged, and the CLI reports

```
error: <file>(5,20): unsupported syntax: a comment or directive inside a ?. or !. operator. The file was left unchanged.
```

Input — a conditional group between `!` and `.`:

```csharp
#nullable enable

class C
{
    string M()
    {
        return Get()!
#if DEBUG
            .Trim();
#else
            .ToUpper();
#endif
    }

    string? Get()
    {
        return null;
    }
}
```

Before (`main`), in a build without `DEBUG`: `.ToUpper()` moves under the `!`, while the disabled `.Trim()` keeps its column, so the two branches get different columns.

```csharp
#nullable enable

class C
{
    string M()
    {
        return Get()!
#if DEBUG
            .Trim();
#else
                    .ToUpper();
#endif
    }

    string? Get()
    {
        return null;
    }
}
```

After: the file is unchanged, in every build, and the CLI reports `error: <file>(8,1): unsupported syntax: a comment or directive inside a ?. or !. operator. The file was left unchanged.` Written around the whole statement, the same choice is formatted.

Input — a `!.` and a `!?.` split by comments in a wrapped chain:

```csharp
#nullable enable

class C
{
    object? M(dynamic a, string? s)
    {
        var r = a.B()
                 .C()! // c1
                 .D()
                 ?.E();
        var t = s!
            // c2
            ?.Length;

        return r ?? t;
    }
}
```

Before (`main`): each `!` moves onto a line of its own, and a blank line is inserted inside the second statement.

```csharp
#nullable enable

class C
{
    object? M(dynamic a, string? s)
    {
        var r = a.B()
                 .C()
                 ! // c1
                 .D()
                 ?.E();
        var t = s!

                 // c2
                 ?.Length;

        return r ?? t;
    }
}
```

After: the file is unchanged, and the CLI reports `error: <file>(8,24): unsupported syntax: a comment or directive inside a ?. or !. operator. The file was left unchanged.`

Input — block comments inside the operators on one line:

```csharp
#nullable enable

class C
{
    object? M(string? s, string? t, string? u)
    {
        var a = s? /* c1 */ .Length;
        var b = t! /* c2 */ .Length;
        var c = u! /* c3 */ ?.Length;
        var d = u!? /* c4 */ .Length;

        return a ?? b + c + d;
    }
}
```

Before (`main`): the space behind each comment is removed.

```csharp
        var a = s? /* c1 */.Length;
        var b = t! /* c2 */.Length;
        var c = u! /* c3 */?.Length;
        var d = u!? /* c4 */.Length;
```

After: the file is unchanged, and the CLI reports `error: <file>(7,20): unsupported syntax: a comment or directive inside a ?. or !. operator. The file was left unchanged.`

The analyzers do not change: no rule is added, and analyzers do not consult the precheck. RH5201 still reports a link whose operator a comment keeps split only where RH5201-001 or RH5201-002 does (RH5201-003). No rule page changes.

**2. Code fixes that format a node holding the shape are withheld** (SHARED-FIX-010); their diagnostics stay.

RH5201 (`Method chains should be aligned`), input:

```csharp
class C
{
    object M(dynamic a)
    {
        var r = a.B()? // note
                 .C()
            .D();

        return r;
    }
}
```

Before (`main`): the fix is offered and gives

```csharp
        var r = a.B()
                 ? // note
                 .C()
                 .D();
```

After: no fix offered.

RH5112 (`Wrapped fluent calls should keep the first call on the original line`), input:

```csharp
class C
{
    object M(dynamic a)
    {
        var r = a
            .B()? // note
            .C();

        return r;
    }
}
```

Before (`main`): the fix is offered and gives

```csharp
        var r = a.B()
                 ? // note
                 .C();
```

After: no fix offered.

RH3202 (`Expression style methods should not be used`), input:

```csharp
class C
{
    int M(string s) => s? // note
                        .Length ?? 0;
}
```

Before (`main`): the fix is offered and gives

```csharp
class C
{
    int M(string s)
    {
        return s? // note
                .Length ?? 0;
    }
}
```

After: no fix offered.

RH2001 (`Private auto-implemented properties should not be used`), input:

```csharp
class C
{
    private static string Source = "";

    private int? Value { get; set; } = Source? // three
                                             .Length;
}
```

Before (`main`): the fix is offered and gives

```csharp
    private int? _value = Source? // three
                                .Length;
```

After: no fix offered (RH2001-007).

RH7101 (`Do not combine fields`), input:

```csharp
class C
{
    private static string Source = "";

    private int? _a = Source? // one
                            .Length, _b;
}
```

Before (`main`): the fix is offered and gives

```csharp
    private int? _a = Source? // one
                            .Length;
    private int? _b;
```

After: no fix offered (RH7101-007). The fix formats the whole type, so the shape anywhere in the type withholds it.

The same holds for every other fix that formats through the formatter, wherever the node it formats holds the shape. No rule page changes: the rules lose their fix only for the refused shape, which `documentation/rules/general-notes.md` covers ("A diagnostic without a code fix").

## Special cases

### Deleted

- **The refusal to close a gap inside a link operator** — `ChainLineBreakRewriter.CloseInnerGaps` selects the gaps between a link operator's tokens through `CloseGap`, whose `LineBreakTriviaUtilities.WouldJoinAcrossUnjoinableTrivia` check skips a gap that holds a comment, a directive, or disabled text. No such gap reaches the phase any more, so closing a gap inside a link operator needs no such check. (The maintainer named it `TryCloseGap`; the method is called `CloseGap` today.)
- **The alignment of a link operator's rest under its first token** — `MethodChainAlignmentContributor.AlignRestOfRun` placed a token of a link operator that starts a line under the operator's first token. After the line-break phase, no token of a link operator after the first starts a line: a line break with nothing but whitespace beside it is moved in front of the operator (SHARED-CHAIN-006), and every other one is refused. The tokens of one operator belong to one node, so a node-level input either holds the whole operator, and the precheck sees the shape, or holds no link with that operator at all (FMT-UNSUP-003).

### Checked and kept

- **`CloseGap`'s check itself** — it also serves the gap between a link's `.` and its member name (FMT-BREAK-020) and the gap in front of the chain's first link (SHARED-CHAIN-004), which are not in the shape.
- **The check of the root line in `ChainLineBreakRewriter.GetWrappedLayoutLineBreaks`** — it reads the gap in front of the first link (SHARED-CHAIN-004), which is not in the shape.
- **`ChainLineBreakRewriter.IsBehindMultiLineCommentTail` and `EnsureLineBreak`** — they serve a link operator whose first token stands behind a comment that spans several lines (SHARED-CHAIN-005), a gap in front of the operator.
- **`FluentChainLink.HasInnerLineBreak` and its `isJoinableOnly` parameter** — the formatter calls it with `false` to tell whether a chain is wrapped (SHARED-CHAIN-002), which no comment decides. RH5201's analyzer calls it with `true` (RH5201-003); analyzers do not consult the precheck, and what RH5201 reports does not change.
- **`FluentChain.IsFirstLinkBlocked` and the anchor fallback to the root** — they decide the gap in front of the first link (SHARED-CHAIN-004, SHARED-CHAIN-007).
- **`ListElementInteriorJoinRewriter.CanJoin`'s check** — it guards every gap inside a list element; a link operator's gaps are not its own (`IsOwnedGap`), so it never decided the shape.

## Decisions

### Declare the shape, in this change, and add no rule (maintainer, triage of thoenissen/Reihitsu#894)

The issue proposed a Layout rule that reports a comment, a directive, or disabled text inside a link operator, without a code fix. The maintainer decided to declare the shape unsupported instead, in this issue's own change, and dropped the proposed rule with its "no code fix" section (escalation 10, answered before the run). The change deletes the special cases the shape makes unnecessary and lists them above.

### The shape is a place in the syntax tree, on any line

The shape is each gap between two tokens of a link operator, whether or not it holds a line break, as for the operators of FMT-UNSUP-008, FMT-UNSUP-011, and FMT-UNSUP-014. A block comment there on one line (`s? /* c */ .Length`) is refused too; on `main` the formatter removes the space behind it (change 1, last input). Refusing only a gap that holds a line break would make the boundary depend on layout, and a later phase could not tell the two apart without a check of its own.

### Directives and disabled text belong to the shape

The join refusal and the alignment of the operator's rest test comments, directives, and disabled text alike; with comments alone, both would have to stay.

### `?[` stays FMT-UNSUP-014's

The issue lists `?[` among the link operators. In the chain model a conditional element access is an attached part, not a link (SHARED-CHAIN-001), and FMT-UNSUP-014 already refuses a comment, a directive, or disabled text between its `?` and `[`, and between a `!` and the `?` of `!?[`. FMT-UNSUP-015 covers `?.`, `!.`, and `!?.`, so no gap is declared twice and no reason changes.

### The reason names the operators

The reason `a comment or directive inside a ?. or !. operator` names the two operators users write; `!?.` combines both. It follows the form of the other reasons, which name the place of the comment.

## Alternatives considered

- **A new Layout rule that reports the trivia, without a fix** (the issue's proposal) — rejected by the maintainer: the formatter would keep the join refusal and the alignment of the operator's rest, and the diagnostic would only point at a shape the formatter still handles silently.
- **Refuse comments only, and keep directives and disabled text** — rejected: both special cases would stay for directives and disabled text.
- **Also declare the gap between a link's `.` and its member name** — not part of this change: the issue is about the operator's own tokens, and `CloseGap`'s check stays for the gap in front of the first link anyway. It is recorded as a follow-up candidate of the run.

# A directive inside a field declaration with several declarators is unsupported

| | |
|---|---|
| Status | Draft |
| Date | 2026-11 |
| Affected requirements | FMT-UNSUP-010 (added), FMT-UNSUP-009 (changed: the reason at a declarator comma that FMT-UNSUP-010 also refuses), FMT-STRUCT-022 (changed: the leading trivia carries a directive to the first field), FMT-STRUCT-023 (retired), FMT-STRUCT-024 (changed: FMT-STRUCT-023 no longer named), FMT-PIPE-029 (changed: FMT-STRUCT-023 no longer named), FMT-PIPE-031 (changed: no directive condition), SHARED-FIX-011 (changed: example reference), RH7101-005 (changed: withheld through the precheck, not for a directive in front of the declaration) |
| Depends on | [The formatter's Unsupported outcome](unsupported-precheck.md), [Declaring a shape unsupported is a refusal](unsupported-shape-as-refusal.md), [A comment or directive in front of a comma on a later line is unsupported](comma-behind-comment-unsupported.md) |

## What

The formatter declares a new unsupported shape (FMT-UNSUP-010): a preprocessor directive or disabled text in the trivia of a token of a field declaration with more than one declarator, from its first token through its `;`, initializer values included — except the leading trivia of the declaration's first token. Every entry point refuses an input that holds it and reports it with the reason `a directive inside a field declaration with several declarators`; the CLI leaves the file unchanged and reports it (CLI-019); and every code fix whose formatted node holds it is withheld (SHARED-FIX-010).

The field split (FMT-STRUCT-021) loses its directive guard. A field declaration whose only directives and disabled text stand in its leading trivia — a `#region` in front of it, the `#if` or `#else` of a group that holds the whole declaration, the `#endif` and the disabled text of a group in front of it — is no longer kept whole: it is split, and the leading trivia stays on the first field (FMT-STRUCT-022). The RH7101 fix is offered for it and splits it (RH7101-005).

This change covers thoenissen/Reihitsu#1728 (the shape) and thoenissen/Reihitsu#1586 (the declaration with a directive in front of it), which share their cause: the split's one guard for every directive in a field declaration.

## Why

The split repeats the attributes, modifiers, and type on every field and carries over only the declaration's leading trivia, the declarators with their values, and the trivia behind the `;`. A directive anywhere else would be dropped, would end up guarding a different field, or — for a conditional group that starts in one value and ends in another or in front of the `;` — would put the repeated modifiers and type into another build's text. The split therefore kept every declaration with a directive anywhere in its trivia whole, through `FieldDeclarationSplitTransform.CarriesDirective`, and the RH7101 fix withheld itself by asking for it (FMT-PIPE-031, RH7101-005).

That guard was wider than the danger. The leading trivia of the declaration's first token is the place of a `#region` and of the directives of a group around the declaration, which is common code; the split keeps that trivia on the first field unchanged, so a region still opens in front of the same code and a group around the declaration holds all of its fields. Kept whole there, the declaration carried an RH7101 diagnostic that neither the formatter nor the fix ever corrected (thoenissen/Reihitsu#1586). Everywhere else in the declaration, a directive is rare, and the maintainer decided to refuse it as a shape rather than keep the split's special case (thoenissen/Reihitsu#1728).

## Visible output changes

### A directive inside the declaration refuses the file

Input:

```csharp
class C
{
    int a,
#if X
        b,
#endif
        c;

    void M(){ }
}
```

Before (`main`, `scripts/format.sh`):

```csharp
class C
{
    int a,
#if X
        b,
#endif
    c;

    void M()
    {
    }
}
```

After: the file is unchanged, and the CLI reports

```
error: <file>(4,1): unsupported syntax: a directive inside a field declaration with several declarators. The file was left unchanged.
```

The same holds for a directive inside an initializer value, which `main` keeps unsplit and lays out:

```csharp
using System;

class C
{
    private Func<int> _a = () =>
    {
#if DEBUG
        return 1;
#else
        return 2;
#endif
    }, _b;
}
```

Before (`main`):

```csharp
using System;

class C
{
    private Func<int> _a = () =>
                           {
#if DEBUG
        return 1;
#else
                               return 2;
#endif
                           }, _b;
}
```

After: the file is unchanged, and the CLI reports `error: <file>(7,1): unsupported syntax: a directive inside a field declaration with several declarators. The file was left unchanged.`

### Code fixes on code that holds the shape are withheld

A code fix whose formatted node holds such a declaration is no longer offered there (SHARED-FIX-010); the diagnostics stay. The fixes affected are those that format the declaration, its type, or a context that holds it, among them RH7101 (the type), RH5110 and RH5404 (the type), RH5113 (the declaration), RH5111 on a declarator of it (the declaration as context), and RH5112 and RH5201 on a chain in one of its values (the declaration as context). For example, RH7101's fix of another declaration of the same type:

Input:

```csharp
class C
{
    private int _p, _q;

    private int _a,
#pragma warning disable CS0169
        _b;
#pragma warning restore CS0169
}
```

Before (`main`, `scripts/apply-fix.sh RH7101`): the fix of `_p, _q` is offered and gives

```csharp
class C
{
    private int _p;
    private int _q;

    private int _a,
#pragma warning disable CS0169
        _b;
#pragma warning restore CS0169
}
```

After: no code fix is offered for `_p, _q` nor for `_a, _b`; both are reported. A fix that formats only part of the declaration, such as RH5301, RH5303, or RH5305 on an initializer value, is not affected (FMT-UNSUP-003).

### A directive in front of the declaration no longer keeps it whole

Input:

```csharp
class C
{
    #region Fields

    private int _x, _y;

    #endregion // Fields

#if !RELEASE
    private int _p, _q = 1;
#endif
}
```

Before (`main`): the formatter keeps both declarations combined; RH7101 reports both, and no code fix is offered.

After (formatter and RH7101 fix, checked on `main` with the directive guard removed):

```csharp
class C
{
    #region Fields

    private int _x;
    private int _y;

    #endregion // Fields

#if !RELEASE
    private int _p;
    private int _q = 1;
#endif
}
```

The fixes that format the whole type, RH5110 and RH5404, split such a declaration as part of their result, as they already split every other combined field of the type.

No rule page changes: `documentation/rules/RH7101.md` says nothing about directives, and the withheld fixes are covered by `documentation/rules/general-notes.md` ("A diagnostic without a code fix").

## Decisions

### Declare the shape, in a change of its own (maintainer, escalation 10)

The maintainer decided to declare a directive or disabled text inside a field declaration with several declarators unsupported, in a change of its own, in place of the split's directive guard (thoenissen/Reihitsu#1728).

### The leading trivia of the first token is not part of the shape

Open: this is the output question of thoenissen/Reihitsu#1586, asked in this run. The spec is written on the recommendation — the leading trivia is excepted and the declaration is split — because the split keeps that trivia on the first field unchanged, and refusing it would refuse every file whose combined field follows a `#region` or ends a conditional group in front of it. The alternatives are recorded below.

### No initializer value is excepted

FMT-UNSUP-004 excepts the inside of a value; this requirement does not. A conditional group can start in one value and end in another value or in front of the `;`, and the split would then put the repeated modifiers and type into another build's text. Telling a group that stays inside one value from one that crosses a comma needs the directives matched up, which is no cheap check (`AGENTS.md`, *Special cases do not grow the source*); a directive in a value of a declaration with several declarators is rare.

### The declaration's own tokens decide, whatever declares it

FMT-UNSUP-003 decides the shape from the input's own tokens, and the node the input stands in does not decide it; so the shape is any field declaration with more than one declarator, also one that RH7101-002's extension block would hold, not only the members FMT-STRUCT-021 splits. The declarators are those of the build being formatted, as FMT-UNSUP-009 counts commas: a declarator in disabled text does not count.

### FMT-UNSUP-009 keeps its reason at a declarator comma

A directive between a declarator and a comma on a later line is refused today under FMT-UNSUP-009, and FMT-UNSUP-010 refuses it as well, starting at the same directive. The reason and the location stay FMT-UNSUP-009's, which is what such a declaration reports today; every other directive in the declaration gives FMT-UNSUP-010's.

## Special cases

### Deleted

- `FieldDeclarationSplitTransform.CarriesDirective` and its operand in `FieldDeclarationSplitTransform.Splits`, with the sentences of their documentation and of `SplitFields` that explain it. Every directive and disabled text it found outside the leading trivia of the first token is in FMT-UNSUP-010's span, and the split never meets a node-level input that holds part of a field declaration: it runs on the member list of a type, so every declaration it splits is in the input whole (FMT-UNSUP-003).
- The directive condition of `ReihitsuFormatter.SplitsFieldDeclaration` (FMT-PIPE-031), which is `Splits`, and the sentence of its documentation.
- FMT-STRUCT-023 (retired) and its mentions in FMT-STRUCT-024, FMT-PIPE-029, FMT-PIPE-031, and SHARED-FIX-011; RH7101-005's own withholding, which now follows from SHARED-FIX-010 through the type the fix already asks about.

### Checked and kept

- RH7101's analyzer condition that a conditional directive between the declarators stops the report (`SyntaxTriviaUtilities.ContainsConditionalCompilationBoundary`, RH7101-001): analyzers do not consult the precheck, and whether the rule reports is its own decision.
- `FieldDeclarationSplitTransform.Splits`' declarator count, owner kinds, and documentation condition (FMT-STRUCT-021, FMT-STRUCT-030): they serve declarations without a directive.
- `CreateSplitField`'s dropping of a later declarator's leading trivia and a separator's trailing trivia: no guard; that trivia now provably holds no comment (FMT-UNSUP-004) and no directive or disabled text (FMT-UNSUP-010).
- FMT-UNSUP-009's refusal of a directive in front of a declarator comma on a later line: it also serves local declarations and every other list.
- FMT-STRUCT-028's "no split with a comment or a directive between the brackets": it serves attribute lists of every declaration, also of a field with one declarator.
- The RH7101 fix's call of `SplitsFieldDeclaration` and `FindUnsupportedSyntax` on the type: unchanged; together they now withhold the fix for the shape and offer it for a directive in the leading trivia.

## Alternatives considered

- **Keep the guard** — rejected by the maintainer (thoenissen/Reihitsu#1728): the declaration stays unsplit silently, and the split keeps a special case for a rare input.
- **Refuse the leading trivia too** (thoenissen/Reihitsu#1586, open) — it would refuse every file in which a `#region`, a group around the declaration, or the end of a group in front of it precedes a combined field: common code, refused as a whole, where `main` formats it and only keeps the declaration combined.
- **Keep the declaration whole where a directive leads it** (thoenissen/Reihitsu#1586, open) — keeps a narrowed copy of `CarriesDirective` for the leading trivia, and the RH7101 diagnostic stays without a fix for a common shape, although the split drops and moves nothing there.
- **Except the inside of an initializer value, as FMT-UNSUP-004 does** — rejected: a conditional group that crosses a comma would reach the split, and recognizing one needs the directives matched up.
- **Declare it for every variable declaration** — rejected: locals, `for` initializers, and `using` and `fixed` declarations are not split, so the wider shape deletes nothing (`AGENTS.md`: a shape that deletes nothing is not declared).

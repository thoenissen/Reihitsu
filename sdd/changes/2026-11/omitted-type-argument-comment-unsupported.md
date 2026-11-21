# Comments and directives inside a list of omitted type arguments are unsupported

| | |
|---|---|
| Status | Accepted |
| Date | 2026-11 |
| Affected requirements | FMT-UNSUP-018 (new), FMT-UNSUP-009, FMT-INDENT-015, RH7101-007 |

## What

A comment, a directive, or disabled text inside a type argument list whose arguments are all omitted (`Dictionary<,>`, `List<>`) is a shape the formatter refuses as a whole (FMT-UNSUP-018). The CLI leaves such a file unchanged and reports it, and every code fix whose formatted node holds the shape is withheld (SHARED-FIX-010).

FMT-INDENT-015 loses its exception for a wrapped list of omitted type arguments: such a list never reaches the indentation phase wrapped, because FMT-BREAK-014 joins it and the precheck refuses whatever would keep it wrapped.

This change covers the omitted-type-argument case of thoenissen/Reihitsu#953 (question 4 of the run).

## Why

A list of omitted type arguments has no first element with text, so FMT-INDENT-015's alignment to the first element had a case of its own that left such a list to block indentation. The first draft of this run made its lines continuation lines (FMT-INDENT-034); the maintainer decided to refuse the comment that keeps the list wrapped instead, so the exception goes (batch 1b, answer 4b A). Such a list only names an unbound generic type in a `typeof` or a `nameof`, and a comment or a conditional group inside it is rare.

## Visible output changes

Every *before* below is the output of `main`, produced with `scripts/format.sh`. The CLI's line has the form it prints today for the other unsupported shapes.

Input:

```csharp
class C
{
    void M()
    {
        var type = typeof(System.Collections.Generic.Dictionary<, // c
            >);
    }
}
```

Before (`main`): `>);` moves to the column of the statement.

```csharp
class C
{
    void M()
    {
        var type = typeof(System.Collections.Generic.Dictionary<, // c
        >);
    }
}
```

After: the file is unchanged, and the CLI reports

```
error: <file>(5,67): unsupported syntax: a comment or directive inside a list of omitted type arguments. The file was left unchanged.
```

A comment inside such a list on one line, `typeof(System.Collections.Generic.List</* c */>)`, which `main` formats, refuses the file the same way. A comment behind the list or in a list that holds a type (`Dictionary<int, /* c */ string>`) is formatted as on `main`.

Code fixes: every code fix whose formatted node holds such a list with a comment or a directive is withheld. No spec shows one, and no rule reports a comment inside a list of omitted type arguments.

No rule page changes.

## Special cases

### Deleted

- **The zero-width anchor guard in `ListElementAligner.AlignToFirstElement`** — it left a list whose first element is an omitted type argument to block indentation, because that element has no column. With the shape refused and FMT-BREAK-014 joining every other such list, no list of omitted type arguments reaches the indentation phase wrapped, and the other callers pass attribute lists, whose first element always has text. The redundant skip of an empty first token in `ListElementAligner.AlignContinuations` goes with it: an omitted element holds no token other than its own.

### Checked and kept

- **`LineBreakTriviaUtilities.CollapseTokenToSameLine`'s reach back behind a token without text** — it serves the join of a list of omitted type arguments that holds no comment (`typeof(Dictionary<⏎,>)`), which FMT-BREAK-014 still makes, and the join behind an omitted array size.
- **`TokenLocator.TryGetPreviousToken`'s inclusion of zero-width tokens** — it serves every lookup of the token in front of another, not this shape.
- **The precheck's handling of an omitted element in front of a comma (FMT-UNSUP-009)** — it decides FMT-UNSUP-009's gap, which starts behind the token in front of an omitted element; where both shapes start at the same comment, FMT-UNSUP-009's reason is given.
- **SHARED-BLANK-008's rule that a token without text never starts its line** — it holds for an omitted array size as well; its example now shows the closing `]` of a rank specifier instead of the `>` of a list of omitted type arguments.

## Decisions

### Declare the shape, in this run (maintainer, batch 1b, answer 4b A)

The maintainer chose to declare a comment inside a list of omitted type arguments unsupported in this run, and to delete the FMT-INDENT-015 exception.

### Directives and disabled text belong to the shape (maintainer, batch 2, answer 7 A)

A directive or disabled text inside such a list keeps it wrapped like a comment does. With comments alone, the indentation phase would keep its case for a wrapped list of omitted type arguments, and the exception could not go. Every other declared shape includes them for the same reason.

### Only lists whose every argument is omitted

A type argument list with a type in it has a first element with text to align to, and FMT-INDENT-015 places it; a comment in it is formatted as on `main`.

## Alternatives considered

- **Indent the list's later lines as continuation lines (FMT-INDENT-034)** — rejected by the maintainer in favour of the declaration.
- **Keep FMT-INDENT-015's block-column exception** — rejected: it is a special case for a rare input.
- **Declare comments only** — rejected by the maintainer (batch 2, answer 7 A): a directive would keep the list wrapped, and the exception would have to stay for it.

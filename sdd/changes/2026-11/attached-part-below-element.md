# An element access or a null-forgiving operator written below its element joins the element's line

| | |
|---|---|
| Status | Accepted |
| Date | 2026-11 |
| Affected requirements | FMT-BREAK-040 (new), FMT-UNSUP-014 (added), FMT-BREAK-039 (changed: the join never meets the shape; its comment example is refused), FMT-BREAK-020, FMT-BREAK-015, FMT-BREAK-031, RH2001-007 (changed: a comment in front of an argument list, an element access, or a null-forgiving operator of the initializer's value withholds the fix), RH7101-007 (changed: names FMT-UNSUP-014 among the shapes that withhold the fix), FMT-SPACE-028 (changed: a comment in the gap between `with` and its argument list is refused; its example `[with /* size */ (4)]` is removed) |
| Depends on | [An argument list written below its name joins the name's line](argument-list-below-name.md), [The formatter's Unsupported outcome](unsupported-precheck.md), [Declaring a shape unsupported is a refusal](unsupported-shape-as-refusal.md) |

## What

The line-break phase joins an element access, a conditional element access, and a null-forgiving operator onto the line of the token in front of it when a line break separates the two (FMT-BREAK-040): `arr⏎[0]` becomes `arr[0]`, `n⏎?[0]` becomes `n?[0]`, and `Get()⏎!` becomes `Get()!`, wherever the part stands. Blank lines in the gap go with the join (FMT-BREAK-030). The line break the join removes counts for a surrounding construct as the one in front of an argument list does (FMT-BREAK-039): a collection expression or another construct of FMT-BREAK-022 written over several lines only because of it keeps one element per line, and a list (FMT-BREAK-012) and a conditional expression (FMT-BREAK-018) are judged as the line they become.

FMT-BREAK-040 is the one requirement that owns these gaps, as FMT-BREAK-039 owns the gap in front of an argument list's `(`. FMT-BREAK-020, which joined an element access and a conditional element access in a chain with at least one link, now states only the rejoin of a split member name and points to FMT-BREAK-039 and FMT-BREAK-040 for attached parts. FMT-BREAK-015 names the new owner among the gaps it leaves alone. FMT-BREAK-031 lists the new join among the joins that write the minimal separator; it changes no output of its own.

The formatter declares a new unsupported shape (FMT-UNSUP-014): a comment, a preprocessor directive, or disabled text in the gap in front of an argument list that FMT-BREAK-039 joins, or in front of an element access, a conditional element access, or a null-forgiving operator that FMT-BREAK-040 joins — in the trailing trivia of the token in front of the part's first token or in the leading trivia of that first token — or between the `?` and the `[` of a conditional element access, whether the gap holds a line break or not. Every entry point refuses an input that holds the shape and reports it with the reason `a comment or directive in front of an argument list, an element access, or a null-forgiving operator`; the CLI leaves the file unchanged and reports it; and every code fix whose formatted node holds it is withheld (SHARED-FIX-010). So neither join meets a comment, a directive, or disabled text in its gap, and FMT-BREAK-001 never has to refuse one there. FMT-SPACE-028 no longer keeps a comment between a collection expression's `with` and its argument list; that input is refused. Not refused: the inside of a part's brackets or parentheses, a `!` that starts a link operator (`!.`, `!?.`), and every other `(` and `[` — parameter lists, attribute argument lists, parenthesized expressions, implicit element accesses, collection expressions.

The argument-list case of thoenissen/Reihitsu#895 (`Foo⏎(1)`, primary-constructor base arguments) is already joined on `main` by FMT-BREAK-039 and is not part of this change.

## Why

On `main` the phase joins these parts only in a chain with at least one link, and a conditional element access also outside such a chain. Everywhere else the line break stays, and the indentation phase gives the part's line the column of an unowned continuation line, the enclosing statement's column:

```csharp
// input
var a = list.Count.ToString()
    [0];
var b = arr
    [0];
```

```csharp
// main
var a = list.Count.ToString()[0];
var b = arr
[0];
```

The same construct is laid out differently depending on whether a `.X` appears in the expression, and `[0]` at the statement's column reads as a collection expression or an attribute list, `!` as a negation of what follows. The null-forgiving operator is joined in a chain on `main` too, though FMT-BREAK-020 did not name it; FMT-BREAK-040 states that join as well.

How a list counts such a join differs on `main` by part and position: a list element's `arr⏎[0]` is joined before the list is laid out (`Foo(arr⏎[0], 2)` → `Foo(arr[0], 2)`), but `n⏎?[0]` and a chain's `x.ToString()⏎!` are joined after it, so the list keeps one argument per line. One owner joins every such gap before the list is laid out, as FMT-BREAK-039 does for an argument list.

## Visible output changes

Every *before* below is the output of `main`, produced with `scripts/format.sh`; every *after* was produced by formatting the input with the line break in front of the part removed by hand, so the parts this change leaves alone are today's output, apart from change 4, whose *after* follows the precedent named there.

**1. An element access and a null-forgiving operator outside a chain with links join the line in front of them.**

```csharp
// input
void M(int[] arr, int[]? n, string? s, Dictionary<string, int[]> d)
{
    var a = arr
    [0];
    var b = Get()
    !;
    var c = s

    ! + "";
    var e = d
    ["k"]
    [0];
    var f = this
    [0];
    arr
    [0] = 1;
    var g = (arr)
    [0];
}
```

```csharp
// before
void M(int[] arr, int[]? n, string? s, Dictionary<string, int[]> d)
{
    var a = arr
    [0];
    var b = Get()
    !;
    var c = s

    ! + "";
    var e = d
    ["k"]
    [0];
    var f = this
    [0];
    arr
    [0] = 1;

    var g = (arr)
    [0];
}
```

```csharp
// after
void M(int[] arr, int[]? n, string? s, Dictionary<string, int[]> d)
{
    var a = arr[0];
    var b = Get()!;
    var c = s! + "";
    var e = d["k"][0];
    var f = this[0];
    arr[0] = 1;

    var g = (arr)[0];
}
```

The same holds inside a lambda and an interpolation hole that is not in a list (`() => arr⏎[0]` → `() => arr[0]`, `$"{arr⏎[0]}"` → `$"{arr[0]}"`). A conditional element access outside a chain with links (`n⏎?[0]`) is joined on `main` already.

**2. A conditional expression is judged as the line it becomes.**

```csharp
// input
var t1 = c ? arr
    [0] : 1;
var t2 = c ? s
    ! : "";
```

```csharp
// before
var t1 = c
             ? arr
[0]
             : 1;
var t2 = c
             ? s
!
             : "";
```

```csharp
// after
var t1 = c ? arr[0] : 1;
var t2 = c ? s! : "";
```

(The *before* lines `[0]` and `!` stand at the statement's column.) On `main` a conditional is already judged after the joins in a chain: `c ? x.ToString()⏎! : ""` becomes `c ? x.ToString()! : ""`.

**3. A list is judged as the line it becomes, also for the joins `main` already makes after the list is laid out.**

```csharp
// input
Baz(s
    !, 2);
Baz(n
    ?[0], 2);
Baz(list.Count.ToString()
    !, 2);
Baz($"{arr
[0]}", 2);
Baz($"{list.Count.ToString()
[0]}", 2);
Baz(from v in list select arr
[v], 2);
Baz(new[] { s
! }, 2);
```

```csharp
// before
Baz(s
    !,
    2);
Baz(n?[0],
    2);
Baz(list.Count.ToString()!,
    2);
Baz($"{arr
[0]}",
    2);
Baz($"{list.Count.ToString()[0]}",
    2);
Baz(from v in list select arr
    [v],
    2);
Baz(new[] { s
! },
    2);
```

```csharp
// after
Baz(s!, 2);
Baz(n?[0], 2);
Baz(list.Count.ToString()!, 2);
Baz($"{arr[0]}", 2);
Baz($"{list.Count.ToString()[0]}", 2);
Baz(from v in list select arr[v], 2);
Baz(new[] { s! }, 2);
```

The second, third, and fifth lines change in code `main` already joins: there the join came after the list was laid out. The last four follow the maintainer's answer to change 7 of [the argument-list change](argument-list-below-name.md) for an argument list in the same positions.

**4. A collection expression written over several lines only because of the line break keeps one element per line.**

```csharp
// input
int[] e = [arr
[0], 2];
string[] f = [s
!];
```

```csharp
// before
int[] e = [
              arr
[0],
              2
          ];
string[] f = [
                 s
!
             ];
```

```csharp
// after
int[] e = [
              arr[0],
              2
          ];
string[] f = [
                 s!
             ];
```

This is the layout `main` gives the same split in a chain (`[list.Count.ToString()⏎[0], 1]` → one element per line, `list.Count.ToString()[0]` on its own line), and the layout FMT-BREAK-039 gives an argument list (`[Bar⏎(4)]`).

**5. Code-fix results.** Every code fix whose formatted node holds such a part writes the new layout. Checked with `scripts/apply-fix.sh` on `main`:

```csharp
// input
return a &&
    arr
    [0] > 1;                                 // RH5302
return a &&
    Has(s
    !, 2);                                   // RH5302
```

```csharp
// before
return a
       && arr
       [0] > 1;
return a
       && Has(s
              !,
              2);
```

```csharp
// after
return a
       && arr[0] > 1;
return a
       && Has(s!, 2);
```

No rule page changes: no analyzer rule reports an element access or a null-forgiving operator below its element, and what the rules report does not change.

A `[` that is not the first token of an attached part keeps today's output: the implicit element access of an initializer (`{⏎["a"] = 1,⏎["b"] = 2⏎}`) and the `[` of an array or `stackalloc` creation (`new⏎[]`, `new int⏎[3]`, `stackalloc⏎[]`) are not joined.

**6. A comment, a directive, or disabled text in front of an argument list, an element access, or a null-forgiving operator refuses the file.** Every *before* below is the output of `main`, produced with `scripts/format.sh`. The CLI's line has the form it prints today for the other unsupported shapes.

Input — an element access below a `//` comment:

```csharp
class C
{
    int M(int[] arr)
    {
        var a = arr // first
            [0];

        return a;
    }
}
```

Before (`main`): `[0]` moves to the statement's column.

```csharp
class C
{
    int M(int[] arr)
    {
        var a = arr // first
        [0];

        return a;
    }
}
```

After: the file is unchanged, and the CLI reports

```
Error: <file>(5,21): unsupported syntax: a comment or directive in front of an argument list, an element access, or a null-forgiving operator. The file was left unchanged.
```

Input — a null-forgiving operator below a comment line:

```csharp
#nullable enable

class C
{
    string M(string? s)
    {
        var t = s
            // known to be set
            !;

        return t;
    }
}
```

Before (`main`): a blank line is inserted inside the statement, and `!` moves to the statement's column.

```csharp
#nullable enable

class C
{
    string M(string? s)
    {
        var t = s

        // known to be set
        !;

        return t;
    }
}
```

After: the file is unchanged, and the CLI reports `Error: <file>(8,13): unsupported syntax: a comment or directive in front of an argument list, an element access, or a null-forgiving operator. The file was left unchanged.`

Input — an element access in a chain with links, below a comment:

```csharp
using System.Linq;

class C
{
    int M(int[] items)
    {
        return items.Where(x => x > 0)
                    .ToArray() // the first one
                    [0];
    }
}
```

Before (`main`): unchanged; `[0]` is aligned to the chain's links. After: the file is unchanged, and the CLI reports `Error: <file>(8,32): unsupported syntax: a comment or directive in front of an argument list, an element access, or a null-forgiving operator. The file was left unchanged.`

Input — a conditional group that chooses between two argument lists:

```csharp
class C
{
    int M()
    {
        return Foo
#if DEBUG
            (1);
#else
            (2);
#endif
    }

    int Foo(int x)
    {
        return x;
    }
}
```

Before (`main`): the two branches get different columns.

```csharp
class C
{
    int M()
    {
        return Foo
#if DEBUG
            (1);
#else
        (2);
#endif
    }

    int Foo(int x)
    {
        return x;
    }
}
```

After: the file is unchanged, in every build, and the CLI reports `Error: <file>(6,1): unsupported syntax: a comment or directive in front of an argument list, an element access, or a null-forgiving operator. The file was left unchanged.` Written around the whole statement, the same choice is formatted.

Input — block comments in the gap on one line; the whole file is refused, also the code elsewhere in it that the formatter would correct:

```csharp
class C{
    int M(int[] arr)
    {
        return Foo /* x */ (arr /* first */ [0]);
    }

    int Foo(int x)
    {
        return x;
    }
}
```

Before (`main`): only the brace of `class C{` moves to its own line; both comments stay. After: the file is unchanged, `class C{` included, and the CLI reports `Error: <file>(4,20): unsupported syntax: a comment or directive in front of an argument list, an element access, or a null-forgiving operator. The file was left unchanged.`

Input — a block comment between a collection expression's `with` and its argument list, on one line:

```csharp
using System.Collections.Generic;

class C
{
    List<string> M()
    {
        List<string> d = [with /* size */ (4)];

        return d;
    }
}
```

Before (`main`): the space behind the comment is removed (FMT-SPACE-028).

```csharp
        List<string> d = [with /* size */(4)];
```

After: the file is unchanged, and the CLI reports `Error: <file>(7,32): unsupported syntax: a comment or directive in front of an argument list, an element access, or a null-forgiving operator. The file was left unchanged.`

Input — a comment between the `?` and the `[` of a conditional element access:

```csharp
class C
{
    int? M(int[]? n)
    {
        return n? // first
            [0];
    }
}
```

Before (`main`): `[0]` moves under the `?`.

```csharp
class C
{
    int? M(int[]? n)
    {
        return n? // first
                [0];
    }
}
```

After: the file is unchanged, and the CLI reports `Error: <file>(5,19): unsupported syntax: a comment or directive in front of an argument list, an element access, or a null-forgiving operator. The file was left unchanged.`

The analyzers do not change: no analyzer rule reports these comments, and analyzers do not consult the precheck. No rule page changes.

**7. Code fixes that format a node holding the shape are withheld** (SHARED-FIX-010); their diagnostics stay. Checked with `scripts/apply-fix.sh` on `main`.

RH3202 (`Expression-style methods should not be used`), input:

```csharp
class C
{
    int M(int[] arr) => arr // first
                        [0];
}
```

Before (`main`): the fix is offered and gives

```csharp
class C
{
    int M(int[] arr)
    {
        return arr // first
        [0];
    }
}
```

After: no fix offered.

RH2001 (`Private auto-implemented properties should not be used`), input:

```csharp
class C
{
    private int Value { get; set; } = Offset // three
                                       (3);

    private static int Offset(int value)
    {
        return value;
    }
}
```

Before (`main`): the fix is offered and gives

```csharp
    private int _value = Offset // three
    (3);
```

After: no fix offered. A comment inside the argument list, such as `= Offset(/* three */ 3);`, still does not withhold the fix.

RH7101 (`Do not combine fields`), input:

```csharp
class C
{
    private int _a = Offset // one
                     (1), _b;

    private static int Offset(int value)
    {
        return value;
    }
}
```

Before (`main`): the fix is offered and gives

```csharp
    private int _a = Offset // one
    (1);
    private int _b;
```

After: no fix offered. The fix formats the whole type, so the shape anywhere in the type withholds it.

RH5112 (`Wrapped fluent calls should keep the first call on the original line`), input:

```csharp
internal sealed class Example
{
    private static string Run(int[] items)
    {
        return items // element
            ?[0]
            .ToString();
    }
}
```

Before (`main`): the fix is offered and gives

```csharp
        return items // element
               ?[0].ToString();
```

After: no fix offered. The chain the fix formats holds the comment in front of `?[0]`. The same holds for every other fix that formats through the formatter, wherever the node it formats holds the shape.

No rule page changes: the rules lose their fix only for the refused shape, which `documentation/rules/general-notes.md` covers ("A diagnostic without a code fix").

## Decisions

The maintainer answered the run's output questions in batch 1: 1A, 2A, 3A, 4A, and 5A choose the visible output changes 1–5 as drafted — join `[` and `!` outside chains (1), judge a conditional as the line it becomes (2), judge a list as the line it becomes, also for `?[` and the `!` in a chain (3), keep one element per line in a collection expression (4), and let RH5302's fix results follow (5). Question 6 asked about a part kept on its own line by a comment; the maintainer answered to declare the shape unsupported: 6.1A, 6.2A, 6.3A (escalation 10). Changes 6 and 7 follow from that answer. In batch 2 the maintainer answered 1A, a comment in the gap with no line break is part of the shape, and 2A, the gap between a conditional element access's `?` and `[` is declared too, in this run.

### Declare the shape, in this change (maintainer, questions 6.1A, 6.2A, 6.3A)

A part kept on its own line by a comment, a directive, or disabled text had no column of its own outside a chain with links, and the issue proposed giving it the column of the expression's first token. The maintainer chose to declare the shape unsupported instead, in this run (6.1A). The shape covers the gap in front of the first token of an element access, a conditional element access, and a null-forgiving operator — the gaps FMT-BREAK-040 joins — and the gap in front of an argument list's `(` that FMT-BREAK-039 joins (6.2A); and it holds a comment, a directive, or disabled text (6.3A). Directives and disabled text belong to it because the join refusal and the chain contributor's column for a part kept below test them alike: with comments alone, both would stay (*Special cases*).

### The gaps of the part, on any line (maintainer, batch 2, 1A)

The shape is each gap as a place in the syntax tree, as for the operators of FMT-UNSUP-008 and FMT-UNSUP-011: a block comment there with no line break (`Foo /* c */ (1)`) is refused too. The maintainer chose this reading (1A) over refusing only a gap that holds a line break. This member deletes no special case of its own — every deleted guard acts only where a line break separates two tokens of the part or the part from the token in front of it. It makes FMT-SPACE-028's example of a comment between a collection expression's `with` and its argument list (`[with /* size */ (4)]`, formatted to `[with /* size */(4)]` on `main`) a refused input, so FMT-SPACE-028 drops that sentence and that example line and points to FMT-UNSUP-014, and change 6 shows the refusal. It also withholds the RH2001 and RH7101 fixes on `= Offset /* three */ (3)`, which change 7's rule already covers.

### The gap between `?` and `[` is declared too (maintainer, batch 2, 2A)

The gap between a conditional element access's `?` and `[` is part of the shape, in this change (2A). With it, the shape covers every gap between the tokens a part starts with, so no line break FMT-BREAK-040 removes is ever kept by a comment, a directive, or disabled text, and FMT-BREAK-040 no longer sends that gap to FMT-BREAK-001. It deletes the refusal FMT-BREAK-040's owner would need for that gap and the chain contributor's alignment of a part's later tokens (*Special cases*). It refuses `n? // c⏎[0]`, which `main` formats with `[0]` under the `?` (change 6).

### Which `!` and `?` belong to the shape is decided by the syntax tree

The `?` is that of a conditional access whose next token is the `[` of an element binding; the `!` is a null-forgiving operator not followed by `.` or the `?.` of a conditional member access. A `!` followed by `.` or `?.` starts a link operator (SHARED-CHAIN-001), whose gap the chain layout decides and where a comment in front of a wrapped link stays supported. These are the parts FMT-BREAK-040 joins, so the precheck needs no chain model, only the token's parent and the next token.

### FMT-UNSUP-004 and FMT-UNSUP-010 keep their reason

A comment in front of such a part in an attribute argument of a declaration is refused under FMT-UNSUP-004 already, and a directive in front of one in a field declaration with several declarators under FMT-UNSUP-010; both start at the same comment or directive, where FMT-UNSUP-001's first shape in the text does not decide. The reason stays the one these declarations report today, as for FMT-UNSUP-011. Inside an initializer value, FMT-UNSUP-004 excepts the comment, so the new reason applies there.

### Code fixes are withheld wherever their formatted node holds the shape

The fixes ask the formatter (FMT-UNSUP-007, SHARED-FIX-010), so the withheld cases follow from the node each fix formats, with no check of their own. RH2001-007 and RH7101-007 list the shapes that withhold their fix, so both name FMT-UNSUP-014.

### The join is stated by its cause, the gap in front of an attached part

The requirement names the gap in front of the first token of an element access, a conditional element access, and a null-forgiving operator — the attached parts of SHARED-CHAIN-001 that FMT-BREAK-039 does not own — whatever expression stands in front of it, rather than listing receivers (`arr`, `this`, `(arr)`, `d["k"]`). A `!` followed by `.` or `?.` is part of a link operator and stays the chain layout's (FMT-BREAK-019). The `[` joined is the one that opens the bracketed argument list of an element access or of the element binding behind a conditional element access's `?`; every other `[` is not an attached part and keeps today's output. That excludes the implicit element access of an object or dictionary initializer (`["a"] = 1`), whose `[` follows the initializer's `{` or a `,` rather than an element, and the `[` of `new[]`, `new int[3]`, and `stackalloc []`, which belongs to the creation expression itself; reading these as attached parts would join common initializer code onto its `{`.

### One requirement owns the gaps, and the join counts as FMT-BREAK-039's

FMT-BREAK-040 states the join for every such part, so FMT-BREAK-020 (in a chain) and FMT-BREAK-015 (inside a list element) no longer state or perform it. Its counting is FMT-BREAK-039's, referenced rather than restated, so an attached part split from its element lays out alike whichever part it is. This changes the output of joins `main` already makes after the list is laid out (change 3): `main` judged such a list as written only because of where its join ran, not by a decision.

### The null-forgiving operator in a chain becomes specified

`main` joins a null-forgiving operator in a chain with links (`x.ToString()⏎!` → `x.ToString()!`), which FMT-BREAK-020 did not state. FMT-BREAK-040 states it; that output does not change.

## Special cases

### Deleted

- `ArgumentListLineStartRewriter.CanJoin`: the `LineBreakTriviaUtilities.WouldJoinAcrossUnjoinableTrivia(previousToken, token) == false` term, and the remarks' sentence that a gap holding a comment, a directive, or disabled text is not joined. The term tests exactly the trailing trivia of the token in front of an argument list's `(` and the `(`'s leading trivia for comments, directives, and disabled text — FMT-UNSUP-014's place for the argument lists FMT-BREAK-039 names, which are the ones `IsArgumentListOpenParenthesis` admits. `CanJoin` joins only when the token in front lies inside the root, so every gap it tests lies between two tokens of the input, and a refused input never reaches it (FMT-UNSUP-003).
- `MethodChainAlignmentContributor.Contribute`: the column of an attached part's first token that starts a line — `LayoutComputer.SetIfFirstOnLine(attachedPart.FirstToken, column, "MethodChainAttachedPart", model)` with its choice between the root column and the anchor column — and the class summary's sentence about it. An attached part's first token starts a line only when the gap in front of it holds a line break the line-break phase did not join. FMT-BREAK-039 and FMT-BREAK-040 join every such gap of a chain inside the input unless a comment, a directive, or disabled text stands in it (on `main` the chain rewriter and the argument-list rewriter already did so in a chain with links, which is the only chain the loop runs for), and FMT-UNSUP-014 refuses every input that holds one there. With the declaration no supported input reaches the loop. `main`'s output for it is change 6's chain example.
- The refusal the join of FMT-BREAK-040 would need for the gap in front of the part's first token. On `main` that gap is joined by `ChainLineBreakRewriter.CloseGap` for an attached part's first token (in `NormalizeChain` and `JoinConditionalElementAccesses`) and by `ListElementInteriorJoinRewriter.CanJoin` inside a list element, each behind `WouldJoinAcrossUnjoinableTrivia`; decision 3 of the run moves these joins to one owner, and that owner carries no copy of FMT-BREAK-001's refusal for the gap in front of the part.
- The refusal the FMT-BREAK-040 owner would need for the gap between a conditional element access's `?` and `[`. On `main` that gap is joined by `ChainLineBreakRewriter.CloseInnerGaps` over the part's tokens (in `NormalizeChain` and `JoinConditionalElementAccesses`), which refuses through `CloseGap`'s `WouldJoinAcrossUnjoinableTrivia` term; the part's tokens are the `?` and the `[`, behind a `!` when one stands in front (`FluentChain.GetConditionalTokens`), and every gap between them is in the shape. Decision 3 moves the join to one owner, and that owner carries no refusal for it. `CloseGap` and `CloseInnerGaps` themselves stay for link operators (*Checked and kept*).
- `MethodChainAlignmentContributor.Contribute`: both `AlignRestOfRun(attachedPart.Tokens, model)` calls — the one for a chain without links and the one in the attached-part loop of a chain with links. The only attached part with more than one token is a conditional element access (`!`, `?`, `[`, or `?`, `[`), and a later token of it starts a line only when the gap in front of it holds a line break FMT-BREAK-040 did not remove, which only a comment, a directive, or disabled text in the gap can cause; FMT-UNSUP-014 refuses every input that holds one there. Every node-level input that holds the `?` holds the whole conditional access, so the `[` behind it as well, and no input holds only one side of that gap. `AlignRestOfRun` itself stays for link operators.
- Spec text: FMT-BREAK-040's sentence that the removal of a line break between `?` and `[` still yields to FMT-BREAK-001, and FMT-SPACE-028's sentence and example of a comment kept in front of the argument list of `with(…)`.
- Spec text: FMT-BREAK-039's example of a comment, a directive, or disabled text that keeps the parenthesis on its own line (now an example of the refused input), and this change doc's statement that a part kept below keeps today's column.

### Checked and kept

- `LineBreakTriviaUtilities.WouldJoinAcrossUnjoinableTrivia` and `SyntaxTriviaUtilities.WouldJoinAcrossUnjoinableTrivia`: many other callers — parameter lists, chains, assignments, accessors, lists, constraints, and analyzers.
- `ChainLineBreakRewriter.CloseGap`'s refusal: it also serves the rejoin of a split member name (FMT-BREAK-020), the inner gaps of link operators, and the first link's join (FMT-BREAK-019), which the shape does not cover.
- `ListElementInteriorJoinRewriter.CanJoin`'s refusal: it serves every interior wrap of a list element (FMT-BREAK-015), not only the gaps in front of attached parts, which it leaves to their owners.
- `ChainLineBreakRewriter.CloseInnerGaps` and `MethodChainAlignmentContributor.AlignRestOfRun` as methods: both serve a link operator's tokens (`!?.`, `?.`, `!.`), whose inner gaps the shape does not cover. Only their calls for an attached part's tokens are deleted.
- `LineBreakListRewriter.CollapseOwnedOpenParenToDeclarationLine` and `CollapseOpenParenToDeclarationLine`: they join a parameter list's `(`, which is not in the shape.
- `FluentChain` and `FluentChainLink` in `Reihitsu.Core`: their trivia checks decide whether a chain is wrapped and where its first link stands, for the analyzers (RH5112, RH5201, RH5048) as for the formatter, and analyzers do not consult the precheck.
- The indentation of a part kept below outside a chain with links: no contributor has a case for it — the line took the enclosing statement's column as an unowned continuation line — so there is nothing to delete.
- The code fixes: none has a guard of its own for this gap; the fixes that format through the formatter are withheld for the shape by SHARED-FIX-010 without a change.

## Alternatives considered

- **Keep the line break outside chains (today's output)** — not recommended: the same part is laid out differently depending on whether the expression has a link, and the part's line stands at a column that shows nothing about where it belongs.
- **Join, and keep judging a list as written where `main` joins after the list is laid out** (change 3's second, third, and fifth lines as on `main`) — not recommended: the list would then depend on which part was split and where, and FMT-BREAK-039 already judges a list as the line it becomes for an argument list in the same positions.
- **Let a collection expression that spanned several lines only because of the join become one line** (`[arr[0], 2]`) — not recommended: a collection expression would then lay out differently after this join than after a chain's join or FMT-BREAK-039's.
- **Give a part kept on its own line by a comment, a directive, or disabled text the column of the expression's first token**, as the issue proposes — rejected by the maintainer (question 6): it would add a column rule for a rare input, and the join refusal would stay; the shape is declared unsupported instead.
- **Keep the refusal (FMT-BREAK-001) and today's column for a part kept below** — rejected by the maintainer (question 6): outside a chain with links the part stays at the enclosing statement's column, and the branches of a conditional group around it get different columns.
- **Declare comments only** — not offered: directives and disabled text reach the same refusal and the same column, so the declaration would delete neither.
- **Leave the gap between a conditional element access's `?` and `[` out of the shape** — rejected by the maintainer (batch 2, 2A): the refusal for that gap and the chain contributor's alignment of a part's later tokens would stay, for a comment placed between the two characters of `?[`.
- **Refuse only a gap that holds a line break** — rejected by the maintainer (batch 2, 1A): `Foo /* c */ (1)` and `[with /* size */ (4)]` would stay formatted, and FMT-SPACE-028 unchanged; the shape would differ from FMT-UNSUP-008 and FMT-UNSUP-011, where the gap is a place in the syntax tree.
- **Report the shape with a new analyzer rule** — out of scope: the formatter may do more than the analyzers (P11); a rule for it is a candidate of its own.

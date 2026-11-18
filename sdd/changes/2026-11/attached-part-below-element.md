# An element access or a null-forgiving operator written below its element joins the element's line

| | |
|---|---|
| Status | Draft |
| Date | 2026-11 |
| Affected requirements | FMT-BREAK-040 (new), FMT-BREAK-020, FMT-BREAK-015, FMT-BREAK-031 |
| Depends on | [An argument list written below its name joins the name's line](argument-list-below-name.md) |

## What

The line-break phase joins an element access, a conditional element access, and a null-forgiving operator onto the line of the token in front of it when a line break separates the two (FMT-BREAK-040): `arr⏎[0]` becomes `arr[0]`, `n⏎?[0]` becomes `n?[0]`, and `Get()⏎!` becomes `Get()!`, wherever the part stands. A comment, a directive, or disabled text in the gap blocks the join (FMT-BREAK-001), and blank lines in the gap go with it (FMT-BREAK-030). The line break the join removes counts for a surrounding construct as the one in front of an argument list does (FMT-BREAK-039): a collection expression or another construct of FMT-BREAK-022 written over several lines only because of it keeps one element per line, and a list (FMT-BREAK-012) and a conditional expression (FMT-BREAK-018) are judged as the line they become.

FMT-BREAK-040 is the one requirement that owns these gaps, as FMT-BREAK-039 owns the gap in front of an argument list's `(`. FMT-BREAK-020, which joined an element access and a conditional element access in a chain with at least one link, now states only the rejoin of a split member name and points to FMT-BREAK-039 and FMT-BREAK-040 for attached parts. FMT-BREAK-015 names the new owner among the gaps it leaves alone. FMT-BREAK-031 lists the new join among the joins that write the minimal separator; it changes no output of its own.

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

A part kept on its own line by a comment, a directive, or disabled text in the gap is not joined (FMT-BREAK-001), and its column is today's.

A `[` that is not the first token of an attached part keeps today's output: the implicit element access of an initializer (`{⏎["a"] = 1,⏎["b"] = 2⏎}`) and the `[` of an array or `stackalloc` creation (`new⏎[]`, `new int⏎[3]`, `stackalloc⏎[]`) are not joined.

## Decisions

### The join is stated by its cause, the gap in front of an attached part

The requirement names the gap in front of the first token of an element access, a conditional element access, and a null-forgiving operator — the attached parts of SHARED-CHAIN-001 that FMT-BREAK-039 does not own — whatever expression stands in front of it, rather than listing receivers (`arr`, `this`, `(arr)`, `d["k"]`). A `!` followed by `.` or `?.` is part of a link operator and stays the chain layout's (FMT-BREAK-019). The `[` joined is the one that opens the bracketed argument list of an element access or of the element binding behind a conditional element access's `?`; every other `[` is not an attached part and keeps today's output. That excludes the implicit element access of an object or dictionary initializer (`["a"] = 1`), whose `[` follows the initializer's `{` or a `,` rather than an element, and the `[` of `new[]`, `new int[3]`, and `stackalloc []`, which belongs to the creation expression itself; reading these as attached parts would join common initializer code onto its `{`.

### One requirement owns the gaps, and the join counts as FMT-BREAK-039's

FMT-BREAK-040 states the join for every such part, so FMT-BREAK-020 (in a chain) and FMT-BREAK-015 (inside a list element) no longer state or perform it. Its counting is FMT-BREAK-039's, referenced rather than restated, so an attached part split from its element lays out alike whichever part it is. This changes the output of joins `main` already makes after the list is laid out (change 3): `main` judged such a list as written only because of where its join ran, not by a decision.

### The null-forgiving operator in a chain becomes specified

`main` joins a null-forgiving operator in a chain with links (`x.ToString()⏎!` → `x.ToString()!`), which FMT-BREAK-020 did not state. FMT-BREAK-040 states it; that output does not change.

## Alternatives considered

- **Keep the line break outside chains (today's output)** — not recommended: the same part is laid out differently depending on whether the expression has a link, and the part's line stands at a column that shows nothing about where it belongs.
- **Join, and keep judging a list as written where `main` joins after the list is laid out** (change 3's second, third, and fifth lines as on `main`) — not recommended: the list would then depend on which part was split and where, and FMT-BREAK-039 already judges a list as the line it becomes for an argument list in the same positions.
- **Let a collection expression that spanned several lines only because of the join become one line** (`[arr[0], 2]`) — not recommended: a collection expression would then lay out differently after this join than after a chain's join or FMT-BREAK-039's.
- **Give a part kept on its own line by a comment, a directive, or disabled text the column of the expression's first token**, as the issue proposes — not part of this change: it is an indentation change for a rare input, asked separately.
- **Report the shape with a new analyzer rule** — out of scope: the formatter may do more than the analyzers (P11); a rule for it is a candidate of its own.

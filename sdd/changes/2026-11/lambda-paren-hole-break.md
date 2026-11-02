# Parenthesized lambdas keep their line, and an anonymous method in a raw-string hole aligns like a block lambda

| | |
|---|---|
| Status | Accepted |
| Date | 2026-11 |
| Affected requirements | FMT-BREAK-011 (changed), FMT-INDENT-031 (added), FMT-RAW-007 (changed) |

## What

- **The opening parenthesis of a parenthesized lambda has no declaration-line rule** (FMT-BREAK-011). FMT-BREAK-011 keeps its comma and closing-parenthesis clauses for every parameter list, lambdas included. Its opening-parenthesis clause now covers every parameter list except a parenthesized lambda's. The gap in front of the lambda belongs to whichever requirement owns it: FMT-BREAK-015 inside a list element, FMT-BREAK-012 for the first element of a list, and FMT-BREAK-016 for an assignment's value. Where none of them joins the gap, the line-break phase leaves its line break and blank lines as written (FMT-BREAK-030). A line break that another requirement inserts in front of the lambda stays, as it does in front of any other expression. Until now the line-break phase stripped the leading line break, blank lines, and indentation of every lambda that started a line, and the indentation phase wrote the indentation back. In the process it removed the lambda's blank lines and undid the line breaks that FMT-BREAK-021 and FMT-BREAK-022 insert in front of an element.
- **An anonymous method in a raw-string hole aligns like a block lambda** (FMT-INDENT-031). In an interpolation hole of a multi-line interpolated raw string literal, an anonymous method is broken as it is anywhere else (FMT-BREAK-002, FMT-BREAK-006, FMT-BLANK). Its block then shifts so that the opening brace stands under its anchor: `async` for an async anonymous method, otherwise `delegate`, also behind `static`. This is the anchor a block lambda in the same position takes. The body stays one level deeper than the braces, and the whole block moves with the literal. The same holds when the anonymous method lies in a hole of a regular, verbatim, or single-line raw interpolated string that is itself nested in such a hole: it aligns like the block lambda in the same position. An anonymous method inside the block body of a lambda or of another anonymous method in the same hole keeps the scope-depth layout of that body. Outside holes of multi-line interpolated raw string literals nothing changes. FMT-RAW-007 counts FMT-INDENT-031 among the alignments that place a hole line.

## Why

**Parenthesized lambdas.** FMT-BREAK-011's opening-parenthesis clause listed parenthesized lambdas, but a lambda has no name in front of its parenthesis. The token in front of a lambda's `(` always lies outside the parameter list. Every other parameter-list owner joins that gap in its own visit afterwards; the lambda has no such visit, so for a lambda the collapse amounted to a bare strip of the parenthesis's leading trivia. Most of the time the indentation phase wrote the indentation back and the two phases undid each other on every pass. Where the trivia held a blank line, or a line break another rule had just inserted, or where the line had no layout in the indentation phase, the strip changed the output. It broke FMT-BREAK-030, FMT-BREAK-021, FMT-BREAK-022, and FMT-RAW-007. RH5105 already leaves a lambda's opening parenthesis unreported, and `ListElementInteriorJoinRewriter` already treats its gap as unowned by the list rules.

**Raw-string holes.** An anonymous method in a hole was broken onto lines that nothing placed. Outside a hole its block takes the scope-depth column, but FMT-RAW-007 forbids that column in a hole. The lines therefore kept the column the line-break phase left them in (column 0) and moved only with the literal. The same tokens ended in column 13 or in column 0, depending on how far the literal stood off in the input. A block lambda in the same place is aligned to the lambda (FMT-INDENT-028), so its lines are placed wherever the literal stands; the anonymous method now gets the same kind of alignment.

## Visible output changes

Each entry shows the formatter's output on `main` (`scripts/format.sh` at e094895) and the output after this change. Every "after" output in entries 1–4 and 7 is a fixed point of `main`'s formatter. The "before" output of entry 8 was taken at 3663caf, which leaves this case unaligned as `main` does. In the first case of entry 9 the "before" output is `main`'s first pass and the "after" output is `main`'s second pass; in its second case `main` is stable on the "before" output, and the "after" output is the one FMT-INDENT-028 and FMT-INDENT-031 demand.

### 1. An anonymous method in a hole of a literal the formatter moves is aligned under `delegate`

```csharp
class C
{
    int M(System.Func<int> f)
    {
        var a = $"""
    Hello {M(delegate { return 1; })}
    """;
    }
}
```

Before:

```csharp
class C
{
    int M(System.Func<int> f)
    {
        var a = $"""
                 Hello {M(delegate
             {
             return 1;
             })}
                 """;
    }
}
```

After:

```csharp
class C
{
    int M(System.Func<int> f)
    {
        var a = $"""
                 Hello {M(delegate
                          {
                              return 1;
                          })}
                 """;
    }
}
```

### 2. The same in an aligned literal

Input: the input of entry 1 with the literal already aligned (`Hello` in column 17). Before: the anonymous method's lines `{`, `return 1;`, `})}` in column 0. After: the output of entry 1.

### 3. Statements of such an anonymous method are separated as anywhere else, inside the aligned block

```csharp
class C
{
    int M(System.Func<int> f)
    {
        var a = $"""
                 Hello {M(delegate { int x = 1; return x; })}
                 """;
    }
}
```

Before:

```csharp
class C
{
    int M(System.Func<int> f)
    {
        var a = $"""
                 Hello {M(delegate
{
int x = 1;

return x;
})}
                 """;
    }
}
```

After:

```csharp
class C
{
    int M(System.Func<int> f)
    {
        var a = $"""
                 Hello {M(delegate
                          {
                              int x = 1;

                              return x;
                          })}
                 """;
    }
}
```

### 4. A block lambda inside such an anonymous method follows the aligned block

```csharp
class C
{
    int M(System.Func<int> f)
    {
        var a = $"""
                 Hello {M(delegate { return M(() => { return 1; }); })}
                 """;
    }
}
```

Before:

```csharp
class C
{
    int M(System.Func<int> f)
    {
        var a = $"""
                 Hello {M(delegate
{
return M(() =>
         {
             return 1;
         });
})}
                 """;
    }
}
```

After:

```csharp
class C
{
    int M(System.Func<int> f)
    {
        var a = $"""
                 Hello {M(delegate
                          {
                              return M(() =>
                                       {
                                           return 1;
                                       });
                          })}
                 """;
    }
}
```

### 5. The blank line in front of a parenthesized lambda that starts its line is kept

The gaps covered are: behind the `,` of an argument list, between array-initializer elements, behind an expression lambda's `=>`, behind `return`, and between collection-expression elements (entry 7 shows the last).

```csharp
using System;
using System.Linq;

class C
{
    Func<int, int> M(int[] items)
    {
        var total = items.Aggregate(0,

                                    (sum, item) => sum + item);
        var l = new Func<int, int>[]
                {
                    (x) => x,

                    (x) => x
                };
        Func<int, int> h = x =>

            (y) => y;

        return

            (x) => x;
    }
}
```

Before:

```csharp
using System;
using System.Linq;

class C
{
    Func<int, int> M(int[] items)
    {
        var total = items.Aggregate(0,
                                    (sum, item) => sum + item);
        var l = new Func<int, int>[]
                {
                    (x) => x,
                    (x) => x
                };
        Func<int, int> h = x =>
        (y) => y;

        return
        (x) => x;
    }
}
```

After: the before output with each of the four blank lines kept in place, columns unchanged. This is what `main` gives with `(1)` in place of each lambda. FMT-BREAK-030 already demands it.

### 6. A lambda that starts a hole line no alignment places keeps its column

```csharp
using System;

class C
{
    int M(Func<int, int> f, int v)
    {
        var a = $"""
                 A {M(v switch
                      {
                          _ =>
                              (x) => x
                      }, 1)}
                 """;

        return 0;
    }
}
```

Before:

```csharp
using System;

class C
{
    int M(Func<int, int> f, int v)
    {
        var a = $"""
                 A {M(v switch
                      {
                          _ =>
(x) => x
                      },
                      1)}
                 """;

        return 0;
    }
}
```

After:

```csharp
using System;

class C
{
    int M(Func<int, int> f, int v)
    {
        var a = $"""
                 A {M(v switch
                      {
                          _ =>
                              (x) => x
                      },
                      1)}
                 """;

        return 0;
    }
}
```

FMT-RAW-007 already demands this output: the literal is aligned, so the line stays where it is.

### 7. Line breaks that other rules insert in front of a lambda element are kept

This entry is new beyond the maintainer's answers. FMT-BREAK-021 and FMT-BREAK-022 already demand the "after" output, and `main` gives it with `(1)` in place of each lambda. Today the strip undoes the line breaks those rules insert in front of a lambda element.

```csharp
using System;
using System.Collections.Generic;

class C
{
    void M()
    {
        var list = new List<Func<int, int>> { (x) => x, (x) => x };
        Func<int, int>[] col = [(x) => x,
                                (x) => x];
        Func<int, int>[] two = [
            (x) => x, (x) => x];
        Func<int, int>[] gap = [(x) => x,

                                (x) => x];
    }
}
```

Before:

```csharp
using System;
using System.Collections.Generic;

class C
{
    void M()
    {
        var list = new List<Func<int, int>>
                   { (x) => x, (x) => x
                   };
        Func<int, int>[] col = [(x) => x,
                                   (x) => x
                               ];
        Func<int, int>[] two = [
                                   (x) => x, (x) => x
                               ];
        Func<int, int>[] gap = [(x) => x,
                                   (x) => x
                               ];
    }
}
```

After:

```csharp
using System;
using System.Collections.Generic;

class C
{
    void M()
    {
        var list = new List<Func<int, int>>
                   {
                       (x) => x,
                       (x) => x
                   };
        Func<int, int>[] col = [
                                   (x) => x,
                                   (x) => x
                               ];
        Func<int, int>[] two = [
                                   (x) => x,
                                   (x) => x
                               ];
        Func<int, int>[] gap = [
                                   (x) => x,

                                   (x) => x
                               ];
    }
}
```

### 8. The same for an anonymous method in a string nested in such a hole

An anonymous method in a hole of a regular, verbatim, or single-line raw interpolated string that lies in a hole of a multi-line interpolated raw string literal aligns under `delegate`, as the block lambda `() => { … }` in the same position aligns at its `(` (maintainer answer 1, the general clarification, and lead decision 10).

```csharp
class C
{
    int M(System.Func<int> f)
    {
        var a = $"""
    R {$"in {M(delegate { return 1; })}"}
    V {$@"in {M(delegate { return 1; })}"}
    S {$"""in {M(delegate { return 1; })}"""}
    """;

        return 0;
    }
}
```

Before:

```csharp
class C
{
    int M(System.Func<int> f)
    {
        var a = $"""
                 R {$"in {M(delegate
             {
             return 1;
             })}"}
                 V {$@"in {M(delegate
             {
             return 1;
             })}"}
                 S {$"""in {M(delegate
             {
             return 1;
             })}"""}
                 """;

        return 0;
    }
}
```

After:

```csharp
class C
{
    int M(System.Func<int> f)
    {
        var a = $"""
                 R {$"in {M(delegate
                            {
                                return 1;
                            })}"}
                 V {$@"in {M(delegate
                             {
                                 return 1;
                             })}"}
                 S {$"""in {M(delegate
                              {
                                  return 1;
                              })}"""}
                 """;

        return 0;
    }
}
```

With the literal already aligned, the lines `{`, `return 1;`, and `})}…` started in column 0 before; after, the output is the same as above.

### 9. An alignment whose anchor stands on a hole line that does not move lands under its anchor on the first pass

Two kinds of hole line do not move with the literal: a line that starts inside text begun on an earlier line, such as the continuation of a verbatim string's text, and the line on which a multi-line `/* */` or `/** */` comment ends (FMT-RAW-009). An anchor that stands on such a line keeps its column while the literal moves. Two kinds of anchor column on such a line change their output. FMT-INDENT-028, FMT-INDENT-030, and FMT-INDENT-031 already demand the new output: the column of the anchor is measured after the literal moved, and the block of a lambda or an anonymous method starts under its anchor wherever that is. The requirements do not change (lead decisions 12, 13, 15, and 17).

**An anchor left of the literal's rightward shift.** On `main`, a line aligned to such an anchor landed in the shift's column on the first pass, and only the second pass put it under the anchor (for a block whose anchor stands in the statement's column, see the second case). This holds for the block of a block lambda (FMT-INDENT-028), whose body was not one level deeper either, and in the same way for the other alignments that FMT-INDENT-030 measures at the anchor's final column, among them wrapped arguments (FMT-INDENT-012), creation initializers (FMT-INDENT-022), collection expressions (FMT-INDENT-025), switch expressions (FMT-INDENT-027), member-access chains (FMT-INDENT-029), and conditional operators (FMT-INDENT-020). The lines now land under the anchor on the first pass, and the "after" output is `main`'s second-pass output.

```csharp
class C
{
    int M(System.Func<int> f)
    {
        var a = $"""
    V {$@"first
continued{M(() => { return 1; })} end"}
    """;
        var b = $"""
    K {M(/* one
ends here */() => { return 1; })}
    """;

        return 0;
    }
}
```

Before (first pass; the second pass then moves the lines to the "after" columns):

```csharp
class C
{
    int M(System.Func<int> f)
    {
        var a = $"""
                 V {$@"first
continued{M(() =>
             {
             return 1;
             })} end"}
                 """;
        var b = $"""
                 K {M(/* one
ends here */() =>
             {
             return 1;
             })}
                 """;

        return 0;
    }
}
```

After:

```csharp
class C
{
    int M(System.Func<int> f)
    {
        var a = $"""
                 V {$@"first
continued{M(() =>
            {
                return 1;
            })} end"}
                 """;
        var b = $"""
                 K {M(/* one
ends here */() =>
            {
                return 1;
            })}
                 """;

        return 0;
    }
}
```

The same for a wrapped argument list and a member-access chain behind the end line of a comment:

```csharp
class C
{
    int M(int a, int b)
    {
        var s = "";
        var a = $"""
    A {M(/* one
ends */M(1,
2), 0)}
    C {M(/* one
ends */s.Trim()
.Trim()
.Length, 0)}
    """;

        return 0;
    }
}
```

Before (first pass; the second pass then moves `2),` to column 9 and the links to column 8):

```csharp
class C
{
    int M(int a, int b)
    {
        var s = "";
        var a = $"""
                 A {M(/* one
ends */M(1,
             2),
                      0)}
                 C {M(/* one
ends */s.Trim()
             .Trim()
             .Length,
                      0)}
                 """;

        return 0;
    }
}
```

After:

```csharp
class C
{
    int M(int a, int b)
    {
        var s = "";
        var a = $"""
                 A {M(/* one
ends */M(1,
         2),
                      0)}
                 C {M(/* one
ends */s.Trim()
        .Trim()
        .Length,
                      0)}
                 """;

        return 0;
    }
}
```

The lines aligned to `M(`'s own `(`, which stands on a line that moves, keep their output (`0)}`).

**An anchor in the column that block indentation gives the opening brace.** That column is the column of the statement around the literal: 8 in a method body, 12 one block deeper. When the anchor of a block lambda or of an anonymous method stands there, on `main` nothing placed the block's lines: they kept the column the line-break phase gave them and moved only with the literal. They stood in the shift's column when the literal moved right and in column 0 when it was aligned or moved left, the body not one level deeper, and a second pass left them there. They now stand under the anchor, with the body one level deeper, whether the literal moves or not. Unlike the first case, this "after" output is not `main`'s second pass; it is the output FMT-INDENT-028 and FMT-INDENT-031 demand at every other anchor column.

```csharp
class C
{
    int M(System.Func<int> f)
    {
        var a = $"""
    K {M(/* one
aaaaa */() => { return 1; })}
    """;
        var b = $"""
                 K {M(/* one
aaaaa */() => { return 1; })}
                 """;

        return 0;
    }
}
```

Before (first and second pass):

```csharp
class C
{
    int M(System.Func<int> f)
    {
        var a = $"""
                 K {M(/* one
aaaaa */() =>
             {
             return 1;
             })}
                 """;
        var b = $"""
                 K {M(/* one
aaaaa */() =>
{
return 1;
})}
                 """;

        return 0;
    }
}
```

After:

```csharp
class C
{
    int M(System.Func<int> f)
    {
        var a = $"""
                 K {M(/* one
aaaaa */() =>
        {
            return 1;
        })}
                 """;
        var b = $"""
                 K {M(/* one
aaaaa */() =>
        {
            return 1;
        })}
                 """;

        return 0;
    }
}
```

Only blocks reach this case. The other alignments of the first case are placed from an anchor in the statement's column as from any other column left of the shift: on `main` on the second pass, now on the first.

An anonymous method in the same positions (`continued{M(delegate { return 1; })} end"}`, `ends here */delegate { return 1; })}`, `aaaaa */delegate { return 1; })}`) aligns under `delegate` on the first pass in the same way, also with `delegate` in the statement's column. On `main` it is not aligned at all (entries 1 and 2, entry 8 for the nested verbatim string). An alignment keeps its output when its anchor stands on a line that moves, or when it stands on a line that does not move, in or right of the shift's column or in a literal that is already aligned or moves left, and in neither case in the statement's column.

### Unchanged

- A lambda argument on its own line without a blank line above it (`Aggregate(0,⏎(sum, item) => sum + item)`) keeps its output; only the strip-and-repaint between the two phases goes.
- A block lambda in a hole is broken and aligned as before (`BlockLambdaBrokenInsideHoleOfAlignedLiteralKeepsItsAlignment`), and so is an anonymous method inside its block body. The one exception is entry 9: an anchor on a hole line that does not move, in a column left of the literal's shift or in the statement's column, now places the block under the anchor on the first pass.
- An anonymous method outside interpolation holes, and in a hole of a regular, verbatim, or single-line raw interpolated string that no hole of a multi-line interpolated raw string literal encloses, takes the scope-depth column as before (`var c = $"""in {M(delegate { return 1; })}""";` keeps its block at the statement's scope depth).
- An anonymous method inside the block body of a lambda in a hole keeps the scope-depth layout of that body also when a regular, verbatim, or single-line raw interpolated string lies between the body and the anonymous method.

## Decisions

### Maintainer answers (2026-11-02)

1. **Anonymous method in a hole of a multi-line raw string literal.** It is broken and aligned, with its braces under the anonymous method's first token (refined by answers 4 and 6) as a block lambda in a hole is aligned, and its lines move with the literal; it is not kept on one line. No analyzer changes. The maintainer added a general principle: inside interpolation holes the same formatting applies as everywhere else, and a hole has no layout rules of its own. Entries 1–4 and 8 follow this answer.
2. **Blank line in front of a parenthesized lambda that starts its line.** It is kept, in every gap of entry 5, with columns unchanged.
3. **A lambda starting a hole line that no alignment places.** It keeps its column (entry 6).
4. **A `static` anonymous method in a hole.** Its block aligns under `delegate`, not under `static`, as a `static` block lambda aligns past `static`.
5. **Lambda elements of initializers and wrapped collection expressions (entry 7).** Accepted as written.
6. **An `async` anonymous method in a hole.** Its block aligns under `async`, as an `async` block lambda does (FMT-INDENT-028). The rule is that an anonymous method aligns the way its block-lambda counterpart in the same position does (see *The anchor follows the block-lambda counterpart*).

### The anchor follows the block-lambda counterpart

An anonymous method in such a hole aligns its block the way a block lambda in the same position does (FMT-INDENT-028). An `async` anonymous method aligns under `async`, as an `async` block lambda does. Every other anonymous method aligns under `delegate`, a `static` one included, as a `static` block lambda aligns past `static`, at its `(` (maintainer answers 4 and 6).

### FMT-INDENT-031 applies only where nothing else places the block

Inside the block body of a lambda or of another anonymous method in the same hole, the anonymous method's block already has a column: the scope depth relative to that body's brace, which moves with the body. Aligning it there as well would change output that is placed and stable today, and would make it differ from the same code outside a hole.

### FMT-BREAK-011 keeps the comma and closing-parenthesis clauses for lambdas

RH5106 and RH5107 report a lambda's misplaced closing parenthesis and comma. Both lie inside the parameter list, where the collapse can and does join. Only the opening clause, whose gap lies outside the list, is dropped for lambdas.

### The test expectation changes

Three tests in `RawStringInterpolationHoleTests` change their expected output. Each keeps its input and its subject.

- `AnonymousMethodBrokenInsideHoleIsStableOnTheSecondPass` checked only that the broken output was stable on the second pass. It changes to the aligned output of entry 1.
- `ConstraintClauseInHoleOfAlignedLiteralIsIndentedFromItsDeclaration` and `ConstraintClauseInHoleOfMisalignedLiteralIsIndentedFromItsDeclarationAfterTheShift` assert that a `where` clause of a local function in an anonymous-method body in a hole sits one level deeper than its declaration's line (FMT-INDENT-017). Their anonymous method's block already starts its own line in the input, in the literal's content column, and kept that column on `main`. Under FMT-INDENT-031 it moves under `delegate`, as in entries 1 and 2; the `where` clause still sits one level deeper than its declaration's line. Both tests now expect:

```csharp
        var a = $"""
                 Hello {M(delegate
                          {
                              void L<T>()
                                  where T : class
                              {
                              }

                              return 1;
                          })}
                 """;
```

On `main` the same lines started in column 17, under `Hello`. This is the layout of entries 1 and 2 applied to a block that is already broken, not an output change of its own.

## Alternatives considered

- **The collapse owns parenthesized lambdas fully and joins a lambda argument onto the previous line** (`items.Aggregate(0, (sum, item) => sum + item)`) — rejected: it conflicts with FMT-BREAK-012, which keeps a wrapped list at one element per line.
- **Keep an anonymous method in a hole on the lines the author wrote** (no phase inserts a line break into it) — rejected by the maintainer (answer 1). It would also leave the statement blank-line rules (RH5001–RH5019), RH5103, and RH5417 reporting formatted code.
- **Give lines inside a hole block indentation from the hole's first line** — rejected: a hole has no layout rules of its own, and the scope depth does not move with the literal (FMT-RAW-007).
- **Align anonymous methods like block lambdas everywhere, outside holes too** — not part of this change. It would change output outside holes, which the issue does not cover; it is a follow-up candidate.

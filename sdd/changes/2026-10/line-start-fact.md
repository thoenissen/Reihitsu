# One line-start fact for analyzers and every formatter phase

| | |
|---|---|
| Status | Accepted |
| Date | 2026-10 |
| Affected requirements | SHARED-BLANK-008 (new), FMT-INDENT-002, FMT-INDENT-015, FMT-DOC-003, FMT-SPACE-001, FMT-SPACE-015 (retired), RH5203-001, RH5204-001, RH6001-002, RH6002-001, RH6002-002, SHARED-CHAIN-002 |
| Depends on | [A preparation phase replaces tabs and normalizes line endings first](preparation-phase.md) |

Amendment for approval: the maintainer accepted this change doc on 2026-10-30. The passages marked *(amended)* below changed since then and were approved again on the same day. They make a wrapped type argument list of omitted type arguments join like any other list (FMT-BREAK-014) instead of staying wrapped with `>` in the block column; FMT-INDENT-015 keeps block indentation only for such a list that a comment or a directive keeps wrapped. Everything not marked is as accepted.

The passage marked *(added 2026-10-31)* (RH6001) is provisionally accepted under the maintainer's standing instruction for this run (`squad/decisions/2026-10-31-line-start-fact-audit-2-repeat.md`).

## What

Analyzers and every formatter phase decide with one rule whether a token starts its line, SHARED-BLANK-008, which extends to tokens what SHARED-BLANK-002 already decides for trivia:

- A token starts its line when nothing but whitespace stands between it and the last line break in front of it, whichever trivia carries that line break: an end-of-line trivia, or the line break that ends a `//` comment, a `///` documentation comment, or a directive. A line break inside the text of a multi-line comment does not count (SHARED-BLANK-002).
- A token without text — the omitted type argument of `Dictionary<,>`, an omitted array size, the end of the file — never starts a line. Trivia in front of it still starts its line as SHARED-BLANK-002 decides, so a comment or `#endregion` behind the last closing brace of a file is indented as today.

The user-visible consequence is that a line which starts behind a `///` comment written behind code is a line like any other. Today no phase places it, because Roslyn never puts a `///` comment into trailing trivia and its line break is part of the comment's structure, not an end-of-line trivia:

```csharp
class D
{
    int A; /// <summary>end</summary>
          }
```

```csharp
class D
{
    int A; /// <summary>end</summary>
}
```

FMT-INDENT-002 states that the first token of such a line is indented like any other; FMT-DOC-003's second example changes accordingly (`;` in column 8 moves to its declaration's column, 4). FMT-SPACE-015 is retired: the whitespace behind a `///` is part of a gap that holds a line break, which FMT-SPACE-001 already leaves to the indentation phase.

## Why

Whether a token starts its line is decided today by several predicates on three notions of a line: the `EndOfLineTrivia` kind, the text, and Roslyn's line table. They agree on ordinary code and disagree exactly where the output is wrong:

- A predicate that looks for `EndOfLineTrivia` does not see the line break of a `///` comment. The line behind a `///` written behind code is placed by no phase and keeps any column — the closing brace above stays in column 10 — and the horizontal-spacing phase needs FMT-SPACE-015 to keep the whitespace there, because nothing restores it once removed.
- The same blind spot lets the line-break phase move a stranded `///` comment in front of a chain link onto its own line, against FMT-DOC-003, and the next run then adds a blank line above it, against FMT-PIPE-004 (example under *Outputs that change*).
- *(amended)* A predicate that skips tokens without text misreads a list of omitted type arguments. The line-break phase does not join `typeof(Dictionary<,` / `>)`, although FMT-BREAK-014 joins every wrapped type argument list, because it finds no first element with text to join from. And when a comment keeps such a list wrapped, the omitted type argument and the `>` behind it both count as first on their line, so the indentation phase writes the line's column twice: in `typeof(Dictionary<, // c` / `>)`, `>` lands in column 16 instead of the block column 8 that FMT-INDENT-015 requires.

A rule that only one component states drifts from the copies that the others keep. One rule, shared by the analyzers that ask where a line starts and by every formatter phase, removes the disagreement and the compensating exception FMT-SPACE-015.

## Decisions

### The rule lives in the shared blank-line spec

SHARED-BLANK already specifies, for analyzer and formatter alike, what counts as a blank line and when trivia starts its line (SHARED-BLANK-002). The token rule is the same decision for the other kind of line element, so it is a new requirement there, SHARED-BLANK-008, and not a formatter requirement that analyzers would have to copy. Specs that decide something by whether a token starts or ends its line reference it: FMT-INDENT-002, FMT-SPACE-001, RH5203-001, RH5204-001, RH6002-001, RH6002-002, and SHARED-CHAIN-002.

### FMT-INDENT-002 places the line behind a `///` written behind code

FMT-INDENT-002's "first token of every line" is the token that starts its line by SHARED-BLANK-008, including a token behind a `///` comment written behind code on the line above. Every other requirement that places a line — alignments of lists, operators, and chains — then applies to such a line unchanged, so the line gets exactly the column it gets behind a `//` comment written in the same place.

The lines of the stranded comment itself are not affected: they are the comment's text, which FMT-DOC-003 keeps as written, and FMT-INDENT-008 indents only a `///` comment that starts its line. In

```csharp
class C
{
    int A; ///<remarks>a
        /// b</remarks>
}
```

the second `///` keeps column 8.

### FMT-DOC-003 keeps the comment, not the line behind it

FMT-DOC-003 still forbids moving a stranded comment and rewriting its text. Its second example changes: the `;` on the line behind `int A /// stray` was kept in column 8 only because no phase placed that line; it now moves to column 4, as it does behind `// stray` today. A new example states that a stranded comment in front of a chain link stays behind code while the links are aligned (FMT-INDENT-029).

### FMT-SPACE-015 is retired into FMT-SPACE-001

FMT-SPACE-001 leaves a gap unchanged when it holds a line break; the line break that ends a `///` comment now counts as one. That covers what FMT-SPACE-015 kept — the whitespace behind the comment — and the whitespace in front of the comment, which the phase already leaves unchanged today. The phase's output does not change, so the requirement is folded into FMT-SPACE-001 and FMT-SPACE-015 is retired with a pointer to it.

### Analyzer diagnostics follow the shared rule

RH5203, RH5204, RH6001, RH6002, and the fluent-chain rules RH5112, RH5201, and RH5048 ask whether a token starts or ends its line through the predicates this change unifies. With SHARED-BLANK-008:

- **RH5204** reports a namespace, type, block, accessor-list, or switch brace that starts its line behind a stranded `///` comment and is not in its scope column — the closing brace of `D` above. The formatter moves it to that column, so the diagnostic and the formatter agree (SHARED-DIAG-010).
- **RH5203** checks an argument whose first token starts its line behind a `///` comment and reports it outside the alignment column, as it does behind a `//` comment.
- *(added 2026-10-31)* **RH6001** stops reporting a keyword that a `///` comment ends the line behind, such as `if/// x` / `(c)`: the `(` starts its line, so a line break stands between the keyword and the `(` (RH6001-002), as behind `// x`. Today it reports the formatter's output for `if`, `while` (including that of a `do` statement), `for`, `foreach`, `fixed`, `lock`, `switch`, and `using`, and its fix writes `if /// x`.
- **RH6002** stops reporting the whitespace in front of a `///` comment that stands between a token and a comma, and the whitespace between a comma and a `///` comment behind it: a `///` comment ends its line, so the comma starts its line (RH6002-001) or ends it (RH6002-002). Today it reports both and its fix removes the space in front of the comment (`N(1 /// x` becomes `N(1/// x`), against RH6002-004, while the formatter keeps both gaps.
- **RH5201** and **RH5112** count a link behind a `///` comment as starting its line (SHARED-CHAIN-002). RH5201 reports such a link outside the chain's alignment column, and its fix aligns it without moving the comment, which is the formatter's output (SHARED-CHAIN-008); today the fix moves the comment onto its own line, as the formatter does. A first link behind a `///` comment keeps its line (SHARED-CHAIN-004), as behind any other comment.
- **RH5048** reports an ordinary comment in front of a chain's first link when that link starts its line behind a `///` comment, as it does behind a `//` comment (RH5048-001): in `items /* c */ /// x` / `.Where(…)`, `/* c */` is reported, and today nothing is.

For a token without text the same rule applies; none of the rules named above checks the position of an omitted type argument or array size, so none of their diagnostics changes for one.

### Outputs that change

Formatter output changes only for these inputs:

- A line that starts behind a `///` comment written behind code gets the column the same line gets behind a `//` comment — for example a closing brace, a `;`, a `)` or `]`, a leading `,`, a wrapped binary operator, `is`, `=>`, `=`, a chain link, a conditional-access link `?.`, the `?` or `:` of a conditional, or the `while` of a do-while statement — also when a directive or an own-line comment stands between the `///` comment and that line.
- A stranded `///` comment in front of a chain link, a conditional-access link `?.`, the `?` or `:` of a conditional, or the `while` of a do-while statement stays behind code (FMT-DOC-003), and the line behind it is placed as behind a `//` comment. For a chain:

  ```csharp
  return items.Where(x => x > 0) /// x
                   .Select(x => x)
                       .ToList();
  ```

  ```csharp
  return items.Where(x => x > 0) /// x
                      .Select(x => x)
                      .ToList();
  ```

  Today the line-break phase moves `/// x` onto a line of its own in front of each of these tokens, and a second run puts a blank line above it.
- *(amended)* A wrapped type argument list whose elements are omitted type arguments is joined onto one line like any other type argument list (FMT-BREAK-014): `typeof(Dictionary<,` / `>)` becomes `typeof(Dictionary<,>)`, and `typeof(Func<` / `,,>)` becomes `typeof(Func<,,>)`. Today both stay wrapped.
- *(amended)* A line whose first token with text follows a token without text on the same line is placed once, at the column its own requirement gives it: a type argument list of omitted type arguments that a comment or a directive keeps wrapped (`typeof(Dictionary<, // c` / `>)`, FMT-INDENT-015), and the same shapes with an omitted array size (`int[,` / `] a`), which no requirement joins, whose `]` or `,` keeps the block column as `int[2,` / `3] a` does today.

### Bugs fixed in the same change

*(amended)* These outputs change without a change of intended behavior, because the code violates an existing requirement:

- *(amended)* **FMT-BREAK-014** — a wrapped type argument list is joined also when its elements are omitted type arguments: `typeof(Dictionary<,` / `>)` becomes `typeof(Dictionary<,>)`.
- *(amended)* **FMT-INDENT-015** — a list whose first element is an omitted type argument and that stays wrapped keeps block indentation: `>` in `typeof(Dictionary<, // c` / `>)` goes to column 8, not 16.
- **FMT-INDENT-006** — a comment that starts its line takes the column of the code that follows it also when its text holds a line terminator other than `\n`, such as a lone `\r`, U+2028, U+2029, or U+0085; today such a comment keeps its column, while the same comment with `\n` inside moves. The same holds for an own-line comment in the same leading trivia as a multi-line comment or disabled text (FMT-INDENT-011) whose text holds such a terminator.

### What stays as it is

- SHARED-BLANK-002, the rule for trivia, is unchanged; SHARED-BLANK-008 applies the same notion of a line break to tokens.
- A `///` comment written behind code is still relocated or kept exactly as FMT-DOC-001 to FMT-DOC-003 decide. Only the line behind a kept comment is affected.
- The text of every comment, its interior lines included, is kept (FMT-PIPE-002, FMT-INDENT-007).
- Every input whose lines are all ended by end-of-line trivia, and whose lines start with a token with text, keeps its output and its diagnostics.

## Alternatives considered

- **Keep the author's column behind a `///` written behind code, with an exception in FMT-INDENT-002, and keep FMT-SPACE-015** — rejected: the exception would have to be repeated by every requirement that places a line and by every analyzer that asks where a line starts, which is the drift this change removes; and the line would stay the only one in a file that formatting leaves at any column.
- **Specify the line-start rule only for the formatter** — rejected: RH5204 and the formatter's indentation phase must agree on which lines they place (SHARED-DIAG-010), and SHARED-BLANK-002 already shares the trivia half of the rule between them.
- **Treat a token without text as starting its line and give it a column** — rejected: it has no width, so the column it would get is never visible, and the token with text behind it would be placed a second time from that column.

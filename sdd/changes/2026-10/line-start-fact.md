# One line-start fact for analyzers and every formatter phase

| | |
|---|---|
| Status | Accepted |
| Date | 2026-10 |
| Affected requirements | SHARED-BLANK-008 (new), FMT-INDENT-002, FMT-DOC-003, FMT-SPACE-001, FMT-SPACE-015 (retired), RH5203-001, RH5204-001, RH6002-001, RH6002-002, SHARED-CHAIN-002 |
| Depends on | [A preparation phase replaces tabs and normalizes line endings first](preparation-phase.md) |

## What

Analyzers and every formatter phase decide with one rule whether a token starts its line, SHARED-BLANK-008, which extends to tokens what SHARED-BLANK-002 already decides for trivia:

- A token starts its line when nothing but whitespace stands between it and the last line break in front of it, whichever trivia carries that line break: an end-of-line trivia, or the line break that ends a `//` comment, a `///` documentation comment, or a directive. A line break inside the text of a multi-line comment does not count (SHARED-BLANK-002).
- A token without text — the omitted type argument of `Dictionary<,>`, an omitted array size — never starts a line.

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
- A predicate that skips tokens without text treats both the omitted type argument of `Dictionary<,` and the `>` behind it as first on their line, so the indentation phase writes the line's column twice: `>` lands in column 16 instead of the block column 8 that FMT-INDENT-015 requires.

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

RH5203, RH5204, RH6002, and the fluent-chain rules RH5112, RH5201, and RH5048 ask whether a token starts or ends its line through the predicates this change unifies. With SHARED-BLANK-008:

- **RH5204** reports a namespace, type, block, accessor-list, or switch brace that starts its line behind a stranded `///` comment and is not in its scope column — the closing brace of `D` above. The formatter moves it to that column, so the diagnostic and the formatter agree (SHARED-DIAG-010).
- **RH5203** checks an argument whose first token starts its line behind a `///` comment and reports it outside the alignment column, as it does behind a `//` comment.
- **RH6002** stops reporting the whitespace in front of a `///` comment that stands between a token and a comma, and the whitespace between a comma and a `///` comment behind it: a `///` comment ends its line, so the comma starts its line (RH6002-001) or ends it (RH6002-002). Today it reports both and its fix removes the space in front of the comment (`N(1 /// x` becomes `N(1/// x`), against RH6002-004, while the formatter keeps both gaps.
- **RH5201** and **RH5112** count a link behind a `///` comment as starting its line (SHARED-CHAIN-002). RH5201 reports such a link outside the chain's alignment column, and its fix aligns it without moving the comment, which is the formatter's output (SHARED-CHAIN-008); today the fix moves the comment onto its own line, as the formatter does. A first link behind a `///` comment keeps its line (SHARED-CHAIN-004), as behind any other comment.

For a token without text the same rule applies; none of the rules named above checks the position of an omitted type argument or array size, so none of their diagnostics changes for one.

### Outputs that change

Formatter output changes only for these inputs:

- A line that starts behind a `///` comment written behind code gets the column the same line gets behind a `//` comment — for example a closing brace, a `;`, a `)` or `]`, a wrapped binary operator, or a chain link.
- A stranded `///` comment in front of a chain link stays behind code, and the chain is aligned:

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

  Today the line-break phase moves `/// x` onto a line of its own, and a second run puts a blank line above it.
- A line whose first token with text follows a token without text on the same line is placed once, at the column its own requirement gives it (the `Dictionary<,>` case of FMT-INDENT-015).

### Bugs fixed in the same change

Two outputs change without a change of intended behavior, because the code violates an existing requirement:

- **FMT-INDENT-015** — a list whose first element is an omitted type argument keeps block indentation; `>` in `typeof(Dictionary<,` / `>)` goes to column 8, not 16.
- **FMT-INDENT-006** — a comment that starts its line takes the column of the code that follows it also when its text holds a line terminator other than `\n`, such as a lone `\r` or U+2028; today such a comment keeps its column, while the same comment with `\n` inside moves.

### What stays as it is

- SHARED-BLANK-002, the rule for trivia, is unchanged; SHARED-BLANK-008 applies the same notion of a line break to tokens.
- A `///` comment written behind code is still relocated or kept exactly as FMT-DOC-001 to FMT-DOC-003 decide. Only the line behind a kept comment is affected.
- The text of every comment, its interior lines included, is kept (FMT-PIPE-002, FMT-INDENT-007).
- Every input whose lines are all ended by end-of-line trivia, and whose lines start with a token with text, keeps its output and its diagnostics.

## Alternatives considered

- **Keep the author's column behind a `///` written behind code, with an exception in FMT-INDENT-002, and keep FMT-SPACE-015** — rejected: the exception would have to be repeated by every requirement that places a line and by every analyzer that asks where a line starts, which is the drift this change removes; and the line would stay the only one in a file that formatting leaves at any column.
- **Specify the line-start rule only for the formatter** — rejected: RH5204 and the formatter's indentation phase must agree on which lines they place (SHARED-DIAG-010), and SHARED-BLANK-002 already shares the trivia half of the rule between them.
- **Treat a token without text as starting its line and give it a column** — rejected: it has no width, so the column it would get is never visible, and the token with text behind it would be placed a second time from that column.

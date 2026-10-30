Role: skeptic
Commit: b7177fc
Verdict: proceed

The prompt carried no intended fix, suspected cause, or hoped-for verdict. The issue's proposed code structure (`Pipeline/Core` facts, helper moves, the `ReihitsuFormatterHelpers` split, the one-contributor test) is implementation. I did not judge it, apart from its scope under *Simpler option*.

All probes ran on `b7177fc` (formatter and analyzers as on `main` 802afbf; the branch has changed only specs). Fixtures were kept in the session scratchpad, outside the repository. The commands were: `scripts/format.sh --force` twice (pass 1 output and pass 2 stability), `scripts/trace.sh --passes 2` (to attribute a change to a phase), and `scripts/apply-fix.sh` for RH5204, RH5203, RH6002, RH5201, and RH5112. For every apply-fix fixture, LF and CRLF gave the same result. RH5048 has no code fix, so `apply-fix.sh` cannot run it.

## Premise

The premise is real. All three of the issue's examples reproduce on today's code:

- **Closing brace behind a stranded `///`.** `class D { int A; /// <summary>end</summary>` with `}` on the next line in column 10. The formatter's output is unchanged and RH5204 reports nothing. With `// x` in place of the `///`, the formatter moves `}` to column 0.
- **Omitted type argument.** In `typeof(Dictionary<,` / `>)`, `>` lands in column 16; block indentation is column 8.
- **Lone `\r` inside a comment.** `/* a\r b */` in an LF file keeps column 12. The same comment with `\n` inside moves to column 8.

**Is the spec wrong instead?** No. The bug cases are FMT-INDENT-015 and FMT-INDENT-006 as they stand. The stranded-`///` case is the approved change doc. FMT-SPACE-015's `Why:` ("no later phase would restore the indentation") is only a compensation for the blind spot this change removes. Retiring it does not change the horizontal-spacing phase's output: `return 1    /// stray` / `;` is unchanged by today's whole formatter, and so is its `//` counterpart.

**FMT-SPACE-001's new sentence changes nothing today.** The sentence says a line break inside a multi-line comment is not a line break of the gap. Today the phase already treats such a gap as a same-line gap. `var x = a    /* x\n y */   + 1;` becomes `a /* x\n y */ + 1`, and `M(a   /* x\n y */   , 2)` becomes `M(a /* x\n y */, 2)`, so this states existing behavior.

**Spec examples checked against today's output for the `//` counterpart** (the change doc makes that output the target):

| Example | Expected | `//` counterpart today |
|---|---|---|
| FMT-INDENT-002 brace | column 0 | column 0 |
| FMT-DOC-003 `;` | column 4 | column 4 |
| FMT-DOC-003 chain | links in column 20 | links in column 20 |
| FMT-INDENT-002 continuation line `/// b</remarks>` | stays in column 8 | (no counterpart) the whole input is unchanged by today's formatter |
| SHARED-BLANK-008 `b */ }` | not placed, not reported | the formatter leaves it unchanged, RH5204 reports nothing |

**Spec defect to repair before tests: SHARED-BLANK-008 contradicts itself for a first token behind a comment.** The requirement's first sentence says a token starts its line "exactly when … nothing but whitespace stands between it and the last line break in front of it". Its last sentence says that without a preceding token, "a token with text starts its line when nothing but whitespace precedes it". The two disagree for the first token of a file, or of a detached formatting root, when a `///` comment or a directive stands in front of it. A type or member with a documentation comment at the start of a file, or a member formatted with `FormatNode`, is this case. Example input:

```csharp
/// <summary>x</summary>
    public class C
{
}
```

- Under the first sentence, `public` starts its line. That is today's behavior: the formatter moves it to column 0, and RH5204 reports it.
- Under the last sentence read on its own, `public` does not start its line. Neither surface would place it.

SHARED-BLANK-002's parallel sentence is a fallback for the case where no line break exists, and the last sentence is evidently meant the same way. It needs wording such as "Without a preceding token and without a line break in front of it, …". I route this to the spec author as a wording repair, not a question. One reading contradicts the requirement's own "exactly when", today's output, and the change doc's *What stays as it is*. If the spec author or the lead sees it differently, it becomes a maintainer question.

## Mechanism and candidates

Three mechanisms, one per defect class. All three are decisions about where a line starts or how many lines a trivia spans, made in more than one place.

**M1, the stranded `///`.** Predicates that decide whether a token starts its line look for `EndOfLineTrivia`. They do not see the line break that ends a `SingleLineDocumentationCommentTrivia`, because Roslyn files that comment in the next token's leading trivia with its terminator inside its structure. Those predicates are:

- `SyntaxTokenPositionUtilities.IsFirstOnLine`
- `SyntaxTriviaUtilities.AreSeparatedByEndOfLine`
- `IndentationRewriter`'s `startsAtLineStart`
- the line-break rewriters' own checks

`TokenGapAnalysis` scans structured trivia text and does see this line break, so trivia and blank-line decisions already agree with SHARED-BLANK-008 for it.

**M2, zero-width tokens.** `IsFirstOnLine` finds the previous token through `GetPreviousToken()`, which skips zero-width tokens. So both a zero-width token and the token with text behind it on the same line count as first on their line, and the indentation phase writes the line's column twice.

**M3, line terminators.** `IndentationRewriter` counts the lines of a trivia by the `\n` characters in its text (`CountEmbeddedNewLines`, `CountLineBreaks`). The layout model numbers lines by Roslyn's line table. Take a leading-trivia list that holds a trivia whose text has a line terminator other than `\n`. The trivia in that list, up to and including the one holding the terminator, are looked up on the wrong line and keep their column.

Candidate dispatch:

- **M1:** the trivia kind `SyntaxKind.SingleLineDocumentationCommentTrivia` in a token's leading trivia when no `EndOfLineTrivia` follows it. The token after it is any token that does not open the node the comment would document. FMT-DOC-003 keeps such a comment, and the documentation-comment phase leaves it in place.
- **M2:** zero-width tokens that are not missing, which means a valid tree:
  - `OmittedTypeArgumentToken` (inside `OmittedTypeArgumentSyntax`)
  - `OmittedArraySizeExpressionToken` (inside `OmittedArraySizeExpressionSyntax`)
  - `EndOfFileToken`
- **M3:** trivia whose text can hold a raw terminator and is counted by `CountEmbeddedNewLines`:
  - `MultiLineCommentTrivia`
  - `MultiLineDocumentationCommentTrivia`
  - `DisabledTextTrivia`
  - `SingleLineDocumentationCommentTrivia`

## Breaking inputs

Failure classes that reach this change:

- **FC-015 and FC-016:** the policy has many owners, and the issue's list of them is incomplete; see the M1 matrix.
- **FC-012 and FC-010:** phases undo each other.
- **FC-013:** analyzer and formatter parity.
- **FC-007:** a code fix needs more than one pass.
- **FC-019, FC-020, FC-021:** comments and directives in the gap.
- **FC-028:** detached-node and document-scoped formatting disagree.
- **FC-017 and FC-033:** LF, CRLF, and lone-CR evidence.
- **FC-001 and FC-002:** for `EndOfFileToken`; see M2.

### M1: a line behind a stranded `///`

The target for every row is the output of the `//` counterpart.

**Line kind (the token behind the comment).**

These lines keep the author's column today. Every one of them changes to the `//` counterpart's column:

- `}` of a type, block, accessor list, namespace, or switch statement
- `}` of an object initializer or a switch expression
- `;` of a field or a `return`
- `)`, `]`, `>` of a type argument list, and a leading `,`
- the binary operators `+`, `&&`, and `??`
- `is`, the lambda `=>`, the assignment `=`, and a single `?.`

The line-break phase moves these comments today. The comment goes onto its own line in pass 1 (against FMT-DOC-003), and pass 2 adds a blank line above it (against FMT-PIPE-004). `trace.sh` attributes the move to `LineBreakPhase` for every row:

| Token behind the comment | Note |
|---|---|
| later chain link `.` | named in the change doc |
| conditional-access chain link `?.` | `items.FirstOrDefault()?.ToString() /// x` / `?.Trim()` / `?.Length` |
| conditional `?` | `return a /// x` / `? 1` / `: 2` |
| conditional `:` | `return a ? 1 /// x` / `: 2` |
| `while` of a do-while | `} /// x` / `while (a);` |

The change doc names only the chain row under *Outputs that change*. Its first bullet ("gets the column the same line gets behind a `//` comment") covers the other four, because their `//` counterparts keep the comment behind the code. They need tests.

**Must stay.** For these tokens the documentation-comment phase relocates the comment today, in pass 1, and the output is stable. This change must not alter that:

- `else`, `catch`, or `select`
- an argument behind a `,`
- a statement such as `if` or `return`
- a switch label
- a block `{`
- a member declaration

**Context axes.**

- Nothing else in the gap.
- A blank line after the comment. Today the formatter removes the blank line and leaves `}` in column 10.
- An own-line `//` comment after the comment.
- A directive after the comment (`#if DEBUG … #endif` before `;`). Today `;` keeps column 12, while the `//` counterpart puts it in column 8.
- A multi-line stranded comment (`///<remarks>a` / `/// b</remarks>`). Its continuation line stays in column 8, and the line behind it is placed.
- Whitespace in front of the comment (`return 1    /// stray`). It stays (FMT-SPACE-001).
- LF and CRLF.
- The detached `FormatNode` path versus `FormatNodeInDocumentAsync` (FC-028), as reached through the RH5204 and RH5201 fixes.

**Analyzer members.** Formatter output for every `//` counterpart is clean today under RH5204, RH5203, RH6002, RH5201, and RH5112. That is the parity proxy for the `///` targets.

- **RH5204, closing brace.** Newly reports `}` behind `///`.
- **RH5204, statements.** Newly reports statements behind a `///` that the formatter relocates: `var x = 1; /// stray` / `            return;` (no diagnostic today), and `if` in the lead's follow-up input. The fix must converge (FC-007). Today the follow-up input and `if (a) /// x` / `{` already end in `no-progress` (the fix relocates the comment and leaves the column). The `//` counterpart `var x = 1; // stray` / `            return;` is fixed in 1 iteration. Neither row runs today with `return` reported.
- **RH5204, blank line.** With a blank line between `int A; /// x` and `}`, the fix today reports "fixed" in 1 iteration. All it does is delete the blank line and leave `}` in column 10. The diagnostic goes away only because the analyzer can no longer see the line start. After the change the analyzer still sees it, so this row tests whether the fix places the brace.
- **RH5203.** Newly reports `N(1, /// x` / `              2);`. The formatter relocates that comment (documentation-comment phase) and puts `2` in column 10. Check whether the fix converges and whether it keeps the comment.
- **RH6002.** `N(1 /// x` / `, 2)` and `N(1,   /// x` / `2)` are no longer reported. Today both are reported and fixed.
- **RH5201.** The later `.` link, the `?.` chain, and the first-link chain are reported today. The fix must align the links and keep the comment where it is.
- **RH5112.** A first link behind a `///` keeps its line (SHARED-CHAIN-004).
- **RH5048.** Its report condition is `chain.FirstLink.StartsLine`. So an ordinary comment in front of a stranded `///` before the first link (`items /* c */ /// x` / `.Where(…)`) is newly reported, just as with `// x` today. The `///` itself stays excluded by `IsOrdinaryComment`. The change doc names RH5048 among the rules that are affected but states no consequence for it. This member needs an analyzer test. I could not run it here.

### M2: zero-width tokens

Today every cell is stable across passes but wrong. The expected column is the block column 8.

The control inputs, where every element has text, hold. `new int[2,` / `3]` and `new int[2` / `]` both put the wrapped line in column 8.

Axes:

- **Zero-width kind:** omitted type argument; omitted array size.
- **Token that starts the line:** closing `>` or `]`; separator `,`.
- **Arity:** one, two, or three omitted elements.
- **Container:** `typeof(…<…>)`; rank specifier of a declared type; rank of an array creation; nested rank `int[][,`.
- **Gap:** nothing; trailing `// c` on the line above; own-line comment.

| Input (line break at `/`) | Today | Expected |
|---|---|---|
| `Dictionary<,` / `>` | 16 | 8 |
| `Dictionary<` / `,>` | 16 | 8 |
| `Action<` / `>` | 16 | 8 |
| `Func<,,` / `>` | 16 | 8 |
| `Func<,` / `,>` | 16 | 8 |
| `>` indented to 20 by the author | 16 | 8 |
| `int[,` / `] a` | 16 | 8 |
| `int[` / `,] a` | 16 | 8 |
| `new int[,` / `] { { 1 } }` | 16 | 8 |
| `int[][,` / `] a` | 16 | 8 |
| `Dictionary<, // c` / `>` | 16 | 8 |
| `Dictionary<,` / own-line `// c` / `>` | `>` in 8, but see below | 8, no whitespace-only line |
| `Dictionary<` / own-line `// c` / `,>` | same as above | same as above |

The two rows with an own-line comment fail differently. Pass 1 leaves a whitespace-only line of 8 spaces where the blank-line phase inserted a blank line, against FMT-CLEAN-001. In pass 2, the indentation phase writes 16 spaces there and the cleanup phase writes 8, so two phases undo each other (FC-012).

The omitted-array-size rows are a change in output that the change doc covers only through its general bullet and SHARED-BLANK-008's "omitted array size".

**`EndOfFileToken` must stay.** It is zero-width, and today the rewriter re-indents trivia in its leading trivia only because `IsFirstOnLine(EOF)` is `true`. That covers a misindented own-line comment or `#endregion` after the last `}`. Today all of these are placed: comment to column 0, `#endregion` to column 0, and RH5204 reports and fixes `#endregion`. SHARED-BLANK-002 keeps those trivia line-starting even though the token "never starts a line". This input must be tested (FC-001 and FC-002).

### M3: line terminators in trivia text

Axes:

- **Terminator inside the trivia:** lone `\r`, U+2028, U+2029, U+0085, `\r\n`, `\n`.
- **Trivia kind:** `/* */`, `/** */`, disabled text, multi-line `///` in a lone-CR file.
- **Position:** the trivia itself; an own-line trivia before it in the same leading trivia; an own-line trivia after it.
- **File line ending:** LF, CRLF, lone CR.

Failing cells (column 12 is kept where 8 is required, FMT-INDENT-006):

- **`/* */` itself:** a comment holding `\r`, U+2028, U+2029, or U+0085, in an LF file or a CRLF file. In a lone-CR file, a comment whose interior is `\r` (the file's own terminator) fails too. In a CR file, every multi-line comment that spans lines is therefore a member.
- **An own-line `// first` before `/* a\r b */` or `/* a U+2028 b */`:** `// first` happens to get column 8 because the line it is looked up on has a layout, but the multi-line comment keeps column 12.
- **An own-line `// first` before a disabled `#if NEVER` block whose text holds `\r` or U+2028:** `// first` keeps column 12.
- **An own-line `// first` before a closing brace, with the multi-line comment behind it:** the multi-line comment keeps column 4 where 8 is required.

Holding cells:

- interiors with `\n` or `\r\n`
- trivia after the one holding the terminator (for example `// after` behind disabled text)
- `/** */`: B1 normalizes its interior newline tokens to the file's line ending. `/** a\r b */` came out as `/** a\n b */`, so this kind leaves the class.
- a multi-line `///` in a lone-CR file
- verbatim strings holding `\r` or U+2028, because tokens are numbered by the line table

The issue names a whole lone-`\r` file as a case. The disabled-text members are missing from its list.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `Reihitsu.Core` `SyntaxTokenPositionUtilities.IsFirstOnLine` | Must change (M1, M2) |
| `Reihitsu.Core` `SyntaxTriviaUtilities.AreSeparatedByEndOfLine` | Must change: used by RH6002 and the FMT-SPACE-001 gap guard |
| `Reihitsu.Core` `TokenGapAnalysis` | Must stay for trivia (SHARED-BLANK-001, SHARED-BLANK-002). It already sees the `///` line break. Its terminator set is `\r` and `\n` only |
| `Reihitsu.Core` `FluentChainLink.StartsLine`, `FluentChain` | Must change. This is what moves RH5201, RH5112, RH5048, and the chain code fix |
| `IndentationPhase` (`LayoutComputer`, `IndentationRewriter`, `ListElementAligner`, `MethodChainAlignmentContributor`, `ObjectInitializerContributor`) | Must change (M1, M2, M3). `EndOfFileToken` trivia handling must stay |
| `LineBreakPhase` | Must change: the rows in the M1 table that are moved today (chain `.`, `?.`, conditional `?` and `:`, do-while `while`). Every other line-break decision must stay |
| `BlankLinePhase` | Must stay. Its pass-2 blank line disappears only because the line-break phase stops moving the comment |
| `HorizontalSpacingPhase` | Output must stay. FMT-SPACE-015's exception is retired into FMT-SPACE-001 |
| `DocumentationCommentFormattingPhase` | Must stay: the relocations in the M1 *Must stay* list |
| `CleanupPhase` | Not involved. The whitespace-only line in M2 comes from the indentation phase |
| RH5204 analyzer / fix | Analyzer must change. The fix is newly reached by statements and braces behind a `///`, and must converge (FC-007) |
| RH5203 analyzer / fix | Analyzer must change. The fix is newly reached |
| RH6002 analyzer / fix | Analyzer must change. The fix is no longer offered for the two shapes |
| RH5201, RH5112 analyzers / `FluentChainCodeFixHelper` | Must change through `StartsLine`. The fix must keep the comment where it is |
| RH5048 analyzer | Changes through `FirstLink.StartsLine` (see the M1 analyzer members). No fix exists |
| Fix All | Not involved beyond the providers above. The Fix All scenarios for RH5204, RH5201, and RH6002 must still correct two diagnostics |
| `documentation/rules/` | Not involved. Agreed with the spec author: these are corner cases that `general-notes.md` covers |
| CLI | Not involved |
| Tests | Expectations of `StrandedDocumentationTokenSpacingTests` and `WrappedAngleBracketListTests.UnboundGenericWithZeroWidthAnchorIsNotAlignedToADegenerateColumn` change. The FMT-SPACE-015 tags move |

## Simpler option

There is none for the behavior: the maintainer approved it.

**Scope.** The issue's restructuring items carry no behavior and are not in the change doc:

- the helper moves out of phase folders
- the `ConfigurableStructuralTransforms` move
- the `ReihitsuFormatterHelpers` split
- the one-contributor-per-line test

`ReihitsuFormatterHelpers` is `internal`. Code fixes reach it through `InternalsVisibleTo`, so splitting it is no public-API change. These items still multiply the production paths. The issue's *Done when* makes them binding, so the lead should count them in the run's intended paths now, so that escalation 5 is measured against the right baseline.

## Questions for the maintainer

_None._ The SHARED-BLANK-008 wording under *Premise* is a repair for the spec author. It becomes a question only if the spec author reads the last sentence as intended.

## Architecture findings

**One decision, many owners.** "Where does a line start, and how many lines does a trivia span" is decided by several components. They disagree on exactly the inputs this run must fix:

| Component | How it decides | Disagreement |
|---|---|---|
| `SyntaxTokenPositionUtilities.IsFirstOnLine` | `EndOfLineTrivia` kind, previous token found by `GetPreviousToken()` | Blind to the `///` line break; skips zero-width tokens (M1, M2) |
| `SyntaxTriviaUtilities.AreSeparatedByEndOfLine` | `EndOfLineTrivia` kind | Blind to the `///` line break (M1) |
| `IndentationRewriter` | `startsAtLineStart`, plus `\n` counting | `\n` counting disagrees with `LayoutComputer`'s line table (M3) |
| `TokenGapAnalysis` | Text scan for `\r` and `\n` | Sees the `///` line break, so it disagrees with the two predicates above |
| `LineBreakPhase` rewriters for chains, conditional-access chains, conditionals, and do-while | Their own checks | Move a stranded `///` that the documentation-comment phase keeps (FMT-DOC-003) |

The inputs that show the disagreements are under *Breaking inputs*.

This is the run's own defect. The issue's *Proposed Behavior*, and the change doc the maintainer approved under escalation 2, already are the structural change. Two parts of what my enumeration found are not in the issue's list:

- the line-break rewriters for conditionals, conditional-access chains, and do-while
- disabled text as an M3 carrier

The lead decides how escalation 12 is put to the maintainer, given that the approved scope is "change the structure in this run".

## Follow-up candidates

- **RH5204 fix does not converge at the start of a file.** The input is a first token behind a `///` that starts its own line, or behind a directive (`/// <summary>x</summary>` / `    public class C`, and `#nullable enable` / `    public class C`). The result is `no-progress (2 iteration(s))` under LF and CRLF, while the formatter puts `public` in column 0. This input has no `///` behind code, so the run does not reach it.
- **RH5204 does not report a misindented field whose first token is a predefined-type keyword.** For `int A;` / `        int B;` it reports nothing; `public int B;` is reported. RH5204-001 ("a keyword or a modifier that starts … a member") reads as covering it. Spec or analyzer question, outside this run.
- **The documentation-comment phase relocates comments that FMT-DOC-001 leaves undecided.** It moves a `///` written behind code in front of `else`, `catch`, `select`, an argument, a statement, a switch label, or a block `{`, and adds a blank line above it. FMT-DOC-001 states it "does not yet decide comments before a statement or inside an expression", so the formatter decides something no spec states. This is a spec gap.
- **RH5048 and `///`.** The spec author found it: `IsOrdinaryComment` excludes `///`, while SHARED-COMMENT-001 counts `///` as an ordinary comment. Still unverified; RH5048 has no fix for `apply-fix.sh`.

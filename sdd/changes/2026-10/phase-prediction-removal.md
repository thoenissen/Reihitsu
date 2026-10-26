# No formatter phase predicts a later phase's columns

| | |
|---|---|
| Status | Accepted |
| Date | 2026-10 |
| Affected requirements | FMT-PIPE-001, FMT-PIPE-015, FMT-PIPE-016, FMT-PIPE-017, FMT-PIPE-018, FMT-CLEAN-002, FMT-CLEAN-003, FMT-CLEAN-004, FMT-RAW-005, FMT-RAW-007, FMT-INDENT-006, FMT-INDENT-011, FMT-INDENT-023, FMT-INDENT-030, SHARED-INIT-002, SHARED-FIX-007, RH5303-003, RH5303-005, RH5304-002, RH5304-004, RH5303-006 and RH5304-005 (added and retired by this change), the spec `formatter/tab-replacement.md` (FMT-TAB-001 to FMT-TAB-003) |

## What

Two formatter decisions move between phases, so that no phase computes a column from what a later phase will do to it, and the indentation phase computes a token's column once:

- **Tabs in whitespace are replaced first.** A new first phase, `TabReplacementPhase`, replaces every tab in whitespace trivia and between the tokens of a directive with four spaces, before the structural transforms. `CleanupPhase` no longer replaces tabs. The rules themselves are unchanged and move from FMT-CLEAN-002, FMT-CLEAN-003, and the tab half of FMT-CLEAN-004 to FMT-TAB-001 to FMT-TAB-003.
- **The indentation phase moves the lines of raw string literals.** `RawStringAlignmentPhase` is no longer a phase of its own. The indentation phase places the opening quotes of a multi-line raw string literal and then moves the literal's content lines, its hole lines, and its closing marker as FMT-RAW specifies, in the same phase that measures every anchor. It moves them by the distance its own layout computed for the anchors, so the move is decided once. FMT-RAW keeps its requirements; FMT-RAW-005 and FMT-RAW-007 name the indentation phase where they named a separate phase or the cleanup phase.
- **The indentation phase measures a token behind a comment that leads its line where the token ends up.** On a line where a comment stands in front of the first token, the layout takes the line's column as the column of that token, while the phase writes the comment in that column and the token behind it. Every column derived from such a token — an alignment anchor, the opening quotes of a raw string literal — is therefore short by the width of the comment and the space after it. The layout now counts what stands in front of a token on its line, so the column it computes is the column the phase writes (FMT-INDENT-030, FMT-RAW-005). This holds for every anchor, also one that is itself the first code on its line, such as the opening brace of a complex element initializer behind `/* Keep element. */`: the comment takes the column the line starts in, and the lines aligned to the brace follow the brace where it stands. This is fixed first, because the single move above is correct only where the layout's column is the written one.
- **RH5303 and RH5304 measure a wrapped complex element from its brace.** For a complex element initializer (`{ 1, 2 }` inside a collection initializer) whose opening brace stands behind a comment that leads its line, the rules measure the closing brace and the expressions from the column the brace stands in, not from the comment's column (SHARED-INIT-002). They then accept the formatter's output for such an element, and report the layout they accepted before, whose expressions start one level deeper than the comment. Their code fixes lay such an element out as the formatter does (RH5303-005, RH5304-004).
- **The node-level entry points take every column from the indentation phase.** `FormatNodeInDocumentAsync` and `FormatNodeInDocumentWithContextAsync` lay out the node they format with its first token in its column in the document and every other line placed by the indentation phase, and change no line's column after the last phase (FMT-PIPE-017). The blank lines in front of a target of `FormatNodeInDocumentWithContextAsync` stay the document's, as they are today (FMT-PIPE-018). Today they format the node as if it started at another column and then move the start of each line by the difference: a line that starts inside a comment and the lines of a raw string literal are not moved, and a tab in the node's own leading whitespace counts as one column. A tab in front of a node that does not start its line stays in the code in front of the node and keeps counting as one column, as the rules count it, so a fix's output stays clean for its own rule (SHARED-FIX-001, SHARED-FIX-007). Every code fix that formats through these entry points then lays out its code in the columns the formatter gives it.
- **A comment line above a comment-led line takes that line's column, also inside an interpolation hole.** On ordinary lines a comment on its own line above a line that another comment leads already takes the column of that leading comment (FMT-INDENT-006). Inside an interpolation hole it took the column of the token behind the leading comment; it now takes the column of the leading comment there too (FMT-RAW-007).

The pipeline keeps twelve phases. Three new pipeline requirements state what this guarantees: no phase after the first sees or writes a tab in whitespace (FMT-PIPE-016), no phase after the indentation phase moves a token, a comment, or a directive within its line (FMT-PIPE-015), and the node-level entry points change no line's column after the last phase (FMT-PIPE-017).

Every example of FMT-CLEAN, FMT-RAW, and FMT-INDENT that exists today keeps its output. Output changes in two kinds of input only, and each change brings the output in line with a requirement that already exists:

- Output that today is not idempotent because the tab width was counted differently by two phases. It now reaches its final form in one pass, as FMT-PIPE-004 requires.
- Output that is stable today but wrong because a comment leads the line of an anchor or of a raw string's opening quotes. It now meets FMT-INDENT-030 and the requirement of the alignment that anchor decides, named in each row below.

The inputs below sit in `class C { void M() { ... } }`. `/` separates lines, `→` stands for a tab, and columns are 0-based:

| Input | Today | After this change | Requirement |
|---|---|---|---|
| `        /* c */ Call(1,` / `2);` | `2` in column 13, while `1` is in 21 | `2` in column 21 | FMT-INDENT-012 |
| `        /** d */ Call(1,` / `2);` | `2` in column 13, while `1` is in 22 | `2` in column 22 | FMT-INDENT-012 |
| `        /* c */ a.B()` / `.C();` | `.C()` in column 9, while `.B` is in 17 | `.C()` in column 17 | FMT-INDENT-029 |
| `        /* c */ var s = """` / `    a` / `    """.Trim()` / `    .ToUpper();` | the literal under its opening quotes in column 24; `.ToUpper` in column 19 | `.ToUpper` in column 27, under `.Trim` | FMT-INDENT-030 |
| the same with `$"""` and a content line `    a{x}` | the literal in column 25; `.ToUpper` in column 20 | `.ToUpper` in column 28 | FMT-INDENT-030 |
| `        var s = Call(1,` / `            /* c */ """` / `    a` / `    """.Trim()` / `    .ToUpper());` | the literal in column 29; `.ToUpper` in column 24 | `.ToUpper` in column 32 | FMT-INDENT-030 |
| `        /* a` / `        b */ Call(1,` / `2);` | `2` in column 13, while `1` is in 18 | `2` in column 18 | FMT-INDENT-012, FMT-INDENT-030 |
| `        var x = a→/** c */.B()` / `.C();` | `.C()` in column 26 after one pass, 29 after the second | `.C()` in column 29 in one pass | FMT-PIPE-004, FMT-TAB-001 |
| `        var s = x→/** d */.Foo(` / `"""` / `    a` / `    """.Trim()` / `.ToUpper());` | `.ToUpper` in column 34 after one pass, 37 after the second | `.ToUpper` in column 37 in one pass | FMT-PIPE-004, FMT-TAB-001 |

In the sixth row the blank line FMT-BLANK-011 puts above the comment is unchanged. The seventh row is a line led by the last line of a multi-line comment; thoenissen/Reihitsu#1382 decides that the formatter breaks before such a token, which will later change that output again. Where a literal itself ends up does not change: `    /* c */ var s = """` / `    a` / `    """;` puts the literal under its opening quotes in column 24, today and after this change; and on the first line of a text that keeps its byte order mark, `var s = """` / `    a` / `    """;` puts the literal in column 9, under the quotes counted with the byte order mark, today and after this change (FMT-RAW-005).

An anchor that is the first code on a line that a comment leads changes the same way. In the rows below `/* c */` stands in column 8 and the anchor behind it in column 16, except where a column is given; in the first two rows the comment stands at the element's or the member's column, 20, and the brace behind it in column 28:

| Input | Today | After this change | Requirement |
|---|---|---|---|
| a complex element initializer `/* e */ {` / `1,` / `2` / `}` inside `new Dictionary<int, int> { … }` | `1` and `2` in column 24 and `}` in 20, one level deeper than the comment and under it | `1` and `2` in column 32 and `}` in 28, under the brace | FMT-INDENT-023 |
| a nested initializer `Child =` / `/* c */ {` / `X = 2` / `}` | `X` in column 24 and `}` in 20 | `X` in column 32 and `}` in 28 | FMT-INDENT-023 |
| `int[] b =` / `/* c */ {` / `1,` / `2` / `};` | `1` and `2` in column 12 and `}` in 8 | `1` and `2` in column 20 and `}` in 16 | FMT-INDENT-023 |
| `List<int> x =` / `/* c */ [` / `1,` / `2` / `];`, and the same list pattern behind `is` | `1` and `2` in column 12 and `]` in 8 | `1` and `2` in column 20 and `]` in 16 | FMT-INDENT-025 |
| `var p =` / `/* c */ new P` / `{` / `X = 2` / `};`, the same with a collection initializer, and with an anonymous object `new` / `{` / `X = 1` / `};` | the braces in column 8 and the member in 12 | the braces in column 16, under `new`, and the member in 20 | FMT-INDENT-022, FMT-INDENT-024 |
| `return` / `/* c */ v switch` / `{` / `1 => 2,` / `_ => 3` / `};` | the braces in column 8 and the arms in 12 | the braces in column 16 and the arms in 20 | FMT-INDENT-027 |
| `Action a =` / `/* c */ () =>` / `{` / `M();` / `};` | the braces in column 8 and `M();` in 12 | the braces in column 16 and `M();` in 20 | FMT-INDENT-028 |
| `return` / `/* c */ a` / `&& b;` | `&&` in column 8 | `&&` in column 16 | FMT-INDENT-018 |
| `return` / `/* c */ a` / `? 1` / `: 2;` | `?` and `:` in column 12 | `?` and `:` in column 20 | FMT-INDENT-020 |
| `return` / `/* c */ s.Trim()` / `.ToUpper();` | `.ToUpper` in column 9 | `.ToUpper` in column 17, under `.Trim` | FMT-INDENT-029 |
| `return` / `/* c */ o is int` / `or long;` | `or` in column 10 | `or` in column 18, under `is` | FMT-INDENT-019 |

Where the comment stands on a line of its own above the anchor, the anchor starts its line and nothing changes. A token that starts its line behind a comment but anchors nothing changes nothing either: the opening brace of a creation initializer `/* Keep open. */ {` keeps its members one level deeper than `new`. RH5301 and RH5303, for the braces of an object or collection initializer, already measure `new` where it stands, and RH5305 measures the opening bracket of a collection expression where it stands: they report today's output of the fifth row and of the collection expression in the fourth, and accept the output after this change. The only rule check that measured an anchor at the comment instead is the complex-element check of RH5303 and RH5304, which SHARED-INIT-002 changes (see What).

In the rows of the second table whose comment follows a line that ends with `=` or `return`, the blank line above the comment and the column 8 the comment takes are unchanged.

The node-level entry points change where their move after the last phase misplaced lines, and the output of a code fix that formats through them changes with them. The first two rows were observed through the RH3202 fix, the third through the RH5303 fix:

| Input | Today | After this change | Requirement |
|---|---|---|---|
| `→→int Get() => 1;` inside `namespace N { →class C →{ … →} }`, converted to a block body | `int Get()` in column 2 and `return 1;` in 6 | `int Get()` in column 8 and `return 1;` in 12 | FMT-PIPE-017, FMT-TAB-001 |
| `→→string Get() => """` with its content and closing lines indented 16 spaces, converted to a block body | `return """` with the opening quotes in column 13, and the content and closing marker in 19 | the content and closing marker under the opening quotes | FMT-PIPE-017, FMT-RAW-001 |
| `        var d = /* a` / `                   b */ new List<int>` / `          {` / `            1` / `          };` | the fix puts the braces in column 30 and `1` in 34, and RH5303 reports its own result | the braces in column 24, under `new`, and `1` in 28, as document-level formatting puts them | FMT-PIPE-017, RH5303-005 |

The other members the move would misplace behave the same way: an object creation, a call's wrapped arguments, or a wrapped `&&` operand behind the last line of a multi-line comment, inside a declaration's value or an expression body that RH5301, RH5303, or RH5304 fixes. Behind a one-line comment, and in a target that starts its line at its column, the move changed nothing, and nothing changes.

Inside an interpolation hole, a comment line above a hole line that another comment leads changes as described under What: in `$"""` / `    a {` / `    // above` / `    /* c */ x` / `    } b` / `    """;` behind `        var a = `, `// above` moved to column 25, under `x`, and now moves to column 17, under `/* c */` (FMT-RAW-007, FMT-INDENT-006).

## Why

FMT-PIPE-001 says a phase may assume the phases before it ran and must not assume anything about the phases after it. Two places in the formatter break that rule, and every mismatch between their prediction and the phase they predict is either output that the next pass changes again (FMT-PIPE-004) or a column that is wrong on every pass, which a check for stability does not catch:

- **The indentation phase predicts the raw-string phase.** `LayoutComputer.GetAdjustedColumn` and `LayoutModel` add the move that `RawStringAlignmentPhase` will apply later, through `RawStringLineShifts`, so that an anchor on a line of a raw string literal is measured where that line ends up. Every alignment contributor, `ListElementAligner`, and the comment alignment read columns this way. The raw-string phase then computes the same move a second time from the text, so one decision is made by two components that can disagree.
- **The indentation phase and the raw-string phase predict the cleanup phase.** `RawStringLineShifts.GetColumnAfterTabReplacement` counts every tab in whitespace as the four spaces `CleanupPhase` writes at the end of the pipeline. The indentation phase registers the column of a hole line that keeps its whitespace this way, and the raw-string phase measures opening quotes and rewrites the whitespace of hole lines this way, doing part of the cleanup phase's job itself. The run that aligned interpolation holes found three failing shapes of this prediction in three consecutive audits — a tab in front of a comment on a hole line, in front of a nested literal's opening quotes, and on a hole line of a literal that moves left — and the class matrix in `squad/failure-classes.md` ("Tab width across formatter phases") records them.

Inside the indentation phase, a third disagreement decides where a token ends up: the layout computation and the rewriter that writes the line disagree about the column of a token behind a comment that leads its line. Folding the raw-string move into the phase makes the move depend on the layout's column, so this disagreement has to go in the same change; otherwise a literal on such a line would move to the layout's column and no longer start under its opening quotes (FMT-RAW-001), which RH5202 reports.

The node-level entry points add a fourth disagreement after the phases. They format the node as if it started at the column of its block, and then move the start of each line by the difference to the node's column in the document. The indentation phase has then decided every line's column already, and the move changes it again, but only for lines that start with whitespace: a line behind the last line of a multi-line comment and the lines of a raw string literal stay where the phase put them, while the lines aligned to them move, and the move counts a tab as one column where the phases count four. Measuring an anchor where it stands made the first of these visible in code fixes that format a declaration's value or an expression body. The move is the same prediction problem in another place, so it goes in the same change: the node is laid out at its own column, and nothing moves it afterwards.

Repairing such mismatches one shape at a time does not end: each repair fixes the shape it was shown and leaves the next. Removing the predictions removes the class. Layout is computed in spaces, so a tab is a fact of the input that no phase needs to see; and the column a raw string's line ends up in is a layout decision, which belongs to the phase that decides layout.

The inventory of the places where a phase reads, copies, or anticipates a later phase, taken from the formatter's sources:

| Site | Predicts | Requirements it protects | After this change |
|---|---|---|---|
| `LayoutComputer.GetAdjustedColumn`, `LayoutModel` (raw-string shift), `CommentIndentationContributor`, `ListElementAligner` | `RawStringAlignmentPhase` moving a literal's lines | FMT-INDENT-030, FMT-RAW-007, FMT-PIPE-004 | The move happens in the indentation phase itself |
| `LayoutComputer.RegisterRawStringHoleLines`, `LayoutModel` (registered hole-line column) | `CleanupPhase` replacing a tab with four spaces | FMT-RAW-007, FMT-CLEAN-002 | No tab is left to count |
| `RawStringAlignmentPhase` (opening-quote column, hole-line indentation) | `CleanupPhase` replacing a tab with four spaces | FMT-RAW-001, FMT-RAW-005, FMT-RAW-007, FMT-CLEAN-002 | No tab is left to count, and the phase is folded into the indentation phase |
| `RawStringAlignmentPhase` recomputing the shift the indentation phase already used | — (a second computation of one decision) | FMT-RAW-001, FMT-RAW-005 | One phase computes and applies the shift |
| `TrailingWhitespaceWriter` (a lone tab is not one space) | `CleanupPhase` expanding the tab, in its rationale only | FMT-SPACE-002, FMT-PIPE-004 | Unchanged behavior; no tab reaches it |
| `RH5202RawStringLiteralsShouldBeFormattedCorrectlyCodeFixProvider.RealignContent` | A copy of the raw-string shift outside the formatter | SHARED-FIX-003, FMT-RAW-009 | Not changed here; thoenissen/Reihitsu#1458 |
| `RootListPosition` (whether a blank line above a block statement survives) | `LineBreakPhase` keeping or removing that blank line, for the node-level entry points | FMT-PIPE-010 | Not changed here: it decides a blank line, not a column, and reuses the line-break phase's own predicate |
| `ReihitsuFormatterHelpers.ComputeTokenColumn` and `AdjustNodeIndentation`, used by `FormatNodeInDocumentAsync` and `FormatNodeInDocumentWithContextAsync` | The indentation phase's columns (they run after the phases: they measure the target's column in the document, counting a tab as one column, and move the formatted target's line starts, but not a raw string's content and closing lines, nor a line that starts inside a comment) | FMT-PIPE-010, SHARED-FIX-007 | No line's column changes after the last phase: the node is laid out from its first token's column in the document, counted as the phases count it (FMT-PIPE-017) |

## Decisions

### Tabs are replaced by a phase of their own, before every other phase

`TabReplacementPhase` runs first. It is a phase of its own, not a step of `StructuralTransformPhase`: the structural transforms rewrite syntax shape, and the node-level entry points switch some of them off (FMT-PIPE-011), while tab replacement applies to every entry point and every input. Running it before the structural transforms means no phase ever receives a tab in whitespace — neither a phase that measures columns nor one that copies whitespace, such as the field split, the documentation-comment rebuild, or the region formatting.

The replacement rule is unchanged: four spaces per tab, independent of the column, in whitespace trivia and between directive tokens, never inside a literal, a comment, disabled text, or a directive's free text. Its requirements move to the new phase's spec rather than staying in `cleanup.md`, because a requirement's ID names the spec of the phase that owns it.

### The indentation phase owns the final column of every line

The indentation phase places the opening quotes of every multi-line raw string literal and then moves the literal's lines by the distance FMT-RAW-001 and FMT-RAW-007 define. Every anchor is measured after that move, inside the same phase, so the move is the phase's own result and not a prediction. `RawStringAlignmentPhase` leaves the pipeline; the requirements of FMT-RAW stay where they are and describe how the indentation phase moves a raw string's lines.

The phase grows by the raw-string move, at most the 200 lines of today's raw-string phase on top of some 2,800, and the move's helpers (`RawStringLineShifts`, `RawStringLiteral`) are already used by the layout today, so no new dependency appears. What the fold does change is that no later phase measures the opening quotes again in the text: a wrong column in the layout would now misplace the literal itself, not only an anchor on its lines. The next decision removes the one known source of such a wrong column.

After the indentation phase, the column of every token, comment, and directive is final. The two phases after it only remove whitespace at the end of a line and line breaks at the end of the file (FMT-CLEAN) and rewrite line-ending characters (FMT-EOL).

### The layout computes the column the phase writes, and the raw string moves by it once

A token's column in the layout is the column it has in the indentation phase's output, counting a comment that stands in front of it on its line and the space behind that comment, and the tail of a multi-line comment that ends in front of it. The indentation phase moves a raw string's lines by the difference between that column of the opening quotes and the column of the closing marker, which is the same shift the anchors on the literal's lines were measured with. One component computes the shift and the phase applies it; nothing measures it again.

This is fixed before the raw-string move is folded, as structure first: with the layout's column right, the single move cannot misplace a literal, and every alignment that reads a column on a comment-led line reads the right one.

### An anchor that is the first code on its line is measured behind a comment too

The column of an anchor is where it stands, with no exception for an anchor that is itself the first code on its line. When a comment leads that line, the comment takes the column the line starts in, and every line aligned to the anchor follows the anchor behind the comment: the members of a complex element initializer behind `/* Keep element. */ {` start one level deeper than the brace, and its closing brace starts under it. One rule without an exception is the simpler rule, for the layout and for a reader, and it gives the same answer for an anchor that is first on its line as for one that is not.

The rules that check these layouts follow the same anchor, so they do not report the formatter's output. For `new` and for the opening bracket of a collection expression they already measure where the token stands. The complex-element check shared by RH5303 and RH5304 measured from the comment; SHARED-INIT-002 now measures from the brace. Code that the formatter laid out before this change, with a complex element's members one level deeper than the comment, is reported by these rules afterwards, and their code fixes lay it out as the formatter now does.

### Changed output is only output that violates an existing requirement today

This change moves decisions between phases and corrects one column the indentation phase computed wrongly; it adds no new layout rule, and it changes one rule check, SHARED-INIT-002, so that RH5303 and RH5304 agree with that column. Two more outputs change on purpose: the node-level entry points lay out their node as document-level formatting does (FMT-PIPE-017), where today's move after the phases left lines out of place against SHARED-FIX-007 and FMT-RAW-001, and a comment line above a comment-led hole line takes the column it takes on ordinary lines (FMT-RAW-007). The rules of FMT-CLEAN-002 to FMT-CLEAN-004 and of FMT-RAW keep their outputs. The inputs found whose output changes are listed under What. Any other input whose output changes belongs to one of the same two kinds: it is formatted differently in a second pass today, or it is stable in a column that FMT-INDENT-012 to FMT-INDENT-030 already rule out because a comment stands in front of a token on its line.

### The node-level entry points lay out their node at its own column

`FormatNodeInDocumentAsync` and `FormatNodeInDocumentWithContextAsync` hand the indentation phase the column the node's first token has in the document, so that the phase lays out every line of the node in its final column, and no step after the last phase changes a line's column (FMT-PIPE-017). The first token stays where it is, and the lines the depth places keep their place relative to it, as they do today. The indentation phase is then the only owner of a line's final column for node-level targets as for documents, and a code fix that formats through these entry points lays out its code in the columns the formatter gives it (SHARED-FIX-007).

The first token's column counts the characters in front of it as they stand, except a tab in the node's own leading whitespace, which the first phase replaces with four spaces, and a byte order mark, which the layout counts as zero columns. A tab in the whitespace in front of a node that does not start its line belongs to the code in front of the node, which the entry point keeps; it counts as one column, as the rules that check the fix's result count it. Counting it as four would make a fix such as RH5303's put the braces of a tab-indented declaration where its own rule reports them, and a second application would change nothing (SHARED-FIX-001, SHARED-FIX-005). SHARED-FIX-007 therefore does not count the move that formatting the result makes once it replaces that tab.

This change first withheld the RH5303 and RH5304 fixes for the one shape that exposed the move (RH5303-006, RH5304-005). The audit found the same misplacement for every anchor behind the last line of a multi-line comment, in the fixes of RH5301, RH5303, and RH5304, so the move is changed instead of guarded, as structure first, and the two requirements are retired. The fixes are offered for that shape and lay it out as the formatter does. The move's other two defects, the tab counted as one column and the raw-string lines left in place, go with it.

### A comment line above a comment-led line takes that line's column everywhere

A comment on its own line takes the column of the code below it (FMT-INDENT-006). When the line below starts with another comment, the code below starts in that comment's column, and on ordinary lines the comment above already takes that column. Inside an interpolation hole the comment above took the column of the token behind the leading comment; it now takes the leading comment's column there too, so a hole line is laid out like any other line (FMT-RAW-007).

### The RH5202 code fix and the node-level blank-line facts stay out of scope

The RH5202 code fix keeps its own copy of the raw-string shift until thoenissen/Reihitsu#1458 brings it in line with the formatter. `RootListPosition` anticipates a decision of the line-break phase, but the decision is a blank line rather than a column, and it reads the line-break phase's own predicate instead of a copy; it is recorded in the inventory and is not changed by this change. FMT-PIPE-001 therefore rules out predicted columns only and leaves predicted line breaks and blank lines undecided.


## Alternatives considered

- **Keep the predictions and repair each mismatch** — rejected: each repair fixed one shape and the next audit found another; three audit-repair rounds of one run went to this class.
- **Replace tabs inside `StructuralTransformPhase`** — rejected: the phase owns syntax-shape rewrites, and the node-level entry points configure which of them run (FMT-PIPE-011), while tab replacement must run everywhere. One phase would hold two unrelated jobs.
- **Replace tabs at the start of the indentation phase** — rejected: it would remove the tab predictions of the indentation and raw-string code, but every phase before it would still receive tabs, and whitespace those phases copy, such as the indentation of a split field, would carry tabs forward.
- **Run the raw-string phase before the indentation phase** — rejected: the target column of a literal is the column of its opening quotes, which the indentation phase decides.
- **Split the indentation phase in two around the raw-string phase** (place lines, move raw strings, then measure anchors) — rejected: a line that holds an anchor can also hold the opening quotes of another literal, so placing lines and moving literals depend on each other in both directions. The indentation phase already resolves such dependencies by repeating its alignment sweep until the columns settle; a split would turn that loop into a fixed sequence of phases that cannot repeat.
- **Keep `RawStringAlignmentPhase` separate and replace only the tabs first** — rejected: it removes every non-idempotent output found, but the layout keeps predicting the raw-string phase's move and the raw-string phase keeps computing it a second time; both inventory rows stay.
- **Fold the move, but measure the opening quotes again in the indented text as the phase's last step, and leave the comment-led column as it is** — rejected: the output would stay as it is today, including the stable wrong columns listed under What, and the shift would still be computed twice, only now inside one phase.
- **Fold the move and apply the layout's shift without correcting the layout's column** — rejected: on a line a comment leads, the layout takes the opening quotes to be short by the comment's width. `    /* c */ var s = """` / `    a` / `    """;` would put the content line and the closing marker in column 16, under opening quotes in column 24 — stable, against FMT-RAW-001, and reported by RH5202.
- **Keep the line's column for an anchor that is the first code on its line** (narrow FMT-INDENT-030 so that only an anchor behind other code on its line is measured behind the comment) — rejected: it would keep the RH5303 and RH5304 complex-element check and its tests unchanged, but it is a rule with an exception, and it would leave the formatter disagreeing with RH5301, RH5303, and RH5305 where those measure `new` or an opening bracket that a comment leads.
- **Withhold the RH5303 and RH5304 fixes for a complex element behind the last line of a multi-line comment, and leave the node-level move as it is** (RH5303-006, RH5304-005, the first choice of this change) — rejected after the audit: the move misplaces every line aligned to an anchor on such a line, in the fixes of other rules too, so a guard covers one member of the class only.
- **Widen the withholding to every fix that formats through the node-level entry points where such a line occurs** — rejected: it would turn a layout defect into missing fixes for every anchor kind, and keep a step that moves the indentation phase's columns afterwards, against the structure FMT-PIPE-001 sets for the phases.
- **Measure an anchor behind the last line of a multi-line comment at the line's column again** (an exception in FMT-INDENT-030) — rejected: it would bring back a rule with an exception, which the decision on anchors that are first on their line removed, only to fit the move after the phases.
- **Count a tab in front of a node that does not start its line as four columns**, as the phases count the tabs they replace — rejected: the rules count it as one character, so the fix's own rule would report the fix's result until the tab is replaced (SHARED-FIX-001, SHARED-FIX-005). Withholding those fixes instead would give away fixes that are safe, and making every alignment rule count a tab as four columns reaches far beyond this change.
- **Keep a comment line inside an interpolation hole at the column of the token behind the leading comment** — rejected: it lays out a hole line differently from an ordinary line, for no reason the hole gives.
- **Let the indentation phase compute the moves and a separate phase apply them** — rejected: phases receive only the output of the phase before them (FMT-PIPE-001). A plan handed from one phase to a later one outside the tree is the same shared decision in two components, which is what this change removes.

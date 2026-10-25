# No formatter phase predicts a later phase's columns

| | |
|---|---|
| Status | Draft |
| Date | 2026-10 |
| Affected requirements | FMT-PIPE-001, FMT-PIPE-015, FMT-PIPE-016, FMT-CLEAN-002, FMT-CLEAN-003, FMT-CLEAN-004, FMT-RAW-005, FMT-RAW-007, FMT-INDENT-011, FMT-INDENT-030, the spec `formatter/tab-replacement.md` (FMT-TAB-001 to FMT-TAB-003) |

## What

Two formatter decisions move between phases, so that no phase computes a column from what a later phase will do to it, and the indentation phase computes a token's column once:

- **Tabs in whitespace are replaced first.** A new first phase, `TabReplacementPhase`, replaces every tab in whitespace trivia and between the tokens of a directive with four spaces, before the structural transforms. `CleanupPhase` no longer replaces tabs. The rules themselves are unchanged and move from FMT-CLEAN-002, FMT-CLEAN-003, and the tab half of FMT-CLEAN-004 to FMT-TAB-001 to FMT-TAB-003.
- **The indentation phase moves the lines of raw string literals.** `RawStringAlignmentPhase` is no longer a phase of its own. The indentation phase places the opening quotes of a multi-line raw string literal and then moves the literal's content lines, its hole lines, and its closing marker as FMT-RAW specifies, in the same phase that measures every anchor. It moves them by the distance its own layout computed for the anchors, so the move is decided once. FMT-RAW keeps its requirements; FMT-RAW-005 and FMT-RAW-007 name the indentation phase where they named a separate phase or the cleanup phase.
- **The indentation phase measures a token behind a comment that leads its line where the token ends up.** On a line where a comment stands in front of the first token, the layout takes the line's column as the column of that token, while the phase writes the comment in that column and the token behind it. Every column derived from such a token — an alignment anchor, the opening quotes of a raw string literal — is therefore short by the width of the comment and the space after it. The layout now counts what stands in front of a token on its line, so the column it computes is the column the phase writes (FMT-INDENT-030, FMT-RAW-005). This is fixed first, because the single move above is correct only where the layout's column is the written one.

The pipeline keeps twelve phases. Two new pipeline requirements state what this guarantees: no phase after the first sees or writes a tab in whitespace (FMT-PIPE-016), and no phase after the indentation phase moves a token, a comment, or a directive within its line (FMT-PIPE-015).

Every example of FMT-CLEAN, FMT-RAW, and FMT-INDENT that exists today keeps its output. Output changes in two kinds of input only, and each change brings the output in line with a requirement that already exists:

- Output that today is not idempotent because the tab width was counted differently by two phases. It now reaches its final form in one pass, as FMT-PIPE-004 requires.
- Output that is stable today but wrong because a comment leads the line of an anchor or of a raw string's opening quotes. It now meets FMT-INDENT-012, FMT-INDENT-029, and FMT-INDENT-030.

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

In the sixth row the blank line FMT-BLANK-011 puts above the comment is unchanged. The seventh row is a line led by the last line of a multi-line comment; thoenissen/Reihitsu#1382 decides that the formatter breaks before such a token, which will later change that output again. Where a literal itself ends up does not change: `    /* c */ var s = """` / `    a` / `    """;` puts the literal under its opening quotes in column 24, today and after this change.

## Why

FMT-PIPE-001 says a phase may assume the phases before it ran and must not assume anything about the phases after it. Two places in the formatter break that rule, and every mismatch between their prediction and the phase they predict is either output that the next pass changes again (FMT-PIPE-004) or a column that is wrong on every pass, which a check for stability does not catch:

- **The indentation phase predicts the raw-string phase.** `LayoutComputer.GetAdjustedColumn` and `LayoutModel` add the move that `RawStringAlignmentPhase` will apply later, through `RawStringLineShifts`, so that an anchor on a line of a raw string literal is measured where that line ends up. Every alignment contributor, `ListElementAligner`, and the comment alignment read columns this way. The raw-string phase then computes the same move a second time from the text, so one decision is made by two components that can disagree.
- **The indentation phase and the raw-string phase predict the cleanup phase.** `RawStringLineShifts.GetColumnAfterTabReplacement` counts every tab in whitespace as the four spaces `CleanupPhase` writes at the end of the pipeline. The indentation phase registers the column of a hole line that keeps its whitespace this way, and the raw-string phase measures opening quotes and rewrites the whitespace of hole lines this way, doing part of the cleanup phase's job itself. The run that aligned interpolation holes found three failing shapes of this prediction in three consecutive audits — a tab in front of a comment on a hole line, in front of a nested literal's opening quotes, and on a hole line of a literal that moves left — and the class matrix in `squad/failure-classes.md` ("Tab width across formatter phases") records them.

Inside the indentation phase, a third disagreement decides where a token ends up: the layout computation and the rewriter that writes the line disagree about the column of a token behind a comment that leads its line. Folding the raw-string move into the phase makes the move depend on the layout's column, so this disagreement has to go in the same change; otherwise a literal on such a line would move to the layout's column and no longer start under its opening quotes (FMT-RAW-001), which RH5202 reports.

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
| `ReihitsuFormatterHelpers.ComputeTokenColumn` and `AdjustNodeIndentation`, used by `FormatNodeInDocumentAsync` and `FormatNodeInDocumentWithContextAsync` | — (they run after the phases: they measure the target's column in the document, counting a tab as one column, and move the formatted target's line starts, but not a raw string's content and closing lines) | FMT-PIPE-010 | Not changed here: the step is not a phase, so FMT-PIPE-015 and FMT-PIPE-016 do not reach it |

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

### Changed output is only output that violates an existing requirement today

This change moves decisions between phases and corrects one column the indentation phase computed wrongly; it adds no new layout rule. The rules of FMT-CLEAN-002 to FMT-CLEAN-004 and of FMT-RAW keep their outputs. The inputs found whose output changes are listed under What. Any other input whose output changes belongs to one of the same two kinds: it is formatted differently in a second pass today, or it is stable in a column that FMT-INDENT-012 to FMT-INDENT-030 already rule out because a comment stands in front of a token on its line.

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
- **Let the indentation phase compute the moves and a separate phase apply them** — rejected: phases receive only the output of the phase before them (FMT-PIPE-001). A plan handed from one phase to a later one outside the tree is the same shared decision in two components, which is what this change removes.

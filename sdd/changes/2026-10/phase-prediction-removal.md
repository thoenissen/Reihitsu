# No formatter phase predicts a later phase's columns

| | |
|---|---|
| Status | Draft |
| Date | 2026-10 |
| Affected requirements | FMT-PIPE-001, FMT-PIPE-015, FMT-PIPE-016, FMT-CLEAN-002, FMT-CLEAN-003, FMT-CLEAN-004, FMT-RAW-005, FMT-RAW-007, FMT-INDENT-011, FMT-INDENT-030, the spec `formatter/tab-replacement.md` (FMT-TAB-001 to FMT-TAB-003) |

## What

Two formatter phases change their place in the pipeline, so that no phase computes a column from what a later phase will do to it:

- **Tabs in whitespace are replaced first.** A new first phase, `TabReplacementPhase`, replaces every tab in whitespace trivia and between the tokens of a directive with four spaces, before the structural transforms. `CleanupPhase` no longer replaces tabs. The rules themselves are unchanged and move from FMT-CLEAN-002, FMT-CLEAN-003, and the tab half of FMT-CLEAN-004 to FMT-TAB-001 to FMT-TAB-003.
- **The indentation phase moves the lines of raw string literals.** `RawStringAlignmentPhase` is no longer a phase of its own. The indentation phase places the opening quotes of a multi-line raw string literal and then moves the literal's content lines, its hole lines, and its closing marker as FMT-RAW specifies, in the same phase that measures every anchor. FMT-RAW keeps its requirements; FMT-RAW-005 and FMT-RAW-007 name the indentation phase where they named a separate phase or the cleanup phase.

The pipeline keeps twelve phases. Two new pipeline requirements state what this guarantees: no phase after the first sees or writes a tab in whitespace (FMT-PIPE-016), and no phase after the indentation phase moves a token, a comment, or a directive within its line (FMT-PIPE-015).

The formatted output is meant to stay as it is. Every example of FMT-CLEAN, FMT-RAW, and FMT-INDENT keeps its output, and so does every output the formatter tests assert. What can change is output that today is not idempotent because a prediction disagreed with the phase it predicted: such an input now reaches its final form in one pass, as FMT-PIPE-004 already requires.

## Why

FMT-PIPE-001 says a phase may assume the phases before it ran and must not assume anything about the phases after it. Two places in the formatter break that rule, and every mismatch between their prediction and the phase they predict is output that the next pass changes again (FMT-PIPE-004):

- **The indentation phase predicts the raw-string phase.** `LayoutComputer.GetAdjustedColumn` and `LayoutModel` add the move that `RawStringAlignmentPhase` will apply later, through `RawStringLineShifts`, so that an anchor on a line of a raw string literal is measured where that line ends up. Every alignment contributor, `ListElementAligner`, and the comment alignment read columns this way. The raw-string phase then computes the same move a second time from the text, so one decision is made by two components that can disagree.
- **The indentation phase and the raw-string phase predict the cleanup phase.** `RawStringLineShifts.GetColumnAfterTabReplacement` counts every tab in whitespace as the four spaces `CleanupPhase` writes at the end of the pipeline. The indentation phase registers the column of a hole line that keeps its whitespace this way, and the raw-string phase measures opening quotes and rewrites the whitespace of hole lines this way, doing part of the cleanup phase's job itself. The run that aligned interpolation holes found three failing shapes of this prediction in three consecutive audits — a tab in front of a comment on a hole line, in front of a nested literal's opening quotes, and on a hole line of a literal that moves left — and the class matrix in `squad/failure-classes.md` ("Tab width across formatter phases") records them.

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

## Decisions

### Tabs are replaced by a phase of their own, before every other phase

`TabReplacementPhase` runs first. It is a phase of its own, not a step of `StructuralTransformPhase`: the structural transforms rewrite syntax shape, and the node-level entry points switch some of them off (FMT-PIPE-011), while tab replacement applies to every entry point and every input. Running it before the structural transforms means no phase ever receives a tab in whitespace — neither a phase that measures columns nor one that copies whitespace, such as the field split, the documentation-comment rebuild, or the region formatting.

The replacement rule is unchanged: four spaces per tab, independent of the column, in whitespace trivia and between directive tokens, never inside a literal, a comment, disabled text, or a directive's free text. Its requirements move to the new phase's spec rather than staying in `cleanup.md`, because a requirement's ID names the spec of the phase that owns it.

### The indentation phase owns the final column of every line

The indentation phase places the opening quotes of every multi-line raw string literal and then moves the literal's lines by the distance FMT-RAW-001 and FMT-RAW-007 define. Every anchor is measured after that move, inside the same phase, so the move is the phase's own result and not a prediction. `RawStringAlignmentPhase` leaves the pipeline; the requirements of FMT-RAW stay where they are and describe how the indentation phase moves a raw string's lines.

After the indentation phase, the column of every token, comment, and directive is final. The two phases after it only remove whitespace at the end of a line and line breaks at the end of the file (FMT-CLEAN) and rewrite line-ending characters (FMT-EOL).

### The output stays as it is

This change moves decisions between phases; it does not decide anything differently. The rules of FMT-CLEAN-002 to FMT-CLEAN-004 and of FMT-RAW keep their outputs. An input whose output changes is one that the current pipeline formats differently in a second pass, and the new output is the one FMT-PIPE-004 requires.

### The RH5202 code fix and the node-level blank-line facts stay out of scope

The RH5202 code fix keeps its own copy of the raw-string shift until thoenissen/Reihitsu#1458 brings it in line with the formatter. `RootListPosition` anticipates a decision of the line-break phase, but the decision is a blank line rather than a column, and it reads the line-break phase's own predicate instead of a copy; it is recorded in the inventory and is not changed by this change. FMT-PIPE-001 therefore rules out predicted columns only and leaves predicted line breaks and blank lines undecided.

## Alternatives considered

- **Keep the predictions and repair each mismatch** — rejected: each repair fixed one shape and the next audit found another; three audit-repair rounds of one run went to this class.
- **Replace tabs inside `StructuralTransformPhase`** — rejected: the phase owns syntax-shape rewrites, and the node-level entry points configure which of them run (FMT-PIPE-011), while tab replacement must run everywhere. One phase would hold two unrelated jobs.
- **Replace tabs at the start of the indentation phase** — rejected: it would remove the tab predictions of the indentation and raw-string code, but every phase before it would still receive tabs, and whitespace those phases copy, such as the indentation of a split field, would carry tabs forward.
- **Run the raw-string phase before the indentation phase** — rejected: the target column of a literal is the column of its opening quotes, which the indentation phase decides.
- **Split the indentation phase in two around the raw-string phase** (place lines, move raw strings, then measure anchors) — rejected: a line that holds an anchor can also hold the opening quotes of another literal, so placing lines and moving literals depend on each other in both directions. The indentation phase already resolves such dependencies by repeating its alignment sweep until the columns settle; a split would turn that loop into a fixed sequence of phases that cannot repeat.
- **Let the indentation phase compute the moves and a separate phase apply them** — rejected: phases receive only the output of the phase before them (FMT-PIPE-001). A plan handed from one phase to a later one outside the tree is the same shared decision in two components, which is what this change removes.

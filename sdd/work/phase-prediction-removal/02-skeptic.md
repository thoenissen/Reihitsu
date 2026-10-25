Role: skeptic
Commit: e5bf2f0
Verdict: question for the maintainer

The prompt carried no intended fix, no suspected cause, and no verdict anyone hoped for. Everything below was observed on e5bf2f0 with `scripts/trace.sh` (3 passes), `scripts/format.sh`, and `scripts/apply-fix.sh`, on fixtures in a scratch directory outside the repository; the fixtures are deleted. In inputs, `→` stands for a tab and `/` separates lines; every fixture sits in `class C { void M() { ... } }` unless stated. Columns are 0-based.

## Premise

The issue is real. In two places it claims more than the evidence shows.

- **The prediction sites exist as the inventory lists them.** `LayoutComputer.GetAdjustedColumn` adds `LayoutModel.GetRawStringShift`, and `LayoutModel.Set` subtracts it. `RawStringLineShifts.GetColumnAfterTabReplacement` is called by `LayoutComputer.RegisterRawStringHoleLines` and twice by `RawStringAlignmentPhase` (`CollectReplacements`, `GetIndentation`). `RawStringAlignmentPhase.ComputeReplacement` computes the shift again from the indented text.
- **The tab-width class still fails today, outside raw-string holes.** The repairs of thoenissen/Reihitsu#1457 closed only the hole-line cells of the catalog matrix. Two members fail, both not idempotent (FMT-PIPE-004), and both are removed by replacing tabs first (D1 below):
  - t06 `var x = a→/** c */.B()` / `.C();`: pass 1 puts `.C()` at 26, pass 2 at 29. The same input with four spaces instead of the tab gives 29 in one pass.
  - t07 `var s = x→/** d */.Foo(` / `"""` / `    a` / `    """.Trim()` / `.ToUpper());`: pass 1 puts `.ToUpper` at 34, pass 2 at 37. With four spaces it gives 37 in one pass.
- **Replacing tabs first changed no stable output.** The new pipeline's first phase writes every whitespace tab as four spaces, and the remaining phases then receive exactly that input. So its output equals today's output for the same input with the tabs already written as spaces. For each of the following, the tab input and the space input gave identical output:
  - f01–f10 cover tabs in indentation, in token gaps, and in front of an end-of-line comment and a `/** */`; `#region→foo`; tab-indented `///` lines and a single-line `<summary>`; tab-indented usings inside a namespace that get reordered; a tab-indented split field; and an inactive `#if` branch with a tab in the disabled text and tabs before `#else`/`#endif`.
  - t01–t05 cover an argument, a binary operand, an initializer, a conditional, and raw-string opening quotes, each behind a tab before a `/** */`.
  - Only t06 and t07 differed, and both are unstable today.
- **The FMT-PIPE-015 example matches today's output** (p015, stable after one pass).
- **Overstated: "every mismatch ... is a non-idempotent output".** The catalog matrix already holds one cell that is stable and wrong, and D2 below produces more (g01, g02, g08). A stability check passes all of them (FC-017).
- **Overstated: "the formatter test suite ... passes unchanged".** Some tests call phases directly, and those phases lose or move the work: the 13 tests of `RawStringAlignmentPhaseTests` (they call `RawStringAlignmentPhase.Execute`), the tab tests of `CleanupPhaseTests`, and the phase list in `FormattingPipelineTests`. The full-pipeline tests can pass unchanged, including the 10 tests in `RawStringInterpolationHoleTests` whose names contain `Tab`, which already write tabs as `→` and replace them at run time.

## Mechanism and candidates

Two defects reach the run.

**D1 — tab width.** This is the defect the run's tab half removes.

Mechanism: a tab in whitespace that no phase before the indentation phase rewrites reaches that phase and counts as one column there. The cleanup phase then writes it as four spaces, so every token behind it on its line ends up three columns away from where the anchors were measured.

The candidates are the whitespace that the phases before indentation keep as written:

| Whitespace kept as written | Defined by | What is measured behind it | Result today |
|---|---|---|---|
| In front of a `/** */` that the right token carries | FMT-SPACE-014 | a chain link (t06); raw-string opening quotes whose closing line carries an anchor (t07) | fails, not idempotent |
| same | FMT-SPACE-014 | an argument (t01), a binary operand (t02), an initializer element (t03), a conditional branch (t04) | holds: the comment and the operand are moved onto their own line, and the tab becomes trailing whitespace |
| In front of an end-of-line comment | FMT-SPACE-001 | nothing | holds |
| In front of a comment that the left token carries | FMT-SPACE-013 | — | holds: collapsed to one space |
| After a `///` | FMT-SPACE-015 | the start of the next line, which indentation rewrites | holds |
| A hole line of a multi-line interpolated raw string that no construct in the hole places | FMT-RAW-007 | the catalog matrix cells | hold since #1457 (existing tests) |
| Between the tokens of a directive | FMT-CLEAN-003 | nothing | holds |

**D2 — the layout model's column on a line that a comment leads.** This one is an architecture finding (below).

Mechanism: `LayoutComputer.GetColumnBeforeRawStringShift` takes a line's layout column as the column of the line's first token. `IndentationRewriter.RebuildLeadingTrivia` writes that column in front of the first trivia on the line instead. So when a comment stands in front of the first token on that token's own line, every column the model derives for a token on that line is short by the width of the comment and the whitespace after it.

Membership: a line whose first token's leading trivia holds anything other than whitespace in front of the token, on the token's own line.

| In front of the first token, on its line | Type | Result today |
|---|---|---|
| nothing, or only whitespace | `WhitespaceTrivia` | holds |
| a one-line `/* */` | `MultiLineCommentTrivia` | fails (g01, g02, g06, g07, g08) |
| a one-line `/** */` | `MultiLineDocumentationCommentTrivia` | fails (g04) |
| the tail of a multi-line `/* */` | `MultiLineCommentTrivia` | fails (g05). thoenissen/Reihitsu#1382 (P33) decides that the formatter breaks before such a token, which removes this member |
| `//`, `///`, a directive, disabled text | — | not members: each ends with its line |

Two kinds of code read the wrong column:
- the alignment contributors, through `GetAdjustedColumn` (g02 FMT-INDENT-012, g07 FMT-INDENT-029);
- the raw-string shift, through `GetRawStringShift` (g01, g06, g08: anchors on a closing line, FMT-INDENT-030).

Observed today. Every output below is stable after one pass:
- g02 `        /* c */ Call(1,` / `2);` → `2` at 13, while `1` stands at 21.
- g04 `        /** d */ Call(1,` / `2);` → `2` at 13, while `1` stands at 22.
- g07 `        /* c */ a.B()` / `.C();` → `.C()` at 9, while `.B` stands at 17.
- g01 `        /* c */ var s = """` / `    a` / `    """.Trim()` / `    .ToUpper();` → the literal is under its opening quotes at 24, because the raw-string phase measures the text, but `.ToUpper` is at 19. The IndentationPhase diff places `.ToUpper` at 19 while the closing line still starts at 4. So the model takes the opening quotes to be at 16 (a shift of 12), and the raw-string phase then applies 20.
- g06 is g01 with `$"""` and a hole `{x}` → the literal at 25, `.ToUpper` at 20.
- g08 `        var s = Call(1,` / `            /* c */ """` / `    a` / `    """.Trim()` / `    .ToUpper());` → the literal at 29, `.ToUpper` at 24. The model takes the quotes to be at 21.
- g03 `    /* c */ var s = """` / `    a` / `    """;` → the literal at 24, which is correct today.

## Breaking inputs

Failure classes that reach the change:
- FC-012, FC-015, FC-016 — tab width counted by two components (D1).
- FC-013 — RH5202 measures the actual column of the opening quotes, so a literal moved to the model's column would be reported on formatter output (Q1).
- FC-017 — D2 and one D1 cell are stable and wrong, so assert exact output, not stability.
- FC-019, FC-021 — a comment leading the line, and a `/* */` that ends in the middle of a line (D2).
- FC-022, FC-027 — plain, UTF-8, and interpolated raw strings; a literal nested in a hole (FMT-RAW-008); shifts left, right, and none.
- FC-010, FC-033 — every case under LF and CRLF; tabs written as `→` in test sources and replaced at run time.
- FC-028 — `FormatNode` and `FormatNodeInDocumentAsync` with tabs inside the target: tab replacement runs there too.
- FC-031 — stale summaries on `LayoutModel`, `LayoutComputer.RegisterRawStringHoleLines`, `RawStringLineShifts`, `CommentIndentationContributor`, `TrailingWhitespaceWriter`, `CleanupPhase`, and `FormattingPipeline.CreatePhases`.

Inputs the test implementer must cover:

- **D1 (FMT-PIPE-016, FMT-TAB-001):**
  - t06 and t07 in one pass, expected 29 and 37 (the output of the space variant).
  - The existing catalog-cell tests in `RawStringInterpolationHoleTests`, which must stay green.
  - The cells that hold today and must keep holding: the end-of-line comment gap (the FMT-TAB-001 example), the left-token comment gap, and t01–t04.
- **Text that keeps its tabs (FMT-TAB-003):**
  - `/// <param→name="x">b</param>` keeps its tab today (d01), and must keep it.
  - A tab-indented `///` continuation line is realigned by the indentation phase (g11 `int x; /// first` / `→/// second` / `int y;` in a class; existing `IndentationTests.DocumentationCommentContinuationLineWithTabIsRealigned`).
  - Disabled text, and the whitespace before `#else`/`#endif` inside an inactive branch (f08).
- **Directives (FMT-TAB-002):** `#region→foo` becomes `#region Foo` with one space through the full pipeline (f02), not four (S2).
- **Raw strings (FMT-RAW-001/005, FMT-INDENT-030):**
  - g03 and g08: the literal must stay under its opening quotes (24 and 29). This case catches a move taken from the model's column (Q1).
  - g01 and g06: the expected output depends on the answer to Q1.
  - The new FMT-INDENT-030 example and its unmoved-literal side, and the FMT-PIPE-015 example (p015).
- **Outside raw strings:** g02, g04, and g07 change only under Q1 B.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `TabReplacementPhase` (new) | must change: the scope of today's `CleanupPhase.NormalizeWhitespaceTabs` and `NormalizeDirectiveTriviaTabs`, no wider (S1) |
| `CleanupPhase` | must change: the tab code is removed; the rest must stay |
| `FormattingPipeline.CreatePhases` | must change |
| `IndentationPhase`, `LayoutComputer`, `LayoutModel`, `IndentationRewriter` | must change; how depends on Q1 |
| `RawStringAlignmentPhase` | removed under the approved 3A; its tests move |
| `RawStringLineShifts`, `RawStringLiteral` | must change: `GetColumnAfterTabReplacement` goes, the summaries change |
| `TrailingWhitespaceWriter` | summary only; the behavior must stay |
| Every other formatter phase | must stay (D1 probes show no stable change) |
| RH5601 analyzer and fix | must stay (spec references retagged only) |
| RH5202 analyzer | must stay; it is the counterpart that checks formatter output (FC-013) |
| RH5202 code fix | not involved (thoenissen/Reihitsu#1458) |
| `ReihitsuFormatter` node-level entry points, `ReihitsuFormatterHelpers` | must stay in this run; see follow-ups 1 and 2 |
| `Reihitsu.Core` (`SyntaxTokenPositionUtilities.IsFirstOnLine` is shared with RH5204) | must stay |
| Rule pages | not involved |
| CLI, `scripts/trace.sh` | not involved; the trace still shows twelve phases, and the raw-string move appears inside the IndentationPhase diff |
| Tests | `FormattingPipelineTests`, `CleanupPhaseTests` tab tests, `RawStringAlignmentPhaseTests`, `WhitespaceCleanupTests` retags, and the new FMT-PIPE-015/016 and FMT-TAB tests |

## Simpler option

The tab half alone removes every failure observed in this class: t06, t07, and all the catalog cells. On input without tabs, the fold of raw-string alignment changes no output unless D2 is fixed too, and fixing D2 does not need the fold. So the fold's benefit is structural: one computation of the shift instead of two. Keeping `RawStringAlignmentPhase` as a separate phase in this run would leave inventory row 1. The new FMT-PIPE-001 sentence and the FMT-RAW-005/FMT-INDENT-030 edits would then have to be narrowed. This is Q1 C.

## Questions for the maintainer

### The concern raised under escalation 2 (3A: a larger indentation phase)

- **Size.** The indentation phase is about 2,840 lines: 25 in the phase, 1,262 in its utilities, and 1,556 in its contributors. `RawStringAlignmentPhase` is 202 lines. `RawStringLineShifts` and `RawStringLiteral` (349 lines) are already used by `LayoutComputer` and `LayoutModel` today, so the fold adds no new dependency direction, and the tab counting it removes shrinks the code. The growth is about 7 %. Size is not the risk.
- **The risk is losing the independent measurement.** Today the raw-string phase measures the opening quotes in the indented text, so an error in the model's column (D2) misplaces at most an anchor on the literal's lines (g01: `.ToUpper`). If the folded phase moves the literal by the model's shift — the single computation that inventory row 4 and the change doc's last alternative call for — then the literal itself follows the model's error. g03 would become `        /* c */ var s = """` / `                a` / `                """;`: the closing marker at 16, under opening quotes at 24. That output is stable, violates FMT-RAW-001, and RH5202 reports it (FC-013). The column 16 comes from g01's IndentationPhase diff (see D2).
- **Smaller costs.** The 13 phase-level tests move. `trace.sh` and the phase observer no longer show the move as its own step.
- No other new risk was found. The alignment sweep loop already includes the shift through `GetRawStringShift`, and node-level formatting already ran the raw-string phase.

### Q1 — Which column does the indentation phase move a raw string's lines to, and is D2 fixed in this run?

FMT-RAW-005 now says "the column the indentation phase gives the opening quotes". On a line that a comment leads, the model gives one column and the indented tree another, so that wording does not decide between the options below. They produce different output:

- **A — measure in the indented tree.** The last step of the indentation phase measures the opening quotes in the tree it has just indented, as the raw-string phase does today.
  - Output: unchanged everywhere. g03 stays at 24, and g01's `.ToUpper` stays at 19 (stable and wrong, as today).
  - Cost: the layout still adds a predicted shift for anchors, and the last step measures it again. Inventory row 4 (two computations of one decision) survives inside one phase. The FMT-INDENT-030 sentence the spec diff adds holds only on lines that no comment leads, and FMT-RAW-005 has to name the indented tree.
- **B — fix the model.** The model counts the trivia in front of a line's first token, and the phase moves the literal by the model's own shift, as one computation.
  - Output that changes from stable today: g02 `2` → 21, g04 → 22, g07 `.C()` → 17, g01 `.ToUpper` → 27, g06 → 28, g08 → 32. Each then meets FMT-INDENT-012, FMT-INDENT-029, and FMT-INDENT-030.
  - Cost: the change doc's "output stays as it is" must list these outputs. The change touches the column every contributor reads (scope, escalation 5).
- **C — keep `RawStringAlignmentPhase` separate in this run; do the tab half only.**
  - Output: the same as A.
  - Cost: inventory rows 1 and 4 remain. The new FMT-PIPE-001 sentence and the FMT-RAW/FMT-INDENT edits of the spec diff have to be narrowed so that one phase is named as the owner of the remaining dependency, which the issue's "Done when" allows.

One implementation is not an option: moving by the model's shift without fixing the model. It breaks g03 as shown above.

### Spec precision (for the spec author; one reading only, no decision needed)

- **S1.** FMT-PIPE-016 ("every phase after the first receives and returns whitespace without tabs") and FMT-TAB-001 ("every tab character in whitespace trivia") also cover the whitespace inside a documentation comment's structure, which FMT-TAB-003 keeps.
  - d01 keeps `<param→name="x">` through today's whole pipeline.
  - In g11 the `→/// second` line carries its tab from the DocumentationCommentFormattingPhase output through the HorizontalSpacingPhase output, until IndentationPhase realigns the marker.
  - Replacing those tabs in the first phase would change d01, against FMT-TAB-003 and against the change doc's "output stays".
  - Both requirements should name the whitespace FMT-TAB covers: outside comments, documentation comments, literals, disabled text, and a directive's free text. The FMT-PIPE-016 observer test then scans only that whitespace.
- **S2.** FMT-TAB-002's new sentence says a tab between `#region` and its description "becomes four spaces". That holds for the first phase's output only. Through the pipeline, `#region→foo` ends as `#region Foo`, because `RegionFormattingPhase` rebuilds the directive (f02 trace). A full-pipeline test derived from the sentence would fail.
- **S3.** FMT-RAW-005's wording follows whichever option Q1 picks.

## Architecture findings

**D2 — two components of `IndentationPhase` decide where a token on a comment-led line ends up.**
- **Components:** `LayoutComputer` and `LayoutModel` (`GetColumnBeforeRawStringShift`, `TryGetPlacedColumnBeforeRawStringShift`, `GetAdjustedColumn`, `GetRawStringShift`), and `IndentationRewriter` (`RebuildLeadingTrivia`, `AddLineStartWhitespace`).
- **Shared decision:** the column a token has after the indentation phase, on a line where a comment stands in front of the first token. The model puts the token at the layout column; the rewriter puts the comment there.
- **Inputs that show it:** g02, g04, and g07 (alignment anchors); g01, g06, and g08 (the raw-string shift).
- **Why it reaches the run:** it is a pre-existing defect that the approved 3A newly depends on. Moving the literal as the phase's own result, with every anchor measured after the move, is correct only where the model's column equals the indented tree's column. g03 shows the regression otherwise.

D1 is the structural defect the run itself removes. The maintainer has already chosen its structure (2A), and the newly found members t06 and t07 are covered by that design, so D1 needs no further decision.

## Follow-up candidates

1. **The node-level column offset counts a tab as one column.**
   - Code: `FormatNodeInDocumentAsync` and `FormatNodeInDocumentWithContextAsync` measure the target's original column with `ReihitsuFormatterHelpers.ComputeTokenColumn` (a character offset) and shift the formatted target by the difference with `AdjustNodeIndentation`.
   - Observed: RH3202 on `namespace N { →class C →{ →→int Get() => 1; ... } }` writes `int Get()` at 2 spaces and `return 1;` at 6, under LF and CRLF (a01).
   - Structural cause: two components count a tab's width differently, the pipeline as four (FMT-TAB-001) and the node-level offset as one.
   - It is unchanged by this run, because it measures outside the pipeline. It does fall within the issue's inventory scope ("code fixes that reuse formatter logic"), so the inventory posted to the issue should list it.
2. **`AdjustNodeIndentation` moves line starts after the pipeline, but not a raw string's content and closing lines.**
   - Observed: the same fix on `→→string Get() => """` with its content and closing lines indented 16 spaces gives `return """` with the quotes at 13 and the content and closing at 19 (a02).
   - Structural cause: a column move after the phase that aligns raw strings. FMT-PIPE-015 does not reach it, because it is not a phase.
3. **t04 is not idempotent, with spaces as well as with a tab.**
   - Input: `var x = c    /** d */ ? 1` / `: 2;`.
   - Observed: `LineBreakPhase` moves the `/** */` onto its own line after `BlankLinePhase` has run, so the blank line above it appears only in pass 2.
   - Cause: a blank-line decision depends on a later phase's line break, the class that FMT-PIPE-001 now leaves undecided.
   - The relocation itself is thoenissen/Reihitsu#962.
4. **The catalog matrix "Tab width across formatter phases" is out of date.**
   - Its axis "what the tab stands in front of" lacks the FMT-SPACE-014 gap (t06, t07).
   - Its **Decision** paragraph describes the component split this run removes.
   - It is a squad file, outside this role.

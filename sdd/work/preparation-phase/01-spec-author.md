Role: spec author
Commit: 07cb8de
Verdict: change doc written

The spec diff below follows the change doc `sdd/changes/2026-10/preparation-phase.md`.

## Approval

The maintainer approved the change doc (escalation 2), recorded in `squad/decisions/2026-10-30-preparation-phase-change-doc.md`. What and Why are approved as written. FMT-PIPE-020 covers every terminator FMT-EOL-001 lists, as drafted, so the U+0085 and U+2028 cases are in scope. The doc stays under `sdd/changes/2026-10/`. Its `Status` is now `Accepted`.

## Changed requirements

- FMT-PIPE-001 — Fixed phase order — changed: the table starts with the preparation phase's sub-phases `PreparationPhase/TabReplacement` (1.1) and `PreparationPhase/LineEndings` (1.2); `LineEndingNormalizationPhase` no longer runs last; a paragraph states that each sub-phase is a phase of its own, reported under its table name, and that `PreparationPhase` itself is not reported; the "Why" no longer says line endings are normalized last.
- FMT-PIPE-015 — Columns are final after the indentation phase — changed: the allowance to rewrite line-ending characters after the indentation phase is dropped; the example's pointer moves from FMT-PIPE-016 to FMT-PIPE-019.
- FMT-PIPE-016 — No phase after the first sees or writes a tab in whitespace — retired, pointing at FMT-PIPE-019.
- FMT-PIPE-017 — Node-level targets take their columns from the indentation phase — changed in wording only: a tab the entry point does not keep counts with the width the preparation phase gives it (FMT-PIPE-019) instead of restating "four columns" from FMT-TAB-001. The columns do not change.
- FMT-PIPE-019 — After the preparation phase, no phase sees or writes a tab in whitespace or a foreign line ending — added.
- FMT-PIPE-020 — The layout does not depend on the input's line endings — added.
- FMT-TAB-001 — changed in "Why" only (pointer FMT-PIPE-016 → FMT-PIPE-019); FMT-TAB scope sentence and `Uses` updated.
- FMT-EOL-001, FMT-EOL-003 — changed in "Why" only; FMT-EOL scope sentence, one non-goal pointer, and `Uses` updated. The rules of FMT-EOL-001 to FMT-EOL-004 are unchanged.
- `cleanup.md` non-goal: "first phase of the pipeline" → "preparation phase" (no requirement changed).
- Ledger: FMT-PIPE-019 and FMT-PIPE-020 inserted after FMT-PIPE-018.
- `sdd/specs/README.md` and the pipeline spec header: `PreparationPhase` added as code owner of the pipeline spec. The type does not exist yet, so `SpecConsistencyTests` fails until the implementer adds it.
- Outside the specs, written to match the change doc: the `AGENTS.md` architecture paragraph and ownership table (rows `PreparationPhase/TabReplacement` and `PreparationPhase/LineEndings` first, `LineEndingNormalizationPhase` row removed; still twelve rows) and the trace description in `scripts/README.md`. `PhaseOwnershipDocumentationTests` fails against this text until the observer reports the new names.

The type names `TabReplacementPhase` and `LineEndingNormalizationPhase` stay the code owners of FMT-TAB and FMT-EOL (a decision of the change doc). If the implementer renames either, the spec headers and `sdd/specs/README.md` must follow.

## Examples for the tests

- FMT-PIPE-001 — observed order: `PreparationPhase/TabReplacement`, `PreparationPhase/LineEndings`, then `StructuralTransformPhase` … `CleanupPhase` (twelve observed phases); a sub-phase run alone through `FormattingPipeline.ExecutePhases` is reported under the same name (change doc, *Sub-phases are phases of their own*).
- FMT-PIPE-019 — the tab example (moved from FMT-PIPE-016; the existing FMT-PIPE-016 tests in `PhaseColumnOwnershipTests` retag to it), and the brace-wrapping example: an LF file with `if (a)` / `// leading` / `Foo();`, where no phase's output holds a line ending other than `\n`, and the same under `\r\n`. The issue's per-phase test (every phase on LF and CRLF input, no foreign line ending outside comments, literals, and disabled text) derives from this requirement.
- FMT-PIPE-020 — the lone-`\r` two-region class (the issue's example) gives the LF layout shown; the same with U+0085, U+2028, U+2029, and with mixed `\r\n` / `\n` / lone `\r`.
- FMT-PIPE-015, FMT-PIPE-017 — existing tests unchanged in outcome.

## Statements of thoenissen/Reihitsu#1485 checked against 07cb8de

- Today's output for the lone-`\r` example: reproduced with `scripts/format.sh` — `#region A` in column 0, every line break `\n`. Formatting that output a second time moves `#region A` to column 4, so today's behavior breaks FMT-PIPE-004. The issue's statement that FMT-PIPE-007 and FMT-EOL-001 "promise the LF layout" is overstated: both specify line-ending characters only, which today's output satisfies; no requirement stated the layout. FMT-PIPE-020 now states it.
- Also observed (not in the issue): the same class with U+0085 or U+2028 after every line gives two blank lines between `#endregion // A` and `#region B` where the LF copy has one. FMT-PIPE-020 covers these terminators; this extends the issue's Done-when (lone `\r` only) toward the parent's goal, and is a point of the change doc for the maintainer to approve.
- `ControlFlowBraceTransform` writes `SyntaxFactory.ElasticCarriageReturnLineFeed` — still true. `scripts/trace.sh` on the LF `if (a)` / `// leading` / `Foo();` file shows `LineEndingNormalizationPhase` changing `CRLF=2, LF=8` to `LF=10`.
- `DocCommentElementNormalizer.GetLineBreak` copies the source line's break or falls back to `Environment.NewLine` — still true.
- `BlankLineTokenCleanupRewriter` initializes `endOfLineText` to `Environment.NewLine` in two methods — still true.
- `ReihitsuFormatterHelpers.ComputeDocumentColumn` counts an own-leading-whitespace tab as four columns — still true.
- `PhaseOwnershipDocumentationTests` expects 12 observed phases — still true; after the change the count stays 12 (two sub-phases plus ten phases).
- "Every entry point runs the PreparationPhase, the node-level ones included" — already true in structure: all five entry points call `FormattingPipeline.Execute`, which runs the full phase list.
- "The LineEndings sub-phase returns its input node unchanged when it has nothing to rewrite" — already true of `LineEndingNormalizationPhase` (it returns `root` when it finds no newline token and no end-of-line trivia to replace). Dropped as work; kept as a constraint in the change doc.
- Observer names today are `phase.GetType().Name` in `FormattingPipeline.ExecutePhases`; nothing reports sub-phases yet.

## Non-goals

- No change to which line breaks FMT-EOL rewrites: line breaks inside multi-line comments and disabled text stay outside (thoenissen/Reihitsu#959), literals stay outside (FMT-EOL-004).
- No change to line-ending detection (FMT-PIPE-007) or to the tab rules (FMT-TAB-001 to FMT-TAB-003).
- FMT-PIPE-020 does not cover the node-level entry points; detaching node-level roots is thoenissen/Reihitsu#1489.
- No change to the columns of FMT-PIPE-017 or to the byte-order-mark handling.

## Open questions for the maintainer

_None._

# A preparation phase replaces tabs and normalizes line endings first

| | |
|---|---|
| Status | Draft |
| Date | 2026-10 |
| Affected requirements | FMT-PIPE-001, FMT-PIPE-015, FMT-PIPE-016 (retired), FMT-PIPE-017, FMT-PIPE-019 (new), FMT-PIPE-020 (new), FMT-TAB-001, FMT-EOL-001, FMT-EOL-003 |
| Depends on | [Phase prediction removal](phase-prediction-removal.md) |

## What

The formatter's first phase becomes a `PreparationPhase` with two sub-phases, run in this order: TabReplacement (FMT-TAB, unchanged rules) and LineEndings (FMT-EOL, unchanged rules). The line-ending normalization no longer runs as the last phase; it runs second, before every phase that decides layout.

One pipeline invariant, FMT-PIPE-019, replaces FMT-PIPE-016: after the preparation phase, no phase receives or returns a tab in the whitespace of the code or between directive tokens, nor a line break other than the detected line ending (FMT-PIPE-007) — between tokens, at the end of a directive, or inside a documentation comment. Text outside FMT-TAB and FMT-EOL keeps what it holds: comments, literals, disabled text, and a directive's free text.

A new requirement, FMT-PIPE-020, states the user-visible consequence: a file gets the same output as its copy whose line breaks are already the detected line ending. A file saved with a lone `\r` after every line gets the layout of its LF copy.

## Why

Every phase before the last one sees the input's line breaks, and some write the detected line ending next to them. In a file whose line breaks are a lone `\r`, FMT-PIPE-007 detects `\n`; when a phase writes `\n` behind an input `\r`, the text holds one line break (`\r\n`) where the tree holds two, and every later phase that numbers lines through the text works on wrong lines. The issue's example — a class with two regions saved with lone `\r` — comes out with `#region A` in column 0, and formatting that output again moves it to column 4, so today the formatter also breaks FMT-PIPE-004 for such a file. Saved with U+0085 or U+2028 after every line, the same class comes out with two blank lines between the regions where the LF copy has one.

The final normalization also hides foreign line endings that phases write: `ControlFlowBraceTransform` writes `\r\n` into an LF file when it wraps a statement with a leading comment in braces, and the documentation-comment phase falls back to `Environment.NewLine`, as does the blank-line phase. The last phase repairs the characters, so no test sees them, while every phase between the writer and the end works on a text whose line breaks differ from the file's.

Normalizing first gives every later phase a text with one tab-free whitespace form and one line-ending form, which is the same rule FMT-PIPE-016 already applies to tabs. A phase may then rely on it, and a phase that writes something else is a defect that a per-phase check finds, instead of a difference the end of the pipeline repairs.

## Decisions

### The phase table

FMT-PIPE-001 lists the preparation phase first, with its two sub-phases as steps 1.1 and 1.2, followed by the ten phases from `StructuralTransformPhase` to `CleanupPhase`. `LineEndingNormalizationPhase` no longer appears at the end. The "Why" of FMT-PIPE-001 no longer says line endings are normalized last; it says tabs and line endings are normalized before any other phase (FMT-PIPE-019).

### Sub-phases are phases of their own

Each sub-phase is a phase in the sense of FMT-PIPE-001: it receives the output of the step before it and is reported on its own. The pipeline observer and the trace (`scripts/trace.sh`) report the sub-phases as `PreparationPhase/TabReplacement` and `PreparationPhase/LineEndings`; the preparation phase itself is not reported as a separate step, so every reported step is one that can change text, and the trace shows each of them. Every other phase keeps the name it is reported under today. A sub-phase run alone through `FormattingPipeline.ExecutePhases` is reported under the same name; `PreparationPhase` run through it reports its two sub-phases.

The types keep their names: `TabReplacementPhase` and `LineEndingNormalizationPhase` stay the code owners of FMT-TAB and FMT-EOL, and the new `PreparationPhase` becomes a code owner of the pipeline spec, which owns the composition and its invariant.

### FMT-PIPE-019 replaces FMT-PIPE-016

FMT-PIPE-016 is retired with a pointer to FMT-PIPE-019. FMT-PIPE-019 keeps FMT-PIPE-016's scope for tabs unchanged, its one exception included: the whitespace in front of the `///` of a documentation comment's continuation line keeps its tab until the indentation phase rewrites it (FMT-INDENT-008). For line endings, its scope is exactly what FMT-EOL rewrites: end-of-line trivia, the line break that ends a directive, and the newlines of a documentation comment. The line breaks inside literals, multi-line comments, and disabled text stay outside it (FMT-EOL-004 and the FMT-EOL non-goal, thoenissen/Reihitsu#959).

It is one requirement for both, because it is one decision with one reason: every later phase receives the input in the one form the preparation phase establishes, and no phase reintroduces another.

### Layout does not depend on the input's line ending

FMT-PIPE-020 is added so that the outcome the issue asks for — the lone-`\r` file gets the LF layout — is a requirement a regression test can cover, rather than a consequence nobody states. It covers the document- and tree-level entry points and every line terminator FMT-EOL-001 lists. The node-level entry points keep the rest of the document as it stands (FMT-PIPE-010), so for them the requirement would compare a target with a document it cannot make equal; they are left to the restructuring of the node-level entry points.

### FMT-EOL and FMT-TAB

The FMT-EOL scope sentence says the phase is the second sub-phase of the preparation phase instead of the last phase. FMT-EOL-001's "Why" no longer says the phase catches a line break an earlier phase copied; it says the phase runs before every other phase, so no phase sees another line ending (FMT-PIPE-019). FMT-EOL-003's "Why" no longer says the documentation-comment phase relies on this phase to normalize what it keeps; the separators it keeps are already the detected line ending when it runs. The rules of FMT-EOL-001 to FMT-EOL-004 do not change.

FMT-TAB's scope sentence calls the phase the first sub-phase of the preparation phase, and FMT-TAB-001's "Why" points at FMT-PIPE-019 instead of FMT-PIPE-016. Its rules do not change.

### FMT-PIPE-015

The allowance to "rewrite the characters of a line ending (FMT-EOL-001)" after the indentation phase is dropped: no phase after it rewrites line endings any more. What the phases after it may still change — trailing whitespace and line breaks at the end of the file — is unchanged.

### FMT-PIPE-017

The column of a node-level target's first token is defined through the preparation phase: a tab in whitespace that belongs to the node and that the entry point does not keep counts with the width the preparation phase gives it (FMT-PIPE-019), instead of restating "four columns" from FMT-TAB-001. The resulting columns do not change.

### Documentation of the phase order

In the `AGENTS.md` ownership table the preparation phase has one row per sub-phase, named by its reported name (`PreparationPhase/TabReplacement`, `PreparationPhase/LineEndings`), as its first two rows; the `LineEndingNormalizationPhase` row at the end goes away. `PhaseOwnershipDocumentationTests` counts reported phases, so it still counts twelve: two sub-phases and ten phases. The architecture paragraph of `AGENTS.md` and the trace description in `scripts/README.md` name the new order.

### What stays as it is

- Every entry point runs the preparation phase, the node-level ones included, because every entry point runs the whole pipeline (already true today).
- Until node-level roots are detached at the entry point, the LineEndings sub-phase returns its input node itself when it has nothing to rewrite, as TabReplacement does (already true of `LineEndingNormalizationPhase` today), so a node-level root stays attached to its document exactly as long as it does today.
- A file whose line breaks are all LF or all CRLF keeps its output, and so does a byte order mark at the start of the file.

## Alternatives considered

- **Normalize first and again last** — rejected: a final pass that silently repairs a foreign line ending hides exactly the defect FMT-PIPE-019 forbids.
- **Normalize only last, and require that no phase writes a line ending that can fuse with an input line ending** — rejected: every phase would still see the input's line breaks, so each phase that numbers lines or compares line breaks would have to handle every terminator itself, and the lone-`\r` file would keep depending on every writer getting that rule right.
- **Report `PreparationPhase` as one step to the observer and the trace** — rejected: the trace would show one diff for two independent rewrites, and a line-ending defect could not be told apart from a tab defect.

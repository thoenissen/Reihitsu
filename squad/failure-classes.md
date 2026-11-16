# Failure classes

Known ways a change in this repository goes wrong. The skeptic names the classes that can reach a change, the test implementer covers them, and the auditor checks every one. IDs are immutable; a class that no longer applies is marked retired, not removed. A new class is added only by a pull request that names the failure it prevents, and its **Origin** says where it was learned; it never cites a run note, which is temporary ([`notes/README.md`](notes/README.md)). Where the members of a class are known, a [matrix](#class-matrices) lists them: the skeptic and the auditor enumerate a defect's class from it and report all failing members together ([`README.md`](README.md#architecture-findings)).

**Origin** says where the class was learned: *RC reviews* — the 1.0-RC review rounds, which traced nearly every critical finding to the trivia, semantics, convergence, idempotency, parity, and closure classes; *audits* — findings of the former preflight audit and Behavior Contract gate; *reproduction* — the former reproduction gate; *precheck* — the change that introduced the formatter's Unsupported outcome ([`unsupported-precheck.md`](../sdd/changes/2026-11/unsupported-precheck.md)).

## Guards and predicates

| ID | Failure class | Origin |
|---|---|---|
| FC-001 | A guard narrowed to fix one symptom inspects a smaller span than the decision it protects; the region it no longer covers still decides something wider | audits |
| FC-002 | A qualifier of the requirement ("terminal", "direct", "inside", "trailing") never became an explicit condition of the predicate | audits |
| FC-003 | A changed condition is tested on one side of its boundary only — "no trivia" and "trivia on one line" are two tests | audits |
| FC-015 | A policy has more than one owner: a private copy next to the shared helper (usually in `Reihitsu.Core`) drifts from it | RC reviews |
| FC-016 | A fix closes the reported instance while sibling shapes or other private copies keep the same hazard | RC reviews |
| FC-044 | A guard models a result that the compiler, the Renamer, or another build configuration computes — which members a rename rebinds or which interface member it implements, which elements a documentation expansion binds, which code an `#if` group excludes, which column a later phase writes — so every audit finds the next input the model misses; reading the computed result or refusing the class by a cheap check on its cause ends it (`README.md`, *Architecture findings*) | audits |

## Trivia and directives

| ID | Failure class | Origin |
|---|---|---|
| FC-004 | An edit that joins lines, collapses a gap, or moves, reorders, or deletes tokens or members drops or relocates a comment, doc comment, directive, or disabled text in that gap, instead of keeping it or refusing | RC reviews |
| FC-005 | A guard that covers comments is taken to cover directives too | RC reviews |
| FC-019 | A rule touching token gaps or line joins is tested without a comment and without a directive in the gap | RC reviews |
| FC-020 | `#if`/`#else`/`#elif`/`#endif`, `#pragma`, `#region` in a gap the edit touches — between modifiers, parameters, members, accessors, attribute lists, base-list entries — including fully inactive `#if false` blocks | RC reviews |
| FC-021 | An end-of-line comment on a line being joined, a comment between modifiers, a comment on the header line of a rewritten construct, a `/* */` ending mid-line, a comment before the opening or the closing delimiter | RC reviews |

## Rewrites and semantics

| ID | Failure class | Origin |
|---|---|---|
| FC-006 | A rewrite does not compile or changes meaning: target-typed expressions (`FormattableString`, overload selection), user-defined operators, literal contents, language-version gates, `partial` types, explicit interface implementations, `file` types, modifier order | RC reviews |
| FC-022 | Raw and interpolated raw string literals, verbatim strings, or comment-looking text inside a literal | RC reviews |
| FC-023 | Verbatim identifiers (`@class`), digit-only, `_`, or Unicode identifiers | RC reviews |
| FC-024 | `partial` types and members, explicit interface implementations, `file` types, static interface members | RC reviews |
| FC-025 | One of the seven expression-bodied member kinds (method, property, indexer, constructor, finalizer, operator, local function), with or without `async`, `Task<T>`, `ValueTask<T>` | RC reviews |
| FC-026 | Several identical tokens on one line, so a token is re-resolved to the wrong one after an earlier edit; nested and `else if` chains for brace edits | RC reviews |
| FC-027 | Only the single-line form, a plain sibling, or one nesting level was considered: the multi-line form, a complex sibling (lambda, query, nested initializer), nested syntax of the same kind, or another enclosing scope (statement, equals-value clause, arrow expression, argument, attribute, initializer) behaves differently | audits |
| FC-028 | Detached-node formatting (`FormatNode`) and document-scoped formatting (`FormatNodeInDocumentAsync`) disagree | audits |
| FC-029 | Paired key/value layout is tested on some of its four combinations of single-line and multi-line key and value | audits |
| FC-036 | A rename is treated as behavior-preserving, although it reaches `nameof`, reflection, serialization, source generators, public API, and named arguments | audits |

## Code fixes

| ID | Failure class | Origin |
|---|---|---|
| FC-007 | A code fix needs more than one pass to silence its own diagnostic, or raises another RH diagnostic in the edited span | RC reviews |
| FC-008 | Fix All misbehaves on overlapping spans or on positions that went stale after the first application | RC reviews |
| FC-009 | A registered code action cannot change the document, yet is offered | RC reviews |
| FC-018 | A naming fix renames the declaration without retargeting its references through the Renamer | RC reviews |
| FC-030 | A code fix formats more than the node it fixes, dragging unrelated edits and inherited formatter defects into the fix | RC reviews |

## Formatter stability

| ID | Failure class | Origin |
|---|---|---|
| FC-010 | A phase run on its own output is not a no-op, often only on CRLF input | RC reviews |
| FC-011 | A loop-until-stable has no decreasing measure, or a guard that refuses an edit still reports "changed" upstream — a hang | RC reviews |
| FC-012 | Two neighboring phases undo each other (insert ↔ collapse) | RC reviews |
| FC-013 | Analyzer and formatter disagree: formatter output is flagged by a rule that [SHARED-DIAG-010](../sdd/specs/shared/diagnostics.md) (P31) covers — a layout rule whose code fix lays code out as the formatter does. An order or content rule may report what the formatter keeps, and analyzer-clean code that the formatter changes is not this class where [P11](../sdd/README.md#the-formatter-may-do-more-than-the-analyzers-p11) calls the rewrite intended | RC reviews |
| FC-014 | A new rule's concern is of a kind the formatter judges — layout, or an order or a syntax form that a formatter phase rewrites, such as using order, expression bodies, trailing commas, braces of empty types, or field splitting — but no formatter phase owns it for the rule's construct, leaving permanent diagnostics for CLI users. Order or content that the formatter does not judge, such as the order of members or the content of comments, is not this class: the formatter keeps it without judging it, so [SHARED-DIAG-010](../sdd/specs/shared/diagnostics.md) lets a rule about it report what the formatter keeps | RC reviews |

## Unsupported shapes

These classes reach a change that declares a shape unsupported, or changes the precheck's check of one ([`unsupported-syntax.md`](../sdd/specs/formatter/unsupported-syntax.md)).

| ID | Failure class | Origin |
|---|---|---|
| FC-038 | A phase produces a declared shape — for example, a split, join, move, or relocation carries a comment into the span the shape covers — so the formatter refuses its own output on the next pass (FMT-UNSUP-005) | precheck |
| FC-039 | The check stops descending at the outer match or excepted region, so a nested occurrence — a declaration inside an initializer value or a lambda body — is missed, or the outer node's exception leaks inward to it; the precheck's case of FC-027 | precheck |
| FC-040 | The outcome depends on the documentation mode: the parser attaches `///` or `/** */` text to another token, or parses it as structured trivia, so the CLI and a code fix answer differently for the same text | precheck |
| FC-041 | The check is not local to its input: it reads text outside a node-level input, so a target that holds only part of the shape is refused, or the same node, as the precheck's input, gets a different answer as a target than as a detached node or a document root (FMT-UNSUP-003); the precheck's case of FC-028 | precheck |
| FC-042 | A guard that only serves a declared shape survives in a phase, a code fix, or a spec clause, so the shape is handled twice and the guard drifts from the precheck; the precheck's case of FC-015 | precheck |
| FC-043 | A code fix asks `FindUnsupportedSyntax` about another node than the formatter receives from it — the target instead of the context, or the replaced code instead of the node it builds — so it is offered and then changes nothing, or is withheld where the formatter would format (SHARED-FIX-010); the precheck's case of FC-009 | precheck |

## Tests and evidence

| ID | Failure class | Origin |
|---|---|---|
| FC-017 | An assertion is weaker than the guarantee: balanced token counts for exact output, one pass for convergence, LF only for line-ending stability, a single formatter pass for idempotency | audits |
| FC-031 | A changed method keeps an XML summary or inline comment that describes its previous behavior — the most common last finding once everything else is right | audits |
| FC-032 | A project result gathered before a compiled change is taken as still valid; a formatter change can flip analyzer results because the code fixes delegate to the formatter | audits |
| FC-033 | A reproduction stays green because the harness normalized the scenario away — line endings, surface, code-fix path, or the trivia the example omitted | reproduction |
| FC-034 | A regression test reproduces a simplified cousin of the reported shape instead of the shape itself | RC reviews |
| FC-035 | Expected test values were taken from a patched copy of the code, so the test cannot falsify the implementation that produced them | audits |
| FC-037 | A comment-only change is "proven" by a line-based `grep`, which cannot tell a comment from a directive or from comment-looking text inside a literal; only `scripts/verify-text-only.sh` proves it | audits |

## Class matrices

A matrix spans the neighboring shapes of a known class along its axes; every combination of one member per axis is a cell. A defect found in one cell is checked in every cell, and every failing cell is reported in the same finding. A cell holds when, under LF and under CRLF alike, the output meets the requirement the decision serves and a second application — a second formatting pass, a second code-fix iteration — leaves it unchanged. Stability alone does not show it: a cell can be stable and wrong. Each matrix names the requirement its cells are checked against. Each matrix names the components that share the decision, because a class whose members fail between two components can be an architecture finding; whether it is one of the run is decided by [`README.md`](README.md#architecture-findings), *Architecture findings*.

### Tab width across formatter phases

Classes: FC-012, FC-015, FC-016. Learned in the run that aligned interpolation holes of multi-line raw strings, where three consecutive audits each found failing cells that the audit before had not enumerated; [No formatter phase predicts a later phase's columns](../sdd/changes/2026-10/phase-prediction-removal.md) then removed the predictions those cells failed between.

**Decision.** The column of a token behind a tab, and of every line aligned to it. The preparation phase replaces every tab in the whitespace of the code and between directive tokens with four spaces before any other phase runs (`PreparationPhase/TabReplacement`, FMT-TAB-001, FMT-TAB-002), so no later phase sees a tab there or counts its width (FMT-PIPE-019), and `IndentationPhase` places every line in spaces, the content, hole, and closing lines of a multi-line raw string included (FMT-RAW-001, FMT-RAW-007). A tab still reaches a column in two places, and a component that counts it differently from what the output holds is the defect: a tab inside a comment, a literal, or disabled text is kept (FMT-TAB-003) and counts as one column wherever it stands in front of a token; and the node-level entry points keep the whitespace in front of their target as it stands, counting a tab there as one column, while a tab inside the target becomes four spaces (FMT-PIPE-017, FMT-PIPE-018). Wherever two components count the same tab differently, a token ends up three columns away from where another component measured it, and the next pass or the next fix iteration moves it (FMT-PIPE-004, SHARED-FIX-007). Wherever a move is clamped at a tab, the line moves less than it must: the token is off by the clamped amount, and the output can be stable and still wrong.

**Check.** The cells hold as [Class matrices](#class-matrices) says, checked against these requirements: after one pass, every token of the line stands at the column its requirement gives it — FMT-RAW-007 on a hole line, FMT-INDENT-029 for a chain link, the alignment requirement of any other anchored line — measured on the output, after the tab replacement; and through a node-level entry point, the target's first token keeps its column in the document and every other line of the target follows it as FMT-PIPE-017 states.

| Axis | Members |
|---|---|
| Entry point | a whole document; a node-level target with the tab inside it; a node-level target with the tab in the whitespace in front of it |
| Shift of the literal | right; none (aligned literal); left by fewer than the spaces in front of the tab; left by exactly those spaces; left by more than those spaces |
| What the tab stands in front of | the line's first token; a comment in front of the token on the same line; the opening quotes of a nested raw string; a `/** */` comment in the middle of a line, ahead of a chain link or of the opening quotes of a raw string (`var x = a→/** c */.B()` / `.C();`) |
| Anchoring | none; another line aligned to the line's column — a comment line above it that follows the token below, a wrapped argument, the next link of a chain, or the content and closing quotes of the nested raw string |

The failing cells, in the order they were found. The first four failed between `CleanupPhase`, which then replaced tabs last, and `IndentationPhase` and `RawStringAlignmentPhase`, which predicted that replacement; the last was found when those predictions were removed:

| Found by | Entry point | Shift | Tab in front of | Anchoring | What went wrong |
|---|---|---|---|---|---|
| first audit | whole document | none | the first token | comment line above; wrapped argument | A hole line that no construct places kept its tab, and its column was registered with the tab counted as one |
| repair round 1 | whole document | none | a comment | wrapped argument | The tab count stopped at the first non-whitespace character, while the cleanup phase replaced tabs in every whitespace trivia in front of the token |
| repair round 1 | whole document | none; right | the opening quotes of a nested raw string | content and closing quotes | The raw-string phase measured the opening quotes with the tab counted as one, the indentation phase with the tab counted as four |
| repair round 2 | whole document | left by more than the spaces in front of the tab | the first token | none; wrapped argument | The raw-string phase moved the line by rewriting only the spaces in front of the tab, so a leftward move was clamped at the tab |
| phase-prediction removal | whole document | none | a `/** */` comment in the middle of a line, ahead of a chain link or of a raw string's opening quotes | the next link of the chain | `.C()` in `var x = a→/** c */.B()` / `.C();` stood in column 26 after one pass and in 29 after the second, and `.ToUpper` behind a raw string in `var s = x→/** d */.Foo(` in 34, then 37: two phases counted the tab in front of the comment differently. `TabReplacementTests` pins the one-pass columns, the chain link also through `FormatNode` and `FormatNodeInDocumentAsync` |

Every tab test before the last repair of the raw-string run shifted right or not at all. That repair pinned the leftward boundary, a shift of exactly the spaces in front of the tab, next to the shift beyond it (FC-003). The cell with no anchor in the fourth row was stable but wrong, which a stability check alone passes.

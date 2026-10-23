# Failure classes

Known ways a change in this repository goes wrong. The skeptic names the classes that can reach a change, the test implementer covers them, and the auditor checks every one. IDs are immutable; a class that no longer applies is marked retired, not removed. A new class is added only together with a `squad/decisions/` entry naming the failure it prevents.

**Origin** says where the class was learned: *RC reviews* — the 1.0-RC review rounds, which traced nearly every critical finding to the trivia, semantics, convergence, idempotency, parity, and closure classes; *audits* — findings of the former preflight audit and Behavior Contract gate; *reproduction* — the former reproduction gate.

## Guards and predicates

| ID | Failure class | Origin |
|---|---|---|
| FC-001 | A guard narrowed to fix one symptom inspects a smaller span than the decision it protects; the region it no longer covers still decides something wider | audits |
| FC-002 | A qualifier of the requirement ("terminal", "direct", "inside", "trailing") never became an explicit condition of the predicate | audits |
| FC-003 | A changed condition is tested on one side of its boundary only — "no trivia" and "trivia on one line" are two tests | audits |
| FC-015 | A policy has more than one owner: a private copy next to the shared helper (usually in `Reihitsu.Core`) drifts from it | RC reviews |
| FC-016 | A fix closes the reported instance while sibling shapes or other private copies keep the same hazard | RC reviews |

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
| FC-014 | A new layout analyzer — one whose subject is line breaks, blank lines, indentation, or spacing — has a concern that no formatter phase owns, leaving permanent diagnostics for CLI users. A new order or content rule is not this class: the formatter does not judge order or content, so [SHARED-DIAG-010](../sdd/specs/shared/diagnostics.md) lets such a rule report what the formatter keeps | RC reviews |

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

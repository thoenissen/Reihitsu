# Beta Code Review — Criteria Catalog

Review scope: `Reihitsu.Core`, `Reihitsu.Cli`, `Reihitsu.Formatter`, `Reihitsu.Analyzer`, `Reihitsu.Analyzer.CodeFixes` (704 files). Test projects excluded. The review is read-only; findings go to `findings.md`, coverage is tracked in `coverage.md`.

Finding format: `- [Critical|Major|Minor] path:line — problem. Suggested direction for fix.`

Severity scale:

- **Critical** — produces wrong/corrupted code output, crash, data loss, broken public contract.
- **Major** — edge-case bug, false positive/negative in an analyzer, incomplete code fix, SRP/layering violation, significant performance problem.
- **Minor** — naming, dead code, stale XML docs/comments, micro-performance, inconsistency.

No positive feedback: a file without findings is fine and gets no entry.

## A. General (applies to every area)

1. **Correctness** — logic errors, off-by-one, null/empty handling; wrong assumptions about C# syntax shapes (records, primary constructors, file-scoped namespaces, raw strings, tuples, local functions, expression bodies, preprocessor directives).
2. **Roslyn usage** — trivia preservation on rewrites; immutability misuse (`WithX` result discarded); stale node references after tree replacement; `Span` vs `FullSpan` confusion; annotation handling.
3. **CancellationToken** — accepted, propagated, and observed in loops (precedent: BlankLineCollapser fix #221).
4. **SRP / layering** — one class, one topic; a phase/rule doing another phase's job (cross-phase topic bleed); logic living in the wrong assembly (Core vs. Formatter vs. Analyzer).
5. **Duplication** — copy-paste logic across rules/phases that belongs in Core or a base class.
6. **Dead code** — unused members, unreachable branches, leftover parameters.
7. **API surface** — unnecessarily `public` types/members (precedent: Formatter surface lock #224).
8. **Performance** — repeated full-tree traversals, LINQ allocations in hot paths, regex/array allocations per call instead of static, missing early-outs.
9. **Error handling** — swallowed exceptions, wrong exception types, silent failure paths.
10. **Readability (Minor)** — misleading names, comments contradicting code.

## B. Formatter (`Reihitsu.Formatter`)

1. **Idempotency** — every phase stable on a second run; no two phases that oscillate (one produces what another reverses).
2. **Phase-order assumptions** — implicit dependencies on earlier phases valid and documented; phase robust when input is not yet normalized.
3. **Trivia fidelity** — comments, `#region`/`#endregion`, `#if`/`#else`/`#endif`, `#pragma` preserved and re-attached to the right token at rewrite boundaries; leading/trailing trivia ownership at edit edges.
4. **Line-ending handling** — mixed CRLF/LF input, final newline behavior.
5. **Guard behavior** — syntax-invalid and auto-generated code returned unchanged (contract from CLAUDE.md); guards present on every entry point, not just one.
6. **Scope discipline** — rewriters touch only nodes they claim to handle; no over-rewriting; `FormatNode` vs. `FormatNodeInDocumentAsync` semantics consistent.
7. **Performance** — no `ToFullString`/re-parse round-trips mid-pipeline where a tree edit would do; single pass where possible.

## C. Analyzer (`Reihitsu.Analyzer`)

1. **Registration correctness** — right action kind (SyntaxNode vs. Symbol vs. SyntaxTree vs. Operation); `EnableConcurrentExecution`; `ConfigureGeneratedCodeAnalysis` appropriate per rule.
2. **Statelessness** — no mutable shared state in analyzer instances (instances are reused concurrently).
3. **Diagnostic location precision** — span on the offending token/identifier, not the whole node (IDE squiggle quality).
4. **False positives** — edge shapes: partial types, nested types, operators, lambdas, local functions, records, generated-style code.
5. **False negatives** — rule covers all syntax forms its title claims (or the omission is clearly deliberate).
6. **ID/category consistency** — RH#### numeric range matches the category folder; default severity sensible; suffix-letter convention respected.
7. **Resources** — title/message/description present in all resource languages; message-format placeholder count matches arguments.
8. **Doc contract** — `documentation/rules/RH####.md` exists and matches behavior (helpLinkUri contract).
9. **Performance** — no semantic-model use where syntax suffices; no per-node allocations that could be hoisted.

## D. CodeFixes (`Reihitsu.Analyzer.CodeFixes`)

1. **Semantic preservation** — the fix never changes program behavior; comments/trivia on fixed nodes don't vanish.
2. **Comprehensiveness** — fix handles every shape the analyzer flags (repo contract: comprehensive fix or no fix at all).
3. **FixAll support** — batch fixer correct; equivalence keys stable and distinct.
4. **Formatting delegation** — final layout via `ReihitsuFormatter.FormatNodeInDocumentAsync`/`FormatNode`, no manual trivia surgery (repo convention).
5. **Robustness** — no throw on partial/incomplete code; cancellation respected; no unnecessary solution-wide operations.

## E. Core (`Reihitsu.Core`)

1. **Layering purity** — no formatter-/analyzer-specific knowledge; dependencies point only downward.
2. **Utility correctness** — edge cases: empty/single-char/unicode/digit input for casing utilities; ordering comparers total, stable, and consistent with each other.
3. **API generality** — shapes reusable, not tailored to a single caller; correct visibility.

## F. CLI (`Reihitsu.Cli`)

1. **Argument parsing** — unknown options, missing option values, duplicates, relative vs. absolute paths.
2. **Exit codes** — distinct and consistent for success / `--check` with differences / errors.
3. **File handling** — encoding and BOM preserved on write; read-only files; nonexistent paths; recursion and skip rules (`bin\`, `obj\`, `.Designer.cs`, `.g.cs`, `.g.i.cs`) exactly as documented.
4. **Mode contracts** — `--check` and `--dry-run` strictly side-effect-free.
5. **Abstraction discipline** — file-system/console access through the `Abstractions` layer everywhere, no direct `File.*` bypass.
6. **Error UX** — errors don't silently abort the whole run; messages actionable; diff output correct.

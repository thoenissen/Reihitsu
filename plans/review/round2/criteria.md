# 1.0.0 RC Code Review (Round 2) — Criteria Catalog

Review scope: `Reihitsu.Core`, `Reihitsu.Cli`, `Reihitsu.Formatter`, `Reihitsu.Analyzer`, `Reihitsu.Analyzer.CodeFixes` (737 files at planning time; session 1 refreshes the inventory). Test projects excluded. The review is read-only; findings go to `findings.md`, coverage is tracked in `coverage.md`.

This is the second full review. Round 1 (documents in `plans/review/round1/`) reviewed the codebase as of commit `cada3ee` (2026-06-10), produced 164 findings, and all of them were fixed through issues #225–#262 and the follow-up wave shipped in v1.0.0-beta2 through v1.0.0-beta4. Round 2 reviews the post-fix codebase as the final quality gate before 1.0.0-rc1. Roughly 390 of the 737 files changed substantively since round 1 (marked `*` in `coverage.md`): the fix wave itself, consolidations into `Reihitsu.Core` (seven new utility classes), five new code-fix base classes, and the rules added since (RH5031/RH5032, RH5113, RH7308, RH7409–RH7412, RH8306 rescope, RH8309).

Finding format: `- [Critical|Major|Minor] path:line — problem. Suggested direction for fix.`

Severity scale:

- **Critical** — produces wrong/corrupted code output, crash, data loss, broken public contract.
- **Major** — edge-case bug, false positive/negative in an analyzer, incomplete code fix, SRP/layering violation, significant performance problem.
- **Minor** — naming, dead code, stale XML docs/comments, micro-performance, inconsistency.

No positive feedback: a file without findings is fine and gets no entry.

## A. General (applies to every area)

1. **Correctness** — logic errors, off-by-one, null/empty handling; wrong assumptions about C# syntax shapes (records, primary constructors, file-scoped namespaces, raw strings, tuples, local functions, expression bodies, preprocessor directives).
2. **Roslyn usage** — trivia preservation on rewrites; immutability misuse (`WithX` result discarded); stale node references after tree replacement; `Span` vs `FullSpan` confusion; annotation handling.
3. **CancellationToken** — accepted, propagated, and observed in loops.
4. **SRP / layering** — one class, one topic; a phase/rule doing another phase's job; logic living in the wrong assembly (Core vs. Formatter vs. Analyzer).
5. **Duplication** — copy-paste logic across rules/phases that belongs in Core or a base class. Round 1 consolidated the known families; watch for new copies that appeared during the fix wave.
6. **Dead code** — unused members, unreachable branches, leftover parameters; in particular helpers orphaned by the round-1 consolidations.
7. **API surface** — unnecessarily `public` types/members (precedent: Formatter surface lock #224; Core has grown since — check it kept a deliberate surface).
8. **Performance** — repeated full-tree traversals, LINQ allocations in hot paths, regex/array allocations per call instead of static, missing early-outs.
9. **Error handling** — swallowed exceptions, wrong exception types, silent failure paths.
10. **Readability (Minor)** — misleading names, comments contradicting code.

## B. Formatter (`Reihitsu.Formatter`)

1. **Idempotency** — every phase stable on a second run; no two phases that oscillate (one produces what another reverses).
2. **Phase-order assumptions** — implicit dependencies on earlier phases valid and documented; phase robust when input is not yet normalized.
3. **Trivia fidelity** — comments, `#region`/`#endregion`, `#if`/`#else`/`#endif`, `#pragma` preserved and re-attached to the right token at rewrite boundaries; leading/trailing trivia ownership at edit edges.
4. **Line-ending handling** — mixed CRLF/LF input, final newline behavior; the CRLF regressions fixed in beta3/beta4 (#328, #361) must stay covered by the phase logic, not just by tests.
5. **Guard behavior** — syntax-invalid and auto-generated code returned unchanged; guards present on every entry point, not just one.
6. **Scope discipline** — rewriters touch only nodes they claim to handle; no over-rewriting; `FormatNode` vs. `FormatNodeInDocumentAsync` semantics consistent.
7. **Performance** — no `ToFullString`/re-parse round-trips mid-pipeline where a tree edit would do; single pass where possible.

## C. Analyzer (`Reihitsu.Analyzer`)

1. **Registration correctness** — right action kind (SyntaxNode vs. Symbol vs. SyntaxTree vs. Operation); `EnableConcurrentExecution`; `ConfigureGeneratedCodeAnalysis` appropriate per rule.
2. **Statelessness** — no mutable shared state in analyzer instances (instances are reused concurrently).
3. **Diagnostic location precision** — span on the offending token/identifier, not the whole node.
4. **False positives** — edge shapes: partial types, nested types, operators, lambdas, local functions, records, generated-style code.
5. **False negatives** — rule covers all syntax forms its title claims (or the omission is clearly deliberate).
6. **ID/category consistency** — RH#### numeric range matches the category folder; default severity sensible; suffix-letter convention respected.
7. **Resources** — title/message/description present in all resource languages; message-format placeholder count matches arguments.
8. **Doc contract** — `documentation/rules/RH####.md` exists and matches behavior (helpLinkUri contract), including the rules added since round 1.
9. **Performance** — no semantic-model use where syntax suffices; no per-node allocations that could be hoisted.

## D. CodeFixes (`Reihitsu.Analyzer.CodeFixes`)

1. **Semantic preservation** — the fix never changes program behavior; comments/trivia on fixed nodes don't vanish.
2. **Comprehensiveness** — fix handles every shape the analyzer flags (repo contract: comprehensive fix or no fix at all).
3. **FixAll support** — batch fixer correct; equivalence keys stable and distinct.
4. **Formatting delegation** — final layout via `ReihitsuFormatter.FormatNodeInDocumentAsync`/`FormatNode`, no manual trivia surgery.
5. **Robustness** — no throw on partial/incomplete code; cancellation respected; no unnecessary solution-wide operations.
6. **New base classes** — the five bases added by the fix wave (`CollapseTokenGapCodeFixProviderBase`, `CommentSafeSpanReplacementCodeFixProviderBase`, `RegionDirectiveBlankLineCodeFixProviderBase`, `RemoveWhitespaceRunCodeFixProviderBase`, `TrailingCommaRemovalCodeFixProviderBase`) carry the safety contract for every derived fix; review them as load-bearing infrastructure, and check every derived fix actually goes through them instead of bypassing.

## E. Core (`Reihitsu.Core`)

1. **Layering purity** — no formatter-/analyzer-specific knowledge; dependencies point only downward.
2. **Utility correctness** — edge cases: empty/single-char/unicode/digit input for casing utilities; ordering comparers total, stable, and consistent with each other.
3. **API generality** — shapes reusable, not tailored to a single caller; correct visibility.
4. **Consolidation integrity** — Core absorbed seven new utility classes during the fix wave (`DocumentationCommentUtilities`, `FormattingSafetyUtilities`, `LineEndingUtilities`, `RawStringLiteralUtilities`, `RegionDirectiveBlankLineUtilities`, `UnaryOperatorSpacingUtilities`, `XmlDocumentationElementOrderingUtilities`). Verify each consolidation is complete: no stale private copy of the same logic survives in Formatter/Analyzer/CodeFixes, and all callers agree on one policy.

## F. CLI (`Reihitsu.Cli`)

1. **Argument parsing** — unknown options, missing option values, duplicates, relative vs. absolute paths.
2. **Exit codes** — distinct and consistent for success / `--check` with differences / errors, including the empty-file-set contract fixed in #272.
3. **File handling** — encoding and BOM behavior on read and write (including the UTF-8 BOM enforcement from #305 and the non-UTF-8 skip from #274); read-only files; nonexistent paths; recursion and skip rules (`bin\`, `obj\`, `.Designer.cs`, `.g.cs`, `.g.i.cs`) exactly as documented.
4. **Mode contracts** — `--check` and `--dry-run` strictly side-effect-free.
5. **Abstraction discipline** — file-system/console access through the `Abstractions` layer everywhere, no direct `File.*` bypass.
6. **Error UX** — errors don't silently abort the whole run; messages actionable; diff output correct.

## G. Round-2 emphases (lessons from round 1)

These produced nearly all round-1 Criticals and are first-class checks in every session, on top of sections A–F:

1. **Fix-wave regression review** — every `*`-marked file changed after round 1 and its change has never been reviewed. Read it fully. For each round-1 fix, check the fix is *complete* (covers all call sites and syntax shapes, not just the reported one) and did not introduce an adjacent defect. Do not assume a guard is correct because it exists.
2. **Guard completeness at every call site** — the round-1 Critical families were fixed by adding guards (comment-join guards in `LineBreakTriviaUtilities`, casing guards, re-lex guards in spacing). Verify every join/collapse/glue/rename call site actually routes through the guard; a single unguarded path re-opens the corruption class.
3. **Fix convergence** — for every analyzer/fix pair: applying the fix must silence the diagnostic in one application, and must not raise a different RH diagnostic that a second fix would revert (ping-pong). Round 1 found fixes that no-op'd while the diagnostic persisted (RH7204) — check the pattern, especially where a fix delegates to the formatter.
4. **Analyzer/formatter policy parity** — for every layout-affecting rule: formatter output must be diagnostic-free for that rule, and code the analyzer accepts must be stable under the formatter. Divergence produces permanent diagnostics or oscillation. Round 1 found five divergent rule/phase pairs; the fix consolidated policies — verify none has crept back and the new rules (RH5031/32, RH5113, RH7308, RH7409–7412) were born aligned.
5. **Token-join and re-lex safety** — any edit that joins tokens or removes the gap between them must prove the result re-lexes to the same tokens (`- -x` vs `--x` family) and cannot absorb code into a preceding single-line comment. Check this for edits added since round 1, not only the repaired ones.
6. **Second-run idempotency reasoning** — for each formatter phase and each formatting-delegating fix, reason explicitly about the second run: does the output re-trigger the same phase or another one? Record any oscillation candidate as a finding even without a reproducing example.

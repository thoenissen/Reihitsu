# Phase 6 · Re-check of the 1.4.0 run plan after #1790 and the C# 15 PRs

Base: `claude/sdd-coordinator` at e59a2661 (`origin/main` merged, incl. #1779, #1782, #1783, #1784, #1787, #1790). Plan: `phase6-runs-1.4.0.md` (written at 19252d13). Milestone 1.4.0 (number 6): 24 open issues, read on GitHub with comments.

## Coverage

All 24 open issues sit in exactly one run: 1470 1471 1479 1544 (1.1) · 1505 1577 1576 (1.2) · 1507 1428 (1.3) · 867 (1.4) · 1672 (1.5) · 1728 1586 (2.1) · 898 1780 (2.2) · 1724 (2.4) · 1781 1786 (2.5) · 1726 (3.1) · 1587 (3.2) · 1516 (3.3) · 1732 (4.1) · 1759 (4.2) · 1788 (5.1). No open issue is missing; the only plan issues no longer open are the four #1790 closed (#1427, #1545, #1693, #1725).

## Runs

| Run | Command | Lane | Verdict | Reason |
|---|---|---|---|---|
| 1.1 | `/squad #1470 #1471 #1479 #1544` | tooling | adjust | #1427/#1725/#1545 closed by #1790. Every remaining premise is still valid on main: eight coverage statements remain (`AGENTS.md:14`, `CONTRIBUTING.md:22`, `squad/README.md:19`, `squad/roles/validator.md:19`, `squad/roles/implementer.md:30`, `.claude/agents/validator.md:3`, `scripts/test.sh:12`, `scripts/README.md:13/80`). `LaneClassifier` still leaves `CODE_OF_CONDUCT.md`/`SECURITY.md` unclassified. `failure-classes.md:96` still names `CleanupPhase`/FMT-CLEAN-002. The `BlankLinePhase` row still lacks `BraceBlankLinePolicy`, and that class exists. #1790 touched none of these lines. The run no longer touches `scripts/proof/verify-text-only.cs`, `scripts/verify-text-only.sh`, or the role templates' follow-up sections |
| 1.2 | `/squad #1505 #1577 #1576` | tests (→ tooling if the self-hosting `.csproj` gains a reference) | keep | No change to `Reihitsu.Formatter.Test/Helpers` or `Reihitsu.SelfHosting.Test` since the plan; the partial-done state of #1505 is unchanged |
| 1.3 | `/squad #1507 #1428` | tests | keep | `ArchitectureTests` unchanged. #1790 edited only the *Analyzer specs* sentence of `sdd/README.md`; #1428 edits *Tests and requirements* and *Checks* |
| 1.4 | `/squad #867` | tests | keep | No `WarningLevel` setting exists in `Reihitsu.Analyzer.Test` yet. #1784/#1787 added `UnionSupport.cs` and an `onConfigure` overload in `FormatterTestsBase{TAnalyzer}.cs`; the union tests are new fixtures the raised level will also compile |
| 1.5 | `/squad #1672` | bug-fix | keep | `PrecheckPhase.cs` is unchanged since the plan (`FindCommentInDeclaration` at :239) |
| 2.1 | `/squad #1728 #1586` | behavior-change | keep | `CarriesDirective` is still there (`FieldDeclarationSplitTransform.cs:75`). #1782 added the union parent and `VisitUnionDeclaration` to the transform and to `ReihitsuFormatter.SplitsFieldDeclaration`; only line numbers moved, and the union case is one more parent to test |
| 2.2 | `/squad #898 #1780` | tooling | adjust (premise detail) | `trace.sh` is still `100644`. Eleven `.csproj` now pin Roslyn 5.12.0 (#1779), while `verify-text-only.cs` still pins `Microsoft.CodeAnalysis.CSharp@5.6.0`; #1790 kept that pin. The run must decide whether the file-based app follows the one owner or is excluded on purpose. Run 1.1 no longer edits that file, so the only remaining reason to wait for batch 1 is #1576's possible `.csproj` edit |
| 2.3 | — | — | drop | #1693 closed by #1790 (`sdd/README.md:62` rewritten) |
| 2.4 | `/squad #1724` | bug-fix | keep | `PipelineTraceCommand` is still in `src/Reihitsu.Cli/Diagnostics/`, so shipped source → bug-fix. Risk unchanged: `scripts/README.md:40` lists the skips ("Syntax-invalid and generated inputs …"), and editing it would put tooling next to shipped source without a change doc |
| 2.5 | `/squad #1781` and `/squad #1786` | behavior-change | adjust (split) | #1790's new rule *A batch shares a mechanism* (`squad/README.md`, Several issues): the FMT-BLANK-011 exemption (blank-line phase) and the argument-list join (line-break phase, RH5305) are different mechanisms, so the lead would propose a split before the claim. They share no file except `requirement-ids.txt` (ledger exception), so both stay in batch 2 as separate runs |
| 3.1 | `/squad #1726` | behavior-change | keep | Unchanged |
| 3.2 | `/squad #1587` | behavior-change | keep | Unchanged. The split now also covers union members (#1782) |
| 3.3 | `/squad #1516` | behavior-change | keep | Unchanged. #1782 added code to `LineBreakListRewriter`/`MemberLineStartRewriter`, which are in its `SyntaxFactory` walk |
| 4.1 | `/squad #1732` | behavior-change | keep | Unchanged |
| 4.2 | `/squad #1759` | behavior-change | keep | Unchanged. #1787 added union kinds to RH7102–RH7110 registrations; the walk reads them as they are |
| 5.1 | `/squad #1788` | (skeptic decides) | keep | Premise grew: #1787 added `UnionDeclaration` to about 15 registrations and to `ParameterListParentPolicy`/`AttributeTargetUtilities` by hand, which is exactly the duplication the issue names |

## Findings

- **Batch 1 file disjointness holds.** Runs 1.1 and 1.3 meet only through `AGENTS.md`: #1507's test reads the table, and #1544 adds `BraceBlankLinePolicy` (declared in `src/Reihitsu.Formatter/Pipeline/BlankLines/Utilities/BraceBlankLinePolicy.cs`, so it resolves). Run the architecture tests on main once both have merged. `sdd/README.md` is now edited only by 1.3. No run in batch 1 depends on another.
- **Run 1.1 is still worth a run.** It has four open issues. Their shared files force them together: #1470 and #1544 both edit `AGENTS.md`, and #1470 and #1471 both edit `scripts/README.md` and `squad/README.md`. #1544 cannot join #1507 because that run is tests and #1544 is tooling (escalation 8). Moving 1.1 to batch 2 would collide with 2.1 (`AGENTS.md`). Under the new mechanism rule the lead may propose a split. Pre-decided answer: keep the issues together, because they are all "one statement of an agent-facing rule" and share files. #1479 alone (`squad/failure-classes.md`) could also run on its own, but that saves nothing.
- **#1725's certain escalation is gone** with #1725. Run 1.1 expects no output question and no escalation.
- **Optional:** with #1545 gone, #898 alone (only the mode bit plus an optional mode test, no `.csproj`) could join batch 1 as its own tooling run. It would share `scripts/README.md` only if it documents the mode test, so it is safer to leave it in 2.2.
- **Batches 2–4 overlap check:** batch 2 is now 2.1, 2.2, 2.4, 2.5a (#1781), and 2.5b (#1786). These runs share no file apart from `requirement-ids.txt`. 2.5a and 2.5b each need their own change doc. Batches 3 and 4 are unchanged, including the conflict watches the plan already lists (3.1/3.3, 4.1/4.2).

## Recommended batch 1

```
/squad #1470 #1471 #1479 #1544
/squad #1505 #1577 #1576
/squad #1507 #1428
/squad #867
/squad #1672
```

Later batches: drop run 2.3, and split run 2.5 into `/squad #1781` and `/squad #1786`, both still in batch 2. Run 2.2 has to decide the 5.6.0 pin in `scripts/proof/verify-text-only.cs`. Batches 3, 4, and 5 are unchanged.

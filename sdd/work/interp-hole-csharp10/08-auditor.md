Role: auditor
Commit: 369a7ae15a94c660921e6060f354e54d5cb1bfcc
Verdict: findings

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

Full audit of thoenissen/Reihitsu#1948. Head `369a7ae15a94c660921e6060f354e54d5cb1bfcc` (checked out, clean tree). Base `origin/main` = `0feb51810751b245fec4c51eb6fd207d0f824f75`, an ancestor of the head. The diff `0feb518..369a7ae` matches 02–07: 13 production files (Core overload, two statement blank-line bases, the attribute placement base, RH5103, RH5110, RH5206, RH5301, RH5303, RH5403, RH5417 de-duplication, and the RH3004 and RH3208 fixes), specs plus the ledger, two test-base helpers, and the rule tests. `scripts/lane-check.sh bug-fix`: confirmed.

## Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | high | `src/Reihitsu.Analyzer.CodeFixes/Rules/Clarity/RH3004UseLambdaSyntaxCodeFixProvider.cs:393-401` (`WithholdsFixForLines` returns `false` before the hole check on the expression-body path); `sdd/specs/analyzer/RH3004.md` RH3004-010 ("On the expression-body path, the fix is offered there … because the lambda it writes stays on one line"); test `RH3004UseLambdaSyntaxAnalyzerTests.ExpressionBodyInsideANonVerbatimInterpolationIsFixedBeforeCSharp11` (`:1727`) | FC-028, FC-016, FC-027, FC-044 | – | same requirement (SHARED-FIX-003, FMT-BREAK-044, the issue's "no fix formats a target inside such a hole") | The expression-body path still runs `ReihitsuFormatter.FormatNodeInDocumentAsync` on a lambda that lies inside a C# 10 `$"` hole. The formatter receives the lambda as a detached target and lays out any brace construct in the returned expression over several lines. The result does not compile. Run with `scripts/apply-fix.sh RH3004 <dir> --lang-version 10`, under LF and CRLF alike. Each input is a method body line `var s = $"a {G(delegate (int p) { return <expr>; })} b";`. These inputs compile under C# 10. **Failing members** (the fix is offered and writes a multi-line hole): `new P { A = p }.A` → `$"a {G((int p) => new P⏎{⏎A = p⏎}.A)} b"`, `new { A = p }.A`, `new List<int> { p, 1 }[0]`, `(r with { A = p }).A`, `p switch { 1 => 2, _ => 3 }`, `F(() => { var a = p; return a; })` (the result also gains a blank line), `F(delegate { return p; })`. **Holding members** (the result stays on one line): `p`, `new[] { p, 1 }[0]`, `p > 0 ? p : 1`, `p is { } q ? q : 0`, `F(() => p)`. A scratch `net11.0` project with `<LangVersion>10</LangVersion>` fails on the first failing result with `error CS8967: Newlines inside a non-verbatim interpolated string are not supported in C# 10.0`, and the input builds. `apply-fix.sh` labels these results `fixed`, as the skeptic noted. Decision 9 narrowed decision 4 to the block path because one cell (`return p;`) held. That narrowing is the gap: the cell list missed the class. On the axis "string kind": a `$@"` hole nested in a `$"` hole fails the same way, because the predicate is the same. A `$@"` hole outside every `$"` hole and every C# ≥ 11 cell hold. Fix All goes through the same `ApplyCodeFixAsync`, so it fails the same way. | Refuse the class by its cause: withhold RH3004's fix, on both paths, for every anonymous method inside such a hole. That means doing the `IsInsideSingleLineInterpolation` check before the `keepsBlockBody` branch. Do not refuse by a guard that predicts which expressions the formatter breaks (a "contains a brace" list), because that models a later component's layout (FC-044). Amend RH3004-010 and its second example, and RH3004-003's wording if needed. Change the expression-path test to a no-fix test at C# 10, and add a no-fix cell with a brace construct, for example `new P { A = p }.A`. Withholding a fix for this rare class puts nothing in question (README, *What reaches the maintainer*), although `main` offers the `(int p) => p` fix today. The lead decides; the spec author's 02 note proposed asking. |

## Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| SHARED-STMT-005: no "preceded by blank line" report in the hole (`StatementShouldBePrecededByABlankLineAnalyzerBase.OnStatement`) | the reported statement, ancestors and self | the gap in front of it: its previous token is the block's `{` or the end of the previous statement in the same block, both inside the same hole | agree; a statement that only contains a `$"` string stays reported (tested) | holds |
| RH5011-009 (`StatementShouldBeFollowedByABlankLineAnalyzerBase.OnStatement`) | the `break` statement | the gap behind it to the next token in the same section, inside the same hole | agree | holds |
| SHARED-ATTR-024 (`TargetAttributePlacementAnalyzerBase.OnAttributeList`, SeparateLine branch only) | the attribute list | `tokenAfter`, the target's next token, inside the same hole. The SingleLine branch (RH5523 parameters) cannot fire in a one-line hole | agree | holds |
| RH5103-006 (`AnalyzeStatements`) | `context.Node`, the block or switch section | the reported statements, which are its children | agree | holds |
| RH5110-004 | the declaration (a local function, inside a hole) | its constraint clauses | agree | holds |
| RH5206-004, RH5301 and RH5303 (SHARED-INIT-003), RH5403-004 | the switch expression, initializer, or block | the braces and elements the rule would break, which are descendants | agree; an initializer cannot lie outside its creation expression | holds |
| RH5417-005 (de-duplicated) | the block, unchanged | Core overload; the same `SyntaxTree.Options` read as the deleted private copy | identical | holds |
| RH3208-004 (`CanRegisterCodeFix`) | the local function | the node the base formats, the local function, inside the same hole | agree | holds |
| RH3004-010 (`WithholdsFixForLines`) | the anonymous method, on the block path only | the node the formatter formats on both paths, the lambda that replaces it | **disagree**: the expression path is not covered | finding #1 |
| Language-version gate (Core `IsInsideSingleLineInterpolation(SyntaxNode)`) | `SyntaxTree.Options.LanguageVersion` (effective version, not `Default`/`Latest`) | the formatter reads `FormattingContext.LanguageVersion` with the same `CanInterpolationsHoldLineBreaks` threshold | agree | holds |

## Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| "No line break inside a `$"` hole below C# 11" | `rg -n "IsInsideSingleLineInterpolation\|CanInterpolationsHoldLineBreaks\|InterpolatedStringStartToken\|CSharp11" src`: one owner, `Reihitsu.Core.StringInterpolationUtilities` (`CanInterpolationsHoldLineBreaks`, `IsInsideSingleLineInterpolation(SyntaxNode, LanguageVersion)`, new `(SyntaxNode)` overload). Callers: the formatter (`SingleLineInterpolationUtilities`, `ControlFlowBraceTransform`, `DocumentationCommentFormattingPhase`, `SwitchCaseBraceRewriter`), the 12 analyzer and fix sites of this run, and RH5417. RH5417's private copy is deleted. `CasingRenameSafetyUtilities.cs:892` checks C# 11 for an unrelated feature (generic attributes) | single owner |
| Language version in analyzer tests | new `AnalyzerTestsBase<TAnalyzer>.CreateLanguageVersionTransform`; existing per-class copies in the RH4105, RH3103, RH3210, RH5412, RH5415, RH5416, RH5417, RH7003 test classes | test-only duplication that predates the run; hint |

## Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| 20 SHARED-STMT rule triples, RH5011, RH5103, RH5110, RH5206, RH5301, RH5303, RH5403, RH5513, and RH5527 `…IsNotReportedBeforeCSharp11` | no diagnostic in a direct or nested `$"` hole at C# 10; the "contains a string" cell stays reported | any diagnostic in `M`/`N`, or the holding cell missing | `Verify` with `CreateLanguageVersionTransform`. This transform runs after the verifier's own `Latest` transform; the tests failed on `main` (03), so the version is effective (FC-033) | adequate |
| `…IsReportedFromCSharp11`, `…InsideAVerbatimInterpolationIsReportedBeforeCSharp11` | the boundary's other side, and `$@"`/`@$"` | missing diagnostic | `Verify` | adequate (FC-003) |
| RH5008 C# 7.3, alignment and format clause, anonymous method, and comment cells; RH5301 comment, triple-nest, verbatim-beside-`$"`, nested, and indexer cells | the position axes | report or miss | `Verify` | adequate |
| RH3004 and RH3208 `…OffersNoFixBeforeCSharp11` (direct, and behind a verbatim string) | no action registered at C# 10 | any registered action | `VerifyNoCodeFixIsOffered(source, LanguageVersion)`. It re-parses the ad hoc document at the language version, and the old default stays `Latest` | adequate |
| RH3004 and RH3208 `FixAllLeaves…` | Fix All leaves the hole diagnostic and fixes `N` | text mismatch | `Verify(source, fixed, configure)` with `MarkupHandling.Allow` on fixed and batch states. Fails on the pre-fix provider with a text mismatch (05) | adequate |
| RH3004 `ExpressionBodyInsideANonVerbatimInterpolationIsFixedBeforeCSharp11` | pins that the expression path keeps its fix at C# 10, on `return p;` only | — | `Verify` | asserts the generalization finding #1 refutes; part of finding #1 |
| RH3004 and RH3208 holding fix cells (C# 11, `$@"`, contains-a-string) | `main`'s output on unchanged cells | text mismatch | `Verify` | adequate (FC-035: values match `main`, checked in 03) |

## Failure classes dismissed

- FC-004, FC-005, FC-019, FC-020, FC-021: the run adds no rewrite. An analyzer skips a report or a fix is withheld. A directive cannot stand in a one-line hole. `/* */` and `/** */` cells are tested.
- FC-006: the language-version gate is the subject. It reads the effective `CSharpParseOptions.LanguageVersion`, which never returns `Default` or `Latest`.
- FC-007, FC-008: no fix output changes where a fix is still offered, except finding #1. Fix All is tested for RH3004 and RH3208.
- FC-009: the withheld fixes register no action (tested).
- FC-010 to FC-012: no formatter change.
- FC-013: with this change, the analyzers stop reporting what the formatter already leaves as written in the hole (FMT-BREAK-044). Agreement improves.
- FC-014: no new rule.
- FC-017: see *Assertion adequacy*.
- FC-018, FC-023 to FC-026, FC-029, FC-036: no rename, identifier, member-kind, or key/value change.
- FC-022: `$"""` requires C# 11, where the gate is open. A verbatim string nested in a `$"` hole is covered by the ancestor walk (tested).
- FC-030: the fixes are unchanged where they are offered.
- FC-031: every changed summary names the new case. The RH3004 summary describes the block-path gap as implemented.
- FC-032: no formatter change.
- FC-034: the reproduction uses the issue's own shapes.
- FC-037: no text-only claim.
- FC-038 to FC-043: no unsupported shape is declared or changed. RH3004's `FindUnsupportedSyntax` call is unchanged. Lead decision 5 (not declaring) is consistent with FMT-UNSUP-003: the failing input, a detached target, does not hold the shape.
- FC-001, FC-002, FC-003, FC-015, FC-027: traced in the tables above; they hold, except the RH3004 member in finding #1.

Other reported-in-hole fixes checked, outside the skeptic's list:
- RH5405, RH5406, and RH5407 (`StatementBracesCodeFixProviderBase`): SHARED-BRACE-003 withholds them, because code always stands in front of the `if` in a one-line hole.
- RH7004 (`TryCreateChanges` refuses when the previous token shares the declaration's line) and RH7501 (`CanMoveBreak` requires `break` to start its line): both always withheld in a hole.
- RH3102 writes no line break.
- RH5404 registers type declarations only.
- RH5304/RH5304-like nested initializer rules were not observed reporting in a hole (01).
- Every other formatter-calling fix acts on members or types, or on forms that already span lines.

## Architecture findings

FMT-BREAK-044's decision ("no line break in a `$"` hole below C# 11") is applied by whole-document formatting (`SingleLineInterpolationUtilities.Annotate` reads it from the root). It is not applied by the node-level entry points (`ReihitsuFormatter.FormatNode`, `FormatNodeInDocumentAsync`), which detach the target. This is the skeptic's FC-028 finding in 01. Lead decision 4 refused the class in each fix, and decision 9 narrowed that refusal to the RH3004 block path by one holding cell. Finding #1 shows that the narrowed refusal misses members of the same class. Refusing by the cause (the target lies inside such a hole), not by cells, closes it without a structural change.

## Hints

- The language-version solution transform now exists once in `AnalyzerTestsBase<TAnalyzer>`, but older test classes keep private copies (RH4105, RH3103, RH3210, RH5412, RH5415, RH5416, RH5417, RH7003). This is test-only and predates the run; it does not block.

## Follow-up candidates

_None_ beyond the lead's existing `scripts/apply-fix.sh` candidate. It labeled every failing member of finding #1 `fixed` again, for the same cause: `FixtureRunner.HasSyntaxErrors` reads only syntax-tree diagnostics.

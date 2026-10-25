Role: auditor
Commit: 6dbf0451d1020057a69e117936b755adb0e2e768 (revision 2, repair round 2; revision 1 was 2f5432b931881d400e2dfbe818c729c2670b27c4, revision 0 was 6555cd10b2d84a3798073660100c21a867a62543)
Verdict: findings

My prompt carried no intended fix, suspected cause, or hoped-for verdict. I changed nothing except this handover.

How the statements below were established: static tracing of `git diff origin/main` and of every counterpart file named below, plus probes in a scratch directory outside the repository. Each probe was formatted twice with this head's `scripts/format.sh` and twice with the same script from a temporary `origin/main` worktree (removed afterwards), traced with `scripts/trace.sh`, and run through `scripts/apply-fix.sh` (RH5031, RH5022, RH5020, RH5201, RH3202, LF and CRLF). The C# scope claims were compiled in a scratch `net10.0` library. Findings are numbered A1–A7 across the run.

## thoenissen/Reihitsu#948

### Scope

Full audit (first round). Head `6555cd1`, base `f3d514d`, which equals `git ls-remote origin refs/heads/main` and is an ancestor of the head. The production change is `SwitchCaseBraceRewriter.HasCrossSectionLocalUse`, `GetDirectlyDeclaredLocalNames`, and `GetDesignatedNames`, plus the guard call in `VisitSwitchStatement`, as `04-implementer.md` describes.

### Findings

| # | Severity | Location | Failure class | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|
| A2 | medium | `src/Reihitsu.Formatter/Pipeline/SwitchCaseBraces/Rewriter/SwitchCaseBraceRewriter.cs:196` (use scan), `:323` (`GetDesignatedNames`) | FC-002, FC-001 | PR-introduced | The guard fires for names that are not the local. (1) A lambda parameter in another section shadows the local (allowed since C# 8): section 1 `int y = 1;` / `Use(y);`, section 2 `Func<int, int> f = y => y + 1;` / `Use(f(2));`. The switch stays unbraced, although the local is used only in its own section, and FMT-SWITCH-014 says such a local "MUST NOT trigger this guard". (2) A pattern variable in a query clause, which C# scopes to the clause: section 1 `var q = from a in xs where a is int n select n;`, section 2 `Use(n);` where `n` is a field. This switch also stays unbraced. Both braced forms compile (scratch build), and `origin/main` braces both. The output compiles, but it breaks FMT-SWITCH-014's MUST NOT for (1), and (2) is a clause-scoped declaration counted as a direct one. The implementer's handover calls (1) a deliberate over-approximation, but no spec text backs it. | Narrow the predicate. Do not count a simple name bound inside a lambda, anonymous method, or local function that declares a parameter or local of that name. Do not collect designations in query clauses after the first `from` expression. Add a test for each. Alternatively, the maintainer accepts a syntax-only over-approximation, and FMT-SWITCH-014 says so; that is a spec change. |
| A3 | medium | `SwitchCaseBraceRewriter.cs:237`; `sdd/specs/formatter/switch-case-braces.md` FMT-SWITCH-014 list; `src/Reihitsu.Formatter.Test/Regression/SwitchCaseBraces/SwitchSectionSharedLocalTests.cs` | FC-016, FC-017 | same requirement | The code collects `out var` and pattern variables from a local declaration's initializer, and that branch is needed: `var b = F(out var o);` in section 1 with `o = 2;` in section 2 compiles unbraced (scratch build). The branch leaves that switch unchanged, while `origin/main` braces it into CS0103. No test exercises the branch: every `out var` test uses an expression statement, `return`, `throw`, `yield return`, `lock`, a nested switch, or a label, and `var (a, b) = …` is an expression statement. Deleting the branch would keep the suite green. FMT-SWITCH-014's third bullet also omits the local-declaration initializer, so the spec under-states what the code must do. | Test implementer: a regression test with `var b = F(out var o);` used in another section. Spec author: name the initializer of a local declaration in FMT-SWITCH-014's list. |

### Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Whole switch left unbraced for a shared local (`HasCrossSectionLocalUse`) | Declarations: the statements each section owns, by kind (local declaration, local function, expression statement, `return`/`throw`/`yield return`, `if` condition, nested `switch` expression, `lock` expression), through labels. Designations come from `GetDesignatedNames`, which skips anonymous functions and switch-expression arms. Uses: every `SimpleNameSyntax` in every other section, labels included | C# switch-block scoping (compiled), and the sibling whole-switch guard `HasCrossSectionGotoTarget` | Declarations agree with C# for every form the skeptic compiled, and loops, `using`, `fixed`, blocks, and `else` fall to the default branch. The guard over-approximates query-clause designations and simple names bound by a shadowing lambda or local-function parameter | A2 |

### Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Cross-section local scope | `rg "SingleVariableDesignationSyntax\|DeclarationExpressionSyntax" src --glob '!*Test*'`: only `SwitchCaseBraceRewriter` decides scope. The other hits are the naming analyzers RH4115/RH4117/RH4125/RH4126, `SingleLetterIdentifierUtilities`, and the casing fix base, none of which decides scope | One owner |

### Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `SwitchSectionSharedLocalTests` "…LeavesTheSwitchUnchanged" (18) | The whole pipeline is a fixed point on the input, LF and CRLF | Any brace inserted | `AssertRuleResult(input)`, exact equality | Adequate; the local-declaration initializer form is missing (A3) |
| `SwitchSectionSharedLocalTests` `ReportedLocalUsedInAnotherSectionLeavesTheSwitchUnbraced`, "…DoesNotBlockTheBraces", `GuardedOuterSwitchKeepsInnerSwitchDecision`, `TwoSwitchesInOneMethodAreDecidedSeparately` | Exact braced output, then a second pass that changes nothing | A wrong or missing brace, or a pass-2 change | `AssertRuleResult(input, expected)` | Adequate |

### Failure classes dismissed

- FC-001: the guard inspects every section of the switch, which is the span of the decision it protects.
- FC-003: both sides are tested: a local used in another section, and one used only in its own section.
- FC-004, FC-005, FC-019, FC-020, FC-021: the guard makes no edit; it returns the node unchanged.
- FC-006: the guard exists to keep the output compiling, and the compiled probes agree.
- FC-010, FC-011, FC-012: two-pass probes are stable.
- FC-013, FC-014: no rule reports switch-section braces.
- FC-015: one owner.
- FC-023: names are compared by `ValueText`, so `@y` and `y` match, as C# binds them.
- FC-026: two switches in one method are tested, and section identity is reference equality within one tree.
- FC-027: nested switches, lambdas, and existing blocks are tested.
- FC-031: the summaries describe the code.
- FC-007, FC-008, FC-009, FC-018, FC-022, FC-024, FC-025, FC-028, FC-029, FC-030, FC-036: no code fix, rename, literal, or member conversion is involved.
- FC-032 to FC-035, FC-037: the suites were rerun after the last production commit (`03-test-implementer.md` revision 2, `05-cleaner.md`). Expected values come from the spec examples, and the issue input is reproduced exactly.

### Hints

- RemoveBraces (the other direction) widens scope too. Two single-line blocks that declare the same local would collide once their braces are removed. LineBreakPhase expands such blocks before this phase runs, and probing showed no input that reaches it. Not a finding.

## thoenissen/Reihitsu#949

### Scope

Full audit. The production change is `SwitchCaseBraceRewriter.BuildTerminalBreakLeadingTrivia` and its call site in `AddBraces`.

### Findings

| # | Severity | Location | Failure class | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|
| A1 | high | `src/Reihitsu.Formatter/Pipeline/SwitchCaseBraces/Rewriter/SwitchCaseBraceRewriter.cs:478` (the end of line appended at `:501`) | FC-016, FC-010 | same requirement | A documentation comment in front of the terminal `break` is a comment for FMT-SWITCH-007, but its structured trivia already ends with a line break. The method appends another one. Input: `case 1:` / `A();` / `A();` / `/// doc` / `break;`, with a second section `A(); break;`. Pass 1 gives `}` / blank / `/// doc` / blank / `break;`. Pass 2's `BlankLinePhase` removes the blank line under `/// doc` (`scripts/trace.sh`, stable after pass 3). The pass that inserts the braces must already produce the final layout (FMT-SWITCH-007's new paragraph, FMT-PIPE-004). `origin/main` also needed two passes for this input, with the line removed elsewhere, so the hazard is not new. It lives in the method this run wrote for the requirement. | Do not append the end of line behind a comment trivia that already ends with a line break (`BlankLineTriviaUtilities.EndsWithLineBreak` exists). Test implementer: a `///`-before-`break` test with the two-pass helper. |

### Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Blank lines kept in the rebuilt `break` trivia | The original leading trivia of the terminal `break`. An end of line on a line without content counts as a blank line, and pending blank lines are written only in front of a comment | `BlankLinePhase` FMT-BLANK-011 (blank line above the comment), FMT-PIPE-013 (blank lines between comments), RH5020 | Agrees for `//` and `/* */` (tests, and RH5020 `apply-fix` on probe outputs: no diagnostic, LF and CRLF). Disagrees for `///`, whose trivia carries its own line break | A1 |

### Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Rebuilding comment trivia on brace insertion | `BuildLeadingTrivia` (first statement moved into the block) and `BuildTerminalBreakLeadingTrivia` (terminal `break`), both in `SwitchCaseBraceRewriter`. `rg -n "BuildLeadingTrivia" src/Reihitsu.Formatter`: no other user | Two owners that differ in blank-line handling. The first-statement path's FMT-PIPE-013 defect was scoped out of the run (skeptic follow-up, not routed), so the difference is justified by scope. Both copies share the `///` hazard (A1); see the hints |
| Blank line above a comment | `BlankLinePhase` (FMT-BLANK-011). The rewriter only preserves it | One owner |

### Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `TerminalBreakCommentTests` (8) | Exact pass-1 output and an unchanged pass 2, LF and CRLF | A missing blank line, or a pass-2 change | `AssertRuleResult` | Adequate for `//` and `/* */`; `///` is missing (A1) |
| `RH5020…FormatterTests.VerifyFormatterSeparatesCommentBeforeTerminalBreakOf{Braced,Unbraced}Section` | The formatter output equals the fixed code and is RH5020-clean, LF and CRLF, second pass | An RH5020 report or output drift | `VerifyFormatter` | Adequate |

### Failure classes dismissed

- FC-004, FC-021: no comment text is lost. Two comments on one line are split, which is old behavior; see the hints.
- FC-005, FC-019, FC-020: a section with a directive is not braced (`SectionCarriesDirectives`).
- FC-012: `trace.sh` on the formatted outputs shows no phase change.
- FC-013: RH5020 is clean.
- FC-003: the boundaries are tested (unbraced switch; a comment first in a break-only section).
- FC-017: the helper checks the exact output and a second pass.
- FC-026, FC-027: several labels, and a section braced because of another section, are tested.
- FC-001, FC-002, FC-006 to FC-009, FC-011, FC-014, FC-015 (see the table), FC-018, FC-022 to FC-025, FC-028 to FC-037: no guard, rewrite of code, fix, or evidence issue in this change.

### Hints

- `/* a */ /* b */` on one line in front of the terminal `break` is split onto two lines, by the old code and by the new code alike. FMT-PIPE-013 keeps the space between two comments on one line for wrapped code; the `break` stays outside the block.
- `BuildLeadingTrivia` (first statement) still drops blank lines between comments (the skeptic's follow-up). It also emits the same extra line behind `///`.

## thoenissen/Reihitsu#954

### Scope

Full audit. The production change is the `TokenGapNormalizer.TryNormalizeGapAfterLeadingStructuredTrivia` brace guard, the new `BlankLineEditor.IsExemptFromPrecedingBlankLineBeforeRegionDirective(previousToken, leadingTrivia, directiveIndex)` and its use in `EnsureBlankLineBeforeFirstDirective`, and the removed outer guards in `BlankLineRegionDirectiveRewriter` and `BlankLineTriviaBoundaryRewriter`.

### Findings

| # | Severity | Location | Failure class | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|
| A4 | medium | `src/Reihitsu.Formatter/Pipeline/BlankLines/Utilities/BlankLineEditor.cs:123` (the conditional cases at `:141`), summaries at `:108` and `:269` | FC-013, FC-015, FC-016, FC-031 | same requirement | The new predicate keeps the brace exemption when `#if`/`#elif`/`#else`/`#endif` or disabled text stands between `{` and the region directive. SHARED-BLANK-005 says only the single line directly above counts, and Core `RegionDirectiveBlankLineUtilities.IsMissingRequiredBlankLineBefore` follows it. Counterexample: `class E` / `{` / `#if !NEVER` / `#region A` / … / `#endregion // A` / `#endif` / `}`. The formatter keeps `#if !NEVER` directly above `#region A`, and `apply-fix.sh RH5031` on the formatter's output reports it and inserts the line (`fixed`, LF and CRLF). The same holds for `{` / `#if NEVER` / disabled / `#endif` / `#region A`. The output is identical on `origin/main`, but this predicate is new code that encodes the carve-out. It sits beside `#pragma` and `#nullable`, which the run separates from the region, in the skeptic's input class "non-region directive". Its summary contradicts itself ("Only the single line directly above the directive counts" next to "keep the brace exemption"). `EnsureBlankLineBeforeFirstDirective`'s summary ("the line directly above it is the opening brace line or the start of the file") matches neither the conditional case nor the start-of-file case. The untagged `DisabledRegionLayoutTests.RegionInsideActiveConditionalIsStillFormatted` pins the contradicting output. | Drop the conditional-directive and disabled-text cases from the predicate, and have the test implementer correct the existing test's expected value; the spec is unambiguous. If the maintainer wants the carve-out, SHARED-BLANK-005 and RH5031 have to state it together, which is a spec change. Either way, align both summaries with the code. |

### Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Brace exemption for a region directive (`IsExemptFromPrecedingBlankLineBeforeRegionDirective`, 3-argument) | The previous token's kind and trailing comment, and the kinds of the leading trivia in front of the directive | Core `IsMissingRequiredBlankLineBefore` (RH5031): only the line directly above is blank or ends with `{` | Agrees for `//`, `/* */`, several comments, `#pragma`, `#nullable`, and `///` (probes clean under RH5031). Disagrees for conditional directives and disabled text behind `{` (A4), and for any trivia in front of the file's first token (hint) | A4 |
| Region-ending gap left alone behind `{` (`TokenGapNormalizer`) | The token's previous token, including zero-width tokens, is `{`, and the gap's last structured trivia is a region directive followed by nothing but whitespace and line breaks | `BlankLinePhase` FMT-BLANK-007 (`IsFirstInBlock`: previous token `{`) | Both key on the previous token `{`. Probes over an interface, struct, record, constructor, lambda, local function, operator, object initializer, comment above the region, two regions, and CRLF: no blank line after `{`, stable, RH5022- and RH5031-clean | Agrees. The guard was narrowed to `{` (FC-001) within the maintainer's approval; see the hints |
| Trailing-trivia `#endregion` branch | Token-only predicate, unchanged | — | Unchanged behavior | Agrees |

### Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| SHARED-BLANK-005 brace and start-of-file exemption | Formatter: `BlankLineEditor.IsExemptFromPrecedingBlankLineBeforeRegionDirective` (two overloads; the token-only one is still read by `BlankLineTriviaBoundaryRewriter.VisitToken` for trailing `#endregion`). Analyzer: Core `RegionDirectiveBlankLineUtilities.IsMissingRequiredBlankLineBefore` | Two owners, formatter and analyzer by design. Parity is not proven and does not hold for conditional directives (A4) or the start of the file (hint) |
| Normalization of a region-ending gap | `TokenGapNormalizer.TryNormalizeGapAfterLeadingStructuredTrivia` only | One owner |

### Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `RegionDirectlyAfterOpeningBraceTests` (17) | The exact FMT-BLANK-007/-013 output after pass 1 and pass 2, LF and CRLF; for the already formatted inputs, a fixed point | A blank line after `{`, a missing blank line under a comment or `#pragma`, a pass-2 change | `AssertRuleResult` | Adequate. It asserts exact output rather than the text-level two-pass check, which could not see the defect (FC-012). The conditional-directive case is pinned the other way by an existing test (A4) |
| `RegionFormattingFullPipelineTests.EmptyRegionInsideBlockIsPreservedAndFormatted` (corrected) | No blank line after `{` | The old blank line | `AssertRuleResult` | Adequate; the expected value follows the spec, not the patched code |
| RH5022 and RH5031 formatter parity tests | Formatter output equals the fixed code and is clean | A report on the formatter output | `VerifyFormatter` | Adequate |

### Failure classes dismissed

- FC-004, FC-005, FC-019, FC-020: the edits insert line breaks only, and directives are kept.
- FC-010: CRLF is tested and probed.
- FC-011: no loop.
- FC-012: `trace.sh` on formatted outputs shows no phase change.
- FC-003: both sides are tested: `{ // x`, the active and inactive region, and `#if`/`#endif` without a region.
- FC-017: exact-output helper.
- FC-027: the brace owners were probed.
- FC-002, FC-006 to FC-009, FC-014, FC-018, FC-021 to FC-026, FC-028 to FC-030, FC-032 to FC-037: no rewrite of code, no fix, and no evidence issue.

### Hints

- Start of the file: `// header` / `#region A` keeps no blank line. RH5031 reports the formatter's output (`apply-fix`: `fixed`, LF and CRLF). This is old behavior; the new predicate's `previousToken.Exists == false` early return keeps it. It is already among the spec checker's items for the lead.
- The `TokenGapNormalizer` guard covers only gaps behind `{`. The mis-resolved previous token of other region-ending gaps remains, as in the implementer's FMT-BLANK-016 follow-up (`#pragma warning restore` in front of `#endregion`). That case lies outside the approved scope.
- A `///` first after `{` above `#region` now gets a blank line between them. RH5031 is clean, the result is stable in two passes, and the comment still documents the member below.

## thoenissen/Reihitsu#1390

### Scope

Full audit. The production change is `ExpressionBodiedTransformUtilities` (`RequiresAsyncExpressionBodyPreservation`, `GetWrittenTaskTypeName`), the method and local-function transforms, the guards in the RH3202 provider and the expression-body code-fix base, `ExpressionBodyToBlockConverter.CreateBlock`/`TrySplitOwnLineComments`/`CreateReturnStatement`, and `LineBreakInitializerRewriter.VisitWithExpression`.

### Findings

| # | Severity | Location | Failure class | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|
| A5 | medium | `src/Reihitsu.Formatter/Pipeline/StructuralTransforms/Utilities/ExpressionBodyToBlockConverter.cs:39` (arrow comments moved) with `:54` (arrow trailing comment kept on `{`) | FC-004, FC-021 | same requirement | When an own-line comment before the arrow meets a comment on the arrow's line or glued behind the arrow, the comments come out in reverse order. `int A2()` / `// a` / `=> // b` / `42;` becomes `{// b` / `// a` / `return 42;`. `int Get3()` / `// own` / `=> /* glued */ 3;` becomes `{/* glued */` / `// own` / `return 3;`. The formatter does this, and so does the RH3202 fix (`apply-fix.sh RH3202`, LF and CRLF); the RH3203 and RH3205–RH3208 fixes reach the same `CreateBlock` through the formatter. `origin/main` kept the source order (`// a` above `{// b`). FMT-STRUCT-006 leaves the arrow-line comment unspecified (#963), so no requirement decides the combination. Still, AGENTS.md "Code-fix coverage" calls a fix that reorders user comments unsafe, and the reorder is new in this PR. Every order-preserving layout touches the unspecified arrow-line placement. | Spec author: raise the mixed case as an open question for the maintainer, for example keeping the before-arrow comments outside the block when the arrow carries a comment, or moving the arrow comment below them. Then implement and test the answer. |
| A6 | low | `src/Reihitsu.Formatter.Test/Regression/Structural/ExpressionBodyNextLineExpressionTests.cs:518` | FC-031 | same requirement | `BlockCommentLeadingNextLineExpressionKeepsOwnLine` now asserts `return /* c */ 42;`, where the comment joins the `return` line. The method name says the opposite. | Rename the test to match its assertion. |

### Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Keep an `async` expression body (`RequiresAsyncExpressionBodyPreservation`) | `async` modifier; return type: `void`, a `?` looked through, the last simple name of a simple, qualified, or alias-qualified name, `Task`/`ValueTask` | `UsesExpressionStatement` (same `GetWrittenTaskTypeName`); the code-fix registration in the base and in the RH3202 provider (same predicate); Core `ExpressionBodyRewriteUtilities.BlocksRewrite` (analyzer report; deliberately not consulted, SHARED-BODY-004) | Agrees. The statement form and the keep decision read one name function. The fix is withheld exactly where the formatter keeps the body, and the analyzer still reports | Agrees |
| Move comments around the arrow into the block (`TrySplitOwnLineComments`, arrow-line condition) | Arrow leading trivia (only whitespace, line breaks, ordinary comments; the arrow's own line holds whitespace only) and expression leading trivia | Open-brace trailing trivia from the arrow's trailing comment (`ExtractSignificantTrivia`) | Disagrees: the move ignores whether the arrow carries a trailing comment, which stays on `{` and ends up above the moved comments | A5 |
| Join `return /* c */ 42;` (`CreateReturnStatement`) | Expression leading trivia is all whitespace or `/* */` | The last line `TrySplitOwnLineComments` leaves on the expression | Agrees. A multi-line `/* */` there is also joined, with its text kept. A doc comment or directive keeps the old layout | Agrees |
| `with` initializer brace (`VisitWithExpression`) | `node.Initializer.OpenBraceToken`, with the `with` expression owning both tokens | `VisitInitializerExpression` (`EnsureBraceOnOwnLine`), `VisitObjectCreationExpression` | Agrees. Probes (empty, one element, two elements, nested `with`, argument, lambda, `/* c */` and `// c` behind `with`, `#pragma` in front of `{`) are stable in two passes, and directives and comments are kept | Agrees |

### Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| FMT-STRUCT-025 keep predicate | `ExpressionBodiedTransformUtilities.RequiresAsyncExpressionBodyPreservation`, read by `ExpressionBodiedMethodTransform`, `ExpressionBodiedLocalFunctionTransform`, `ExpressionBodyToBlockCodeFixProviderBase`, and `RH3202ExpressionStyleMethodsShouldNotBeUsedCodeFixProvider` (`rg "RequiresAsyncExpressionBodyPreservation\|IsTaskLikeName\|UsesExpressionStatement"`) | One owner. The RH3202 provider's copy of the registration loop predates the run; see the hints |
| Statement form | `UsesExpressionStatement` (method, local-function, and operator transforms) | One owner |
| Comment carry into the block | `ExpressionBodyToBlockConverter.CreateBlock`, used by the six expression-bodied transforms | One owner |

### Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `AsyncUnknownTaskTypeExpressionBodyTests` (13) | Kept bodies are fixed points; converted bodies have the exact form, twice, LF and CRLF | Conversion of an alias or custom type, or the wrong statement form | `AssertRuleResult` | Adequate |
| `ExpressionBodyArrowCommentTests` (15) and the four corrected tests | Exact block layout with the comments in front of the statement, two passes | A comment between the signature and `{`, glued to `{`, or between `return` and the value | `AssertRuleResult` | Adequate for the specified shapes. The mixed shape is not pinned (A5); one name is wrong (A6) |
| `WithInitializerLayoutTests` (7) | Layout reached in pass 1, and pass 2 changes nothing | `{` left on the `with` line | `AssertRuleResult` | Adequate |
| RH3202 and RH3208 `…ReportedWithoutCodeFix` (4) | Reported, and zero actions for every fixable diagnostic | Any registered action | `VerifyNoCodeFixIsOffered` | Adequate |
| RH3202 `VerifyFixLeavesAsyncMethodReturningTaskAliasNextToConvertedMethods` | Incremental and Fix All convert only the convertible members; the kept one stays reported | Kept member converted, or its diagnostic lost | `Verify` with marked fixed state | Adequate |
| RH3202 and RH3208 nullable and comment fixes (4) | Exact fixed text | Wrong statement form, or a misplaced comment | `Verify` | Adequate (LF only, acceptable for an LF-normalized fix test) |

### Failure classes dismissed

- FC-006: CS1997 is prevented, and the nullable forms are tested.
- FC-007: by reading RH5020-001/-002, the comments the fix carries into the block follow `{`, so RH5020 does not report them (not run).
- FC-008, FC-009: Fix All and no-action are tested.
- FC-010: CRLF is tested and probed.
- FC-011: no loop.
- FC-012: `trace.sh` on formatted outputs shows no phase change.
- FC-013, FC-014: no rule reports `with` layout, and the RH3202 report on a kept member is decided by SHARED-BODY-004.
- FC-015: one owner each.
- FC-017: exact helpers.
- FC-022: a multi-line `/* */` keeps its text.
- FC-024, FC-025: the predicate reads only return-type syntax; methods, local functions, `static` local functions, and generic methods are tested, and constructors, finalizers, and operators cannot be `async`.
- FC-027: nested `with`, argument, lambda, and return are probed or tested.
- FC-028: the fixes use the document-scoped path, unchanged.
- FC-030: the fix formats only the member.
- FC-032: the analyzer suite was rerun on the final production code (`03-test-implementer.md` revision 2).
- FC-035: expected values come from the spec examples.
- FC-001 to FC-003, FC-005, FC-016, FC-018 to FC-020, FC-023, FC-026, FC-029, FC-031, FC-033, FC-034, FC-036, FC-037: nothing else reaches the change. Directives and doc comments near the arrow keep the old path (`TrySplitOwnLineComments` returns false), and the `///` before an arrow is unchanged from `origin/main`.

### Hints

- `RH3202ExpressionStyleMethodsShouldNotBeUsedCodeFixProvider` duplicates the base's registration loop, so the new guard is written twice (the implementer's follow-up).
- `new Foo { Inner = { A = 1, B = 2 } }` still needs two passes, on this head and on `origin/main` (the skeptic's follow-up; outside FMT-BREAK-021's `with` wording).
- An alias named `Task` for a type with a result is converted without `return` (CS0161). The skeptic recorded it as a residual risk of a syntax-only check.

## thoenissen/Reihitsu#888

### Scope

Full audit. The production change is `ChainLineBreakRewriter.IsBehindMultiLineCommentTail` and its use in `EnsureLineBreak`.

### Findings

| # | Severity | Location | Failure class | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|
| A7 | low | `src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/ChainLineBreakRewriter.cs:239`, `:118`; `src/Reihitsu.Formatter.Test/Regression/LineBreaks/ChainLinkBehindMultiLineCommentTests.cs` | FC-003, FC-016 | same requirement | `EnsureLineBreak` has three call sites in `ApplyWrappedLayout`: a later link, a prefix link after the first, and the first invoked link. `IsBehindMultiLineCommentTail` also accepts `/** */`. The tests cover only the later-link site, with `/* */` and with `.` and `?.`. Probes show the other sites changed in this PR. `a.B` / `/* x` / `*/ .B` / `.C()` / `.D()` and `a.B.C()` / `/* x` / `*/ .C()` / `.D()` are now broken in front of the link; `!.` and `/** */` are handled too. Each output is stable and RH5201-clean, while `origin/main`'s output was reported by RH5201. No test pins any of it. | Test implementer: one test each for a prefix link and a first invoked link behind a comment tail, and one for `/** */`. |

### Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| A link behind a multi-line comment tail does not start its line (`IsBehindMultiLineCommentTail`) | The operator's leading trivia, scanned back to the last end-of-line trivia, for a `/* */` or `/** */` holding CR or LF | `HasLineBreak` (the gap holds an end of line), and Core `FluentChainLink.StartsLine` / `FluentChain.IsWrapped` (wrap detection, which counts the link as starting a line, as SHARED-COMMENT-012's follow-up notes) | Agrees with SHARED-COMMENT-012 for every `EnsureLineBreak` call site. A one-line `/* x */` stays (SHARED-COMMENT-011). A directive line in between stops the scan at its line break. Core's wrap detection is unchanged, so the chain is still treated as wrapped and the formatter now breaks consistently (RH5201-clean) | Agrees |

### Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Line start behind a multi-line comment tail (SHARED-COMMENT-012) | `ChainLineBreakRewriter.IsBehindMultiLineCommentTail` (new, private). `rg MultiLineCommentTrivia` over Core, Formatter, and Analyzer finds no other owner of the tail test; Core `FluentChainLink.StartsLine` and `SyntaxTokenPositionUtilities.IsFirstOnLine` keep counting the line as started, as the maintainer decided (RH5201 unchanged) | One formatter owner. The Core divergence predates the run (#1382 and the skeptic's follow-up) |

### Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `ChainLinkBehindMultiLineCommentTests` (7) | Exact output and pass 2, LF and CRLF | The link left behind `*/`, or a wrong column | `AssertRuleResult` | Adequate for the later-link site (A7) |
| RH5201 `VerifyFixBreaksLinkBehindMultiLineCommentTailInOnePass` (LF, CRLF) | Converges in one incremental and one Fix All iteration | A second iteration needed | `Verify` with iteration counts 1 | Adequate for the one-pass guarantee |
| RH5201 formatter parity | Formatter output equals the fixed code and is clean | An RH5201 report on the formatter output | `VerifyFormatter` | Adequate |

### Failure classes dismissed

- FC-004, FC-021: a line break is appended behind the comment, and the comment text is kept. Two comments on the last line are left unspecified by the spec.
- FC-005, FC-019, FC-020: a directive between the tail and the link ends its own line, so the scan stops there.
- FC-007: one-pass convergence is tested.
- FC-010: CRLF is tested and probed.
- FC-011: no loop; after the break, the operator's trivia ends with a line break.
- FC-012: the probes are stable in two passes.
- FC-013: probes and tests are RH5201-clean.
- FC-015: one owner.
- FC-017: the iteration counts pin convergence.
- FC-026: replacements are keyed by token.
- FC-027: a chain in an argument was probed.
- FC-034: the issue input is reproduced exactly.
- FC-001, FC-002, FC-006, FC-008, FC-009, FC-014, FC-018, FC-022 to FC-025, FC-028 to FC-033, FC-035 to FC-037: not reached.

### Hints

- The first link of a chain (link index 0) behind a comment tail, as in `var r1 = a` / `/* x` / `*/ .B` / `.C()` / `.D();`: the formatter leaves `.B` behind `*/` (`TryCloseGap` refuses because of the comment). RH5201 reports that output, and its fix makes no progress (`apply-fix`: `no-progress`, LF and CRLF). This is identical on `origin/main` and outside FMT-BREAK-019's "a later link". Follow-up candidate under SHARED-FIX-001.

## Whole run

- State: the checkout is `6555cd1`; `origin/main` and `git ls-remote origin refs/heads/main` are both `f3d514d`, and it is merged. The diff's 13 production files, 18 test files, and spec and rule-page changes are exactly what `01`–`06` describe. `scripts/lane-check.sh bug-fix`: "confirmed — claimed bug-fix, diff needs bug-fix". No file of a concurrent run (00-lead.md list) is touched; the RH3xxx files fall under the maintainer's release (Escalation 1, Q1).
- No source `TODO`, no commented-out code, and no tracker number in the diff's `src/` lines.
- Blocking: A1 (high), A2, A3, A4, A5 (medium), A6, A7 (low).
- Routing: A1 to the implementer and the test implementer. A2 to the implementer, or to the maintainer for a spec change. A3 to the test implementer and the spec author. A4 to the implementer and the test implementer, or to the maintainer for a spec change. A5 to the spec author as an open question. A6 and A7 to the test implementer.
- I re-ran no repository test. Every finding rests on a probe or on static reading, as stated in its row.

## Revision 1 — repair round 1

Commit: 2f5432b931881d400e2dfbe818c729c2670b27c4
Verdict: findings (#948: findings; #949: pass; #954: pass; #1390: findings; #888: pass)

My prompt carried no intended fix, suspected cause, or hoped-for verdict. I changed nothing except this handover.

### Scope

This revision audits the repair only, against revision 0 (`6555cd1`).

- State: the head is `2f5432b`. `origin/main` and `git ls-remote origin refs/heads/main` are both `3973631`, merged as `7a040e2`.
- The repair is `6555cd1..3642dd8` (specs, tests, three production files) plus the spec author's revision 6 (`4253cc4`, `35a852c`).
- The merge overlaps the run's files only in `sdd/specs/requirement-ids.txt`, which adds the run's three IDs at their sorted positions. The merged `ReihitsuFormatterHelpers` and `BlankLineTokenCleanupRewriter` changes (byte order mark) do not touch the predicates of this run.
- `git diff origin/main -- documentation` is empty: the rule pages are as on `origin/main`, and the withheld fix is now recorded in `sdd/specs/analyzer/RH3202.md` and `RH3208.md`.
- Answers taken from `00-lead.md`: A4 → B (comments only), A5 → A (move nothing when the arrow carries a comment), A7 → A (SHARED-CHAIN-005 exception), Q4 → B.
- How I checked: static reading of the repair diff, probes in the scratchpad (`format.sh` twice, `trace.sh`), and a scratch `net10.0` build for the C# claims. Evidence from revision 0 is reused where the code is byte-identical: `TokenGapNormalizer`, `ChainLineBreakRewriter`, `LineBreakInitializerRewriter`, `ExpressionBodiedTransformUtilities`, and the code-fix guards are unchanged.

### Previous findings

| # | Issue | Status | Evidence |
|---|---|---|---|
| A1 | #949 | closed | `BuildTerminalBreakLeadingTrivia` no longer appends a line break behind trivia that ends with one (`SwitchCaseBraceRewriter.cs:584`). `trace.sh` on the revision-0 input settles in pass 1 (pass 2: no change). `DocumentationCommentBeforeTerminalBreakIsLaidOutInThePassThatInsertsTheBraces` pins it with the two-pass helper, LF and CRLF. |
| A2 | #948 | open (narrowed) | `IsBoundInsideSection` and the `QueryBodySyntax` stop fix both revision-0 inputs, and tests pin them. Other shadowing forms inside a lambda still trigger the guard, so the switch stays unbraced although the local is used only in its own section, which FMT-SWITCH-014 still forbids. Compiled counterexamples: section 2 `Action a = () => { foreach (var y in ys) { Use(y); } };`, and section 2 `Func<object, int> f = o => o switch { int y => y, _ => 0 };`, each next to section 1 `int y = 1; Use(y);`. Both compile braced and unbraced, and the head leaves both switches unbraced. The implementer's revision 2 names this residual on purpose; no spec text allows it. |
| A3 | #948 | closed | FMT-SWITCH-014's third bullet names the local-declaration initializer. `OutVariableOfLocalDeclarationInitializer…` and `PatternVariableOfLocalDeclarationInitializer…` pin the branch. |
| A4 | #954 | closed (answer B) | Only comments changed in `BlankLineEditor.cs`. Both summaries now state the start-of-file exemption and the conditional-directive exemption behind `{`, as the code does. |
| A5 | #1390 | closed (answer A) | `ArrowLineHoldsComment` turns off every move, including the `return /* c */` join, when the arrow's trailing trivia, or its line in front of it, holds a comment. Every revision-0 input and four more probes keep the source order and are stable in two passes, LF and CRLF. The new tests assert order and position without pinning the unspecified layout (adequate for the spec as written). |
| A6 | #1390 | closed | Renamed to `BlockCommentLeadingNextLineExpressionJoinsReturnKeyword`; the assertion is unchanged. |
| A7 | #888 | closed (answer A) | SHARED-CHAIN-005 carries the exception and an example. The `!.`, `/** */`, prefix, first-invoked, and no-invoked-link tests and the RH5201 parity test pin the behavior that revision 0 had probed. |

### thoenissen/Reihitsu#948

#### Findings

| # | Severity | Location | Failure class | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|
| A8 | high | `src/Reihitsu.Formatter/Pipeline/SwitchCaseBraces/Rewriter/SwitchCaseBraceRewriter.cs:222`, the `ParenthesizedLambdaExpressionSyntax` and `LocalFunctionStatementSyntax` arms at `:237` and `:240` | FC-001, FC-006 | PR-introduced (repair round 1) | `IsBoundInsideSection` treats every simple name under a lambda or local function as bound once the function has a parameter of that name. Names in the function's attribute lists and parameter default values are outside the parameter's scope, though, and C# binds them to the switch-scoped local. Counterexample (scratch build): section 1 `const int y = 2;` / `Use(y);`; section 2 `[My(y)]` / `int L(int y) => y;` / `Use(L(1));`. The same happens with `int L(int y, int b = y) => b;`. Unbraced, both compile. The head braces both switches, and the output fails with CS0103 at the attribute argument and at the default value (FMT-PIPE-003). At revision 0, any `y` in section 2 kept the switch unchanged. | Bind a parameter only for names inside the function's body (`Body` or `ExpressionBody`), not in its attribute lists, parameter list, return type, or constraint clauses. Test implementer: a test for each of the two shapes. |
| A2 | medium | `SwitchCaseBraceRewriter.cs:222` | FC-016, FC-002 | same requirement | Still open; the counterexamples are in the table above. Not yet recognized as binders: a `foreach`, `for`, `using`, `fixed`, or `catch` variable, a `while` or `do` condition designation, a switch-expression arm designation, and the sections of a nested switch, each inside a lambda or local function. | Extend the binder walk to these constructs. Or the maintainer accepts that unrecognized shadowing forms keep the switch unbraced, and FMT-SWITCH-014 says so; that is a spec change. |

#### Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| A name in another section is bound inside it (`IsBoundInsideSection`) | Ancestors of the name up to the section: parameters of lambdas, anonymous methods, and local functions; designations in lambda and local-function expression bodies; direct locals of enclosing blocks | C# binding (compiled) | Too wide for parameters: the parameter's scope is the function body, but the predicate inspects the whole function node, attributes and defaults included (A8). Too narrow for the other binders inside a lambda (A2; safe, since the output still compiles) | A8, A2 |
| Query clauses declare nothing directly (`GetDesignatedNames` stops at `QueryBodySyntax`) | Everything behind the first `from` expression | C# clause scope | Agrees. The first `from` expression still counts, and must: `var q = from a in xs.Take(F(out var o)) select a;` with `o = 2;` in another section compiles unbraced (scratch build), and the head leaves that switch unchanged | Agrees |

#### Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `LambdaParameterShadowing…`, `LocalFunctionParameterShadowing…`, `PatternVariableInQueryClause…DoesNotBlockTheBraces` | Exact braced output, two passes, LF and CRLF | The switch left unbraced | `AssertRuleResult` | Adequate; the A8 shapes are not tested |
| `OutVariableOfLocalDeclarationInitializer…`, `PatternVariableOfLocalDeclarationInitializer…` | Fixed point | A brace inserted | `AssertRuleResult(input)` | Adequate |

#### Failure classes dismissed

- FC-011: `IsBoundInsideSection` walks a finite ancestor chain.
- FC-015: the narrowing reuses `GetDirectlyDeclaredLocalNames` and `GetDesignatedNames`; no second scope policy was found by rg (only `RH4001TypeNameShouldMatchFileNameHelper`, which handles type names).
- FC-031: the summaries describe the narrowing, except that the `IsBoundInsideSection` summary implies any parameter of an enclosing function binds the name, which is A8 itself.
- The other classes are unchanged from revision 0.

### thoenissen/Reihitsu#949

The A1 repair also gives a doc comment followed by a blank line and another comment the expected result: the blank line is kept as a pending blank line. `BlankLineTriviaUtilities.EndsWithLineBreak` is false for `//`, `/* */`, and `/** */`, so those paths are unchanged. No new finding. `BuildLeadingTrivia` (the first statement) keeps the hazard, which stays a follow-up candidate, as revision 0 noted.

### thoenissen/Reihitsu#954

Only comments changed. The new summaries match the code: the start of the file is exempt whatever trivia comes first, and behind a `{` without a trailing comment, whitespace, line breaks, conditional-compilation directives, and disabled text keep the exemption. No new finding. Revision 0's conditional-directive and start-of-file divergences from RH5031 remain follow-up candidates, as answered.

### thoenissen/Reihitsu#1390

#### Findings

| # | Severity | Location | Failure class | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|
| A9 | low | `sdd/specs/formatter/structural-transforms.md:259` (FMT-STRUCT-006) and `src/Reihitsu.Formatter/Pipeline/StructuralTransforms/Utilities/ExpressionBodyToBlockConverter.cs:90` | FC-002 | PR-introduced (spec revision 4) | The spec turns off the move when "the arrow's line holds a comment, in front of the arrow or behind it". `ArrowLineHoldsComment` looks only at the arrow's own trivia. So a comment further behind on the arrow's line, such as `int M()` / `// a` / `=> 42; // t` or `=> 42 /* t */;`, still lets `// a` move into the block. The output is `{` / `// a` / `return 42;` / `} // t`, in source order and stable in two passes, which matches the spec's `Why:`. It contradicts the literal MUST NOT, though. | Spec author: name the comment as one directly at the arrow, in front of it on its line or in its trailing trivia, which is what the examples show and the code checks. This is a text change only. |

#### Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Move no comment (`ArrowLineHoldsComment`) | The arrow's trailing trivia, and its leading trivia back to the last line break (or a trivia that ends with one) | What lands on `{` (`ExtractSignificantTrivia(arrowToken.TrailingTrivia)`), and the arrow-line check in `TrySplitOwnLineComments` | Agrees with both: every comment that could end up in front of the moved comments is seen. Disagrees with the spec text's wider "arrow's line" (A9; order still preserved) | A9 |
| `return /* c */` join turned off (`CreateReturnStatement` with `moveComments`) | Expression leading trivia | The same flag | Agrees. With a comment on the arrow, the output equals `origin/main`'s placement (probes `M1`, `M6`, `M7`), in source order | Agrees |

#### Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `OwnLineCommentStaysOutOfTheBlockWhen…` (3), RH3202 `VerifyFixKeepsCommentOrderWhenTheArrowLineCarriesAComment` | Converted; each comment exactly once; source order; the own-line comment is not between `{` and the statement; second pass unchanged; LF and CRLF | Reversed order, a lost or duplicated comment, a moved own-line comment, or a pass-2 change | `AssertCommentsKeepOrderOutsideTheBlock`; `ApplyCodeFixAsync` for RH3202 | Adequate for a layout the spec leaves unspecified |

### thoenissen/Reihitsu#888

No production change. The SHARED-CHAIN-005 exception and FMT-BREAK-019's sentence match `EnsureLineBreak`'s three call sites, and the first link stays excluded (Q4 → B). The new tests use the exact-output two-pass helper and `VerifyFormatter`. No new finding.

### Whole run

- Blocking: A8 (high), A2 (medium, open), A9 (low).
- Routing: A8 to the implementer and the test implementer. A2 to the implementer, or to the maintainer for a spec change. A9 to the spec author (text only).
- Not a finding (hint): revision 0's hints stand. `int A4()` / `// a` / `/* c */ => 42;` now keeps `origin/main`'s `/* c */ {` layout. It is in order and stable, but odd, and it belongs to thoenissen/Reihitsu#963 by the A5 answer.
- I re-ran no repository test. The implementer's full formatter and analyzer runs (`780162b`) and the cleaner's runs after the merge stand. Every finding rests on a probe or a scratch build named in its row.

## Revision 2 — repair round 2

Commit: 6dbf0451d1020057a69e117936b755adb0e2e768
Verdict: findings (#948: findings; #949: pass; #954: pass; #1390: findings; #888: pass)

My prompt carried no intended fix, suspected cause, or hoped-for verdict. I changed nothing except this handover.

### Scope

This revision audits the repair only, against revision 1 (`2f5432b`).

- State: the head is `6dbf045`. `origin/main` and `git ls-remote origin refs/heads/main` are both `3973631` and already merged; nothing was merged since revision 1.
- The repair is:
  - `SwitchCaseBraceRewriter.cs`: `IsBoundInsideSection` now hands each ancestor and the child it was reached from to the new `IsBoundByAncestor`;
  - FMT-STRUCT-006 wording (spec author revision 7);
  - 14 new or changed tests in `SwitchSectionSharedLocalTests`;
  - 4 new tests in `ExpressionBodyArrowCommentTests`.
- Every other production file is byte-identical to revision 1, and so is its evidence.
- How I checked: static reading. I compiled probes in a scratch `net10.0` library, unbraced and as the head formats them, and ran `format.sh` twice. I re-ran no repository test.

### Previous findings

| # | Issue | Status | Evidence |
|---|---|---|---|
| A8 | #948 | closed | A parameter or expression-body designation of a lambda, anonymous method, or local function now binds only names in that function's `Body` or `ExpressionBody`. On the head, revision 1's two inputs (`[My(y)] int L(int y)` and `int L(int y, int b = y)`) leave the switch unchanged, and the output compiles. So does a parameter attribute, `([My(y)] int y) => y` (probe). Four tests pin the attribute and default-value shapes of a lambda and a local function. |
| A2 | #948 | closed | `foreach`, `for`, `using`, `fixed`, `catch`, `while`, `do`, switch-expression arms, and nested switch sections now bind inside their C# scope. Revision 1's counterexamples (a `foreach` variable and a switch-expression arm, both in a lambda) are now braced, and the output compiles. Nine tests pin these binders, plus one already-passing nested-switch test. Unrecognized forms still err toward leaving the switch unbraced, which the implementer documents. The narrower `do` scope (the condition only, although C# reports CS0841 for a body use as well, so the scope covers the body too) is on the safe side. |
| A9 | #1390 | open (narrowed, text only) | See A9 below. |

### thoenissen/Reihitsu#948

#### Findings

| # | Severity | Location | Failure class | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|
| A10 | medium | `SwitchSectionSharedLocalTests.cs`; the child conditions at `SwitchCaseBraceRewriter.cs:269` (nested `switch`) and `:271` (`foreach`) | FC-003, FC-017 | PR-introduced (repair round 2) | The new scope checks are tested on one side only, except for lambdas and local functions. The tests cover names inside each binder's scope; none covers a same-named use just outside it within the same construct. Two such uses are load-bearing. (1) A `foreach` collection expression: section 1 `const int y = 2;` / `Use(y);`, and in section 2's lambda `foreach (var y in new[] { y }) { Use(y); }`. (2) A nested switch's governing expression: section 2's local function `switch (w + y) { case 1: int y = 3; Use(y); break; }`. Both compile unbraced, and C# binds the outer `y` to section 1's const. The head handles both correctly: the outer switch stays unbraced, and the output compiles. But deleting `child == forEachStatement.Statement` or `child is SwitchSectionSyntax` would keep the suite green while the formatter braces both into CS0103, which is the A8 failure for these binders. | Test implementer: two fixed-point tests (`AssertRuleResult(input)`), one for each input above. The `for`, `using`, `fixed`, `catch`, `while`, and `do` conditions have no compiling outside-scope use (CS0841, compiled for `for` and `do`), so they need none. |
| A11 | low | `SwitchCaseBraceRewriter.cs:217` and `:222` (`IsBoundInsideSection` summary) | FC-031 | PR-introduced (repair round 2) | Two items of the summary do not match the code. The nested-switch item says "for a name in that block or switch", but the code binds only names in the switch's sections, not in its governing expression; that is exactly the A10 input (2). The arm item names only "the pattern", but `IsBoundByAncestor` also binds the `when` clause's designations. | Name "a section of that switch", and add the arm's `when` clause. Comment only; it does not reach compiled behavior. |

#### Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| A declaration of an ancestor binds the name (`IsBoundByAncestor`) | The ancestor, plus the child it was reached from. Functions: `Body`/`ExpressionBody`. `foreach`/`using`/`fixed`: the embedded statement. `for`: condition, incrementors, statement. `catch`: block and filter. `while`: condition and statement. `do`: condition. A block or a nested switch's sections: the whole block or the sections. An arm: the whole arm | C# scope (compiled probes) | Agrees, or is narrower, for every binder probed. Narrower means the switch stays unbraced and compiles: `for` declaration, `do` body, using-expression and `for`-condition designations, arm-expression designations, query range variables. No false negative found. Compiled probes: the `foreach` collection, an arm expression using the local, a nested switch governing expression, a lambda parameter attribute, the A8 inputs, and the revision-1 A2 inputs | Agrees; the outside side is untested (A10) |

#### Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| A8 tests: attribute and default value on a local function, default value on a lambda | Fixed point | A brace inserted | `AssertRuleResult(input)` | Adequate |
| `AttributeArgumentOfLambdaUsesTheLocalAndLeavesTheSwitchUnchanged` | No section braced, idempotent, LF and CRLF | `{` on the line after a `case …:` line, or a pass-2 change | `AssertSwitchSectionsStayUnbraced` (regex with `Regex.InfiniteMatchTimeout`) | Adequate for the invariant; the lambda's attribute layout is unspecified |
| A2 tests (10) | Exact braced output, two passes | The switch left unbraced | `AssertRuleResult(input, expected)` | Adequate; the outside side is missing (A10) |

#### Failure classes dismissed

- FC-011: the ancestor loop ends at the section or the root.
- FC-015: the binder walk reuses `GetDirectlyDeclaredLocalNames` and `GetDesignatedNames`, and no second owner exists.
- FC-026: children are compared by reference within one red tree, and Roslyn caches the child nodes, so the comparison is stable.
- FC-006: every probe output compiles.

### thoenissen/Reihitsu#1390

#### Findings

| # | Severity | Location | Failure class | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|
| A9 | low | `sdd/specs/formatter/structural-transforms.md:259` (FMT-STRUCT-006) | FC-002 | same requirement | The new definition counts a comment that "precedes the arrow on the arrow's line, behind the last line break in front of the arrow" as standing at the arrow. Read literally, that includes a comment in the trailing trivia of the token in front of the arrow on the same line: `int M() /* c */ =>` / `// a` / `42;`. In that case the spec says `// a` MUST NOT move. `ArrowLineHoldsComment` looks only at the arrow's own trivia, so the head moves it: `int M() /* c */` / `{` / `// a` / `return 42;` / `}`. The output keeps source order and is stable in two passes, which matches the `Why:`. The spec author's revision 7 notes this input as "not decided by the text (unverified)", but the text does decide it. | Spec author: define "in front of the arrow" as on a line that holds no token in front of the arrow, as the example `/* c */ => 42;` shows and the code checks. Text only. |

#### Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `CommentBehindTheExpression…DoesNotStopTheMove…` (2) | Exact output, two passes | `// a` left outside the block | `AssertRuleResult` | Adequate |
| `CommentBehindTheSemicolon…DoesNotStopTheMove…` (2) | The own-line comment sits between `{` and the statement; the trailing comment appears once, behind the statement; idempotent; LF and CRLF | Comment not moved, duplicated, or reordered | `AssertOwnLineCommentLeadsStatementAndTrailingCommentFollows` | Adequate for the part the spec decides |

### thoenissen/Reihitsu#949, #954, #888

No change since revision 1. No new finding.

### Whole run

- Blocking: A10 (medium).
- Text only: A11 (a source comment) and A9 (spec wording). Neither reaches compiled behavior.
- Routing: A10 to the test implementer. A11 to the implementer, as a comment-only edit. A9 to the spec author.
- Hints: revision 0's and revision 1's hints stand. `int K()` / `// a` / `/* c */ =>` / `// b` / `42;` keeps `origin/main`'s `/* c */ {` and `return` / blank / `// b` / `42;` layout. It is in order and stable, and per the A5 answer it belongs to thoenissen/Reihitsu#963.

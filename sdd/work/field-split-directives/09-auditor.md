Role: auditor
Commit: cce9f0a41fc49137702ae1a5393975b9912afadd
Verdict: #1728: findings; #1586: pass

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

Full audit. Head cce9f0a (the checkout is c56d396, which differs from cce9f0a only in `sdd/work/field-split-directives/00-lead.md`); base `origin/main` = eb89003, which is an ancestor of the head. The diff matches handovers 01–08: four production files (`PrecheckPhase.cs`, `FieldDeclarationSplitTransform.cs`, `ReihitsuFormatter.cs` doc only, `RH7101DoNotCombineFieldsCodeFixProvider.cs` doc only), the specs and the change doc, and the tests listed in 05. `scripts/lane-check.sh behavior-change`: confirmed. The two issues share one mechanism, so the tables below are written once under #1728 and referenced from #1586.

Executed (fixtures in the scratchpad, outside the repository):
- `scripts/format.sh --check` on directives that two shapes refuse at the same trivia: a directive in front of a comma of an attribute argument list of a combined field, a directive behind `?` in a value, and a conditional group in front of a comma of a call in a value. All three gave FMT-UNSUP-010's reason, which matches FMT-UNSUP-010 ("Every other directive in the declaration gives this requirement's reason").
- `scripts/format.sh` twice, then `--check`, on a region-led combined field next to a nested type whose attributed combined field sits inside `#if !RELEASE`, and on an *inactive* `#pragma` in front of a combined field (directive trivia, no disabled text). Both were split, and the second pass changed nothing.
- `scripts/apply-fix.sh RH7101` on those two fixtures, plus a `#define D` file with an active `#endif` in front of `_m, _n` and `_r, _s; private int _t;` on one line inside a `#region`. Result: 6 fixed, 0 not-converged, 0 line-ending drift.

## thoenissen/Reihitsu#1728

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | medium | `src/Reihitsu.Formatter/Pipeline/Precheck/PrecheckPhase.cs:89-103` (candidate order); `src/Reihitsu.Formatter.Test/Unit/Precheck/UnsupportedFieldDeclarationDirectiveTests.cs` | FC-003, FC-017 | – | PR-introduced | The diff adds an explicit tie order. In the non-declarator list, `FieldDeclarationDirectiveReason` goes ahead of `ListSeparatorCommentReason`; in both lists it goes ahead of `ConditionalOperatorCommentReason`. This order is what makes FMT-UNSUP-010's reason win over FMT-UNSUP-009 at a non-declarator comma (for example `[Obsolete("m"⏎#pragma warning disable CS1030⏎, false)] private int _a, _b;`) and over FMT-UNSUP-008 (`private int _a = true ?⏎#pragma warning disable CS0162⏎1 : 2, _b;`). That is visible reason text in the CLI report, and FMT-UNSUP-010 decides it. No test pins either tie: moving `firstFieldDirective` behind the separator or operator candidate passes every test in the run (the implementer noted the gap in 06). The current code gives the specified reason; I checked both inputs with the CLI. | Add one `AssertUnsupportedFor(FieldDeclarationDirectiveReason, …)` test per tie: a directive in front of a comma on a later line inside an attribute argument list (or a call in a value) of a combined field, and a directive behind `?` or `:` in a value of a combined field. |
| 2 | low | `AGENTS.md`, *Formatter phase ownership*, `StructuralTransformPhase` row | FC-031 | – | PR-introduced | The row's guard list dropped `FieldDeclarationSplitTransform.CarriesDirective` but does not name the guard the split keeps, `FieldDeclarationSplitTransform.HasDisabledTextInLeadingTrivia`. 03-spec-author.md handed this to the implementer ("adds its name there if it becomes a named member"), and it is now a named member. Text only; it cannot reach compiled behavior. | Name `FieldDeclarationSplitTransform.HasDisabledTextInLeadingTrivia` among the per-rewriter guards of that row. |

### Requirements checked
- FMT-UNSUP-010: `FindDirectiveInFieldDeclaration` covers exactly the specified span. It acts only on `FieldDeclarationSyntax` (event fields and locals are excluded) with `Variables.Count >= 2` (the declarators of the parsed build), and checks the leading trivia of every token but the first and every trailing trivia, initializer values included, with no value exception. All examples, and every input class in the skeptic's matrix (position × kind × count × owner × entry point × co-occurrence), are covered by tests in `UnsupportedFieldDeclarationDirectiveTests`. The checking of the `;`'s trailing trivia is harmless: a directive or disabled text is never trailing trivia.
- FMT-UNSUP-009 tie at a declarator comma: `isDeclaratorComma` puts the separator candidate ahead of `firstFieldDirective`. Tested (`DirectiveInFrontOfADeclaratorCommaOnALaterLineKeepsTheReasonOfAListSeparator`, `ThirdDeclaratorInAConditionalGroupIsUnsupportedInEveryBuild`, `DeclaratorInDisabledTextDoesNotCount…`). The global flag is sound: `FindTriviaInFrontOfComma` returns the first qualifying gap in text order, so a tie can only occur at that gap.
- FMT-UNSUP-001 (first in text), FMT-UNSUP-002 (all entry points), FMT-UNSUP-003 (part of a declaration is not refused), FMT-UNSUP-005 (closure), FMT-UNSUP-007 (`FindUnsupportedSyntax` = precheck): held and tested.
- SHARED-FIX-010 / change doc *Code fixes on code that holds the shape are withheld*: I traced every fix the change doc names to the node it asks about. RH7101 asks about `fieldDeclaration.Parent`, the node it passes as context. RH5110 and RH5404 ask about the type. RH5113 asks about the declaration. RH5111 on a declarator asks about `GetFormattingContextNode`, which is `variableDeclarator.Parent.Parent`, the field. RH5112 and RH5201 ask through `FluentChainCodeFixHelper.IsFormattingRefused`, whose context is the nearest `StatementSyntax`/`MemberDeclarationSyntax`, the field. Every call of `FormatNodeInDocumentWithContextAsync` in the code fixes asks about its context (`rg`).
- Change doc *Special cases → Deleted*: all three entries are gone from the diff (`CarriesDirective` with its operand and the `SplitFields` comment; the `SplitsFieldDeclaration` documentation; FMT-STRUCT-023 with its mentions in FMT-STRUCT-024, FMT-PIPE-029, FMT-PIPE-031, SHARED-FIX-011; RH7101-005 now withholds through SHARED-FIX-010). `rg "FMT-STRUCT-023|CarriesDirective"` outside history and the retired heading finds nothing.
- The issue's goals: the shape is declared, and the guards are deleted except the disabled-text check (decision 2, maintainer Q1 A). RH7101-005 is changed in place rather than deleted (decision 1, internal).

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Refuse a directive or disabled text in a combined field (FMT-UNSUP-010) | `fieldDeclaration.DescendantTokens()`: leading trivia of every token but the first, trailing trivia of every token | `FieldDeclarationSplitTransform.HasDisabledTextInLeadingTrivia`: `fieldDeclaration.GetLeadingTrivia()` (the first token, including zero-width) | The two spans are disjoint and together cover all of the declaration's trivia. The precheck's first token is `DescendantTokens().First()`, the same token `GetLeadingTrivia` reads. Zero-width tokens occur only with syntax errors, which return earlier (FMT-PIPE-005) | ok |
| Keep a combined field whole for disabled text in front (FMT-STRUCT-022) | `DisabledTextTrivia` in the first token's leading trivia | `ReihitsuFormatter.SplitsFieldDeclaration` → `Splits` (FMT-PIPE-031), the RH7101 fix | One owner, delegated | ok |
| Split with a directive in front (FMT-STRUCT-022) | Directive trivia is admitted by kind. Inactive non-conditional directives inside a disabled branch are directive trivia, not disabled text, and are split. They are positional (`#pragma`, `#nullable`, `#region`), so they apply to all fields after the split; checked by CLI and RH7101 run | — | — | ok |
| Report RH7101 (RH7101-001) | Conditional directives between the first and last declarator's `SpanStart` | Unchanged and not a split decision; the change doc lists it under *Checked and kept* | Different decision (whether to report) | ok |
| Tie order of reasons (FMT-UNSUP-009/010, 008/010) | Candidate list order at an equal `FullSpan.Start` | FMT-UNSUP-009's tie paragraph, FMT-UNSUP-010's last paragraph | Agrees (CLI run); untested, see #1 | finding #1 |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Unsupported shapes | `PrecheckPhase.FindUnsupportedSyntax` (`rg "FindUnsupportedSyntax"`); `ReihitsuFormatter.FindUnsupportedSyntax` delegates; 27 code-fix call sites ask the latter | one owner |
| Whether a field declaration is split | `FieldDeclarationSplitTransform.Splits`; `ReihitsuFormatter.SplitsFieldDeclaration` delegates; callers `RH7101DoNotCombineFieldsCodeFixProvider.TryGetFixableFieldDeclaration`, `FormatNodeInDocumentWithContextAsync` (`rg "Splits\(|SplitsFieldDeclaration"`) | one owner |
| Disabled text in a declaration's leading trivia | `rg "DisabledTextTrivia" src` (no tests): only `FieldDeclarationSplitTransform.HasDisabledTextInLeadingTrivia` states it for a declaration. Core's `SyntaxTriviaUtilities` 463/475, `OrderingDeclarationUtilities` 597/857, `ModifierOrderingUtilities` 376, and `SyntaxIndentationUtilities` 568 serve other decisions | one owner |
| Directive-or-disabled-text trivia | `SyntaxTriviaUtilities.IsDirectiveOrDisabledTextTrivia`, reused by the precheck | reused, no copy |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `UnsupportedFieldDeclarationDirectiveTests.*` refusal rows | Input refused, unchanged, with reason and exact span | Formatted output, other reason, other location | `AssertUnsupportedFor` (LF and CRLF, byte-exact text, reason, `AreSame` tree, span) | adequate |
| `DirectiveInTheLeadingTriviaOfTheFirstTokenIsSupported`, `EventFieldAndLocalDeclarationAreSupported`, `DirectiveInsideAFieldWithOneDeclaratorIsSupported` | Not refused, and stable | Refusal; second pass or phase changes output | `AssertSupported` (LF/CRLF, second pass, `PhaseStabilityCheck`); exact split output is pinned in `FieldDeclarationSplitTests` | adequate |
| `NodeLevelEntryPointsRefuseATargetThatHoldsTheShape`, `TargetThatHoldsOnlyPartOfTheDeclarationIsNotRefused`, `FindUnsupportedSyntax*` | FMT-UNSUP-002/003/007 per entry point | A target refused or formatted wrongly; query disagrees with the precheck | Direct entry-point calls, LF/CRLF | adequate |
| `OutputOfFormattedDeclarationsIsNotUnsupported` | FMT-UNSUP-005 | Output refused on the second pass | `FormatSyntaxTree` twice, LF/CRLF | adequate |
| RH7101 / RH5113 / RH5110 withheld tests | No action offered; diagnostic stays | An action is registered | `VerifyNoCodeFixIsOffered` | adequate |
| `RH5303…FixIsOfferedForAnInitializerInsideACombinedFieldWithADirective…` | A value-only fix is not refused (FMT-UNSUP-003) | Fix withheld, or pragma or declarator lost | `TryApplyCodeFixAsync` with exact text | adequate |
| CLI `DirectiveBetweenTheDeclaratorsOfAFieldIsReportedAsUnsupported`, `DirectiveInsideAValueOfACombinedFieldIsReportedAsUnsupported` | File unchanged, report line with location, exit code | Rewritten file; other line or exit code | `Program.Main` | adequate |
| Tie order with FMT-UNSUP-009 at a non-declarator comma, and with FMT-UNSUP-008 | FMT-UNSUP-010's reason wins | — | missing | finding #1 |

### Failure classes dismissed
- FC-004, FC-021: the refusal moves nothing, and the split side is covered under #1586.
- FC-005: the precheck is a separate predicate for directives and does not reuse a comment guard.
- FC-006, FC-022–FC-026, FC-029, FC-036, FC-018: no new rewrite of semantics, literals, identifiers, members, or names. A directive cannot appear inside a literal.
- FC-007–FC-009: the RH7101 fix converges (apply-fix: 6/6, 2 iterations for Fix All of two diagnostics). FC-043 checked above.
- FC-010–FC-012: no phase changed; the refusal returns the input.
- FC-013, FC-014: RH7101 still reports what the formatter refuses or keeps whole. That is a diagnostic without a fix, a supported end state.
- FC-015, FC-042: single owners (table above). No guard that serves only the declared shape survives in code or specs: `DeclarationSemicolonLineBreakRewriter` serves every declaration kind, and no other formatter component handles `FieldDeclarationSyntax` and directives (`rg FieldDeclarationSyntax src/Reihitsu.Formatter`).
- FC-016: event fields and locals are deliberately excluded (the change doc: a wider shape deletes nothing).
- FC-019, FC-020: directive kinds `#if`/`#elif`/`#else`/`#endif`, `#if false`, `#pragma`, `#region`, `#nullable`, and empty groups are covered in every gap.
- FC-027, FC-039: values with nested lambdas and collection expressions and nested types are tested. A field declaration cannot occur inside a value.
- FC-028, FC-041: the precheck reads only `DescendantTokens()` of the field inside its input. Detached, target, and context runs are tested.
- FC-032, FC-035: expected values come from the spec. Results were gathered on the final production commit (f5afc3a; later commits only change `sdd/work`).
- FC-033, FC-034: the issue's exact input (`int a,⏎#if X⏎b,⏎#endif⏎c;`) is the spec example and the CLI test.
- FC-037: no comment-only claim.
- FC-038: the split never writes trivia into a later token's leading trivia or into a separator gap (`CreateSplitField` clears both), and a kept-whole declaration keeps its trivia. Tested.
- FC-040: directives do not depend on the documentation mode; all three modes are tested.
- FC-044: the kept-whole check is by trivia kind and does not model which build compiles what.

### Architecture findings
_None._

### Hints
- `PrecheckPhase` now has three helpers that differ only in their predicate: `FindFirstComment`, `FindFirstUnsupportedOperatorTrivia`, `FindFirstDirectiveOrDisabledText`. A shared "first trivia matching a predicate, or null" would remove the repetition. Cosmetic.
- FMT-UNSUP-009's second tie bullet ("Every other comma in the span … give this requirement's reason") is written under a lead sentence about comments, while FMT-UNSUP-010 explicitly takes every other directive. The two read consistently, but the bullet could name FMT-UNSUP-010 for a directive the way the first bullet does.

### Follow-up candidates
_None_ beyond 00-lead.md's.

## thoenissen/Reihitsu#1586

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| – | – | – | – | – | – | None | – |

### Requirements checked
- FMT-STRUCT-022: the leading trivia stays on the first field, and later fields get empty leading trivia (`CreateSplitField`), so a `#region`/`#if` is not duplicated in front of a repeated attribute list. Checked with an attributed field inside `#if !RELEASE`: one `#if`, attributes repeated. Disabled text in the leading trivia keeps the declaration whole: `HasDisabledTextInLeadingTrivia`; L3, L4, L5, L6, L9, and `#if false` are tested on the formatter side and the RH7101 side.
- FMT-PIPE-031: `SplitsFieldDeclaration` = `Splits`, and both examples are tested.
- RH7101-005: the region and `#if !RELEASE` example is fixed (LF, CRLF, Fix All); the disabled-text example has no fix. In `FormatSplitFieldInContext`, the line-sharing check (`AreSeparatedByEndOfLine`) cannot misfire behind a directive, because a directive always follows an end of line in the previous token's trailing trivia. Confirmed by apply-fix on a directly preceding `#endif` and `#region`.
- Issue "Done when": tests for a region (`SpecExampleRegionAndGroupAroundTheDeclarationAreSplit`, RH7101 LF/CRLF), an `#if` (same, plus `VerifyFixSplitsAFieldInsideAnActiveConditionalGroup`), and a directive between declarators (now refused: #1728's tests). The goal "specified in FMT-STRUCT-023" moved to FMT-STRUCT-022 (internal).

### Visible output is decided (both issues)
| Changed expected value | Covered by |
|---|---|
| Refusal and CLI report for a directive inside a combined field; fixes withheld (RH7101, RH5113, RH5110) | Q2 A (`00-lead.md`), change doc *Decisions → Declare the shape* |
| Split behind a region, a group, a pragma, or an active `#endif`, by the formatter, the RH7101 fix, and the CLI | Q1 A, change doc *Decisions → The leading trivia of the first token is not part of the shape* |
| RH5110 fix splits a region-led combined field of the type | The fix's result is the formatter's output for the type it formats. The change doc lists it under *Visible output changes*, and Q1 A decides the formatter's split |
| Kept whole and no fix for disabled text in front | `main`'s output; Q1 A and decision 2 |
| RH5303 fix on a value inside a refused declaration | `main`'s output (FMT-UNSUP-003, unchanged) |

No rule page changed.

### Guard scope, Policy ownership, Assertion adequacy, Failure classes dismissed
See #1728 (one shared mechanism). Additionally, for the split side: `SpecExampleDisabledTextInTheLeadingTriviaKeepsTheDeclarationWhole`, `DisabledTextInTheLeadingTriviaKeepsTheDeclarationWhole`, `PragmaDirectiveInFrontOfTheDeclarationStaysOnTheFirstField`, `DeclarationInTheActiveBranchInFrontOfADisabledElseBranchIsSplit`, `RegionInFrontOfAnAttributedDeclarationStaysInFrontOfTheFirstField`, and `RegionAndCommentInFrontOfTheDeclarationStayInFrontOfTheFirstField` use `AssertRuleResult`, which checks exact output under LF and CRLF, a second pass, and phase stability. The `ReturnsTrue…`/`ReturnsFalse…` rows pin both sides of the leading-trivia boundary (FC-001, FC-003). Adequate.

### Architecture findings
_None._

### Hints
_None._

### Follow-up candidates
_None._

## Whole run

- `scripts/lane-check.sh behavior-change`: confirmed.
- No `TODO` and no tracker reference in the added source lines.
- Scope: no edit outside the run's issues, specs, and tests.
- Fixtures and outputs were kept in the scratchpad only.

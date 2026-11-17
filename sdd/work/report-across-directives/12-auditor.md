Role: auditor
Commit: e6621499ee79ba874c7628ef689ad83b6554efa6
Verdict: pass

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

Full audit of thoenissen/Reihitsu#1759 (`agent-filed`), lane behavior change. Head `e662149` (checked out, clean tree). `git ls-remote origin refs/heads/main` = `fe1d12215a0ad854e17d40957b169659641545b7`, the SHA in the prompt, and an ancestor of HEAD (merged). Diff: `git diff fe1d122...HEAD`, 103 files; the production part (24 files, guards removed from 12 analyzers and 2 bases, RH7311 switched to `IsActive`, two new fix guards, five unused `Reihitsu.Core` helpers deleted) matches `05-implementer.md`; specs, change doc, and rule pages match `01`/`03`/`06`/`10-spec-author.md`; tests match `04`/`07-test-implementer.md`. `scripts/lane-check.sh behavior-change`: confirmed.

Inputs: the issue, `00-lead.md` (maintainer answers 1A–6A, decisions 1–8), handovers `01`–`11`, the change doc `sdd/changes/2026-11/report-across-conditional-directives.md`, the changed specs, and the counterpart files named below, whether or not in the diff.

Executed checks (fixtures in the scratchpad, outside the repository):

- `scripts/format.sh` twice on `M(1, 2, #if DEBUG 4, #endif 3)`, `M(1, #if NEVER 4, #else 5, #endif 2, 3)`, and `void P(int a, int b, #if DEBUG int d, #endif int c)` (excluded elements, not only empty blocks): every list is broken one element per line, second pass `Formatted 0 of 1`. RH5102/RH5109 report nothing the formatter keeps (FC-013).
- `scripts/apply-fix.sh RH7107` on three accessor lists the old analyzer guard kept silent: an active `#if !NEVER` group around the moved `get` → `no-fix-offered` (LF, CRLF); a `#if`…`#endif` group around both accessors → `no-fix-offered`; an excluded `init` in a balanced pair in front of `get` → `fixed`, the pair moves with `get` (SHARED-ORDER-006). Accessor order carries no meaning, so the moved pair keeps every build as it was.

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|

_N/A — no confirmed finding._

## Guard scope

Every fix that the analyzer's old silence kept away from an input is now reached by it. For each, the old analyzer span and the fix guard that must now cover it:

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Trailing-comma fix withheld (SHARED-TRAIL-002), `TrailingCommaCodeFixHelper.CommaPrecedesConditionalCompilationBoundary` | comma end → `GetNextToken().SpanStart` | Formatter keep-the-comma check, `EnumTrailingCommaRemovalTransform`/`InitializerTrailingCommaRemovalTransform`: last separator end → `CloseBraceToken.SpanStart`; removed analyzer guard: same gap | Same span: the diagnostic is always the last separator (`TrailingCommaAnalyzerBase<TNode>`), and directives/disabled text are trivia, so the next token is the close brace. Same predicate (`SyntaxTriviaUtilities.ContainsConditionalCompilationBoundary`). A non-comma at the span now withholds instead of registering a no-op action | holds |
| RH8201 fix withheld (RH8201-003), `IsImplicitImplementationWithConditionalInterfaces` | override → offered; explicit → offered; otherwise `#if` in any tree of the type or of `AllInterfaces` | Removed analyzer branch of `InheritsDocumentation` | Moved verbatim (same `HasConditionalInterfaces`, same override/explicit exemptions, same fallback for a non-`TypeDeclarationSyntax` parent or missing symbol, now withholding instead of silencing). BatchFixer runs the registration guard per diagnostic | holds |
| RH5102 / RH5109 fix | `LeadingAndInteriorContainsCommentOrDirective(list)` | Removed analyzer `ContainsConditionalCompilationBoundaryInElementGaps` (element/separator first tokens + close paren) | Fix span is a superset (open paren leading trivia + whole interior) | holds |
| RH5111 fix | declarator: full trivia; assignment/property/parameter/enum member: interior | Removed analyzer check of the leading trivia of `=` and of the value's first token | Both gaps lie inside every formatting node's interior | holds |
| RH7101 fix | `FindUnsupportedSyntax` (FMT-UNSUP-010: any directive in a multi-declarator field outside its first token's leading trivia) | Removed analyzer span first declarator start → last declarator start | Superset | holds |
| RH7105 / RH7106 fix | `ReorderingWouldDropCommentOrDirective`: directive in front of modifiers 2..n, trailing trivia, directive/disabled text before the next token | Removed `HasConditionalDirectiveBetweenModifiers` (first modifier start → last modifier start) | Superset; disabled text in front of a later modifier is always accompanied by a directive in the same trivia | holds |
| RH7102 / RH7103 fix | `TypeMemberOrderingCodeFixProviderBase.IsMoveSafe` (SHARED-ORDER-003/007/008) | Removed `HasModifiersSplitByConditionalDirective` (first modifier → next token) | ORDER-008 refuses a group that does not wrap whole units; the six-shape tests include the `#endif` in front of the type (`Active modifier` row) | holds |
| RH7107 / RH7108 fix | `OrderingMoveSafety.MoveRangeContainsDirectives` (each half must hold every partner) | Removed `ContainsConditionalCompilationBoundaryOutsideBodies` (whole list outside bodies) | Not a superset by design: balanced pairs inside one half stay movable (SHARED-ORDER-006, unchanged); unpaired directives in either half are refused. Traced and executed above | holds |
| SHARED-ATTR-019 split fix | `InteriorContainsCommentOrDirective(attributeList)` (registration and apply) | Removed `ContainsConditionalDirectiveInShape` (element gaps + target tokens) | Superset | holds |
| SHARED-ATTR-020 merge fix | `CanMergeAttributeListGroup` → `GroupInteriorContainsCommentOrDirective` (first list start → last list full end, first list's leading directive, next token's leading directive) | Removed analyzer span `siblings[0]` `[` end → reported list `]` start | Superset; the fix's group filter equals the analyzer's sibling filter (same target, in scope, same shape mode) | holds |
| RH7311 comparison | `RegionDirectiveTriviaSyntax.IsActive` | RH7309 (`IsActive: true`), `RegionDirectiveBlankLineAnalyzerBase`, RH8302 | Same reading of the build's regions | holds |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Conditional directive or disabled text in a span | `rg -n "ContainsConditionalCompilationBoundary" src`: one owner `SyntaxTriviaUtilities.ContainsConditionalCompilationBoundary`; callers `EnumTrailingCommaRemovalTransform`, `InitializerTrailingCommaRemovalTransform`, `TrailingCommaCodeFixHelper.CommaPrecedesConditionalCompilationBoundary` | one owner; the gap is built at each call site, as the deleted analyzer copy did, and the spans agree (above) |
| `#if` in a file of the type or its interfaces (RH8201) | `rg -n "HasConditionalInterfaces" src`: `RH8201InheritdocShouldBeUsedCodeFixProvider` only | one owner (moved, not copied) |
| Deleted silence helpers | `rg -n "HasModifiersSplitByConditionalDirective\|HasConditionalDirectiveBetweenModifiers\|ContainsConditionalCompilationBoundaryOutsideBodies\|ContainsConditionalCompilationBoundaryInElementGaps\|LeadingTriviaContainsConditionalCompilationBoundary\|IsInConditionalBranch\|GetCloseBraceToken\|ContainsConditionalDirectiveInShape"` over the repository minus `sdd/work`: only the historical change doc `comma-behind-comment-unsupported.md` and the pre-existing private fix guard `RH2001PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider.HasConditionalDirectiveBetweenModifiers`, which this run does not touch and which now has no shared counterpart | no stale reference; no second owner |
| Remaining analyzer reads of conditional directives | `rg -n "IfDirectiveTrivia\|ElseDirectiveTrivia\|ElifDirectiveTrivia\|EndIfDirectiveTrivia\|DisabledTextTrivia\|IsConditionalDirective\|IsActive" src/Reihitsu.Analyzer src/Reihitsu.Core`: RH7309, RH7311, RH8302, `RegionDirectiveBlankLine*` (`IsActive`), RH6004 (layout of inactive directives), RH7310 (kept silence, change doc *Decisions*), fix-side guards in `DeclarationModifierUtilities`, `ModifierOrderingUtilities`, `SyntaxIndentationUtilities`, `FormattingTextAnalysisUtilities` | no reporting silence for a build-configuration reason left outside the change doc's kept list |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| "reported, no fix" tests (RH7105/7106/7102/7103/7107/7108/5102/5109/5111/7101, attribute shape ×16, RH5409–5411, RH8201) | The marked diagnostic is reported and no action is registered for any reported diagnostic | An extra/missing diagnostic, or any registered action | `VerifyNoCodeFixIsOffered` (exact diagnostics + per-diagnostic registration check with the same parse options and symbols) | adequate |
| Six-shape matrices (empty, excluded, active, if/else, elif, `#if false`) | Same, per shape | Same | `VerifyNoCodeFixIsOffered` | adequate (FC-020, FC-003 with the region/pragma contrasts) |
| Other-build halves (`VerifyWithPreprocessorSymbols`, `VerifyNoCodeFixIsOffered(…, "DEBUG")`) | The build that defines the symbol reports exactly the marked set | Any diagnostic in that build, or a compile error in it | `Verify` with parse-option transform; harness compiles with default `CompilerDiagnostics.Errors` | adequate; this is how decision 8 surfaced |
| Fix All contrasts (19 tests, decision 7) | Only the unguarded diagnostics are fixed; the guarded one stays at its location | The guarded one changes or disappears; the unguarded one stays | `Verify(source, fixed, onConfigure)` with `MarkupMode.Allow` on fixed and batch-fixed state | adequate: the remaining diagnostic is now checked by ID and span |
| RH7107/RH7108 `BalancedConditionalDirectivePair…MovesWithIt` | The fix is offered and the pair moves with the accessor (SHARED-ORDER-006) | Fix withheld, or pair left behind | `Verify(source, fixed)` | adequate |
| RH7311 compiled-branch / excluded-branch / two-branch tests | Compiled-branch regions are compared; excluded and sibling-branch regions are not | A report on an excluded region, or no report on the compiled one | `Verify` in both builds | adequate |
| RH5409/RH5410/RH5411 formatter parity (`…IsReportedAndKeptByTheFormatter`) | The analyzer reports the comma and formatting changes nothing, LF and CRLF | Any formatter edit, or a missing report | `VerifyFormatterKeepsReportedShape` | adequate for the asserted guarantee; see *Hints* |
| RH5412, RH2005, RH7003 spec-example tests | Unchanged output, now tagged SHARED-DIAG-008 | Change in report or fix | `VerifyNoCodeFixIsOffered`, `Verify` | adequate |

## Failure classes dismissed

- FC-001, FC-002, FC-015, FC-016 — traced under *Guard scope* and *Policy ownership*; every fix the change newly reaches has a guard covering at least the removed analyzer span, and the one decision with a counterpart (the trailing-comma gap) agrees with the formatter's.
- FC-044 — the guards are cheap presence checks (`#if` in a file, conditional directive or disabled text in a gap); none evaluates a condition or models another build.
- FC-004, FC-005, FC-019, FC-020, FC-021 — reached only through the fixes, whose guards cover directives of every kind and comments; the tests run every directive shape.
- FC-006, FC-022–FC-029, FC-036, FC-018 — no rewrite changed; RH7107/RH7108 moves were exercised on directive shapes above.
- FC-007, FC-008, FC-009, FC-030 — BatchFixer/document-order providers re-run the registration guards per diagnostic; Fix All contrasts are tested; the trailing-comma guard also stops a no-op registration for a non-comma span.
- FC-010, FC-011, FC-012 — no formatter change.
- FC-013, FC-014 — RH5102/RH5109: the formatter breaks the lists (executed). RH5111: the formatter keeps the gap; maintainer answer 3 extends RH5111's SHARED-DIAG-010 exception. Attribute shape and trailing commas are not SHARED-DIAG-010 layout rules and are covered by maintainer answers 1 and 2.
- FC-017, FC-033, FC-034, FC-035 — see *Assertion adequacy*; `04-test-implementer.md` states the fix outputs were confirmed on unchanged code with the guarded markup removed.
- FC-031, FC-037 — every changed method summary and inline comment describes the new behavior (`rg -i "conditional|#if|directive"` over the 24 changed production files); no `TODO`, no tracker numbers, no commented-out code.
- FC-032 — no project result is reused across the compiled change; the validator runs after this audit.
- FC-038–FC-043 — the run declares no shape unsupported (maintainer answer 6A); RH7101's withheld fix relies on the existing FMT-UNSUP-010 check, unchanged.

Check 10 (visible output decided): every new report and every withheld fix is listed under the change doc's *Visible output changes* and covered by maintainer answers 1–5; RH7107/RH7108's fix for a balanced pair is demanded by the unchanged SHARED-ORDER-006 and was stated in the change doc before the answers (`3d85973`); the Fix All outputs and the RH5412/RH2005/RH7003 tests assert unchanged outputs under unchanged requirements; the rule-page edits (removed *Exceptions* in RH5409, RH5410, RH5411, RH8201; new paragraph in `general-notes.md`) are listed in the change doc and covered by answers 2 and 5. Check 11: the two new guards are required by SHARED-TRAIL-002 and RH8201-003, both cheap checks; no new machinery. Check 9: no edit outside the run's scope.

## Architecture findings

_None._

## Hints

- `VerifyFormatterKeepsReportedShape` (`src/Reihitsu.Analyzer.Test/Base/FormatterTestsBase{TAnalyzer}.cs`) passes when the formatter refuses the input as unsupported, because a refused document is also unchanged; FMT-STRUCT-019/020 state that the transform keeps the comma, which a refusal would satisfy only by accident. The existing `VerifyFormatter` has the same tolerance, and none of the three inputs is refused today (no FMT-UNSUP shape). Asserting `isRefused == false` there would pin the stronger guarantee. Not a finding.
- `general-notes.md`, *Code excluded by `#if`*, first paragraph (unchanged text): "several of the region rules check [`#region`/`#endregion` in an excluded branch]". RH7309, RH7311, RH8302, and the region blank-line rules read only active regions; whether any region rule still checks inactive ones was not verified. Pre-existing wording, not changed by this run.

## Follow-up candidates

_None_ beyond those already in `00-lead.md`.

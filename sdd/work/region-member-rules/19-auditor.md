Role: auditor
Commit: 3fcbebf67bcf85b9d6ee3c4fafc84bbd423b5014
Verdict: pass

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

Repair delta, audited against `5dde996` (the first audit, `11-auditor.md`), plus the merge of `origin/main`.

State checks:
- The checkout is `3fcbebf`.
- `git ls-remote origin refs/heads/main` returns `81eabebef044033483214fc0f01e63cf06067bb1`, the SHA in the prompt. It is an ancestor of HEAD, merged by `2f0a416`.
- `scripts/lane-check.sh bug-fix` reports: confirmed.
- The diff is what the handovers describe:
  - The round's own changes, `git diff 5dde996 ddd601b -- . ':(exclude)sdd/work'`, are 8 files: `AccessorOrderingUtilities`, `OrderingDeclarationUtilities`, the RH7105 and RH7106 analyzers, and the RH7105–RH7108 test files. This matches 12 and 13.
  - The merge `2f0a416` brings in `main`'s 11 files. Ten of them are byte-identical to `81eabeb` on HEAD.
  - The eleventh, `RH5102ArgumentsShouldBeOnSingleOrSeparateLinesAnalyzerTests.cs`, is the branch version plus `main`'s added lines, line for line. The added lines equal `main`'s diff against `8a26419`, and the branch's own lines equal its diff against `81eabeb`. This matches 17.

Scope did not grow: no spec, rule page, or change doc is touched, and no surface beyond the three findings is touched.

Execution: one focused run on the merged head. It checks FC-032, because `main`'s `LineBreakListRewriter` change reaches every run code fix that delegates to the formatter, and only RH5101/RH5102 had been run on the merged tree:

```shell
scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH5102|FullyQualifiedName~RH5109|FullyQualifiedName~RH5111|FullyQualifiedName~RH7101|FullyQualifiedName~RH7102|FullyQualifiedName~RH7105|FullyQualifiedName~RH7106|FullyQualifiedName~RH7107|FullyQualifiedName~RH7108|FullyQualifiedName~RH7109|FullyQualifiedName~RH73|FullyQualifiedName~RH74"
```

Result: Passed 1440 of 1440. Every other statement below comes from static tracing.

### Previous findings

| # | Status | Evidence |
|---|---|---|
| 1 | closed | `AccessorOrderingUtilities.GetBodySpan` now runs an expression body from `=>` through the accessor's `;`. The `;`'s leading trivia (`SpanStart` inside the span, after its start) is no longer read. The `=>` token (`SpanStart` equals the span start) is still read, and so is a block's `{`. The gap in front of a body is therefore judged the same way for both body kinds, and the inside of both (block through `}`, expression through `;`) is skipped. Both finding members are now reported and fixed, and are pinned with exact output: RH7107 `set … #if DEBUG + 1 #endif ;` and RH7108 `remove … ?? throw … ;`, LF and CRLF. So are the moved-accessor variants and the active `#if !NEVER` pair. The members the finding listed as holding (directive at the end of a block body, directive inside the expression) are unchanged. Both callers, RH7107 and RH7108, use the one function |
| 2 | closed | Both analyzers call `OrderingDeclarationUtilities.HasConditionalDirectiveBetweenModifiers`. Its span `[first modifier start, last modifier start)` with `Count < 2 → false` is equivalent to the removed inline `Count > 1 && …`. The helper sits next to `HasModifiersSplitByConditionalDirective`, and its summary names the difference (decision 13) |
| 3 | closed | `ConditionalDirectiveInFrontOfTheFirstModifierDoesNotKeepTheDeclarationFromBeingReportedOrFixed` in RH7105 and RH7106 pins the lower boundary, reported and fixed: without `DEBUG` the whole pair and the disabled member lie in `static`'s / `internal`'s leading trivia; with `DEBUG` only the `#endif` does |

## Findings

_None._

## Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| RH7107/RH7108: is a token's leading trivia inside an accessor body | Block: `(Body.SpanStart, Body.Span.End)`. Expression body: `(ExpressionBody.SpanStart, ;.Span.End)`, or the arrow clause's end when `;` is missing (syntax error, not analyzed in practice) | RH7107-001/RH7108-001 ("inside an accessor's body, a block or an expression body"; "the gap in front of a body" outside). The C# grammar's accessor body is `=> expression ;` | Both body kinds now exclude their closing token's trivia and read their opening token's trivia. A directive in the `;` gap of a body-less accessor (`get;`) is still read (no body) | holds |
| RH7107/RH7108 fix: directive moved or crossed | `OrderingMoveSafety.MoveRangeContainsDirectives` (unchanged) | SHARED-ORDER-006 | The new tests show the fix offered with the pair intact for both the moved and the crossed accessor, LF and CRLF | holds |
| RH7105/RH7106 judged modifier span | `HasConditionalDirectiveBetweenModifiers`: `[first modifier start, last modifier start)`, two or more modifiers | `HasModifiersSplitByConditionalDirective` (RH7102/RH7103, through the token after the last modifier); SHARED-MODORDER-002/-003 | The two spans differ by design (Q3-B, decision 13); one owner each. Both boundaries are tested: the lower one (round 1) and the upper one (first round) | holds |

## Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Accessor body span | `rg "Body \?\? .*ExpressionBody\|ExpressionBody.Span" src --glob '!*Test*'` → only `AccessorOrderingUtilities.GetBodySpan` in the run's code | one owner |
| RH7105/RH7106 modifier span | `rg "modifiers.First\(\).SpanStart" src --glob '!*Test*'` → `OrderingDeclarationUtilities.HasConditionalDirectiveBetweenModifiers` (RH7105, RH7106) and `HasModifiersSplitByConditionalDirective` (RH7102, RH7103) | one owner per span; the two spans are justified by decision 13 |
| Same name, other policy | `RH2001PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider.HasConditionalDirectiveBetweenModifiers` (private, pre-existing on `main`, unchanged). It also reads the type's leading trivia, for RH2001-009's withholding of a property-to-field conversion | a different decision (RH2001-009, the fix only); not a copy of the RH7105/RH7106 span |

## Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH7107 `ExcludedCodeAtTheEndOfTheExpressionBodyOfTheSetAccessor…` (LF, CRLF) | A `;`-gap pair in the crossed accessor lies inside the body; the fix keeps the pair | No diagnostic; pair relocated or dropped | `VerifyCodeFixWithEndOfLine`, exact text | adequate |
| RH7107 `ConditionalDirectiveClosingAtTheEndOfTheExpressionBodyOfTheSetAccessor…` | Active pair: only the `#endif` is in the `;`'s trivia | No diagnostic | `Verify(source, fixed)` | adequate |
| RH7107 `…OfTheGetAccessorDoesNotKeepItFromBeingReported`; RH7108 `…OfTheAddAccessor…` | The pair travels with the moved accessor | No diagnostic; pair left behind | `Verify(source, fixed)` | adequate |
| RH7108 `…OfTheRemoveAccessor…` (LF, CRLF) | The first audit's counterexample | No diagnostic | `VerifyCodeFixWithEndOfLine` | adequate |
| RH7105/RH7106 `ConditionalDirectiveInFrontOfTheFirstModifier…` | Lower boundary of the judged span, with and without `DEBUG` | Silence or no fix if the span widened to the first modifier's leading trivia | `Verify(source, fixed)` twice, the second with `DefineDebug` | adequate |
| Merged RH5102 tests (`main`'s four next to the run's two RH5102-004 tests) | Unchanged on both sides | — | — | adequate. Unchanged assertions; the 1440-test run covers both sides on the merged tree |

## Visible output

Every expected value the round adds is demanded by an unchanged requirement:
- RH7107-001/RH7108-001 and SHARED-ORDER-006 for the reported and fixed accessor lists. `main` produced the same output (12-test-implementer).
- RH7105-001/RH7106-001 and SHARED-MODORDER-002 for the lower-boundary tests.

The merge changes no run expected value.

## Failure classes dismissed

Delta only; the first audit's dismissals still hold for the unchanged code.

- FC-001, FC-002: traced above (Guard scope). The repair widens the excluded span exactly to the requirement's "expression body".
- FC-003: the round's moved boundary (the `;` gap) is tested on both sides. Inside: the new tests. Outside, between the accessors: existing tests.
- FC-004, FC-020, FC-021: no new rewrite. The accessor fix is unchanged and pinned with the pair in the moved and in the crossed accessor.
- FC-005: the predicate reads conditional directives and disabled text only, as before.
- FC-006, FC-022–FC-026, FC-029, FC-036: no expression rewrite, literal, identifier, or rename touched.
- FC-007, FC-008, FC-009, FC-018, FC-030: the fixes are unchanged. Fix All is unaffected; the new inputs add no co-occurrence.
- FC-010–FC-014, FC-028, FC-038–FC-043: no formatter or precheck change from the run. `main`'s formatter change is checked under FC-032.
- FC-015: one owner per span (Policy ownership).
- FC-016: sibling body kinds (block, expression, none), both rules, and both accessor roles (moved, crossed) are covered.
- FC-017: exact-text helpers; LF and CRLF on the counterexample shapes.
- FC-027: the multi-line expression body is the tested form; a directive inside the expression is unchanged and holds (first audit).
- FC-031: the `ContainsConditionalCompilationBoundaryOutsideBodies` summary and the new `GetBodySpan` and `HasConditionalDirectiveBetweenModifiers` summaries describe the new code. The inline comments in RH7105/RH7106 still fit.
- FC-032: checked by the focused run on the merged head (1440/1440).
- FC-033, FC-034, FC-035: the reproductions use the first audit's exact counterexamples. Expected values come from `main`'s output and the unchanged specs, not from the patched code.
- FC-037: not a text-only change.

Checks 8 and 9: no `TODO`, no commented-out code, and no tracker number in the delta. Every changed path belongs to findings #1–#3 or to the merge.

Check 11: `GetBodySpan` is a cheap token-position check, and the new helper only moves an existing check. No special case was added.

## Architecture findings

_None._ No finding repeats the class of #1, #2, or #3.

## Hints

- `RH2001PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider` has a private method with the same name as the new Core helper `HasConditionalDirectiveBetweenModifiers`, but a different span. It also reads the type's leading trivia, under RH2001-009. This is pre-existing and harmless, but a reader may take one for the other.
- The first audit's still-open hints (duplicate `TryGetContainingRegionName` wrappers; the self-hosting scan may show new RH7308/RH7311/RH74xx findings) were decided by the lead (decision 19) or are left to CI.

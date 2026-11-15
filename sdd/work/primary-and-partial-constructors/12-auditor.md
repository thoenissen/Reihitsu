Role: auditor
Commit: 0f08cfd80cf9f88a900ad967864beb4e37528c8f
Verdict: #1680: pass; #1742: pass

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

How this was established: static reading of the repair diff (`git diff 257eea7 0f08cfd -- src`) and the counterparts it reaches, plus the throwaway probe from the first audit (session scratchpad, `probe/`, references `src/Reihitsu.Analyzer` at head, runs the chosen analyzers with their IDs forced to warning) over a new corpus of 15 primary-constructor shapes and a control file without parameter lists. No test was re-run; the roles' focused results (08, 09), the implementer's analyzer run (9388/9388) and the cleaner's build (0 warnings) are trusted.

## Scope (whole run)

Head `0f08cfd80cf9f88a900ad967864beb4e37528c8f` is checked out, working tree clean. `origin/main` is `846aca462c94c0d85bc19a1d5ad9d78605590c29` (`git ls-remote origin refs/heads/main` agrees) and is an ancestor of head. Repair delta against `257eea788cdad88f7022155884a53f11aa76ddd8`: `AnalysisContextExtensions.cs` (precheck and summaries), the registrations of RH3002, RH3005, RH3006, and nine tests (seven for finding #1, two for finding #2) — what 08-test-implementer.md and 09-implementer.md describe. No spec, rule page, or ID changed (11-spec-checker.md agrees). The repair stays inside the component the run already changes; scope did not grow, so this is a delta audit. `scripts/lane-check.sh bug-fix --base 846aca4`: `LANE CHECK: confirmed — claimed bug-fix, diff needs bug-fix`. No `TODO`, commented-out code, or issue/PR number in the added source lines.

## thoenissen/Reihitsu#1680

### Previous findings

| # | Status | Evidence |
|---|---|---|
| 1 (RH3002/RH3005/RH3006 report directive-condition expressions twice) | closed | The three registrations now go through `RegisterStructuredTriviaAction`; `IsRepeatedRun` first returns `false` outside structured trivia. Probe at head, every directive-condition member once: `#if`/`#elif` in front of the type and in front of `}` of a class, record, record struct, struct, nested class, both parts of a partial class; inside the parameter list (constructor run only); inside the base argument list (constructor run only); in front of the base type name and behind a `[method: ...]` list (type run only). RH3006 `||`/`&&`, RH3005 `==`/`!=`, RH3002 `((A))` each once; control file identical counts. Members that held in the first audit (RH3001, RH5302, RH1001, RH3103, RH6011, RH6012, RH6015) are in unchanged files; an `rg` of every registration on a kind that can occur inside structured trivia finds no further one. |
| 2 (guard edges untested) | closed | `VerifyLowercaseRegionInFrontOfBaseTypeArgumentListOfClassWithPrimaryConstructorIsReportedOnce` puts the directive in the leading trivia of the base argument list's `(`; `VerifyLowercaseRegionInFrontOfParameterListOfClassWithPrimaryConstructorIsReportedOnce` puts it in the leading trivia of the parameter list's `(`. These are the C2 and C1 shapes whose mutation result (deleting the base-argument-list term, or `FullSpan` to `Span`, gives 0 diagnostics) the first audit probed; the test shapes differ only in names and an added `#endregion`, which does not move the `#region`. |
| Hint (`IsRepeatedRun` summary omits the type-run-only group) | closed | The summary now names both one-run groups, matching the first audit's probe. |

### Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| _None._ | | | | | | | |

### Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| New precheck: a node outside structured trivia is never a repeated run | `context.Node.IsPartOfStructuredTrivia()` — the node and its parents up to the structured-trivia root | The analyzer driver visits a node twice only inside structured trivia attached to a primary-constructor type's header or closing brace (first audit's probe) | Agrees; for every node outside trivia the wrapper is a pass-through, so RH3002/RH3005/RH3006 behave as a plain `RegisterSyntaxNodeAction` there (probe: `Ordinary` primary-constructor class with parenthesized defaults, base arguments, field initializers gives the same diagnostics as the control). For the ten round-0 registrations the registered node is itself structured trivia or inside it (`IsPartOfStructuredTrivia` includes the node itself), so they are unchanged. | holds |
| Which registrations get the guard | Now 13: the ten trivia-kind registrations plus `ParenthesizedExpression` (RH3002), the six comparison kinds (RH3005), `LogicalOrExpression` (RH3006) | Every kind that can occur inside structured trivia and is registered by some analyzer (`rg` over `Register*` lines with directive, comment, XML, cref-type, and directive-expression kinds) | Agrees; the remaining unguarded registrations (RH3001, RH5302, RH1001, RH3103, RH6011, RH6012, RH6015) report nothing inside structured trivia in either run (first audit's probe, files unchanged). RH3006's `OrPattern` stays plain: a pattern cannot occur in a directive condition. | holds |
| Position test (unchanged) | Outside `ParameterList.FullSpan` and the base argument list's `FullSpan` | — | Byte-identical to the first audit, now also reached by expression nodes; probe confirms it for directive conditions in both exempt regions | holds |

### Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Skip the repeated primary-constructor run | `rg -n "IsRepeatedRun\|WrapWithRepeatedRunCheck\|IsPrimaryConstructorRun\|MethodKind\.Constructor" src/Reihitsu.Analyzer src/Reihitsu.Core`: `AnalysisContextExtensions` only (other hits as in the first audit, different decisions) | one owner |
| Skip nodes inside structured trivia | `rg -n "IsPartOfStructuredTrivia" src/Reihitsu.Analyzer`: RH6011, RH6012, RH6015, RH6024 skip trivia nodes altogether (a different decision: do not report inside trivia) and the new precheck | no duplicate policy |

### Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH3006 `AndInsideOrIn{If,Elif}ConditionInFrontOfClosingBrace…`, `…InFrontOfClassWithPrimaryConstructorIsReportedOnce` | one diagnostic on the `&&` operand | count 2 (watched failing before the fix, 08) or 0 | `AnalyzerTestsBase<TAnalyzer>.Verify(source, expected)` — analyzer verifier, exact count, markup span | adequate |
| RH3005 `ConstantOnLeftInIfCondition…` (two positions) | one diagnostic on `==` | count 2 or 0 | same | adequate |
| RH3002 `ParenthesesInIfCondition…` (two positions) | one diagnostic on the inner pair | count 2 or 0 | same | adequate |
| RH7302 `…InFrontOfBaseTypeArgumentList…`, `…InFrontOfParameterList…` | the constructor-run-only directive keeps its one diagnostic | 0 once the base-argument-list term is deleted or `FullSpan` becomes `Span` | same | adequate |

### Visible output is decided

Every new expected value is one diagnostic per element under unchanged requirements: RH3002-007, RH3005-003, RH3006-002, RH7302-003. No rule page changed. Nothing undecided.

### Cost of special cases

The precheck is one cheap syntactic check; it serves every directive-condition expression, not a rare class. No unsupported shape is involved.

### Failure classes dismissed

Reused from the first audit (07-auditor.md), which the delta does not change: no rewrite, no formatter, no code fix changed (FC-004, FC-006–FC-014, FC-018–FC-023, FC-025–FC-030, FC-036, FC-038–FC-043); no text-only claim (FC-037); no build reuse (FC-032, FC-033). For the delta:
- FC-001, FC-002, FC-016 — see Guard scope: the guard's span and the driver's double-visit set now agree for every registered kind that can occur inside structured trivia.
- FC-003, FC-017 — both guard edges and all seven directive-condition shapes have count-exact tests.
- FC-005 — directives are the subject; conditions in `#if` and `#elif`, in front of the type, in front of `}`, inside both exempt lists, probed.
- FC-015 — one owner.
- FC-024 — partial type: the part without the parameter list is visited once with the type symbol (probe, `P1` second part).
- FC-031 — the class summary, both registration summaries, `IsRepeatedRun`, and `WrapWithRepeatedRunCheck` describe the new scope.
- FC-034 — the tests use the counterexample shapes from finding #1.
- FC-035 — expected values come from the requirements, with a control (08).

### Architecture findings

_None_ new. The per-registration opt-in remains the structural fact recorded by the skeptic and the first audit; the repair extended it to every registration that can see a node inside structured trivia.

### Hints

- `IsRepeatedRun` evaluates `IsPartOfStructuredTrivia()` first, which walks from the node to the tree root for every parenthesized expression, comparison, and `||` in a compilation, before the constant-time test on `ContainingSymbol`. Testing the constructor symbol first would keep the walk off almost every node; both conditions stay required. Performance only, not a trap.
- `RegisterStructuredTriviaAction` is now also used for kinds that mostly occur outside trivia (RH3002, RH3005, RH3006). Its summary is accurate but does not say that nodes outside structured trivia run as with a plain registration; a reader of RH3002's `Initialize` could take the call for a trivia-only registration. The implementer named this and kept the name to keep the round small.

## thoenissen/Reihitsu#1742

### Scope

The repair does not touch SHARED-DIAG-013, its tests, or anything the partial-constructor handling concerns. The round-0 result in 07-auditor.md stands. The precheck cannot reach a partial constructor: a node outside structured trivia returns early, and inside trivia the constructor's declaring syntax is a `ConstructorDeclarationSyntax`, not a type declaration, as before.

### Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| _None._ | | | | | | | |

### Guard scope

_N/A — no predicate added or changed for this issue._

### Policy ownership

_N/A — unchanged since 07-auditor.md._

### Assertion adequacy

_N/A — no test added or changed for this issue in the repair._

### Failure classes dismissed

Every class — no code or test for this issue changed in the repair.

### Architecture findings

_None._

### Hints

_None._

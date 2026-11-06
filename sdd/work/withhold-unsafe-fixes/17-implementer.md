Role: implementer
Commit: 47c8237a
Verdict: tests pass

Repair round 2 with the architecture changes of rows 26–28. It carries out lead decisions 26–29, 31 and 33 (`00-lead.md`) against the tests of `16-test-implementer.md` and the spec diff `09a5cd28..97fcf63d`. My prompt named the decided rows and the standing instruction. It carried no intended fix, suspected cause, or hoped-for verdict beyond those rows, so I had nothing to set aside. I changed no test file and no file under `sdd/specs`. I raised no output questions.

## thoenissen/Reihitsu#1257

### Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Clarity/RH3005UseReadableConditionsCodeFixProvider.cs`, `RH3101…CodeFixProvider.cs`, `RH3105…CodeFixProvider.cs` | `GetFixAllProvider` returns `DocumentOrderFixAllProvider.Create(new <rule analyzer>())`. For the provider itself, see *Whole run* | SHARED-FIX-002 |

The nested RH3005 comparison `1 < (1 < x ? 2 : 3)` is now fixed completely in one Fix All application.

## thoenissen/Reihitsu#1279

### Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Clarity/RH3004UseLambdaSyntaxCodeFixProvider.cs` | **Row 27:** `GetFixAllProvider` returns `DocumentOrderFixAllProvider.Create(new RH3004UseLambdaSyntaxAnalyzer())` instead of `BatchFixer`. | SHARED-FIX-002 |
| same file | **Row 28:** The private raw-string line shifting is removed. `CanMoveLine` is replaced by `KeepsColumns`: a line that starts inside a literal token keeps its columns, which now only a verbatim string can reach. The fix is withheld (`ContainsMultiLineRawStringLiteral`) when the block holds a multi-line raw string token: plain, UTF-8, or interpolated start. This also covers the same tokens in the disabled text of an inactive branch, checked with `SyntaxFactory.ParseTokens`, so whether the fix is offered does not depend on the defined symbols. The check runs at registration, so the expression-body path is covered too. | RH3004-007, RH3004-003 |
| same file | **Rows 29, 31, 33:** `MoveStatementsAsync` collects the columns of the lines that move (`GetMovedLineColumns`). It leaves out blank lines, lines that stay in column 0, and lines that keep their columns. (1) When the first statement shares the opening brace's line and a later line starts left of it, the later lines move by their own delta, which puts their leftmost line in the statement column. (2) Otherwise, when the measured delta would put any moved line left of column 0, every moved line moves by the delta that puts the leftmost line in the statement column. In every other case the measured delta applies. `IsColumnZeroDirectiveLine` becomes `StaysInColumnZero`, which also covers a line whose comment starts in column 0: `//`, `/* */`, a `///` line (each line of a multi-line documentation comment is checked on its own text), and `//` or `/*` at column 0 in disabled text. The continuation line of a multi-line comment is not counted. A block with no statement line is measured on its first line that does not stay in column 0. The summary describes the new rules. | RH3004-006 |

## thoenissen/Reihitsu#1281

### Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Core/RewriteBindingUtilities.cs` (new, public) | **Row 26.** `BindsAsBefore(semanticModel, expression, replacement, out replacementConvertedType, cancellationToken)` speculatively rebinds the outermost binding root around the expression. That root is a non-block statement, a variable or property initializer, an expression body, a constructor initializer, or a primary constructor base type, below the member or accessor. The search continues past statements inside anonymous functions and past local functions. The binding root is rebound with the replacement in place. The method returns `true` when all of these hold: the replacement's conversion exists; the replaced operation tree has no more errors than the original (invalid operations, conversions that do not exist, error types); and every symbol the expression and its ancestors up to the root bind to is the same, including candidates and reason when binding fails, and the collection initializer `Add` method. Anonymous-function symbols are skipped. When the root cannot be rebound, it returns `false`. | RH3106-002, RH3106-003 |
| `src/Reihitsu.Analyzer/Rules/Clarity/RH3106UnnecessaryDelegateParenthesesShouldBeRemovedAnalyzer.cs` | **Row 26.** `GetPositionExpression`, `HasDelegateTypedNeighbor`, `IsDelegateTypedNonAnonymousFunction`, `HasCompetingCandidates`, and the position switch are removed. `TakesNaturalType` binds a probe: the same anonymous method with the parameter list `(int reihitsuNaturalTypeProbe)`, which gives it a different natural type. The anonymous method takes its natural type, and is not reported, when two things hold: the probe binds as before, and the probe's converted type follows the natural type. That means a different delegate type when the original converted to a delegate type, or the same non-delegate type otherwise. A delegate target rejects the probe. A choice among candidates picks another candidate or none. A conditional with a delegate-typed other branch under `object` converts the probe to `object`, not to a delegate, so it stays reported. | RH3106-001, RH3106-003 |
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Clarity/RH3106UnnecessaryDelegateParenthesesShouldBeRemovedCodeFixProvider.cs` | **Row 26.** `KeepsMeaningAsync` is now one call: `BindsAsBefore` with the list removed (`BuildNodeWithoutList`), plus the same converted type (`SymbolEqualityComparer`). `IsOverloadResolvedCall`, `GetBoundSymbolText`, and the `ArgumentSyntax`/`BinaryExpressionSyntax` gate are removed. | RH3106-002, SHARED-FIX-003 |
| `src/Reihitsu.Analyzer.CodeFixes/Base/EmptyParenthesesCodeFixProviderBase{TNode}.cs`, `RH3106…CodeFixProvider.cs`, `RH3107…CodeFixProvider.cs` | The base constructor takes the rule's analyzer, which it passes to `DocumentOrderFixAllProvider.Create`. | SHARED-FIX-002 |

Probed with `scripts/apply-fix.sh RH3106` on a scratch fixture outside the repository, LF and CRLF:
- reported and fixed: field initializer `Action F`, expression-bodied property, `this(…)` initializer, event `+=`, `var x = b ? … : someAction`, `Func<Action> f = () => …`, `new List<Action> { … }`, a local function's expression body;
- not reported: `object G = …`, `var g = () => { return delegate () { }; }`.

## thoenissen/Reihitsu#1282

Nothing to do: RH3107 only gets the base constructor argument. Its tests pass unchanged.

## thoenissen/Reihitsu#1417

Nothing to do: no change touches RH3103.

## thoenissen/Reihitsu#1422

Nothing to do: no change touches RH3104 or RH5417.

## Whole run

### Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer.CodeFixes/Core/DocumentOrderFixAllProvider.cs` | **Row 27.** The provider is now a sealed class built with `Create(DiagnosticAnalyzer)` and no longer has a static `Instance`. A pass works as before: in document order, re-registering each fix on the current document, and mapping later spans through the text changes. When a change overlaps a later diagnostic's span, that diagnostic is no longer dropped. The pass records it as lost. After a pass that applied a fix and lost a diagnostic, the provider runs the rule's analyzer on the changed document (`CompilationWithAnalyzers`, syntax and semantic diagnostics of the tree, filtered to `FixAllContext.DiagnosticIds`, suppressed ones excluded) and runs another pass over what it reports. A pass that applies no fix, or loses nothing, ends the work. The number of passes is bounded by the number of starting diagnostics. The common case, with no overlap, costs exactly what it did before: one pass and no re-analysis. | SHARED-FIX-002 |

### Policy owners

- **Rebinding a rewrite or probe.**
  - Command: `rg -n "TryGetSpeculativeSemanticModel" src --glob '!**/bin/**' --glob '!**/obj/**' --glob '!*Test*/**'`.
  - It finds only `RewriteBindingUtilities`. The RH3106 analyzer (probe) and the RH3106 fix (removal) both decide through it, so the position lists of both are gone.
  - `rg -ln "TypeKind.Delegate"` finds only the RH3106 analyzer's `IsDelegateType`, which applies the "follows the natural type" rule on top of the shared answer.
- **Re-analysis inside Fix All.**
  - Command: `rg -ln "WithAnalyzers\(" src …`.
  - It finds `DocumentOrderFixAllProvider` and `Reihitsu.Tooling/FixtureRunner.cs` (the fixture runner, not a fix).
- **Multi-line raw string tokens.**
  - Command: `rg -ln "MultiLineRawStringLiteralToken" src …`.
  - It finds the formatter's `RawStringLineShifts`, RH5202 (analyzer and fix), RH5601, and the RH3004 fix.
  - The RH3004 fix now only checks for presence, to withhold the fix. It no longer moves raw-string lines, so the private copy of the formatter's raw-string rule (finding 7) is gone.
  - `RawStringLiteralUtilities` in Core has no "is multi-line raw" predicate to reuse.
- **Lines that stay in column 0.**
  - The RH3004 fix's `StaysInColumnZero` sits next to the formatter's `IndentationRewriter.IsNonRegionDirectiveTrivia`. The follow-up from rows 21–25 is unchanged.
  - Comment lines in column 0 are a fix-only decision (RH3004-006, row 33).

### Tests run

| Command | Result |
|---|---|
| `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH3004UseLambdaSyntaxAnalyzerTests\|FullyQualifiedName~RH3005UseReadableConditionsAnalyzerTests\|FullyQualifiedName~RH3106UnnecessaryDelegateParenthesesShouldBeRemovedAnalyzerTests"` on 97fcf63d, before any change | Failed 23, Passed 141, Total 164. These are exactly the 23 tests `16-test-implementer.md` marks as "fails". |
| `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH3004UseLambdaSyntaxAnalyzerTests\|FullyQualifiedName~RH3005\|FullyQualifiedName~RH3101\|FullyQualifiedName~RH3105\|FullyQualifiedName~RH3106\|FullyQualifiedName~RH3107"`, after the change | Passed 243, Failed 0 |
| `scripts/test.sh --configuration Debug --project analyzer` | Passed 6904, Failed 0 |
| `scripts/test.sh --configuration Debug --project core` | Passed 621, Failed 0 |
| `scripts/test.sh --configuration Debug --project architecture` | Passed 115, Failed 0 |
| `scripts/build.sh` (Release) | 0 warnings, 0 errors |
| `scripts/lane-check.sh bug-fix` | confirmed |

I ran `scripts/format.sh` over every changed source file.

### Disputed tests or scope

_None disputed._ Scope notes for the lead:

- **New public Core helper `RewriteBindingUtilities`.** It has two consumers, the RH3106 analyzer and the RH3106 fix, and it sits next to `RewriteParseUtilities`. It has no direct Core unit tests. Decision 13 asked for such tests for the earlier Core helper, and the test implementer may want the same here. Today it is covered only through the RH3106 tests.
- **The RH3106 analyzer now speculatively rebinds the enclosing statement or initializer** for every empty parameter list of an anonymous method. The cost is one speculative bind per candidate. It does not grow with the document.
- **`DocumentOrderFixAllProvider` now takes the rule's analyzer and runs it on the changed document**, but only after a pass in which a fix overlapped a later diagnostic. For nested instances, this is the "track each later diagnostic" option the auditor named in finding 6. The `EmptyParenthesesCodeFixProviderBase` constructor gained an analyzer parameter.
- **Inputs the RH3106 helper cannot rebind are withheld by the fix and reported by the analyzer.** These are positions with no speculative-model overload, which in practice means none reachable from compiling code with an anonymous method. This is the conservative default on both sides.

### Architecture findings

_None new._ Rows 26–28 are carried out:
- RH3106: one semantic decision, shared by the analyzer and the fix.
- Fix All: one provider, with nested instances found again by the analyzer, and RH3004 moved onto it.
- RH3004: no private raw-string rule.

### Follow-up candidates

- **RH3004-006 measures the shift from the first character of the moved code on the first statement line.** When that line starts with the continuation of a multi-line comment that opens behind `{` (`{ /* a⏎   b */ Foo();`), the shift is taken from the comment's continuation, not from the statement. The result is consistent and nothing is clamped, but it is unusual (probed, not tested). This shape is rare. It is listed only so that the spec author can decide whether "first character" should name the statement.
- **`RewriteBindingUtilities` counts binder errors through the operation tree, not through compiler diagnostics.** This is enough for every RH3106 case probed. A future consumer that needs diagnostics-level certainty would need a compilation fork. No caller needs one today.

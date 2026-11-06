Role: implementer
Commit: 6961521a
Verdict: #1257: test disputed; #1279: test disputed; #1281: test disputed; #1282: test disputed; #1417: tests pass; #1422: tests pass

My prompt named the decisions to follow (00-lead.md rows 3 and 7) and carried no intended fix, suspected cause, or hoped-for verdict beyond them; there was nothing to set aside.

"Test disputed" means: every handed-over test passes except the nine `FixAll…Reported` tests listed under *Whole run*, which fail on a verifier setting, not on production behavior (proof there). No test file was changed.

## thoenissen/Reihitsu#1257

### Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Core/RewriteParseUtilities.cs` (new) | `ParsesAsWritten(originalRoot, rewrittenRoot, ct)`: re-parses the rewritten text with the original parse options (incrementally, one text change between common prefix and suffix) and compares tokens (kind, text, missing) and node structure with the rewritten tree; trivia is not compared. The one shared check of lead decision 3 | RH3001-007, RH3005-008, RH3101-005, RH3105-005 (and RH3005-007, RH3201-006 below) |
| `RH3001NotOperatorShouldNotBeUsedCodeFixProvider.cs` | Replacement built by `BuildReplacement` (unchanged shape, with its parentheses); registration requires `ParsesAsWritten` on it | RH3001-007 |
| `RH3005UseReadableConditionsCodeFixProvider.cs` | Swap built as nodes (`GetMirroredKinds`, `BuildSwappedExpression`) instead of parsed from text, same output text; registration requires `ParsesAsWritten` | RH3005-008 |
| `RH3101DoNotPrefixCallsWithBaseUnlessLocalImplementationExistsCodeFixProvider.cs` | `BuildReplacement` + `CanRewrite`, which requires `ParsesAsWritten` (generic and cast readings) | RH3101-005 |
| `RH3105DoNotPrefixLocalMembersWithThisCodeFixProvider.cs` | Registration requires `ParsesAsWritten` | RH3105-005 |

`SyntaxNode.IsEquivalentTo` was tried first and rejected: it reports two token-identical trees as different when a block contains `#if` directives (seen on `DirectivesInsideTheParenthesizedOperandDoNotWithholdTheFix`), so the check compares tokens and structure itself.

## thoenissen/Reihitsu#1279

### Changes

| File | What changed | Requirement |
|---|---|---|
| `RH3004UseLambdaSyntaxCodeFixProvider.cs` | `IsOperandOfCastOrBinaryOperator` withholds the fix when the anonymous method's parent is a `CastExpressionSyntax` or `BinaryExpressionSyntax` | RH3004-005 |
| same | The lambda is annotated and formatted with `ReihitsuFormatter.FormatNodeInDocumentAsync` (FMT-PIPE-009, FC-030); afterwards `RestoreDocumentationCommentsAsync` writes the block's documentation comments back as written | RH3004-003, RH3004-006 |

The restore step exists because the formatter's documentation phase rewrites a `///` comment inside the block (`DocumentationCommentedAnonymousMethodKeepsItsBody`: one-line `<summary>` split into three lines), while RH3004-003 keeps the statements unchanged. See *Architecture findings*.

## thoenissen/Reihitsu#1281

### Changes

| File | What changed | Requirement |
|---|---|---|
| `RH3005UseReadableConditionsCodeFixProvider.cs` | Building the swap as nodes makes the regrouping visible to `ParsesAsWritten` (`1 == 1 == b` re-parses as `(b == 1) == 1`) | RH3005-007 |
| `RH3101…CodeFixProvider.cs` | `BindsToOverridableMember`: withheld when the bound symbol (or any candidate) is virtual, abstract, or a non-sealed override and the containing type of the enclosing symbol is not sealed | RH3101-007 |
| `RH3106UnnecessaryDelegateParenthesesShouldBeRemovedAnalyzer.cs` | `TakesNaturalType` (not reported): `var` initializer; cast to a non-delegate type; converted type not a delegate type, unless it is an argument whose call has competing candidates (`HasCompetingCandidates`: generic method, several constructors, member group > 1, several indexers, or no bound symbol) | RH3106-001, RH3106-003 |
| `RH3106…CodeFixProvider.cs` | `KeepsMeaningAsync`: when an `ArgumentSyntax` encloses the anonymous method (up to the enclosing statement or member), the document is forked with the list removed; the fix is withheld unless the anonymous method's converted type and every enclosing invocation, object creation, constructor initializer, primary constructor base type, and element access bind to the same symbol (compared by display string, since the fork is another compilation) | RH3106-002 |
| `RH3201CommentsMustContainTextCodeFixProvider.cs` | `KeepsTheTokensApart`: removes every single-line and multi-line comment between the tokens on both sides of the comment and requires `ParsesAsWritten`; a comment before the first or behind the last token, or inside structured trivia, is not checked | RH3201-006 |
| same | `ApplyCodeFixAsync`: when code precedes the comment on its line and code follows it directly, only the comment is removed | RH3201-007 |
| `EmptyParenthesesAnalyzerBase{TNode}.cs`, `RH3107…Analyzer.cs` | `HasUnnecessaryParentheses` takes the semantic model and cancellation token (RH3106 needs them; RH3107 ignores them) | RH3106-003 |

## thoenissen/Reihitsu#1282

### Changes

| File | What changed | Requirement |
|---|---|---|
| `RH3101…CodeFixProvider.cs` | `CanRewrite` withholds a member access with a comment or directive in `TextSpan.FromBounds(Expression.Span.End, Name.SpanStart)` (`SyntaxNodeUtilities.SpanContainsCommentOrDirective`); element access unchanged | RH3101-006 |
| `RH3105…CodeFixProvider.cs` | Same span check | RH3105-006 |
| `RH3104DoNotUseDefaultValueTypeConstructorCodeFixProvider.cs` | Withheld when `SyntaxNodeUtilities.InteriorContainsCommentOrDirective(creation)` | RH3104-006 |
| `src/Reihitsu.Analyzer.CodeFixes/Base/EmptyParenthesesCodeFixProviderBase{TNode}.cs` (new) | Shared empty-list removal (lead decision 7): withheld when the list's interior holds a comment or directive; the trivia in front of `(` and behind `)` becomes the trailing trivia of the preceding token, with each whitespace run on a line collapsed to one space, indentation after a line break kept, no whitespace before a line break, and at the end one space (RH3106) or none (RH3107) | SHARED-PARENS-002, SHARED-PARENS-003 |
| `RH3106…CodeFixProvider.cs`, `RH3107…CodeFixProvider.cs` | Rewritten as thin subclasses of the base (`GetList`, `RemoveList`, `KeepsSpaceInFrontOfFollowingToken`); RH3106 adds `KeepsMeaningAsync` (#1281) | SHARED-PARENS-002, SHARED-PARENS-003 |

## thoenissen/Reihitsu#1417

### Changes

| File | What changed | Requirement |
|---|---|---|
| `RH3103UseShorthandForNullableTypesAnalyzer.cs` | `ShouldSkip` walks up through qualified names, alias-qualified names, and member accesses whose `Name` is the current node, then skips a `nameof` argument, the receiver of a member access, a `using static` target, and a using-alias target below C# 12; `OnGenericName` skips an `OmittedTypeArgumentSyntax` (`typeof(Nullable<>)`) | RH3103-001, RH3103-007 |

The code fix's `MemberAccessExpressionSyntax { Name: genericName } => null` arm (`RH3103UseShorthandForNullableTypesCodeFixProvider.cs:181`) is no longer reached by compiling code, since the analyzer no longer reports those positions; I left it as a defensive guard.

## thoenissen/Reihitsu#1422

### Changes

| File | What changed | Requirement |
|---|---|---|
| `RH3104DoNotUseDefaultValueTypeConstructorAnalyzer.cs` | `StandsAsStatement`: expression statement; `for` initializer or incrementor; arrow body whose declared symbol is a method with `HasStatementBody`; expression-bodied lambda whose symbol has `HasStatementBody`. `HasStatementBody`: `ReturnsVoid`, or `IsAsync` with a non-generic return type (covers constructors, finalizers, set/init/add/remove accessors, `Task`, `ValueTask`, `async void`) | RH3104-001, RH3104-005 |
| `RH5417FunctionAndAccessorBodyBracesMustNotShareLineAnalyzer.cs` | `IsInsideSingleLineInterpolation`: below C# 11, any ancestor interpolation of an interpolated string whose start token is `InterpolatedStringStartToken` (`$"`) skips the report | RH5417-001, RH5417-005 |

## Whole run

### Policy owners

- Rewrite re-parse: `rg -n "WithChangedText|ParseSyntaxTree|IsEquivalentTo\(|ParsesAsWritten|WouldMergeIntoOneToken" src --glob '!**/bin/**' --glob '!**/obj/**' --glob '!*Test*/**'`. Owners outside the formatter: `RewriteParseUtilities.ParsesAsWritten` (new, the run's five fixes) and `TokenSeparationUtilities.WouldMergeIntoOneToken` (token-pair lexing, used by RH3002's code fix and formatter utilities). RH3002's analyzer keeps its private generic-name scan (`RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.cs`, `ScanTypeArgumentList` line 215, `WouldDetachTypeArgumentList` line 515), which is out of this run by decision 3 (follow-up already listed). RH3201 uses `ParsesAsWritten` and not `WouldMergeIntoOneToken`, following decision 3; the reparse also covers the multi-line and adjacent-comment members at once.
- Empty-list removal: `rg -n "SyntaxRemoveOptions.KeepExteriorTrivia" src/Reihitsu.Analyzer.CodeFixes/Rules/Clarity` finds no RH3106/RH3107 copy left; the base class is the one owner.
- Comment or directive in a span: `rg -n "SpanContainsCommentOrDirective|InteriorContainsCommentOrDirective" src/Reihitsu.Core/SyntaxNodeUtilities.cs` — reused, no copy written.
- Language-version gate: `rg -n "LanguageVersion: [<>]" src/Reihitsu.Analyzer` — same pattern as `RH7003`; no shared helper exists.

### Tests run

All commands in Debug (`--configuration Debug`).

| Command | Result |
|---|---|
| `scripts/test.sh --project analyzer --filter "<the eleven classes of 04-test-implementer.md>"` | Failed 9, Passed 367, Total 376 — the nine tests below |
| same, with the nine tests temporarily given `MarkupMode.Allow` for `FixedState` and `BatchFixedState` (reverted with `git checkout -- src/Reihitsu.Analyzer.Test`, never committed) | Passed 376, Failed 0 |
| `scripts/test.sh --project analyzer` | Failed 9, Passed 6821, Total 6830 — the same nine |
| `scripts/test.sh --project core` | Passed 607 |
| `scripts/test.sh --project architecture` | Failed 1 (`UsesListsExactlyTheReferencedRequirements`), Passed 114 — fails identically on 15a055b8 without my changes (stash run), see below |
| `scripts/build.sh --configuration Debug` | 0 warnings, 0 errors |
| `scripts/apply-fix.sh RH3201 <fixture>` with `/**/class C … }/**/` | fixed, no exception (edge-token guard) |

`scripts/format.sh` was run over every changed file.

### Disputed tests or scope

1. **Nine `FixAll…Reported` tests cannot pass as written.** Each puts the withheld diagnostic as markup into `fixedCode` and calls `Verify(testCode, fixedCode)` without configuration. The verifier's default fixed-state markup handling ignores markup diagnostics whose ID the provider can fix, so it expects 0 diagnostics while the analyzer correctly reports 1 (or 2): "Context: Diagnostics of fixed state — Mismatch between number of diagnostics returned, expected "0" actual "1"". The repository's existing tests of this shape set `test.FixedState.MarkupHandling = MarkupMode.Allow; test.BatchFixedState.MarkupHandling = MarkupMode.Allow;` (e.g. `RH8107VoidReturnValueMustNotBeDocumentedAnalyzerTests.cs:863`, `RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzerTests.cs:463`). With exactly that configuration added, all nine pass against this commit, so their expected documents are right. Tests:
   - `RH3001NotOperatorShouldNotBeUsedAnalyzerTests.FixAllLeavesANegationThatWouldReadAsAGenericNameReported`
   - `RH3004UseLambdaSyntaxAnalyzerTests.FixAllLeavesABinaryOperandReported`
   - `RH3005UseReadableConditionsAnalyzerTests.FixAllLeavesAComparisonThatWouldReadAsAGenericNameReported`
   - `RH3101DoNotPrefixCallsWithBaseUnlessLocalImplementationExistsAnalyzerTests.FixAllLeavesAnAccessThatWouldReadAsACastReported`
   - `RH3104DoNotUseDefaultValueTypeConstructorAnalyzerTests.FixAllLeavesACreationWithACommentInsideReported`
   - `RH3105DoNotPrefixLocalMembersWithThisAnalyzerTests.FixAllLeavesAnAccessThatWouldReadAsAGenericNameReported`
   - `RH3106UnnecessaryDelegateParenthesesShouldBeRemovedAnalyzerTests.FixAllLeavesAParameterListWithACommentInsideReported`
   - `RH3107UnnecessaryAttributeConstructorParenthesesShouldBeRemovedAnalyzerTests.FixAllLeavesAnArgumentListWithACommentInsideReported`
   - `RH3201CommentsMustContainTextAnalyzerTests.FixAllLeavesAdjacentCommentsBetweenTwoTokensReported`

   Owner: test implementer (test configuration only; no expected output changes).
2. **`UsesListsExactlyTheReferencedRequirements` (architecture) fails on the spec diff, not on code.** The Uses rows of RH3001, RH3004, RH3005, RH3101, RH3104, RH3105, RH3106 miss SHARED-FIX-003 / SHARED-FIX-007 / SHARED-DIAG-011, which the new requirements reference, and RH3106/RH3107 list SHARED-PARENS-003 without referencing it outside the non-goals. Same failure on 15a055b8. Owner: spec author.
3. **Scope.** Two new shared components: `RewriteParseUtilities` (public, `Reihitsu.Core`, five consumers; decision 3) and `EmptyParenthesesCodeFixProviderBase<TNode>` (two consumers; decision 7). `RewriteParseUtilities` has no direct test in `Reihitsu.Core.Test`; it is covered only through the analyzer tests. The abstract `HasUnnecessaryParentheses` of `EmptyParenthesesAnalyzerBase<TNode>` changed its signature (internal API, both subclasses updated).

### Architecture findings

**RH3004's fix and the formatter's documentation phase.** RH3004-006 asks the fix to lay out the lambda as the formatter does, and RH3004-003 keeps the block's statements unchanged. `ReihitsuFormatter.FormatNodeInDocumentAsync`, the only entry point a fix may use (FMT-PIPE-009), runs the documentation phase too, which rewrites the text of a `///` comment inside the block (`DocumentationCommentedAnonymousMethodKeepsItsBody`). To pass the test, the fix writes the original documentation comment trivia back after formatting (`RestoreDocumentationCommentsAsync`), so it undoes part of what the formatter decided. It does not compute a column. Its limit: a documentation comment spanning several `///` lines carries the indentation of its continuation lines inside the trivia. Where the fix moves the block (the `{`-on-the-next-line and one-line shapes), those continuation lines would keep their old columns. No test covers that input. Structural alternative for the lead: a layout-only option on the node-level entry point.

### Follow-up candidates

- `RewriteParseUtilities.ParsesAsWritten` needs direct unit tests in `Reihitsu.Core.Test` (token merge, generic reading, cast reading, directive-bearing block, unchanged tree).
- RH3004: a multi-line `///` documentation comment inside a block that the fix moves (see *Architecture findings*).
- RH3103 code fix: the `MemberAccessExpressionSyntax` arm at `RH3103UseShorthandForNullableTypesCodeFixProvider.cs:181` is now dead for compiling code; remove it or keep it as a guard.

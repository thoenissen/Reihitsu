Role: implementer
Commit: 9ba0bc3 (revision 3, repair round 2; revision 2 was 780162b, revision 1 2dea131)
Verdict: #948: tests pass; #949: tests pass; #954: tests pass; #1390: tests pass; #888: tests pass

My prompt carried no intended fix, suspected cause, or hoped-for verdict. The skeptic's mechanism readings (`02-skeptic.md`) were treated as claims: each one named below was checked by running the code (instrumented trace or a failing test) before it was used. Every handed-over test assertion, input, and expected output is unchanged. No file of a concurrent run was touched. The RH3202 code-fix provider sits in R7's RH3xxx area, which the maintainer released to this run (00-lead.md, Escalation 1, Q1).

`scripts/format.sh` was run over every changed production file; `scripts/lane-check.sh bug-fix` reports "confirmed — claimed bug-fix, diff needs bug-fix".

## thoenissen/Reihitsu#948

### Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/SwitchCaseBraces/Rewriter/SwitchCaseBraceRewriter.cs` | New whole-switch guard `HasCrossSectionLocalUse`, checked in `VisitSwitchStatement` next to `HasCrossSectionGotoTarget`. `GetDirectlyDeclaredLocalNames` collects the names a section declares directly: local declarations (`const` and initializer expression variables included), local functions, and `out var` / pattern / deconstruction designations in expression statements, `return`, `throw`, `yield return`, an `if` condition, a nested switch's governing expression, and a `lock` expression, each also behind labels. `GetDesignatedNames` skips lambda and anonymous-method bodies and switch-expression arms. A use is any simple name in another section, labels included. Loops, `using`, `fixed`, blocks, the embedded statements and `else` of an `if`, and nested switch sections declare nothing. | FMT-SWITCH-014, FMT-PIPE-003 |

The guard errs toward leaving the switch alone. A simple name behind `.` (unspecified by FMT-SWITCH-014) and a lambda parameter that shadows the local both count as a use. Either one leaves the switch unbraced, and the output still compiles.

### Policy owners

- `rg -n "SingleVariableDesignationSyntax|DeclarationExpressionSyntax" src --glob '!*Test*'`: only RH4115/RH4117/RH4125/RH4126 naming analyzers, `SingleLetterIdentifierUtilities`, and the casing fix base, none of which decides C# scope. The formatter and Core have no existing owner, so the predicate lives in the rewriter next to the goto guard it mirrors.

### Tests run

- `scripts/test.sh --configuration Debug --project formatter --filter "FullyQualifiedName~SwitchSectionSharedLocalTests|FullyQualifiedName~SwitchCaseBrace"`: every `SwitchSectionSharedLocalTests` test passes; the only failures at that point were the five #949 tests, fixed afterwards.

### Disputed tests or scope

_None._

## thoenissen/Reihitsu#949

### Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/SwitchCaseBraces/Rewriter/SwitchCaseBraceRewriter.cs` | New `BuildTerminalBreakLeadingTrivia`, used for the terminal `break` that moves behind the new block, instead of `BuildLeadingTrivia`. It keeps the blank lines in front of each comment as the blank-line phase placed them: above the first comment and between two comments. Blank lines that no comment follows are dropped, as before. `BuildLeadingTrivia` still rebuilds the first statement moved into the block, unchanged. | FMT-SWITCH-007, FMT-BLANK-011, FMT-PIPE-004 |

Mechanism, checked against the failing output: in the same pass, the blank-line phase had already inserted the line above the comment. Brace insertion then rebuilt the break's trivia from comments alone. The policy owner of that blank line stays `BlankLinePhase`; the rewriter now only preserves its decision.

### Policy owners

- The blank-line decision is owned by `BlankLinePhase` (FMT-BLANK-011); no second copy was added. `rg -n "BuildLeadingTrivia" src/Reihitsu.Formatter`: the rewriter is its only user.

### Tests run

- `scripts/test.sh --configuration Debug --project formatter --filter "FullyQualifiedName~SwitchCaseBrace|FullyQualifiedName~Switch"`: 141 passed, 1 failed (`EmptyRegionAfterSwitchStatementBraceGetsNoBlankLineAfterTheBrace`, a #954 test, fixed afterwards).
- RH5020 parity tests: in the full analyzer run below, both pass.

### Disputed tests or scope

_None._

## thoenissen/Reihitsu#954

### Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/LineBreaks/Utilities/TokenGapNormalizer.cs` | `TryNormalizeGapAfterLeadingStructuredTrivia`: a gap that ends in a region directive is now left alone when the token in front of the gap is an opening brace, as well as behind a documentation comment, which was already the case. Summary updated. | FMT-BLANK-007, SHARED-BLANK-005 (mechanism a) |
| `src/Reihitsu.Formatter/Pipeline/BlankLines/Utilities/BlankLineEditor.cs` | New overload `IsExemptFromPrecedingBlankLineBeforeRegionDirective(previousToken, leadingTrivia, directiveIndex)`. The opening-brace exemption holds only when no comment and no directive other than a conditional-compilation directive stands between the brace and the region directive. `EnsureBlankLineBeforeFirstDirective` applies the exemption itself, where the directive index is known. | SHARED-BLANK-005, FMT-BLANK-013 (mechanism b) |
| `src/Reihitsu.Formatter/Pipeline/BlankLines/Rewriter/BlankLineRegionDirectiveRewriter.cs`, `.../BlankLineTriviaBoundaryRewriter.cs` | Their outer exemption guards for the leading-trivia case are removed: the exemption now lives in `EnsureBlankLineBeforeFirstDirective`, its only caller pair. The trailing-trivia `#endregion` branch of the boundary rewriter keeps the old token-only predicate. | same |

Mechanism (a), established with temporary instrumentation (removed before the commit). `TokenLocator.TryGetPreviousToken` resolves the `}` gap's "previous token" to the `#endregion` directive's own `EndOfDirectiveToken`. The regular normalization therefore prepends a line break in front of the gap's first content and tries to strip the line break from that directive token, but `{` keeps its own. The result is a blank line after `{` in every pass.

Scope decisions, kept within the maintainer's approvals (00-lead.md: `TokenGapNormalizer` limited to the blank line after `{`; Q2: comment first after `{`):

- The `TokenGapNormalizer` change applies only behind an opening brace. A wider form, which leaves every region-ending gap alone, also changed `ReihitsuFormatterLineEndingTests.LineBreaksEndingPragmaAndRegionDirectivesAreRewritten` (see the follow-up candidates). It was narrowed to stay inside the approval.
- Conditional-compilation directives (`#if`, `#elif`, `#else`, `#endif`) and disabled text keep the brace exemption, so `{` / `#if DEBUG` / `#region` is formatted as before; `DisabledRegionLayoutTests.RegionInsideActiveConditionalIsStillFormatted` pins that (see the follow-up candidates). The start of the file (no previous token) keeps its exemption unchanged; this is the skeptic's start-of-file follow-up.

### Policy owners

- `rg -n "IsExemptFromPrecedingBlankLineBeforeRegionDirective|EnsureBlankLineBeforeFirstDirective" src --glob '!*Test*'`: owner `BlankLineEditor`, consumers `BlankLineRegionDirectiveRewriter` and `BlankLineTriviaBoundaryRewriter`. The analyzer-side policy is Core `RegionDirectiveBlankLineUtilities.IsMissingRequiredBlankLineBefore`, which checks the line directly above; it is unchanged, and the formatter now matches it for comments and non-conditional directives.
- `rg -n "HasOwnLinePlacementOwner|TryNormalizeGapAfterLeadingStructuredTrivia" src/Reihitsu.Formatter`: the decision is private to `TokenGapNormalizer`.

### Tests run

- `scripts/test.sh --configuration Debug --project formatter --filter "FullyQualifiedName~Region|FullyQualifiedName~BlankLine|FullyQualifiedName~LineEnding|FullyQualifiedName~TokenGap|FullyQualifiedName~Directive"`: 701 passed, 1 failed (`BlankLineBetweenTwoCommentsAfterArrowIsKept`, a #1390 test, fixed afterwards).
- RH5022 / RH5031 parity tests: in the full analyzer run below, both pass.

### Disputed tests or scope

_None._

## thoenissen/Reihitsu#1390

### Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/StructuralTransforms/Utilities/ExpressionBodiedTransformUtilities.cs` | New `RequiresAsyncExpressionBodyPreservation(SyntaxNode)`, the single owner of the FMT-STRUCT-025 predicate: method or local function, `async`, not `void`, and the return type not written as `Task` / `ValueTask` / `Task<…>` / `ValueTask<…>` (simple, qualified, or alias-qualified, with or without `?`). `UsesExpressionStatement` now reads the same `GetWrittenTaskTypeName`, which looks through `?`, so `async Task?` gets an expression statement. | FMT-STRUCT-025, FMT-STRUCT-004 |
| `.../StructuralTransforms/Rewriter/ExpressionBodiedMethodTransform.cs`, `.../ExpressionBodiedLocalFunctionTransform.cs` | Refuse the conversion when the predicate holds; class summaries updated. | FMT-STRUCT-025, FMT-PIPE-003 |
| `src/Reihitsu.Analyzer.CodeFixes/Base/ExpressionBodyToBlockCodeFixProviderBase{TNode}.cs` (RH3208 and the others) and `src/Reihitsu.Analyzer.CodeFixes/Rules/Clarity/RH3202ExpressionStyleMethodsShouldNotBeUsedCodeFixProvider.cs` | No code action is registered when the predicate holds. RH3202's provider does not derive from the base; it has its own copy of the registration loop. It got the same one-line guard instead of a rebase onto the base, because a rebase would change the base type of a public class. Analyzers are unchanged: the member stays reported. | SHARED-BODY-004 |
| `.../StructuralTransforms/Utilities/ExpressionBodyToBlockConverter.cs` | `CreateBlock`: comments on lines of their own in the arrow's leading trivia, and between the arrow and the expression, become the created statement's leading comment lines, in source order. Indentation is dropped, the blank lines between two comments are kept, and the open brace's line is ended first unless its trailing trivia already ends it. A trivia list holding anything else (directive, documentation comment), or an arrow with a comment glued on its own line, keeps today's behavior. The arrow-line comment (#963, `{// why`) is untouched. New `CreateReturnStatement` (replaces `RemoveIndentationOnlyLeadingTrivia`): with no line break in front of the expression, the indentation is dropped and comments on the expression's line move behind `return` (`return /* c */ 42;`), where the parser places them. | FMT-STRUCT-006, FMT-PIPE-013, SHARED-BODY-003 |
| `src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/LineBreakInitializerRewriter.cs` | New `VisitWithExpression`: it places the initializer's `{` from the `with` expression, which owns both tokens, as `VisitObjectCreationExpression` already does. `TokenGapNormalizer` was not changed for this part. | FMT-BREAK-021, FMT-PIPE-004 |

### Policy owners

- `rg -n "UsesExpressionStatement|BlocksRewrite|CanRewrite" src --glob '!*Test*'`: the statement-form and FMT-STRUCT-025 decisions are owned by `ExpressionBodiedTransformUtilities`, read by the method, local-function, and operator transforms, the code-fix base, and the RH3202 provider. Core `ExpressionBodyRewriteUtilities.BlocksRewrite` (shared with the analyzers) is unchanged on purpose, because SHARED-BODY-004 keeps the report.
- `rg -n "class ExpressionBodyToBlockConverter|CreateBlock\(" src/Reihitsu.Formatter`: `CreateBlock` is the one owner for all six expression-bodied transforms.
- `rg -n "public override SyntaxNode Visit" src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/LineBreakInitializerRewriter.cs`: initializer braces are placed by the existing creation visits; the new visit mirrors them.

### Tests run

- `scripts/test.sh --configuration Debug --project formatter --filter "FullyQualifiedName~FormatterProducesNoChangesOnSourceFiles|FullyQualifiedName~ExpressionBod|FullyQualifiedName~Structural"`: 492 passed, 1 failed (`BlockCommentLeadingNextLineExpressionKeepsOwnLine`, disputed below).
- `scripts/test.sh --configuration Debug --project formatter --filter "FullyQualifiedName~Structural|FullyQualifiedName~ExpressionBod|FullyQualifiedName~Initializer|FullyQualifiedName~With"`: 947 passed, 1 failed (the same disputed test).
- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH3202|FullyQualifiedName~RH3208|FullyQualifiedName~RH3203|FullyQualifiedName~RH3205|FullyQualifiedName~RH3206|FullyQualifiedName~RH3207"`: 115 passed, 1 failed (`VerifyFixLeavesAsyncMethodReturningTaskAliasNextToConvertedMethods`, disputed below).

### Disputed tests or scope

1. **`src/Reihitsu.Formatter.Test/Regression/Structural/ExpressionBodyNextLineExpressionTests.cs` `BlockCommentLeadingNextLineExpressionKeepsOwnLine`** (existing test, no requirement tag).
   - What it asserts: `int M() =>` / `/* c */ 42;` formats to `return` / blank line / `/* c */ 42;`.
   - What the spec says: FMT-STRUCT-006, as clarified by the maintainer's Q3 answer, requires `return /* c */ 42;`.
   - Why they disagree: the new test `ExpressionBodyArrowCommentTests.CommentInFrontOfExpressionOnItsLineJoinsTheReturnKeyword` has the same input and asserts the spec's output, so no implementation can pass both. The old test pins the pre-fix output; the test implementer's correction list (three tests) missed it. It fails now with actual = the spec output.
2. **`src/Reihitsu.Analyzer.Test/Formatting/RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzerTests.cs` `VerifyFixLeavesAsyncMethodReturningTaskAliasNextToConvertedMethods`.**
   - What it asserts: the fixed code is the two converted members plus the kept `KeptAsync`, still marked `{|RH3202:…|}`.
   - What fails: the fixed text matches exactly; only "Diagnostics of fixed state" fails ("expected 0, actual 1", the kept diagnostic at 6:32–6:51, the marked span).
   - Why: the test does not set `FixedState.MarkupHandling` / `BatchFixedState.MarkupHandling` to `MarkupMode.Allow`. Under the testing SDK's default, the fixed-state markup of a fixable diagnostic is not counted. Other tests that keep a diagnostic in fixed code set it (`RH4107ProtectedFieldCasingAnalyzerTests`, `RH4109PublicFieldCasingAnalyzerTests`).
   - Evidence: a temporary copy of this test with only those two settings added passed on 2dea131. The copy was deleted and never committed.
   - SHARED-BODY-004 and the behavior agree; the test's verifier configuration does not.

Both are test defects; the production behavior matches the specs. They go back to the lead for routing (escalation 4 only if the lead reads either as a spec change; neither asks for one).

## thoenissen/Reihitsu#888

### Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/ChainLineBreakRewriter.cs` | `EnsureLineBreak`: when the gap already holds a line break but the operator stands behind the closing line of a delimited comment that spans lines, a line break is appended to the operator's leading trivia, behind the comment (trailing whitespace removed). New helper `IsBehindMultiLineCommentTail` scans the operator's leading trivia back to the last line break, for a `/* */` or `/** */` comment whose text holds a line break. A one-line `/* x */` in front of the link is not affected (SHARED-COMMENT-011). | FMT-BREAK-019, SHARED-COMMENT-012 |

### Policy owners

- `rg -n "IsMultiLineComment|ContainsLineBreak|MultiLineCommentTrivia" src/Reihitsu.Core src/Reihitsu.Formatter --glob '!*Test*'`: no owner of the multi-line comment tail test.
  - Core `TokenGapAnalysis.StartsLineAtLeadingTriviaIndex` treats any comment in front of a trivia as line content, a one-line `/* x */` included, so reusing it would break `LinkBehindSingleLineCommentOnItsOwnLineIsUnchanged`.
  - Core `FluentChainLink.StartsLine` / `FluentChain.IsWrapped` are shared with RH5112/RH5201/RH5048 and stay unchanged, as the skeptic's surface table asks.

### Tests run

- `scripts/test.sh --configuration Debug --project formatter --filter "FullyQualifiedName~Chain"`: 281 passed, 0 failed.
- RH5201 code-fix (LF and CRLF) and formatter-parity tests: in the full analyzer run below, all pass.

### Disputed tests or scope

_None._

## Whole run

### Tests run

- `scripts/test.sh --configuration Debug --project formatter`, at the final formatter code before `format.sh` reflowed one file's layout: 2776 passed, 2 failed.
  - `BlockCommentLeadingNextLineExpressionKeepsOwnLine` is disputed (#1390).
  - `FormatterProducesNoChangesOnSourceFiles` flagged only the layout of my new lines in `ExpressionBodyToBlockConverter.cs`. After `scripts/format.sh` it passes (focused re-run above).
- `scripts/test.sh --configuration Debug --project analyzer`: 6063 passed, 3 failed, all RH3202, run before the RH3202 provider guard. After the guard, the focused RH32xx run leaves only the disputed test. Every RH5020 / RH5022 / RH5031 / RH5201 parity and code-fix test and every RH3208 test passes.
- `scripts/test.sh --configuration Debug --project architecture`: 84 passed, 0 failed.
- `scripts/test.sh --configuration Debug --project cli`: 250 passed, 0 failed.
- `scripts/build.sh --configuration Debug`: no warnings, no errors. The Release build is the cleaner's.

### Scope

- Production paths the handovers named: `SwitchCaseBraceRewriter`, `TokenGapNormalizer`, `BlankLineEditor` and its two consumers, `ExpressionBodiedTransformUtilities`, `ExpressionBodyToBlockConverter`, `LineBreakInitializerRewriter`, `ExpressionBodyToBlockCodeFixProviderBase`, and `ChainLineBreakRewriter`.
- Added beyond those:
  - the two transform call sites (`ExpressionBodiedMethodTransform`, `ExpressionBodiedLocalFunctionTransform`);
  - the RH3202 provider, whose own registration loop bypasses the base.
- That is 13 files against 10 named, well under double. No public API, no dependency, and no Core helper was changed.

### Follow-up candidates

- **FMT-BLANK-016 violation in a region gap.** `src/Reihitsu.Formatter.Test/Unit/ReihitsuFormatterLineEndingTests.cs` `LineBreaksEndingPragmaAndRegionDirectivesAreRewritten` pins a blank line that the formatter inserts directly in front of `#pragma warning restore CS0169`, which stands between `int x;` and `#endregion` in front of `}`. The input has none there. FMT-BLANK-016 forbids inserting a blank line directly in front of a directive. The cause is the #954 mechanism (a): the "previous token" of `}` is the `#endregion` directive's end token. Fixing it needs `TokenGapNormalizer` beyond the approved "after `{`" scope and a corrected expected value.
- **Conditional directive directly above `#region` after `{`.** `src/Reihitsu.Formatter.Test/Regression/Regions/DisabledRegionLayoutTests.cs` `RegionInsideActiveConditionalIsStillFormatted` pins `{` / `#if DEBUG` / `#region Test` with no blank line in front of `#region`. Read literally, SHARED-BLANK-005 asks for one there: the line above is neither blank nor the brace line. By reading `RegionDirectiveBlankLineUtilities.IsMissingRequiredBlankLineBefore`, RH5031 would report it; I did not run RH5031 on it. Conditional directives kept the brace exemption in this run so the test stays green; the maintainer decides.
- **Start of file.** A comment above `#region` at the start of the file keeps the exemption (skeptic's follow-up), unchanged.
- **RH3202 provider duplication.** `RH3202ExpressionStyleMethodsShouldNotBeUsedCodeFixProvider` duplicates `ExpressionBodyToBlockCodeFixProviderBase<MethodDeclarationSyntax>` instead of deriving from it (SHARED-BODY's code owner). Rebasing it would remove the second guard call; it changes a public class's base type, so it belongs to R7 or a separate issue.
- The skeptic's and the test implementer's follow-ups stand unchanged: RH7501 fix output, FMT-PIPE-013 for the first statement moved into a braced section, the nested `Inner = { … }` initializer, and `RH5201.md` `Uses`.

## Revision 2 — repair round 1

Commit: 780162b. Items A1, A2, A4, A5 of `07-auditor.md`, as routed in `00-lead.md` ("Repair round 1", and the maintainer's answers A4 → B and A5 → A). The specs are at `01-spec-author.md` revision 5 and the tests at `03-test-implementer.md` revisions 3–4. No test assertion, input, or expected output was changed. Revision 1's two disputed #1390 tests were corrected by the test implementer (5a6472f) and pass. `scripts/format.sh` was run over the changed files.

### thoenissen/Reihitsu#948 — A2

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/SwitchCaseBraces/Rewriter/SwitchCaseBraceRewriter.cs` | New `IsBoundInsideSection`. A simple name in another section is not a use when a declaration inside that section, enclosing the name, binds it: a parameter of an enclosing lambda, anonymous method, or local function; a variable that an enclosing lambda's or local function's expression body designates; or a local that an enclosing block declares directly. `GetDirectlyDeclaredLocalNames` now takes a statement list, so blocks reuse the same dispatch. `GetDesignatedNames` stops at `QueryBodySyntax`, so a pattern variable in a query clause behind the first `from` expression does not count. | FMT-SWITCH-014 (revision 5: query clauses; "used only in the section that declares it") |

The narrowing stays on the safe side. Every binding it accepts is one C# resolves to the inner declaration. An inner local that shadows the switch-scoped local outside a lambda is CS0136 anyway. A shadowing form it does not recognize (a `for`, `foreach`, `using`, or `catch` variable inside a lambda) still triggers the guard, so the switch stays unbraced and still compiles.

### thoenissen/Reihitsu#949 — A1

| File | What changed | Requirement |
|---|---|---|
| `SwitchCaseBraceRewriter.cs`, `BuildTerminalBreakLeadingTrivia` | No line break is appended behind a comment trivia that already ends with one (`BlankLineTriviaUtilities.EndsWithLineBreak`, a `///` comment); the line behind it counts as a new line. Summary updated. | FMT-SWITCH-007, FMT-BLANK-011, FMT-PIPE-004 |

`BuildLeadingTrivia` (the first statement moved into the block) has the same `///` hazard, per the auditor's hint. It was not changed, because it is outside the routed item.

### thoenissen/Reihitsu#954 — A4 (answer B: behavior unchanged)

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/BlankLines/Utilities/BlankLineEditor.cs` | Documentation comments only. The 3-argument `IsExemptFromPrecedingBlankLineBeforeRegionDirective` now states what the code does: the start of the file is exempt whatever trivia stands in front; behind an opening brace, whitespace, line breaks, conditional-compilation directives, and disabled text keep the exemption, even when such a directive is the line directly above; a comment or any other directive ends it. `EnsureBlankLineBeforeFirstDirective`'s summary names the same two exemptions. No code changed. | SHARED-BLANK-005 (behavior as on `origin/main` for conditional directives, per the maintainer's answer) |

### thoenissen/Reihitsu#1390 — A5 (answer A)

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/StructuralTransforms/Utilities/ExpressionBodyToBlockConverter.cs` | New `ArrowLineHoldsComment`: a comment in the arrow's trailing trivia, or in its leading trivia behind the last line break. When it holds one, `CreateBlock` moves no comment: the own-line comments stay in front of `{` and in front of the expression, as on `origin/main`. `CreateReturnStatement` then drops whitespace-only leading trivia only, as on `origin/main`, and does not join a `/* */` behind `return`. Summaries updated. | FMT-STRUCT-006 (revision 4), SHARED-BODY-003 |

The RH3202 fix and the other expression-body fixes reach the same `CreateBlock` through the formatter, so `VerifyFixKeepsCommentOrderWhenTheArrowLineCarriesAComment` passes without a code-fix change.

### Policy owners

- `rg -n "BuildTerminalBreakLeadingTrivia|BuildLeadingTrivia" src/Reihitsu.Formatter`: both live only in `SwitchCaseBraceRewriter`. `rg -n "static bool EndsWithLineBreak" src/Reihitsu.Formatter`: the one owner, `BlankLineTriviaUtilities`, is reused in the rewriter and in `ArrowLineHoldsComment`.
- `rg -n "IsExemptFromPrecedingBlankLineBeforeRegionDirective" src --glob '!*Test*'`: unchanged since revision 1.
- `IsBoundInsideSection` reuses `GetDirectlyDeclaredLocalNames` and `GetDesignatedNames`; no second scope policy was added.

### Tests run

- `scripts/test.sh --configuration Debug --project formatter --filter "FullyQualifiedName~SwitchSectionSharedLocalTests|FullyQualifiedName~TerminalBreakCommentTests|FullyQualifiedName~SwitchCaseBrace"`: 87 passed, 0 failed.
- `scripts/test.sh --configuration Debug --project formatter --filter "FullyQualifiedName~Structural|FullyQualifiedName~ExpressionBod|FullyQualifiedName~SwitchCaseBrace|FullyQualifiedName~SwitchSection|FullyQualifiedName~TerminalBreak"`: 600 passed, 0 failed.
- `scripts/test.sh --configuration Debug --project formatter`: 2793 passed, 0 failed.
- `scripts/test.sh --configuration Debug --project analyzer`: 6068 passed, 0 failed.

### Disputed tests or scope

_None._

### Follow-up candidates

- `BuildLeadingTrivia` (the first statement moved into a braced section) appends a line break behind a `///` comment and drops blank lines between comments (auditor hint; skeptic follow-up).
- Revision 1's follow-ups stand. The conditional-directive case under SHARED-BLANK-005 is now the maintainer's A4 follow-up.

## Revision 3 — repair round 2

Commit: 9ba0bc3. Findings A8 and A2 (narrowed) of `07-auditor.md` (the note after repair round 1), as routed in `00-lead.md` under "Repair round 2". Tests: `03-test-implementer.md` "Revision 5 — repair round 2". I changed no test assertion, input, or expected output. I took the auditor's suggestion as a suggestion only. I ran `scripts/format.sh` over the changed file (no change).

### thoenissen/Reihitsu#948 — A8, A2

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/SwitchCaseBraces/Rewriter/SwitchCaseBraceRewriter.cs` | `IsBoundInsideSection` now walks the ancestors from the name up to its section, together with the child each step comes from. The new `IsBoundByAncestor` lets a declaration bind the name only when that child lies inside the declaration's C# scope. | FMT-SWITCH-014, FMT-PIPE-003 |

Scopes `IsBoundByAncestor` recognizes:

- Parameters, and expression-body designations, of a lambda, anonymous method, or local function: only for a name in the body. Names in the function's attribute lists, parameter list (default values included), return type, and constraint clauses stay uses (A8).
- Locals that a block, or a section of a nested switch, declares directly.
- A `foreach` or deconstructing `foreach` variable, and a `using` or `fixed` variable: in the embedded statement.
- A `for` variable: in the condition, the incrementors, and the embedded statement.
- A `catch` variable: in the filter and the block.
- A `while` condition designation: in the condition and the embedded statement. A `do` condition designation: in the condition.
- A switch-expression arm designation: in that arm (A2).

A shadowing form not on this list still counts as a use, which leaves the switch unbraced, and the output still compiles. The `for` variable is not bound inside its own declaration, which takes the same conservative side. The summary lists each scope.

### Policy owners

- No new scope policy. `IsBoundByAncestor` reuses `GetDirectlyDeclaredLocalNames` (blocks and nested switch sections) and `GetDesignatedNames` (expression bodies, conditions, deconstruction variables, arm patterns), which stay the only owners in `SwitchCaseBraceRewriter`.
- `rg -n "SingleVariableDesignationSyntax|DeclarationExpressionSyntax" src --glob '!*Test*'` finds no other scope owner, unchanged since revision 1.

### Tests run

- `scripts/test.sh --configuration Debug --project formatter --filter "FullyQualifiedName~SwitchSectionSharedLocalTests|FullyQualifiedName~SwitchCaseBrace|FullyQualifiedName~TerminalBreak"`: 101 passed, 0 failed. This includes the 13 failing tests named in revision 5: A8 ×4, A2 ×9.
- `scripts/test.sh --configuration Debug --project formatter`: 2859 passed, 0 failed.
- `scripts/test.sh --configuration Debug --project analyzer`: 6133 passed, 0 failed.

### Disputed tests or scope

_None._

### Follow-up candidates

_None new._ The earlier revisions' follow-ups stand. `sdd/specs/formatter/structural-transforms.md` has an uncommitted change in the worktree that is not mine (A9, the spec author's); I did not stage it.
